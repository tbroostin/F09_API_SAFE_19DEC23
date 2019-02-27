//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers.HumanResources;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.HumanResources
{
    [TestClass]
    public class PayrollDeductionArrangementsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IPayrollDeductionArrangementService> payrollDeductionArrangementsServiceMock;
        private Mock<ILogger> loggerMock;
        private PayrollDeductionArrangementsController payrollDeductionArrangementsController;      
        private IEnumerable<Domain.HumanResources.Entities.PayrollDeductionArrangements> allPayrollDeductionArrangements;
        private List<Dtos.PayrollDeductionArrangements> payrollDeductionArrangementsCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize() 
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            payrollDeductionArrangementsServiceMock = new Mock<IPayrollDeductionArrangementService>();
            loggerMock = new Mock<ILogger>();
            payrollDeductionArrangementsCollection = new List<Dtos.PayrollDeductionArrangements>();

            allPayrollDeductionArrangements  = new List<Domain.HumanResources.Entities.PayrollDeductionArrangements>()
                {
                    new Domain.HumanResources.Entities.PayrollDeductionArrangements("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d"),
                    new Domain.HumanResources.Entities.PayrollDeductionArrangements("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "d2253ac7-9931-4560-b42f-1fccd43c952e"),
                    new Domain.HumanResources.Entities.PayrollDeductionArrangements("d2253ac7-9931-4560-b42f-1fccd43c952e", "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc")
                };
            
            foreach (var source in allPayrollDeductionArrangements)
            {
                var payrollDeductionArrangements = new Ellucian.Colleague.Dtos.PayrollDeductionArrangements
                {
                    Id = source.Guid,
                    Person = new GuidObject2(source.PersonId),
                    Status = Dtos.EnumProperties.PayrollDeductionArrangementStatuses.Active,
                    PaymentTarget = new Dtos.DtoProperties.PaymentTargetDtoProperty()
                    {
                        Deduction = new Dtos.DtoProperties.PaymentTargetDeduction() {
                            DeductionType = new GuidObject2(source.Guid)
                        }
                    },
                    amountPerPayment = new Dtos.DtoProperties.AmountDtoProperty()
                    {
                        Value = (decimal?) 5.0,
                        Currency = Dtos.EnumProperties.CurrencyCodes.USD
                    },
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now,
                    PayPeriodOccurence = new Dtos.DtoProperties.PayPeriodOccurance()
                    {
                        Interval = 4
                    }
                    
                    //Job = new GuidObject2(source.PerposId),
                    //CompletedOn = (DateTime) source.CompletedDate,
                    //Type = new GuidObject2(source.RatingCycleCode),
                    //Rating = new EmploymentPerformanceReviewsRatingDtoProperty() { Detail = new GuidObject2(source.RatingCycleCode) }
                };
                payrollDeductionArrangementsCollection.Add(payrollDeductionArrangements);
            }

            payrollDeductionArrangementsController = new PayrollDeductionArrangementsController(loggerMock.Object, payrollDeductionArrangementsServiceMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            payrollDeductionArrangementsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            payrollDeductionArrangementsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
        }

        [TestCleanup]
        public void Cleanup()
        {
            payrollDeductionArrangementsController = null;
            allPayrollDeductionArrangements = null;
            payrollDeductionArrangementsCollection = null;
            loggerMock = null;
            payrollDeductionArrangementsServiceMock = null;
        }

        #region GET
        [TestMethod]
        public async Task PayrollDeductionArrangementsController_GetPayrollDeductionArrangements_ValidateFields_Nocache()
        {
            payrollDeductionArrangementsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            var payrollDeductionArrangementsTuple
                    = new Tuple<IEnumerable<PayrollDeductionArrangements>, int>(payrollDeductionArrangementsCollection, payrollDeductionArrangementsCollection.Count);

            payrollDeductionArrangementsServiceMock.Setup(x => x.GetPayrollDeductionArrangementsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), 
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(payrollDeductionArrangementsTuple);

            Paging paging = new Paging(payrollDeductionArrangementsCollection.Count(), 0);
            var payrollDeductionArrangements = (await payrollDeductionArrangementsController.GetPayrollDeductionArrangementsAsync(paging));

            var cancelToken = new System.Threading.CancellationToken(false);
            var httpResponseMessage = await payrollDeductionArrangements.ExecuteAsync(cancelToken);
            var results = ((ObjectContent<IEnumerable<Dtos.PayrollDeductionArrangements>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.PayrollDeductionArrangements>;

            Assert.IsNotNull(results);
            Assert.AreEqual(payrollDeductionArrangementsCollection.Count, results.Count());
            foreach (var actual in results)
            {
                var expected = payrollDeductionArrangementsCollection.FirstOrDefault(i => i.Id.Equals(actual.Id));
                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Person.Id, actual.Person.Id);
                //Assert.AreEqual(expected.Job.Id, actual.Job.Id);
                //Assert.AreEqual(expected.CompletedOn, actual.CompletedOn);
                //Assert.AreEqual(expected.Type.Id, actual.Type.Id);
                //Assert.AreEqual(expected.Rating.Detail.Id, actual.Rating.Detail.Id);
                //if (expected.Comment != null)
                //    Assert.AreEqual(expected.Comment, actual.Comment);
                //if (expected.ReviewedBy != null)
                //    Assert.AreEqual(expected.ReviewedBy.Id, actual.ReviewedBy.Id);
            }
        }

        [TestMethod]
        public async Task PayrollDeductionArrangementsController_GetPayrollDeductionArrangements_ValidateFields_Cache()
        {
            payrollDeductionArrangementsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue {NoCache = true};

            var payrollDeductionArrangementsTuple
                    = new Tuple<IEnumerable<PayrollDeductionArrangements>, int>(payrollDeductionArrangementsCollection, payrollDeductionArrangementsCollection.Count);

            payrollDeductionArrangementsServiceMock.Setup(x => x.GetPayrollDeductionArrangementsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(payrollDeductionArrangementsTuple);

            Paging paging = new Paging(payrollDeductionArrangementsCollection.Count(), 0);
            var payrollDeductionArrangements = (await payrollDeductionArrangementsController.GetPayrollDeductionArrangementsAsync(paging));

            var cancelToken = new System.Threading.CancellationToken(false);
            var httpResponseMessage = await payrollDeductionArrangements.ExecuteAsync(cancelToken);
            var results = ((ObjectContent<IEnumerable<Dtos.PayrollDeductionArrangements>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.PayrollDeductionArrangements>;

            Assert.IsNotNull(results);
            Assert.AreEqual(payrollDeductionArrangementsCollection.Count, results.Count());
            foreach (var actual in results)
            {
                var expected = payrollDeductionArrangementsCollection.FirstOrDefault(i => i.Id.Equals(actual.Id));
                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Person.Id, actual.Person.Id);
                //Assert.AreEqual(expected.Job.Id, actual.Job.Id);
                //Assert.AreEqual(expected.CompletedOn, actual.CompletedOn);
                //Assert.AreEqual(expected.Type.Id, actual.Type.Id);
                //Assert.AreEqual(expected.Rating.Detail.Id, actual.Rating.Detail.Id);
                //if (expected.Comment != null)
                //    Assert.AreEqual(expected.Comment, actual.Comment);
                //if (expected.ReviewedBy != null)
                //    Assert.AreEqual(expected.ReviewedBy.Id, actual.ReviewedBy.Id);
            }
        }

         [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayrollDeductionArrangementsController_GetPayrollDeductionArrangements_KeyNotFoundException()
        {
            //
            payrollDeductionArrangementsServiceMock.Setup(x => x.GetPayrollDeductionArrangementsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Throws<KeyNotFoundException>();
            Paging paging = new Paging(payrollDeductionArrangementsCollection.Count(), 0);
            await payrollDeductionArrangementsController.GetPayrollDeductionArrangementsAsync(paging);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayrollDeductionArrangementsController_GetPayrollDeductionArrangements_PermissionsException()
        {

            payrollDeductionArrangementsServiceMock.Setup(x => x.GetPayrollDeductionArrangementsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Throws<PermissionsException>();
            Paging paging = new Paging(payrollDeductionArrangementsCollection.Count(), 0);
            await payrollDeductionArrangementsController.GetPayrollDeductionArrangementsAsync(paging);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayrollDeductionArrangementsController_GetPayrollDeductionArrangements_ArgumentException()
        {

            payrollDeductionArrangementsServiceMock.Setup(x => x.GetPayrollDeductionArrangementsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Throws<ArgumentException>();
            Paging paging = new Paging(payrollDeductionArrangementsCollection.Count(), 0);
            await payrollDeductionArrangementsController.GetPayrollDeductionArrangementsAsync(paging);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayrollDeductionArrangementsController_GetPayrollDeductionArrangements_RepositoryException()
        {

            payrollDeductionArrangementsServiceMock.Setup(x => x.GetPayrollDeductionArrangementsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Throws<RepositoryException>();
            Paging paging = new Paging(payrollDeductionArrangementsCollection.Count(), 0);
            await payrollDeductionArrangementsController.GetPayrollDeductionArrangementsAsync(paging);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayrollDeductionArrangementsController_GetPayrollDeductionArrangements_IntegrationApiException()
        {

            payrollDeductionArrangementsServiceMock.Setup(x => x.GetPayrollDeductionArrangementsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Throws<IntegrationApiException>();
            Paging paging = new Paging(payrollDeductionArrangementsCollection.Count(), 0);
            await payrollDeductionArrangementsController.GetPayrollDeductionArrangementsAsync(paging);
        }

        [TestMethod]
        public async Task PayrollDeductionArrangementsController_GetPayrollDeductionArrangementsByGuidAsync_ValidateFields()
        {
            var expected = payrollDeductionArrangementsCollection.FirstOrDefault();
            payrollDeductionArrangementsServiceMock.Setup(x => x.GetPayrollDeductionArrangementsByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await payrollDeductionArrangementsController.GetPayrollDeductionArrangementByIdAsync(expected.Id);

            Assert.IsNotNull(expected);
            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.Person.Id, actual.Person.Id);
            //Assert.AreEqual(expected.Job.Id, actual.Job.Id);
            //Assert.AreEqual(expected.CompletedOn, actual.CompletedOn);
            //Assert.AreEqual(expected.Type.Id, actual.Type.Id);
            //Assert.AreEqual(expected.Rating.Detail.Id, actual.Rating.Detail.Id);
            //if (expected.Comment != null)
            //    Assert.AreEqual(expected.Comment, actual.Comment);
            //if (expected.ReviewedBy != null)
            //    Assert.AreEqual(expected.ReviewedBy.Id, actual.ReviewedBy.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayrollDeductionArrangementsController_GetPayrollDeductionArrangements_Exception()
        {
            payrollDeductionArrangementsServiceMock.Setup(x => x.GetPayrollDeductionArrangementsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Throws<Exception>();
            Paging paging = new Paging(payrollDeductionArrangementsCollection.Count(), 0);
            await payrollDeductionArrangementsController.GetPayrollDeductionArrangementsAsync(paging);       
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayrollDeductionArrangementsController_GetPayrollDeductionArrangementsByGuidAsync_Exception()
        {
            payrollDeductionArrangementsServiceMock.Setup(x => x.GetPayrollDeductionArrangementsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await payrollDeductionArrangementsController.GetPayrollDeductionArrangementByIdAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayrollDeductionArrangementsController_GetPayrollDeductionArrangementsByGuid_KeyNotFoundException()
        {
            payrollDeductionArrangementsServiceMock.Setup(x => x.GetPayrollDeductionArrangementsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<KeyNotFoundException>();
            await payrollDeductionArrangementsController.GetPayrollDeductionArrangementByIdAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayrollDeductionArrangementsController_GetPayrollDeductionArrangementsByGuid_PermissionsException()
        {
            payrollDeductionArrangementsServiceMock.Setup(x => x.GetPayrollDeductionArrangementsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<PermissionsException>();
            await payrollDeductionArrangementsController.GetPayrollDeductionArrangementByIdAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task PayrollDeductionArrangementsController_GetPayrollDeductionArrangementsByGuid_ArgumentException()
        {
            payrollDeductionArrangementsServiceMock.Setup(x => x.GetPayrollDeductionArrangementsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<ArgumentException>();
            await payrollDeductionArrangementsController.GetPayrollDeductionArrangementByIdAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayrollDeductionArrangementsController_GetPayrollDeductionArrangementsByGuid_RepositoryException()
        {
            payrollDeductionArrangementsServiceMock.Setup(x => x.GetPayrollDeductionArrangementsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<RepositoryException>();
            await payrollDeductionArrangementsController.GetPayrollDeductionArrangementByIdAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayrollDeductionArrangementsController_GetPayrollDeductionArrangementsByGuid_IntegrationApiException()
        {
            payrollDeductionArrangementsServiceMock.Setup(x => x.GetPayrollDeductionArrangementsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<IntegrationApiException>();
            await payrollDeductionArrangementsController.GetPayrollDeductionArrangementByIdAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayrollDeductionArrangementsController_GetPayrollDeductionArrangementsByGuid_Exception()
        {
            payrollDeductionArrangementsServiceMock.Setup(x => x.GetPayrollDeductionArrangementsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await payrollDeductionArrangementsController.GetPayrollDeductionArrangementByIdAsync(expectedGuid);
        }
        #endregion

        #region PUT
        [TestMethod]
        public async Task PayrollDeductionArrangementsController_PutPayrollDeductionArrangementAsync()
        {
            payrollDeductionArrangementsServiceMock.Setup(i => i.GetPayrollDeductionArrangementsByGuidAsync(payrollDeductionArrangementsCollection.FirstOrDefault().Id, It.IsAny<bool>())).ReturnsAsync(payrollDeductionArrangementsCollection.FirstOrDefault());
            payrollDeductionArrangementsServiceMock.Setup(i => i.UpdatePayrollDeductionArrangementsAsync(payrollDeductionArrangementsCollection.FirstOrDefault().Id, payrollDeductionArrangementsCollection.FirstOrDefault())).ReturnsAsync(payrollDeductionArrangementsCollection.FirstOrDefault());
            var result = await payrollDeductionArrangementsController.PutPayrollDeductionArrangementAsync(payrollDeductionArrangementsCollection.FirstOrDefault().Id, payrollDeductionArrangementsCollection.FirstOrDefault());
            Assert.AreEqual(payrollDeductionArrangementsCollection.FirstOrDefault().Id, result.Id);
            Assert.AreEqual(payrollDeductionArrangementsCollection.FirstOrDefault().Person.Id, result.Person.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayrollDeductionArrangementsController_PutPayrollDeductionArrangementAsync_ArgumentNullException()
        {
            payrollDeductionArrangementsServiceMock.Setup(i => i.UpdatePayrollDeductionArrangementsAsync(It.IsAny<string>(), It.IsAny<Dtos.PayrollDeductionArrangements>())).ThrowsAsync(new ArgumentNullException());
            await payrollDeductionArrangementsController.PutPayrollDeductionArrangementAsync(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayrollDeductionArrangementsController_PutPayrollDeductionArrangementAsync_IdNull_ArgumentNullException()
        {
            payrollDeductionArrangementsCollection.FirstOrDefault().Id = string.Empty;
            payrollDeductionArrangementsServiceMock.Setup(i => i.UpdatePayrollDeductionArrangementsAsync(It.IsAny<string>(), It.IsAny<Dtos.PayrollDeductionArrangements>())).ThrowsAsync(new ArgumentNullException());
            await payrollDeductionArrangementsController.PutPayrollDeductionArrangementAsync(payrollDeductionArrangementsCollection.FirstOrDefault().Id, payrollDeductionArrangementsCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayrollDeductionArrangementsController_PutPayrollDeductionArrangementAsync_InvalidOperationException()
        {
            payrollDeductionArrangementsServiceMock.Setup(i => i.UpdatePayrollDeductionArrangementsAsync(It.IsAny<string>(), It.IsAny<Dtos.PayrollDeductionArrangements>())).ThrowsAsync(new InvalidOperationException());
            await payrollDeductionArrangementsController.PutPayrollDeductionArrangementAsync("id", payrollDeductionArrangementsCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayrollDeductionArrangementsController_PutPayrollDeductionArrangementAsync_KeyNotFoundException()
        {
            payrollDeductionArrangementsServiceMock.Setup(i => i.UpdatePayrollDeductionArrangementsAsync(It.IsAny<string>(), It.IsAny<Dtos.PayrollDeductionArrangements>())).ThrowsAsync(new KeyNotFoundException());
            await payrollDeductionArrangementsController.PutPayrollDeductionArrangementAsync("id", payrollDeductionArrangementsCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayrollDeductionArrangementsController_PutPayrollDeductionArrangementAsync_Exception()
        {
            payrollDeductionArrangementsServiceMock.Setup(i => i.UpdatePayrollDeductionArrangementsAsync(It.IsAny<string>(), It.IsAny<Dtos.PayrollDeductionArrangements>())).ThrowsAsync(new Exception());
            await payrollDeductionArrangementsController.PutPayrollDeductionArrangement2Async("id", payrollDeductionArrangementsCollection.FirstOrDefault());
        }
        #endregion

        #region POST
        [TestMethod]
        public async Task PayrollDeductionArrangementsController_PostPayrollDeductionArrangementAsync()
        {
            payrollDeductionArrangementsServiceMock.Setup(i => i.CreatePayrollDeductionArrangementsAsync(payrollDeductionArrangementsCollection.FirstOrDefault())).ReturnsAsync(payrollDeductionArrangementsCollection.FirstOrDefault());
            var result = await payrollDeductionArrangementsController.PostPayrollDeductionArrangementAsync(payrollDeductionArrangementsCollection.FirstOrDefault());
            Assert.AreEqual(payrollDeductionArrangementsCollection.FirstOrDefault().Id, result.Id);
            Assert.AreEqual(payrollDeductionArrangementsCollection.FirstOrDefault().Person.Id, result.Person.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayrollDeductionArrangementsController_PostPayrollDeductionArrangementAsync_ArgumentNullException()
        {
            payrollDeductionArrangementsServiceMock.Setup(i => i.CreatePayrollDeductionArrangementsAsync(It.IsAny<Dtos.PayrollDeductionArrangements>())).ThrowsAsync(new ArgumentNullException());
            await payrollDeductionArrangementsController.PostPayrollDeductionArrangementAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayrollDeductionArrangementsController_PostPayrollDeductionArrangementAsync_IdNull_ArgumentNullException()
        {
            payrollDeductionArrangementsCollection.FirstOrDefault().Id = string.Empty;
            payrollDeductionArrangementsServiceMock.Setup(i => i.CreatePayrollDeductionArrangementsAsync(It.IsAny<Dtos.PayrollDeductionArrangements>())).ThrowsAsync(new ArgumentNullException());
            await payrollDeductionArrangementsController.PostPayrollDeductionArrangementAsync(payrollDeductionArrangementsCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayrollDeductionArrangementsController_PostPayrollDeductionArrangementAsync_InvalidOperationException()
        {
            payrollDeductionArrangementsServiceMock.Setup(i => i.CreatePayrollDeductionArrangementsAsync(It.IsAny<Dtos.PayrollDeductionArrangements>())).ThrowsAsync(new InvalidOperationException());
            await payrollDeductionArrangementsController.PostPayrollDeductionArrangementAsync(payrollDeductionArrangementsCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayrollDeductionArrangementsController_PostPayrollDeductionArrangementAsync_KeyNotFoundException()
        {
            payrollDeductionArrangementsServiceMock.Setup(i => i.CreatePayrollDeductionArrangementsAsync(It.IsAny<Dtos.PayrollDeductionArrangements>())).ThrowsAsync(new KeyNotFoundException());
            await payrollDeductionArrangementsController.PostPayrollDeductionArrangementAsync(payrollDeductionArrangementsCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayrollDeductionArrangementsController_PostPayrollDeductionArrangementAsync_Exception()
        {
            payrollDeductionArrangementsServiceMock.Setup(i => i.CreatePayrollDeductionArrangementsAsync(It.IsAny<Dtos.PayrollDeductionArrangements>())).ThrowsAsync(new Exception());
            await payrollDeductionArrangementsController.PostPayrollDeductionArrangementsAsync(payrollDeductionArrangementsCollection.FirstOrDefault());
        }
        #endregion

        #region DELETE
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayrollDeductionArrangementsController_DeletePayrollDeductionArrangementAsync_HttpResponseMessage()
        {
            string id = "375ef15b-f2d2-40ed-ac47-f0d2d45260f0";
            await payrollDeductionArrangementsController.DeletePayrollDeductionArrangementAsync(id);
        }
        #endregion
    }

}