// Copyright 2015-2023 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class DemographicServiceTests
    {
        // sets up a current user
        public abstract class CurrentUserSetup
        {
            protected Domain.Entities.Role personRole = new Domain.Entities.Role(105, "Faculty");

            public class PersonUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "George",
                            PersonId = "0000015",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "Faculty",
                            Roles = new List<string>() { "Faculty" },
                            SessionFixationId = "abc123",
                        });
                    }
                }
            }
        }

        [TestClass]
        public class GetCitizenshipStatuses : CurrentUserSetup
        {
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IPersonBaseRepository> personBaseRepoMock;
            private IPersonBaseRepository personBaseRepo;
            private Mock<IReferenceDataRepository> refRepoMock;
            private IReferenceDataRepository refRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private Mock<IStaffRepository> staffRepoMock;
            private IStaffRepository staffRepo;
            private Mock<IConfigurationRepository> configRepoMock;
            private IConfigurationRepository configRepo;
            private ICurrentUserFactory currentUserFactory;
            private IEnumerable<Domain.Base.Entities.CitizenshipStatus> allCitizenshipStatuses;
            private DemographicService demographicService;
            private string demographicGuid = "87ec6f69-9b16-4ed5-8954-59067f0318ec";
            private Mock<IConfigurationRepository> _configurationRepositoryMock;

            private Domain.Entities.Permission permissionViewAnyPerson;

            [TestInitialize]
            public void Initialize()
            {
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                personBaseRepoMock = new Mock<IPersonBaseRepository>();
                personBaseRepo = personBaseRepoMock.Object;
                refRepoMock = new Mock<IReferenceDataRepository>();
                refRepo = refRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                _configurationRepositoryMock = new Mock<IConfigurationRepository>();
                roleRepo = roleRepoMock.Object;
                staffRepoMock = new Mock<IStaffRepository>();
                staffRepo = staffRepoMock.Object;
                configRepoMock = new Mock<IConfigurationRepository>();
                configRepo = configRepoMock.Object;
                logger = new Mock<ILogger>().Object;

                allCitizenshipStatuses = new TestCitizenshipStatusRepository().Get();

                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                // Mock permissions
                permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.ViewAnyPerson);
                personRole.AddPermission(permissionViewAnyPerson);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

                demographicService = new DemographicService(refRepo, personBaseRepo, adapterRegistry, currentUserFactory, roleRepo, logger, configRepo, staffRepo);
            }

            [TestCleanup]
            public void Cleanup()
            {
                refRepo = null;
                personRepo = null;
                allCitizenshipStatuses = null;
                adapterRegistry = null;
                roleRepo = null;
                logger = null;
                demographicService = null;
                _configurationRepositoryMock = null;
            }

            [TestMethod]
            public async Task GetCitizenshipStatusByGuid_HEDM_ValidCitizenshipStatusIdAsync()
            {
                Domain.Base.Entities.CitizenshipStatus thisCitizenshipStatus = allCitizenshipStatuses.Where(m => m.Guid == demographicGuid).FirstOrDefault();
                refRepoMock.Setup(repo => repo.GetCitizenshipStatusesAsync(true)).ReturnsAsync(allCitizenshipStatuses.Where(m => m.Guid == demographicGuid));
                Dtos.CitizenshipStatus citizenshipStatus = await demographicService.GetCitizenshipStatusByGuidAsync(demographicGuid);
                Assert.AreEqual(thisCitizenshipStatus.Guid, citizenshipStatus.Id);
                Assert.AreEqual(thisCitizenshipStatus.Code, citizenshipStatus.Code);
                Assert.AreEqual(null, citizenshipStatus.Description);
                Assert.AreEqual(thisCitizenshipStatus.Description, citizenshipStatus.Title);
            }


            [TestMethod]
            public async Task GetCitizenshipStatuses_HEDM_CountCitizenshipStatusesAsync()
            {
                refRepoMock.Setup(repo => repo.GetCitizenshipStatusesAsync(false)).ReturnsAsync(allCitizenshipStatuses);
                IEnumerable<Ellucian.Colleague.Dtos.CitizenshipStatus> citizenshipStatus = await demographicService.GetCitizenshipStatusesAsync();
                Assert.AreEqual(3, citizenshipStatus.Count());
            }

            [TestMethod]
            public async Task GetCitizenshipStatuses_HEDM_CompareCitizenshipStatusesAsync()
            {
                refRepoMock.Setup(repo => repo.GetCitizenshipStatusesAsync(false)).ReturnsAsync(allCitizenshipStatuses);

                IEnumerable<Dtos.CitizenshipStatus> citizenshipStatuses = await demographicService.GetCitizenshipStatusesAsync();
                Assert.AreEqual(allCitizenshipStatuses.ElementAt(0).Guid, citizenshipStatuses.ElementAt(0).Id);
                Assert.AreEqual(allCitizenshipStatuses.ElementAt(0).Code, citizenshipStatuses.ElementAt(0).Code);
                Assert.AreEqual(null, citizenshipStatuses.ElementAt(0).Description);
                Assert.AreEqual(allCitizenshipStatuses.ElementAt(0).Description, citizenshipStatuses.ElementAt(0).Title);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task DemographicService_GetCitizenshipStatusByGuid_HEDM_ThrowsInvOpExc()
            {
                refRepoMock.Setup(repo => repo.GetCitizenshipStatusesAsync(It.IsAny<bool>())).Throws<InvalidOperationException>();
                await demographicService.GetCitizenshipStatusByGuidAsync("dshjfkj");
            }
        }

        [TestClass]
        public class GetEthnicities : CurrentUserSetup
        {
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IPersonBaseRepository> personBaseRepoMock;
            private IPersonBaseRepository personBaseRepo;
            private Mock<IReferenceDataRepository> refRepoMock;
            private IReferenceDataRepository refRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private Mock<IStaffRepository> staffRepoMock;
            private IStaffRepository staffRepo;
            private Mock<IConfigurationRepository> configRepoMock;
            private IConfigurationRepository configRepo;
            private ICurrentUserFactory currentUserFactory;
            private IEnumerable<Domain.Base.Entities.Ethnicity> allEthnicities;
            private DemographicService demographicService;
            private string demographicGuid = "9ae3a175-1dfd-4937-b97b-3c9ad596e023";
            private Mock<IConfigurationRepository> _configurationRepositoryMock;

            private Domain.Entities.Permission permissionViewAnyPerson;

            [TestInitialize]
            public void Initialize()
            {
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                personBaseRepoMock = new Mock<IPersonBaseRepository>();
                personBaseRepo = personBaseRepoMock.Object;
                refRepoMock = new Mock<IReferenceDataRepository>();
                refRepo = refRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                staffRepoMock = new Mock<IStaffRepository>();
                staffRepo = staffRepoMock.Object;
                configRepoMock = new Mock<IConfigurationRepository>();
                configRepo = configRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                _configurationRepositoryMock = new Mock<IConfigurationRepository>();

                allEthnicities = new TestEthnicityRepository().Get();

                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                // Mock permissions
                permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.ViewAnyPerson);
                personRole.AddPermission(permissionViewAnyPerson);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

                demographicService = new DemographicService(refRepo, personBaseRepo, adapterRegistry, currentUserFactory, roleRepo, logger, configRepo, staffRepo);
            }

            [TestCleanup]
            public void Cleanup()
            {
                refRepo = null;
                personRepo = null;
                allEthnicities = null;
                adapterRegistry = null;
                roleRepo = null;
                logger = null;
                demographicService = null;
                _configurationRepositoryMock = null;
            }

            [TestMethod]
            public async Task GetEthnicityByGuid_ValidEthnicityIdAsync()
            {
                Domain.Base.Entities.Ethnicity thisEthnicity = allEthnicities.Where(m => m.Guid == demographicGuid).FirstOrDefault();
                refRepoMock.Setup(repo => repo.GetEthnicitiesAsync(true)).ReturnsAsync(allEthnicities.Where(m => m.Guid == demographicGuid));
                Dtos.Ethnicity2 ethnicity = await demographicService.GetEthnicityById2Async(demographicGuid);
                Assert.AreEqual(thisEthnicity.Guid, ethnicity.Id);
                Assert.AreEqual(thisEthnicity.Code, ethnicity.Code);
                Assert.AreEqual(null, ethnicity.Description);
                Assert.AreEqual(thisEthnicity.Description, ethnicity.Title);
            }


            [TestMethod]
            public async Task GetEthnicities_CountEthnicitiesAsync()
            {
                refRepoMock.Setup(repo => repo.GetEthnicitiesAsync(false)).ReturnsAsync(allEthnicities);
                IEnumerable<Ellucian.Colleague.Dtos.Ethnicity2> ethnicity = await demographicService.GetEthnicities2Async();
                Assert.AreEqual(2, ethnicity.Count());
            }

            [TestMethod]
            public async Task GetEthnicities_CompareEthnicitiesMajorsAsync()
            {
                refRepoMock.Setup(repo => repo.GetEthnicitiesAsync(false)).ReturnsAsync(allEthnicities);

                IEnumerable<Dtos.Ethnicity2> ethnicities = await demographicService.GetEthnicities2Async();
                Assert.AreEqual(allEthnicities.ElementAt(0).Guid, ethnicities.ElementAt(0).Id);
                Assert.AreEqual(allEthnicities.ElementAt(0).Code, ethnicities.ElementAt(0).Code);
                Assert.AreEqual(null, ethnicities.ElementAt(0).Description);
                Assert.AreEqual(allEthnicities.ElementAt(0).Description, ethnicities.ElementAt(0).Title);
            }
        }

        [TestClass]
        public class GetGeographicAreaTypes : CurrentUserSetup
        {
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IPersonBaseRepository> personBaseRepoMock;
            private IPersonBaseRepository personBaseRepo;
            private Mock<IReferenceDataRepository> refRepoMock;
            private IReferenceDataRepository refRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private Mock<IStaffRepository> staffRepoMock;
            private IStaffRepository staffRepo;
            private Mock<IConfigurationRepository> configRepoMock;
            private IConfigurationRepository configRepo;
            private ICurrentUserFactory currentUserFactory;
            private IEnumerable<Domain.Base.Entities.GeographicAreaType> allGeographicAreaTypes;
            private DemographicService demographicService;
            private string demographicGuid = "87ec6f69-9b16-4ed5-8954-59067f0318ec";
            private Mock<IConfigurationRepository> _configurationRepositoryMock;

            private Domain.Entities.Permission permissionViewAnyPerson;

            [TestInitialize]
            public void Initialize()
            {
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                personBaseRepoMock = new Mock<IPersonBaseRepository>();
                personBaseRepo = personBaseRepoMock.Object;
                refRepoMock = new Mock<IReferenceDataRepository>();
                refRepo = refRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                staffRepoMock = new Mock<IStaffRepository>();
                staffRepo = staffRepoMock.Object;
                configRepoMock = new Mock<IConfigurationRepository>();
                configRepo = configRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                _configurationRepositoryMock = new Mock<IConfigurationRepository>();
                allGeographicAreaTypes = new TestGeographicAreaTypeRepository().Get();

                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                // Mock permissions
                permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.ViewAnyPerson);
                personRole.AddPermission(permissionViewAnyPerson);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

                demographicService = new DemographicService(refRepo, personBaseRepo, adapterRegistry, currentUserFactory, roleRepo, logger, configRepo, staffRepo);
            }

            [TestCleanup]
            public void Cleanup()
            {
                refRepo = null;
                personRepo = null;
                allGeographicAreaTypes = null;
                adapterRegistry = null;
                roleRepo = null;
                logger = null;
                demographicService = null;
                _configurationRepositoryMock = null;
            }

            [TestMethod]
            public async Task GetGeographicAreaTypeByGuid_HEDM_ValidGeographicAreaTypeIdAsync()
            {
                Domain.Base.Entities.GeographicAreaType thisGeographicAreaType = allGeographicAreaTypes.Where(m => m.Guid == demographicGuid).FirstOrDefault();
                refRepoMock.Setup(repo => repo.GetGeographicAreaTypesAsync(true)).ReturnsAsync(allGeographicAreaTypes.Where(m => m.Guid == demographicGuid));
                Dtos.GeographicAreaType geographicAreaType = await demographicService.GetGeographicAreaTypeByGuidAsync(demographicGuid);
                Assert.AreEqual(thisGeographicAreaType.Guid, geographicAreaType.Id);
                Assert.AreEqual(thisGeographicAreaType.Code, geographicAreaType.Code);
                Assert.AreEqual(null, geographicAreaType.Description);
                Assert.AreEqual(thisGeographicAreaType.Description, geographicAreaType.Title);
            }


            [TestMethod]
            public async Task GetGeographicAreaTypes_HEDM_CountGeographicAreaTypesAsync()
            {
                refRepoMock.Setup(repo => repo.GetGeographicAreaTypesAsync(false)).ReturnsAsync(allGeographicAreaTypes);
                IEnumerable<Ellucian.Colleague.Dtos.GeographicAreaType> geographicAreaType = await demographicService.GetGeographicAreaTypesAsync();
                Assert.AreEqual(4, geographicAreaType.Count());
            }

            [TestMethod]
            public async Task GetGeographicAreaTypes_HEDM_CompareGeographicAreaTypesAsync()
            {
                refRepoMock.Setup(repo => repo.GetGeographicAreaTypesAsync(false)).ReturnsAsync(allGeographicAreaTypes);

                IEnumerable<Dtos.GeographicAreaType> geographicAreaTypes = await demographicService.GetGeographicAreaTypesAsync();
                Assert.AreEqual(allGeographicAreaTypes.ElementAt(0).Guid, geographicAreaTypes.ElementAt(0).Id);
                Assert.AreEqual(allGeographicAreaTypes.ElementAt(0).Code, geographicAreaTypes.ElementAt(0).Code);
                Assert.AreEqual(null, geographicAreaTypes.ElementAt(0).Description);
                Assert.AreEqual(allGeographicAreaTypes.ElementAt(0).Description, geographicAreaTypes.ElementAt(0).Title);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task DemographicService_GetGeographicAreaTypeByGuid_HEDM_ThrowsInvOpExc()
            {
                refRepoMock.Setup(repo => repo.GetGeographicAreaTypesAsync(It.IsAny<bool>())).Throws<InvalidOperationException>();
                await demographicService.GetGeographicAreaTypeByGuidAsync("dshjfkj");
            }
        }

        [TestClass]
        public class GetIdentityDocumentTypes : CurrentUserSetup
        {
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IPersonBaseRepository> personBaseRepoMock;
            private IPersonBaseRepository personBaseRepo;
            private Mock<IReferenceDataRepository> refRepoMock;
            private IReferenceDataRepository refRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private Mock<IStaffRepository> staffRepoMock;
            private IStaffRepository staffRepo;
            private Mock<IConfigurationRepository> configRepoMock;
            private IConfigurationRepository configRepo;
            private ICurrentUserFactory currentUserFactory;
            private IEnumerable<Domain.Base.Entities.IdentityDocumentType> allIdentityDocumentTypes;
            private DemographicService demographicService;
            private string demographicGuid = "4236641d-5c29-4884-9a17-530820ec9d29";
            private Mock<IConfigurationRepository> _configurationRepositoryMock;

            private Domain.Entities.Permission permissionViewAnyPerson;

            [TestInitialize]
            public void Initialize()
            {
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                personBaseRepoMock = new Mock<IPersonBaseRepository>();
                personBaseRepo = personBaseRepoMock.Object;
                refRepoMock = new Mock<IReferenceDataRepository>();
                refRepo = refRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                staffRepoMock = new Mock<IStaffRepository>();
                staffRepo = staffRepoMock.Object;
                configRepoMock = new Mock<IConfigurationRepository>();
                configRepo = configRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                _configurationRepositoryMock = new Mock<IConfigurationRepository>();
                allIdentityDocumentTypes = new TestIdentityDocumentTypeRepository().Get();

                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                // Mock permissions
                permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.ViewAnyPerson);
                personRole.AddPermission(permissionViewAnyPerson);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

                demographicService = new DemographicService(refRepo, personBaseRepo, adapterRegistry, currentUserFactory, roleRepo, logger, configRepo, staffRepo);
            }

            [TestCleanup]
            public void Cleanup()
            {
                refRepo = null;
                personRepo = null;
                allIdentityDocumentTypes = null;
                adapterRegistry = null;
                roleRepo = null;
                logger = null;
                demographicService = null;
                _configurationRepositoryMock = null;
            }

            [TestMethod]
            public async Task GetIdentityDocumentTypeByGuid_HEDM_ValidIdentityDocumentTypeIdAsync()
            {
                Domain.Base.Entities.IdentityDocumentType thisIdentityDocumentType = allIdentityDocumentTypes.Where(m => m.Guid == demographicGuid).FirstOrDefault();
                refRepoMock.Setup(repo => repo.GetIdentityDocumentTypesAsync(true)).ReturnsAsync(allIdentityDocumentTypes.Where(m => m.Guid == demographicGuid));
                Dtos.IdentityDocumentType identityDocumentType = await demographicService.GetIdentityDocumentTypeByGuidAsync(demographicGuid);
                Assert.AreEqual(thisIdentityDocumentType.Guid, identityDocumentType.Id);
                Assert.AreEqual(thisIdentityDocumentType.Code, identityDocumentType.Code);
                Assert.AreEqual(null, identityDocumentType.Description);
                Assert.AreEqual(thisIdentityDocumentType.Description, identityDocumentType.Title);
            }


            [TestMethod]
            public async Task GetIdentityDocumentTypes_HEDM_CountIdentityDocumentTypesAsync()
            {
                refRepoMock.Setup(repo => repo.GetIdentityDocumentTypesAsync(false)).ReturnsAsync(allIdentityDocumentTypes);
                IEnumerable<Ellucian.Colleague.Dtos.IdentityDocumentType> identityDocumentType = await demographicService.GetIdentityDocumentTypesAsync();
                Assert.AreEqual(allIdentityDocumentTypes.Count(), identityDocumentType.Count());
            }

            [TestMethod]
            public async Task GetIdentityDocumentTypes_HEDM_CompareIdentityDocumentTypesAsync()
            {
                refRepoMock.Setup(repo => repo.GetIdentityDocumentTypesAsync(false)).ReturnsAsync(allIdentityDocumentTypes);

                IEnumerable<Dtos.IdentityDocumentType> identityDocumentTypes = await demographicService.GetIdentityDocumentTypesAsync();
                Assert.AreEqual(allIdentityDocumentTypes.ElementAt(0).Guid, identityDocumentTypes.ElementAt(0).Id);
                Assert.AreEqual(allIdentityDocumentTypes.ElementAt(0).Code, identityDocumentTypes.ElementAt(0).Code);
                Assert.AreEqual(null, identityDocumentTypes.ElementAt(0).Description);
                Assert.AreEqual(allIdentityDocumentTypes.ElementAt(0).Description, identityDocumentTypes.ElementAt(0).Title);
            }

            
        }

        [TestClass]
        public class GetPersonFilters : CurrentUserSetup
        {
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IPersonBaseRepository> personBaseRepoMock;
            private IPersonBaseRepository personBaseRepo;
            private Mock<IReferenceDataRepository> refRepoMock;
            private IReferenceDataRepository refRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private Mock<IStaffRepository> staffRepoMock;
            private IStaffRepository staffRepo;
            private Mock<IConfigurationRepository> configRepoMock;
            private IConfigurationRepository configRepo;
            private ICurrentUserFactory currentUserFactory;
            private IEnumerable<Domain.Base.Entities.PersonFilter> allPersonFilters;
            private DemographicService demographicService;
            private string demographicGuid = "625c69ff-280b-4ed3-9474-662a43616a8a";
            private Mock<IConfigurationRepository> _configurationRepositoryMock;

            private Domain.Entities.Permission permissionViewAnyPerson;

            [TestInitialize]
            public void Initialize()
            {
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                personBaseRepoMock = new Mock<IPersonBaseRepository>();
                personBaseRepo = personBaseRepoMock.Object;
                refRepoMock = new Mock<IReferenceDataRepository>();
                refRepo = refRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                staffRepoMock = new Mock<IStaffRepository>();
                staffRepo = staffRepoMock.Object;
                configRepoMock = new Mock<IConfigurationRepository>();
                configRepo = configRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                _configurationRepositoryMock = new Mock<IConfigurationRepository>();
                allPersonFilters = new TestPersonFilterRepository().GetPersonFilters();

                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                // Mock permissions
                permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.ViewAnyPerson);
                personRole.AddPermission(permissionViewAnyPerson);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

                demographicService = new DemographicService(refRepo, personBaseRepo, adapterRegistry, currentUserFactory, roleRepo, logger, configRepo, staffRepo);
            }

            [TestCleanup]
            public void Cleanup()
            {
                refRepo = null;
                personRepo = null;
                allPersonFilters = null;
                adapterRegistry = null;
                roleRepo = null;
                logger = null;
                demographicService = null;
                _configurationRepositoryMock = null;
            }

            [TestMethod]
            public async Task GetPersonFilterByGuid_HEDM_ValidPersonFilterIdAsync()
            {
                Domain.Base.Entities.PersonFilter thisPersonFilter = allPersonFilters.Where(m => m.Guid == demographicGuid).FirstOrDefault();
                refRepoMock.Setup(repo => repo.GetPersonFiltersAsync(true)).ReturnsAsync(allPersonFilters.Where(m => m.Guid == demographicGuid));
                refRepoMock.Setup(repo => repo.GetPersonFilterByGuidAsync(It.IsAny<string>()))
                    .ReturnsAsync(allPersonFilters.FirstOrDefault(m => m.Guid == demographicGuid));

                Dtos.PersonFilter personFilter = await demographicService.GetPersonFilterByGuidAsync(demographicGuid);
                Assert.AreEqual(thisPersonFilter.Guid, personFilter.Id);
                Assert.AreEqual(thisPersonFilter.Code, personFilter.Code);
                Assert.AreEqual(null, personFilter.Description);
                Assert.AreEqual(thisPersonFilter.Description, personFilter.Title);
            }


            [TestMethod]
            public async Task GetPersonFilters_HEDM_CountPersonFiltersAsync()
            {
                refRepoMock.Setup(repo => repo.GetPersonFiltersAsync(false)).ReturnsAsync(allPersonFilters);
                IEnumerable<Ellucian.Colleague.Dtos.PersonFilter> personFilter = await demographicService.GetPersonFiltersAsync();
                Assert.AreEqual(4, personFilter.Count());
            }

            [TestMethod]
            public async Task GetPersonFilters_HEDM_ComparePersonFiltersAsync()
            {
                refRepoMock.Setup(repo => repo.GetPersonFiltersAsync(false)).ReturnsAsync(allPersonFilters);

                IEnumerable<Dtos.PersonFilter> personFilters = await demographicService.GetPersonFiltersAsync();
                Assert.AreEqual(allPersonFilters.ElementAt(0).Guid, personFilters.ElementAt(0).Id);
                Assert.AreEqual(allPersonFilters.ElementAt(0).Code, personFilters.ElementAt(0).Code);
                Assert.AreEqual(null, personFilters.ElementAt(0).Description);
                Assert.AreEqual(allPersonFilters.ElementAt(0).Description, personFilters.ElementAt(0).Title);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task DemographicService_GetPersonFilterByGuid_HEDM_ThrowsInvOpExc()
            {
                refRepoMock.Setup(repo => repo.GetPersonFiltersAsync(It.IsAny<bool>())).Throws<InvalidOperationException>();
                refRepoMock.Setup(repo => repo.GetPersonFilterByGuidAsync(It.IsAny<string>()))
                    .Throws<InvalidOperationException>();
                await demographicService.GetPersonFilterByGuidAsync("dshjfkj");
            }
        }

        [TestClass]
        public class GetPrivacyStatuses : CurrentUserSetup
        {
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IPersonBaseRepository> personBaseRepoMock;
            private IPersonBaseRepository personBaseRepo;
            private Mock<IReferenceDataRepository> refRepoMock;
            private IReferenceDataRepository refRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private Mock<IStaffRepository> staffRepoMock;
            private IStaffRepository staffRepo;
            private Mock<IConfigurationRepository> configRepoMock;
            private IConfigurationRepository configRepo;
            private ICurrentUserFactory currentUserFactory;
            private IEnumerable<Domain.Base.Entities.PrivacyStatus> allPrivacyStatuses;
            private Dictionary<string, string> allPrivacyMessages;
            private DemographicService demographicService;
            private string demographicGuid = "87ec6f69-9b16-4ed5-8954-59067f0318ec";
            private Mock<IConfigurationRepository> _configurationRepositoryMock;

            private Domain.Entities.Permission permissionViewAnyPerson;

            [TestInitialize]
            public void Initialize()
            {
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                personBaseRepoMock = new Mock<IPersonBaseRepository>();
                personBaseRepo = personBaseRepoMock.Object;
                refRepoMock = new Mock<IReferenceDataRepository>();
                refRepo = refRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                staffRepoMock = new Mock<IStaffRepository>();
                staffRepo = staffRepoMock.Object;
                configRepoMock = new Mock<IConfigurationRepository>();
                configRepo = configRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                _configurationRepositoryMock = new Mock<IConfigurationRepository>();
                allPrivacyStatuses = new TestPrivacyStatusRepository().Get();
                allPrivacyMessages = GeneratePrivacyMessages(allPrivacyStatuses);

                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                // Mock permissions
                permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.ViewAnyPerson);
                personRole.AddPermission(permissionViewAnyPerson);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

                demographicService = new DemographicService(refRepo, personBaseRepo, adapterRegistry, currentUserFactory, roleRepo, logger, configRepo, staffRepo);
            }

            [TestCleanup]
            public void Cleanup()
            {
                refRepo = null;
                personRepo = null;
                allPrivacyStatuses = null;
                allPrivacyMessages = null;
                adapterRegistry = null;
                roleRepo = null;
                logger = null;
                demographicService = null;
                _configurationRepositoryMock = null;
            }

            private Dictionary<string, string> GeneratePrivacyMessages(IEnumerable<Domain.Base.Entities.PrivacyStatus> statuses)
            {
                var messages = new Dictionary<string, string>();
                foreach (var item in statuses)
                {
                    messages.Add(item.Code, "This is a message for: " + item.Code);
                }
                return messages;
            }

            [TestMethod]
            public async Task GetPrivacyStatusByGuid_HEDM_ValidPrivacyStatusIdAsync()
            {
                Domain.Base.Entities.PrivacyStatus thisPrivacyStatus = allPrivacyStatuses.Where(m => m.Guid == demographicGuid).FirstOrDefault();
                refRepoMock.Setup(repo => repo.GetPrivacyStatusesAsync(true)).ReturnsAsync(allPrivacyStatuses.Where(m => m.Guid == demographicGuid));
                refRepoMock.Setup(repo => repo.GetPrivacyMessagesAsync()).ReturnsAsync(allPrivacyMessages);
                Dtos.PrivacyStatus privacyStatus = await demographicService.GetPrivacyStatusByGuidAsync(demographicGuid);

                string message = null;
                allPrivacyMessages.TryGetValue(allPrivacyStatuses.ElementAt(0).Code, out message);
                
                Assert.AreEqual(thisPrivacyStatus.Guid, privacyStatus.Id);
                Assert.AreEqual(thisPrivacyStatus.Code, privacyStatus.Code);
                Assert.AreEqual(message, privacyStatus.Description);
                Assert.AreEqual(thisPrivacyStatus.Description, privacyStatus.Title);
            }


            [TestMethod]
            public async Task GetPrivacyStatuses_HEDM_CountPrivacyStatusesAsync()
            {
                refRepoMock.Setup(repo => repo.GetPrivacyStatusesAsync(false)).ReturnsAsync(allPrivacyStatuses);
                IEnumerable<Ellucian.Colleague.Dtos.PrivacyStatus> privacyStatus = await demographicService.GetPrivacyStatusesAsync();
                Assert.AreEqual(4, privacyStatus.Count());
            }

            [TestMethod]
            public async Task GetPrivacyStatuses_HEDM_ComparePrivacyStatusesAsync()
            {
                refRepoMock.Setup(repo => repo.GetPrivacyStatusesAsync(false)).ReturnsAsync(allPrivacyStatuses);
                refRepoMock.Setup(repo => repo.GetPrivacyMessagesAsync()).ReturnsAsync(allPrivacyMessages);
                IEnumerable<Dtos.PrivacyStatus> privacyStatuses = await demographicService.GetPrivacyStatusesAsync();

                string message = null;
                allPrivacyMessages.TryGetValue(allPrivacyStatuses.ElementAt(0).Code, out message);

                Assert.AreEqual(allPrivacyStatuses.ElementAt(0).Guid, privacyStatuses.ElementAt(0).Id);
                Assert.AreEqual(allPrivacyStatuses.ElementAt(0).Code, privacyStatuses.ElementAt(0).Code);
                Assert.AreEqual(message, privacyStatuses.ElementAt(0).Description);
                Assert.AreEqual(allPrivacyStatuses.ElementAt(0).Description, privacyStatuses.ElementAt(0).Title);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task DemographicService_GetPrivacyStatusByGuid_HEDM_ThrowsInvOpExc()
            {
                refRepoMock.Setup(repo => repo.GetPrivacyStatusesAsync(It.IsAny<bool>())).Throws<InvalidOperationException>();
                await demographicService.GetPrivacyStatusByGuidAsync("dshjfkj");
            }
        }

        [TestClass]
        public class GetRaces : CurrentUserSetup
        {
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IPersonBaseRepository> personBaseRepoMock;
            private IPersonBaseRepository personBaseRepo;
            private Mock<IReferenceDataRepository> refRepoMock;
            private IReferenceDataRepository refRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private Mock<IStaffRepository> staffRepoMock;
            private IStaffRepository staffRepo;
            private Mock<IConfigurationRepository> configRepoMock;
            private IConfigurationRepository configRepo;
            private ICurrentUserFactory currentUserFactory;
            private IEnumerable<Domain.Base.Entities.Race> allRaces;
            private DemographicService demographicService;
            private string demographicGuid = "87ec6f69-9b16-4ed5-8954-59067f0318ec";
            private Mock<IConfigurationRepository> _configurationRepositoryMock;

            private Domain.Entities.Permission permissionViewAnyPerson;

            [TestInitialize]
            public void Initialize()
            {
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                personBaseRepoMock = new Mock<IPersonBaseRepository>();
                personBaseRepo = personBaseRepoMock.Object;
                refRepoMock = new Mock<IReferenceDataRepository>();
                refRepo = refRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                staffRepoMock = new Mock<IStaffRepository>();
                staffRepo = staffRepoMock.Object;
                configRepoMock = new Mock<IConfigurationRepository>();
                configRepo = configRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                _configurationRepositoryMock = new Mock<IConfigurationRepository>();
                allRaces = new TestRaceRepository().Get();

                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                // Mock permissions
                permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.ViewAnyPerson);
                personRole.AddPermission(permissionViewAnyPerson);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

                demographicService = new DemographicService(refRepo, personBaseRepo, adapterRegistry, currentUserFactory, roleRepo, logger, configRepo, staffRepo);
            }

            [TestCleanup]
            public void Cleanup()
            {
                refRepo = null;
                personRepo = null;
                allRaces = null;
                adapterRegistry = null;
                roleRepo = null;
                logger = null;
                demographicService = null;
                _configurationRepositoryMock = null;
            }

            [TestMethod]
            public async Task GetRaceByGuid_HEDM_ValidRaceIdAsync()
            {
                Domain.Base.Entities.Race thisRace = allRaces.Where(m => m.Guid == demographicGuid).FirstOrDefault();
                refRepoMock.Setup(repo => repo.GetRacesAsync(true)).ReturnsAsync(allRaces.Where(m => m.Guid == demographicGuid));
                Dtos.Race2 race = await demographicService.GetRaceById2Async(demographicGuid);
                Assert.AreEqual(thisRace.Guid, race.Id);
                Assert.AreEqual(thisRace.Code, race.Code);
                Assert.AreEqual(null, race.Description);
                Assert.AreEqual(thisRace.Description, race.Title);
            }


            [TestMethod]
            public async Task GetRaces_HEDM_CountRacesAsync()
            {
                refRepoMock.Setup(repo => repo.GetRacesAsync(false)).ReturnsAsync(allRaces);
                IEnumerable<Ellucian.Colleague.Dtos.Race2> race = await demographicService.GetRaces2Async();
                Assert.AreEqual(5, race.Count());
            }

            [TestMethod]
            public async Task GetRaces_HEDM_CompareRacesMajorsAsync()
            {
                refRepoMock.Setup(repo => repo.GetRacesAsync(false)).ReturnsAsync(allRaces);

                IEnumerable<Dtos.Race2> races = await demographicService.GetRaces2Async();
                Assert.AreEqual(allRaces.ElementAt(0).Guid, races.ElementAt(0).Id);
                Assert.AreEqual(allRaces.ElementAt(0).Code, races.ElementAt(0).Code);
                Assert.AreEqual(null, races.ElementAt(0).Description);
                Assert.AreEqual(allRaces.ElementAt(0).Description, races.ElementAt(0).Title);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task DemographicService_GetRaceById2_HEDM_ThrowsInvOpExc()
            {
                refRepoMock.Setup(repo => repo.GetRacesAsync(It.IsAny<bool>())).Throws<InvalidOperationException>();
                await demographicService.GetRaceById2Async("dshjfkj");
            }

            [TestMethod]
            public async Task GetRaceByGuid_CDM_ValidRaceIdAsync()
            {
                Domain.Base.Entities.Race thisRace = allRaces.Where(m => m.Guid == demographicGuid).FirstOrDefault();
                refRepoMock.Setup(repo => repo.GetRacesAsync(true)).ReturnsAsync(allRaces.Where(m => m.Guid == demographicGuid));
                Dtos.Race race = await demographicService.GetRaceByGuidAsync(demographicGuid);
                Assert.AreEqual(thisRace.Guid, race.Guid);
                Assert.AreEqual(thisRace.Code, race.Abbreviation);
                Assert.AreEqual(null, race.Description);
                Assert.AreEqual(thisRace.Description, race.Title);
            }


            [TestMethod]
            public async Task GetRaces_CDM_CountRacesAsync()
            {
                refRepoMock.Setup(repo => repo.GetRacesAsync(false)).ReturnsAsync(allRaces);
                IEnumerable<Ellucian.Colleague.Dtos.Race> race = await demographicService.GetRacesAsync();
                Assert.AreEqual(5, race.Count());
            }

            [TestMethod]
            public async Task GetRaces_CDM_CompareRacesMajorsAsync()
            {
                refRepoMock.Setup(repo => repo.GetRacesAsync(false)).ReturnsAsync(allRaces);

                IEnumerable<Dtos.Race> races = await demographicService.GetRacesAsync();
                Assert.AreEqual(allRaces.ElementAt(0).Guid, races.ElementAt(0).Guid);
                Assert.AreEqual(allRaces.ElementAt(0).Code, races.ElementAt(0).Abbreviation);
                Assert.AreEqual(null, races.ElementAt(0).Description);
                Assert.AreEqual(allRaces.ElementAt(0).Description, races.ElementAt(0).Title);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task DemographicService_GetRaceByGuid_CDM_ThrowsInvOpExc()
            {
                refRepoMock.Setup(repo => repo.GetRacesAsync(It.IsAny<bool>())).Throws<InvalidOperationException>();
                await demographicService.GetRaceByGuidAsync("dshjfkj");
            }
        }

        [TestClass]
        public class GetReligions : CurrentUserSetup
        {
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IPersonBaseRepository> personBaseRepoMock;
            private IPersonBaseRepository personBaseRepo;
            private Mock<IReferenceDataRepository> refRepoMock;
            private IReferenceDataRepository refRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private Mock<IStaffRepository> staffRepoMock;
            private IStaffRepository staffRepo;
            private Mock<IConfigurationRepository> configRepoMock;
            private IConfigurationRepository configRepo;
            private ICurrentUserFactory currentUserFactory;
            private IEnumerable<Domain.Base.Entities.Denomination> allDenominations;
            private DemographicService demographicService;
            private string demographicGuid = "9ae3a175-1dfd-4937-b97b-3c9ad596e023";
            private Mock<IConfigurationRepository> _configurationRepositoryMock;

            private Domain.Entities.Permission permissionViewAnyPerson;

            [TestInitialize]
            public void Initialize()
            {
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                personBaseRepoMock = new Mock<IPersonBaseRepository>();
                personBaseRepo = personBaseRepoMock.Object;
                refRepoMock = new Mock<IReferenceDataRepository>();
                refRepo = refRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                staffRepoMock = new Mock<IStaffRepository>();
                staffRepo = staffRepoMock.Object;
                configRepoMock = new Mock<IConfigurationRepository>();
                configRepo = configRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                _configurationRepositoryMock = new Mock<IConfigurationRepository>();
                allDenominations = new TestReligionRepository().GetDenominations();

                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                // Mock permissions
                permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.ViewAnyPerson);
                personRole.AddPermission(permissionViewAnyPerson);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

                demographicService = new DemographicService(refRepo, personBaseRepo, adapterRegistry, currentUserFactory, roleRepo, logger, configRepo, staffRepo);
            }

            [TestCleanup]
            public void Cleanup()
            {
                refRepo = null;
                personRepo = null;
                allDenominations = null;
                adapterRegistry = null;
                roleRepo = null;
                logger = null;
                demographicService = null;
                _configurationRepositoryMock = null;
            }

            [TestMethod]
            public async Task GetReligionByGuid_ValidReligionIdAsync()
            {
                Domain.Base.Entities.Denomination thisDenomination = allDenominations.Where(m => m.Guid == demographicGuid).FirstOrDefault();
                refRepoMock.Setup(repo => repo.GetDenominationsAsync(true)).ReturnsAsync(allDenominations.Where(m => m.Guid == demographicGuid));
                Dtos.Religion religion = await demographicService.GetReligionByIdAsync(demographicGuid);
                Assert.AreEqual(thisDenomination.Guid, religion.Id);
                Assert.AreEqual(thisDenomination.Code, religion.Code);
                Assert.AreEqual(null, religion.Description);
                Assert.AreEqual(thisDenomination.Description, religion.Title);
            }


            [TestMethod]
            public async Task GetDenominations_CountReligionsAsync()
            {
                refRepoMock.Setup(repo => repo.GetDenominationsAsync(false)).ReturnsAsync(allDenominations);
                IEnumerable<Ellucian.Colleague.Dtos.Religion> religion = await demographicService.GetReligionsAsync();
                Assert.AreEqual(4, religion.Count());
            }

            [TestMethod]
            public async Task GetDenominations_CompareReligionsAsync()
            {
                refRepoMock.Setup(repo => repo.GetDenominationsAsync(false)).ReturnsAsync(allDenominations);

                IEnumerable<Dtos.Religion> religions = await demographicService.GetReligionsAsync();
                Assert.AreEqual(allDenominations.ElementAt(0).Guid, religions.ElementAt(0).Id);
                Assert.AreEqual(allDenominations.ElementAt(0).Code, religions.ElementAt(0).Code);
                Assert.AreEqual(null, religions.ElementAt(0).Description);
                Assert.AreEqual(allDenominations.ElementAt(0).Description, religions.ElementAt(0).Title);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task DemographicService_GetReligionById_HEDM_ThrowsInvOpExc()
            {
                refRepoMock.Setup(repo => repo.GetDenominationsAsync(It.IsAny<bool>())).Throws<InvalidOperationException>();
                await demographicService.GetReligionByIdAsync("dshjfkj");
            }
        }

        [TestClass]
        public class GetSocialMediaTypes : CurrentUserSetup
        {
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IPersonBaseRepository> personBaseRepoMock;
            private IPersonBaseRepository personBaseRepo;
            private Mock<IReferenceDataRepository> refRepoMock;
            private IReferenceDataRepository refRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private Mock<IStaffRepository> staffRepoMock;
            private IStaffRepository staffRepo;
            private Mock<IConfigurationRepository> configRepoMock;
            private IConfigurationRepository configRepo;
            private ICurrentUserFactory currentUserFactory;
            private IEnumerable<Domain.Base.Entities.SocialMediaType> allSocialMediaTypes;
            private DemographicService demographicService;
            private string demographicGuid = "31d8aa32-dbe6-4a49-a1c4-2cad39e232e4";
            private Mock<IConfigurationRepository> _configurationRepositoryMock;

            private Domain.Entities.Permission permissionViewAnyPerson;

            [TestInitialize]
            public void Initialize()
            {
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                personBaseRepoMock = new Mock<IPersonBaseRepository>();
                personBaseRepo = personBaseRepoMock.Object;
                refRepoMock = new Mock<IReferenceDataRepository>();
                refRepo = refRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                staffRepoMock = new Mock<IStaffRepository>();
                staffRepo = staffRepoMock.Object;
                configRepoMock = new Mock<IConfigurationRepository>();
                configRepo = configRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                _configurationRepositoryMock = new Mock<IConfigurationRepository>();
                allSocialMediaTypes = new TestSocialMediaTypesRepository().GetSocialMediaTypes();

                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                // Mock permissions
                permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.ViewAnyPerson);
                personRole.AddPermission(permissionViewAnyPerson);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

                demographicService = new DemographicService(refRepo, personBaseRepo, adapterRegistry, currentUserFactory, roleRepo, logger, configRepo, staffRepo);
            }

            [TestCleanup]
            public void Cleanup()
            {
                refRepo = null;
                personRepo = null;
                allSocialMediaTypes = null;
                adapterRegistry = null;
                roleRepo = null;
                logger = null;
                demographicService = null;
                _configurationRepositoryMock = null;
            }

            [TestMethod]
            public async Task DemographicService_GetSocialMediaTypeByIdAsync()
            {
                Domain.Base.Entities.SocialMediaType thisSocialMediaType = allSocialMediaTypes.Where(m => m.Guid == demographicGuid).FirstOrDefault();
                refRepoMock.Setup(repo => repo.GetSocialMediaTypesAsync(true)).ReturnsAsync(allSocialMediaTypes.Where(m => m.Guid == demographicGuid));
                Dtos.SocialMediaType socialMediaType = await demographicService.GetSocialMediaTypeByIdAsync(demographicGuid);
                Assert.AreEqual(thisSocialMediaType.Guid, socialMediaType.Id);
                Assert.AreEqual(thisSocialMediaType.Code, socialMediaType.Code);
                Assert.AreEqual(null, socialMediaType.Description);
                Assert.AreEqual(Dtos.SocialMediaTypeCategory.facebook, socialMediaType.SocialMediaTypeCategory);
            }


            [TestMethod]
            public async Task DemographicService_GetSocialMediaTypesAsync_Count_Cache()
            {
                refRepoMock.Setup(repo => repo.GetSocialMediaTypesAsync(false)).ReturnsAsync(allSocialMediaTypes);
                IEnumerable<Ellucian.Colleague.Dtos.SocialMediaType> socialMediaType = await demographicService.GetSocialMediaTypesAsync(false);
                Assert.AreEqual(allSocialMediaTypes.Count(), socialMediaType.Count());
            }

            [TestMethod]
            public async Task DemographicService_GetSocialMediaTypesAsync_Cache()
            {
                refRepoMock.Setup(repo => repo.GetSocialMediaTypesAsync(false)).ReturnsAsync(allSocialMediaTypes);

                IEnumerable<Dtos.SocialMediaType> socialMediaTypes = await demographicService.GetSocialMediaTypesAsync(false);
                Assert.AreEqual(allSocialMediaTypes.ElementAt(0).Guid, socialMediaTypes.ElementAt(0).Id);
                Assert.AreEqual(allSocialMediaTypes.ElementAt(0).Code, socialMediaTypes.ElementAt(0).Code);
                Assert.AreEqual(null, socialMediaTypes.ElementAt(0).Description);
                Assert.AreEqual(allSocialMediaTypes.ElementAt(0).Description, socialMediaTypes.ElementAt(0).Title);
            }


            [TestMethod]
            public async Task DemographicService_GetSocialMediaTypesAsync_Count_NonCache()
            {
                refRepoMock.Setup(repo => repo.GetSocialMediaTypesAsync(true)).ReturnsAsync(allSocialMediaTypes);
                IEnumerable<Ellucian.Colleague.Dtos.SocialMediaType> socialMediaType = await demographicService.GetSocialMediaTypesAsync(true);
                Assert.AreEqual(allSocialMediaTypes.Count(), socialMediaType.Count());
            }

            [TestMethod]
            public async Task DemographicService_GetSocialMediaTypesAsync_NonCache()
            {
                refRepoMock.Setup(repo => repo.GetSocialMediaTypesAsync(true)).ReturnsAsync(allSocialMediaTypes);

                IEnumerable<Dtos.SocialMediaType> socialMediaTypes = await demographicService.GetSocialMediaTypesAsync(true);
                Assert.AreEqual(allSocialMediaTypes.ElementAt(0).Guid, socialMediaTypes.ElementAt(0).Id);
                Assert.AreEqual(allSocialMediaTypes.ElementAt(0).Code, socialMediaTypes.ElementAt(0).Code);
                Assert.AreEqual(null, socialMediaTypes.ElementAt(0).Description);
                Assert.AreEqual(allSocialMediaTypes.ElementAt(0).Description, socialMediaTypes.ElementAt(0).Title);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task DemographicService_GetSocialMediaTypeByIdAsync_ThrowsInvOpExc()
            {
                refRepoMock.Setup(repo => repo.GetSocialMediaTypesAsync(It.IsAny<bool>())).Throws<KeyNotFoundException>();
                await demographicService.GetSocialMediaTypeByIdAsync("dshjfkj");
            }
        }

        [TestClass]
        public class GetSourceContext : CurrentUserSetup
        {
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IPersonBaseRepository> personBaseRepoMock;
            private IPersonBaseRepository personBaseRepo;
            private Mock<IReferenceDataRepository> refRepoMock;
            private IReferenceDataRepository refRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private Mock<IStaffRepository> staffRepoMock;
            private IStaffRepository staffRepo;
            private Mock<IConfigurationRepository> configRepoMock;
            private IConfigurationRepository configRepo;
            private ICurrentUserFactory currentUserFactory;
            private IEnumerable<Domain.Base.Entities.SourceContext> allSourceContexts;
            private DemographicService demographicService;
            private string demographicGuid = "b3bf4128-259a-4623-8731-f519c0ae933c";
            private Mock<IConfigurationRepository> _configurationRepositoryMock;

            private Domain.Entities.Permission permissionViewAnyPerson;

            [TestInitialize]
            public void Initialize()
            {
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                personBaseRepoMock = new Mock<IPersonBaseRepository>();
                personBaseRepo = personBaseRepoMock.Object;
                refRepoMock = new Mock<IReferenceDataRepository>();
                refRepo = refRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                staffRepoMock = new Mock<IStaffRepository>();
                staffRepo = staffRepoMock.Object;
                configRepoMock = new Mock<IConfigurationRepository>();
                configRepo = configRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                _configurationRepositoryMock = new Mock<IConfigurationRepository>();
                allSourceContexts = new TestSourceContextRepository().GetSourceContexts();

                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                // Mock permissions
                permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.ViewAnyPerson);
                personRole.AddPermission(permissionViewAnyPerson);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

                demographicService = new DemographicService(refRepo, personBaseRepo, adapterRegistry, currentUserFactory, roleRepo, logger, configRepo, staffRepo);
            }

            [TestCleanup]
            public void Cleanup()
            {
                refRepo = null;
                personRepo = null;
                allSourceContexts = null;
                adapterRegistry = null;
                roleRepo = null;
                logger = null;
                demographicService = null;
                _configurationRepositoryMock = null;
            }

            [TestMethod]
            public async Task DemographicService_GetSourceContextsById()
            {
                Domain.Base.Entities.SourceContext thisSourceContext = allSourceContexts.Where(m => m.Guid == demographicGuid).FirstOrDefault();
                refRepoMock.Setup(repo => repo.GetSourceContextsAsync(true)).ReturnsAsync(allSourceContexts.Where(m => m.Guid == demographicGuid));
                Dtos.SourceContext sourceContext = await demographicService.GetSourceContextsByIdAsync(demographicGuid);
                Assert.AreEqual(thisSourceContext.Guid, sourceContext.Id);
                Assert.AreEqual(thisSourceContext.Code, sourceContext.Code);
                Assert.AreEqual(null, sourceContext.Description);
            }


            [TestMethod]
            public async Task DemographicService_GetSourceContexts_Count_Cache()
            {
                refRepoMock.Setup(repo => repo.GetSourceContextsAsync(false)).ReturnsAsync(allSourceContexts);
                IEnumerable<Ellucian.Colleague.Dtos.SourceContext> soureContexts = await demographicService.GetSourceContextsAsync(false);
                Assert.AreEqual(allSourceContexts.Count(), soureContexts.Count());
            }

            [TestMethod]
            public async Task DemographicService_GetSourceContexts_Cache()
            {
                refRepoMock.Setup(repo => repo.GetSourceContextsAsync(false)).ReturnsAsync(allSourceContexts);

                IEnumerable<Dtos.SourceContext> sourceContexts = await demographicService.GetSourceContextsAsync(false);
                Assert.AreEqual(allSourceContexts.ElementAt(0).Guid, sourceContexts.ElementAt(0).Id);
                Assert.AreEqual(allSourceContexts.ElementAt(0).Code, sourceContexts.ElementAt(0).Code);
                Assert.AreEqual(null, sourceContexts.ElementAt(0).Description);
                Assert.AreEqual(allSourceContexts.ElementAt(0).Description, sourceContexts.ElementAt(0).Title);
            }


            [TestMethod]
            public async Task DemographicService_GetSourceContexts_Count_NonCache()
            {
                refRepoMock.Setup(repo => repo.GetSourceContextsAsync(true)).ReturnsAsync(allSourceContexts);
                IEnumerable<Ellucian.Colleague.Dtos.SourceContext> soureContexts = await demographicService.GetSourceContextsAsync(true);
                Assert.AreEqual(allSourceContexts.Count(), soureContexts.Count());
            }

            [TestMethod]
            public async Task DemographicService_GetSourceContexts_NonCache()
            {
                refRepoMock.Setup(repo => repo.GetSourceContextsAsync(true)).ReturnsAsync(allSourceContexts);

                IEnumerable<Dtos.SourceContext> sourceContexts = await demographicService.GetSourceContextsAsync(true);
                Assert.AreEqual(allSourceContexts.ElementAt(0).Guid, sourceContexts.ElementAt(0).Id);
                Assert.AreEqual(allSourceContexts.ElementAt(0).Code, sourceContexts.ElementAt(0).Code);
                Assert.AreEqual(null, sourceContexts.ElementAt(0).Description);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task DemographicService_GetSourceContextsById_ThrowsInvOpExc()
            {
                refRepoMock.Setup(repo => repo.GetSourceContextsAsync(It.IsAny<bool>())).Throws<KeyNotFoundException>();
                await demographicService.GetSourceContextsByIdAsync("dshjfkj");
            }
        }

        [TestClass]
        public class GetVisaTypes : CurrentUserSetup
        {
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IPersonBaseRepository> personBaseRepoMock;
            private IPersonBaseRepository personBaseRepo;
            private Mock<IReferenceDataRepository> refRepoMock;
            private IReferenceDataRepository refRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private Mock<IStaffRepository> staffRepoMock;
            private IStaffRepository staffRepo;
            private Mock<IConfigurationRepository> configRepoMock;
            private IConfigurationRepository configRepo;
            private ICurrentUserFactory currentUserFactory;
            private IEnumerable<Domain.Base.Entities.VisaTypeGuidItem> allVisaTypes;
            private DemographicService demographicService;
            private string demographicGuid = "9ae3a175-1dfd-4937-b97b-3c9ad596e023";
            private Mock<IConfigurationRepository> _configurationRepositoryMock;

            private Domain.Entities.Permission permissionViewAnyPerson;

            [TestInitialize]
            public void Initialize()
            {
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                personBaseRepoMock = new Mock<IPersonBaseRepository>();
                personBaseRepo = personBaseRepoMock.Object;
                refRepoMock = new Mock<IReferenceDataRepository>();
                refRepo = refRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                staffRepoMock = new Mock<IStaffRepository>();
                staffRepo = staffRepoMock.Object;
                configRepoMock = new Mock<IConfigurationRepository>();
                configRepo = configRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                _configurationRepositoryMock = new Mock<IConfigurationRepository>();
                allVisaTypes = new TestVisaTypeRepository().GetVisaTypes();

                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                // Mock permissions
                permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.ViewAnyPerson);
                personRole.AddPermission(permissionViewAnyPerson);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

                demographicService = new DemographicService(refRepo, personBaseRepo, adapterRegistry, currentUserFactory, roleRepo, logger, configRepo, staffRepo);
            }

            [TestCleanup]
            public void Cleanup()
            {
                refRepo = null;
                personRepo = null;
                allVisaTypes = null;
                adapterRegistry = null;
                roleRepo = null;
                logger = null;
                demographicService = null;
                _configurationRepositoryMock = null;
            }

            [TestMethod]
            public async Task DemographicService_GetVisaTypeByIdAsync()
            {
                Domain.Base.Entities.VisaTypeGuidItem thisVisaType = allVisaTypes.Where(m => m.Guid == demographicGuid).FirstOrDefault();
                refRepoMock.Setup(repo => repo.GetVisaTypesAsync(true)).ReturnsAsync(allVisaTypes.Where(m => m.Guid == demographicGuid));
                Dtos.VisaType visaType = await demographicService.GetVisaTypeByIdAsync(demographicGuid);
                Assert.AreEqual(thisVisaType.Guid, visaType.Id);
                Assert.AreEqual(thisVisaType.Code, visaType.Code);
                Assert.AreEqual(null, visaType.Description);
                Assert.AreEqual(Dtos.VisaTypeCategory.Immigrant, visaType.VisaTypeCategory);
            }


            [TestMethod]
            public async Task DemographicService_GetVisaTypesAsync_Count_Cache()
            {
                refRepoMock.Setup(repo => repo.GetVisaTypesAsync(false)).ReturnsAsync(allVisaTypes);
                IEnumerable<Ellucian.Colleague.Dtos.VisaType> visaType = await demographicService.GetVisaTypesAsync(false);
                Assert.AreEqual(allVisaTypes.Count(), visaType.Count());
            }

            [TestMethod]
            public async Task DemographicService_GetVisaTypesAsync_Cache()
            {
                refRepoMock.Setup(repo => repo.GetVisaTypesAsync(false)).ReturnsAsync(allVisaTypes);

                IEnumerable<Dtos.VisaType> visaTypes = await demographicService.GetVisaTypesAsync(false);
                Assert.AreEqual(allVisaTypes.ElementAt(0).Guid, visaTypes.ElementAt(0).Id);
                Assert.AreEqual(allVisaTypes.ElementAt(0).Code, visaTypes.ElementAt(0).Code);
                Assert.AreEqual(null, visaTypes.ElementAt(0).Description);
                Assert.AreEqual(allVisaTypes.ElementAt(0).Description, visaTypes.ElementAt(0).Title);
            }


            [TestMethod]
            public async Task DemographicService_GetVisaTypesAsync_Count_NonCache()
            {
                refRepoMock.Setup(repo => repo.GetVisaTypesAsync(true)).ReturnsAsync(allVisaTypes);
                IEnumerable<Ellucian.Colleague.Dtos.VisaType> visaType = await demographicService.GetVisaTypesAsync(true);
                Assert.AreEqual(allVisaTypes.Count(), visaType.Count());
            }

            [TestMethod]
            public async Task DemographicService_GetVisaTypesAsync_NonCache()
            {
                refRepoMock.Setup(repo => repo.GetVisaTypesAsync(true)).ReturnsAsync(allVisaTypes);

                IEnumerable<Dtos.VisaType> visaTypes = await demographicService.GetVisaTypesAsync(true);
                Assert.AreEqual(allVisaTypes.ElementAt(0).Guid, visaTypes.ElementAt(0).Id);
                Assert.AreEqual(allVisaTypes.ElementAt(0).Code, visaTypes.ElementAt(0).Code);
                Assert.AreEqual(null, visaTypes.ElementAt(0).Description);
                Assert.AreEqual(allVisaTypes.ElementAt(0).Description, visaTypes.ElementAt(0).Title);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task DemographicService_GetVisaTypeByIdAsync_ThrowsInvOpExc()
            {
                refRepoMock.Setup(repo => repo.GetVisaTypesAsync(It.IsAny<bool>())).Throws<KeyNotFoundException>();
                await demographicService.GetVisaTypeByIdAsync("dshjfkj");
            }
        }

        [TestClass]
        public class GetMaritalStatuses : CurrentUserSetup
        {
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IPersonBaseRepository> personBaseRepoMock;
            private IPersonBaseRepository personBaseRepo;
            private Mock<IReferenceDataRepository> refRepoMock;
            private IReferenceDataRepository refRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private Mock<IStaffRepository> staffRepoMock;
            private IStaffRepository staffRepo;
            private Mock<IConfigurationRepository> configRepoMock;
            private IConfigurationRepository configRepo;
            private ICurrentUserFactory currentUserFactory;
            private IEnumerable<Domain.Base.Entities.MaritalStatus> allMaritalStatuses;
            private DemographicService demographicService;
            private string demographicGuid = "87ec6f69-9b16-4ed5-8954-59067f0318ec";
            private Mock<IConfigurationRepository> _configurationRepositoryMock;

            private Domain.Entities.Permission permissionViewAnyPerson;

            [TestInitialize]
            public void Initialize()
            {
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                personBaseRepoMock = new Mock<IPersonBaseRepository>();
                personBaseRepo = personBaseRepoMock.Object;
                refRepoMock = new Mock<IReferenceDataRepository>();
                refRepo = refRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                staffRepoMock = new Mock<IStaffRepository>();
                staffRepo = staffRepoMock.Object;
                configRepoMock = new Mock<IConfigurationRepository>();
                configRepo = configRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                _configurationRepositoryMock = new Mock<IConfigurationRepository>();
                allMaritalStatuses = new TestMaritalStatusRepository().Get();

                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                // Mock permissions
                permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.ViewAnyPerson);
                personRole.AddPermission(permissionViewAnyPerson);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

                demographicService = new DemographicService(refRepo, personBaseRepo, adapterRegistry, currentUserFactory, roleRepo, logger, configRepo, staffRepo);
            }

            [TestCleanup]
            public void Cleanup()
            {
                refRepo = null;
                personRepo = null;
                allMaritalStatuses = null;
                adapterRegistry = null;
                roleRepo = null;
                logger = null;
                demographicService = null;
                _configurationRepositoryMock = null;
            }

            [TestMethod]
            public async Task GetMaritalStatusByGuid_HEDM_ValidMaritalStatusIdAsync()
            {
                Domain.Base.Entities.MaritalStatus thisMaritalStatus = allMaritalStatuses.Where(m => m.Guid == demographicGuid).FirstOrDefault();
                refRepoMock.Setup(repo => repo.GetMaritalStatusesAsync(true)).ReturnsAsync(allMaritalStatuses.Where(m => m.Guid == demographicGuid));
                Dtos.MaritalStatus2 maritalStatus = await demographicService.GetMaritalStatusById2Async(demographicGuid);
                Assert.AreEqual(thisMaritalStatus.Guid, maritalStatus.Id);
                Assert.AreEqual(thisMaritalStatus.Code, maritalStatus.Code);
                Assert.AreEqual(null, maritalStatus.Description);
                Assert.AreEqual(thisMaritalStatus.Description, maritalStatus.Title);
            }


            [TestMethod]
            public async Task GetMaritalStatuses_HEDM_CountMaritalStatusesAsync()
            {
                refRepoMock.Setup(repo => repo.GetMaritalStatusesAsync(false)).ReturnsAsync(allMaritalStatuses);
                IEnumerable<Ellucian.Colleague.Dtos.MaritalStatus2> race = await demographicService.GetMaritalStatuses2Async();
                Assert.AreEqual(5, race.Count());
            }

            [TestMethod]
            public async Task GetMaritalStatuses_HEDM_CompareMaritalStatusesMajorsAsync()
            {
                refRepoMock.Setup(repo => repo.GetMaritalStatusesAsync(false)).ReturnsAsync(allMaritalStatuses);

                IEnumerable<Dtos.MaritalStatus2> maritalStatuses = await demographicService.GetMaritalStatuses2Async();
                Assert.AreEqual(allMaritalStatuses.ElementAt(0).Guid, maritalStatuses.ElementAt(0).Id);
                Assert.AreEqual(allMaritalStatuses.ElementAt(0).Code, maritalStatuses.ElementAt(0).Code);
                Assert.AreEqual(null, maritalStatuses.ElementAt(0).Description);
                Assert.AreEqual(allMaritalStatuses.ElementAt(0).Description, maritalStatuses.ElementAt(0).Title);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task DemographicService_GetMaritalStatusById2_HEDM_ThrowsInvOpExc()
            {
                refRepoMock.Setup(repo => repo.GetMaritalStatusesAsync(It.IsAny<bool>())).Throws<InvalidOperationException>();
                await demographicService.GetMaritalStatusById2Async("dshjfkj");
            }

            [TestMethod]
            public async Task GetMaritalStatusByGuid_CDM_ValidMaritalStatusIdAsync()
            {
                Domain.Base.Entities.MaritalStatus thisMaritalStatus = allMaritalStatuses.Where(m => m.Guid == demographicGuid).FirstOrDefault();
                refRepoMock.Setup(repo => repo.GetMaritalStatusesAsync(true)).ReturnsAsync(allMaritalStatuses.Where(m => m.Guid == demographicGuid));
                Dtos.MaritalStatus maritalStatus = await demographicService.GetMaritalStatusByGuidAsync(demographicGuid);
                Assert.AreEqual(thisMaritalStatus.Guid, maritalStatus.Guid);
                Assert.AreEqual(thisMaritalStatus.Code, maritalStatus.Abbreviation);
                Assert.AreEqual(null, maritalStatus.Description);
                Assert.AreEqual(thisMaritalStatus.Description, maritalStatus.Title);
            }


            [TestMethod]
            public async Task GetMaritalStatuses_CDM_CountMaritalStatusesAsync()
            {
                refRepoMock.Setup(repo => repo.GetMaritalStatusesAsync(false)).ReturnsAsync(allMaritalStatuses);
                IEnumerable<Ellucian.Colleague.Dtos.MaritalStatus> maritalStatus = await demographicService.GetMaritalStatusesAsync();
                Assert.AreEqual(5, maritalStatus.Count());
            }

            [TestMethod]
            public async Task GetMaritalStatuses_CDM_CompareMaritalStatusesMajorsAsync()
            {
                refRepoMock.Setup(repo => repo.GetMaritalStatusesAsync(false)).ReturnsAsync(allMaritalStatuses);

                IEnumerable<Dtos.MaritalStatus> maritalStatuses = await demographicService.GetMaritalStatusesAsync();
                Assert.AreEqual(allMaritalStatuses.ElementAt(0).Guid, maritalStatuses.ElementAt(0).Guid);
                Assert.AreEqual(allMaritalStatuses.ElementAt(0).Code, maritalStatuses.ElementAt(0).Abbreviation);
                Assert.AreEqual(null, maritalStatuses.ElementAt(0).Description);
                Assert.AreEqual(allMaritalStatuses.ElementAt(0).Description, maritalStatuses.ElementAt(0).Title);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task DemographicService_GetMaritalStatusByGuid_CDM_ThrowsInvOpExc()
            {
                refRepoMock.Setup(repo => repo.GetMaritalStatusesAsync(It.IsAny<bool>())).Throws<InvalidOperationException>();
                await demographicService.GetMaritalStatusByGuidAsync("dshjfkj");
            }
        }
    }
}
