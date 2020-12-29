// Copyright 2019-2020 Ellucian Company L.P. and its affiliates.
using Moq;
using System;
using slf4net;
using System.Linq;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using System.Threading.Tasks;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class RetentionAlertServiceTests
    {
        private Role advisorAnyAdviseeRole = new Role(2, "Advisor");

        // Sets up a Current user that is a faculty
        public abstract class CurrentUserSetup
        {
            protected Role facultyRole = new Role(105, "Faculty");

            public class AdvisorUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "George",
                            PersonId = "0000011",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "Advisor",
                            Roles = new List<string>() { "Advisor" },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }

            public class FacultyUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "George Smith",
                            PersonId = "0000011",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "GSmith",
                            Roles = new List<string>() { "Faculty" },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }
        }

        [TestClass]
        public class GetCaseTypes : CurrentUserSetup
        {
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            private IReferenceDataRepository referenceDataRepository;
            private IEnumerable<CaseType> caseTypeResponse;
            private IRetentionAlertService retentionAlertService;
            private Mock<IStudentRepository> studentRepositoryMock;
            private IStudentRepository studentRepository;
            private Mock<IConfigurationRepository> configurationRepositoryMock;
            private IConfigurationRepository configurationRepository;
            private ICurrentUserFactory currentUserFactory;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;

            private Mock<IRetentionAlertRepository> retentionAlertRepositoryMock;
            private IRetentionAlertRepository retentionAlertRepository;

            [TestInitialize]
            public async void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                logger = new Mock<ILogger>().Object;

                studentRepositoryMock = new Mock<IStudentRepository>();
                studentRepository = studentRepositoryMock.Object;
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                configurationRepository = configurationRepositoryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;

                retentionAlertRepositoryMock = new Mock<IRetentionAlertRepository>();
                retentionAlertRepository = retentionAlertRepositoryMock.Object;

                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;

                caseTypeResponse = BuildCaseTypesResponse();
                referenceDataRepositoryMock.Setup(repo => repo.GetCaseTypesAsync()).Returns(Task.FromResult(caseTypeResponse));

                // Mock Adapters
                var caseTypeDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.CaseType, Ellucian.Colleague.Dtos.Student.CaseType>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.CaseType, Ellucian.Colleague.Dtos.Student.CaseType>()).Returns(caseTypeDtoAdapter);

                // Set up current user
                currentUserFactory = new CurrentUserSetup.FacultyUserFactory();

                retentionAlertService = new RetentionAlertService(adapterRegistry, referenceDataRepository, studentRepository, configurationRepository, currentUserFactory, roleRepo, logger, retentionAlertRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistry = null;
                roleRepo = null;
                retentionAlertService = null;
                referenceDataRepository = null;
                studentRepository = null;
                configurationRepository = null;
                retentionAlertRepository = null;
            }

            [TestMethod]
            public async Task GetCaseTypes_ReturnsCaseTypes()
            {
                referenceDataRepositoryMock.Setup(repo => repo.GetCaseTypesAsync()).Returns(Task.FromResult(caseTypeResponse));
                var caseTypeDto = await retentionAlertService.GetCaseTypesAsync();

                Assert.AreEqual(2, caseTypeDto.Count());
                Assert.AreEqual(caseTypeResponse.ElementAt(0).Code, caseTypeDto.ElementAt(0).Code);
                Assert.AreEqual(caseTypeResponse.ElementAt(0).Description, caseTypeDto.ElementAt(0).Description);
                Assert.AreEqual(caseTypeResponse.ElementAt(0).CaseTypeId, caseTypeDto.ElementAt(0).CaseTypeId);
                Assert.AreEqual(caseTypeResponse.ElementAt(0).Category, caseTypeDto.ElementAt(0).Category);
                Assert.AreEqual(caseTypeResponse.ElementAt(0).Priority, caseTypeDto.ElementAt(0).Priority);
                Assert.AreEqual(caseTypeResponse.ElementAt(0).IsActive, caseTypeDto.ElementAt(0).IsActive);
                Assert.AreEqual(caseTypeResponse.ElementAt(0).AllowCaseContribution, caseTypeDto.ElementAt(0).AllowCaseContribution);
                Assert.AreEqual(caseTypeResponse.ElementAt(0).AvailableCommunicationCodes.Count, caseTypeDto.ElementAt(0).AvailableCommunicationCodes.Count());
            }

            private IEnumerable<CaseType> BuildCaseTypesResponse()
            {
                List<CaseType> cases = new List<CaseType>();
                cases.Add(new CaseType("EARLY", "Early Alert", "1", "4", "H", true, true, new List<string>() {"3","4"}));
                cases.Add(new CaseType("LOW_GPA", "Low Cum GPA", "2", "2", "M", true, true, new List<string>() { "5", "4" }));
                return cases;
            }
        }

        [TestClass]
        public class GetCasePriorities : CurrentUserSetup
        {
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            private IReferenceDataRepository referenceDataRepository;
            private IEnumerable<CasePriority> casePriorityResponse;
            private IRetentionAlertService retentionAlertService;
            private Mock<IStudentRepository> studentRepositoryMock;
            private IStudentRepository studentRepository;
            private Mock<IConfigurationRepository> configurationRepositoryMock;
            private IConfigurationRepository configurationRepository;
            private ICurrentUserFactory currentUserFactory;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;

            private Mock<IRetentionAlertRepository> retentionAlertRepositoryMock;
            private IRetentionAlertRepository retentionAlertRepository;

            [TestInitialize]
            public async void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                logger = new Mock<ILogger>().Object;

                studentRepositoryMock = new Mock<IStudentRepository>();
                studentRepository = studentRepositoryMock.Object;
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                configurationRepository = configurationRepositoryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;

                retentionAlertRepositoryMock = new Mock<IRetentionAlertRepository>();
                retentionAlertRepository = retentionAlertRepositoryMock.Object;

                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;

                casePriorityResponse = BuildCasePrioritiesResponse();
                referenceDataRepositoryMock.Setup(repo => repo.GetCasePrioritiesAsync()).Returns(Task.FromResult(casePriorityResponse));

                // Mock Adapters
                var casePriorityDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.CasePriority, Ellucian.Colleague.Dtos.Student.CasePriority>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.CasePriority, Ellucian.Colleague.Dtos.Student.CasePriority>()).Returns(casePriorityDtoAdapter);

                // Set up current user
                currentUserFactory = new CurrentUserSetup.FacultyUserFactory();

                retentionAlertService = new RetentionAlertService(adapterRegistry, referenceDataRepository, studentRepository, configurationRepository, currentUserFactory, roleRepo, logger, retentionAlertRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistry = null;
                roleRepo = null;
                retentionAlertService = null;
                referenceDataRepository = null;
                studentRepository = null;
                configurationRepository = null;
                retentionAlertRepository = null;
            }

            [TestMethod]
            public async Task GetCasePriorities_ReturnsCaseTypes()
            {
                referenceDataRepositoryMock.Setup(repo => repo.GetCasePrioritiesAsync()).Returns(Task.FromResult(casePriorityResponse));
                var casePriorityDto = await retentionAlertService.GetCasePrioritiesAsync();
                Assert.AreEqual(2, casePriorityDto.Count());
                Assert.AreEqual(casePriorityResponse.ElementAt(0).Code, casePriorityDto.ElementAt(0).Code);
                Assert.AreEqual(casePriorityResponse.ElementAt(0).Description, casePriorityDto.ElementAt(0).Description);               
            }

            private IEnumerable<CasePriority> BuildCasePrioritiesResponse()
            {
                List<CasePriority> casePriorities = new List<CasePriority>();
                casePriorities.Add(new CasePriority("H", "High"));
                casePriorities.Add(new CasePriority("L", "Low"));
                return casePriorities;
            }
        }

        public class GetCaseCategories : CurrentUserSetup
        {
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            private IReferenceDataRepository referenceDataRepository;
            private IEnumerable<CaseCategory> caseCategoryResponse;
            private IRetentionAlertService retentionAlertService;
            private Mock<IStudentRepository> studentRepositoryMock;
            private IStudentRepository studentRepository;
            private Mock<IConfigurationRepository> configurationRepositoryMock;
            private IConfigurationRepository configurationRepository;
            private ICurrentUserFactory currentUserFactory;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;

            private Mock<IRetentionAlertRepository> retentionAlertRepositoryMock;
            private IRetentionAlertRepository retentionAlertRepository;

            [TestInitialize]
            public async void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                logger = new Mock<ILogger>().Object;

                studentRepositoryMock = new Mock<IStudentRepository>();
                studentRepository = studentRepositoryMock.Object;
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                configurationRepository = configurationRepositoryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;

                retentionAlertRepositoryMock = new Mock<IRetentionAlertRepository>();
                retentionAlertRepository = retentionAlertRepositoryMock.Object;

                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;

                caseCategoryResponse = BuildCaseCategoriesResponse();
                referenceDataRepositoryMock.Setup(repo => repo.GetCaseCategoriesAsync()).Returns(Task.FromResult(caseCategoryResponse));

                // Mock Adapters
                var caseCategoryDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.CaseCategory, Ellucian.Colleague.Dtos.Student.CaseCategory>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.CaseCategory, Ellucian.Colleague.Dtos.Student.CaseCategory>()).Returns(caseCategoryDtoAdapter);

                // Set up current user
                currentUserFactory = new CurrentUserSetup.FacultyUserFactory();

                retentionAlertService = new RetentionAlertService(adapterRegistry, referenceDataRepository, studentRepository, configurationRepository, currentUserFactory, roleRepo, logger, retentionAlertRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistry = null;
                roleRepo = null;
                retentionAlertService = null;
                referenceDataRepository = null;
                studentRepository = null;
                configurationRepository = null;
                retentionAlertRepository = null;
            }

            [TestMethod]
            public async Task GetCaseCategories_ReturnsCaseTypes()
            {
                referenceDataRepositoryMock.Setup(repo => repo.GetCaseCategoriesAsync()).Returns(Task.FromResult(caseCategoryResponse));
                var caseCategoryDto = await retentionAlertService.GetCaseCategoriesAsync();

                Assert.AreEqual(2, caseCategoryDto.Count());
                Assert.AreEqual(caseCategoryResponse.ElementAt(0).Code, caseCategoryDto.ElementAt(0).Code);
                Assert.AreEqual(caseCategoryResponse.ElementAt(0).Description, caseCategoryDto.ElementAt(0).Description);
                Assert.AreEqual(caseCategoryResponse.ElementAt(0).CategoryId, caseCategoryDto.ElementAt(0).CategoryId);
                Assert.AreEqual(caseCategoryResponse.ElementAt(0).CaseTypes.Count, caseCategoryDto.ElementAt(0).CaseTypes.Count());
                Assert.AreEqual(caseCategoryResponse.ElementAt(0).CaseClosureReasons.Count, caseCategoryDto.ElementAt(0).CaseClosureReasons.Count());               
            }

            private IEnumerable<CaseCategory> BuildCaseCategoriesResponse()
            {
                List<CaseCategory> caseCategories = new List<CaseCategory>();
                caseCategories.Add(new CaseCategory("EARLY", "Early Alert", "1", new List<string>() { "1", "2" }, new List<string>() { "3", "4" }, null));
                caseCategories.Add(new CaseCategory("LOW_GPA", "Low Cum GPA", "2", new List<string>() { "3", "4" }, new List<string>() { "5", "6" }, null));
                return caseCategories;
            }
        }

        [TestClass]
        public class GetCaseClosureReasons : CurrentUserSetup
        {
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            private IReferenceDataRepository referenceDataRepository;
            private IEnumerable<CaseClosureReason> caseClosureResponse;
            private IRetentionAlertService retentionAlertService;
            private Mock<IStudentRepository> studentRepositoryMock;
            private IStudentRepository studentRepository;
            private Mock<IConfigurationRepository> configurationRepositoryMock;
            private IConfigurationRepository configurationRepository;
            private ICurrentUserFactory currentUserFactory;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;

            private Mock<IRetentionAlertRepository> retentionAlertRepositoryMock;
            private IRetentionAlertRepository retentionAlertRepository;

            [TestInitialize]
            public async void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                logger = new Mock<ILogger>().Object;

                studentRepositoryMock = new Mock<IStudentRepository>();
                studentRepository = studentRepositoryMock.Object;
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                configurationRepository = configurationRepositoryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;

                retentionAlertRepositoryMock = new Mock<IRetentionAlertRepository>();
                retentionAlertRepository = retentionAlertRepositoryMock.Object;

                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;

                caseClosureResponse = BuildCaseClosureReasonsResponse();
                referenceDataRepositoryMock.Setup(repo => repo.GetCaseClosureReasonsAsync()).Returns(Task.FromResult(caseClosureResponse));

                // Mock Adapters
                var caseClosureDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.CaseClosureReason, Ellucian.Colleague.Dtos.Student.CaseClosureReason>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.CaseClosureReason, Ellucian.Colleague.Dtos.Student.CaseClosureReason>()).Returns(caseClosureDtoAdapter);

                // Set up current user
                currentUserFactory = new CurrentUserSetup.FacultyUserFactory();

                retentionAlertService = new RetentionAlertService(adapterRegistry, referenceDataRepository, studentRepository, configurationRepository, currentUserFactory, roleRepo, logger, retentionAlertRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistry = null;
                roleRepo = null;
                retentionAlertService = null;
                referenceDataRepository = null;
                studentRepository = null;
                configurationRepository = null;
                retentionAlertRepository = null;
            }

            [TestMethod]
            public async Task GetCaseClosureReasons_ReturnsCaseTypes()
            {
                referenceDataRepositoryMock.Setup(repo => repo.GetCaseClosureReasonsAsync()).Returns(Task.FromResult(caseClosureResponse));
                var caseClosureReasonDto = await retentionAlertService.GetCaseClosureReasonsAsync();
                Assert.AreEqual(2, caseClosureReasonDto.Count());
                Assert.AreEqual(caseClosureResponse.ElementAt(0).Code, caseClosureReasonDto.ElementAt(0).Code);
                Assert.AreEqual(caseClosureResponse.ElementAt(0).Description, caseClosureReasonDto.ElementAt(0).Description);
                Assert.AreEqual(caseClosureResponse.ElementAt(0).ClosureReasonId, caseClosureReasonDto.ElementAt(0).ClosureReasonId);
                Assert.AreEqual(caseClosureResponse.ElementAt(0).CaseCategories.Count, caseClosureReasonDto.ElementAt(0).CaseCategories.Count());

            }

            private IEnumerable<CaseClosureReason> BuildCaseClosureReasonsResponse()
            {
                List<CaseClosureReason> caseClosureReasons = new List<CaseClosureReason>();
                caseClosureReasons.Add(new CaseClosureReason("c", "Closed","1",new List<string>() { "3","4"}));
                caseClosureReasons.Add(new CaseClosureReason("D", "Deleted", "2", new List<string>() { "5", "6" }));
                return caseClosureReasons;
            }
        }

        [TestClass]
        public class GetRetentionAlertCases : RetentionAlertServiceTests
        {
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            private IReferenceDataRepository referenceDataRepository;
            private IRetentionAlertService retentionAlertService;
            private Mock<IStudentRepository> studentRepositoryMock;
            private IStudentRepository studentRepository;
            private Mock<IConfigurationRepository> configurationRepositoryMock;
            private IConfigurationRepository configurationRepository;
            private ICurrentUserFactory currentUserFactory;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;

            private Mock<IRetentionAlertRepository> retentionAlertRepositoryMock;
            private IRetentionAlertRepository retentionAlertRepository;

            private List<RetentionAlertWorkCase> retentionAlertWorkCase;
            private Dtos.Student.RetentionAlertQueryCriteria criteria;

            [TestInitialize]
            public void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                logger = new Mock<ILogger>().Object;

                studentRepositoryMock = new Mock<IStudentRepository>();
                studentRepository = studentRepositoryMock.Object;
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                configurationRepository = configurationRepositoryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;

                retentionAlertRepositoryMock = new Mock<IRetentionAlertRepository>();
                retentionAlertRepository = retentionAlertRepositoryMock.Object;

                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;

                retentionAlertWorkCase = new List<RetentionAlertWorkCase>()
                {
                    new RetentionAlertWorkCase("31", "0000011")
                    {
                        CaseOwner = "case owner",
                        Category = "Early.Alert",
                        Priority = "Medium",
                        DateCreated = DateTime.Now.AddDays(-10),
                        Status = "New",
                        CategoryDescription = "Category Description"
                    },
                    new RetentionAlertWorkCase("32", "0000012")
                    {
                        CaseOwner = "test owner",
                        Category = "Early.Alert",
                        Priority = "High",
                        DateCreated = DateTime.Now.AddDays(-5),
                        Status = "Active",
                        CategoryDescription = "Category Description"
                    }
                };

                criteria = new Dtos.Student.RetentionAlertQueryCriteria() { CaseIds = new List<string> { "31" }, StudentSearchKeyword = "000011" };

                retentionAlertRepositoryMock.Setup(repo => repo.GetRetentionAlertCasesAsync(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>())).ReturnsAsync(retentionAlertWorkCase);

                // Mock Adapters
                var workCaseDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCase, Ellucian.Colleague.Domain.Base.Entities.RetentionAlertWorkCase>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCase, Ellucian.Colleague.Domain.Base.Entities.RetentionAlertWorkCase>()).Returns(workCaseDtoAdapter);

                var responseDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.RetentionAlertWorkCase, Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCase>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.RetentionAlertWorkCase, Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCase>()).Returns(responseDtoAdapter);

                // Set up current user
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();
                retentionAlertService = new RetentionAlertService(adapterRegistry, referenceDataRepository, studentRepository, configurationRepository, currentUserFactory, roleRepo, logger, retentionAlertRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistry = null;
                roleRepo = null;
                retentionAlertService = null;
                referenceDataRepository = null;
                studentRepository = null;
                configurationRepository = null;
                retentionAlertRepository = null;
            }

            [TestMethod]
            public async Task GetRetentionAlertCases_returns_Success()
            {
                // Setup
                advisorAnyAdviseeRole.AddPermission(new Permission(RetentionAlertPermissionCodes.WorkAnyCase));
                advisorAnyAdviseeRole.AddPermission(new Permission(RetentionAlertPermissionCodes.WorkCases));

                roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role>() { advisorAnyAdviseeRole });
                roleRepoMock.Setup(rr => rr.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { advisorAnyAdviseeRole });

                var casesDtos = await retentionAlertService.GetRetentionAlertCasesAsync(criteria);
                // Assert
                Assert.IsTrue(casesDtos is IEnumerable<Dtos.Student.RetentionAlertWorkCase>);
                Assert.AreEqual(2, casesDtos.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task GetRetentionAlertCases_NoPermission()
            {
                // Setup
                roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role>());
                roleRepoMock.Setup(rr => rr.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>());
                // Test
                var programs = await retentionAlertService.GetRetentionAlertCasesAsync(criteria);
            }
        }

        [TestClass]
        public class AddRetentionAlertCase : RetentionAlertServiceTests
        {
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            private IReferenceDataRepository referenceDataRepository;
            private IRetentionAlertService retentionAlertService;
            private Mock<IStudentRepository> studentRepositoryMock;
            private IStudentRepository studentRepository;
            private Mock<IConfigurationRepository> configurationRepositoryMock;
            private IConfigurationRepository configurationRepository;
            private ICurrentUserFactory currentUserFactory;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;

            private Mock<IRetentionAlertRepository> retentionAlertRepositoryMock;
            private IRetentionAlertRepository retentionAlertRepository;

            private Dtos.Student.RetentionAlertCase addCase;

            [TestInitialize]
            public void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                logger = new Mock<ILogger>().Object;

                studentRepositoryMock = new Mock<IStudentRepository>();
                studentRepository = studentRepositoryMock.Object;
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                configurationRepository = configurationRepositoryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;

                retentionAlertRepositoryMock = new Mock<IRetentionAlertRepository>();
                retentionAlertRepository = retentionAlertRepositoryMock.Object;

                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;

                RetentionAlertCaseCreateResponse createResponse = new RetentionAlertCaseCreateResponse()
                {
                    CaseId = "31",
                    CaseItemsId = "32",
                    CaseStatus = "ACTIVE",
                    OwnerIds = new List<string>(),
                    OwnerNames = new List<string>(),
                    OwnerRoles = new List<string>(),
                    OwnerRoleTitles = new List<string>(),
                    ErrorMessages = new List<string>()
                };

                retentionAlertRepositoryMock.Setup(repo => repo.AddRetentionAlertCaseAsync(It.IsAny<RetentionAlertCase>())).ReturnsAsync(createResponse);

                // Mock Adapters
                var caseDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Dtos.Student.RetentionAlertCase, Ellucian.Colleague.Domain.Base.Entities.RetentionAlertCase>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Dtos.Student.RetentionAlertCase, Ellucian.Colleague.Domain.Base.Entities.RetentionAlertCase>()).Returns(caseDtoAdapter);

                var caseResponseDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.RetentionAlertCaseCreateResponse, Ellucian.Colleague.Dtos.Student.RetentionAlertCaseCreateResponse>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.RetentionAlertCaseCreateResponse, Ellucian.Colleague.Dtos.Student.RetentionAlertCaseCreateResponse>()).Returns(caseResponseDtoAdapter);

                // Set up current user
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();

                retentionAlertService = new RetentionAlertService(adapterRegistry, referenceDataRepository, studentRepository, configurationRepository, currentUserFactory, roleRepo, logger, retentionAlertRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistry = null;
                roleRepo = null;
                retentionAlertService = null;
                referenceDataRepository = null;
                studentRepository = null;
                configurationRepository = null;
                retentionAlertRepository = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AddRetentionAlertCase_NullArgument()
            {
                var serviceResult = await retentionAlertService.AddRetentionAlertCaseAsync(null);
            }

            [TestMethod]
            public async Task AddRetentionAlertCase_Success()
            {
                addCase = new Dtos.Student.RetentionAlertCase
                {
                    StudentId = "0000011",
                    CaseType = "EARLY.ALERT",
                    MethodOfContact = new List<string> { "Email" },
                    Notes = new List<string> { "case notes" },
                    Summary = "case summary"
                };

                advisorAnyAdviseeRole.AddPermission(new Permission(RetentionAlertPermissionCodes.WorkAnyCase));
                advisorAnyAdviseeRole.AddPermission(new Permission(RetentionAlertPermissionCodes.WorkCases));
                advisorAnyAdviseeRole.AddPermission(new Permission(RetentionAlertPermissionCodes.ContributeToCases));
                roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role>() { advisorAnyAdviseeRole });
                roleRepoMock.Setup(rr => rr.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { advisorAnyAdviseeRole });

                var createResponse = await retentionAlertService.AddRetentionAlertCaseAsync(addCase);
                Assert.IsNotNull(createResponse);
                Assert.AreEqual("31", createResponse.CaseId);
                Assert.AreEqual("32", createResponse.CaseItemsId);
                Assert.AreEqual("ACTIVE", createResponse.CaseStatus);
                Assert.AreEqual(0, createResponse.OwnerIds.Count());
                Assert.AreEqual(0, createResponse.OwnerNames.Count());
                Assert.AreEqual(0, createResponse.OwnerRoles.Count());
                Assert.AreEqual(0, createResponse.OwnerRoleTitles.Count());
                Assert.AreEqual(0, createResponse.ErrorMessages.Count());
            }
        }

        [TestClass]
        public class UpdateRetentionAlertCase : CurrentUserSetup
        {
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            private IReferenceDataRepository referenceDataRepository;
            private IRetentionAlertService retentionAlertService;
            private Mock<IStudentRepository> studentRepositoryMock;
            private IStudentRepository studentRepository;
            private Mock<IConfigurationRepository> configurationRepositoryMock;
            private IConfigurationRepository configurationRepository;
            private ICurrentUserFactory currentUserFactory;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;

            private Mock<IRetentionAlertRepository> retentionAlertRepositoryMock;
            private IRetentionAlertRepository retentionAlertRepository;

            private Dtos.Student.RetentionAlertCase addCase;

            [TestInitialize]
            public void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                logger = new Mock<ILogger>().Object;

                studentRepositoryMock = new Mock<IStudentRepository>();
                studentRepository = studentRepositoryMock.Object;
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                configurationRepository = configurationRepositoryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;

                retentionAlertRepositoryMock = new Mock<IRetentionAlertRepository>();
                retentionAlertRepository = retentionAlertRepositoryMock.Object;

                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;

                RetentionAlertCaseCreateResponse createResponse = new RetentionAlertCaseCreateResponse()
                {
                    CaseId = "31",
                    CaseItemsId = "32",
                    CaseStatus = "ACTIVE",
                    OwnerIds = new List<string>(),
                    OwnerNames = new List<string>(),
                    OwnerRoles = new List<string>(),
                    OwnerRoleTitles = new List<string>(),
                    ErrorMessages = new List<string>()
                };

                retentionAlertRepositoryMock.Setup(repo => repo.UpdateRetentionAlertCaseAsync(It.IsAny<string>(), It.IsAny<RetentionAlertCase>())).ReturnsAsync(createResponse);

                // Mock Adapters
                var caseDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Dtos.Student.RetentionAlertCase, Ellucian.Colleague.Domain.Base.Entities.RetentionAlertCase>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Dtos.Student.RetentionAlertCase, Ellucian.Colleague.Domain.Base.Entities.RetentionAlertCase>()).Returns(caseDtoAdapter);

                var caseResponseDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.RetentionAlertCaseCreateResponse, Ellucian.Colleague.Dtos.Student.RetentionAlertCaseCreateResponse>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.RetentionAlertCaseCreateResponse, Ellucian.Colleague.Dtos.Student.RetentionAlertCaseCreateResponse>()).Returns(caseResponseDtoAdapter);

                // Set up current user
                currentUserFactory = new CurrentUserSetup.FacultyUserFactory();

                retentionAlertService = new RetentionAlertService(adapterRegistry, referenceDataRepository, studentRepository, configurationRepository, currentUserFactory, roleRepo, logger, retentionAlertRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistry = null;
                roleRepo = null;
                retentionAlertService = null;
                referenceDataRepository = null;
                studentRepository = null;
                configurationRepository = null;
                retentionAlertRepository = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task UpdateRetentionAlertCase_NullArgument()
            {
                var serviceResult = await retentionAlertService.UpdateRetentionAlertCaseAsync("", null);
            }

            [TestMethod]
            public async Task UpdateRetentionAlertCase_Success()
            {
                addCase = new Dtos.Student.RetentionAlertCase
                {
                    StudentId = "0000011",
                    CaseType = "EARLY.ALERT",
                    MethodOfContact = new List<string> { "Email" },
                    Notes = new List<string> { "case notes" },
                    Summary = "case summary"
                };

                var createResponse = await retentionAlertService.UpdateRetentionAlertCaseAsync("31", addCase);
                Assert.IsNotNull(createResponse);
                Assert.AreEqual("31", createResponse.CaseId);
                Assert.AreEqual("32", createResponse.CaseItemsId);
                Assert.AreEqual("ACTIVE", createResponse.CaseStatus);
                Assert.AreEqual(0, createResponse.OwnerIds.Count());
                Assert.AreEqual(0, createResponse.OwnerNames.Count());
                Assert.AreEqual(0, createResponse.OwnerRoles.Count());
                Assert.AreEqual(0, createResponse.OwnerRoleTitles.Count());
                Assert.AreEqual(0, createResponse.ErrorMessages.Count());
            }
        }

        [TestClass]
        public class GetRetentionAlertContributionsAsync : RetentionAlertServiceTests
        {
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            private IReferenceDataRepository referenceDataRepository;
            private IRetentionAlertService retentionAlertService;
            private Mock<IStudentRepository> studentRepositoryMock;
            private IStudentRepository studentRepository;
            private Mock<IConfigurationRepository> configurationRepositoryMock;
            private IConfigurationRepository configurationRepository;
            private ICurrentUserFactory currentUserFactory;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;

            private Mock<IRetentionAlertRepository> retentionAlertRepositoryMock;
            private IRetentionAlertRepository retentionAlertRepository;

            private List<RetentionAlertWorkCase> retentionAlertContributions; //TODO: Rename to Contributions
            private Dtos.Student.ContributionsQueryCriteria contributionsQueryCriteria;

            [TestInitialize]
            public void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                logger = new Mock<ILogger>().Object;

                studentRepositoryMock = new Mock<IStudentRepository>();
                studentRepository = studentRepositoryMock.Object;
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                configurationRepository = configurationRepositoryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;

                retentionAlertRepositoryMock = new Mock<IRetentionAlertRepository>();
                retentionAlertRepository = retentionAlertRepositoryMock.Object;

                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;

                retentionAlertContributions = new List<RetentionAlertWorkCase>()
                {
                    new RetentionAlertWorkCase("31", "0000011")
                    {
                        CaseOwner = "case owner",
                        Category = "Early.Alert",
                        CaseItemIds = "260",
                        Priority = "Medium",
                        DateCreated = DateTime.Now.AddDays(-10),
                        Status = "New",
                        CategoryDescription = "Category Description",
                        Summary = "Case Summary 1"
                    },
                    new RetentionAlertWorkCase("32", "0000012")
                    {
                        CaseOwner = "test owner",
                        Category = "Early.Alert",
                        CaseItemIds = "261",
                        Priority = "High",
                        DateCreated = DateTime.Now.AddDays(-5),
                        Status = "Active",
                        CategoryDescription = "Category Description",
                        Summary = "Case Summary 2"
                    }
                };
                contributionsQueryCriteria = new Dtos.Student.ContributionsQueryCriteria() { IncludeCasesOverOneYear = false, IncludeClosedCases = false, IncludeOwnedCases = false };

                retentionAlertRepositoryMock.Setup(repo => repo.GetRetentionAlertContributionsAsync(It.IsAny<string>(), It.IsAny<ContributionsQueryCriteria>())).ReturnsAsync(retentionAlertContributions);

                // Mock Adapters
                var workCaseDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCase, Ellucian.Colleague.Domain.Base.Entities.RetentionAlertWorkCase>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCase, Ellucian.Colleague.Domain.Base.Entities.RetentionAlertWorkCase>()).Returns(workCaseDtoAdapter);

                // Mock Adapters
                var criteriaDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Dtos.Student.ContributionsQueryCriteria, Ellucian.Colleague.Domain.Base.Entities.ContributionsQueryCriteria>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Dtos.Student.ContributionsQueryCriteria, Ellucian.Colleague.Domain.Base.Entities.ContributionsQueryCriteria>()).Returns(criteriaDtoAdapter);

                var responseDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.RetentionAlertWorkCase, Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCase>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.RetentionAlertWorkCase, Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCase>()).Returns(responseDtoAdapter);

                // Set up current user
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();
                retentionAlertService = new RetentionAlertService(adapterRegistry, referenceDataRepository, studentRepository, configurationRepository, currentUserFactory, roleRepo, logger, retentionAlertRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistry = null;
                roleRepo = null;
                retentionAlertService = null;
                referenceDataRepository = null;
                studentRepository = null;
                configurationRepository = null;
                retentionAlertRepository = null;
            }

            [TestMethod]
            public async Task GetRetentionAlertContributionsAsync_returns_Success()
            {
                // Setup
                advisorAnyAdviseeRole.AddPermission(new Permission(RetentionAlertPermissionCodes.WorkAnyCase));
                advisorAnyAdviseeRole.AddPermission(new Permission(RetentionAlertPermissionCodes.WorkCases));
                advisorAnyAdviseeRole.AddPermission(new Permission(RetentionAlertPermissionCodes.ContributeToCases));
                roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role>() { advisorAnyAdviseeRole });
                roleRepoMock.Setup(rr => rr.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { advisorAnyAdviseeRole });

                var contributionsDtos = await retentionAlertService.GetRetentionAlertContributionsAsync(contributionsQueryCriteria);
                
                Assert.IsTrue(contributionsDtos is IEnumerable<Dtos.Student.RetentionAlertWorkCase>);
                Assert.AreEqual(2, contributionsDtos.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task QueryAdvisorsByPostAsync_NoPermission()
            {
                roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role>());
                roleRepoMock.Setup(rr => rr.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>());
                
                var contributions = await retentionAlertService.GetRetentionAlertContributionsAsync(contributionsQueryCriteria);
            }
        }

        [TestClass]
        public class GetRetentionAlertOpenCasesAsync : RetentionAlertServiceTests
        {
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            private IReferenceDataRepository referenceDataRepository;
            private IRetentionAlertService retentionAlertService;
            private Mock<IStudentRepository> studentRepositoryMock;
            private IStudentRepository studentRepository;
            private Mock<IConfigurationRepository> configurationRepositoryMock;
            private IConfigurationRepository configurationRepository;
            private ICurrentUserFactory currentUserFactory;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;

            private Mock<IRetentionAlertRepository> retentionAlertRepositoryMock;
            private IRetentionAlertRepository retentionAlertRepository;

            private List<RetentionAlertOpenCase> retentionAlertOpenCase;

            [TestInitialize]
            public void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                logger = new Mock<ILogger>().Object;

                studentRepositoryMock = new Mock<IStudentRepository>();
                studentRepository = studentRepositoryMock.Object;
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                configurationRepository = configurationRepositoryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;

                retentionAlertRepositoryMock = new Mock<IRetentionAlertRepository>();
                retentionAlertRepository = retentionAlertRepositoryMock.Object;

                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;

                retentionAlertOpenCase = new List<RetentionAlertOpenCase>()
                {
                    new RetentionAlertOpenCase()
                    {
                        CategoryId = "2",
                        Category = "Early Alert",
                        ThirtyDaysOld = new List<string> { "30,40,50" },
                        SixtyDaysOld = new List<string> { "45" },
                        NinetyDaysOld = new List<string> { "" },
                        OverNinetyDaysOld = new List<string> { "12,23" },
                        TotalOpenCases = new List<string> { "6" }
                    },
                    new RetentionAlertOpenCase()
                    {
                        CategoryId = "3",
                        Category = "Financial Problem",
                        ThirtyDaysOld = new List<string> { "30" },
                        SixtyDaysOld = new List<string> { "45,40,50, 55" } ,
                        NinetyDaysOld = new List<string> { "34" },
                        OverNinetyDaysOld = new List<string> { "11" },
                        TotalOpenCases = new List<string> { "7" }
                    }
                };

                retentionAlertRepositoryMock.Setup(repo => repo.GetRetentionAlertOpenCasesAsync(It.IsAny<string>())).ReturnsAsync(retentionAlertOpenCase);

                // Mock Adapters
                var workCaseDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Dtos.Student.RetentionAlertOpenCase, Ellucian.Colleague.Domain.Base.Entities.RetentionAlertOpenCase>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Dtos.Student.RetentionAlertOpenCase, Ellucian.Colleague.Domain.Base.Entities.RetentionAlertOpenCase>()).Returns(workCaseDtoAdapter);

                var responseDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.RetentionAlertOpenCase, Ellucian.Colleague.Dtos.Student.RetentionAlertOpenCase>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.RetentionAlertOpenCase, Ellucian.Colleague.Dtos.Student.RetentionAlertOpenCase>()).Returns(responseDtoAdapter);

                // Set up current user
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();
                retentionAlertService = new RetentionAlertService(adapterRegistry, referenceDataRepository, studentRepository, configurationRepository, currentUserFactory, roleRepo, logger, retentionAlertRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistry = null;
                roleRepo = null;
                retentionAlertService = null;
                referenceDataRepository = null;
                studentRepository = null;
                configurationRepository = null;
                retentionAlertRepository = null;
            }

            [TestMethod]
            public async Task GetRetentionAlertOpenCasesAsync_returns_Success()
            {
                // Setup
                advisorAnyAdviseeRole.AddPermission(new Permission(RetentionAlertPermissionCodes.WorkAnyCase));
                roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role>() { advisorAnyAdviseeRole });
                roleRepoMock.Setup(rr => rr.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { advisorAnyAdviseeRole });

                var openCasesDtos = await retentionAlertService.GetRetentionAlertOpenCasesAsync();

                Assert.IsTrue(openCasesDtos is IEnumerable<Dtos.Student.RetentionAlertOpenCase>);
                Assert.AreEqual(2, openCasesDtos.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task GetRetentionAlertOpenCasesAsync_NoPermission()
            {
                roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role>());
                roleRepoMock.Setup(rr => rr.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>());

                var openCasesDtos = await retentionAlertService.GetRetentionAlertOpenCasesAsync();
            }
        }
      
        [TestClass]
        public class GetRetentionAlertCaseDetailAsync : RetentionAlertServiceTests
        {
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            private IReferenceDataRepository referenceDataRepository;
            private IRetentionAlertService retentionAlertService;
            private Mock<IStudentRepository> studentRepositoryMock;
            private IStudentRepository studentRepository;
            private Mock<IConfigurationRepository> configurationRepositoryMock;
            private IConfigurationRepository configurationRepository;
            private ICurrentUserFactory currentUserFactory;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;

            private Mock<IRetentionAlertRepository> retentionAlertRepositoryMock;
            private IRetentionAlertRepository retentionAlertRepository;

            private RetentionAlertCaseDetail retentionAlertCaseDetail; 

            [TestInitialize]
            public void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                logger = new Mock<ILogger>().Object;

                studentRepositoryMock = new Mock<IStudentRepository>();
                studentRepository = studentRepositoryMock.Object;
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                configurationRepository = configurationRepositoryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;

                retentionAlertRepositoryMock = new Mock<IRetentionAlertRepository>();
                retentionAlertRepository = retentionAlertRepositoryMock.Object;

                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;

                List<RetentionAlertCaseHistory> caseHistory = new List<RetentionAlertCaseHistory>()
                {
                    new RetentionAlertCaseHistory()
                    {
                        CaseItemType = "Case Item Type 1",
                        CaseItemId = "260",
                        CaseType = "Case Type",
                        DateCreated = DateTime.Now.AddDays(-10),
                        Summary ="Summary 1",
                        ContactMethod = "Phone",
                        DetailedNote =  new List<string>{ "Detailed Notes" },
                        UpdatedBy = "0000011",
                        CaseClosureReason = "8"
                    },
                    new RetentionAlertCaseHistory()
                    {
                        CaseItemType = "Case Item Type 2",
                        CaseItemId = "260",
                        CaseType = "Case Type 1",
                        DateCreated = DateTime.Now.AddDays(-5),
                        Summary ="Summary 2",
                        ContactMethod = "Email",
                        DetailedNote =  new List<string>{ "Detailed Notes" },
                        UpdatedBy = "0000012",
                        CaseClosureReason = "7"
                    }
                };

                retentionAlertCaseDetail = new RetentionAlertCaseDetail("32")
                {
                    CaseHistory = caseHistory,
                    Status = "New",
                    CaseOwner = "case owner",
                    CaseType = "Case Type",
                    CategoryName = "EARLY.ALERT",
                    CategoryId = "1",
                    CreatedBy = "0000010",
                    Priority = "Medium",
                    CasePriorityCode = "M",
                    StudentId = "0000015",
                    CaseReassignmentList = new List<RetentionAlertCaseReassignmentDetail>()
                    {
                        new RetentionAlertCaseReassignmentDetail()
                        {
                            AssignmentCode ="67",
                            Title ="Jose",
                            Role="0",
                            IsSelected = false
                        },
                        new RetentionAlertCaseReassignmentDetail()
                        {
                            AssignmentCode ="68",
                            Title ="George",
                            Role="0",
                            IsSelected = true
                        }
                    }
                };

                retentionAlertRepositoryMock.Setup(repo => repo.GetRetentionAlertCaseDetailAsync(It.IsAny<string>())).ReturnsAsync(retentionAlertCaseDetail);

                var responseDtoAdapter = new Ellucian.Colleague.Coordination.Student.Adapters.RetentionAlertCaseDetailEntityToDtoAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.RetentionAlertCaseDetail, Ellucian.Colleague.Dtos.Student.RetentionAlertCaseDetail>()).Returns(responseDtoAdapter);

                // Set up current user
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();
                retentionAlertService = new RetentionAlertService(adapterRegistry, referenceDataRepository, studentRepository, configurationRepository, currentUserFactory, roleRepo, logger, retentionAlertRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistry = null;
                roleRepo = null;
                retentionAlertService = null;
                referenceDataRepository = null;
                studentRepository = null;
                configurationRepository = null;
                retentionAlertRepository = null;
            }

            [TestMethod]
            public async Task GetRetentionAlertContributionsAsync_returns_Success()
            {
                // Setup
                advisorAnyAdviseeRole.AddPermission(new Permission(RetentionAlertPermissionCodes.WorkAnyCase));
                advisorAnyAdviseeRole.AddPermission(new Permission(RetentionAlertPermissionCodes.WorkCases));
                advisorAnyAdviseeRole.AddPermission(new Permission(RetentionAlertPermissionCodes.ContributeToCases));
                roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role>() { advisorAnyAdviseeRole });
                roleRepoMock.Setup(rr => rr.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { advisorAnyAdviseeRole });

                var caseDetailDtos = await retentionAlertService.GetRetentionAlertCaseDetailAsync("31");

                Assert.IsTrue(caseDetailDtos is Dtos.Student.RetentionAlertCaseDetail);
                Assert.IsNotNull(caseDetailDtos);
                Assert.AreEqual("32", caseDetailDtos.CaseId);
                Assert.AreEqual(2, caseDetailDtos.CaseReassignmentList.Count());
                Assert.AreEqual(retentionAlertCaseDetail.Status, caseDetailDtos.Status);
                Assert.AreEqual(retentionAlertCaseDetail.CategoryName, caseDetailDtos.CategoryName);
                Assert.AreEqual(retentionAlertCaseDetail.CategoryId, caseDetailDtos.CategoryId);
                Assert.AreEqual(retentionAlertCaseDetail.CaseType, caseDetailDtos.CaseType);
                Assert.AreEqual(retentionAlertCaseDetail.CreatedBy, caseDetailDtos.CreatedBy);
                Assert.AreEqual(retentionAlertCaseDetail.CaseOwner, caseDetailDtos.CaseOwner);
                Assert.AreEqual(retentionAlertCaseDetail.Priority, caseDetailDtos.Priority);
                Assert.AreEqual(retentionAlertCaseDetail.CasePriorityCode, caseDetailDtos.CasePriorityCode);
                Assert.AreEqual(retentionAlertCaseDetail.StudentId, caseDetailDtos.StudentId);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task GetRetentionAlertContributionsAsync_NoPermission()
            {
                roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role>());
                roleRepoMock.Setup(rr => rr.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>());

                var caseDetail = await retentionAlertService.GetRetentionAlertCaseDetailAsync("32");
            }
        }

        [TestClass]
        public class GetRetentionAlertPermissionsAsync : RetentionAlertServiceTests
        {
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            private IReferenceDataRepository referenceDataRepository;
            private IRetentionAlertService retentionAlertService;
            private Mock<IStudentRepository> studentRepositoryMock;
            private IStudentRepository studentRepository;
            private Mock<IConfigurationRepository> configurationRepositoryMock;
            private IConfigurationRepository configurationRepository;
            private ICurrentUserFactory currentUserFactory;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;

            private Mock<IRetentionAlertRepository> retentionAlertRepositoryMock;
            private IRetentionAlertRepository retentionAlertRepository;

            private RetentionAlertPermissions retentionAlertPermissions;

            [TestInitialize]
            public void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                logger = new Mock<ILogger>().Object;

                studentRepositoryMock = new Mock<IStudentRepository>();
                studentRepository = studentRepositoryMock.Object;
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                configurationRepository = configurationRepositoryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;

                retentionAlertRepositoryMock = new Mock<IRetentionAlertRepository>();
                retentionAlertRepository = retentionAlertRepositoryMock.Object;

                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;

                
                IEnumerable<string> permissionCodes = new List<string> { RetentionAlertPermissionCodes.WorkCases, RetentionAlertPermissionCodes.WorkAnyCase, RetentionAlertPermissionCodes.ContributeToCases };

                retentionAlertPermissions = new RetentionAlertPermissions(permissionCodes);

                var permissionDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.RetentionAlertPermissions, Ellucian.Colleague.Dtos.Student.RetentionAlertPermissions>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.RetentionAlertPermissions, Ellucian.Colleague.Dtos.Student.RetentionAlertPermissions>()).Returns(permissionDtoAdapter);

                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();
                retentionAlertService = new RetentionAlertService(adapterRegistry, referenceDataRepository, studentRepository, configurationRepository, currentUserFactory, roleRepo, logger, retentionAlertRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistry = null;
                roleRepo = null;
                retentionAlertService = null;
                referenceDataRepository = null;
                studentRepository = null;
                configurationRepository = null;
                retentionAlertRepository = null;
            }

            [TestMethod]
            public async Task GetRetentionAlertContributionsAsync_returns_Success()
            {
                advisorAnyAdviseeRole.AddPermission(new Permission(RetentionAlertPermissionCodes.WorkCases));
                advisorAnyAdviseeRole.AddPermission(new Permission(RetentionAlertPermissionCodes.WorkAnyCase));
                advisorAnyAdviseeRole.AddPermission(new Permission(RetentionAlertPermissionCodes.ContributeToCases));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorAnyAdviseeRole });

                var result = await retentionAlertService.GetRetentionAlertPermissionsAsync();

                Assert.IsTrue(result.CanWorkCases);
                Assert.IsTrue(result.CanWorkAnyCase);
                Assert.IsTrue(result.CanContributeToCases);
            }
        }

        [TestClass]
        public class RetentionAlertWorkCaseActions : RetentionAlertServiceTests
        {
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            private IReferenceDataRepository referenceDataRepository;
            private IRetentionAlertService retentionAlertService;
            private Mock<IStudentRepository> studentRepositoryMock;
            private IStudentRepository studentRepository;
            private Mock<IConfigurationRepository> configurationRepositoryMock;
            private IConfigurationRepository configurationRepository;
            private ICurrentUserFactory currentUserFactory;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;

            private Mock<IRetentionAlertRepository> retentionAlertRepositoryMock;
            private IRetentionAlertRepository retentionAlertRepository;

            [TestInitialize]
            public void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                logger = new Mock<ILogger>().Object;

                studentRepositoryMock = new Mock<IStudentRepository>();
                studentRepository = studentRepositoryMock.Object;
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                configurationRepository = configurationRepositoryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;

                retentionAlertRepositoryMock = new Mock<IRetentionAlertRepository>();
                retentionAlertRepository = retentionAlertRepositoryMock.Object;

                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;


                var responseDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.RetentionAlertWorkCaseActionResponse, Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCaseActionResponse>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.RetentionAlertWorkCaseActionResponse, Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCaseActionResponse>()).Returns(responseDtoAdapter);

                // Set up current user
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();

                // Setup
                advisorAnyAdviseeRole.AddPermission(new Permission(PlanningPermissionCodes.AllAccessAnyAdvisee));
                advisorAnyAdviseeRole.AddPermission(new Permission(PlanningPermissionCodes.ReviewAnyAdvisee));
                advisorAnyAdviseeRole.AddPermission(new Permission(PlanningPermissionCodes.UpdateAnyAdvisee));
                advisorAnyAdviseeRole.AddPermission(new Permission(PlanningPermissionCodes.UpdateAssignedAdvisees));
                roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role>() { advisorAnyAdviseeRole });
                roleRepoMock.Setup(rr => rr.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { advisorAnyAdviseeRole });

            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistry = null;
                roleRepo = null;
                retentionAlertService = null;
                referenceDataRepository = null;
                studentRepository = null;
                configurationRepository = null;
                retentionAlertRepository = null;
            }

            [TestMethod]
            public async Task AddRetentionAlertCaseNoteAsync_returns_Success()
            {
                // Setup
                advisorAnyAdviseeRole.AddPermission(new Permission(RetentionAlertPermissionCodes.WorkAnyCase));
                advisorAnyAdviseeRole.AddPermission(new Permission(RetentionAlertPermissionCodes.WorkCases));
                advisorAnyAdviseeRole.AddPermission(new Permission(RetentionAlertPermissionCodes.ContributeToCases));
                roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role>() { advisorAnyAdviseeRole });
                roleRepoMock.Setup(rr => rr.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { advisorAnyAdviseeRole });

                // Mock Adapters
                var workCaseDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCaseNote, Ellucian.Colleague.Domain.Base.Entities.RetentionAlertWorkCaseNote>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCaseNote, Ellucian.Colleague.Domain.Base.Entities.RetentionAlertWorkCaseNote>()).Returns(workCaseDtoAdapter);

                var actionDto = new Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCaseNote()
                {
                    Summary = "Add Case Note Summary",
                    Notes = new List<string>() { "Add Case Note Notes" }
                };

                var response = new RetentionAlertWorkCaseActionResponse()
                {
                    CaseId = "1",
                    HasError = false,
                    ErrorMessages = new List<string>()
                };

                retentionAlertRepositoryMock.Setup(repo => repo.AddRetentionAlertCaseNoteAsync("1", It.IsAny<RetentionAlertWorkCaseNote>())).ReturnsAsync(response);
                retentionAlertService = new RetentionAlertService(adapterRegistry, referenceDataRepository, studentRepository, configurationRepository, currentUserFactory, roleRepo, logger, retentionAlertRepository);

                var retentionAlertWorkCaseActionResponse = await retentionAlertService.AddRetentionAlertCaseNoteAsync("1", actionDto);

                Assert.IsNotNull(retentionAlertWorkCaseActionResponse);
                Assert.AreEqual(retentionAlertWorkCaseActionResponse.CaseId, response.CaseId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AddRetentionAlertCaseNoteAsync_returns_Failure()
            {
                // Mock Adapters
                var workCaseDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCaseNote, Ellucian.Colleague.Domain.Base.Entities.RetentionAlertWorkCaseNote>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCaseNote, Ellucian.Colleague.Domain.Base.Entities.RetentionAlertWorkCaseNote>()).Returns(workCaseDtoAdapter);

                Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCaseNote actionDto = null;

                var response = new RetentionAlertWorkCaseActionResponse()
                {
                    CaseId = "1",
                    HasError = false,
                    ErrorMessages = new List<string>()
                };

                retentionAlertRepositoryMock.Setup(repo => repo.AddRetentionAlertCaseNoteAsync("1", It.IsAny<RetentionAlertWorkCaseNote>())).ReturnsAsync(response);
                retentionAlertService = new RetentionAlertService(adapterRegistry, referenceDataRepository, studentRepository, configurationRepository, currentUserFactory, roleRepo, logger, retentionAlertRepository);

                var retentionAlertWorkCaseActionResponse = await retentionAlertService.AddRetentionAlertCaseNoteAsync("1", actionDto);

            }

            [TestMethod]
            public async Task AddRetentionAlertCaseCommCodeAsync_returns_Success()
            {
                // Setup
                advisorAnyAdviseeRole.AddPermission(new Permission(RetentionAlertPermissionCodes.WorkAnyCase));
                advisorAnyAdviseeRole.AddPermission(new Permission(RetentionAlertPermissionCodes.WorkCases));
                roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role>() { advisorAnyAdviseeRole });
                roleRepoMock.Setup(rr => rr.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { advisorAnyAdviseeRole });

                // Mock Adapters
                var workCaseDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCaseCommCode, Ellucian.Colleague.Domain.Base.Entities.RetentionAlertWorkCaseCommCode>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCaseCommCode, Ellucian.Colleague.Domain.Base.Entities.RetentionAlertWorkCaseCommCode>()).Returns(workCaseDtoAdapter);

                var actionDto = new Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCaseCommCode()
                {
                    CommunicationCode = "ABCD"
                };

                var response = new RetentionAlertWorkCaseActionResponse()
                {
                    CaseId = "1",
                    HasError = false,
                    ErrorMessages = new List<string>()
                };

                retentionAlertRepositoryMock.Setup(repo => repo.AddRetentionAlertCaseCommCodeAsync("1", It.IsAny<RetentionAlertWorkCaseCommCode>())).ReturnsAsync(response);
                retentionAlertService = new RetentionAlertService(adapterRegistry, referenceDataRepository, studentRepository, configurationRepository, currentUserFactory, roleRepo, logger, retentionAlertRepository);

                var retentionAlertWorkCaseActionResponse = await retentionAlertService.AddRetentionAlertCaseCommCodeAsync("1", actionDto);

                Assert.IsNotNull(retentionAlertWorkCaseActionResponse);
                Assert.AreEqual(retentionAlertWorkCaseActionResponse.CaseId, response.CaseId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AddRetentionAlertCaseCommCodeAsync_returns_Failure()
            {
                // Mock Adapters
                var workCaseDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCaseCommCode, Ellucian.Colleague.Domain.Base.Entities.RetentionAlertWorkCaseCommCode>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCaseCommCode, Ellucian.Colleague.Domain.Base.Entities.RetentionAlertWorkCaseCommCode>()).Returns(workCaseDtoAdapter);

                Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCaseCommCode actionDto = null;

                var response = new RetentionAlertWorkCaseActionResponse()
                {
                    CaseId = "1",
                    HasError = false,
                    ErrorMessages = new List<string>()
                };

                retentionAlertRepositoryMock.Setup(repo => repo.AddRetentionAlertCaseCommCodeAsync("1", It.IsAny<RetentionAlertWorkCaseCommCode>())).ReturnsAsync(response);
                retentionAlertService = new RetentionAlertService(adapterRegistry, referenceDataRepository, studentRepository, configurationRepository, currentUserFactory, roleRepo, logger, retentionAlertRepository);

                var retentionAlertWorkCaseActionResponse = await retentionAlertService.AddRetentionAlertCaseCommCodeAsync("1", actionDto);

            }

            [TestMethod]
            public async Task AddRetentionAlertCaseTypeAsync_returns_Success()
            {
                // Setup
                advisorAnyAdviseeRole.AddPermission(new Permission(RetentionAlertPermissionCodes.WorkAnyCase));
                advisorAnyAdviseeRole.AddPermission(new Permission(RetentionAlertPermissionCodes.WorkCases));
                roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role>() { advisorAnyAdviseeRole });
                roleRepoMock.Setup(rr => rr.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { advisorAnyAdviseeRole });

                // Mock Adapters
                var workCaseDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCaseType, Ellucian.Colleague.Domain.Base.Entities.RetentionAlertWorkCaseType>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCaseType, Ellucian.Colleague.Domain.Base.Entities.RetentionAlertWorkCaseType>()).Returns(workCaseDtoAdapter);

                var actionDto = new Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCaseType()
                {
                    CaseType = "ABCD",
                    Notes = new List<string>()
                };

                var response = new RetentionAlertWorkCaseActionResponse()
                {
                    CaseId = "1",
                    HasError = false,
                    ErrorMessages = new List<string>()
                };

                retentionAlertRepositoryMock.Setup(repo => repo.AddRetentionAlertCaseTypeAsync("1", It.IsAny<RetentionAlertWorkCaseType>())).ReturnsAsync(response);
                retentionAlertService = new RetentionAlertService(adapterRegistry, referenceDataRepository, studentRepository, configurationRepository, currentUserFactory, roleRepo, logger, retentionAlertRepository);

                var retentionAlertWorkCaseActionResponse = await retentionAlertService.AddRetentionAlertCaseTypeAsync("1", actionDto);

                Assert.IsNotNull(retentionAlertWorkCaseActionResponse);
                Assert.AreEqual(retentionAlertWorkCaseActionResponse.CaseId, response.CaseId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AddRetentionAlertCaseTypeAsync_returns_Failure()
            {
                // Mock Adapters
                var workCaseDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCaseType, Ellucian.Colleague.Domain.Base.Entities.RetentionAlertWorkCaseType>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCaseType, Ellucian.Colleague.Domain.Base.Entities.RetentionAlertWorkCaseType>()).Returns(workCaseDtoAdapter);

                Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCaseType actionDto = null;

                var response = new RetentionAlertWorkCaseActionResponse()
                {
                    CaseId = "1",
                    HasError = false,
                    ErrorMessages = new List<string>()
                };

                retentionAlertRepositoryMock.Setup(repo => repo.AddRetentionAlertCaseTypeAsync("1", It.IsAny<RetentionAlertWorkCaseType>())).ReturnsAsync(response);
                retentionAlertService = new RetentionAlertService(adapterRegistry, referenceDataRepository, studentRepository, configurationRepository, currentUserFactory, roleRepo, logger, retentionAlertRepository);

                var retentionAlertWorkCaseActionResponse = await retentionAlertService.AddRetentionAlertCaseTypeAsync("1", actionDto);

            }

            [TestMethod]
            public async Task ChangeRetentionAlertCasePriorityAsync_returns_Success()
            {
                // Setup
                advisorAnyAdviseeRole.AddPermission(new Permission(RetentionAlertPermissionCodes.WorkAnyCase));
                advisorAnyAdviseeRole.AddPermission(new Permission(RetentionAlertPermissionCodes.WorkCases));
                roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role>() { advisorAnyAdviseeRole });
                roleRepoMock.Setup(rr => rr.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { advisorAnyAdviseeRole });

                // Mock Adapters
                var workCaseDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCasePriority, Ellucian.Colleague.Domain.Base.Entities.RetentionAlertWorkCasePriority>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCasePriority, Ellucian.Colleague.Domain.Base.Entities.RetentionAlertWorkCasePriority>()).Returns(workCaseDtoAdapter);

                var actionDto = new Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCasePriority()
                {
                    Priority = "H"
                };

                var response = new RetentionAlertWorkCaseActionResponse()
                {
                    CaseId = "1",
                    HasError = false,
                    ErrorMessages = new List<string>()
                };

                retentionAlertRepositoryMock.Setup(repo => repo.ChangeRetentionAlertCasePriorityAsync("1", It.IsAny<RetentionAlertWorkCasePriority>())).ReturnsAsync(response);
                retentionAlertService = new RetentionAlertService(adapterRegistry, referenceDataRepository, studentRepository, configurationRepository, currentUserFactory, roleRepo, logger, retentionAlertRepository);

                var retentionAlertWorkCaseActionResponse = await retentionAlertService.ChangeRetentionAlertCasePriorityAsync("1", actionDto);

                Assert.IsNotNull(retentionAlertWorkCaseActionResponse);
                Assert.AreEqual(retentionAlertWorkCaseActionResponse.CaseId, response.CaseId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ChangeRetentionAlertCasePriorityAsync_returns_Failure()
            {
                // Mock Adapters
                var workCaseDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCaseType, Ellucian.Colleague.Domain.Base.Entities.RetentionAlertWorkCaseType>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCaseType, Ellucian.Colleague.Domain.Base.Entities.RetentionAlertWorkCaseType>()).Returns(workCaseDtoAdapter);

                Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCasePriority actionDto = null;

                var response = new RetentionAlertWorkCaseActionResponse()
                {
                    CaseId = "1",
                    HasError = false,
                    ErrorMessages = new List<string>()
                };

                retentionAlertRepositoryMock.Setup(repo => repo.ChangeRetentionAlertCasePriorityAsync("1", It.IsAny<RetentionAlertWorkCasePriority>())).ReturnsAsync(response);
                retentionAlertService = new RetentionAlertService(adapterRegistry, referenceDataRepository, studentRepository, configurationRepository, currentUserFactory, roleRepo, logger, retentionAlertRepository);

                var retentionAlertWorkCaseActionResponse = await retentionAlertService.ChangeRetentionAlertCasePriorityAsync("1", actionDto);

            }

            [TestMethod]
            public async Task CloseRetentionAlertCaseAsync_returns_Success()
            {
                // Setup
                advisorAnyAdviseeRole.AddPermission(new Permission(RetentionAlertPermissionCodes.WorkAnyCase));
                advisorAnyAdviseeRole.AddPermission(new Permission(RetentionAlertPermissionCodes.WorkCases));
                roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role>() { advisorAnyAdviseeRole });
                roleRepoMock.Setup(rr => rr.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { advisorAnyAdviseeRole });

                // Mock Adapters
                var workCaseDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCaseClose, Ellucian.Colleague.Domain.Base.Entities.RetentionAlertWorkCaseClose>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCaseClose, Ellucian.Colleague.Domain.Base.Entities.RetentionAlertWorkCaseClose>()).Returns(workCaseDtoAdapter);

                var actionDto = new Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCaseClose()
                {
                    ClosureReason = "Withdraw",
                    Summary = "Student withdrew",
                    Notes = new List<string>() { "student withdrew" }
                };

                var response = new RetentionAlertWorkCaseActionResponse()
                {
                    CaseId = "1",
                    HasError = false,
                    ErrorMessages = new List<string>()
                };

                retentionAlertRepositoryMock.Setup(repo => repo.CloseRetentionAlertCaseAsync("1", It.IsAny<RetentionAlertWorkCaseClose>())).ReturnsAsync(response);
                retentionAlertService = new RetentionAlertService(adapterRegistry, referenceDataRepository, studentRepository, configurationRepository, currentUserFactory, roleRepo, logger, retentionAlertRepository);

                var retentionAlertWorkCaseActionResponse = await retentionAlertService.CloseRetentionAlertCaseAsync("1", actionDto);

                Assert.IsNotNull(retentionAlertWorkCaseActionResponse);
                Assert.AreEqual(retentionAlertWorkCaseActionResponse.CaseId, response.CaseId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CloseRetentionAlertCaseAsync_returns_Failure()
            {
                // Mock Adapters
                var workCaseDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCaseClose, Ellucian.Colleague.Domain.Base.Entities.RetentionAlertWorkCaseClose>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCaseClose, Ellucian.Colleague.Domain.Base.Entities.RetentionAlertWorkCaseClose>()).Returns(workCaseDtoAdapter);

                Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCaseClose actionDto = null;

                var response = new RetentionAlertWorkCaseActionResponse()
                {
                    CaseId = "1",
                    HasError = false,
                    ErrorMessages = new List<string>()
                };

                retentionAlertRepositoryMock.Setup(repo => repo.CloseRetentionAlertCaseAsync("1", It.IsAny<RetentionAlertWorkCaseClose>())).ReturnsAsync(response);
                retentionAlertService = new RetentionAlertService(adapterRegistry, referenceDataRepository, studentRepository, configurationRepository, currentUserFactory, roleRepo, logger, retentionAlertRepository);

                var retentionAlertWorkCaseActionResponse = await retentionAlertService.CloseRetentionAlertCaseAsync("1", actionDto);

            }

            [TestMethod]
            public async Task SendRetentionAlertWorkCaseMailAsync_returns_Success()
            {
                // Setup
                advisorAnyAdviseeRole.AddPermission(new Permission(RetentionAlertPermissionCodes.WorkAnyCase));
                advisorAnyAdviseeRole.AddPermission(new Permission(RetentionAlertPermissionCodes.WorkCases));
                advisorAnyAdviseeRole.AddPermission(new Permission(RetentionAlertPermissionCodes.ContributeToCases));
                roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role>() { advisorAnyAdviseeRole });
                roleRepoMock.Setup(rr => rr.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { advisorAnyAdviseeRole });

                // Mock Adapters
                var workCaseDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCaseSendMail, Ellucian.Colleague.Domain.Base.Entities.RetentionAlertWorkCaseSendMail>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCaseSendMail, Ellucian.Colleague.Domain.Base.Entities.RetentionAlertWorkCaseSendMail>()).Returns(workCaseDtoAdapter);

                var sendMailDto = new Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCaseSendMail()
                {                    
                     MailSubject = "Mail subject",
                     MailBody = "Mail body",
                     MailNames = new List<string> { "Jason Bird" , "Kuldeep Singh" },
                     MailAddresses = new List<string> { "jason.bird@ellucian.com", "kuldeep.singh@ellucian.com" },
                     MailTypes = new List<string> { "TO" , "CC"}
                };

                var response = new RetentionAlertWorkCaseActionResponse()
                {
                    CaseId = "1",
                    HasError = false,
                    ErrorMessages = new List<string>()
                };

                retentionAlertRepositoryMock.Setup(repo => repo.SendRetentionAlertWorkCaseMailAsync("1", It.IsAny<RetentionAlertWorkCaseSendMail>())).ReturnsAsync(response);
                retentionAlertService = new RetentionAlertService(adapterRegistry, referenceDataRepository, studentRepository, configurationRepository, currentUserFactory, roleRepo, logger, retentionAlertRepository);

                var retentionAlertWorkCaseActionResponse = await retentionAlertService.SendRetentionAlertWorkCaseMailAsync("1", sendMailDto);

                Assert.IsNotNull(retentionAlertWorkCaseActionResponse);
                Assert.AreEqual(retentionAlertWorkCaseActionResponse.CaseId, response.CaseId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SendRetentionAlertWorkCaseMailAsync_returns_Failure()
            {
                // Mock Adapters
                var workCaseDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCaseSendMail, Ellucian.Colleague.Domain.Base.Entities.RetentionAlertWorkCaseSendMail>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCaseSendMail, Ellucian.Colleague.Domain.Base.Entities.RetentionAlertWorkCaseSendMail>()).Returns(workCaseDtoAdapter);

                Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCaseSendMail sendMailDto = null;

                var response = new RetentionAlertWorkCaseActionResponse()
                {
                    CaseId = "1",
                    HasError = false,
                    ErrorMessages = new List<string>()
                };

                retentionAlertRepositoryMock.Setup(repo => repo.SendRetentionAlertWorkCaseMailAsync("1", It.IsAny<RetentionAlertWorkCaseSendMail>())).ReturnsAsync(response);
                retentionAlertService = new RetentionAlertService(adapterRegistry, referenceDataRepository, studentRepository, configurationRepository, currentUserFactory, roleRepo, logger, retentionAlertRepository);

                var retentionAlertWorkCaseActionResponse = await retentionAlertService.SendRetentionAlertWorkCaseMailAsync("1", sendMailDto);

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AddRetentionAlertCaseReminderAsync_returns_Failure_1()
            {
                // Mock Adapters
                var workCaseDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCaseSetReminder, Ellucian.Colleague.Domain.Base.Entities.RetentionAlertWorkCaseSetReminder>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCaseSetReminder, Ellucian.Colleague.Domain.Base.Entities.RetentionAlertWorkCaseSetReminder>()).Returns(workCaseDtoAdapter);

                Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCaseSetReminder dto = null;

                var response = new RetentionAlertWorkCaseActionResponse()
                {
                    CaseId = "1",
                    HasError = false,
                    ErrorMessages = new List<string>()
                };

                retentionAlertRepositoryMock.Setup(repo => repo.AddRetentionAlertCaseReminderAsync("1", It.IsAny<RetentionAlertWorkCaseSetReminder>())).ReturnsAsync(response);
                retentionAlertService = new RetentionAlertService(adapterRegistry, referenceDataRepository, studentRepository, configurationRepository, currentUserFactory, roleRepo, logger, retentionAlertRepository);

                var retentionAlertWorkCaseActionResponse = await retentionAlertService.AddRetentionAlertCaseReminderAsync("1", dto);
            }

            [TestMethod]
            [ExpectedException(typeof(AutoMapper.AutoMapperMappingException))]
            public async Task AddRetentionAlertCaseReminderAsync_returns_Failure_2()
            {
                // Setup
                advisorAnyAdviseeRole.AddPermission(new Permission(RetentionAlertPermissionCodes.WorkAnyCase));
                advisorAnyAdviseeRole.AddPermission(new Permission(RetentionAlertPermissionCodes.WorkCases));
                advisorAnyAdviseeRole.AddPermission(new Permission(RetentionAlertPermissionCodes.ContributeToCases));
                roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role>() { advisorAnyAdviseeRole });
                roleRepoMock.Setup(rr => rr.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { advisorAnyAdviseeRole });

                // Mock Adapters
                var workCaseDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCaseSetReminder, Ellucian.Colleague.Domain.Base.Entities.RetentionAlertWorkCaseSetReminder>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCaseSetReminder, Ellucian.Colleague.Domain.Base.Entities.RetentionAlertWorkCaseSetReminder>()).Returns(workCaseDtoAdapter);

                Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCaseSetReminder dto = new Dtos.Student.RetentionAlertWorkCaseSetReminder();

                var response = new RetentionAlertWorkCaseActionResponse()
                {
                    CaseId = "1",
                    HasError = false,
                    ErrorMessages = new List<string>()
                };

                retentionAlertRepositoryMock.Setup(repo => repo.AddRetentionAlertCaseReminderAsync("1", It.IsAny<RetentionAlertWorkCaseSetReminder>())).ReturnsAsync(response);
                retentionAlertService = new RetentionAlertService(adapterRegistry, referenceDataRepository, studentRepository, configurationRepository, currentUserFactory, roleRepo, logger, retentionAlertRepository);

                var retentionAlertWorkCaseActionResponse = await retentionAlertService.AddRetentionAlertCaseReminderAsync("1", dto);
            }

            [TestMethod]
            public async Task AddRetentionAlertCaseReminderAsync_returns_Success()
            {
                // Setup
                advisorAnyAdviseeRole.AddPermission(new Permission(RetentionAlertPermissionCodes.WorkAnyCase));
                advisorAnyAdviseeRole.AddPermission(new Permission(RetentionAlertPermissionCodes.WorkCases));
                advisorAnyAdviseeRole.AddPermission(new Permission(RetentionAlertPermissionCodes.ContributeToCases));
                roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role>() { advisorAnyAdviseeRole });
                roleRepoMock.Setup(rr => rr.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { advisorAnyAdviseeRole });

                // Mock Adapters
                var workCaseDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCaseSetReminder, Ellucian.Colleague.Domain.Base.Entities.RetentionAlertWorkCaseSetReminder>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCaseSetReminder, Ellucian.Colleague.Domain.Base.Entities.RetentionAlertWorkCaseSetReminder>()).Returns(workCaseDtoAdapter);

                Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCaseSetReminder dto = new Dtos.Student.RetentionAlertWorkCaseSetReminder()
                {
                    UpdatedBy = "1234567",
                    ReminderDate = new DateTime(2019, 01, 01),
                    Summary = "Summary",
                    Notes = new List<string>() { "Notes" }
                };

                var response = new RetentionAlertWorkCaseActionResponse()
                {
                    CaseId = "1",
                    HasError = false,
                    ErrorMessages = new List<string>()
                };

                retentionAlertRepositoryMock.Setup(repo => repo.AddRetentionAlertCaseReminderAsync("1", It.IsAny<RetentionAlertWorkCaseSetReminder>())).ReturnsAsync(response);
                retentionAlertService = new RetentionAlertService(adapterRegistry, referenceDataRepository, studentRepository, configurationRepository, currentUserFactory, roleRepo, logger, retentionAlertRepository);

                var retentionAlertWorkCaseActionResponse = await retentionAlertService.AddRetentionAlertCaseReminderAsync("1", dto);

                Assert.AreEqual(response.CaseId, retentionAlertWorkCaseActionResponse.CaseId);
                Assert.AreEqual(response.HasError, retentionAlertWorkCaseActionResponse.HasError);                
                CollectionAssert.AreEqual(response.ErrorMessages, retentionAlertWorkCaseActionResponse.ErrorMessages.ToList());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ManageRetentionAlertCaseRemindersAsync_returns_Failure()
            {
                // Mock Adapters
                var workCaseDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCaseManageReminders, Ellucian.Colleague.Domain.Base.Entities.RetentionAlertWorkCaseManageReminders>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCaseManageReminders, Ellucian.Colleague.Domain.Base.Entities.RetentionAlertWorkCaseManageReminders>()).Returns(workCaseDtoAdapter);

                Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCaseManageReminders dto = null;

                var response = new RetentionAlertWorkCaseActionResponse()
                {
                    CaseId = "1",
                    HasError = false,
                    ErrorMessages = new List<string>()
                };

                retentionAlertRepositoryMock.Setup(repo => repo.ManageRetentionAlertCaseRemindersAsync("1", It.IsAny<RetentionAlertWorkCaseManageReminders>())).ReturnsAsync(response);
                retentionAlertService = new RetentionAlertService(adapterRegistry, referenceDataRepository, studentRepository, configurationRepository, currentUserFactory, roleRepo, logger, retentionAlertRepository);

                var retentionAlertWorkCaseActionResponse = await retentionAlertService.ManageRetentionAlertCaseRemindersAsync(null, null);

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ManageRetentionAlertCaseRemindersAsync_returns_Failure1()
            {
                // Mock Adapters
                var workCaseDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCaseManageReminders, Ellucian.Colleague.Domain.Base.Entities.RetentionAlertWorkCaseManageReminders>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCaseManageReminders, Ellucian.Colleague.Domain.Base.Entities.RetentionAlertWorkCaseManageReminders>()).Returns(workCaseDtoAdapter);

                Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCaseManageReminders dto = null;

                var response = new RetentionAlertWorkCaseActionResponse()
                {
                    CaseId = "1",
                    HasError = false,
                    ErrorMessages = new List<string>()
                };

                retentionAlertRepositoryMock.Setup(repo => repo.SendRetentionAlertWorkCaseMailAsync("1", It.IsAny<RetentionAlertWorkCaseSendMail>())).ReturnsAsync(response);
                retentionAlertService = new RetentionAlertService(adapterRegistry, referenceDataRepository, studentRepository, configurationRepository, currentUserFactory, roleRepo, logger, retentionAlertRepository);

                var retentionAlertWorkCaseActionResponse = await retentionAlertService.ManageRetentionAlertCaseRemindersAsync("1", dto);

            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task ManageRetentionAlertCaseRemindersAsync_returns_Failure2()
            {
                // Mock Adapters
                var workCaseDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCaseManageReminders, Ellucian.Colleague.Domain.Base.Entities.RetentionAlertWorkCaseManageReminders>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCaseManageReminders, Ellucian.Colleague.Domain.Base.Entities.RetentionAlertWorkCaseManageReminders>()).Returns(workCaseDtoAdapter);

                Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCaseManageReminders dto = new Dtos.Student.RetentionAlertWorkCaseManageReminders()
                {
                    UpdatedBy = "1234567",
                    Reminders = new List<Dtos.Student.RetentionAlertWorkCaseManageReminder>()
                    {
                        new Dtos.Student.RetentionAlertWorkCaseManageReminder()
                        {
                            CaseItemsId = "1",
                            ClearReminderDate = "Y"
                        },
                        new Dtos.Student.RetentionAlertWorkCaseManageReminder()
                        {
                            CaseItemsId = "2",
                            ClearReminderDate = "N"
                        }
                    }
                };

                var response = new RetentionAlertWorkCaseActionResponse()
                {
                    CaseId = "1",
                    HasError = false,
                    ErrorMessages = new List<string>()
                };

                retentionAlertRepositoryMock.Setup(repo => repo.ManageRetentionAlertCaseRemindersAsync("1", It.IsAny<RetentionAlertWorkCaseManageReminders>())).ReturnsAsync(response);
                retentionAlertService = new RetentionAlertService(adapterRegistry, referenceDataRepository, studentRepository, configurationRepository, currentUserFactory, roleRepo, logger, retentionAlertRepository);

                var retentionAlertWorkCaseActionResponse = await retentionAlertService.ManageRetentionAlertCaseRemindersAsync("1", dto);

            }

            [TestMethod]
            public async Task ManageRetentionAlertCaseRemindersAsync_returns_Success()
            {
                // Setup
                advisorAnyAdviseeRole.AddPermission(new Permission(RetentionAlertPermissionCodes.WorkAnyCase));
                advisorAnyAdviseeRole.AddPermission(new Permission(RetentionAlertPermissionCodes.WorkCases));
                advisorAnyAdviseeRole.AddPermission(new Permission(RetentionAlertPermissionCodes.ContributeToCases));
                roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role>() { advisorAnyAdviseeRole });
                roleRepoMock.Setup(rr => rr.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { advisorAnyAdviseeRole });

                // Mock Adapters
                var manageRemindersAdapter = new AutoMapperAdapter<Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCaseManageReminders, Ellucian.Colleague.Domain.Base.Entities.RetentionAlertWorkCaseManageReminders>(adapterRegistry, logger);
                var manageReminderAdapter = new AutoMapperAdapter<Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCaseManageReminder, Ellucian.Colleague.Domain.Base.Entities.RetentionAlertWorkCaseManageReminder>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCaseManageReminders, Ellucian.Colleague.Domain.Base.Entities.RetentionAlertWorkCaseManageReminders>()).Returns(manageRemindersAdapter);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCaseManageReminder, Ellucian.Colleague.Domain.Base.Entities.RetentionAlertWorkCaseManageReminder >()).Returns(manageReminderAdapter);

                Dtos.Student.RetentionAlertWorkCaseManageReminders dto = new Dtos.Student.RetentionAlertWorkCaseManageReminders()
                {
                    Reminders = new List<Dtos.Student.RetentionAlertWorkCaseManageReminder>()
                    {
                        new Dtos.Student.RetentionAlertWorkCaseManageReminder()
                        {
                            CaseItemsId = "1",
                            ClearReminderDate = "Y"
                        },
                        new Dtos.Student.RetentionAlertWorkCaseManageReminder()
                        {
                            CaseItemsId = "2",
                            ClearReminderDate = "N"
                        }
                    }
                };

                var response = new RetentionAlertWorkCaseActionResponse()
                {
                    CaseId = "1",
                    HasError = false,
                    ErrorMessages = new List<string>()
                };

                retentionAlertRepositoryMock.Setup(repo => repo.ManageRetentionAlertCaseRemindersAsync(It.IsAny<string>(), It.IsAny<RetentionAlertWorkCaseManageReminders>())).ReturnsAsync(response);
                retentionAlertService = new RetentionAlertService(adapterRegistry, referenceDataRepository, studentRepository, configurationRepository, currentUserFactory, roleRepo, logger, retentionAlertRepository);

                var retentionAlertWorkCaseActionResponse = await retentionAlertService.ManageRetentionAlertCaseRemindersAsync("1", dto);

            }

            [TestMethod]
            public async Task GetRetentionAlertCaseCategoryOrgRoles_returns_Success_1()
            {
                var caseCategoryOrgRoleAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.RetentionAlertCaseCategoryOrgRole, Ellucian.Colleague.Dtos.Student.RetentionAlertCaseCategoryOrgRole>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.RetentionAlertCaseCategoryOrgRole, Ellucian.Colleague.Dtos.Student.RetentionAlertCaseCategoryOrgRole>()).Returns(caseCategoryOrgRoleAdapter);

                var caseCategoryOrgRolesAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.RetentionAlertCaseCategoryOrgRoles, Ellucian.Colleague.Dtos.Student.RetentionAlertCaseCategoryOrgRoles>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.RetentionAlertCaseCategoryOrgRoles, Ellucian.Colleague.Dtos.Student.RetentionAlertCaseCategoryOrgRoles>()).Returns(caseCategoryOrgRolesAdapter);

                List<string> caseCategoryIds = new List<string>() { "2" };

                var caseCategoryOrgRoles = new List<RetentionAlertCaseCategoryOrgRole>();
                caseCategoryOrgRoles.Add(new RetentionAlertCaseCategoryOrgRole()
                {
                    OrgRoleId = "1",
                    OrgRoleName = "FACULTY",
                    IsAssignedInitially = "N",
                    IsAvailableForReassignment = "Y",
                    IsReportingAndAdministrative = "N"
                });

                caseCategoryOrgRoles.Add(new RetentionAlertCaseCategoryOrgRole()
                {
                    OrgRoleId = "2",
                    OrgRoleName = "ADVISOR",
                    IsAssignedInitially = "Y",
                    IsAvailableForReassignment = "Y",
                    IsReportingAndAdministrative = "Y"
                });

                var response = new List<RetentionAlertCaseCategoryOrgRoles>();
                response.Add(new RetentionAlertCaseCategoryOrgRoles()
                {
                    CaseCategoryId = "2",
                    CaseCategoryOrgRoles = caseCategoryOrgRoles
                });

                retentionAlertRepositoryMock.Setup(r => r.GetRetentionAlertCaseCategoryOrgRolesAsync(caseCategoryIds)).ReturnsAsync(response);
                retentionAlertService = new RetentionAlertService(adapterRegistry, referenceDataRepository, studentRepository, configurationRepository, currentUserFactory, roleRepo, logger, retentionAlertRepository);

                var retentionAlertCaseCategoryOrgRolesResponse = await retentionAlertService.GetRetentionAlertCaseCategoryOrgRolesAsync(caseCategoryIds);

                Assert.IsNotNull(retentionAlertCaseCategoryOrgRolesResponse);
                
                for (var i = 0; i < retentionAlertCaseCategoryOrgRolesResponse.Count(); i++)
                {
                    Assert.AreEqual(response[i].CaseCategoryId, retentionAlertCaseCategoryOrgRolesResponse.ToList()[i].CaseCategoryId);

                    for (var j = 0; j < retentionAlertCaseCategoryOrgRolesResponse.ToList()[i].CaseCategoryOrgRoles.Count(); j++)
                    {
                        Assert.AreEqual(response[i].CaseCategoryOrgRoles[j].OrgRoleId, retentionAlertCaseCategoryOrgRolesResponse.ToList()[i].CaseCategoryOrgRoles[j].OrgRoleId);
                        Assert.AreEqual(response[i].CaseCategoryOrgRoles[j].OrgRoleName, retentionAlertCaseCategoryOrgRolesResponse.ToList()[i].CaseCategoryOrgRoles[j].OrgRoleName);
                        Assert.AreEqual(response[i].CaseCategoryOrgRoles[j].IsAssignedInitially, retentionAlertCaseCategoryOrgRolesResponse.ToList()[i].CaseCategoryOrgRoles[j].IsAssignedInitially);
                        Assert.AreEqual(response[i].CaseCategoryOrgRoles[j].IsAvailableForReassignment, retentionAlertCaseCategoryOrgRolesResponse.ToList()[i].CaseCategoryOrgRoles[j].IsAvailableForReassignment);
                        Assert.AreEqual(response[i].CaseCategoryOrgRoles[j].IsReportingAndAdministrative, retentionAlertCaseCategoryOrgRolesResponse.ToList()[i].CaseCategoryOrgRoles[j].IsReportingAndAdministrative);
                    }
                }
            }

            [TestMethod]
            public async Task GetRetentionAlertCaseCategoryOrgRoles_returns_Success_2()
            {
                var caseCategoryOrgRoleAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.RetentionAlertCaseCategoryOrgRole, Ellucian.Colleague.Dtos.Student.RetentionAlertCaseCategoryOrgRole>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.RetentionAlertCaseCategoryOrgRole, Ellucian.Colleague.Dtos.Student.RetentionAlertCaseCategoryOrgRole>()).Returns(caseCategoryOrgRoleAdapter);

                var caseCategoryOrgRolesAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.RetentionAlertCaseCategoryOrgRoles, Ellucian.Colleague.Dtos.Student.RetentionAlertCaseCategoryOrgRoles>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.RetentionAlertCaseCategoryOrgRoles, Ellucian.Colleague.Dtos.Student.RetentionAlertCaseCategoryOrgRoles>()).Returns(caseCategoryOrgRolesAdapter);

                List<string> caseCategoryIds = new List<string>() { "2", "3" };

                var caseCategoryOrgRoles = new List<RetentionAlertCaseCategoryOrgRole>();
                caseCategoryOrgRoles.Add(new RetentionAlertCaseCategoryOrgRole()
                {
                    OrgRoleId = "1",
                    OrgRoleName = "FACULTY",
                    IsAssignedInitially = "N",
                    IsAvailableForReassignment = "Y",
                    IsReportingAndAdministrative = "N"
                });

                caseCategoryOrgRoles.Add(new RetentionAlertCaseCategoryOrgRole()
                {
                    OrgRoleId = "2",
                    OrgRoleName = "ADVISOR",
                    IsAssignedInitially = "Y",
                    IsAvailableForReassignment = "Y",
                    IsReportingAndAdministrative = "Y"
                });

                var response = new List<RetentionAlertCaseCategoryOrgRoles>();
                response.Add(new RetentionAlertCaseCategoryOrgRoles()
                {
                    CaseCategoryId = "2",
                    CaseCategoryOrgRoles = caseCategoryOrgRoles
                });

                response.Add(new RetentionAlertCaseCategoryOrgRoles()
                {
                    CaseCategoryId = "3",
                    CaseCategoryOrgRoles = caseCategoryOrgRoles
                });


                retentionAlertRepositoryMock.Setup(r => r.GetRetentionAlertCaseCategoryOrgRolesAsync(caseCategoryIds)).ReturnsAsync(response);
                retentionAlertService = new RetentionAlertService(adapterRegistry, referenceDataRepository, studentRepository, configurationRepository, currentUserFactory, roleRepo, logger, retentionAlertRepository);

                var retentionAlertCaseCategoryOrgRolesResponse = await retentionAlertService.GetRetentionAlertCaseCategoryOrgRolesAsync(caseCategoryIds);

                Assert.IsNotNull(retentionAlertCaseCategoryOrgRolesResponse);

                for (var i = 0; i < retentionAlertCaseCategoryOrgRolesResponse.Count(); i++)
                {
                    Assert.AreEqual(response[i].CaseCategoryId, retentionAlertCaseCategoryOrgRolesResponse.ToList()[i].CaseCategoryId);

                    for (var j = 0; j < retentionAlertCaseCategoryOrgRolesResponse.ToList()[i].CaseCategoryOrgRoles.Count(); j++)
                    {
                        Assert.AreEqual(response[i].CaseCategoryOrgRoles[j].OrgRoleId, retentionAlertCaseCategoryOrgRolesResponse.ToList()[i].CaseCategoryOrgRoles[j].OrgRoleId);
                        Assert.AreEqual(response[i].CaseCategoryOrgRoles[j].OrgRoleName, retentionAlertCaseCategoryOrgRolesResponse.ToList()[i].CaseCategoryOrgRoles[j].OrgRoleName);
                        Assert.AreEqual(response[i].CaseCategoryOrgRoles[j].IsAssignedInitially, retentionAlertCaseCategoryOrgRolesResponse.ToList()[i].CaseCategoryOrgRoles[j].IsAssignedInitially);
                        Assert.AreEqual(response[i].CaseCategoryOrgRoles[j].IsAvailableForReassignment, retentionAlertCaseCategoryOrgRolesResponse.ToList()[i].CaseCategoryOrgRoles[j].IsAvailableForReassignment);
                        Assert.AreEqual(response[i].CaseCategoryOrgRoles[j].IsReportingAndAdministrative, retentionAlertCaseCategoryOrgRolesResponse.ToList()[i].CaseCategoryOrgRoles[j].IsReportingAndAdministrative);
                    }
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetRetentionAlertCaseCategoryOrgRoles_returns_Failure_1()
            {
                var caseCategoryOrgRoleAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.RetentionAlertCaseCategoryOrgRole, Ellucian.Colleague.Dtos.Student.RetentionAlertCaseCategoryOrgRole>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.RetentionAlertCaseCategoryOrgRole, Ellucian.Colleague.Dtos.Student.RetentionAlertCaseCategoryOrgRole>()).Returns(caseCategoryOrgRoleAdapter);

                var caseCategoryOrgRolesAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.RetentionAlertCaseCategoryOrgRoles, Ellucian.Colleague.Dtos.Student.RetentionAlertCaseCategoryOrgRoles>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.RetentionAlertCaseCategoryOrgRoles, Ellucian.Colleague.Dtos.Student.RetentionAlertCaseCategoryOrgRoles>()).Returns(caseCategoryOrgRolesAdapter);

                List<string> caseCategoryIds = new List<string>();

                var caseCategoryOrgRoles = new List<RetentionAlertCaseCategoryOrgRole>();
                caseCategoryOrgRoles.Add(new RetentionAlertCaseCategoryOrgRole()
                {
                    OrgRoleId = "1",
                    OrgRoleName = "FACULTY",
                    IsAssignedInitially = "N",
                    IsAvailableForReassignment = "Y",
                    IsReportingAndAdministrative = "N"
                });

                caseCategoryOrgRoles.Add(new RetentionAlertCaseCategoryOrgRole()
                {
                    OrgRoleId = "2",
                    OrgRoleName = "ADVISOR",
                    IsAssignedInitially = "Y",
                    IsAvailableForReassignment = "Y",
                    IsReportingAndAdministrative = "Y"
                });

                var response = new List<RetentionAlertCaseCategoryOrgRoles>();
                response.Add(new RetentionAlertCaseCategoryOrgRoles()
                {
                    CaseCategoryId = "2",
                    CaseCategoryOrgRoles = caseCategoryOrgRoles
                });

                response.Add(new RetentionAlertCaseCategoryOrgRoles()
                {
                    CaseCategoryId = "3",
                    CaseCategoryOrgRoles = caseCategoryOrgRoles
                });

                retentionAlertRepositoryMock.Setup(r => r.GetRetentionAlertCaseCategoryOrgRolesAsync(caseCategoryIds)).ReturnsAsync(response);
                retentionAlertService = new RetentionAlertService(adapterRegistry, referenceDataRepository, studentRepository, configurationRepository, currentUserFactory, roleRepo, logger, retentionAlertRepository);

                var retentionAlertCaseCategoryOrgRolesResponse = await retentionAlertService.GetRetentionAlertCaseCategoryOrgRolesAsync(caseCategoryIds);                
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetRetentionAlertCaseCategoryOrgRoles_returns_Failure_2()
            {
                var caseCategoryOrgRoleAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.RetentionAlertCaseCategoryOrgRole, Ellucian.Colleague.Dtos.Student.RetentionAlertCaseCategoryOrgRole>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.RetentionAlertCaseCategoryOrgRole, Ellucian.Colleague.Dtos.Student.RetentionAlertCaseCategoryOrgRole>()).Returns(caseCategoryOrgRoleAdapter);

                var caseCategoryOrgRolesAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.RetentionAlertCaseCategoryOrgRoles, Ellucian.Colleague.Dtos.Student.RetentionAlertCaseCategoryOrgRoles>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.RetentionAlertCaseCategoryOrgRoles, Ellucian.Colleague.Dtos.Student.RetentionAlertCaseCategoryOrgRoles>()).Returns(caseCategoryOrgRolesAdapter);

                List<string> caseCategoryIds = null;

                var caseCategoryOrgRoles = new List<RetentionAlertCaseCategoryOrgRole>();
                caseCategoryOrgRoles.Add(new RetentionAlertCaseCategoryOrgRole()
                {
                    OrgRoleId = "1",
                    OrgRoleName = "FACULTY",
                    IsAssignedInitially = "N",
                    IsAvailableForReassignment = "Y",
                    IsReportingAndAdministrative = "N"
                });

                caseCategoryOrgRoles.Add(new RetentionAlertCaseCategoryOrgRole()
                {
                    OrgRoleId = "2",
                    OrgRoleName = "ADVISOR",
                    IsAssignedInitially = "Y",
                    IsAvailableForReassignment = "Y",
                    IsReportingAndAdministrative = "Y"
                });

                var response = new List<RetentionAlertCaseCategoryOrgRoles>();
                response.Add(new RetentionAlertCaseCategoryOrgRoles()
                {
                    CaseCategoryId = "2",
                    CaseCategoryOrgRoles = caseCategoryOrgRoles
                });

                response.Add(new RetentionAlertCaseCategoryOrgRoles()
                {
                    CaseCategoryId = "3",
                    CaseCategoryOrgRoles = caseCategoryOrgRoles
                });

                retentionAlertRepositoryMock.Setup(r => r.GetRetentionAlertCaseCategoryOrgRolesAsync(caseCategoryIds)).ReturnsAsync(response);
                retentionAlertService = new RetentionAlertService(adapterRegistry, referenceDataRepository, studentRepository, configurationRepository, currentUserFactory, roleRepo, logger, retentionAlertRepository);

                var retentionAlertCaseCategoryOrgRolesResponse = await retentionAlertService.GetRetentionAlertCaseCategoryOrgRolesAsync(caseCategoryIds);
            }

        }

        [TestClass]
        public class GetRetentionAlertGroupOfCasesSummaryAsync : RetentionAlertServiceTests
        {
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            private IReferenceDataRepository referenceDataRepository;
            private IRetentionAlertService retentionAlertService;
            private Mock<IStudentRepository> studentRepositoryMock;
            private IStudentRepository studentRepository;
            private Mock<IConfigurationRepository> configurationRepositoryMock;
            private IConfigurationRepository configurationRepository;
            private ICurrentUserFactory currentUserFactory;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;

            private Mock<IRetentionAlertRepository> retentionAlertRepositoryMock;
            private IRetentionAlertRepository retentionAlertRepository;

            private RetentionAlertGroupOfCasesSummary groupOfCaseSummary;

            [TestInitialize]
            public void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                logger = new Mock<ILogger>().Object;

                studentRepositoryMock = new Mock<IStudentRepository>();
                studentRepository = studentRepositoryMock.Object;
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                configurationRepository = configurationRepositoryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;

                retentionAlertRepositoryMock = new Mock<IRetentionAlertRepository>();
                retentionAlertRepository = retentionAlertRepositoryMock.Object;

                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;


                var responseDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.RetentionAlertWorkCaseActionResponse, Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCaseActionResponse>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.RetentionAlertWorkCaseActionResponse, Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCaseActionResponse>()).Returns(responseDtoAdapter);

                groupOfCaseSummary = new RetentionAlertGroupOfCasesSummary()
                {
                    Summary = "Advising Alert"
                };

                groupOfCaseSummary.AddEntityCase(new RetentionAlertGroupOfCases()
                {
                    Name = "John Doe",
                    CaseIds = new List<string>() { "1", "2", "3" }
                });

                groupOfCaseSummary.AddRoleCase(new RetentionAlertGroupOfCases()
                {
                    Name = "ADVISOR",
                    CaseIds = new List<string> { "4", "5", "6" }
                });

                // Set up current user
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();

                // Setup
                advisorAnyAdviseeRole.AddPermission(new Permission(PlanningPermissionCodes.AllAccessAnyAdvisee));
                advisorAnyAdviseeRole.AddPermission(new Permission(PlanningPermissionCodes.ReviewAnyAdvisee));
                advisorAnyAdviseeRole.AddPermission(new Permission(PlanningPermissionCodes.UpdateAnyAdvisee));
                advisorAnyAdviseeRole.AddPermission(new Permission(PlanningPermissionCodes.UpdateAssignedAdvisees));

                roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role>() { advisorAnyAdviseeRole });
                roleRepoMock.Setup(rr => rr.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { advisorAnyAdviseeRole });
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistry = null;
                roleRepo = null;
                retentionAlertService = null;
                referenceDataRepository = null;
                studentRepository = null;
                configurationRepository = null;
                retentionAlertRepository = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetRetentionAlertCaseCategoryOrgRolesAsync_returns_Failure_NULLARGUMENT()
            {
                var groupOfCasesSummaryAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.RetentionAlertGroupOfCasesSummary, Ellucian.Colleague.Dtos.Student.RetentionAlertGroupOfCasesSummary>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.RetentionAlertGroupOfCasesSummary, Ellucian.Colleague.Dtos.Student.RetentionAlertGroupOfCasesSummary>()).Returns(groupOfCasesSummaryAdapter);

                var caseCategoryOrgRolesAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.RetentionAlertGroupOfCases, Ellucian.Colleague.Dtos.Student.RetentionAlertGroupOfCases>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.RetentionAlertGroupOfCases, Ellucian.Colleague.Dtos.Student.RetentionAlertGroupOfCases>()).Returns(caseCategoryOrgRolesAdapter);


                retentionAlertRepositoryMock.Setup(r => r.GetRetentionAlertCaseOwnerSummaryAsync("")).ReturnsAsync(groupOfCaseSummary);
                retentionAlertService = new RetentionAlertService(adapterRegistry, referenceDataRepository, studentRepository, configurationRepository, currentUserFactory, roleRepo, logger, retentionAlertRepository);

                var response = await retentionAlertService.GetRetentionAlertCaseOwnerSummaryAsync(null);

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetRetentionAlertCaseCategoryOrgRolesAsync_returns_Failure_EMPTYARGUMENT()
            {
                var groupOfCasesSummaryAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.RetentionAlertGroupOfCasesSummary, Ellucian.Colleague.Dtos.Student.RetentionAlertGroupOfCasesSummary>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.RetentionAlertGroupOfCasesSummary, Ellucian.Colleague.Dtos.Student.RetentionAlertGroupOfCasesSummary>()).Returns(groupOfCasesSummaryAdapter);

                var caseCategoryOrgRolesAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.RetentionAlertGroupOfCases, Ellucian.Colleague.Dtos.Student.RetentionAlertGroupOfCases>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.RetentionAlertGroupOfCases, Ellucian.Colleague.Dtos.Student.RetentionAlertGroupOfCases>()).Returns(caseCategoryOrgRolesAdapter);


                retentionAlertRepositoryMock.Setup(r => r.GetRetentionAlertCaseOwnerSummaryAsync("")).ReturnsAsync(groupOfCaseSummary);
                retentionAlertService = new RetentionAlertService(adapterRegistry, referenceDataRepository, studentRepository, configurationRepository, currentUserFactory, roleRepo, logger, retentionAlertRepository);

                var response = await retentionAlertService.GetRetentionAlertCaseOwnerSummaryAsync("");
            }

            [TestMethod]
            public async Task GetRetentionAlertCaseCategoryOrgRolesAsync_returns_Success_1()
            {
                // Setup
                advisorAnyAdviseeRole.AddPermission(new Permission(RetentionAlertPermissionCodes.WorkAnyCase));
                advisorAnyAdviseeRole.AddPermission(new Permission(RetentionAlertPermissionCodes.WorkCases));
                advisorAnyAdviseeRole.AddPermission(new Permission(RetentionAlertPermissionCodes.ContributeToCases));
                roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role>() { advisorAnyAdviseeRole });
                roleRepoMock.Setup(rr => rr.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { advisorAnyAdviseeRole });

                var groupOfCasesSummaryAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.RetentionAlertGroupOfCasesSummary, Ellucian.Colleague.Dtos.Student.RetentionAlertGroupOfCasesSummary>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.RetentionAlertGroupOfCasesSummary, Ellucian.Colleague.Dtos.Student.RetentionAlertGroupOfCasesSummary>()).Returns(groupOfCasesSummaryAdapter);

                var caseCategoryOrgRolesAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.RetentionAlertGroupOfCases, Ellucian.Colleague.Dtos.Student.RetentionAlertGroupOfCases>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.RetentionAlertGroupOfCases, Ellucian.Colleague.Dtos.Student.RetentionAlertGroupOfCases>()).Returns(caseCategoryOrgRolesAdapter);

                retentionAlertRepositoryMock.Setup(r => r.GetRetentionAlertCaseOwnerSummaryAsync("1")).ReturnsAsync(groupOfCaseSummary);
                retentionAlertService = new RetentionAlertService(adapterRegistry, referenceDataRepository, studentRepository, configurationRepository, currentUserFactory, roleRepo, logger, retentionAlertRepository);

                var response = await retentionAlertService.GetRetentionAlertCaseOwnerSummaryAsync("1");

                Assert.IsNotNull(response);
                Assert.AreEqual(groupOfCaseSummary.Summary, response.Summary);

                for (var i = 0; i < groupOfCaseSummary.EntityCases.Count; i++)
                {
                    Assert.AreEqual(groupOfCaseSummary.EntityCases[i].Name, response.EntityCases[i].Name);
                    Assert.AreEqual(groupOfCaseSummary.EntityCases[i].NumberOfCases, response.EntityCases[i].NumberOfCases);

                    for (var j = 0; j < groupOfCaseSummary.EntityCases[i].CaseIds.Count; j++)
                    {
                        Assert.AreEqual(groupOfCaseSummary.EntityCases[i].CaseIds[j], response.EntityCases[i].CaseIds[j]);
                    }
                }

                for (var i = 0; i < groupOfCaseSummary.RoleCases.Count; i++)
                {
                    Assert.AreEqual(groupOfCaseSummary.RoleCases[i].Name, response.RoleCases[i].Name);
                    Assert.AreEqual(groupOfCaseSummary.RoleCases[i].NumberOfCases, response.RoleCases[i].NumberOfCases);

                    for (var j = 0; j < groupOfCaseSummary.RoleCases[i].CaseIds.Count; j++)
                    {
                        Assert.AreEqual(groupOfCaseSummary.RoleCases[i].CaseIds[j], response.RoleCases[i].CaseIds[j]);
                    }
                }

            }
        }

        [TestClass]
        public class SetRetentionAlertEmailPreferenceAsync : RetentionAlertServiceTests
        {
            #region Variables
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            private IReferenceDataRepository referenceDataRepository;
            private IRetentionAlertService retentionAlertService;
            private Mock<IStudentRepository> studentRepositoryMock;
            private IStudentRepository studentRepository;
            private Mock<IConfigurationRepository> configurationRepositoryMock;
            private IConfigurationRepository configurationRepository;
            private ICurrentUserFactory currentUserFactory;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;

            private Mock<IRetentionAlertRepository> retentionAlertRepositoryMock;
            private IRetentionAlertRepository retentionAlertRepository;
            #endregion

            [TestInitialize]
            public void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                logger = new Mock<ILogger>().Object;

                studentRepositoryMock = new Mock<IStudentRepository>();
                studentRepository = studentRepositoryMock.Object;
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                configurationRepository = configurationRepositoryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;

                retentionAlertRepositoryMock = new Mock<IRetentionAlertRepository>();
                retentionAlertRepository = retentionAlertRepositoryMock.Object;

                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;


                var responseDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.RetentionAlertWorkCaseActionResponse, Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCaseActionResponse>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.RetentionAlertWorkCaseActionResponse, Ellucian.Colleague.Dtos.Student.RetentionAlertWorkCaseActionResponse>()).Returns(responseDtoAdapter);

                 // Set up current user
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();

                // Setup
                advisorAnyAdviseeRole.AddPermission(new Permission(PlanningPermissionCodes.AllAccessAnyAdvisee));
                advisorAnyAdviseeRole.AddPermission(new Permission(PlanningPermissionCodes.ReviewAnyAdvisee));
                advisorAnyAdviseeRole.AddPermission(new Permission(PlanningPermissionCodes.UpdateAnyAdvisee));
                advisorAnyAdviseeRole.AddPermission(new Permission(PlanningPermissionCodes.UpdateAssignedAdvisees));

                roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role>() { advisorAnyAdviseeRole });
                roleRepoMock.Setup(rr => rr.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { advisorAnyAdviseeRole });
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistry = null;
                roleRepo = null;
                retentionAlertService = null;
                referenceDataRepository = null;
                studentRepository = null;
                configurationRepository = null;
                retentionAlertRepository = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SetRetentionAlertEmailPreferenceAsync_returns_Failure_NULLARGUMENT()
            {
                RetentionAlertSendEmailPreference preference = new RetentionAlertSendEmailPreference()
                {
                    HasSendEmailFlag = true,
                    Message = "Your current setting is to receive e-mail reminders when cases are assigned to you."
                };

                var adapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.RetentionAlertSendEmailPreference, Ellucian.Colleague.Dtos.Student.RetentionAlertSendEmailPreference>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.RetentionAlertSendEmailPreference, Ellucian.Colleague.Dtos.Student.RetentionAlertSendEmailPreference>()).Returns(adapter);

                var caseCategoryOrgRolesAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.RetentionAlertGroupOfCases, Ellucian.Colleague.Dtos.Student.RetentionAlertGroupOfCases>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.RetentionAlertGroupOfCases, Ellucian.Colleague.Dtos.Student.RetentionAlertGroupOfCases>()).Returns(caseCategoryOrgRolesAdapter);
                
                retentionAlertRepositoryMock.Setup(r => r.SetRetentionAlertEmailPreferenceAsync(It.IsAny<string>(), It.IsAny<RetentionAlertSendEmailPreference>())).ReturnsAsync(preference);
                retentionAlertService = new RetentionAlertService(adapterRegistry, referenceDataRepository, studentRepository, configurationRepository, currentUserFactory, roleRepo, logger, retentionAlertRepository);

                var response = await retentionAlertService.SetRetentionAlertEmailPreferenceAsync(null, null);

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SetRetentionAlertEmailPreferenceAsync_returns_Failure_NULLARGUMENT_1()
            {
                RetentionAlertSendEmailPreference preference = new RetentionAlertSendEmailPreference()
                {
                    HasSendEmailFlag = true,
                    Message = "Your current setting is to receive e-mail reminders when cases are assigned to you."
                };

                var adapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.RetentionAlertSendEmailPreference, Ellucian.Colleague.Dtos.Student.RetentionAlertSendEmailPreference>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.RetentionAlertSendEmailPreference, Ellucian.Colleague.Dtos.Student.RetentionAlertSendEmailPreference>()).Returns(adapter);

                var caseCategoryOrgRolesAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.RetentionAlertGroupOfCases, Ellucian.Colleague.Dtos.Student.RetentionAlertGroupOfCases>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.RetentionAlertGroupOfCases, Ellucian.Colleague.Dtos.Student.RetentionAlertGroupOfCases>()).Returns(caseCategoryOrgRolesAdapter);

                retentionAlertRepositoryMock.Setup(r => r.SetRetentionAlertEmailPreferenceAsync(It.IsAny<string>(), It.IsAny<RetentionAlertSendEmailPreference>())).ReturnsAsync(preference);
                retentionAlertService = new RetentionAlertService(adapterRegistry, referenceDataRepository, studentRepository, configurationRepository, currentUserFactory, roleRepo, logger, retentionAlertRepository);

                var response = await retentionAlertService.SetRetentionAlertEmailPreferenceAsync("", null);

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SetRetentionAlertEmailPreferenceAsync_returns_Failure_NULLARGUMENT_2()
            {
                RetentionAlertSendEmailPreference preference = new RetentionAlertSendEmailPreference()
                {
                    HasSendEmailFlag = true,
                    Message = "Your current setting is to receive e-mail reminders when cases are assigned to you."
                };

                var adapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.RetentionAlertSendEmailPreference, Ellucian.Colleague.Dtos.Student.RetentionAlertSendEmailPreference>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.RetentionAlertSendEmailPreference, Ellucian.Colleague.Dtos.Student.RetentionAlertSendEmailPreference>()).Returns(adapter);

                var caseCategoryOrgRolesAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.RetentionAlertGroupOfCases, Ellucian.Colleague.Dtos.Student.RetentionAlertGroupOfCases>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.RetentionAlertGroupOfCases, Ellucian.Colleague.Dtos.Student.RetentionAlertGroupOfCases>()).Returns(caseCategoryOrgRolesAdapter);

                retentionAlertRepositoryMock.Setup(r => r.SetRetentionAlertEmailPreferenceAsync(It.IsAny<string>(), It.IsAny<RetentionAlertSendEmailPreference>())).ReturnsAsync(preference);
                retentionAlertService = new RetentionAlertService(adapterRegistry, referenceDataRepository, studentRepository, configurationRepository, currentUserFactory, roleRepo, logger, retentionAlertRepository);

                var response = await retentionAlertService.SetRetentionAlertEmailPreferenceAsync("1", null);

            }

            [TestMethod]
            public async Task SetRetentionAlertEmailPreferenceAsync_returns_Success()
            {
                // Setup
                advisorAnyAdviseeRole.AddPermission(new Permission(RetentionAlertPermissionCodes.WorkAnyCase));
                advisorAnyAdviseeRole.AddPermission(new Permission(RetentionAlertPermissionCodes.WorkCases));
                advisorAnyAdviseeRole.AddPermission(new Permission(RetentionAlertPermissionCodes.ContributeToCases));
                roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role>() { advisorAnyAdviseeRole });
                roleRepoMock.Setup(rr => rr.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { advisorAnyAdviseeRole });

                RetentionAlertSendEmailPreference preferenceEntity = new RetentionAlertSendEmailPreference()
                {
                    HasSendEmailFlag = true,
                    Message = "Your current setting is to receive e-mail reminders when cases are assigned to you."
                };

                Dtos.Student.RetentionAlertSendEmailPreference preferenceDto = new Dtos.Student.RetentionAlertSendEmailPreference()
                {
                    HasSendEmailFlag = true,
                    Message = "Your current setting is to receive e-mail reminders when cases are assigned to you."
                };

                var adapter = new AutoMapperAdapter<RetentionAlertSendEmailPreference, Dtos.Student.RetentionAlertSendEmailPreference>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<RetentionAlertSendEmailPreference, Dtos.Student.RetentionAlertSendEmailPreference>()).Returns(adapter);

                var adapterDtoToEntity = new AutoMapperAdapter<Dtos.Student.RetentionAlertSendEmailPreference, RetentionAlertSendEmailPreference>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Dtos.Student.RetentionAlertSendEmailPreference, RetentionAlertSendEmailPreference>()).Returns(adapterDtoToEntity);

                retentionAlertRepositoryMock.Setup(r => r.SetRetentionAlertEmailPreferenceAsync(It.IsAny<string>(), It.IsAny<RetentionAlertSendEmailPreference>())).ReturnsAsync(preferenceEntity);
                retentionAlertService = new RetentionAlertService(adapterRegistry, referenceDataRepository, studentRepository, configurationRepository, currentUserFactory, roleRepo, logger, retentionAlertRepository);

                var response = await retentionAlertService.SetRetentionAlertEmailPreferenceAsync("1", preferenceDto);

                Assert.AreEqual(preferenceDto.HasSendEmailFlag, response.HasSendEmailFlag);
                Assert.AreEqual(preferenceDto.Message, response.Message);
            }
        }
    }
}
