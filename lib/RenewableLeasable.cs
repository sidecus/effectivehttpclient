namespace EffectiveHttpClient
{
    using System;
    using System.Diagnostics;
    using System.Threading;

    /// <summary>
    /// Lease management
    /// </summary>
    /// <typeparam name="T">data object</typeparam>
    public class RenewableLeasable<T> : IRenewableLeasable<T>, IDisposable  where T : class, IDisposable
    {
        /// <summary>
        /// is this object disposed.
        /// </summary>
        private bool disposed = false;

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
        private Object syncObj;

        /// <summary>
        /// Getter for lease count
        /// </summary>
        public int LeaseCount => this.leaseCount;

        /// <summary>
        /// Initializing a new lease with the given data object
        /// </summary>
        /// <param name="dataObject">data object</param>
        public RenewableLeasable(T dataObject)
        {
            if (dataObject == null)
            {
                throw new ArgumentNullException(nameof(dataObject));
            }

            this.syncObj = new Object();
            this.dataObject = dataObject;
        }

        /// <summary>
        /// Acquire a lease
        /// </summary>
        /// <returns>acquired data object</returns>
        public T Acquire()
        {
            lock (this.syncObj)
            {
                this.leaseCount ++;
            }
            return this.dataObject;
        }

        /// <summary>
        /// Release a lease
        /// </summary>
        public int Release()
        {
            lock (this.syncObj)
            {
                this.leaseCount --;
            }

            return this.leaseCount;
        }

        /// <summary>
        /// Recreate the lease and acquire - thread safe.
        /// No gauranee to really recreate. Data object might be "stale" if there is racing condition.
        /// </summary>
        /// <param name="dataFactory">data Factory</param>
        /// <returns>acquired data object and lease count</returns>
        public T RenewAndAcquire(Func<T> dataFactory)
        {
            if (dataFactory == null)
            {
                throw new ArgumentNullException(nameof(dataFactory));
            }

            var renewed = false;

            // Try to recreate
            if (this.leaseCount == 0)
            {
                lock(this.syncObj)
                {
                    if (this.leaseCount == 0)
                    {
                        // Dispose the data first
                        this.DisposeData();

                        // Create new data for lease
                        this.dataObject = dataFactory();
                        this.leaseCount = 0;
                        renewed = true;
                    }
                }
            }

            if (!renewed)
            {
                Trace.WriteLine("RenewAndAcquire could not renew. Returned data might be 'stale'.");
            }

            // Acquire regardless it's renewed or not
            return this.Acquire();
        }

        /// <summary>
        /// Dispose the data object
        /// </summary>
        private void DisposeData()
        {
            if (this.dataObject != null)
            {
                this.dataObject.Dispose();
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