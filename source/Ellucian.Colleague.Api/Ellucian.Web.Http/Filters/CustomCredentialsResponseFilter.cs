// Copyright 2014-2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Security;
using System.Web;

namespace Ellucian.Web.Http.Filters
{
    /// <summary>
    /// Adds the X-CustomCredentials header to the response.
    /// </summary>
    public class CustomCredentialsResponseFilter : System.Web.Http.Filters.ActionFilterAttribute
    {
        private const string CredentialsHeaderKey = "X-CustomCredentials";

        /// <summary>
        /// Attempt to add the X-CustomCredentials header to the response.
        /// </summary>
        /// <param name="actionExecutedContext"><see cref="System.Web.Http.Filters.HttpActionExecutedContext"/>action context</param>
        public override void OnActionExecuted(System.Web.Http.Filters.HttpActionExecutedContext actionExecutedContext)
        {
            try
            {
                if (HttpContext.Current.User != null && HttpContext.Current.User.Identity != null && HttpContext.Current.User.Identity.IsAuthenticated)
                {
                    string jwt = JwtHelper.Create(HttpContext.Current.User);
                    if (!string.IsNullOrEmpty(jwt))
                    {
                        actionExecutedContext.Response.Headers.Add(CredentialsHeaderKey, jwt);
                    }
                }
            }
            catch
            {
                var doNothing = true; // avoid empty catch block
            }

            base.OnActionExecuted(actionExecutedContext);
        }
    }
}
