// Copyright 2017-2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using Newtonsoft.Json.Linq;
using System.Net.Http.Formatting;
using System.Net.Http;
using System.Threading;
using Ellucian.Web.Http.EthosExtend;
using Ellucian.Web.Http.Utilities;
using slf4net;


namespace Ellucian.Web.Http.Filters
{
    /// <summary>
    /// Action Filter to perform Eedm processing Data Privacy and Extensibility
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class EedmResponseFilter : ActionFilterAttribute
    {

        private const string CustomMediaType = "X-Media-Type";

        public override Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            var logger = actionExecutedContext.Request.GetDependencyScope().GetService(typeof(ILogger)) as ILogger;

            //Check if Response, Content are NOT NULL and that the content is ObjectContent
            if (actionExecutedContext.Response != null && actionExecutedContext.Response.Content != null && actionExecutedContext.Response.Content is ObjectContent)
            {
                //only process successful responses that are a put, post or get request
                if (actionExecutedContext.Response.IsSuccessStatusCode && (actionExecutedContext.Request.Method == HttpMethod.Put || actionExecutedContext.Request.Method == HttpMethod.Get || actionExecutedContext.Request.Method == HttpMethod.Post))
                {
                    //get objectContent and find out if it is an collection
                    var objectContent = actionExecutedContext.Response.Content as ObjectContent;
                    var isEnumerable = objectContent.ObjectType.Name == "IEnumerable`1" || objectContent.ObjectType.GetInterface("IEnumerable`1") != null;

                    //read json data out
                    var responseMessage = actionExecutedContext.Response.Content.ReadAsStringAsync().Result;

                    //set variable to get X-Media-Type value
                    IEnumerable<string> customMediaTypeValue = null;

                    //get the extended data froim the request context
                    object extendedData;
                    actionExecutedContext.Request.Properties.TryGetValue("ExtendedData", out extendedData);

                    var extendedDataList = (IList<EthosExtensibleData>)extendedData;

                    //get the dataprivacylist from the request context
                    object dataPrivacy;
                    actionExecutedContext.Request.Properties.TryGetValue("DataPrivacy", out dataPrivacy);
                    
                    var dataPrivacyList = (IList<string>)dataPrivacy;

                    //if there is nothing to process for DataPrivacy or Extensibility, just return to base.
                    if ((dataPrivacyList == null || !dataPrivacyList.Any()) && (extendedDataList == null || !extendedDataList.Any()))
                    {
                        return base.OnActionExecutedAsync(actionExecutedContext, cancellationToken);
                    }

                    //take the content string and parse it based on if it is a collection or single
                    JContainer jsonToModify;
                    try
                    {
                        if (isEnumerable)
                        {
                            jsonToModify = JArray.Parse(responseMessage);
                        }
                        else
                        {
                            jsonToModify = JObject.Parse(responseMessage);
                        }
                    }
                    catch
                    {
                        //return empty object if the parsing of the json fails
                        //throw new ArgumentNullException("Response.Content","The JSON Response is empty or null upon return from GET operation. ");
                       
                        logger.Error("The JSON Response is empty or null upon return from GET operation.");
                        return base.OnActionExecutedAsync(actionExecutedContext, cancellationToken);
                    }

                    var extendedReturn = Extensibility.ApplyExtensibility(jsonToModify, extendedDataList, logger);

                    bool extensionsApplied = false;
                    if (extendedReturn != null)
                    {
                        jsonToModify = extendedReturn;
                        extensionsApplied = true;
                    }

                    //if this is a post and not a QAPI return now
                    if (actionExecutedContext.Request.Method == HttpMethod.Post && !IsThisQueryByPost(actionExecutedContext))
                    {
                        //if extensions were applied set content first
                        if (extensionsApplied)
                        {
                            if (isEnumerable)
                            {
                                actionExecutedContext.Response.Content = new ObjectContent<IEnumerable<dynamic>>(jsonToModify, new JsonMediaTypeFormatter(), @"application/json");
                            }
                            else
                            {
                                actionExecutedContext.Response.Content = new ObjectContent<dynamic>(jsonToModify, new JsonMediaTypeFormatter(), @"application/json");
                            }
                        }

                        var requestedContentTypeval = actionExecutedContext.ActionContext.RequestContext.RouteData.Values["RequestedContentType"];
                        if ((requestedContentTypeval != null) && (!actionExecutedContext.Response.Content.Headers.TryGetValues(CustomMediaType, out customMediaTypeValue)))
                            actionExecutedContext.Response.Content.Headers.Add(CustomMediaType, requestedContentTypeval.ToString());

                        return base.OnActionExecutedAsync(actionExecutedContext, cancellationToken);
                    }

                    //this is a put or get so dataprivacy must be applied
                    var returnedJson = DataPrivacy.ApplyDataPrivacy(jsonToModify, dataPrivacyList, logger);
                    if (returnedJson != null)
                    {
                        if (isEnumerable)
                        {
                            actionExecutedContext.Response.Content = new ObjectContent<IEnumerable<dynamic>>(returnedJson, new JsonMediaTypeFormatter(), @"application/json");
                        }
                        else
                        {
                            actionExecutedContext.Response.Content = new ObjectContent<dynamic>(returnedJson, new JsonMediaTypeFormatter(), @"application/json");
                        }
                        if (!actionExecutedContext.Response.Headers.Contains("X-Content-Restricted"))
                            actionExecutedContext.Response.Headers.Add("X-Content-Restricted", "partial");
                    }
                    else if (extensionsApplied)//check if extensions were applied and return the modified content even if dataprivacy is not applied
                    {
                        if (isEnumerable)
                        {
                            actionExecutedContext.Response.Content = new ObjectContent<IEnumerable<dynamic>>(jsonToModify, new JsonMediaTypeFormatter(), @"application/json");
                        }
                        else
                        {
                            actionExecutedContext.Response.Content = new ObjectContent<dynamic>(jsonToModify, new JsonMediaTypeFormatter(), @"application/json");
                        }
                    }

                    var RequestedContentTypeval = actionExecutedContext.ActionContext.RequestContext.RouteData.Values["RequestedContentType"];
                    if ((RequestedContentTypeval != null) && (!actionExecutedContext.Response.Content.Headers.TryGetValues(CustomMediaType, out customMediaTypeValue)))
                        actionExecutedContext.Response.Content.Headers.Add(CustomMediaType, RequestedContentTypeval.ToString());
                }
            }
            return base.OnActionExecutedAsync(actionExecutedContext, cancellationToken);
        }

        private bool IsThisQueryByPost(HttpActionExecutedContext actionExecutedContext)
        {
            var routeData = actionExecutedContext.Request.GetRouteData();

            if (routeData == null)
            {
                return false;
            }

            string routeTemplate = string.Empty;
            if (routeData.Route != null)
            {
                routeTemplate = routeData.Route.RouteTemplate;
            }

            if (string.IsNullOrEmpty(routeTemplate)) return false;

            return routeTemplate.Contains("qapi/");
        }
    }
}
