// Copyright 2021 Ellucian Company L.P. and its affiliates.

using System;
using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Ellucian.Web.Http.Filters
{
    /// <summary>
    /// Action Filter to add the PermissionsCollection
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class PermissionsFilter : ActionFilterAttribute
    {
        static readonly string requestPropertyName = "PermissionsFilter";

        /// <summary>
        /// Gets & Sets the value for the PermissionsCollection
        /// </summary>
        public string[] PermissionsCollection { get; set; }

        public PermissionsFilter(string permission)
        {
            PermissionsCollection = new string[] { permission };
        }

        public PermissionsFilter(string[] permissions)
        {
            PermissionsCollection = permissions;
        }
        /// <summary>
        /// OnActionExecuting
        /// </summary>
        /// <param name = "actionContext" ></ param >
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if ((actionContext != null) && (PermissionsCollection != null) && (PermissionsCollection.Any()))
            {
                actionContext.Request.Properties.Add(requestPropertyName, PermissionsCollection);
            }
            base.OnActionExecuting(actionContext);
        }
    }
}