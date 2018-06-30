namespace EffectiveHttpClientTest
{
    using System.Text;
    using System.Net.Http;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using EffectiveHttpClient;

    [TestClass]
    public class HttpClientManagerTest
    {
        [TestMethod]
        public void TestSinletonBehavior()
        {
            // // This should not compile
            // var manager = new HttpClientManager<string>();

            var stringManager = LeasingOffice<string, RenewableHttpClient>.Instance;
            var sbManager = LeasingOffice<StringBuilder, RenewableHttpClient>.Instance;

            // Verify the singleton behavior on specialized generic class
            Assert.AreSame(stringManager, stringManager);
            Assert.AreNotSame(stringManager, sbManager);
        }
    }
}
