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
            const string Acquired = "acquired";

            int leaseCount = 0;

            var mock = new Mock<ILeasable<string>>();
            mock.SetupGet(x => x.LeaseCount).Returns(() => leaseCount);
            mock.Setup(x => x.Acquire())
                .Returns(() =>
                {
                    leaseCount++;
                    return Acquired;
                });
            mock.Setup(x => x.Release()).Returns(() => --leaseCount);

            AutoLease<string> auto;

            // first lease
            leaseCount = 0;
            using (auto = new AutoLease<string>(mock.Object))
            {
                Assert.IsTrue(auto.DataObject == Acquired);
                Assert.IsTrue(leaseCount == 1);
            }
            Assert.IsTrue(leaseCount == 0);

            // not first lease
            leaseCount = 100;
            auto = new AutoLease<string>(mock.Object);
            Assert.IsTrue(auto.DataObject == Acquired);
            Assert.IsTrue(leaseCount == 101);

            // dispose should reduce, but multple dispose doesn't reduce multiple times
            auto.Dispose();
            Assert.IsTrue(leaseCount == 100);
            Assert.IsTrue(auto.DataObject == null);
            auto.Dispose();
            Assert.IsTrue(leaseCount == 100);
            Assert.IsTrue(auto.DataObject == null);

        }
    }
}