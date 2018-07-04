namespace EffectiveHttpClientTest
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using EffectiveHttpClient;

    [TestClass]
    public class AutoRenewLeasableTest
    {
        [TestMethod]
        public void TestConstructorThrowsOnNullArguments()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new AutoRenewLeasable<IRenewable>(
                null as IBuildStrategy<IRenewable>,
                null as IRenewStrategy));
        }

        [TestMethod]
        public void TestBasicAcquireReleaseAndRenew()
        {
            var mockBuildStrategy = new Mock<IBuildStrategy<IRenewable>>();
            var renewStrategyMock = new Mock<IRenewStrategy>();
            var renewableMock = new Mock<IRenewable>();

            bool shouldRenew = false;
            mockBuildStrategy.Setup(x => x.Build()).Returns(() => renewableMock.Object);
            renewStrategyMock
                .Setup(x => x.ShallRenew(It.IsAny<IRenewable>()))
                .Returns((IRenewable x) => shouldRenew);

            var leasable = new AutoRenewLeasable<IRenewable>(mockBuildStrategy.Object, renewStrategyMock.Object);
            IRenewable ret = null;

            // acquire and release, this should trigger build
            ret = leasable.Acquire();
            Assert.AreSame(renewableMock.Object, ret);
            mockBuildStrategy.Verify(x => x.Build(), Times.Once);
            Assert.IsTrue(leasable.LeaseCount == 1);
            leasable.Release();
            renewStrategyMock.Verify(x => x.ShallRenew(It.Is((IRenewable y) => y == renewableMock.Object)), Times.Once);

            // mark object as should renew, and acquire/release again. Last release should destroy the object.
            shouldRenew = true;
            ret = leasable.Acquire();
            Assert.AreSame(renewableMock.Object, ret);
            mockBuildStrategy.Verify(x => x.Build(), Times.Once);
            Assert.IsTrue(leasable.LeaseCount == 1);
            leasable.Release();
            renewStrategyMock.Verify(x => x.ShallRenew(It.Is((IRenewable y) => y == renewableMock.Object)), Times.Exactly(2));

            // Now renew should happen and Build is called again
            ret = leasable.Acquire();
            mockBuildStrategy.Verify(x => x.Build(), Times.Exactly(2));
            Assert.IsTrue(leasable.LeaseCount == 1);

            // An embeded acquire/release doesn't trigger renew since there is active lease
            var ret2 = leasable.Acquire();
            mockBuildStrategy.Verify(x => x.Build(), Times.Exactly(2));
            Assert.IsTrue(leasable.LeaseCount == 2);
            leasable.Release();
            Assert.IsTrue(leasable.LeaseCount == 1);
            renewStrategyMock.Verify(x => x.ShallRenew(It.Is((IRenewable y) => y == renewableMock.Object)), Times.Exactly(2));

            // outer release triggers check
            leasable.Release();
            renewStrategyMock.Verify(x => x.ShallRenew(It.Is((IRenewable y) => y == renewableMock.Object)), Times.Exactly(3));
        }

        [TestMethod]
        public async Task TestMultiThread()
        {
            var mockBuildStrategy = new Mock<IBuildStrategy<IRenewable>>();
            var mockRenewPolicy = new Mock<IRenewStrategy>();
            var renewableMock = new Mock<IRenewable>();

            int renewCount = 0;
            mockBuildStrategy.Setup(x => x.Build()).Returns(() => renewableMock.Object);
            mockRenewPolicy
                .Setup(x => x.ShallRenew(It.IsAny<IRenewable>()))
                .Returns((IRenewable x) => ++ renewCount % 3 == 0);

            var leasable = new AutoRenewLeasable<IRenewable>(mockBuildStrategy.Object, mockRenewPolicy.Object);

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