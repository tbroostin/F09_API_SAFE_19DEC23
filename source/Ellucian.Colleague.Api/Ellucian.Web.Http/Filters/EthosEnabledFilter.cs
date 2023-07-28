// Copyright 2022 Ellucian Company L.P. and its affiliates.

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
using System.Net.Http.Headers;
using System.Text;
using Ellucian.Web.Http.Routes;
using Microsoft.Practices.ObjectBuilder2;
using System.Web.Http.Controllers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Ellucian.Web.Http.Filters
{
    /// <summary>
    /// Action Filter to add Ethos Functionality such as Data Privacy, Extended Data,
    /// v2 error messaging, and standard header values in response.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class EthosEnabledFilter : ActionFilterAttribute
    {
        private const string CustomMediaType = "X-Media-Type";
        private const string RestrictedHeaderName = "X-Content-Restricted";

        public const string IntegrationErrors1 = "application/vnd.hedtech.integration.errors.v1+json";
        public const string IntegrationErrors2 = "application/vnd.hedtech.integration.errors.v2+json";

        /// <summary>
        /// Type of the Service interface.  The service must have access to the configuration repository.
        /// </summary>
        public Type _baseServiceType { get; private set; }

        /// <summary>
        /// Boolean flag to support conversion of JSON objects into camelCase instead of defaulting to
        /// the casing used in the DTO.
        /// </summary>
        public bool _useCamelCase { get; private set; }

        /// <summary>
        /// Initialize the filter to set the base service type and convert to camelCase (default to true).
        /// </summary>
        /// <param name="baseServiceType">Use the Service Type with access to Configuration Repository.
        /// If your service doesn't have access to the Configuration Repository, you may need to add it
        /// for PUT and POST to work properly.  Otherwise, you can use typeof(IEthosApiBuilderService).</param>
        /// <param name="useCamelCase">Set to true (default value) to serialize JSON using camelCase, set
        /// to false to use the casing as defined by the DTO.</param>
        public EthosEnabledFilter(Type baseServiceType, bool useCamelCase = true)
        {
            _baseServiceType = baseServiceType;
            _useCamelCase = useCamelCase;
        }
        
        /// <summary>
         /// Initialize the filter to set the base service type and convert to camelCase (default to true).
         /// </summary>
         /// <param name="useCamelCase">Set to true (default value) to serialize JSON using camelCase, set
         /// to false to use the casing as defined by the DTO.</param>
        public EthosEnabledFilter(bool useCamelCase = true)
        {
            _useCamelCase = useCamelCase;
        }

        /// <summary>
        /// Execute Ethos standard for a self-service API that is Ethos Enabled
        /// </summary>
        /// <param name="actionExecutedContext"></param>
        public override async Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            //Check if Response, Content are NOT NULL and that the content is ObjectContent
            if (actionExecutedContext.Response != null && actionExecutedContext.Response.Content != null)
            {
                bool isEthosEnabled = false;
                if (actionExecutedContext.Request.GetRouteData().Route.Defaults.ContainsKey("isEthosEnabled"))
                {
                    isEthosEnabled = (bool)actionExecutedContext.Request.GetRouteData().Route.Defaults["isEthosEnabled"];
                }
                if (isEthosEnabled)
                {
                    var dependencyScope = actionExecutedContext.Request.GetDependencyScope();
                    var logger = dependencyScope.GetService(typeof(ILogger)) as ILogger;
                    if (actionExecutedContext.Response.IsSuccessStatusCode)
                    {
                        if (_useCamelCase)
                        {
                            ConvertToCamelCase(actionExecutedContext, cancellationToken, logger);
                        }
                        if (_baseServiceType != null)
                        {
                            var resourceRouteInfo = GetEthosResourceRouteInfo(actionExecutedContext.ActionContext);
                            var baseServiceMethods = dependencyScope.GetService(_baseServiceType);
                            var bypassCache = resourceRouteInfo.BypassCache;
                            var GetDataPrivacyListByApiMethod = baseServiceMethods.GetType().GetMethod("GetDataPrivacyListByApi", new Type[] { resourceRouteInfo.GetType(), typeof(bool) });
                            if (GetDataPrivacyListByApiMethod != null)
                            {
                                var dpParameters = new Object[] { resourceRouteInfo, bypassCache };
                                var dataPrivacyList = await (Task<IEnumerable<string>>)GetDataPrivacyListByApiMethod.Invoke(baseServiceMethods, dpParameters);

                                var GetExtendedEthosDataByResourceMethod = baseServiceMethods.GetType().GetMethod("GetExtendedEthosDataByResource");
                                if (GetExtendedEthosDataByResourceMethod != null)
                                {
                                    try
                                    {
                                        //read json data out
                                        string responseMessage = actionExecutedContext.Response.Content.ReadAsStringAsync().Result;
                                        JContainer jsonToSearch;
                                        jsonToSearch = (JContainer)JToken.Parse(responseMessage);
                                        var ids = ExtractIdsFromJsonObject(jsonToSearch);
                                        if (ids != null && ids.Any())
                                        {
                                            var extendParameters = new Object[] { resourceRouteInfo, ids, bypassCache, true };
                                            var extendedData = await (Task<IList<EthosExtensibleData>>)GetExtendedEthosDataByResourceMethod.Invoke(baseServiceMethods, extendParameters);

                                            if (actionExecutedContext != null && actionExecutedContext.Request != null && actionExecutedContext.Request.Properties != null)
                                            {
                                                //remove key item first to ensure it isn't added a second time, different things in the put cycle need to add it earlier
                                                actionExecutedContext.Request.Properties.Remove("DataPrivacy");
                                                actionExecutedContext.Request.Properties.Add("DataPrivacy", dataPrivacyList);

                                                //remove key item first to ensure it isn't added a second time, different things in the put cycle need to add it earlier
                                                actionExecutedContext.Request.Properties.Remove("ExtendedData");
                                                actionExecutedContext.Request.Properties.Add("ExtendedData", extendedData);
                                            }
                                        }
                                    }
                                    catch
                                    {
                                        logger.Error("The JSON Response is empty or null upon return from GET operation.");
                                    }
                                }
                            }
                            // Set Content Restricted Header if required.
                            var GetSecureDataDefinition = baseServiceMethods.GetType().GetMethod("GetSecureDataDefinition");
                            if (GetSecureDataDefinition != null)
                            {
                                var sdParameters = new Object[] { };
                                var secureDataDefinition = (Tuple<bool, List<string>, List<string>>)GetSecureDataDefinition.Invoke(baseServiceMethods, sdParameters);

                                if (secureDataDefinition != null && secureDataDefinition.Item1)
                                {
                                    if (!actionExecutedContext.Response.Headers.Contains(RestrictedHeaderName))
                                        actionExecutedContext.Response.Headers.Add(RestrictedHeaderName, "partial");
                                }
                            }
                        }
                        var addFilter = new EedmResponseFilter();
                        await addFilter.OnActionExecutedAsync(actionExecutedContext, cancellationToken);
                    }
                    else
                    {
                        //read json data out
                        string responseMessage = actionExecutedContext.Response.Content.ReadAsStringAsync().Result;
                        int status = (int)actionExecutedContext.Response.StatusCode;

                        //take the content string and parse it
                        JContainer jsonToModify;
                        bool useVersion2Errors = false;
                        try
                        {
                            jsonToModify = JObject.Parse(responseMessage);
                            var modifiedMessage = ErrorMessageHelper.ConvertToV2ErrorMessage(jsonToModify, status, logger);
                            if (modifiedMessage != null)
                            {
                                jsonToModify = modifiedMessage;
                                useVersion2Errors = true;
                            }
                            actionExecutedContext.Response.Content = new ObjectContent<dynamic>(jsonToModify, new JsonMediaTypeFormatter(), @"application/json");
                        }
                        catch
                        {
                            logger.Error("The JSON Response is empty or null upon return from GET operation.");
                        }

                        // Update the custom media type in the response headers
                        actionExecutedContext.Response.Content.Headers.Remove(CustomMediaType);

                        IEnumerable<string> customMediaTypeValue = null;
                        if (!actionExecutedContext.Response.Content.Headers.TryGetValues(CustomMediaType, out customMediaTypeValue))
                        {
                            if (useVersion2Errors)
                                actionExecutedContext.Response.Content.Headers.Add(CustomMediaType, IntegrationErrors2);
                            else
                                actionExecutedContext.Response.Content.Headers.Add(CustomMediaType, IntegrationErrors1);
                        }

                        // Update the content type in the response headers
                        actionExecutedContext.Response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json")
                        {
                            CharSet = Encoding.UTF8.WebName
                        };
                        IEnumerable<string> contentTypeValue = null;
                        if (!actionExecutedContext.Response.Content.Headers.TryGetValues("Content-Type", out contentTypeValue))
                            actionExecutedContext.Response.Content.Headers.Add("Content-Type", "application/json;charset=UTF-8");
                    }
                }
            }

            // Execute Base OnActionExecutedAsync to continue with other filters.
            await base.OnActionExecutedAsync(actionExecutedContext, cancellationToken);
        }

        /// <summary>
        /// Execute Ethos standard for a self-service API that is Ethos Enabled (Before results)
        /// </summary>
        /// <param name="actionExecutedContext"></param>
        public override async Task OnActionExecutingAsync(HttpActionContext actionExecutedContext, CancellationToken cancellationToken)
        {
            //Check if Response, Content are NOT NULL and that the content is ObjectContent
            if (actionExecutedContext.Request != null && actionExecutedContext.Request.Content != null)
            {
                bool isEthosEnabled = false;
                if (actionExecutedContext.Request.GetRouteData().Route.Defaults.ContainsKey("isEthosEnabled"))
                {
                    isEthosEnabled = (bool)actionExecutedContext.Request.GetRouteData().Route.Defaults["isEthosEnabled"];
                }
                if (isEthosEnabled)
                {
                    var dependencyScope = actionExecutedContext.Request.GetDependencyScope();
                    var logger = dependencyScope.GetService(typeof(ILogger)) as ILogger;
                    if (actionExecutedContext.Request.Method == HttpMethod.Put || actionExecutedContext.Request.Method == HttpMethod.Post)
                    {
                        if (_baseServiceType != null)
                        {
                            try
                            {
                                var resourceRouteInfo = GetEthosResourceRouteInfo(actionExecutedContext);
                                var baseServiceMethods = dependencyScope.GetService(_baseServiceType);
                                var bypassCache = resourceRouteInfo.BypassCache;

                                var GetExtendedEthosConfigurationByResource = baseServiceMethods.GetType().GetMethod("GetExtendedEthosConfigurationByResource");
                                if (GetExtendedEthosConfigurationByResource != null)
                                {
                                    var edParameters = new Object[] { resourceRouteInfo, bypassCache };
                                    var ethosExtensibleDataConfig = await (Task<EthosExtensibleData>)GetExtendedEthosConfigurationByResource.Invoke(baseServiceMethods, edParameters);

                                    if (ethosExtensibleDataConfig != null)
                                    {
                                        var extendedData = ExtractExtendedData(actionExecutedContext, ethosExtensibleDataConfig, logger);
                                        if (extendedData != null && extendedData.Any())
                                        {
                                            //add this property for Extended Data dictionary used on PUT and POST requests
                                            actionExecutedContext.Request.Properties.Remove("EthosExtendedDataDictionary");
                                            actionExecutedContext.Request.Properties.Add("EthosExtendedDataDictionary", extendedData as Object);
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                var messageString = "{\"message\":\"" + ex.Message + "\"}";
                                var jsonToModify = JObject.Parse(messageString);
                                int status = 400;
                                var modifiedMessage = ErrorMessageHelper.ConvertToV2ErrorMessage(jsonToModify, status, logger);
                                if (modifiedMessage != null)
                                {
                                    jsonToModify = (JObject)modifiedMessage;
                                }
                                var serialized = JsonConvert.SerializeObject(jsonToModify);
                                actionExecutedContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest) { Content = new StringContent(serialized) };
                                actionExecutedContext.Response.Headers.Add(CustomMediaType, IntegrationErrors2);

                                // Update the content type in the response headers
                                actionExecutedContext.Response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json")
                                {
                                    CharSet = Encoding.UTF8.WebName
                                };
                                IEnumerable<string> contentTypeValue = null;
                                if (!actionExecutedContext.Response.Content.Headers.TryGetValues("Content-Type", out contentTypeValue))
                                    actionExecutedContext.Response.Content.Headers.Add("Content-Type", "application/json;charset=UTF-8");
                            }
                        }
                    }
                }
            }
            await base.OnActionExecutingAsync(actionExecutedContext, cancellationToken);
        }

        /// <summary>
        /// Gets Ethos Resource information from route
        /// </summary>
        /// <returns>EthosResourceRouteInfo</returns>
        private EthosResourceRouteInfo GetEthosResourceRouteInfo(HttpActionContext controllerContext)
        {
            var bypassCache = false;
            if (controllerContext.Request != null && controllerContext.Request.Headers != null && controllerContext.Request.Headers.CacheControl != null)
            {
                if (controllerContext.Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            var ethosRouteInfo = new EthosResourceRouteInfo();

            var actionRequestContext = controllerContext.Request;

            if (actionRequestContext == null)
            {
                return ethosRouteInfo;
            }

            var routeData = actionRequestContext.GetRouteData();

            if (routeData == null)
            {
                return ethosRouteInfo;
            }


            if (routeData.Route == null) return ethosRouteInfo;

            var routeTemplate = routeData.Route.RouteTemplate;


            string[] routeStrings = routeTemplate.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (routeStrings.Any())
            {
                var count = routeStrings.Count();
                if (count == 1)
                {
                    ethosRouteInfo.ResourceName = routeStrings[0];
                }

                ethosRouteInfo.ResourceName = routeStrings[0].Equals("qapi", StringComparison.OrdinalIgnoreCase)
                    ? routeStrings[1]
                    : routeStrings[0];
            }

            object contentTypeObj;
            routeData.Values.TryGetValue("RequestedContentType", out contentTypeObj);

            if (contentTypeObj != null)
            {
                ethosRouteInfo.EthosResourceIdentifier = contentTypeObj.ToString();
            }

            object headerVersionObj;
            routeData.Route.Constraints.TryGetValue("headerVersion", out headerVersionObj);

            if (headerVersionObj != null)
            {
                ethosRouteInfo.ResourceVersionNumber = ((HeaderVersionConstraint)headerVersionObj).RouteVersion.ToString();
            }

            // Update global bypassCache flag for configuration cache
            ethosRouteInfo.BypassCache = bypassCache;

            return ethosRouteInfo;
        }

        /// <summary>
        /// Extract keys from Json Object
        /// </summary>
        /// <param name="JsonToSearch">json body to extend</param>
        /// <param name="logger">logger</param>
        /// <returns></returns>
        private static List<string> ExtractIdsFromJsonObject(JContainer JsonToSearch)
        {
            List<string> ids = new List<string>();
            try
            {
                if (JsonToSearch.Type == JTokenType.Array)
                {
                    var jArray = new JArray();

                    JsonToSearch.ForEach(j =>
                    {
                        var id = (string)j.SelectToken("$._id", false);
                        if (string.IsNullOrEmpty(id)) id = (string)j.SelectToken("$.id", false);
                        if (string.IsNullOrEmpty(id)) id = (string)j.SelectToken("$.Id", false);
                        if (string.IsNullOrEmpty(id)) id = (string)j.SelectToken("$.Code", false);
                        if (string.IsNullOrEmpty(id)) id = (string)j.SelectToken("$.code", false);
                        if (!string.IsNullOrEmpty(id))
                        {
                            ids.Add(id);
                        }
                    });
                }
                else if (JsonToSearch.Type == JTokenType.Object)
                {
                    var id = (string)JsonToSearch.SelectToken("$._id", false);
                    if (string.IsNullOrEmpty(id)) id = (string)JsonToSearch.SelectToken("$.id", false);
                    if (string.IsNullOrEmpty(id)) id = (string)JsonToSearch.SelectToken("$.Id", false);
                    if (string.IsNullOrEmpty(id)) id = (string)JsonToSearch.SelectToken("$.Code", false);
                    if (string.IsNullOrEmpty(id)) id = (string)JsonToSearch.SelectToken("$.code", false);
                    if (!string.IsNullOrEmpty(id))
                    {
                        ids.Add(id);
                    }
                }
            }
            catch (Exception)
            {
                return ids;
            }

            return ids;
        }

        private Dictionary<string, string> ExtractExtendedData(HttpActionContext actionExecutedContext, EthosExtensibleData extensibleDataDefinitions, ILogger logger)
        {
            if (extensibleDataDefinitions == null)
            {
                return new Dictionary<string, string>();
            }

            var bodyString = actionExecutedContext.Request.Content.ReadAsStringAsync().Result;
            if (string.IsNullOrEmpty(bodyString))
            {
                try
                {
                    object extendedData;
                    actionExecutedContext.Request.Properties.TryGetValue("EthosExtendedDataObject", out extendedData);
                    if (extendedData != null)
                    {
                        return Extensibility.ExtractExtendedEthosData((JObject)extendedData, extensibleDataDefinitions, logger);
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e, "Getting Extended Ethos Data body failed. EthosExtendedData request property is not set.");
                    return new Dictionary<string, string>();
                }
            }
            else
            {
                try
                {
                    JObject extendedData = JObject.Parse(bodyString);
                    if (extendedData != null)
                    {
                        return Extensibility.ExtractExtendedEthosData(extendedData, extensibleDataDefinitions, logger);
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e, "Extracting Extended Ethos Data properties failed.");
                }
            }

            return new Dictionary<string, string>();
        }

        private void ConvertToCamelCase(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken, ILogger logger)
        {
            //Check if Response, Content are NOT NULL and that the content is ObjectContent
            if (actionExecutedContext.Response != null && actionExecutedContext.Response.Content != null && actionExecutedContext.Response.Content is ObjectContent)
            {
                //only process successful responses that are a put, post or get request
                if (actionExecutedContext.Response.IsSuccessStatusCode && (actionExecutedContext.Request.Method == HttpMethod.Put || actionExecutedContext.Request.Method == HttpMethod.Get || actionExecutedContext.Request.Method == HttpMethod.Post))
                {
                    //get objectContent and find out if it is an collection
                    var objectContent = actionExecutedContext.Response.Content as ObjectContent;
                    var isEnumerable = objectContent.ObjectType.Name == "IEnumerable`1" || objectContent.ObjectType.GetInterface("IEnumerable`1") != null;

                    if (objectContent.Value != null)
                    {
                        JsonSerializerSettings jsonSettings = new JsonSerializerSettings()
                        {
                            ContractResolver = new CamelCasePropertyNamesContractResolver(),
                            NullValueHandling = NullValueHandling.Ignore
                        };
                        var responseMessage = JsonConvert.SerializeObject(objectContent.Value, jsonSettings);

                        //take the content string and parse it based on if it is a collection or single
                        JContainer jsonToModify;
                        try
                        {
                            if (isEnumerable)
                            {
                                jsonToModify = JArray.Parse(responseMessage);
                                actionExecutedContext.Response.Content = new ObjectContent<IEnumerable<dynamic>>(jsonToModify, new JsonMediaTypeFormatter(), @"application/json");
                            }
                            else
                            {
                                jsonToModify = JObject.Parse(responseMessage);
                                actionExecutedContext.Response.Content = new ObjectContent<dynamic>(jsonToModify, new JsonMediaTypeFormatter(), @"application/json");
                            }
                        }
                        catch
                        {
                            logger.Error("The JSON Response is empty or null upon return from GET operation.");
                        }
                    }
                }
            }
        }
    }
}