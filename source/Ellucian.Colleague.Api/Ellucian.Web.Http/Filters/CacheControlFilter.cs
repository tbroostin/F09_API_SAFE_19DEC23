using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http.Filters;
using System.Net.Http.Headers;

namespace Ellucian.Web.Http.Filters
{
    public class CacheControlFilter : System.Web.Http.Filters.ActionFilterAttribute
    {
        public bool Public { get; set; }
        public int MaxAgeHours { get; set; }
        public int MaxAgeMinutes { get; set; }
        public bool Revalidate { get; set; }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            base.OnActionExecuted(actionExecutedContext);

            // Only cache successful requests
            if (actionExecutedContext.Response != null && actionExecutedContext.Response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                // Try minutes first, then hours, and finally fall back to a default cache value of 1 hour
                TimeSpan maxAge = (MaxAgeMinutes > 0) ? TimeSpan.FromMinutes(MaxAgeMinutes) : (MaxAgeHours > 0) ? TimeSpan.FromHours(MaxAgeHours) : TimeSpan.FromHours(1);

                actionExecutedContext.Response.Headers.CacheControl = new CacheControlHeaderValue();
                actionExecutedContext.Response.Headers.CacheControl.Public = Public;
                actionExecutedContext.Response.Headers.CacheControl.MaxAge = maxAge;
                actionExecutedContext.Response.Headers.CacheControl.MustRevalidate = Revalidate;
            }
        }
    }
}
