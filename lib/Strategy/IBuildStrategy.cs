namespace EffectiveHttpClient
{
    /// <summary>
    /// Interface for Renew strategy
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IBuildStrategy<T> where T: class, IRenewable
    {
        /// <summary>
        /// Build the object
        /// </summary>
        T Build();
    }
}