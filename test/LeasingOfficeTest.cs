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

            var stringManager = LeasingOffice<string, HttpClient>.Instance;
            var sbManager = LeasingOffice<StringBuilder, HttpClient>.Instance;

            // Verify the singleton behavior on specialized generic class
            Assert.AreSame(stringManager, stringManager);
            Assert.AreNotSame(stringManager, sbManager);
        }
    }
}
