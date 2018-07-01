namespace EffectiveHttpClient
{
    using System;
    using System.Net;
    using System.Net.Http;

    /// <summary>
    /// Specialized EffectiveHttpClient which uses base address as the key
    /// </summary>
    public class RenewableHttpClient : IRenewable
    {
        /// <summary>
        /// Creation time of this object
        /// </summary>
        private DateTime CreationTime = DateTime.UtcNow;

        /// <summary>
        /// Gets the http client
        /// </summary>
        public HttpClient Client { get; private set;}

        /// <summary>
        /// Creation Time of the client
        /// </summary>
        public TimeSpan Age => DateTime.UtcNow - this.CreationTime;

        /// <summary>
        /// Gets or sets the error count
        /// </summary>
        public int ErrorCount { get; private set; } = 0;

        /// <summary>
        /// Initializes a new instance of RenewableHttpClient
        /// </summary>
        /// <param name="client">the http client</param>
        public RenewableHttpClient(HttpClient client)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            this.Client = client;
        }

        /// <summary>
        /// Handle error when making http calls
        /// </summary>
        /// <param name="e">web exception</param>
        public void OnError(WebException e)
        {
            this.ErrorCount = this.ErrorCount + 1;
        }

        /// <summary>
        /// Dispose the client
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// internal dispose
        /// </summary>
        /// <param name="disposing">called from disposing</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && this.Client != null)
            {
                this.Client.Dispose();
                this.Client = null;
            }
        }
    }
}
