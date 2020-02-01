﻿// Copyright 2016-2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Web.Http.Filters;

namespace Ellucian.Web.Http.Filters
{
    /// <summary>
    /// Action Filter to add the custom content type
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class CustomMediaTypeAttributeFilter : ActionFilterAttribute
    {
        private const string CustomMediaType = "X-Media-Type";
        /// <summary>
        /// Gets & Sets the value to Content Type when error is occured
        /// </summary>
        public string ErrorContentType { get; set; }

        /// <summary>
        /// Add the custom Content type to the response headers
        /// </summary>
        /// <param name="actionExecutedContext"></param>
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            //Check if Response, Content are NOT NULL
            if (actionExecutedContext.Response != null && actionExecutedContext.Response.Content != null)
            {
                if (actionExecutedContext.Response.IsSuccessStatusCode)
                {
                    var RequestedContentTypeval = actionExecutedContext.ActionContext.RequestContext.RouteData.Values["RequestedContentType"];
                 
                    IEnumerable<string> customMediaTypeValue = null;
                    if ((RequestedContentTypeval != null) && (!actionExecutedContext.Response.Content.Headers.TryGetValues(CustomMediaType, out customMediaTypeValue)))
                        actionExecutedContext.Response.Content.Headers.Add(CustomMediaType, RequestedContentTypeval.ToString());

                }
                else if (!string.IsNullOrEmpty(ErrorContentType))
                {
                    actionExecutedContext.Response.Content.Headers.Add(CustomMediaType, this.ErrorContentType);
                }
            }
            base.OnActionExecuted(actionExecutedContext);
        }
    }
}