namespace EffectiveHttpClient
{
    using System;
    using System.Net.Http;

    /// <summary>
    /// EffectiveHttpClient which advocates sharing HttpClient for the same type of connection
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
        public T clientKey { get; }

        /// <summary>
        /// Creates a new instance of EffectiveHttpClient
        /// <param name="valueFactory">factory method to initialize the client</param>
        /// </summary>
        public EffectiveHttpClient(T key, Func<T, HttpClient> valueFactory = null)
        {
            this.clientKey = key;
            this.manager = HttpClientManager<T>.Instance;
            this.client = this.manager.GetClient(key, valueFactory);
        }

        #region HttpClient proxies

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