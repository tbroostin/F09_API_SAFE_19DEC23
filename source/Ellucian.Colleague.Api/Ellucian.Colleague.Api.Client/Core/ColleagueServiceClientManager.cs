// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Concurrent;
using slf4net;

namespace Ellucian.Colleague.Api.Client.Core
{
    /// <summary>
    /// Provides manager access to the ColleagueServiceClient objects that are instantiated for
    /// each Web API base URL that is used for a given instance
    /// </summary>
    public sealed class ColleagueServiceClientManager
    {
        private static volatile ColleagueServiceClientManager instance;
        private static object lockObject = new Object();
        private static ConcurrentDictionary<string, ColleagueServiceClient> serviceClientCollection = new ConcurrentDictionary<string, ColleagueServiceClient>();

        private ColleagueServiceClientManager() { }

        /// <summary>
        /// Gets the singleton instance of the ColleagueServiceClientManager
        /// </summary>
        /// <value>
        /// The instance of the object.
        /// </value>
        public static ColleagueServiceClientManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockObject)
                    {
                        if (instance == null)
                        {
                            instance = new ColleagueServiceClientManager();
                        }
                    }
                }

                return instance;
            }
        }

        /// <summary>
        /// Gets the colleague service client for a given Web API base URL
        /// </summary>
        /// <param name="baseUrl">The base Web API URL.</param>
        /// <param name="maxConnections">The maximum connections allowed for this base URL.</param>
        /// <param name="logger">The logger.</param>
        /// <returns></returns>
        internal ColleagueServiceClient GetColleagueServiceClient(string baseUrl, int maxConnections, ILogger logger)
        {
            string serviceClientKey = ConvertUrlToKey(baseUrl);
            ColleagueServiceClient serviceClient;

            if (serviceClientCollection.ContainsKey(serviceClientKey))
            {
                // Instance of the service client for this URL already exists; use it
                serviceClient = serviceClientCollection[serviceClientKey];
            }
            else
            {
                // Instance of the service client for this URL does not yet exist; try to get a lock so we can instantiate it
                lock (lockObject)
                {
                    // Verify one more time that the key still doesn't exist (now that we have the lock)
                    if (serviceClientCollection.ContainsKey(serviceClientKey))
                    {
                        // Instance of the service client for this URL was created while trying to acquire the lock; so, now we can use it
                        serviceClient = serviceClientCollection[serviceClientKey];
                    }
                    else
                    {
                        // Instantiate new service client for this URL
                        ColleagueServiceClient newServiceClient = new ColleagueServiceClient(baseUrl, logger);
                        newServiceClient.MaxConnections = maxConnections;

                        // Add the service client to the concurrent dictionary
                        serviceClientCollection[serviceClientKey] = newServiceClient;

                        serviceClient = newServiceClient;
                    }
                }
            }

            return serviceClient;
        }

        /// <summary>
        /// Removes the Colleague Service Client from the collection per the base URL to be removed.
        /// </summary>
        /// <param name="baseUrl">The base URL to remove from the Colleague Service Client collection.</param>
        public void RemoveServiceClient(string baseUrl)
        {
            string serviceClientKey = ConvertUrlToKey(baseUrl);
            lock (lockObject)
            {
                if (serviceClientCollection.ContainsKey(serviceClientKey))
                {
                    // Remove the ColleagueServiceClient object from the collection
                    ColleagueServiceClient serviceClient;
                    serviceClientCollection.TryRemove(serviceClientKey, out serviceClient);

                    if (serviceClient != null)
                    {
                        // Dispose of the ColleagueServiceClient object
                        serviceClient.Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// Removes all Colleague Service Clients from the collection.
        /// </summary>
        public void RemoveAllServiceClients()
        {
            lock (lockObject)
            {
                // Iterate over the collection to dispose of all ColleagueServiceClient objects
                foreach (var serviceClientEntry in serviceClientCollection)
                {
                    serviceClientEntry.Value.Dispose();
                }

                // Clear the collection
                serviceClientCollection.Clear();
            }
        }

        /// <summary>
        /// Converts the URL to a key to use for the concurrent dictionary of Colleague Service Client objects.
        /// </summary>
        /// <param name="baseUrl">The base URL.</param>
        /// <returns></returns>
        private string ConvertUrlToKey(string baseUrl)
        {
            // Strip out the trailing slash from the URL if it exists
            return baseUrl.Trim().TrimEnd('/');
        }
    }
}
