// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Finance;
using Ellucian.Colleague.Dtos.Finance.Payments;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System.Collections.Generic;
using System;

namespace Ellucian.Colleague.Api.Controllers.Finance
{
    /// <summary>
    /// Provides access to process student payments.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Finance)]
    public class PaymentController : BaseCompressedApiController
    {
        private readonly IPaymentService _service;
        private readonly IAccountsReceivableService _arService;
        private readonly ILogger _logger;

        /// <summary>
        /// PaymentController class constructor
        /// </summary>
        /// <param name="service">Service of type <see cref="IPaymentService">IPaymentService</see></param>
        /// <param name="arService">Service of type <see cref="IAccountsReceivableService">IAccountsReceivableService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public PaymentController(IPaymentService service, IAccountsReceivableService arService, ILogger logger)
        {
            _service = service;
            _arService = arService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves information required to process a student payment.
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can get these resources
        /// </accessComments>
        /// <param name="distribution">Distribution ID</param>
        /// <param name="paymentMethod">Payment Method</param>
        /// <param name="amountToPay">Amount being paid</param>
        /// <returns>The <see cref="PaymentConfirmation">Payment Confirmation</see> information</returns>
        public PaymentConfirmation GetPaymentConfirmation(string distribution, string paymentMethod, string amountToPay)
        {
            return _service.GetPaymentConfirmation(distribution, paymentMethod, amountToPay);
        }

        /// <summary>
        /// Process a student payment using a credit card
        /// </summary>
        /// <accessComments>
        /// Users may change their own data. Additionally, users who have proxy permissions can
        /// change other users' data
        /// </accessComments>
        /// <param name="paymentDetails">The <see cref="Payment">Payment</see> information</param>
        /// <returns>The <see cref="PaymentProvider">Payment Provider</see> information</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to make this payment</exception>
        public PaymentProvider PostPaymentProvider(Payment paymentDetails)
        {
            try
            {
                return _service.PostPaymentProvider(paymentDetails);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
        }

        /// <summary>
        /// Retrieves the information needed to acknowledge a payment.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.STUDENT.ACCOUNT.ACTIVITY
        /// permission or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="transactionId">e-Commerce Transaction ID</param>
        /// <param name="cashReceiptId">Cash Receipt ID</param>
        /// <returns>The <see cref="PaymentReceipt">Payment Receipt</see> information</returns>
        public PaymentReceipt GetPaymentReceipt(string transactionId, string cashReceiptId)
        {
            try
            {
                return _service.GetPaymentReceipt(transactionId, cashReceiptId);
            }
            catch(PermissionsException pex)
            {
                _logger.Error(pex, pex.Message);
                throw CreateHttpResponseException("Permission denied to access receipt information. See log for details.", HttpStatusCode.Forbidden);
            }
            catch(Exception ex)
            {
                _logger.Error(ex, ex.Message);
                if (ex.Message == "Your payment has been canceled.")
                {
                    throw CreateHttpResponseException("Your payment has been canceled. Contact the system administrator if you are receiving this message in error.");
                }
                else
                {
                    throw CreateHttpResponseException("Could not retrieve payment receipt with the specified information. See log for details.");
                }
            }
        }

        /// <summary>
        /// Process a student payment using an electronic check
        /// </summary>
        /// <accessComments>
        /// Users may change their own data. Additionally, users who have proxy permissions can
        /// change other users' data
        /// </accessComments>
        /// <param name="paymentDetails">The <see cref="Payment">Payment</see> information</param>
        /// <returns>The <see cref="ElectronicCheckProcessingResult">Electronic Check Processing Result</see> information</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to make this payment</exception>
        public ElectronicCheckProcessingResult PostProcessElectronicCheck(Payment paymentDetails)
        {
            try
            {
                return _service.PostProcessElectronicCheck(paymentDetails);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
        }

        /// <summary>
        /// Retrieves the payer information needed to process an e-check.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have proxy permissions can
        /// request other users' data
        /// </accessComments>
        /// <param name="personId">Payer ID</param>
        /// <returns>The <see cref="ElectronicCheckPayer">Electronic Check Payer</see> information</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to make this payment</exception>
        public ElectronicCheckPayer GetCheckPayerInformation(string personId)
        {
            try
            {
                return _service.GetCheckPayerInformation(personId);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
        }

        /// <summary>
        /// Retrieves the payment distributions for a student, account types, and payment process.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.STUDENT.ACCOUNT.ACTIVITY 
        /// permission or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="studentId">Student ID</param>
        /// <param name="accountTypes">Comma-delimited list of account type codes</param>
        /// <param name="paymentProcess">Code of payment process</param>
        /// <returns>List of payment distributions</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        public IEnumerable<string> GetPaymentDistributions(string studentId, string accountTypes, string paymentProcess)
        {
            var types = (string.IsNullOrEmpty(accountTypes)) ? new List<string>() : new List<string>(accountTypes.Split(','));
            try
            {
                return _arService.GetDistributions(studentId, types, paymentProcess);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
        }
    }
}
