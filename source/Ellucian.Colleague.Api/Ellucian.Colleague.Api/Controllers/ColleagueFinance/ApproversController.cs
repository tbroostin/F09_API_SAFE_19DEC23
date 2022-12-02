// Copyright 2018-2021 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
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
    /// Provides access to approver objects.
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    [Authorize]
    public class ApproversController : BaseCompressedApiController
    {
        private readonly IApproverService approverService;
        private readonly ILogger logger;

        /// <summary>
        /// This constructor initializes the approver service controller.
        /// </summary>
        /// <param name="approverService">Approves service object.</param>
        /// <param name="logger">Logger object</param>
        public ApproversController(IApproverService approverService, ILogger logger)
        {
            this.approverService = approverService;
            this.logger = logger;
        }

        /// <summary>
        /// Validate an approver ID. 
        /// A next approver ID and an approver ID are the same. They are just
        /// populated under different circumstances.
        /// </summary>
        /// <param name="nextApproverId">Next approver ID.</param>
        /// <returns>A <see cref="NextApproverValidationResponse">DTO.</see>/></returns>
        /// <accessComments>
        /// Requires permission CREATE.UPDATE.BUDGET.ADJUSTMENT
        /// </accessComments>
        [HttpGet]
        public async Task<NextApproverValidationResponse> GetNextApproverValidationAsync(string nextApproverId)
        {
            try
            {
                return await approverService.ValidateApproverAsync(nextApproverId);
            }
            catch (PermissionsException peex)
            {
                logger.Error(peex.Message);
                throw CreateHttpResponseException("Insufficient permissions to validate the next approver.", HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                throw CreateHttpResponseException("Unable to validate a next approver.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get the list of next apporvers based on keyword search.
        /// </summary>
        /// <param name="queryKeyword">parameter for passing search keyword</param>
        /// <returns>The Next approver search results</returns>      
        /// <accessComments>
        /// Requires at least one of the permissions VIEW.ANY.PERSON or CREATE.UPDATE.REQUISITION or CREATE.UPDATE.PURCHASE.ORDER or CREATE.UPDATE.VOUCHER or CREATE.UPDATE.BUDGET.ADJUSTMENT
        /// </accessComments>
        [Obsolete("Obsolete as of API 1.34. Use QueryNextApproverByKeywordAsync.")]
        [HttpGet]
        public async Task<IEnumerable<NextApprover>> GetNextApproverByKeywordAsync(string queryKeyword)
        {
            if (string.IsNullOrEmpty(queryKeyword))
            {
                string message = "query keyword is required to query.";
                logger.Error(message);
                throw CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }

            try
            {
                var nextApproverSearchResults = await approverService.QueryNextApproverByKeywordAsync(queryKeyword);
                return nextApproverSearchResults;
            }
            catch (ArgumentNullException anex)
            {
                logger.Error(anex, "Invalid argument.");
                throw CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            catch (PermissionsException peex)
            {
                logger.Error(peex, "Insufficient permissions to get the approver info.");
                throw CreateHttpResponseException("Insufficient permissions to get the approver info.", HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfex)
            {
                logger.Error(knfex, "Record not found.");
                throw CreateHttpResponseException("Record not found.", HttpStatusCode.NotFound);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to search approver");
                throw CreateHttpResponseException("Unable to search approver", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get the list of Next Approvers based on keyword search.
        /// </summary>
        /// <param name="criteria">KeywordSearchCriteria parameter for passing search keyword</param>
        /// <returns> The Next approver search results</returns>      
        /// <accessComments>
        /// Requires at least one of the permissions VIEW.ANY.PERSON or CREATE.UPDATE.REQUISITION or CREATE.UPDATE.PURCHASE.ORDER or CREATE.UPDATE.VOUCHER or CREATE.UPDATE.BUDGET.ADJUSTMENT
        /// </accessComments>
        [HttpPost]
        public async Task<IEnumerable<NextApprover>> QueryNextApproverByKeywordAsync([FromBody] KeywordSearchCriteria criteria)
        {

            if (criteria == null || string.IsNullOrEmpty(criteria.Keyword))
            {
                string message = "query keyword is required to query.";
                logger.Error(message);
                throw CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }

            try
            {
                var nextApproverSearchResults = await approverService.QueryNextApproverByKeywordAsync(criteria.Keyword);
                return nextApproverSearchResults;
            }
            catch (ArgumentNullException anex)
            {
                logger.Error(anex, "Invalid argument.");
                throw CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            catch (PermissionsException peex)
            {
                logger.Error(peex, "Insufficient permissions to get the approver info.");
                throw CreateHttpResponseException("Insufficient permissions to get the approver info.", HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfex)
            {
                logger.Error(knfex, "Record not found.");
                throw CreateHttpResponseException("Record not found.", HttpStatusCode.NotFound);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to search approver");
                throw CreateHttpResponseException("Unable to search approver", HttpStatusCode.BadRequest);
            }
        }
    }
}