namespace EffectiveHttpClient
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net.Http;
    using System.Net.Http.Headers;

    /// <summary>
    /// Http client build strategy. This class provides fluent helper methods to help build HttpClient.
    /// </summary>
    public class HttpClientBuildStrategy : IBuildStrategy<HttpClient>
    {
        /// <summary>
        /// Default value factory
        /// </summary>
        private static readonly Func<HttpClient> DefaultClientFactory = () => new HttpClient();

        /// <summary>
        /// Reference to the base address
        /// </summary>
        public Uri BaseAddress { get; }

        /// <summary>
        /// Chain of factory methods which will be used to initialize an HttpClient
        /// </summary>
        private IList<Func<HttpClient, HttpClient>> factoryChain = new List<Func<HttpClient, HttpClient>>();

        /// <summary>
        /// Create a ClientBuildStrategy which does "new HttpClient()" by default
        /// <param name="baseAddress">Uri for the baseAddress, mandatory</param>
        /// </summary>
        public HttpClientBuildStrategy(Uri baseAddress)
            : this (baseAddress, HttpClientBuildStrategy.DefaultClientFactory)
        {
        }

        /// <summary>
        /// Create a ClientBuildStrategy which uses the passed in factory method to create the client
        /// <param name="baseAddress">Uri for the baseAddress, mandatory</param>
        /// <param name="creationFactory">Factory method to create HttpClient</param>
        /// </summary>
        public HttpClientBuildStrategy(Uri baseAddress, Func<HttpClient> creationFactory)
        {
            if (baseAddress == null)
            {
                throw new ArgumentNullException(nameof(baseAddress));
            }

            if (creationFactory == null)
            {
                throw new ArgumentNullException(nameof(creationFactory));
            }

            this.BaseAddress = baseAddress;

            // Add client creation and base address setting to the chain
            this.factoryChain.Add(x =>
            {
                // The passed http client must be null here
                Debug.Assert(x == null);

                // Create http client with base address
                var client = creationFactory();
                client.BaseAddress = baseAddress;
                return client;
            });
        }

        /// <summary>
        /// Tell the strategy object that some default headers need to be set
        /// </summary>
        /// <param name="headerAction">action to set default headers</param>
        /// <returns>ClientBuildStrategy object</returns>
        public HttpClientBuildStrategy UseDefaultHeaders(Action<HttpRequestHeaders> headerAction)
        {
            if (headerAction == null)
            {
                throw new ArgumentNullException(nameof(headerAction));
            }

            Debug.Assert(this.factoryChain != null);

            // Append header action
            this.factoryChain.Add(x =>
            {
                headerAction(x.DefaultRequestHeaders);
                return x;
            });

            return this;
        }

        /// <summary>
        /// Use the current strategy to build the client
        /// </summary>
        /// <returns>Http client object</returns>
        public HttpClient Build()
        {
            HttpClient client = null;
            foreach (var factoryMethod in this.factoryChain)
            {
                client = factoryMethod(client);
            }

            return client;
        }
    }
}