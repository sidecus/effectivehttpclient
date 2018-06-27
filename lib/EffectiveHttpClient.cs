namespace EffectiveHttpClient
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    /// <summary>
    /// Generic EffectiveHttpClient which advocates sharing HttpClient for the same type of connection
    /// </summary>
    /// <typeparam name="T">Type used to identify "same type of connection"</typeparam>
    public class EffectiveHttpClient<T> : IDisposable
        where T : class
    {
        /// <summary>
        /// Http client
        /// </summary>
        protected HttpClient client;

        /// <summary>
        /// Http client manager - singleton
        /// </summary>
        protected HttpClientManager<T> manager;

        /// <summary>
        /// client key
        /// </summary>
        public T ClientKey { get; }

        /// <summary>
        /// Creates a new instance of EffectiveHttpClient
        /// <param name="clientFactory">factory method to initialize the client</param>
        /// </summary>
        public EffectiveHttpClient(T key, ClientBuildStrategy strategy)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (strategy == null)
            {
                throw new ArgumentNullException(nameof(strategy));
            }

            this.ClientKey = key;
            this.manager = HttpClientManager<T>.Instance;
            this.client = this.manager.GetClient(key, strategy.Build);
        }

        #region HttpClient proxy properties and methods

        /// <summary>
        /// Gets the base address
        /// </summary>
        public Uri BaseAddress => this.client.BaseAddress;

        /// <summary>
        /// Get a string from a specified url
        /// </summary>
        /// <param name="url">the url (absolute or relative)</param>
        /// <returns>payload as string</returns>
        public virtual async Task<string> GetStringAsync(string url)
        {
            return await this.client.GetStringAsync(url);
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Dispose the httpclient
        /// </summary>
        public void Dispose()
        {
            // Do nothing! We keep IDisposable pattern to make the code looks similar as HttpClient sample codes.
            // Real disposing is done by HttpClientManager.
        }

        #endregion
    }

    /// <summary>
    /// Specialized EffectiveHttpClient which uses base address as the key
    /// </summary>
    /// <typeparam name="string"></typeparam>
    public class EffectiveHttpClient : EffectiveHttpClient<string>
    {
        /// <summary>
        /// Normalize a uri to a schema+domain+port key
        /// </summary>
        /// <param name="uri">uri</param>
        /// <returns>string key which can be used to identify unique host</returns>
        private static string UriToKey(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            // Normalize the schema, domain and port part so that we can ues it as key
            return $"{uri.Scheme}://{uri.Host.ToLowerInvariant()}:{uri.Port}";
        }

        /// <summary>
        /// Initializes a new EffectiveHttpClient with the base address, with no special client initialization
        /// </summary>
        /// <param name="baseAddress">base address</param>
        public EffectiveHttpClient(Uri baseAddress)
            : base(EffectiveHttpClient.UriToKey(baseAddress), new ClientBuildStrategy(baseAddress))
        {
        }

        /// <summary>
        /// Initializes a new EffectiveHttpClient with a client build strategy
        /// </summary>
        /// <param name="strategy">client build strategy</param>
        public EffectiveHttpClient(ClientBuildStrategy strategy)
            : base(EffectiveHttpClient.UriToKey(strategy.BaseAddress), strategy)
        {
        }
    }
}