namespace EffectiveHttpClientTest
{
    using System;
    using System.Net.Http;
    using System.Text;
    using EffectiveHttpClient;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Moq.Protected;

    [TestClass]
    public class EffectiveHttpClientTest
    {
        internal class EffectiveHttpClientProxy : EffectiveHttpClient<string>
        {
            public HttpClient HttpClient => this.client;

            public EffectiveHttpClientProxy(string key, Func<string, HttpClient> valueFactory = null) : base(key)
            {
            }
        }

        [TestMethod]
        public void TestClientSharing()
        {
            var key = "bing";
            var client1 = new EffectiveHttpClientProxy(key);
            var client2 = new EffectiveHttpClientProxy(key);

            Assert.IsTrue(client1.clientKey == key);
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
            Assert.IsTrue(client.clientKey == "google.com");

            // Call dispose, and make sure HttpClient is not really disposed
            client.Dispose();
            httpClientMock.Protected().Verify("Dispose", Times.Never(), It.IsAny<bool>());
        }
    }
}
