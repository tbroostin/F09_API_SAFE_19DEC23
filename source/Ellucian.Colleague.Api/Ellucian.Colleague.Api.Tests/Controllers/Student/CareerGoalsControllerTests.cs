//Copyright 2020 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Web.Adapters;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class CareerGoalsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<ICareerGoalsService> careerGoalsServiceMock;
        private Mock<ILogger> loggerMock;
        private CareerGoalsController careerGoalsController;
        private IEnumerable<Domain.Student.Entities.CareerGoal> allCareerGoals;
        private List<Dtos.CareerGoals> careerGoalsCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        private Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
        private Mock<IAdapterRegistry> adapterRegistryMock;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            careerGoalsServiceMock = new Mock<ICareerGoalsService>();
            loggerMock = new Mock<ILogger>();
            careerGoalsCollection = new List<Dtos.CareerGoals>();
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();

            allCareerGoals = new List<Domain.Student.Entities.CareerGoal>()
                {
                    new Domain.Student.Entities.CareerGoal("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.Student.Entities.CareerGoal("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.Student.Entities.CareerGoal("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

            foreach (var source in allCareerGoals)
            {
                var careerGoals = new Ellucian.Colleague.Dtos.CareerGoals
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                careerGoalsCollection.Add(careerGoals);
            }

            careerGoalsController = new CareerGoalsController(adapterRegistryMock.Object, studentReferenceDataRepositoryMock.Object, 
                    careerGoalsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            careerGoalsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            careerGoalsController = null;
            allCareerGoals = null;
            careerGoalsCollection = null;
            loggerMock = null;
            careerGoalsServiceMock = null;
        }

        [TestMethod]
        public async Task CareerGoalsController_GetCareerGoals_ValidateFields_Nocache()
        {
            careerGoalsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            careerGoalsServiceMock.Setup(x => x.GetCareerGoalsAsync(false)).ReturnsAsync(careerGoalsCollection);

            var sourceContexts = (await careerGoalsController.GetCareerGoalsAsync()).ToList();
            Assert.AreEqual(careerGoalsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = careerGoalsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task CareerGoalsController_GetCareerGoals_ValidateFields_Cache()
        {
            careerGoalsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            careerGoalsServiceMock.Setup(x => x.GetCareerGoalsAsync(true)).ReturnsAsync(careerGoalsCollection);

            var sourceContexts = (await careerGoalsController.GetCareerGoalsAsync()).ToList();
            Assert.AreEqual(careerGoalsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = careerGoalsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CareerGoalsController_GetCareerGoals_KeyNotFoundException()
        {
            //
            careerGoalsServiceMock.Setup(x => x.GetCareerGoalsAsync(false))
                .Throws<KeyNotFoundException>();
            await careerGoalsController.GetCareerGoalsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CareerGoalsController_GetCareerGoals_PermissionsException()
        {

            careerGoalsServiceMock.Setup(x => x.GetCareerGoalsAsync(false))
                .Throws<PermissionsException>();
            await careerGoalsController.GetCareerGoalsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CareerGoalsController_GetCareerGoals_ArgumentException()
        {

            careerGoalsServiceMock.Setup(x => x.GetCareerGoalsAsync(false))
                .Throws<ArgumentException>();
            await careerGoalsController.GetCareerGoalsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CareerGoalsController_GetCareerGoals_RepositoryException()
        {

            careerGoalsServiceMock.Setup(x => x.GetCareerGoalsAsync(false))
                .Throws<RepositoryException>();
            await careerGoalsController.GetCareerGoalsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CareerGoalsController_GetCareerGoals_IntegrationApiException()
        {

            careerGoalsServiceMock.Setup(x => x.GetCareerGoalsAsync(false))
                .Throws<IntegrationApiException>();
            await careerGoalsController.GetCareerGoalsAsync();
        }

        [TestMethod]
        public async Task CareerGoalsController_GetCareerGoalsByGuidAsync_ValidateFields()
        {
            var expected = careerGoalsCollection.FirstOrDefault();
            careerGoalsServiceMock.Setup(x => x.GetCareerGoalsByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await careerGoalsController.GetCareerGoalsByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CareerGoalsController_GetCareerGoals_Exception()
        {
            careerGoalsServiceMock.Setup(x => x.GetCareerGoalsAsync(false)).Throws<Exception>();
            await careerGoalsController.GetCareerGoalsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CareerGoalsController_GetCareerGoalsByGuidAsync_Exception()
        {
            careerGoalsServiceMock.Setup(x => x.GetCareerGoalsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await careerGoalsController.GetCareerGoalsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CareerGoalsController_GetCareerGoalsByGuid_KeyNotFoundException()
        {
            careerGoalsServiceMock.Setup(x => x.GetCareerGoalsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await careerGoalsController.GetCareerGoalsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CareerGoalsController_GetCareerGoalsByGuid_PermissionsException()
        {
            careerGoalsServiceMock.Setup(x => x.GetCareerGoalsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await careerGoalsController.GetCareerGoalsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CareerGoalsController_GetCareerGoalsByGuid_ArgumentException()
        {
            careerGoalsServiceMock.Setup(x => x.GetCareerGoalsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await careerGoalsController.GetCareerGoalsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CareerGoalsController_GetCareerGoalsByGuid_RepositoryException()
        {
            careerGoalsServiceMock.Setup(x => x.GetCareerGoalsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await careerGoalsController.GetCareerGoalsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CareerGoalsController_GetCareerGoalsByGuid_IntegrationApiException()
        {
            careerGoalsServiceMock.Setup(x => x.GetCareerGoalsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await careerGoalsController.GetCareerGoalsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CareerGoalsController_GetCareerGoalsByGuid_Exception()
        {
            careerGoalsServiceMock.Setup(x => x.GetCareerGoalsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await careerGoalsController.GetCareerGoalsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CareerGoalsController_PostCareerGoalsAsync_Exception()
        {
            await careerGoalsController.PostCareerGoalsAsync(careerGoalsCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CareerGoalsController_PutCareerGoalsAsync_Exception()
        {
            var sourceContext = careerGoalsCollection.FirstOrDefault();
            await careerGoalsController.PutCareerGoalsAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CareerGoalsController_DeleteCareerGoalsAsync_Exception()
        {
            await careerGoalsController.DeleteCareerGoalsAsync(careerGoalsCollection.FirstOrDefault().Id);
        }
    }
}