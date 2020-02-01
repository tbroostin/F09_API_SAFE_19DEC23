﻿// Copyright 2017-2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.ColleagueFinance.Exceptions;
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
    /// Controls actions for budget adjustments
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    [Authorize]
    public class BudgetAdjustmentsController : BaseCompressedApiController
    {
        private IBudgetAdjustmentService budgetAdjustmentService;
        private readonly ILogger logger;

        /// <summary>
        /// Initialize the controller.
        /// </summary>
        /// <param name="budgetAdjustmentService">Budget adjustment service</param>
        /// <param name="logger">Logger</param>
        public BudgetAdjustmentsController(IBudgetAdjustmentService budgetAdjustmentService, ILogger logger)
        {
            this.budgetAdjustmentService = budgetAdjustmentService;
            this.logger = logger;
        }

        /// <summary>
        /// Creates a new budget adjustment.
        /// </summary>
        /// <param name="budgetAdjustmentDto">Budget adjustment DTO</param>
        /// <accessComments>
        /// Requires permission CREATE.UPDATE.BUDGET.ADJUSTMENT
        /// </accessComments>
        [HttpPost]
        public async Task<Dtos.ColleagueFinance.BudgetAdjustment> PostAsync([FromBody] Dtos.ColleagueFinance.BudgetAdjustment budgetAdjustmentDto)
        {
            try
            {
                return await budgetAdjustmentService.CreateBudgetAdjustmentAsync(budgetAdjustmentDto);
            }
            catch (PermissionsException peex)
            {
                logger.Error(peex.Message);
                throw CreateHttpResponseException("Insufficient permissions to create the budget adjustment.", HttpStatusCode.Forbidden);
            }
            catch (ConfigurationException cex)
            {
                logger.Error(cex.Message);
                throw CreateHttpResponseException("Unable to get budget adjustment configuration.", HttpStatusCode.NotFound);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                throw CreateHttpResponseException("Unable to create a budget adjustment.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Updates an existing budget adjustment.
        /// </summary>
        /// <param name="id">The ID of the budget adjustment that will be updated.</param>
        /// <param name="budgetAdjustmentDto">The budget adjustment content that will be updated.</param>
        /// <accessComments>
        /// Requires permission CREATE.UPDATE.BUDGET.ADJUSTMENT
        /// </accessComments>
        /// <returns>The updated budget adjustment as it was stored.</returns>
        [HttpPut]
        public async Task<Dtos.ColleagueFinance.BudgetAdjustment> PutAsync(string id, [FromBody] Dtos.ColleagueFinance.BudgetAdjustment budgetAdjustmentDto)
        {
            try
            {
                return await budgetAdjustmentService.UpdateBudgetAdjustmentAsync(id, budgetAdjustmentDto);
            }
            catch (PermissionsException peex)
            {
                logger.Error(peex.Message);
                throw CreateHttpResponseException("Insufficient permissions to update the budget adjustment.", HttpStatusCode.Forbidden);
            }
            catch (ConfigurationException cex)
            {
                logger.Error(cex.Message);
                throw CreateHttpResponseException("Unable to get budget adjustment configuration.", HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                throw CreateHttpResponseException("Unable to update the budget adjustment.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get a budget adjustment.
        /// </summary>
        /// <param name="id">The ID of the budget adjustment.</param>
        /// <returns>A budget adjustment</returns>
        /// <accessComments>
        /// Requires permission VIEW.BUDGET.ADJUSTMENT.
        /// The current user must be the user that created the budget adjustment.
        /// </accessComments>
        [HttpGet]
        public async Task<BudgetAdjustment> GetBudgetAdjustmentAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                string message = "A budget adjustment number must be specified.";
                logger.Error(message);
                throw CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
            try
            {
                return await budgetAdjustmentService.GetBudgetAdjustmentAsync(id);
            }
            catch (PermissionsException peex)
            {
                logger.Error(peex.Message);
                throw CreateHttpResponseException("Insufficient permissions to get the budget adjustment.", HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException cex)
            {
                logger.Error(cex.Message);
                throw CreateHttpResponseException("Record not found.", HttpStatusCode.NotFound);
            }
            catch (ArgumentException agex)
            {
                logger.Error(agex.Message);
                throw CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                throw CreateHttpResponseException("Unable to get the budget adjustment.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get a budget adjustment that is awaiting the user's approval.
        /// </summary>
        /// <param name="id">The ID of the budget adjustment.</param>
        /// <returns>A budget adjustment</returns>
        /// <accessComments>
        /// Requires permission VIEW.BUD.ADJ.PENDING.APPR.
        /// The current user must be one of the users listed as a "next approver" on the budget adjustment.
        /// Adjustment Line data will only be returned for GL accounts that the user has access to based on their GL user access.
        /// </accessComments>
        [HttpGet]
        public async Task<BudgetAdjustment> GetBudgetAdjustmentPendingApprovalDetailAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                string message = "A budget adjustment number must be specified.";
                logger.Error(message);
                throw CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
            try
            {
                return await budgetAdjustmentService.GetBudgetAdjustmentPendingApprovalDetailAsync(id);
            }
            catch (PermissionsException peex)
            {
                logger.Error(peex.Message);
                throw CreateHttpResponseException("Insufficient permissions to get the budget adjustment.", HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException cex)
            {
                logger.Error(cex.Message);
                throw CreateHttpResponseException("Record not found.", HttpStatusCode.NotFound);
            }
            catch (ArgumentException agex)
            {
                logger.Error(agex.Message);
                throw CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                throw CreateHttpResponseException("Unable to get the budget adjustment.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// A user approves an existing budget adjustment.
        /// </summary>
        /// <param name="id">The ID of the budget adjustment that will get the approval ID.</param>
        /// <param name="budgetAdjustmentApprovalDto">The budget adjustment approval data.</param>
        /// <returns>The budget adjustment approval dto.</returns>
        /// <accessComments>
        /// Requires permission VIEW.BUD.ADJ.PENDING.APPR.
        /// The current user must be one of the users listed as a "next approver" on the budget adjustment,
        /// and they must have not approved this budget adjustment already.
        /// </accessComments>
        [HttpPost]
        public async Task<Dtos.ColleagueFinance.BudgetAdjustmentApproval> PostBudgetAdjustmentApprovalAsync(string id, [FromBody] Dtos.ColleagueFinance.BudgetAdjustmentApproval budgetAdjustmentApprovalDto)
        {
            try
            {
                return await budgetAdjustmentService.PostBudgetAdjustmentApprovalAsync(id, budgetAdjustmentApprovalDto);
            }
            catch (PermissionsException peex)
            {
                logger.Error(peex.Message);
                throw CreateHttpResponseException("Insufficient permissions to approve the budget adjustment.", HttpStatusCode.Forbidden);
            }
            catch (AlreadyApprovedByUserException appx)
            {
                logger.Error(appx.Message);
                throw CreateHttpResponseException("You have already approved this budget adjustment.", HttpStatusCode.BadRequest);
            }
            catch (NotApprovedStatusException naex)
            {
                logger.Error(naex.Message);
                throw CreateHttpResponseException("The budget adjustment does not have a not approved status.", HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                throw CreateHttpResponseException("Unable to approve the budget adjustment.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get all the budget adjustments summary for a user.
        /// </summary>
        /// <returns>List of budget adjustment summary DTOs for the current user.</returns>
        /// <accessComments>
        /// Requires permission VIEW.BUDGET.ADJUSTMENT.
        /// A user can only get a list of draft and non-draft budget adjustments that they created.
        /// </accessComments>
        [HttpGet]
        public async Task<IEnumerable<BudgetAdjustmentSummary>> GetBudgetAdjustmentsSummaryAsync()
        {
            try
            {
                return await budgetAdjustmentService.GetBudgetAdjustmentsSummaryAsync();
            }
            catch (PermissionsException peex)
            {
                logger.Error(peex.Message);
                throw CreateHttpResponseException("Insufficient permissions to get the budget adjustment summary.", HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                throw CreateHttpResponseException("Unable to get budget adjustments summary", HttpStatusCode.BadRequest);
            }
        }


        /// <summary>
        /// Get all the budget adjustments pending approval summaries for a user.
        /// </summary>
        /// <returns>List of budget adjustment pending approval summary DTOs for the current user.</returns>
        /// <accessComments>
        /// Requires permission VIEW.BUD.ADJ.PENDING.APPR.
        /// A user can only get a list of budget adjustments pending approval 
        /// where they are a next approver.
        /// </accessComments>
        [HttpGet]
        public async Task<IEnumerable<BudgetAdjustmentPendingApprovalSummary>> GetBudgetAdjustmentsPendingApprovalSummaryAsync()
        {
            try
            {
                return await budgetAdjustmentService.GetBudgetAdjustmentsPendingApprovalSummaryAsync();
            }
            catch (PermissionsException peex)
            {
                logger.Error(peex.Message);
                throw CreateHttpResponseException("Insufficient permissions to get the budget adjustment pending approval summary.", HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                throw CreateHttpResponseException("Unable to get budget adjustments pending approval summary", HttpStatusCode.BadRequest);
            }
        }
    }
}