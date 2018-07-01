namespace EffectiveHttpClient
{
    using System;
    using System.Diagnostics;
    using System.Threading;

    /// <summary>
    /// Auto renewal lease with "passive" life time management
    /// </summary>
    public sealed class AutoLease<T> : IDisposable where T : class
    {
        /// <summary>
        /// The lease object
        /// </summary>
        private ILeasable<T> leasable = null;

        /// <summary>
        /// Data object which holds the reference once leased.
        /// </summary>
        public T DataObject { get; private set; }

        /// <summary>
        /// Initializes a new LifeTimeWrapper
        /// </summary>
        /// <param name="leasable">lease object</param>
        public AutoLease(ILeasable<T> leasable)
        {
            if (leasable == null)
            {
                throw new ArgumentNullException(nameof(leasable));
            }

            this.leasable = leasable;

            // Try to acquire a lease on the leasable object, and renew if necessary
            this.DataObject = this.leasable.Acquire();
        }

        #region IDisposable

        /// <summary>
        /// Dispose the lease. No need to implement the dispose pattern since this just release the lease and is sealed
        /// </summary>
        public void Dispose()
        {
            if (this.leasable != null)
            {
                Debug.Assert(this.DataObject != null);
                
                this.leasable.Release();
                this.leasable = null;
                this.DataObject = null;
            }
        }

        #endregion
    }
}