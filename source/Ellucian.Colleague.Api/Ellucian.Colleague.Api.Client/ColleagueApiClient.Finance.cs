// Copyright 2012-2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Colleague.Dtos.Finance;
using Ellucian.Colleague.Dtos.Finance.AccountActivity;
using Ellucian.Colleague.Dtos.Finance.AccountDue;
using Ellucian.Colleague.Dtos.Finance.Configuration;
using Ellucian.Colleague.Dtos.Finance.Payments;
using Ellucian.Web.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Client
{
    public partial class ColleagueApiClient
    {
        #region AccountActivity

        /// <summary>
        /// Get the account activity summary for a student (for AA dropdown)
        /// </summary>
        /// <param name="personId">Student ID</param>
        /// <returns>An AccountActivityPeriods DTO</returns>
        public AccountActivityPeriods GetAccountActivityPeriodsForStudent(string personId)
        {
            var baseUrl = UrlUtility.CombineUrlPath(_accountActivityPeriodsForStudentPath, personId);

            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

            var responseString = ExecuteGetRequestWithResponse(baseUrl, headers: headers);

            var activityPeriods = JsonConvert.DeserializeObject<AccountActivityPeriods>(responseString.Content.ReadAsStringAsync().Result);

            return activityPeriods;
        }

        /// <summary>
        /// Get the account activity for a student for a specific term
        /// </summary>
        /// <param name="termId">Term ID</param>
        /// <param name="personId">Student ID</param>
        /// <returns>A DetailedAccountPeriod DTO</returns>
        [Obsolete("Obsolete as of API version 1.8, use GetAccountActivityByTermForStudent2 instead.", false)]
        public DetailedAccountPeriod GetAccountActivityByTermForStudent(string termId, string personId)
        {
            var baseUrl = UrlUtility.CombineUrlPath(_accountActivityByTermForStudentPath, personId);
            var queryString = UrlUtility.BuildEncodedQueryString("TermId", termId);

            var combinedUrl = UrlUtility.CombineUrlPathAndArguments(baseUrl, queryString);

            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

            var response = ExecuteGetRequestWithResponse(combinedUrl, headers: headers);

            var activity = JsonConvert.DeserializeObject<DetailedAccountPeriod>(response.Content.ReadAsStringAsync().Result);

            return activity;
        }

        /// <summary>
        /// Get the account activity for a student for a specific term
        /// </summary>
        /// <param name="termId">Term ID</param>
        /// <param name="personId">Student ID</param>
        /// <returns>A DetailedAccountPeriod DTO</returns>
        public DetailedAccountPeriod GetAccountActivityByTermForStudent2(string termId, string personId)
        {
            var baseUrl = UrlUtility.CombineUrlPath(_accountActivityByTermForStudentPath, personId);
            var queryString = UrlUtility.BuildEncodedQueryString("TermId", termId);

            var combinedUrl = UrlUtility.CombineUrlPathAndArguments(baseUrl, queryString);

            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);

            var response = ExecuteGetRequestWithResponse(combinedUrl, headers: headers);

            var activity = JsonConvert.DeserializeObject<DetailedAccountPeriod>(response.Content.ReadAsStringAsync().Result);

            return activity;
        }

        /// <summary>
        /// Get the account activity for a student for a specific PCF period
        /// </summary>
        /// <param name="associatedPeriods">A list of term IDs in this period</param>
        /// <param name="startDate">Starting date of the period</param>
        /// <param name="endDate">Ending date of the period</param>
        /// <param name="personId">Student ID</param>
        /// <returns></returns>
        [Obsolete("Obsolete as of API 1.8, use GetAccountActivityPeriodsForStudent2 instead.", false)]
        public DetailedAccountPeriod GetAccountActivityByPeriodForStudent(IEnumerable<string> associatedPeriods, DateTime? startDate, DateTime? endDate, string personId)
        {
            var outboundArguments = new AccountActivityPeriodArguments() { AssociatedPeriods = associatedPeriods, StartDate = startDate, EndDate = endDate };

            var baseUrl = UrlUtility.CombineUrlPath(_accountActivityByPeriodForStudentPath, personId);

            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

            var response = ExecutePostRequestWithResponse(outboundArguments, baseUrl, headers: headers);

            var activity = JsonConvert.DeserializeObject<DetailedAccountPeriod>(response.Content.ReadAsStringAsync().Result);

            return activity;
        }

        /// <summary>
        /// Get the account activity for a student for a specific PCF period
        /// </summary>
        /// <param name="associatedPeriods">A list of term IDs in this period</param>
        /// <param name="startDate">Starting date of the period</param>
        /// <param name="endDate">Ending date of the period</param>
        /// <param name="personId">Student ID</param>
        /// <returns></returns>
        public DetailedAccountPeriod GetAccountActivityByPeriodForStudent2(IEnumerable<string> associatedPeriods, DateTime? startDate, DateTime? endDate, string personId)
        {
            var outboundArguments = new AccountActivityPeriodArguments() { AssociatedPeriods = associatedPeriods, StartDate = startDate, EndDate = endDate };

            var baseUrl = UrlUtility.CombineUrlPath(_accountActivityByPeriodForStudentPath, personId);

            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);

            var response = ExecutePostRequestWithResponse(outboundArguments, baseUrl, headers: headers);

            var activity = JsonConvert.DeserializeObject<DetailedAccountPeriod>(response.Content.ReadAsStringAsync().Result);

            return activity;
        }

        /// <summary>
        /// Gets student award disbursement information for the specified award for the specified year
        /// </summary>
        /// <param name="studentId">student id</param>
        /// <param name="awardYear">award year</param>
        /// <param name="awardId">award id</param>
        /// <returns>StudentAwardDisbursementInfo DTO</returns>
        public async Task<StudentAwardDisbursementInfo> GetStudentAwardDisbursementInfoAsync(string studentId, string awardYear, string awardId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (string.IsNullOrEmpty(awardYear))
            {
                throw new ArgumentNullException("awardYear");
            }
            if (string.IsNullOrEmpty(awardId))
            {
                throw new ArgumentNullException("awardId");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_studentsPath, studentId, _studentAwardDisbursementsInfoPath, awardYear, awardId);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderStudentFinanceDisbursementsVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<StudentAwardDisbursementInfo>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to retrieve student award disbursement data.");
                throw;
            }
        }

        #endregion

        #region AccountDue

        /// <summary>
        /// Get detailed account due information for a student by term
        /// </summary>
        /// <param name="personId">Student ID</param>
        /// <returns>An AccountDue DTO</returns>
        public AccountDue GetPaymentsDueByTermForStudent(string personId)
        {
            var baseUrl = UrlUtility.CombineUrlPath(_getPaymentsDueByTermForStudentPath, personId);

            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

            var responseString = ExecuteGetRequestWithResponse(baseUrl, headers: headers);

            var paymentsDue = JsonConvert.DeserializeObject<AccountDue>(responseString.Content.ReadAsStringAsync().Result);

            return paymentsDue;
        }

        /// <summary>
        /// Get detailed account due information for a student by period
        /// </summary>
        /// <param name="personId">Student ID</param>
        /// <returns>An AccountDuePeriod DTO</returns>
        public AccountDuePeriod GetPaymentsDueByPeriodForStudent(string personId)
        {
            var baseUrl = UrlUtility.CombineUrlPath(_paymentsDueByPeriodForStudentPath, personId);

            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

            var responseString = ExecuteGetRequestWithResponse(baseUrl, headers: headers);

            var paymentsDue = JsonConvert.DeserializeObject<AccountDuePeriod>(responseString.Content.ReadAsStringAsync().Result);

            return paymentsDue;
        }

        #endregion

        #region AccountsReceivable

        /// <summary>
        /// Get a privacy-restricted accountholder by ID
        /// </summary>
        /// <param name="id">Accountholder ID</param>
        /// <returns>The AccountHolder DTO</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public AccountHolder GetAccountHolder2(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID is required for accountholder retrieval.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_receivablesPath, "account-holder", id);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);

                var responseString = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var response = JsonConvert.DeserializeObject<AccountHolder>(responseString.Content.ReadAsStringAsync().Result);

                return response;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get accountholder.");
                throw;
            }
        }

        /// <summary>
        /// Retrieves the privacy-restricted information for a single accountholder if an id is provided,
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
        /// <param name="criteria">a query keyword such as a Person ID or a first and last name.  A middle name is optional.</param>
        /// <returns>An enumeration of <see cref="AccountHolder">AccountHolder</see> information</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        [Obsolete("Obsolete as of API version 1.16. Use QueryAccountHoldersByPost3Async.")]
        public async Task<IEnumerable<AccountHolder>> QueryAccountHoldersByPostAsync2(string criteria)
        {
            if (string.IsNullOrEmpty(criteria))
            {
                throw new ArgumentNullException("criteria", "criteria is required for accountholder retrieval.");
            }            
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_qapiPath, _receivablesPath, "account-holder");

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);

                var responseString = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<AccountHolder>>(await responseString.Content.ReadAsStringAsync());
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get accountholders.");
                throw;
            }
        }

        /// <summary>
        /// Retrieves the privacy-restricted information for a single accountholder if an id is provided,
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
        /// <param name="criteria">either a list of person ids or a query keyword such as a Person ID 
        /// or a first and last name.  A middle name is optional.</param>
        /// <returns>An enumeration of <see cref="AccountHolder">AccountHolder</see> information</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<AccountHolder>> QueryAccountHoldersByPost3Async(AccountHolderQueryCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria", "criteria is required for accountholder retrieval.");
            }
            if ((criteria.Ids == null || !criteria.Ids.Any())
                && string.IsNullOrEmpty(criteria.QueryKeyword))
            {
                throw new ArgumentException("either a list of person ids or query keyword must be present for account holder retrieval");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_qapiPath, _receivablesPath, "account-holder");

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion3);

                var responseString = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<AccountHolder>>(await responseString.Content.ReadAsStringAsync());
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get accountholders.");
                throw;
            }
        }

        /// <summary>
        /// Get all defined receivable types
        /// </summary>
        /// <returns>The collection of ReceivableType DTOs</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<ReceivableType> GetReceivableTypes()
        {
            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

                var responseString = ExecuteGetRequestWithResponse(UrlUtility.CombineUrlPath(_receivablesPath, "receivable-types"), headers: headers);
                var response = JsonConvert.DeserializeObject<IEnumerable<ReceivableType>>(responseString.Content.ReadAsStringAsync().Result);

                return response;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get account types.");
                throw;
            }
        }

        /// <summary>
        /// Get a group of invoices by their IDs
        /// </summary>
        /// <param name="ids">Invoice IDs</param>
        /// <returns>Collection of Invoice DTOs</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<Invoice> GetInvoices(IEnumerable<string> ids)
        {
            if (ids == null || ids.Count() == 0)
            {
                throw new ArgumentNullException("ids", "IDs are required for invoice retrieval.");
            }

            try
            {
                string query = UrlUtility.BuildEncodedQueryString("invoiceIds", String.Join(",", ids));
                string urlPath = UrlUtility.CombineUrlPathAndArguments(UrlUtility.CombineUrlPath(_receivablesPath, "invoices"), query);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

                var responseString = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var response = JsonConvert.DeserializeObject<IEnumerable<Invoice>>(responseString.Content.ReadAsStringAsync().Result);

                return response;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get set of invoices.");
                throw;
            }
        }

        /// <summary>
        /// Get invoice using the given invoice ids. 
        /// </summary>
        /// <returns>Returns a list of <see cref="Invoicet">invoice objects</see>"/></returns>
        /// <exception cref="ArgumentNullException">The resource invoice Ids must be provided.</exception>
        public async Task<IEnumerable<Invoice>> QueryInvoicesAsync(InvoiceQueryCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria", "Invoice query criteria cannot be null.");
            }
            try
            {
                // Build url path from qapi path and receivable invoices path
                string[] pathStrings = new string[] { _qapiPath, _receivableInvoicesPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);
                // Add version header
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

                // Use URL path and request data to call web api method (including query string)
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);

                return JsonConvert.DeserializeObject<IEnumerable<Invoice>>(await response.Content.ReadAsStringAsync());
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get requested Invoice objects");
                throw;
            }
        }

        /// <summary>
        /// Get invoice payments using the given invoice ids. (Invoice payments are invoices with the addition of amount paid)
        /// </summary>
        /// <returns>Returns a list of <see cref="InvoicePayment">invoice payment objects</see>"/></returns>
        /// <exception cref="ArgumentNullException">The resource invoice Ids must be provided.</exception>
        public async Task<IEnumerable<InvoicePayment>> QueryInvoicePaymentsAsync(InvoiceQueryCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria", "Invoice query criteria cannot be null.");
            }
            try
            {
                // Build url path from qapi path and receivable invoices path
                string[] pathStrings = new string[] { _qapiPath, _receivableInvoicesPath };
                var urlPath = UrlUtility.CombineUrlPath(pathStrings);
                // Add version header
                var headers = new NameValueCollection();
                // Add special media header for this type of invoice.
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderInvoicePaymentVersion1);
                // Use URL path and request data to call web api method (including query string)
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);

                return JsonConvert.DeserializeObject<IEnumerable<InvoicePayment>>(await response.Content.ReadAsStringAsync());
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get requested InvoicePayment objects");
                throw;
            }
        }

        /// <summary>
        /// Gets valid billing term payment plan information from a proposed billing term payment plan information collection
        /// </summary>
        /// <param name="billingTerms">List of billing terms</param>
        /// <returns>Valid billing term payment plan information from a proposed billing term payment plan information collection</returns>
        /// <exception cref="ArgumentNullException">A collection of payment items must be provided.</exception>
        [Obsolete("Obsolete as of API version 1.16. Use QueryAccountHolderPaymentPlanOptions2Async.")]
        public async Task<PaymentPlanEligibility> QueryAccountHolderPaymentPlanOptionsAsync(IEnumerable<BillingTermPaymentPlanInformation> billingTerms)
        {
            if (billingTerms == null || !billingTerms.Any())
            {
                throw new ArgumentNullException("billingTerms", "At least one billing term payment plan information object must be supplied.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_qapiPath, _receivablesPath, "account-holder", "payment-plan-options");

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

                var responseString = await ExecutePostRequestWithResponseAsync(billingTerms, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<PaymentPlanEligibility>(await responseString.Content.ReadAsStringAsync());
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get billing term payment plan information.");
                throw;
            }
        }

        /// <summary>
        /// Gets valid billing term payment plan information from a proposed billing term payment plan information collection
        /// </summary>
        /// <param name="criteria">Payment plan query criteria</param>
        /// <returns>Valid billing term payment plan information from a proposed billing term payment plan information collection</returns>
        /// <exception cref="ArgumentNullException">criteria must be provided</exception>
        /// <exception cref="ArgumentException">A collection of payment items must be provided.</exception>
        public async Task<PaymentPlanEligibility> QueryAccountHolderPaymentPlanOptions2Async(PaymentPlanQueryCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria cannot be null");
            }
            if (criteria.BillingTerms == null || !criteria.BillingTerms.Any())
            {
                throw new ArgumentException("billingTerms", "At least one billing term payment plan information object must be supplied.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_qapiPath, _receivablesPath, "account-holder", "payment-plan-options");

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);

                var responseString = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<PaymentPlanEligibility>(await responseString.Content.ReadAsStringAsync());
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get billing term payment plan information.");
                throw;
            }
        }

        /// <summary>
        /// Get all defined charge codes
        /// </summary>
        /// <returns>The collection of ChargeCode DTOs</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<ChargeCode>> GetChargeCodesAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_receivablesPath, "charge-codes");

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

                var responseString = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var response = JsonConvert.DeserializeObject<IEnumerable<ChargeCode>>(responseString.Content.ReadAsStringAsync().Result);

                return response;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get charge codes.");
                throw;
            }
        }

        #endregion

        #region Configuration

        /// <summary>
        /// Get Student Finance configuration
        /// </summary>
        /// <returns>FinanceConfiguration DTO</returns>
        public FinanceConfiguration GetConfiguration()
        {
            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

            var responseString = ExecuteGetRequestWithResponse(_configurationPath, headers: headers);
            var configuration = JsonConvert.DeserializeObject<FinanceConfiguration>(responseString.Content.ReadAsStringAsync().Result);

            return configuration;
        }

        /// <summary>
        /// Get Immediate Payment Control configuration
        /// </summary>
        /// <returns>ImmediatePaymentControl DTO</returns>
        public ImmediatePaymentControl GetImmediatePaymentControl()
        {
            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

            var responseString = ExecuteGetRequestWithResponse(_configurationPath + "/ipc", headers: headers);
            var response = JsonConvert.DeserializeObject<ImmediatePaymentControl>(responseString.Content.ReadAsStringAsync().Result);

            return response;
        }
        #endregion

        #region Deposits

        /// <summary>
        /// Get all defined deposit types
        /// </summary>
        /// <returns>The collection of DepositType DTOs</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<DepositType> GetDepositTypes()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_depositsPath, "deposit-types");

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

                var responseString = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var response = JsonConvert.DeserializeObject<IEnumerable<DepositType>>(responseString.Content.ReadAsStringAsync().Result);

                return response;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get deposit types.");
                throw;
            }
        }

        #endregion

        #region Payments

        /// <summary>
        /// Process a student payment through the payment gateway
        /// </summary>
        /// <param name="paymentDetails">Payment details</param>
        /// <returns>A PaymentProvider DTO</returns>
        public PaymentProvider ProcessStudentPayment(Payment paymentDetails)
        {
            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

            var responseString = ExecutePostRequestWithResponse(paymentDetails, _processStudentPaymentPath, headers: headers);

            var paymentRedirect = JsonConvert.DeserializeObject<PaymentProvider>(responseString.Content.ReadAsStringAsync().Result);

            return paymentRedirect;
        }

        /// <summary>
        /// Process an electronic check
        /// </summary>
        /// <param name="paymentDetails">Payment details</param>
        /// <returns>An ElectronicCheckProcessingResult</returns>
        public ElectronicCheckProcessingResult ProcessElectronicCheck(Payment paymentDetails)
        {
            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

            var responseString = ExecutePostRequestWithResponse(paymentDetails, _electronicCheckPaymentPath, headers: headers);

            var processingResult = JsonConvert.DeserializeObject<ElectronicCheckProcessingResult>(responseString.Content.ReadAsStringAsync().Result);

            return processingResult;
        }

        /// <summary>
        /// Get the payer information for an electronic check.
        /// </summary>
        /// <param name="personId">Payer ID</param>
        /// <returns>An ElectronicCheckPayer DTO</returns>
        public ElectronicCheckPayer GetCheckPayerInformation(string personId)
        {
            var baseUrl = UrlUtility.CombineUrlPath(_electronicCheckPayerPath, personId);

            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

            var responseString = ExecuteGetRequestWithResponse(baseUrl, headers: headers);

            var paymentRedirect = JsonConvert.DeserializeObject<ElectronicCheckPayer>(responseString.Content.ReadAsStringAsync().Result);

            return paymentRedirect;
        }

        /// <summary>
        /// Verify that a student payment can be processed
        /// </summary>
        /// <param name="selectedDistribution">Distribution code</param>
        /// <param name="paymentMethodSelection">Payment method selected</param>
        /// <param name="totalAmountToPay">Total payment amount</param>
        /// <returns>A PaymentConfirmation DTO</returns>
        public PaymentConfirmation ConfirmStudentPayment(string selectedDistribution, string paymentMethodSelection, string totalAmountToPay)
        {
            var queryString = UrlUtility.BuildEncodedQueryString("Distribution", selectedDistribution, "PaymentMethod", paymentMethodSelection, "AmountToPay", totalAmountToPay);

            var combinedUrl = UrlUtility.CombineUrlPathAndArguments(_confirmStudentPaymentPath, queryString);

            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

            var responseString = ExecuteGetRequestWithResponse(combinedUrl, headers: headers);

            var confirmation = JsonConvert.DeserializeObject<PaymentConfirmation>(responseString.Content.ReadAsStringAsync().Result);

            return confirmation;
        }

        /// <summary>
        /// Get the detail for a cash receipt
        /// </summary>
        /// <param name="transactionId">E-Commerce transaction ID</param>
        /// <param name="cashRcptsId">Cash Receipt ID</param>
        /// <returns>A PaymentReceipt DTO</returns>
        public Ellucian.Colleague.Dtos.Finance.Payments.PaymentReceipt GetCashReceipt(string transactionId, string cashRcptsId)
        {
            var queryString = UrlUtility.BuildEncodedQueryString("transactionId", transactionId, "cashReceiptId", cashRcptsId);

            var combinedUrl = UrlUtility.CombineUrlPathAndArguments(_cashReceiptPath, queryString);

            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

            var responseString = ExecuteGetRequestWithResponse(combinedUrl, headers: headers);

            var acknowledgment = JsonConvert.DeserializeObject<Ellucian.Colleague.Dtos.Finance.Payments.PaymentReceipt>(responseString.Content.ReadAsStringAsync().Result);

            return acknowledgment;
        }

        /// <summary>
        /// Get the payment distributions for a student, account types, and payment process
        /// </summary>
        /// <param name="studentId">Student ID</param>
        /// <param name="accountTypes">List of account type codes</param>
        /// <param name="paymentProcess">Payment process code</param>
        /// <returns>List of payment distributions</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<string> GetPaymentDistributions(string studentId, IEnumerable<string> accountTypes, string paymentProcess)
        {
            if (String.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "Student ID is required.");
            }

            try
            {
                var query = UrlUtility.BuildEncodedQueryString("paymentProcess", paymentProcess, "accountTypes", String.Join(",", accountTypes));
                var urlPath = UrlUtility.CombineUrlPathAndArguments(UrlUtility.CombineUrlPath(_distributionsPath, studentId), query);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

                var responseString = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var response = JsonConvert.DeserializeObject<IEnumerable<string>>(responseString.Content.ReadAsStringAsync().Result);

                return response;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get payment distributions.");
                throw;
            }
        }

        #endregion

        #region PaymentPlans

        /// <summary>
        /// Gets all payment plan templates
        /// </summary>
        /// <returns>A list of PaymentPlanTemplate DTOs</returns>
        public IEnumerable<PaymentPlanTemplate> GetPaymentPlanTemplates()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_paymentPlansPath, "templates");

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

                var responseString = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var response = JsonConvert.DeserializeObject<IEnumerable<PaymentPlanTemplate>>(responseString.Content.ReadAsStringAsync().Result);

                return response;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get payment plan templates.");
                throw;
            }
        }

        /// <summary>
        /// Gets the specified payment plan template
        /// </summary>
        /// <param name="templateId">ID of the payment plan template</param>
        /// <returns>A PaymentPlanTemplate DTO</returns>
        /// <exception cref="ArgumentNullException">The resource templateId must be provided.</exception>
        public PaymentPlanTemplate GetPaymentPlanTemplate(string templateId)
        {
            if (String.IsNullOrEmpty(templateId))
            {
                throw new ArgumentNullException("templateId", "Template ID is required for payment plan template retrieval.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_paymentPlansPath, "templates", UrlParameterUtility.EncodeWithSubstitution(templateId));

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

                var responseString = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var response = JsonConvert.DeserializeObject<PaymentPlanTemplate>(responseString.Content.ReadAsStringAsync().Result);

                return response;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get payment plan template.");
                throw;
            }
        }

        /// <summary>
        /// Get a payment plan by ID
        /// </summary>
        /// <param name="paymentPlanId">Payment Plan ID</param>
        /// <returns>The PaymentPlan DTO</returns>
        /// <exception cref="ArgumentNullException">The resource planId must be provided.</exception>
        public PaymentPlan GetPaymentPlan(string paymentPlanId)
        {
            if (String.IsNullOrEmpty(paymentPlanId))
            {
                throw new ArgumentNullException("paymentPlanId", "Plan ID is required for payment plan retrieval.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_paymentPlansPath, paymentPlanId);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

                var responseString = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var response = JsonConvert.DeserializeObject<PaymentPlan>(responseString.Content.ReadAsStringAsync().Result);

                return response;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get payment plan.");
                throw;
            }
        }

        /// <summary>
        /// Gets the specified payment plan approval
        /// </summary>
        /// <param name="approvalId">ID of the payment plan approval</param>
        /// <returns>A PaymentPlanApproval DTO</returns>
        /// <exception cref="ArgumentNullException">The resource templateId must be provided.</exception>
        public PaymentPlanApproval GetPaymentPlanApproval(string approvalId)
        {
            if (String.IsNullOrEmpty(approvalId))
            {
                throw new ArgumentNullException("approvalId", "Approval ID is required for payment plan approval retrieval.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_paymentPlansPath, "approvals", approvalId);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

                var responseString = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var response = JsonConvert.DeserializeObject<PaymentPlanApproval>(responseString.Content.ReadAsStringAsync().Result);

                return response;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get payment plan approval.");
                throw;
            }
        }

        /// <summary>
        /// Accept the terms and conditions associated with a payment plan
        /// </summary>
        /// <param name="acceptance">The information for the acceptance</param>
        /// <returns>The updated approval information</returns>
        /// <exception cref="ArgumentNullException">The resource acceptance must be provided.</exception>
        public PaymentPlanApproval AcceptPaymentPlanTerms(PaymentPlanTermsAcceptance acceptance)
        {
            if (acceptance == null)
            {
                throw new ArgumentNullException("acceptance", "Payment plan terms acceptance object cannot be null.");
            }

            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

                string urlPath = UrlUtility.CombineUrlPath(_paymentPlansPath, "accept-terms");
                var responseString = ExecutePostRequestWithResponse(acceptance, urlPath, headers: headers);

                var response = JsonConvert.DeserializeObject<PaymentPlanApproval>(responseString.Content.ReadAsStringAsync().Result);

                return response;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to accept payment plan terms.");
                throw;
            }
        }

        /// <summary>
        /// Create a payment plan
        /// </summary>
        /// <param name="paymentPlan">Proposed PaymentPlan Dto</param>
        /// <returns>Created PaymentPlan Dto</returns>
        /// <exception cref="ArgumentNullException">The resource paymentPlan must be provided.</exception>
        public PaymentPlan CreatePaymentPlan(PaymentPlan paymentPlan)
        {
            if (paymentPlan == null)
            {
                throw new ArgumentNullException("paymentPlan", "Payment plan object cannot be null.");
            }

            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

                var responseString = ExecutePostRequestWithResponse<PaymentPlan>(paymentPlan, _paymentPlansPath, headers: headers);

                var response = JsonConvert.DeserializeObject<PaymentPlan>(responseString.Content.ReadAsStringAsync().Result);

                return response;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to create payment plan.");
                throw;
            }
        }


        /// <summary>
        /// Gets the down payment information for a payment control, payment plan, pay method and amount
        /// </summary>
        /// <param name="payControlId">Registration payment control ID</param>
        /// <param name="planId">Payment plan ID</param>
        /// <param name="payMethod">Payment method code</param>
        /// <param name="amount">Total payment amount</param>
        /// <returns>List of payments to be made</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public Payment GetPlanPaymentSummary(string planId, string payMethod, decimal amount, string payControlId)
        {
            if (string.IsNullOrEmpty(planId))
            {
                throw new ArgumentNullException("planId", "A payment plan ID must be provided.");
            }
            if (string.IsNullOrEmpty(payMethod))
            {
                throw new ArgumentNullException("payMethod", "A payment method must be provided.");
            }
            if (amount <= 0)
            {
                throw new ArgumentException("Amount must be a positive number.", "amount");
            }

            try
            {
                string query = UrlUtility.BuildEncodedQueryString("payMethod", payMethod, "amount", amount.ToString(), "payControlId", payControlId);
                string urlPath = UrlUtility.CombineUrlPathAndArguments(UrlUtility.CombineUrlPath(_paymentPlansPath, UrlParameterUtility.EncodeWithSubstitution(planId), "payment-summary"), query);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

                var responseString = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var response = JsonConvert.DeserializeObject<Payment>(responseString.Content.ReadAsStringAsync().Result);

                return response;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get plan payment summary.");
                throw;
            }
        }

        /// <summary>
        /// Gets a proposed payment plan for a given person for a given term and receivable type with total charges
        /// no greater than the stated amount
        /// </summary>
        /// <param name="personId">Proposed plan owner ID</param>
        /// <param name="termId">Billing term ID</param>
        /// <param name="receivableTypeCode">Receivable Type Code</param>
        /// <param name="planAmount">Maximum total payment plan charges</param>
        /// <returns>Proposed payment plan</returns>
        public async Task<PaymentPlan> GetProposedPaymentPlanAsync(string personId, string termId,
            string receivableTypeCode, decimal planAmount)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "A person ID must be supplied.");
            }
            if (string.IsNullOrEmpty(termId))
            {
                throw new ArgumentNullException("termId", "A term ID must be supplied.");
            }
            if (string.IsNullOrEmpty(receivableTypeCode))
            {
                throw new ArgumentNullException("receivableTypeCode", "A receivable type code must be supplied.");
            }
            if (planAmount <= 0)
            {
                throw new ArgumentException("planAmount", "A positive plan amount must be supplied.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_paymentPlansProposedPath, personId);
                var queryString = UrlUtility.BuildEncodedQueryString("termId", termId,
                            "receivableTypeCode", receivableTypeCode, "planAmount", planAmount.ToString());
                var combinedUrl = UrlUtility.CombineUrlPathAndArguments(urlPath, queryString);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

                var responseString = await ExecuteGetRequestWithResponseAsync(combinedUrl, headers: headers);
                return JsonConvert.DeserializeObject<PaymentPlan>(await responseString.Content.ReadAsStringAsync());
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                throw;
            }
        }

        #endregion

        #region Registration Billing

        /// <summary>
        /// Get a registration payment control
        /// </summary>
        /// <param name="id">Control ID</param>
        /// <returns>The RegistrationPaymentControl DTO</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public RegistrationPaymentControl GetRegistrationPaymentControl(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID is required for registration payment control retrieval.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_paymentControlsPath, id);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

                var responseString = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var response = JsonConvert.DeserializeObject<RegistrationPaymentControl>(responseString.Content.ReadAsStringAsync().Result);

                return response;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get registration payment control.");
                throw;
            }
        }

        /// <summary>
        /// Get all the payment controls for a student
        /// </summary>
        /// <param name="studentId">Student ID</param>
        /// <returns>A list of RegistrationPaymentControl DTOs</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<RegistrationPaymentControl> GetStudentPaymentControls(string studentId)
        {
            if (String.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "Student ID is required for payment control retrieval.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_paymentControlsPath, "student", studentId);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

                var responseString = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var response = JsonConvert.DeserializeObject<IEnumerable<RegistrationPaymentControl>>(responseString.Content.ReadAsStringAsync().Result);

                return response;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get registration payment control.");
                throw;
            }
        }

        /// <summary>
        /// Get a registration payment control document
        /// </summary>
        /// <param name="id">Control ID</param>
        /// <param name="documentId">Document ID</param>
        /// <returns>The TextDocument DTO</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public TextDocument GetRegistrationPaymentControlDocument(string id, string documentId)
        {
            if (String.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Payment control ID is required for document retrieval.");
            }
            if (String.IsNullOrEmpty(documentId))
            {
                throw new ArgumentNullException("documentId", "Document ID is required for document retrieval.");
            }

            try
            {
                // Document ID could have characters that would cause problems, so it goes in the query string
                string query = UrlUtility.BuildEncodedQueryString("documentId", documentId);
                string urlPath = UrlUtility.CombineUrlPathAndArguments(UrlUtility.CombineUrlPath(_paymentControlsPath, id), query);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

                var responseString = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var response = JsonConvert.DeserializeObject<TextDocument>(responseString.Content.ReadAsStringAsync().Result);

                return response;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get registration payment control document.");
                throw;
            }
        }

        /// <summary>
        /// Accept the registration terms and conditions
        /// </summary>
        /// <param name="acceptance">PaymentTermsAcceptance DTO</param>
        /// <returns>Updated RegistrationApproval DTO</returns>
        /// <exception cref="ArgumentNullException">The resource must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        [Obsolete("Obsolete as of API 1.5; please use AcceptRegistrationTerms2 instead.", false)]
        public RegistrationTermsApproval AcceptRegistrationTerms(PaymentTermsAcceptance acceptance)
        {
            if (acceptance == null)
            {
                throw new ArgumentNullException("acceptance", "Payment terms acceptance object cannot be null.");
            }

            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

                string urlPath = UrlUtility.CombineUrlPath(_paymentControlsPath, "accept-terms");
                var responseString = ExecutePostRequestWithResponse(acceptance, urlPath, headers: headers);

                var response = JsonConvert.DeserializeObject<RegistrationTermsApproval>(responseString.Content.ReadAsStringAsync().Result);

                return response;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to accept registration terms.");
                throw;
            }
        }

        /// <summary>
        /// Accept the registration terms and conditions
        /// </summary>
        /// <param name="acceptance">PaymentTermsAcceptance DTO</param>
        /// <returns>Updated RegistrationApproval DTO</returns>
        /// <exception cref="ArgumentNullException">The resource must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public RegistrationTermsApproval2 AcceptRegistrationTerms2(PaymentTermsAcceptance2 acceptance)
        {
            if (acceptance == null)
            {
                throw new ArgumentNullException("acceptance", "Payment terms acceptance object cannot be null.");
            }

            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);

                string urlPath = UrlUtility.CombineUrlPath(_paymentControlsPath, "accept-terms");
                var responseString = ExecutePostRequestWithResponse(acceptance, urlPath, headers: headers);

                var response = JsonConvert.DeserializeObject<RegistrationTermsApproval2>(responseString.Content.ReadAsStringAsync().Result);

                return response;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to accept registration terms.");
                throw;
            }
        }

        /// <summary>
        /// Get registration payment options for a student for a term
        /// </summary>
        /// <param name="id">Control ID</param>
        /// <returns>The ImmediatePaymentOptions DTO</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public ImmediatePaymentOptions GetRegistrationPaymentOptions(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID is required for registration payment option retrieval.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_paymentControlsPath, "options", id);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

                var responseString = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var response = JsonConvert.DeserializeObject<ImmediatePaymentOptions>(responseString.Content.ReadAsStringAsync().Result);

                return response;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get registration payment options.");
                throw;
            }
        }

        /// <summary>
        /// Update a registration payment control
        /// </summary>
        /// <param name="rpcDto">The payment control to update</param>
        /// <returns>The updated payment control</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public RegistrationPaymentControl UpdateRegistrationPaymentControl(RegistrationPaymentControl rpcDto)
        {
            if (rpcDto == null)
            {
                throw new ArgumentNullException("rpcDto", "Registration Payment Control is required for update.");
            }

            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

            try
            {
                var responseString = ExecutePutRequestWithResponse(rpcDto, _paymentControlsPath, headers: headers);
                var response = JsonConvert.DeserializeObject<RegistrationPaymentControl>(responseString.Content.ReadAsStringAsync().Result);

                return response;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to update registration payment control.");
                throw;
            }
        }

        /// <summary>
        /// Get the summary information for a registration payment
        /// </summary>
        /// <param name="id">Payment control ID</param>
        /// <param name="payMethod">Payment method code</param>
        /// <param name="amount">Total amount to pay</param>
        /// <returns>List of payments to be made</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<Payment> GetRegistrationPaymentSummary(string id, string payMethod, decimal amount)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Payment Control ID is required for payment summary.");
            }

            if (string.IsNullOrEmpty(payMethod))
            {
                throw new ArgumentNullException("payMethod", "Payment Method is required for payment summary.");
            }

            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException("amount", "Payment amount must be greater than zero.");
            }

            try
            {
                string query = UrlUtility.BuildEncodedQueryString("payMethod", payMethod, "amount", amount.ToString());
                string urlPath = UrlUtility.CombineUrlPathAndArguments(UrlUtility.CombineUrlPath(_paymentControlsPath, "summary", id), query);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

                var responseString = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var response = JsonConvert.DeserializeObject<IEnumerable<Payment>>(responseString.Content.ReadAsStringAsync().Result);

                return response;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get payment summary.");
                throw;
            }

        }

        /// <summary>
        /// Start the registration payment process
        /// </summary>
        /// <param name="paymentDto">Payment information</param>
        /// <returns>Payment provider information</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public PaymentProvider StartRegistrationPayment(Payment paymentDto)
        {
            if (paymentDto == null)
            {
                throw new ArgumentNullException("paymentDto", "Payment is required for starting a registration payment.");
            }

            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

                string urlPath = UrlUtility.CombineUrlPath(_paymentControlsPath, "start-payment");
                var responseString = ExecutePostRequestWithResponse(paymentDto, urlPath, headers: headers);

                var response = JsonConvert.DeserializeObject<PaymentProvider>(responseString.Content.ReadAsStringAsync().Result);

                return response; 
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to start the registration payment.");
                throw;
            }
        }

        /// <summary>
        /// Get a registration terms approval
        /// </summary>
        /// <param name="id">Approval ID</param>
        /// <returns>The RegistrationTermsApproval DTO</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        [Obsolete("Obsolete as of API 1.5; please use GetRegistrationTermsApproval instead.", false)]
        public RegistrationTermsApproval GetRegistrationTermsApproval(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID is required for registration terms approval retrieval.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_paymentControlsPath, "terms-approval", id);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

                var responseString = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var response = JsonConvert.DeserializeObject<RegistrationTermsApproval>(responseString.Content.ReadAsStringAsync().Result);

                return response;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get registration terms approval.");
                throw;
            }
        }

        /// <summary>
        /// Get a registration terms approval
        /// </summary>
        /// <param name="id">Approval ID</param>
        /// <returns>The RegistrationTermsApproval2 DTO</returns>
        /// <exception cref="ArgumentNullException">The resource id must be provided.</exception>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public RegistrationTermsApproval2 GetRegistrationTermsApproval2(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID is required for registration terms approval retrieval.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_paymentControlsPath, "terms-approval", id);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);

                var responseString = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var response = JsonConvert.DeserializeObject<RegistrationTermsApproval2>(responseString.Content.ReadAsStringAsync().Result);

                return response;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get registration terms approval.");
                throw;
            }
        }

        /// <summary>
        /// Get a proposed payment plan
        /// </summary>
        /// <param name="payControlId">ID of a payment control record</param>
        /// <param name="receivableType">Receivable Type for proposed payment plan</param>
        /// <returns>The PaymentPlan DTO</returns>
        /// <exception cref="ArgumentNullException">The resource payControlId must be provided.</exception>
        public PaymentPlan GetPaymentControlProposedPaymentPlan(string payControlId, string receivableType)
        {
            if (String.IsNullOrEmpty(payControlId))
            {
                throw new ArgumentNullException("payControlId", "Payment Control ID is required for proposed payment plan retrieval.");
            }
            if (String.IsNullOrEmpty(receivableType))
            {
                throw new ArgumentNullException("receivableType", "Receivable Type is required for proposed payment plan retrieval.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_paymentControlsPath, "proposed-plan", payControlId, UrlParameterUtility.EncodeWithSubstitution(receivableType));
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

                var responseString = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var response = JsonConvert.DeserializeObject<PaymentPlan>(responseString.Content.ReadAsStringAsync().Result);

                return response;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get proposed payment plan.");
                throw;
            }
        }

        #endregion

        #region Statements

        /// <summary>
        /// Get an student's accounts receivable report for the given timeframe
        /// </summary>
        /// <param name="accountHolderId">ID of the student for whom the statement will be generated</param>
        /// <param name="timeframeId">ID of the timeframe for which the statement will be generated</param>
        /// <param name="startDate">Date on which the supplied timeframe starts</param>
        /// <param name="endDate">Date on which the supplied timeframe ends</param>
        /// <param name="fileName">The name of the PDF file returned in the byte array.</param>
        /// <returns>A byte array representation of the PDF Award Letter Report</returns>
        /// <exception cref="Exception">Thrown if an error occurred generating the student statement report.</exception>
        public byte[] GetStudentStatement(string accountHolderId, string timeframeId, DateTime? startDate, DateTime? endDate, out string fileName)
        {
            if (string.IsNullOrEmpty(accountHolderId))
            {
                throw new ArgumentNullException("accountHolderId", "Account Holder ID cannot be null or empty.");
            }
            if (string.IsNullOrEmpty(timeframeId))
            {
                throw new ArgumentNullException("timeframeId", "Term/Period ID cannot be null or empty.");
            }

            try
            {
                // Build url path from qapi path and student statements path
                var baseUrl = UrlUtility.CombineUrlPath(_accountHoldersPath, accountHolderId, _statementsPath, UrlParameterUtility.EncodeWithSubstitution(timeframeId));
                var queryString = string.Empty;
                if (startDate != null)
                {
                    if (endDate != null)
                    {
                        queryString = UrlUtility.BuildEncodedQueryString("startDate", startDate.Value.ToShortDateString(),
                            "endDate", endDate.Value.ToShortDateString());
                    }
                    else
                    {
                        queryString = UrlUtility.BuildEncodedQueryString("startDate", startDate.Value.ToShortDateString());
                    }
                }
                else
                {
                    if (endDate != null)
                    {
                        queryString = UrlUtility.BuildEncodedQueryString("endDate", endDate.Value.ToShortDateString());
                    }
                }

                var combinedUrl = UrlUtility.CombineUrlPathAndArguments(baseUrl, queryString);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                headers.Add(AcceptHeaderKey, "application/pdf");
                headers.Add(AcceptHeaderKey, "application/vnd.ellucian.v1+pdf");
                headers.Add("X-Ellucian-Media-Type", "application/vnd.ellucian.v1+pdf");
                var response = ExecuteGetRequestWithResponse(combinedUrl, headers: headers);
                fileName = response.Content.Headers.ContentDisposition.FileName;
                var resource = response.Content.ReadAsByteArrayAsync().Result;
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex.GetBaseException(), "Unable to retrieve student statement.");
                throw;
            }
        }
        #endregion
    }
}
