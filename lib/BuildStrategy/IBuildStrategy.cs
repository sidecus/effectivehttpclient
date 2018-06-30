namespace EffectiveHttpClient
{
    /// <summary>
    /// Interface for Renew strategy
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IBuildStrategy<T> where T: class
    {
        /// <summary>
        /// Build the object
        /// </summary>
        T Build();
    }
}