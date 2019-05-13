﻿using System;
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
        private const string EllucianPDFMediaTypeFormat = "application/vnd.ellucian.v{0}+pdf";

        /// <summary>
        /// F09 Custom Routings
        /// </summary>
        /// <param name="routes"></param>
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.MapHttpRoute(
                name: "GetF09ActiveRestrictionsById",
                routeTemplate: "f09/active-restrictions/{personId}",
                defaults: new { controller = "ActiveRestrictions", action = "GetActiveRestrictionsAsync" },
                constraints: new
                {
                    httpMethod = new HttpMethodConstraint("GET"),
                    headerVersion = new HeaderVersionConstraint(1, true)
                }
            );

            routes.MapHttpRoute(
                name: "GetF09StudentRestrictionById",
                routeTemplate: "f09/student-restriction/{personId}",
                defaults: new { controller = "StudentRestriction", action = "GetStudentRestrictionAsync" },
                constraints: new
                {
                    httpMethod = new HttpMethodConstraint("GET"),
                    headerVersion = new HeaderVersionConstraint(1, true)
                }
            );

            routes.MapHttpRoute(
                name: "UpdateF09StudentRestriction",
                routeTemplate: "f09/student-restriction",
                defaults: new { controller = "StudentRestriction", action = "PutStudentRestrictionAsync" },
                constraints: new
                {
                    httpMethod = new HttpMethodConstraint("PUT"),
                    headerVersion = new HeaderVersionConstraint(1, true)
                }
            );

            // F09 added here on 03-14-2019
            routes.MapHttpRoute(
                name: "GetF09ScholarshipApplicationById",
                routeTemplate: "f09/get-scholarship-application/{personId}",
                defaults: new { controller = "ScholarshipApplication", action = "GetScholarshipApplicationAsync" },
                constraints: new
                {
                    httpMethod = new HttpMethodConstraint("GET"),
                    headerVersion = new HeaderVersionConstraint(1, true)
                }
            );

            routes.MapHttpRoute(
                name: "UpdateF09ScholarshipApplication2",
                routeTemplate: "f09/update-scholarship-application",
                defaults: new { controller = "ScholarshipApplication", action = "PutScholarshipApplicationAsync" },
                constraints: new
                {
                    httpMethod = new HttpMethodConstraint("PUT"),
                    headerVersion = new HeaderVersionConstraint(1, true)
                }
            );

            // F09 added on 04-01-2019
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

            // F09 added on 05-13-2019 for Student Alumni Directories project
            routes.MapHttpRoute(
                 name: "GetF09StudentAlumniDirectoriesById",
                 routeTemplate: "f09/get-student-alumni-directories/{personId}",
                 defaults: new { controller = "StudentAlumniDirectories", action = "GetStudentAlumnniDirectoriesAsync" },
                 constraints: new
                 {
                     httpMethod = new HttpMethodConstraint("GET"),
                     headerVersion = new HeaderVersionConstraint(1, true)
                 }
             );

            routes.MapHttpRoute(
                name: "UpdateF09StudentAlumniDirectories",
                routeTemplate: "f09/update-student-alumni-directories",
                defaults: new { controller = "StudentAlumniDirectories", action = "PutStudentAlumnniDirectoriesAsync" },
                constraints: new
                {
                    httpMethod = new HttpMethodConstraint("PUT"),
                    headerVersion = new HeaderVersionConstraint(1, true)
                }
            );

            // F09 added on 05-05-2019 for Demo Reporting Project
            routes.MapHttpRoute(
                name: "GetF09StudentStatement",
                routeTemplate: "f09/get-student-statement/{accountHolderId}",
                defaults: new { controller = "ScholarshipApplication", action = "GetStudentStatementAsync" },
                constraints: new
                {
                    httpMethod = new HttpMethodConstraint("GET"),
                    headerVersion = new HeaderVersionConstraint(1, true, string.Format(EllucianPDFMediaTypeFormat, 1)),
                }
            );
        }
    }
}