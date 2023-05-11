// Copyright 2018-2022 Ellucian Company L.P. and its affiliates.

using System.Web.Http.Controllers;
using System.Net.Http;
using System.Linq;
using System;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.Converters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Controllers;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;

namespace Ellucian.Web.Http.Filters
{
    /// <summary>
    /// Action Filter for all QueryStringFilter requests
    /// Defines which query parameter are supported for filtering.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class QueryStringFilterFilter : System.Web.Http.Filters.ActionFilterAttribute
    {
        private const string CustomMediaType = "X-Media-Type";

        /// <summary>
        /// Gets & Sets the filter group name
        /// </summary>
        public string FilterGroupName
        {
            get { return _filterGroupName; }
            set { _filterGroupName = value; }
        }

        private string _filterGroupName;

        /// <summary>
        /// Gets & Sets the value to ignore filter at the action filter
        /// </summary>
        public bool IgnoreFilters { get; set; }

        /// <summary>
        /// The query parameter will need to be deserialized to this type
        /// </summary>
        public Type FilterType { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public QueryStringFilterFilter(string filterGroupName, Type T)
        {
            if (string.IsNullOrEmpty(filterGroupName))
            {
                throw new ArgumentNullException("filterGroupName", "FilterGroupName is a required field");
            }
            if (T == null)
            {
                throw new ArgumentNullException("Type", "Type is a required field");
            }
            FilterType = T;
            IgnoreFilters = false;
            _filterGroupName = filterGroupName;
        }      

        /// <summary>
        /// Executed before the action method is called.
        /// </summary>
        /// <param name="actionExecutedContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task OnActionExecutingAsync(HttpActionContext actionExecutedContext, CancellationToken cancellationToken)
        {          
            if ((IgnoreFilters) || string.IsNullOrEmpty(this.FilterGroupName))
            {
                return base.OnActionExecutingAsync(actionExecutedContext, cancellationToken);
            }

            if ((actionExecutedContext.Request != null) && (actionExecutedContext.Request.Method == HttpMethod.Get))
            {
                if (actionExecutedContext.ActionArguments != null && actionExecutedContext.ActionArguments.Any() &&
                        actionExecutedContext.ActionArguments.ContainsKey(this.FilterGroupName))
                {
                    var queryStringFilter = actionExecutedContext.ActionArguments[this.FilterGroupName] as QueryStringFilter;

                    if ((queryStringFilter != null) && (!string.IsNullOrEmpty(queryStringFilter.JsonQuery)))
                    {

                        //Create JsonConverter to control how query string segment will be deserialized
                        // this will also determine if invalid filter properties are received  
                        try
                        {
                            var filterConverter = new FilterConverter(this.FilterGroupName);

                            var filterObj = JsonConvert.DeserializeObject(queryStringFilter.JsonQuery,
                                FilterType,
                               filterConverter);

                            if (filterConverter.ContainsInvalidFilterProperties())
                            {
                                throw new ColleagueWebApiException(filterConverter.GetInvalidFilterErrorMessage());
                            }

                            if (filterConverter.ContainsEmptyFilterProperties)
                            {
                                var emptyFilterPropertiesName = "EmptyFilterProperties";
                                object emptyQueryStringParms;
                                actionExecutedContext.Request.Properties.TryGetValue(emptyFilterPropertiesName, out emptyQueryStringParms);
                                if (emptyQueryStringParms == null)
                                {
                                    actionExecutedContext.Request.Properties.Add(emptyFilterPropertiesName, true);
                                }
                            }

                            if (filterConverter.ContainsFilterProperties())
                            {
                                var filterPropertyName = "FilterProperties";
                                actionExecutedContext.Request.Properties.Add(filterPropertyName, filterConverter.GetFilterProperties);
                            }

                            var contextPropertyName = string.Format("FilterObject{0}", this.FilterGroupName);
                            actionExecutedContext.Request.Properties.Add(contextPropertyName, filterObj);
                        }
                        catch (Exception ex)
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
                                    ex.Message.Replace(Environment.NewLine, " ").Replace("\n", " ")));
                                var serialized = JsonConvert.SerializeObject(exceptionObject);
                                actionExecutedContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest) { Content = new StringContent(serialized) };

                                actionExecutedContext.Response.Content.Headers.Remove(CustomMediaType);

                                IEnumerable<string> customMediaTypeValue = null;
                                if (!actionExecutedContext.Response.Content.Headers.TryGetValues(CustomMediaType, out customMediaTypeValue))
                                    actionExecutedContext.Response.Content.Headers.Add(CustomMediaType, BaseCompressedApiController.IntegrationErrors2);

                                actionExecutedContext.Response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json")
                                {
                                    CharSet = Encoding.UTF8.WebName
                                };
                                IEnumerable<string> contentTypeValue = null;
                                if (!actionExecutedContext.Response.Content.Headers.TryGetValues("Content-Type", out contentTypeValue))
                                    actionExecutedContext.Response.Content.Headers.Add("Content-Type", "application/json;charset=UTF-8");

                                return base.OnActionExecutingAsync(actionExecutedContext, cancellationToken);
                            }
                            throw new ColleagueWebApiException(ex.Message, ex.InnerException);
                        }
                    }                  
                }
            }

            return base.OnActionExecutingAsync(actionExecutedContext, cancellationToken);
        }
    }
}