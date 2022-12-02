// Copyright 2012-2022 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Ellucian.Colleague.Api.Areas.Student.Models.Tests;
using Ellucian.Colleague.Api.Client;
using Ellucian.Colleague.Api.Models;
using Microsoft.IdentityModel.Claims;
using slf4net;
using System.Net;
using Ellucian.Web.Mvc.Filter;
using Ellucian.Web.Http.Configuration;
using Ellucian.Web.Security;
using Ellucian.Logging;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Api.Areas.Student.Controllers
{
    /// <summary>
    /// Provides the test utilities for the student area.
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
        /// Gets the index page.
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            var tests = new ApiTests();
            tests.Tests.Add("subjects");
            return View(tests);
        }

        /// <summary>
        /// Gets the subject test index page.
        /// </summary>
        /// <returns></returns>
        public ActionResult Subjects()
        {
            return View();
        }

        /// <summary>
        /// Submits a subject test.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult PostSubjects()
        {
            var sw = new Stopwatch();
            sw.Start();
            var results = CreateClient().GetSubjects();
            sw.Stop();
            TempData["elapsed"] = sw.ElapsedMilliseconds;
            return RedirectToAction("Done");
        }

        /// <summary>
        /// Gets the sections for courses test index page.
        /// </summary>
        /// <returns></returns>
        public ActionResult SectionsForCourses()
        {
            return View();
        }

        /// <summary>
        /// Submits a request to test getting sections for pages.
        /// </summary>
        /// <param name="sectionsForCourses"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SectionsForCourses(TestSectionsForCourses sectionsForCourses)
        {
            var sw = new Stopwatch();
            sw.Start();
            var results = CreateClient().GetSectionsByCourse(GetDelimited(sectionsForCourses.CourseIds), sectionsForCourses.FromCache);
            sw.Stop();
            TempData["elapsed"] = sw.ElapsedMilliseconds;
            return RedirectToAction("Done");
        }

        /// <summary>
        /// Gets the test credits index page.
        /// </summary>
        /// <returns></returns>
        public ActionResult Credits()
        {
            return View();
        }

        /// <summary>
        /// Submits a request to run the credits tests.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult PostCredits()
        {
            var sw = new Stopwatch();
            sw.Start();
            var results = CreateClient().GetAcademicHistory2((string)HttpContext.Items[PersonIdKey]);
            sw.Stop();
            TempData["elapsed"] = sw.ElapsedMilliseconds;
            return RedirectToAction("Done");
        }

        /// <summary>
        /// Gets the test course search index page.
        /// </summary>
        /// <returns></returns>
        public ActionResult CourseSearch()
        {
            return View();
        }

        /// <summary>
        /// Submits a request to run the course search test.
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CourseSearch(TestCourseSearch search)
        {
            var sw = new Stopwatch();
            sw.Start();
            var emptyList = new List<string>();
            var results = CreateClient().SearchCourses(emptyList, emptyList, emptyList, emptyList, emptyList, emptyList, emptyList, emptyList, emptyList, emptyList, 
                null, null, search.Keywords, null, "", emptyList, emptyList, 10, 1);
            sw.Stop();
            TempData["elapsed"] = sw.ElapsedMilliseconds;
            return RedirectToAction("Done");
        }

        /// <summary>
        /// Gets the test course search parallel index page.
        /// </summary>
        /// <returns></returns>
        public ActionResult CourseSearchParallel()
        {
            return View();
        }

        /// <summary>
        /// Submits a request to run the course search parallel test.
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CourseSearchParallel(TestCourseSearch search)
        {
            var sw = new Stopwatch();
            sw.Start();
            var emptyList = new List<string>();
            Thread lastThread = null;
            for (int i = 0; i < 5; i++)
            {
                Thread t1 = new Thread(ExecuteCourseSearch);
                t1.Start(search.Keywords);
                lastThread = t1;
            }
            lastThread.Join();
            sw.Stop();
            TempData["elapsed"] = sw.ElapsedMilliseconds;
            return RedirectToAction("Done");
        }

        private void ExecuteCourseSearch(object keywords)
        {
            var emptyList = new List<string>();
            CreateClient().SearchCourses(emptyList, emptyList, emptyList, emptyList, emptyList, emptyList, emptyList, emptyList, emptyList, emptyList,
                null, null, (string)keywords, null, "", emptyList, emptyList, 10, 1);
        }

        /// <summary>
        /// Gets the test courses index page.
        /// </summary>
        /// <returns></returns>
        public ActionResult Courses()
        {
            return View();
        }

        /// <summary>
        /// Gets the test section index page.
        /// </summary>
        /// <returns></returns>
        public ActionResult Sections()
        {
            return View();
        }

        /// <summary>
        /// Submits a request to run the sections test.
        /// </summary>
        /// <param name="sections"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Sections(TestSections sections)
        {
            return Run(() =>
            {
                var results = CreateClient().GetSections3(GetDelimited(sections.SectionIds), sections.FromCache);
                return results.Count() + " sections returned";
            });
        }

        /// <summary>
        /// Gets the test faculty index page.
        /// </summary>
        /// <returns></returns>
        public ActionResult Faculty()
        {
            return View();
        }

        /// <summary>
        /// Submits a request to run the the faculty test.
        /// </summary>
        /// <param name="faculty"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Faculty(TestFaculty faculty)
        {
            var sw = new Stopwatch();
            sw.Start();
            var results = CreateClient().GetFaculty(faculty.FacultyIds.ElementAt(0).ToString());
            sw.Stop();
            TempData["elapsed"] = sw.ElapsedMilliseconds;
            return RedirectToAction("Done");
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
