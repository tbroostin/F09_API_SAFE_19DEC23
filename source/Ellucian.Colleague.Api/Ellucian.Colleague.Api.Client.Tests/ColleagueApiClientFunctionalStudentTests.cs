// Copyright 2014-2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Client.Tests
{
    /// <summary>
    /// ColleagueApiClientFunctionalStudentTests is used to test the ColleagueApiClient against a Colleague Web API deployment.
    /// Many of the tests are dependent on the data in the environment being tested matching the tests.
    /// 
    /// Only put tests in here that use the student module or below (base)!
    /// 
    /// 1. DO NOT CHECK THIS FILE IN WITHOUT FIRST BLANKING OUT ANY SENSITIVE SYSTEM INFORMATION.
    /// 2. Make sure the Ignore keyword is not commented out on checkin.  These tests should not be allowed
    ///    to run as part of a "run all tests in solution" because actually hitting an environment for data is 
    ///    slower than test data.
    /// </summary>
    [TestClass]
    [Ignore]
    public class ColleagueApiClientFunctionalStudentTests
    {
        private ColleagueApiClient client;
        private ILogger logger;

        [TestInitialize]
        public void Initialize()
        {
            // Configuration for the environment to run functional tests against.
            string baseUrl = "http://localhost/Ellucian.Colleague.Api"; // Default URL for local dev deployment, change if necessary.
            //string userName = ""; // Configure own username and password based on environment.
            //string pass = "";       

            this.logger = new StringLogger();
            client = new ColleagueApiClient(baseUrl, this.logger);
            client.Timeout = new TimeSpan(0, 0, 30); // 30 second timeout for these tests
            //client.Credentials = client.Login(userName, pass, "ColleagueApiClientFunctionalTests");
        }

        [TestCleanup]
        public void TearDown()
        {
           client.Logout(client.Credentials);
        }

        #region Tests for Student module

        [TestMethod]
        public void TestGetSections4()
        {
            // Set up parameters for the function being tested.
            List<string> sectionIds = new List<string>() {"1", "2", "3", "4", "5"};
            
            // Call the client function with the parameters being tested.
            var result = client.GetSections4(sectionIds, false, true);

            // Check that the results are what's expected.
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(IEnumerable<Dtos.Student.Section3>));
            Assert.AreEqual(sectionIds.Count, result.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(Ellucian.Rest.Client.Exceptions.HttpRequestFailedException))]
        public void TestGetSections4EmptyIds()
        {
            List<string> sectionIds = new List<string>(); // Testing with empty list

            var result = client.GetSections4(sectionIds, false, true);
            // Should throw exception because we passed in empty list of IDs.
        }

        [TestMethod]
        public void TestGetAffiliations()
        {
            var result = client.GetAffiliations();

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(IEnumerable<Dtos.Student.Affiliation>));
            Assert.AreEqual(4, result.Count());
        }

        [TestMethod]
        public void TestQueryStudentAffiliations()
        {
            List<string> studentIds = new List<string>() { "0010655", "0003977", "0004241", "0010697" };
            string termId = "";
            string affiliationId = "";

            var result = client.QueryStudentAffiliations(studentIds, termId, affiliationId);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(IEnumerable<Dtos.Student.StudentAffiliation>));
            Assert.AreEqual(4, result.Count());
        }

        [TestMethod]
        public void TestSearchFacultyIds()
        {
            // Advisors only
            var result = client.SearchFacultyIds(false, true);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(IEnumerable<string>));
            Assert.AreEqual(374, result.Count());
            // Faculty only
            result = client.SearchFacultyIds(true, false);
            Assert.AreEqual(79, result.Count());
            // Faculty and Advisors
            result = client.SearchFacultyIds(false, false);
            Assert.AreEqual(453, result.Count());
        }

        [TestMethod]
        public void TestSearchStudentIds()
        {
            // Advisors only
            StudentQuery criteria = new StudentQuery();
            criteria.termId = "2014/FA";
            var result = client.SearchStudentIds(criteria);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(IEnumerable<string>));
            Assert.AreEqual(120, result.Count());
        }

        [TestMethod]
        public void TestGetAcademicHistoryLevelByIds()
        {
            List<string> studentIds = new List<string>() { "0010655", "0003977", "0004241", "0010697" };
            var result = client.GetAcademicHistoryLevelByIds(studentIds, true, false, null);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(IEnumerable<AcademicHistoryLevel>));
            Assert.AreEqual(6, result.Count());
        }

        [TestMethod]
        public void TestLoggingExceptionHandling()
        {
            // Set up parameters we're going to call these functions with.
            List<string> studentIds = new List<string>() { "0010655", "0003977", "0004241", "0010697" };

            // Call the functions (much more useful with a debugger running)
            try
            {
                client.GetPersonAddressesByIds(studentIds);
                client.GetPersonPhonesByIds(studentIds);
                client.GetStudentStandings(studentIds);
                client.GetStudentRestrictionsByStudentIds(studentIds);
                client.GetStudentTermsByStudentIds(studentIds);
                client.QueryStudentAffiliations(studentIds);
            }
            catch (Exception e)
            {
                logger.Info(e.GetBaseException().Message + e.GetBaseException().StackTrace);
            }
        }

        [TestMethod]
        public async Task Test_GetPersonPhonesByIdsAsync()
        {
            // Set up parameters we're going to call these functions with.
            List<string> studentIds = new List<string>() { "0013055", "0013056" };
            var result = await client.GetPersonPhonesByIdsAsync(studentIds);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(IEnumerable<PhoneNumber>));
            Assert.AreEqual(2, result.Count());
        }


        [TestMethod]
        public async Task Test_GetPilotPersonPhonesByIds()
        {
            // Set up parameters we're going to call these functions with.
            List<string> studentIds = new List<string>() { "0013055", "0013056" };
            var result = await client.GetPilotPersonPhonesByIds(studentIds);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(IEnumerable<PilotPhoneNumber>));
            Assert.AreEqual(2, result.Count());
        }

        [TestMethod]
        public void TestPilotStudentTermsGpas()
        {
            // Set up parameters we're going to call these functions with.
            List<string> studentIds = new List<string>() { "0003977", "0004241" };
            string term = "2014/FA";
            var result = client.GetPilotStudentTermsGpas(studentIds, term);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(IEnumerable<PilotStudentTermLevelGpa>));
            Assert.AreEqual(3, result.Count());
        }

        #endregion
    }
}
