// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System.Web.Mvc;
using Ellucian.Web.Mvc.Controller;
using slf4net;
using Ellucian.Dmi.Client.Das;
using Ellucian.Colleague.Api.Models;
using Ellucian.Web.Http.Configuration;
using System;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Home controller. Matches all requests for /.
    /// </summary>
    public class HomeController : BaseCompressedController
    {
        private ISettingsRepository settingsRepository;
        private ILogger logger;

        /// <summary>
        /// Initializes a new instance of the HomeController class.
        /// </summary>
        /// <param name="settingsRepository">ISettingsRepository instance</param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public HomeController(ISettingsRepository settingsRepository, ILogger logger)
            : base(logger)
        {
            if (settingsRepository == null)
            {
                throw new ArgumentNullException("settingsRepository");
            }
            this.settingsRepository = settingsRepository;
            this.logger = logger;
        }

        /// <summary>
        /// /index page
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            var domainSettings = settingsRepository.Get();
            ApiStatusModel apiStatus = new ApiStatusModel();
            apiStatus.UnSuccessfulLoginCounter = DasSession.DasUnsuccessfulLoginCounter;
            apiStatus.UseDasDataReader = domainSettings.ColleagueSettings.GeneralSettings.UseDasDatareader;
            return View(apiStatus);
        }

    }
}
