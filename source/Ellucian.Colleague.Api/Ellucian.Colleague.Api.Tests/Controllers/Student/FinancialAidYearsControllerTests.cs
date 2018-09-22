// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Dtos.EnumProperties;
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
    public class FinancialAidYearsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IFinancialAidYearService> financialAidYearServiceMock;
        private Mock<ILogger> loggerMock;
        private FinancialAidYearsController financialAidYearsController;
        private IEnumerable<FinancialAidYear> allFinancialAidYear;
        private List<Dtos.FinancialAidYear> financialAidYearCollection;

        [TestInitialize]
        public async void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            financialAidYearServiceMock = new Mock<IFinancialAidYearService>();
            loggerMock = new Mock<ILogger>();
            financialAidYearCollection = new List<Dtos.FinancialAidYear>();

            allFinancialAidYear = (await new TestStudentReferenceDataRepository().GetFinancialAidYearsAsync(true)).ToList();

            foreach (var source in allFinancialAidYear)
            {
                var financialAidYear = new Ellucian.Colleague.Dtos.FinancialAidYear
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null,
                    Status = FinancialAidYearStatus.Active
                };
                financialAidYearCollection.Add(financialAidYear);
            }

            financialAidYearsController = new FinancialAidYearsController(financialAidYearServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            financialAidYearsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            financialAidYearsController = null;
            allFinancialAidYear = null;
            financialAidYearCollection = null;
            loggerMock = null;
            financialAidYearServiceMock = null;
        }

        [TestMethod]
        public async Task FinancialAidYearController_GetFinancialAidYear_ValidateFields_Nocache()
        {
            financialAidYearsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            financialAidYearServiceMock.Setup(x => x.GetFinancialAidYearsAsync(false)).ReturnsAsync(financialAidYearCollection);

            var sourceContexts = (await financialAidYearsController.GetFinancialAidYearsAsync()).ToList();
            Assert.AreEqual(financialAidYearCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = financialAidYearCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task FinancialAidYearController_GetFinancialAidYear_ValidateFields_Cache()
        {
            financialAidYearsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            financialAidYearServiceMock.Setup(x => x.GetFinancialAidYearsAsync(true)).ReturnsAsync(financialAidYearCollection);

            var sourceContexts = (await financialAidYearsController.GetFinancialAidYearsAsync()).ToList();
            Assert.AreEqual(financialAidYearCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = financialAidYearCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task FinancialAidYearController_GetFinancialAidYearsByIdAsync_ValidateFields()
        {
            financialAidYearsController.Request.Headers.CacheControl = 
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            var expected = financialAidYearCollection.FirstOrDefault();
            financialAidYearServiceMock.Setup(x => x.GetFinancialAidYearByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await financialAidYearsController.GetFinancialAidYearByIdAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidYearController_GetFinancialAidYear_PermissionsException()
        {
            financialAidYearServiceMock.Setup(x => x.GetFinancialAidYearsAsync(false)).Throws<PermissionsException>();
            await financialAidYearsController.GetFinancialAidYearsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidYearController_GetFinancialAidYear_KeyNotFoundException()
        {
            financialAidYearServiceMock.Setup(x => x.GetFinancialAidYearsAsync(false)).Throws<KeyNotFoundException>();
            await financialAidYearsController.GetFinancialAidYearsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidYearController_GetFinancialAidYear_ArgumentNullException()
        {
            financialAidYearServiceMock.Setup(x => x.GetFinancialAidYearsAsync(false)).Throws<ArgumentNullException>();
            await financialAidYearsController.GetFinancialAidYearsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidYearController_GetFinancialAidYear_RepositoryException()
        {
            financialAidYearServiceMock.Setup(x => x.GetFinancialAidYearsAsync(false)).Throws<RepositoryException>();
            await financialAidYearsController.GetFinancialAidYearsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidYearController_GetFinancialAidYear_IntgApiException()
        {
            financialAidYearServiceMock.Setup(x => x.GetFinancialAidYearsAsync(false)).Throws<IntegrationApiException>();
            await financialAidYearsController.GetFinancialAidYearsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidYearController_GetFinancialAidYear_Exception()
        {
            financialAidYearServiceMock.Setup(x => x.GetFinancialAidYearsAsync(false)).Throws<Exception>();
            await financialAidYearsController.GetFinancialAidYearsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidYearController_GetFinancialAidYearsByIdAsync_PermissionsException()
        {
            financialAidYearServiceMock.Setup(x => x.GetFinancialAidYearByGuidAsync(It.IsAny<string>())).Throws<PermissionsException>();
            await financialAidYearsController.GetFinancialAidYearByIdAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidYearController_GetFinancialAidYearsByIdAsync_KeyNotFoundException()
        {
            financialAidYearServiceMock.Setup(x => x.GetFinancialAidYearByGuidAsync(It.IsAny<string>())).Throws<KeyNotFoundException>();
            await financialAidYearsController.GetFinancialAidYearByIdAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidYearController_GetFinancialAidYearsByIdAsync_ArgumentNullException()
        {
            financialAidYearServiceMock.Setup(x => x.GetFinancialAidYearByGuidAsync(It.IsAny<string>())).Throws<ArgumentNullException>();
            await financialAidYearsController.GetFinancialAidYearByIdAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidYearController_GetFinancialAidYearsByIdAsync_RepositoryException()
        {
            financialAidYearServiceMock.Setup(x => x.GetFinancialAidYearByGuidAsync(It.IsAny<string>())).Throws<RepositoryException>();
            await financialAidYearsController.GetFinancialAidYearByIdAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidYearController_GetFinancialAidYearsByIdAsync_IntgApiException()
        {
            financialAidYearServiceMock.Setup(x => x.GetFinancialAidYearByGuidAsync(It.IsAny<string>())).Throws<IntegrationApiException>();
            await financialAidYearsController.GetFinancialAidYearByIdAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidYearController_GetFinancialAidYearsByIdAsync_Exception()
        {
            financialAidYearServiceMock.Setup(x => x.GetFinancialAidYearByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await financialAidYearsController.GetFinancialAidYearByIdAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidYearController_PostFinancialAidYearsAsync_Exception()
        {
            await financialAidYearsController.PostFinancialAidYearAsync(financialAidYearCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidYearController_PutFinancialAidYearsAsync_Exception()
        {
            var sourceContext = financialAidYearCollection.FirstOrDefault();
            await financialAidYearsController.PutFinancialAidYearAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidYearController_DeleteFinancialAidYearsAsync_Exception()
        {
            await financialAidYearsController.DeleteFinancialAidYearAsync(financialAidYearCollection.FirstOrDefault().Id);
        }
    }
}