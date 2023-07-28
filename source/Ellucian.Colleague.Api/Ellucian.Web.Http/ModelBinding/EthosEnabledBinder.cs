// Copyright 2017 - 2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;


namespace Ellucian.Web.Http.ModelBinding
{
    public class EthosEnabledBinder : IModelBinder
    {
        private const string CustomMediaType = "X-Media-Type";

        private IntegrationApiException exceptionObject = null;

        /// <summary>
        /// Custom ModelBinder for Self-Service APIs body that are Ethos enabled.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="bindingContext"></param>
        /// <returns></returns>
        public bool BindModel(HttpActionContext context, ModelBindingContext bindingContext)
        {
            var retVal = true;
            
            var logger = context.Request.GetDependencyScope().GetService(typeof(ILogger)) as ILogger;

            if (bindingContext != null && bindingContext.ModelType != null && !string.IsNullOrEmpty(bindingContext.ModelType.AssemblyQualifiedName))
            {
                exceptionObject = new IntegrationApiException();
                var bodyString = context.Request.Content.ReadAsStringAsync().Result;

                object bodyObject = null;
                if (!string.IsNullOrEmpty(bodyString))
                {
                    try
                    {
                        bodyObject = JsonConvert.DeserializeObject(bodyString, Type.GetType(bindingContext.ModelType.AssemblyQualifiedName));
                    }
                    catch (Exception ex)
                    {
                        var message = ex.Message;
                        string code = "Global.Internal.Error";
                        string description = "Unspecified Error on the system which prevented execution.";
                        ModelBinderError(context, code, description, message);

                        if ((context.Response == null) && (exceptionObject != null) && (exceptionObject.Errors.Any()))
                        {
                            var serialized = JsonConvert.SerializeObject(exceptionObject);
                            context.Response = new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest) { Content = new StringContent(serialized) };
                            context.Response.Headers.Add(CustomMediaType, BaseCompressedApiController.IntegrationErrors2);

                            // Update the content type in the response headers
                            context.Response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json")
                            {
                                CharSet = Encoding.UTF8.WebName
                            };
                            IEnumerable<string> contentTypeValue = null;
                            if (!context.Response.Content.Headers.TryGetValues("Content-Type", out contentTypeValue))
                                context.Response.Content.Headers.Add("Content-Type", "application/json;charset=UTF-8");

                            return false;
                        }
                    }
                }

                var isBodyProvided = ValidateMessageBody(context, bodyObject);
                if (!isBodyProvided)
                {
                    return false;
                }

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
        /// Validates message body.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="bodyObject"></param>
        private bool ValidateMessageBody(HttpActionContext context, object bodyObject)
        {
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

                // Update the content type in the response headers
                context.Response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json")
                {
                    CharSet = Encoding.UTF8.WebName
                };
                IEnumerable<string> contentTypeValue = null;
                if (!context.Response.Content.Headers.TryGetValues("Content-Type", out contentTypeValue))
                    context.Response.Content.Headers.Add("Content-Type", "application/json;charset=UTF-8");

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

            if (exceptionObject == null)
                exceptionObject = new IntegrationApiException();

            exceptionObject.AddError(new IntegrationApiError(code, description, message));

        }
    }
}