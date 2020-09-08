// Copyright 2015-16 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Domain.Base.Entities;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class PhoneTypeServiceTests
    {
        // The service to be tested
        private PhoneTypeService _phoneTypesService;

        private Mock<IReferenceDataRepository> _refRepoMock;
        private IReferenceDataRepository _refRepo;
        private Mock<IEventRepository> _eventRepoMock;
        private IEventRepository _eventRepo;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private IAdapterRegistry _adapterRegistry;
        private ILogger _logger;
        private Mock<IRoleRepository> _roleRepoMock;
        private IRoleRepository _roleRepo;

        private ICurrentUserFactory _currentUserFactory;
        private IConfigurationRepository _configurationRepository;
        private Mock<IConfigurationRepository> _configurationRepositoryMock;

        // Emergency information data for one person for tests
        private const string personId = "S001";

        private IEnumerable<Domain.Base.Entities.PhoneType> allPhoneTypes;

        private string phoneTypeGuid = "9ae3a175-1dfd-4937-b97b-3c9ad596e023";

        [TestInitialize]
        public void Initialize()
        {
            _eventRepoMock = new Mock<IEventRepository>();
            _eventRepo = _eventRepoMock.Object;

            _configurationRepositoryMock = new Mock<IConfigurationRepository>();
            _configurationRepository = _configurationRepositoryMock.Object;

            _refRepoMock = new Mock<IReferenceDataRepository>();
            _refRepo = _refRepoMock.Object;
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _adapterRegistry = _adapterRegistryMock.Object;
            _roleRepoMock = new Mock<IRoleRepository>();
            _roleRepo = _roleRepoMock.Object;
            _logger = new Mock<ILogger>().Object;

            // Set up current user
            _currentUserFactory = new PersonServiceTests.CurrentUserSetup.PersonUserFactory();
            _phoneTypesService = new PhoneTypeService(_adapterRegistry, _refRepo, _currentUserFactory, _roleRepo, _logger, _configurationRepository);

            allPhoneTypes = new TestPhoneTypeRepository().Get();
            _refRepoMock.Setup(repo => repo.GetPhoneTypesAsync(It.IsAny<bool>())).ReturnsAsync(allPhoneTypes);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _phoneTypesService = null;
            _refRepoMock = null;
            _refRepo = null;
            _configurationRepository = null;
            _configurationRepositoryMock = null;
            _eventRepo = null;
            _eventRepoMock = null;
            _adapterRegistry = null;
            _adapterRegistryMock = null;
            _logger = null;
            _roleRepoMock = null;
            _roleRepo = null;
            _currentUserFactory = null;
        }

        [TestMethod]
        public async Task GetPhoneTypeByGuidAsync_ComparePhoneTypesAsync()
        {
            Ellucian.Colleague.Domain.Base.Entities.PhoneType thisPhoneType = allPhoneTypes.Where(m => m.Guid == phoneTypeGuid).FirstOrDefault();
            _refRepoMock.Setup(repo => repo.GetPhoneTypesAsync(true)).ReturnsAsync(allPhoneTypes.Where(m => m.Guid == phoneTypeGuid));
            var phoneType = await _phoneTypesService.GetPhoneTypeByGuidAsync(phoneTypeGuid);
            Assert.AreEqual(thisPhoneType.Guid, phoneType.Id);
            Assert.AreEqual(thisPhoneType.Code, phoneType.Code);
            Assert.AreEqual(thisPhoneType.Description, phoneType.Title);
        }

        [TestMethod]
        public async Task GetPhoneTypesAsync_CountPhoneTypesAsync()
        {
            _refRepoMock.Setup(repo => repo.GetPhoneTypesAsync(false)).ReturnsAsync(allPhoneTypes);
            var addressTypes = await _phoneTypesService.GetPhoneTypesAsync();
            Assert.AreEqual(17, addressTypes.Count());
        }

        [TestMethod]
        public async Task GetPhoneTypesAsync_ComparePhoneTypesAsync_Cache()
        {
            _refRepoMock.Setup(repo => repo.GetPhoneTypesAsync(false)).ReturnsAsync(allPhoneTypes);
            var phoneTypes = await _phoneTypesService.GetPhoneTypesAsync(It.IsAny<bool>());
            Assert.AreEqual(allPhoneTypes.ElementAt(0).Guid, phoneTypes.ElementAt(0).Id);
            Assert.AreEqual(allPhoneTypes.ElementAt(0).Code, phoneTypes.ElementAt(0).Code);
            Assert.AreEqual(allPhoneTypes.ElementAt(0).Description, phoneTypes.ElementAt(0).Title);
        }

        [TestMethod]
        public async Task GetPhoneTypesAsync_ComparePhoneTypesAsync_NoCache()
        {
            _refRepoMock.Setup(repo => repo.GetPhoneTypesAsync(true)).ReturnsAsync(allPhoneTypes);
            var phoneTypes = await _phoneTypesService.GetPhoneTypesAsync(true);
            Assert.AreEqual(allPhoneTypes.ElementAt(0).Guid, phoneTypes.ElementAt(0).Id);
            Assert.AreEqual(allPhoneTypes.ElementAt(0).Code, phoneTypes.ElementAt(0).Code);
            Assert.AreEqual(allPhoneTypes.ElementAt(0).Description, phoneTypes.ElementAt(0).Title);
        }

        [ExpectedException(typeof(KeyNotFoundException))]
        [TestMethod]
        public async Task GetPhoneTypeByGuidAsync_InvalidAsync()
        {
            _refRepoMock.Setup(repo => repo.GetPhoneTypesAsync(false)).ReturnsAsync(allPhoneTypes);
            await _phoneTypesService.GetPhoneTypeByGuidAsync("dhjigodd");
        }

        [ExpectedException(typeof(KeyNotFoundException))]
        [TestMethod]
        public async Task GetPhoneTypeByGuid2Async_InvalidAsync()
        {
            _refRepoMock.Setup(repo => repo.GetPhoneTypesAsync(true)).ReturnsAsync(allPhoneTypes);
            await _phoneTypesService.GetPhoneTypeByGuidAsync("siuowurhf");
        }

        // Fake an ICurrentUserFactory
        public class Person001UserFactory : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims
                    {
                        // Only the PersonId is part of the test, whether it matches the ID of the person whose 
                        // emergency information is requested. The remaining fields are arbitrary.
                        ControlId = "123",
                        Name = "Fred",
                        PersonId = personId,
                        /* From the test data of the test class */
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "Student",
                        Roles = new List<string> { "Student" },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }
        //// sets up a current user
        //public abstract class CurrentUserSetup
        //{
        //    protected Domain.Entities.Role personRole = new Domain.Entities.Role(105, "Faculty");

        //    public class PersonUserFactory : ICurrentUserFactory
        //    {
        //        public ICurrentUser CurrentUser
        //        {
        //            get
        //            {
        //                return new CurrentUser(new Claims()
        //                {
        //                    ControlId = "123",
        //                    Name = "George",
        //                    PersonId = "0000015",
        //                    SecurityToken = "321",
        //                    SessionTimeout = 30,
        //                    UserName = "Faculty",
        //                    Roles = new List<string>() { "Faculty" },
        //                    SessionFixationId = "abc123",
        //                });
        //            }
        //        }
        //    }
        //}

        //[TestClass]
        //public class PhoneTypeService_Get : CurrentUserSetup
        //{

        //    private Mock<IReferenceDataRepository> refRepoMock;
        //    private IReferenceDataRepository refRepo;
        //    private Mock<IAdapterRegistry> adapterRegistryMock;
        //    private IAdapterRegistry adapterRegistry;
        //    private ILogger logger;
        //    private Mock<IRoleRepository> roleRepoMock;
        //    private IRoleRepository roleRepo;
        //    private ICurrentUserFactory currentUserFactory;
        //    private IEnumerable<Domain.Base.Entities.PhoneTypeItem> allPhoneTypes;
        //    private IEnumerable<Domain.Base.Entities.PhoneTypeItem> allOrganizationPhoneTypes;
        //    private IEnumerable<Domain.Base.Entities.PhoneTypeItem> allPersonPhoneTypes;

        //    private PhoneTypeService phoneTypeService;
        //    private string phoneTypeGuid = "69d3987d-a1da-4c32-a7ce-edb9b6c9c8b5";
        //    private Domain.Entities.Permission permissionViewAnyPerson;
        //    private string invalidGuid;

        //    [TestInitialize]
        //    public void Initialize()
        //    {
        //        invalidGuid = "zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz";

        //        refRepoMock = new Mock<IReferenceDataRepository>();
        //        refRepo = refRepoMock.Object;
        //        adapterRegistryMock = new Mock<IAdapterRegistry>();
        //        adapterRegistry = adapterRegistryMock.Object;
        //        roleRepoMock = new Mock<IRoleRepository>();
        //        roleRepo = roleRepoMock.Object;
        //        logger = new Mock<ILogger>().Object;

        //        allPhoneTypes = new TestPhoneTypeRepository().Get();

        //        adapterRegistryMock = new Mock<IAdapterRegistry>();
        //        adapterRegistry = adapterRegistryMock.Object;

        //        // Set up current user
        //        currentUserFactory = new CurrentUserSetup.PersonUserFactory();

        //        // Mock permissions
        //        permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.ViewAnyPerson);
        //        personRole.AddPermission(permissionViewAnyPerson);
        //        roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

        //        phoneTypeService = new PhoneTypeService(adapterRegistry, refRepo, currentUserFactory, roleRepo, logger);

        //        allOrganizationPhoneTypes = new TestPhoneTypeRepository().GetOrganizationPhoneTypes();

        //        allPersonPhoneTypes = new TestPhoneTypeRepository().GetPersonPhoneType();

        //    }

        //    [TestCleanup]
        //    public void Cleanup()
        //    {
        //        refRepo = null;
        //        allPhoneTypes = null;
        //        adapterRegistry = null;
        //        roleRepo = null;
        //        logger = null;
        //        phoneTypeService = null;
        //    }

        //    [TestMethod]
        //    public async Task GetPhoneTypeItemByGuid_ValidGuid()
        //    {
        //        Ellucian.Colleague.Domain.Base.Entities.PhoneTypeItem thisPhoneType = allPhoneTypes.Where(m => m.Guid == phoneTypeGuid).FirstOrDefault();
        //        refRepoMock.Setup(repo => repo.GetPhoneTypesAsync(true)).ReturnsAsync(allPhoneTypes.Where(m => m.Guid == phoneTypeGuid));
        //        Dtos.PhoneTypeItem phoneTypeItem = await phoneTypeService.GetPhoneTypeByGuidAsync(phoneTypeGuid);
        //        Assert.AreEqual(thisPhoneType.Guid, phoneTypeItem.Id);
        //        Assert.AreEqual(thisPhoneType.Code, phoneTypeItem.Code);
        //        Assert.AreEqual(thisPhoneType.Description, phoneTypeItem.Title);
        //        // Assert.AreEqual(thisPhoneType.Type, phoneTypeItem.Type);
        //    }

        //    [TestMethod]
        //    [ExpectedException(typeof(KeyNotFoundException))]
        //    public async Task GetPhoneTypeItemByGuid_InvalidGuid()
        //    {
        //        refRepoMock.Setup<Task<IEnumerable<Domain.Base.Entities.PhoneTypeItem>>>(repo => repo.GetPhoneTypesAsync(It.IsAny<bool>())).ReturnsAsync(allPhoneTypes);
        //        await phoneTypeService.GetPhoneTypeByGuidAsync("");
        //    }

        //    [TestMethod]
        //    [ExpectedException(typeof(Exception))]
        //    public async Task GetPhoneTypeItemByGuid_InvalidPhoneType()
        //    {
        //        refRepoMock.Setup(repo => repo.GetPhoneTypesAsync(It.IsAny<bool>())).Throws<Exception>();
        //        await phoneTypeService.GetPhoneTypeByGuidAsync(It.IsAny<string>());
        //    }

        //    [TestMethod]
        //    public async Task GetPhoneTypeItems_CountPhoneTypeItems()
        //    {
        //        refRepoMock.Setup(repo => repo.GetPhoneTypesAsync(false)).ReturnsAsync(allPhoneTypes);
        //        IEnumerable<Dtos.PhoneTypeItem> phoneTypeItem = await phoneTypeService.GetPhoneTypesAsync();
        //        Assert.AreEqual(allPhoneTypes.Count(), phoneTypeItem.Count());
        //    }

        //    [TestMethod]
        //    public async Task GetPhoneTypeItems_GetPhoneTypesAsync()
        //    {
        //        refRepoMock.Setup(repo => repo.GetPhoneTypesAsync(false)).ReturnsAsync(allPhoneTypes);
        //        IEnumerable<Dtos.PhoneTypeItem> phoneTypeItem = await phoneTypeService.GetPhoneTypesAsync();
        //        Assert.AreEqual(allPhoneTypes.ElementAt(0).Guid, phoneTypeItem.ElementAt(0).Id);
        //        Assert.AreEqual(allPhoneTypes.ElementAt(0).Code, phoneTypeItem.ElementAt(0).Code);
        //        Assert.AreEqual(allPhoneTypes.ElementAt(0).Description, phoneTypeItem.ElementAt(0).Title);
        //        // Assert.AreEqual(allPhoneTypes.ElementAt(0).Type, phoneTypeItem.ElementAt(0).Type);
        //    }

        //    /*
        //    [TestMethod]
        //    public async Task GetPhoneTypeItemByGuid_OrganizationPhoneTypeMain()
        //    {

        //        // var values = Enum.GetValues(typeof(Ellucian.Colleague.Domain.Base.Entities.OrganizationPhoneType));
        //        var values = Enum.GetValues(typeof(OrganizationPhoneTypeList)).Cast<Ellucian.Colleague.Domain.Base.Entities.OrganizationPhoneType>();
        //        foreach (var value in values)
        //        {

        //           // var thisPhoneType = new Ellucian.Colleague.Domain.Base.Entities.PhoneTypeItem("78d3987d-a1da-4c32-a7ce-edb9b6c9c8c4", "BU", "Business", EntityType.Organization, Ellucian.Colleague.Domain.Base.Entities.PersonPhoneType.Home, Ellucian.Colleague.Domain.Base.Entities.OrganizationPhoneType.Main);
        //            var thisPhoneType = new Ellucian.Colleague.Domain.Base.Entities.PhoneTypeItem("78d3987d-a1da-4c32-a7ce-edb9b6c9c8c4", "BU", "Business", EntityType.Organization, Ellucian.Colleague.Domain.Base.Entities.PersonPhoneType.Home, value);
        //            var phoneTypeItems = new List<Ellucian.Colleague.Domain.Base.Entities.PhoneTypeItem>();
        //            phoneTypeItems.Add(thisPhoneType);

        //            //var phoneType = new PhoneType2() { OrganizationPhoneType = new Dtos.OrganizationPhoneType() { OrganizationPhoneTypeList = OrganizationPhoneTypeList.Main } };
        //            var phoneType = new PhoneType2() { OrganizationPhoneType = new Dtos.OrganizationPhoneType() { OrganizationPhoneTypeList = value } };

        //            refRepoMock.Setup(repo => repo.GetPhoneTypesAsync(true)).ReturnsAsync(phoneTypeItems as IEnumerable<Ellucian.Colleague.Domain.Base.Entities.PhoneTypeItem>);
        //            Dtos.PhoneTypeItem phoneTypeItem = await phoneTypeService.GetPhoneTypeByGuidAsync("78d3987d-a1da-4c32-a7ce-edb9b6c9c8c4");
        //            Assert.AreEqual(thisPhoneType.Guid, phoneTypeItem.Id);
        //            Assert.AreEqual(thisPhoneType.Code, phoneTypeItem.Code);
        //            Assert.AreEqual(thisPhoneType.Description, phoneTypeItem.Title);
        //            Assert.AreEqual(phoneType, phoneTypeItem.Type);
        //        }
        //    }
        //    */
        //    [TestMethod]
        //    public async Task GetPhoneTypeItemByGuid_AllOrganizationPhoneTypes()
        //    {
        //        var allOrgs = new TestPhoneTypeRepository().GetOrganizationPhoneTypes();

        //        var values = Enum.GetValues(typeof(OrganizationPhoneTypeList)).Cast<Ellucian.Colleague.Domain.Base.Entities.OrganizationPhoneType>();
        //        foreach (var value in values)
        //        {
        //            var thisPhoneType = allOrgs.Where(x => x.Type.OrganizationPhoneType == value).FirstOrDefault();
        //            refRepoMock.Setup(repo => repo.GetPhoneTypesAsync(true)).ReturnsAsync(allOrgs.Where(x => x.Type.OrganizationPhoneType == value));
        //            var phoneTypeItem = (await phoneTypeService.GetPhoneTypesAsync(true)).FirstOrDefault();

        //            Assert.AreEqual(thisPhoneType.Guid, phoneTypeItem.Id);
        //            Assert.AreEqual(thisPhoneType.Code, phoneTypeItem.Code);
        //            Assert.AreEqual(thisPhoneType.Description, phoneTypeItem.Title);
        //        }
        //    }

        //    [TestMethod]
        //    public async Task GetPhoneTypeItemByGuid_OrganizationPhoneType_Main()
        //    {
        //        var organizationPhoneType = Ellucian.Colleague.Domain.Base.Entities.OrganizationPhoneType.Main;
        //        var thisPhoneType = allOrganizationPhoneTypes.Where(x => x.Type.OrganizationPhoneType == organizationPhoneType).FirstOrDefault();

        //        refRepoMock.Setup(repo => repo.GetPhoneTypesAsync(true)).ReturnsAsync(allOrganizationPhoneTypes.Where(x => x.Type.OrganizationPhoneType == organizationPhoneType));
        //        var phoneTypeItem = (await phoneTypeService.GetPhoneTypesAsync(true)).FirstOrDefault();

        //        Assert.AreEqual(thisPhoneType.Guid, phoneTypeItem.Id);
        //        Assert.AreEqual(thisPhoneType.Code, phoneTypeItem.Code);
        //        Assert.AreEqual(thisPhoneType.Description, phoneTypeItem.Title);
        //        Assert.AreEqual(OrganizationPhoneTypeList.Main, phoneTypeItem.Type.OrganizationPhoneType.OrganizationPhoneTypeList);
        //    }

        //    [TestMethod]
        //    public async Task GetPhoneTypeItemByGuid_OrganizationPhoneType_Billing()
        //    {
        //        var organizationPhoneType = Ellucian.Colleague.Domain.Base.Entities.OrganizationPhoneType.Billing;
        //        var thisPhoneType = allOrganizationPhoneTypes.Where(x => x.Type.OrganizationPhoneType == organizationPhoneType).FirstOrDefault();

        //        refRepoMock.Setup(repo => repo.GetPhoneTypesAsync(true)).ReturnsAsync(allOrganizationPhoneTypes.Where(x => x.Type.OrganizationPhoneType == organizationPhoneType));
        //        var phoneTypeItem = (await phoneTypeService.GetPhoneTypesAsync(true)).FirstOrDefault();

        //        Assert.AreEqual(thisPhoneType.Guid, phoneTypeItem.Id);
        //        Assert.AreEqual(thisPhoneType.Code, phoneTypeItem.Code);
        //        Assert.AreEqual(thisPhoneType.Description, phoneTypeItem.Title);
        //        Assert.AreEqual(OrganizationPhoneTypeList.Billing, phoneTypeItem.Type.OrganizationPhoneType.OrganizationPhoneTypeList);
        //    }

        //    [TestMethod]
        //    public async Task GetPhoneTypeItemByGuid_OrganizationPhoneType_Branch()
        //    {
        //        var organizationPhoneType = Ellucian.Colleague.Domain.Base.Entities.OrganizationPhoneType.Branch;
        //        var thisPhoneType = allOrganizationPhoneTypes.Where(x => x.Type.OrganizationPhoneType == organizationPhoneType).FirstOrDefault();

        //        refRepoMock.Setup(repo => repo.GetPhoneTypesAsync(true)).ReturnsAsync(allOrganizationPhoneTypes.Where(x => x.Type.OrganizationPhoneType == organizationPhoneType));
        //        var phoneTypeItem = (await phoneTypeService.GetPhoneTypesAsync(true)).FirstOrDefault();

        //        Assert.AreEqual(thisPhoneType.Guid, phoneTypeItem.Id);
        //        Assert.AreEqual(thisPhoneType.Code, phoneTypeItem.Code);
        //        Assert.AreEqual(thisPhoneType.Description, phoneTypeItem.Title);
        //        Assert.AreEqual(OrganizationPhoneTypeList.Branch, phoneTypeItem.Type.OrganizationPhoneType.OrganizationPhoneTypeList);
        //    }

        //    [TestMethod]
        //    public async Task GetPhoneTypeItemByGuid_OrganizationPhoneType_Other()
        //    {
        //        var organizationPhoneType = Ellucian.Colleague.Domain.Base.Entities.OrganizationPhoneType.Other;
        //        var thisPhoneType = allOrganizationPhoneTypes.Where(x => x.Type.OrganizationPhoneType == organizationPhoneType).FirstOrDefault();

        //        refRepoMock.Setup(repo => repo.GetPhoneTypesAsync(true)).ReturnsAsync(allOrganizationPhoneTypes.Where(x => x.Type.OrganizationPhoneType == organizationPhoneType));
        //        var phoneTypeItem = (await phoneTypeService.GetPhoneTypesAsync(true)).FirstOrDefault();

        //        Assert.AreEqual(thisPhoneType.Guid, phoneTypeItem.Id);
        //        Assert.AreEqual(thisPhoneType.Code, phoneTypeItem.Code);
        //        Assert.AreEqual(thisPhoneType.Description, phoneTypeItem.Title);
        //        Assert.AreEqual(OrganizationPhoneTypeList.Other, phoneTypeItem.Type.OrganizationPhoneType.OrganizationPhoneTypeList);
        //    }

        //    [TestMethod]
        //    public async Task GetPhoneTypeItemByGuid_OrganizationPhoneType_Region()
        //    {
        //        var organizationPhoneType = Ellucian.Colleague.Domain.Base.Entities.OrganizationPhoneType.Region;
        //        var thisPhoneType = allOrganizationPhoneTypes.Where(x => x.Type.OrganizationPhoneType == organizationPhoneType).FirstOrDefault();

        //        refRepoMock.Setup(repo => repo.GetPhoneTypesAsync(true)).ReturnsAsync(allOrganizationPhoneTypes.Where(x => x.Type.OrganizationPhoneType == organizationPhoneType));
        //        var phoneTypeItem = (await phoneTypeService.GetPhoneTypesAsync(true)).FirstOrDefault();

        //        Assert.AreEqual(thisPhoneType.Guid, phoneTypeItem.Id);
        //        Assert.AreEqual(thisPhoneType.Code, phoneTypeItem.Code);
        //        Assert.AreEqual(thisPhoneType.Description, phoneTypeItem.Title);
        //        Assert.AreEqual(OrganizationPhoneTypeList.Region, phoneTypeItem.Type.OrganizationPhoneType.OrganizationPhoneTypeList);
        //    }

        //    [TestMethod]
        //    public async Task GetPhoneTypeItemByGuid_OrganizationPhoneType_Support()
        //    {
        //        var organizationPhoneType = Ellucian.Colleague.Domain.Base.Entities.OrganizationPhoneType.Support;
        //        var thisPhoneType = allOrganizationPhoneTypes.Where(x => x.Type.OrganizationPhoneType == organizationPhoneType).FirstOrDefault();

        //        refRepoMock.Setup(repo => repo.GetPhoneTypesAsync(true)).ReturnsAsync(allOrganizationPhoneTypes.Where(x => x.Type.OrganizationPhoneType == organizationPhoneType));
        //        var phoneTypeItem = (await phoneTypeService.GetPhoneTypesAsync(true)).FirstOrDefault();

        //        Assert.AreEqual(thisPhoneType.Guid, phoneTypeItem.Id);
        //        Assert.AreEqual(thisPhoneType.Code, phoneTypeItem.Code);
        //        Assert.AreEqual(thisPhoneType.Description, phoneTypeItem.Title);
        //        Assert.AreEqual(OrganizationPhoneTypeList.Support, phoneTypeItem.Type.OrganizationPhoneType.OrganizationPhoneTypeList);
        //    }

        //    [TestMethod]
        //    public async Task GetPhoneTypeItemByGuid_PersonPhoneType_Business()
        //    {
        //        var personPhoneType = Ellucian.Colleague.Domain.Base.Entities.PersonPhoneType.Business;
        //        var thisPhoneType = allPersonPhoneTypes.Where(x => x.Type.PersonPhoneType == personPhoneType).FirstOrDefault();

        //        refRepoMock.Setup(repo => repo.GetPhoneTypesAsync(It.IsAny<bool>())).ReturnsAsync(allPersonPhoneTypes.Where(x => x.Type.PersonPhoneType == personPhoneType));
        //        var phoneTypeItem = (await phoneTypeService.GetPhoneTypesAsync(true)).FirstOrDefault();

        //        Assert.AreEqual(thisPhoneType.Guid, phoneTypeItem.Id);
        //        Assert.AreEqual(thisPhoneType.Code, phoneTypeItem.Code);
        //        Assert.AreEqual(thisPhoneType.Description, phoneTypeItem.Title);
        //        Assert.AreEqual(PersonPhoneTypeList.Business, phoneTypeItem.Type.PersonPhoneType.PersonPhoneTypeList);
        //    }

        //    [TestMethod]
        //    public async Task GetPhoneTypeItemByGuid_PersonPhoneType_Fax()
        //    {
        //        var personPhoneType = Ellucian.Colleague.Domain.Base.Entities.PersonPhoneType.Fax;
        //        var thisPhoneType = allPersonPhoneTypes.Where(x => x.Type.PersonPhoneType == personPhoneType).FirstOrDefault();

        //        refRepoMock.Setup(repo => repo.GetPhoneTypesAsync(It.IsAny<bool>())).ReturnsAsync(allPersonPhoneTypes.Where(x => x.Type.PersonPhoneType == personPhoneType));
        //        var phoneTypeItem = (await phoneTypeService.GetPhoneTypesAsync(true)).FirstOrDefault();

        //        Assert.AreEqual(thisPhoneType.Guid, phoneTypeItem.Id);
        //        Assert.AreEqual(thisPhoneType.Code, phoneTypeItem.Code);
        //        Assert.AreEqual(thisPhoneType.Description, phoneTypeItem.Title);
        //        Assert.AreEqual(PersonPhoneTypeList.Fax, phoneTypeItem.Type.PersonPhoneType.PersonPhoneTypeList);
        //    }

        //    [TestMethod]
        //    public async Task GetPhoneTypeItemByGuid_PersonPhoneType_Home()
        //    {
        //        var personPhoneType = Ellucian.Colleague.Domain.Base.Entities.PersonPhoneType.Home;
        //        var thisPhoneType = allPersonPhoneTypes.Where(x => x.Type.PersonPhoneType == personPhoneType).FirstOrDefault();

        //        refRepoMock.Setup(repo => repo.GetPhoneTypesAsync(It.IsAny<bool>())).ReturnsAsync(allPersonPhoneTypes.Where(x => x.Type.PersonPhoneType == personPhoneType));
        //        var phoneTypeItem = (await phoneTypeService.GetPhoneTypesAsync(true)).FirstOrDefault();

        //        Assert.AreEqual(thisPhoneType.Guid, phoneTypeItem.Id);
        //        Assert.AreEqual(thisPhoneType.Code, phoneTypeItem.Code);
        //        Assert.AreEqual(thisPhoneType.Description, phoneTypeItem.Title);
        //        Assert.AreEqual(PersonPhoneTypeList.Home, phoneTypeItem.Type.PersonPhoneType.PersonPhoneTypeList);
        //    }

        //    [TestMethod]
        //    public async Task GetPhoneTypeItemByGuid_PersonPhoneType_Mobile()
        //    {
        //        var personPhoneType = Ellucian.Colleague.Domain.Base.Entities.PersonPhoneType.Mobile;
        //        var thisPhoneType = allPersonPhoneTypes.Where(x => x.Type.PersonPhoneType == personPhoneType).FirstOrDefault();

        //        refRepoMock.Setup(repo => repo.GetPhoneTypesAsync(It.IsAny<bool>())).ReturnsAsync(allPersonPhoneTypes.Where(x => x.Type.PersonPhoneType == personPhoneType));
        //        var phoneTypeItem = (await phoneTypeService.GetPhoneTypesAsync(true)).FirstOrDefault();

        //        Assert.AreEqual(thisPhoneType.Guid, phoneTypeItem.Id);
        //        Assert.AreEqual(thisPhoneType.Code, phoneTypeItem.Code);
        //        Assert.AreEqual(thisPhoneType.Description, phoneTypeItem.Title);
        //        Assert.AreEqual(PersonPhoneTypeList.Mobile, phoneTypeItem.Type.PersonPhoneType.PersonPhoneTypeList);
        //    }

        //    [TestMethod]
        //    public async Task GetPhoneTypeItemByGuid_PersonPhoneType_Other()
        //    {
        //        var personPhoneType = Ellucian.Colleague.Domain.Base.Entities.PersonPhoneType.Other;
        //        var thisPhoneType = allPersonPhoneTypes.Where(x => x.Type.PersonPhoneType == personPhoneType).FirstOrDefault();

        //        refRepoMock.Setup(repo => repo.GetPhoneTypesAsync(It.IsAny<bool>())).ReturnsAsync(allPersonPhoneTypes.Where(x => x.Type.PersonPhoneType == personPhoneType));
        //        var phoneTypeItem = (await phoneTypeService.GetPhoneTypesAsync(true)).FirstOrDefault();

        //        Assert.AreEqual(thisPhoneType.Guid, phoneTypeItem.Id);
        //        Assert.AreEqual(thisPhoneType.Code, phoneTypeItem.Code);
        //        Assert.AreEqual(thisPhoneType.Description, phoneTypeItem.Title);
        //        Assert.AreEqual(PersonPhoneTypeList.Other, phoneTypeItem.Type.PersonPhoneType.PersonPhoneTypeList);
        //    }

        //    [TestMethod]
        //    public async Task GetPhoneTypeItemByGuid_PersonPhoneType_Pager()
        //    {
        //        var personPhoneType = Ellucian.Colleague.Domain.Base.Entities.PersonPhoneType.Pager;
        //        var thisPhoneType = allPersonPhoneTypes.Where(x => x.Type.PersonPhoneType == personPhoneType).FirstOrDefault();

        //        refRepoMock.Setup(repo => repo.GetPhoneTypesAsync(It.IsAny<bool>())).ReturnsAsync(allPersonPhoneTypes.Where(x => x.Type.PersonPhoneType == personPhoneType));
        //        var phoneTypeItem = (await phoneTypeService.GetPhoneTypesAsync(true)).FirstOrDefault();

        //        Assert.AreEqual(thisPhoneType.Guid, phoneTypeItem.Id);
        //        Assert.AreEqual(thisPhoneType.Code, phoneTypeItem.Code);
        //        Assert.AreEqual(thisPhoneType.Description, phoneTypeItem.Title);
        //        Assert.AreEqual(PersonPhoneTypeList.Pager, phoneTypeItem.Type.PersonPhoneType.PersonPhoneTypeList);
        //    }

        //    [TestMethod]
        //    public async Task GetPhoneTypeItemByGuid_PersonPhoneType_School()
        //    {
        //        var personPhoneType = Ellucian.Colleague.Domain.Base.Entities.PersonPhoneType.School;
        //        var thisPhoneType = allPersonPhoneTypes.Where(x => x.Type.PersonPhoneType == personPhoneType).FirstOrDefault();

        //        refRepoMock.Setup(repo => repo.GetPhoneTypesAsync(It.IsAny<bool>())).ReturnsAsync(allPersonPhoneTypes.Where(x => x.Type.PersonPhoneType == personPhoneType));
        //        var phoneTypeItem = (await phoneTypeService.GetPhoneTypesAsync(true)).FirstOrDefault();

        //        Assert.AreEqual(thisPhoneType.Guid, phoneTypeItem.Id);
        //        Assert.AreEqual(thisPhoneType.Code, phoneTypeItem.Code);
        //        Assert.AreEqual(thisPhoneType.Description, phoneTypeItem.Title);
        //        Assert.AreEqual(PersonPhoneTypeList.School, phoneTypeItem.Type.PersonPhoneType.PersonPhoneTypeList);
        //    }

        //    [TestMethod]
        //    public async Task GetPhoneTypeItemByGuid_PersonPhoneType_TDD()
        //    {
        //        var personPhoneType = Ellucian.Colleague.Domain.Base.Entities.PersonPhoneType.TDD;
        //        var thisPhoneType = allPersonPhoneTypes.Where(x => x.Type.PersonPhoneType == personPhoneType).FirstOrDefault();

        //        refRepoMock.Setup(repo => repo.GetPhoneTypesAsync(It.IsAny<bool>())).ReturnsAsync(allPersonPhoneTypes.Where(x => x.Type.PersonPhoneType == personPhoneType));
        //        var phoneTypeItem = (await phoneTypeService.GetPhoneTypesAsync(true)).FirstOrDefault();

        //        Assert.AreEqual(thisPhoneType.Guid, phoneTypeItem.Id);
        //        Assert.AreEqual(thisPhoneType.Code, phoneTypeItem.Code);
        //        Assert.AreEqual(thisPhoneType.Description, phoneTypeItem.Title);
        //        Assert.AreEqual(PersonPhoneTypeList.TDD, phoneTypeItem.Type.PersonPhoneType.PersonPhoneTypeList);
        //    }

        //    [TestMethod]
        //    public async Task GetPhoneTypeItemByGuid_PersonPhoneType_Vacation()
        //    {
        //        var personPhoneType = Ellucian.Colleague.Domain.Base.Entities.PersonPhoneType.Vacation;
        //        var thisPhoneType = allPersonPhoneTypes.Where(x => x.Type.PersonPhoneType == personPhoneType).FirstOrDefault();

        //        refRepoMock.Setup(repo => repo.GetPhoneTypesAsync(It.IsAny<bool>())).ReturnsAsync(allPersonPhoneTypes.Where(x => x.Type.PersonPhoneType == personPhoneType));
        //        var phoneTypeItem = (await phoneTypeService.GetPhoneTypesAsync(true)).FirstOrDefault();

        //        Assert.AreEqual(thisPhoneType.Guid, phoneTypeItem.Id);
        //        Assert.AreEqual(thisPhoneType.Code, phoneTypeItem.Code);
        //        Assert.AreEqual(thisPhoneType.Description, phoneTypeItem.Title);
        //        Assert.AreEqual(PersonPhoneTypeList.Vacation, phoneTypeItem.Type.PersonPhoneType.PersonPhoneTypeList);
        //    }
        //}
    }
}