namespace EffectiveHttpClient
{
    using System;
    using System.Threading;

    /// <summary>
    /// Lease management
    /// </summary>
    /// <typeparam name="T">lease object</typeparam>
    public class Lease<T> where T : class
    {
        /// <summary>
        /// reference holding the data object
        /// </summary>
        private T dataObject = null;

        /// <summary>
        /// Reference count
        /// </summary>
        private int leaseCount = 0;

        /// <summary>
        /// Getter for lease count
        /// </summary>
        public int LeaseCount => this.leaseCount;

        /// <summary>
        /// Initializing a new lease with the given data object
        /// </summary>
        /// <param name="dataObject">object for lease</param>
        public Lease(T dataObject)
        {
            if (dataObject == null)
            {
                throw new ArgumentNullException(nameof(dataObject));
            }

            this.dataObject = dataObject;
        }

        /// <summary>
        /// Acquire a lease
        /// </summary>
        /// <returns></returns>
        public (T, int) Acquire()
        {
            var count = Interlocked.Increment(ref this.leaseCount);
            return (this.dataObject, count);
        }

        /// <summary>
        /// Release a lease
        /// </summary>
        public int Release()
        {
            return Interlocked.Decrement(ref this.leaseCount);
        }
    }
}