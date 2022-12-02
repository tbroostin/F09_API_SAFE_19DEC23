/*Copyright 2014-2017 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Data.Colleague.Exceptions;
using System.Net;

namespace Ellucian.Colleague.Api.Controllers.FinancialAid
{
    /// <summary>
    /// LoanRequests Controller
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class LoanRequestsController : BaseCompressedApiController
    {
        private readonly ILoanRequestService loanRequestService;

        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;

        /// <summary>
        /// Instantiate a new LoanRequestController object
        /// </summary>
        /// <param name="adapterRegistry">AdapterRegistry</param>
        /// <param name="loanRequestService">LoanRequestService</param>
        /// <param name="logger">Logger</param>
        public LoanRequestsController(IAdapterRegistry adapterRegistry, ILoanRequestService loanRequestService, ILogger logger)
        {
            this.loanRequestService = loanRequestService;
            this.adapterRegistry = adapterRegistry;
            this.logger = logger;
        }

        /// <summary>
        /// Get a LoanRequest object with the specified Id. A LoanRequest id can be found in a StudentAwardYear DTO
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have
        /// VIEW.FINANCIAL.AID.INFORMATION permission or proxy permissions
        /// can request other users' data
        /// </accessComments>
        /// <param name="id">The id of the loan request</param>
        /// <returns>A LoanRequest DTO with the specified id</returns>
        /// <exception cref="HttpResponseException">400, Thrown if no id is specified, if the server is unable to create the 
        /// LoanRequest object due to data corruption, or if some other unknown error occurs.</exception>
        /// <exception cref="HttpResponseException">403, Thrown if the current user does not have permission to access the requested LoanRequest resource</exception>
        /// <exception cref="HttpResponseException">404, Thrown if the requested resource cannot be found or does not exist</exception>
        [HttpGet]
        public async Task<LoanRequest> GetLoanRequestAsync([FromUri]string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw CreateHttpResponseException("id cannot be null or empty");
            }

            try
            {
                return await loanRequestService.GetLoanRequestAsync(id);
                 
            }
            catch (PermissionsException pe)
            {
                logger.Error(pe, pe.Message);
                throw CreateHttpResponseException("Access to Loan Request resource is forbidden. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.Error(knfe, knfe.Message);
                throw CreateNotFoundException("LoanRequest", id);
            }
            catch (ApplicationException ae)
            {
                logger.Error(ae, ae.Message);
                throw CreateHttpResponseException("Exception encountered during LoanRequest object creation. See log for details.");
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unknown error occurred getting LoanRequest resource. See log for details.");
            }
        }

        /// <summary>
        /// Create a new pending LoanRequest resource based on the data in the loanRequest argument. Only one pending LoanRequest resource can exist for
        /// a StudentId and AwardYear.
        /// </summary>
        /// 
        /// <accessComments>
        /// Users may request changes to their own data only
        /// </accessComments>
        /// <param name="loanRequest">LoanRequest object containing data with which to create a LoanRequest resource. 
        /// StudentId, AwardYear and TotalRequestAmount are required, and TotalRequestAmount must be greater than zero</param>
        /// <returns>HttpResponseMessage containing the new LoanRequest object and a header value that specifies the Location 
        /// of the new LoanRequest resource. HttpStatusCode is 201 - Created</returns>
        /// <exception cref="HttpResponseException">400, Thrown if the loanRequest argument is null, if the required argument properties are invalid,
        /// if the resource's current state prevents the LoanRequest from being created, if an data error was encountered during object creation, or
        /// if some unknown error occurred</exception>
        /// <exception cref="HttpResponseException">403, Thrown if the Current user does not have permission to create the LoanRequest resource</exception>
        /// <exception cref="HttpResponseException">409, Thrown if a LoanRequest resource already exists for the requested new LoanRequest, or if a
        /// conflicting record lock exists on the server</exception>
        [HttpPost]
        public async Task<HttpResponseMessage> CreateLoanRequestAsync([FromBody]LoanRequest loanRequest)
        {
            if (loanRequest == null)
            {
                throw CreateHttpResponseException("loanRequest object is required in request body");
            }

            try
            {
                var newLoanRequest = await loanRequestService.CreateLoanRequestAsync(loanRequest);
                var response = Request.CreateResponse<LoanRequest>(System.Net.HttpStatusCode.Created, newLoanRequest);
                SetResourceLocationHeader("GetLoanRequest", new { id = newLoanRequest.Id });
                return response;
            }
            catch (ColleagueSessionExpiredException csee)
            {
                throw CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException ae)
            {
                logger.Error(ae, ae.Message);
                throw CreateHttpResponseException("Input LoanRequest object is invalid. See log for details");
            }
            catch (PermissionsException pe)
            {
                logger.Error(pe, pe.Message);
                throw CreateHttpResponseException("Permission denied to create LoanRequest resource. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (ExistingResourceException ere)
            {
                logger.Error(ere, ere.Message);
                SetResourceLocationHeader("GetLoanRequest", new { id = ere.ExistingResourceId });
                throw CreateHttpResponseException("Cannot create resource that already exists. See log for details.", System.Net.HttpStatusCode.Conflict);
            }
            catch (OperationCanceledException oce)
            {
                logger.Error(oce, oce.Message);
                throw CreateHttpResponseException("Request was cancelled because of a conflict on the server. See log for details.", System.Net.HttpStatusCode.Conflict);
            }
            catch (InvalidOperationException ioe)
            {
                logger.Error(ioe, ioe.Message);
                throw CreateHttpResponseException("Request is invalid for the resource's current state. See log for details.");
            }
            catch (ApplicationException ae)
            {
                logger.Error(ae, ae.Message);
                throw CreateHttpResponseException("Exception encountered during LoanRequest object creation. See log for details.");
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unknown error occurred creating LoanRequest resource. See log for details.");
            }
        }
    }
}