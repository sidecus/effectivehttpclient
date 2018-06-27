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
    public class ClientBuildStrategyTest
    {
        [TestMethod]
        public void TestClientBuildStrategyConstruction()
        {
            var uri = new Uri("https://test.com");
            var dummyclient = new HttpClient();

            // Test strategy with no creation factory
            var strategy = new ClientBuildStrategy(uri);
            Assert.IsTrue(strategy.BaseAddress == uri);
            var client = strategy.Build();
            Assert.IsTrue(client is HttpClient);
            Assert.IsTrue(client.BaseAddress == uri);

            // Test strategy with creation factory
            dummyclient.BaseAddress = new Uri("https://unknown.com");
            var strategy2 = new ClientBuildStrategy(uri, () => dummyclient);
            var client2 = strategy2.Build();
            Assert.IsTrue(client2.BaseAddress == uri);

            // Test strategy with UseDefaultHeaders
            dummyclient.BaseAddress = new Uri("https://unknown.com");
            var strategy3 = new ClientBuildStrategy(uri, () => dummyclient).UseDefaultHeaders(x =>
            {
                x.Add("x-custom", "x-custom");
                x.Authorization = new AuthenticationHeaderValue("Bearer", "token");
                x.From = "from@from.com";
            });
            var client3 = strategy3.Build();
            Assert.IsTrue(client3.BaseAddress == uri);
            var customHeader = client3.DefaultRequestHeaders.GetValues("x-custom");
            Assert.IsTrue(customHeader.Count() == 1);
            Assert.IsTrue(customHeader.FirstOrDefault() == "x-custom");
            Assert.IsTrue(client3.DefaultRequestHeaders.Authorization.Scheme == "Bearer");
            Assert.IsTrue(client3.DefaultRequestHeaders.Authorization.Parameter == "token");
            Assert.IsTrue(client3.DefaultRequestHeaders.From == "from@from.com");
        }
    }
}