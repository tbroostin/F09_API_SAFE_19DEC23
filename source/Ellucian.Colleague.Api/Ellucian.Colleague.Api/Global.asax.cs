// Copyright 2012-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Ellucian.Dmi.Client;
using Ellucian.Web.Http.Descriptions;
using Ellucian.Web.Security;
using Microsoft.IdentityModel.Claims;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Dmi.Client.Das;
using System.Web.Helpers;

namespace Ellucian.Colleague.Api
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    /// <summary>
    /// The Colleague Web API application
    /// </summary>
    public class StudentWebApiApplication : System.Web.HttpApplication
    {

        /// <summary>
        /// Handles the Start event of the Application control.
        /// </summary>
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            // Initialize dependency resolver
            Bootstrapper.Initialize();

            // ASP.NET MVC 4 now uses static classes for routes, filters, and bundles
            FilterConfig.RegisterGlobalFilters(GlobalConfiguration.Configuration.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            AntiForgeryConfig.UniqueClaimTypeIdentifier = "sid";

            // setup the API Explorer doc source
            string xmlDocumentationFileName = "~/bin/Ellucian.Colleague.Api.xml";
            if (File.Exists(HttpContext.Current.Server.MapPath(xmlDocumentationFileName)))
            {
                try
                {
                    GlobalConfiguration.Configuration.Services.Replace(typeof(IDocumentationProvider), new XmlCommentDocumentationProvider(HttpContext.Current.Server.MapPath(xmlDocumentationFileName)));
                }
                catch (Exception e)
                {
                    Exception exception = new Exception("Error replacing IDocumentationProvider with XmlCommentDocumentationProvider." + Environment.NewLine + e.ToString());
                    DependencyResolver.Current.GetService<ILogger>().Error(exception.ToString());
                }
            }
        }

        #region Authenticate Request

        /// <summary>
        /// Handles the AuthenticateRequest event of the Application control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Application_AuthenticateRequest(Object sender, EventArgs e)
        {
            // We only accept our claims-based authentication 
            // via the JWT string, which is the X-CustomCredentials header. 
            // Any other authentication will be invalidated.
            var jwt = Context.Request.Headers[Ellucian.Colleague.Api.Client.ColleagueApiClient.CredentialsHeaderKey];
            if (!string.IsNullOrEmpty(jwt))
            {
                try
                {
                    var principal = JwtHelper.CreatePrincipal(jwt);
                    JwtHelper.ValidateAntiForgeryClaim((principal as IClaimsPrincipal).Identities.First().Claims);
                    Context.User = principal;
                }
                catch (TokenValidationException tve)
                {
                    DependencyResolver.Current.GetService<ILogger>().Error(tve, "Invalid session token detected");
                    Context.User = null;
                }
                catch (Exception exc)
                {
                    // Invalidate the user in an unexpected event to be safe.
                    Context.User = null;
                    throw exc;
                }
            }
            else
            {
                // X-CustomCredentials was not present, but the principal could have been set by another auth handler, so as long as it's JWT let it through.
                if (Context.User != null)
                {
                    if (!Context.User.Identity.AuthenticationType.Equals("JWT", StringComparison.CurrentCultureIgnoreCase))
                    {
                        Context.User = null;
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Handles the End event of the Application control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Application_End(Object sender, EventArgs e)
        {
            try
            {
                // DMI clean up
                Task.Run(async () => await DmiConnectionPool.CloseAllConnectionsAsync()).GetAwaiter().GetResult();
            }
            catch
            {
                // Ignore
            }
            try
            {
                // DMI clean up
                Task.Run(async () => await DasSessionPool.CloseAllConnectionsAsync()).GetAwaiter().GetResult();
            }
            catch
            {
                // Ignore
            }
        }
    }
}
