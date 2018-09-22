//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class AdmissionResidencyTypesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IAdmissionResidencyTypesService> admissionResidencyTypesServiceMock;
        private Mock<ILogger> loggerMock;
        private AdmissionResidencyTypesController admissionResidencyTypesController;      
        private IEnumerable<AdmissionResidencyType> allResidencyStatuses;
        private List<Dtos.AdmissionResidencyTypes> admissionResidencyTypesCollection;

        [TestInitialize]
        public async void Initialize() 
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            admissionResidencyTypesServiceMock = new Mock<IAdmissionResidencyTypesService>();
            loggerMock = new Mock<ILogger>();
            admissionResidencyTypesCollection = new List<Dtos.AdmissionResidencyTypes>();

            allResidencyStatuses = new List<AdmissionResidencyType>()
                {
                    new AdmissionResidencyType("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new AdmissionResidencyType("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new AdmissionResidencyType("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };
            
            foreach (var source in allResidencyStatuses)
            {
                var admissionResidencyTypes = new Ellucian.Colleague.Dtos.AdmissionResidencyTypes
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                admissionResidencyTypesCollection.Add(admissionResidencyTypes);
            }

            admissionResidencyTypesController = new AdmissionResidencyTypesController(admissionResidencyTypesServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            admissionResidencyTypesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            admissionResidencyTypesController = null;
            allResidencyStatuses = null;
            admissionResidencyTypesCollection = null;
            loggerMock = null;
            admissionResidencyTypesServiceMock = null;
        }

        [TestMethod]
        public async Task AdmissionResidencyTypesController_GetAdmissionResidencyTypes_ValidateFields_Nocache()
        {
            admissionResidencyTypesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            
            admissionResidencyTypesServiceMock.Setup(x => x.GetAdmissionResidencyTypesAsync(false)).ReturnsAsync(admissionResidencyTypesCollection);
       
            var sourceContexts = (await admissionResidencyTypesController.GetAdmissionResidencyTypesAsync()).ToList();
            Assert.AreEqual(admissionResidencyTypesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = admissionResidencyTypesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task AdmissionResidencyTypesController_GetAdmissionResidencyTypes_ValidateFields_Cache()
        {
            admissionResidencyTypesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue {NoCache = true};

            admissionResidencyTypesServiceMock.Setup(x => x.GetAdmissionResidencyTypesAsync(true)).ReturnsAsync(admissionResidencyTypesCollection);

            var sourceContexts = (await admissionResidencyTypesController.GetAdmissionResidencyTypesAsync()).ToList();
            Assert.AreEqual(admissionResidencyTypesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = admissionResidencyTypesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task AdmissionResidencyTypesController_GetAdmissionResidencyTypesByGuidAsync_ValidateFields()
        {
            var expected = admissionResidencyTypesCollection.FirstOrDefault();
            admissionResidencyTypesServiceMock.Setup(x => x.GetAdmissionResidencyTypesByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await admissionResidencyTypesController.GetAdmissionResidencyTypesByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionResidencyTypesController_GetAdmissionResidencyTypes_Exception()
        {
            admissionResidencyTypesServiceMock.Setup(x => x.GetAdmissionResidencyTypesAsync(false)).Throws<Exception>();
            await admissionResidencyTypesController.GetAdmissionResidencyTypesAsync();       
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionResidencyTypesController_GetAdmissionResidencyTypesByGuidAsync_Exception()
        {
            admissionResidencyTypesServiceMock.Setup(x => x.GetAdmissionResidencyTypesByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await admissionResidencyTypesController.GetAdmissionResidencyTypesByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionResidencyTypesController_PostAdmissionResidencyTypesAsync_Exception()
        {
            await admissionResidencyTypesController.PostAdmissionResidencyTypesAsync(admissionResidencyTypesCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionResidencyTypesController_PutAdmissionResidencyTypesAsync_Exception()
        {
            var sourceContext = admissionResidencyTypesCollection.FirstOrDefault();
            await admissionResidencyTypesController.PutAdmissionResidencyTypesAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionResidencyTypesController_DeleteAdmissionResidencyTypesAsync_Exception()
        {
            await admissionResidencyTypesController.DeleteAdmissionResidencyTypesAsync(admissionResidencyTypesCollection.FirstOrDefault().Id);
        }
    }
}