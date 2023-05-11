// Copyright 2018-2021 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
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
    public class DraftBudgetAdjustmentsController : BaseCompressedApiController
    {
        private IDraftBudgetAdjustmentService draftBudgetAdjustmentService;
        private readonly ILogger logger;

        /// <summary>
        /// Initialize the controller.
        /// </summary>
        /// <param name="draftBudgetAdjustmentService">Draft Budget adjustment service</param>
        /// <param name="logger">Logger</param>
        public DraftBudgetAdjustmentsController(IDraftBudgetAdjustmentService draftBudgetAdjustmentService, ILogger logger)
        {
            this.draftBudgetAdjustmentService = draftBudgetAdjustmentService;
            this.logger = logger;
        }

        /// <summary>
        /// Creates a new draft budget adjustment.
        /// </summary>
        /// <param name="draftBudgetAdjustmentDto">Draft Budget adjustment DTO</param>
        /// <accessComments>
        /// Requires permission CREATE.UPDATE.BUDGET.ADJUSTMENT
        /// </accessComments>
        [HttpPost]
        public async Task<Dtos.ColleagueFinance.DraftBudgetAdjustment> PostAsync([FromBody] Dtos.ColleagueFinance.DraftBudgetAdjustment draftBudgetAdjustmentDto)
        {
            try
            {
                return await draftBudgetAdjustmentService.SaveDraftBudgetAdjustmentAsync(draftBudgetAdjustmentDto);
            }
            catch (PermissionsException peex)
            {
                logger.Error(peex.Message);
                throw CreateHttpResponseException("Insufficient permissions to create the draft budget adjustment.", HttpStatusCode.Forbidden);
            }
            catch (ConfigurationException cex)
            {
                logger.Error(cex.Message);
                throw CreateHttpResponseException("Unable to create draft budget adjustment - configuration exception.", HttpStatusCode.NotFound);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, "==> PostAsync session expired <==");
                throw CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                throw CreateHttpResponseException("Unable to create a draft budget adjustment.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Updates the requested draft budget adjustment.
        /// </summary>
        /// <param name="id">A draft budget adjustment id.</param>
        /// <param name="draftBudgetAdjustmentDto">Draft Budget adjustment DTO</param>
        /// <accessComments>
        /// Requires permission CREATE.UPDATE.BUDGET.ADJUSTMENT
        /// A user can only update a draft budget adjustments that they created.
        /// </accessComments>
        [HttpPut]
        public async Task<Dtos.ColleagueFinance.DraftBudgetAdjustment> UpdateAsync(string id, [FromBody] Dtos.ColleagueFinance.DraftBudgetAdjustment draftBudgetAdjustmentDto)
        {
            if (id == null)
            {
                throw CreateHttpResponseException("id is required in body of request", HttpStatusCode.BadRequest);
            }
            if (draftBudgetAdjustmentDto == null)
            {
                throw CreateHttpResponseException("draftBudgetAdjustmentDto is required in body of request", HttpStatusCode.BadRequest);
            }
            try
            {
                draftBudgetAdjustmentDto.Id = id;
                return await draftBudgetAdjustmentService.SaveDraftBudgetAdjustmentAsync(draftBudgetAdjustmentDto);
            }
            catch (PermissionsException peex)
            {
                logger.Error(peex.Message);
                throw CreateHttpResponseException("Insufficient permissions to update the budget adjustment.", HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException cex)
            {
                logger.Error(cex.Message);
                throw CreateHttpResponseException("Record not found.", HttpStatusCode.NotFound);
            }
            catch (ConfigurationException cex)
            {
                logger.Error(cex.Message);
                throw CreateHttpResponseException("Unable to update draft budget adjustment - configuration exception.", HttpStatusCode.BadRequest);
            }
            catch (ApplicationException aex)
            {
                logger.Error(aex.Message);
                throw CreateHttpResponseException("Unable to update draft budget adjustment - application exception.", HttpStatusCode.BadRequest);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, "==> UpdateAsync session expired <==");
                throw CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                throw CreateHttpResponseException("Unable to update draft budget adjustment.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get the requested draft budget adjustment.
        /// </summary>
        /// <param name="id">A draft budget adjustment id.</param>
        /// <returns>A draft budget adjustment DTO.</returns>
        /// <accessComments>
        /// Requires permission VIEW.BUDGET.ADJUSTMENT
        /// A user can only get a draft budget adjustment that they have created.
        /// </accessComments>
        [HttpGet]
        public async Task<Dtos.ColleagueFinance.DraftBudgetAdjustment> GetAsync(string id)
        {
            if (id == null)
            {
                throw CreateHttpResponseException("id is required in body of request", HttpStatusCode.BadRequest);
            }

            try
            {
                return await draftBudgetAdjustmentService.GetAsync(id);
            }
            catch (PermissionsException peex)
            {
                logger.Error(peex.Message);
                throw CreateHttpResponseException("Insufficient permissions to get the draft budget adjustment.", HttpStatusCode.Forbidden);
            }
            catch (ArgumentNullException argex)
            {
                logger.Error(argex.Message);
                throw CreateHttpResponseException("Unable to get the draft budget adjustment.", HttpStatusCode.BadRequest);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                throw CreateHttpResponseException("Unable to get the draft budget adjustment.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Delete the requested draft budget adjustment.
        /// </summary>
        /// <param name="id">The draft budget adjustment ID to delete.</param>
        /// <returns>nothing</returns>
        /// <accessComments>
        /// Requires permission DELETE.BUDGET.ADJUSTMENT
        /// A user can only delete a draft budget adjustment that they have created.
        /// </accessComments>
        [HttpDelete]
        public async Task<HttpResponseMessage> DeleteAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                logger.Error("A draft budget adjustment ID is required.");
                throw CreateHttpResponseException("A draft budget adjustment ID is required.", HttpStatusCode.BadRequest);
            }
            try
            {
                await draftBudgetAdjustmentService.DeleteAsync(id);
                var response = Request.CreateResponse(HttpStatusCode.OK);
                return response;
            }
            catch (PermissionsException peex)
            {
                logger.Error(peex.Message);
                throw CreateHttpResponseException("Insufficient permissions to delete the draft budget adjustment.", HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.Error(knfe, knfe.Message);
                throw CreateHttpResponseException("Unable to delete draft budget adjustment.", HttpStatusCode.NotFound);
            }
            catch (ArgumentNullException aex)
            {
                logger.Error(aex, aex.Message);
                throw CreateHttpResponseException("Unable to delete draft budget adjustment - application exception", HttpStatusCode.BadRequest);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, "==> DeleteAsync session expired <==");
                throw CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                throw CreateHttpResponseException("Unable to delete the draft budget adjustment.", HttpStatusCode.BadRequest);
            }
        }
    }
}