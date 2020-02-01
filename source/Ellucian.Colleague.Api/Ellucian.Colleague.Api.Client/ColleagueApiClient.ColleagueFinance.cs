﻿// Copyright 2014-2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Rest.Client.Exceptions;
using Ellucian.Web.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Client
{
    public partial class ColleagueApiClient
    {
        #region Budget Adjustments

        /// <summary>
        /// Get configuration information necessary to validate a budget adjustment.
        /// </summary>
        /// <returns>Budget Adjustment configuration parameters.</returns>
        /// <exception cref="Exception">Unable to get the budget adjustment configuration.</exception>
        public async Task<BudgetAdjustmentConfiguration> GetBudgetAdjustmentConfigurationAsync()
        {
            try
            {
                // Create and execute a request to get the budget adjustment configuration.
                string urlPath = UrlUtility.CombineUrlPath(_budgetAdjustmentConfigurationPath);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);

                var resource = JsonConvert.DeserializeObject<BudgetAdjustmentConfiguration>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to retrieve budget adjustment configuration.");
                throw;
            }
        }

        /// <summary>
        /// Get indicator that determines whether budget adjustments are turned on or off.
        /// </summary>
        /// <returns>BudgetAdjustmentEnabled DTO</returns>
        /// <exception cref="Exception">Unable to get the budget adjustment enabled configuration.</exception>
        public async Task<BudgetAdjustmentsEnabled> GetBudgetAdjustmentsEnabledAsync()
        {
            try
            {
                // Create and execute a request to get the budget adjustment enabled configuration.
                string urlPath = UrlUtility.CombineUrlPath(_budgetAdjustmentEnabledPath);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);

                var resource = JsonConvert.DeserializeObject<BudgetAdjustmentsEnabled>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get the budget adjustment enabled configuration.");
                throw;
            }
        }

        /// <summary>
        /// Create a draft budget adjustment.
        /// </summary>
        /// <param name="draftBudgetAdjustment">Draft Budget Adjustment DTO</param>
        /// <returns>Draft Budget Adjustment DTO</returns>
        public async Task<DraftBudgetAdjustment> CreateDraftBudgetAdjustmentAsync(DraftBudgetAdjustment draftBudgetAdjustment)
        {
            try
            {
                // Create and execute a request to create a new 
                string[] pathStrings = new string[] { _draftBudgetAdjustmentsPath };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePostRequestWithResponseAsync(draftBudgetAdjustment, urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<DraftBudgetAdjustment>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw;
            }
        }

        /// <summary>
        /// Update a draft budget adjustment.
        /// </summary>
        /// <param name="draftBudgetAdjustment">Draft Budget Adjustment DTO</param>
        /// <returns>Draft Budget Adjustment DTO</returns>
        public async Task<DraftBudgetAdjustment> UpdateDraftBudgetAdjustmentAsync(DraftBudgetAdjustment draftBudgetAdjustment)
        {
            try
            {
                // Create and execute a request to create a new 
                string[] pathStrings = new string[] { _draftBudgetAdjustmentsPath, draftBudgetAdjustment.Id };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePutRequestWithResponseAsync(draftBudgetAdjustment, urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<DraftBudgetAdjustment>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw;
            }
        }

        /// <summary>
        /// Get a draft budget Adjustment.
        /// </summary>
        /// <param name="id">A draft budget adjustment ID.</param>
        /// <returns>Draft Budget Adjustment DTO.</returns>
        public async Task<DraftBudgetAdjustment> GetDraftBudgetAdjustmentAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_draftBudgetAdjustmentsPath, id);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);

                var resource = JsonConvert.DeserializeObject<DraftBudgetAdjustment>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException ex)
            {
                logger.Error(ex, "Unable to get draft budget adjustment record {0}.", id);
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get the draft budget adjustment record.");
                throw;
            }
        }

        /// <summary>
        /// Delete a draft budget Adjustment.
        /// </summary>
        /// <param name="id">A draft budget adjustment ID.</param>
        /// <returns></returns>
        public async Task DeleteDraftBudgetAdjustmentAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_draftBudgetAdjustmentsPath, id);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteDeleteRequestWithResponseAsync(urlPath, headers: headers);
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to delete draft budget adjustment record.");
                throw;
            }
        }

        /// <summary>
        /// Create a budget adjustment.
        /// </summary>
        /// <param name="budgetAdjustment">Budget Adjustment DTO</param>
        /// <returns>Budget Adjustment DTO</returns>
        public async Task<BudgetAdjustment> CreateBudgetAdjustmentAsync(BudgetAdjustment budgetAdjustment)
        {
            try
            {
                // Create and execute a request to create a new 
                string[] pathStrings = new string[] { _budgetAdjustmentsPath };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePostRequestWithResponseAsync(budgetAdjustment, urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<BudgetAdjustment>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw;
            }
        }

        /// <summary>
        /// Update an existing budget adjustment.
        /// </summary>
        /// <param name="budgetAdjustment">Budget Adjustment DTO</param>
        /// <returns>Budget Adjustment DTO</returns>
        public async Task<BudgetAdjustment> UpdateBudgetAdjustmentAsync(BudgetAdjustment budgetAdjustment)
        {
            try
            {
                // Create and execute a request to update a budget adjustment
                string[] pathStrings = new string[] { _budgetAdjustmentsPath, budgetAdjustment.Id };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePutRequestWithResponseAsync(budgetAdjustment, urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<BudgetAdjustment>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw;
            }
        }

        /// <summary>
        /// Get a budget Adjustment.
        /// </summary>
        /// <param name="id">A budget adjustment ID.</param>
        /// <returns>Budget Adjustment DTO.</returns>
        public async Task<BudgetAdjustment> GetBudgetAdjustmentAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_budgetAdjustmentsPath, id);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);

                var resource = JsonConvert.DeserializeObject<BudgetAdjustment>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException ex)
            {
                logger.Error(ex, "Unable to get the budget adjustment record {0}.", id);
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get the budget adjustment record.");
                throw;
            }
        }

        /// <summary>
        /// Get a budget Adjustment that is pending the user's approval.
        /// </summary>
        /// <param name="id">A budget adjustment ID.</param>
        /// <returns>Budget Adjustment DTO.</returns>
        public async Task<BudgetAdjustment> GetBudgetAdjustmentPendingApprovalDetailAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_budgetAdjustmentsPendingApprovalDetailPath, id);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);

                var resource = JsonConvert.DeserializeObject<BudgetAdjustment>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException ex)
            {
                logger.Error(ex, "Unable to get the budget adjustment record {0}.", id);
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get the budget adjustment record.");
                throw;
            }
        }

        /// <summary>
        /// Approve a budget adjustment by the current user.
        /// </summary>
        /// <param name="budgetAdjustmentId">A budget adjustment ID.</param>
        /// <param name="budgetAdjustmentApproval">The budget adjustment approval DTO.</param>
        /// <returns>Budget Adjustment DTO.</returns>
        public async Task<BudgetAdjustment> PostBudgetAdjustmentApprovalAsync(string budgetAdjustmentId, BudgetAdjustmentApproval budgetAdjustmentApproval)
        {
            if (string.IsNullOrWhiteSpace(budgetAdjustmentId))
            {
                throw new ArgumentNullException("budgetAdjustmentId", "The budget adjustment ID cannot be empty/null when posting an approval.");
            }
            if (budgetAdjustmentApproval == null)
            {
                throw new ArgumentNullException("budgetAdjustmentApproval", "The budget adjustment approval cannot be empty/null when posting an approval.");
            }
            try
            {
                string[] pathStrings = new string[] { _budgetAdjustmentsPath, budgetAdjustmentId, _approvalsPath };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePostRequestWithResponseAsync(budgetAdjustmentApproval, urlPath, headers: headers);

                var resource = JsonConvert.DeserializeObject<BudgetAdjustment>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (ResourceNotFoundException exnf)
            {
                logger.Error(exnf, "Unable to approve the budget adjustment record {0}.", budgetAdjustmentId);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to approve the budget adjustment record.");
                throw;
            }
        }

        /// <summary>
        /// Get the list of budget adjustments for the current user.
        /// </summary>
        /// <returns>List of budget adjustments for the current user.</returns>
        public async Task<IEnumerable<BudgetAdjustmentSummary>> GetBudgetAdjustmentsSummaryAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_budgetAdjustmentsSummaryPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);

                var resource = JsonConvert.DeserializeObject<List<BudgetAdjustmentSummary>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get the budget adjustments for the user.");
                throw;
            }
        }

        /// <summary>
        /// Get the list of budget adjustments pending approval for the current user.
        /// </summary>
        /// <returns>List of budget adjustments pending approval summary DTOs for the current user.</returns>
        public async Task<IEnumerable<BudgetAdjustmentPendingApprovalSummary>> GetBudgetAdjustmentsPendingApprovalSummaryAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_budgetAdjustmentsPendingApprovalPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);

                var resource = JsonConvert.DeserializeObject<List<BudgetAdjustmentPendingApprovalSummary>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get the budget adjustments pending approval for the user.");
                throw;
            }
        }

        #endregion

        #region Cost Centers

        /// <summary>
        /// Gets a list of cost centers for a user.
        /// </summary>
        /// <param name="fiscalYear">The GL fiscal year.</param>
        /// <returns>A list of cost centers.</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        /// <exception cref="Exception">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<CostCenter>> GetCostCentersAsync(string fiscalYear)
        {
            try
            {
                string query = "fiscalYear=" + fiscalYear;
                string urlPath = UrlUtility.CombineUrlPath(_costCentersPath);

                urlPath = UrlUtility.CombineUrlPathAndArguments(urlPath, query);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);

                var resource = JsonConvert.DeserializeObject<List<CostCenter>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException ex)
            {
                logger.Error(ex, "Unable to get cost centers for fiscal year {0}.", fiscalYear);
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get cost centers for fiscal year {0}.", fiscalYear);
                throw;
            }
        }

        /// <summary>
        /// Get the cost center selected by the user.
        /// </summary>
        /// <param name="costCenterId">The cost center ID.</param>
        /// <param name="fiscalYear">The GL fiscal year.</param>
        /// <returns>A cost center.</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        /// <exception cref="Exception">The requested resource cannot be found.</exception>
        public async Task<CostCenter> GetCostCenterAsync(string costCenterId, string fiscalYear)
        {
            try
            {
                // Create and execute a request to get a specified cost center.
                string query = "fiscalYear=" + fiscalYear;
                string[] pathStrings = new string[] { _costCentersPath, costCenterId };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);
                urlPath = UrlUtility.CombineUrlPathAndArguments(urlPath, query);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);

                var resource = JsonConvert.DeserializeObject<CostCenter>(await response.Content.ReadAsStringAsync());
                return resource;
            }

            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException ex)
            {
                logger.Error(ex, "Unable to get the cost center {0} for fiscal year {1}.", costCenterId, fiscalYear);
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get the cost center {0} for fiscal year {1}.", costCenterId, fiscalYear);
                throw;
            }
        }

        /// <summary>
        /// Get the cost centers based on filter criteria.
        /// </summary>
        /// <param name="criteria">The <see cref="Dtos.ColleagueFinance.CostCenterComponentQueryCriteria"> criteria</see> to query by.</param>
        /// <returns>Cost centers that match the query criteria.</returns>
        public async Task<IEnumerable<CostCenter>> QueryCostCentersAsync(CostCenterQueryCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria", "Cost center query criteria cannot be null.");
            }
            try
            {
                string[] pathStrings = new string[] { _qapiPath, _costCentersPath };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);

                // Add version header
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

                // Use URL path and request data to call web api method (including query string)
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);

                var resource = JsonConvert.DeserializeObject<List<CostCenter>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException ex)
            {
                logger.Error(ex, "Unable to get filtered cost centers.");
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get filtered cost centers.");
                throw;
            }
        }

        #endregion

        #region General Ledger Account

        /// <summary>
        /// Get the user GL accounts.
        /// If there is a fiscal year, it will also validate it for that year.
        /// </summary>
        /// <param name="glClass">Optional: null for all the user GL accounts, expense for only the expense type GL accounts.</param>
        /// <returns>Success: List of GL accounts fpr the user.</returns>
        public async Task<IEnumerable<GlAccount>> GetUserGeneralLedgerAccountsAsync(string glClass = null)
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_generalLedgerAccountsPath);

                IDictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("glClass", glClass);
                urlPath = UrlUtility.CombineEncodedUrlPathAndArguments(urlPath, parameters);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);

                var resource = JsonConvert.DeserializeObject<IEnumerable<GlAccount>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to retrieve the GL accounts.");
                throw;
            }
        }




        /// <summary>
        /// Validate a GL account. 
        /// If there is a fiscal year, it will also validate it for that year.
        /// </summary>
        /// <param name="generalLedgerAccountId">A GL account.</param>
        /// <param name="fiscalYear">Optional; General Ledger fiscal year.</param>
        /// <returns>Success: GL account information; failure: an error message.</returns>
        /// <exception cref="Exception">Unable to validate the GL account.</exception>
        public async Task<GlAccountValidationResponse> GetGlAccountValidationAsync(string generalLedgerAccountId, string fiscalYear = null)
        {
            if (string.IsNullOrEmpty(generalLedgerAccountId))
            {
                throw new ArgumentNullException("generalLedgerAccountId", "The General Ledger Account cannot be empty/null to perform validation.");
            }

            try
            {
                string[] pathStrings = new string[] { _glAccountValidationPath, generalLedgerAccountId };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);

                // Create and execute a request to validate a GL account.
                if (!string.IsNullOrEmpty(fiscalYear))
                {
                    IDictionary<string, string> parameters = new Dictionary<string, string>();
                    parameters.Add("fiscalYear", fiscalYear);
                    urlPath = UrlUtility.CombineEncodedUrlPathAndArguments(urlPath, parameters);
                }

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);

                var resource = JsonConvert.DeserializeObject<GlAccountValidationResponse>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to validate the GL account.");
                throw;
            }
        }

        #endregion

        #region General Ledger Activity

        /// <summary>
        /// Get the General Ledger Activity Details for a GL account for a fiscal year.
        /// </summary>
        /// <param name="criteria">The <see cref="Dtos.ColleagueFinance.GlActivityDetailQueryCriteria"> criteria</see> to query by.</param>
        /// <returns>A GL account with its actuals and encumbrance transactions for a fiscal year.</returns>
        public async Task<GlAccountActivityDetail> QueryGeneralLedgerActivityDetailsAsync(GlActivityDetailQueryCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria", "GL activity detail query criteria cannot be null.");
            }
            try
            {
                string[] pathStrings = new string[] { _qapiPath, _generalLedgerActivityDetailsPath };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);

                // Add version header
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

                // Use URL path and request data to call web api method (including query string)
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);

                var resource = JsonConvert.DeserializeObject<GlAccountActivityDetail>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException ex)
            {
                logger.Error(ex, "Unable to get GL activity details for the GL account {0} for fiscal year {1}.", criteria.GlAccount, criteria.FiscalYear);
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get GL activity details for the GL account {0} for fiscal year {1}.", criteria.GlAccount, criteria.FiscalYear);
                throw;
            }
        }

        #endregion

        /// <summary>
        /// Validate a next approver ID.
        /// </summary>
        /// <param name="nextApproverId">An approver ID.</param>
        /// <returns>Next Approver DTO with validation information.</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        /// <exception cref="Exception">The requested resource cannot be found.</exception>
        public async Task<NextApproverValidationResponse> GetNextApproverValidationAsync(string nextApproverId)
        {
            if (string.IsNullOrEmpty(nextApproverId))
            {
                throw new ArgumentNullException("nextApproverId", "The next approver ID cannot be empty/null to perform validation.");
            }

            try
            {
                // Create and execute a request to validate a specified next approver.
                string[] pathStrings = new string[] { _nextApproversPath, nextApproverId };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);

                var resource = JsonConvert.DeserializeObject<NextApproverValidationResponse>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException ex)
            {
                logger.Error(ex, "Unable to validate next approver {0}.", nextApproverId);
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to validate next approver.");
                throw;
            }
        }

        #region General Ledger Configuration

        /// <summary>
        /// Get the General Ledger configuration.
        /// </summary>
        /// <returns>General Ledger Configuration parameters.</returns>
        /// <exception cref="ResourceNotFoundException">Unable to get the General Ledger configuration.</exception>
        /// <exception cref="Exception">Unable to get the General Ledger configuration.</exception>
        public async Task<GeneralLedgerConfiguration> GetGeneralLedgerConfigurationAsync()
        {
            try
            {
                // Create and execute a request to get the GL configuration.
                string urlPath = UrlUtility.CombineUrlPath(_generalLedgerConfigurationPath);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);

                var resource = JsonConvert.DeserializeObject<GeneralLedgerConfiguration>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException ex)
            {
                logger.Error(ex, "Unable to get the General Ledger configuration.");
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get the General Ledger configuration.");
                throw;
            }
        }

        /// <summary>
        /// Get the General Ledger Fiscal year configuration.
        /// </summary>
        /// <returns>General Ledger Fiscal year Configuration parameters.</returns>
        /// <exception cref="ResourceNotFoundException">Unable to get the General Ledger Fiscal year configuration.</exception>
        /// <exception cref="Exception">Unable to get the General Ledger Fiscal year configuration.</exception>
        public async Task<GlFiscalYearConfiguration> GetGlFiscalYearConfigurationAsync()
        {
            try
            {
                // Create and execute a request to get the GL fiscal year configuration.
                string urlPath = UrlUtility.CombineUrlPath(_glFiscalYearConfigurationPath);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);

                var resource = JsonConvert.DeserializeObject<GlFiscalYearConfiguration>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException ex)
            {
                logger.Error(ex, "Unable to get the General Ledger fiscal year configuration.");
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get the General Ledger fiscal year configuration.");
                throw;
            }
        }



        #endregion

        #region Fiscal Years

        /// <summary>
        /// Get today's fiscal year based on the GL configuration.
        /// </summary>
        /// <returns>Returns today's fiscal year.</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        /// <exception cref="Exception">The requested resource cannot be found.</exception>
        public async Task<string> GetFiscalYearForTodayAsync()
        {
            try
            {
                // Create and execute a request to get the list of fiscal years
                string[] pathStrings = new string[] { _todaysFiscalYearPath };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);

                var resource = JsonConvert.DeserializeObject<string>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException ex)
            {
                logger.Error(ex, "Unable to get today's fiscal year.");
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get today's fiscal year.");
                throw;
            }
        }

        /// <summary>
        /// Get a list of fiscal years: the fiscal year for today's date plus
        /// up to the previous five fiscal years that are available.
        /// </summary>
        /// <returns>Returns a list of fiscal years for a drop down filter.</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        /// <exception cref="Exception">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<string>> GetFiscalYearsAsync()
        {
            try
            {
                // Create and execute a request to get the list of fiscal years
                string[] pathStrings = new string[] { _fiscalYearsPath };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);

                var resource = JsonConvert.DeserializeObject<IEnumerable<string>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException ex)
            {
                logger.Error(ex, "Unable to get fiscal years.");
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get fiscal years.");
                throw;
            }
        }

        #endregion

        /// <summary>
        /// Get a voucher document.
        /// </summary>
        /// <param name="voucherId">Voucher ID</param>
        /// <returns>Returns a voucher document.</returns>
        [Obsolete("Obsolete as of API 1.12. Use GetVoucherAsync.")]
        public Voucher GetVoucher(string voucherId)
        {
            try
            {
                // Create and execute a request to get a specified voucher
                string[] pathStrings = new string[] { _vouchersPath, voucherId };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);

                var resource = JsonConvert.DeserializeObject<Voucher>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException ex)
            {
                logger.Error(ex, "Unable to get voucher {0}.", voucherId);
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get voucher.");
                throw;
            }
        }

        /// <summary>
        /// Get a voucher document.
        /// </summary>
        /// <param name="voucherId">Voucher ID</param>
        /// <returns>Returns a voucher document.</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        /// <exception cref="Exception">The requested resource cannot be found.</exception>
        [Obsolete("Obsolete as of API 1.15. Use GetVoucher2Async.")]
        public async Task<Voucher> GetVoucherAsync(string voucherId)
        {
            try
            {
                // Create and execute a request to get a specified voucher
                string[] pathStrings = new string[] { _vouchersPath, voucherId };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);

                var resource = JsonConvert.DeserializeObject<Voucher>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException ex)
            {
                logger.Error(ex, "Unable to get voucher {0}.", voucherId);
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get voucher.");
                throw;
            }
        }

        /// <summary>
        /// Get a voucher document.
        /// </summary>
        /// <param name="voucherId">Voucher ID</param>
        /// <returns>Returns a voucher document.</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        /// <exception cref="Exception">The requested resource cannot be found.</exception>
        public async Task<Voucher2> GetVoucher2Async(string voucherId)
        {
            try
            {
                // Create and execute a request to get a specified voucher
                string[] pathStrings = new string[] { _vouchersPath, voucherId };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);

                var resource = JsonConvert.DeserializeObject<Voucher2>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException ex)
            {
                logger.Error(ex, "Unable to get voucher {0}.", voucherId);
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get voucher.");
                throw;
            }
        }

        /// <summary>
        /// Get a purchase order document.
        /// </summary>
        /// <param name="purchaseOrderId">Purchase Order ID</param>
        /// <returns>Returns a purchase order document.</returns>
        [Obsolete("Obsolete as of API 1.12. Use GetPurchaseOrderAsync.")]
        public PurchaseOrder GetPurchaseOrder(string purchaseOrderId)
        {
            try
            {
                // Create and execute a request to get a specified purchase order
                string[] pathStrings = new string[] { _purchaseOrdersPath, purchaseOrderId };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);

                var resource = JsonConvert.DeserializeObject<PurchaseOrder>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException ex)
            {
                logger.Error(ex, "Unable to get purchase order {0}.", purchaseOrderId);
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get purchase order.");
                throw;
            }
        }

        /// <summary>
        /// Get a purchase order document.
        /// </summary>
        /// <param name="purchaseOrderId">Purchase Order ID</param>
        /// <returns>Returns a purchase order document.</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        /// <exception cref="Exception">The requested resource cannot be found.</exception>
        public async Task<PurchaseOrder> GetPurchaseOrderAsync(string purchaseOrderId)
        {
            try
            {
                // Create and execute a request to get a specified purchase order
                string[] pathStrings = new string[] { _purchaseOrdersPath, purchaseOrderId };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);

                var resource = JsonConvert.DeserializeObject<PurchaseOrder>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException ex)
            {
                logger.Error(ex, "Unable to get purchase order {0}.", purchaseOrderId);
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get purchase order.");
                throw;
            }
        }

        /// <summary>
        /// Get a blanket purchase orders document.
        /// </summary>
        /// <param name="blanketPurchaseOrderId">Blanket purchase order ID</param>
        /// <returns>Returns a blanket purchase order document.</returns>
        [Obsolete("Obsolete as of API 1.12. Use GetBlanketPurchaseOrderAsync.")]
        public BlanketPurchaseOrder GetBlanketPurchaseOrder(string blanketPurchaseOrderId)
        {
            try
            {
                // Create and execute a request to get a specified blanket purchase order
                string[] pathStrings = new string[] { _blanketPurchaseOrdersPath, blanketPurchaseOrderId };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);

                var resource = JsonConvert.DeserializeObject<BlanketPurchaseOrder>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException ex)
            {
                logger.Error(ex, "Unable to get blanket purchase order {0}.", blanketPurchaseOrderId);
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get blanket purchase order.");
                throw;
            }
        }

        /// <summary>
        /// Get a blanket purchase orders document.
        /// </summary>
        /// <param name="blanketPurchaseOrderId">Blanket purchase order ID</param>
        /// <returns>Returns a blanket purchase order document.</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        /// <exception cref="Exception">The requested resource cannot be found.</exception>
        public async Task<BlanketPurchaseOrder> GetBlanketPurchaseOrderAsync(string blanketPurchaseOrderId)
        {
            try
            {
                // Create and execute a request to get a specified blanket purchase order
                string[] pathStrings = new string[] { _blanketPurchaseOrdersPath, blanketPurchaseOrderId };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);

                var resource = JsonConvert.DeserializeObject<BlanketPurchaseOrder>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException ex)
            {
                logger.Error(ex, "Unable to get blanket purchase order {0}.", blanketPurchaseOrderId);
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get blanket purchase order.");
                throw;
            }
        }

        /// <summary>
        /// Get a requisition document.
        /// </summary>
        /// <param name="requisitionId">Requisition ID</param>
        /// <returns>Returns a requisition document.</returns>
        [Obsolete("Obsolete as of API 1.12. Use GetRequisitionAsync.")]
        public Requisition GetRequisition(string requisitionId)
        {
            try
            {
                // Create and execute a request to a specified requisition
                string[] pathStrings = new string[] { _requisitionsPath, requisitionId };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);

                var resource = JsonConvert.DeserializeObject<Requisition>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException ex)
            {
                logger.Error(ex, "Unable to get requisition {0}.", requisitionId);
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get requisition.");
                throw;
            }
        }

        /// <summary>
        /// Get a requisition document.
        /// </summary>
        /// <param name="requisitionId">Requisition ID</param>
        /// <returns>Returns a requisition document.</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        /// <exception cref="Exception">The requested resource cannot be found.</exception>
        public async Task<Requisition> GetRequisitionAsync(string requisitionId)
        {
            try
            {
                // Create and execute a request to a specified requisition
                string[] pathStrings = new string[] { _requisitionsPath, requisitionId };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);

                var resource = JsonConvert.DeserializeObject<Requisition>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException ex)
            {
                logger.Error(ex, "Unable to get requisition {0}.", requisitionId);
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get requisition.");
                throw;
            }
        }

        /// <summary>
        /// Get a journal entry
        /// </summary>
        /// <param name="journalEntryId">Journal Entry ID</param>
        /// <returns>Returns a journal entry</returns>
        [Obsolete("Obsolete as of API 1.12. Use GetJournalEntryAsync.")]
        public JournalEntry GetJournalEntry(string journalEntryId)
        {
            try
            {
                // Create and execute a request to a specified journal entry
                string[] pathStrings = new string[] { _journalEntriesPath, journalEntryId };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);

                var resource = JsonConvert.DeserializeObject<JournalEntry>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException ex)
            {
                logger.Error(ex, "Unable to get journal entry {0}.", journalEntryId);
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get journal entry.");
                throw;
            }
        }

        /// <summary>
        /// Get a journal entry
        /// </summary>
        /// <param name="journalEntryId">Journal Entry ID</param>
        /// <returns>Returns a journal entry</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        /// <exception cref="Exception">The requested resource cannot be found.</exception>
        public async Task<JournalEntry> GetJournalEntryAsync(string journalEntryId)
        {
            try
            {
                // Create and execute a request to a specified journal entry
                string[] pathStrings = new string[] { _journalEntriesPath, journalEntryId };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);

                var resource = JsonConvert.DeserializeObject<JournalEntry>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException ex)
            {
                logger.Error(ex, "Unable to get journal entry {0}.", journalEntryId);
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get journal entry.");
                throw;
            }
        }

        /// <summary>
        /// Get a recurring voucher document
        /// </summary>
        /// <param name="recurringVoucherId">Recurring Voucher ID</param>
        /// <returns>Returns a recurring voucher document</returns>
        [Obsolete("Obsolete as of API 1.12. Use GetRecurringVoucherAsync.")]
        public RecurringVoucher GetRecurringVoucher(string recurringVoucherId)
        {
            try
            {
                // Create and execute a request to get a specified recurring voucher
                string[] pathStrings = new string[] { _recurringVouchersPath, recurringVoucherId };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);

                var resource = JsonConvert.DeserializeObject<RecurringVoucher>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException ex)
            {
                logger.Error(ex, "Unable to get recurring voucher {0}.", recurringVoucherId);
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get recurring voucher.");
                throw;
            }
        }

        /// <summary>
        /// Get a recurring voucher document
        /// </summary>
        /// <param name="recurringVoucherId">Recurring Voucher ID</param>
        /// <returns>Returns a recurring voucher document</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        /// <exception cref="Exception">The requested resource cannot be found.</exception>
        public async Task<RecurringVoucher> GetRecurringVoucherAsync(string recurringVoucherId)
        {
            try
            {
                // Create and execute a request to get a specified recurring voucher
                string[] pathStrings = new string[] { _recurringVouchersPath, recurringVoucherId };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);

                var resource = JsonConvert.DeserializeObject<RecurringVoucher>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException ex)
            {
                logger.Error(ex, "Unable to get recurring voucher {0}.", recurringVoucherId);
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get recurring voucher.");
                throw;
            }
        }

        /// <summary>
        /// Get all AP tax codes
        /// </summary>
        /// <returns>Returns all Accounts Payable Tax codes</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        [Obsolete("Obsolete as of API 1.12. Use GetAccountsPayableTaxesAsync.")]
        public IEnumerable<AccountsPayableTax> GetAccountsPayableTaxes()
        {
            try
            {
                // Create and execute a request to get all Accounts Payable Tax codes.
                string urlPath = UrlUtility.CombineUrlPath(_accountsPayableTaxCodesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);

                var resource = JsonConvert.DeserializeObject<List<AccountsPayableTax>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException ex)
            {
                logger.Error(ex, "Unable to get Accounts Payable Tax codes.");
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get Accounts Payable Tax codes.");
                throw;
            }
        }

        /// <summary>
        /// Get all AP tax codes
        /// </summary>
        /// <returns>Returns all Accounts Payable Tax codes</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        /// <exception cref="Exception">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<AccountsPayableTax>> GetAccountsPayableTaxesAsync()
        {
            try
            {
                // Create and execute a request to get all Accounts Payable Tax codes.
                string urlPath = UrlUtility.CombineUrlPath(_accountsPayableTaxCodesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);

                var resource = JsonConvert.DeserializeObject<List<AccountsPayableTax>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException ex)
            {
                logger.Error(ex, "Unable to get Accounts Payable Tax codes.");
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get Accounts Payable Tax codes.");
                throw;
            }
        }

        /// <summary>
        /// Get all Accounts Payable Type codes.
        /// </summary>
        /// <returns>Returns all Accounts Payable type codes</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        [Obsolete("Obsolete as of API 1.12. Use GetAccountsPayableTypesAsync.")]
        public IEnumerable<AccountsPayableType> GetAccountsPayableTypes()
        {
            try
            {
                // Create and execute a request to get all Accounts Payable tax codes.
                string urlPath = UrlUtility.CombineUrlPath(_accountsPayableTypeCodesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);

                var resource = JsonConvert.DeserializeObject<List<AccountsPayableType>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException ex)
            {
                logger.Error(ex, "Unable to get Accounts Payable Type codes.");
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get Accounts Payable Type codes.");
                throw;
            }
        }

        /// <summary>
        /// Get all Accounts Payable Type codes.
        /// </summary>
        /// <returns>Returns all Accounts Payable type codes</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        /// <exception cref="Exception">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<AccountsPayableType>> GetAccountsPayableTypesAsync()
        {
            try
            {
                // Create and execute a request to get all Accounts Payable tax codes.
                string urlPath = UrlUtility.CombineUrlPath(_accountsPayableTypeCodesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);

                var resource = JsonConvert.DeserializeObject<List<AccountsPayableType>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException ex)
            {
                logger.Error(ex, "Unable to get Accounts Payable Type codes.");
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get Accounts Payable Type codes.");
                throw;
            }
        }

        /// <summary>
        /// Get a general ledger account object.
        /// </summary>
        /// <returns>A general ledger account</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        /// <exception cref="Exception">The requested resource cannot be found.</exception>
        public async Task<GeneralLedgerAccount> GetGeneralLedgerAccountAsync(string generalLedgerAccountId)
        {
            try
            {
                // Create and execute a request to get a specified recurring voucher
                string[] pathStrings = new string[] { _generalLedgerAccountsPath, generalLedgerAccountId };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);

                var resource = JsonConvert.DeserializeObject<GeneralLedgerAccount>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException ex)
            {
                logger.Error(ex, "Unable to get general ledger account {0}.", generalLedgerAccountId);
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get general ledger account.");
                throw;
            }
        }

        /// <summary>
        /// Get the GL object codes based on filter criteria.
        /// </summary>
        /// <param name="criteria">The <see cref="Dtos.ColleagueFinance.CostCenterComponentQueryCriteria"> criteria</see> to query by.</param>
        /// <returns>GL object codes that match the query criteria.</returns>
        public async Task<IEnumerable<GlObjectCode>> QueryGlObjectCodesAsync(CostCenterQueryCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria", "Cost center query criteria cannot be null.");
            }
            try
            {
                string[] pathStrings = new string[] { _qapiPath, _generalLedgerObjectCodesPath };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);

                // Add version header
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

                // Use URL path and request data to call web api method (including query string)
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);

                var resource = JsonConvert.DeserializeObject<List<GlObjectCode>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException ex)
            {
                logger.Error(ex, "Unable to get filtered cost centers.");
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get filtered cost centers.");
                throw;
            }
        }

        #region Finance Query
        /// <summary>
        /// Get the GL accounts based on filter criteria.
        /// </summary>
        /// <param name="criteria">The <see cref="Dtos.ColleagueFinance.FinanceQueryCriteria"> criteria</see> to query by.</param>
        /// <returns>GL accounts that match the query criteria.</returns>
        public async Task<IEnumerable<FinanceQuery>> QueryFinanceQuerySelectionByPostAsync(FinanceQueryCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria", "Finance query criteria cannot be null.");
            }
            try
            {
                string[] pathStrings = new string[] { _qapiPath, _financeQueryPath };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);

                // Add version header
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

                // Use URL path and request data to call web api method (including query string)
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);

                var resource = JsonConvert.DeserializeObject<List<FinanceQuery>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException ex)
            {
                logger.Error(ex, "Unable to get filtered finance query criteria results.");
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get filtered finance query criteria results.");
                throw;
            }
        }
        #endregion

        /// <summary>
        /// Get a list of requisition summary.
        /// </summary>
        /// <param name="personId">Person ID</param>
        /// <returns>list of requisition summary.</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        /// <exception cref="Exception">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<RequisitionSummary>> GetRequisitionsSummaryByPersonIdAsync(string personId)
        {
            try
            {
                // Create and execute a request to a specified requisition
                string[] pathStrings = new string[] { _requisitionsSummaryPath, personId };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);

                var resource = JsonConvert.DeserializeObject<List<RequisitionSummary>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException ex)
            {
                logger.Error(ex, "Unable to get requisition summary list  {0}.", personId);
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get requisition summary list.");
                throw;
            }
        }
        
        /// <summary>
        /// Get Ship To Codes with descriptions
        /// </summary>
        /// <returns>Returns List of ShipToCodes</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        /// <exception cref="Exception">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<ShipToCode>> GetShipToCodesAsync()
        {
            try
            {
                // Create and execute a request to get the list of fiscal years
                string[] pathStrings = new string[] { _shipToCodesPath };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<List<ShipToCode>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException ex)
            {
                logger.Error(ex, "Unable to get ship to codes.");
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get ship to codes.");
                throw;
            }
        }

        /// <summary>
        /// Get Commodity Codes with descriptions
        /// </summary>
        /// <returns>Returns List of Commodity Codes</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        /// <exception cref="Exception">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.ColleagueFinance.ProcurementCommodityCode>> GetAllCommodityCodesAsync()
        {
            try
            {
                // Create and execute a request to get the list of fiscal years
                string[] pathStrings = new string[] { _commodityCodesPath };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<List<Ellucian.Colleague.Dtos.ColleagueFinance.ProcurementCommodityCode>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException ex)
            {
                logger.Error(ex, "Unable to get commodity codes.");
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get commodity codes.");
                throw;
            }
        }

        /// <summary>
        /// Get CF Web configurations
        /// </summary>
        /// <returns>Returns CF Web configurations.</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        /// <exception cref="Exception">The requested resource cannot be found.</exception>
        public async Task<ColleagueFinanceWebConfiguration> GetColleagueFinanceWebConfigurationsAsync()
        {
            try
            {
                // Create and execute a request to fetch CF Web configurations
                string[] pathStrings = new string[] { _cfWebConfigurationsPath };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);

                var resource = JsonConvert.DeserializeObject<ColleagueFinanceWebConfiguration>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException ex)
            {
                logger.Error(ex, "Unable to get CF Web configurations.");
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get CF Web configurations.");
                throw;
            }
        }

        /// <summary>
        /// Create / Update a requisition.
        /// </summary>
        /// <param name="requisitionCreateUpdateRequest">The requisition create update request DTO.</param>        
        /// <returns>The requisition create update response DTO.</returns>
        public async Task<RequisitionCreateUpdateResponse> CreateUpdateRequisitionAsync(RequisitionCreateUpdateRequest requisitionCreateUpdateRequest)
        {

            if (requisitionCreateUpdateRequest == null)
            {
                throw new ArgumentNullException("requisitionCreateUpdateRequest", "The requisition create update request cannot be empty/null when posting a requisition.");
            }
            try
            {
                string[] pathStrings = new string[] { _requisitionsPath };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePostRequestWithResponseAsync(requisitionCreateUpdateRequest, urlPath, headers: headers);

                var resource = JsonConvert.DeserializeObject<RequisitionCreateUpdateResponse>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (ResourceNotFoundException exnf)
            {
                logger.Error(exnf, "Unable to create requisition.");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to to create requisition.");
                throw;
            }
        }

        /// <summary>
        /// Get a list of Purchase Order summary.
        /// </summary>
        /// <param name="personId">person id</param>
        /// <returns>list of purchase order summary.</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        /// <exception cref="Exception">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<PurchaseOrderSummary>> GetPurchaseOrderSummaryByPersonIdAsync(string personId)
        {
            try
            {
                // Create and execute a request to a specified purchase order
                string[] pathStrings = new string[] { _purchaseOrdersSummaryPath, personId };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);

                var resource = JsonConvert.DeserializeObject<List<PurchaseOrderSummary>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException ex)
            {
                logger.Error(ex, "Unable to get purchase order {0}.", personId);
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get purchase order.");
                throw;
            }
        }

        /// <summary>
        /// Get the list of vendors based on keyword search.
        /// </summary>
        /// <param name="criteria"> The search criteria get list of vendors.</param>
        /// <returns> The vendor search results</returns>
        /// <exception cref="ResourceNotFoundException">Unable to perform vendor search.</exception>
        /// <exception cref="Exception">Unable to perform vendor search.</exception>
        public async Task<IEnumerable<VendorSearchResult>> QueryVendorsByPostAsync(VendorSearchCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria", "vendor query criteria cannot be null.");
            }
            try
            {
                // Create and execute a request to get the search vendors by keyword.
                string[] pathStrings = new string[] { _qapiPath, _vendorsPath };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);

                // Add version header
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

                // Use URL path and request data to call the web api method (including query string)
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);

                var resource = JsonConvert.DeserializeObject<List<VendorSearchResult>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let the calling code determine how to handle it.
            catch (ResourceNotFoundException ex)
            {
                logger.Error(ex, "Unable to perform vendor search.");
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to perform vendor search.");
                throw;
            }
        }

        /// <summary>
        /// Get commodity unit types for line items.
        /// </summary>
        /// <returns>Success: List of unit type objects.</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.CommodityUnitType>> GetAllCommodityUnitTypesAsync()
        {
            try
            {
                // Create and execute a request to get the list of unit types.
                string[] pathStrings = new string[] { _commodityUnitTypesPath };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<List<Ellucian.Colleague.Dtos.CommodityUnitType>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException ex)
            {
                logger.Error(ex, "Unable to get unit types.");
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get unit types.");
                throw;
            }
        }

        /// <summary>
        /// Get fixed assets transfer flags with descriptions
        /// </summary>
        /// <returns>Returns list of fixed assets transfer flags</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        /// <exception cref="Exception">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<FixedAssetsFlag>> GetFixedAssetTransferFlagsAsync()
        {
            try
            {
                // Create and execute a request to get the list of fiscal years
                string[] pathStrings = new string[] { _fixedAssetTransferFlagsPath };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<List<FixedAssetsFlag>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException ex)
            {
                logger.Error(ex, "Unable to get fixed asset transfer flags.");
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get fixed asset transfer flags.");
                throw;
            }
        }

        /// <summary>
        /// Get tax forms with descriptions
        /// </summary>
        /// <returns>Returns list of tax forms</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        /// <exception cref="Exception">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<TaxForm>> GetTaxFormsAsync()
        {
            try
            {
                // Create and execute a request to get the list of fiscal years
                string[] pathStrings = new string[] { _taxFormCodesPath };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<List<TaxForm>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException ex)
            {
                logger.Error(ex, "Unable to get tax form codes.");
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get tax form codes.");
                throw;
            }
        }

        /// <summary>
        /// Get requisition selected by user and line item default information.
        /// </summary>
        /// <param name="requisitionId">Requisition ID</param>
        /// <returns>Modify requisition dto.</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        /// <exception cref="Exception">The requested resource cannot be found.</exception>
        public async Task<ModifyRequisition> GetRequisitionForModifyWithLineItemDefaultsAsync(string requisitionId)
        {
            try
            {
                // Create and execute a request to a specified requisition
                string[] pathStrings = new string[] { _requisitionModifyPath, requisitionId };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);

                var resource = JsonConvert.DeserializeObject<ModifyRequisition>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException ex)
            {
                logger.Error(ex, "Unable to get requisition for modify {0}.", requisitionId);
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get requisition  for modify.");
                throw;
            }
        }
    }
}

