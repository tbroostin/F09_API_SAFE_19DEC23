// Copyright 2021 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using slf4net;
using Ellucian.Web.Dependency;
using Ellucian.Colleague.Domain.Exceptions;

namespace Ellucian.Colleague.Data.Base.Repositories
{
    [RegisterType]
    public class EedmCacheKeysRepository : BaseColleagueRepository, IEedmCacheKeysRepository
    {
        private RepositoryException exception;

        public EedmCacheKeysRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using level 1 cache time out value for data that rarely changes.
            CacheTimeout = Level1CacheTimeoutValue;
            exception = new RepositoryException();
        }

        /// <summary>
        /// Clear EEDM cache keys
        /// </summary>
        public void ClearEedmCacheKeys()
        {
            //var request = new ClearApiCacheKeysRequest();
            //var response = transactionInvoker.ExecuteAsync<ClearApiCacheKeysRequest, ClearApiCacheKeysResponse>(request);
            transactionInvoker.ExecuteAsync<ClearApiCacheKeysRequest, ClearApiCacheKeysResponse>(new ClearApiCacheKeysRequest());
        }
    }
}