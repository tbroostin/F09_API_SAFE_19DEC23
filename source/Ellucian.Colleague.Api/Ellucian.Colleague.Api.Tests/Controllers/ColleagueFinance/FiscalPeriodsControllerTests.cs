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
    public class FiscalPeriodsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IFiscalPeriodsService> fiscalPeriodsServiceMock;
        private Mock<ILogger> loggerMock;
        private FiscalPeriodsController fiscalPeriodsController;
        private IEnumerable<Domain.ColleagueFinance.Entities.FiscalPeriodsIntg> allFiscalPeriodsIntg;
        private List<Dtos.FiscalPeriods> fiscalPeriodsCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private string fiscalYearGuid = "8a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        private IEnumerable<Domain.ColleagueFinance.Entities.FiscalYear> allFiscalYears;
        private List<Dtos.FiscalYears> fiscalYearsCollection;
        private Ellucian.Web.Http.Models.QueryStringFilter criteriaFilter = new Web.Http.Models.QueryStringFilter("criteria", "");

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            fiscalPeriodsServiceMock = new Mock<IFiscalPeriodsService>();
            loggerMock = new Mock<ILogger>();
            fiscalPeriodsCollection = new List<Dtos.FiscalPeriods>();

            allFiscalPeriodsIntg = new List<Domain.ColleagueFinance.Entities.FiscalPeriodsIntg>()
                {
                    new Domain.ColleagueFinance.Entities.FiscalPeriodsIntg("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "2015")
                    { Status = "O" , Month = 7},
                    new Domain.ColleagueFinance.Entities.FiscalPeriodsIntg("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "2016")
                    { Status = "O" , Month = 7},
                    new Domain.ColleagueFinance.Entities.FiscalPeriodsIntg("d2253ac7-9931-4560-b42f-1fccd43c952e", "2017")
                    { Status = "O", Month = 7 },
                };

            foreach (var source in allFiscalPeriodsIntg)
            {
                var fiscalPeriods = new Ellucian.Colleague.Dtos.FiscalPeriods
                {
                    Id = source.Guid,
                    Status = Dtos.EnumProperties.FiscalPeriodsStatus.Open,
                    FiscalYear = new Dtos.GuidObject2(fiscalYearGuid)
                };

                fiscalPeriodsCollection.Add(fiscalPeriods);
            }

            fiscalPeriodsController = new FiscalPeriodsController(fiscalPeriodsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            fiscalPeriodsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            fiscalPeriodsController = null;
            allFiscalPeriodsIntg = null;
            fiscalPeriodsCollection = null;
            loggerMock = null;
            fiscalPeriodsServiceMock = null;
        }

        [TestMethod]
        public async Task FiscalPeriodsController_GetFiscalPeriods_ValidateFields_Nocache()
        {
            fiscalPeriodsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            fiscalPeriodsServiceMock.Setup(x => x.GetFiscalPeriodsAsync(false, It.IsAny<string>())).ReturnsAsync(fiscalPeriodsCollection);

            var sourceContexts = (await fiscalPeriodsController.GetFiscalPeriodsAsync(criteriaFilter)).ToList();
            Assert.AreEqual(fiscalPeriodsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = fiscalPeriodsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                
            }
        }

        [TestMethod]
        public async Task FiscalPeriodsController_GetFiscalPeriods_ValidateFields_Cache()
        {
            fiscalPeriodsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            fiscalPeriodsServiceMock.Setup(x => x.GetFiscalPeriodsAsync(true, It.IsAny<string>())).ReturnsAsync(fiscalPeriodsCollection);

            var sourceContexts = (await fiscalPeriodsController.GetFiscalPeriodsAsync(criteriaFilter)).ToList();
            Assert.AreEqual(fiscalPeriodsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = fiscalPeriodsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
             
            }
        }

        [TestMethod]
        public async Task FiscalPeriodsController_GetFiscalPeriods_ValidateFields_Filter()
        {
            var filterGroupName = "criteria";

            fiscalPeriodsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            fiscalPeriodsServiceMock.Setup(x => x.GetFiscalPeriodsAsync(true, It.IsAny<string>())).ReturnsAsync(fiscalPeriodsCollection.Where(x=>x.FiscalYear.Id == fiscalYearGuid));

            fiscalPeriodsController.Request.Properties.Add(
              string.Format("FilterObject{0}", filterGroupName),
              new Dtos.FiscalPeriods() { FiscalYear = new Dtos.GuidObject2(fiscalYearGuid) });

            var sourceContexts = (await fiscalPeriodsController.GetFiscalPeriodsAsync(criteriaFilter)).ToList();
            Assert.AreEqual(fiscalPeriodsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = fiscalPeriodsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
            }
        }


        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FiscalPeriodsController_GetFiscalPeriods_KeyNotFoundException()
        {
            fiscalPeriodsServiceMock.Setup(x => x.GetFiscalPeriodsAsync(false, It.IsAny<string>()))
                .Throws<KeyNotFoundException>();
            await fiscalPeriodsController.GetFiscalPeriodsAsync(criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FiscalPeriodsController_GetFiscalPeriods_PermissionsException()
        {

            fiscalPeriodsServiceMock.Setup(x => x.GetFiscalPeriodsAsync(false, It.IsAny<string>()))
                .Throws<PermissionsException>();
            await fiscalPeriodsController.GetFiscalPeriodsAsync(criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FiscalPeriodsController_GetFiscalPeriods_ArgumentException()
        {

            fiscalPeriodsServiceMock.Setup(x => x.GetFiscalPeriodsAsync(false, It.IsAny<string>()))
                .Throws<ArgumentException>();
            await fiscalPeriodsController.GetFiscalPeriodsAsync(criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FiscalPeriodsController_GetFiscalPeriods_RepositoryException()
        {

            fiscalPeriodsServiceMock.Setup(x => x.GetFiscalPeriodsAsync(false, It.IsAny<string>()))
                .Throws<RepositoryException>();
            await fiscalPeriodsController.GetFiscalPeriodsAsync(criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FiscalPeriodsController_GetFiscalPeriods_IntegrationApiException()
        {

            fiscalPeriodsServiceMock.Setup(x => x.GetFiscalPeriodsAsync(false, It.IsAny<string>()))
                .Throws<IntegrationApiException>();
            await fiscalPeriodsController.GetFiscalPeriodsAsync(criteriaFilter);
        }

        [TestMethod]
        public async Task FiscalPeriodsController_GetFiscalPeriodsByGuidAsync_ValidateFields()
        {
            var expected = fiscalPeriodsCollection.FirstOrDefault();
            fiscalPeriodsServiceMock.Setup(x => x.GetFiscalPeriodsByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await fiscalPeriodsController.GetFiscalPeriodsByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
           
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FiscalPeriodsController_GetFiscalPeriods_Exception()
        {
            fiscalPeriodsServiceMock.Setup(x => x.GetFiscalPeriodsAsync(false, It.IsAny<string>())).Throws<Exception>();
            await fiscalPeriodsController.GetFiscalPeriodsAsync(criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FiscalPeriodsController_GetFiscalPeriodsByGuidAsync_Exception()
        {
            fiscalPeriodsServiceMock.Setup(x => x.GetFiscalPeriodsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await fiscalPeriodsController.GetFiscalPeriodsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FiscalPeriodsController_GetFiscalPeriodsByGuid_KeyNotFoundException()
        {
            fiscalPeriodsServiceMock.Setup(x => x.GetFiscalPeriodsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await fiscalPeriodsController.GetFiscalPeriodsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FiscalPeriodsController_GetFiscalPeriodsByGuid_PermissionsException()
        {
            fiscalPeriodsServiceMock.Setup(x => x.GetFiscalPeriodsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await fiscalPeriodsController.GetFiscalPeriodsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FiscalPeriodsController_GetFiscalPeriodsByGuid_ArgumentException()
        {
            fiscalPeriodsServiceMock.Setup(x => x.GetFiscalPeriodsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await fiscalPeriodsController.GetFiscalPeriodsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FiscalPeriodsController_GetFiscalPeriodsByGuid_RepositoryException()
        {
            fiscalPeriodsServiceMock.Setup(x => x.GetFiscalPeriodsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await fiscalPeriodsController.GetFiscalPeriodsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FiscalPeriodsController_GetFiscalPeriodsByGuid_IntegrationApiException()
        {
            fiscalPeriodsServiceMock.Setup(x => x.GetFiscalPeriodsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await fiscalPeriodsController.GetFiscalPeriodsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FiscalPeriodsController_GetFiscalPeriodsByGuid_Exception()
        {
            fiscalPeriodsServiceMock.Setup(x => x.GetFiscalPeriodsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await fiscalPeriodsController.GetFiscalPeriodsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FiscalPeriodsController_PostFiscalPeriodsAsync_Exception()
        {
            await fiscalPeriodsController.PostFiscalPeriodsAsync(fiscalPeriodsCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FiscalPeriodsController_PutFiscalPeriodsAsync_Exception()
        {
            var sourceContext = fiscalPeriodsCollection.FirstOrDefault();
            await fiscalPeriodsController.PutFiscalPeriodsAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FiscalPeriodsController_DeleteFiscalPeriodsAsync_Exception()
        {
            await fiscalPeriodsController.DeleteFiscalPeriodsAsync(fiscalPeriodsCollection.FirstOrDefault().Id);
        }
    }
}