// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Data.Colleague.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Data.Colleague;
using Ellucian.Web.Cache;
using slf4net;
using Ellucian.Web.Dependency;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Configuration;

namespace Ellucian.Colleague.Data.Base.Repositories
{
    /// <summary>
    /// Authentication Scheme Repository
    /// </summary>
    [RegisterType]
    public class AuthenticationSchemeRepository : BaseColleagueRepository, IAuthenticationSchemeRepository
    {
        private IColleagueTransactionInvoker anonymousTransactionInvoker;
        /// <summary>
        /// Authentication Scheme Repository constructor
        /// </summary>
        /// <param name="cacheProvider"></param>
        /// <param name="transactionFactory"></param>
        /// <param name="logger"></param>
        /// <param name="colleagueSettings"></param>
        public AuthenticationSchemeRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ColleagueSettings colleagueSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            // transactionInvoker will only be non-null when a user is logged in
            // If this is being requested anonymously, create a transaction invoker 
            // without any user context.
            anonymousTransactionInvoker = transactionInvoker ?? new ColleagueTransactionInvoker(null, null, logger, colleagueSettings.DmiSettings);
        }

        /// <summary>
        /// Gets the authentication scheme associated with the given username.
        /// </summary>
        /// <param name="username">User name</param>
        /// <returns>The authentication scheme the username is subject to. Null if the username does not have a defined authentication scheme.</returns>
        public async Task<AuthenticationScheme> GetAuthenticationSchemeAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentNullException("username");
            }

            var authSchemeRequest = new GetAuthenticationSchemeRequest()
            {
                Username = username
            };
            var authSchemeResponse = await anonymousTransactionInvoker.ExecuteAnonymousAsync<GetAuthenticationSchemeRequest, GetAuthenticationSchemeResponse>(authSchemeRequest);
            if (authSchemeResponse != null)
            {
                // A blank or 0 ErrorOccurred is a success
                if (string.IsNullOrWhiteSpace(authSchemeResponse.ErrorOccurred) || authSchemeResponse.ErrorOccurred == "0")
                {
                    if (string.IsNullOrEmpty(authSchemeResponse.AuthenticationScheme))
                    {
                        return null;
                    }
                    return new AuthenticationScheme(authSchemeResponse.AuthenticationScheme);
                }
                else
                {
                    logger.Error(string.Format("Error occurred when attempting to retrieve authentication scheme for user: {0}. Details: ({1}) {2}", username, authSchemeResponse.ErrorOccurred, authSchemeResponse.ErrorMessage));
                    throw new ApplicationException("Authentication scheme transaction returned an error.");
                }
            }
            else
            {
                logger.Error(string.Format("Authentication scheme transaction returned no response when searching for user: {0}", username));
                throw new ApplicationException("Authentication scheme transaction returned no response.");
            }
        }
    }
}
