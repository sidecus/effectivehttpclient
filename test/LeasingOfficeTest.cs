namespace EffectiveHttpClientTest
{
    using System.Text;
    using System.Net.Http;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using EffectiveHttpClient;
    using System.IO;
    using System;

    [TestClass]
    public class HttpClientManagerTest
    {
        [TestMethod]
        public void TestSinletonBehavior()
        {
            // // This should not compile
            // var manager = new HttpClientManager<string>();

            // Verify the singleton behavior on specialized generic class
            var stringManager = AutoRenewLeasingOffice<string, Renewable<HttpClient>>.Instance;
            var sbManager = AutoRenewLeasingOffice<StringBuilder, Renewable<HttpClient>>.Instance;
            Assert.IsNotNull(stringManager);
            Assert.IsNotNull(sbManager);
            Assert.AreNotSame(stringManager, sbManager);
        }

        [TestMethod]
        public void TestGetLeasableBehavior()
        {
            var manager = AutoRenewLeasingOffice<string, Renewable<MemoryStream>>.Instance;

            var msMock = new Mock<MemoryStream>();
            var renewableMock = new Mock<Renewable<MemoryStream>>(msMock.Object);
            var renewMock = new Mock<IRenewStrategy>();
            var buildMock = new Mock<IBuildStrategy<Renewable<MemoryStream>>>();

            var leasingOffice = AutoRenewLeasingOffice<string, Renewable<MemoryStream>>.Instance;

            // Argument checking
            Assert.ThrowsException<ArgumentNullException>(() => leasingOffice.GetLeasable(null as string, buildMock.Object, renewMock.Object));
            Assert.ThrowsException<ArgumentNullException>(() => leasingOffice.GetLeasable("test", null, renewMock.Object));
            Assert.ThrowsException<ArgumentNullException>(() => leasingOffice.GetLeasable("test", buildMock.Object, null));

            // Get by "test" as key, same object should be returned on multiple calls
            var ret1 = leasingOffice.GetLeasable("test", buildMock.Object, renewMock.Object);
            Assert.IsTrue(ret1 != null);
            Assert.IsTrue(ret1 is AutoRenewLeasable<Renewable<MemoryStream>>);
            Assert.IsTrue(ret1.LeaseCount == 0);
            var ret2 = leasingOffice.GetLeasable("test", buildMock.Object, renewMock.Object);
            Assert.AreSame(ret2, ret1);

            // Get by a different key, should get a new object
            var ret3 = leasingOffice.GetLeasable("differentkey", buildMock.Object, renewMock.Object);
            Assert.IsTrue(ret3 != null);
            Assert.IsTrue(ret3 is AutoRenewLeasable<Renewable<MemoryStream>>);
            Assert.AreNotSame(ret3, ret1);
        }
    }
}
