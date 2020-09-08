// Copyright 2017 - 2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using slf4net;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;


namespace Ellucian.Web.Http.ModelBinding
{
    public class EedmModelBinder : IModelBinder
    {
        private readonly string[] inputParamNames = new string[] { "id", "guid" };

        private const string CustomMediaType = "X-Media-Type";

        private IntegrationApiException exceptionObject = null;
        /// <summary>
        /// Custom ModelBinder for EEDM API bodies
        /// </summary>
        /// <param name="context"></param>
        /// <param name="bindingContext"></param>
        /// <returns></returns>
        public bool BindModel(HttpActionContext context, ModelBindingContext bindingContext)
        {
            exceptionObject = null;
                
            var logger = context.Request.GetDependencyScope().GetService(typeof(ILogger)) as ILogger;

         
            if (bindingContext != null && bindingContext.ModelType != null && !string.IsNullOrEmpty(bindingContext.ModelType.AssemblyQualifiedName))
            {
                var bodyString = context.Request.Content.ReadAsStringAsync().Result;

                object bodyObject = null;
                if (!string.IsNullOrEmpty(bodyString))
                {
                    bodyObject = JsonConvert.DeserializeObject(bodyString, Type.GetType(bindingContext.ModelType.AssemblyQualifiedName));
                }

                var isBodyProvided = ValidateMessageBody(context, bodyObject);

                if (!isBodyProvided || string.IsNullOrEmpty(bodyString))
                {
                    return false;
                }
             
                var retVal = ValidateInputGuids(context, bodyObject);

                bindingContext.Model = bodyObject;

                Object jsonObject = JObject.Parse(bodyString);
                //add this property for PartialUpdates
                context.Request.Properties.Add("PartialInputJsonObject", jsonObject);
                //add this property for Extended Data checks
                context.Request.Properties.Add("EthosExtendedDataObject", jsonObject); 
                
                return retVal;
            }

            else if (logger != null)
            {
                logger.Error("Custom EEDM Binding failed. The BindingContext does not contain the AssemblyQualifiedName.");
            }

            return false;
        }

