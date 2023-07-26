//Copyright 2016-2022 Ellucian Company L.P. and its affiliates.
using Ellucian.App.Config.Storage.Service.Client;
using Ellucian.Colleague.Api.Client;
using Ellucian.Colleague.Api.Helpers;
using Ellucian.Colleague.Api.Models;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Http.Configuration;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Mvc.Controller;
using Ellucian.Web.Resource;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Controller to modify resource files and save the history of modifications done to the res files.
    /// </summary>
    /// <seealso cref="Ellucian.Web.Mvc.Controller.BaseCompressedController" />
    public class ResourceFileEditorController : BaseCompressedController
    {
        private IResourceRepository resourceRepository;
        private ApiSettings apiSettings;
        private ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceFileEditorController" /> class.
        /// </summary>
        /// <param name="resourceRepository">The resource repository.</param>
        /// <param name="apiSettings">API Settings</param>
        /// <param name="logger">The logger.</param>
        /// <exception cref="System.ArgumentNullException">resourceRepository</exception>
        public ResourceFileEditorController(IResourceRepository resourceRepository, ApiSettings apiSettings, ILogger logger)
            : base(logger)
        {
            if (resourceRepository == null)
            {
                throw new ArgumentNullException("resourceRepository");
            }
            this.resourceRepository = resourceRepository;
            this.apiSettings = apiSettings;
            this.logger = logger;
        }

        /// <summary>
        /// Returns the Resource File Editor if a user is logged in 
        /// </summary>
        /// <returns>Resource File Editor</returns>
        public ActionResult ResourceFileEditor()
        {
            if (LocalUserUtilities.GetCurrentUser(Request) == null)
            {
                var error = "You must login before accessing the Resource File Editor";
                string returnUrl = Url.Action("ResourceFileEditor", "ResourceFileEditor");
                return RedirectToAction("Login", "Admin", new { returnUrl = returnUrl, error = error });
            }
            return View();
        }

        /// <summary>
        /// Gets the list of resource files found in the working directory
        /// </summary>
        /// <returns></returns>
        public ActionResult GetResourceFiles()
        {
            List<string> listResFilesFound = resourceRepository.GetResourceFilePaths(true);

            List<KeyValuePair<string, string>> resFileNamePaths = listResFilesFound.Select(x => new KeyValuePair<string, string>(System.IO.Path.GetFileName(x), x)).ToList();

            return Json(resFileNamePaths, JsonRequestBehavior.AllowGet);

        }
        /// <summary>
        /// Gets the resource items of the resource file in the provided file path
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>Gets the resource items in the file as a JSonResult object </returns>
        public ActionResult GetResourceItemsByFile(string filePath)
        {

            try
            {
                ResourceFile currentFile = resourceRepository.GetResourceFile(filePath);
                List<ResourceFileEntry> listResourceItems = currentFile.ResourceFileEntries;

                return Json(currentFile, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                Response.StatusCode = (int)HttpStatusCode.BadRequest;

                string errorMessage = "Error when saving values: " + ex.Message;
                if (ex.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.Message))
                {
                    errorMessage += " (" + ex.InnerException.Message + ")";
                }
                return Json(errorMessage);
            }
        }

        /// <summary>
        /// Saves the updated resource file to the path of the file
        /// </summary>
        /// <param name="model">The resource file with the updated values</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> SaveResourceFile(string model)
        {
            try
            {
                ResourceFileModel file = Newtonsoft.Json.JsonConvert.DeserializeObject<ResourceFileModel>(model);
                ResourceFile updatedResourceFile = new ResourceFile(file.ResourceFileName);
                ResourceFile oldResourceFile = resourceRepository.GetResourceFile(file.ResourceFilePath);

                //Map the fileEntryModels to FileEntry object
                updatedResourceFile.ResourceFileEntries = file.ResourceFileEntries.Select
                    (x => new ResourceFileEntry() { Key = x.Key, Value = x.Value, Comment = x.Comment, OriginalValue = x.OriginalValue }).ToList();

                try
                {
                    var cookie = LocalUserUtilities.GetCookie(Request);

                    IPrincipal localUser = null;
                    if (cookie != null)
                    {
                        localUser = LocalUserUtilities.GetCurrentUser(Request);
                    }
                    var localUserName = localUser?.Identity.Name ?? "LocalAdmin";
                    updatedResourceFile.AuditLogConfigurationChanges(oldResourceFile, localUserName);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Failed to audit log changes for web api resource file editor.");
                }

                resourceRepository.UpdateResourceFile(file.ResourceFilePath, updatedResourceFile);
                PerformBackupConfig();
                return Json("Success");

            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                string errorMessage = "Error when saving values: " + ex.Message;
                if (ex.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.Message))
                {
                    errorMessage += " (" + ex.InnerException.Message + ")";
                }
                return Json(errorMessage);
            }
        }

        /// <summary>
        /// Backup all API configs
        /// </summary>
        public void PerformBackupConfig()
        {
            // SaaS backup
            if (AppConfigUtility.ConfigServiceClientSettings != null && AppConfigUtility.ConfigServiceClientSettings.IsSaaSEnvironment)
            {
                string username = "unknown";
                try
                {
                    username = HttpContext.User.Identity.Name;
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message, "Error at user identity name");
                }

                try
                {
                    // send a copy of this latest config data to the storage service.
                    var configObject = AppConfigUtility.GetApiConfigurationObject();
                    AppConfigUtility.StorageServiceClient.PostConfigurationAsync(
                        configObject.Namespace, configObject.ConfigData, username,
                        configObject.ConfigVersion, configObject.ProductId, configObject.ProductVersion).GetAwaiter().GetResult();

                    // after submitting a new snapshot, set the lastrestoredchecksum to this new snapshot's checksum.
                    // This must be done to avoid a looping situation where instances keep performing merges
                    // in lock step with each other due to lastrestoredchecksum file containing an older checksum, when 
                    // there are changes that are repeated (e.g. logging toggled on/off).
                    var currentChecksum = Utilities.GetMd5ChecksumString(configObject.ConfigData);
                    Utilities.SetLastRestoredChecksum(currentChecksum);
                }
                catch (Exception e)
                {
                    logger.Error(e, "Configuration changes have been saved, but the backup to config storage service failed. See API log for more details.");
                }
            }
            else
            {
                if (!apiSettings.EnableConfigBackup)
                {
                    return;
                }
                try
                {
                    var cookie = LocalUserUtilities.GetCookie(Request);
                    var cookieValue = cookie == null ? null : cookie.Value;
                    if (string.IsNullOrEmpty(cookieValue))
                    {
                        throw new ColleagueWebApiException("Log in first");
                    }
                    var baseUrl = cookieValue.Split('*')[0];
                    var token = cookieValue.Split('*')[1];
                    var client = new ColleagueApiClient(baseUrl, logger);
                    client.Credentials = token;
                    Task.Run(() => client.PostBackupApiConfigDataAsync()).Wait();
                }
                catch (Exception e)
                {
                    logger.Error(e, "Configuration changes have been saved, but the backup action failed. See API log for more details.");
                    throw;
                }
            }
        }
    }
}
