namespace EffectiveHttpClient
{
    using System;

    /// <summary>
    /// Interface 
    /// </summary>
    public interface IRenewable : IDisposable
    {
        /// <summary>
        /// Creation time of the object
        /// </summary>
        DateTime CreationTime { get; }

        /// <summary>
        /// Total error count so far
        /// </summary>
        int ErrorCount { get; }
    }
}