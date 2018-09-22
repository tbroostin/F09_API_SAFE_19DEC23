// Copyright 2016-2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Student.Adapters;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Tests;
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
    public class AcademicHistoryServiceTests
    {
        // Sets up a Current user that is a student and one that is an advisor
        public abstract class CurrentUserSetup
        {
            protected Role advisorRole = new Role(105, "Advisor");

            public class StudentUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "Johnny",
                            PersonId = "0000894",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "Student",
                            Roles = new List<string>() { },
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
        public class GetHistory_AsStudentUser : CurrentUserSetup
        {
            private AcademicHistoryService academicHistoryService;
            private Mock<IAcademicCreditRepository> academicCreditRepoMock;
            private IAcademicCreditRepository academicCreditRepo;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Domain.Student.Entities.Student student1;
            private Domain.Student.Entities.Student student2;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private ITermRepository termRepo;
            private ISectionRepository sectionRepo;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            [TestInitialize]
            public void Initialize()
            {
                academicCreditRepoMock = new Mock<IAcademicCreditRepository>();
                academicCreditRepo = academicCreditRepoMock.Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                // Mock good student repo response
                List<string> academicCreditIds = new List<string>() { "1", "2", "3", "4" };
                student1 = new Domain.Student.Entities.Student("0000894", "Smith", 2, new List<string>() { "BA.ENGL" }, academicCreditIds) { FirstName = "Bob", MiddleName = "Blakely" };
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);

                IEnumerable<Domain.Student.Entities.AcademicCredit> academicCredits = new TestAcademicCreditRepository().GetAsync(academicCreditIds).Result;
                academicCreditRepoMock.Setup(repo => repo.GetAsync(academicCreditIds, false, true, It.IsAny<bool>())).Returns(Task.FromResult(academicCredits));

                // Mock another student ....
                student2 = new Domain.Student.Entities.Student("00004002", "Jones", 802, new List<string>() { "BA.MATH" }, academicCreditIds);
                studentRepoMock.Setup(repo => repo.GetAsync("00004002")).ReturnsAsync(student2);

                // Mock get grade restriction
                GradeRestriction studentGradeRestriction = new GradeRestriction(true);
                studentGradeRestriction.AddReason("Bad");
                studentRepoMock.Setup(srepo => srepo.GetGradeRestrictionsAsync(It.IsAny<string>())).Returns(Task.FromResult(studentGradeRestriction));

                // Mock Adapters
                var academicHistoryDtoAdapter = new AcademicHistoryEntityAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicHistory, Ellucian.Colleague.Dtos.Student.AcademicHistory>()).Returns(academicHistoryDtoAdapter);

                var academicTermDtoAdapter = new AcademicTermEntityAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicTerm, Ellucian.Colleague.Dtos.Student.AcademicTerm>()).Returns(academicTermDtoAdapter);

                var academicCreditDtoAdapter = new AcademicCreditEntityAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicCredit, Ellucian.Colleague.Dtos.Student.AcademicCredit>()).Returns(academicCreditDtoAdapter);

                var midTermGradeDtoAdapter = new MidTermGradeEntityAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.MidTermGrade, Ellucian.Colleague.Dtos.Student.MidTermGrade>()).Returns(midTermGradeDtoAdapter);

                var gradeDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Grade, Ellucian.Colleague.Dtos.Student.Grade>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Grade, Ellucian.Colleague.Dtos.Student.Grade>()).Returns(gradeDtoAdapter);

                var gradeRestrictionDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.GradeRestriction, Ellucian.Colleague.Dtos.Student.GradeRestriction>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.GradeRestriction, Ellucian.Colleague.Dtos.Student.GradeRestriction>()).Returns(gradeRestrictionDtoAdapter);

                var gradingTypeAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.GradingType, Ellucian.Colleague.Dtos.Student.GradingType>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.GradingType, Ellucian.Colleague.Dtos.Student.GradingType>()).Returns(gradingTypeAdapter);

                var replacedStatusAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.ReplacedStatus, Ellucian.Colleague.Dtos.Student.ReplacedStatus>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.ReplacedStatus, Ellucian.Colleague.Dtos.Student.ReplacedStatus>()).Returns(replacedStatusAdapter);

                var replacementStatusAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.ReplacementStatus, Ellucian.Colleague.Dtos.Student.ReplacementStatus>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.ReplacementStatus, Ellucian.Colleague.Dtos.Student.ReplacementStatus>()).Returns(replacementStatusAdapter);

                // Set up current user
                currentUserFactory = new CurrentUserSetup.StudentUserFactory();
                termRepo = null;
                sectionRepo = null;
                academicHistoryService = new AcademicHistoryService(adapterRegistry, studentRepo, academicCreditRepo, termRepo, sectionRepo, currentUserFactory, roleRepo, logger, baseConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentRepo = null;
                adapterRegistry = null;
                currentUserFactory = null;
                roleRepo = null;
                logger = null;
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task ThrowsErrorIfNotSelf()
            {
                studentRepoMock.Setup(repo => repo.GetAsync("00004002")).ReturnsAsync(student2);
                await academicHistoryService.GetAcademicHistoryAsync("00004002");
            }

            [TestMethod]
            public async Task GradeRestrictedStudent_NoGrades()
            {
                // Student is himself.  But he has a grade restriction. should be no grades returned.
                var result = await academicHistoryService.GetAcademicHistoryAsync("0000894");
                Assert.IsTrue(result is Dtos.Student.AcademicHistory);
                Assert.IsTrue(result.GradeRestriction.IsRestricted);
                var term = result.AcademicTerms.First();
                var cred = term.AcademicCredits.First();
                Assert.IsNull(cred.VerifiedGradeId);

            }
        }

        [TestClass]
        public class GetHistory_AsNonStudent : CurrentUserSetup
        {
            private AcademicHistoryService academicHistoryService;
            private Mock<IAcademicCreditRepository> academicCreditRepoMock;
            private IAcademicCreditRepository academicCreditRepo;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private ITermRepository termRepo;
            private ISectionRepository sectionRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Domain.Student.Entities.Student student1;
            private Domain.Student.Entities.Student student2;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            [TestInitialize]
            public void Initialize()
            {
                academicCreditRepoMock = new Mock<IAcademicCreditRepository>();
                academicCreditRepo = academicCreditRepoMock.Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                // Mock good student repo response
                List<string> academicCreditIds = new List<string>() { "1", "2", "3", "4" };
                student1 = new Domain.Student.Entities.Student("0000894", "Smith", 2, new List<string>() { "BA.ENGL" }, academicCreditIds) { FirstName = "Bob", MiddleName = "Blakely" };
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);

                IEnumerable<Domain.Student.Entities.AcademicCredit> academicCredits = new TestAcademicCreditRepository().GetAsync(academicCreditIds).Result;
                academicCreditRepoMock.Setup(repo => repo.GetAsync(academicCreditIds, false, true, It.IsAny<bool>())).Returns(Task.FromResult(academicCredits));

                // Mock another student ....
                student2 = new Domain.Student.Entities.Student("00004002", "Jones", 802, new List<string>() { "BA.MATH" }, academicCreditIds);
                studentRepoMock.Setup(repo => repo.GetAsync("00004002")).ReturnsAsync(student2);

                // Mock get grade restriction
                GradeRestriction studentGradeRestriction = new GradeRestriction(true);
                studentGradeRestriction.AddReason("Bad");
                studentRepoMock.Setup(srepo => srepo.GetGradeRestrictionsAsync(It.IsAny<string>())).Returns(Task.FromResult(studentGradeRestriction));

                // Mock Adapters
                var academicHistoryDtoAdapter = new AcademicHistoryEntityAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicHistory, Ellucian.Colleague.Dtos.Student.AcademicHistory>()).Returns(academicHistoryDtoAdapter);

                var academicTermDtoAdapter = new AcademicTermEntityAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicTerm, Ellucian.Colleague.Dtos.Student.AcademicTerm>()).Returns(academicTermDtoAdapter);

                var academicCreditDtoAdapter = new AcademicCreditEntityAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicCredit, Ellucian.Colleague.Dtos.Student.AcademicCredit>()).Returns(academicCreditDtoAdapter);

                var midTermGradeDtoAdapter = new MidTermGradeEntityAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.MidTermGrade, Ellucian.Colleague.Dtos.Student.MidTermGrade>()).Returns(midTermGradeDtoAdapter);

                var gradeDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Grade, Ellucian.Colleague.Dtos.Student.Grade>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Grade, Ellucian.Colleague.Dtos.Student.Grade>()).Returns(gradeDtoAdapter);

                var gradeRestrictionDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.GradeRestriction, Ellucian.Colleague.Dtos.Student.GradeRestriction>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.GradeRestriction, Ellucian.Colleague.Dtos.Student.GradeRestriction>()).Returns(gradeRestrictionDtoAdapter);

                var gradingTypeAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.GradingType, Ellucian.Colleague.Dtos.Student.GradingType>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.GradingType, Ellucian.Colleague.Dtos.Student.GradingType>()).Returns(gradingTypeAdapter);

                var replacedStatusAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.ReplacedStatus, Ellucian.Colleague.Dtos.Student.ReplacedStatus>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.ReplacedStatus, Ellucian.Colleague.Dtos.Student.ReplacedStatus>()).Returns(replacedStatusAdapter);

                var replacementStatusAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.ReplacementStatus, Ellucian.Colleague.Dtos.Student.ReplacementStatus>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.ReplacementStatus, Ellucian.Colleague.Dtos.Student.ReplacementStatus>()).Returns(replacementStatusAdapter);

                // Set up current advisor user
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();
                termRepo = null;
                sectionRepo = null;
                academicHistoryService = new AcademicHistoryService(adapterRegistry, studentRepo, academicCreditRepo, termRepo, sectionRepo, currentUserFactory, roleRepo, logger, baseConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentRepo = null;
                adapterRegistry = null;
                currentUserFactory = null;
                roleRepo = null;
                logger = null;
            }


            [TestMethod]
            public async Task AllowsAccessIfAdvisorHasViewAnyAdviseesPermission()
            {
                // Advisor is permitted to see grades even though student has restriction. 
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                var result = await academicHistoryService.GetAcademicHistoryAsync("0000894");
                Assert.IsTrue(result is Dtos.Student.AcademicHistory);
                Assert.IsTrue(result.GradeRestriction.IsRestricted);
                var term = result.AcademicTerms.First();
                var cred = term.AcademicCredits.First();
                Assert.IsNotNull(cred.VerifiedGradeId);

            }

            [TestMethod]
            public async Task AllowsAccessIfAdvisorHasReviewAnyAdviseesPermission()
            {
                // Advisor is permitted to see grades even though student has restriction. 
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.ReviewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                var result = await academicHistoryService.GetAcademicHistoryAsync("0000894");
                Assert.IsTrue(result is Dtos.Student.AcademicHistory);
                Assert.IsTrue(result.GradeRestriction.IsRestricted);
                var term = result.AcademicTerms.First();
                var cred = term.AcademicCredits.First();
                Assert.IsNotNull(cred.VerifiedGradeId);

            }

            [TestMethod]
            public async Task AllowsAccessIfAdvisorHasViewAssignedAdviseesPermission()
            {
                // Advisor is permitted to see grades even though student has restriction. 
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Add advisor to student's advisor list
                student1.AddAdvisor("0000896");
                student1.AddAdvisement("0000896", null, null, null);
                student1.AddAdvisor("0000111");
                student1.AddAdvisement("0000111", null, null, null);
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);
                var result = await academicHistoryService.GetAcademicHistoryAsync("0000894");
                Assert.IsTrue(result is Dtos.Student.AcademicHistory);
                Assert.IsTrue(result.GradeRestriction.IsRestricted);
                var term = result.AcademicTerms.First();
                var cred = term.AcademicCredits.First();
                Assert.IsNotNull(cred.VerifiedGradeId);

            }

            [TestMethod]
            public async Task AllowsAccessIfAdvisorHasReviewAssignedAdviseesPermission()
            {
                // Advisor is permitted to see grades even though student has restriction. 
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.ReviewAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Add advisor to student's advisor list
                student1.AddAdvisor("0000896");
                student1.AddAdvisement("0000896", null, null, null);
                student1.AddAdvisor("0000111");
                student1.AddAdvisement("0000111", null, null, null);
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);
                var result = await academicHistoryService.GetAcademicHistoryAsync("0000894");
                Assert.IsTrue(result is Dtos.Student.AcademicHistory);
                Assert.IsTrue(result.GradeRestriction.IsRestricted);
                var term = result.AcademicTerms.First();
                var cred = term.AcademicCredits.First();
                Assert.IsNotNull(cred.VerifiedGradeId);

            }

            [TestMethod]
            public async Task AllowsAccessIfAdvisorHasUpdateAnydviseePermission()
            {
                // Advisor is permitted to see grades even though student has restriction. 
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.UpdateAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Add advisor to student's advisor list
                student1.AddAdvisor("0000896");
                student1.AddAdvisement("0000896", null, null, null);
                student1.AddAdvisor("0000111");
                student1.AddAdvisement("0000111", null, null, null);
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);
                var result = await academicHistoryService.GetAcademicHistoryAsync("0000894");
                Assert.IsTrue(result is Dtos.Student.AcademicHistory);
                Assert.IsTrue(result.GradeRestriction.IsRestricted);
                var term = result.AcademicTerms.First();
                var cred = term.AcademicCredits.First();
                Assert.IsNotNull(cred.VerifiedGradeId);

            }

            [TestMethod]
            public async Task AllowsAccessIfAdvisorHasAllAccessAnydviseePermission()
            {
                // Advisor is permitted to see grades even though student has restriction. 
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Add advisor to student's advisor list
                student1.AddAdvisor("0000896");
                student1.AddAdvisement("0000896", null, null, null);
                student1.AddAdvisor("0000111");
                student1.AddAdvisement("0000111", null, null, null);
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);
                var result = await academicHistoryService.GetAcademicHistoryAsync("0000894");
                Assert.IsTrue(result is Dtos.Student.AcademicHistory);
                Assert.IsTrue(result.GradeRestriction.IsRestricted);
                var term = result.AcademicTerms.First();
                var cred = term.AcademicCredits.First();
                Assert.IsNotNull(cred.VerifiedGradeId);

            }

            [TestMethod]
            public async Task AllowsAccessIfPersonHasViewStudentInformationPermission()
            {
                // Advisor is permitted to see grades even though student has restriction. 
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewStudentInformation));
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() { advisorRole });
                // Add advisor to student's advisor list
                student1.AddAdvisor("0000896");
                student1.AddAdvisement("0000896", null, null, null);
                student1.AddAdvisor("0000111");
                student1.AddAdvisement("0000111", null, null, null);
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);
                var result = await academicHistoryService.GetAcademicHistoryAsync("0000894");
                Assert.IsTrue(result is Dtos.Student.AcademicHistory);
                Assert.IsTrue(result.GradeRestriction.IsRestricted);
                var term = result.AcademicTerms.First();
                var cred = term.AcademicCredits.First();
                Assert.IsNotNull(cred.VerifiedGradeId);

            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task ThrowsErrorIfAdvisorNotInStudentsAdvisorList()
            {
                // Set up needed permission. Advisor is personid 0000111
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Add advisor to student's advisor list
                student1.AddAdvisor("0000896");
                student1.AddAdvisement("0000896", null, null, null);
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);
                var result = await academicHistoryService.GetAcademicHistoryAsync("0000894");
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task ThrowsErrorIfPersonNotSelfAndNoPermissions()
            {
                // Set up needed permission. Advisor is personid 0000111
                //roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Add advisor to student's advisor list
                student1.AddAdvisor("0000896");
                student1.AddAdvisement("0000896", null, null, null);
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);
                var result = await academicHistoryService.GetAcademicHistoryAsync("0000894");
            }
        }

        [TestClass]
        public class GetHistory2_AsStudentUser : CurrentUserSetup
        {
            private AcademicHistoryService academicHistoryService;
            private Mock<IAcademicCreditRepository> academicCreditRepoMock;
            private IAcademicCreditRepository academicCreditRepo;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Domain.Student.Entities.Student student1;
            private Domain.Student.Entities.Student student2;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private ITermRepository termRepo;
            private ISectionRepository sectionRepo;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            [TestInitialize]
            public void Initialize()
            {
                academicCreditRepoMock = new Mock<IAcademicCreditRepository>();
                academicCreditRepo = academicCreditRepoMock.Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                // Mock good student repo response
                List<string> academicCreditIds = new List<string>() { "1", "2", "3", "4" };
                student1 = new Domain.Student.Entities.Student("0000894", "Smith", 2, new List<string>() { "BA.ENGL" }, academicCreditIds) { FirstName = "Bob", MiddleName = "Blakely" };
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);

                IEnumerable<Domain.Student.Entities.AcademicCredit> academicCredits = new TestAcademicCreditRepository().GetAsync(academicCreditIds).Result;
                academicCreditRepoMock.Setup(repo => repo.GetAsync(academicCreditIds, false, true, It.IsAny<bool>())).Returns(Task.FromResult(academicCredits));

                // Mock another student ....
                student2 = new Domain.Student.Entities.Student("00004002", "Jones", 802, new List<string>() { "BA.MATH" }, academicCreditIds);
                studentRepoMock.Setup(repo => repo.GetAsync("00004002")).ReturnsAsync(student2);

                // Mock get grade restriction
                GradeRestriction studentGradeRestriction = new GradeRestriction(true);
                studentGradeRestriction.AddReason("Bad");
                studentRepoMock.Setup(srepo => srepo.GetGradeRestrictionsAsync(It.IsAny<string>())).Returns(Task.FromResult(studentGradeRestriction));

                // Mock Adapters
                var academicHistoryDtoAdapter = new AcademicHistoryEntityToAcademicHistory2DtoAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicHistory, Ellucian.Colleague.Dtos.Student.AcademicHistory2>()).Returns(academicHistoryDtoAdapter);

                var academicTermDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicTerm, Ellucian.Colleague.Dtos.Student.AcademicTerm2>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicTerm, Ellucian.Colleague.Dtos.Student.AcademicTerm2>()).Returns(academicTermDtoAdapter);

                var academicCreditDtoAdapter = new AcademicCreditEntityToAcademicCredit2DtoAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicCredit, Ellucian.Colleague.Dtos.Student.AcademicCredit2>()).Returns(academicCreditDtoAdapter);

                var midTermGradeDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.MidTermGrade, Ellucian.Colleague.Dtos.Student.MidTermGrade2>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.MidTermGrade, Ellucian.Colleague.Dtos.Student.MidTermGrade2>()).Returns(midTermGradeDtoAdapter);

                var gradeDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Grade, Ellucian.Colleague.Dtos.Student.Grade>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Grade, Ellucian.Colleague.Dtos.Student.Grade>()).Returns(gradeDtoAdapter);

                // Set up current user
                currentUserFactory = new CurrentUserSetup.StudentUserFactory();
                termRepo = null;
                sectionRepo = null;
                academicHistoryService = new AcademicHistoryService(adapterRegistry, studentRepo, academicCreditRepo, termRepo, sectionRepo, currentUserFactory, roleRepo, logger, baseConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentRepo = null;
                adapterRegistry = null;
                currentUserFactory = null;
                roleRepo = null;
                logger = null;
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task ThrowsErrorIfNotSelf()
            {
                studentRepoMock.Setup(repo => repo.GetAsync("00004002")).ReturnsAsync(student2);
                await academicHistoryService.GetAcademicHistory2Async("00004002");
            }

            [TestMethod]
            public async Task GradeRestrictedStudent_NoGrades()
            {
                // Student is himself.  But he has a grade restriction. should be no grades returned.
                var result = await academicHistoryService.GetAcademicHistory2Async("0000894");
                Assert.IsTrue(result is Dtos.Student.AcademicHistory2);
                Assert.IsTrue(result.GradeRestriction.IsRestricted);
                var term = result.AcademicTerms.First();
                var cred = term.AcademicCredits.First();
                Assert.IsNull(cred.VerifiedGradeId);

            }
        }

        [TestClass]
        public class GetHistory2_AsNonStudent : CurrentUserSetup
        {
            private AcademicHistoryService academicHistoryService;
            private Mock<IAcademicCreditRepository> academicCreditRepoMock;
            private IAcademicCreditRepository academicCreditRepo;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Domain.Student.Entities.Student student1;
            private Domain.Student.Entities.Student student2;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private ITermRepository termRepo;
            private ISectionRepository sectionRepo;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            [TestInitialize]
            public void Initialize()
            {
                academicCreditRepoMock = new Mock<IAcademicCreditRepository>();
                academicCreditRepo = academicCreditRepoMock.Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                // Mock good student repo response
                List<string> academicCreditIds = new List<string>() { "1", "2", "3", "4" };
                student1 = new Domain.Student.Entities.Student("0000894", "Smith", 2, new List<string>() { "BA.ENGL" }, academicCreditIds) { FirstName = "Bob", MiddleName = "Blakely" };
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);

                IEnumerable<Domain.Student.Entities.AcademicCredit> academicCredits = new TestAcademicCreditRepository().GetAsync(academicCreditIds).Result;
                academicCreditRepoMock.Setup(repo => repo.GetAsync(academicCreditIds, false, true, It.IsAny<bool>())).Returns(Task.FromResult(academicCredits));

                // Mock another student ....
                student2 = new Domain.Student.Entities.Student("00004002", "Jones", 802, new List<string>() { "BA.MATH" }, academicCreditIds);
                studentRepoMock.Setup(repo => repo.GetAsync("00004002")).ReturnsAsync(student2);

                // Mock get grade restriction
                GradeRestriction studentGradeRestriction = new GradeRestriction(true);
                studentGradeRestriction.AddReason("Bad");
                studentRepoMock.Setup(srepo => srepo.GetGradeRestrictionsAsync(It.IsAny<string>())).Returns(Task.FromResult(studentGradeRestriction));

                // Mock Adapters
                var academicHistoryDtoAdapter = new AcademicHistoryEntityToAcademicHistory2DtoAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicHistory, Ellucian.Colleague.Dtos.Student.AcademicHistory2>()).Returns(academicHistoryDtoAdapter);

                var academicTermDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicTerm, Ellucian.Colleague.Dtos.Student.AcademicTerm2>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicTerm, Ellucian.Colleague.Dtos.Student.AcademicTerm2>()).Returns(academicTermDtoAdapter);

                var academicCreditDtoAdapter = new AcademicCreditEntityToAcademicCredit2DtoAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicCredit, Ellucian.Colleague.Dtos.Student.AcademicCredit2>()).Returns(academicCreditDtoAdapter);

                var midTermGradeDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.MidTermGrade, Ellucian.Colleague.Dtos.Student.MidTermGrade2>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.MidTermGrade, Ellucian.Colleague.Dtos.Student.MidTermGrade2>()).Returns(midTermGradeDtoAdapter);

                var gradeDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Grade, Ellucian.Colleague.Dtos.Student.Grade>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Grade, Ellucian.Colleague.Dtos.Student.Grade>()).Returns(gradeDtoAdapter);

                // Set up current advisor user
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();
                termRepo = null;
                sectionRepo = null;
                academicHistoryService = new AcademicHistoryService(adapterRegistry, studentRepo, academicCreditRepo, termRepo, sectionRepo, currentUserFactory, roleRepo, logger, baseConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentRepo = null;
                adapterRegistry = null;
                currentUserFactory = null;
                roleRepo = null;
                logger = null;
            }


            [TestMethod]
            public async Task AllowsAccessIfAdvisorHasViewAnyAdviseesPermission()
            {
                // Advisor is permitted to see grades even though student has restriction. 
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                var result = await academicHistoryService.GetAcademicHistory2Async("0000894");
                Assert.IsTrue(result is Dtos.Student.AcademicHistory2);
                Assert.IsTrue(result.GradeRestriction.IsRestricted);
                var term = result.AcademicTerms.First();
                var cred = term.AcademicCredits.First();
                Assert.IsNotNull(cred.VerifiedGradeId);

            }

            [TestMethod]
            public async Task AllowsAccessIfAdvisorHasReviewAnyAdviseesPermission()
            {
                // Advisor is permitted to see grades even though student has restriction. 
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.ReviewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                var result = await academicHistoryService.GetAcademicHistory2Async("0000894");
                Assert.IsTrue(result is Dtos.Student.AcademicHistory2);
                Assert.IsTrue(result.GradeRestriction.IsRestricted);
                var term = result.AcademicTerms.First();
                var cred = term.AcademicCredits.First();
                Assert.IsNotNull(cred.VerifiedGradeId);

            }

            [TestMethod]
            public async Task AllowsAccessIfAdvisorHasViewAssignedAdviseesPermission()
            {
                // Advisor is permitted to see grades even though student has restriction. 
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Add advisor to student's advisor list
                student1.AddAdvisor("0000896");
                student1.AddAdvisement("0000896", null, null, null);
                student1.AddAdvisor("0000111");
                student1.AddAdvisement("0000111", null, null, null);
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);
                var result = await academicHistoryService.GetAcademicHistory2Async("0000894");
                Assert.IsTrue(result is Dtos.Student.AcademicHistory2);
                Assert.IsTrue(result.GradeRestriction.IsRestricted);
                var term = result.AcademicTerms.First();
                var cred = term.AcademicCredits.First();
                Assert.IsNotNull(cred.VerifiedGradeId);

            }

            [TestMethod]
            public async Task AllowsAccessIfAdvisorHasReviewAssignedAdviseesPermission()
            {
                // Advisor is permitted to see grades even though student has restriction. 
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.ReviewAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Add advisor to student's advisor list
                student1.AddAdvisor("0000896");
                student1.AddAdvisement("0000896", null, null, null);
                student1.AddAdvisor("0000111");
                student1.AddAdvisement("0000111", null, null, null);
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);
                var result = await academicHistoryService.GetAcademicHistory2Async("0000894");
                Assert.IsTrue(result is Dtos.Student.AcademicHistory2);
                Assert.IsTrue(result.GradeRestriction.IsRestricted);
                var term = result.AcademicTerms.First();
                var cred = term.AcademicCredits.First();
                Assert.IsNotNull(cred.VerifiedGradeId);

            }

            [TestMethod]
            public async Task AllowsAccessIfAdvisorHasUpdateAnydviseePermission()
            {
                // Advisor is permitted to see grades even though student has restriction. 
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.UpdateAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Add advisor to student's advisor list
                student1.AddAdvisor("0000896");
                student1.AddAdvisement("0000896", null, null, null);
                student1.AddAdvisor("0000111");
                student1.AddAdvisement("0000111", null, null, null);
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);
                var result = await academicHistoryService.GetAcademicHistory2Async("0000894");
                Assert.IsTrue(result is Dtos.Student.AcademicHistory2);
                Assert.IsTrue(result.GradeRestriction.IsRestricted);
                var term = result.AcademicTerms.First();
                var cred = term.AcademicCredits.First();
                Assert.IsNotNull(cred.VerifiedGradeId);

            }

            [TestMethod]
            public async Task AllowsAccessIfAdvisorHasAllAccessAnydviseePermission()
            {
                // Advisor is permitted to see grades even though student has restriction. 
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Add advisor to student's advisor list
                student1.AddAdvisor("0000896");
                student1.AddAdvisement("0000896", null, null, null);
                student1.AddAdvisor("0000111");
                student1.AddAdvisement("0000111", null, null, null);
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);
                var result = await academicHistoryService.GetAcademicHistory2Async("0000894");
                Assert.IsTrue(result is Dtos.Student.AcademicHistory2);
                Assert.IsTrue(result.GradeRestriction.IsRestricted);
                var term = result.AcademicTerms.First();
                var cred = term.AcademicCredits.First();
                Assert.IsNotNull(cred.VerifiedGradeId);

            }

            [TestMethod]
            public async Task AllowsAccessIfPersonHasViewStudentInformationPermission()
            {
                // Advisor is permitted to see grades even though student has restriction. 
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewStudentInformation));
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() { advisorRole });
                // Add advisor to student's advisor list
                student1.AddAdvisor("0000896");
                student1.AddAdvisement("0000896", null, null, null);
                student1.AddAdvisor("0000111");
                student1.AddAdvisement("0000111", null, null, null);
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);
                var result = await academicHistoryService.GetAcademicHistory2Async("0000894");
                Assert.IsTrue(result is Dtos.Student.AcademicHistory2);
                Assert.IsTrue(result.GradeRestriction.IsRestricted);
                var term = result.AcademicTerms.First();
                var cred = term.AcademicCredits.First();
                Assert.IsNotNull(cred.VerifiedGradeId);

            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task ThrowsErrorIfAdvisorNotInStudentsAdvisorList()
            {
                // Set up needed permission. Advisor is personid 0000111
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Add advisor to student's advisor list
                student1.AddAdvisor("0000896");
                student1.AddAdvisement("0000896", null, null, null);
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);
                var result = await academicHistoryService.GetAcademicHistory2Async("0000894");
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task ThrowsErrorIfPersonNotSelfAndNoPermissions()
            {
                // Set up needed permission. Advisor is personid 0000111
                //roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Add advisor to student's advisor list
                student1.AddAdvisor("0000896");
                student1.AddAdvisement("0000896", null, null, null);
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);
                var result = await academicHistoryService.GetAcademicHistory2Async("0000894");
            }
        }
        [TestClass]
        public class GetInvalidStudentEnrollment : CurrentUserSetup
        {
            private AcademicHistoryService academicHistoryService;
            private Mock<IAcademicCreditRepository> academicCreditRepoMock;
            private IAcademicCreditRepository academicCreditRepo;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private ITermRepository termRepo;
            private ISectionRepository sectionRepo;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            [TestInitialize]
            public void Initialize()
            {
                academicCreditRepoMock = new Mock<IAcademicCreditRepository>();
                academicCreditRepo = academicCreditRepoMock.Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                // Mock good student repo response
                List<string> academicCreditIds = new List<string>() { "1", "2", "3", "4" };

                IEnumerable<Domain.Student.Entities.AcademicCredit> academicCredits = new TestAcademicCreditRepository().GetAsync(academicCreditIds).Result;
                Dictionary<string, List<Domain.Student.Entities.AcademicCredit>> dictAcademicCredits = new Dictionary<string, List<AcademicCredit>>();
                dictAcademicCredits.Add("0000894", academicCredits.ToList());
                IEnumerable<string> studentIds = new List<string>() { "0000894" };
                academicCreditRepoMock.Setup(repo => repo.GetAcademicCreditByStudentIdsAsync(studentIds, true, false, It.IsAny<bool>())).Returns(Task.FromResult(dictAcademicCredits));

                // Set up current advisor user
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();
                termRepo = null;
                sectionRepo = null;
                academicHistoryService = new AcademicHistoryService(adapterRegistry, studentRepo, academicCreditRepo, termRepo, sectionRepo, currentUserFactory, roleRepo, logger, baseConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentRepo = null;
                adapterRegistry = null;
                currentUserFactory = null;
                roleRepo = null;
                logger = null;
            }

            [TestMethod]
            public async Task ReturnInvalidStudentEnrollments()
            {
                // Advisor is permitted to see grades even though student has restriction. 
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewStudentInformation));
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() { advisorRole });
                List<Dtos.Student.StudentEnrollment> criteria = new List<Dtos.Student.StudentEnrollment>();
                criteria.Add(new Dtos.Student.StudentEnrollment() { SectionId = "12345", StudentId = "0000894", TermId = "2009/SP" });
                criteria.Add(new Dtos.Student.StudentEnrollment() { SectionId = "8001", StudentId = "0000894", TermId = "2009/SP" });
                criteria.Add(new Dtos.Student.StudentEnrollment() { SectionId = "8002", StudentId = "0000894", TermId = "2009/SP" });
                criteria.Add(new Dtos.Student.StudentEnrollment() { SectionId = "8003", StudentId = "0000894", TermId = "2009/SP" });
                criteria.Add(new Dtos.Student.StudentEnrollment() { SectionId = "8004", StudentId = "0000894", TermId = "2009/SP" });

                var result = await academicHistoryService.GetInvalidStudentEnrollmentAsync(criteria);

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Count());
            }
        }

        [TestClass]
        public class GetHistory3_AsStudentUser : CurrentUserSetup
        {
            private AcademicHistoryService academicHistoryService;
            private Mock<IAcademicCreditRepository> academicCreditRepoMock;
            private IAcademicCreditRepository academicCreditRepo;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Domain.Student.Entities.Student student1;
            private Domain.Student.Entities.Student student2;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private ITermRepository termRepo;
            private ISectionRepository sectionRepo;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            [TestInitialize]
            public void Initialize()
            {
                academicCreditRepoMock = new Mock<IAcademicCreditRepository>();
                academicCreditRepo = academicCreditRepoMock.Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                // Mock good student repo response
                List<string> academicCreditIds = new List<string>() { "1", "2", "3", "4" };
                student1 = new Domain.Student.Entities.Student("0000894", "Smith", 2, new List<string>() { "BA.ENGL" }, academicCreditIds) { FirstName = "Bob", MiddleName = "Blakely" };
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);

                IEnumerable<Domain.Student.Entities.AcademicCredit> academicCredits = new TestAcademicCreditRepository().GetAsync(academicCreditIds).Result;
                academicCreditRepoMock.Setup(repo => repo.GetAsync(academicCreditIds, false, true, It.IsAny<bool>())).Returns(Task.FromResult(academicCredits));

                // Mock another student ....
                student2 = new Domain.Student.Entities.Student("00004002", "Jones", 802, new List<string>() { "BA.MATH" }, academicCreditIds);
                studentRepoMock.Setup(repo => repo.GetAsync("00004002")).ReturnsAsync(student2);

                // Mock get grade restriction
                GradeRestriction studentGradeRestriction = new GradeRestriction(true);
                studentGradeRestriction.AddReason("Bad");
                studentRepoMock.Setup(srepo => srepo.GetGradeRestrictionsAsync(It.IsAny<string>())).Returns(Task.FromResult(studentGradeRestriction));

                // Mock Adapters
                var academicHistoryDtoAdapter = new AcademicHistoryEntityToAcademicHistory3DtoAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicHistory, Ellucian.Colleague.Dtos.Student.AcademicHistory3>()).Returns(academicHistoryDtoAdapter);

                var academicTermDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicTerm, Ellucian.Colleague.Dtos.Student.AcademicTerm3>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicTerm, Ellucian.Colleague.Dtos.Student.AcademicTerm3>()).Returns(academicTermDtoAdapter);

                var academicCreditDtoAdapter = new AcademicCreditEntityToAcademicCredit2DtoAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicCredit, Ellucian.Colleague.Dtos.Student.AcademicCredit2>()).Returns(academicCreditDtoAdapter);

                var midTermGradeDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.MidTermGrade, Ellucian.Colleague.Dtos.Student.MidTermGrade2>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.MidTermGrade, Ellucian.Colleague.Dtos.Student.MidTermGrade2>()).Returns(midTermGradeDtoAdapter);

                var gradeDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Grade, Ellucian.Colleague.Dtos.Student.Grade>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Grade, Ellucian.Colleague.Dtos.Student.Grade>()).Returns(gradeDtoAdapter);

                // Set up current user
                currentUserFactory = new CurrentUserSetup.StudentUserFactory();
                termRepo = null;
                sectionRepo = null;
                academicHistoryService = new AcademicHistoryService(adapterRegistry, studentRepo, academicCreditRepo, termRepo, sectionRepo, currentUserFactory, roleRepo, logger, baseConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentRepo = null;
                adapterRegistry = null;
                currentUserFactory = null;
                roleRepo = null;
                logger = null;
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task ThrowsErrorIfNotSelf()
            {
                studentRepoMock.Setup(repo => repo.GetAsync("00004002")).ReturnsAsync(student2);
                await academicHistoryService.GetAcademicHistory3Async("00004002");
            }

            [TestMethod]
            public async Task GradeRestrictedStudent_NoGrades()
            {
                // Student is himself.  But he has a grade restriction. should be no grades returned.
                var result = await academicHistoryService.GetAcademicHistory3Async("0000894");
                Assert.IsTrue(result is Dtos.Student.AcademicHistory3);
                Assert.IsTrue(result.GradeRestriction.IsRestricted);
                var term = result.AcademicTerms.First();
                var cred = term.AcademicCredits.First();
                Assert.IsNull(cred.VerifiedGradeId);

            }
        }

        [TestClass]
        public class GetHistory3_AsNonStudent : CurrentUserSetup
        {
            private AcademicHistoryService academicHistoryService;
            private Mock<IAcademicCreditRepository> academicCreditRepoMock;
            private IAcademicCreditRepository academicCreditRepo;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Domain.Student.Entities.Student student1;
            private Domain.Student.Entities.Student student2;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private ITermRepository termRepo;
            private ISectionRepository sectionRepo;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            [TestInitialize]
            public void Initialize()
            {
                academicCreditRepoMock = new Mock<IAcademicCreditRepository>();
                academicCreditRepo = academicCreditRepoMock.Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                // Mock good student repo response
                List<string> academicCreditIds = new List<string>() { "1", "2", "3", "4" };
                student1 = new Domain.Student.Entities.Student("0000894", "Smith", 2, new List<string>() { "BA.ENGL" }, academicCreditIds) { FirstName = "Bob", MiddleName = "Blakely" };
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);

                IEnumerable<Domain.Student.Entities.AcademicCredit> academicCredits = new TestAcademicCreditRepository().GetAsync(academicCreditIds).Result;
                academicCreditRepoMock.Setup(repo => repo.GetAsync(academicCreditIds, false, true, It.IsAny<bool>())).Returns(Task.FromResult(academicCredits));

                // Mock another student ....
                student2 = new Domain.Student.Entities.Student("00004002", "Jones", 802, new List<string>() { "BA.MATH" }, academicCreditIds);
                studentRepoMock.Setup(repo => repo.GetAsync("00004002")).ReturnsAsync(student2);

                // Mock get grade restriction
                GradeRestriction studentGradeRestriction = new GradeRestriction(true);
                studentGradeRestriction.AddReason("Bad");
                studentRepoMock.Setup(srepo => srepo.GetGradeRestrictionsAsync(It.IsAny<string>())).Returns(Task.FromResult(studentGradeRestriction));

                // Mock Adapters
                var academicHistoryDtoAdapter = new AcademicHistoryEntityToAcademicHistory3DtoAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicHistory, Ellucian.Colleague.Dtos.Student.AcademicHistory3>()).Returns(academicHistoryDtoAdapter);

                var academicTermDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicTerm, Ellucian.Colleague.Dtos.Student.AcademicTerm3>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicTerm, Ellucian.Colleague.Dtos.Student.AcademicTerm3>()).Returns(academicTermDtoAdapter);

                var academicCreditDtoAdapter = new AcademicCreditEntityToAcademicCredit2DtoAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicCredit, Ellucian.Colleague.Dtos.Student.AcademicCredit2>()).Returns(academicCreditDtoAdapter);

                var midTermGradeDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.MidTermGrade, Ellucian.Colleague.Dtos.Student.MidTermGrade2>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.MidTermGrade, Ellucian.Colleague.Dtos.Student.MidTermGrade2>()).Returns(midTermGradeDtoAdapter);

                var gradeDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Grade, Ellucian.Colleague.Dtos.Student.Grade>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Grade, Ellucian.Colleague.Dtos.Student.Grade>()).Returns(gradeDtoAdapter);

                // Set up current advisor user
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();
                termRepo = null;
                sectionRepo = null;
                academicHistoryService = new AcademicHistoryService(adapterRegistry, studentRepo, academicCreditRepo, termRepo, sectionRepo, currentUserFactory, roleRepo, logger, baseConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentRepo = null;
                adapterRegistry = null;
                currentUserFactory = null;
                roleRepo = null;
                logger = null;
            }


            [TestMethod]
            public async Task AllowsAccessIfAdvisorHasViewAnyAdviseesPermission()
            {
                // Advisor is permitted to see grades even though student has restriction. 
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                var result = await academicHistoryService.GetAcademicHistory3Async("0000894");
                Assert.IsTrue(result is Dtos.Student.AcademicHistory3);
                Assert.IsTrue(result.GradeRestriction.IsRestricted);
                var term = result.AcademicTerms.First();
                var cred = term.AcademicCredits.First();
                Assert.IsNotNull(cred.VerifiedGradeId);

            }

            [TestMethod]
            public async Task AllowsAccessIfAdvisorHasReviewAnyAdviseesPermission()
            {
                // Advisor is permitted to see grades even though student has restriction. 
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.ReviewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                var result = await academicHistoryService.GetAcademicHistory3Async("0000894");
                Assert.IsTrue(result is Dtos.Student.AcademicHistory3);
                Assert.IsTrue(result.GradeRestriction.IsRestricted);
                var term = result.AcademicTerms.First();
                var cred = term.AcademicCredits.First();
                Assert.IsNotNull(cred.VerifiedGradeId);

            }

            [TestMethod]
            public async Task AllowsAccessIfAdvisorHasViewAssignedAdviseesPermission()
            {
                // Advisor is permitted to see grades even though student has restriction. 
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Add advisor to student's advisor list
                student1.AddAdvisor("0000896");
                student1.AddAdvisement("0000896", null, null, null);
                student1.AddAdvisor("0000111");
                student1.AddAdvisement("0000111", null, null, null);
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);
                var result = await academicHistoryService.GetAcademicHistory3Async("0000894");
                Assert.IsTrue(result is Dtos.Student.AcademicHistory3);
                Assert.IsTrue(result.GradeRestriction.IsRestricted);
                var term = result.AcademicTerms.First();
                var cred = term.AcademicCredits.First();
                Assert.IsNotNull(cred.VerifiedGradeId);

            }

            [TestMethod]
            public async Task AllowsAccessIfAdvisorHasReviewAssignedAdviseesPermission()
            {
                // Advisor is permitted to see grades even though student has restriction. 
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.ReviewAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Add advisor to student's advisor list
                student1.AddAdvisor("0000896");
                student1.AddAdvisement("0000896", null, null, null);
                student1.AddAdvisor("0000111");
                student1.AddAdvisement("0000111", null, null, null);
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);
                var result = await academicHistoryService.GetAcademicHistory3Async("0000894");
                Assert.IsTrue(result is Dtos.Student.AcademicHistory3);
                Assert.IsTrue(result.GradeRestriction.IsRestricted);
                var term = result.AcademicTerms.First();
                var cred = term.AcademicCredits.First();
                Assert.IsNotNull(cred.VerifiedGradeId);

            }

            [TestMethod]
            public async Task AllowsAccessIfAdvisorHasUpdateAnydviseePermission()
            {
                // Advisor is permitted to see grades even though student has restriction. 
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.UpdateAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Add advisor to student's advisor list
                student1.AddAdvisor("0000896");
                student1.AddAdvisement("0000896", null, null, null);
                student1.AddAdvisor("0000111");
                student1.AddAdvisement("0000111", null, null, null);
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);
                var result = await academicHistoryService.GetAcademicHistory3Async("0000894");
                Assert.IsTrue(result is Dtos.Student.AcademicHistory3);
                Assert.IsTrue(result.GradeRestriction.IsRestricted);
                var term = result.AcademicTerms.First();
                var cred = term.AcademicCredits.First();
                Assert.IsNotNull(cred.VerifiedGradeId);

            }

            [TestMethod]
            public async Task AllowsAccessIfAdvisorHasAllAccessAnydviseePermission()
            {
                // Advisor is permitted to see grades even though student has restriction. 
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Add advisor to student's advisor list
                student1.AddAdvisor("0000896");
                student1.AddAdvisement("0000896", null, null, null);
                student1.AddAdvisor("0000111");
                student1.AddAdvisement("0000111", null, null, null);
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);
                var result = await academicHistoryService.GetAcademicHistory3Async("0000894");
                Assert.IsTrue(result is Dtos.Student.AcademicHistory3);
                Assert.IsTrue(result.GradeRestriction.IsRestricted);
                var term = result.AcademicTerms.First();
                var cred = term.AcademicCredits.First();
                Assert.IsNotNull(cred.VerifiedGradeId);

            }

            [TestMethod]
            public async Task AllowsAccessIfPersonHasViewStudentInformationPermission()
            {
                // Advisor is permitted to see grades even though student has restriction. 
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewStudentInformation));
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() { advisorRole });
                // Add advisor to student's advisor list
                student1.AddAdvisor("0000896");
                student1.AddAdvisement("0000896", null, null, null);
                student1.AddAdvisor("0000111");
                student1.AddAdvisement("0000111", null, null, null);
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);
                var result = await academicHistoryService.GetAcademicHistory3Async("0000894");
                Assert.IsTrue(result is Dtos.Student.AcademicHistory3);
                Assert.IsTrue(result.GradeRestriction.IsRestricted);
                var term = result.AcademicTerms.First();
                var cred = term.AcademicCredits.First();
                Assert.IsNotNull(cred.VerifiedGradeId);

            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task ThrowsErrorIfAdvisorNotInStudentsAdvisorList()
            {
                // Set up needed permission. Advisor is personid 0000111
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Add advisor to student's advisor list
                student1.AddAdvisor("0000896");
                student1.AddAdvisement("0000896", null, null, null);
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);
                var result = await academicHistoryService.GetAcademicHistory3Async("0000894");
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task ThrowsErrorIfPersonNotSelfAndNoPermissions()
            {
                // Set up needed permission. Advisor is personid 0000111
                //roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Add advisor to student's advisor list
                student1.AddAdvisor("0000896");
                student1.AddAdvisement("0000896", null, null, null);
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);
                var result = await academicHistoryService.GetAcademicHistory3Async("0000894");
            }
        }

        [TestClass]
        public class GetHistory4_AsStudentUser : CurrentUserSetup
        {
            private AcademicHistoryService academicHistoryService;
            private Mock<IAcademicCreditRepository> academicCreditRepoMock;
            private IAcademicCreditRepository academicCreditRepo;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Domain.Student.Entities.Student student1;
            private Domain.Student.Entities.Student student2;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private ITermRepository termRepo;
            private ISectionRepository sectionRepo;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            [TestInitialize]
            public void Initialize()
            {
                academicCreditRepoMock = new Mock<IAcademicCreditRepository>();
                academicCreditRepo = academicCreditRepoMock.Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                // Mock good student repo response
                List<string> academicCreditIds = new List<string>() { "1", "2", "3", "4" };
                student1 = new Domain.Student.Entities.Student("0000894", "Smith", 2, new List<string>() { "BA.ENGL" }, academicCreditIds) { FirstName = "Bob", MiddleName = "Blakely" };
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);

                IEnumerable<Domain.Student.Entities.AcademicCredit> academicCredits = new TestAcademicCreditRepository().GetAsync(academicCreditIds).Result;
                academicCreditRepoMock.Setup(repo => repo.GetAsync(academicCreditIds, false, true, It.IsAny<bool>())).Returns(Task.FromResult(academicCredits));

                // Mock another student ....
                student2 = new Domain.Student.Entities.Student("00004002", "Jones", 802, new List<string>() { "BA.MATH" }, academicCreditIds);
                studentRepoMock.Setup(repo => repo.GetAsync("00004002")).ReturnsAsync(student2);

                // Mock get grade restriction
                GradeRestriction studentGradeRestriction = new GradeRestriction(true);
                studentGradeRestriction.AddReason("Bad");
                studentRepoMock.Setup(srepo => srepo.GetGradeRestrictionsAsync(It.IsAny<string>())).Returns(Task.FromResult(studentGradeRestriction));

                // Mock Adapters
                var academicHistoryDtoAdapter = new AcademicHistoryEntityToAcademicHistory4DtoAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicHistory, Ellucian.Colleague.Dtos.Student.AcademicHistory4>()).Returns(academicHistoryDtoAdapter);

                var academicTermDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicTerm, Ellucian.Colleague.Dtos.Student.AcademicTerm3>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicTerm, Ellucian.Colleague.Dtos.Student.AcademicTerm3>()).Returns(academicTermDtoAdapter);

                var academicCreditDtoAdapter = new AcademicCreditEntityToAcademicCredit2DtoAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicCredit, Ellucian.Colleague.Dtos.Student.AcademicCredit2>()).Returns(academicCreditDtoAdapter);

                var midTermGradeDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.MidTermGrade, Ellucian.Colleague.Dtos.Student.MidTermGrade2>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.MidTermGrade, Ellucian.Colleague.Dtos.Student.MidTermGrade2>()).Returns(midTermGradeDtoAdapter);

                var gradeDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Grade, Ellucian.Colleague.Dtos.Student.Grade>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Grade, Ellucian.Colleague.Dtos.Student.Grade>()).Returns(gradeDtoAdapter);

                // Set up current user
                currentUserFactory = new CurrentUserSetup.StudentUserFactory();
                termRepo = null;
                sectionRepo = null;
                academicHistoryService = new AcademicHistoryService(adapterRegistry, studentRepo, academicCreditRepo, termRepo, sectionRepo, currentUserFactory, roleRepo, logger, baseConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentRepo = null;
                adapterRegistry = null;
                currentUserFactory = null;
                roleRepo = null;
                logger = null;
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task ThrowsErrorIfNotSelf()
            {
                studentRepoMock.Setup(repo => repo.GetAsync("00004002")).ReturnsAsync(student2);
                await academicHistoryService.GetAcademicHistory4Async("00004002");
            }

            [TestMethod]
            public async Task GradeRestrictedStudent_NoGrades()
            {
                // Student is himself.  But he has a grade restriction. should be no grades returned.
                var result = await academicHistoryService.GetAcademicHistory4Async("0000894");
                Assert.IsTrue(result is Dtos.Student.AcademicHistory4);
                Assert.IsTrue(result.GradeRestriction.IsRestricted);
                var term = result.AcademicTerms.First();
                var cred = term.AcademicCredits.First();
                Assert.IsNull(cred.VerifiedGradeId);

            }
        }

        [TestClass]
        public class GetHistory4_AsNonStudent : CurrentUserSetup
        {
            private AcademicHistoryService academicHistoryService;
            private Mock<IAcademicCreditRepository> academicCreditRepoMock;
            private IAcademicCreditRepository academicCreditRepo;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Domain.Student.Entities.Student student1;
            private Domain.Student.Entities.Student student2;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private ITermRepository termRepo;
            private ISectionRepository sectionRepo;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            [TestInitialize]
            public void Initialize()
            {
                academicCreditRepoMock = new Mock<IAcademicCreditRepository>();
                academicCreditRepo = academicCreditRepoMock.Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                // Mock good student repo response
                List<string> academicCreditIds = new List<string>() { "1", "2", "3", "4" };
                student1 = new Domain.Student.Entities.Student("0000894", "Smith", 2, new List<string>() { "BA.ENGL" }, academicCreditIds) { FirstName = "Bob", MiddleName = "Blakely" };
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);

                IEnumerable<Domain.Student.Entities.AcademicCredit> academicCredits = new TestAcademicCreditRepository().GetAsync(academicCreditIds).Result;
                academicCreditRepoMock.Setup(repo => repo.GetAsync(academicCreditIds, false, true, It.IsAny<bool>())).Returns(Task.FromResult(academicCredits));

                // Mock another student ....
                student2 = new Domain.Student.Entities.Student("00004002", "Jones", 802, new List<string>() { "BA.MATH" }, academicCreditIds);
                studentRepoMock.Setup(repo => repo.GetAsync("00004002")).ReturnsAsync(student2);

                // Mock get grade restriction
                GradeRestriction studentGradeRestriction = new GradeRestriction(true);
                studentGradeRestriction.AddReason("Bad");
                studentRepoMock.Setup(srepo => srepo.GetGradeRestrictionsAsync(It.IsAny<string>())).Returns(Task.FromResult(studentGradeRestriction));

                // Mock Adapters
                var academicHistoryDtoAdapter = new AcademicHistoryEntityToAcademicHistory4DtoAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicHistory, Ellucian.Colleague.Dtos.Student.AcademicHistory4>()).Returns(academicHistoryDtoAdapter);

                var academicTermDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicTerm, Ellucian.Colleague.Dtos.Student.AcademicTerm3>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicTerm, Ellucian.Colleague.Dtos.Student.AcademicTerm3>()).Returns(academicTermDtoAdapter);

                var academicCreditDtoAdapter = new AcademicCreditEntityToAcademicCredit2DtoAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicCredit, Ellucian.Colleague.Dtos.Student.AcademicCredit2>()).Returns(academicCreditDtoAdapter);

                var midTermGradeDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.MidTermGrade, Ellucian.Colleague.Dtos.Student.MidTermGrade2>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.MidTermGrade, Ellucian.Colleague.Dtos.Student.MidTermGrade2>()).Returns(midTermGradeDtoAdapter);

                var gradeDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Grade, Ellucian.Colleague.Dtos.Student.Grade>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Grade, Ellucian.Colleague.Dtos.Student.Grade>()).Returns(gradeDtoAdapter);

                // Set up current advisor user
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();
                termRepo = null;
                sectionRepo = null;
                academicHistoryService = new AcademicHistoryService(adapterRegistry, studentRepo, academicCreditRepo, termRepo, sectionRepo, currentUserFactory, roleRepo, logger, baseConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentRepo = null;
                adapterRegistry = null;
                currentUserFactory = null;
                roleRepo = null;
                logger = null;
            }


            [TestMethod]
            public async Task AllowsAccessIfAdvisorHasViewAnyAdviseesPermission()
            {
                // Advisor is permitted to see grades even though student has restriction. 
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                var result = await academicHistoryService.GetAcademicHistory4Async("0000894");
                Assert.IsTrue(result is Dtos.Student.AcademicHistory4);
                Assert.IsTrue(result.GradeRestriction.IsRestricted);
                var term = result.AcademicTerms.First();
                var cred = term.AcademicCredits.First();
                Assert.IsNotNull(cred.VerifiedGradeId);

            }

            [TestMethod]
            public async Task AllowsAccessIfAdvisorHasReviewAnyAdviseesPermission()
            {
                // Advisor is permitted to see grades even though student has restriction. 
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.ReviewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                var result = await academicHistoryService.GetAcademicHistory4Async("0000894");
                Assert.IsTrue(result is Dtos.Student.AcademicHistory4);
                Assert.IsTrue(result.GradeRestriction.IsRestricted);
                var term = result.AcademicTerms.First();
                var cred = term.AcademicCredits.First();
                Assert.IsNotNull(cred.VerifiedGradeId);

            }

            [TestMethod]
            public async Task AllowsAccessIfAdvisorHasViewAssignedAdviseesPermission()
            {
                // Advisor is permitted to see grades even though student has restriction. 
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Add advisor to student's advisor list
                student1.AddAdvisor("0000896");
                student1.AddAdvisement("0000896", null, null, null);
                student1.AddAdvisor("0000111");
                student1.AddAdvisement("0000111", null, null, null);
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);
                var result = await academicHistoryService.GetAcademicHistory4Async("0000894");
                Assert.IsTrue(result is Dtos.Student.AcademicHistory4);
                Assert.IsTrue(result.GradeRestriction.IsRestricted);
                var term = result.AcademicTerms.First();
                var cred = term.AcademicCredits.First();
                Assert.IsNotNull(cred.VerifiedGradeId);

            }

            [TestMethod]
            public async Task AllowsAccessIfAdvisorHasReviewAssignedAdviseesPermission()
            {
                // Advisor is permitted to see grades even though student has restriction. 
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.ReviewAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Add advisor to student's advisor list
                student1.AddAdvisor("0000896");
                student1.AddAdvisement("0000896", null, null, null);
                student1.AddAdvisor("0000111");
                student1.AddAdvisement("0000111", null, null, null);
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);
                var result = await academicHistoryService.GetAcademicHistory4Async("0000894");
                Assert.IsTrue(result is Dtos.Student.AcademicHistory4);
                Assert.IsTrue(result.GradeRestriction.IsRestricted);
                var term = result.AcademicTerms.First();
                var cred = term.AcademicCredits.First();
                Assert.IsNotNull(cred.VerifiedGradeId);

            }

            [TestMethod]
            public async Task AllowsAccessIfAdvisorHasUpdateAnydviseePermission()
            {
                // Advisor is permitted to see grades even though student has restriction. 
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.UpdateAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Add advisor to student's advisor list
                student1.AddAdvisor("0000896");
                student1.AddAdvisement("0000896", null, null, null);
                student1.AddAdvisor("0000111");
                student1.AddAdvisement("0000111", null, null, null);
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);
                var result = await academicHistoryService.GetAcademicHistory4Async("0000894");
                Assert.IsTrue(result is Dtos.Student.AcademicHistory4);
                Assert.IsTrue(result.GradeRestriction.IsRestricted);
                var term = result.AcademicTerms.First();
                var cred = term.AcademicCredits.First();
                Assert.IsNotNull(cred.VerifiedGradeId);

            }

            [TestMethod]
            public async Task AllowsAccessIfAdvisorHasAllAccessAnydviseePermission()
            {
                // Advisor is permitted to see grades even though student has restriction. 
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Add advisor to student's advisor list
                student1.AddAdvisor("0000896");
                student1.AddAdvisement("0000896", null, null, null);
                student1.AddAdvisor("0000111");
                student1.AddAdvisement("0000111", null, null, null);
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);
                var result = await academicHistoryService.GetAcademicHistory4Async("0000894");
                Assert.IsTrue(result is Dtos.Student.AcademicHistory4);
                Assert.IsTrue(result.GradeRestriction.IsRestricted);
                var term = result.AcademicTerms.First();
                var cred = term.AcademicCredits.First();
                Assert.IsNotNull(cred.VerifiedGradeId);

            }

            [TestMethod]
            public async Task AllowsAccessIfPersonHasViewStudentInformationPermission()
            {
                // Advisor is permitted to see grades even though student has restriction. 
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewStudentInformation));
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() { advisorRole });
                // Add advisor to student's advisor list
                student1.AddAdvisor("0000896");
                student1.AddAdvisement("0000896", null, null, null);
                student1.AddAdvisor("0000111");
                student1.AddAdvisement("0000111", null, null, null);
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);
                var result = await academicHistoryService.GetAcademicHistory4Async("0000894");
                Assert.IsTrue(result is Dtos.Student.AcademicHistory4);
                Assert.IsTrue(result.GradeRestriction.IsRestricted);
                var term = result.AcademicTerms.First();
                var cred = term.AcademicCredits.First();
                Assert.IsNotNull(cred.VerifiedGradeId);

            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task ThrowsErrorIfAdvisorNotInStudentsAdvisorList()
            {
                // Set up needed permission. Advisor is personid 0000111
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Add advisor to student's advisor list
                student1.AddAdvisor("0000896");
                student1.AddAdvisement("0000896", null, null, null);
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);
                var result = await academicHistoryService.GetAcademicHistory4Async("0000894");
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task ThrowsErrorIfPersonNotSelfAndNoPermissions()
            {
                // Set up needed permission. Advisor is personid 0000111
                //roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Add advisor to student's advisor list
                student1.AddAdvisor("0000896");
                student1.AddAdvisement("0000896", null, null, null);
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);
                var result = await academicHistoryService.GetAcademicHistory4Async("0000894");
            }
        }

        [TestClass]
        public class QueryAcademicCreditsAsync : CurrentUserSetup
        {
            private AcademicHistoryService academicHistoryService;
            private Mock<IAcademicCreditRepository> academicCreditRepoMock;
            private IAcademicCreditRepository academicCreditRepo;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Domain.Student.Entities.Section section1;
            private Domain.Student.Entities.Section section2;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private ITermRepository termRepo;
            private IStudentRepository studentRepo;
            private IEnumerable<Domain.Student.Entities.AcademicCredit> academicCreditsIncludingCrossListed;
            private IEnumerable<Domain.Student.Entities.AcademicCredit> academicCreditsExcludingCrossListed;
            private IEnumerable<string> sectionIds;
            private IEnumerable<string> sectionIdsNotCrossListed;
            private IEnumerable<string> sectionIdsWithCrossListed;
            private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> sections;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            [TestInitialize]
            public void Initialize()
            {
                academicCreditRepoMock = new Mock<IAcademicCreditRepository>();
                academicCreditRepo = academicCreditRepoMock.Object;
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                var academicCreditIds = new List<string>() { "1", "3", "18", "19", "28", "29", "44", "45" };
                sectionIds = new List<string>() { "section1", "section2" };
                sectionIdsWithCrossListed = new List<string>() { "section1", "section3", "section4" };
                academicCreditsIncludingCrossListed = new TestAcademicCreditRepository().GetAsync(academicCreditIds, false, false).Result;
                academicCreditRepoMock.Setup(repo => repo.GetAcademicCreditsBySectionIdsAsync(sectionIdsWithCrossListed)).Returns(Task.FromResult(academicCreditsIncludingCrossListed));

                // Return a different number of academic credits when include crosslisted is false.
                sectionIdsNotCrossListed = new List<string>() { "section1" };
                academicCreditsExcludingCrossListed = academicCreditsIncludingCrossListed.Where(ac => ac.Id == "1");
                academicCreditRepoMock.Setup(repo => repo.GetAcademicCreditsBySectionIdsAsync(sectionIdsNotCrossListed)).Returns(Task.FromResult(academicCreditsExcludingCrossListed));

                // sections = new TestSectionRepository().GetCachedSectionsAsync(sectionIds, false).Result;
                sections = BuildSectionEntities();
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(sectionIds, false)).Returns(Task.FromResult(sections));
                
                // Mock Adapters
                var academicHistoryDtoAdapter = new AcademicHistoryEntityToAcademicHistory3DtoAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicHistory, Ellucian.Colleague.Dtos.Student.AcademicHistory3>()).Returns(academicHistoryDtoAdapter);

                var academicTermDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicTerm, Ellucian.Colleague.Dtos.Student.AcademicTerm3>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicTerm, Ellucian.Colleague.Dtos.Student.AcademicTerm3>()).Returns(academicTermDtoAdapter);

                var academicCreditDtoAdapter = new AcademicCreditEntityToAcademicCredit2DtoAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicCredit, Ellucian.Colleague.Dtos.Student.AcademicCredit2>()).Returns(academicCreditDtoAdapter);

                var midTermGradeDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.MidTermGrade, Ellucian.Colleague.Dtos.Student.MidTermGrade2>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.MidTermGrade, Ellucian.Colleague.Dtos.Student.MidTermGrade2>()).Returns(midTermGradeDtoAdapter);

                var gradeDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Grade, Ellucian.Colleague.Dtos.Student.Grade>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Grade, Ellucian.Colleague.Dtos.Student.Grade>()).Returns(gradeDtoAdapter);

                // Set up current user
                currentUserFactory = new CurrentUserSetup.StudentUserFactory();
                termRepo = null;
                studentRepo = null;
                academicHistoryService = new AcademicHistoryService(adapterRegistry, studentRepo, academicCreditRepo, termRepo, sectionRepo, currentUserFactory, roleRepo, logger, baseConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentRepo = null;
                adapterRegistry = null;
                currentUserFactory = null;
                roleRepo = null;
                logger = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ThrowsErrorIfNoCriteriaProvided()
            {
                await academicHistoryService.QueryAcademicCreditsAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task ThrowsErrorIfNoSectionIdsInCriteria()
            {
                var criteria = new Ellucian.Colleague.Dtos.Student.AcademicCreditQueryCriteria();
                await academicHistoryService.QueryAcademicCreditsAsync(criteria);
            }

            [TestMethod]
            public async Task ReturnsNoCreditsWhenNoSectionsReturned()
            {
                var criteria = new Ellucian.Colleague.Dtos.Student.AcademicCreditQueryCriteria() { SectionIds = sectionIdsNotCrossListed.ToList() };
                IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> emptySections = new List<Ellucian.Colleague.Domain.Student.Entities.Section>();
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(sectionIdsNotCrossListed, false)).Returns(Task.FromResult(emptySections));
                await academicHistoryService.QueryAcademicCreditsAsync(criteria);
            }

            [TestMethod]
            public async Task ReturnsCredits_ExcludingCrossListedSections()
            {
                var criteria = new Ellucian.Colleague.Dtos.Student.AcademicCreditQueryCriteria() { SectionIds = sectionIds.ToList() };
                var academicCreditDtos = await academicHistoryService.QueryAcademicCreditsAsync(criteria);
                Assert.AreEqual(1, academicCreditDtos.Count());
                
            }

            [TestMethod]
            public async Task ReturnsCredits_IncludingCrossListedSections_Unfiltered()
            {
                var criteria = new Ellucian.Colleague.Dtos.Student.AcademicCreditQueryCriteria() { SectionIds = sectionIds.ToList() };
                criteria.IncludeCrossListedCredits = true;
                var academicCreditDtos = await academicHistoryService.QueryAcademicCreditsAsync(criteria);
                Assert.AreEqual(8, academicCreditDtos.Count());

            }

            [TestMethod]
            public async Task ReturnsCredits_IncludingCrossListedSections_WithSelectedStatuses()
            {
                var criteria = new Ellucian.Colleague.Dtos.Student.AcademicCreditQueryCriteria() { SectionIds = sectionIds.ToList() };
                criteria.IncludeCrossListedCredits = true;
                // Excluding dropped and withdrawn. Should reduct the returned number to 5.
                criteria.CreditStatuses = new List<Ellucian.Colleague.Dtos.Student.CreditStatus>() { Ellucian.Colleague.Dtos.Student.CreditStatus.Add, Ellucian.Colleague.Dtos.Student.CreditStatus.New, Ellucian.Colleague.Dtos.Student.CreditStatus.Preliminary, Ellucian.Colleague.Dtos.Student.CreditStatus.TransferOrNonCourse };

                var academicCreditDtos = await academicHistoryService.QueryAcademicCreditsAsync(criteria);
                Assert.AreEqual(5, academicCreditDtos.Count());

            }

            [TestMethod]
            public async Task ReturnsNoCredits_WhenSectionsAreNotForRequestor()
            {
                IEnumerable<string> singleSectionId = new List<string>() { "section2" };
                IEnumerable<Section> singleSection = sections.Where(s => s.Id == "section2");
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(singleSectionId, false)).Returns(Task.FromResult(singleSection));
                var criteria = new Ellucian.Colleague.Dtos.Student.AcademicCreditQueryCriteria() { SectionIds = singleSectionId.ToList() };

                var academicCreditDtos = await academicHistoryService.QueryAcademicCreditsAsync(criteria);
                Assert.AreEqual(0, academicCreditDtos.Count());

            }

            private IEnumerable<Section> BuildSectionEntities()
            {
                List<Section> sections = new List<Section>();
                var section1 = new Section("section1", "course1", "01", DateTime.Now.AddDays(-30), 3.0m, null, "Section Title 1", "G", new List<OfferingDepartment>() { new OfferingDepartment("MATH") }, new List<string>() {"100"}, "UG", new List<SectionStatusItem>() {new SectionStatusItem(SectionStatus.Active, "A", DateTime.Now )}, true, true, false, true, false, false);
                section1.AddFaculty("0000894");

                // This section has wrong faculty so it should NOT be in the list of sections when going to repo for academic credits.
                var section2 = new Section("section2", "course2", "02", DateTime.Now.AddDays(-30), 3.0m, null, "Section Title 2", "G", new List<OfferingDepartment>() { new OfferingDepartment("MATH") }, new List<string>() { "100" }, "UG", new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Now) }, true, true, false, true, false, false);
                section1.AddFaculty("0000222");

                // Thes sections are cross listed to section 1.
                var section3 = new Section("section3", "course3", "03", DateTime.Now.AddDays(-30), 3.0m, null, "Section Title 3", "G", new List<OfferingDepartment>() { new OfferingDepartment("MATH") }, new List<string>() { "100" }, "UG", new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Now) }, true, true, false, true, false, false);
                var section4 = new Section("section4", "course4", "04", DateTime.Now.AddDays(-30), 3.0m, null, "Section Title 4", "G", new List<OfferingDepartment>() { new OfferingDepartment("MATH") }, new List<string>() { "100" }, "UG", new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Now) }, true, true, false, true, false, false);
                section1.AddCrossListedSection(section3);
                section1.AddCrossListedSection(section4);

                // sections returned by the repo will be section 1 and 2 and of those only 1 is for this faculty.
                sections.Add(section1);
                sections.Add(section2);
                return sections;
            }
        }

        [TestClass]
        public class QueryAcademicCredits2Async : CurrentUserSetup
        {
            private AcademicHistoryService academicHistoryService;
            private Mock<IAcademicCreditRepository> academicCreditRepoMock;
            private IAcademicCreditRepository academicCreditRepo;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Domain.Student.Entities.Section section1;
            private Domain.Student.Entities.Section section2;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private ITermRepository termRepo;
            private IStudentRepository studentRepo;
            private IEnumerable<Domain.Student.Entities.AcademicCredit> academicCreditsIncludingCrossListed;
            private IEnumerable<Domain.Student.Entities.AcademicCredit> academicCreditsExcludingCrossListed;
            private IEnumerable<string> sectionIds;
            private IEnumerable<string> sectionIdsNotCrossListed;
            private IEnumerable<string> sectionIdsWithCrossListed;
            private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> sections;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            [TestInitialize]
            public void Initialize()
            {
                academicCreditRepoMock = new Mock<IAcademicCreditRepository>();
                academicCreditRepo = academicCreditRepoMock.Object;
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                var academicCreditIds = new List<string>() { "1", "3", "18", "19", "28", "29", "44", "45", "102" };
                sectionIds = new List<string>() { "section1", "section2" };
                sectionIdsWithCrossListed = new List<string>() { "section1", "section3", "section4" };
                academicCreditsIncludingCrossListed = new TestAcademicCreditRepository().GetAsync(academicCreditIds, false, false, false).Result;
                var academicCreditsIncludingCrossListedWithDrops = new TestAcademicCreditRepository().GetAsync(academicCreditIds, false, true, true).Result;
                academicCreditRepoMock.Setup(repo => repo.GetAcademicCreditsBySectionIdsAsync(sectionIdsWithCrossListed)).Returns(Task.FromResult(academicCreditsIncludingCrossListed));
                // Return a different number of academic credits when include crosslisted is false.
                sectionIdsNotCrossListed = new List<string>() { "section1" };
                academicCreditsExcludingCrossListed = academicCreditsIncludingCrossListed.Where(ac => ac.Id == "1");
                academicCreditRepoMock.Setup(repo => repo.GetAcademicCreditsBySectionIdsAsync(sectionIdsNotCrossListed)).Returns(Task.FromResult(academicCreditsExcludingCrossListed));

                sections = BuildSectionEntities();
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(sectionIds, false)).Returns(Task.FromResult(sections));

                // Mock Adapters
                var academicHistoryDtoAdapter = new AcademicHistoryEntityToAcademicHistory4DtoAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicHistory, Ellucian.Colleague.Dtos.Student.AcademicHistory4>()).Returns(academicHistoryDtoAdapter);

                var academicTermDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicTerm, Ellucian.Colleague.Dtos.Student.AcademicTerm3>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicTerm, Ellucian.Colleague.Dtos.Student.AcademicTerm3>()).Returns(academicTermDtoAdapter);

                var academicCreditDtoAdapter = new AcademicCreditEntityToAcademicCredit3DtoAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicCredit, Ellucian.Colleague.Dtos.Student.AcademicCredit3>()).Returns(academicCreditDtoAdapter);

                var midTermGradeDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.MidTermGrade, Ellucian.Colleague.Dtos.Student.MidTermGrade2>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.MidTermGrade, Ellucian.Colleague.Dtos.Student.MidTermGrade2>()).Returns(midTermGradeDtoAdapter);

                var gradeDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Grade, Ellucian.Colleague.Dtos.Student.Grade>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Grade, Ellucian.Colleague.Dtos.Student.Grade>()).Returns(gradeDtoAdapter);

                // Set up current user
                currentUserFactory = new CurrentUserSetup.StudentUserFactory();
                termRepo = null;
                studentRepo = null;
                academicHistoryService = new AcademicHistoryService(adapterRegistry, studentRepo, academicCreditRepo, termRepo, sectionRepo, currentUserFactory, roleRepo, logger, baseConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentRepo = null;
                adapterRegistry = null;
                currentUserFactory = null;
                roleRepo = null;
                logger = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ThrowsErrorIfNoCriteriaProvided()
            {
                await academicHistoryService.QueryAcademicCredits2Async(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task ThrowsErrorIfNoSectionIdsInCriteria()
            {
                var criteria = new Ellucian.Colleague.Dtos.Student.AcademicCreditQueryCriteria();
                await academicHistoryService.QueryAcademicCredits2Async(criteria);
            }

            [TestMethod]
            public async Task ReturnsNoCreditsWhenNoSectionsReturned()
            {
                var criteria = new Ellucian.Colleague.Dtos.Student.AcademicCreditQueryCriteria() { SectionIds = sectionIdsNotCrossListed.ToList() };
                IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> emptySections = new List<Ellucian.Colleague.Domain.Student.Entities.Section>();
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(sectionIdsNotCrossListed, false)).Returns(Task.FromResult(emptySections));
                await academicHistoryService.QueryAcademicCredits2Async(criteria);
            }

            [TestMethod]
            public async Task ReturnsCredits_ExcludingCrossListedSections()
            {
                var criteria = new Ellucian.Colleague.Dtos.Student.AcademicCreditQueryCriteria() { SectionIds = sectionIds.ToList() };
                var academicCreditDtos = await academicHistoryService.QueryAcademicCredits2Async(criteria);
                Assert.AreEqual(1, academicCreditDtos.Count());

            }

            [TestMethod]
            public async Task ReturnsCredits_IncludingCrossListedSections_Unfiltered()
            {
                var criteria = new Ellucian.Colleague.Dtos.Student.AcademicCreditQueryCriteria() { SectionIds = sectionIds.ToList(), IncludeCrossListedCredits = true };
                criteria.IncludeCrossListedCredits = true;
                var academicCreditDtos = await academicHistoryService.QueryAcademicCredits2Async(criteria);
                Assert.AreEqual(9, academicCreditDtos.Count());

            }

            [TestMethod]
            public async Task ReturnsCredits_IncludingCrossListedSections_WithSelectedStatuses()
            {
                var criteria = new Ellucian.Colleague.Dtos.Student.AcademicCreditQueryCriteria() { SectionIds = sectionIds.ToList() };
                criteria.IncludeCrossListedCredits = true;
                // Excluding dropped and withdrawn. Should reduct the returned number to 5.
                criteria.CreditStatuses = new List<Ellucian.Colleague.Dtos.Student.CreditStatus>() { Ellucian.Colleague.Dtos.Student.CreditStatus.Add, Ellucian.Colleague.Dtos.Student.CreditStatus.New, Ellucian.Colleague.Dtos.Student.CreditStatus.Preliminary, Ellucian.Colleague.Dtos.Student.CreditStatus.TransferOrNonCourse };

                var academicCreditDtos = await academicHistoryService.QueryAcademicCredits2Async(criteria);
                Assert.AreEqual(5, academicCreditDtos.Count());

            }

            [TestMethod]
            public async Task ReturnsNoCredits_WhenSectionsAreNotForRequestor()
            {
                IEnumerable<string> singleSectionId = new List<string>() { "section2" };
                IEnumerable<Section> singleSection = sections.Where(s => s.Id == "section2");
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(singleSectionId, false)).Returns(Task.FromResult(singleSection));
                var criteria = new Ellucian.Colleague.Dtos.Student.AcademicCreditQueryCriteria() { SectionIds = singleSectionId.ToList() };

                var academicCreditDtos = await academicHistoryService.QueryAcademicCredits2Async(criteria);
                Assert.AreEqual(0, academicCreditDtos.Count());

            }

            private IEnumerable<Section> BuildSectionEntities()
            {
                List<Section> sections = new List<Section>();
                var section1 = new Section("section1", "course1", "01", DateTime.Now.AddDays(-30), 3.0m, null, "Section Title 1", "G", new List<OfferingDepartment>() { new OfferingDepartment("MATH") }, new List<string>() { "100" }, "UG", new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Now) }, true, true, false, true, false, false);
                section1.AddFaculty("0000894");

                // This section has wrong faculty so it should NOT be in the list of sections when going to repo for academic credits.
                var section2 = new Section("section2", "course2", "02", DateTime.Now.AddDays(-30), 3.0m, null, "Section Title 2", "G", new List<OfferingDepartment>() { new OfferingDepartment("MATH") }, new List<string>() { "100" }, "UG", new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Now) }, true, true, false, true, false, false);
                section1.AddFaculty("0000222");

                // Thes sections are cross listed to section 1.
                var section3 = new Section("section3", "course3", "03", DateTime.Now.AddDays(-30), 3.0m, null, "Section Title 3", "G", new List<OfferingDepartment>() { new OfferingDepartment("MATH") }, new List<string>() { "100" }, "UG", new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Now) }, true, true, false, true, false, false);
                var section4 = new Section("section4", "course4", "04", DateTime.Now.AddDays(-30), 3.0m, null, "Section Title 4", "G", new List<OfferingDepartment>() { new OfferingDepartment("MATH") }, new List<string>() { "100" }, "UG", new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Now) }, true, true, false, true, false, false);
                section1.AddCrossListedSection(section3);
                section1.AddCrossListedSection(section4);

                // sections returned by the repo will be section 1 and 2 and of those only 1 is for this faculty.
                sections.Add(section1);
                sections.Add(section2);
                return sections;
            }
        }

    }
}
