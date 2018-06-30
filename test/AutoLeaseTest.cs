namespace EffectiveHttpClientTest
{
    using System;
    using System.Net.Http;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using EffectiveHttpClient;

    [TestClass]
    public class AutoLeaseTest
    {
        [TestMethod]
        public void TestLease()
        {
            const string dataobj = "acquired";

            int leaseCount = 0;

            var mock = new Mock<ILeasable<string>>();
            mock.SetupGet(x => x.LeaseCount).Returns(() => leaseCount);
            mock.Setup(x => x.Acquire())
                .Returns(() =>
                {
                    leaseCount++;
                    return dataobj;
                });
            mock.Setup(x => x.Release()).Returns(() => --leaseCount);

            AutoLease<string> auto;

            // first lease
            leaseCount = 0;
            mock.ResetCalls();
            using (auto = new AutoLease<string>(mock.Object))
            {
                Assert.IsTrue(auto.DataObject == dataobj);
                Assert.IsTrue(leaseCount == 1);
                mock.Verify(x => x.Acquire(), Times.Once);
                mock.Verify(x => x.Release(), Times.Never);
            }
            Assert.IsTrue(leaseCount == 0);
            mock.Verify(x => x.Acquire(), Times.Once);
            mock.Verify(x => x.Release(), Times.Once);

            // not first lease
            leaseCount = 100;
            mock.ResetCalls();
            auto = new AutoLease<string>(mock.Object);
            Assert.IsTrue(auto.DataObject == dataobj);
            Assert.IsTrue(leaseCount == 101);
            mock.Verify(x => x.Acquire(), Times.Once);
            mock.Verify(x => x.Release(), Times.Never);

            // dispose should reduce, but multple dispose doesn't reduce multiple times
            auto.Dispose();
            Assert.IsTrue(leaseCount == 100);
            Assert.IsTrue(auto.DataObject == null);
            mock.Verify(x => x.Acquire(), Times.Once);
            mock.Verify(x => x.Release(), Times.Once);
            auto.Dispose();
            Assert.IsTrue(leaseCount == 100);
            Assert.IsTrue(auto.DataObject == null);
            mock.Verify(x => x.Acquire(), Times.Once);
            mock.Verify(x => x.Release(), Times.Once);
        }
    }
}