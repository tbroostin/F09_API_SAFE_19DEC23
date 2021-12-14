// Copyright 2015-2021 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using RegistrationDate = Ellucian.Colleague.Domain.Student.Entities.RegistrationDate;
using Section = Ellucian.Colleague.Domain.Student.Entities.Section;
using Term = Ellucian.Colleague.Domain.Student.Entities.Term;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class SectionCoordinationServiceTests
    {
        public abstract class CurrentUserSetup
        {
            protected Role thirdPartyRole = new Role(1, "ThirdPartyCanUpdateGrades");
            protected Role createUpdateInstrEvent = new Role(1, "UPDATE.ROOM.BOOKING");

            public class StudentUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "Samwise",
                            PersonId = "STU1",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "Samwise",
                            Roles = new List<string>() { },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }

            // Represents a third party system like ILP
            public class ThirdPartyUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "ILP",
                            PersonId = "ILP",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "ILP",
                            Roles = new List<string>() { "ThirdPartyCanUpdateGrades",
                                                     "CreateAndUpdateRoomBooking",
                                                     "CreateAndUpdateFacultyBooking", "UPDATE.ROOM.BOOKING"},
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }

            public class FacultytUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "678",
                            Name = "Samwise",
                            PersonId = "0000678",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "Samwise-faculty",
                            Roles = new List<string>() { },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }
        }


        [TestClass]
        public class GetSectionRosterAsync : CurrentUserSetup
        {
            private SectionCoordinationService sectionCoordinationService;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IStudentReferenceDataRepository> stuRefDataRepoMock;
            private IStudentReferenceDataRepository studentReferenceDataRepo;
            private Mock<IReferenceDataRepository> referenceDataRepoMock;
            private IReferenceDataRepository referenceDataRepo;
            private Mock<ICourseRepository> courseRepoMock;
            private ICourseRepository courseRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IStudentConfigurationRepository> studentConfigRepoMock;
            private IStudentConfigurationRepository studentConfigRepo;
            private Mock<IConfigurationRepository> configRepoMock;
            private IConfigurationRepository configRepo;
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IRoomRepository> roomRepoMock;
            private IRoomRepository roomRepo;
            private Mock<IEventRepository> eventRepoMock;
            private IEventRepository eventRepo;
            private Mock<IBookRepository> bookRepoMock;
            private IBookRepository bookRepo;


            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ILogger logger;
            private ICurrentUserFactory currentUserFactory;

            private Domain.Student.Entities.Student student1;
            private Domain.Student.Entities.Student student2;
            private Domain.Student.Entities.Student student3;
            private Domain.Student.Entities.Section section1;
            private Domain.Student.Entities.Section section2;
            private Domain.Student.Entities.Section section3;
            private List<OfferingDepartment> dpts = new List<OfferingDepartment>() { new OfferingDepartment("ART", 100m) };
            private List<string> levels = new List<string>() { "Whatever" };
            private List<SectionStatusItem> statuses = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-60)) };

            [TestInitialize]
            public void Initialize()
            {
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                roomRepoMock = new Mock<IRoomRepository>();
                roomRepo = roomRepoMock.Object;
                stuRefDataRepoMock = new Mock<IStudentReferenceDataRepository>();
                studentReferenceDataRepo = stuRefDataRepoMock.Object;
                referenceDataRepoMock = new Mock<IReferenceDataRepository>();
                referenceDataRepo = referenceDataRepoMock.Object;
                courseRepoMock = new Mock<ICourseRepository>();
                courseRepo = courseRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                studentConfigRepoMock = new Mock<IStudentConfigurationRepository>();
                studentConfigRepo = studentConfigRepoMock.Object;
                configRepoMock = new Mock<IConfigurationRepository>();
                configRepo = configRepoMock.Object;
                eventRepoMock = new Mock<IEventRepository>();
                eventRepo = eventRepoMock.Object;
                bookRepoMock = new Mock<IBookRepository>();
                bookRepo = bookRepoMock.Object;


                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;

                // "STU1" is the current user
                currentUserFactory = new CurrentUserSetup.StudentUserFactory();

                logger = new Mock<ILogger>().Object;

                // Mock the student repo response
                student1 = new Domain.Student.Entities.Student("STU1", "Gamgee", null, new List<string>(), new List<string>());
                student1.FirstName = "Samwise";

                student2 = new Domain.Student.Entities.Student("STU2", "Took", null, new List<string>(), new List<string>());
                student2.FirstName = "Peregrin";

                student3 = new Domain.Student.Entities.Student("STU3", "Baggins", null, new List<string>(), new List<string>());
                student3.FirstName = "Frodo"; student3.MiddleName = "Ring-bearer";

                studentRepoMock.Setup(repo => repo.Get("STU1")).Returns(student1);
                studentRepoMock.Setup(repo => repo.Get("STU2")).Returns(student2);
                studentRepoMock.Setup(repo => repo.Get("STU3")).Returns(student3);

                // Mock the section repo response
                section1 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC1", "1119", "01", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses, true);
                section1.TermId = "2012/FA";
                section1.EndDate = new DateTime(2012, 12, 21); section1.AddActiveStudent("STU1");

                section2 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC2", "1119", "02", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses, true);
                section2.TermId = "2012/FA"; section2.AddActiveStudent("STU2");

                section3 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC3", "1119", "03", new DateTime(2011, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses, true);
                section3.TermId = "2011/FA"; section3.AddActiveStudent("STU1"); section3.AddActiveStudent("STU2"); section3.AddActiveStudent("STU3");
                section3.EndDate = new DateTime(2011, 12, 21);              

                List<string> sec1List = new List<string>() { "SEC1" };
                List<Domain.Student.Entities.Section> sec1Resp = new List<Domain.Student.Entities.Section>() { section1 };
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(sec1List, false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sec1Resp));

                List<string> sec2List = new List<string>() { "SEC2" };
                List<Domain.Student.Entities.Section> sec2Resp = new List<Domain.Student.Entities.Section>() { section2 };
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(sec2List, false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sec2Resp));

                List<string> sec3List = new List<string>() { "SEC3" };
                List<Domain.Student.Entities.Section> sec3Resp = new List<Domain.Student.Entities.Section>() { section3 };
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(sec3List, false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sec3Resp));


                List<string> sec4List = new List<string>() { "9999999" };
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(sec4List, false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(new List<Domain.Student.Entities.Section>()));

                var rosterStudentAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.RosterStudent, Ellucian.Colleague.Dtos.Student.RosterStudent>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.RosterStudent, Ellucian.Colleague.Dtos.Student.RosterStudent>()).Returns(rosterStudentAdapter);

                Domain.Student.Entities.RosterStudent rs1 = new Domain.Student.Entities.RosterStudent(student1.Id, student1.LastName) { FirstName = student1.FirstName, MiddleName = student1.MiddleName };
                Domain.Student.Entities.RosterStudent rs2 = new Domain.Student.Entities.RosterStudent(student2.Id, student2.LastName) { FirstName = student2.FirstName, MiddleName = student2.MiddleName };
                Domain.Student.Entities.RosterStudent rs3 = new Domain.Student.Entities.RosterStudent(student3.Id, student3.LastName) { FirstName = student3.FirstName, MiddleName = student3.MiddleName };

                studentRepoMock.Setup(repo => repo.GetRosterStudentsAsync(new List<string>() { "STU1" })).Returns(Task.FromResult(new List<Domain.Student.Entities.RosterStudent>() { rs1 }.AsEnumerable()));
                studentRepoMock.Setup(repo => repo.GetRosterStudentsAsync(new List<string>() { "STU1", "STU2", "STU3" })).Returns(Task.FromResult(new List<Domain.Student.Entities.RosterStudent>() { rs1, rs2, rs3 }.AsEnumerable()));

                // Mock the section service
                sectionCoordinationService = new SectionCoordinationService(adapterRegistry, sectionRepo, courseRepo, studentRepo, studentReferenceDataRepo,
                    referenceDataRepo, termRepo, studentConfigRepo, configRepo, personRepo, roomRepo, eventRepo, bookRepo, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionRepo = null;
                studentRepo = null;
                roleRepo = null;
                adapterRegistry = null;
                sectionCoordinationService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionCoordinationService_GetSectionRosterAsync_EmptySectionId()
            {
                var r = await sectionCoordinationService.GetSectionRosterAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task SectionCoordinationService_GetSectionRosterAsync_UserNotInSection()
            {
                var r = await sectionCoordinationService.GetSectionRosterAsync("SEC2");
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task SectionCoordinationService_GetSectionRosterAsync_InvalidSectionId()
            {
                var r = await sectionCoordinationService.GetSectionRosterAsync("9999999");
            }

            [TestMethod]
            public async Task SectionCoordinationService_GetSectionRosterAsync_OneStudent()
            {
                var res = await sectionCoordinationService.GetSectionRosterAsync("SEC1");
                Assert.AreEqual(1, res.Count());
            }

            [TestMethod]
            public async Task SectionCoordinationService_GetSectionRosterAsync_MultipleStudents()
            {
                var res = await sectionCoordinationService.GetSectionRosterAsync("SEC3");
                Assert.AreEqual(3, res.Count());
            }

            [TestMethod]
            public async Task SectionCoordinationService_GetSectionRosterAsync_RosterStudentId()
            {
                var res = await sectionCoordinationService.GetSectionRosterAsync("SEC3");
                Dtos.Student.RosterStudent rStu1 = res.ElementAt(0);
                Assert.AreEqual(student1.Id, rStu1.Id);
                Dtos.Student.RosterStudent rStu2 = res.ElementAt(1);
                Assert.AreEqual(student2.Id, rStu2.Id);
                Dtos.Student.RosterStudent rStu3 = res.ElementAt(2);
                Assert.AreEqual(student3.Id, rStu3.Id);
            }

            [TestMethod]
            public async Task SectionCoordinationService_GetSectionRosterAsync_RosterStudentFirstName()
            {
                var res = await sectionCoordinationService.GetSectionRosterAsync("SEC3");
                Dtos.Student.RosterStudent rStu1 = res.ElementAt(0);
                Assert.AreEqual(student1.FirstName, rStu1.FirstName);
                Dtos.Student.RosterStudent rStu2 = res.ElementAt(1);
                Assert.AreEqual(student2.FirstName, rStu2.FirstName);
                Dtos.Student.RosterStudent rStu3 = res.ElementAt(2);
                Assert.AreEqual(student3.FirstName, rStu3.FirstName);
            }

            [TestMethod]
            public async Task SectionCoordinationService_GetSectionRosterAsync_RosterStudentMiddleName()
            {
                var res = await sectionCoordinationService.GetSectionRosterAsync("SEC3");
                Dtos.Student.RosterStudent rStu1 = res.ElementAt(0);
                Assert.AreEqual(student1.MiddleName, rStu1.MiddleName);
                Dtos.Student.RosterStudent rStu2 = res.ElementAt(1);
                Assert.AreEqual(student2.MiddleName, rStu2.MiddleName);
                Dtos.Student.RosterStudent rStu3 = res.ElementAt(2);
                Assert.AreEqual(student3.MiddleName, rStu3.MiddleName);
            }

            [TestMethod]
            public async Task SectionCoordinationService_GetSectionRosterAsync_RosterStudentLastName()
            {
                var res = await sectionCoordinationService.GetSectionRosterAsync("SEC3");
                Dtos.Student.RosterStudent rStu1 = res.ElementAt(0);
                Assert.AreEqual(student1.LastName, rStu1.LastName);
                Dtos.Student.RosterStudent rStu2 = res.ElementAt(1);
                Assert.AreEqual(student2.LastName, rStu2.LastName);
                Dtos.Student.RosterStudent rStu3 = res.ElementAt(2);
                Assert.AreEqual(student3.LastName, rStu3.LastName);
            }
        }

        [TestClass]
        public class GetSectionRoster2Async : CurrentUserSetup
        {
            private SectionCoordinationService sectionCoordinationService;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IStudentReferenceDataRepository> stuRefDataRepoMock;
            private IStudentReferenceDataRepository studentReferenceDataRepo;
            private Mock<IReferenceDataRepository> referenceDataRepoMock;
            private IReferenceDataRepository referenceDataRepo;
            private Mock<ICourseRepository> courseRepoMock;
            private ICourseRepository courseRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IStudentConfigurationRepository> studentConfigRepoMock;
            private IStudentConfigurationRepository studentConfigRepo;
            private Mock<IConfigurationRepository> configRepoMock;
            private IConfigurationRepository configRepo;
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IRoomRepository> roomRepoMock;
            private IRoomRepository roomRepo;
            private Mock<IEventRepository> eventRepoMock;
            private IEventRepository eventRepo;
            private Mock<IBookRepository> bookRepoMock;
            private IBookRepository bookRepo;


            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ILogger logger;
            private ICurrentUserFactory currentUserFactory;

            private Domain.Student.Entities.SectionRoster sectionRoster1;
            private Domain.Student.Entities.SectionRoster sectionRoster2;
            private Domain.Student.Entities.SectionRoster sectionRoster3;

            [TestInitialize]
            public void Initialize()
            {
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                roomRepoMock = new Mock<IRoomRepository>();
                roomRepo = roomRepoMock.Object;
                stuRefDataRepoMock = new Mock<IStudentReferenceDataRepository>();
                studentReferenceDataRepo = stuRefDataRepoMock.Object;
                referenceDataRepoMock = new Mock<IReferenceDataRepository>();
                referenceDataRepo = referenceDataRepoMock.Object;
                courseRepoMock = new Mock<ICourseRepository>();
                courseRepo = courseRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                studentConfigRepoMock = new Mock<IStudentConfigurationRepository>();
                studentConfigRepo = studentConfigRepoMock.Object;
                configRepoMock = new Mock<IConfigurationRepository>();
                configRepo = configRepoMock.Object;
                eventRepoMock = new Mock<IEventRepository>();
                eventRepo = eventRepoMock.Object;
                bookRepoMock = new Mock<IBookRepository>();
                bookRepo = bookRepoMock.Object;


                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;

                // "STU1" is the current user
                currentUserFactory = new CurrentUserSetup.StudentUserFactory();

                logger = new Mock<ILogger>().Object;

                // Mock the section repo response
                sectionRoster1 = new Ellucian.Colleague.Domain.Student.Entities.SectionRoster("SEC1");
                sectionRoster1.AddStudentId("0001234");
                sectionRoster1.AddFacultyId("0001235");
                sectionRoster2 = new Ellucian.Colleague.Domain.Student.Entities.SectionRoster("SEC2");
                sectionRoster2.AddStudentId(currentUserFactory.CurrentUser.PersonId);
                sectionRoster2.AddFacultyId("0001235");
                sectionRoster3 = new Ellucian.Colleague.Domain.Student.Entities.SectionRoster("SEC3");
                sectionRoster3.AddStudentId("0001234");
                sectionRoster3.AddFacultyId(currentUserFactory.CurrentUser.PersonId);

                sectionRepoMock.Setup(repo => repo.GetSectionRosterAsync(sectionRoster1.SectionId)).ReturnsAsync(sectionRoster1);
                sectionRepoMock.Setup(repo => repo.GetSectionRosterAsync(sectionRoster2.SectionId)).ReturnsAsync(sectionRoster2);
                sectionRepoMock.Setup(repo => repo.GetSectionRosterAsync(sectionRoster3.SectionId)).ReturnsAsync(sectionRoster3);

                var sectionRosterAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionRoster, Ellucian.Colleague.Dtos.Student.SectionRoster>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionRoster, Ellucian.Colleague.Dtos.Student.SectionRoster>()).Returns(sectionRosterAdapter);

                // Mock the section service
                sectionCoordinationService = new SectionCoordinationService(adapterRegistry, sectionRepo, courseRepo, studentRepo, studentReferenceDataRepo,
                    referenceDataRepo, termRepo, studentConfigRepo, configRepo, personRepo, roomRepo, eventRepo, bookRepo, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionRepo = null;
                studentRepo = null;
                roleRepo = null;
                adapterRegistry = null;
                sectionCoordinationService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionCoordinationService_GetSectionRoster2Async_Null_SectionId()
            {
                var r = await sectionCoordinationService.GetSectionRoster2Async(null);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task SectionCoordinationService_GetSectionRoster2Async_User_not_in_Section()
            {
                var r = await sectionCoordinationService.GetSectionRoster2Async(sectionRoster1.SectionId);
            }

            [TestMethod]
            public async Task SectionCoordinationService_GetSectionRoster2Async_User_is_Student_in_Section()
            {
                var r = await sectionCoordinationService.GetSectionRoster2Async(sectionRoster2.SectionId);
                Assert.IsNotNull(r);
                Assert.AreEqual(sectionRoster2.SectionId, r.SectionId);
                Assert.AreEqual(sectionRoster2.StudentIds.Count, r.StudentIds.Count());
            }

            [TestMethod]
            public async Task SectionCoordinationService_GetSectionRoster2Async_User_is_Faculty_in_Section()
            {
                var r = await sectionCoordinationService.GetSectionRoster2Async(sectionRoster3.SectionId);
                Assert.IsNotNull(r);
                Assert.AreEqual(sectionRoster3.SectionId, r.SectionId);
                Assert.AreEqual(sectionRoster3.StudentIds.Count, r.StudentIds.Count());

            }
        }

        [TestClass]
        public class GetSectionWaitlistAsync : CurrentUserSetup
        {
            private SectionCoordinationService sectionCoordinationService;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IStudentReferenceDataRepository> stuRefDataRepoMock;
            private IStudentReferenceDataRepository studentReferenceDataRepo;
            private Mock<IReferenceDataRepository> referenceDataRepoMock;
            private IReferenceDataRepository referenceDataRepo;
            private Mock<ICourseRepository> courseRepoMock;
            private ICourseRepository courseRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IStudentConfigurationRepository> studentConfigRepoMock;
            private IStudentConfigurationRepository studentConfigRepo;
            private Mock<IConfigurationRepository> configRepoMock;
            private IConfigurationRepository configRepo;
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IRoomRepository> roomRepoMock;
            private IRoomRepository roomRepo;
            private Mock<IEventRepository> eventRepoMock;
            private IEventRepository eventRepo;
            private Mock<IBookRepository> bookRepoMock;
            private IBookRepository bookRepo;


            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ILogger logger;
            private ICurrentUserFactory currentUserFactory;

            private Domain.Student.Entities.Section section1;
            private Domain.Student.Entities.Section section2;
            private Domain.Student.Entities.Section section3;

            private Domain.Student.Entities.SectionWaitlist sectionWaitlist1;
            private Domain.Student.Entities.SectionWaitlist sectionWaitlist2;
            private Domain.Student.Entities.SectionWaitlist sectionWaitlist3;

            private List<OfferingDepartment> dpts = new List<OfferingDepartment>() { new OfferingDepartment("ART", 100m) };
            private List<string> levels = new List<string>() { "Whatever" };
            private List<SectionStatusItem> statuses = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-60)) };
            private List<SectionStatusItem> statuses2 = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Inactive, "I", DateTime.Today.AddDays(-60)) };

            [TestInitialize]
            public void Initialize()
            {
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                roomRepoMock = new Mock<IRoomRepository>();
                roomRepo = roomRepoMock.Object;
                stuRefDataRepoMock = new Mock<IStudentReferenceDataRepository>();
                studentReferenceDataRepo = stuRefDataRepoMock.Object;
                referenceDataRepoMock = new Mock<IReferenceDataRepository>();
                referenceDataRepo = referenceDataRepoMock.Object;
                courseRepoMock = new Mock<ICourseRepository>();
                courseRepo = courseRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                studentConfigRepoMock = new Mock<IStudentConfigurationRepository>();
                studentConfigRepo = studentConfigRepoMock.Object;
                configRepoMock = new Mock<IConfigurationRepository>();
                configRepo = configRepoMock.Object;
                eventRepoMock = new Mock<IEventRepository>();
                eventRepo = eventRepoMock.Object;
                bookRepoMock = new Mock<IBookRepository>();
                bookRepo = bookRepoMock.Object;


                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;

                // "STU1" is the current user
                currentUserFactory = new CurrentUserSetup.StudentUserFactory();

                logger = new Mock<ILogger>().Object;

                // Mock the section repo response
                sectionWaitlist1 = new Ellucian.Colleague.Domain.Student.Entities.SectionWaitlist("SEC1");
                sectionWaitlist1.AddStudentId("0001234");

                sectionWaitlist2 = new Ellucian.Colleague.Domain.Student.Entities.SectionWaitlist("SEC2");
                sectionWaitlist2.AddStudentId(currentUserFactory.CurrentUser.PersonId);

                sectionWaitlist3 = new Ellucian.Colleague.Domain.Student.Entities.SectionWaitlist("SEC3");
                sectionWaitlist3.AddStudentId("0001234");

                sectionRepoMock.Setup(repo => repo.GetSectionWaitlistAsync(sectionWaitlist1.SectionId)).ReturnsAsync(sectionWaitlist1);
                sectionRepoMock.Setup(repo => repo.GetSectionWaitlistAsync(sectionWaitlist2.SectionId)).ReturnsAsync(sectionWaitlist2);
                sectionRepoMock.Setup(repo => repo.GetSectionWaitlistAsync(sectionWaitlist3.SectionId)).ReturnsAsync(sectionWaitlist3);

                section1 = new Section("SEC1", "1119", "01", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses, true);
                section2 = new Section("SEC2", "1119", "02", new DateTime(2012, 09, 01), 3.0m, null, "Introduction to Art", "IN", dpts, levels, "UG", statuses2, true);
                section2.AddFaculty("STU1");

                section3 = null;

                sectionRepoMock.Setup(repo => repo.GetSectionAsync("SEC1")).ReturnsAsync(section1);
                sectionRepoMock.Setup(repo => repo.GetSectionAsync("SEC2")).ReturnsAsync(section2);
                sectionRepoMock.Setup(repo => repo.GetSectionAsync("SEC3")).ReturnsAsync(section3);

                var sectionWaitlistAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionWaitlist, Ellucian.Colleague.Dtos.Student.SectionWaitlist>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionWaitlist, Ellucian.Colleague.Dtos.Student.SectionWaitlist>()).Returns(sectionWaitlistAdapter);

                // Mock the section service
                sectionCoordinationService = new SectionCoordinationService(adapterRegistry, sectionRepo, courseRepo, studentRepo, studentReferenceDataRepo,
                    referenceDataRepo, termRepo, studentConfigRepo, configRepo, personRepo, roomRepo, eventRepo, bookRepo, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionRepo = null;
                studentRepo = null;
                roleRepo = null;
                adapterRegistry = null;
                sectionCoordinationService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionCoordinationService_GetSectionWaitlistAsync_Null_SectionId()
            {
                var r = await sectionCoordinationService.GetSectionWaitlistAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task SectionCoordinationService_GetSectionWaitlistAsync_User_not_in_Section()
            {
                var r = await sectionCoordinationService.GetSectionWaitlistAsync(sectionWaitlist1.SectionId);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task SectionCoordinationService_GetSectionWaitlistAsync_Null_Section()
            {
                var r = await sectionCoordinationService.GetSectionWaitlistAsync(sectionWaitlist3.SectionId);
            }

            [TestMethod]
            public async Task SectionCoordinationService_GetSectionWaitlistAsync_User_is_Student_in_Section()
            {
                var r = await sectionCoordinationService.GetSectionWaitlistAsync(sectionWaitlist2.SectionId);
                Assert.IsNotNull(r);
                Assert.AreEqual(sectionWaitlist2.SectionId, r.SectionId);
                Assert.AreEqual(sectionWaitlist2.StudentIds.Count, r.StudentIds.Count());
            }
        }

        [TestClass]
        public class GetSectionWaitlist2Async : CurrentUserSetup
        {
            private SectionCoordinationService sectionCoordinationService;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IStudentReferenceDataRepository> stuRefDataRepoMock;
            private IStudentReferenceDataRepository studentReferenceDataRepo;
            private Mock<IReferenceDataRepository> referenceDataRepoMock;
            private IReferenceDataRepository referenceDataRepo;
            private Mock<ICourseRepository> courseRepoMock;
            private ICourseRepository courseRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IStudentConfigurationRepository> studentConfigRepoMock;
            private IStudentConfigurationRepository studentConfigRepo;
            private Mock<IConfigurationRepository> configRepoMock;
            private IConfigurationRepository configRepo;
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IRoomRepository> roomRepoMock;
            private IRoomRepository roomRepo;
            private Mock<IEventRepository> eventRepoMock;
            private IEventRepository eventRepo;
            private Mock<IBookRepository> bookRepoMock;
            private IBookRepository bookRepo;


            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ILogger logger;
            private ICurrentUserFactory currentUserFactory;

            private Domain.Student.Entities.Section section1;
            private Domain.Student.Entities.Section section2;
            private Domain.Student.Entities.Section section3;

            private List<Domain.Student.Entities.SectionWaitlistStudent> sectionWaitlist1 = new List<Domain.Student.Entities.SectionWaitlistStudent>();
            private List<Domain.Student.Entities.SectionWaitlistStudent> sectionWaitlist2 = new List<Domain.Student.Entities.SectionWaitlistStudent>();
            private List<Domain.Student.Entities.SectionWaitlistStudent> sectionWaitlist3 = new List<Domain.Student.Entities.SectionWaitlistStudent>();

            private List<OfferingDepartment> dpts = new List<OfferingDepartment>() { new OfferingDepartment("ART", 100m) };
            private List<string> levels = new List<string>() { "Whatever" };
            private List<SectionStatusItem> statuses = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-60)) };
            private List<SectionStatusItem> statuses2 = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Inactive, "I", DateTime.Today.AddDays(-60)) };

            [TestInitialize]
            public void Initialize()
            {
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                roomRepoMock = new Mock<IRoomRepository>();
                roomRepo = roomRepoMock.Object;
                stuRefDataRepoMock = new Mock<IStudentReferenceDataRepository>();
                studentReferenceDataRepo = stuRefDataRepoMock.Object;
                referenceDataRepoMock = new Mock<IReferenceDataRepository>();
                referenceDataRepo = referenceDataRepoMock.Object;
                courseRepoMock = new Mock<ICourseRepository>();
                courseRepo = courseRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                studentConfigRepoMock = new Mock<IStudentConfigurationRepository>();
                studentConfigRepo = studentConfigRepoMock.Object;
                configRepoMock = new Mock<IConfigurationRepository>();
                configRepo = configRepoMock.Object;
                eventRepoMock = new Mock<IEventRepository>();
                eventRepo = eventRepoMock.Object;
                bookRepoMock = new Mock<IBookRepository>();
                bookRepo = bookRepoMock.Object;


                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;

                // "STU1" is the current user
                currentUserFactory = new CurrentUserSetup.StudentUserFactory();

                logger = new Mock<ILogger>().Object;

                // Mock the section repo response
                sectionWaitlist1.Add(new Ellucian.Colleague.Domain.Student.Entities.SectionWaitlistStudent("SEC1", "0001234", 1, 3, "1", new DateTime(), new DateTime(), new DateTime()));

                sectionWaitlist2.Add(new Ellucian.Colleague.Domain.Student.Entities.SectionWaitlistStudent("SEC2", currentUserFactory.CurrentUser.PersonId, 2, 2, "1", new DateTime(), new DateTime(), new DateTime()));

                sectionWaitlist3.Add(new Ellucian.Colleague.Domain.Student.Entities.SectionWaitlistStudent("SEC3", "0001234", 3, 1, "1", new DateTime(), new DateTime(), new DateTime()));

                
                sectionRepoMock.Setup(repo => repo.GetSectionWaitlist2Async(new List<string>() { sectionWaitlist1[0].SectionId })).ReturnsAsync(sectionWaitlist1);
                sectionRepoMock.Setup(repo => repo.GetSectionWaitlist2Async(new List<string>() { sectionWaitlist2[0].SectionId })).ReturnsAsync(sectionWaitlist2);
                sectionRepoMock.Setup(repo => repo.GetSectionWaitlist2Async(new List<string>() { sectionWaitlist3[0].SectionId })).ReturnsAsync(sectionWaitlist3);

                section1 = new Section("SEC1", "1119", "01", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses, true);
                section2 = new Section("SEC2", "1119", "02", new DateTime(2012, 09, 01), 3.0m, null, "Introduction to Art", "IN", dpts, levels, "UG", statuses2, true);
                section2.AddFaculty("STU1");

                section3 = null;

                sectionRepoMock.Setup(repo => repo.GetSectionAsync("SEC1")).ReturnsAsync(section1);
                sectionRepoMock.Setup(repo => repo.GetSectionAsync("SEC2")).ReturnsAsync(section2);
                sectionRepoMock.Setup(repo => repo.GetSectionAsync("SEC3")).ReturnsAsync(section3);

                var sectionWaitlistAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionWaitlistStudent, Ellucian.Colleague.Dtos.Student.SectionWaitlistStudent>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionWaitlistStudent, Ellucian.Colleague.Dtos.Student.SectionWaitlistStudent>()).Returns(sectionWaitlistAdapter);

                // Mock the section service
                sectionCoordinationService = new SectionCoordinationService(adapterRegistry, sectionRepo, courseRepo, studentRepo, studentReferenceDataRepo,
                    referenceDataRepo, termRepo, studentConfigRepo, configRepo, personRepo, roomRepo, eventRepo, bookRepo, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionRepo = null;
                studentRepo = null;
                roleRepo = null;
                adapterRegistry = null;
                sectionCoordinationService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionCoordinationService_GetSectionWaitlist2Async_Null_SectionId()
            {
                var r = await sectionCoordinationService.GetSectionWaitlist2Async(null);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task SectionCoordinationService_GetSectionWaitlist2Async_User_not_in_Section()
            {
                var r = await sectionCoordinationService.GetSectionWaitlist2Async(sectionWaitlist1[0].SectionId);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task SectionCoordinationService_GetSectionWaitlist2Async_Null_Section()
            {
                var r = await sectionCoordinationService.GetSectionWaitlist2Async(sectionWaitlist3[0].SectionId);
            }

            [TestMethod]
            public async Task SectionCoordinationService_GetSectionWaitlistAsync_User_is_Student_in_Section()
            {
                var r = await sectionCoordinationService.GetSectionWaitlist2Async(sectionWaitlist2[0].SectionId);
                Assert.IsNotNull(r);
                Assert.AreEqual(sectionWaitlist2[0].SectionId, r.ElementAt(0).SectionId);
                Assert.AreEqual(sectionWaitlist2[0].StudentId, r.ElementAt(0).StudentId);
                Assert.AreEqual(sectionWaitlist2[0].Rating, r.ElementAt(0).Rating);
                Assert.AreEqual(sectionWaitlist2[0].StatusDate, r.ElementAt(0).StatusDate);
                Assert.AreEqual(sectionWaitlist2.Count(), r.Count());
            }
        }

        [TestClass]
        public class UserCanUpdateGrades : CurrentUserSetup
        {
            private SectionCoordinationService sectionCoordinationService;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ILogger logger;
            private ICurrentUserFactory currentUserFactory;
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IRoomRepository> roomRepoMock;
            private IRoomRepository roomRepo;
            private Mock<IEventRepository> eventRepoMock;
            private IEventRepository eventRepo;
            private Mock<IBookRepository> bookRepoMock;
            private IBookRepository bookRepo;


            private Mock<IStudentReferenceDataRepository> stuRefDataRepoMock;
            private IStudentReferenceDataRepository studentReferenceDataRepo;
            private Mock<IReferenceDataRepository> referenceDataRepoMock;
            private IReferenceDataRepository referenceDataRepo;
            private Mock<ICourseRepository> courseRepoMock;
            private ICourseRepository courseRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IStudentConfigurationRepository> studentConfigRepoMock;
            private IStudentConfigurationRepository studentConfigRepo;
            private Mock<IConfigurationRepository> configRepoMock;
            private IConfigurationRepository configRepo;

            private Dtos.Student.SectionGrades3 sectionGrades3Dto;
            private Domain.Student.Entities.SectionGradeSectionResponse sectionGradeResponse;
            private Domain.Student.Entities.SectionGradeSectionResponse sectionGradeResponseNonIlp;

            [TestInitialize]
            public void Initialize()
            {
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                logger = new Mock<ILogger>().Object;
                currentUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                roomRepoMock = new Mock<IRoomRepository>();
                roomRepo = roomRepoMock.Object;
                stuRefDataRepoMock = new Mock<IStudentReferenceDataRepository>();
                studentReferenceDataRepo = stuRefDataRepoMock.Object;
                referenceDataRepoMock = new Mock<IReferenceDataRepository>();
                referenceDataRepo = referenceDataRepoMock.Object;
                courseRepoMock = new Mock<ICourseRepository>();
                courseRepo = courseRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                studentConfigRepoMock = new Mock<IStudentConfigurationRepository>();
                studentConfigRepo = studentConfigRepoMock.Object;
                configRepoMock = new Mock<IConfigurationRepository>();
                configRepo = configRepoMock.Object;
                eventRepoMock = new Mock<IEventRepository>();
                eventRepo = eventRepoMock.Object;
                bookRepoMock = new Mock<IBookRepository>();
                bookRepo = bookRepoMock.Object;

                var gradeAdapter = new Ellucian.Colleague.Coordination.Student.Adapters.SectionGradesAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Dtos.Student.SectionGrades, Ellucian.Colleague.Domain.Student.Entities.SectionGrades>()).Returns(gradeAdapter);

                var grade2Adapter = new Ellucian.Colleague.Coordination.Student.Adapters.SectionGrades2Adapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Dtos.Student.SectionGrades2, Ellucian.Colleague.Domain.Student.Entities.SectionGrades>()).Returns(grade2Adapter);

                var grade3Adapter = new Ellucian.Web.Adapters.AutoMapperAdapter<Ellucian.Colleague.Dtos.Student.SectionGrades3, Ellucian.Colleague.Domain.Student.Entities.SectionGrades>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Dtos.Student.SectionGrades3, Ellucian.Colleague.Domain.Student.Entities.SectionGrades>()).Returns(grade3Adapter);

                var grade4Adapter = new Ellucian.Web.Adapters.AutoMapperAdapter<Ellucian.Colleague.Dtos.Student.SectionGrades4, Ellucian.Colleague.Domain.Student.Entities.SectionGrades>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Dtos.Student.SectionGrades4, Ellucian.Colleague.Domain.Student.Entities.SectionGrades>()).Returns(grade4Adapter);

                var responseAdapter = new Ellucian.Colleague.Coordination.Student.Adapters.SectionGradeResponseAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionGradeResponse, Ellucian.Colleague.Dtos.Student.SectionGradeResponse>()).Returns(responseAdapter);

                // For testing the v1 and v2 put grades services.
                // Mock repository ImportGradesAsync to return GetSectionGradeResponse if called with any SectionGrades, false for forceNoVerifyFlag, 
                // false for checkForLocksFlag, ILP for callerType, false for sendGradingCompleteEmail.
                // The v1 and v2 services will always pass those values for the last three arguments.
                sectionRepoMock.Setup(r => r.ImportGradesAsync(It.IsAny<Ellucian.Colleague.Domain.Student.Entities.SectionGrades>(), It.Is<bool>(b => b == false),
                        It.Is<bool>(b => b == false), It.Is<GradesPutCallerTypes>(ct => ct == GradesPutCallerTypes.ILP), It.Is<bool>(b => b == false))).Returns
                        (Task.FromResult<Domain.Student.Entities.SectionGradeSectionResponse>(GetSectionGradeResponse()));

                // Instantiate a SectionGrades3 dto with particular values that will invoke a particular ImportGradesAsync repository mock.
                // Set forceNoVerify true here, which will eventually pass true in forceNoVerify to the ImportGradesAsync repository method.
                sectionGrades3Dto = new Dtos.Student.SectionGrades3()
                {
                    SectionId = "Section3",
                    ForceNoVerifyFlag = true,
                    StudentGrades = new List<StudentGrade2>() { new StudentGrade2() { FinalGrade = "C" } }
                };

                // Instantiate a response from repository ImportGradesAsync
                sectionGradeResponse = new Domain.Student.Entities.SectionGradeSectionResponse();
                sectionGradeResponse.StudentResponses = 
                    new List<Domain.Student.Entities.SectionGradeResponse>()
                    {
                        new Domain.Student.Entities.SectionGradeResponse ()
                        {
                            Status = "Failure",
                            Errors = new List<Domain.Student.Entities.SectionGradeResponseError>() {
                                new Domain.Student.Entities.SectionGradeResponseError() { Message = "Grades3TestError" } }
                        }
                    };

                // Instantiate an alternative repository response when called for a non-ILP caller type
                sectionGradeResponseNonIlp = new Domain.Student.Entities.SectionGradeSectionResponse();
                sectionGradeResponseNonIlp.StudentResponses = 
                    new List<Domain.Student.Entities.SectionGradeResponse>()
                    {
                        new Domain.Student.Entities.SectionGradeResponse ()
                        {
                            Status = "Failure",
                            Errors = new List<Domain.Student.Entities.SectionGradeResponseError>() {
                                new Domain.Student.Entities.SectionGradeResponseError() { Message = "NonIlpPutGradesTestError" } }
                        }
                    };


                // For testing the v3 put grades service and the v1 ilp put grades service which are essentially identical.
                // Mock the repository ImportGradesAsync to return sectionGradeResponse when called with the data in sectionGrades3Dto, true for forceNoVerifyFlag (
                // from the dto), true for checkForLocksFlag (which will always be true from the v3 service),  ILP for callerType (which is always the value
                // from the v3 service or the v1 ilp service.), and false for sendGradingCompleteEmail (always value for v3 or v1 ilp)
                sectionRepoMock.Setup(r =>
                    r.ImportGradesAsync(
                        It.Is<Ellucian.Colleague.Domain.Student.Entities.SectionGrades>(sg => sg.SectionId == sectionGrades3Dto.SectionId &&
                            sg.StudentGrades[0].FinalGrade == sectionGrades3Dto.StudentGrades[0].FinalGrade), It.Is<bool>(b => b == true),
                            It.Is<bool>(b => b == true), It.Is<GradesPutCallerTypes>(ct => ct == GradesPutCallerTypes.ILP), It.Is<bool>(b => b == false))).Returns(
                        Task.FromResult<Domain.Student.Entities.SectionGradeSectionResponse>(sectionGradeResponse));

                // For testing the v3 put grades service when the incoming dto specifies no section, such that the user will fail to be a faculty
                // of the section, and therefore must have the permission to grade all sections.
                // Also return a null entity from the repository, to make sure the service method handles a null object.
                // Mock the repository ImportGradesAsync to return sectionGradeResponse when called with any dto, false for forceNoVerifyFlag (
                // default when not specified), true for checkForLocksFlag (which will always be true from the v3 service),  ILP for callerType (which
                // is always the value from the v3 service or the v1 ilp service.), and false for sendGradingCompleteEmail (always value for v3 or v1 ilp)
                sectionRepoMock.Setup(r =>
                    r.ImportGradesAsync(
                        It.IsAny<Ellucian.Colleague.Domain.Student.Entities.SectionGrades>(), It.Is<bool>(b => b == false),
                            It.Is<bool>(b => b == true), It.Is<GradesPutCallerTypes>(ct => ct == GradesPutCallerTypes.ILP), It.Is<bool>(b => b == false))).Returns(
                        Task.FromResult<Domain.Student.Entities.SectionGradeSectionResponse>(null));

                // For testing the v4 put grades service.
                // Mock the repository ImportGradesAsync to return sectionGradeResponseNonIlp when called with the data in sectionGrades3Dto, true for 
                // forceNoVerifyFlag (from the dto), true for checkForLocksFlag (which will always be true from the v3 service), and Standard for callerType 
                // (which is always the value from the v4 service.), and false for sendGradingCompleteEmail (always value for v4)
                sectionRepoMock.Setup(r =>
                    r.ImportGradesAsync(
                        It.Is<Ellucian.Colleague.Domain.Student.Entities.SectionGrades>(sg => sg.SectionId == sectionGrades3Dto.SectionId &&
                            sg.StudentGrades[0].FinalGrade == sectionGrades3Dto.StudentGrades[0].FinalGrade), It.Is<bool>(b => b == true),
                            It.Is<bool>(b => b == true), It.Is<GradesPutCallerTypes>(ct => ct == GradesPutCallerTypes.Standard), It.Is<bool>(b => b == false))).Returns(
                        Task.FromResult<Domain.Student.Entities.SectionGradeSectionResponse>(sectionGradeResponseNonIlp));

                // Mock repository GetCachedSectionsAsync to return the section in sectionGrades3Dto with CurrentUser.PersonId as a faculty
                // member. This will be used to test the permission which allows faculty members to grade sections they teach.
                // Only the sectionId and FacultyIds need values for the test.
                var section = new Ellucian.Colleague.Domain.Student.Entities.Section(sectionGrades3Dto.SectionId, "courseId", "number", new DateTime(2016, 1, 1),
                       3M, null, "title", "credittypecode", new List<OfferingDepartment>() { new OfferingDepartment("deptA", 100M) },
                       new List<string>() { "courselevel" }, "acadlevelcode",
                       new List<SectionStatusItem>() { new SectionStatusItem(new SectionStatus(), "code", new DateTime(2016, 1, 1)) });
                section.AddFaculty(currentUserFactory.CurrentUser.PersonId);
                sectionRepoMock.Setup(r =>
                    r.GetCachedSectionsAsync(It.Is<IEnumerable<string>>(s => s.ElementAt(0) == sectionGrades3Dto.SectionId),
                        It.IsAny<bool>())).Returns(
                        Task.FromResult<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section>>(
                            new List<Ellucian.Colleague.Domain.Student.Entities.Section>() { section })
                    );

                sectionCoordinationService = new SectionCoordinationService(adapterRegistry, sectionRepo, courseRepo, studentRepo, studentReferenceDataRepo,
                    referenceDataRepo, termRepo, studentConfigRepo, configRepo, personRepo, roomRepo, eventRepo, bookRepo, currentUserFactory, roleRepo, logger);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionService_NullInputThrowsException()
            {
                var r = await sectionCoordinationService.ImportGradesAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionService_ImportGrades3Async_NullInputThrowsException()
            {
                var r = await sectionCoordinationService.ImportGrades3Async(null);
            }


            [TestMethod]
            [ExpectedException(typeof(Ellucian.Web.Security.PermissionsException))]
            public async Task SectionService_ImportGradesThrowsPermissionsException()
            {
                thirdPartyRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission("some other permission"));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { thirdPartyRole });
                var r = await sectionCoordinationService.ImportGradesAsync(new Dtos.Student.SectionGrades());
            }

            [TestMethod]
            [ExpectedException(typeof(Ellucian.Web.Security.PermissionsException))]
            public async Task SectionService_ImportGrades2ThrowsPermissionsException()
            {
                thirdPartyRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission("some other permission"));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { thirdPartyRole });
                var r = await sectionCoordinationService.ImportGrades2Async(new Dtos.Student.SectionGrades2());
            }

            [TestMethod]
            [ExpectedException(typeof(Ellucian.Web.Security.PermissionsException))]
            public async Task SectionService__ImportGrades3Async_NoPermissionsAndNotTeacherOfSection()
            {
                // No UpdateGrades permission
                thirdPartyRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission("some other permission"));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { thirdPartyRole });
                // No section passed, so cannot be teacher of section.
                var r = await sectionCoordinationService.ImportGrades3Async(new Dtos.Student.SectionGrades3());
            }

            [TestMethod]
            public async Task SectionService__ImportGrades3Async_UpdateGradesPermissionsWorks()
            {
                
                thirdPartyRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(SectionPermissionCodes.UpdateGrades));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { thirdPartyRole });
                // No section passed, so cannot be teacher of section.
                // If the permissions fail, an exception will be thrown and this test will fail. If the permissions
                // pass as expected, no exception will be thrown and the test will succeed.
                var r = await sectionCoordinationService.ImportGrades3Async(new Dtos.Student.SectionGrades3());
            }

            [TestMethod]
            public async Task SectionService__ImportGrades3Async_TeacherOfSectionIsGrantedPermission()
            {
                // No UpdateGrades permission
                thirdPartyRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission("some other permission"));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { thirdPartyRole });

                // sectionGrades3Dto contains a sectionId that will cause the mocked repository to return a section with 
                // CurrentUser.PersonId in FacultyIds. Therefore the permissions should pass and no exception should be thrown.
                var r = await sectionCoordinationService.ImportGrades3Async(sectionGrades3Dto);
            }

            [TestMethod]
            [ExpectedException(typeof(Ellucian.Web.Security.PermissionsException))]
            public async Task SectionService__ImportIlpGrades1Async_NoPermissionsAndNotTeacherOfSection()
            {
                // No UpdateGrades permission
                thirdPartyRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission("some other permission"));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { thirdPartyRole });
                // No section passed, so cannot be teacher of section.
                var r = await sectionCoordinationService.ImportIlpGrades1Async(new Dtos.Student.SectionGrades3());
            }

            [TestMethod]
            public async Task SectionService__ImportIlpGrades1Async_UpdateGradesPermissionsWorks()
            {
                thirdPartyRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(SectionPermissionCodes.UpdateGrades));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { thirdPartyRole });
                // No section passed, so cannot be teacher of section.
                // If the permissions fail, an exception will be thrown and this test will fail. If the permissions
                // pass as expected, no exception will be thrown and the test will succeed.
                var r = await sectionCoordinationService.ImportIlpGrades1Async(new Dtos.Student.SectionGrades3());
            }

            [TestMethod]
            public async Task SectionService__ImportIlpGrades1Async_TeacherOfSectionIsGrantedPermission()
            {
                // No UpdateGrades permission
                thirdPartyRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission("some other permission"));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { thirdPartyRole });

                // sectionGrades3Dto contains a sectionId that will cause the mocked repository to return a section with 
                // CurrentUser.PersonId in FacultyIds. Therefore the permissions should pass and no exception should be thrown.
                var r = await sectionCoordinationService.ImportIlpGrades1Async(sectionGrades3Dto);
            }

            [TestMethod]
            [ExpectedException(typeof(Ellucian.Web.Security.PermissionsException))]
            public async Task SectionService__ImportGrades4Async_NoPermissionsAndNotTeacherOfSection()
            {
                // No UpdateGrades permission
                thirdPartyRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission("some other permission"));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { thirdPartyRole });
                // No section passed, so cannot be teacher of section.
                var r = await sectionCoordinationService.ImportGrades4Async(new Dtos.Student.SectionGrades3());
            }

            [TestMethod]
            public async Task SectionService__ImportGrades4Async_UpdateGradesPermissionsWorks()
            {
                thirdPartyRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(SectionPermissionCodes.UpdateGrades));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { thirdPartyRole });
                // No section passed, so cannot be teacher of section.
                // If the permissions fail, an exception will be thrown and this test will fail. If the permissions
                // pass as expected, no exception will be thrown and the test will succeed.
                var r = await sectionCoordinationService.ImportGrades4Async(new Dtos.Student.SectionGrades3());
            }

            [TestMethod]
            public async Task SectionService__ImportIlpGrades4Async_TeacherOfSectionIsGrantedPermission()
            {
                // No UpdateGrades permission
                thirdPartyRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission("some other permission"));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { thirdPartyRole });

                // sectionGrades3Dto contains a sectionId that will cause the mocked repository to return a section with 
                // CurrentUser.PersonId in FacultyIds. Therefore the permissions should pass and no exception should be thrown.
                var r = await sectionCoordinationService.ImportGrades4Async(sectionGrades3Dto);
            }


            [TestMethod]
            public void SectionService_ImportGrades()
            {
                thirdPartyRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(SectionPermissionCodes.UpdateGrades));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { thirdPartyRole });
                // The mock of ImportGradesAsync in the repository expects "false" in forceNoVerify and false in checkForLocksFlag
                // which the v1 service should pass.
                var response = sectionCoordinationService.ImportGradesAsync(new Dtos.Student.SectionGrades()).Result;

                var expected = GetSectionGradeResponse();
                Assert.AreEqual(expected.StudentResponses[0].StudentId, response.First().StudentId);
                Assert.AreEqual(expected.StudentResponses[0].Status, response.First().Status);
                Assert.AreEqual(expected.StudentResponses[0].Errors.Count(), response.First().Errors.Count());
                Assert.AreEqual(expected.StudentResponses[0].Errors[0].Message, response.First().Errors[0].Message);
                Assert.AreEqual(expected.StudentResponses[0].Errors[0].Property, response.First().Errors[0].Property);
            }

            [TestMethod]
            public void SectionService_ImportGrades2()
            {
                thirdPartyRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(SectionPermissionCodes.UpdateGrades));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { thirdPartyRole });
                // The mock of ImportGradesAsync in the repository expects "false" in forceNoVerify and false in checkForLocksFlag
                // which the v2 service should pass.
                var response = sectionCoordinationService.ImportGrades2Async(new Dtos.Student.SectionGrades2()).Result;

                var expected = GetSectionGradeResponse();
                Assert.AreEqual(expected.StudentResponses[0].StudentId, response.First().StudentId);
                Assert.AreEqual(expected.StudentResponses[0].Status, response.First().Status);
                Assert.AreEqual(expected.StudentResponses[0].Errors.Count(), response.First().Errors.Count());
                Assert.AreEqual(expected.StudentResponses[0].Errors[0].Message, response.First().Errors[0].Message);
                Assert.AreEqual(expected.StudentResponses[0].Errors[0].Property, response.First().Errors[0].Property);
            }

            [TestMethod]
            public void SectionService_ImportGrades3Async_SuccessfulExecution()
            {
                // Give the permission to update all grades.
                thirdPartyRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(SectionPermissionCodes.UpdateGrades));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { thirdPartyRole });

                // This will test the mapping of the dto to the domain entity which is passed to the repository ImportGradesAsync.
                // It will also test that the service calls the repository method with the expected arguments.
                // The mock repository will return sectionGradeResponse when passed sectionGrades3Dto, true in forceNoVerifyFlag which comes from
                // sectionGrades3Dto, true in checkForLocks which ImportIlpGrades1Async should always pass, and ILP as caller type which 
                // ImportGrades3Async always passes.
                IEnumerable<Dtos.Student.SectionGradeResponse> response = sectionCoordinationService.ImportGrades3Async(sectionGrades3Dto).Result;

                Assert.AreEqual(sectionGradeResponse.StudentResponses[0].Status, response.First().Status);
                Assert.AreEqual(sectionGradeResponse.StudentResponses[0].Errors.Count(), response.First().Errors.Count());
                Assert.AreEqual(sectionGradeResponse.StudentResponses[0].Errors[0].Message, response.First().Errors[0].Message);
            }

            [TestMethod]
            public void SectionService_ImportIlpGrades1Async_SuccessfulExecution()
            {
                // ImportIlpGrades1Async has the same logic as ImportGrades3Async, so the test is the same.

                // Give the permission to update all grades.
                thirdPartyRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(SectionPermissionCodes.UpdateGrades));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { thirdPartyRole });

                // This will test the mapping of the dto to the domain entity which is passed to the repository ImportGradesAsync.
                // It will also test that the service calls the repository method with the expected arguments.
                // The mock repository will return sectionGradeResponse when passed sectionGrades3Dto, true in forceNoVerifyFlag which comes from
                // sectionGrades3Dto, true in checkForLocks which ImportIlpGrades1Async should always pass, and ILP as caller type which 
                // ImportIlpGrades1Async always passes.
                IEnumerable<Dtos.Student.SectionGradeResponse> response = sectionCoordinationService.ImportIlpGrades1Async(sectionGrades3Dto).Result;

                Assert.AreEqual(sectionGradeResponse.StudentResponses[0].Status, response.First().Status);
                Assert.AreEqual(sectionGradeResponse.StudentResponses[0].Errors.Count(), response.First().Errors.Count());
                Assert.AreEqual(sectionGradeResponse.StudentResponses[0].Errors[0].Message, response.First().Errors[0].Message);
            }

            [TestMethod]
            public void SectionService_ImportGrades4Async_SuccessfulExecution()
            {
                // Give the permission to update all grades.
                thirdPartyRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(SectionPermissionCodes.UpdateGrades));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { thirdPartyRole });

                // This will test the mapping of the dto to the domain entity which is passed to the repository ImportGradesAsync.
                // It will also test that the service calls the repository method with the expected arguments.
                // The mock repository will return sectionGradeResponseNonIlp when passed sectionGrades3Dto, true in forceNoVerifyFlag which comes from
                // sectionGrades3Dto, true in checkForLocks which ImportIlpGrades1Async should always pass, and Standdard as caller type which 
                // ImportGrades4Async always passes.
                IEnumerable<Dtos.Student.SectionGradeResponse> response = sectionCoordinationService.ImportGrades4Async(sectionGrades3Dto).Result;

                Assert.AreEqual(sectionGradeResponseNonIlp.StudentResponses[0].Status, response.First().Status);
                Assert.AreEqual(sectionGradeResponseNonIlp.StudentResponses[0].Errors.Count(), response.First().Errors.Count());
                Assert.AreEqual(sectionGradeResponseNonIlp.StudentResponses[0].Errors[0].Message, response.First().Errors[0].Message);
            }

            private Ellucian.Colleague.Domain.Student.Entities.SectionGradeSectionResponse GetSectionGradeResponse()
            {
                var studentResponse = new Ellucian.Colleague.Domain.Student.Entities.SectionGradeResponse();
                studentResponse.StudentId = "101";
                studentResponse.Status = "status";

                var error = new Ellucian.Colleague.Domain.Student.Entities.SectionGradeResponseError();
                error.Message = "message";
                error.Property = "property";
                studentResponse.Errors.Add(error);

                List<Ellucian.Colleague.Domain.Student.Entities.SectionGradeResponse> list = new List<Domain.Student.Entities.SectionGradeResponse>();
                list.Add(studentResponse);
                var sectionResponse = new Domain.Student.Entities.SectionGradeSectionResponse();
                sectionResponse.StudentResponses = list;
                return sectionResponse;
            }
        }

        [TestClass]
        public class InstructionalEvents_Get : CurrentUserSetup
        {
            private SectionCoordinationService sectionCoordinationService;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IStudentReferenceDataRepository> stuRefDataRepoMock;
            private IStudentReferenceDataRepository studentReferenceDataRepo;
            private Mock<IReferenceDataRepository> referenceDataRepoMock;
            private IReferenceDataRepository referenceDataRepo;
            private Mock<ICourseRepository> courseRepoMock;
            private ICourseRepository courseRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IStudentConfigurationRepository> studentConfigRepoMock;
            private IStudentConfigurationRepository studentConfigRepo;
            private Mock<IConfigurationRepository> configRepoMock;
            private IConfigurationRepository configRepo;
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IRoomRepository> roomRepoMock;
            private IRoomRepository roomRepo;
            private Mock<IEventRepository> eventRepoMock;
            private IEventRepository eventRepo;
            private Mock<IBookRepository> bookRepoMock;
            private IBookRepository bookRepo;


            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ILogger logger;
            private ICurrentUserFactory currentUserFactory;

            private Domain.Student.Entities.Section section1;
            private Domain.Student.Entities.Section section2;
            private Domain.Student.Entities.Section section3;
            private Domain.Student.Entities.SectionMeeting meeting1;
            private Domain.Student.Entities.SectionMeeting meeting2;
            private Domain.Student.Entities.SectionMeeting meeting3;
            private Domain.Student.Entities.SectionMeeting meeting4;

            private List<OfferingDepartment> dpts = new List<OfferingDepartment>() { new OfferingDepartment("ART", 100m) };
            private List<string> levels = new List<string>() { "Whatever" };
            private List<SectionStatusItem> statuses = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-60)) };

            private string meeting1Guid = Guid.NewGuid().ToString();
            private string instructionalMethodGuid = Guid.NewGuid().ToString();
            private string roomGuid = Guid.NewGuid().ToString();

            [TestInitialize]
            public void Initialize()
            {
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                roomRepoMock = new Mock<IRoomRepository>();
                roomRepo = roomRepoMock.Object;
                stuRefDataRepoMock = new Mock<IStudentReferenceDataRepository>();
                studentReferenceDataRepo = stuRefDataRepoMock.Object;
                referenceDataRepoMock = new Mock<IReferenceDataRepository>();
                referenceDataRepo = referenceDataRepoMock.Object;
                courseRepoMock = new Mock<ICourseRepository>();
                courseRepo = courseRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                studentConfigRepoMock = new Mock<IStudentConfigurationRepository>();
                studentConfigRepo = studentConfigRepoMock.Object;
                configRepoMock = new Mock<IConfigurationRepository>();
                configRepo = configRepoMock.Object;
                eventRepoMock = new Mock<IEventRepository>();
                eventRepo = eventRepoMock.Object;
                bookRepoMock = new Mock<IBookRepository>();
                bookRepo = bookRepoMock.Object;


                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;

                // "STU1" is the current user
                currentUserFactory = new CurrentUserSetup.StudentUserFactory();

                logger = new Mock<ILogger>().Object;

                // Mock the section repo response
                section1 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC1", "1119", "01", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses, true);
                section1.TermId = "2012/FA";
                section1.EndDate = new DateTime(2012, 12, 21);

                section2 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC2", "1119", "02", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses, true);
                section2.TermId = "2012/FA";

                section3 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC3", "1119", "03", new DateTime(2011, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses, true);
                section3.TermId = "2011/FA";
                section3.EndDate = new DateTime(2011, 12, 21);

                var sectionFaculty = new Domain.Student.Entities.SectionFaculty("1234", "SEC1", "0000678", "LEC", new DateTime(2012, 09, 01), new DateTime(2011, 12, 21), 100.0m);

                meeting1 = new Domain.Student.Entities.SectionMeeting("123", "SEC1", "LEC", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "W");
                meeting1.Guid = meeting1Guid;
                meeting1.AddFacultyId("0000678");
                meeting1.AddSectionFaculty(sectionFaculty);
                meeting1.StartTime = new DateTimeOffset(2012, 01, 01, 08, 30, 00, new TimeSpan());
                meeting1.EndTime = new DateTimeOffset(2012, 12, 21, 09, 30, 00, new TimeSpan());
                meeting1.Room = "DAN*102";
                section1.AddSectionMeeting(meeting1);

                meeting2 = new Domain.Student.Entities.SectionMeeting("123", "SEC1", "LEC", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "D");
                meeting2.Guid = meeting1Guid;

                meeting3 = new Domain.Student.Entities.SectionMeeting("456", "SEC1", "LEC", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "M");
                meeting3.Guid = meeting1Guid;

                meeting4 = new Domain.Student.Entities.SectionMeeting("789", "SEC1", "LEC", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "Y");
                meeting4.Guid = meeting1Guid;

                var instructionalMethods = new List<Domain.Student.Entities.InstructionalMethod>();
                var instructionalMethod = new Domain.Student.Entities.InstructionalMethod(instructionalMethodGuid, "LEC", "Lecture", false);
                instructionalMethods.Add(instructionalMethod);

                var rooms = new List<Domain.Base.Entities.Room>();
                var room = new Domain.Base.Entities.Room(roomGuid, "DAN*102", "Danville Hall Room 102");
                rooms.Add(room);

                var scheduleRepeats = new List<Domain.Base.Entities.ScheduleRepeat>();
                var scheduleRepeat = new Domain.Base.Entities.ScheduleRepeat("W", "Weekly", "7", FrequencyType.Weekly);
                scheduleRepeats.Add(scheduleRepeat);
                scheduleRepeat = new Domain.Base.Entities.ScheduleRepeat("D", "Daily", "1", FrequencyType.Daily);
                scheduleRepeats.Add(scheduleRepeat);
                scheduleRepeat = new Domain.Base.Entities.ScheduleRepeat("M", "Monthly", "30", FrequencyType.Monthly);
                scheduleRepeats.Add(scheduleRepeat);
                scheduleRepeat = new Domain.Base.Entities.ScheduleRepeat("Y", "Yearly", "365", FrequencyType.Yearly);
                scheduleRepeats.Add(scheduleRepeat);

                List<string> sec1List = new List<string>() { "SEC1" };
                List<Domain.Student.Entities.Section> sec1Resp = new List<Domain.Student.Entities.Section>() { section1 };
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(sec1List, false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sec1Resp));

                List<string> sec2List = new List<string>() { "SEC2" };
                List<Domain.Student.Entities.Section> sec2Resp = new List<Domain.Student.Entities.Section>() { section2 };
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(sec2List, false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sec2Resp));

                List<string> sec3List = new List<string>() { "SEC3" };
                List<Domain.Student.Entities.Section> sec3Resp = new List<Domain.Student.Entities.Section>() { section3 };
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(sec3List, false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sec3Resp));

                List<string> sec4List = new List<string>() { "9999999" };
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(sec4List, false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(new List<Domain.Student.Entities.Section>()));

                sectionRepoMock.Setup(repo => repo.GetSectionMeetingByGuidAsync(meeting1.Guid)).ReturnsAsync(meeting1);
                sectionRepoMock.Setup(repo => repo.GetSectionGuidFromIdAsync(meeting1.SectionId)).ReturnsAsync(meeting1.Guid);

                stuRefDataRepoMock.Setup(repo => repo.GetInstructionalMethodsAsync(It.IsAny<bool>())).ReturnsAsync(instructionalMethods);
                roomRepoMock.Setup(repo => repo.RoomsAsync()).ReturnsAsync(rooms);
                referenceDataRepoMock.Setup(repo => repo.ScheduleRepeats).Returns(scheduleRepeats);

                foreach (var i in instructionalMethods)
                {
                    stuRefDataRepoMock.Setup(repo => repo.GetInstructionalMethodGuidAsync(i.Code)).ReturnsAsync(i.Guid);

                }
                foreach (var r in rooms)
                {
                    roomRepoMock.Setup(repo => repo.GetRoomsGuidAsync(r.Id)).ReturnsAsync(r.Guid);
                }

                Dictionary<string, string> sectDict = new Dictionary<string, string>();
                sectDict.Add(meeting1.SectionId, Guid.NewGuid().ToString());
                sectionRepoMock.Setup(sr => sr.GetSectionGuidsCollectionAsync(It.IsAny<string[]>())).ReturnsAsync(sectDict);
                sectionRepoMock.Setup(sr => sr.GetSectionGuidsCollectionAsync(It.IsAny<List<string>>())).ReturnsAsync(sectDict);


                //foreach (var acadPeriodsEntity in acadPeriodsEntities)
                //{
                //    termRepoMock.Setup(repo => repo.GetAcademicPeriodsCodeFromGuidAsync(acadPeriodsEntity.Guid)).ReturnsAsync(acadPeriodsEntity.Code);
                //}
                // Mock the section service
                sectionCoordinationService = new SectionCoordinationService(adapterRegistry, sectionRepo, courseRepo, studentRepo, studentReferenceDataRepo,
                    referenceDataRepo, termRepo, studentConfigRepo, configRepo, personRepo, roomRepo, eventRepo, bookRepo, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionRepo = null;
                studentRepo = null;
                roleRepo = null;
                adapterRegistry = null;
                sectionCoordinationService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionService_MeetingEmptyId_V6()
            {
                var r = await sectionCoordinationService.GetInstructionalEvent2Async("");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task SectionService_MeetingInvalidSectionId_V6()
            {
                var r = await sectionCoordinationService.GetInstructionalEvent2Async("9999999");
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_GetInstructionalEvent4Async_EmptyArgument()
            {
                var r = await sectionCoordinationService.GetInstructionalEvent4Async(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_GetInstructionalEvent4Async_InvalidGuid()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionMeetingByGuidAsync(It.IsAny<string>())).ReturnsAsync(() => null);
                var r = await sectionCoordinationService.GetInstructionalEvent4Async("9999999");
            }

            [TestMethod]
            public async Task SectionService_GetInstructionalEvent4Async()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionMeetingByGuidAsync(It.IsAny<string>())).ReturnsAsync(meeting1);
                var result = await sectionCoordinationService.GetInstructionalEvent4Async(meeting1Guid);
                Assert.IsNotNull(result);
            }

            [TestMethod]
            public async Task SectionService_GetInstructionalEvent4Async_GetALL_Filters()
            {
                var startDate = DateTime.Now;
                var endDate = DateTime.Now.AddDays(30);
                List<Domain.Student.Entities.SectionMeeting> meetings = new List<Domain.Student.Entities.SectionMeeting>() { meeting1 };

                sectionRepoMock.Setup(repo => repo.GetUnidataFormattedDate(It.IsAny<string>())).ReturnsAsync(startDate.ToString());
                sectionRepoMock.Setup(repo => repo.GetUnidataFormattedDate(It.IsAny<string>())).ReturnsAsync(endDate.ToString());
                sectionRepoMock.Setup(repo => repo.GetSectionMeeting2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>()))
                    .ReturnsAsync(new Tuple<IEnumerable<Domain.Student.Entities.SectionMeeting>, int>(meetings, 1));

                var result = await sectionCoordinationService.GetInstructionalEvent4Async(0, 1, It.IsAny<string>(), startDate.ToString(), endDate.ToString(), It.IsAny<string>());
                Assert.IsNotNull(result);
            }

            [TestMethod]
            public async Task SectionService_GetByGuid_Weekly_V6()
            {
                var res = await sectionCoordinationService.GetInstructionalEvent2Async(meeting1.Guid);
                Assert.AreEqual(meeting1.Guid, res.Id);
                var classroom = res.Locations.ElementAt(0).Locations as Dtos.InstructionalRoom2;
                Assert.AreEqual(roomGuid, classroom.Room.Id);
                Assert.AreEqual(instructionalMethodGuid, res.InstructionalMethod.Id);
            }

            [TestMethod]
            public async Task SectionService_GetByGuid_Daily_V6()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionMeetingByGuidAsync(meeting2.Guid)).ReturnsAsync(meeting2);
                var res = await sectionCoordinationService.GetInstructionalEvent2Async(meeting2.Guid);
                Assert.AreEqual(meeting2.Guid, res.Id);
            }

            [TestMethod]
            public async Task SectionService_GetByGuid_Monthly_V6()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionMeetingByGuidAsync(meeting3.Guid)).ReturnsAsync(meeting3);
                var res = await sectionCoordinationService.GetInstructionalEvent2Async(meeting3.Guid);
                Assert.AreEqual(meeting2.Guid, res.Id);
            }

            [TestMethod]
            public async Task SectionService_GetByGuid_Yearly_V6()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionMeetingByGuidAsync(meeting4.Guid)).ReturnsAsync(meeting4);
                var res = await sectionCoordinationService.GetInstructionalEvent2Async(meeting4.Guid);
                Assert.AreEqual(meeting2.Guid, res.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SectionService_GetByGuid_Invalid_Freq_V6()
            {
                meeting4 = new Domain.Student.Entities.SectionMeeting("789", "SEC1", "LEC", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "X");
                meeting4.Guid = Guid.NewGuid().ToString();
                sectionRepoMock.Setup(repo => repo.GetSectionMeetingByGuidAsync(meeting4.Guid)).ReturnsAsync(meeting4);
                var res = await sectionCoordinationService.GetInstructionalEvent2Async(meeting4.Guid);
            }

            //[TestMethod]
            //[ExpectedException(typeof(PermissionsException))]
            //public async Task SectionService_RoomBookingPermissionsException_V6()
            //{
            //    var instructionalEvent = new Dtos.InstructionalEvent2();
            //    var room = new Dtos.InstructionalRoom2() { Room = new Dtos.GuidObject2() { Id = Guid.NewGuid().ToString() }, LocationType = Dtos.InstructionalLocationType.InstructionalRoom };
            //    var location = new Dtos.Location() { Locations = room };
            //    instructionalEvent.Locations = new List<Dtos.Location>();
            //    instructionalEvent.Locations.Add(location);
            //    var r = await sectionCoordinationService.CreateInstructionalEvent2Async(instructionalEvent);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(PermissionsException))]
            //public async Task SectionService_FacultyBookingPermissionsException_V6()
            //{
            //    var instructionalEvent = new Dtos.InstructionalEvent2();
            //    var faculty = new Dtos.InstructionalEventInstructor2() { Instructor = new Dtos.GuidObject2() { Id = Guid.NewGuid().ToString() } };
            //    instructionalEvent.Instructors = new List<Dtos.InstructionalEventInstructor2>();
            //    instructionalEvent.Instructors.Add(faculty);
            //    var r = await sectionCoordinationService.CreateInstructionalEvent2Async(instructionalEvent);
            //}
        }

        [TestClass]
        public class InstructionalEvents_Put : CurrentUserSetup
        {
            private SectionCoordinationService sectionCoordinationService;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IStudentReferenceDataRepository> stuRefDataRepoMock;
            private IStudentReferenceDataRepository studentReferenceDataRepo;
            private Mock<IReferenceDataRepository> referenceDataRepoMock;
            private IReferenceDataRepository referenceDataRepo;
            private Mock<ICourseRepository> courseRepoMock;
            private ICourseRepository courseRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IStudentConfigurationRepository> studentConfigRepoMock;
            private IStudentConfigurationRepository studentConfigRepo;
            private Mock<IConfigurationRepository> configRepoMock;
            private IConfigurationRepository configRepo;
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IRoomRepository> roomRepoMock;
            private IRoomRepository roomRepo;
            private Mock<IEventRepository> eventRepoMock;
            private IEventRepository eventRepo;
            private Mock<IBookRepository> bookRepoMock;
            private IBookRepository bookRepo;
            // private Mock<IAcademicPeriodRepository> academicPeriodRepoMock;

            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ILogger logger;
            private ICurrentUserFactory currentUserFactory;

            private Domain.Student.Entities.Section section1;
            private Domain.Student.Entities.Section section2;
            private Domain.Student.Entities.Section section3;
            private Domain.Student.Entities.SectionMeeting meeting1;
            private Domain.Student.Entities.SectionMeeting meeting2;
            private Domain.Student.Entities.SectionMeeting meeting3;
            private Domain.Student.Entities.SectionMeeting meeting4;

            private List<OfferingDepartment> dpts = new List<OfferingDepartment>() { new OfferingDepartment("ART", 100m) };
            private List<string> levels = new List<string>() { "Whatever" };
            private List<SectionStatusItem> statuses = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-60)) };

            private string meeting1Guid = Guid.NewGuid().ToString();
            private string instructionalMethodGuid = Guid.NewGuid().ToString();
            private string roomGuid = Guid.NewGuid().ToString();

            [TestInitialize]
            public void Initialize()
            {
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                roomRepoMock = new Mock<IRoomRepository>();
                roomRepo = roomRepoMock.Object;
                stuRefDataRepoMock = new Mock<IStudentReferenceDataRepository>();
                studentReferenceDataRepo = stuRefDataRepoMock.Object;
                referenceDataRepoMock = new Mock<IReferenceDataRepository>();
                referenceDataRepo = referenceDataRepoMock.Object;
                courseRepoMock = new Mock<ICourseRepository>();
                courseRepo = courseRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                studentConfigRepoMock = new Mock<IStudentConfigurationRepository>();
                studentConfigRepo = studentConfigRepoMock.Object;
                configRepoMock = new Mock<IConfigurationRepository>();
                configRepo = configRepoMock.Object;
                eventRepoMock = new Mock<IEventRepository>();
                eventRepo = eventRepoMock.Object;
                bookRepoMock = new Mock<IBookRepository>();
                bookRepo = bookRepoMock.Object;


                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;

                // Set Current User
                currentUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();

                logger = new Mock<ILogger>().Object;

                // Mock the section repo response
                section1 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC1", "1119", "01", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses, true);
                section1.TermId = "2012/FA";
                section1.EndDate = new DateTime(2012, 12, 21);
                section1.Guid = Guid.NewGuid().ToString();

                section2 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC2", "1119", "02", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses, true);
                section2.TermId = "2012/FA";
                section2.Guid = Guid.NewGuid().ToString();

                section3 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC3", "1119", "03", new DateTime(2011, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses, true);
                section3.TermId = "2011/FA";
                section3.EndDate = new DateTime(2011, 12, 21);
                section3.Guid = Guid.NewGuid().ToString();

                var sectionFaculty = new Domain.Student.Entities.SectionFaculty("1234", "SEC1", "0000678", "LEC", new DateTime(2012, 09, 01), new DateTime(2011, 12, 21), 100.0m);

                meeting1 = new Domain.Student.Entities.SectionMeeting("123", "SEC1", "LEC", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "D");
                meeting1.Guid = Guid.NewGuid().ToString();
                meeting1.AddFacultyId("0000678");
                meeting1.AddSectionFaculty(sectionFaculty);
                meeting1.StartTime = new DateTimeOffset(2012, 01, 01, 08, 30, 00, new TimeSpan());
                meeting1.EndTime = new DateTimeOffset(2012, 12, 21, 09, 30, 00, new TimeSpan());
                meeting1.Room = "DAN*102";
                section1.AddSectionMeeting(meeting1);

                meeting2 = new Domain.Student.Entities.SectionMeeting("456", "SEC2", "LEC", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "W");
                meeting2.StartTime = new DateTimeOffset(2012, 01, 01, 08, 30, 00, new TimeSpan());
                meeting2.EndTime = new DateTimeOffset(2012, 12, 21, 09, 30, 00, new TimeSpan());
                meeting2.Guid = Guid.NewGuid().ToString();
                section2.AddSectionMeeting(meeting2);

                meeting3 = new Domain.Student.Entities.SectionMeeting("789", "SEC3", "LEC", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "M");
                meeting3.StartTime = new DateTimeOffset(2012, 01, 01, 08, 30, 00, new TimeSpan());
                meeting3.EndTime = new DateTimeOffset(2012, 12, 21, 09, 30, 00, new TimeSpan());
                meeting3.Guid = Guid.NewGuid().ToString();
                section3.AddSectionMeeting(meeting3);

                meeting4 = new Domain.Student.Entities.SectionMeeting("012", "SEC1", "LEC", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "Y");
                meeting4.StartTime = new DateTimeOffset(2012, 01, 01, 08, 30, 00, new TimeSpan());
                meeting4.EndTime = new DateTimeOffset(2012, 12, 21, 09, 30, 00, new TimeSpan());
                meeting4.Guid = Guid.NewGuid().ToString();
                section1.AddSectionMeeting(meeting4);

                var instructionalMethods = new List<Domain.Student.Entities.InstructionalMethod>();
                var instructionalMethod = new Domain.Student.Entities.InstructionalMethod(instructionalMethodGuid, "LEC", "Lecture", false);
                instructionalMethods.Add(instructionalMethod);

                var rooms = new List<Domain.Base.Entities.Room>();
                var room = new Domain.Base.Entities.Room(roomGuid, "DAN*102", "Danville Hall Room 102");
                rooms.Add(room);

                var scheduleRepeats = new List<Domain.Base.Entities.ScheduleRepeat>();
                var scheduleRepeat = new Domain.Base.Entities.ScheduleRepeat("W", "Weekly", "7", FrequencyType.Weekly);
                scheduleRepeats.Add(scheduleRepeat);
                scheduleRepeat = new Domain.Base.Entities.ScheduleRepeat("D", "Daily", "1", FrequencyType.Daily);
                scheduleRepeats.Add(scheduleRepeat);
                scheduleRepeat = new Domain.Base.Entities.ScheduleRepeat("M", "Monthly", "30", FrequencyType.Monthly);
                scheduleRepeats.Add(scheduleRepeat);
                scheduleRepeat = new Domain.Base.Entities.ScheduleRepeat("Y", "Yearly", "365", FrequencyType.Yearly);
                scheduleRepeats.Add(scheduleRepeat);

                sectionRepoMock.Setup(repo => repo.GetSectionByGuidAsync(section1.Guid)).ReturnsAsync(section1);
                sectionRepoMock.Setup(repo => repo.GetSectionByGuidAsync(section2.Guid)).ReturnsAsync(section2);
                sectionRepoMock.Setup(repo => repo.GetSectionByGuidAsync(section3.Guid)).ReturnsAsync(section3);
                sectionRepoMock.Setup(repo => repo.PutSectionMeetingAsync(It.IsAny<Domain.Student.Entities.Section>(), meeting1.Guid)).ReturnsAsync(meeting1);
                sectionRepoMock.Setup(repo => repo.PutSectionMeetingAsync(It.IsAny<Domain.Student.Entities.Section>(), meeting2.Guid)).ReturnsAsync(meeting2);
                sectionRepoMock.Setup(repo => repo.PutSectionMeetingAsync(It.IsAny<Domain.Student.Entities.Section>(), meeting3.Guid)).ReturnsAsync(meeting3);
                sectionRepoMock.Setup(repo => repo.PutSectionMeetingAsync(It.IsAny<Domain.Student.Entities.Section>(), meeting4.Guid)).ReturnsAsync(meeting4);

                sectionRepoMock.Setup(repo => repo.GetSectionMeetingByGuidAsync(meeting1.Guid)).ReturnsAsync(meeting1);
                sectionRepoMock.Setup(repo => repo.GetSectionMeetingIdFromGuidAsync(meeting1.Guid)).ReturnsAsync(meeting1.Id);
                sectionRepoMock.Setup(repo => repo.GetSectionMeetingByGuidAsync(meeting2.Guid)).ReturnsAsync(meeting2);
                sectionRepoMock.Setup(repo => repo.GetSectionMeetingIdFromGuidAsync(meeting2.Guid)).ReturnsAsync(meeting2.Id);
                sectionRepoMock.Setup(repo => repo.GetSectionMeetingByGuidAsync(meeting3.Guid)).ReturnsAsync(meeting3);
                sectionRepoMock.Setup(repo => repo.GetSectionMeetingIdFromGuidAsync(meeting3.Guid)).ReturnsAsync(meeting3.Id);
                sectionRepoMock.Setup(repo => repo.GetSectionMeetingByGuidAsync(meeting4.Guid)).ReturnsAsync(meeting4);
                sectionRepoMock.Setup(repo => repo.GetSectionMeetingIdFromGuidAsync(meeting4.Guid)).ReturnsAsync(meeting4.Id);
                stuRefDataRepoMock.Setup(repo => repo.GetInstructionalMethodsAsync(It.IsAny<bool>())).ReturnsAsync(instructionalMethods);
                roomRepoMock.Setup(repo => repo.RoomsAsync()).ReturnsAsync(rooms);
                referenceDataRepoMock.Setup(repo => repo.ScheduleRepeats).Returns(scheduleRepeats);

                // Configuration defaults and campus calendar information.
                configRepoMock.Setup(repo => repo.GetDefaultsConfiguration()).Returns(new DefaultsConfiguration() { CampusCalendarId = "MAIN" });
                studentConfigRepoMock.Setup(repo => repo.GetCurriculumConfigurationAsync()).ReturnsAsync(new CurriculumConfiguration() { SectionActiveStatusCode = "A", SectionInactiveStatusCode = "I", DefaultInstructionalMethodCode = "INT" });
                eventRepoMock.Setup(repo => repo.GetCalendar("MAIN")).Returns(new CampusCalendar("MAIN", "Default Calendar", new TimeSpan(), new TimeSpan()));

                // Mock the section service
                sectionCoordinationService = new SectionCoordinationService(adapterRegistry, sectionRepo, courseRepo, studentRepo, studentReferenceDataRepo,
                    referenceDataRepo, termRepo, studentConfigRepo, configRepo, personRepo, roomRepo, eventRepo, bookRepo, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionRepo = null;
                studentRepo = null;
                roleRepo = null;
                adapterRegistry = null;
                sectionCoordinationService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_Null_Object_V6()
            {
                var r = await sectionCoordinationService.UpdateInstructionalEvent2Async(new Dtos.InstructionalEvent2());
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_InvalidId_V6()
            {
                var instrEvent = new Dtos.InstructionalEvent2() { Id = "999999999999999" };
                var r = await sectionCoordinationService.UpdateInstructionalEvent2Async(instrEvent);
            }


            [TestMethod]
            public async Task SectionService_Daily_V6()
            {
                var timePeriod = new Dtos.RepeatTimePeriod2
                {
                    StartOn = new DateTime(2012, 1, 1),
                    EndOn = new DateTime(2012, 12, 21)
                };
                var repeatRuleEnds = new Dtos.RepeatRuleEnds { Date = new DateTime(2012, 12, 21) };
                var repeatRule = new Dtos.RepeatRuleDaily { Type = Dtos.FrequencyType2.Daily, Ends = repeatRuleEnds, Interval = 1 };
                var instrEvent = new Dtos.InstructionalEvent2()
                {
                    Id = meeting1.Guid,
                    Section = new Dtos.GuidObject2() { Id = section1.Guid },
                    InstructionalMethod = new Dtos.GuidObject2() { Id = instructionalMethodGuid },
                    Workload = 10.5m,
                    Recurrence = new Dtos.Recurrence3() { RepeatRule = repeatRule, TimePeriod = timePeriod }
                };
                var r = await sectionCoordinationService.UpdateInstructionalEvent2Async(instrEvent);
                Assert.AreEqual(r.Id, meeting1.Guid);
                Assert.AreEqual(r.InstructionalMethod.Id, instructionalMethodGuid);
                Assert.AreEqual(r.Recurrence.RepeatRule.Type.ToString(), FrequencyType.Daily.ToString());
                Assert.AreEqual(r.Recurrence.TimePeriod.StartOn.Value.Date, timePeriod.StartOn.Value.Date);
                Assert.AreEqual(r.Recurrence.TimePeriod.EndOn.Value.Date, timePeriod.EndOn.Value.Date);
                Assert.AreEqual(r.Recurrence.TimePeriod.StartOn.Value.ToLocalTime().TimeOfDay.ToString(), "08:30:00");
                Assert.AreEqual(r.Recurrence.TimePeriod.EndOn.Value.ToLocalTime().TimeOfDay.ToString(), "09:30:00");
            }

            [TestMethod]
            public async Task SectionService_Weekly_V6()
            {
                var timePeriod = new Dtos.RepeatTimePeriod2
                {
                    StartOn = new DateTime(2012, 1, 1, 8, 30, 0),
                    EndOn = new DateTime(2012, 12, 21, 9, 30, 0)
                };
                var repeatRuleEnds = new Dtos.RepeatRuleEnds { Date = new DateTime(2012, 12, 21) };
                var repeatRule = new Dtos.RepeatRuleWeekly { Ends = new Dtos.RepeatRuleEnds() { Date = new DateTime(2012, 12, 21) }, DayOfWeek = new List<HedmDayOfWeek?> { HedmDayOfWeek.Monday } };
                var instrEvent = new Dtos.InstructionalEvent2()
                {
                    Id = meeting2.Guid,
                    Section = new Dtos.GuidObject2() { Id = section2.Guid },
                    InstructionalMethod = new Dtos.GuidObject2() { Id = instructionalMethodGuid },
                    Workload = 10.5m,
                    Recurrence = new Dtos.Recurrence3() { RepeatRule = repeatRule, TimePeriod = timePeriod }
                };
                var r = await sectionCoordinationService.UpdateInstructionalEvent2Async(instrEvent);
                Assert.AreEqual(r.Id, meeting2.Guid);
                Assert.AreEqual(r.InstructionalMethod.Id, instructionalMethodGuid);
                Assert.AreEqual(r.Recurrence.RepeatRule.Type.ToString(), FrequencyType.Weekly.ToString());
                Assert.AreEqual(r.Recurrence.TimePeriod.StartOn.Value.Date, timePeriod.StartOn.Value.Date);
                Assert.AreEqual(r.Recurrence.TimePeriod.EndOn.Value.Date, timePeriod.EndOn.Value.Date);
                Assert.AreEqual(r.Recurrence.TimePeriod.StartOn.Value.ToLocalTime().TimeOfDay.ToString(), "08:30:00");
                Assert.AreEqual(r.Recurrence.TimePeriod.EndOn.Value.ToLocalTime().TimeOfDay.ToString(), "09:30:00");
            }


            [TestMethod]
            public async Task SectionService_Monthly_V6()
            {
                var timePeriod = new Dtos.RepeatTimePeriod2
                {
                    StartOn = new DateTime(2012, 1, 1, 8, 30, 0),
                    EndOn = new DateTime(2012, 12, 21, 9, 30, 0)
                };
                var repeatRuleEnds = new Dtos.RepeatRuleEnds { Date = new DateTime(2012, 12, 21) };
                var repeatRule = new Dtos.RepeatRuleMonthly { RepeatBy = new Dtos.RepeatRuleRepeatBy() { DayOfMonth = 30 }, Ends = new Dtos.RepeatRuleEnds() { Date = new DateTime(2012, 12, 21) } };
                var instrEvent = new Dtos.InstructionalEvent2()
                {
                    Id = meeting3.Guid,
                    Section = new Dtos.GuidObject2() { Id = section3.Guid },
                    InstructionalMethod = new Dtos.GuidObject2() { Id = instructionalMethodGuid },
                    Workload = 10.5m,
                    Recurrence = new Dtos.Recurrence3() { RepeatRule = repeatRule, TimePeriod = timePeriod }
                };
                var r = await sectionCoordinationService.UpdateInstructionalEvent2Async(instrEvent);
                Assert.AreEqual(r.Id, meeting3.Guid);
                Assert.AreEqual(r.InstructionalMethod.Id, instructionalMethodGuid);
                Assert.AreEqual(r.Recurrence.RepeatRule.Type.ToString(), FrequencyType.Monthly.ToString());
                Assert.AreEqual(r.Recurrence.TimePeriod.StartOn.Value.Date, timePeriod.StartOn.Value.Date);
                Assert.AreEqual(r.Recurrence.TimePeriod.EndOn.Value.Date, timePeriod.EndOn.Value.Date);
                Assert.AreEqual(r.Recurrence.TimePeriod.StartOn.Value.ToLocalTime().TimeOfDay.ToString(), "08:30:00");
                Assert.AreEqual(r.Recurrence.TimePeriod.EndOn.Value.ToLocalTime().TimeOfDay.ToString(), "09:30:00");
            }

            [TestMethod]
            public async Task SectionService_Yearly_V6()
            {
                var timePeriod = new Dtos.RepeatTimePeriod2
                {
                    StartOn = new DateTime(2012, 1, 1, 8, 30, 0),
                    EndOn = new DateTime(2012, 12, 21, 9, 30, 0)
                };
                var repeatRuleEnds = new Dtos.RepeatRuleEnds { Date = new DateTime(2012, 12, 21) };
                var repeatRule = new Dtos.RepeatRuleYearly { Ends = new Dtos.RepeatRuleEnds() { Date = new DateTime(2012, 12, 21) } };
                var instrEvent = new Dtos.InstructionalEvent2()
                {
                    Id = meeting4.Guid,
                    Section = new Dtos.GuidObject2() { Id = section3.Guid },
                    InstructionalMethod = new Dtos.GuidObject2() { Id = instructionalMethodGuid },
                    Workload = 10.5m,
                    Recurrence = new Dtos.Recurrence3() { RepeatRule = repeatRule, TimePeriod = timePeriod }
                };
                var r = await sectionCoordinationService.UpdateInstructionalEvent2Async(instrEvent);
                Assert.AreEqual(r.Id, meeting4.Guid);
                Assert.AreEqual(r.InstructionalMethod.Id, instructionalMethodGuid);
                Assert.AreEqual(r.Recurrence.RepeatRule.Type.ToString(), FrequencyType.Yearly.ToString());
                Assert.AreEqual(r.Recurrence.TimePeriod.StartOn.Value.Date, timePeriod.StartOn.Value.Date);
                Assert.AreEqual(r.Recurrence.TimePeriod.EndOn.Value.Date, timePeriod.EndOn.Value.Date);
                Assert.AreEqual(r.Recurrence.TimePeriod.StartOn.Value.ToLocalTime().TimeOfDay.ToString(), "08:30:00");
                Assert.AreEqual(r.Recurrence.TimePeriod.EndOn.Value.ToLocalTime().TimeOfDay.ToString(), "09:30:00");
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_Null_Object_V8()
            {
                var r = await sectionCoordinationService.UpdateInstructionalEvent3Async(new Dtos.InstructionalEvent3());
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_InvalidId_V8()
            {
                var instrEvent = new Dtos.InstructionalEvent3() { Id = "999999999999999" };
                var r = await sectionCoordinationService.UpdateInstructionalEvent3Async(instrEvent);
            }


            [TestMethod]
            public async Task SectionService_Daily_V8()
            {
                var timePeriod = new Dtos.RepeatTimePeriod2
                {
                    StartOn = new DateTime(2012, 1, 1),
                    EndOn = new DateTime(2012, 12, 21)
                };
                var repeatRuleEnds = new Dtos.RepeatRuleEnds { Date = new DateTime(2012, 12, 21) };
                var repeatRule = new Dtos.RepeatRuleDaily { Type = Dtos.FrequencyType2.Daily, Ends = repeatRuleEnds, Interval = 1 };
                var instrEvent = new Dtos.InstructionalEvent3()
                {
                    Id = meeting1.Guid,
                    Section = new Dtos.GuidObject2() { Id = section1.Guid },
                    InstructionalMethod = new Dtos.GuidObject2() { Id = instructionalMethodGuid },
                    Workload = 10.5m,
                    Recurrence = new Dtos.Recurrence3() { RepeatRule = repeatRule, TimePeriod = timePeriod }
                };
                var r = await sectionCoordinationService.UpdateInstructionalEvent3Async(instrEvent);
                Assert.AreEqual(r.Id, meeting1.Guid);
                Assert.AreEqual(r.InstructionalMethod.Id, instructionalMethodGuid);
                Assert.AreEqual(r.Recurrence.RepeatRule.Type.ToString(), FrequencyType.Daily.ToString());
                Assert.AreEqual(r.Recurrence.TimePeriod.StartOn.Value.Date, timePeriod.StartOn.Value.Date);
                Assert.AreEqual(r.Recurrence.TimePeriod.EndOn.Value.Date, timePeriod.EndOn.Value.Date);
                Assert.AreEqual(r.Recurrence.TimePeriod.StartOn.Value.ToLocalTime().TimeOfDay.ToString(), "08:30:00");
                Assert.AreEqual(r.Recurrence.TimePeriod.EndOn.Value.ToLocalTime().TimeOfDay.ToString(), "09:30:00");
            }

            [TestMethod]
            public async Task SectionService_Weekly_V8()
            {
                var timePeriod = new Dtos.RepeatTimePeriod2
                {
                    StartOn = new DateTime(2012, 1, 1, 8, 30, 0),
                    EndOn = new DateTime(2012, 12, 21, 9, 30, 0)
                };
                var repeatRuleEnds = new Dtos.RepeatRuleEnds { Date = new DateTime(2012, 12, 21) };
                var repeatRule = new Dtos.RepeatRuleWeekly { Ends = new Dtos.RepeatRuleEnds() { Date = new DateTime(2012, 12, 21) }, DayOfWeek = new List<HedmDayOfWeek?> { HedmDayOfWeek.Monday } };
                var instrEvent = new Dtos.InstructionalEvent3()
                {
                    Id = meeting2.Guid,
                    Section = new Dtos.GuidObject2() { Id = section2.Guid },
                    InstructionalMethod = new Dtos.GuidObject2() { Id = instructionalMethodGuid },
                    Workload = 10.5m,
                    Recurrence = new Dtos.Recurrence3() { RepeatRule = repeatRule, TimePeriod = timePeriod }
                };
                var r = await sectionCoordinationService.UpdateInstructionalEvent3Async(instrEvent);
                Assert.AreEqual(r.Id, meeting2.Guid);
                Assert.AreEqual(r.InstructionalMethod.Id, instructionalMethodGuid);
                Assert.AreEqual(r.Recurrence.RepeatRule.Type.ToString(), FrequencyType.Weekly.ToString());
                Assert.AreEqual(r.Recurrence.TimePeriod.StartOn.Value.Date, timePeriod.StartOn.Value.Date);
                Assert.AreEqual(r.Recurrence.TimePeriod.EndOn.Value.Date, timePeriod.EndOn.Value.Date);
                Assert.AreEqual(r.Recurrence.TimePeriod.StartOn.Value.ToLocalTime().TimeOfDay.ToString(), "08:30:00");
                Assert.AreEqual(r.Recurrence.TimePeriod.EndOn.Value.ToLocalTime().TimeOfDay.ToString(), "09:30:00");
            }


            [TestMethod]
            public async Task SectionService_Monthly_V8()
            {
                var timePeriod = new Dtos.RepeatTimePeriod2
                {
                    StartOn = new DateTime(2012, 1, 1, 8, 30, 0),
                    EndOn = new DateTime(2012, 12, 21, 9, 30, 0)
                };
                var repeatRuleEnds = new Dtos.RepeatRuleEnds { Date = new DateTime(2012, 12, 21) };
                var repeatRule = new Dtos.RepeatRuleMonthly { RepeatBy = new Dtos.RepeatRuleRepeatBy() { DayOfMonth = 30 }, Ends = new Dtos.RepeatRuleEnds() { Date = new DateTime(2012, 12, 21) } };
                var instrEvent = new Dtos.InstructionalEvent3()
                {
                    Id = meeting3.Guid,
                    Section = new Dtos.GuidObject2() { Id = section3.Guid },
                    InstructionalMethod = new Dtos.GuidObject2() { Id = instructionalMethodGuid },
                    Workload = 10.5m,
                    Recurrence = new Dtos.Recurrence3() { RepeatRule = repeatRule, TimePeriod = timePeriod }
                };
                var r = await sectionCoordinationService.UpdateInstructionalEvent3Async(instrEvent);
                Assert.AreEqual(r.Id, meeting3.Guid);
                Assert.AreEqual(r.InstructionalMethod.Id, instructionalMethodGuid);
                Assert.AreEqual(r.Recurrence.RepeatRule.Type.ToString(), FrequencyType.Monthly.ToString());
                Assert.AreEqual(r.Recurrence.TimePeriod.StartOn.Value.Date, timePeriod.StartOn.Value.Date);
                Assert.AreEqual(r.Recurrence.TimePeriod.EndOn.Value.Date, timePeriod.EndOn.Value.Date);
                Assert.AreEqual(r.Recurrence.TimePeriod.StartOn.Value.ToLocalTime().TimeOfDay.ToString(), "08:30:00");
                Assert.AreEqual(r.Recurrence.TimePeriod.EndOn.Value.ToLocalTime().TimeOfDay.ToString(), "09:30:00");
            }

            [TestMethod]
            public async Task SectionService_Yearly_V8()
            {
                var timePeriod = new Dtos.RepeatTimePeriod2
                {
                    StartOn = new DateTime(2012, 1, 1, 8, 30, 0),
                    EndOn = new DateTime(2012, 12, 21, 9, 30, 0)
                };
                var repeatRuleEnds = new Dtos.RepeatRuleEnds { Date = new DateTime(2012, 12, 21) };
                var repeatRule = new Dtos.RepeatRuleYearly { Ends = new Dtos.RepeatRuleEnds() { Date = new DateTime(2012, 12, 21) } };
                var instrEvent = new Dtos.InstructionalEvent3()
                {
                    Id = meeting4.Guid,
                    Section = new Dtos.GuidObject2() { Id = section3.Guid },
                    InstructionalMethod = new Dtos.GuidObject2() { Id = instructionalMethodGuid },
                    Workload = 10.5m,
                    Recurrence = new Dtos.Recurrence3() { RepeatRule = repeatRule, TimePeriod = timePeriod }
                };
                var r = await sectionCoordinationService.UpdateInstructionalEvent3Async(instrEvent);
                Assert.AreEqual(r.Id, meeting4.Guid);
                Assert.AreEqual(r.InstructionalMethod.Id, instructionalMethodGuid);
                Assert.AreEqual(r.Recurrence.RepeatRule.Type.ToString(), FrequencyType.Yearly.ToString());
                Assert.AreEqual(r.Recurrence.TimePeriod.StartOn.Value.Date, timePeriod.StartOn.Value.Date);
                Assert.AreEqual(r.Recurrence.TimePeriod.EndOn.Value.Date, timePeriod.EndOn.Value.Date);
                Assert.AreEqual(r.Recurrence.TimePeriod.StartOn.Value.ToLocalTime().TimeOfDay.ToString(), "08:30:00");
                Assert.AreEqual(r.Recurrence.TimePeriod.EndOn.Value.ToLocalTime().TimeOfDay.ToString(), "09:30:00");
            }
        }

        [TestClass]
        public class InstructionalEvents_Put_Post_V11 : CurrentUserSetup
        {
            private SectionCoordinationService sectionCoordinationService;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IStudentReferenceDataRepository> stuRefDataRepoMock;
            private IStudentReferenceDataRepository studentReferenceDataRepo;
            private Mock<IReferenceDataRepository> referenceDataRepoMock;
            private IReferenceDataRepository referenceDataRepo;
            private Mock<ICourseRepository> courseRepoMock;
            private ICourseRepository courseRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IStudentConfigurationRepository> studentConfigRepoMock;
            private IStudentConfigurationRepository studentConfigRepo;
            private Mock<IConfigurationRepository> configRepoMock;
            private IConfigurationRepository configRepo;
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IRoomRepository> roomRepoMock;
            private IRoomRepository roomRepo;
            private Mock<IEventRepository> eventRepoMock;
            private IEventRepository eventRepo;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private Mock<IBookRepository> bookRepoMock;
            private IBookRepository bookRepo;
            private ILogger logger;
            private ICurrentUserFactory currentUserFactory;

            private Dtos.InstructionalEvent4 instrEvents4;
            private Dtos.InstructionalEvent4 instrEvents4DTO;

            private Domain.Student.Entities.Section section1;
            private Domain.Student.Entities.Section section2;
            private Domain.Student.Entities.Section section3;
            private Domain.Student.Entities.SectionMeeting meeting1;
            private Domain.Student.Entities.SectionMeeting meeting2;
            private Domain.Student.Entities.SectionMeeting meeting3;
            private Domain.Student.Entities.SectionMeeting meeting4;

            private Domain.Entities.Permission permissionUpdateRoomBooking;


            private List<OfferingDepartment> dpts = new List<OfferingDepartment>() { new OfferingDepartment("ART", 100m) };
            private List<string> levels = new List<string>() { "Whatever" };
            private List<SectionStatusItem> statuses = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-60)) };

            private string meeting1Guid = Guid.NewGuid().ToString();
            private string instructionalMethodGuid = "3d84a4e4-bffa-426a-9531-4844b9aefae7";
            private string roomGuid = "a5b017bc-b393-4432-a6f5-7d4822070146";

            [TestInitialize]
            public void Initialize()
            {
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                roomRepoMock = new Mock<IRoomRepository>();
                roomRepo = roomRepoMock.Object;
                bookRepoMock = new Mock<IBookRepository>();
                bookRepo = bookRepoMock.Object;
                stuRefDataRepoMock = new Mock<IStudentReferenceDataRepository>();
                studentReferenceDataRepo = stuRefDataRepoMock.Object;
                referenceDataRepoMock = new Mock<IReferenceDataRepository>();
                referenceDataRepo = referenceDataRepoMock.Object;
                courseRepoMock = new Mock<ICourseRepository>();
                courseRepo = courseRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                studentConfigRepoMock = new Mock<IStudentConfigurationRepository>();
                studentConfigRepo = studentConfigRepoMock.Object;
                configRepoMock = new Mock<IConfigurationRepository>();
                configRepo = configRepoMock.Object;
                eventRepoMock = new Mock<IEventRepository>();
                eventRepo = eventRepoMock.Object;


                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;

                // Set Current User
                currentUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();

                // Mock permissions
                permissionUpdateRoomBooking = new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateAndUpdateRoomBooking);
                createUpdateInstrEvent.AddPermission(permissionUpdateRoomBooking);

                logger = new Mock<ILogger>().Object;

                instrEvents4 = new Dtos.InstructionalEvent4()
                {
                    Id = meeting1Guid,
                    Section = new Dtos.GuidObject2() { Id = "19c84d19-cc57-43a9-93d8-a90597a8bbcd" },
                    InstructionalMethod = new Dtos.GuidObject2() { Id = "3d84a4e4-bffa-426a-9531-4844b9aefae7" }
                };

                instrEvents4DTO = new Dtos.InstructionalEvent4()
                {
                    Id = meeting1Guid,
                    Section = new Dtos.GuidObject2() { Id = "19c84d19-cc57-43a9-93d8-a90597a8bbcd" },
                    InstructionalMethod = new Dtos.GuidObject2() { Id = "3d84a4e4-bffa-426a-9531-4844b9aefae7" },
                    Approvals = new List<Dtos.InstructionalEventApproval2>()
                    {
                        new Dtos.InstructionalEventApproval2() { ApprovingEntity = Dtos.InstructionalEventApprovalEntity2.User, Type = Dtos.InstructionalEventApprovalType2.InstructorAvailability }
                    },
                    Description = "Description",
                    Locations = new List<Dtos.Location>()
                    {
                        new Dtos.Location()
                        {
                            Locations = new Dtos.InstructionalRoom2()
                            {
                                LocationType = Dtos.InstructionalLocationType.InstructionalRoom,
                                Room = new Dtos.GuidObject2(roomGuid)
                            }
                        }
                    },
                    Recurrence = new Dtos.Recurrence3()
                    {
                        TimePeriod = new Dtos.RepeatTimePeriod2()
                        {
                            StartOn = DateTime.Today,
                            EndOn = DateTime.Today.AddDays(60)
                        },
                        RepeatRule = new Dtos.RepeatRuleDaily()
                        {
                            Ends = new Dtos.RepeatRuleEnds()
                            {
                                Date = DateTime.Today.AddDays(60)
                                //Repetitions = 2
                            }
                        }
                    },
                    Title = "Title",
                    Workload = 5m
                };

                // Mock the section repo response
                section1 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC1", "1119", "01", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses, true);
                section1.TermId = "2012/FA";
                section1.EndDate = new DateTime(2012, 12, 21);
                section1.Guid = "19c84d19-cc57-43a9-93d8-a90597a8bbcd";

                section2 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC2", "1119", "02", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses, true);
                section2.TermId = "2012/FA";
                section2.Guid = Guid.NewGuid().ToString();

                section3 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC3", "1119", "03", new DateTime(2011, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses, true);
                section3.TermId = "2011/FA";
                section3.EndDate = new DateTime(2011, 12, 21);
                section3.Guid = Guid.NewGuid().ToString();

                var sectionFaculty = new Domain.Student.Entities.SectionFaculty("1234", "SEC1", "0000678", "LEC", new DateTime(2012, 09, 01), new DateTime(2011, 12, 21), 100.0m);

                meeting1 = new Domain.Student.Entities.SectionMeeting("123", "SEC1", "LEC", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "D");
                meeting1.Guid = "19c84d19-cc57-43a9-93d8-a90597a8bbcd";
                meeting1.AddFacultyId("0000678");
                meeting1.AddSectionFaculty(sectionFaculty);
                meeting1.StartTime = new DateTimeOffset(2012, 01, 01, 08, 30, 00, new TimeSpan());
                meeting1.EndTime = new DateTimeOffset(2012, 12, 21, 09, 30, 00, new TimeSpan());
                meeting1.Room = "DAN*102";
                section1.AddSectionMeeting(meeting1);

                meeting2 = new Domain.Student.Entities.SectionMeeting("456", "SEC2", "LEC", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "W");
                meeting2.StartTime = new DateTimeOffset(2012, 01, 01, 08, 30, 00, new TimeSpan());
                meeting2.EndTime = new DateTimeOffset(2012, 12, 21, 09, 30, 00, new TimeSpan());
                meeting2.Guid = Guid.NewGuid().ToString();
                section2.AddSectionMeeting(meeting2);

                meeting3 = new Domain.Student.Entities.SectionMeeting("789", "SEC3", "LEC", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "M");
                meeting3.StartTime = new DateTimeOffset(2012, 01, 01, 08, 30, 00, new TimeSpan());
                meeting3.EndTime = new DateTimeOffset(2012, 12, 21, 09, 30, 00, new TimeSpan());
                meeting3.Guid = Guid.NewGuid().ToString();
                section3.AddSectionMeeting(meeting3);

                meeting4 = new Domain.Student.Entities.SectionMeeting("012", "SEC1", "LEC", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "Y");
                meeting4.StartTime = new DateTimeOffset(2012, 01, 01, 08, 30, 00, new TimeSpan());
                meeting4.EndTime = new DateTimeOffset(2012, 12, 21, 09, 30, 00, new TimeSpan());
                meeting4.Guid = Guid.NewGuid().ToString();
                section1.AddSectionMeeting(meeting4);

                var instructionalMethods = new List<Domain.Student.Entities.InstructionalMethod>();
                var instructionalMethod = new Domain.Student.Entities.InstructionalMethod("3d84a4e4-bffa-426a-9531-4844b9aefae7", "LEC", "Lecture", false);
                instructionalMethods.Add(instructionalMethod);

                var rooms = new List<Domain.Base.Entities.Room>();
                var room = new Domain.Base.Entities.Room(roomGuid, "DAN*102", "Danville Hall Room 102");
                rooms.Add(room);

                var scheduleRepeats = new List<Domain.Base.Entities.ScheduleRepeat>();
                var scheduleRepeat = new Domain.Base.Entities.ScheduleRepeat("W", "Weekly", "7", FrequencyType.Weekly);
                scheduleRepeats.Add(scheduleRepeat);
                scheduleRepeat = new Domain.Base.Entities.ScheduleRepeat("D", "Daily", "1", FrequencyType.Daily);
                scheduleRepeats.Add(scheduleRepeat);
                scheduleRepeat = new Domain.Base.Entities.ScheduleRepeat("M", "Monthly", "30", FrequencyType.Monthly);
                scheduleRepeats.Add(scheduleRepeat);
                scheduleRepeat = new Domain.Base.Entities.ScheduleRepeat("Y", "Yearly", "365", FrequencyType.Yearly);
                scheduleRepeats.Add(scheduleRepeat);

                sectionRepoMock.Setup(repo => repo.GetSectionByGuidAsync(section1.Guid)).ReturnsAsync(section1);
                sectionRepoMock.Setup(repo => repo.GetSectionByGuidAsync(section2.Guid)).ReturnsAsync(section2);
                sectionRepoMock.Setup(repo => repo.GetSectionByGuidAsync(section3.Guid)).ReturnsAsync(section3);
                sectionRepoMock.Setup(repo => repo.PutSectionMeetingAsync(It.IsAny<Domain.Student.Entities.Section>(), meeting1.Guid)).ReturnsAsync(meeting1);
                sectionRepoMock.Setup(repo => repo.PutSectionMeetingAsync(It.IsAny<Domain.Student.Entities.Section>(), meeting2.Guid)).ReturnsAsync(meeting2);
                sectionRepoMock.Setup(repo => repo.PutSectionMeetingAsync(It.IsAny<Domain.Student.Entities.Section>(), meeting3.Guid)).ReturnsAsync(meeting3);
                sectionRepoMock.Setup(repo => repo.PutSectionMeetingAsync(It.IsAny<Domain.Student.Entities.Section>(), meeting4.Guid)).ReturnsAsync(meeting4);
                sectionRepoMock.Setup(repo => repo.PostSectionMeeting2Async(It.IsAny<Domain.Student.Entities.Section>(), meeting1.Guid)).ReturnsAsync(meeting1);
                sectionRepoMock.Setup(repo => repo.PostSectionMeeting2Async(It.IsAny<Domain.Student.Entities.Section>(), meeting2.Guid)).ReturnsAsync(meeting2);
                sectionRepoMock.Setup(repo => repo.PostSectionMeeting2Async(It.IsAny<Domain.Student.Entities.Section>(), meeting3.Guid)).ReturnsAsync(meeting3);

                sectionRepoMock.Setup(repo => repo.GetSectionMeetingByGuidAsync(meeting1.Guid)).ReturnsAsync(meeting1);
                sectionRepoMock.Setup(repo => repo.GetSectionMeetingIdFromGuidAsync(meeting1.Guid)).ReturnsAsync(meeting1.Id);
                sectionRepoMock.Setup(repo => repo.GetSectionMeetingByGuidAsync(meeting2.Guid)).ReturnsAsync(meeting2);
                sectionRepoMock.Setup(repo => repo.GetSectionMeetingIdFromGuidAsync(meeting2.Guid)).ReturnsAsync(meeting2.Id);
                sectionRepoMock.Setup(repo => repo.GetSectionMeetingByGuidAsync(meeting3.Guid)).ReturnsAsync(meeting3);
                sectionRepoMock.Setup(repo => repo.GetSectionMeetingIdFromGuidAsync(meeting3.Guid)).ReturnsAsync(meeting3.Id);
                sectionRepoMock.Setup(repo => repo.GetSectionMeetingByGuidAsync(meeting4.Guid)).ReturnsAsync(meeting4);
                sectionRepoMock.Setup(repo => repo.GetSectionMeetingIdFromGuidAsync(meeting4.Guid)).ReturnsAsync(meeting4.Id);
                stuRefDataRepoMock.Setup(repo => repo.GetInstructionalMethodsAsync(It.IsAny<bool>())).ReturnsAsync(instructionalMethods);
                roomRepoMock.Setup(repo => repo.RoomsAsync()).ReturnsAsync(rooms);
                referenceDataRepoMock.Setup(repo => repo.ScheduleRepeats).Returns(scheduleRepeats);
                sectionRepoMock.Setup(repo => repo.GetSectionGuidFromIdAsync(meeting1.SectionId)).ReturnsAsync(meeting1.Guid);
                sectionRepoMock.Setup(repo => repo.GetSectionGuidFromIdAsync(meeting2.SectionId)).ReturnsAsync(meeting2.Guid);
                sectionRepoMock.Setup(repo => repo.GetSectionGuidFromIdAsync(meeting3.SectionId)).ReturnsAsync(meeting3.Guid);
                sectionRepoMock.Setup(repo => repo.GetSectionGuidFromIdAsync(meeting4.SectionId)).ReturnsAsync(meeting4.Guid);


                // Configuration defaults and campus calendar information.
                configRepoMock.Setup(repo => repo.GetDefaultsConfiguration()).Returns(new DefaultsConfiguration() { CampusCalendarId = "MAIN" });
                studentConfigRepoMock.Setup(repo => repo.GetCurriculumConfigurationAsync()).ReturnsAsync(new CurriculumConfiguration() { SectionActiveStatusCode = "A", SectionInactiveStatusCode = "I", DefaultInstructionalMethodCode = "INT" });
                eventRepoMock.Setup(repo => repo.GetCalendar("MAIN")).Returns(new CampusCalendar("MAIN", "Default Calendar", new TimeSpan(), new TimeSpan()));
                foreach (var i in instructionalMethods)
                {
                    stuRefDataRepoMock.Setup(repo => repo.GetInstructionalMethodGuidAsync(i.Code)).ReturnsAsync(i.Guid);

                }
                foreach (var r in rooms)
                {
                    roomRepoMock.Setup(repo => repo.GetRoomsGuidAsync(r.Id)).ReturnsAsync(r.Guid);
                }
                //foreach (var acadPeriodsEntity in acadPeriodsEntities)
                //{
                //    termRepoMock.Setup(repo => repo.GetAcademicPeriodsCodeFromGuidAsync(acadPeriodsEntity.Guid)).ReturnsAsync(acadPeriodsEntity.Code);
                //}
                Dictionary<string, string> sectDict = new Dictionary<string, string>();
                sectDict.Add(meeting1.SectionId, Guid.NewGuid().ToString());
                sectionRepoMock.Setup(sr => sr.GetSectionGuidsCollectionAsync(It.IsAny<string[]>())).ReturnsAsync(sectDict);
                sectionRepoMock.Setup(sr => sr.GetSectionGuidsCollectionAsync(It.IsAny<List<string>>())).ReturnsAsync(sectDict);


                // Mock the section service
                sectionCoordinationService = new SectionCoordinationService(adapterRegistry, sectionRepo, courseRepo, studentRepo, studentReferenceDataRepo,
                    referenceDataRepo, termRepo, studentConfigRepo, configRepo, personRepo, roomRepo, eventRepo, bookRepo, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionRepo = null;
                studentRepo = null;
                roleRepo = null;
                adapterRegistry = null;
                sectionCoordinationService = null;
                meeting1 = null;
                meeting2 = null;
                meeting3 = null;
                meeting4 = null;
            }

            [TestMethod]
            public async Task SectionService_UpdateInstructionalEvent4Async_V11()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateInstrEvent });
                sectionRepoMock.Setup(repo => repo.PutSectionMeeting2Async(It.IsAny<Domain.Student.Entities.Section>(), It.IsAny<string>())).ReturnsAsync(meeting1);
                var r = await sectionCoordinationService.UpdateInstructionalEvent4Async(instrEvents4DTO);
                Assert.AreEqual(r.Id, meeting1.Guid);
                Assert.AreEqual(r.InstructionalMethod.Id, instructionalMethodGuid);
            }

            [TestMethod]
            public async Task SectionService_CreateInstructionalEvent4Async_V11()
            {
                instrEvents4DTO.Id = Guid.Empty.ToString();
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateInstrEvent });
                sectionRepoMock.Setup(repo => repo.PostSectionMeeting2Async(It.IsAny<Domain.Student.Entities.Section>(), It.IsAny<string>())).ReturnsAsync(meeting1);
                var r = await sectionCoordinationService.CreateInstructionalEvent4Async(instrEvents4DTO);
                Assert.AreEqual(r.InstructionalMethod.Id, instructionalMethodGuid);
            }

            [TestMethod]
            public async Task SectionService_UpdateInstructionalEvent4Async_DefaultInstrEventCode_V11()
            {
                instrEvents4.InstructionalMethod = null;
                sectionRepoMock.Setup(repo => repo.PutSectionMeeting2Async(It.IsAny<Domain.Student.Entities.Section>(), It.IsAny<string>())).ReturnsAsync(meeting1);
                var r = await sectionCoordinationService.UpdateInstructionalEvent4Async(instrEvents4);
                Assert.AreEqual(r.Id, meeting1.Guid);
                Assert.AreEqual(r.InstructionalMethod.Id, instructionalMethodGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_CreateInstructionalEvent4Async_EmptyArgument()
            {
                var r = await sectionCoordinationService.CreateInstructionalEvent4Async(null);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_UpdateInstructionalEvent4Async_RoomsAsync_IntegrationApiException()
            {
                ((Dtos.InstructionalRoom2)instrEvents4DTO.Locations[0].Locations).Room.Id = "BadGuid";
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateInstrEvent });
                sectionRepoMock.Setup(repo => repo.PutSectionMeeting2Async(It.IsAny<Domain.Student.Entities.Section>(), It.IsAny<string>())).ReturnsAsync(meeting1);
                var r = await sectionCoordinationService.UpdateInstructionalEvent4Async(instrEvents4DTO);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_UpdateInstructionalEvent4Async_DefaultInstrEventCode_IntegrationApiException()
            {
                instrEvents4.InstructionalMethod = null;
                sectionRepoMock.Setup(repo => repo.PutSectionMeeting2Async(It.IsAny<Domain.Student.Entities.Section>(), It.IsAny<string>())).ReturnsAsync(meeting1);
                studentConfigRepoMock.Setup(repo => repo.GetCurriculumConfigurationAsync()).ReturnsAsync(new CurriculumConfiguration()
                { SectionActiveStatusCode = "A", SectionInactiveStatusCode = "I", DefaultInstructionalMethodCode = null });
                var r = await sectionCoordinationService.UpdateInstructionalEvent4Async(instrEvents4);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_UpdateInstructionalEvent4Async_IntegrationApiException()
            {
                var r = await sectionCoordinationService.UpdateInstructionalEvent4Async(new Dtos.InstructionalEvent4());
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_UpdateInstructionalEvent4Async_EmptyArgumentException()
            {
                var r = await sectionCoordinationService.UpdateInstructionalEvent4Async(null);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_UpdateInstructionalEvent4Async_IntegrationException()
            {
                sectionRepoMock.Setup(repo => repo.PutSectionMeeting2Async(It.IsAny<Domain.Student.Entities.Section>(), It.IsAny<string>())).ReturnsAsync(meeting1);
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(() => null);
                var r = await sectionCoordinationService.UpdateInstructionalEvent4Async(new Dtos.InstructionalEvent4()
                {
                    Id = meeting1Guid,
                    Section = new Dtos.GuidObject2() { Id = section1.Id }
                });
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_Null_Object_V11()
            {
                var r = await sectionCoordinationService.CreateInstructionalEvent4Async(new Dtos.InstructionalEvent4());
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_Null_Section_V11()
            {
                var r = await sectionCoordinationService.CreateInstructionalEvent4Async(new Dtos.InstructionalEvent4()
                {
                    Id = meeting1.Guid,
                });
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_Empty_Section_Id_V11()
            {
                var r = await sectionCoordinationService.CreateInstructionalEvent4Async(new Dtos.InstructionalEvent4()
                {
                    Id = meeting1.Guid,
                    Section = new Dtos.GuidObject2()
                });
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_SectionKeyNotFound_V11()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionByGuidAsync(section1.Guid)).Throws(new KeyNotFoundException());
                var r = await sectionCoordinationService.CreateInstructionalEvent4Async(new Dtos.InstructionalEvent4()
                {
                    Id = meeting1.Guid,
                    Section = new Dtos.GuidObject2() { Id = section1.Guid }
                });
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_InvalidId_V11()
            {
                var instrEvent = new Dtos.InstructionalEvent4() { Id = "999999999999999" };
                var r = await sectionCoordinationService.CreateInstructionalEvent4Async(instrEvent);
            }

            [TestMethod]
            public async Task SectionService_OverrideRoomCapacity_V11()
            {
                section1.SectionCapacity = 0;
                var approval = new Dtos.InstructionalEventApproval2() { ApprovingEntity = Dtos.InstructionalEventApprovalEntity2.User, Type = Dtos.InstructionalEventApprovalType2.RoomCapacity };
                var timePeriod = new Dtos.RepeatTimePeriod2
                {
                    StartOn = new DateTime(2012, 1, 1),
                    EndOn = new DateTime(2012, 12, 21)
                };
                var repeatRuleEnds = new Dtos.RepeatRuleEnds { Date = new DateTime(2012, 12, 21) };
                var repeatRule = new Dtos.RepeatRuleDaily { Type = Dtos.FrequencyType2.Daily, Ends = repeatRuleEnds, Interval = 1 };
                var instrEvent = new Dtos.InstructionalEvent4()
                {
                    Id = meeting1.Guid,
                    Section = new Dtos.GuidObject2() { Id = section1.Guid },
                    InstructionalMethod = new Dtos.GuidObject2() { Id = instructionalMethodGuid },
                    Workload = 10.5m,
                    Approvals = new List<Dtos.InstructionalEventApproval2>() { approval },
                    Recurrence = new Dtos.Recurrence3() { RepeatRule = repeatRule, TimePeriod = timePeriod }
                };
                var r = await sectionCoordinationService.CreateInstructionalEvent4Async(instrEvent);
                Assert.AreEqual(r.Id, meeting1.Guid);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task SectionService_UpdateInstructionalEvent4Async_PermissionsException()
            {
                roleRepo = null;
                var r = await sectionCoordinationService.UpdateInstructionalEvent4Async(new Dtos.InstructionalEvent4()
                {
                    Locations = new List<Dtos.Location>()
                    {
                        new Dtos.Location()
                        {

                        }
                    }
                });
            }
        }

        [TestClass]
        public class InstructionalEvents_Post : CurrentUserSetup
        {
            private SectionCoordinationService sectionCoordinationService;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IStudentReferenceDataRepository> stuRefDataRepoMock;
            private IStudentReferenceDataRepository studentReferenceDataRepo;
            private Mock<IReferenceDataRepository> referenceDataRepoMock;
            private IReferenceDataRepository referenceDataRepo;
            private Mock<ICourseRepository> courseRepoMock;
            private ICourseRepository courseRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IStudentConfigurationRepository> studentConfigRepoMock;
            private IStudentConfigurationRepository studentConfigRepo;
            private Mock<IConfigurationRepository> configRepoMock;
            private IConfigurationRepository configRepo;
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IRoomRepository> roomRepoMock;
            private IRoomRepository roomRepo;
            private Mock<IEventRepository> eventRepoMock;
            private IEventRepository eventRepo;
            private Mock<IBookRepository> bookRepoMock;
            private IBookRepository bookRepo;


            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ILogger logger;
            private ICurrentUserFactory currentUserFactory;

            private Domain.Student.Entities.Section section1;
            private Domain.Student.Entities.Section section2;
            private Domain.Student.Entities.Section section3;
            private Domain.Student.Entities.SectionMeeting meeting1;
            private Domain.Student.Entities.SectionMeeting meeting2;
            private Domain.Student.Entities.SectionMeeting meeting3;
            private Domain.Student.Entities.SectionMeeting meeting4;

            private List<OfferingDepartment> dpts = new List<OfferingDepartment>() { new OfferingDepartment("ART", 100m) };
            private List<string> levels = new List<string>() { "Whatever" };
            private List<SectionStatusItem> statuses = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-60)) };

            private string meeting1Guid = Guid.NewGuid().ToString();
            private string instructionalMethodGuid = Guid.NewGuid().ToString();
            private string roomGuid = Guid.NewGuid().ToString();

            [TestInitialize]
            public void Initialize()
            {
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                roomRepoMock = new Mock<IRoomRepository>();
                roomRepo = roomRepoMock.Object;
                stuRefDataRepoMock = new Mock<IStudentReferenceDataRepository>();
                studentReferenceDataRepo = stuRefDataRepoMock.Object;
                referenceDataRepoMock = new Mock<IReferenceDataRepository>();
                referenceDataRepo = referenceDataRepoMock.Object;
                courseRepoMock = new Mock<ICourseRepository>();
                courseRepo = courseRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                studentConfigRepoMock = new Mock<IStudentConfigurationRepository>();
                studentConfigRepo = studentConfigRepoMock.Object;
                configRepoMock = new Mock<IConfigurationRepository>();
                configRepo = configRepoMock.Object;
                eventRepoMock = new Mock<IEventRepository>();
                eventRepo = eventRepoMock.Object;
                bookRepoMock = new Mock<IBookRepository>();
                bookRepo = bookRepoMock.Object;


                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;

                // Set Current User
                currentUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();

                logger = new Mock<ILogger>().Object;

                // Mock the section repo response
                section1 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC1", "1119", "01", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses, true);
                section1.TermId = "2012/FA";
                section1.EndDate = new DateTime(2012, 12, 21);
                section1.Guid = Guid.NewGuid().ToString();

                section2 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC2", "1119", "02", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses, true);
                section2.TermId = "2012/FA";
                section2.Guid = Guid.NewGuid().ToString();

                section3 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC3", "1119", "03", new DateTime(2011, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses, true);
                section3.TermId = "2011/FA";
                section3.EndDate = new DateTime(2011, 12, 21);
                section3.Guid = Guid.NewGuid().ToString();

                var sectionFaculty = new Domain.Student.Entities.SectionFaculty("1234", "SEC1", "0000678", "LEC", new DateTime(2012, 09, 01), new DateTime(2011, 12, 21), 100.0m);

                meeting1 = new Domain.Student.Entities.SectionMeeting("123", "SEC1", "LEC", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "W");
                meeting1.Guid = meeting1Guid;
                meeting1.AddFacultyId("0000678");
                meeting1.AddSectionFaculty(sectionFaculty);
                meeting1.StartTime = new DateTimeOffset(2012, 01, 01, 08, 30, 00, new TimeSpan());
                meeting1.EndTime = new DateTimeOffset(2012, 12, 21, 09, 30, 00, new TimeSpan());
                meeting1.Room = "DAN*102";
                section1.AddSectionMeeting(meeting1);

                meeting2 = new Domain.Student.Entities.SectionMeeting("123", "SEC1", "LEC", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "D");
                meeting2.Guid = meeting1Guid;

                meeting3 = new Domain.Student.Entities.SectionMeeting("456", "SEC1", "LEC", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "M");
                meeting3.Guid = meeting1Guid;

                meeting4 = new Domain.Student.Entities.SectionMeeting("789", "SEC1", "LEC", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "Y");
                meeting4.Guid = meeting1Guid;

                var instructionalMethods = new List<Domain.Student.Entities.InstructionalMethod>();
                var instructionalMethod = new Domain.Student.Entities.InstructionalMethod(instructionalMethodGuid, "LEC", "Lecture", false);
                instructionalMethods.Add(instructionalMethod);

                var rooms = new List<Domain.Base.Entities.Room>();
                var room = new Domain.Base.Entities.Room(roomGuid, "DAN*102", "Danville Hall Room 102");
                rooms.Add(room);

                var scheduleRepeats = new List<Domain.Base.Entities.ScheduleRepeat>();
                var scheduleRepeat = new Domain.Base.Entities.ScheduleRepeat("W", "Weekly", "7", FrequencyType.Weekly);
                scheduleRepeats.Add(scheduleRepeat);
                scheduleRepeat = new Domain.Base.Entities.ScheduleRepeat("D", "Daily", "1", FrequencyType.Daily);
                scheduleRepeats.Add(scheduleRepeat);
                scheduleRepeat = new Domain.Base.Entities.ScheduleRepeat("M", "Monthly", "30", FrequencyType.Monthly);
                scheduleRepeats.Add(scheduleRepeat);
                scheduleRepeat = new Domain.Base.Entities.ScheduleRepeat("Y", "Yearly", "365", FrequencyType.Yearly);
                scheduleRepeats.Add(scheduleRepeat);

                sectionRepoMock.Setup(repo => repo.GetSectionByGuidAsync(section1.Guid)).ReturnsAsync(section1);
                sectionRepoMock.Setup(repo => repo.GetSectionByGuidAsync(section2.Guid)).ReturnsAsync(section2);
                sectionRepoMock.Setup(repo => repo.GetSectionByGuidAsync(section3.Guid)).ReturnsAsync(section3);
                sectionRepoMock.Setup(repo => repo.PostSectionMeetingAsync(It.IsAny<Domain.Student.Entities.Section>(), meeting1.Guid)).ReturnsAsync(meeting1);
                sectionRepoMock.Setup(repo => repo.PostSectionMeetingAsync(It.IsAny<Domain.Student.Entities.Section>(), meeting2.Guid)).ReturnsAsync(meeting2);
                sectionRepoMock.Setup(repo => repo.PostSectionMeetingAsync(It.IsAny<Domain.Student.Entities.Section>(), meeting3.Guid)).ReturnsAsync(meeting3);

                sectionRepoMock.Setup(repo => repo.GetSectionMeetingByGuidAsync(meeting1.Guid)).ReturnsAsync(meeting1);
                sectionRepoMock.Setup(repo => repo.GetSectionMeetingIdFromGuidAsync(meeting1.Guid)).ReturnsAsync(meeting1.Id);
                sectionRepoMock.Setup(repo => repo.GetSectionMeetingByGuidAsync(meeting2.Guid)).ReturnsAsync(meeting2);
                sectionRepoMock.Setup(repo => repo.GetSectionMeetingIdFromGuidAsync(meeting2.Guid)).ReturnsAsync(meeting2.Id);
                sectionRepoMock.Setup(repo => repo.GetSectionMeetingByGuidAsync(meeting3.Guid)).ReturnsAsync(meeting3);
                sectionRepoMock.Setup(repo => repo.GetSectionMeetingIdFromGuidAsync(meeting3.Guid)).ReturnsAsync(meeting3.Id);
                stuRefDataRepoMock.Setup(repo => repo.GetInstructionalMethodsAsync(It.IsAny<bool>())).ReturnsAsync(instructionalMethods);
                roomRepoMock.Setup(repo => repo.RoomsAsync()).ReturnsAsync(rooms);
                referenceDataRepoMock.Setup(repo => repo.ScheduleRepeats).Returns(scheduleRepeats);

                // Configuration defaults and campus calendar information.
                configRepoMock.Setup(repo => repo.GetDefaultsConfiguration()).Returns(new DefaultsConfiguration() { CampusCalendarId = "MAIN" });
                studentConfigRepoMock.Setup(repo => repo.GetCurriculumConfigurationAsync()).ReturnsAsync(new CurriculumConfiguration() { SectionActiveStatusCode = "A", SectionInactiveStatusCode = "I", DefaultInstructionalMethodCode = "INT" });
                eventRepoMock.Setup(repo => repo.GetCalendar("MAIN")).Returns(new CampusCalendar("MAIN", "Default Calendar", new TimeSpan(), new TimeSpan()));

                // Mock the section service
                sectionCoordinationService = new SectionCoordinationService(adapterRegistry, sectionRepo, courseRepo, studentRepo, studentReferenceDataRepo,
                    referenceDataRepo, termRepo, studentConfigRepo, configRepo, personRepo, roomRepo, eventRepo, bookRepo, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionRepo = null;
                studentRepo = null;
                roleRepo = null;
                adapterRegistry = null;
                sectionCoordinationService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_Null_Object_V6()
            {
                var r = await sectionCoordinationService.CreateInstructionalEvent2Async(new Dtos.InstructionalEvent2());
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_Null_Section_V6()
            {
                var r = await sectionCoordinationService.CreateInstructionalEvent2Async(new Dtos.InstructionalEvent2()
                {
                    Id = meeting1.Guid,
                });
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_Empty_Section_Id_V6()
            {
                var r = await sectionCoordinationService.CreateInstructionalEvent2Async(new Dtos.InstructionalEvent2()
                {
                    Id = meeting1.Guid,
                    Section = new Dtos.GuidObject2()
                });
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_SectionKeyNotFound_V6()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionByGuidAsync(section1.Guid)).Throws(new KeyNotFoundException());
                var r = await sectionCoordinationService.CreateInstructionalEvent2Async(new Dtos.InstructionalEvent2()
                {
                    Id = meeting1.Guid,
                    Section = new Dtos.GuidObject2() { Id = section1.Guid }
                });
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_InvalidId_V6()
            {
                var instrEvent = new Dtos.InstructionalEvent2() { Id = "999999999999999" };
                var r = await sectionCoordinationService.CreateInstructionalEvent2Async(instrEvent);
            }

            [TestMethod]
            public async Task SectionService_OverrideRoomCapacity_V6()
            {
                section1.SectionCapacity = 0;
                var approval = new Dtos.InstructionalEventApproval2() { ApprovingEntity = Dtos.InstructionalEventApprovalEntity2.User, Type = Dtos.InstructionalEventApprovalType2.RoomCapacity };
                var timePeriod = new Dtos.RepeatTimePeriod2
                {
                    StartOn = new DateTime(2012, 1, 1),
                    EndOn = new DateTime(2012, 12, 21)
                };
                var repeatRuleEnds = new Dtos.RepeatRuleEnds { Date = new DateTime(2012, 12, 21) };
                var repeatRule = new Dtos.RepeatRuleDaily { Type = Dtos.FrequencyType2.Daily, Ends = repeatRuleEnds, Interval = 1 };
                var instrEvent = new Dtos.InstructionalEvent2()
                {
                    Id = meeting1.Guid,
                    Section = new Dtos.GuidObject2() { Id = section1.Guid },
                    InstructionalMethod = new Dtos.GuidObject2() { Id = instructionalMethodGuid },
                    Workload = 10.5m,
                    Approvals = new List<Dtos.InstructionalEventApproval2>() { approval },
                    Recurrence = new Dtos.Recurrence3() { RepeatRule = repeatRule, TimePeriod = timePeriod }
                };
                var r = await sectionCoordinationService.CreateInstructionalEvent2Async(instrEvent);
                Assert.AreEqual(r.Id, meeting1.Guid);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_Null_Object_V8()
            {
                var r = await sectionCoordinationService.CreateInstructionalEvent3Async(new Dtos.InstructionalEvent3());
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_Null_Section_V8()
            {
                var r = await sectionCoordinationService.CreateInstructionalEvent3Async(new Dtos.InstructionalEvent3()
                {
                    Id = meeting1.Guid,
                });
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_Empty_Section_Id_V8()
            {
                var r = await sectionCoordinationService.CreateInstructionalEvent3Async(new Dtos.InstructionalEvent3()
                {
                    Id = meeting1.Guid,
                    Section = new Dtos.GuidObject2()
                });
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_SectionKeyNotFound_V8()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionByGuidAsync(section1.Guid)).Throws(new KeyNotFoundException());
                var r = await sectionCoordinationService.CreateInstructionalEvent3Async(new Dtos.InstructionalEvent3()
                {
                    Id = meeting1.Guid,
                    Section = new Dtos.GuidObject2() { Id = section1.Guid }
                });
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_InvalidId_V8()
            {
                var instrEvent = new Dtos.InstructionalEvent3() { Id = "999999999999999" };
                var r = await sectionCoordinationService.CreateInstructionalEvent3Async(instrEvent);
            }

            [TestMethod]
            public async Task SectionService_OverrideRoomCapacity_V8()
            {
                section1.SectionCapacity = 0;
                var approval = new Dtos.InstructionalEventApproval2() { ApprovingEntity = Dtos.InstructionalEventApprovalEntity2.User, Type = Dtos.InstructionalEventApprovalType2.RoomCapacity };
                var timePeriod = new Dtos.RepeatTimePeriod2
                {
                    StartOn = new DateTime(2012, 1, 1),
                    EndOn = new DateTime(2012, 12, 21)
                };
                var repeatRuleEnds = new Dtos.RepeatRuleEnds { Date = new DateTime(2012, 12, 21) };
                var repeatRule = new Dtos.RepeatRuleDaily { Type = Dtos.FrequencyType2.Daily, Ends = repeatRuleEnds, Interval = 1 };
                var instrEvent = new Dtos.InstructionalEvent3()
                {
                    Id = meeting1.Guid,
                    Section = new Dtos.GuidObject2() { Id = section1.Guid },
                    InstructionalMethod = new Dtos.GuidObject2() { Id = instructionalMethodGuid },
                    Workload = 10.5m,
                    Approvals = new List<Dtos.InstructionalEventApproval2>() { approval },
                    Recurrence = new Dtos.Recurrence3() { RepeatRule = repeatRule, TimePeriod = timePeriod }
                };
                var r = await sectionCoordinationService.CreateInstructionalEvent3Async(instrEvent);
                Assert.AreEqual(r.Id, meeting1.Guid);
            }
        }

        [TestClass]
        public class InstructionalEvents_Delete : CurrentUserSetup
        {
            private SectionCoordinationService sectionCoordinationService;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IStudentReferenceDataRepository> stuRefDataRepoMock;
            private IStudentReferenceDataRepository studentReferenceDataRepo;
            private Mock<IReferenceDataRepository> referenceDataRepoMock;
            private IReferenceDataRepository referenceDataRepo;
            private Mock<ICourseRepository> courseRepoMock;
            private ICourseRepository courseRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IStudentConfigurationRepository> studentConfigRepoMock;
            private IStudentConfigurationRepository studentConfigRepo;
            private Mock<IConfigurationRepository> configRepoMock;
            private IConfigurationRepository configRepo;
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IRoomRepository> roomRepoMock;
            private IRoomRepository roomRepo;
            private Mock<IEventRepository> eventRepoMock;
            private IEventRepository eventRepo;
            private Mock<IBookRepository> bookRepoMock;
            private IBookRepository bookRepo;


            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ILogger logger;
            private ICurrentUserFactory currentUserFactory;

            private Domain.Student.Entities.Section section1;
            private Domain.Student.Entities.Section section2;
            private Domain.Student.Entities.Section section3;
            private Domain.Student.Entities.SectionMeeting meeting1;
            private Domain.Student.Entities.SectionMeeting meeting2;
            private Domain.Student.Entities.SectionMeeting meeting3;
            private Domain.Student.Entities.SectionMeeting meeting4;

            private List<OfferingDepartment> dpts = new List<OfferingDepartment>() { new OfferingDepartment("ART", 100m) };
            private List<string> levels = new List<string>() { "Whatever" };
            private List<SectionStatusItem> statuses = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-60)) };

            private string meeting1Guid = Guid.NewGuid().ToString();
            private string instructionalMethodGuid = Guid.NewGuid().ToString();
            private string roomGuid = Guid.NewGuid().ToString();

            [TestInitialize]
            public void Initialize()
            {
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                roomRepoMock = new Mock<IRoomRepository>();
                roomRepo = roomRepoMock.Object;
                stuRefDataRepoMock = new Mock<IStudentReferenceDataRepository>();
                studentReferenceDataRepo = stuRefDataRepoMock.Object;
                referenceDataRepoMock = new Mock<IReferenceDataRepository>();
                referenceDataRepo = referenceDataRepoMock.Object;
                courseRepoMock = new Mock<ICourseRepository>();
                courseRepo = courseRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                studentConfigRepoMock = new Mock<IStudentConfigurationRepository>();
                studentConfigRepo = studentConfigRepoMock.Object;
                configRepoMock = new Mock<IConfigurationRepository>();
                configRepo = configRepoMock.Object;
                eventRepoMock = new Mock<IEventRepository>();
                eventRepo = eventRepoMock.Object;
                bookRepoMock = new Mock<IBookRepository>();
                bookRepo = bookRepoMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;

                // Set Current User
                currentUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();

                logger = new Mock<ILogger>().Object;

                // Mock the section repo response
                section1 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC1", "1119", "01", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses, true);
                section1.TermId = "2012/FA";
                section1.EndDate = new DateTime(2012, 12, 21);

                section2 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC2", "1119", "02", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses, true);
                section2.TermId = "2012/FA";

                section3 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC3", "1119", "03", new DateTime(2011, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses, true);
                section3.TermId = "2011/FA";
                section3.EndDate = new DateTime(2011, 12, 21);

                var sectionFaculty = new Domain.Student.Entities.SectionFaculty("1234", "SEC1", "0000678", "LEC", new DateTime(2012, 09, 01), new DateTime(2011, 12, 21), 100.0m);

                meeting1 = new Domain.Student.Entities.SectionMeeting("123", "SEC1", "LEC", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "W");
                meeting1.Guid = meeting1Guid;
                meeting1.AddFacultyId("0000678");
                meeting1.AddSectionFaculty(sectionFaculty);
                meeting1.StartTime = new DateTimeOffset(2012, 01, 01, 08, 30, 00, new TimeSpan());
                meeting1.EndTime = new DateTimeOffset(2012, 12, 21, 09, 30, 00, new TimeSpan());
                meeting1.Room = "DAN*102";
                section1.AddSectionMeeting(meeting1);

                meeting2 = new Domain.Student.Entities.SectionMeeting("123", "SEC1", "LEC", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "D");
                meeting2.Guid = meeting1Guid;

                meeting3 = new Domain.Student.Entities.SectionMeeting("456", "SEC1", "LEC", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "M");
                meeting3.Guid = meeting1Guid;

                meeting4 = new Domain.Student.Entities.SectionMeeting("789", "SEC1", "LEC", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "Y");
                meeting4.Guid = meeting1Guid;

                var instructionalMethods = new List<Domain.Student.Entities.InstructionalMethod>();
                var instructionalMethod = new Domain.Student.Entities.InstructionalMethod(instructionalMethodGuid, "LEC", "Lecture", false);
                instructionalMethods.Add(instructionalMethod);

                var rooms = new List<Domain.Base.Entities.Room>();
                var room = new Domain.Base.Entities.Room(roomGuid, "DAN*102", "Danville Hall Room 102");
                rooms.Add(room);

                var scheduleRepeats = new List<Domain.Base.Entities.ScheduleRepeat>();
                var scheduleRepeat = new Domain.Base.Entities.ScheduleRepeat("W", "Weekly", "7", FrequencyType.Weekly);
                scheduleRepeats.Add(scheduleRepeat);
                scheduleRepeat = new Domain.Base.Entities.ScheduleRepeat("D", "Daily", "1", FrequencyType.Daily);
                scheduleRepeats.Add(scheduleRepeat);
                scheduleRepeat = new Domain.Base.Entities.ScheduleRepeat("M", "Monthly", "30", FrequencyType.Monthly);
                scheduleRepeats.Add(scheduleRepeat);
                scheduleRepeat = new Domain.Base.Entities.ScheduleRepeat("Y", "Yearly", "365", FrequencyType.Yearly);
                scheduleRepeats.Add(scheduleRepeat);

                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(section1.Guid, It.IsAny<bool>())).ReturnsAsync(section1);
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(section2.Guid, It.IsAny<bool>())).ReturnsAsync(section2);
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(section3.Guid, It.IsAny<bool>())).ReturnsAsync(section3);

                sectionRepoMock.Setup(repo => repo.GetSectionMeetingByGuidAsync(meeting1.Guid)).ReturnsAsync(meeting1);
                stuRefDataRepoMock.Setup(repo => repo.GetInstructionalMethodsAsync(It.IsAny<bool>())).ReturnsAsync(instructionalMethods);
                roomRepoMock.Setup(repo => repo.RoomsAsync()).ReturnsAsync(rooms);
                referenceDataRepoMock.Setup(repo => repo.ScheduleRepeats).Returns(scheduleRepeats);

                // Mock the section service
                sectionCoordinationService = new SectionCoordinationService(adapterRegistry, sectionRepo, courseRepo, studentRepo, studentReferenceDataRepo,
                    referenceDataRepo, termRepo, studentConfigRepo, configRepo, personRepo, roomRepo, eventRepo, bookRepo, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionRepo = null;
                studentRepo = null;
                roleRepo = null;
                adapterRegistry = null;
                sectionCoordinationService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionService_Null_Guid()
            {
                await sectionCoordinationService.DeleteInstructionalEventAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task SectionService_InvalidGuid()
            {
                await sectionCoordinationService.DeleteInstructionalEventAsync("999999999999999");
            }

            [TestMethod]
            public async Task SectionService_Delete()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionAsync(meeting1.SectionId)).ReturnsAsync(section1);
                sectionRepoMock.Setup(repo => repo.DeleteSectionMeetingAsync(meeting1.Id, section1.Faculty.ToList()));
                await sectionCoordinationService.DeleteInstructionalEventAsync(meeting1.Guid);
            }
        }

        [TestClass]
        public class InstructionalEvents_Filters : CurrentUserSetup
        {
            private SectionCoordinationService sectionCoordinationService;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IStudentReferenceDataRepository> stuRefDataRepoMock;
            private IStudentReferenceDataRepository studentReferenceDataRepo;
            private Mock<IReferenceDataRepository> referenceDataRepoMock;
            private IReferenceDataRepository referenceDataRepo;
            private Mock<ICourseRepository> courseRepoMock;
            private ICourseRepository courseRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IStudentConfigurationRepository> studentConfigRepoMock;
            private IStudentConfigurationRepository studentConfigRepo;
            private Mock<IConfigurationRepository> configRepoMock;
            private IConfigurationRepository configRepo;
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IRoomRepository> roomRepoMock;
            private IRoomRepository roomRepo;
            private Mock<IEventRepository> eventRepoMock;
            private IEventRepository eventRepo;
            private Mock<IBookRepository> bookRepoMock;
            private IBookRepository bookRepo;


            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ILogger logger;
            private ICurrentUserFactory currentUserFactory;

            private Domain.Student.Entities.SectionMeeting meeting1;
            private Domain.Student.Entities.SectionMeeting meeting2;
            private Domain.Student.Entities.SectionMeeting meeting3;
            private Domain.Student.Entities.SectionMeeting meeting4;

            private string meeting1Guid = Guid.NewGuid().ToString();
            private string instructionalMethodGuid = Guid.NewGuid().ToString();
            private string roomGuid = Guid.NewGuid().ToString();
            private string sectionGuid = Guid.NewGuid().ToString();
            private string instructorGuid = Guid.NewGuid().ToString();
            private List<Domain.Student.Entities.AcademicPeriod> acadPeriodsEntities;

            [TestInitialize]
            public void Initialize()
            {
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                roomRepoMock = new Mock<IRoomRepository>();
                roomRepo = roomRepoMock.Object;
                stuRefDataRepoMock = new Mock<IStudentReferenceDataRepository>();
                studentReferenceDataRepo = stuRefDataRepoMock.Object;
                referenceDataRepoMock = new Mock<IReferenceDataRepository>();
                referenceDataRepo = referenceDataRepoMock.Object;
                courseRepoMock = new Mock<ICourseRepository>();
                courseRepo = courseRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                studentConfigRepoMock = new Mock<IStudentConfigurationRepository>();
                studentConfigRepo = studentConfigRepoMock.Object;
                configRepoMock = new Mock<IConfigurationRepository>();
                configRepo = configRepoMock.Object;
                eventRepoMock = new Mock<IEventRepository>();
                eventRepo = eventRepoMock.Object;
                bookRepoMock = new Mock<IBookRepository>();
                bookRepo = bookRepoMock.Object;

                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;

                // Set Current User
                currentUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();

                logger = new Mock<ILogger>().Object;

                var sectionFaculty = new Domain.Student.Entities.SectionFaculty("1234", "SEC1", "0000678", "LEC", new DateTime(2012, 09, 01), new DateTime(2011, 12, 21), 100.0m);

                meeting1 = new Domain.Student.Entities.SectionMeeting("123", "SEC1", "LEC", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "W");
                meeting1.Guid = meeting1Guid;
                meeting1.AddFacultyId("0000678");
                meeting1.AddSectionFaculty(sectionFaculty);
                meeting1.StartTime = new DateTimeOffset(2012, 01, 01, 08, 30, 00, new TimeSpan());
                meeting1.EndTime = new DateTimeOffset(2012, 12, 21, 09, 30, 00, new TimeSpan());
                meeting1.Room = "DAN*102";

                meeting2 = new Domain.Student.Entities.SectionMeeting("123", "SEC1", "LEC", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "D");
                meeting2.Guid = meeting1Guid;

                meeting3 = new Domain.Student.Entities.SectionMeeting("456", "SEC1", "LEC", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "M");
                meeting3.Guid = meeting1Guid;

                meeting4 = new Domain.Student.Entities.SectionMeeting("789", "SEC1", "LEC", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "Y");
                meeting4.Guid = meeting1Guid;

                var instructionalMethods = new List<Domain.Student.Entities.InstructionalMethod>();
                var instructionalMethod = new Domain.Student.Entities.InstructionalMethod(instructionalMethodGuid, "LEC", "Lecture", false);
                instructionalMethods.Add(instructionalMethod);

                var rooms = new List<Domain.Base.Entities.Room>();
                var room = new Domain.Base.Entities.Room(roomGuid, "DAN*102", "Danville Hall Room 102");
                rooms.Add(room);

                var scheduleRepeats = new List<Domain.Base.Entities.ScheduleRepeat>();
                var scheduleRepeat = new Domain.Base.Entities.ScheduleRepeat("W", "Weekly", "7", FrequencyType.Weekly);
                scheduleRepeats.Add(scheduleRepeat);
                scheduleRepeat = new Domain.Base.Entities.ScheduleRepeat("D", "Daily", "1", FrequencyType.Daily);
                scheduleRepeats.Add(scheduleRepeat);
                scheduleRepeat = new Domain.Base.Entities.ScheduleRepeat("M", "Monthly", "30", FrequencyType.Monthly);
                scheduleRepeats.Add(scheduleRepeat);
                scheduleRepeat = new Domain.Base.Entities.ScheduleRepeat("Y", "Yearly", "365", FrequencyType.Yearly);
                scheduleRepeats.Add(scheduleRepeat);

                sectionRepoMock.Setup(repo => repo.GetSectionIdFromGuidAsync(sectionGuid)).ReturnsAsync("SEC1");
                sectionRepoMock.Setup(repo => repo.GetSectionGuidFromIdAsync("SEC1")).ReturnsAsync(sectionGuid);
                stuRefDataRepoMock.Setup(repo => repo.GetInstructionalMethodsAsync(It.IsAny<bool>())).ReturnsAsync(instructionalMethods);

                foreach (var i in instructionalMethods)
                {
                    stuRefDataRepoMock.Setup(repo => repo.GetInstructionalMethodGuidAsync(i.Code)).ReturnsAsync(i.Guid);

                }

                roomRepoMock.Setup(repo => repo.RoomsAsync()).ReturnsAsync(rooms);
                roomRepoMock.Setup(repo => repo.GetRoomsAsync(false)).ReturnsAsync(rooms);

                foreach (var r in rooms)
                {
                    roomRepoMock.Setup(repo => repo.GetRoomsGuidAsync(r.Id)).ReturnsAsync(r.Guid);
                }

                referenceDataRepoMock.Setup(repo => repo.ScheduleRepeats).Returns(scheduleRepeats);
                personRepoMock.Setup(repo => repo.GetPersonGuidFromIdAsync("0000678")).ReturnsAsync(instructorGuid);
                sectionRepoMock.Setup(repo => repo.GetSectionMeetingAsync(It.IsAny<string>())).ReturnsAsync(meeting1);
                personRepoMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("0000678");

                //Acad Periods
                acadPeriodsEntities = new List<Domain.Student.Entities.AcademicPeriod>()
                {
                    new Domain.Student.Entities.AcademicPeriod("8d3fcd4d-cec8-4405-90eb-948392bd0a7e", "2012/FA", "Fall 2014", DateTime.Today.AddDays(-60), DateTime.Today.AddDays(10), DateTime.Today.Year, 4, "Spring", "5f7e7071-5aef-4d22-891f-86ab472a9f15",
                        "edcfd1ee-4adf-46bc-8b87-8853ae49dbeb", null)
                };
                termRepoMock.Setup(repo => repo.GetAcademicPeriods(It.IsAny<IEnumerable<Term>>())).Returns(acadPeriodsEntities);

                foreach (var acadPeriodsEntity in acadPeriodsEntities)
                {
                    termRepoMock.Setup(repo => repo.GetAcademicPeriodsCodeFromGuidAsync(acadPeriodsEntity.Guid)).ReturnsAsync(acadPeriodsEntity.Code);

                }

                // Mock the section service
                sectionCoordinationService = new SectionCoordinationService(adapterRegistry, sectionRepo, courseRepo, studentRepo, studentReferenceDataRepo,
                    referenceDataRepo, termRepo, studentConfigRepo, configRepo, personRepo, roomRepo, eventRepo, bookRepo, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionRepo = null;
                studentRepo = null;
                roleRepo = null;
                adapterRegistry = null;
                sectionCoordinationService = null;
                acadPeriodsEntities = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SectionService_Null_Arguments_V6()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionMeetingAsync(It.IsAny<int>(), It.IsAny<int>(), "", "", "", "", "", new List<string>(), new List<string>(), new List<string>(), "")).Throws(new ArgumentException());
                await sectionCoordinationService.GetInstructionalEvent2Async(It.IsAny<int>(), It.IsAny<int>(), "", "", "", "", "");
            }

            [TestMethod]
            public async Task SectionService_Section_V6()
            {
                var sectionMeetings = new List<Domain.Student.Entities.SectionMeeting>();
                sectionMeetings.Add(meeting1);
                var tuple = new Tuple<IEnumerable<Domain.Student.Entities.SectionMeeting>, int>(sectionMeetings, 1);
                sectionRepoMock.Setup(repo => repo.GetSectionMeetingAsync(0, 1, "SEC1", "", "", "", "", new List<string>(), new List<string>(), new List<string>(), "")).ReturnsAsync(tuple);
                var r = await sectionCoordinationService.GetInstructionalEvent2Async(0, 1, sectionGuid, "", "", "", "");
                var instrEvent = r.Item1.FirstOrDefault();
                Assert.AreEqual(instrEvent.Section.Id, sectionGuid);
            }

            [TestMethod]
            public async Task SectionService_StartDate_V6()
            {
                var sectionMeetings = new List<Domain.Student.Entities.SectionMeeting>();
                sectionMeetings.Add(meeting1);
                var tuple = new Tuple<IEnumerable<Domain.Student.Entities.SectionMeeting>, int>(sectionMeetings, 1);
                sectionRepoMock.Setup(repo => repo.GetSectionMeetingAsync(0, 1, "", new DateTime(2012, 01, 01, 0, 0, 0, DateTimeKind.Local).ToShortDateString(), "", "", "", new List<string>(), new List<string>(), new List<string>(), "")).ReturnsAsync(tuple);
                sectionRepoMock.Setup(repo => repo.GetUnidataFormattedDate(new DateTime(2012, 01, 01, 0, 0, 0, DateTimeKind.Local).ToShortDateString())).ReturnsAsync(new DateTime(2012, 01, 01).ToShortDateString());
                var r = await sectionCoordinationService.GetInstructionalEvent2Async(0, 1, "", new DateTime(2012, 01, 01, 0, 0, 0, DateTimeKind.Local).ToShortDateString(), "", "", "");
                var instrEvent = r.Item1.FirstOrDefault();
                Assert.AreEqual(instrEvent.Recurrence.TimePeriod.StartOn.Value.Date.ToShortDateString(), new DateTime(2012, 01, 01, 0, 0, 0, DateTimeKind.Local).ToShortDateString());
            }

            [TestMethod]
            public async Task SectionService_EndDate_V6()
            {
                var sectionMeetings = new List<Domain.Student.Entities.SectionMeeting>();
                sectionMeetings.Add(meeting1);
                var tuple = new Tuple<IEnumerable<Domain.Student.Entities.SectionMeeting>, int>(sectionMeetings, 1);
                sectionRepoMock.Setup(repo => repo.GetSectionMeetingAsync(0, 1, "", "", new DateTime(2012, 12, 21, 0, 0, 0, DateTimeKind.Local).ToShortDateString(), "", "", new List<string>(), new List<string>(), new List<string>(), "")).ReturnsAsync(tuple);
                sectionRepoMock.Setup(repo => repo.GetUnidataFormattedDate(new DateTime(2012, 12, 21, 0, 0, 0, DateTimeKind.Local).ToShortDateString())).ReturnsAsync(new DateTime(2012, 12, 21, 0, 0, 0, DateTimeKind.Local).ToShortDateString());
                var r = await sectionCoordinationService.GetInstructionalEvent2Async(0, 1, "", "", new DateTime(2012, 12, 21, 0, 0, 0, DateTimeKind.Local).ToShortDateString(), "", "");
                var instrEvent = r.Item1.FirstOrDefault();
                Assert.AreEqual(instrEvent.Recurrence.TimePeriod.EndOn.Value.Date.ToShortDateString(), new DateTime(2012, 12, 21, 0, 0, 0, DateTimeKind.Local).ToShortDateString());
            }

            [TestMethod]
            public async Task SectionService_StartTime_V6()
            {
                var sectionMeetings = new List<Domain.Student.Entities.SectionMeeting>();
                sectionMeetings.Add(meeting1);
                var tuple = new Tuple<IEnumerable<Domain.Student.Entities.SectionMeeting>, int>(sectionMeetings, 1);
                sectionRepoMock.Setup(repo => repo.GetSectionMeetingAsync(0, 1, "", new DateTime(2012, 01, 01, 0, 0, 0, DateTimeKind.Local).ToShortDateString(), "", new DateTime(2012, 01, 01, 08, 30, 0, DateTimeKind.Local).ToLocalTime().TimeOfDay.ToString(), "", new List<string>(), new List<string>(), new List<string>(), "")).ReturnsAsync(tuple);
                sectionRepoMock.Setup(repo => repo.GetUnidataFormattedDate(new DateTime(2012, 01, 01, 08, 30, 0, DateTimeKind.Local).ToString("yyyy-MM-ddThh:mm:ss"))).ReturnsAsync(new DateTime(2012, 01, 01, 08, 30, 0, DateTimeKind.Local).ToShortDateString());
                var r = await sectionCoordinationService.GetInstructionalEvent2Async(0, 1, "", new DateTime(2012, 01, 01, 08, 30, 0, DateTimeKind.Local).ToString("yyyy-MM-ddThh:mm:ss"), "", "", "");
                var instrEvent = r.Item1.FirstOrDefault();
                Assert.AreEqual(instrEvent.Recurrence.TimePeriod.StartOn.Value.TimeOfDay.ToString(), new DateTime(2012, 01, 01, 08, 30, 0, DateTimeKind.Local).ToLocalTime().TimeOfDay.ToString());
            }

            [TestMethod]
            public async Task SectionService_EndTime_V6()
            {
                var sectionMeetings = new List<Domain.Student.Entities.SectionMeeting>();
                sectionMeetings.Add(meeting1);
                var tuple = new Tuple<IEnumerable<Domain.Student.Entities.SectionMeeting>, int>(sectionMeetings, 1);
                sectionRepoMock.Setup(repo => repo.GetSectionMeetingAsync(0, 1, "", "", new DateTime(2012, 12, 21, 0, 0, 0, DateTimeKind.Local).ToShortDateString(), "", new DateTime(2012, 12, 21, 09, 30, 0, DateTimeKind.Local).ToLocalTime().TimeOfDay.ToString(), new List<string>(), new List<string>(), new List<string>(), "")).ReturnsAsync(tuple);
                sectionRepoMock.Setup(repo => repo.GetUnidataFormattedDate(new DateTime(2012, 12, 21, 09, 30, 0, DateTimeKind.Local).ToString("yyyy-MM-ddThh:mm:ss"))).ReturnsAsync(new DateTime(2012, 12, 21, 0, 0, 0, DateTimeKind.Local).ToShortDateString());
                var r = await sectionCoordinationService.GetInstructionalEvent2Async(0, 1, "", "", new DateTime(2012, 12, 21, 09, 30, 0, DateTimeKind.Local).ToString("yyyy-MM-ddThh:mm:ss"), "", "");
                var instrEvent = r.Item1.FirstOrDefault();
                Assert.AreEqual(instrEvent.Recurrence.TimePeriod.EndOn.Value.TimeOfDay.ToString(), new DateTime(2012, 12, 21, 09, 30, 0, DateTimeKind.Local).ToLocalTime().TimeOfDay.ToString());
            }

            [TestMethod]
            public async Task SectionService_ClassRoom_V6()
            {
                var sectionMeetings = new List<Domain.Student.Entities.SectionMeeting>();
                sectionMeetings.Add(meeting1);
                var tuple = new Tuple<IEnumerable<Domain.Student.Entities.SectionMeeting>, int>(sectionMeetings, 1);
                sectionRepoMock.Setup(repo => repo.GetSectionMeetingAsync(0, 1, "", "", "", "", "", new List<string>() { "DAN" }, new List<string>() { "102" }, new List<string>(), "")).ReturnsAsync(tuple);
                var r = await sectionCoordinationService.GetInstructionalEvent2Async(0, 1, "", "", "", roomGuid, "");
                var instrEvent = r.Item1.FirstOrDefault();
                Assert.AreEqual(((Dtos.InstructionalRoom2)instrEvent.Locations.ElementAt(0).Locations).Room.Id, roomGuid);
            }

            [TestMethod]
            public async Task SectionService_Instructor_V6()
            {
                var sectionMeetings = new List<Domain.Student.Entities.SectionMeeting>();
                sectionMeetings.Add(meeting1);
                var tuple = new Tuple<IEnumerable<Domain.Student.Entities.SectionMeeting>, int>(sectionMeetings, 1);
                sectionRepoMock.Setup(repo => repo.GetSectionMeetingAsync(0, 1, "", "", "", "", "", new List<string>(), new List<string>(), new List<string>() { "0000678" }, "")).ReturnsAsync(tuple);
                var r = await sectionCoordinationService.GetInstructionalEvent2Async(0, 1, "", "", "", "", instructorGuid);
                var instrEvent = r.Item1.FirstOrDefault();
                Assert.AreEqual(instrEvent.Instructors.FirstOrDefault().Instructor.Id, instructorGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SectionService_Null_Arguments_V8()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionMeetingAsync(It.IsAny<int>(), It.IsAny<int>(), "", "", "", "", "", new List<string>(), new List<string>(), new List<string>(), "")).Throws(new ArgumentException());
                await sectionCoordinationService.GetInstructionalEvent3Async(It.IsAny<int>(), It.IsAny<int>(), "", "", "", new List<string>(), new List<string>(), "");
            }

            [TestMethod]
            public async Task SectionService_Section_V8()
            {
                var sectionMeetings = new List<Domain.Student.Entities.SectionMeeting>();
                sectionMeetings.Add(meeting1);
                var tuple = new Tuple<IEnumerable<Domain.Student.Entities.SectionMeeting>, int>(sectionMeetings, 1);
                sectionRepoMock.Setup(repo => repo.GetSectionMeetingAsync(0, 1, "SEC1", "", "", "", "", new List<string>(), new List<string>(), new List<string>(), "")).ReturnsAsync(tuple);
                var r = await sectionCoordinationService.GetInstructionalEvent3Async(0, 1, sectionGuid, "", "", new List<string>(), new List<string>(), "");
                var instrEvent = r.Item1.FirstOrDefault();
                Assert.AreEqual(instrEvent.Section.Id, sectionGuid);
            }

            [TestMethod]
            public async Task SectionService_StartDate_V8()
            {
                var sectionMeetings = new List<Domain.Student.Entities.SectionMeeting>();
                sectionMeetings.Add(meeting1);
                var tuple = new Tuple<IEnumerable<Domain.Student.Entities.SectionMeeting>, int>(sectionMeetings, 1);
                sectionRepoMock.Setup(repo => repo.GetSectionMeetingAsync(0, 1, "", new DateTime(2012, 01, 01, 0, 0, 0, DateTimeKind.Local).ToShortDateString(), "", "", "", new List<string>(), new List<string>(), new List<string>(), "")).ReturnsAsync(tuple);
                sectionRepoMock.Setup(repo => repo.GetUnidataFormattedDate(new DateTime(2012, 01, 01, 0, 0, 0, DateTimeKind.Local).ToShortDateString())).ReturnsAsync(new DateTime(2012, 01, 01).ToShortDateString());
                var r = await sectionCoordinationService.GetInstructionalEvent3Async(0, 1, "", new DateTime(2012, 01, 01, 0, 0, 0, DateTimeKind.Local).ToShortDateString(), "", new List<string>(), new List<string>(), "");
                var instrEvent = r.Item1.FirstOrDefault();
                Assert.AreEqual(instrEvent.Recurrence.TimePeriod.StartOn.Value.Date.ToShortDateString(), new DateTime(2012, 01, 01, 0, 0, 0, DateTimeKind.Local).ToShortDateString());
            }

            [TestMethod]
            public async Task SectionService_EndDate_V8()
            {
                var sectionMeetings = new List<Domain.Student.Entities.SectionMeeting>();
                sectionMeetings.Add(meeting1);
                var tuple = new Tuple<IEnumerable<Domain.Student.Entities.SectionMeeting>, int>(sectionMeetings, 1);
                sectionRepoMock.Setup(repo => repo.GetSectionMeetingAsync(0, 1, "", "", new DateTime(2012, 12, 21, 0, 0, 0, DateTimeKind.Local).ToShortDateString(), "", "", new List<string>(), new List<string>(), new List<string>(), "")).ReturnsAsync(tuple);
                sectionRepoMock.Setup(repo => repo.GetUnidataFormattedDate(new DateTime(2012, 12, 21, 0, 0, 0, DateTimeKind.Local).ToShortDateString())).ReturnsAsync(new DateTime(2012, 12, 21, 0, 0, 0, DateTimeKind.Local).ToShortDateString());
                var r = await sectionCoordinationService.GetInstructionalEvent3Async(0, 1, "", "", new DateTime(2012, 12, 21, 0, 0, 0, DateTimeKind.Local).ToShortDateString(), new List<string>(), new List<string>(), "");
                var instrEvent = r.Item1.FirstOrDefault();
                Assert.AreEqual(instrEvent.Recurrence.TimePeriod.EndOn.Value.Date.ToShortDateString(), new DateTime(2012, 12, 21, 0, 0, 0, DateTimeKind.Local).ToShortDateString());
            }

            [TestMethod]
            public async Task SectionService_StartTime_V8()
            {
                var sectionMeetings = new List<Domain.Student.Entities.SectionMeeting>();
                sectionMeetings.Add(meeting1);
                var tuple = new Tuple<IEnumerable<Domain.Student.Entities.SectionMeeting>, int>(sectionMeetings, 1);
                sectionRepoMock.Setup(repo => repo.GetSectionMeetingAsync(0, 1, "", new DateTime(2012, 01, 01, 0, 0, 0, DateTimeKind.Local).ToShortDateString(), "", new DateTime(2012, 01, 01, 08, 30, 0, DateTimeKind.Local).ToLocalTime().TimeOfDay.ToString(), "", new List<string>(), new List<string>(), new List<string>(), "")).ReturnsAsync(tuple);
                sectionRepoMock.Setup(repo => repo.GetUnidataFormattedDate(new DateTime(2012, 01, 01, 08, 30, 0, DateTimeKind.Local).ToString("yyyy-MM-ddThh:mm:ss"))).ReturnsAsync(new DateTime(2012, 01, 01, 08, 30, 0, DateTimeKind.Local).ToShortDateString());
                var r = await sectionCoordinationService.GetInstructionalEvent3Async(0, 1, "", new DateTime(2012, 01, 01, 08, 30, 0, DateTimeKind.Local).ToString("yyyy-MM-ddThh:mm:ss"), "", new List<string>(), new List<string>(), "");
                var instrEvent = r.Item1.FirstOrDefault();
                Assert.AreEqual(instrEvent.Recurrence.TimePeriod.StartOn.Value.TimeOfDay.ToString(), new DateTime(2012, 01, 01, 08, 30, 0, DateTimeKind.Local).ToLocalTime().TimeOfDay.ToString());
            }

            [TestMethod]
            public async Task SectionService_EndTime_V8()
            {
                var sectionMeetings = new List<Domain.Student.Entities.SectionMeeting>();
                sectionMeetings.Add(meeting1);
                var tuple = new Tuple<IEnumerable<Domain.Student.Entities.SectionMeeting>, int>(sectionMeetings, 1);
                sectionRepoMock.Setup(repo => repo.GetSectionMeetingAsync(0, 1, "", "", new DateTime(2012, 12, 21, 0, 0, 0, DateTimeKind.Local).ToShortDateString(), "", new DateTime(2012, 12, 21, 09, 30, 0, DateTimeKind.Local).ToLocalTime().TimeOfDay.ToString(), new List<string>(), new List<string>(), new List<string>(), "")).ReturnsAsync(tuple);
                sectionRepoMock.Setup(repo => repo.GetUnidataFormattedDate(new DateTime(2012, 12, 21, 09, 30, 0, DateTimeKind.Local).ToString("yyyy-MM-ddThh:mm:ss"))).ReturnsAsync(new DateTime(2012, 12, 21, 0, 0, 0, DateTimeKind.Local).ToShortDateString());
                var r = await sectionCoordinationService.GetInstructionalEvent3Async(0, 1, "", "", new DateTime(2012, 12, 21, 09, 30, 0, DateTimeKind.Local).ToString("yyyy-MM-ddThh:mm:ss"), new List<string>(), new List<string>(), "");
                var instrEvent = r.Item1.FirstOrDefault();
                Assert.AreEqual(instrEvent.Recurrence.TimePeriod.EndOn.Value.TimeOfDay.ToString(), new DateTime(2012, 12, 21, 09, 30, 0, DateTimeKind.Local).ToLocalTime().TimeOfDay.ToString());
            }

            [TestMethod]
            public async Task SectionService_ClassRoom_V8()
            {
                var sectionMeetings = new List<Domain.Student.Entities.SectionMeeting>();
                sectionMeetings.Add(meeting1);
                var tuple = new Tuple<IEnumerable<Domain.Student.Entities.SectionMeeting>, int>(sectionMeetings, 1);
                sectionRepoMock.Setup(repo => repo.GetSectionMeetingAsync(0, 1, "", "", "", "", "", new List<string>() { "DAN" }, new List<string>() { "102" }, new List<string>(), "")).ReturnsAsync(tuple);
                var r = await sectionCoordinationService.GetInstructionalEvent3Async(0, 1, "", "", "", new List<string>() { roomGuid }, new List<string>(), "");
                var instrEvent = r.Item1.FirstOrDefault();
                Assert.AreEqual(((Dtos.InstructionalRoom2)instrEvent.Locations.ElementAt(0).Locations).Room.Id, roomGuid);
            }

            [TestMethod]
            public async Task SectionService_Instructor_V8()
            {
                var sectionMeetings = new List<Domain.Student.Entities.SectionMeeting>();
                sectionMeetings.Add(meeting1);
                var tuple = new Tuple<IEnumerable<Domain.Student.Entities.SectionMeeting>, int>(sectionMeetings, 1);
                sectionRepoMock.Setup(repo => repo.GetSectionMeetingAsync(0, 1, "", "", "", "", "", new List<string>(), new List<string>(), new List<string>() { "0000678" }, "")).ReturnsAsync(tuple);
                var r = await sectionCoordinationService.GetInstructionalEvent3Async(0, 1, "", "", "", new List<string>(), new List<string>() { instructorGuid }, "");
                var instrEvent = r.Item1.FirstOrDefault();
                Assert.AreEqual(instrEvent.Instructors.FirstOrDefault().Instructor.Id, instructorGuid);
            }

            [TestMethod]
            public async Task SectionService_AcademicPeriod_V8()
            {
                var sectionMeetings = new List<Domain.Student.Entities.SectionMeeting>();
                sectionMeetings.Add(meeting1);
                var tuple = new Tuple<IEnumerable<Domain.Student.Entities.SectionMeeting>, int>(sectionMeetings, 1);
                sectionRepoMock.Setup(repo => repo.GetSectionMeetingAsync(0, 1, "", "", "", "", "", new List<string>(), new List<string>(), new List<string>(), "2012/FA")).ReturnsAsync(tuple);
                var r = await sectionCoordinationService.GetInstructionalEvent3Async(0, 1, "", "", "", new List<string>(), new List<string>(), acadPeriodsEntities.FirstOrDefault().Guid);
                var instrEvent = r.Item1.FirstOrDefault();
                Assert.AreEqual(instrEvent.Id, meeting1.Guid);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_Null_Arguments_V11()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionMeeting2Async(It.IsAny<int>(), It.IsAny<int>(), "", "", "", "", "", new List<string>(), new List<string>(), new List<string>(), "")).Throws(new ArgumentException());
                await sectionCoordinationService.GetInstructionalEvent4Async(It.IsAny<int>(), It.IsAny<int>(), "", "", "", "");
            }

            [TestMethod]
            public async Task SectionService_Section_V11()
            {
                var sectionMeetings = new List<Domain.Student.Entities.SectionMeeting>();
                sectionMeetings.Add(meeting1);
                Dictionary<string, string> sectDict = new Dictionary<string, string>();
                sectDict.Add(meeting1.SectionId, sectionGuid);
                sectionRepoMock.Setup(sr => sr.GetSectionGuidsCollectionAsync(It.IsAny<string[]>())).ReturnsAsync(sectDict);

                var tuple = new Tuple<IEnumerable<Domain.Student.Entities.SectionMeeting>, int>(sectionMeetings, 1);
                sectionRepoMock.Setup(repo => repo.GetSectionMeeting2Async(0, 1, "SEC1", "", "", "", "", new List<string>(), new List<string>(), new List<string>(), "")).ReturnsAsync(tuple);
                var r = await sectionCoordinationService.GetInstructionalEvent4Async(0, 1, sectionGuid, "", "", "");
                var instrEvent = r.Item1.FirstOrDefault();
                Assert.AreEqual(instrEvent.Section.Id, sectionGuid);
            }

            [TestMethod]
            public async Task SectionService_StartDate_V11()
            {
                var sectionMeetings = new List<Domain.Student.Entities.SectionMeeting>();
                sectionMeetings.Add(meeting1);
                var tuple = new Tuple<IEnumerable<Domain.Student.Entities.SectionMeeting>, int>(sectionMeetings, 1);
                Dictionary<string, string> sectDict = new Dictionary<string, string>();
                sectDict.Add(meeting1.SectionId, sectionGuid);
                sectionRepoMock.Setup(sr => sr.GetSectionGuidsCollectionAsync(It.IsAny<string[]>())).ReturnsAsync(sectDict);


                sectionRepoMock.Setup(repo => repo.GetSectionMeeting2Async(0, 1, "", new DateTime(2012, 01, 01, 0, 0, 0, DateTimeKind.Local).ToShortDateString(), "", "", "", new List<string>(), new List<string>(), new List<string>(), "")).ReturnsAsync(tuple);
                sectionRepoMock.Setup(repo => repo.GetUnidataFormattedDate(new DateTime(2012, 01, 01, 0, 0, 0, DateTimeKind.Local).ToShortDateString())).ReturnsAsync(new DateTime(2012, 01, 01).ToShortDateString());
                var r = await sectionCoordinationService.GetInstructionalEvent4Async(0, 1, "", new DateTime(2012, 01, 01, 0, 0, 0, DateTimeKind.Local).ToShortDateString(), "", "");
                var instrEvent = r.Item1.FirstOrDefault();
                Assert.AreEqual(instrEvent.Recurrence.TimePeriod.StartOn.Value.Date.ToShortDateString(), new DateTime(2012, 01, 01, 0, 0, 0, DateTimeKind.Local).ToShortDateString());
            }

            [TestMethod]
            public async Task SectionService_EndDate_V11()
            {
                var sectionMeetings = new List<Domain.Student.Entities.SectionMeeting>();
                sectionMeetings.Add(meeting1);

                Dictionary<string, string> sectDict = new Dictionary<string, string>();
                sectDict.Add(meeting1.SectionId, Guid.NewGuid().ToString());
                sectionRepoMock.Setup(sr => sr.GetSectionGuidsCollectionAsync(It.IsAny<string[]>())).ReturnsAsync(sectDict);


                var tuple = new Tuple<IEnumerable<Domain.Student.Entities.SectionMeeting>, int>(sectionMeetings, 1);
                sectionRepoMock.Setup(repo => repo.GetSectionMeeting2Async(0, 1, "", "", new DateTime(2012, 12, 21, 0, 0, 0, DateTimeKind.Local).ToShortDateString(), "", "", new List<string>(), new List<string>(), new List<string>(), "")).ReturnsAsync(tuple);
                sectionRepoMock.Setup(repo => repo.GetUnidataFormattedDate(new DateTime(2012, 12, 21, 0, 0, 0, DateTimeKind.Local).ToShortDateString())).ReturnsAsync(new DateTime(2012, 12, 21, 0, 0, 0, DateTimeKind.Local).ToShortDateString());
                var r = await sectionCoordinationService.GetInstructionalEvent4Async(0, 1, "", "", new DateTime(2012, 12, 21, 0, 0, 0, DateTimeKind.Local).ToShortDateString(), "");
                var instrEvent = r.Item1.FirstOrDefault();
                Assert.AreEqual(instrEvent.Recurrence.TimePeriod.EndOn.Value.Date.ToShortDateString(), new DateTime(2012, 12, 21, 0, 0, 0, DateTimeKind.Local).ToShortDateString());
            }

            [TestMethod]
            public async Task SectionService_StartTime_V11()
            {
                var sectionMeetings = new List<Domain.Student.Entities.SectionMeeting>();
                sectionMeetings.Add(meeting1);
                Dictionary<string, string> sectDict = new Dictionary<string, string>();
                sectDict.Add(meeting1.SectionId, Guid.NewGuid().ToString());
                sectionRepoMock.Setup(sr => sr.GetSectionGuidsCollectionAsync(It.IsAny<string[]>())).ReturnsAsync(sectDict);


                var tuple = new Tuple<IEnumerable<Domain.Student.Entities.SectionMeeting>, int>(sectionMeetings, 1);
                sectionRepoMock.Setup(repo => repo.GetSectionMeeting2Async(0, 1, "", new DateTime(2012, 01, 01, 0, 0, 0, DateTimeKind.Local).ToShortDateString(), "", new DateTime(2012, 01, 01, 08, 30, 0, DateTimeKind.Local).ToLocalTime().TimeOfDay.ToString(), "", new List<string>(), new List<string>(), new List<string>(), "")).ReturnsAsync(tuple);
                sectionRepoMock.Setup(repo => repo.GetUnidataFormattedDate(new DateTime(2012, 01, 01, 08, 30, 0, DateTimeKind.Local).ToString("yyyy-MM-ddThh:mm:ss"))).ReturnsAsync(new DateTime(2012, 01, 01, 08, 30, 0, DateTimeKind.Local).ToShortDateString());
                var r = await sectionCoordinationService.GetInstructionalEvent4Async(0, 1, "", new DateTime(2012, 01, 01, 08, 30, 0, DateTimeKind.Local).ToString("yyyy-MM-ddThh:mm:ss"), "", "");
                var instrEvent = r.Item1.FirstOrDefault();
                Assert.AreEqual(instrEvent.Recurrence.TimePeriod.StartOn.Value.TimeOfDay.ToString(), new DateTime(2012, 01, 01, 08, 30, 0, DateTimeKind.Local).ToLocalTime().TimeOfDay.ToString());
            }

            [TestMethod]
            public async Task SectionService_EndTime_V11()
            {
                var sectionMeetings = new List<Domain.Student.Entities.SectionMeeting>();
                sectionMeetings.Add(meeting1);
                Dictionary<string, string> sectDict = new Dictionary<string, string>();
                sectDict.Add(meeting1.SectionId, Guid.NewGuid().ToString());
                sectionRepoMock.Setup(sr => sr.GetSectionGuidsCollectionAsync(It.IsAny<string[]>())).ReturnsAsync(sectDict);


                var tuple = new Tuple<IEnumerable<Domain.Student.Entities.SectionMeeting>, int>(sectionMeetings, 1);
                sectionRepoMock.Setup(repo => repo.GetSectionMeeting2Async(0, 1, "", "", new DateTime(2012, 12, 21, 0, 0, 0, DateTimeKind.Local).ToShortDateString(), "", new DateTime(2012, 12, 21, 09, 30, 0, DateTimeKind.Local).ToLocalTime().TimeOfDay.ToString(), new List<string>(), new List<string>(), new List<string>(), "")).ReturnsAsync(tuple);
                sectionRepoMock.Setup(repo => repo.GetUnidataFormattedDate(new DateTime(2012, 12, 21, 09, 30, 0, DateTimeKind.Local).ToString("yyyy-MM-ddThh:mm:ss"))).ReturnsAsync(new DateTime(2012, 12, 21, 0, 0, 0, DateTimeKind.Local).ToShortDateString());
                var r = await sectionCoordinationService.GetInstructionalEvent4Async(0, 1, "", "", new DateTime(2012, 12, 21, 09, 30, 0, DateTimeKind.Local).ToString("yyyy-MM-ddThh:mm:ss"), "");
                var instrEvent = r.Item1.FirstOrDefault();
                Assert.AreEqual(instrEvent.Recurrence.TimePeriod.EndOn.Value.TimeOfDay.ToString(), new DateTime(2012, 12, 21, 09, 30, 0, DateTimeKind.Local).ToLocalTime().TimeOfDay.ToString());
            }

            [TestMethod]
            public async Task SectionService_AcademicPeriod_V11()
            {
                var sectionMeetings = new List<Domain.Student.Entities.SectionMeeting>();
                sectionMeetings.Add(meeting1);

                Dictionary<string, string> sectDict = new Dictionary<string, string>();
                sectDict.Add(meeting1.SectionId, Guid.NewGuid().ToString());
                sectionRepoMock.Setup(sr => sr.GetSectionGuidsCollectionAsync(It.IsAny<string[]>())).ReturnsAsync(sectDict);


                var tuple = new Tuple<IEnumerable<Domain.Student.Entities.SectionMeeting>, int>(sectionMeetings, 1);
                sectionRepoMock.Setup(repo => repo.GetSectionMeeting2Async(0, 1, "", "", "", "", "", new List<string>(), new List<string>(), new List<string>(), "2012/FA")).ReturnsAsync(tuple);
                var r = await sectionCoordinationService.GetInstructionalEvent4Async(0, 1, "", "", "", acadPeriodsEntities.FirstOrDefault().Guid);
                var instrEvent = r.Item1.FirstOrDefault();
                Assert.AreEqual(instrEvent.Id, meeting1.Guid);
            }
        }

        [TestClass]
        public class SectionsMaximum_V6 : CurrentUserSetup
        {
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<ISectionRepository> sectionRepoMock;
            private Mock<IStudentRepository> studentRepoMock;
            private Mock<IStudentReferenceDataRepository> stuRefDataRepoMock;
            private Mock<IReferenceDataRepository> referenceDataRepoMock;
            private Mock<ICourseRepository> courseRepoMock;
            private Mock<ITermRepository> termRepoMock;
            private Mock<IStudentConfigurationRepository> studentConfigRepoMock;
            private Mock<IConfigurationRepository> configRepoMock;
            private Mock<IPersonRepository> personRepoMock;
            private Mock<IRoomRepository> roomRepoMock;
            private Mock<IEventRepository> eventRepoMock;
            private Mock<IBookRepository> bookRepoMock;
            private Mock<IRoleRepository> roleRepoMock;
            private Mock<ILogger> loggerMock;
            private ICurrentUserFactory currentUserFactory;


            private SectionCoordinationService sectionCoordinationService;
            Tuple<IEnumerable<Domain.Student.Entities.Section>, int> sectionMaxEntitiesTuple;
            IEnumerable<Domain.Student.Entities.Section> sectionEntities;

            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course> courseEntities;
            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Subject> subjectEntities;
            IEnumerable<Domain.Student.Entities.CreditCategory> creditCategoryEntities;
            IEnumerable<Domain.Student.Entities.GradeScheme> gradeSchemeEntities;
            IEnumerable<Domain.Student.Entities.CourseLevel> courseLevelEntities;
            IEnumerable<Domain.Student.Entities.AcademicPeriod> acadPeriodsEntities;
            IEnumerable<Domain.Student.Entities.AcademicLevel> academicLevelEntities;
            IEnumerable<Domain.Base.Entities.InstructionalPlatform> instructionalPlatformEntities;
            IEnumerable<Domain.Base.Entities.Location> locationEntities;
            IEnumerable<Domain.Base.Entities.Department> departmentEntities;
            IEnumerable<Domain.Student.Entities.InstructionalMethod> instrMethodEntities;
            IEnumerable<Domain.Base.Entities.Room> roomEntities;

            Domain.Base.Entities.Person person;
            private Domain.Student.Entities.SectionMeeting meeting1;


            [TestInitialize]
            public void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                sectionRepoMock = new Mock<ISectionRepository>();
                courseRepoMock = new Mock<ICourseRepository>();
                studentRepoMock = new Mock<IStudentRepository>();
                stuRefDataRepoMock = new Mock<IStudentReferenceDataRepository>();
                referenceDataRepoMock = new Mock<IReferenceDataRepository>();
                termRepoMock = new Mock<ITermRepository>();
                studentConfigRepoMock = new Mock<IStudentConfigurationRepository>();
                configRepoMock = new Mock<IConfigurationRepository>();
                personRepoMock = new Mock<IPersonRepository>();
                roomRepoMock = new Mock<IRoomRepository>();
                eventRepoMock = new Mock<IEventRepository>();
                bookRepoMock = new Mock<IBookRepository>();
                roleRepoMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();

                currentUserFactory = new CurrentUserSetup.StudentUserFactory();
                BuildData();

                sectionCoordinationService = new SectionCoordinationService(adapterRegistryMock.Object, sectionRepoMock.Object, courseRepoMock.Object, studentRepoMock.Object,
                    stuRefDataRepoMock.Object, referenceDataRepoMock.Object, termRepoMock.Object, studentConfigRepoMock.Object, configRepoMock.Object, personRepoMock.Object,
                    roomRepoMock.Object, eventRepoMock.Object, bookRepoMock.Object, currentUserFactory, roleRepoMock.Object, loggerMock.Object);

            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                sectionRepoMock = null;
                courseRepoMock = null;
                studentRepoMock = null;
                stuRefDataRepoMock = null;
                referenceDataRepoMock = null;
                termRepoMock = null;
                studentConfigRepoMock = null;
                configRepoMock = null;
                personRepoMock = null;
                roomRepoMock = null;
                eventRepoMock = null;
                roleRepoMock = null;
                sectionCoordinationService = null;
                courseEntities = null;
                subjectEntities = null;
                creditCategoryEntities = null;
                gradeSchemeEntities = null;
                courseLevelEntities = null;
                acadPeriodsEntities = null;
                academicLevelEntities = null;
                instructionalPlatformEntities = null;
                locationEntities = null;
                departmentEntities = null;
                instrMethodEntities = null;
                roomEntities = null;
                person = null;
                meeting1 = null;
            }

            [TestMethod]
            public async Task SectionCoordinationServiceTests_V6_GetSectionsMaximum2Async_ValidateAllFilters()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionsAsync(0, 3, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(sectionMaxEntitiesTuple);

                var results = await sectionCoordinationService.GetSectionsMaximum2Async(0, 3, It.IsAny<string>(), "2016-01-01", "2016-10-01", It.IsAny<string>(), It.IsAny<string>(), "840e72f0-57b9-42a2-ae88-df3c2262fbbc", "8d3fcd4d-cec8-4405-90eb-948392bd0a7e",
                    "558ca14c-718a-4b6e-8d92-77f498034f9f", "38b4330c-befa-435e-81d5-c3ffd52759f2", "b0eba383-5acf-4050-949d-8bb7a17c5012", "open", "1a2e2906-d46b-4698-80f6-af87b8083c64");

                Assert.IsNotNull(results);
                Assert.AreEqual(0, results.Item1.Count());
            }

            [TestMethod]
            public async Task SectionCoordinationServiceTests_V6_GetSectionMaximumByGuid2Async_CE()
            {
                sectionEntities = new List<Domain.Student.Entities.Section>()
                {
                    new Domain.Student.Entities.Section("1", "180", "111", new DateTime(2016, 01, 01), 3, 3, "Title 1", "C", new Collection<Domain.Student.Entities.OfferingDepartment>(){ new Domain.Student.Entities.OfferingDepartment("MATH") },
                        new Collection<string>(){ "100", "200"}, "UG", new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddYears(-6)) })
                        {
                            Guid = "0b983700-29eb-46ff-8616-21a8c3a48a0c",
                            LearningProvider = "CE",
                            TermId = "2012/FA",
                            Location = "MAIN",
                            NumberOfWeeks = 2,
                            NumberOnWaitlist = 10,
                            GradeSchemeCode = "UG"
                        }
                };
                sectionMaxEntitiesTuple = new Tuple<IEnumerable<Section>, int>(sectionEntities, sectionEntities.Count());
                sectionRepoMock.Setup(repo => repo.GetSectionByGuidAsync(It.IsAny<string>())).ReturnsAsync(sectionEntities.ToList()[0]);

                var result = await sectionCoordinationService.GetSectionMaximumByGuid2Async("0b983700-29eb-46ff-8616-21a8c3a48a0c");

                Assert.IsNotNull(result);
            }

            [TestMethod]
            public async Task SectionCoordinationServiceTests_V6_GetSectionMaximumByGuid2Async_Transfer()
            {
                sectionEntities = new List<Domain.Student.Entities.Section>()
                {
                    new Domain.Student.Entities.Section("1", "180", "111", new DateTime(2016, 01, 01), 3, 3, "Title 1", "T", new Collection<Domain.Student.Entities.OfferingDepartment>(){ new Domain.Student.Entities.OfferingDepartment("MATH") },
                        new Collection<string>(){ "100", "200"}, "UG", new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddYears(-6)) })
                        {
                            Guid = "0b983700-29eb-46ff-8616-21a8c3a48a0c",
                            LearningProvider = "CE",
                            TermId = "2012/FA",
                            Location = "MAIN",
                            NumberOfWeeks = 2,
                            NumberOnWaitlist = 10,
                            GradeSchemeCode = "UG"
                        }
                };
                sectionMaxEntitiesTuple = new Tuple<IEnumerable<Section>, int>(sectionEntities, sectionEntities.Count());
                sectionRepoMock.Setup(repo => repo.GetSectionByGuidAsync(It.IsAny<string>())).ReturnsAsync(sectionEntities.ToList()[0]);

                var result = await sectionCoordinationService.GetSectionMaximumByGuid2Async("0b983700-29eb-46ff-8616-21a8c3a48a0c");

                Assert.IsNotNull(result);
            }

            [TestMethod]
            public async Task SectionCoordinationServiceTests_V6_GetSectionMaximumByGuid2Async_Exchange()
            {
                sectionEntities = new List<Domain.Student.Entities.Section>()
                {
                    new Domain.Student.Entities.Section("1", "180", "111", new DateTime(2016, 01, 01), 3, 3, "Title 1", "E", new Collection<Domain.Student.Entities.OfferingDepartment>(){ new Domain.Student.Entities.OfferingDepartment("MATH") },
                        new Collection<string>(){ "100", "200"}, "UG", new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddYears(-6)) })
                        {
                            Guid = "0b983700-29eb-46ff-8616-21a8c3a48a0c",
                            LearningProvider = "CE",
                            TermId = "2012/FA",
                            Location = "MAIN",
                            NumberOfWeeks = 2,
                            NumberOnWaitlist = 10,
                            GradeSchemeCode = "UG"
                        }
                };
                sectionMaxEntitiesTuple = new Tuple<IEnumerable<Section>, int>(sectionEntities, sectionEntities.Count());
                sectionRepoMock.Setup(repo => repo.GetSectionByGuidAsync(It.IsAny<string>())).ReturnsAsync(sectionEntities.ToList()[0]);

                var result = await sectionCoordinationService.GetSectionMaximumByGuid2Async("0b983700-29eb-46ff-8616-21a8c3a48a0c");

                Assert.IsNotNull(result);
            }

            [TestMethod]
            public async Task SectionCoordinationServiceTests_V6_GetSectionMaximumByGuid2Async_Other()
            {
                sectionEntities = new List<Domain.Student.Entities.Section>()
                {
                    new Domain.Student.Entities.Section("1", "180", "111", new DateTime(2016, 01, 01), 3, 3, "Title 1", "O", new Collection<Domain.Student.Entities.OfferingDepartment>(){ new Domain.Student.Entities.OfferingDepartment("MATH") },
                        new Collection<string>(){ "100", "200"}, "UG", new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddYears(-6)) })
                        {
                            Guid = "0b983700-29eb-46ff-8616-21a8c3a48a0c",
                            LearningProvider = "CE",
                            TermId = "2012/FA",
                            Location = "MAIN",
                            NumberOfWeeks = 2,
                            NumberOnWaitlist = 10,
                            GradeSchemeCode = "UG"
                        }
                };
                sectionMaxEntitiesTuple = new Tuple<IEnumerable<Section>, int>(sectionEntities, sectionEntities.Count());
                sectionRepoMock.Setup(repo => repo.GetSectionByGuidAsync(It.IsAny<string>())).ReturnsAsync(sectionEntities.ToList()[0]);

                var result = await sectionCoordinationService.GetSectionMaximumByGuid2Async("0b983700-29eb-46ff-8616-21a8c3a48a0c");

                Assert.IsNotNull(result);
            }

            [TestMethod]
            public async Task SectionCoordinationServiceTests_V6_GetSectionMaximumByGuid2Async_NoCredit()
            {
                sectionEntities = new List<Domain.Student.Entities.Section>()
                {
                    new Domain.Student.Entities.Section("1", "180", "111", new DateTime(2016, 01, 01), 3, 3, "Title 1", "N", new Collection<Domain.Student.Entities.OfferingDepartment>(){ new Domain.Student.Entities.OfferingDepartment("MATH") },
                        new Collection<string>(){ "100", "200"}, "UG", new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddYears(-6)) })
                        {
                            Guid = "0b983700-29eb-46ff-8616-21a8c3a48a0c",
                            LearningProvider = "CE",
                            TermId = "2012/FA",
                            Location = "MAIN",
                            NumberOfWeeks = 2,
                            NumberOnWaitlist = 10,
                            GradeSchemeCode = "UG"
                        }
                };
                sectionMaxEntitiesTuple = new Tuple<IEnumerable<Section>, int>(sectionEntities, sectionEntities.Count());
                sectionRepoMock.Setup(repo => repo.GetSectionByGuidAsync(It.IsAny<string>())).ReturnsAsync(sectionEntities.ToList()[0]);

                var result = await sectionCoordinationService.GetSectionMaximumByGuid2Async("0b983700-29eb-46ff-8616-21a8c3a48a0c");

                Assert.IsNotNull(result);
            }

            [TestMethod]
            public async Task SectionCoordinationServiceTests_V6_GetSectionMaximumByGuid2Async_WithId_CreditTypeCode_Blank()
            {
                sectionEntities = new List<Domain.Student.Entities.Section>()
                {
                    new Domain.Student.Entities.Section("1", "180", "111", new DateTime(2016, 01, 01), 3, 3, "Title 1", " ", new Collection<Domain.Student.Entities.OfferingDepartment>(){ new Domain.Student.Entities.OfferingDepartment("MATH") },
                        new Collection<string>(){ "100", "200"}, "UG", new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddYears(-6)) })
                        {
                            Guid = "0b983700-29eb-46ff-8616-21a8c3a48a0c",
                            LearningProvider = "CE",
                            TermId = "2012/FA",
                            Location = "MAIN",
                            NumberOfWeeks = 2,
                            NumberOnWaitlist = 10,
                            GradeSchemeCode = "UG"
                        }
                };
                sectionMaxEntitiesTuple = new Tuple<IEnumerable<Section>, int>(sectionEntities, sectionEntities.Count());
                sectionRepoMock.Setup(repo => repo.GetSectionByGuidAsync(It.IsAny<string>())).ReturnsAsync(sectionEntities.ToList()[0]);

                var result = await sectionCoordinationService.GetSectionMaximumByGuid2Async("0b983700-29eb-46ff-8616-21a8c3a48a0c");

                Assert.IsNotNull(result);
            }

            [TestMethod]
            public async Task SectionCoordinationServiceTests_V6_GetSectionMaximumByGuid2Async_WithId_No_CEUS()
            {
                sectionEntities = new List<Domain.Student.Entities.Section>()
                {
                    new Domain.Student.Entities.Section("1", "180", "111", new DateTime(2016, 01, 01), 3, null, "Title 1", "I", new Collection<Domain.Student.Entities.OfferingDepartment>(){ new Domain.Student.Entities.OfferingDepartment("MATH") },
                        new Collection<string>(){ "100", "200"}, "UG", new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddYears(-6)) })
                        {
                            Guid = "0b983700-29eb-46ff-8616-21a8c3a48a0c",
                            LearningProvider = "CE",
                            TermId = "2012/FA",
                            Location = "MAIN",
                            NumberOfWeeks = 2,
                            NumberOnWaitlist = 10,
                            GradeSchemeCode = "UG"
                        }
                };
                sectionMaxEntitiesTuple = new Tuple<IEnumerable<Section>, int>(sectionEntities, sectionEntities.Count());
                sectionRepoMock.Setup(repo => repo.GetSectionByGuidAsync(It.IsAny<string>())).ReturnsAsync(sectionEntities.ToList()[0]);

                var result = await sectionCoordinationService.GetSectionMaximumByGuid2Async("0b983700-29eb-46ff-8616-21a8c3a48a0c");

                Assert.IsNotNull(result);
            }

            [TestMethod]
            public async Task SectionCoordinationServiceTests_V6_GetSectionMaximumByGuid2Async_WithId_Freq_Daily()
            {
                var sectionFaculty = new Domain.Student.Entities.SectionFaculty("1234", "SEC1", "0000678", "LG", new DateTime(2012, 09, 01), new DateTime(2011, 12, 21), 100.0m);

                meeting1 = new Domain.Student.Entities.SectionMeeting("123", "SEC1", "LG", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "D");
                meeting1.Guid = "1f1485cb-2705-4b59-9f51-7aa85c79b377";
                meeting1.AddFacultyId("0000678");
                meeting1.AddSectionFaculty(sectionFaculty);
                meeting1.StartTime = new DateTimeOffset(2012, 01, 01, 08, 30, 00, new TimeSpan());
                meeting1.EndTime = new DateTimeOffset(2012, 12, 21, 09, 30, 00, new TimeSpan());
                meeting1.Room = "COE*Room 1";
                var sectionMeetings = new List<Domain.Student.Entities.SectionMeeting>();
                sectionMeetings.Add(meeting1);
                var tuple = new Tuple<IEnumerable<Domain.Student.Entities.SectionMeeting>, int>(sectionMeetings, 1);
                sectionRepoMock.Setup(repo => repo.GetSectionMeetingAsync(0, 0, "1", "", "", "", "", new List<string>(), new List<string>(), new List<string>(), "")).ReturnsAsync(tuple);

                sectionRepoMock.Setup(repo => repo.GetSectionByGuidAsync(It.IsAny<string>())).ReturnsAsync(sectionEntities.ToList()[0]);

                var result = await sectionCoordinationService.GetSectionMaximumByGuid2Async("0b983700-29eb-46ff-8616-21a8c3a48a0c");

                Assert.IsNotNull(result);
                Assert.AreEqual(Dtos.FrequencyType2.Daily, result.InstructionalEvents.ToList()[0].Recurrence.RepeatRule.Type);
            }

            [TestMethod]
            public async Task SectionCoordinationServiceTests_V6_GetSectionMaximumByGuid2Async_WithId_Freq_Monthly_WithDays()
            {
                var sectionFaculty = new Domain.Student.Entities.SectionFaculty("1234", "SEC1", "0000678", "LG", new DateTime(2012, 09, 01), new DateTime(2011, 12, 21), 100.0m);

                meeting1 = new Domain.Student.Entities.SectionMeeting("123", "SEC1", "LG", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "M");
                meeting1.Days = new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday };
                meeting1.Guid = "1f1485cb-2705-4b59-9f51-7aa85c79b377";
                meeting1.AddFacultyId("0000678");
                meeting1.AddSectionFaculty(sectionFaculty);
                meeting1.StartTime = new DateTimeOffset(2012, 01, 01, 08, 30, 00, new TimeSpan());
                meeting1.EndTime = new DateTimeOffset(2012, 12, 21, 09, 30, 00, new TimeSpan());
                meeting1.Room = "COE*Room 1";
                var sectionMeetings = new List<Domain.Student.Entities.SectionMeeting>();
                sectionMeetings.Add(meeting1);
                var tuple = new Tuple<IEnumerable<Domain.Student.Entities.SectionMeeting>, int>(sectionMeetings, 1);
                sectionRepoMock.Setup(repo => repo.GetSectionMeetingAsync(0, 0, "1", "", "", "", "", new List<string>(), new List<string>(), new List<string>(), "")).ReturnsAsync(tuple);

                sectionRepoMock.Setup(repo => repo.GetSectionByGuidAsync(It.IsAny<string>())).ReturnsAsync(sectionEntities.ToList()[0]);

                var result = await sectionCoordinationService.GetSectionMaximumByGuid2Async("0b983700-29eb-46ff-8616-21a8c3a48a0c");

                Assert.IsNotNull(result);
                Assert.AreEqual(Dtos.FrequencyType2.Monthly, result.InstructionalEvents.ToList()[0].Recurrence.RepeatRule.Type);
            }

            [TestMethod]
            public async Task SectionCoordinationServiceTests_V6_GetSectionMaximumByGuid2Async_WithId_Freq_Monthly_WithoutDays()
            {
                var sectionFaculty = new Domain.Student.Entities.SectionFaculty("1234", "SEC1", "0000678", "LG", new DateTime(2012, 09, 01), new DateTime(2011, 12, 21), 100.0m);

                meeting1 = new Domain.Student.Entities.SectionMeeting("123", "SEC1", "LG", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "M");
                meeting1.Guid = "1f1485cb-2705-4b59-9f51-7aa85c79b377";
                meeting1.AddFacultyId("0000678");
                meeting1.AddSectionFaculty(sectionFaculty);
                meeting1.StartTime = new DateTimeOffset(2012, 01, 01, 08, 30, 00, new TimeSpan());
                meeting1.EndTime = new DateTimeOffset(2012, 12, 21, 09, 30, 00, new TimeSpan());
                meeting1.Room = "COE*Room 1";
                var sectionMeetings = new List<Domain.Student.Entities.SectionMeeting>();
                sectionMeetings.Add(meeting1);
                var tuple = new Tuple<IEnumerable<Domain.Student.Entities.SectionMeeting>, int>(sectionMeetings, 1);
                sectionRepoMock.Setup(repo => repo.GetSectionMeetingAsync(0, 0, "1", "", "", "", "", new List<string>(), new List<string>(), new List<string>(), "")).ReturnsAsync(tuple);

                sectionRepoMock.Setup(repo => repo.GetSectionByGuidAsync(It.IsAny<string>())).ReturnsAsync(sectionEntities.ToList()[0]);

                var result = await sectionCoordinationService.GetSectionMaximumByGuid2Async("0b983700-29eb-46ff-8616-21a8c3a48a0c");

                Assert.IsNotNull(result);
                Assert.AreEqual(Dtos.FrequencyType2.Monthly, result.InstructionalEvents.ToList()[0].Recurrence.RepeatRule.Type);
            }

            [TestMethod]
            public async Task SectionCoordinationServiceTests_V6_GetSectionMaximumByGuid2Async_WithId_Freq_Yearly()
            {
                var sectionFaculty = new Domain.Student.Entities.SectionFaculty("1234", "SEC1", "0000678", "LG", new DateTime(2012, 09, 01), new DateTime(2011, 12, 21), 100.0m);

                meeting1 = new Domain.Student.Entities.SectionMeeting("123", "SEC1", "LG", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "Y");
                meeting1.Guid = "1f1485cb-2705-4b59-9f51-7aa85c79b377";
                meeting1.AddFacultyId("0000678");
                meeting1.AddSectionFaculty(sectionFaculty);
                meeting1.StartTime = new DateTimeOffset(2012, 01, 01, 08, 30, 00, new TimeSpan());
                meeting1.EndTime = new DateTimeOffset(2012, 12, 21, 09, 30, 00, new TimeSpan());
                meeting1.Room = "COE*Room 1";
                var sectionMeetings = new List<Domain.Student.Entities.SectionMeeting>();
                sectionMeetings.Add(meeting1);
                var tuple = new Tuple<IEnumerable<Domain.Student.Entities.SectionMeeting>, int>(sectionMeetings, 1);
                sectionRepoMock.Setup(repo => repo.GetSectionMeetingAsync(0, 0, "1", "", "", "", "", new List<string>(), new List<string>(), new List<string>(), "")).ReturnsAsync(tuple);

                sectionRepoMock.Setup(repo => repo.GetSectionByGuidAsync(It.IsAny<string>())).ReturnsAsync(sectionEntities.ToList()[0]);

                var result = await sectionCoordinationService.GetSectionMaximumByGuid2Async("0b983700-29eb-46ff-8616-21a8c3a48a0c");

                Assert.IsNotNull(result);
                Assert.AreEqual(Dtos.FrequencyType2.Yearly, result.InstructionalEvents.ToList()[0].Recurrence.RepeatRule.Type);
            }

            [TestMethod]
            public async Task SectionCoordinationServiceTests_V6_GetSectionMaximumByGuid2Async_WithId()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionByGuidAsync(It.IsAny<string>())).ReturnsAsync(sectionEntities.ToList()[0]);

                var result = await sectionCoordinationService.GetSectionMaximumByGuid2Async("0b983700-29eb-46ff-8616-21a8c3a48a0c");

                Assert.IsNotNull(result);
            }

            #region All Exceptions

            [TestMethod]
            public async Task SectionCoordinationServiceTests_V6_GetSectionsMaximum2Async_NullSectionGuid_ArgumentException()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionsAsync(0, 3, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(sectionMaxEntitiesTuple);
                sectionRepoMock.Setup(repo => repo.GetSectionGuidFromIdAsync(It.IsAny<string>())).ThrowsAsync(new ArgumentException());

                var results = await sectionCoordinationService.GetSectionsMaximum2Async(0, 3, It.IsAny<string>(), "2016-01-01", "2016-10-01", It.IsAny<string>(), It.IsAny<string>(), "840e72f0-57b9-42a2-ae88-df3c2262fbbc", "8d3fcd4d-cec8-4405-90eb-948392bd0a7e",
                    "558ca14c-718a-4b6e-8d92-77f498034f9f", "38b4330c-befa-435e-81d5-c3ffd52759f2", "b0eba383-5acf-4050-949d-8bb7a17c5012", "open", "1a2e2906-d46b-4698-80f6-af87b8083c64");
                Assert.AreEqual(0, results.Item2);
            }

            [TestMethod]
            public async Task SectionCoordinationServiceTests_V6_GetSectionsMaximum2Async_NullInstrMethodGuid_ArgumentException()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionsAsync(0, 3, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(sectionMaxEntitiesTuple);
                stuRefDataRepoMock.Setup(repo => repo.GetInstructionalMethodsAsync(It.IsAny<bool>())).ThrowsAsync(new ArgumentException());

                var results = await sectionCoordinationService.GetSectionsMaximum2Async(0, 3, It.IsAny<string>(), "2016-01-01", "2016-10-01", It.IsAny<string>(), It.IsAny<string>(), "840e72f0-57b9-42a2-ae88-df3c2262fbbc", "8d3fcd4d-cec8-4405-90eb-948392bd0a7e",
                    "558ca14c-718a-4b6e-8d92-77f498034f9f", "38b4330c-befa-435e-81d5-c3ffd52759f2", "b0eba383-5acf-4050-949d-8bb7a17c5012", "open", "1a2e2906-d46b-4698-80f6-af87b8083c64");

                Assert.AreEqual(0, results.Item2);
            }

            [TestMethod]
            public async Task SectionCoordinationServiceTests_V6_GetSectionsMaximum2Async_RepeatCodeNull_ArgumentException()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionsAsync(0, 3, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(sectionMaxEntitiesTuple);

                meeting1.Frequency = "abc";

                var results = await sectionCoordinationService.GetSectionsMaximum2Async(0, 3, It.IsAny<string>(), "2016-01-01", "2016-10-01", It.IsAny<string>(), It.IsAny<string>(), "840e72f0-57b9-42a2-ae88-df3c2262fbbc", "8d3fcd4d-cec8-4405-90eb-948392bd0a7e",
                    "558ca14c-718a-4b6e-8d92-77f498034f9f", "38b4330c-befa-435e-81d5-c3ffd52759f2", "b0eba383-5acf-4050-949d-8bb7a17c5012", "open", "1a2e2906-d46b-4698-80f6-af87b8083c64");

                Assert.AreEqual(0, results.Item2);
            }

            [TestMethod]
            public async Task SectionCoordinationServiceTests_V6_GetSectionsMaximum2Async_GetPersonGuidFromIdAsync_Error_ArgumentException()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionsAsync(0, 3, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(sectionMaxEntitiesTuple);

                personRepoMock.Setup(repo => repo.GetPersonGuidFromIdAsync(It.IsAny<string>())).ThrowsAsync(new ArgumentException());

                var results = await sectionCoordinationService.GetSectionsMaximum2Async(0, 3, It.IsAny<string>(), "2016-01-01", "2016-10-01", It.IsAny<string>(), It.IsAny<string>(), "840e72f0-57b9-42a2-ae88-df3c2262fbbc", "8d3fcd4d-cec8-4405-90eb-948392bd0a7e",
                    "558ca14c-718a-4b6e-8d92-77f498034f9f", "38b4330c-befa-435e-81d5-c3ffd52759f2", "b0eba383-5acf-4050-949d-8bb7a17c5012", "open", "1a2e2906-d46b-4698-80f6-af87b8083c64");

                Assert.AreEqual(0, results.Item2);
            }

            [TestMethod]
            public async Task SectionCoordinationServiceTests_V6_GetSectionsMaximum2Async_ArgumentException()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionsAsync(0, 3, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(sectionMaxEntitiesTuple);
                sectionRepoMock.Setup(repo => repo.GetUnidataFormattedDate(It.IsAny<string>())).ThrowsAsync(new ArgumentException());

                var results = await sectionCoordinationService.GetSectionsMaximum2Async(0, 3, It.IsAny<string>(), "2016-01-01", "2016-10-01", It.IsAny<string>(), It.IsAny<string>(), "840e72f0-57b9-42a2-ae88-df3c2262fbbc", "8d3fcd4d-cec8-4405-90eb-948392bd0a7e",
                    "558ca14c-718a-4b6e-8d92-77f498034f9f", "38b4330c-befa-435e-81d5-c3ffd52759f2", "b0eba383-5acf-4050-949d-8bb7a17c5012", "open", "1a2e2906-d46b-4698-80f6-af87b8083c64");
                Assert.AreEqual(0, results.Item2);
            }

            [TestMethod]
            public async Task SectionCoordinationServiceTests_V6_GetSectionsMaximum2Async_NullInstrPlatList_ArgumentNullException()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionsAsync(0, 3, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(sectionMaxEntitiesTuple);
                referenceDataRepoMock.Setup(repo => repo.GetInstructionalPlatformsAsync(It.IsAny<bool>())).ReturnsAsync(() => null);

                var results = await sectionCoordinationService.GetSectionsMaximum2Async(0, 3, It.IsAny<string>(), "2016-01-01", "2016-10-01", It.IsAny<string>(), It.IsAny<string>(), "840e72f0-57b9-42a2-ae88-df3c2262fbbc", "8d3fcd4d-cec8-4405-90eb-948392bd0a7e",
                    "558ca14c-718a-4b6e-8d92-77f498034f9f", "38b4330c-befa-435e-81d5-c3ffd52759f2", "b0eba383-5acf-4050-949d-8bb7a17c5012", "open", "1a2e2906-d46b-4698-80f6-af87b8083c64");
                Assert.AreEqual(0, results.Item2);
            }

            [TestMethod]
            public async Task SectionCoordinationServiceTests_V6_GetSectionsMaximum2Async_EmptyCourseGuid_RepositoryException()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionsAsync(0, 3, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(sectionMaxEntitiesTuple);
                courseRepoMock.Setup(repo => repo.GetCourseGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(string.Empty);

                var results = await sectionCoordinationService.GetSectionsMaximum2Async(0, 3, It.IsAny<string>(), "2016-01-01", "2016-10-01", It.IsAny<string>(), It.IsAny<string>(), "840e72f0-57b9-42a2-ae88-df3c2262fbbc", "8d3fcd4d-cec8-4405-90eb-948392bd0a7e",
                    "558ca14c-718a-4b6e-8d92-77f498034f9f", "38b4330c-befa-435e-81d5-c3ffd52759f2", "b0eba383-5acf-4050-949d-8bb7a17c5012", "open", "1a2e2906-d46b-4698-80f6-af87b8083c64");

                Assert.AreEqual(0, results.Item2);
            }

            [TestMethod]
            public async Task SectionCoordinationServiceTests_V6_GetSectionsMaximum2Async_NullCourse_RepositoryException()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionsAsync(0, 3, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(sectionMaxEntitiesTuple);
                courseRepoMock.Setup(repo => repo.GetCourseByGuidAsync(It.IsAny<string>())).ReturnsAsync(() => null);

                var results = await sectionCoordinationService.GetSectionsMaximum2Async(0, 3, It.IsAny<string>(), "2016-01-01", "2016-10-01", It.IsAny<string>(), It.IsAny<string>(), "840e72f0-57b9-42a2-ae88-df3c2262fbbc", "8d3fcd4d-cec8-4405-90eb-948392bd0a7e",
                    "558ca14c-718a-4b6e-8d92-77f498034f9f", "38b4330c-befa-435e-81d5-c3ffd52759f2", "b0eba383-5acf-4050-949d-8bb7a17c5012", "open", "1a2e2906-d46b-4698-80f6-af87b8083c64");

                Assert.AreEqual(0, results.Item2);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionCoordinationServiceTests_V6_GetSectionMaximumByGuid2Async_WithNullId_ArgumentNullException()
            {
                var result = await sectionCoordinationService.GetSectionMaximumByGuid2Async("");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task SectionCoordinationServiceTests_V6_GetSectionMaximumByGuid2Async_WithNullEntity_KeyNotFoundException()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(() => null);

                var result = await sectionCoordinationService.GetSectionMaximumByGuid2Async("0b983700-29eb-46ff-8616-21a8c3a48a0c");
            }

            #endregion

            private void BuildData()
            {
                //Course entity
                courseEntities = new TestCourseRepository().GetAsync().Result.Take(41);
                var courseEntity = courseEntities.FirstOrDefault(i => i.Id.Equals("180"));
                courseRepoMock.Setup(repo => repo.GetCourseGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(courseEntity.Guid);
                courseRepoMock.Setup(repo => repo.GetCourseByGuidAsync(courseEntity.Guid)).ReturnsAsync(courseEntity);
                //Subject entity
                subjectEntities = new TestSubjectRepository().Get();
                stuRefDataRepoMock.Setup(repo => repo.GetSubjectsAsync()).ReturnsAsync(subjectEntities);
                var subjectEntity = subjectEntities.FirstOrDefault(s => s.Code.Equals(courseEntity.SubjectCode));
                //Credit Categories
                creditCategoryEntities = new List<Domain.Student.Entities.CreditCategory>()
                {
                    new Domain.Student.Entities.CreditCategory("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "I", "Institutional", Domain.Student.Entities.CreditType.Institutional),
                    new Domain.Student.Entities.CreditCategory("73244057-D1EC-4094-A0B7-DE602533E3A6", "C", "Continuing Education", Domain.Student.Entities.CreditType.ContinuingEducation),
                    new Domain.Student.Entities.CreditCategory("73244057-D1EC-4094-A0B7-DE602533E3A6", "E", "Exchange", Domain.Student.Entities.CreditType.Exchange),
                    new Domain.Student.Entities.CreditCategory("73244057-D1EC-4094-A0B7-DE602533E3A6", "O", "Other", Domain.Student.Entities.CreditType.Other),
                    new Domain.Student.Entities.CreditCategory("73244057-D1EC-4094-A0B7-DE602533E3A6", "N", "Continuing Education", Domain.Student.Entities.CreditType.None),
                    new Domain.Student.Entities.CreditCategory("1df164eb-8178-4321-a9f7-24f12d3991d8", "T", "Transfer Credit", Domain.Student.Entities.CreditType.Transfer)
                };
                stuRefDataRepoMock.Setup(repo => repo.GetCreditCategoriesAsync()).ReturnsAsync(creditCategoryEntities);
                //Grade Schemes
                gradeSchemeEntities = new TestStudentReferenceDataRepository().GetGradeSchemesAsync().Result as List<Ellucian.Colleague.Domain.Student.Entities.GradeScheme>;
                stuRefDataRepoMock.Setup(repo => repo.GetGradeSchemesAsync()).ReturnsAsync(gradeSchemeEntities);
                //Course Levels
                courseLevelEntities = new List<Domain.Student.Entities.CourseLevel>()
                {
                    new Domain.Student.Entities.CourseLevel("19f6e2cd-1e5f-4485-9b27-60d4f4e4b1ff", "100", "First Yr"),
                    new Domain.Student.Entities.CourseLevel("73244057-D1EC-4094-A0B7-DE602533E3A6", "200", "Second Year"),
                    new Domain.Student.Entities.CourseLevel("1df164eb-8178-4321-a9f7-24f12d3991d8", "300", "Third Year"),
                    new Domain.Student.Entities.CourseLevel("4aa187ae-6d22-4b10-a2e2-1304ebc18176", "400", "Third Year"),
                    new Domain.Student.Entities.CourseLevel("d9f42a0f-39de-44bc-87af-517619141bde", "500", "Third Year")
                };
                stuRefDataRepoMock.Setup(repo => repo.GetCourseLevelsAsync()).ReturnsAsync(courseLevelEntities);

                //IEnumerable<SectionMeeting>
                var sectionFaculty = new Domain.Student.Entities.SectionFaculty("1234", "SEC1", "0000678", "LG", new DateTime(2012, 09, 01), new DateTime(2011, 12, 21), 100.0m);

                meeting1 = new Domain.Student.Entities.SectionMeeting("123", "SEC1", "LG", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "W");
                meeting1.Guid = "1f1485cb-2705-4b59-9f51-7aa85c79b377";
                meeting1.AddFacultyId("0000678");
                meeting1.AddSectionFaculty(sectionFaculty);
                meeting1.StartTime = new DateTimeOffset(2012, 01, 01, 08, 30, 00, new TimeSpan());
                meeting1.EndTime = new DateTimeOffset(2012, 12, 21, 09, 30, 00, new TimeSpan());
                meeting1.Room = "COE*Room 1";
                var sectionMeetings = new List<Domain.Student.Entities.SectionMeeting>();
                sectionMeetings.Add(meeting1);
                var tuple = new Tuple<IEnumerable<Domain.Student.Entities.SectionMeeting>, int>(sectionMeetings, 1);
                sectionRepoMock.Setup(repo => repo.GetSectionMeetingAsync(0, 0, "1", "", "", "", "", new List<string>(), new List<string>(), new List<string>(), "")).ReturnsAsync(tuple);
                sectionRepoMock.Setup(repo => repo.GetSectionGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("ad7655d0-77e3-4f8a-a07c-5c86f6a6f86a");

                //Instructor Id
                personRepoMock.Setup(repo => repo.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("ebf585ad-cc1f-478f-a7a3-aefae87f873a");
                //Instructional Methods
                instrMethodEntities = new List<Domain.Student.Entities.InstructionalMethod>()
                {
                    new Domain.Student.Entities.InstructionalMethod("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "LG", "LG", false),
                    new Domain.Student.Entities.InstructionalMethod("73244057-D1EC-4094-A0B7-DE602533E3A6", "30", "30", true),
                    new Domain.Student.Entities.InstructionalMethod("1df164eb-8178-4321-a9f7-24f12d3991d8", "04", "04", false)
                };
                stuRefDataRepoMock.Setup(repo => repo.GetInstructionalMethodsAsync(It.IsAny<bool>())).ReturnsAsync(instrMethodEntities);
                //schedule repeats
                var scheduleRepeats = new List<Domain.Base.Entities.ScheduleRepeat>()
                {
                    new Domain.Base.Entities.ScheduleRepeat("W", "Weekly", "7", FrequencyType.Weekly),
                    new Domain.Base.Entities.ScheduleRepeat("D", "Daily", "1", FrequencyType.Daily),
                    new Domain.Base.Entities.ScheduleRepeat("M", "Monthly", "30", FrequencyType.Monthly),
                    new Domain.Base.Entities.ScheduleRepeat("Y", "Yearly", "365", FrequencyType.Yearly)
                };
                referenceDataRepoMock.Setup(repo => repo.ScheduleRepeats).Returns(scheduleRepeats);
                //Rooms entities
                roomEntities = new List<Domain.Base.Entities.Room>
                {
                    new Room("2ae6e009-40ca-4ac0-bb41-c123f7c344e3", "COE*Room 1", "COE")
                    {
                        Capacity = 50,
                        RoomType = "110",
                        Name = "Room 1"
                    },
                    new Room("8c92e963-5f05-45a2-8484-d9ad21e6ab47", "COE*0110", "CEE")
                    {
                        Capacity = 100,
                        RoomType = "111",
                        Name = "Room 2"
                    },
                    new Room("8fdbaec7-4198-4348-b95a-a48a357e67f5", "COE*0120", "CDF")
                    {
                        Capacity = 20,
                        RoomType = "111",
                        Name = "Room 13"
                    },
                    new Room("327a6856-0230-4a6d-82ed-5c99dc1b1862", "COE*0121", "CSD")
                    {
                        Capacity = 50,
                        RoomType = "111",
                        Name = "Room 112"
                    },
                    new Room("cc9aa34c-db5e-46dc-9e5b-ba3f4b2557a8", "EIN*0121", "BSF")
                    {
                        Capacity = 30,
                        RoomType = "111",
                        Name = "Room BSF"
                    }
                };
                roomRepoMock.Setup(repo => repo.RoomsAsync()).ReturnsAsync(roomEntities);
                //Person
                person = new Domain.Base.Entities.Person("1", "Brown")
                {
                    Guid = "96cf912f-5e87-4099-96bf-73baac4c7715",
                    Prefix = "Mr.",
                    FirstName = "Ricky",
                    MiddleName = "Lee",
                    Suffix = "Jr.",
                    Nickname = "Rick",
                    BirthDate = new DateTime(1930, 1, 1),
                    DeceasedDate = new DateTime(2014, 5, 12),
                    GovernmentId = "111-11-1111",
                    MaritalStatusCode = "M",
                    EthnicCodes = new List<string> { "H" },
                    RaceCodes = new List<string> { "AS" }
                };
                person.AddEmailAddress(new EmailAddress("xyz@xmail.com", "COL"));
                person.AddPersonAlt(new PersonAlt("1", Domain.Base.Entities.PersonAlt.ElevatePersonAltType.ToString()));

                personRepoMock.Setup(repo => repo.GetPersonByGuidNonCachedAsync(It.IsAny<string>())).ReturnsAsync(person);
                // Mock the reference repository for prefix
                referenceDataRepoMock.Setup(repo => repo.Prefixes).Returns(new List<Prefix>()
                {
                    new Prefix("MR","Mr","Mr."),
                    new Prefix("MS","Ms","Ms."),
                    new Prefix("JR","Jr","Jr."),
                    new Prefix("SR","Sr","Sr.")
                });

                // Mock the reference repository for suffix
                referenceDataRepoMock.Setup(repo => repo.Suffixes).Returns(new List<Suffix>()
                {
                    new Suffix("JR","Jr","Jr."),
                    new Suffix("SR","Sr","Sr.")
                });

                //Acad Periods
                acadPeriodsEntities = new List<Domain.Student.Entities.AcademicPeriod>()
                {
                    new Domain.Student.Entities.AcademicPeriod("8d3fcd4d-cec8-4405-90eb-948392bd0a7e", "2012/FA", "Fall 2014", DateTime.Today.AddDays(-60), DateTime.Today.AddDays(10), DateTime.Today.Year, 4, "Spring", "5f7e7071-5aef-4d22-891f-86ab472a9f15",
                        "edcfd1ee-4adf-46bc-8b87-8853ae49dbeb", null)
                };
                termRepoMock.Setup(repo => repo.GetAcademicPeriods(It.IsAny<IEnumerable<Term>>())).Returns(acadPeriodsEntities);

                academicLevelEntities = new TestAcademicLevelRepository().GetAsync().Result as List<Ellucian.Colleague.Domain.Student.Entities.AcademicLevel>;
                stuRefDataRepoMock.Setup(repo => repo.GetAcademicLevelsAsync()).ReturnsAsync(academicLevelEntities);

                locationEntities = new TestLocationRepository().Get();
                referenceDataRepoMock.Setup(repo => repo.Locations).Returns(locationEntities);

                departmentEntities = new TestDepartmentRepository().Get().ToList();
                referenceDataRepoMock.Setup(repo => repo.DepartmentsAsync()).ReturnsAsync(departmentEntities);

                instructionalPlatformEntities = new List<InstructionalPlatform>
                {
                    new InstructionalPlatform("840e72f0-57b9-42a2-ae88-df3c2262fbbc", "CE", "Continuing Education"),
                    new InstructionalPlatform("e986b8a5-25f3-4aa0-bd0e-90982865e749", "D", "Institutional"),
                    new InstructionalPlatform("b5cc288b-8692-474e-91be-bdc55778e2f5", "TR", "Transfer")
                };
                referenceDataRepoMock.Setup(repo => repo.GetInstructionalPlatformsAsync(It.IsAny<bool>())).ReturnsAsync(instructionalPlatformEntities);

                sectionRepoMock.Setup(repo => repo.GetUnidataFormattedDate(It.IsAny<string>())).ReturnsAsync("2016/01/01");
                sectionRepoMock.Setup(repo => repo.GetCourseIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
                sectionRepoMock.Setup(repo => repo.ConvertStatusToStatusCodeAsync(It.IsAny<string>())).ReturnsAsync("Open");

                sectionEntities = new List<Domain.Student.Entities.Section>()
                {
                    new Domain.Student.Entities.Section("1", "180", "111", new DateTime(2016, 01, 01), 3, 3, "Title 1", "I", new Collection<Domain.Student.Entities.OfferingDepartment>(){ new Domain.Student.Entities.OfferingDepartment("MATH") },
                        new Collection<string>(){ "100", "200"}, "UG", new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddYears(-6)) })
                        {
                            Guid = "0b983700-29eb-46ff-8616-21a8c3a48a0c",
                            LearningProvider = "CE",
                            TermId = "2012/FA",
                            Location = "MAIN",
                            NumberOfWeeks = 2,
                            NumberOnWaitlist = 10,
                            GradeSchemeCode = "UG"
                        }
                };
                sectionMaxEntitiesTuple = new Tuple<IEnumerable<Section>, int>(sectionEntities, sectionEntities.Count());
            }
        }

        [TestClass]
        public class Sections_Get_V6 : CurrentUserSetup
        {
            private SectionCoordinationService sectionCoordinationService;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IStudentReferenceDataRepository> stuRefDataRepoMock;
            private IStudentReferenceDataRepository studentReferenceDataRepo;
            private Mock<IReferenceDataRepository> referenceDataRepoMock;
            private IReferenceDataRepository referenceDataRepo;
            private Mock<ICourseRepository> courseRepoMock;
            private ICourseRepository courseRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IStudentConfigurationRepository> studentConfigRepoMock;
            private IStudentConfigurationRepository studentConfigRepo;
            private Mock<IConfigurationRepository> configRepoMock;
            private IConfigurationRepository configRepo;
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IRoomRepository> roomRepoMock;
            private IRoomRepository roomRepo;
            private Mock<IEventRepository> eventRepoMock;
            private IEventRepository eventRepo;
            private Mock<IBookRepository> bookRepoMock;
            private IBookRepository bookRepo;

            // private Mock<IAcademicPeriodRepository> academicPeriodRepoMock;
            // private IAcademicPeriodRepository academicPeriodRepo;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ILogger logger;
            private ICurrentUserFactory currentUserFactory;

            private Domain.Student.Entities.Section section1;
            private Domain.Student.Entities.Section section2;
            private Domain.Student.Entities.Section section3;
            private Domain.Student.Entities.Section section4;
            private Domain.Student.Entities.SectionMeeting meeting1;
            private Domain.Student.Entities.SectionMeeting meeting2;
            private Domain.Student.Entities.SectionMeeting meeting3;
            private Domain.Student.Entities.SectionMeeting meeting4;

            private List<OfferingDepartment> dpts = new List<OfferingDepartment>() { new OfferingDepartment("ART", 100m) };
            private List<string> levels = new List<string>() { "Whatever" };
            private List<SectionStatusItem> statuses = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-60)) };
            private List<SectionStatusItem> statuses2 = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Inactive, "I", DateTime.Today.AddDays(-60)) };
            private List<SectionStatusItem> statuses3 = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Cancelled, "C", DateTime.Today.AddDays(-60)) };
            private List<SectionStatusItem> statuses4 = new List<SectionStatusItem>() { new SectionStatusItem(new SectionStatus(), "N/A", DateTime.Today.AddDays(-60)) };
            private List<CreditCategory> creditCategories = new List<CreditCategory>() { new CreditCategory(Guid.NewGuid().ToString(), "IN", "Regular Credit", CreditType.Institutional) };
            private List<Domain.Student.Entities.AcademicLevel> academicLevels = new List<Domain.Student.Entities.AcademicLevel>() { new Domain.Student.Entities.AcademicLevel(Guid.NewGuid().ToString(), "UG", "Undergraduate") };
            private List<Domain.Student.Entities.GradeScheme> gradeSchemes = new List<Domain.Student.Entities.GradeScheme>() { new Domain.Student.Entities.GradeScheme(Guid.NewGuid().ToString(), "Pass/Fail", "Pass/Fail grade") };
            private List<Domain.Student.Entities.CourseLevel> courseLevels = new List<Domain.Student.Entities.CourseLevel>() { new Domain.Student.Entities.CourseLevel(Guid.NewGuid().ToString(), "100", "Introduction to terms, concepts, and techniques.") };
            private List<Department> departments = new List<Department>() { new Department(Guid.NewGuid().ToString(), "PHYS", "Physics", true) };
            private List<Location> locations = new List<Location>() { new Location(Guid.NewGuid().ToString(), "MD", "Maryland") };
            private List<AcademicPeriod> academicPeriods = new List<AcademicPeriod>() { new AcademicPeriod(Guid.NewGuid().ToString(), "2012/FA", "Fall 2014", DateTime.Today.AddDays(-60), DateTime.Today, 2015, 4, "Spring", Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), null) };
            private List<Domain.Base.Entities.InstructionalPlatform> instructionalPlatforms = new List<Domain.Base.Entities.InstructionalPlatform>() { new Domain.Base.Entities.InstructionalPlatform(Guid.NewGuid().ToString(), "Ellucian", "IP") };

            private string meeting1Guid = Guid.NewGuid().ToString();
            private string sectionGuid = Guid.NewGuid().ToString();
            private string roomGuid = Guid.NewGuid().ToString();
            private string courseGuid = Guid.NewGuid().ToString();

            [TestInitialize]
            public void Initialize()
            {
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                roomRepoMock = new Mock<IRoomRepository>();
                roomRepo = roomRepoMock.Object;
                stuRefDataRepoMock = new Mock<IStudentReferenceDataRepository>();
                studentReferenceDataRepo = stuRefDataRepoMock.Object;
                referenceDataRepoMock = new Mock<IReferenceDataRepository>();
                referenceDataRepo = referenceDataRepoMock.Object;
                courseRepoMock = new Mock<ICourseRepository>();
                courseRepo = courseRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                studentConfigRepoMock = new Mock<IStudentConfigurationRepository>();
                studentConfigRepo = studentConfigRepoMock.Object;
                configRepoMock = new Mock<IConfigurationRepository>();
                configRepo = configRepoMock.Object;
                eventRepoMock = new Mock<IEventRepository>();
                eventRepo = eventRepoMock.Object;
                bookRepoMock = new Mock<IBookRepository>();
                bookRepo = bookRepoMock.Object;

                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;

                // "STU1" is the current user
                currentUserFactory = new CurrentUserSetup.StudentUserFactory();

                logger = new Mock<ILogger>().Object;

                // Mock the section repo response
                section1 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC1", "1119", "01", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses, true);
                section1.Guid = sectionGuid;
                section1.TermId = "2012/FA";
                section1.EndDate = new DateTime(2012, 12, 21);
                section1.LearningProvider = "Ellucian";

                section2 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC2", "1119", "02", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses2, true);
                section2.Guid = Guid.NewGuid().ToString();
                section2.TermId = "2012/FA";

                section3 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC3", "1119", "03", new DateTime(2011, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses3, true);
                section3.Guid = Guid.NewGuid().ToString();
                section3.TermId = "2011/FA";
                section3.EndDate = new DateTime(2011, 12, 21);

                section4 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC4", "1119", "04", new DateTime(2011, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses4, true);
                section4.Guid = Guid.NewGuid().ToString();
                section4.TermId = "2011/FA";
                section4.EndDate = new DateTime(2011, 12, 21);

                var sectionFaculty = new Domain.Student.Entities.SectionFaculty("1234", "SEC1", "0000678", "LEC", new DateTime(2012, 09, 01), new DateTime(2011, 12, 21), 100.0m);

                meeting1 = new Domain.Student.Entities.SectionMeeting("123", "SEC1", "LEC", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "W");
                meeting1.Guid = meeting1Guid;
                meeting1.AddFacultyId("0000678");
                meeting1.AddSectionFaculty(sectionFaculty);
                meeting1.StartTime = new DateTimeOffset(2012, 01, 01, 08, 30, 00, new TimeSpan());
                meeting1.EndTime = new DateTimeOffset(2012, 12, 21, 09, 30, 00, new TimeSpan());
                meeting1.Room = "DAN*102";
                section1.AddSectionMeeting(meeting1);

                meeting2 = new Domain.Student.Entities.SectionMeeting("123", "SEC1", "LEC", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "D");
                meeting2.Guid = meeting1Guid;

                meeting3 = new Domain.Student.Entities.SectionMeeting("456", "SEC1", "LEC", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "M");
                meeting3.Guid = meeting1Guid;

                meeting4 = new Domain.Student.Entities.SectionMeeting("789", "SEC1", "LEC", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "Y");
                meeting4.Guid = meeting1Guid;

                var sections = new List<Domain.Student.Entities.Section>();
                //var section = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC1", "1119", "01", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses, true);
                sections.Add(section1);
                sections.Add(section2);
                sections.Add(section3);
                sections.Add(section4);

                var rooms = new List<Domain.Base.Entities.Room>();
                var room = new Domain.Base.Entities.Room(roomGuid, "DAN*102", "Danville Hall Room 102");
                rooms.Add(room);

                var scheduleRepeats = new List<Domain.Base.Entities.ScheduleRepeat>();
                var scheduleRepeat = new Domain.Base.Entities.ScheduleRepeat("W", "Weekly", "7", FrequencyType.Weekly);
                scheduleRepeats.Add(scheduleRepeat);
                scheduleRepeat = new Domain.Base.Entities.ScheduleRepeat("D", "Daily", "1", FrequencyType.Daily);
                scheduleRepeats.Add(scheduleRepeat);
                scheduleRepeat = new Domain.Base.Entities.ScheduleRepeat("M", "Monthly", "30", FrequencyType.Monthly);
                scheduleRepeats.Add(scheduleRepeat);
                scheduleRepeat = new Domain.Base.Entities.ScheduleRepeat("Y", "Yearly", "365", FrequencyType.Yearly);
                scheduleRepeats.Add(scheduleRepeat);

                List<string> sec1List = new List<string>() { "SEC1" };
                List<Domain.Student.Entities.Section> sec1Resp = new List<Domain.Student.Entities.Section>() { section1 };
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(sec1List, false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sec1Resp));

                List<string> sec2List = new List<string>() { "SEC2" };
                List<Domain.Student.Entities.Section> sec2Resp = new List<Domain.Student.Entities.Section>() { section2 };
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(sec2List, false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sec2Resp));

                List<string> sec3List = new List<string>() { "SEC3" };
                List<Domain.Student.Entities.Section> sec3Resp = new List<Domain.Student.Entities.Section>() { section3 };
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(sec3List, false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sec3Resp));

                List<string> sec4List = new List<string>() { "9999999" };
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(sec4List, false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(new List<Domain.Student.Entities.Section>()));

                sectionRepoMock.Setup(repo => repo.GetSectionByGuidAsync(section1.Guid)).ReturnsAsync(section1);
                sectionRepoMock.Setup(repo => repo.GetSectionByGuidAsync(section2.Guid)).ReturnsAsync(section2);
                sectionRepoMock.Setup(repo => repo.GetSectionByGuidAsync(section3.Guid)).ReturnsAsync(section3);
                sectionRepoMock.Setup(repo => repo.GetSectionByGuidAsync(section4.Guid)).ReturnsAsync(section4);
                courseRepoMock.Setup(repo => repo.GetCourseGuidFromIdAsync(section1.CourseId)).ReturnsAsync(section1.Title);
                courseRepoMock.Setup(repo => repo.GetCourseGuidFromIdAsync(section2.CourseId)).ReturnsAsync(section2.Title);
                courseRepoMock.Setup(repo => repo.GetCourseGuidFromIdAsync(section3.CourseId)).ReturnsAsync(section3.Title);
                courseRepoMock.Setup(repo => repo.GetCourseGuidFromIdAsync(section4.CourseId)).ReturnsAsync(section4.Title);
                stuRefDataRepoMock.Setup(repo => repo.GetCreditCategoriesAsync()).ReturnsAsync(creditCategories);
                stuRefDataRepoMock.Setup(repo => repo.GetAcademicLevelsAsync()).ReturnsAsync(academicLevels);
                stuRefDataRepoMock.Setup(repo => repo.GetGradeSchemesAsync()).ReturnsAsync(gradeSchemes);
                stuRefDataRepoMock.Setup(repo => repo.GetCourseLevelsAsync()).ReturnsAsync(courseLevels);
                referenceDataRepoMock.Setup(repo => repo.DepartmentsAsync()).ReturnsAsync(departments);
                referenceDataRepoMock.Setup(repo => repo.Locations).Returns(locations);
                termRepoMock.Setup(repo => repo.GetAcademicPeriods(It.IsAny<IEnumerable<Domain.Student.Entities.Term>>())).Returns(academicPeriods);
                referenceDataRepoMock.Setup(repo => repo.GetInstructionalPlatformsAsync(It.IsAny<bool>())).ReturnsAsync(instructionalPlatforms);

                sectionRepoMock.Setup(repo => repo.GetSectionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new Tuple<IEnumerable<Domain.Student.Entities.Section>, int>(sections, 100));

                //roomRepoMock.Setup(repo => repo.RoomsAsync()).ReturnsAsync(rooms);
                //referenceDataRepoMock.Setup(repo => repo.ScheduleRepeats).Returns(scheduleRepeats);

                // Mock the section service
                sectionCoordinationService = new SectionCoordinationService(adapterRegistry, sectionRepo, courseRepo, studentRepo, studentReferenceDataRepo,
                    referenceDataRepo, termRepo, studentConfigRepo, configRepo, personRepo, roomRepo, eventRepo, bookRepo, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionRepo = null;
                studentRepo = null;
                roleRepo = null;
                adapterRegistry = null;
                sectionCoordinationService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionService_SectionEmptyId_V6()
            {
                var r = await sectionCoordinationService.GetSection3ByGuidAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task SectionService_InvalidSectionId_V6()
            {
                var r = await sectionCoordinationService.GetSection3ByGuidAsync("9999999");
            }

            [TestMethod]
            public async Task SectionService_GetByGuid_V6()
            {
                var res = await sectionCoordinationService.GetSection3ByGuidAsync(section1.Guid);
                Assert.AreEqual(section1.Guid, res.Id);
            }

            [TestMethod]
            public async Task SectionService_GetAll_EmptyArgument_V6()
            {
                var res = await sectionCoordinationService.GetSections3Async(0, 5);
                var sec = res.Item1.Where(r => r.Id == section1.Guid).FirstOrDefault();
                Assert.AreEqual(section1.Guid, sec.Id);
            }

            [TestMethod]
            public async Task SectionService_GetAll_V6()
            {
                sectionRepoMock.Setup(repo => repo.GetUnidataFormattedDate("2014-09-01T00:00:00+00:00")).ReturnsAsync("2014-09-01T00:00:00+00:00");
                sectionRepoMock.Setup(repo => repo.GetUnidataFormattedDate("2015-08-31T23:59:59+00:00")).ReturnsAsync("2015-08-31T23:59:59+00:00");
                sectionRepoMock.Setup(repo => repo.GetCourseIdFromGuidAsync(section1.Guid)).ReturnsAsync(section1.CourseId);
                sectionRepoMock.Setup(repo => repo.ConvertStatusToStatusCodeAsync("open")).ReturnsAsync("'A'");
                var res = await sectionCoordinationService.GetSections3Async(0, 5, "",
                    "2014-09-01T00:00:00+00:00", "2015-08-31T23:59:59+00:00", "", "", "", "", "",
                    section1.Guid, "", "open", "");
                var sec = res.Item1.Where(r => r.Id == section1.Guid).FirstOrDefault();
                Assert.AreEqual(section1.Guid, sec.Id);
            }

            [TestMethod]
            public async Task SectionService_GetAll_WithInvalidDateFormat_V6()
            {
                sectionRepoMock.Setup(repo => repo.GetUnidataFormattedDate(It.IsAny<string>())).Throws<Exception>();
                var res = await sectionCoordinationService.GetSections3Async(0, 5, "",
                    "2014-09-01T00:00:00+00:00", "2015-08-31T23:59:59+00:00", "", "", "", "", "",
                    section1.Guid, "", "open", "");
                // Invalid filter value now returns empty set
                Assert.AreEqual(0, res.Item2);
            }
            [TestMethod]
            public async Task SectionService_GetByGuidWithInactiveStatus_V6()
            {
                var res = await sectionCoordinationService.GetSection3ByGuidAsync(section2.Guid);
                Assert.AreEqual(section2.Guid, res.Id);
            }

            [TestMethod]
            public async Task SectionService_GetByGuidWithCancelledStatus_V6()
            {
                var res = await sectionCoordinationService.GetSection3ByGuidAsync(section3.Guid);
                Assert.AreEqual(section3.Guid, res.Id);
            }
            [TestMethod]
            public async Task SectionService_GetByGuidWithNoStatus_V6()
            {
                var res = await sectionCoordinationService.GetSection3ByGuidAsync(section4.Guid);
                Assert.AreEqual(section4.Guid, res.Id);
            }
        }

        [TestClass]
        public class Sections_Post_V6 : CurrentUserSetup
        {
            private SectionCoordinationService sectionCoordinationService;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IStudentReferenceDataRepository> stuRefDataRepoMock;
            private IStudentReferenceDataRepository studentReferenceDataRepo;
            private Mock<IReferenceDataRepository> referenceDataRepoMock;
            private IReferenceDataRepository referenceDataRepo;
            private Mock<ICourseRepository> courseRepoMock;
            private ICourseRepository courseRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IStudentConfigurationRepository> studentConfigRepoMock;
            private IStudentConfigurationRepository studentConfigRepo;
            private Mock<IConfigurationRepository> configRepoMock;
            private IConfigurationRepository configRepo;
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IRoomRepository> roomRepoMock;
            private IRoomRepository roomRepo;
            private Mock<IEventRepository> eventRepoMock;
            private IEventRepository eventRepo;
            private Mock<IBookRepository> bookRepoMock;
            private IBookRepository bookRepo;


            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ILogger logger;
            private ICurrentUserFactory currentUserFactory;

            private Domain.Student.Entities.Section section1;
            private Domain.Student.Entities.Section section2;
            private Domain.Student.Entities.Section section3;
            private Domain.Student.Entities.SectionMeeting meeting1;
            private Domain.Student.Entities.SectionMeeting meeting2;
            private Domain.Student.Entities.SectionMeeting meeting3;
            private Domain.Student.Entities.SectionMeeting meeting4;

            private List<OfferingDepartment> dpts = new List<OfferingDepartment>() { new OfferingDepartment("ART", 100m) };
            private List<string> levels = new List<string>() { "Whatever" };
            private List<SectionStatusItem> statuses = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-60)) };
            private List<SectionStatusItem> statuses2 = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Inactive, "I", DateTime.Today.AddDays(-60)) };
            private List<SectionStatusItem> statuses3 = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Cancelled, "C", DateTime.Today.AddDays(-60)) };
            private IEnumerable<Domain.Student.Entities.Course> allCourses = new TestCourseRepository().GetAsync().Result;

            private List<CreditCategory> creditCategories = new List<CreditCategory>() { new CreditCategory(Guid.NewGuid().ToString(), "RC", "Regular Credit", CreditType.Institutional),
                new CreditCategory(Guid.NewGuid().ToString(), "RC", "Regular Credit", CreditType.Transfer), new CreditCategory(Guid.NewGuid().ToString(), "RC", "Regular Credit", CreditType.ContinuingEducation ) };
            private List<Domain.Student.Entities.AcademicLevel> academicLevels = new List<Domain.Student.Entities.AcademicLevel>() { new Domain.Student.Entities.AcademicLevel(Guid.NewGuid().ToString(), "UG", "Undergraduate") };
            private List<Domain.Student.Entities.GradeScheme> gradeSchemes = new List<Domain.Student.Entities.GradeScheme>() { new Domain.Student.Entities.GradeScheme(Guid.NewGuid().ToString(), "Pass/Fail", "Pass/Fail grade") };
            private List<Domain.Student.Entities.CourseLevel> courseLevels = new List<Domain.Student.Entities.CourseLevel>() { new Domain.Student.Entities.CourseLevel(Guid.NewGuid().ToString(), "100", "Introduction to terms, concepts, and techniques.") };
            private List<Department> departments = new List<Department>() { new Department(Guid.NewGuid().ToString(), "PHYS", "Physics", true) };
            private List<Location> locations = new List<Location>() { new Location(Guid.NewGuid().ToString(), "MD", "Maryland") };

            private string meeting1Guid = Guid.NewGuid().ToString();
            private string instructionalMethodGuid = Guid.NewGuid().ToString();
            private string roomGuid = Guid.NewGuid().ToString();
            private Domain.Entities.Permission permissionCreateandUpdateSections;

            [TestInitialize]
            public void Initialize()
            {
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                roomRepoMock = new Mock<IRoomRepository>();
                roomRepo = roomRepoMock.Object;
                stuRefDataRepoMock = new Mock<IStudentReferenceDataRepository>();
                studentReferenceDataRepo = stuRefDataRepoMock.Object;
                referenceDataRepoMock = new Mock<IReferenceDataRepository>();
                referenceDataRepo = referenceDataRepoMock.Object;
                courseRepoMock = new Mock<ICourseRepository>();
                courseRepo = courseRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                studentConfigRepoMock = new Mock<IStudentConfigurationRepository>();
                studentConfigRepo = studentConfigRepoMock.Object;
                configRepoMock = new Mock<IConfigurationRepository>();
                configRepo = configRepoMock.Object;
                eventRepoMock = new Mock<IEventRepository>();
                eventRepo = eventRepoMock.Object;
                bookRepoMock = new Mock<IBookRepository>();
                bookRepo = bookRepoMock.Object;


                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;

                // Set Current User
                currentUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();

                logger = new Mock<ILogger>().Object;

                // Mock permissions
                permissionCreateandUpdateSections = new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateAndUpdateSection);
                thirdPartyRole.AddPermission(permissionCreateandUpdateSections);
                //permissionViewAnyProjects = new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewAnyProjects);
                //thirdPartyRole.AddPermission(permissionViewAnyProjects);
                //permissionViewAnyProjectsLineItems = new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.ViewAnyProjectsLineItems);
                //thirdPartyRole.AddPermission(permissionViewAnyProjectsLineItems);

                // Mock the section repo response
                section1 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC1", "1119", "01", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses, true);
                section1.TermId = "2012/FA";
                section1.EndDate = new DateTime(2012, 12, 21);
                section1.Guid = Guid.NewGuid().ToString();

                section2 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC2", "1119", "02", new DateTime(2012, 09, 01), 3.0m, null, "Introduction to Art", "IN", dpts, levels, "UG", statuses2, true);
                section2.TermId = "2012/FA";
                section2.Guid = Guid.NewGuid().ToString();

                section3 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC3", "1119", "03", new DateTime(2011, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses3, true);
                section3.TermId = "2011/FA";
                section3.EndDate = new DateTime(2011, 12, 21);
                section3.Guid = Guid.NewGuid().ToString();

                var sectionFaculty = new Domain.Student.Entities.SectionFaculty("1234", "SEC1", "0000678", "LEC", new DateTime(2012, 09, 01), new DateTime(2011, 12, 21), 100.0m);

                meeting1 = new Domain.Student.Entities.SectionMeeting("123", "SEC1", "LEC", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "W");
                meeting1.Guid = meeting1Guid;
                meeting1.AddFacultyId("0000678");
                meeting1.AddSectionFaculty(sectionFaculty);
                meeting1.StartTime = new DateTimeOffset(2012, 01, 01, 08, 30, 00, new TimeSpan());
                meeting1.EndTime = new DateTimeOffset(2012, 12, 21, 09, 30, 00, new TimeSpan());
                meeting1.Room = "DAN*102";
                section1.AddSectionMeeting(meeting1);

                meeting2 = new Domain.Student.Entities.SectionMeeting("123", "SEC1", "LEC", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "D");
                meeting2.Guid = meeting1Guid;

                meeting3 = new Domain.Student.Entities.SectionMeeting("456", "SEC1", "LEC", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "M");
                meeting3.Guid = meeting1Guid;

                meeting4 = new Domain.Student.Entities.SectionMeeting("789", "SEC1", "LEC", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "Y");
                meeting4.Guid = meeting1Guid;

                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(section1.Guid, It.IsAny<bool>())).ReturnsAsync(section1);
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(section2.Guid, It.IsAny<bool>())).ReturnsAsync(section2);
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(section3.Guid, It.IsAny<bool>())).ReturnsAsync(section3);
                sectionRepoMock.Setup(repo => repo.PostSectionMeetingAsync(It.IsAny<Domain.Student.Entities.Section>(), meeting1.Guid)).ReturnsAsync(meeting1);
                sectionRepoMock.Setup(repo => repo.PostSectionMeetingAsync(It.IsAny<Domain.Student.Entities.Section>(), meeting2.Guid)).ReturnsAsync(meeting2);
                sectionRepoMock.Setup(repo => repo.PostSectionMeetingAsync(It.IsAny<Domain.Student.Entities.Section>(), meeting3.Guid)).ReturnsAsync(meeting3);
                courseRepoMock.Setup(repo => repo.GetAsync()).Returns(Task.FromResult(allCourses));
                var course_temp = allCourses.Where(c => c.Id == "342").FirstOrDefault();
                courseRepoMock.Setup(repo => repo.GetCourseByGuidAsync(It.IsAny<String>())).ReturnsAsync(course_temp);
                stuRefDataRepoMock.Setup(repo => repo.GetCreditCategoriesAsync()).ReturnsAsync(creditCategories);
                stuRefDataRepoMock.Setup(repo => repo.GetAcademicLevelsAsync()).ReturnsAsync(academicLevels);
                stuRefDataRepoMock.Setup(repo => repo.GetGradeSchemesAsync()).ReturnsAsync(gradeSchemes);
                stuRefDataRepoMock.Setup(repo => repo.GetCourseLevelsAsync()).ReturnsAsync(courseLevels);
                referenceDataRepoMock.Setup(repo => repo.DepartmentsAsync()).ReturnsAsync(departments);
                referenceDataRepoMock.Setup(repo => repo.Locations).Returns(locations);

                // Configuration defaults and campus calendar information.
                configRepoMock.Setup(repo => repo.GetDefaultsConfiguration()).Returns(new DefaultsConfiguration() { CampusCalendarId = "MAIN" });
                studentConfigRepoMock.Setup(repo => repo.GetCurriculumConfigurationAsync()).ReturnsAsync(new CurriculumConfiguration() { SectionActiveStatusCode = "A", SectionInactiveStatusCode = "I", DefaultInstructionalMethodCode = "INT" });
                eventRepoMock.Setup(repo => repo.GetCalendar("MAIN")).Returns(new CampusCalendar("MAIN", "Default Calendar", new TimeSpan(), new TimeSpan()));

                var sectionStatusCodes = new List<SectionStatusCode>()
                {
                    new SectionStatusCode("P", "Pending", SectionStatus.Active, SectionStatusIntegration.Pending),
                    new SectionStatusCode("C", "Closed", SectionStatus.Active, SectionStatusIntegration.Closed),
                    new SectionStatusCode("C", "Cancelled", SectionStatus.Active, SectionStatusIntegration.Cancelled),
                    new SectionStatusCode("O", "Open", SectionStatus.Active, SectionStatusIntegration.Open),
                };

                stuRefDataRepoMock.Setup(repo => repo.GetSectionStatusCodesAsync()).ReturnsAsync(sectionStatusCodes);


                // Mock the section service
                sectionCoordinationService = new SectionCoordinationService(adapterRegistry, sectionRepo, courseRepo, studentRepo, studentReferenceDataRepo,
                    referenceDataRepo, termRepo, studentConfigRepo, configRepo, personRepo, roomRepo, eventRepo, bookRepo, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionRepo = null;
                studentRepo = null;
                roleRepo = null;
                adapterRegistry = null;
                sectionCoordinationService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionService_Null_Object_V6()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                var r = await sectionCoordinationService.PostSection3Async(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SectionService_Null_Guid_V6()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                var r = await sectionCoordinationService.PostSection3Async(new Dtos.Section3());
            }

        

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionService_NullCourse_V6()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                //var credit = new List<Dtos.Credit2>() { new Dtos.Credit2() {CreditCategory = new Dtos.CreditIdAndTypeProperty() { 
                //    Detail = new Dtos.GuidObject2(creditCategories[0].Guid) }, Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = new Dtos.GuidObject2(creditCategories[0].Guid) }, Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section2 = new Dtos.Section3()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    Title = "Modern Algebra",
                    Course = null,
                    Credits = credit,
                    Status = Dtos.SectionStatus2.Open
                };
                var r = await sectionCoordinationService.PostSection3Async(section2);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionService_EmptyTitle_V6()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = new Dtos.GuidObject2(creditCategories[0].Guid) }, Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section2 = new Dtos.Section3()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    Title = "",
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,
                    Status = Dtos.SectionStatus2.Open
                };
                var r = await sectionCoordinationService.PostSection3Async(section2);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionService_NullStartOn_V6()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = new Dtos.GuidObject2(creditCategories[0].Guid) }, Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section2 = new Dtos.Section3()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = null,
                    Title = "MATH",
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,
                    Status = Dtos.SectionStatus2.Open
                };
                var r = await sectionCoordinationService.PostSection3Async(section2);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SectionService_StartOnGreaterThanEndOn_V6()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = new Dtos.GuidObject2(creditCategories[0].Guid) }, Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section2 = new Dtos.Section3()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    EndOn = new DateTime(2010, 09, 01),
                    Title = "MATH",
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,
                    Status = Dtos.SectionStatus2.Open
                };
                var r = await sectionCoordinationService.PostSection3Async(section2);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SectionService_NullCreditMeasure_V6()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = new Dtos.GuidObject2(creditCategories[0].Guid) }, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section2 = new Dtos.Section3()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    Title = "MATH",
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,
                    Status = Dtos.SectionStatus2.Open
                };
                var r = await sectionCoordinationService.PostSection3Async(section2);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SectionService_NullCreditDetailAndCreditType_V6()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(It.IsAny<String>(), It.IsAny<bool>())).ReturnsAsync(section1);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PostSectionAsync(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section1);
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = null }, Measure = new Dtos.CreditMeasure2(), Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section2 = new Dtos.Section3()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    Title = "MATH",
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,
                    Status = Dtos.SectionStatus2.Open
                };
                var r = await sectionCoordinationService.PostSection3Async(section2);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SectionService_NullCreditMinimum_V6()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = new Dtos.GuidObject2(creditCategories[0].Guid) }, Measure = Dtos.CreditMeasure2.CEU, Maximum = 4.0m, Increment = 1.0m}};
                var section2 = new Dtos.Section3()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    EndOn = new DateTime(2010, 09, 01),
                    Title = "MATH",
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,
                    Status = Dtos.SectionStatus2.Open
                };
                var r = await sectionCoordinationService.PostSection3Async(section2);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SectionService_NullCreditType_V6()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(It.IsAny<String>(), It.IsAny<bool>())).ReturnsAsync(section1);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PostSectionAsync(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section1);
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2(),
                    Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section = new Dtos.Section3()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    EndOn = new DateTime(2011, 12, 14),
                    Title = "Modern Algebra",
                    Course = new Dtos.GuidObject2("342"),
                    Status = Dtos.SectionStatus2.Open,
                    Credits = credit,
                    CourseLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2("sjdhd7392") }
                };
                var r = await sectionCoordinationService.PostSection3Async(section);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SectionService_NullInstructionalPlaforms_V6()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(It.IsAny<String>(), It.IsAny<bool>())).ReturnsAsync(section1);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PostSectionAsync(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section1);
                referenceDataRepoMock.Setup(repo => repo.GetInstructionalPlatformsAsync(It.IsAny<bool>())).ReturnsAsync(() => null);
                //var credit = new List<Dtos.Credit2>() { new Dtos.Credit2() {CreditCategory = new Dtos.CreditIdAndTypeProperty(), 
                //Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section = new Dtos.Section3()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    EndOn = new DateTime(2011, 12, 14),
                    Title = "Modern Algebra",
                    Course = new Dtos.GuidObject2("342"),
                    Status = Dtos.SectionStatus2.Open,
                    AcademicLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2(academicLevels[0].Guid) },
                    //Credits = credit,
                    InstructionalPlatform = new Dtos.GuidObject2("jfhjdhjs"),
                    //OwningOrganizations = new List<Dtos.OfferingOrganization2>() { 
                    //    new Dtos.OfferingOrganization2() {Organization = new Dtos.GuidObject2(departments[0].Guid) } },
                    CourseLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2("sjdhd7392") }
                };
                var r = await sectionCoordinationService.PostSection3Async(section);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SectionService_NullCreditCategoryOfCredit_V6()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(It.IsAny<String>(), It.IsAny<bool>())).ReturnsAsync(section1);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PostSectionAsync(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section1);
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() { Measure = Dtos.CreditMeasure2.CEU,
                    Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section = new Dtos.Section3()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    EndOn = new DateTime(2011, 12, 14),
                    Title = "Modern Algebra",
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,
                    Status = Dtos.SectionStatus2.Open,
                    CourseLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2("sjdhd7392") }
                };
                var r = await sectionCoordinationService.PostSection3Async(section);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SectionService_NullCreditCategories_V6()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PostSectionAsync(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section2);
                stuRefDataRepoMock.Setup(repo => repo.GetCreditCategoriesAsync()).ReturnsAsync(() => null);
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = new Dtos.GuidObject2(creditCategories[0].Guid) }, Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section = new Dtos.Section3()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    EndOn = new DateTime(2011, 12, 14),
                    Title = "Modern Algebra",
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,
                    Status = Dtos.SectionStatus2.Open,
                    CourseLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2("sjdhd7392") }
                };
                var r = await sectionCoordinationService.PostSection3Async(section);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SectionService_NullGradeSchemes_V6()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(It.IsAny<String>(), It.IsAny<bool>())).ReturnsAsync(section3);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PostSectionAsync(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section3);
                stuRefDataRepoMock.Setup(repo => repo.GetGradeSchemesAsync()).ReturnsAsync(() => null);
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() { 
                    /*Detail = new Dtos.GuidObject2(creditCategories[0].Guid),*/ CreditType = Dtos.EnumProperties.CreditCategoryType3.Institutional }, Measure = Dtos.CreditMeasure2.Credit, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section = new Dtos.Section3()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    EndOn = new DateTime(2011, 12, 14),
                    Title = "Modern Algebra",
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,
                    Status = Dtos.SectionStatus2.Open,
                    CourseLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2("sjdhd7392") }
                };
                var r = await sectionCoordinationService.PostSection3Async(section);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SectionService_NullAcademicLevels_V6()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(It.IsAny<String>(), It.IsAny<bool>())).ReturnsAsync(section1);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PostSectionAsync(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section1);
                stuRefDataRepoMock.Setup(repo => repo.GetAcademicLevelsAsync()).ReturnsAsync(() => null);
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() { 
                    /*Detail = new Dtos.GuidObject2(creditCategories[0].Guid),*/ CreditType = Dtos.EnumProperties.CreditCategoryType3.Transfer }, Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section = new Dtos.Section3()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    EndOn = new DateTime(2011, 12, 14),
                    Title = "Modern Algebra",
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,
                    Status = Dtos.SectionStatus2.Open,
                    CourseLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2("sjdhd7392") }
                };
                var r = await sectionCoordinationService.PostSection3Async(section);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SectionService_NullLocations_V6()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(It.IsAny<String>(), It.IsAny<bool>())).ReturnsAsync(section2);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PostSectionAsync(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section2);
                referenceDataRepoMock.Setup(repo => repo.Locations).Returns<Location>(null);
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() { 
                    /*Detail = new Dtos.GuidObject2(creditCategories[0].Guid),*/ CreditType = Dtos.EnumProperties.CreditCategoryType3.ContinuingEducation }, Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section = new Dtos.Section3()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    EndOn = new DateTime(2011, 12, 14),
                    Title = "Modern Algebra",
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,
                    Status = Dtos.SectionStatus2.Open,
                    CourseLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2("sjdhd7392") }
                };
                var r = await sectionCoordinationService.PostSection3Async(section);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SectionService_NullCourseLevels_V6()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PostSectionAsync(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section1);
                stuRefDataRepoMock.Setup(repo => repo.GetCourseLevelsAsync()).ReturnsAsync(() => null);
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = new Dtos.GuidObject2(creditCategories[0].Guid) }, Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section2 = new Dtos.Section3()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    Title = "Modern Algebra",
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,
                    Status = Dtos.SectionStatus2.Open
                };
                var r = await sectionCoordinationService.PostSection3Async(section2);
            }

            [TestMethod]
            public async Task SectionService_Post_V6()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PostSectionAsync(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section1);
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = new Dtos.GuidObject2(creditCategories[0].Guid), CreditType = Dtos.EnumProperties.CreditCategoryType3.Institutional },
                    Measure = Dtos.CreditMeasure2.Credit, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section2 = new Dtos.Section3()
                {
                    Id = section1.Guid,
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    Title = "Modern Algebra",
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,
                    Status = Dtos.SectionStatus2.Open
                };
                var res = await sectionCoordinationService.PostSection3Async(section2);
                Assert.AreEqual(section1.Guid, res.Id);
            }
        }

        [TestClass]
        public class Sections_Put_V6 : CurrentUserSetup
        {
            private SectionCoordinationService sectionCoordinationService;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IStudentReferenceDataRepository> stuRefDataRepoMock;
            private IStudentReferenceDataRepository studentReferenceDataRepo;
            private Mock<IReferenceDataRepository> referenceDataRepoMock;
            private IReferenceDataRepository referenceDataRepo;
            private Mock<ICourseRepository> courseRepoMock;
            private ICourseRepository courseRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IStudentConfigurationRepository> studentConfigRepoMock;
            private IStudentConfigurationRepository studentConfigRepo;
            private Mock<IConfigurationRepository> configRepoMock;
            private IConfigurationRepository configRepo;
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IRoomRepository> roomRepoMock;
            private IRoomRepository roomRepo;
            private Mock<IEventRepository> eventRepoMock;
            private IEventRepository eventRepo;
            private Mock<IBookRepository> bookRepoMock;
            private IBookRepository bookRepo;


            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ILogger logger;
            private ICurrentUserFactory currentUserFactory;

            private Domain.Student.Entities.Section section1;
            private Domain.Student.Entities.Section section2;
            private Domain.Student.Entities.Section section3;
            private Domain.Student.Entities.SectionMeeting meeting1;
            private Domain.Student.Entities.SectionMeeting meeting2;
            private Domain.Student.Entities.SectionMeeting meeting3;
            private Domain.Student.Entities.SectionMeeting meeting4;

            private List<OfferingDepartment> dpts = new List<OfferingDepartment>() { new OfferingDepartment("ART", 100m) };
            private List<string> levels = new List<string>() { "Whatever" };
            private List<SectionStatusItem> statuses = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-60)) };
            private List<SectionStatusItem> statuses2 = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Inactive, "I", DateTime.Today.AddDays(-60)) };
            private List<SectionStatusItem> statuses3 = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Cancelled, "C", DateTime.Today.AddDays(-60)) };
            private IEnumerable<Domain.Student.Entities.Course> allCourses = new TestCourseRepository().GetAsync().Result;

            private List<CreditCategory> creditCategories = new List<CreditCategory>() { new CreditCategory(Guid.NewGuid().ToString(), "RC", "Regular Credit", CreditType.Institutional),
                new CreditCategory(Guid.NewGuid().ToString(), "RC", "Regular Credit", CreditType.Transfer), new CreditCategory(Guid.NewGuid().ToString(), "RC", "Regular Credit", CreditType.ContinuingEducation ) };
            private List<Domain.Student.Entities.AcademicLevel> academicLevels = new List<Domain.Student.Entities.AcademicLevel>() { new Domain.Student.Entities.AcademicLevel(Guid.NewGuid().ToString(), "UG", "Undergraduate") };
            private List<Domain.Student.Entities.GradeScheme> gradeSchemes = new List<Domain.Student.Entities.GradeScheme>() { new Domain.Student.Entities.GradeScheme(Guid.NewGuid().ToString(), "Pass/Fail", "Pass/Fail grade") };
            private List<Domain.Student.Entities.CourseLevel> courseLevels = new List<Domain.Student.Entities.CourseLevel>() { new Domain.Student.Entities.CourseLevel(Guid.NewGuid().ToString(), "100", "Introduction to terms, concepts, and techniques.") };
            private List<Department> departments = new List<Department>() { new Department(Guid.NewGuid().ToString(), "PHYS", "Physics", true) };
            private List<Location> locations = new List<Location>() { new Location(Guid.NewGuid().ToString(), "MD", "Maryland") };

            private string meeting1Guid = Guid.NewGuid().ToString();
            private string instructionalMethodGuid = Guid.NewGuid().ToString();
            private string roomGuid = Guid.NewGuid().ToString();
            private Domain.Entities.Permission permissionCreateandUpdateSections;

            [TestInitialize]
            public void Initialize()
            {
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                roomRepoMock = new Mock<IRoomRepository>();
                roomRepo = roomRepoMock.Object;
                stuRefDataRepoMock = new Mock<IStudentReferenceDataRepository>();
                studentReferenceDataRepo = stuRefDataRepoMock.Object;
                referenceDataRepoMock = new Mock<IReferenceDataRepository>();
                referenceDataRepo = referenceDataRepoMock.Object;
                courseRepoMock = new Mock<ICourseRepository>();
                courseRepo = courseRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                studentConfigRepoMock = new Mock<IStudentConfigurationRepository>();
                studentConfigRepo = studentConfigRepoMock.Object;
                configRepoMock = new Mock<IConfigurationRepository>();
                configRepo = configRepoMock.Object;
                eventRepoMock = new Mock<IEventRepository>();
                eventRepo = eventRepoMock.Object;
                bookRepoMock = new Mock<IBookRepository>();
                bookRepo = bookRepoMock.Object;


                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;

                // Set Current User
                currentUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();

                logger = new Mock<ILogger>().Object;

                // Mock permissions
                permissionCreateandUpdateSections = new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateAndUpdateSection);
                thirdPartyRole.AddPermission(permissionCreateandUpdateSections);
                //permissionViewAnyProjects = new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewAnyProjects);
                //thirdPartyRole.AddPermission(permissionViewAnyProjects);
                //permissionViewAnyProjectsLineItems = new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.ViewAnyProjectsLineItems);
                //thirdPartyRole.AddPermission(permissionViewAnyProjectsLineItems);

                // Mock the section repo response
                section1 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC1", "1119", "01", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses, true);
                section1.TermId = "2012/FA";
                section1.EndDate = new DateTime(2012, 12, 21);
                section1.Guid = Guid.NewGuid().ToString();

                section2 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC2", "1119", "02", new DateTime(2012, 09, 01), 3.0m, null, "Introduction to Art", "IN", dpts, levels, "UG", statuses2, true);
                section2.TermId = "2012/FA";
                section2.Guid = Guid.NewGuid().ToString();

                section3 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC3", "1119", "03", new DateTime(2011, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses3, true);
                section3.TermId = "2011/FA";
                section3.EndDate = new DateTime(2011, 12, 21);
                section3.Guid = Guid.NewGuid().ToString();

                var sectionFaculty = new Domain.Student.Entities.SectionFaculty("1234", "SEC1", "0000678", "LEC", new DateTime(2012, 09, 01), new DateTime(2011, 12, 21), 100.0m);

                meeting1 = new Domain.Student.Entities.SectionMeeting("123", "SEC1", "LEC", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "W");
                meeting1.Guid = meeting1Guid;
                meeting1.AddFacultyId("0000678");
                meeting1.AddSectionFaculty(sectionFaculty);
                meeting1.StartTime = new DateTimeOffset(2012, 01, 01, 08, 30, 00, new TimeSpan());
                meeting1.EndTime = new DateTimeOffset(2012, 12, 21, 09, 30, 00, new TimeSpan());
                meeting1.Room = "DAN*102";
                section1.AddSectionMeeting(meeting1);

                meeting2 = new Domain.Student.Entities.SectionMeeting("123", "SEC1", "LEC", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "D");
                meeting2.Guid = meeting1Guid;

                meeting3 = new Domain.Student.Entities.SectionMeeting("456", "SEC1", "LEC", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "M");
                meeting3.Guid = meeting1Guid;

                meeting4 = new Domain.Student.Entities.SectionMeeting("789", "SEC1", "LEC", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "Y");
                meeting4.Guid = meeting1Guid;

                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(section1.Guid, It.IsAny<bool>())).ReturnsAsync(section1);
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(section2.Guid, It.IsAny<bool>())).ReturnsAsync(section2);
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(section3.Guid, It.IsAny<bool>())).ReturnsAsync(section3);
                sectionRepoMock.Setup(repo => repo.PutSectionMeetingAsync(It.IsAny<Domain.Student.Entities.Section>(), meeting1.Guid)).ReturnsAsync(meeting1);
                sectionRepoMock.Setup(repo => repo.PutSectionMeetingAsync(It.IsAny<Domain.Student.Entities.Section>(), meeting2.Guid)).ReturnsAsync(meeting2);
                sectionRepoMock.Setup(repo => repo.PutSectionMeetingAsync(It.IsAny<Domain.Student.Entities.Section>(), meeting3.Guid)).ReturnsAsync(meeting3);
                courseRepoMock.Setup(repo => repo.GetAsync()).Returns(Task.FromResult(allCourses));
                var course_temp = allCourses.Where(c => c.Id == "342").FirstOrDefault();
                courseRepoMock.Setup(repo => repo.GetCourseByGuidAsync(It.IsAny<String>())).ReturnsAsync(course_temp);
                stuRefDataRepoMock.Setup(repo => repo.GetCreditCategoriesAsync()).ReturnsAsync(creditCategories);
                stuRefDataRepoMock.Setup(repo => repo.GetAcademicLevelsAsync()).ReturnsAsync(academicLevels);
                stuRefDataRepoMock.Setup(repo => repo.GetGradeSchemesAsync()).ReturnsAsync(gradeSchemes);
                stuRefDataRepoMock.Setup(repo => repo.GetCourseLevelsAsync()).ReturnsAsync(courseLevels);
                referenceDataRepoMock.Setup(repo => repo.DepartmentsAsync()).ReturnsAsync(departments);
                referenceDataRepoMock.Setup(repo => repo.Locations).Returns(locations);

                // Configuration defaults and campus calendar information.
                configRepoMock.Setup(repo => repo.GetDefaultsConfiguration()).Returns(new DefaultsConfiguration() { CampusCalendarId = "MAIN" });
                studentConfigRepoMock.Setup(repo => repo.GetCurriculumConfigurationAsync()).ReturnsAsync(new CurriculumConfiguration() { SectionActiveStatusCode = "A", SectionInactiveStatusCode = "I", DefaultInstructionalMethodCode = "INT" });
                eventRepoMock.Setup(repo => repo.GetCalendar("MAIN")).Returns(new CampusCalendar("MAIN", "Default Calendar", new TimeSpan(), new TimeSpan()));

                var sectionStatusCodes = new List<SectionStatusCode>()
                {
                    new SectionStatusCode("P", "Pending", SectionStatus.Active, SectionStatusIntegration.Pending),
                    new SectionStatusCode("C", "Closed", SectionStatus.Active, SectionStatusIntegration.Closed),
                    new SectionStatusCode("C", "Cancelled", SectionStatus.Active, SectionStatusIntegration.Cancelled),
                    new SectionStatusCode("O", "Open", SectionStatus.Active, SectionStatusIntegration.Open),
                };

                stuRefDataRepoMock.Setup(repo => repo.GetSectionStatusCodesAsync()).ReturnsAsync(sectionStatusCodes);


                // Mock the section service
                sectionCoordinationService = new SectionCoordinationService(adapterRegistry, sectionRepo, courseRepo, studentRepo, studentReferenceDataRepo,
                    referenceDataRepo, termRepo, studentConfigRepo, configRepo, personRepo, roomRepo, eventRepo, bookRepo, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionRepo = null;
                studentRepo = null;
                roleRepo = null;
                adapterRegistry = null;
                sectionCoordinationService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionService_Null_Object_V6()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                var r = await sectionCoordinationService.PutSection3Async(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SectionService_Null_Guid_V6()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                var r = await sectionCoordinationService.PutSection3Async(new Dtos.Section3());
            }

            //[TestMethod]
            //[ExpectedException(typeof(PermissionsException))]
            //public async Task SectionService_SectionKeyNotFound_V6()
            //{
            //    sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(section1.Guid, It.IsAny<bool>())).Throws(new KeyNotFoundException());
            //    var r = await sectionCoordinationService.PutSection3Async(new Dtos.Section3()
            //    {
            //        Id = meeting1.Guid
            //    });
            //}

            //[TestMethod]
            //[ExpectedException(typeof(PermissionsException))]
            //public async Task SectionService_NoPermission_V6()
            //{
            //    var section = new Dtos.Section3() { Id = "999999999999999" };
            //    var r = await sectionCoordinationService.PutSection3Async(section);
            //}

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionService_NullCourse_V6()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                //var credit = new List<Dtos.Credit2>() { new Dtos.Credit2() {CreditCategory = new Dtos.CreditIdAndTypeProperty() { 
                //    Detail = new Dtos.GuidObject2(creditCategories[0].Guid) }, Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = new Dtos.GuidObject2(creditCategories[0].Guid) }, Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section2 = new Dtos.Section3()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    Title = "Modern Algebra",
                    Course = null,
                    Credits = credit,
                    Status = Dtos.SectionStatus2.Open
                };
                var r = await sectionCoordinationService.PutSection3Async(section2);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionService_EmptyTitle_V6()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = new Dtos.GuidObject2(creditCategories[0].Guid) }, Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section2 = new Dtos.Section3()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    Title = "",
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,
                    Status = Dtos.SectionStatus2.Open
                };
                var r = await sectionCoordinationService.PutSection3Async(section2);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionService_NullStartOn_V6()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = new Dtos.GuidObject2(creditCategories[0].Guid) }, Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section2 = new Dtos.Section3()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = null,
                    Title = "MATH",
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,
                    Status = Dtos.SectionStatus2.Open
                };
                var r = await sectionCoordinationService.PutSection3Async(section2);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SectionService_StartOnGreaterThanEndOn_V6()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = new Dtos.GuidObject2(creditCategories[0].Guid) }, Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section2 = new Dtos.Section3()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    EndOn = new DateTime(2010, 09, 01),
                    Title = "MATH",
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,
                    Status = Dtos.SectionStatus2.Open
                };
                var r = await sectionCoordinationService.PutSection3Async(section2);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SectionService_NullCreditMeasure_V6()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = new Dtos.GuidObject2(creditCategories[0].Guid) }, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section2 = new Dtos.Section3()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    Title = "MATH",
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,
                    Status = Dtos.SectionStatus2.Open
                };
                var r = await sectionCoordinationService.PutSection3Async(section2);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SectionService_NullCreditDetailAndCreditType_V6()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(It.IsAny<String>(), It.IsAny<bool>())).ReturnsAsync(section1);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PutSectionAsync(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section1);
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = null }, Measure = new Dtos.CreditMeasure2(), Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section2 = new Dtos.Section3()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    Title = "MATH",
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,
                    Status = Dtos.SectionStatus2.Open
                };
                var r = await sectionCoordinationService.PutSection3Async(section2);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SectionService_NullCreditMinimum_V6()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = new Dtos.GuidObject2(creditCategories[0].Guid) }, Measure = Dtos.CreditMeasure2.CEU, Maximum = 4.0m, Increment = 1.0m}};
                var section2 = new Dtos.Section3()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    EndOn = new DateTime(2010, 09, 01),
                    Title = "MATH",
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,
                    Status = Dtos.SectionStatus2.Open
                };
                var r = await sectionCoordinationService.PutSection3Async(section2);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SectionService_NullCreditType_V6()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(It.IsAny<String>(), It.IsAny<bool>())).ReturnsAsync(section1);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PutSectionAsync(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section1);
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2(),
                    Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section = new Dtos.Section3()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    EndOn = new DateTime(2011, 12, 14),
                    Title = "Modern Algebra",
                    Course = new Dtos.GuidObject2("342"),
                    Status = Dtos.SectionStatus2.Open,
                    Credits = credit,
                    CourseLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2("sjdhd7392") }
                };
                var r = await sectionCoordinationService.PutSection3Async(section);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SectionService_NullInstructionalPlaforms_V6()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(It.IsAny<String>(), It.IsAny<bool>())).ReturnsAsync(section1);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PutSectionAsync(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section1);
                referenceDataRepoMock.Setup(repo => repo.GetInstructionalPlatformsAsync(It.IsAny<bool>())).ReturnsAsync(() => null);
                //var credit = new List<Dtos.Credit2>() { new Dtos.Credit2() {CreditCategory = new Dtos.CreditIdAndTypeProperty(), 
                //Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section = new Dtos.Section3()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    EndOn = new DateTime(2011, 12, 14),
                    Title = "Modern Algebra",
                    Course = new Dtos.GuidObject2("342"),
                    Status = Dtos.SectionStatus2.Open,
                    AcademicLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2(academicLevels[0].Guid) },
                    //Credits = credit,
                    InstructionalPlatform = new Dtos.GuidObject2("jfhjdhjs"),
                    //OwningOrganizations = new List<Dtos.OfferingOrganization2>() { 
                    //    new Dtos.OfferingOrganization2() {Organization = new Dtos.GuidObject2(departments[0].Guid) } },
                    CourseLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2("sjdhd7392") }
                };
                var r = await sectionCoordinationService.PutSection3Async(section);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SectionService_NullCreditCategoryOfCredit_V6()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(It.IsAny<String>(), It.IsAny<bool>())).ReturnsAsync(section1);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PutSectionAsync(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section1);
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() { Measure = Dtos.CreditMeasure2.CEU,
                    Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section = new Dtos.Section3()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    EndOn = new DateTime(2011, 12, 14),
                    Title = "Modern Algebra",
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,
                    Status = Dtos.SectionStatus2.Open,
                    CourseLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2("sjdhd7392") }
                };
                var r = await sectionCoordinationService.PutSection3Async(section);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SectionService_NullCreditCategories_V6()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PutSectionAsync(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section2);
                stuRefDataRepoMock.Setup(repo => repo.GetCreditCategoriesAsync()).ReturnsAsync(() => null);
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = new Dtos.GuidObject2(creditCategories[0].Guid) }, Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section = new Dtos.Section3()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    EndOn = new DateTime(2011, 12, 14),
                    Title = "Modern Algebra",
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,
                    Status = Dtos.SectionStatus2.Open,
                    CourseLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2("sjdhd7392") }
                };
                var r = await sectionCoordinationService.PutSection3Async(section);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SectionService_NullGradeSchemes_V6()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(It.IsAny<String>(), It.IsAny<bool>())).ReturnsAsync(section3);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PutSectionAsync(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section3);
                stuRefDataRepoMock.Setup(repo => repo.GetGradeSchemesAsync()).ReturnsAsync(() => null);
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() { 
                    /*Detail = new Dtos.GuidObject2(creditCategories[0].Guid),*/ CreditType = Dtos.EnumProperties.CreditCategoryType3.Institutional }, Measure = Dtos.CreditMeasure2.Credit, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section = new Dtos.Section3()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    EndOn = new DateTime(2011, 12, 14),
                    Title = "Modern Algebra",
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,
                    Status = Dtos.SectionStatus2.Open,
                    CourseLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2("sjdhd7392") }
                };
                var r = await sectionCoordinationService.PutSection3Async(section);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SectionService_NullAcademicLevels_V6()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(It.IsAny<String>(), It.IsAny<bool>())).ReturnsAsync(section1);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PutSectionAsync(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section1);
                stuRefDataRepoMock.Setup(repo => repo.GetAcademicLevelsAsync()).ReturnsAsync(() => null);
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() { 
                    /*Detail = new Dtos.GuidObject2(creditCategories[0].Guid),*/ CreditType = Dtos.EnumProperties.CreditCategoryType3.Transfer }, Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section = new Dtos.Section3()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    EndOn = new DateTime(2011, 12, 14),
                    Title = "Modern Algebra",
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,
                    Status = Dtos.SectionStatus2.Open,
                    CourseLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2("sjdhd7392") }
                };
                var r = await sectionCoordinationService.PutSection3Async(section);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SectionService_NullLocations_V6()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(It.IsAny<String>(), It.IsAny<bool>())).ReturnsAsync(section2);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PutSectionAsync(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section2);
                referenceDataRepoMock.Setup(repo => repo.Locations).Returns<Location>(null);
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() { 
                    /*Detail = new Dtos.GuidObject2(creditCategories[0].Guid),*/ CreditType = Dtos.EnumProperties.CreditCategoryType3.ContinuingEducation }, Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section = new Dtos.Section3()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    EndOn = new DateTime(2011, 12, 14),
                    Title = "Modern Algebra",
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,
                    Status = Dtos.SectionStatus2.Open,
                    CourseLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2("sjdhd7392") }
                };
                var r = await sectionCoordinationService.PutSection3Async(section);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SectionService_NullCourseLevels_V6()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PutSectionAsync(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section1);
                stuRefDataRepoMock.Setup(repo => repo.GetCourseLevelsAsync()).ReturnsAsync(() => null);
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = new Dtos.GuidObject2(creditCategories[0].Guid) }, Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section2 = new Dtos.Section3()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    Title = "Modern Algebra",
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,
                    Status = Dtos.SectionStatus2.Open
                };
                var r = await sectionCoordinationService.PutSection3Async(section2);
            }

            [TestMethod]
            public async Task SectionService_Put_V6()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PutSectionAsync(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section1);
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = new Dtos.GuidObject2(creditCategories[0].Guid), CreditType = Dtos.EnumProperties.CreditCategoryType3.Institutional },
                    Measure = Dtos.CreditMeasure2.Credit, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section2 = new Dtos.Section3()
                {
                    Id = section1.Guid,
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    Title = "Modern Algebra",
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,
                    Status = Dtos.SectionStatus2.Open
                };
                var res = await sectionCoordinationService.PutSection3Async(section2);
                Assert.AreEqual(section1.Guid, res.Id);
            }
        }

        [TestClass]
        public class Sections_Get_V8 : CurrentUserSetup
        {
            private SectionCoordinationService sectionCoordinationService;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IStudentReferenceDataRepository> stuRefDataRepoMock;
            private IStudentReferenceDataRepository studentReferenceDataRepo;
            private Mock<IReferenceDataRepository> referenceDataRepoMock;
            private IReferenceDataRepository referenceDataRepo;
            private Mock<ICourseRepository> courseRepoMock;
            private ICourseRepository courseRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IStudentConfigurationRepository> studentConfigRepoMock;
            private IStudentConfigurationRepository studentConfigRepo;
            private Mock<IConfigurationRepository> configRepoMock;
            private IConfigurationRepository configRepo;
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IRoomRepository> roomRepoMock;
            private IRoomRepository roomRepo;
            private Mock<IEventRepository> eventRepoMock;
            private IEventRepository eventRepo;
            private Mock<IBookRepository> bookRepoMock;
            private IBookRepository bookRepo;

            // private Mock<IAcademicPeriodRepository> academicPeriodRepoMock;
            // private IAcademicPeriodRepository academicPeriodRepo;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ILogger logger;
            private ICurrentUserFactory currentUserFactory;

            private Domain.Student.Entities.Section section1;
            private Domain.Student.Entities.Section section2;
            private Domain.Student.Entities.Section section3;
            private Domain.Student.Entities.Section section4;
            private Domain.Student.Entities.SectionMeeting meeting1;
            private Domain.Student.Entities.SectionMeeting meeting2;
            private Domain.Student.Entities.SectionMeeting meeting3;
            private Domain.Student.Entities.SectionMeeting meeting4;

            private List<OfferingDepartment> dpts = new List<OfferingDepartment>() { new OfferingDepartment("ART", 100m) };
            private List<string> levels = new List<string>() { "Whatever" };
            private List<SectionStatusItem> statuses = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-60)) };
            private List<SectionStatusItem> statuses2 = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Inactive, "I", DateTime.Today.AddDays(-60)) };
            private List<SectionStatusItem> statuses3 = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Cancelled, "C", DateTime.Today.AddDays(-60)) };
            private List<SectionStatusItem> statuses4 = new List<SectionStatusItem>() { new SectionStatusItem(new SectionStatus(), "N/A", DateTime.Today.AddDays(-60)) };
            private List<CreditCategory> creditCategories = new List<CreditCategory>() { new CreditCategory(Guid.NewGuid().ToString(), "IN", "Regular Credit", CreditType.Institutional) };
            private List<Domain.Student.Entities.AcademicLevel> academicLevels = new List<Domain.Student.Entities.AcademicLevel>() { new Domain.Student.Entities.AcademicLevel(Guid.NewGuid().ToString(), "UG", "Undergraduate") };
            private List<Domain.Student.Entities.GradeScheme> gradeSchemes = new List<Domain.Student.Entities.GradeScheme>() { new Domain.Student.Entities.GradeScheme(Guid.NewGuid().ToString(), "Pass/Fail", "Pass/Fail grade") };
            private List<Domain.Student.Entities.CourseLevel> courseLevels = new List<Domain.Student.Entities.CourseLevel>() { new Domain.Student.Entities.CourseLevel(Guid.NewGuid().ToString(), "100", "Introduction to terms, concepts, and techniques.") };
            private List<Department> departments = new List<Department>() { new Department(Guid.NewGuid().ToString(), "PHYS", "Physics", true) };
            private List<Location> locations = new List<Location>() { new Location(Guid.NewGuid().ToString(), "MD", "Maryland") };
            private List<AcademicPeriod> academicPeriods = new List<AcademicPeriod>() { new AcademicPeriod(Guid.NewGuid().ToString(), "2012/FA", "Fall 2014", DateTime.Today.AddDays(-60), DateTime.Today, 2015, 4, "Spring", Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), null) };
            private List<Domain.Base.Entities.InstructionalPlatform> instructionalPlatforms = new List<Domain.Base.Entities.InstructionalPlatform>() { new Domain.Base.Entities.InstructionalPlatform(Guid.NewGuid().ToString(), "Ellucian", "IP") };

            private string meeting1Guid = Guid.NewGuid().ToString();
            private string sectionGuid = Guid.NewGuid().ToString();
            private string roomGuid = Guid.NewGuid().ToString();
            private string courseGuid = Guid.NewGuid().ToString();

            [TestInitialize]
            public void Initialize()
            {
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                roomRepoMock = new Mock<IRoomRepository>();
                roomRepo = roomRepoMock.Object;
                stuRefDataRepoMock = new Mock<IStudentReferenceDataRepository>();
                studentReferenceDataRepo = stuRefDataRepoMock.Object;
                referenceDataRepoMock = new Mock<IReferenceDataRepository>();
                referenceDataRepo = referenceDataRepoMock.Object;
                courseRepoMock = new Mock<ICourseRepository>();
                courseRepo = courseRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                studentConfigRepoMock = new Mock<IStudentConfigurationRepository>();
                studentConfigRepo = studentConfigRepoMock.Object;
                configRepoMock = new Mock<IConfigurationRepository>();
                configRepo = configRepoMock.Object;
                eventRepoMock = new Mock<IEventRepository>();
                eventRepo = eventRepoMock.Object;
                bookRepoMock = new Mock<IBookRepository>();
                bookRepo = bookRepoMock.Object;

                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;

                // "STU1" is the current user
                currentUserFactory = new CurrentUserSetup.StudentUserFactory();

                logger = new Mock<ILogger>().Object;

                // Mock the section repo response
                section1 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC1", "1119", "01", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses, true);
                section1.Guid = sectionGuid;
                section1.TermId = "2012/FA";
                section1.EndDate = new DateTime(2012, 12, 21);
                section1.LearningProvider = "Ellucian";
                section1.Location = "Loc1";
                section1.BillingCred = 3;
                section1.SectionCapacity = 10;
                section1.GlobalCapacity = 20;

                section2 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC2", "1119", "02", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses2, true);
                section2.Guid = Guid.NewGuid().ToString();
                section2.TermId = "2012/FA";
                section1.Location = "Loc2";
                section2.BillingCred = 4;

                section3 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC3", "1119", "03", new DateTime(2011, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses3, true);
                section3.Guid = Guid.NewGuid().ToString();
                section3.TermId = "2011/FA";
                section3.EndDate = new DateTime(2011, 12, 21);
                section3.BillingCred = 3;
                section3.CensusDates = new List<DateTime?>() { new DateTime(2011, 09, 10) };

                section4 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC4", "1119", "04", new DateTime(2011, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses4, true);
                section4.Guid = Guid.NewGuid().ToString();
                section4.TermId = "2011/FA";
                section4.EndDate = new DateTime(2011, 12, 21);
                section4.Location = "Loc1";

                var sectionFaculty = new Domain.Student.Entities.SectionFaculty("1234", "SEC1", "0000678", "LEC", new DateTime(2012, 09, 01), new DateTime(2011, 12, 21), 100.0m);

                meeting1 = new Domain.Student.Entities.SectionMeeting("123", "SEC1", "LEC", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "W");
                meeting1.Guid = meeting1Guid;
                meeting1.AddFacultyId("0000678");
                meeting1.AddSectionFaculty(sectionFaculty);
                meeting1.StartTime = new DateTimeOffset(2012, 01, 01, 08, 30, 00, new TimeSpan());
                meeting1.EndTime = new DateTimeOffset(2012, 12, 21, 09, 30, 00, new TimeSpan());
                meeting1.Room = "DAN*102";
                section1.AddSectionMeeting(meeting1);

                meeting2 = new Domain.Student.Entities.SectionMeeting("123", "SEC1", "LEC", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "D");
                meeting2.Guid = meeting1Guid;

                meeting3 = new Domain.Student.Entities.SectionMeeting("456", "SEC1", "LEC", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "M");
                meeting3.Guid = meeting1Guid;

                meeting4 = new Domain.Student.Entities.SectionMeeting("789", "SEC1", "LEC", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "Y");
                meeting4.Guid = meeting1Guid;

                var sections = new List<Domain.Student.Entities.Section>();
                //var section = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC1", "1119", "01", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses, true);
                sections.Add(section1);
                sections.Add(section2);
                sections.Add(section3);
                sections.Add(section4);

                var rooms = new List<Domain.Base.Entities.Room>();
                var room = new Domain.Base.Entities.Room(roomGuid, "DAN*102", "Danville Hall Room 102");
                rooms.Add(room);

                var scheduleRepeats = new List<Domain.Base.Entities.ScheduleRepeat>();
                var scheduleRepeat = new Domain.Base.Entities.ScheduleRepeat("W", "Weekly", "7", FrequencyType.Weekly);
                scheduleRepeats.Add(scheduleRepeat);
                scheduleRepeat = new Domain.Base.Entities.ScheduleRepeat("D", "Daily", "1", FrequencyType.Daily);
                scheduleRepeats.Add(scheduleRepeat);
                scheduleRepeat = new Domain.Base.Entities.ScheduleRepeat("M", "Monthly", "30", FrequencyType.Monthly);
                scheduleRepeats.Add(scheduleRepeat);
                scheduleRepeat = new Domain.Base.Entities.ScheduleRepeat("Y", "Yearly", "365", FrequencyType.Yearly);
                scheduleRepeats.Add(scheduleRepeat);

                List<string> sec1List = new List<string>() { "SEC1" };
                List<Domain.Student.Entities.Section> sec1Resp = new List<Domain.Student.Entities.Section>() { section1 };
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(sec1List, false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sec1Resp));

                List<string> sec2List = new List<string>() { "SEC2" };
                List<Domain.Student.Entities.Section> sec2Resp = new List<Domain.Student.Entities.Section>() { section2 };
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(sec2List, false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sec2Resp));

                List<string> sec3List = new List<string>() { "SEC3" };
                List<Domain.Student.Entities.Section> sec3Resp = new List<Domain.Student.Entities.Section>() { section3 };
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(sec3List, false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sec3Resp));

                List<string> sec4List = new List<string>() { "9999999" };
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(sec4List, false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(new List<Domain.Student.Entities.Section>()));

                Term term1 = new Term("termGuid1", "2011/FA", "2011 Fall", new DateTime(2011, 09, 01), new DateTime(2011, 12, 15), 2012,
                    0, false, false, "2011/FA", false);
                var date1 = new List<DateTime?>() { new DateTime(2011, 09, 15) };
                var regDate = new RegistrationDate("Loc1", null, null, null, null, null, null, null, null, null, date1);
                term1.AddRegistrationDates(regDate);
                Term term2 = new Term("termGuid2", "2012/FA", "2012 Fall", new DateTime(2012, 09, 01), new DateTime(2012, 12, 15), 2013,
                    0, false, false, "2012/FA", false);
                regDate = new RegistrationDate("Loc2", null, null, null, null, null, null, null, null, null, new List<DateTime?>());
                term2.AddRegistrationDates(regDate);
                List<Term> allTerms = new List<Term>() { term1, term2 };
                termRepoMock.Setup(repo => repo.Get()).Returns(allTerms);
                termRepoMock.Setup(repo => repo.GetAsync()).ReturnsAsync(allTerms);

                sectionRepoMock.Setup(repo => repo.GetSectionByGuidAsync(section1.Guid)).ReturnsAsync(section1);
                sectionRepoMock.Setup(repo => repo.GetSectionByGuidAsync(section2.Guid)).ReturnsAsync(section2);
                sectionRepoMock.Setup(repo => repo.GetSectionByGuidAsync(section3.Guid)).ReturnsAsync(section3);
                sectionRepoMock.Setup(repo => repo.GetSectionByGuidAsync(section4.Guid)).ReturnsAsync(section4);
                courseRepoMock.Setup(repo => repo.GetCourseGuidFromIdAsync(section1.CourseId)).ReturnsAsync(section1.Title);
                courseRepoMock.Setup(repo => repo.GetCourseGuidFromIdAsync(section2.CourseId)).ReturnsAsync(section2.Title);
                courseRepoMock.Setup(repo => repo.GetCourseGuidFromIdAsync(section3.CourseId)).ReturnsAsync(section3.Title);
                courseRepoMock.Setup(repo => repo.GetCourseGuidFromIdAsync(section4.CourseId)).ReturnsAsync(section4.Title);
                stuRefDataRepoMock.Setup(repo => repo.GetCreditCategoriesAsync()).ReturnsAsync(creditCategories);
                stuRefDataRepoMock.Setup(repo => repo.GetAcademicLevelsAsync()).ReturnsAsync(academicLevels);
                stuRefDataRepoMock.Setup(repo => repo.GetGradeSchemesAsync()).ReturnsAsync(gradeSchemes);
                stuRefDataRepoMock.Setup(repo => repo.GetCourseLevelsAsync()).ReturnsAsync(courseLevels);
                referenceDataRepoMock.Setup(repo => repo.DepartmentsAsync()).ReturnsAsync(departments);
                referenceDataRepoMock.Setup(repo => repo.Locations).Returns(locations);
                termRepoMock.Setup(repo => repo.GetAcademicPeriods(It.IsAny<IEnumerable<Domain.Student.Entities.Term>>())).Returns(academicPeriods);
                referenceDataRepoMock.Setup(repo => repo.GetInstructionalPlatformsAsync(It.IsAny<bool>())).ReturnsAsync(instructionalPlatforms);
                sectionRepoMock.Setup(repo => repo.GetSectionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new Tuple<IEnumerable<Domain.Student.Entities.Section>, int>(sections, 100));
                sectionRepoMock.Setup(repo => repo.GetSectionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(),
                    It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new Tuple<IEnumerable<Domain.Student.Entities.Section>, int>(sections, 100));

                //roomRepoMock.Setup(repo => repo.RoomsAsync()).ReturnsAsync(rooms);
                //referenceDataRepoMock.Setup(repo => repo.ScheduleRepeats).Returns(scheduleRepeats);

                // Mock the section service
                sectionCoordinationService = new SectionCoordinationService(adapterRegistry, sectionRepo, courseRepo, studentRepo, studentReferenceDataRepo,
                    referenceDataRepo, termRepo, studentConfigRepo, configRepo, personRepo, roomRepo, eventRepo, bookRepo, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionRepo = null;
                studentRepo = null;
                roleRepo = null;
                adapterRegistry = null;
                sectionCoordinationService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionService_SectionEmptyId_V8()
            {
                var r = await sectionCoordinationService.GetSection4ByGuidAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task SectionService_InvalidSectionId_V8()
            {
                var r = await sectionCoordinationService.GetSection4ByGuidAsync("9999999");
            }

            [TestMethod]
            public async Task SectionService_GetByGuid_V8()
            {
                var res = await sectionCoordinationService.GetSection4ByGuidAsync(section1.Guid);
                Assert.AreEqual(section1.Guid, res.Id);
            }

            [TestMethod]
            public async Task SectionService_GetByGuid_V8_capacity()
            {
                var res = await sectionCoordinationService.GetSection4ByGuidAsync(section1.Guid);
                Assert.AreEqual(10, res.MaximumEnrollment);
            }

            [TestMethod]
            public async Task SectionService_GetAll_EmptyArgument_V8()
            {
                var res = await sectionCoordinationService.GetSections4Async(0, 5);
                var sec = res.Item1.Where(r => r.Id == section1.Guid).FirstOrDefault();
                Assert.AreEqual(section1.Guid, sec.Id);
            }

            [TestMethod]
            public async Task SectionService_GetAll_V8()
            {
                sectionRepoMock.Setup(repo => repo.GetUnidataFormattedDate("2014-09-01T00:00:00+00:00")).ReturnsAsync("2014-09-01T00:00:00+00:00");
                sectionRepoMock.Setup(repo => repo.GetUnidataFormattedDate("2015-08-31T23:59:59+00:00")).ReturnsAsync("2015-08-31T23:59:59+00:00");
                sectionRepoMock.Setup(repo => repo.GetCourseIdFromGuidAsync(section1.Guid)).ReturnsAsync(section1.CourseId);
                sectionRepoMock.Setup(repo => repo.ConvertStatusToStatusCodeNoDefaultAsync("open")).ReturnsAsync("'A'");
                var res = await sectionCoordinationService.GetSections4Async(0, 5, "",
                    "2014-09-01T00:00:00+00:00", "2015-08-31T23:59:59+00:00", "", "", "", "", new List<string>(),
                    section1.Guid, "", "open", new List<string>());
                var sec = res.Item1.Where(r => r.Id == section1.Guid).FirstOrDefault();
                Assert.AreEqual(section1.Guid, sec.Id);
            }

            [TestMethod]
            public async Task SectionService_GetByGuidWithInactiveStatus_V8()
            {
                var res = await sectionCoordinationService.GetSection4ByGuidAsync(section2.Guid);
                Assert.AreEqual(section2.Guid, res.Id);
            }

            [TestMethod]
            public async Task SectionService_GetByGuidWithCancelledStatus_V8()
            {
                var res = await sectionCoordinationService.GetSection4ByGuidAsync(section3.Guid);
                Assert.AreEqual(section3.Guid, res.Id);
            }
            [TestMethod]
            public async Task SectionService_GetByGuidWithNoStatus_V8()
            {
                var res = await sectionCoordinationService.GetSection4ByGuidAsync(section4.Guid);
                Assert.AreEqual(section4.Guid, res.Id);
            }
            [TestMethod]
            public async Task SectionService_GetAll_V8_WithBadPeriodFilter_ReturnsEmptySet()
            {
                var emptyResult = new Tuple<IEnumerable<Section>, int>(new List<Domain.Student.Entities.Section>(), 0);
                sectionRepoMock.Setup(repo => repo.GetSectionsAsync(1, 1, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(emptyResult);
                var result = await sectionCoordinationService.GetSections4Async(1, 1, "", "", "", "", "", "", "badperiod", null, "", "", "", null);
                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Item1.Count());
                Assert.AreEqual(0, result.Item2);
            }
            [TestMethod]
            public async Task SectionService_GetAll_V8_WithBadSubjectFilter_ReturnsEmptySet()
            {
                var emptyResult = new Tuple<IEnumerable<Section>, int>(new List<Domain.Student.Entities.Section>(), 0);

                sectionRepoMock.Setup(repo => repo.GetSectionsAsync(1, 1, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(),
                It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(emptyResult);


                var result = await sectionCoordinationService.GetSections4Async(1, 1, "", "", "", "badsubject", "", "", "", null, "", "", "", null);
                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Item1.Count());
                Assert.AreEqual(0, result.Item2);
            }

            [TestMethod]
            public async Task SectionService_GetAll_V8_WithBadPlatformFilter_ReturnsEmptySet()
            {
                var emptyResult = new Tuple<IEnumerable<Section>, int>(new List<Domain.Student.Entities.Section>(), 0);
                sectionRepoMock.Setup(repo => repo.GetSectionsAsync(1, 1, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(emptyResult);
                var result = await sectionCoordinationService.GetSections4Async(1, 1, "", "", "", "", "", "badplatform", "", null, "", "", "", null);
                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Item1.Count());
                Assert.AreEqual(0, result.Item2);
            }
            [TestMethod]
            public async Task SectionService_GetAll_V8_WithBadLevelFilter_ReturnsEmptySet()
            {
                var emptyResult = new Tuple<IEnumerable<Section>, int>(new List<Domain.Student.Entities.Section>(), 0);
                sectionRepoMock.Setup(repo => repo.GetSectionsAsync(1, 1, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(emptyResult);
                var result = await sectionCoordinationService.GetSections4Async(1, 1, "", "", "", "", "", "", "", new List<string>() { "badlevel" }, "", "", "", null);
                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Item1.Count());
                Assert.AreEqual(0, result.Item2);
            }
            [TestMethod]
            public async Task SectionService_GetAll_V8_WithBadSiteFilter_ReturnsEmptySet()
            {
                var emptyResult = new Tuple<IEnumerable<Section>, int>(new List<Domain.Student.Entities.Section>(), 0);
                sectionRepoMock.Setup(repo => repo.GetSectionsAsync(1, 1, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(emptyResult);
                var result = await sectionCoordinationService.GetSections4Async(1, 1, "", "", "", "", "", "", "", null, "", "badsite", "", null);
                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Item1.Count());
                Assert.AreEqual(0, result.Item2);
            }
            [TestMethod]
            public async Task SectionService_GetAll_V8_WithBadStatusFilter_ReturnsEmptySet()
            {
                var emptyResult = new Tuple<IEnumerable<Section>, int>(new List<Domain.Student.Entities.Section>(), 0);
                sectionRepoMock.Setup(repo => repo.GetSectionsAsync(1, 1, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(emptyResult);
                var result = await sectionCoordinationService.GetSections4Async(1, 1, "", "", "", "", "", "", "", null, "", "", "badstatus", null);
                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Item1.Count());
                Assert.AreEqual(0, result.Item2);
            }
            [TestMethod]
            public async Task SectionService_GetAll_V8_WithBadOwningOrganizationFilter_ReturnsEmptySet()
            {
                var emptyResult = new Tuple<IEnumerable<Section>, int>(new List<Domain.Student.Entities.Section>(), 0);
                sectionRepoMock.Setup(repo => repo.GetSectionsAsync(1, 1, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(emptyResult);
                var result = await sectionCoordinationService.GetSections4Async(1, 1, "", "", "", "", "", "", "", null, "", "", "", new List<string>() { "badorg" });
                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Item1.Count());
                Assert.AreEqual(0, result.Item2);
            }

        }

        [TestClass]
        public class Sections_Get_V16 : CurrentUserSetup
        {
            private SectionCoordinationService sectionCoordinationService;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IStudentReferenceDataRepository> stuRefDataRepoMock;
            private IStudentReferenceDataRepository studentReferenceDataRepo;
            private Mock<IReferenceDataRepository> referenceDataRepoMock;
            private IReferenceDataRepository referenceDataRepo;
            private Mock<ICourseRepository> courseRepoMock;
            private ICourseRepository courseRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IStudentConfigurationRepository> studentConfigRepoMock;
            private IStudentConfigurationRepository studentConfigRepo;
            private Mock<IConfigurationRepository> configRepoMock;
            private IConfigurationRepository configRepo;
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IRoomRepository> roomRepoMock;
            private IRoomRepository roomRepo;
            private Mock<IEventRepository> eventRepoMock;
            private IEventRepository eventRepo;
            private Mock<IBookRepository> bookRepoMock;
            private IBookRepository bookRepo;

            // private Mock<IAcademicPeriodRepository> academicPeriodRepoMock;
            // private IAcademicPeriodRepository academicPeriodRepo;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ILogger logger;
            private ICurrentUserFactory currentUserFactory;

            private Domain.Student.Entities.Section section1;
            private Domain.Student.Entities.Section section2;
            private Domain.Student.Entities.Section section3;
            private Domain.Student.Entities.Section section4;
            private List<Domain.Student.Entities.Section> sections;
            private Domain.Student.Entities.SectionMeeting meeting1;
            private Domain.Student.Entities.SectionMeeting meeting2;
            private Domain.Student.Entities.SectionMeeting meeting3;
            private Domain.Student.Entities.SectionMeeting meeting4;

            private List<OfferingDepartment> dpts = new List<OfferingDepartment>() { new OfferingDepartment("ART", 100m) };
            private List<string> levels = new List<string>() { "Whatever" };
            private List<SectionStatusItem> statuses = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-60)) };
            private List<SectionStatusItem> statuses2 = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Inactive, "I", DateTime.Today.AddDays(-60)) };
            private List<SectionStatusItem> statuses3 = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Cancelled, "C", DateTime.Today.AddDays(-60)) };
            private List<SectionStatusItem> statuses4 = new List<SectionStatusItem>() { new SectionStatusItem(new SectionStatus(), "N/A", DateTime.Today.AddDays(-60)) };
            private List<CreditCategory> creditCategories = new List<CreditCategory>() { new CreditCategory(Guid.NewGuid().ToString(), "IN", "Regular Credit", CreditType.Institutional) };
            string guid = "9C3B805D-CFE6-483B-86C3-4C20562F8C15";
            private List<Domain.Student.Entities.AcademicLevel> academicLevels = new List<Domain.Student.Entities.AcademicLevel>() { new Domain.Student.Entities.AcademicLevel("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "UG", "Undergraduate") };
            private List<Domain.Student.Entities.GradeScheme> gradeSchemes = new List<Domain.Student.Entities.GradeScheme>() { new Domain.Student.Entities.GradeScheme("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "Pass/Fail", "Pass/Fail grade") };
            private List<Domain.Student.Entities.CourseLevel> courseLevels = new List<Domain.Student.Entities.CourseLevel>() { new Domain.Student.Entities.CourseLevel("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "100", "Introduction to terms, concepts, and techniques.") };
            private List<Department> departments = new List<Department>() { new Department("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "PHYS", "Physics", true) };
            private List<Location> locations = new List<Location>() { new Location("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "MD", "Maryland") };
            private List<AcademicPeriod> academicPeriods = new List<AcademicPeriod>()
                {
                    new AcademicPeriod(Guid.NewGuid().ToString(), "2012/FA", "Fall 2012", DateTime.Today.AddDays(-60), DateTime.Today, 2012, 4, "Fall", Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), null),
                    new AcademicPeriod(Guid.NewGuid().ToString(), "2011/FA", "Fall 2011", DateTime.Today.AddDays(-60), DateTime.Today, 2011, 4, "Fall", Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), null)
                };
            private List<ChargeAssessmentMethod> chargeAssessmentMethods = new List<ChargeAssessmentMethod>()
                {
                    new ChargeAssessmentMethod("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "BM1", "Desc1")
                };
            private List<AdministrativeInstructionalMethod> administrativeInstructionalMethods = new List<Domain.Student.Entities.AdministrativeInstructionalMethod>()
                {
                    new AdministrativeInstructionalMethod("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "LG", "LG", "9C3B805D-CFE6-483B-86C3-4C20562F8C15")
                };
            private List<Domain.Base.Entities.InstructionalPlatform> instructionalPlatforms = new List<Domain.Base.Entities.InstructionalPlatform>() { new Domain.Base.Entities.InstructionalPlatform(Guid.NewGuid().ToString(), "Ellucian", "IP") };
            private List<Domain.Student.Entities.InstructionalMethod> instrMethodEntities = new List<Domain.Student.Entities.InstructionalMethod>()
                {
                    new Domain.Student.Entities.InstructionalMethod("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "LG", "LG", false),
                    new Domain.Student.Entities.InstructionalMethod("73244057-D1EC-4094-A0B7-DE602533E3A6", "30", "30", true),
                    new Domain.Student.Entities.InstructionalMethod("1df164eb-8178-4321-a9f7-24f12d3991d8", "04", "04", false)
                };

            private string meeting1Guid = Guid.NewGuid().ToString();
            private string sectionGuid = Guid.NewGuid().ToString();
            private string roomGuid = Guid.NewGuid().ToString();
            private string courseGuid = Guid.NewGuid().ToString();

            [TestInitialize]
            public void Initialize()
            {
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                roomRepoMock = new Mock<IRoomRepository>();
                roomRepo = roomRepoMock.Object;
                stuRefDataRepoMock = new Mock<IStudentReferenceDataRepository>();
                studentReferenceDataRepo = stuRefDataRepoMock.Object;
                referenceDataRepoMock = new Mock<IReferenceDataRepository>();
                referenceDataRepo = referenceDataRepoMock.Object;
                courseRepoMock = new Mock<ICourseRepository>();
                courseRepo = courseRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                studentConfigRepoMock = new Mock<IStudentConfigurationRepository>();
                studentConfigRepo = studentConfigRepoMock.Object;
                configRepoMock = new Mock<IConfigurationRepository>();
                configRepo = configRepoMock.Object;
                eventRepoMock = new Mock<IEventRepository>();
                eventRepo = eventRepoMock.Object;
                bookRepoMock = new Mock<IBookRepository>();
                bookRepo = bookRepoMock.Object;

                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;

                // "STU1" is the current user
                currentUserFactory = new CurrentUserSetup.StudentUserFactory();

                logger = new Mock<ILogger>().Object;

                // Mock the section repo response
                section1 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC1", "1119", "01", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses, true, allowWaitlist: true);
                section1.Guid = sectionGuid;
                section1.TermId = "2012/FA";
                section1.EndDate = new DateTime(2012, 12, 21);
                section1.LearningProvider = "Ellucian";
                section1.Location = "Loc1";
                section1.BillingCred = 3;
                section1.SectionCapacity = 10;
                section1.GlobalCapacity = 20;
                section1.NumberOfWeeks = 2;
                section1.AddCourseType("Course 1");
                section1.AddInstructionalContact(new InstructionalContact("LG") { ClockHours = 10, ContactHours = 10, ContactMeasure = "Code 1", Load = 10 });
                section1.AddInstructionalMethod("LG");
                section1.BillingMethod = "BM1";
                section1.WaitListNumberOfDays = 10;
                section1.Comments = "comments from section1";
                section1.GradeSchemeCode = "GRADE_SCHEME";

                section2 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC2", "1119", "02", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses2, true);
                section2.Guid = Guid.NewGuid().ToString();
                section2.TermId = "2012/FA";
                section1.Location = "Loc2";
                section2.BillingCred = 4;
                section2.Comments = "comments from section2";

                section3 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC3", "1119", "03", new DateTime(2011, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses3, true);
                section3.Guid = Guid.NewGuid().ToString();
                section3.TermId = "2011/FA";
                section3.EndDate = new DateTime(2011, 12, 21);
                section3.BillingCred = 3;
                section3.CensusDates = new List<DateTime?>() { new DateTime(2011, 09, 10) };
                section3.Comments = "comments from section3";

                section4 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC4", "1119", "04", new DateTime(2011, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses4, true);
                section4.Guid = Guid.NewGuid().ToString();
                section4.TermId = "2011/FA";
                section4.EndDate = new DateTime(2011, 12, 21);
                section4.Location = "Loc1";
                section4.Comments = "comments from section4";

                var sectionFaculty = new Domain.Student.Entities.SectionFaculty("1234", "SEC1", "0000678", "LEC", new DateTime(2012, 09, 01), new DateTime(2011, 12, 21), 100.0m);

                meeting1 = new Domain.Student.Entities.SectionMeeting("123", "SEC1", "LEC", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "W");
                meeting1.Guid = meeting1Guid;
                meeting1.AddFacultyId("0000678");
                meeting1.AddSectionFaculty(sectionFaculty);
                meeting1.StartTime = new DateTimeOffset(2012, 01, 01, 08, 30, 00, new TimeSpan());
                meeting1.EndTime = new DateTimeOffset(2012, 12, 21, 09, 30, 00, new TimeSpan());
                meeting1.Room = "DAN*102";
                section1.AddSectionMeeting(meeting1);

                meeting2 = new Domain.Student.Entities.SectionMeeting("123", "SEC1", "LEC", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "D");
                meeting2.Guid = meeting1Guid;

                meeting3 = new Domain.Student.Entities.SectionMeeting("456", "SEC1", "LEC", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "M");
                meeting3.Guid = meeting1Guid;

                meeting4 = new Domain.Student.Entities.SectionMeeting("789", "SEC1", "LEC", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "Y");
                meeting4.Guid = meeting1Guid;

                sections = new List<Domain.Student.Entities.Section>();
                //var section = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC1", "1119", "01", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses, true);
                sections.Add(section1);
                sections.Add(section2);
                sections.Add(section3);
                sections.Add(section4);

                var rooms = new List<Domain.Base.Entities.Room>();
                var room = new Domain.Base.Entities.Room(roomGuid, "DAN*102", "Danville Hall Room 102");
                rooms.Add(room);

                var scheduleRepeats = new List<Domain.Base.Entities.ScheduleRepeat>();
                var scheduleRepeat = new Domain.Base.Entities.ScheduleRepeat("W", "Weekly", "7", FrequencyType.Weekly);
                scheduleRepeats.Add(scheduleRepeat);
                scheduleRepeat = new Domain.Base.Entities.ScheduleRepeat("D", "Daily", "1", FrequencyType.Daily);
                scheduleRepeats.Add(scheduleRepeat);
                scheduleRepeat = new Domain.Base.Entities.ScheduleRepeat("M", "Monthly", "30", FrequencyType.Monthly);
                scheduleRepeats.Add(scheduleRepeat);
                scheduleRepeat = new Domain.Base.Entities.ScheduleRepeat("Y", "Yearly", "365", FrequencyType.Yearly);
                scheduleRepeats.Add(scheduleRepeat);

                List<string> sec1List = new List<string>() { "SEC1" };
                List<Domain.Student.Entities.Section> sec1Resp = new List<Domain.Student.Entities.Section>() { section1 };
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(sec1List, false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sec1Resp));

                List<string> sec2List = new List<string>() { "SEC2" };
                List<Domain.Student.Entities.Section> sec2Resp = new List<Domain.Student.Entities.Section>() { section2 };
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(sec2List, false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sec2Resp));

                List<string> sec3List = new List<string>() { "SEC3" };
                List<Domain.Student.Entities.Section> sec3Resp = new List<Domain.Student.Entities.Section>() { section3 };
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(sec3List, false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sec3Resp));

                List<string> sec4List = new List<string>() { "9999999" };
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(sec4List, false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(new List<Domain.Student.Entities.Section>()));

                Term term1 = new Term("termGuid1", "2011/FA", "2011 Fall", new DateTime(2011, 09, 01), new DateTime(2011, 12, 15), 2012,
                    0, false, false, "2011/FA", false);
                var date1 = new List<DateTime?>() { new DateTime(2011, 09, 15) };
                var regDate = new RegistrationDate("Loc1", null, null, null, null, null, null, null, null, null, date1);
                term1.AddRegistrationDates(regDate);
                Term term2 = new Term("termGuid2", "2012/FA", "2012 Fall", new DateTime(2012, 09, 01), new DateTime(2012, 12, 15), 2013,
                    0, false, false, "2012/FA", false);
                regDate = new RegistrationDate("Loc2", null, null, null, null, null, null, null, null, null, new List<DateTime?>());
                term2.AddRegistrationDates(regDate);
                var allTerms = new List<Term>() { term1, term2 };
                termRepoMock.Setup(repo => repo.Get()).Returns(allTerms);
                termRepoMock.Setup(repo => repo.GetAsync()).ReturnsAsync(allTerms);

                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(section1.Guid, It.IsAny<bool>())).ReturnsAsync(section1);
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(section2.Guid, It.IsAny<bool>())).ReturnsAsync(section2);
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(section3.Guid, It.IsAny<bool>())).ReturnsAsync(section3);
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(section4.Guid, It.IsAny<bool>())).ReturnsAsync(section4);
                courseRepoMock.Setup(repo => repo.GetCourseGuidFromIdAsync(section1.CourseId)).ReturnsAsync(section1.Title);
                courseRepoMock.Setup(repo => repo.GetCourseGuidFromIdAsync(section2.CourseId)).ReturnsAsync(section2.Title);
                courseRepoMock.Setup(repo => repo.GetCourseGuidFromIdAsync(section3.CourseId)).ReturnsAsync(section3.Title);
                courseRepoMock.Setup(repo => repo.GetCourseGuidFromIdAsync(section4.CourseId)).ReturnsAsync(section4.Title);
                stuRefDataRepoMock.Setup(repo => repo.GetCreditCategoriesAsync()).ReturnsAsync(creditCategories);
                stuRefDataRepoMock.Setup(repo => repo.GetAcademicLevelsAsync()).ReturnsAsync(academicLevels);

                stuRefDataRepoMock.Setup(repo => repo.GetGradeSchemesAsync()).ReturnsAsync(gradeSchemes);
                stuRefDataRepoMock.Setup(repo => repo.GetCourseLevelsAsync()).ReturnsAsync(courseLevels);
                referenceDataRepoMock.Setup(repo => repo.DepartmentsAsync()).ReturnsAsync(departments);
                referenceDataRepoMock.Setup(repo => repo.Locations).Returns(locations);
                termRepoMock.Setup(repo => repo.GetAcademicPeriods(It.IsAny<IEnumerable<Domain.Student.Entities.Term>>())).Returns(academicPeriods);
                referenceDataRepoMock.Setup(repo => repo.GetInstructionalPlatformsAsync(It.IsAny<bool>())).ReturnsAsync(instructionalPlatforms);
                sectionRepoMock.Setup(repo => repo.GetSectionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new Tuple<IEnumerable<Domain.Student.Entities.Section>, int>(sections, sections.Count()));
                sectionRepoMock.Setup(repo => repo.GetSectionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(),
                    It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new Tuple<IEnumerable<Domain.Student.Entities.Section>, int>(sections, sections.Count()));
                sectionRepoMock.Setup(repo => repo.GetSections2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(),
                    It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new Tuple<IEnumerable<Domain.Student.Entities.Section>, int>(sections, sections.Count()));

                var sectionTitleTypes = new List<Domain.Student.Entities.SectionTitleType>()
                {
                    new Domain.Student.Entities.SectionTitleType("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "short", "Descr"),
                    new Domain.Student.Entities.SectionTitleType("8C3B805D-CFE6-483B-86C3-4C20562F8C15", "long", "Descr")
                };
                stuRefDataRepoMock.Setup(repo => repo.GetSectionTitleTypesAsync(It.IsAny<bool>())).ReturnsAsync(sectionTitleTypes);
                stuRefDataRepoMock.Setup(repo => repo.GetSectionTitleTypesGuidAsync(It.IsAny<string>())).ReturnsAsync("9C3B805D-CFE6-483B-86C3-4C20562F8C15");


                var sectionDescTypes = new List<Domain.Student.Entities.SectionDescriptionType>()
                {
                    new Domain.Student.Entities.SectionDescriptionType("8D3B807D-CFE6-483B-86C3-4C20562F8C15", "printed", "Descr")
                };
                stuRefDataRepoMock.Setup(repo => repo.GetSectionDescriptionTypesAsync(It.IsAny<bool>())).ReturnsAsync(sectionDescTypes);
                stuRefDataRepoMock.Setup(repo => repo.GetSectionDescriptionTypesGuidAsync(It.IsAny<string>())).ReturnsAsync("8D3B807D-CFE6-483B-86C3-4C20562F8C15");

                sectionRepoMock.Setup(repo => repo.GetStatusCodesWithGuidsAsync()).ReturnsAsync(new List<SectionStatusCodeGuid>() { new SectionStatusCodeGuid(Guid.NewGuid().ToString(), "A", "Active") });

                var _allDepartments = new List<Domain.Base.Entities.Department>()
                {
                    new Domain.Base.Entities.Department("1C3B805D-CFE6-483B-86C3-4C20562F8C14", "ART", "Descr 1", true)
                };
                referenceDataRepoMock.Setup(repo => repo.GetDepartmentsAsync(It.IsAny<bool>())).ReturnsAsync(_allDepartments);

                var courseTypes = new List<Domain.Student.Entities.CourseType>()
                {
                    new Domain.Student.Entities.CourseType("1C3B805D-CFE6-483B-86C3-4C20562F8C14", "Course 1", "Descr 1", true)
                };
                stuRefDataRepoMock.Setup(repo => repo.GetCourseTypesAsync(It.IsAny<bool>())).ReturnsAsync(courseTypes);
                stuRefDataRepoMock.Setup(repo => repo.GetInstructionalMethodsAsync(It.IsAny<bool>())).ReturnsAsync(instrMethodEntities);
                stuRefDataRepoMock.Setup(repo => repo.GetAdministrativeInstructionalMethodsAsync(It.IsAny<bool>())).ReturnsAsync(administrativeInstructionalMethods);

                var contactMeasures = new List<Domain.Student.Entities.ContactMeasure>()
                {
                    new Domain.Student.Entities.ContactMeasure("Code 1", "Descr 1", "PT1")
                };
                stuRefDataRepoMock.Setup(repo => repo.GetContactMeasuresAsync(It.IsAny<bool>())).ReturnsAsync(contactMeasures);
                stuRefDataRepoMock.Setup(repo => repo.GetChargeAssessmentMethodsAsync(It.IsAny<bool>())).ReturnsAsync(chargeAssessmentMethods);

                // Mock the section service
                sectionCoordinationService = new SectionCoordinationService(adapterRegistry, sectionRepo, courseRepo, studentRepo, studentReferenceDataRepo,
                    referenceDataRepo, termRepo, studentConfigRepo, configRepo, personRepo, roomRepo, eventRepo, bookRepo, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionRepo = null;
                studentRepo = null;
                roleRepo = null;
                adapterRegistry = null;
                sectionCoordinationService = null;
            }

            [TestMethod]
            public async Task SectionService_GetSections6Async_BadStartOnDate()
            {
                sectionRepoMock.Setup(repo => repo.GetUnidataFormattedDate(It.IsAny<string>())).ThrowsAsync(new ArgumentException());
                var r = await sectionCoordinationService.GetSections6Async(It.IsAny<int>(), It.IsAny<int>(), "", "badDate");
                Assert.AreEqual(false, r.Item1.Any());
                Assert.AreEqual(0, r.Item2);
            }

            [TestMethod]
            public async Task SectionService_GetSections6Async_BadEndOnDate()
            {
                sectionRepoMock.Setup(repo => repo.GetUnidataFormattedDate(It.IsAny<string>())).ThrowsAsync(new ArgumentException());
                var r = await sectionCoordinationService.GetSections6Async(It.IsAny<int>(), It.IsAny<int>(), "", "", "badDate");
                Assert.AreEqual(false, r.Item1.Any());
                Assert.AreEqual(0, r.Item2);
            }

            [TestMethod]
            public async Task SectionService_GetSections6Async_InstructionalPlatform()
            {
                referenceDataRepoMock.Setup(repo => repo.GetInstructionalPlatformsAsync(It.IsAny<bool>())).ReturnsAsync(() => null);
                var r = await sectionCoordinationService.GetSections6Async(It.IsAny<int>(), It.IsAny<int>(), "", "", "", "", "", "1");
                Assert.AreEqual(false, r.Item1.Any());
                Assert.AreEqual(0, r.Item2);
            }

            [TestMethod]
            public async Task SectionService_GetSections6Async_InstructionalPlatform_IdNull()
            {
                var r = await sectionCoordinationService.GetSections6Async(It.IsAny<int>(), It.IsAny<int>(), "", "", "", "", "", null);
                Assert.AreEqual(false, r.Item1.Any());
                Assert.AreEqual(0, r.Item2);
            }

            [TestMethod]
            public async Task SectionService_GetSections6Async_InstructionalPlatform_BadId()
            {
                var r = await sectionCoordinationService.GetSections6Async(It.IsAny<int>(), It.IsAny<int>(), "", "", "", "", "", "badId");
                Assert.AreEqual(false, r.Item1.Any());
                Assert.AreEqual(0, r.Item2);
            }

            [TestMethod]
            public async Task SectionService_GetSections6Async_AcademicPeriod_BadId()
            {
                var r = await sectionCoordinationService.GetSections6Async(It.IsAny<int>(), It.IsAny<int>(), "", "", "", "", "", "", "badId");
                Assert.AreEqual(false, r.Item1.Any());
                Assert.AreEqual(0, r.Item2);
            }

            [TestMethod]
            public async Task SectionService_GetSections6Async_Course_BadId()
            {
                var r = await sectionCoordinationService.GetSections6Async(It.IsAny<int>(), It.IsAny<int>(), "", "", "", "", "", "", "", "", null, "badId");
                Assert.AreEqual(false, r.Item1.Any());
                Assert.AreEqual(0, r.Item2);
            }

            [TestMethod]
            public async Task SectionService_GetSections6Async_Site_BadId()
            {
                var r = await sectionCoordinationService.GetSections6Async(It.IsAny<int>(), It.IsAny<int>(), "", "", "", "", "", "", "", "", null, "", "badId");
                Assert.AreEqual(false, r.Item1.Any());
                Assert.AreEqual(0, r.Item2);
            }

            [TestMethod]
            public async Task SectionService_GetSections6Async_Status_BadId()
            {
                var r = await sectionCoordinationService.GetSections6Async(It.IsAny<int>(), It.IsAny<int>(), "", "", "", "", "", "", "", "", null, "", "", "badId");
                Assert.AreEqual(false, r.Item1.Any());
                Assert.AreEqual(0, r.Item2);
            }

            [TestMethod]
            public async Task SectionService_GetSections6Async_Instructor_BadId()
            {
                var r = await sectionCoordinationService.GetSections6Async(It.IsAny<int>(), It.IsAny<int>(), "", "", "", "", "", "", "", "", null, "", "", "", null, "", "badId");
                Assert.AreEqual(false, r.Item1.Any());
                Assert.AreEqual(0, r.Item2);
            }

            [TestMethod]
            public async Task SectionService_GetSections6Async_Subject_BadId()
            {
                var r = await sectionCoordinationService.GetSections6Async(It.IsAny<int>(), It.IsAny<int>(), "", "", "", "", "", "", "", "", null, "", "", "", null, Guid.NewGuid().ToString());
                Assert.AreEqual(false, r.Item1.Any());
                Assert.AreEqual(0, r.Item2);
            }

            [TestMethod]
            public async Task SectionService_GetSections6Async_AcademicLevel_BadId()
            {
                var r = await sectionCoordinationService.GetSections6Async(It.IsAny<int>(), It.IsAny<int>(), "", "", "", "", "", "", "", "", new List<string>() { "badId" });
                Assert.AreEqual(false, r.Item1.Any());
                Assert.AreEqual(0, r.Item2);
            }

            [TestMethod]
            public async Task SectionService_GetSections6Async_OwningOrganization_BadId()
            {
                var r = await sectionCoordinationService.GetSections6Async(It.IsAny<int>(), It.IsAny<int>(), "", "", "", "", "", "", "", "", null, "", "", "", new List<string>() { "badId" });
                Assert.AreEqual(false, r.Item1.Any());
                Assert.AreEqual(0, r.Item2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_GetSections6Async()
            {
                var results = await sectionCoordinationService.GetSections6Async(It.IsAny<int>(), It.IsAny<int>());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionService_GetSections6Async_ArgumentNullException()
            {
                sectionRepoMock.Setup(repo => repo.GetSections2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(),
                    It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new ArgumentNullException());
                var results = await sectionCoordinationService.GetSections6Async(It.IsAny<int>(), It.IsAny<int>());
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task SectionService_GetSections6Async_Exception()
            {
                sectionRepoMock.Setup(repo => repo.GetSections2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(),
                    It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new Exception());
                var results = await sectionCoordinationService.GetSections6Async(It.IsAny<int>(), It.IsAny<int>());
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_GetSections6Async_ArgumentException()
            {
                stuRefDataRepoMock.Setup(repo => repo.GetSectionTitleTypesGuidAsync(It.IsAny<string>())).ThrowsAsync(new RepositoryException());
                var results = await sectionCoordinationService.GetSections6Async(It.IsAny<int>(), It.IsAny<int>());
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_GetSections6Async_All_IntegrationApiExceptions()
            {
                section1.Statuses.FirstOrDefault().IntegrationStatus = SectionStatusIntegration.NotSet;
                stuRefDataRepoMock.Setup(repo => repo.GetSectionDescriptionTypesGuidAsync(It.IsAny<string>())).ThrowsAsync(new RepositoryException());
                stuRefDataRepoMock.Setup(repo => repo.GetAcademicLevelsGuidAsync(It.IsAny<string>())).ThrowsAsync(new RepositoryException());
                stuRefDataRepoMock.Setup(repo => repo.GetGradeSchemeGuidAsync(It.IsAny<string>())).ThrowsAsync(new RepositoryException());
                stuRefDataRepoMock.Setup(repo => repo.GetCourseLevelGuidAsync(It.IsAny<string>())).ThrowsAsync(new RepositoryException());
                stuRefDataRepoMock.Setup(repo => repo.GetCourseTypeGuidAsync(It.IsAny<string>())).ThrowsAsync(new RepositoryException());
                stuRefDataRepoMock.Setup(repo => repo.GetAdministrativeInstructionalMethodGuidAsync(It.IsAny<string>())).ThrowsAsync(new RepositoryException());
                stuRefDataRepoMock.Setup(repo => repo.GetInstructionalMethodGuidAsync(It.IsAny<string>())).ThrowsAsync(new RepositoryException());
                termRepoMock.Setup(repo => repo.GetAcademicPeriodsGuidAsync(It.IsAny<string>())).ThrowsAsync(new RepositoryException());
                referenceDataRepoMock.Setup(repo => repo.GetInstructionalPlatformsGuidAsync(It.IsAny<string>())).ThrowsAsync(new RepositoryException());
                referenceDataRepoMock.Setup(repo => repo.GetLocationsGuidAsync(It.IsAny<string>())).ThrowsAsync(new RepositoryException());
                referenceDataRepoMock.Setup(repo => repo.GetDepartments2GuidAsync(It.IsAny<string>())).ThrowsAsync(new RepositoryException());
                termRepoMock.Setup(repo => repo.GetAsync()).ThrowsAsync(new Exception());
                sectionRepoMock.Setup(repo => repo.GetStatusCodesWithGuidsAsync()).ThrowsAsync(new ArgumentNullException());
                courseRepoMock.Setup(repo => repo.GetCourseGuidFromIdAsync(It.IsAny<string>())).ThrowsAsync(new ArgumentException());

                var results = await sectionCoordinationService.GetSections6Async(It.IsAny<int>(), It.IsAny<int>());
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task SectionService_GetSection6ByGuidAsync_KeyNotFoundException()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(() => null);
                var result = await sectionCoordinationService.GetSection6ByGuidAsync(section1.Guid);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionService_GetSection6ByGuidAsync_ArgumentNullException()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(() => null);
                var result = await sectionCoordinationService.GetSection6ByGuidAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_GetSection6ByGuidAsync_IntegrationApiException()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new IntegrationApiException());
                var result = await sectionCoordinationService.GetSection6ByGuidAsync(section1.Guid);
            }

            [TestMethod]
            public async Task SectionService_GetSection6ByGuidAsync()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(section1);
                var result = await sectionCoordinationService.GetSection6ByGuidAsync(section1.Guid);
                Assert.IsNotNull(result);
            }
        }

        [TestClass]
        public class Sections_Post_V8 : CurrentUserSetup
        {
            private SectionCoordinationService sectionCoordinationService;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IStudentReferenceDataRepository> stuRefDataRepoMock;
            private IStudentReferenceDataRepository studentReferenceDataRepo;
            private Mock<IReferenceDataRepository> referenceDataRepoMock;
            private IReferenceDataRepository referenceDataRepo;
            private Mock<ICourseRepository> courseRepoMock;
            private ICourseRepository courseRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IStudentConfigurationRepository> studentConfigRepoMock;
            private IStudentConfigurationRepository studentConfigRepo;
            private Mock<IConfigurationRepository> configRepoMock;
            private IConfigurationRepository configRepo;
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IRoomRepository> roomRepoMock;
            private IRoomRepository roomRepo;
            private Mock<IEventRepository> eventRepoMock;
            private IEventRepository eventRepo;
            private Mock<IBookRepository> bookRepoMock;
            private IBookRepository bookRepo;


            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ILogger logger;
            private ICurrentUserFactory currentUserFactory;

            private Domain.Student.Entities.Section section1;
            private Domain.Student.Entities.Section section2;
            private Domain.Student.Entities.Section section3;
            private Domain.Student.Entities.SectionMeeting meeting1;
            private Domain.Student.Entities.SectionMeeting meeting2;
            private Domain.Student.Entities.SectionMeeting meeting3;
            private Domain.Student.Entities.SectionMeeting meeting4;

            private List<OfferingDepartment> dpts = new List<OfferingDepartment>() { new OfferingDepartment("ART", 100m) };
            private List<string> levels = new List<string>() { "Whatever" };
            private List<SectionStatusItem> statuses = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-60)) };
            private List<SectionStatusItem> statuses2 = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Inactive, "I", DateTime.Today.AddDays(-60)) };
            private List<SectionStatusItem> statuses3 = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Cancelled, "C", DateTime.Today.AddDays(-60)) };
            private IEnumerable<Domain.Student.Entities.Course> allCourses = new TestCourseRepository().GetAsync().Result;

            private List<CreditCategory> creditCategories = new List<CreditCategory>() { new CreditCategory(Guid.NewGuid().ToString(), "RC", "Regular Credit", CreditType.Institutional),
                new CreditCategory(Guid.NewGuid().ToString(), "RC", "Regular Credit", CreditType.Transfer), new CreditCategory(Guid.NewGuid().ToString(), "RC", "Regular Credit", CreditType.ContinuingEducation ) };
            private List<Domain.Student.Entities.AcademicLevel> academicLevels = new List<Domain.Student.Entities.AcademicLevel>() { new Domain.Student.Entities.AcademicLevel(Guid.NewGuid().ToString(), "UG", "Undergraduate") };
            private List<Domain.Student.Entities.GradeScheme> gradeSchemes = new List<Domain.Student.Entities.GradeScheme>() { new Domain.Student.Entities.GradeScheme(Guid.NewGuid().ToString(), "Pass/Fail", "Pass/Fail grade") };
            private List<Domain.Student.Entities.CourseLevel> courseLevels = new List<Domain.Student.Entities.CourseLevel>() { new Domain.Student.Entities.CourseLevel(Guid.NewGuid().ToString(), "100", "Introduction to terms, concepts, and techniques.") };
            private List<Department> departments = new List<Department>() { new Department(Guid.NewGuid().ToString(), "PHYS", "Physics", true) };
            private List<Location> locations = new List<Location>() { new Location(Guid.NewGuid().ToString(), "MD", "Maryland") };

            private string meeting1Guid = Guid.NewGuid().ToString();
            private string instructionalMethodGuid = Guid.NewGuid().ToString();
            private string roomGuid = Guid.NewGuid().ToString();
            private Domain.Entities.Permission permissionCreateandUpdateSections;

            [TestInitialize]
            public void Initialize()
            {
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                roomRepoMock = new Mock<IRoomRepository>();
                roomRepo = roomRepoMock.Object;
                stuRefDataRepoMock = new Mock<IStudentReferenceDataRepository>();
                studentReferenceDataRepo = stuRefDataRepoMock.Object;
                referenceDataRepoMock = new Mock<IReferenceDataRepository>();
                referenceDataRepo = referenceDataRepoMock.Object;
                courseRepoMock = new Mock<ICourseRepository>();
                courseRepo = courseRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                studentConfigRepoMock = new Mock<IStudentConfigurationRepository>();
                studentConfigRepo = studentConfigRepoMock.Object;
                configRepoMock = new Mock<IConfigurationRepository>();
                configRepo = configRepoMock.Object;
                eventRepoMock = new Mock<IEventRepository>();
                eventRepo = eventRepoMock.Object;
                bookRepoMock = new Mock<IBookRepository>();
                bookRepo = bookRepoMock.Object;


                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;

                // Set Current User
                currentUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();

                logger = new Mock<ILogger>().Object;

                // Mock permissions
                permissionCreateandUpdateSections = new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateAndUpdateSection);
                thirdPartyRole.AddPermission(permissionCreateandUpdateSections);
                //permissionViewAnyProjects = new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewAnyProjects);
                //thirdPartyRole.AddPermission(permissionViewAnyProjects);
                //permissionViewAnyProjectsLineItems = new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.ViewAnyProjectsLineItems);
                //thirdPartyRole.AddPermission(permissionViewAnyProjectsLineItems);

                // Mock the section repo response
                section1 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC1", "1119", "01", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses, true);
                section1.TermId = "2012/FA";
                section1.EndDate = new DateTime(2012, 12, 21);
                section1.Guid = Guid.NewGuid().ToString();

                section2 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC2", "1119", "02", new DateTime(2012, 09, 01), 3.0m, null, "Introduction to Art", "IN", dpts, levels, "UG", statuses2, true);
                section2.TermId = "2012/FA";
                section2.Guid = Guid.NewGuid().ToString();

                section3 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC3", "1119", "03", new DateTime(2011, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses3, true);
                section3.TermId = "2011/FA";
                section3.EndDate = new DateTime(2011, 12, 21);
                section3.Guid = Guid.NewGuid().ToString();

                var sectionFaculty = new Domain.Student.Entities.SectionFaculty("1234", "SEC1", "0000678", "LEC", new DateTime(2012, 09, 01), new DateTime(2011, 12, 21), 100.0m);

                meeting1 = new Domain.Student.Entities.SectionMeeting("123", "SEC1", "LEC", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "W");
                meeting1.Guid = meeting1Guid;
                meeting1.AddFacultyId("0000678");
                meeting1.AddSectionFaculty(sectionFaculty);
                meeting1.StartTime = new DateTimeOffset(2012, 01, 01, 08, 30, 00, new TimeSpan());
                meeting1.EndTime = new DateTimeOffset(2012, 12, 21, 09, 30, 00, new TimeSpan());
                meeting1.Room = "DAN*102";
                section1.AddSectionMeeting(meeting1);

                meeting2 = new Domain.Student.Entities.SectionMeeting("123", "SEC1", "LEC", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "D");
                meeting2.Guid = meeting1Guid;

                meeting3 = new Domain.Student.Entities.SectionMeeting("456", "SEC1", "LEC", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "M");
                meeting3.Guid = meeting1Guid;

                meeting4 = new Domain.Student.Entities.SectionMeeting("789", "SEC1", "LEC", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "Y");
                meeting4.Guid = meeting1Guid;

                Term term1 = new Term("termGuid1", "2011/FA", "2011 Fall", new DateTime(2011, 09, 01), new DateTime(2011, 12, 15), 2012,
                    0, false, false, "2011/FA", false);
                var date1 = new List<DateTime?>() { new DateTime(2011, 09, 15) };
                var regDate = new RegistrationDate("Loc1", null, null, null, null, null, null, null, null, null, date1);
                term1.AddRegistrationDates(regDate);
                Term term2 = new Term("termGuid2", "2012/FA", "2012 Fall", new DateTime(2012, 09, 01), new DateTime(2012, 12, 15), 2013,
                    0, false, false, "2012/FA", false);
                regDate = new RegistrationDate("Loc2", null, null, null, null, null, null, null, null, null, new List<DateTime?>());
                term2.AddRegistrationDates(regDate);
                List<Term> allTerms = new List<Term>() { term1, term2 };
                termRepoMock.Setup(repo => repo.Get()).Returns(allTerms);
                termRepoMock.Setup(repo => repo.GetAsync()).ReturnsAsync(allTerms);

                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(section1.Guid, It.IsAny<bool>())).ReturnsAsync(section1);
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(section2.Guid, It.IsAny<bool>())).ReturnsAsync(section2);
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(section3.Guid, It.IsAny<bool>())).ReturnsAsync(section3);
                sectionRepoMock.Setup(repo => repo.PostSectionMeetingAsync(It.IsAny<Domain.Student.Entities.Section>(), meeting1.Guid)).ReturnsAsync(meeting1);
                sectionRepoMock.Setup(repo => repo.PostSectionMeetingAsync(It.IsAny<Domain.Student.Entities.Section>(), meeting2.Guid)).ReturnsAsync(meeting2);
                sectionRepoMock.Setup(repo => repo.PostSectionMeetingAsync(It.IsAny<Domain.Student.Entities.Section>(), meeting3.Guid)).ReturnsAsync(meeting3);
                courseRepoMock.Setup(repo => repo.GetAsync()).Returns(Task.FromResult(allCourses));
                var course_temp = allCourses.Where(c => c.Id == "342").FirstOrDefault();
                courseRepoMock.Setup(repo => repo.GetCourseByGuidAsync(It.IsAny<String>())).ReturnsAsync(course_temp);
                stuRefDataRepoMock.Setup(repo => repo.GetCreditCategoriesAsync()).ReturnsAsync(creditCategories);
                stuRefDataRepoMock.Setup(repo => repo.GetAcademicLevelsAsync()).ReturnsAsync(academicLevels);
                stuRefDataRepoMock.Setup(repo => repo.GetGradeSchemesAsync()).ReturnsAsync(gradeSchemes);
                stuRefDataRepoMock.Setup(repo => repo.GetCourseLevelsAsync()).ReturnsAsync(courseLevels);
                referenceDataRepoMock.Setup(repo => repo.DepartmentsAsync()).ReturnsAsync(departments);
                referenceDataRepoMock.Setup(repo => repo.Locations).Returns(locations);

                // Configuration defaults and campus calendar information.
                configRepoMock.Setup(repo => repo.GetDefaultsConfiguration()).Returns(new DefaultsConfiguration() { CampusCalendarId = "MAIN" });
                studentConfigRepoMock.Setup(repo => repo.GetCurriculumConfigurationAsync()).ReturnsAsync(new CurriculumConfiguration() { SectionActiveStatusCode = "A", SectionInactiveStatusCode = "I", DefaultInstructionalMethodCode = "INT" });
                eventRepoMock.Setup(repo => repo.GetCalendar("MAIN")).Returns(new CampusCalendar("MAIN", "Default Calendar", new TimeSpan(), new TimeSpan()));

                var sectionStatusCodes = new List<SectionStatusCode>()
                {
                    new SectionStatusCode("P", "Pending", SectionStatus.Active, SectionStatusIntegration.Pending),
                    new SectionStatusCode("C", "Closed", SectionStatus.Active, SectionStatusIntegration.Closed),
                    new SectionStatusCode("C", "Cancelled", SectionStatus.Active, SectionStatusIntegration.Cancelled),
                    new SectionStatusCode("O", "Open", SectionStatus.Active, SectionStatusIntegration.Open),
                };

                stuRefDataRepoMock.Setup(repo => repo.GetSectionStatusCodesAsync()).ReturnsAsync(sectionStatusCodes);


                // Mock the section service
                sectionCoordinationService = new SectionCoordinationService(adapterRegistry, sectionRepo, courseRepo, studentRepo, studentReferenceDataRepo,
                    referenceDataRepo, termRepo, studentConfigRepo, configRepo, personRepo, roomRepo, eventRepo, bookRepo, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionRepo = null;
                studentRepo = null;
                roleRepo = null;
                adapterRegistry = null;
                sectionCoordinationService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionService_Null_Object_V8()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                var r = await sectionCoordinationService.PostSection4Async(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SectionService_Null_Guid_V8()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                var r = await sectionCoordinationService.PostSection4Async(new Dtos.Section4());
            }

            //[TestMethod]
            //[ExpectedException(typeof(PermissionsException))]
            //public async Task SectionService_SectionKeyNotFound_V8()
            //{
            //    sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(section1.Guid, It.IsAny<bool>())).Throws(new KeyNotFoundException());
            //    var r = await sectionCoordinationService.PostSection4Async(new Dtos.Section4()
            //    {
            //        Id = meeting1.Guid
            //    });
            //}

            //[TestMethod]
            //[ExpectedException(typeof(PermissionsException))]
            //public async Task SectionService_NoPermission_V8()
            //{
            //    var section = new Dtos.Section4() { Id = "999999999999999" };
            //    var r = await sectionCoordinationService.PostSection4Async(section);
            //}

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionService_NullCourse_V8()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                //var credit = new List<Dtos.Credit2>() { new Dtos.Credit2() {CreditCategory = new Dtos.CreditIdAndTypeProperty() { 
                //    Detail = new Dtos.GuidObject2(creditCategories[0].Guid) }, Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = new Dtos.GuidObject2(creditCategories[0].Guid) }, Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section2 = new Dtos.Section4()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    Title = "Modern Algebra",
                    Course = null,
                    Credits = credit,
                    Status = Dtos.SectionStatus2.Open
                };
                var r = await sectionCoordinationService.PostSection4Async(section2);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionService_EmptyTitle_V8()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = new Dtos.GuidObject2(creditCategories[0].Guid) }, Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section2 = new Dtos.Section4()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    Title = "",
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,
                    Status = Dtos.SectionStatus2.Open
                };
                var r = await sectionCoordinationService.PostSection4Async(section2);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionService_NullStartOn_V8()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = new Dtos.GuidObject2(creditCategories[0].Guid) }, Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section2 = new Dtos.Section4()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = null,
                    Title = "MATH",
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,
                    Status = Dtos.SectionStatus2.Open
                };
                var r = await sectionCoordinationService.PostSection4Async(section2);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SectionService_StartOnGreaterThanEndOn_V8()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = new Dtos.GuidObject2(creditCategories[0].Guid) }, Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section2 = new Dtos.Section4()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    EndOn = new DateTime(2010, 09, 01),
                    Title = "MATH",
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,
                    Status = Dtos.SectionStatus2.Open
                };
                var r = await sectionCoordinationService.PostSection4Async(section2);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SectionService_NullCreditMeasure_V8()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = new Dtos.GuidObject2(creditCategories[0].Guid) }, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section2 = new Dtos.Section4()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    Title = "MATH",
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,
                    Status = Dtos.SectionStatus2.Open
                };
                var r = await sectionCoordinationService.PostSection4Async(section2);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SectionService_NullCreditDetailAndCreditType_V8()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(It.IsAny<String>(), It.IsAny<bool>())).ReturnsAsync(section1);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PostSectionAsync(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section1);
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = null }, Measure = new Dtos.CreditMeasure2(), Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section2 = new Dtos.Section4()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    Title = "MATH",
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,
                    Status = Dtos.SectionStatus2.Open
                };
                var r = await sectionCoordinationService.PostSection4Async(section2);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SectionService_NullCreditMinimum_V8()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = new Dtos.GuidObject2(creditCategories[0].Guid) }, Measure = Dtos.CreditMeasure2.CEU, Maximum = 4.0m, Increment = 1.0m}};
                var section2 = new Dtos.Section4()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    EndOn = new DateTime(2010, 09, 01),
                    Title = "MATH",
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,
                    Status = Dtos.SectionStatus2.Open
                };
                var r = await sectionCoordinationService.PostSection4Async(section2);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SectionService_NullCreditType_V8()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(It.IsAny<String>(), It.IsAny<bool>())).ReturnsAsync(section1);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PostSectionAsync(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section1);
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2(),
                    Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section = new Dtos.Section4()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    EndOn = new DateTime(2011, 12, 14),
                    Title = "Modern Algebra",
                    Course = new Dtos.GuidObject2("342"),
                    Status = Dtos.SectionStatus2.Open,
                    Credits = credit,
                    CourseLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2("sjdhd7392") }
                };
                var r = await sectionCoordinationService.PostSection4Async(section);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SectionService_NullInstructionalPlaforms_V8()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(It.IsAny<String>(), It.IsAny<bool>())).ReturnsAsync(section1);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PostSectionAsync(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section1);
                referenceDataRepoMock.Setup(repo => repo.GetInstructionalPlatformsAsync(It.IsAny<bool>())).ReturnsAsync(() => null);
                //var credit = new List<Dtos.Credit2>() { new Dtos.Credit2() {CreditCategory = new Dtos.CreditIdAndTypeProperty(), 
                //Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section = new Dtos.Section4()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    EndOn = new DateTime(2011, 12, 14),
                    Title = "Modern Algebra",
                    Course = new Dtos.GuidObject2("342"),
                    Status = Dtos.SectionStatus2.Open,
                    AcademicLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2(academicLevels[0].Guid) },
                    //Credits = credit,
                    InstructionalPlatform = new Dtos.GuidObject2("jfhjdhjs"),
                    //OwningOrganizations = new List<Dtos.OfferingOrganization2>() { 
                    //    new Dtos.OfferingOrganization2() {Organization = new Dtos.GuidObject2(departments[0].Guid) } },
                    CourseLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2("sjdhd7392") }
                };
                var r = await sectionCoordinationService.PostSection4Async(section);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SectionService_NullCreditCategoryOfCredit_V8()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(It.IsAny<String>(), It.IsAny<bool>())).ReturnsAsync(section1);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PostSectionAsync(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section1);
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() { Measure = Dtos.CreditMeasure2.CEU,
                    Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section = new Dtos.Section4()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    EndOn = new DateTime(2011, 12, 14),
                    Title = "Modern Algebra",
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,
                    Status = Dtos.SectionStatus2.Open,
                    CourseLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2("sjdhd7392") }
                };
                var r = await sectionCoordinationService.PostSection4Async(section);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SectionService_NullCreditCategories_V8()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PostSectionAsync(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section2);
                stuRefDataRepoMock.Setup(repo => repo.GetCreditCategoriesAsync()).ReturnsAsync(() => null);
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = new Dtos.GuidObject2(creditCategories[0].Guid) }, Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section = new Dtos.Section4()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    EndOn = new DateTime(2011, 12, 14),
                    Title = "Modern Algebra",
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,
                    Status = Dtos.SectionStatus2.Open,
                    CourseLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2("sjdhd7392") }
                };
                var r = await sectionCoordinationService.PostSection4Async(section);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SectionService_NullGradeSchemes_V8()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(It.IsAny<String>(), It.IsAny<bool>())).ReturnsAsync(section3);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PostSectionAsync(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section3);
                stuRefDataRepoMock.Setup(repo => repo.GetGradeSchemesAsync()).ReturnsAsync(() => null);
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() { 
                    /*Detail = new Dtos.GuidObject2(creditCategories[0].Guid),*/ CreditType = Dtos.EnumProperties.CreditCategoryType3.Institutional }, Measure = Dtos.CreditMeasure2.Credit, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section = new Dtos.Section4()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    EndOn = new DateTime(2011, 12, 14),
                    Title = "Modern Algebra",
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,
                    Status = Dtos.SectionStatus2.Open,
                    CourseLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2("sjdhd7392") }
                };
                var r = await sectionCoordinationService.PostSection4Async(section);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SectionService_NullAcademicLevels_V8()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(It.IsAny<String>(), It.IsAny<bool>())).ReturnsAsync(section1);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PostSectionAsync(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section1);
                stuRefDataRepoMock.Setup(repo => repo.GetAcademicLevelsAsync()).ReturnsAsync(() => null);
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() { 
                    /*Detail = new Dtos.GuidObject2(creditCategories[0].Guid),*/ CreditType = Dtos.EnumProperties.CreditCategoryType3.Transfer }, Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section = new Dtos.Section4()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    EndOn = new DateTime(2011, 12, 14),
                    Title = "Modern Algebra",
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,
                    Status = Dtos.SectionStatus2.Open,
                    CourseLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2("sjdhd7392") }
                };
                var r = await sectionCoordinationService.PostSection4Async(section);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SectionService_NullLocations_V8()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(It.IsAny<String>(), It.IsAny<bool>())).ReturnsAsync(section2);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PostSectionAsync(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section2);
                referenceDataRepoMock.Setup(repo => repo.Locations).Returns<Location>(null);
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() { 
                    /*Detail = new Dtos.GuidObject2(creditCategories[0].Guid),*/ CreditType = Dtos.EnumProperties.CreditCategoryType3.ContinuingEducation }, Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section = new Dtos.Section4()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    EndOn = new DateTime(2011, 12, 14),
                    Title = "Modern Algebra",
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,
                    Status = Dtos.SectionStatus2.Open,
                    CourseLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2("sjdhd7392") }
                };
                var r = await sectionCoordinationService.PostSection4Async(section);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SectionService_NullCourseLevels_V8()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PostSectionAsync(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section1);
                stuRefDataRepoMock.Setup(repo => repo.GetCourseLevelsAsync()).ReturnsAsync(() => null);
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = new Dtos.GuidObject2(creditCategories[0].Guid) }, Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section2 = new Dtos.Section4()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    Title = "Modern Algebra",
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,
                    Status = Dtos.SectionStatus2.Open
                };
                var r = await sectionCoordinationService.PostSection4Async(section2);
            }

            [TestMethod]
            public async Task SectionService_Post_V8()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PostSectionAsync(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section1);
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = new Dtos.GuidObject2(creditCategories[0].Guid), CreditType = Dtos.EnumProperties.CreditCategoryType3.Institutional },
                    Measure = Dtos.CreditMeasure2.Credit, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section2 = new Dtos.Section4()
                {
                    Id = section1.Guid,
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    Title = "Modern Algebra",
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,
                    Status = Dtos.SectionStatus2.Open
                };
                var res = await sectionCoordinationService.PostSection4Async(section2);
                Assert.AreEqual(section1.Guid, res.Id);
            }
        }

        [TestClass]
        public class Sections_Put_V8 : CurrentUserSetup
        {
            private SectionCoordinationService sectionCoordinationService;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IStudentReferenceDataRepository> stuRefDataRepoMock;
            private IStudentReferenceDataRepository studentReferenceDataRepo;
            private Mock<IReferenceDataRepository> referenceDataRepoMock;
            private IReferenceDataRepository referenceDataRepo;
            private Mock<ICourseRepository> courseRepoMock;
            private ICourseRepository courseRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IStudentConfigurationRepository> studentConfigRepoMock;
            private IStudentConfigurationRepository studentConfigRepo;
            private Mock<IConfigurationRepository> configRepoMock;
            private IConfigurationRepository configRepo;
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IRoomRepository> roomRepoMock;
            private IRoomRepository roomRepo;
            private Mock<IEventRepository> eventRepoMock;
            private IEventRepository eventRepo;
            private Mock<IBookRepository> bookRepoMock;
            private IBookRepository bookRepo;


            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ILogger logger;
            private ICurrentUserFactory currentUserFactory;

            private Domain.Student.Entities.Section section1;
            private Domain.Student.Entities.Section section2;
            private Domain.Student.Entities.Section section3;
            private Domain.Student.Entities.SectionMeeting meeting1;
            private Domain.Student.Entities.SectionMeeting meeting2;
            private Domain.Student.Entities.SectionMeeting meeting3;
            private Domain.Student.Entities.SectionMeeting meeting4;

            private List<OfferingDepartment> dpts = new List<OfferingDepartment>() { new OfferingDepartment("ART", 100m) };
            private List<string> levels = new List<string>() { "Whatever" };
            private List<SectionStatusItem> statuses = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-60)) };
            private List<SectionStatusItem> statuses2 = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Inactive, "I", DateTime.Today.AddDays(-60)) };
            private List<SectionStatusItem> statuses3 = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Cancelled, "C", DateTime.Today.AddDays(-60)) };
            private IEnumerable<Domain.Student.Entities.Course> allCourses = new TestCourseRepository().GetAsync().Result;

            private List<CreditCategory> creditCategories = new List<CreditCategory>() { new CreditCategory(Guid.NewGuid().ToString(), "RC", "Regular Credit", CreditType.Institutional),
                new CreditCategory(Guid.NewGuid().ToString(), "RC", "Regular Credit", CreditType.Transfer), new CreditCategory(Guid.NewGuid().ToString(), "RC", "Regular Credit", CreditType.ContinuingEducation ) };
            private List<Domain.Student.Entities.AcademicLevel> academicLevels = new List<Domain.Student.Entities.AcademicLevel>() { new Domain.Student.Entities.AcademicLevel(Guid.NewGuid().ToString(), "UG", "Undergraduate") };
            private List<Domain.Student.Entities.GradeScheme> gradeSchemes = new List<Domain.Student.Entities.GradeScheme>() { new Domain.Student.Entities.GradeScheme(Guid.NewGuid().ToString(), "Pass/Fail", "Pass/Fail grade") };
            private List<Domain.Student.Entities.CourseLevel> courseLevels = new List<Domain.Student.Entities.CourseLevel>() { new Domain.Student.Entities.CourseLevel(Guid.NewGuid().ToString(), "100", "Introduction to terms, concepts, and techniques.") };
            private List<Department> departments = new List<Department>() { new Department(Guid.NewGuid().ToString(), "PHYS", "Physics", true) };
            private List<Location> locations = new List<Location>() { new Location(Guid.NewGuid().ToString(), "MD", "Maryland") };

            private string meeting1Guid = Guid.NewGuid().ToString();
            private string instructionalMethodGuid = Guid.NewGuid().ToString();
            private string roomGuid = Guid.NewGuid().ToString();
            private Domain.Entities.Permission permissionCreateandUpdateSections;

            [TestInitialize]
            public void Initialize()
            {
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                roomRepoMock = new Mock<IRoomRepository>();
                roomRepo = roomRepoMock.Object;
                stuRefDataRepoMock = new Mock<IStudentReferenceDataRepository>();
                studentReferenceDataRepo = stuRefDataRepoMock.Object;
                referenceDataRepoMock = new Mock<IReferenceDataRepository>();
                referenceDataRepo = referenceDataRepoMock.Object;
                courseRepoMock = new Mock<ICourseRepository>();
                courseRepo = courseRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                studentConfigRepoMock = new Mock<IStudentConfigurationRepository>();
                studentConfigRepo = studentConfigRepoMock.Object;
                configRepoMock = new Mock<IConfigurationRepository>();
                configRepo = configRepoMock.Object;
                eventRepoMock = new Mock<IEventRepository>();
                eventRepo = eventRepoMock.Object;
                bookRepoMock = new Mock<IBookRepository>();
                bookRepo = bookRepoMock.Object;


                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;

                // Set Current User
                currentUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();

                logger = new Mock<ILogger>().Object;

                // Mock permissions
                permissionCreateandUpdateSections = new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateAndUpdateSection);
                thirdPartyRole.AddPermission(permissionCreateandUpdateSections);
                //permissionViewAnyProjects = new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewAnyProjects);
                //thirdPartyRole.AddPermission(permissionViewAnyProjects);
                //permissionViewAnyProjectsLineItems = new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.ViewAnyProjectsLineItems);
                //thirdPartyRole.AddPermission(permissionViewAnyProjectsLineItems);

                // Mock the section repo response
                section1 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC1", "1119", "01", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses, true);
                section1.TermId = "2012/FA";
                section1.EndDate = new DateTime(2012, 12, 21);
                section1.Guid = Guid.NewGuid().ToString();

                section2 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC2", "1119", "02", new DateTime(2012, 09, 01), 3.0m, null, "Introduction to Art", "IN", dpts, levels, "UG", statuses2, true);
                section2.TermId = "2012/FA";
                section2.Guid = Guid.NewGuid().ToString();

                section3 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC3", "1119", "03", new DateTime(2011, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses3, true);
                section3.TermId = "2011/FA";
                section3.EndDate = new DateTime(2011, 12, 21);
                section3.Guid = Guid.NewGuid().ToString();

                var sectionFaculty = new Domain.Student.Entities.SectionFaculty("1234", "SEC1", "0000678", "LEC", new DateTime(2012, 09, 01), new DateTime(2011, 12, 21), 100.0m);

                meeting1 = new Domain.Student.Entities.SectionMeeting("123", "SEC1", "LEC", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "W");
                meeting1.Guid = meeting1Guid;
                meeting1.AddFacultyId("0000678");
                meeting1.AddSectionFaculty(sectionFaculty);
                meeting1.StartTime = new DateTimeOffset(2012, 01, 01, 08, 30, 00, new TimeSpan());
                meeting1.EndTime = new DateTimeOffset(2012, 12, 21, 09, 30, 00, new TimeSpan());
                meeting1.Room = "DAN*102";
                section1.AddSectionMeeting(meeting1);

                meeting2 = new Domain.Student.Entities.SectionMeeting("123", "SEC1", "LEC", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "D");
                meeting2.Guid = meeting1Guid;

                meeting3 = new Domain.Student.Entities.SectionMeeting("456", "SEC1", "LEC", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "M");
                meeting3.Guid = meeting1Guid;

                meeting4 = new Domain.Student.Entities.SectionMeeting("789", "SEC1", "LEC", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "Y");
                meeting4.Guid = meeting1Guid;

                Term term1 = new Term("termGuid1", "2011/FA", "2011 Fall", new DateTime(2011, 09, 01), new DateTime(2011, 12, 15), 2012,
                    0, false, false, "2011/FA", false);
                var date1 = new List<DateTime?>() { new DateTime(2011, 09, 15) };
                var regDate = new RegistrationDate("Loc1", null, null, null, null, null, null, null, null, null, date1);
                term1.AddRegistrationDates(regDate);
                Term term2 = new Term("termGuid2", "2012/FA", "2012 Fall", new DateTime(2012, 09, 01), new DateTime(2012, 12, 15), 2013,
                    0, false, false, "2012/FA", false);
                regDate = new RegistrationDate("Loc2", null, null, null, null, null, null, null, null, null, new List<DateTime?>());
                term2.AddRegistrationDates(regDate);
                List<Term> allTerms = new List<Term>() { term1, term2 };
                termRepoMock.Setup(repo => repo.Get()).Returns(allTerms);
                termRepoMock.Setup(repo => repo.GetAsync()).ReturnsAsync(allTerms);

                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(section1.Guid, It.IsAny<bool>())).ReturnsAsync(section1);
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(section2.Guid, It.IsAny<bool>())).ReturnsAsync(section2);
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(section3.Guid, It.IsAny<bool>())).ReturnsAsync(section3);
                sectionRepoMock.Setup(repo => repo.PutSectionMeetingAsync(It.IsAny<Domain.Student.Entities.Section>(), meeting1.Guid)).ReturnsAsync(meeting1);
                sectionRepoMock.Setup(repo => repo.PutSectionMeetingAsync(It.IsAny<Domain.Student.Entities.Section>(), meeting2.Guid)).ReturnsAsync(meeting2);
                sectionRepoMock.Setup(repo => repo.PutSectionMeetingAsync(It.IsAny<Domain.Student.Entities.Section>(), meeting3.Guid)).ReturnsAsync(meeting3);
                courseRepoMock.Setup(repo => repo.GetAsync()).Returns(Task.FromResult(allCourses));
                var course_temp = allCourses.Where(c => c.Id == "342").FirstOrDefault();
                courseRepoMock.Setup(repo => repo.GetCourseByGuidAsync(It.IsAny<String>())).ReturnsAsync(course_temp);
                stuRefDataRepoMock.Setup(repo => repo.GetCreditCategoriesAsync()).ReturnsAsync(creditCategories);
                stuRefDataRepoMock.Setup(repo => repo.GetAcademicLevelsAsync()).ReturnsAsync(academicLevels);
                stuRefDataRepoMock.Setup(repo => repo.GetGradeSchemesAsync()).ReturnsAsync(gradeSchemes);
                stuRefDataRepoMock.Setup(repo => repo.GetCourseLevelsAsync()).ReturnsAsync(courseLevels);
                referenceDataRepoMock.Setup(repo => repo.DepartmentsAsync()).ReturnsAsync(departments);
                referenceDataRepoMock.Setup(repo => repo.Locations).Returns(locations);

                // Configuration defaults and campus calendar information.
                configRepoMock.Setup(repo => repo.GetDefaultsConfiguration()).Returns(new DefaultsConfiguration() { CampusCalendarId = "MAIN" });
                studentConfigRepoMock.Setup(repo => repo.GetCurriculumConfigurationAsync()).ReturnsAsync(new CurriculumConfiguration() { SectionActiveStatusCode = "A", SectionInactiveStatusCode = "I", DefaultInstructionalMethodCode = "INT" });
                eventRepoMock.Setup(repo => repo.GetCalendar("MAIN")).Returns(new CampusCalendar("MAIN", "Default Calendar", new TimeSpan(), new TimeSpan()));

                var sectionStatusCodes = new List<SectionStatusCode>()
                {
                    new SectionStatusCode("P", "Pending", SectionStatus.Active, SectionStatusIntegration.Pending),
                    new SectionStatusCode("C", "Closed", SectionStatus.Active, SectionStatusIntegration.Closed),
                    new SectionStatusCode("C", "Cancelled", SectionStatus.Active, SectionStatusIntegration.Cancelled),
                    new SectionStatusCode("O", "Open", SectionStatus.Active, SectionStatusIntegration.Open),
                };

                stuRefDataRepoMock.Setup(repo => repo.GetSectionStatusCodesAsync()).ReturnsAsync(sectionStatusCodes);


                // Mock the section service
                sectionCoordinationService = new SectionCoordinationService(adapterRegistry, sectionRepo, courseRepo, studentRepo, studentReferenceDataRepo,
                    referenceDataRepo, termRepo, studentConfigRepo, configRepo, personRepo, roomRepo, eventRepo, bookRepo, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionRepo = null;
                studentRepo = null;
                roleRepo = null;
                adapterRegistry = null;
                sectionCoordinationService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionService_Null_Object_V8()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                var r = await sectionCoordinationService.PutSection4Async(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SectionService_Null_Guid_V8()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                var r = await sectionCoordinationService.PutSection4Async(new Dtos.Section4());
            }

           

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionService_NullCourse_V8()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                //var credit = new List<Dtos.Credit2>() { new Dtos.Credit2() {CreditCategory = new Dtos.CreditIdAndTypeProperty() { 
                //    Detail = new Dtos.GuidObject2(creditCategories[0].Guid) }, Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = new Dtos.GuidObject2(creditCategories[0].Guid) }, Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section2 = new Dtos.Section4()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    Title = "Modern Algebra",
                    Course = null,
                    Credits = credit,
                    Status = Dtos.SectionStatus2.Open
                };
                var r = await sectionCoordinationService.PutSection4Async(section2);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionService_EmptyTitle_V8()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = new Dtos.GuidObject2(creditCategories[0].Guid) }, Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section2 = new Dtos.Section4()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    Title = "",
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,
                    Status = Dtos.SectionStatus2.Open
                };
                var r = await sectionCoordinationService.PutSection4Async(section2);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionService_NullStartOn_V8()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = new Dtos.GuidObject2(creditCategories[0].Guid) }, Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section2 = new Dtos.Section4()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = null,
                    Title = "MATH",
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,
                    Status = Dtos.SectionStatus2.Open
                };
                var r = await sectionCoordinationService.PutSection4Async(section2);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SectionService_StartOnGreaterThanEndOn_V8()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = new Dtos.GuidObject2(creditCategories[0].Guid) }, Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section2 = new Dtos.Section4()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    EndOn = new DateTime(2010, 09, 01),
                    Title = "MATH",
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,
                    Status = Dtos.SectionStatus2.Open
                };
                var r = await sectionCoordinationService.PutSection4Async(section2);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SectionService_NullCreditMeasure_V8()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = new Dtos.GuidObject2(creditCategories[0].Guid) }, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section2 = new Dtos.Section4()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    Title = "MATH",
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,
                    Status = Dtos.SectionStatus2.Open
                };
                var r = await sectionCoordinationService.PutSection4Async(section2);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SectionService_NullCreditDetailAndCreditType_V8()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(It.IsAny<String>(), It.IsAny<bool>())).ReturnsAsync(section1);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PutSectionAsync(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section1);
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = null }, Measure = new Dtos.CreditMeasure2(), Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section2 = new Dtos.Section4()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    Title = "MATH",
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,
                    Status = Dtos.SectionStatus2.Open
                };
                var r = await sectionCoordinationService.PutSection4Async(section2);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SectionService_NullCreditMinimum_V8()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = new Dtos.GuidObject2(creditCategories[0].Guid) }, Measure = Dtos.CreditMeasure2.CEU, Maximum = 4.0m, Increment = 1.0m}};
                var section2 = new Dtos.Section4()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    EndOn = new DateTime(2010, 09, 01),
                    Title = "MATH",
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,
                    Status = Dtos.SectionStatus2.Open
                };
                var r = await sectionCoordinationService.PutSection4Async(section2);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SectionService_NullCreditType_V8()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(It.IsAny<String>(), It.IsAny<bool>())).ReturnsAsync(section1);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PutSectionAsync(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section1);
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2(),
                    Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section = new Dtos.Section4()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    EndOn = new DateTime(2011, 12, 14),
                    Title = "Modern Algebra",
                    Course = new Dtos.GuidObject2("342"),
                    Status = Dtos.SectionStatus2.Open,
                    Credits = credit,
                    CourseLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2("sjdhd7392") }
                };
                var r = await sectionCoordinationService.PutSection4Async(section);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SectionService_NullInstructionalPlaforms_V8()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(It.IsAny<String>(), It.IsAny<bool>())).ReturnsAsync(section1);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PutSectionAsync(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section1);
                referenceDataRepoMock.Setup(repo => repo.GetInstructionalPlatformsAsync(It.IsAny<bool>())).ReturnsAsync(() => null);
                //var credit = new List<Dtos.Credit2>() { new Dtos.Credit2() {CreditCategory = new Dtos.CreditIdAndTypeProperty(), 
                //Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section = new Dtos.Section4()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    EndOn = new DateTime(2011, 12, 14),
                    Title = "Modern Algebra",
                    Course = new Dtos.GuidObject2("342"),
                    Status = Dtos.SectionStatus2.Open,
                    AcademicLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2(academicLevels[0].Guid) },
                    //Credits = credit,
                    InstructionalPlatform = new Dtos.GuidObject2("jfhjdhjs"),
                    //OwningOrganizations = new List<Dtos.OfferingOrganization2>() { 
                    //    new Dtos.OfferingOrganization2() {Organization = new Dtos.GuidObject2(departments[0].Guid) } },
                    CourseLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2("sjdhd7392") }
                };
                var r = await sectionCoordinationService.PutSection4Async(section);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SectionService_NullCreditCategoryOfCredit_V8()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(It.IsAny<String>(), It.IsAny<bool>())).ReturnsAsync(section1);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PutSectionAsync(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section1);
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() { Measure = Dtos.CreditMeasure2.CEU,
                    Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section = new Dtos.Section4()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    EndOn = new DateTime(2011, 12, 14),
                    Title = "Modern Algebra",
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,
                    Status = Dtos.SectionStatus2.Open,
                    CourseLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2("sjdhd7392") }
                };
                var r = await sectionCoordinationService.PutSection4Async(section);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SectionService_NullCreditCategories_V8()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PutSectionAsync(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section2);
                stuRefDataRepoMock.Setup(repo => repo.GetCreditCategoriesAsync()).ReturnsAsync(() => null);
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = new Dtos.GuidObject2(creditCategories[0].Guid) }, Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section = new Dtos.Section4()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    EndOn = new DateTime(2011, 12, 14),
                    Title = "Modern Algebra",
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,
                    Status = Dtos.SectionStatus2.Open,
                    CourseLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2("sjdhd7392") }
                };
                var r = await sectionCoordinationService.PutSection4Async(section);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SectionService_NullGradeSchemes_V8()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(It.IsAny<String>(), It.IsAny<bool>())).ReturnsAsync(section3);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PutSectionAsync(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section3);
                stuRefDataRepoMock.Setup(repo => repo.GetGradeSchemesAsync()).ReturnsAsync(() => null);
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() { 
                    /*Detail = new Dtos.GuidObject2(creditCategories[0].Guid),*/ CreditType = Dtos.EnumProperties.CreditCategoryType3.Institutional }, Measure = Dtos.CreditMeasure2.Credit, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section = new Dtos.Section4()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    EndOn = new DateTime(2011, 12, 14),
                    Title = "Modern Algebra",
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,
                    Status = Dtos.SectionStatus2.Open,
                    CourseLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2("sjdhd7392") }
                };
                var r = await sectionCoordinationService.PutSection4Async(section);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SectionService_NullAcademicLevels_V8()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(It.IsAny<String>(), It.IsAny<bool>())).ReturnsAsync(section1);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PutSectionAsync(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section1);
                stuRefDataRepoMock.Setup(repo => repo.GetAcademicLevelsAsync()).ReturnsAsync(() => null);
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() { 
                    /*Detail = new Dtos.GuidObject2(creditCategories[0].Guid),*/ CreditType = Dtos.EnumProperties.CreditCategoryType3.Transfer }, Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section = new Dtos.Section4()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    EndOn = new DateTime(2011, 12, 14),
                    Title = "Modern Algebra",
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,
                    Status = Dtos.SectionStatus2.Open,
                    CourseLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2("sjdhd7392") }
                };
                var r = await sectionCoordinationService.PutSection4Async(section);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SectionService_NullLocations_V8()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(It.IsAny<String>(), It.IsAny<bool>())).ReturnsAsync(section2);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PutSectionAsync(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section2);
                referenceDataRepoMock.Setup(repo => repo.Locations).Returns<Location>(null);
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() { 
                    /*Detail = new Dtos.GuidObject2(creditCategories[0].Guid),*/ CreditType = Dtos.EnumProperties.CreditCategoryType3.ContinuingEducation }, Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section = new Dtos.Section4()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    EndOn = new DateTime(2011, 12, 14),
                    Title = "Modern Algebra",
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,
                    Status = Dtos.SectionStatus2.Open,
                    CourseLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2("sjdhd7392") }
                };
                var r = await sectionCoordinationService.PutSection4Async(section);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SectionService_NullCourseLevels_V8()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PutSectionAsync(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section1);
                stuRefDataRepoMock.Setup(repo => repo.GetCourseLevelsAsync()).ReturnsAsync(() => null);
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = new Dtos.GuidObject2(creditCategories[0].Guid) }, Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section2 = new Dtos.Section4()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    Title = "Modern Algebra",
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,
                    Status = Dtos.SectionStatus2.Open
                };
                var r = await sectionCoordinationService.PutSection4Async(section2);
            }

            [TestMethod]
            public async Task SectionService_Put_V8()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PutSectionAsync(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section1);
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = new Dtos.GuidObject2(creditCategories[0].Guid), CreditType = Dtos.EnumProperties.CreditCategoryType3.Institutional },
                    Measure = Dtos.CreditMeasure2.Credit, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section2 = new Dtos.Section4()
                {
                    Id = section1.Guid,
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    Title = "Modern Algebra",
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,
                    Status = Dtos.SectionStatus2.Open
                };
                var res = await sectionCoordinationService.PutSection4Async(section2);
                Assert.AreEqual(section1.Guid, res.Id);
            }
        }

        [TestClass]
        public class Sections_Post_V16 : CurrentUserSetup
        {
            private SectionCoordinationService sectionCoordinationService;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IStudentReferenceDataRepository> stuRefDataRepoMock;
            private IStudentReferenceDataRepository studentReferenceDataRepo;
            private Mock<IReferenceDataRepository> referenceDataRepoMock;
            private IReferenceDataRepository referenceDataRepo;
            private Mock<ICourseRepository> courseRepoMock;
            private ICourseRepository courseRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IStudentConfigurationRepository> studentConfigRepoMock;
            private IStudentConfigurationRepository studentConfigRepo;
            private Mock<IConfigurationRepository> configRepoMock;
            private IConfigurationRepository configRepo;
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IRoomRepository> roomRepoMock;
            private IRoomRepository roomRepo;
            private Mock<IEventRepository> eventRepoMock;
            private IEventRepository eventRepo;
            private Mock<IBookRepository> bookRepoMock;
            private IBookRepository bookRepo;


            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ILogger logger;
            private ICurrentUserFactory currentUserFactory;

            private Domain.Student.Entities.Section section1;
            private Domain.Student.Entities.Section section2;
            private Domain.Student.Entities.Section section3;
            private Domain.Student.Entities.SectionMeeting meeting1;
            private Domain.Student.Entities.SectionMeeting meeting2;
            private Domain.Student.Entities.SectionMeeting meeting3;
            private Domain.Student.Entities.SectionMeeting meeting4;

            private List<OfferingDepartment> dpts = new List<OfferingDepartment>() { new OfferingDepartment("ART", 100m) };
            private List<string> levels = new List<string>() { "Whatever" };
            private List<SectionStatusItem> statuses = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-60)) };
            private List<SectionStatusItem> statuses2 = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Inactive, "I", DateTime.Today.AddDays(-60)) };
            private List<SectionStatusItem> statuses3 = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Cancelled, "C", DateTime.Today.AddDays(-60)) };
            private IEnumerable<Domain.Student.Entities.Course> allCourses = new TestCourseRepository().GetAsync().Result;

            string credCatguid = "16fcac65-ceec-4f15-8a23-a3f3f70e3c92";
            private List<CreditCategory> creditCategories = new List<CreditCategory>()
            {
                new CreditCategory("16fcac65-ceec-4f15-8a23-a3f3f70e3c92", "RC", "Regular Credit", CreditType.Institutional),
                new CreditCategory(Guid.NewGuid().ToString(), "RC", "Regular Credit", CreditType.Transfer),
                new CreditCategory(Guid.NewGuid().ToString(), "RC", "Regular Credit", CreditType.ContinuingEducation )
            };
            private List<Domain.Student.Entities.AcademicLevel> academicLevels = new List<Domain.Student.Entities.AcademicLevel>() { new Domain.Student.Entities.AcademicLevel(Guid.NewGuid().ToString(), "UG", "Undergraduate") };
            private List<Domain.Student.Entities.GradeScheme> gradeSchemes = new List<Domain.Student.Entities.GradeScheme>() { new Domain.Student.Entities.GradeScheme(Guid.NewGuid().ToString(), "Pass/Fail", "Pass/Fail grade") };
            private List<Domain.Student.Entities.CourseLevel> courseLevels = new List<Domain.Student.Entities.CourseLevel>() { new Domain.Student.Entities.CourseLevel(Guid.NewGuid().ToString(), "100", "Introduction to terms, concepts, and techniques.") };
            private List<Department> departments = new List<Department>() { new Department("16fcac65-ceec-4f15-8a23-a3f3f70e3c92", "PHYS", "Physics", true) };
            private List<Location> locations = new List<Location>() { new Location(Guid.NewGuid().ToString(), "MD", "Maryland") };

            private string meeting1Guid = Guid.NewGuid().ToString();
            private string instructionalMethodGuid = Guid.NewGuid().ToString();
            private string roomGuid = Guid.NewGuid().ToString();
            private Domain.Entities.Permission permissionCreateandUpdateSections;

            [TestInitialize]
            public void Initialize()
            {
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                roomRepoMock = new Mock<IRoomRepository>();
                roomRepo = roomRepoMock.Object;
                stuRefDataRepoMock = new Mock<IStudentReferenceDataRepository>();
                studentReferenceDataRepo = stuRefDataRepoMock.Object;
                referenceDataRepoMock = new Mock<IReferenceDataRepository>();
                referenceDataRepo = referenceDataRepoMock.Object;
                courseRepoMock = new Mock<ICourseRepository>();
                courseRepo = courseRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                studentConfigRepoMock = new Mock<IStudentConfigurationRepository>();
                studentConfigRepo = studentConfigRepoMock.Object;
                configRepoMock = new Mock<IConfigurationRepository>();
                configRepo = configRepoMock.Object;
                eventRepoMock = new Mock<IEventRepository>();
                eventRepo = eventRepoMock.Object;
                bookRepoMock = new Mock<IBookRepository>();
                bookRepo = bookRepoMock.Object;

                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;

                // Set Current User
                currentUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();

                logger = new Mock<ILogger>().Object;

                // Mock permissions
                permissionCreateandUpdateSections = new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateAndUpdateSection);
                thirdPartyRole.AddPermission(permissionCreateandUpdateSections);
                //permissionViewAnyProjects = new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewAnyProjects);
                //thirdPartyRole.AddPermission(permissionViewAnyProjects);
                //permissionViewAnyProjectsLineItems = new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.ViewAnyProjectsLineItems);
                //thirdPartyRole.AddPermission(permissionViewAnyProjectsLineItems);

                // Mock the section repo response
                section1 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC1", "1119", "01", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses, true);
                section1.TermId = "2012/FA";
                section1.EndDate = new DateTime(2012, 12, 21);
                section1.Guid = Guid.NewGuid().ToString();

                section2 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC2", "1119", "02", new DateTime(2012, 09, 01), 3.0m, null, "Introduction to Art", "IN", dpts, levels, "UG", statuses2, true);
                section2.TermId = "2012/FA";
                section2.Guid = Guid.NewGuid().ToString();

                section3 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC3", "1119", "03", new DateTime(2011, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses3, true);
                section3.TermId = "2011/FA";
                section3.EndDate = new DateTime(2011, 12, 21);
                section3.Guid = Guid.NewGuid().ToString();

                var sectionFaculty = new Domain.Student.Entities.SectionFaculty("1234", "SEC1", "0000678", "LEC", new DateTime(2012, 09, 01), new DateTime(2011, 12, 21), 100.0m);

                meeting1 = new Domain.Student.Entities.SectionMeeting("123", "SEC1", "LEC", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "W");
                meeting1.Guid = meeting1Guid;
                meeting1.AddFacultyId("0000678");
                meeting1.AddSectionFaculty(sectionFaculty);
                meeting1.StartTime = new DateTimeOffset(2012, 01, 01, 08, 30, 00, new TimeSpan());
                meeting1.EndTime = new DateTimeOffset(2012, 12, 21, 09, 30, 00, new TimeSpan());
                meeting1.Room = "DAN*102";
                section1.AddSectionMeeting(meeting1);

                meeting2 = new Domain.Student.Entities.SectionMeeting("123", "SEC1", "LEC", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "D");
                meeting2.Guid = meeting1Guid;

                meeting3 = new Domain.Student.Entities.SectionMeeting("456", "SEC1", "LEC", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "M");
                meeting3.Guid = meeting1Guid;

                meeting4 = new Domain.Student.Entities.SectionMeeting("789", "SEC1", "LEC", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "Y");
                meeting4.Guid = meeting1Guid;

                Term term1 = new Term("termGuid1", "2011/FA", "2011 Fall", new DateTime(2011, 09, 01), new DateTime(2011, 12, 15), 2012,
                    0, false, false, "2011/FA", false);
                var date1 = new List<DateTime?>() { new DateTime(2011, 09, 15) };
                var regDate = new RegistrationDate("Loc1", null, null, null, null, null, null, null, null, null, date1);
                term1.AddRegistrationDates(regDate);
                Term term2 = new Term("termGuid2", "2012/FA", "2012 Fall", new DateTime(2012, 09, 01), new DateTime(2012, 12, 15), 2013,
                    0, false, false, "2012/FA", false);
                regDate = new RegistrationDate("Loc2", null, null, null, null, null, null, null, null, null, new List<DateTime?>());
                term2.AddRegistrationDates(regDate);
                List<Term> allTerms = new List<Term>() { term1, term2 };
                termRepoMock.Setup(repo => repo.Get()).Returns(allTerms);
                termRepoMock.Setup(repo => repo.GetAsync()).ReturnsAsync(allTerms);

                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(section1.Guid, It.IsAny<bool>())).ReturnsAsync(section1);
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(section2.Guid, It.IsAny<bool>())).ReturnsAsync(section2);
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(section3.Guid, It.IsAny<bool>())).ReturnsAsync(section3);
                sectionRepoMock.Setup(repo => repo.PostSectionMeetingAsync(It.IsAny<Domain.Student.Entities.Section>(), meeting1.Guid)).ReturnsAsync(meeting1);
                sectionRepoMock.Setup(repo => repo.PostSectionMeetingAsync(It.IsAny<Domain.Student.Entities.Section>(), meeting2.Guid)).ReturnsAsync(meeting2);
                sectionRepoMock.Setup(repo => repo.PostSectionMeetingAsync(It.IsAny<Domain.Student.Entities.Section>(), meeting3.Guid)).ReturnsAsync(meeting3);
                courseRepoMock.Setup(repo => repo.GetAsync()).Returns(Task.FromResult(allCourses));
                var course_temp = allCourses.Where(c => c.Id == "342").FirstOrDefault();
                courseRepoMock.Setup(repo => repo.GetCourseByGuidAsync(It.IsAny<String>())).ReturnsAsync(course_temp);
                stuRefDataRepoMock.Setup(repo => repo.GetCreditCategoriesAsync()).ReturnsAsync(creditCategories);
                stuRefDataRepoMock.Setup(repo => repo.GetAcademicLevelsAsync()).ReturnsAsync(academicLevels);
                stuRefDataRepoMock.Setup(repo => repo.GetGradeSchemesAsync()).ReturnsAsync(gradeSchemes);
                stuRefDataRepoMock.Setup(repo => repo.GetCourseLevelsAsync()).ReturnsAsync(courseLevels);
                referenceDataRepoMock.Setup(repo => repo.DepartmentsAsync()).ReturnsAsync(departments);
                referenceDataRepoMock.Setup(repo => repo.Locations).Returns(locations);

                // Configuration defaults and campus calendar information.
                configRepoMock.Setup(repo => repo.GetDefaultsConfiguration()).Returns(new DefaultsConfiguration() { CampusCalendarId = "MAIN" });
                studentConfigRepoMock.Setup(repo => repo.GetCurriculumConfigurationAsync()).ReturnsAsync(new CurriculumConfiguration() { SectionActiveStatusCode = "A", SectionInactiveStatusCode = "I", DefaultInstructionalMethodCode = "INT" });
                eventRepoMock.Setup(repo => repo.GetCalendar("MAIN")).Returns(new CampusCalendar("MAIN", "Default Calendar", new TimeSpan(), new TimeSpan()));

                var sectionStatusCodes = new List<SectionStatusCode>()
                {
                    new SectionStatusCode("P", "Pending", SectionStatus.Active, SectionStatusIntegration.Pending),
                    new SectionStatusCode("C", "Closed", SectionStatus.Active, SectionStatusIntegration.Closed),
                    new SectionStatusCode("C", "Cancelled", SectionStatus.Active, SectionStatusIntegration.Cancelled),
                    new SectionStatusCode("O", "Open", SectionStatus.Active, SectionStatusIntegration.Open),
                };

                stuRefDataRepoMock.Setup(repo => repo.GetSectionStatusCodesAsync()).ReturnsAsync(sectionStatusCodes);

                var sectionTitleTypes = new List<Domain.Student.Entities.SectionTitleType>()
                {
                    new Domain.Student.Entities.SectionTitleType("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "short", "Descr"),
                    new Domain.Student.Entities.SectionTitleType("8C3B805D-CFE6-483B-86C3-4C20562F8C15", "long", "Descr")
                };
                stuRefDataRepoMock.Setup(repo => repo.GetSectionTitleTypesAsync(It.IsAny<bool>())).ReturnsAsync(sectionTitleTypes);
                stuRefDataRepoMock.Setup(repo => repo.GetSectionTitleTypesGuidAsync(It.IsAny<string>())).ReturnsAsync("9C3B805D-CFE6-483B-86C3-4C20562F8C15");

                // Mock the section service
                sectionCoordinationService = new SectionCoordinationService(adapterRegistry, sectionRepo, courseRepo, studentRepo, studentReferenceDataRepo,
                    referenceDataRepo, termRepo, studentConfigRepo, configRepo, personRepo, roomRepo, eventRepo, bookRepo, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionRepo = null;
                studentRepo = null;
                roleRepo = null;
                adapterRegistry = null;
                sectionCoordinationService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task SectionService_NewEntity_Null()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PostSection2Async(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section1);
                sectionRepoMock.Setup(repo => repo.GetStatusCodesWithGuidsAsync()).ReturnsAsync(new List<SectionStatusCodeGuid>() { new SectionStatusCodeGuid("asda", "A", "asda") });
                stuRefDataRepoMock.Setup(repo => repo.GetSectionStatusesAsync(It.IsAny<bool>())).ReturnsAsync(new List<SectionStatuses>() { new SectionStatuses("asda", "A", "asda") });
                stuRefDataRepoMock.Setup(repo => repo.GetInstructionalMethodsAsync(It.IsAny<bool>()))
                    .ReturnsAsync(new List<Domain.Student.Entities.InstructionalMethod>()
                        {
                            new Domain.Student.Entities.InstructionalMethod(instructionalMethodGuid, "LEC", "Lecture", false)
                        }
                    );

                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = new Dtos.GuidObject2(creditCategories[0].Guid), CreditType = Dtos.EnumProperties.CreditCategoryType3.Institutional },
                    Measure = Dtos.CreditMeasure2.Credit, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section2 = new Dtos.Section6()
                {
                    Id = section1.Guid,
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    Titles = new List<Dtos.DtoProperties.CoursesTitlesDtoProperty>()
                    {
                        new Dtos.DtoProperties.CoursesTitlesDtoProperty()
                        {
                            Value = "Modern Algebra"
                        }
                    },
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,
                    Status = new Dtos.DtoProperties.SectionStatusDtoProperty()
                    {
                        Category = Dtos.SectionStatus2.Open,
                        Detail = new Dtos.GuidObject2("asda")
                    },
                    InstructionalMethods = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2(instructionalMethodGuid) }
                };
                sectionRepoMock.Setup(repo => repo.PostSection2Async(It.IsAny<Section>())).ReturnsAsync(() => null);
                var res = await sectionCoordinationService.PostSection6Async(section2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_Null_Object_V16()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                var r = await sectionCoordinationService.PostSection6Async(null);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_Null_Guid_V16()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                var r = await sectionCoordinationService.PostSection6Async(new Dtos.Section6());
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_SectionKeyNotFound_V16()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(section1.Guid, It.IsAny<bool>())).Throws(new KeyNotFoundException());
                var r = await sectionCoordinationService.PostSection6Async(new Dtos.Section6()
                {
                    Id = meeting1.Guid
                });
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task SectionService_SectionStatus_Null_V16()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                var r = await sectionCoordinationService.PostSection6Async(new Dtos.Section6()
                {
                    Id = meeting1.Guid,
                    Status = new Dtos.DtoProperties.SectionStatusDtoProperty()
                    {
                        Category = new Dtos.SectionStatus2()
                    }
                });
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_NoPermission_V16()
            {
                var section = new Dtos.Section6() { Id = "999999999999999" };
                var r = await sectionCoordinationService.PostSection6Async(section);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_NullCourse_V16()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = new Dtos.GuidObject2(creditCategories[0].Guid) }, Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section2 = new Dtos.Section6()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    Titles = new List<Dtos.DtoProperties.CoursesTitlesDtoProperty>()
                    {
                        new Dtos.DtoProperties.CoursesTitlesDtoProperty()
                        {
                            Value = "Modern Algebra"
                        }
                    },
                    Course = null,
                    Credits = credit

                };
                var r = await sectionCoordinationService.PostSection6Async(section2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_EmptyTitle_V16()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = new Dtos.GuidObject2(creditCategories[0].Guid) }, Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section2 = new Dtos.Section6()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    Titles = new List<Dtos.DtoProperties.CoursesTitlesDtoProperty>()
                    {
                        new Dtos.DtoProperties.CoursesTitlesDtoProperty()
                        {
                            Value = ""
                        }
                    },
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit
                };
                var r = await sectionCoordinationService.PostSection6Async(section2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_NullStartOn_V16()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = new Dtos.GuidObject2(creditCategories[0].Guid) }, Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section2 = new Dtos.Section6()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = null,
                    Titles = new List<Dtos.DtoProperties.CoursesTitlesDtoProperty>()
                    {
                        new Dtos.DtoProperties.CoursesTitlesDtoProperty()
                        {
                            Value = "MATH"
                        }
                    },
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit
                };
                var r = await sectionCoordinationService.PostSection6Async(section2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_StartOnGreaterThanEndOn_V16()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = new Dtos.GuidObject2(creditCategories[0].Guid) }, Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section2 = new Dtos.Section6()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    EndOn = new DateTime(2010, 09, 01),
                    Titles = new List<Dtos.DtoProperties.CoursesTitlesDtoProperty>()
                    {
                        new Dtos.DtoProperties.CoursesTitlesDtoProperty()
                        {
                            Value = "MATH"
                        }
                    },
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit
                };
                var r = await sectionCoordinationService.PostSection6Async(section2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_NullCreditMeasure_V16()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = new Dtos.GuidObject2(creditCategories[0].Guid) }, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section2 = new Dtos.Section6()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    Titles = new List<Dtos.DtoProperties.CoursesTitlesDtoProperty>()
                    {
                        new Dtos.DtoProperties.CoursesTitlesDtoProperty()
                        {
                            Value = "MATH"
                        }
                    },
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit
                };
                var r = await sectionCoordinationService.PostSection6Async(section2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_NullCreditDetailAndCreditType_V16()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(It.IsAny<String>(), It.IsAny<bool>())).ReturnsAsync(section1);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PostSectionAsync(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section1);
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = null }, Measure = new Dtos.CreditMeasure2(), Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section2 = new Dtos.Section6()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    Titles = new List<Dtos.DtoProperties.CoursesTitlesDtoProperty>()
                    {
                        new Dtos.DtoProperties.CoursesTitlesDtoProperty()
                        {
                            Value = "MATH"
                        }
                    },
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit
                };
                var r = await sectionCoordinationService.PostSection6Async(section2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_NullCreditMinimum_V16()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = new Dtos.GuidObject2(creditCategories[0].Guid) }, Measure = Dtos.CreditMeasure2.CEU, Maximum = 4.0m, Increment = 1.0m}};
                var section2 = new Dtos.Section6()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    EndOn = new DateTime(2010, 09, 01),
                    Titles = new List<Dtos.DtoProperties.CoursesTitlesDtoProperty>()
                    {
                        new Dtos.DtoProperties.CoursesTitlesDtoProperty()
                        {
                            Value = "MATH"
                        }
                    },
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit
                };
                var r = await sectionCoordinationService.PostSection6Async(section2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_NullCreditType_V16()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(It.IsAny<String>(), It.IsAny<bool>())).ReturnsAsync(section1);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PostSectionAsync(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section1);
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2(),
                    Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section = new Dtos.Section6()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    EndOn = new DateTime(2011, 12, 14),
                    Titles = new List<Dtos.DtoProperties.CoursesTitlesDtoProperty>()
                    {
                        new Dtos.DtoProperties.CoursesTitlesDtoProperty()
                        {
                            Value = "Modern Algebra"
                        }
                    },
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,
                    CourseLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2("sjdhd7392") }
                };
                var r = await sectionCoordinationService.PostSection6Async(section);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_NullInstructionalPlaforms_V16()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(It.IsAny<String>(), It.IsAny<bool>())).ReturnsAsync(section1);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PostSectionAsync(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section1);
                referenceDataRepoMock.Setup(repo => repo.GetInstructionalPlatformsAsync(It.IsAny<bool>())).ReturnsAsync(() => null);
                var section = new Dtos.Section6()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    EndOn = new DateTime(2011, 12, 14),
                    Titles = new List<Dtos.DtoProperties.CoursesTitlesDtoProperty>()
                    {
                        new Dtos.DtoProperties.CoursesTitlesDtoProperty()
                        {
                            Value = "Modern Algebra"
                        }
                    },
                    Course = new Dtos.GuidObject2("342"),
                    AcademicLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2(academicLevels[0].Guid) },
                    //Credits = credit,
                    InstructionalPlatform = new Dtos.GuidObject2("jfhjdhjs"),
                    CourseLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2("sjdhd7392") }
                };
                var r = await sectionCoordinationService.PostSection6Async(section);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_NullCreditCategoryOfCredit_V16()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(It.IsAny<String>(), It.IsAny<bool>())).ReturnsAsync(section1);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PostSectionAsync(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section1);
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() { Measure = Dtos.CreditMeasure2.CEU,
                    Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section = new Dtos.Section6()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    EndOn = new DateTime(2011, 12, 14),
                    Titles = new List<Dtos.DtoProperties.CoursesTitlesDtoProperty>()
                    {
                        new Dtos.DtoProperties.CoursesTitlesDtoProperty()
                        {
                            Value = "Modern Algebra"
                        }
                    },
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,

                    CourseLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2("sjdhd7392") }
                };
                var r = await sectionCoordinationService.PostSection6Async(section);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_NullCreditCategories_V16()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PostSectionAsync(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section2);
                stuRefDataRepoMock.Setup(repo => repo.GetCreditCategoriesAsync()).ReturnsAsync(() => null);
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = new Dtos.GuidObject2(creditCategories[0].Guid) }, Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section = new Dtos.Section6()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    EndOn = new DateTime(2011, 12, 14),
                    Titles = new List<Dtos.DtoProperties.CoursesTitlesDtoProperty>()
                    {
                        new Dtos.DtoProperties.CoursesTitlesDtoProperty()
                        {
                            Value = "Modern Algebra"
                        }
                    },
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,

                    CourseLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2("sjdhd7392") }
                };
                var r = await sectionCoordinationService.PostSection6Async(section);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_NullGradeSchemes_V16()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(It.IsAny<String>(), It.IsAny<bool>())).ReturnsAsync(section3);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PostSectionAsync(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section3);
                stuRefDataRepoMock.Setup(repo => repo.GetGradeSchemesAsync()).ReturnsAsync(() => null);
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() { 
                    /*Detail = new Dtos.GuidObject2(creditCategories[0].Guid),*/ CreditType = Dtos.EnumProperties.CreditCategoryType3.Institutional }, Measure = Dtos.CreditMeasure2.Credit, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section = new Dtos.Section6()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    EndOn = new DateTime(2011, 12, 14),
                    Titles = new List<Dtos.DtoProperties.CoursesTitlesDtoProperty>()
                    {
                        new Dtos.DtoProperties.CoursesTitlesDtoProperty()
                        {
                            Value = "Modern Algebra"
                        }
                    },
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,

                    CourseLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2("sjdhd7392") }
                };
                var r = await sectionCoordinationService.PostSection6Async(section);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_NullAcademicLevels_V16()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(It.IsAny<String>(), It.IsAny<bool>())).ReturnsAsync(section1);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PostSectionAsync(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section1);
                stuRefDataRepoMock.Setup(repo => repo.GetAcademicLevelsAsync()).ReturnsAsync(() => null);
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() { 
                    /*Detail = new Dtos.GuidObject2(creditCategories[0].Guid),*/ CreditType = Dtos.EnumProperties.CreditCategoryType3.Transfer }, Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section = new Dtos.Section6()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    EndOn = new DateTime(2011, 12, 14),
                    Titles = new List<Dtos.DtoProperties.CoursesTitlesDtoProperty>()
                    {
                        new Dtos.DtoProperties.CoursesTitlesDtoProperty()
                        {
                            Value = "Modern Algebra"
                        }
                    },
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,

                    CourseLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2("sjdhd7392") }
                };
                var r = await sectionCoordinationService.PostSection6Async(section);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_NullLocations_V16()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(It.IsAny<String>(), It.IsAny<bool>())).ReturnsAsync(section2);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PostSectionAsync(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section2);
                referenceDataRepoMock.Setup(repo => repo.Locations).Returns<Location>(null);
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() { 
                    /*Detail = new Dtos.GuidObject2(creditCategories[0].Guid),*/ CreditType = Dtos.EnumProperties.CreditCategoryType3.ContinuingEducation }, Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section = new Dtos.Section6()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    EndOn = new DateTime(2011, 12, 14),
                    Titles = new List<Dtos.DtoProperties.CoursesTitlesDtoProperty>()
                    {
                        new Dtos.DtoProperties.CoursesTitlesDtoProperty()
                        {
                            Value = "Modern Algebra"
                        }
                    },
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,

                    CourseLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2("sjdhd7392") }
                };
                var r = await sectionCoordinationService.PostSection6Async(section);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_NullCourseLevels_V16()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PostSectionAsync(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section1);
                stuRefDataRepoMock.Setup(repo => repo.GetCourseLevelsAsync()).ReturnsAsync(() => null);
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = new Dtos.GuidObject2(creditCategories[0].Guid) }, Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section2 = new Dtos.Section6()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    Titles = new List<Dtos.DtoProperties.CoursesTitlesDtoProperty>()
                    {
                        new Dtos.DtoProperties.CoursesTitlesDtoProperty()
                        {
                            Value = "Modern Algebra"
                        }
                    },
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,

                };
                var r = await sectionCoordinationService.PostSection6Async(section2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_NullInstructionalMethod_V16()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PostSection2Async(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section1);
                sectionRepoMock.Setup(repo => repo.GetStatusCodesWithGuidsAsync()).ReturnsAsync(new List<SectionStatusCodeGuid>() { new SectionStatusCodeGuid("asda", "A", "asda") });
                stuRefDataRepoMock.Setup(repo => repo.GetSectionStatusesAsync(It.IsAny<bool>())).ReturnsAsync(new List<SectionStatuses>() { new SectionStatuses("asda", "A", "asda") });

                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = new Dtos.GuidObject2(creditCategories[0].Guid), CreditType = Dtos.EnumProperties.CreditCategoryType3.Institutional },
                    Measure = Dtos.CreditMeasure2.Credit, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section2 = new Dtos.Section6()
                {
                    Id = section1.Guid,
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    Titles = new List<Dtos.DtoProperties.CoursesTitlesDtoProperty>()
                    {
                        new Dtos.DtoProperties.CoursesTitlesDtoProperty()
                        {
                            Value = "Modern Algebra"
                        }
                    },
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,
                    Status = new Dtos.DtoProperties.SectionStatusDtoProperty()
                    {
                        Category = Dtos.SectionStatus2.Open,
                        Detail = new Dtos.GuidObject2("asda")
                    }
                };
                var res = await sectionCoordinationService.PostSection6Async(section2);
            }

            [TestMethod]
            public async Task SectionService_Post_V16()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PostSection2Async(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section1);
                sectionRepoMock.Setup(repo => repo.GetStatusCodesWithGuidsAsync()).ReturnsAsync(new List<SectionStatusCodeGuid>() { new SectionStatusCodeGuid("asda", "A", "asda") });
                stuRefDataRepoMock.Setup(repo => repo.GetSectionStatusesAsync(It.IsAny<bool>())).ReturnsAsync(new List<SectionStatuses>() { new SectionStatuses("asda", "A", "asda") });
                stuRefDataRepoMock.Setup(repo => repo.GetInstructionalMethodsAsync(It.IsAny<bool>()))
                    .ReturnsAsync(new List<Domain.Student.Entities.InstructionalMethod>()
                        {
                            new Domain.Student.Entities.InstructionalMethod(instructionalMethodGuid, "LEC", "Lecture", false)
                        }
                    );

                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = new Dtos.GuidObject2(creditCategories[0].Guid), CreditType = Dtos.EnumProperties.CreditCategoryType3.Institutional },
                    Measure = Dtos.CreditMeasure2.Credit, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section2 = new Dtos.Section6()
                {
                    Id = section1.Guid,
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    Titles = new List<Dtos.DtoProperties.CoursesTitlesDtoProperty>()
                    {
                        new Dtos.DtoProperties.CoursesTitlesDtoProperty()
                        {
                            Value = "Modern Algebra"
                        }
                    },
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,
                    Status = new Dtos.DtoProperties.SectionStatusDtoProperty()
                    {
                        Category = Dtos.SectionStatus2.Open,
                        Detail = new Dtos.GuidObject2("asda")
                    },
                    InstructionalMethods = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2(instructionalMethodGuid) }
                };
                var res = await sectionCoordinationService.PostSection6Async(section2);
                Assert.AreEqual(section1.Guid, res.Id);
            }
        }

        [TestClass]
        public class Sections_Put_V16 : CurrentUserSetup
        {
            private SectionCoordinationService sectionCoordinationService;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IStudentReferenceDataRepository> stuRefDataRepoMock;
            private IStudentReferenceDataRepository studentReferenceDataRepo;
            private Mock<IReferenceDataRepository> referenceDataRepoMock;
            private IReferenceDataRepository referenceDataRepo;
            private Mock<ICourseRepository> courseRepoMock;
            private ICourseRepository courseRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IStudentConfigurationRepository> studentConfigRepoMock;
            private IStudentConfigurationRepository studentConfigRepo;
            private Mock<IConfigurationRepository> configRepoMock;
            private IConfigurationRepository configRepo;
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IRoomRepository> roomRepoMock;
            private IRoomRepository roomRepo;
            private Mock<IEventRepository> eventRepoMock;
            private IEventRepository eventRepo;
            private Mock<IBookRepository> bookRepoMock;
            private IBookRepository bookRepo;


            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ILogger logger;
            private ICurrentUserFactory currentUserFactory;

            private Domain.Student.Entities.Section section1;
            private Domain.Student.Entities.Section section2;
            private Domain.Student.Entities.Section section3;
            private Domain.Student.Entities.SectionMeeting meeting1;
            private Domain.Student.Entities.SectionMeeting meeting2;
            private Domain.Student.Entities.SectionMeeting meeting3;
            private Domain.Student.Entities.SectionMeeting meeting4;

            private List<OfferingDepartment> dpts = new List<OfferingDepartment>() { new OfferingDepartment("ART", 100m) };
            private List<string> levels = new List<string>() { "Whatever" };
            private List<SectionStatusItem> statuses = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-60)) };
            private List<SectionStatusItem> statuses2 = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Inactive, "I", DateTime.Today.AddDays(-60)) };
            private List<SectionStatusItem> statuses3 = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Cancelled, "C", DateTime.Today.AddDays(-60)) };
            private IEnumerable<Domain.Student.Entities.Course> allCourses = new TestCourseRepository().GetAsync().Result;

            private List<CreditCategory> creditCategories = new List<CreditCategory>() { new CreditCategory(Guid.NewGuid().ToString(), "RC", "Regular Credit", CreditType.Institutional),
                new CreditCategory(Guid.NewGuid().ToString(), "RC", "Regular Credit", CreditType.Transfer), new CreditCategory(Guid.NewGuid().ToString(), "RC", "Regular Credit", CreditType.ContinuingEducation ) };
            private List<Domain.Student.Entities.AcademicLevel> academicLevels = new List<Domain.Student.Entities.AcademicLevel>() { new Domain.Student.Entities.AcademicLevel(Guid.NewGuid().ToString(), "UG", "Undergraduate") };
            private List<Domain.Student.Entities.GradeScheme> gradeSchemes = new List<Domain.Student.Entities.GradeScheme>() { new Domain.Student.Entities.GradeScheme(Guid.NewGuid().ToString(), "Pass/Fail", "Pass/Fail grade") };
            private List<Domain.Student.Entities.CourseLevel> courseLevels = new List<Domain.Student.Entities.CourseLevel>() { new Domain.Student.Entities.CourseLevel(Guid.NewGuid().ToString(), "100", "Introduction to terms, concepts, and techniques.") };
            private List<Department> departments = new List<Department>() { new Department(Guid.NewGuid().ToString(), "PHYS", "Physics", true) };
            private List<Location> locations = new List<Location>() { new Location(Guid.NewGuid().ToString(), "MD", "Maryland") };

            private string meeting1Guid = Guid.NewGuid().ToString();
            private string instructionalMethodGuid = Guid.NewGuid().ToString();
            private string roomGuid = Guid.NewGuid().ToString();
            private Domain.Entities.Permission permissionCreateandUpdateSections;

            [TestInitialize]
            public void Initialize()
            {
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                roomRepoMock = new Mock<IRoomRepository>();
                roomRepo = roomRepoMock.Object;
                stuRefDataRepoMock = new Mock<IStudentReferenceDataRepository>();
                studentReferenceDataRepo = stuRefDataRepoMock.Object;
                referenceDataRepoMock = new Mock<IReferenceDataRepository>();
                referenceDataRepo = referenceDataRepoMock.Object;
                courseRepoMock = new Mock<ICourseRepository>();
                courseRepo = courseRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                studentConfigRepoMock = new Mock<IStudentConfigurationRepository>();
                studentConfigRepo = studentConfigRepoMock.Object;
                configRepoMock = new Mock<IConfigurationRepository>();
                configRepo = configRepoMock.Object;
                eventRepoMock = new Mock<IEventRepository>();
                eventRepo = eventRepoMock.Object;
                bookRepoMock = new Mock<IBookRepository>();
                bookRepo = bookRepoMock.Object;


                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;

                // Set Current User
                currentUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();

                logger = new Mock<ILogger>().Object;

                // Mock permissions
                permissionCreateandUpdateSections = new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateAndUpdateSection);
                thirdPartyRole.AddPermission(permissionCreateandUpdateSections);

                // Mock the section repo response
                section1 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC1", "1119", "01", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses, true);
                section1.TermId = "2012/FA";
                section1.EndDate = new DateTime(2012, 12, 21);
                section1.Guid = Guid.NewGuid().ToString();

                section2 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC2", "1119", "02", new DateTime(2012, 09, 01), 3.0m, null, "Introduction to Art", "IN", dpts, levels, "UG", statuses2, true);
                section2.TermId = "2012/FA";
                section2.Guid = Guid.NewGuid().ToString();

                section3 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC3", "1119", "03", new DateTime(2011, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses3, true);
                section3.TermId = "2011/FA";
                section3.EndDate = new DateTime(2011, 12, 21);
                section3.Guid = Guid.NewGuid().ToString();

                var sectionFaculty = new Domain.Student.Entities.SectionFaculty("1234", "SEC1", "0000678", "LEC", new DateTime(2012, 09, 01), new DateTime(2011, 12, 21), 100.0m);

                meeting1 = new Domain.Student.Entities.SectionMeeting("123", "SEC1", "LEC", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "W");
                meeting1.Guid = meeting1Guid;
                meeting1.AddFacultyId("0000678");
                meeting1.AddSectionFaculty(sectionFaculty);
                meeting1.StartTime = new DateTimeOffset(2012, 01, 01, 08, 30, 00, new TimeSpan());
                meeting1.EndTime = new DateTimeOffset(2012, 12, 21, 09, 30, 00, new TimeSpan());
                meeting1.Room = "DAN*102";
                section1.AddSectionMeeting(meeting1);

                meeting2 = new Domain.Student.Entities.SectionMeeting("123", "SEC1", "LEC", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "D");
                meeting2.Guid = meeting1Guid;

                meeting3 = new Domain.Student.Entities.SectionMeeting("456", "SEC1", "LEC", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "M");
                meeting3.Guid = meeting1Guid;

                meeting4 = new Domain.Student.Entities.SectionMeeting("789", "SEC1", "LEC", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "Y");
                meeting4.Guid = meeting1Guid;

                Term term1 = new Term("termGuid1", "2011/FA", "2011 Fall", new DateTime(2011, 09, 01), new DateTime(2011, 12, 15), 2012,
                    0, false, false, "2011/FA", false);
                var date1 = new List<DateTime?>() { new DateTime(2011, 09, 15) };
                var regDate = new RegistrationDate("Loc1", null, null, null, null, null, null, null, null, null, date1);
                term1.AddRegistrationDates(regDate);
                Term term2 = new Term("termGuid2", "2012/FA", "2012 Fall", new DateTime(2012, 09, 01), new DateTime(2012, 12, 15), 2013,
                    0, false, false, "2012/FA", false);
                regDate = new RegistrationDate("Loc2", null, null, null, null, null, null, null, null, null, new List<DateTime?>());
                term2.AddRegistrationDates(regDate);
                List<Term> allTerms = new List<Term>() { term1, term2 };
                termRepoMock.Setup(repo => repo.Get()).Returns(allTerms);
                termRepoMock.Setup(repo => repo.GetAsync()).ReturnsAsync(allTerms);

                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(section1.Guid, It.IsAny<bool>())).ReturnsAsync(section1);
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(section2.Guid, It.IsAny<bool>())).ReturnsAsync(section2);
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(section3.Guid, It.IsAny<bool>())).ReturnsAsync(section3);
                sectionRepoMock.Setup(repo => repo.PutSectionMeetingAsync(It.IsAny<Domain.Student.Entities.Section>(), meeting1.Guid)).ReturnsAsync(meeting1);
                sectionRepoMock.Setup(repo => repo.PutSectionMeetingAsync(It.IsAny<Domain.Student.Entities.Section>(), meeting2.Guid)).ReturnsAsync(meeting2);
                sectionRepoMock.Setup(repo => repo.PutSectionMeetingAsync(It.IsAny<Domain.Student.Entities.Section>(), meeting3.Guid)).ReturnsAsync(meeting3);
                courseRepoMock.Setup(repo => repo.GetAsync()).Returns(Task.FromResult(allCourses));
                var course_temp = allCourses.Where(c => c.Id == "342").FirstOrDefault();
                courseRepoMock.Setup(repo => repo.GetCourseByGuidAsync(It.IsAny<String>())).ReturnsAsync(course_temp);
                stuRefDataRepoMock.Setup(repo => repo.GetCreditCategoriesAsync()).ReturnsAsync(creditCategories);
                stuRefDataRepoMock.Setup(repo => repo.GetAcademicLevelsAsync()).ReturnsAsync(academicLevels);
                stuRefDataRepoMock.Setup(repo => repo.GetGradeSchemesAsync()).ReturnsAsync(gradeSchemes);
                stuRefDataRepoMock.Setup(repo => repo.GetCourseLevelsAsync()).ReturnsAsync(courseLevels);
                referenceDataRepoMock.Setup(repo => repo.DepartmentsAsync()).ReturnsAsync(departments);
                referenceDataRepoMock.Setup(repo => repo.Locations).Returns(locations);

                // Configuration defaults and campus calendar information.
                configRepoMock.Setup(repo => repo.GetDefaultsConfiguration()).Returns(new DefaultsConfiguration() { CampusCalendarId = "MAIN" });
                studentConfigRepoMock.Setup(repo => repo.GetCurriculumConfigurationAsync()).ReturnsAsync(new CurriculumConfiguration() { SectionActiveStatusCode = "A", SectionInactiveStatusCode = "I", DefaultInstructionalMethodCode = "INT" });
                eventRepoMock.Setup(repo => repo.GetCalendar("MAIN")).Returns(new CampusCalendar("MAIN", "Default Calendar", new TimeSpan(), new TimeSpan()));

                var sectionStatusCodes = new List<SectionStatusCode>()
                {
                    new SectionStatusCode("P", "Pending", SectionStatus.Active, SectionStatusIntegration.Pending),
                    new SectionStatusCode("C", "Closed", SectionStatus.Active, SectionStatusIntegration.Closed),
                    new SectionStatusCode("C", "Cancelled", SectionStatus.Active, SectionStatusIntegration.Cancelled),
                    new SectionStatusCode("O", "Open", SectionStatus.Active, SectionStatusIntegration.Open),
                };
                stuRefDataRepoMock.Setup(repo => repo.GetSectionStatusCodesAsync()).ReturnsAsync(sectionStatusCodes);

                var sectionTitleTypes = new List<Domain.Student.Entities.SectionTitleType>()
                {
                    new Domain.Student.Entities.SectionTitleType("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "short", "Descr"),
                    new Domain.Student.Entities.SectionTitleType("8C3B805D-CFE6-483B-86C3-4C20562F8C15", "long", "Descr")
                };
                stuRefDataRepoMock.Setup(repo => repo.GetSectionTitleTypesAsync(It.IsAny<bool>())).ReturnsAsync(sectionTitleTypes);
                stuRefDataRepoMock.Setup(repo => repo.GetSectionTitleTypesGuidAsync(It.IsAny<string>())).ReturnsAsync("9C3B805D-CFE6-483B-86C3-4C20562F8C15");

                // Mock the section service
                sectionCoordinationService = new SectionCoordinationService(adapterRegistry, sectionRepo, courseRepo, studentRepo, studentReferenceDataRepo,
                    referenceDataRepo, termRepo, studentConfigRepo, configRepo, personRepo, roomRepo, eventRepo, bookRepo, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionRepo = null;
                studentRepo = null;
                roleRepo = null;
                adapterRegistry = null;
                sectionCoordinationService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_Null_Object_V16()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                var r = await sectionCoordinationService.PutSection6Async(null);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_Null_Guid_V16()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                var r = await sectionCoordinationService.PutSection6Async(new Dtos.Section6());
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_SectionKeyNotFound_V16()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(section1.Guid, It.IsAny<bool>())).Throws(new KeyNotFoundException());
                var r = await sectionCoordinationService.PutSection6Async(new Dtos.Section6()
                {
                    Id = meeting1.Guid
                });
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_NoPermission_V16()
            {
                var section = new Dtos.Section6() { Id = "999999999999999" };
                var r = await sectionCoordinationService.PutSection6Async(section);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_NullCourse_V16()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                //var credit = new List<Dtos.Credit2>() { new Dtos.Credit2() {CreditCategory = new Dtos.CreditIdAndTypeProperty() { 
                //    Detail = new Dtos.GuidObject2(creditCategories[0].Guid) }, Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = new Dtos.GuidObject2(creditCategories[0].Guid) }, Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section2 = new Dtos.Section6()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    Titles = new List<Dtos.DtoProperties.CoursesTitlesDtoProperty>()
                    {
                        new Dtos.DtoProperties.CoursesTitlesDtoProperty()
                        {
                            Value = "Modern Algebra"
                        }
                    },
                    Course = null,
                    Credits = credit
                };
                var r = await sectionCoordinationService.PutSection6Async(section2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_EmptyTitle_V16()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = new Dtos.GuidObject2(creditCategories[0].Guid) }, Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section2 = new Dtos.Section6()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    Titles = new List<Dtos.DtoProperties.CoursesTitlesDtoProperty>()
                    {
                        new Dtos.DtoProperties.CoursesTitlesDtoProperty()
                        {
                            Value = ""
                        }
                    },
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit
                };
                var r = await sectionCoordinationService.PutSection6Async(section2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_NullStartOn_V16()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = new Dtos.GuidObject2(creditCategories[0].Guid) }, Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section2 = new Dtos.Section6()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = null,
                    Titles = new List<Dtos.DtoProperties.CoursesTitlesDtoProperty>()
                    {
                        new Dtos.DtoProperties.CoursesTitlesDtoProperty()
                        {
                            Value = "MATH"
                        }
                    },
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit
                };
                var r = await sectionCoordinationService.PutSection6Async(section2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_StartOnGreaterThanEndOn_V16()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = new Dtos.GuidObject2(creditCategories[0].Guid) }, Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section2 = new Dtos.Section6()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    EndOn = new DateTime(2010, 09, 01),
                    Titles = new List<Dtos.DtoProperties.CoursesTitlesDtoProperty>()
                    {
                        new Dtos.DtoProperties.CoursesTitlesDtoProperty()
                        {
                            Value = "MATH"
                        }
                    },
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit
                };
                var r = await sectionCoordinationService.PutSection6Async(section2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_NullCreditMeasure_V16()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = new Dtos.GuidObject2(creditCategories[0].Guid) }, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section2 = new Dtos.Section6()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    Titles = new List<Dtos.DtoProperties.CoursesTitlesDtoProperty>()
                    {
                        new Dtos.DtoProperties.CoursesTitlesDtoProperty()
                        {
                            Value = "MATH"
                        }
                    },
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit
                };
                var r = await sectionCoordinationService.PutSection6Async(section2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_NullCreditDetailAndCreditType_V16()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(It.IsAny<String>(), It.IsAny<bool>())).ReturnsAsync(section1);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PutSectionAsync(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section1);
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = null }, Measure = new Dtos.CreditMeasure2(), Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section2 = new Dtos.Section6()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    Titles = new List<Dtos.DtoProperties.CoursesTitlesDtoProperty>()
                    {
                        new Dtos.DtoProperties.CoursesTitlesDtoProperty()
                        {
                            Value = "MATH"
                        }
                    },
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit
                };
                var r = await sectionCoordinationService.PutSection6Async(section2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_NullCreditMinimum_V16()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = new Dtos.GuidObject2(creditCategories[0].Guid) }, Measure = Dtos.CreditMeasure2.CEU, Maximum = 4.0m, Increment = 1.0m}};
                var section2 = new Dtos.Section6()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    EndOn = new DateTime(2010, 09, 01),
                    Titles = new List<Dtos.DtoProperties.CoursesTitlesDtoProperty>()
                    {
                        new Dtos.DtoProperties.CoursesTitlesDtoProperty()
                        {
                            Value = "MATH"
                        }
                    },
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit
                };
                var r = await sectionCoordinationService.PutSection6Async(section2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_NullCreditType_V16()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(It.IsAny<String>(), It.IsAny<bool>())).ReturnsAsync(section1);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PutSectionAsync(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section1);
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2(),
                    Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section = new Dtos.Section6()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    EndOn = new DateTime(2011, 12, 14),
                    Titles = new List<Dtos.DtoProperties.CoursesTitlesDtoProperty>()
                    {
                        new Dtos.DtoProperties.CoursesTitlesDtoProperty()
                        {
                            Value = "Modern Algebra"
                        }
                    },
                    Course = new Dtos.GuidObject2("342"),

                    Credits = credit,
                    CourseLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2("sjdhd7392") }
                };
                var r = await sectionCoordinationService.PutSection6Async(section);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_NullInstructionalPlaforms_V16()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(It.IsAny<String>(), It.IsAny<bool>())).ReturnsAsync(section1);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PutSectionAsync(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section1);
                referenceDataRepoMock.Setup(repo => repo.GetInstructionalPlatformsAsync(It.IsAny<bool>())).ReturnsAsync(() => null);
                //var credit = new List<Dtos.Credit2>() { new Dtos.Credit2() {CreditCategory = new Dtos.CreditIdAndTypeProperty(), 
                //Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section = new Dtos.Section6()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    EndOn = new DateTime(2011, 12, 14),
                    Titles = new List<Dtos.DtoProperties.CoursesTitlesDtoProperty>()
                    {
                        new Dtos.DtoProperties.CoursesTitlesDtoProperty()
                        {
                            Value = "Modern Algebra"
                        }
                    },
                    Course = new Dtos.GuidObject2("342"),

                    AcademicLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2(academicLevels[0].Guid) },
                    //Credits = credit,
                    InstructionalPlatform = new Dtos.GuidObject2("jfhjdhjs"),
                    //OwningOrganizations = new List<Dtos.OfferingOrganization2>() { 
                    //    new Dtos.OfferingOrganization2() {Organization = new Dtos.GuidObject2(departments[0].Guid) } },
                    CourseLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2("sjdhd7392") }
                };
                var r = await sectionCoordinationService.PutSection6Async(section);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_NullCreditCategoryOfCredit_V16()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(It.IsAny<String>(), It.IsAny<bool>())).ReturnsAsync(section1);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PutSectionAsync(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section1);
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() { Measure = Dtos.CreditMeasure2.CEU,
                    Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section = new Dtos.Section6()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    EndOn = new DateTime(2011, 12, 14),
                    Titles = new List<Dtos.DtoProperties.CoursesTitlesDtoProperty>()
                    {
                        new Dtos.DtoProperties.CoursesTitlesDtoProperty()
                        {
                            Value = "Modern Algebra"
                        }
                    },
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,

                    CourseLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2("sjdhd7392") }
                };
                var r = await sectionCoordinationService.PutSection6Async(section);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_NullCreditCategories_V16()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PutSectionAsync(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section2);
                stuRefDataRepoMock.Setup(repo => repo.GetCreditCategoriesAsync()).ReturnsAsync(() => null);
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = new Dtos.GuidObject2(creditCategories[0].Guid) }, Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section = new Dtos.Section6()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    EndOn = new DateTime(2011, 12, 14),
                    Titles = new List<Dtos.DtoProperties.CoursesTitlesDtoProperty>()
                    {
                        new Dtos.DtoProperties.CoursesTitlesDtoProperty()
                        {
                            Value = "Modern Algebra"
                        }
                    },
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,

                    CourseLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2("sjdhd7392") }
                };
                var r = await sectionCoordinationService.PutSection6Async(section);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_NullGradeSchemes_V16()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(It.IsAny<String>(), It.IsAny<bool>())).ReturnsAsync(section3);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PutSectionAsync(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section3);
                stuRefDataRepoMock.Setup(repo => repo.GetGradeSchemesAsync()).ReturnsAsync(() => null);
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() { 
                    /*Detail = new Dtos.GuidObject2(creditCategories[0].Guid),*/ CreditType = Dtos.EnumProperties.CreditCategoryType3.Institutional }, Measure = Dtos.CreditMeasure2.Credit, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section = new Dtos.Section6()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    EndOn = new DateTime(2011, 12, 14),
                    Titles = new List<Dtos.DtoProperties.CoursesTitlesDtoProperty>()
                    {
                        new Dtos.DtoProperties.CoursesTitlesDtoProperty()
                        {
                            Value = "Modern Algebra"
                        }
                    },
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,
                    CourseLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2("sjdhd7392") }
                };
                var r = await sectionCoordinationService.PutSection6Async(section);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_NullAcademicLevels_V16()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(It.IsAny<String>(), It.IsAny<bool>())).ReturnsAsync(section1);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PutSectionAsync(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section1);
                stuRefDataRepoMock.Setup(repo => repo.GetAcademicLevelsAsync()).ReturnsAsync(() => null);
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() { 
                    /*Detail = new Dtos.GuidObject2(creditCategories[0].Guid),*/ CreditType = Dtos.EnumProperties.CreditCategoryType3.Transfer }, Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section = new Dtos.Section6()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    EndOn = new DateTime(2011, 12, 14),
                    Titles = new List<Dtos.DtoProperties.CoursesTitlesDtoProperty>()
                    {
                        new Dtos.DtoProperties.CoursesTitlesDtoProperty()
                        {
                            Value = "Modern Algebra"
                        }
                    },
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,

                    CourseLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2("sjdhd7392") }
                };
                var r = await sectionCoordinationService.PutSection6Async(section);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_NullLocations_V16()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(It.IsAny<String>(), It.IsAny<bool>())).ReturnsAsync(section2);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PutSectionAsync(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section2);
                referenceDataRepoMock.Setup(repo => repo.Locations).Returns<Location>(null);
                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() { 
                    /*Detail = new Dtos.GuidObject2(creditCategories[0].Guid),*/ CreditType = Dtos.EnumProperties.CreditCategoryType3.ContinuingEducation }, Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section = new Dtos.Section6()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    EndOn = new DateTime(2011, 12, 14),
                    Titles = new List<Dtos.DtoProperties.CoursesTitlesDtoProperty>()
                    {
                        new Dtos.DtoProperties.CoursesTitlesDtoProperty()
                        {
                            Value = "Modern Algebra"
                        }
                    },
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,

                    CourseLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2("sjdhd7392") }
                };
                var r = await sectionCoordinationService.PutSection6Async(section);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_NullCourseLevels_V16()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PutSectionAsync(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section1);
                stuRefDataRepoMock.Setup(repo => repo.GetCourseLevelsAsync()).ReturnsAsync(() => null);



                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = new Dtos.GuidObject2(creditCategories[0].Guid) }, Measure = Dtos.CreditMeasure2.CEU, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section2 = new Dtos.Section6()
                {
                    Id = "999999999999999",
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    Titles = new List<Dtos.DtoProperties.CoursesTitlesDtoProperty>()
                    {
                        new Dtos.DtoProperties.CoursesTitlesDtoProperty()
                        {
                            Value = "Modern Algebra"
                        }
                    },
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,

                };
                var r = await sectionCoordinationService.PutSection6Async(section2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionService_NullInstructionalMethod_V16()
            {
                List<Division> division = new List<Division>()
                {
                    new Division("8c8c1a04-984a-4000-a719-ab2df5105c58", "div1", "Descr1")
                };
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PutSection2Async(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section1);
                sectionRepoMock.Setup(repo => repo.GetStatusCodesWithGuidsAsync()).ReturnsAsync(new List<SectionStatusCodeGuid>() { new SectionStatusCodeGuid("asda", "A", "asda") });
                stuRefDataRepoMock.Setup(repo => repo.GetSectionStatusesAsync(It.IsAny<bool>())).ReturnsAsync(new List<SectionStatuses>() { new SectionStatuses("asda", "A", "asda") });
                referenceDataRepoMock.Setup(repo => repo.GetDivisionsAsync(It.IsAny<bool>())).ReturnsAsync(division);

                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = new Dtos.GuidObject2(creditCategories[0].Guid), CreditType = Dtos.EnumProperties.CreditCategoryType3.Institutional },
                    Measure = Dtos.CreditMeasure2.Credit, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section2 = new Dtos.Section6()
                {
                    Id = section1.Guid,
                    Billing = -1,
                    Number = "01",
                    StartOn = default(DateTime?),
                    EndOn = default(DateTime?),
                    Titles = null,
                    Course = new Dtos.GuidObject2("342"),
                    Credits = new List<Dtos.DtoProperties.SectionCreditDtoProperty>()
                    {
                        new Dtos.DtoProperties.SectionCreditDtoProperty()
                        {
                            CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2()
                            {
                                Detail = new Dtos.GuidObject2()
                            },
                            Increment = 1,
                            Measure = Dtos.CreditMeasure2.CEU
                        },
                        new Dtos.DtoProperties.SectionCreditDtoProperty()
                        {
                            CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2()
                            {
                                CreditType = CreditCategoryType3.Exam
                            }
                        }
                    },
                    CourseLevels = new List<Dtos.GuidObject2>()
                    {
                        new Dtos.GuidObject2()
                    },
                    InstructionalPlatform = new Dtos.GuidObject2(),
                    AcademicLevels = new List<Dtos.GuidObject2>()
                    {
                        new Dtos.GuidObject2()
                    },
                    GradeSchemes = new List<Dtos.GuidObject2>()
                    {
                        new Dtos.GuidObject2()
                    },
                    OwningInstitutionUnits = new List<Dtos.OwningInstitutionUnit>()
                    {
                        new Dtos.OwningInstitutionUnit(){ InstitutionUnit = new Dtos.GuidObject2(), OwnershipPercentage = 0 },
                        new Dtos.OwningInstitutionUnit(){ InstitutionUnit = new Dtos.GuidObject2("8c8c1a04-984a-4000-a719-ab2df5105c58")}
                    },
                    Duration = new Dtos.SectionDuration2()
                    {
                        Unit = Dtos.DurationUnit2.Months,
                        Length = -1
                    },
                    ReservedSeatsMaximum = 1,
                    Status = new Dtos.DtoProperties.SectionStatusDtoProperty()
                    {
                        Category = Dtos.SectionStatus2.Open,
                        Detail = new Dtos.GuidObject2("asda")
                    },
                    Waitlist = new Dtos.DtoProperties.SectionWaitlistDtoProperty()
                    {
                        RegistrationInterval = new Dtos.DtoProperties.SectionRegistrationIntervalDtoProperty()
                        {
                            Unit = SectionWaitlistRegistrationIntervalUnit.Hour,
                            Value = null
                        }
                    }
                };
                var res = await sectionCoordinationService.PutSection6Async(section2);
            }

            [TestMethod]
            public async Task SectionService_Put_V16()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { thirdPartyRole });
                sectionRepoMock.Setup(repo => repo.PutSection2Async(It.IsAny<Domain.Student.Entities.Section>())).ReturnsAsync(section1);
                sectionRepoMock.Setup(repo => repo.GetStatusCodesWithGuidsAsync()).ReturnsAsync(new List<SectionStatusCodeGuid>() { new SectionStatusCodeGuid("asda", "A", "asda") });
                stuRefDataRepoMock.Setup(repo => repo.GetSectionStatusesAsync(It.IsAny<bool>())).ReturnsAsync(new List<SectionStatuses>() { new SectionStatuses("asda", "A", "asda") });
                stuRefDataRepoMock.Setup(repo => repo.GetInstructionalMethodsAsync(It.IsAny<bool>()))
                    .ReturnsAsync(new List<Domain.Student.Entities.InstructionalMethod>()
                        {
                            new Domain.Student.Entities.InstructionalMethod(instructionalMethodGuid, "LEC", "Lecture", false)
                        }
                    );

                var credit = new List<Dtos.DtoProperties.SectionCreditDtoProperty>() { new Dtos.DtoProperties.SectionCreditDtoProperty() {CreditCategory = new Dtos.DtoProperties.CreditIdAndTypeProperty2() {
                    Detail = new Dtos.GuidObject2(creditCategories[0].Guid), CreditType = Dtos.EnumProperties.CreditCategoryType3.Institutional },
                    Measure = Dtos.CreditMeasure2.Credit, Minimum = 1.0m, Maximum = 4.0m, Increment = 1.0m}};
                var section2 = new Dtos.Section6()
                {
                    Id = section1.Guid,
                    Number = "01",
                    StartOn = new DateTime(2011, 09, 01),
                    Titles = new List<Dtos.DtoProperties.CoursesTitlesDtoProperty>()
                    {
                        new Dtos.DtoProperties.CoursesTitlesDtoProperty()
                        {
                            Value = "Modern Algebra"
                        }
                    },
                    Course = new Dtos.GuidObject2("342"),
                    Credits = credit,

                    Status = new Dtos.DtoProperties.SectionStatusDtoProperty()
                    {
                        Category = Dtos.SectionStatus2.Open,
                        Detail = new Dtos.GuidObject2("asda")
                    },
                    InstructionalMethods = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2(instructionalMethodGuid) }
                };
                var res = await sectionCoordinationService.PutSection6Async(section2);
                Assert.AreEqual(section1.Guid, res.Id);
            }
        }

        [TestClass]
        public class SectionsMaximum_V8 : CurrentUserSetup
        {
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<ISectionRepository> sectionRepoMock;
            private Mock<IStudentRepository> studentRepoMock;
            private Mock<IStudentReferenceDataRepository> stuRefDataRepoMock;
            private Mock<IReferenceDataRepository> referenceDataRepoMock;
            private Mock<ICourseRepository> courseRepoMock;
            private Mock<ITermRepository> termRepoMock;
            private Mock<IStudentConfigurationRepository> studentConfigRepoMock;
            private Mock<IConfigurationRepository> configRepoMock;
            private Mock<IPersonRepository> personRepoMock;
            private Mock<IRoomRepository> roomRepoMock;
            private Mock<IEventRepository> eventRepoMock;
            private Mock<IBookRepository> bookRepoMock;
            private Mock<IRoleRepository> roleRepoMock;
            private Mock<ILogger> loggerMock;
            private ICurrentUserFactory currentUserFactory;


            private SectionCoordinationService sectionCoordinationService;
            Tuple<IEnumerable<Domain.Student.Entities.Section>, int> sectionMaxEntitiesTuple;
            IEnumerable<Domain.Student.Entities.Section> sectionEntities;

            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course> courseEntities;
            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Subject> subjectEntities;
            IEnumerable<Domain.Student.Entities.CreditCategory> creditCategoryEntities;
            IEnumerable<Domain.Student.Entities.GradeScheme> gradeSchemeEntities;
            IEnumerable<Domain.Student.Entities.CourseLevel> courseLevelEntities;
            IEnumerable<Domain.Student.Entities.AcademicPeriod> acadPeriodsEntities;
            IEnumerable<Domain.Student.Entities.AcademicLevel> academicLevelEntities;
            IEnumerable<Domain.Base.Entities.InstructionalPlatform> instructionalPlatformEntities;
            IEnumerable<Domain.Base.Entities.Location> locationEntities;
            IEnumerable<Domain.Base.Entities.Department> departmentEntities;
            IEnumerable<Domain.Student.Entities.InstructionalMethod> instrMethodEntities;
            IEnumerable<Domain.Base.Entities.Room> roomEntities;

            Domain.Base.Entities.Person person;
            private Domain.Student.Entities.SectionMeeting meeting1;


            [TestInitialize]
            public void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                sectionRepoMock = new Mock<ISectionRepository>();
                courseRepoMock = new Mock<ICourseRepository>();
                studentRepoMock = new Mock<IStudentRepository>();
                stuRefDataRepoMock = new Mock<IStudentReferenceDataRepository>();
                referenceDataRepoMock = new Mock<IReferenceDataRepository>();
                termRepoMock = new Mock<ITermRepository>();
                studentConfigRepoMock = new Mock<IStudentConfigurationRepository>();
                configRepoMock = new Mock<IConfigurationRepository>();
                personRepoMock = new Mock<IPersonRepository>();
                roomRepoMock = new Mock<IRoomRepository>();
                eventRepoMock = new Mock<IEventRepository>();
                bookRepoMock = new Mock<IBookRepository>();
                roleRepoMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();

                currentUserFactory = new CurrentUserSetup.StudentUserFactory();
                BuildData();

                sectionCoordinationService = new SectionCoordinationService(adapterRegistryMock.Object, sectionRepoMock.Object, courseRepoMock.Object, studentRepoMock.Object,
                    stuRefDataRepoMock.Object, referenceDataRepoMock.Object, termRepoMock.Object, studentConfigRepoMock.Object, configRepoMock.Object, personRepoMock.Object,
                    roomRepoMock.Object, eventRepoMock.Object, bookRepoMock.Object, currentUserFactory, roleRepoMock.Object, loggerMock.Object);

            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                sectionRepoMock = null;
                courseRepoMock = null;
                studentRepoMock = null;
                stuRefDataRepoMock = null;
                referenceDataRepoMock = null;
                termRepoMock = null;
                studentConfigRepoMock = null;
                configRepoMock = null;
                personRepoMock = null;
                roomRepoMock = null;
                eventRepoMock = null;
                roleRepoMock = null;
                sectionCoordinationService = null;
                courseEntities = null;
                subjectEntities = null;
                creditCategoryEntities = null;
                gradeSchemeEntities = null;
                courseLevelEntities = null;
                acadPeriodsEntities = null;
                academicLevelEntities = null;
                instructionalPlatformEntities = null;
                locationEntities = null;
                departmentEntities = null;
                instrMethodEntities = null;
                roomEntities = null;
                person = null;
                meeting1 = null;
            }

            [TestMethod]
            public async Task SectionCoordinationServiceTests_V8_GetSectionsMaximum8Async_ValidateAllFilters()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionsAsync(0, 3, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(sectionMaxEntitiesTuple);

                var results = await sectionCoordinationService.GetSectionsMaximum3Async(0, 3, It.IsAny<string>(), "2016-01-01", "2016-10-01", It.IsAny<string>(), It.IsAny<string>(), "840e72f0-57b9-42a2-ae88-df3c2262fbbc", "8d3fcd4d-cec8-4405-90eb-948392bd0a7e",
                    new List<string>() { "558ca14c-718a-4b6e-8d92-77f498034f9f" }, "38b4330c-befa-435e-81d5-c3ffd52759f2", "b0eba383-5acf-4050-949d-8bb7a17c5012", "open", new List<string>() { "1a2e2906-d46b-4698-80f6-af87b8083c64" });

                Assert.IsNotNull(results);
                Assert.AreEqual(0, results.Item1.Count());
            }

            [TestMethod]
            public async Task SectionCoordinationServiceTests_V8_GetSectionMaximumByGuid3Async_CE()
            {
                sectionEntities = new List<Domain.Student.Entities.Section>()
                {
                    new Domain.Student.Entities.Section("1", "180", "111", new DateTime(2016, 01, 01), 3, 3, "Title 1", "C", new Collection<Domain.Student.Entities.OfferingDepartment>(){ new Domain.Student.Entities.OfferingDepartment("MATH") },
                        new Collection<string>(){ "100", "200"}, "UG", new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddYears(-6)) })
                        {
                            Guid = "0b983700-29eb-46ff-8616-21a8c3a48a0c",
                            LearningProvider = "CE",
                            TermId = "2012/FA",
                            Location = "MAIN",
                            NumberOfWeeks = 2,
                            NumberOnWaitlist = 10,
                            GradeSchemeCode = "UG"
                        }
                };
                sectionMaxEntitiesTuple = new Tuple<IEnumerable<Section>, int>(sectionEntities, sectionEntities.Count());
                sectionRepoMock.Setup(repo => repo.GetSectionByGuidAsync(It.IsAny<string>())).ReturnsAsync(sectionEntities.ToList()[0]);

                var result = await sectionCoordinationService.GetSectionMaximumByGuid3Async("0b983700-29eb-46ff-8616-21a8c3a48a0c");

                Assert.IsNotNull(result);
            }

            [TestMethod]
            public async Task SectionCoordinationServiceTests_V8_GetSectionMaximumByGuid3Async_Transfer()
            {
                sectionEntities = new List<Domain.Student.Entities.Section>()
                {
                    new Domain.Student.Entities.Section("1", "180", "111", new DateTime(2016, 01, 01), 3, 3, "Title 1", "T", new Collection<Domain.Student.Entities.OfferingDepartment>(){ new Domain.Student.Entities.OfferingDepartment("MATH") },
                        new Collection<string>(){ "100", "200"}, "UG", new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddYears(-6)) })
                        {
                            Guid = "0b983700-29eb-46ff-8616-21a8c3a48a0c",
                            LearningProvider = "CE",
                            TermId = "2012/FA",
                            Location = "MAIN",
                            NumberOfWeeks = 2,
                            NumberOnWaitlist = 10,
                            GradeSchemeCode = "UG"
                        }
                };
                sectionMaxEntitiesTuple = new Tuple<IEnumerable<Section>, int>(sectionEntities, sectionEntities.Count());
                sectionRepoMock.Setup(repo => repo.GetSectionByGuidAsync(It.IsAny<string>())).ReturnsAsync(sectionEntities.ToList()[0]);

                var result = await sectionCoordinationService.GetSectionMaximumByGuid3Async("0b983700-29eb-46ff-8616-21a8c3a48a0c");

                Assert.IsNotNull(result);
            }

            [TestMethod]
            public async Task SectionCoordinationServiceTests_V8_GetSectionMaximumByGuid3Async_Exchange()
            {
                sectionEntities = new List<Domain.Student.Entities.Section>()
                {
                    new Domain.Student.Entities.Section("1", "180", "111", new DateTime(2016, 01, 01), 3, 3, "Title 1", "E", new Collection<Domain.Student.Entities.OfferingDepartment>(){ new Domain.Student.Entities.OfferingDepartment("MATH") },
                        new Collection<string>(){ "100", "200"}, "UG", new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddYears(-6)) })
                        {
                            Guid = "0b983700-29eb-46ff-8616-21a8c3a48a0c",
                            LearningProvider = "CE",
                            TermId = "2012/FA",
                            Location = "MAIN",
                            NumberOfWeeks = 2,
                            NumberOnWaitlist = 10,
                            GradeSchemeCode = "UG"
                        }
                };
                sectionMaxEntitiesTuple = new Tuple<IEnumerable<Section>, int>(sectionEntities, sectionEntities.Count());
                sectionRepoMock.Setup(repo => repo.GetSectionByGuidAsync(It.IsAny<string>())).ReturnsAsync(sectionEntities.ToList()[0]);

                var result = await sectionCoordinationService.GetSectionMaximumByGuid3Async("0b983700-29eb-46ff-8616-21a8c3a48a0c");

                Assert.IsNotNull(result);
            }

            [TestMethod]
            public async Task SectionCoordinationServiceTests_V8_GetSectionMaximumByGuid3Async_Other()
            {
                sectionEntities = new List<Domain.Student.Entities.Section>()
                {
                    new Domain.Student.Entities.Section("1", "180", "111", new DateTime(2016, 01, 01), 3, 3, "Title 1", "O", new Collection<Domain.Student.Entities.OfferingDepartment>(){ new Domain.Student.Entities.OfferingDepartment("MATH") },
                        new Collection<string>(){ "100", "200"}, "UG", new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddYears(-6)) })
                        {
                            Guid = "0b983700-29eb-46ff-8616-21a8c3a48a0c",
                            LearningProvider = "CE",
                            TermId = "2012/FA",
                            Location = "MAIN",
                            NumberOfWeeks = 2,
                            NumberOnWaitlist = 10,
                            GradeSchemeCode = "UG"
                        }
                };
                sectionMaxEntitiesTuple = new Tuple<IEnumerable<Section>, int>(sectionEntities, sectionEntities.Count());
                sectionRepoMock.Setup(repo => repo.GetSectionByGuidAsync(It.IsAny<string>())).ReturnsAsync(sectionEntities.ToList()[0]);

                var result = await sectionCoordinationService.GetSectionMaximumByGuid3Async("0b983700-29eb-46ff-8616-21a8c3a48a0c");

                Assert.IsNotNull(result);
            }

            [TestMethod]
            public async Task SectionCoordinationServiceTests_V8_GetSectionMaximumByGuid3Async_NoCredit()
            {
                sectionEntities = new List<Domain.Student.Entities.Section>()
                {
                    new Domain.Student.Entities.Section("1", "180", "111", new DateTime(2016, 01, 01), 3, 3, "Title 1", "N", new Collection<Domain.Student.Entities.OfferingDepartment>(){ new Domain.Student.Entities.OfferingDepartment("MATH") },
                        new Collection<string>(){ "100", "200"}, "UG", new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddYears(-6)) })
                        {
                            Guid = "0b983700-29eb-46ff-8616-21a8c3a48a0c",
                            LearningProvider = "CE",
                            TermId = "2012/FA",
                            Location = "MAIN",
                            NumberOfWeeks = 2,
                            NumberOnWaitlist = 10,
                            GradeSchemeCode = "UG"
                        }
                };
                sectionMaxEntitiesTuple = new Tuple<IEnumerable<Section>, int>(sectionEntities, sectionEntities.Count());
                sectionRepoMock.Setup(repo => repo.GetSectionByGuidAsync(It.IsAny<string>())).ReturnsAsync(sectionEntities.ToList()[0]);

                var result = await sectionCoordinationService.GetSectionMaximumByGuid3Async("0b983700-29eb-46ff-8616-21a8c3a48a0c");

                Assert.IsNotNull(result);
            }

            [TestMethod]
            public async Task SectionCoordinationServiceTests_V8_GetSectionMaximumByGuid3Async_WithId_CreditTypeCode_Blank()
            {
                sectionEntities = new List<Domain.Student.Entities.Section>()
                {
                    new Domain.Student.Entities.Section("1", "180", "111", new DateTime(2016, 01, 01), 3, 3, "Title 1", " ", new Collection<Domain.Student.Entities.OfferingDepartment>(){ new Domain.Student.Entities.OfferingDepartment("MATH") },
                        new Collection<string>(){ "100", "200"}, "UG", new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddYears(-6)) })
                        {
                            Guid = "0b983700-29eb-46ff-8616-21a8c3a48a0c",
                            LearningProvider = "CE",
                            TermId = "2012/FA",
                            Location = "MAIN",
                            NumberOfWeeks = 2,
                            NumberOnWaitlist = 10,
                            GradeSchemeCode = "UG"
                        }
                };
                sectionMaxEntitiesTuple = new Tuple<IEnumerable<Section>, int>(sectionEntities, sectionEntities.Count());
                sectionRepoMock.Setup(repo => repo.GetSectionByGuidAsync(It.IsAny<string>())).ReturnsAsync(sectionEntities.ToList()[0]);

                var result = await sectionCoordinationService.GetSectionMaximumByGuid3Async("0b983700-29eb-46ff-8616-21a8c3a48a0c");

                Assert.IsNotNull(result);
            }

            [TestMethod]
            public async Task SectionCoordinationServiceTests_V8_GetSectionMaximumByGuid3Async_WithId_No_CEUS()
            {
                sectionEntities = new List<Domain.Student.Entities.Section>()
                {
                    new Domain.Student.Entities.Section("1", "180", "111", new DateTime(2016, 01, 01), 3, null, "Title 1", "I", new Collection<Domain.Student.Entities.OfferingDepartment>(){ new Domain.Student.Entities.OfferingDepartment("MATH") },
                        new Collection<string>(){ "100", "200"}, "UG", new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddYears(-6)) })
                        {
                            Guid = "0b983700-29eb-46ff-8616-21a8c3a48a0c",
                            LearningProvider = "CE",
                            TermId = "2012/FA",
                            Location = "MAIN",
                            NumberOfWeeks = 2,
                            NumberOnWaitlist = 10,
                            GradeSchemeCode = "UG"
                        }
                };
                sectionMaxEntitiesTuple = new Tuple<IEnumerable<Section>, int>(sectionEntities, sectionEntities.Count());
                sectionRepoMock.Setup(repo => repo.GetSectionByGuidAsync(It.IsAny<string>())).ReturnsAsync(sectionEntities.ToList()[0]);

                var result = await sectionCoordinationService.GetSectionMaximumByGuid3Async("0b983700-29eb-46ff-8616-21a8c3a48a0c");

                Assert.IsNotNull(result);
            }

            [TestMethod]
            public async Task SectionCoordinationServiceTests_V8_GetSectionMaximumByGuid3Async_WithId_Freq_Daily()
            {
                var sectionFaculty = new Domain.Student.Entities.SectionFaculty("1234", "SEC1", "0000678", "LG", new DateTime(2012, 09, 01), new DateTime(2011, 12, 21), 100.0m);

                meeting1 = new Domain.Student.Entities.SectionMeeting("123", "SEC1", "LG", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "D");
                meeting1.Guid = "1f1485cb-2705-4b59-9f51-7aa85c79b377";
                meeting1.AddFacultyId("0000678");
                meeting1.AddSectionFaculty(sectionFaculty);
                meeting1.StartTime = new DateTimeOffset(2012, 01, 01, 08, 30, 00, new TimeSpan());
                meeting1.EndTime = new DateTimeOffset(2012, 12, 21, 09, 30, 00, new TimeSpan());
                meeting1.Room = "COE*Room 1";
                var sectionMeetings = new List<Domain.Student.Entities.SectionMeeting>();
                sectionMeetings.Add(meeting1);
                var tuple = new Tuple<IEnumerable<Domain.Student.Entities.SectionMeeting>, int>(sectionMeetings, 1);
                sectionRepoMock.Setup(repo => repo.GetSectionMeetingAsync(0, 0, "1", "", "", "", "", new List<string>(), new List<string>(), new List<string>(), "")).ReturnsAsync(tuple);

                sectionRepoMock.Setup(repo => repo.GetSectionByGuidAsync(It.IsAny<string>())).ReturnsAsync(sectionEntities.ToList()[0]);

                var result = await sectionCoordinationService.GetSectionMaximumByGuid3Async("0b983700-29eb-46ff-8616-21a8c3a48a0c");

                Assert.IsNotNull(result);
                Assert.AreEqual(Dtos.FrequencyType2.Daily, result.InstructionalEvents.ToList()[0].Recurrence.RepeatRule.Type);
            }

            [TestMethod]
            public async Task SectionCoordinationServiceTests_V8_GetSectionMaximumByGuid3Async_WithId_Freq_Monthly_WithDays()
            {
                var sectionFaculty = new Domain.Student.Entities.SectionFaculty("1234", "SEC1", "0000678", "LG", new DateTime(2012, 09, 01), new DateTime(2011, 12, 21), 100.0m);

                meeting1 = new Domain.Student.Entities.SectionMeeting("123", "SEC1", "LG", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "M");
                meeting1.Days = new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday };
                meeting1.Guid = "1f1485cb-2705-4b59-9f51-7aa85c79b377";
                meeting1.AddFacultyId("0000678");
                meeting1.AddSectionFaculty(sectionFaculty);
                meeting1.StartTime = new DateTimeOffset(2012, 01, 01, 08, 30, 00, new TimeSpan());
                meeting1.EndTime = new DateTimeOffset(2012, 12, 21, 09, 30, 00, new TimeSpan());
                meeting1.Room = "COE*Room 1";
                var sectionMeetings = new List<Domain.Student.Entities.SectionMeeting>();
                sectionMeetings.Add(meeting1);
                var tuple = new Tuple<IEnumerable<Domain.Student.Entities.SectionMeeting>, int>(sectionMeetings, 1);
                sectionRepoMock.Setup(repo => repo.GetSectionMeetingAsync(0, 0, "1", "", "", "", "", new List<string>(), new List<string>(), new List<string>(), "")).ReturnsAsync(tuple);

                sectionRepoMock.Setup(repo => repo.GetSectionByGuidAsync(It.IsAny<string>())).ReturnsAsync(sectionEntities.ToList()[0]);

                var result = await sectionCoordinationService.GetSectionMaximumByGuid3Async("0b983700-29eb-46ff-8616-21a8c3a48a0c");

                Assert.IsNotNull(result);
                Assert.AreEqual(Dtos.FrequencyType2.Monthly, result.InstructionalEvents.ToList()[0].Recurrence.RepeatRule.Type);
            }

            [TestMethod]
            public async Task SectionCoordinationServiceTests_V8_GetSectionMaximumByGuid3Async_WithId_Freq_Monthly_WithoutDays()
            {
                var sectionFaculty = new Domain.Student.Entities.SectionFaculty("1234", "SEC1", "0000678", "LG", new DateTime(2012, 09, 01), new DateTime(2011, 12, 21), 100.0m);

                meeting1 = new Domain.Student.Entities.SectionMeeting("123", "SEC1", "LG", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "M");
                meeting1.Guid = "1f1485cb-2705-4b59-9f51-7aa85c79b377";
                meeting1.AddFacultyId("0000678");
                meeting1.AddSectionFaculty(sectionFaculty);
                meeting1.StartTime = new DateTimeOffset(2012, 01, 01, 08, 30, 00, new TimeSpan());
                meeting1.EndTime = new DateTimeOffset(2012, 12, 21, 09, 30, 00, new TimeSpan());
                meeting1.Room = "COE*Room 1";
                var sectionMeetings = new List<Domain.Student.Entities.SectionMeeting>();
                sectionMeetings.Add(meeting1);
                var tuple = new Tuple<IEnumerable<Domain.Student.Entities.SectionMeeting>, int>(sectionMeetings, 1);
                sectionRepoMock.Setup(repo => repo.GetSectionMeetingAsync(0, 0, "1", "", "", "", "", new List<string>(), new List<string>(), new List<string>(), "")).ReturnsAsync(tuple);

                sectionRepoMock.Setup(repo => repo.GetSectionByGuidAsync(It.IsAny<string>())).ReturnsAsync(sectionEntities.ToList()[0]);

                var result = await sectionCoordinationService.GetSectionMaximumByGuid3Async("0b983700-29eb-46ff-8616-21a8c3a48a0c");

                Assert.IsNotNull(result);
                Assert.AreEqual(Dtos.FrequencyType2.Monthly, result.InstructionalEvents.ToList()[0].Recurrence.RepeatRule.Type);
            }

            [TestMethod]
            public async Task SectionCoordinationServiceTests_V8_GetSectionMaximumByGuid3Async_WithId_Freq_Yearly()
            {
                var sectionFaculty = new Domain.Student.Entities.SectionFaculty("1234", "SEC1", "0000678", "LG", new DateTime(2012, 09, 01), new DateTime(2011, 12, 21), 100.0m);

                meeting1 = new Domain.Student.Entities.SectionMeeting("123", "SEC1", "LG", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "Y");
                meeting1.Guid = "1f1485cb-2705-4b59-9f51-7aa85c79b377";
                meeting1.AddFacultyId("0000678");
                meeting1.AddSectionFaculty(sectionFaculty);
                meeting1.StartTime = new DateTimeOffset(2012, 01, 01, 08, 30, 00, new TimeSpan());
                meeting1.EndTime = new DateTimeOffset(2012, 12, 21, 09, 30, 00, new TimeSpan());
                meeting1.Room = "COE*Room 1";
                var sectionMeetings = new List<Domain.Student.Entities.SectionMeeting>();
                sectionMeetings.Add(meeting1);
                var tuple = new Tuple<IEnumerable<Domain.Student.Entities.SectionMeeting>, int>(sectionMeetings, 1);
                sectionRepoMock.Setup(repo => repo.GetSectionMeetingAsync(0, 0, "1", "", "", "", "", new List<string>(), new List<string>(), new List<string>(), "")).ReturnsAsync(tuple);

                sectionRepoMock.Setup(repo => repo.GetSectionByGuidAsync(It.IsAny<string>())).ReturnsAsync(sectionEntities.ToList()[0]);

                var result = await sectionCoordinationService.GetSectionMaximumByGuid3Async("0b983700-29eb-46ff-8616-21a8c3a48a0c");

                Assert.IsNotNull(result);
                Assert.AreEqual(Dtos.FrequencyType2.Yearly, result.InstructionalEvents.ToList()[0].Recurrence.RepeatRule.Type);
            }

            [TestMethod]
            public async Task SectionCoordinationServiceTests_V8_GetSectionMaximumByGuid3Async_WithId()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionByGuidAsync(It.IsAny<string>())).ReturnsAsync(sectionEntities.ToList()[0]);

                var result = await sectionCoordinationService.GetSectionMaximumByGuid3Async("0b983700-29eb-46ff-8616-21a8c3a48a0c");

                Assert.IsNotNull(result);
            }

            [TestMethod]
            public async Task SectionCoordinationServiceTests_V8_GetSectionMaximum3Async_WithBadPeriodFilter_ReturnsEmptySet()
            {
                var emptyResult = new Tuple<IEnumerable<Section>, int>(new List<Domain.Student.Entities.Section>(), 0);
                sectionRepoMock.Setup(repo => repo.GetSectionsAsync(1, 1, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(),
                    It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(emptyResult);

                var result = await sectionCoordinationService.GetSectionsMaximum3Async(1, 1, "", "", "", "", "", "", "badperiod", null, "", "", "", null);
                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Item1.Count());
                Assert.AreEqual(0, result.Item2);
            }
            [TestMethod]
            public async Task SectionCoordinationServiceTests_V8_GetSectionMaximum3Async_WithBadSubjectFilter_ReturnsEmptySet()
            {
                var emptyResult = new Tuple<IEnumerable<Section>, int>(new List<Domain.Student.Entities.Section>(), 0);
                sectionRepoMock.Setup(repo => repo.GetSectionsAsync(1, 1, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                  It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                  It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(),
                  It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(emptyResult);
                sectionRepoMock.Setup(repo => repo.GetSectionsAsync(1, 1, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                 It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                 It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                 It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(emptyResult);
                var result = await sectionCoordinationService.GetSectionsMaximum3Async(1, 1, "", "", "", "badsubject", "", "", "", null, "", "", "", null);
                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Item1.Count());
                Assert.AreEqual(0, result.Item2);
            }

            [TestMethod]
            public async Task SectionCoordinationServiceTests_V8_GetSectionMaximum3Async_WithBadPlatformFilter_ReturnsEmptySet()
            {
                var emptyResult = new Tuple<IEnumerable<Section>, int>(new List<Domain.Student.Entities.Section>(), 0);
                sectionRepoMock.Setup(repo => repo.GetSectionsAsync(1, 1, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(emptyResult);
                var result = await sectionCoordinationService.GetSectionsMaximum3Async(1, 1, "", "", "", "", "", "badplatform", "", null, "", "", "", null);
                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Item1.Count());
                Assert.AreEqual(0, result.Item2);
            }
            [TestMethod]
            public async Task SectionCoordinationServiceTests_V8_GetSectionMaximum3Async_WithBadLevelFilter_ReturnsEmptySet()
            {
                var emptyResult = new Tuple<IEnumerable<Section>, int>(new List<Domain.Student.Entities.Section>(), 0);
                sectionRepoMock.Setup(repo => repo.GetSectionsAsync(1, 1, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(emptyResult);
                var result = await sectionCoordinationService.GetSectionsMaximum3Async(1, 1, "", "", "", "", "", "", "", new List<string>() { "badlevel" }, "", "", "", null);
                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Item1.Count());
                Assert.AreEqual(0, result.Item2);
            }
            [TestMethod]
            public async Task SectionCoordinationServiceTests_V8_GetSectionMaximum3Async_WithBadSiteFilter_ReturnsEmptySet()
            {
                var emptyResult = new Tuple<IEnumerable<Section>, int>(new List<Domain.Student.Entities.Section>(), 0);
                sectionRepoMock.Setup(repo => repo.GetSectionsAsync(1, 1, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(emptyResult);
                var result = await sectionCoordinationService.GetSectionsMaximum3Async(1, 1, "", "", "", "", "", "", "", null, "", "badsite", "", null);
                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Item1.Count());
                Assert.AreEqual(0, result.Item2);
            }
            [TestMethod]
            public async Task SectionCoordinationServiceTests_V8_GetSectionMaximum3Async_WithBadStatusFilter_ReturnsEmptySet()
            {
                var emptyResult = new Tuple<IEnumerable<Section>, int>(new List<Domain.Student.Entities.Section>(), 0);
                sectionRepoMock.Setup(repo => repo.GetSectionsAsync(1, 1, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(emptyResult);
                var result = await sectionCoordinationService.GetSectionsMaximum3Async(1, 1, "", "", "", "", "", "", "", null, "", "", "badstatus", null);
                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Item1.Count());
                Assert.AreEqual(0, result.Item2);
            }
            [TestMethod]
            public async Task SectionCoordinationServiceTests_V8_GetSectionMaximum3Async_WithBadOwningOrganizationFilter_ReturnsEmptySet()
            {
                var emptyResult = new Tuple<IEnumerable<Section>, int>(new List<Domain.Student.Entities.Section>(), 0);
                sectionRepoMock.Setup(repo => repo.GetSectionsAsync(1, 1, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                 It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                 It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(),
                 It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(emptyResult);
                sectionRepoMock.Setup(repo => repo.GetSectionsAsync(1, 1, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(emptyResult);

                var result = await sectionCoordinationService.GetSectionsMaximum3Async(1, 1, "", "", "", "", "", "", "", null, "", "", "", new List<string> { "badorg" });
                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Item1.Count());
                Assert.AreEqual(0, result.Item2);
            }
            [TestMethod]
            public async Task SectionCoordinationServiceTests_V8_GetSectionMaximum3Async_WithBadCourseFilter_ReturnsEmptySet()
            {
                var emptyResult = new Tuple<IEnumerable<Section>, int>(new List<Domain.Student.Entities.Section>(), 0);
                sectionRepoMock.Setup(repo => repo.GetSectionsAsync(1, 1, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(),
                    It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(emptyResult);
                sectionRepoMock.Setup(repo => repo.GetSectionsAsync(1, 1, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(emptyResult);

                var result = await sectionCoordinationService.GetSectionsMaximum3Async(1, 1, "", "", "", "", "", "", "", null, "badcourse", "", "", null);
                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Item1.Count());
                Assert.AreEqual(0, result.Item2);
            }

            [TestMethod]
            public async Task SectionCoordinationServiceTests_V8_GetSectionMaximum3Async_ReturnsCorrectCourseTitle()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionByGuidAsync(It.IsAny<string>())).ReturnsAsync(sectionEntities.ToList()[0]);

                var result = await sectionCoordinationService.GetSectionMaximumByGuid3Async("0b983700-29eb-46ff-8616-21a8c3a48a0c");
                var expectedCourseGuid = await courseRepoMock.Object.GetCourseGuidFromIdAsync("180");
                var expectedCourse = await courseRepoMock.Object.GetCourseByGuidAsync(expectedCourseGuid);

                Assert.IsNotNull(result);
                Assert.AreEqual(expectedCourse.LongTitle, result.Course.Title);

            }
            [TestMethod]
            public async Task SectionCoordinationServiceTests_V11_and_V16_GetSectionMaximum4Async_ReturnsCorrectCourseTitle()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionByGuidAsync(It.IsAny<string>())).ReturnsAsync(sectionEntities.ToList()[0]);

                sectionRepoMock.Setup(repo => repo.GetStatusCodesWithGuidsAsync()).ReturnsAsync(new List<SectionStatusCodeGuid>() { new SectionStatusCodeGuid("asda", "A", "asda") });

                var result = await sectionCoordinationService.GetSectionMaximumByGuid4Async("0b983700-29eb-46ff-8616-21a8c3a48a0c");
                var expectedCourseGuid = await courseRepoMock.Object.GetCourseGuidFromIdAsync("180");
                var expectedCourse = await courseRepoMock.Object.GetCourseByGuidAsync(expectedCourseGuid);

                Assert.IsNotNull(result);
                Assert.AreEqual(expectedCourse.LongTitle, result.Course.Title);

            }

            #region All Exceptions

            [TestMethod]
            //[ExpectedException(typeof(ArgumentException))]
            public async Task SectionCoordinationServiceTests_V8_GetSectionsMaximum3Async_NullSectionGuid_ArgumentException()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionsAsync(0, 3, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(sectionMaxEntitiesTuple);
                sectionRepoMock.Setup(repo => repo.GetSectionGuidFromIdAsync(It.IsAny<string>())).ThrowsAsync(new ArgumentException());

                var results = await sectionCoordinationService.GetSectionsMaximum3Async(0, 3, It.IsAny<string>(), "2016-01-01", "2016-10-01", It.IsAny<string>(), It.IsAny<string>(), "840e72f0-57b9-42a2-ae88-df3c2262fbbc", "8d3fcd4d-cec8-4405-90eb-948392bd0a7e",
                    new List<string> { "558ca14c-718a-4b6e-8d92-77f498034f9f" }, "38b4330c-befa-435e-81d5-c3ffd52759f2", "b0eba383-5acf-4050-949d-8bb7a17c5012", "open", new List<string> { "1a2e2906-d46b-4698-80f6-af87b8083c64" });
                Assert.AreEqual(0, results.Item1.Count());
                Assert.AreEqual(0, results.Item2);
            }

            [TestMethod]
            //[ExpectedException(typeof(ArgumentException))]
            public async Task SectionCoordinationServiceTests_V8_GetSectionsMaximum3Async_NullInstrMethodGuid_ArgumentException()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionsAsync(0, 3, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(sectionMaxEntitiesTuple);
                stuRefDataRepoMock.Setup(repo => repo.GetInstructionalMethodsAsync(It.IsAny<bool>())).ThrowsAsync(new ArgumentException());

                var results = await sectionCoordinationService.GetSectionsMaximum3Async(0, 3, It.IsAny<string>(), "2016-01-01", "2016-10-01", It.IsAny<string>(), It.IsAny<string>(), "840e72f0-57b9-42a2-ae88-df3c2262fbbc", "8d3fcd4d-cec8-4405-90eb-948392bd0a7e",
                    new List<string> { "558ca14c-718a-4b6e-8d92-77f498034f9f" }, "38b4330c-befa-435e-81d5-c3ffd52759f2", "b0eba383-5acf-4050-949d-8bb7a17c5012", "open", new List<string> { "1a2e2906-d46b-4698-80f6-af87b8083c64" });
                Assert.AreEqual(0, results.Item1.Count());
                Assert.AreEqual(0, results.Item2);
            }

            [TestMethod]
            //[ExpectedException(typeof(ArgumentException))]
            public async Task SectionCoordinationServiceTests_V8_GetSectionsMaximum3Async_RepeatCodeNull_ArgumentException()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionsAsync(0, 3, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(sectionMaxEntitiesTuple);

                meeting1.Frequency = "abc";

                var results = await sectionCoordinationService.GetSectionsMaximum3Async(0, 3, It.IsAny<string>(), "2016-01-01", "2016-10-01", It.IsAny<string>(), It.IsAny<string>(), "840e72f0-57b9-42a2-ae88-df3c2262fbbc", "8d3fcd4d-cec8-4405-90eb-948392bd0a7e",
                    new List<string> { "558ca14c-718a-4b6e-8d92-77f498034f9f" }, "38b4330c-befa-435e-81d5-c3ffd52759f2", "b0eba383-5acf-4050-949d-8bb7a17c5012", "open", new List<string> { "1a2e2906-d46b-4698-80f6-af87b8083c64" });
                Assert.AreEqual(0, results.Item1.Count());
                Assert.AreEqual(0, results.Item2);
            }

            [TestMethod]
            //[ExpectedException(typeof(ArgumentException))]
            public async Task SectionCoordinationServiceTests_V8_GetSectionsMaximum3Async_GetPersonGuidFromIdAsync_Error_ArgumentException()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionsAsync(0, 3, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(sectionMaxEntitiesTuple);

                personRepoMock.Setup(repo => repo.GetPersonGuidFromIdAsync(It.IsAny<string>())).ThrowsAsync(new ArgumentException());

                var results = await sectionCoordinationService.GetSectionsMaximum3Async(0, 3, It.IsAny<string>(), "2016-01-01", "2016-10-01", It.IsAny<string>(), It.IsAny<string>(), "840e72f0-57b9-42a2-ae88-df3c2262fbbc", "8d3fcd4d-cec8-4405-90eb-948392bd0a7e",
                    new List<string> { "558ca14c-718a-4b6e-8d92-77f498034f9f" }, "38b4330c-befa-435e-81d5-c3ffd52759f2", "b0eba383-5acf-4050-949d-8bb7a17c5012", "open", new List<string> { "1a2e2906-d46b-4698-80f6-af87b8083c64" });
                Assert.IsNotNull(results);
                Assert.AreEqual(0, results.Item2);
            }

            [TestMethod]
            //[ExpectedException(typeof(RepositoryException))]
            public async Task SectionCoordinationServiceTests_V8_GetSectionsMaximum3Async_NullCourse_RepositoryException()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionsAsync(0, 3, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(sectionMaxEntitiesTuple);
                courseRepoMock.Setup(repo => repo.GetCourseByGuidAsync(It.IsAny<string>())).ReturnsAsync(() => null);

                var results = await sectionCoordinationService.GetSectionsMaximum3Async(0, 3, It.IsAny<string>(), "2016-01-01", "2016-10-01", It.IsAny<string>(), It.IsAny<string>(), "840e72f0-57b9-42a2-ae88-df3c2262fbbc", "8d3fcd4d-cec8-4405-90eb-948392bd0a7e",
                    new List<string> { "558ca14c-718a-4b6e-8d92-77f498034f9f" }, "38b4330c-befa-435e-81d5-c3ffd52759f2", "b0eba383-5acf-4050-949d-8bb7a17c5012", "open", new List<string> { "1a2e2906-d46b-4698-80f6-af87b8083c64" });
                Assert.IsNotNull(results);
                Assert.AreEqual(0, results.Item2);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionCoordinationServiceTests_V8_GetSectionMaximumByGuid3Async_WithNullId_ArgumentNullException()
            {
                var result = await sectionCoordinationService.GetSectionMaximumByGuid3Async("");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task SectionCoordinationServiceTests_V8_GetSectionMaximumByGuid3Async_WithNullEntity_KeyNotFoundException()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(() => null);

                var result = await sectionCoordinationService.GetSectionMaximumByGuid3Async("0b983700-29eb-46ff-8616-21a8c3a48a0c");
            }



            #endregion

            private void BuildData()
            {
                //Course entity
                courseEntities = new TestCourseRepository().GetAsync().Result.Take(41);
                var courseEntity = courseEntities.FirstOrDefault(i => i.Id.Equals("180"));
                courseRepoMock.Setup(repo => repo.GetCourseGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(courseEntity.Guid);
                courseRepoMock.Setup(repo => repo.GetCourseByGuidAsync(courseEntity.Guid)).ReturnsAsync(courseEntity);
                //Subject entity
                subjectEntities = new TestSubjectRepository().Get();
                stuRefDataRepoMock.Setup(repo => repo.GetSubjectsAsync()).ReturnsAsync(subjectEntities);
                var subjectEntity = subjectEntities.FirstOrDefault(s => s.Code.Equals(courseEntity.SubjectCode));
                //Credit Categories
                creditCategoryEntities = new List<Domain.Student.Entities.CreditCategory>()
                {
                    new Domain.Student.Entities.CreditCategory("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "I", "Institutional", Domain.Student.Entities.CreditType.Institutional),
                    new Domain.Student.Entities.CreditCategory("73244057-D1EC-4094-A0B7-DE602533E3A6", "C", "Continuing Education", Domain.Student.Entities.CreditType.ContinuingEducation),
                    new Domain.Student.Entities.CreditCategory("73244057-D1EC-4094-A0B7-DE602533E3A6", "E", "Exchange", Domain.Student.Entities.CreditType.Exchange),
                    new Domain.Student.Entities.CreditCategory("73244057-D1EC-4094-A0B7-DE602533E3A6", "O", "Other", Domain.Student.Entities.CreditType.Other),
                    new Domain.Student.Entities.CreditCategory("73244057-D1EC-4094-A0B7-DE602533E3A6", "N", "Continuing Education", Domain.Student.Entities.CreditType.None),
                    new Domain.Student.Entities.CreditCategory("1df164eb-8178-4321-a9f7-24f12d3991d8", "T", "Transfer Credit", Domain.Student.Entities.CreditType.Transfer)
                };
                stuRefDataRepoMock.Setup(repo => repo.GetCreditCategoriesAsync()).ReturnsAsync(creditCategoryEntities);
                //Grade Schemes
                gradeSchemeEntities = new TestStudentReferenceDataRepository().GetGradeSchemesAsync().Result as List<Ellucian.Colleague.Domain.Student.Entities.GradeScheme>;
                stuRefDataRepoMock.Setup(repo => repo.GetGradeSchemesAsync()).ReturnsAsync(gradeSchemeEntities);
                //Course Levels
                courseLevelEntities = new List<Domain.Student.Entities.CourseLevel>()
                {
                    new Domain.Student.Entities.CourseLevel("19f6e2cd-1e5f-4485-9b27-60d4f4e4b1ff", "100", "First Yr"),
                    new Domain.Student.Entities.CourseLevel("73244057-D1EC-4094-A0B7-DE602533E3A6", "200", "Second Year"),
                    new Domain.Student.Entities.CourseLevel("1df164eb-8178-4321-a9f7-24f12d3991d8", "300", "Third Year"),
                    new Domain.Student.Entities.CourseLevel("4aa187ae-6d22-4b10-a2e2-1304ebc18176", "400", "Third Year"),
                    new Domain.Student.Entities.CourseLevel("d9f42a0f-39de-44bc-87af-517619141bde", "500", "Third Year")
                };
                stuRefDataRepoMock.Setup(repo => repo.GetCourseLevelsAsync()).ReturnsAsync(courseLevelEntities);

                //IEnumerable<SectionMeeting>
                var sectionFaculty = new Domain.Student.Entities.SectionFaculty("1234", "SEC1", "0000678", "LG", new DateTime(2012, 09, 01), new DateTime(2011, 12, 21), 100.0m);

                meeting1 = new Domain.Student.Entities.SectionMeeting("123", "SEC1", "LG", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "W");
                meeting1.Guid = "1f1485cb-2705-4b59-9f51-7aa85c79b377";
                meeting1.AddFacultyId("0000678");
                meeting1.AddSectionFaculty(sectionFaculty);
                meeting1.StartTime = new DateTimeOffset(2012, 01, 01, 08, 30, 00, new TimeSpan());
                meeting1.EndTime = new DateTimeOffset(2012, 12, 21, 09, 30, 00, new TimeSpan());
                meeting1.Room = "COE*Room 1";
                var sectionMeetings = new List<Domain.Student.Entities.SectionMeeting>();
                sectionMeetings.Add(meeting1);
                var tuple = new Tuple<IEnumerable<Domain.Student.Entities.SectionMeeting>, int>(sectionMeetings, 1);
                sectionRepoMock.Setup(repo => repo.GetSectionMeetingAsync(0, 0, "1", "", "", "", "", new List<string>(), new List<string>(), new List<string>(), "")).ReturnsAsync(tuple);
                sectionRepoMock.Setup(repo => repo.GetSectionGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("ad7655d0-77e3-4f8a-a07c-5c86f6a6f86a");

                //Instructor Id
                personRepoMock.Setup(repo => repo.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("ebf585ad-cc1f-478f-a7a3-aefae87f873a");
                //Instructional Methods
                instrMethodEntities = new List<Domain.Student.Entities.InstructionalMethod>()
                {
                    new Domain.Student.Entities.InstructionalMethod("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "LG", "LG", false),
                    new Domain.Student.Entities.InstructionalMethod("73244057-D1EC-4094-A0B7-DE602533E3A6", "30", "30", true),
                    new Domain.Student.Entities.InstructionalMethod("1df164eb-8178-4321-a9f7-24f12d3991d8", "04", "04", false)
                };
                stuRefDataRepoMock.Setup(repo => repo.GetInstructionalMethodsAsync(It.IsAny<bool>())).ReturnsAsync(instrMethodEntities);
                //schedule repeats
                var scheduleRepeats = new List<Domain.Base.Entities.ScheduleRepeat>()
                {
                    new Domain.Base.Entities.ScheduleRepeat("W", "Weekly", "7", FrequencyType.Weekly),
                    new Domain.Base.Entities.ScheduleRepeat("D", "Daily", "1", FrequencyType.Daily),
                    new Domain.Base.Entities.ScheduleRepeat("M", "Monthly", "30", FrequencyType.Monthly),
                    new Domain.Base.Entities.ScheduleRepeat("Y", "Yearly", "365", FrequencyType.Yearly)
                };
                referenceDataRepoMock.Setup(repo => repo.ScheduleRepeats).Returns(scheduleRepeats);
                //Rooms entities
                roomEntities = new List<Domain.Base.Entities.Room>
                {
                    new Room("2ae6e009-40ca-4ac0-bb41-c123f7c344e3", "COE*Room 1", "COE")
                    {
                        Capacity = 50,
                        RoomType = "110",
                        Name = "Room 1"
                    },
                    new Room("8c92e963-5f05-45a2-8484-d9ad21e6ab47", "COE*0110", "CEE")
                    {
                        Capacity = 100,
                        RoomType = "111",
                        Name = "Room 2"
                    },
                    new Room("8fdbaec7-4198-4348-b95a-a48a357e67f5", "COE*0120", "CDF")
                    {
                        Capacity = 20,
                        RoomType = "111",
                        Name = "Room 13"
                    },
                    new Room("327a6856-0230-4a6d-82ed-5c99dc1b1862", "COE*0121", "CSD")
                    {
                        Capacity = 50,
                        RoomType = "111",
                        Name = "Room 112"
                    },
                    new Room("cc9aa34c-db5e-46dc-9e5b-ba3f4b2557a8", "EIN*0121", "BSF")
                    {
                        Capacity = 30,
                        RoomType = "111",
                        Name = "Room BSF"
                    }
                };
                roomRepoMock.Setup(repo => repo.RoomsAsync()).ReturnsAsync(roomEntities);
                //Person
                person = new Domain.Base.Entities.Person("1", "Brown")
                {
                    Guid = "96cf912f-5e87-4099-96bf-73baac4c7715",
                    Prefix = "Mr.",
                    FirstName = "Ricky",
                    MiddleName = "Lee",
                    Suffix = "Jr.",
                    Nickname = "Rick",
                    BirthDate = new DateTime(1930, 1, 1),
                    DeceasedDate = new DateTime(2014, 5, 12),
                    GovernmentId = "111-11-1111",
                    MaritalStatusCode = "M",
                    EthnicCodes = new List<string> { "H" },
                    RaceCodes = new List<string> { "AS" }
                };
                person.AddEmailAddress(new EmailAddress("xyz@xmail.com", "COL"));
                person.AddPersonAlt(new PersonAlt("1", Domain.Base.Entities.PersonAlt.ElevatePersonAltType.ToString()));

                personRepoMock.Setup(repo => repo.GetPersonByGuidNonCachedAsync(It.IsAny<string>())).ReturnsAsync(person);
                // Mock the reference repository for prefix
                referenceDataRepoMock.Setup(repo => repo.Prefixes).Returns(new List<Prefix>()
                {
                    new Prefix("MR","Mr","Mr."),
                    new Prefix("MS","Ms","Ms."),
                    new Prefix("JR","Jr","Jr."),
                    new Prefix("SR","Sr","Sr.")
                });

                // Mock the reference repository for suffix
                referenceDataRepoMock.Setup(repo => repo.Suffixes).Returns(new List<Suffix>()
                {
                    new Suffix("JR","Jr","Jr."),
                    new Suffix("SR","Sr","Sr.")
                });

                //Acad Periods
                var registrationDates = new RegistrationDate(null, DateTime.Today.AddYears(1), DateTime.Today.AddYears(1), DateTime.Today.AddYears(1), DateTime.Today.AddYears(1),
                    DateTime.Today.AddYears(1), DateTime.Today.AddYears(1), DateTime.Today.AddYears(1), DateTime.Today.AddYears(1), DateTime.Today.AddYears(1), new List<DateTime?>() { DateTime.Today.AddYears(1) });
                acadPeriodsEntities = new List<Domain.Student.Entities.AcademicPeriod>()
                {
                    new Domain.Student.Entities.AcademicPeriod("8d3fcd4d-cec8-4405-90eb-948392bd0a7e", "2012/FA", "Fall 2014", DateTime.Today.AddDays(-60), DateTime.Today.AddDays(10),
                        DateTime.Today.Year, 4, "2012/FA", "5f7e7071-5aef-4d22-891f-86ab472a9f15", "edcfd1ee-4adf-46bc-8b87-8853ae49dbeb",new List<RegistrationDate>(){registrationDates} )
                };
                termRepoMock.Setup(repo => repo.GetAcademicPeriods(It.IsAny<IEnumerable<Term>>())).Returns(acadPeriodsEntities);

                academicLevelEntities = new TestAcademicLevelRepository().GetAsync().Result as List<Ellucian.Colleague.Domain.Student.Entities.AcademicLevel>;
                stuRefDataRepoMock.Setup(repo => repo.GetAcademicLevelsAsync()).ReturnsAsync(academicLevelEntities);

                locationEntities = new TestLocationRepository().Get();
                referenceDataRepoMock.Setup(repo => repo.Locations).Returns(locationEntities);

                departmentEntities = new TestDepartmentRepository().Get().ToList();
                referenceDataRepoMock.Setup(repo => repo.DepartmentsAsync()).ReturnsAsync(departmentEntities);

                instructionalPlatformEntities = new List<InstructionalPlatform>
                {
                    new InstructionalPlatform("840e72f0-57b9-42a2-ae88-df3c2262fbbc", "CE", "Continuing Education"),
                    new InstructionalPlatform("e986b8a5-25f3-4aa0-bd0e-90982865e749", "D", "Institutional"),
                    new InstructionalPlatform("b5cc288b-8692-474e-91be-bdc55778e2f5", "TR", "Transfer")
                };
                referenceDataRepoMock.Setup(repo => repo.GetInstructionalPlatformsAsync(It.IsAny<bool>())).ReturnsAsync(instructionalPlatformEntities);

                sectionRepoMock.Setup(repo => repo.GetUnidataFormattedDate(It.IsAny<string>())).ReturnsAsync("2016/01/01");
                sectionRepoMock.Setup(repo => repo.GetCourseIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
                sectionRepoMock.Setup(repo => repo.ConvertStatusToStatusCodeAsync(It.IsAny<string>())).ReturnsAsync("Open");

                sectionEntities = new List<Domain.Student.Entities.Section>()
                {
                    new Domain.Student.Entities.Section("1", "180", "111", new DateTime(2016, 01, 01), 3, 3, "Title 1", "I", new Collection<Domain.Student.Entities.OfferingDepartment>(){ new Domain.Student.Entities.OfferingDepartment("MATH") },
                        new Collection<string>(){ "100", "200"}, "UG", new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddYears(-6)) })
                        {
                            Guid = "0b983700-29eb-46ff-8616-21a8c3a48a0c",
                            LearningProvider = "CE",
                            TermId = "2012/FA",
                            Location = "MAIN",
                            NumberOfWeeks = 2,
                            NumberOnWaitlist = 10,
                            GradeSchemeCode = "UG",
                            BillingCred = 1
                        }
                };
                sectionMaxEntitiesTuple = new Tuple<IEnumerable<Section>, int>(sectionEntities, sectionEntities.Count());
            }
        }

        [TestClass]
        public class SectionsMaximum_V16 : CurrentUserSetup
        {
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<ISectionRepository> sectionRepoMock;
            private Mock<IStudentRepository> studentRepoMock;
            private Mock<IStudentReferenceDataRepository> stuRefDataRepoMock;
            private Mock<IReferenceDataRepository> referenceDataRepoMock;
            private Mock<ICourseRepository> courseRepoMock;
            private Mock<ITermRepository> termRepoMock;
            private Mock<IStudentConfigurationRepository> studentConfigRepoMock;
            private Mock<IConfigurationRepository> configRepoMock;
            private Mock<IPersonRepository> personRepoMock;
            private Mock<IRoomRepository> roomRepoMock;
            private Mock<IEventRepository> eventRepoMock;
            private Mock<IBookRepository> bookRepoMock;
            private Mock<IRoleRepository> roleRepoMock;
            private Mock<ILogger> loggerMock;
            private ICurrentUserFactory currentUserFactory;


            private SectionCoordinationService sectionCoordinationService;
            Tuple<IEnumerable<Domain.Student.Entities.Section>, int> sectionMaxEntitiesTuple;
            IEnumerable<Domain.Student.Entities.Section> sectionEntities;

            IEnumerable<string> courseEntityIds;
            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course> courseEntities;
            IEnumerable<CourseTitleType> courseTitleTypeEntities;
            IEnumerable<SectionStatusCodeGuid> sectionStatusCodeGuidEntities;
            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Subject> subjectEntities;
            IEnumerable<Domain.Student.Entities.CreditCategory> creditCategoryEntities;
            IEnumerable<Domain.Student.Entities.GradeScheme> gradeSchemeEntities;
            IEnumerable<Domain.Student.Entities.CourseLevel> courseLevelEntities;
            IEnumerable<Domain.Student.Entities.AcademicPeriod> acadPeriodsEntities;
            IEnumerable<Domain.Student.Entities.AcademicLevel> academicLevelEntities;
            IEnumerable<Domain.Base.Entities.InstructionalPlatform> instructionalPlatformEntities;
            IEnumerable<Domain.Base.Entities.Location> locationEntities;
            IEnumerable<Domain.Base.Entities.Department> departmentEntities;
            IEnumerable<Domain.Student.Entities.InstructionalMethod> instrMethodEntities;
            IEnumerable<Domain.Base.Entities.Room> roomEntities;
            IEnumerable<Domain.Base.Entities.Building> buildingEntities;
            IEnumerable<string> personEntityIds;
            Dictionary<string, string> personGuidCollection;

            Domain.Base.Entities.PersonIntegration person;
            private Domain.Student.Entities.SectionMeeting meeting1;


            [TestInitialize]
            public void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                sectionRepoMock = new Mock<ISectionRepository>();
                courseRepoMock = new Mock<ICourseRepository>();
                studentRepoMock = new Mock<IStudentRepository>();
                stuRefDataRepoMock = new Mock<IStudentReferenceDataRepository>();
                referenceDataRepoMock = new Mock<IReferenceDataRepository>();
                termRepoMock = new Mock<ITermRepository>();
                studentConfigRepoMock = new Mock<IStudentConfigurationRepository>();
                configRepoMock = new Mock<IConfigurationRepository>();
                personRepoMock = new Mock<IPersonRepository>();
                roomRepoMock = new Mock<IRoomRepository>();
                eventRepoMock = new Mock<IEventRepository>();
                bookRepoMock = new Mock<IBookRepository>();
                roleRepoMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();

                currentUserFactory = new CurrentUserSetup.StudentUserFactory();
                BuildData();

                sectionCoordinationService = new SectionCoordinationService(adapterRegistryMock.Object, sectionRepoMock.Object, courseRepoMock.Object, studentRepoMock.Object,
                    stuRefDataRepoMock.Object, referenceDataRepoMock.Object, termRepoMock.Object, studentConfigRepoMock.Object, configRepoMock.Object, personRepoMock.Object,
                    roomRepoMock.Object, eventRepoMock.Object, bookRepoMock.Object, currentUserFactory, roleRepoMock.Object, loggerMock.Object);

            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                sectionRepoMock = null;
                courseRepoMock = null;
                studentRepoMock = null;
                stuRefDataRepoMock = null;
                referenceDataRepoMock = null;
                termRepoMock = null;
                studentConfigRepoMock = null;
                configRepoMock = null;
                personRepoMock = null;
                roomRepoMock = null;
                eventRepoMock = null;
                roleRepoMock = null;
                sectionCoordinationService = null;
                courseEntities = null;
                subjectEntities = null;
                creditCategoryEntities = null;
                gradeSchemeEntities = null;
                courseLevelEntities = null;
                acadPeriodsEntities = null;
                academicLevelEntities = null;
                instructionalPlatformEntities = null;
                locationEntities = null;
                departmentEntities = null;
                instrMethodEntities = null;
                roomEntities = null;
                buildingEntities = null;
                person = null;
                meeting1 = null;
            }

            [TestMethod]
            public async Task SectionCoordinationServiceTests_V16_GetSectionsMaximum16Async_ValidateAllFilters()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionsAsync(0, 3, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(sectionMaxEntitiesTuple);

                var results = await sectionCoordinationService.GetSectionsMaximum5Async(0, 3, It.IsAny<string>(), "2016-01-01", "2016-10-01", It.IsAny<string>(), It.IsAny<string>(), "840e72f0-57b9-42a2-ae88-df3c2262fbbc", "8d3fcd4d-cec8-4405-90eb-948392bd0a7e",
                    "840e72f0-57b9-42a2-ae88-df3c2262fbbc", new List<string>() { "558ca14c-718a-4b6e-8d92-77f498034f9f" }, "38b4330c-befa-435e-81d5-c3ffd52759f2", "b0eba383-5acf-4050-949d-8bb7a17c5012", "open", new List<string>() { "1a2e2906-d46b-4698-80f6-af87b8083c64" },
                    new List<string>() { "1a2e2906-d46b-4698-80f6-af87b8083c64" }, It.IsAny<string>(), It.IsAny<bool>());

                Assert.IsNotNull(results);
                Assert.AreEqual(0, results.Item1.Count());
            }

            [TestMethod]
            public async Task SectionCoordinationServiceTests_V16_GetSectionMaximumByGuid5Async_CE()
            {
                string courseId = "180";
                var offeringDepartment = new Collection<Domain.Student.Entities.OfferingDepartment>() { new Domain.Student.Entities.OfferingDepartment("MATH") };
                var courseLevelCodes = new Collection<string>() { "100", "200" };
                sectionEntities = new List<Domain.Student.Entities.Section>()
                {
                    new Domain.Student.Entities.Section("1", courseId, "111", new DateTime(2016, 01, 01), 3, 3, "Title 1", "C", offeringDepartment,
                        courseLevelCodes, "UG", new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddYears(-6)) })
                        {
                            Guid = "0b983700-29eb-46ff-8616-21a8c3a48a0c",
                            LearningProvider = "CE",
                            TermId = "2012/FA",
                            Location = "MAIN",
                            NumberOfWeeks = 2,
                            NumberOnWaitlist = 10,
                            GradeSchemeCode = "UG"
                        }
                };
                string[] facultyIds = new string[] { };
                courseEntityIds = new List<string>() { courseId };
                var courseEntity = new Domain.Student.Entities.Course(courseId, "shortTitle", "longTitle", offeringDepartment, "MATH", "111", "UG",
                    courseLevelCodes, 0, 0, new Collection<CourseApproval>())
                {
                    Guid = "b0897320-2e9b-4f6f-6816-12ac834a80ac"
                };
                courseEntities = new List<Domain.Student.Entities.Course>() { courseEntity };
                sectionMaxEntitiesTuple = new Tuple<IEnumerable<Section>, int>(sectionEntities, sectionEntities.Count());
                sectionRepoMock.Setup(repo => repo.GetSectionByGuidAsync(It.IsAny<string>())).ReturnsAsync(sectionEntities.ToList()[0]);
                personRepoMock.Setup(repo => repo.GetPersonGuidsCollectionAsync(new List<string>())).ReturnsAsync(new Dictionary<string, string>());
                courseRepoMock.Setup(repo => repo.GetCoursesByIdAsync(courseEntityIds)).ReturnsAsync(courseEntities);

                var result = await sectionCoordinationService.GetSectionMaximumByGuid5Async("0b983700-29eb-46ff-8616-21a8c3a48a0c");

                Assert.IsNotNull(result);
            }

            [TestMethod]
            public async Task SectionCoordinationServiceTests_V16_GetSectionMaximumByGuid5Async_Transfer()
            {
                string courseId = "180";
                var offeringDepartment = new Collection<Domain.Student.Entities.OfferingDepartment>() { new Domain.Student.Entities.OfferingDepartment("MATH") };
                var courseLevelCodes = new Collection<string>() { "100", "200" };
                sectionEntities = new List<Domain.Student.Entities.Section>()
                {
                    new Domain.Student.Entities.Section("1", courseId, "111", new DateTime(2016, 01, 01), 3, 3, "Title 1", "T", offeringDepartment,
                        courseLevelCodes, "UG", new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddYears(-6)) })
                        {
                            Guid = "0b983700-29eb-46ff-8616-21a8c3a48a0c",
                            LearningProvider = "CE",
                            TermId = "2012/FA",
                            Location = "MAIN",
                            NumberOfWeeks = 2,
                            NumberOnWaitlist = 10,
                            GradeSchemeCode = "UG"
                        }
                };
                string[] facultyIds = new string[] { };
                courseEntityIds = new List<string>() { courseId };
                var courseEntity = new Domain.Student.Entities.Course(courseId, "shortTitle", "longTitle", offeringDepartment, "MATH", "111", "UG",
                    courseLevelCodes, 0, 0, new Collection<CourseApproval>())
                {
                    Guid = "b0897320-2e9b-4f6f-6816-12ac834a80ac"
                };
                courseEntities = new List<Domain.Student.Entities.Course>() { courseEntity };
                sectionMaxEntitiesTuple = new Tuple<IEnumerable<Section>, int>(sectionEntities, sectionEntities.Count());
                sectionRepoMock.Setup(repo => repo.GetSectionByGuidAsync(It.IsAny<string>())).ReturnsAsync(sectionEntities.ToList()[0]);
                personRepoMock.Setup(repo => repo.GetPersonGuidsCollectionAsync(new List<string>())).ReturnsAsync(new Dictionary<string, string>());
                courseRepoMock.Setup(repo => repo.GetCoursesByIdAsync(courseEntityIds)).ReturnsAsync(courseEntities);

                var result = await sectionCoordinationService.GetSectionMaximumByGuid5Async("0b983700-29eb-46ff-8616-21a8c3a48a0c");

                Assert.IsNotNull(result);
            }

            [TestMethod]
            public async Task SectionCoordinationServiceTests_V16_GetSectionMaximumByGuid5Async_Exchange()
            {
                string courseId = "180";
                var offeringDepartment = new Collection<Domain.Student.Entities.OfferingDepartment>() { new Domain.Student.Entities.OfferingDepartment("MATH") };
                var courseLevelCodes = new Collection<string>() { "100", "200" };
                sectionEntities = new List<Domain.Student.Entities.Section>()
                {
                    new Domain.Student.Entities.Section("1", courseId, "111", new DateTime(2016, 01, 01), 3, 3, "Title 1", "E", offeringDepartment,
                        courseLevelCodes, "UG", new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddYears(-6)) })
                        {
                            Guid = "0b983700-29eb-46ff-8616-21a8c3a48a0c",
                            LearningProvider = "CE",
                            TermId = "2012/FA",
                            Location = "MAIN",
                            NumberOfWeeks = 2,
                            NumberOnWaitlist = 10,
                            GradeSchemeCode = "UG"
                        }
                };
                string[] facultyIds = new string[] { };
                courseEntityIds = new List<string>() { courseId };
                var courseEntity = new Domain.Student.Entities.Course(courseId, "shortTitle", "longTitle", offeringDepartment, "MATH", "111", "UG",
                    courseLevelCodes, 0, 0, new Collection<CourseApproval>())
                {
                    Guid = "b0897320-2e9b-4f6f-6816-12ac834a80ac"
                };
                courseEntities = new List<Domain.Student.Entities.Course>() { courseEntity };
                sectionMaxEntitiesTuple = new Tuple<IEnumerable<Section>, int>(sectionEntities, sectionEntities.Count());
                sectionRepoMock.Setup(repo => repo.GetSectionByGuidAsync(It.IsAny<string>())).ReturnsAsync(sectionEntities.ToList()[0]);
                personRepoMock.Setup(repo => repo.GetPersonGuidsCollectionAsync(new List<string>())).ReturnsAsync(new Dictionary<string, string>());
                courseRepoMock.Setup(repo => repo.GetCoursesByIdAsync(courseEntityIds)).ReturnsAsync(courseEntities);

                var result = await sectionCoordinationService.GetSectionMaximumByGuid5Async("0b983700-29eb-46ff-8616-21a8c3a48a0c");

                Assert.IsNotNull(result);
            }

            [TestMethod]
            public async Task SectionCoordinationServiceTests_V16_GetSectionMaximumByGuid5Async_Other()
            {
                string courseId = "180";
                var offeringDepartment = new Collection<Domain.Student.Entities.OfferingDepartment>() { new Domain.Student.Entities.OfferingDepartment("MATH") };
                var courseLevelCodes = new Collection<string>() { "100", "200" };
                sectionEntities = new List<Domain.Student.Entities.Section>()
                {
                    new Domain.Student.Entities.Section("1", courseId, "111", new DateTime(2016, 01, 01), 3, 3, "Title 1", "O", offeringDepartment,
                        courseLevelCodes, "UG", new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddYears(-6)) })
                        {
                            Guid = "0b983700-29eb-46ff-8616-21a8c3a48a0c",
                            LearningProvider = "CE",
                            TermId = "2012/FA",
                            Location = "MAIN",
                            NumberOfWeeks = 2,
                            NumberOnWaitlist = 10,
                            GradeSchemeCode = "UG"
                        }
                };
                string[] facultyIds = new string[] { };
                courseEntityIds = new List<string>() { courseId };
                var courseEntity = new Domain.Student.Entities.Course(courseId, "shortTitle", "longTitle", offeringDepartment, "MATH", "111", "UG",
                    courseLevelCodes, 0, 0, new Collection<CourseApproval>())
                {
                    Guid = "b0897320-2e9b-4f6f-6816-12ac834a80ac"
                };
                courseEntities = new List<Domain.Student.Entities.Course>() { courseEntity };
                sectionMaxEntitiesTuple = new Tuple<IEnumerable<Section>, int>(sectionEntities, sectionEntities.Count());
                sectionRepoMock.Setup(repo => repo.GetSectionByGuidAsync(It.IsAny<string>())).ReturnsAsync(sectionEntities.ToList()[0]);
                personRepoMock.Setup(repo => repo.GetPersonGuidsCollectionAsync(new List<string>())).ReturnsAsync(new Dictionary<string, string>());
                courseRepoMock.Setup(repo => repo.GetCoursesByIdAsync(courseEntityIds)).ReturnsAsync(courseEntities);

                var result = await sectionCoordinationService.GetSectionMaximumByGuid5Async("0b983700-29eb-46ff-8616-21a8c3a48a0c");

                Assert.IsNotNull(result);
            }

            [TestMethod]
            public async Task SectionCoordinationServiceTests_V16_GetSectionMaximumByGuid5Async_NoCredit()
            {
                string courseId = "180";
                var offeringDepartment = new Collection<Domain.Student.Entities.OfferingDepartment>() { new Domain.Student.Entities.OfferingDepartment("MATH") };
                var courseLevelCodes = new Collection<string>() { "100", "200" };
                sectionEntities = new List<Domain.Student.Entities.Section>()
                {
                    new Domain.Student.Entities.Section("1", courseId, "111", new DateTime(2016, 01, 01), 3, 3, "Title 1", "N", offeringDepartment,
                        courseLevelCodes, "UG", new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddYears(-6)) })
                        {
                            Guid = "0b983700-29eb-46ff-8616-21a8c3a48a0c",
                            LearningProvider = "CE",
                            TermId = "2012/FA",
                            Location = "MAIN",
                            NumberOfWeeks = 2,
                            NumberOnWaitlist = 10,
                            GradeSchemeCode = "UG"
                        }
                };
                string[] facultyIds = new string[] { };
                courseEntityIds = new List<string>() { courseId };
                var courseEntity = new Domain.Student.Entities.Course(courseId, "shortTitle", "longTitle", offeringDepartment, "MATH", "111", "UG",
                    courseLevelCodes, 0, 0, new Collection<CourseApproval>())
                {
                    Guid = "b0897320-2e9b-4f6f-6816-12ac834a80ac"
                };
                courseEntities = new List<Domain.Student.Entities.Course>() { courseEntity };
                sectionMaxEntitiesTuple = new Tuple<IEnumerable<Section>, int>(sectionEntities, sectionEntities.Count());
                sectionRepoMock.Setup(repo => repo.GetSectionByGuidAsync(It.IsAny<string>())).ReturnsAsync(sectionEntities.ToList()[0]);
                personRepoMock.Setup(repo => repo.GetPersonGuidsCollectionAsync(new List<string>())).ReturnsAsync(new Dictionary<string, string>());
                courseRepoMock.Setup(repo => repo.GetCoursesByIdAsync(courseEntityIds)).ReturnsAsync(courseEntities);

                var result = await sectionCoordinationService.GetSectionMaximumByGuid5Async("0b983700-29eb-46ff-8616-21a8c3a48a0c");

                Assert.IsNotNull(result);
            }

            [TestMethod]
            public async Task SectionCoordinationServiceTests_V16_GetSectionMaximumByGuid5Async_WithId_CreditTypeCode_Blank()
            {
                string courseId = "180";
                var offeringDepartment = new Collection<Domain.Student.Entities.OfferingDepartment>() { new Domain.Student.Entities.OfferingDepartment("MATH") };
                var courseLevelCodes = new Collection<string>() { "100", "200" };
                sectionEntities = new List<Domain.Student.Entities.Section>()
                {
                    new Domain.Student.Entities.Section("1", courseId, "111", new DateTime(2016, 01, 01), 3, 3, "Title 1", " ", offeringDepartment,
                        courseLevelCodes, "UG", new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddYears(-6)) })
                        {
                            Guid = "0b983700-29eb-46ff-8616-21a8c3a48a0c",
                            LearningProvider = "CE",
                            TermId = "2012/FA",
                            Location = "MAIN",
                            NumberOfWeeks = 2,
                            NumberOnWaitlist = 10,
                            GradeSchemeCode = "UG"
                        }
                };
                string[] facultyIds = new string[] { };
                courseEntityIds = new List<string>() { courseId };
                var courseEntity = new Domain.Student.Entities.Course(courseId, "shortTitle", "longTitle", offeringDepartment, "MATH", "111", "UG",
                    courseLevelCodes, 0, 0, new Collection<CourseApproval>())
                {
                    Guid = "b0897320-2e9b-4f6f-6816-12ac834a80ac"
                };
                courseEntities = new List<Domain.Student.Entities.Course>() { courseEntity };
                sectionMaxEntitiesTuple = new Tuple<IEnumerable<Section>, int>(sectionEntities, sectionEntities.Count());
                sectionRepoMock.Setup(repo => repo.GetSectionByGuidAsync(It.IsAny<string>())).ReturnsAsync(sectionEntities.ToList()[0]);
                personRepoMock.Setup(repo => repo.GetPersonGuidsCollectionAsync(new List<string>())).ReturnsAsync(new Dictionary<string, string>());
                courseRepoMock.Setup(repo => repo.GetCoursesByIdAsync(courseEntityIds)).ReturnsAsync(courseEntities);

                var result = await sectionCoordinationService.GetSectionMaximumByGuid5Async("0b983700-29eb-46ff-8616-21a8c3a48a0c");

                Assert.IsNotNull(result);
            }

            [TestMethod]
            public async Task SectionCoordinationServiceTests_V16_GetSectionMaximumByGuid5Async_WithId_No_CEUS()
            {
                string courseId = "180";
                var offeringDepartment = new Collection<Domain.Student.Entities.OfferingDepartment>() { new Domain.Student.Entities.OfferingDepartment("MATH") };
                var courseLevelCodes = new Collection<string>() { "100", "200" };
                sectionEntities = new List<Domain.Student.Entities.Section>()
                {
                    new Domain.Student.Entities.Section("1", courseId, "111", new DateTime(2016, 01, 01), 3, null, "Title 1", "I", offeringDepartment,
                        courseLevelCodes, "UG", new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddYears(-6)) })
                        {
                            Guid = "0b983700-29eb-46ff-8616-21a8c3a48a0c",
                            LearningProvider = "CE",
                            TermId = "2012/FA",
                            Location = "MAIN",
                            NumberOfWeeks = 2,
                            NumberOnWaitlist = 10,
                            GradeSchemeCode = "UG"
                        }
                };
                string[] facultyIds = new string[] { };
                courseEntityIds = new List<string>() { courseId };
                var courseEntity = new Domain.Student.Entities.Course(courseId, "shortTitle", "longTitle", offeringDepartment, "MATH", "111", "UG",
                    courseLevelCodes, 0, 0, new Collection<CourseApproval>())
                {
                    Guid = "b0897320-2e9b-4f6f-6816-12ac834a80ac"
                };
                courseEntities = new List<Domain.Student.Entities.Course>() { courseEntity };
                sectionMaxEntitiesTuple = new Tuple<IEnumerable<Section>, int>(sectionEntities, sectionEntities.Count());
                sectionRepoMock.Setup(repo => repo.GetSectionByGuidAsync(It.IsAny<string>())).ReturnsAsync(sectionEntities.ToList()[0]);
                personRepoMock.Setup(repo => repo.GetPersonGuidsCollectionAsync(new List<string>())).ReturnsAsync(new Dictionary<string, string>());
                courseRepoMock.Setup(repo => repo.GetCoursesByIdAsync(courseEntityIds)).ReturnsAsync(courseEntities);

                var result = await sectionCoordinationService.GetSectionMaximumByGuid5Async("0b983700-29eb-46ff-8616-21a8c3a48a0c");

                Assert.IsNotNull(result);
            }

            [TestMethod]
            public async Task SectionCoordinationServiceTests_V16_GetSectionMaximumByGuid5Async_WithId_Freq_Daily()
            {
                string courseId = "180";
                var offeringDepartment = new Collection<Domain.Student.Entities.OfferingDepartment>() { new Domain.Student.Entities.OfferingDepartment("MATH") };
                var courseLevelCodes = new Collection<string>() { "100", "200" };
                var sectionEntity = new Domain.Student.Entities.Section("1", courseId, "111", new DateTime(2016, 01, 01), 3, null, "Title 1", "I", offeringDepartment,
                        courseLevelCodes, "UG", new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddYears(-6)) })
                {
                    Guid = "0b983700-29eb-46ff-8616-21a8c3a48a0c",
                    LearningProvider = "CE",
                    TermId = "2012/FA",
                    Location = "MAIN",
                    NumberOfWeeks = 2,
                    NumberOnWaitlist = 10,
                    GradeSchemeCode = "UG"
                };
                var courseEntity = new Domain.Student.Entities.Course(courseId, "shortTitle", "longTitle", offeringDepartment, "MATH", "111", "UG",
                    courseLevelCodes, 0, 0, new Collection<CourseApproval>())
                {
                    Guid = "b0897320-2e9b-4f6f-6816-12ac834a80ac"
                };
                courseEntities = new List<Domain.Student.Entities.Course>() { courseEntity };
                string facultyId = "0000678";
                string facultyGuid = "fcfdf6ad-90ac-46e9-9051-a378cc85cf3b";
                var sectionFaculty = new Domain.Student.Entities.SectionFaculty("1234", courseId, facultyId, "LG", new DateTime(2012, 09, 01), new DateTime(2011, 12, 21), 100.0m);
                courseEntityIds = new List<string>() { courseId };
                personEntityIds = new List<string>() { facultyId };
                personGuidCollection = new Dictionary<string, string>() { };
                personGuidCollection.Add(facultyId, facultyGuid);
                var facultyPersonEntity = new Domain.Base.Entities.PersonIntegration(facultyId, "Doe")
                {
                    Guid = facultyGuid,
                    FirstName = "John"
                };
                var facultyPersonEntities = new List<Domain.Base.Entities.PersonIntegration>() { facultyPersonEntity };
                meeting1 = new Domain.Student.Entities.SectionMeeting("123", courseId, "LG", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "D")
                {
                    Guid = "1f1485cb-2705-4b59-9f51-7aa85c79b377",
                    StartTime = new DateTimeOffset(2012, 01, 01, 08, 30, 00, new TimeSpan()),
                    EndTime = new DateTimeOffset(2012, 12, 21, 09, 30, 00, new TimeSpan()),
                    Room = "COE*Room 1"
                };
                meeting1.AddFacultyId(facultyId);
                meeting1.AddSectionFaculty(sectionFaculty);
                sectionEntity.AddSectionMeeting(meeting1);
                sectionEntity.AddSectionFaculty(sectionFaculty);

                sectionEntities = new List<Section>() { sectionEntity };
                sectionMaxEntitiesTuple = new Tuple<IEnumerable<Section>, int>(sectionEntities, sectionEntities.Count());
                sectionRepoMock.Setup(repo => repo.GetSectionByGuidAsync(It.IsAny<string>())).ReturnsAsync(sectionEntities.ToList()[0]);

                personRepoMock.Setup(repo => repo.GetPersonGuidsCollectionAsync(It.IsAny<string[]>())).ReturnsAsync(personGuidCollection);
                courseRepoMock.Setup(repo => repo.GetCoursesByIdAsync(It.IsAny<List<string>>())).ReturnsAsync(courseEntities);
                personRepoMock.Setup(repo => repo.GetPersonNamesAndCredsByGuidAsync(It.IsAny<string[]>())).ReturnsAsync(facultyPersonEntities);

                var result = await sectionCoordinationService.GetSectionMaximumByGuid5Async("0b983700-29eb-46ff-8616-21a8c3a48a0c");

                Assert.IsNotNull(result);
                Assert.AreEqual(Dtos.FrequencyType2.Daily, result.InstructionalEvents.ToList()[0].Recurrence.RepeatRule.Type);
            }

            [TestMethod]
            public async Task SectionCoordinationServiceTests_V16_GetSectionMaximumByGuid5Async_WithId_Freq_Monthly_WithDays()
            {
                string courseId = "180";
                var offeringDepartment = new Collection<Domain.Student.Entities.OfferingDepartment>() { new Domain.Student.Entities.OfferingDepartment("MATH") };
                var courseLevelCodes = new Collection<string>() { "100", "200" };
                var sectionEntity = new Domain.Student.Entities.Section("1", courseId, "111", new DateTime(2016, 01, 01), 3, null, "Title 1", "I", offeringDepartment,
                        courseLevelCodes, "UG", new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddYears(-6)) })
                {
                    Guid = "0b983700-29eb-46ff-8616-21a8c3a48a0c",
                    LearningProvider = "CE",
                    TermId = "2012/FA",
                    Location = "MAIN",
                    NumberOfWeeks = 2,
                    NumberOnWaitlist = 10,
                    GradeSchemeCode = "UG"
                };
                var courseEntity = new Domain.Student.Entities.Course(courseId, "shortTitle", "longTitle", offeringDepartment, "MATH", "111", "UG",
                    courseLevelCodes, 0, 0, new Collection<CourseApproval>())
                {
                    Guid = "b0897320-2e9b-4f6f-6816-12ac834a80ac"
                };
                courseEntities = new List<Domain.Student.Entities.Course>() { courseEntity };
                string facultyId = "0000678";
                string facultyGuid = "fcfdf6ad-90ac-46e9-9051-a378cc85cf3b";
                var sectionFaculty = new Domain.Student.Entities.SectionFaculty("1234", courseId, facultyId, "LG", new DateTime(2012, 09, 01), new DateTime(2011, 12, 21), 100.0m);
                courseEntityIds = new List<string>() { courseId };
                personEntityIds = new List<string>() { facultyId };
                personGuidCollection = new Dictionary<string, string>() { };
                personGuidCollection.Add(facultyId, facultyGuid);
                var facultyPersonEntity = new Domain.Base.Entities.PersonIntegration(facultyId, "Doe")
                {
                    Guid = facultyGuid,
                    FirstName = "John"
                };
                var facultyPersonEntities = new List<Domain.Base.Entities.PersonIntegration>() { facultyPersonEntity };

                meeting1 = new Domain.Student.Entities.SectionMeeting("123", courseId, "LG", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "M")
                {
                    Guid = "1f1485cb-2705-4b59-9f51-7aa85c79b377",
                    Days = new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday },
                    StartTime = new DateTimeOffset(2012, 01, 01, 08, 30, 00, new TimeSpan()),
                    EndTime = new DateTimeOffset(2012, 12, 21, 09, 30, 00, new TimeSpan()),
                    Room = "COE*Room 1"
                };
                meeting1.AddFacultyId(facultyId);
                meeting1.AddSectionFaculty(sectionFaculty);
                sectionEntity.AddSectionMeeting(meeting1);
                sectionEntity.AddSectionFaculty(sectionFaculty);
                sectionEntities = new List<Section>() { sectionEntity };
                sectionMaxEntitiesTuple = new Tuple<IEnumerable<Section>, int>(sectionEntities, sectionEntities.Count());
                sectionRepoMock.Setup(repo => repo.GetSectionByGuidAsync(It.IsAny<string>())).ReturnsAsync(sectionEntities.ToList()[0]);
                personRepoMock.Setup(repo => repo.GetPersonGuidsCollectionAsync(It.IsAny<string[]>())).ReturnsAsync(personGuidCollection);
                courseRepoMock.Setup(repo => repo.GetCoursesByIdAsync(It.IsAny<List<string>>())).ReturnsAsync(courseEntities);
                personRepoMock.Setup(repo => repo.GetPersonNamesAndCredsByGuidAsync(It.IsAny<string[]>())).ReturnsAsync(facultyPersonEntities);

                var result = await sectionCoordinationService.GetSectionMaximumByGuid5Async("0b983700-29eb-46ff-8616-21a8c3a48a0c");

                Assert.IsNotNull(result);
                Assert.AreEqual(Dtos.FrequencyType2.Monthly, result.InstructionalEvents.ToList()[0].Recurrence.RepeatRule.Type);
            }

            [TestMethod]
            public async Task SectionCoordinationServiceTests_V16_GetSectionMaximumByGuid5Async_WithId_Freq_Monthly_WithoutDays()
            {
                string courseId = "180";
                var offeringDepartment = new Collection<Domain.Student.Entities.OfferingDepartment>() { new Domain.Student.Entities.OfferingDepartment("MATH") };
                var courseLevelCodes = new Collection<string>() { "100", "200" };
                var sectionEntity = new Domain.Student.Entities.Section("1", courseId, "111", new DateTime(2016, 01, 01), 3, null, "Title 1", "I", offeringDepartment,
                        courseLevelCodes, "UG", new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddYears(-6)) })
                {
                    Guid = "0b983700-29eb-46ff-8616-21a8c3a48a0c",
                    LearningProvider = "CE",
                    TermId = "2012/FA",
                    Location = "MAIN",
                    NumberOfWeeks = 2,
                    NumberOnWaitlist = 10,
                    GradeSchemeCode = "UG"
                };
                var courseEntity = new Domain.Student.Entities.Course(courseId, "shortTitle", "longTitle", offeringDepartment, "MATH", "111", "UG",
                    courseLevelCodes, 0, 0, new Collection<CourseApproval>())
                {
                    Guid = "b0897320-2e9b-4f6f-6816-12ac834a80ac"
                };
                courseEntities = new List<Domain.Student.Entities.Course>() { courseEntity };
                string facultyId = "0000678";
                string facultyGuid = "fcfdf6ad-90ac-46e9-9051-a378cc85cf3b";
                var sectionFaculty = new Domain.Student.Entities.SectionFaculty("1234", courseId, facultyId, "LG", new DateTime(2012, 09, 01), new DateTime(2011, 12, 21), 100.0m);
                courseEntityIds = new List<string>() { courseId };
                personEntityIds = new List<string>() { facultyId };
                personGuidCollection = new Dictionary<string, string>() { };
                personGuidCollection.Add(facultyId, facultyGuid);
                var facultyPersonEntity = new Domain.Base.Entities.PersonIntegration(facultyId, "Doe")
                {
                    Guid = facultyGuid,
                    FirstName = "John"
                };
                var facultyPersonEntities = new List<Domain.Base.Entities.PersonIntegration>() { facultyPersonEntity };


                meeting1 = new Domain.Student.Entities.SectionMeeting("123", courseId, "LG", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "M")
                {
                    Guid = "1f1485cb-2705-4b59-9f51-7aa85c79b377",
                    StartTime = new DateTimeOffset(2012, 01, 01, 08, 30, 00, new TimeSpan()),
                    EndTime = new DateTimeOffset(2012, 12, 21, 09, 30, 00, new TimeSpan()),
                    Room = "COE*Room 1"
                };
                meeting1.AddFacultyId(facultyId);
                meeting1.AddSectionFaculty(sectionFaculty);
                sectionEntity.AddSectionMeeting(meeting1);
                sectionEntity.AddSectionFaculty(sectionFaculty);
                sectionEntities = new List<Section>() { sectionEntity };
                sectionMaxEntitiesTuple = new Tuple<IEnumerable<Section>, int>(sectionEntities, sectionEntities.Count());
                sectionRepoMock.Setup(repo => repo.GetSectionByGuidAsync(It.IsAny<string>())).ReturnsAsync(sectionEntities.ToList()[0]);

                personRepoMock.Setup(repo => repo.GetPersonGuidsCollectionAsync(It.IsAny<string[]>())).ReturnsAsync(personGuidCollection);
                courseRepoMock.Setup(repo => repo.GetCoursesByIdAsync(It.IsAny<List<string>>())).ReturnsAsync(courseEntities);
                personRepoMock.Setup(repo => repo.GetPersonNamesAndCredsByGuidAsync(It.IsAny<string[]>())).ReturnsAsync(facultyPersonEntities);

                var result = await sectionCoordinationService.GetSectionMaximumByGuid5Async("0b983700-29eb-46ff-8616-21a8c3a48a0c");

                Assert.IsNotNull(result);
                Assert.AreEqual(Dtos.FrequencyType2.Monthly, result.InstructionalEvents.ToList()[0].Recurrence.RepeatRule.Type);
            }

            [TestMethod]
            public async Task SectionCoordinationServiceTests_V16_GetSectionMaximumByGuid5Async_WithId_Freq_Yearly()
            {
                string courseId = "180";
                var offeringDepartment = new Collection<Domain.Student.Entities.OfferingDepartment>() { new Domain.Student.Entities.OfferingDepartment("MATH") };
                var courseLevelCodes = new Collection<string>() { "100", "200" };
                var sectionEntity = new Domain.Student.Entities.Section("1", courseId, "111", new DateTime(2016, 01, 01), 3, null, "Title 1", "I", offeringDepartment,
                        courseLevelCodes, "UG", new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddYears(-6)) })
                {
                    Guid = "0b983700-29eb-46ff-8616-21a8c3a48a0c",
                    LearningProvider = "CE",
                    TermId = "2012/FA",
                    Location = "MAIN",
                    NumberOfWeeks = 2,
                    NumberOnWaitlist = 10,
                    GradeSchemeCode = "UG"
                };
                var courseEntity = new Domain.Student.Entities.Course(courseId, "shortTitle", "longTitle", offeringDepartment, "MATH", "111", "UG",
                    courseLevelCodes, 0, 0, new Collection<CourseApproval>())
                {
                    Guid = "b0897320-2e9b-4f6f-6816-12ac834a80ac"
                };
                courseEntities = new List<Domain.Student.Entities.Course>() { courseEntity };
                string facultyId = "0000678";
                string facultyGuid = "fcfdf6ad-90ac-46e9-9051-a378cc85cf3b";
                var sectionFaculty = new Domain.Student.Entities.SectionFaculty("1234", courseId, facultyId, "LG", new DateTime(2012, 09, 01), new DateTime(2011, 12, 21), 100.0m);
                courseEntityIds = new List<string>() { courseId };
                personEntityIds = new List<string>() { facultyId };
                personGuidCollection = new Dictionary<string, string>() { };
                personGuidCollection.Add(facultyId, facultyGuid);
                var facultyPersonEntity = new Domain.Base.Entities.PersonIntegration(facultyId, "Doe")
                {
                    Guid = facultyGuid,
                    FirstName = "John"
                };
                var facultyPersonEntities = new List<Domain.Base.Entities.PersonIntegration>() { facultyPersonEntity };
                meeting1 = new Domain.Student.Entities.SectionMeeting("123", courseId, "LG", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "Y")
                {
                    Guid = "1f1485cb-2705-4b59-9f51-7aa85c79b377",
                    StartTime = new DateTimeOffset(2012, 01, 01, 08, 30, 00, new TimeSpan()),
                    EndTime = new DateTimeOffset(2012, 12, 21, 09, 30, 00, new TimeSpan()),
                    Room = "COE*Room 1"
                };
                meeting1.AddFacultyId(facultyId);
                meeting1.AddSectionFaculty(sectionFaculty);
                sectionEntity.AddSectionMeeting(meeting1);
                sectionEntity.AddSectionFaculty(sectionFaculty);
                sectionEntities = new List<Section>() { sectionEntity };
                sectionMaxEntitiesTuple = new Tuple<IEnumerable<Section>, int>(sectionEntities, sectionEntities.Count());
                sectionRepoMock.Setup(repo => repo.GetSectionByGuidAsync(It.IsAny<string>())).ReturnsAsync(sectionEntities.ToList()[0]);

                personRepoMock.Setup(repo => repo.GetPersonGuidsCollectionAsync(It.IsAny<string[]>())).ReturnsAsync(personGuidCollection);
                courseRepoMock.Setup(repo => repo.GetCoursesByIdAsync(It.IsAny<List<string>>())).ReturnsAsync(courseEntities);
                personRepoMock.Setup(repo => repo.GetPersonNamesAndCredsByGuidAsync(It.IsAny<string[]>())).ReturnsAsync(facultyPersonEntities);

                var result = await sectionCoordinationService.GetSectionMaximumByGuid5Async("0b983700-29eb-46ff-8616-21a8c3a48a0c");

                Assert.IsNotNull(result);
                Assert.AreEqual(Dtos.FrequencyType2.Yearly, result.InstructionalEvents.ToList()[0].Recurrence.RepeatRule.Type);
            }

            [TestMethod]
            public async Task SectionCoordinationServiceTests_V16_GetSectionMaximumByGuid5Async_WithId()
            {
                string courseId = "180";
                var offeringDepartment = new Collection<Domain.Student.Entities.OfferingDepartment>() { new Domain.Student.Entities.OfferingDepartment("MATH") };
                var courseLevelCodes = new Collection<string>() { "100", "200" };
                var sectionEntity = new Domain.Student.Entities.Section("1", courseId, "111", new DateTime(2016, 01, 01), 3, null, "Title 1", "I", offeringDepartment,
                        courseLevelCodes, "UG", new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddYears(-6)) })
                {
                    Guid = "0b983700-29eb-46ff-8616-21a8c3a48a0c",
                    LearningProvider = "CE",
                    TermId = "2012/FA",
                    Location = "MAIN",
                    NumberOfWeeks = 2,
                    NumberOnWaitlist = 10,
                    GradeSchemeCode = "UG"
                };
                var courseEntity = new Domain.Student.Entities.Course(courseId, "shortTitle", "longTitle", offeringDepartment, "MATH", "111", "UG",
                    courseLevelCodes, 0, 0, new Collection<CourseApproval>())
                {
                    Guid = "b0897320-2e9b-4f6f-6816-12ac834a80ac"
                };
                courseEntities = new List<Domain.Student.Entities.Course>() { courseEntity };
                courseEntityIds = new List<string>() { courseId };
                personEntityIds = new List<string>() { };
                personGuidCollection = new Dictionary<string, string>() { };
                var facultyPersonEntities = new List<Domain.Base.Entities.PersonIntegration>() { };
                meeting1 = new Domain.Student.Entities.SectionMeeting("123", courseId, "LG", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "Y")
                {
                    Guid = "1f1485cb-2705-4b59-9f51-7aa85c79b377",
                    StartTime = new DateTimeOffset(2012, 01, 01, 08, 30, 00, new TimeSpan()),
                    EndTime = new DateTimeOffset(2012, 12, 21, 09, 30, 00, new TimeSpan()),
                    Room = "COE*Room 1"
                };
                sectionEntity.AddSectionMeeting(meeting1);
                sectionEntities = new List<Section>() { sectionEntity };
                sectionMaxEntitiesTuple = new Tuple<IEnumerable<Section>, int>(sectionEntities, sectionEntities.Count());
                sectionRepoMock.Setup(repo => repo.GetSectionByGuidAsync(It.IsAny<string>())).ReturnsAsync(sectionEntities.ToList()[0]);

                personRepoMock.Setup(repo => repo.GetPersonGuidsCollectionAsync(It.IsAny<string[]>())).ReturnsAsync(personGuidCollection);
                courseRepoMock.Setup(repo => repo.GetCoursesByIdAsync(It.IsAny<List<string>>())).ReturnsAsync(courseEntities);
                personRepoMock.Setup(repo => repo.GetPersonIntegration2ByGuidNonCachedAsync(It.IsAny<string[]>())).ReturnsAsync(facultyPersonEntities);

                var result = await sectionCoordinationService.GetSectionMaximumByGuid5Async("0b983700-29eb-46ff-8616-21a8c3a48a0c");

                Assert.IsNotNull(result);
            }

            [TestMethod]
            public async Task SectionCoordinationServiceTests_V16_GetSectionMaximum5Async_WithBadPeriodFilter_ReturnsEmptySet()
            {
                var emptyResult = new Tuple<IEnumerable<Section>, int>(new List<Domain.Student.Entities.Section>(), 0);
                sectionRepoMock.Setup(repo => repo.GetSectionsAsync(1, 1, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(),
                    It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(emptyResult);

                var result = await sectionCoordinationService.GetSectionsMaximum5Async(1, 1, "", "", "", "", "", "", "badperiod", null, null, "", "", "", null, null);
                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Item1.Count());
                Assert.AreEqual(0, result.Item2);
            }
            [TestMethod]
            public async Task SectionCoordinationServiceTests_V16_GetSectionMaximum5Async_WithBadSubjectFilter_ReturnsEmptySet()
            {
                var emptyResult = new Tuple<IEnumerable<Section>, int>(new List<Domain.Student.Entities.Section>(), 0);
                sectionRepoMock.Setup(repo => repo.GetSectionsAsync(1, 1, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                  It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                  It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(),
                  It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(emptyResult);
                sectionRepoMock.Setup(repo => repo.GetSectionsAsync(1, 1, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                 It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                 It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                 It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(emptyResult);
                var result = await sectionCoordinationService.GetSectionsMaximum5Async(1, 1, "", "", "", "badsubject", "", "", "", null, null, "", "", "", null, null);
                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Item1.Count());
                Assert.AreEqual(0, result.Item2);
            }

            [TestMethod]
            public async Task SectionCoordinationServiceTests_V16_GetSectionMaximum5Async_WithBadPlatformFilter_ReturnsEmptySet()
            {
                var emptyResult = new Tuple<IEnumerable<Section>, int>(new List<Domain.Student.Entities.Section>(), 0);
                sectionRepoMock.Setup(repo => repo.GetSectionsAsync(1, 1, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(emptyResult);
                var result = await sectionCoordinationService.GetSectionsMaximum5Async(1, 1, "", "", "", "", "", "badplatform", "", null, null, "", "", "", null, null);
                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Item1.Count());
                Assert.AreEqual(0, result.Item2);
            }
            [TestMethod]
            public async Task SectionCoordinationServiceTests_V16_GetSectionMaximum5Async_WithBadLevelFilter_ReturnsEmptySet()
            {
                var emptyResult = new Tuple<IEnumerable<Section>, int>(new List<Domain.Student.Entities.Section>(), 0);
                sectionRepoMock.Setup(repo => repo.GetSectionsAsync(1, 1, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(emptyResult);
                var result = await sectionCoordinationService.GetSectionsMaximum5Async(1, 1, "", "", "", "", "", "", "", "", new List<string>() { "badlevel" }, null, "", "", null, null);
                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Item1.Count());
                Assert.AreEqual(0, result.Item2);
            }
            [TestMethod]
            public async Task SectionCoordinationServiceTests_V16_GetSectionMaximum5Async_WithBadSiteFilter_ReturnsEmptySet()
            {
                var emptyResult = new Tuple<IEnumerable<Section>, int>(new List<Domain.Student.Entities.Section>(), 0);
                sectionRepoMock.Setup(repo => repo.GetSectionsAsync(1, 1, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(emptyResult);
                var result = await sectionCoordinationService.GetSectionsMaximum5Async(1, 1, "", "", "", "", "", "", "", null, null, "", "badsite", "", null, null);
                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Item1.Count());
                Assert.AreEqual(0, result.Item2);
            }
            [TestMethod]
            public async Task SectionCoordinationServiceTests_V16_GetSectionMaximum5Async_WithBadStatusFilter_ReturnsEmptySet()
            {
                var emptyResult = new Tuple<IEnumerable<Section>, int>(new List<Domain.Student.Entities.Section>(), 0);
                sectionRepoMock.Setup(repo => repo.GetSectionsAsync(1, 1, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(emptyResult);
                var result = await sectionCoordinationService.GetSectionsMaximum5Async(1, 1, "", "", "", "", "", "", "", null, null, "", "", "badstatus", null, null);
                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Item1.Count());
                Assert.AreEqual(0, result.Item2);
            }
            [TestMethod]
            public async Task SectionCoordinationServiceTests_V16_GetSectionMaximum5Async_WithBadOwningOrganizationFilter_ReturnsEmptySet()
            {
                var emptyResult = new Tuple<IEnumerable<Section>, int>(new List<Domain.Student.Entities.Section>(), 0);
                sectionRepoMock.Setup(repo => repo.GetSectionsAsync(1, 1, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                 It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                 It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(),
                 It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(emptyResult);
                sectionRepoMock.Setup(repo => repo.GetSectionsAsync(1, 1, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(emptyResult);

                var result = await sectionCoordinationService.GetSectionsMaximum5Async(1, 1, "", "", "", "", "", "", "", null, null, "", "", "", new List<string> { "badorg" }, null);
                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Item1.Count());
                Assert.AreEqual(0, result.Item2);
            }
            [TestMethod]
            public async Task SectionCoordinationServiceTests_V16_GetSectionMaximum5Async_WithBadCourseFilter_ReturnsEmptySet()
            {
                var emptyResult = new Tuple<IEnumerable<Section>, int>(new List<Domain.Student.Entities.Section>(), 0);
                sectionRepoMock.Setup(repo => repo.GetSectionsAsync(1, 1, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(),
                    It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(emptyResult);
                sectionRepoMock.Setup(repo => repo.GetSectionsAsync(1, 1, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(emptyResult);

                var result = await sectionCoordinationService.GetSectionsMaximum5Async(1, 1, "", "", "", "", "", "", "", null, null, "badcourse", "", "badstatus", null, null);
                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Item1.Count());
                Assert.AreEqual(0, result.Item2);
            }

            //[TestMethod]
            //public async Task SectionCoordinationServiceTests_V16_GetSectionMaximum5Async_ReturnsCorrectCourseTitle()
            //{
            //    sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(It.IsAny<string>())).ReturnsAsync(sectionEntities.ToList()[0]);

            //    var result = await sectionCoordinationService.GetSectionMaximumByGuid5Async("0b983700-29eb-46ff-8616-21a8c3a48a0c");
            //    var expectedCourseGuid = await courseRepoMock.Object.GetCourseGuidFromIdAsync("180");
            //    var expectedCourse = await courseRepoMock.Object.GetCourseByGuidAsync(expectedCourseGuid);

            //    Assert.IsNotNull(result);
            //    Assert.AreEqual(expectedCourse.LongTitle, result.Course.Titles.Title);

            //}

            #region All Exceptions

            [TestMethod]
            //[ExpectedException(typeof(ArgumentException))]
            public async Task SectionCoordinationServiceTests_V16_GetSectionsMaximum5Async_NullSectionGuid_ArgumentException()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionsAsync(0, 3, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(sectionMaxEntitiesTuple);
                sectionRepoMock.Setup(repo => repo.GetSectionGuidFromIdAsync(It.IsAny<string>())).ThrowsAsync(new ArgumentException());

                var results = await sectionCoordinationService.GetSectionsMaximum5Async(0, 3, It.IsAny<string>(), "2016-01-01", "2016-10-01", It.IsAny<string>(), It.IsAny<string>(), "840e72f0-57b9-42a2-ae88-df3c2262fbbc", "8d3fcd4d-cec8-4405-90eb-948392bd0a7e",
                    "840e72f0-57b9-42a2-ae88-df3c2262fbbc", new List<string>() { "558ca14c-718a-4b6e-8d92-77f498034f9f" }, "38b4330c-befa-435e-81d5-c3ffd52759f2", "b0eba383-5acf-4050-949d-8bb7a17c5012", "open", new List<string>() { "1a2e2906-d46b-4698-80f6-af87b8083c64" },
                    new List<string>() { "1a2e2906-d46b-4698-80f6-af87b8083c64" }, It.IsAny<string>(), It.IsAny<bool>());

                Assert.AreEqual(0, results.Item1.Count());
                Assert.AreEqual(0, results.Item2);
            }

            [TestMethod]
            //[ExpectedException(typeof(ArgumentException))]
            public async Task SectionCoordinationServiceTests_V16_GetSectionsMaximum5Async_NullInstrMethodGuid_ArgumentException()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionsAsync(0, 3, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(sectionMaxEntitiesTuple);
                stuRefDataRepoMock.Setup(repo => repo.GetInstructionalMethodsAsync(It.IsAny<bool>())).ThrowsAsync(new ArgumentException());

                var results = await sectionCoordinationService.GetSectionsMaximum5Async(0, 3, It.IsAny<string>(), "2016-01-01", "2016-10-01", It.IsAny<string>(), It.IsAny<string>(), "840e72f0-57b9-42a2-ae88-df3c2262fbbc", "8d3fcd4d-cec8-4405-90eb-948392bd0a7e",
                    "840e72f0-57b9-42a2-ae88-df3c2262fbbc", new List<string>() { "558ca14c-718a-4b6e-8d92-77f498034f9f" }, "38b4330c-befa-435e-81d5-c3ffd52759f2", "b0eba383-5acf-4050-949d-8bb7a17c5012", "open", new List<string>() { "1a2e2906-d46b-4698-80f6-af87b8083c64" },
                    new List<string>() { "1a2e2906-d46b-4698-80f6-af87b8083c64" }, It.IsAny<string>(), It.IsAny<bool>());

                Assert.AreEqual(0, results.Item1.Count());
                Assert.AreEqual(0, results.Item2);
            }

            [TestMethod]
            //[ExpectedException(typeof(ArgumentException))]
            public async Task SectionCoordinationServiceTests_V16_GetSectionsMaximum5Async_RepeatCodeNull_ArgumentException()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionsAsync(0, 3, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(sectionMaxEntitiesTuple);

                meeting1.Frequency = "abc";

                var results = await sectionCoordinationService.GetSectionsMaximum5Async(0, 3, It.IsAny<string>(), "2016-01-01", "2016-10-01", It.IsAny<string>(), It.IsAny<string>(), "840e72f0-57b9-42a2-ae88-df3c2262fbbc", "8d3fcd4d-cec8-4405-90eb-948392bd0a7e",
                    "840e72f0-57b9-42a2-ae88-df3c2262fbbc", new List<string>() { "558ca14c-718a-4b6e-8d92-77f498034f9f" }, "38b4330c-befa-435e-81d5-c3ffd52759f2", "b0eba383-5acf-4050-949d-8bb7a17c5012", "open", new List<string>() { "1a2e2906-d46b-4698-80f6-af87b8083c64" },
                    new List<string>() { "1a2e2906-d46b-4698-80f6-af87b8083c64" }, It.IsAny<string>(), It.IsAny<bool>());

                Assert.AreEqual(0, results.Item1.Count());
                Assert.AreEqual(0, results.Item2);
            }

            [TestMethod]
            //[ExpectedException(typeof(ArgumentException))]
            public async Task SectionCoordinationServiceTests_V16_GetSectionsMaximum5Async_GetPersonGuidFromIdAsync_Error_ArgumentException()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionsAsync(0, 3, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(sectionMaxEntitiesTuple);

                personRepoMock.Setup(repo => repo.GetPersonGuidFromIdAsync(It.IsAny<string>())).ThrowsAsync(new ArgumentException());

                var results = await sectionCoordinationService.GetSectionsMaximum5Async(0, 3, It.IsAny<string>(), "2016-01-01", "2016-10-01", It.IsAny<string>(), It.IsAny<string>(), "840e72f0-57b9-42a2-ae88-df3c2262fbbc", "8d3fcd4d-cec8-4405-90eb-948392bd0a7e",
                    "840e72f0-57b9-42a2-ae88-df3c2262fbbc", new List<string>() { "558ca14c-718a-4b6e-8d92-77f498034f9f" }, "38b4330c-befa-435e-81d5-c3ffd52759f2", "b0eba383-5acf-4050-949d-8bb7a17c5012", "open", new List<string>() { "1a2e2906-d46b-4698-80f6-af87b8083c64" },
                    new List<string>() { "1a2e2906-d46b-4698-80f6-af87b8083c64" }, It.IsAny<string>(), It.IsAny<bool>());

                Assert.IsNotNull(results);
                Assert.AreEqual(0, results.Item2);
            }

            [TestMethod]
            //[ExpectedException(typeof(RepositoryException))]
            public async Task SectionCoordinationServiceTests_V16_GetSectionsMaximum5Async_NullCourse_RepositoryException()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionsAsync(0, 3, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(sectionMaxEntitiesTuple);
                courseRepoMock.Setup(repo => repo.GetCourseByGuidAsync(It.IsAny<string>())).ReturnsAsync(() => null);

                var results = await sectionCoordinationService.GetSectionsMaximum5Async(0, 3, It.IsAny<string>(), "2016-01-01", "2016-10-01", It.IsAny<string>(), It.IsAny<string>(), "840e72f0-57b9-42a2-ae88-df3c2262fbbc", "8d3fcd4d-cec8-4405-90eb-948392bd0a7e",
                    "840e72f0-57b9-42a2-ae88-df3c2262fbbc", new List<string>() { "558ca14c-718a-4b6e-8d92-77f498034f9f" }, "38b4330c-befa-435e-81d5-c3ffd52759f2", "b0eba383-5acf-4050-949d-8bb7a17c5012", "open", new List<string>() { "1a2e2906-d46b-4698-80f6-af87b8083c64" },
                    new List<string>() { "1a2e2906-d46b-4698-80f6-af87b8083c64" }, It.IsAny<string>(), It.IsAny<bool>());

                Assert.IsNotNull(results);
                Assert.AreEqual(0, results.Item2);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionCoordinationServiceTests_V16_GetSectionMaximumByGuid5Async_WithNullId_ArgumentNullException()
            {
                var result = await sectionCoordinationService.GetSectionMaximumByGuid5Async("");
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task SectionCoordinationServiceTests_V16_GetSectionMaximumByGuid5Async_WithNullEntity_KeyNotFoundException()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionByGuid2Async(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(() => null);

                var result = await sectionCoordinationService.GetSectionMaximumByGuid5Async("0b983700-29eb-46ff-8616-21a8c3a48a0c");
            }



            #endregion

            private void BuildData()
            {
                //Course entity
                courseEntities = new TestCourseRepository().GetAsync().Result.Take(41);
                var courseEntity = courseEntities.FirstOrDefault(i => i.Id.Equals("180"));
                courseRepoMock.Setup(repo => repo.GetCourseGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(courseEntity.Guid);
                courseRepoMock.Setup(repo => repo.GetCourseByGuidAsync(courseEntity.Guid)).ReturnsAsync(courseEntity);
                //Subject entity
                subjectEntities = new TestSubjectRepository().Get();
                stuRefDataRepoMock.Setup(repo => repo.GetSubjectsAsync()).ReturnsAsync(subjectEntities);
                stuRefDataRepoMock.Setup(repo => repo.GetSubjectsAsync(It.IsAny<bool>())).ReturnsAsync(subjectEntities);
                var subjectEntity = subjectEntities.FirstOrDefault(s => s.Code.Equals(courseEntity.SubjectCode));
                //Credit Categories
                creditCategoryEntities = new List<Domain.Student.Entities.CreditCategory>()
                {
                    new Domain.Student.Entities.CreditCategory("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "I", "Institutional", Domain.Student.Entities.CreditType.Institutional),
                    new Domain.Student.Entities.CreditCategory("73244057-D1EC-4094-A0B7-DE602533E3A6", "C", "Continuing Education", Domain.Student.Entities.CreditType.ContinuingEducation),
                    new Domain.Student.Entities.CreditCategory("73244057-D1EC-4094-A0B7-DE602533E3A6", "E", "Exchange", Domain.Student.Entities.CreditType.Exchange),
                    new Domain.Student.Entities.CreditCategory("73244057-D1EC-4094-A0B7-DE602533E3A6", "O", "Other", Domain.Student.Entities.CreditType.Other),
                    new Domain.Student.Entities.CreditCategory("73244057-D1EC-4094-A0B7-DE602533E3A6", "N", "Continuing Education", Domain.Student.Entities.CreditType.None),
                    new Domain.Student.Entities.CreditCategory("1df164eb-8178-4321-a9f7-24f12d3991d8", "T", "Transfer Credit", Domain.Student.Entities.CreditType.Transfer)
                };
                stuRefDataRepoMock.Setup(repo => repo.GetCreditCategoriesAsync()).ReturnsAsync(creditCategoryEntities);
                //Grade Schemes
                gradeSchemeEntities = new TestStudentReferenceDataRepository().GetGradeSchemesAsync().Result as List<Ellucian.Colleague.Domain.Student.Entities.GradeScheme>;
                stuRefDataRepoMock.Setup(repo => repo.GetGradeSchemesAsync()).ReturnsAsync(gradeSchemeEntities);
                //Course Levels
                courseLevelEntities = new List<Domain.Student.Entities.CourseLevel>()
                {
                    new Domain.Student.Entities.CourseLevel("19f6e2cd-1e5f-4485-9b27-60d4f4e4b1ff", "100", "First Yr"),
                    new Domain.Student.Entities.CourseLevel("73244057-D1EC-4094-A0B7-DE602533E3A6", "200", "Second Year"),
                    new Domain.Student.Entities.CourseLevel("1df164eb-8178-4321-a9f7-24f12d3991d8", "300", "Third Year"),
                    new Domain.Student.Entities.CourseLevel("4aa187ae-6d22-4b10-a2e2-1304ebc18176", "400", "Third Year"),
                    new Domain.Student.Entities.CourseLevel("d9f42a0f-39de-44bc-87af-517619141bde", "500", "Third Year")
                };
                stuRefDataRepoMock.Setup(repo => repo.GetCourseLevelsAsync()).ReturnsAsync(courseLevelEntities);

                courseTitleTypeEntities = new List<CourseTitleType>()
                {
                    new Domain.Student.Entities.CourseTitleType("19f6e2cd-1e5f-4485-9b27-60d4f4e4b1ff", "short", "First Yr"),
                    new Domain.Student.Entities.CourseTitleType("73244057-D1EC-4094-A0B7-DE602533E3A6", "long", "Second Year"),
                    new Domain.Student.Entities.CourseTitleType("1df164eb-8178-4321-a9f7-24f12d3991d8", "short", "Third Year"),
                    new Domain.Student.Entities.CourseTitleType("4aa187ae-6d22-4b10-a2e2-1304ebc18176", "long", "Third Year"),
                    new Domain.Student.Entities.CourseTitleType("d9f42a0f-39de-44bc-87af-517619141bde", "short", "Third Year")
                };
                stuRefDataRepoMock.Setup(repo => repo.GetCourseTitleTypesAsync(It.IsAny<bool>())).ReturnsAsync(courseTitleTypeEntities);

                sectionStatusCodeGuidEntities = new List<SectionStatusCodeGuid>()
                {
                    new Domain.Student.Entities.SectionStatusCodeGuid("19f6e2cd-1e5f-4485-9b27-60d4f4e4b1ff", "short", "First Yr"),
                    new Domain.Student.Entities.SectionStatusCodeGuid("73244057-D1EC-4094-A0B7-DE602533E3A6", "long", "Second Year"),
                    new Domain.Student.Entities.SectionStatusCodeGuid("1df164eb-8178-4321-a9f7-24f12d3991d8", "short", "Third Year"),
                    new Domain.Student.Entities.SectionStatusCodeGuid("4aa187ae-6d22-4b10-a2e2-1304ebc18176", "long", "Third Year"),
                    new Domain.Student.Entities.SectionStatusCodeGuid("d9f42a0f-39de-44bc-87af-517619141bde", "short", "Third Year")
                };
                sectionRepoMock.Setup(repo => repo.GetStatusCodesWithGuidsAsync()).ReturnsAsync(sectionStatusCodeGuidEntities);


                //IEnumerable<SectionMeeting>
                var sectionFaculty = new Domain.Student.Entities.SectionFaculty("1234", "SEC1", "0000678", "LG", new DateTime(2012, 09, 01), new DateTime(2011, 12, 21), 100.0m);
                var sectionFacultyTuple = new Tuple<IEnumerable<Domain.Student.Entities.SectionFaculty>, int>(new List<Domain.Student.Entities.SectionFaculty>() { sectionFaculty }, 1);
                sectionRepoMock.Setup(repo => repo.GetSectionFacultyAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>())).ReturnsAsync(sectionFacultyTuple);

                meeting1 = new Domain.Student.Entities.SectionMeeting("123", "SEC1", "LG", new DateTime(2012, 01, 01), new DateTime(2012, 12, 21), "W");
                meeting1.Guid = "1f1485cb-2705-4b59-9f51-7aa85c79b377";
                meeting1.AddFacultyId("0000678");
                meeting1.AddSectionFaculty(sectionFaculty);
                meeting1.StartTime = new DateTimeOffset(2012, 01, 01, 08, 30, 00, new TimeSpan());
                meeting1.EndTime = new DateTimeOffset(2012, 12, 21, 09, 30, 00, new TimeSpan());
                meeting1.Room = "COE*Room 1";
                var sectionMeetings = new List<Domain.Student.Entities.SectionMeeting>();
                sectionMeetings.Add(meeting1);
                var tuple = new Tuple<IEnumerable<Domain.Student.Entities.SectionMeeting>, int>(sectionMeetings, 1);
                sectionRepoMock.Setup(repo => repo.GetSectionMeetingAsync(0, 0, "1", "", "", "", "", new List<string>(), new List<string>(), new List<string>(), "")).ReturnsAsync(tuple);
                sectionRepoMock.Setup(repo => repo.GetSectionGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("ad7655d0-77e3-4f8a-a07c-5c86f6a6f86a");

                //Instructor Id
                personRepoMock.Setup(repo => repo.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("ebf585ad-cc1f-478f-a7a3-aefae87f873a");
                List<PersonPin> personPinList = new List<PersonPin>()
                {
                    new PersonPin("1", "PersonPin1")
                };
                personRepoMock.Setup(repo => repo.GetPersonPinsAsync(It.IsAny<string[]>())).ReturnsAsync(personPinList);
                //Instructional Methods
                instrMethodEntities = new List<Domain.Student.Entities.InstructionalMethod>()
                {
                    new Domain.Student.Entities.InstructionalMethod("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "LG", "LG", false),
                    new Domain.Student.Entities.InstructionalMethod("73244057-D1EC-4094-A0B7-DE602533E3A6", "30", "30", true),
                    new Domain.Student.Entities.InstructionalMethod("1df164eb-8178-4321-a9f7-24f12d3991d8", "04", "04", false)
                };
                stuRefDataRepoMock.Setup(repo => repo.GetInstructionalMethodsAsync(It.IsAny<bool>())).ReturnsAsync(instrMethodEntities);
                //schedule repeats
                var scheduleRepeats = new List<Domain.Base.Entities.ScheduleRepeat>()
                {
                    new Domain.Base.Entities.ScheduleRepeat("W", "Weekly", "7", FrequencyType.Weekly),
                    new Domain.Base.Entities.ScheduleRepeat("D", "Daily", "1", FrequencyType.Daily),
                    new Domain.Base.Entities.ScheduleRepeat("M", "Monthly", "30", FrequencyType.Monthly),
                    new Domain.Base.Entities.ScheduleRepeat("Y", "Yearly", "365", FrequencyType.Yearly)
                };
                referenceDataRepoMock.Setup(repo => repo.ScheduleRepeats).Returns(scheduleRepeats);
                //Rooms entities
                roomEntities = new List<Domain.Base.Entities.Room>
                {
                    new Room("2ae6e009-40ca-4ac0-bb41-c123f7c344e3", "COE*Room 1", "COE")
                    {
                        Capacity = 50,
                        RoomType = "110",
                        Name = "Room 1"
                    },
                    new Room("8c92e963-5f05-45a2-8484-d9ad21e6ab47", "COE*0110", "CEE")
                    {
                        Capacity = 100,
                        RoomType = "111",
                        Name = "Room 2"
                    },
                    new Room("8fdbaec7-4198-4348-b95a-a48a357e67f5", "COE*0120", "CDF")
                    {
                        Capacity = 20,
                        RoomType = "111",
                        Name = "Room 13"
                    },
                    new Room("327a6856-0230-4a6d-82ed-5c99dc1b1862", "COE*0121", "CSD")
                    {
                        Capacity = 50,
                        RoomType = "111",
                        Name = "Room 112"
                    },
                    new Room("cc9aa34c-db5e-46dc-9e5b-ba3f4b2557a8", "EIN*0121", "BSF")
                    {
                        Capacity = 30,
                        RoomType = "111",
                        Name = "Room BSF"
                    }
                };
                roomRepoMock.Setup(repo => repo.RoomsAsync()).ReturnsAsync(roomEntities);
                foreach (var r in roomEntities)
                {
                    roomRepoMock.Setup(repo => repo.GetRoomsGuidAsync(r.Id)).ReturnsAsync(r.Guid);
                }

                //Buildings entities
                buildingEntities = new List<Domain.Base.Entities.Building>
                {
                    new Building("e81f3642-a585-4187-8616-6eb4919a6990", "COE", "Coe Building")
                    {},
                    new Building("e5f177e3-3f54-40d2-9235-1c7552e19729", "EIN", "Einstein Building")
                    {}
                };
                referenceDataRepoMock.Setup(repo => repo.BuildingsAsync()).ReturnsAsync(buildingEntities);

                //Person
                person = new Domain.Base.Entities.PersonIntegration("1", "Brown")
                {
                    Guid = "96cf912f-5e87-4099-96bf-73baac4c7715",
                    Prefix = "Mr.",
                    FirstName = "Ricky",
                    MiddleName = "Lee",
                    Suffix = "Jr.",
                    Nickname = "Rick",
                    BirthDate = new DateTime(1930, 1, 1),
                    DeceasedDate = new DateTime(2014, 5, 12),
                    GovernmentId = "111-11-1111",
                    MaritalStatusCode = "M",
                    EthnicCodes = new List<string> { "H" },
                    RaceCodes = new List<string> { "AS" }
                };
                person.AddEmailAddress(new EmailAddress("xyz@xmail.com", "COL"));
                person.AddPersonAlt(new PersonAlt("1", Domain.Base.Entities.PersonAlt.ElevatePersonAltType.ToString()));

                personRepoMock.Setup(repo => repo.GetPersonIntegration2ByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(person);
                // Mock the reference repository for prefix
                referenceDataRepoMock.Setup(repo => repo.Prefixes).Returns(new List<Prefix>()
                {
                    new Prefix("MR","Mr","Mr."),
                    new Prefix("MS","Ms","Ms."),
                    new Prefix("JR","Jr","Jr."),
                    new Prefix("SR","Sr","Sr.")
                });

                // Mock the reference repository for suffix
                referenceDataRepoMock.Setup(repo => repo.Suffixes).Returns(new List<Suffix>()
                {
                    new Suffix("JR","Jr","Jr."),
                    new Suffix("SR","Sr","Sr.")
                });

                //Acad Periods
                var registrationDates = new RegistrationDate(null, DateTime.Today.AddYears(1), DateTime.Today.AddYears(1), DateTime.Today.AddYears(1), DateTime.Today.AddYears(1),
                    DateTime.Today.AddYears(1), DateTime.Today.AddYears(1), DateTime.Today.AddYears(1), DateTime.Today.AddYears(1), DateTime.Today.AddYears(1), new List<DateTime?>() { DateTime.Today.AddYears(1) });
                acadPeriodsEntities = new List<Domain.Student.Entities.AcademicPeriod>()
                {
                    new Domain.Student.Entities.AcademicPeriod("8d3fcd4d-cec8-4405-90eb-948392bd0a7e", "2012/FA", "Fall 2014", DateTime.Today.AddDays(-60), DateTime.Today.AddDays(10),
                        DateTime.Today.Year, 4, "2012/FA", "5f7e7071-5aef-4d22-891f-86ab472a9f15", "edcfd1ee-4adf-46bc-8b87-8853ae49dbeb",new List<RegistrationDate>(){registrationDates} )
                };
                termRepoMock.Setup(repo => repo.GetAcademicPeriods(It.IsAny<IEnumerable<Term>>())).Returns(acadPeriodsEntities);

                academicLevelEntities = new TestAcademicLevelRepository().GetAsync().Result as List<Ellucian.Colleague.Domain.Student.Entities.AcademicLevel>;
                stuRefDataRepoMock.Setup(repo => repo.GetAcademicLevelsAsync()).ReturnsAsync(academicLevelEntities);

                locationEntities = new TestLocationRepository().Get();
                referenceDataRepoMock.Setup(repo => repo.Locations).Returns(locationEntities);

                departmentEntities = new TestDepartmentRepository().Get().ToList();
                referenceDataRepoMock.Setup(repo => repo.DepartmentsAsync()).ReturnsAsync(departmentEntities);

                instructionalPlatformEntities = new List<InstructionalPlatform>
                {
                    new InstructionalPlatform("840e72f0-57b9-42a2-ae88-df3c2262fbbc", "CE", "Continuing Education"),
                    new InstructionalPlatform("e986b8a5-25f3-4aa0-bd0e-90982865e749", "D", "Institutional"),
                    new InstructionalPlatform("b5cc288b-8692-474e-91be-bdc55778e2f5", "TR", "Transfer")
                };
                referenceDataRepoMock.Setup(repo => repo.GetInstructionalPlatformsAsync(It.IsAny<bool>())).ReturnsAsync(instructionalPlatformEntities);

                sectionRepoMock.Setup(repo => repo.GetUnidataFormattedDate(It.IsAny<string>())).ReturnsAsync("2016/01/01");
                sectionRepoMock.Setup(repo => repo.GetCourseIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
                sectionRepoMock.Setup(repo => repo.ConvertStatusToStatusCodeAsync(It.IsAny<string>())).ReturnsAsync("Open");

                sectionEntities = new List<Domain.Student.Entities.Section>()
                {
                    new Domain.Student.Entities.Section("1", "180", "111", new DateTime(2016, 01, 01), 3, 3, "Title 1", "I", new Collection<Domain.Student.Entities.OfferingDepartment>(){ new Domain.Student.Entities.OfferingDepartment("MATH") },
                        new Collection<string>(){ "100", "200"}, "UG", new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddYears(-6)) })
                        {
                            Guid = "0b983700-29eb-46ff-8616-21a8c3a48a0c",
                            LearningProvider = "CE",
                            TermId = "2012/FA",
                            Location = "MAIN",
                            NumberOfWeeks = 2,
                            NumberOnWaitlist = 10,
                            GradeSchemeCode = "UG",
                            BillingCred = 1
                        }
                };
                sectionMaxEntitiesTuple = new Tuple<IEnumerable<Section>, int>(sectionEntities, sectionEntities.Count());
            }
        }

        [TestClass]
        public class UpdateSectionBookAsync : CurrentUserSetup
        {
            private SectionCoordinationService sectionCoordinationService;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IStudentReferenceDataRepository> stuRefDataRepoMock;
            private IStudentReferenceDataRepository studentReferenceDataRepo;
            private Mock<IReferenceDataRepository> referenceDataRepoMock;
            private IReferenceDataRepository referenceDataRepo;
            private Mock<ICourseRepository> courseRepoMock;
            private ICourseRepository courseRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IStudentConfigurationRepository> studentConfigRepoMock;
            private IStudentConfigurationRepository studentConfigRepo;
            private Mock<IConfigurationRepository> configRepoMock;
            private IConfigurationRepository configRepo;
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IRoomRepository> roomRepoMock;
            private IRoomRepository roomRepo;
            private Mock<IEventRepository> eventRepoMock;
            private IEventRepository eventRepo;
            private Mock<IBookRepository> bookRepoMock;
            private IBookRepository bookRepo;


            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ILogger logger;
            private ICurrentUserFactory currentUserFactory;

            private Domain.Student.Entities.Section section1;
            private Domain.Student.Entities.Section section2;
            private Domain.Student.Entities.Book textbook;

            private List<OfferingDepartment> dpts = new List<OfferingDepartment>() { new OfferingDepartment("ART", 100m) };
            private List<string> levels = new List<string>() { "Whatever" };
            private List<SectionStatusItem> statuses = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-60)) };
            private List<SectionStatusItem> statuses2 = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Inactive, "I", DateTime.Today.AddDays(-60)) };
            private List<SectionStatusItem> statuses3 = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Cancelled, "C", DateTime.Today.AddDays(-60)) };
            private IEnumerable<Domain.Student.Entities.Course> allCourses = new TestCourseRepository().GetAsync().Result;

            [TestInitialize]
            public void Initialize()
            {
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                roomRepoMock = new Mock<IRoomRepository>();
                roomRepo = roomRepoMock.Object;
                stuRefDataRepoMock = new Mock<IStudentReferenceDataRepository>();
                studentReferenceDataRepo = stuRefDataRepoMock.Object;
                referenceDataRepoMock = new Mock<IReferenceDataRepository>();
                referenceDataRepo = referenceDataRepoMock.Object;
                courseRepoMock = new Mock<ICourseRepository>();
                courseRepo = courseRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                studentConfigRepoMock = new Mock<IStudentConfigurationRepository>();
                studentConfigRepo = studentConfigRepoMock.Object;
                configRepoMock = new Mock<IConfigurationRepository>();
                configRepo = configRepoMock.Object;
                eventRepoMock = new Mock<IEventRepository>();
                eventRepo = eventRepoMock.Object;
                bookRepoMock = new Mock<IBookRepository>();
                bookRepo = bookRepoMock.Object;


                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;

                // Set Current User
                currentUserFactory = new CurrentUserSetup.StudentUserFactory();

                logger = new Mock<ILogger>().Object;

                // Mock the section repo response
                section1 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC1", "1119", "01", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses, true);
                section1.AddFaculty("STU1");
                section2 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC2", "1119", "02", new DateTime(2012, 09, 01), 3.0m, null, "Introduction to Art", "IN", dpts, levels, "UG", statuses2, true);

                sectionRepoMock.Setup(repo => repo.GetSectionAsync("SEC1")).ReturnsAsync(section1);
                sectionRepoMock.Setup(repo => repo.GetSectionAsync("SEC2")).ReturnsAsync(section2);


                textbook = new Domain.Student.Entities.Book("", "123456789", "Title", "Author", "Publisher", "Copyright",
                                "Edition", true, 10m, 20m, "Comment", "External Comments", "altId1", "altId2", "altId3");
                bookRepoMock.Setup(repo => repo.CreateBookAsync(It.IsAny<Domain.Student.Entities.SectionTextbook>())).ReturnsAsync(textbook);

                var sectionAdapter = new Student.Adapters.SectionEntityToSection3DtoAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Section, Section3>()).Returns(sectionAdapter);
                var sectionTextbookAdapter = new Student.Adapters.SectionTextbookDtoToEntityAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Dtos.Student.SectionTextbook, Domain.Student.Entities.SectionTextbook>()).Returns(sectionTextbookAdapter);

                // Mock the section service
                sectionCoordinationService = new SectionCoordinationService(adapterRegistry, sectionRepo, courseRepo, studentRepo, studentReferenceDataRepo,
                    referenceDataRepo, termRepo, studentConfigRepo, configRepo, personRepo, roomRepo, eventRepo, bookRepo, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionRepo = null;
                studentRepo = null;
                roleRepo = null;
                adapterRegistry = null;
                sectionCoordinationService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionCoordinationService_UpdateSectionBookAsync_TextbookNull()
            {
                var result = await sectionCoordinationService.UpdateSectionBookAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionCoordinationService_UpdateSectionBookAsync_SectionIdNull()
            {
                var sectionTextbook = new Dtos.Student.SectionTextbook()
                {
                    SectionId = null,
                    Action = Dtos.Student.SectionBookAction.Update,
                    RequirementStatusCode = "R",
                    Textbook = new Dtos.Student.Book()
                    {
                        Id = "123",
                        Isbn = "123456789",
                        Title = "Title",
                        Author = "Author",
                        Publisher = "Publisher",
                        Copyright = "Copyright",
                        Edition = "Edition",
                        IsActive = true,
                        PriceUsed = 10m,
                        Price = 20m,
                        Comment = "Comment",
                        ExternalComments = "External Comments",
                        AlternateID1 = "altId1",
                        AlternateID2 = "altId2",
                        AlternateID3 = "altId3"
                    }
                };
                var result = await sectionCoordinationService.UpdateSectionBookAsync(sectionTextbook);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionCoordinationService_UpdateSectionBookAsync_SectionIdEmpty()
            {
                var sectionTextbook = new Dtos.Student.SectionTextbook()
                {
                    SectionId = string.Empty,
                    Action = Dtos.Student.SectionBookAction.Update,
                    RequirementStatusCode = "R",
                    Textbook = new Dtos.Student.Book()
                    {
                        Id = "123",
                        Isbn = "123456789",
                        Title = "Title",
                        Author = "Author",
                        Publisher = "Publisher",
                        Copyright = "Copyright",
                        Edition = "Edition",
                        IsActive = true,
                        PriceUsed = 10m,
                        Price = 20m,
                        Comment = "Comment",
                        ExternalComments = "External Comments",
                        AlternateID1 = "altId1",
                        AlternateID2 = "altId2",
                        AlternateID3 = "altId3"
                    }
                };
                var result = await sectionCoordinationService.UpdateSectionBookAsync(sectionTextbook);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task SectionCoordinationService_UpdateSectionBookAsync_UserHasBadPermissions()
            {
                var sectionTextbook = new Dtos.Student.SectionTextbook()
                {
                    SectionId = "SEC2",
                    Action = Dtos.Student.SectionBookAction.Update,
                    RequirementStatusCode = "R",
                    Textbook = new Dtos.Student.Book()
                    {
                        Id = "123",
                        Isbn = "123456789",
                        Title = "Title",
                        Author = "Author",
                        Publisher = "Publisher",
                        Copyright = "Copyright",
                        Edition = "Edition",
                        IsActive = true,
                        PriceUsed = 10m,
                        Price = 20m,
                        Comment = "Comment",
                        ExternalComments = "External Comments",
                        AlternateID1 = "altId1",
                        AlternateID2 = "altId2",
                        AlternateID3 = "altId3"
                    }
                };
                sectionRepoMock.Setup(repo => repo.UpdateSectionBookAsync(It.IsAny<Domain.Student.Entities.SectionTextbook>())).ReturnsAsync(section2);
                var result = await sectionCoordinationService.UpdateSectionBookAsync(sectionTextbook);
            }

            [TestMethod]
            public async Task SectionCoordinationService_UpdateSectionBookAsync_UpdatedSection()
            {
                var sectionTextbook = new Dtos.Student.SectionTextbook()
                {
                    SectionId = "SEC1",
                    Action = Dtos.Student.SectionBookAction.Update,
                    RequirementStatusCode = "R",
                    Textbook = new Dtos.Student.Book()
                    {
                        Id = "123",
                        Isbn = "123456789",
                        Title = "Title",
                        Author = "Author",
                        Publisher = "Publisher",
                        Copyright = "Copyright",
                        Edition = "Edition",
                        IsActive = true,
                        PriceUsed = 10m,
                        Price = 20m,
                        Comment = "Comment",
                        ExternalComments = "External Comments",
                        AlternateID1 = "altId1",
                        AlternateID2 = "altId2",
                        AlternateID3 = "altId3"
                    }
                };
                sectionRepoMock.Setup(repo => repo.UpdateSectionBookAsync(It.IsAny<Domain.Student.Entities.SectionTextbook>())).ReturnsAsync(section1);
                var result = await sectionCoordinationService.UpdateSectionBookAsync(sectionTextbook);
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(Section3));
            }

            [TestMethod]
            public async Task SectionCoordinationService_UpdateSectionBookAsync_UpdatedSection_RemoveAction()
            {
                var sectionTextbook = new Dtos.Student.SectionTextbook()
                {
                    SectionId = "SEC1",
                    Action = Dtos.Student.SectionBookAction.Remove,
                    RequirementStatusCode = "R",
                    Textbook = new Dtos.Student.Book()
                    {
                        Id = "123",
                        Isbn = "123456789",
                        Title = "Title",
                        Author = "Author",
                        Publisher = "Publisher",
                        Copyright = "Copyright",
                        Edition = "Edition",
                        IsActive = true,
                        PriceUsed = 10m,
                        Price = 20m,
                        Comment = "Comment",
                        ExternalComments = "External Comments",
                        AlternateID1 = "altId1",
                        AlternateID2 = "altId2",
                        AlternateID3 = "altId3"
                    }
                };
                sectionRepoMock.Setup(repo => repo.UpdateSectionBookAsync(It.IsAny<Domain.Student.Entities.SectionTextbook>())).ReturnsAsync(section1);
                var result = await sectionCoordinationService.UpdateSectionBookAsync(sectionTextbook);
                Assert.AreEqual(Dtos.Student.SectionBookAction.Remove, sectionTextbook.Action);
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(Section3));
            }

            [TestMethod]
            public async Task SectionCoordinationService_UpdateSectionBookAsync_UpdatedSection_AddAction()
            {
                var sectionTextbook = new Dtos.Student.SectionTextbook()
                {
                    SectionId = "SEC1",
                    Action = Dtos.Student.SectionBookAction.Add,
                    RequirementStatusCode = "R",
                    Textbook = new Dtos.Student.Book()
                    {
                        Id = "123",
                        Isbn = "123456789",
                        Title = "Title",
                        Author = "Author",
                        Publisher = "Publisher",
                        Copyright = "Copyright",
                        Edition = "Edition",
                        IsActive = true,
                        PriceUsed = 10m,
                        Price = 20m,
                        Comment = "Comment",
                        ExternalComments = "External Comments",
                        AlternateID1 = "altId1",
                        AlternateID2 = "altId2",
                        AlternateID3 = "altId3"
                    }
                };
                sectionRepoMock.Setup(repo => repo.UpdateSectionBookAsync(It.IsAny<Domain.Student.Entities.SectionTextbook>())).ReturnsAsync(section1);
                var result = await sectionCoordinationService.UpdateSectionBookAsync(sectionTextbook);
                Assert.AreEqual(Dtos.Student.SectionBookAction.Add, sectionTextbook.Action);
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(Section3));
            }

            [TestMethod]
            public async Task SectionCoordinationService_UpdateSectionBookAsync_UpdatedSection_UpdateAction()
            {
                var sectionTextbook = new Dtos.Student.SectionTextbook()
                {
                    SectionId = "SEC1",
                    Action = Dtos.Student.SectionBookAction.Update,
                    RequirementStatusCode = "R",
                    Textbook = new Dtos.Student.Book()
                    {
                        Id = "123",
                        Isbn = "123456789",
                        Title = "Title",
                        Author = "Author",
                        Publisher = "Publisher",
                        Copyright = "Copyright",
                        Edition = "Edition",
                        IsActive = true,
                        PriceUsed = 10m,
                        Price = 20m,
                        Comment = "Comment",
                        ExternalComments = "External Comments",
                        AlternateID1 = "altId1",
                        AlternateID2 = "altId2",
                        AlternateID3 = "altId3"
                    }
                };
                sectionRepoMock.Setup(repo => repo.UpdateSectionBookAsync(It.IsAny<Domain.Student.Entities.SectionTextbook>())).ReturnsAsync(section1);
                var result = await sectionCoordinationService.UpdateSectionBookAsync(sectionTextbook);
                Assert.AreEqual(Dtos.Student.SectionBookAction.Update, sectionTextbook.Action);
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(Section3));
            }

            [TestMethod]
            public async Task SectionCoordinationService_UpdateSectionBookAsync_UpdatedSection_NoTextbookId()
            {
                var sectionTextbook = new Dtos.Student.SectionTextbook()
                {
                    SectionId = "SEC1",
                    Action = Dtos.Student.SectionBookAction.Add,
                    RequirementStatusCode = "R",
                    Textbook = new Dtos.Student.Book()
                    {
                        Id = "",
                        Isbn = "123456789",
                        Title = "Title",
                        Author = "Author",
                        Publisher = "Publisher",
                        Copyright = "Copyright",
                        Edition = "Edition",
                        IsActive = true,
                        PriceUsed = 10m,
                        Price = 20m,
                        Comment = "Comment",
                        ExternalComments = "External Comments",
                        AlternateID1 = "altId1",
                        AlternateID2 = "altId2",
                        AlternateID3 = "altId3"
                    }
                };
                sectionRepoMock.Setup(repo => repo.UpdateSectionBookAsync(It.IsAny<Domain.Student.Entities.SectionTextbook>())).ReturnsAsync(section1);

                var result = await sectionCoordinationService.UpdateSectionBookAsync(sectionTextbook);
                Assert.AreEqual(Dtos.Student.SectionBookAction.Add, sectionTextbook.Action);
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(Section3));
            }
        }

        [TestClass]
        public class GetSectionMeetingInstance : SectionCoordinationServiceTests
        {
            private SectionCoordinationService sectionCoordinationService;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IStudentReferenceDataRepository> stuRefDataRepoMock;
            private IStudentReferenceDataRepository studentReferenceDataRepo;
            private Mock<IReferenceDataRepository> referenceDataRepoMock;
            private IReferenceDataRepository referenceDataRepo;
            private Mock<ICourseRepository> courseRepoMock;
            private ICourseRepository courseRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IStudentConfigurationRepository> studentConfigRepoMock;
            private IStudentConfigurationRepository studentConfigRepo;
            private Mock<IConfigurationRepository> configRepoMock;
            private IConfigurationRepository configRepo;
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IRoomRepository> roomRepoMock;
            private IRoomRepository roomRepo;
            private Mock<IEventRepository> eventRepoMock;
            private IEventRepository eventRepo;
            private Mock<IBookRepository> bookRepoMock;
            private IBookRepository bookRepo;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ILogger logger;
            private ICurrentUserFactory currentUserFactory;

            [TestInitialize]
            public void GetSectionMeetingInstance_Initialize()
            {
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                roomRepoMock = new Mock<IRoomRepository>();
                roomRepo = roomRepoMock.Object;
                stuRefDataRepoMock = new Mock<IStudentReferenceDataRepository>();
                studentReferenceDataRepo = stuRefDataRepoMock.Object;
                referenceDataRepoMock = new Mock<IReferenceDataRepository>();
                referenceDataRepo = referenceDataRepoMock.Object;
                courseRepoMock = new Mock<ICourseRepository>();
                courseRepo = courseRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                studentConfigRepoMock = new Mock<IStudentConfigurationRepository>();
                studentConfigRepo = studentConfigRepoMock.Object;
                configRepoMock = new Mock<IConfigurationRepository>();
                configRepo = configRepoMock.Object;
                eventRepoMock = new Mock<IEventRepository>();
                eventRepo = eventRepoMock.Object;
                bookRepoMock = new Mock<IBookRepository>();
                bookRepo = bookRepoMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;

                // Set Current User
                currentUserFactory = new CurrentUserSetup.StudentUserFactory();

                logger = new Mock<ILogger>().Object;

                // Mock the SectionMeetingInstance adapter
                var sectionAdapter = new AutoMapperAdapter<Domain.Student.Entities.SectionMeetingInstance, Dtos.Student.SectionMeetingInstance>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.SectionMeetingInstance, Dtos.Student.SectionMeetingInstance>()).Returns(sectionAdapter);


                // Mock the section service
                sectionCoordinationService = new SectionCoordinationService(adapterRegistry, sectionRepo, courseRepo, studentRepo, studentReferenceDataRepo,
                    referenceDataRepo, termRepo, studentConfigRepo, configRepo, personRepo, roomRepo, eventRepo, bookRepo, currentUserFactory, roleRepo, logger);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetSectionMeetingInstance_Null_SectionId()
            {
                var instances = await sectionCoordinationService.GetSectionMeetingInstancesAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task GetSectionMeetingInstance_Repository_Returns_Null()
            {
                var sectionId = "1234";
                sectionRepoMock.Setup(repo => repo.GetSectionMeetingInstancesAsync(sectionId)).ReturnsAsync(() => null);
                var instances = await sectionCoordinationService.GetSectionMeetingInstancesAsync(sectionId);
            }

            [TestMethod]
            public async Task GetSectionMeetingInstance_Returns_SectionMeetingInstance_Objects()
            {
                var sectionId = "1234";
                List<Domain.Student.Entities.SectionMeetingInstance> instanceEntities = new List<Domain.Student.Entities.SectionMeetingInstance>()
                {
                    new Domain.Student.Entities.SectionMeetingInstance("1", sectionId, DateTime.Today.AddDays(1), DateTime.Now.AddHours(2), DateTime.Now.AddHours(3)),
                    new Domain.Student.Entities.SectionMeetingInstance("2", sectionId, DateTime.Today.AddDays(3), DateTime.Now.AddHours(2), DateTime.Now.AddHours(3)),
                };
                sectionRepoMock.Setup(repo => repo.GetSectionMeetingInstancesAsync(sectionId)).ReturnsAsync(instanceEntities);
                var instances = await sectionCoordinationService.GetSectionMeetingInstancesAsync(sectionId);
                Assert.AreEqual(instanceEntities.Count, instances.Count());
            }
        }

        [TestClass]
        public class GetSection_V1 : CurrentUserSetup
        {
            private SectionCoordinationService sectionCoordinationService;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IStudentReferenceDataRepository> stuRefDataRepoMock;
            private IStudentReferenceDataRepository studentReferenceDataRepo;
            private Mock<IReferenceDataRepository> referenceDataRepoMock;
            private IReferenceDataRepository referenceDataRepo;
            private Mock<ICourseRepository> courseRepoMock;
            private ICourseRepository courseRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IStudentConfigurationRepository> studentConfigRepoMock;
            private IStudentConfigurationRepository studentConfigRepo;
            private Mock<IConfigurationRepository> configRepoMock;
            private IConfigurationRepository configRepo;
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IRoomRepository> roomRepoMock;
            private IRoomRepository roomRepo;
            private Mock<IEventRepository> eventRepoMock;
            private IEventRepository eventRepo;
            private Mock<IBookRepository> bookRepoMock;
            private IBookRepository bookRepo;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ILogger logger;
            private ICurrentUserFactory currentUserFactory;

            private Domain.Student.Entities.Section section1;
            private Domain.Student.Entities.Section section2;
            private Domain.Student.Entities.Section section3;
            private Domain.Student.Entities.Section section4;

            private List<OfferingDepartment> dpts = new List<OfferingDepartment>() { new OfferingDepartment("ART", 100m) };
            private List<string> levels = new List<string>() { "Whatever" };
            private List<SectionStatusItem> statuses = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-60)) };

            [TestInitialize]
            public void Initialize()
            {
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                roomRepoMock = new Mock<IRoomRepository>();
                roomRepo = roomRepoMock.Object;
                stuRefDataRepoMock = new Mock<IStudentReferenceDataRepository>();
                studentReferenceDataRepo = stuRefDataRepoMock.Object;
                referenceDataRepoMock = new Mock<IReferenceDataRepository>();
                referenceDataRepo = referenceDataRepoMock.Object;
                courseRepoMock = new Mock<ICourseRepository>();
                courseRepo = courseRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                studentConfigRepoMock = new Mock<IStudentConfigurationRepository>();
                studentConfigRepo = studentConfigRepoMock.Object;
                configRepoMock = new Mock<IConfigurationRepository>();
                configRepo = configRepoMock.Object;
                eventRepoMock = new Mock<IEventRepository>();
                eventRepo = eventRepoMock.Object;
                bookRepoMock = new Mock<IBookRepository>();
                bookRepo = bookRepoMock.Object;

                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;

                // "STU1" is the current user
                currentUserFactory = new CurrentUserSetup.StudentUserFactory();

                logger = new Mock<ILogger>().Object;

                var sectionAdapter = new AutoMapperAdapter<Domain.Student.Entities.Section, Dtos.Student.Section>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.Section, Dtos.Student.Section>()).Returns(sectionAdapter);


                // Mock the section repo response
                section1 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC1", "1119", "01", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses, true);
                section1.TermId = "2012/FA";
                section1.EndDate = new DateTime(2012, 12, 21);
                section1.LearningProvider = "Ellucian";
                section1.AddActiveStudent("student-1");
                section1.AddActiveStudent("student-2");
                section1.AddFaculty("0000678");

                var sections = new List<Domain.Student.Entities.Section>();
                sections.Add(section1);

                List<string> sec1List = new List<string>() { "SEC1" };
                List<Domain.Student.Entities.Section> sec1Resp = new List<Domain.Student.Entities.Section>() { section1 };
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(sec1List, false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sec1Resp));


                List<string> sec4List = new List<string>() { "9999999" };
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(sec4List, false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(new List<Domain.Student.Entities.Section>()));

                // Mock the section service
                sectionCoordinationService = new SectionCoordinationService(adapterRegistry, sectionRepo, courseRepo, studentRepo, studentReferenceDataRepo,
                    referenceDataRepo, termRepo, studentConfigRepo, configRepo, personRepo, roomRepo, eventRepo, bookRepo, currentUserFactory, roleRepo, logger);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetSection_SectionId_As_Null()
            {
                await sectionCoordinationService.GetSectionAsync(null, true);

            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetSection_SectionId_As_Empty()
            {
                await sectionCoordinationService.GetSectionAsync("9999999", true);

            }

            [TestMethod]
            public async Task GetSection_With_Student_User()
            {
                PrivacyWrapper<Dtos.Student.Section> section = await sectionCoordinationService.GetSectionAsync("SEC1", true);
                Assert.IsTrue(section.HasPrivacyRestrictions);
                Assert.AreEqual(0, section.Dto.ActiveStudentIds.Count());
            }

            [TestMethod]
            public async Task GetSection_With_Faculty_User_And_Assigned_To_Section()
            {
                currentUserFactory = new CurrentUserSetup.FacultytUserFactory();
                // Mock the section service
                sectionCoordinationService = new SectionCoordinationService(adapterRegistry, sectionRepo, courseRepo, studentRepo, studentReferenceDataRepo,
                    referenceDataRepo, termRepo, studentConfigRepo, configRepo, personRepo, roomRepo, eventRepo, bookRepo, currentUserFactory, roleRepo, logger);
                PrivacyWrapper<Dtos.Student.Section> section = await sectionCoordinationService.GetSectionAsync("SEC1", true);
                Assert.IsFalse(section.HasPrivacyRestrictions);
                Assert.IsNotNull(section.Dto.ActiveStudentIds);
                Assert.AreEqual(2, section.Dto.ActiveStudentIds.Count());
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionRepo = null;
                studentRepo = null;
                roleRepo = null;
                adapterRegistry = null;
                sectionCoordinationService = null;
            }

        }

        [TestClass]
        public class GetSection_V2 : CurrentUserSetup
        {
            private SectionCoordinationService sectionCoordinationService;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IStudentReferenceDataRepository> stuRefDataRepoMock;
            private IStudentReferenceDataRepository studentReferenceDataRepo;
            private Mock<IReferenceDataRepository> referenceDataRepoMock;
            private IReferenceDataRepository referenceDataRepo;
            private Mock<ICourseRepository> courseRepoMock;
            private ICourseRepository courseRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IStudentConfigurationRepository> studentConfigRepoMock;
            private IStudentConfigurationRepository studentConfigRepo;
            private Mock<IConfigurationRepository> configRepoMock;
            private IConfigurationRepository configRepo;
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IRoomRepository> roomRepoMock;
            private IRoomRepository roomRepo;
            private Mock<IEventRepository> eventRepoMock;
            private IEventRepository eventRepo;
            private Mock<IBookRepository> bookRepoMock;
            private IBookRepository bookRepo;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ILogger logger;
            private ICurrentUserFactory currentUserFactory;

            private Domain.Student.Entities.Section section1;
            private Domain.Student.Entities.Section section2;
            private Domain.Student.Entities.Section section3;

            private List<OfferingDepartment> dpts = new List<OfferingDepartment>() { new OfferingDepartment("ART", 100m) };
            private List<string> levels = new List<string>() { "Whatever" };
            private List<SectionStatusItem> statuses = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-60)) };

            [TestInitialize]
            public void Initialize()
            {
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                roomRepoMock = new Mock<IRoomRepository>();
                roomRepo = roomRepoMock.Object;
                stuRefDataRepoMock = new Mock<IStudentReferenceDataRepository>();
                studentReferenceDataRepo = stuRefDataRepoMock.Object;
                referenceDataRepoMock = new Mock<IReferenceDataRepository>();
                referenceDataRepo = referenceDataRepoMock.Object;
                courseRepoMock = new Mock<ICourseRepository>();
                courseRepo = courseRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                studentConfigRepoMock = new Mock<IStudentConfigurationRepository>();
                studentConfigRepo = studentConfigRepoMock.Object;
                configRepoMock = new Mock<IConfigurationRepository>();
                configRepo = configRepoMock.Object;
                eventRepoMock = new Mock<IEventRepository>();
                eventRepo = eventRepoMock.Object;
                bookRepoMock = new Mock<IBookRepository>();
                bookRepo = bookRepoMock.Object;

                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;

                // "STU1" is the current user
                currentUserFactory = new CurrentUserSetup.StudentUserFactory();

                logger = new Mock<ILogger>().Object;

                var sectionAdapter = new AutoMapperAdapter<Domain.Student.Entities.Section, Dtos.Student.Section2>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.Section, Dtos.Student.Section2>()).Returns(sectionAdapter);


                // section have faculty same as logged in user
                section1 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC1", "1119", "01", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses, true);
                section1.TermId = "2012/FA";
                section1.EndDate = new DateTime(2012, 12, 21);
                section1.LearningProvider = "Ellucian";
                section1.AddActiveStudent("student-1");
                section1.AddActiveStudent("student-2");
                section1.AddFaculty("0000678");

                //section with no faculty assigned
                section2 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC2", "1119", "01", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses, true);
                section2.TermId = "2012/FA";
                section2.EndDate = new DateTime(2012, 12, 21);
                section2.LearningProvider = "Ellucian";
                section2.AddActiveStudent("student-1");
                section2.AddActiveStudent("student-2");


                //section with  faculty assigned but not the same faculty as lggoed in (not 678)
                section3 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC3", "1119", "01", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses, true);
                section3.TermId = "2012/FA";
                section3.EndDate = new DateTime(2012, 12, 21);
                section3.LearningProvider = "Ellucian";
                section3.AddActiveStudent("student-1");
                section3.AddActiveStudent("student-2");
                section3.AddFaculty("0000679");


                var sections = new List<Domain.Student.Entities.Section>();
                sections.Add(section1);
                sections.Add(section2);
                sections.Add(section3);


                List<string> sec1List = new List<string>() { "SEC1" };
                List<Domain.Student.Entities.Section> sec1Resp = new List<Domain.Student.Entities.Section>() { section1 };
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(sec1List, false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sec1Resp));

                List<Domain.Student.Entities.Section> sec2Resp = new List<Domain.Student.Entities.Section>() { section2 };
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(new List<string>() { "SEC2" }, false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sec2Resp));


                List<Domain.Student.Entities.Section> sec3Resp = new List<Domain.Student.Entities.Section>() { section3 };
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(new List<string>() { "SEC3" }, false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sec3Resp));


                List<string> sec4List = new List<string>() { "9999999" };
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(sec4List, false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(new List<Domain.Student.Entities.Section>()));

                // Mock the section service
                sectionCoordinationService = new SectionCoordinationService(adapterRegistry, sectionRepo, courseRepo, studentRepo, studentReferenceDataRepo,
                    referenceDataRepo, termRepo, studentConfigRepo, configRepo, personRepo, roomRepo, eventRepo, bookRepo, currentUserFactory, roleRepo, logger);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetSection_SectionId_As_Null()
            {
                await sectionCoordinationService.GetSection2Async(null, true);

            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetSection_SectionId_As_Empty()
            {
                await sectionCoordinationService.GetSection2Async("9999999", true);

            }

            [TestMethod]
            public async Task GetSection_With_Student_User()
            {
                PrivacyWrapper<Dtos.Student.Section2> section = await sectionCoordinationService.GetSection2Async("SEC1", true);
                Assert.IsTrue(section.HasPrivacyRestrictions);
                Assert.AreEqual(0, section.Dto.ActiveStudentIds.Count());
            }

            [TestMethod]
            public async Task GetSection_With_Faculty_User_And_Assigned_To_Section()
            {
                currentUserFactory = new CurrentUserSetup.FacultytUserFactory();
                // Mock the section service
                sectionCoordinationService = new SectionCoordinationService(adapterRegistry, sectionRepo, courseRepo, studentRepo, studentReferenceDataRepo,
                    referenceDataRepo, termRepo, studentConfigRepo, configRepo, personRepo, roomRepo, eventRepo, bookRepo, currentUserFactory, roleRepo, logger);
                PrivacyWrapper<Dtos.Student.Section2> section = await sectionCoordinationService.GetSection2Async("SEC1", true);
                Assert.IsFalse(section.HasPrivacyRestrictions);
                Assert.IsNotNull(section.Dto.ActiveStudentIds);
                Assert.AreEqual(2, section.Dto.ActiveStudentIds.Count());
            }

            [TestMethod]
            public async Task GetSection_With_Null_Faculty_Assigned_To_Section()
            {
                currentUserFactory = new CurrentUserSetup.FacultytUserFactory();
                // Mock the section service
                sectionCoordinationService = new SectionCoordinationService(adapterRegistry, sectionRepo, courseRepo, studentRepo, studentReferenceDataRepo,
                    referenceDataRepo, termRepo, studentConfigRepo, configRepo, personRepo, roomRepo, eventRepo, bookRepo, currentUserFactory, roleRepo, logger);
                PrivacyWrapper<Dtos.Student.Section2> section = await sectionCoordinationService.GetSection2Async("SEC2", true);
                Assert.IsTrue(section.HasPrivacyRestrictions);
                Assert.AreEqual(0, section.Dto.ActiveStudentIds.Count());

            }


            [TestMethod]
            public async Task GetSection_With_Faculty_Assigned_To_Section_But_Not_same_As_loggedIn()
            {
                currentUserFactory = new CurrentUserSetup.FacultytUserFactory();
                // Mock the section service
                sectionCoordinationService = new SectionCoordinationService(adapterRegistry, sectionRepo, courseRepo, studentRepo, studentReferenceDataRepo,
                    referenceDataRepo, termRepo, studentConfigRepo, configRepo, personRepo, roomRepo, eventRepo, bookRepo, currentUserFactory, roleRepo, logger);
                PrivacyWrapper<Dtos.Student.Section2> section = await sectionCoordinationService.GetSection2Async("SEC3", true);
                Assert.IsTrue(section.HasPrivacyRestrictions);
                Assert.AreEqual(0, section.Dto.ActiveStudentIds.Count());

            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionRepo = null;
                studentRepo = null;
                roleRepo = null;
                adapterRegistry = null;
                sectionCoordinationService = null;
            }

        }

        [TestClass]
        public class GetSection_V3 : CurrentUserSetup
        {
            private SectionCoordinationService sectionCoordinationService;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IStudentReferenceDataRepository> stuRefDataRepoMock;
            private IStudentReferenceDataRepository studentReferenceDataRepo;
            private Mock<IReferenceDataRepository> referenceDataRepoMock;
            private IReferenceDataRepository referenceDataRepo;
            private Mock<ICourseRepository> courseRepoMock;
            private ICourseRepository courseRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IStudentConfigurationRepository> studentConfigRepoMock;
            private IStudentConfigurationRepository studentConfigRepo;
            private Mock<IConfigurationRepository> configRepoMock;
            private IConfigurationRepository configRepo;
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IRoomRepository> roomRepoMock;
            private IRoomRepository roomRepo;
            private Mock<IEventRepository> eventRepoMock;
            private IEventRepository eventRepo;
            private Mock<IBookRepository> bookRepoMock;
            private IBookRepository bookRepo;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ILogger logger;
            private ICurrentUserFactory currentUserFactory;

            private Domain.Student.Entities.Section section1;
            private Domain.Student.Entities.Section section2;
            private Domain.Student.Entities.Section section3;

            private List<OfferingDepartment> dpts = new List<OfferingDepartment>() { new OfferingDepartment("ART", 100m) };
            private List<string> levels = new List<string>() { "Whatever" };
            private List<SectionStatusItem> statuses = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-60)) };

            [TestInitialize]
            public void Initialize()
            {
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                roomRepoMock = new Mock<IRoomRepository>();
                roomRepo = roomRepoMock.Object;
                stuRefDataRepoMock = new Mock<IStudentReferenceDataRepository>();
                studentReferenceDataRepo = stuRefDataRepoMock.Object;
                referenceDataRepoMock = new Mock<IReferenceDataRepository>();
                referenceDataRepo = referenceDataRepoMock.Object;
                courseRepoMock = new Mock<ICourseRepository>();
                courseRepo = courseRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                studentConfigRepoMock = new Mock<IStudentConfigurationRepository>();
                studentConfigRepo = studentConfigRepoMock.Object;
                configRepoMock = new Mock<IConfigurationRepository>();
                configRepo = configRepoMock.Object;
                eventRepoMock = new Mock<IEventRepository>();
                eventRepo = eventRepoMock.Object;
                bookRepoMock = new Mock<IBookRepository>();
                bookRepo = bookRepoMock.Object;

                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;

                // "STU1" is the current user
                currentUserFactory = new CurrentUserSetup.StudentUserFactory();

                logger = new Mock<ILogger>().Object;

                var sectionAdapter = new AutoMapperAdapter<Domain.Student.Entities.Section, Dtos.Student.Section3>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.Section, Dtos.Student.Section3>()).Returns(sectionAdapter);


                // section have faculty same as logged in user
                section1 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC1", "1119", "01", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses, true);
                section1.TermId = "2012/FA";
                section1.EndDate = new DateTime(2012, 12, 21);
                section1.LearningProvider = "Ellucian";
                section1.AddActiveStudent("student-1");
                section1.AddActiveStudent("student-2");
                section1.AddFaculty("0000678");

                //section with no faculty assigned
                section2 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC2", "1119", "01", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses, true);
                section2.TermId = "2012/FA";
                section2.EndDate = new DateTime(2012, 12, 21);
                section2.LearningProvider = "Ellucian";
                section2.AddActiveStudent("student-1");
                section2.AddActiveStudent("student-2");


                //section with  faculty assigned but not the same faculty as lggoed in (not 678)
                section3 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC3", "1119", "01", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses, true);
                section3.TermId = "2012/FA";
                section3.EndDate = new DateTime(2012, 12, 21);
                section3.LearningProvider = "Ellucian";
                section3.AddActiveStudent("student-1");
                section3.AddActiveStudent("student-2");
                section3.AddFaculty("0000679");


                var sections = new List<Domain.Student.Entities.Section>();
                sections.Add(section1);
                sections.Add(section2);
                sections.Add(section3);


                List<string> sec1List = new List<string>() { "SEC1" };
                List<Domain.Student.Entities.Section> sec1Resp = new List<Domain.Student.Entities.Section>() { section1 };
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(sec1List, false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sec1Resp));

                List<Domain.Student.Entities.Section> sec2Resp = new List<Domain.Student.Entities.Section>() { section2 };
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(new List<string>() { "SEC2" }, false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sec2Resp));


                List<Domain.Student.Entities.Section> sec3Resp = new List<Domain.Student.Entities.Section>() { section3 };
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(new List<string>() { "SEC3" }, false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sec3Resp));


                List<string> sec4List = new List<string>() { "9999999" };
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(sec4List, false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(new List<Domain.Student.Entities.Section>()));

                // Mock the section service
                sectionCoordinationService = new SectionCoordinationService(adapterRegistry, sectionRepo, courseRepo, studentRepo, studentReferenceDataRepo,
                    referenceDataRepo, termRepo, studentConfigRepo, configRepo, personRepo, roomRepo, eventRepo, bookRepo, currentUserFactory, roleRepo, logger);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetSection_SectionId_As_Null()
            {
                await sectionCoordinationService.GetSection3Async(null, true);

            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetSection_SectionId_As_Empty()
            {
                await sectionCoordinationService.GetSection3Async("9999999", true);

            }

            [TestMethod]
            public async Task GetSection_With_Student_User()
            {
                PrivacyWrapper<Dtos.Student.Section3> section = await sectionCoordinationService.GetSection3Async("SEC1", true);
                Assert.IsTrue(section.HasPrivacyRestrictions);
                Assert.AreEqual(0, section.Dto.ActiveStudentIds.Count());
            }

            [TestMethod]
            public async Task GetSection_With_Faculty_User_And_Assigned_To_Section()
            {
                currentUserFactory = new CurrentUserSetup.FacultytUserFactory();
                // Mock the section service
                sectionCoordinationService = new SectionCoordinationService(adapterRegistry, sectionRepo, courseRepo, studentRepo, studentReferenceDataRepo,
                    referenceDataRepo, termRepo, studentConfigRepo, configRepo, personRepo, roomRepo, eventRepo, bookRepo, currentUserFactory, roleRepo, logger);
                PrivacyWrapper<Dtos.Student.Section3> section = await sectionCoordinationService.GetSection3Async("SEC1", true);
                Assert.IsFalse(section.HasPrivacyRestrictions);
                Assert.IsNotNull(section.Dto.ActiveStudentIds);
                Assert.AreEqual(2, section.Dto.ActiveStudentIds.Count());
            }

            [TestMethod]
            public async Task GetSection_With_Null_Faculty_Assigned_To_Section()
            {
                currentUserFactory = new CurrentUserSetup.FacultytUserFactory();
                // Mock the section service
                sectionCoordinationService = new SectionCoordinationService(adapterRegistry, sectionRepo, courseRepo, studentRepo, studentReferenceDataRepo,
                    referenceDataRepo, termRepo, studentConfigRepo, configRepo, personRepo, roomRepo, eventRepo, bookRepo, currentUserFactory, roleRepo, logger);
                PrivacyWrapper<Dtos.Student.Section3> section = await sectionCoordinationService.GetSection3Async("SEC2", true);
                Assert.IsTrue(section.HasPrivacyRestrictions);
                Assert.AreEqual(0, section.Dto.ActiveStudentIds.Count());

            }


            [TestMethod]
            public async Task GetSection_With_Faculty_Assigned_To_Section_But_Not_same_As_loggedIn()
            {
                currentUserFactory = new CurrentUserSetup.FacultytUserFactory();
                // Mock the section service
                sectionCoordinationService = new SectionCoordinationService(adapterRegistry, sectionRepo, courseRepo, studentRepo, studentReferenceDataRepo,
                    referenceDataRepo, termRepo, studentConfigRepo, configRepo, personRepo, roomRepo, eventRepo, bookRepo, currentUserFactory, roleRepo, logger);
                PrivacyWrapper<Dtos.Student.Section3> section = await sectionCoordinationService.GetSection3Async("SEC3", true);
                Assert.IsTrue(section.HasPrivacyRestrictions);
                Assert.AreEqual(0, section.Dto.ActiveStudentIds.Count());

            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionRepo = null;
                studentRepo = null;
                roleRepo = null;
                adapterRegistry = null;
                sectionCoordinationService = null;
            }

        }

        [TestClass]
        public class GetSection_V4 : CurrentUserSetup
        {
            private SectionCoordinationService sectionCoordinationService;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IStudentReferenceDataRepository> stuRefDataRepoMock;
            private IStudentReferenceDataRepository studentReferenceDataRepo;
            private Mock<IReferenceDataRepository> referenceDataRepoMock;
            private IReferenceDataRepository referenceDataRepo;
            private Mock<ICourseRepository> courseRepoMock;
            private ICourseRepository courseRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IStudentConfigurationRepository> studentConfigRepoMock;
            private IStudentConfigurationRepository studentConfigRepo;
            private Mock<IConfigurationRepository> configRepoMock;
            private IConfigurationRepository configRepo;
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IRoomRepository> roomRepoMock;
            private IRoomRepository roomRepo;
            private Mock<IEventRepository> eventRepoMock;
            private IEventRepository eventRepo;
            private Mock<IBookRepository> bookRepoMock;
            private IBookRepository bookRepo;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ILogger logger;
            private ICurrentUserFactory currentUserFactory;

            private Domain.Student.Entities.Section section1;
            private Domain.Student.Entities.Section section2;
            private Domain.Student.Entities.Section section3;
            private Domain.Student.Entities.Section section4;
            private Domain.Student.Entities.Section section5;

            private List<OfferingDepartment> dpts = new List<OfferingDepartment>() { new OfferingDepartment("ART", 100m) };
            private List<string> levels = new List<string>() { "Whatever" };
            private List<SectionStatusItem> statuses = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-60)) };

            [TestInitialize]
            public void Initialize()
            {
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                roomRepoMock = new Mock<IRoomRepository>();
                roomRepo = roomRepoMock.Object;
                stuRefDataRepoMock = new Mock<IStudentReferenceDataRepository>();
                studentReferenceDataRepo = stuRefDataRepoMock.Object;
                referenceDataRepoMock = new Mock<IReferenceDataRepository>();
                referenceDataRepo = referenceDataRepoMock.Object;
                courseRepoMock = new Mock<ICourseRepository>();
                courseRepo = courseRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                studentConfigRepoMock = new Mock<IStudentConfigurationRepository>();
                studentConfigRepo = studentConfigRepoMock.Object;
                configRepoMock = new Mock<IConfigurationRepository>();
                configRepo = configRepoMock.Object;
                eventRepoMock = new Mock<IEventRepository>();
                eventRepo = eventRepoMock.Object;
                bookRepoMock = new Mock<IBookRepository>();
                bookRepo = bookRepoMock.Object;

                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;

                // "STU1" is the current user
                currentUserFactory = new CurrentUserSetup.StudentUserFactory();

                logger = new Mock<ILogger>().Object;

                var sectionAdapter = new AutoMapperAdapter<Domain.Student.Entities.Section, Dtos.Student.Section3>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.Section, Dtos.Student.Section3>()).Returns(sectionAdapter);


                // section have faculty same as logged in user
                //No census cert exist. so it will be empty collection
                section1 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC1", "1119", "01", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses, true);
                section1.TermId = "2012/FA";
                section1.EndDate = new DateTime(2012, 12, 21);
                section1.LearningProvider = "Ellucian";
                section1.AddActiveStudent("student-1");
                section1.AddActiveStudent("student-2");
                section1.AddFaculty("0000678");                

                //section with no faculty assigned
                section2 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC2", "1119", "01", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses, true);
                section2.TermId = "2012/FA";
                section2.EndDate = new DateTime(2012, 12, 21);
                section2.LearningProvider = "Ellucian";
                section2.AddActiveStudent("student-1");
                section2.AddActiveStudent("student-2");

                section2.AddSectionCensusCertification(new Domain.Student.Entities.SectionCensusCertification(new DateTime(2021, 01, 02), "1", "after 1 month", new DateTime(2021, 02, 03), new DateTime(2021, 02, 03, 06, 03, 02), "personId"));


                //section with  faculty assigned but not the same faculty as logged in (not 678)
                section3 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC3", "1119", "01", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses, true);
                section3.TermId = "2012/FA";
                section3.EndDate = new DateTime(2012, 12, 21);
                section3.LearningProvider = "Ellucian";
                section3.AddActiveStudent("student-1");
                section3.AddActiveStudent("student-2");
                section3.AddFaculty("0000679");
                section3.AddSectionCensusCertification(new Domain.Student.Entities.SectionCensusCertification(new DateTime(2021, 01, 02), string.Empty, string.Empty, new DateTime(2021, 02, 03), new DateTime(2021, 02, 03, 06, 03, 02), string.Empty));
                section3.AddSectionCensusCertification(new Domain.Student.Entities.SectionCensusCertification(new DateTime(2021, 01, 02), null, null, null, null, null));
                section3.ShowDropRoster = true;


                // section with Attendance Tracking Type = PresentAbsentWithoutSectionMeeting
                section4 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC4", "1119", "01", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses, true);
                section4.AttendanceTrackingType = Domain.Student.Entities.AttendanceTrackingType.PresentAbsentWithoutSectionMeeting;
                section4.LearningProvider = "Ellucian";
                section4.AddActiveStudent("student-1");
                section4.AddActiveStudent("student-2");
                section4.AddFaculty("0000679");
                section4.ShowDropRoster = false;

                //section with  faculty assigned 
                section5 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC5", "1119", "01", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses, true);
                section5.TermId = "2012/FA";
                section5.EndDate = new DateTime(2012, 12, 21);
                section5.LearningProvider = "Ellucian";
                section5.AddActiveStudent("student-1");
                section5.AddActiveStudent("student-2");
                section5.AddFaculty("0000678");
                section5.AddFaculty("0000680");
                section5.AddSectionFaculty(new Domain.Student.Entities.SectionFaculty("0000678", "SEC5", "0000678", "Lec", DateTime.Today, DateTime.Today, 0) { });
                section5.AddSectionFaculty(new Domain.Student.Entities.SectionFaculty("0000680", "SEC5", "0000680", "Lab", DateTime.Today, DateTime.Today, 0) { });
              

                var sections = new List<Domain.Student.Entities.Section>();
                sections.Add(section1);
                sections.Add(section2);
                sections.Add(section3);

                List<string> sec1List = new List<string>() { "SEC1" };
                List<Domain.Student.Entities.Section> sec1Resp = new List<Domain.Student.Entities.Section>() { section1 };
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(sec1List, false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sec1Resp));

                List<Domain.Student.Entities.Section> sec2Resp = new List<Domain.Student.Entities.Section>() { section2 };
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(new List<string>() { "SEC2" }, false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sec2Resp));

                List<Domain.Student.Entities.Section> sec3Resp = new List<Domain.Student.Entities.Section>() { section3 };
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(new List<string>() { "SEC3" }, false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sec3Resp));

                List<Domain.Student.Entities.Section> sec4Resp = new List<Domain.Student.Entities.Section>() { section4 };
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(new List<string>() { "SEC4" }, false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sec4Resp));

                List<Domain.Student.Entities.Section> sec5Resp = new List<Domain.Student.Entities.Section>() { section5 };
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(new List<string>() { "SEC5" }, false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sec5Resp));

                List<string> sec4List = new List<string>() { "9999999" };
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(sec4List, false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(new List<Domain.Student.Entities.Section>()));

                // Mock the section service
                sectionCoordinationService = new SectionCoordinationService(adapterRegistry, sectionRepo, courseRepo, studentRepo, studentReferenceDataRepo,
                    referenceDataRepo, termRepo, studentConfigRepo, configRepo, personRepo, roomRepo, eventRepo, bookRepo, currentUserFactory, roleRepo, logger);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetSection4_SectionId_As_Null()
            {
                await sectionCoordinationService.GetSection4Async(null, true);

            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetSection4_SectionId_As_Empty()
            {
                await sectionCoordinationService.GetSection4Async("9999999", true);

            }

            [TestMethod]
            public async Task GetSection4_With_Student_User()
            {
                PrivacyWrapper<Dtos.Student.Section4> section = await sectionCoordinationService.GetSection4Async("SEC1", true);
                Assert.IsTrue(section.HasPrivacyRestrictions);
                Assert.AreEqual(0, section.Dto.ActiveStudentIds.Count());
            }

            [TestMethod]
            public async Task GetSection4_With_Faculty_User_And_Assigned_To_Section()
            {
                currentUserFactory = new CurrentUserSetup.FacultytUserFactory();
                // Mock the section service
                sectionCoordinationService = new SectionCoordinationService(adapterRegistry, sectionRepo, courseRepo, studentRepo, studentReferenceDataRepo,
                    referenceDataRepo, termRepo, studentConfigRepo, configRepo, personRepo, roomRepo, eventRepo, bookRepo, currentUserFactory, roleRepo, logger);
                PrivacyWrapper<Dtos.Student.Section4> section = await sectionCoordinationService.GetSection4Async("SEC1", true);
                Assert.IsFalse(section.HasPrivacyRestrictions);
                Assert.IsNotNull(section.Dto.ActiveStudentIds);
                Assert.AreEqual(2, section.Dto.ActiveStudentIds.Count());
            }

            [TestMethod]
            public async Task GetSection4_With_Null_Faculty_Assigned_To_Section()
            {
                currentUserFactory = new CurrentUserSetup.FacultytUserFactory();
                // Mock the section service
                sectionCoordinationService = new SectionCoordinationService(adapterRegistry, sectionRepo, courseRepo, studentRepo, studentReferenceDataRepo,
                    referenceDataRepo, termRepo, studentConfigRepo, configRepo, personRepo, roomRepo, eventRepo, bookRepo, currentUserFactory, roleRepo, logger);
                PrivacyWrapper<Dtos.Student.Section4> section = await sectionCoordinationService.GetSection4Async("SEC2", true);
                Assert.IsTrue(section.HasPrivacyRestrictions);
                Assert.AreEqual(0, section.Dto.ActiveStudentIds.Count());

            }

            [TestMethod]
            public async Task GetSection4_With_Faculty_Assigned_To_Section_But_Not_same_As_loggedIn()
            {
                currentUserFactory = new CurrentUserSetup.FacultytUserFactory();
                // Mock the section service
                sectionCoordinationService = new SectionCoordinationService(adapterRegistry, sectionRepo, courseRepo, studentRepo, studentReferenceDataRepo,
                    referenceDataRepo, termRepo, studentConfigRepo, configRepo, personRepo, roomRepo, eventRepo, bookRepo, currentUserFactory, roleRepo, logger);
                PrivacyWrapper<Dtos.Student.Section4> section = await sectionCoordinationService.GetSection4Async("SEC3", true);
                Assert.IsTrue(section.HasPrivacyRestrictions);
                Assert.AreEqual(0, section.Dto.ActiveStudentIds.Count());
            }

            [TestMethod]
            public async Task GetSection4_With_SectionFaculty()
            {
                currentUserFactory = new CurrentUserSetup.FacultytUserFactory();
                // Mock the section service
                sectionCoordinationService = new SectionCoordinationService(adapterRegistry, sectionRepo, courseRepo, studentRepo, studentReferenceDataRepo,
                    referenceDataRepo, termRepo, studentConfigRepo, configRepo, personRepo, roomRepo, eventRepo, bookRepo, currentUserFactory, roleRepo, logger);
                PrivacyWrapper<Dtos.Student.Section4> section = await sectionCoordinationService.GetSection4Async("SEC5", true);
                Assert.AreEqual(section.Dto.Faculty.Count, 2);
                Assert.AreEqual(section.Dto.Faculty[0].FacultyId, "0000678");
                Assert.AreEqual(section.Dto.Faculty[1].FacultyId, "0000680");
                Assert.AreEqual(section.Dto.Faculty[0].InstructionalMethodCode, "Lec");
                Assert.AreEqual(section.Dto.Faculty[1].InstructionalMethodCode, "Lab");

            }


            [TestMethod]
            public async Task GetSection4_AttendanceTrackingType_PresentAbsentWithoutSectionMeeting()
            {
                currentUserFactory = new CurrentUserSetup.FacultytUserFactory();
                // Mock the section service
                sectionCoordinationService = new SectionCoordinationService(adapterRegistry, sectionRepo, courseRepo, studentRepo, studentReferenceDataRepo,
                    referenceDataRepo, termRepo, studentConfigRepo, configRepo, personRepo, roomRepo, eventRepo, bookRepo, currentUserFactory, roleRepo, logger);
                PrivacyWrapper<Dtos.Student.Section4> section = await sectionCoordinationService.GetSection4Async("SEC4", true);
                Assert.AreEqual(Dtos.Student.AttendanceTrackingType2.PresentAbsentWithoutSectionMeeting, section.Dto.AttendanceTrackingType);
            }

            [TestMethod]
            public async Task GetSection4_ShowDropRoster_True()
            {
                currentUserFactory = new CurrentUserSetup.FacultytUserFactory();
                // Mock the section service
                sectionCoordinationService = new SectionCoordinationService(adapterRegistry, sectionRepo, courseRepo, studentRepo, studentReferenceDataRepo,
                    referenceDataRepo, termRepo, studentConfigRepo, configRepo, personRepo, roomRepo, eventRepo, bookRepo, currentUserFactory, roleRepo, logger);
                PrivacyWrapper<Dtos.Student.Section4> section = await sectionCoordinationService.GetSection4Async("SEC3", true);
                Assert.IsTrue(section.Dto.ShowDropRoster);
            }

            [TestMethod]
            public async Task GetSection4_ShowDropRoster_False()
            {
                currentUserFactory = new CurrentUserSetup.FacultytUserFactory();
                // Mock the section service
                sectionCoordinationService = new SectionCoordinationService(adapterRegistry, sectionRepo, courseRepo, studentRepo, studentReferenceDataRepo,
                    referenceDataRepo, termRepo, studentConfigRepo, configRepo, personRepo, roomRepo, eventRepo, bookRepo, currentUserFactory, roleRepo, logger);
                PrivacyWrapper<Dtos.Student.Section4> section = await sectionCoordinationService.GetSection4Async("SEC4", true);
                Assert.IsFalse(section.Dto.ShowDropRoster);
            }

            [TestMethod]
            public async Task GetSection4_SectionCertCensuses_Validate()
            {
                currentUserFactory = new CurrentUserSetup.FacultytUserFactory();
                // Mock the section service
                sectionCoordinationService = new SectionCoordinationService(adapterRegistry, sectionRepo, courseRepo, studentRepo, studentReferenceDataRepo,
                    referenceDataRepo, termRepo, studentConfigRepo, configRepo, personRepo, roomRepo, eventRepo, bookRepo, currentUserFactory, roleRepo, logger);
                PrivacyWrapper<Dtos.Student.Section4> section1Dto = await sectionCoordinationService.GetSection4Async("SEC1", true);
                Assert.AreEqual(0, section1Dto.Dto.SectionCertifiedCensuses.Count());

                PrivacyWrapper<Dtos.Student.Section4> section2Dto = await sectionCoordinationService.GetSection4Async("SEC2", true);
                Assert.AreEqual(1, section2Dto.Dto.SectionCertifiedCensuses.Count());
                Assert.AreEqual(section2.SectionCertifiedCensuses[0].CensusCertificationDate, section2Dto.Dto.SectionCertifiedCensuses.ToList()[0].CensusCertificationDate);
                Assert.AreEqual(section2.SectionCertifiedCensuses[0].CensusCertificationRecordedDate, section2Dto.Dto.SectionCertifiedCensuses.ToList()[0].CensusCertificationRecordedDate);
                Assert.AreEqual(section2.SectionCertifiedCensuses[0].CensusCertificationRecordedTime, section2Dto.Dto.SectionCertifiedCensuses.ToList()[0].CensusCertificationRecordedTime);
                Assert.AreEqual(section2.SectionCertifiedCensuses[0].CensusCertificationPosition, section2Dto.Dto.SectionCertifiedCensuses.ToList()[0].CensusCertificationPosition);
                Assert.AreEqual(section2.SectionCertifiedCensuses[0].CensusCertificationLabel, section2Dto.Dto.SectionCertifiedCensuses.ToList()[0].CensusCertificationLabel);

                PrivacyWrapper<Dtos.Student.Section4> section3Dto = await sectionCoordinationService.GetSection4Async("SEC3", true);
                Assert.AreEqual(2, section3Dto.Dto.SectionCertifiedCensuses.Count());
                Assert.AreEqual(section3.SectionCertifiedCensuses[0].CensusCertificationDate, section3Dto.Dto.SectionCertifiedCensuses.ToList()[0].CensusCertificationDate);
                Assert.AreEqual(section3.SectionCertifiedCensuses[0].CensusCertificationRecordedDate, section3Dto.Dto.SectionCertifiedCensuses.ToList()[0].CensusCertificationRecordedDate);
                Assert.AreEqual(section3.SectionCertifiedCensuses[0].CensusCertificationRecordedTime, section3Dto.Dto.SectionCertifiedCensuses.ToList()[0].CensusCertificationRecordedTime);
                Assert.AreEqual(section3.SectionCertifiedCensuses[0].CensusCertificationPosition, section3Dto.Dto.SectionCertifiedCensuses.ToList()[0].CensusCertificationPosition);
                Assert.AreEqual(section3.SectionCertifiedCensuses[0].CensusCertificationLabel, section3Dto.Dto.SectionCertifiedCensuses.ToList()[0].CensusCertificationLabel);

                Assert.AreEqual(section3.SectionCertifiedCensuses[1].CensusCertificationDate, section3Dto.Dto.SectionCertifiedCensuses.ToList()[1].CensusCertificationDate);
                Assert.AreEqual(section3.SectionCertifiedCensuses[1].CensusCertificationRecordedDate, section3Dto.Dto.SectionCertifiedCensuses.ToList()[1].CensusCertificationRecordedDate);
                Assert.AreEqual(section3.SectionCertifiedCensuses[1].CensusCertificationRecordedTime, section3Dto.Dto.SectionCertifiedCensuses.ToList()[1].CensusCertificationRecordedTime);
                Assert.AreEqual(section3.SectionCertifiedCensuses[1].CensusCertificationPosition, section3Dto.Dto.SectionCertifiedCensuses.ToList()[1].CensusCertificationPosition);
                Assert.AreEqual(section3.SectionCertifiedCensuses[1].CensusCertificationLabel, section3Dto.Dto.SectionCertifiedCensuses.ToList()[1].CensusCertificationLabel);


            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionRepo = null;
                studentRepo = null;
                roleRepo = null;
                adapterRegistry = null;
                sectionCoordinationService = null;
            }

        }


        [TestClass]
        public class GetSections_V1 : CurrentUserSetup
        {
            private SectionCoordinationService sectionCoordinationService;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IStudentReferenceDataRepository> stuRefDataRepoMock;
            private IStudentReferenceDataRepository studentReferenceDataRepo;
            private Mock<IReferenceDataRepository> referenceDataRepoMock;
            private IReferenceDataRepository referenceDataRepo;
            private Mock<ICourseRepository> courseRepoMock;
            private ICourseRepository courseRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IStudentConfigurationRepository> studentConfigRepoMock;
            private IStudentConfigurationRepository studentConfigRepo;
            private Mock<IConfigurationRepository> configRepoMock;
            private IConfigurationRepository configRepo;
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IRoomRepository> roomRepoMock;
            private IRoomRepository roomRepo;
            private Mock<IEventRepository> eventRepoMock;
            private IEventRepository eventRepo;
            private Mock<IBookRepository> bookRepoMock;
            private IBookRepository bookRepo;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ILogger logger;
            private ICurrentUserFactory currentUserFactory;

            private Domain.Student.Entities.Section section1;
            private Domain.Student.Entities.Section section2;
            private Domain.Student.Entities.Section section3;

            private List<OfferingDepartment> dpts = new List<OfferingDepartment>() { new OfferingDepartment("ART", 100m) };
            private List<string> levels = new List<string>() { "Whatever" };
            private List<SectionStatusItem> statuses = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-60)) };

            [TestInitialize]
            public void Initialize()
            {
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                roomRepoMock = new Mock<IRoomRepository>();
                roomRepo = roomRepoMock.Object;
                stuRefDataRepoMock = new Mock<IStudentReferenceDataRepository>();
                studentReferenceDataRepo = stuRefDataRepoMock.Object;
                referenceDataRepoMock = new Mock<IReferenceDataRepository>();
                referenceDataRepo = referenceDataRepoMock.Object;
                courseRepoMock = new Mock<ICourseRepository>();
                courseRepo = courseRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                studentConfigRepoMock = new Mock<IStudentConfigurationRepository>();
                studentConfigRepo = studentConfigRepoMock.Object;
                configRepoMock = new Mock<IConfigurationRepository>();
                configRepo = configRepoMock.Object;
                eventRepoMock = new Mock<IEventRepository>();
                eventRepo = eventRepoMock.Object;
                bookRepoMock = new Mock<IBookRepository>();
                bookRepo = bookRepoMock.Object;

                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;

                // "STU1" is the current user
                currentUserFactory = new CurrentUserSetup.StudentUserFactory();

                logger = new Mock<ILogger>().Object;

                var sectionAdapter = new AutoMapperAdapter<Domain.Student.Entities.Section, Dtos.Student.Section>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.Section, Dtos.Student.Section>()).Returns(sectionAdapter);


                // Mock the section repo response
                section1 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC1", "1119", "01", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses, true);
                section1.TermId = "2012/FA";
                section1.EndDate = new DateTime(2012, 12, 21);
                section1.LearningProvider = "Ellucian";
                section1.AddActiveStudent("student-1");
                section1.AddActiveStudent("student-2");
                section1.AddFaculty("0000678");

                //section with no faculty assigned
                section2 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC2", "1119", "01", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses, true);
                section2.TermId = "2012/FA";
                section2.EndDate = new DateTime(2012, 12, 21);
                section2.LearningProvider = "Ellucian";
                section2.AddActiveStudent("student-1");
                section2.AddActiveStudent("student-2");


                //section with  faculty assigned but not the same faculty as lggoed in (not 678)
                section3 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC3", "1119", "01", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses, true);
                section3.TermId = "2012/FA";
                section3.EndDate = new DateTime(2012, 12, 21);
                section3.LearningProvider = "Ellucian";
                section3.AddActiveStudent("student-1");
                section3.AddActiveStudent("student-2");
                section3.AddFaculty("0000679");


                var sections = new List<Domain.Student.Entities.Section>();
                sections.Add(section1);
                sections.Add(section2);
                sections.Add(section3);


                List<string> sec1List = new List<string>() { "SEC1" };
                List<Domain.Student.Entities.Section> sec1Resp = new List<Domain.Student.Entities.Section>() { section1 };
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(sec1List, false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sec1Resp));

                List<string> sec1And2List = new List<string>() { "SEC1", "SEC2" };
                List<Domain.Student.Entities.Section> sec1And2Resp = new List<Domain.Student.Entities.Section>() { section1, section2 };
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(sec1And2List, false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sec1And2Resp));


                List<string> sec4List = new List<string>() { "9999999" };
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(sec4List, false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(new List<Domain.Student.Entities.Section>()));

                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(sec4List, false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(new List<Domain.Student.Entities.Section>()));


                // Mock the section service
                sectionCoordinationService = new SectionCoordinationService(adapterRegistry, sectionRepo, courseRepo, studentRepo, studentReferenceDataRepo,
                    referenceDataRepo, termRepo, studentConfigRepo, configRepo, personRepo, roomRepo, eventRepo, bookRepo, currentUserFactory, roleRepo, logger);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetSections_SectionIdList_As_Null()
            {
                await sectionCoordinationService.GetSectionsAsync(null, true);

            }

            [TestMethod]
            public async Task GetSection_SectionId_As_Empty()
            {
                PrivacyWrapper<List<Dtos.Student.Section>> section = await sectionCoordinationService.GetSectionsAsync(new List<string>() { "9999999" }, true);
                Assert.IsFalse(section.HasPrivacyRestrictions);
                Assert.AreEqual(0, section.Dto.Count());
            }

            [TestMethod]
            public async Task GetSection_With_Student_User()
            {
                PrivacyWrapper<List<Dtos.Student.Section>> section = await sectionCoordinationService.GetSectionsAsync(new List<string>() { "SEC1", "SEC2" }, true);
                Assert.IsTrue(section.HasPrivacyRestrictions);
                Assert.IsNotNull(section.Dto);
                Assert.AreEqual(2, section.Dto.Count());
                Assert.AreEqual(0, section.Dto[0].ActiveStudentIds.Count());
                Assert.AreEqual(0, section.Dto[1].ActiveStudentIds.Count());
            }

            [TestMethod]
            public async Task GetSection_With_Faculty_User_And_Assigned_To_Section()
            {
                currentUserFactory = new CurrentUserSetup.FacultytUserFactory();
                // Mock the section service
                sectionCoordinationService = new SectionCoordinationService(adapterRegistry, sectionRepo, courseRepo, studentRepo, studentReferenceDataRepo,
                    referenceDataRepo, termRepo, studentConfigRepo, configRepo, personRepo, roomRepo, eventRepo, bookRepo, currentUserFactory, roleRepo, logger);
                PrivacyWrapper<List<Dtos.Student.Section>> section = await sectionCoordinationService.GetSectionsAsync(new List<string>() { "SEC1", "SEC2" }, true);
                Assert.IsTrue(section.HasPrivacyRestrictions);
                Assert.IsNotNull(section.Dto);
                Assert.AreEqual(2, section.Dto.Count());
                Assert.AreEqual(2, section.Dto[0].ActiveStudentIds.Count());
                Assert.AreEqual(0, section.Dto[1].ActiveStudentIds.Count());
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionRepo = null;
                studentRepo = null;
                roleRepo = null;
                adapterRegistry = null;
                sectionCoordinationService = null;
            }

        }

        [TestClass]
        public class GetSections_V2 : CurrentUserSetup
        {
            private SectionCoordinationService sectionCoordinationService;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IStudentReferenceDataRepository> stuRefDataRepoMock;
            private IStudentReferenceDataRepository studentReferenceDataRepo;
            private Mock<IReferenceDataRepository> referenceDataRepoMock;
            private IReferenceDataRepository referenceDataRepo;
            private Mock<ICourseRepository> courseRepoMock;
            private ICourseRepository courseRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IStudentConfigurationRepository> studentConfigRepoMock;
            private IStudentConfigurationRepository studentConfigRepo;
            private Mock<IConfigurationRepository> configRepoMock;
            private IConfigurationRepository configRepo;
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IRoomRepository> roomRepoMock;
            private IRoomRepository roomRepo;
            private Mock<IEventRepository> eventRepoMock;
            private IEventRepository eventRepo;
            private Mock<IBookRepository> bookRepoMock;
            private IBookRepository bookRepo;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ILogger logger;
            private ICurrentUserFactory currentUserFactory;

            private Domain.Student.Entities.Section section1;
            private Domain.Student.Entities.Section section2;
            private Domain.Student.Entities.Section section3;

            private List<OfferingDepartment> dpts = new List<OfferingDepartment>() { new OfferingDepartment("ART", 100m) };
            private List<string> levels = new List<string>() { "Whatever" };
            private List<SectionStatusItem> statuses = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-60)) };

            [TestInitialize]
            public void Initialize()
            {
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                roomRepoMock = new Mock<IRoomRepository>();
                roomRepo = roomRepoMock.Object;
                stuRefDataRepoMock = new Mock<IStudentReferenceDataRepository>();
                studentReferenceDataRepo = stuRefDataRepoMock.Object;
                referenceDataRepoMock = new Mock<IReferenceDataRepository>();
                referenceDataRepo = referenceDataRepoMock.Object;
                courseRepoMock = new Mock<ICourseRepository>();
                courseRepo = courseRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                studentConfigRepoMock = new Mock<IStudentConfigurationRepository>();
                studentConfigRepo = studentConfigRepoMock.Object;
                configRepoMock = new Mock<IConfigurationRepository>();
                configRepo = configRepoMock.Object;
                eventRepoMock = new Mock<IEventRepository>();
                eventRepo = eventRepoMock.Object;
                bookRepoMock = new Mock<IBookRepository>();
                bookRepo = bookRepoMock.Object;

                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;

                // "STU1" is the current user
                currentUserFactory = new CurrentUserSetup.StudentUserFactory();

                logger = new Mock<ILogger>().Object;

                var sectionAdapter = new AutoMapperAdapter<Domain.Student.Entities.Section, Dtos.Student.Section2>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.Section, Dtos.Student.Section2>()).Returns(sectionAdapter);


                // Mock the section repo response
                section1 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC1", "1119", "01", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses, true);
                section1.TermId = "2012/FA";
                section1.EndDate = new DateTime(2012, 12, 21);
                section1.LearningProvider = "Ellucian";
                section1.AddActiveStudent("student-1");
                section1.AddActiveStudent("student-2");
                section1.AddFaculty("0000678");

                //section with no faculty assigned
                section2 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC2", "1119", "01", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses, true);
                section2.TermId = "2012/FA";
                section2.EndDate = new DateTime(2012, 12, 21);
                section2.LearningProvider = "Ellucian";
                section2.AddActiveStudent("student-1");
                section2.AddActiveStudent("student-2");


                //section with  faculty assigned but not the same faculty as lggoed in (not 678)
                section3 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC3", "1119", "01", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses, true);
                section3.TermId = "2012/FA";
                section3.EndDate = new DateTime(2012, 12, 21);
                section3.LearningProvider = "Ellucian";
                section3.AddActiveStudent("student-1");
                section3.AddActiveStudent("student-2");
                section3.AddFaculty("0000679");


                var sections = new List<Domain.Student.Entities.Section>();
                sections.Add(section1);
                sections.Add(section2);
                sections.Add(section3);


                List<string> sec1List = new List<string>() { "SEC1" };
                List<Domain.Student.Entities.Section> sec1Resp = new List<Domain.Student.Entities.Section>() { section1 };
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(sec1List, false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sec1Resp));

                List<string> sec1And2List = new List<string>() { "SEC1", "SEC2" };
                List<Domain.Student.Entities.Section> sec1And2Resp = new List<Domain.Student.Entities.Section>() { section1, section2 };
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(sec1And2List, false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sec1And2Resp));


                List<string> sec4List = new List<string>() { "9999999" };
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(sec4List, false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(new List<Domain.Student.Entities.Section>()));

                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(sec4List, false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(new List<Domain.Student.Entities.Section>()));


                // Mock the section service
                sectionCoordinationService = new SectionCoordinationService(adapterRegistry, sectionRepo, courseRepo, studentRepo, studentReferenceDataRepo,
                    referenceDataRepo, termRepo, studentConfigRepo, configRepo, personRepo, roomRepo, eventRepo, bookRepo, currentUserFactory, roleRepo, logger);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetSections_SectionIdList_As_Null()
            {
                await sectionCoordinationService.GetSections2Async(null, true);

            }

            [TestMethod]
            public async Task GetSection_SectionId_As_Empty()
            {
                PrivacyWrapper<List<Dtos.Student.Section2>> section = await sectionCoordinationService.GetSections2Async(new List<string>() { "9999999" }, true);
                Assert.IsFalse(section.HasPrivacyRestrictions);
                Assert.AreEqual(0, section.Dto.Count());
            }

            [TestMethod]
            public async Task GetSection_With_Student_User()
            {
                PrivacyWrapper<List<Dtos.Student.Section2>> section = await sectionCoordinationService.GetSections2Async(new List<string>() { "SEC1", "SEC2" }, true);
                Assert.IsTrue(section.HasPrivacyRestrictions);
                Assert.IsNotNull(section.Dto);
                Assert.AreEqual(2, section.Dto.Count());
                Assert.AreEqual(0, section.Dto[0].ActiveStudentIds.Count());
                Assert.AreEqual(0, section.Dto[1].ActiveStudentIds.Count());
            }

            [TestMethod]
            public async Task GetSection_With_Faculty_User_And_Assigned_To_Section()
            {
                currentUserFactory = new CurrentUserSetup.FacultytUserFactory();
                // Mock the section service
                sectionCoordinationService = new SectionCoordinationService(adapterRegistry, sectionRepo, courseRepo, studentRepo, studentReferenceDataRepo,
                    referenceDataRepo, termRepo, studentConfigRepo, configRepo, personRepo, roomRepo, eventRepo, bookRepo, currentUserFactory, roleRepo, logger);
                PrivacyWrapper<List<Dtos.Student.Section2>> section = await sectionCoordinationService.GetSections2Async(new List<string>() { "SEC1", "SEC2" }, true);
                Assert.IsTrue(section.HasPrivacyRestrictions);
                Assert.IsNotNull(section.Dto);
                Assert.AreEqual(2, section.Dto.Count());
                Assert.AreEqual(2, section.Dto[0].ActiveStudentIds.Count());
                Assert.AreEqual(0, section.Dto[1].ActiveStudentIds.Count());
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionRepo = null;
                studentRepo = null;
                roleRepo = null;
                adapterRegistry = null;
                sectionCoordinationService = null;
            }

        }

        [TestClass]
        public class GetSections_V3 : CurrentUserSetup
        {
            private SectionCoordinationService sectionCoordinationService;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IStudentReferenceDataRepository> stuRefDataRepoMock;
            private IStudentReferenceDataRepository studentReferenceDataRepo;
            private Mock<IReferenceDataRepository> referenceDataRepoMock;
            private IReferenceDataRepository referenceDataRepo;
            private Mock<ICourseRepository> courseRepoMock;
            private ICourseRepository courseRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IStudentConfigurationRepository> studentConfigRepoMock;
            private IStudentConfigurationRepository studentConfigRepo;
            private Mock<IConfigurationRepository> configRepoMock;
            private IConfigurationRepository configRepo;
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IRoomRepository> roomRepoMock;
            private IRoomRepository roomRepo;
            private Mock<IEventRepository> eventRepoMock;
            private IEventRepository eventRepo;
            private Mock<IBookRepository> bookRepoMock;
            private IBookRepository bookRepo;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ILogger logger;
            private ICurrentUserFactory currentUserFactory;

            private Domain.Student.Entities.Section section1;
            private Domain.Student.Entities.Section section2;
            private Domain.Student.Entities.Section section3;

            private List<OfferingDepartment> dpts = new List<OfferingDepartment>() { new OfferingDepartment("ART", 100m) };
            private List<string> levels = new List<string>() { "Whatever" };
            private List<SectionStatusItem> statuses = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-60)) };

            [TestInitialize]
            public void Initialize()
            {
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                roomRepoMock = new Mock<IRoomRepository>();
                roomRepo = roomRepoMock.Object;
                stuRefDataRepoMock = new Mock<IStudentReferenceDataRepository>();
                studentReferenceDataRepo = stuRefDataRepoMock.Object;
                referenceDataRepoMock = new Mock<IReferenceDataRepository>();
                referenceDataRepo = referenceDataRepoMock.Object;
                courseRepoMock = new Mock<ICourseRepository>();
                courseRepo = courseRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                studentConfigRepoMock = new Mock<IStudentConfigurationRepository>();
                studentConfigRepo = studentConfigRepoMock.Object;
                configRepoMock = new Mock<IConfigurationRepository>();
                configRepo = configRepoMock.Object;
                eventRepoMock = new Mock<IEventRepository>();
                eventRepo = eventRepoMock.Object;
                bookRepoMock = new Mock<IBookRepository>();
                bookRepo = bookRepoMock.Object;

                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;

                // "STU1" is the current user
                currentUserFactory = new CurrentUserSetup.StudentUserFactory();

                logger = new Mock<ILogger>().Object;

                var sectionAdapter = new AutoMapperAdapter<Domain.Student.Entities.Section, Dtos.Student.Section3>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.Section, Dtos.Student.Section3>()).Returns(sectionAdapter);


                // Mock the section repo response
                section1 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC1", "1119", "01", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses, true);
                section1.TermId = "2012/FA";
                section1.EndDate = new DateTime(2012, 12, 21);
                section1.LearningProvider = "Ellucian";
                section1.AddActiveStudent("student-1");
                section1.AddActiveStudent("student-2");
                section1.AddFaculty("0000678");

                //section with no faculty assigned
                section2 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC2", "1119", "01", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses, true);
                section2.TermId = "2012/FA";
                section2.EndDate = new DateTime(2012, 12, 21);
                section2.LearningProvider = "Ellucian";
                section2.AddActiveStudent("student-1");
                section2.AddActiveStudent("student-2");


                //section with  faculty assigned but not the same faculty as lggoed in (not 678)
                section3 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC3", "1119", "01", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses, true);
                section3.TermId = "2012/FA";
                section3.EndDate = new DateTime(2012, 12, 21);
                section3.LearningProvider = "Ellucian";
                section3.AddActiveStudent("student-1");
                section3.AddActiveStudent("student-2");
                section3.AddFaculty("0000679");


                var sections = new List<Domain.Student.Entities.Section>();
                sections.Add(section1);
                sections.Add(section2);
                sections.Add(section3);


                List<string> sec1List = new List<string>() { "SEC1" };
                List<Domain.Student.Entities.Section> sec1Resp = new List<Domain.Student.Entities.Section>() { section1 };
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(sec1List, false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sec1Resp));

                List<string> sec1And2List = new List<string>() { "SEC1", "SEC2" };
                List<Domain.Student.Entities.Section> sec1And2Resp = new List<Domain.Student.Entities.Section>() { section1, section2 };
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(sec1And2List, false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sec1And2Resp));


                List<string> sec4List = new List<string>() { "9999999" };
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(sec4List, false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(new List<Domain.Student.Entities.Section>()));

                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(sec4List, false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(new List<Domain.Student.Entities.Section>()));


                // Mock the section service
                sectionCoordinationService = new SectionCoordinationService(adapterRegistry, sectionRepo, courseRepo, studentRepo, studentReferenceDataRepo,
                    referenceDataRepo, termRepo, studentConfigRepo, configRepo, personRepo, roomRepo, eventRepo, bookRepo, currentUserFactory, roleRepo, logger);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetSections_SectionIdList_As_Null()
            {
                await sectionCoordinationService.GetSections3Async(null, true);

            }

            [TestMethod]
            public async Task GetSection_SectionId_As_Empty()
            {
                PrivacyWrapper<List<Dtos.Student.Section3>> section = await sectionCoordinationService.GetSections3Async(new List<string>() { "9999999" }, true);
                Assert.IsFalse(section.HasPrivacyRestrictions);
                Assert.AreEqual(0, section.Dto.Count());
            }

            [TestMethod]
            public async Task GetSection_With_Student_User()
            {
                PrivacyWrapper<List<Dtos.Student.Section3>> section = await sectionCoordinationService.GetSections3Async(new List<string>() { "SEC1", "SEC2" }, true);
                Assert.IsTrue(section.HasPrivacyRestrictions);
                Assert.IsNotNull(section.Dto);
                Assert.AreEqual(2, section.Dto.Count());
                Assert.AreEqual(0, section.Dto[0].ActiveStudentIds.Count());
                Assert.AreEqual(0, section.Dto[1].ActiveStudentIds.Count());
            }

            [TestMethod]
            public async Task GetSection_With_Faculty_User_And_Assigned_To_Section()
            {
                currentUserFactory = new CurrentUserSetup.FacultytUserFactory();
                // Mock the section service
                sectionCoordinationService = new SectionCoordinationService(adapterRegistry, sectionRepo, courseRepo, studentRepo, studentReferenceDataRepo,
                    referenceDataRepo, termRepo, studentConfigRepo, configRepo, personRepo, roomRepo, eventRepo, bookRepo, currentUserFactory, roleRepo, logger);
                PrivacyWrapper<List<Dtos.Student.Section3>> section = await sectionCoordinationService.GetSections3Async(new List<string>() { "SEC1", "SEC2" }, true);
                Assert.IsTrue(section.HasPrivacyRestrictions);
                Assert.IsNotNull(section.Dto);
                Assert.AreEqual(2, section.Dto.Count());
                Assert.AreEqual(2, section.Dto[0].ActiveStudentIds.Count());
                Assert.AreEqual(0, section.Dto[1].ActiveStudentIds.Count());
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionRepo = null;
                studentRepo = null;
                roleRepo = null;
                adapterRegistry = null;
                sectionCoordinationService = null;
            }

        }

        [TestClass]
        public class GetSections_V4 : CurrentUserSetup
        {
            private SectionCoordinationService sectionCoordinationService;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IStudentReferenceDataRepository> stuRefDataRepoMock;
            private IStudentReferenceDataRepository studentReferenceDataRepo;
            private Mock<IReferenceDataRepository> referenceDataRepoMock;
            private IReferenceDataRepository referenceDataRepo;
            private Mock<ICourseRepository> courseRepoMock;
            private ICourseRepository courseRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IStudentConfigurationRepository> studentConfigRepoMock;
            private IStudentConfigurationRepository studentConfigRepo;
            private Mock<IConfigurationRepository> configRepoMock;
            private IConfigurationRepository configRepo;
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IRoomRepository> roomRepoMock;
            private IRoomRepository roomRepo;
            private Mock<IEventRepository> eventRepoMock;
            private IEventRepository eventRepo;
            private Mock<IBookRepository> bookRepoMock;
            private IBookRepository bookRepo;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ILogger logger;
            private ICurrentUserFactory currentUserFactory;

            private Domain.Student.Entities.Section section1;
            private Domain.Student.Entities.Section section2;
            private Domain.Student.Entities.Section section4;

            private List<OfferingDepartment> dpts = new List<OfferingDepartment>() { new OfferingDepartment("ART", 100m) };
            private List<string> levels = new List<string>() { "Whatever" };
            private List<SectionStatusItem> statuses = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-60)) };

            [TestInitialize]
            public void Initialize()
            {
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                roomRepoMock = new Mock<IRoomRepository>();
                roomRepo = roomRepoMock.Object;
                stuRefDataRepoMock = new Mock<IStudentReferenceDataRepository>();
                studentReferenceDataRepo = stuRefDataRepoMock.Object;
                referenceDataRepoMock = new Mock<IReferenceDataRepository>();
                referenceDataRepo = referenceDataRepoMock.Object;
                courseRepoMock = new Mock<ICourseRepository>();
                courseRepo = courseRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                studentConfigRepoMock = new Mock<IStudentConfigurationRepository>();
                studentConfigRepo = studentConfigRepoMock.Object;
                configRepoMock = new Mock<IConfigurationRepository>();
                configRepo = configRepoMock.Object;
                eventRepoMock = new Mock<IEventRepository>();
                eventRepo = eventRepoMock.Object;
                bookRepoMock = new Mock<IBookRepository>();
                bookRepo = bookRepoMock.Object;

                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;

                // "STU1" is the current user
                currentUserFactory = new CurrentUserSetup.StudentUserFactory();

                logger = new Mock<ILogger>().Object;

                var sectionAdapter = new Coordination.Student.Adapters.SectionEntityToStudentSection4DtoAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.Section, Dtos.Student.Section4>()).Returns(sectionAdapter);


                // Mock the section repo response
                section1 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC1", "1119", "01", new DateTime(2012, 09, 01), 4.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses, true);
                section1.TermId = "2012/FA";
                section1.EndDate = new DateTime(2012, 12, 21);
                section1.LearningProvider = "Ellucian";
                section1.AddActiveStudent("student-1");
                section1.AddActiveStudent("student-2");
                section1.AddFaculty("0000678");

                //section with no faculty assigned
                section2 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC2", "1119", "01", new DateTime(2012, 09, 01), 4.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses, true);
                section2.TermId = "2012/FA";
                section2.EndDate = new DateTime(2012, 12, 21);
                section2.LearningProvider = "Ellucian";
                section2.AddActiveStudent("student-1");
                section2.AddActiveStudent("student-2");


                //section with  faculty assigned but not the same faculty as lggoed in (not 678)
                section4 = new Ellucian.Colleague.Domain.Student.Entities.Section("SEC4", "1119", "01", new DateTime(2012, 09, 01), 4.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses, true);
                section4.TermId = "2012/FA";
                section4.EndDate = new DateTime(2012, 12, 21);
                section4.LearningProvider = "Ellucian";
                section4.AddActiveStudent("student-1");
                section4.AddActiveStudent("student-2");
                section4.AddFaculty("0000679");


                var sections = new List<Domain.Student.Entities.Section>();
                sections.Add(section1);
                sections.Add(section2);
                sections.Add(section4);


                List<string> sec1List = new List<string>() { "SEC1" };
                List<Domain.Student.Entities.Section> sec1Resp = new List<Domain.Student.Entities.Section>() { section1 };
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(sec1List, false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sec1Resp));

                List<string> sec1And2List = new List<string>() { "SEC1", "SEC2" };
                List<Domain.Student.Entities.Section> sec1And2Resp = new List<Domain.Student.Entities.Section>() { section1, section2 };
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(sec1And2List, false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sec1And2Resp));


                List<string> sec4List = new List<string>() { "9999999" };
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(sec4List, false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(new List<Domain.Student.Entities.Section>()));

                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(sec4List, false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(new List<Domain.Student.Entities.Section>()));


                // Mock the section service
                sectionCoordinationService = new SectionCoordinationService(adapterRegistry, sectionRepo, courseRepo, studentRepo, studentReferenceDataRepo,
                    referenceDataRepo, termRepo, studentConfigRepo, configRepo, personRepo, roomRepo, eventRepo, bookRepo, currentUserFactory, roleRepo, logger);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetSections_SectionIdList_As_Null()
            {
                await sectionCoordinationService.GetSections4Async(null, true);

            }

            [TestMethod]
            public async Task GetSection_SectionId_As_Empty()
            {
                PrivacyWrapper<List<Dtos.Student.Section4>> section = await sectionCoordinationService.GetSections4Async(new List<string>() { "9999999" }, true);
                Assert.IsFalse(section.HasPrivacyRestrictions);
                Assert.AreEqual(0, section.Dto.Count());
            }

            [TestMethod]
            public async Task GetSection_With_Student_User()
            {
                PrivacyWrapper<List<Dtos.Student.Section4>> section = await sectionCoordinationService.GetSections4Async(new List<string>() { "SEC1", "SEC2" }, true);
                Assert.IsTrue(section.HasPrivacyRestrictions);
                Assert.IsNotNull(section.Dto);
                Assert.AreEqual(2, section.Dto.Count());
                Assert.AreEqual(0, section.Dto[0].ActiveStudentIds.Count());
                Assert.AreEqual(0, section.Dto[1].ActiveStudentIds.Count());
            }

            [TestMethod]
            public async Task GetSection_With_Faculty_User_And_Assigned_To_Section()
            {
                currentUserFactory = new CurrentUserSetup.FacultytUserFactory();
                // Mock the section service
                sectionCoordinationService = new SectionCoordinationService(adapterRegistry, sectionRepo, courseRepo, studentRepo, studentReferenceDataRepo,
                    referenceDataRepo, termRepo, studentConfigRepo, configRepo, personRepo, roomRepo, eventRepo, bookRepo, currentUserFactory, roleRepo, logger);
                PrivacyWrapper<List<Dtos.Student.Section4>> section = await sectionCoordinationService.GetSections4Async(new List<string>() { "SEC1", "SEC2" }, true);
                Assert.IsTrue(section.HasPrivacyRestrictions);
                Assert.IsNotNull(section.Dto);
                Assert.AreEqual(2, section.Dto.Count());
                Assert.AreEqual(2, section.Dto[0].ActiveStudentIds.Count());
                Assert.AreEqual(0, section.Dto[1].ActiveStudentIds.Count());
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionRepo = null;
                studentRepo = null;
                roleRepo = null;
                adapterRegistry = null;
                sectionCoordinationService = null;
            }

        }


        [TestClass]
        public class GetSectionEventsICalAsync : CurrentUserSetup
        {
            IEnumerable<Event> allCals;
            private SectionCoordinationService sectionCoordinationService;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IStudentReferenceDataRepository> stuRefDataRepoMock;
            private IStudentReferenceDataRepository studentReferenceDataRepo;
            private Mock<IReferenceDataRepository> referenceDataRepoMock;
            private IReferenceDataRepository referenceDataRepo;
            private Mock<ICourseRepository> courseRepoMock;
            private ICourseRepository courseRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IStudentConfigurationRepository> studentConfigRepoMock;
            private IStudentConfigurationRepository studentConfigRepo;
            private Mock<IConfigurationRepository> configRepoMock;
            private IConfigurationRepository configRepo;
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IRoomRepository> roomRepoMock;
            private IRoomRepository roomRepo;
            private Mock<IEventRepository> eventRepoMock;
            private IEventRepository eventRepo;
            private Mock<IBookRepository> bookRepoMock;
            private IBookRepository bookRepo;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ILogger logger;
            private ICurrentUserFactory currentUserFactory;

            //public AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.Event, Ellucian.Colleague.Dtos.Base.Event> eventDtoAdapter;
            // public CampusCalendarEntityToDtoAdapter campusCalendarEntityToDtoAdapter;


            [TestInitialize]
            public void Initialize()
            {
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                roomRepoMock = new Mock<IRoomRepository>();
                roomRepo = roomRepoMock.Object;
                stuRefDataRepoMock = new Mock<IStudentReferenceDataRepository>();
                studentReferenceDataRepo = stuRefDataRepoMock.Object;
                referenceDataRepoMock = new Mock<IReferenceDataRepository>();
                referenceDataRepo = referenceDataRepoMock.Object;
                courseRepoMock = new Mock<ICourseRepository>();
                courseRepo = courseRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                studentConfigRepoMock = new Mock<IStudentConfigurationRepository>();
                studentConfigRepo = studentConfigRepoMock.Object;
                configRepoMock = new Mock<IConfigurationRepository>();
                configRepo = configRepoMock.Object;
                eventRepoMock = new Mock<IEventRepository>();
                eventRepo = eventRepoMock.Object;
                bookRepoMock = new Mock<IBookRepository>();
                bookRepo = bookRepoMock.Object;

                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;

                currentUserFactory = new CurrentUserSetup.StudentUserFactory();

                logger = new Mock<ILogger>().Object;

                allCals = new TestEventRepository().Get();

                // Mock the section service
                sectionCoordinationService = new SectionCoordinationService(adapterRegistry, sectionRepo, courseRepo, studentRepo, studentReferenceDataRepo,
                    referenceDataRepo, termRepo, studentConfigRepo, configRepo, personRepo, roomRepo, eventRepo, bookRepo, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {

                allCals = null;

            }


            [TestMethod]
            public async Task GetSectionEventsICalAsync_ValidSingleSectionWithEvents()
            {
                string secId = "1111111";
                List<string> secIds = new List<string>() { secId };
                var csEvents = allCals.Where(c => c.Pointer == secId && c.Type == "CS");
                sectionRepoMock.Setup(repo => repo.GetSectionEventsICalAsync("CS", secIds, null, null)).ReturnsAsync(csEvents);
                string iCal = (await sectionCoordinationService.GetSectionEventsICalAsync(secIds, null, null)).iCal;
                string[] separator = new string[1] { "END:VEVENT" };
                string[] res = iCal.Split(separator, 99, System.StringSplitOptions.None);
                Assert.AreEqual(6, res.Length); // 5 events split into 6 pieces (+1 for "END:VCALENDAR" after last VEVENT)
                // Find the DTSTART and DTEND in each item and verify the correct format (do not process the last item, it's the END:VCALENDAR portion)
                for (int i = 0; i < res.Count() - 1; i++)
                {
                    var eventItem = csEvents.ElementAt(i);
                    separator[0] = "\r\n";
                    string[] eventPieces = res[i].Split(separator, 99, StringSplitOptions.None);
                    string startPiece = eventPieces.Where(e => e.StartsWith("DTSTART:")).FirstOrDefault();
                    if (startPiece != null)
                    {
                        // ical date is formatted: YYYYMMDDTHHMMSSZ (T and Z are hardcoded characters)
                        var dateTimeString = startPiece.Substring(8);
                        VerifyICalZuluDateTime(dateTimeString, eventItem.StartTime.UtcDateTime);
                    }
                    string endPiece = eventPieces.Where(e => e.StartsWith("DTEND:")).FirstOrDefault();
                    if (endPiece != null)
                    {
                        var dateTimeString = endPiece.Substring(6);
                        VerifyICalZuluDateTime(dateTimeString, eventItem.EndTime.UtcDateTime);
                    }
                }

            }

            [TestMethod]
            public async Task GetSectionEventsICalAsync_SingleSectionFirstThreeDates()
            {
                string secId = "1111111";
                List<string> secIds = new List<string>() { secId };
                DateTime start = new DateTime(2012, 8, 1);
                DateTime end = new DateTime(2012, 8, 3);
                sectionRepoMock.Setup(repo => repo.GetSectionEventsICalAsync("CS", secIds, start, end)).ReturnsAsync(allCals.Where(c => c.Pointer == secId && c.Type == "CS" && c.Start.Year == 2012 && c.Start.Month == 8 && c.Start.Day <= 3));
                string iCal = (await sectionCoordinationService.GetSectionEventsICalAsync(secIds, start, end)).iCal;
                string[] seps = new string[1] { "END:VEVENT" };
                string[] res = iCal.Split(seps, 99, System.StringSplitOptions.None);
                Assert.AreEqual(4, res.Length); // 3 events split into 4 pieces (+1 for "END:VCALENDAR" after last VEVENT)
            }

            [TestMethod]
            public async Task GetSectionEventsICalAsync_ValidMultipleSectionsWithEvents()
            {
                string secId1 = "1111111";
                string secId2 = "2222222";
                List<string> secIds = new List<string>() { secId1, secId2 };
                sectionRepoMock.Setup(repo => repo.GetSectionEventsICalAsync("CS", secIds, null, null)).ReturnsAsync(allCals.Where(c => (c.Pointer == secId1 || c.Pointer == secId2) && c.Type == "CS"));
                string iCal = (await sectionCoordinationService.GetSectionEventsICalAsync(secIds, null, null)).iCal;
                string[] seps = new string[1] { "END:VEVENT" };
                string[] res = iCal.Split(seps, 99, System.StringSplitOptions.None);
                Assert.AreEqual(9, res.Length); // 8 (5+3) events split into 9 pieces (+1 for "END:VCALENDAR" after last VEVENT)
            }

            [TestMethod]
            public async Task GetSectionEventsICalAsync_InvalidSection()
            {
                string secId = "9999999";
                List<string> secIds = new List<string>() { secId };
                sectionRepoMock.Setup(repo => repo.GetSectionEventsICalAsync("CS", secIds, null, null)).ReturnsAsync(allCals.Where(c => c.Pointer == secId && c.Type == "CS"));
                string iCal = (await sectionCoordinationService.GetSectionEventsICalAsync(secIds, null, null)).iCal;
                string[] seps = new string[1] { "END:VEVENT" };
                string[] res = iCal.Split(seps, 99, System.StringSplitOptions.None);
                Assert.AreEqual(1, res.Length); // no splits occur
            }

            private void VerifyICalZuluDateTime(string dateTimeString, DateTime eventTime)
            {
                Assert.AreEqual('Z', dateTimeString[15]);
                Assert.AreEqual('T', dateTimeString[8]);
                Assert.AreEqual(eventTime, new DateTime(int.Parse(dateTimeString.Substring(0, 4)), int.Parse(dateTimeString.Substring(4, 2)), int.Parse(dateTimeString.Substring(6, 2)), int.Parse(dateTimeString.Substring(9, 2)), int.Parse(dateTimeString.Substring(11, 2)), int.Parse(dateTimeString.Substring(13, 2))));
            }
        }

        [TestClass]
        public class GetSectionMidtermGradingCompleteAsync : CurrentUserSetup
        {
            private SectionCoordinationService sectionCoordinationService;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IStudentReferenceDataRepository> stuRefDataRepoMock;
            private IStudentReferenceDataRepository studentReferenceDataRepo;
            private Mock<IReferenceDataRepository> referenceDataRepoMock;
            private IReferenceDataRepository referenceDataRepo;
            private Mock<ICourseRepository> courseRepoMock;
            private ICourseRepository courseRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IStudentConfigurationRepository> studentConfigRepoMock;
            private IStudentConfigurationRepository studentConfigRepo;
            private Mock<IConfigurationRepository> configRepoMock;
            private IConfigurationRepository configRepo;
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IRoomRepository> roomRepoMock;
            private IRoomRepository roomRepo;
            private Mock<IEventRepository> eventRepoMock;
            private IEventRepository eventRepo;
            private Mock<IBookRepository> bookRepoMock;
            private IBookRepository bookRepo;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ILogger logger;
            private ICurrentUserFactory currentUserFactory;


            [TestInitialize]
            public void Initialize()
            {
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                roomRepoMock = new Mock<IRoomRepository>();
                roomRepo = roomRepoMock.Object;
                stuRefDataRepoMock = new Mock<IStudentReferenceDataRepository>();
                studentReferenceDataRepo = stuRefDataRepoMock.Object;
                referenceDataRepoMock = new Mock<IReferenceDataRepository>();
                referenceDataRepo = referenceDataRepoMock.Object;
                courseRepoMock = new Mock<ICourseRepository>();
                courseRepo = courseRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                studentConfigRepoMock = new Mock<IStudentConfigurationRepository>();
                studentConfigRepo = studentConfigRepoMock.Object;
                configRepoMock = new Mock<IConfigurationRepository>();
                configRepo = configRepoMock.Object;
                eventRepoMock = new Mock<IEventRepository>();
                eventRepo = eventRepoMock.Object;
                bookRepoMock = new Mock<IBookRepository>();
                bookRepo = bookRepoMock.Object;

                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;

                // Mock the adapter registry to return the midterm grading complete entity to dto adapter
                var emptyAdapterRegistryMock = new Mock<IAdapterRegistry>();
                var sectionGradingCompleteAdapter = new Student.Adapters.SectionMidtermGradingCompleteEntityToDtoAdapter(emptyAdapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x =>
                    x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionMidtermGradingComplete,
                        Ellucian.Colleague.Dtos.Student.SectionMidtermGradingComplete>()).Returns(sectionGradingCompleteAdapter);

                currentUserFactory = new CurrentUserSetup.StudentUserFactory();

                logger = new Mock<ILogger>().Object;

                // Mock the section service
                sectionCoordinationService = new SectionCoordinationService(adapterRegistry, sectionRepo, courseRepo, studentRepo, studentReferenceDataRepo,
                    referenceDataRepo, termRepo, studentConfigRepo, configRepo, personRepo, roomRepo, eventRepo, bookRepo, currentUserFactory, roleRepo, logger);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetSectionMidtermGradingCompleteAsync_NullSectionId()
            {
                var dto = await sectionCoordinationService.GetSectionMidtermGradingCompleteAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetSectionMidtermGradingCompleteAsync_GetSectionFails()
            {
                // Test when the get section from the repository fails
                string sectionId = "nullSection";
                sectionRepoMock.Setup(repo => repo.GetSectionAsync(sectionId)).Returns(Task.FromResult<Section>(null));
                var dto = await sectionCoordinationService.GetSectionMidtermGradingCompleteAsync(sectionId);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task GetSectionMidtermGradingCompleteAsync_PermissionFailure()
            {
                // Test when the the user does not have necessary permission
                string sectionId = "NoFaculty";
                // Section with no assigned faculty, so the current user will not be a faculty member
                Section secEntity = new Section(sectionId, "crsId", "num", new DateTime(2010, 1, 1), 3, null, "title", "Inst",
                        new List<OfferingDepartment> { new OfferingDepartment("DEPT1") }, new List<string> { "UG" }, "UG",
                        new List<SectionStatusItem> { new SectionStatusItem(SectionStatus.Active, "A", new DateTime(2010, 1, 1)) });
                sectionRepoMock.Setup(repo => repo.GetSectionAsync(sectionId)).Returns(Task.FromResult<Section>(secEntity));

                // Current user is not a faculty member of the section
                var dto = await sectionCoordinationService.GetSectionMidtermGradingCompleteAsync(sectionId);
            }

            [TestMethod]
            public async Task GetSectionMidtermGradingCompleteAsync_SuccessfulGet()
            {
                // Test a successful get which will cover the conversion of the entity to the dto, as well as a succesful permission check
                // when the current user is a faculty member.

                // Assign the currrent user of the mocked current user factory as a faculty member of the section
                string sectionId = "CurrentUser";
                // Section with no assigned faculty, so the current user will not be a faculty member
                Section secEntity = new Section(sectionId, "crsId", "num", new DateTime(2010, 1, 1), 3, null, "title", "Inst",
                        new List<OfferingDepartment> { new OfferingDepartment("DEPT1") }, new List<string> { "UG" }, "UG",
                        new List<SectionStatusItem> { new SectionStatusItem(SectionStatus.Active, "A", new DateTime(2010, 1, 1)) });
                secEntity.AddFaculty("STU1");
                sectionRepoMock.Setup(repo => repo.GetSectionAsync(sectionId)).Returns(Task.FromResult<Section>(secEntity));

                // Mock a midterm grading complete entity that the repository will return
                var entity = new Ellucian.Colleague.Domain.Student.Entities.SectionMidtermGradingComplete(sectionId);
                entity.AddMidtermGrading1Complete("Oper1", new DateTimeOffset(2010, 1, 1, 1, 1, 1, new TimeSpan(-4, 0, 0)));
                entity.AddMidtermGrading1Complete("Oper2", new DateTimeOffset(2010, 1, 2, 1, 1, 2, new TimeSpan(-4, 0, 0)));
                entity.AddMidtermGrading2Complete("Oper3", new DateTimeOffset(2010, 1, 3, 1, 1, 3, new TimeSpan(-4, 0, 0)));
                entity.AddMidtermGrading2Complete("Oper4", new DateTimeOffset(2010, 1, 4, 1, 1, 4, new TimeSpan(-4, 0, 0)));
                entity.AddMidtermGrading3Complete("Oper5", new DateTimeOffset(2010, 1, 5, 1, 1, 5, new TimeSpan(-4, 0, 0)));
                entity.AddMidtermGrading3Complete("Oper6", new DateTimeOffset(2010, 1, 6, 1, 1, 6, new TimeSpan(-4, 0, 0)));
                entity.AddMidtermGrading4Complete("Oper7", new DateTimeOffset(2010, 1, 7, 1, 1, 7, new TimeSpan(-4, 0, 0)));
                entity.AddMidtermGrading4Complete("Oper8", new DateTimeOffset(2010, 1, 8, 1, 1, 8, new TimeSpan(-4, 0, 0)));
                entity.AddMidtermGrading5Complete("Oper9", new DateTimeOffset(2010, 1, 9, 1, 1, 9, new TimeSpan(-4, 0, 0)));
                entity.AddMidtermGrading5Complete("Oper10", new DateTimeOffset(2010, 1, 10, 1, 1, 10, new TimeSpan(-4, 0, 0)));
                entity.AddMidtermGrading6Complete("Oper11", new DateTimeOffset(2010, 1, 11, 1, 1, 11, new TimeSpan(-4, 0, 0)));
                entity.AddMidtermGrading6Complete("Oper12", new DateTimeOffset(2010, 1, 12, 1, 1, 12, new TimeSpan(-4, 0, 0)));
                sectionRepoMock.Setup(repo => repo.GetSectionMidtermGradingCompleteAsync(sectionId)).Returns
                        (Task.FromResult<Ellucian.Colleague.Domain.Student.Entities.SectionMidtermGradingComplete>(entity));

                var dto = await sectionCoordinationService.GetSectionMidtermGradingCompleteAsync(sectionId);

                Assert.AreEqual(entity.SectionId, dto.SectionId);
                Assert.AreEqual(entity.MidtermGrading1Complete.Count, dto.MidtermGrading1Complete.Count());
                Assert.AreEqual(entity.MidtermGrading2Complete.Count, dto.MidtermGrading2Complete.Count());
                Assert.AreEqual(entity.MidtermGrading3Complete.Count, dto.MidtermGrading3Complete.Count());
                Assert.AreEqual(entity.MidtermGrading4Complete.Count, dto.MidtermGrading4Complete.Count());
                Assert.AreEqual(entity.MidtermGrading5Complete.Count, dto.MidtermGrading5Complete.Count());
                Assert.AreEqual(entity.MidtermGrading6Complete.Count, dto.MidtermGrading6Complete.Count());

                Assert.AreEqual(entity.MidtermGrading1Complete[0].CompleteOperator, dto.MidtermGrading1Complete.ElementAt(0).CompleteOperator);
                Assert.AreEqual(entity.MidtermGrading1Complete[0].DateAndTime, dto.MidtermGrading1Complete.ElementAt(0).DateAndTime);
                Assert.AreEqual(entity.MidtermGrading1Complete[1].CompleteOperator, dto.MidtermGrading1Complete.ElementAt(1).CompleteOperator);
                Assert.AreEqual(entity.MidtermGrading1Complete[1].DateAndTime, dto.MidtermGrading1Complete.ElementAt(1).DateAndTime);
                Assert.AreEqual(entity.MidtermGrading2Complete[0].CompleteOperator, dto.MidtermGrading2Complete.ElementAt(0).CompleteOperator);
                Assert.AreEqual(entity.MidtermGrading2Complete[0].DateAndTime, dto.MidtermGrading2Complete.ElementAt(0).DateAndTime);
                Assert.AreEqual(entity.MidtermGrading2Complete[1].CompleteOperator, dto.MidtermGrading2Complete.ElementAt(1).CompleteOperator);
                Assert.AreEqual(entity.MidtermGrading2Complete[1].DateAndTime, dto.MidtermGrading2Complete.ElementAt(1).DateAndTime);
                Assert.AreEqual(entity.MidtermGrading3Complete[0].CompleteOperator, dto.MidtermGrading3Complete.ElementAt(0).CompleteOperator);
                Assert.AreEqual(entity.MidtermGrading3Complete[0].DateAndTime, dto.MidtermGrading3Complete.ElementAt(0).DateAndTime);
                Assert.AreEqual(entity.MidtermGrading3Complete[1].CompleteOperator, dto.MidtermGrading3Complete.ElementAt(1).CompleteOperator);
                Assert.AreEqual(entity.MidtermGrading3Complete[1].DateAndTime, dto.MidtermGrading3Complete.ElementAt(1).DateAndTime);
                Assert.AreEqual(entity.MidtermGrading4Complete[0].CompleteOperator, dto.MidtermGrading4Complete.ElementAt(0).CompleteOperator);
                Assert.AreEqual(entity.MidtermGrading4Complete[0].DateAndTime, dto.MidtermGrading4Complete.ElementAt(0).DateAndTime);
                Assert.AreEqual(entity.MidtermGrading4Complete[1].CompleteOperator, dto.MidtermGrading4Complete.ElementAt(1).CompleteOperator);
                Assert.AreEqual(entity.MidtermGrading4Complete[1].DateAndTime, dto.MidtermGrading4Complete.ElementAt(1).DateAndTime);
                Assert.AreEqual(entity.MidtermGrading5Complete[0].CompleteOperator, dto.MidtermGrading5Complete.ElementAt(0).CompleteOperator);
                Assert.AreEqual(entity.MidtermGrading5Complete[0].DateAndTime, dto.MidtermGrading5Complete.ElementAt(0).DateAndTime);
                Assert.AreEqual(entity.MidtermGrading5Complete[1].CompleteOperator, dto.MidtermGrading5Complete.ElementAt(1).CompleteOperator);
                Assert.AreEqual(entity.MidtermGrading5Complete[1].DateAndTime, dto.MidtermGrading5Complete.ElementAt(1).DateAndTime);
                Assert.AreEqual(entity.MidtermGrading6Complete[0].CompleteOperator, dto.MidtermGrading6Complete.ElementAt(0).CompleteOperator);
                Assert.AreEqual(entity.MidtermGrading6Complete[0].DateAndTime, dto.MidtermGrading6Complete.ElementAt(0).DateAndTime);
                Assert.AreEqual(entity.MidtermGrading6Complete[1].CompleteOperator, dto.MidtermGrading6Complete.ElementAt(1).CompleteOperator);
                Assert.AreEqual(entity.MidtermGrading6Complete[1].DateAndTime, dto.MidtermGrading6Complete.ElementAt(1).DateAndTime);
            }
        }

        [TestClass]
        public class PostSectionMidtermGradingCompleteAsync : CurrentUserSetup
        {
            private SectionCoordinationService sectionCoordinationService;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IStudentReferenceDataRepository> stuRefDataRepoMock;
            private IStudentReferenceDataRepository studentReferenceDataRepo;
            private Mock<IReferenceDataRepository> referenceDataRepoMock;
            private IReferenceDataRepository referenceDataRepo;
            private Mock<ICourseRepository> courseRepoMock;
            private ICourseRepository courseRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IStudentConfigurationRepository> studentConfigRepoMock;
            private IStudentConfigurationRepository studentConfigRepo;
            private Mock<IConfigurationRepository> configRepoMock;
            private IConfigurationRepository configRepo;
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IRoomRepository> roomRepoMock;
            private IRoomRepository roomRepo;
            private Mock<IEventRepository> eventRepoMock;
            private IEventRepository eventRepo;
            private Mock<IBookRepository> bookRepoMock;
            private IBookRepository bookRepo;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ILogger logger;
            private ICurrentUserFactory currentUserFactory;
            private SectionMidtermGradingCompleteForPost postDto;


            [TestInitialize]
            public void Initialize()
            {
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                roomRepoMock = new Mock<IRoomRepository>();
                roomRepo = roomRepoMock.Object;
                stuRefDataRepoMock = new Mock<IStudentReferenceDataRepository>();
                studentReferenceDataRepo = stuRefDataRepoMock.Object;
                referenceDataRepoMock = new Mock<IReferenceDataRepository>();
                referenceDataRepo = referenceDataRepoMock.Object;
                courseRepoMock = new Mock<ICourseRepository>();
                courseRepo = courseRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                studentConfigRepoMock = new Mock<IStudentConfigurationRepository>();
                studentConfigRepo = studentConfigRepoMock.Object;
                configRepoMock = new Mock<IConfigurationRepository>();
                configRepo = configRepoMock.Object;
                eventRepoMock = new Mock<IEventRepository>();
                eventRepo = eventRepoMock.Object;
                bookRepoMock = new Mock<IBookRepository>();
                bookRepo = bookRepoMock.Object;

                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;

                // Mock the adapter registry to return the midterm grading complete entity to dto adapter
                var emptyAdapterRegistryMock = new Mock<IAdapterRegistry>();
                var sectionGradingCompleteAdapter = new Student.Adapters.SectionMidtermGradingCompleteEntityToDtoAdapter(emptyAdapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x =>
                    x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionMidtermGradingComplete,
                        Ellucian.Colleague.Dtos.Student.SectionMidtermGradingComplete>()).Returns(sectionGradingCompleteAdapter);

                currentUserFactory = new CurrentUserSetup.StudentUserFactory();

                logger = new Mock<ILogger>().Object;

                postDto = new SectionMidtermGradingCompleteForPost()
                {
                    CompleteOperator = "0012345",
                    DateAndTime = DateTimeOffset.Now,
                    MidtermGradeNumber = 1
                };

                // Mock the section service
                sectionCoordinationService = new SectionCoordinationService(adapterRegistry, sectionRepo, courseRepo, studentRepo, studentReferenceDataRepo,
                    referenceDataRepo, termRepo, studentConfigRepo, configRepo, personRepo, roomRepo, eventRepo, bookRepo, currentUserFactory, roleRepo, logger);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PostSectionMidtermGradingCompleteAsync_NullSectionId()
            {
                var dto = await sectionCoordinationService.PostSectionMidtermGradingCompleteAsync(null, postDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PostSectionMidtermGradingCompleteAsync_Null_Dto()
            {
                var dto = await sectionCoordinationService.PostSectionMidtermGradingCompleteAsync("12345", null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PostSectionMidtermGradingCompleteAsync_Null_Dto_MidtermGradeNumber()
            {
                postDto.MidtermGradeNumber = null;
                var dto = await sectionCoordinationService.PostSectionMidtermGradingCompleteAsync("12345", postDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task PostSectionMidtermGradingCompleteAsync_Dto_MidtermGradeNumber_Too_Low()
            {
                postDto.MidtermGradeNumber = 0;
                var dto = await sectionCoordinationService.PostSectionMidtermGradingCompleteAsync("12345", postDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task PostSectionMidtermGradingCompleteAsync_Dto_MidtermGradeNumber_Too_High()
            {
                postDto.MidtermGradeNumber = 7;
                var dto = await sectionCoordinationService.PostSectionMidtermGradingCompleteAsync("12345", postDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PostSectionMidtermGradingCompleteAsync_Null_Dto_CompleteOperator()
            {
                postDto.CompleteOperator = null;
                var dto = await sectionCoordinationService.PostSectionMidtermGradingCompleteAsync("12345", postDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PostSectionMidtermGradingCompleteAsync_Empty_Dto_CompleteOperator()
            {
                postDto.CompleteOperator = string.Empty;
                var dto = await sectionCoordinationService.PostSectionMidtermGradingCompleteAsync("12345", postDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PostSectionMidtermGradingCompleteAsync_Null_Dto_DateAndTime()
            {
                postDto.DateAndTime = null;
                var dto = await sectionCoordinationService.PostSectionMidtermGradingCompleteAsync("12345", postDto);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task PostSectionMidtermGradingCompleteAsync_GetSection_returns_null()
            {
                // Test when the get section from the repository fails
                string sectionId = "nullSection";
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(new List<string>() { sectionId }, It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<Section>>(null));
                var dto = await sectionCoordinationService.PostSectionMidtermGradingCompleteAsync(sectionId, postDto);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task PostSectionMidtermGradingCompleteAsync_GetSection_returns_empty_list()
            {
                // Test when the get section from the repository fails
                string sectionId = "nullSection";
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(new List<string>() { sectionId }, It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<Section>>(new List<Section>()));
                var dto = await sectionCoordinationService.PostSectionMidtermGradingCompleteAsync(sectionId, postDto);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task PostSectionMidtermGradingCompleteAsync_GetSection_returns_list_with_null_section()
            {
                // Test when the get section from the repository fails
                string sectionId = "nullSection";
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(new List<string>() { sectionId }, It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<Section>>(new List<Section>() { null }));
                var dto = await sectionCoordinationService.PostSectionMidtermGradingCompleteAsync(sectionId, postDto);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task PostSectionMidtermGradingCompleteAsync_PermissionFailure()
            {
                // Test when the the user does not have necessary permission
                string sectionId = "NoFaculty";
                // Section with no assigned faculty, so the current user will not be a faculty member
                Section secEntity = new Section(sectionId, "crsId", "num", new DateTime(2010, 1, 1), 3, null, "title", "Inst",
                        new List<OfferingDepartment> { new OfferingDepartment("DEPT1") }, new List<string> { "UG" }, "UG",
                        new List<SectionStatusItem> { new SectionStatusItem(SectionStatus.Active, "A", new DateTime(2010, 1, 1)) });
                sectionRepoMock.Setup(repo => repo.GetSectionAsync(sectionId)).Returns(Task.FromResult<Section>(secEntity));

                // Current user is not a faculty member of the section
                var dto = await sectionCoordinationService.PostSectionMidtermGradingCompleteAsync(sectionId, postDto);
            }

            [TestMethod]
            public async Task PostSectionMidtermGradingCompleteAsync_SuccessfulPost()
            {
                // Test a successful post which will cover the conversion of the entity to the dto, as well as a succesful permission check
                // when the current user is a faculty member.

                // Assign the currrent user of the mocked current user factory as a faculty member of the section
                string sectionId = "CurrentUser";
                // Section with no assigned faculty, so the current user will not be a faculty member
                Section secEntity = new Section(sectionId, "crsId", "num", new DateTime(2010, 1, 1), 3, null, "title", "Inst",
                        new List<OfferingDepartment> { new OfferingDepartment("DEPT1") }, new List<string> { "UG" }, "UG",
                        new List<SectionStatusItem> { new SectionStatusItem(SectionStatus.Active, "A", new DateTime(2010, 1, 1)) });
                secEntity.AddFaculty("STU1");
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(new List<string>() { sectionId }, It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<Section>>(new List<Section>() { secEntity }));

                // Mock a midterm grading complete entity that the repository will return
                var entity = new Ellucian.Colleague.Domain.Student.Entities.SectionMidtermGradingComplete(sectionId);
                entity.AddMidtermGrading1Complete("Oper1", new DateTimeOffset(2010, 1, 1, 1, 1, 1, new TimeSpan(-4, 0, 0)));
                entity.AddMidtermGrading1Complete("Oper2", new DateTimeOffset(2010, 1, 2, 1, 1, 2, new TimeSpan(-4, 0, 0)));
                entity.AddMidtermGrading2Complete("Oper3", new DateTimeOffset(2010, 1, 3, 1, 1, 3, new TimeSpan(-4, 0, 0)));
                entity.AddMidtermGrading2Complete("Oper4", new DateTimeOffset(2010, 1, 4, 1, 1, 4, new TimeSpan(-4, 0, 0)));
                entity.AddMidtermGrading3Complete("Oper5", new DateTimeOffset(2010, 1, 5, 1, 1, 5, new TimeSpan(-4, 0, 0)));
                entity.AddMidtermGrading3Complete("Oper6", new DateTimeOffset(2010, 1, 6, 1, 1, 6, new TimeSpan(-4, 0, 0)));
                entity.AddMidtermGrading4Complete("Oper7", new DateTimeOffset(2010, 1, 7, 1, 1, 7, new TimeSpan(-4, 0, 0)));
                entity.AddMidtermGrading4Complete("Oper8", new DateTimeOffset(2010, 1, 8, 1, 1, 8, new TimeSpan(-4, 0, 0)));
                entity.AddMidtermGrading5Complete("Oper9", new DateTimeOffset(2010, 1, 9, 1, 1, 9, new TimeSpan(-4, 0, 0)));
                entity.AddMidtermGrading5Complete("Oper10", new DateTimeOffset(2010, 1, 10, 1, 1, 10, new TimeSpan(-4, 0, 0)));
                entity.AddMidtermGrading6Complete("Oper11", new DateTimeOffset(2010, 1, 11, 1, 1, 11, new TimeSpan(-4, 0, 0)));
                entity.AddMidtermGrading6Complete("Oper12", new DateTimeOffset(2010, 1, 12, 1, 1, 12, new TimeSpan(-4, 0, 0)));
                sectionRepoMock.Setup(repo => repo.PostSectionMidtermGradingCompleteAsync(sectionId, postDto.MidtermGradeNumber, postDto.CompleteOperator, postDto.DateAndTime)).Returns
                        (Task.FromResult<Ellucian.Colleague.Domain.Student.Entities.SectionMidtermGradingComplete>(entity));

                var dto = await sectionCoordinationService.PostSectionMidtermGradingCompleteAsync(sectionId, postDto);

                Assert.AreEqual(entity.SectionId, dto.SectionId);
                Assert.AreEqual(entity.MidtermGrading1Complete.Count, dto.MidtermGrading1Complete.Count());
                Assert.AreEqual(entity.MidtermGrading2Complete.Count, dto.MidtermGrading2Complete.Count());
                Assert.AreEqual(entity.MidtermGrading3Complete.Count, dto.MidtermGrading3Complete.Count());
                Assert.AreEqual(entity.MidtermGrading4Complete.Count, dto.MidtermGrading4Complete.Count());
                Assert.AreEqual(entity.MidtermGrading5Complete.Count, dto.MidtermGrading5Complete.Count());
                Assert.AreEqual(entity.MidtermGrading6Complete.Count, dto.MidtermGrading6Complete.Count());

                Assert.AreEqual(entity.MidtermGrading1Complete[0].CompleteOperator, dto.MidtermGrading1Complete.ElementAt(0).CompleteOperator);
                Assert.AreEqual(entity.MidtermGrading1Complete[0].DateAndTime, dto.MidtermGrading1Complete.ElementAt(0).DateAndTime);
                Assert.AreEqual(entity.MidtermGrading1Complete[1].CompleteOperator, dto.MidtermGrading1Complete.ElementAt(1).CompleteOperator);
                Assert.AreEqual(entity.MidtermGrading1Complete[1].DateAndTime, dto.MidtermGrading1Complete.ElementAt(1).DateAndTime);
                Assert.AreEqual(entity.MidtermGrading2Complete[0].CompleteOperator, dto.MidtermGrading2Complete.ElementAt(0).CompleteOperator);
                Assert.AreEqual(entity.MidtermGrading2Complete[0].DateAndTime, dto.MidtermGrading2Complete.ElementAt(0).DateAndTime);
                Assert.AreEqual(entity.MidtermGrading2Complete[1].CompleteOperator, dto.MidtermGrading2Complete.ElementAt(1).CompleteOperator);
                Assert.AreEqual(entity.MidtermGrading2Complete[1].DateAndTime, dto.MidtermGrading2Complete.ElementAt(1).DateAndTime);
                Assert.AreEqual(entity.MidtermGrading3Complete[0].CompleteOperator, dto.MidtermGrading3Complete.ElementAt(0).CompleteOperator);
                Assert.AreEqual(entity.MidtermGrading3Complete[0].DateAndTime, dto.MidtermGrading3Complete.ElementAt(0).DateAndTime);
                Assert.AreEqual(entity.MidtermGrading3Complete[1].CompleteOperator, dto.MidtermGrading3Complete.ElementAt(1).CompleteOperator);
                Assert.AreEqual(entity.MidtermGrading3Complete[1].DateAndTime, dto.MidtermGrading3Complete.ElementAt(1).DateAndTime);
                Assert.AreEqual(entity.MidtermGrading4Complete[0].CompleteOperator, dto.MidtermGrading4Complete.ElementAt(0).CompleteOperator);
                Assert.AreEqual(entity.MidtermGrading4Complete[0].DateAndTime, dto.MidtermGrading4Complete.ElementAt(0).DateAndTime);
                Assert.AreEqual(entity.MidtermGrading4Complete[1].CompleteOperator, dto.MidtermGrading4Complete.ElementAt(1).CompleteOperator);
                Assert.AreEqual(entity.MidtermGrading4Complete[1].DateAndTime, dto.MidtermGrading4Complete.ElementAt(1).DateAndTime);
                Assert.AreEqual(entity.MidtermGrading5Complete[0].CompleteOperator, dto.MidtermGrading5Complete.ElementAt(0).CompleteOperator);
                Assert.AreEqual(entity.MidtermGrading5Complete[0].DateAndTime, dto.MidtermGrading5Complete.ElementAt(0).DateAndTime);
                Assert.AreEqual(entity.MidtermGrading5Complete[1].CompleteOperator, dto.MidtermGrading5Complete.ElementAt(1).CompleteOperator);
                Assert.AreEqual(entity.MidtermGrading5Complete[1].DateAndTime, dto.MidtermGrading5Complete.ElementAt(1).DateAndTime);
                Assert.AreEqual(entity.MidtermGrading6Complete[0].CompleteOperator, dto.MidtermGrading6Complete.ElementAt(0).CompleteOperator);
                Assert.AreEqual(entity.MidtermGrading6Complete[0].DateAndTime, dto.MidtermGrading6Complete.ElementAt(0).DateAndTime);
                Assert.AreEqual(entity.MidtermGrading6Complete[1].CompleteOperator, dto.MidtermGrading6Complete.ElementAt(1).CompleteOperator);
                Assert.AreEqual(entity.MidtermGrading6Complete[1].DateAndTime, dto.MidtermGrading6Complete.ElementAt(1).DateAndTime);
            }
        }

        [TestClass]
        public class UpdateSectionCensusCertificationAsync : CurrentUserSetup
        {
            private SectionCoordinationService sectionCoordinationService;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IStudentReferenceDataRepository> stuRefDataRepoMock;
            private IStudentReferenceDataRepository studentReferenceDataRepo;
            private Mock<IReferenceDataRepository> referenceDataRepoMock;
            private IReferenceDataRepository referenceDataRepo;
            private Mock<ICourseRepository> courseRepoMock;
            private ICourseRepository courseRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IStudentConfigurationRepository> studentConfigRepoMock;
            private IStudentConfigurationRepository studentConfigRepo;
            private Mock<IConfigurationRepository> configRepoMock;
            private IConfigurationRepository configRepo;
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IRoomRepository> roomRepoMock;
            private IRoomRepository roomRepo;
            private Mock<IEventRepository> eventRepoMock;
            private IEventRepository eventRepo;
            private Mock<IBookRepository> bookRepoMock;
            private IBookRepository bookRepo;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ILogger logger;
            private ICurrentUserFactory currentUserFactory;
            private string sectionId = "12345";
            private Section sectToPlayWith = null;

            private Dtos.Student.SectionRegistrationDate sectionRegistrationDate = new Dtos.Student.SectionRegistrationDate();
            private Dtos.Student.SectionCensusToCertify sectionCensusToCertify = new Dtos.Student.SectionCensusToCertify();


            [TestInitialize]
            public void Initialize()
            {
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                roomRepoMock = new Mock<IRoomRepository>();
                roomRepo = roomRepoMock.Object;
                stuRefDataRepoMock = new Mock<IStudentReferenceDataRepository>();
                studentReferenceDataRepo = stuRefDataRepoMock.Object;
                referenceDataRepoMock = new Mock<IReferenceDataRepository>();
                referenceDataRepo = referenceDataRepoMock.Object;
                courseRepoMock = new Mock<ICourseRepository>();
                courseRepo = courseRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                studentConfigRepoMock = new Mock<IStudentConfigurationRepository>();
                studentConfigRepo = studentConfigRepoMock.Object;
                configRepoMock = new Mock<IConfigurationRepository>();
                configRepo = configRepoMock.Object;
                eventRepoMock = new Mock<IEventRepository>();
                eventRepo = eventRepoMock.Object;
                bookRepoMock = new Mock<IBookRepository>();
                bookRepo = bookRepoMock.Object;

                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;

                // Mock the adapter registry to return the midterm grading complete entity to dto adapter
                var sectionCensusAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionCensusCertification, Dtos.Student.SectionCensusCertification>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x =>
                    x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionCensusCertification, Dtos.Student.SectionCensusCertification>()).Returns(sectionCensusAdapter);

                currentUserFactory = new CurrentUserSetup.FacultytUserFactory();
                logger = new Mock<ILogger>().Object;

                //section to certify
                sectionCensusToCertify.CensusCertificationDate = new DateTime(2021, 01, 02);
                sectionCensusToCertify.CensusCertificationPosition = "1";
                sectionCensusToCertify.CensusCertificationLabel = "final grading";
                sectionCensusToCertify.CensusCertificationRecordedDate = new DateTime(2021, 02, 01);
                sectionCensusToCertify.CensusCertificationRecordedTime = new DateTimeOffset(new DateTime(2021, 02, 01, 05, 03, 04));

                //section registration dates
                sectionRegistrationDate = new Dtos.Student.SectionRegistrationDate();

                // Census Date Configuration
                var censusDatePositionSubmissions = new List<Domain.Student.Entities.CensusDatePositionSubmission>();
                censusDatePositionSubmissions.Add(new Domain.Student.Entities.CensusDatePositionSubmission(position: 1, label: "final grading", certifyDaysBeforeOffset: 1));

                var sectionCensusConfiguration = new Domain.Student.Entities.SectionCensusConfiguration(
                    lastDateAttendedNeverAttendedCensusRoster: Domain.Student.Entities.LastDateAttendedNeverAttendedFieldDisplayType.Editable,
                    censusDatePositionSubmissions: censusDatePositionSubmissions, facultyDropReasonCode: "D");
                studentConfigRepoMock.Setup(repo => repo.GetSectionCensusConfigurationAsync()).ReturnsAsync(sectionCensusConfiguration);

                // Mock the section service
                sectionCoordinationService = new SectionCoordinationService(adapterRegistry, sectionRepo, courseRepo, studentRepo, studentReferenceDataRepo,
                    referenceDataRepo, termRepo, studentConfigRepo, configRepo, personRepo, roomRepo, eventRepo, bookRepo, currentUserFactory, roleRepo, logger);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionCoordinationService_CreateSectionCensusCertificationAsync_SectionId_IsNull()
            {
                await sectionCoordinationService.CreateSectionCensusCertificationAsync(null, sectionCensusToCertify, sectionRegistrationDate);
            }

            //sectionToCertify is null
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionCoordinationService_CreateSectionCensusCertificationAsync_SectionToVerify_IsNull()
            {
                await sectionCoordinationService.CreateSectionCensusCertificationAsync(sectionId, null, sectionRegistrationDate);
            }

            //SectionToCertify Census date is null
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionCoordinationService_CreateSectionCensusCertificationAsync_SectionToVerify_CensusDate_IsNull()
            {
                sectionCensusToCertify.CensusCertificationDate = null;
                await sectionCoordinationService.CreateSectionCensusCertificationAsync(sectionId, sectionCensusToCertify, sectionRegistrationDate);
            }

            //SectionToCertify recorded date is null
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionCoordinationService_CreateSectionCensusCertificationAsync_SectionToVerify_RecordedDate_IsNull()
            {
                sectionCensusToCertify.CensusCertificationRecordedDate = null;
                await sectionCoordinationService.CreateSectionCensusCertificationAsync(sectionId, sectionCensusToCertify, sectionRegistrationDate);
            }

            //SectionToCertify record time date is null
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionCoordinationService_CreateSectionCensusCertificationAsync_SectionToVerify_RecordedTime_IsNull()
            {
                sectionCensusToCertify.CensusCertificationRecordedTime = null;
                await sectionCoordinationService.CreateSectionCensusCertificationAsync(sectionId, sectionCensusToCertify, sectionRegistrationDate);
            }

            //SectionToCertify position is null
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionCoordinationService_CreateSectionCensusCertificationAsync_SectionToVerify_Position_IsNull()
            {
                sectionCensusToCertify.CensusCertificationPosition = null;
                await sectionCoordinationService.CreateSectionCensusCertificationAsync(sectionId, sectionCensusToCertify, sectionRegistrationDate);
            }

            //SectionToCertify position is empty
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionCoordinationService_CreateSectionCensusCertificationAsync_SectionToVerify_Position_IsEmpty()
            {
                sectionCensusToCertify.CensusCertificationPosition = string.Empty;
                await sectionCoordinationService.CreateSectionCensusCertificationAsync(sectionId, sectionCensusToCertify, sectionRegistrationDate);
            }

            //SectionToCertify position is whitespace
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SectionCoordinationService_CreateSectionCensusCertificationAsync_SectionToVerify_Position_IsWhitespace()
            {
                sectionCensusToCertify.CensusCertificationPosition = "   ";
                await sectionCoordinationService.CreateSectionCensusCertificationAsync(sectionId, sectionCensusToCertify, sectionRegistrationDate);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task SectionCoordinationService_CreateSectionCensusCertificationAsync_SectionToVerify_GetSection_ThrowsException()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionAsync(It.IsAny<string>())).Throws(new Exception("what happened!!"));
                await sectionCoordinationService.CreateSectionCensusCertificationAsync(sectionId, sectionCensusToCertify, sectionRegistrationDate);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task SectionCoordinationService_CreateSectionCensusCertificationAsync_SectionToVerify_GetSection_ReturnsNull()
            {
                Section sec = null;
                sectionRepoMock.Setup(repo => repo.GetSectionAsync(It.IsAny<string>())).ReturnsAsync(sec);
                await sectionCoordinationService.CreateSectionCensusCertificationAsync(sectionId, sectionCensusToCertify, sectionRegistrationDate);
            }
            //person is not the faculty in section
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task SectionCoordinationService_CreateSectionCensusCertificationAsync_SectionToVerify_CurrentUser_IsNotTheFaculty()
            {
                //setup Section
                // Section with  assigned faculty, so the current user is assigned faculty member
                Section secEntity = new Section(sectionId, "crsId", "num", new DateTime(2010, 1, 1), 3, null, "title", "Inst",
                        new List<OfferingDepartment> { new OfferingDepartment("DEPT1") }, new List<string> { "UG" }, "UG",
                        new List<SectionStatusItem> { new SectionStatusItem(SectionStatus.Active, "A", new DateTime(2010, 1, 1)) });
                secEntity.AddFaculty("whoAreYou");
                sectionRepoMock.Setup(repo => repo.GetSectionAsync(It.IsAny<string>())).ReturnsAsync(secEntity);
                await sectionCoordinationService.CreateSectionCensusCertificationAsync(sectionId, sectionCensusToCertify, sectionRegistrationDate);
            }

            //SectionToCertify position has leading whitespace
            [TestMethod]
            public async Task SectionCoordinationService_CreateSectionCensusCertificationAsync_SectionToVerify_Position_HasLeadingWhitespace()
            {
                sectionCensusToCertify.CensusCertificationPosition = "   1";

                Domain.Student.Entities.SectionCensusCertification certifiedCensus = new Domain.Student.Entities.SectionCensusCertification(
                    sectionCensusToCertify.CensusCertificationDate,
                    sectionCensusToCertify.CensusCertificationPosition.Trim(), 
                    sectionCensusToCertify.CensusCertificationLabel, 
                    sectionCensusToCertify.CensusCertificationRecordedDate, 
                    sectionCensusToCertify.CensusCertificationRecordedTime.Value.DateTime, 
                    "000678");

                // Section with  assigned faculty, so the current user is assigned faculty member
                Section secEntity = new Section(sectionId, "crsId", "num", new DateTime(2010, 1, 1), 3, null, "title", "Inst",
                        new List<OfferingDepartment> { new OfferingDepartment("DEPT1") }, new List<string> { "UG" }, "UG",
                        new List<SectionStatusItem> { new SectionStatusItem(SectionStatus.Active, "A", new DateTime(2010, 1, 1)) });
                secEntity.AddFaculty("0000678");
                sectionRegistrationDate.CensusDates = new List<DateTime?>();
                sectionRegistrationDate.CensusDates.Add(sectionCensusToCertify.CensusCertificationDate);
                sectionRepoMock.Setup(repo => repo.GetSectionAsync(It.IsAny<string>())).ReturnsAsync(secEntity);
                sectionRepoMock.Setup(repo => repo.CreateSectionCensusCertificationAsync(It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<DateTimeOffset>(), It.IsAny<string>())).ReturnsAsync(certifiedCensus);

                Dtos.Student.SectionCensusCertification certifiedCensusDto = await sectionCoordinationService.CreateSectionCensusCertificationAsync(sectionId, sectionCensusToCertify, sectionRegistrationDate);

                Assert.IsNotNull(certifiedCensusDto);
                Assert.AreEqual(sectionCensusToCertify.CensusCertificationDate, certifiedCensusDto.CensusCertificationDate);
                Assert.AreEqual(sectionCensusToCertify.CensusCertificationPosition, certifiedCensusDto.CensusCertificationPosition);
                Assert.AreEqual(sectionCensusToCertify.CensusCertificationLabel, certifiedCensusDto.CensusCertificationLabel);
                Assert.AreEqual(sectionCensusToCertify.CensusCertificationRecordedDate, certifiedCensusDto.CensusCertificationRecordedDate);
                Assert.AreEqual(sectionCensusToCertify.CensusCertificationRecordedTime.Value.DateTime, certifiedCensusDto.CensusCertificationRecordedTime);
            }

            //SectionToCertify position has trailing whitespace
            [TestMethod]
            public async Task SectionCoordinationService_CreateSectionCensusCertificationAsync_SectionToVerify_Position_HasTrailingWhitespace()
            {
                sectionCensusToCertify.CensusCertificationPosition = "1  ";

                Domain.Student.Entities.SectionCensusCertification certifiedCensus = new Domain.Student.Entities.SectionCensusCertification(
                    sectionCensusToCertify.CensusCertificationDate,
                    sectionCensusToCertify.CensusCertificationPosition.Trim(), 
                    sectionCensusToCertify.CensusCertificationLabel, 
                    sectionCensusToCertify.CensusCertificationRecordedDate, 
                    sectionCensusToCertify.CensusCertificationRecordedTime.Value.DateTime, 
                    "000678");

                // Section with  assigned faculty, so the current user is assigned faculty member
                Section secEntity = new Section(sectionId, "crsId", "num", new DateTime(2010, 1, 1), 3, null, "title", "Inst",
                        new List<OfferingDepartment> { new OfferingDepartment("DEPT1") }, new List<string> { "UG" }, "UG",
                        new List<SectionStatusItem> { new SectionStatusItem(SectionStatus.Active, "A", new DateTime(2010, 1, 1)) });
                secEntity.AddFaculty("0000678");
                sectionRegistrationDate.CensusDates = new List<DateTime?>();
                sectionRegistrationDate.CensusDates.Add(sectionCensusToCertify.CensusCertificationDate);
                sectionRepoMock.Setup(repo => repo.GetSectionAsync(It.IsAny<string>())).ReturnsAsync(secEntity);
                sectionRepoMock.Setup(repo => repo.CreateSectionCensusCertificationAsync(It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<DateTimeOffset>(), It.IsAny<string>())).ReturnsAsync(certifiedCensus);

                Dtos.Student.SectionCensusCertification certifiedCensusDto = await sectionCoordinationService.CreateSectionCensusCertificationAsync(sectionId, sectionCensusToCertify, sectionRegistrationDate);

                Assert.IsNotNull(certifiedCensusDto);
                Assert.AreEqual(sectionCensusToCertify.CensusCertificationDate, certifiedCensusDto.CensusCertificationDate);
                Assert.AreEqual(sectionCensusToCertify.CensusCertificationPosition, certifiedCensusDto.CensusCertificationPosition);
                Assert.AreEqual(sectionCensusToCertify.CensusCertificationLabel, certifiedCensusDto.CensusCertificationLabel);
                Assert.AreEqual(sectionCensusToCertify.CensusCertificationRecordedDate, certifiedCensusDto.CensusCertificationRecordedDate);
                Assert.AreEqual(sectionCensusToCertify.CensusCertificationRecordedTime.Value.DateTime, certifiedCensusDto.CensusCertificationRecordedTime);
            }

            //SectionToCertify position has leading and trailing whitespace
            [TestMethod]
            public async Task SectionCoordinationService_CreateSectionCensusCertificationAsync_SectionToVerify_Position_HasLeadingTrailingWhitespace()
            {
                sectionCensusToCertify.CensusCertificationPosition = "  1  ";

                Domain.Student.Entities.SectionCensusCertification certifiedCensus = new Domain.Student.Entities.SectionCensusCertification(
                    sectionCensusToCertify.CensusCertificationDate,
                    sectionCensusToCertify.CensusCertificationPosition.Trim(), 
                    sectionCensusToCertify.CensusCertificationLabel, 
                    sectionCensusToCertify.CensusCertificationRecordedDate, 
                    sectionCensusToCertify.CensusCertificationRecordedTime.Value.DateTime, 
                    "000678");

                // Section with  assigned faculty, so the current user is assigned faculty member
                Section secEntity = new Section(sectionId, "crsId", "num", new DateTime(2010, 1, 1), 3, null, "title", "Inst",
                        new List<OfferingDepartment> { new OfferingDepartment("DEPT1") }, new List<string> { "UG" }, "UG",
                        new List<SectionStatusItem> { new SectionStatusItem(SectionStatus.Active, "A", new DateTime(2010, 1, 1)) });
                secEntity.AddFaculty("0000678");
                sectionRegistrationDate.CensusDates = new List<DateTime?>();
                sectionRegistrationDate.CensusDates.Add(sectionCensusToCertify.CensusCertificationDate);
                sectionRepoMock.Setup(repo => repo.GetSectionAsync(It.IsAny<string>())).ReturnsAsync(secEntity);
                sectionRepoMock.Setup(repo => repo.CreateSectionCensusCertificationAsync(It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<DateTimeOffset>(), It.IsAny<string>())).ReturnsAsync(certifiedCensus);

                Dtos.Student.SectionCensusCertification certifiedCensusDto = await sectionCoordinationService.CreateSectionCensusCertificationAsync(sectionId, sectionCensusToCertify, sectionRegistrationDate);

                Assert.IsNotNull(certifiedCensusDto);
                Assert.AreEqual(sectionCensusToCertify.CensusCertificationDate, certifiedCensusDto.CensusCertificationDate);
                Assert.AreEqual(sectionCensusToCertify.CensusCertificationPosition, certifiedCensusDto.CensusCertificationPosition);
                Assert.AreEqual(sectionCensusToCertify.CensusCertificationLabel, certifiedCensusDto.CensusCertificationLabel);
                Assert.AreEqual(sectionCensusToCertify.CensusCertificationRecordedDate, certifiedCensusDto.CensusCertificationRecordedDate);
                Assert.AreEqual(sectionCensusToCertify.CensusCertificationRecordedTime.Value.DateTime, certifiedCensusDto.CensusCertificationRecordedTime);
            }

            //section is already certified for the census date provided with a census position number that includes leading spaces
            [TestMethod]
            [ExpectedException(typeof(Ellucian.Colleague.Domain.Base.Exceptions.ExistingResourceException))]
            public async Task SectionCoordinationService_CreateSectionCensusCertificationAsync_SectionToVerify_Already_Certified_Position_HasLeadingWhitespace()
            {
                sectionCensusToCertify.CensusCertificationPosition = "  1";

                // Section with  assigned faculty, so the current user is assigned faculty member
                Section secEntity = new Section(sectionId, "crsId", "num", new DateTime(2010, 1, 1), 3, null, "title", "Inst",
                        new List<OfferingDepartment> { new OfferingDepartment("DEPT1") }, new List<string> { "UG" }, "UG",
                        new List<SectionStatusItem> { new SectionStatusItem(SectionStatus.Active, "A", new DateTime(2010, 1, 1)) });
                secEntity.AddFaculty("0000678");
                secEntity.AddSectionCensusCertification(new Domain.Student.Entities.SectionCensusCertification(
                    sectionCensusToCertify.CensusCertificationDate,
                    sectionCensusToCertify.CensusCertificationPosition.Trim(),
                    sectionCensusToCertify.CensusCertificationLabel,
                    sectionCensusToCertify.CensusCertificationRecordedDate,
                    sectionCensusToCertify.CensusCertificationRecordedTime.Value.DateTime,
                    "0000678"));
                sectionRepoMock.Setup(repo => repo.GetSectionAsync(It.IsAny<string>())).ReturnsAsync(secEntity);
                await sectionCoordinationService.CreateSectionCensusCertificationAsync(sectionId, sectionCensusToCertify, sectionRegistrationDate);
            }

            //section is already certified for the census date provided with a census position number that includes trailing spaces
            [TestMethod]
            [ExpectedException(typeof(Ellucian.Colleague.Domain.Base.Exceptions.ExistingResourceException))]
            public async Task SectionCoordinationService_CreateSectionCensusCertificationAsync_SectionToVerify_Already_Certified_Position_HasTrailingWhitespace()
            {
                sectionCensusToCertify.CensusCertificationPosition = "1  ";

                // Section with  assigned faculty, so the current user is assigned faculty member
                Section secEntity = new Section(sectionId, "crsId", "num", new DateTime(2010, 1, 1), 3, null, "title", "Inst",
                        new List<OfferingDepartment> { new OfferingDepartment("DEPT1") }, new List<string> { "UG" }, "UG",
                        new List<SectionStatusItem> { new SectionStatusItem(SectionStatus.Active, "A", new DateTime(2010, 1, 1)) });
                secEntity.AddFaculty("0000678");
                secEntity.AddSectionCensusCertification(new Domain.Student.Entities.SectionCensusCertification(
                    sectionCensusToCertify.CensusCertificationDate, 
                    sectionCensusToCertify.CensusCertificationPosition.Trim(), 
                    sectionCensusToCertify.CensusCertificationLabel, 
                    sectionCensusToCertify.CensusCertificationRecordedDate, 
                    sectionCensusToCertify.CensusCertificationRecordedTime.Value.DateTime, 
                    "0000678"));
                sectionRepoMock.Setup(repo => repo.GetSectionAsync(It.IsAny<string>())).ReturnsAsync(secEntity);
                await sectionCoordinationService.CreateSectionCensusCertificationAsync(sectionId, sectionCensusToCertify, sectionRegistrationDate);
            }

            //section is already certified for the census date provided with a census position number that includes leading and trailing spaces
            [TestMethod]
            [ExpectedException(typeof(Ellucian.Colleague.Domain.Base.Exceptions.ExistingResourceException))]
            public async Task SectionCoordinationService_CreateSectionCensusCertificationAsync_SectionToVerify_Already_Certified_Position_HasLeadingTrailingWhitespace()
            {
                sectionCensusToCertify.CensusCertificationPosition = "  1  ";

                // Section with  assigned faculty, so the current user is assigned faculty member
                Section secEntity = new Section(sectionId, "crsId", "num", new DateTime(2010, 1, 1), 3, null, "title", "Inst",
                        new List<OfferingDepartment> { new OfferingDepartment("DEPT1") }, new List<string> { "UG" }, "UG",
                        new List<SectionStatusItem> { new SectionStatusItem(SectionStatus.Active, "A", new DateTime(2010, 1, 1)) });
                secEntity.AddFaculty("0000678");
                secEntity.AddSectionCensusCertification(new Domain.Student.Entities.SectionCensusCertification(
                    sectionCensusToCertify.CensusCertificationDate,
                    sectionCensusToCertify.CensusCertificationPosition.Trim(),
                    sectionCensusToCertify.CensusCertificationLabel,
                    sectionCensusToCertify.CensusCertificationRecordedDate,
                    sectionCensusToCertify.CensusCertificationRecordedTime.Value.DateTime,
                    "0000678"));
                sectionRepoMock.Setup(repo => repo.GetSectionAsync(It.IsAny<string>())).ReturnsAsync(secEntity);
                await sectionCoordinationService.CreateSectionCensusCertificationAsync(sectionId, sectionCensusToCertify, sectionRegistrationDate);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task SectionCoordinationService_CreateSectionCensusCertificationAsync_GetSectionCensusConfigurationAsync_ThrowsException()
            {
                // Section with  assigned faculty, so the current user is assigned faculty member
                Section secEntity = new Section(sectionId, "crsId", "num", new DateTime(2010, 1, 1), 3, null, "title", "Inst",
                        new List<OfferingDepartment> { new OfferingDepartment("DEPT1") }, new List<string> { "UG" }, "UG",
                        new List<SectionStatusItem> { new SectionStatusItem(SectionStatus.Active, "A", new DateTime(2010, 1, 1)) });
                secEntity.AddFaculty("0000678");
                secEntity.AddSectionCensusCertification(new Domain.Student.Entities.SectionCensusCertification(
                    sectionCensusToCertify.CensusCertificationDate, 
                    sectionCensusToCertify.CensusCertificationPosition.Trim(), 
                    sectionCensusToCertify.CensusCertificationLabel, 
                    sectionCensusToCertify.CensusCertificationRecordedDate, 
                    sectionCensusToCertify.CensusCertificationRecordedTime.Value.DateTime, 
                    "0000678"));
                sectionRepoMock.Setup(repo => repo.GetSectionAsync(It.IsAny<string>())).ReturnsAsync(secEntity);

                studentConfigRepoMock.Setup(repo => repo.GetSectionCensusConfigurationAsync()).Throws(new Exception("what happened!!"));
                await sectionCoordinationService.CreateSectionCensusCertificationAsync(sectionId, sectionCensusToCertify, sectionRegistrationDate);
            }

            [TestMethod]
            [ExpectedException(typeof(Ellucian.Colleague.Domain.Base.Exceptions.ConfigurationException))]
            public async Task SectionCoordinationService_CreateSectionCensusCertificationAsync_GetSectionCensusConfigurationAsync_ReturnsNull()
            {
                // Section with  assigned faculty, so the current user is assigned faculty member
                Section secEntity = new Section(sectionId, "crsId", "num", new DateTime(2010, 1, 1), 3, null, "title", "Inst",
                        new List<OfferingDepartment> { new OfferingDepartment("DEPT1") }, new List<string> { "UG" }, "UG",
                        new List<SectionStatusItem> { new SectionStatusItem(SectionStatus.Active, "A", new DateTime(2010, 1, 1)) });
                secEntity.AddFaculty("0000678");
                secEntity.AddSectionCensusCertification(new Domain.Student.Entities.SectionCensusCertification(
                    sectionCensusToCertify.CensusCertificationDate, 
                    sectionCensusToCertify.CensusCertificationPosition.Trim(), 
                    sectionCensusToCertify.CensusCertificationLabel, 
                    sectionCensusToCertify.CensusCertificationRecordedDate, 
                    sectionCensusToCertify.CensusCertificationRecordedTime.Value.DateTime, 
                    "0000678"));
                sectionRepoMock.Setup(repo => repo.GetSectionAsync(It.IsAny<string>())).ReturnsAsync(secEntity);

                Domain.Student.Entities.SectionCensusConfiguration sectionCensusConfig = null;
                studentConfigRepoMock.Setup(repo => repo.GetSectionCensusConfigurationAsync()).ReturnsAsync(sectionCensusConfig);

                await sectionCoordinationService.CreateSectionCensusCertificationAsync(sectionId, sectionCensusToCertify, sectionRegistrationDate);
            }

            //section is already certified for the census date provided
            [TestMethod]
            [ExpectedException(typeof(Ellucian.Colleague.Domain.Base.Exceptions.ExistingResourceException))]
            public async Task SectionCoordinationService_CreateSectionCensusCertificationAsync_SectionToVerify_Already_Certified()
            {
                // Section with  assigned faculty, so the current user is assigned faculty member
                Section secEntity = new Section(sectionId, "crsId", "num", new DateTime(2010, 1, 1), 3, null, "title", "Inst",
                        new List<OfferingDepartment> { new OfferingDepartment("DEPT1") }, new List<string> { "UG" }, "UG",
                        new List<SectionStatusItem> { new SectionStatusItem(SectionStatus.Active, "A", new DateTime(2010, 1, 1)) });
                secEntity.AddFaculty("0000678");
                secEntity.AddSectionCensusCertification(new Domain.Student.Entities.SectionCensusCertification(
                    sectionCensusToCertify.CensusCertificationDate, 
                    sectionCensusToCertify.CensusCertificationPosition.Trim(), 
                    sectionCensusToCertify.CensusCertificationLabel, 
                    sectionCensusToCertify.CensusCertificationRecordedDate, 
                    sectionCensusToCertify.CensusCertificationRecordedTime.Value.DateTime, 
                    "0000678"));
                sectionRepoMock.Setup(repo => repo.GetSectionAsync(It.IsAny<string>())).ReturnsAsync(secEntity);
                await sectionCoordinationService.CreateSectionCensusCertificationAsync(sectionId, sectionCensusToCertify, sectionRegistrationDate);
            }

            //census date to verify is not valid a census date- SectionregistrationDates does not have census date 
            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task SectionCoordinationService_CreateSectionCensusCertificationAsync_SectionToVerify_Has_IncorrectCensusDate()
            {
                // Section with  assigned faculty, so the current user is assigned faculty member
                Section secEntity = new Section(sectionId, "crsId", "num", new DateTime(2010, 1, 1), 3, null, "title", "Inst",
                        new List<OfferingDepartment> { new OfferingDepartment("DEPT1") }, new List<string> { "UG" }, "UG",
                        new List<SectionStatusItem> { new SectionStatusItem(SectionStatus.Active, "A", new DateTime(2010, 1, 1)) });
                secEntity.AddFaculty("0000678");
                sectionRegistrationDate.CensusDates = new List<DateTime?>();
                sectionRegistrationDate.CensusDates.Add(DateTime.Today);
                sectionRepoMock.Setup(repo => repo.GetSectionAsync(It.IsAny<string>())).ReturnsAsync(secEntity);
                await sectionCoordinationService.CreateSectionCensusCertificationAsync(sectionId, sectionCensusToCertify, sectionRegistrationDate);
            }

            //section repo to update census throws exception
            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task SectionCoordinationService_CreateSectionCensusCertificationAsync_RepoToCreateCensusCert_ThrowsException()
            {
                // Section with  assigned faculty, so the current user is assigned faculty member
                Section secEntity = new Section(sectionId, "crsId", "num", new DateTime(2010, 1, 1), 3, null, "title", "Inst",
                        new List<OfferingDepartment> { new OfferingDepartment("DEPT1") }, new List<string> { "UG" }, "UG",
                        new List<SectionStatusItem> { new SectionStatusItem(SectionStatus.Active, "A", new DateTime(2010, 1, 1)) });
                secEntity.AddFaculty("0000678");
                sectionRegistrationDate.CensusDates = new List<DateTime?>();
                sectionRegistrationDate.CensusDates.Add(sectionCensusToCertify.CensusCertificationDate);
                sectionRepoMock.Setup(repo => repo.GetSectionAsync(It.IsAny<string>())).ReturnsAsync(secEntity);
                sectionRepoMock.Setup(repo => repo.CreateSectionCensusCertificationAsync(It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<DateTimeOffset>(), It.IsAny<string>())).Throws(new Exception());
                await sectionCoordinationService.CreateSectionCensusCertificationAsync(sectionId, sectionCensusToCertify, sectionRegistrationDate);
            }

            // census certification recorded date is in the future
            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public async Task SectionCoordinationService_CreateSectionCensusCertificationAsync_RecordedDateIsFutureDate()
            {
                sectionCensusToCertify.CensusCertificationRecordedDate = DateTime.Today.AddDays(1);

                // Section with  assigned faculty, so the current user is assigned faculty member
                Section secEntity = new Section(sectionId, "crsId", "num", new DateTime(2010, 1, 1), 3, null, "title", "Inst",
                        new List<OfferingDepartment> { new OfferingDepartment("DEPT1") }, new List<string> { "UG" }, "UG",
                        new List<SectionStatusItem> { new SectionStatusItem(SectionStatus.Active, "A", new DateTime(2010, 1, 1)) });
                secEntity.AddFaculty("0000678");
                sectionRegistrationDate.CensusDates = new List<DateTime?>();
                sectionRegistrationDate.CensusDates.Add(DateTime.Today);
                sectionRepoMock.Setup(repo => repo.GetSectionAsync(It.IsAny<string>())).ReturnsAsync(secEntity);
                await sectionCoordinationService.CreateSectionCensusCertificationAsync(sectionId, sectionCensusToCertify, sectionRegistrationDate);
            }

            // census certification recorded date is before certification start date
            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public async Task SectionCoordinationService_CreateSectionCensusCertificationAsync_RecordedDateIsPriorToCertificationStartDate()
            {
                sectionCensusToCertify.CensusCertificationRecordedDate = new DateTime(2020, 12, 31);

                // Section with  assigned faculty, so the current user is assigned faculty member
                Section secEntity = new Section(sectionId, "crsId", "num", new DateTime(2010, 1, 1), 3, null, "title", "Inst",
                        new List<OfferingDepartment> { new OfferingDepartment("DEPT1") }, new List<string> { "UG" }, "UG",
                        new List<SectionStatusItem> { new SectionStatusItem(SectionStatus.Active, "A", new DateTime(2010, 1, 1)) });
                secEntity.AddFaculty("0000678");
                sectionRegistrationDate.CensusDates = new List<DateTime?>();
                sectionRegistrationDate.CensusDates.Add(DateTime.Today);
                sectionRepoMock.Setup(repo => repo.GetSectionAsync(It.IsAny<string>())).ReturnsAsync(secEntity);
                await sectionCoordinationService.CreateSectionCensusCertificationAsync(sectionId, sectionCensusToCertify, sectionRegistrationDate);
            }

            // census certification recorded date is on certification start date
            [TestMethod]
            public async Task SectionCoordinationService_CreateSectionCensusCertificationAsync_RecordedDateOnCertificationStartDate()
            {
                sectionCensusToCertify.CensusCertificationRecordedDate = new DateTime(2021, 01, 02);

                Domain.Student.Entities.SectionCensusCertification certifiedCensus = new Domain.Student.Entities.SectionCensusCertification(
                    sectionCensusToCertify.CensusCertificationDate,
                    sectionCensusToCertify.CensusCertificationPosition, 
                    sectionCensusToCertify.CensusCertificationLabel, 
                    sectionCensusToCertify.CensusCertificationRecordedDate, 
                    sectionCensusToCertify.CensusCertificationRecordedTime.Value.DateTime, 
                    "000678");

                // Section with  assigned faculty, so the current user is assigned faculty member
                Section secEntity = new Section(sectionId, "crsId", "num", new DateTime(2010, 1, 1), 3, null, "title", "Inst",
                        new List<OfferingDepartment> { new OfferingDepartment("DEPT1") }, new List<string> { "UG" }, "UG",
                        new List<SectionStatusItem> { new SectionStatusItem(SectionStatus.Active, "A", new DateTime(2010, 1, 1)) });
                secEntity.AddFaculty("0000678");
                sectionRegistrationDate.CensusDates = new List<DateTime?>();
                sectionRegistrationDate.CensusDates.Add(sectionCensusToCertify.CensusCertificationDate);
                sectionRepoMock.Setup(repo => repo.GetSectionAsync(It.IsAny<string>())).ReturnsAsync(secEntity);
                sectionRepoMock.Setup(repo => repo.CreateSectionCensusCertificationAsync(It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<DateTimeOffset>(), It.IsAny<string>())).ReturnsAsync(certifiedCensus);

                Dtos.Student.SectionCensusCertification certifiedCensusDto = await sectionCoordinationService.CreateSectionCensusCertificationAsync(sectionId, sectionCensusToCertify, sectionRegistrationDate);

                Assert.IsNotNull(certifiedCensusDto);
                Assert.AreEqual(sectionCensusToCertify.CensusCertificationDate, certifiedCensusDto.CensusCertificationDate);
                Assert.AreEqual(sectionCensusToCertify.CensusCertificationPosition, certifiedCensusDto.CensusCertificationPosition);
                Assert.AreEqual(sectionCensusToCertify.CensusCertificationLabel, certifiedCensusDto.CensusCertificationLabel);
                Assert.AreEqual(sectionCensusToCertify.CensusCertificationRecordedDate, certifiedCensusDto.CensusCertificationRecordedDate);
                Assert.AreEqual(sectionCensusToCertify.CensusCertificationRecordedTime.Value.DateTime, certifiedCensusDto.CensusCertificationRecordedTime);
            }

            // census certification recorded date after certification start date
            [TestMethod]
            public async Task SectionCoordinationService_CreateSectionCensusCertificationAsync_RecordedDateAfterCertificationStartDate()
            {
                sectionCensusToCertify.CensusCertificationRecordedDate = new DateTime(2021, 02, 02);
                Domain.Student.Entities.SectionCensusCertification certifiedCensus = new Domain.Student.Entities.SectionCensusCertification(
                    sectionCensusToCertify.CensusCertificationDate,
                    sectionCensusToCertify.CensusCertificationPosition,
                    sectionCensusToCertify.CensusCertificationLabel,
                    sectionCensusToCertify.CensusCertificationRecordedDate,
                    sectionCensusToCertify.CensusCertificationRecordedTime.Value.DateTime,
                    "000678");

                // Section with  assigned faculty, so the current user is assigned faculty member
                Section secEntity = new Section(sectionId, "crsId", "num", new DateTime(2010, 1, 1), 3, null, "title", "Inst",
                        new List<OfferingDepartment> { new OfferingDepartment("DEPT1") }, new List<string> { "UG" }, "UG",
                        new List<SectionStatusItem> { new SectionStatusItem(SectionStatus.Active, "A", new DateTime(2010, 1, 1)) });
                secEntity.AddFaculty("0000678");
                sectionRegistrationDate.CensusDates = new List<DateTime?>();
                sectionRegistrationDate.CensusDates.Add(sectionCensusToCertify.CensusCertificationDate);
                sectionRepoMock.Setup(repo => repo.GetSectionAsync(It.IsAny<string>())).ReturnsAsync(secEntity);
                sectionRepoMock.Setup(repo => repo.CreateSectionCensusCertificationAsync(It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<DateTimeOffset>(), It.IsAny<string>())).ReturnsAsync(certifiedCensus);

                Dtos.Student.SectionCensusCertification certifiedCensusDto = await sectionCoordinationService.CreateSectionCensusCertificationAsync(sectionId, sectionCensusToCertify, sectionRegistrationDate);

                Assert.IsNotNull(certifiedCensusDto);
                Assert.AreEqual(sectionCensusToCertify.CensusCertificationDate, certifiedCensusDto.CensusCertificationDate);
                Assert.AreEqual(sectionCensusToCertify.CensusCertificationPosition, certifiedCensusDto.CensusCertificationPosition);
                Assert.AreEqual(sectionCensusToCertify.CensusCertificationLabel, certifiedCensusDto.CensusCertificationLabel);
                Assert.AreEqual(sectionCensusToCertify.CensusCertificationRecordedDate, certifiedCensusDto.CensusCertificationRecordedDate);
                Assert.AreEqual(sectionCensusToCertify.CensusCertificationRecordedTime.Value.DateTime, certifiedCensusDto.CensusCertificationRecordedTime);
            }


            //section repo to update census returns valid sectionCensuCertification
            [TestMethod]
            public async Task SectionCoordinationService_CreateSectionCensusCertificationAsync_RepoToCreateCensusCert_Success()
            {
                Domain.Student.Entities.SectionCensusCertification certifiedCensus = new Domain.Student.Entities.SectionCensusCertification(sectionCensusToCertify.CensusCertificationDate,
                    sectionCensusToCertify.CensusCertificationPosition, sectionCensusToCertify.CensusCertificationLabel, sectionCensusToCertify.CensusCertificationRecordedDate, sectionCensusToCertify.CensusCertificationRecordedTime.Value.DateTime, "000678");

                // Section with  assigned faculty, so the current user is assigned faculty member
                Section secEntity = new Section(sectionId, "crsId", "num", new DateTime(2010, 1, 1), 3, null, "title", "Inst",
                        new List<OfferingDepartment> { new OfferingDepartment("DEPT1") }, new List<string> { "UG" }, "UG",
                        new List<SectionStatusItem> { new SectionStatusItem(SectionStatus.Active, "A", new DateTime(2010, 1, 1)) });
                secEntity.AddFaculty("0000678");
                sectionRegistrationDate.CensusDates = new List<DateTime?>();
                sectionRegistrationDate.CensusDates.Add(sectionCensusToCertify.CensusCertificationDate);
                sectionRepoMock.Setup(repo => repo.GetSectionAsync(It.IsAny<string>())).ReturnsAsync(secEntity);
                sectionRepoMock.Setup(repo => repo.CreateSectionCensusCertificationAsync(It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<DateTimeOffset>(), It.IsAny<string>())).ReturnsAsync(certifiedCensus);

                Dtos.Student.SectionCensusCertification certifiedCensusDto = await sectionCoordinationService.CreateSectionCensusCertificationAsync(sectionId, sectionCensusToCertify, sectionRegistrationDate);

                Assert.IsNotNull(certifiedCensusDto);
                Assert.AreEqual(sectionCensusToCertify.CensusCertificationDate, certifiedCensusDto.CensusCertificationDate);
                Assert.AreEqual(sectionCensusToCertify.CensusCertificationPosition, certifiedCensusDto.CensusCertificationPosition);
                Assert.AreEqual(sectionCensusToCertify.CensusCertificationLabel, certifiedCensusDto.CensusCertificationLabel);
                Assert.AreEqual(sectionCensusToCertify.CensusCertificationRecordedDate, certifiedCensusDto.CensusCertificationRecordedDate);
                Assert.AreEqual(sectionCensusToCertify.CensusCertificationRecordedTime.Value.DateTime, certifiedCensusDto.CensusCertificationRecordedTime);
            }
        }
    }
}
