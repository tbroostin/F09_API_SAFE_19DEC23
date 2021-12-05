// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Finance;
using Ellucian.Colleague.Dtos.Finance;
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

namespace Ellucian.Colleague.Api.Controllers.Finance
{
    /// <summary>
    /// Provides access to get and update Accounts Receivable information.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Finance)]
    public class ReceivablesController : BaseCompressedApiController
    {
        private readonly IAccountsReceivableService _service;
        private readonly IPaymentPlanService _payPlanService;
        private readonly ILogger _logger;
        private const string _restrictedHeaderName = "X-Content-Restricted";
        private const string _restrictedHeaderValue = "partial";

        /// <summary>
        /// AccountsReceivableController class constructor
        /// </summary>
        /// <param name="service">Service of type <see cref="IAccountsReceivableService">IAccountsReceivableService</see></param>
        /// <param name="payPlanService">Service of type <see cref="IPaymentPlanService">IPaymentPlanService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public ReceivablesController(IAccountsReceivableService service, IPaymentPlanService payPlanService, ILogger logger)
        {
            _service = service;
            _payPlanService = payPlanService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves the account holder information for a specified person.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.STUDENT.ACCOUNT.ACTIVITY
        /// permission or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="personId">Person ID</param>
        /// <returns>The <see cref="AccountHolder">AccountHolder</see> information</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        [Obsolete("Obsolete as of API version 1.3, use GetAccountHolder2 instead.")]
        public AccountHolder GetAccountHolder(string personId)
        {
            try
            {
                return _service.GetAccountHolder(personId);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
        }

        /// <summary>
        /// Retrieves the privacy-restricted account holder information for a specified person.
        /// </summary>
        /// <param name="personId">Person ID</param>
        /// <param name="bypassCache">bypassCache</param>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.STUDENT.ACCOUNT.ACTIVITY
        /// permission or proxy permissions can request other users' data
        /// </accessComments>
        /// <returns>The <see cref="AccountHolder">AccountHolder</see> information. Account Holder privacy is enforced by this 
        /// response. If any Account Holder has an assigned privacy code that the user is not authorized to access, the AccountHolder response object is returned with a
        /// X-Content-Restricted header with a value of "partial" to indicate only partial information is returned for some subset of account holders. In this situation, 
        /// all details except the advisee name are cleared from the specific AccountHolder object.</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        public async Task<AccountHolder> GetAccountHolder2Async(string personId, bool bypassCache)
        {
            try
            {
                var privacyWrapper = await _service.GetAccountHolder2Async(personId, bypassCache);
                var accountHolder = privacyWrapper.Dto as AccountHolder;
                if (privacyWrapper.HasPrivacyRestrictions)
                {
                    HttpContext.Current.Response.AppendHeader(_restrictedHeaderName, _restrictedHeaderValue);
                }
                return accountHolder;
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.Info(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves the information for a single accountholder if an id is provided,
        /// or the matching accountholders if a first and last name are provided.  
        /// In the latter case, a middle name is optional.
        /// Matching is done by partial name; i.e., 'Bro' will match 'Brown' or 'Brodie'. 
        /// Capitalization is ignored.
        /// </summary>
        /// <remarks>the following input is legal
        /// <list type="bullet">
        /// <item>a Colleague id.  Short ids will be zero-padded.</item>
        /// <item>First Last</item>
        /// <item>First Middle Last</item>
        /// <item>Last, First</item>
        /// <item>Last, First Middle</item>
        /// </list>
        /// </remarks>
        /// <param name="criteria">either a Person ID or a first and last name.  A middle name is optional.</param>
        /// <returns>An enumeration of <see cref="AccountHolder">AccountHolder</see> information</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        [HttpPost]
        public async Task<IEnumerable<AccountHolder>> QueryAccountHoldersByPostAsync([FromBody]string criteria)
        {
            try
            {
                return await _service.SearchAccountHoldersAsync(criteria);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.Info(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves the privacy-restricted information for a single accountholder if an id is provided,
        /// or the matching accountholders if a first and last name are provided.  
        /// In the latter case, a middle name is optional.
        /// Matching is done by partial name; i.e., 'Bro' will match 'Brown' or 'Brodie'. 
        /// Capitalization is ignored.
        /// </summary>
        /// <accessComments>
        /// Users who have VIEW.STUDENT.ACCOUNT.ACTIVITY permission may request this data
        /// </accessComments>
        /// <remarks>the following input is legal
        /// <list type="bullet">
        /// <item>a Colleague id.  Short ids will be zero-padded.</item>
        /// <item>First Last</item>
        /// <item>First Middle Last</item>
        /// <item>Last, First</item>
        /// <item>Last, First Middle</item>
        /// </list>
        /// </remarks>
        /// <param name="criteria">either a Person ID or a first and last name.  A middle name is optional.</param>
        /// <returns>An enumeration of <see cref="AccountHolder">AccountHolder</see> information. Account Holder privacy is enforced by this 
        /// response. If any Account Holder has an assigned privacy code that the user is not authorized to access, the AccountHolder response object is returned with a
        /// X-Content-Restricted header with a value of "partial" to indicate only partial information is returned for some subset of account holders. In this situation, 
        /// all details except the advisee name are cleared from the specific AccountHolder object.</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        [HttpPost]
        [Obsolete("Obsolete as of API version 1.16, use QueryAccountHoldersByPost3Async instead.")]
        public async Task<IEnumerable<AccountHolder>> QueryAccountHoldersByPostAsync2([FromBody]string criteria)
        {
            if (string.IsNullOrEmpty(criteria))
            {
                throw CreateHttpResponseException("criteria cannot be null");
            }
            try
            {
                var privacyWrapper = await _service.SearchAccountHoldersAsync2(criteria);
                var accountHolders = privacyWrapper.Dto as List<AccountHolder>;
                if (privacyWrapper.HasPrivacyRestrictions)
                {
                    HttpContext.Current.Response.AppendHeader(_restrictedHeaderName, _restrictedHeaderValue);
                }
                return (IEnumerable<AccountHolder>)accountHolders;
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.Info(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves the privacy-restricted information for a single accountholder if an id is provided,
        /// or the matching accountholders if a first and last name are provided.  
        /// In the latter case, a middle name is optional.
        /// Matching is done by partial name; i.e., 'Bro' will match 'Brown' or 'Brodie'. 
        /// Capitalization is ignored.
        /// </summary>
        /// <accessComments>
        /// Users who have VIEW.STUDENT.ACCOUNT.ACTIVITY permission may request this data
        /// </accessComments>
        /// <remarks>the following input is legal
        /// <list type="bullet">
        /// <item>a Colleague id.  Short ids will be zero-padded.</item>
        /// <item>First Last</item>
        /// <item>First Middle Last</item>
        /// <item>Last, First</item>
        /// <item>Last, First Middle</item>
        /// </list>
        /// </remarks>
        /// <param name="criteria">either a Person ID or a first and last name, or a list of Person Ids.</param>
        /// <returns>An enumeration of <see cref="AccountHolder">AccountHolder</see> information. Account Holder privacy is enforced by this 
        /// response. If any Account Holder has an assigned privacy code that the user is not authorized to access, the AccountHolder response object is returned with a
        /// X-Content-Restricted header with a value of "partial" to indicate only partial information is returned for some subset of account holders. In this situation, 
        /// all details except the advisee name are cleared from the specific AccountHolder object.</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        [HttpPost]
        public async Task<IEnumerable<AccountHolder>> QueryAccountHoldersByPost3Async([FromBody]AccountHolderQueryCriteria criteria)
        {
            if (criteria == null)
            {
                throw CreateHttpResponseException("criteria cannot be null");
            }
            if ((criteria.Ids == null || !criteria.Ids.Any())
                && string.IsNullOrEmpty(criteria.QueryKeyword))
            {
                throw CreateHttpResponseException("criteria must contain either a list of account holder ids or a query keyword");
            }
            try
            {
                var privacyWrapper = await _service.SearchAccountHolders3Async(criteria);
                var accountHolders = privacyWrapper.Dto as List<AccountHolder>;
                if (privacyWrapper.HasPrivacyRestrictions)
                {
                    HttpContext.Current.Response.AppendHeader(_restrictedHeaderName, _restrictedHeaderValue);
                }
                return (IEnumerable<AccountHolder>)accountHolders;
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.Info(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves a set of specified invoices.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.STUDENT.ACCOUNT.ACTIVITY
        /// permission or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="invoiceIds">Comma-delimited list of invoice IDs</param>
        /// <returns>The collection of <see cref="Invoice">Invoice</see> information</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        [Obsolete("Obsolete as of API version 1.12, use QueryInvoicesByPostAsync instead.")]
        public IEnumerable<Invoice> GetInvoices(string invoiceIds)
        {
            if (String.IsNullOrEmpty(invoiceIds))
            {
                return new List<Invoice>();
            }

            var ids = new List<string>(invoiceIds.Split(','));
            try
            {
                return _service.GetInvoices(ids);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch(Exception ex)
            {
                _logger.Error(ex, ex.Message);
                throw CreateHttpResponseException("Cannot retrieve invoices with the specified information. See log for details.");
            }
        }

        /// <summary>
        /// Accepts a list invoice Ids and will post a query against invoices.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.STUDENT.ACCOUNT.ACTIVITY
        /// permission or proxy permissions can request other users' data
        ///</accessComments>
        /// <param name="criteria"><see cref="InvoiceQueryCriteria">Query Criteria</see> including the list of Invoice Ids to use to retrieve invoices.</param>
        /// <returns>List of <see cref="Invoice">Invoices</see> objects. </returns>
        [HttpPost]
        public async Task<IEnumerable<Invoice>> QueryInvoicesByPostAsync([FromBody]InvoiceQueryCriteria criteria)
        {
            try
            {
                return await _service.QueryInvoicesAsync(criteria.InvoiceIds);
            }
            catch (PermissionsException pex)
            {
                _logger.Error(pex.Message);
                throw CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (ArgumentOutOfRangeException aex)
            {
                _logger.Error(aex.Message);
                throw CreateHttpResponseException(aex.Message, HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Accepts a list invoice Ids and will post a query against invoice payments.
        /// Retrieves a set of specified invoice payment items which inherits from invoice but has the addition of an amount paid on the invoice
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.STUDENT.ACCOUNT.ACTIVITY
        /// permission or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="criteria"><see cref="InvoiceQueryCriteria">Query Criteria</see> including the list of Invoice Ids. At least 1 invoice Id must be specified.</param>
        /// <returns>List of <see cref="InvoicePayment">InvoicePayments</see> objects. </returns>
        [HttpPost]
        public async Task<IEnumerable<InvoicePayment>> QueryInvoicePaymentsByPostAsync([FromBody]InvoiceQueryCriteria criteria)
        {
            try
            {
                return await _service.QueryInvoicePaymentsAsync(criteria.InvoiceIds);
            }
            catch (PermissionsException pex)
            {
                _logger.Error(pex.Message);
                throw CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (ArgumentOutOfRangeException aex)
            {
                _logger.Error(aex.Message);
                throw CreateHttpResponseException(aex.Message, HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Get a set of specified payments
        /// </summary>
        /// <param name="paymentIds">Comma-delimited list of payment IDs</param>
        /// <returns>The collection of <see cref="ReceivablePayment">ReceivablePayment</see> information</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        public IEnumerable<ReceivablePayment> GetPayments(string paymentIds)
        {
            if (String.IsNullOrEmpty(paymentIds))
            {
                return new List<ReceivablePayment>();
            }

            var ids = new List<string>(paymentIds.Split(','));
            try
            {
                return _service.GetPayments(ids);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
        }

        /// <summary>
        /// OBSOLETE - Use GetDepositsDue() in DepositsController to get deposits due for a student.
        /// Get the deposits due for a specified student
        /// </summary>
        /// <param name="id">Student ID</param>
        /// <returns>A list of <see cref="DepositDue">deposits due</see> for the student</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        [Obsolete("Obsolete as of API version 1.3")]
        public IEnumerable<DepositDue> GetDepositsDue(string id)
        {
            try
            {
                return _service.GetDepositsDue(id);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
        }

        /// <summary>
        /// OBSOLETE - Use GetDepositTypes() in DepositsController to get all deposit types.
        /// Get all deposit types
        /// </summary>
        /// <returns>A list of all <see cref="DepositType">deposit types</see></returns>
        [Obsolete("Obsolete as of API version 1.3")]
        public IEnumerable<DepositType> GetDepositTypes()
        {
            return _service.GetDepositTypes();
        }

        /// <summary>
        /// Retrieves all receivable types.
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can get these resources
        /// </accessComments>
        /// <returns>A list of all <see cref="ReceivableType">receivable types</see></returns>
        public IEnumerable<ReceivableType> GetReceivableTypes()
        {
            return _service.GetReceivableTypes();
        }

        /// <summary>
        /// Gets valid billing term payment plan information from a proposed billing term payment plan information collection
        /// </summary>
        /// <accessComments>
        /// Users may request their own data only
        /// </accessComments>
        /// <param name="billingTerms">List of payment items</param>
        /// <returns>Valid billing term payment plan information from a proposed billing term payment plan information collection</returns>
        [HttpPost]
        [Obsolete("Obsolete as of API version 1.16, use QueryAccountHolderPaymentPlanOptions2Async instead.")]
        public async Task<PaymentPlanEligibility> QueryAccountHolderPaymentPlanOptionsAsync([FromBody]IEnumerable<BillingTermPaymentPlanInformation> billingTerms)
        {
            if (billingTerms == null || !billingTerms.Any())
            {
                throw CreateHttpResponseException("billingTerms cannot be null or empty");
            }
            try
            {
                return await _payPlanService.GetBillingTermPaymentPlanInformationAsync(billingTerms);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Gets valid billing term payment plan information from a proposed billing term payment plan information collection
        /// </summary>
        /// <accessComments>
        /// Users may request their own data only
        /// </accessComments>
        /// <param name="criteria">payment plan query criteria</param>
        /// <returns>Valid billing term payment plan information from a proposed billing term payment plan information collection</returns>
        [HttpPost]
        public async Task<PaymentPlanEligibility> QueryAccountHolderPaymentPlanOptions2Async([FromBody]PaymentPlanQueryCriteria criteria)
        {
            if (criteria == null)
            {
                throw CreateHttpResponseException("criteria cannot be null");
            }
            try
            {
                return await _payPlanService.GetBillingTermPaymentPlanInformation2Async(criteria);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Retrieves all charge codes.
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can get these resources
        /// </accessComments>
        /// <returns>A list of all <see cref="ChargeCode">receivable types</see></returns>
        [HttpGet]
        public async Task<IEnumerable<ChargeCode>> GetChargeCodesAsync()
        {
            try
            {
                return await _service.GetChargeCodesAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }
    }
}
