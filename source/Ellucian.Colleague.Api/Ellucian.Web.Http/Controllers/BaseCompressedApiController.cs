// Copyright 2012-2021 Ellucian Company L.P. and its affiliates.

using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Utilities;
using Ellucian.Web.Mvc.Filter;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.SessionState;
using Ellucian.Web.Http.EthosExtend;
using Ellucian.Web.Http.Routes;
using Ellucian.Colleague.Dtos;
using System.Collections;

namespace Ellucian.Web.Http.Controllers
{
    [SessionState(SessionStateBehavior.Disabled)]
    [BrowserCompressionFilter]
    [CustomCredentialsResponseFilter]
    public abstract class BaseCompressedApiController : ApiController
    {
        private const string RestrictedHeaderName = "X-Content-Restricted";

        public const string IntegrationErrors1 = "application/vnd.hedtech.integration.errors.v1+json";
        public const string IntegrationErrors2 = "application/vnd.hedtech.integration.errors.v2+json";
        public const string IntegrationCustomMediaType = "application/vnd.hedtech.integration.{0}.v{1}+json";


        public BaseCompressedApiController()
        {
            var controllerType = this.GetType();
            System.ComponentModel.LicenseManager.Validate(controllerType);
        }

        /// <summary>
        /// GetPermissionsMetaData associated with controller method.
        /// </summary>
        /// <returns>Tuple consisting of:
        /// 1. string[]: array of valid permissions
        /// 2. string: http method (ex: "GET")
        /// 3. string: resource name (ex: 'person-holds')
        /// </returns>
        public Tuple<string[], string, string> GetPermissionsMetaData()
        {
            string[] permissionsCollection = null;
            if (ActionContext == null || ActionContext.Request == null)
            {
                return null;
            }

            var routeTemplate = string.Empty;

            var method = ActionContext.Request.Method == null
                ? null : ActionContext.Request.Method.ToString();
            var routeData = ActionContext.Request.GetRouteData();
            if (routeData != null && routeData.Route != null)
            {
                routeTemplate = routeData.Route.RouteTemplate;
                // remove {id},{guid},{qapi} from the template name
                if (routeTemplate.Contains("/"))
                {
                    try
                    {
                        var aRouteTemplate = routeTemplate.Split('/');
                        if ((routeTemplate.Contains("qapi")) && (aRouteTemplate.Length > 0))
                            routeTemplate = aRouteTemplate[1];
                        else
                            routeTemplate = aRouteTemplate[0];
                    }
                    catch (Exception)
                    {
                        // do nothing.. and continue to use the existing routeTemplate name
                    }
                }
            }

            object filterObject;
            ActionContext.Request.Properties.TryGetValue("PermissionsFilter", out filterObject);
            if (filterObject != null)
            {
                permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();
            }
            return new Tuple<string[], string, string>(permissionsCollection, method, routeTemplate);
        }

        /// <summary>
        /// Creates an HttpResponseException with an optional message.
        /// </summary>
        /// <param name="message">Optional message to be returned with the exception e.g. update failed.</param>
        /// <param name="statusCode">HttpStatusCode to be returned. Default is 400 (bad request)</param>
        /// <returns>HttpResponseException</returns>
        protected HttpResponseException CreateHttpResponseException(string message = null, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            if (!string.IsNullOrEmpty(message))
            {
                message = message.Replace(Environment.NewLine, " ").Replace("\n", " ");
            }
            return CreateHttpResponseException(new WebApiException() { Message = message ?? string.Empty }, statusCode);
        }

        /// <summary>
        /// Creates an HttpResponseException using the supplied WebApiException.
        /// </summary>
        /// <param name="apiException">WebApiException object detailing what went wrong.</param>
        /// <param name="statusCode">HttpStatusCode to be returned. Default is 400 (bad request)</param>
        /// <returns>HttpResponseException</returns>
        protected HttpResponseException CreateHttpResponseException(WebApiException apiException, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            var httpResponseMessage = new HttpResponseMessage(statusCode);

            if (apiException == null)
            {
                apiException = new WebApiException() { Message = "..." };
            }

            if (!string.IsNullOrEmpty(apiException.Message))
            {
                apiException.Message = apiException.Message.Replace(Environment.NewLine, " ").Replace("\n", " ");
            }

            var serialized = JsonConvert.SerializeObject(apiException);
            httpResponseMessage.Content = new StringContent(serialized);

            return new HttpResponseException(httpResponseMessage);
        }

