namespace EffectiveHttpClient
{
    using System;
    using System.Net;
    using System.Net.Http;

    /// <summary>
    /// Renewable which wraps object with IRenewable properties
    /// </summary>
    public class Renewable<T> : IRenewable where T : class, IDisposable
    {
        /// <summary>
        /// Creation time of this object
        /// </summary>
        private DateTime CreationTime = DateTime.UtcNow;

        /// <summary>
        /// Gets the data object
        /// </summary>
        public T Client { get; private set;}

        /// <summary>
        /// Creation Time of the client
        /// </summary>
        public TimeSpan Age => DateTime.UtcNow - this.CreationTime;

        /// <summary>
        /// Gets or sets the error count
        /// </summary>
        public int ErrorCount { get; private set; } = 0;

        /// <summary>
        /// Initializes a new instance of Renewable
        /// </summary>
        /// <param name="client">the client object</param>
        public Renewable(T client)
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
        public virtual void OnError(WebException e)
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
