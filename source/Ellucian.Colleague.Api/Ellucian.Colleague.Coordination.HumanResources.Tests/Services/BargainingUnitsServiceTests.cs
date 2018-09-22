//Copyright 2017 Ellucian Company L.P. and its affiliates.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Tests;

namespace Ellucian.Colleague.Coordination.HumanResources.Tests.Services
{    
    [TestClass]
    public class BargainingUnitsServiceTests : CurrentUserSetup
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
        private Mock<IConfigurationRepository> configurationRepoMock;
        private IConfigurationRepository configurationRepo;
        private ICurrentUserFactory currentUserFactory;
        private IEnumerable<Domain.HumanResources.Entities.BargainingUnit> allBargainingUnits;
        private BargainingUnitsService bargainingUnitsService;
        private string bargainingUnitsGuid = "9ae3a175-1dfd-4937-b97b-3c9ad596e023";

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
            configurationRepoMock = new Mock<IConfigurationRepository>();
            configurationRepo = configurationRepoMock.Object;
            logger = new Mock<ILogger>().Object;

            allBargainingUnits = new TestBargainingUnitsRepository().GetBargainingUnits();

            // Set up current user
            currentUserFactory = new CurrentUserSetup.PersonUserFactory();

            //// Mock permissions
            //permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.ViewAnyPerson);
            //personRole.AddPermission(permissionViewAnyPerson);
            //roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

            bargainingUnitsService = new BargainingUnitsService(refRepo, adapterRegistry, currentUserFactory, roleRepo, configurationRepo, logger);
        }

        [TestCleanup]
        public void Cleanup()
        {
            refRepo = null;
            personRepo = null;
            allBargainingUnits = null;
            adapterRegistry = null;
            roleRepo = null;
            logger = null;
            bargainingUnitsService = null;
        }

        [TestMethod]
        public async Task GetBargainingUnitByGuid_ValidBargainingUnitIdAsync()
        {
            Domain.HumanResources.Entities.BargainingUnit thisBargainingUnit = allBargainingUnits.Where(m => m.Guid == bargainingUnitsGuid).FirstOrDefault();
            refRepoMock.Setup(repo => repo.GetBargainingUnitsAsync(true)).ReturnsAsync(allBargainingUnits.Where(m => m.Guid == bargainingUnitsGuid));
            Dtos.BargainingUnit bargainingunit = await bargainingUnitsService.GetBargainingUnitsByGuidAsync(bargainingUnitsGuid);
            Assert.AreEqual(thisBargainingUnit.Guid, bargainingunit.Id);
            Assert.AreEqual(thisBargainingUnit.Code, bargainingunit.Code);
            Assert.AreEqual(null, bargainingunit.Description);
            Assert.AreEqual(thisBargainingUnit.Description, bargainingunit.Title);
        }


        [TestMethod]
        public async Task GetBargainingUnits_CountBargainingUnitsAsync()
        {
            refRepoMock.Setup(repo => repo.GetBargainingUnitsAsync(false)).ReturnsAsync(allBargainingUnits);
            IEnumerable<Ellucian.Colleague.Dtos.BargainingUnit> bargainingUnit = await bargainingUnitsService.GetBargainingUnitsAsync();
            Assert.AreEqual(4, bargainingUnit.Count());
        }

        [TestMethod]
        public async Task GetBargainingUnits_CompareBargainingUnitsAsync()
        {
            refRepoMock.Setup(repo => repo.GetBargainingUnitsAsync(false)).ReturnsAsync(allBargainingUnits);

            IEnumerable<Dtos.BargainingUnit> bargainingUnits = await bargainingUnitsService.GetBargainingUnitsAsync();
            Assert.AreEqual(allBargainingUnits.ElementAt(0).Guid, bargainingUnits.ElementAt(0).Id);
            Assert.AreEqual(allBargainingUnits.ElementAt(0).Code, bargainingUnits.ElementAt(0).Code);
            Assert.AreEqual(null, bargainingUnits.ElementAt(0).Description);
            Assert.AreEqual(allBargainingUnits.ElementAt(0).Description, bargainingUnits.ElementAt(0).Title);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task BargainingUnitService_GetBargainingUnitById_HEDM_ThrowsInvOpExc()
        {
            refRepoMock.Setup(repo => repo.GetBargainingUnitsAsync(It.IsAny<bool>())).Throws<InvalidOperationException>();
            await bargainingUnitsService.GetBargainingUnitsByGuidAsync("dshjfkj");
        }
    }
}