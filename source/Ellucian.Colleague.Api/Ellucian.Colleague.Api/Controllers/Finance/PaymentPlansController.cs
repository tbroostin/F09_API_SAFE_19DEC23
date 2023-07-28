// Copyright 2014-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Coordination.Finance;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Colleague.Dtos.Finance;
using Ellucian.Web.Http.Controllers;
using slf4net;
using Ellucian.Colleague.Dtos.Finance.Payments;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Ellucian.Web.Http.Filters;
using System.Threading.Tasks;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Api.Controllers.Finance
{
    /// <summary>
    /// Provides access to get and update payment plan information
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Finance)]
    [Metadata(ApiDescription = "Provides access to get and update payment plan information", ApiDomain = "Finance")]
    public class PaymentPlansController : BaseCompressedApiController
    {
        private readonly IPaymentPlanService _service;
        private readonly ILogger _logger;

        /// <summary>
        /// PaymentPlansController class constructor
        /// </summary>
        /// <param name="service">Service of type <see cref="IPaymentPlanService">IPaymentPlanService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public PaymentPlansController(IPaymentPlanService service, ILogger logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Gets all payment plan templates
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can get these resources
        /// </accessComments>
        /// <returns>A list of PaymentPlanTemplate DTOs</returns>
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        [HttpGet]
        public IEnumerable<PaymentPlanTemplate> GetPaymentPlanTemplates()
        {
            try
            {
                return _service.GetPaymentPlanTemplates();
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException();
            }
        }

        /// <summary>
        /// Get the specified payment plan
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.STUDENT.ACCOUNT.ACTIVITY
        /// permission or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="paymentPlanId">ID of the payment plan</param>
        /// <returns>A PaymentPlan DTO</returns>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if the payment plan ID is not provided.</exception>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user does not have the role or permissions required to get the payment plan</exception>
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        [HttpGet]
        public PaymentPlan GetPaymentPlan(string paymentPlanId)
        {
            if (string.IsNullOrEmpty(paymentPlanId))
            {
                string message = "Payment plan ID must be specified.";
                _logger.Error(message);
                throw CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
            try
            {
                return _service.GetPaymentPlan(paymentPlanId);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException();
            }
        }

        /// <summary>
        /// Gets the specified payment plan template
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can get these resources
        /// </accessComments>
        /// <param name="templateId">ID of the payment plan template</param>
        /// <returns>A PaymentPlanTemplate DTO</returns>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if the payment plan is not provided.</exception>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user does not have the role or permissions required to create the payment plan</exception>
        [ParameterSubstitutionFilter]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        [HttpGet]
        public PaymentPlanTemplate GetPaymentPlanTemplate(string templateId)
        {
            if (string.IsNullOrEmpty(templateId))
            {
                string message = "Payment plan template ID must be specified.";
                _logger.Error(message);
                throw CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
            try
            {
                return _service.GetPaymentPlanTemplate(templateId);
            }
            catch (KeyNotFoundException knfe)
            {
                _logger.Error(knfe.ToString());
                throw CreateNotFoundException("Payment Plan Template", templateId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException();
            }
        }

        /// <summary>
        /// Post the approval of a payment plan's terms and conditions
        /// </summary>
        /// <accessComments>
        /// Users may change their own data only
        /// </accessComments>
        /// <param name="approval">The payment plan approval information</param>
        /// <returns>The updated <see cref="PaymentPlanApproval">Payment Plan approval</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if the payment plan is not provided.</exception>
        [HttpPost]
        public PaymentPlanApproval PostAcceptTerms(PaymentPlanTermsAcceptance approval)
        {
            try
            {
                return _service.ApprovePaymentPlanTerms(approval);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                _logger.Error(csee, csee.Message);
                throw CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException();
            }
        }

        /// <summary>
        /// Get an approval for a payment plan
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.STUDENT.ACCOUNT.ACTIVITY
        /// permission or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="approvalId">Approval ID</param>
        /// <returns>A PaymentPlanApproval DTO</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if the payment plan approval is not provided.</exception>
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        [HttpGet]
        public PaymentPlanApproval GetPaymentPlanApproval(string approvalId)
        {
            if (string.IsNullOrEmpty(approvalId))
            {
                string message = "Payment plan approval ID must be specified.";
                _logger.Error(message);
                throw CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
            try
            {
                return _service.GetPaymentPlanApproval(approvalId);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                _logger.Error(csee, csee.Message);
                throw CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException();
            }
        }


        /// <summary>
        /// Retrieves the down payment information for a payment control, payment plan, pay method and amount
        /// </summary>
        /// <accessComments>
        /// Users may request their own data only
        /// </accessComments>
        /// <param name="planId">Payment plan ID</param>
        /// <param name="payMethod">Payment method code</param>
        /// <param name="amount">Total payment amount</param>
        /// <param name="payControlId">Registration payment control ID</param>
        /// <returns>List of payments to be made</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        [ParameterSubstitutionFilter(ParameterNames = new string[] { "planId" })]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        [HttpGet]
        public Payment GetPlanPaymentSummary(string planId, string payMethod, decimal amount, string payControlId)
        {
            try
            {
                return _service.GetPlanPaymentSummary(planId, payMethod, amount, payControlId);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
        }

        /// <summary>
        /// Gets a proposed payment plan for a given person for a given term and receivable type with total charges
        /// no greater than the stated amount
        /// </summary>
        /// <accessComments>
        /// Users may request their own data only
        /// </accessComments>
        /// <param name="personId">Proposed plan owner ID</param>
        /// <param name="termId">Billing term ID</param>
        /// <param name="receivableTypeCode">Receivable Type Code</param>
        /// <param name="planAmount">Maximum total payment plan charges</param>
        /// <returns>Proposed payment plan</returns>
        [HttpGet]
        [ParameterSubstitutionFilter(ParameterNames = new string[] { "termId" })]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        public async Task<PaymentPlan> GetProposedPaymentPlanAsync([FromUri]string personId, [FromUri]string termId,
            [FromUri]string receivableTypeCode, [FromUri]decimal planAmount)
        {
            try
            {
                return await _service.GetProposedPaymentPlanAsync(personId, termId, receivableTypeCode, planAmount);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException();
            }
        }
    }
}
