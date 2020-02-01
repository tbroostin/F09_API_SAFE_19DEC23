// Copyright 2017 - 2019 Ellucian Company L.P. and its affiliates.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using slf4net;
using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;

namespace Ellucian.Web.Http.ModelBinding
{
    public class EedmModelBinder : IModelBinder
    {
        private readonly string[] inputParamNames = new string[] { "id", "guid" };
        /// <summary>
        /// Custom ModelBinder for EEDM API bodies
        /// </summary>
        /// <param name="context"></param>
        /// <param name="bindingContext"></param>
        /// <returns></returns>
        public bool BindModel(HttpActionContext context, ModelBindingContext bindingContext)
        {
            var logger = context.Request.GetDependencyScope().GetService(typeof(ILogger)) as ILogger;

            var bodyString = context.Request.Content.ReadAsStringAsync().Result;
            if (string.IsNullOrEmpty(bodyString))
            {
                return false;
            }
            var jsonObject = JObject.Parse(bodyString);
            //add this property for PartialUpdates
            context.Request.Properties.Add("PartialInputJsonObject", jsonObject);
            //add this property for Extended Data checks
            context.Request.Properties.Add("EthosExtendedDataObject", jsonObject);

            if (bindingContext != null && bindingContext.ModelType != null && !string.IsNullOrEmpty(bindingContext.ModelType.AssemblyQualifiedName))
            {
                var bodyObject = JsonConvert.DeserializeObject(bodyString, Type.GetType(bindingContext.ModelType.AssemblyQualifiedName));

                ValidateInputGuids(context, bodyObject);

                bindingContext.Model = bodyObject;
                return true;
            }

            if (logger != null)
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
        private void ValidateInputGuids(HttpActionContext context, object bodyObject)
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
                if(context.ActionArguments.ContainsKey(inputParamName))
                {
                    guidInUrl = context.ActionArguments[inputParamName].ToString();
                    paramName = inputParamName;
                    break;
                }
            }

            //This is to make sure clients don't use nill guid in the URL
            if(!string.IsNullOrWhiteSpace(guidInUrl) && guidInUrl.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(string.Format("Nil GUID must not be used in {0} PUT operation.", routeTemplate), paramName);
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

            if(!string.IsNullOrWhiteSpace(guidInUrl) && !string.IsNullOrWhiteSpace(guidInBody) && !guidInUrl.Equals(guidInBody, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("GUID in URL is not the same as in request body.");
            }

            //In POST operation make sure GUID is a null guid in the POST body.
            if(context.Request.Method.Equals(HttpMethod.Post) && !string.IsNullOrWhiteSpace(guidInBody) && 
                !guidInBody.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase) && !routeTemplate.Equals("qapi", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("On a post you can not define a GUID.");
            }

            Guid guidOutput;
            //This will cover PUT, guid in the URL.
            if (!string.IsNullOrWhiteSpace(guidInUrl) && !Guid.TryParse(guidInUrl, out guidOutput))
            {
                throw new ArgumentException(string.Format("Must provide a valid GUID for {0} {1} in the URL.", routeTemplate, httpMethod), paramName);
            }
            //This will cover PUT & POST, guid in the request body.
            if (!string.IsNullOrWhiteSpace(guidInBody) && !guidInBody.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                if (!Guid.TryParse(guidInBody, out guidOutput))
                {
                    throw new ArgumentException(string.Format("Must provide a valid GUID for {0} {1} in request body.", routeTemplate, httpMethod), paramName);
                }
            }
        }
    }
}