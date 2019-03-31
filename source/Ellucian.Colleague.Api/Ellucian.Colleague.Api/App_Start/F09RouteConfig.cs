using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using Ellucian.Web.Http.Routes;

namespace Ellucian.Colleague.Api
{
    /// <summary>
    /// 
    /// </summary>
    public class F09RouteConfig
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="routes"></param>
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.MapHttpRoute(
                name: "GetF09ActiveRestrictionsById",
                routeTemplate: "F09/active-restrictions/{personId}",
                defaults: new { controller = "ActiveRestrictions", action = "GetActiveRestrictionsAsync" },
                constraints: new
                {
                    httpMethod = new HttpMethodConstraint("GET"),
                    headerVersion = new HeaderVersionConstraint(1, true)
                }
            );

            routes.MapHttpRoute(
                name: "GetF09StudentRestrictionById",
                routeTemplate: "F09/student-restriction/{personId}",
                defaults: new { controller = "StudentRestriction", action = "GetStudentRestrictionAsync" },
                constraints: new
                {
                    httpMethod = new HttpMethodConstraint("GET"),
                    headerVersion = new HeaderVersionConstraint(1, true)
                }
            );

            routes.MapHttpRoute(
                name: "UpdateF09StudentRestriction",
                routeTemplate: "F09/student-restriction",
                defaults: new { controller = "StudentRestriction", action = "PutStudentRestrictionAsync" },
                constraints: new
                {
                    httpMethod = new HttpMethodConstraint("PUT"),
                    headerVersion = new HeaderVersionConstraint(1, true)
                }
            );

            routes.MapHttpRoute(
                name: "GetF09StudentRestrictionById2",
                routeTemplate: "F09/student-restriction2/{personId}",
                defaults: new { controller = "StudentRestriction2", action = "GetStudentRestrictionAsync" },
                constraints: new
                {
                    httpMethod = new HttpMethodConstraint("GET"),
                    headerVersion = new HeaderVersionConstraint(1, true)
                }
            );

            routes.MapHttpRoute(
                name: "UpdateF09StudentRestriction2",
                routeTemplate: "F09/student-restriction2",
                defaults: new { controller = "StudentRestriction2", action = "PutStudentRestrictionAsync" },
                constraints: new
                {
                    httpMethod = new HttpMethodConstraint("PUT"),
                    headerVersion = new HeaderVersionConstraint(1, true)
                }
            );

            routes.MapHttpRoute(
                 name: "GetF09StuTrackingSheetById",
                 routeTemplate: "f09/f09StuTrackingSheet/{Id}",
                 defaults: new { controller = "F09StuTrackingSheet", action = "GetF09StuTrackingSheetAsync" },
                 constraints: new
                 {
                     httpMethod = new HttpMethodConstraint("GET"),
                     headerVersion = new HeaderVersionConstraint(1, true)
                 }
             );
        }
    }
}