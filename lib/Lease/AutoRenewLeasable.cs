namespace EffectiveHttpClient
{
    using System;
    using System.Diagnostics;
    using System.Threading;

    /// <summary>
    /// Lease management
    /// </summary>
    /// <typeparam name="T">data object</typeparam>
    public class AutoRenewLeasable<T> : ILeasable<T>, IDisposable  where T : class, IRenewable
    {
        /// <summary>
        /// build strategy
        /// </summary>
        private readonly IBuildStrategy<T> buildStrategy = null;

        /// <summary>
        /// renew policy
        /// </summary>
        private readonly IRenewStrategy renewStrategy = null;

        /// <summary>
        /// reference holding the data object
        /// </summary>
        private T dataObject = null;

        /// <summary>
        /// Reference count
        /// </summary>
        private int leaseCount = 0;

        /// <summary>
        /// Sync object to gaurantee no multiple recreation
        /// </summary>
        private Object syncObj = new Object();

        /// <summary>
        /// Getter for lease count
        /// </summary>
        public int LeaseCount => this.leaseCount;

        /// <summary>
        /// Initializing a new lease with the given data object
        /// </summary>
        /// <param name="buildStrategy">build strategy</param>
        /// <param name="renewStrategy">build strategy</param>
        public AutoRenewLeasable(IBuildStrategy<T> buildStrategy, IRenewStrategy renewStrategy)
        {
            if (buildStrategy == null)
            {
                throw new ArgumentNullException(nameof(buildStrategy));
            }

            if (renewStrategy == null)
            {
                throw new ArgumentNullException(nameof(renewStrategy));
            }

            this.buildStrategy = buildStrategy;
            this.renewStrategy = renewStrategy;
        }

        /// <summary>
        /// Acquire a lease
        /// </summary>
        /// <returns>acquired data object</returns>
        public T Acquire()
        {
            lock (this.syncObj)
            {
                // Create the data object if it's not there
                if (this.dataObject == null)
                {
                    Debug.Assert(this.leaseCount == 0);
                    this.dataObject = this.buildStrategy.Build();
                    this.leaseCount = 0;
                }

                this.leaseCount ++;
            }

            return this.dataObject;
        }

        /// <summary>
        /// Release a lease
        /// </summary>
        public int Release()
        {
            Debug.Assert(this.leaseCount > 0);
            
            lock (this.syncObj)
            {
                this.leaseCount --;

                if (this.leaseCount == 0 && this.renewStrategy.ShallRenew(this.dataObject))
                {
                    // If there is no active lease, and it's due for renew, destroy the object.
                    // Renew will happen when it's requested again.
                    Debug.Assert(this.dataObject != null);
                    this.DisposeData();
                }
            }

            return this.leaseCount;
        }

        /// <summary>
        /// Dispose the data object
        /// </summary>
        private void DisposeData()
        {
            if (this.dataObject != null)
            {
                this.dataObject.Dispose();
                this.dataObject = null;
                this.leaseCount = 0;
            }
        }

        #region IDisposable

        /// <summary>
        /// Dispose the data we hold
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
            if (disposing && this.dataObject != null)
            {
                // Called from Dispose, dispose realy
                this.DisposeData();
            }
        }

        #endregion
    }
}