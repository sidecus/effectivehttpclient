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
    public class HttpClientBuildStrategyTest
    {
        [TestMethod]
        public void TestClientBuildStrategyConstruction()
        {
            var uri = new Uri("https://test.com");

            // Test strategy with no creation factory
            var strategy = new HttpClientBuildStrategy(uri);
            Assert.IsTrue(strategy.BaseAddress == uri);
            var client = strategy.Build();
            Assert.IsTrue(client is RenewableHttpClient);
            Assert.IsTrue(client.Client.BaseAddress == uri);

            // Test strategy with creation factory
            var dummyclient = new HttpClient();
            dummyclient.BaseAddress = new Uri("https://unknown.com");
            var strategy2 = new HttpClientBuildStrategy(uri, () => dummyclient);
            var client2 = strategy2.Build();
            Assert.IsTrue(client2.Client.BaseAddress == uri);

            // Test strategy with UseDefaultHeaders
            dummyclient.BaseAddress = new Uri("https://unknown.com");
            var strategy3 = new HttpClientBuildStrategy(uri, () => dummyclient).UseDefaultHeaders(x =>
            {
                x.Add("x-custom", "x-custom");
                x.Authorization = new AuthenticationHeaderValue("Bearer", "token");
                x.From = "from@from.com";
            });
            var client3 = strategy3.Build();
            Assert.IsTrue(client3.Client.BaseAddress == uri);
            var customHeader = client3.Client.DefaultRequestHeaders.GetValues("x-custom");
            Assert.IsTrue(customHeader.Count() == 1);
            Assert.IsTrue(customHeader.FirstOrDefault() == "x-custom");
            Assert.IsTrue(client3.Client.DefaultRequestHeaders.Authorization.Scheme == "Bearer");
            Assert.IsTrue(client3.Client.DefaultRequestHeaders.Authorization.Parameter == "token");
            Assert.IsTrue(client3.Client.DefaultRequestHeaders.From == "from@from.com");
        }
    }
}