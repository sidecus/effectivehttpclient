namespace EffectiveHttpClient
{
    /// <summary>
    /// Interface for Renew policy
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRenewPolicy<T> where T: class
    {
        /// <summary>
        /// Should we renew now?
        /// </summary>
        /// <param name="data">data object</param>
        /// <returns>true if yes</returns>
        bool ShallRenew(T data);
    }
}