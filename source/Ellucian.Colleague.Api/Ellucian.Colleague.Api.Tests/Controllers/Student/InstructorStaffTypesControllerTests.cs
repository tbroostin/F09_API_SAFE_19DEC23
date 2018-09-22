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
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class InstructorStaffTypesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IInstructorStaffTypesService> instructorStaffTypesServiceMock;
        private Mock<ILogger> loggerMock;
        private InstructorStaffTypesController instructorStaffTypesController;
        private IEnumerable<FacultyContractTypes> allFacultyContractTypes;
        private List<Dtos.InstructorStaffTypes> instructorStaffTypesCollection;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            instructorStaffTypesServiceMock = new Mock<IInstructorStaffTypesService>();
            loggerMock = new Mock<ILogger>();
            instructorStaffTypesCollection = new List<Dtos.InstructorStaffTypes>();

            allFacultyContractTypes = new List<FacultyContractTypes>()
                {
                    new FacultyContractTypes("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new FacultyContractTypes("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new FacultyContractTypes("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

            foreach (var source in allFacultyContractTypes)
            {
                var instructorStaffTypes = new Ellucian.Colleague.Dtos.InstructorStaffTypes
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                instructorStaffTypesCollection.Add(instructorStaffTypes);
            }

            instructorStaffTypesController = new InstructorStaffTypesController(instructorStaffTypesServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            instructorStaffTypesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            instructorStaffTypesController = null;
            allFacultyContractTypes = null;
            instructorStaffTypesCollection = null;
            loggerMock = null;
            instructorStaffTypesServiceMock = null;
        }

        [TestMethod]
        public async Task InstructorStaffTypesController_GetInstructorStaffTypes_ValidateFields_Nocache()
        {
            instructorStaffTypesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            instructorStaffTypesServiceMock.Setup(x => x.GetInstructorStaffTypesAsync(false)).ReturnsAsync(instructorStaffTypesCollection);

            var sourceContexts = (await instructorStaffTypesController.GetInstructorStaffTypesAsync()).ToList();
            Assert.AreEqual(instructorStaffTypesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = instructorStaffTypesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task InstructorStaffTypesController_GetInstructorStaffTypes_ValidateFields_Cache()
        {
            instructorStaffTypesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            instructorStaffTypesServiceMock.Setup(x => x.GetInstructorStaffTypesAsync(true)).ReturnsAsync(instructorStaffTypesCollection);

            var sourceContexts = (await instructorStaffTypesController.GetInstructorStaffTypesAsync()).ToList();
            Assert.AreEqual(instructorStaffTypesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = instructorStaffTypesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task InstructorStaffTypesController_GetInstructorStaffTypesByGuidAsync_ValidateFields()
        {
            var expected = instructorStaffTypesCollection.FirstOrDefault();
            instructorStaffTypesServiceMock.Setup(x => x.GetInstructorStaffTypesByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await instructorStaffTypesController.GetInstructorStaffTypesByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorStaffTypesController_GetInstructorStaffTypes_Exception()
        {
            instructorStaffTypesServiceMock.Setup(x => x.GetInstructorStaffTypesAsync(false)).Throws<Exception>();
            await instructorStaffTypesController.GetInstructorStaffTypesAsync();
        }

        //[TestMethod]
        //[ExpectedException(typeof(HttpResponseException))]
        //public async Task InstructorStaffTypesController_GetInstructorStaffTypes_Exception()
        //{
        //    instructorStaffTypesServiceMock.Setup(x => x.GetInstructorStaffTypesAsync(false)).Throws<Exception>();
        //    await instructorStaffTypesController.GetInstructorStaffTypesAsync();
        //}

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorStaffTypesController_GetInstructorStaffTypes_KeyNotFoundException()
        {
            instructorStaffTypesServiceMock.Setup(x => x.GetInstructorStaffTypesAsync(false)).Throws<KeyNotFoundException>();
            await instructorStaffTypesController.GetInstructorStaffTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorStaffTypesController_GetInstructorStaffTypes_PermissionsException()
        {
            instructorStaffTypesServiceMock.Setup(x => x.GetInstructorStaffTypesAsync(false)).Throws<PermissionsException>();
            await instructorStaffTypesController.GetInstructorStaffTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorStaffTypesController_GetInstructorStaffTypes_ArgumentException()
        {
            instructorStaffTypesServiceMock.Setup(x => x.GetInstructorStaffTypesAsync(false)).Throws<ArgumentException>();
            await instructorStaffTypesController.GetInstructorStaffTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorStaffTypesController_GetInstructorStaffTypes_RepositoryException()
        {
            instructorStaffTypesServiceMock.Setup(x => x.GetInstructorStaffTypesAsync(false)).Throws<RepositoryException>();
            await instructorStaffTypesController.GetInstructorStaffTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorStaffTypesController_GetInstructorStaffTypes_IntegrationApiException()
        {
            instructorStaffTypesServiceMock.Setup(x => x.GetInstructorStaffTypesAsync(false)).Throws<IntegrationApiException>();
            await instructorStaffTypesController.GetInstructorStaffTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorStaffTypesController_GetInstructorStaffTypesByGuidAsync_Empty_Guid()
        {
            instructorStaffTypesServiceMock.Setup(x => x.GetInstructorStaffTypesByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await instructorStaffTypesController.GetInstructorStaffTypesByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorStaffTypesController_GetInstructorStaffTypesByGuidAsync_KeyNotFoundException()
        {
            instructorStaffTypesServiceMock.Setup(x => x.GetInstructorStaffTypesByGuidAsync(It.IsAny<string>())).Throws<KeyNotFoundException>();
            await instructorStaffTypesController.GetInstructorStaffTypesByGuidAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorStaffTypesController_GetInstructorStaffTypesByGuidAsync_PermissionsException()
        {
            instructorStaffTypesServiceMock.Setup(x => x.GetInstructorStaffTypesByGuidAsync(It.IsAny<string>())).Throws<PermissionsException>();
            await instructorStaffTypesController.GetInstructorStaffTypesByGuidAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorStaffTypesController_GetInstructorStaffTypesByGuidAsync_ArgumentException()
        {
            instructorStaffTypesServiceMock.Setup(x => x.GetInstructorStaffTypesByGuidAsync(It.IsAny<string>())).Throws<ArgumentException>();
            await instructorStaffTypesController.GetInstructorStaffTypesByGuidAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorStaffTypesController_GetInstructorStaffTypesByGuidAsync_RepositoryException()
        {
            instructorStaffTypesServiceMock.Setup(x => x.GetInstructorStaffTypesByGuidAsync(It.IsAny<string>())).Throws<RepositoryException>();
            await instructorStaffTypesController.GetInstructorStaffTypesByGuidAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorStaffTypesController_GetInstructorStaffTypesByGuidAsync_IntegrationApiException()
        {
            instructorStaffTypesServiceMock.Setup(x => x.GetInstructorStaffTypesByGuidAsync(It.IsAny<string>())).Throws<IntegrationApiException>();
            await instructorStaffTypesController.GetInstructorStaffTypesByGuidAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorStaffTypesController_GetInstructorStaffTypesByGuidAsync_Exception()
        {
            instructorStaffTypesServiceMock.Setup(x => x.GetInstructorStaffTypesByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await instructorStaffTypesController.GetInstructorStaffTypesByGuidAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorStaffTypesController_PostInstructorStaffTypesAsync_Exception()
        {
            await instructorStaffTypesController.PostInstructorStaffTypesAsync(instructorStaffTypesCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorStaffTypesController_PutInstructorStaffTypesAsync_Exception()
        {
            var sourceContext = instructorStaffTypesCollection.FirstOrDefault();
            await instructorStaffTypesController.PutInstructorStaffTypesAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorStaffTypesController_DeleteInstructorStaffTypesAsync_Exception()
        {
            await instructorStaffTypesController.DeleteInstructorStaffTypesAsync(instructorStaffTypesCollection.FirstOrDefault().Id);
        }
    }
}