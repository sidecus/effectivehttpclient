namespace samples
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using EffectiveHttpClient;

    class Program
    {
        public static async Task MakeHttpGet()
        {
            var baseAddress = new Uri(@"https://bing.com");
            var url = @"?toWww=1&redig=test";

            // 5 minute timeout on all calls for debugging
            var buildStrategy = new HttpClientBuildStrategy(baseAddress)
                .UseTimeOut(new TimeSpan(0, 5, 0));

            // 5 minute age then it'll retire
            var renewStrategy = new RenewStrategy()
                .UseAgeStrategy(new TimeSpan(1, 5, 0));

            var random = new Random();
            using (var client = new EffectiveHttpClient(buildStrategy, renewStrategy))
            {
                // Sleep random number of milliseconds (20 to 500) before hand to allow connection reuse
                // This simulates real time concurrency scenarios
                Thread.Sleep(random.Next(20, 500));

                // Make the real call
                var ret = await client.GetStringAsync(url).ConfigureAwait(false);
                Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} succeeded with {ret.Length} characters.");
                Console.WriteLine(client.ToString());
            }
        }

        static void Main(string[] args)
        {
            var tasks = new List<Task>();

            // Make 10 concurrent calls against bing to simulate prod scenarios.
            // You should have less than 10 sockets againts www.bing.com.
            Console.WriteLine("Starting 10 threads with HTTP GET to www.bing.com with age strategy.");
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(MakeHttpGet());
            }
            Task.WaitAll(tasks.ToArray());
            Console.WriteLine("Please check socket number.");
        }
    }
}
