namespace EffectiveHttpClient
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net.Http;

    /// <summary>
    /// HttpClientManager which advocates the pattern to reuse HttpClient,
    /// while making it easier to manager.
    /// This is a singleton based on type parameter T.
    /// </summary>
    /// TODO: client life cycle mnagement
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
        protected ConcurrentDictionary<T, Lease<HttpClient>> clients = new ConcurrentDictionary<T, Lease<HttpClient>>();

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
            var lease = this.clients.GetOrAdd(key, x => this.CreateLease(clientFactory));

            // Lease the client
            (var client, var count) = lease.Acquire();

            // Todo: do something with the client...

            return client;
        }

        /// <summary>
        /// Create a new HttpClient and establish a lease for it
        /// </summary>
        /// <param name="clientFactory">factory method used to create the client</param>
        /// <returns>a lease wrapper for the client</returns>
        private Lease<HttpClient> CreateLease(Func<HttpClient> clientFactory)
        {
            Debug.Assert(clientFactory != null);
            return new Lease<HttpClient>(clientFactory());
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
                        (var client, var count) = kvp.Value.Acquire();
                        // Dispose, regardles whether we have reference or not
                        Debug.Assert(count == 0);
                        client.Dispose();
                    }
                }
            }

            this.disposed = true;
        }

        #endregion
    }
}
