// Copyright 2019-2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class EducationGoalsControllerTests
    {
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        private IStudentReferenceDataRepository referenceDataRepository;
        private Mock<IStudentReferenceDataRepository> referenceDataRepositoryMock;
        private EducationGoalsController educationGoalsController;
        private IEnumerable<Domain.Student.Entities.EducationGoal> educationGoals;
        private IEnumerable<Domain.Student.Entities.EducationGoal> educationGoals2;
        private IAdapterRegistry adapterRegistry;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private ILogger logger;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
            referenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
            referenceDataRepository = referenceDataRepositoryMock.Object;

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            var adapter = new AutoMapperAdapter<Domain.Student.Entities.EducationGoal, Dtos.Student.EducationGoal>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.EducationGoal, Dtos.Student.EducationGoal>()).Returns(adapter);

            logger = new Mock<ILogger>().Object;

            educationGoals = BuildEducationGoals();
            educationGoals2 = BuildNonCachedEducationGoals();
            referenceDataRepositoryMock.Setup(repo => repo.GetAllEducationGoalsAsync(false)).ReturnsAsync(educationGoals);
            referenceDataRepositoryMock.Setup(repo => repo.GetAllEducationGoalsAsync(true)).ReturnsAsync(educationGoals2);
            educationGoalsController = new EducationGoalsController(adapterRegistry, referenceDataRepository, logger)
            {
                Request = new HttpRequestMessage()
            };
        }

        [TestMethod]
        public async Task EducationGoalsController_GetAsync_returns_EducationGoal_DTOs()
        {
            var educationGoalDtos = await educationGoalsController.GetEducationGoalsAsync();
            Assert.AreEqual(educationGoalDtos.Count(), educationGoals.Count());
            for (int i = 0; i < educationGoals.Count(); i++)
            {
                Assert.AreEqual(educationGoals.ElementAt(i).Code, educationGoalDtos.ElementAt(i).Code);
                Assert.AreEqual(educationGoals.ElementAt(i).Description, educationGoalDtos.ElementAt(i).Description);
            }
        }

        [TestMethod]
        public async Task EducationGoalsController_GetAsync_Bypass_Cache_returns_noncached_EducationGoal_DTOs()
        {
            educationGoalsController = new EducationGoalsController(adapterRegistry, referenceDataRepository, logger)
            {
                Request = new HttpRequestMessage()
            };
            educationGoalsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey,
                new HttpConfiguration());
            educationGoalsController.Request.Headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true,
                Public = true
            };
            var educationGoalDtos = await educationGoalsController.GetEducationGoalsAsync();
            Assert.AreEqual(educationGoalDtos.Count(), educationGoals2.Count());
            for (int i = 0; i < educationGoals2.Count(); i++)
            {
                Assert.AreEqual(educationGoals2.ElementAt(i).Code, educationGoalDtos.ElementAt(i).Code);
                Assert.AreEqual(educationGoals2.ElementAt(i).Description, educationGoalDtos.ElementAt(i).Description);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationGoalsController_GetAsync_ColleagueSessionExpiredException_ReturnsHttpResponseException_Unauthorized()
        {
            try
            {
                referenceDataRepositoryMock.Setup(repo => repo.GetAllEducationGoalsAsync(It.IsAny<bool>())).Throws(new ColleagueSessionExpiredException("session expired"));
                await educationGoalsController.GetEducationGoalsAsync();
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, ex.Response.StatusCode);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationGoalsController_GetAsync_throws_exception_when_exception_caught()
        {
            referenceDataRepositoryMock.Setup(repo => repo.GetAllEducationGoalsAsync(It.IsAny<bool>())).ThrowsAsync(new KeyNotFoundException());
            var EducationGoals = await educationGoalsController.GetEducationGoalsAsync();
        }

        private IEnumerable<Domain.Student.Entities.EducationGoal> BuildEducationGoals()
        {
            var educationGoals = new List<Domain.Student.Entities.EducationGoal>()
            {
                new Domain.Student.Entities.EducationGoal("BA", "Bachelor's Degree"),
                new Domain.Student.Entities.EducationGoal("MA", "Master's Degree")
            };

            return educationGoals;
        }
        private IEnumerable<Domain.Student.Entities.EducationGoal> BuildNonCachedEducationGoals()
        {
            var educationGoals = new List<Domain.Student.Entities.EducationGoal>()
            {
                new Domain.Student.Entities.EducationGoal("BA", "Bachelor's Degree"),
                new Domain.Student.Entities.EducationGoal("MA", "Master's Degree"),
                new Domain.Student.Entities.EducationGoal("PHD", "Doctorate Degree"),
            };

            return educationGoals;
        }

    }
}
