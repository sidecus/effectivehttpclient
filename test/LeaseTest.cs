namespace EffectiveHttpClientTest
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using EffectiveHttpClient;

    [TestClass]
    public class LeaseTest
    {
        [TestMethod]
        public async Task TestLease()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new Lease<Object>(null as Object));

            var dataObj = new Object();
            var lease = new Lease<Object>(dataObj);

            (Object ret, int count) = lease.Acquire();
            Assert.AreSame(dataObj, ret);
            Assert.IsTrue(count == 1);
            Assert.IsTrue(lease.LeaseCount == 1);

            count = lease.Release();
            Assert.IsTrue(count == 0);

            Action<int> execute = (int upper) =>
            {
                for (int i = 0; i < upper; i++)
                {
                    lease.Acquire();
                    lease.Release();
                }
            };
            await Task.WhenAll(
                Task.Run(() => execute(10)),
                Task.Run(() => execute(20)),
                Task.Run(() => execute(30)));
            Assert.IsTrue(lease.LeaseCount == 0);
        }
    }
}