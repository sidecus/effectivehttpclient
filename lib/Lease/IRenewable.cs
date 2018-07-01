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
        TimeSpan Age { get; }

        /// <summary>
        /// Total error count so far
        /// </summary>
        int ErrorCount { get; }
    }
}