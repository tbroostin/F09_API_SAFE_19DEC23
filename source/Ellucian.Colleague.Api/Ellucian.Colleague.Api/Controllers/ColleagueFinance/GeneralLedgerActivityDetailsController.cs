// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.

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
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// This is the controller for GL activity details.
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    [Authorize]
    public class GeneralLedgerActivityDetailsController : BaseCompressedApiController
    {
        private readonly IGeneralLedgerActivityDetailService generalLedgerActivityDetailsService;
        private readonly ILogger logger;

        /// <summary>
        /// GeneralLedgerActivityDetailsController class constructor.
        /// </summary>
        /// <param name="generalLedgerActivityDetailsService">GL activity details service object.</param>
        /// <param name="logger">Logger object.</param>
        public GeneralLedgerActivityDetailsController(IGeneralLedgerActivityDetailService generalLedgerActivityDetailsService, ILogger logger)
        {
            this.generalLedgerActivityDetailsService = generalLedgerActivityDetailsService;
            this.logger = logger;
        }

        /// <summary>
        /// Retrieves all the actuals and encumbrances activity detail for the GL account and the fiscal year.
        /// </summary>
        /// <param name="criteria"><see cref="GlActivityDetailQueryCriteria">Query criteria</see>includes the GL account and the fiscal year for the query.</param>
        /// <returns>List of GL activity detail DTOs for the specified GL account for the specified fiscal year.</returns>
        /// <accessComments>
        /// The user can only access transactions for a GL account for which they have
        /// GL account security access granted.
        /// </accessComments>
        [HttpPost]
        public async Task<GlAccountActivityDetail> QueryGeneralLedgerActivityDetailsByPostAsync([FromBody]GlActivityDetailQueryCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria", "The query criteria must be specified.");
            }

            if (string.IsNullOrEmpty(criteria.GlAccount))
            {
                throw new ArgumentNullException("GlAccount", "A GL account must be specified.");
            }

            if (string.IsNullOrEmpty(criteria.FiscalYear))
            {
                throw new ArgumentNullException("FiscalYear", "A fiscal year must be specified.");
            }

            try
            {
                return await generalLedgerActivityDetailsService.QueryGlAccountActivityDetailAsync(criteria.GlAccount, criteria.FiscalYear);
            }
            catch (PermissionsException peex)
            {
                logger.Error(peex.Message);
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
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
                throw CreateHttpResponseException("Unable to get activity for the GL account.", HttpStatusCode.BadRequest);
            }
        }
    }
}
