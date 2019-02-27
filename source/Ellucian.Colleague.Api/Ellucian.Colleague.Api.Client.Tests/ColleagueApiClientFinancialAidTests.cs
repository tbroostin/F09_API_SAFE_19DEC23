//Copyright 2014-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Http.TestUtil;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.Base;

namespace Ellucian.Colleague.Api.Client.Tests
{
    [TestClass, System.Runtime.InteropServices.GuidAttribute("7A1C914B-9A03-4726-BA68-3C38143F2122")]
    public class ColleagueApiClientFinancialAidTests
    {
        public const string _contentType = "application/json";
        public const string _serviceUrl = "http://service.url";
        public const string _studentId = "0003914";

        private Mock<ILogger> loggerMock;
        private ColleagueApiClient client;
        private MockHandler mockHandler;

        [TestInitialize]
        public void Initialize()
        {
            loggerMock = new Mock<ILogger>();
            mockHandler = new MockHandler();
        }

        #region GetAwardLettersTests

        [TestMethod]
        public void GetAwardLettersTest()
        {
            var awardLetterResponse = new List<AwardLetter>()
            {
                new AwardLetter()
                {
                    StudentId = _studentId,
                    AwardYearCode = "2014",
                    OpeningParagraph = "This is the opening paragraph",
                    ClosingParagraph = "This is the closing paragraph",

                    BudgetAmount = 12000,
                    EstimatedFamilyContributionAmount = 10000,
                    NeedAmount = 2000
                }
            };

            var serializedResponse = JsonConvert.SerializeObject(awardLetterResponse);

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);            
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, loggerMock.Object);

            //Act
            var expectedResult = awardLetterResponse.First();
            var actualResult = client.GetAwardLetters(_studentId).First();

            //Assert
            Assert.AreEqual(expectedResult.StudentId, actualResult.StudentId);
            Assert.AreEqual(expectedResult.AwardYearCode, actualResult.AwardYearCode);
            Assert.AreEqual(expectedResult.OpeningParagraph, actualResult.OpeningParagraph);
            Assert.AreEqual(expectedResult.ClosingParagraph, actualResult.ClosingParagraph);

