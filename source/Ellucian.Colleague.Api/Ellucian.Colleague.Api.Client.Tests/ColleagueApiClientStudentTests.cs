﻿// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.Base;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Colleague.Dtos.Student.Requirements;
using Ellucian.Rest.Client.Exceptions;
using Ellucian.Web.Http.TestUtil;
using Ellucian.Web.Infrastructure.TestUtil;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Client.Tests
{
    [TestClass]
    public class ColleagueApiClientStudentTests
    {
        [TestClass]
        public class CoursesAndSections
        {

            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _studentId = "123456";
            private const string _studentId2 = "678";
            private const string _token = "1234567890";
            private const string _courseId = "MATH-100";
            private const string _courseId2 = "ENGL-101";
            private const string _facultyId = "2222222";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public void GetCourse()
            {
                // Arrange
                var courseResponse = new Course()
                {
                    Id = _courseId,
                    SubjectCode = "MATH",
                    Title = "Mathematics"
                };

                var serializedResponse = JsonConvert.SerializeObject(courseResponse);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var result = client.GetCourse(_courseId);

                // Assert
                Assert.AreEqual(courseResponse.Id, result.Id);
                Assert.AreEqual(courseResponse.SubjectCode, result.SubjectCode);
                Assert.AreEqual(courseResponse.Title, result.Title);
            }


            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GetCourse_NullCourseId()
            {
                // Arrange
                var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent(string.Empty, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var result = client.GetCourse(null);
            }

            [TestMethod]
            public void GetCourse_EmptyResponse()
            {
                // Arrange
                var serializedResponse = JsonConvert.SerializeObject(string.Empty);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var result = client.GetCourse(_courseId);

                // Assert
                Assert.IsNull(result);
            }

            [TestMethod]
            public void SearchCourses()
            {
                // Arrange
                var simpleStringValue = "12345678";
                var simpleStringArgument = new List<string>() { simpleStringValue };
                var pageSize = 10;
                var pageIndex = 1;

                var courseResponse = new CourseSearch()
                {
                    Id = _courseId,
                    SubjectCode = "MATH",
                    Title = "Mathematics"
                };

                var courseListResponse = new List<CourseSearch>();
                courseListResponse.Add(courseResponse);

                var coursePageResponse = new CoursePage(courseListResponse, pageSize, pageIndex);
                coursePageResponse.AcademicLevels = new List<Filter>() {
                new Filter() {
                    Count = 0,
                    Selected = false,
                    Value = simpleStringValue
                }
            };

                var serializedResponse = JsonConvert.SerializeObject(coursePageResponse);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act

                var result = client.SearchCourses(simpleStringArgument, simpleStringArgument, simpleStringArgument, simpleStringArgument, simpleStringArgument, simpleStringArgument, simpleStringArgument, simpleStringArgument, simpleStringArgument, simpleStringArgument, null, null, string.Empty, null, string.Empty, simpleStringArgument, simpleStringArgument, pageSize, pageIndex);

                var firstCourse = result.CurrentPageItems.ElementAt(0);
                var academicLevelFilterValues = result.AcademicLevels.Select(x => x.Value).ToList();

                // Assert
                Assert.AreEqual(pageIndex, result.CurrentPageIndex);
                Assert.AreEqual(pageSize, result.PageSize);
                Assert.AreEqual(courseResponse.Id, firstCourse.Id);
                CollectionAssert.AreEquivalent(simpleStringArgument, academicLevelFilterValues);
            }

            [TestMethod]
            public void GetSectionsByCourse()
            {
                // Arrange
                var mathSection = new Section()
                {
                    Id = "01",
                    CourseId = _courseId,
                    Title = "Mathematics"
                };

                var courseListResponse = new List<Section>();
                courseListResponse.Add(mathSection);

                var courseId = mathSection.Id;

                var serializedResponse = JsonConvert.SerializeObject(courseListResponse);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var result = client.GetSectionsByCourse(new List<string>() { courseId });

                var retrievedMathSection = result.ElementAt(0);

                // Assert
                Assert.IsNotNull(result);
                Assert.AreEqual(mathSection.Id, retrievedMathSection.Id);
                Assert.AreEqual(mathSection.CourseId, retrievedMathSection.CourseId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GetSectionsByCourse_NullCourseIdList()
            {
                // Arrange
                var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent(string.Empty, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var result = client.GetSectionsByCourse(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GetSectionsByCourse_EmptyCourseId()
            {
                // Arrange
                var emptyId = string.Empty;
                var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent(string.Empty, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var result = client.GetSectionsByCourse(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GetSections_NullSectionIdList()
            {
                // Arrange
                var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent(string.Empty, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var result = client.GetSections(null, false);
            }

            [TestMethod]
            public void GetCourses()
            {
                // Arrange
                var courseIds = new List<string>() { "1", "2", "3" };
                var coursesResponse = new List<Course2>()
                {
                    new Course2()
                    {
                        Id = "1",
                        SubjectCode = "MATH",
                        Title = "Mathematics"
                    },
                    new Course2()
                    {
                        Id = "2",
                        SubjectCode = "ENGL",
                        Title = "English 1"
                    }
                };

                var serializedResponse = JsonConvert.SerializeObject(coursesResponse);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var criteria = new CourseQueryCriteria() { CourseIds = courseIds };
                var result = client.QueryCourses2(criteria);

                // Assert
                Assert.AreEqual(2, result.Count());
                Assert.IsTrue(result is IEnumerable<Course2>);

            }
            [TestMethod]
            public void GetFacultySections3()
            {
                // Arrange
                var mathSection = new Section3()
                {
                    Id = "01",
                    CourseId = _courseId,
                    Title = "Mathematics",
                    FacultyIds = new List<string>() { _facultyId }
                };

                var courseListResponse = new List<Section3>();
                courseListResponse.Add(mathSection);

                var courseId = mathSection.Id;
                var facultyId = mathSection.FacultyIds.First();

                var serializedResponse = JsonConvert.SerializeObject(courseListResponse);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var result = client.GetFacultySections3(facultyId);

                var retrievedMathSection = result.ElementAt(0);

                // Assert
                Assert.IsNotNull(result);
                Assert.AreEqual(mathSection.Id, retrievedMathSection.Id);
                Assert.AreEqual(mathSection.CourseId, retrievedMathSection.CourseId);
            }

            [TestMethod]
            public async Task GetFacultySections4_Cache()
            {
                // Arrange
                var mathSection = new Section3()
                {
                    Id = "01",
                    CourseId = _courseId,
                    Title = "Mathematics",
                    FacultyIds = new List<string>() { _facultyId }
                };

                var courseListResponse = new List<Section3>();
                courseListResponse.Add(mathSection);

                var courseId = mathSection.Id;
                var facultyId = mathSection.FacultyIds.First();

                var serializedResponse = JsonConvert.SerializeObject(courseListResponse);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var result = await client.GetFacultySections4Async(facultyId);

                var retrievedMathSection = result.ElementAt(0);

                // Assert
                Assert.IsNotNull(result);
                Assert.AreEqual(mathSection.Id, retrievedMathSection.Id);
                Assert.AreEqual(mathSection.CourseId, retrievedMathSection.CourseId);
            }

            [TestMethod]
            public async Task GetFacultySections4_NoCache()
            {
                // Arrange
                var mathSection = new Section3()
                {
                    Id = "01",
                    CourseId = _courseId,
                    Title = "Mathematics",
                    FacultyIds = new List<string>() { _facultyId }
                };

                var courseListResponse = new List<Section3>();
                courseListResponse.Add(mathSection);

                var courseId = mathSection.Id;
                var facultyId = mathSection.FacultyIds.First();

                var serializedResponse = JsonConvert.SerializeObject(courseListResponse);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var result = await client.GetFacultySections4Async(facultyId, useCache: false);

                var retrievedMathSection = result.ElementAt(0);

                // Assert
                Assert.IsNotNull(result);
                Assert.AreEqual(mathSection.Id, retrievedMathSection.Id);
                Assert.AreEqual(mathSection.CourseId, retrievedMathSection.CourseId);
            }

        }

        [TestClass]
        public class CheckRegistrationEligibility
        {

            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _studentId = "123456";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public void CheckRegistrationEligibilityResponse()
            {
                // Arrange
                var regElig = new RegistrationEligibility()
                {
                    Messages = new List<RegistrationMessage>()
                        {new RegistrationMessage() {Message = "eligibility message"}
                        },
                    IsEligible = true,
                    HasOverride = true
                };

                var serializedResponse = JsonConvert.SerializeObject(regElig);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = client.CheckRegistrationEligibility(_studentId);

                // Assert
                Assert.AreEqual(regElig.Messages.ElementAt(0).Message, clientResponse.Messages.ElementAt(0).Message);
                Assert.AreEqual(regElig.IsEligible, clientResponse.IsEligible);
                Assert.AreEqual(regElig.HasOverride, clientResponse.HasOverride);
            }
        }

        [TestClass]
        public class QueryFaculty
        {

            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _studentId = "123456";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public void QueryFaculty_ReturnsSerializedFaculty()
            {
                // Arrange
                var facultyDtos = new List<Faculty>()
                {
                    new Faculty() {Id = "0000001", LastName = "Smith"},
                    new Faculty() {Id = "0000011", LastName = "Jones"}
                };
                IEnumerable<string> facultyIds = facultyDtos.Select(f => f.Id);

                var serializedResponse = JsonConvert.SerializeObject(facultyDtos.AsEnumerable());

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var criteria = new FacultyQueryCriteria() { FacultyIds = facultyIds };
                var clientResponse = client.QueryFaculty(criteria);

                // Assert
                Assert.AreEqual(facultyDtos.Count(), clientResponse.Count());
                foreach (var id in facultyIds)
                {
                    var facultyDto = facultyDtos.Where(f => f.Id == id).First();
                    var facultyResponse = clientResponse.Where(f => f.Id == id).First();

                    Assert.AreEqual(facultyDto.Id, facultyResponse.Id);
                    Assert.AreEqual(facultyDto.LastName, facultyResponse.LastName);
                }
            }
        }

        [TestClass]
        public class QueryRequirements
        {

            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _studentId = "123456";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public void QueryRequirements_ReturnsSerializedRequirements()
            {
                // Arrange
                var requirementDtos = new List<Requirement>()
                {
                    new Requirement() {Id = "1", Code = "REQ1"},
                    new Requirement() {Id = "2", Code = "REQ2"}
                };
                List<string> requirementIds = requirementDtos.Select(r => r.Id).ToList();

                var serializedResponse = JsonConvert.SerializeObject(requirementDtos.AsEnumerable());

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = client.QueryRequirements(requirementIds);

                // Assert
                Assert.AreEqual(requirementDtos.Count(), clientResponse.Count());
                foreach (var id in requirementIds)
                {
                    var requirementDto = requirementDtos.Where(r => r.Id == id).First();
                    var requirementResponse = clientResponse.Where(r => r.Id == id).First();

                    Assert.AreEqual(requirementDto.Id, requirementResponse.Id);
                    Assert.AreEqual(requirementDto.Code, requirementResponse.Code);
                }
            }
        }

        [TestClass]
        public class AcademicHistory
        {

            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public async Task AcademicHistory_GetAcademicHistory3Async()
            {
                // Arrange
                var id = "0001234";
                var academicHistory3 = new AcademicHistory3()
                {
                    AcademicTerms = new List<AcademicTerm3>()
                        {
                             new AcademicTerm3(),
                             new AcademicTerm3()
                        },
                    NonTermAcademicCredits = new List<AcademicCredit2>()
                        {
                             new AcademicCredit2(),
                             new AcademicCredit2(),
                             new AcademicCredit2()
                        },
                    GradeRestriction = new GradeRestriction(),
                    TotalCreditsCompleted = 3,
                    OverallGradePointAverage = (decimal)3.4,
                    StudentId = id
                };

                var serializedResponse = JsonConvert.SerializeObject(academicHistory3);
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);
                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);
                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetAcademicHistory3Async(id);

                //Assert
                Assert.IsInstanceOfType(clientResponse, typeof(AcademicHistory3));
                Assert.AreEqual(id, clientResponse.StudentId);
                Assert.AreEqual(academicHistory3.AcademicTerms.Count(), clientResponse.AcademicTerms.Count());
                Assert.AreEqual(academicHistory3.NonTermAcademicCredits.Count(), clientResponse.NonTermAcademicCredits.Count());
                Assert.AreEqual(academicHistory3.TotalCreditsCompleted, clientResponse.TotalCreditsCompleted);
                Assert.AreEqual(academicHistory3.OverallGradePointAverage, clientResponse.OverallGradePointAverage);
                Assert.AreEqual((decimal)3.4, clientResponse.OverallGradePointAverage);
            }

            [TestMethod]
            public async Task AcademicHistory_GetAcademicHistory4Async()
            {
                // Arrange
                var id = "0001234";
                var academicHistory3 = new AcademicHistory4()
                {
                    AcademicTerms = new List<AcademicTerm4>()
                        {
                             new AcademicTerm4(),
                             new AcademicTerm4()
                        },
                    NonTermAcademicCredits = new List<AcademicCredit3>()
                        {
                             new AcademicCredit3(),
                             new AcademicCredit3(),
                             new AcademicCredit3()
                        },
                    GradeRestriction = new GradeRestriction(),
                    TotalCreditsCompleted = 3,
                    OverallGradePointAverage = (decimal)3.4,
                    StudentId = id
                };

                var serializedResponse = JsonConvert.SerializeObject(academicHistory3);
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);
                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);
                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetAcademicHistory4Async(id);

                //Assert
                Assert.IsInstanceOfType(clientResponse, typeof(AcademicHistory4));
                Assert.AreEqual(id, clientResponse.StudentId);
                Assert.AreEqual(academicHistory3.AcademicTerms.Count(), clientResponse.AcademicTerms.Count());
                Assert.AreEqual(academicHistory3.NonTermAcademicCredits.Count(), clientResponse.NonTermAcademicCredits.Count());
                Assert.AreEqual(academicHistory3.TotalCreditsCompleted, clientResponse.TotalCreditsCompleted);
                Assert.AreEqual(academicHistory3.OverallGradePointAverage, clientResponse.OverallGradePointAverage);
                Assert.AreEqual((decimal)3.4, clientResponse.OverallGradePointAverage);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpRequestFailedException))]
            public async Task AcademicHistory_GetAcademicHistory4Async_BadRequest()
            {
                // Arrange
                var id = "0001234";

                var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent(string.Empty, Encoding.UTF8, _contentType);
                response.RequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://test");
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetAcademicHistory4Async(id);
            }
        }

        [TestClass]
        public class GetFacultyPermissions
        {

            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public void GetFacultyPermissions_ReturnsSerializedPermissions()
            {
                // Arrange
                var permissions = new List<string>()
                {
                    "Permission1",
                    "Permission2"
                };

                var serializedResponse = JsonConvert.SerializeObject(permissions.AsEnumerable());

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = client.GetFacultyPermissions();

                // Assert
                Assert.AreEqual(permissions.Count(), clientResponse.Count());
                foreach (var item in permissions)
                {
                    Assert.IsTrue(clientResponse.Contains(item));
                }
            }
        }

        [TestClass]
        public class GetSectionWaivers
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public void Client_GetSectionStudentWaivers_ReturnsSerializedWaivers()
            {
                // Arrange
                var waivers = new List<Dtos.Student.StudentWaiver>()
                {
                    new StudentWaiver() {Id = "1", StudentId = "STU1", SectionId = "SEC+1", ReasonCode = "OTH", RequisiteWaivers = new List<RequisiteWaiver>() {new RequisiteWaiver() {RequisiteId = "R1", Status = WaiverStatus.Waived}}},
                    new StudentWaiver() {Id = "2", StudentId = "STU2", SectionId = "SEC+1", ReasonCode = "OTH", RequisiteWaivers = new List<RequisiteWaiver>()}
                };

                var serializedResponse = JsonConvert.SerializeObject(waivers.AsEnumerable());

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = client.GetSectionStudentWaivers("SEC+1");

                // Assert that the expected number of items is returned and each of the expected items is found in the response
                Assert.AreEqual(waivers.Count(), clientResponse.Count());
                foreach (var item in waivers)
                {
                    var waiver = clientResponse.Where(w => w.Id == item.Id).FirstOrDefault();
                    Assert.IsNotNull(waiver);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Client_GetSectionStudentWaivers_ThrowsExceptionForNullSectionId()
            {
                // Arrange
                var mockHandler = new MockHandler();
                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = client.GetSectionStudentWaivers("");
            }
        }

        [TestClass]
        public class AddWaiver
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public void Client_AddWaiver_ReturnsNewWaiver()
            {
                // Arrange
                var waiver = new Dtos.Student.StudentWaiver()
                {
                    StudentId = "STU1",
                    SectionId = "SEC1",
                    ReasonCode = "OTH",
                    RequisiteWaivers = new List<RequisiteWaiver>()
                        {
                            new RequisiteWaiver() {RequisiteId = "R1", Status = WaiverStatus.Waived}
                        }
                };

                var serializedResponse = JsonConvert.SerializeObject(waiver);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = client.AddStudentWaiver(waiver);

                // Assert that the expected item is found in the response
                Assert.AreEqual(waiver.ReasonCode, clientResponse.ReasonCode);
                Assert.AreEqual(waiver.StudentId, clientResponse.StudentId);
                Assert.AreEqual(waiver.SectionId, clientResponse.SectionId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Client_AddWaiver_ThrowsExceptionForNullWaiver()
            {
                // Arrange
                var mockHandler = new MockHandler();
                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = client.AddStudentWaiver(null);
            }
        }

        [TestClass]
        public class GetStudentWaiverReasons
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public void Client_GetStudentWaiverReasons_ReturnsSerializedWaiverReasons()
            {
                // Arrange
                var waiverReasons = new List<Dtos.Student.StudentWaiverReason>()
                {
                    new StudentWaiverReason() {Code = "LIFE", Description = "Life Experience"},
                    new StudentWaiverReason() {Code = "OTHER", Description = "Other reason"}
                };

                var serializedResponse = JsonConvert.SerializeObject(waiverReasons.AsEnumerable());

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = client.GetStudentWaiverReasons();

                // Assert that the expected number of items is returned and each of the expected items is found in the response
                Assert.AreEqual(waiverReasons.Count(), clientResponse.Count());
                foreach (var item in waiverReasons)
                {
                    var waiverReason = clientResponse.Where(w => w.Code == item.Code).FirstOrDefault();
                    Assert.IsNotNull(waiverReason);
                    Assert.AreEqual(item.Code, waiverReason.Code);
                    Assert.AreEqual(item.Description, waiverReason.Description);
                }
            }
        }

        [TestClass]
        public class GetStudentWaiver
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public void Client_GetStudentWaiver_ReturnsSerializedWaiver()
            {
                // Arrange
                var waiver = new Dtos.Student.StudentWaiver()
                {
                    Id = "2",
                    StudentId = "STU1",
                    SectionId = "SEC+1",
                    ReasonCode = "OTH",
                    RequisiteWaivers = new List<RequisiteWaiver>() { new RequisiteWaiver() { RequisiteId = "R1", Status = WaiverStatus.Waived } }
                };

                var serializedResponse = JsonConvert.SerializeObject(waiver);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = client.GetStudentWaiver("STU1", "2");

                // Assert that the expected item is returned
                Assert.IsNotNull(clientResponse);
                Assert.AreEqual(waiver.Id, clientResponse.Id);
                Assert.AreEqual(waiver.StudentId, clientResponse.StudentId);
                Assert.AreEqual(waiver.SectionId, clientResponse.SectionId);
                Assert.AreEqual(waiver.RequisiteWaivers.ElementAt(0).RequisiteId, clientResponse.RequisiteWaivers.ElementAt(0).RequisiteId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Client_GetWaiver_ThrowsExceptionForNullWaiverId()
            {
                // Arrange
                var mockHandler = new MockHandler();
                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = client.GetStudentWaiver(null, null);
            }
        }

        [TestClass]
        public class QuerySectionRegistrationDates
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public void Client_GetSectionRegistrationDates_ReturnsSerializedDateObject()
            {
                // Arrange
                var dates = new List<Dtos.Student.SectionRegistrationDate>();
                var sectionRegistrationDate = new Dtos.Student.SectionRegistrationDate()
                {
                    SectionId = "section1",
                    RegistrationStartDate = DateTime.Today,
                    RegistrationEndDate = DateTime.Today.AddDays(1),
                    PreRegistrationStartDate = DateTime.Today.AddDays(2)
                };
                dates.Add(sectionRegistrationDate);
                var sectionIds = dates.Select(d => d.SectionId);
                var serializedResponse = JsonConvert.SerializeObject(dates);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = client.GetSectionRegistrationDates(sectionIds);

                // Assert
                Assert.AreEqual(sectionIds.Count(), clientResponse.Count());
                foreach (var date in dates)
                {
                    var sectionRegDate = clientResponse.Where(s => s.SectionId == date.SectionId).First();
                    Assert.AreEqual(date.RegistrationStartDate, sectionRegDate.RegistrationStartDate);
                    Assert.AreEqual(date.RegistrationEndDate, sectionRegDate.RegistrationEndDate);
                    Assert.AreEqual(date.PreRegistrationStartDate, sectionRegDate.PreRegistrationStartDate);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Client_QuerySectionRegistrationDates_ThrowsExceptionForNullSectionIds()
            {
                // Arrange
                var mockHandler = new MockHandler();
                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = client.GetSectionRegistrationDates(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Client_QuerySectionRegistrationDates_ThrowsExceptionForEmptySectionIds()
            {
                // Arrange
                var mockHandler = new MockHandler();
                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = client.GetSectionRegistrationDates(new List<string>());
            }
        }

        [TestClass]
        public class GetSectionPermissions
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public async Task ClientGetSectionPermissions_ReturnsSerializedSectionPermissions()
            {
                // Arrange
                var sectionPermission = new SectionPermission();
                sectionPermission.FacultyConsents = new List<StudentPetition>()
                {
                    new StudentPetition(){Id="1",SectionId="19143"},
                     new StudentPetition(){Id="2",SectionId="19143"},
                };
                sectionPermission.StudentPetitions = new List<StudentPetition>()
                {
                    new StudentPetition(){Id="3",SectionId="19143"},
                     new StudentPetition(){Id="1",SectionId="19143"},
                };
                var serializedResponse = JsonConvert.SerializeObject(sectionPermission);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetSectionPermissionsAsync("19143");

                // Assert that the expected number of items is returned and each of the expected items is found in the response
                Assert.IsNotNull(clientResponse);
                //Assert.IsNotNull(clientResponse.FacultyConsents);
                //Assert.IsNotNull(clientResponse.StudentPetitions);
                //Assert.AreEqual(sectionPermission.FacultyConsents.Count(), clientResponse.FacultyConsents.Count());
                //Assert.AreEqual(sectionPermission.StudentPetitions.Count(), clientResponse.StudentPetitions.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Client_GetSectionStudentWaivers_ThrowsExceptionForNullSectionId()
            {
                // Arrange
                var mockHandler = new MockHandler();
                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = client.GetSectionStudentWaivers("");
            }
        }

        [TestClass]
        public class GetPetitionStatuses
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public void Client_GetPetitionStatuses_ReturnsSerializedPetitionStatuses()
            {
                // Arrange
                var petitionStatuses = new List<Dtos.Student.PetitionStatus>()
                {
                    new PetitionStatus() {Code = "ACC", Description = "Accepted", IsGranted = true},
                    new PetitionStatus() {Code = "OTHER", Description = "Other reason"}
                };

                var serializedResponse = JsonConvert.SerializeObject(petitionStatuses.AsEnumerable());

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = client.GetPetitionStatuses();

                // Assert that the expected number of items is returned and each of the expected items is found in the response
                Assert.AreEqual(petitionStatuses.Count(), clientResponse.Count());
                foreach (var item in petitionStatuses)
                {
                    var status = clientResponse.Where(w => w.Code == item.Code).FirstOrDefault();
                    Assert.IsNotNull(status);
                    Assert.AreEqual(item.Code, status.Code);
                    Assert.AreEqual(item.Description, status.Description);
                }
            }
        }

        [TestClass]
        public class GetStudentPetitionReasons
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public void Client_GetStudentPetitionReasons_ReturnsSerializedStudentPetitionReasons()
            {
                // Arrange
                var studentPetitionReasons = new List<Dtos.Student.StudentPetitionReason>()
                {
                    new StudentPetitionReason() {Code = "ICJI", Description = "I can handle it"},
                    new StudentPetitionReason() {Code = "OVMH", Description = "Over my head"}
                };

                var serializedResponse = JsonConvert.SerializeObject(studentPetitionReasons.AsEnumerable());

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = client.GetStudentPetitionReasons();

                // Assert that the expected number of items is returned and each of the expected items is found in the response
                Assert.AreEqual(studentPetitionReasons.Count(), clientResponse.Count());
                foreach (var item in studentPetitionReasons)
                {
                    var studentPetitionReason = clientResponse.Where(w => w.Code == item.Code).FirstOrDefault();
                    Assert.IsNotNull(studentPetitionReason);
                    Assert.AreEqual(item.Code, studentPetitionReason.Code);
                    Assert.AreEqual(item.Description, studentPetitionReason.Description);
                }
            }
        }

        [TestClass]
        public class AddStudentPetition
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public void Client_AddStudentPetition_ReturnsNewPetition()
            {
                // Arrange
                var studentPetition = new Dtos.Student.StudentPetition()
                {
                    StudentId = "STU1",
                    SectionId = "SEC1",
                    ReasonCode = "OTH",
                    StatusCode = "A",
                    Type = Dtos.Student.StudentPetitionType.FacultyConsent,
                    DateTimeChanged = DateTime.Now,
                    UpdatedBy = "1111111",
                    Id = "9999"

                };

                var serializedResponse = JsonConvert.SerializeObject(studentPetition);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = client.AddStudentPetition(studentPetition);

                // Assert that the expected item is found in the response
                Assert.AreEqual(studentPetition.ReasonCode, clientResponse.ReasonCode);
                Assert.AreEqual(studentPetition.StudentId, clientResponse.StudentId);
                Assert.AreEqual(studentPetition.SectionId, clientResponse.SectionId);
                Assert.AreEqual(studentPetition.StatusCode, clientResponse.StatusCode);
                Assert.AreEqual(studentPetition.UpdatedBy, clientResponse.UpdatedBy);
                Assert.AreEqual(studentPetition.DateTimeChanged, clientResponse.DateTimeChanged);
                Assert.AreEqual(studentPetition.Type, clientResponse.Type);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Client_AddStudentPetition_ThrowsExceptionForNullPetition()
            {
                // Arrange
                var mockHandler = new MockHandler();
                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = client.AddStudentPetition(null);
            }
        }

        [TestClass]
        public class GetStudentPetition
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public void Client_GetStudentPetition_ReturnsSerializedStudentPetition()
            {
                // Arrange
                var petitionId = "9999";
                var sectionId = "SectionId";
                StudentPetitionType type = StudentPetitionType.FacultyConsent;
                var studentPetition = new Dtos.Student.StudentPetition()
                {
                    StudentId = "STU1",
                    SectionId = sectionId,
                    ReasonCode = "OTH",
                    StatusCode = "A",
                    Type = type,
                    DateTimeChanged = DateTime.Now,
                    UpdatedBy = "1111111",
                    Id = petitionId
                };

                var serializedResponse = JsonConvert.SerializeObject(studentPetition);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = client.GetStudentPetition(petitionId, sectionId, type);

                // Assert that the expected item is found in the response
                Assert.AreEqual(studentPetition.ReasonCode, clientResponse.ReasonCode);
                Assert.AreEqual(studentPetition.StudentId, clientResponse.StudentId);
                Assert.AreEqual(studentPetition.SectionId, clientResponse.SectionId);
                Assert.AreEqual(studentPetition.StatusCode, clientResponse.StatusCode);
                Assert.AreEqual(studentPetition.UpdatedBy, clientResponse.UpdatedBy);
                Assert.AreEqual(studentPetition.DateTimeChanged, clientResponse.DateTimeChanged);
                Assert.AreEqual(studentPetition.Type, clientResponse.Type);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Client_GetStudentPetition_ThrowsExceptionForNullPetitionId()
            {
                // Arrange
                var sectionId = "SectionId";
                StudentPetitionType type = StudentPetitionType.FacultyConsent;
                var mockHandler = new MockHandler();
                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = client.GetStudentPetition(null, sectionId, type);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Client_GetStudentPetition_ThrowsExceptionForNullSectionId()
            {
                // Arrange
                var petitionId = "9999";
                StudentPetitionType type = StudentPetitionType.FacultyConsent;
                var mockHandler = new MockHandler();
                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = client.GetStudentPetition(petitionId, null, type);
            }

            [TestMethod]
            public async Task Client_GetStudentPetition_ReturnsSerializedStudentPetitions()
            {
                // Arrange
                List<StudentPetition> petitions = new List<StudentPetition>();
                StudentPetitionType type = StudentPetitionType.FacultyConsent;

                var studentPetition = new Dtos.Student.StudentPetition()
                {
                    StudentId = "STU1",
                    SectionId = "SectionId",
                    ReasonCode = "OTH",
                    StatusCode = "A",
                    Type = type,
                    DateTimeChanged = DateTime.Now,
                    UpdatedBy = "1111111",
                    Id = "9999"
                };
                petitions.Add(studentPetition);
                studentPetition = new Dtos.Student.StudentPetition()
                {
                    StudentId = "STU1",
                    SectionId = "SectionId",
                    ReasonCode = "OTH",
                    StatusCode = "A",
                    Type = StudentPetitionType.StudentPetition,
                    DateTimeChanged = DateTime.Now,
                    UpdatedBy = "1111111",
                    Id = "999999"
                };
                petitions.Add(studentPetition);
                var serializedResponse = JsonConvert.SerializeObject(petitions);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = (await client.GetStudentPetitionsAsync("STU1")).ToList();

                // Assert that the expected item is found in the response
                Assert.AreEqual(petitions.Count(), clientResponse.Count());
                Assert.AreEqual(petitions[0].ReasonCode, clientResponse[0].ReasonCode);
                Assert.AreEqual(petitions[0].StudentId, clientResponse[0].StudentId);
                Assert.AreEqual(petitions[0].SectionId, clientResponse[0].SectionId);
                Assert.AreEqual(petitions[0].StatusCode, clientResponse[0].StatusCode);
                Assert.AreEqual(petitions[0].UpdatedBy, clientResponse[0].UpdatedBy);
                Assert.AreEqual(petitions[0].DateTimeChanged, clientResponse[0].DateTimeChanged);
                Assert.AreEqual(petitions[0].Type, clientResponse[0].Type);
            }
        }

        [TestClass]
        public class GetGraduationConfigurationAsync
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public async Task ClientGetGraduationConfigurationAsync_ReturnsSerializedGraduationConfiguration()
            {
                // Arrange
                var graduationConfiguration = new GraduationConfiguration();
                graduationConfiguration.ApplyForDifferentProgramLink = "www.differentprogram.com/abc";
                graduationConfiguration.CapAndGownLink = "http:\\capandgown.com";
                graduationConfiguration.CommencementInformationLink = "someCommencementLinkText.com";
                graduationConfiguration.MaximumCommencementGuests = 5;
                graduationConfiguration.PhoneticSpellingLink = "phoneticspelling.com";
                graduationConfiguration.ApplicationQuestions = new List<GraduationQuestion>()
                {
                    new GraduationQuestion() {Type = GraduationQuestionType.DiplomaName, IsRequired = false},
                     new GraduationQuestion(){Type = GraduationQuestionType.AttendCommencement, IsRequired = true},
                };
                graduationConfiguration.GraduationTerms = new List<string>()
                {
                    "2016/SP",
                     "2016/FA"
                };
                var serializedResponse = JsonConvert.SerializeObject(graduationConfiguration);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetGraduationConfigurationAsync();

                // Assert that the expected number of items is returned and each of the expected items is found in the response
                Assert.IsNotNull(clientResponse);
                Assert.IsNotNull(clientResponse.CommencementInformationLink);
                Assert.IsNotNull(clientResponse.ApplyForDifferentProgramLink);
                Assert.IsNotNull(clientResponse.CapAndGownLink);
                Assert.IsNotNull(clientResponse.PhoneticSpellingLink);
                Assert.IsNotNull(clientResponse.MaximumCommencementGuests);
                Assert.AreEqual(graduationConfiguration.ApplicationQuestions.Count(), clientResponse.ApplicationQuestions.Count());
                Assert.AreEqual(graduationConfiguration.GraduationTerms.Count(), clientResponse.GraduationTerms.Count());
            }

        }

        [TestClass]
        public class GetCapSizesAsync
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public async Task ClientGetCapSizesAsync_ReturnsSerializedCapSizes()
            {
                // Arrange
                var capSizes = new List<CapSize>()
                    {
                        new CapSize(){Code="SMALL",Description="Small"},
                        new CapSize(){Code="MEDIUM",Description="Meduium"},
                        new CapSize(){Code="LARGE",Description="Large"}
                    };
                var serializedResponse = JsonConvert.SerializeObject(capSizes);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetCapSizesAsync();

                // Assert that the expected number of items is returned and each of the expected items is found in the response
                Assert.IsNotNull(clientResponse);
                Assert.AreEqual(capSizes.Count(), clientResponse.Count());
                foreach (var cap in clientResponse)
                {
                    Assert.IsNotNull(cap.Code);
                    Assert.IsNotNull(cap.Description);
                    var capSize = capSizes.Where(c => c.Code == cap.Code).FirstOrDefault();
                    Assert.AreEqual(capSize.Code, cap.Code);
                    Assert.AreEqual(capSize.Description, cap.Description);
                }
            }

        }

        [TestClass]
        public class GetGownSizesAsync
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public async Task ClientGetGownSizesAsync_ReturnsSerializedGownSizes()
            {
                // Arrange
                var gownSizes = new List<GownSize>()
                    {
                        new GownSize(){Code="SMALL",Description="Small"},
                        new GownSize(){Code="MEDIUM",Description="Meduium"},
                        new GownSize(){Code="LARGE",Description="Large"}
                    };
                var serializedResponse = JsonConvert.SerializeObject(gownSizes);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetGownSizesAsync();

                // Assert that the expected number of items is returned and each of the expected items is found in the response
                Assert.IsNotNull(clientResponse);
                Assert.AreEqual(gownSizes.Count(), clientResponse.Count());
                foreach (var gown in clientResponse)
                {
                    Assert.IsNotNull(gown.Code);
                    Assert.IsNotNull(gown.Description);
                    var gownSize = gownSizes.Where(c => c.Code == gown.Code).FirstOrDefault();
                    Assert.AreEqual(gownSize.Code, gown.Code);
                    Assert.AreEqual(gownSize.Description, gown.Description);
                }
            }

        }


        [TestClass]
        public class GetGraduationApplicationAsync
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public async Task GetGraduationApplicationAsync_ReturnsSerializedGraduationApplication()
            {
                // Arrange
                var graduationApplication = new GraduationApplication();
                graduationApplication.StudentId = "000011";
                graduationApplication.ProgramCode = "MATH.BA";
                graduationApplication.GraduationTerm = "2015/FA";

                var serializedGraduationApplication = JsonConvert.SerializeObject(graduationApplication);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedGraduationApplication, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.RetrieveGraduationApplicationAsync("000011", "MATH.BA");

                // Assert that the expected number of items is returned and each of the expected items is found in the response
                Assert.IsNotNull(clientResponse);
                Assert.AreEqual(graduationApplication.StudentId, clientResponse.StudentId);
                Assert.AreEqual(graduationApplication.ProgramCode, clientResponse.ProgramCode);
            }

            [TestMethod]
            public async Task GetGraduationApplicationAsync_ReturnsSerializedGraduationApplications()
            {
                // Arrange
                var graduationApplications = new List<GraduationApplication>()
                    {
                        new GraduationApplication(){StudentId="000011",ProgramCode="MATH.BA", GraduationTerm="2015/FA"},
                        new GraduationApplication(){StudentId="000011",ProgramCode="ENGL.BA", GraduationTerm="2016/FA"},
                        new GraduationApplication(){StudentId="000011",ProgramCode="ACCT.BA", GraduationTerm="2017/FA"}
                    };
                var serializedResponse = JsonConvert.SerializeObject(graduationApplications);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.RetrieveGraduationApplicationsAsync("000011");

                // Assert that the expected number of items is returned and each of the expected items is found in the response
                Assert.IsNotNull(clientResponse);
                Assert.AreEqual(graduationApplications.Count(), clientResponse.Count());
                foreach (var application in clientResponse)
                {
                    Assert.IsNotNull(application.StudentId);
                    Assert.IsNotNull(application.ProgramCode);
                    var gradApp = graduationApplications.Where(c => c.StudentId == application.StudentId && c.ProgramCode == application.ProgramCode).FirstOrDefault();
                    Assert.AreEqual(gradApp.StudentId, application.StudentId);
                    Assert.AreEqual(gradApp.ProgramCode, application.ProgramCode);
                }
            }

        }

        [TestClass]
        public class UpdateGraduationApplicationAsync
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public async Task UpdateGraduationApplicationAsync_ReturnsSerializedGraduationApplication()
            {
                // Arrange
                var graduationApplication = new GraduationApplication();
                graduationApplication.StudentId = "StudentId";
                graduationApplication.ProgramCode = "ProgramCode";
                graduationApplication.GraduationTerm = "TermCode";

                var serializedGraduationApplication = JsonConvert.SerializeObject(graduationApplication);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedGraduationApplication, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.UpdateGraduationApplicationAsync(graduationApplication);

                // Assert that the expected number of items is returned and each of the expected items is found in the response
                Assert.IsNotNull(clientResponse);
                Assert.AreEqual(graduationApplication.StudentId, clientResponse.StudentId);
                Assert.AreEqual(graduationApplication.ProgramCode, clientResponse.ProgramCode);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task UpdateGraduationApplicationAsync_ApplicationNull()
            {
                // Arrange
                var mockHandler = new MockHandler();

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.UpdateGraduationApplicationAsync(null);

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task UpdateGraduationApplicationAsync_ApplicationHasEmptyStudentId()
            {
                // Arrange
                var graduationApplication = new GraduationApplication();
                graduationApplication.StudentId = string.Empty;
                graduationApplication.ProgramCode = "ProgramCode";
                graduationApplication.GraduationTerm = "TermCode";
                var mockHandler = new MockHandler();

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.UpdateGraduationApplicationAsync(graduationApplication);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task UpdateGraduationApplicationAsync_ApplicationHasNullStudentId()
            {
                // Arrange
                var graduationApplication = new GraduationApplication();
                graduationApplication.StudentId = null;
                graduationApplication.ProgramCode = "ProgramCode";
                graduationApplication.GraduationTerm = "TermCode";
                var mockHandler = new MockHandler();

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.UpdateGraduationApplicationAsync(graduationApplication);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task UpdateGraduationApplicationAsync_ApplicationHasEmptyProgramCode()
            {
                // Arrange
                var graduationApplication = new GraduationApplication();
                graduationApplication.StudentId = "StudentId";
                graduationApplication.ProgramCode = string.Empty;
                graduationApplication.GraduationTerm = "TermCode";
                var mockHandler = new MockHandler();

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.UpdateGraduationApplicationAsync(graduationApplication);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task UpdateGraduationApplicationAsync_ApplicationHasNullProgramCode()
            {
                // Arrange
                var graduationApplication = new GraduationApplication();
                graduationApplication.StudentId = "StudentId";
                graduationApplication.ProgramCode = null;
                graduationApplication.GraduationTerm = "TermCode";
                var mockHandler = new MockHandler();

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.UpdateGraduationApplicationAsync(graduationApplication);
            }
        }

        [TestClass]
        public class GetGraduationApplicationFeeAsync
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public async Task GetGraduationApplicationFeeAsync_ReturnsSerializedGraduationApplicationFee()
            {
                // Arrange
                var graduationApplicationFee = new GraduationApplicationFee();
                graduationApplicationFee.StudentId = "000011";
                graduationApplicationFee.ProgramCode = "MATH.BA";
                graduationApplicationFee.PaymentDistributionCode = "DIST";
                graduationApplicationFee.Amount = 30m;

                var serializedGraduationApplication = JsonConvert.SerializeObject(graduationApplicationFee);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedGraduationApplication, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetGraduationApplicationFeeAsync("000011", "MATH.BA");

                // Assert that the expected number of items is returned and each of the expected items is found in the response
                Assert.IsNotNull(clientResponse);
                Assert.AreEqual(graduationApplicationFee.StudentId, clientResponse.StudentId);
                Assert.AreEqual(graduationApplicationFee.ProgramCode, clientResponse.ProgramCode);
                Assert.AreEqual(graduationApplicationFee.Amount, clientResponse.Amount);
                Assert.AreEqual(graduationApplicationFee.PaymentDistributionCode, clientResponse.PaymentDistributionCode);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetGraduationApplicationFeeAsync_NullStudentId()
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetGraduationApplicationFeeAsync(null, "MATH.BA");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetGraduationApplicationFeeAsync_EmptyStudentId()
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetGraduationApplicationFeeAsync(string.Empty, "MATH.BA");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetGraduationApplicationFeeAsync_NullProgramCode()
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetGraduationApplicationFeeAsync("studentId", null);
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetGraduationApplicationFeeAsync_EmptyProgramCode()
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetGraduationApplicationFeeAsync("studentId", string.Empty);
            }
        }

        [TestClass]
        public class GetStudentPrograms2Async
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }
            [TestMethod]
            public async Task GetStudentPrograms2Async_ReturnsSerializedStudentProgram2()
            {
                // Arrange
                List<StudentProgram2> lstOfStudentPrograms = new List<StudentProgram2>()
                {
                new StudentProgram2(){StudentId = "000011",ProgramCode = "MATH.BA"},
                new StudentProgram2(){StudentId = "000011",ProgramCode = "ENG.BA"}
            };


                var serializedStudentPrograms = JsonConvert.SerializeObject(lstOfStudentPrograms);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedStudentPrograms, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetStudentPrograms2Async("000011");

                // Assert that the expected number of items is returned and each of the expected items is found in the response
                Assert.IsNotNull(clientResponse);
                Assert.AreEqual(lstOfStudentPrograms.Count(), clientResponse.Count());
                foreach (var program in clientResponse)
                {
                    Assert.IsNotNull(program.StudentId);
                    Assert.IsNotNull(program.ProgramCode);
                    var programRetrieved = lstOfStudentPrograms.Where(c => c.StudentId == program.StudentId && c.ProgramCode == program.ProgramCode).FirstOrDefault();
                    Assert.AreEqual(program.StudentId, programRetrieved.StudentId);
                    Assert.AreEqual(program.ProgramCode, programRetrieved.ProgramCode);
                }
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetStudentPrograms2Async_NullStudentId()
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetGraduationApplicationFeeAsync(null, "MATH.BA");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetStudentPrograms2Async_EmptyStudentId()
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetGraduationApplicationFeeAsync(string.Empty, "MATH.BA");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetStudentPrograms2Async_NullProgramCode()
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetGraduationApplicationFeeAsync("studentId", null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetStudentPrograms2Async_EmptyProgramCode()
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetGraduationApplicationFeeAsync("studentId", string.Empty);
            }

        }

        [TestClass]
        public class GetStudentWaivers
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public async Task Client_GetStudentWaivers_ReturnsSerializedWaivers()
            {
                // Arrange
                var waivers = new List<Dtos.Student.StudentWaiver>()
                {
                    new StudentWaiver() {Id = "1", StudentId = "STU1", SectionId = "SEC+1", ReasonCode = "OTH", RequisiteWaivers = new List<RequisiteWaiver>() {new RequisiteWaiver() {RequisiteId = "R1", Status = WaiverStatus.Waived}}},
                    new StudentWaiver() {Id = "2", StudentId = "STU1", SectionId = "SEC+1", ReasonCode = "OTH", RequisiteWaivers = new List<RequisiteWaiver>()}
                };

                var serializedResponse = JsonConvert.SerializeObject(waivers.AsEnumerable());

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetStudentWaiversAsync("STU1");

                // Assert that the expected number of items is returned and each of the expected items is found in the response
                Assert.AreEqual(waivers.Count(), clientResponse.Count());
                foreach (var item in waivers)
                {
                    var waiver = clientResponse.Where(w => w.Id == item.Id).FirstOrDefault();
                    Assert.IsNotNull(waiver);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Client_GetStudentWaivers_ThrowsExceptionForNullSectionId()
            {
                // Arrange
                var mockHandler = new MockHandler();
                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetStudentWaiversAsync("");
            }
        }

        [TestClass]
        public class GetSessionCyclesAsync
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public async Task ClientGetSessionCyclesAsync_ReturnsSerializedSessionCycles()
            {
                // Arrange
                var sessionCycles = new List<SessionCycle>()
                    {
                        new SessionCycle(){Code="FO",Description="Fall Only"},
                        new SessionCycle(){Code="SO",Description="Spring Only"},
                        new SessionCycle(){Code="WO",Description="Winter Only"}
                    };
                var serializedResponse = JsonConvert.SerializeObject(sessionCycles);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetSessionCyclesAsync();

                // Assert that the expected number of items is returned and each of the expected items is found in the response
                Assert.IsNotNull(clientResponse);
                Assert.AreEqual(sessionCycles.Count(), clientResponse.Count());
                foreach (var sc in clientResponse)
                {
                    Assert.IsNotNull(sc.Code);
                    Assert.IsNotNull(sc.Description);
                    var sessionCycle = sessionCycles.Where(c => c.Code == sc.Code).FirstOrDefault();
                    Assert.AreEqual(sessionCycle.Code, sc.Code);
                    Assert.AreEqual(sessionCycle.Description, sc.Description);
                }
            }

        }

        [TestClass]
        public class GetYearlyCyclesAsync
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public async Task ClientGetYearlyCyclesAsync_ReturnsSerializedYearlyCycles()
            {
                // Arrange
                var yearlyCycles = new List<YearlyCycle>()
                    {
                        new YearlyCycle(){Code="EY",Description="Even Years Only"},
                        new YearlyCycle(){Code="OY",Description="Odd Years Only"},
                        new YearlyCycle(){Code="LY",Description="Election Years Only"}
                    };
                var serializedResponse = JsonConvert.SerializeObject(yearlyCycles);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetYearlyCyclesAsync();

                // Assert that the expected number of items is returned and each of the expected items is found in the response
                Assert.IsNotNull(clientResponse);
                Assert.AreEqual(yearlyCycles.Count(), clientResponse.Count());
                foreach (var sc in clientResponse)
                {
                    Assert.IsNotNull(sc.Code);
                    Assert.IsNotNull(sc.Description);
                    var yearlyCycle = yearlyCycles.Where(c => c.Code == sc.Code).FirstOrDefault();
                    Assert.AreEqual(yearlyCycle.Code, sc.Code);
                    Assert.AreEqual(yearlyCycle.Description, sc.Description);
                }
            }

        }

        [TestClass]
        public class GetStudentRequestConfigurationAsync
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public async Task ClientGetStudentRequestConfigurationAsync_ReturnsSerializedTranscriptRequestConfiguration()
            {
                // Arrange
                var studentRequestConfiguration = new StudentRequestConfiguration();
                studentRequestConfiguration.DefaultWebEmailType = "XXX";
                studentRequestConfiguration.SendTranscriptRequestConfirmation = true;
                studentRequestConfiguration.SendEnrollmentRequestConfirmation = true;
                var serializedResponse = JsonConvert.SerializeObject(studentRequestConfiguration);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetStudentRequestConfigurationAsync();

                // Assert that the expected number of items is returned and each of the expected items is found in the response
                Assert.IsNotNull(clientResponse);
                Assert.IsTrue(clientResponse.SendTranscriptRequestConfirmation);
                Assert.IsTrue(clientResponse.SendEnrollmentRequestConfirmation);
                Assert.AreEqual("XXX", clientResponse.DefaultWebEmailType);

            }

        }

        [TestClass]
        public class GetHoldRequestTypesAsync
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public async Task ClientGetHoldRequestTypesAsync_ReturnsSerializedHoldRequestTypes()
            {
                // Arrange
                var holdRequestTypes = new List<HoldRequestType>()
                    {
                        new HoldRequestType(){Code="GRADE",Description="Hold For Grades"},
                        new HoldRequestType(){Code="OTHER",Description="Other"},
                    };
                var serializedResponse = JsonConvert.SerializeObject(holdRequestTypes);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetHoldRequestTypesAsync();

                // Assert that the expected number of items is returned and each of the expected items is found in the response
                Assert.IsNotNull(clientResponse);
                Assert.AreEqual(holdRequestTypes.Count(), clientResponse.Count());
                foreach (var hrt in clientResponse)
                {
                    Assert.IsNotNull(hrt.Code);
                    Assert.IsNotNull(hrt.Description);
                    var holdRequestType = holdRequestTypes.Where(c => c.Code == hrt.Code).FirstOrDefault();
                    Assert.AreEqual(holdRequestType.Code, hrt.Code);
                    Assert.AreEqual(holdRequestType.Description, hrt.Description);
                }
            }

        }

        [TestClass]
        public class AddStudentTranscriptRequest
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public async Task Client_AddTranscriptRequest_ReturnsNewTranscriptRequest()
            {
                // Arrange
                var request = new Dtos.Student.StudentTranscriptRequest("STU1", "my name", new List<string> { "line 1" })
                {
                    HoldRequest = "GRADE",
                    NumberOfCopies = 3,
                    TranscriptGrouping = "UG"
                };

                var serializedResponse = JsonConvert.SerializeObject(request);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.AddStudentTranscriptRequestAsync(request);

                // Assert that the expected item is found in the response
                Assert.AreEqual(request.TranscriptGrouping, clientResponse.TranscriptGrouping);
                Assert.AreEqual(request.HoldRequest, clientResponse.HoldRequest);
                Assert.AreEqual(request.StudentId, clientResponse.StudentId);
                Assert.AreEqual(request.RecipientName, clientResponse.RecipientName);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Client_AddTranscriptRequest_ThrowsExceptionForNullRequest()
            {
                // Arrange
                var mockHandler = new MockHandler();
                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.AddStudentTranscriptRequestAsync(null);
            }
        }


        [TestClass]
        public class AddStudentEnrollmentRequest
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public async Task Client_AddEnrollmentRequest_ReturnsNewEnrollmentRequest()
            {
                // Arrange
                var request = new Dtos.Student.StudentEnrollmentRequest("STU1", "my name", new List<string>() { "address line 1" })
                {
                    HoldRequest = "GRADE",
                    NumberOfCopies = 3
                };

                var serializedResponse = JsonConvert.SerializeObject(request);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.AddStudentEnrollmentRequestAsync(request);

                // Assert that the expected item is found in the response
                Assert.AreEqual(request.HoldRequest, clientResponse.HoldRequest);
                Assert.AreEqual(request.StudentId, clientResponse.StudentId);
                Assert.AreEqual(request.RecipientName, clientResponse.RecipientName);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Client_AddEnrollmentRequest_ThrowsExceptionForNullRequest()
            {
                // Arrange
                var mockHandler = new MockHandler();
                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.AddStudentEnrollmentRequestAsync(null);
            }
        }

        [TestClass]
        public class GetStudentTranscriptRequest
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public async Task Client_GetStudentTranscriptRequest_ReturnsSerializedRequest()
            {
                // Arrange
                var request = new Dtos.Student.StudentTranscriptRequest("STU1", "my name", new List<string>() { "address line 1" })
                {
                    Id = "2",
                    HoldRequest = "GRADE",
                    NumberOfCopies = 3,
                    TranscriptGrouping = "UG"
                };

                var serializedResponse = JsonConvert.SerializeObject(request);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetStudentTranscriptRequestAsync("2");

                // Assert that the expected item is returned
                Assert.IsNotNull(clientResponse);
                Assert.AreEqual(request.Id, clientResponse.Id);
                Assert.AreEqual(request.StudentId, clientResponse.StudentId);
                Assert.AreEqual(request.RecipientName, clientResponse.RecipientName);
                Assert.AreEqual(request.TranscriptGrouping, clientResponse.TranscriptGrouping);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Client_GetStudentTranscriptRequest_ThrowsExceptionForNullRequestId()
            {
                // Arrange
                var mockHandler = new MockHandler();
                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetStudentTranscriptRequestAsync(null);
            }
        }

        [TestClass]
        public class GetStudentEnrollmentRequest
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public async Task Client_GetStudentEnrollmentRequest_ReturnsSerializedRequest()
            {
                // Arrange
                var request = new Dtos.Student.StudentEnrollmentRequest("STU1", "my name", new List<string>() { "address line 1" })
                {
                    Id = "2",
                    HoldRequest = "GRADE",
                    NumberOfCopies = 3
                };

                var serializedResponse = JsonConvert.SerializeObject(request);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetStudentEnrollmentRequestAsync("2");

                // Assert that the expected item is returned
                Assert.IsNotNull(clientResponse);
                Assert.AreEqual(request.Id, clientResponse.Id);
                Assert.AreEqual(request.StudentId, clientResponse.StudentId);
                Assert.AreEqual(request.RecipientName, clientResponse.RecipientName);
                Assert.AreEqual(request.HoldRequest, clientResponse.HoldRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Client_GetStudentEnrollmentRequest_ThrowsExceptionForNullRequestId()
            {
                // Arrange
                var mockHandler = new MockHandler();
                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetStudentEnrollmentRequestAsync(null);
            }
        }

        [TestClass]
        public class GetStudentEnrollmentRequests
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public async Task Client_GetStudentEnrollmentRequests_ReturnsSerializedRequest()
            {
                // Arrange
                List<Dtos.Student.StudentEnrollmentRequest> enrollRequests = new List<Dtos.Student.StudentEnrollmentRequest>();
                var request1 = new Dtos.Student.StudentEnrollmentRequest("STU1", "my name", new List<string>() { "address line 1" })
                {
                    Id = "2",
                    Comments = "Something",
                    NumberOfCopies = 1
                };
                enrollRequests.Add(request1);
                var request2 = new Dtos.Student.StudentEnrollmentRequest("STU1", "my name", new List<string>() { "address line 2" })
                {
                    Id = "4",
                    Comments = "Anything",
                    NumberOfCopies = 3
                };
                enrollRequests.Add(request2);

                var serializedResponse = JsonConvert.SerializeObject(enrollRequests);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetStudentEnrollmentRequestsAsync("STU1");

                // Assert that the expected items are returned
                Assert.IsNotNull(clientResponse);
                foreach (var request in enrollRequests)
                {
                    var responseItem = clientResponse.Where(cr => cr.Id == request.Id).FirstOrDefault();
                    Assert.IsNotNull(responseItem);
                    Assert.AreEqual(request.StudentId, responseItem.StudentId);
                    Assert.AreEqual(request.RecipientName, responseItem.RecipientName);
                    Assert.AreEqual(request.MailToAddressLines.ElementAt(0), responseItem.MailToAddressLines.ElementAt(0));
                    Assert.AreEqual(request.Comments, responseItem.Comments);
                }

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Client_GetStudentEnrollmentRequests_ThrowsExceptionForNullRequestId()
            {
                // Arrange
                var mockHandler = new MockHandler();
                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetStudentEnrollmentRequestsAsync(null);
            }
        }

        [TestClass]
        public class GetStudentTranscriptRequests
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public async Task Client_GetStudentTransriptRequests_ReturnsSerializedRequest()
            {
                // Arrange
                List<Dtos.Student.StudentTranscriptRequest> transcriptRequests = new List<Dtos.Student.StudentTranscriptRequest>();
                var request1 = new Dtos.Student.StudentTranscriptRequest("STU1", "my name", new List<string>() { "address line 1" })
                {
                    Id = "2",
                    Comments = "Something",
                    NumberOfCopies = 1,
                    TranscriptGrouping = "GR"
                };
                transcriptRequests.Add(request1);
                var request2 = new Dtos.Student.StudentTranscriptRequest("STU1", "my name", new List<string>() { "address line 2" })
                {
                    Id = "4",
                    Comments = "Anything",
                    NumberOfCopies = 3,
                    TranscriptGrouping = "UG"
                };
                transcriptRequests.Add(request2);

                var serializedResponse = JsonConvert.SerializeObject(transcriptRequests);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetStudentTranscriptRequestsAsync("STU1");

                // Assert that the expected items are returned
                Assert.IsNotNull(clientResponse);
                foreach (var request in transcriptRequests)
                {
                    var responseItem = clientResponse.Where(cr => cr.Id == request.Id).FirstOrDefault();
                    Assert.IsNotNull(responseItem);
                    Assert.AreEqual(request.StudentId, responseItem.StudentId);
                    Assert.AreEqual(request.RecipientName, responseItem.RecipientName);
                    Assert.AreEqual(request.MailToAddressLines.ElementAt(0), responseItem.MailToAddressLines.ElementAt(0));
                    Assert.AreEqual(request.Comments, responseItem.Comments);
                }

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Client_GetStudentTranscriptRequests_ThrowsExceptionForNullRequestId()
            {
                // Arrange
                var mockHandler = new MockHandler();
                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetStudentTranscriptRequestsAsync(null);
            }
        }

        [TestClass]
        public class GetStudentRequestFeeAsync
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public async Task GetStudentRequestFeeAsync_ReturnsSerializedStudentRequestFee()
            {
                // Arrange
                var studentRequestFee = new StudentRequestFee();
                studentRequestFee.StudentId = "000011";
                studentRequestFee.RequestId = "12345";
                studentRequestFee.PaymentDistributionCode = "DIST";
                studentRequestFee.Amount = 30m;

                var serializedStudentRequest = JsonConvert.SerializeObject(studentRequestFee);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedStudentRequest, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetStudentRequestFeeAsync("000011", "12345");

                // Assert that the expected number of items is returned and each of the expected items is found in the response
                Assert.IsNotNull(clientResponse);
                Assert.AreEqual(studentRequestFee.StudentId, clientResponse.StudentId);
                Assert.AreEqual(studentRequestFee.RequestId, clientResponse.RequestId);
                Assert.AreEqual(studentRequestFee.Amount, clientResponse.Amount);
                Assert.AreEqual(studentRequestFee.PaymentDistributionCode, clientResponse.PaymentDistributionCode);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetStudentRequestFeeAsync_NullStudentId()
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetStudentRequestFeeAsync(null, "12345");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetStudentRequestFeeAsync_EmptyStudentId()
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetStudentRequestFeeAsync(string.Empty, "12345");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetStudentRequestFeeAsync_NullProgramCode()
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetStudentRequestFeeAsync("studentId", null);
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetStudentRequestFeeAsync_EmptyProgramCode()
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetStudentRequestFeeAsync("studentId", string.Empty);
            }
        }


        [TestClass]
        public class QueryAcademicCreditsAsync
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public async Task Client_QueryAcademicCreditsAsync_ReturnsSerializedRequest()
            {
                // Arrange
                List<Dtos.Student.AcademicCredit2> academicCredits = new List<Dtos.Student.AcademicCredit2>();
                var credit1 = new Dtos.Student.AcademicCredit2()
                {
                    Id = "1",
                    CourseId = "100",
                    Credit = 3.0m,
                    Status = CreditStatus.Add.ToString(),
                    SectionId = "3000",
                    StudentId = "1111111"

                };
                academicCredits.Add(credit1);
                var credit2 = new Dtos.Student.AcademicCredit2()
                {
                    Id = "2",
                    CourseId = "100",
                    Credit = 3.0m,
                    Status = CreditStatus.Dropped.ToString(),
                    SectionId = "3000",
                    StudentId = "1111112"

                };
                academicCredits.Add(credit2);

                AcademicCreditQueryCriteria criteria = new AcademicCreditQueryCriteria() { SectionIds = new List<string>() { "3000" } };

                var serializedResponse = JsonConvert.SerializeObject(academicCredits);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.QueryAcademicCreditsAsync(criteria);

                // Assert that the expected items are returned
                Assert.IsNotNull(clientResponse);
                foreach (var credit in academicCredits)
                {
                    var responseItem = clientResponse.Where(cr => cr.Id == credit.Id).FirstOrDefault();
                    Assert.IsNotNull(responseItem);
                    Assert.AreEqual(credit.StudentId, responseItem.StudentId);
                    Assert.AreEqual(credit.CourseId, responseItem.CourseId);
                    Assert.AreEqual(credit.Credit, responseItem.Credit);
                    Assert.AreEqual(credit.SectionId, responseItem.SectionId);
                    Assert.AreEqual(credit.Status, responseItem.Status);
                }

            }

        }

        [TestClass]
        public class QueryAcademicCredits2Async
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public async Task Client_QueryAcademicCredits2Async_ReturnsSerializedRequest()
            {
                // Arrange
                List<Dtos.Student.AcademicCredit3> academicCredits = new List<Dtos.Student.AcademicCredit3>();
                var credit1 = new Dtos.Student.AcademicCredit3()
                {
                    Id = "1",
                    CourseId = "100",
                    Credit = 3.0m,
                    Status = CreditStatus.Add.ToString(),
                    SectionId = "3000",
                    StudentId = "1111111",
                    AdjustedCredit = 3m

                };
                academicCredits.Add(credit1);
                var credit2 = new Dtos.Student.AcademicCredit3()
                {
                    Id = "2",
                    CourseId = "100",
                    Credit = 3.0m,
                    Status = CreditStatus.Dropped.ToString(),
                    SectionId = "3000",
                    StudentId = "1111112",
                    AdjustedCredit = null

                };
                academicCredits.Add(credit2);

                AcademicCreditQueryCriteria criteria = new AcademicCreditQueryCriteria() { SectionIds = new List<string>() { "3000" } };

                var serializedResponse = JsonConvert.SerializeObject(academicCredits);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.QueryAcademicCredits2Async(criteria);

                // Assert that the expected items are returned
                Assert.IsNotNull(clientResponse);
                foreach (var credit in academicCredits)
                {
                    var responseItem = clientResponse.Where(cr => cr.Id == credit.Id).FirstOrDefault();
                    Assert.IsNotNull(responseItem);
                    Assert.AreEqual(credit.StudentId, responseItem.StudentId);
                    Assert.AreEqual(credit.CourseId, responseItem.CourseId);
                    Assert.AreEqual(credit.Credit, responseItem.Credit);
                    Assert.AreEqual(credit.SectionId, responseItem.SectionId);
                    Assert.AreEqual(credit.Status, responseItem.Status);
                    Assert.AreEqual(credit.AdjustedCredit, responseItem.AdjustedCredit);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Client_QueryAcademicCredits2Async_Null_Criteria_throws_Exception()
            {
                // Arrange
                List<Dtos.Student.AcademicCredit3> academicCredits = new List<Dtos.Student.AcademicCredit3>();
                var credit1 = new Dtos.Student.AcademicCredit3()
                {
                    Id = "1",
                    CourseId = "100",
                    Credit = 3.0m,
                    Status = CreditStatus.Add.ToString(),
                    SectionId = "3000",
                    StudentId = "1111111",
                    AdjustedCredit = 3m

                };
                academicCredits.Add(credit1);
                var credit2 = new Dtos.Student.AcademicCredit3()
                {
                    Id = "2",
                    CourseId = "100",
                    Credit = 3.0m,
                    Status = CreditStatus.Dropped.ToString(),
                    SectionId = "3000",
                    StudentId = "1111112",
                    AdjustedCredit = null

                };
                academicCredits.Add(credit2);

                AcademicCreditQueryCriteria criteria = new AcademicCreditQueryCriteria() { SectionIds = new List<string>() { "3000" } };

                var serializedResponse = JsonConvert.SerializeObject(academicCredits);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.QueryAcademicCredits2Async(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Client_QueryAcademicCredits2Async_Null_Criteria_SectionIds_throws_Exception()
            {
                // Arrange
                List<Dtos.Student.AcademicCredit3> academicCredits = new List<Dtos.Student.AcademicCredit3>();
                var credit1 = new Dtos.Student.AcademicCredit3()
                {
                    Id = "1",
                    CourseId = "100",
                    Credit = 3.0m,
                    Status = CreditStatus.Add.ToString(),
                    SectionId = "3000",
                    StudentId = "1111111",
                    AdjustedCredit = 3m

                };
                academicCredits.Add(credit1);
                var credit2 = new Dtos.Student.AcademicCredit3()
                {
                    Id = "2",
                    CourseId = "100",
                    Credit = 3.0m,
                    Status = CreditStatus.Dropped.ToString(),
                    SectionId = "3000",
                    StudentId = "1111112",
                    AdjustedCredit = null

                };
                academicCredits.Add(credit2);

                AcademicCreditQueryCriteria criteria = new AcademicCreditQueryCriteria() { SectionIds = null };

                var serializedResponse = JsonConvert.SerializeObject(academicCredits);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.QueryAcademicCredits2Async(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Client_QueryAcademicCredits2Async_Empty_Criteria_SectionIds_throws_Exception()
            {
                // Arrange
                List<Dtos.Student.AcademicCredit3> academicCredits = new List<Dtos.Student.AcademicCredit3>();
                var credit1 = new Dtos.Student.AcademicCredit3()
                {
                    Id = "1",
                    CourseId = "100",
                    Credit = 3.0m,
                    Status = CreditStatus.Add.ToString(),
                    SectionId = "3000",
                    StudentId = "1111111",
                    AdjustedCredit = 3m

                };
                academicCredits.Add(credit1);
                var credit2 = new Dtos.Student.AcademicCredit3()
                {
                    Id = "2",
                    CourseId = "100",
                    Credit = 3.0m,
                    Status = CreditStatus.Dropped.ToString(),
                    SectionId = "3000",
                    StudentId = "1111112",
                    AdjustedCredit = null

                };
                academicCredits.Add(credit2);

                AcademicCreditQueryCriteria criteria = new AcademicCreditQueryCriteria() { SectionIds = new List<string>() };

                var serializedResponse = JsonConvert.SerializeObject(academicCredits);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.QueryAcademicCredits2Async(null);
            }
        }

        [TestClass]
        public class GetFacultyGradingConfigurationAsync
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public async Task ClientGetFacultyGradingConfigurationAsync_ReturnsSerializedFacultyGradingConfiguration()
            {
                // Arrange
                var facultyGradingConfiguration = new FacultyGradingConfiguration();
                facultyGradingConfiguration.IncludeCrosslistedStudents = true;
                facultyGradingConfiguration.IncludeDroppedWithdrawnStudents = true;
                facultyGradingConfiguration.AllowedGradingTerms = new List<string>()
                {
                    "2016/SP",
                     "2016/FA"
                };
                var serializedResponse = JsonConvert.SerializeObject(facultyGradingConfiguration);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetFacultyGradingConfigurationAsync();

                // Assert that the expected number of items is returned and each of the expected items is found in the response
                Assert.IsNotNull(clientResponse);
                Assert.IsTrue(clientResponse.IncludeCrosslistedStudents);
                Assert.IsTrue(clientResponse.IncludeDroppedWithdrawnStudents);
                Assert.AreEqual(facultyGradingConfiguration.AllowedGradingTerms.Count(), clientResponse.AllowedGradingTerms.Count());
            }

        }

        [TestClass]
        public class GetTranscriptRestrictions2Async
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public async Task ClientGetTranscriptRestrictions2Async_ReturnsSerializedTranscriptAccess()
            {
                // Arrange
                var studentId = "studentId";
                var transcriptAccess = new TranscriptAccess();
                transcriptAccess.EnforceTranscriptRestriction = true;
                var ta1 = new TranscriptRestriction() { Code = "BH", Description = "Business Office Hold" };
                var ta2 = new TranscriptRestriction() { Code = "PF", Description = "Parking Fine" };
                transcriptAccess.TranscriptRestrictions = new List<TranscriptRestriction>() { ta1, ta2 };

                var serializedResponse = JsonConvert.SerializeObject(transcriptAccess);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetTranscriptRestrictions2Async(studentId);

                // Assert that the expected number of items is returned and each of the expected items is found in the response
                Assert.IsNotNull(clientResponse);
                Assert.IsTrue(clientResponse.EnforceTranscriptRestriction);
                Assert.AreEqual(transcriptAccess.TranscriptRestrictions.Count(), clientResponse.TranscriptRestrictions.Count());
            }

        }

        [TestClass]
        public class GetTestResult2Async
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public async Task GetTestResult2Async_ReturnsSerializedTestResults()
            {
                // Arrange
                var studentId = "studentId";
                var tr1 = new TestResult2() { Code = "SAT", Description = "SAT Test", Score = 10.25m, StudentId = "studentId", Category = TestType.Admissions, DateTaken = DateTime.Now.AddDays(-30) };
                var tr2 = new TestResult2() { Code = "MATH", Description = "Math Placement Test", Score = 1.333m, StudentId = "studentId", Category = TestType.Placement, DateTaken = DateTime.Now.AddDays(-30) };
                var testResults = new List<TestResult2>() { tr1, tr2 };

                var serializedResponse = JsonConvert.SerializeObject(testResults);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetTestResults2Async(studentId, null);

                // Assert that the expected number of items is returned and each of the expected items is found in the response
                Assert.IsNotNull(clientResponse);
                Assert.AreEqual(testResults.Count(), clientResponse.Count());
                Assert.IsInstanceOfType(clientResponse.ElementAt(0), typeof(TestResult2));
            }

        }

        [TestClass]
        public class GetCourseCatalogConfigurationAsync
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public async Task ClientGetCourseCatalogConfigurationAsync_ReturnsSerializedCourseCatalogConfiguration()
            {
                // Arrange
                var searchStartDate = DateTime.Now.AddDays(-180);
                var searchEndDate = DateTime.Now.AddDays(180);


                var courseCatalogConfiguration = new CourseCatalogConfiguration();
                courseCatalogConfiguration.EarliestSearchDate = searchStartDate;
                courseCatalogConfiguration.LatestSearchDate = searchEndDate;
                var serializedResponse = JsonConvert.SerializeObject(courseCatalogConfiguration);
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetCourseCatalogConfigurationAsync();

                // Assert that the expected number of items is returned and each of the expected items is found in the response
                Assert.IsNotNull(clientResponse);
                Assert.AreEqual(searchStartDate, clientResponse.EarliestSearchDate);
                Assert.AreEqual(searchEndDate, clientResponse.LatestSearchDate);

            }

        }

        [TestClass]
        public class GetRegistrationConfigurationAsync
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public async Task ClientGetRegistrationConfigurationAsync_ReturnsSerializedRegistrationConfiguration()
            {
                // Arrange
                var searchStartDate = DateTime.Now.AddDays(-180);
                var searchEndDate = DateTime.Now.AddDays(180);


                var registrationConfiguration = new RegistrationConfiguration();
                registrationConfiguration.RequireFacultyAddAuthorization = true;
                registrationConfiguration.AddAuthorizationStartOffsetDays = 3;
                var serializedResponse = JsonConvert.SerializeObject(registrationConfiguration);
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetRegistrationConfigurationAsync();

                // Assert that the expected number of items is returned and each of the expected items is found in the response
                Assert.IsNotNull(clientResponse);
                Assert.IsTrue(clientResponse.RequireFacultyAddAuthorization);
                Assert.AreEqual(3, clientResponse.AddAuthorizationStartOffsetDays);

            }

        }

        [TestClass]
        public class QueryStudentsById4AsyncTests
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public async Task Client_QueryStudentsById4Async_ReturnsSerializedObject()
            {
                // Arrange
                var students = new List<Dtos.Student.StudentBatch3>();
                var student = new Dtos.Student.StudentBatch3()
                {
                    Id = "10000",
                    FirstName = "FirstName",
                    LastName = "LastName",
                    MiddleName = "M",
                    PreferredEmailAddress = "firstname@yahoo.com",
                    PersonDisplayName = new PersonHierarchyName() { FullName = "DisplayName", LastName = "Last", HierarchyCode = "PREFERRED" },
                    StudentRestrictionIds = new List<string>() { "111", "222" },
                    IsConfidential = false,
                    IsLegacyStudent = true,
                    IsTransfer = false,
                    BirthDate = DateTime.Now.AddYears(-20),


                };
                students.Add(student);
                var studentIds = students.Select(d => d.Id);
                var serializedResponse = JsonConvert.SerializeObject(students);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.QueryStudentsById4Async(studentIds);

                // Assert
                Assert.AreEqual(students.Count(), clientResponse.Count());
                foreach (var stuBatch in students)
                {
                    var batchResponse = clientResponse.Where(s => s.Id == student.Id).First();
                    Assert.AreEqual(student.FirstName, batchResponse.FirstName);
                    Assert.AreEqual(student.LastName, batchResponse.LastName);
                    Assert.AreEqual(student.MiddleName, batchResponse.MiddleName);
                    Assert.AreEqual(student.PreferredEmailAddress, batchResponse.PreferredEmailAddress);
                    Assert.IsNotNull(batchResponse.PersonDisplayName);
                    Assert.AreEqual(student.IsLegacyStudent, batchResponse.IsLegacyStudent);
                    Assert.AreEqual(student.StudentRestrictionIds.Count(), batchResponse.StudentRestrictionIds.Count());

                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Client_QueryStudentsById4Async__ThrowsExceptionForNullStudentIds()
            {
                // Arrange
                var mockHandler = new MockHandler();
                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.QueryStudentsById4Async(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Client_QueryStudentsById4Async_ThrowsExceptionForEmptyStudentIds()
            {
                // Arrange
                var mockHandler = new MockHandler();
                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.QueryStudentsById4Async(new List<string>());
            }
        }

        [TestClass]
        public class GetStudentRestrictions2Async
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public async Task Client_GetStudentRestrictions2Async_ReturnsSerializedObject()
            {
                // Arrange
                var restrictions = new List<PersonRestriction>
                {
                    new PersonRestriction()
                    {
                        Id = "PR1",
                        Title = "Person Restriction 1"
                    }
                };
                var serializedResponse = JsonConvert.SerializeObject(restrictions);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetStudentRestrictions2Async("0000001");

                // Assert
                Assert.AreEqual(restrictions.Count(), clientResponse.Count());
                foreach (var restriction in restrictions)
                {
                    var batchResponse = clientResponse.Where(r => r.Id == restriction.Id).First();
                    Assert.AreEqual(restriction.Id, batchResponse.Id);
                    Assert.AreEqual(restriction.Title, batchResponse.Title);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Client_GetStudentRestrictions2Async__ThrowsExceptionForNullStudentIds()
            {
                // Arrange
                var mockHandler = new MockHandler();
                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetStudentRestrictions2Async(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Client_GetStudentRestrictions2Async_ThrowsExceptionForEmptyStudentId()
            {
                // Arrange
                var mockHandler = new MockHandler();
                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetStudentRestrictions2Async(string.Empty);
            }

        }

        [TestClass]
        public class UpdateSectionBookAsync
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task UpdateSectionBooksAsync_TextbookNull()
            {
                //Arrange
                var mockHandler = new MockHandler();

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.UpdateSectionBookAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task UpdateSectionBooksAsync_SectionIdEmpty()
            {
                // Arrange
                var sectionTextbook = new SectionTextbook()
                {
                    SectionId = string.Empty,
                    Action = SectionBookAction.Update,
                    RequirementStatusCode = "R",
                    Textbook = new Book()
                    {
                        Id = "123",
                        Isbn = "123456789",
                        Title = "Title",
                        Author = "Author",
                        Publisher = "Publisher",
                        Copyright = "Copyright",
                        Edition = "Edition",
                        IsActive = true,
                        PriceUsed = 10m,
                        Price = 20m,
                        Comment = "Comment",
                        ExternalComments = "External Comments",
                        AlternateID1 = "altId1",
                        AlternateID2 = "altId2",
                        AlternateID3 = "altId3"
                    }
                };

                var mockHandler = new MockHandler();

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.UpdateSectionBookAsync(sectionTextbook);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task UpdateSectionBooksAsync_SectionIdNull()
            {
                // Arrange
                var sectionTextbook = new SectionTextbook()
                {
                    SectionId = null,
                    Action = SectionBookAction.Update,
                    RequirementStatusCode = "R",
                    Textbook = new Book()
                    {
                        Id = "123",
                        Isbn = "123456789",
                        Title = "Title",
                        Author = "Author",
                        Publisher = "Publisher",
                        Copyright = "Copyright",
                        Edition = "Edition",
                        IsActive = true,
                        PriceUsed = 10m,
                        Price = 20m,
                        Comment = "Comment",
                        ExternalComments = "External Comments",
                        AlternateID1 = "altId1",
                        AlternateID2 = "altId2",
                        AlternateID3 = "altId3"
                    }
                };

                var mockHandler = new MockHandler();

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.UpdateSectionBookAsync(sectionTextbook);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task UpdateSectionBooksAsync_BadRequest()
            {
                // Arrange
                var sectionTextbook = new SectionTextbook()
                {
                    SectionId = "12345",
                    Action = SectionBookAction.Add,
                    RequirementStatusCode = "R",
                    Textbook = new Book()
                    {
                        Id = "123",
                        Isbn = "123456789",
                        Title = "Title",
                        Author = "Author",
                        Publisher = "Publisher",
                        Copyright = "Copyright",
                        Edition = "Edition",
                        IsActive = true,
                        PriceUsed = 10m,
                        Price = 20m,
                        Comment = "Comment",
                        ExternalComments = "External Comments",
                        AlternateID1 = "altId1",
                        AlternateID2 = "altId2",
                        AlternateID3 = "altId3"
                    }
                };

                // Arrange
                var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent(string.Empty, Encoding.UTF8, _contentType);
                response.RequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://test");
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var result = await client.UpdateSectionBookAsync(sectionTextbook);
                _loggerMock.Verify();
            }

            [TestMethod]
            public async Task UpdateSectionBooksAsync_UpdatedSection()
            {
                // Arrange
                var sectionTextbook = new SectionTextbook()
                {
                    SectionId = "12345",
                    Action = SectionBookAction.Add,
                    RequirementStatusCode = "R",
                    Textbook = new Book()
                    {
                        Id = "123",
                        Isbn = "123456789",
                        Title = "Title",
                        Author = "Author",
                        Publisher = "Publisher",
                        Copyright = "Copyright",
                        Edition = "Edition",
                        IsActive = true,
                        PriceUsed = 10m,
                        Price = 20m,
                        Comment = "Comment",
                        ExternalComments = "External Comments",
                        AlternateID1 = "altId1",
                        AlternateID2 = "altId2",
                        AlternateID3 = "altId3"
                    }
                };

                var serializedSectionTextbook = JsonConvert.SerializeObject(sectionTextbook);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedSectionTextbook, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.UpdateSectionBookAsync(sectionTextbook);

                // Assert that the expected number of items is returned and each of the expected items is found in the response
                Assert.IsNotNull(clientResponse);
                Assert.IsInstanceOfType(clientResponse, typeof(Section3));
            }


        }

        [TestClass]
        public class QueryBooks
        {

            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _studentId = "123456";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public async Task QueryBooksByIdsAsync_Ids_ReturnsSerializedBooks()
            {
                // Arrange
                var bookDtos = new List<Book>()
                {
                    new Book() {
                        Id = "111",
                        Title = "Title of the Book One",
                        Author = "John Smith",
                        Publisher = "Publisher One",
                        Isbn = "12121212121212",
                        Copyright = "3434",
                        Edition = "First",
                        IsActive = true,
                        ExternalComments = "These are external comments",
                        Price = 400m,
                        PriceUsed = 5m,
                        Comment = "Regular Comment"},
                    new Book() {
                        Id = "222",
                        Title = "Title of Book Two",
                        Isbn = "47474747",
                        Author = "Sam Jones",
                        Publisher = "Publisher Two",
                        IsActive = false,
                        ExternalComments = "Additional external comments"

                } };
                IEnumerable<string> bookIds = bookDtos.Select(f => f.Id);

                var serializedResponse = JsonConvert.SerializeObject(bookDtos.AsEnumerable());

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.QueryBooksByPostAsync(bookIds.ToList(), null);

                // Assert
                Assert.AreEqual(bookDtos.Count(), clientResponse.Count());
                foreach (var id in bookIds)
                {
                    var bookDto = bookDtos.Where(f => f.Id == id).First();
                    var bookResponse = clientResponse.Where(f => f.Id == id).First();

                    Assert.AreEqual(bookDto.Id, bookResponse.Id);
                    Assert.AreEqual(bookDto.Title, bookResponse.Title);
                    Assert.AreEqual(bookDto.Author, bookResponse.Author);
                    Assert.AreEqual(bookDto.Publisher, bookResponse.Publisher);
                    Assert.AreEqual(bookDto.Copyright, bookResponse.Copyright);
                    Assert.AreEqual(bookDto.Isbn, bookResponse.Isbn);
                    Assert.AreEqual(bookDto.ExternalComments, bookResponse.ExternalComments);
                    Assert.AreEqual(bookDto.Comment, bookResponse.Comment);
                    Assert.AreEqual(bookDto.Price, bookResponse.Price);
                    Assert.AreEqual(bookDto.PriceUsed, bookResponse.PriceUsed);
                    Assert.AreEqual(bookDto.IsActive, bookResponse.IsActive);
                    Assert.AreEqual(bookDto.Edition, bookResponse.Edition);
                }
            }

            [TestMethod]
            public async Task QueryBooksByIdsAsync_QueryString_ReturnsSerializedBooks()
            {
                // Arrange
                var bookDtos = new List<Book>()
                {
                    new Book() {
                        Id = "111",
                        Title = "Title of the Book One",
                        Author = "John Smith",
                        Publisher = "Publisher One",
                        Isbn = "12121212121212",
                        Copyright = "3434",
                        Edition = "First",
                        IsActive = true,
                        ExternalComments = "These are external comments",
                        Price = 400m,
                        PriceUsed = 5m,
                        Comment = "Regular Comment"},
                    new Book() {
                        Id = "222",
                        Title = "Title of Book Two",
                        Isbn = "47474747",
                        Author = "Sam Jones",
                        Publisher = "Publisher Two",
                        IsActive = false,
                        ExternalComments = "Additional external comments"

                } };
                string queryString = "Query String";

                var serializedResponse = JsonConvert.SerializeObject(bookDtos.AsEnumerable());

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.QueryBooksByPostAsync(null, queryString);

                // Assert
                Assert.AreEqual(bookDtos.Count(), clientResponse.Count());
                var bookIds = bookDtos.Select(dto => dto.Id).ToList();
                foreach (var id in bookIds)
                {
                    var bookDto = bookDtos.Where(f => f.Id == id).First();
                    var bookResponse = clientResponse.Where(f => f.Id == id).First();

                    Assert.AreEqual(bookDto.Id, bookResponse.Id);
                    Assert.AreEqual(bookDto.Title, bookResponse.Title);
                    Assert.AreEqual(bookDto.Author, bookResponse.Author);
                    Assert.AreEqual(bookDto.Publisher, bookResponse.Publisher);
                    Assert.AreEqual(bookDto.Copyright, bookResponse.Copyright);
                    Assert.AreEqual(bookDto.Isbn, bookResponse.Isbn);
                    Assert.AreEqual(bookDto.ExternalComments, bookResponse.ExternalComments);
                    Assert.AreEqual(bookDto.Comment, bookResponse.Comment);
                    Assert.AreEqual(bookDto.Price, bookResponse.Price);
                    Assert.AreEqual(bookDto.PriceUsed, bookResponse.PriceUsed);
                    Assert.AreEqual(bookDto.IsActive, bookResponse.IsActive);
                    Assert.AreEqual(bookDto.Edition, bookResponse.Edition);
                }
            }

        }

        [TestClass]
        public class GetBookOptionsAsync
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task GetBookOptionsAsync_BadRequest()
            {
                //Arrange
                var bookOptions = new List<BookOption>()
                {
                    new BookOption() {Code="R", Description="Required", IsRequired=true},
                    new BookOption() {Code="C", Description="Recommended", IsRequired=false},
                    new BookOption() {Code="O", Description="Optional", IsRequired=false}
                };

                var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent(string.Empty, Encoding.UTF8, _contentType);
                response.RequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://test");
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var result = await client.GetBookOptionsAsync();
                _loggerMock.Verify();
            }

            [TestMethod]
            public async Task GetBookOptionsAsync_ReturnSerializedBookOptions()
            {
                //Arrange
                var bookOptions = new List<BookOption>()
                {
                    new BookOption() {Code="R", Description="Required", IsRequired=true},
                    new BookOption() {Code="C", Description="Recommended", IsRequired=false},
                    new BookOption() {Code="O", Description="Optional", IsRequired=false}
                };

                var serializedResponse = JsonConvert.SerializeObject(bookOptions);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetBookOptionsAsync();

                // Assert that the expected number of items is returned and each of the expected items is found in the response
                Assert.IsNotNull(clientResponse);
                Assert.IsInstanceOfType(clientResponse, typeof(IEnumerable<BookOption>));
                Assert.AreEqual(bookOptions.Count(), clientResponse.Count());
                foreach (var option in clientResponse)
                {
                    Assert.IsNotNull(option.Code);
                    Assert.IsNotNull(option.Description);
                    Assert.IsNotNull(option.IsRequired);
                    var bookOption = bookOptions.Where(c => c.Code == option.Code).FirstOrDefault();
                    Assert.AreEqual(bookOption.Code, option.Code);
                    Assert.AreEqual(bookOption.Description, option.Description);
                    Assert.AreEqual(bookOption.IsRequired, option.IsRequired);
                }
            }
        }

        [TestClass]
        public class GetSectionMeetingInstancesAsync
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetSectionMeetingInstancesAsync_MissingSectionId()
            {


                var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent(string.Empty, Encoding.UTF8, _contentType);
                response.RequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://test");
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var result = await client.GetSectionMeetingInstancesAsync(null);
                _loggerMock.Verify();
            }

            [TestMethod]
            public async Task GetSectionMeetingInstancesAsync_ReturnSerializedMeetingInstances()
            {
                //Arrange
                var timeoffset = new TimeSpan(4, 0, 0);
                var meetingDtos = new List<SectionMeetingInstance>()
                {
                    new SectionMeetingInstance() {SectionId = "1111111", InstructionalMethod = "LEC", MeetingDate = new DateTime(2018, 1, 1), StartTime = new DateTimeOffset(2017, 7, 1, 9, 15, 0, timeoffset), EndTime = new DateTimeOffset(2017, 12, 15, 10, 15, 0, timeoffset) },
                    new SectionMeetingInstance() {SectionId = "1111111", InstructionalMethod = "LEC", MeetingDate = new DateTime(2018, 1, 3), StartTime = new DateTimeOffset(2017, 7, 1, 9, 15, 0, timeoffset), EndTime = new DateTimeOffset(2017, 12, 15, 10, 15, 0, timeoffset) },
                    new SectionMeetingInstance() {SectionId = "1111111", InstructionalMethod = string.Empty, MeetingDate = new DateTime(2018, 1, 5), StartTime = null, EndTime = null }

                };

                var serializedResponse = JsonConvert.SerializeObject(meetingDtos);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetSectionMeetingInstancesAsync("1111111");

                // Assert that the expected number of items is returned and each of the expected items is found in the response
                Assert.IsNotNull(clientResponse);
                Assert.IsInstanceOfType(clientResponse, typeof(IEnumerable<SectionMeetingInstance>));
                Assert.AreEqual(meetingDtos.Count(), clientResponse.Count());
                foreach (var meeting in clientResponse)
                {
                    Assert.IsNotNull(meeting.SectionId);
                    Assert.IsNotNull(meeting.MeetingDate);
                    var expectedMeeting = meetingDtos.Where(c => c.MeetingDate == meeting.MeetingDate).FirstOrDefault();
                    Assert.AreEqual(expectedMeeting.InstructionalMethod, meeting.InstructionalMethod);
                    Assert.AreEqual(expectedMeeting.StartTime, meeting.StartTime);
                    Assert.AreEqual(expectedMeeting.EndTime, meeting.EndTime);
                }
            }
        }

        [TestClass]
        public class QueryStudentAttendancesAsync
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task QueryStudentAttendancesAsync_MissingSectionId()
            {


                var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent(string.Empty, Encoding.UTF8, _contentType);
                response.RequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://test");
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var result = await client.QueryStudentAttendancesAsync(null, false, null);
                _loggerMock.Verify();
            }

            [TestMethod]
            public async Task QueryStudentAttendancesAsync_ReturnSerializedStudentAttendancesInstances()
            {
                //Arrange
                var timeoffset = new TimeSpan(4, 0, 0);
                var meetingInstance = new SectionMeetingInstance() { SectionId = "1111111", InstructionalMethod = string.Empty, MeetingDate = new DateTime(2018, 1, 5), StartTime = null, EndTime = null };
                var studentAttendanceDtos = new List<StudentAttendance>()
                {
                    new StudentAttendance() {StudentId = "1111111", SectionId = "SEC1", StudentCourseSectionId = "1234", MeetingDate = new DateTime(2018, 1, 5), AttendanceCategoryCode = "P", Comment = "Comment for 1111111"},
                    new StudentAttendance() {StudentId = "2222222", SectionId = "SEC1", StudentCourseSectionId = "1235", MeetingDate = new DateTime(2018, 1, 5), AttendanceCategoryCode = "L", Comment = "Comment for 2222222"},
                    new StudentAttendance() {StudentId = "3333333", SectionId = "SEC1",  StudentCourseSectionId = "1236", MeetingDate = new DateTime(2018, 1, 5), AttendanceCategoryCode = "E"}

                };

                var serializedResponse = JsonConvert.SerializeObject(studentAttendanceDtos);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.QueryStudentAttendancesAsync("111", false, null);

                // Assert that the expected number of items is returned and each of the expected items is found in the response
                Assert.IsNotNull(clientResponse);
                Assert.IsInstanceOfType(clientResponse, typeof(IEnumerable<StudentAttendance>));
                Assert.AreEqual(studentAttendanceDtos.Count(), clientResponse.Count());
                foreach (var attendance in clientResponse)
                {
                    var expectedAttendance = studentAttendanceDtos.Where(sa => sa.StudentId == attendance.StudentId).FirstOrDefault();
                    Assert.IsNotNull(expectedAttendance);
                    Assert.AreEqual(expectedAttendance.StudentId, attendance.StudentId);
                    Assert.AreEqual(expectedAttendance.SectionId, attendance.SectionId);
                    Assert.AreEqual(expectedAttendance.StudentCourseSectionId, attendance.StudentCourseSectionId);
                    Assert.AreEqual(expectedAttendance.AttendanceCategoryCode, attendance.AttendanceCategoryCode);
                    Assert.AreEqual(expectedAttendance.Comment, attendance.Comment);
                    Assert.AreEqual(expectedAttendance.MeetingDate, attendance.MeetingDate);
                }
            }
        }

        [TestClass]
        public class QueryStudentSectionAttendancesAsync
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task QueryStudentSectionAttendancesAsync_MissingStudentId()
            {


                var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent(string.Empty, Encoding.UTF8, _contentType);
                response.RequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://test");
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var result = await client.QueryStudentSectionAttendancesAsync(null, new List<string>() { "S1", "S2" });
                _loggerMock.Verify();
            }

            [TestMethod]
            public async Task QueryStudentSectionAttendancesAsync_MissingSectionIds()
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(string.Empty, Encoding.UTF8, _contentType);
                response.RequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://test");
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var result = await client.QueryStudentSectionAttendancesAsync("ST001", null);
                Assert.IsNull(result);
            }

            [TestMethod]
            public async Task QueryStudentSectionAttendancesAsync_ReturnSerializedStudentAttendancesInstances()
            {
                var studentAttendanceDtos = new List<StudentAttendance>()
                {
                    new StudentAttendance() {StudentId = "1111111", SectionId = "SEC1", StudentCourseSectionId = "1234", MeetingDate = new DateTime(2018, 1, 5), AttendanceCategoryCode = "P", Comment = "Comment for 1111111"},
                    new StudentAttendance() {StudentId = "2222222", SectionId = "SEC1", StudentCourseSectionId = "1235", MeetingDate = new DateTime(2018, 1, 5), AttendanceCategoryCode = "L", Comment = "Comment for 2222222"},
                    new StudentAttendance() {StudentId = "3333333", SectionId = "SEC1",  StudentCourseSectionId = "1236", MeetingDate = new DateTime(2018, 1, 5), AttendanceCategoryCode = "E"},
                    new StudentAttendance() {StudentId = "1111111", SectionId = "SEC2", StudentCourseSectionId = "1234", MeetingDate = new DateTime(2018, 1, 5), AttendanceCategoryCode = "P", Comment = "Comment for 1111111"},
                    new StudentAttendance() {StudentId = "2222222", SectionId = "SEC2", StudentCourseSectionId = "1235", MeetingDate = new DateTime(2018, 1, 5), AttendanceCategoryCode = "L", Comment = "Comment for 2222222"},

                };
                var studentSectionsAttendances = new StudentSectionsAttendances();
                studentSectionsAttendances.SectionWiseAttendances.Add("SEC1", new List<StudentAttendance>() { studentAttendanceDtos[0], studentAttendanceDtos[1], studentAttendanceDtos[2] });
                studentSectionsAttendances.SectionWiseAttendances.Add("SEC2", new List<StudentAttendance>() { studentAttendanceDtos[3], studentAttendanceDtos[4] });
                var serializedResponse = JsonConvert.SerializeObject(studentSectionsAttendances);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.QueryStudentSectionAttendancesAsync("student1", new List<string>() { "111" });

                // Assert that the expected number of items is returned and each of the expected items is found in the response
                Assert.IsNotNull(clientResponse);
                Assert.IsInstanceOfType(clientResponse, typeof(StudentSectionsAttendances));
                Assert.AreEqual(2, clientResponse.SectionWiseAttendances.Count());
                Assert.AreEqual(3, clientResponse.SectionWiseAttendances["SEC1"].Count());
                Assert.AreEqual(2, clientResponse.SectionWiseAttendances["SEC2"].Count());

            }
        }


        [TestClass]
        public class GetAttendanceCategoriesAsync
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }



            [TestMethod]
            public async Task GetAttendanceCategoriesAsync_ReturnSerializedAttendancecategories()
            {
                //Arrange
                var timeoffset = new TimeSpan(4, 0, 0);
                var attendanceCategoriesDto = new List<Ellucian.Colleague.Dtos.AttendanceCategories>()
                {
                    new Ellucian.Colleague.Dtos.AttendanceCategories() {Code="P", Title="Present" },
                    new Ellucian.Colleague.Dtos.AttendanceCategories() {Code="A",Title="Absent" },
                    new Ellucian.Colleague.Dtos.AttendanceCategories() {Code="L", Title="Late" }

                };

                var serializedResponse = JsonConvert.SerializeObject(attendanceCategoriesDto);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetAttendanceCategoriesAsync();

                // Assert that the expected number of items is returned and each of the expected items is found in the response
                Assert.IsNotNull(clientResponse);
                Assert.IsInstanceOfType(clientResponse, typeof(IEnumerable<Ellucian.Colleague.Dtos.AttendanceCategories>));
                Assert.AreEqual(attendanceCategoriesDto.Count(), clientResponse.Count());
                foreach (var category in clientResponse)
                {
                    Assert.IsNotNull(category.Code);
                    Assert.IsNotNull(category.Title);
                }
            }
        }

        [TestClass]
        public class UpdateStudentAttendanceAsync
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task UpdateStudentAttendanceAsync_StudentAttendanceNull()
            {
                //Arrange
                var mockHandler = new MockHandler();

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.UpdateStudentAttendanceAsync(null);
            }


            [TestMethod]
            public async Task UpdateStudentAttendanceAsync_UpdatedStudentAttendance()
            {
                // Arrange
                var studentAttendance = new StudentAttendance()
                {
                    SectionId = "12345",
                    StudentId = "studentId",
                    MeetingDate = DateTime.Today,
                    AttendanceCategoryCode = "P",
                    Comment = "This is the comment.",
                    StartTime = DateTimeOffset.Now.AddHours(-1),
                    EndTime = DateTimeOffset.Now,
                    InstructionalMethod = "LEC"

                };

                var serializedStudentAttendance = JsonConvert.SerializeObject(studentAttendance);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedStudentAttendance, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.UpdateStudentAttendanceAsync(studentAttendance);

                // Assert that the expected number of items is returned and each of the expected items is found in the response
                Assert.IsNotNull(clientResponse);
                Assert.IsInstanceOfType(clientResponse, typeof(StudentAttendance));
            }


        }

        [TestClass]
        public class GetNonAcademicAttendanceEventTypesAsync
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }



            [TestMethod]
            public async Task GetNonAcademicAttendanceEventTypesAsync_ReturnSerializedNonAcademicAttendanceEventTypes()
            {
                //Arrange
                var nonAcademicAttendanceEventTypeDtos = new List<Ellucian.Colleague.Dtos.Student.NonAcademicAttendanceEventType>()
                {
                    new Ellucian.Colleague.Dtos.Student.NonAcademicAttendanceEventType() { Code="CHAP", Description ="Chapel" },
                    new Ellucian.Colleague.Dtos.Student.NonAcademicAttendanceEventType() { Code="COMM", Description ="Community Service" },
                };

                var serializedResponse = JsonConvert.SerializeObject(nonAcademicAttendanceEventTypeDtos);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetNonAcademicAttendanceEventTypesAsync();

                // Assert that the expected number of items is returned and each of the expected items is found in the response
                Assert.IsNotNull(clientResponse);
                Assert.IsInstanceOfType(clientResponse, typeof(IEnumerable<Ellucian.Colleague.Dtos.Student.NonAcademicAttendanceEventType>));
                Assert.AreEqual(nonAcademicAttendanceEventTypeDtos.Count(), clientResponse.Count());
                foreach (var category in clientResponse)
                {
                    Assert.IsNotNull(category.Code);
                    Assert.IsNotNull(category.Description);
                }
            }


            [TestMethod]
            [ExpectedException(typeof(HttpRequestFailedException))]
            public async Task GetNonAcademicAttendanceEventTypesAsync_rethrows_caught_exceptions()
            {
                //Arrange
                var nonAcademicAttendanceEventTypeDtos = new List<Ellucian.Colleague.Dtos.Student.NonAcademicAttendanceEventType>()
                {
                    new Ellucian.Colleague.Dtos.Student.NonAcademicAttendanceEventType() { Code="CHAP", Description ="Chapel" },
                    new Ellucian.Colleague.Dtos.Student.NonAcademicAttendanceEventType() { Code="COMM", Description ="Community Service" },
                };

                var serializedResponse = JsonConvert.SerializeObject(nonAcademicAttendanceEventTypeDtos);

                // Arrange
                var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent(string.Empty, Encoding.UTF8, _contentType);
                response.RequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://test");
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetNonAcademicAttendanceEventTypesAsync();
                _loggerMock.Verify();
            }
        }

        [TestClass]
        public class GetNonAcademicAttendanceRequirementsAsync
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public async Task GetNonAcademicAttendanceRequirementsAsync_ReturnSerializedNonAcademicAttendanceRequirements()
            {
                //Arrange
                var personId = "0001234";
                var dtos = new List<NonAcademicAttendanceRequirement>()
                {
                    new NonAcademicAttendanceRequirement()
                    {
                        DefaultRequiredUnits = 30m,
                        Id = "1",
                        NonAcademicAttendanceIds = new List<string>() { "11", "12" },
                        PersonId = personId,
                        RequiredUnits = 24m,
                        RequiredUnitsOverride = 24m,
                        TermCode = "TERM"
                    },
                    new NonAcademicAttendanceRequirement()
                    {
                        DefaultRequiredUnits = 30m,
                        Id = "2",
                        NonAcademicAttendanceIds = new List<string>() { "13", "14" },
                        PersonId = personId,
                        RequiredUnits = 30m,
                        TermCode = "TERM2"
                    }
                };

                var serializedResponse = JsonConvert.SerializeObject(dtos);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetNonAcademicAttendanceRequirementsAsync(personId);

                // Assert that the expected number of items is returned and each of the expected items is found in the response
                Assert.IsNotNull(clientResponse);
                Assert.IsInstanceOfType(clientResponse, typeof(IEnumerable<Ellucian.Colleague.Dtos.Student.NonAcademicAttendanceRequirement>));
                Assert.AreEqual(dtos.Count(), clientResponse.Count());
                foreach (var category in clientResponse)
                {
                    Assert.IsNotNull(category.Id);
                    Assert.IsNotNull(category.NonAcademicAttendanceIds);
                    Assert.IsNotNull(category.PersonId);
                    Assert.IsNotNull(category.TermCode);
                }
            }


            [TestMethod]
            [ExpectedException(typeof(HttpRequestFailedException))]
            public async Task GetNonAcademicAttendanceRequirementsAsync_rethrows_caught_exceptions()
            {
                //Arrange
                var personId = "0001234";
                var dtos = new List<NonAcademicAttendanceRequirement>()
                {
                    new NonAcademicAttendanceRequirement()
                    {
                        DefaultRequiredUnits = 30m,
                        Id = "1",
                        NonAcademicAttendanceIds = new List<string>() { "11", "12" },
                        PersonId = personId,
                        RequiredUnits = 24m,
                        RequiredUnitsOverride = 24m,
                        TermCode = "TERM"
                    },
                    new NonAcademicAttendanceRequirement()
                    {
                        DefaultRequiredUnits = 30m,
                        Id = "2",
                        NonAcademicAttendanceIds = new List<string>() { "13", "14" },
                        PersonId = personId,
                        RequiredUnits = 30m,
                        TermCode = "TERM2"
                    }
                };

                var serializedResponse = JsonConvert.SerializeObject(dtos);

                // Arrange
                var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent(string.Empty, Encoding.UTF8, _contentType);
                response.RequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://test");
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetNonAcademicAttendanceRequirementsAsync(personId);
                _loggerMock.Verify();
            }
        }

        [TestClass]
        public class GetNonAcademicAttendancesAsync
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public async Task GetNonAcademicAttendancesAsync_ReturnSerializedNonAcademicAttendanceRequirements()
            {
                //Arrange
                var personId = "0001234";
                var dtos = new List<NonAcademicAttendance>()
                {
                    new NonAcademicAttendance()
                    {
                        Id = "1",
                        PersonId = personId,
                        UnitsEarned = 24m,
                        EventId = "11"
                    },
                    new NonAcademicAttendance()
                    {
                        Id = "2",
                        PersonId = personId,
                        UnitsEarned = 30m,
                        EventId = "12"
                    }
                };

                var serializedResponse = JsonConvert.SerializeObject(dtos);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetNonAcademicAttendancesAsync(personId);

                // Assert that the expected number of items is returned and each of the expected items is found in the response
                Assert.IsNotNull(clientResponse);
                Assert.IsInstanceOfType(clientResponse, typeof(IEnumerable<Ellucian.Colleague.Dtos.Student.NonAcademicAttendance>));
                Assert.AreEqual(dtos.Count(), clientResponse.Count());
                foreach (var category in clientResponse)
                {
                    Assert.IsNotNull(category.Id);
                    Assert.IsNotNull(category.EventId);
                    Assert.IsNotNull(category.PersonId);
                }
            }


            [TestMethod]
            [ExpectedException(typeof(HttpRequestFailedException))]
            public async Task GetNonAcademicAttendancesAsync_rethrows_caught_exceptions()
            {
                //Arrange
                var personId = "0001234";
                var dtos = new List<NonAcademicAttendance>()
                {
                    new NonAcademicAttendance()
                    {
                        Id = "1",
                        PersonId = personId,
                        UnitsEarned = 24m,
                        EventId = "11"
                    },
                    new NonAcademicAttendance()
                    {
                        Id = "2",
                        PersonId = personId,
                        UnitsEarned = 30m,
                        EventId = "12"
                    }
                };

                var serializedResponse = JsonConvert.SerializeObject(dtos);

                // Arrange
                var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent(string.Empty, Encoding.UTF8, _contentType);
                response.RequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://test");
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetNonAcademicAttendancesAsync(personId);
                _loggerMock.Verify();
            }
        }

        [TestClass]
        public class QueryNonacademicEventsByIdsAsync
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task QueryNonacademicEventsByIdsAsync_NullEventIds()
            {


                var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent(string.Empty, Encoding.UTF8, _contentType);
                response.RequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://test");
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var result = await client.QueryNonacademicEventsByIdsAsync(null);
                _loggerMock.Verify();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task QueryNonacademicEventsByIdsAsync_EmptyEventIds()
            {


                var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent(string.Empty, Encoding.UTF8, _contentType);
                response.RequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://test");
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var result = await client.QueryNonacademicEventsByIdsAsync(null);
                _loggerMock.Verify();
            }

            [TestMethod]
            public async Task QueryNonacademicEventsByIdsAsync_ReturnSerializedNonacademicEvents()
            {
                //Arrange
                var timeoffset = new TimeSpan(4, 0, 0);
                var eventIds = new List<string>() { "1", "2", "3" };
                var nonacademicEventDtos = new List<NonAcademicEvent>()
                {
                    new NonAcademicEvent() {Id = "1", BuildingCode = "BLD1", Date = DateTime.Today, EndTime = DateTime.Now.AddHours(5), EventTypeCode = "Type1", LocationCode = "LOC", RoomCode = "R1", StartTime = DateTime.Now, TermCode = "TERM1", Title = "Title for Event 1", Venue = "Venue for Event 1"},
                    new NonAcademicEvent() {Id = "2", BuildingCode = "BLD2", Date = DateTime.Today.AddDays(-10), TermCode = "TERM2", Title = "Title for Event 2"},
                    new NonAcademicEvent() {Id = "3"}

                };

                var serializedResponse = JsonConvert.SerializeObject(nonacademicEventDtos.AsEnumerable());

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.QueryNonacademicEventsByIdsAsync(eventIds);

                // Assert that the expected number of items is returned and each of the expected items is found in the response
                Assert.IsNotNull(clientResponse);
                Assert.IsInstanceOfType(clientResponse, typeof(IEnumerable<NonAcademicEvent>));
                Assert.AreEqual(nonacademicEventDtos.Count(), clientResponse.Count());
                foreach (var nevent in clientResponse)
                {
                    var expectedEvent = nonacademicEventDtos.Where(sa => sa.Id == nevent.Id).FirstOrDefault();
                    Assert.IsNotNull(expectedEvent);
                    Assert.AreEqual(expectedEvent.Id, nevent.Id);
                    Assert.AreEqual(expectedEvent.BuildingCode, nevent.BuildingCode);
                    Assert.AreEqual(expectedEvent.Date, nevent.Date);
                    Assert.AreEqual(expectedEvent.EndTime, nevent.EndTime);
                    Assert.AreEqual(expectedEvent.EventTypeCode, nevent.EventTypeCode);
                    Assert.AreEqual(expectedEvent.LocationCode, nevent.LocationCode);
                    Assert.AreEqual(expectedEvent.RoomCode, nevent.RoomCode);
                    Assert.AreEqual(expectedEvent.StartTime, nevent.StartTime);
                    Assert.AreEqual(expectedEvent.TermCode, nevent.TermCode);
                    Assert.AreEqual(expectedEvent.Title, nevent.Title);
                    Assert.AreEqual(expectedEvent.Venue, nevent.Venue);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpRequestFailedException))]
            public async Task QueryNonacademicEventsByIdsAsync_rethrows_caught_exceptions()
            {
                //Arrange
                //Arrange
                var timeoffset = new TimeSpan(4, 0, 0);
                var eventIds = new List<string>() { "1", "2", "3" };
                var nonacademicEventDtos = new List<NonAcademicEvent>()
                {
                    new NonAcademicEvent() {Id = "1", BuildingCode = "BLD1", Date = DateTime.Today, EndTime = DateTime.Now.AddHours(5), EventTypeCode = "Type1", LocationCode = "LOC", RoomCode = "R1", StartTime = DateTime.Now, TermCode = "TERM1", Title = "Title for Event 1", Venue = "Venue for Event 1"},
                    new NonAcademicEvent() {Id = "2", BuildingCode = "BLD2", Date = DateTime.Today.AddDays(-10), TermCode = "TERM2", Title = "Title for Event 2"},
                    new NonAcademicEvent() {Id = "3"}

                };

                var serializedResponse = JsonConvert.SerializeObject(nonacademicEventDtos.AsEnumerable());

                // Arrange
                var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent(string.Empty, Encoding.UTF8, _contentType);
                response.RequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://test");
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.QueryNonacademicEventsByIdsAsync(eventIds);
                _loggerMock.Verify();
            }

        }

        [TestClass]
        public class UpdateSectionAttendancesAsync
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task UpdateSectionAttendanceAsync_SectionAttendanceNull()
            {
                //Arrange
                var mockHandler = new MockHandler();

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.UpdateSectionAttendancesAsync(null);
            }


            [TestMethod]
            public async Task UpdateSectionAttendancesAsync_UpdatedSectionAttendances()
            {
                // Arrange
                var sectionAttendance = new SectionAttendance()
                {
                    SectionId = "12345",
                    MeetingInstance = new SectionMeetingInstance()
                    {
                        MeetingDate = DateTime.Today,
                        StartTime = DateTimeOffset.Now.AddHours(-1),
                        EndTime = DateTimeOffset.Now,
                        InstructionalMethod = "LEC",
                        Id = "1111",
                        SectionId = "12345"
                    },
                    StudentAttendances = new List<StudentSectionAttendance>()
                    {
                        new StudentSectionAttendance()
                        {
                            AttendanceCategoryCode = "P",
                            Comment = "This is the comment.",
                            StudentCourseSectionId = "10"
                        },
                        new StudentSectionAttendance()
                        {
                            AttendanceCategoryCode = "L",
                            StudentCourseSectionId = "11"
                        }
                    }
                };

                // Arrange
                var sectionAttendanceResponse = new SectionAttendanceResponse()
                {
                    SectionId = "12345",
                    MeetingInstance = new SectionMeetingInstance()
                    {
                        MeetingDate = DateTime.Today,
                        StartTime = DateTimeOffset.Now.AddHours(-1),
                        EndTime = DateTimeOffset.Now,
                        InstructionalMethod = "LEC",
                        Id = "1111",
                        SectionId = "12345"
                    },
                    UpdatedStudentAttendances = new List<StudentAttendance>()
                    {
                        new StudentAttendance()
                        {
                            StudentId = "studentId",
                            AttendanceCategoryCode = "P",
                            Comment = "This is the comment.",
                            StudentCourseSectionId = "10"
                        },
                    },
                    StudentAttendanceErrors = new List<StudentSectionAttendanceError>()
                    {
                        new StudentSectionAttendanceError()
                        {
                            StudentCourseSectionId = "11",
                            ErrorMessage = "Locked"
                        }
                    }
                };

                var serializedSectionAttendance = JsonConvert.SerializeObject(sectionAttendance);
                var serializedSectionAttendanceResponse = JsonConvert.SerializeObject(sectionAttendanceResponse);
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedSectionAttendanceResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.UpdateSectionAttendancesAsync(sectionAttendance);

                // Assert that the expected number of items is returned and each of the expected items is found in the response
                Assert.IsNotNull(clientResponse);
                Assert.IsInstanceOfType(clientResponse, typeof(SectionAttendanceResponse));
                Assert.AreEqual("12345", clientResponse.SectionId);
                Assert.IsInstanceOfType(clientResponse.MeetingInstance, typeof(SectionMeetingInstance));
            }


        }

        [TestClass]
        public class GetSectionRosterStudentsAsync
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public async Task GetSectionRosterStudentsAsync_ReturnSerializedRosterStudents()
            {
                //Arrange
                var sectionId = "123";
                var dtos = new List<RosterStudent>()
                {
                    new RosterStudent()
                    {
                        Id = "0001234",
                        FirstName = "John",
                        LastName = "Smith"
                    },
                    new RosterStudent()
                    {
                        Id = "0005678",
                        FirstName = "Jane",
                        LastName = "Doe"
                    }
                };

                var serializedResponse = JsonConvert.SerializeObject(dtos);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetSectionRosterStudentsAsync(sectionId);

                // Assert that the expected number of items is returned and each of the expected items is found in the response
                Assert.IsNotNull(clientResponse);
                Assert.IsInstanceOfType(clientResponse, typeof(IEnumerable<Ellucian.Colleague.Dtos.Student.RosterStudent>));
                Assert.AreEqual(dtos.Count(), clientResponse.Count());
                foreach (var category in clientResponse)
                {
                    Assert.IsNotNull(category.Id);
                    Assert.IsNotNull(category.FirstName);
                    Assert.IsNotNull(category.LastName);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpRequestFailedException))]
            public async Task GetSectionRosterStudentsAsync_rethrows_caught_exceptions()
            {
                //Arrange
                var personId = "0001234";
                var dtos = new List<NonAcademicAttendance>()
                {
                    new NonAcademicAttendance()
                    {
                        Id = "1",
                        PersonId = personId,
                        UnitsEarned = 24m,
                        EventId = "11"
                    },
                    new NonAcademicAttendance()
                    {
                        Id = "2",
                        PersonId = personId,
                        UnitsEarned = 30m,
                        EventId = "12"
                    }
                };

                var serializedResponse = JsonConvert.SerializeObject(dtos);

                // Arrange
                var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent(string.Empty, Encoding.UTF8, _contentType);
                response.RequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://test");
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetSectionRosterStudentsAsync(personId);
                _loggerMock.Verify();
            }
        }

        [TestClass]
        public class GetSectionRosterStudents2Async
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public async Task GetSectionRosterStudents2Async_ReturnSerializedRosterStudents_UseCache_True()
            {
                //Arrange
                var sectionId = "123";
                var dto = new SectionRoster()
                {
                    SectionId = sectionId,
                    StudentIds = new List<string>() { "0001234", "0001235" },
                    FacultyIds = new List<string>() { "0001236", "0001237" }
                };

                var serializedResponse = JsonConvert.SerializeObject(dto);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetSectionRosterStudents2Async(sectionId);

                // Assert that theitem is returned and each item property is found in the response
                Assert.IsNotNull(clientResponse);
                Assert.IsInstanceOfType(clientResponse, typeof(SectionRoster));
                Assert.AreEqual(dto.SectionId, clientResponse.SectionId);
                CollectionAssert.AreEqual(dto.StudentIds.ToList(), clientResponse.StudentIds.ToList());
                CollectionAssert.AreEqual(dto.FacultyIds.ToList(), clientResponse.FacultyIds.ToList());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetSectionRosterStudents2Async_null_SectionId_throws_exception()
            {
                //Arrange
                var sectionId = "123";
                var dto = new SectionRoster()
                {
                    SectionId = sectionId,
                    StudentIds = new List<string>() { "0001234", "0001235" },
                    FacultyIds = new List<string>() { "0001236", "0001237" }
                };

                var serializedResponse = JsonConvert.SerializeObject(dto);

                // Arrange
                var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent(string.Empty, Encoding.UTF8, _contentType);
                response.RequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://test");
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetSectionRosterStudents2Async(null);
                _loggerMock.Verify();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpRequestFailedException))]
            public async Task GetSectionRosterStudents2Async_rethrows_caught_exceptions()
            {
                //Arrange
                var sectionId = "123";
                var dto = new SectionRoster()
                {
                    SectionId = sectionId,
                    StudentIds = new List<string>() { "0001234", "0001235" },
                    FacultyIds = new List<string>() { "0001236", "0001237" }
                };

                var serializedResponse = JsonConvert.SerializeObject(dto);

                // Arrange
                var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent(string.Empty, Encoding.UTF8, _contentType);
                response.RequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://test");
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetSectionRosterStudents2Async(sectionId);
                _loggerMock.Verify();
            }
        }

        [TestClass]
        public class UpdateAddAuthorizationAsync
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task UpdateAddAuthorizationAsync_AuthorizationNull()
            {
                //Arrange
                var mockHandler = new MockHandler();

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.UpdateAddAuthorizationAsync(null);
            }

            [TestMethod]
            public async Task UpdateAddAuthorizationAsync_Updated()
            {
                // Arrange
                var addAuthorization = new AddAuthorization()
                {
                    SectionId = "12345",
                    AddAuthorizationCode = "abcd1234",
                    Id = "authId",
                    StudentId = "StudentId"

                };

                var serializedAddAuthorization = JsonConvert.SerializeObject(addAuthorization);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedAddAuthorization, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.UpdateAddAuthorizationAsync(addAuthorization);

                // Assert that the expected number of items is returned and each of the expected items is found in the response
                Assert.IsNotNull(clientResponse);
                Assert.IsInstanceOfType(clientResponse, typeof(AddAuthorization));
            }


        }

        [TestClass]
        public class GetSectionAddAuthorizationsAsync
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public async Task GetSectionAddAuthorizationsAsync_ReturnSerializedAddAuthorizations()
            {
                //Arrange
                List<AddAuthorization> addAuthorizationDtos = new List<Dtos.Student.AddAuthorization>();
                string sectionId = "section/Id";
                var addAuthorizationDto1 = new Ellucian.Colleague.Dtos.Student.AddAuthorization()
                {
                    StudentId = "studentId1",
                    SectionId = "section/Id",
                    Id = "authID1",
                    AddAuthorizationCode = "addCode",
                    AssignedBy = "facultyId"
                };
                addAuthorizationDtos.Add(addAuthorizationDto1);
                var addAuthorizationDto2 = new Ellucian.Colleague.Dtos.Student.AddAuthorization()
                {
                    StudentId = "studentId2",
                    SectionId = "section/Id",
                    Id = "authID2",
                    AddAuthorizationCode = "addCode",
                    AssignedBy = "facultyId"
                };
                addAuthorizationDtos.Add(addAuthorizationDto2);

                var serializedResponse = JsonConvert.SerializeObject(addAuthorizationDtos.AsEnumerable());

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetSectionAddAuthorizationsAsync(sectionId);

                // Assert that theitem is returned and each item property is found in the response
                Assert.IsNotNull(clientResponse);
                Assert.IsInstanceOfType(clientResponse, typeof(IEnumerable<AddAuthorization>));
                Assert.AreEqual(addAuthorizationDtos.Count(), clientResponse.Count());
                foreach (var resultDto in clientResponse)
                {
                    var expectedDto = addAuthorizationDtos.Where(aa => aa.Id == resultDto.Id).FirstOrDefault();
                    Assert.AreEqual(expectedDto.StudentId, resultDto.StudentId);
                    Assert.AreEqual(expectedDto.SectionId, resultDto.SectionId);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetSectionAddAuthorizationsAsyncc_null_SectionId_throws_exception()
            {


                // Arrange
                var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent(string.Empty, Encoding.UTF8, _contentType);
                response.RequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://test");
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetSectionAddAuthorizationsAsync(null);
                _loggerMock.Verify();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpRequestFailedException))]
            public async Task GetSectionAddAuthorizationsAsyncc_rethrows_caught_exceptions()
            {
                //Arrange
                var sectionId = "123";

                // Arrange
                var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent(string.Empty, Encoding.UTF8, _contentType);
                response.RequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://test");
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetSectionAddAuthorizationsAsync(sectionId);
                _loggerMock.Verify();
            }
        }

        [TestClass]
        public class GetDropReasonsAsync
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public async Task ClientGetDropReasonsAsync_ReturnsSerializedDropReasons()
            {
                // Arrange
                var dropReasons = new List<DropReason>()
                    {
                        new DropReason(){Code="C",Description="Changed Mind", DisplayInSelfService=false},
                        new DropReason(){Code="D",Description="Difficult", DisplayInSelfService=true},
                        new DropReason(){Code="W",Description="My own wish", DisplayInSelfService = false}
                    };
                var serializedResponse = JsonConvert.SerializeObject(dropReasons);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetDropReasonsAsync();

                // Assert that the expected number of items is returned and each of the expected items is found in the response
                Assert.IsNotNull(clientResponse);
                Assert.AreEqual(dropReasons.Count(), clientResponse.Count());
                foreach (var dropReason in clientResponse)
                {
                    Assert.IsNotNull(dropReason.Code);
                    Assert.IsNotNull(dropReason.Description);
                    var reason = dropReasons.Where(c => c.Code == dropReason.Code).FirstOrDefault();
                    Assert.AreEqual(reason.Code, dropReason.Code);
                    Assert.AreEqual(reason.Description, dropReason.Description);
                    Assert.AreEqual(reason.DisplayInSelfService, dropReason.DisplayInSelfService);
                }
            }

        }

        [TestClass]
        public class CreateAddAuthorizationAsync
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public async Task Client_CreateAddAuthorizationAsync_ReturnsNewAuthorization()
            {
                // Arrange
                var addAuthInput = new Dtos.Student.AddAuthorizationInput()
                {
                    StudentId = "STU1",
                    SectionId = "SEC1",
                    AssignedBy = "Faculty1",
                    AssignedTime = DateTime.Now.AddHours(-2)
                };

                var serializedResponse = JsonConvert.SerializeObject(addAuthInput);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.CreateAddAuthorizationAsync(addAuthInput);

                // Assert that the expected item is found in the response
                Assert.IsTrue(clientResponse is AddAuthorization);
                Assert.AreEqual(addAuthInput.StudentId, clientResponse.StudentId);
                Assert.AreEqual(addAuthInput.SectionId, clientResponse.SectionId);
                Assert.AreEqual(addAuthInput.AssignedBy, clientResponse.AssignedBy);
                Assert.AreEqual(addAuthInput.AssignedTime, clientResponse.AssignedTime);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Client_CreateAddAuthorizationAsync_ThrowsExceptionIfNullInput()
            {
                // Arrange
                var mockHandler = new MockHandler();
                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.CreateAddAuthorizationAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task Client_CreateAddAuthorizationAsync_ThrowsExceptionIfNoStudentId()
            {
                // Arrange
                var addAuthInput = new Dtos.Student.AddAuthorizationInput()
                {
                    SectionId = "SEC1",
                    AssignedBy = "Faculty1",
                    AssignedTime = DateTime.Now.AddHours(-2)
                };

                var serializedResponse = JsonConvert.SerializeObject(addAuthInput);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.CreateAddAuthorizationAsync(addAuthInput);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task Client_CreateAddAuthorizationAsync_ThrowsExceptionIfNoSectionId()
            {
                // Arrange
                var addAuthInput = new Dtos.Student.AddAuthorizationInput()
                {
                    StudentId = "STU1",
                    AssignedBy = "Faculty1",
                    AssignedTime = DateTime.Now.AddHours(-2)
                };

                var serializedResponse = JsonConvert.SerializeObject(addAuthInput);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.CreateAddAuthorizationAsync(addAuthInput);
            }
        }

        [TestClass]
        public class GetAddAuthorizationAsync
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public async Task GetAddAuthorizationAsync_ReturnSerializedAddAuthorization()
            {
                //Arrange
                AddAuthorization addAuthorizationDto = new Ellucian.Colleague.Dtos.Student.AddAuthorization()
                {
                    StudentId = "studentId1",
                    SectionId = "section/Id",
                    Id = "authId",
                    AddAuthorizationCode = "addCode",
                    AssignedBy = "facultyId"
                };

                var serializedResponse = JsonConvert.SerializeObject(addAuthorizationDto);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetAddAuthorizationAsync("authId");

                // Assert that theitem is returned and each item property is found in the response
                Assert.IsNotNull(clientResponse);
                Assert.IsInstanceOfType(clientResponse, typeof(AddAuthorization));
                Assert.AreEqual(addAuthorizationDto.StudentId, clientResponse.StudentId);
                Assert.AreEqual(addAuthorizationDto.SectionId, clientResponse.SectionId);
                Assert.AreEqual(addAuthorizationDto.Id, clientResponse.Id);
                Assert.AreEqual(addAuthorizationDto.AddAuthorizationCode, clientResponse.AddAuthorizationCode);
                Assert.AreEqual(addAuthorizationDto.AssignedBy, clientResponse.AssignedBy);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetAddAuthorizationAsync_nullId_throws_exception()
            {


                // Arrange
                var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent(string.Empty, Encoding.UTF8, _contentType);
                response.RequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://test");
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetAddAuthorizationAsync(null);
                _loggerMock.Verify();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpRequestFailedException))]
            public async Task GetSectionAddAuthorizationsAsync_rethrows_caught_exceptions()
            {
                // Arrange
                var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent(string.Empty, Encoding.UTF8, _contentType);
                response.RequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://test");
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetAddAuthorizationAsync("authId");
                _loggerMock.Verify();
            }
        }

        [TestClass]
        public class ServiceClient_GetFacultyPermissions2Async_Tests
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }


            [TestMethod]
            [ExpectedException(typeof(HttpRequestFailedException))]
            public async Task ServiceClient_GetFacultyPermissions2Async_error_rethrows_caught_exception()
            {
                //Arrange
                FacultyPermissions permissions = new FacultyPermissions()
                {
                    CanGrantFacultyConsent = true,
                    CanGrantStudentPetition = true,
                    CanUpdateGrades = true,
                    CanWaivePrerequisiteRequirement = true,

                };

                // Arrange
                var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent(string.Empty, Encoding.UTF8, _contentType);
                response.RequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://test");
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetFacultyPermissions2Async();
                _loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), "An error occurred while retrieving Faculty Permissions."));
            }

            [TestMethod]
            public async Task ServiceClient_GetFacultyPermissions2Async_Success()
            {
                //Arrange
                FacultyPermissions permissions = new FacultyPermissions()
                {
                    CanGrantFacultyConsent = true,
                    CanGrantStudentPetition = true,
                    CanUpdateGrades = true,
                    CanWaivePrerequisiteRequirement = true,

                };
                var serializedResponse = JsonConvert.SerializeObject(permissions);
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);
                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);
                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetFacultyPermissions2Async();

                // Assert that theitem is returned and each item property is found in the response
                Assert.IsNotNull(clientResponse);
                Assert.IsInstanceOfType(clientResponse, typeof(FacultyPermissions));
            }
        }

        [TestClass]
        public class ServiceClient_QueryStudentsById4Async_Tests
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ServiceClient_QueryStudentsById4Async_null_student_IDs_throws_exception()
            {
                // Arrange
                var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent(string.Empty, Encoding.UTF8, _contentType);
                response.RequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://test");
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.QueryStudentsById4Async(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ServiceClient_QueryStudentsById4Async_empty_student_IDs_throws_exception()
            {
                // Arrange
                var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent(string.Empty, Encoding.UTF8, _contentType);
                response.RequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://test");
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.QueryStudentsById4Async(new List<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpRequestFailedException))]
            public async Task ServiceClient_QueryStudentsById4Async_error_rethrows_caught_exception()
            {
                // Arrange
                var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent(string.Empty, Encoding.UTF8, _contentType);
                response.RequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://test");
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.QueryStudentsById4Async(new List<string>() { "0001234" });
                _loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), "Unable to retrieve students."));
            }

            [TestMethod]
            public async Task ServiceClient_QueryStudentsById4Async_Success()
            {
                //Arrange
                var serializedResponse = JsonConvert.SerializeObject(new List<StudentBatch3>() { new StudentBatch3() { Id = "0001234", LastName = "Smith" } });
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);
                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);
                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.QueryStudentsById4Async(new List<string>() { "0001234" });

                // Assert that theitem is returned and each item property is found in the response
                Assert.IsNotNull(clientResponse);
                Assert.IsInstanceOfType(clientResponse, typeof(IEnumerable<StudentBatch3>));
            }
        }

        [TestClass]
        public class ServiceClient_QueryStudents4ById_Tests
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ServiceClient_QueryStudents4ById_null_student_IDs_throws_exception()
            {
                // Arrange
                var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent(string.Empty, Encoding.UTF8, _contentType);
                response.RequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://test");
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = client.QueryStudents4ById(null, null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpRequestFailedException))]
            public void ServiceClient_QueryStudents4ById_error_rethrows_caught_exception()
            {
                // Arrange
                var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent(string.Empty, Encoding.UTF8, _contentType);
                response.RequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://test");
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = client.QueryStudents4ById(new List<string>() { "0001234" }, "2018/FA");
                _loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), "Unable to retrieve students."));
            }

            [TestMethod]
            public void ServiceClient_QueryStudents4ById_Success()
            {
                //Arrange
                var serializedResponse = JsonConvert.SerializeObject(new List<StudentBatch3>() { new StudentBatch3() { Id = "0001234", LastName = "Smith" } });
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);
                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);
                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = client.QueryStudents4ById(new List<string>() { "0001234" }, "2018/FA");

                // Assert that theitem is returned and each item property is found in the response
                Assert.IsNotNull(clientResponse);
                Assert.IsInstanceOfType(clientResponse, typeof(IEnumerable<StudentBatch3>));
            }
        }

        [TestClass]
        public class ServiceClient_QueryStudentsSearchAsync_Tests
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task ServiceClient_QueryStudentsSearchAsync_null_keyword_throws_exception()
            {
                // Arrange
                var mockHandler = new MockHandler();
                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.QueryStudentsSearchAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task ServiceClient_QueryStudentsSearchAsync_unauthorized_request_throws_ApplicationException()
            {
                // Arrange
                var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
                response.Content = new StringContent(string.Empty, Encoding.UTF8, _contentType);
                response.RequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://test");
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.QueryStudentsSearchAsync("0001234");
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task ServiceClient_QueryStudentsSearchAsync_bad_request_throws_ApplicationException()
            {
                // Arrange
                var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent(string.Empty, Encoding.UTF8, _contentType);
                response.RequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://test");
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.QueryStudentsSearchAsync("0001234");
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task ServiceClient_QueryStudentsSearchAsync_caught_exception_throws_ApplicationException()
            {
                // Arrange
                var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = null;
                response.RequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://test");
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.QueryStudentsSearchAsync("0001234");
                _loggerMock.Verify(l => l.Error(It.IsAny<string>()));
            }

            [TestMethod]
            public async Task ServiceClient_QueryStudentsSearchAsync_Success()
            {
                //Arrange
                var serializedResponse = JsonConvert.SerializeObject(new List<Dtos.Student.Student>() { new Dtos.Student.Student() { Id = "0001234", LastName = "Smith" } });
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);
                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);
                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.QueryStudentsSearchAsync("0001234");

                // Assert that theitem is returned and each item property is found in the response
                Assert.IsNotNull(clientResponse);
                Assert.IsInstanceOfType(clientResponse, typeof(IEnumerable<Dtos.Student.Student>));
            }
        }

        [TestClass]
        public class ServiceClient_QuerySectionEventsICalAsync_Tests
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ServiceClient_QuerySectionEventsICalAsync_null_criteria_throws_exception()
            {
                // Arrange
                var mockHandler = new MockHandler();
                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.QuerySectionEventsICalAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ServiceClient_QuerySectionEventsICalAsync_null_sectionIds_throws_Exception()
            {
                SectionEventsICalQueryCriteria criteria = new SectionEventsICalQueryCriteria();
                criteria.SectionIds = null;
                // Arrange
                var mockHandler = new MockHandler();
                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.QuerySectionEventsICalAsync(criteria);
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ServiceClient_QuerySectionEventsICalAsync_empty_sectionIds_throws_Exception()
            {
                SectionEventsICalQueryCriteria criteria = new SectionEventsICalQueryCriteria();
                criteria.SectionIds = new List<string> ();
                // Arrange
                var mockHandler = new MockHandler();
                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.QuerySectionEventsICalAsync(criteria);
            }

            [TestMethod]
            public async Task ServiceClient_QuerySectionEventsICalAsync_bad_request_returns_emptyString()
            {
                SectionEventsICalQueryCriteria criteria = new SectionEventsICalQueryCriteria();
                criteria.SectionIds = new List<string>() {"s001" };
                // Arrange
                var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent(string.Empty, Encoding.UTF8, _contentType);
                response.RequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://test");
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.QuerySectionEventsICalAsync(criteria);
                Assert.AreEqual(string.Empty, clientResponse);
            }

         

            [TestMethod]
            public async Task ServiceClient_QQuerySectionEventsICalAsync_Success()
            {
                SectionEventsICalQueryCriteria criteria = new SectionEventsICalQueryCriteria();
                criteria.SectionIds = new List<string>() { "s001" };
                var serializedResponse = JsonConvert.SerializeObject(new EventsICal("raw ical"));
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);
                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);
                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.QuerySectionEventsICalAsync(criteria);

                // Assert that theitem is returned and each item property is found in the response
                Assert.IsNotNull(clientResponse);
                Assert.IsInstanceOfType(clientResponse, typeof(string) );
                Assert.AreEqual("raw ical", clientResponse);
            }
        }


        [TestClass]
        public class ServiceClient_QueryGraduationApplicationEligibilityAsync_Tests
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private List<string> programCodes;
            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                programCodes = new List<string>() { "PROG1", "PROG2" };
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ServiceClient_QueryGraduationApplicationEligibilityAsync_NullStudentId_throws_Exception()
            {
                // Arrange
                var mockHandler = new MockHandler();
                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.QueryGraduationApplicationEligibilityAsync(null, programCodes);
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ServiceClient_QueryGraduationApplicationEligibilityAsync_EmptyStudentId_throws_Exception()
            {
 
                // Arrange
                var mockHandler = new MockHandler();
                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.QueryGraduationApplicationEligibilityAsync(string.Empty, programCodes);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ServiceClient_QueryGraduationApplicationEligibilityAsync_NullProgramCodes_throws_Exception()
            {
                // Arrange
                var mockHandler = new MockHandler();
                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.QueryGraduationApplicationEligibilityAsync("studentId", null);
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ServiceClient_QueryGraduationApplicationEligibilityAsync_EmptyProgramCodes_throws_Exception()
            {
                // Arrange
                var mockHandler = new MockHandler();
                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.QueryGraduationApplicationEligibilityAsync("studentId", new List<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task ServiceClient_QueryGraduationApplicationEligibilityAsync_bad_request_Throws()
            {
                // Arrange
                var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent(string.Empty, Encoding.UTF8, _contentType);
                response.RequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://test");
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.QueryGraduationApplicationEligibilityAsync("studentId", programCodes);

            }



            [TestMethod]
            public async Task ServiceClient_QueryGraduationApplicationEligibilityAsync_Success()
            {
                // Build return object
                var graduationApplicationEligDtos = new List<Dtos.Student.GraduationApplicationProgramEligibility>()
                {
                       new Dtos.Student.GraduationApplicationProgramEligibility() { ProgramCode = "Program1", IsEligible = true, IneligibleMessages = new List<string>()},
                       new Dtos.Student.GraduationApplicationProgramEligibility() { ProgramCode = "Program2", IsEligible = false, IneligibleMessages = new List<string>() {"string1", "string2" } }
                };

                var serializedResponse = JsonConvert.SerializeObject(graduationApplicationEligDtos);
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);
                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);
                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.QueryGraduationApplicationEligibilityAsync("studentId", programCodes);

                // Assert that result is returned and each item property is found in the response
                Assert.AreEqual(2, clientResponse.Count());
                var result1 = clientResponse.ElementAt(0);
                Assert.IsInstanceOfType(result1, typeof(GraduationApplicationProgramEligibility));
                Assert.AreEqual("Program1", result1.ProgramCode);
                Assert.IsTrue(result1.IsEligible);
                Assert.AreEqual(0, result1.IneligibleMessages.Count());
                var result2 = clientResponse.ElementAt(1);
                Assert.IsInstanceOfType(result2, typeof(GraduationApplicationProgramEligibility));
                Assert.AreEqual("Program2", result2.ProgramCode);
                Assert.IsFalse(result2.IsEligible);
                Assert.AreEqual(2, result2.IneligibleMessages.Count());

            }
        }

    }

}

