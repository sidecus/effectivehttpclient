namespace EffectiveHttpClient
{
    using System;
    using System.Diagnostics;
    using System.Net.Http;
    using System.Threading.Tasks;

    /// <summary>
    /// Specialized EffectiveHttpClient which uses base address as the key
    /// </summary>
    public class EffectiveHttpClient : EffectiveHttpClient<string>
    {
        /// <summary>
        /// Initializes a new EffectiveHttpClient with the base address, with no special client initialization
        /// </summary>
        /// <param name="baseAddress">base address</param>
        public EffectiveHttpClient(Uri baseAddress)
            : base(baseAddress.ToString().ToLowerInvariant(), new HttpClientBuildStrategy(baseAddress))
        {
        }

        /// <summary>
        /// Initializes a new EffectiveHttpClient with a client build strategy
        /// </summary>
        /// <param name="strategy">client build strategy</param>
        public EffectiveHttpClient(HttpClientBuildStrategy strategy)
            : base(strategy.BaseAddress.ToString().ToLowerInvariant(), strategy)
        {
        }
    }

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
        public EffectiveHttpClient(T key, HttpClientBuildStrategy strategy)
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
        public virtual Uri BaseAddress => this.client.BaseAddress;

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

            this.EnsureSameHost(url);

            return await this.client.GetStringAsync(url).ConfigureAwait(false);
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

            this.EnsureSameHost(url);

            return await this.client.PostAsync(url, content).ConfigureAwait(false);
        }

        #endregion

        /// <summary>
        /// Verify that the given uri points to the same destination with the client base address
        /// </summary>
        /// <param name="url">destination uri, absolute or relative</param>
        private void EnsureSameHost(string url)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(url));

            // Construct a new uri using base address. This ensures we can get a valid when url is relative path
            var uri = new Uri(this.BaseAddress, url);
            
            // Do our best - if schema, or host, or domain doesn't match, we throw.
            if (!string.Equals(uri.Scheme, this.BaseAddress.Scheme, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(uri.Host, this.BaseAddress.Host, StringComparison.OrdinalIgnoreCase) ||
                uri.Port != this.BaseAddress.Port)
            {
                throw new InvalidOperationException($"{url} points to different host, schema or port from client base address {this.BaseAddress}");
            }
        }

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