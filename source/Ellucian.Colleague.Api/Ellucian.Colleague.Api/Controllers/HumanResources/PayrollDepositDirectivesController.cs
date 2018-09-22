/*Copyright 2017-2018 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Colleague.Dtos.HumanResources;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// a
    /// </summary>
    [System.Web.Http.Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class PayrollDepositDirectivesController : BaseCompressedApiController
    {
        private const string stepUpAuthenticationHeaderKey = "X-Step-Up-Authentication";

        private readonly ILogger logger;
        private readonly IPayrollDepositDirectiveService payrollDepositDirectiveService;

        /// <summary>
        /// PayrollDepositDirectives Controller constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="payrollDepositDirectiveService"></param>
        public PayrollDepositDirectivesController(ILogger logger, IPayrollDepositDirectiveService payrollDepositDirectiveService)
        {
            this.logger = logger;
            this.payrollDepositDirectiveService = payrollDepositDirectiveService;
        }

        /// <summary>
        /// Gets a list of PayrollDepositDirectives for the Current User
        /// </summary>
        /// <returns>The Current User's PayrollDepositDirectives</returns>
        [HttpGet]
        public async Task<IEnumerable<PayrollDepositDirective>> GetPayrollDepositDirectivesAsync()
        {
            try
            {
                return await payrollDepositDirectiveService.GetPayrollDepositDirectivesAsync();
            }

            catch (KeyNotFoundException knfe)
            {
                var message = "Unable to find current user in payroll file";
                logger.Error(knfe, message);
                throw CreateHttpResponseException(message, HttpStatusCode.NotFound);
            }
            catch (PermissionsException pe)
            {
                var message = "You are forbidden from accessing this resource";
                logger.Error(pe, message);
                throw CreateHttpResponseException(message, HttpStatusCode.Forbidden);

            }
            catch (Exception e)
            {
                logger.Error(e, "Unknown error occurred");
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Gets a single PayrollDepositDirective from its record identifier. Requested resource must be owned by the current user.
        /// </summary>
        /// <param name="id">The Id of the payroll deposit directive</param>
        /// <returns>The requested payroll deposit directive</returns>
        [HttpGet]
        public async Task<PayrollDepositDirective> GetPayrollDepositDirectiveAsync([FromUri]string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw CreateHttpResponseException("PayrollDepositDirective Id must me provided in the URI", HttpStatusCode.BadRequest);
            }
            try
            {
                return await payrollDepositDirectiveService.GetPayrollDepositDirectiveAsync(id);
            }
            catch (KeyNotFoundException knfe)
            {
                var message = string.Format("Unable to find requested deposit {0} in payroll file", id);
                logger.Error(knfe, message);
                throw CreateHttpResponseException(message, HttpStatusCode.NotFound);
            }
            catch (PermissionsException pe)
            {
                var message = "You are forbidden from accessing this resource";
                logger.Error(pe, message);
                throw CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.Error(e, "Unknown error occurred");
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Updates a list of payroll deposit directives. Use this endpoint to batch update directives all at once. You must obtain an authentication token from the 
        /// Accept: application/vnd.ellucian-step-up-authentication.v1+json
        /// POST payroll-deposit-directives endpoint 
        ///         
        /// The BankingAuthenticationToken.Token property is 
        /// required in the X-Step-Up-Authentication header 
        /// if account authentication is enabled.
        /// 
        /// </summary>
        /// <param name="payrollDepositDirectives">A list of PayrollDepositDirectives containing the updates.</param>
        /// <returns>The list of PayrollDepositDirectives with the successfully updated properties</returns>
        [HttpPut]
        public async Task<IEnumerable<PayrollDepositDirective>> UpdatePayrollDepositDirectivesAsync([FromBody]IEnumerable<PayrollDepositDirective> payrollDepositDirectives)
        {
            if (payrollDepositDirectives == null)
            {
                throw CreateHttpResponseException("PayrollDepositDirectives cannot be null when updating PayrollDepositDirectives",
                    HttpStatusCode.BadRequest);
            }

            var token = GetStepUpAuthenticationHeaderValue();

            try
            {
                return await payrollDepositDirectiveService.UpdatePayrollDepositDirectivesAsync(token, payrollDepositDirectives);
            }
            catch (KeyNotFoundException knfe)
            {
                var message = string.Format("Unable to find deposit to update in payroll file");
                logger.Error(knfe, message);
                throw CreateHttpResponseException(message, HttpStatusCode.NotFound);
            }
            catch (PermissionsException pe)
            {
                var message = "You are forbidden from accessing this resource";
                logger.Error(pe, message);
                throw CreateHttpResponseException(message, HttpStatusCode.Forbidden);

            }
            catch (Exception e)
            {
                logger.Error(e, "Unknown error occurred");
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Updates a single PayrollDepositDirective. You must obtain an authentication token from the 
        /// Accept: application/vnd.ellucian-step-up-authentication.v1+json
        /// POST payroll-deposit-directives endpoint 
        ///         
        /// The BankingAuthenticationToken.Token property is 
        /// required in the X-Step-Up-Authentication header
        /// if account authentication is enabled.
        /// 
        /// </summary>
        /// <param name="id">The id of the directive being updated</param>
        /// <param name="payrollDepositDirective">The PayrollDepositDirective to update</param>
        /// <returns>The updated PayrollDepositDirective</returns>
        [HttpPut]
        public async Task<PayrollDepositDirective> UpdatePayrollDepositDirectiveAsync([FromUri]string id, [FromBody] PayrollDepositDirective payrollDepositDirective)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw CreateHttpResponseException("Identifier cannot be null when updating PayrollDepositDirectives",
                    HttpStatusCode.BadRequest);
            }
            if (payrollDepositDirective == null)
            {
                throw CreateHttpResponseException("PayrollDepositDirectives cannot be null when updating PayrollDepositDirectives",
                    HttpStatusCode.BadRequest);
            }
            if (id != payrollDepositDirective.Id)
            {
                throw CreateHttpResponseException("Id in URI must match Id in directive",
                    HttpStatusCode.BadRequest);
            }

            var token = GetStepUpAuthenticationHeaderValue();

            try
            {
                return await payrollDepositDirectiveService.UpdatePayrollDepositDirectiveAsync(token, payrollDepositDirective);
            }
            catch (KeyNotFoundException knfe)
            {
                var message = string.Format("Unable to find requested deposit {0} in payroll file", id);
                logger.Error(knfe, message);
                throw CreateHttpResponseException(message, HttpStatusCode.NotFound);
            }
            catch (PermissionsException pe)
            {
                var message = "You are forbidden from accessing this resource";
                logger.Error(pe, message);
                throw CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.Error(e, "Unknown error occurred");
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }


        /// <summary>
        /// Create a payroll deposit directive. You must obtain an authentication token from the 
        /// Accept: application/vnd.ellucian-step-up-authentication.v1+json
        /// POST payroll-deposit-directives endpoint 
        ///         
        /// The BankingAuthenticationToken.Token property is 
        /// required in the X-Step-Up-Authentication header
        /// if account authentication is enabled.
        /// 
        /// </summary>
        /// <param name="payrollDepositDirective">The PayrollDepositDirective to create</param>
        /// <returns>The created PayrollDepositDirective. Response Status will be 201 - Created </returns>
        [HttpPost]
        public async Task<HttpResponseMessage> CreatePayrollDepositDirectiveAsync([FromBody]PayrollDepositDirective payrollDepositDirective)
        {
            if (payrollDepositDirective == null)
            {
                throw CreateHttpResponseException("payrollDepositDirective cannot be null when creating PayrollDepositDirectives",
                    HttpStatusCode.BadRequest);
            }
            var token = GetStepUpAuthenticationHeaderValue();
            try
            {
                var createdDirective = await payrollDepositDirectiveService.CreatePayrollDepositDirectiveAsync(token, payrollDepositDirective);
                var response = Request.CreateResponse<PayrollDepositDirective>(HttpStatusCode.Created, createdDirective);
                SetResourceLocationHeader("GetPayrollDepositDirective", new { id = createdDirective.Id });
                return response;
            }
            catch (KeyNotFoundException knfe)
            {
                var message = string.Format("Unable to find deposit to create in payroll file");
                logger.Error(knfe, message);
                throw CreateHttpResponseException(message, HttpStatusCode.NotFound);
            }
            catch (PermissionsException pe)
            {
                var message = "You are forbidden from accessing this resource";
                logger.Error(pe, message);
                throw CreateHttpResponseException(message, HttpStatusCode.Forbidden);

            }
            catch (Exception e)
            {
                logger.Error(e, "Unknown error occurred");
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }


        /// <summary>
        /// Deletes a single PayrollDepositDirective using its record identifier. You must obtain an authentication token from the 
        /// Accept: application/vnd.ellucian-step-up-authentication.v1+json
        /// POST payroll-deposit-directives endpoint 
        ///         
        /// The BankingAuthenticationToken.Token property is 
        /// required in the X-Step-Up-Authentication header
        /// if account authentication is enabled.
        /// 
        /// </summary>
        /// <param name="id">The Id of the directive to delete</param>
        /// <returns>HTTP Status 204 - No Content</returns>
        [HttpDelete]
        public async Task<HttpResponseMessage> DeletePayrollDepositDirectiveAsync([FromUri]string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw CreateHttpResponseException("PayrollDepositDirective Id must me provided in the URI", HttpStatusCode.BadRequest);
            }
            var token = GetStepUpAuthenticationHeaderValue();
            try
            {
                var isSuccess = await payrollDepositDirectiveService.DeletePayrollDepositDirectiveAsync(token, id);
                if (isSuccess)
                {
                    return Request.CreateResponse(HttpStatusCode.NoContent);
                }
                else
                {
                    throw CreateHttpResponseException("Unable to delete payroll deposit directieve", HttpStatusCode.BadRequest);
                }
            }
            catch (KeyNotFoundException knfe)
            {
                var message = string.Format("Unable to find requested deposit {0} in payroll file", id);
                logger.Error(knfe, message);
                throw CreateHttpResponseException(message, HttpStatusCode.NotFound);
            }
            catch (PermissionsException pe)
            {
                var message = "You are forbidden from accessing this resource";
                logger.Error(pe, message);
                throw CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.Error(e, "Unknown error occurred");
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Deletes one or more PayrollDepositDirective a list of record identifiers. You must obtain an authentication token from the URI. Each id must
        /// be provided in the request:
        /// 
        /// Example:
        ///     1. to delete a single PayrollDepositDirective use /payroll-deposit-directives?id=123
        ///     2. to delete multiple PayrollDepositDirectives use /payroll-deposit-directives?id=123&amp;id=456
        /// 
        /// Accept: application/vnd.ellucian-step-up-authentication.v1+json
        /// POST payroll-deposit-directives endpoint 
        /// 
        /// The endpoint will not delete the requested PayrollDepositDirective(s) if:
        ///     1.  400 - no ids are provided in the uri
        ///     2.  403 - User does not have permission to delete requested record ids
        ///     3.  403 - no BankingAuthenticationToken is povided
        ///     4.  404 - requested id(s) do not exist
        ///     
        /// The BankingAuthenticationToken.Token property is 
        /// required in the X-Step-Up-Authentication header
        /// if account authentication is enabled.
        /// 
        /// </summary>
        /// <param name="id">The Id(s) of the directives to delete</param>
        /// <returns>HTTP Status 204 - No Content</returns>
        [HttpDelete]
        public async Task<HttpResponseMessage> DeletePayrollDepositDirectivesAsync([FromUri]IEnumerable<string> id)
        {
            if (!id.Any())
            {
                throw CreateHttpResponseException("One or more PayrollDepositDirective Ids must me provided in the uri", HttpStatusCode.BadRequest);
            }

            var token = GetStepUpAuthenticationHeaderValue();

            try
            {
                var isSuccess = await payrollDepositDirectiveService.DeletePayrollDepositDirectivesAsync(token, id);
                if (isSuccess)
                {
                    return Request.CreateResponse(HttpStatusCode.NoContent);
                }
                else
                {
                    throw CreateHttpResponseException("Unable to delete payroll deposit directives", HttpStatusCode.BadRequest);
                }
            }
            catch (KeyNotFoundException knfe)
            {
                string message = "Unable to find requested deposit id in payroll file";
                logger.Error(knfe, message);
                throw CreateHttpResponseException(message, HttpStatusCode.NotFound);
            }
            catch (PermissionsException pe)
            {
                var message = "You are forbidden from accessing this resource";
                logger.Error(pe, message);
                throw CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.Error(e, "Unknown error occurred");
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Post a request for authentication to create or update a PayrollDepositDirective.
        /// To create a directive, post authentication for the remainder directive. 
        /// </summary>
        /// <param name="id">id of the deposit directive to authenticate against</param>
        /// <param name="value">the authentication value, which should be the account id of the deposit directive specified by the id in the URI</param>
        /// <returns>A BankingAuthenticationToken object. Tokens expire after ten (10) minutes.</returns>
        [HttpPost]
        public async Task<BankingAuthenticationToken> PostPayrollDepositDirectiveAuthenticationAsync([FromUri]string id, [FromBody]string value)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw CreateHttpResponseException("id of PayrollDepositDirective is required in request URI", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrWhiteSpace(value))
            {
                throw CreateHttpResponseException("authentication value is required in request body");
            }

            try
            {
                return await payrollDepositDirectiveService.AuthenticateCurrentUserAsync(id, value);
            }
            catch (Exception e)
            {
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Post a request for authentication to create a PayrollDepositDirective when an employee has no existing directives
        /// </summary>
        /// <returns>A BankingAuthenticationToken object. Tokens expire after ten (10) minutes.</returns>
        [HttpPost]
        public async Task<BankingAuthenticationToken> PostPayrollDepositDirectivesAuthenticationAsync()
        {
            try
            {
                return await payrollDepositDirectiveService.AuthenticateCurrentUserAsync(null, null);
            }
            catch (Exception e)
            {
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }

        }

        /// <summary>
        /// Gets the value of the token used for Step-Up Authentication
        /// </summary>
        /// <returns>The first X-Step-Up-Authentication header value, if present. Otherwise null.</returns>
        private string GetStepUpAuthenticationHeaderValue()
        {
            IEnumerable<string> token;
            if (Request.Headers.TryGetValues(stepUpAuthenticationHeaderKey, out token))
            {
                return token.FirstOrDefault();
            }
            return null;
        }
    }
}