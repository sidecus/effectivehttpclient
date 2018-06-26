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
        internal class EffectiveHttpClientProxy : EffectiveHttpClient<string>
        {
            public HttpClient HttpClient => this.client;

            public EffectiveHttpClientProxy(string key, Func<string, HttpClient> valueFactory = null) : base(key)
            {
            }
        }

        [TestMethod]
        public async Task TestNormalUsage()
        {
            var google = "https://google.com";
            Func<string, HttpClient> valueFactory = x =>
            {
                var client = new HttpClient();

                // Set common stuff which doesn't change from request to request
                client.BaseAddress = new Uri(google);
                client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue()
                {
                    NoCache = true,
                };
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                return client;
            };

            var googleClient1 = new EffectiveHttpClient<string>(google, valueFactory);
            var googleClient2 = new EffectiveHttpClient<string>(google, valueFactory);
            var result = await googleClient1.GetStringAsync("/");
            Assert.IsFalse(string.IsNullOrWhiteSpace(result));
        }

        [TestMethod]
        public void TestHttpClientSharing()
        {
            var key = "bing.com";
            var client1 = new EffectiveHttpClientProxy(key);
            var client2 = new EffectiveHttpClientProxy(key);

            Assert.IsTrue(client1.ClientKey == key);
            Assert.AreNotSame(client1, client2);

            // Same key leads to the same http client
            Assert.AreSame(client1.HttpClient, client2.HttpClient);
        }

        [TestMethod]
        public void EnsureHttpClientNotDisposedWhenDisposeCalled()
        {
            // since we cannot mock HttpClient.Dispose(non virtual), we mock HttpClient.Dispose(bool) instead.
            var httpClientMock = new Mock<HttpClient>();
            httpClientMock.Protected().Setup("Dispose", It.IsAny<bool>());
            var client = new EffectiveHttpClientProxy("google.com", x => httpClientMock.Object);

            // Make sure client key is right
            Assert.IsTrue(client.ClientKey == "google.com");

            // Call dispose, and make sure HttpClient is not really disposed
            client.Dispose();
            httpClientMock.Protected().Verify("Dispose", Times.Never(), It.IsAny<bool>());
        }
    }
}
