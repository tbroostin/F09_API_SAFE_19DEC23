// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.App.Config.Storage.Service.Client;
using Ellucian.Colleague.Api.Client;
using Ellucian.Colleague.Api.Models;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Dmi.Client.Das;
using Ellucian.Dmi.Runtime;
using Ellucian.Logging;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Configuration;
using Ellucian.Web.Mvc.Filter;
using Ellucian.Web.Resource;
using Ellucian.Web.Security;
using Newtonsoft.Json;
using slf4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Hosting;
using Ellucian.Web.Http.Exceptions;
using System.Web.Mvc;
using Serilog.Core;
using Lucene.Net.Support;
using Ellucian.Colleague.Api.Helpers;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Protocols;
using System.Threading;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Web API administration controller.
    /// </summary>
    [LocalRequest]
    public class AdminController : Controller
    {
        private const int DMI_ALSERVER = 2;
        private const string PasswordSecretPlaceholder = "*********";
        private ISettingsRepository settingsRepository;
        private IConfigurationService configurationService;
        private ILogger logger;
        private ApiSettings apiSettings;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="settingsRepository">ISettingsRepository instance</param>
        /// <param name="configurationService">IConfigurationService instance</param>
        /// <param name="apiSettings">IConfigurationService instance</param>
        /// <param name="logger">ILogger instance</param>
        public AdminController(ISettingsRepository settingsRepository, IConfigurationService configurationService, ApiSettings apiSettings, ILogger logger)
        {
            if (settingsRepository == null)
            {
                throw new ArgumentNullException("settingsRepository");
            }
            this.settingsRepository = settingsRepository;
            this.logger = logger;
            this.configurationService = configurationService;
            this.apiSettings = apiSettings;
        }

        /// <summary>
        /// Gets the main API administration page.
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            FileVersionInfo assemblyFileVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            if (assemblyFileVersion != null)
            {
                ViewBag.ApiVersionNumber = assemblyFileVersion.FileVersion;
            }
            return View();
        }

        #region local settings actions

        /// <summary>
        /// Gets the API connection settings page.
        /// </summary>
        /// <returns></returns>
        public ActionResult ConnectionSettings()
        {
            var domainSettings = settingsRepository.Get();
            var model = BuildSettingsModel(domainSettings);

            if (!string.IsNullOrEmpty(model.DasPassword))
                model.DasPassword = PasswordSecretPlaceholder;
            if (!string.IsNullOrEmpty(model.SharedSecret1))
                model.SharedSecret1 = PasswordSecretPlaceholder;
            if (!string.IsNullOrEmpty(model.SharedSecret2))
                model.SharedSecret2 = PasswordSecretPlaceholder;
            if (!string.IsNullOrEmpty(model.OauthProxyPassword))
                model.OauthProxyPassword = PasswordSecretPlaceholder;

            ViewBag.json = JsonConvert.SerializeObject(model);
            return View();
        }

        /// <summary>
        /// Post the API connection settings page.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> ConnectionSettings(string model)
        {
            var localSettingsModel = JsonConvert.DeserializeObject<WebApiSettings>(model);
            var domainSettings = settingsRepository.Get();
            var oldModel = BuildSettingsModel(domainSettings);

            if (localSettingsModel.SharedSecret1 == PasswordSecretPlaceholder)
                localSettingsModel.SharedSecret1 = oldModel.SharedSecret1;

            if (localSettingsModel.SharedSecret2 == PasswordSecretPlaceholder)
                localSettingsModel.SharedSecret2 = oldModel.SharedSecret2;

            if (localSettingsModel.DasPassword == PasswordSecretPlaceholder)
                localSettingsModel.DasPassword = oldModel.DasPassword;

            if (localSettingsModel.OauthProxyPassword == PasswordSecretPlaceholder)
                localSettingsModel.OauthProxyPassword = oldModel.OauthProxyPassword;

            try
            {
                var cookie = LocalUserUtilities.GetCookie(Request);

                IPrincipal localUser = null;
                if (cookie != null)
                {
                    localUser = LocalUserUtilities.GetCurrentUser(Request);
                }
                var localUserName = localUser?.Identity.Name ?? "LocalAdmin";
                localSettingsModel.AuditLogConfigurationChanges(oldModel, localUserName);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to audit log changes for web api connection settings.");
            }

            var settings = BuildSettingsDomain(localSettingsModel);
            settingsRepository.Update(settings);
            PerformBackupConfig();
            RecycleApp();
            return RedirectToAction("SettingsConfirmation");
        }

        /// <summary>
        /// Gets the API logging settings page.
        /// </summary>
        /// <returns></returns>
        public ActionResult Logging()
        {
            var domainSettings = settingsRepository.Get();
            var model = BuildSettingsModel(domainSettings);

            if (!string.IsNullOrEmpty(model.DasPassword))
                model.DasPassword = PasswordSecretPlaceholder;
            if (!string.IsNullOrEmpty(model.SharedSecret1))
                model.SharedSecret1 = PasswordSecretPlaceholder;
            if (!string.IsNullOrEmpty(model.SharedSecret2))
                model.SharedSecret2 = PasswordSecretPlaceholder;
            if (!string.IsNullOrEmpty(model.OauthProxyPassword))
                model.OauthProxyPassword = PasswordSecretPlaceholder;

            ViewBag.json = JsonConvert.SerializeObject(model);
            return View();
        }

        /// <summary>
        /// Post the API logging settings page.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> Logging(string model)
        {
            var localSettingsModel = JsonConvert.DeserializeObject<WebApiSettings>(model);
            var domainSettings = settingsRepository.Get();
            var oldModel = BuildSettingsModel(domainSettings);

            if (localSettingsModel.SharedSecret1 == PasswordSecretPlaceholder)
                localSettingsModel.SharedSecret1 = oldModel.SharedSecret1;

            if (localSettingsModel.SharedSecret2 == PasswordSecretPlaceholder)
                localSettingsModel.SharedSecret2 = oldModel.SharedSecret2;

            if (localSettingsModel.DasPassword == PasswordSecretPlaceholder)
                localSettingsModel.DasPassword = oldModel.DasPassword;

            if (localSettingsModel.OauthProxyPassword == PasswordSecretPlaceholder)
                localSettingsModel.OauthProxyPassword = oldModel.OauthProxyPassword;

            try
            {
                var cookie = LocalUserUtilities.GetCookie(Request);

                IPrincipal localUser = null;
                if (cookie != null)
                {
                    localUser = LocalUserUtilities.GetCurrentUser(Request);
                }
                var localUserName = localUser?.Identity.Name ?? "LocalAdmin";
                localSettingsModel.AuditLogConfigurationChanges(oldModel, localUserName);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to audit log changes for web api logging settings.");
            }

            var settings = BuildSettingsDomain(localSettingsModel);
            settingsRepository.Update(settings);
            PerformBackupConfig();
            RecycleApp();
            return RedirectToAction("SettingsConfirmation");
        }

        /// <summary>
        /// Desiged as an API method to allow for a simple change for the log level.
        /// </summary>
        /// <param name="loggingLevel">One of the valid options: Off, Error, Warning, Information, Verbose</param>
        /// <returns>OK or BadRequest</returns>
        [HttpPut]
        public ActionResult LoggingLevel(string loggingLevel)
        {
            var domainSettings = settingsRepository.Get();
            var model = BuildSettingsModel(domainSettings);

            var validLogLevel = model.LogLevels.FirstOrDefault(l => l.Value.Equals(loggingLevel, StringComparison.OrdinalIgnoreCase)
                                                                 || l.Text.Equals(loggingLevel, StringComparison.OrdinalIgnoreCase));
            if (validLogLevel != null)
            {
                model.LogLevel = validLogLevel.Text;

                var settings = BuildSettingsDomain(model);
                settingsRepository.Update(settings);
                PerformBackupConfig();
                RecycleApp();
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }

        #endregion

        #region api settings profile actions

        /// <summary>
        /// Gets the API settings profile page.
        /// </summary>
        /// <returns></returns>
        public ActionResult ApiSettingsProfile()
        {
            if (LocalUserUtilities.GetCurrentUser(Request) == null)
            {
                var error = "You must login before accessing the API Settings Profile";
                string returnUrl = Url.Action("ApiSettings", "Admin");
                return RedirectToAction("Login", new { returnUrl = returnUrl, error = error });
            }

            ViewBag.json = JsonConvert.SerializeObject(GetApiSettingsProfileModel());
            return View();
        }

        /// <summary>
        /// Posts the API settings profile page.
        /// </summary>
        /// <param name="model">JSON string representing a <see cref="ApiSettingsProfileModel"/></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ApiSettingsProfile(string model)
        {
            var apiSettingsProfileModel = JsonConvert.DeserializeObject<ApiSettingsProfileModel>(model);
            var settingsDomain = settingsRepository.Get();

            // 1 of 2 things will happen: 1) create new profile, 2) change the current profile name.
            string selectedExistingProfileName = null;
            if (apiSettingsProfileModel.SelectedExistingProfileName != null && !string.IsNullOrEmpty(apiSettingsProfileModel.SelectedExistingProfileName.Value))
            {
                selectedExistingProfileName = apiSettingsProfileModel.SelectedExistingProfileName.Value;
            }

            // new profile
            if (!string.IsNullOrEmpty(apiSettingsProfileModel.NewProfileName) && string.IsNullOrEmpty(selectedExistingProfileName))
            {
                try
                {
                    ApiSettings apiSettings = new ApiSettings(apiSettingsProfileModel.NewProfileName);
                    var apiSettingsRepo = CreateApiSettingsRepository();
                    apiSettingsRepo.Update(apiSettings);
                }
                catch (Exception e)
                {
                    throw e;
                }
                // set the new name in the xml
                settingsDomain.ProfileName = apiSettingsProfileModel.NewProfileName;
            }
            // existing, just set the new name
            else if (string.IsNullOrEmpty(apiSettingsProfileModel.NewProfileName) && !string.IsNullOrEmpty(selectedExistingProfileName))
            {
                settingsDomain.ProfileName = selectedExistingProfileName;
            }

            // update the xml
            settingsRepository.Update(settingsDomain);

            PerformBackupConfig();

            // recycle
            RecycleApp();

            return RedirectToAction("ApiSettings");
        }

        #endregion

        #region api settings actions

        /// <summary>
        /// Processes a user's request to access API settings
        /// </summary>
        /// <returns>The ApiSettings view</returns>
        public ActionResult ApiSettings()
        {
            if (LocalUserUtilities.GetCurrentUser(Request) == null)
            {
                var error = "You must login before accessing the API Settings";
                string returnUrl = Url.Action("ApiSettings", "Admin");
                return RedirectToAction("Login", new { returnUrl = returnUrl, error = error });
            }

            var settingsDomain = settingsRepository.Get();
            if (settingsDomain != null && string.IsNullOrEmpty(settingsDomain.ProfileName))
            {
                // profile name not defined in local settings - force the admin to create or set one...
                return RedirectToAction("ApiSettingsProfile");
            }

            var apiSettingsRepo = CreateApiSettingsRepository();
            ApiSettings apiSettingsDomain = null;
            try
            {
                apiSettingsDomain = apiSettingsRepo.Get(settingsDomain.ProfileName);
            }
            catch (ArgumentException)
            {
                // profile name does not exist in colleague
                return RedirectToAction("ApiSettingsProfile");
            }

            // Instantiate an ApiSettingsModel
            var apiSettingsModel = new ApiSettingsModel();
            apiSettingsModel.Id = apiSettingsDomain.Id;
            apiSettingsModel.Version = apiSettingsDomain.Version;
            apiSettingsModel.ProfileName = settingsDomain.ProfileName;

            // Initialize the photo settings
            apiSettingsModel.PhotoSettings.ParseFormattedUrl(apiSettingsDomain.PhotoURL);
            var selectedImageType = apiSettingsModel.PhotoSettings.ImageTypes.Where(a => a.Key == apiSettingsDomain.PhotoType).FirstOrDefault();
            if (selectedImageType.Key != null)
            {
                apiSettingsModel.PhotoSettings.SelectedImageType = selectedImageType;
            }

            if (apiSettingsDomain.PhotoHeaders != null && apiSettingsDomain.PhotoHeaders.Count > 0) // name should be PhotoHeaders
            {
                apiSettingsModel.PhotoSettings.CustomHeaders = apiSettingsDomain.PhotoHeaders.Select(x => new KeyValuePair<string, string>(x.Key, x.Value)).ToList();
            }

            // Initialize the report settings
            apiSettingsModel.ReportSettings.ReportLogoPath = apiSettingsDomain.ReportLogoPath;
            apiSettingsModel.ReportSettings.UnofficialWatermarkPath = apiSettingsDomain.UnofficialWatermarkPath;

            // Initialize the cache settings
            //   Set up the supported cache providers data for the drop-down
            apiSettingsModel.CacheSettings.SupportedCacheProviders = apiSettingsDomain.SupportedCacheProviders;

            // Set the currently-selected cache provider; if the setting from the domain is empty, default to in-process caching since that's the default setting
            string currentCacheProvider = apiSettingsDomain.CacheProvider;
            if (string.IsNullOrEmpty(currentCacheProvider))
            {
                currentCacheProvider = Ellucian.Web.Http.Configuration.ApiSettings.INPROC_CACHE;
            }
            var selectedCacheProvider = apiSettingsModel.CacheSettings.SupportedCacheProviders.Where(a => a.Value == apiSettingsDomain.CacheProvider).FirstOrDefault();
            if (selectedCacheProvider.Key != null)
            {
                apiSettingsModel.CacheSettings.SelectedCacheProvider = selectedCacheProvider;
            }

            if (apiSettingsDomain.CacheProvider == Ellucian.Web.Http.Configuration.ApiSettings.INPROC_CACHE)
            {
                // For in-process caching, the cache host, port, name, and trace levels do not apply
                apiSettingsModel.CacheSettings.CacheHost = string.Empty;
                apiSettingsModel.CacheSettings.CachePort = null;
            }
            else
            {
                // Set the cache host, port, and name from the values retrieved from Colleague
                apiSettingsModel.CacheSettings.CacheHost = apiSettingsDomain.CacheHost;
                apiSettingsModel.CacheSettings.CachePort = apiSettingsDomain.CachePort;
            }

            // Serialize the settings model and add to the view bag
            ViewBag.json = JsonConvert.SerializeObject(apiSettingsModel);
            return View();
        }

        /// <summary>
        /// Processes a user's request to update API settings
        /// </summary>
        /// <param name="model">The Api settings view model</param>
        /// <returns>The settings confirmation view</returns>
        [HttpPost]
        public async Task<ActionResult> ApiSettings(string model)
        {
            var apiSettingsModel = JsonConvert.DeserializeObject<ApiSettingsModel>(model);

            if (!string.IsNullOrEmpty(apiSettingsModel.ProfileName))
            {
                ApiSettings apiSettingsDomain = new Web.Http.Configuration.ApiSettings(apiSettingsModel.Id, apiSettingsModel.ProfileName, apiSettingsModel.Version);
                if (apiSettingsModel.PhotoSettings != null)
                {
                    var photoSettings = apiSettingsModel.PhotoSettings;
                    apiSettingsDomain.PhotoURL = photoSettings.GetFormattedUrl();
                    apiSettingsDomain.PhotoType = photoSettings.SelectedImageType.Key;
                    if (photoSettings.CustomHeaders != null && photoSettings.CustomHeaders.Count > 0)
                    {
                        foreach (var header in photoSettings.CustomHeaders)
                        {
                            if (!string.IsNullOrEmpty(header.Key))
                            {
                                apiSettingsDomain.PhotoHeaders.Add(header.Key, header.Value ?? string.Empty);
                            }
                        }
                    }
                }

                if (apiSettingsModel.ReportSettings != null)
                {
                    var reportSettings = apiSettingsModel.ReportSettings;
                    apiSettingsDomain.ReportLogoPath = reportSettings.ReportLogoPath;
                    apiSettingsDomain.UnofficialWatermarkPath = reportSettings.UnofficialWatermarkPath;
                }

                if (apiSettingsModel.CacheSettings != null)
                {
                    var cacheSettings = apiSettingsModel.CacheSettings;
                    apiSettingsDomain.CacheProvider = cacheSettings.SelectedCacheProvider.Value;
                    if (apiSettingsDomain.CacheProvider == Ellucian.Web.Http.Configuration.ApiSettings.INPROC_CACHE)
                    {
                        // In-process caching does not use any of the other settings; so, even if set, disregard them
                        apiSettingsDomain.CacheHost = string.Empty;
                        apiSettingsDomain.CachePort = null;
                    }
                    else
                    {
                        // For all other providers, set all other settings
                        apiSettingsDomain.CacheHost = cacheSettings.CacheHost;
                        apiSettingsDomain.CachePort = cacheSettings.CachePort;
                    }
                }

                var apiSettingsRepo = CreateApiSettingsRepository();
                var oldApiSettings = apiSettingsRepo.Get(apiSettingsDomain.Name);
                apiSettingsRepo.Update(apiSettingsDomain);

                try
                {
                    var cookie = LocalUserUtilities.GetCookie(Request);

                    IPrincipal localUser = null;
                    if (cookie != null)
                    {
                        localUser = LocalUserUtilities.GetCurrentUser(Request);
                    }
                    var localUserName = localUser?.Identity.Name ?? "LocalAdmin";
                    apiSettingsDomain.AuditLogConfigurationChanges(oldApiSettings, localUserName);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Failed to audit log changes for web api logging settings.");
                }
            }

            // do a backup, even though API settings isn't included in the restore.
            PerformBackupConfig();

            // recycle
            RecycleApp();

            return RedirectToAction("SettingsConfirmation");
        }

        #endregion

        #region Oauth settings actions

        /// <summary>
        /// Gets the API connection settings page.
        /// </summary>
        /// <returns></returns>
        public ActionResult OauthSettings()
        {
            var domainSettings = settingsRepository.Get();
            var model = BuildSettingsModel(domainSettings);

            if (!string.IsNullOrEmpty(model.DasPassword))
                model.DasPassword = PasswordSecretPlaceholder;
            if (!string.IsNullOrEmpty(model.SharedSecret1))
                model.SharedSecret1 = PasswordSecretPlaceholder;
            if (!string.IsNullOrEmpty(model.SharedSecret2))
                model.SharedSecret2 = PasswordSecretPlaceholder;
            if (!string.IsNullOrEmpty(model.OauthProxyPassword))
                model.OauthProxyPassword = PasswordSecretPlaceholder;

            ViewBag.json = JsonConvert.SerializeObject(model);
            return View();
        }

        /// <summary>
        /// Post the API connection settings page.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> OauthSettings(string model)
        {
            var localSettingsModel = JsonConvert.DeserializeObject<WebApiSettings>(model);
            var domainSettings = settingsRepository.Get();
            var oldModel = BuildSettingsModel(domainSettings);

            if (localSettingsModel.SharedSecret1 == PasswordSecretPlaceholder)
                localSettingsModel.SharedSecret1 = oldModel.SharedSecret1;

            if (localSettingsModel.SharedSecret2 == PasswordSecretPlaceholder)
                localSettingsModel.SharedSecret2 = oldModel.SharedSecret2;

            if (localSettingsModel.DasPassword == PasswordSecretPlaceholder)
                localSettingsModel.DasPassword = oldModel.DasPassword;

            if (localSettingsModel.OauthProxyPassword == PasswordSecretPlaceholder)
                localSettingsModel.OauthProxyPassword = oldModel.OauthProxyPassword;

            try
            {
                var cookie = LocalUserUtilities.GetCookie(Request);

                IPrincipal localUser = null;
                if (cookie != null)
                {
                    localUser = LocalUserUtilities.GetCurrentUser(Request);
                }
                var localUserName = localUser?.Identity.Name ?? "LocalAdmin";
                localSettingsModel.AuditLogConfigurationChanges(oldModel, localUserName);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to audit log changes for web api connection settings.");
            }

            var settings = BuildSettingsDomain(localSettingsModel);
            settingsRepository.Update(settings);
            PerformBackupConfig();
            RecycleApp();
            return RedirectToAction("SettingsConfirmation");
        }

        #endregion

        #region login/out actions

        /// <summary>
        /// Gets the login page used by the API settings pages.
        /// </summary>
        /// <param name="returnUrl">URL to return to after login, if any.</param>
        /// <param name="error">Error message from a previous failed login.</param>
        /// <returns></returns>
        public ActionResult Login(string returnUrl, string error)
        {
            if (!string.IsNullOrEmpty(error))
            {
                ModelState.AddModelError("", error);
            }
            if (!string.IsNullOrEmpty(returnUrl))
            {
                ViewBag.RouteValues = new { returnUrl = returnUrl };
            }
            return View();
        }

        /// <summary>
        /// Submits the login page used by the API settings pages.
        /// </summary>
        /// <param name="credentials"><see cref="TestLogin"/> model</param>
        /// <param name="returnUrl">URL to return to after login, if any.</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> Login(TestLogin credentials, string returnUrl)
        {
            try
            {
                string baseUrl = credentials.BaseUrl;
                if (string.IsNullOrEmpty(baseUrl))
                {
                    baseUrl = GetWebAppRoot();

                }
                var client = new ColleagueApiClient(baseUrl, logger);
                var token = await client.Login2Async(credentials.UserId, credentials.Password);
                Response.Cookies.Add(LocalUserUtilities.CreateCookie(baseUrl, token));
            }
            catch (Exception e)
            {
                ModelState.AddModelError("", e.Message);
                ViewBag.RouteValues = new { returnUrl = returnUrl };
                return View(credentials);
            }

            if (returnUrl != null && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// Submits a logout for the login used by the API settings pages.
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> Logout()
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
            await client.LogoutAsync(token);
            Response.Cookies.Add(LocalUserUtilities.CreateExpiredCookie());
            return RedirectToAction("Index", "Home");
        }
        #endregion

        /// <summary>
        /// Gets the API settings confirmation page.
        /// </summary>
        /// <returns></returns>
        public ActionResult SettingsConfirmation()
        {
            var domainSettings = settingsRepository.Get();
            var model = BuildSettingsModel(domainSettings);

            if (!string.IsNullOrEmpty(model.DasPassword))
                model.DasPassword = PasswordSecretPlaceholder;
            if (!string.IsNullOrEmpty(model.SharedSecret1))
                model.SharedSecret1 = PasswordSecretPlaceholder;
            if (!string.IsNullOrEmpty(model.SharedSecret2))
                model.SharedSecret2 = PasswordSecretPlaceholder;

            ViewBag.json = JsonConvert.SerializeObject(model);

            return View();
        }

        /// <summary>
        /// Submits a request to test the app listener connection setting to DMI.
        /// </summary>
        /// <param name="model"><see cref="TestConnection"/> model</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> TestAppConnectionAsync(TestConnection model)
        {
            var collSettings = new ColleagueSettings();
            var dmiSettings = new DmiSettings();
            dmiSettings.AccountName = model.AccountName;
            dmiSettings.ConnectionPoolSize = model.ConnectionPoolSize;
            dmiSettings.HostNameOverride = model.HostNameOverride;
            dmiSettings.IpAddress = model.IpAddress;
            dmiSettings.Port = model.Port;
            dmiSettings.Secure = model.Secure;
            if (model.SharedSecret1 == PasswordSecretPlaceholder)
            {
                var domainSettings = settingsRepository.Get();
                var oldModel = BuildSettingsModel(domainSettings);
                dmiSettings.SharedSecret = oldModel.SharedSecret1;
            }
            else
                dmiSettings.SharedSecret = model.SharedSecret1;

            collSettings.DmiSettings = dmiSettings;
            var cacheProvider = DependencyResolver.Current.GetService<ICacheProvider>();
            var sessionRepo = new ColleagueSessionRepository(dmiSettings, cacheProvider);
            string token = null;
            try
            {
                token = await sessionRepo.LoginAsync(model.UserId, model.Password);
            }
            catch (LoginException lex)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(lex.Message);
            }
            catch (Exception ex)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json("Login failed: " + ex.Message);
            }

            try
            {
                // try a data read to verify the shared secret
                var claims = JwtHelper.CreatePrincipal(token);
                var dataReader = CreateTransactionFactory(collSettings, claims).GetDataReader();
                await dataReader.SelectAsync("UT.PARMS", "");

                // Bye
                await sessionRepo.LogoutAsync(token);
                return Json("Success!");
            }
            catch (Exception ex)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json("Data reader test failed. Check the Shared Secret on the API Connection Settings form. The error was: " + ex.Message);
            }
            finally
            {
                HttpContext.User = null;
            }
        }

        /// <summary>
        /// Submits a request to test the OAuth setting URL and Proxy UserID/Password.
        /// </summary>
        /// <param name="model"><see cref="TestConnection"/> model</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> TestOauthSettingsAsync(TestConnection model)
        {
            var domainSettings = settingsRepository.Get();
            var dmiSettings = domainSettings.ColleagueSettings.DmiSettings;
            var myIssuer = model.OauthIssuerUrl;

            if (model.Password == PasswordSecretPlaceholder)
            {
                var oldModel = BuildSettingsModel(domainSettings);
                model.Password = oldModel.OauthProxyPassword;
            }

            try
            {
                IConfigurationManager<OpenIdConnectConfiguration> configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>($"{myIssuer}/.well-known/openid-configuration", new OpenIdConnectConfigurationRetriever());
                OpenIdConnectConfiguration openIdConfig = await configurationManager.GetConfigurationAsync(CancellationToken.None);
            }
            catch (Exception)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json("The OAUTH Issuer URL did not respond with the OpenID Configuration.");
            }

            var cacheProvider = DependencyResolver.Current.GetService<ICacheProvider>();
            var sessionRepo = new ColleagueSessionRepository(dmiSettings, cacheProvider);
            string token;
            try
            {
                token = await sessionRepo.LoginAsync(model.UserId, model.Password);
            }
            catch (LoginException lex)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(lex.Message);
            }
            catch (Exception ex)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json("Login failed: " + ex.Message);
            }

            // Bye
            await sessionRepo.LogoutAsync(token);
            HttpContext.User = null;

            return Json("Success!");
        }

        /// <summary>
        /// Submits a request to test the connection setting to DAS.
        /// </summary>
        /// <param name="model"><see cref="TestDASConnectionAsync"/>model</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> TestDASConnectionAsync(TestConnection model)
        {
            var collSettings = new ColleagueSettings();
            var dasSettings = new DasSettings();
            dasSettings.AccountName = model.DasAccountName;
            dasSettings.ConnectionPoolSize = model.DasConnectionPoolSize.HasValue ? model.DasConnectionPoolSize.Value : 1;
            dasSettings.HostNameOverride = model.DasHostNameOverride;
            dasSettings.IpAddress = model.DasIpAddress;
            dasSettings.Port = model.DasPort.HasValue ? model.DasPort.Value : 1;
            dasSettings.Secure = model.DasSecure;
            dasSettings.DbLogin = model.DasUsername;
            if (model.DasPassword == PasswordSecretPlaceholder)
            {
                var domainSettings = settingsRepository.Get();
                var oldModel = BuildSettingsModel(domainSettings);
                dasSettings.DbPassword = oldModel.DasPassword;
            }
            else
                dasSettings.DbPassword = model.DasPassword;

            collSettings.DasSettings = dasSettings;

            // Das session instantiation
            DasSession dasSession;
            try
            {
                dasSession = new DasSession(dasSettings);
            }
            catch (Exception ex)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                string errorMessage = "DAS session creation failed. The error was: " + ex.Message;
                if (ex.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.Message))
                {
                    errorMessage += " (" + ex.InnerException.Message + ")";
                }
                return Json(errorMessage);
            }

            // Login
            try
            {
                if (model.DasPassword == PasswordSecretPlaceholder)
                {
                    var domainSettings = settingsRepository.Get();
                    var oldModel = BuildSettingsModel(domainSettings);
                    await dasSession.LoginAsync(model.DasUsername, oldModel.DasPassword);
                }
                else
                    await dasSession.LoginAsync(model.DasUsername, model.DasPassword);
            }
            catch (Exception ex)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json("Login failed: " + ex.Message);
            }

            // Data Reader test (to validate registry/account name)
            bool exceptionOccurred = false;
            try
            {
                // Take the specified account name from the settings page, and read from DMIACCTS
                var dmiacctsReadObject = await dasSession.ReadRecordAsync("DMIACCTS", dasSettings.AccountName);
                if (dmiacctsReadObject.ReadStatusCode == ReadStatus.Normal)
                {
                    string dmiacctsRawRecord = dmiacctsReadObject.Record;
                    string dmiAlserverValue = DasUtils.Field(dmiacctsRawRecord, DmiString._FM, DMI_ALSERVER);

                    if (!dmiAlserverValue.EndsWith("_database"))
                    {
                        // The pointer to the ALSERVRS record isn't referencing the database connection (never the case for
                        // the DAS registry/account name); throw exception
                        throw new ColleagueDataReaderException("This does not appear to be a Colleague DAS listener. Please check the DAS Registry Name on the API Connection Settings form.");
                    }
                }
                else
                {
                    // Failure to read record; exception
                    throw new ColleagueDataReaderException("This does not appear to be a Colleague DAS listener. Check the DAS Registry Name, Listener IP, and/or Listener Port on the API Connection Settings form.");
                }
            }
            catch (Exception ex)
            {
                exceptionOccurred = true;
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(ex.Message);
            }
            finally
            {
                // Ensure that we log out if an exception occurred
                if (exceptionOccurred)
                {
                    try
                    {
                        // Can't await in finally block (yet)
                        Task.Run(async () => { await dasSession.LogoutAsync(); }).GetAwaiter().GetResult();
                    }
                    catch (Exception ex)
                    {
                        // Can ignore exceptions here
                        logger.Error(ex.Message, "Error DAS session logout");
                    }
                    finally
                    {
                        HttpContext.User = null;
                    }
                }
            }

            // Logout
            try
            {
                await dasSession.LogoutAsync();
                return Json("Success!");
            }
            catch (Exception ex)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json("Login was successful but error on logout. The error was: " + ex.Message);
            }
            finally
            {
                HttpContext.User = null;
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
                    var result = AppConfigUtility.StorageServiceClient.PostConfigurationAsync(
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
                // Colleague-based backup
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

        private string GetWebAppRoot()
        {
            string host = (Request.Url.IsDefaultPort) ?
                Request.Url.Host :
                Request.Url.Authority;
            host = String.Format("{0}://{1}", Request.Url.Scheme, host);
            if (Request.ApplicationPath == "/")
                return host;
            else
                return host + Request.ApplicationPath;
        }

        private void RecycleApp()
        {
            try
            {
                // Touch web.config to force a recycle
                string ConfigPath = HttpContext.Request.PhysicalApplicationPath + "\\web.config";
                System.IO.File.SetLastWriteTimeUtc(ConfigPath, DateTime.UtcNow);
            }
            catch (UnauthorizedAccessException uae)
            {
                throw new ColleagueWebApiException("The settings were saved, but restarting the application failed. Either manually recycle the application pool, or verify that the web.config file " +
                        " is not read only and that the application pool has write permissions.", uae);
            }
        }

        private HttpContextTransactionFactory CreateTransactionFactory(ColleagueSettings collSettings = null, IPrincipal user = null)
        {
            if (collSettings == null)
            {
                var settings = DependencyResolver.Current.GetService<XmlSettingsRepository>().Get();
                if (settings != null)
                {
                    collSettings = settings.ColleagueSettings;
                }
            }
            if (user != null)
            {
                HttpContext.User = user;
            }
            var logger = DependencyResolver.Current.GetService<ILogger>();
            return new HttpContextTransactionFactory(logger, collSettings);
        }

        private ApiSettingsRepository CreateApiSettingsRepository()
        {
            var cookie = LocalUserUtilities.GetCookie(Request);
            var cookieValue = cookie == null ? null : cookie.Value;
            if (string.IsNullOrEmpty(cookieValue))
            {
                throw new ColleagueWebApiException("Log in first");
            }
            var baseUrl = cookieValue.Split('*')[0];
            var token = cookieValue.Split('*')[1];

            var principal = JwtHelper.CreatePrincipal(token);

            var cacheProvider = DependencyResolver.Current.GetService<ICacheProvider>();
            var logger = DependencyResolver.Current.GetService<ILogger>();

            return new ApiSettingsRepository(cacheProvider, CreateTransactionFactory(user: principal), logger);
        }

        private WebApiSettings BuildSettingsModel(Settings settings)
        {
            var model = new WebApiSettings();
            model.AccountName = settings.ColleagueSettings.DmiSettings.AccountName;
            model.ConnectionPoolSize = settings.ColleagueSettings.DmiSettings.ConnectionPoolSize;
            model.HostNameOverride = settings.ColleagueSettings.DmiSettings.HostNameOverride;
            model.IpAddress = settings.ColleagueSettings.DmiSettings.IpAddress;
            model.Port = settings.ColleagueSettings.DmiSettings.Port;
            model.Secure = settings.ColleagueSettings.DmiSettings.Secure;
            model.SharedSecret1 = settings.ColleagueSettings.DmiSettings.SharedSecret;
            model.SharedSecret2 = settings.ColleagueSettings.DmiSettings.SharedSecret;
            model.UseDasDatareader = settings.ColleagueSettings.GeneralSettings.UseDasDatareader;
            model.DasAccountName = settings.ColleagueSettings.DasSettings.AccountName;
            model.DasIpAddress = settings.ColleagueSettings.DasSettings.IpAddress;
            model.DasPort = settings.ColleagueSettings.DasSettings.Port;
            model.DasSecure = settings.ColleagueSettings.DasSettings.Secure;
            model.DasHostNameOverride = settings.ColleagueSettings.DasSettings.HostNameOverride;
            model.DasConnectionPoolSize = settings.ColleagueSettings.DasSettings.ConnectionPoolSize;
            model.DasUsername = settings.ColleagueSettings.DasSettings.DbLogin;
            model.DasPassword = settings.ColleagueSettings.DasSettings.DbPassword;
            model.OauthIssuerUrl = settings.OauthSettings.OauthIssuerUrl;
            model.OauthProxyUsername = settings.OauthSettings.OauthProxyLogin;
            model.OauthProxyPassword = settings.OauthSettings.OauthProxyPassword;

            string[] levels = new string[5];
            levels[0] = SourceLevels.Off.ToString(); //in serilog there is no off setting, this will be converted to fatal level
            levels[1] = SourceLevels.Error.ToString();
            levels[2] = SourceLevels.Warning.ToString();
            levels[3] = SourceLevels.Information.ToString();
            levels[4] = SourceLevels.Verbose.ToString();

            var selectList = levels.Select(x => new SelectListItem
            {
                Text = x,
                Value = x
            }).ToList();

            model.LogLevels = selectList;

            if (settings.LogLevel.ToString() == "Fatal")
                model.LogLevel = "Off";
            else
                model.LogLevel = settings.LogLevel.ToString();

            model.ProfileName = settings.ProfileName;
            string mkError, mkWarning;
            CheckMachineKeySetting(out mkError, out mkWarning);
            model.MachineKeySettingError = mkError;
            model.MachineKeySettingWarning = mkWarning;
            return model;
        }

        private Settings BuildSettingsDomain(WebApiSettings webApiSettings)
        {
            var collSettings = new ColleagueSettings();
            var dmiSettings = new DmiSettings();
            var dasSettings = new DasSettings();
            var generalSettings = new GeneralSettings();
            var oauthSettings = new OauthSettings();

            dmiSettings.AccountName = webApiSettings.AccountName;
            dmiSettings.ConnectionPoolSize = webApiSettings.ConnectionPoolSize;
            dmiSettings.HostNameOverride = webApiSettings.HostNameOverride;
            dmiSettings.IpAddress = webApiSettings.IpAddress;
            dmiSettings.Port = webApiSettings.Port;
            dmiSettings.Secure = webApiSettings.Secure;
            dmiSettings.SharedSecret = webApiSettings.SharedSecret2;
            collSettings.DmiSettings = dmiSettings;

            generalSettings.UseDasDatareader = webApiSettings.UseDasDatareader;
            collSettings.GeneralSettings = generalSettings;

            dasSettings.AccountName = webApiSettings.DasAccountName;
            dasSettings.IpAddress = webApiSettings.DasIpAddress;
            dasSettings.Port = webApiSettings.DasPort.HasValue ? webApiSettings.DasPort.Value : 0;
            dasSettings.Secure = webApiSettings.DasSecure;
            dasSettings.HostNameOverride = webApiSettings.DasHostNameOverride;
            dasSettings.ConnectionPoolSize = webApiSettings.DasConnectionPoolSize.HasValue ? webApiSettings.DasConnectionPoolSize.Value : 0;
            dasSettings.DbLogin = webApiSettings.DasUsername;
            dasSettings.DbPassword = webApiSettings.DasPassword;
            collSettings.DasSettings = dasSettings;

            oauthSettings.OauthIssuerUrl = webApiSettings.OauthIssuerUrl;
            oauthSettings.OauthProxyLogin = webApiSettings.OauthProxyUsername;
            oauthSettings.OauthProxyPassword = webApiSettings.OauthProxyPassword;

            return new Settings(collSettings, oauthSettings,
                SerilogAdapter.LevelFromString(webApiSettings.LogLevel))
            { ProfileName = webApiSettings.ProfileName };
        }


        private ApiSettingsProfileModel GetApiSettingsProfileModel()
        {
            ApiSettingsProfileModel model = new ApiSettingsProfileModel();

            // read xml setting to get the current profile name...
            var domainSettings = settingsRepository.Get();
            model.CurrentProfileName = domainSettings.ProfileName;

            // get the existing profile names in Colleague
            var apiSettingsRepo = CreateApiSettingsRepository();
            IEnumerable<string> profileNames = apiSettingsRepo.GetNames();

            // if profile name...
            if (!string.IsNullOrEmpty(model.CurrentProfileName))
            {
                // see if the current profile exists in Colleague (if present)
                bool exists = false;
                if (profileNames != null && profileNames.Count() > 0 && !string.IsNullOrEmpty(model.CurrentProfileName))
                {
                    string current = model.CurrentProfileName.Replace(" ", "").ToUpper();
                    foreach (string name in profileNames)
                    {
                        if (name.Replace(" ", "").ToUpper() == current)
                        {
                            exists = true;
                            break;
                        }
                    }
                }
                if (!exists)
                {
                    // local name does not exist in colleague...
                    ViewBag.error = "The current profile name specified by the local configuration does not exist in Colleague - click save to create it.";
                    model.NewProfileName = model.CurrentProfileName;
                    model.CurrentProfileName = "";
                }
            }
            else
            {
                if (profileNames != null && profileNames.Count() > 0)
                {
                    // local name is not defined, and there are names in colleague...
                    ViewBag.error = "Please select either an existing profile or specify a new profile name to associate with this Web API.";
                }
                else
                {
                    // suggest a new one as none exist, anywhere...
                    ViewBag.error = "No profiles exist. A new profile name has been suggested based on the current IIS website name.";
                    var suggestion = System.Web.Hosting.HostingEnvironment.ApplicationHost.GetSiteName();
                    suggestion = suggestion.Trim().Replace(" ", string.Empty).ToUpper();
                    model.NewProfileName = suggestion;
                }
            }

            if (profileNames != null && profileNames.Count() > 0)
            {
                foreach (string profileName in profileNames)
                {
                    if (profileName.ToUpper() != model.CurrentProfileName.ToUpper())
                    {
                        (model.ExistingProfileNames as IList<SelectListItem>).Add(new SelectListItem() { Value = profileName, Text = profileName });
                    }
                }
            }

            var emptySelectListItem = new SelectListItem() { Text = "select...", Value = "" };
            (model.ExistingProfileNames as IList<SelectListItem>).Insert(0, emptySelectListItem);
            model.SelectedExistingProfileName = emptySelectListItem;

            return model;
        }

        /// <summary>
        /// Check machine key setting and return appropriate error/warning messages.
        /// </summary>
        /// <returns></returns>
        private static void CheckMachineKeySetting(out string error, out string warning)
        {
            error = string.Empty;
            warning = string.Empty;
            string messageType, keyConfig, level;

            string templateMessage = "{0}: The Machine Key configuration used by this website is currently configured with {1} keys{2}. " +
                        "The recommended configuration is to use Static keys at the Website level. Other configurations can lead to encryption/decryption problems without warning. " +
                        "Please refer to the \"Setting Up Colleague Web API\" manual, section \"Generate Machine Keys for Security\", for instructions to configure Machine Key.";

            // Machine Key config is correct if both validation and decryption keys are static (not automatically generated).
            MachineKeySection machineKeySection = (MachineKeySection)WebConfigurationManager.GetSection("system.web/machineKey");
            bool machineKeySettingIsCorrect = false;
            if (machineKeySection == null)
            {
                // No machineKey section found means machine key is set to auto generate at all levels. 
                // Display error for the website level.
                messageType = "ERROR";
                error = string.Format(templateMessage, messageType, "Automatically Generated", "Website");
                return;
            }
            else
            {
                machineKeySettingIsCorrect = (!machineKeySection.ValidationKey.Contains("AutoGenerate") &&
                                                    !machineKeySection.ValidationKey.Contains("IsolateApps") &&
                                                    !machineKeySection.DecryptionKey.Contains("AutoGenerate") &&
                                                    !machineKeySection.DecryptionKey.Contains("IsolateApps"));
            }

            string webConfigPath = machineKeySection.ElementInformation.Source;
            string appPhysicalPath = HostingEnvironment.ApplicationPhysicalPath;

            keyConfig = machineKeySettingIsCorrect ? "Static" : "Automatically Generated";

            if (string.IsNullOrEmpty(webConfigPath))
            {
                // It's unclear where the machinekey setting is read from,
                // so don't mention the level.
                level = "";
                messageType = "ERROR";
                error = string.Format(templateMessage, messageType, keyConfig, level);
            }
            else if (webConfigPath.Contains("\\Windows\\Microsoft.NET\\Framework"))
            {
                level = " at the Server level";
                // The machine key is being read from the server level config file
                // (server level config location example C:\Windows\Microsoft.NET\Framework64\v4.0.30319\Config)
                if (machineKeySettingIsCorrect)
                {
                    messageType = "Notice";
                    warning = string.Format(templateMessage, messageType, keyConfig, level);
                }
                else
                {
                    messageType = "ERROR";
                    error = string.Format(templateMessage, messageType, keyConfig, level);
                }
            }
            else
            {
                if (webConfigPath.Contains(appPhysicalPath))
                {
                    // the machine key is being read from the application level config file
                    // and we consider this an error regardless whether the key configuration is correct.
                    // This is because this configuration will be overwritten after a website upgrade.
                    level = " at the Web Application level";
                    messageType = "ERROR";
                    error = string.Format(templateMessage, messageType, keyConfig, level);
                }
                else
                {
                    // the machine key is being read from
                    // the website level, which is what we recommend. 
                    level = " at the Website level";
                    if (!machineKeySettingIsCorrect)
                    {
                        messageType = "ERROR";
                        error = string.Format(templateMessage, messageType, keyConfig, level);
                    }
                }
            }
        }
    }
}
