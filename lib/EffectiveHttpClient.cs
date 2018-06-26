namespace EffectiveHttpClient
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    /// <summary>
    /// EffectiveHttpClient which advocates sharing HttpClient for the same type of connection
    /// </summary>
    /// <typeparam name="T">Type used to identify "same type of connection"</typeparam>
    public class EffectiveHttpClient<T> : IDisposable
        where T : class
    {
        /// <summary>
        /// Default value factory
        /// </summary>
        private static readonly Func<T, HttpClient> DefaultClientFactory = x => new HttpClient();

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
        public EffectiveHttpClient(T key, Func<T, HttpClient> clientFactory = null)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            // If valueFactory is not specified, use the default one which creates a default HttpClient w/o special initialization
            clientFactory = clientFactory ?? EffectiveHttpClient<T>.DefaultClientFactory;

            this.ClientKey = key;
            this.manager = HttpClientManager<T>.Instance;
            this.client = this.manager.GetClient(key, clientFactory);
        }

        #region HttpClient proxy methods

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
}