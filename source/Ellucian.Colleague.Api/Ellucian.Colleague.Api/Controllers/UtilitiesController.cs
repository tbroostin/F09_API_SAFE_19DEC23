// Copyright 2013-2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Client;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Configuration;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Mvc.Filter;
using slf4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Mvc;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides a top-level controller for API utilities.
    /// </summary>
    [LocalRequest]
    public class UtilitiesController : Controller
    {
        private ISettingsRepository settingsRepository;
        private ILogger logger;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="settingsRepository">ISettingsRepository instance</param>
        /// <param name="logger">ILogger instance</param>
        public UtilitiesController(ISettingsRepository settingsRepository, ILogger logger)
        {
            if (settingsRepository == null)
            {
                throw new ArgumentNullException("settingsRepository");
            }
            this.settingsRepository = settingsRepository;
            this.logger = logger;
        }

        /// <summary>
        /// Gets the index page.
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Gets the clear cache page. This action currently clears the cache.
        /// </summary>
        /// <returns></returns>
        public ActionResult ClearCache()
        {
            var cacheProvider = DependencyResolver.Current.GetService<ICacheProvider>();
            List<string> filter = new List<string> { "Repositories" };

            UtilityCacheRepository cacheRepo = new UtilityCacheRepository(cacheProvider);
            cacheRepo.ClearCache(filter);

            return View();
        }

        /// <summary>
        /// Private repository class which extends the base caching repository; a bit of a hack so this controller
        /// can access methods in the abstract base caching repository
        /// </summary>
        private class UtilityCacheRepository : BaseCachingRepository
        {
            protected internal UtilityCacheRepository(ICacheProvider cacheProvider)
                : base(cacheProvider)
            {
            }
        }

        /// <summary>
        /// Backs up API config
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> BackupConfigAsync()
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
            await client.PostBackupApiConfigDataAsync().ConfigureAwait(false);
            return View();
        }

        /// <summary>
        /// Restores API config
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> RestoreConfigAsync()
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
            await client.PostRestoreApiConfigDataAsync().ConfigureAwait(false);
            return View();
        }        
    }
}
