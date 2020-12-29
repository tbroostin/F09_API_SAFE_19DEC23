//Copyright 2020 Ellucian Company L.P. and its affiliates.

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
    public class EducationalGoalsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IEducationalGoalsService> educationalGoalsServiceMock;
        private Mock<ILogger> loggerMock;
        private EducationalGoalsController educationalGoalsController;      
        private IEnumerable<Domain.Student.Entities.EducationGoals> allEducationGoals;
        private List<Dtos.EducationalGoals> educationalGoalsCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize() 
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            educationalGoalsServiceMock = new Mock<IEducationalGoalsService>();
            loggerMock = new Mock<ILogger>();
            educationalGoalsCollection = new List<Dtos.EducationalGoals>();

            allEducationGoals  = new List<Domain.Student.Entities.EducationGoals>()
                {
                    new Domain.Student.Entities.EducationGoals("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.Student.Entities.EducationGoals("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.Student.Entities.EducationGoals("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };
            
            foreach (var source in allEducationGoals)
            {
                var educationalGoals = new Ellucian.Colleague.Dtos.EducationalGoals
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                educationalGoalsCollection.Add(educationalGoals);
            }

            educationalGoalsController = new EducationalGoalsController(educationalGoalsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            educationalGoalsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            educationalGoalsController = null;
            allEducationGoals = null;
            educationalGoalsCollection = null;
            loggerMock = null;
            educationalGoalsServiceMock = null;
        }

        [TestMethod]
        public async Task EducationalGoalsController_GetEducationalGoals_ValidateFields_Nocache()
        {
            educationalGoalsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            
            educationalGoalsServiceMock.Setup(x => x.GetEducationalGoalsAsync(false)).ReturnsAsync(educationalGoalsCollection);
       
            var sourceContexts = (await educationalGoalsController.GetEducationalGoalsAsync()).ToList();
            Assert.AreEqual(educationalGoalsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = educationalGoalsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task EducationalGoalsController_GetEducationalGoals_ValidateFields_Cache()
        {
            educationalGoalsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue {NoCache = true};

            educationalGoalsServiceMock.Setup(x => x.GetEducationalGoalsAsync(true)).ReturnsAsync(educationalGoalsCollection);

            var sourceContexts = (await educationalGoalsController.GetEducationalGoalsAsync()).ToList();
            Assert.AreEqual(educationalGoalsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = educationalGoalsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

         [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalGoalsController_GetEducationalGoals_KeyNotFoundException()
        {
            //
            educationalGoalsServiceMock.Setup(x => x.GetEducationalGoalsAsync(false))
                .Throws<KeyNotFoundException>();
            await educationalGoalsController.GetEducationalGoalsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalGoalsController_GetEducationalGoals_PermissionsException()
        {
            
            educationalGoalsServiceMock.Setup(x => x.GetEducationalGoalsAsync(false))
                .Throws<PermissionsException>();
            await educationalGoalsController.GetEducationalGoalsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalGoalsController_GetEducationalGoals_ArgumentException()
        {
            
            educationalGoalsServiceMock.Setup(x => x.GetEducationalGoalsAsync(false))
                .Throws<ArgumentException>();
            await educationalGoalsController.GetEducationalGoalsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalGoalsController_GetEducationalGoals_RepositoryException()
        {
            
            educationalGoalsServiceMock.Setup(x => x.GetEducationalGoalsAsync(false))
                .Throws<RepositoryException>();
            await educationalGoalsController.GetEducationalGoalsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalGoalsController_GetEducationalGoals_IntegrationApiException()
        {
            
            educationalGoalsServiceMock.Setup(x => x.GetEducationalGoalsAsync(false))
                .Throws<IntegrationApiException>();
            await educationalGoalsController.GetEducationalGoalsAsync();
        }

        [TestMethod]
        public async Task EducationalGoalsController_GetEducationalGoalsByGuidAsync_ValidateFields()
        {
            var expected = educationalGoalsCollection.FirstOrDefault();
            educationalGoalsServiceMock.Setup(x => x.GetEducationalGoalsByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await educationalGoalsController.GetEducationalGoalsByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalGoalsController_GetEducationalGoals_Exception()
        {
            educationalGoalsServiceMock.Setup(x => x.GetEducationalGoalsAsync(false)).Throws<Exception>();
            await educationalGoalsController.GetEducationalGoalsAsync();       
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalGoalsController_GetEducationalGoalsByGuidAsync_Exception()
        {
            educationalGoalsServiceMock.Setup(x => x.GetEducationalGoalsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await educationalGoalsController.GetEducationalGoalsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalGoalsController_GetEducationalGoalsByGuid_KeyNotFoundException()
        {
            educationalGoalsServiceMock.Setup(x => x.GetEducationalGoalsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await educationalGoalsController.GetEducationalGoalsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalGoalsController_GetEducationalGoalsByGuid_PermissionsException()
        {
            educationalGoalsServiceMock.Setup(x => x.GetEducationalGoalsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await educationalGoalsController.GetEducationalGoalsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task EducationalGoalsController_GetEducationalGoalsByGuid_ArgumentException()
        {
            educationalGoalsServiceMock.Setup(x => x.GetEducationalGoalsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await educationalGoalsController.GetEducationalGoalsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalGoalsController_GetEducationalGoalsByGuid_RepositoryException()
        {
            educationalGoalsServiceMock.Setup(x => x.GetEducationalGoalsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await educationalGoalsController.GetEducationalGoalsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalGoalsController_GetEducationalGoalsByGuid_IntegrationApiException()
        {
            educationalGoalsServiceMock.Setup(x => x.GetEducationalGoalsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await educationalGoalsController.GetEducationalGoalsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalGoalsController_GetEducationalGoalsByGuid_Exception()
        {
            educationalGoalsServiceMock.Setup(x => x.GetEducationalGoalsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await educationalGoalsController.GetEducationalGoalsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalGoalsController_PostEducationalGoalsAsync_Exception()
        {
            await educationalGoalsController.PostEducationalGoalsAsync(educationalGoalsCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalGoalsController_PutEducationalGoalsAsync_Exception()
        {
            var sourceContext = educationalGoalsCollection.FirstOrDefault();
            await educationalGoalsController.PutEducationalGoalsAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalGoalsController_DeleteEducationalGoalsAsync_Exception()
        {
            await educationalGoalsController.DeleteEducationalGoalsAsync(educationalGoalsCollection.FirstOrDefault().Id);
        }
    }
}