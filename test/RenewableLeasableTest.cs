namespace EffectiveHttpClientTest
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using EffectiveHttpClient;

    [TestClass]
    public class RenewableLeasableTest
    {
        [TestMethod]
        public async Task TestAutoRenewLeasableBehavior()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new AutoRenewLeasable<HttpClient>(null as IBuildStrategy<HttpClient>, null as IRenewPolicy<HttpClient>));

            var mockBuildStrategy = new Mock<IBuildStrategy<HttpClient>>();
            var mockRenewPolicy = new Mock<IRenewPolicy<HttpClient>>();

            var dataObj = new HttpClient();
            int buildCount = 0;
            int renewCheckCount = 0;
            mockBuildStrategy.Setup(x => x.Build()).Returns(() =>
            {
                buildCount ++;
                return dataObj;
            });
            mockRenewPolicy.Setup(x => x.ShallRenew(It.IsAny<HttpClient>())).Returns((HttpClient x) =>
            {
                renewCheckCount ++;
                return renewCheckCount % 2 == 0;
            });

            var leasable = new AutoRenewLeasable<HttpClient>(mockBuildStrategy.Object, mockRenewPolicy.Object);
            HttpClient ret = null;

            // first acquire and release
            Assert.IsTrue(buildCount == 0);
            Assert.IsTrue(renewCheckCount == 0);
            ret = leasable.Acquire();
            Assert.AreSame(dataObj, ret);
            Assert.IsTrue(leasable.LeaseCount == 1);
            Assert.IsTrue(buildCount == 1);
            leasable.Release();
            Assert.IsTrue(leasable.LeaseCount == 0);
            Assert.IsTrue(renewCheckCount == 1);

            // acquire and release again
            leasable.Acquire();
            Assert.IsTrue(buildCount == 1);
            leasable.Release();
            Assert.IsTrue(leasable.LeaseCount == 0);
            Assert.IsTrue(renewCheckCount == 2);

            // again - we should trigger renew
            leasable.Acquire();
            Assert.IsTrue(leasable.LeaseCount == 1);
            Assert.IsTrue(buildCount == 2);
            leasable.Release();
            Assert.IsTrue(renewCheckCount == 3);

            // Multi thread acquire/release should not cause incorrect couter issues
            buildCount = 0;
            renewCheckCount = 0;
            Action<int> execute = (int upper) =>
            {
                for (int i = 0; i < upper; i++)
                {
                    leasable.Acquire();
                    leasable.Release();
                }
            };
            await Task.WhenAll(
                Task.Run(() => execute(5)),
                Task.Run(() => execute(7)),
                Task.Run(() => execute(3)));
            Assert.IsTrue(leasable.LeaseCount == 0);
        }
    }
}