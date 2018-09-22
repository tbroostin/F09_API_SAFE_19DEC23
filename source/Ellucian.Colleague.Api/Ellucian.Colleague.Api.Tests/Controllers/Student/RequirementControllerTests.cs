// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Linq;
using Ellucian.Colleague.Dtos.Student;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class RequirementControllerTests
    {
        #region Test Context

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

        #endregion

        private RequirementsController requirementsController;

        private Mock<IRequirementRepository> requirementRepoMock;
        private IRequirementRepository requirementRepo;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private IAdapterRegistry adapterRegistry;
        private ILogger logger;
        private Requirement requirementData;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            requirementRepoMock = new Mock<IRequirementRepository>();
            requirementRepo = requirementRepoMock.Object;

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;

            logger = new Mock<ILogger>().Object;

            //requirementData = new TestRequirementRepository().Get("PREREQ1");
            // For now just creating a "basic" requirement for testing - avoids issue of the adapter dependencies.
            requirementData = new Requirement("111", "PREREQ1", "MATH 101", "UG", null);

            requirementRepoMock.Setup(repo => repo.GetAsync("PREREQ1")).Returns(Task.FromResult(requirementData));

            var adapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Requirements.Requirement, Dtos.Student.Requirements.Requirement>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Requirements.Requirement, Dtos.Student.Requirements.Requirement>()).Returns(adapter);

            requirementsController = new RequirementsController(adapterRegistry, requirementRepo, logger);
        }

        [TestCleanup]
        public void Cleanup()
        {
            requirementsController = null;
            requirementRepo = null;
        }

        [TestMethod]
        public async Task Get_ReturnsRequirement()
        {
            var requirementResponse =await requirementsController.GetAsync("PREREQ1");
            Assert.IsTrue(requirementResponse is Dtos.Student.Requirements.Requirement);
            Assert.AreEqual(requirementData.Description, requirementResponse.Description);
        }

        [TestMethod]
        public async Task QueryRequirementsByPost_ReturnsList()
        {
            var requirementList = new List<Requirement>()
            {
                new Requirement("1", "REQ1", "Requirement 1", "UG", new RequirementType("X", "desc", "")),
                new Requirement("2", "REQ2", "Requirement 2", "GR", new RequirementType("Y", "descy", ""))
            };

            requirementRepoMock.Setup(repo => repo.GetAsync(It.IsAny<List<string>>())).Returns(Task.FromResult(requirementList.AsEnumerable()));

            var requirementResponse = await requirementsController.QueryRequirementsByPostAsync(new RequirementQueryCriteria() { RequirementIds = new List<string>() { "1", "2" } });
            Assert.IsTrue(requirementResponse is List<Dtos.Student.Requirements.Requirement>);
            Assert.AreEqual("1", requirementResponse.ElementAt(0).Id);
            Assert.AreEqual("REQ2", requirementResponse.ElementAt(1).Code);
        }
    }
}
