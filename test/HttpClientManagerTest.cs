namespace EffectiveHttpClientTest
{
    using System.Text;
    using EffectiveHttpClient;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class HttpClientManagerTest
    {
        [TestMethod]
        public void TestSinletonBehavior()
        {
            // // Doesn't compile
            // var manager = new HttpClientManager<string>();
            var stringManager = HttpClientManager<string>.Instance;
            var sbManager = HttpClientManager<StringBuilder>.Instance;

            Assert.AreSame(stringManager, stringManager);
            Assert.AreNotSame(stringManager, sbManager);
        }
    }
}
