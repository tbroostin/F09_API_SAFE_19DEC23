// Copyright 2012-2022 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Ellucian.Colleague.Api.Areas.Planning.Models.Tests;
using Ellucian.Colleague.Api.Client;
using Ellucian.Colleague.Api.Models;
using Microsoft.IdentityModel.Claims;
using slf4net;
using System.Net;
using Ellucian.Web.Mvc.Filter;
using Ellucian.Web.Http.Configuration;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Ellucian.Logging;

namespace Ellucian.Colleague.Api.Areas.Planning.Controllers
{
    /// <summary>
    /// Provides the test utilities for the planning area.
    /// </summary>
    [LocalRequest]
    public class TestController : Controller
    {
        /// <summary>
        /// Person id string.
        /// </summary>
        public static string PersonIdKey = "PersonId";

        private StringLogger logger = new StringLogger();

        /// <summary>
        /// Gets the test index page.
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            var tests = new ApiTests();
            tests.Tests.Add("subjects");
            return View(tests);
        }

        /// <summary>
        /// Gets the test degree plan index page.
        /// </summary>
        /// <returns></returns>
        public ActionResult DegreePlan()
        {
            return View();
        }

        /// <summary>
        /// Submits a request to run the degree plan test.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult PostDegreePlan()
        {
            var sw = new Stopwatch();
            sw.Start();
            var results = CreateClient().GetOrCreateDegreePlanForStudent((string)HttpContext.Items[PersonIdKey]);
            sw.Stop();
            TempData["elapsed"] = sw.ElapsedMilliseconds;
            return RedirectToAction("Done");
        }

        /// <summary>
        /// Gets the test advisees index page.
        /// </summary>
        /// <returns></returns>
        public ActionResult Advisees()
        {
            return View();
        }

        /// <summary>
        /// Submits a request to run the advisees test.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult PostAdvisees()
        {
            var sw = new Stopwatch();
            sw.Start();
            var results = CreateClient().GetAdvisor((string)HttpContext.Items[PersonIdKey]);
            sw.Stop();
            TempData["elapsed"] = sw.ElapsedMilliseconds;
            return RedirectToAction("Done");
        }

        /// <summary>
        /// Gets the test evaluation page.
        /// </summary>
        /// <returns></returns>
        public ActionResult Evaluation()
        {
            return View();
        }

        /// <summary>
        /// Submits a request to run the evaluation test.
        /// </summary>
        /// <param name="evaluation"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Evaluation(TestEvaluation evaluation)
        {
            return Run(() =>
            {
                var result = CreateClient().GetProgramEvaluation((string)HttpContext.Items[PersonIdKey], evaluation.Program);
                return result.RequirementResults.Count + " requirement results";
            });
        }

        /// <summary>
        /// Gets the done page.
        /// </summary>
        /// <returns></returns>
        public ActionResult Done()
        {
            ViewBag.Result = TempData["result"];
            ViewBag.Elapsed = (long)TempData["elapsed"];
            var ex = (Exception) TempData["exception"];
            if (ex != null) 
            {
                ViewBag.ExceptionMessage = ex.ToString();
                ViewBag.Log = (string)TempData["log"];
            }
            return View();
        }


        private ColleagueApiClient CreateClient()
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
            var personId = (principal as IClaimsPrincipal).Identities.First().Claims.First(claim => claim.ClaimType == ClaimConstants.PersonId).Value;
            HttpContext.Items[PersonIdKey] = personId;
            var client = new ColleagueApiClient(baseUrl, logger);
            client.Credentials = token;
            return client;
        }

        private static List<string> GetDelimited(string input)
        {
            if (input == null)
            {
                return new List<string>();
            }
            if (input.Contains(','))
            {
                return input.Split(',').Where(s => !string.IsNullOrEmpty(s)).ToList();
            }
            return input.Split(' ').Where(s => !string.IsNullOrEmpty(s)).ToList();
        }

        private ActionResult Run(Func<string> action) {
            var sw = new Stopwatch();
            logger.Clear();
            sw.Start();
            string result = "";
            try
            {
                result = action.Invoke();
            }
            catch (Exception ex)
            {
                TempData["exception"] = ex;
            }
            sw.Stop();
            TempData["result"] = result;
            TempData["elapsed"] = sw.ElapsedMilliseconds;
            TempData["log"] = logger.ToString();
            logger.Clear();
            return RedirectToAction("Done");
        }
    }
}
