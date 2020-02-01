// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.BudgetManagement.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Dtos.BudgetManagement;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using System;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.BudgetManagement
{
    /// <summary>
    /// Budget Development Configuration controller.
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.BudgetManagement)]
    [Authorize]
    public class BudgetDevelopmentConfigurationController : BaseCompressedApiController
    {
        private readonly IBudgetDevelopmentConfigurationService budgetDevelopmentConfigurationService;
        private readonly ILogger logger;

        /// <summary>
        /// This constructor initializes the Budget Development Configuration controller.
        /// </summary>
        /// <param name="budgetDevelopmentConfigurationService">BudgetDevelopment Configuration service object.</param>
        /// <param name="logger">Logger object.</param>
        public BudgetDevelopmentConfigurationController(IBudgetDevelopmentConfigurationService budgetDevelopmentConfigurationService, ILogger logger)
        {
            this.budgetDevelopmentConfigurationService = budgetDevelopmentConfigurationService;
            this.logger = logger;
        }

        /// <summary>
        /// Returns the BudgetDevelopment configuration.
        /// </summary>
        /// <returns>The BudgetDevelopment configuration.</returns>
        /// <accessComments>
        /// No permission is needed.
        /// </accessComments>
        public async Task<BudgetConfiguration> GetBudgetDevelopmentConfigurationAsync()
        {
            try
            {
                // Call the service method to obtain necessary Budget Development configuration parameters.
                var budgetDevelopmentConfiguration = await budgetDevelopmentConfigurationService.GetBudgetDevelopmentConfigurationAsync();
                return budgetDevelopmentConfiguration;
            }
            catch (ConfigurationException cnex)
            {
                logger.Error(cnex, cnex.Message);
                throw CreateHttpResponseException("The working budget is not available.", HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                throw CreateHttpResponseException("The working budget is not available.", HttpStatusCode.BadRequest);
            }
        }
    }
}
