// Copyright 2019-2021 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// This is the controller for GL finance query.
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    [Authorize]
    public class FinanceQueryController : BaseCompressedApiController
    {
        private readonly IFinanceQueryService financeQueryService;
        private readonly ILogger logger;

        /// <summary>
        /// This constructor initializes the GL Finance query service object.
        /// </summary>
        /// <param name="financeQueryService">GL Finance query service object</param>
        /// <param name="logger">Logger object</param>
        public FinanceQueryController(IFinanceQueryService financeQueryService, ILogger logger)
        {
            this.financeQueryService = financeQueryService;
            this.logger = logger;
        }

        /// <summary>
        /// Retrieves the filtered GL Accounts list
        /// </summary>
        /// <param name="criteria">Finance query filter criteria.</param>
        /// <returns>GL accounts that match the filter criteria.</returns>
        /// <accessComments>
        /// The user can only access those GL accounts for which they have
        /// GL account security access granted.
        /// </accessComments>
        [HttpPost]
        public async Task<IEnumerable<FinanceQuery>> QueryFinanceQuerySelectionByPostAsync([FromBody]FinanceQueryCriteria criteria)
        {
            try
            {
                if (criteria == null)
                {
                    throw new ArgumentNullException("criteria", "The query criteria must be specified.");
                }                

                return await financeQueryService.QueryFinanceQuerySelectionByPostAsync(criteria);
            }
            catch (ArgumentNullException anex)
            {
                logger.Error(anex, anex.Message);
                throw CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                throw CreateHttpResponseException("Unable to get finance query results", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves the filtered GL Account detail data.
        /// </summary>
        /// <param name="criteria">Finance query filter criteria.</param>
        /// <returns>GL account data that match the filter criteria.</returns>
        /// <accessComments>
        /// The user can only access those GL accounts for which they have
        /// GL account security access granted.
        /// </accessComments>
        [HttpPost]
        public async Task<IEnumerable<FinanceQueryActivityDetail>> QueryFinanceQueryDetailSelectionByPostAsync([FromBody] FinanceQueryCriteria criteria)
        {
            try
            {
                if (criteria == null)
                {
                    throw new ArgumentNullException("criteria", "The query criteria must be specified.");
                }

                return await financeQueryService.QueryFinanceQueryDetailSelectionByPostAsync(criteria);
            }
            catch (ArgumentNullException anex)
            {
                logger.Error(anex, anex.Message);
                throw CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                throw CreateHttpResponseException("Unable to get finance query detail results.", HttpStatusCode.BadRequest);
            }
        }
    }
}

