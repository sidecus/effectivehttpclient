namespace EffectiveHttpClientTest
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using EffectiveHttpClient;

    public class RenewableMock : IRenewable
    {
        public DateTime CreationTime => DateTime.UtcNow;
        public int ErrorCount => 1;
        public void Dispose() {}
    }

    [TestClass]
    public class RenewableLeasableTest
    {
        [TestMethod]
        public void TestConstructorThrowsOnNullArguments()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new AutoRenewLeasable<RenewableMock>(
                null as IBuildStrategy<RenewableMock>,
                null as IRenewStrategy<RenewableMock>));
        }

        [TestMethod]
        public void TestBasicAcquireReleaseAndRenew()
        {
            var mockBuildStrategy = new Mock<IBuildStrategy<RenewableMock>>();
            var mockRenewPolicy = new Mock<IRenewStrategy<RenewableMock>>();

            var dataObj = new RenewableMock();
            bool shouldRenew = false;
            mockBuildStrategy.Setup(x => x.Build()).Returns(() => dataObj);
            mockRenewPolicy
                .Setup(x => x.ShallRenew(It.IsAny<RenewableMock>()))
                .Returns((RenewableMock x) => shouldRenew);

            var leasable = new AutoRenewLeasable<RenewableMock>(mockBuildStrategy.Object, mockRenewPolicy.Object);
            RenewableMock ret = null;

            // acquire and release, should return dataObj
            ret = leasable.Acquire();
            Assert.AreSame(dataObj, ret);
            mockBuildStrategy.Verify(x => x.Build(), Times.Once);
            Assert.IsTrue(leasable.LeaseCount == 1);
            leasable.Release();
            mockRenewPolicy.Verify(x => x.ShallRenew(It.Is((RenewableMock y) => y == dataObj)), Times.Once);

            // mark object as should renew, and acquire/release again
            shouldRenew = true;
            ret = leasable.Acquire();
            Assert.AreSame(dataObj, ret);
            mockBuildStrategy.Verify(x => x.Build(), Times.Once);
            Assert.IsTrue(leasable.LeaseCount == 1);
            leasable.Release();
            mockRenewPolicy.Verify(x => x.ShallRenew(It.Is((RenewableMock y) => y == dataObj)), Times.Exactly(2));

            // Now renew should happen
            ret = leasable.Acquire();
            mockBuildStrategy.Verify(x => x.Build(), Times.Exactly(2));
            Assert.IsTrue(leasable.LeaseCount == 1);

            // An embeded acquire/release doesn't trigger renew
            var ret2 = leasable.Acquire();
            mockBuildStrategy.Verify(x => x.Build(), Times.Exactly(2));
            Assert.IsTrue(leasable.LeaseCount == 2);
            leasable.Release();
            Assert.IsTrue(leasable.LeaseCount == 1);
            mockRenewPolicy.Verify(x => x.ShallRenew(It.Is((RenewableMock y) => y == dataObj)), Times.Exactly(2));

            // outer release triggers check
            leasable.Release();
            mockRenewPolicy.Verify(x => x.ShallRenew(It.Is((RenewableMock y) => y == dataObj)), Times.Exactly(3));
        }

        [TestMethod]
        public async Task TestMultiThread()
        {
            var mockBuildStrategy = new Mock<IBuildStrategy<RenewableMock>>();
            var mockRenewPolicy = new Mock<IRenewStrategy<RenewableMock>>();

            var dataObj = new RenewableMock();
            int renewCount = 0;
            mockBuildStrategy.Setup(x => x.Build()).Returns(() => dataObj);
            mockRenewPolicy
                .Setup(x => x.ShallRenew(It.IsAny<RenewableMock>()))
                .Returns((RenewableMock x) => ++ renewCount % 3 == 0);

            var leasable = new AutoRenewLeasable<RenewableMock>(mockBuildStrategy.Object, mockRenewPolicy.Object);

            // Multi thread acquire/release should not cause incorrect couter issues
            Action<int> execute = (int upper) =>
            {
                for (int i = 0; i < upper; i++)
                {
                    leasable.Acquire();
                    leasable.Release();
                }
            };
            await Task.WhenAll(
                Task.Run(() => execute(5)),
                Task.Run(() => execute(7)),
                Task.Run(() => execute(3)));
            Assert.IsTrue(leasable.LeaseCount == 0);
        }
    }
}