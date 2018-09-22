/*Copyright 2017-2018 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;

namespace Ellucian.Colleague.Api.Controllers.Base
{

    /// <summary>
    /// Exposes Payable Deposit Directives functionality
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class PayableDepositDirectivesController : BaseCompressedApiController
    {
        private readonly ILogger logger;
        private readonly IPayableDepositDirectiveService payableDepositDirectiveService;

        private const string stepUpAuthenticationHeaderKey = "X-Step-Up-Authentication";

        /// <summary>
        /// Instantiate a new PayableDepositDirectivesController
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="payableDepositDirectiveService"></param>
        public PayableDepositDirectivesController(ILogger logger, IPayableDepositDirectiveService payableDepositDirectiveService)
        {
            this.logger = logger;
            this.payableDepositDirectiveService = payableDepositDirectiveService;
        }

        /// <summary>
        /// Get all of the current user's PayableDepositDirectives.
        /// </summary>
        /// <returns>A list of a person's PayableDepositDirectives</returns>
        public async Task<IEnumerable<PayableDepositDirective>> GetPayableDepositDirectivesAsync()
        {
            try
            {
                return await payableDepositDirectiveService.GetPayableDepositDirectivesAsync();
            }
            catch (Exception e)
            {
                logger.Error(e, "Unknown exception occurred");
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get a single PayableDepositDirective for the current user
        /// </summary>
        /// <param name="id">Id of the payableDepositDirective</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<PayableDepositDirective> GetPayableDepositDirectiveAsync([FromUri] string id)
        {

            if (string.IsNullOrEmpty(id))
            {
                throw CreateHttpResponseException("payableDepositDirectiveId is required");
            }

            try
            {
                var payableDepositDirective = await payableDepositDirectiveService.GetPayableDepositDirectiveAsync(id);

                return payableDepositDirective;
            }
            catch (PermissionsException pe)
            {
                logger.Error(pe, "Access to resource is forbidden");
                throw CreateHttpResponseException("You don't have permission to access to this resource", HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.Error(knfe, knfe.Message);
                throw CreateNotFoundException("PayableDepositDirective", id);
            }
            catch (Exception e)
            {
                logger.Error(e, "Unknown exception occurred");
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }

        }

        /// <summary>
        /// Create a new PayableDepositDirective resource based on the data in body of the request. The BankingAuthenticationToken.Token property is 
        /// required in the X-Step-Up-Authentication header
        /// if account authentication is enabled.
        /// 
        /// </summary>
        /// <param name="payableDepositDirective">payableDepositDirective object containing data with which to create a PayableDepositDirective resource.</param>
        /// <returns>An HttpResponseMessage with the created resource in the Content property of the body and the URI of the created resource in the 
        /// Location Header. The schema of the PayableDepositDirective in the Content property of the response is the same as the schema of the input PayableDepositDirective from the Request</returns>
        [HttpPost]
        public async Task<HttpResponseMessage> CreatePayableDepositDirectiveAsync([FromBody] PayableDepositDirective payableDepositDirective)
        {
            if (payableDepositDirective == null)
            {
                throw CreateHttpResponseException("payableDepositDirective object is required in request body");
            }

            var token = GetStepUpAuthenticationHeaderValue();

            try
            {
                var newPayableDepositDirective = await payableDepositDirectiveService.CreatePayableDepositDirectiveAsync(token, payableDepositDirective);
                var response = Request.CreateResponse<PayableDepositDirective>(HttpStatusCode.Created, newPayableDepositDirective);
                SetResourceLocationHeader("GetPayableDepositDirective", new { id = newPayableDepositDirective.Id });
                return response;
            }

            // need to catch all the exceptions from the layers below this one
            catch (ArgumentException ae)
            {
                logger.Error(ae, ae.Message);
                throw CreateHttpResponseException("Input arguments are not valid");
            }
            catch (PermissionsException pe)
            {
                logger.Error(pe, "Creating this resource is forbidden");
                throw CreateHttpResponseException("You don't have permission to create this resource", HttpStatusCode.Forbidden);
            }
            catch (ApplicationException ae)
            {
                logger.Error(ae, ae.Message);
                throw CreateHttpResponseException("Application exception occurred creating PayableDepositDirective resource");
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unknown error occurred creating PayableDepositDirective resource");
            }
        }

        /// <summary>
        /// Updates a Payable Deposit Directive for the current user. This POST request has PUT characteristics, however, only updates
        /// to the PayableDepositDirective Nickname and IsElectronicPaymentRequested flag are accepted. Any other updates are ignored.
        /// 
        /// The BankingAuthenticationToken.Token property is 
        /// required in the X-Step-Up-Authentication header
        /// if account authentication is enabled.
        /// 
        /// The endpoint will reject updates if:
        ///     1. 400 - A deposit's end date occurs before its start date
        ///     2. 400 - A PayableDepositDirective has malformed RoutingId, InstitutionId, or BranchNumber
        ///     3. 409 - The PayableDepositDirective resource has changed on server - see ChangeDateTime of PayableDepositDirective
        ///     4. 409 - The Business Office has a lock on the resource
        /// 
        /// </summary>
        /// <param name="updatedPayableDepositDirective"></param>
        /// <returns>The updated PayableDepositDirective object</returns>
        [HttpPost]
        public async Task<PayableDepositDirective> UpdatePayableDepositDirectiveAsync([FromBody] PayableDepositDirective updatedPayableDepositDirective)
        {
            if (updatedPayableDepositDirective == null)
            {
                throw CreateHttpResponseException("updatedPayableDepositDirective object is required in request body");
            }

            var token = GetStepUpAuthenticationHeaderValue();

            try
            {
                return await payableDepositDirectiveService.UpdatePayableDepositDirectiveAsync(token, updatedPayableDepositDirective);
            }
            catch (ArgumentException ae)
            {
                var message = "Input arguments are not valid";
                logger.Error(ae, message);
                throw CreateHttpResponseException(message);
            }

            catch (PermissionsException pe)
            {
                var message = "You are forbidden from updating this resource";
                logger.Error(pe, message);
                throw CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.Error(knfe, knfe.Message);
                throw CreateNotFoundException("PayableDepositDirective", updatedPayableDepositDirective.Id);
            }
            catch (RecordLockException rle)
            {
                logger.Error(rle, "PersonAddrBnkInfo record is locked in the db.");
                throw CreateHttpResponseException(rle.Message, HttpStatusCode.Conflict);
            }

            catch (Exception e)
            {
                logger.Error(e, "Unknown error occurred");
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Delete a Payable Deposit Directive.
        /// 
        /// The BankingAuthenticationToken.Token property is 
        /// required in the X-Step-Up-Authentication header
        /// if account authentication is enabled.
        /// 
        /// The endpoint will reject updates if:
        ///     1. 400 - Something went wrong trying to delete the resource
        ///     2. 403 - You attempt to delete a deposit directive that is not yours, or you do not have permission to edit payable deposits.
        ///     3. 409 - The Business Office has a lock on the resource, or the server was unable to delete the resource.
        /// 
        /// </summary>
        /// <param name="id">The id of the PayableDepositDirective resource to delete</param>    
        /// <returns>204 Status if deletion of the resource was successful</returns>
        [HttpDelete]
        public async Task<HttpResponseMessage> DeletePayableDepositDirectiveAsync([FromUri] string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw CreateHttpResponseException("Id is required in request body");
            }

            var token = GetStepUpAuthenticationHeaderValue();

            try
            {
                await payableDepositDirectiveService.DeletePayableDepositDirectiveAsync(token, id);
                var response = Request.CreateResponse(HttpStatusCode.NoContent);
                return response;
            }
            catch (ArgumentException ae)
            {
                var message = "Input arguments are not valid";
                logger.Error(ae, message);
                throw CreateHttpResponseException(message);
            }

            catch (PermissionsException pe)
            {
                var message = "You are forbidden from updating this resource";
                logger.Error(pe, message);
                throw CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.Error(knfe, knfe.Message);
                throw CreateNotFoundException("PayableDepositDirective", id);
            }
            catch (RecordLockException rle)
            {
                logger.Error(rle, "PersonAddrBnkInfo record is locked in the db.");
                throw CreateHttpResponseException(rle.Message, HttpStatusCode.Conflict);
            }
            catch (ApplicationException ae)
            {
                logger.Error(ae, "Application exception occurred deleting PersonAddrBnkInfo resource");
                throw CreateHttpResponseException(ae.Message, HttpStatusCode.BadRequest);
            }
            catch (Exception e)
            {
                logger.Error(e, "Unknown error occurred");
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Post a request for authentication to create or update a PayableDepositDirective.
        /// </summary>
        /// <param name="challenge">A challenge object containing the required authentication data</param>
        /// <returns>A BankingAuthenticationToken that can be used to authenticate future Update and Create PayableDepositDirectives. Tokens expire after ten (10) minutes</returns>
        [HttpPost]
        public async Task<BankingAuthenticationToken> AuthenticatePayableDepositDirectiveAsync([FromBody] PayableDepositDirectiveAuthenticationChallenge challenge)
        {
            if (challenge == null)
            {
                throw CreateHttpResponseException("challenge object is required in body of request");
            }

            try
            {
                return await payableDepositDirectiveService.AuthenticatePayableDepositDirectiveAsync(challenge.PayableDepositDirectiveId, challenge.ChallengeValue, challenge.AddressId);
            }
            catch (ArgumentNullException ane)
            {
                logger.Error(ane, ane.Message);
                throw CreateHttpResponseException("Invalid Arguments");
            }
            catch (PermissionsException pe)
            {
                logger.Error(pe, pe.Message);
                throw CreateHttpResponseException("Current user does not have permission to authenticate payable directive", HttpStatusCode.Forbidden);
            }
            catch (BankingAuthenticationException bae)
            {
                logger.Error(bae, bae.Message);
                throw CreateHttpResponseException("Authentication failed", HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.Error(knfe, knfe.Message);
                throw CreateNotFoundException("PayabeDepositDirective", challenge.PayableDepositDirectiveId);
            }
            catch (Exception e)
            {
                logger.Error(e, "Error authenticating payable deposit directive");
                throw CreateHttpResponseException(e.Message);
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

