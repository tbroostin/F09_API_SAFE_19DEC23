// Copyright 2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// This is the controller for GL account balances.
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    [Authorize]
    public class GlAccountBalancesController : BaseCompressedApiController
    {
        private readonly IGlAccountBalancesService glAccountBalancesService;
        private readonly ILogger logger;

        /// <summary>
        /// GlAccountBalancesController class constructor.
        /// </summary>
        /// <param name="glAccountBalancesService">GL account balances service object.</param>
        /// <param name="logger">Logger object.</param>
        public GlAccountBalancesController(IGlAccountBalancesService glAccountBalancesService, ILogger logger)
        {
            this.glAccountBalancesService = glAccountBalancesService;
            this.logger = logger;
        }

        /// <summary>
        /// Retrieves balances for the list of GL accounts and the fiscal year.
        /// </summary>
        /// <param name="criteria"><see cref="GlAccountBalancesQueryCriteria">Query criteria</see>includes the GL account and the fiscal year for the query.</param>
        /// <returns>List of GL account balances DTOs for the specified GL accounts for the specified fiscal year.</returns>
        /// <accessComments>
        /// The user can only access transactions for a GL account for which they have
        /// GL account security access granted and Requires at least one of the permissions CREATE.UPDATE.REQUISITION, CREATE.UPDATE.PURCHASE.ORDER, CREATE.UPDATE.VOUCHER
        /// </accessComments>
        [HttpPost]
        public async Task<IEnumerable<GlAccountBalances>> QueryGlAccountBalancesAsync([FromBody]GlAccountBalancesQueryCriteria criteria)
        {
            try
            {
                if (criteria == null)
                {
                    throw new ArgumentNullException("criteria", "The query criteria must be specified.");
                }

                if (string.IsNullOrEmpty(criteria.FiscalYear))
                {
                    throw new ArgumentNullException("FiscalYear", "A fiscal year must be specified.");
                }

                if (criteria.GlAccounts == null || !criteria.GlAccounts.Any())
                {
                    throw new ArgumentNullException("GlAccount", "A GL account must be specified.");
                }

                return await glAccountBalancesService.QueryGlAccountBalancesAsync(criteria.GlAccounts, criteria.FiscalYear);
            }
            catch (PermissionsException peex)
            {
                logger.Error(peex.Message);
                throw CreateHttpResponseException("Insufficient permissions to access the GL account balances.", HttpStatusCode.Forbidden);
            }
            catch (ConfigurationException cnex)
            {
                logger.Error(cnex, cnex.Message);
                throw CreateHttpResponseException("Invalid configuration.", HttpStatusCode.NotFound);
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
                throw CreateHttpResponseException("Unable to get balances for the GL account.", HttpStatusCode.BadRequest);
            }
        }
    }
}
