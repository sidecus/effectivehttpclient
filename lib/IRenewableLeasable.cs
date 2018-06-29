namespace EffectiveHttpClient
{
    using System;

    /// <summary>
    /// Interface for renewable leasable object
    /// </summary>
    /// <typeparam name="T">type object</typeparam>
    public interface IRenewableLeasable<T> where T : class
    {
        /// <summary>
        /// Recreate and acquire lease
        /// </summary>
        /// <returns>recreate and acquire new lease</returns>
        T RenewAndAcquire(Func<T> dataFactory);

        /// <summary>
        /// Acquire lease
        /// </summary>
        /// <returns>new lease</returns>
        T Acquire();

        /// <summary>
        /// Release the lease
        /// </summary>
        int Release();

        /// <summary>
        /// Lease count
        /// </summary>
        /// <returns></returns>
        int LeaseCount { get; }
    }
}