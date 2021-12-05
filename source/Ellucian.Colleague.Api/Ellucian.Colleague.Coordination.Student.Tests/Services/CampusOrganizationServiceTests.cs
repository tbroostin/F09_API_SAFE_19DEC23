// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Data.Student.Repositories;
using CampusOrgEntity = Ellucian.Colleague.Domain.Student.Entities.CampusOrganization;
using CampusOrgDto = Ellucian.Colleague.Dtos.CampusOrganization;
using CampusOrgType = Ellucian.Colleague.Domain.Student.Entities.CampusOrganizationType;
using CampusInvlEntity = Ellucian.Colleague.Domain.Student.Entities.CampusInvolvement;
using CampusInvRolesEntity = Ellucian.Colleague.Domain.Student.Entities.CampusInvRole;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Coordination.Student.Tests.UserFactories;


namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class CampusOrganizationServiceTests
    {
        [TestClass]
        public class CampusOrganizations
        {
            Mock<IPersonBaseRepository> personBaseRepositoryMock;
            Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
            Mock<IPersonRepository> personRepositoryMock;
            Mock<ICampusOrganizationRepository> campusOrganizationRepositoryMock;
            private ILogger logger;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;

            CampusOrganizationService campusOrganizationService;
            List<CampusOrgEntity> campusOrganizationEntities = new List<CampusOrgEntity>();

            List<CampusOrgDto> campusOrganizationsDtos = new List<CampusOrgDto>();
            List<CampusOrgType> campusOrganizationTypes = new List<CampusOrgType>();
            List<CampusOrgType> filteredCampusOrgTypes = new List<CampusOrgType>();

            IDictionary<string, string> parentOrganizationIds = new Dictionary<string, string>();
            List<string> campusOrganizationTypeIds;

            [TestInitialize]
            public void Initialize()
            {
                personBaseRepositoryMock = new Mock<IPersonBaseRepository>();
                studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                personRepositoryMock = new Mock<IPersonRepository>();
                campusOrganizationRepositoryMock = new Mock<ICampusOrganizationRepository>();
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;
                logger = new Mock<ILogger>().Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;

                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                BuildData();

                campusOrganizationService = new CampusOrganizationService(adapterRegistry, personBaseRepositoryMock.Object,
                    campusOrganizationRepositoryMock.Object, studentReferenceDataRepositoryMock.Object, personRepositoryMock.Object, baseConfigurationRepository, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                campusOrganizationsDtos = null;
                campusOrganizationEntities = null;
                campusOrganizationRepositoryMock = null;
                studentReferenceDataRepositoryMock = null;
            }

            [TestMethod]
            public async Task CampusOrganizationService__GetCampusOrganizationsAsync()
            {
                campusOrganizationRepositoryMock.Setup(i => i.GetCampusOrganizationsAsync(It.IsAny<bool>())).ReturnsAsync(campusOrganizationEntities);
                studentReferenceDataRepositoryMock.Setup(i => i.GetCampusOrganizationTypesAsync(It.IsAny<bool>())).ReturnsAsync(filteredCampusOrgTypes);
                personBaseRepositoryMock.SetupSequence(i => i.GetPersonGuidFromOpersAsync(It.IsAny<string>()))
                    .Returns(Task.FromResult("84efc02c-4b2e-4ad2-91fd-4688b94915e9"))
                    .Returns(Task.FromResult("09035e0f-1a59-46e2-9abc-8e634ad4fdda"))
                    .Returns(Task.FromResult("13e50284-676f-4df4-90be-1432c34dfe40"));

                var actuals = await campusOrganizationService.GetCampusOrganizationsAsync(It.IsAny<bool>());
                Assert.AreEqual(actuals.Count(), 4);

                foreach (var actual in actuals)
                {
                    var expected = campusOrganizationsDtos.FirstOrDefault(i => i.Id.Equals(actual.Id));

                    Assert.IsNotNull(expected);
                    Assert.AreEqual(expected.CampusOrganizationName, actual.CampusOrganizationName);
                    Assert.AreEqual(expected.CampusOrganizationType.Id, actual.CampusOrganizationType.Id);
                    Assert.AreEqual(expected.Code, actual.Code);
                    if (expected.ParentOrganization != null)
                    {
                        Assert.AreEqual(expected.ParentOrganization.Id, actual.ParentOrganization.Id);
                    }
                }
            }

            [TestMethod]
            public async Task CampusOrganizationService__GetCampusOrganizationByIdAsync()
            {
                var id = "d190d4b5-03b5-41aa-99b8-b8286717c956";
                var expected = campusOrganizationsDtos.FirstOrDefault(i => i.Id.Equals(id));
                var campOrg = campusOrganizationEntities.FirstOrDefault(org => org.Guid.Equals(id, StringComparison.OrdinalIgnoreCase));
                filteredCampusOrgTypes = campusOrganizationTypes.Where(i => campOrg.CampusOrganizationTypeId.Equals(i.Code, StringComparison.OrdinalIgnoreCase)).ToList();

                campusOrganizationRepositoryMock.Setup(i => i.GetCampusOrganizationsAsync(true)).ReturnsAsync(campusOrganizationEntities);
                studentReferenceDataRepositoryMock.Setup(i => i.GetCampusOrganizationTypesAsync(It.IsAny<bool>())).ReturnsAsync(filteredCampusOrgTypes);
                personBaseRepositoryMock.Setup(i => i.GetPersonGuidFromOpersAsync(It.IsAny<string>())).ReturnsAsync("84efc02c-4b2e-4ad2-91fd-4688b94915e9");

                var actual = await campusOrganizationService.GetCampusOrganizationByGuidAsync(id);

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.CampusOrganizationName, actual.CampusOrganizationName);
                Assert.AreEqual(expected.CampusOrganizationType.Id, actual.CampusOrganizationType.Id);
                if (expected.ParentOrganization != null)
                {
                    Assert.AreEqual(expected.ParentOrganization.Id, actual.ParentOrganization.Id);
                }
            }

            private void BuildData()
            {
                campusOrganizationEntities = new List<CampusOrgEntity>() 
                { 
                    new CampusOrgEntity("1", "d190d4b5-03b5-41aa-99b8-b8286717c956", "Assoc for Computing MacHinery", "1", "ACAD"),
                    new CampusOrgEntity("2", "2d37defe-6c88-4c06-bd37-17242956424e", "Alpha Kappa Lamdba", "2", "GREK"),
                    new CampusOrgEntity("3", "cecdce5a-54a7-45fb-a975-5392a579e5bf", "Art Club", "", "FNAR"),
                    new CampusOrgEntity("4", "038179c8-8d34-4c94-99e8-e2a53bca0305", "Bacon Lovers Of Ellucian Univ", "4", "SOCI"),
                };

                campusOrganizationsDtos = new List<CampusOrgDto>() 
                {
                    new CampusOrgDto()
                    {
                        Id = "d190d4b5-03b5-41aa-99b8-b8286717c956",
                        Code = "1",
                        CampusOrganizationName = "Assoc for Computing MacHinery",
                        CampusOrganizationType = new Dtos.GuidObject2("ea661349-133a-4025-86fa-68d73fbe14a5"),
                        ParentOrganization = new Dtos.GuidObject2("84efc02c-4b2e-4ad2-91fd-4688b94915e9")
                    },
                    new CampusOrgDto()
                    {
                        Id = "2d37defe-6c88-4c06-bd37-17242956424e",
                        Code = "2",
                        CampusOrganizationName = "Alpha Kappa Lamdba",
                        CampusOrganizationType = new Dtos.GuidObject2("606fd9cb-ca3c-4241-bb51-d760ad907788"),
                        ParentOrganization = new Dtos.GuidObject2("09035e0f-1a59-46e2-9abc-8e634ad4fdda")
                    },
                    new CampusOrgDto()
                    {
                        Id = "cecdce5a-54a7-45fb-a975-5392a579e5bf",
                        Code = "3",
                        CampusOrganizationName = "Art Club",
                        CampusOrganizationType = new Dtos.GuidObject2("143e48c3-80b3-41de-bf85-ef189a2615c8")
                    },
                    new CampusOrgDto()
                    {
                        Id = "038179c8-8d34-4c94-99e8-e2a53bca0305",
                        Code = "4",
                        CampusOrganizationName = "Bacon Lovers Of Ellucian Univ",
                        CampusOrganizationType = new Dtos.GuidObject2("462f0d57-563b-4807-b2b4-cac4df1f874c"),
                        ParentOrganization = new Dtos.GuidObject2("13e50284-676f-4df4-90be-1432c34dfe40")
                    },
                };

                campusOrganizationTypes = new List<CampusOrgType>() 
                {
                    new CampusOrgType("ea661349-133a-4025-86fa-68d73fbe14a5", "ACAD", "Academic"),
                    new CampusOrgType("b43ee9f1-6415-450b-b953-49487f49e51a", "ATHL", "Athletics"),
                    new CampusOrgType("143e48c3-80b3-41de-bf85-ef189a2615c8", "FNAR", "Fine Arts"),
                    new CampusOrgType("222599a9-dafe-4f5d-b451-922c9739c568", "GOVT", "Governance"),
                    new CampusOrgType("606fd9cb-ca3c-4241-bb51-d760ad907788", "GREK", "FraternitySorority"),
                    new CampusOrgType("462f0d57-563b-4807-b2b4-cac4df1f874c", "SOCI", "Social"),
                };

                parentOrganizationIds.Add("1", "84efc02c-4b2e-4ad2-91fd-4688b94915e9");
                parentOrganizationIds.Add("2", "09035e0f-1a59-46e2-9abc-8e634ad4fdda");
                parentOrganizationIds.Add("3", "");
                parentOrganizationIds.Add("4", "13e50284-676f-4df4-90be-1432c34dfe40");

                campusOrganizationTypeIds = campusOrganizationEntities.Where(i => !string.IsNullOrEmpty(i.CampusOrganizationTypeId)).Select(i => i.CampusOrganizationTypeId).ToList();

                filteredCampusOrgTypes = campusOrganizationTypes.Where(i => campusOrganizationTypeIds.Contains(i.Code)).ToList();
            }
        }

        [TestClass]
        public class CampusOrganizationTypes
        {
            private Mock<IPersonBaseRepository> personBaseRepository;
            private Mock<ICampusOrganizationRepository> campusOrganizationRepositoryMock;
            private Mock<IStudentReferenceDataRepository> referenceRepositoryMock;
            private Mock<IPersonRepository> personRepositoryMock;
            private IStudentReferenceDataRepository referenceRepository;
            private ILogger logger;
            private CampusOrganizationService campusOrganizationService;
            private ICollection<Domain.Student.Entities.CampusOrganizationType> campusOrganizationTypeCollection = new List<Domain.Student.Entities.CampusOrganizationType>();
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;

            [TestInitialize]
            public void Initialize()
            {
                personBaseRepository = new Mock<IPersonBaseRepository>();
                campusOrganizationRepositoryMock = new Mock<ICampusOrganizationRepository>();
                referenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                referenceRepository = referenceRepositoryMock.Object;
                personRepositoryMock = new Mock<IPersonRepository>();
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;
                logger = new Mock<ILogger>().Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;

                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                campusOrganizationTypeCollection.Add(new Domain.Student.Entities.CampusOrganizationType("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "LG", "null"));
                campusOrganizationTypeCollection.Add(new Domain.Student.Entities.CampusOrganizationType("73244057-D1EC-4094-A0B7-DE602533E3A6", "30", "null"));
                campusOrganizationTypeCollection.Add(new Domain.Student.Entities.CampusOrganizationType("1df164eb-8178-4321-a9f7-24f12d3991d8", "04", "null"));
                referenceRepositoryMock.Setup(repo => repo.GetCampusOrganizationTypesAsync(true)).ReturnsAsync(campusOrganizationTypeCollection);
                referenceRepositoryMock.Setup(repo => repo.GetCampusOrganizationTypesAsync(false)).ReturnsAsync(campusOrganizationTypeCollection);

                campusOrganizationService = new CampusOrganizationService(adapterRegistry, personBaseRepository.Object,
                    campusOrganizationRepositoryMock.Object, referenceRepository, personRepositoryMock.Object, baseConfigurationRepository, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                campusOrganizationTypeCollection = null;
                referenceRepository = null;
                campusOrganizationService = null;
            }

            [TestMethod]
            public async Task CampusOrganizationService__CampusOrganizationTypes()
            {
                var results = await campusOrganizationService.GetCampusOrganizationTypesAsync();
                Assert.IsTrue(results is IEnumerable<Dtos.CampusOrganizationType>);
                Assert.IsNotNull(results);
            }

            public async Task CampusOrganizationService_CampusOrganizationTypes_Count()
            {
                var results = await campusOrganizationService.GetCampusOrganizationTypesAsync();
                Assert.AreEqual(3, results.Count());
            }

            [TestMethod]
            public async Task CampusOrganizationService_CampusOrganizationTypes_Properties()
            {
                var results = await campusOrganizationService.GetCampusOrganizationTypesAsync();
                var campusOrganizationType = results.Where(x => x.Code == "LG").FirstOrDefault();
                Assert.IsNotNull(campusOrganizationType.Id);
                Assert.IsNotNull(campusOrganizationType.Code);
            }

            [TestMethod]
            public async Task CampusOrganizationService_CampusOrganizationTypes_Expected()
            {
                var expectedResults = campusOrganizationTypeCollection.Where(c => c.Code == "LG").FirstOrDefault();
                var results = await campusOrganizationService.GetCampusOrganizationTypesAsync();
                var campusOrganizationType = results.Where(s => s.Code == "LG").FirstOrDefault();
                Assert.AreEqual(expectedResults.Guid, campusOrganizationType.Id);
                Assert.AreEqual(expectedResults.Code, campusOrganizationType.Code);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CampusOrganizationService_GetCampusOrganizationTypeByGuid_Empty()
            {
                await campusOrganizationService.GetCampusOrganizationTypeByGuidAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CampusOrganizationService_GetCampusOrganizationTypeByGuid_Null()
            {
                await campusOrganizationService.GetCampusOrganizationTypeByGuidAsync(null);
            }

            [TestMethod]
            public async Task CampusOrganizationService_GetCampusOrganizationTypeByGuid_Expected()
            {
                var expectedResults = campusOrganizationTypeCollection.Where(c => c.Guid == "1df164eb-8178-4321-a9f7-24f12d3991d8").FirstOrDefault();
                var campusOrganizationType = await campusOrganizationService.GetCampusOrganizationTypeByGuidAsync("1df164eb-8178-4321-a9f7-24f12d3991d8");
                Assert.AreEqual(expectedResults.Guid, campusOrganizationType.Id);
                Assert.AreEqual(expectedResults.Code, campusOrganizationType.Code);
            }

            [TestMethod]
            public async Task CampusOrganizationService_GetCampusOrganizationTypeByGuid_Properties()
            {
                var expectedResults = campusOrganizationTypeCollection.Where(c => c.Guid == "1df164eb-8178-4321-a9f7-24f12d3991d8").FirstOrDefault();
                var campusOrganizationType = await campusOrganizationService.GetCampusOrganizationTypeByGuidAsync("1df164eb-8178-4321-a9f7-24f12d3991d8");
                Assert.IsNotNull(campusOrganizationType.Id);
                Assert.IsNotNull(campusOrganizationType.Code);
            }
        }

        [TestClass]
        public class CampusInvolvements : StudentUserFactory
        {
            Mock<ICampusOrganizationRepository> campusOrganizationRepositoryMock;
            Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
            Mock<IPersonRepository> personRepositoryMock;
            Mock<IPersonBaseRepository> personBaseRepositoryMock;
            private ILogger logger;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;

            CampusOrganizationService campusOrganizationService;

            protected Ellucian.Colleague.Domain.Entities.Role viewInvolvementRole = new Ellucian.Colleague.Domain.Entities.Role(1, "VIEW.CAMPUS.ORG.MEMBERS");

            IEnumerable<CampusInvlEntity> campusInvolvementEntities;
            IEnumerable<Dtos.CampusInvolvement> campusInvolvementDtos;
            IEnumerable<CampusInvRolesEntity> campusInvRolesEntities;
            List<CampusOrgEntity> campusOrganizationEntities = new List<CampusOrgEntity>();

            Tuple<IEnumerable<CampusInvlEntity>, int> campusInvlEntitiesTuple;
            Tuple<IEnumerable<Dtos.CampusInvolvement>, int> campusInvlDtoTuple;

            int offset = 0;
            int limit = 200;


            [TestInitialize]
            public void Initialize()
            {
                campusOrganizationRepositoryMock = new Mock<ICampusOrganizationRepository>();
                studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                personRepositoryMock = new Mock<IPersonRepository>();
                personBaseRepositoryMock = new Mock<IPersonBaseRepository>();
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;
                logger = new Mock<ILogger>().Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;

                currentUserFactory = new CampusInvolvementsUser();

                BuildData();
                BuildMocks();
                personBaseRepositoryMock.SetupSequence(i => i.GetPersonGuidFromOpersAsync(It.IsAny<string>()))
                    .Returns(Task.FromResult("e0c0c94c-53a7-46b7-96c4-76b12512c323"))
                    .Returns(Task.FromResult("0bbb15f2-bb03-4056-bb9b-57a0ddf057ff"))
                    .Returns(Task.FromResult("0ac28907-5a9b-4102-a0d7-5d3d9c585512"))
                    .Returns(Task.FromResult("bb6c261c-3818-4dc3-b693-eb3e64d70d8b"));

                campusOrganizationService = new CampusOrganizationService(adapterRegistry, personBaseRepositoryMock.Object,
                    campusOrganizationRepositoryMock.Object, studentReferenceDataRepositoryMock.Object, personRepositoryMock.Object, baseConfigurationRepository, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                campusOrganizationRepositoryMock = null;
                studentReferenceDataRepositoryMock = null;
                personBaseRepositoryMock = null;
                campusInvolvementEntities = null;
                campusInvolvementDtos = null;
                campusInvRolesEntities = null;
            }

            private void BuildData()
            {                
                //Dtos
                campusInvolvementDtos = new List<Dtos.CampusInvolvement>() 
                {
                    new Dtos.CampusInvolvement()
                    {
                        CampusOrganizationId = new Dtos.GuidObject2("d190d4b5-03b5-41aa-99b8-b8286717c956"), 
                        Id = "bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a", 
                        InvolvementStartOn = new DateTime(2016, 09, 03), 
                        InvolvementEndOn = new DateTime(2016, 11, 30), 
                        InvolvementRole = new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f"), 
                        PersonId = new Dtos.GuidObject2("e0c0c94c-53a7-46b7-96c4-76b12512c323")
                    },
                    new Dtos.CampusInvolvement()
                    {
                        CampusOrganizationId = new Dtos.GuidObject2("d190d4b5-03b5-41aa-99b8-b8286717c956"), 
                        Id = "3f67b180-ce1d-4552-8d81-feb96b9fea5b", 
                        InvolvementStartOn = new DateTime(2016, 09, 01), 
                        InvolvementEndOn = new DateTime(2016, 11, 30),
                        InvolvementRole = new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f"), 
                        PersonId = new Dtos.GuidObject2("0bbb15f2-bb03-4056-bb9b-57a0ddf057ff")
                    },
                    new Dtos.CampusInvolvement()
                    {
                        CampusOrganizationId = new Dtos.GuidObject2("cecdce5a-54a7-45fb-a975-5392a579e5bf"), 
                        Id = "bf67e156-8f5d-402b-8101-81b0a2796873", 
                        InvolvementStartOn = new DateTime(2016, 04, 01), 
                        InvolvementEndOn = new DateTime(2016, 09, 30),  
                        InvolvementRole = null, 
                        PersonId = new Dtos.GuidObject2("0ac28907-5a9b-4102-a0d7-5d3d9c585512")
                    },
                    new Dtos.CampusInvolvement()
                    {
                        CampusOrganizationId = new Dtos.GuidObject2("cecdce5a-54a7-45fb-a975-5392a579e5bf"), 
                        Id = "0111d6ef-5a86-465f-ac58-4265a997c136", 
                        InvolvementStartOn = new DateTime(2016, 02, 01), 
                        InvolvementEndOn = new DateTime(2016, 06, 30), 
                        InvolvementRole = new Dtos.GuidObject2("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52"), 
                        PersonId = new Dtos.GuidObject2("bb6c261c-3818-4dc3-b693-eb3e64d70d8b")
                    },
                };

                //Entities
                campusInvolvementEntities = new List<CampusInvlEntity>() 
                {
                    new CampusInvlEntity("bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a", "1", "100")
                    {
                        AcademicPeriodId = null,
                        EndOn = new DateTime(2016, 11, 30),
                        RoleId = "AD",
                        StartOn = new DateTime(2016, 09, 03)
                    },
                    new CampusInvlEntity("3f67b180-ce1d-4552-8d81-feb96b9fea5b", "1", "200")
                    {
                        AcademicPeriodId = null,
                        EndOn = new DateTime(2016, 11, 30),
                        RoleId = "AD",
                        StartOn = new DateTime(2016, 09, 01)
                    },
                    new CampusInvlEntity("bf67e156-8f5d-402b-8101-81b0a2796873", "3", "300")
                    {
                        AcademicPeriodId = null,
                        EndOn = new DateTime(2016, 09, 30),
                        RoleId = "",
                        StartOn = new DateTime(2016, 04, 01)
                    },
                    new CampusInvlEntity("0111d6ef-5a86-465f-ac58-4265a997c136", "3", "400")
                    {                        
                        AcademicPeriodId = null,
                        EndOn = new DateTime(2016, 06, 30),
                        RoleId = "ME",
                        StartOn = new DateTime(2016, 02, 01)

                    },
                };

                //Roles
                campusInvRolesEntities = new List<CampusInvRolesEntity>() 
                {
                    new CampusInvRolesEntity("b90812ee-b573-4acb-88b0-6999a050be4f", "AD", "Advisor"),
                    new CampusInvRolesEntity("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "ME", "Member"),
                    new CampusInvRolesEntity("abe5524b-6704-4f09-b858-763ee2ab5fe4", "LAD", "Limited Advisor"),
                    new CampusInvRolesEntity("2158ad73-3416-467b-99d5-1b7b92599389", "OF", "Officer")
                };
                
                //Campus organisations
                campusOrganizationEntities = new List<CampusOrgEntity>() 
                { 
                    new CampusOrgEntity("1", "d190d4b5-03b5-41aa-99b8-b8286717c956", "Assoc for Computing MacHinery", "1", "ACAD"),
                    new CampusOrgEntity("2", "2d37defe-6c88-4c06-bd37-17242956424e", "Alpha Kappa Lamdba", "2", "GREK"),
                    new CampusOrgEntity("3", "cecdce5a-54a7-45fb-a975-5392a579e5bf", "Art Club", "", "FNAR"),
                    new CampusOrgEntity("4", "038179c8-8d34-4c94-99e8-e2a53bca0305", "Bacon Lovers Of Ellucian Univ", "4", "SOCI"),
                };



                //Tuple
                campusInvlEntitiesTuple = new Tuple<IEnumerable<CampusInvlEntity>, int>(campusInvolvementEntities, campusInvolvementEntities.Count());
                campusInvlDtoTuple = new Tuple<IEnumerable<Dtos.CampusInvolvement>, int>(campusInvolvementDtos, campusInvolvementDtos.Count());
            }

            private void BuildMocks()
            {
                viewInvolvementRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewCampusInvolvements));
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewInvolvementRole });

                campusOrganizationRepositoryMock.Setup(i => i.GetCampusOrganizationsAsync(It.IsAny<bool>())).ReturnsAsync(campusOrganizationEntities);
                studentReferenceDataRepositoryMock.Setup(i => i.GetCampusInvolvementRolesAsync(It.IsAny<bool>())).ReturnsAsync(campusInvRolesEntities);
            }

            //[TestMethod]
            //public async Task CampusOrganizationService__GetCampusInvolvementsAsync()
            //{
            //    campusOrganizationRepositoryMock.Setup(i => i.GetCampusInvolvementsAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(campusInvlEntitiesTuple);
            //    var campusInvolvements = await campusOrganizationService.GetCampusInvolvementsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>());

            //    Assert.IsNotNull(campusInvolvements);

            //    foreach (var actual in campusInvolvements.Item1)
            //    {
            //        var expected = campusInvolvementDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
            //        Assert.IsNotNull(expected);

            //        Assert.AreEqual(expected.Id, actual.Id);
            //        Assert.AreEqual(expected.AcademicPeriod, actual.AcademicPeriod);
            //        Assert.AreEqual(expected.CampusOrganizationId.Id, actual.CampusOrganizationId.Id);
            //        Assert.AreEqual(expected.InvolvementEndOn, actual.InvolvementEndOn);
            //        if (actual.InvolvementRole == null)
            //        {
            //            Assert.IsNull(actual.InvolvementRole);
            //            Assert.IsNull(expected.InvolvementRole);
            //        }
            //        else
            //        {
            //            Assert.AreEqual(expected.InvolvementRole.Id, actual.InvolvementRole.Id);
            //        }
                    
            //        Assert.AreEqual(expected.InvolvementStartOn, actual.InvolvementStartOn);
            //        Assert.AreEqual(expected.PersonId.Id, actual.PersonId.Id);
            //    }
            //}

            //[TestMethod]
            //public async Task CampusOrganizationService__GetCampusInvolvementByGuidAsync()
            //{
            //    var id = "bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a";
            //    var campusInvlEntity = campusInvolvementEntities.FirstOrDefault(i => i.CampusInvolvementId.Equals(id));
            //    campusOrganizationRepositoryMock.Setup(i => i.GetGetCampusInvolvementByIdAsync(It.IsAny<string>())).ReturnsAsync(campusInvlEntity);

            //    var actual = await campusOrganizationService.GetCampusInvolvementByGuidAsync(id);

            //    Assert.IsNotNull(actual);

            //    var expected = campusInvolvementDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
            //    Assert.IsNotNull(expected);

            //    Assert.AreEqual(expected.Id, actual.Id);
            //    Assert.AreEqual(expected.AcademicPeriod, actual.AcademicPeriod);
            //    Assert.AreEqual(expected.CampusOrganizationId.Id, actual.CampusOrganizationId.Id);
            //    Assert.AreEqual(expected.InvolvementEndOn, actual.InvolvementEndOn);
            //    Assert.AreEqual(expected.InvolvementRole.Id, actual.InvolvementRole.Id);
            //    Assert.AreEqual(expected.InvolvementStartOn, actual.InvolvementStartOn);
            //    Assert.AreEqual(expected.PersonId.Id, actual.PersonId.Id);
            //}


        }

        [TestClass]
        public class CampusInvolvementRoles
        {
            private Mock<IPersonBaseRepository> personBaseRepository;
            private Mock<ICampusOrganizationRepository> campusOrganizationRepository;
            private Mock<IStudentReferenceDataRepository> referenceRepositoryMock;
            private IStudentReferenceDataRepository referenceRepository;
            private Mock<IPersonRepository> personRepositoryMock;
            private ILogger logger;
            private CampusOrganizationService campusOrganizationService;
            private ICollection<Domain.Student.Entities.CampusInvRole> campusInvolvementRoleCollection = new List<Domain.Student.Entities.CampusInvRole>();
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;

            [TestInitialize]
            public void Initialize()
            {
                personBaseRepository = new Mock<IPersonBaseRepository>();
                campusOrganizationRepository = new Mock<ICampusOrganizationRepository>();
                referenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                referenceRepository = referenceRepositoryMock.Object;
                personRepositoryMock = new Mock<IPersonRepository>();
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;
                logger = new Mock<ILogger>().Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;

                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                campusInvolvementRoleCollection.Add(new Domain.Student.Entities.CampusInvRole("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "LG", "null"));
                campusInvolvementRoleCollection.Add(new Domain.Student.Entities.CampusInvRole("73244057-D1EC-4094-A0B7-DE602533E3A6", "30", "null"));
                campusInvolvementRoleCollection.Add(new Domain.Student.Entities.CampusInvRole("1df164eb-8178-4321-a9f7-24f12d3991d8", "04", "null"));
                referenceRepositoryMock.Setup(repo => repo.GetCampusInvolvementRolesAsync(true)).ReturnsAsync(campusInvolvementRoleCollection);
                referenceRepositoryMock.Setup(repo => repo.GetCampusInvolvementRolesAsync(false)).ReturnsAsync(campusInvolvementRoleCollection);

                campusOrganizationService = new CampusOrganizationService(adapterRegistry, personBaseRepository.Object,
                    campusOrganizationRepository.Object, referenceRepository, personRepositoryMock.Object, baseConfigurationRepository, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                campusInvolvementRoleCollection = null;
                referenceRepository = null;
                campusOrganizationService = null;
            }

            [TestMethod]
            public async Task CampusOrganizationService__CampusInvolvementRoles()
            {
                var results = await campusOrganizationService.GetCampusInvolvementRolesAsync();
                Assert.IsTrue(results is IEnumerable<Dtos.CampusInvolvementRole>);
                Assert.IsNotNull(results);
            }

            public async Task CampusOrganizationService_CampusInvolvementRoles_Count()
            {
                var results = await campusOrganizationService.GetCampusInvolvementRolesAsync();
                Assert.AreEqual(3, results.Count());
            }

            [TestMethod]
            public async Task CampusOrganizationService_CampusInvolvementRoles_Properties()
            {
                var results = await campusOrganizationService.GetCampusInvolvementRolesAsync();
                var campusInvolvementRole = results.Where(x => x.Code == "LG").FirstOrDefault();
                Assert.IsNotNull(campusInvolvementRole.Id);
                Assert.IsNotNull(campusInvolvementRole.Code);
            }

            [TestMethod]
            public async Task CampusOrganizationService_CampusInvolvementRoles_Expected()
            {
                var expectedResults = campusInvolvementRoleCollection.Where(c => c.Code == "LG").FirstOrDefault();
                var results = await campusOrganizationService.GetCampusInvolvementRolesAsync();
                var campusInvolvementRole = results.Where(s => s.Code == "LG").FirstOrDefault();
                Assert.AreEqual(expectedResults.Guid, campusInvolvementRole.Id);
                Assert.AreEqual(expectedResults.Code, campusInvolvementRole.Code);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CampusOrganizationService_GetCampusInvolvementRoleByGuid_Empty()
            {
                await campusOrganizationService.GetCampusInvolvementRoleByGuidAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CampusOrganizationService_GetCampusInvolvementRoleByGuid_Null()
            {
                await campusOrganizationService.GetCampusInvolvementRoleByGuidAsync(null);
            }

            [TestMethod]
            public async Task CampusOrganizationService_GetCampusInvolvementRoleByGuid_Expected()
            {
                var expectedResults = campusInvolvementRoleCollection.Where(c => c.Guid == "1df164eb-8178-4321-a9f7-24f12d3991d8").FirstOrDefault();
                var campusInvolvementRole = await campusOrganizationService.GetCampusInvolvementRoleByGuidAsync("1df164eb-8178-4321-a9f7-24f12d3991d8");
                Assert.AreEqual(expectedResults.Guid, campusInvolvementRole.Id);
                Assert.AreEqual(expectedResults.Code, campusInvolvementRole.Code);
            }

            [TestMethod]
            public async Task CampusOrganizationService_GetCampusInvolvementRoleByGuid_Properties()
            {
                var expectedResults = campusInvolvementRoleCollection.Where(c => c.Guid == "1df164eb-8178-4321-a9f7-24f12d3991d8").FirstOrDefault();
                var campusInvolvementRole = await campusOrganizationService.GetCampusInvolvementRoleByGuidAsync("1df164eb-8178-4321-a9f7-24f12d3991d8");
                Assert.IsNotNull(campusInvolvementRole.Id);
                Assert.IsNotNull(campusInvolvementRole.Code);
            }
        }

    }
}
