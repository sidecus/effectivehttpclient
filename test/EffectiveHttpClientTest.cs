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

    [TestClass]
    public class EffectiveHttpClientTest
    {
        // Proxy class which exposes the HttClient object
        internal class ProxyClass : EffectiveHttpClient
        {
            public HttpClient HttpClient => this.client;

            public ProxyClass(Uri baseAddress) : base(baseAddress) {}
            public ProxyClass(ClientBuildStrategy buildStrategy) : base(buildStrategy) {}
        }

        [TestMethod]
        public void TestKeyAndClientBehavior()
        {
            var uri = new Uri("https://bing.com");
            var uri2 = new Uri("https://bing.com:443/api");
            var uri3 = new Uri("http://bing.com:443/api/test");
            var client1 = new ProxyClass(uri);
            var client2 = new ProxyClass(uri2);
            var client3 = new ProxyClass(uri3);

            // client 1 and client2 should share the same client, and have the same key
            Assert.IsTrue(client1.ClientKey == client2.ClientKey);
            Assert.AreSame(client1.HttpClient, client2.HttpClient);

            // client1 and client3 should not share the same client
            Assert.IsFalse(client1.ClientKey == client3.ClientKey);
            Assert.AreNotSame(client1.HttpClient, client3.HttpClient);
        }

        [TestMethod]
        public void EnsureHttpClientNotDisposedWhenDisposeCalled()
        {
            var baseAddress = "http://google.com";

            // since we cannot mock HttpClient.Dispose(non virtual), we mock HttpClient.Dispose(bool) instead.
            var httpClientMock = new Mock<HttpClient>();
            httpClientMock.Protected().Setup("Dispose", It.IsAny<bool>());
            var strategy = new ClientBuildStrategy(new Uri(baseAddress), () => httpClientMock.Object);
            var client = new EffectiveHttpClient(strategy);

            // Call dispose, and make sure HttpClient is not really disposed
            client.Dispose();
            httpClientMock.Protected().Verify("Dispose", Times.Never(), It.IsAny<bool>());
            client.Dispose();
            httpClientMock.Protected().Verify("Dispose", Times.Never(), It.IsAny<bool>());
        }
        
        [TestMethod]
        public async Task TestGoogleGet()
        {
            var google = new Uri("https://google.com");
            var buildStrategy =
                new ClientBuildStrategy(google)
                .UseDefaultHeaders(x => 
                {
                    // Set common stuff which doesn't change from request to request
                    x.CacheControl = new CacheControlHeaderValue()
                    {
                        NoCache = true,
                    };
                    x.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                });

            var googleClient = new EffectiveHttpClient(buildStrategy);
            var result = await googleClient.GetStringAsync("/");
            Assert.IsFalse(string.IsNullOrWhiteSpace(result));
        }
    }
}
