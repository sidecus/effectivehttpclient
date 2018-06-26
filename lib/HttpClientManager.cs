namespace EffectiveHttpClient
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;

    /// <summary>
    /// HttpClientManager which advocates the pattern to reuse HttpClient,
    /// while making it easier to manager.
    /// This is a singleton based on type parameter T.
    /// </summary>
    public class HttpClientManager<T> : IDisposable
        where T: class
    {
        /// <summary>
        /// Default value factory
        /// </summary>
        private static readonly Func<T, HttpClient> DefaultValueFactory = x => new HttpClient();

        /// <summary>
        /// Has the object been disposed
        /// </summary>
        private bool disposed = false;

        /// <summary>
        /// List of available http clients
        /// </summary>
        /// <typeparam name="T">Type used for httpclient lookup</typeparam>
        protected ConcurrentDictionary<T, HttpClient> clients = new ConcurrentDictionary<T, HttpClient>();

        /// <summary>
        /// The singleton instance
        /// </summary>
        /// <typeparam name="T">Type used for http client lookup</typeparam>
        /// <returns></returns>
        public static readonly HttpClientManager<T> Instance = new HttpClientManager<T>();

        /// <summary>
        /// Protected constructor
        /// </summary>
        protected HttpClientManager()
        {
        }

        /// <summary>
        /// Get a client based off a key of type T
        /// </summary>
        /// <param name="key">the key to reuse</param>
        /// <param name="valueFactor">factory method to initialize an HttpClient</param>
        /// <returns>An HttpClient</returns>
        public HttpClient GetClient(T key, Func<T, HttpClient> valueFactory = null)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            // If valueFactory is not specified, use the default one which creates a default HttpClient w/o special initialization
            valueFactory = valueFactory ?? HttpClientManager<T>.DefaultValueFactory;

            // Thread safe GetOrAdd
            return this.clients.GetOrAdd(key, valueFactory);
        }

        #region IDisposable

        /// <summary>
        /// Dispose the httpclient
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Internal disposer
        /// </summary>
        /// <param name="disposing">Called from Dispose</param>
        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                if (this.clients != null && this.clients.Count > 0)
                {
                    // Explicitly dispose all HttpClients we are holding
                    foreach (var kvp in this.clients)
                    {
                        kvp.Value.Dispose();
                    }
                }
            }

            this.disposed = true;
        }

        #endregion
    }
}
