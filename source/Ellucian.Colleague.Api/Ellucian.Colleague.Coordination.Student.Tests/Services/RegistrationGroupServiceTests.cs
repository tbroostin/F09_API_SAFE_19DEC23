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
            IEnumerable<Section> sections;
            List<Term> regTerms;

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

                
                 sections = allSections.Where(s => s.Id == "15" || s.Id == "16" || s.Id=="342" || s.Id=="669");

                //census dates on section 15 is from TestTermRepositorry to add on term 2012/FA
                //census dates on section 16 is from testTermrepository with location NW
                //669 is from 2014/FA - no census date anywhere ACTM -> TLOC -> SRGD
                //add census dates on section 342
                List<DateTime?> censusDates = new List<DateTime?>();
                censusDates.AddRange(new List<DateTime?>(){
                    new DateTime(2021, 11, 12),
                    new DateTime(2021, 12, 11)
                    });
                sections.Where(s => s.Id == "342").First().RegistrationDateOverrides = new RegistrationDate(null, null, null, null, null,null,null,null,null,null,censusDates );
                
                
                sectionIds = sections.Select(s => s.Id).ToList();
                sectionRepositoryMock.Setup(acc => acc.GetCachedSectionsAsync(sectionIds, false)).Returns(Task.FromResult<IEnumerable<Section>>(sections));

                 regTerms = (await new TestTermRepository().GetRegistrationTermsAsync()).ToList();
                Term newTerm = new Term("2014/FA", "new term with no dates", new DateTime(2014, 1, 2), new DateTime(2014, 09, 12), 2014, 1, false, false, "2014R", false);
                newTerm.AddRegistrationDates(new RegistrationDate(null, null, null, null, null, null, null, null, null, null, null));

                regTerms.Add(newTerm);
                termRepositoryMock.Setup(tr => tr.GetRegistrationTermsAsync()).Returns(Task.FromResult(regTerms.AsEnumerable()));
                

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
                var sectionRegistrationDateDTOs = registrationGroupService.GetSectionRegistrationDatesAsync(sectionIds,true).Result;
                Assert.AreEqual(sectionIds.Count(), sectionRegistrationDateDTOs.Count());
            }

            [TestMethod]
            public void  RegistrationGroupService_GetSectionRegistrationDates_CensusDates()
            {
                //Census dates on ACTM only

                var sectionRegistrationDateDTOs = registrationGroupService.GetSectionRegistrationDatesAsync(sectionIds, false).Result;
                //15  from 2012/FA term and that term have census dates (ACTM)
                var section1Dates = sectionRegistrationDateDTOs.Where(s => s.SectionId == "15").First();

                //16 have location NW and term 2012/FA have registration dates for the term NW (TLOC)
                var section2Dates = sectionRegistrationDateDTOs.Where(s => s.SectionId == "16").First();

                //342 is from term 2013/SP and have census dates on section (SGRD)
                var section3Dates = sectionRegistrationDateDTOs.Where(s => s.SectionId == "342").First();

                //669 from 2014/FA
                var section4Dates = sectionRegistrationDateDTOs.Where(s => s.SectionId == "669").First();

                Assert.AreEqual(sectionIds.Count(), sectionRegistrationDateDTOs.Count());

                //section1 takes from ACTM i,e 2012/FA with no location dates

                Assert.AreEqual(2, section1Dates.CensusDates.Count);
                Assert.AreEqual(new DateTime(2021, 01, 01), section1Dates.CensusDates[0]);
                Assert.AreEqual(new DateTime(2021, 03, 02), section1Dates.CensusDates[1]);

                //section2 takes from TLOC with 2012/fa and NW location
                Assert.AreEqual(3, section2Dates.CensusDates.Count);
                Assert.AreEqual(new DateTime(2021, 02, 01), section2Dates.CensusDates[0]);
                Assert.AreEqual(new DateTime(2021, 05, 02), section2Dates.CensusDates[1]);
                Assert.AreEqual(new DateTime(2021, 06, 02), section2Dates.CensusDates[2]);

                //section3 takes from SGRD
                Assert.AreEqual(2, section1Dates.CensusDates.Count);
                Assert.AreEqual(new DateTime(2021, 11, 12), section3Dates.CensusDates[0]);
                Assert.AreEqual(new DateTime(2021, 12, 11), section3Dates.CensusDates[1]);

                //section4
                Assert.AreEqual(0, section4Dates.CensusDates.Count());
            }

        }
    }
}
