namespace EffectiveHttpClient
{
    /// <summary>
    /// Interface for Renew policy. Renew policy 
    /// </summary>
    /// <typeparam name="T">The renewable object</typeparam>
    public interface IRenewStrategy<T> where T: class, IRenewable
    {
        /// <summary>
        /// Should we renew now?
        /// </summary>
        /// <param name="data">data object</param>
        /// <returns>true if yes</returns>
        bool ShallRenew(T data);
    }
}