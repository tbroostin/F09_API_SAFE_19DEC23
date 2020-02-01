// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Text;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Newtonsoft.Json;
using Ellucian.Web.Http.TestUtil;
using Ellucian.Web.Infrastructure.TestUtil;
using Ellucian.Rest.Client.Exceptions;
using Ellucian.Colleague.Api.Client.Exceptions;
using Ellucian.Colleague.Dtos.Planning;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Colleague.Dtos.Student.DegreePlans;

namespace Ellucian.Colleague.Api.Client.Tests
{
    [TestClass]
    public class ColleagueApiClientPlanningTests
    {
        #region Constants

        private const string _serviceUrl = "http://service.url";
        private const string _contentType = "application/json";
        private const string _studentId = "123456";
        private const string _studentId2 = "678";
        private const string _token = "1234567890";
        private const string _courseId = "MATH-100";
        private const string _courseId2 = "ENGL-101";

        #endregion

        private Mock<ILogger> _loggerMock;
        private ILogger _logger;

        [TestInitialize]
        public void Initialize()
        {
            _loggerMock = MockLogger.Instance;

            _logger = _loggerMock.Object;
        }

        #region Add Degree Plan Tests

        // use this to model preview
        [TestMethod]
        public async Task GetDegreePlanPreview5Async()
        {
            // Arrange
            var degreePlan1 = new DegreePlan4()
            {
                Id = 12345,
                NonTermPlannedCourses = new List<PlannedCourse4>(),
                PersonId = "12345",
                Terms = new List<DegreePlanTerm4>(),
                Version = 1
            };

            var degreePlan2 = new DegreePlan4()
            {
                Id = 123451,
                NonTermPlannedCourses = new List<PlannedCourse4>(),
                PersonId = "12345",
                Terms = new List<DegreePlanTerm4>(),
                Version = 1
            };

            var academicHistoryResponse = new AcademicHistory3()
            {
                StudentId = "12345",
                AcademicTerms = new List<AcademicTerm3>(),
            };

            var degreePlanPreviewResponse = new DegreePlanPreview5()
            {
                Preview = degreePlan1,
                MergedDegreePlan = degreePlan2,
                AcademicHistory = academicHistoryResponse
            };

            var serializedResponse = JsonConvert.SerializeObject(degreePlanPreviewResponse);

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, _logger);

            // Act
            var result = await client.PreviewSampleDegreePlan5Async(degreePlan1.Id, "ENGL+BA", "2015FA");

            // Assert
            Assert.AreEqual(degreePlan1.Id, result.Preview.Id);
            Assert.AreEqual(degreePlan1.PersonId, result.Preview.PersonId);
            Assert.AreEqual(degreePlan1.Version, result.Preview.Version);
            Assert.AreEqual(academicHistoryResponse.StudentId, result.AcademicHistory.StudentId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetDegreePlanPreview5Async_EmptyProgramId()
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
            var thisShouldFail = await client.PreviewSampleDegreePlan5Async(12345, "", "COMP+BS");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetDegreePlanPreview5Async_EmptyTerm()
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
            var thisShouldFail = await client.PreviewSampleDegreePlan5Async(12345, "12345", "");
        }

        /// 
        /// Tests for PreviewSamplePlan6Async
        /// 
        [TestMethod]
        public async Task GetDegreePlanPreview6Async()
        {
            // Arrange
            var degreePlan1 = new DegreePlan4()
            {
                Id = 12345,
                NonTermPlannedCourses = new List<PlannedCourse4>(),
                PersonId = "12345",
                Terms = new List<DegreePlanTerm4>(),
                Version = 1
            };

            var degreePlan2 = new DegreePlan4()
            {
                Id = 123451,
                NonTermPlannedCourses = new List<PlannedCourse4>(),
                PersonId = "12345",
                Terms = new List<DegreePlanTerm4>(),
                Version = 1
            };

            var academicHistoryResponse = new AcademicHistory4()
            {
                StudentId = "12345",
                AcademicTerms = new List<AcademicTerm4>(),
            };

            var degreePlanPreviewResponse = new DegreePlanPreview6()
            {
                Preview = degreePlan1,
                MergedDegreePlan = degreePlan2,
                AcademicHistory = academicHistoryResponse
            };

            var serializedResponse = JsonConvert.SerializeObject(degreePlanPreviewResponse);

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, _logger);

            // Act
            var result = await client.PreviewSampleDegreePlan6Async(degreePlan1.Id, "ENGL+BA", "2015FA");

            // Assert
            Assert.IsInstanceOfType(result, typeof(DegreePlanPreview6));
            Assert.AreEqual(degreePlan1.Id, result.Preview.Id);
            Assert.AreEqual(degreePlan1.PersonId, result.Preview.PersonId);
            Assert.AreEqual(degreePlan1.Version, result.Preview.Version);
            Assert.AreEqual(academicHistoryResponse.StudentId, result.AcademicHistory.StudentId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetDegreePlanPreview6Async_ZeroDegreePlanId()
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
            var thisShouldFail = await client.PreviewSampleDegreePlan6Async(0, "ENGL+BA", "COMP+BS");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetDegreePlanPreview6Async_EmptyProgramId()
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
            var thisShouldFail = await client.PreviewSampleDegreePlan6Async(12345, "", "COMP+BS");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetDegreePlanPreview6Async_EmptyTerm()
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
            var thisShouldFail = await client.PreviewSampleDegreePlan6Async(12345, "12345", "");
        }

