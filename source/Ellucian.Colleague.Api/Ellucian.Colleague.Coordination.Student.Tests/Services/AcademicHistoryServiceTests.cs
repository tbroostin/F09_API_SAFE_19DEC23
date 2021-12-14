// Copyright 2016-2020 Ellucian Company L.P. and its affiliates.
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

        [TestClass]
        public class QueryAcademicCreditsWithInvalidKeysAsync : CurrentUserSetup
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
                academicCreditRepoMock.Setup(repo => repo.GetAcademicCreditsBySectionIdsWithInvalidKeysAsync(sectionIdsWithCrossListed)).Returns(Task.FromResult(new AcademicCreditsWithInvalidKeys(academicCreditsIncludingCrossListed, new List<string>())));
                // Return a different number of academic credits when include crosslisted is false.
                sectionIdsNotCrossListed = new List<string>() { "section1" };
                academicCreditsExcludingCrossListed = academicCreditsIncludingCrossListed.Where(ac => ac.Id == "1");
                academicCreditRepoMock.Setup(repo => repo.GetAcademicCreditsBySectionIdsWithInvalidKeysAsync(sectionIdsNotCrossListed)).Returns(Task.FromResult(new AcademicCreditsWithInvalidKeys(academicCreditsExcludingCrossListed, new List<string>())));

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
                await academicHistoryService.QueryAcademicCreditsWithInvalidKeysAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task ThrowsErrorIfNoSectionIdsInCriteria()
            {
                var criteria = new Ellucian.Colleague.Dtos.Student.AcademicCreditQueryCriteria();
                await academicHistoryService.QueryAcademicCreditsWithInvalidKeysAsync(criteria);
            }

            [TestMethod]
            public async Task ReturnsEmptyAcademicCreditsWithInvalidKeysWhenNoSectionsReturned()
            {
                var criteria = new Ellucian.Colleague.Dtos.Student.AcademicCreditQueryCriteria() { SectionIds = sectionIdsNotCrossListed.ToList() };
                IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> emptySections = new List<Ellucian.Colleague.Domain.Student.Entities.Section>();
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(sectionIdsNotCrossListed, false)).Returns(Task.FromResult(emptySections));
                Dtos.Student.AcademicCreditsWithInvalidKeys academicCredits = await academicHistoryService.QueryAcademicCreditsWithInvalidKeysAsync(criteria);
                Assert.AreEqual(0, academicCredits.AcademicCredits.Count());
                Assert.AreEqual(0, academicCredits.InvalidAcademicCreditIds.Count());
            }


            [TestMethod]
            public async Task ReturnsCredits_ExcludingCrossListedSections()
            {
                var criteria = new Ellucian.Colleague.Dtos.Student.AcademicCreditQueryCriteria() { SectionIds = sectionIds.ToList() };
                var academicCreditWithInvalidKeys = await academicHistoryService.QueryAcademicCreditsWithInvalidKeysAsync(criteria);
                Assert.AreEqual(1, academicCreditWithInvalidKeys.AcademicCredits.Count());
                Assert.AreEqual(0, academicCreditWithInvalidKeys.InvalidAcademicCreditIds.Count());

            }

            [TestMethod]
            public async Task ReturnsCredits_IncludingCrossListedSections_Unfiltered()
            {
                var criteria = new Ellucian.Colleague.Dtos.Student.AcademicCreditQueryCriteria() { SectionIds = sectionIds.ToList(), IncludeCrossListedCredits = true };
                criteria.IncludeCrossListedCredits = true;
                var academicCreditWithInvalidKeys = await academicHistoryService.QueryAcademicCreditsWithInvalidKeysAsync(criteria);
                Assert.AreEqual(9, academicCreditWithInvalidKeys.AcademicCredits.Count());

            }

            [TestMethod]
            public async Task ReturnsCredits_IncludingCrossListedSections_WithSelectedStatuses()
            {
                var criteria = new Ellucian.Colleague.Dtos.Student.AcademicCreditQueryCriteria() { SectionIds = sectionIds.ToList() };
                criteria.IncludeCrossListedCredits = true;
                // Excluding dropped and withdrawn. Should reduct the returned number to 5.
                criteria.CreditStatuses = new List<Ellucian.Colleague.Dtos.Student.CreditStatus>() { Ellucian.Colleague.Dtos.Student.CreditStatus.Add, Ellucian.Colleague.Dtos.Student.CreditStatus.New, Ellucian.Colleague.Dtos.Student.CreditStatus.Preliminary, Ellucian.Colleague.Dtos.Student.CreditStatus.TransferOrNonCourse };

                var academicCreditWithInvalidKeys = await academicHistoryService.QueryAcademicCreditsWithInvalidKeysAsync(criteria);
                Assert.AreEqual(5, academicCreditWithInvalidKeys.AcademicCredits.Count());

            }

            [TestMethod]
            public async Task ReturnsNoCredits_WhenSectionsAreNotForRequestor()
            {
                IEnumerable<string> singleSectionId = new List<string>() { "section2" };
                IEnumerable<Section> singleSection = sections.Where(s => s.Id == "section2");
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(singleSectionId, false)).Returns(Task.FromResult(singleSection));
                var criteria = new Ellucian.Colleague.Dtos.Student.AcademicCreditQueryCriteria() { SectionIds = singleSectionId.ToList() };

                var academicCreditWithInvalidKeys = await academicHistoryService.QueryAcademicCreditsWithInvalidKeysAsync(criteria);
                Assert.AreEqual(0, academicCreditWithInvalidKeys.AcademicCredits.Count());

            }

            [TestMethod]
            public async Task ReturnEmptyAcademicCreditsWithInvalidKeys_Dto_WithNullValues_FromRepository()
            {
                academicCreditRepoMock.Setup(repo => repo.GetAcademicCreditsBySectionIdsWithInvalidKeysAsync(sectionIdsWithCrossListed)).Returns(Task.FromResult<AcademicCreditsWithInvalidKeys>(null));
                var criteria = new Ellucian.Colleague.Dtos.Student.AcademicCreditQueryCriteria() { SectionIds = sectionIds.ToList(), IncludeCrossListedCredits = true };
                criteria.IncludeCrossListedCredits = true;
                var academicCreditWithInvalidKeys = await academicHistoryService.QueryAcademicCreditsWithInvalidKeysAsync(criteria);
                Assert.AreEqual(0, academicCreditWithInvalidKeys.AcademicCredits.Count());
                Assert.AreEqual(0, academicCreditWithInvalidKeys.InvalidAcademicCreditIds.Count());
            }

            [TestMethod]
            public async Task ReturnEmptyAcademicCreditsWithInvalidKeys_Dto_WithNullInvalidKeys_FromRepository()
            {
                academicCreditRepoMock.Setup(repo => repo.GetAcademicCreditsBySectionIdsWithInvalidKeysAsync(sectionIdsWithCrossListed)).Returns(Task.FromResult(new AcademicCreditsWithInvalidKeys(new List<Domain.Student.Entities.AcademicCredit>(), null)));
                var criteria = new Ellucian.Colleague.Dtos.Student.AcademicCreditQueryCriteria() { SectionIds = sectionIds.ToList(), IncludeCrossListedCredits = true };
                criteria.IncludeCrossListedCredits = true;
                var academicCreditWithInvalidKeys = await academicHistoryService.QueryAcademicCreditsWithInvalidKeysAsync(criteria);
                Assert.AreEqual(0, academicCreditWithInvalidKeys.AcademicCredits.Count());
                Assert.AreEqual(0, academicCreditWithInvalidKeys.InvalidAcademicCreditIds.Count());
            }

            [TestMethod]
            public async Task ReturnEmptyAcademicCreditsWithInvalidKeys_Dto_WithEmptyValues_FromRepository()
            {
                academicCreditRepoMock.Setup(repo => repo.GetAcademicCreditsBySectionIdsWithInvalidKeysAsync(sectionIdsWithCrossListed)).Returns(Task.FromResult(new AcademicCreditsWithInvalidKeys(new List<Domain.Student.Entities.AcademicCredit>(), new List<string>())));
                var criteria = new Ellucian.Colleague.Dtos.Student.AcademicCreditQueryCriteria() { SectionIds = sectionIds.ToList(), IncludeCrossListedCredits = true };
                criteria.IncludeCrossListedCredits = true;
                var academicCreditWithInvalidKeys = await academicHistoryService.QueryAcademicCreditsWithInvalidKeysAsync(criteria);
                Assert.AreEqual(0, academicCreditWithInvalidKeys.AcademicCredits.Count());
                Assert.AreEqual(0, academicCreditWithInvalidKeys.InvalidAcademicCreditIds.Count());
            }
            [TestMethod]
            public async Task ReturnEmptyAcademicCreditsWithInvalidKeys_Dto_WithOnlyInvalidKeysValues_FromRepository()
            {
                academicCreditRepoMock.Setup(repo => repo.GetAcademicCreditsBySectionIdsWithInvalidKeysAsync(sectionIdsWithCrossListed)).Returns(Task.FromResult(new AcademicCreditsWithInvalidKeys(new List<Domain.Student.Entities.AcademicCredit>(), new List<string>() { "999" })));
                var criteria = new Ellucian.Colleague.Dtos.Student.AcademicCreditQueryCriteria() { SectionIds = sectionIds.ToList(), IncludeCrossListedCredits = true };
                criteria.IncludeCrossListedCredits = true;
                var academicCreditWithInvalidKeys = await academicHistoryService.QueryAcademicCreditsWithInvalidKeysAsync(criteria);
                Assert.AreEqual(0, academicCreditWithInvalidKeys.AcademicCredits.Count());
                Assert.AreEqual(1, academicCreditWithInvalidKeys.InvalidAcademicCreditIds.Count());
            }
            [TestMethod]
            public async Task ReturnEmptyAcademicCreditsWithInvalidKeys_Dto_WithAcademicCreditsAndInvalidKeysValues_FromRepository()
            {
                academicCreditRepoMock.Setup(repo => repo.GetAcademicCreditsBySectionIdsWithInvalidKeysAsync(sectionIdsWithCrossListed)).Returns(Task.FromResult(new AcademicCreditsWithInvalidKeys(academicCreditsIncludingCrossListed, new List<string>() { "999", "9998" })));
                var criteria = new Ellucian.Colleague.Dtos.Student.AcademicCreditQueryCriteria() { SectionIds = sectionIds.ToList(), IncludeCrossListedCredits = true };
                criteria.IncludeCrossListedCredits = true;
                var academicCreditWithInvalidKeys = await academicHistoryService.QueryAcademicCreditsWithInvalidKeysAsync(criteria);
                Assert.AreEqual(9, academicCreditWithInvalidKeys.AcademicCredits.Count());
                Assert.AreEqual(2, academicCreditWithInvalidKeys.InvalidAcademicCreditIds.Count());
            }

            [TestMethod]
            public async Task AcademicHistoryService_QueryAcademicCreditsWithInvalidKeysAsync_UseCache_False_Calls_SectionRepository_GetNonCachedSectionsAsync()
            {
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(sectionIds, false)).ThrowsAsync(new ApplicationException("Wrong repo method was called!"));
                sectionRepoMock.Setup(repo => repo.GetNonCachedSectionsAsync(sectionIds, false)).Returns(Task.FromResult(sections));
                academicCreditRepoMock.Setup(repo => repo.GetAcademicCreditsBySectionIdsWithInvalidKeysAsync(sectionIdsWithCrossListed)).Returns(Task.FromResult(new AcademicCreditsWithInvalidKeys(academicCreditsIncludingCrossListed, new List<string>() { "999", "9998" })));
                var criteria = new Ellucian.Colleague.Dtos.Student.AcademicCreditQueryCriteria() { SectionIds = sectionIds.ToList(), IncludeCrossListedCredits = true };
                criteria.IncludeCrossListedCredits = true;
                var academicCreditWithInvalidKeys = await academicHistoryService.QueryAcademicCreditsWithInvalidKeysAsync(criteria, false);
                Assert.AreEqual(9, academicCreditWithInvalidKeys.AcademicCredits.Count());
                Assert.AreEqual(2, academicCreditWithInvalidKeys.InvalidAcademicCreditIds.Count());
            }

            [TestMethod]
            public async Task ReturnEmptyAcademicCreditsWithInvalidKeys_Dto_WithNull_FromRepository()
            {
                academicCreditRepoMock.Setup(repo => repo.GetAcademicCreditsBySectionIdsWithInvalidKeysAsync(sectionIdsWithCrossListed)).Returns(Task.FromResult<AcademicCreditsWithInvalidKeys>(null));
                var criteria = new Ellucian.Colleague.Dtos.Student.AcademicCreditQueryCriteria() { SectionIds = sectionIds.ToList(), IncludeCrossListedCredits = true };
                criteria.IncludeCrossListedCredits = true;
                var academicCreditWithInvalidKeys = await academicHistoryService.QueryAcademicCreditsWithInvalidKeysAsync(criteria);
                Assert.AreEqual(0, academicCreditWithInvalidKeys.AcademicCredits.Count());
                Assert.AreEqual(0, academicCreditWithInvalidKeys.InvalidAcademicCreditIds.Count());
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

        [TestClass]
        public class GetPilotAcademicHistoryLevelByIdsAsync : CurrentUserSetup
        {
            private AcademicHistoryService academicHistoryService;
            private Mock<IAcademicCreditRepository> academicCreditRepoMock;
            private IAcademicCreditRepository academicCreditRepo;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private Mock<ILogger> loggerMock;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private IStudentRepository studentRepo;
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
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                loggerMock = new Mock<ILogger>();
                logger = loggerMock.Object;
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                // Initialize current user
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();

                // Mock AdapterRegistry
                adapterRegistryMock.Setup(ar => ar.GetAdapter<PilotAcademicHistoryLevel, Ellucian.Colleague.Dtos.Student.PilotAcademicHistoryLevel>()).Returns(new PilotAcademicHistoryLevelDtoAdapter(adapterRegistry, logger));

                // Mock roles for current user
                Role advisorRole = new Role(1, currentUserFactory.CurrentUser.Roles.ElementAt(0));
                advisorRole.AddPermission(new Permission(StudentPermissionCodes.ViewStudentInformation));
                roleRepoMock.Setup(r => r.Roles).Returns(new List<Role>()
                {
                    advisorRole
                });

                // Mock AcademicCreditRepository
                academicCreditRepoMock.Setup(acr => acr.GetPilotCensusBooleanAsync()).ReturnsAsync(true);
                academicCreditRepoMock.Setup(acr => acr.ConvertCreditStatusAsync("A")).ReturnsAsync(CreditStatus.Add);
                academicCreditRepoMock.Setup(acr => acr.ConvertCreditStatusAsync("C")).ReturnsAsync(CreditStatus.Cancelled);
                academicCreditRepoMock.Setup(acr => acr.ConvertCreditStatusAsync("X")).ReturnsAsync(CreditStatus.Deleted);
                academicCreditRepoMock.Setup(acr => acr.ConvertCreditStatusAsync("D")).ReturnsAsync(CreditStatus.Dropped);
                academicCreditRepoMock.Setup(acr => acr.ConvertCreditStatusAsync("N")).ReturnsAsync(CreditStatus.New);
                academicCreditRepoMock.Setup(acr => acr.ConvertCreditStatusAsync("P")).ReturnsAsync(CreditStatus.Preliminary);
                academicCreditRepoMock.Setup(acr => acr.ConvertCreditStatusAsync("T")).ReturnsAsync(CreditStatus.TransferOrNonCourse);
                academicCreditRepoMock.Setup(acr => acr.ConvertCreditStatusAsync("U")).ReturnsAsync(CreditStatus.Unknown);
                academicCreditRepoMock.Setup(acr => acr.ConvertCreditStatusAsync("W")).ReturnsAsync(CreditStatus.Withdrawn);

                // Mock TermRepository
                List<Term> allTerms = new List<Term>()
                {
                    null, // Nulls should be handled gracefully
                    new Term("2020/SP1", "2020 Spring Term 1", DateTime.Today.AddDays(-90), DateTime.Today.AddDays(-10), 2020, 1, false, false, "2020", false),
                    new Term("2020/SP2", "2020 Spring Term 2",  DateTime.Today.AddDays(-10), DateTime.Today.AddDays(80), 2020, 2, false, false, "2020", false),
                    new Term("2020/SP3", "2020 Spring Term 3 - Ends Today",  DateTime.Today.AddDays(-10), DateTime.Today, 2020, 3, false, false, "2020", false),
                    new Term("2020/TODAY", "2020 Today", DateTime.Today, DateTime.Today, 2020, 4, false, false, "2020", false),
                    new Term("2020/SP4", "2020 Spring Term 4 - Starts Today",  DateTime.Today, DateTime.Today.AddDays(10), 2020, 5, false, false, "2020", false),
                    new Term("2020/SP5", "2020 Spring Term 5",  DateTime.Today.AddDays(10), DateTime.Today.AddDays(90), 2020, 6, false, false, "2020", false)
                };
                termRepoMock.Setup(tr => tr.GetAsync()).ReturnsAsync(allTerms);

                // Mock SectionRepository
                List<Section> cachedSections = new List<Section>()
                {
                    null, // Nulls should be handled gracefully
                    new Section("1", "1", "100", DateTime.Today.AddDays(-90), 3m, null, "Section starts and ends in past", "IN",
                        new List<OfferingDepartment>() { new OfferingDepartment("HIST") },
                        new List<string>() {"100"},
                        "UG",
                        new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-100)) }) { EndDate = DateTime.Today.AddDays(-10), TermId = "2020/SP1" },
                    new Section("2", "2", "101", DateTime.Today.AddDays(-10), 3m, null, "Section starts in past and ends in future", "IN",
                        new List<OfferingDepartment>() { new OfferingDepartment("HIST") },
                        new List<string>() {"100"},
                        "UG",
                        new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-20)) }) { EndDate = DateTime.Today.AddDays(80), TermId = "2020/SP2" },
                    new Section("3", "3", "102", DateTime.Today.AddDays(-10), 3m, null, "Section starts in past and ends today", "IN",
                        new List<OfferingDepartment>() { new OfferingDepartment("HIST") },
                        new List<string>() {"100"},
                        "UG",
                        new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-10)) }) { EndDate = DateTime.Today, TermId = "2020/SP3" },
                    new Section("4", "4", "103", DateTime.Today, 3m, null, "Section starts and ends today", "IN",
                        new List<OfferingDepartment>() { new OfferingDepartment("HIST") },
                        new List<string>() {"100"},
                        "UG",
                        new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today) }) { EndDate = DateTime.Today, TermId = "2020/TODAY" },
                    new Section("5", "5", "104", DateTime.Today, 3m, null, "Section starts today and ends in the future", "IN",
                        new List<OfferingDepartment>() { new OfferingDepartment("HIST") },
                        new List<string>() {"100"},
                        "UG",
                        new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-10)) }) { EndDate = DateTime.Today.AddDays(10), TermId = "2020/SP5" },
                    new Section("6", "6", "105", DateTime.Today.AddDays(10), 3m, null, "Section starts and ends in the future", "IN",
                        new List<OfferingDepartment>() { new OfferingDepartment("HIST") },
                        new List<string>() {"100"},
                        "UG",
                        new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today) }) { EndDate = DateTime.Today.AddDays(90), TermId = "2020/SP6" },

                };
                sectionRepoMock.Setup(sr => sr.GetCachedSectionsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>())).ReturnsAsync(cachedSections);

                // Initialize service
                academicHistoryService = new AcademicHistoryService(adapterRegistry, studentRepo, academicCreditRepo, termRepo, sectionRepo, currentUserFactory, roleRepo, logger, baseConfigurationRepository);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetPilotAcademicHistoryLevelByIdsAsync_null_StudentIds_throws_ArgumentNullException()
            {
                var acadHistoryLevel = await academicHistoryService.GetPilotAcademicHistoryLevelByIdsAsync(null, true, true);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetPilotAcademicHistoryLevelByIdsAsync_empty_StudentIds_throws_ArgumentNullException()
            {
                var acadHistoryLevel = await academicHistoryService.GetPilotAcademicHistoryLevelByIdsAsync(new List<string>(), true, true);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetPilotAcademicHistoryLevelByIdsAsync_list_with_null_empty_only_StudentIds_throws_ArgumentNullException()
            {
                var acadHistoryLevel = await academicHistoryService.GetPilotAcademicHistoryLevelByIdsAsync(new List<string>() { null, string.Empty }, true, true);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task GetPilotAcademicHistoryLevelByIdsAsync_user_without_VIEW_STUDENT_INFORMATION_permission_throws_PermissionsException()
            {
                // Mock roles for current user
                Role advisorRole = new Role(1, currentUserFactory.CurrentUser.Roles.ElementAt(0));
                roleRepoMock.Setup(r => r.Roles).Returns(new List<Role>()
                {
                    advisorRole
                });

                // Initialize service
                academicHistoryService = new AcademicHistoryService(adapterRegistry, studentRepo, academicCreditRepo, termRepo, sectionRepo, currentUserFactory, roleRepo, logger, baseConfigurationRepository);

                // Call service method
                var acadHistoryLevel = await academicHistoryService.GetPilotAcademicHistoryLevelByIdsAsync(new List<string>() { currentUserFactory.CurrentUser.PersonId }, true, true);
            }

            [TestMethod]
            public async Task GetPilotAcademicHistoryLevelByIdsAsync_UseCensusDate_False_returns_earliest_term()
            {
                // Modify mocks
                academicCreditRepoMock.Setup(acr => acr.GetPilotCensusBooleanAsync()).ReturnsAsync(false);

                Dictionary<string, List<PilotAcademicCredit>> acadCreditDict = new Dictionary<string, List<PilotAcademicCredit>>();
                acadCreditDict.Add(currentUserFactory.CurrentUser.PersonId, new List<PilotAcademicCredit>()
                {
                    null, // Nulls should be handled gracefully
                    new PilotAcademicCredit("1") { Status = CreditStatus.Add, TermCode = "2020/SP1", SectionId = "1" },
                    new PilotAcademicCredit("2") { Status = CreditStatus.Add, TermCode = "2020/SP2", AcademicLevelCode = "UG" },
                    new PilotAcademicCredit("3") { Status = CreditStatus.Add, SectionId = "3", AcademicLevelCode = "UG" },
                    new PilotAcademicCredit("4") { Status = CreditStatus.Dropped, TermCode = "2020/SP4", SectionId = "4", AcademicLevelCode = "UG" },
                    new PilotAcademicCredit("5") { Status = CreditStatus.New, TermCode = "2020/SP5", SectionId = "5", AcademicLevelCode = "UG" },
                });
                academicCreditRepoMock.Setup(acr => acr.GetPilotAcademicCreditsByStudentIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<AcademicCreditDataSubset>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>())).ReturnsAsync(acadCreditDict);

                // Initialize service
                academicHistoryService = new AcademicHistoryService(adapterRegistry, studentRepo, academicCreditRepo, termRepo, sectionRepo, currentUserFactory, roleRepo, logger, baseConfigurationRepository);

                // Call service method
                var acadHistoryLevel = await academicHistoryService.GetPilotAcademicHistoryLevelByIdsAsync(new List<string>() { currentUserFactory.CurrentUser.PersonId }, true, true);
                Assert.IsNotNull(acadHistoryLevel);
                Assert.AreEqual(1, acadHistoryLevel.Count());
                Assert.AreEqual("2020/SP2", acadHistoryLevel.ElementAt(0).FirstTermEnrolled);
            }

            [TestMethod]
            public async Task GetPilotAcademicHistoryLevelByIdsAsync_UseCensusDate_True_returns_correct_term()
            {
                // Modify mocks
                academicCreditRepoMock.Setup(acr => acr.GetPilotCensusBooleanAsync()).ReturnsAsync(true);

                Dictionary<string, List<PilotAcademicCredit>> acadCreditDict = new Dictionary<string, List<PilotAcademicCredit>>();
                acadCreditDict.Add(currentUserFactory.CurrentUser.PersonId, new List<PilotAcademicCredit>()
                {
                    null, // Nulls should be handled gracefully
                    new PilotAcademicCredit("1") { Status = CreditStatus.Add, TermCode = "2020/SP1", SectionId = "1" },
                    new PilotAcademicCredit("2") { Status = CreditStatus.Add, TermCode = "2020/SP2", AcademicLevelCode = "UG" },
                    new PilotAcademicCredit("3") { Status = CreditStatus.Add, SectionId = "3", AcademicLevelCode = "UG" },
                    new PilotAcademicCredit("4") { Status = CreditStatus.Dropped, TermCode = "2020/SP4", SectionId = "4", AcademicLevelCode = "UG" },
                    new PilotAcademicCredit("5") { Status = CreditStatus.New, TermCode = "2020/SP5", SectionId = "5", AcademicLevelCode = "UG" },
                });
                academicCreditRepoMock.Setup(acr => acr.GetPilotAcademicCreditsByStudentIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<AcademicCreditDataSubset>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>())).ReturnsAsync(acadCreditDict);

                // Initialize service
                academicHistoryService = new AcademicHistoryService(adapterRegistry, studentRepo, academicCreditRepo, termRepo, sectionRepo, currentUserFactory, roleRepo, logger, baseConfigurationRepository);

                // Call service method
                var acadHistoryLevel = await academicHistoryService.GetPilotAcademicHistoryLevelByIdsAsync(new List<string>() { currentUserFactory.CurrentUser.PersonId }, true, true);
                Assert.IsNotNull(acadHistoryLevel);
                Assert.AreEqual(1, acadHistoryLevel.Count());
                Assert.AreEqual("2020/SP5", acadHistoryLevel.ElementAt(0).FirstTermEnrolled);
            }


            [TestMethod]
            public async Task GetPilotAcademicHistoryLevelByIdsAsync_UseCensusDate_True_Section_Census_Dates_returns_correct_term()
            {
                // Modify mocks
                List<Section> cachedSections = new List<Section>()
                {
                    null, // Nulls should be handled gracefully
                    new Section("1", "1", "100", DateTime.Today.AddDays(-90), 3m, null, "Section starts and ends in past", "IN",
                        new List<OfferingDepartment>() { new OfferingDepartment("HIST") },
                        new List<string>() {"100"},
                        "UG",
                        new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-100)) }) { EndDate = DateTime.Today.AddDays(-10), TermId = "2020/SP1",
                        RegistrationDateOverrides = new RegistrationDate("", null, null, null, null, null, null, null, null, null, new List<DateTime?>() { DateTime.Today.AddDays(-180) }) },
                    new Section("2", "2", "101", DateTime.Today.AddDays(-10), 3m, null, "Section starts in past and ends in future", "IN",
                        new List<OfferingDepartment>() { new OfferingDepartment("HIST") },
                        new List<string>() {"100"},
                        "UG",
                        new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-20)) }) { EndDate = DateTime.Today.AddDays(80), TermId = "2020/SP2",
                        RegistrationDateOverrides = new RegistrationDate("", null, null, null, null, null, null, null, null, null, new List<DateTime?>() { DateTime.Today.AddDays(-270) })},
                    new Section("3", "3", "102", DateTime.Today.AddDays(-10), 3m, null, "Section starts in past and ends today", "IN",
                        new List<OfferingDepartment>() { new OfferingDepartment("HIST") },
                        new List<string>() {"100"},
                        "UG",
                        new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-10)) }) { EndDate = DateTime.Today, TermId = "2020/SP3" },
                    new Section("4", "4", "103", DateTime.Today, 3m, null, "Section starts and ends today", "IN",
                        new List<OfferingDepartment>() { new OfferingDepartment("HIST") },
                        new List<string>() {"100"},
                        "UG",
                        new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today) }) { EndDate = DateTime.Today, TermId = "2020/TODAY" },
                    new Section("5", "5", "104", DateTime.Today, 3m, null, "Section starts today and ends in the future", "IN",
                        new List<OfferingDepartment>() { new OfferingDepartment("HIST") },
                        new List<string>() {"100"},
                        "UG",
                        new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-10)) }) { EndDate = DateTime.Today.AddDays(10), TermId = "2020/SP5" },
                    new Section("6", "6", "105", DateTime.Today.AddDays(10), 3m, null, "Section starts and ends in the future", "IN",
                        new List<OfferingDepartment>() { new OfferingDepartment("HIST") },
                        new List<string>() {"100"},
                        "UG",
                        new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today) }) { EndDate = DateTime.Today.AddDays(90), TermId = "2020/SP6" },

                };
                sectionRepoMock.Setup(sr => sr.GetCachedSectionsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>())).ReturnsAsync(cachedSections);

                academicCreditRepoMock.Setup(acr => acr.GetPilotCensusBooleanAsync()).ReturnsAsync(true);

                Dictionary<string, List<PilotAcademicCredit>> acadCreditDict = new Dictionary<string, List<PilotAcademicCredit>>();
                acadCreditDict.Add(currentUserFactory.CurrentUser.PersonId, new List<PilotAcademicCredit>()
                {
                    null, // Nulls should be handled gracefully
                    new PilotAcademicCredit("1") { Status = CreditStatus.Add, TermCode = "2020/SP1", AcademicLevelCode = "UG", SectionId = "1", AcademicCreditStatuses = new List<AcademicCreditStatus>() { new AcademicCreditStatus("A", DateTime.Today.AddDays(-179), DateTime.Now, "X") } },
                    new PilotAcademicCredit("2") { Status = CreditStatus.Add, TermCode = "2020/SP2", AcademicLevelCode = "UG", SectionId = "2", AcademicCreditStatuses = new List<AcademicCreditStatus>() { new AcademicCreditStatus("A", DateTime.Today.AddDays(-271), DateTime.Now, "X") } },
                    new PilotAcademicCredit("3") { Status = CreditStatus.Add, SectionId = "3", AcademicLevelCode = "UG" },
                    new PilotAcademicCredit("4") { Status = CreditStatus.Dropped, TermCode = "2020/SP4", SectionId = "4", AcademicLevelCode = "UG" },
                    new PilotAcademicCredit("5") { Status = CreditStatus.New, TermCode = "2020/SP5", SectionId = "5", AcademicLevelCode = "UG" },
                });
                academicCreditRepoMock.Setup(acr => acr.GetPilotAcademicCreditsByStudentIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<AcademicCreditDataSubset>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>())).ReturnsAsync(acadCreditDict);

                // Initialize service
                academicHistoryService = new AcademicHistoryService(adapterRegistry, studentRepo, academicCreditRepo, termRepo, sectionRepo, currentUserFactory, roleRepo, logger, baseConfigurationRepository);

                // Call service method
                var acadHistoryLevel = await academicHistoryService.GetPilotAcademicHistoryLevelByIdsAsync(new List<string>() { currentUserFactory.CurrentUser.PersonId }, true, true);
                Assert.IsNotNull(acadHistoryLevel);
                Assert.AreEqual(1, acadHistoryLevel.Count());
                Assert.AreEqual("2020/SP2", acadHistoryLevel.ElementAt(0).FirstTermEnrolled);
            }

            [TestMethod]
            public async Task GetPilotAcademicHistoryLevelByIdsAsync_UseCensusDate_True_Term_Location_Census_Dates_returns_correct_term()
            {
                // Modify mocks
                List<Section> cachedSections = new List<Section>()
                {
                    null, // Nulls should be handled gracefully
                    new Section("1", "1", "100", DateTime.Today.AddDays(-90), 3m, null, "Section starts and ends in past", "IN",
                        new List<OfferingDepartment>() { new OfferingDepartment("HIST") },
                        new List<string>() {"100"},
                        "UG",
                        new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-100)) }) { EndDate = DateTime.Today.AddDays(-10), TermId = "2020/SP1" },
                    new Section("2", "2", "101", DateTime.Today.AddDays(-10), 3m, null, "Section starts in past and ends in future", "IN",
                        new List<OfferingDepartment>() { new OfferingDepartment("HIST") },
                        new List<string>() {"100"},
                        "UG",
                        new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-20)) }) { EndDate = DateTime.Today.AddDays(80), TermId = "2020/SP2" }
                };
                sectionRepoMock.Setup(sr => sr.GetCachedSectionsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>())).ReturnsAsync(cachedSections);

                academicCreditRepoMock.Setup(acr => acr.GetPilotCensusBooleanAsync()).ReturnsAsync(true);

                Dictionary<string, List<PilotAcademicCredit>> acadCreditDict = new Dictionary<string, List<PilotAcademicCredit>>();
                acadCreditDict.Add(currentUserFactory.CurrentUser.PersonId, new List<PilotAcademicCredit>()
                {
                    null, // Nulls should be handled gracefully
                    new PilotAcademicCredit("1") { Status = CreditStatus.Add, TermCode = "2020/SP1", AcademicLevelCode = "UG", Location = "MC", SectionId = "1", AcademicCreditStatuses = new List<AcademicCreditStatus>() { new AcademicCreditStatus("A", DateTime.Today.AddDays(-179), DateTime.Now, "X") } },
                    new PilotAcademicCredit("2") { Status = CreditStatus.Add, TermCode = "2020/SP2", AcademicLevelCode = "UG", Location = "SC", SectionId = "2", AcademicCreditStatuses = new List<AcademicCreditStatus>() { new AcademicCreditStatus("A", DateTime.Today.AddDays(-271), DateTime.Now, "X") } },
                });
                academicCreditRepoMock.Setup(acr => acr.GetPilotAcademicCreditsByStudentIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<AcademicCreditDataSubset>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>())).ReturnsAsync(acadCreditDict);

                var term1 = new Term("2020/SP1", "2020 Spring Term 1", DateTime.Today.AddDays(-90), DateTime.Today.AddDays(-10), 2020, 1, false, false, "2020", false);
                term1.AddRegistrationDates(new RegistrationDate("MC", null, null, null, null, null, null, null, null, null, new List<DateTime?>() { DateTime.Today.AddDays(-180) }));
                var term2 = new Term("2020/SP2", "2020 Spring Term 2", DateTime.Today.AddDays(-10), DateTime.Today.AddDays(80), 2020, 2, false, false, "2020", false);
                term2.AddRegistrationDates(new RegistrationDate("SC", null, null, null, null, null, null, null, null, null, new List<DateTime?>() { DateTime.Today.AddDays(-270) }));

                List<Term> allTerms = new List<Term>()
                {
                    term1,
                    term2,
                };
                termRepoMock.Setup(tr => tr.GetAsync()).ReturnsAsync(allTerms);

                // Initialize service
                academicHistoryService = new AcademicHistoryService(adapterRegistry, studentRepo, academicCreditRepo, termRepo, sectionRepo, currentUserFactory, roleRepo, logger, baseConfigurationRepository);

                // Call service method
                var acadHistoryLevel = await academicHistoryService.GetPilotAcademicHistoryLevelByIdsAsync(new List<string>() { currentUserFactory.CurrentUser.PersonId }, true, true);
                Assert.IsNotNull(acadHistoryLevel);
                Assert.AreEqual(1, acadHistoryLevel.Count());
                Assert.AreEqual("2020/SP2", acadHistoryLevel.ElementAt(0).FirstTermEnrolled);
            }

            [TestMethod]
            public async Task GetPilotAcademicHistoryLevelByIdsAsync_UseCensusDate_True_Term_Census_Dates_returns_correct_term()
            {
                // Modify mocks
                List<Section> cachedSections = new List<Section>()
                {
                    null, // Nulls should be handled gracefully
                    new Section("1", "1", "100", DateTime.Today.AddDays(-90), 3m, null, "Section starts and ends in past", "IN",
                        new List<OfferingDepartment>() { new OfferingDepartment("HIST") },
                        new List<string>() {"100"},
                        "UG",
                        new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-100)) }) { EndDate = DateTime.Today.AddDays(-10), TermId = "2020/SP1" },
                    new Section("2", "2", "101", DateTime.Today.AddDays(-10), 3m, null, "Section starts in past and ends in future", "IN",
                        new List<OfferingDepartment>() { new OfferingDepartment("HIST") },
                        new List<string>() {"100"},
                        "UG",
                        new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-20)) }) { EndDate = DateTime.Today.AddDays(80), TermId = "2020/SP2" }
                };
                sectionRepoMock.Setup(sr => sr.GetCachedSectionsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>())).ReturnsAsync(cachedSections);

                academicCreditRepoMock.Setup(acr => acr.GetPilotCensusBooleanAsync()).ReturnsAsync(true);

                Dictionary<string, List<PilotAcademicCredit>> acadCreditDict = new Dictionary<string, List<PilotAcademicCredit>>();
                acadCreditDict.Add(currentUserFactory.CurrentUser.PersonId, new List<PilotAcademicCredit>()
                {
                    null, // Nulls should be handled gracefully
                    new PilotAcademicCredit("1") { Status = CreditStatus.Add, TermCode = "2020/SP1", AcademicLevelCode = "UG", Location = "MC", SectionId = "1", AcademicCreditStatuses = new List<AcademicCreditStatus>() { new AcademicCreditStatus("A", DateTime.Today.AddDays(-179), DateTime.Now, "X") } },
                    new PilotAcademicCredit("2") { Status = CreditStatus.Add, TermCode = "2020/SP2", AcademicLevelCode = "UG", Location = "SC", SectionId = "2", AcademicCreditStatuses = new List<AcademicCreditStatus>() { new AcademicCreditStatus("A", DateTime.Today.AddDays(-271), DateTime.Now, "X") } },
                });
                academicCreditRepoMock.Setup(acr => acr.GetPilotAcademicCreditsByStudentIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<AcademicCreditDataSubset>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>())).ReturnsAsync(acadCreditDict);

                var term1 = new Term("2020/SP1", "2020 Spring Term 1", DateTime.Today.AddDays(-90), DateTime.Today.AddDays(-10), 2020, 1, false, false, "2020", false);
                term1.AddRegistrationDates(new RegistrationDate(string.Empty, null, null, null, null, null, null, null, null, null, new List<DateTime?>() { DateTime.Today.AddDays(-180) }));
                var term2 = new Term("2020/SP2", "2020 Spring Term 2", DateTime.Today.AddDays(-10), DateTime.Today.AddDays(80), 2020, 2, false, false, "2020", false);
                term2.AddRegistrationDates(new RegistrationDate(string.Empty, null, null, null, null, null, null, null, null, null, new List<DateTime?>() { DateTime.Today.AddDays(-270) }));

                List<Term> allTerms = new List<Term>()
                {
                    term1,
                    term2,
                };
                termRepoMock.Setup(tr => tr.GetAsync()).ReturnsAsync(allTerms);

                // Initialize service
                academicHistoryService = new AcademicHistoryService(adapterRegistry, studentRepo, academicCreditRepo, termRepo, sectionRepo, currentUserFactory, roleRepo, logger, baseConfigurationRepository);

                // Call service method
                var acadHistoryLevel = await academicHistoryService.GetPilotAcademicHistoryLevelByIdsAsync(new List<string>() { currentUserFactory.CurrentUser.PersonId }, true, true);
                Assert.IsNotNull(acadHistoryLevel);
                Assert.AreEqual(1, acadHistoryLevel.Count());
                Assert.AreEqual("2020/SP2", acadHistoryLevel.ElementAt(0).FirstTermEnrolled);
            }

        }

        [TestClass]
        public class GetAcademicHistory5_AsStudentUser : CurrentUserSetup
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
                //1,2,3,4 are from 2009/SP 8, 11from 2009/FA  14 frm 2010/SP
                List<string> academicCreditIds = new List<string>() { "1", "2", "3", "4", "8", "11", "14" };
                student1 = new Domain.Student.Entities.Student("0000894", "Smith", 2, new List<string>() { "BA.ENGL" }, academicCreditIds) { FirstName = "Bob", MiddleName = "Blakely" };
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);


                // Mock another student ....
                student2 = new Domain.Student.Entities.Student("00004002", "Jones", 802, new List<string>() { "BA.MATH" }, academicCreditIds);
                studentRepoMock.Setup(repo => repo.GetAsync("00004002")).ReturnsAsync(student2);



                IEnumerable<Domain.Student.Entities.AcademicCredit> academicCredits = new TestAcademicCreditRepository().GetAsync(academicCreditIds).Result;
                Dictionary<string, List<Domain.Student.Entities.AcademicCredit>> dictAcademicCredits = new Dictionary<string, List<AcademicCredit>>();
                dictAcademicCredits.Add("0000894", academicCredits.ToList());
                IEnumerable<string> studentIds = new List<string>() { "0000894" };
                academicCreditRepoMock.Setup(repo => repo.GetAcademicCreditByStudentIdsAsync(studentIds, false, false, It.IsAny<bool>())).Returns(Task.FromResult(dictAcademicCredits));

                // Mock get grade restriction
                GradeRestriction studentGradeRestriction = new GradeRestriction(false);
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
                Dictionary<string, List<Domain.Student.Entities.AcademicCredit>> dictAcademicCredits = new Dictionary<string, List<AcademicCredit>>();
                dictAcademicCredits.Add("0004002", new List<AcademicCredit>());
                IEnumerable<string> studentIds = new List<string>() { "0004002" };
                academicCreditRepoMock.Setup(repo => repo.GetAcademicCreditByStudentIdsAsync(studentIds, false, false, It.IsAny<bool>())).Returns(Task.FromResult(dictAcademicCredits));

                studentRepoMock.Setup(repo => repo.GetAsync("0004002")).ReturnsAsync(student2);
                await academicHistoryService.GetAcademicHistory5Async("0004002", false, false);
            }

            [TestMethod]
            public async Task GradeRestrictedStudent_NoGrades()
            {
                // Mock get grade restriction
                GradeRestriction studentGradeRestriction = new GradeRestriction(true);
                studentGradeRestriction.AddReason("Bad");
                studentRepoMock.Setup(srepo => srepo.GetGradeRestrictionsAsync(It.IsAny<string>())).Returns(Task.FromResult(studentGradeRestriction));
                // Student is himself.  But he has a grade restriction. should be no grades returned.
                var result = await academicHistoryService.GetAcademicHistory5Async("0000894", false, false);
                Assert.IsTrue(result is Dtos.Student.AcademicHistory4);
                Assert.IsTrue(result.GradeRestriction.IsRestricted);
                var term = result.AcademicTerms.First();
                var cred = term.AcademicCredits.First();
                Assert.IsNull(cred.VerifiedGradeId);
            }

            [TestMethod]
            public async Task GetAcademicHistory_WithAllTheAcadCredits()
            {
                var result = await academicHistoryService.GetAcademicHistory5Async("0000894", false, false);
                Assert.IsTrue(result is Dtos.Student.AcademicHistory4);
                Assert.IsFalse(result.GradeRestriction.IsRestricted);
                Assert.AreEqual(3, result.AcademicTerms.Count);
                Assert.AreEqual(4, result.AcademicTerms[0].AcademicCredits.Count);
                Assert.AreEqual("2009/SP", result.AcademicTerms[0].TermId);
                Assert.AreEqual(2, result.AcademicTerms[1].AcademicCredits.Count);
                Assert.AreEqual("2009/FA", result.AcademicTerms[1].TermId);
                Assert.AreEqual(1, result.AcademicTerms[2].AcademicCredits.Count);
                Assert.AreEqual("2010/SP", result.AcademicTerms[2].TermId);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task GetAcademicHistory_WhenRepoReturnsNull()
            {
                IEnumerable<string> studentIds = new List<string>() { "0000894" };
                academicCreditRepoMock.Setup(repo => repo.GetAcademicCreditByStudentIdsAsync(studentIds, false, false, It.IsAny<bool>())).ReturnsAsync(() => null);

                var result = await academicHistoryService.GetAcademicHistory5Async("0000894", false, false);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task GetAcademicHistory_WhenRepoReturnsDict_WithNotTheSamestudent()
            {
                var result = await academicHistoryService.GetAcademicHistory5Async("isnotthere", false, false);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task GetAcademicHistory_WhenRepoThrowsException()
            {
                IEnumerable<string> studentIds = new List<string>() { "0000894" };
                academicCreditRepoMock.Setup(repo => repo.GetAcademicCreditByStudentIdsAsync(studentIds, false, false, It.IsAny<bool>())).Throws(new Exception("repo error"));
                var result = await academicHistoryService.GetAcademicHistory5Async("0000894", false, false);
            }

            [TestMethod]
            public async Task GetAcademicHistory_FilterOnTerm_WhenTermExits()
            {
                var result = await academicHistoryService.GetAcademicHistory5Async("0000894", false, false, term: "2009/SP");
                Assert.IsTrue(result is Dtos.Student.AcademicHistory4);
                Assert.IsFalse(result.GradeRestriction.IsRestricted);
                Assert.AreEqual(1, result.AcademicTerms.Count);
                Assert.AreEqual(4, result.AcademicTerms[0].AcademicCredits.Count);
            }
            [TestMethod]
            public async Task GetAcademicHistory_FilterOnTerm_WhenTermDoesNotExits()
            {
                var result = await academicHistoryService.GetAcademicHistory5Async("0000894", false, false, term: "notexist");
                Assert.IsTrue(result is Dtos.Student.AcademicHistory4);
                Assert.IsFalse(result.GradeRestriction.IsRestricted);
                Assert.AreEqual(0, result.AcademicTerms.Count);
            }


        }

        [TestClass]
        public class GetAcademicHistory5_AsNonStudent : CurrentUserSetup
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
                Dictionary<string, List<Domain.Student.Entities.AcademicCredit>> dictAcademicCredits = new Dictionary<string, List<AcademicCredit>>();
                dictAcademicCredits.Add("0000894", academicCredits.ToList());
                IEnumerable<string> studentIds = new List<string>() { "0000894" };
                academicCreditRepoMock.Setup(repo => repo.GetAcademicCreditByStudentIdsAsync(studentIds, false, false, It.IsAny<bool>())).Returns(Task.FromResult(dictAcademicCredits));

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
                var result = await academicHistoryService.GetAcademicHistory5Async("0000894", false, false);
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
                var result = await academicHistoryService.GetAcademicHistory5Async("0000894", false, false);
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
                var result = await academicHistoryService.GetAcademicHistory5Async("0000894", false, false);
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
                var result = await academicHistoryService.GetAcademicHistory5Async("0000894", false, false);
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
                var result = await academicHistoryService.GetAcademicHistory5Async("0000894", false, false);
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
                var result = await academicHistoryService.GetAcademicHistory5Async("0000894", false, false);
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
                var result = await academicHistoryService.GetAcademicHistory5Async("0000894", false, false);
            }
        }
    }
}
