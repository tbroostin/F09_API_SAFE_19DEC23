// Copyright 2012-2016 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using slf4net;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Web.Dependency;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using System.Threading.Tasks;
using System;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class TestResultService : StudentCoordinationService, ITestResultService
    {
        private readonly ITestResultRepository _testResultRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IConfigurationRepository _configurationRepository;
        private ILogger _logger;

        public TestResultService(IAdapterRegistry adapterRegistry, IStudentRepository studentRepository, ITestResultRepository testResultRepository, ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger, IConfigurationRepository configurationRepository)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, studentRepository, configurationRepository)
        {
            _configurationRepository = configurationRepository;
            _testResultRepository = testResultRepository;
            _studentRepository = studentRepository;
            _logger = logger;
        }

        /// <summary>
        /// Get test results for the specified students 
        /// </summary>
        /// <param name="studentIds">Student Ids</param>
        /// <param name="type">Type of test to select (admissions, placement, other).  If no type is provided all tests will be returned. </param>
        /// <returns>Test Results</returns>
        [Obsolete("Obsolete on version 1.15 of the Api. Use GetTestResults2ByIdsAsync.")]
        public async Task<IEnumerable<Dtos.Student.TestResult>> GetTestResultsByIdsAsync(string[] studentIds, string type)
        {
            ICollection<Dtos.Student.TestResult> testResultsDto = new List<Dtos.Student.TestResult>();
            if (!HasPermission(StudentPermissionCodes.ViewStudentInformation))
            {
                throw new PermissionsException("User does not have permissions to query students");
            }

            if (studentIds != null && studentIds.Count() > 0)
            {
                IEnumerable<Ellucian.Colleague.Domain.Student.Entities.TestResult> testResults = await _testResultRepository.GetTestResultsByIdsAsync(studentIds);
                if (type == "admissions")
                {
                    testResults = testResults.Where(tr => tr.Category == TestType.Admissions);
                }
                if (type == "placement")
                {
                    testResults = testResults.Where(tr => tr.Category == TestType.Placement);
                }
                if (type == "other")
                {
                    testResults = testResults.Where(tr => tr.Category == TestType.Other);
                }
                foreach (var testResult in testResults)
                {
                    if (!(testResult == null))
                    {
                        // Get the right adapter for the type mapping
                        var testResultDtoAdapter = _adapterRegistry.GetAdapter<TestResult, Ellucian.Colleague.Dtos.Student.TestResult>();

                        // Map the degree plan entity to the degree plan DTO
                        var testResultDto = testResultDtoAdapter.MapToType(testResult);

                        testResultsDto.Add(testResultDto);
                    }
                }
            }
            return testResultsDto;
        }

        /// <summary>
        /// Gets test score results for a student. You may optionally supply the types of scores requested.
        /// </summary>
        /// <param name="studentId">Id of the student for whom the scores are requested.</param>
        /// <param name="type">Type of score requests - admissions, placement, other. If no type is provided all test results for the student will be returned.</param>
        /// <returns></returns>
        [Obsolete("Obsolete on version 1.15 of the Api. Use Get2Async.")]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Student.TestResult>> GetAsync(string studentId, string type)
        {
            // First, make sure user has access to this student-- If not, method throws exception
           await CheckUserAccessAsync(studentId);

            ICollection<Dtos.Student.TestResult> testResultsDto = new List<Dtos.Student.TestResult>();
            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.TestResult> testResults = await _testResultRepository.GetAsync(studentId);
            if (type == "admissions")
            {
                testResults = testResults.Where(tr => tr.Category == TestType.Admissions);
            }
            if (type == "placement")
            {
                testResults = testResults.Where(tr => tr.Category == TestType.Placement);
            }
            if (type == "other")
            {
                testResults = testResults.Where(tr => tr.Category == TestType.Other);
            }
            // Get the right adapter for the type mapping
            var testResultDtoAdapter = _adapterRegistry.GetAdapter<TestResult, Ellucian.Colleague.Dtos.Student.TestResult>();
            foreach (var testResult in testResults)
            {
                // Map the degree plan entity to the degree plan DTO
                try
                {
                    var testResultDto = testResultDtoAdapter.MapToType(testResult);
                    testResultsDto.Add(testResultDto);
                }
                catch (Exception)
                {
                    
                    throw;
                }
                
            }

            return testResultsDto;
        }

        /// <summary>
        /// Get test results for the specified students 
        /// </summary>
        /// <param name="studentIds">Student Ids</param>
        /// <param name="type">Type of test to select (admissions, placement, other).  If no type is provided all tests will be returned. </param>
        /// <returns>Test Results</returns>
        public async Task<IEnumerable<Dtos.Student.TestResult2>> GetTestResults2ByIdsAsync(string[] studentIds, string type)
        {
            ICollection<Dtos.Student.TestResult2> testResultsDto = new List<Dtos.Student.TestResult2>();
            if (!HasPermission(StudentPermissionCodes.ViewStudentInformation))
            {
                throw new PermissionsException("User does not have permissions to query students");
            }

            if (studentIds != null && studentIds.Count() > 0)
            {
                IEnumerable<Ellucian.Colleague.Domain.Student.Entities.TestResult> testResults = await _testResultRepository.GetTestResultsByIdsAsync(studentIds);
                if (type == "admissions")
                {
                    testResults = testResults.Where(tr => tr.Category == TestType.Admissions);
                }
                if (type == "placement")
                {
                    testResults = testResults.Where(tr => tr.Category == TestType.Placement);
                }
                if (type == "other")
                {
                    testResults = testResults.Where(tr => tr.Category == TestType.Other);
                }
                foreach (var testResult in testResults)
                {
                    if (!(testResult == null))
                    {
                        // Get the right adapter for the type mapping
                        var testResultDtoAdapter = _adapterRegistry.GetAdapter<TestResult, Ellucian.Colleague.Dtos.Student.TestResult2>();

                        // Map the degree plan entity to the degree plan DTO
                        var testResultDto = testResultDtoAdapter.MapToType(testResult);

                        testResultsDto.Add(testResultDto);
                    }
                }
            }
            return testResultsDto;
        }

        /// <summary>
        /// Gets test score results for a student. You may optionally supply the types of scores requested.
        /// </summary>
        /// <param name="studentId">Id of the student for whom the scores are requested.</param>
        /// <param name="type">Type of score requests - admissions, placement, other. If no type is provided all test results for the student will be returned.</param>
        /// <returns>Test results</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Student.TestResult2>> Get2Async(string studentId, string type)
        {
            // First, make sure user has access to this student-- If not, method throws exception
            await CheckStudentAdvisorUserAccessAsync(studentId);

            ICollection<Dtos.Student.TestResult2> testResultsDto = new List<Dtos.Student.TestResult2>();
            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.TestResult> testResults = await _testResultRepository.GetAsync(studentId);
            if (type == "admissions")
            {
                testResults = testResults.Where(tr => tr.Category == TestType.Admissions);
            }
            if (type == "placement")
            {
                testResults = testResults.Where(tr => tr.Category == TestType.Placement);
            }
            if (type == "other")
            {
                testResults = testResults.Where(tr => tr.Category == TestType.Other);
            }
            // Get the right adapter for the type mapping
            var testResultDtoAdapter = _adapterRegistry.GetAdapter<TestResult, Ellucian.Colleague.Dtos.Student.TestResult2>();
            foreach (var testResult in testResults)
            {
                // Map the degree plan entity to the degree plan DTO
                var testResultDto = testResultDtoAdapter.MapToType(testResult);
                testResultsDto.Add(testResultDto);
            }

            return testResultsDto;
        }
    }
}
