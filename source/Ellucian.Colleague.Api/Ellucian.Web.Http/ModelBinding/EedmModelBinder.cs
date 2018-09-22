// Copyright 2017 - 2018 Ellucian Company L.P. and its affiliates.

using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using Newtonsoft.Json.Linq;
using slf4net;

namespace Ellucian.Web.Http.ModelBinding
{
    public class EedmModelBinder : IModelBinder
    {
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
                bindingContext.Model = bodyObject;
                return true;
            }

            if (logger != null)
            {
                logger.Error("Custom EEDM Binding failed. The BindingContext does not contain the AssemblyQualifiedName.");
            }

            return false;
        }
    }
}
