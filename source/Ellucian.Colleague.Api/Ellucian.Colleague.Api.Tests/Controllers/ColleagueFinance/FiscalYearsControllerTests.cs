//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Threading.Tasks;
using System.Collections.Generic;
using Ellucian.Colleague.Configuration.Licensing;
using System.Net.Http;
using System.Web.Http.Hosting;
using System.Web.Http;
using Ellucian.Colleague.Api.Controllers.ColleagueFinance;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Api.Tests.Controllers.ColleagueFinance
{
    [TestClass]
    public class FiscalYearsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IFiscalYearsService> fiscalYearsServiceMock;
        private Mock<ICostCenterService> costCenterServiceMock;
        private Mock<ILogger> loggerMock;
        private FiscalYearsController fiscalYearsController;
        private IEnumerable<Domain.ColleagueFinance.Entities.FiscalYear> allFiscalYear;
        private List<Dtos.FiscalYears> fiscalYearsCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private Ellucian.Web.Http.Models.QueryStringFilter criteriaFilter = new Web.Http.Models.QueryStringFilter("criteria", "");

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            costCenterServiceMock = new Mock<ICostCenterService>();
            fiscalYearsServiceMock = new Mock<IFiscalYearsService>();
            loggerMock = new Mock<ILogger>();
            fiscalYearsCollection = new List<Dtos.FiscalYears>();

            allFiscalYear = new List<Domain.ColleagueFinance.Entities.FiscalYear>()
                {
                    new Domain.ColleagueFinance.Entities.FiscalYear("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "2015")
                    { Title = "Fiscal Year: 2015", Status = "O", InstitutionName = "Ellucian University"},
                    new Domain.ColleagueFinance.Entities.FiscalYear("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "2016")
                    { Title = "Fiscal Year: 2016", Status = "O", InstitutionName = "Ellucian University"},
                    new Domain.ColleagueFinance.Entities.FiscalYear("d2253ac7-9931-4560-b42f-1fccd43c952e", "2017")
                    { Title = "Fiscal Year: 2017", Status = "O", InstitutionName = "Ellucian University"}
                };

            foreach (var source in allFiscalYear)
            {
                var fiscalYears = new Ellucian.Colleague.Dtos.FiscalYears
                {
                    Id = source.Guid,                   
                    Title = source.Title,
                    Status = Dtos.EnumProperties.FiscalPeriodsStatus.Open,
                    NumberOfPeriods = 12,
                    ReportingSegment = source.InstitutionName,
                };
                fiscalYearsCollection.Add(fiscalYears);
            }

            fiscalYearsController = new FiscalYearsController(costCenterServiceMock.Object, fiscalYearsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            fiscalYearsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            costCenterServiceMock = null;
            fiscalYearsController = null;
            allFiscalYear = null;
            fiscalYearsCollection = null;
            loggerMock = null;
            fiscalYearsServiceMock = null;
        }

        [TestMethod]
        public async Task FiscalYearsController_GetFiscalYears_ValidateFields_Nocache()
        {
            fiscalYearsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            fiscalYearsServiceMock.Setup(x => x.GetFiscalYearsAsync(false, It.IsAny<string>())).ReturnsAsync(fiscalYearsCollection);

            var sourceContexts = (await fiscalYearsController.GetFiscalYearsAsync(criteriaFilter)).ToList();
            Assert.AreEqual(fiscalYearsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = fiscalYearsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.ReportingSegment, actual.ReportingSegment, "ReportingSegment, Index=" + i.ToString());
                Assert.AreEqual(expected.NumberOfPeriods, 12, "NumberOfPeriods, Index=" + i.ToString());
                Assert.AreEqual(expected.Status, Dtos.EnumProperties.FiscalPeriodsStatus.Open, "Status, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task FiscalYearsController_GetFiscalYears_ValidateFields_Cache()
        {
            fiscalYearsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            fiscalYearsServiceMock.Setup(x => x.GetFiscalYearsAsync(true, It.IsAny<string>())).ReturnsAsync(fiscalYearsCollection);

            var sourceContexts = (await fiscalYearsController.GetFiscalYearsAsync(criteriaFilter)).ToList();
            Assert.AreEqual(fiscalYearsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = fiscalYearsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FiscalYearsController_GetFiscalYears_KeyNotFoundException()
        {
            //
            fiscalYearsServiceMock.Setup(x => x.GetFiscalYearsAsync(false, It.IsAny<string>()))
                .Throws<KeyNotFoundException>();
            await fiscalYearsController.GetFiscalYearsAsync(criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FiscalYearsController_GetFiscalYears_PermissionsException()
        {

            fiscalYearsServiceMock.Setup(x => x.GetFiscalYearsAsync(false, It.IsAny<string>()))
                .Throws<PermissionsException>();
            await fiscalYearsController.GetFiscalYearsAsync(criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FiscalYearsController_GetFiscalYears_ArgumentException()
        {

            fiscalYearsServiceMock.Setup(x => x.GetFiscalYearsAsync(false, It.IsAny<string>()))
                .Throws<ArgumentException>();
            await fiscalYearsController.GetFiscalYearsAsync(criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FiscalYearsController_GetFiscalYears_RepositoryException()
        {

            fiscalYearsServiceMock.Setup(x => x.GetFiscalYearsAsync(false, It.IsAny<string>()))
                .Throws<RepositoryException>();
            await fiscalYearsController.GetFiscalYearsAsync(criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FiscalYearsController_GetFiscalYears_IntegrationApiException()
        {

            fiscalYearsServiceMock.Setup(x => x.GetFiscalYearsAsync(false, It.IsAny<string>()))
                .Throws<IntegrationApiException>();
            await fiscalYearsController.GetFiscalYearsAsync(criteriaFilter);
        }

        [TestMethod]
        public async Task FiscalYearsController_GetFiscalYearsByGuidAsync_ValidateFields()
        {
            var expected = fiscalYearsCollection.FirstOrDefault();
            fiscalYearsServiceMock.Setup(x => x.GetFiscalYearsByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await fiscalYearsController.GetFiscalYearsByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FiscalYearsController_GetFiscalYears_Exception()
        {
            fiscalYearsServiceMock.Setup(x => x.GetFiscalYearsAsync(false, It.IsAny<string>())).Throws<Exception>();
            await fiscalYearsController.GetFiscalYearsAsync(criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FiscalYearsController_GetFiscalYearsByGuidAsync_Exception()
        {
            fiscalYearsServiceMock.Setup(x => x.GetFiscalYearsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await fiscalYearsController.GetFiscalYearsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FiscalYearsController_GetFiscalYearsByGuid_KeyNotFoundException()
        {
            fiscalYearsServiceMock.Setup(x => x.GetFiscalYearsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await fiscalYearsController.GetFiscalYearsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FiscalYearsController_GetFiscalYearsByGuid_PermissionsException()
        {
            fiscalYearsServiceMock.Setup(x => x.GetFiscalYearsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await fiscalYearsController.GetFiscalYearsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FiscalYearsController_GetFiscalYearsByGuid_ArgumentException()
        {
            fiscalYearsServiceMock.Setup(x => x.GetFiscalYearsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await fiscalYearsController.GetFiscalYearsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FiscalYearsController_GetFiscalYearsByGuid_RepositoryException()
        {
            fiscalYearsServiceMock.Setup(x => x.GetFiscalYearsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await fiscalYearsController.GetFiscalYearsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FiscalYearsController_GetFiscalYearsByGuid_IntegrationApiException()
        {
            fiscalYearsServiceMock.Setup(x => x.GetFiscalYearsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await fiscalYearsController.GetFiscalYearsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FiscalYearsController_GetFiscalYearsByGuid_Exception()
        {
            fiscalYearsServiceMock.Setup(x => x.GetFiscalYearsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await fiscalYearsController.GetFiscalYearsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FiscalYearsController_PostFiscalYearsAsync_Exception()
        {
            await fiscalYearsController.PostFiscalYearsAsync(fiscalYearsCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FiscalYearsController_PutFiscalYearsAsync_Exception()
        {
            var sourceContext = fiscalYearsCollection.FirstOrDefault();
            await fiscalYearsController.PutFiscalYearsAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FiscalYearsController_DeleteFiscalYearsAsync_Exception()
        {
            await fiscalYearsController.DeleteFiscalYearsAsync(fiscalYearsCollection.FirstOrDefault().Id);
        }
    }
}