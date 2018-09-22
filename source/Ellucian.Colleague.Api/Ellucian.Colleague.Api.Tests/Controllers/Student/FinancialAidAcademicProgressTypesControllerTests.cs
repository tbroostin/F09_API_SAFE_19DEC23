//Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
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
    public class FinancialAidAcademicProgressTypesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IFinancialAidAcademicProgressTypesService> financialAidAcademicProgressTypesServiceMock;
        private Mock<ILogger> loggerMock;
        private FinancialAidAcademicProgressTypesController financialAidAcademicProgressTypesController;      
        private IEnumerable<Domain.Student.Entities.SapType> allSaptype;
        private List<Dtos.FinancialAidAcademicProgressTypes> financialAidAcademicProgressTypesCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize() 
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            financialAidAcademicProgressTypesServiceMock = new Mock<IFinancialAidAcademicProgressTypesService>();
            loggerMock = new Mock<ILogger>();
            financialAidAcademicProgressTypesCollection = new List<Dtos.FinancialAidAcademicProgressTypes>();

            allSaptype  = new List<Domain.Student.Entities.SapType>()
                {
                    new Domain.Student.Entities.SapType("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.Student.Entities.SapType("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.Student.Entities.SapType("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };
            
            foreach (var source in allSaptype)
            {
                var financialAidAcademicProgressTypes = new Ellucian.Colleague.Dtos.FinancialAidAcademicProgressTypes
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                financialAidAcademicProgressTypesCollection.Add(financialAidAcademicProgressTypes);
            }

            financialAidAcademicProgressTypesController = new FinancialAidAcademicProgressTypesController(financialAidAcademicProgressTypesServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            financialAidAcademicProgressTypesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            financialAidAcademicProgressTypesController = null;
            allSaptype = null;
            financialAidAcademicProgressTypesCollection = null;
            loggerMock = null;
            financialAidAcademicProgressTypesServiceMock = null;
        }

        [TestMethod]
        public async Task FinancialAidAcademicProgressTypesController_GetFinancialAidAcademicProgressTypes_ValidateFields_Nocache()
        {
            financialAidAcademicProgressTypesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            
            financialAidAcademicProgressTypesServiceMock.Setup(x => x.GetFinancialAidAcademicProgressTypesAsync(false)).ReturnsAsync(financialAidAcademicProgressTypesCollection);
       
            var sourceContexts = (await financialAidAcademicProgressTypesController.GetFinancialAidAcademicProgressTypesAsync()).ToList();
            Assert.AreEqual(financialAidAcademicProgressTypesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = financialAidAcademicProgressTypesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task FinancialAidAcademicProgressTypesController_GetFinancialAidAcademicProgressTypes_ValidateFields_Cache()
        {
            financialAidAcademicProgressTypesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue {NoCache = true};

            financialAidAcademicProgressTypesServiceMock.Setup(x => x.GetFinancialAidAcademicProgressTypesAsync(true)).ReturnsAsync(financialAidAcademicProgressTypesCollection);

            var sourceContexts = (await financialAidAcademicProgressTypesController.GetFinancialAidAcademicProgressTypesAsync()).ToList();
            Assert.AreEqual(financialAidAcademicProgressTypesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = financialAidAcademicProgressTypesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

         [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidAcademicProgressTypesController_GetFinancialAidAcademicProgressTypes_KeyNotFoundException()
        {
            //
            financialAidAcademicProgressTypesServiceMock.Setup(x => x.GetFinancialAidAcademicProgressTypesAsync(false))
                .Throws<KeyNotFoundException>();
            await financialAidAcademicProgressTypesController.GetFinancialAidAcademicProgressTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidAcademicProgressTypesController_GetFinancialAidAcademicProgressTypes_PermissionsException()
        {
            
            financialAidAcademicProgressTypesServiceMock.Setup(x => x.GetFinancialAidAcademicProgressTypesAsync(false))
                .Throws<PermissionsException>();
            await financialAidAcademicProgressTypesController.GetFinancialAidAcademicProgressTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidAcademicProgressTypesController_GetFinancialAidAcademicProgressTypes_ArgumentException()
        {
            
            financialAidAcademicProgressTypesServiceMock.Setup(x => x.GetFinancialAidAcademicProgressTypesAsync(false))
                .Throws<ArgumentException>();
            await financialAidAcademicProgressTypesController.GetFinancialAidAcademicProgressTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidAcademicProgressTypesController_GetFinancialAidAcademicProgressTypes_RepositoryException()
        {
            
            financialAidAcademicProgressTypesServiceMock.Setup(x => x.GetFinancialAidAcademicProgressTypesAsync(false))
                .Throws<RepositoryException>();
            await financialAidAcademicProgressTypesController.GetFinancialAidAcademicProgressTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidAcademicProgressTypesController_GetFinancialAidAcademicProgressTypes_IntegrationApiException()
        {
            
            financialAidAcademicProgressTypesServiceMock.Setup(x => x.GetFinancialAidAcademicProgressTypesAsync(false))
                .Throws<IntegrationApiException>();
            await financialAidAcademicProgressTypesController.GetFinancialAidAcademicProgressTypesAsync();
        }

        [TestMethod]
        public async Task FinancialAidAcademicProgressTypesController_GetFinancialAidAcademicProgressTypesByGuidAsync_ValidateFields()
        {
            var expected = financialAidAcademicProgressTypesCollection.FirstOrDefault();
            financialAidAcademicProgressTypesServiceMock.Setup(x => x.GetFinancialAidAcademicProgressTypesByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await financialAidAcademicProgressTypesController.GetFinancialAidAcademicProgressTypesByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        public async Task FinancialAidAcademicProgressTypesController_GetFinancialAidAcademicProgressTypesByGuidAsync_ValidateFields_CacheTrue()
        {
            financialAidAcademicProgressTypesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
            var expected = financialAidAcademicProgressTypesCollection.FirstOrDefault();
            financialAidAcademicProgressTypesServiceMock.Setup(x => x.GetFinancialAidAcademicProgressTypesByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await financialAidAcademicProgressTypesController.GetFinancialAidAcademicProgressTypesByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidAcademicProgressTypesController_GetFinancialAidAcademicProgressTypes_Exception()
        {
            financialAidAcademicProgressTypesServiceMock.Setup(x => x.GetFinancialAidAcademicProgressTypesAsync(false)).Throws<Exception>();
            await financialAidAcademicProgressTypesController.GetFinancialAidAcademicProgressTypesAsync();       
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidAcademicProgressTypesController_GetFinancialAidAcademicProgressTypesByGuidAsync_Exception()
        {
            financialAidAcademicProgressTypesServiceMock.Setup(x => x.GetFinancialAidAcademicProgressTypesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await financialAidAcademicProgressTypesController.GetFinancialAidAcademicProgressTypesByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidAcademicProgressTypesController_GetFinancialAidAcademicProgressTypesByGuid_KeyNotFoundException()
        {
            financialAidAcademicProgressTypesServiceMock.Setup(x => x.GetFinancialAidAcademicProgressTypesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await financialAidAcademicProgressTypesController.GetFinancialAidAcademicProgressTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidAcademicProgressTypesController_GetFinancialAidAcademicProgressTypesByGuid_PermissionsException()
        {
            financialAidAcademicProgressTypesServiceMock.Setup(x => x.GetFinancialAidAcademicProgressTypesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await financialAidAcademicProgressTypesController.GetFinancialAidAcademicProgressTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task FinancialAidAcademicProgressTypesController_GetFinancialAidAcademicProgressTypesByGuid_ArgumentException()
        {
            financialAidAcademicProgressTypesServiceMock.Setup(x => x.GetFinancialAidAcademicProgressTypesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await financialAidAcademicProgressTypesController.GetFinancialAidAcademicProgressTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidAcademicProgressTypesController_GetFinancialAidAcademicProgressTypesByGuid_RepositoryException()
        {
            financialAidAcademicProgressTypesServiceMock.Setup(x => x.GetFinancialAidAcademicProgressTypesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await financialAidAcademicProgressTypesController.GetFinancialAidAcademicProgressTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidAcademicProgressTypesController_GetFinancialAidAcademicProgressTypesByGuid_IntegrationApiException()
        {
            financialAidAcademicProgressTypesServiceMock.Setup(x => x.GetFinancialAidAcademicProgressTypesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await financialAidAcademicProgressTypesController.GetFinancialAidAcademicProgressTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidAcademicProgressTypesController_GetFinancialAidAcademicProgressTypesByGuid_Exception()
        {
            financialAidAcademicProgressTypesServiceMock.Setup(x => x.GetFinancialAidAcademicProgressTypesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await financialAidAcademicProgressTypesController.GetFinancialAidAcademicProgressTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidAcademicProgressTypesController_PostFinancialAidAcademicProgressTypesAsync_Exception()
        {
            await financialAidAcademicProgressTypesController.PostFinancialAidAcademicProgressTypesAsync(financialAidAcademicProgressTypesCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidAcademicProgressTypesController_PutFinancialAidAcademicProgressTypesAsync_Exception()
        {
            var sourceContext = financialAidAcademicProgressTypesCollection.FirstOrDefault();
            await financialAidAcademicProgressTypesController.PutFinancialAidAcademicProgressTypesAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidAcademicProgressTypesController_DeleteFinancialAidAcademicProgressTypesAsync_Exception()
        {
            await financialAidAcademicProgressTypesController.DeleteFinancialAidAcademicProgressTypesAsync(financialAidAcademicProgressTypesCollection.FirstOrDefault().Id);
        }
    }
}