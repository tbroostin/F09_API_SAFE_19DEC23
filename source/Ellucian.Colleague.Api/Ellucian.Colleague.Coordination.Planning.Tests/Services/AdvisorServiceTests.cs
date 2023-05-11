// Copyright 2012-2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Planning.Services;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Planning.Entities;
using Ellucian.Colleague.Domain.Planning.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Dtos.Planning;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Coordination.Planning.Tests.Services
{
    [TestClass]
    public class AdvisorServiceTests
    {
        // Sets up a Current user that is an advisor
        public abstract class CurrentUserSetup
        {
            protected Role advisorRole = new Role(105, "Advisor");

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

            public class NonAdvisorUserFactory : ICurrentUserFactory
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
                            Roles = new List<string>() { "Student" },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }

            public class NullUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return null;
                    }
                }
            }
        }

        [TestClass]
        public class GetAdvisor : CurrentUserSetup
        {
            private Mock<IAdvisorRepository> advisorRepoMock;
            private IAdvisorRepository advisorRepo;
            private Mock<IDegreePlanRepository> degreePlanRepoMock;
            private IDegreePlanRepository degreePlanRepo;
            private Mock<IStudentDegreePlanRepository> studentDegreePlanRepoMock;
            private IStudentDegreePlanRepository studentDegreePlanRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IAdviseeRepository> adviseeRepoMock;
            private IAdviseeRepository adviseeRepo;
            private Mock<IStudentConfigurationRepository> configRepoMock;
            private IStudentConfigurationRepository configRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private AdvisorService advisorService;
            private ILogger logger;
            private Domain.Planning.Entities.Advisor advisor;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private StudentConfiguration config;
            private IStudentRepository studentRepo;
            private Mock<IStudentRepository> studentRepoMock;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;
            private IPersonBaseRepository personBaseRepo;
            private Mock<IPersonBaseRepository> personBaseRepoMock;

            [TestInitialize]
            public void Initialize()
            {
                advisorRepoMock = new Mock<IAdvisorRepository>();
                advisorRepo = advisorRepoMock.Object;
                degreePlanRepoMock = new Mock<IDegreePlanRepository>();
                degreePlanRepo = degreePlanRepoMock.Object;
                studentDegreePlanRepoMock = new Mock<IStudentDegreePlanRepository>();
                studentDegreePlanRepo = studentDegreePlanRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                adviseeRepoMock = new Mock<IAdviseeRepository>();
                adviseeRepo = adviseeRepoMock.Object;
                configRepoMock = new Mock<IStudentConfigurationRepository>();
                configRepo = configRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;
                personBaseRepoMock = new Mock<IPersonBaseRepository>();
                personBaseRepo = personBaseRepoMock.Object;

                // Mock advisor response from advisor Repository, including advisee ids.
                advisor = new Domain.Planning.Entities.Advisor("0000011", "Brown");
                advisor.AddEmailAddress(new EmailAddress("xyz@xmail.com", "FAC"));
                advisor.AddAdvisee("0000894");
                advisor.AddAdvisee("0004002");
                advisorRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>(), It.IsAny<AdviseeInclusionType>())).ReturnsAsync(advisor);

                // Mock config repo response
                config = new StudentConfiguration();
                config.FacultyEmailTypeCode = "FAC";
                configRepoMock.Setup(repo => repo.GetStudentConfigurationAsync()).ReturnsAsync(config);

                var advisorDtoAdapter = new AutoMapperAdapter<Domain.Planning.Entities.Advisor, Dtos.Planning.Advisor>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Planning.Entities.Advisor, Dtos.Planning.Advisor>()).Returns(advisorDtoAdapter);

                // Set up current user
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();

                advisorService = new AdvisorService(
                    adapterRegistry, advisorRepo, degreePlanRepo, termRepo, adviseeRepo, configRepo,
                    currentUserFactory, roleRepo, logger, studentRepo, null, baseConfigurationRepository, personBaseRepo,
                    studentDegreePlanRepo);
            }

            [TestCleanup]
            public void Cleanup()
            {
                degreePlanRepo = null;
                studentDegreePlanRepo = null;
                adapterRegistry = null;
                advisorRepo = null;
                termRepo = null;
                adviseeRepo = null;
                roleRepo = null;
                advisorService = null;
                advisorRole = null;
            }


            [TestMethod]
            public async Task ReturnsAdvisorDto()
            {

                // Act--get advisor
                var advisorDto = await advisorService.GetAdvisorAsync("0000011");
                // Assert
                Assert.IsTrue(advisorDto is Dtos.Planning.Advisor);
                Assert.AreEqual(advisor.Id, advisorDto.Id);
                Assert.AreEqual(advisor.LastName, advisorDto.LastName);
                Assert.IsFalse(advisor.IsActive);
            }

            [TestMethod]
            public async Task ReturnAdvisorDTO_Emails()
            {
                // Act--get advisor
                var advisorDto = await advisorService.GetAdvisorAsync("0000011");
                Assert.AreEqual(advisor.GetEmailAddresses("FAC").Count(), advisorDto.EmailAddresses.Count());
                var advisorDtoEmail = advisorDto.EmailAddresses.FirstOrDefault();
                var advisorEmails = advisor.GetEmailAddresses("FAC");
                var advisorEmailAddress = advisorEmails.ElementAt(0);
                Assert.AreEqual(advisorEmailAddress, advisorDtoEmail);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AdvisorNotFacultyOrStaff_ThrowsNotFoundException()
            {
                // Mock empty list returned from advisor Repository
                advisorRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>(), It.IsAny<AdviseeInclusionType>())).ThrowsAsync(new KeyNotFoundException());
                // Act--get advisor
                var advisorDto = await advisorService.GetAdvisorAsync("0000011");
            }

        }

        [TestClass]
        public class GetAdvisorsAsync : CurrentUserSetup
        {
            private Mock<IAdvisorRepository> advisorRepoMock;
            private IAdvisorRepository advisorRepo;
            private Mock<IDegreePlanRepository> degreePlanRepoMock;
            private IDegreePlanRepository degreePlanRepo;
            private Mock<IStudentDegreePlanRepository> studentDegreePlanRepoMock;
            private IStudentDegreePlanRepository studentDegreePlanRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IAdviseeRepository> adviseeRepoMock;
            private IAdviseeRepository adviseeRepo;
            private Mock<IStudentConfigurationRepository> configRepoMock;
            private IStudentConfigurationRepository configRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private AdvisorService advisorService;
            private ILogger logger;
            private Domain.Planning.Entities.Advisor advisor1;
            private Domain.Planning.Entities.Advisor advisor2;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private StudentConfiguration config;
            private IStudentRepository studentRepo;
            private Mock<IStudentRepository> studentRepoMock;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;
            private IPersonBaseRepository personBaseRepo;
            private Mock<IPersonBaseRepository> personBaseRepoMock;


            [TestInitialize]
            public void Initialize()
            {
                advisorRepoMock = new Mock<IAdvisorRepository>();
                advisorRepo = advisorRepoMock.Object;
                degreePlanRepoMock = new Mock<IDegreePlanRepository>();
                degreePlanRepo = degreePlanRepoMock.Object;
                studentDegreePlanRepoMock = new Mock<IStudentDegreePlanRepository>();
                studentDegreePlanRepo = studentDegreePlanRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                adviseeRepoMock = new Mock<IAdviseeRepository>();
                adviseeRepo = adviseeRepoMock.Object;
                configRepoMock = new Mock<IStudentConfigurationRepository>();
                configRepo = configRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;
                personBaseRepoMock = new Mock<IPersonBaseRepository>();
                personBaseRepo = personBaseRepoMock.Object;


                // Mock advisor response from advisor Repository, including advisee ids.
                advisor1 = new Domain.Planning.Entities.Advisor("0000011", "Brown");
                advisor1.FirstName = "James";
                advisor1.AddEmailAddress(new EmailAddress("xyz@xmail.com", "FAC"));
                advisor1.AddEmailAddress(new EmailAddress("preferred@xmail.com", "FAC"));
                advisor1.EmailAddresses.ElementAt(1).IsPreferred = true;
                advisor1.AddAdvisee("0000894");
                advisor1.AddAdvisee("0004002");

                advisor2 = new Domain.Planning.Entities.Advisor("0000012", "Smith");
                advisor2.FirstName = "Joan";
                advisor2.AddEmailAddress(new EmailAddress("abc@xmail.com", "NOT"));
                advisor2.AddEmailAddress(new EmailAddress("preferred2@xmail.com", "NOT"));
                advisor2.EmailAddresses.ElementAt(0).IsPreferred = true;

                advisorRepoMock.Setup(repo => repo.GetAdvisorsAsync(It.IsAny<List<string>>(), It.IsAny<AdviseeInclusionType>())).ReturnsAsync((new List<Domain.Planning.Entities.Advisor> { advisor1, advisor2 }).AsEnumerable());

                // Mock config repo response
                config = new StudentConfiguration();
                config.FacultyEmailTypeCode = "FAC";
                configRepoMock.Setup(repo => repo.GetStudentConfigurationAsync()).ReturnsAsync(config);

                var advisorDtoAdapter = new AutoMapperAdapter<Domain.Planning.Entities.Advisor, Dtos.Planning.Advisor>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Planning.Entities.Advisor, Dtos.Planning.Advisor>()).Returns(advisorDtoAdapter);

                // Set up current user
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();

                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                advisorService = new AdvisorService(
                    adapterRegistry, advisorRepo, degreePlanRepo, termRepo, adviseeRepo, configRepo, currentUserFactory,
                    roleRepo, logger, studentRepo, null, baseConfigurationRepository, personBaseRepo, studentDegreePlanRepo);
            }

            [TestCleanup]
            public void Cleanup()
            {
                degreePlanRepo = null;
                studentDegreePlanRepo = null;
                adapterRegistry = null;
                advisorRepo = null;
                termRepo = null;
                adviseeRepo = null;
                roleRepo = null;
                advisorService = null;
                advisorRole = null;
            }


            [TestMethod]
            public async Task ReturnsAdvisorDtoList()
            {
                // Act--get advisor
                var advisorDtos = await advisorService.GetAdvisorsAsync(new AdvisorQueryCriteria() { AdvisorIds = new List<string>() { "0000011", "0000012" } });
                // Assert
                Assert.IsTrue(advisorDtos is IEnumerable<Dtos.Planning.Advisor>);
                Assert.AreEqual(2, advisorDtos.Count());
                // First advisor in result
                var advisorDto = advisorDtos.ElementAt(0);
                Assert.AreEqual(advisor1.Id, advisorDto.Id);
                Assert.AreEqual(advisor1.LastName, advisorDto.LastName);
                Assert.AreEqual(2, advisorDto.EmailAddresses.Count());
                // Second advisor in result
                advisorDto = advisorDtos.ElementAt(1);
                Assert.AreEqual(advisor2.Id, advisorDto.Id);
                Assert.AreEqual(advisor2.LastName, advisorDto.LastName);
                Assert.AreEqual(0, advisorDto.EmailAddresses.Count());
            }

            [TestMethod]
            public async Task ReturnAdvisorDTO_FacultyTypeEmails()
            {
                // Act--get advisor
                var advisorDtos = await advisorService.GetAdvisorsAsync(new AdvisorQueryCriteria() { AdvisorIds = new List<string>() { "0000011", "0000012" } });
                Assert.AreEqual(advisor1.GetEmailAddresses("FAC").Count(), advisorDtos.First().EmailAddresses.Count());
                var advisorDtoEmails = advisorDtos.First().EmailAddresses;
                var advisorEmails = advisor1.GetEmailAddresses("FAC");
                for (int i = 0; i < advisorEmails.Count(); i++)
                {
                    Assert.AreEqual(advisorEmails.ElementAt(i), advisorDtoEmails.ElementAt(i));
                }
            }

            [TestMethod]
            public async Task EmptyResponse_ReturnsEmptyAdvisorDto()
            {
                advisorRepoMock.Setup(repo => repo.GetAdvisorsAsync(It.IsAny<List<string>>(), It.IsAny<AdviseeInclusionType>())).ReturnsAsync(new List<Domain.Planning.Entities.Advisor>());
                var advisorDtos = await advisorService.GetAdvisorsAsync(new AdvisorQueryCriteria() { AdvisorIds = new List<string>() { "0000098", "0000099" } });
                Assert.AreEqual(0, advisorDtos.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task RethrowsRepositoryException()
            {
                advisorRepoMock.Setup(repo => repo.GetAdvisorsAsync(It.IsAny<List<string>>(), It.IsAny<AdviseeInclusionType>())).ThrowsAsync(new Exception());
                var advisorDtos = await advisorService.GetAdvisorsAsync(new AdvisorQueryCriteria() { AdvisorIds = new List<string>() { "0000098", "0000099" } });
            }

        }

        [TestClass]
        public class QueryAdvisorsByPostAsync : CurrentUserSetup
        {
            private Mock<IAdvisorRepository> advisorRepoMock;
            private IAdvisorRepository advisorRepo;
            private Mock<IDegreePlanRepository> degreePlanRepoMock;
            private IDegreePlanRepository degreePlanRepo;
            private Mock<IStudentDegreePlanRepository> studentDegreePlanRepoMock;
            private IStudentDegreePlanRepository studentDegreePlanRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IAdviseeRepository> adviseeRepoMock;
            private IAdviseeRepository adviseeRepo;
            private Mock<IStudentConfigurationRepository> configRepoMock;
            private IStudentConfigurationRepository configRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private AdvisorService advisorService;
            private Mock<ILogger> loggerMock;
            private ILogger logger;
            private Domain.Planning.Entities.Advisor advisor1;
            private Domain.Planning.Entities.Advisor advisor2;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private StudentConfiguration config;
            private IStudentRepository studentRepo;
            private Mock<IStudentRepository> studentRepoMock;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;
            private IPersonBaseRepository personBaseRepo;
            private Mock<IPersonBaseRepository> personBaseRepoMock;

            [TestInitialize]
            public void Initialize()
            {
                advisorRepoMock = new Mock<IAdvisorRepository>();
                advisorRepo = advisorRepoMock.Object;
                degreePlanRepoMock = new Mock<IDegreePlanRepository>();
                degreePlanRepo = degreePlanRepoMock.Object;
                studentDegreePlanRepoMock = new Mock<IStudentDegreePlanRepository>();
                studentDegreePlanRepo = studentDegreePlanRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                adviseeRepoMock = new Mock<IAdviseeRepository>();
                adviseeRepo = adviseeRepoMock.Object;
                configRepoMock = new Mock<IStudentConfigurationRepository>();
                configRepo = configRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                loggerMock = new Mock<ILogger>();
                logger = loggerMock.Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;
                personBaseRepoMock = new Mock<IPersonBaseRepository>();
                personBaseRepo = personBaseRepoMock.Object;

                // Mock advisor response from advisor Repository, including advisee ids.
                advisor1 = new Domain.Planning.Entities.Advisor("0000011", "Brown");
                advisor1.FirstName = "James";
                advisor1.AddEmailAddress(new EmailAddress("xyz@xmail.com", "FAC"));
                advisor1.AddEmailAddress(new EmailAddress("preferred@xmail.com", "FAC"));
                advisor1.EmailAddresses.ElementAt(1).IsPreferred = true;
                advisor1.AddAdvisee("0000894");
                advisor1.AddAdvisee("0004002");

                advisor2 = new Domain.Planning.Entities.Advisor("0000012", "Smith");
                advisor2.FirstName = "Joan";
                advisor2.AddEmailAddress(new EmailAddress("abc@xmail.com", "NOT"));
                advisor2.AddEmailAddress(new EmailAddress("preferred2@xmail.com", "NOT"));
                advisor2.EmailAddresses.ElementAt(0).IsPreferred = true;

                advisorRepoMock.Setup(repo => repo.GetAdvisorsAsync(It.IsAny<List<string>>(), AdviseeInclusionType.NoAdvisees)).ReturnsAsync((new List<Domain.Planning.Entities.Advisor> { advisor1, advisor2 }).AsEnumerable());

                // Mock config repo response
                config = new StudentConfiguration();
                config.FacultyEmailTypeCode = "FAC";
                configRepoMock.Setup(repo => repo.GetStudentConfigurationAsync()).ReturnsAsync(config);

                // Set up current user
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();

                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                advisorService = new AdvisorService(
                    adapterRegistry, advisorRepo, degreePlanRepo, termRepo, adviseeRepo, configRepo, currentUserFactory,
                    roleRepo, logger, studentRepo, null, baseConfigurationRepository, personBaseRepo, studentDegreePlanRepo);
            }

            [TestCleanup]
            public void Cleanup()
            {
                degreePlanRepo = null;
                studentDegreePlanRepo = null;
                adapterRegistry = null;
                advisorRepo = null;
                termRepo = null;
                adviseeRepo = null;
                roleRepo = null;
                advisorService = null;
                advisorRole = null;
            }


            [TestMethod]
            public async Task QueryAdvisorsByPostAsync_returns_Advisor_DTO_list()
            {
                // Act--get advisor
                var advisorDtos = await advisorService.QueryAdvisorsByPostAsync(new List<string>() { "0000011", "0000012" });
                // Assert
                Assert.IsTrue(advisorDtos is IEnumerable<Dtos.Planning.Advisor>);
                Assert.AreEqual(2, advisorDtos.Count());
                // First advisor in result
                var advisorDto = advisorDtos.ElementAt(0);
                Assert.AreEqual(advisor1.Id, advisorDto.Id);
                Assert.AreEqual(advisor1.LastName, advisorDto.LastName);
                Assert.AreEqual(2, advisorDto.EmailAddresses.Count());
                // Second advisor in result
                advisorDto = advisorDtos.ElementAt(1);
                Assert.AreEqual(advisor2.Id, advisorDto.Id);
                Assert.AreEqual(advisor2.LastName, advisorDto.LastName);
                Assert.AreEqual(0, advisorDto.EmailAddresses.Count());
            }

            [TestMethod]
            public async Task QueryAdvisorsByPostAsync_returns_Advisor_DTO_type_emails()
            {
                // Act--get advisor
                var advisorDtos = await advisorService.QueryAdvisorsByPostAsync(new List<string>() { "0000011", "0000012" });
                Assert.AreEqual(advisor1.GetEmailAddresses("FAC").Count(), advisorDtos.First().EmailAddresses.Count());
                var advisorDtoEmails = advisorDtos.First().EmailAddresses;
                var advisorEmails = advisor1.GetEmailAddresses("FAC");
                for (int i = 0; i < advisorEmails.Count(); i++)
                {
                    Assert.AreEqual(advisorEmails.ElementAt(i), advisorDtoEmails.ElementAt(i));
                }
            }

            [TestMethod]
            public async Task QueryAdvisorsByPostAsync_invalid_Advisor_entity_excluded_from_returned_Advisor_DTO_list()
            {
                advisorRepoMock.Setup(repo => repo.GetAdvisorsAsync(It.IsAny<List<string>>(), AdviseeInclusionType.NoAdvisees)).ReturnsAsync((new List<Domain.Planning.Entities.Advisor> { advisor1, null }).AsEnumerable());
                var advisorDtos = await advisorService.QueryAdvisorsByPostAsync(new List<string>() { "0000098", "0000099" });
                loggerMock.Verify(l => l.Error(Moq.It.Is<string>(str => str.StartsWith("Error converting Advisor entity to Advisor DTO."))));
                Assert.AreEqual(1, advisorDtos.Count());
            }

            [TestMethod]
            public async Task QueryAdvisorsByPostAsync_empty_repository_response_returns_empty_Advisor_DTO_list()
            {
                advisorRepoMock.Setup(repo => repo.GetAdvisorsAsync(It.IsAny<List<string>>(), AdviseeInclusionType.NoAdvisees)).ReturnsAsync((new List<Domain.Planning.Entities.Advisor>()).AsEnumerable());
                var advisorDtos = await advisorService.QueryAdvisorsByPostAsync(new List<string>() { "0000098", "0000099" });
                Assert.AreEqual(0, advisorDtos.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task QueryAdvisorsByPostAsync_Rethrows_repository_Exception()
            {
                advisorRepoMock.Setup(repo => repo.GetAdvisorsAsync(It.IsAny<List<string>>(), AdviseeInclusionType.NoAdvisees)).ThrowsAsync(new Exception());
                var advisorDtos = await advisorService.QueryAdvisorsByPostAsync(new List<string>() { "0000098", "0000099" });
            }
        }


        [TestClass]
        public class GetAdvisees : CurrentUserSetup
        {
            private Mock<IDegreePlanRepository> degreePlanRepoMock;
            private IDegreePlanRepository degreePlanRepo;
            private Mock<IStudentDegreePlanRepository> studentDegreePlanRepoMock;
            private IStudentDegreePlanRepository studentDegreePlanRepo;
            private Mock<IAdvisorRepository> advisorRepoMock;
            private IAdvisorRepository advisorRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IAdviseeRepository> adviseeRepoMock;
            private IAdviseeRepository adviseeRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private AdvisorService advisorService;
            private ILogger logger;
            private Domain.Student.Entities.DegreePlans.DegreePlan degreePlan1;
            private Domain.Student.Entities.DegreePlans.DegreePlan degreePlan2;
            private Domain.Student.Entities.PlanningStudent student1;
            private Domain.Student.Entities.PlanningStudent student2;
            private IEnumerable<Domain.Student.Entities.DegreePlans.DegreePlan> degreePlans;
            private Domain.Planning.Entities.Advisor advisor;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private IStudentRepository studentRepo;
            private Mock<IStudentRepository> studentRepoMock;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;
            private IPersonBaseRepository personBaseRepo;
            private Mock<IPersonBaseRepository> personBaseRepoMock;

            [TestInitialize]
            public async void Initialize()
            {
                degreePlanRepoMock = new Mock<IDegreePlanRepository>();
                degreePlanRepo = degreePlanRepoMock.Object;
                studentDegreePlanRepoMock = new Mock<IStudentDegreePlanRepository>();
                studentDegreePlanRepo = studentDegreePlanRepoMock.Object;
                advisorRepoMock = new Mock<IAdvisorRepository>();
                advisorRepo = advisorRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                adviseeRepoMock = new Mock<IAdviseeRepository>();
                adviseeRepo = adviseeRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;
                personBaseRepoMock = new Mock<IPersonBaseRepository>();
                personBaseRepo = personBaseRepoMock.Object;


                // Mock successful Get degree plans for person ID#0000894 and person ID#00004002
                var testStudentDegreePlanRepository = new TestStudentDegreePlanRepository();
                degreePlan1 = (await testStudentDegreePlanRepository.GetAsync()).Where(d => d.Id == 2).First();
                degreePlan2 = (await testStudentDegreePlanRepository.GetAsync()).Where(d => d.Id == 802).First();
                degreePlan1.AddApproval("0000011", Domain.Student.Entities.DegreePlans.DegreePlanApprovalStatus.Approved, DateTime.Now, "130", "2008/FA");
                degreePlans = new List<Domain.Student.Entities.DegreePlans.DegreePlan>() { degreePlan1, degreePlan2 };
                studentDegreePlanRepoMock.Setup(repo => repo.GetAsync(It.Is<IEnumerable<string>>(s => s.Contains("0000894")))).ReturnsAsync(degreePlans);

                // Mock advisor response from advisor Repository, including advisee ids.
                advisor = new Domain.Planning.Entities.Advisor("0000011", "Brown");
                advisor.AddAdvisee("0000894");
                advisor.AddAdvisee("00004002");
                advisor.IsActive = true;
                advisorRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>(), AdviseeInclusionType.AllAdvisees)).ReturnsAsync(advisor);

                // Mock student repo response
                student1 = new Domain.Student.Entities.PlanningStudent("0000894", "Smith", 2, new List<string>() { "BA.ENGL" }) { FirstName = "Bob", MiddleName = "Blakely", EducationalGoal = "Masters Degree", PersonalPronounCode = "XHE" };
                student2 = new Domain.Student.Entities.PlanningStudent("00004002", "Jones", 802, new List<string>() { "BA.MATH" });
                IEnumerable<Domain.Student.Entities.PlanningStudent> students = new List<Domain.Student.Entities.PlanningStudent>() { student1, student2 };
                adviseeRepoMock.Setup(repo => repo.GetAsync(advisor.Advisees, 1, 1)).ReturnsAsync(students);

                // Mock the staff repository

                // Mock Adapters
                var degreePlanDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan>()).Returns(degreePlanDtoAdapter);
                var adviseeDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.PlanningStudent, Dtos.Planning.Advisee>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.PlanningStudent, Dtos.Planning.Advisee>()).Returns(adviseeDtoAdapter);
                var advisorDtoAdapter = new AutoMapperAdapter<Domain.Planning.Entities.Advisor, Dtos.Planning.Advisor>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Planning.Entities.Advisor, Dtos.Planning.Advisor>()).Returns(advisorDtoAdapter);

                // Set up current user
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();

                advisorService = new AdvisorService(
                    adapterRegistry, advisorRepo, degreePlanRepo, termRepo, adviseeRepo, null, currentUserFactory,
                    roleRepo, logger, studentRepo, null, baseConfigurationRepository, personBaseRepo, studentDegreePlanRepo);
            }

            [TestCleanup]
            public void Cleanup()
            {
                degreePlanRepo = null;
                studentDegreePlanRepo = null;
                adapterRegistry = null;
                advisorRepo = null;
                termRepo = null;
                adviseeRepo = null;
                roleRepo = null;
                advisorService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task ThrowsErrorIfPermissionMissing()
            {
                var advisorDto = await advisorService.GetAdviseesAsync("0000011", 1, 1);
            }

            [TestMethod]
            public async Task ReturnsAdviseeDtos_ViewPermission()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Act--get advisees for advisor
                var privacyWrapper = await advisorService.GetAdviseesAsync("0000011", 1, 1);
                var adviseeDtos = privacyWrapper.Dto as List<Advisee>;
                // Assert
                Assert.AreEqual(2, adviseeDtos.Count());
            }

            [TestMethod]
            public async Task ReturnsAdviseeDtos_UpdatePermission()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.UpdateAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Act--get advisees for advisor
                var privacyWrapper = await advisorService.GetAdviseesAsync("0000011", 1, 1);
                var adviseeDtos = privacyWrapper.Dto as List<Advisee>;
                // Assert
                Assert.AreEqual(2, adviseeDtos.Count());
            }

            [TestMethod]
            public async Task ReturnsAdviseeDtos_AllAccessPermission()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Act--get advisees for advisor
                var privacyWrapper = await advisorService.GetAdviseesAsync("0000011", 1, 1);
                var adviseeDtos = privacyWrapper.Dto as List<Advisee>;
                // Assert
                Assert.AreEqual(2, adviseeDtos.Count());
            }

            [TestMethod]
            public async Task ReturnsAdviseeDtos_ReviewPermission()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ReviewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Act--get advisees for advisor
                var privacyWrapper = await advisorService.GetAdviseesAsync("0000011", 1, 1);
                var adviseeDtos = privacyWrapper.Dto as List<Advisee>;
                // Assert
                Assert.AreEqual(2, adviseeDtos.Count());
            }


            [TestMethod]
            public async Task ReturnsStudentDataInAdviseeDto()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Act--get advisees for advisor
                var privacyWrapper = await advisorService.GetAdviseesAsync("0000011", 1, 1);
                var adviseeDtos = privacyWrapper.Dto as List<Advisee>;
                // Assert--
                var advisee = adviseeDtos.ElementAt(0);
                var isApprovalRequested = degreePlan1.ReviewRequested;
                Assert.AreEqual(isApprovalRequested, advisee.ApprovalRequested);
                Assert.AreEqual(degreePlan1.Id, advisee.DegreePlanId);
                Assert.AreEqual(student1.Id, advisee.Id);
                Assert.AreEqual(student1.LastName, advisee.LastName);
                Assert.AreEqual(student1.FirstName, advisee.FirstName);
                Assert.AreEqual(student1.MiddleName, advisee.MiddleName);
                Assert.AreEqual(student1.DegreePlanId, advisee.DegreePlanId);
                Assert.AreEqual(student1.ProgramIds.ElementAt(0), advisee.ProgramIds.ElementAt(0));
                Assert.AreEqual(student1.EducationalGoal, advisee.EducationalGoal);
                Assert.IsNotNull(advisee.PersonalPronounCode);
                Assert.AreEqual(student1.PersonalPronounCode, advisee.PersonalPronounCode);
            }

            [TestMethod]
            public async Task ReturnsEmptyListIfNoAdvisees()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Set up empty list of advisees to return
                advisorRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>(), AdviseeInclusionType.AllAdvisees)).ReturnsAsync(new Domain.Planning.Entities.Advisor("0000011", "Brown") { IsActive = true });
                // Set up thrown error returned by student repo getmulti
                adviseeRepoMock.Setup(repo => repo.GetAsync(It.IsAny<List<string>>(), 1, 1)).ThrowsAsync(new ArgumentException("no student ids provided"));
                // Act--get advisees
                var privacyWrapper = await advisorService.GetAdviseesAsync("0000011", 1, 1);
                var adviseeDtos = privacyWrapper.Dto as List<Advisee>;
                // Assert
                Assert.AreEqual(0, adviseeDtos.Count());
            }

            [TestMethod]
            public async Task ReturnsNullDegreePlanInfoIfNoDegreePlan()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Set up advisee with no degree plan
                degreePlans = new List<Domain.Student.Entities.DegreePlans.DegreePlan>();
                studentDegreePlanRepoMock.Setup(repo => repo.GetAsync(It.Is<IEnumerable<string>>(s => s.Contains("0000894")))).ReturnsAsync(degreePlans);
                // Act--get advisee
                var privacyWrapper = await advisorService.GetAdviseesAsync("0000011", 1, 1);
                var adviseeDtos = privacyWrapper.Dto as List<Advisee>;
                // Assert--
                Assert.IsFalse(adviseeDtos.ElementAt(0).ApprovalRequested);
                Assert.AreEqual(null, adviseeDtos.ElementAt(0).DegreePlanId);
                Assert.AreEqual("0000894", adviseeDtos.ElementAt(0).Id);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task AdvisorNotActive_ThrowsException()
            {
                // Create active staff record
                var id = "0000011";
                var lastName = "Brown";
                advisor = new Domain.Planning.Entities.Advisor(id, lastName);
                advisor.IsActive = false;
                // Set up needed permissions for advisor role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Mock get from advisor Repository
                advisorRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>(), AdviseeInclusionType.AllAdvisees)).ReturnsAsync(advisor);
                var advisees = await advisorService.GetAdviseesAsync(id, int.MaxValue, 1);
            }

            [TestMethod]
            public async Task AdvisorWithNoAdvisees_WithAnyPermissions_ReturnsResults()
            {
                // Create active advisor record
                var id = "0000011";
                var lastName = "Brown";
                advisor = new Domain.Planning.Entities.Advisor(id, lastName);
                advisor.IsActive = true;
                // Mock thrown error from advisor Repository
                advisorRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>(), AdviseeInclusionType.AllAdvisees)).ReturnsAsync(advisor);
                // Set up needed permissions for advisor role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Act--get advisor
                var privacyWrapper = await advisorService.GetAdviseesAsync("0000011", int.MaxValue, 1);
                var adviseeDtos = privacyWrapper.Dto as List<Advisee>;
                // Assert
                Assert.IsTrue(adviseeDtos is IEnumerable<Dtos.Planning.Advisee>);
                Assert.AreEqual(0, adviseeDtos.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task AdvisorWithNoAdvisees_WithoutAnyPermissions_ThrowsException()
            {
                // Create active advisor record
                var id = "0000011";
                var lastName = "Brown";
                advisor = new Domain.Planning.Entities.Advisor(id, lastName);
                advisor.IsActive = true;
                // Mock thrown error from advisor Repository
                advisorRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>(), AdviseeInclusionType.AllAdvisees)).ReturnsAsync(advisor);
                // Act--get advisees
                var advisees = await advisorService.GetAdviseesAsync(id, int.MaxValue, 1);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task AdvisorNotCurrentUser_ThrowsException()
            {
                // Current User Id must match the Advisor Id in the request
                // Create active staff record
                var id = "12345";
                var lastName = "Smith";
                advisor = new Domain.Planning.Entities.Advisor(id, lastName);
                advisor.IsActive = true;
                // Mock thrown error from advisor Repository
                advisorRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>(), AdviseeInclusionType.AllAdvisees)).ReturnsAsync(advisor);
                // Act--get advisor (current user is 0000011)
                var advisorDto = await advisorService.GetAdviseesAsync("12345", int.MaxValue, 1);
            }

        }
        [TestClass]
        public class Search : CurrentUserSetup
        {
            private Mock<IDegreePlanRepository> degreePlanRepoMock;
            private IDegreePlanRepository degreePlanRepo;
            private Mock<IStudentDegreePlanRepository> studentDegreePlanRepoMock;
            private IStudentDegreePlanRepository studentDegreePlanRepo;
            private Mock<IAdvisorRepository> advisorRepoMock;
            private IAdvisorRepository advisorRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IAdviseeRepository> adviseeRepoMock;
            private IAdviseeRepository adviseeRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private AdvisorService advisorService;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private Domain.Planning.Entities.Advisor advisor;
            private IStudentRepository studentRepo;
            private Mock<IStudentRepository> studentRepoMock;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;
            private IPersonBaseRepository personBaseRepo;
            private Mock<IPersonBaseRepository> personBaseRepoMock;

            [TestInitialize]
            public void Initialize()
            {
                degreePlanRepoMock = new Mock<IDegreePlanRepository>();
                degreePlanRepo = degreePlanRepoMock.Object;
                studentDegreePlanRepoMock = new Mock<IStudentDegreePlanRepository>();
                studentDegreePlanRepo = studentDegreePlanRepoMock.Object;
                advisorRepoMock = new Mock<IAdvisorRepository>();
                advisorRepo = advisorRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                adviseeRepoMock = new Mock<IAdviseeRepository>();
                adviseeRepo = adviseeRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                logger = new Mock<ILogger>().Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                // Set up current user
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;
                personBaseRepoMock = new Mock<IPersonBaseRepository>();
                personBaseRepo = personBaseRepoMock.Object;


                // Mock advisor response from advisor Repository, including advisee ids.
                advisor = new Domain.Planning.Entities.Advisor("0000011", "Brown");
                advisor.AddAdvisee("0000894");
                advisor.AddAdvisee("00004002");
                advisor.IsActive = true;
                advisorRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>(), AdviseeInclusionType.AllAdvisees)).ReturnsAsync(advisor);

                // Mock AdviseeRepository Get
                var student1 = new Domain.Student.Entities.PlanningStudent("0000894", "Smith", 2, new List<string>() { "BA.ENGL" }) { FirstName = "Bob", MiddleName = "Blakely" };
                var student2 = new Domain.Student.Entities.PlanningStudent("00004002", "Jones", 802, new List<string>() { "BA.MATH" });
                IEnumerable<Domain.Student.Entities.PlanningStudent> students = new List<Domain.Student.Entities.PlanningStudent>() { student1, student2 };
                adviseeRepoMock.Setup(repo => repo.GetAsync(advisor.Advisees, 1, 1)).ReturnsAsync(students);

                // Mock Adapters
                var degreePlanDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan>()).Returns(degreePlanDtoAdapter);
                var adviseeDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.PlanningStudent, Dtos.Planning.Advisee>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.PlanningStudent, Dtos.Planning.Advisee>()).Returns(adviseeDtoAdapter);
                var advisorDtoAdapter = new AutoMapperAdapter<Domain.Planning.Entities.Advisor, Dtos.Planning.Advisor>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Planning.Entities.Advisor, Dtos.Planning.Advisor>()).Returns(advisorDtoAdapter);

                // Set up current user
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();

                advisorService = new AdvisorService(
                    adapterRegistry, advisorRepo, degreePlanRepo, termRepo, adviseeRepo, null, currentUserFactory,
                    roleRepo, logger, studentRepo, null, baseConfigurationRepository, personBaseRepo, studentDegreePlanRepo);
            }

            [TestCleanup]
            public void Cleanup()
            {
                degreePlanRepo = null;
                studentDegreePlanRepo = null;
                adapterRegistry = null;
                advisorRepo = null;
                termRepo = null;
                adviseeRepo = null;
                roleRepo = null;
                advisorService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task ThrowsErrorIfPermissionMissing()
            {
                await advisorService.SearchAsync("jones", 1, 1);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task ThrowsErrorWhenSearchStringNull()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Act--search 
                await advisorService.SearchAsync(null, 1, 1);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task ThrowsErrorWhenSearchStringEmpty()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Act--search
                await advisorService.SearchAsync("", 1, 1);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task ThrowsErrorWhenSearchStringOnlyBlanks()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Act--search
                await advisorService.SearchAsync("  ", 1, 1);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task ThrowsErrorWhenSearchStringTooShort()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Act--search
                await advisorService.SearchAsync(" x ", 1, 1);
            }

            [TestMethod]
            public async Task SearchStringContainsOnlyOneString()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Setup callback
                string[] parsedStrings = null;
                adviseeRepoMock.Setup(repo => repo.SearchByNameAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                    .Callback<string, string, string, int, int, IEnumerable<string>>((string s1, string s2, string s3, int i1, int i2, IEnumerable<string> a) => parsedStrings = new string[] { s1, s2, s3 })
                    .ReturnsAsync(new List<Domain.Student.Entities.PlanningStudent>());
                // Act--search
                await advisorService.SearchAsync("jones", 1, 1);
                // Asserts
                Assert.AreEqual("jones", parsedStrings[0]);
                Assert.AreEqual(null, parsedStrings[1]);
                Assert.AreEqual(null, parsedStrings[2]);
            }

            [TestMethod]
            public async Task TwoStringsNoCommaParsedAsFirstLast()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Set up callback
                string[] parsedStrings = null;
                adviseeRepoMock.Setup(repo => repo.SearchByNameAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                    .Callback<string, string, string, int, int, IEnumerable<string>>((string s1, string s2, string s3, int i1, int i2, IEnumerable<string> a) => parsedStrings = new string[] { s1, s2, s3 })
                    .ReturnsAsync(new List<Domain.Student.Entities.PlanningStudent>());
                // Act--search
                await advisorService.SearchAsync("barney jones", 1, 1);
                // Assert--parsed strings
                Assert.AreEqual("jones", parsedStrings[0]); // Last Name
                Assert.AreEqual("barney", parsedStrings[1]); // First Name
                Assert.AreEqual(null, parsedStrings[2]); // Middle Name
            }

            [TestMethod]
            public async Task ThreeStringsNoCommaParsedAsFirstMiddleLast()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // set up callback
                string[] parsedStrings = null;
                adviseeRepoMock.Setup(repo => repo.SearchByNameAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                    .Callback<string, string, string, int, int, IEnumerable<string>>((string s1, string s2, string s3, int i1, int i2, IEnumerable<string> a) => parsedStrings = new string[] { s1, s2, s3 })
                    .ReturnsAsync(new List<Domain.Student.Entities.PlanningStudent>());
                // Act--search
                await advisorService.SearchAsync("barney a jones", 1, 1);
                // Assert--parsed strings
                Assert.AreEqual("jones", parsedStrings[0]); // Last Name
                Assert.AreEqual("barney", parsedStrings[1]); // First Name
                Assert.AreEqual("a", parsedStrings[2]); // Middle Name
            }

            [TestMethod]
            public async Task TwoStringsWithCommaParsedAsLastFirst()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Setup callback
                string[] parsedStrings = null;
                adviseeRepoMock.Setup(repo => repo.SearchByNameAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                    .Callback<string, string, string, int, int, IEnumerable<string>>((string s1, string s2, string s3, int i1, int i2, IEnumerable<string> a) => parsedStrings = new string[] { s1, s2, s3 })
                    .ReturnsAsync(new List<Domain.Student.Entities.PlanningStudent>());
                // Act--search
                await advisorService.SearchAsync("jones, barney", 1, 1);
                // Assert--parsed strings
                Assert.AreEqual("jones", parsedStrings[0]); // Last Name
                Assert.AreEqual("barney", parsedStrings[1]); // First Name
                Assert.AreEqual(null, parsedStrings[2]); // Middle Name
            }

            [TestMethod]
            public async Task ThreeStringsWithCommaParsedAsLastFirstMiddle()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // setup callback
                string[] parsedStrings = null;
                adviseeRepoMock.Setup(repo => repo.SearchByNameAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                    .Callback<string, string, string, int, int, IEnumerable<string>>((string s1, string s2, string s3, int i1, int i2, IEnumerable<string> a) => parsedStrings = new string[] { s1, s2, s3 })
                    .ReturnsAsync(new List<Domain.Student.Entities.PlanningStudent>());
                // Act--search
                await advisorService.SearchAsync("jones, barney a", 1, 1);
                // Assert--parsed strings
                Assert.AreEqual("jones", parsedStrings[0]); // Last Name
                Assert.AreEqual("barney", parsedStrings[1]); // First Name
                Assert.AreEqual("a", parsedStrings[2]); // Middle Name
            }

            [TestMethod]
            public async Task OneStringPrecededWithCommaParsesEmptyLastName()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Set up callback
                string[] parsedStrings = null;
                adviseeRepoMock.Setup(repo => repo.SearchByNameAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                    .Callback<string, string, string, int, int, IEnumerable<string>>((string s1, string s2, string s3, int i1, int i2, IEnumerable<string> a) => parsedStrings = new string[] { s1, s2, s3 })
                    .ReturnsAsync(new List<Domain.Student.Entities.PlanningStudent>());
                // Act--search
                await advisorService.SearchAsync(" , jones", 1, 1);
                // Assert--parsed strings
                // An error will be thrown at the repository level
                Assert.IsTrue(string.IsNullOrEmpty(parsedStrings[0])); // Last name
            }

            [TestMethod]
            public async Task OneStringFollowedByCommaParsesOnlyLastName()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Setup callback
                string[] parsedStrings = null;
                adviseeRepoMock.Setup(repo => repo.SearchByNameAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                    .Callback<string, string, string, int, int, IEnumerable<string>>((string s1, string s2, string s3, int i1, int i2, IEnumerable<string> a) => parsedStrings = new string[] { s1, s2, s3 })
                    .ReturnsAsync(new List<Domain.Student.Entities.PlanningStudent>());
                // Act--search
                await advisorService.SearchAsync(" jones ,  ", 1, 1);
                // Assert--parsed strings
                Assert.AreEqual("jones", parsedStrings[0]); // Last name
                Assert.IsTrue(string.IsNullOrEmpty(parsedStrings[1])); // Middle name
                Assert.IsTrue(string.IsNullOrEmpty(parsedStrings[2])); // Last name
            }

            [TestMethod]
            public async Task MultipleSpacesParsedOut()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // setup callback
                string[] parsedStrings = null;
                adviseeRepoMock.Setup(repo => repo.SearchByNameAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                    .Callback<string, string, string, int, int, IEnumerable<string>>((string s1, string s2, string s3, int i1, int i2, IEnumerable<string> a) => parsedStrings = new string[] { s1, s2, s3 })
                    .ReturnsAsync(new List<Domain.Student.Entities.PlanningStudent>());
                // Act--search
                await advisorService.SearchAsync(" jones ,  joe   billy  ", 1, 1);
                // Assert--parsed strings
                Assert.AreEqual("jones", parsedStrings[0]); // Last name
                Assert.AreEqual("joe", parsedStrings[1]); // Middle name
                Assert.AreEqual("billy", parsedStrings[2]); // Last name
            }

            [TestMethod]
            public async Task ValidStudentIdReturnsId()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // setup returned student
                var student1 = new Domain.Student.Entities.PlanningStudent("0000894", "Smith", 2, new List<string>() { "BA.ENGL" }) { FirstName = "Bob", MiddleName = "Blakely" };
                adviseeRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);
                // Act--search
                var result = await advisorService.SearchAsync("0000894", 1, 1);
                // Assert student returned
                var validStudent = result.ElementAt(0);
                Assert.AreEqual("0000894", validStudent);
            }

            [TestMethod]
            public async Task InvalidStudentIdThrowsError()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // setup null student response
                Domain.Student.Entities.PlanningStudent student = null;
                adviseeRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>())).ReturnsAsync(student);
                // Act--search
                var result = await advisorService.SearchAsync("9999999", 1, 1);
                // Assert--empty list returned
                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            public async Task RemovesPunctuationCharacters()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // setup callback
                string[] parsedStrings = null;
                adviseeRepoMock.Setup(repo => repo.SearchByNameAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                    .Callback<string, string, string, int, int, IEnumerable<string>>((string s1, string s2, string s3, int i1, int i2, IEnumerable<string> a) => parsedStrings = new string[] { s1, s2, s3 })
                    .ReturnsAsync(new List<Domain.Student.Entities.PlanningStudent>());
                // Act--search
                var result = await advisorService.SearchAsync("james bing :;<.>?/1234567890]smith~!@#$%^&*()_+={}|", 1, 1);
                // Assert--punctuation removed from parsed values
                Assert.AreEqual("smith", parsedStrings[0]); // Last name
                Assert.AreEqual("james", parsedStrings[1]); // Middle name
                Assert.AreEqual("bing", parsedStrings[2]); // Last name
            }

            [TestMethod]
            public async Task RemovesSpaceInLastName()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // setup callback
                string[] parsedStrings = null;
                adviseeRepoMock.Setup(repo => repo.SearchByNameAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                    .Callback<string, string, string, int, int, IEnumerable<string>>((string s1, string s2, string s3, int i1, int i2, IEnumerable<string> a) => parsedStrings = new string[] { s1, s2, s3 })
                    .ReturnsAsync(new List<Domain.Student.Entities.PlanningStudent>());
                // Act--search
                var result = await advisorService.SearchAsync(":;<.>?/1234567890]~!@#$%^&*()_+={}| van buren, james bing", It.IsAny<int>(), It.IsAny<int>());
                Assert.AreEqual("vanburen", parsedStrings[0]); // Last name
                // Assert--space removed
                Assert.AreEqual("james", parsedStrings[1]); // Middle name
                Assert.AreEqual("bing", parsedStrings[2]); // Last name
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task AdvisorNoAdvisees_WithOutAnyPermissions_ThrowsException()
            {
                // Create active advisor record
                var id = "0000011";
                var lastName = "Brown";
                var advisor = new Domain.Planning.Entities.Advisor(id, lastName);
                advisor.IsActive = true;
                // Set up needed permissions for advisor role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Mock thrown error from advisor Repository
                advisorRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>(), AdviseeInclusionType.AllAdvisees)).ReturnsAsync(advisor);
                // Act--invoke search
                await advisorService.SearchAsync("jones", 1, 1);
            }

            [TestMethod]
            public async Task AdvisorNoAdvisees_WithAnyPermissions_ReturnsResults()
            {
                // Create active staff record
                var id = "0000011";
                var lastName = "Brown";
                var advisor = new Domain.Planning.Entities.Advisor(id, lastName);
                advisor.IsActive = true;
                // Mock advisor Repository
                advisorRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>(), AdviseeInclusionType.AllAdvisees)).ReturnsAsync(advisor);
                // Set up needed permissions for advisor role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Mock search response
                var student1 = new Domain.Student.Entities.PlanningStudent("0000894", "Smith", 2, new List<string>() { "BA.ENGL" }) { FirstName = "Bob", MiddleName = "Blakely" };
                adviseeRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);
                // Act--search
                var result = await advisorService.SearchAsync("0000894", 1, 1);
                // Assert
                Assert.AreEqual(1, result.Count());
            }
        }

        [TestClass]
        public class Search2 : CurrentUserSetup
        {
            private Mock<IDegreePlanRepository> degreePlanRepoMock;
            private IDegreePlanRepository degreePlanRepo;
            private Mock<IStudentDegreePlanRepository> studentDegreePlanRepoMock;
            private IStudentDegreePlanRepository studentDegreePlanRepo;
            private Mock<IAdvisorRepository> advisorRepoMock;
            private IAdvisorRepository advisorRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IAdviseeRepository> adviseeRepoMock;
            private IAdviseeRepository adviseeRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private AdvisorService advisorService;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private IStudentRepository studentRepo;
            private Mock<IStudentRepository> studentRepoMock;
            private ICurrentUserFactory currentUserFactory;
            private Domain.Planning.Entities.Advisor advisor;
            private Dtos.Planning.AdviseeSearchCriteria criteria;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;
            private IPersonBaseRepository personBaseRepo;
            private Mock<IPersonBaseRepository> personBaseRepoMock;

            [TestInitialize]
            public void Initialize()
            {
                degreePlanRepoMock = new Mock<IDegreePlanRepository>();
                degreePlanRepo = degreePlanRepoMock.Object;
                studentDegreePlanRepoMock = new Mock<IStudentDegreePlanRepository>();
                studentDegreePlanRepo = studentDegreePlanRepoMock.Object;
                advisorRepoMock = new Mock<IAdvisorRepository>();
                advisorRepo = advisorRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                adviseeRepoMock = new Mock<IAdviseeRepository>();
                adviseeRepo = adviseeRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                logger = new Mock<ILogger>().Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                // Set up current user
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;
                personBaseRepoMock = new Mock<IPersonBaseRepository>();
                personBaseRepo = personBaseRepoMock.Object;

                // Mock advisor response from advisor Repository, including advisee ids.
                advisor = new Domain.Planning.Entities.Advisor("0000011", "Brown");
                advisor.AddAdvisee("0000894");
                advisor.AddAdvisee("00004002");
                advisor.IsActive = true;
                advisorRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>(), AdviseeInclusionType.AllAdvisees)).ReturnsAsync(advisor);

                // Initialize the AdvisorSearchCriteria for all the existing search string tests
                criteria = new AdviseeSearchCriteria();

                // Mock AdviseeRepository Get
                var student1 = new Domain.Student.Entities.PlanningStudent("0000894", "Smith", 2, new List<string>() { "BA.ENGL" }) { FirstName = "Bob", MiddleName = "Blakely" };
                var student2 = new Domain.Student.Entities.PlanningStudent("00004002", "Jones", 802, new List<string>() { "BA.MATH" });
                IEnumerable<Domain.Student.Entities.PlanningStudent> students = new List<Domain.Student.Entities.PlanningStudent>() { student1, student2 };
                adviseeRepoMock.Setup(repo => repo.GetAsync(advisor.Advisees, 1, 1)).ReturnsAsync(students);

                // Mock Adapters
                var degreePlanDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan>()).Returns(degreePlanDtoAdapter);
                var adviseeDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.PlanningStudent, Dtos.Planning.Advisee>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.PlanningStudent, Dtos.Planning.Advisee>()).Returns(adviseeDtoAdapter);
                var advisorDtoAdapter = new AutoMapperAdapter<Domain.Planning.Entities.Advisor, Dtos.Planning.Advisor>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Planning.Entities.Advisor, Dtos.Planning.Advisor>()).Returns(advisorDtoAdapter);

                // Set up current user
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();

                advisorService = new AdvisorService(
                    adapterRegistry, advisorRepo, degreePlanRepo, termRepo, adviseeRepo, null, currentUserFactory,
                    roleRepo, logger, studentRepo, null, baseConfigurationRepository, personBaseRepo, studentDegreePlanRepo);
            }

            [TestCleanup]
            public void Cleanup()
            {
                degreePlanRepo = null;
                studentDegreePlanRepo = null;
                adapterRegistry = null;
                advisorRepo = null;
                termRepo = null;
                adviseeRepo = null;
                roleRepo = null;
                advisorService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task ThrowsErrorIfPermissionMissing()
            {
                await advisorService.Search2Async("jones", 1, 1);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task ThrowsErrorWhenSearchStringNull()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Act--search 
                await advisorService.Search2Async(null, 1, 1);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task ThrowsErrorWhenSearchStringEmpty()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Act--search
                await advisorService.Search2Async("", 1, 1);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task ThrowsErrorWhenSearchStringOnlyBlanks()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Act--search
                await advisorService.Search2Async("  ", 1, 1);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task ThrowsErrorWhenSearchStringTooShort()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Act--search
                await advisorService.Search2Async(" x ", 1, 1);
            }

            [TestMethod]
            public async Task SearchStringContainsOnlyOneString()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Setup callback
                string[] parsedStrings = null;
                adviseeRepoMock.Setup(repo => repo.SearchByNameAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                    .Callback<string, string, string, int, int, IEnumerable<string>>((string s1, string s2, string s3, int i1, int i2, IEnumerable<string> a) => parsedStrings = new string[] { s1, s2, s3 })
                    .ReturnsAsync(new List<Domain.Student.Entities.PlanningStudent>());
                // Act--search
                await advisorService.Search2Async("jones", 1, 1);
                // Asserts
                Assert.AreEqual("jones", parsedStrings[0]);
                Assert.AreEqual(null, parsedStrings[1]);
                Assert.AreEqual(null, parsedStrings[2]);
            }

            [TestMethod]
            public async Task TwoStringsNoCommaParsedAsFirstLast()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Set up callback
                string[] parsedStrings = null;
                adviseeRepoMock.Setup(repo => repo.SearchByNameAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                    .Callback<string, string, string, int, int, IEnumerable<string>>((string s1, string s2, string s3, int i1, int i2, IEnumerable<string> a) => parsedStrings = new string[] { s1, s2, s3 })
                    .ReturnsAsync(new List<Domain.Student.Entities.PlanningStudent>());
                // Act--search
                await advisorService.Search2Async("barney jones", 1, 1);
                // Assert--parsed strings
                Assert.AreEqual("jones", parsedStrings[0]); // Last Name
                Assert.AreEqual("barney", parsedStrings[1]); // First Name
                Assert.AreEqual(null, parsedStrings[2]); // Middle Name
            }

            [TestMethod]
            public async Task ThreeStringsNoCommaParsedAsFirstMiddleLast()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // set up callback
                string[] parsedStrings = null;
                adviseeRepoMock.Setup(repo => repo.SearchByNameAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                    .Callback<string, string, string, int, int, IEnumerable<string>>((string s1, string s2, string s3, int i1, int i2, IEnumerable<string> a) => parsedStrings = new string[] { s1, s2, s3 })
                    .ReturnsAsync(new List<Domain.Student.Entities.PlanningStudent>());
                // Act--search
                await advisorService.Search2Async("barney a jones", 1, 1);
                // Assert--parsed strings
                Assert.AreEqual("jones", parsedStrings[0]); // Last Name
                Assert.AreEqual("barney", parsedStrings[1]); // First Name
                Assert.AreEqual("a", parsedStrings[2]); // Middle Name
            }

            [TestMethod]
            public async Task TwoStringsWithCommaParsedAsLastFirst()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Setup callback
                string[] parsedStrings = null;
                adviseeRepoMock.Setup(repo => repo.SearchByNameAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                    .Callback<string, string, string, int, int, IEnumerable<string>>((string s1, string s2, string s3, int i1, int i2, IEnumerable<string> a) => parsedStrings = new string[] { s1, s2, s3 })
                    .ReturnsAsync(new List<Domain.Student.Entities.PlanningStudent>());
                // Act--search
                await advisorService.Search2Async("jones, barney", 1, 1);
                // Assert--parsed strings
                Assert.AreEqual("jones", parsedStrings[0]); // Last Name
                Assert.AreEqual("barney", parsedStrings[1]); // First Name
                Assert.AreEqual(null, parsedStrings[2]); // Middle Name
            }

            [TestMethod]
            public async Task ThreeStringsWithCommaParsedAsLastFirstMiddle()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // setup callback
                string[] parsedStrings = null;
                adviseeRepoMock.Setup(repo => repo.SearchByNameAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                    .Callback<string, string, string, int, int, IEnumerable<string>>((string s1, string s2, string s3, int i1, int i2, IEnumerable<string> a) => parsedStrings = new string[] { s1, s2, s3 })
                    .ReturnsAsync(new List<Domain.Student.Entities.PlanningStudent>());
                // Act--search
                await advisorService.Search2Async("jones, barney a", 1, 1);
                // Assert--parsed strings
                Assert.AreEqual("jones", parsedStrings[0]); // Last Name
                Assert.AreEqual("barney", parsedStrings[1]); // First Name
                Assert.AreEqual("a", parsedStrings[2]); // Middle Name
            }

            [TestMethod]
            public async Task OneStringPrecededWithCommaParsesEmptyLastName()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Set up callback
                string[] parsedStrings = null;
                adviseeRepoMock.Setup(repo => repo.SearchByNameAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                    .Callback<string, string, string, int, int, IEnumerable<string>>((string s1, string s2, string s3, int i1, int i2, IEnumerable<string> a) => parsedStrings = new string[] { s1, s2, s3 })
                    .ReturnsAsync(new List<Domain.Student.Entities.PlanningStudent>());
                // Act--search
                await advisorService.Search2Async(" , jones", 1, 1);
                // Assert--parsed strings
                // An error will be thrown at the repository level
                Assert.IsTrue(string.IsNullOrEmpty(parsedStrings[0])); // Last name
            }

            [TestMethod]
            public async Task OneStringFollowedByCommaParsesOnlyLastName()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Setup callback
                string[] parsedStrings = null;
                adviseeRepoMock.Setup(repo => repo.SearchByNameAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                    .Callback<string, string, string, int, int, IEnumerable<string>>((string s1, string s2, string s3, int i1, int i2, IEnumerable<string> a)
                        =>
                        parsedStrings = new string[] { s1, s2, s3 })
                    .ReturnsAsync(new List<Domain.Student.Entities.PlanningStudent>());
                // Act--search
                await advisorService.Search2Async(" jones ,  ", 1, 1);
                // Assert--parsed strings
                Assert.AreEqual("jones", parsedStrings[0]); // Last name
                Assert.IsTrue(string.IsNullOrEmpty(parsedStrings[1])); // Middle name
                Assert.IsTrue(string.IsNullOrEmpty(parsedStrings[2])); // Last name
            }

            [TestMethod]
            public async Task MultipleSpacesParsedOut()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // setup callback
                string[] parsedStrings = null;
                adviseeRepoMock.Setup(repo => repo.SearchByNameAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                    .Callback<string, string, string, int, int, IEnumerable<string>>((string s1, string s2, string s3, int i1, int i2, IEnumerable<string> a)
                        =>
                        parsedStrings = new string[] { s1, s2, s3 })
                    .ReturnsAsync(new List<Domain.Student.Entities.PlanningStudent>());
                // Act--search
                await advisorService.Search2Async(" jones ,  joe   billy  ", 1, 1);
                // Assert--parsed strings
                Assert.AreEqual("jones", parsedStrings[0]); // Last name
                Assert.AreEqual("joe", parsedStrings[1]); // Middle name
                Assert.AreEqual("billy", parsedStrings[2]); // Last name
            }

            [TestMethod]
            public async Task ValidStudentIdReturnsId()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // setup returned student
                var student1 = new Domain.Student.Entities.PlanningStudent("0000894", "Smith", 2, new List<string>() { "BA.ENGL" }) { FirstName = "Bob", MiddleName = "Blakely" };
                adviseeRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);
                // Act--search
                var result = await advisorService.Search2Async("0000894", 1, 1);
                // Assert student returned
                var validStudent = result.ElementAt(0);
                Assert.AreEqual("0000894", validStudent.Id);
            }

            [TestMethod]
            public async Task InvalidStudentIdThrowsError()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // setup null student response
                Domain.Student.Entities.PlanningStudent student = null;
                adviseeRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>())).ReturnsAsync(student);
                // Act--search
                var result = await advisorService.Search2Async("9999999", 1, 1);
                // Assert--empty list returned
                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            public async Task RemovesPunctuationCharacters()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // setup callback
                string[] parsedStrings = null;
                adviseeRepoMock.Setup(repo => repo.SearchByNameAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                    .Callback<string, string, string, int, int, IEnumerable<string>>((string s1, string s2, string s3, int i1, int i2, IEnumerable<string> a) => parsedStrings = new string[] { s1, s2, s3 })
                    .ReturnsAsync(new List<Domain.Student.Entities.PlanningStudent>());
                // Act--search
                var result = await advisorService.Search2Async("james bing :;<.>?/1234567890]smith~!@#$%^&*()_+={}|", 1, 1);
                // Assert--punctuation removed from parsed values
                Assert.AreEqual("smith", parsedStrings[0]); // Last name
                Assert.AreEqual("james", parsedStrings[1]); // Middle name
                Assert.AreEqual("bing", parsedStrings[2]); // Last name
            }

            [TestMethod]
            public async Task RemovesSpaceInLastName()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // setup callback
                string[] parsedStrings = null;
                adviseeRepoMock.Setup(repo => repo.SearchByNameAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                    .Callback<string, string, string, int, int, IEnumerable<string>>((string s1, string s2, string s3, int i1, int i2, IEnumerable<string> a) => parsedStrings = new string[] { s1, s2, s3 })
                    .ReturnsAsync(new List<Domain.Student.Entities.PlanningStudent>());
                // Act--search
                var result = await advisorService.Search2Async(":;<.>?/1234567890]~!@#$%^&*()_+={}| van buren, james bing", It.IsAny<int>(), It.IsAny<int>());
                Assert.AreEqual("vanburen", parsedStrings[0]); // Last name
                // Assert--space removed
                Assert.AreEqual("james", parsedStrings[1]); // Middle name
                Assert.AreEqual("bing", parsedStrings[2]); // Last name
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task AdvisorNoAdvisees_WithOutAnyPermissions_ThrowsException()
            {
                // Create active advisor record
                var id = "0000011";
                var lastName = "Brown";
                var advisor = new Domain.Planning.Entities.Advisor(id, lastName);
                advisor.IsActive = true;
                // Set up needed permissions for advisor role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Mock thrown error from advisor Repository
                advisorRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>(), AdviseeInclusionType.AllAdvisees)).ReturnsAsync(advisor);
                // Act--invoke search
                await advisorService.Search2Async("jones", 1, 1);
            }

            [TestMethod]
            public async Task AdvisorNoAdvisees_WithAnyPermissions_ReturnsResults()
            {
                // Create active staff record
                var id = "0000011";
                var lastName = "Brown";
                var advisor = new Domain.Planning.Entities.Advisor(id, lastName);
                advisor.IsActive = true;
                // Mock advisor Repository
                advisorRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>(), AdviseeInclusionType.AllAdvisees)).ReturnsAsync(advisor);
                // Set up needed permissions for advisor role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Mock search response
                var student1 = new Domain.Student.Entities.PlanningStudent("0000894", "Smith", 2, new List<string>() { "BA.ENGL" }) { FirstName = "Bob", MiddleName = "Blakely" };
                adviseeRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);
                // Act--search
                var result = await advisorService.Search2Async("0000894", 1, 1);
                // Assert
                Assert.AreEqual(1, result.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task Advisor_NotPerson_ThrowsException()
            {
                // Mock thrown error from advisor Repository
                advisorRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>(), AdviseeInclusionType.AllAdvisees)).ThrowsAsync(new KeyNotFoundException());
                // Set up needed permissions for advisor role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Mock search response
                var student1 = new Domain.Student.Entities.PlanningStudent("0000894", "Smith", 2, new List<string>() { "BA.ENGL" }) { FirstName = "Bob", MiddleName = "Blakely" };
                adviseeRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);
                // Act--search
                var result = await advisorService.Search2Async("0000894", 1, 1);
            }
        }

        [TestClass]
        public class GetAdvisorPermissions : CurrentUserSetup
        {
            private Mock<IDegreePlanRepository> degreePlanRepoMock;
            private IDegreePlanRepository degreePlanRepo;
            private Mock<IStudentDegreePlanRepository> studentDegreePlanRepoMock;
            private IStudentDegreePlanRepository studentDegreePlanRepo;
            private Mock<IAdvisorRepository> advisorRepoMock;
            private IAdvisorRepository advisorRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IAdviseeRepository> adviseeRepoMock;
            private IAdviseeRepository adviseeRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private AdvisorService advisorService;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private IStudentRepository studentRepo;
            private Mock<IStudentRepository> studentRepoMock;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;
            private IPersonBaseRepository personBaseRepo;
            private Mock<IPersonBaseRepository> personBaseRepoMock;

            [TestInitialize]
            public void Initialize()
            {
                degreePlanRepoMock = new Mock<IDegreePlanRepository>();
                degreePlanRepo = degreePlanRepoMock.Object;
                studentDegreePlanRepoMock = new Mock<IStudentDegreePlanRepository>();
                studentDegreePlanRepo = studentDegreePlanRepoMock.Object;
                advisorRepoMock = new Mock<IAdvisorRepository>();
                advisorRepo = advisorRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                adviseeRepoMock = new Mock<IAdviseeRepository>();
                adviseeRepo = adviseeRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;
                personBaseRepoMock = new Mock<IPersonBaseRepository>();
                personBaseRepo = personBaseRepoMock.Object;

                // Set up current user
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();

                advisorService = new AdvisorService(
                    adapterRegistry, advisorRepo, degreePlanRepo, termRepo, adviseeRepo, null, currentUserFactory,
                    roleRepo, logger, studentRepo, null, baseConfigurationRepository, personBaseRepo, studentDegreePlanRepo);
            }

            [TestMethod]
            public async Task ReturnsPermissions()
            {
                // Set up update permissions on advisor's role
                // Old ones:
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAdviseeDegreePlan));
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.UpdateAdviseeDegreePlan));
                // Current ones:
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ReviewAnyAdvisee));
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ReviewAssignedAdvisees));
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.UpdateAnyAdvisee));
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.UpdateAssignedAdvisees));
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAnyAdvisee));
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Act - call method that determines whether current user has permissions
                var result = await advisorService.GetAdvisorPermissionsAsync();

                // Assert--result is true
                Assert.IsTrue(result.Contains(PlanningPermissionCodes.ViewAdviseeDegreePlan));
                Assert.IsTrue(result.Contains(PlanningPermissionCodes.UpdateAdviseeDegreePlan));
                Assert.IsTrue(result.Contains(PlanningPermissionCodes.ViewAnyAdvisee));
                Assert.IsTrue(result.Contains(PlanningPermissionCodes.ViewAssignedAdvisees));
                Assert.IsTrue(result.Contains(PlanningPermissionCodes.ReviewAnyAdvisee));
                Assert.IsTrue(result.Contains(PlanningPermissionCodes.ReviewAssignedAdvisees));
                Assert.IsTrue(result.Contains(PlanningPermissionCodes.UpdateAnyAdvisee));
                Assert.IsTrue(result.Contains(PlanningPermissionCodes.UpdateAssignedAdvisees));
                Assert.IsTrue(result.Contains(PlanningPermissionCodes.AllAccessAnyAdvisee));
                Assert.IsTrue(result.Contains(PlanningPermissionCodes.AllAccessAssignedAdvisees));
            }

            [TestMethod]
            public async Task ReturnsEmptyListIfNoPermissions()
            {
                // Default advisor does not have any permissions
                // Act - call method that determines whether current user has permissions
                var result = await advisorService.GetAdvisorPermissionsAsync();

                // Assert--result is true
                Assert.IsTrue(result.Count() == 0);
            }
        }

        [TestClass]
        public class AdvisorService_GetAdvisingPermissions2Async_Tests
        {
            private Mock<IDegreePlanRepository> degreePlanRepoMock;
            private IDegreePlanRepository degreePlanRepo;
            private Mock<IStudentDegreePlanRepository> studentDegreePlanRepoMock;
            private IStudentDegreePlanRepository studentDegreePlanRepo;
            private Mock<IAdvisorRepository> advisorRepoMock;
            private IAdvisorRepository advisorRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IAdviseeRepository> adviseeRepoMock;
            private IAdviseeRepository adviseeRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private AdvisorService advisorService;
            private Mock<ILogger> loggerMock;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private IStudentRepository studentRepo;
            private Mock<IStudentRepository> studentRepoMock;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;
            private IPersonBaseRepository personBaseRepo;
            private Mock<IPersonBaseRepository> personBaseRepoMock;
            private List<Role> roles;

            [TestInitialize]
            public void AdvisorService_GetAdvisingPermissions2Async_Initialize()
            {
                degreePlanRepoMock = new Mock<IDegreePlanRepository>();
                degreePlanRepo = degreePlanRepoMock.Object;
                studentDegreePlanRepoMock = new Mock<IStudentDegreePlanRepository>();
                studentDegreePlanRepo = studentDegreePlanRepoMock.Object;
                advisorRepoMock = new Mock<IAdvisorRepository>();
                advisorRepo = advisorRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                adviseeRepoMock = new Mock<IAdviseeRepository>();
                adviseeRepo = adviseeRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                loggerMock = new Mock<ILogger>();
                logger = loggerMock.Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;
                personBaseRepoMock = new Mock<IPersonBaseRepository>();
                personBaseRepo = personBaseRepoMock.Object;

                // Set up current user
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();

                // Set up role repository
                Role role1 = new Role(1, "VIEW.ANY.ADVISEE");
                role1.AddPermission(new Permission("VIEW.ANY.ADVISEE"));
                Role role2 = new Role(1, "REVIEW.ANY.ADVISEE");
                role2.AddPermission(new Permission("REVIEW.ANY.ADVISEE"));
                Role role3 = new Role(1, "UDPATE.ANY.ADVISEE");
                role3.AddPermission(new Permission("UDPATE.ANY.ADVISEE"));
                Role role4 = new Role(1, "ALL.ACCESS.ANY.ADVISEE");
                role4.AddPermission(new Permission("ALL.ACCESS.ANY.ADVISEE"));
                Role role5 = new Role(1, "VIEW.ASSIGNED.ADVISEES");
                role5.AddPermission(new Permission("VIEW.ASSIGNED.ADVISEES"));
                Role role6 = new Role(1, "REVIEW.ASSIGNED.ADVISEES");
                role6.AddPermission(new Permission("REVIEW.ASSIGNED.ADVISEES"));
                Role role7 = new Role(1, "UPDATE.ASSIGNED.ADVISEES");
                role7.AddPermission(new Permission("UPDATE.ASSIGNED.ADVISEES"));
                Role role8 = new Role(1, "ALL.ACCESS.ASSIGNED.ADVISEES");
                role8.AddPermission(new Permission("ALL.ACCESS.ASSIGNED.ADVISEES"));

                roles = new List<Role>();
                roles.Add(role1);
                roles.Add(role2);
                roles.Add(role3);
                roles.Add(role4);
                roles.Add(role5);
                roles.Add(role6);
                roles.Add(role7);
                roles.Add(role8);
                roleRepoMock.Setup(r => r.GetRolesAsync()).ReturnsAsync(roles);

                // Set up adapter registry
                var dtoAdapter = new AutoMapperAdapter<Domain.Planning.Entities.AdvisingPermissions, Dtos.Planning.AdvisingPermissions>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Planning.Entities.AdvisingPermissions, Dtos.Planning.AdvisingPermissions>()).Returns(dtoAdapter);

                // Build coordination service
                advisorService = new AdvisorService(
                    adapterRegistry, advisorRepo, degreePlanRepo, termRepo, adviseeRepo, null, currentUserFactory,
                    roleRepo, logger, studentRepo, null, baseConfigurationRepository, personBaseRepo, studentDegreePlanRepo);
            }

            [TestMethod]
            public async Task AdvisorService_GetAdvisingPermissions2Async_Valid()
            {
                var permissions = await advisorService.GetAdvisingPermissions2Async();
                Assert.IsNotNull(permissions);
                Assert.IsInstanceOfType(permissions, typeof(Dtos.Planning.AdvisingPermissions));
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task AdvisorService_GetAdvisingPermissions2Async_catches_exception_and_throws_ApplicationException_CurrentUser_defined()
            {
                Exception thrownEx = new Exception("Error retrieving roles from database.");
                roleRepoMock.Setup(r => r.GetRolesAsync()).ThrowsAsync(thrownEx);

                // Build coordination service
                advisorService = new AdvisorService(
                    adapterRegistry, advisorRepo, degreePlanRepo, termRepo, adviseeRepo, null, currentUserFactory,
                    roleRepo, logger, studentRepo, null, baseConfigurationRepository, personBaseRepo, studentDegreePlanRepo);

                var permissions = await advisorService.GetAdvisingPermissions2Async();
                loggerMock.Verify(l => l.Error(thrownEx, "An error occurred while retrieving user " + currentUserFactory.CurrentUser.PersonId + "'s advising permissions."));
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task AdvisorService_GetAdvisingPermissions2Async_catches_exception_and_throws_ApplicationException_CurrentUser_null()
            {
                currentUserFactory = new CurrentUserSetup.NullUserFactory();

                Exception thrownEx = new Exception("Error retrieving roles from database.");
                roleRepoMock.Setup(r => r.GetRolesAsync()).ThrowsAsync(thrownEx);

                // Build coordination service
                advisorService = new AdvisorService(
                    adapterRegistry, advisorRepo, degreePlanRepo, termRepo, adviseeRepo, null, currentUserFactory,
                    roleRepo, logger, studentRepo, null, baseConfigurationRepository, personBaseRepo, studentDegreePlanRepo);

                var permissions = await advisorService.GetAdvisingPermissions2Async();
                loggerMock.Verify(l => l.Error(thrownEx, "An error occurred while retrieving advising permissions."));
            }
        }

        [TestClass]
        public class GetAdvisee : CurrentUserSetup
        {
            private Mock<IAdvisorRepository> advisorRepoMock;
            private IAdvisorRepository advisorRepo;
            private Mock<IDegreePlanRepository> degreePlanRepoMock;
            private IDegreePlanRepository degreePlanRepo;
            private Mock<IStudentDegreePlanRepository> studentDegreePlanRepoMock;
            private IStudentDegreePlanRepository studentDegreePlanRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IAdviseeRepository> adviseeRepoMock;
            private IAdviseeRepository adviseeRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private AdvisorService advisorService;
            private ILogger logger;
            private IStudentRepository studentRepo;
            private Mock<IStudentRepository> studentRepoMock;
            private Domain.Planning.Entities.Advisor advisor;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private Domain.Student.Entities.PlanningStudent student;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;
            private IPersonBaseRepository personBaseRepo;
            private Mock<IPersonBaseRepository> personBaseRepoMock;

            [TestInitialize]
            public void Initialize()
            {
                advisorRepoMock = new Mock<IAdvisorRepository>();
                advisorRepo = advisorRepoMock.Object;
                degreePlanRepoMock = new Mock<IDegreePlanRepository>();
                degreePlanRepo = degreePlanRepoMock.Object;
                studentDegreePlanRepoMock = new Mock<IStudentDegreePlanRepository>();
                studentDegreePlanRepo = studentDegreePlanRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                adviseeRepoMock = new Mock<IAdviseeRepository>();
                adviseeRepo = adviseeRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;
                personBaseRepoMock = new Mock<IPersonBaseRepository>();
                personBaseRepo = personBaseRepoMock.Object;

                // Mock advisor response from advisor Repository, including advisee ids.
                advisor = new Domain.Planning.Entities.Advisor("0000011", "Brown");
                advisor.IsActive = true;
                advisor.AddAdvisee("0000894");
                advisor.AddAdvisee("0004002");
                advisorRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>(), AdviseeInclusionType.AllAdvisees)).ReturnsAsync(advisor);

                // Mock student response from student Repository
                student = new Domain.Student.Entities.PlanningStudent("0000894", "Smith", 2, new List<string>() { "MATH.BA" });
                adviseeRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>())).ReturnsAsync(student);

                // Mock degreePlan response from Degree Plan Repository
                var degreePlan = new Domain.Student.Entities.DegreePlans.DegreePlan(2, "0000894", 1);
                studentDegreePlanRepoMock.Setup(repo => repo.GetAsync(It.IsAny<int>())).ReturnsAsync(degreePlan);

                // Mock Adapters
                var degreePlanDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan>()).Returns(degreePlanDtoAdapter);
                var adviseeDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.PlanningStudent, Dtos.Planning.Advisee>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.PlanningStudent, Dtos.Planning.Advisee>()).Returns(adviseeDtoAdapter);
                var advisorDtoAdapter = new AutoMapperAdapter<Domain.Planning.Entities.Advisor, Dtos.Planning.Advisor>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Planning.Entities.Advisor, Dtos.Planning.Advisor>()).Returns(advisorDtoAdapter);

                // Set up current user
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();

                advisorService = new AdvisorService(
                    adapterRegistry, advisorRepo, degreePlanRepo, termRepo, adviseeRepo, null, currentUserFactory,
                    roleRepo, logger, studentRepo, null, baseConfigurationRepository, personBaseRepo, studentDegreePlanRepo);
            }

            [TestCleanup]
            public void Cleanup()
            {
                degreePlanRepo = null;
                studentDegreePlanRepo = null;
                adapterRegistry = null;
                advisorRepo = null;
                termRepo = null;
                adviseeRepo = null;
                roleRepo = null;
                advisorService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ReturnsException_AdvisorIdNull()
            {
                var adviseeDto = await advisorService.GetAdviseeAsync(null, "0000894");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ReturnsException_AdviseeIdNull()
            {
                var adviseeDto = await advisorService.GetAdviseeAsync("0000011", null);
            }

            [TestMethod]
            public async Task ReturnsAdviseeDto_ViewAssigned_IsAssigned()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Act--get advisee
                var privacyWrapper = await advisorService.GetAdviseeAsync("0000011", "0000894");
                var adviseeDto = privacyWrapper.Dto as Advisee;

                // Assert
                Assert.IsTrue(adviseeDto is Dtos.Planning.Advisee);
                Assert.AreEqual(student.Id, adviseeDto.Id);
                Assert.AreEqual(student.LastName, adviseeDto.LastName);
            }

            [TestMethod]
            public async Task ReturnsAdviseeDto_ViewAssigned()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Act--get advisee
                var privacyWrapper = await advisorService.GetAdviseeAsync("0000011", "0000894");
                var adviseeDto = privacyWrapper.Dto as Advisee;
                // Assert
                Assert.IsTrue(adviseeDto is Dtos.Planning.Advisee);
                Assert.AreEqual(student.Id, adviseeDto.Id);
                Assert.AreEqual(student.LastName, adviseeDto.LastName);
                Assert.IsTrue(adviseeDto.IsAdvisee);
            }

            [TestMethod]
            public async Task ReturnsAdviseeDto_ViewAny()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Act--get advisee
                var privacyWrapper = await advisorService.GetAdviseeAsync("0000011", "0000894");
                var adviseeDto = privacyWrapper.Dto as Advisee;
                // Assert
                Assert.IsTrue(adviseeDto is Dtos.Planning.Advisee);
                Assert.AreEqual(student.Id, adviseeDto.Id);
                Assert.AreEqual(student.LastName, adviseeDto.LastName);
                Assert.IsTrue(adviseeDto.IsAdvisee);
            }

            [TestMethod]
            public async Task ReturnsAdviseeDto_ViewAny_NoDegreePlan()
            {
                // Mock student response from student Repository
                student = new Domain.Student.Entities.PlanningStudent("0000894", "Smith", null, new List<string>() { "MATH.BA" });
                adviseeRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>())).ReturnsAsync(student);
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Act--get advisee
                var privacyWrapper = await advisorService.GetAdviseeAsync("0000011", "0000894");
                var adviseeDto = privacyWrapper.Dto as Advisee;
                // Assert
                Assert.IsTrue(adviseeDto is Dtos.Planning.Advisee);
                Assert.AreEqual(student.Id, adviseeDto.Id);
                Assert.AreEqual(student.LastName, adviseeDto.LastName);
                Assert.IsTrue(adviseeDto.IsAdvisee);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task ReturnsAdviseeDto_NoPermission()
            {
                // Act--get advisee
                var adviseeDto = await advisorService.GetAdviseeAsync("0000011", "0000894");
            }

            [TestMethod]
            public async Task ReturnsAdviseeDto_ViewAny_NotAssigned()
            {
                // Mock advisor response from advisor Repository, including advisee ids.
                advisor = new Domain.Planning.Entities.Advisor("0000011", "Brown");
                advisor.IsActive = true;
                advisor.AddAdvisee("0004002");
                advisorRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>(), AdviseeInclusionType.AllAdvisees)).ReturnsAsync(advisor);
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Act--get advisee
                var privacyWrapper = await advisorService.GetAdviseeAsync("0000011", "0000894");
                var adviseeDto = privacyWrapper.Dto as Advisee;
                // Assert
                Assert.IsTrue(adviseeDto is Dtos.Planning.Advisee);
                Assert.AreEqual(student.Id, adviseeDto.Id);
                Assert.AreEqual(student.LastName, adviseeDto.LastName);
                Assert.IsFalse(adviseeDto.IsAdvisee);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task ReturnsAdviseeDto_ViewAssigned_NotAssigned()
            {
                // Mock advisor response from advisor Repository, including advisee ids.
                advisor = new Domain.Planning.Entities.Advisor("0000011", "Brown");
                advisor.IsActive = true;
                advisor.AddAdvisee("0004002");
                advisorRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>(), It.IsAny<AdviseeInclusionType>())).ReturnsAsync(advisor);
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Act--get advisee
                var adviseeDto = await advisorService.GetAdviseeAsync("0000011", "0000894");
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task AdvisorNoAdvisees_WithOutAnyPermissions_ThrowsException()
            {
                // Create active staff record
                var id = "0000011";
                var lastName = "Brown";
                var advisor = new Domain.Planning.Entities.Advisor(id, lastName);
                advisor.IsActive = true;
                // Set up needed permissions for advisor role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Mock thrown error from advisor Repository
                advisorRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>(), AdviseeInclusionType.AllAdvisees)).ReturnsAsync(advisor);
                // Act--GetAdvisee
                await advisorService.GetAdviseeAsync("0000011", "0000894");
            }

            [TestMethod]
            public async Task AdvisorNoAdvisees_HasAnyPermission_ReturnsResults()
            {
                // Create active staff record
                var id = "0000011";
                var lastName = "Brown";
                var advisor = new Domain.Planning.Entities.Advisor(id, lastName);
                advisor.IsActive = true;
                // Mock thrown error from advisor Repository
                advisorRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>(), AdviseeInclusionType.AllAdvisees)).ReturnsAsync(advisor);
                // Set up needed permissions for advisor role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Act--get advisee
                var privacyWrapper = await advisorService.GetAdviseeAsync("0000011", "0000894");
                var adviseeDto = privacyWrapper.Dto as Advisee;
                // Assert
                Assert.IsTrue(adviseeDto is Dtos.Planning.Advisee);
                Assert.AreEqual(student.Id, adviseeDto.Id);
                Assert.AreEqual(student.LastName, adviseeDto.LastName);
                Assert.IsFalse(adviseeDto.IsAdvisee);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task AdvisorNotCurrentUser_ThrowsException()
            {
                // Current User Id must match the Advisor Id in the request
                // Create active staff record
                var id = "12345";
                var lastName = "Smith";
                var advisor = new Domain.Planning.Entities.Advisor(id, lastName);
                advisor.IsActive = true;
                // Mock thrown error from advisor Repository
                advisorRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>(), AdviseeInclusionType.AllAdvisees)).ReturnsAsync(advisor);
                // Act--get advisor (current user is 0000011)
                var advisorDto = await advisorService.GetAdviseesAsync("12345", int.MaxValue, 1);
            }
        }

        [TestClass]
        public class Search3 : CurrentUserSetup
        {
            private Mock<IDegreePlanRepository> degreePlanRepoMock;
            private IDegreePlanRepository degreePlanRepo;
            private Mock<IStudentDegreePlanRepository> studentDegreePlanRepoMock;
            private IStudentDegreePlanRepository studentDegreePlanRepo;
            private Mock<IAdvisorRepository> advisorRepoMock;
            private IAdvisorRepository advisorRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IAdviseeRepository> adviseeRepoMock;
            private IAdviseeRepository adviseeRepo;
            private Mock<IStaffRepository> staffRepoMock;
            private IStaffRepository staffRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private AdvisorService advisorService;
            private ILogger logger;
            private IStudentRepository studentRepo;
            private Mock<IStudentRepository> studentRepoMock;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;
            private IPersonBaseRepository personBaseRepo;
            private Mock<IPersonBaseRepository> personBaseRepoMock;

            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private Domain.Planning.Entities.Advisor advisor;
            private Dtos.Planning.AdviseeSearchCriteria criteria;

            [TestInitialize]
            public void Initialize()
            {
                degreePlanRepoMock = new Mock<IDegreePlanRepository>();
                degreePlanRepo = degreePlanRepoMock.Object;
                studentDegreePlanRepoMock = new Mock<IStudentDegreePlanRepository>();
                studentDegreePlanRepo = studentDegreePlanRepoMock.Object;
                advisorRepoMock = new Mock<IAdvisorRepository>();
                advisorRepo = advisorRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                adviseeRepoMock = new Mock<IAdviseeRepository>();
                adviseeRepo = adviseeRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                logger = new Mock<ILogger>().Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                staffRepoMock = new Mock<IStaffRepository>();
                staffRepo = staffRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                // Set up current user
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;
                personBaseRepoMock = new Mock<IPersonBaseRepository>();
                personBaseRepo = personBaseRepoMock.Object;

                // Mock advisor response from advisor Repository, including advisee ids.
                advisor = new Domain.Planning.Entities.Advisor("0000011", "Brown");
                advisor.AddAdvisee("0000894");
                advisor.AddAdvisee("00004002");
                advisor.IsActive = true;
                advisorRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>(), AdviseeInclusionType.AllAdvisees)).ReturnsAsync(advisor);

                // Initialize the AdvisorSearchCriteria for all the existing search string tests
                criteria = new AdviseeSearchCriteria();


                // Mock AdviseeRepository Get
                var student1 = new Domain.Student.Entities.PlanningStudent("0000894", "Smith", 2, new List<string>() { "BA.ENGL" }) { FirstName = "Bob", MiddleName = "Blakely" };
                var student2 = new Domain.Student.Entities.PlanningStudent("00004002", "Jones", 802, new List<string>() { "BA.MATH" });
                IEnumerable<Domain.Student.Entities.PlanningStudent> students = new List<Domain.Student.Entities.PlanningStudent>() { student1, student2 };
                adviseeRepoMock.Setup(repo => repo.GetAsync(advisor.Advisees, 1, 1)).ReturnsAsync(students);

                // Mock Adapters
                var degreePlanDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan>()).Returns(degreePlanDtoAdapter);
                var adviseeDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.PlanningStudent, Dtos.Planning.Advisee>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.PlanningStudent, Dtos.Planning.Advisee>()).Returns(adviseeDtoAdapter);
                var advisorDtoAdapter = new AutoMapperAdapter<Domain.Planning.Entities.Advisor, Dtos.Planning.Advisor>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Planning.Entities.Advisor, Dtos.Planning.Advisor>()).Returns(advisorDtoAdapter);

                // Set up current user
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();

                advisorService = new AdvisorService(
                    adapterRegistry, advisorRepo, degreePlanRepo, termRepo, adviseeRepo, null, currentUserFactory,
                    roleRepo, logger, studentRepo, null, baseConfigurationRepository, personBaseRepo, studentDegreePlanRepo);
            }

            [TestCleanup]
            public void Cleanup()
            {
                degreePlanRepo = null;
                studentDegreePlanRepo = null;
                adapterRegistry = null;
                advisorRepo = null;
                termRepo = null;
                adviseeRepo = null;
                roleRepo = null;
                advisorService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task ThrowsErrorIfPermissionMissing()
            {
                criteria.AdviseeKeyword = "jones";
                await advisorService.Search3Async(criteria, 1, 1);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ThrowsErrorWhenCriteriaNull()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Act--search 
                await advisorService.Search3Async(null, 1, 1);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task ThrowsErrorWhenCriteriaHasNeitherKeyword()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Act--search
                AdviseeSearchCriteria emptycriteria = new AdviseeSearchCriteria();
                await advisorService.Search3Async(emptycriteria, 1, 1);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task ThrowsErrorWhenCriteriaHasBothKeywords()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Act--search
                AdviseeSearchCriteria badcriteria = new AdviseeSearchCriteria();
                badcriteria.AdviseeKeyword = "xxx";
                badcriteria.AdvisorKeyword = "yyy";
                await advisorService.Search3Async(badcriteria, 1, 1);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task ThrowsErrorWhenKeywordOnlyBlanks()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Act--search
                AdviseeSearchCriteria badcriteria = new AdviseeSearchCriteria();
                badcriteria.AdviseeKeyword = "   ";
                await advisorService.Search3Async(badcriteria, 1, 1);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task ThrowsErrorWhenAdviseeKeywordTooShort()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Act--search
                AdviseeSearchCriteria badcriteria = new AdviseeSearchCriteria();
                badcriteria.AdviseeKeyword = " x  ";
                await advisorService.Search3Async(badcriteria, 1, 1);
            }

            [TestMethod]
            public async Task SearchStringContainsOnlyOneString()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Setup callback
                string[] parsedStrings = null;
                adviseeRepoMock.Setup(repo => repo.SearchByNameAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                    .Callback<string, string, string, int, int, IEnumerable<string>>((string s1, string s2, string s3, int i1, int i2, IEnumerable<string> a) => parsedStrings = new string[] { s1, s2, s3 })
                    .ReturnsAsync(new List<Domain.Student.Entities.PlanningStudent>());
                // Act--search
                criteria.AdviseeKeyword = "jones";
                await advisorService.Search3Async(criteria, 1, 1);
                // Asserts
                Assert.AreEqual("jones", parsedStrings[0]);
                Assert.AreEqual(null, parsedStrings[1]);
                Assert.AreEqual(null, parsedStrings[2]);
            }

            [TestMethod]
            public async Task TwoStringsNoCommaParsedAsFirstLast()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Set up callback
                string[] parsedStrings = null;
                adviseeRepoMock.Setup(repo => repo.SearchByNameAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                    .Callback<string, string, string, int, int, IEnumerable<string>>((string s1, string s2, string s3, int i1, int i2, IEnumerable<string> a) => parsedStrings = new string[] { s1, s2, s3 })
                    .ReturnsAsync(new List<Domain.Student.Entities.PlanningStudent>());
                // Act--search
                criteria.AdviseeKeyword = "barney jones";
                await advisorService.Search3Async(criteria, 1, 1);
                // Assert--parsed strings
                Assert.AreEqual("jones", parsedStrings[0]); // Last Name
                Assert.AreEqual("barney", parsedStrings[1]); // First Name
                Assert.AreEqual(null, parsedStrings[2]); // Middle Name
            }

            [TestMethod]
            public async Task ThreeStringsNoCommaParsedAsFirstMiddleLast()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // set up callback
                string[] parsedStrings = null;
                adviseeRepoMock.Setup(repo => repo.SearchByNameAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                    .Callback<string, string, string, int, int, IEnumerable<string>>((string s1, string s2, string s3, int i1, int i2, IEnumerable<string> a) => parsedStrings = new string[] { s1, s2, s3 })
                    .ReturnsAsync(new List<Domain.Student.Entities.PlanningStudent>());
                // Act--search
                criteria.AdviseeKeyword = "barney a jones";
                await advisorService.Search3Async(criteria, 1, 1);
                // Assert--parsed strings
                Assert.AreEqual("jones", parsedStrings[0]); // Last Name
                Assert.AreEqual("barney", parsedStrings[1]); // First Name
                Assert.AreEqual("a", parsedStrings[2]); // Middle Name
            }

            [TestMethod]
            public async Task TwoStringsWithCommaParsedAsLastFirst()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Setup callback
                string[] parsedStrings = null;
                adviseeRepoMock.Setup(repo => repo.SearchByNameAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                    .Callback<string, string, string, int, int, IEnumerable<string>>((string s1, string s2, string s3, int i1, int i2, IEnumerable<string> a) => parsedStrings = new string[] { s1, s2, s3 })
                    .ReturnsAsync(new List<Domain.Student.Entities.PlanningStudent>());
                // Act--search
                criteria.AdviseeKeyword = "jones, barney";
                await advisorService.Search3Async(criteria, 1, 1);
                // Assert--parsed strings
                Assert.AreEqual("jones", parsedStrings[0]); // Last Name
                Assert.AreEqual("barney", parsedStrings[1]); // First Name
                Assert.AreEqual(null, parsedStrings[2]); // Middle Name
            }

            [TestMethod]
            public async Task ThreeStringsWithCommaParsedAsLastFirstMiddle()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // setup callback
                string[] parsedStrings = null;
                adviseeRepoMock.Setup(repo => repo.SearchByNameAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                    .Callback<string, string, string, int, int, IEnumerable<string>>((string s1, string s2, string s3, int i1, int i2, IEnumerable<string> a) => parsedStrings = new string[] { s1, s2, s3 })
                    .ReturnsAsync(new List<Domain.Student.Entities.PlanningStudent>());
                // Act--search
                criteria.AdviseeKeyword = "jones, barney a";
                await advisorService.Search3Async(criteria, 1, 1);
                // Assert--parsed strings
                Assert.AreEqual("jones", parsedStrings[0]); // Last Name
                Assert.AreEqual("barney", parsedStrings[1]); // First Name
                Assert.AreEqual("a", parsedStrings[2]); // Middle Name
            }

            [TestMethod]
            public async Task OneStringPrecededWithCommaParsesEmptyLastName()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Set up callback
                string[] parsedStrings = null;
                adviseeRepoMock.Setup(repo => repo.SearchByNameAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                    .Callback<string, string, string, int, int, IEnumerable<string>>((string s1, string s2, string s3, int i1, int i2, IEnumerable<string> a) => parsedStrings = new string[] { s1, s2, s3 })
                    .ReturnsAsync(new List<Domain.Student.Entities.PlanningStudent>());
                // Act--search
                criteria.AdviseeKeyword = " , jones";
                await advisorService.Search3Async(criteria, 1, 1);
                // Assert--parsed strings
                // An error will be thrown at the repository level
                Assert.IsTrue(string.IsNullOrEmpty(parsedStrings[0])); // Last name
            }

            [TestMethod]
            public async Task OneStringFollowedByCommaParsesOnlyLastName()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Setup callback
                string[] parsedStrings = null;
                adviseeRepoMock.Setup(repo => repo.SearchByNameAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                    .Callback<string, string, string, int, int, IEnumerable<string>>((string s1, string s2, string s3, int i1, int i2, IEnumerable<string> a) => parsedStrings = new string[] { s1, s2, s3 })
                    .ReturnsAsync(new List<Domain.Student.Entities.PlanningStudent>());
                // Act--search
                criteria.AdviseeKeyword = " jones ,  ";
                await advisorService.Search3Async(criteria, 1, 1);
                // Assert--parsed strings
                Assert.AreEqual("jones", parsedStrings[0]); // Last name
                Assert.IsTrue(string.IsNullOrEmpty(parsedStrings[1])); // Middle name
                Assert.IsTrue(string.IsNullOrEmpty(parsedStrings[2])); // Last name
            }

            [TestMethod]
            public async Task MultipleSpacesParsedOut()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // setup callback
                string[] parsedStrings = null;
                adviseeRepoMock.Setup(repo => repo.SearchByNameAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                    .Callback<string, string, string, int, int, IEnumerable<string>>((string s1, string s2, string s3, int i1, int i2, IEnumerable<string> a) => parsedStrings = new string[] { s1, s2, s3 })
                    .ReturnsAsync(new List<Domain.Student.Entities.PlanningStudent>());
                // Act--search
                criteria.AdviseeKeyword = " jones ,  joe   billy  ";
                await advisorService.Search3Async(criteria, 1, 1);
                // Assert--parsed strings
                Assert.AreEqual("jones", parsedStrings[0]); // Last name
                Assert.AreEqual("joe", parsedStrings[1]); // Middle name
                Assert.AreEqual("billy", parsedStrings[2]); // Last name
            }

            [TestMethod]
            public async Task ValidStudentIdReturnsId()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // setup returned student
                var student1 = new Domain.Student.Entities.PlanningStudent("0000894", "Smith", 2, new List<string>() { "BA.ENGL" }) { FirstName = "Bob", MiddleName = "Blakely" };
                adviseeRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);
                // Act--search
                criteria.AdviseeKeyword = "0000894";
                var privacyWrapper = await advisorService.Search3Async(criteria, 1, 1);
                var result = privacyWrapper.Dto as List<Advisee>;
                // Assert student returned
                var validStudent = result.ElementAt(0);
                Assert.AreEqual("0000894", validStudent.Id);
            }

            [TestMethod]
            public async Task InvalidStudentIdThrowsError()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // setup null student response
                Domain.Student.Entities.PlanningStudent student = null;
                adviseeRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>())).ReturnsAsync(student);
                // Act--search
                criteria.AdviseeKeyword = "9999999";
                var privacyWrapper = await advisorService.Search3Async(criteria, 1, 1);
                var result = privacyWrapper.Dto as List<Advisee>;
                // Assert--empty list returned
                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            public async Task RemovesPunctuationCharacters()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // setup callback
                string[] parsedStrings = null;
                adviseeRepoMock.Setup(repo => repo.SearchByNameAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                    .Callback<string, string, string, int, int, IEnumerable<string>>((string s1, string s2, string s3, int i1, int i2, IEnumerable<string> a) => parsedStrings = new string[] { s1, s2, s3 })
                    .ReturnsAsync(new List<Domain.Student.Entities.PlanningStudent>());
                // Act--search
                criteria.AdviseeKeyword = "james bing :;<.>?/1234567890]smith~!@#$%^&*()_+={}|";
                var privacyWrapper = await advisorService.Search3Async(criteria, 1, 1);
                var result = privacyWrapper.Dto as List<Advisee>;
                // Assert--punctuation removed from parsed values
                Assert.AreEqual("smith", parsedStrings[0]); // Last name
                Assert.AreEqual("james", parsedStrings[1]); // Middle name
                Assert.AreEqual("bing", parsedStrings[2]); // Last name
            }

            [TestMethod]
            public async Task RemovesSpaceInLastName()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // setup callback
                string[] parsedStrings = null;
                adviseeRepoMock.Setup(repo => repo.SearchByNameAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                    .Callback<string, string, string, int, int, IEnumerable<string>>((string s1, string s2, string s3, int i1, int i2, IEnumerable<string> a) => parsedStrings = new string[] { s1, s2, s3 })
                    .ReturnsAsync(new List<Domain.Student.Entities.PlanningStudent>());
                // Act--search
                criteria.AdviseeKeyword = ":;<.>?/1234567890]~!@#$%^&*()_+={}| van buren, james bing";
                var privacyWrapper = await advisorService.Search3Async(criteria, 1, 1);
                var result = privacyWrapper.Dto as List<Advisee>;
                Assert.AreEqual("vanburen", parsedStrings[0]); // Last name
                // Assert--space removed
                Assert.AreEqual("james", parsedStrings[1]); // Middle name
                Assert.AreEqual("bing", parsedStrings[2]); // Last name
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task AdvisorNoAdvisees_WithOutAnyPermissions_ThrowsException()
            {
                // Create active advisor record
                var id = "0000011";
                var lastName = "Brown";
                var advisor = new Domain.Planning.Entities.Advisor(id, lastName);
                advisor.IsActive = true;
                // Set up needed permissions for advisor role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Mock thrown error from advisor Repository
                advisorRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>(), It.IsAny<AdviseeInclusionType>())).ReturnsAsync(advisor);
                // Act--invoke search
                criteria.AdviseeKeyword = "jones";
                await advisorService.Search3Async(criteria, 1, 1);
            }

            [TestMethod]
            public async Task AdvisorNoAdvisees_WithAnyPermissions_ReturnsResults()
            {
                // Create active staff record
                var id = "0000011";
                var lastName = "Brown";
                var advisor = new Domain.Planning.Entities.Advisor(id, lastName);
                advisor.IsActive = true;
                // Mock advisor Repository
                advisorRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>(), It.IsAny<AdviseeInclusionType>())).ReturnsAsync(advisor);
                // Set up needed permissions for advisor role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Mock search response
                var student1 = new Domain.Student.Entities.PlanningStudent("0000894", "Smith", 2, new List<string>() { "BA.ENGL" }) { FirstName = "Bob", MiddleName = "Blakely" };
                adviseeRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);
                // Act--search
                criteria.AdviseeKeyword = "0000894";
                var privacyWrapper = await advisorService.Search3Async(criteria, 1, 1);
                var result = privacyWrapper.Dto as List<Advisee>;
                // Assert
                Assert.AreEqual(1, result.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task Advisor_NotPerson_ThrowsException()
            {
                // Mock thrown error from advisor Repository
                advisorRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>(), It.IsAny<AdviseeInclusionType>())).ThrowsAsync(new KeyNotFoundException());
                // Set up needed permissions for advisor role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Mock search response
                var student1 = new Domain.Student.Entities.PlanningStudent("0000894", "Smith", 2, new List<string>() { "BA.ENGL" }) { FirstName = "Bob", MiddleName = "Blakely" };
                adviseeRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);
                // Act--search
                criteria.AdviseeKeyword = "0000894";
                var result = await advisorService.Search3Async(criteria, 1, 1);
            }

            // Additional AdvisorKeyword Tests
            // Since parsing was fully tested with the advisee search string just add a couple test to be sure advisorKeyword gets passed and parsed properly

            [TestMethod]
            public async Task AdvisorSearchStringContainsOnlyOneString()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Setup callback
                string[] parsedStrings = null;
                advisorRepoMock.Setup(repo => repo.SearchAdvisorByNameAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Callback<string, string, string>((string s1, string s2, string s3) => parsedStrings = new string[] { s1, s2, s3 })
                    .ReturnsAsync(new List<string>());
                // Act--search
                criteria.AdvisorKeyword = "jones";
                await advisorService.Search3Async(criteria, 1, 1);
                // Asserts
                Assert.AreEqual("jones", parsedStrings[0]);
                Assert.AreEqual(null, parsedStrings[1]);
                Assert.AreEqual(null, parsedStrings[2]);
            }

            [TestMethod]
            public async Task AdvisorSearchStringContainsAnId_AnyAdvisee()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Mock advisee repo mock of SearchByAdvisorIds
                var student1 = new Domain.Student.Entities.PlanningStudent("0000894", "Smith", 2, new List<string>() { "BA.ENGL" }) { FirstName = "Bob", MiddleName = "Blakely" };
                var student2 = new Domain.Student.Entities.PlanningStudent("00004002", "Jones", 802, new List<string>() { "BA.MATH" });
                IEnumerable<Domain.Student.Entities.PlanningStudent> students = new List<Domain.Student.Entities.PlanningStudent>() { student1, student2 };
                adviseeRepoMock.Setup(repo => repo.SearchByAdvisorIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<int>(), It.IsAny<int>(), null)).ReturnsAsync(students);
                // Act--search - Id doesn't matter since the repo will return 2 students
                criteria.AdvisorKeyword = "0009999";
                var privacyWrapper = await advisorService.Search3Async(criteria, 1, 1);
                var advisees = privacyWrapper.Dto as List<Advisee>;
                // Asserts
                Assert.AreEqual(2, advisees.Count());
            }

            [TestMethod]
            public async Task AdvisorSearchStringContainsName_AssignedAdvisee()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Mock advisee repo mock of SearchByAdvisorIds
                var student1 = new Domain.Student.Entities.PlanningStudent("0000894", "Smith", 2, new List<string>() { "BA.ENGL" }) { FirstName = "Bob", MiddleName = "Blakely" };
                var student2 = new Domain.Student.Entities.PlanningStudent("00004002", "Jones", 802, new List<string>() { "BA.MATH" });
                IEnumerable<Domain.Student.Entities.PlanningStudent> students = new List<Domain.Student.Entities.PlanningStudent>() { student1, student2 };
                adviseeRepoMock.Setup(repo => repo.SearchByAdvisorIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>())).ReturnsAsync(students);
                // Act--search - Id doesn't matter since the repo will return 2 students
                criteria.AdvisorKeyword = "Jones";
                var privacyWrapper = await advisorService.Search3Async(criteria, 1, 1);
                var advisees = privacyWrapper.Dto as List<Advisee>;
                // Asserts
                Assert.AreEqual(2, advisees.Count());
            }
        }

        [TestClass]
        public class PostCompletedAdvisementAsync : CurrentUserSetup
        {
            private Mock<IAdvisorRepository> advisorRepoMock;
            private IAdvisorRepository advisorRepo;
            private Mock<IDegreePlanRepository> degreePlanRepoMock;
            private IDegreePlanRepository degreePlanRepo;
            private Mock<IStudentDegreePlanRepository> studentDegreePlanRepoMock;
            private IStudentDegreePlanRepository studentDegreePlanRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IAdviseeRepository> adviseeRepoMock;
            private IAdviseeRepository adviseeRepo;
            private Mock<IStudentConfigurationRepository> configRepoMock;
            private IStudentConfigurationRepository configRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private AdvisorService advisorService;
            private ILogger logger;
            private Domain.Planning.Entities.Advisor advisor;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private StudentConfiguration config;
            private IStudentRepository studentRepo;
            private Mock<IStudentRepository> studentRepoMock;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;
            private IPersonBaseRepository personBaseRepo;
            private Mock<IPersonBaseRepository> personBaseRepoMock;

            private string studentId;
            private Dtos.Student.CompletedAdvisement completedAdvisement;
            private Domain.Student.Entities.Student student;
            private StudentAccess studentAccess;
            private IEnumerable<StudentAccess> studentAccesses;
            private Role role;
            private IEnumerable<Role> roles;
            private Domain.Planning.Entities.Advisor advisorEntity;
            private Domain.Student.Entities.PlanningStudent planningStudentEntity;
            private DateTime today = DateTime.Today;
            private DateTimeOffset now = DateTimeOffset.Now;

            [TestInitialize]
            public void Initialize()
            {
                advisorRepoMock = new Mock<IAdvisorRepository>();
                advisorRepo = advisorRepoMock.Object;
                degreePlanRepoMock = new Mock<IDegreePlanRepository>();
                degreePlanRepo = degreePlanRepoMock.Object;
                studentDegreePlanRepoMock = new Mock<IStudentDegreePlanRepository>();
                studentDegreePlanRepo = studentDegreePlanRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                adviseeRepoMock = new Mock<IAdviseeRepository>();
                adviseeRepo = adviseeRepoMock.Object;
                configRepoMock = new Mock<IStudentConfigurationRepository>();
                configRepo = configRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;
                personBaseRepoMock = new Mock<IPersonBaseRepository>();
                personBaseRepo = personBaseRepoMock.Object;


                var adviseeDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.PlanningStudent, Dtos.Planning.Advisee>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.PlanningStudent, Dtos.Planning.Advisee>()).Returns(adviseeDtoAdapter);

                var planningStudentEntityToAdviseeDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.CompletedAdvisement, Dtos.Student.CompletedAdvisement>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.CompletedAdvisement, Dtos.Student.CompletedAdvisement>()).Returns(planningStudentEntityToAdviseeDtoAdapter);

                // Set up current user
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();

                // Data setup
                studentId = "0001234";
                completedAdvisement = new Dtos.Student.CompletedAdvisement()
                {
                    AdvisorId = currentUserFactory.CurrentUser.PersonId,
                    CompletionDate = DateTime.Today,
                    CompletionTime = DateTime.Now
                };
                student = new Domain.Student.Entities.Student(studentId, "Smith", null, null, null, null);
                studentAccess = new StudentAccess(student.Id);
                studentAccess.AddAdvisement(currentUserFactory.CurrentUser.PersonId, DateTime.Today.AddDays(-30), DateTime.Today.AddDays(30), "ABC");
                studentAccesses = new List<StudentAccess>()
                {
                   studentAccess
                };
                role = new Role(0, currentUserFactory.CurrentUser.Roles.FirstOrDefault());
                role.AddPermission(new Permission(PlanningPermissionCodes.AllAccessAnyAdvisee));
                role.AddPermission(new Permission(PlanningPermissionCodes.UpdateAnyAdvisee));
                role.AddPermission(new Permission(PlanningPermissionCodes.ReviewAnyAdvisee));
                role.AddPermission(new Permission(PlanningPermissionCodes.AllAccessAssignedAdvisees));
                role.AddPermission(new Permission(PlanningPermissionCodes.UpdateAssignedAdvisees));
                role.AddPermission(new Permission(PlanningPermissionCodes.ReviewAssignedAdvisees));
                roles = new List<Role>()
                {
                    role
                };
                advisorEntity = new Domain.Planning.Entities.Advisor(currentUserFactory.CurrentUser.PersonId, "Smith") { IsActive = true };
                advisorEntity.AddAdvisee(studentId);
                planningStudentEntity = new Domain.Student.Entities.PlanningStudent(studentId, "Smith", null, null, null) { FirstName = "John", MiddleName = "Patrick" };
                planningStudentEntity.AddCompletedAdvisement(currentUserFactory.CurrentUser.PersonId, today, now);

                // Repository setup
                studentRepoMock.Setup(repo => repo.GetAsync(student.Id)).ReturnsAsync(student);
                studentRepoMock.Setup(repo => repo.GetStudentAccessAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(studentAccesses);
                roleRepoMock.Setup(repo => repo.GetRolesAsync()).ReturnsAsync(roles);
                advisorRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>(), It.IsAny<AdviseeInclusionType>())).ReturnsAsync(advisorEntity);
                adviseeRepoMock.Setup(repo => repo.PostCompletedAdvisementAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTimeOffset>(), It.IsAny<string>())).ReturnsAsync(planningStudentEntity);

                advisorService = new AdvisorService(
                    adapterRegistry, advisorRepo, degreePlanRepo, termRepo, adviseeRepo, configRepo, currentUserFactory,
                    roleRepo, logger, studentRepo, null, baseConfigurationRepository, personBaseRepo, studentDegreePlanRepo);
            }

            [TestCleanup]
            public void Cleanup()
            {
                degreePlanRepo = null;
                studentDegreePlanRepo = null;
                adapterRegistry = null;
                advisorRepo = null;
                termRepo = null;
                adviseeRepo = null;
                roleRepo = null;
                advisorService = null;
                advisorRole = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PostCompletedAdvisementAsync_Null_StudentId()
            {
                var advisee = await advisorService.PostCompletedAdvisementAsync(null, completedAdvisement);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PostCompletedAdvisementAsync_Null_CompletedAdvisement()
            {
                var advisee = await advisorService.PostCompletedAdvisementAsync(studentId, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PostCompletedAdvisementAsync_Null_AdvisorId()
            {
                completedAdvisement.AdvisorId = null;
                var advisee = await advisorService.PostCompletedAdvisementAsync(studentId, completedAdvisement);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task PostCompletedAdvisementAsync_same_StudentId_and_AdvisorId()
            {
                var advisee = await advisorService.PostCompletedAdvisementAsync(completedAdvisement.AdvisorId, completedAdvisement);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task PostCompletedAdvisementAsync_CurrentUser_PersonId_does_not_match_AdvisorId()
            {
                completedAdvisement.AdvisorId = "0001235";
                var advisee = await advisorService.PostCompletedAdvisementAsync(studentId, completedAdvisement);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task PostCompletedAdvisementAsync_StudentRepository_GetAsync_Returns_Null()
            {
                studentRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>())).ReturnsAsync(() => null);
                advisorService = new AdvisorService(
                    adapterRegistry, advisorRepo, degreePlanRepo, termRepo, adviseeRepo, configRepo, currentUserFactory,
                    roleRepo, logger, studentRepo, null, baseConfigurationRepository, personBaseRepo, studentDegreePlanRepo);

                var advisee = await advisorService.PostCompletedAdvisementAsync(studentId, completedAdvisement);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task PostCompletedAdvisementAsync_CurrentUser_does_not_have_Permission()
            {
                role = new Role(0, currentUserFactory.CurrentUser.Roles.FirstOrDefault());
                roles = new List<Role>()
                {
                    role
                };
                roleRepoMock.Setup(repo => repo.GetRolesAsync()).ReturnsAsync(roles);
                advisorService = new AdvisorService(
                    adapterRegistry, advisorRepo, degreePlanRepo, termRepo, adviseeRepo, configRepo, currentUserFactory,
                    roleRepo, logger, studentRepo, null, baseConfigurationRepository, personBaseRepo, studentDegreePlanRepo);

                var advisee = await advisorService.PostCompletedAdvisementAsync(studentId, completedAdvisement);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task PostCompletedAdvisementAsync_AdviseeRepository_Returns_Null()
            {
                adviseeRepoMock.Setup(repo => repo.PostCompletedAdvisementAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTimeOffset>(), It.IsAny<string>())).ReturnsAsync(() => null);
                advisorService = new AdvisorService(
                    adapterRegistry, advisorRepo, degreePlanRepo, termRepo, adviseeRepo, configRepo, currentUserFactory,
                    roleRepo, logger, studentRepo, null, baseConfigurationRepository, personBaseRepo, studentDegreePlanRepo);

                var advisee = await advisorService.PostCompletedAdvisementAsync(studentId, completedAdvisement);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task PostCompletedAdvisementAsync_AdvisorRepository_Throws_KeyNotFoundException()
            {
                advisorRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>(), It.IsAny<AdviseeInclusionType>())).ThrowsAsync(new KeyNotFoundException());
                advisorService = new AdvisorService(
                    adapterRegistry, advisorRepo, degreePlanRepo, termRepo, adviseeRepo, configRepo, currentUserFactory,
                    roleRepo, logger, studentRepo, null, baseConfigurationRepository, personBaseRepo, studentDegreePlanRepo);

                var advisee = await advisorService.PostCompletedAdvisementAsync(studentId, completedAdvisement);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task PostCompletedAdvisementAsync_Advisor_not_Active()
            {
                advisorEntity.IsActive = false;
                advisorRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>(), It.IsAny<AdviseeInclusionType>())).ReturnsAsync(advisorEntity);
                advisorService = new AdvisorService(
                    adapterRegistry, advisorRepo, degreePlanRepo, termRepo, adviseeRepo, configRepo, currentUserFactory,
                    roleRepo, logger, studentRepo, null, baseConfigurationRepository, personBaseRepo, studentDegreePlanRepo);

                var advisee = await advisorService.PostCompletedAdvisementAsync(studentId, completedAdvisement);
            }

            [TestMethod]
            public async Task PostCompletedAdvisementAsync_CurrentUser_has_Permission()
            {
                var advisee = await advisorService.PostCompletedAdvisementAsync(studentId, completedAdvisement);
                Assert.IsNotNull(advisee);
                Assert.AreEqual(planningStudentEntity.CompletedAdvisements.Count, advisee.Dto.CompletedAdvisements.Count());
            }
        }

        [TestClass]
        public class SearchForExactMatch : CurrentUserSetup
        {
            private Mock<IDegreePlanRepository> degreePlanRepoMock;
            private IDegreePlanRepository degreePlanRepo;
            private Mock<IStudentDegreePlanRepository> studentDegreePlanRepoMock;
            private IStudentDegreePlanRepository studentDegreePlanRepo;
            private Mock<IAdvisorRepository> advisorRepoMock;
            private IAdvisorRepository advisorRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IAdviseeRepository> adviseeRepoMock;
            private IAdviseeRepository adviseeRepo;
            private Mock<IStaffRepository> staffRepoMock;
            private IStaffRepository staffRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private AdvisorService advisorService;
            private ILogger logger;
            private IStudentRepository studentRepo;
            private Mock<IStudentRepository> studentRepoMock;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;
            private IPersonBaseRepository personBaseRepo;
            private Mock<IPersonBaseRepository> personBaseRepoMock;

            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private Domain.Planning.Entities.Advisor advisor;
            private Dtos.Planning.AdviseeSearchCriteria criteria;

            [TestInitialize]
            public void Initialize()
            {
                degreePlanRepoMock = new Mock<IDegreePlanRepository>();
                degreePlanRepo = degreePlanRepoMock.Object;
                studentDegreePlanRepoMock = new Mock<IStudentDegreePlanRepository>();
                studentDegreePlanRepo = studentDegreePlanRepoMock.Object;
                advisorRepoMock = new Mock<IAdvisorRepository>();
                advisorRepo = advisorRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                adviseeRepoMock = new Mock<IAdviseeRepository>();
                adviseeRepo = adviseeRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                logger = new Mock<ILogger>().Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                staffRepoMock = new Mock<IStaffRepository>();
                staffRepo = staffRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                // Set up current user
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;
                personBaseRepoMock = new Mock<IPersonBaseRepository>();
                personBaseRepo = personBaseRepoMock.Object;

                // Mock advisor response from advisor Repository, including advisee ids.
                advisor = new Domain.Planning.Entities.Advisor("0000011", "Brown");
                advisor.AddAdvisee("0000894");
                advisor.AddAdvisee("00004002");
                advisor.IsActive = true;
                advisorRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>(), AdviseeInclusionType.AllAdvisees)).ReturnsAsync(advisor);

                // Initialize the AdvisorSearchCriteria for all the existing search string tests
                criteria = new AdviseeSearchCriteria();


                // Mock AdviseeRepository Get
                var student1 = new Domain.Student.Entities.PlanningStudent("0000894", "Smith", 2, new List<string>() { "BA.ENGL" }) { FirstName = "Bob", MiddleName = "Blakely" };
                var student2 = new Domain.Student.Entities.PlanningStudent("00004002", "Jones", 802, new List<string>() { "BA.MATH" });
                IEnumerable<Domain.Student.Entities.PlanningStudent> students = new List<Domain.Student.Entities.PlanningStudent>() { student1, student2 };
                adviseeRepoMock.Setup(repo => repo.GetAsync(advisor.Advisees, 1, 1)).ReturnsAsync(students);

                // Mock Adapters
                var degreePlanDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan>()).Returns(degreePlanDtoAdapter);
                var adviseeDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.PlanningStudent, Dtos.Planning.Advisee>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.PlanningStudent, Dtos.Planning.Advisee>()).Returns(adviseeDtoAdapter);
                var advisorDtoAdapter = new AutoMapperAdapter<Domain.Planning.Entities.Advisor, Dtos.Planning.Advisor>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Planning.Entities.Advisor, Dtos.Planning.Advisor>()).Returns(advisorDtoAdapter);

                // Set up current user
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();

                advisorService = new AdvisorService(
                    adapterRegistry, advisorRepo, degreePlanRepo, termRepo, adviseeRepo, null, currentUserFactory,
                    roleRepo, logger, studentRepo, null, baseConfigurationRepository, personBaseRepo, studentDegreePlanRepo);
            }

            [TestCleanup]
            public void Cleanup()
            {
                degreePlanRepo = null;
                studentDegreePlanRepo = null;
                adapterRegistry = null;
                advisorRepo = null;
                termRepo = null;
                adviseeRepo = null;
                roleRepo = null;
                advisorService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task ThrowsErrorIfPermissionMissing()
            {
                criteria.AdviseeKeyword = "jones";
                await advisorService.SearchForExactMatchAsync(criteria, 1, 1);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ThrowsErrorWhenCriteriaNull()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Act--search 
                await advisorService.SearchForExactMatchAsync(null, 1, 1);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task ThrowsErrorWhenCriteriaHasNeitherKeyword()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Act--search
                AdviseeSearchCriteria emptycriteria = new AdviseeSearchCriteria();
                await advisorService.SearchForExactMatchAsync(emptycriteria, 1, 1);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task ThrowsErrorWhenCriteriaHasBothKeywords()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Act--search
                AdviseeSearchCriteria badcriteria = new AdviseeSearchCriteria();
                badcriteria.AdviseeKeyword = "xxx";
                badcriteria.AdvisorKeyword = "yyy";
                await advisorService.SearchForExactMatchAsync(badcriteria, 1, 1);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task ThrowsErrorWhenKeywordOnlyBlanks()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Act--search
                AdviseeSearchCriteria badcriteria = new AdviseeSearchCriteria();
                badcriteria.AdviseeKeyword = "   ";
                await advisorService.SearchForExactMatchAsync(badcriteria, 1, 1);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task ThrowsErrorWhenAdviseeKeywordTooShort()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Act--search
                AdviseeSearchCriteria badcriteria = new AdviseeSearchCriteria();
                badcriteria.AdviseeKeyword = " x  ";
                await advisorService.SearchForExactMatchAsync(badcriteria, 1, 1);
            }

            [TestMethod]
            public async Task SearchStringContainsOnlyOneString()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Setup callback
                string[] parsedStrings = null;
                adviseeRepoMock.Setup(repo => repo.SearchByNameForExactMatchAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                    .Callback<string, string, string, int, int, IEnumerable<string>>((string s1, string s2, string s3, int i1, int i2, IEnumerable<string> a) => parsedStrings = new string[] { s1, s2, s3 })
                    .ReturnsAsync(new List<Domain.Student.Entities.PlanningStudent>());
                // Act--search
                criteria.AdviseeKeyword = "jones";
                await advisorService.SearchForExactMatchAsync(criteria, 1, 1);
                // Asserts
                Assert.AreEqual("jones", parsedStrings[0]);
                Assert.AreEqual(null, parsedStrings[1]);
                Assert.AreEqual(null, parsedStrings[2]);
            }

            [TestMethod]
            public async Task TwoStringsNoCommaParsedAsFirstLast()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Set up callback
                string[] parsedStrings = null;
                adviseeRepoMock.Setup(repo => repo.SearchByNameForExactMatchAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                    .Callback<string, string, string, int, int, IEnumerable<string>>((string s1, string s2, string s3, int i1, int i2, IEnumerable<string> a) => parsedStrings = new string[] { s1, s2, s3 })
                    .ReturnsAsync(new List<Domain.Student.Entities.PlanningStudent>());
                // Act--search
                criteria.AdviseeKeyword = "barney jones";
                await advisorService.SearchForExactMatchAsync(criteria, 1, 1);
                // Assert--parsed strings
                Assert.AreEqual("jones", parsedStrings[0]); // Last Name
                Assert.AreEqual("barney", parsedStrings[1]); // First Name
                Assert.AreEqual(null, parsedStrings[2]); // Middle Name
            }

            [TestMethod]
            public async Task ThreeStringsNoCommaParsedAsFirstMiddleLast()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // set up callback
                string[] parsedStrings = null;
                adviseeRepoMock.Setup(repo => repo.SearchByNameForExactMatchAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                    .Callback<string, string, string, int, int, IEnumerable<string>>((string s1, string s2, string s3, int i1, int i2, IEnumerable<string> a) => parsedStrings = new string[] { s1, s2, s3 })
                    .ReturnsAsync(new List<Domain.Student.Entities.PlanningStudent>());
                // Act--search
                criteria.AdviseeKeyword = "barney a jones";
                await advisorService.SearchForExactMatchAsync(criteria, 1, 1);
                // Assert--parsed strings
                Assert.AreEqual("jones", parsedStrings[0]); // Last Name
                Assert.AreEqual("barney", parsedStrings[1]); // First Name
                Assert.AreEqual("a", parsedStrings[2]); // Middle Name
            }

            [TestMethod]
            public async Task TwoStringsWithCommaParsedAsLastFirst()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Setup callback
                string[] parsedStrings = null;
                adviseeRepoMock.Setup(repo => repo.SearchByNameForExactMatchAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                    .Callback<string, string, string, int, int, IEnumerable<string>>((string s1, string s2, string s3, int i1, int i2, IEnumerable<string> a) => parsedStrings = new string[] { s1, s2, s3 })
                    .ReturnsAsync(new List<Domain.Student.Entities.PlanningStudent>());
                // Act--search
                criteria.AdviseeKeyword = "jones, barney";
                await advisorService.SearchForExactMatchAsync(criteria, 1, 1);
                // Assert--parsed strings
                Assert.AreEqual("jones", parsedStrings[0]); // Last Name
                Assert.AreEqual("barney", parsedStrings[1]); // First Name
                Assert.AreEqual(null, parsedStrings[2]); // Middle Name
            }

            [TestMethod]
            public async Task ThreeStringsWithCommaParsedAsLastFirstMiddle()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // setup callback
                string[] parsedStrings = null;
                adviseeRepoMock.Setup(repo => repo.SearchByNameForExactMatchAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                    .Callback<string, string, string, int, int, IEnumerable<string>>((string s1, string s2, string s3, int i1, int i2, IEnumerable<string> a) => parsedStrings = new string[] { s1, s2, s3 })
                    .ReturnsAsync(new List<Domain.Student.Entities.PlanningStudent>());
                // Act--search
                criteria.AdviseeKeyword = "jones, barney a";
                await advisorService.SearchForExactMatchAsync(criteria, 1, 1);
                // Assert--parsed strings
                Assert.AreEqual("jones", parsedStrings[0]); // Last Name
                Assert.AreEqual("barney", parsedStrings[1]); // First Name
                Assert.AreEqual("a", parsedStrings[2]); // Middle Name
            }

            [TestMethod]
            public async Task OneStringPrecededWithCommaParsesEmptyLastName()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Set up callback
                string[] parsedStrings = null;
                adviseeRepoMock.Setup(repo => repo.SearchByNameForExactMatchAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                    .Callback<string, string, string, int, int, IEnumerable<string>>((string s1, string s2, string s3, int i1, int i2, IEnumerable<string> a) => parsedStrings = new string[] { s1, s2, s3 })
                    .ReturnsAsync(new List<Domain.Student.Entities.PlanningStudent>());
                // Act--search
                criteria.AdviseeKeyword = " , jones";
                await advisorService.SearchForExactMatchAsync(criteria, 1, 1);
                // Assert--parsed strings
                // An error will be thrown at the repository level
                Assert.IsTrue(string.IsNullOrEmpty(parsedStrings[0])); // Last name
            }

            [TestMethod]
            public async Task OneStringFollowedByCommaParsesOnlyLastName()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Setup callback
                string[] parsedStrings = null;
                adviseeRepoMock.Setup(repo => repo.SearchByNameForExactMatchAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                    .Callback<string, string, string, int, int, IEnumerable<string>>((string s1, string s2, string s3, int i1, int i2, IEnumerable<string> a) => parsedStrings = new string[] { s1, s2, s3 })
                    .ReturnsAsync(new List<Domain.Student.Entities.PlanningStudent>());
                // Act--search
                criteria.AdviseeKeyword = " jones ,  ";
                await advisorService.SearchForExactMatchAsync(criteria, 1, 1);
                // Assert--parsed strings
                Assert.AreEqual("jones", parsedStrings[0]); // Last name
                Assert.IsTrue(string.IsNullOrEmpty(parsedStrings[1])); // Middle name
                Assert.IsTrue(string.IsNullOrEmpty(parsedStrings[2])); // Last name
            }

            [TestMethod]
            public async Task MultipleSpacesParsedOut()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // setup callback
                string[] parsedStrings = null;
                adviseeRepoMock.Setup(repo => repo.SearchByNameForExactMatchAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                    .Callback<string, string, string, int, int, IEnumerable<string>>((string s1, string s2, string s3, int i1, int i2, IEnumerable<string> a) => parsedStrings = new string[] { s1, s2, s3 })
                    .ReturnsAsync(new List<Domain.Student.Entities.PlanningStudent>());
                // Act--search
                criteria.AdviseeKeyword = " jones ,  joe   billy  ";
                await advisorService.SearchForExactMatchAsync(criteria, 1, 1);
                // Assert--parsed strings
                Assert.AreEqual("jones", parsedStrings[0]); // Last name
                Assert.AreEqual("joe", parsedStrings[1]); // Middle name
                Assert.AreEqual("billy", parsedStrings[2]); // Last name
            }

            [TestMethod]
            public async Task ValidStudentIdReturnsId()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // setup returned student
                var student1 = new Domain.Student.Entities.PlanningStudent("0000894", "Smith", 2, new List<string>() { "BA.ENGL" }) { FirstName = "Bob", MiddleName = "Blakely" };
                adviseeRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);
                // Act--search
                criteria.AdviseeKeyword = "0000894";
                var privacyWrapper = await advisorService.SearchForExactMatchAsync(criteria, 1, 1);
                var result = privacyWrapper.Dto as List<Advisee>;
                // Assert student returned
                var validStudent = result.ElementAt(0);
                Assert.AreEqual("0000894", validStudent.Id);
            }

            [TestMethod]
            public async Task InvalidStudentIdThrowsError()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // setup null student response
                Domain.Student.Entities.PlanningStudent student = null;
                adviseeRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>())).ReturnsAsync(student);
                // Act--search
                criteria.AdviseeKeyword = "9999999";
                var privacyWrapper = await advisorService.SearchForExactMatchAsync(criteria, 1, 1);
                var result = privacyWrapper.Dto as List<Advisee>;
                // Assert--empty list returned
                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            public async Task RemovesPunctuationCharacters()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // setup callback
                string[] parsedStrings = null;
                adviseeRepoMock.Setup(repo => repo.SearchByNameForExactMatchAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                    .Callback<string, string, string, int, int, IEnumerable<string>>((string s1, string s2, string s3, int i1, int i2, IEnumerable<string> a) => parsedStrings = new string[] { s1, s2, s3 })
                    .ReturnsAsync(new List<Domain.Student.Entities.PlanningStudent>());
                // Act--search
                criteria.AdviseeKeyword = "james bing :;<.>?/1234567890]smith~!@#$%^&*()_+={}|";
                var privacyWrapper = await advisorService.SearchForExactMatchAsync(criteria, 1, 1);
                var result = privacyWrapper.Dto as List<Advisee>;
                // Assert--punctuation removed from parsed values
                Assert.AreEqual("smith", parsedStrings[0]); // Last name
                Assert.AreEqual("james", parsedStrings[1]); // Middle name
                Assert.AreEqual("bing", parsedStrings[2]); // Last name
            }

            [TestMethod]
            public async Task RemovesSpaceInLastName()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // setup callback
                string[] parsedStrings = null;
                adviseeRepoMock.Setup(repo => repo.SearchByNameForExactMatchAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                    .Callback<string, string, string, int, int, IEnumerable<string>>((string s1, string s2, string s3, int i1, int i2, IEnumerable<string> a) => parsedStrings = new string[] { s1, s2, s3 })
                    .ReturnsAsync(new List<Domain.Student.Entities.PlanningStudent>());
                // Act--search
                criteria.AdviseeKeyword = ":;<.>?/1234567890]~!@#$%^&*()_+={}| van buren, james bing";
                var privacyWrapper = await advisorService.SearchForExactMatchAsync(criteria, 1, 1);
                var result = privacyWrapper.Dto as List<Advisee>;
                Assert.AreEqual("vanburen", parsedStrings[0]); // Last name
                // Assert--space removed
                Assert.AreEqual("james", parsedStrings[1]); // Middle name
                Assert.AreEqual("bing", parsedStrings[2]); // Last name
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task AdvisorNoAdvisees_WithOutAnyPermissions_ThrowsException()
            {
                // Create active advisor record
                var id = "0000011";
                var lastName = "Brown";
                var advisor = new Domain.Planning.Entities.Advisor(id, lastName);
                advisor.IsActive = true;
                // Set up needed permissions for advisor role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Mock thrown error from advisor Repository
                advisorRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>(), It.IsAny<AdviseeInclusionType>())).ReturnsAsync(advisor);
                // Act--invoke search
                criteria.AdviseeKeyword = "jones";
                await advisorService.SearchForExactMatchAsync(criteria, 1, 1);
            }

            [TestMethod]
            public async Task AdvisorNoAdvisees_WithAnyPermissions_ReturnsResults()
            {
                // Create active staff record
                var id = "0000011";
                var lastName = "Brown";
                var advisor = new Domain.Planning.Entities.Advisor(id, lastName);
                advisor.IsActive = true;
                // Mock advisor Repository
                advisorRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>(), It.IsAny<AdviseeInclusionType>())).ReturnsAsync(advisor);
                // Set up needed permissions for advisor role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Mock search response
                var student1 = new Domain.Student.Entities.PlanningStudent("0000894", "Smith", 2, new List<string>() { "BA.ENGL" }) { FirstName = "Bob", MiddleName = "Blakely" };
                adviseeRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);
                // Act--search
                criteria.AdviseeKeyword = "0000894";
                var privacyWrapper = await advisorService.SearchForExactMatchAsync(criteria, 1, 1);
                var result = privacyWrapper.Dto as List<Advisee>;
                // Assert
                Assert.AreEqual(1, result.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task Advisor_NotPerson_ThrowsException()
            {
                // Mock thrown error from advisor Repository
                advisorRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>(), It.IsAny<AdviseeInclusionType>())).ThrowsAsync(new KeyNotFoundException());
                // Set up needed permissions for advisor role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Mock search response
                var student1 = new Domain.Student.Entities.PlanningStudent("0000894", "Smith", 2, new List<string>() { "BA.ENGL" }) { FirstName = "Bob", MiddleName = "Blakely" };
                adviseeRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);
                // Act--search
                criteria.AdviseeKeyword = "0000894";
                var result = await advisorService.SearchForExactMatchAsync(criteria, 1, 1);
            }

            // Additional AdvisorKeyword Tests
            // Since parsing was fully tested with the advisee search string just add a couple test to be sure advisorKeyword gets passed and parsed properly

            [TestMethod]
            public async Task AdvisorSearchStringContainsOnlyOneString()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Setup callback
                string[] parsedStrings = null;
                advisorRepoMock.Setup(repo => repo.SearchAdvisorByNameForExactMatchAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Callback<string, string, string>((string s1, string s2, string s3) => parsedStrings = new string[] { s1, s2, s3 })
                    .ReturnsAsync(new List<string>());
                // Act--search
                criteria.AdvisorKeyword = "jones";
                await advisorService.SearchForExactMatchAsync(criteria, 1, 1);
                // Asserts
                Assert.AreEqual("jones", parsedStrings[0]);
                Assert.AreEqual(null, parsedStrings[1]);
                Assert.AreEqual(null, parsedStrings[2]);
            }

            [TestMethod]
            public async Task AdvisorSearchStringContainsAnId_AnyAdvisee()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Mock advisee repo mock of SearchByAdvisorIds
                var student1 = new Domain.Student.Entities.PlanningStudent("0000894", "Smith", 2, new List<string>() { "BA.ENGL" }) { FirstName = "Bob", MiddleName = "Blakely" };
                var student2 = new Domain.Student.Entities.PlanningStudent("00004002", "Jones", 802, new List<string>() { "BA.MATH" });
                IEnumerable<Domain.Student.Entities.PlanningStudent> students = new List<Domain.Student.Entities.PlanningStudent>() { student1, student2 };
                adviseeRepoMock.Setup(repo => repo.SearchByAdvisorIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<int>(), It.IsAny<int>(), null)).ReturnsAsync(students);
                // Act--search - Id doesn't matter since the repo will return 2 students
                criteria.AdvisorKeyword = "0009999";
                var privacyWrapper = await advisorService.SearchForExactMatchAsync(criteria, 1, 1);
                var advisees = privacyWrapper.Dto as List<Advisee>;
                // Asserts
                Assert.AreEqual(2, advisees.Count());
            }

            [TestMethod]
            public async Task AdvisorSearchStringContainsName_AssignedAdvisee()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Mock advisee repo mock of SearchByAdvisorIds
                var student1 = new Domain.Student.Entities.PlanningStudent("0000894", "Smith", 2, new List<string>() { "BA.ENGL" }) { FirstName = "Bob", MiddleName = "Blakely" };
                var student2 = new Domain.Student.Entities.PlanningStudent("00004002", "Jones", 802, new List<string>() { "BA.MATH" });
                IEnumerable<Domain.Student.Entities.PlanningStudent> students = new List<Domain.Student.Entities.PlanningStudent>() { student1, student2 };
                adviseeRepoMock.Setup(repo => repo.SearchByAdvisorIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>())).ReturnsAsync(students);
                // Act--search - Id doesn't matter since the repo will return 2 students
                criteria.AdvisorKeyword = "Jones";
                var privacyWrapper = await advisorService.SearchForExactMatchAsync(criteria, 1, 1);
                var advisees = privacyWrapper.Dto as List<Advisee>;
                // Asserts
                Assert.AreEqual(2, advisees.Count());
            }
        }
    }
}
