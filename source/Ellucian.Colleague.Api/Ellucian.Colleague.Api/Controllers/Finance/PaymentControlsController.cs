// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Finance;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Colleague.Dtos.Finance;
using Ellucian.Colleague.Dtos.Finance.Payments;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.Finance
{
    /// <summary>
    /// Provides access to get and update registration billing information
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Finance)]
    public class PaymentControlsController : BaseCompressedApiController
    {
        private readonly IRegistrationBillingService _service;
        private readonly ILogger _logger;

        /// <summary>
        /// RegistrationBillingController class constructor
        /// </summary>
        /// <param name="service">Service of type <see cref="IRegistrationBillingService">IRegistrationBillingService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public PaymentControlsController(IRegistrationBillingService service, ILogger logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves a specified registration payment control.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.STUDENT.ACCOUNT.ACTIVITY
        /// permission or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="id">Registration payment control ID</param>
        /// <returns>The <see cref="RegistrationPaymentControl">Registration Payment Control</see> information</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        public RegistrationPaymentControl Get(string id)
        {
            try
            {
                return _service.GetPaymentControl(id);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
        }

        /// <summary>
        /// Retrieves the incomplete payment controls for a student.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.STUDENT.ACCOUNT.ACTIVITY
        /// permission or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="studentId">Student ID</param>
        /// <returns>The list of <see cref="RegistrationPaymentControl">Registration Payment Control</see> information</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        public IEnumerable<RegistrationPaymentControl> GetStudent(string studentId)
        {
            try
            {
                return _service.GetStudentPaymentControls(studentId);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
        }

        /// <summary>
        /// Retrieves a registration payment control document.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.STUDENT.ACCOUNT.ACTIVITY
        /// permission or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="id">Registration payment control ID</param>
        /// <param name="documentId">Document ID</param>
        /// <returns>The <see cref="TextDocument">Text Document</see> information</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        public TextDocument GetDocument(string id, string documentId)
        {
            try
            {
                return _service.GetPaymentControlDocument(id, documentId);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
        }

        /// <summary>
        /// Post the approval of a registration's terms and conditions
        /// </summary>
        /// <accessComments>
        /// Users may change their own data only
        /// </accessComments>
        /// <param name="approval">The registration approval information</param>
        /// <returns>The updated <see cref="RegistrationTermsApproval">registration approval</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        [Obsolete("Obsolete endpoint as of API 1.5: DTO has changed; use PostAcceptTerms2 instead.", false)]
        public RegistrationTermsApproval PostAcceptTerms(PaymentTermsAcceptance approval)
        {
            try
            {
                return _service.ApproveRegistrationTerms(approval);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
        }

        /// <summary>
        /// Post the approval of a registration's terms and conditions
        /// </summary>
        /// <accessComments>
        /// Users may change their own data only
        /// </accessComments>
        /// <param name="approval">The registration approval information</param>
        /// <returns>The updated <see cref="RegistrationTermsApproval">registration approval</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        public RegistrationTermsApproval2 PostAcceptTerms2(PaymentTermsAcceptance2 approval)
        {
            try
            {
                return _service.ApproveRegistrationTerms2(approval);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
        }

        /// <summary>
        /// Retrieves payment options for a student for a term.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.STUDENT.ACCOUNT.ACTIVITY permission or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="id">Registration payment control ID</param>
        /// <returns>The <see cref="ImmediatePaymentOptions">Immediate Payment Options</see> information</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        public ImmediatePaymentOptions GetOptions(string id)
        {
            try
            {
                return _service.GetPaymentOptions(id);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
        }

        /// <summary>
        /// Updates a payment control
        /// </summary>
        /// <accessComments>
        /// Users may change their own data only
        /// </accessComments>
        /// <param name="rpcDto"><see cref="RegistrationPaymentControl">Registration Payment Control</see> DTO to update</param>
        /// <returns>The updated <see cref="RegistrationPaymentControl">Registration Payment Control</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        public RegistrationPaymentControl Put(RegistrationPaymentControl rpcDto)
        {
            try
            {
                return _service.UpdatePaymentControl(rpcDto);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
        }

        /// <summary>
        /// Retrieves the payment summary for a payment control, pay method, and payment amount.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data only
        /// </accessComments>
        /// <param name="id">Registration payment control ID</param>
        /// <param name="payMethod">Payment method code</param>
        /// <param name="amount">Total payment amount</param>
        /// <returns>The List of <see cref="Payment">payments</see> to be made</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        public IEnumerable<Payment> GetSummary(string id, string payMethod, decimal amount)
        {
            try
            {
                return _service.GetPaymentSummary(id, payMethod, amount);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
        }

        /// <summary>
        /// Starts a registration payment
        /// </summary>
        /// <accessComments>
        /// Users may create their own data only
        /// </accessComments>
        /// <param name="payment">The registration payment</param>
        /// <returns>Payment provider information to start a payment</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        public PaymentProvider PostStartPayment(Payment payment)
        {
            try
            {
                return _service.StartRegistrationPayment(payment);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
        }

        /// <summary>
        /// Retrieves the registration terms approval.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.STUDENT.ACCOUNT.ACTIVITY
        /// permission or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="id">Terms approval ID</param>
        /// <returns>The <see cref="RegistrationTermsApproval">terms approval</see> information</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        [Obsolete("Obsolete as of API version 1.5, use GetRegistrationTermsApproval2 instead.")]
        public RegistrationTermsApproval GetTermsApproval(string id)
        {
            try
            {
                return _service.GetTermsApproval(id);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
        }

        /// <summary>
        /// Retrieves the registration terms approval.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.STUDENT.ACCOUNT.ACTIVITY
        /// permission or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="id">Terms approval ID</param>
        /// <returns>The <see cref="RegistrationTermsApproval2">terms approval</see> information</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        public RegistrationTermsApproval2 GetTermsApproval2(string id)
        {
            try
            {
                return _service.GetTermsApproval2(id);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
        }

        /// <summary>
        /// Get a proposed payment plan
        /// </summary>
        /// <accessComments>
        /// Users may request their own data only
        /// </accessComments>
        /// <param name="payControlId">ID of a payment control record</param>
        /// <param name="receivableType">Receivable Type for proposed payment plan</param>
        /// <returns>The proposed<see cref="PaymentPlan">Payment Plan</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to get proposed payment plan</exception>
        [ParameterSubstitutionFilter]
        public PaymentPlan GetProposedPaymentPlan(string payControlId, string receivableType)
        {
            try
            {
                return _service.GetProposedPaymentPlan(payControlId, receivableType);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
        }
    }
}
