// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class FinancialAidAwardPeriodsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IFinancialAidAwardPeriodService> financialAidAwardPeriodServiceMock;
        private Mock<ILogger> loggerMock;
        private FinancialAidAwardPeriodsController financialAidAwardPeriodsController;
        private IEnumerable<FinancialAidAwardPeriod> allFinancialAidAwardPeriod;
        private List<Dtos.FinancialAidAwardPeriod> financialAidAwardPeriodCollection;

        [TestInitialize]
        public async void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            financialAidAwardPeriodServiceMock = new Mock<IFinancialAidAwardPeriodService>();
            loggerMock = new Mock<ILogger>();
            financialAidAwardPeriodCollection = new List<Dtos.FinancialAidAwardPeriod>();

            allFinancialAidAwardPeriod = (await new TestStudentReferenceDataRepository().GetFinancialAidAwardPeriodsAsync(true)).ToList();

            foreach (var source in allFinancialAidAwardPeriod)
            {
                var financialAidAwardPeriod = new Ellucian.Colleague.Dtos.FinancialAidAwardPeriod
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null,
                    Status = "STATUS1"
                };
                financialAidAwardPeriodCollection.Add(financialAidAwardPeriod);
            }

            financialAidAwardPeriodsController = new FinancialAidAwardPeriodsController(financialAidAwardPeriodServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            financialAidAwardPeriodsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            financialAidAwardPeriodsController = null;
            allFinancialAidAwardPeriod = null;
            financialAidAwardPeriodCollection = null;
            loggerMock = null;
            financialAidAwardPeriodServiceMock = null;
        }

        [TestMethod]
        public async Task FinancialAidAwardPeriodController_GetFinancialAidAwardPeriod_ValidateFields_Nocache()
        {
            financialAidAwardPeriodsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            financialAidAwardPeriodServiceMock.Setup(x => x.GetFinancialAidAwardPeriodsAsync(false)).ReturnsAsync(financialAidAwardPeriodCollection);

            var sourceContexts = (await financialAidAwardPeriodsController.GetFinancialAidAwardPeriodsAsync()).ToList();
            Assert.AreEqual(financialAidAwardPeriodCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = financialAidAwardPeriodCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task FinancialAidAwardPeriodController_GetFinancialAidAwardPeriod_ValidateFields_Cache()
        {
            financialAidAwardPeriodsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            financialAidAwardPeriodServiceMock.Setup(x => x.GetFinancialAidAwardPeriodsAsync(true)).ReturnsAsync(financialAidAwardPeriodCollection);

            var sourceContexts = (await financialAidAwardPeriodsController.GetFinancialAidAwardPeriodsAsync()).ToList();
            Assert.AreEqual(financialAidAwardPeriodCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = financialAidAwardPeriodCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task FinancialAidAwardPeriodController_GetFinancialAidAwardPeriodsByIdAsync_ValidateFields()
        {
            financialAidAwardPeriodsController.Request.Headers.CacheControl = 
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            var expected = financialAidAwardPeriodCollection.FirstOrDefault();
            financialAidAwardPeriodServiceMock.Setup(x => x.GetFinancialAidAwardPeriodByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await financialAidAwardPeriodsController.GetFinancialAidAwardPeriodByIdAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidAwardPeriodController_GetFinancialAidAwardPeriod_PermissionsException()
        {
            financialAidAwardPeriodServiceMock.Setup(x => x.GetFinancialAidAwardPeriodsAsync(false)).Throws<PermissionsException>();
            await financialAidAwardPeriodsController.GetFinancialAidAwardPeriodsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidAwardPeriodController_GetFinancialAidAwardPeriod_KeyNotFoundException()
        {
            financialAidAwardPeriodServiceMock.Setup(x => x.GetFinancialAidAwardPeriodsAsync(false)).Throws<KeyNotFoundException>();
            await financialAidAwardPeriodsController.GetFinancialAidAwardPeriodsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidAwardPeriodController_GetFinancialAidAwardPeriod_ArgumentNullException()
        {
            financialAidAwardPeriodServiceMock.Setup(x => x.GetFinancialAidAwardPeriodsAsync(false)).Throws<ArgumentNullException>();
            await financialAidAwardPeriodsController.GetFinancialAidAwardPeriodsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidAwardPeriodController_GetFinancialAidAwardPeriod_RepositoryException()
        {
            financialAidAwardPeriodServiceMock.Setup(x => x.GetFinancialAidAwardPeriodsAsync(false)).Throws<RepositoryException>();
            await financialAidAwardPeriodsController.GetFinancialAidAwardPeriodsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidAwardPeriodController_GetFinancialAidAwardPeriod_IntgApiException()
        {
            financialAidAwardPeriodServiceMock.Setup(x => x.GetFinancialAidAwardPeriodsAsync(false)).Throws<IntegrationApiException>();
            await financialAidAwardPeriodsController.GetFinancialAidAwardPeriodsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidAwardPeriodController_GetFinancialAidAwardPeriod_Exception()
        {
            financialAidAwardPeriodServiceMock.Setup(x => x.GetFinancialAidAwardPeriodsAsync(false)).Throws<Exception>();
            await financialAidAwardPeriodsController.GetFinancialAidAwardPeriodsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidAwardPeriodController_GetFinancialAidAwardPeriodsByIdAsync_PermissionsException()
        {
            financialAidAwardPeriodServiceMock.Setup(x => x.GetFinancialAidAwardPeriodByGuidAsync(It.IsAny<string>())).Throws<PermissionsException>();
            await financialAidAwardPeriodsController.GetFinancialAidAwardPeriodByIdAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidAwardPeriodController_GetFinancialAidAwardPeriodsByIdAsync_KeyNotFoundException()
        {
            financialAidAwardPeriodServiceMock.Setup(x => x.GetFinancialAidAwardPeriodByGuidAsync(It.IsAny<string>())).Throws<KeyNotFoundException>();
            await financialAidAwardPeriodsController.GetFinancialAidAwardPeriodByIdAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidAwardPeriodController_GetFinancialAidAwardPeriodsByIdAsync_ArgumentNullException()
        {
            financialAidAwardPeriodServiceMock.Setup(x => x.GetFinancialAidAwardPeriodByGuidAsync(It.IsAny<string>())).Throws<ArgumentNullException>();
            await financialAidAwardPeriodsController.GetFinancialAidAwardPeriodByIdAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidAwardPeriodController_GetFinancialAidAwardPeriodsByIdAsync_RepositoryException()
        {
            financialAidAwardPeriodServiceMock.Setup(x => x.GetFinancialAidAwardPeriodByGuidAsync(It.IsAny<string>())).Throws<RepositoryException>();
            await financialAidAwardPeriodsController.GetFinancialAidAwardPeriodByIdAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidAwardPeriodController_GetFinancialAidAwardPeriodsByIdAsync_IntgApiException()
        {
            financialAidAwardPeriodServiceMock.Setup(x => x.GetFinancialAidAwardPeriodByGuidAsync(It.IsAny<string>())).Throws<IntegrationApiException>();
            await financialAidAwardPeriodsController.GetFinancialAidAwardPeriodByIdAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidAwardPeriodController_GetFinancialAidAwardPeriodsByIdAsync_Exception()
        {
            financialAidAwardPeriodServiceMock.Setup(x => x.GetFinancialAidAwardPeriodByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await financialAidAwardPeriodsController.GetFinancialAidAwardPeriodByIdAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidAwardPeriodController_PostFinancialAidAwardPeriodsAsync_Exception()
        {
            await financialAidAwardPeriodsController.PostFinancialAidAwardPeriodAsync(financialAidAwardPeriodCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidAwardPeriodController_PutFinancialAidAwardPeriodsAsync_Exception()
        {
            var sourceContext = financialAidAwardPeriodCollection.FirstOrDefault();
            await financialAidAwardPeriodsController.PutFinancialAidAwardPeriodAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidAwardPeriodController_DeleteFinancialAidAwardPeriodsAsync_Exception()
        {
            await financialAidAwardPeriodsController.DeleteFinancialAidAwardPeriodAsync(financialAidAwardPeriodCollection.FirstOrDefault().Id);
        }
    }
}