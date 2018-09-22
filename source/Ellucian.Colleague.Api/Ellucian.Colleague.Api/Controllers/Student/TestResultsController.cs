// Copyright 2012-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Student Test Results data for Admissions, Placement and Other Tests
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class TestResultsController : BaseCompressedApiController
    {
        private readonly ITestResultRepository _testResultRepository;
        private readonly ITestResultService _testResultService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the TestsResultsController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="testResultService">Service for TestResult <see cref="ITestResultService">ITestResultService</see>/></param>
        /// <param name="testResultRepository">Repository of type <see cref="ITestResultRepository">ITestResultRepository</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public TestResultsController(IAdapterRegistry adapterRegistry,ITestResultService testResultService,
            ITestResultRepository testResultRepository, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _testResultRepository = testResultRepository;
            _testResultService = testResultService;
            _logger = logger;
        }

        /// <summary>
        /// Gets test results for a specific student.
        /// </summary>
        /// <param name="studentId">Student ID to retrieve test scores for</param>
        /// <param name="type">Type of test to select (admissions, placement, other).  If no type is provided all tests will be returned. </param>
        /// <returns>The <see cref="TestResult">Test Results</see> for the given student, limited to type requested.</returns>
        [Obsolete("Obsolete as of Api version 1.15, use version 2")]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Student.TestResult>> GetAsync(string studentId, string type = null)
        {
            try
            {
                return await _testResultService.GetAsync(studentId, type);
            }
            catch (PermissionsException pex)
            {
                throw CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                throw CreateHttpResponseException(e.Message);
            }
        }

        /// <summary>
        /// Gets test results for a specific student.
        /// </summary>
        /// <param name="studentId">Student ID to retrieve test scores for</param>
        /// <param name="type">Type of test to select (admissions, placement, other).  If no type is provided all tests will be returned. </param>
        /// <returns>The <see cref="TestResult2">Test Results</see> for the given student, limited to type requested.</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Student.TestResult2>> Get2Async(string studentId, string type = null)
        {
            try
            {
                return await _testResultService.Get2Async(studentId, type);
            }
            catch (PermissionsException pex)
            {
                throw CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                throw CreateHttpResponseException(e.Message);
            }
        }

        /// <summary>
        /// Gets all test results for a list of Student Ids
        /// </summary>
        /// <param name="criteria">DTO Object containing a list of student Ids and Type.</param>
        /// <returns>TestResults DTO Objects</returns>
        [Obsolete("Obsolete as of Api version 1.15, use version 2")]
        [HttpPost]   
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Student.TestResult>> QueryTestResultsAsync([FromBody] TestResultsQueryCriteria criteria)
        {
            IEnumerable<string> studentIds = criteria.StudentIds;
            string type = criteria.Type;

            try
            {
                return await _testResultService.GetTestResultsByIdsAsync(studentIds.ToArray(), type);
            }
            catch (PermissionsException pex)
            {
                throw CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                throw CreateHttpResponseException(e.Message);
            }
        }

        /// <summary>
        /// Gets all test results for a list of Student Ids
        /// </summary>
        /// <param name="criteria">DTO Object containing a list of student Ids and Type.</param>
        /// <returns>TestResults DTO Objects</returns>
        [HttpPost]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Student.TestResult2>> QueryTestResults2Async([FromBody] TestResultsQueryCriteria criteria)
        {
            IEnumerable<string> studentIds = criteria.StudentIds;
            string type = criteria.Type;

            try
            {
                return await _testResultService.GetTestResults2ByIdsAsync(studentIds.ToArray(), type);
            }
            catch (PermissionsException pex)
            {
                throw CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                throw CreateHttpResponseException(e.Message);
            }
        }
    }
}
