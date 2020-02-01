﻿// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.
using Ellucian.App.Config.Storage.Service.Client;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Dmi.Client;
using Ellucian.Dmi.Client.Das;
using Ellucian.Web.Http.Descriptions;
using Ellucian.Web.Security;
using Microsoft.IdentityModel.Claims;
using slf4net;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

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

            ConfigUpdateAndMonitor();
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

        #region config update / monitor

        private void ConfigUpdateAndMonitor()
        {
            var logger = DependencyResolver.Current.GetService<ILogger>();
            int atStep = 1;
            try
            {
                // Only execute this code block if config monitoring is configured and this is a SaaS environment
                if (AppConfigUtility.ConfigServiceClientSettings != null && AppConfigUtility.ConfigServiceClientSettings.IsSaaSEnvironment)
                {
                    // Force outbound requests to include TLS 1.2 as that's the only one the storage service accepts.
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                    // ***FIRST***, grab current config object and its checksum by calling this app's own "gather config data" method

                    var configObject = AppConfigUtility.GetApiConfigurationObject();
                    logger.Info("Config storage client created with namespace " + AppConfigUtility.StorageServiceClient.NameSpace);
                    // *Required*: set the current config's checksum for the monitor client so it can do long polling later.
                    var currentChecksum = Utilities.GetMd5ChecksumString(configObject.ConfigData);                    
                    ConfigStorageServiceHttpClient.CurrentConfigChecksum = currentChecksum;


                    // ***SECOND***, send a GET /latest to get latest config record - not going to do long poll here to avoid delaying app start

                    atStep = 2;
                    logger.Info("Getting latest config...");
                    App.Config.Storage.Service.Client.Models.Configuration latestConfig = null;
                    try
                    {
                        latestConfig = Task.Run(() => AppConfigUtility.StorageServiceClient.GetLatestAsync()).GetAwaiter().GetResult();
                    }
                    catch (InvalidCredentialException ice)
                    {
                        logger.Error(ice, "Invalid credentials. Cannot start config monitor job.");
                        return;
                    }
                    catch (Exception e)
                    {
                        // ignore any http exception here in case the storage service is temporarily down, in which
                        // case we still want to start a monitor thread that will wait for the service to come back on.
                        logger.Error(e, "Exception ocurred getting latest config record from storage service.");
                    }

                    // ***THIRD***, if new config data found, apply config update and shut down appdomain. Otherwise start the background monitor thread.

                    logger.Info(string.Format("Current checksum: {0}; latest checksum: {1}",
                        currentChecksum,
                        (latestConfig == null)? "null" : latestConfig.Checksum));

                    bool startMonitorThread = true;
                    if (latestConfig != null && latestConfig.Checksum != currentChecksum)
                    {
                        // Check whether this latest config has actually been restored before.
                        var lastRestoredChecksum = Utilities.GetLastRestoredChecksum();
                        if (!string.IsNullOrWhiteSpace(lastRestoredChecksum) && lastRestoredChecksum == latestConfig.Checksum)
                        {
                            logger.Info("Latest config found, but it has already been restored previously. This means the last restore performed a merge.");
                            // This "latest" backup config has already been restored previously. The fact that this instance's 
                            // current checksum is different than the "latest's" means the last restore performed a merge of
                            // different config data versions, which resulted in a new unique checksum. 

                            // So, instead of restoring this obsolete "latest" config data, we will submit the instance's current
                            // config data as the new backup config data, which will serve as the new "latest".
                            string username = "Application_Start";
                            try
                            {
                                var result = AppConfigUtility.StorageServiceClient.PostConfigurationAsync(
                                    configObject.Namespace, configObject.ConfigData, username,
                                    configObject.ConfigVersion, configObject.ProductId, configObject.ProductVersion).GetAwaiter().GetResult();
                                logger.Info("Post-merge backup sent to config storage.");
                            }
                            catch (Exception e)
                            {
                                logger.Error(e, "Post-merge backup to config storage service failed.");
                            }
                        }
                        else
                        {
                            // This is new backup data. Verify its version is valid (same or lower than this instance's) and restore it.
                            // Note: the monitoring job also does the version check and will only issue an app shutdown if latest config version is valid.
                            // This version check logic below is necessary for the startup scenario.
                            if(Utilities.VerifyNewConfigVersionOK(configObject.ConfigVersion, latestConfig.ConfigVersion))
                            {
                                logger.Info("Current config data:\n " + configObject.ConfigData);
                                logger.Info("\nNew config data:\n " + latestConfig.ConfigData);

                                atStep = 3;
                                logger.Info("New config data found, apply new data and shutting down...");
                                // We have new config to apply.
                                // Apply new config and shutdown app domain
                                AppConfigUtility.RestoreApiBackupConfiguration(latestConfig.ConfigData);
                                // Set the "last restored checksum" so on respin after a config merge 
                                // (the merge is due to current config version being higher/different than the backup config version, which results in a new checksum)
                                // we know not to restore the same backup config again and instead perform a backup.
                                Utilities.SetLastRestoredChecksum(latestConfig.Checksum);
                                atStep = 4;
                                startMonitorThread = false;
                                HttpRuntime.UnloadAppDomain();
                            }else
                            {
                                logger.Info(string.Format(
                                    "New config's version '{0}' is higher than this instance's config version '{1}'. It will not be restored.", 
                                    latestConfig.ConfigVersion, configObject.ConfigVersion));
                            }
                        }
                    }
                    
                    if (startMonitorThread)
                    {
                        atStep = 5;
                        // No new update found at this time.
                        // Kick off monitor thread and pass it a callback delegate that will shutdown app domain to restart itself.
                        // If this thread gets killed mid operation, it actually doesn't matter.
                        logger.Info("No new config data to apply. Kicking off monitor job...");
                        var monitorJob = new BackgroundMonitorJob(AppConfigUtility.StorageServiceClient);
                        ThreadStart threadDelegate = new ThreadStart(monitorJob.Start);
                        Thread newThread = new Thread(threadDelegate);
                        newThread.Start();
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(e, "Exception ocurred updating/monitoring config data at step " + atStep);
            }
        }

        #endregion
    }
}
