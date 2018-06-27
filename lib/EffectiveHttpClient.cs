namespace EffectiveHttpClient
{
    using System;

    /// <summary>
    /// Specialized EffectiveHttpClient which uses base address as the key
    /// </summary>
    /// <typeparam name="string"></typeparam>
    public class EffectiveHttpClient : EffectiveHttpClient<string>
    {
        /// <summary>
        /// Normalize a uri to a schema+domain+port key
        /// </summary>
        /// <param name="uri">uri</param>
        /// <returns>string key which can be used to identify unique host</returns>
        private static string UriToKey(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            // Normalize the schema, domain and port part so that we can ues it as key
            return $"{uri.Scheme}://{uri.Host.ToLowerInvariant()}:{uri.Port}";
        }

        /// <summary>
        /// Initializes a new EffectiveHttpClient with the base address, with no special client initialization
        /// </summary>
        /// <param name="baseAddress">base address</param>
        public EffectiveHttpClient(Uri baseAddress)
            : base(EffectiveHttpClient.UriToKey(baseAddress), new ClientBuildStrategy(baseAddress))
        {
        }

        /// <summary>
        /// Initializes a new EffectiveHttpClient with a client build strategy
        /// </summary>
        /// <param name="strategy">client build strategy</param>
        public EffectiveHttpClient(ClientBuildStrategy strategy)
            : base(EffectiveHttpClient.UriToKey(strategy.BaseAddress), strategy)
        {
        }
    }
}