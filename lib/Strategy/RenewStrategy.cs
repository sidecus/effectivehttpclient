namespace EffectiveHttpClient
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Eager renew policy. Returns true whenever being asked
    /// </summary>
    public class RenewStrategy : IRenewStrategy
    {
        /// <summary>
        /// Chain of factory methods which will be used to initialize an HttpClient
        /// </summary>
        private IList<Predicate<IRenewable>> predicateChain = new List<Predicate<IRenewable>>();

        /// <summary>
        /// Initializes a new RenewStrategy
        /// </summary>
        public RenewStrategy()
        {
        }

        /// <summary>
        /// Should the renewable be renewed
        /// </summary>
        /// <returns>true if yes</returns>
        public virtual bool ShallRenew(IRenewable renewable)
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
        public virtual IRenewStrategy UseAgeStrategy(TimeSpan ageLimit)
        {
            this.predicateChain.Add(x =>
            {
                return x.Age >= ageLimit;
            });

            return this;
        }

        /// <summary>
        /// Use error strategy. Object needs to be renewed if it has seen number of errors more than errorLimit
        /// </summary>
        /// <param name="errorLimit">max number of errors</param>
        public virtual IRenewStrategy UseErrorStrategy(int errorLimit)
        {
            this.predicateChain.Add(x =>
            {
                return x.ErrorCount > errorLimit;
            });

            return this;
        }

        /// <summary>
        /// Use "usage" based strategy. Object needs to be renewed if it's been leased more than usageLimit times
        /// </summary>
        /// <param name="usageLimit">max usage till renew</param>
        public virtual IRenewStrategy UseUsageStrategy(int usageLimit)
        {
            this.predicateChain.Add(x =>
            {
                return x.UsageCount > usageLimit;
            });

            return this;
        }
    }
}