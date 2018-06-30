namespace EffectiveHttpClient
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Eager renew policy. Returns true whenever being asked
    /// </summary>
    public class HttpClientRenewStrategy : IRenewStrategy<RenewableHttpClient>
    {
        /// <summary>
        /// Chain of factory methods which will be used to initialize an HttpClient
        /// </summary>
        private IList<Predicate<IRenewable>> predicateChain = new List<Predicate<IRenewable>>();

        /// <summary>
        /// Initializes a new HttpClientRenewStrategy
        /// </summary>
        public HttpClientRenewStrategy()
        {
        }

        /// <summary>
        /// Should the renewable be renewed
        /// </summary>
        /// <returns>true if yes</returns>
        public bool ShallRenew(RenewableHttpClient renewable)
        {
            if (this.predicateChain.Count == 0)
            {
                // Default to eager renew - whenever asked
                return true;
            }

            foreach (var predicate in this.predicateChain)
            {
                // If any predicate says yes, we need to renew
                if (predicate(renewable))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Use age strategy. Object needs to be renewed if it's age it's older than ageLimit
        /// </summary>
        /// <param name="ageLimit">age limit</param>
        public HttpClientRenewStrategy UseAgeStrategy(TimeSpan ageLimit)
        {
            this.predicateChain.Add(x =>
            {
                var age = DateTime.UtcNow - x.CreationTime;
                return age > ageLimit;
            });

            return this;
        }

        /// <summary>
        /// Use error strategy. Object needs to be renewed if it has seen number of errors more than errorLimit
        /// </summary>
        /// <param name="errorLimit">max number of errors</param>
        public HttpClientRenewStrategy UseErrorStrategy(int errorLimit)
        {
            this.predicateChain.Add(x =>
            {
                return x.ErrorCount > errorLimit;
            });

            return this;
        }
    }
}