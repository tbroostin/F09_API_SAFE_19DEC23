// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.BudgetManagement.Services;
using Ellucian.Colleague.Dtos.BudgetManagement;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Collections.Generic;
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
    public class BudgetOfficerController : BaseCompressedApiController
    {
        private readonly IBudgetDevelopmentService budgetDevelopmentService;
        private readonly ILogger logger;

        /// <summary>
        /// This constructor initializes the Budget Development controller.
        /// </summary>
        /// <param name="budgetDevelopmentService">BudgetDevelopment service object.</param>
        /// <param name="logger">Logger object.</param>
        public BudgetOfficerController(IBudgetDevelopmentService budgetDevelopmentService, ILogger logger)
        {
            this.budgetDevelopmentService = budgetDevelopmentService;
            this.logger = logger;
        }

        /// <summary>
        /// Returns the budget officers for the working budget.
        /// </summary>
        /// <returns>The budget officers for the working budget.</returns>
        /// <param name="isInWorkingBudget">Indicates whether to get budget officers for the working budget for a user.</param>
        /// <accessComments>
        /// No permission is needed. A user has access based on what budget officers they and their reporting units are assigned.
        /// </accessComments>
        [HttpGet]
        public async Task<List<BudgetOfficer>> GetBudgetOfficersAsync(bool isInWorkingBudget)
        {
            if (isInWorkingBudget)
            {
                try
                {
                    // Call the service method to return the budget officers for the working budget for the user.
                    var workingBudgetBudgetOfficers = await budgetDevelopmentService.GetBudgetDevelopmentBudgetOfficersAsync();
                    return workingBudgetBudgetOfficers;
                }
                catch (Exception ex)
                {
                    logger.Error(ex, ex.Message);
                    throw CreateHttpResponseException("The budget officers for the working budget are not available.", HttpStatusCode.BadRequest);
                }
            }
            else
            {
                logger.Error("Getting budget officers that are not associated with the working budget is not available.");
                throw CreateHttpResponseException("Getting budget officers that are not associated with the working budget is not available.", HttpStatusCode.NotImplemented);
            }
        }
    }
}
