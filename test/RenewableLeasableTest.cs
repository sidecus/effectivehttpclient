namespace EffectiveHttpClientTest
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using EffectiveHttpClient;

    [TestClass]
    public class RenewableLeasableTest
    {
        [TestMethod]
        public async Task TestRenewableLeasableBehavior()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new RenewableLeasable<HttpClient>(null as HttpClient));

            var dataObj = new HttpClient();
            var leasable = new RenewableLeasable<HttpClient>(dataObj);
            HttpClient ret = null;
            int count = 0;

            // Normal acquire
            Assert.IsTrue(count == 0);
            ret = leasable.Acquire();
            Assert.AreSame(dataObj, ret);
            Assert.IsTrue(leasable.LeaseCount == 1);
            count = leasable.Release();
            Assert.IsTrue(count == 0);

            // Renew and acquire. Single threaded so no danger of not being able to renew.
            ret = leasable.RenewAndAcquire(() => new HttpClient());
            Assert.AreNotSame(dataObj, ret);
            Assert.IsTrue(leasable.LeaseCount == 1);
            count = leasable.Release();
            Assert.IsTrue(count == 0);

            Action<int> execute = (int upper) =>
            {
                for (int i = 0; i < upper; i++)
                {
                    var test = i % 5 == 0 ? leasable.Acquire() : leasable.RenewAndAcquire(() => new HttpClient());
                    leasable.Release();
                }
            };
            await Task.WhenAll(
                Task.Run(() => execute(10)),
                Task.Run(() => execute(20)),
                Task.Run(() => execute(30)));
            Assert.IsTrue(leasable.LeaseCount == 0);
        }
    }
}