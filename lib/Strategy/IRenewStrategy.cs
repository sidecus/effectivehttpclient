namespace EffectiveHttpClient
{
    /// <summary>
    /// Interface for Renew policy. Renew policy 
    /// </summary>
    public interface IRenewStrategy
    {
        /// <summary>
        /// Should we renew now?
        /// </summary>
        /// <param name="data">data object</param>
        /// <returns>true if yes</returns>
        bool ShallRenew(IRenewable data);
    }
}