        /// <summary>
        /// Validates guids for PUT in url & request body. For POST in request body.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="bodyObject"></param>
        private bool ValidateInputGuids(HttpActionContext context, object bodyObject)
        {
            var guidInUrl = string.Empty;
            var guidInBody = string.Empty;
            var paramName = string.Empty;
            var httpMethod = context.Request.Method.Method;

            //Get the api name e.g. person-visas or section-registrations etc. used in the error message.
            var routeTemplate = context.Request.GetRouteData().Route.RouteTemplate;
            if (routeTemplate.Contains("/"))
            {
                var splitRoute = routeTemplate.Split(new string[] { "/" }, StringSplitOptions.None);
                if (splitRoute.Any() && splitRoute.Count() > 1)
                {
                    routeTemplate = splitRoute[0];
                }
            }

            //We have used "id" or "guid" as names for the input parameters in PUT operation.
            foreach (var inputParamName in inputParamNames)
            {
                if (context.ActionArguments.ContainsKey(inputParamName))
                {
                    guidInUrl = context.ActionArguments[inputParamName].ToString();
                    paramName = inputParamName;
                    break;
                }
            }

            //This is to make sure clients don't use nill guid in the URL
            if (!string.IsNullOrWhiteSpace(guidInUrl) && guidInUrl.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {;
                var message = string.Format("Nil GUID must not be used in {0} PUT operation.", routeTemplate);
                ModelBinderError(context, "Nil.GUID", "The nil GUID cannot be used on a PUT request.", message);
            }

            var logger = context.Request.GetDependencyScope().GetService(typeof(ILogger)) as ILogger;

            //Following will cover the POST scenario since there is no GUID as input parameter via URL.
            if (bodyObject != null)
            {
                try
                {
                    guidInBody = ((dynamic)bodyObject).Id;
                }
                catch (Exception e)
                {
                    //No need to halt processing in case casting to dynamic object throws error, try catch just to make sure nothing bombs.                    
                    if (logger != null)
                    {
                        logger.Error(e.Message);
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(guidInUrl) && !string.IsNullOrWhiteSpace(guidInBody) && !guidInUrl.Equals(guidInBody, StringComparison.OrdinalIgnoreCase))
            {
                var message = "GUID in URL is not the same as in request body.";
                ModelBinderError(context, "Mismatched.GUID", "The GUID does not match the URL and request body.", message);
            }

            //In POST operation make sure GUID is a null guid in the POST body.
            if (context.Request.Method.Equals(HttpMethod.Post) && !string.IsNullOrWhiteSpace(guidInBody) &&
                !guidInBody.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase) && !routeTemplate.Equals("qapi", StringComparison.OrdinalIgnoreCase))
            {
                var message = "On a post you can not define a GUID.";
                ModelBinderError(context, "Cannot.Set.GUID", "The requested GUID cannot be consumed on a POST request.", message);
            }

            //In POST operation make sure GUID is a null guid in the POST body.
            if ((context.Request.Method.Equals(HttpMethod.Post) || context.Request.Method.Equals(HttpMethod.Put))  && (bodyObject == null))
            {
                var message = "The request body is required.";
                ModelBinderError(context, "Missing.Request.Body", "Empty request body.", message);
            }


            Guid guidOutput;
            //This will cover PUT, guid in the URL.
            if (context.Request.Method.Equals(HttpMethod.Put) && string.IsNullOrWhiteSpace(guidInUrl))
            {
                var message = string.Format("Must provide a valid GUID for {0} {1} in the URL.", routeTemplate, httpMethod);
                ModelBinderError(context, "Missing.Request.ID", "Empty request ID.", message);
            }

            //This will cover PUT, guid in the URL.
            if (!string.IsNullOrWhiteSpace(guidInUrl) && !Guid.TryParse(guidInUrl, out guidOutput))
            {
                var message = string.Format("Must provide a valid GUID for {0} {1} in the URL.", routeTemplate, httpMethod);
                ModelBinderError(context, "Invalid.GUID", "An invalid GUID was provided.", message);
            }
            //This will cover PUT & POST, guid in the request body.
            if (!string.IsNullOrWhiteSpace(guidInBody) && !guidInBody.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                if (!Guid.TryParse(guidInBody, out guidOutput))
                {
                    var message = string.Format("Must provide a valid GUID for {0} {1} in request body.", routeTemplate, httpMethod);
                    ModelBinderError(context, "Invalid.GUID", "An invalid GUID was provided.", message);
                }
            }

            if ((context.Response == null) && (exceptionObject != null) && (exceptionObject.Errors.Any()))
            {
                var serialized = JsonConvert.SerializeObject(exceptionObject);
                context.Response = new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest) { Content = new StringContent(serialized) };
                context.Response.Headers.Add(CustomMediaType, BaseCompressedApiController.IntegrationErrors2);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates message body.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="bodyObject"></param>
        private bool ValidateMessageBody(HttpActionContext context, object bodyObject)
        {
            //In POST operation make sure GUID is a null guid in the POST body.
            if ((context.Request.Method.Equals(HttpMethod.Post) || context.Request.Method.Equals(HttpMethod.Put)) && (bodyObject == null))
            {
                var message = "The request body is required.";
                ModelBinderError(context, "Missing.Request.Body", "Empty request body.", message);
            }

            if ((context.Response == null) && (exceptionObject != null) && (exceptionObject.Errors.Any()))
            {
                var serialized = JsonConvert.SerializeObject(exceptionObject);
                context.Response = new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest) { Content = new StringContent(serialized) };
                context.Response.Headers.Add(CustomMediaType, BaseCompressedApiController.IntegrationErrors2);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Supports error messages for legacy implmentation by throwing first encountered error, rather than attempting to build a collection.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="code"></param>
        /// <param name="description"></param>
        /// <param name="message"></param>
        private void ModelBinderError(HttpActionContext context, string code = "Global.Internal.Error", string description = "",
            string message = "An unexpected error has occurred setting the model bindings.")
        {
            if (context == null || context.ActionDescriptor == null)
            {
                throw new ArgumentException(message);
            }

            bool customMediaTypeAttributeFilter = false;
            var customAttributes = context.ActionDescriptor.GetCustomAttributes<CustomMediaTypeAttributeFilter>();

            if (customAttributes != null)
            {
                customMediaTypeAttributeFilter = customAttributes.Any(x => x.ErrorContentType.Equals(BaseCompressedApiController.IntegrationErrors2,
                    StringComparison.OrdinalIgnoreCase));
            }

            if (customAttributes == null || !customMediaTypeAttributeFilter)
            {
                throw new ArgumentException(message);
            }

            if (exceptionObject == null)
                exceptionObject = new IntegrationApiException();

            exceptionObject.AddError(new IntegrationApiError(code, description, message));

        }
    }
}