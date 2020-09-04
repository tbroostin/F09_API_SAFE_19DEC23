/*Copyright 2015-2020 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Dtos.HumanResources;
using Ellucian.Web.Http.TestUtil;
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
    public class ColleagueApiClientHumanResourcesTests
    {
        public const string contentType = "application/json";
        public const string serviceUrl = "http://service.url";
        public const string payeeId = "0003914";

        private Mock<ILogger> loggerMock;
        private ColleagueApiClient client;
        private MockHandler mockHandler;

        [TestInitialize]
        public void Initialize()
        {
            mockHandler = new MockHandler();
            loggerMock = new Mock<ILogger>();
        }



        #region EarningsTypes

        [TestMethod]
        public async Task EarningsTypes_GetEarningsTypes_GetTest()
        {
            var responseObj = new List<EarningsType>()
            {
                new EarningsType() {
                    Id = "REG",
                    Description = "Regular Pay",
                    IsActive = true,
                    Category = EarningsCategory.Regular
                }
            };

            var serializedResponse = JsonConvert.SerializeObject(responseObj);

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(serializedResponse, Encoding.UTF8, contentType);
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, loggerMock.Object);
            var actualResult = await client.GetEarningsTypesAsync();

            Assert.IsInstanceOfType(actualResult, typeof(IEnumerable<EarningsType>));
            Assert.AreEqual(responseObj[0].Id, actualResult.ToArray()[0].Id);
        }

        #endregion

          #region HumanResourceDemographics
          [TestMethod]
          public async Task HumanResourceDemographics_GetHumanResourceDemographicsAsync_GetTest()
          {
               var responseObj = new List<HumanResourceDemographics>()
            {
                new HumanResourceDemographics()
                {
                     Id = "0012882",
                     FirstName = "Ted",
                     LastName = "Cruz"
                }
            };

               var serializedResponse = JsonConvert.SerializeObject(responseObj);

               var response = new HttpResponseMessage(HttpStatusCode.OK);
               response.Content = new StringContent(serializedResponse, Encoding.UTF8, contentType);
               var mockHandler = new MockHandler();
               mockHandler.Responses.Enqueue(response);

               var testHttpClient = new HttpClient(mockHandler);
               testHttpClient.BaseAddress = new Uri(serviceUrl);

               var client = new ColleagueApiClient(testHttpClient, loggerMock.Object);
               var actualResult = await client.GetHumanResourceDemographicsAsync();

               Assert.IsInstanceOfType(actualResult, typeof(IEnumerable<HumanResourceDemographics>));
               Assert.AreEqual(responseObj[0].Id, actualResult.ToArray()[0].Id);
          }

        [TestMethod]
        public async Task HumanResourceDemographics_GetHumanResourceDemographicsAsync_GetTestById()
        {
            var responseObj = new List<HumanResourceDemographics>()
            {
                new HumanResourceDemographics()
                {
                     Id = "0012882",
                     FirstName = "Ted",
                     LastName = "Cruz"
                }
            };

            var serializedResponse = JsonConvert.SerializeObject(responseObj);

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(serializedResponse, Encoding.UTF8, contentType);
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, loggerMock.Object);
            var actualResult = await client.GetHumanResourceDemographicsAsync("0012882");

            Assert.IsInstanceOfType(actualResult, typeof(IEnumerable<HumanResourceDemographics>));
            Assert.AreEqual(responseObj[0].Id, actualResult.ToArray()[0].Id);
        }

        [TestMethod]
          public async Task HumanResourceDemographics_GetSpecificHumanResourceDemographicsAsync_GetTest()
          {
               var responseObj = new HumanResourceDemographics()
               {
                    Id = "0012882",
                    FirstName = "Ted",
                    LastName = "Cruz"
               };

               var serializedResponse = JsonConvert.SerializeObject(responseObj);

               var response = new HttpResponseMessage(HttpStatusCode.OK);
               response.Content = new StringContent(serializedResponse, Encoding.UTF8, contentType);
               var mockHandler = new MockHandler();
               mockHandler.Responses.Enqueue(response);

               var testHttpClient = new HttpClient(mockHandler);
               testHttpClient.BaseAddress = new Uri(serviceUrl);

               var client = new ColleagueApiClient(testHttpClient, loggerMock.Object);
               var actualResult = await client.GetSpecificHumanResourceDemographicsAsync("0012882");

               Assert.IsInstanceOfType(actualResult, typeof(HumanResourceDemographics));
               Assert.AreEqual(responseObj.Id, actualResult.Id);
          }


          #endregion

        #region PayCycles

        [TestMethod]
        public async Task PayCycles_GetPayCycles_GetTest()
        {
            var responseObj = new List<PayCycle>()
            {
                new PayCycle() {
                    Id = "SM",
                    Description = "Semi-monthly 24/year",
                    PayClassIds = new List<string>(){"SMS","ADSM"},
                    PayPeriods = new List<PayPeriod>()
                    {
                        new PayPeriod()
                        {
                            StartDate = new DateTime(2016,01,01),
                            EndDate = new DateTime(2016,01,15)
                        }   
                    }
                }
            };

            var serializedResponse = JsonConvert.SerializeObject(responseObj);

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(serializedResponse, Encoding.UTF8, contentType);
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, loggerMock.Object);
            var actualResult = await client.GetPayCyclesAsync();

            Assert.IsInstanceOfType(actualResult, typeof(IEnumerable<PayCycle>));
            Assert.AreEqual(responseObj[0].Id, actualResult.ToArray()[0].Id);
        }

        #endregion

        #region PersonPositions
        [TestMethod]
        public async Task PersonPositions_GetPersonPositions_GetTest()
        {
            var responseObj = new List<PersonPosition>()
            {
                new PersonPosition()
                {
                    Id = "1",
                    AlternateSupervisorId = "2",
                    EndDate = null,
                    PersonId = "3",
                    PositionId = "4",
                    StartDate = DateTime.Today,
                    SupervisorId = "5"
                }
            };

            var serializedResponse = JsonConvert.SerializeObject(responseObj);

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(serializedResponse, Encoding.UTF8, contentType);
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, loggerMock.Object);
            var actualResult = await client.GetPersonPositionsAsync();

            Assert.IsInstanceOfType(actualResult, typeof(IEnumerable<PersonPosition>));
            Assert.AreEqual(responseObj[0].Id, actualResult.ToArray()[0].Id);
        }

        [TestMethod]
        public async Task PersonPositions_GetPersonPositions_GetByIdTest()
        {
            var responseObj = new List<PersonPosition>()
            {
                new PersonPosition()
                {
                    Id = "1",
                    AlternateSupervisorId = "2",
                    EndDate = null,
                    PersonId = "3",
                    PositionId = "4",
                    StartDate = DateTime.Today,
                    SupervisorId = "5"
                }
            };

            var serializedResponse = JsonConvert.SerializeObject(responseObj);

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(serializedResponse, Encoding.UTF8, contentType);
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, loggerMock.Object);
            var actualResult = await client.GetPersonPositionsAsync("0000001");

            Assert.IsInstanceOfType(actualResult, typeof(IEnumerable<PersonPosition>));
            Assert.AreEqual(responseObj[0].Id, actualResult.ToArray()[0].Id);
        }

        #endregion

        #region PersonPositionWages

        [TestMethod]
        public async Task PersonPositionWages_GetPersonPositionWagesAsync_GetTest()
        {
            var responseObj = new List<PersonPositionWage>()
            {
                new PersonPositionWage()
                {
                    Id = "1",
                    EndDate = DateTime.Today,
                    FundingSources = new List<PositionFundingSource>() {new PositionFundingSource() {FundingSourceId = "COMP", FundingOrder = 0, ProjectId = "20"}},
                    IsPaySuspended = false,
                    PayClassId = "CM",
                    PayCycleId = "MC",
                    PersonId = "0003914",
                    PersonPositionId = "123",
                    PositionId = "PROFESSOR",
                    PositionPayDefaultId = "321",
                    RegularWorkEarningsTypeId = "REG",
                    StartDate = DateTime.Today
                }
            };

            var serializedResponse = JsonConvert.SerializeObject(responseObj);

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(serializedResponse, Encoding.UTF8, contentType);
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, loggerMock.Object);
            var actualResult = await client.GetPersonPositionWagesAsync();

            Assert.IsInstanceOfType(actualResult, typeof(IEnumerable<PersonPositionWage>));
            Assert.AreEqual(responseObj[0].Id, actualResult.ToArray()[0].Id);
        }

        [TestMethod]
        public async Task PersonPositionWages_GetPersonPositionWagesAsync_GetByIdTest()
        {
            var responseObj = new List<PersonPositionWage>()
            {
                new PersonPositionWage()
                {
                    Id = "1",
                    EndDate = DateTime.Today,
                    FundingSources = new List<PositionFundingSource>() {new PositionFundingSource() {FundingSourceId = "COMP", FundingOrder = 0, ProjectId = "20"}},
                    IsPaySuspended = false,
                    PayClassId = "CM",
                    PayCycleId = "MC",
                    PersonId = "0003914",
                    PersonPositionId = "123",
                    PositionId = "PROFESSOR",
                    PositionPayDefaultId = "321",
                    RegularWorkEarningsTypeId = "REG",
                    StartDate = DateTime.Today
                }
            };

            var serializedResponse = JsonConvert.SerializeObject(responseObj);

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(serializedResponse, Encoding.UTF8, contentType);
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, loggerMock.Object);
            var actualResult = await client.GetPersonPositionWagesAsync("0000001");

            Assert.IsInstanceOfType(actualResult, typeof(IEnumerable<PersonPositionWage>));
            Assert.AreEqual(responseObj[0].Id, actualResult.ToArray()[0].Id);
        }

        #endregion

        #region Positions

        [TestMethod]
        public async Task Positions_GetPositions_GetTest()
        {
            var responseObj = new List<Position>()
            {
                new Position() {
                    Id = "1",
                    AlternateSupervisorPositionId = "2",
                    EndDate = null,
                    IsExempt = false,
                    IsSalary = true,
                    PositionPayScheduleIds = new List<string>(){"3","4"},
                    ShortTitle = "Pos",
                    Title = "Position",
                    StartDate = DateTime.Today,
                    SupervisorPositionId = "5"    
                }
            };


            var serializedResponse = JsonConvert.SerializeObject(responseObj);

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(serializedResponse, Encoding.UTF8, contentType);
            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, loggerMock.Object);
            var actualResult = await client.GetPositionsAsync();

            Assert.IsInstanceOfType(actualResult, typeof(IEnumerable<Position>));
            Assert.AreEqual(responseObj[0].Id, actualResult.ToArray()[0].Id);
        }

        #endregion

        #region PayStatements
        [TestMethod]
        public async Task PayStatementPdf_GetTest()
        {
            var responseObj = new byte[1] { byte.Parse("1") };

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new ByteArrayContent(responseObj);

            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, loggerMock.Object);
            var actual = await client.GetPayStatementPdf("foobar");

            //Assert.AreEqual(responseObj., actual);
            CollectionAssert.AreEqual(responseObj, actual.Item2);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task PayStatementPdf_IdRequiredTest()
        {
            var mockHandler = new MockHandler();
            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(serviceUrl);
            var client = new ColleagueApiClient(testHttpClient, loggerMock.Object);
            var actual = await client.GetPayStatementPdf(null);

        }
        #endregion


        #region employee-leave-plans
        [TestMethod]
        public async Task EmployeeLeavePlansV2_GetAllTest()
        {
            var responseObj = new List<EmployeeLeavePlan>()
            {
                new EmployeeLeavePlan()
                {
                    Id = "foobar"
                }
            };

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(JsonConvert.SerializeObject(responseObj));

            var mockHandler = new MockHandler();
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(serviceUrl);

            var client = new ColleagueApiClient(testHttpClient, loggerMock.Object);
            var actual = await client.GetEmployeeLeavePlansAsync();

            Assert.AreEqual(responseObj[0].Id, actual.First().Id);

            
        }
        #endregion

        #region GetEmployeeBenefitsEnrollmentPackageAsync
        private string employeeId = "1234567";
        public EmployeeBenefitsEnrollmentPackage expectedEnrollmentPackage = new EmployeeBenefitsEnrollmentPackage()
        {
            BenefitsEnrollmentPeriodId = "19Fall",
            EmployeeId = "1234567",
            PackageDescription = "Benefits package",
            PackageId ="BEN",
            EmployeeEligibleBenefitTypes = new List<EmployeeBenefitType>()
            {
                new EmployeeBenefitType()
                {
                    BenefitType = "MED",
                    BenefitTypeDescription = "Medical"
                },
                new EmployeeBenefitType()
                {
                    BenefitType = "DEN",
                    BenefitTypeDescription = "Dental"
                }
            }
        };

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetEmployeeBenefitsEnrollmentPackageAsync_NullEmployeeId_ArgumentNullExceptionThrownTest()
        {
            var serializedResponse = JsonConvert.SerializeObject(expectedEnrollmentPackage);
            setResponse(serializedResponse, HttpStatusCode.OK);

            await client.GetEmployeeBenefitsEnrollmentPackageAsync(null);
        }

        [TestMethod]
        public async Task GetEmployeeBenefitsEnrollmentPackageAsync_RethrowsExceptionTest()
        {
            bool exceptionThrown = false;
            var serializedResponse = JsonConvert.SerializeObject(expectedEnrollmentPackage);
            setResponse(serializedResponse, HttpStatusCode.BadRequest);
            try
            {
                await client.GetEmployeeBenefitsEnrollmentPackageAsync(employeeId);
            }
            catch { exceptionThrown = true; }
            Assert.IsTrue(exceptionThrown);
            
        }

        [TestMethod]
        public async Task GetEmployeeBenefitsEnrollmentPackageAsync_ReturnExpectedResultTest()
        {
            var serializedResponse = JsonConvert.SerializeObject(expectedEnrollmentPackage);
            setResponse(serializedResponse, HttpStatusCode.OK);
            var actualEnrollmentPackage = await client.GetEmployeeBenefitsEnrollmentPackageAsync(employeeId);

            Assert.AreEqual(expectedEnrollmentPackage.BenefitsEnrollmentPeriodId, actualEnrollmentPackage.BenefitsEnrollmentPeriodId);
            Assert.AreEqual(expectedEnrollmentPackage.EmployeeId, actualEnrollmentPackage.EmployeeId);
            Assert.AreEqual(expectedEnrollmentPackage.PackageDescription, actualEnrollmentPackage.PackageDescription);
            Assert.AreEqual(expectedEnrollmentPackage.PackageId, actualEnrollmentPackage.PackageId);
            Assert.AreEqual(expectedEnrollmentPackage.EmployeeEligibleBenefitTypes.Count(), actualEnrollmentPackage.EmployeeEligibleBenefitTypes.Count());
        }

        #endregion

        #region QueryEnrollmentPeriodBenefitsAsync
        private IEnumerable<EnrollmentPeriodBenefit> expectedBenefits = new List<EnrollmentPeriodBenefit>()
        {
            new EnrollmentPeriodBenefit()
                    {
                        BenefitId = "MED",
                        BenefitDescription = "medical",
                        BenefitTypeId = "MED",
                        EnrollmentPeriodBenefitId = "MED2020"
                    },
                    new EnrollmentPeriodBenefit()
                    {
                        BenefitId = "DEN",
                        BenefitDescription = "dental",
                        BenefitTypeId = "DEN",
                        EnrollmentPeriodBenefitId = "DEN2020"
                    }
        };

        private BenefitEnrollmentBenefitsQueryCriteria enrollmentPeriodBenefitsCriteria = new BenefitEnrollmentBenefitsQueryCriteria()
        {
            BenefitTypeId = "MED"
        };

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task QueryEnrollmentPeriodBenefitsAsync_NullCriteria_ArgumentNullExceptionThrownTest()
        {
            var serializedResponse = JsonConvert.SerializeObject(expectedBenefits);
            setResponse(serializedResponse, HttpStatusCode.OK);

            await client.QueryEnrollmentPeriodBenefitsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task QueryEnrollmentPeriodBenefitsAsync_NullBenefitTypeId_ArgumentExceptionThrownTest()
        {
            var serializedResponse = JsonConvert.SerializeObject(expectedBenefits);
            setResponse(serializedResponse, HttpStatusCode.OK);

            await client.QueryEnrollmentPeriodBenefitsAsync(new BenefitEnrollmentBenefitsQueryCriteria());
        }

        [TestMethod]
        public async Task QueryEnrollmentPeriodBenefitsAsync_RethrowsExceptionTest()
        {
            bool exceptionThrown = false;
            var serializedResponse = JsonConvert.SerializeObject(expectedBenefits);
            setResponse(serializedResponse, HttpStatusCode.BadRequest);
            try
            {
                await client.QueryEnrollmentPeriodBenefitsAsync(enrollmentPeriodBenefitsCriteria);
            }
            catch { exceptionThrown = true; }
            Assert.IsTrue(exceptionThrown);

        }

        [TestMethod]
        public async Task QueryEnrollmentPeriodBenefitsAsync_ReturnExpectedResultTest()
        {
            var serializedResponse = JsonConvert.SerializeObject(expectedBenefits);
            setResponse(serializedResponse, HttpStatusCode.OK);

            var actualBenefits = await client.QueryEnrollmentPeriodBenefitsAsync(enrollmentPeriodBenefitsCriteria);
            foreach(var expected in expectedBenefits)
            {
                var actual = actualBenefits.FirstOrDefault(b => b.BenefitId == expected.BenefitId);
                Assert.IsNotNull(actual);
                Assert.AreEqual(expected.BenefitDescription, actual.BenefitDescription);
                Assert.AreEqual(expected.BenefitTypeId, actual.BenefitTypeId);
                Assert.AreEqual(expected.EnrollmentPeriodBenefitId, actual.EnrollmentPeriodBenefitId);
            }
        }

        #endregion

            #region Helpers

        private void setResponse(string serializedResponse, HttpStatusCode responseStatusCode)
        {
            var response = new HttpResponseMessage(responseStatusCode);
            response.Content = new StringContent(serializedResponse, Encoding.UTF8, contentType);
            mockHandler.Responses.Enqueue(response);

            var testHttpClient = new HttpClient(mockHandler);
            testHttpClient.BaseAddress = new Uri(serviceUrl);

            client = new ColleagueApiClient(testHttpClient, loggerMock.Object);
            client.Credentials = "otorres";
        }

        #endregion
    }
}