            Assert.AreEqual(expectedResult.BudgetAmount, actualResult.BudgetAmount);
            Assert.AreEqual(expectedResult.EstimatedFamilyContributionAmount, actualResult.EstimatedFamilyContributionAmount);
            Assert.AreEqual(expectedResult.NeedAmount, actualResult.NeedAmount);
        }

        [TestMethod]
        public void GetAwardLetters2Test()
        {
            var awardLetterResponse = new List<AwardLetter>()
            {
                new AwardLetter()
                {
                    StudentId = _studentId,
                    AwardYearCode = "2014",
                    OpeningParagraph = "This is the opening paragraph",
                    ClosingParagraph = "This is the closing paragraph",

                    BudgetAmount = 12000,
                    EstimatedFamilyContributionAmount = 10000,
                    NeedAmount = 2000
                }
            };

            var serializedResponse = JsonConvert.SerializeObject(awardLetterResponse);

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);            
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, loggerMock.Object);

            //Act
            var expectedResult = awardLetterResponse.First();
            var actualResult = client.GetAwardLetters2(_studentId).First();

            //Assert
            Assert.AreEqual(expectedResult.StudentId, actualResult.StudentId);
            Assert.AreEqual(expectedResult.AwardYearCode, actualResult.AwardYearCode);
            Assert.AreEqual(expectedResult.OpeningParagraph, actualResult.OpeningParagraph);
            Assert.AreEqual(expectedResult.ClosingParagraph, actualResult.ClosingParagraph);

            Assert.AreEqual(expectedResult.BudgetAmount, actualResult.BudgetAmount);
            Assert.AreEqual(expectedResult.EstimatedFamilyContributionAmount, actualResult.EstimatedFamilyContributionAmount);
            Assert.AreEqual(expectedResult.NeedAmount, actualResult.NeedAmount);
        }        

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetAwardLetters_NullStudentIdTest()
        {
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
            response.Content = new StringContent(string.Empty, Encoding.UTF8, _contentType);            
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, loggerMock.Object);

            var result = client.GetAwardLetters(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetAwardLetters2_NullStudentIdTest()
        {
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
            response.Content = new StringContent(string.Empty, Encoding.UTF8, _contentType);            
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, loggerMock.Object);

            var result = client.GetAwardLetters2(null);
        }

        [TestMethod]
        public void GetAwardLetters_EmptyResponseTest()
        {
            var serializedResponse = JsonConvert.SerializeObject(new List<AwardLetter>());

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);            
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, loggerMock.Object);

            //Act
            var expectedResult = new List<AwardLetter>();
            var actualResult = client.GetAwardLetters(_studentId);

            //Assert
            Assert.AreEqual(expectedResult.Count(), actualResult.Count());
        }

        [TestMethod]
        public void GetAwardLetters2_EmptyResponseTest()
        {
            var serializedResponse = JsonConvert.SerializeObject(new List<AwardLetter>());

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);            
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, loggerMock.Object);

            //Act
            var expectedResult = new List<AwardLetter>();
            var actualResult = client.GetAwardLetters2(_studentId);

            //Assert
            Assert.AreEqual(expectedResult.Count(), actualResult.Count());


        }

        [TestMethod]
        public async Task GetAwardLetters4AsyncTest()
        {
            var awardLetterResponse = new List<AwardLetter3>()
            {
                new AwardLetter3()
                {
                    StudentId = _studentId,
                    AwardLetterYear = "2014",
                    OpeningParagraph = "This is the opening paragraph",
                    ClosingParagraph = "This is the closing paragraph",

                    BudgetAmount = 12000,
                    EstimatedFamilyContributionAmount = 10000,
                    NeedAmount = 2000
                }
            };

            var serializedResponse = JsonConvert.SerializeObject(awardLetterResponse);

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, loggerMock.Object);

            //Act
            var expectedResult = awardLetterResponse.First();
            var actualResult = (await client.GetAwardLetters4Async(_studentId)).First();

            //Assert
            Assert.AreEqual(expectedResult.StudentId, actualResult.StudentId);
            Assert.AreEqual(expectedResult.AwardLetterYear, actualResult.AwardLetterYear);
            Assert.AreEqual(expectedResult.OpeningParagraph, actualResult.OpeningParagraph);
            Assert.AreEqual(expectedResult.ClosingParagraph, actualResult.ClosingParagraph);

            Assert.AreEqual(expectedResult.BudgetAmount, actualResult.BudgetAmount);
            Assert.AreEqual(expectedResult.EstimatedFamilyContributionAmount, actualResult.EstimatedFamilyContributionAmount);
            Assert.AreEqual(expectedResult.NeedAmount, actualResult.NeedAmount);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetAwardLetters4Async_NullStudentIdTest()
        {
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
            response.Content = new StringContent(string.Empty, Encoding.UTF8, _contentType);
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, loggerMock.Object);

            var result = await client.GetAwardLetters4Async(null);
        }

        [TestMethod]
        public async Task GetAwardLetters4Async_EmptyResponseTest()
        {
            var serializedResponse = JsonConvert.SerializeObject(new List<AwardLetter>());

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, loggerMock.Object);

            //Act
            var expectedResult = new List<AwardLetter>();
            var actualResult = await client.GetAwardLetters4Async(_studentId);

            //Assert
            Assert.AreEqual(expectedResult.Count(), actualResult.Count());


        }

        #endregion

        #region GetAwardLetter Tests
        [TestMethod]
        public async Task GetAwardLetter4AsyncTest()
        {
            var awardYear = "2014";
            var awardLetterResponse = new AwardLetter3()
            {
                StudentId = _studentId,
                AwardLetterYear = "2014",
                OpeningParagraph = "This is the opening paragraph",
                ClosingParagraph = "This is the closing paragraph",

                BudgetAmount = 12000,
                EstimatedFamilyContributionAmount = 10000,
                NeedAmount = 2000

            };

            var serializedResponse = JsonConvert.SerializeObject(awardLetterResponse);

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, loggerMock.Object);

            //Act
            var expectedResult = awardLetterResponse;
            var actualResult = (await client.GetAwardLetter4Async(_studentId, awardYear));

            //Assert
            Assert.AreEqual(expectedResult.StudentId, actualResult.StudentId);
            Assert.AreEqual(expectedResult.AwardLetterYear, actualResult.AwardLetterYear);
            Assert.AreEqual(expectedResult.OpeningParagraph, actualResult.OpeningParagraph);
            Assert.AreEqual(expectedResult.ClosingParagraph, actualResult.ClosingParagraph);

            Assert.AreEqual(expectedResult.BudgetAmount, actualResult.BudgetAmount);
            Assert.AreEqual(expectedResult.EstimatedFamilyContributionAmount, actualResult.EstimatedFamilyContributionAmount);
            Assert.AreEqual(expectedResult.NeedAmount, actualResult.NeedAmount);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetAwardLetter4Async_NullStudentIdTest()
        {
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
            response.Content = new StringContent(string.Empty, Encoding.UTF8, _contentType);
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, loggerMock.Object);

            var result = await client.GetAwardLetter4Async(null, "2014");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetAwardLetter4Async_NullAwardYearTest()
        {
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
            response.Content = new StringContent(string.Empty, Encoding.UTF8, _contentType);
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, loggerMock.Object);

            var result = await client.GetAwardLetter4Async(_studentId, null);
        }

        [TestMethod]
        public async Task GetAwardLetter4Async_EmptyResponseTest()
        {
            var serializedResponse = JsonConvert.SerializeObject(new List<AwardLetter>());

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, loggerMock.Object);

            //Act
            var expectedResult = new List<AwardLetter>();
            var actualResult = await client.GetAwardLetters4Async(_studentId);

            //Assert
            Assert.AreEqual(expectedResult.Count(), actualResult.Count());


        }
        #endregion

        #region UpdateAwardLetter Tests

        [TestMethod]
        public async Task UpdateAwardLetter3AsyncTest()
        {

            var awardYear = "2014";
            var awardLetterDto = new AwardLetter3()
            {
                StudentId = _studentId,
                AwardLetterYear = "2014",
                OpeningParagraph = "This is the opening paragraph",
                ClosingParagraph = "This is the closing paragraph",

                BudgetAmount = 12000,
                EstimatedFamilyContributionAmount = 10000,
                NeedAmount = 2000,
                AcceptedDate = new DateTime()
            };

            var serializedResponse = JsonConvert.SerializeObject(awardLetterDto);

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, loggerMock.Object);

            //Act
            var expectedResult = awardLetterDto;
            var actualResult = (await client.UpdateAwardLetter3Async(_studentId, awardYear, awardLetterDto));

            //Assert
            Assert.AreEqual(expectedResult.StudentId, actualResult.StudentId);
            Assert.AreEqual(expectedResult.AwardLetterYear, actualResult.AwardLetterYear);
            Assert.AreEqual(expectedResult.OpeningParagraph, actualResult.OpeningParagraph);
            Assert.AreEqual(expectedResult.ClosingParagraph, actualResult.ClosingParagraph);

            Assert.AreEqual(expectedResult.BudgetAmount, actualResult.BudgetAmount);
            Assert.AreEqual(expectedResult.EstimatedFamilyContributionAmount, actualResult.EstimatedFamilyContributionAmount);
            Assert.AreEqual(expectedResult.NeedAmount, actualResult.NeedAmount);

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UpdateAwardLetter3Async_NullStudentIdTest()
        {
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
            response.Content = new StringContent(string.Empty, Encoding.UTF8, _contentType);
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, loggerMock.Object);

            var result = await client.UpdateAwardLetter3Async(null, "2014", new AwardLetter3() { StudentId = _studentId });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UpdateAwardLetter3Async_NullAwardYearTest()
        {
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
            response.Content = new StringContent(string.Empty, Encoding.UTF8, _contentType);
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, loggerMock.Object);

            var result = await client.UpdateAwardLetter3Async(_studentId, null, new AwardLetter3() { StudentId = _studentId });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UpdateAwardLetter3Async_NullAwardLetterDtoTest()
        {
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
            response.Content = new StringContent(string.Empty, Encoding.UTF8, _contentType);
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, loggerMock.Object);

            var result = await client.UpdateAwardLetter3Async(_studentId, "2014", null);
        }

        #endregion

        #region GetAcademicProgressAppealCodesTests

        private List<AcademicProgressAppealCode> expectedAppealCodes = new List<AcademicProgressAppealCode>()
        {
            new AcademicProgressAppealCode(){
                Code = "SR",
                Description = "Student Requested"
            },
            new AcademicProgressAppealCode(){
                Code = "R",
                Description = "Rejected"
            }
        };

        [TestMethod]
        public async Task GetAcademicProgressAppealCodesAsync_ReturnsExpectedResultTest()
        {
            var serializedResponse = JsonConvert.SerializeObject(expectedAppealCodes);
            setResponse(serializedResponse, HttpStatusCode.OK);

            var actualAppealCodesResponse = (await client.GetAcademicProgressAppealCodesAsync()).ToList();
            foreach (var expectedRecord in expectedAppealCodes)
            {
                var actualRecord = actualAppealCodesResponse.First(a => a.Code == expectedRecord.Code);
                Assert.AreEqual(expectedRecord.Description, actualRecord.Description);
            }
        }

        [TestMethod]
        public async Task GetAcademicProgressAppealCodesAsync_RethrowsGenericExceptionTest()
        {
            var serializedResponse = JsonConvert.SerializeObject(expectedAppealCodes);
            setResponse(serializedResponse, HttpStatusCode.BadRequest);

            bool exceptionThrown = false;
            try
            {
                await client.GetAcademicProgressAppealCodesAsync();
            }
            catch { exceptionThrown = true; }
            Assert.IsTrue(exceptionThrown);
        }

        #endregion

        #region GetAcademicProgressStatusesAsyncTests
       
        private List<AcademicProgressStatus> expectedAcademicProgressStatuses = new List<AcademicProgressStatus>()
        {
            new AcademicProgressStatus(){
                Category = AcademicProgressStatusCategory.Satisfactory,
                Code = "sat",
                Description = "Satisfactory status"
            },
            new AcademicProgressStatus(){
                Category = AcademicProgressStatusCategory.Warning,
                Code = "warn",
                Description = "Warning status"
            }
        };

        [TestMethod]
        public async Task GetAcademicProgressStatusesAsync_ReturnsExpectedResultTest()
        {
            var serializedResponse = JsonConvert.SerializeObject(expectedAcademicProgressStatuses);
            setResponse(serializedResponse, HttpStatusCode.OK);

            var actualStatusesResponse = (await client.GetAcademicProgressStatusesAsync()).ToList();
            foreach (var expectedRecord in expectedAcademicProgressStatuses)
            {
                var actualRecord = actualStatusesResponse.First(a => a.Code == expectedRecord.Code);
                Assert.AreEqual(expectedRecord.Category, actualRecord.Category);
                Assert.AreEqual(expectedRecord.Description, actualRecord.Description);
            }
        }

        [TestMethod]
        public async Task GetAcademicProgressStatusesAsync_RethrowsGenericExceptionTest()
        {
            var serializedResponse = JsonConvert.SerializeObject(expectedAcademicProgressStatuses);
            setResponse(serializedResponse, HttpStatusCode.BadRequest);

            bool exceptionThrown = false;
            try
            {
                await client.GetAcademicProgressStatusesAsync();
            }
            catch { exceptionThrown = true; }
            Assert.IsTrue(exceptionThrown);
        }
        #endregion

        #region GetStudentAcademicProgressEvaluationsTests

        public FunctionEqualityComparer<AcademicProgressEvaluation> academicProgressEvaluationDtoComparer
            = new FunctionEqualityComparer<AcademicProgressEvaluation>((e1, e2) => e1.Id == e2.Id, e => e.Id.GetHashCode());

        [TestMethod]
        public void GetStudentAcademicProgressEvaluationsTest()
        {
            var evaluations = new List<AcademicProgressEvaluation>()
            {
                new AcademicProgressEvaluation() 
                {
                    Id = "1234",
                    StudentId = _studentId,
                    StatusCode = "S",
                    EvaluationDateTime = DateTime.Now
                },
                new AcademicProgressEvaluation()
                {
                    Id = "4321",
                    StudentId = _studentId,
                    StatusCode = "S",
                    EvaluationDateTime = DateTime.Now
                }
            };

            var serializedResponse = JsonConvert.SerializeObject(evaluations);

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);           
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, loggerMock.Object);

            var actualResult = client.GetStudentAcademicProgressEvaluations(_studentId);

            CollectionAssert.AreEqual(evaluations, actualResult.ToList(), academicProgressEvaluationDtoComparer);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StudentIdRequiredTest()
        {
            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);
            var client = new ColleagueApiClient(testHttpClient, loggerMock.Object);

            var actualResult = client.GetStudentAcademicProgressEvaluations(null);
        }

        #endregion  

        #region GetStudentAcademicProgressEvaluationAsyncTests
        
        public List<AcademicProgressEvaluation> expectedEvaluations = new List<AcademicProgressEvaluation>()
            {
                new AcademicProgressEvaluation() 
                {
                    Id = "1234",
                    StudentId = _studentId,
                    StatusCode = "S",
                    EvaluationDateTime = DateTime.Now
                },
                new AcademicProgressEvaluation()
                {
                    Id = "4321",
                    StudentId = _studentId,
                    StatusCode = "S",
                    EvaluationDateTime = DateTime.Now
                }
            };
        [TestMethod]
        public async Task GetStudentAcademicProgressEvaluationsAsyncTest()
        {
            var serializedResponse = JsonConvert.SerializeObject(expectedEvaluations);
            setResponse(serializedResponse, HttpStatusCode.OK);

            var actualResult = await client.GetStudentAcademicProgressEvaluationsAsync(_studentId);

            CollectionAssert.AreEqual(expectedEvaluations, actualResult.ToList(), academicProgressEvaluationDtoComparer);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetStudentAcademicProgressEvaluationsAsync_StudentIdRequiredTest()
        {
            var serializedResponse = JsonConvert.SerializeObject(expectedEvaluations);
            setResponse(serializedResponse, HttpStatusCode.OK);
            var actualResult = await client.GetStudentAcademicProgressEvaluationsAsync(null);
        }

        #endregion

        #region GetStudentAcademicProgressEvaluationAsync2Tests

        public FunctionEqualityComparer<AcademicProgressEvaluation2> academicProgressEvaluation2DtoComparer
            = new FunctionEqualityComparer<AcademicProgressEvaluation2>((e1, e2) => e1.Id == e2.Id, e => e.Id.GetHashCode());

        public List<AcademicProgressEvaluation2> expectedEvaluations2 = new List<AcademicProgressEvaluation2>()
            {
                new AcademicProgressEvaluation2() 
                {
                    Id = "1234",
                    StudentId = _studentId,
                    StatusCode = "S",
                    EvaluationDateTime = DateTime.Now
                },
                new AcademicProgressEvaluation2()
                {
                    Id = "4321",
                    StudentId = _studentId,
                    StatusCode = "S",
                    EvaluationDateTime = DateTime.Now
                }
            };

        [TestMethod]
        public async Task GetStudentAcademicProgressEvaluationsAsync2Test()
        {
            var serializedResponse = JsonConvert.SerializeObject(expectedEvaluations2);
            setResponse(serializedResponse, HttpStatusCode.OK);

            var actualResult = await client.GetStudentAcademicProgressEvaluations2Async(_studentId);

            CollectionAssert.AreEqual(expectedEvaluations2, actualResult.ToList(), academicProgressEvaluation2DtoComparer);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetStudentAcademicProgressEvaluationsAsync2_StudentIdRequiredTest()
        {
            var serializedResponse = JsonConvert.SerializeObject(expectedEvaluations2);
            setResponse(serializedResponse, HttpStatusCode.OK);
            var actualResult = await client.GetStudentAcademicProgressEvaluations2Async(null);
        }

        #endregion

        #region StudentFinancialAidChecklistTests

        public FunctionEqualityComparer<StudentFinancialAidChecklist> studentChecklistDtoComparer = new FunctionEqualityComparer<StudentFinancialAidChecklist>(
                (c1, c2) => c1.AwardYear == c2.AwardYear && c1.StudentId == c2.StudentId && c1.ChecklistItems.Count == c2.ChecklistItems.Count,
                (c) => c.AwardYear.GetHashCode() ^ c.StudentId.GetHashCode() ^ c.ChecklistItems.Count);

        public FunctionEqualityComparer<StudentChecklistItem> studentChecklistItemDtoComparer = new FunctionEqualityComparer<StudentChecklistItem>(
                (i1, i2) => i1.Code == i2.Code && i1.ControlStatus == i2.ControlStatus,
                (i) => i.Code.GetHashCode() ^ i.ControlStatus.GetHashCode());


        [TestMethod]
        public void StudentFinancialAidChecklist_GetAll_GetAllStudentChecklistsTests()
        {
            var responseObj = new List<StudentFinancialAidChecklist>()
            {
                new StudentFinancialAidChecklist()
                {
                    AwardYear = "2014",
                    StudentId = "0003914",
                    ChecklistItems = new List<StudentChecklistItem>() 
                    {
                        new StudentChecklistItem() {Code = "F", ControlStatus = ChecklistItemControlStatus.CompletionRequired},
                        new StudentChecklistItem() {Code = "Q", ControlStatus = ChecklistItemControlStatus.CompletionRequiredLater}
                    }
                },
                new StudentFinancialAidChecklist()
                {
                    AwardYear = "2015",
                    StudentId = "0003914",
                    ChecklistItems = new List<StudentChecklistItem>()
                    {
                        new StudentChecklistItem() {Code = "R", ControlStatus= ChecklistItemControlStatus.CompletionRequiredLater},
                        new StudentChecklistItem() { Code = "Z", ControlStatus = ChecklistItemControlStatus.RemovedFromChecklist}
                    }
                }
            };

            var serializedResponse = JsonConvert.SerializeObject(responseObj);

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);            
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, loggerMock.Object);
            var actualResult = client.GetAllStudentChecklists(_studentId);

            CollectionAssert.AreEqual(responseObj, actualResult.ToList(), studentChecklistDtoComparer);
            CollectionAssert.AreEqual(responseObj.SelectMany(r => r.ChecklistItems).ToList(), actualResult.SelectMany(r => r.ChecklistItems).ToList(), studentChecklistItemDtoComparer);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StudentFinancialAidChecklist_GetAll_StudentIdRequiredTest()
        {
            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);
            var client = new ColleagueApiClient(testHttpClient, loggerMock.Object);

            var actualResult = client.GetAllStudentChecklists(null);
        }


        [TestMethod]
        public void StudentFinancialAidChecklist_GetSingle_GetSingleChecklistTest()
        {
            var year = "2014";
            var responseObj = new StudentFinancialAidChecklist()
            {
                AwardYear = year,
                StudentId = "0003914",
                ChecklistItems = new List<StudentChecklistItem>() 
                {
                    new StudentChecklistItem() {Code = "F", ControlStatus = ChecklistItemControlStatus.CompletionRequired},
                    new StudentChecklistItem() {Code = "Q", ControlStatus = ChecklistItemControlStatus.CompletionRequiredLater}
                }				
            };

            var serializedResponse = JsonConvert.SerializeObject(responseObj);

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);            
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, loggerMock.Object);
            var actualResult = client.GetStudentFinancialAidChecklist(_studentId, year);

            Assert.IsTrue(studentChecklistDtoComparer.Equals(responseObj, actualResult));
            CollectionAssert.AreEqual(responseObj.ChecklistItems, actualResult.ChecklistItems, studentChecklistItemDtoComparer);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StudentFinancialAidChecklist_GetSingle_StudentIdRequiredTest()
        {
            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);
            var client = new ColleagueApiClient(testHttpClient, loggerMock.Object);

            var actualResult = client.GetStudentFinancialAidChecklist(null, "2014");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StudentFinancialAidChecklist_GetSingle_AwardYearRequiredTest()
        {
            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);
            var client = new ColleagueApiClient(testHttpClient, loggerMock.Object);

            var actualResult = client.GetStudentFinancialAidChecklist(_studentId, null);
        }

        [TestMethod]
        public void StudentFinancialAidChecklist_Create_CreateStudentChecklistTest()
        {
            var year = "2014";
            var responseObj = new StudentFinancialAidChecklist()
            {
                AwardYear = year,
                StudentId = "0003914",
                ChecklistItems = new List<StudentChecklistItem>() 
                {
                    new StudentChecklistItem() {Code = "F", ControlStatus = ChecklistItemControlStatus.CompletionRequired},
                    new StudentChecklistItem() {Code = "Q", ControlStatus = ChecklistItemControlStatus.CompletionRequiredLater}
                }
            };

            var serializedResponse = JsonConvert.SerializeObject(responseObj);

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);            
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, loggerMock.Object);
            var actualResult = client.CreateStudentFinancialAidChecklist(_studentId, year);

            Assert.IsTrue(studentChecklistDtoComparer.Equals(responseObj, actualResult));
            CollectionAssert.AreEqual(responseObj.ChecklistItems, actualResult.ChecklistItems, studentChecklistItemDtoComparer);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StudentFinancialAidChecklist_Create_StudentIdRequiredTest()
        {
            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);
            var client = new ColleagueApiClient(testHttpClient, loggerMock.Object);

            var actualResult = client.CreateStudentFinancialAidChecklist(null, "2014");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StudentFinancialAidChecklist_Create_AwardYearRequiredTest()
        {
            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);
            var client = new ColleagueApiClient(testHttpClient, loggerMock.Object);

            var actualResult = client.CreateStudentFinancialAidChecklist(_studentId, null);
        }

        #endregion

        #region GetFinancialAidCounselorsByIdAsync

        private List<string> staffIds;        
        private List<FinancialAidCounselor> expectedResult;

        [TestMethod]
        public async Task GetFinancialAidCounselorsTest()
        {            
            GetFinancialAidCounselorsAsyncTestsSetUp(out staffIds, out client, out expectedResult);
            var actualResult = await client.GetFinancialAidCounselorsByIdAsync(new FinancialAidCounselorQueryCriteria() {FinancialAidCounselorIds = staffIds});

            foreach (var counselor in expectedResult)
            {
                var actualCounselor =actualResult.First(ec => ec.Id == counselor.Id);
                Assert.IsNotNull(actualCounselor);
                Assert.AreEqual(counselor.EmailAddress, actualCounselor.EmailAddress);
                Assert.AreEqual(counselor.Name, actualCounselor.Name);
            }
            
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task NoIdsPassedGetFinancialAidCounselors_ExceptionThrownTest()
        {
            GetFinancialAidCounselorsAsyncTestsSetUp(out staffIds, out client, out expectedResult);
            FinancialAidCounselorQueryCriteria criteria = null;
            await client.GetFinancialAidCounselorsByIdAsync(criteria);
        }

        [TestMethod]
        public async Task EmptyResponseReturnsEmptyListTest()
        {
            staffIds = new List<string>() { "0000001", "0000002", "0000003" };
            var serializedResponse = JsonConvert.SerializeObject(new List<FinancialAidCounselor>());

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);            
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, loggerMock.Object);
            var actualResult = await client.GetFinancialAidCounselorsByIdAsync(new FinancialAidCounselorQueryCriteria() { FinancialAidCounselorIds = staffIds });
            Assert.IsTrue(actualResult.Count() == 0);
        }

        private void GetFinancialAidCounselorsAsyncTestsSetUp(out List<string> staffIds, out ColleagueApiClient client, out List<FinancialAidCounselor> expectedResult)
        {
            staffIds = new List<string>() { "0000001", "0000002", "0000003" };
            var counselorsResponse = new List<FinancialAidCounselor>(){
                new FinancialAidCounselor(){
                    Id = "0000001",
                    Name = "Sam Sam",
                    EmailAddress = "sam.sam@ellucian.edu"
                },
                new FinancialAidCounselor(){
                    Id = "0000002",
                    Name = "Jane Thero",
                    EmailAddress = "jane.thero@ellucian.edu"
                },
                new FinancialAidCounselor(){
                    Id = "0000003",
                    Name = "Ellucian Adams",
                    EmailAddress = "ellucian.adams@ellucian.edu"
                }
            };

            var serializedResponse = JsonConvert.SerializeObject(counselorsResponse);

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);            
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            client = new ColleagueApiClient(testHttpClient, loggerMock.Object);

            expectedResult = counselorsResponse;
        }
        
        #endregion 

        #region GetAwardLetterConfigurationsAsync        

        private List<AwardLetterConfiguration> expectedConfigurationsResponse = new List<AwardLetterConfiguration>(){
            new AwardLetterConfiguration(){
                Id = "1",
                IsContactBlockActive = true,
                IsHousingBlockActive = true,
                IsNeedBlockActive = true,
                AwardTableTitle = "awards",
                AwardTotalTitle = "total",
                AwardCategoriesGroups = new List<AwardLetterGroup>(),
                AwardPeriodsGroups = new List<AwardLetterGroup>()
            },
            new AwardLetterConfiguration(){
                Id = "2",
                IsContactBlockActive = false,
                IsHousingBlockActive = false,
                IsNeedBlockActive = false,
                AwardTableTitle = "Awards table",
                AwardTotalTitle = "Total column",
                AwardCategoriesGroups = new List<AwardLetterGroup>(),
                AwardPeriodsGroups = new List<AwardLetterGroup>()
            }
        };

        /// <summary>
        /// Tests if the actual award letter configuration dtos match the expected ones
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetAwardLetterConfigurationsAsyncTest()
        {
            client = awardLetterConfigurationsResponseSetUp(expectedConfigurationsResponse, HttpStatusCode.OK);
            var actualConfigurations = await client.GetAwardLetterConfigurationsAsync();

            foreach (var expected in expectedConfigurationsResponse)
            {
                var actual = actualConfigurations.FirstOrDefault(c => c.Id == expected.Id);
                Assert.IsNotNull(actual);
                Assert.AreEqual(expected.IsContactBlockActive, actual.IsContactBlockActive);
                Assert.AreEqual(expected.IsHousingBlockActive, actual.IsHousingBlockActive);
                Assert.AreEqual(expected.IsNeedBlockActive, actual.IsNeedBlockActive);
                Assert.AreEqual(expected.ParagraphSpacing, actual.ParagraphSpacing);
                Assert.AreEqual(expected.AwardTableTitle, actual.AwardTableTitle);
                Assert.AreEqual(expected.AwardTotalTitle, actual.AwardTotalTitle);
            }
        } 

        /// <summary>
        /// Tests if bad request exception gets rethrown
        /// </summary>
        /// <returns></returns>
        [TestMethod]        
        public async Task GetAwardLetterConfigurationsAsync_RethrowsBadRequestExceptionTest()
        {
            bool exceptionCaught = false;
            Exception e;
            client = awardLetterConfigurationsResponseSetUp(expectedConfigurationsResponse, HttpStatusCode.BadRequest);
            try
            {
                await client.GetAwardLetterConfigurationsAsync();
            }
            catch (Exception ex) { exceptionCaught = true; e = ex; }
            Assert.IsTrue(exceptionCaught);            
        }

        /// <summary>
        /// Tests if not found exception gets caught and rethrown
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetAwardLetterConfigurationsAsync_RethrowsNotFoundExceptionTest()
        {
            bool exceptionCaught = false;
            Exception e;
            client = awardLetterConfigurationsResponseSetUp(expectedConfigurationsResponse, HttpStatusCode.NotFound);
            try
            {
                await client.GetAwardLetterConfigurationsAsync();
            }
            catch (Exception ex) { exceptionCaught = true; e = ex; }
            Assert.IsTrue(exceptionCaught);
        }        

        /// <summary>
        /// Tests if an empty list is returned if the response does not contain any configurations
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task NoAwardLetterConfigurationsInResponse_ReturnsEmptyDTOListTest()
        {
            client = awardLetterConfigurationsResponseSetUp(new List<AwardLetterConfiguration>(), HttpStatusCode.OK);
            var actualConfigurations = await client.GetAwardLetterConfigurationsAsync();
            Assert.IsFalse(actualConfigurations.Any());
        }

        private ColleagueApiClient awardLetterConfigurationsResponseSetUp(List<AwardLetterConfiguration> awardLetterConfigurationsResponse, HttpStatusCode responseStatusCode)
        {
            ColleagueApiClient client;
            var serializedResponse = JsonConvert.SerializeObject(awardLetterConfigurationsResponse);

            var response = new HttpResponseMessage(responseStatusCode);
            response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);            
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            client = new ColleagueApiClient(testHttpClient, loggerMock.Object);
            return client;
        }

        #endregion

        #region GetStudentDocumentsAsync

        private List<StudentDocument> expectedDocumentsResponse = new List<StudentDocument>()
            {
                new StudentDocument()
                {
                    StudentId = _studentId,
                    Code ="Doc1",
                    StatusDescription = "Complete",
                    Status = DocumentStatus.Received
                },

                new StudentDocument()
                {
                    StudentId = _studentId,
                    Code ="Doc2",
                    StatusDescription = "Waived",
                    Status = DocumentStatus.Waived
                }
            };

        [TestMethod]
        public async Task GetStudentDocumentsAsync_ReturnsExpectedResultTest()
        {
            var serializedResponse = JsonConvert.SerializeObject(expectedDocumentsResponse);
            setResponse(serializedResponse, HttpStatusCode.OK);

            var expectedResult = expectedDocumentsResponse.First();
            var actualResult = (await client.GetStudentDocumentsAsync(_studentId)).First();

            Assert.AreEqual(expectedResult.Code, actualResult.Code);
            Assert.AreEqual(expectedResult.StudentId, actualResult.StudentId);
            Assert.AreEqual(expectedResult.Status, actualResult.Status);
            Assert.AreEqual(expectedResult.StatusDescription, actualResult.StatusDescription);
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentNullException))]
        public async Task NullStudentId_ThrowsExceptionTest()
        {
            setResponse(string.Empty, HttpStatusCode.OK);
            await client.GetStudentDocumentsAsync(null);
        }

        [TestMethod]
        public async Task GetStudentDocumentsAsync_RethrowsBadRequestExceptionTest()
        {
            bool exceptionCaught = false;
            
            var serializedResponse = JsonConvert.SerializeObject(expectedDocumentsResponse);
            setResponse(serializedResponse, HttpStatusCode.BadRequest);
            try
            {
                await client.GetStudentDocumentsAsync(_studentId);
            }
            catch { exceptionCaught = true; }
            Assert.IsTrue(exceptionCaught);
        }

        [TestMethod]
        public async Task GetStudentDocumentsAsync_RethrowsNotFoundExceptionTest()
        {
            bool exceptionCaught = false;

            var serializedResponse = JsonConvert.SerializeObject(expectedDocumentsResponse);
            setResponse(serializedResponse, HttpStatusCode.NotFound);
            try
            {
                await client.GetStudentDocumentsAsync(_studentId);
            }
            catch { exceptionCaught = true; }
            Assert.IsTrue(exceptionCaught);
        }

        [TestMethod]
        public async Task GetStudentDocumentsAsync_ReturnsEmptyStudentDocumentListTest()
        {
            var serializedResponse = JsonConvert.SerializeObject(new List<StudentDocument>());
            setResponse(serializedResponse, HttpStatusCode.OK);
            var actualResult = await client.GetStudentDocumentsAsync(_studentId);
            Assert.IsFalse(actualResult.Any());
        }        
        #endregion

        #region GetStudentLoanSummaryAsync

        private StudentLoanSummary expectedLoanSummaryResponse = new StudentLoanSummary()
        {
            DirectLoanEntranceInterviewDate = new DateTime(2016, 03, 03),
            DirectLoanMpnExpirationDate = new DateTime(2026, 03, 03),
            PlusLoanMpnExpirationDate = new DateTime(2100, 01, 01),
            GraduatePlusLoanEntranceInterviewDate = new DateTime(2016, 01, 25),
            StudentId = "0004791",
            StudentLoanCombinedTotalAmount = 50000,
            StudentLoanHistory = new List<StudentLoanHistory>(){
                new StudentLoanHistory(){
                    OpeId = "00001111",
                    TotalLoanAmount = 30000
                },
                new StudentLoanHistory(){
                    OpeId = "22223333",
                    TotalLoanAmount = 20000
                }
            }
        };

        [TestMethod]
        public async Task GetStudentLoanSummaryAsync_ReturnsExpectedResultTest()
        {
            var serializedResponse = JsonConvert.SerializeObject(expectedLoanSummaryResponse);
            setResponse(serializedResponse, HttpStatusCode.OK);

            var actualResult = (await client.GetStudentLoanSummaryAsync("0004791"));

            Assert.AreEqual(expectedLoanSummaryResponse.DirectLoanEntranceInterviewDate, actualResult.DirectLoanEntranceInterviewDate);
            Assert.AreEqual(expectedLoanSummaryResponse.DirectLoanMpnExpirationDate, actualResult.DirectLoanMpnExpirationDate);
            Assert.AreEqual(expectedLoanSummaryResponse.GraduatePlusLoanEntranceInterviewDate, actualResult.GraduatePlusLoanEntranceInterviewDate);
            Assert.AreEqual(expectedLoanSummaryResponse.PlusLoanMpnExpirationDate, actualResult.PlusLoanMpnExpirationDate);
            Assert.AreEqual(expectedLoanSummaryResponse.StudentLoanCombinedTotalAmount, actualResult.StudentLoanCombinedTotalAmount);
            Assert.AreEqual(expectedLoanSummaryResponse.StudentId, actualResult.StudentId);
            Assert.AreEqual(expectedLoanSummaryResponse.StudentLoanHistory.Count, actualResult.StudentLoanHistory.Count);
        }

        [TestMethod]
        public async Task GetStudentLoanSummaryAsync_ReturnsNullTest()
        {
            var serializedResponse = JsonConvert.SerializeObject(null);
            setResponse(serializedResponse, HttpStatusCode.OK);

            var actualResult = (await client.GetStudentLoanSummaryAsync("0004791"));
            Assert.IsNull(actualResult);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task NullStudentId_ArgumentNullExceptionThrownTest()
        {
            var serializedResponse = JsonConvert.SerializeObject(expectedLoanSummaryResponse);
            setResponse(serializedResponse, HttpStatusCode.OK);

            await client.GetStudentLoanSummaryAsync(null);
        }

        [TestMethod]
        public async Task GetStudentLoanSummaryAsync_RethrowsBadRequestExceptionTest()
        {
            bool exceptionThrown = false;
            var serializedResponse = JsonConvert.SerializeObject(expectedLoanSummaryResponse);
            setResponse(serializedResponse, HttpStatusCode.BadRequest);

            try
            {
                await client.GetStudentLoanSummaryAsync("0004791");
            }
            catch { exceptionThrown = true; }
            Assert.IsTrue(exceptionThrown);
        }

        [TestMethod]
        public async Task GetStudentLoanSummaryAsync_RethrowsKeyNotFoundExceptionTest()
        {
            bool exceptionThrown = false;
            var serializedResponse = JsonConvert.SerializeObject(expectedLoanSummaryResponse);
            setResponse(serializedResponse, HttpStatusCode.NotFound);

            try
            {
                await client.GetStudentLoanSummaryAsync("0004791");
            }
            catch { exceptionThrown = true; }
            Assert.IsTrue(exceptionThrown);
        }
        #endregion

        #region CreateStudentFinancialAidChecklistAsync

        private StudentFinancialAidChecklist expectedCreatedChecklistResponse = new StudentFinancialAidChecklist()
        {
            AwardYear = "2016",
            StudentId = "0004791",
            ChecklistItems = new List<StudentChecklistItem>(){
                new StudentChecklistItem(){
                    Code = "Awards",
                    ControlStatus = ChecklistItemControlStatus.CompletionRequiredLater
                },
                new StudentChecklistItem(){
                    Code = "Award letter",
                    ControlStatus = ChecklistItemControlStatus.RemovedFromChecklist
                },
                new StudentChecklistItem(){
                    Code = "Profile",
                    ControlStatus = ChecklistItemControlStatus.CompletionRequired
                },
                new StudentChecklistItem(){
                    Code = "FAFSA",
                    ControlStatus = ChecklistItemControlStatus.CompletionRequired
                }
            }
        };

        [TestMethod]
        public async Task CreateStudentFinancialAidChecklistAsync_ReturnsExpectedResultTest()
        {
            var serializedResponse = JsonConvert.SerializeObject(expectedCreatedChecklistResponse);
            setResponse(serializedResponse, HttpStatusCode.OK);

            var actualResult = await client.CreateStudentFinancialAidChecklistAsync("0004791", "2016");

            Assert.AreEqual(expectedCreatedChecklistResponse.AwardYear, actualResult.AwardYear);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task NullStudentId_CreateStudentChecklistAsyncThrowsExceptionTest()
        {
            var serializedResponse = JsonConvert.SerializeObject(expectedCreatedChecklistResponse);
            setResponse(serializedResponse, HttpStatusCode.OK);

            await client.CreateStudentFinancialAidChecklistAsync(null, "2016");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task NoAwardYear_CreateStudentChecklistAsyncThrowsExceptionTest()
        {
            var serializedResponse = JsonConvert.SerializeObject(expectedCreatedChecklistResponse);
            setResponse(serializedResponse, HttpStatusCode.OK);

            await client.CreateStudentFinancialAidChecklistAsync("0004791", "");
        }

        [TestMethod]
        public async Task CreateStudentFinancialAidChecklistAsync_RethrowsPermissionExceptionTest()
        {
            var exceptionThrown = false;
            var serializedResponse = JsonConvert.SerializeObject(expectedCreatedChecklistResponse);
            setResponse(serializedResponse, HttpStatusCode.Forbidden);

            try
            {
                await client.CreateStudentFinancialAidChecklistAsync("0004791", "2016");
            }
            catch { exceptionThrown = true; }
            Assert.IsTrue(exceptionThrown);
        }

        [TestMethod]
        public async Task CreateStudentFinancialAidChecklistAsync_RethrowsExistingResourceExceptionTest()
        {
            var exceptionThrown = false;
            var serializedResponse = JsonConvert.SerializeObject(expectedCreatedChecklistResponse);
            setResponse(serializedResponse, HttpStatusCode.Conflict);

            try
            {
                await client.CreateStudentFinancialAidChecklistAsync("0004791", "2016");
            }
            catch { exceptionThrown = true; }
            Assert.IsTrue(exceptionThrown);
        }

        [TestMethod]
        public async Task CreateStudentFinancialAidChecklistAsync_RethrowsGenericExceptionTest()
        {
            var exceptionThrown = false;
            var serializedResponse = JsonConvert.SerializeObject(expectedCreatedChecklistResponse);
            setResponse(serializedResponse, HttpStatusCode.BadRequest);

            try
            {
                await client.CreateStudentFinancialAidChecklistAsync("0004791", "2016");
            }
            catch { exceptionThrown = true; }
            Assert.IsTrue(exceptionThrown);
        }

        #endregion

        #region GetAllStudentChecklistsAsync

        private List<StudentFinancialAidChecklist> expectedChecklistsResponse = new List<StudentFinancialAidChecklist>()
        {
            new StudentFinancialAidChecklist()
            {
                AwardYear = "2016",
                StudentId = "0004791",
                ChecklistItems = new List<StudentChecklistItem>(){
                    new StudentChecklistItem(){
                        Code = "Awards",
                        ControlStatus = ChecklistItemControlStatus.CompletionRequiredLater
                    },
                    new StudentChecklistItem(){
                        Code = "Award letter",
                        ControlStatus = ChecklistItemControlStatus.RemovedFromChecklist
                    },
                    new StudentChecklistItem(){
                        Code = "Profile",
                        ControlStatus = ChecklistItemControlStatus.CompletionRequired
                    },
                    new StudentChecklistItem(){
                        Code = "FAFSA",
                        ControlStatus = ChecklistItemControlStatus.CompletionRequired
                    }
                }
            },
            new StudentFinancialAidChecklist()
            {
                AwardYear = "2017",
                StudentId = "0004791",
                ChecklistItems = new List<StudentChecklistItem>(){
                    new StudentChecklistItem(){
                        Code = "Award package",
                        ControlStatus = ChecklistItemControlStatus.CompletionRequired
                    },
                    new StudentChecklistItem(){
                        Code = "Award letter item",
                        ControlStatus = ChecklistItemControlStatus.CompletionRequired
                    },
                    new StudentChecklistItem(){
                        Code = "Profile",
                        ControlStatus = ChecklistItemControlStatus.CompletionRequired
                    },
                    new StudentChecklistItem(){
                        Code = "FAFSA",
                        ControlStatus = ChecklistItemControlStatus.CompletionRequired
                    }
                }
            }
        };

        [TestMethod]
        public async Task GetAllStudentChecklistsAsync_ReturnExpectedResultTest()
        {
            var serializedResponse = JsonConvert.SerializeObject(expectedChecklistsResponse);
            setResponse(serializedResponse, HttpStatusCode.OK);

            var actualResponse = (await client.GetAllStudentChecklistsAsync("0004791")).ToList();

            Assert.AreEqual(expectedChecklistsResponse.Count, actualResponse.Count());
            for (var i = 0; i < expectedChecklistsResponse.Count; i++)
            {
                var expected = expectedChecklistsResponse[i];
                var actual = actualResponse[i];
                Assert.AreEqual(expected.AwardYear, actual.AwardYear);
                Assert.AreEqual(expected.StudentId, actual.StudentId);
                Assert.AreEqual(expected.ChecklistItems.Count, actual.ChecklistItems.Count);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task NoStudentId_GetAllStudentChecklistsAsyncThrowsExceptionTest()
        {
            var serializedResponse = JsonConvert.SerializeObject(expectedChecklistsResponse);
            setResponse(serializedResponse, HttpStatusCode.OK);

            await client.GetAllStudentChecklistsAsync("");
        }

        [TestMethod]
        public async Task GetAllStudentChecklistsAsync_RethrowsPermissionExceptionTest()
        {
            var exceptionThrown = false;
            var serializedResponse = JsonConvert.SerializeObject(expectedChecklistsResponse);
            setResponse(serializedResponse, HttpStatusCode.Forbidden);
            try
            {
                await client.GetAllStudentChecklistsAsync("0004791");
            }
            catch { exceptionThrown = true; }
            Assert.IsTrue(exceptionThrown);
        }

        [TestMethod]
        public async Task GetAllStudentChecklistsAsync_RethrowsAnyExceptionTest()
        {
            var exceptionThrown = false;
            var serializedResponse = JsonConvert.SerializeObject(expectedChecklistsResponse);
            setResponse(serializedResponse, HttpStatusCode.BadRequest);
            try
            {
                await client.GetAllStudentChecklistsAsync("0004791");
            }
            catch { exceptionThrown = true; }
            Assert.IsTrue(exceptionThrown);
        }

        #endregion

        #region GetStudentFinancialAidChecklistAsync

        private StudentFinancialAidChecklist expectedChecklistResponse = new StudentFinancialAidChecklist()
        {
            AwardYear = "2036",
            StudentId = "future student",
            ChecklistItems = new List<StudentChecklistItem>(){
                new StudentChecklistItem(){
                    Code = "Awards",
                    ControlStatus = ChecklistItemControlStatus.CompletionRequiredLater
                },
                new StudentChecklistItem(){
                    Code = "Award letter",
                    ControlStatus = ChecklistItemControlStatus.RemovedFromChecklist
                },
                new StudentChecklistItem(){
                    Code = "Profile",
                    ControlStatus = ChecklistItemControlStatus.CompletionRequiredLater
                },
                new StudentChecklistItem(){
                    Code = "FAFSA",
                    ControlStatus = ChecklistItemControlStatus.CompletionRequiredLater
                },
                new StudentChecklistItem(){
                    Code = "New item for 2036",
                    ControlStatus = ChecklistItemControlStatus.CompletionRequiredLater
                }
            }
        };

        [TestMethod]
        public async Task GetStudentFinancialAidChecklistAsync_ReturnsExpectedResultTest()
        {
            var serializedResponse = JsonConvert.SerializeObject(expectedChecklistResponse);
            setResponse(serializedResponse, HttpStatusCode.OK);

            var actualResponse = await client.GetStudentFinancialAidChecklistAsync("future student", "2036");

            Assert.AreEqual(expectedChecklistResponse.AwardYear, actualResponse.AwardYear);
            Assert.AreEqual(expectedChecklistResponse.StudentId, actualResponse.StudentId);
            Assert.AreEqual(expectedChecklistResponse.ChecklistItems.Count, actualResponse.ChecklistItems.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task NullStudentId_GetStudentFinancialAidChecklistAsyncThrowsExceptionTest()
        {
            var serializedResponse = JsonConvert.SerializeObject(expectedChecklistResponse);
            setResponse(serializedResponse, HttpStatusCode.OK);

            await client.GetStudentFinancialAidChecklistAsync(null, "2036");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task NoAwardYear_GetStudentFinancialAidChecklistAsyncThrowsExceptionTest()
        {
            var serializedResponse = JsonConvert.SerializeObject(expectedChecklistResponse);
            setResponse(serializedResponse, HttpStatusCode.OK);

            await client.GetStudentFinancialAidChecklistAsync("future student", null);
        }

        [TestMethod]
        public async Task GetStudentChecklisAsync_RethrowsPermissionExceptionTest()
        {
            var exceptionThrown = false;
            var serializedResponse = JsonConvert.SerializeObject(expectedChecklistResponse);
            setResponse(serializedResponse, HttpStatusCode.Forbidden);

            try
            {
                await client.GetStudentFinancialAidChecklistAsync("future student", "2036");
            }
            catch { exceptionThrown = true; }
            Assert.IsTrue(exceptionThrown);
        }

        [TestMethod]
        public async Task GetStudentChecklisAsync_RethrowsAnyExceptionTest()
        {
            var exceptionThrown = false;
            var serializedResponse = JsonConvert.SerializeObject(expectedChecklistResponse);
            setResponse(serializedResponse, HttpStatusCode.NotFound);

            try
            {
                await client.GetStudentFinancialAidChecklistAsync("future student", "2036");
            }
            catch { exceptionThrown = true; }
            Assert.IsTrue(exceptionThrown);
        }

        #endregion

        #region CreateOutsideAwardAsync
        
        private OutsideAward expectedOutsideAwardResponse = new OutsideAward()
        {
            StudentId = "0004791",
            AwardYearCode = "2016",
            AwardName = "award name",
            AwardType = "Loan",
            AwardAmount = 567.56m,
            AwardFundingSource = "Americas"
        };

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task NullInputOutsideAward_ArgumentNullExceptionThrownTest()
        {
            var serializedResponse = JsonConvert.SerializeObject(expectedOutsideAwardResponse);
            setResponse(serializedResponse, HttpStatusCode.OK);

            await client.CreateOutsideAwardAsync(null);
        }

        [TestMethod]
        public async Task CreateOutsideAwardAsync_ReturnsExpectedResponseTest()
        {
            var serializedResponse = JsonConvert.SerializeObject(expectedOutsideAwardResponse);
            setResponse(serializedResponse, HttpStatusCode.OK);

            var actualOutsideAwardResponse = await client.CreateOutsideAwardAsync(expectedOutsideAwardResponse);
            Assert.AreEqual(expectedOutsideAwardResponse.StudentId, actualOutsideAwardResponse.StudentId);
            Assert.AreEqual(expectedOutsideAwardResponse.AwardName, actualOutsideAwardResponse.AwardName);
        }

        [TestMethod]
        public async Task CreateOutsideAwardAsync_RethrowsGenericExceptionTest()
        {
            var exceptionThrown = false;
            var serializedResponse = JsonConvert.SerializeObject(expectedOutsideAwardResponse);
            setResponse(serializedResponse, HttpStatusCode.BadRequest);

            try
            {
                await client.CreateOutsideAwardAsync(expectedOutsideAwardResponse);
            }
            catch { exceptionThrown = true; }
            Assert.IsTrue(exceptionThrown);
        }

        [TestMethod]
        public async Task CreateOutsideAwardAsync_RethrowsPermissionsExceptionTest()
        {
            var exceptionThrown = false;
            var serializedResponse = JsonConvert.SerializeObject(expectedOutsideAwardResponse);
            setResponse(serializedResponse, HttpStatusCode.Forbidden);

            try
            {
                await client.CreateOutsideAwardAsync(expectedOutsideAwardResponse);
            }
            catch { exceptionThrown = true; }
            Assert.IsTrue(exceptionThrown);
        }

        #endregion

        #region GetOutsideAwardsAsync

        private List<OutsideAward> expectedOutsideAwardsResponse = new List<OutsideAward>()
        {
            new OutsideAward(){
                Id = "1",
                StudentId = "0003914",
                AwardName = "Award1",
                AwardYearCode = "2016",
                AwardType = "loan",
                AwardAmount = 567.89m,
                AwardFundingSource = "bank"
            },
            new OutsideAward(){
                Id = "34",
                StudentId = "0003914",
                AwardName = "Award2",
                AwardYearCode = "2016",
                AwardType = "scholarship",
                AwardAmount = 3000,
                AwardFundingSource = "boy scouts"
            }
        };
        private string studentId = "0003914";
        private string awardYearCode = "2016";

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetOutsideAwardsAsync_NullStudentId_ArgumentNullExceptionThrownTest()
        {
            var serializedResponse = JsonConvert.SerializeObject(expectedOutsideAwardsResponse);
            setResponse(serializedResponse, HttpStatusCode.OK);

            await client.GetOutsideAwardsAsync(null, awardYearCode);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetOutsideAwardsAsync_NullAwardYearCode_ArgumentNullExceptionThrownTest()
        {
            var serializedResponse = JsonConvert.SerializeObject(expectedOutsideAwardsResponse);
            setResponse(serializedResponse, HttpStatusCode.OK);

            await client.GetOutsideAwardsAsync(studentId, null);
        }

        [TestMethod]
        public async Task GetOutsideAwardsAsync_ReturnsExpectedResultTest()
        {
            var serializedResponse = JsonConvert.SerializeObject(expectedOutsideAwardsResponse);
            setResponse(serializedResponse, HttpStatusCode.OK);

            var actualOutsideAwardsResponse = (await client.GetOutsideAwardsAsync(studentId, awardYearCode)).ToList();
            foreach (var expectedRecord in expectedOutsideAwardsResponse)
            {
                var actualRecord = actualOutsideAwardsResponse.First(a => a.Id == expectedRecord.Id);
                Assert.AreEqual(expectedRecord.StudentId, actualRecord.StudentId);
                Assert.AreEqual(expectedRecord.AwardName, actualRecord.AwardName);
            }
        }

        [TestMethod]
        public async Task GetOutsideAwardsAsync_RethrowsGenericExceptionTest()
        {
            var serializedResponse = JsonConvert.SerializeObject(expectedOutsideAwardsResponse);
            setResponse(serializedResponse, HttpStatusCode.BadRequest);

            bool exceptionThrown = false;
            try
            {
                await client.GetOutsideAwardsAsync(studentId, awardYearCode);
            }
            catch { exceptionThrown = true; }
            Assert.IsTrue(exceptionThrown);
        }

        [TestMethod]
        public async Task GetOutsideAwardsAsync_RethrowsPermissionsExceptionTest()
        {
            var serializedResponse = JsonConvert.SerializeObject(expectedOutsideAwardsResponse);
            setResponse(serializedResponse, HttpStatusCode.Forbidden);

            bool exceptionThrown = false;
            try
            {
                await client.GetOutsideAwardsAsync(studentId, awardYearCode);
            }
            catch { exceptionThrown = true; }
            Assert.IsTrue(exceptionThrown);
        }

        #endregion

        #region DeleteOutsideAwardAsync

        private string outsideAwardId = "bar";

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DeleteOutsideAwardAsync_NullStudentId_ArgumentNullExceptionThrownTest()
        {
            setResponse(string.Empty, HttpStatusCode.OK);
            await client.DeleteOutsideAwardAsync(null, outsideAwardId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DeleteOutsideAwardAsync_NullOutsideAwardId_ArgumentNullExceptionThrownTest()
        {
            setResponse(string.Empty, HttpStatusCode.OK);
            await client.DeleteOutsideAwardAsync(studentId, null);
        }

        [TestMethod]
        public async Task DeleteOutsideAwardAsync_RethrowsGenericExceptionTest()
        {
            setResponse(string.Empty, HttpStatusCode.BadRequest);

            bool exceptionThrown = false;
            try
            {
                await client.DeleteOutsideAwardAsync(studentId, outsideAwardId);
            }
            catch { exceptionThrown = true; }
            Assert.IsTrue(exceptionThrown);
        }

        [TestMethod]
        public async Task DeleteOutsideAwardAsync_RethrowsPermissionsExceptionTest()
        {
            setResponse(string.Empty, HttpStatusCode.Forbidden);

            bool exceptionThrown = false;
            try
            {
                await client.DeleteOutsideAwardAsync(studentId, outsideAwardId);
            }
            catch { exceptionThrown = true; }
            Assert.IsTrue(exceptionThrown);
        }

        [TestMethod]
        public async Task DeleteOutsideAwardAsync_RethrowsKeyNotFoundExceptionTest()
        {
            setResponse(string.Empty, HttpStatusCode.NotFound);

            bool exceptionThrown = false;
            try
            {
                await client.DeleteOutsideAwardAsync(studentId, outsideAwardId);
            }
            catch { exceptionThrown = true; }
            Assert.IsTrue(exceptionThrown);
        }
        #endregion

        #region CreateAwardPackageChangeRequestAsync

        private AwardPackageChangeRequest expectedChangeRequestResponse = new AwardPackageChangeRequest()
        {
            AssignedToCounselorId = "0004791",
            AwardId = "ZEBRA",
            AwardPeriodChangeRequests = new List<AwardPeriodChangeRequest>()
            {
                new AwardPeriodChangeRequest(){
                    AwardPeriodId = "16/FA",
                    NewAwardStatusId = "D",
                    Status = AwardPackageChangeRequestStatus.Pending
                }
            },
            AwardYearId = "2016",
            StudentId = "0003915"            
        };

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreateAwardPackageChangeRequestAsync_NullStudentId_ArgumentNullExceptionThrownTest(){
            var serializedResponse = JsonConvert.SerializeObject(expectedChangeRequestResponse);
            setResponse(serializedResponse, HttpStatusCode.OK);

            await client.CreateAwardPackageChangeRequestAsync(null, expectedChangeRequestResponse);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreateAwardPackageChangeRequestAsync_NullChangeRequest_ArgumentNullExceptionThrownTest()
        {
            var serializedResponse = JsonConvert.SerializeObject(expectedChangeRequestResponse);
            setResponse(serializedResponse, HttpStatusCode.OK);

            await client.CreateAwardPackageChangeRequestAsync(studentId, null);
        }

        [TestMethod]
        public async Task CreateAwardPackageChangeRequest_ReturnsExpectedResultTest()
        {
            var serializedResponse = JsonConvert.SerializeObject(expectedChangeRequestResponse);
            setResponse(serializedResponse, HttpStatusCode.OK);

            var actualRequest = await client.CreateAwardPackageChangeRequestAsync(studentId, expectedChangeRequestResponse);
            Assert.AreEqual(expectedChangeRequestResponse.AssignedToCounselorId, actualRequest.AssignedToCounselorId);
            Assert.AreEqual(expectedChangeRequestResponse.AwardId, actualRequest.AwardId);
            Assert.AreEqual(expectedChangeRequestResponse.AwardYearId, actualRequest.AwardYearId);
        }
        
        #endregion

        #region GetStudentNsldsInformationAsync

        private StudentNsldsInformation expectedNsldsInformationResponse = new StudentNsldsInformation()
        {
            StudentId = "0003914",
            PellLifetimeEligibilityUsedPercentage = 23245.89m
        };

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetStudentNsldsInformationAsync_NullStudentId_ThrowsArgumentNullExceptionTest()
        {
            var serializedResponse = JsonConvert.SerializeObject(expectedNsldsInformationResponse);
            setResponse(serializedResponse, HttpStatusCode.OK);

            await client.GetStudentNsldsInformationAsync(null);
        }

        [TestMethod]
        public async Task GetStudentNsldsInformationAsync_ReturnsExpectedResponseTest()
        {
            var serializedResponse = JsonConvert.SerializeObject(expectedNsldsInformationResponse);
            setResponse(serializedResponse, HttpStatusCode.OK);

            var actualResponse = await client.GetStudentNsldsInformationAsync(studentId);
            Assert.AreEqual(expectedNsldsInformationResponse.StudentId, actualResponse.StudentId);
            Assert.AreEqual(expectedNsldsInformationResponse.PellLifetimeEligibilityUsedPercentage, actualResponse.PellLifetimeEligibilityUsedPercentage);
        }

        [TestMethod]
        public async Task GetStudentNsldsInformationAsync_LogsErrorOnExceptionTest()
        {
            var serializedResponse = JsonConvert.SerializeObject(expectedNsldsInformationResponse);
            setResponse(serializedResponse, HttpStatusCode.Forbidden);

            bool exceptionThrown = false;
            try
            {
                await client.GetStudentNsldsInformationAsync(studentId);
            }
            catch (Exception e)
            { 
                exceptionThrown = true;
                loggerMock.Verify(l => l.Error(e, string.Format("Unable to retrieve student NSLDS information for student {0}.", studentId)));
            }
            Assert.IsTrue(exceptionThrown);
            
        }


        #endregion

        #region GetFinancialAidOffices2Async

        private List<FinancialAidOffice2> expectedOffices = new List<FinancialAidOffice2>()
        {
            new FinancialAidOffice2(){
                Id = "LAW",
                Configurations = new List<FinancialAidConfiguration2>(),
                IsDefault = true,
                OpeId = "9999999",
                AcademicProgressConfiguration = new AcademicProgressConfiguration(),
                DirectorName = "John Doe",
                EmailAddress = "example@world.com"
            },
            new FinancialAidOffice2(){
                Id = "MAIN",
                Configurations = new List<FinancialAidConfiguration2>(),
                IsDefault = true,
                OpeId = "87566472",
                AcademicProgressConfiguration = new AcademicProgressConfiguration(),
                DirectorName = "Marilyn Monroe",
                EmailAddress = "example@world.edu"
            }
        };

        [TestMethod]
        public async Task GetFinancialAidOffices2Async_ReturnsExpectedResultTest()
        {
            var serializedResponse = JsonConvert.SerializeObject(expectedOffices);
            setResponse(serializedResponse, HttpStatusCode.OK);

            CollectionAssert.Equals(expectedOffices, await client.GetFinancialAidOffices2Async());
        }

        [TestMethod]
        public async Task GetFinancialAidOffices2Async_RethrowsGenericExceptionTest()
        {
            var serializedResponse = JsonConvert.SerializeObject(expectedOffices);
            setResponse(serializedResponse, HttpStatusCode.BadRequest);

            bool exceptionThrown = false;
            try
            {
                await client.GetFinancialAidOffices2Async();
            }
            catch { exceptionThrown = true; }
            Assert.IsTrue(exceptionThrown);
        }

        #endregion

        #region GetFinancialAidOffices3Async

        private List<FinancialAidOffice3> expectedOffices3 = new List<FinancialAidOffice3>()
        {
            new FinancialAidOffice3(){
                Id = "LAW",
                Configurations = new List<FinancialAidConfiguration3>(),
                IsDefault = true,
                OpeId = "9999999",
                AcademicProgressConfiguration = new AcademicProgressConfiguration(),
                DirectorName = "John Doe",
                EmailAddress = "example@world.com"
            },
            new FinancialAidOffice3(){
                Id = "MAIN",
                Configurations = new List<FinancialAidConfiguration3>(),
                IsDefault = true,
                OpeId = "87566472",
                AcademicProgressConfiguration = new AcademicProgressConfiguration(),
                DirectorName = "Marilyn Monroe",
                EmailAddress = "example@world.edu"
            }
        };

        [TestMethod]
        public async Task GetFinancialAidOffices3Async_ReturnsExpectedResultTest()
        {
            var serializedResponse = JsonConvert.SerializeObject(expectedOffices3);
            setResponse(serializedResponse, HttpStatusCode.OK);

            CollectionAssert.Equals(expectedOffices, await client.GetFinancialAidOffices3Async());
        }

        [TestMethod]
        public async Task GetFinancialAidOffices3Async_RethrowsGenericExceptionTest()
        {
            var serializedResponse = JsonConvert.SerializeObject(expectedOffices3);
            setResponse(serializedResponse, HttpStatusCode.BadRequest);

            bool exceptionThrown = false;
            try
            {
                await client.GetFinancialAidOffices3Async();
            }
            catch { exceptionThrown = true; }
            Assert.IsTrue(exceptionThrown);
        }

        #endregion

        #region QueryFinancialAidPersonsAsync

        private List<Person> expectedFinancialAidPersonResponse = new List<Person>()
        {
            new Person()
            {
                Id = "0008877",
                PreferredName = "Julia Montesque"
            }
        };

        private FinancialAidPersonQueryCriteria searchCriteria = new FinancialAidPersonQueryCriteria()
        {
            FinancialAidPersonQueryKeyword = "Montesque"
        };

        [TestMethod]
        public async Task QueryFinancialAidPersonsAsync_ReturnsExpectedResultTest()
        {
            var serializedResponse = JsonConvert.SerializeObject(expectedFinancialAidPersonResponse);
            setResponse(serializedResponse, HttpStatusCode.OK);

            var actualPersonsResponse = (await client.QueryFinancialAidPersonsAsync(searchCriteria)).ToList();
            foreach (var person in expectedFinancialAidPersonResponse)
            {
                var actualPerson = actualPersonsResponse.FirstOrDefault(p => p.Id == person.Id);
                Assert.IsNotNull(actualPerson);
                Assert.AreEqual(person.PreferredName, actualPerson.PreferredName);
            }
        }

        [TestMethod]
        public async Task QueryFinancialAidPersonsAsync_RethrowsGenericExceptionTest()
        {
            var serializedResponse = JsonConvert.SerializeObject(expectedFinancialAidPersonResponse);
            setResponse(serializedResponse, HttpStatusCode.BadRequest);

            bool exceptionThrown = false;
            try
            {
                await client.QueryFinancialAidPersonsAsync(searchCriteria);
            }
            catch { exceptionThrown = true; }
            Assert.IsTrue(exceptionThrown);
        }

        #endregion

        #region GetFinancialAidExplanationsAsync

        private List<FinancialAidExplanation> expectedFaExplanations = new List<FinancialAidExplanation>()
        {
            new FinancialAidExplanation()
            {
                ExplanationText = "This is Pell LEU explanation",
                ExplanationType = FinancialAidExplanationType.PellLEU
            }
        };

        [TestMethod]
        public async Task GetFinancialAidExplanationsAsync_ReturnsExpectedResultTest()
        {
            var serializedResponse = JsonConvert.SerializeObject(expectedFaExplanations);
            setResponse(serializedResponse, HttpStatusCode.OK);

            CollectionAssert.Equals(expectedFaExplanations, await client.GetFinancialAidExplanationsAsync());
        }

        [TestMethod]
        public async Task GetFinancialAidExplanationsAsync_RethrowsGenericExceptionTest()
        {
            var serializedResponse = JsonConvert.SerializeObject(expectedFaExplanations);
            setResponse(serializedResponse, HttpStatusCode.BadRequest);

            bool exceptionThrown = false;
            try
            {
                await client.GetFinancialAidExplanationsAsync();
            }
            catch { exceptionThrown = true; }
            Assert.IsTrue(exceptionThrown);
        }
   

        #endregion

        #region Helpers

        private void setResponse(string serializedResponse, HttpStatusCode responseStatusCode)
        {
            var response = new HttpResponseMessage(responseStatusCode);
            response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(_serviceUrl);

            client = new ColleagueApiClient(testHttpClient, loggerMock.Object);
            client.Credentials = "otorres";
        }

        #endregion
    }
}