        #endregion

        #region MediaTypeHeaderTests

        // Verify the logic in ExecuteGetRequestWithResponse to remove the previous media type header
        // from the request even using a cached http client. Assert checks for only two headers, one
        // for custom credentials, the other for media type. Any more will cause the test to fail, 
        // presuming additional items are extraneous media type headers.
        [TestMethod]
        public void GetWithResponseHeader()
        {

            // Arrange
            string advisorId1 = "123";
            var advisorResponse1 = new Advisor()
            {
                Id = advisorId1,
                LastName = "Smith",
                FirstName = "George",
                MiddleName = "Mullins"
            };

            string advisorId2 = "456";
            var advisorResponse2 = new Advisor()
            {
                Id = advisorId2,
                LastName = "Jones",
                FirstName = "Janet",
                MiddleName = "Quincy"
            };

            // first response
            var serializedResponse1 = JsonConvert.SerializeObject(advisorResponse1);
            var response1 = new HttpResponseMessage(HttpStatusCode.OK);
            response1.Content = new StringContent(serializedResponse1, Encoding.UTF8, _contentType);

            // second response
            var serializedResponse2 = JsonConvert.SerializeObject(advisorResponse2);
            var response2 = new HttpResponseMessage(HttpStatusCode.OK);
            response2.Content = new StringContent(serializedResponse2, Encoding.UTF8, _contentType);

            // Enque two responses
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response1);
            mockHandler.Responses.Enqueue(response2);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, _logger);

            // Act
            var result1 = client.GetAdvisor(advisorId1);
            // Assert to check response
            Assert.AreEqual(advisorResponse1.Id, result1.Id);
            // verify the media header is present
            var mediaHeader = mockHandler.Request.Headers.FirstOrDefault(x => x.Key == ColleagueApiClient.AcceptHeaderKey);
            Assert.IsNotNull(mediaHeader.Value.First());

