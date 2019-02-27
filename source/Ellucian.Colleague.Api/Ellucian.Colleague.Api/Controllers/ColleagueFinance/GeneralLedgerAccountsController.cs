// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using Ellucian.Colleague.Domain.Base.Exceptions;
using System.Net;

namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// Provides access to general ledger objects.
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    [Authorize]
    public class GeneralLedgerAccountsController : BaseCompressedApiController
    {
        private readonly IGeneralLedgerAccountService generalLedgerAccountService;
        private readonly ILogger logger;

        /// <summary>
        /// This constructor initializes the GL account controller.
        /// </summary>
        /// <param name="generalLedgerAccountService">General ledger account service object</param>
        /// <param name="logger">Logger object</param>
        public GeneralLedgerAccountsController(IGeneralLedgerAccountService generalLedgerAccountService, ILogger logger)
        {
            this.generalLedgerAccountService = generalLedgerAccountService;
            this.logger = logger;
        }

        /// <summary>
        /// Retrieves a single general ledger object using the supplied GL account ID.
        /// </summary>
        /// <param name="generalLedgerAccountId">General ledger account ID.</param>
        /// <returns>General ledger account DTO.</returns>
        /// <accessComments>
        /// The user can only access those GL accounts for which they have
        /// GL account security access granted.
        /// </accessComments>
        public async Task<GeneralLedgerAccount> GetAsync(string generalLedgerAccountId)
        {
            try
            {
                return await generalLedgerAccountService.GetAsync(generalLedgerAccountId);
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
                logger.Error(ex.Message);
                throw CreateHttpResponseException("Unable to get the GL account.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Validate a GL account. 
        /// If there is a fiscal year, it will also validate it for that year.
        /// </summary>
        /// <param name="generalLedgerAccountId">GL account ID.</param>
        /// <param name="fiscalYear">Optional; General Ledger fiscal year.</param>
        /// <returns>A <see cref="GlAccountValidationResponse">DTO.</see>/></returns>
        /// <accessComments>
        /// The user can only access those GL accounts for which they have
        /// GL account security access granted.
        /// </accessComments>     
        public async Task<GlAccountValidationResponse> GetGlAccountValidationAsync(string generalLedgerAccountId,
            [FromUri(Name = "fiscalYear")] string fiscalYear)
        {
            try
            {
                return await generalLedgerAccountService.ValidateGlAccountAsync(generalLedgerAccountId, fiscalYear);
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
                throw CreateHttpResponseException("Unable to validate the GL account.", HttpStatusCode.BadRequest);
            }
        }
    }
}