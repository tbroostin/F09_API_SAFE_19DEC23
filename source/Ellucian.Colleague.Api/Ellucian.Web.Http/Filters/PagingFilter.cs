// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using System.Web;
using System.Net.Http;
using System.Net.Http.Formatting;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Extensions;
using System.Collections;
using System.Web.Http;
using System.Collections.Specialized;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Ellucian.Web.Http.Filters
{
    /// <summary>
    /// Action Filter for all the paging requests
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class PagingFilter : System.Web.Http.Filters.ActionFilterAttribute
    {
        /// <summary>
        /// Gets & Sets the value to ignore paging at the action filter
        /// </summary>
        public bool IgnorePaging { get; set; }
        /// <summary>
        /// Gets and Sets the value of default limit, if no limit or provided limit > default limit
        /// </summary>
        public int DefaultLimit { get; set; }

        /// <summary>
        /// Default Constructor for Paging Filter
        /// </summary>
        public PagingFilter()
        {
            IgnorePaging = false;
            DefaultLimit = int.MaxValue;
        }

        /// <summary>
        /// Generate the paged result and add pagination links to the response headers
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuted(HttpActionExecutedContext context)
        {
            bool isEnumerable = false;
            try
            {
                //Check for Response object NOT NULL
                if (context.Response != null)
                {
                    //Checking for not login/logout pages
                    //Check for context.response.content is not null  && is objectcontent
                    if (context.Response.Content != null && context.Response.Content is ObjectContent)
                    {
                        var objectContent = context.Response.Content as ObjectContent;
                        if (objectContent != null)
                        {
                            if (objectContent.ObjectType.IsGenericType)
                            {
                                //Check if if ObjectContent is IEnumerable OR Implements IEnumerable
                                isEnumerable = objectContent.ObjectType.Name == "IEnumerable`1" || objectContent.ObjectType.GetInterface("IEnumerable`1") != null;
                                Paging page = context.Request.GetPagingParameters(DefaultLimit);
                                //Check for Return Type
                                if (isEnumerable)
                                {
                                    IEnumerable<dynamic> model = null;
                                    context.Response.TryGetContentValue(out model);
                                    if (model != null)
                                    {
                                        if (!IgnorePaging)
                                        {
                                            context.Response.AddTotalRecordCountHeader(model.Count());
                                            //Check if Paging is done at action method or not AND if offset & limit parameters exists
                                            if (page != null)
                                            {
                                                ObjectContent pagedContent = new ObjectContent<IEnumerable<dynamic>>(model.Skip(page.Offset).Take(page.Limit), new JsonMediaTypeFormatter());
                                                IEnumerable<KeyValuePair<string, IEnumerable<string>>> responseHeaders;
                                                responseHeaders = context.Response.Content.Headers;
                                                foreach (KeyValuePair<string, IEnumerable<string>> header in responseHeaders)
                                                {
                                                    if (header.Key != "Content-Type")
                                                    pagedContent.Headers.Add(header.Key, header.Value);
                                                }
                                                context.Response.Content = pagedContent;
                                            context.Response.AddPagingHeaders(page.Offset, page.Limit, model.Count());
                                            }
                                        }
                                    }
                                }
                            }
                            else if (objectContent.ObjectType == typeof(JArray) && !IgnorePaging)
                            {
                                JArray jArrayContent = null;
                                context.Response.TryGetContentValue(out jArrayContent);

                                if (jArrayContent != null)
                                {
                                    context.Response.AddTotalRecordCountHeader(jArrayContent.Count);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            { }
            base.OnActionExecuted(context);
        }

    }
}
