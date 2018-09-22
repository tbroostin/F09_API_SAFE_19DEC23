//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class AdvisorTypesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IAdapterRegistry> adapterRegistryMock;
        private Mock<IStudentReferenceDataRepository> referenceDataRepositoryMock;
        private Mock<IAdvisorTypesService> advisorTypesServiceMock;
        private Mock<ILogger> loggerMock;
        private AdvisorTypesController advisorTypesController;      
        private IEnumerable<AdvisorType> allAdvisorTypes;
        private List<Dtos.AdvisorTypes> advisorTypesCollection;

        [TestInitialize]
        public async void Initialize() 
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            referenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
            advisorTypesServiceMock = new Mock<IAdvisorTypesService>();
            loggerMock = new Mock<ILogger>();
            advisorTypesCollection = new List<Dtos.AdvisorTypes>();

            allAdvisorTypes  = new List<AdvisorType>()
                {
                    new AdvisorType("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic", "1"),
                    new AdvisorType("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic", "2"),
                    new AdvisorType("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural", "3")
                };
            
            foreach (var source in allAdvisorTypes)
            {
                var advisorTypes = new Ellucian.Colleague.Dtos.AdvisorTypes
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                advisorTypesCollection.Add(advisorTypes);
            }

            advisorTypesController = new AdvisorTypesController(adapterRegistryMock.Object, referenceDataRepositoryMock.Object, advisorTypesServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            advisorTypesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            advisorTypesController = null;
            allAdvisorTypes = null;
            advisorTypesCollection = null;
            loggerMock = null;
            advisorTypesServiceMock = null;
            referenceDataRepositoryMock = null;
            adapterRegistryMock = null;
        }

        [TestMethod]
        public async Task AdvisorTypesController_GetAdvisorTypes_ValidateFields_Nocache()
        {
            advisorTypesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            
            advisorTypesServiceMock.Setup(x => x.GetAdvisorTypesAsync(false)).ReturnsAsync(advisorTypesCollection);
       
            var sourceContexts = (await advisorTypesController.GetAdvisorTypesAsync()).ToList();
            Assert.AreEqual(advisorTypesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = advisorTypesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());

            }
        }

        [TestMethod]
        public async Task AdvisorTypesController_GetAdvisorTypes_ValidateFields_Cache()
        {
            advisorTypesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue {NoCache = true};

            advisorTypesServiceMock.Setup(x => x.GetAdvisorTypesAsync(true)).ReturnsAsync(advisorTypesCollection);

            var sourceContexts = (await advisorTypesController.GetAdvisorTypesAsync()).ToList();
            Assert.AreEqual(advisorTypesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = advisorTypesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task AdvisorTypesController_GetAdvisorTypesByGuidAsync_ValidateFields()
        {
            var expected = advisorTypesCollection.FirstOrDefault();
            advisorTypesServiceMock.Setup(x => x.GetAdvisorTypesByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await advisorTypesController.GetAdvisorTypesByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdvisorTypesController_GetAdvisorTypes_Exception()
        {
            advisorTypesServiceMock.Setup(x => x.GetAdvisorTypesAsync(false)).Throws<Exception>();
            await advisorTypesController.GetAdvisorTypesAsync();       
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdvisorTypesController_GetAdvisorTypesByGuidAsync_Exception()
        {
            advisorTypesServiceMock.Setup(x => x.GetAdvisorTypesByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await advisorTypesController.GetAdvisorTypesByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdvisorTypesController_PostAdvisorTypesAsync_Exception()
        {
            await advisorTypesController.PostAdvisorTypesAsync(advisorTypesCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdvisorTypesController_PutAdvisorTypesAsync_Exception()
        {
            var sourceContext = advisorTypesCollection.FirstOrDefault();
            await advisorTypesController.PutAdvisorTypesAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdvisorTypesController_DeleteAdvisorTypesAsync_Exception()
        {
            await advisorTypesController.DeleteAdvisorTypesAsync(advisorTypesCollection.FirstOrDefault().Id);
        }
    }
}