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
    public class BudgetDevelopmentController : BaseCompressedApiController
    {
        private readonly IBudgetDevelopmentService budgetDevelopmentService;
        private readonly ILogger logger;

        /// <summary>
        /// This constructor initializes the Budget Development controller.
        /// </summary>
        /// <param name="budgetDevelopmentService">BudgetDevelopment service object.</param>
        /// <param name="logger">Logger object.</param>
        public BudgetDevelopmentController(IBudgetDevelopmentService budgetDevelopmentService, ILogger logger)
        {
            this.budgetDevelopmentService = budgetDevelopmentService;
            this.logger = logger;
        }

        /// <summary>
        /// Returns the filtered line items in the working budget with or without subtotals.
        /// </summary>
        /// <returns>The working budget line items that match the filtered criteria with or without subtotals.</returns>
        /// <param name="criteria">Working budget filter criteria.</param>
        /// <accessComments>
        /// No permission is needed. A user has access based on what budget officers they are assigned.
        /// </accessComments>
        [HttpPost]
        public async Task<WorkingBudget2> QueryWorkingBudgetByPost2Async([FromBody]WorkingBudgetQueryCriteria criteria)
        {
            try
            {
                if (criteria == null)
                {
                    throw new ArgumentNullException("criteria", "The query criteria must be specified.");
                }

                var budgetDevelopmentWorkingBudget = await budgetDevelopmentService.QueryWorkingBudget2Async(criteria);
                return budgetDevelopmentWorkingBudget;
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                throw CreateHttpResponseException("Unable to get the working budget.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Updates the BudgetDevelopment working budget.
        /// </summary>
        /// <param name="budgetLineItemsDto">A list of budget line items for the working budget.</param>
        /// <returns>A list of updated budget line items for the working budget.</returns>
        /// <accessComments>
        /// No permission is needed. A user may only update budget line items based on what budget officers that they are assigned.
        /// </accessComments>
        [HttpPut]
        public async Task<List<BudgetLineItem>> UpdateBudgetDevelopmentWorkingBudgetAsync([FromBody] List<BudgetLineItem> budgetLineItemsDto)
        {
            try
            {
                return await budgetDevelopmentService.UpdateBudgetDevelopmentWorkingBudgetAsync(budgetLineItemsDto);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                throw CreateHttpResponseException("Unable to update the working budget.", HttpStatusCode.BadRequest);
            }
        }

        #region Obsolete/Deprecated

        /// <summary>
        /// Returns the BudgetDevelopment working budget.
        /// </summary>
        /// <returns>The BudgetDevelopment working budget.</returns>
        /// <param name="startPosition">Start position of the budget line items to return.</param>
        /// <param name="recordCount">Number of budget line items to return.</param>
        /// <accessComments>
        /// No permission is needed. A user has access based on what budget officers they are assigned.
        /// </accessComments>
        [HttpGet]
        public async Task<WorkingBudget> GetBudgetDevelopmentWorkingBudgetAsync(int startPosition, int recordCount)
        {
            try
            {
                // Call the service method to return the working budget for the user.
                var budgetDevelopmentWorkingBudget = await budgetDevelopmentService.GetBudgetDevelopmentWorkingBudgetAsync(startPosition, recordCount);
                return budgetDevelopmentWorkingBudget;
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                throw CreateHttpResponseException("The working budget is not available.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Returns the filtered line items in the working budget.
        /// </summary>
        /// <returns>The working budget line items that match the filtered criteria.</returns>
        /// <param name="criteria">Working budget filter criteria.</param>
        /// <accessComments>
        /// No permission is needed. A user has access based on what budget officers they are assigned.
        /// </accessComments>
        [Obsolete("Obsolete as of Colleague Web API 1.25. Use QueryWorkingBudgetByPost2Async")]
        [HttpPost]
        public async Task<WorkingBudget> QueryWorkingBudgetByPostAsync([FromBody]WorkingBudgetQueryCriteria criteria)
        {
            try
            {
                if (criteria == null)
                {
                    throw new ArgumentNullException("criteria", "The query criteria must be specified.");
                }

                var budgetDevelopmentWorkingBudget = await budgetDevelopmentService.QueryWorkingBudgetAsync(criteria);
                return budgetDevelopmentWorkingBudget;
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                throw CreateHttpResponseException("Unable to get the working budget.", HttpStatusCode.BadRequest);
            }
        }

        #endregion
    }
}
