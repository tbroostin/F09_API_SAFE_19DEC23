// Copyright 2015 Ellucian Company L.P. and its affiliates.using System;
using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Repositories;
using Moq;
using Ellucian.Web.Adapters;
using slf4net;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Web.Security;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Tests;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class RegistrationGroupServiceTests
    {
        // Sets up a Current user 
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
                            PersonId = "0000111",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "Advisor",
                            Roles = new List<string>() { "Advisor" },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }
        }
        
        [TestClass]
        public class GetSectionRegistrationDates
        {
            private Mock<IRegistrationGroupRepository> registrationGroupRepositoryMock;
            private IRegistrationGroupRepository registrationGroupRepository;
            private Mock<ISectionRepository> sectionRepositoryMock;
            private ISectionRepository sectionRepository;
            private Mock<ITermRepository> termRepositoryMock;
            private ITermRepository termRepository;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionRegistrationDate, Ellucian.Colleague.Dtos.Student.SectionRegistrationDate> sectionRegistrationDateAdapter;
            private RegistrationGroupService registrationGroupService;
            private List<string> sectionIds;

            [TestInitialize]
            public async void Initialize()
            {
                registrationGroupRepositoryMock = new Mock<IRegistrationGroupRepository>();
                registrationGroupRepository = registrationGroupRepositoryMock.Object;
                sectionRepositoryMock = new Mock<ISectionRepository>();
                sectionRepository = sectionRepositoryMock.Object;
                termRepositoryMock = new Mock<ITermRepository>();
                termRepository = termRepositoryMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                logger = new Mock<ILogger>().Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;

                sectionRegistrationDateAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionRegistrationDate, Ellucian.Colleague.Dtos.Student.SectionRegistrationDate>(adapterRegistry, logger);
                adapterRegistryMock.Setup(areg => areg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionRegistrationDate, Ellucian.Colleague.Dtos.Student.SectionRegistrationDate>()).Returns(sectionRegistrationDateAdapter);

                registrationGroupRepositoryMock.Setup(x => x.GetRegistrationGroupIdAsync(It.IsAny<string>())).Returns(Task.FromResult("WEBREG"));

                var registrationGroup = new RegistrationGroup("WEBREG");
                registrationGroupRepositoryMock.Setup(x => x.GetRegistrationGroupAsync(It.IsAny<string>())).Returns(Task.FromResult(registrationGroup));

                var allSections = new TestSectionRepository().GetAsync().Result;
                var sections = allSections.Where(s => s.Id == "15" || s.Id == "16");
                sectionIds = sections.Select(s => s.Id).ToList();
                sectionRepositoryMock.Setup(acc => acc.GetCachedSectionsAsync(sectionIds, false)).Returns(Task.FromResult<IEnumerable<Section>>(sections));

                var regTerms = await new TestTermRepository().GetRegistrationTermsAsync();
                termRepositoryMock.Setup(tr => tr.GetRegistrationTermsAsync()).Returns(Task.FromResult(regTerms));
                

                // Set up current user
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();

                registrationGroupService = new RegistrationGroupService(adapterRegistryMock.Object, registrationGroupRepositoryMock.Object, sectionRepositoryMock.Object, termRepositoryMock.Object, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                registrationGroupRepository = null;
                sectionRepository = null;
                termRepository = null;
                adapterRegistry = null;
                registrationGroupService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task RegistrationGroupService_NullSectionIds_ThrowsError()
            {
                var sectionRegistrationDateDTOs = await registrationGroupService.GetSectionRegistrationDatesAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task RegistrationGroupService_EmptySectionIds_ThrowsError()
            {
                var sectionRegistrationDateDTOs = await registrationGroupService.GetSectionRegistrationDatesAsync(new List<String>());
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task RegistrationGroupService_UnableToGetRegUserId_ThrowsError()
            {
                registrationGroupRepositoryMock.Setup(x => x.GetRegistrationGroupIdAsync(It.IsAny<string>())).Returns(Task.FromResult(string.Empty));
                var sectionRegistrationDateDTOs = await registrationGroupService.GetSectionRegistrationDatesAsync(sectionIds);
            }

            [TestMethod]
            public void RegistrationGroupService_SectionsNotCached_ReturnsEmptyList()
            {
                sectionRepositoryMock.Setup(acc => acc.GetCachedSectionsAsync(sectionIds, false)).Returns(Task.FromResult<IEnumerable<Section>>(new List<Section>()));
                var sectionRegistrationDateDTOs = registrationGroupService.GetSectionRegistrationDatesAsync(sectionIds).Result;
                Assert.AreEqual(0, sectionRegistrationDateDTOs.Count());
            }

            [TestMethod]
            public void RegistrationGroupService_GetSectionRegistrationDates()
            {
                var sectionRegistrationDateDTOs = registrationGroupService.GetSectionRegistrationDatesAsync(sectionIds).Result;
                Assert.AreEqual(sectionIds.Count(), sectionRegistrationDateDTOs.Count());
            }
        }
    }
}
