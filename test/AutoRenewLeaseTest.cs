namespace EffectiveHttpClientTest
{
    using System;
    using System.Net.Http;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using EffectiveHttpClient;

    [TestClass]
    public class AutoRenewLeaseTest
    {
        [TestMethod]
        public void TestAutoRenew()
        {
            const string Renewed = "renewed";
            const string Acquired = "acquired";

            int leaseCount = 0;
            bool renewed = false;

            var mock = new Mock<IRenewableLeasable<string>>();
            mock.SetupGet(x => x.LeaseCount).Returns(() => leaseCount);
            mock.Setup(x => x.Acquire())
                .Returns(() =>
                {
                    leaseCount++;
                    return Acquired;
                });
            mock.Setup(x => x.Release()).Returns(() => --leaseCount);
            mock.Setup(x => x.RenewAndAcquire(It.IsAny<Func<string>>()))
                .Returns((Func<string> x) =>
                {
                    renewed = true;
                    leaseCount ++;
                    return x();
                });

            AutoRenewLease<string> auto;

            // not first lease, auto renew lease should just return old value
            leaseCount = 100;
            auto = new AutoRenewLease<string>(mock.Object, () => Acquired);
            Assert.IsTrue(auto.DataObject == Acquired);
            Assert.IsFalse(renewed);
            Assert.IsTrue(leaseCount == 101);

            // dispose should reduce, but multple dispose doesn't reduce multiple times
            auto.Dispose();
            Assert.IsTrue(leaseCount == 100);
            auto.Dispose();
            Assert.IsTrue(leaseCount == 100);

            // first lease
            leaseCount = 0;
            auto = new AutoRenewLease<string>(mock.Object, () => Renewed);
            // TODO: Should be false - depending on the strategy
            Assert.IsTrue(auto.DataObject == Renewed);
            Assert.IsTrue(renewed);
            Assert.IsTrue(leaseCount == 1);
        }
    }
}