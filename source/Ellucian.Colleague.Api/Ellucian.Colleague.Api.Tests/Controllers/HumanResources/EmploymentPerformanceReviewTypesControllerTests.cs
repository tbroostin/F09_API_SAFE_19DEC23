//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers.HumanResources;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.HumanResources
{
    [TestClass]
    public class EmploymentPerformanceReviewTypesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IEmploymentPerformanceReviewTypesService> employmentPerformanceReviewTypesServiceMock;
        private Mock<ILogger> loggerMock;
        private EmploymentPerformanceReviewTypesController employmentPerformanceReviewTypesController;      
        private IEnumerable<Domain.HumanResources.Entities.EmploymentPerformanceReviewType> allEmploymentPerformanceReviewTypes;
        private List<Dtos.EmploymentPerformanceReviewTypes> employmentPerformanceReviewTypesCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize() 
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            employmentPerformanceReviewTypesServiceMock = new Mock<IEmploymentPerformanceReviewTypesService>();
            loggerMock = new Mock<ILogger>();
            employmentPerformanceReviewTypesCollection = new List<Dtos.EmploymentPerformanceReviewTypes>();

            allEmploymentPerformanceReviewTypes  = new List<Domain.HumanResources.Entities.EmploymentPerformanceReviewType>()
                {
                    new Domain.HumanResources.Entities.EmploymentPerformanceReviewType("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.HumanResources.Entities.EmploymentPerformanceReviewType("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.HumanResources.Entities.EmploymentPerformanceReviewType("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };
            
            foreach (var source in allEmploymentPerformanceReviewTypes)
            {
                var employmentPerformanceReviewTypes = new Ellucian.Colleague.Dtos.EmploymentPerformanceReviewTypes
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                employmentPerformanceReviewTypesCollection.Add(employmentPerformanceReviewTypes);
            }

            employmentPerformanceReviewTypesController = new EmploymentPerformanceReviewTypesController(employmentPerformanceReviewTypesServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            employmentPerformanceReviewTypesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            employmentPerformanceReviewTypesController = null;
            allEmploymentPerformanceReviewTypes = null;
            employmentPerformanceReviewTypesCollection = null;
            loggerMock = null;
            employmentPerformanceReviewTypesServiceMock = null;
        }

        [TestMethod]
        public async Task EmploymentPerformanceReviewTypesController_GetEmploymentPerformanceReviewTypes_ValidateFields_Nocache()
        {
            employmentPerformanceReviewTypesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            
            employmentPerformanceReviewTypesServiceMock.Setup(x => x.GetEmploymentPerformanceReviewTypesAsync(false)).ReturnsAsync(employmentPerformanceReviewTypesCollection);
       
            var sourceContexts = (await employmentPerformanceReviewTypesController.GetEmploymentPerformanceReviewTypesAsync()).ToList();
            Assert.AreEqual(employmentPerformanceReviewTypesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = employmentPerformanceReviewTypesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task EmploymentPerformanceReviewTypesController_GetEmploymentPerformanceReviewTypes_ValidateFields_Cache()
        {
            employmentPerformanceReviewTypesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue {NoCache = true};

            employmentPerformanceReviewTypesServiceMock.Setup(x => x.GetEmploymentPerformanceReviewTypesAsync(true)).ReturnsAsync(employmentPerformanceReviewTypesCollection);

            var sourceContexts = (await employmentPerformanceReviewTypesController.GetEmploymentPerformanceReviewTypesAsync()).ToList();
            Assert.AreEqual(employmentPerformanceReviewTypesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = employmentPerformanceReviewTypesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

         [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentPerformanceReviewTypesController_GetEmploymentPerformanceReviewTypes_KeyNotFoundException()
        {
            //
            employmentPerformanceReviewTypesServiceMock.Setup(x => x.GetEmploymentPerformanceReviewTypesAsync(false))
                .Throws<KeyNotFoundException>();
            await employmentPerformanceReviewTypesController.GetEmploymentPerformanceReviewTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentPerformanceReviewTypesController_GetEmploymentPerformanceReviewTypes_PermissionsException()
        {
            
            employmentPerformanceReviewTypesServiceMock.Setup(x => x.GetEmploymentPerformanceReviewTypesAsync(false))
                .Throws<PermissionsException>();
            await employmentPerformanceReviewTypesController.GetEmploymentPerformanceReviewTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentPerformanceReviewTypesController_GetEmploymentPerformanceReviewTypes_ArgumentException()
        {
            
            employmentPerformanceReviewTypesServiceMock.Setup(x => x.GetEmploymentPerformanceReviewTypesAsync(false))
                .Throws<ArgumentException>();
            await employmentPerformanceReviewTypesController.GetEmploymentPerformanceReviewTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentPerformanceReviewTypesController_GetEmploymentPerformanceReviewTypes_RepositoryException()
        {
            
            employmentPerformanceReviewTypesServiceMock.Setup(x => x.GetEmploymentPerformanceReviewTypesAsync(false))
                .Throws<RepositoryException>();
            await employmentPerformanceReviewTypesController.GetEmploymentPerformanceReviewTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentPerformanceReviewTypesController_GetEmploymentPerformanceReviewTypes_IntegrationApiException()
        {
            
            employmentPerformanceReviewTypesServiceMock.Setup(x => x.GetEmploymentPerformanceReviewTypesAsync(false))
                .Throws<IntegrationApiException>();
            await employmentPerformanceReviewTypesController.GetEmploymentPerformanceReviewTypesAsync();
        }

        [TestMethod]
        public async Task EmploymentPerformanceReviewTypesController_GetEmploymentPerformanceReviewTypesByGuidAsync_ValidateFields()
        {
            var expected = employmentPerformanceReviewTypesCollection.FirstOrDefault();
            employmentPerformanceReviewTypesServiceMock.Setup(x => x.GetEmploymentPerformanceReviewTypesByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await employmentPerformanceReviewTypesController.GetEmploymentPerformanceReviewTypesByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentPerformanceReviewTypesController_GetEmploymentPerformanceReviewTypes_Exception()
        {
            employmentPerformanceReviewTypesServiceMock.Setup(x => x.GetEmploymentPerformanceReviewTypesAsync(false)).Throws<Exception>();
            await employmentPerformanceReviewTypesController.GetEmploymentPerformanceReviewTypesAsync();       
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentPerformanceReviewTypesController_GetEmploymentPerformanceReviewTypesByGuidAsync_Exception()
        {
            employmentPerformanceReviewTypesServiceMock.Setup(x => x.GetEmploymentPerformanceReviewTypesByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await employmentPerformanceReviewTypesController.GetEmploymentPerformanceReviewTypesByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentPerformanceReviewTypesController_GetEmploymentPerformanceReviewTypesByGuid_KeyNotFoundException()
        {
            employmentPerformanceReviewTypesServiceMock.Setup(x => x.GetEmploymentPerformanceReviewTypesByGuidAsync(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();
            await employmentPerformanceReviewTypesController.GetEmploymentPerformanceReviewTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentPerformanceReviewTypesController_GetEmploymentPerformanceReviewTypesByGuid_PermissionsException()
        {
            employmentPerformanceReviewTypesServiceMock.Setup(x => x.GetEmploymentPerformanceReviewTypesByGuidAsync(It.IsAny<string>()))
                .Throws<PermissionsException>();
            await employmentPerformanceReviewTypesController.GetEmploymentPerformanceReviewTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task EmploymentPerformanceReviewTypesController_GetEmploymentPerformanceReviewTypesByGuid_ArgumentException()
        {
            employmentPerformanceReviewTypesServiceMock.Setup(x => x.GetEmploymentPerformanceReviewTypesByGuidAsync(It.IsAny<string>()))
                .Throws<ArgumentException>();
            await employmentPerformanceReviewTypesController.GetEmploymentPerformanceReviewTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentPerformanceReviewTypesController_GetEmploymentPerformanceReviewTypesByGuid_RepositoryException()
        {
            employmentPerformanceReviewTypesServiceMock.Setup(x => x.GetEmploymentPerformanceReviewTypesByGuidAsync(It.IsAny<string>()))
                .Throws<RepositoryException>();
            await employmentPerformanceReviewTypesController.GetEmploymentPerformanceReviewTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentPerformanceReviewTypesController_GetEmploymentPerformanceReviewTypesByGuid_IntegrationApiException()
        {
            employmentPerformanceReviewTypesServiceMock.Setup(x => x.GetEmploymentPerformanceReviewTypesByGuidAsync(It.IsAny<string>()))
                .Throws<IntegrationApiException>();
            await employmentPerformanceReviewTypesController.GetEmploymentPerformanceReviewTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentPerformanceReviewTypesController_GetEmploymentPerformanceReviewTypesByGuid_Exception()
        {
            employmentPerformanceReviewTypesServiceMock.Setup(x => x.GetEmploymentPerformanceReviewTypesByGuidAsync(It.IsAny<string>()))
                .Throws<Exception>();
            await employmentPerformanceReviewTypesController.GetEmploymentPerformanceReviewTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentPerformanceReviewTypesController_PostEmploymentPerformanceReviewTypesAsync_Exception()
        {
            await employmentPerformanceReviewTypesController.PostEmploymentPerformanceReviewTypesAsync(employmentPerformanceReviewTypesCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentPerformanceReviewTypesController_PutEmploymentPerformanceReviewTypesAsync_Exception()
        {
            var sourceContext = employmentPerformanceReviewTypesCollection.FirstOrDefault();
            await employmentPerformanceReviewTypesController.PutEmploymentPerformanceReviewTypesAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentPerformanceReviewTypesController_DeleteEmploymentPerformanceReviewTypesAsync_Exception()
        {
            await employmentPerformanceReviewTypesController.DeleteEmploymentPerformanceReviewTypesAsync(employmentPerformanceReviewTypesCollection.FirstOrDefault().Id);
        }
    }
}