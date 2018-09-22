//Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Models;
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
    public class FinancialAidAcademicProgressStatusesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IFinancialAidAcademicProgressStatusesService> financialAidAcademicProgressStatusesServiceMock;
        private Mock<ILogger> loggerMock;
        private FinancialAidAcademicProgressStatusesController financialAidAcademicProgressStatusesController;      
        private IEnumerable<Domain.Student.Entities.SapStatuses> allSapStatuses;
        private List<Dtos.FinancialAidAcademicProgressStatuses> financialAidAcademicProgressStatusesCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize() 
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            financialAidAcademicProgressStatusesServiceMock = new Mock<IFinancialAidAcademicProgressStatusesService>();
            loggerMock = new Mock<ILogger>();
            financialAidAcademicProgressStatusesCollection = new List<Dtos.FinancialAidAcademicProgressStatuses>();

            allSapStatuses  = new List<Domain.Student.Entities.SapStatuses>()
                {
                    new Domain.Student.Entities.SapStatuses("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.Student.Entities.SapStatuses("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.Student.Entities.SapStatuses("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };
            
            foreach (var source in allSapStatuses)
            {
                var financialAidAcademicProgressStatuses = new Ellucian.Colleague.Dtos.FinancialAidAcademicProgressStatuses
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                financialAidAcademicProgressStatusesCollection.Add(financialAidAcademicProgressStatuses);
            }

            financialAidAcademicProgressStatusesController = new FinancialAidAcademicProgressStatusesController(financialAidAcademicProgressStatusesServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            financialAidAcademicProgressStatusesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            financialAidAcademicProgressStatusesController = null;
            allSapStatuses = null;
            financialAidAcademicProgressStatusesCollection = null;
            loggerMock = null;
            financialAidAcademicProgressStatusesServiceMock = null;
        }

        [TestMethod]
        public async Task FinancialAidAcademicProgressStatusesController_GetFinancialAidAcademicProgressStatuses_ValidateFields_Nocache()
        {
            financialAidAcademicProgressStatusesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            
            financialAidAcademicProgressStatusesServiceMock.Setup(x => x.GetFinancialAidAcademicProgressStatusesAsync(It.IsAny<Dtos.EnumProperties.RestrictedVisibility>(),false)).ReturnsAsync(financialAidAcademicProgressStatusesCollection);
       
            var sourceContexts = (await financialAidAcademicProgressStatusesController.GetFinancialAidAcademicProgressStatusesAsync(It.IsAny<QueryStringFilter>())).ToList();
            Assert.AreEqual(financialAidAcademicProgressStatusesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = financialAidAcademicProgressStatusesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task FinancialAidAcademicProgressStatusesController_GetFinancialAidAcademicProgressStatuses_ValidateFields_Cache()
        {
            financialAidAcademicProgressStatusesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue {NoCache = true};

            financialAidAcademicProgressStatusesServiceMock.Setup(x => x.GetFinancialAidAcademicProgressStatusesAsync(It.IsAny<Dtos.EnumProperties.RestrictedVisibility>(), true)).ReturnsAsync(financialAidAcademicProgressStatusesCollection);

            var sourceContexts = (await financialAidAcademicProgressStatusesController.GetFinancialAidAcademicProgressStatusesAsync(It.IsAny<QueryStringFilter>())).ToList();
            Assert.AreEqual(financialAidAcademicProgressStatusesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = financialAidAcademicProgressStatusesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

         [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidAcademicProgressStatusesController_GetFinancialAidAcademicProgressStatuses_KeyNotFoundException()
        {
            //
            financialAidAcademicProgressStatusesServiceMock.Setup(x => x.GetFinancialAidAcademicProgressStatusesAsync(It.IsAny<Dtos.EnumProperties.RestrictedVisibility>(), false))
                .Throws<KeyNotFoundException>();
            await financialAidAcademicProgressStatusesController.GetFinancialAidAcademicProgressStatusesAsync(It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidAcademicProgressStatusesController_GetFinancialAidAcademicProgressStatuses_PermissionsException()
        {
            
            financialAidAcademicProgressStatusesServiceMock.Setup(x => x.GetFinancialAidAcademicProgressStatusesAsync(It.IsAny<Dtos.EnumProperties.RestrictedVisibility>(), false))
                .Throws<PermissionsException>();
            await financialAidAcademicProgressStatusesController.GetFinancialAidAcademicProgressStatusesAsync(It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidAcademicProgressStatusesController_GetFinancialAidAcademicProgressStatuses_ArgumentException()
        {
            
            financialAidAcademicProgressStatusesServiceMock.Setup(x => x.GetFinancialAidAcademicProgressStatusesAsync(It.IsAny<Dtos.EnumProperties.RestrictedVisibility>(), false))
                .Throws<ArgumentException>();
            await financialAidAcademicProgressStatusesController.GetFinancialAidAcademicProgressStatusesAsync(It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidAcademicProgressStatusesController_GetFinancialAidAcademicProgressStatuses_RepositoryException()
        {
            
            financialAidAcademicProgressStatusesServiceMock.Setup(x => x.GetFinancialAidAcademicProgressStatusesAsync(It.IsAny<Dtos.EnumProperties.RestrictedVisibility>(), false))
                .Throws<RepositoryException>();
            await financialAidAcademicProgressStatusesController.GetFinancialAidAcademicProgressStatusesAsync(It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidAcademicProgressStatusesController_GetFinancialAidAcademicProgressStatuses_IntegrationApiException()
        {
            
            financialAidAcademicProgressStatusesServiceMock.Setup(x => x.GetFinancialAidAcademicProgressStatusesAsync(It.IsAny<Dtos.EnumProperties.RestrictedVisibility>(), false))
                .Throws<IntegrationApiException>();
            await financialAidAcademicProgressStatusesController.GetFinancialAidAcademicProgressStatusesAsync(It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        public async Task FinancialAidAcademicProgressStatusesController_GetFinancialAidAcademicProgressStatusesByGuidAsync_ValidateFields()
        {
            var expected = financialAidAcademicProgressStatusesCollection.FirstOrDefault();
            financialAidAcademicProgressStatusesServiceMock.Setup(x => x.GetFinancialAidAcademicProgressStatusesByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await financialAidAcademicProgressStatusesController.GetFinancialAidAcademicProgressStatusesByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidAcademicProgressStatusesController_GetFinancialAidAcademicProgressStatuses_Exception()
        {
            financialAidAcademicProgressStatusesServiceMock.Setup(x => x.GetFinancialAidAcademicProgressStatusesAsync(It.IsAny<Dtos.EnumProperties.RestrictedVisibility>(), false)).Throws<Exception>();
            await financialAidAcademicProgressStatusesController.GetFinancialAidAcademicProgressStatusesAsync(It.IsAny<QueryStringFilter>());       
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidAcademicProgressStatusesController_GetFinancialAidAcademicProgressStatusesByGuidAsync_Exception()
        {
            financialAidAcademicProgressStatusesServiceMock.Setup(x => x.GetFinancialAidAcademicProgressStatusesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await financialAidAcademicProgressStatusesController.GetFinancialAidAcademicProgressStatusesByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidAcademicProgressStatusesController_GetFinancialAidAcademicProgressStatusesByGuid_KeyNotFoundException()
        {
            financialAidAcademicProgressStatusesServiceMock.Setup(x => x.GetFinancialAidAcademicProgressStatusesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await financialAidAcademicProgressStatusesController.GetFinancialAidAcademicProgressStatusesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidAcademicProgressStatusesController_GetFinancialAidAcademicProgressStatusesByGuid_PermissionsException()
        {
            financialAidAcademicProgressStatusesServiceMock.Setup(x => x.GetFinancialAidAcademicProgressStatusesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await financialAidAcademicProgressStatusesController.GetFinancialAidAcademicProgressStatusesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task FinancialAidAcademicProgressStatusesController_GetFinancialAidAcademicProgressStatusesByGuid_ArgumentException()
        {
            financialAidAcademicProgressStatusesServiceMock.Setup(x => x.GetFinancialAidAcademicProgressStatusesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await financialAidAcademicProgressStatusesController.GetFinancialAidAcademicProgressStatusesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidAcademicProgressStatusesController_GetFinancialAidAcademicProgressStatusesByGuid_RepositoryException()
        {
            financialAidAcademicProgressStatusesServiceMock.Setup(x => x.GetFinancialAidAcademicProgressStatusesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await financialAidAcademicProgressStatusesController.GetFinancialAidAcademicProgressStatusesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidAcademicProgressStatusesController_GetFinancialAidAcademicProgressStatusesByGuid_IntegrationApiException()
        {
            financialAidAcademicProgressStatusesServiceMock.Setup(x => x.GetFinancialAidAcademicProgressStatusesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await financialAidAcademicProgressStatusesController.GetFinancialAidAcademicProgressStatusesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidAcademicProgressStatusesController_GetFinancialAidAcademicProgressStatusesByGuid_Exception()
        {
            financialAidAcademicProgressStatusesServiceMock.Setup(x => x.GetFinancialAidAcademicProgressStatusesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await financialAidAcademicProgressStatusesController.GetFinancialAidAcademicProgressStatusesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidAcademicProgressStatusesController_PostFinancialAidAcademicProgressStatusesAsync_Exception()
        {
            await financialAidAcademicProgressStatusesController.PostFinancialAidAcademicProgressStatusesAsync(financialAidAcademicProgressStatusesCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidAcademicProgressStatusesController_PutFinancialAidAcademicProgressStatusesAsync_Exception()
        {
            var sourceContext = financialAidAcademicProgressStatusesCollection.FirstOrDefault();
            await financialAidAcademicProgressStatusesController.PutFinancialAidAcademicProgressStatusesAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidAcademicProgressStatusesController_DeleteFinancialAidAcademicProgressStatusesAsync_Exception()
        {
            await financialAidAcademicProgressStatusesController.DeleteFinancialAidAcademicProgressStatusesAsync(financialAidAcademicProgressStatusesCollection.FirstOrDefault().Id);
        }
    }
}