            // Act and assert second request
            var result2 = client.GetAdvisor(advisorId2);
            // Assert to verify response
            Assert.AreEqual(advisorResponse2.Id, result2.Id);
            // Expect one version header
            Assert.AreEqual(1, mockHandler.Request.Headers.Count());
        }

        // Verify that the extraneous media header is removed from get request headers
        // when followed by a post that uses the cached http object
        [TestMethod]
        public void GetThenPostMediaHeader()
        {
            // Arrange
            string advisorId1 = "123";
            var advisorResponse1 = new Advisor()
            {
                Id = advisorId1,
                LastName = "Smith",
                FirstName = "George",
                MiddleName = "Mullins"
            };

            var degreePlanResponse2 = new DegreePlan()
            {
                Id = 678,
                NonTermPlannedCourses = new List<PlannedCourse>(),
                PersonId = "678",
                Terms = new List<DegreePlanTerm>(),
                Version = 1
            };

            // first response (GET)
            var serializedResponse1 = JsonConvert.SerializeObject(advisorResponse1);
            var response1 = new HttpResponseMessage(HttpStatusCode.OK);
            response1.Content = new StringContent(serializedResponse1, Encoding.UTF8, _contentType);

            // second response (POST)
            var serializedResponse2 = JsonConvert.SerializeObject(degreePlanResponse2);
            var response2 = new HttpResponseMessage(HttpStatusCode.OK);
            response2.Content = new StringContent(serializedResponse2, Encoding.UTF8, _contentType);

            // Enque two responses
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response1);
            mockHandler.Responses.Enqueue(response2);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, _logger);

            // Act
            var result1 = client.GetAdvisor(advisorId1);
            // Assert
            Assert.AreEqual(advisorResponse1.Id, result1.Id);
            // verify the media header is present
            var mediaHeader = mockHandler.Request.Headers.FirstOrDefault(x => x.Key == ColleagueApiClient.AcceptHeaderKey);
            Assert.IsNotNull(mediaHeader.Value.First());

            // Act
            var result2 = client.AddDegreePlan(_studentId);
            // Assert
            Assert.AreEqual(degreePlanResponse2.Id, result2.Id);
            Assert.AreEqual(1, mockHandler.Request.Headers.Count()); // media type
            Assert.AreEqual(1, mockHandler.Request.Content.Headers.Count()); // content type
        }

        [TestMethod]
        public void GetWithResponseServerErrorReturnsNull()
        {

            // Arrange
            string advisorId1 = "123";
            var advisorResponse1 = new Advisor()
            {
                Id = advisorId1,
                LastName = "Smith",
                FirstName = "George",
                MiddleName = "Mullins"
            };

            // first response
            var serializedResponse1 = JsonConvert.SerializeObject(advisorResponse1);
            var response1 = new HttpResponseMessage(HttpStatusCode.BadRequest);
            response1.Content = new StringContent(serializedResponse1, Encoding.UTF8, _contentType);

            // Enque two responses
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response1);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, _logger);

            // Act
            var result1 = client.GetAdvisor(advisorId1);

            // Assert
            Assert.AreEqual(null, result1);
        }

        #endregion

        [TestMethod]
        public async Task GetProgramEvaluation2AsyncTest()
        {
            // Arrange
            var personId = "000123";
            var program = "ENGL+BA";
            var programEval2 = new ProgramEvaluation2
            {
                ProgramCode = "ENGL+BA",
                CatalogCode = "2015Catalog",
                Credits = 6,
                InstitutionalCredits = 3,
                InProgressCredits = 3,
                PlannedCredits = 0,
                InstGpa = 0,
                CumGpa = 0,
                OverallCreditsModificationMessage = "Overall Credits Modification Message",
                InstitutionalCreditsModificationMessage = "Institutional Credits Modification Message",
                OverallGpaModificationMessage = "Overall Gpa Modification Message",
                InstitutionalGpaModificationMessage = "Institutional Gpa Modification Message",
                RequirementResults = new List<Ellucian.Colleague.Dtos.Student.Requirements.RequirementResult2>()
                {
                        new Ellucian.Colleague.Dtos.Student.Requirements.RequirementResult2()
                },
                ProgramRequirements = new Ellucian.Colleague.Dtos.Student.Requirements.ProgramRequirements(),
                OtherPlannedCredits = new List<Ellucian.Colleague.Dtos.Student.Requirements.PlannedCredit>()
                {
                        new Ellucian.Colleague.Dtos.Student.Requirements.PlannedCredit()
                },
                OtherAcademicCredits = new List<string>()
                {
                        "OtherCredit1",
                        "OtherCredit2"
                }
            };

            var serializedResponse = JsonConvert.SerializeObject(programEval2);
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response);
            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);
            var client = new ColleagueApiClient(testHttpClient, _logger);

            // Act
            var clientResponse = await client.GetProgramEvaluation2Async(personId, program);

            // Assert
            Assert.IsInstanceOfType(clientResponse, typeof(ProgramEvaluation2));
            Assert.AreEqual(program, clientResponse.ProgramCode);
            Assert.AreEqual(programEval2.CatalogCode, clientResponse.CatalogCode);

            Assert.AreEqual(programEval2.Credits, clientResponse.Credits);
            Assert.AreEqual(programEval2.InstitutionalCredits, clientResponse.InstitutionalCredits);
            Assert.AreEqual(programEval2.InProgressCredits, clientResponse.InProgressCredits);
            Assert.AreEqual(programEval2.PlannedCredits, clientResponse.PlannedCredits);
            Assert.AreEqual(programEval2.InstGpa, clientResponse.InstGpa);
            Assert.AreEqual(programEval2.CumGpa, clientResponse.CumGpa);

            Assert.AreEqual(programEval2.OverallCreditsModificationMessage, clientResponse.OverallCreditsModificationMessage);
            Assert.AreEqual(programEval2.InstitutionalCreditsModificationMessage, clientResponse.InstitutionalCreditsModificationMessage);
            Assert.AreEqual(programEval2.OverallGpaModificationMessage, clientResponse.OverallGpaModificationMessage);
            Assert.AreEqual(programEval2.InstitutionalGpaModificationMessage, clientResponse.InstitutionalGpaModificationMessage);
            Assert.AreEqual(programEval2.RequirementResults.Count, clientResponse.RequirementResults.Count);
        }

        [TestMethod]
        public async Task QueryProgramEvaluations2AsyncTest()
        {
            // Arrange
            var id = "0000001";
            var programCodes = new List<string>() { "ENGL+BA" };
            var programEval2List = new List<ProgramEvaluation2>()
            {
                new ProgramEvaluation2(){
                        ProgramCode = "ENGL+BA",
                        CatalogCode = "2015Catalog",
                        Credits = 6,
                        InstitutionalCredits = 3,
                        InProgressCredits = 3,
                        PlannedCredits = 0,
                        InstGpa = 0,
                        CumGpa = 0,
                        OverallCreditsModificationMessage = "Overall Credits Modification Message",
                        InstitutionalCreditsModificationMessage = "Institutional Credits Modification Message",
                        OverallGpaModificationMessage = "Overall Gpa Modification Message",
                        InstitutionalGpaModificationMessage = "Institutional Gpa Modification Message",
                        RequirementResults = new List<Ellucian.Colleague.Dtos.Student.Requirements.RequirementResult2>()
                        {
                        new Ellucian.Colleague.Dtos.Student.Requirements.RequirementResult2()
                        },
                        ProgramRequirements = new Ellucian.Colleague.Dtos.Student.Requirements.ProgramRequirements(),
                        OtherPlannedCredits = new List<Ellucian.Colleague.Dtos.Student.Requirements.PlannedCredit>()
                        {
                            new Ellucian.Colleague.Dtos.Student.Requirements.PlannedCredit()
                        },
                        OtherAcademicCredits = new List<string>()
                        {
                            "OtherCredit1",
                            "OtherCredit2"
                        }
                }
            };

            var serializedResponse = JsonConvert.SerializeObject(programEval2List);
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response);
            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);
            var client = new ColleagueApiClient(testHttpClient, _logger);

            // Act
            var clientResponse = await client.QueryProgramEvaluations2Async(id, programCodes);

            // Assert
            Assert.AreEqual(programEval2List.Count(), clientResponse.Count());
            Assert.AreEqual(programEval2List.ElementAt(0).CatalogCode, clientResponse.ElementAt(0).CatalogCode);
            Assert.AreEqual(programEval2List.ElementAt(0).Credits, clientResponse.ElementAt(0).Credits);
            Assert.AreEqual(programEval2List.ElementAt(0).InstitutionalCredits, clientResponse.ElementAt(0).InstitutionalCredits);
            Assert.AreEqual(programEval2List.ElementAt(0).InProgressCredits, clientResponse.ElementAt(0).InProgressCredits);
            Assert.AreEqual(programEval2List.ElementAt(0).PlannedCredits, clientResponse.ElementAt(0).PlannedCredits);
            Assert.AreEqual(programEval2List.ElementAt(0).InstGpa, clientResponse.ElementAt(0).InstGpa);
            Assert.AreEqual(programEval2List.ElementAt(0).CumGpa, clientResponse.ElementAt(0).CumGpa);
            Assert.AreEqual(programEval2List.ElementAt(0).OverallCreditsModificationMessage, clientResponse.ElementAt(0).OverallCreditsModificationMessage);
            Assert.AreEqual(programEval2List.ElementAt(0).InstitutionalCreditsModificationMessage, clientResponse.ElementAt(0).InstitutionalCreditsModificationMessage);
            Assert.AreEqual(programEval2List.ElementAt(0).OverallGpaModificationMessage, clientResponse.ElementAt(0).OverallGpaModificationMessage);
            Assert.AreEqual(programEval2List.ElementAt(0).InstitutionalGpaModificationMessage, clientResponse.ElementAt(0).InstitutionalGpaModificationMessage);
            Assert.AreEqual(programEval2List.ElementAt(0).RequirementResults.Count, clientResponse.ElementAt(0).RequirementResults.Count);
        }

        [TestMethod]
        public async Task GetProgramEvaluation3AsyncTest()
        {
            // Arrange
            var personId = "000123";
            var program = "ENGL+BA";
            var programEval3 = new ProgramEvaluation3
            {
                ProgramCode = "ENGL+BA",
                CatalogCode = "2015Catalog",
                Credits = 6,
                InstitutionalCredits = 3,
                InProgressCredits = 3,
                PlannedCredits = 0,
                InstGpa = 0,
                CumGpa = 0,
                OverallCreditsModificationMessage = "Overall Credits Modification Message",
                InstitutionalCreditsModificationMessage = "Institutional Credits Modification Message",
                OverallGpaModificationMessage = "Overall Gpa Modification Message",
                InstitutionalGpaModificationMessage = "Institutional Gpa Modification Message",
                RequirementResults = new List<Ellucian.Colleague.Dtos.Student.Requirements.RequirementResult3>()
                {
                        new Ellucian.Colleague.Dtos.Student.Requirements.RequirementResult3()
                },
                ProgramRequirements = new Ellucian.Colleague.Dtos.Student.Requirements.ProgramRequirements(),
                OtherPlannedCredits = new List<Ellucian.Colleague.Dtos.Student.Requirements.PlannedCredit>()
                {
                        new Ellucian.Colleague.Dtos.Student.Requirements.PlannedCredit()
                },
                OtherAcademicCredits = new List<string>()
                {
                        "OtherCredit1",
                        "OtherCredit2"
                }
            };

            var serializedResponse = JsonConvert.SerializeObject(programEval3);
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response);
            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);
            var client = new ColleagueApiClient(testHttpClient, _logger);

            // Act
            var clientResponse = await client.GetProgramEvaluation3Async(personId, program, null);

            // Assert
            Assert.IsInstanceOfType(clientResponse, typeof(ProgramEvaluation3));
            Assert.AreEqual(program, clientResponse.ProgramCode);
            Assert.AreEqual(programEval3.CatalogCode, clientResponse.CatalogCode);

            Assert.AreEqual(programEval3.Credits, clientResponse.Credits);
            Assert.AreEqual(programEval3.InstitutionalCredits, clientResponse.InstitutionalCredits);
            Assert.AreEqual(programEval3.InProgressCredits, clientResponse.InProgressCredits);
            Assert.AreEqual(programEval3.PlannedCredits, clientResponse.PlannedCredits);
            Assert.AreEqual(programEval3.InstGpa, clientResponse.InstGpa);
            Assert.AreEqual(programEval3.CumGpa, clientResponse.CumGpa);

            Assert.AreEqual(programEval3.OverallCreditsModificationMessage, clientResponse.OverallCreditsModificationMessage);
            Assert.AreEqual(programEval3.InstitutionalCreditsModificationMessage, clientResponse.InstitutionalCreditsModificationMessage);
            Assert.AreEqual(programEval3.OverallGpaModificationMessage, clientResponse.OverallGpaModificationMessage);
            Assert.AreEqual(programEval3.InstitutionalGpaModificationMessage, clientResponse.InstitutionalGpaModificationMessage);
            Assert.AreEqual(programEval3.RequirementResults.Count, clientResponse.RequirementResults.Count);
        }

        [TestMethod]
        public async Task QueryProgramEvaluations3AsyncTest()
        {
            // Arrange
            var id = "0000001";
            var programCodes = new List<string>() { "ENGL+BA" };
            var programEval3List = new List<ProgramEvaluation3>()
            {
                new ProgramEvaluation3(){
                        ProgramCode = "ENGL+BA",
                        CatalogCode = "2015Catalog",
                        Credits = 6,
                        InstitutionalCredits = 3,
                        InProgressCredits = 3,
                        PlannedCredits = 0,
                        InstGpa = 0,
                        CumGpa = 0,
                        OverallCreditsModificationMessage = "Overall Credits Modification Message",
                        InstitutionalCreditsModificationMessage = "Institutional Credits Modification Message",
                        OverallGpaModificationMessage = "Overall Gpa Modification Message",
                        InstitutionalGpaModificationMessage = "Institutional Gpa Modification Message",
                        RequirementResults = new List<Ellucian.Colleague.Dtos.Student.Requirements.RequirementResult3>()
                        {
                        new Ellucian.Colleague.Dtos.Student.Requirements.RequirementResult3()
                        },
                        ProgramRequirements = new Ellucian.Colleague.Dtos.Student.Requirements.ProgramRequirements(),
                        OtherPlannedCredits = new List<Ellucian.Colleague.Dtos.Student.Requirements.PlannedCredit>()
                        {
                            new Ellucian.Colleague.Dtos.Student.Requirements.PlannedCredit()
                        },
                        OtherAcademicCredits = new List<string>()
                        {
                            "OtherCredit1",
                            "OtherCredit2"
                        }
                }
            };

            var serializedResponse = JsonConvert.SerializeObject(programEval3List);
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response);
            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);
            var client = new ColleagueApiClient(testHttpClient, _logger);

            // Act
            var clientResponse = await client.QueryProgramEvaluations3Async(id, programCodes);

            // Assert
            Assert.AreEqual(programEval3List.Count(), clientResponse.Count());
            Assert.AreEqual(programEval3List.ElementAt(0).CatalogCode, clientResponse.ElementAt(0).CatalogCode);
            Assert.AreEqual(programEval3List.ElementAt(0).Credits, clientResponse.ElementAt(0).Credits);
            Assert.AreEqual(programEval3List.ElementAt(0).InstitutionalCredits, clientResponse.ElementAt(0).InstitutionalCredits);
            Assert.AreEqual(programEval3List.ElementAt(0).InProgressCredits, clientResponse.ElementAt(0).InProgressCredits);
            Assert.AreEqual(programEval3List.ElementAt(0).PlannedCredits, clientResponse.ElementAt(0).PlannedCredits);
            Assert.AreEqual(programEval3List.ElementAt(0).InstGpa, clientResponse.ElementAt(0).InstGpa);
            Assert.AreEqual(programEval3List.ElementAt(0).CumGpa, clientResponse.ElementAt(0).CumGpa);
            Assert.AreEqual(programEval3List.ElementAt(0).OverallCreditsModificationMessage, clientResponse.ElementAt(0).OverallCreditsModificationMessage);
            Assert.AreEqual(programEval3List.ElementAt(0).InstitutionalCreditsModificationMessage, clientResponse.ElementAt(0).InstitutionalCreditsModificationMessage);
            Assert.AreEqual(programEval3List.ElementAt(0).OverallGpaModificationMessage, clientResponse.ElementAt(0).OverallGpaModificationMessage);
            Assert.AreEqual(programEval3List.ElementAt(0).InstitutionalGpaModificationMessage, clientResponse.ElementAt(0).InstitutionalGpaModificationMessage);
            Assert.AreEqual(programEval3List.ElementAt(0).RequirementResults.Count, clientResponse.ElementAt(0).RequirementResults.Count);
        }

        [TestMethod]
        public void GetProgramEvaluationNoticesTest()
        {
            // Arrange
            var studentId = "0000001";
            var programCode = "ENGL+BA";

            var evaluationNotices = new List<EvaluationNotice>()
            {
                new EvaluationNotice()
                {
                    StudentId = studentId,
                    ProgramCode = programCode,
                    Text = new List<string>() { "line1", string.Empty, "line2", "line3"},
                    Type = EvaluationNoticeType.StudentProgram
                },
                new EvaluationNotice()
                {
                    StudentId = studentId,
                    ProgramCode = programCode,
                    Text = new List<string>() { "linex"},
                    Type = EvaluationNoticeType.Start
                }
            };

            var serializedResponse = JsonConvert.SerializeObject(evaluationNotices);

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, _logger);

            // Act
            var clientResponse = client.GetProgramEvaluationNotices(studentId, programCode);

            // Assert
            Assert.AreEqual(2, clientResponse.Count());

            Assert.AreEqual(evaluationNotices.ElementAt(0).StudentId, clientResponse.ElementAt(0).StudentId);
            Assert.AreEqual(evaluationNotices.ElementAt(0).ProgramCode, clientResponse.ElementAt(0).ProgramCode);
            Assert.AreEqual(evaluationNotices.ElementAt(0).Type.ToString(), clientResponse.ElementAt(0).Type.ToString());
            Assert.AreEqual(evaluationNotices.ElementAt(0).Text.Count(), clientResponse.ElementAt(0).Text.Count());
            for (int i = 0; i < evaluationNotices.ElementAt(0).Text.Count(); i++)
            {
                Assert.AreEqual(evaluationNotices.ElementAt(0).Text.ElementAt(i), clientResponse.ElementAt(0).Text.ElementAt(i));
            }
        }

        [TestMethod]
        public void QueryAdvisorsTest()
        {

            var advisors = new List<Advisor>()
            {
                new Advisor()
                {
                    LastName = "Blue",
                    FirstName = "Jim",
                    MiddleName = "Bob",
                    EmailAddresses = new List<string>() {"jjb@xmail.com", "personaleml@xmail.com"}
                },
                new Advisor()
                {
                    LastName = "Norton",
                    FirstName = "John",
                    MiddleName = "Brown",
                    EmailAddresses = new List<string>() {"jbnorton@xmail.com"}
                },
            };

            var serializedResponse = JsonConvert.SerializeObject(advisors);

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, _logger);

            // Act
            var clientResponse = client.QueryAdvisors(new List<string>() { "0000001", "0000002" });

            // Assert
            Assert.AreEqual(2, clientResponse.Count());

            Assert.AreEqual(advisors.ElementAt(0).LastName, clientResponse.ElementAt(0).LastName);
            Assert.AreEqual(advisors.ElementAt(0).FirstName, clientResponse.ElementAt(0).FirstName);
            Assert.AreEqual(advisors.ElementAt(0).MiddleName, clientResponse.ElementAt(0).MiddleName);
            Assert.AreEqual(advisors.ElementAt(0).EmailAddresses.Count(), clientResponse.ElementAt(0).EmailAddresses.Count());
            for (int i = 0; i < advisors.ElementAt(0).EmailAddresses.Count(); i++)
            {
                Assert.AreEqual(advisors.ElementAt(0).EmailAddresses.ElementAt(i), clientResponse.ElementAt(0).EmailAddresses.ElementAt(i));
            }
            Assert.AreEqual(advisors.ElementAt(1).LastName, clientResponse.ElementAt(1).LastName);
            Assert.AreEqual(advisors.ElementAt(1).FirstName, clientResponse.ElementAt(1).FirstName);
            Assert.AreEqual(advisors.ElementAt(1).MiddleName, clientResponse.ElementAt(1).MiddleName);
            Assert.AreEqual(advisors.ElementAt(1).EmailAddresses.ElementAt(0), clientResponse.ElementAt(1).EmailAddresses.ElementAt(0));

        }

        [TestClass]
        public class QueryAdvisors2Async_Tests : ColleagueApiClientPlanningTests
        {
            [TestMethod]
            public async Task QueryAdvisors2Async_Valid()
            {
                var advisors = new List<Advisor>()
                {
                    new Advisor()
                    {
                        LastName = "Blue",
                        FirstName = "Jim",
                        MiddleName = "Bob",
                        EmailAddresses = new List<string>() {"jjb@xmail.com", "personaleml@xmail.com"}
                    },
                    new Advisor()
                    {
                        LastName = "Norton",
                        FirstName = "John",
                        MiddleName = "Brown",
                        EmailAddresses = new List<string>() {"jbnorton@xmail.com"}
                    },
                };

                var serializedResponse = JsonConvert.SerializeObject(advisors);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.QueryAdvisors2Async(new List<string>() { "0000001", "0000002" });

                // Assert
                Assert.AreEqual(2, clientResponse.Count());

                Assert.AreEqual(advisors.ElementAt(0).LastName, clientResponse.ElementAt(0).LastName);
                Assert.AreEqual(advisors.ElementAt(0).FirstName, clientResponse.ElementAt(0).FirstName);
                Assert.AreEqual(advisors.ElementAt(0).MiddleName, clientResponse.ElementAt(0).MiddleName);
                Assert.AreEqual(advisors.ElementAt(0).EmailAddresses.Count(), clientResponse.ElementAt(0).EmailAddresses.Count());
                for (int i = 0; i < advisors.ElementAt(0).EmailAddresses.Count(); i++)
                {
                    Assert.AreEqual(advisors.ElementAt(0).EmailAddresses.ElementAt(i), clientResponse.ElementAt(0).EmailAddresses.ElementAt(i));
                }
                Assert.AreEqual(advisors.ElementAt(1).LastName, clientResponse.ElementAt(1).LastName);
                Assert.AreEqual(advisors.ElementAt(1).FirstName, clientResponse.ElementAt(1).FirstName);
                Assert.AreEqual(advisors.ElementAt(1).MiddleName, clientResponse.ElementAt(1).MiddleName);
                Assert.AreEqual(advisors.ElementAt(1).EmailAddresses.ElementAt(0), clientResponse.ElementAt(1).EmailAddresses.ElementAt(0));
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task QueryAdvisors2Async_null_AdvisorIds_throws_exception()
            {
                var advisors = new List<Advisor>()
                {
                    new Advisor()
                    {
                        LastName = "Blue",
                        FirstName = "Jim",
                        MiddleName = "Bob",
                        EmailAddresses = new List<string>() {"jjb@xmail.com", "personaleml@xmail.com"}
                    },
                    new Advisor()
                    {
                        LastName = "Norton",
                        FirstName = "John",
                        MiddleName = "Brown",
                        EmailAddresses = new List<string>() {"jbnorton@xmail.com"}
                    },
                };

                var serializedResponse = JsonConvert.SerializeObject(advisors);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.QueryAdvisors2Async(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task QueryAdvisors2Async_empty_AdvisorIds_throws_exception()
            {
                var advisors = new List<Advisor>()
                {
                    new Advisor()
                    {
                        LastName = "Blue",
                        FirstName = "Jim",
                        MiddleName = "Bob",
                        EmailAddresses = new List<string>() {"jjb@xmail.com", "personaleml@xmail.com"}
                    },
                    new Advisor()
                    {
                        LastName = "Norton",
                        FirstName = "John",
                        MiddleName = "Brown",
                        EmailAddresses = new List<string>() {"jbnorton@xmail.com"}
                    },
                };

                var serializedResponse = JsonConvert.SerializeObject(advisors);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.QueryAdvisors2Async(new List<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpRequestFailedException))]
            public async Task QueryAdvisors2Async_rethrows_application_exception()
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
                var clientResponse = await client.QueryAdvisors2Async(new List<string>() { "0000001", "0000002" });
                _loggerMock.Verify();
            }
        }

        [TestMethod]
        public async Task GetPlanningStudentAsync()
        {
            // Arrange
            var planningStudent = new PlanningStudent()
            {
                Id = "0000001",
                LastName = "brown",
                ProgramIds = new List<string>() { "ENGL.BA" },
                DegreePlanId = 3
            };

            var serializedResponse = JsonConvert.SerializeObject(planningStudent);

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, _logger);

            // Act
            var clientResponse = await client.GetPlanningStudentAsync("0000001");

            // Assert
            Assert.AreEqual(planningStudent.Id, clientResponse.Id);
            Assert.AreEqual(planningStudent.ProgramIds.ElementAt(0), clientResponse.ProgramIds.ElementAt(0));

        }

        [TestMethod]
        public void QueryPlanningStudentsTest()
        {
            var planningStudents = new List<PlanningStudent>() {
            new PlanningStudent()
        {
            Id = "0000001",
            LastName = "last name",
            ProgramIds = new List<string>() { "ENGL.BA" },
            DegreePlanId = 3
        },
        new PlanningStudent()
        {
            Id = "0000002",
            LastName = "last name",
            ProgramIds = new List<string>() { "ENGL.BA" },
            DegreePlanId = 3
        }
        };
            var studentIds = new List<string>() { "0000001", "0000002" };
            var serializedResponse = JsonConvert.SerializeObject(planningStudents);
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response);
            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);
            var client = new ColleagueApiClient(testHttpClient, _logger);
            var clientResponse = client.QueryPlanningStudents(studentIds);
            Assert.IsTrue(clientResponse is List<PlanningStudent>);
            Assert.AreEqual(2, clientResponse.Count());
        }

        [TestClass]
        public class ServiceClient_GetAdvisorPermissionsAsync_Tests : ColleagueApiClientPlanningTests
        {
            [TestMethod]
            [ExpectedException(typeof(AdvisingException))]
            public async Task ServiceClient_GetAdvisorPermissionsAsync_Unauthorized_throws_AdvisingException()
            {
                //Arrange
                List<string> permissions = new List<string>() { "VIEW.ANY.ADVISEE" };
                var serializedResponse = JsonConvert.SerializeObject(permissions.AsEnumerable());
                var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);
                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);
                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetAdvisorPermissionsAsync();
            }

            [TestMethod]
            public async Task ServiceClient_GetAdvisorPermissionsAsync_Success()
            {
                //Arrange
                List<string> permissions = new List<string>() { "VIEW.ANY.ADVISEE" };
                var serializedResponse = JsonConvert.SerializeObject(permissions.AsEnumerable());
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);
                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);
                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetAdvisorPermissionsAsync();

                // Assert that theitem is returned and each item property is found in the response
                Assert.IsNotNull(clientResponse);
                Assert.IsInstanceOfType(clientResponse, typeof(IEnumerable<string>));
                CollectionAssert.AreEqual(permissions.ToList(), clientResponse.ToList());
            }

            [TestMethod]
            [ExpectedException(typeof(AdvisingException))]
            public async Task ServiceClient_GetAdvisorPermissionsAsync_Not_OK_or_Unauthorized_throws_AdvisingException()
            {
                //Arrange
                List<string> permissions = new List<string>() { "VIEW.ANY.ADVISEE" };
                var serializedResponse = JsonConvert.SerializeObject(permissions.AsEnumerable());
                var response = new HttpResponseMessage(HttpStatusCode.Conflict);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);
                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);
                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetAdvisorPermissionsAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(AdvisingException))]
            public async Task ServiceClient_GetAdvisorPermissionsAsync_Exception()
            {
                //Arrange

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
                var clientResponse = await client.GetAdvisorPermissionsAsync();
            }

        }

        [TestClass]
        public class ServiceClient_GetAdvisingPermissions2Async_Tests : ColleagueApiClientPlanningTests
        {
            [TestMethod]
            [ExpectedException(typeof(HttpRequestFailedException))]
            public async Task ServiceClient_GetAdvisingPermissions2Async_error_rethrows_caught_exception()
            {
                //Arrange
                AdvisingPermissions permissions = new AdvisingPermissions()
                {
                    CanReviewAnyAdvisee = true,
                    CanReviewAssignedAdvisees = true,
                    CanUpdateAnyAdvisee = true,
                    CanUpdateAssignedAdvisees = true,
                    CanViewAnyAdvisee = true,
                    CanViewAssignedAdvisees = true,
                    HasFullAccessForAnyAdvisee = true,
                    HasFullAccessForAssignedAdvisees = true
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
                var clientResponse = await client.GetAdvisingPermissions2Async();
                _loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), "An error occurred while retrieving advising permissions."));
            }

            [TestMethod]
            public async Task ServiceClient_GetAdvisingPermissions2Async_Success()
            {
                //Arrange
                AdvisingPermissions permissions = new AdvisingPermissions()
                {
                    CanReviewAnyAdvisee = true,
                    CanReviewAssignedAdvisees = true,
                    CanUpdateAnyAdvisee = true,
                    CanUpdateAssignedAdvisees = true,
                    CanViewAnyAdvisee = true,
                    CanViewAssignedAdvisees = true,
                    HasFullAccessForAnyAdvisee = true,
                    HasFullAccessForAssignedAdvisees = true
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
                var clientResponse = await client.GetAdvisingPermissions2Async();

                // Assert that theitem is returned and each item property is found in the response
                Assert.IsNotNull(clientResponse);
                Assert.IsInstanceOfType(clientResponse, typeof(AdvisingPermissions));
            }
        }

        [TestMethod]
        public async Task GetFacultySections4Test()
        {
            // Arrange
            var facultyId = "0000001";

            var sections = new List<Section3>()
            {
                new Section3() { Id = "1", CourseName = "MATH-100", MinimumCredits = 3m, StartDate = DateTime.Now.AddDays(-10), TermId = "2017/SP", FacultyIds = new List<string>() { "0000001"}},
                new Section3() { Id = "2", CourseName = "MATH-100", MinimumCredits = 3m, StartDate = DateTime.Now.AddDays(-10), TermId = "2017/SP", FacultyIds = new List<string>() { "0000001"}}
            };

            var serializedResponse = JsonConvert.SerializeObject(sections);

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, _logger);

            // Act
            var clientResponse = await client.GetFacultySections4Async(facultyId, null, null, false);

            // Assert
            Assert.AreEqual(2, clientResponse.Count());
            CollectionAssert.AllItemsAreInstancesOfType(clientResponse.ToList(), typeof(Ellucian.Colleague.Dtos.Student.Section3));
            Assert.AreEqual(sections.ElementAt(0).Id, clientResponse.ElementAt(0).Id);
            Assert.AreEqual(sections.ElementAt(0).CourseName, clientResponse.ElementAt(0).CourseName);
            Assert.AreEqual(sections.ElementAt(0).TermId, clientResponse.ElementAt(0).TermId);
            Assert.AreEqual(sections.ElementAt(1).Id, clientResponse.ElementAt(1).Id);
            Assert.AreEqual(sections.ElementAt(1).CourseName, clientResponse.ElementAt(1).CourseName);
            Assert.AreEqual(sections.ElementAt(1).TermId, clientResponse.ElementAt(1).TermId);
        }

        [TestClass]
        public class GetPlanningConfigurationAsync : ColleagueApiClientPlanningTests
        {
            private PlanningConfiguration planningConfiguration;

            [TestInitialize]
            public void GetPlanningConfigurationAsync_Initialize()
            {
                //Arrange
                planningConfiguration = new PlanningConfiguration()
                {
                    DefaultCatalogPolicy = CatalogPolicy.StudentCatalogYear,
                    DefaultCurriculumTrack = "DEFAULT",
                    ShowAdvisementCompleteWorkflow = true
                };
            }

            [TestMethod]
            [ExpectedException(typeof(HttpRequestFailedException))]
            public async Task GetPlanningConfigurationAsync_BadRequest()
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
                var result = await client.GetPlanningConfigurationAsync();
                _loggerMock.Verify();
            }

            [TestMethod]
            public async Task GetPlanningConfigurationAsync_Returns_Serialized_PlanningConfiguration()
            {
                var serializedResponse = JsonConvert.SerializeObject(planningConfiguration);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetPlanningConfigurationAsync();

                // Assert
                Assert.IsNotNull(clientResponse);
                Assert.IsInstanceOfType(clientResponse, typeof(PlanningConfiguration));
                Assert.AreEqual(planningConfiguration.DefaultCatalogPolicy, clientResponse.DefaultCatalogPolicy);
                Assert.AreEqual(planningConfiguration.DefaultCurriculumTrack, clientResponse.DefaultCurriculumTrack);
                Assert.AreEqual(planningConfiguration.ShowAdvisementCompleteWorkflow, clientResponse.ShowAdvisementCompleteWorkflow);
            }
        }

        [TestClass]
        public class PostCompletedAdvisementAsync : ColleagueApiClientPlanningTests
        {
            private DateTime today = DateTime.Today;
            private DateTimeOffset now = DateTimeOffset.Now;
            private string studentId;
            private CompletedAdvisement advisement;
            private Advisee advisee;

            [TestInitialize]
            public void PostCompletedAdvisementAsync_Initialize()
            {
                //Arrange
                studentId = "0001234";
                advisement = new CompletedAdvisement()
                {
                    AdvisorId = "0001235",
                    CompletionDate = today,
                    CompletionTime = now
                };
                advisee = new Advisee()
                {
                    Id = studentId,
                    AdvisorIds = new List<string>() { advisement.AdvisorId },
                    CompletedAdvisements = new List<CompletedAdvisement>()
                    {
                        advisement
                    },
                    FirstName = "John",
                    LastName = "Smith"
                };
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PostCompletedAdvisementAsync_Null_StudentId()
            {
                var serializedResponse = JsonConvert.SerializeObject(advisee);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.PostCompletedAdvisementAsync(null, advisement);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpRequestFailedException))]
            public async Task PostCompletedAdvisementAsync_Request_Throws_Exception()
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
                var clientResponse = await client.PostCompletedAdvisementAsync(studentId, advisement);
                _loggerMock.Verify();

            }

            [TestMethod]
            public async Task PostCompletedAdvisementAsync_Returns_Serialized_Advisee()
            {
                var serializedResponse = JsonConvert.SerializeObject(advisee);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.PostCompletedAdvisementAsync(studentId, advisement);

                // Assert
                Assert.IsNotNull(clientResponse);
                Assert.IsInstanceOfType(clientResponse, typeof(Advisee));
                Assert.AreEqual(advisee.Id, clientResponse.Id);
                Assert.IsTrue(clientResponse.AdvisorIds.Count == 1 && clientResponse.AdvisorIds.Contains(advisee.AdvisorIds[0]));
                Assert.AreEqual(advisee.FirstName, clientResponse.FirstName);
                Assert.AreEqual(advisee.LastName, clientResponse.LastName);
            }
        }

    }
}
