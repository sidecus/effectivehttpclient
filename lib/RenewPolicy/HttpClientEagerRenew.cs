namespace EffectiveHttpClient
{
    using System.Net.Http;

    /// <summary>
    /// Eager renew policy. This policy will enable the same behavior as HttpClient default
    /// </summary>
    /// <typeparam name="HttpClient">http client</typeparam>
    public class HttpClientEagerRenew : IRenewPolicy<HttpClient>
    {
        /// <summary>
        /// Should the HttpClient be renewed
        /// </summary>
        /// <returns>true if yes</returns>
        public bool ShallRenew(HttpClient client)
        {
            return true;
        }
    }
}