namespace EffectiveHttpClient
{
    using System;
    using System.Net.Http;

    /// <summary>
    /// Specialized EffectiveHttpClient which uses base address as the key
    /// </summary>
    public class RenewableHttpClient : IRenewable
    {
        /// <summary>
        /// Gets the http client
        /// </summary>
        public HttpClient Client { get; private set;}

        /// <summary>
        /// Creation Time of the client
        /// </summary>
        public DateTime CreationTime { get; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the error count
        /// </summary>
        public int ErrorCount { get; set; } = 0;

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
        /// Dispose the client
        /// </summary>
        public void Dispose()
        {
            this.Client.Dispose();
            this.Client = null;
        }
    }
}
