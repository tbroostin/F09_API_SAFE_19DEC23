/* Copyright 2016 Ellucian Company L.P. and its affiliates. */
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
    public class RehireTypeServiceTests : CurrentUserSetup
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
        private IEnumerable<Domain.HumanResources.Entities.RehireType> allRehireTypes;
        private RehireTypeService rehireTypeService;
        private string guid = "625c69ff-280b-4ed3-9474-662a43616a8a";
        private IConfigurationRepository _configurationRepository;
        private Mock<IConfigurationRepository> _configurationRepositoryMock;

        private Domain.Entities.Permission permissionViewAnyPerson;

        [TestInitialize]
        public void Initialize()
        {
            personRepoMock = new Mock<IPersonRepository>();
            personRepo = personRepoMock.Object;
            refRepoMock = new Mock<IHumanResourcesReferenceDataRepository>();
            refRepo = refRepoMock.Object;
            _configurationRepositoryMock = new Mock<IConfigurationRepository>();
            _configurationRepository = _configurationRepositoryMock.Object;
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            roleRepoMock = new Mock<IRoleRepository>();
            roleRepo = roleRepoMock.Object;
            logger = new Mock<ILogger>().Object;

            allRehireTypes = new TestRehireTypeRepository().GetRehireTypes();

            // Set up current user
            currentUserFactory = new CurrentUserSetup.PersonUserFactory();

            // Mock permissions
            permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.ViewAnyPerson);
            personRole.AddPermission(permissionViewAnyPerson);
            roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

            rehireTypeService = new RehireTypeService(refRepo, adapterRegistry, currentUserFactory, _configurationRepository, roleRepo, logger);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _configurationRepository = null;
            refRepo = null;
            personRepo = null;
            allRehireTypes = null;
            adapterRegistry = null;
            roleRepo = null;
            logger = null;
            rehireTypeService = null;
        }

        [TestMethod]
        public async Task GetRehireTypeByGuid_HEDM_ValidRehireTypeIdAsync()
        {
            Domain.HumanResources.Entities.RehireType thisRehireType = allRehireTypes.Where(m => m.Guid == guid).FirstOrDefault();
            refRepoMock.Setup(repo => repo.GetRehireTypesAsync(true)).ReturnsAsync(allRehireTypes.Where(m => m.Guid == guid));
            Dtos.RehireType rehireType = await rehireTypeService.GetRehireTypeByGuidAsync(guid);
            Assert.AreEqual(thisRehireType.Guid, rehireType.Id);
            Assert.AreEqual(thisRehireType.Code, rehireType.Code);
            Assert.AreEqual(null, rehireType.Description);
            Assert.AreEqual(thisRehireType.Description, rehireType.Title);
        }


        [TestMethod]
        public async Task GetRehireTypes_HEDM_CountRehireTypesAsync()
        {
            refRepoMock.Setup(repo => repo.GetRehireTypesAsync(false)).ReturnsAsync(allRehireTypes);
            IEnumerable<Ellucian.Colleague.Dtos.RehireType> rehireType = await rehireTypeService.GetRehireTypesAsync();
            Assert.AreEqual(4, rehireType.Count());
        }

        [TestMethod]
        public async Task GetRehireTypes_HEDM_CompareRehireTypesAsync()
        {
            refRepoMock.Setup(repo => repo.GetRehireTypesAsync(false)).ReturnsAsync(allRehireTypes);

            IEnumerable<Dtos.RehireType> rehireTypes = await rehireTypeService.GetRehireTypesAsync();
            Assert.AreEqual(allRehireTypes.ElementAt(0).Guid, rehireTypes.ElementAt(0).Id);
            Assert.AreEqual(allRehireTypes.ElementAt(0).Code, rehireTypes.ElementAt(0).Code);
            Assert.AreEqual(null, rehireTypes.ElementAt(0).Description);
            Assert.AreEqual(allRehireTypes.ElementAt(0).Description, rehireTypes.ElementAt(0).Title);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task DemographicService_GetRehireTypeByGuid_HEDM_ThrowsInvOpExc()
        {
            refRepoMock.Setup(repo => repo.GetRehireTypesAsync(It.IsAny<bool>())).Throws<InvalidOperationException>();
            await rehireTypeService.GetRehireTypeByGuidAsync("dshjfkj");
        }
    }
}
