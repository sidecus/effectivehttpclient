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
    /// TODO: capacity & MUFO/FIFO strategy?
    public class HttpClientManager<T> : IDisposable
        where T: class
    {
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
        public static readonly HttpClientManager<T> Instance = new HttpClientManager<T>();

        /// <summary>
        /// Protected constructor
        /// </summary>
        protected HttpClientManager() {}

        /// <summary>
        /// Get a client based off a key of type T
        /// </summary>
        /// <param name="key">the key to reuse</param>
        /// <param name="valueFactor">factory method to initialize an HttpClient</param>
        /// <returns>An HttpClient</returns>
        public HttpClient GetClient(T key, Func<HttpClient> clientFactory)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            if (clientFactory == null)
            {
                throw new ArgumentNullException("clientFactory");
            }

            // Thread safe GetOrAdd
            return this.clients.GetOrAdd(key, x => clientFactory());
        }

        /// <summary>
        /// Remove a client based on the client key. Caller is in charge of disposing the client!
        /// </summary>
        /// <param name="key">client key</param>
        /// <returns>client if exists, otherwise null</returns>
        public HttpClient RemoveClient(T key)
        {
            HttpClient client;
            var ret = this.clients.TryRemove(key, out client);
            return ret ? client : null;
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
