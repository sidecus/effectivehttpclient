namespace EffectiveHttpClient
{
    using System;

    /// <summary>
    /// Interface for renewable leasable object
    /// </summary>
    /// <typeparam name="T">type object</typeparam>
    public interface ILeasable<T> where T : class
    {
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