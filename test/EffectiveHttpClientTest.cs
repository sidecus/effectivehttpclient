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
        [TestMethod]
        public async Task TestRealSimpleGet()
        {
            // Simple usage (default client build strategy)
            var google = new Uri("https://google.com");
            using (var googleClient = new EffectiveHttpClient(google))
            {
                var result = await googleClient.GetStringAsync("https://google.com");
                Assert.IsFalse(string.IsNullOrWhiteSpace(result));
            }
        }

        [TestMethod]
        public async Task TestRealComplexPost()
        {
            // More complex usage with http post
            var httpbin = new Uri("http://httpbin.org");
            var buildStrategy = new HttpClientBuildStrategy(httpbin)
                .UseDefaultHeaders(x => 
                {
                    x.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                });

            // Using is not needed, but you can still use it without having to worry about anything
            using (var httpbinClient = new EffectiveHttpClient(buildStrategy))
            {
                var payload = "testdata";
                var content = new StringContent(payload, Encoding.UTF8);
                using(var response = await httpbinClient.PostAsync("http://httpbin.org/post", content))
                {
                    Assert.IsTrue(response.IsSuccessStatusCode);

                    var result = await response.Content.ReadAsStringAsync();
                    Assert.IsTrue(result.Contains("testdata"));
                }
            }
        }

        // Proxy class which exposes the HttClient object
        internal class ProxyClass : EffectiveHttpClient
        {
            public HttpClient HttpClient => this.client;

            public ProxyClass(Uri baseAddress) : base(baseAddress) {}
            public ProxyClass(HttpClientBuildStrategy buildStrategy) : base(buildStrategy) {}
        }

        [TestMethod]
        public void TestKeyAndClientBehavior()
        {
            var uri = new Uri("HTTPS://bing.com");
            var uri2 = new Uri("https://bINg.com:443");
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
        public void TestHttpClientNotDisposedWhenDisposeCalled()
        {
            var baseAddress = "http://google.com";

            // since we cannot mock HttpClient.Dispose(non virtual), we mock HttpClient.Dispose(bool) instead.
            var httpClientMock = new Mock<HttpClient>();
            httpClientMock.Protected().Setup("Dispose", It.IsAny<bool>());
            var strategy = new HttpClientBuildStrategy(new Uri(baseAddress), () => httpClientMock.Object);
            var client = new EffectiveHttpClient(strategy);

            // Call dispose, and make sure HttpClient is not really disposed
            client.Dispose();
            httpClientMock.Protected().Verify("Dispose", Times.Never(), It.IsAny<bool>());
            client.Dispose();
            httpClientMock.Protected().Verify("Dispose", Times.Never(), It.IsAny<bool>());
        }

        [TestMethod]
        public async Task TestHostChangeNotAllowed()
        {
            // Call the client with a different host ends up with exception
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => {
                var google = new Uri("https://google.com");
                using (var googleClient = new EffectiveHttpClient(google))
                {
                    await googleClient.GetStringAsync("https://bing.com");
                }
            });

            // Call the client with a different scheme ends up with exception
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => {
                var google = new Uri("https://google.com");
                using (var googleClient = new EffectiveHttpClient(google))
                {
                    await googleClient.GetStringAsync("http://google.com");
                }
            });
        }
    }
}
