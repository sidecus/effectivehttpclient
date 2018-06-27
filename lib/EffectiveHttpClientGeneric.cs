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
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            this.VerifyUriAgainstBaseAddress(new Uri(url));

            return await this.client.GetStringAsync(url);
        }

        /// <summary>
        /// Proxy for HttpClient.PostAsync
        /// </summary>
        /// <param name="url">destination url</param>
        /// <param name="content">http content</param>
        /// <returns>Task<HttpResponseMessage></returns>
        public virtual async Task<HttpResponseMessage> PostAsync(string url, HttpContent content)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            this.VerifyUriAgainstBaseAddress(new Uri(url));

            return await this.client.PostAsync(url, content);
        }

        /// <summary>
        /// Verify that the given uri points to the same destination with the client base address
        /// TODO - how can this work against relative path?
        /// </summary>
        /// <param name="uri">destimation uri</param>
        private void VerifyUriAgainstBaseAddress(Uri uri)
        {
            if (uri == null ||
                !string.Equals(uri.Scheme, this.client.BaseAddress.Scheme, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(uri.Host, this.client.BaseAddress.Host, StringComparison.OrdinalIgnoreCase) ||
                uri.Port != this.client.BaseAddress.Port)
            {
                throw new InvalidOperationException($"{nameof(uri)} points to different host/schema or port from client");
            }
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

