// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using System;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// General Ledger Configuration controller.
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    [Authorize]
    public class GeneralLedgerConfigurationController : BaseCompressedApiController
    {
        private readonly IGeneralLedgerConfigurationService glConfigurationService;
        private readonly ILogger logger;

        /// <summary>
        /// This constructor initializes the General Ledger Configuration controller.
        /// </summary>
        /// <param name="glConfigurationService">General Ledger Configuration service object.</param>
        /// <param name="logger">Logger object.</param>
        public GeneralLedgerConfigurationController(IGeneralLedgerConfigurationService glConfigurationService, ILogger logger)
        {
            this.glConfigurationService = glConfigurationService;
            this.logger = logger;
        }

        /// <summary>
        /// Returns the General Ledger configuration.
        /// </summary>
        /// <returns>The General Ledger configuration.</returns>
        /// <accessComments>
        /// No permission is needed.
        /// </accessComments>
        public async Task<GeneralLedgerConfiguration> GetGeneralLedgerConfigurationAsync()
        {
            try
            {
                // Call the service method to obtain necessary GL configuration parameters.
                var glConfiguration = await glConfigurationService.GetGeneralLedgerConfigurationAsync();
                return glConfiguration;
            }
            catch (ConfigurationException cnex)
            {
                logger.Error(cnex, cnex.Message);
                throw CreateHttpResponseException("Invalid configuration.", HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                throw CreateHttpResponseException("Unable to get the General Ledger configuration.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get configuration information necessary to validate user input in a budget adjustment.
        /// </summary>
        /// <returns>BudgetAdjustmentConfiguration DTO</returns>
        /// <accessComments>
        /// No permission is needed.
        /// </accessComments> 
        public async Task<BudgetAdjustmentConfiguration> GetBudgetAdjustmentConfigurationAsync()
        {
            try
            {
                return await glConfigurationService.GetBudgetAdjustmentConfigurationAsync();
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
                throw CreateHttpResponseException("Unable to get the budget adjustment configuration.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get indicator that determines whether budget adjustments are turned on or off.
        /// </summary>
        /// <returns>BudgetAdjustmentEnabled DTO</returns>
        /// <accessComments>
        /// No permission is needed.
        /// </accessComments>
        public async Task<BudgetAdjustmentsEnabled> GetBudgetAdjustmentsEnabledAsync()
        {
            try
            {
                return await glConfigurationService.GetBudgetAdjustmentEnabledAsync();
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                throw CreateHttpResponseException("Unable to get the budget adjustment configuration.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get fiscal year configuration information necessary to validate fiscal year dates used in finance query.
        /// </summary>
        /// <returns>GlFiscalYearConfiguration DTO</returns>
        /// <accessComments>
        /// No permission is needed.
        /// </accessComments> 
        public async Task<GlFiscalYearConfiguration> GetGlFiscalYearConfigurationAsync()
        {
            try
            {
                return await glConfigurationService.GetGlFiscalYearConfigurationAsync();
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
                throw CreateHttpResponseException("Unable to get the gl fiscal year configuration.", HttpStatusCode.BadRequest);
            }
        }
    }
}
