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
    public class EmploymentFrequenciesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IEmploymentFrequenciesService> employmentFrequenciesServiceMock;
        private Mock<ILogger> loggerMock;
        private EmploymentFrequenciesController employmentFrequenciesController;      
        private IEnumerable<Domain.HumanResources.Entities.EmploymentFrequency> allTimeFrequencies;
        private List<Dtos.EmploymentFrequencies> employmentFrequenciesCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize() 
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            employmentFrequenciesServiceMock = new Mock<IEmploymentFrequenciesService>();
            loggerMock = new Mock<ILogger>();
            employmentFrequenciesCollection = new List<Dtos.EmploymentFrequencies>();

            allTimeFrequencies = new List<Domain.HumanResources.Entities.EmploymentFrequency>()
                {
                    new Domain.HumanResources.Entities.EmploymentFrequency("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic", "365"),
                    new Domain.HumanResources.Entities.EmploymentFrequency("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic", "24"),
                    new Domain.HumanResources.Entities.EmploymentFrequency("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural", "26")
                };
            
            foreach (var source in allTimeFrequencies)
            {
                var employmentFrequencies = new Ellucian.Colleague.Dtos.EmploymentFrequencies
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                employmentFrequenciesCollection.Add(employmentFrequencies);
            }

            employmentFrequenciesController = new EmploymentFrequenciesController(employmentFrequenciesServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            employmentFrequenciesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            employmentFrequenciesController = null;
            allTimeFrequencies = null;
            employmentFrequenciesCollection = null;
            loggerMock = null;
            employmentFrequenciesServiceMock = null;
        }

        [TestMethod]
        public async Task EmploymentFrequenciesController_GetEmploymentFrequencies_ValidateFields_Nocache()
        {
            employmentFrequenciesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            
            employmentFrequenciesServiceMock.Setup(x => x.GetEmploymentFrequenciesAsync(false)).ReturnsAsync(employmentFrequenciesCollection);
       
            var sourceContexts = (await employmentFrequenciesController.GetEmploymentFrequenciesAsync()).ToList();
            Assert.AreEqual(employmentFrequenciesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = employmentFrequenciesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task EmploymentFrequenciesController_GetEmploymentFrequencies_ValidateFields_Cache()
        {
            employmentFrequenciesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue {NoCache = true};

            employmentFrequenciesServiceMock.Setup(x => x.GetEmploymentFrequenciesAsync(true)).ReturnsAsync(employmentFrequenciesCollection);

            var sourceContexts = (await employmentFrequenciesController.GetEmploymentFrequenciesAsync()).ToList();
            Assert.AreEqual(employmentFrequenciesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = employmentFrequenciesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

         [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentFrequenciesController_GetEmploymentFrequencies_KeyNotFoundException()
        {
            //
            employmentFrequenciesServiceMock.Setup(x => x.GetEmploymentFrequenciesAsync(false))
                .Throws<KeyNotFoundException>();
            await employmentFrequenciesController.GetEmploymentFrequenciesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentFrequenciesController_GetEmploymentFrequencies_PermissionsException()
        {
            
            employmentFrequenciesServiceMock.Setup(x => x.GetEmploymentFrequenciesAsync(false))
                .Throws<PermissionsException>();
            await employmentFrequenciesController.GetEmploymentFrequenciesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentFrequenciesController_GetEmploymentFrequencies_ArgumentException()
        {
            
            employmentFrequenciesServiceMock.Setup(x => x.GetEmploymentFrequenciesAsync(false))
                .Throws<ArgumentException>();
            await employmentFrequenciesController.GetEmploymentFrequenciesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentFrequenciesController_GetEmploymentFrequencies_RepositoryException()
        {
            
            employmentFrequenciesServiceMock.Setup(x => x.GetEmploymentFrequenciesAsync(false))
                .Throws<RepositoryException>();
            await employmentFrequenciesController.GetEmploymentFrequenciesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentFrequenciesController_GetEmploymentFrequencies_IntegrationApiException()
        {
            
            employmentFrequenciesServiceMock.Setup(x => x.GetEmploymentFrequenciesAsync(false))
                .Throws<IntegrationApiException>();
            await employmentFrequenciesController.GetEmploymentFrequenciesAsync();
        }

        [TestMethod]
        public async Task EmploymentFrequenciesController_GetEmploymentFrequenciesByGuidAsync_ValidateFields()
        {
            var expected = employmentFrequenciesCollection.FirstOrDefault();
            employmentFrequenciesServiceMock.Setup(x => x.GetEmploymentFrequenciesByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await employmentFrequenciesController.GetEmploymentFrequenciesByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentFrequenciesController_GetEmploymentFrequencies_Exception()
        {
            employmentFrequenciesServiceMock.Setup(x => x.GetEmploymentFrequenciesAsync(false)).Throws<Exception>();
            await employmentFrequenciesController.GetEmploymentFrequenciesAsync();       
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentFrequenciesController_GetEmploymentFrequenciesByGuidAsync_Exception()
        {
            employmentFrequenciesServiceMock.Setup(x => x.GetEmploymentFrequenciesByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await employmentFrequenciesController.GetEmploymentFrequenciesByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentFrequenciesController_GetEmploymentFrequenciesByGuid_KeyNotFoundException()
        {
            employmentFrequenciesServiceMock.Setup(x => x.GetEmploymentFrequenciesByGuidAsync(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();
            await employmentFrequenciesController.GetEmploymentFrequenciesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentFrequenciesController_GetEmploymentFrequenciesByGuid_PermissionsException()
        {
            employmentFrequenciesServiceMock.Setup(x => x.GetEmploymentFrequenciesByGuidAsync(It.IsAny<string>()))
                .Throws<PermissionsException>();
            await employmentFrequenciesController.GetEmploymentFrequenciesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task EmploymentFrequenciesController_GetEmploymentFrequenciesByGuid_ArgumentException()
        {
            employmentFrequenciesServiceMock.Setup(x => x.GetEmploymentFrequenciesByGuidAsync(It.IsAny<string>()))
                .Throws<ArgumentException>();
            await employmentFrequenciesController.GetEmploymentFrequenciesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentFrequenciesController_GetEmploymentFrequenciesByGuid_RepositoryException()
        {
            employmentFrequenciesServiceMock.Setup(x => x.GetEmploymentFrequenciesByGuidAsync(It.IsAny<string>()))
                .Throws<RepositoryException>();
            await employmentFrequenciesController.GetEmploymentFrequenciesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentFrequenciesController_GetEmploymentFrequenciesByGuid_IntegrationApiException()
        {
            employmentFrequenciesServiceMock.Setup(x => x.GetEmploymentFrequenciesByGuidAsync(It.IsAny<string>()))
                .Throws<IntegrationApiException>();
            await employmentFrequenciesController.GetEmploymentFrequenciesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentFrequenciesController_GetEmploymentFrequenciesByGuid_Exception()
        {
            employmentFrequenciesServiceMock.Setup(x => x.GetEmploymentFrequenciesByGuidAsync(It.IsAny<string>()))
                .Throws<Exception>();
            await employmentFrequenciesController.GetEmploymentFrequenciesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentFrequenciesController_PostEmploymentFrequenciesAsync_Exception()
        {
            await employmentFrequenciesController.PostEmploymentFrequenciesAsync(employmentFrequenciesCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentFrequenciesController_PutEmploymentFrequenciesAsync_Exception()
        {
            var sourceContext = employmentFrequenciesCollection.FirstOrDefault();
            await employmentFrequenciesController.PutEmploymentFrequenciesAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentFrequenciesController_DeleteEmploymentFrequenciesAsync_Exception()
        {
            await employmentFrequenciesController.DeleteEmploymentFrequenciesAsync(employmentFrequenciesCollection.FirstOrDefault().Id);
        }
    }
}