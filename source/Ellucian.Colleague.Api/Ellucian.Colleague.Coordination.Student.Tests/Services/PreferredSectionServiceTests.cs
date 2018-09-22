// Copyright 2014-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ellucian.Web.Http.TestUtil;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Tests;
using slf4net;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class PreferredSectionServiceTests
    {
        // Setup a Current User that is a student
        public abstract class CurrentUserSetup
        {
            public class Student001UserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "Fred",
                            PersonId = "S001",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "Student",
                            Roles = new List<string>() { "Student" },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }
            public class Student002UserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "234",
                            Name = "Barney",
                            PersonId = "S002",
                            SecurityToken = "432",
                            SessionTimeout = 30,
                            UserName = "Student",
                            Roles = new List<string>() { "Student" },
                            SessionFixationId = "bcd234"
                        });
                    }
                }
            }
        }

        [TestClass]
        public class Get_AsStudentUser : CurrentUserSetup
        {
            private PreferredSectionService preferredSectionService;
            private Mock<IStudentRepository> stuRepoMock;
            private IStudentRepository stuRepo;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private Mock<IPreferredSectionRepository> preferredSectionRepoMock;
            private IPreferredSectionRepository preferredSectionRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private Domain.Student.Entities.Student student1;
            private Domain.Student.Entities.Student student2;
            private Domain.Student.Entities.PreferredSection preferredSection1;
            private Domain.Student.Entities.PreferredSection preferredSection2;
            private Domain.Student.Entities.PreferredSection preferredSection3;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            [TestInitialize]
            public void Initialize()
            {
                stuRepoMock = new Mock<IStudentRepository>();
                stuRepo = stuRepoMock.Object;
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                preferredSectionRepoMock = new Mock<IPreferredSectionRepository>();
                preferredSectionRepo = preferredSectionRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                logger = new Mock<ILogger>().Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                student1 = new Domain.Student.Entities.Student("S001", "Klemperer", null, new List<string>(), new List<string>());
                stuRepoMock.Setup(r => r.GetAsync("S001")).ReturnsAsync(student1);
                student2 = new Domain.Student.Entities.Student("S002", "Banner", null, new List<string>(), new List<string>());
                stuRepoMock.Setup(r => r.GetAsync("S002")).ReturnsAsync(student2);

                currentUserFactory = new CurrentUserSetup.Student001UserFactory();

                Domain.Student.Entities.Section sect1 = new Domain.Student.Entities.Section("SEC001", "C01", "100", new DateTime(2014, 9, 3), 3m, null, "Test Section1", "IN", new List<OfferingDepartment>() { new OfferingDepartment("HIST", 100m) }, new List<string>() { "100" }, "UG", new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddYears(-5)) });
                Domain.Student.Entities.Section sect2 = new Domain.Student.Entities.Section("SEC002", "C02", "100", new DateTime(2014, 9, 3), 3m, null, "Test Section2", "IN", new List<OfferingDepartment>() { new OfferingDepartment("HIST", 100m) }, new List<string>() { "100" }, "UG", new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddYears(-5)) });
                Domain.Student.Entities.Section sect3 = new Domain.Student.Entities.Section("SEC003", "C03", "100", new DateTime(2014, 9, 3), 3m, null, "Test Section3", "IN", new List<OfferingDepartment>() { new OfferingDepartment("HIST", 100m) }, new List<string>() { "100" }, "UG", new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddYears(-5)) });

                IEnumerable<string> sec1IdList = new List<string>() { "SEC001" };
                List<Domain.Student.Entities.Section> sec1List = new List<Domain.Student.Entities.Section>() { sect1 };
                sectionRepoMock.Setup(r => r.GetCachedSectionsAsync(sec1IdList, false)).ReturnsAsync(sec1List);
                IEnumerable<string> sec2IdList = new List<string>() { "SEC002" };
                List<Domain.Student.Entities.Section> sec2List = new List<Domain.Student.Entities.Section>() { sect2 };
                sectionRepoMock.Setup(r => r.GetCachedSectionsAsync(sec2IdList, false)).ReturnsAsync(sec2List);
                IEnumerable<string> sec3IdList = new List<string>() { "SEC003" };
                List<Domain.Student.Entities.Section> sec3List = new List<Domain.Student.Entities.Section>() { sect3 };
                sectionRepoMock.Setup(r => r.GetCachedSectionsAsync(sec3IdList, false)).ReturnsAsync(sec3List);

                preferredSection1 = new Domain.Student.Entities.PreferredSection("S001", "SEC001", null);
                decimal? credits1 = decimal.Parse("3");
                preferredSection2 = new Domain.Student.Entities.PreferredSection("S001", "SEC002", credits1);
                decimal? credits2 = Decimal.Parse("4.75");
                preferredSection3 = new Domain.Student.Entities.PreferredSection("S001", "SEC003", credits2);
                IEnumerable<Domain.Student.Entities.PreferredSection> preferredSections = new List<Domain.Student.Entities.PreferredSection>() { preferredSection1, preferredSection2, preferredSection3 }.AsEnumerable();
                IEnumerable<Domain.Student.Entities.PreferredSection> noSections = new List<Domain.Student.Entities.PreferredSection>();

                IEnumerable<Domain.Student.Entities.PreferredSectionMessage> noMessages = new List<Domain.Student.Entities.PreferredSectionMessage>();

                Domain.Student.Entities.PreferredSectionsResponse prefSecResp = new Domain.Student.Entities.PreferredSectionsResponse(preferredSections, noMessages);
                preferredSectionRepoMock.Setup(r => r.GetAsync("S001")).ReturnsAsync(prefSecResp);

                Domain.Student.Entities.PreferredSectionsResponse noPrefSecResp = new Domain.Student.Entities.PreferredSectionsResponse(noSections, noMessages);
                preferredSectionRepoMock.Setup(r => r.GetAsync("S002")).ReturnsAsync(noPrefSecResp);

                var preferredSectionsResponseDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.PreferredSectionsResponse, Ellucian.Colleague.Dtos.Student.PreferredSectionsResponse>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.PreferredSectionsResponse, Ellucian.Colleague.Dtos.Student.PreferredSectionsResponse>()).Returns(preferredSectionsResponseDtoAdapter);
                var preferredSectionDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.PreferredSection, Ellucian.Colleague.Dtos.Student.PreferredSection>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.PreferredSection, Ellucian.Colleague.Dtos.Student.PreferredSection>()).Returns(preferredSectionDtoAdapter);
                var preferredSectionMessageDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.PreferredSectionMessage, Ellucian.Colleague.Dtos.Student.PreferredSectionMessage>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.PreferredSectionMessage, Ellucian.Colleague.Dtos.Student.PreferredSectionMessage>()).Returns(preferredSectionMessageDtoAdapter);

                preferredSectionService = new PreferredSectionService(adapterRegistry, preferredSectionRepo, stuRepo, currentUserFactory, roleRepo, logger, baseConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                stuRepo = null;
                sectionRepo = null;
                preferredSectionRepo = null;
                adapterRegistry = null;
                logger = null;
                preferredSectionService = null;
                currentUserFactory = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PreferredSection_GetThrowsIfNullStudentId()
            {
                await preferredSectionService.GetAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PreferredSection_GetThrowsIfEmptyStudentId()
            {
                await preferredSectionService.GetAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task PreferredSection_GetThrowsIfStudentNotFound()
            {
                Domain.Student.Entities.Student student = null;
                stuRepoMock.Setup(repo => repo.GetAsync("S001")).ReturnsAsync(student);
                await preferredSectionService.GetAsync("S001");
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task PreferredSection_GetThrowsIfNotCorrectStudent()
            {
                var results = await preferredSectionService.GetAsync("S002");
            }

            [TestMethod]
            public async Task PreferredSection_GetReturnsPreferredSections()
            {
                var results = await preferredSectionService.GetAsync("S001");
                Assert.AreEqual(3, results.PreferredSections.Count());
            }
        }

        [TestClass]
        public class Update_AsStudentUser : CurrentUserSetup
        {
            private PreferredSectionService preferredSectionService;
            private Mock<IStudentRepository> stuRepoMock;
            private IStudentRepository stuRepo;
            private Mock<IPreferredSectionRepository> preferredSectionRepoMock;
            private IPreferredSectionRepository preferredSectionRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private Domain.Student.Entities.Student student1;
            private Domain.Student.Entities.Student student2;
            private Domain.Student.Entities.PreferredSection preferredSection1;
            private Domain.Student.Entities.PreferredSection preferredSection2;
            private Domain.Student.Entities.PreferredSection preferredSection3;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            [TestInitialize]
            public void Initialize()
            {
                stuRepoMock = new Mock<IStudentRepository>();
                stuRepo = stuRepoMock.Object;
                preferredSectionRepoMock = new Mock<IPreferredSectionRepository>();
                preferredSectionRepo = preferredSectionRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                logger = new Mock<ILogger>().Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                student1 = new Domain.Student.Entities.Student("S001", "Klemperer", null, new List<string>(), new List<string>());
                stuRepoMock.Setup(r => r.GetAsync("S001")).ReturnsAsync(student1);
                student2 = new Domain.Student.Entities.Student("S002", "Banner", null, new List<string>(), new List<string>());
                stuRepoMock.Setup(r => r.GetAsync("S002")).ReturnsAsync(student2);

                currentUserFactory = new CurrentUserSetup.Student001UserFactory();

                preferredSection1 = new Domain.Student.Entities.PreferredSection("S001", "SEC001", null);
                decimal? credits1 = decimal.Parse("3");
                decimal? credits2 = decimal.Parse("4.75");
                preferredSection2 = new Domain.Student.Entities.PreferredSection("S001", "SEC002", credits1);
                preferredSection3 = new Domain.Student.Entities.PreferredSection("S001", "SEC003", credits2);
                List<Domain.Student.Entities.PreferredSection> preferredSections = new List<Domain.Student.Entities.PreferredSection>() { preferredSection1, preferredSection2, preferredSection3 };

                // the success case
                IEnumerable<Domain.Student.Entities.PreferredSectionMessage> noMessages = new List<Domain.Student.Entities.PreferredSectionMessage>();


                preferredSectionRepoMock.Setup(r => r.UpdateAsync("S001", It.IsAny<List<Domain.Student.Entities.PreferredSection>>())).ReturnsAsync(noMessages);

                var preferredSectionsResponseDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.PreferredSectionsResponse, Ellucian.Colleague.Dtos.Student.PreferredSectionsResponse>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.PreferredSectionsResponse, Ellucian.Colleague.Dtos.Student.PreferredSectionsResponse>()).Returns(preferredSectionsResponseDtoAdapter);
                var preferredSectionDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.PreferredSection, Ellucian.Colleague.Dtos.Student.PreferredSection>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.PreferredSection, Ellucian.Colleague.Dtos.Student.PreferredSection>()).Returns(preferredSectionDtoAdapter);
                var preferredSectionMessageDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.PreferredSectionMessage, Ellucian.Colleague.Dtos.Student.PreferredSectionMessage>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.PreferredSectionMessage, Ellucian.Colleague.Dtos.Student.PreferredSectionMessage>()).Returns(preferredSectionMessageDtoAdapter);

                preferredSectionService = new PreferredSectionService(adapterRegistry, preferredSectionRepo, stuRepo, currentUserFactory, roleRepo, logger, baseConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                stuRepo = null;
                preferredSectionRepo = null;
                adapterRegistry = null;
                logger = null;
                preferredSectionService = null;
                currentUserFactory = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PreferredSection_UpdateThrowsIfNullStudentId()
            {
                await preferredSectionService.UpdateAsync(null, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PreferredSection_UpdateThrowsIfEmptyStudentId()
            {
                await preferredSectionService.UpdateAsync("", null);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task PreferredSection_UpdateThrowsIfStudentNotFound()
            {
                Domain.Student.Entities.Student student = null;
                stuRepoMock.Setup(repo => repo.GetAsync("S001")).ReturnsAsync(student);
                await preferredSectionService.UpdateAsync("S001", null);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task PreferredSection_UpdateThrowsIfNotCorrectStudent()
            {
                var results = await preferredSectionService.UpdateAsync("S002", null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task PreferredSection_UpdateThrowsIfNullPreferredSections()
            {
                var results = await preferredSectionService.UpdateAsync("S001", null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task PreferredSection_UpdateThrowsIfNoPreferredSections()
            {
                IEnumerable<Dtos.Student.PreferredSection> noSections = new List<Dtos.Student.PreferredSection>();
                var results = await preferredSectionService.UpdateAsync("S001", noSections);
            }

            [TestMethod]
            public async Task PreferredSection_UpdateSuccessfully()
            {
                List<Dtos.Student.PreferredSection> sections = new List<Dtos.Student.PreferredSection>();
                sections.Add(new Dtos.Student.PreferredSection() { StudentId = "S001", SectionId = "SEC003", Credits = null });
                IEnumerable<Dtos.Student.PreferredSectionMessage> results = await preferredSectionService.UpdateAsync("S001", sections);
                Assert.AreEqual(0, results.Count());
            }
        }

        [TestClass]
        public class Delete_AsStudentUser : CurrentUserSetup
        {
            private PreferredSectionService preferredSectionService;
            private Mock<IStudentRepository> stuRepoMock;
            private IStudentRepository stuRepo;
            private Mock<IPreferredSectionRepository> preferredSectionRepoMock;
            private IPreferredSectionRepository preferredSectionRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private Domain.Student.Entities.Student student1;
            private Domain.Student.Entities.Student student2;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            [TestInitialize]
            public void Initialize()
            {
                stuRepoMock = new Mock<IStudentRepository>();
                stuRepo = stuRepoMock.Object;
                preferredSectionRepoMock = new Mock<IPreferredSectionRepository>();
                preferredSectionRepo = preferredSectionRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                logger = new Mock<ILogger>().Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                student1 = new Domain.Student.Entities.Student("S001", "Klemperer", null, new List<string>(), new List<string>());
                stuRepoMock.Setup(r => r.GetAsync("S001")).ReturnsAsync(student1);
                student2 = new Domain.Student.Entities.Student("S002", "Banner", null, new List<string>(), new List<string>());
                stuRepoMock.Setup(r => r.GetAsync("S002")).ReturnsAsync(student2);

                currentUserFactory = new CurrentUserSetup.Student001UserFactory();

                List<Domain.Student.Entities.PreferredSectionMessage> noMessages = new List<Domain.Student.Entities.PreferredSectionMessage>();
                List<Domain.Student.Entities.PreferredSectionMessage> oneMessage = new List<Domain.Student.Entities.PreferredSectionMessage>();
                oneMessage.Add(new Domain.Student.Entities.PreferredSectionMessage("BOGUS", "Some Repo Message"));
                preferredSectionRepoMock.Setup(r => r.DeleteAsync("S001", "SEC001")).ReturnsAsync(noMessages.AsEnumerable());
                preferredSectionRepoMock.Setup(r => r.DeleteAsync("S001", "BOGUS")).ReturnsAsync(oneMessage.AsEnumerable());


                var preferredSectionsResponseDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.PreferredSectionsResponse, Ellucian.Colleague.Dtos.Student.PreferredSectionsResponse>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.PreferredSectionsResponse, Ellucian.Colleague.Dtos.Student.PreferredSectionsResponse>()).Returns(preferredSectionsResponseDtoAdapter);
                var preferredSectionDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.PreferredSection, Ellucian.Colleague.Dtos.Student.PreferredSection>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.PreferredSection, Ellucian.Colleague.Dtos.Student.PreferredSection>()).Returns(preferredSectionDtoAdapter);
                var preferredSectionMessageDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.PreferredSectionMessage, Ellucian.Colleague.Dtos.Student.PreferredSectionMessage>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.PreferredSectionMessage, Ellucian.Colleague.Dtos.Student.PreferredSectionMessage>()).Returns(preferredSectionMessageDtoAdapter);

                preferredSectionService = new PreferredSectionService(adapterRegistry, preferredSectionRepo, stuRepo, currentUserFactory, roleRepo, logger, baseConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                stuRepo = null;
                preferredSectionRepo = null;
                adapterRegistry = null;
                logger = null;
                preferredSectionService = null;
                currentUserFactory = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PreferredSection_DeleteThrowsIfNullStudentId()
            {
                await preferredSectionService.DeleteAsync(null, "SEC001");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PreferredSection_DeleteThrowsIfEmptyStudentId()
            {
                await preferredSectionService.DeleteAsync("", "SEC001");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task PreferredSection_DeleteThrowsIfStudentNotFound()
            {
                Domain.Student.Entities.Student student = null;
                stuRepoMock.Setup(repo => repo.GetAsync("S001")).ReturnsAsync(student);
                await preferredSectionService.DeleteAsync("S001", "SEC001");
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task PreferredSection_DeleteThrowsIfNotCorrectStudent()
            {
                var results = await preferredSectionService.DeleteAsync("S002", "SEC001");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task PreferredSection_DeleteThrowsIfNullSectionId()
            {
                var results = await preferredSectionService.DeleteAsync("S001", null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task PreferredSection_DeleteThrowsIfEmptySectionId()
            {
                var results = await preferredSectionService.DeleteAsync("S001", "");
            }

            [TestMethod]
            public async Task PreferredSection_DeleteSuccessNoMessages()
            {
                // successful deletion yields no messages
                IEnumerable<Dtos.Student.PreferredSectionMessage> results = await preferredSectionService.DeleteAsync("S001", "SEC001");
                Assert.AreEqual(0, results.Count());
            }

            [TestMethod]
            public async Task PreferredSection_PassesThruRepoMessage()
            {
                IEnumerable<Dtos.Student.PreferredSectionMessage> results = await preferredSectionService.DeleteAsync("S001", "BOGUS");
                Assert.AreEqual(1, results.Count());
            }

        }
    }
}
