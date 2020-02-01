// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class FacultyServiceTests
    {
        // Sets up a Current user that is an advisor
        public abstract class CurrentUserSetup
        {
            protected Role facultyRole = new Role(105, "Faculty");

            public class FacultyUserFactory : ICurrentUserFactory
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
                            UserName = "Faculty",
                            Roles = new List<string>() { "Faculty" },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }
        }

        [TestClass]
        public class GetFacultySectionsAsync_Tests
        {
            private FacultyService facultyService;
            private Mock<IFacultyRepository> facultyRepoMock;
            private IFacultyRepository facultyRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private Mock<IStudentConfigurationRepository> configRepoMock;
            private IStudentConfigurationRepository configRepo;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IReferenceDataRepository> referenceRepoMock;
            private IReferenceDataRepository referenceRepo;
            private Mock<IStudentReferenceDataRepository> studentReferenceRepoMock;
            private IStudentReferenceDataRepository studentReferenceRepo;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private string faculty1Id;
            private Domain.Student.Entities.Faculty faculty1;
            private string faculty2Id;
            private Domain.Student.Entities.Faculty faculty2;
            private Domain.Student.Entities.Section section1;
            private Domain.Student.Entities.Section section2;
            private Domain.Student.Entities.Section section3;
            private Domain.Student.Entities.Section section4;
            private Domain.Student.Entities.Section section5;
            private Domain.Student.Entities.Section section6;
            private Domain.Student.Entities.Section section7;
            private Domain.Student.Entities.Section section8;
            private Domain.Student.Entities.Section section9;
            private Domain.Student.Entities.Section section10;
            private Domain.Student.Entities.Section section11;
            private Domain.Student.Entities.Section section12;
            private Domain.Student.Entities.Term term1;
            private Domain.Student.Entities.Term term2;
            private Domain.Student.Entities.Term term3;
            private StudentConfiguration config;

            [TestInitialize]
            public async void Initialize()
            {
                facultyRepoMock = new Mock<IFacultyRepository>();
                facultyRepo = facultyRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                configRepoMock = new Mock<IStudentConfigurationRepository>();
                configRepo = configRepoMock.Object;
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                referenceRepoMock = new Mock<IReferenceDataRepository>();
                referenceRepo = referenceRepoMock.Object;
                studentReferenceRepoMock = new Mock<IStudentReferenceDataRepository>();
                studentReferenceRepo = studentReferenceRepoMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;

                // Set up current user
                currentUserFactory = new CurrentUserSetup.FacultyUserFactory();

                // Mock faculty repo response
                faculty1Id = "0000002";
                faculty1 = new Domain.Student.Entities.Faculty(faculty1Id, "Quincy");
                faculty1.AddEmailAddress(new EmailAddress("xyz@xmail.com", "FAC"));
                faculty1.AddPhone(new Phone("864-123-4564", "O"));
                facultyRepoMock.Setup(repo => repo.GetAsync(faculty1Id)).Returns(Task.FromResult(faculty1));

                // Mock faculty repo response
                faculty2Id = "0000004";
                faculty2 = new Domain.Student.Entities.Faculty(faculty2Id, "Jones");
                faculty2.AddEmailAddress(new EmailAddress("abc@xmail.com", "FAC"));
                faculty2.AddPhone(new Phone("864-123-9998", "O"));
                facultyRepoMock.Setup(repo => repo.GetAsync(faculty2Id)).Returns(Task.FromResult(faculty2));

                // Mock config repo response
                config = new StudentConfiguration();
                config.FacultyEmailTypeCode = "FAC";
                config.FacultyPhoneTypeCode = "O";
                configRepoMock.Setup(repo => repo.GetStudentConfigurationAsync()).Returns(Task.FromResult(config));

                // Mock term repo response
                term1 = new Domain.Student.Entities.Term("2011/FA", "Fall 2011", new DateTime(2011, 9, 1), new DateTime(2011, 12, 25), 2011, 1, true, true, "2011RT", true);
                term2 = new Domain.Student.Entities.Term("2012/SP", "Spring 2012", new DateTime(2012, 1, 5), new DateTime(2012, 5, 30), 2011, 2, true, true, "2011RT", true);
                term3 = new Domain.Student.Entities.Term("2012/SU", "Summer 2012", new DateTime(2012, 6, 4), new DateTime(2012, 8, 25), 2012, 1, true, true, "2012RT", true);
                IEnumerable<Domain.Student.Entities.Term> termList = new List<Domain.Student.Entities.Term>() { term1, term2, term3 };
                termRepoMock.Setup(repo => repo.GetRegistrationTermsAsync()).Returns(Task.FromResult(termList));

                // Mock section repo response
                ICollection<OfferingDepartment> depts = new List<OfferingDepartment>() { new OfferingDepartment("D1", 50m), new OfferingDepartment("D2", 50m) };
                ICollection<string> clcs = new List<string>() { "100", "200", "300", "400" };
                List<SectionStatusItem> statuses = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddYears(-5)) };
                // sections 1 -- 6 go in term 1 for range testing with test range start and end of 9/5 and 12/15, 
                // and the test faculty ("0000001") assigned to all sections
                // testing for overlap of time periods
                // period           start       end
                // section 1  <---> |            |        - runs  9/1  -  9/4,  excluded
                // section 2  <-----|----->      |        - runs  9/1  - 11/15, included
                // section 3        |   <----->  |        - runs 10/5  - 11/15, included
                // section 4        |      <-----|----->  - runs 11/5  - 12/25, included
                // section 5        |            |  <-->  - runs 12/17 - 12/25, excluded
                // section 6  <-----|------------|----->  - runs  9/1  - 12/25, included
                // Setup the sections in term1
                section1 = new Domain.Student.Entities.Section("11", "11", "01", new DateTime(2011, 9, 1), 3, null, "Section1 Title", "IN", depts, clcs, "UG", statuses, false, true, false, true, false);
                section1.TermId = "2011/FA"; section1.EndDate = new DateTime(2011, 9, 4); section1.AddFaculty("0000001");
                // section 2 overlap start before test start date, ends before test end date
                section2 = new Domain.Student.Entities.Section("12", "11", "02", new DateTime(2011, 9, 1), 3, null, "Section2 Title", "IN", depts, clcs, "UG", statuses, false, true, false, true, false);
                section2.TermId = "2011/FA"; section2.EndDate = new DateTime(2011, 11, 15); section2.AddFaculty("0000001");
                // section 3 start after test start date and end before test end date
                section3 = new Domain.Student.Entities.Section("13", "12", "01", new DateTime(2011, 10, 5), 3, null, "Section3 Title", "IN", depts, clcs, "UG", statuses, false, true, false, true, false);
                section3.TermId = "2011/FA"; section3.EndDate = new DateTime(2011, 11, 15); section3.AddFaculty("0000001");
                // section 4 start after test start date and before test end date and end after test end date
                section4 = new Domain.Student.Entities.Section("14", "12", "02", new DateTime(2011, 11, 5), 3, null, "Section4 Title", "IN", depts, clcs, "UG", statuses, false, true, false, true, false);
                section4.TermId = "2011/FA"; section4.EndDate = new DateTime(2011, 12, 25); section4.AddFaculty("0000001");
                // section 5 start after test end date
                section5 = new Domain.Student.Entities.Section("15", "13", "01", new DateTime(2011, 12, 17), 3, null, "Section5 Title", "IN", depts, clcs, "UG", statuses, false, true, false, true, false);
                section5.TermId = "2011/FA"; section5.EndDate = new DateTime(2011, 12, 25); section5.AddFaculty("0000001");
                // section 6 start before test start date and end after test end date
                section6 = new Domain.Student.Entities.Section("16", "13", "02", new DateTime(2011, 9, 1), 3, null, "Section6 Title", "IN", depts, clcs, "UG", statuses, false, true, false, true, false);
                section6.TermId = "2011/FA"; section6.EndDate = new DateTime(2011, 12, 25); section6.AddFaculty("0000001");

                // sections 7 -- 9 go in term2, test faculty assigned to only 2 sections
                section7 = new Domain.Student.Entities.Section("21", "11", "01", new DateTime(2012, 1, 5), 3, null, "Section7 Title", "IN", depts, clcs, "UG", statuses, false, true, false, true, false);
                section7.TermId = "2012/SP"; section7.EndDate = new DateTime(2012, 5, 30); section7.AddFaculty("0000001");
                section8 = new Domain.Student.Entities.Section("22", "12", "02", new DateTime(2012, 1, 5), 3, null, "Section8 Title", "IN", depts, clcs, "UG", statuses, false, true, false, true, false);
                section8.TermId = "2012/SP"; section8.EndDate = new DateTime(2012, 5, 30); section8.AddFaculty("0000001");
                section9 = new Domain.Student.Entities.Section("23", "13", "03", new DateTime(2012, 1, 5), 3, null, "Section9 Title", "IN", depts, clcs, "UG", statuses, false, true, false, true, false);
                section9.TermId = "2012/SP"; section9.EndDate = new DateTime(2012, 5, 30); section9.AddFaculty("0000002");

                // sections 10 -- 12 go in term 3, test faculty assigned to no sections
                section10 = new Domain.Student.Entities.Section("31", "11", "02", new DateTime(2012, 6, 4), 3, null, "Section10 Title", "IN", depts, clcs, "UG", statuses, false, true, false, true, false);
                section10.TermId = "2012/SU"; section10.EndDate = new DateTime(2012, 8, 25); section10.AddFaculty("0000002");
                section11 = new Domain.Student.Entities.Section("32", "12", "01", new DateTime(2012, 6, 4), 3, null, "Section11 Title", "IN", depts, clcs, "UG", statuses, false, true, false, true, false);
                section11.TermId = "2012/SU"; section11.EndDate = new DateTime(2012, 8, 25); section11.AddFaculty("0000002");
                section12 = new Domain.Student.Entities.Section("33", "13", "01", new DateTime(2012, 6, 4), 3, null, "Section12 Title", "IN", depts, clcs, "UG", statuses, false, true, false, true, false);
                section12.TermId = "2012/SU"; section12.EndDate = new DateTime(2012, 8, 25); section12.AddFaculty("0000002");
                IEnumerable<Domain.Student.Entities.Section> sectionList = new List<Domain.Student.Entities.Section>() { section1, section2, section3, section4, section5, section6, section7, section8, section9, section10, section11, section12 };
                sectionRepoMock.Setup(repo => repo.GetRegistrationSectionsAsync(termList)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sectionList));

                // Mock Adapters
                var facultyDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Faculty, Ellucian.Colleague.Dtos.Student.Faculty>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Faculty, Ellucian.Colleague.Dtos.Student.Faculty>()).Returns(facultyDtoAdapter);
                var phoneDtoAdapter = new AutoMapperAdapter<Domain.Base.Entities.Phone, Dtos.Base.Phone>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Base.Entities.Phone, Dtos.Base.Phone>()).Returns(phoneDtoAdapter);
                var sectionDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.Section, Dtos.Student.Section>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.Section, Dtos.Student.Section>()).Returns(sectionDtoAdapter);

                // Instantiate faculty service with mocked items
                facultyService = new FacultyService(adapterRegistry, facultyRepo, configRepo, sectionRepo, termRepo, referenceRepo, studentReferenceRepo, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                facultyRepo = null;
                adapterRegistry = null;
                configRepo = null;
                termRepo = null;
                sectionRepo = null;
                logger = null;
            }

            [TestMethod]
            public async Task GetFacultySectionsAsync_ReturnsEmptyListForNoFacultyId()
            {
#pragma warning disable 618
                var result = await facultyService.GetFacultySectionsAsync(null, null, null, false);
#pragma warning restore 618
                Assert.IsNotNull(result);
                Assert.IsNotNull(result.Dto);
                Assert.AreEqual(0, result.Dto.Count());
            }

            [TestMethod]
            public async Task GetFacultySectionsAsync_ReturnsEmptyListForFacultyWithNoSections()
            {
#pragma warning disable 618
                var result = await facultyService.GetFacultySectionsAsync("0000003", null, null, false);
#pragma warning restore 618
                Assert.IsNotNull(result);
                Assert.IsNotNull(result.Dto);
                Assert.AreEqual(0, result.Dto.Count());
            }

            [TestMethod]
            public async Task GetFacultySectionsAsync_ReturnsAllFaculty1Sections()
            {
                // mainly setup validation
                DateTime start = new DateTime(2011, 8, 1);
                DateTime end = new DateTime(2012, 12, 31);
#pragma warning disable 618
                var result = await facultyService.GetFacultySectionsAsync("0000001", start, end, false);
#pragma warning restore 618
                Assert.IsNotNull(result);
                Assert.IsNotNull(result.Dto);
                Assert.AreEqual(8, result.Dto.Count());
            }

            [TestMethod]
            public async Task GetFacultySectionsAsync_ReturnsAllFaculty2Sections()
            {
                // mainly setup validation
                DateTime start = new DateTime(2011, 8, 1);
                DateTime end = new DateTime(2012, 12, 31);
#pragma warning disable 618
                var result = await facultyService.GetFacultySectionsAsync("0000002", start, end, false);
#pragma warning restore 618
                Assert.IsNotNull(result);
                Assert.IsNotNull(result.Dto);
                Assert.AreEqual(4, result.Dto.Count());
            }

            [TestMethod]
            public async Task GetFacultySectionsAsync_ReturnsAllFaculty1FallSections()
            {
                DateTime start = new DateTime(2011, 8, 1);
                DateTime end = new DateTime(2011, 12, 31);
#pragma warning disable 618
                var result = await facultyService.GetFacultySectionsAsync("0000001", start, end, false);
#pragma warning restore 618
                Assert.AreEqual(6, result.Dto.Count());
            }

            [TestMethod]
            public async Task GetFacultySectionsAsync_ReturnsSixFaculty1FallSections()
            {
                DateTime start = new DateTime(2011, 9, 5);
                DateTime end = new DateTime(2011, 12, 15);
#pragma warning disable 618
                var result = await facultyService.GetFacultySectionsAsync("0000001", start, end, false);
#pragma warning restore 618
                Assert.IsNotNull(result);
                Assert.IsNotNull(result.Dto);
                // should omit section1, and section5
                Assert.AreEqual(4, result.Dto.Count());
            }

            [TestMethod]
            public async Task GetFacultySectionsAsync_ReturnsTwoFaculty1SpringSections()
            {
                DateTime start = new DateTime(2012, 1, 5);
                DateTime end = new DateTime(2012, 5, 30);
#pragma warning disable 618
                var result = await facultyService.GetFacultySectionsAsync("0000001", start, end, false);
#pragma warning restore 618
                Assert.IsNotNull(result);
                Assert.IsNotNull(result.Dto);
                Assert.AreEqual(2, result.Dto.Count());
            }

            [TestMethod]
            public async Task GetFacultySectionsAsync_ReturnsZeroFaculty1SummerSections()
            {
                DateTime start = new DateTime(2012, 6, 4);
                DateTime end = new DateTime(2012, 8, 25);
#pragma warning disable 618
                var result = await facultyService.GetFacultySectionsAsync("0000001", start, end, false);
#pragma warning restore 618
                Assert.IsNotNull(result);
                Assert.IsNotNull(result.Dto);
                Assert.AreEqual(0, result.Dto.Count());
            }

            [TestMethod]
            public async Task GetFacultySectionsAsync_ReturnsFiveFaculty1FallSpringSections()
            {
                DateTime start = new DateTime(2011, 11, 16);
                // null end date maps to 11/16/2011 + 90 days
#pragma warning disable 618
                var result = await facultyService.GetFacultySectionsAsync("0000001", start, null, false);
#pragma warning restore 618
                Assert.IsNotNull(result);
                Assert.IsNotNull(result.Dto);
                Assert.AreEqual(5, result.Dto.Count());
            }

            [TestMethod]
            public async Task GetFacultySectionsAsync_EnsuresStartEndOrder()
            {
                DateTime start = new DateTime(2011, 11, 16);
                DateTime end = new DateTime(2011, 10, 15);
                // end before start is discarded, and end is assigned start + 90 days
#pragma warning disable 618
                var result = await facultyService.GetFacultySectionsAsync("0000001", start, end, false);
#pragma warning restore 618
                Assert.IsNotNull(result);
                Assert.IsNotNull(result.Dto);
                Assert.AreEqual(5, result.Dto.Count());
            }

            [TestMethod]
            public async Task GetFacultySectionsAsync_result_has_no_Privacy_Restrictions()
            {
                DateTime start = new DateTime(2011, 11, 16);
                DateTime end = new DateTime(2011, 10, 15);
                IEnumerable<Domain.Student.Entities.Section> sectionList = new List<Domain.Student.Entities.Section>() { section1, section2, section3, section4, section5, section6, section7, section8, section9, section10, section11, section12 };
                // Assign user as faculty on every section returned by repo call
                foreach (var section in sectionList)
                {
                    section.AddFaculty(currentUserFactory.CurrentUser.PersonId);
                }
                sectionRepoMock.Setup(repo => repo.GetRegistrationSectionsAsync(It.IsAny<IEnumerable<Domain.Student.Entities.Term>>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sectionList));
                facultyService = new FacultyService(adapterRegistry, facultyRepo, configRepo, sectionRepo, termRepo, referenceRepo, studentReferenceRepo, currentUserFactory, roleRepo, logger);

                // end before start is discarded, and end is assigned start + 90 days
                #pragma warning disable 618
                var result = await facultyService.GetFacultySectionsAsync("0000001", start, end, false);
                #pragma warning restore 618
                Assert.IsNotNull(result);
                Assert.IsNotNull(result.Dto);
                Assert.AreEqual(5, result.Dto.Count());
                // Since user is faculty on every section returned by repo, none has a privacy restriction
                Assert.IsFalse(result.HasPrivacyRestrictions);
            }

            [TestMethod]
            public async Task GetFacultySectionsAsync_result_has_Privacy_Restrictions()
            {
                DateTime start = new DateTime(2011, 11, 16);
                DateTime end = new DateTime(2011, 10, 15);
                IEnumerable<Domain.Student.Entities.Section> sectionList = new List<Domain.Student.Entities.Section>() { section1, section2, section3, section4, section5, section6, section7, section8, section9, section10, section11, section12 };
                // Only sections where user is not assigned faculty
                sectionList = sectionList.Where(s => !s.FacultyIds.Contains(currentUserFactory.CurrentUser.PersonId)).ToList();
                sectionRepoMock.Setup(repo => repo.GetRegistrationSectionsAsync(It.IsAny<IEnumerable<Domain.Student.Entities.Term>>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sectionList));
                facultyService = new FacultyService(adapterRegistry, facultyRepo, configRepo, sectionRepo, termRepo, referenceRepo, studentReferenceRepo, currentUserFactory, roleRepo, logger);

                // end before start is discarded, and end is assigned start + 90 days
                #pragma warning disable 618
                var result = await facultyService.GetFacultySectionsAsync("0000001", start, end, false);
                #pragma warning restore 618
                Assert.IsNotNull(result);
                Assert.IsNotNull(result.Dto);
                Assert.IsTrue(result.HasPrivacyRestrictions);
                Assert.AreEqual(0, result.Dto.ElementAt(0).ActiveStudentIds.Count());
                Assert.AreEqual(0, result.Dto.ElementAt(1).ActiveStudentIds.Count());
                Assert.AreEqual(0, result.Dto.ElementAt(2).ActiveStudentIds.Count());
                Assert.AreEqual(0, result.Dto.ElementAt(3).ActiveStudentIds.Count());
            }
        }

        [TestClass]
        public class GetFacultySections2Async_Tests
        {
            private FacultyService facultyService;
            private Mock<IFacultyRepository> facultyRepoMock;
            private IFacultyRepository facultyRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private Mock<IStudentConfigurationRepository> configRepoMock;
            private IStudentConfigurationRepository configRepo;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IReferenceDataRepository> referenceRepoMock;
            private IReferenceDataRepository referenceRepo;
            private Mock<IStudentReferenceDataRepository> studentReferenceRepoMock;
            private IStudentReferenceDataRepository studentReferenceRepo;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private string faculty1Id;
            private Domain.Student.Entities.Faculty faculty1;
            private string faculty2Id;
            private Domain.Student.Entities.Faculty faculty2;
            private Domain.Student.Entities.Section section1;
            private Domain.Student.Entities.Section section2;
            private Domain.Student.Entities.Section section3;
            private Domain.Student.Entities.Section section4;
            private Domain.Student.Entities.Section section5;
            private Domain.Student.Entities.Section section6;
            private Domain.Student.Entities.Section section7;
            private Domain.Student.Entities.Section section8;
            private Domain.Student.Entities.Section section9;
            private Domain.Student.Entities.Section section10;
            private Domain.Student.Entities.Section section11;
            private Domain.Student.Entities.Section section12;
            private Domain.Student.Entities.Term term1;
            private Domain.Student.Entities.Term term2;
            private Domain.Student.Entities.Term term3;
            private StudentConfiguration config;

            [TestInitialize]
            public async void Initialize()
            {
                facultyRepoMock = new Mock<IFacultyRepository>();
                facultyRepo = facultyRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                configRepoMock = new Mock<IStudentConfigurationRepository>();
                configRepo = configRepoMock.Object;
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                referenceRepoMock = new Mock<IReferenceDataRepository>();
                referenceRepo = referenceRepoMock.Object;
                studentReferenceRepoMock = new Mock<IStudentReferenceDataRepository>();
                studentReferenceRepo = studentReferenceRepoMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;

                // Set up current user
                currentUserFactory = new CurrentUserSetup.FacultyUserFactory();

                // Mock faculty repo response
                faculty1Id = "0000002";
                faculty1 = new Domain.Student.Entities.Faculty(faculty1Id, "Quincy");
                faculty1.AddEmailAddress(new EmailAddress("xyz@xmail.com", "FAC"));
                faculty1.AddPhone(new Phone("864-123-4564", "O"));
                facultyRepoMock.Setup(repo => repo.GetAsync(faculty1Id)).Returns(Task.FromResult(faculty1));

                // Mock faculty repo response
                faculty2Id = "0000004";
                faculty2 = new Domain.Student.Entities.Faculty(faculty2Id, "Jones");
                faculty2.AddEmailAddress(new EmailAddress("abc@xmail.com", "FAC"));
                faculty2.AddPhone(new Phone("864-123-9998", "O"));
                facultyRepoMock.Setup(repo => repo.GetAsync(faculty2Id)).Returns(Task.FromResult(faculty2));

                // Mock config repo response
                config = new StudentConfiguration();
                config.FacultyEmailTypeCode = "FAC";
                config.FacultyPhoneTypeCode = "O";
                configRepoMock.Setup(repo => repo.GetStudentConfigurationAsync()).Returns(Task.FromResult(config));

                // Mock term repo response
                term1 = new Domain.Student.Entities.Term("2011/FA", "Fall 2011", new DateTime(2011, 9, 1), new DateTime(2011, 12, 25), 2011, 1, true, true, "2011RT", true);
                term2 = new Domain.Student.Entities.Term("2012/SP", "Spring 2012", new DateTime(2012, 1, 5), new DateTime(2012, 5, 30), 2011, 2, true, true, "2011RT", true);
                term3 = new Domain.Student.Entities.Term("2012/SU", "Summer 2012", new DateTime(2012, 6, 4), new DateTime(2012, 8, 25), 2012, 1, true, true, "2012RT", true);
                IEnumerable<Domain.Student.Entities.Term> termList = new List<Domain.Student.Entities.Term>() { term1, term2, term3 };
                termRepoMock.Setup(repo => repo.GetRegistrationTermsAsync()).Returns(Task.FromResult(termList));

                // Mock section repo response
                ICollection<OfferingDepartment> depts = new List<OfferingDepartment>() { new OfferingDepartment("D1", 50m), new OfferingDepartment("D2", 50m) };
                ICollection<string> clcs = new List<string>() { "100", "200", "300", "400" };
                List<SectionStatusItem> statuses = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddYears(-5)) };
                // sections 1 -- 6 go in term 1 for range testing with test range start and end of 9/5 and 12/15, 
                // and the test faculty ("0000001") assigned to all sections
                // testing for overlap of time periods
                // period           start       end
                // section 1  <---> |            |        - runs  9/1  -  9/4,  excluded
                // section 2  <-----|----->      |        - runs  9/1  - 11/15, included
                // section 3        |   <----->  |        - runs 10/5  - 11/15, included
                // section 4        |      <-----|----->  - runs 11/5  - 12/25, included
                // section 5        |            |  <-->  - runs 12/17 - 12/25, excluded
                // section 6  <-----|------------|----->  - runs  9/1  - 12/25, included
                // Setup the sections in term1
                section1 = new Domain.Student.Entities.Section("11", "11", "01", new DateTime(2011, 9, 1), 3, null, "Section1 Title", "IN", depts, clcs, "UG", statuses, false, true, false, true, false);
                section1.TermId = "2011/FA"; section1.EndDate = new DateTime(2011, 9, 4); section1.AddFaculty("0000001");
                // section 2 overlap start before test start date, ends before test end date
                section2 = new Domain.Student.Entities.Section("12", "11", "02", new DateTime(2011, 9, 1), 3, null, "Section2 Title", "IN", depts, clcs, "UG", statuses, false, true, false, true, false);
                section2.TermId = "2011/FA"; section2.EndDate = new DateTime(2011, 11, 15); section2.AddFaculty("0000001");
                // section 3 start after test start date and end before test end date
                section3 = new Domain.Student.Entities.Section("13", "12", "01", new DateTime(2011, 10, 5), 3, null, "Section3 Title", "IN", depts, clcs, "UG", statuses, false, true, false, true, false);
                section3.TermId = "2011/FA"; section3.EndDate = new DateTime(2011, 11, 15); section3.AddFaculty("0000001");
                // section 4 start after test start date and before test end date and end after test end date
                section4 = new Domain.Student.Entities.Section("14", "12", "02", new DateTime(2011, 11, 5), 3, null, "Section4 Title", "IN", depts, clcs, "UG", statuses, false, true, false, true, false);
                section4.TermId = "2011/FA"; section4.EndDate = new DateTime(2011, 12, 25); section4.AddFaculty("0000001");
                // section 5 start after test end date
                section5 = new Domain.Student.Entities.Section("15", "13", "01", new DateTime(2011, 12, 17), 3, null, "Section5 Title", "IN", depts, clcs, "UG", statuses, false, true, false, true, false);
                section5.TermId = "2011/FA"; section5.EndDate = new DateTime(2011, 12, 25); section5.AddFaculty("0000001");
                // section 6 start before test start date and end after test end date
                section6 = new Domain.Student.Entities.Section("16", "13", "02", new DateTime(2011, 9, 1), 3, null, "Section6 Title", "IN", depts, clcs, "UG", statuses, false, true, false, true, false);
                section6.TermId = "2011/FA"; section6.EndDate = new DateTime(2011, 12, 25); section6.AddFaculty("0000001");

                // sections 7 -- 9 go in term2, test faculty assigned to only 2 sections
                section7 = new Domain.Student.Entities.Section("21", "11", "01", new DateTime(2012, 1, 5), 3, null, "Section7 Title", "IN", depts, clcs, "UG", statuses, false, true, false, true, false);
                section7.TermId = "2012/SP"; section7.EndDate = new DateTime(2012, 5, 30); section7.AddFaculty("0000001");
                section8 = new Domain.Student.Entities.Section("22", "12", "02", new DateTime(2012, 1, 5), 3, null, "Section8 Title", "IN", depts, clcs, "UG", statuses, false, true, false, true, false);
                section8.TermId = "2012/SP"; section8.EndDate = new DateTime(2012, 5, 30); section8.AddFaculty("0000001");
                section9 = new Domain.Student.Entities.Section("23", "13", "03", new DateTime(2012, 1, 5), 3, null, "Section9 Title", "IN", depts, clcs, "UG", statuses, false, true, false, true, false);
                section9.TermId = "2012/SP"; section9.EndDate = new DateTime(2012, 5, 30); section9.AddFaculty("0000002");

                // sections 10 -- 12 go in term 3, test faculty assigned to no sections
                section10 = new Domain.Student.Entities.Section("31", "11", "02", new DateTime(2012, 6, 4), 3, null, "Section10 Title", "IN", depts, clcs, "UG", statuses, false, true, false, true, false);
                section10.TermId = "2012/SU"; section10.EndDate = new DateTime(2012, 8, 25); section10.AddFaculty("0000002");
                section11 = new Domain.Student.Entities.Section("32", "12", "01", new DateTime(2012, 6, 4), 3, null, "Section11 Title", "IN", depts, clcs, "UG", statuses, false, true, false, true, false);
                section11.TermId = "2012/SU"; section11.EndDate = new DateTime(2012, 8, 25); section11.AddFaculty("0000002");
                section12 = new Domain.Student.Entities.Section("33", "13", "01", new DateTime(2012, 6, 4), 3, null, "Section12 Title", "IN", depts, clcs, "UG", statuses, false, true, false, true, false);
                section12.TermId = "2012/SU"; section12.EndDate = new DateTime(2012, 8, 25); section12.AddFaculty("0000002");
                IEnumerable<Domain.Student.Entities.Section> sectionList = new List<Domain.Student.Entities.Section>() { section1, section2, section3, section4, section5, section6, section7, section8, section9, section10, section11, section12 };
                sectionRepoMock.Setup(repo => repo.GetRegistrationSectionsAsync(termList)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sectionList));

                // Mock Adapters
                var facultyDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Faculty, Ellucian.Colleague.Dtos.Student.Faculty>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Faculty, Ellucian.Colleague.Dtos.Student.Faculty>()).Returns(facultyDtoAdapter);
                var phoneDtoAdapter = new AutoMapperAdapter<Domain.Base.Entities.Phone, Dtos.Base.Phone>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Base.Entities.Phone, Dtos.Base.Phone>()).Returns(phoneDtoAdapter);
                var sectionDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.Section, Dtos.Student.Section2>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.Section, Dtos.Student.Section2>()).Returns(sectionDtoAdapter);

                // Instantiate faculty service with mocked items
                facultyService = new FacultyService(adapterRegistry, facultyRepo, configRepo, sectionRepo, termRepo, referenceRepo, studentReferenceRepo, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                facultyRepo = null;
                adapterRegistry = null;
                configRepo = null;
                termRepo = null;
                sectionRepo = null;
                logger = null;
            }

            [TestMethod]
            public async Task GetFacultySections2Async_ReturnsEmptyListForNoFacultyId()
            {
#pragma warning disable 618
                var result = await facultyService.GetFacultySections2Async(null, null, null, false);
#pragma warning restore 618
                Assert.IsNotNull(result);
                Assert.IsNotNull(result.Dto);
                Assert.AreEqual(0, result.Dto.Count());
            }

            [TestMethod]
            public async Task GetFacultySections2Async_ReturnsEmptyListForFacultyWithNoSections()
            {
#pragma warning disable 618
                var result = await facultyService.GetFacultySections2Async("0000003", null, null, false);
#pragma warning restore 618
                Assert.IsNotNull(result);
                Assert.IsNotNull(result.Dto);
                Assert.AreEqual(0, result.Dto.Count());
            }

            [TestMethod]
            public async Task GetFacultySections2Async_ReturnsAllFaculty1Sections()
            {
                // mainly setup validation
                DateTime start = new DateTime(2011, 8, 1);
                DateTime end = new DateTime(2012, 12, 31);
#pragma warning disable 618
                var result = await facultyService.GetFacultySections2Async("0000001", start, end, false);
#pragma warning restore 618
                Assert.IsNotNull(result);
                Assert.IsNotNull(result.Dto);
                Assert.AreEqual(8, result.Dto.Count());
            }

            [TestMethod]
            public async Task GetFacultySections2Async_ReturnsAllFaculty2Sections()
            {
                // mainly setup validation
                DateTime start = new DateTime(2011, 8, 1);
                DateTime end = new DateTime(2012, 12, 31);
#pragma warning disable 618
                var result = await facultyService.GetFacultySections2Async("0000002", start, end, false);
#pragma warning restore 618
                Assert.IsNotNull(result);
                Assert.IsNotNull(result.Dto);
                Assert.AreEqual(4, result.Dto.Count());
            }

            [TestMethod]
            public async Task GetFacultySections2Async_ReturnsAllFaculty1FallSections()
            {
                DateTime start = new DateTime(2011, 8, 1);
                DateTime end = new DateTime(2011, 12, 31);
#pragma warning disable 618
                var result = await facultyService.GetFacultySections2Async("0000001", start, end, false);
#pragma warning restore 618
                Assert.AreEqual(6, result.Dto.Count());
            }

            [TestMethod]
            public async Task GetFacultySections2Async_ReturnsSixFaculty1FallSections()
            {
                DateTime start = new DateTime(2011, 9, 5);
                DateTime end = new DateTime(2011, 12, 15);
#pragma warning disable 618
                var result = await facultyService.GetFacultySections2Async("0000001", start, end, false);
#pragma warning restore 618
                Assert.IsNotNull(result);
                Assert.IsNotNull(result.Dto);
                // should omit section1, and section5
                Assert.AreEqual(4, result.Dto.Count());
            }

            [TestMethod]
            public async Task GetFacultySections2Async_ReturnsTwoFaculty1SpringSections()
            {
                DateTime start = new DateTime(2012, 1, 5);
                DateTime end = new DateTime(2012, 5, 30);
#pragma warning disable 618
                var result = await facultyService.GetFacultySections2Async("0000001", start, end, false);
#pragma warning restore 618
                Assert.IsNotNull(result);
                Assert.IsNotNull(result.Dto);
                Assert.AreEqual(2, result.Dto.Count());
            }

            [TestMethod]
            public async Task GetFacultySections2Async_ReturnsZeroFaculty1SummerSections()
            {
                DateTime start = new DateTime(2012, 6, 4);
                DateTime end = new DateTime(2012, 8, 25);
#pragma warning disable 618
                var result = await facultyService.GetFacultySections2Async("0000001", start, end, false);
#pragma warning restore 618
                Assert.IsNotNull(result);
                Assert.IsNotNull(result.Dto);
                Assert.AreEqual(0, result.Dto.Count());
            }

            [TestMethod]
            public async Task GetFacultySections2Async_ReturnsFiveFaculty1FallSpringSections()
            {
                DateTime start = new DateTime(2011, 11, 16);
                // null end date maps to 11/16/2011 + 90 days
#pragma warning disable 618
                var result = await facultyService.GetFacultySections2Async("0000001", start, null, false);
#pragma warning restore 618
                Assert.IsNotNull(result);
                Assert.IsNotNull(result.Dto);
                Assert.AreEqual(5, result.Dto.Count());
            }

            [TestMethod]
            public async Task GetFacultySections2Async_EnsuresStartEndOrder()
            {
                DateTime start = new DateTime(2011, 11, 16);
                DateTime end = new DateTime(2011, 10, 15);
                // end before start is discarded, and end is assigned start + 90 days
#pragma warning disable 618
                var result = await facultyService.GetFacultySections2Async("0000001", start, end, false);
#pragma warning restore 618
                Assert.IsNotNull(result);
                Assert.IsNotNull(result.Dto);
                Assert.AreEqual(5, result.Dto.Count());
            }

            [TestMethod]
            public async Task GetFacultySections2Async_result_has_no_Privacy_Restrictions()
            {
                DateTime start = new DateTime(2011, 11, 16);
                DateTime end = new DateTime(2011, 10, 15);
                IEnumerable<Domain.Student.Entities.Section> sectionList = new List<Domain.Student.Entities.Section>() { section1, section2, section3, section4, section5, section6, section7, section8, section9, section10, section11, section12 };
                // Assign user as faculty on every section returned by repo call
                foreach (var section in sectionList)
                {
                    section.AddFaculty(currentUserFactory.CurrentUser.PersonId);
                }
                sectionRepoMock.Setup(repo => repo.GetRegistrationSectionsAsync(It.IsAny<IEnumerable<Domain.Student.Entities.Term>>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sectionList));
                facultyService = new FacultyService(adapterRegistry, facultyRepo, configRepo, sectionRepo, termRepo, referenceRepo, studentReferenceRepo, currentUserFactory, roleRepo, logger);

                // end before start is discarded, and end is assigned start + 90 days
#pragma warning disable 618
                var result = await facultyService.GetFacultySections2Async("0000001", start, end, false);
#pragma warning restore 618
                Assert.IsNotNull(result);
                Assert.IsNotNull(result.Dto);
                Assert.AreEqual(5, result.Dto.Count());
                // Since user is faculty on every section returned by repo, none has a privacy restriction
                Assert.IsFalse(result.HasPrivacyRestrictions);
            }

            [TestMethod]
            public async Task GetFacultySections2Async_result_has_Privacy_Restrictions()
            {
                DateTime start = new DateTime(2011, 11, 16);
                DateTime end = new DateTime(2011, 10, 15);
                IEnumerable<Domain.Student.Entities.Section> sectionList = new List<Domain.Student.Entities.Section>() { section1, section2, section3, section4, section5, section6, section7, section8, section9, section10, section11, section12 };
                // Only sections where user is not assigned faculty
                sectionList = sectionList.Where(s => !s.FacultyIds.Contains(currentUserFactory.CurrentUser.PersonId)).ToList();
                sectionRepoMock.Setup(repo => repo.GetRegistrationSectionsAsync(It.IsAny<IEnumerable<Domain.Student.Entities.Term>>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sectionList));
                facultyService = new FacultyService(adapterRegistry, facultyRepo, configRepo, sectionRepo, termRepo, referenceRepo, studentReferenceRepo, currentUserFactory, roleRepo, logger);

                // end before start is discarded, and end is assigned start + 90 days
#pragma warning disable 618
                var result = await facultyService.GetFacultySections2Async("0000001", start, end, false);
#pragma warning restore 618
                Assert.IsNotNull(result);
                Assert.IsNotNull(result.Dto);
                // Since user is faculty on every section returned by repo, none has a privacy restriction
                Assert.IsTrue(result.HasPrivacyRestrictions);
                Assert.AreEqual(0, result.Dto.ElementAt(0).ActiveStudentIds.Count());
                Assert.AreEqual(0, result.Dto.ElementAt(1).ActiveStudentIds.Count());
                Assert.AreEqual(0, result.Dto.ElementAt(2).ActiveStudentIds.Count());
                Assert.AreEqual(0, result.Dto.ElementAt(3).ActiveStudentIds.Count());
            }
        }

        [TestClass]
        public class GetFacultySections3Async_Tests
        {
            private FacultyService facultyService;
            private Mock<IFacultyRepository> facultyRepoMock;
            private IFacultyRepository facultyRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private Mock<IStudentConfigurationRepository> configRepoMock;
            private IStudentConfigurationRepository configRepo;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IReferenceDataRepository> referenceRepoMock;
            private IReferenceDataRepository referenceRepo;
            private Mock<IStudentReferenceDataRepository> studentReferenceRepoMock;
            private IStudentReferenceDataRepository studentReferenceRepo;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private string faculty1Id;
            private Domain.Student.Entities.Faculty faculty1;
            private string faculty2Id;
            private Domain.Student.Entities.Faculty faculty2;
            private Domain.Student.Entities.Section section1;
            private Domain.Student.Entities.Section section2;
            private Domain.Student.Entities.Section section3;
            private Domain.Student.Entities.Section section4;
            private Domain.Student.Entities.Section section5;
            private Domain.Student.Entities.Section section6;
            private Domain.Student.Entities.Section section7;
            private Domain.Student.Entities.Section section8;
            private Domain.Student.Entities.Section section9;
            private Domain.Student.Entities.Section section10;
            private Domain.Student.Entities.Section section11;
            private Domain.Student.Entities.Section section12;
            private Domain.Student.Entities.Term term1;
            private Domain.Student.Entities.Term term2;
            private Domain.Student.Entities.Term term3;
            private StudentConfiguration config;

            [TestInitialize]
            public async void Initialize()
            {
                facultyRepoMock = new Mock<IFacultyRepository>();
                facultyRepo = facultyRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                configRepoMock = new Mock<IStudentConfigurationRepository>();
                configRepo = configRepoMock.Object;
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                referenceRepoMock = new Mock<IReferenceDataRepository>();
                referenceRepo = referenceRepoMock.Object;
                studentReferenceRepoMock = new Mock<IStudentReferenceDataRepository>();
                studentReferenceRepo = studentReferenceRepoMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;

                // Set up current user
                currentUserFactory = new CurrentUserSetup.FacultyUserFactory();

                // Mock faculty repo response
                faculty1Id = "0000002";
                faculty1 = new Domain.Student.Entities.Faculty(faculty1Id, "Quincy");
                faculty1.AddEmailAddress(new EmailAddress("xyz@xmail.com", "FAC"));
                faculty1.AddPhone(new Phone("864-123-4564", "O"));
                facultyRepoMock.Setup(repo => repo.GetAsync(faculty1Id)).Returns(Task.FromResult(faculty1));

                // Mock faculty repo response
                faculty2Id = "0000004";
                faculty2 = new Domain.Student.Entities.Faculty(faculty2Id, "Jones");
                faculty2.AddEmailAddress(new EmailAddress("abc@xmail.com", "FAC"));
                faculty2.AddPhone(new Phone("864-123-9998", "O"));
                facultyRepoMock.Setup(repo => repo.GetAsync(faculty2Id)).Returns(Task.FromResult(faculty2));

                // Mock config repo response
                config = new StudentConfiguration();
                config.FacultyEmailTypeCode = "FAC";
                config.FacultyPhoneTypeCode = "O";
                configRepoMock.Setup(repo => repo.GetStudentConfigurationAsync()).Returns(Task.FromResult(config));

                // Mock term repo response
                term1 = new Domain.Student.Entities.Term("2011/FA", "Fall 2011", new DateTime(2011, 9, 1), new DateTime(2011, 12, 25), 2011, 1, true, true, "2011RT", true);
                term2 = new Domain.Student.Entities.Term("2012/SP", "Spring 2012", new DateTime(2012, 1, 5), new DateTime(2012, 5, 30), 2011, 2, true, true, "2011RT", true);
                term3 = new Domain.Student.Entities.Term("2012/SU", "Summer 2012", new DateTime(2012, 6, 4), new DateTime(2012, 8, 25), 2012, 1, true, true, "2012RT", true);
                IEnumerable<Domain.Student.Entities.Term> termList = new List<Domain.Student.Entities.Term>() { term1, term2, term3 };
                termRepoMock.Setup(repo => repo.GetRegistrationTermsAsync()).Returns(Task.FromResult(termList));

                // Mock section repo response
                ICollection<OfferingDepartment> depts = new List<OfferingDepartment>() { new OfferingDepartment("D1", 50m), new OfferingDepartment("D2", 50m) };
                ICollection<string> clcs = new List<string>() { "100", "200", "300", "400" };
                List<SectionStatusItem> statuses = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddYears(-5)) };
                // sections 1 -- 6 go in term 1 for range testing with test range start and end of 9/5 and 12/15, 
                // and the test faculty ("0000001") assigned to all sections
                // testing for overlap of time periods
                // period           start       end
                // section 1  <---> |            |        - runs  9/1  -  9/4,  excluded
                // section 2  <-----|----->      |        - runs  9/1  - 11/15, included
                // section 3        |   <----->  |        - runs 10/5  - 11/15, included
                // section 4        |      <-----|----->  - runs 11/5  - 12/25, included
                // section 5        |            |  <-->  - runs 12/17 - 12/25, excluded
                // section 6  <-----|------------|----->  - runs  9/1  - 12/25, included
                // Setup the sections in term1
                section1 = new Domain.Student.Entities.Section("11", "11", "01", new DateTime(2011, 9, 1), 3, null, "Section1 Title", "IN", depts, clcs, "UG", statuses, false, true, false, true, false);
                section1.TermId = "2011/FA"; section1.EndDate = new DateTime(2011, 9, 4); section1.AddFaculty("0000001");
                // section 2 overlap start before test start date, ends before test end date
                section2 = new Domain.Student.Entities.Section("12", "11", "02", new DateTime(2011, 9, 1), 3, null, "Section3 Title", "IN", depts, clcs, "UG", statuses, false, true, false, true, false);
                section2.TermId = "2011/FA"; section2.EndDate = new DateTime(2011, 11, 15); section2.AddFaculty("0000001");
                // section 3 start after test start date and end before test end date
                section3 = new Domain.Student.Entities.Section("13", "12", "01", new DateTime(2011, 10, 5), 3, null, "Section3 Title", "IN", depts, clcs, "UG", statuses, false, true, false, true, false);
                section3.TermId = "2011/FA"; section3.EndDate = new DateTime(2011, 11, 15); section3.AddFaculty("0000001");
                // section 4 start after test start date and before test end date and end after test end date
                section4 = new Domain.Student.Entities.Section("14", "12", "02", new DateTime(2011, 11, 5), 3, null, "Section4 Title", "IN", depts, clcs, "UG", statuses, false, true, false, true, false);
                section4.TermId = "2011/FA"; section4.EndDate = new DateTime(2011, 12, 25); section4.AddFaculty("0000001");
                // section 5 start after test end date
                section5 = new Domain.Student.Entities.Section("15", "13", "01", new DateTime(2011, 12, 17), 3, null, "Section5 Title", "IN", depts, clcs, "UG", statuses, false, true, false, true, false);
                section5.TermId = "2011/FA"; section5.EndDate = new DateTime(2011, 12, 25); section5.AddFaculty("0000001");
                // section 6 start before test start date and end after test end date
                section6 = new Domain.Student.Entities.Section("16", "13", "02", new DateTime(2011, 9, 1), 3, null, "Section6 Title", "IN", depts, clcs, "UG", statuses, false, true, false, true, false);
                section6.TermId = "2011/FA"; section6.EndDate = new DateTime(2011, 12, 25); section6.AddFaculty("0000001");

                // sections 7 -- 9 go in term2, test faculty assigned to only 2 sections
                section7 = new Domain.Student.Entities.Section("21", "11", "01", new DateTime(2012, 1, 5), 3, null, "Section7 Title", "IN", depts, clcs, "UG", statuses, false, true, false, true, false);
                section7.TermId = "2012/SP"; section7.EndDate = new DateTime(2012, 5, 30); section7.AddFaculty("0000001");
                section8 = new Domain.Student.Entities.Section("22", "12", "02", new DateTime(2012, 1, 5), 3, null, "Section8 Title", "IN", depts, clcs, "UG", statuses, false, true, false, true, false);
                section8.TermId = "2012/SP"; section8.EndDate = new DateTime(2012, 5, 30); section8.AddFaculty("0000001");
                section9 = new Domain.Student.Entities.Section("23", "13", "03", new DateTime(2012, 1, 5), 3, null, "Section9 Title", "IN", depts, clcs, "UG", statuses, false, true, false, true, false);
                section9.TermId = "2012/SP"; section9.EndDate = new DateTime(2012, 5, 30); section9.AddFaculty("0000002");

                // sections 10 -- 12 go in term 3, test faculty assigned to no sections
                section10 = new Domain.Student.Entities.Section("31", "11", "02", new DateTime(2012, 6, 4), 3, null, "Section10 Title", "IN", depts, clcs, "UG", statuses, false, true, false, true, false);
                section10.TermId = "2012/SU"; section10.EndDate = new DateTime(2012, 8, 25); section10.AddFaculty("0000002");
                section11 = new Domain.Student.Entities.Section("32", "12", "01", new DateTime(2012, 6, 4), 3, null, "Section11 Title", "IN", depts, clcs, "UG", statuses, false, true, false, true, false);
                section11.TermId = "2012/SU"; section11.EndDate = new DateTime(2012, 8, 25); section11.AddFaculty("0000002");
                section12 = new Domain.Student.Entities.Section("33", "13", "01", new DateTime(2012, 6, 4), 3, null, "Section12 Title", "IN", depts, clcs, "UG", statuses, false, true, false, true, false);
                section12.TermId = "2012/SU"; section12.EndDate = new DateTime(2012, 8, 25); section12.AddFaculty("0000002");
                IEnumerable<Domain.Student.Entities.Section> sectionList = new List<Domain.Student.Entities.Section>() { section1, section2, section3, section4, section5, section6, section7, section8, section9, section10, section11, section12 };
                sectionRepoMock.Setup(repo => repo.GetRegistrationSectionsAsync(termList)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sectionList));

                // Mock Adapters
                var facultyDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Faculty, Ellucian.Colleague.Dtos.Student.Faculty>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Faculty, Ellucian.Colleague.Dtos.Student.Faculty>()).Returns(facultyDtoAdapter);
                var phoneDtoAdapter = new AutoMapperAdapter<Domain.Base.Entities.Phone, Dtos.Base.Phone>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Base.Entities.Phone, Dtos.Base.Phone>()).Returns(phoneDtoAdapter);
                var sectionDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.Section, Dtos.Student.Section3>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.Section, Dtos.Student.Section3>()).Returns(sectionDtoAdapter);

                // Instantiate faculty service with mocked items
                facultyService = new FacultyService(adapterRegistry, facultyRepo, configRepo, sectionRepo, termRepo, referenceRepo, studentReferenceRepo, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                facultyRepo = null;
                adapterRegistry = null;
                configRepo = null;
                termRepo = null;
                sectionRepo = null;
                logger = null;
            }

            [TestMethod]
            public async Task GetFacultySections3Async_ReturnsEmptyListForNoFacultyId()
            {
#pragma warning disable 618
                var result = await facultyService.GetFacultySections3Async(null, null, null, false);
#pragma warning restore 618
                Assert.IsNotNull(result);
                Assert.IsNotNull(result.Dto);
                Assert.AreEqual(0, result.Dto.Count());
            }

            [TestMethod]
            public async Task GetFacultySections3Async_ReturnsEmptyListForFacultyWithNoSections()
            {
#pragma warning disable 618
                var result = await facultyService.GetFacultySections3Async("0000003", null, null, false);
#pragma warning restore 618
                Assert.IsNotNull(result);
                Assert.IsNotNull(result.Dto);
                Assert.AreEqual(0, result.Dto.Count());
            }

            [TestMethod]
            public async Task GetFacultySections3Async_ReturnsAllFaculty1Sections()
            {
                // mainly setup validation
                DateTime start = new DateTime(2011, 8, 1);
                DateTime end = new DateTime(2012, 12, 31);
#pragma warning disable 618
                var result = await facultyService.GetFacultySections3Async("0000001", start, end, false);
#pragma warning restore 618
                Assert.IsNotNull(result);
                Assert.IsNotNull(result.Dto);
                Assert.AreEqual(8, result.Dto.Count());
            }

            [TestMethod]
            public async Task GetFacultySections3Async_ReturnsAllFaculty2Sections()
            {
                // mainly setup validation
                DateTime start = new DateTime(2011, 8, 1);
                DateTime end = new DateTime(2012, 12, 31);
#pragma warning disable 618
                var result = await facultyService.GetFacultySections3Async("0000002", start, end, false);
#pragma warning restore 618
                Assert.IsNotNull(result);
                Assert.IsNotNull(result.Dto);
                Assert.AreEqual(4, result.Dto.Count());
            }

            [TestMethod]
            public async Task GetFacultySections3Async_ReturnsAllFaculty1FallSections()
            {
                DateTime start = new DateTime(2011, 8, 1);
                DateTime end = new DateTime(2011, 12, 31);
#pragma warning disable 618
                var result = await facultyService.GetFacultySections3Async("0000001", start, end, false);
#pragma warning restore 618
                Assert.AreEqual(6, result.Dto.Count());
            }

            [TestMethod]
            public async Task GetFacultySections3Async_ReturnsSixFaculty1FallSections()
            {
                DateTime start = new DateTime(2011, 9, 5);
                DateTime end = new DateTime(2011, 12, 15);
#pragma warning disable 618
                var result = await facultyService.GetFacultySections3Async("0000001", start, end, false);
#pragma warning restore 618
                Assert.IsNotNull(result);
                Assert.IsNotNull(result.Dto);
                // should omit section1, and section5
                Assert.AreEqual(4, result.Dto.Count());
            }

            [TestMethod]
            public async Task GetFacultySections3Async_ReturnsTwoFaculty1SpringSections()
            {
                DateTime start = new DateTime(2012, 1, 5);
                DateTime end = new DateTime(2012, 5, 30);
#pragma warning disable 618
                var result = await facultyService.GetFacultySections3Async("0000001", start, end, false);
#pragma warning restore 618
                Assert.IsNotNull(result);
                Assert.IsNotNull(result.Dto);
                Assert.AreEqual(2, result.Dto.Count());
            }

            [TestMethod]
            public async Task GetFacultySections3Async_ReturnsZeroFaculty1SummerSections()
            {
                DateTime start = new DateTime(2012, 6, 4);
                DateTime end = new DateTime(2012, 8, 25);
#pragma warning disable 618
                var result = await facultyService.GetFacultySections3Async("0000001", start, end, false);
#pragma warning restore 618
                Assert.IsNotNull(result);
                Assert.IsNotNull(result.Dto);
                Assert.AreEqual(0, result.Dto.Count());
            }

            [TestMethod]
            public async Task GetFacultySections3Async_ReturnsFiveFaculty1FallSpringSections()
            {
                DateTime start = new DateTime(2011, 11, 16);
                // null end date maps to 11/16/2011 + 90 days
#pragma warning disable 618
                var result = await facultyService.GetFacultySections3Async("0000001", start, null, false);
#pragma warning restore 618
                Assert.IsNotNull(result);
                Assert.IsNotNull(result.Dto);
                Assert.AreEqual(5, result.Dto.Count());
            }

            [TestMethod]
            public async Task GetFacultySections3Async_EnsuresStartEndOrder()
            {
                DateTime start = new DateTime(2011, 11, 16);
                DateTime end = new DateTime(2011, 10, 15);
                // end before start is discarded, and end is assigned start + 90 days
#pragma warning disable 618
                var result = await facultyService.GetFacultySections3Async("0000001", start, end, false);
#pragma warning restore 618
                Assert.IsNotNull(result);
                Assert.IsNotNull(result.Dto);
                Assert.AreEqual(5, result.Dto.Count());
            }

            [TestMethod]
            public async Task GetFacultySections3Async_result_has_no_Privacy_Restrictions()
            {
                DateTime start = new DateTime(2011, 11, 16);
                DateTime end = new DateTime(2011, 10, 15);
                IEnumerable<Domain.Student.Entities.Section> sectionList = new List<Domain.Student.Entities.Section>() { section1, section2, section3, section4, section5, section6, section7, section8, section9, section10, section11, section12 };
                // Assign user as faculty on every section returned by repo call
                foreach (var section in sectionList)
                {
                    section.AddFaculty(currentUserFactory.CurrentUser.PersonId);
                }
                sectionRepoMock.Setup(repo => repo.GetRegistrationSectionsAsync(It.IsAny<IEnumerable<Domain.Student.Entities.Term>>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sectionList));
                facultyService = new FacultyService(adapterRegistry, facultyRepo, configRepo, sectionRepo, termRepo, referenceRepo, studentReferenceRepo, currentUserFactory, roleRepo, logger);

                // end before start is discarded, and end is assigned start + 90 days
#pragma warning disable 618
                var result = await facultyService.GetFacultySections3Async("0000001", start, end, false);
#pragma warning restore 618
                Assert.IsNotNull(result);
                Assert.IsNotNull(result.Dto);
                Assert.AreEqual(5, result.Dto.Count());
                // Since user is faculty on every section returned by repo, none has a privacy restriction
                Assert.IsFalse(result.HasPrivacyRestrictions);
            }

            [TestMethod]
            public async Task GetFacultySections3Async_result_has_Privacy_Restrictions()
            {
                DateTime start = new DateTime(2011, 11, 16);
                DateTime end = new DateTime(2011, 10, 15);
                IEnumerable<Domain.Student.Entities.Section> sectionList = new List<Domain.Student.Entities.Section>() { section1, section2, section3, section4, section5, section6, section7, section8, section9, section10, section11, section12 };
                // Only sections where user is not assigned faculty
                sectionList = sectionList.Where(s => !s.FacultyIds.Contains(currentUserFactory.CurrentUser.PersonId)).ToList();
                sectionRepoMock.Setup(repo => repo.GetRegistrationSectionsAsync(It.IsAny<IEnumerable<Domain.Student.Entities.Term>>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sectionList));
                facultyService = new FacultyService(adapterRegistry, facultyRepo, configRepo, sectionRepo, termRepo, referenceRepo, studentReferenceRepo, currentUserFactory, roleRepo, logger);

                // end before start is discarded, and end is assigned start + 90 days
#pragma warning disable 618
                var result = await facultyService.GetFacultySections3Async("0000001", start, end, false);
#pragma warning restore 618
                Assert.IsNotNull(result);
                Assert.IsNotNull(result.Dto);
                // Since user is faculty on every section returned by repo, none has a privacy restriction
                Assert.IsTrue(result.HasPrivacyRestrictions);
                Assert.AreEqual(0, result.Dto.ElementAt(0).ActiveStudentIds.Count());
                Assert.AreEqual(0, result.Dto.ElementAt(1).ActiveStudentIds.Count());
                Assert.AreEqual(0, result.Dto.ElementAt(2).ActiveStudentIds.Count());
                Assert.AreEqual(0, result.Dto.ElementAt(3).ActiveStudentIds.Count());
            }
        }

        [TestClass]
        public class GetFacultySections3_SectionsOutsideRegistrationTerms
        {
            // variant on GetFacultySections3 to test new case where an ongoing term
            // has been removed from the list of active registration terms (after end of
            // drop period, but before term is over)
            private FacultyService facultyService;
            private Mock<IFacultyRepository> facultyRepoMock;
            private IFacultyRepository facultyRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private Mock<IStudentConfigurationRepository> configRepoMock;
            private IStudentConfigurationRepository configRepo;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IReferenceDataRepository> refRepoMock;
            private IReferenceDataRepository refRepo;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private ILogger logger;
            private string faculty1Id;
            private Domain.Student.Entities.Faculty faculty1;
            private string faculty2Id;
            private Domain.Student.Entities.Faculty faculty2;
            private Domain.Student.Entities.Section section1;
            private Domain.Student.Entities.Section section2;
            private Domain.Student.Entities.Section section3;
            private Domain.Student.Entities.Section section4;
            private Domain.Student.Entities.Section section5;
            private Domain.Student.Entities.Section section6;
            private Domain.Student.Entities.Section section7;
            private Domain.Student.Entities.Term term1;
            private Domain.Student.Entities.Term term2;
            private Domain.Student.Entities.Term term3;
            private Domain.Student.Entities.Term term4;
            private Domain.Student.Entities.Term term5;
            private Domain.Student.Entities.Term term6;
            private Domain.Student.Entities.Term term7;
            private StudentConfiguration config;
            private IEnumerable<Domain.Student.Entities.Term> regTermList;

            [TestInitialize]
            public void Initialize()
            {
                facultyRepoMock = new Mock<IFacultyRepository>();
                facultyRepo = facultyRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                configRepoMock = new Mock<IStudentConfigurationRepository>();
                configRepo = configRepoMock.Object;
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                refRepoMock = new Mock<IReferenceDataRepository>();
                refRepo = refRepoMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;

                // Set up current user
                currentUserFactory = new CurrentUserSetup.FacultyUserFactory();

                // Mock faculty repo response
                faculty1Id = "0000002";
                faculty1 = new Domain.Student.Entities.Faculty(faculty1Id, "Quincy");
                faculty1.AddEmailAddress(new EmailAddress("xyz@xmail.com", "FAC"));
                faculty1.AddPhone(new Phone("864-123-4564", "O"));
                facultyRepoMock.Setup(repo => repo.GetAsync(faculty1Id)).ReturnsAsync(faculty1);

                // Mock faculty repo response
                faculty2Id = "0000004";
                faculty2 = new Domain.Student.Entities.Faculty(faculty2Id, "Jones");
                faculty2.AddEmailAddress(new EmailAddress("abc@xmail.com", "FAC"));
                faculty2.AddPhone(new Phone("864-123-9998", "O"));
                facultyRepoMock.Setup(repo => repo.GetAsync(faculty2Id)).Returns(Task.FromResult(faculty2));

                // Mock config repo response
                config = new StudentConfiguration();
                config.FacultyEmailTypeCode = "FAC";
                config.FacultyPhoneTypeCode = "O";
                configRepoMock.Setup(repo => repo.GetStudentConfigurationAsync()).Returns(Task.FromResult(config));

                // Mock term repo response - several terms to make sure we catch appropriate terms by date
                term1 = new Domain.Student.Entities.Term("T1", "T1", new DateTime(2011, 1, 1), new DateTime(2011, 1, 20), 2011, 1, true, true, "T1", true);
                term2 = new Domain.Student.Entities.Term("T2", "T2", new DateTime(2011, 2, 1), new DateTime(2011, 2, 20), 2011, 1, true, true, "T2", true);
                term3 = new Domain.Student.Entities.Term("T3", "T3", new DateTime(2011, 3, 1), new DateTime(2011, 3, 20), 2011, 2, true, true, "T3", true);
                term4 = new Domain.Student.Entities.Term("T4", "T4", new DateTime(2011, 1, 1), new DateTime(2011, 2, 15), 2011, 1, true, true, "T4", true);
                term5 = new Domain.Student.Entities.Term("T5", "T5", new DateTime(2011, 2, 17), new DateTime(2011, 3, 20), 2011, 1, true, true, "T5", true);
                term6 = new Domain.Student.Entities.Term("T6", "T6", new DateTime(2011, 1, 1), new DateTime(2011, 3, 30), 2011, 1, true, true, "T6", true);
                term7 = new Domain.Student.Entities.Term("T7", "T7", new DateTime(2011, 3, 25), new DateTime(2011, 4, 30), 2011, 1, true, true, "T7", true);

                IEnumerable<Domain.Student.Entities.Term> termList = new List<Domain.Student.Entities.Term>() { term1, term2, term3, term4, term5, term6, term7 };
                termRepoMock.Setup(repo => repo.GetAsync()).ReturnsAsync(termList);

                regTermList = new List<Domain.Student.Entities.Term>() { term7 };
                termRepoMock.Setup(repo => repo.GetRegistrationTermsAsync()).Returns(Task.FromResult(regTermList));

                // Mock section repo responses
                ICollection<OfferingDepartment> depts = new List<OfferingDepartment>() { new OfferingDepartment("D1", 50m), new OfferingDepartment("D2", 50m) };
                ICollection<string> clcs = new List<string>() { "100", "200", "300", "400" };
                List<SectionStatusItem> statuses = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddYears(-5)) };
                // sections 1 -- 7 go in terms 1 -- 7
                section1 = new Domain.Student.Entities.Section("11", "11", "01", new DateTime(2011, 1, 1), 3, null, "Section1 Title", "IN", depts, clcs, "UG", statuses, true, false, true, false, true, false);
                section1.TermId = "T1"; section1.EndDate = new DateTime(2011, 1, 20); section1.AddFaculty("0000001");

                section2 = new Domain.Student.Entities.Section("12", "11", "02", new DateTime(2011, 2, 1), 3, null, "Section2 Title", "IN", depts, clcs, "UG", statuses, true, false, true, false, true, false);
                section2.TermId = "T2"; section2.EndDate = new DateTime(2011, 2, 20); section2.AddFaculty("0000001");

                section3 = new Domain.Student.Entities.Section("13", "12", "01", new DateTime(2011, 3, 1), 3, null, "Section3 Title", "IN", depts, clcs, "UG", statuses, true, false, true, false, true, false);
                section3.TermId = "T3"; section3.EndDate = new DateTime(2011, 3, 20); section3.AddFaculty("0000001");

                section4 = new Domain.Student.Entities.Section("14", "12", "02", new DateTime(2011, 1, 1), 3, null, "Section4 Title", "IN", depts, clcs, "UG", statuses, true, false, true, false, true, false);
                section4.TermId = "T4"; section4.EndDate = new DateTime(2011, 2, 15); section4.AddFaculty("0000001");

                section5 = new Domain.Student.Entities.Section("15", "13", "01", new DateTime(2011, 2, 17), 3, null, "Section5 Title", "IN", depts, clcs, "UG", statuses, true, false, true, false, true, false);
                section5.TermId = "T5"; section5.EndDate = new DateTime(2011, 3, 20); section5.AddFaculty("0000001");

                section6 = new Domain.Student.Entities.Section("16", "13", "02", new DateTime(2011, 1, 1), 3, null, "Section6 Title", "IN", depts, clcs, "UG", statuses, true, false, true, false, true, false);
                section6.TermId = "T6"; section6.EndDate = new DateTime(2011, 3, 30); section6.AddFaculty("0000001");

                section7 = new Domain.Student.Entities.Section("17", "13", "02", new DateTime(2011, 3, 25), 3, null, "Section7 Title", "IN", depts, clcs, "UG", statuses, true, false, true, false, true, false);
                section7.TermId = "T7"; section7.EndDate = new DateTime(2011, 4, 30); section7.AddFaculty("0000001");

                List<Domain.Student.Entities.Term> nonRegTerms = new List<Domain.Student.Entities.Term>() { term1, term2, term3, term4, term5, term6 };
                IEnumerable<Domain.Student.Entities.Section> nonRegTermSectionList = new List<Domain.Student.Entities.Section>() { section1, section2, section3, section4, section5, section6 };
                // when this endpoint was created it called this method without the optional bestfit argument and the default behavior was bestFit =true.
                sectionRepoMock.Setup(repo => repo.GetNonCachedFacultySectionsAsync(nonRegTerms, "0000001", false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(nonRegTermSectionList));

                IEnumerable<Domain.Student.Entities.Section> regTermSectionList = new List<Domain.Student.Entities.Section>() { section7 };
                sectionRepoMock.Setup(repo => repo.GetRegistrationSectionsAsync(regTermList)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(regTermSectionList));

                // Mock Adapters
                var facultyDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Faculty, Ellucian.Colleague.Dtos.Student.Faculty>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Faculty, Ellucian.Colleague.Dtos.Student.Faculty>()).Returns(facultyDtoAdapter);
                var phoneDtoAdapter = new AutoMapperAdapter<Domain.Base.Entities.Phone, Dtos.Base.Phone>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Base.Entities.Phone, Dtos.Base.Phone>()).Returns(phoneDtoAdapter);
                var sectionDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.Section, Dtos.Student.Section3>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.Section, Dtos.Student.Section3>()).Returns(sectionDtoAdapter);

                // Instantiate faculty service with mocked items
                facultyService = new FacultyService(adapterRegistry, facultyRepo, configRepo, sectionRepo, termRepo, refRepo, null, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                facultyRepo = null;
                adapterRegistry = null;
                configRepo = null;
                termRepo = null;
                sectionRepo = null;
                logger = null;
            }

            [TestMethod]
            public async Task GetFacultySections3_SectionsOutsideRegistrationTermsReturnsEmptyListForNoFacultyId()
            {
                var result = await facultyService.GetFacultySections3Async(null, null, null, false);
                Assert.AreEqual(0, result.Dto.Count());
            }

            [TestMethod]
            public async Task GetFacultySections3_SevenSectionsTotal()
            {
                // encompass all terms (reg and outside reg)
                DateTime start = new DateTime(2011, 1, 1);
                DateTime end = new DateTime(2011, 5, 1);
                var result = await facultyService.GetFacultySections3Async("0000001", start, end, false);
                Assert.AreEqual(7, result.Dto.Count());
            }

            [TestMethod]
            public async Task GetFacultySections3_SixSectionsOutsideRegistrationTerms()
            {
                // mainly setup validation
                DateTime start = new DateTime(2011, 1, 1);
                DateTime end = new DateTime(2011, 3, 20);
                // should match T1 -- T6
                List<Domain.Student.Entities.Term> terms = new List<Domain.Student.Entities.Term>() { term1, term2, term3, term4, term5, term6 };
                List<Domain.Student.Entities.Section> sections = new List<Domain.Student.Entities.Section>() { section1, section2, section3, section4, section5, section6 };
                IEnumerable<Domain.Student.Entities.Section> eSections = sections.AsEnumerable();
                // when this endpoint was created it called this method without the optional bestfit argument and the default behavior was bestFit =true.
                sectionRepoMock.Setup(repo => repo.GetNonCachedFacultySectionsAsync(terms, "0000001", false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(eSections));

                var result = (await facultyService.GetFacultySections3Async("0000001", start, end, false)).Dto.ToList();
                Assert.AreEqual(6, result.Count());
                Assert.AreEqual("T1", result[0].TermId); Assert.AreEqual("11", result[0].Id);
                Assert.AreEqual("T2", result[1].TermId); Assert.AreEqual("12", result[1].Id);
                Assert.AreEqual("T3", result[2].TermId); Assert.AreEqual("13", result[2].Id);
                Assert.AreEqual("T4", result[3].TermId); Assert.AreEqual("14", result[3].Id);
                Assert.AreEqual("T5", result[4].TermId); Assert.AreEqual("15", result[4].Id);
                Assert.AreEqual("T6", result[5].TermId); Assert.AreEqual("16", result[5].Id);
            }

            [TestMethod]
            public async Task GetFacultySections3_ThreeSectionsOutsideRegistrationTerms()
            {
                // dates to target non-Registration Terms
                DateTime start = new DateTime(2011, 1, 1);
                DateTime end = new DateTime(2011, 1, 25);
                // should match T1, T4, T6
                List<Domain.Student.Entities.Term> terms = new List<Domain.Student.Entities.Term>() { term1, term4, term6 };
                List<Domain.Student.Entities.Section> sections = new List<Domain.Student.Entities.Section>() { section1, section4, section6 };
                IEnumerable<Domain.Student.Entities.Section> eSections = sections.AsEnumerable();
                // when this endpoint was created it called this method without the optional bestfit argument and the default behavior was bestFit =true.
                sectionRepoMock.Setup(repo => repo.GetNonCachedFacultySectionsAsync(terms, "0000001", false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(eSections));

                var result = (await facultyService.GetFacultySections3Async("0000001", start, end, false)).Dto.ToList();
                Assert.AreEqual(3, result.Count());
                Assert.AreEqual("T1", result[0].TermId); Assert.AreEqual("11", result[0].Id);
                Assert.AreEqual("T4", result[1].TermId); Assert.AreEqual("14", result[1].Id);
                Assert.AreEqual("T6", result[2].TermId); Assert.AreEqual("16", result[2].Id);
            }

            [TestMethod]
            public async Task GetFacultySections3_FiveSectionsOutsideRegistrationTerms()
            {
                DateTime start = new DateTime(2011, 1, 1);
                DateTime end = new DateTime(2011, 2, 25);
                // should match T1, T2, T4, T5, T6
                List<Domain.Student.Entities.Term> terms = new List<Domain.Student.Entities.Term>() { term1, term2, term4, term5, term6 };
                List<Domain.Student.Entities.Section> sections = new List<Domain.Student.Entities.Section>() { section1, section2, section4, section5, section6 };
                IEnumerable<Domain.Student.Entities.Section> eSections = sections.AsEnumerable();
                // when this endpoint was created it called this method without the optional bestfit argument and the default behavior was bestFit =true.
                sectionRepoMock.Setup(repo => repo.GetNonCachedFacultySectionsAsync(terms, "0000001", false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(eSections));

                var result = (await facultyService.GetFacultySections3Async("0000001", start, end, false)).Dto.ToList();
                Assert.AreEqual(5, result.Count());
                Assert.AreEqual("T1", result[0].TermId); Assert.AreEqual("11", result[0].Id);
                Assert.AreEqual("T2", result[1].TermId); Assert.AreEqual("12", result[1].Id);
                Assert.AreEqual("T4", result[2].TermId); Assert.AreEqual("14", result[2].Id);
                Assert.AreEqual("T5", result[3].TermId); Assert.AreEqual("15", result[3].Id);
                Assert.AreEqual("T6", result[4].TermId); Assert.AreEqual("16", result[4].Id);
            }

            [TestMethod]
            public async Task GetFacultySections3_TwoSectionsOutsideRegistrationTerms()
            {
                DateTime start = new DateTime(2011, 2, 16);
                DateTime end = new DateTime(2011, 2, 16);
                // should match T2, T6
                List<Domain.Student.Entities.Term> terms = new List<Domain.Student.Entities.Term>() { term2, term6 };
                List<Domain.Student.Entities.Section> sections = new List<Domain.Student.Entities.Section>() { section2, section6 };
                IEnumerable<Domain.Student.Entities.Section> eSections = sections.AsEnumerable();
                // when this endpoint was created it called this method without the optional bestfit argument and the default behavior was bestFit =true.
                sectionRepoMock.Setup(repo => repo.GetNonCachedFacultySectionsAsync(terms, "0000001", false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(eSections));

                var result = (await facultyService.GetFacultySections3Async("0000001", start, end, false)).Dto.ToList();
                Assert.AreEqual(2, result.Count());
                Assert.AreEqual("T2", result[0].TermId); Assert.AreEqual("12", result[0].Id);
                Assert.AreEqual("T6", result[1].TermId); Assert.AreEqual("16", result[1].Id);
            }

            [TestMethod]
            public async Task GetFacultySections3_FourSectionsOutsideRegistrationTerms()
            {
                DateTime start = new DateTime(2011, 2, 17);
                DateTime end = new DateTime(2011, 3, 21);
                // should match T2, T3, T5, T6
                List<Domain.Student.Entities.Term> terms = new List<Domain.Student.Entities.Term>() { term2, term3, term5, term6 };
                List<Domain.Student.Entities.Section> sections = new List<Domain.Student.Entities.Section>() { section2, section3, section5, section6 };
                IEnumerable<Domain.Student.Entities.Section> eSections = sections.AsEnumerable();
                // when this endpoint was created it called this method without the optional bestfit argument and the default behavior was bestFit =true.
                sectionRepoMock.Setup(repo => repo.GetNonCachedFacultySectionsAsync(terms, "0000001", false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(eSections));

                var result = (await facultyService.GetFacultySections3Async("0000001", start, end, false)).Dto.ToList();
                Assert.AreEqual(4, result.Count());
                Assert.AreEqual("T2", result[0].TermId); Assert.AreEqual("12", result[0].Id);
                Assert.AreEqual("T3", result[1].TermId); Assert.AreEqual("13", result[1].Id);
                Assert.AreEqual("T5", result[2].TermId); Assert.AreEqual("15", result[2].Id);
                Assert.AreEqual("T6", result[3].TermId); Assert.AreEqual("16", result[3].Id);
            }

            [TestMethod]
            public async Task GetFacultySections3_UniqueListOfSectionsWithNoDups()
            {

                // Test to be sure that if the same sections are returned in the GetRegistrationSections AND in the GetNonCachedFacultySections that
                // duplicates are not returned.

                DateTime start = new DateTime(2011, 2, 16);
                DateTime end = new DateTime(2011, 2, 16);
                // should match T2, T6
                List<Domain.Student.Entities.Term> terms = new List<Domain.Student.Entities.Term>() { term2, term6 };
                List<Domain.Student.Entities.Section> sections = new List<Domain.Student.Entities.Section>() { section2, section6 };
                IEnumerable<Domain.Student.Entities.Section> eSections = sections.AsEnumerable();
                // when this endpoint was created it called this method without the optional bestfit argument and the default behavior was bestFit =true.
                sectionRepoMock.Setup(repo => repo.GetNonCachedFacultySectionsAsync(terms, "0000001", false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(eSections));
                sectionRepoMock.Setup(repo => repo.GetRegistrationSectionsAsync(regTermList)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(eSections));
                var result = (await facultyService.GetFacultySections3Async("0000001", start, end, false)).Dto.ToList();
                Assert.AreEqual(2, result.Count());
                Assert.AreEqual("T2", result[0].TermId); Assert.AreEqual("12", result[0].Id);
                Assert.AreEqual("T6", result[1].TermId); Assert.AreEqual("16", result[1].Id);
            }
        }


        [TestClass]
        public class Get
        {
            private FacultyService facultyService;
            private Mock<IFacultyRepository> facultyRepoMock;
            private IFacultyRepository facultyRepo;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IReferenceDataRepository> refRepoMock;
            private IReferenceDataRepository refRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private Mock<IStudentConfigurationRepository> configRepoMock;
            private IStudentConfigurationRepository configRepo;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private ILogger logger;
            private string faculty1Id;
            private Domain.Student.Entities.Faculty faculty1;
            private string faculty2Id;
            private Domain.Student.Entities.Faculty faculty2;
            private StudentConfiguration config;
            private List<AddressRelationType> addressRelationTypes;

            [TestInitialize]
            public async void Initialize()
            {
                facultyRepoMock = new Mock<IFacultyRepository>();
                facultyRepo = facultyRepoMock.Object;
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                refRepoMock = new Mock<IReferenceDataRepository>();
                refRepo = refRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                configRepoMock = new Mock<IStudentConfigurationRepository>();
                configRepo = configRepoMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;

                // Set up current user
                currentUserFactory = new CurrentUserSetup.FacultyUserFactory();

                // Mock faculty repo response
                faculty1Id = "0000002";
                faculty1 = new Domain.Student.Entities.Faculty(faculty1Id, "Quincy");
                faculty1.FirstName = "John";
                faculty1.MiddleName = "Arnold";
                faculty1.AddEmailAddress(new EmailAddress("xyz@xmail.com", "FAC"));
                faculty1.AddPhone(new Phone("864-123-4564", "O"));
                faculty1.ProfessionalName = "Dr Johnathan Barker DMD";
                facultyRepoMock.Setup(repo => repo.GetAsync(faculty1Id)).Returns(Task.FromResult(faculty1));

                // Mock faculty repo response
                faculty2Id = "0000004";
                faculty2 = new Domain.Student.Entities.Faculty(faculty2Id, "Jones");
                faculty2.FirstName = "William";
                faculty2.MiddleName = string.Empty;
                faculty2.ProfessionalName = null;
                faculty2.AddEmailAddress(new EmailAddress("abc@xmail.com", "FAC"));
                faculty2.AddPhone(new Phone("864-123-9998", "O"));
                facultyRepoMock.Setup(repo => repo.GetAsync(faculty2Id)).Returns(Task.FromResult(faculty2));

                // Mock config repo response
                config = new StudentConfiguration();
                config.FacultyEmailTypeCode = "FAC";
                config.FacultyPhoneTypeCode = "O";
                configRepoMock.Setup(repo => repo.GetStudentConfigurationAsync()).Returns(Task.FromResult(config));

                // Mock Reference Data Repository Response
                addressRelationTypes = new List<AddressRelationType>();
                addressRelationTypes.Add(new AddressRelationType("FAC", "Faculty", "FAC", "FAC"));
                refRepoMock.Setup<IEnumerable<Colleague.Domain.Base.Entities.AddressRelationType>>(repo => repo.AddressRelationTypes).Returns(addressRelationTypes);

                // Mock Adapters
                var facultyDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Faculty, Ellucian.Colleague.Dtos.Student.Faculty>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Faculty, Ellucian.Colleague.Dtos.Student.Faculty>()).Returns(facultyDtoAdapter);
                var phoneDtoAdapter = new AutoMapperAdapter<Domain.Base.Entities.Phone, Dtos.Base.Phone>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Base.Entities.Phone, Dtos.Base.Phone>()).Returns(phoneDtoAdapter);

                // Instantiate faculty service with mocked items
                facultyService = new FacultyService(adapterRegistry, facultyRepo, configRepo, sectionRepo, termRepo, refRepo, null, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                facultyRepo = null;
                adapterRegistry = null;
                configRepo = null;
                termRepo = null;
                sectionRepo = null;
                logger = null;
            }

            [TestMethod]
            public async Task GetSingle_AndCheckDto()
            {
                var facDto = await facultyService.GetAsync(faculty1Id);
                Assert.AreEqual(faculty1.Id, facDto.Id);
                Assert.AreEqual(faculty1.LastName, facDto.LastName);
                Assert.AreEqual(faculty1.MiddleName, facDto.MiddleName);
                Assert.AreEqual(faculty1.FirstName, facDto.FirstName);
                Assert.AreEqual(faculty1.GetFacultyEmailAddresses("FAC").First(), facDto.EmailAddresses.ElementAt(0));
                Assert.AreEqual(faculty1.GetFacultyPhones("O").First().Number, facDto.Phones.ElementAt(0).Number);
                Assert.AreEqual(faculty1.ProfessionalName, facDto.ProfessionalName);
            }

            [TestMethod]
            public async Task GetFacultyByIds()
            {
                // arrange
                facultyRepoMock.Setup<Task<IEnumerable<Colleague.Domain.Student.Entities.Faculty>>>(repo => repo.GetFacultyByIdsAsync(It.IsAny<IEnumerable<string>>())).Returns(Task.FromResult<IEnumerable<Colleague.Domain.Student.Entities.Faculty>>(new List<Colleague.Domain.Student.Entities.Faculty>() { faculty1, faculty2 }));
                // act
                var criteria = new FacultyQueryCriteria() { FacultyIds = new List<string>() { faculty1Id, faculty2Id }.AsEnumerable() };
                var facDtos = await facultyService.QueryFacultyAsync(new FacultyQueryCriteria() { FacultyIds = new List<string>() { faculty1Id, faculty2Id }.AsEnumerable() });

                // assert
                Assert.AreEqual(2, facDtos.Count());
                var fac1Dto = facDtos.Where(f => f.Id == faculty1.Id).FirstOrDefault();
                Assert.IsNotNull(fac1Dto);
                Assert.AreEqual(faculty1.LastName, fac1Dto.LastName);
                var fac2Dto = facDtos.Where(f => f.Id == faculty2.Id).FirstOrDefault();
                Assert.IsNotNull(fac2Dto);
                Assert.AreEqual(faculty2.GetFacultyEmailAddresses("FAC").First(), fac2Dto.EmailAddresses.First());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetFacultyByIds_ThrowsExceptionForNullIds()
            {
                // arrange
                List<string> facultyIds = null;
                facultyRepoMock.Setup<Task<IEnumerable<Colleague.Domain.Student.Entities.Faculty>>>(repo => repo.GetAsync(It.IsAny<IEnumerable<string>>())).Returns(Task.FromResult<IEnumerable<Colleague.Domain.Student.Entities.Faculty>>(new List<Colleague.Domain.Student.Entities.Faculty>() { faculty1, faculty2 }.AsEnumerable()));

                // act
                var criteria = new FacultyQueryCriteria() { FacultyIds = facultyIds };
                var facDtos = await facultyService.QueryFacultyAsync(criteria);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetFacultyByIds_ThrowsExceptionForEmptyIds()
            {
                // arrange
                List<string> facultyIds = new List<string>();
                facultyRepoMock.Setup<Task<IEnumerable<Colleague.Domain.Student.Entities.Faculty>>>(repo => repo.GetAsync(It.IsAny<IEnumerable<string>>())).Returns(Task.FromResult<IEnumerable<Colleague.Domain.Student.Entities.Faculty>>(new List<Colleague.Domain.Student.Entities.Faculty>() { faculty1, faculty2 }.AsEnumerable()));

                // act
                var criteria = new FacultyQueryCriteria() { FacultyIds = facultyIds };
                var facDtos = await facultyService.QueryFacultyAsync(criteria);
            }
        }

        [TestClass]
        public class QueryFaculty
        {
            private FacultyService facultyService;
            private Mock<IFacultyRepository> facultyRepoMock;
            private IFacultyRepository facultyRepo;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IReferenceDataRepository> refRepoMock;
            private IReferenceDataRepository refRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private Mock<IStudentConfigurationRepository> configRepoMock;
            private IStudentConfigurationRepository configRepo;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private ILogger logger;
            private string faculty1Id;
            private Domain.Student.Entities.Faculty faculty1;
            private string faculty2Id;
            private Domain.Student.Entities.Faculty faculty2;
            private StudentConfiguration config;

            [TestInitialize]
            public async void Initialize()
            {
                facultyRepoMock = new Mock<IFacultyRepository>();
                facultyRepo = facultyRepoMock.Object;
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                refRepoMock = new Mock<IReferenceDataRepository>();
                refRepo = refRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                configRepoMock = new Mock<IStudentConfigurationRepository>();
                configRepo = configRepoMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;

                // Set up current user
                currentUserFactory = new CurrentUserSetup.FacultyUserFactory();

                // Mock faculty repo response
                faculty1Id = "0000002";
                faculty1 = new Domain.Student.Entities.Faculty(faculty1Id, "Quincy");
                faculty1.FirstName = "John";
                faculty1.MiddleName = "Arnold";
                faculty1.AddEmailAddress(new EmailAddress("xyz@xmail.com", "FAC"));
                faculty1.AddPhone(new Phone("864-123-4564", "O"));
                faculty1.ProfessionalName = "Dr Johnathan Barker DMD";
                facultyRepoMock.Setup(repo => repo.GetAsync(faculty1Id)).Returns(Task.FromResult(faculty1));

                // Mock faculty repo response
                faculty2Id = "0000004";
                faculty2 = new Domain.Student.Entities.Faculty(faculty2Id, "Jones");
                faculty2.FirstName = "William";
                faculty2.MiddleName = string.Empty;
                faculty2.ProfessionalName = null;
                faculty2.AddEmailAddress(new EmailAddress("abc@xmail.com", "FAC"));
                faculty2.AddPhone(new Phone("864-123-9998", "O"));
                facultyRepoMock.Setup(repo => repo.GetAsync(faculty2Id)).Returns(Task.FromResult(faculty2));

                // Mock config repo response
                config = new StudentConfiguration();
                config.FacultyEmailTypeCode = "FAC";
                config.FacultyPhoneTypeCode = "O";
                configRepoMock.Setup(repo => repo.GetStudentConfigurationAsync()).Returns(Task.FromResult(config));

                // Mock AdressRelationTypes response
                List<AddressRelationType> addressRelationTypes = new List<AddressRelationType>();
                addressRelationTypes.Add(new AddressRelationType("FAC", "Faculty", "FAC", "FAC"));
                refRepoMock.Setup<IEnumerable<AddressRelationType>>(repo => repo.AddressRelationTypes).Returns(addressRelationTypes);

                // Mock Adapters
                var facultyDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Faculty, Ellucian.Colleague.Dtos.Student.Faculty>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Faculty, Ellucian.Colleague.Dtos.Student.Faculty>()).Returns(facultyDtoAdapter);
                var phoneDtoAdapter = new AutoMapperAdapter<Domain.Base.Entities.Phone, Dtos.Base.Phone>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Base.Entities.Phone, Dtos.Base.Phone>()).Returns(phoneDtoAdapter);

                // Instantiate faculty service with mocked items
                facultyService = new FacultyService(adapterRegistry, facultyRepo, configRepo, sectionRepo, termRepo, refRepo, null, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                facultyRepo = null;
                adapterRegistry = null;
                configRepo = null;
                termRepo = null;
                sectionRepo = null;
                logger = null;
            }

            [TestMethod]
            public async Task QueryFaculty_ReturnsFaculty()
            {
                // arrange
                facultyRepoMock.Setup<Task<IEnumerable<Colleague.Domain.Student.Entities.Faculty>>>(repo => repo.GetFacultyByIdsAsync(It.IsAny<IEnumerable<string>>())).Returns(Task.FromResult<IEnumerable<Colleague.Domain.Student.Entities.Faculty>>(new List<Colleague.Domain.Student.Entities.Faculty>() { faculty1, faculty2 }.AsEnumerable()));

                // act
                var criteria = new FacultyQueryCriteria() { FacultyIds = new List<string>() { faculty1Id, faculty2Id }.AsEnumerable() };
                var facDtos = await facultyService.QueryFacultyAsync(new FacultyQueryCriteria() { FacultyIds = new List<string>() { faculty1Id, faculty2Id }.AsEnumerable() });

                // assert
                Assert.AreEqual(2, facDtos.Count());
                var fac1Dto = facDtos.Where(f => f.Id == faculty1.Id).FirstOrDefault();
                Assert.IsNotNull(fac1Dto);
                Assert.AreEqual(faculty1.LastName, fac1Dto.LastName);
                var fac2Dto = facDtos.Where(f => f.Id == faculty2.Id).FirstOrDefault();
                Assert.IsNotNull(fac2Dto);
                Assert.AreEqual(faculty2.GetFacultyEmailAddresses("FAC").First(), fac2Dto.EmailAddresses.First());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task QueryFaculty_ThrowsExceptionForNullIds()
            {
                // arrange
                List<string> facultyIds = null;
                facultyRepoMock.Setup<Task<IEnumerable<Colleague.Domain.Student.Entities.Faculty>>>(repo => repo.GetAsync(It.IsAny<IEnumerable<string>>())).Returns(Task.FromResult<IEnumerable<Colleague.Domain.Student.Entities.Faculty>>(new List<Colleague.Domain.Student.Entities.Faculty>() { faculty1, faculty2 }.AsEnumerable()));

                // act
                var criteria = new FacultyQueryCriteria() { FacultyIds = facultyIds };
                var facDtos = await facultyService.QueryFacultyAsync(criteria);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task QueryFaculty_ThrowsExceptionForEmptyIds()
            {
                // arrange
                List<string> facultyIds = new List<string>();
                facultyRepoMock.Setup<Task<IEnumerable<Colleague.Domain.Student.Entities.Faculty>>>(repo => repo.GetAsync(It.IsAny<IEnumerable<string>>())).Returns(Task.FromResult<IEnumerable<Colleague.Domain.Student.Entities.Faculty>>(new List<Colleague.Domain.Student.Entities.Faculty>() { faculty1, faculty2 }.AsEnumerable()));

                // act
                var criteria = new FacultyQueryCriteria() { FacultyIds = facultyIds };
                var facDtos = await facultyService.QueryFacultyAsync(criteria);
            }
        }

        [TestClass]
        public class GetFacultyPermissions : CurrentUserSetup
        {
            private FacultyService facultyService;
            private Mock<IFacultyRepository> facultyRepoMock;
            private IFacultyRepository facultyRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private Mock<IStudentConfigurationRepository> configRepoMock;
            private IStudentConfigurationRepository configRepo;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IReferenceDataRepository> referenceRepoMock;
            private IReferenceDataRepository referenceRepo;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;

            [TestInitialize]
            public async void Initialize()
            {
                facultyRepoMock = new Mock<IFacultyRepository>();
                facultyRepo = facultyRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                configRepoMock = new Mock<IStudentConfigurationRepository>();
                configRepo = configRepoMock.Object;
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                referenceRepoMock = new Mock<IReferenceDataRepository>();
                referenceRepo = referenceRepoMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;

                // Set up current user
                currentUserFactory = new CurrentUserSetup.FacultyUserFactory();

                facultyService = new FacultyService(adapterRegistry, facultyRepo, configRepo, sectionRepo, termRepo, referenceRepo, null, currentUserFactory, roleRepo, logger);
            }

            [TestMethod]
            public async Task ReturnsPermissions()
            {
                // Set up update permissions on advisor's role
                facultyRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreatePrerequisiteWaiver));
                facultyRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateFacultyConsent));
                facultyRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateStudentPetition));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { facultyRole });

                // Act - call method that determines whether current user has permissions
                var result = await facultyService.GetFacultyPermissionsAsync();

                // Assert--result is true
                Assert.IsTrue(result.Contains(StudentPermissionCodes.CreatePrerequisiteWaiver));
                Assert.IsTrue(result.Contains(StudentPermissionCodes.CreateFacultyConsent));
                Assert.IsTrue(result.Contains(StudentPermissionCodes.CreateStudentPetition));
            }

            [TestMethod]
            public async Task ReturnsEmptyListIfNoPermissions()
            {
                // Default advisor does not have any permissions
                // Act - call method that determines whether current user has permissions
                var result = await facultyService.GetFacultyPermissionsAsync();

                // Assert--result is true
                Assert.IsTrue(result.Count() == 0);
            }
        }

        [TestClass]
        public class GetFacultySections4
        {
            private FacultyService facultyService;
            private Mock<IFacultyRepository> facultyRepoMock;
            private IFacultyRepository facultyRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private Mock<IStudentConfigurationRepository> configRepoMock;
            private IStudentConfigurationRepository configRepo;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IReferenceDataRepository> refRepoMock;
            private IReferenceDataRepository refRepo;
            private Mock<IStudentReferenceDataRepository> studentRefRepoMock;
            private IStudentReferenceDataRepository studentRefRepo;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private ILogger logger;
            private string faculty1Id;
            private Domain.Student.Entities.Faculty faculty1;
            private string faculty2Id;
            private Domain.Student.Entities.Faculty faculty2;
            private Domain.Student.Entities.Section section1;
            private Domain.Student.Entities.Section section2;
            private Domain.Student.Entities.Section section3;
            private Domain.Student.Entities.Section section4;
            private Domain.Student.Entities.Section section5;
            private Domain.Student.Entities.Section section6;
            private Domain.Student.Entities.Section section7;
            private Domain.Student.Entities.Section section8;
            private Domain.Student.Entities.Section section9;
            private Domain.Student.Entities.Section section10;
            private Domain.Student.Entities.Section section11;
            private Domain.Student.Entities.Section section12;
            private Domain.Student.Entities.Term term1;
            private Domain.Student.Entities.Term term2;
            private Domain.Student.Entities.Term term3;
            private StudentConfiguration config;

            [TestInitialize]
            public void Initialize()
            {
                facultyRepoMock = new Mock<IFacultyRepository>();
                facultyRepo = facultyRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                configRepoMock = new Mock<IStudentConfigurationRepository>();
                configRepo = configRepoMock.Object;
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                refRepoMock = new Mock<IReferenceDataRepository>();
                refRepo = refRepoMock.Object;
                studentRefRepoMock = new Mock<IStudentReferenceDataRepository>();
                studentRefRepo = studentRefRepoMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;

                // Set up current user
                currentUserFactory = new CurrentUserSetup.FacultyUserFactory();

                // Mock faculty repo response
                faculty1Id = "0000002";
                faculty1 = new Domain.Student.Entities.Faculty(faculty1Id, "Quincy");
                faculty1.AddEmailAddress(new EmailAddress("xyz@xmail.com", "FAC"));
                faculty1.AddPhone(new Phone("864-123-4564", "O"));
                facultyRepoMock.Setup(repo => repo.GetAsync(faculty1Id)).Returns(Task.FromResult(faculty1));

                // Mock faculty repo response
                faculty2Id = "0000004";
                faculty2 = new Domain.Student.Entities.Faculty(faculty2Id, "Jones");
                faculty2.AddEmailAddress(new EmailAddress("abc@xmail.com", "FAC"));
                faculty2.AddPhone(new Phone("864-123-9998", "O"));
                facultyRepoMock.Setup(repo => repo.GetAsync(faculty2Id)).Returns(Task.FromResult(faculty2));

                // Mock config repo response
                config = new StudentConfiguration();
                config.FacultyEmailTypeCode = "FAC";
                config.FacultyPhoneTypeCode = "O";
                configRepoMock.Setup(repo => repo.GetStudentConfigurationAsync()).Returns(Task.FromResult(config));

                // Mock term repo response
                term1 = new Domain.Student.Entities.Term("2011/FA", "Fall 2011", new DateTime(2011, 9, 1), new DateTime(2011, 12, 25), 2011, 1, true, true, "2011RT", true);
                term2 = new Domain.Student.Entities.Term("2012/SP", "Spring 2012", new DateTime(2012, 1, 5), new DateTime(2012, 5, 30), 2011, 2, true, true, "2011RT", true);
                term3 = new Domain.Student.Entities.Term("2012/SU", "Summer 2012", new DateTime(2012, 6, 4), new DateTime(2012, 8, 25), 2012, 1, true, true, "2012RT", true);
                IEnumerable<Domain.Student.Entities.Term> termList = new List<Domain.Student.Entities.Term>() { term1, term2, term3 };
                termRepoMock.Setup(repo => repo.GetRegistrationTermsAsync()).Returns(Task.FromResult(termList));

                // Mock section repo response
                ICollection<OfferingDepartment> depts = new List<OfferingDepartment>() { new OfferingDepartment("D1", 50m), new OfferingDepartment("D2", 50m) };
                ICollection<string> clcs = new List<string>() { "100", "200", "300", "400" };
                List<SectionStatusItem> statuses = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddYears(-5)) };
                // sections 1 -- 6 go in term 1 for range testing with test range start and end of 9/5 and 12/15, 
                // and the test faculty ("0000001") assigned to all sections
                // testing for overlap of time periods
                // period           start       end
                // section 1  <---> |            |        - runs  9/1  -  9/4,  excluded
                // section 2  <-----|----->      |        - runs  9/1  - 11/15, included
                // section 3        |   <----->  |        - runs 10/5  - 11/15, included
                // section 4        |      <-----|----->  - runs 11/5  - 12/25, included
                // section 5        |            |  <-->  - runs 12/17 - 12/25, excluded
                // section 6  <-----|------------|----->  - runs  9/1  - 12/25, included
                // Setup the sections in term1
                section1 = new Domain.Student.Entities.Section("11", "11", "01", new DateTime(2011, 9, 1), 3, null, "Section1 Title", "IN", depts, clcs, "UG", statuses, false, true, false, true, false);
                section1.TermId = "2011/FA"; section1.EndDate = new DateTime(2011, 9, 4); section1.AddFaculty("0000001");
                // section 2 overlap start before test start date, ends before test end date
                section2 = new Domain.Student.Entities.Section("12", "11", "02", new DateTime(2011, 9, 1), 3, null, "Section2 Title", "IN", depts, clcs, "UG", statuses, false, true, false, true, false);
                section2.TermId = "2011/FA"; section2.EndDate = new DateTime(2011, 11, 15); section2.AddFaculty("0000001");
                // section 3 start after test start date and end before test end date
                section3 = new Domain.Student.Entities.Section("13", "12", "01", new DateTime(2011, 10, 5), 3, null, "Section3 Title", "IN", depts, clcs, "UG", statuses, false, true, false, true, false);
                section3.TermId = "2011/FA"; section3.EndDate = new DateTime(2011, 11, 15); section3.AddFaculty("0000001");
                // section 4 start after test start date and before test end date and end after test end date
                section4 = new Domain.Student.Entities.Section("14", "12", "02", new DateTime(2011, 11, 5), 3, null, "Section4 Title", "IN", depts, clcs, "UG", statuses, false, true, false, true, false);
                section4.TermId = "2011/FA"; section4.EndDate = new DateTime(2011, 12, 25); section4.AddFaculty("0000001");
                // section 5 start after test end date
                section5 = new Domain.Student.Entities.Section("15", "13", "01", new DateTime(2011, 12, 17), 3, null, "Section5 Title", "IN", depts, clcs, "UG", statuses, false, true, false, true, false);
                section5.TermId = "2011/FA"; section5.EndDate = new DateTime(2011, 12, 25); section5.AddFaculty("0000001");
                // section 6 start before test start date and end after test end date
                section6 = new Domain.Student.Entities.Section("16", "13", "02", new DateTime(2011, 9, 1), 3, null, "Section6 Title", "IN", depts, clcs, "UG", statuses, false, true, false, true, false);
                section6.TermId = "2011/FA"; section6.EndDate = new DateTime(2011, 12, 25); section6.AddFaculty("0000001");

                // sections 7 -- 9 go in term2, test faculty assigned to only 2 sections
                section7 = new Domain.Student.Entities.Section("21", "11", "01", new DateTime(2012, 1, 5), 3, null, "Section7 Title", "IN", depts, clcs, "UG", statuses, false, true, false, true, false);
                section7.TermId = "2012/SP"; section7.EndDate = new DateTime(2012, 5, 30); section7.AddFaculty("0000001");
                section8 = new Domain.Student.Entities.Section("22", "12", "02", new DateTime(2012, 1, 5), 3, null, "Section8 Title", "IN", depts, clcs, "UG", statuses, false, true, false, true, false);
                section8.TermId = "2012/SP"; section8.EndDate = new DateTime(2012, 5, 30); section8.AddFaculty("0000001");
                section9 = new Domain.Student.Entities.Section("23", "13", "03", new DateTime(2012, 1, 5), 3, null, "Section9 Title", "IN", depts, clcs, "UG", statuses, false, true, false, true, false);
                section9.TermId = "2012/SP"; section9.EndDate = new DateTime(2012, 5, 30); section9.AddFaculty("0000002");

                // sections 10 -- 12 go in term 3, test faculty assigned to no sections
                section10 = new Domain.Student.Entities.Section("31", "11", "02", new DateTime(2012, 6, 4), 3, null, "Section10 Title", "IN", depts, clcs, "UG", statuses, false, true, false, true, false);
                section10.TermId = "2012/SU"; section10.EndDate = new DateTime(2012, 8, 25); section10.AddFaculty("0000002");
                section11 = new Domain.Student.Entities.Section("32", "12", "01", new DateTime(2012, 6, 4), 3, null, "Section11 Title", "IN", depts, clcs, "UG", statuses, false, true, false, true, false);
                section11.TermId = "2012/SU"; section11.EndDate = new DateTime(2012, 8, 25); section11.AddFaculty("0000002");
                section12 = new Domain.Student.Entities.Section("33", "13", "01", new DateTime(2012, 6, 4), 3, null, "Section12 Title", "IN", depts, clcs, "UG", statuses, false, true, false, true, false);
                section12.TermId = "2012/SU"; section12.EndDate = new DateTime(2012, 8, 25); section12.AddFaculty("0000002");
                IEnumerable<Domain.Student.Entities.Section> sectionList = new List<Domain.Student.Entities.Section>() { section1, section2, section3, section4, section5, section6, section7, section8, section9, section10, section11, section12 };
                sectionRepoMock.Setup(repo => repo.GetRegistrationSectionsAsync(termList)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sectionList));
                sectionRepoMock.Setup(repo => repo.GetNonCachedFacultySectionsAsync(termList, It.IsAny<string>(), It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sectionList));

                // Mock Adapters
                var facultyDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Faculty, Ellucian.Colleague.Dtos.Student.Faculty>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Faculty, Ellucian.Colleague.Dtos.Student.Faculty>()).Returns(facultyDtoAdapter);
                var phoneDtoAdapter = new AutoMapperAdapter<Domain.Base.Entities.Phone, Dtos.Base.Phone>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Base.Entities.Phone, Dtos.Base.Phone>()).Returns(phoneDtoAdapter);
                var sectionDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.Section, Dtos.Student.Section3>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.Section, Dtos.Student.Section3>()).Returns(sectionDtoAdapter);

                // Instantiate faculty service with mocked items
                facultyService = new FacultyService(adapterRegistry, facultyRepo, configRepo, sectionRepo, termRepo, refRepo, studentRefRepo, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                facultyRepo = null;
                adapterRegistry = null;
                configRepo = null;
                termRepo = null;
                sectionRepo = null;
                logger = null;
            }

            [TestMethod]
            public async Task GetFacultySections4_ReturnsEmptyListForNoFacultyId()
            {
                var result = await facultyService.GetFacultySections4Async(null, null, null, false);
                Assert.AreEqual(0, result.Dto.Count());
            }

            [TestMethod]
            public async Task GetFacultySections4_ReturnsEmptyListForFacultyWithNoSections()
            {
                var result = await facultyService.GetFacultySections4Async("0000003", null, null, false);
                Assert.AreEqual(0, result.Dto.Count());
            }

            [TestMethod]
            public async Task GetFacultySections4_ReturnsAllFaculty1Sections()
            {
                // mainly setup validation
                DateTime start = new DateTime(2011, 8, 1);
                DateTime end = new DateTime(2012, 12, 31);
                var result = await facultyService.GetFacultySections4Async("0000001", start, end, false);
                Assert.AreEqual(8, result.Dto.Count());
            }

            [TestMethod]
            public async Task GetFacultySections4_ReturnsAllFaculty2Sections()
            {
                // mainly setup validation
                DateTime start = new DateTime(2011, 8, 1);
                DateTime end = new DateTime(2012, 12, 31);
                var result = await facultyService.GetFacultySections4Async("0000002", start, end, false);
                Assert.AreEqual(4, result.Dto.Count());
            }

            [TestMethod]
            public async Task GetFacultySections4_ReturnsAllFaculty1FallSections()
            {
                DateTime start = new DateTime(2011, 8, 1);
                DateTime end = new DateTime(2011, 12, 31);
                var result = await facultyService.GetFacultySections4Async("0000001", start, end, false);
                Assert.AreEqual(6, result.Dto.Count());
            }

            [TestMethod]
            public async Task GetFacultySections4_ReturnsAllFaculty1FallSections_NoCache()
            {
                DateTime start = new DateTime(2011, 8, 1);
                DateTime end = new DateTime(2011, 12, 31);
                var result = await facultyService.GetFacultySections4Async("0000001", start, end, false, false);
                Assert.AreEqual(6, result.Dto.Count());
            }

            [TestMethod]
            public async Task GetFacultySections4_ReturnsSixFaculty1FallSections()
            {
                DateTime start = new DateTime(2011, 9, 5);
                DateTime end = new DateTime(2011, 12, 15);
                var result = await facultyService.GetFacultySections4Async("0000001", start, end, false);
                // should omit section1, and section5
                Assert.AreEqual(4, result.Dto.Count());
            }

            [TestMethod]
            public async Task GetFacultySections4_ReturnsTwoFaculty1SpringSections()
            {
                DateTime start = new DateTime(2012, 1, 5);
                DateTime end = new DateTime(2012, 5, 30);
                var result = await facultyService.GetFacultySections4Async("0000001", start, end, false);
                Assert.AreEqual(2, result.Dto.Count());
            }

            [TestMethod]
            public async Task GetFacultySections4_ReturnsZeroFaculty1SummerSections()
            {
                DateTime start = new DateTime(2012, 6, 4);
                DateTime end = new DateTime(2012, 8, 25);
                var result = await facultyService.GetFacultySections4Async("0000001", start, end, false);
                Assert.AreEqual(0, result.Dto.Count());
            }

            [TestMethod]
            public async Task GetFacultySections4_ReturnsFiveFaculty1FallSpringSections()
            {
                DateTime start = new DateTime(2011, 11, 16);
                // null end date maps to 11/16/2011 + 90 days
                var result = await facultyService.GetFacultySections4Async("0000001", start, null, false);
                Assert.AreEqual(5, result.Dto.Count());
            }

            [TestMethod]
            public async Task GetFacultySections4_EnsuresStartEndOrder()
            {
                DateTime start = new DateTime(2011, 11, 16);
                DateTime end = new DateTime(2011, 10, 15);
                // end before start is discarded, and end is assigned start + 90 days
                var result = await facultyService.GetFacultySections4Async("0000001", start, end, false);
                Assert.AreEqual(5, result.Dto.Count());
            }

            [TestMethod]
            public async Task GetFacultySections4Async_result_has_no_Privacy_Restrictions()
            {
                DateTime start = new DateTime(2011, 11, 16);
                DateTime end = new DateTime(2011, 10, 15);
                IEnumerable<Domain.Student.Entities.Section> sectionList = new List<Domain.Student.Entities.Section>() { section1, section2, section3, section4, section5, section6, section7, section8, section9, section10, section11, section12 };
                // Assign user as faculty on every section returned by repo call
                foreach (var section in sectionList)
                {
                    section.AddFaculty(currentUserFactory.CurrentUser.PersonId);
                }
                sectionRepoMock.Setup(repo => repo.GetRegistrationSectionsAsync(It.IsAny<IEnumerable<Domain.Student.Entities.Term>>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sectionList));
                facultyService = new FacultyService(adapterRegistry, facultyRepo, configRepo, sectionRepo, termRepo, refRepo, studentRefRepo, currentUserFactory, roleRepo, logger);

                // end before start is discarded, and end is assigned start + 90 days
#pragma warning disable 618
                var result = await facultyService.GetFacultySections4Async("0000001", start, end, false);
#pragma warning restore 618
                Assert.IsNotNull(result);
                Assert.IsNotNull(result.Dto);
                Assert.AreEqual(5, result.Dto.Count());
                // Since user is faculty on every section returned by repo, none has a privacy restriction
                Assert.IsFalse(result.HasPrivacyRestrictions);
            }

            [TestMethod]
            public async Task GetFacultySections4Async_result_has_Privacy_Restrictions()
            {
                DateTime start = new DateTime(2011, 11, 16);
                DateTime end = new DateTime(2011, 10, 15);
                IEnumerable<Domain.Student.Entities.Section> sectionList = new List<Domain.Student.Entities.Section>() { section1, section2, section3, section4, section5, section6, section7, section8, section9, section10, section11, section12 };
                // Only sections where user is not assigned faculty
                sectionList = sectionList.Where(s => !s.FacultyIds.Contains(currentUserFactory.CurrentUser.PersonId)).ToList();
                sectionRepoMock.Setup(repo => repo.GetRegistrationSectionsAsync(It.IsAny<IEnumerable<Domain.Student.Entities.Term>>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sectionList));
                facultyService = new FacultyService(adapterRegistry, facultyRepo, configRepo, sectionRepo, termRepo, refRepo, studentRefRepo, currentUserFactory, roleRepo, logger);

                // end before start is discarded, and end is assigned start + 90 days
#pragma warning disable 618
                var result = await facultyService.GetFacultySections4Async("0000001", start, end, false);
#pragma warning restore 618
                Assert.IsNotNull(result);
                Assert.IsNotNull(result.Dto);
                // Since user is faculty on every section returned by repo, none has a privacy restriction
                Assert.IsTrue(result.HasPrivacyRestrictions);
                Assert.AreEqual(0, result.Dto.ElementAt(0).ActiveStudentIds.Count());
                Assert.AreEqual(0, result.Dto.ElementAt(1).ActiveStudentIds.Count());
                Assert.AreEqual(0, result.Dto.ElementAt(2).ActiveStudentIds.Count());
                Assert.AreEqual(0, result.Dto.ElementAt(3).ActiveStudentIds.Count());
            }

        }

        [TestClass]
        public class GetFacultySections4_SectionsOutsideRegistrationTerms
        {
            // variant on GetFacultySections4 to test new case where an ongoing term
            // has been removed from the list of active registration terms (after end of
            // drop period, but before term is over)
            private FacultyService facultyService;
            private Mock<IFacultyRepository> facultyRepoMock;
            private IFacultyRepository facultyRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private Mock<IStudentConfigurationRepository> configRepoMock;
            private IStudentConfigurationRepository configRepo;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IReferenceDataRepository> refRepoMock;
            private IReferenceDataRepository refRepo;
            private Mock<IStudentReferenceDataRepository> studentRefRepoMock;
            private IStudentReferenceDataRepository studentRefRepo;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private ILogger logger;
            private string faculty1Id;
            private Domain.Student.Entities.Faculty faculty1;
            private string faculty2Id;
            private Domain.Student.Entities.Faculty faculty2;
            private Domain.Student.Entities.Section section1;
            private Domain.Student.Entities.Section section2;
            private Domain.Student.Entities.Section section3;
            private Domain.Student.Entities.Section section4;
            private Domain.Student.Entities.Section section5;
            private Domain.Student.Entities.Section section6;
            private Domain.Student.Entities.Section section7;
            private Domain.Student.Entities.Term term1;
            private Domain.Student.Entities.Term term2;
            private Domain.Student.Entities.Term term3;
            private Domain.Student.Entities.Term term4;
            private Domain.Student.Entities.Term term5;
            private Domain.Student.Entities.Term term6;
            private Domain.Student.Entities.Term term7;
            private StudentConfiguration config;
            private IEnumerable<Domain.Student.Entities.Term> regTermList;

            [TestInitialize]
            public void Initialize()
            {
                facultyRepoMock = new Mock<IFacultyRepository>();
                facultyRepo = facultyRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                configRepoMock = new Mock<IStudentConfigurationRepository>();
                configRepo = configRepoMock.Object;
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                refRepoMock = new Mock<IReferenceDataRepository>();
                refRepo = refRepoMock.Object;
                studentRefRepoMock = new Mock<IStudentReferenceDataRepository>();
                studentRefRepo = studentRefRepoMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;

                // Set up current user
                currentUserFactory = new CurrentUserSetup.FacultyUserFactory();

                // Mock faculty repo response
                faculty1Id = "0000002";
                faculty1 = new Domain.Student.Entities.Faculty(faculty1Id, "Quincy");
                faculty1.AddEmailAddress(new EmailAddress("xyz@xmail.com", "FAC"));
                faculty1.AddPhone(new Phone("864-123-4564", "O"));
                facultyRepoMock.Setup(repo => repo.GetAsync(faculty1Id)).ReturnsAsync(faculty1);

                // Mock faculty repo response
                faculty2Id = "0000004";
                faculty2 = new Domain.Student.Entities.Faculty(faculty2Id, "Jones");
                faculty2.AddEmailAddress(new EmailAddress("abc@xmail.com", "FAC"));
                faculty2.AddPhone(new Phone("864-123-9998", "O"));
                facultyRepoMock.Setup(repo => repo.GetAsync(faculty2Id)).Returns(Task.FromResult(faculty2));

                // Mock config repo response
                config = new StudentConfiguration();
                config.FacultyEmailTypeCode = "FAC";
                config.FacultyPhoneTypeCode = "O";
                configRepoMock.Setup(repo => repo.GetStudentConfigurationAsync()).Returns(Task.FromResult(config));

                // Mock student reference repo response
                var scheduleTermValcodes = new List<ScheduleTerm>();

                // TODO: add some terms - and add tests for this.

                studentRefRepoMock.Setup(repo => repo.GetAllScheduleTermsAsync(false)).Returns(Task.FromResult(scheduleTermValcodes.AsEnumerable()));

                // Mock term repo response - several terms to make sure we catch appropriate terms by date
                term1 = new Domain.Student.Entities.Term("T1", "T1", new DateTime(2011, 1, 1), new DateTime(2011, 1, 20), 2011, 1, true, true, "T1", true);
                term2 = new Domain.Student.Entities.Term("T2", "T2", new DateTime(2011, 2, 1), new DateTime(2011, 2, 20), 2011, 1, true, true, "T2", true);
                term3 = new Domain.Student.Entities.Term("T3", "T3", new DateTime(2011, 3, 1), new DateTime(2011, 3, 20), 2011, 2, true, true, "T3", true);
                term4 = new Domain.Student.Entities.Term("T4", "T4", new DateTime(2011, 1, 1), new DateTime(2011, 2, 15), 2011, 1, true, true, "T4", true);
                term5 = new Domain.Student.Entities.Term("T5", "T5", new DateTime(2011, 2, 17), new DateTime(2011, 3, 20), 2011, 1, true, true, "T5", true);
                term6 = new Domain.Student.Entities.Term("T6", "T6", new DateTime(2011, 1, 1), new DateTime(2011, 3, 30), 2011, 1, true, true, "T6", true);
                term7 = new Domain.Student.Entities.Term("T7", "T7", new DateTime(2011, 3, 25), new DateTime(2011, 4, 30), 2011, 1, true, true, "T7", true);

                IEnumerable<Domain.Student.Entities.Term> termList = new List<Domain.Student.Entities.Term>() { term1, term2, term3, term4, term5, term6, term7 };
                termRepoMock.Setup(repo => repo.GetAsync()).ReturnsAsync(termList);

                regTermList = new List<Domain.Student.Entities.Term>() { term7 };
                termRepoMock.Setup(repo => repo.GetRegistrationTermsAsync()).Returns(Task.FromResult(regTermList));

                // Mock section repo responses
                ICollection<OfferingDepartment> depts = new List<OfferingDepartment>() { new OfferingDepartment("D1", 50m), new OfferingDepartment("D2", 50m) };
                ICollection<string> clcs = new List<string>() { "100", "200", "300", "400" };
                List<SectionStatusItem> statuses = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddYears(-5)) };
                // sections 1 -- 7 go in terms 1 -- 7
                section1 = new Domain.Student.Entities.Section("11", "11", "01", new DateTime(2011, 1, 1), 3, null, "Section1 Title", "IN", depts, clcs, "UG", statuses, true, false, true, false, true, false);
                section1.TermId = "T1"; section1.EndDate = new DateTime(2011, 1, 20); section1.AddFaculty("0000001");

                section2 = new Domain.Student.Entities.Section("12", "11", "02", new DateTime(2011, 2, 1), 3, null, "Section2 Title", "IN", depts, clcs, "UG", statuses, true, false, true, false, true, false);
                section2.TermId = "T2"; section2.EndDate = new DateTime(2011, 2, 20); section2.AddFaculty("0000001");

                section3 = new Domain.Student.Entities.Section("13", "12", "01", new DateTime(2011, 3, 1), 3, null, "Section3 Title", "IN", depts, clcs, "UG", statuses, true, false, true, false, true, false);
                section3.TermId = "T3"; section3.EndDate = new DateTime(2011, 3, 20); section3.AddFaculty("0000001");

                section4 = new Domain.Student.Entities.Section("14", "12", "02", new DateTime(2011, 1, 1), 3, null, "Section4 Title", "IN", depts, clcs, "UG", statuses, true, false, true, false, true, false);
                section4.TermId = "T4"; section4.EndDate = new DateTime(2011, 2, 15); section4.AddFaculty("0000001");

                section5 = new Domain.Student.Entities.Section("15", "13", "01", new DateTime(2011, 2, 17), 3, null, "Section5 Title", "IN", depts, clcs, "UG", statuses, true, false, true, false, true, false);
                section5.TermId = "T5"; section5.EndDate = new DateTime(2011, 3, 20); section5.AddFaculty("0000001");

                section6 = new Domain.Student.Entities.Section("16", "13", "02", new DateTime(2011, 1, 1), 3, null, "Section6 Title", "IN", depts, clcs, "UG", statuses, true, false, true, false, true, false);
                section6.TermId = "T6"; section6.EndDate = new DateTime(2011, 3, 30); section6.AddFaculty("0000001");

                section7 = new Domain.Student.Entities.Section("17", "13", "02", new DateTime(2011, 3, 25), 3, null, "Section7 Title", "IN", depts, clcs, "UG", statuses, true, false, true, false, true, false);
                section7.TermId = "T7"; section7.EndDate = new DateTime(2011, 4, 30); section7.AddFaculty("0000001");

                List<Domain.Student.Entities.Term> nonRegTerms = new List<Domain.Student.Entities.Term>() { term1, term2, term3, term4, term5, term6 };
                IEnumerable<Domain.Student.Entities.Section> nonRegTermSectionList = new List<Domain.Student.Entities.Section>() { section1, section2, section3, section4, section5, section6 };
                sectionRepoMock.Setup(repo => repo.GetNonCachedFacultySectionsAsync(nonRegTerms, "0000001", false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(nonRegTermSectionList));

                IEnumerable<Domain.Student.Entities.Section> regTermSectionList = new List<Domain.Student.Entities.Section>() { section7 };
                sectionRepoMock.Setup(repo => repo.GetRegistrationSectionsAsync(regTermList)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(regTermSectionList));

                // Mock Adapters
                var facultyDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Faculty, Ellucian.Colleague.Dtos.Student.Faculty>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Faculty, Ellucian.Colleague.Dtos.Student.Faculty>()).Returns(facultyDtoAdapter);
                var phoneDtoAdapter = new AutoMapperAdapter<Domain.Base.Entities.Phone, Dtos.Base.Phone>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Base.Entities.Phone, Dtos.Base.Phone>()).Returns(phoneDtoAdapter);
                var sectionDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.Section, Dtos.Student.Section3>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.Section, Dtos.Student.Section3>()).Returns(sectionDtoAdapter);

                // Instantiate faculty service with mocked items
                facultyService = new FacultyService(adapterRegistry, facultyRepo, configRepo, sectionRepo, termRepo, refRepo, studentRefRepo, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                facultyRepo = null;
                adapterRegistry = null;
                configRepo = null;
                termRepo = null;
                sectionRepo = null;
                refRepo = null;
                studentRefRepo = null;
                logger = null;
            }

            [TestMethod]
            public async Task GetFacultySections4_SectionsOutsideRegistrationTermsReturnsEmptyListForNoFacultyId()
            {
                var result = await facultyService.GetFacultySections4Async(null, null, null, false);
                Assert.AreEqual(0, result.Dto.Count());
            }

            [TestMethod]
            public async Task GetFacultySections4_SevenSectionsTotal()
            {
                // encompass all terms (reg and outside reg)
                DateTime start = new DateTime(2011, 1, 1);
                DateTime end = new DateTime(2011, 5, 1);
                var result = await facultyService.GetFacultySections4Async("0000001", start, end, false);
                Assert.AreEqual(7, result.Dto.Count());
            }

            [TestMethod]
            public async Task GetFacultySections4_SixSectionsOutsideRegistrationTerms()
            {
                // mainly setup validation
                DateTime start = new DateTime(2011, 1, 1);
                DateTime end = new DateTime(2011, 3, 20);
                // should match T1 -- T6
                List<Domain.Student.Entities.Term> terms = new List<Domain.Student.Entities.Term>() { term1, term2, term3, term4, term5, term6 };
                List<Domain.Student.Entities.Section> sections = new List<Domain.Student.Entities.Section>() { section1, section2, section3, section4, section5, section6 };
                IEnumerable<Domain.Student.Entities.Section> eSections = sections.AsEnumerable();
                sectionRepoMock.Setup(repo => repo.GetNonCachedFacultySectionsAsync(terms, "0000001", false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(eSections));

                var result = (await facultyService.GetFacultySections4Async("0000001", start, end, false)).Dto.ToList();
                Assert.AreEqual(6, result.Count());
                Assert.AreEqual("T1", result[0].TermId); Assert.AreEqual("11", result[0].Id);
                Assert.AreEqual("T2", result[1].TermId); Assert.AreEqual("12", result[1].Id);
                Assert.AreEqual("T3", result[2].TermId); Assert.AreEqual("13", result[2].Id);
                Assert.AreEqual("T4", result[3].TermId); Assert.AreEqual("14", result[3].Id);
                Assert.AreEqual("T5", result[4].TermId); Assert.AreEqual("15", result[4].Id);
                Assert.AreEqual("T6", result[5].TermId); Assert.AreEqual("16", result[5].Id);
            }

            [TestMethod]
            public async Task GetFacultySections4_ThreeSectionsOutsideRegistrationTerms()
            {
                // dates to target non-Registration Terms
                DateTime start = new DateTime(2011, 1, 1);
                DateTime end = new DateTime(2011, 1, 25);
                // should match T1, T4, T6
                List<Domain.Student.Entities.Term> terms = new List<Domain.Student.Entities.Term>() { term1, term4, term6 };
                List<Domain.Student.Entities.Section> sections = new List<Domain.Student.Entities.Section>() { section1, section4, section6 };
                IEnumerable<Domain.Student.Entities.Section> eSections = sections.AsEnumerable();
                sectionRepoMock.Setup(repo => repo.GetNonCachedFacultySectionsAsync(terms, "0000001", false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(eSections));

                var result = (await facultyService.GetFacultySections4Async("0000001", start, end, false)).Dto.ToList();
                Assert.AreEqual(3, result.Count());
                Assert.AreEqual("T1", result[0].TermId); Assert.AreEqual("11", result[0].Id);
                Assert.AreEqual("T4", result[1].TermId); Assert.AreEqual("14", result[1].Id);
                Assert.AreEqual("T6", result[2].TermId); Assert.AreEqual("16", result[2].Id);
            }

            [TestMethod]
            public async Task GetFacultySections4_FiveSectionsOutsideRegistrationTerms()
            {
                DateTime start = new DateTime(2011, 1, 1);
                DateTime end = new DateTime(2011, 2, 25);
                // should match T1, T2, T4, T5, T6
                List<Domain.Student.Entities.Term> terms = new List<Domain.Student.Entities.Term>() { term1, term2, term4, term5, term6 };
                List<Domain.Student.Entities.Section> sections = new List<Domain.Student.Entities.Section>() { section1, section2, section4, section5, section6 };
                IEnumerable<Domain.Student.Entities.Section> eSections = sections.AsEnumerable();
                sectionRepoMock.Setup(repo => repo.GetNonCachedFacultySectionsAsync(terms, "0000001", false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(eSections));

                var result = (await facultyService.GetFacultySections4Async("0000001", start, end, false)).Dto.ToList();
                Assert.AreEqual(5, result.Count());
                Assert.AreEqual("T1", result[0].TermId); Assert.AreEqual("11", result[0].Id);
                Assert.AreEqual("T2", result[1].TermId); Assert.AreEqual("12", result[1].Id);
                Assert.AreEqual("T4", result[2].TermId); Assert.AreEqual("14", result[2].Id);
                Assert.AreEqual("T5", result[3].TermId); Assert.AreEqual("15", result[3].Id);
                Assert.AreEqual("T6", result[4].TermId); Assert.AreEqual("16", result[4].Id);
            }

            [TestMethod]
            public async Task GetFacultySections4_TwoSectionsOutsideRegistrationTerms()
            {
                DateTime start = new DateTime(2011, 2, 16);
                DateTime end = new DateTime(2011, 2, 16);
                // should match T2, T6
                List<Domain.Student.Entities.Term> terms = new List<Domain.Student.Entities.Term>() { term2, term6 };
                List<Domain.Student.Entities.Section> sections = new List<Domain.Student.Entities.Section>() { section2, section6 };
                IEnumerable<Domain.Student.Entities.Section> eSections = sections.AsEnumerable();
                sectionRepoMock.Setup(repo => repo.GetNonCachedFacultySectionsAsync(terms, "0000001", false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(eSections));

                var result = (await facultyService.GetFacultySections4Async("0000001", start, end, false)).Dto.ToList();
                Assert.AreEqual(2, result.Count());
                Assert.AreEqual("T2", result[0].TermId); Assert.AreEqual("12", result[0].Id);
                Assert.AreEqual("T6", result[1].TermId); Assert.AreEqual("16", result[1].Id);
            }

            [TestMethod]
            public async Task GetFacultySections4_FourSectionsOutsideRegistrationTerms()
            {
                DateTime start = new DateTime(2011, 2, 17);
                DateTime end = new DateTime(2011, 3, 21);
                // should match T2, T3, T5, T6
                List<Domain.Student.Entities.Term> terms = new List<Domain.Student.Entities.Term>() { term2, term3, term5, term6 };
                List<Domain.Student.Entities.Section> sections = new List<Domain.Student.Entities.Section>() { section2, section3, section5, section6 };
                IEnumerable<Domain.Student.Entities.Section> eSections = sections.AsEnumerable();
                sectionRepoMock.Setup(repo => repo.GetNonCachedFacultySectionsAsync(terms, "0000001", false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(eSections));

                var result = (await facultyService.GetFacultySections4Async("0000001", start, end, false)).Dto.ToList();
                Assert.AreEqual(4, result.Count());
                Assert.AreEqual("T2", result[0].TermId); Assert.AreEqual("12", result[0].Id);
                Assert.AreEqual("T3", result[1].TermId); Assert.AreEqual("13", result[1].Id);
                Assert.AreEqual("T5", result[2].TermId); Assert.AreEqual("15", result[2].Id);
                Assert.AreEqual("T6", result[3].TermId); Assert.AreEqual("16", result[3].Id);
            }

            [TestMethod]
            public async Task GetFacultySections4_UniqueListOfSectionsWithNoDups()
            {

                // Test to be sure that if the same sections are returned in the GetRegistrationSections AND in the GetNonCachedFacultySections that
                // duplicates are not returned.

                DateTime start = new DateTime(2011, 2, 16);
                DateTime end = new DateTime(2011, 2, 16);
                // should match T2, T6
                List<Domain.Student.Entities.Term> terms = new List<Domain.Student.Entities.Term>() { term2, term6 };
                List<Domain.Student.Entities.Section> sections = new List<Domain.Student.Entities.Section>() { section2, section6 };
                IEnumerable<Domain.Student.Entities.Section> eSections = sections.AsEnumerable();
                sectionRepoMock.Setup(repo => repo.GetNonCachedFacultySectionsAsync(terms, "0000001", false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(eSections));
                sectionRepoMock.Setup(repo => repo.GetRegistrationSectionsAsync(regTermList)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(eSections));
                var result = (await facultyService.GetFacultySections4Async("0000001", start, end, false)).Dto.ToList();
                Assert.AreEqual(2, result.Count());
                Assert.AreEqual("T2", result[0].TermId); Assert.AreEqual("12", result[0].Id);
                Assert.AreEqual("T6", result[1].TermId); Assert.AreEqual("16", result[1].Id);
            }
        }

        [TestClass]
        public class GetFacultySections4_WhenNoDateRange
        {
            // variant on GetFacultySections4 to test the new methodology of using CSWP and GRWP Allowed Terms 
            // when no date range is specified.  It should NOT default to range of today to 90 days. 
            private FacultyService facultyService;
            private Mock<IFacultyRepository> facultyRepoMock;
            private IFacultyRepository facultyRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private Mock<IStudentConfigurationRepository> configRepoMock;
            private IStudentConfigurationRepository configRepo;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IReferenceDataRepository> refRepoMock;
            private IReferenceDataRepository refRepo;
            private Mock<IStudentReferenceDataRepository> studentRefRepoMock;
            private IStudentReferenceDataRepository studentRefRepo;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private ILogger logger;
            private string faculty1Id;
            private Domain.Student.Entities.Faculty faculty1;
            private string faculty2Id;
            private Domain.Student.Entities.Faculty faculty2;
            private Domain.Student.Entities.Section section1;
            private Domain.Student.Entities.Section section2;
            private Domain.Student.Entities.Section section3;
            private Domain.Student.Entities.Section section4;
            private Domain.Student.Entities.Section section5;
            private Domain.Student.Entities.Section section6;
            private Domain.Student.Entities.Section section7;
            private Domain.Student.Entities.Section section8;
            private Domain.Student.Entities.Term term1;
            private Domain.Student.Entities.Term term2;
            private Domain.Student.Entities.Term term3;
            private Domain.Student.Entities.Term term4;
            private Domain.Student.Entities.Term term5;
            private Domain.Student.Entities.Term term6;
            private Domain.Student.Entities.Term term7;
            private Domain.Student.Entities.Term term8;
            private Ellucian.Colleague.Domain.Student.Entities.FacultyGradingConfiguration config;
            private IEnumerable<Domain.Student.Entities.Term> regTermList;

            [TestInitialize]
            public void Initialize()
            {
                facultyRepoMock = new Mock<IFacultyRepository>();
                facultyRepo = facultyRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                configRepoMock = new Mock<IStudentConfigurationRepository>();
                configRepo = configRepoMock.Object;
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                refRepoMock = new Mock<IReferenceDataRepository>();
                refRepo = refRepoMock.Object;
                studentRefRepoMock = new Mock<IStudentReferenceDataRepository>();
                studentRefRepo = studentRefRepoMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;

                // Set up current user
                currentUserFactory = new CurrentUserSetup.FacultyUserFactory();

                // Mock config repo response
                config = new Ellucian.Colleague.Domain.Student.Entities.FacultyGradingConfiguration();
                config.AddGradingTerm("T2");
                config.AddGradingTerm("T3");
                configRepoMock.Setup(repo => repo.GetFacultyGradingConfigurationAsync()).Returns(Task.FromResult(config));

                // Mock student reference repo response
                var scheduleTermValcodes = new List<ScheduleTerm>() { new ScheduleTerm("guid1", "T6", "Term 6"), new ScheduleTerm("guid2", "T7", "Term 7") };

                studentRefRepoMock.Setup(repo => repo.GetAllScheduleTermsAsync(false)).Returns(Task.FromResult(scheduleTermValcodes.AsEnumerable()));

                // Mock all term repo response - need several terms to make sure we are including appropriate terms.

                term1 = new Domain.Student.Entities.Term("T1", "Term 1", new DateTime(2011, 1, 1), new DateTime(2011, 1, 20), 2011, 1, true, true, "T1", true); // Don't include
                term2 = new Domain.Student.Entities.Term("T2", "Term 2", new DateTime(2012, 1, 1), new DateTime(2012, 5, 20), 2011, 2, true, true, "T2", true); // GRWP
                term3 = new Domain.Student.Entities.Term("T3", "Term 3", new DateTime(2012, 6, 1), new DateTime(2012, 7, 20), 2011, 3, true, true, "T3", true); // RGWP and GRWP 
                term4 = new Domain.Student.Entities.Term("T4", "Term 4", new DateTime(2012, 8, 1), new DateTime(2012, 12, 15), 2012, 1, true, true, "T4", true); // RGWP 
                term5 = new Domain.Student.Entities.Term("T5", "Term 5", new DateTime(2013, 2, 17), new DateTime(2013, 5, 20), 2012, 2, true, true, "T5", true); // Don't include 
                term6 = new Domain.Student.Entities.Term("T6", "Term 6", new DateTime(2013, 6, 1), new DateTime(2013, 7, 30), 2012, 1, true, true, "T6", true); // CSWP
                term7 = new Domain.Student.Entities.Term("T7", "Term 7", new DateTime(2014, 3, 25), new DateTime(2014, 4, 30), 2013, 2, true, true, "T7", true); // CSWP
                term8 = new Domain.Student.Entities.Term("T8", "Term 8", new DateTime(2014, 9, 25), new DateTime(2014, 12, 30), 2014, 1, true, true, "T8", true); // Don't include

                IEnumerable<Domain.Student.Entities.Term> termList = new List<Domain.Student.Entities.Term>() { term1, term2, term3, term4, term5, term6, term7, term8 };
                termRepoMock.Setup(repo => repo.GetAsync()).ReturnsAsync(termList);

                regTermList = new List<Domain.Student.Entities.Term>() { term3, term4 };
                termRepoMock.Setup(repo => repo.GetRegistrationTermsAsync()).Returns(Task.FromResult(regTermList));

                // Mock section repo responses
                ICollection<OfferingDepartment> depts = new List<OfferingDepartment>() { new OfferingDepartment("D1", 50m), new OfferingDepartment("D2", 50m) };
                ICollection<string> clcs = new List<string>() { "100", "200", "300", "400" };
                List<SectionStatusItem> statuses = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddYears(-5)) };
                // sections 1 -- 8 go in terms 1 -- 8
                section1 = new Domain.Student.Entities.Section("11", "11", "01", new DateTime(2011, 1, 1), 3, null, "Section1 Title", "IN", depts, clcs, "UG", statuses, true, false, true, false, true, false);
                section1.TermId = "T1"; section1.EndDate = new DateTime(2011, 1, 20); section1.AddFaculty("0000001");

                section2 = new Domain.Student.Entities.Section("12", "11", "02", new DateTime(2011, 2, 1), 3, null, "Section2 Title", "IN", depts, clcs, "UG", statuses, true, false, true, false, true, false);
                section2.TermId = "T2"; section2.EndDate = new DateTime(2011, 2, 20); section2.AddFaculty("0000001");

                section3 = new Domain.Student.Entities.Section("13", "12", "01", new DateTime(2012, 3, 1), 3, null, "Section3 Title", "IN", depts, clcs, "UG", statuses, true, false, true, false, true, false);
                section3.TermId = "T3"; section3.EndDate = new DateTime(2011, 3, 20); section3.AddFaculty("0000001");

                section4 = new Domain.Student.Entities.Section("14", "12", "02", new DateTime(2012, 1, 1), 3, null, "Section4 Title", "IN", depts, clcs, "UG", statuses, true, false, true, false, true, false);
                section4.TermId = "T4"; section4.EndDate = new DateTime(2011, 2, 15); section4.AddFaculty("0000001");

                section5 = new Domain.Student.Entities.Section("15", "13", "01", new DateTime(2013, 2, 17), 3, null, "Section5 Title", "IN", depts, clcs, "UG", statuses, true, false, true, false, true, false);
                section5.TermId = "T5"; section5.EndDate = new DateTime(2011, 3, 20); section5.AddFaculty("0000001");

                section6 = new Domain.Student.Entities.Section("16", "13", "02", new DateTime(2013, 1, 1), 3, null, "Section6 Title", "IN", depts, clcs, "UG", statuses, true, false, true, false, true, false);
                section6.TermId = "T6"; section6.EndDate = new DateTime(2011, 3, 30); section6.AddFaculty("0000001");

                section7 = new Domain.Student.Entities.Section("17", "13", "02", new DateTime(2014, 3, 25), 3, null, "Section7 Title", "IN", depts, clcs, "UG", statuses, true, false, true, false, true, false);
                section7.TermId = "T7"; section7.EndDate = new DateTime(2011, 4, 30); section7.AddFaculty("0000001");

                section8 = new Domain.Student.Entities.Section("17", "13", "02", new DateTime(2014, 3, 25), 3, null, "Section7 Title", "IN", depts, clcs, "UG", statuses, true, false, true, false, true, false);
                section8.TermId = "T8"; section8.EndDate = new DateTime(2011, 4, 30); section8.AddFaculty("0000001");

                // Set up response for the other terms....
                List<Domain.Student.Entities.Term> nonRegTerms = new List<Domain.Student.Entities.Term>() { term2, term6, term7 };
                IEnumerable<Domain.Student.Entities.Section> nonRegTermSectionList = new List<Domain.Student.Entities.Section>() { section2, section6, section7 };
                sectionRepoMock.Setup(repo => repo.GetNonCachedFacultySectionsAsync(nonRegTerms, "0000001", false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(nonRegTermSectionList));

                IEnumerable<Domain.Student.Entities.Section> regTermSectionList = new List<Domain.Student.Entities.Section>() { section3, section4 };
                sectionRepoMock.Setup(repo => repo.GetRegistrationSectionsAsync(regTermList)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(regTermSectionList));

                // Mock Adapters
                var facultyDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Faculty, Ellucian.Colleague.Dtos.Student.Faculty>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Faculty, Ellucian.Colleague.Dtos.Student.Faculty>()).Returns(facultyDtoAdapter);
                var phoneDtoAdapter = new AutoMapperAdapter<Domain.Base.Entities.Phone, Dtos.Base.Phone>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Base.Entities.Phone, Dtos.Base.Phone>()).Returns(phoneDtoAdapter);
                var sectionDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.Section, Dtos.Student.Section3>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.Section, Dtos.Student.Section3>()).Returns(sectionDtoAdapter);

                // Instantiate faculty service with mocked items
                facultyService = new FacultyService(adapterRegistry, facultyRepo, configRepo, sectionRepo, termRepo, refRepo, studentRefRepo, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                facultyRepo = null;
                adapterRegistry = null;
                configRepo = null;
                termRepo = null;
                sectionRepo = null;
                refRepo = null;
                studentRefRepo = null;
                logger = null;
            }


            [TestMethod]
            public async Task GetFacultySections4_TermsOnAll3ParameterS()
            {
                var result = await facultyService.GetFacultySections4Async("0000001", null, null, false);
                Assert.AreEqual(5, result.Dto.Count());
                Assert.AreEqual(section3.Id, result.Dto.ElementAt(0).Id);
                Assert.AreEqual(section4.Id, result.Dto.ElementAt(1).Id);
                Assert.AreEqual(section2.Id, result.Dto.ElementAt(2).Id);
                Assert.AreEqual(section6.Id, result.Dto.ElementAt(3).Id);
                Assert.AreEqual(section7.Id, result.Dto.ElementAt(4).Id);
            }
            [TestMethod]
            public async Task GetFacultySections4_TermsOnRegAndGradingOnly()
            {
                // Mock student reference repo response
                List<ScheduleTerm> scheduleTermValcodes = new List<ScheduleTerm>();
                studentRefRepoMock.Setup(repo => repo.GetAllScheduleTermsAsync(false)).Returns(Task.FromResult(scheduleTermValcodes.AsEnumerable()));
                // Set up response for the other terms....
                List<Domain.Student.Entities.Term> nonRegTerms = new List<Domain.Student.Entities.Term>() { term2 };
                IEnumerable<Domain.Student.Entities.Section> nonRegTermSectionList = new List<Domain.Student.Entities.Section>() { section2 };
                sectionRepoMock.Setup(repo => repo.GetNonCachedFacultySectionsAsync(nonRegTerms, "0000001", false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(nonRegTermSectionList));
                var result = await facultyService.GetFacultySections4Async("0000001", null, null, false);
                Assert.AreEqual(3, result.Dto.Count());
                Assert.AreEqual(section3.Id, result.Dto.ElementAt(0).Id);
                Assert.AreEqual(section4.Id, result.Dto.ElementAt(1).Id);
                Assert.AreEqual(section2.Id, result.Dto.ElementAt(2).Id);
            }
            [TestMethod]
            public async Task GetFacultySections4_TermsOnRegAndScheduleTermsOnly()
            {
                // Mock config repo response
                config = new Ellucian.Colleague.Domain.Student.Entities.FacultyGradingConfiguration();
                configRepoMock.Setup(repo => repo.GetFacultyGradingConfigurationAsync()).Returns(Task.FromResult(config));

                // Set up response for the other terms.... 
                List<Domain.Student.Entities.Term> nonRegTerms = new List<Domain.Student.Entities.Term>() { term6, term7 };
                IEnumerable<Domain.Student.Entities.Section> nonRegTermSectionList = new List<Domain.Student.Entities.Section>() { section6, section7 };
                sectionRepoMock.Setup(repo => repo.GetNonCachedFacultySectionsAsync(nonRegTerms, "0000001", false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(nonRegTermSectionList));
                var result = await facultyService.GetFacultySections4Async("0000001", null, null, false);
                Assert.AreEqual(4, result.Dto.Count());
                Assert.AreEqual(section3.Id, result.Dto.ElementAt(0).Id);
                Assert.AreEqual(section4.Id, result.Dto.ElementAt(1).Id);
                Assert.AreEqual(section6.Id, result.Dto.ElementAt(2).Id);
                Assert.AreEqual(section7.Id, result.Dto.ElementAt(3).Id);
            }
            [TestMethod]
            public async Task GetFacultySections4_RegistrationTermsOnly()
            {
                // Mock config repo response - no grading terms
                config = new Ellucian.Colleague.Domain.Student.Entities.FacultyGradingConfiguration();
                configRepoMock.Setup(repo => repo.GetFacultyGradingConfigurationAsync()).Returns(Task.FromResult(config));
                // Mock student reference repo response - no schedule tersm
                List<ScheduleTerm> scheduleTermValcodes = new List<ScheduleTerm>();
                studentRefRepoMock.Setup(repo => repo.GetAllScheduleTermsAsync(false)).Returns(Task.FromResult(scheduleTermValcodes.AsEnumerable()));

                // ACT
                var result = await facultyService.GetFacultySections4Async("0000001", null, null, false);
                // Assert
                Assert.AreEqual(2, result.Dto.Count());
                Assert.AreEqual(section3.Id, result.Dto.ElementAt(0).Id);
                Assert.AreEqual(section4.Id, result.Dto.ElementAt(1).Id);
            }
            [TestMethod]
            public async Task GetFacultySections4_NoAllowedTerms()
            {
                // Mock config repo response - no grading terms
                config = new Ellucian.Colleague.Domain.Student.Entities.FacultyGradingConfiguration();
                configRepoMock.Setup(repo => repo.GetFacultyGradingConfigurationAsync()).Returns(Task.FromResult(config));
                // Mock student reference repo response - no schedule tersm
                List<ScheduleTerm> scheduleTermValcodes = new List<ScheduleTerm>();
                // Mock term repo - no registration terms
                studentRefRepoMock.Setup(repo => repo.GetAllScheduleTermsAsync(false)).Returns(Task.FromResult(scheduleTermValcodes.AsEnumerable()));
                regTermList = new List<Domain.Student.Entities.Term>();
                termRepoMock.Setup(repo => repo.GetRegistrationTermsAsync()).Returns(Task.FromResult(regTermList));

                // ACT
                var result = await facultyService.GetFacultySections4Async("0000001", null, null, false);
                // Assert
                Assert.AreEqual(0, result.Dto.Count());
            }
        }

        [TestClass]
        public class GetFacultyPermissions2Async : CurrentUserSetup
        {
            private FacultyService facultyService;
            private Mock<IFacultyRepository> facultyRepoMock;
            private IFacultyRepository facultyRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private Mock<IStudentConfigurationRepository> configRepoMock;
            private IStudentConfigurationRepository configRepo;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IReferenceDataRepository> referenceRepoMock;
            private IReferenceDataRepository referenceRepo;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;

            [TestInitialize]
            public async void Initialize()
            {
                facultyRepoMock = new Mock<IFacultyRepository>();
                facultyRepo = facultyRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                configRepoMock = new Mock<IStudentConfigurationRepository>();
                configRepo = configRepoMock.Object;
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                referenceRepoMock = new Mock<IReferenceDataRepository>();
                referenceRepo = referenceRepoMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;

                // Set up current user
                currentUserFactory = new CurrentUserSetup.FacultyUserFactory();
                var facultyPermissionDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.FacultyPermissions, Dtos.Student.FacultyPermissions>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.FacultyPermissions, Dtos.Student.FacultyPermissions>()).Returns(facultyPermissionDtoAdapter);

                facultyService = new FacultyService(adapterRegistry, facultyRepo, configRepo, sectionRepo, termRepo, referenceRepo, null, currentUserFactory, roleRepo, logger);
            }

            [TestMethod]
            public async Task ReturnsPermissions2()
            {
                // Set up update permissions on faculty's role
                facultyRole.AddPermission(new Permission(StudentPermissionCodes.CreateHousingRequest));
                facultyRole.AddPermission(new Permission(StudentPermissionCodes.CreatePrerequisiteWaiver));
                facultyRole.AddPermission(new Permission(StudentPermissionCodes.CreateFacultyConsent));
                facultyRole.AddPermission(new Permission(StudentPermissionCodes.CreateStudentPetition));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { facultyRole });

                // Act - call method that determines whether current user has permissions
                var result = await facultyService.GetFacultyPermissions2Async();

                // Assert--result is true
                Assert.IsTrue(result.CanGrantFacultyConsent);
                Assert.IsTrue(result.CanWaivePrerequisiteRequirement);
                Assert.IsTrue(result.CanGrantStudentPetition);
                Assert.IsFalse(result.CanUpdateGrades);
            }

            [TestMethod]
            public async Task ReturnsAllFalseIfNoPermissions()
            {
                // Default faculty does not have any permissions
                // Act - call method that determines whether current user has permissions

                var result = await facultyService.GetFacultyPermissions2Async();

                Assert.IsFalse(result.CanGrantFacultyConsent);
                Assert.IsFalse(result.CanWaivePrerequisiteRequirement);
                Assert.IsFalse(result.CanGrantStudentPetition);
                Assert.IsFalse(result.CanUpdateGrades);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task ThrowsApplicationException()
            {
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(null);
                var result = await facultyService.GetFacultyPermissions2Async();
            }
        }

        [TestClass]
        public class SearchFacultyIdsAsync:CurrentUserSetup
        {
            private FacultyService facultyService;
            private Mock<IFacultyRepository> facultyRepoMock;
            private IFacultyRepository facultyRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private Mock<IStudentConfigurationRepository> configRepoMock;
            private IStudentConfigurationRepository configRepo;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IReferenceDataRepository> referenceRepoMock;
            private IReferenceDataRepository referenceRepo;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;

            [TestInitialize]
            public async void Initialize()
            {
                facultyRepoMock = new Mock<IFacultyRepository>();
                facultyRepo = facultyRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                configRepoMock = new Mock<IStudentConfigurationRepository>();
                configRepo = configRepoMock.Object;
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                referenceRepoMock = new Mock<IReferenceDataRepository>();
                referenceRepo = referenceRepoMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;

                // Set up current user
                currentUserFactory = new CurrentUserSetup.FacultyUserFactory();
                //mock faculty repo SearchFacultyIdsAsync method
                facultyRepoMock.Setup(frm => frm.SearchFacultyIdsAsync(It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync(new List<string>() { "0001", "0002" });
                facultyService = new FacultyService(adapterRegistry, facultyRepo, configRepo, sectionRepo, termRepo, referenceRepo, null, currentUserFactory, roleRepo, logger);
            }

            //user have VIEW.ANY.ADVISEE permission
            [TestMethod]
            public async Task FacultyHaveViewAdviseePermission()
            {
                //assign permission to current user
                facultyRole.AddPermission(new Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() { facultyRole });
                var result = await facultyService.SearchFacultyIdsAsync(true, false);
                Assert.IsNotNull(result);
                Assert.AreEqual(2, result.Count());
                Assert.AreEqual("0001", result.ToList()[0]);


            }
            //user does not have VIEW.ANY.ADVISEE permission
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task FacultyDoesNotHaveViewAdviseePermission()
            {
                //assign permission to current user
                facultyRole.AddPermission(new Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() { facultyRole });
                var result = await facultyService.SearchFacultyIdsAsync(false, false);
               
            }
            //when permission is empty or null
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task FacultyDoesNotHaveAnyPermissions()
            {
                //assign permission to current user
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() { facultyRole });
                var result = await facultyService.SearchFacultyIdsAsync(false, true);

            }
        }
    }
}