        /// <summary>
        /// Creates a 404 error with a message.
        /// </summary>
        /// <param name="type">A word describing the object in question, e.g. "course", "book", etc.</param>
        /// <param name="id">The id of the object that was not found.</param>
        /// <returns>HttpResponseException</returns>
        protected HttpResponseException CreateNotFoundException(string type, string id)
        {
            return CreateHttpResponseException(string.Format("The {0} with the ID {1} was not found", type, id), HttpStatusCode.NotFound);
        }

        /// <summary>
        /// Creates an HttpResponseException using the supplied IntegrationApiException
        /// </summary>
        /// <param name="apiException">IntegrationApiException object detailing what went wrong.</param>
        /// <param name="statusCode">HttpStatusCode to be returned. Default is 400 (bad request)</param>
        /// <returns>HttpResponseException</returns>
        protected HttpResponseException CreateHttpResponseException(IntegrationApiException apiException, HttpStatusCode? statusCode = null)
        {
            if (!statusCode.HasValue)
            {
                statusCode = apiException.HttpStatusCode;
            }
            if (HttpStatusCode.IsDefined(typeof(HttpStatusCode), statusCode))
            {
                statusCode = (HttpStatusCode)HttpStatusCode.ToObject(typeof(HttpStatusCode), statusCode);
            }
            else
            {
                statusCode = HttpStatusCode.BadRequest;
            }
            var httpResponseMessage = new HttpResponseMessage(statusCode.Value);

            var serialized = JsonConvert.SerializeObject(apiException);
            httpResponseMessage.Content = new StringContent(serialized);

            return new HttpResponseException(httpResponseMessage);
        }


        /// <summary>
        /// Throw NotAcceptable exception
        /// </summary>
        [System.Web.Http.HttpGet, System.Web.Http.HttpPut, System.Web.Http.HttpPost]
        public void NotAcceptableStatusException()
        {


            throw CreateHttpResponseException(new IntegrationApiException("",
                new IntegrationApiError("Global.Internal.Error", "Unspecified Error on the system which prevented execution.", "The requested version is not supported.", HttpStatusCode.NotAcceptable)));

        }

        /// <summary>
        /// Throw UnsupportedMediaType exception
        /// </summary>
        [System.Web.Http.HttpGet, System.Web.Http.HttpPut, System.Web.Http.HttpPost]
        public void UnsupportedMediaTypeException()
        {
            throw CreateHttpResponseException(new IntegrationApiException("",
                new IntegrationApiError("Global.Internal.Error", "Unspecified Error on the system which prevented execution.", "The requested media type is not supported.", HttpStatusCode.UnsupportedMediaType)));

        }

        /// <summary>
        /// Throw MethodNotAllowed exception
        /// </summary>
        [System.Web.Http.HttpGet, System.Web.Http.HttpPut, System.Web.Http.HttpPost]
        public void MethodNotAllowedException()
        {
            throw CreateHttpResponseException(new IntegrationApiException("",
              new IntegrationApiError("Global.Internal.Error", "Unspecified Error on the system which prevented execution.", "The method is not supported by the resource.", HttpStatusCode.MethodNotAllowed)));
        }

        /// <summary>
        /// Sets the Location header on the HTTP response.
        /// </summary>
        /// <param name="routeName">Route name as defined in the route table.</param>
        /// <param name="routeValues">Route values needed by the route template.</param>
        protected void SetResourceLocationHeader(string routeName, object routeValues = null)
        {
            string location = Url.Link(routeName, routeValues);
            HttpContext.Current.Response.AppendHeader(HttpResponseHeader.Location.ToString(), location);
        }

        /// <summary>
        /// Sets the Content Restricted header on the HTTP response
        /// </summary>
        /// <param name="restriction">The restriction value</param>
        protected void SetContentRestrictedHeader(string restriction)
        {
            HttpContext.Current.Response.AppendHeader(RestrictedHeaderName, restriction);
        }

        /// <summary>
        /// Gets value from a JObject in the criteria
        /// </summary>
        /// <param name="keyToSearch"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public string GetValueFromJsonObject(string keyToSearch, JObject obj)
        {
            string outValue = string.Empty;
            var token = obj.GetValue(keyToSearch, StringComparison.OrdinalIgnoreCase);
            if (token != null)
            {
                outValue = token.ToString();
            }

            return outValue;
        }

