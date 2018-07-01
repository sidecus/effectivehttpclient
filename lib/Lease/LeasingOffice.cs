namespace EffectiveHttpClient
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net.Http;

    /// <summary>
    /// LeasingOffice which manages all leasables.
    /// This is a singleton based on type parameter T.
    /// </summary>
    public class LeasingOffice<TKey, TData> : IDisposable
        where TKey: class
        where TData: class, IRenewable
    {
        /// <summary>
        /// List of available leasables
        /// </summary>
        /// <typeparam name="T">Type used for httpclient lookup</typeparam>
        protected ConcurrentDictionary<TKey, AutoRenewLeasable<TData>> leasables = new ConcurrentDictionary<TKey, AutoRenewLeasable<TData>>();

        /// <summary>
        /// The singleton instance
        /// </summary>
        /// <typeparam name="T">Type used for http client lookup</typeparam>
        public static readonly LeasingOffice<TKey, TData> Instance = new LeasingOffice<TKey, TData>();

        /// <summary>
        /// Protected constructor
        /// </summary>
        protected LeasingOffice() {}

        /// <summary>
        /// Get a leasable based off the given key, or create a new leasable
        /// </summary>
        /// <param name="key">the key to reuse</param>
        /// <param name="buildStrategy">build strategy to initialize a new leasable</param>
        /// <returns>An auto renew leasable</returns>
        public AutoRenewLeasable<TData> GetLeasable(
            TKey key,
            IBuildStrategy<TData> buildStrategy,
            IRenewStrategy renewStrategy)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (buildStrategy == null)
            {
                throw new ArgumentNullException(nameof(buildStrategy));
            }

            // Thread safe GetOrAdd
            var leasable = this.leasables.GetOrAdd(key, x => new AutoRenewLeasable<TData>(buildStrategy, renewStrategy));

            return leasable;
        }

        #region IDisposable

        /// <summary>
        /// Dispose the leasing office. This will distroy all httpclients under the management
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Internal disposer. Disposes all data objects we are holding.
        /// </summary>
        /// <param name="disposing">Called from Dispose</param>
        protected virtual void Dispose(bool disposing)
        {
            // Called from Dispose, dispose realy
            if (disposing && this.leasables != null && this.leasables.Count > 0)
            {
                // Explicitly dispose all data objects we are holding
                foreach (var kvp in this.leasables)
                {
                    kvp.Value.Dispose();
                }

                this.leasables = null;
            }
        }

        #endregion
    }
}
