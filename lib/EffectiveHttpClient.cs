namespace EffectiveHttpClient
{
    using System;
    using System.Diagnostics;
    using System.Net;
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
            : base(
                baseAddress.ToString().ToLowerInvariant(),
                new HttpClientBuildStrategy(baseAddress),
                new RenewStrategy())
        {
        }

        /// <summary>
        /// Initializes a new EffectiveHttpClient with a client build strategy
        /// </summary>
        /// <param name="buildStrategy">client build strategy</param>
        /// <param name="renewStrategy">client renew strategy</param>
        public EffectiveHttpClient(HttpClientBuildStrategy buildStrategy, RenewStrategy renewStrategy)
            : base(
                buildStrategy.BaseAddress.ToString().ToLowerInvariant(),
                buildStrategy,
                renewStrategy)
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
        /// Http client leasing office - singleton
        /// </summary>
        protected readonly LeasingOffice<T, RenewableHttpClient> leasingOffice = LeasingOffice<T, RenewableHttpClient>.Instance;

        /// <summary>
        /// Http client
        /// </summary>
        protected AutoLease<RenewableHttpClient> clientLease = null;

        /// <summary>
        /// get the http client
        /// </summary>
        protected HttpClient httpClient => this.clientLease?.DataObject.Client;

        /// <summary>
        /// client key
        /// </summary>
        public T ClientKey { get; }

        /// <summary>
        /// Creates a new instance of EffectiveHttpClient
        /// <param name="buildStrategy">build strategy to initialize the client</param>
        /// <param name="renewStrategy">renew strategy to renew the client</param>
        /// </summary>
        public EffectiveHttpClient(
            T key,
            HttpClientBuildStrategy buildStrategy,
            RenewStrategy renewStrategy)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (buildStrategy == null)
            {
                throw new ArgumentNullException(nameof(buildStrategy));
            }

            this.ClientKey = key;

            // Accept different renew policy
            var leasable = this.leasingOffice.GetLeasable(key, buildStrategy, renewStrategy);

            // Automatically acquire lease
            this.clientLease = new AutoLease<RenewableHttpClient>(leasable);
        }

        #region HttpClient proxy properties and methods

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

            return await this.MakeHttpCall(url, () => this.httpClient.GetStringAsync(url)).ConfigureAwait(false);
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

            return await this.MakeHttpCall(url, () => this.httpClient.PostAsync(url, content)).ConfigureAwait(false);
        }

        #endregion

        /// <summary>
        /// Wrapper function to make http calls. It checks base address, makes the call, and records error
        /// </summary>
        /// <param name="url">target url, absolute or relative</param>
        /// <param name="func">func to call</param>
        /// <typeparam name="TReturn">return type, for example HttpResponseMessage</typeparam>
        /// <returns>returns the result from the http call</returns>
        private async Task<TReturn> MakeHttpCall<TReturn>(string url, Func<Task<TReturn>> func)
        {
            this.EnsureSameHost(this.httpClient.BaseAddress, url);

            try
            {
                return await func();
            }
            catch(WebException e)
            {
                Trace.WriteLine($"Error occured when making call to {url}. {e}");
                // Call RenewableHttpClient.OnError to increase error count on the client.
                this.clientLease.DataObject.OnError(e);
                throw;
            }
        }

        /// <summary>
        /// Verify that the given uri points to the same destination with the client base address
        /// </summary>
        /// <param name="baseAddress">current base address</param>
        /// <param name="url">destination uri, absolute or relative</param>
        private void EnsureSameHost(Uri baseAddress, string url)
        {
            Debug.Assert(baseAddress != null);
            Debug.Assert(!string.IsNullOrWhiteSpace(url));

            // Construct a new uri using base address. This ensures we can get a valid when url is relative path
            var uri = new Uri(baseAddress, url);
            
            // Do our best - if schema, or host, or domain doesn't match, we throw.
            if (!string.Equals(uri.Scheme, baseAddress.Scheme, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(uri.Host, baseAddress.Host, StringComparison.OrdinalIgnoreCase) ||
                uri.Port != baseAddress.Port)
            {
                throw new InvalidOperationException($"{url} points to different host, schema or port from client base address {baseAddress}");
            }
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

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && this.clientLease != null)
            {
                // Release the lease
                this.clientLease.Dispose();
                this.clientLease = null;
            }
        }

        #endregion
    }
}