        /// <summary>
        /// Gets value from a JObject in the criteria using SelectToken
        /// </summary>
        /// <param name="keyToSearch"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public string GetValueFromJsonObjectToken(string keyToSearch, JObject obj)
        {
            string outValue = string.Empty;
            var token = obj.SelectToken(keyToSearch);
            if (token != null)
            {
                outValue = token.ToString();
            }

            return outValue;
        }

        /// <summary>
        /// Gets value from a JObject in the criteria using SelectToken
        /// </summary>
        /// <param name="keyToSearch"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public List<string> GetArrayFromJsonObjectToken(string arrayToSearch, JObject obj, string keyToSearch = "")
        {
            List<string> outValue = null;
            try
            {
                var token = obj.SelectToken(arrayToSearch);
                if ((token != null) && (token.Type == JTokenType.Array))
                {
                    outValue = new List<string>();
                    foreach (var t in token.ToArray())
                    {
                        if (!string.IsNullOrEmpty(keyToSearch))
                        {
                            string strValue = GetValueFromJsonObjectToken(keyToSearch, JObject.Parse(t.ToString()));
                            if (!string.IsNullOrEmpty(strValue))
                            {
                                outValue.Add(strValue);
                            }
                        }
                        else
                        {
                            outValue.Add(t.ToString());
                        }
                    }
                }
            }
            catch
            {
                // return empty
            }
            return outValue;
        }

        /// <summary>
        /// Gets value from a JObject in the criteria using SelectToken
        /// </summary>
        /// <param name="type"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public List<Tuple<string, string>> GetTupleFromJsonObjectToken(string arrayToSearch, JObject obj, string type = "", string value = "")
        {
            List<Tuple<string, string>> outValueList = new List<Tuple<string, string>>();
            try
            {
                var token = obj.SelectToken(arrayToSearch);
                if ((token != null) && (token.Type == JTokenType.Array))
                {
                    foreach (var t in token.ToArray())
                    {
                        if (!string.IsNullOrEmpty(type) && !string.IsNullOrEmpty(value))
                        {
                            var key = GetValueFromJsonObjectToken(type, (JObject)t);
                            var keyValue = GetValueFromJsonObjectToken(value, (JObject)t);
                            var outTuple = new Tuple<string, string>(key, keyValue);
                            outValueList.Add(outTuple);
                        }
                    }
                }
            }
            catch
            {
                return null;
            }
            return outValueList.Any() ? outValueList : null;
        }

