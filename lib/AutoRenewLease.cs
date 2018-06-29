namespace EffectiveHttpClient
{
    using System;
    using System.Diagnostics;
    using System.Threading;

    /// <summary>
    /// Auto renewal lease with "passive" life time management
    /// </summary>
    public sealed class AutoRenewLease<T> : IDisposable where T : class
    {
        /// <summary>
        /// Disposed mark
        /// </summary>
        private bool disposed = false;

        /// <summary>
        /// The lease object
        /// </summary>
        private IRenewableLeasable<T> leasable;

        /// <summary>
        /// Data object which holds the reference once leased.
        /// </summary>
        public T DataObject { get; private set; }

        /// <summary>
        /// Initializes a new LifeTimeWrapper
        /// </summary>
        /// <param name="leasable">lease object</param>
        /// <param name="dataFactory">data factory in case we need to renew</param>
        public AutoRenewLease(IRenewableLeasable<T> leasable, Func<T> dataFactory)
        {
            if (leasable == null)
            {
                throw new ArgumentNullException(nameof(leasable));
            }

            if (dataFactory == null)
            {
                throw new ArgumentNullException(nameof(dataFactory));
            }

            this.leasable = leasable;

            // Try to acquire a lease on the leasable object, and renew if necessary
            this.DataObject = this.AcquireOrRenew(dataFactory);
        }

        /// <summary>
        /// Acquire the data or renew with the given factory
        /// </summary>
        /// <param name="dataFactory">data factory in case we need to renew</param>
        private T AcquireOrRenew(Func<T> dataFactory)
        {
            if (this.leasable == null)
            {
                throw new InvalidOperationException("leasable already disposed.");
            }

            T data = null;

            // TODO - correct renew strategy - now we renew when there is no lease
            if (this.leasable.LeaseCount == 0)
            {
                data = this.leasable.RenewAndAcquire(dataFactory);
            }
            else
            {
                data = this.leasable.Acquire();
                Debug.Assert(this.leasable.LeaseCount > 0);
            }

            return data;
        }

        #region IDisposable

        /// <summary>
        /// Dispose the lease. No need to implement the dispose pattern since this just release the lease and is sealed
        /// </summary>
        public void Dispose()
        {
            if (this.leasable != null)
            {
                this.leasable.Release();
                this.leasable = null;
            }
        }

        #endregion
    }
}