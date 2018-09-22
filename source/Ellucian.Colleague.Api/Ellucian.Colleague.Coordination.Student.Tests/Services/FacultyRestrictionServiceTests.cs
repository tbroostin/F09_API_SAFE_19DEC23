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
    public class FacultyRestrictionServiceTests
    {
        // Sets up a Current user that is a student and one that is an advisor
        public abstract class CurrentUserSetup
        {
            public class FacultyF001UserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "Fred",
                            PersonId = "F001",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "Faculty",
                            Roles = new List<string>() { "Advisor" },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }
            public class FacultyF002UserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "Fred",
                            PersonId = "F002",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "Faculty2",
                            Roles = new List<string>() { "Advisor" },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }
        }

        [TestClass]
        public class Get_F001
        {
            private FacultyRestrictionService facultyRestrictionService;
            private Mock<IFacultyRepository> facRepoMock;
            private IFacultyRepository facRepo;
            private Mock<IPersonRestrictionRepository> stuRestrRepoMock;
            private IPersonRestrictionRepository stuRestrRepo;
            private Mock<IReferenceDataRepository> refDataRepoMock;
            private IReferenceDataRepository refDataRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Domain.Student.Entities.Faculty faculty1;
            private Domain.Student.Entities.Faculty faculty2;
            private Domain.Base.Entities.Restriction restriction1;
            private Domain.Base.Entities.Restriction restriction2;
            private Domain.Base.Entities.PersonRestriction studentRestriction1;
            private Domain.Base.Entities.PersonRestriction studentRestriction2;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private IStudentRepository studentRepo;
            private Mock<IStudentRepository> studentRepoMock;

            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;
            
            private ICurrentUserFactory currentUserFactory;

            [TestInitialize]
            public void Initialize()
            {
                facRepoMock = new Mock<IFacultyRepository>();
                facRepo = facRepoMock.Object;
                stuRestrRepoMock = new Mock<IPersonRestrictionRepository>();
                stuRestrRepo = stuRestrRepoMock.Object;
                refDataRepoMock = new Mock<IReferenceDataRepository>();
                refDataRepo = refDataRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;


                // mock the first faculty
                faculty1 = new Domain.Student.Entities.Faculty("F001", "Klemperer");
                facRepoMock.Setup(r => r.GetAsync("F001")).Returns(Task.FromResult(faculty1));
                faculty2 = new Domain.Student.Entities.Faculty("F002", "Banner");
                facRepoMock.Setup(r => r.GetAsync("F002")).Returns(Task.FromResult(faculty2));
                // mock the restrictions in ref data repo
                restriction1 = new Domain.Base.Entities.Restriction(Guid.NewGuid().ToString(), "R001", "Restriction1", (int?)4, "Y", "First Restriction", "First Restriction Details", "ST", "WMPT", "ST001", "Pay Me", "N");
                restriction1.Hyperlink = "http://www.someschool.edu.link1";
                restriction2 = new Domain.Base.Entities.Restriction(Guid.NewGuid().ToString(), "R002", "Restriction2", null, "Y", "Second Restriction", "Second Restriction Details", "ST", "WRGS", "ST002", "Add/Drop", "N");
                restriction2.Hyperlink = "http://www.someschool.edu/link2";
                IEnumerable<Restriction> restList = new List<Domain.Base.Entities.Restriction>() { restriction1, restriction2 };
                refDataRepoMock.Setup(r => r.RestrictionsAsync()).ReturnsAsync(restList);
                // mock the student restrictions for the second faculty
                studentRestriction1 = new Domain.Base.Entities.PersonRestriction("SR001", "F002", "R001", new DateTime(2012, 12, 20), null, (int?)4, "Y");
                studentRestriction2 = new Domain.Base.Entities.PersonRestriction("SR002", "F002", "R002", new DateTime(2012, 12, 22), null, null, "Y");
                IEnumerable<Domain.Base.Entities.PersonRestriction> stu2RestList = new List<Domain.Base.Entities.PersonRestriction>() { studentRestriction1, studentRestriction2 };

                // setup a current user
                currentUserFactory = new CurrentUserSetup.FacultyF001UserFactory();

                facultyRestrictionService = new FacultyRestrictionService(adapterRegistry, refDataRepo, facRepo, stuRestrRepo, currentUserFactory, roleRepo, logger, studentRepo, baseConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                facRepo = null;
                stuRestrRepo = null;
                refDataRepo = null;
                adapterRegistry = null;
                currentUserFactory = null;
                roleRepo = null;
                logger = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task ThrowsIfFacultyNotFound()
            {
                Domain.Student.Entities.Faculty faculty = null;
                facRepoMock.Setup(facRepo => facRepo.GetAsync("9999999")).Returns(Task.FromResult(faculty));
               await facultyRestrictionService.GetFacultyRestrictionsAsync("9999999");
            }
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task ThrowsErrorIfNotSelf()
            {
                await facultyRestrictionService.GetFacultyRestrictionsAsync("F002");
            }

            [TestMethod]
            public async Task ReturnsEmptyListIfNoRestrictions()
            {
                var results = await facultyRestrictionService.GetFacultyRestrictionsAsync("F001");
                Assert.AreEqual(0, results.Count());
            }

        }

        [TestClass]
        public class Get_F002
        {
            private FacultyRestrictionService facultyRestrictionService;
            private Mock<IFacultyRepository> facRepoMock;
            private IFacultyRepository facRepo;
            private Mock<IPersonRestrictionRepository> stuRestrRepoMock;
            private IPersonRestrictionRepository stuRestrRepo;
            private Mock<IReferenceDataRepository> refDataRepoMock;
            private IReferenceDataRepository refDataRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Domain.Student.Entities.Faculty faculty2;
            private Domain.Base.Entities.Restriction restriction1;
            private Domain.Base.Entities.Restriction restriction2;
            private Domain.Base.Entities.PersonRestriction studentRestriction1;
            private Domain.Base.Entities.PersonRestriction studentRestriction2;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private IStudentRepository studentRepo;
            private Mock<IStudentRepository> studentRepoMock;

            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            private ICurrentUserFactory currentUserFactory;

            [TestInitialize]
            public void Initialize()
            {
                facRepoMock = new Mock<IFacultyRepository>();
                facRepo = facRepoMock.Object;
                stuRestrRepoMock = new Mock<IPersonRestrictionRepository>();
                stuRestrRepo = stuRestrRepoMock.Object;
                refDataRepoMock = new Mock<IReferenceDataRepository>();
                refDataRepo = refDataRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;

                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                logger = new Mock<ILogger>().Object;

                // mock the second faculty
                faculty2 = new Domain.Student.Entities.Faculty("F002", "Banner");
                facRepoMock.Setup(r => r.GetAsync("F002")).Returns(Task.FromResult(faculty2));
                // mock the restrictions in ref data repo
                restriction1 = new Domain.Base.Entities.Restriction(Guid.NewGuid().ToString(), "R001", "Restriction1", (int?)4, "Y", "First Restriction", "First Restriction Details", "ST", "WMPT", "ST001", "Pay Me", "N");
                restriction1.Hyperlink = "http://www.someschool.edu.link1";
                restriction2 = new Domain.Base.Entities.Restriction(Guid.NewGuid().ToString(), "R002", "Restriction2", null, "Y", "Second Restriction", "Second Restriction Details", "ST", "WRGS", "ST002", "Add/Drop", "N");
                restriction2.Hyperlink = "http://www.someschool.edu/link2";
                IEnumerable<Restriction> restList = new List<Domain.Base.Entities.Restriction>() { restriction1, restriction2 };
                refDataRepoMock.Setup(r => r.RestrictionsAsync()).ReturnsAsync(restList);
                // mock the student restrictions for the second student
                studentRestriction1 = new Domain.Base.Entities.PersonRestriction("SR001", "F002", "R001", new DateTime(2012, 12, 20), null, (int?)4, "Y");
                studentRestriction2 = new Domain.Base.Entities.PersonRestriction("SR002", "F002", "R002", new DateTime(2012, 12, 22), null, null, "Y");
                IEnumerable<Domain.Base.Entities.PersonRestriction> stu2RestList = new List<Domain.Base.Entities.PersonRestriction>() { studentRestriction1, studentRestriction2 };
                stuRestrRepoMock.Setup(r => r.GetAsync("F002", true)).ReturnsAsync(stu2RestList);

                // setup a current user
                currentUserFactory = new CurrentUserSetup.FacultyF002UserFactory();

                facultyRestrictionService = new FacultyRestrictionService(adapterRegistry, refDataRepo, facRepo, stuRestrRepo, currentUserFactory, roleRepo, logger, studentRepo, baseConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                facRepo = null;
                stuRestrRepo = null;
                refDataRepo = null;
                adapterRegistry = null;
                currentUserFactory = null;
                roleRepo = null;
                logger = null;
            }

            [TestMethod]
            public async Task ReturnsMultipleRestrictions()
            {
                var results = await facultyRestrictionService.GetFacultyRestrictionsAsync("F002");
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