        /// <summary>
        ///  ValidateEnum
        /// </summary>
        /// <param name="T">type</param>
        /// <param name="value">value</param>
        /// <returns></returns>
        public bool ValidEnumerationValue(Type T, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }
            foreach (var enumName in Enum.GetNames(T))
            {
                var fieldInfo = T.GetField(enumName);

                if (fieldInfo != null)
                {
                    if ((!string.IsNullOrEmpty(fieldInfo.Name)) && (fieldInfo.Name == value))
                    {
                        return true;
                    }
                    var enumMemberAttribute = ((EnumMemberAttribute[])fieldInfo.GetCustomAttributes(typeof(EnumMemberAttribute), true)).FirstOrDefault();
                    if (enumMemberAttribute != null && enumMemberAttribute.Value == value)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Converts filter string value to enum value.
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="value"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public TEnum GetEnumFromEnumMemberAttribute<TEnum>(string value, TEnum defaultValue) where TEnum : struct
        {
            if (string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }

            TEnum result;
            var enumType = typeof(TEnum);
            foreach (var enumName in Enum.GetNames(enumType))
            {
                var fieldInfo = enumType.GetField(enumName);
                var enumMemberAttribute = ((EnumMemberAttribute[])fieldInfo.GetCustomAttributes(typeof(EnumMemberAttribute), true)).FirstOrDefault();
                if (enumMemberAttribute != null && enumMemberAttribute.Value == value)
                {
                    return Enum.TryParse(enumName, true, out result) ? result : defaultValue;
                }
            }
            //return Enum.TryParse(value, true, out result) ? result : defaultValue;
            return defaultValue;
        }

        /// <summary>
        /// Gets the resource name from the route
        /// </summary>
        /// <returns>resource name from the route</returns>
        public virtual string GetRouteResourceName()
        {
            var actionRequestContext = ActionContext.Request;

            if (actionRequestContext == null)
            {
                return string.Empty;
            }

            var routeData = actionRequestContext.GetRouteData();

            if (routeData == null)
            {
                return string.Empty;
            }

            string routeTemplate = string.Empty;
            if (routeData.Route != null)
            {
                routeTemplate = routeData.Route.RouteTemplate;
            }


            string[] routeStrings = routeTemplate.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (routeStrings.Any())
            {
                var count = routeStrings.Count();
                if (count == 1)
                {
                    return routeStrings[0];
                }

                if (routeStrings[0].Equals("qapi", StringComparison.OrdinalIgnoreCase))
                {
                    return routeStrings[1];
                }
                else
                {
                    return routeStrings[0];
                }
            }

            throw new Exception("Unable to get route resource name");
        }

        /// <summary>
        /// Gets Ethos Resource information from route
        /// </summary>
        /// <returns>EthosResourceRouteInfo</returns>
        public virtual EthosResourceRouteInfo GetEthosResourceRouteInfo()
        {
            var bypassCache = false;
            if (Request != null && Request.Headers != null && Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            var ethosRouteInfo = new EthosResourceRouteInfo();

            var actionRequestContext = ActionContext.Request;

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
        /// Adds the data privacy settings list to the actioncontext request properties
        /// </summary>
        /// <param name="dataPrivacySettingsList"></param>
        public void AddDataPrivacyContextProperty(List<string> dataPrivacySettingsList)
        {
            ActionContext.Request.Properties.Remove("DataPrivacy");
            ActionContext.Request.Properties.Add("DataPrivacy", dataPrivacySettingsList);
        }

        /// <summary>
        /// Adds the data privacy settings list and extended data list to the actioncontext request properties
        /// </summary>
        /// <param name="dataPrivacySettingsList"></param>
        /// <param name="extendedData"></param>
        public void AddEthosContextProperties(IEnumerable<string> dataPrivacySettingsList, IEnumerable<EthosExtensibleData> extendedData)
        {
            if (ActionContext != null && ActionContext.Request != null && ActionContext.Request.Properties != null)
            {
                //remove key item first to ensure it isn't added a second time, different things in the put cycle need to add it earlier
                ActionContext.Request.Properties.Remove("DataPrivacy");
                ActionContext.Request.Properties.Add("DataPrivacy", dataPrivacySettingsList);

                //remove key item first to ensure it isn't added a second time, different things in the put cycle need to add it earlier
                ActionContext.Request.Properties.Remove("ExtendedData");
                ActionContext.Request.Properties.Add("ExtendedData", extendedData);
            }
        }

        ///// <summary>
        ///// Check if the json content for the update contains any properties protected by data privacy settings and throw an exception if so.
        ///// </summary>
        ///// <param name="jsonUpdateBody">object being updated </param>
        ///// <param name="dataPrivacyList">list of data privacy settings </param>
        ///// <param name="logger">logger to log with</param>
        ///// <returns>false if dataprivacy is not violated, throws a permission exception if it is</returns>
        public async Task<bool> DoesUpdateViolateDataPrivacySettings(object jsonUpdateBody, IEnumerable<string> dataPrivacyList, ILogger logger)
        {
            var json = JObject.Parse(JsonConvert.SerializeObject(jsonUpdateBody));

            if (DataPrivacy.ApplyDataPrivacy(json, dataPrivacyList, logger) != null)
            {
                throw new Exception(string.Concat("Update on ", GetRouteResourceName(), " has been rejected for attempting to update restricted properties."));
            }
            return false;
        }

        /// <summary>
        /// Take a possible partial payload and merge it with the existing resource. Performs DataPrivacy check to ensure nothing coming in is protected.
        /// </summary>
        /// <typeparam name="T">Type of the DTO</typeparam>
        /// <param name="payload">incoming DTO</param>
        /// <param name="getCurrentFunction">method to get the full represenation of the DTO</param>
        /// <param name="dataPrivacyList">list of data privacy settings </param>
        /// <param name="logger">logger to log with</param>
        /// <returns></returns>
        public async Task<T> PerformPartialPayloadMerge<T>(T payload, Func<Task<T>> getCurrentFunction, IEnumerable<string> dataPrivacyList, ILogger logger)
        {
            // create object of T
            T origType;
            try
            {
                //get T using Func param
                origType = await getCurrentFunction();
            }
            catch (KeyNotFoundException)
            {
                //if KeyNotFoundException is returned this is actually a create on put, pass the payload back out without touching it
                return payload;
            }
            //any other error from the Get will be passed up, only KeyNotFounds will be caught and ignored 

            try
            {
                return await CreateMergedObject(origType, dataPrivacyList, logger);
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed attempting to perform partial payload merge");
                return payload;
            }
        }

        /// <summary>
        /// Take a possible partial payload and merge it with the existing resource. Performs DataPrivacy check to ensure nothing coming in is protected.
        /// </summary>
        /// <typeparam name="T">Type of the DTO</typeparam>
        /// <param name="payload">incoming DTO</param>
        /// <param name="origType"> the full represenation of the DTO</param>
        /// <param name="dataPrivacyList">list of data privacy settings </param>
        /// <param name="logger">logger to log with</param>
        /// <returns></returns>
        public async Task<T> PerformPartialPayloadMerge<T>(T payload, T origType, IEnumerable<string> dataPrivacyList, ILogger logger)
        {
            if (origType == null)
            {
                return payload;
            }
            try
            {
                return await CreateMergedObject(origType, dataPrivacyList, logger);
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed attempting to perform partial payload merge");
                return payload;
            }
        }


        private async Task<T> CreateMergedObject<T>(T origType, IEnumerable<string> dataPrivacyList, ILogger logger)
        {
            object updateRequest;
            var dpList = dataPrivacyList.ToList();
            //add data privacy settings into the context for put output
            AddDataPrivacyContextProperty(dpList);
            ActionContext.Request.Properties.TryGetValue("PartialInputJsonObject", out updateRequest);

            if (dpList.Any() && DataPrivacy.ApplyDataPrivacy((JObject)updateRequest, dpList, logger) != null)
            {
                throw new InvalidOperationException(string.Concat("Update on ", GetRouteResourceName(), " has been rejected for attempting to update restricted properties."));
            }
            //var updateRequestJObject = DataPrivacy.RemoveOrReplaceEmptyProperties((JObject)updateRequest, logger, true);
            //var orig = JObject.FromObject(origType);
            var orig = JObject.FromObject(origType);

            var updateRequestWithProtectedArrayItemsIncluded = DataPrivacy.ReinsertProtectedArrayItems(orig, (JObject)updateRequest, dpList, logger);

            var updateRequestJObject = DataPrivacy.RemoveOrReplaceEmptyProperties((JObject)updateRequestWithProtectedArrayItemsIncluded, logger, true);

            //json merge settings of array is to replace, new array set coming in replaces existing one
            //json merge settings of nulls is to merge, nulls coming in set exiting ones to null to unset them
            orig.Merge(updateRequestJObject,
                new JsonMergeSettings()
                {
                    MergeArrayHandling = MergeArrayHandling.Replace,
                    MergeNullValueHandling = MergeNullValueHandling.Merge
                });

            return orig.ToObject<T>();
        }

        public async Task<Dictionary<string, string>> ExtractExtendedData(EthosExtensibleData extensibleDataDefinitions, ILogger logger)
        {
            //try and get the EthosExtendedData request property to look for extnded data with, if it is not there log an error and return an empty list
            object extendedData;
            try
            {
                ActionContext.Request.Properties.TryGetValue("EthosExtendedDataObject", out extendedData);
            }
            catch (Exception e)
            {
                logger.Error(e, "Getting Extended Ethos Data body failed. EthosExtendedData request property is not set.");
                return new Dictionary<string, string>();
            }

            if (extensibleDataDefinitions == null)
            {
                return new Dictionary<string, string>();
            }

            try
            {
                return Extensibility.ExtractExtendedEthosData((JObject)extendedData, extensibleDataDefinitions, logger);
            }
            catch (FormatException e)
            {
                throw new IntegrationApiException(e.Message, new IntegrationApiError("Extract.Extended.Data.Property", e.InnerException.Message, e.Message));
            }
            catch (Exception e)
            {
                logger.Error(e, "Extracting Extended Ethos Data properties failed.");
                return new Dictionary<string, string>();
            }
        }

        /// <summary>
        /// CheckForInvalidFilterParameters
        /// </summary>
        public bool CheckForEmptyFilterParameters()
        {
            bool retval = false;

            object filterObject;
            ActionContext.Request.Properties.TryGetValue("EmptyFilterProperties", out filterObject);
            if (filterObject != null)
            {
                retval = (Convert.ToBoolean(filterObject));
            }
            return retval;
        }

        /// <summary>
        /// GetFilterObject
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public T GetFilterObject<T>(ILogger logger, string contextSuffix = "")
        {
            try
            {
                var contextPropertyName = "FilterObject";
                if (!string.IsNullOrEmpty(contextSuffix))
                    contextPropertyName = string.Format("{0}{1}", contextPropertyName, contextSuffix.Trim());

                Object filterObject;
                ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                if (filterObject != null)
                {
                    var orig = JObject.FromObject(filterObject);
                    return orig.ToObject<T>();
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed attempting to perform extract filter.");
            }

            return (T)Activator.CreateInstance(typeof(T), false);
        }

        /// <summary>
        /// Get Filter Properties.
        /// </summary>
        /// <param name="logger"></param>
        /// <returns></returns>
        public IEnumerable<Tuple<string, Colleague.Dtos.Converters.FilterConverter.FilterMemberInfo>> GetFilterProperties(ILogger logger)
        {
            try
            {
                var contextPropertyName = "FilterProperties";

                Object filterObject;
                ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                if (filterObject != null)
                {
                    var orig = (List<Tuple<string, Colleague.Dtos.Converters.FilterConverter.FilterMemberInfo>>)filterObject;
                    return orig;
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed attempting to perform extract filter.");
            }

            return null;
        }

        /// <summary>
        /// Get all filters with query qualifiers, such as GE, EQ, LT, etc.
        /// </summary>
        /// <param name="logger"></param>
        /// <returns></returns>
        public Dictionary<string, string> GetFilterQualifiers(ILogger logger)
        {
            var filterProperties = GetFilterProperties(logger);
            Dictionary<string, string> qualifiers = new Dictionary<string, string>();
            if (filterProperties != null && filterProperties.Any())
            {
                foreach (var prop in filterProperties)
                {
                    if (!qualifiers.ContainsKey(prop.Item1) && prop.Item2 != null && !string.IsNullOrEmpty(prop.Item2.FilterCriteria))
                    {
                        string filterCriteriaOperation = "";
                        switch (prop.Item2.FilterCriteria)
                        {
                            case "$eq":
                                filterCriteriaOperation = "EQ";
                                break;
                            case "$=":
                                filterCriteriaOperation = "EQ";
                                break;
                            case "$ne":
                                filterCriteriaOperation = "NE";
                                break;
                            case "$<>":
                                filterCriteriaOperation = "NE";
                                break;
                            case "$!=":
                                filterCriteriaOperation = "NE";
                                break;
                            case "$lt":
                                filterCriteriaOperation = "LT";
                                break;
                            case "$<":
                                filterCriteriaOperation = "LT";
                                break;
                            case "$gt":
                                filterCriteriaOperation = "GT";
                                break;
                            case "$>":
                                filterCriteriaOperation = "GT";
                                break;
                            case "$le":
                                filterCriteriaOperation = "LE";
                                break;
                            case "$lte":
                                filterCriteriaOperation = "LE";
                                break;
                            case "$<=":
                                filterCriteriaOperation = "LE";
                                break;
                            case "$ge":
                                filterCriteriaOperation = "GE";
                                break;
                            case "$gte":
                                filterCriteriaOperation = "GE";
                                break;
                            case "$>=":
                                filterCriteriaOperation = "GE";
                                break;
                            case "$or":
                                filterCriteriaOperation = "OR";
                                break;
                            case "$and":
                                filterCriteriaOperation = "AND";
                                break;
                            default:
                                filterCriteriaOperation = "LIKE";
                                break;
                        }
                        qualifiers.Add(prop.Item1, filterCriteriaOperation);
                    }
                }
            }
            return qualifiers;
        }

        /// <summary>
        /// Convert list of GUID object Ids to list of strings
        /// </summary>
        /// <param name="guidObjectList">Guid Object of Ids</param>
        /// <returns>List of strings</returns>

        public List<string> ConvertGuidObject2ListToStringList(List<GuidObject2> guidObjectList)
        {
            var retval = new List<string>();
            if (guidObjectList != null & guidObjectList.Any())
            {
                foreach (var guidObject in guidObjectList)
                {
                    if (!string.IsNullOrEmpty(guidObject.Id))
                    {
                        retval.Add(guidObject.Id);
                    }

                }

            }
            return retval;
        }
    }
}