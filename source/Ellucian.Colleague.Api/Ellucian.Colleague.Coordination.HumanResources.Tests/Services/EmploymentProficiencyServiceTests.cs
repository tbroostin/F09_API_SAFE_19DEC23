/* Copyright 2017 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Tests;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.TestUtil;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Tests.Services
{
    [TestClass]
    public class EmploymentProficiencyServiceTests : CurrentUserSetup
    {
        private Mock<IPersonRepository> personRepoMock;
        private IPersonRepository personRepo;
        private Mock<IHumanResourcesReferenceDataRepository> refRepoMock;
        private IHumanResourcesReferenceDataRepository refRepo;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private IAdapterRegistry adapterRegistry;
        private ILogger logger;
        private Mock<IRoleRepository> roleRepoMock;
        private IRoleRepository roleRepo;
        private ICurrentUserFactory currentUserFactory;
        private IEnumerable<Domain.HumanResources.Entities.EmploymentProficiency> allEmploymentProficiencies;
        private EmploymentProficiencyService employmentProficiencyService;
        private string guid = "625c69ff-280b-4ed3-9474-662a43616a8a";

        private Domain.Entities.Permission permissionViewAnyPerson;

        [TestInitialize]
        public void Initialize()
        {
            personRepoMock = new Mock<IPersonRepository>();
            personRepo = personRepoMock.Object;
            refRepoMock = new Mock<IHumanResourcesReferenceDataRepository>();
            refRepo = refRepoMock.Object;
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            roleRepoMock = new Mock<IRoleRepository>();
            roleRepo = roleRepoMock.Object;
            logger = new Mock<ILogger>().Object;

            allEmploymentProficiencies = new TestEmploymentProficiencyRepository().GetEmploymentProficiencies();

            // Set up current user
            currentUserFactory = new CurrentUserSetup.PersonUserFactory();

            // Mock permissions
            permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.ViewAnyPerson);
            personRole.AddPermission(permissionViewAnyPerson);
            roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

            employmentProficiencyService = new EmploymentProficiencyService(refRepo, new Mock<IConfigurationRepository>().Object, adapterRegistry, currentUserFactory, roleRepo, logger);
        }

        [TestCleanup]
        public void Cleanup()
        {
            refRepo = null;
            personRepo = null;
            allEmploymentProficiencies = null;
            adapterRegistry = null;
            roleRepo = null;
            logger = null;
            employmentProficiencyService = null;
        }

        [TestMethod]
        public async Task GetEmploymentProficiencyByGuid_HEDM_ValidEmploymentProficiencyIdAsync()
        {
            Domain.HumanResources.Entities.EmploymentProficiency thisEmploymentProficiency = allEmploymentProficiencies.Where(m => m.Guid == guid).FirstOrDefault();
            refRepoMock.Setup(repo => repo.GetEmploymentProficienciesAsync(true)).ReturnsAsync(allEmploymentProficiencies.Where(m => m.Guid == guid));
            Dtos.EmploymentProficiency employmentProficiency = await employmentProficiencyService.GetEmploymentProficiencyByGuidAsync(guid);
            Assert.AreEqual(thisEmploymentProficiency.Guid, employmentProficiency.Id);
            Assert.AreEqual(thisEmploymentProficiency.Code, employmentProficiency.Code);
            Assert.AreEqual(null, employmentProficiency.Description);
            Assert.AreEqual(thisEmploymentProficiency.Description, employmentProficiency.Title);
        }


        [TestMethod]
        public async Task GetEmploymentProficiencies_HEDM_CountEmploymentProficienciesAsync()
        {
            refRepoMock.Setup(repo => repo.GetEmploymentProficienciesAsync(false)).ReturnsAsync(allEmploymentProficiencies);
            IEnumerable<Ellucian.Colleague.Dtos.EmploymentProficiency> employmentProficiency = await employmentProficiencyService.GetEmploymentProficienciesAsync();
            Assert.AreEqual(4, employmentProficiency.Count());
        }

        [TestMethod]
        public async Task GetEmploymentProficiencies_HEDM_CompareEmploymentProficienciesAsync()
        {
            refRepoMock.Setup(repo => repo.GetEmploymentProficienciesAsync(false)).ReturnsAsync(allEmploymentProficiencies);

            IEnumerable<Dtos.EmploymentProficiency> employmentProficiencies = await employmentProficiencyService.GetEmploymentProficienciesAsync();
            Assert.AreEqual(allEmploymentProficiencies.ElementAt(0).Guid, employmentProficiencies.ElementAt(0).Id);
            Assert.AreEqual(allEmploymentProficiencies.ElementAt(0).Code, employmentProficiencies.ElementAt(0).Code);
            Assert.AreEqual(null, employmentProficiencies.ElementAt(0).Description);
            Assert.AreEqual(allEmploymentProficiencies.ElementAt(0).Description, employmentProficiencies.ElementAt(0).Title);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task DemographicService_GetEmploymentProficiencyByGuid_HEDM_ThrowsInvOpExc()
        {
            refRepoMock.Setup(repo => repo.GetEmploymentProficienciesAsync(It.IsAny<bool>())).Throws<InvalidOperationException>();
            await employmentProficiencyService.GetEmploymentProficiencyByGuidAsync("dshjfkj");
        }
    }
}
