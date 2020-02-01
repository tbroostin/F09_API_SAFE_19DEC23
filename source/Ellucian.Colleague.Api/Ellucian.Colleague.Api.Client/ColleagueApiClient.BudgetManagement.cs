// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.BudgetManagement;
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
        /// <summary>
        /// Get the Budget Development configuration.
        /// </summary>
        /// <returns> Budget Development Configuration parameters.</returns>
        /// <exception cref="ResourceNotFoundException">Unable to get the Budget Development configuration.</exception>
        /// <exception cref="Exception">Unable to get the Budget Development configuration.</exception>
        public async Task<BudgetConfiguration> GetBudgetDevelopmentConfigurationAsync()
        {
            try
            {
                // Create and execute a request to get the Budget Development configuration.
                string urlPath = UrlUtility.CombineUrlPath(_budgetDevelopmentConfigurationPath);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);

                var resource = JsonConvert.DeserializeObject<BudgetConfiguration>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException ex)
            {
                logger.Error(ex, "Unable to get the Budget Development configuration.");
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get the Budget Development configuration.");
                throw;
            }
        }

        /// <summary>
        /// Get the Budget Development working budget.
        /// </summary>
        /// <param name="startPosition">Start position of the budget line items to return.</param>
        /// <param name="recordCount">Number of budget line items to return.star</param>
        /// <returns> Budget Development working budget.</returns>
        /// <exception cref="ResourceNotFoundException">Unable to get the working budget.</exception>
        /// <exception cref="Exception">Unable to get the working budget.</exception>
        public async Task<WorkingBudget> GetBudgetDevelopmentWorkingBudgetAsync(int startPosition, int recordCount)
        {
            try
            {
                // Create and execute a request to get the Budget Development working budget.
                string[] pathStrings = new string[] { _budgetDevelopmentWorkingBudgetPath, startPosition.ToString(), recordCount.ToString() };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);

                var resource = JsonConvert.DeserializeObject<WorkingBudget>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException ex)
            {
                logger.Error(ex, "Unable to get the working budget.");
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get the working budget.");
                throw;
            }
        }

        /// <summary>
        /// Get the working budget based on filter criteria.
        /// </summary>
        /// <param name="criteria"> The criteria to filter the working budget line items.</param>
        /// <returns> The working budget line items that match the query criteria.</returns>
        /// <exception cref="ResourceNotFoundException">Unable to get the working budget.</exception>
        /// <exception cref="Exception">Unable to get the working budget.</exception>
        [Obsolete("Obsolete as of Colleague Web API 1.25. Use QueryWorkingBudget2Async")]
        public async Task<WorkingBudget> QueryWorkingBudgetAsync(WorkingBudgetQueryCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria", "Working budget query criteria cannot be null.");
            }
            try
            {
                // Create and execute a request to get the filtered working budget.
                string[] pathStrings = new string[] { _qapiPath, _workingBudgetPath };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);

                // Add version header
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

                // Use URL path and request data to call the web api method (including query string)
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);

                var resource = JsonConvert.DeserializeObject<WorkingBudget>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let the calling code determine how to handle it.
            catch (ResourceNotFoundException ex)
            {
                logger.Error(ex, "Unable to get the working budget.");
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get the working budget.");
                throw;
            }
        }

        /// <summary>
        /// Get the working budget based on filter criteria with or without subtotals.
        /// </summary>
        /// <param name="criteria"> The criteria to filter the working budget line items.</param>
        /// <returns> The working budget line items that match the query criteria with or without subtotals..</returns>
        /// <exception cref="ResourceNotFoundException">Unable to get the working budget.</exception>
        /// <exception cref="Exception">Unable to get the working budget.</exception>
        public async Task<WorkingBudget2> QueryWorkingBudget2Async(WorkingBudgetQueryCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria", "Working budget query criteria cannot be null.");
            }
            try
            {
                // Create and execute a request to get the filtered working budget.
                string[] pathStrings = new string[] { _qapiPath, _workingBudgetPath };
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);

                // Add version header
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);

                // Use URL path and request data to call the web api method (including query string)
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);

                var resource = JsonConvert.DeserializeObject<WorkingBudget2>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let the calling code determine how to handle it.
            catch (ResourceNotFoundException ex)
            {
                logger.Error(ex, "Unable to get the working budget.");
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get the working budget.");
                throw;
            }
        }

        /// <summary>
        /// Process updates to a list of budget line items.
        /// </summary>
        /// <param name="budgetLineItems">A list of budget line items.</param>
        /// <returns>A list of budget line items that have been updated.</returns>
        public async Task<List<BudgetLineItem>> UpdateBudgetDevelopmentWorkingBudgetAsync(List<BudgetLineItem> budgetLineItems)
        {
            try
            {
                // Create and execute a request to update a budget adjustment
                string[] pathStrings = new string[] { _budgetDevelopmentWorkingBudgetPath};
                string urlPath = UrlUtility.CombineUrlPath(pathStrings);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePutRequestWithResponseAsync(budgetLineItems, urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<List<BudgetLineItem>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw;
            }
        }

        /// <summary>
        /// Get the budget officers for the working budget.
        /// </summary>
        /// <returns>Budget officers for the working budget.</returns>
        /// <exception cref="Exception">Unable to get budget officers for the working budget.</exception>
        public async Task<List<BudgetOfficer>> GetWorkingBudgetOfficersAsync()
        {
            try
            {
                // Create and execute a request to get the budget officers for the working budget.
                var queryString = UrlUtility.BuildEncodedQueryString("isInWorkingBudget", "True");
                var combinedUrl = UrlUtility.CombineUrlPathAndArguments(_budgetOfficersPath, queryString);
                
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(combinedUrl, headers: headers);

                var resource = JsonConvert.DeserializeObject<List<BudgetOfficer>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception e)
            {
                logger.Error(e, "Unable to get budget officers for the working budget.");
                throw;
            }
        }

        /// <summary>
        /// Get the budget reporting units for the working budget.
        /// </summary>
        /// <returns>Budget reporting units for the working budget.</returns>
        /// <exception cref="Exception">Unable to get budget reporting units for the working budget.</exception>
        public async Task<List<BudgetReportingUnit>> GetBudgetReportingUnitsAsync()
        {
            try
            {
                // Create and execute a request to get the budget reporting units for the working budget.
                var queryString = UrlUtility.BuildEncodedQueryString("isInWorkingBudget", "True");
                var combinedUrl = UrlUtility.CombineUrlPathAndArguments(_budgetReportingUnitsPath, queryString);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(combinedUrl, headers: headers);

                var resource = JsonConvert.DeserializeObject<List<BudgetReportingUnit>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception e)
            {
                logger.Error(e, "Unable to get budget reporting units for the working budget.");
                throw;
            }
        }
    }
}
