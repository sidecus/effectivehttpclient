namespace EffectiveHttpClient
{
    using System;
    using System.Diagnostics;
    using System.Threading;

    /// <summary>
    /// Lease management
    /// </summary>
    /// <typeparam name="T">data object</typeparam>
    public class AutoRenewLeasable<T> : ILeasable<T>, IDisposable  where T : class, IDisposable
    {
        /// <summary>
        /// build strategy
        /// </summary>
        private readonly IBuildStrategy<T> buildStrategy = null;

        /// <summary>
        /// renew policy
        /// </summary>
        private readonly IRenewPolicy<T> renewPolicy = null;

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
        /// is this object disposed.
        /// </summary>
        private bool disposed = false;

        /// <summary>
        /// Getter for lease count
        /// </summary>
        public int LeaseCount => this.leaseCount;

        /// <summary>
        /// Initializing a new lease with the given data object
        /// </summary>
        /// <param name="buildStrategy">build strategy</param>
        /// <param name="renewPolicy">build strategy</param>
        public AutoRenewLeasable(IBuildStrategy<T> buildStrategy, IRenewPolicy<T> renewPolicy)
        {
            if (buildStrategy == null)
            {
                throw new ArgumentNullException(nameof(buildStrategy));
            }

            if (renewPolicy == null)
            {
                throw new ArgumentNullException(nameof(renewPolicy));
            }

            this.buildStrategy = buildStrategy;
            this.renewPolicy = renewPolicy;
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

                // If there is no active lease, and it's due to renew, destroy the object.
                // Renew will happen when it's requested again.
                if (this.leaseCount == 0 && this.renewPolicy.ShallRenew(this.dataObject))
                {
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
                this.DisposeData();
            }

            this.disposed = true;
        }

        #endregion
    }
}