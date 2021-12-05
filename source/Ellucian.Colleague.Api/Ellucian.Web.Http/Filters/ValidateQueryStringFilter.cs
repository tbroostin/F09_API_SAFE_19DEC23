// Copyright 2020 Ellucian Company L.P. and its affiliates.

using System.Web.Http.Controllers;
using System.Net.Http;
using System.Linq;
using System;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;
using slf4net;
using System.Collections.Generic;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Controllers;
using System.Net.Http.Headers;
using System.Text;

namespace Ellucian.Web.Http.Filters
{
    /// <summary>
    /// Action Filter to ValidateQueryString.  Used in conjuction 
    /// with the QueryStringFilterFilter to validate the query string only 
    /// contains expected query parameters for filtering
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ValidateQueryStringFilter : System.Web.Http.Filters.ActionFilterAttribute
    {
        private const string CustomMediaType = "X-Media-Type";

        /// <summary>
        /// List of valid query parameters
        /// </summary>
        public string[] ValidQueryParameters = new string[] { "offset", "limit", "sort" };

        /// <summary>
        /// List of valid named query parameters (legacy)
        /// </summary>
        public string[] NamedQueries = null;


        /// <summary>
        /// Prevent query string validation from occuring
        /// </summary>
        public bool IgnoreFilterValidation { get; set; }

        /// <summary>
        /// Allow multiple query parameters to be provided.
        /// </summary>
        public bool AllowMultipleQueries { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ignoreFilterValidation"></param>
        public ValidateQueryStringFilter(bool ignoreFilterValidation = false)
        {
            IgnoreFilterValidation = ignoreFilterValidation;
            AllowMultipleQueries = false;
        }

        /// <summary>
        /// Constructor 
        /// </summary>
        public ValidateQueryStringFilter(string[] filterGroupNames, bool ignoreFilterValidation = false, bool allowMultipleQueries = false, string[] namedQueries = null)
        {
            IgnoreFilterValidation = ignoreFilterValidation;
            AllowMultipleQueries = allowMultipleQueries;
            ValidQueryParameters =
                filterGroupNames.Union(ValidQueryParameters).ToArray();
            NamedQueries = namedQueries;
        }

        /// <summary>
        /// Executed before the action method is called.
        /// </summary>
        /// <param name="actionExecutedContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task OnActionExecutingAsync(HttpActionContext actionExecutedContext, CancellationToken cancellationToken)
        {
            var logger = actionExecutedContext.Request.GetDependencyScope().GetService(typeof(ILogger)) as ILogger;
            //Check if Response, Content are not null and that the content is ObjectContent
            if ((actionExecutedContext.Request != null)
              && (actionExecutedContext.Request.Method == HttpMethod.Get))
            {
                // if the filter override is set, then return
                if (IgnoreFilterValidation)
                    return base.OnActionExecutingAsync(actionExecutedContext, cancellationToken);

                var filterGroupNames = new List<string>();

                //get the declared QueryStringFilter action filters.  They will contain names of valid filters
                var queryStringFilterFilters = actionExecutedContext.ActionDescriptor.GetFilters()
                   .Where(x => x.GetType() == typeof(QueryStringFilterFilter));

                if (queryStringFilterFilters != null && queryStringFilterFilters.Any())
                {
                    foreach (var queryStringFilterFilter in queryStringFilterFilters)
                    {
                        var filter = queryStringFilterFilter as QueryStringFilterFilter;
                        filterGroupNames.Add(filter.FilterGroupName);
                    }
                }

                var queryParameters = actionExecutedContext.Request.GetQueryNameValuePairs();
                // if there are no queryParameters to check, then exit.
                if (queryParameters == null || !queryParameters.Any())
                    return base.OnActionExecutingAsync(actionExecutedContext, cancellationToken);

                // If the queryParameter contains multiple queries, throw an error                       
                var filterQueryParameters = queryParameters.Select(kvp => kvp.Key).ToList().Except(ValidQueryParameters);
                if ((!AllowMultipleQueries) && (filterQueryParameters.Count() > 1))
                {
                    var errorMessage = "Can not provide multiple filter queries in a single request";
                    return ThrowException(actionExecutedContext, cancellationToken, errorMessage);
                }

                // If no filterGroups are defined, and we had queryParameters other than our valid sub list, then return a 400
                if (!filterGroupNames.Any())
                {
                    //if (!queryParameters.Select(kvp => kvp.Key).ToList().Any(x => ValidQueryParameters.ToList().Contains(x)))
                    if (filterQueryParameters.Count() > 0)
                    {
                        var errorMessage = string.Format("'{0}' is an invalid query parameter for filtering", string.Join(" ,", filterQueryParameters));
                        return ThrowException(actionExecutedContext, cancellationToken, errorMessage);
                    }
                }
                // If the queryParameter is not a part of the declared filter group, then return a 400
                else if (filterQueryParameters
                        .Any(queryParameter => !filterGroupNames.Any(p => p == queryParameter)))
                {
                    var errorMessage = string.Format("'{0}' is an invalid query parameter for filtering", string.Join(" ,", filterQueryParameters));
                    return ThrowException(actionExecutedContext, cancellationToken, errorMessage);
                }
            }
            return base.OnActionExecutingAsync(actionExecutedContext, cancellationToken);
        }
        private Task ThrowException(HttpActionContext actionExecutedContext, CancellationToken cancellationToken, string errorMessage)
        {
            bool customMediaTypeAttributeFilter = false;

            var customAttributes = actionExecutedContext
                .ActionDescriptor.GetCustomAttributes<CustomMediaTypeAttributeFilter>();

            if ((customAttributes != null) && (customAttributes.Any()))
            {
                customMediaTypeAttributeFilter = customAttributes.Any(x => x.ErrorContentType == BaseCompressedApiController.IntegrationErrors2);
            }
            if (customMediaTypeAttributeFilter)
            {
                var exceptionObject = new IntegrationApiException();
                exceptionObject.AddError(new IntegrationApiError("Global.Internal.Error",
                    errorMessage.Replace(Environment.NewLine, " ").Replace("\n", " ")));
                var serialized = JsonConvert.SerializeObject(exceptionObject);
                actionExecutedContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest) { Content = new StringContent(serialized) };
                actionExecutedContext.Response.Content.Headers.Remove(CustomMediaType);

                IEnumerable<string> customMediaTypeValue = null;
                if (!actionExecutedContext.Response.Content.Headers.TryGetValues(CustomMediaType, out customMediaTypeValue))
                    actionExecutedContext.Response.Content.Headers.Add(CustomMediaType, BaseCompressedApiController.IntegrationErrors2);


                return base.OnActionExecutingAsync(actionExecutedContext, cancellationToken);
            }
            throw new Exception(errorMessage);
        }
    }
}