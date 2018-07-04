namespace samples
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using EffectiveHttpClient;

    class Program
    {
        public static async Task MakeHttpGet(Random random)
        {
            var baseAddress = new Uri(@"https://bing.com");
            var url = @"?toWww=1&redig=test";

            // 5 minute timeout on all calls for debugging
            var buildStrategy = new HttpClientBuildStrategy(baseAddress)
                .UseTimeOut(new TimeSpan(0, 5, 0));

            // 5 minute age then it'll retire
            var renewStrategy = new RenewStrategy()
                .UseAgeStrategy(new TimeSpan(1, 5, 0));

            using (var client = new EffectiveHttpClient(buildStrategy, renewStrategy))
            {
                // Sleep random number of milliseconds (20 to 500) beforehand to allow connection reuse
                // This simulates real time concurrency scenarios
                Thread.Sleep(random.Next(100, 5000));

                // Make the real call
                var originatingThread = Thread.CurrentThread.ManagedThreadId;
                Console.WriteLine($"{originatingThread}: started...");

                var ret = await client.GetStringAsync(url).ConfigureAwait(false);

                var endingThread = Thread.CurrentThread.ManagedThreadId;
                Console.WriteLine($"{originatingThread} -> {endingThread}: downloaded {ret.Length} characters. {client.ToString()}");
            }
        }

        static void Main(string[] args)
        {
            var tasks = new List<Task>();
            var random = new Random();
            
            // Make 10 concurrent calls against bing to simulate prod scenarios.
            // You should have less than 10 sockets againts www.bing.com.
            Console.WriteLine("Starting 10 threads with HTTP GET to www.bing.com with age strategy.");
            for (int i = 0; i < 10; i++)
            {
                var task = Task.Run(async () => {
                    await MakeHttpGet(random);
                });
                
                tasks.Add(task);
            }
            Task.WaitAll(tasks.ToArray());
            Console.WriteLine("You can now check total connections to bing.com");

            Thread.Sleep(60000);
        }
    }
}
