// Copyright 2012-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Text;
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
    public class StudentRestrictionServiceTests
    {
        // Sets up a Current user that is a student and one that is an advisor
        public abstract class CurrentUserSetup
        {
            protected Role advisorRole = new Role(105, "Advisor");

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
                            ControlId = "123",
                            Name = "Sally",
                            PersonId = "S002",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "Student",
                            Roles = new List<string>() { "Student" },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }

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
        public class Get_AsStudentUser : CurrentUserSetup
        {
            private StudentRestrictionService studentRestrictionService;
            private Mock<IPersonRepository> perRepoMock;
            private IPersonRepository perRepo;
            private Mock<IStudentRepository> stuRepoMock;
            private IStudentRepository stuRepo;
            private Mock<IPersonRestrictionRepository> perRestrRepoMock;
            private IPersonRestrictionRepository perRestrRepo;
            private Mock<IReferenceDataRepository> refDataRepoMock;
            private IReferenceDataRepository refDataRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private Domain.Student.Entities.Student student1;
            private Domain.Student.Entities.Student student2;
            private Domain.Base.Entities.Restriction restriction1;
            private Domain.Base.Entities.Restriction restriction2;
            private Domain.Base.Entities.PersonRestriction studentRestriction1;
            private Domain.Base.Entities.PersonRestriction studentRestriction2;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            [TestInitialize]
            public void Initialize()
            {
                perRepoMock = new Mock<IPersonRepository>();
                perRepo = perRepoMock.Object;
                stuRepoMock = new Mock<IStudentRepository>();
                stuRepo = stuRepoMock.Object;
                perRestrRepoMock = new Mock<IPersonRestrictionRepository>();
                perRestrRepo = perRestrRepoMock.Object;
                refDataRepoMock = new Mock<IReferenceDataRepository>();
                refDataRepo = refDataRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                logger = new Mock<ILogger>().Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                // mock the first student
                student1 = new Domain.Student.Entities.Student("S001", "Klemperer", null, new List<string>(), new List<string>());
                stuRepoMock.Setup(r => r.Get("S001")).Returns(student1);
                
                // mock the second student
                student2 = new Domain.Student.Entities.Student("S002", "Banner", null, new List<string>(), new List<string>());
                student2.AddStudentRestriction("SR001");
                student2.AddStudentRestriction("SR002");
                stuRepoMock.Setup(r => r.Get("S002")).Returns(student2);
                // mock the restrictions in ref data repo
                restriction1 = new Domain.Base.Entities.Restriction(Guid.NewGuid().ToString(), "R001", "Restriction1", (int?)4, "Y", "First Restriction", "First Restriction Details", "ST", "WMPT", "ST001", "Pay Me", "N");
                restriction1.Hyperlink = "http://www.someschool.edu.link1";
                restriction2 = new Domain.Base.Entities.Restriction(Guid.NewGuid().ToString(), "R002", "Restriction2", null, "Y", "Second Restriction", "Second Restriction Details", "ST", "WRGS", "ST002", "Add/Drop", "N");
                restriction2.Hyperlink = "http://www.someschool.edu/link2";
                IEnumerable<Restriction> restList = new List<Domain.Base.Entities.Restriction>() { restriction1, restriction2 };
                refDataRepoMock.Setup(r => r.RestrictionsAsync()).ReturnsAsync(restList);
                // mock the student restrictions for the second student - first student should have none.
                studentRestriction1 = new Domain.Base.Entities.PersonRestriction("SR001", "S002", "R001", new DateTime(2012, 12, 20), null, (int?)4, "Y");
                studentRestriction2 = new Domain.Base.Entities.PersonRestriction("SR002", "S002", "R002", new DateTime(2012, 12, 22), null, null,    "Y");
                IEnumerable<Domain.Base.Entities.PersonRestriction> stu2RestList = new List<Domain.Base.Entities.PersonRestriction>() { studentRestriction1, studentRestriction2 };

                // setup a current user
                currentUserFactory = new CurrentUserSetup.Student001UserFactory();

                IEnumerable<string> stu2RestrIds = new List<string>() { "SR001", "SR002" };
                perRestrRepoMock.Setup(r => r.GetAsync("S002", true)).ReturnsAsync(stu2RestList);

                // mock the response for a student with no restrictions
                perRestrRepoMock.Setup(repo => repo.GetAsync("S001", true)).ReturnsAsync(new List<Domain.Base.Entities.PersonRestriction>());

                studentRestrictionService = new StudentRestrictionService(adapterRegistry, refDataRepo, stuRepo, perRepo, perRestrRepo, currentUserFactory, roleRepo, logger, baseConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                stuRepo = null;
                perRestrRepo = null;
                refDataRepo = null;
                adapterRegistry = null;
                logger = null;
                currentUserFactory = null;
                studentRestrictionService = null;
            }

            [TestMethod]
            public async Task ReturnsEmptyListIfNoRestrictions()
            {
                var results = await studentRestrictionService.GetStudentRestrictionsAsync("S001");
                Assert.AreEqual(0, results.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task StudentNotSelf_ThrowsPermissionError()
            {
                var results =await studentRestrictionService.GetStudentRestrictionsAsync("S002");
            }

            [TestMethod]
            public async Task ReturnsMultipleRestrictions()
            {
                currentUserFactory = new CurrentUserSetup.Student002UserFactory();
                studentRestrictionService = new StudentRestrictionService(adapterRegistry, refDataRepo, stuRepo, perRepo, perRestrRepo, currentUserFactory, roleRepo, logger, baseConfigurationRepository);

                var results = await studentRestrictionService.GetStudentRestrictionsAsync("S002");
                Assert.AreEqual(2, results.Count());

                Assert.AreEqual(restriction1.Title, results.Where(r => r.Id == "SR001").First().Title);
                Assert.AreEqual(studentRestriction1.StartDate, results.Where(r => r.Id == "SR001").First().StartDate);
                Assert.AreEqual(restriction1.Details, results.Where(r => r.Id == "SR001").First().Details);
                Assert.AreEqual(restriction1.Hyperlink, results.Where(r => r.Id == "SR001").First().Hyperlink);
                Assert.AreEqual(restriction1.FollowUpLabel, results.Where(r => r.Id == "SR001").First().HyperlinkText);

                Assert.AreEqual(restriction2.Title, results.Where(r => r.Id == "SR002").First().Title);
                Assert.AreEqual(studentRestriction2.StartDate, results.Where(r => r.Id == "SR002").First().StartDate);
                Assert.AreEqual(restriction2.Details, results.Where(r => r.Id == "SR002").First().Details);
                Assert.AreEqual(restriction2.Hyperlink, results.Where(r => r.Id == "SR002").First().Hyperlink);
                Assert.AreEqual(restriction2.FollowUpLabel, results.Where(r => r.Id == "SR002").First().HyperlinkText);

            }
        }

        [TestClass]
        public class Get_AsAdvisorUser : CurrentUserSetup
        {
            private Mock<IPersonRepository> perRepoMock;
            private IPersonRepository perRepo;
            private StudentRestrictionService studentRestrictionService;
            private Mock<IStudentRepository> stuRepoMock;
            private IStudentRepository stuRepo;
            private Mock<IPersonRestrictionRepository> perRestrRepoMock;
            private IPersonRestrictionRepository perRestrRepo;
            private Mock<IReferenceDataRepository> refDataRepoMock;
            private IReferenceDataRepository refDataRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private Domain.Student.Entities.Student student1;
            private Domain.Student.Entities.Student student2;
            private Domain.Base.Entities.Restriction restriction1;
            private Domain.Base.Entities.Restriction restriction2;
            private Domain.Base.Entities.PersonRestriction studentRestriction1;
            private Domain.Base.Entities.PersonRestriction studentRestriction2;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            [TestInitialize]
            public void Initialize()
            {
                perRepoMock = new Mock<IPersonRepository>();
                perRepo = perRepoMock.Object;
                stuRepoMock = new Mock<IStudentRepository>();
                stuRepo = stuRepoMock.Object;
                perRestrRepoMock = new Mock<IPersonRestrictionRepository>();
                perRestrRepo = perRestrRepoMock.Object;
                refDataRepoMock = new Mock<IReferenceDataRepository>();
                refDataRepo = refDataRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                logger = new Mock<ILogger>().Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                // mock the first student
                student1 = new Domain.Student.Entities.Student("S001", "Klemperer", null, new List<string>(), new List<string>());
                stuRepoMock.Setup(r => r.Get("S001")).Returns(student1);
                // mock the second student
                student2 = new Domain.Student.Entities.Student("S002", "Banner", null, new List<string>(), new List<string>());
                student2.AddStudentRestriction("SR001");
                student2.AddStudentRestriction("SR002");
                student2.AddAdvisor("0000111");
                stuRepoMock.Setup(r => r.Get("S002")).Returns(student2);
                // mock the restrictions in ref data repo
                restriction1 = new Domain.Base.Entities.Restriction(Guid.NewGuid().ToString(), "R001", "Restriction1", (int?)4, "Y", "First Restriction", "First Restriction Details", "ST", "WMPT", "ST001", "Pay Me", "N");
                restriction1.Hyperlink = "http://www.someschool.edu.link1";
                restriction2 = new Domain.Base.Entities.Restriction(Guid.NewGuid().ToString(), "R002", "Restriction2", null, "Y", "Second Restriction", "Second Restriction Details", "ST", "WRGS", "ST002", "Add/Drop", "N");
                restriction2.Hyperlink = "http://www.someschool.edu/link2";
                IEnumerable<Restriction> restList = new List<Domain.Base.Entities.Restriction>() { restriction1, restriction2 };
                refDataRepoMock.Setup(r => r.RestrictionsAsync()).ReturnsAsync(restList);
                // mock the student restrictions for the second student - first student should have none.
                studentRestriction1 = new Domain.Base.Entities.PersonRestriction("SR001", "S002", "R001", new DateTime(2012, 12, 20), null, (int?)4, "Y");
                studentRestriction2 = new Domain.Base.Entities.PersonRestriction("SR002", "S002", "R002", new DateTime(2012, 12, 22), null, null, "Y");
                IEnumerable<Domain.Base.Entities.PersonRestriction> stu2RestList = new List<Domain.Base.Entities.PersonRestriction>() { studentRestriction1, studentRestriction2 };

                // setup a current user
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();

                IEnumerable<string> stu2RestrIds = new List<string>() { "SR001", "SR002" };
                perRestrRepoMock.Setup(r => r.GetAsync("S002",true)).ReturnsAsync(stu2RestList);

                // mock the response for a student with no restrictions
                perRestrRepoMock.Setup(repo => repo.GetAsync("S001", true)).ReturnsAsync(new List<Domain.Base.Entities.PersonRestriction>());
                // mock the response for multiple students
                perRestrRepoMock.Setup(repo => repo.GetRestrictionsByStudentIdsAsync(new List<string> { "S001", "S002" })).ReturnsAsync(stu2RestList);
                studentRestrictionService = new StudentRestrictionService(adapterRegistry, refDataRepo, stuRepo, perRepo, perRestrRepo, currentUserFactory, roleRepo, logger, baseConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                stuRepo = null;
                perRestrRepo = null;
                refDataRepo = null;
                adapterRegistry = null;
                logger = null;
                currentUserFactory = null;
                studentRestrictionService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task AdvisorWithoutPermission_ThrowsPermissionError()
            {

                var results =await studentRestrictionService.GetStudentRestrictionsAsync("S001");
            }

            [TestMethod]
            public async Task AdvisorWithViewAny_ReturnsMultipleRestrictions()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
 
                var results = await studentRestrictionService.GetStudentRestrictionsAsync("S002");
                Assert.AreEqual(2, results.Count());

                Assert.AreEqual(restriction1.Title, results.Where(r => r.Id == "SR001").First().Title);
                Assert.AreEqual(studentRestriction1.StartDate, results.Where(r => r.Id == "SR001").First().StartDate);
                Assert.AreEqual(restriction1.Details, results.Where(r => r.Id == "SR001").First().Details);
                Assert.AreEqual(restriction1.Hyperlink, results.Where(r => r.Id == "SR001").First().Hyperlink);
                Assert.AreEqual(restriction1.FollowUpLabel, results.Where(r => r.Id == "SR001").First().HyperlinkText);

                Assert.AreEqual(restriction2.Title, results.Where(r => r.Id == "SR002").First().Title);
                Assert.AreEqual(studentRestriction2.StartDate, results.Where(r => r.Id == "SR002").First().StartDate);
                Assert.AreEqual(restriction2.Details, results.Where(r => r.Id == "SR002").First().Details);
                Assert.AreEqual(restriction2.Hyperlink, results.Where(r => r.Id == "SR002").First().Hyperlink);
                Assert.AreEqual(restriction2.FollowUpLabel, results.Where(r => r.Id == "SR002").First().HyperlinkText);
            }

            [TestMethod]
            public async Task GetStudentRestrictions2Async_UserWithViewPersonRestriction_ReturnsMultipleRestrictions()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Domain.Base.BasePermissionCodes.ViewPersonRestrictions));
                roleRepoMock.SetupGet(rpm => rpm.Roles).Returns(new List<Role>() { advisorRole });
 
                var results = await studentRestrictionService.GetStudentRestrictions2Async("S002");
                Assert.AreEqual(2, results.Count());

                Assert.AreEqual(restriction1.Title, results.Where(r => r.Id == "SR001").First().Title);
                Assert.AreEqual(studentRestriction1.StartDate, results.Where(r => r.Id == "SR001").First().StartDate);
                Assert.AreEqual(restriction1.Details, results.Where(r => r.Id == "SR001").First().Details);
                Assert.AreEqual(restriction1.Hyperlink, results.Where(r => r.Id == "SR001").First().Hyperlink);
                Assert.AreEqual(restriction1.FollowUpLabel, results.Where(r => r.Id == "SR001").First().HyperlinkText);

                Assert.AreEqual(restriction2.Title, results.Where(r => r.Id == "SR002").First().Title);
                Assert.AreEqual(studentRestriction2.StartDate, results.Where(r => r.Id == "SR002").First().StartDate);
                Assert.AreEqual(restriction2.Details, results.Where(r => r.Id == "SR002").First().Details);
                Assert.AreEqual(restriction2.Hyperlink, results.Where(r => r.Id == "SR002").First().Hyperlink);
                Assert.AreEqual(restriction2.FollowUpLabel, results.Where(r => r.Id == "SR002").First().HyperlinkText);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task GetStudentRestrictions2Async_UserWithoutViewPersonRestriction_ThrowsException()
            {
                // User does not have any permissions
                roleRepoMock.SetupGet(rpm => rpm.Roles).Returns(new List<Role>() { advisorRole });
 
                var results = await studentRestrictionService.GetStudentRestrictions2Async("S002");
            }

            [TestMethod]
            public async Task ReturnsMultiple_ForMultipleStudents()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewStudentInformation));
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() { advisorRole });

                var results = await studentRestrictionService.GetStudentRestrictionsByStudentIdsAsync(new List<string> { "S001", "S002"} );
                Assert.AreEqual(2, results.Count());

                Assert.AreEqual(restriction1.Title, results.Where(r => r.Id == "SR001").First().Title);
                Assert.AreEqual(studentRestriction1.StartDate, results.Where(r => r.Id == "SR001").First().StartDate);
                Assert.AreEqual(restriction1.Details, results.Where(r => r.Id == "SR001").First().Details);
                Assert.AreEqual(restriction1.Hyperlink, results.Where(r => r.Id == "SR001").First().Hyperlink);
                Assert.AreEqual(restriction1.FollowUpLabel, results.Where(r => r.Id == "SR001").First().HyperlinkText);

                Assert.AreEqual(restriction2.Title, results.Where(r => r.Id == "SR002").First().Title);
                Assert.AreEqual(studentRestriction2.StartDate, results.Where(r => r.Id == "SR002").First().StartDate);
                Assert.AreEqual(restriction2.Details, results.Where(r => r.Id == "SR002").First().Details);
                Assert.AreEqual(restriction2.Hyperlink, results.Where(r => r.Id == "SR002").First().Hyperlink);
                Assert.AreEqual(restriction2.FollowUpLabel, results.Where(r => r.Id == "SR002").First().HyperlinkText);

            }

        }
    }
}
