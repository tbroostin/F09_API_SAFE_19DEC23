// Copyright 2018 Ellucian Company L.P. and its affiliates.

using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Extensions;
using Ellucian.Web.Http.Filters;
using System.Net.Http;

namespace Ellucian.Web.Http.ModelBinding
{
    /// <summary>
    /// Model binder for QueryStringFilter model
    /// </summary>
    public class QueryStringFilterBinder : IModelBinder
    {
        /// <summary>
        /// Binds the filter from the querystring to a QueryStringFilter class
        /// </summary>
        /// <param name="context"></param>
        /// <param name="bindingContext"></param>
        /// <returns></returns>
        public bool BindModel(HttpActionContext context, ModelBindingContext bindingContext)
        {           
            var queryStringFilterFilters = context.ActionDescriptor.GetFilters()
                .Where(x => x.GetType() == typeof(QueryStringFilterFilter));

            if (queryStringFilterFilters != null && queryStringFilterFilters.Any())
            {
                foreach (var filters in queryStringFilterFilters)
                {            
                    var filter = filters as QueryStringFilterFilter;

                    if ((filter != null) && (!string.IsNullOrEmpty(filter.FilterGroupName))
                     && (bindingContext.ModelName == filter.FilterGroupName))
                    {
                        //get the query string segment for this filter
                        QueryStringFilter queryStringFilter = context.Request.GetFilterParameters(filter.FilterGroupName);
                        bindingContext.Model = queryStringFilter;
                        return true;
                    }
                }
            }
            return true;
        }
    }
}