namespace EffectiveHttpClientTest
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Moq.Protected;
    using EffectiveHttpClient;
    using System.Linq;

    [TestClass]
    public class RenewStrategyTest
    {
        private Mock<IRenewable> mock = new Mock<IRenewable>();

        private IRenewable renewable => this.mock.Object;

        [TestMethod]
        public void TestDefaultRenewStrategy()
        {
            var strategy = new RenewStrategy();

            // Default behavior is to renew whenever asked
            Assert.IsTrue(strategy.ShallRenew(this.renewable));
        }

        [TestMethod]
        public void TestAgeStrategy()
        {
            var oneHour = new TimeSpan(1, 0, 0);
            var oneHourAndOneSecond = new TimeSpan(1, 0, 1);
            var oneHourMinusOneSecond = new TimeSpan(0, 59, 59);

            this.mock.Setup(x => x.Age).Returns(oneHour);

            // Should renew when renew strategy specifis the same age or younger one
            Assert.IsTrue(new RenewStrategy().UseAgeStrategy(oneHour).ShallRenew(this.renewable));
            Assert.IsTrue(new RenewStrategy().UseAgeStrategy(oneHourMinusOneSecond).ShallRenew(this.renewable));

            // Should not renew when renew stratege specifies a bigger limit
            Assert.IsFalse(new RenewStrategy().UseAgeStrategy(oneHourAndOneSecond).ShallRenew(this.renewable));
        }

        [TestMethod]
        public void TestErrorStrategy()
        {
            this.mock.Setup(x => x.ErrorCount).Returns(5);

            Assert.IsFalse(new RenewStrategy().UseErrorStrategy(6).ShallRenew(this.renewable));

            Assert.IsTrue(new RenewStrategy().UseErrorStrategy(4).ShallRenew(this.renewable));
        }

        [TestMethod]
        public void TestUsageStrategy()
        {
            this.mock.Setup(x => x.UsageCount).Returns(10);

            Assert.IsFalse(new RenewStrategy().UseUsageStrategy(10).ShallRenew(this.renewable));

            Assert.IsTrue(new RenewStrategy().UseUsageStrategy(9).ShallRenew(this.renewable));
        }
    }
}