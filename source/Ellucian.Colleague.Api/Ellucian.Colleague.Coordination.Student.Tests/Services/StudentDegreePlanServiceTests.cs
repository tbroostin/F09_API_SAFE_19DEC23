// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Web.Security;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Coordination.Base.Adapters;
using Ellucian.Colleague.Coordination.Student.Adapters;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Entities.DegreePlans;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Web.License;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class StudentDegreePlanServiceTests
    {
        // Sets up a Current user that is a student
        public abstract class CurrentUserSetup
        {
            protected Role advisorRole = new Role(105, "Advisor");
            protected Role facultyRole = new Role(999, "Faculty");

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
                            PersonId = "0011902",
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
        public abstract class StudentDegreePlanServiceTestsSetup : CurrentUserSetup
        {
            protected ITermRepository termRepo;
            protected Mock<ITermRepository> termRepoMock;
            protected ICourseRepository courseRepo;
            protected Mock<ICourseRepository> courseRepoMock;
            protected ISectionRepository sectionRepo;
            protected Mock<ISectionRepository> sectionRepoMock;
            protected IProgramRepository programRepo;
            protected Mock<IProgramRepository> programRepoMock;
            protected ICatalogRepository catalogRepo;
            protected Mock<ICatalogRepository> catalogRepoMock;
            protected IRuleRepository ruleRepo;
            protected Mock<IRuleRepository> ruleRepoMock;
            protected IRequirementRepository requirementRepo;
            protected Mock<IRequirementRepository> requirementRepoMock;
            protected IProgramRequirementsRepository progReqRepo;
            protected Mock<IProgramRequirementsRepository> progReqRepoMock;
            protected IAcademicCreditRepository academicCreditRepo;
            protected Mock<IAcademicCreditRepository> academicCreditRepoMock;
            protected IStudentRepository studentRepo;
            protected Mock<IStudentRepository> studentRepoMock;
            protected IStudentDegreePlanRepository studentDegreePlanRepo;
            protected Mock<IStudentDegreePlanRepository> studentDegreePlanRepoMock;
            protected IStudentProgramRepository studentProgramRepo;
            protected Mock<IStudentProgramRepository> studentProgramRepoMock;
            protected IAdapterRegistry adapterRegistry;
            protected Mock<IAdapterRegistry> adapterRegistryMock;
            protected IRoleRepository roleRepo;
            protected Mock<IRoleRepository> roleRepoMock;
            protected IGradeRepository gradeRepo;
            protected Mock<IGradeRepository> gradeRepoMock;
            protected IConfigurationRepository baseConfigurationRepository;
            protected Mock<IConfigurationRepository> baseConfigurationRepositoryMock;
            protected IStudentConfigurationRepository studentConfigurationRepository;
            protected Mock<IStudentConfigurationRepository> studentConfigurationRepositoryMock;

            protected IAcademicHistoryService academicHistoryService;
            protected Mock<IAcademicHistoryService> academicHistoryServiceMock;

            protected IStudentDegreePlanService studentDegreePlanService;

            protected ILogger logger;
            protected ICurrentUserFactory currentUserFactory;

  



            protected IAcademicCreditRepository academicCreditRepoInstance = new TestAcademicCreditRepository();

            private Dictionary<string, List<Domain.Student.Entities.AcademicCredit>> GetCredits(IEnumerable<string> studentIds)
            {
                var creditDictionary = new Dictionary<string, List<Domain.Student.Entities.AcademicCredit>>();
                foreach (var id in studentIds)
                {
                    var student = studentRepo.GetAsync(id).Result; //TODO: Fix blocking call
                    var acadCreditIds = student.AcademicCreditIds;

                    var credits = academicCreditRepoInstance.GetAsync(acadCreditIds).Result.ToList();

                    creditDictionary.Add(id, credits);
                }

                return creditDictionary;
            }

            public void SetupInitialize()
            {
                studentDegreePlanRepoMock = new Mock<IStudentDegreePlanRepository>();
                studentDegreePlanRepo = studentDegreePlanRepoMock.Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                studentProgramRepoMock = new Mock<IStudentProgramRepository>();
                studentProgramRepo = studentProgramRepoMock.Object;
                programRepoMock = new Mock<IProgramRepository>();
                programRepo = programRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                courseRepoMock = new Mock<ICourseRepository>();
                courseRepo = courseRepoMock.Object;
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;

                academicCreditRepoMock = new Mock<IAcademicCreditRepository>();
                academicCreditRepo = academicCreditRepoMock.Object;
                academicCreditRepoMock.Setup(repo => repo.GetAcademicCreditByStudentIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns((IEnumerable<string> s, bool b1, bool b2, bool b3) => Task.FromResult(GetCredits(s)));
                academicCreditRepoMock.Setup(repo => (repo.GetAsync(It.IsAny<List<string>>(), false, true, It.IsAny<bool>()))).Returns((List<string> s, bool b1, bool b2, bool b3) => Task.FromResult(academicCreditRepoInstance.GetAsync(s, b1, b2, b3).Result));

                requirementRepoMock = new Mock<IRequirementRepository>();
                requirementRepo = requirementRepoMock.Object;
                ruleRepoMock = new Mock<IRuleRepository>();
                ruleRepo = ruleRepoMock.Object;
                progReqRepoMock = new Mock<IProgramRequirementsRepository>();
                progReqRepo = progReqRepoMock.Object;
                catalogRepoMock = new Mock<ICatalogRepository>();
                catalogRepo = catalogRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                logger = new Mock<ILogger>().Object;
                gradeRepoMock = new Mock<IGradeRepository>();
                gradeRepo = gradeRepoMock.Object;
                academicHistoryServiceMock = new Mock<IAcademicHistoryService>();
                academicHistoryService = academicHistoryServiceMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;

                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                studentConfigurationRepositoryMock = new Mock<IStudentConfigurationRepository>();
                studentConfigurationRepository = studentConfigurationRepositoryMock.Object;

                // Set up student 0000894 as the current user.
                currentUserFactory = new CurrentUserSetup.StudentUserFactory();

            }

            public void SetupCleanup()
            {
                studentDegreePlanRepo = null;
                adapterRegistry = null;
                studentRepo = null;
                studentProgramRepo = null;
                termRepo = null;
                courseRepo = null;
                sectionRepo = null;
                academicCreditRepo = null;
                requirementRepo = null;
                currentUserFactory = null;
                catalogRepo = null;
                gradeRepo = null;
                studentDegreePlanService = null;
                currentUserFactory = null;
                catalogRepo = null;
                gradeRepo = null;
            }
        }

        [TestClass]
        public class StudentDegreePlanService_GetDegreePlan : StudentDegreePlanServiceTestsSetup
        {
            [TestInitialize]
            public async void Initialize()
            {
                SetupInitialize();

                // Mock successful Get of a plan from repository
                IEnumerable<DegreePlan> degreePlans = await new TestStudentDegreePlanRepository().GetAsync();

                // Degree plan for student 0000894
                DegreePlan degreePlan = degreePlans.Where(dp => dp.Id == 2).First();
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(2)).Returns(Task.FromResult(degreePlan));

                // Degree plan for student 0000896
                DegreePlan degreePlan2 = degreePlans.Where(dp => dp.Id == 3).First();
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(3)).Returns(Task.FromResult(degreePlan2));

                // Mock get of a student
                studentRepoMock.Setup(x => x.GetAsync(It.IsAny<string>())).Returns(new TestStudentRepository().GetAsync("00004002"));

                // Mock get of all sections
                var regTerms = await new TestTermRepository().GetRegistrationTermsAsync();
                termRepoMock.Setup(x => x.GetRegistrationTermsAsync()).Returns(Task.FromResult(regTerms));
                var allSections = await new TestSectionRepository().GetRegistrationSectionsAsync(regTerms);
                sectionRepoMock.Setup(x => x.GetRegistrationSectionsAsync(It.IsAny<IEnumerable<Domain.Student.Entities.Term>>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(allSections));
                // Mock unsuccessful Get of a plan from repository
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(99)).Throws(new ArgumentException());

                // Mock grade repository Get
                var grades = await new TestGradeRepository().GetAsync();
                gradeRepoMock.Setup(x => x.GetAsync()).Returns(Task.FromResult(grades));

                // Mock academic credit repository get using student ids returning dict with student's academic credits
                var academicCreditDict = new Dictionary<string, List<Domain.Student.Entities.AcademicCredit>>();
                academicCreditRepoMock.Setup(x => x.GetAcademicCreditByStudentIdsAsync(It.IsAny<List<string>>(), false, true, It.IsAny<bool>())).Returns(Task.FromResult<Dictionary<string, List<Domain.Student.Entities.AcademicCredit>>>(academicCreditDict));

                // Mock get of all courses
                var courses = await new TestCourseRepository().GetAsync();
                courseRepoMock.Setup(x => x.GetAsync()).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Course>>(courses));

                // Mock get requirements for requisite validation 
                var requirementCodes = new List<string>() { };
                var requirements = (await new TestRequirementRepository().GetAsync(requirementCodes)).Where(r => r.RequirementType.Code == "PRE");
                requirementRepoMock.Setup(x => x.GetAsync(It.IsAny<IEnumerable<string>>())).Returns(Task.FromResult(requirements));

                // Mock Adapter 
                var datetimeOffsetAdapter = new DateTimeOffsetToDateTimeAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<DateTimeOffset, DateTime>()).Returns(datetimeOffsetAdapter);

                var degreePlanDtoAdapter = new AutoMapperAdapter<DegreePlan, Dtos.Student.DegreePlans.DegreePlan>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<DegreePlan, Dtos.Student.DegreePlans.DegreePlan>()).Returns(degreePlanDtoAdapter);

                var degreePlanApprovalDtoAdapter = new DegreePlanApprovalEntityAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<DegreePlanApproval, Dtos.Student.DegreePlans.DegreePlanApproval>()).Returns(degreePlanApprovalDtoAdapter);

                var degreePlanNoteDtoAdapter = new DegreePlanNoteEntityAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<DegreePlanNote, Dtos.Student.DegreePlans.DegreePlanNote>()).Returns(degreePlanNoteDtoAdapter);

                //var degreePlanArchiveDtoAdapter = new DegreePlanArchiveEntityAdapter(adapterRegistry, logger);
                //adapterRegistryMock.Setup(reg => reg.GetAdapter<DegreePlanArchive, Dtos.Planning.DegreePlanArchive>()).Returns(degreePlanArchiveDtoAdapter);

                var degreePlanWarningEntityAdapter = new AutoMapperAdapter<PlannedCourseWarning, Dtos.Student.DegreePlans.DegreePlanWarning>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<PlannedCourseWarning, Dtos.Student.DegreePlans.DegreePlanWarning>()).Returns(degreePlanWarningEntityAdapter);

                studentDegreePlanService = new StudentDegreePlanService(
                    adapterRegistry, studentDegreePlanRepo, termRepo, studentRepo, studentProgramRepo, courseRepo, sectionRepo, programRepo,
                    academicCreditRepo, requirementRepo, ruleRepo, progReqRepo, catalogRepo, gradeRepo, academicHistoryService,
                    currentUserFactory, roleRepo, logger, baseConfigurationRepository, studentConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                SetupCleanup();
            }

            [TestMethod]
            public async Task StudentDegreePlanService_GetDegreePlanAsync_Success()
            {
#pragma warning disable 618
                var dp = await studentDegreePlanService.GetDegreePlanAsync(2);
#pragma warning restore 618
                Assert.AreEqual("0000894", dp.PersonId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentDegreePlanService_GetDegreePlanAsync_NoDegreePlanException()
            {
#pragma warning disable 618
                var dp = await studentDegreePlanService.GetDegreePlanAsync(99);
#pragma warning restore 618
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task StudentDegreePlanService_GetDegreePlanAsync_ExceptionIfCurrentUserNotSelf()
            {
#pragma warning disable 618
                var dp = await studentDegreePlanService.GetDegreePlanAsync(3);
#pragma warning restore 618
            }

            [TestMethod]
            public async Task StudentDegreePlanService_GetDegreePlanAsync_AllowedForAdvisorWithViewPermissions()
            {
                // Set up advisor as the current user.
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();

                // Set up view permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAdviseeDegreePlan));
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock student repository get
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));


                // Attempt to get degree plan
#pragma warning disable 618
                await studentDegreePlanService.GetDegreePlanAsync(2);
#pragma warning restore 618
            }
        }

        [TestClass]
        public class StudentDegreePlanService_GetDegreePlan2 : StudentDegreePlanServiceTestsSetup
        {

            [TestInitialize]
            public async void Initialize()
            {
                SetupInitialize();
                // Mock successful Get of a plan from repository
                IEnumerable<DegreePlan> degreePlans = await new TestStudentDegreePlanRepository().GetAsync();

                // Degree plan for student 0000894
                DegreePlan degreePlan = degreePlans.Where(dp => dp.Id == 2).First();
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(2)).Returns(Task.FromResult(degreePlan));

                // Degree plan for student 0000896
                DegreePlan degreePlan2 = degreePlans.Where(dp => dp.Id == 3).First();
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(3)).Returns(Task.FromResult(degreePlan2));

                // Mock get of a student
                studentRepoMock.Setup(x => x.GetAsync(It.IsAny<string>())).Returns(new TestStudentRepository().GetAsync("00004002"));

                // Mock get of all sections
                var regTerms = await new TestTermRepository().GetRegistrationTermsAsync();
                termRepoMock.Setup(x => x.GetRegistrationTermsAsync()).Returns(Task.FromResult(regTerms));
                var allSections = await new TestSectionRepository().GetRegistrationSectionsAsync(regTerms);
                sectionRepoMock.Setup(x => x.GetRegistrationSectionsAsync(It.IsAny<IEnumerable<Domain.Student.Entities.Term>>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(allSections));
                // Mock unsuccessful Get of a plan from repository
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(99)).Throws(new ArgumentException());

                // Mock grade repo get
                var grades = await new TestGradeRepository().GetAsync();
                gradeRepoMock.Setup(x => x.GetAsync()).Returns(Task.FromResult(grades));

                // Mock DateTimeOffset->DateTime adapter to verify backward compatibility
                var datetimeOffsetAdapter = new DateTimeOffsetToDateTimeAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<DateTimeOffset, DateTime>()).Returns(datetimeOffsetAdapter);

                // Mock academic credit repository get using student ids returning dict with student's academic credits
                var academicCreditDict = new Dictionary<string, List<Domain.Student.Entities.AcademicCredit>>();
                academicCreditRepoMock.Setup(x => x.GetAcademicCreditByStudentIdsAsync(It.IsAny<List<string>>(), false, true, It.IsAny<bool>())).Returns(Task.FromResult<Dictionary<string, List<Domain.Student.Entities.AcademicCredit>>>(academicCreditDict));

                // Mock get of all courses
                var courses = await new TestCourseRepository().GetAsync();
                courseRepoMock.Setup(x => x.GetAsync()).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Course>>(courses));

                // Mock get requirements for requisite validation 
                var requirementCodes = new List<string>() { };
                var requirements = (await new TestRequirementRepository().GetAsync(requirementCodes)).Where(r => r.RequirementType.Code == "PRE");
                requirementRepoMock.Setup(x => x.GetAsync(It.IsAny<IEnumerable<string>>())).Returns(Task.FromResult(requirements));

                // Mock Adapter 
                var degreePlanEntityToDegreePlan2DtoAdapter = new AutoMapperAdapter<DegreePlan, Dtos.Student.DegreePlans.DegreePlan2>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<DegreePlan, Dtos.Student.DegreePlans.DegreePlan2>()).Returns(degreePlanEntityToDegreePlan2DtoAdapter);

                var plannedCourseWarningEntityAdapter = new AutoMapperAdapter<Dtos.Student.DegreePlans.PlannedCourseWarning, PlannedCourseWarning>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Dtos.Student.DegreePlans.PlannedCourseWarning, PlannedCourseWarning>()).Returns(plannedCourseWarningEntityAdapter);

                var degreePlanApprovalDtoAdapter = new AutoMapperAdapter<DegreePlanApproval, Dtos.Student.DegreePlans.DegreePlanApproval>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<DegreePlanApproval, Dtos.Student.DegreePlans.DegreePlanApproval>()).Returns(degreePlanApprovalDtoAdapter);

                studentDegreePlanService = new StudentDegreePlanService(
                    adapterRegistry, studentDegreePlanRepo, termRepo, studentRepo, studentProgramRepo, courseRepo, sectionRepo, programRepo,
                    academicCreditRepo, requirementRepo, ruleRepo, progReqRepo, catalogRepo, gradeRepo, academicHistoryService,
                    currentUserFactory, roleRepo, logger, baseConfigurationRepository, studentConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                SetupCleanup();
            }

            [TestMethod]
            public async Task StudentDegreePlanService_GetDegreePlan2Async_Success()
            {
#pragma warning disable 618
                var dp = await studentDegreePlanService.GetDegreePlan2Async(2);
#pragma warning restore 618
                Assert.AreEqual("0000894", dp.PersonId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentDegreePlanService_GetDegreePlan2Async_NoDegreePlan2Exception()
            {
#pragma warning disable 618
                var dp = await studentDegreePlanService.GetDegreePlan2Async(99);
#pragma warning restore 618
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task StudentDegreePlanService_GetDegreePlan2Async_ExceptionIfCurrentUserNotSelf()
            {
#pragma warning disable 618
                var dp = await studentDegreePlanService.GetDegreePlan2Async(3);
#pragma warning restore 618
            }

            [TestMethod]
            public async Task StudentDegreePlanService_GetDegreePlan2Async_AllowedForAdvisorWithViewPermissions()
            {
                // Set up advisor as the current user.
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();

                // Set up view permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAdviseeDegreePlan));
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock student repository get
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));

                // Attempt to get degree plan
#pragma warning disable 618
                await studentDegreePlanService.GetDegreePlan2Async(2);
#pragma warning restore 618
            }

        }

        [TestClass]
        public class StudentDegreePlanService_GetDegreePlan3 : StudentDegreePlanServiceTestsSetup
        {

            [TestInitialize]
            public async void Initialize()
            {
                SetupInitialize();

                // Mock successful Get of a plan from repository
                IEnumerable<DegreePlan> degreePlans = await new TestStudentDegreePlanRepository().GetAsync();

                // Degree plan for student 0000894
                DegreePlan degreePlan = degreePlans.Where(dp => dp.Id == 2).First();
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(2)).Returns(Task.FromResult(degreePlan));

                // Degree plan for student 0000896
                DegreePlan degreePlan2 = degreePlans.Where(dp => dp.Id == 3).First();
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(3)).Returns(Task.FromResult(degreePlan2));

                // Mock get of a student
                studentRepoMock.Setup(x => x.GetAsync(It.IsAny<string>())).Returns(new TestStudentRepository().GetAsync("00004002"));

                // Mock get of all sections
                var regTerms = await new TestTermRepository().GetRegistrationTermsAsync();
                termRepoMock.Setup(x => x.GetRegistrationTermsAsync()).Returns(Task.FromResult(regTerms));
                var allSections = await new TestSectionRepository().GetRegistrationSectionsAsync(regTerms);
                sectionRepoMock.Setup(x => x.GetRegistrationSectionsAsync(It.IsAny<IEnumerable<Domain.Student.Entities.Term>>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(allSections));
                // Mock unsuccessful Get of a plan from repository
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(99)).Throws(new ArgumentException());

                // Mock grade repo get
                var grades = await new TestGradeRepository().GetAsync();
                gradeRepoMock.Setup(x => x.GetAsync()).Returns(Task.FromResult(grades));

                // Mock get of all courses
                var courses = await new TestCourseRepository().GetAsync();
                courseRepoMock.Setup(x => x.GetAsync()).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Course>>(courses));

                // Mock academic credit repository get using student ids returning dict with student's academic credits
                var academicCreditDict = new Dictionary<string, List<Domain.Student.Entities.AcademicCredit>>();
                academicCreditRepoMock.Setup(x => x.GetAcademicCreditByStudentIdsAsync(It.IsAny<List<string>>(), false, true, It.IsAny<bool>())).Returns(Task.FromResult<Dictionary<string, List<Domain.Student.Entities.AcademicCredit>>>(academicCreditDict));

                // Mock get requirements for requisite validation 
                var requirementCodes = new List<string>() { };
                var requirements = (await new TestRequirementRepository().GetAsync(requirementCodes)).Where(r => r.RequirementType.Code == "PRE");
                requirementRepoMock.Setup(x => x.GetAsync(It.IsAny<IEnumerable<string>>())).Returns(Task.FromResult(requirements));

                // Mock Adapter 
                var degreePlanEntityToDegreePlan3DtoAdapter = new AutoMapperAdapter<DegreePlan, Dtos.Student.DegreePlans.DegreePlan3>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<DegreePlan, Dtos.Student.DegreePlans.DegreePlan3>()).Returns(degreePlanEntityToDegreePlan3DtoAdapter);

                var degreePlanApproval2DtoAdapter = new AutoMapperAdapter<DegreePlanApproval, Dtos.Student.DegreePlans.DegreePlanApproval2>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<DegreePlanApproval, Dtos.Student.DegreePlans.DegreePlanApproval2>()).Returns(degreePlanApproval2DtoAdapter);

                var plannedCourseWarningEntityAdapter = new AutoMapperAdapter<PlannedCourseWarning, Dtos.Student.DegreePlans.PlannedCourseWarning2>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<PlannedCourseWarning, Dtos.Student.DegreePlans.PlannedCourseWarning2>()).Returns(plannedCourseWarningEntityAdapter);

                studentDegreePlanService = new StudentDegreePlanService(
                    adapterRegistry, studentDegreePlanRepo, termRepo, studentRepo, studentProgramRepo, courseRepo, sectionRepo, programRepo,
                    academicCreditRepo, requirementRepo, ruleRepo, progReqRepo, catalogRepo, gradeRepo, academicHistoryService,
                    currentUserFactory, roleRepo, logger, baseConfigurationRepository, studentConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                SetupCleanup();
            }

            [TestMethod]
            public async Task StudentDegreePlanService_GetDegreePlan3Async_Success()
            {
                var dp = await studentDegreePlanService.GetDegreePlan3Async(2);
                Assert.AreEqual("0000894", dp.PersonId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentDegreePlanService_GetDegreePlan3Async_NoDegreePlan3Exception()
            {
                var dp = await studentDegreePlanService.GetDegreePlan3Async(99);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task StudentDegreePlanService_GetDegreePlan3Async_ExceptionIfCurrentUserNotSelf()
            {
                var dp = await studentDegreePlanService.GetDegreePlan3Async(3);
            }

            [TestMethod]
            public async Task StudentDegreePlanService_GetDegreePlan3Async_AllowedForAdvisorWithViewPermissions()
            {
                // Set up advisor as the current user.
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();

                // Set up view permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAdviseeDegreePlan));
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock student repository get
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));

                // Attempt to get degree plan
                await studentDegreePlanService.GetDegreePlan3Async(2);
            }
        }

        [TestClass]
        public class StudentDegreePlanService_GetDegreePlan4 : StudentDegreePlanServiceTestsSetup
        {
            private Dtos.Student.AcademicHistory2 history;
            private string studentId;

            [TestInitialize]
            public async void Initialize()
            {
                SetupInitialize();

                // Degree plan for student 0000894
                DegreePlan degreePlan = (await new TestStudentDegreePlanRepository().GetAsync()).Where(dp => dp.Id == 2).First();

                // Adding these planned courses will generate warnings for 2017/SP when verified degree plan is validated
                degreePlan.AddCourse(new PlannedCourse("333"), "2017/SP");
                degreePlan.AddCourse(new PlannedCourse("87"), "2017/SP");
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(2)).Returns(Task.FromResult(degreePlan));

                // Mock get of all courses
                var courses = await new TestCourseRepository().GetAsync();
                courseRepoMock.Setup(x => x.GetAsync()).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Course>>(courses));

                // Mock get requirements for requisite validation 
                var requirementCodes = new List<string>() { };
                var requirements = (await new TestRequirementRepository().GetAsync(requirementCodes)).Where(r => r.RequirementType.Code == "PRE");
                requirementRepoMock.Setup(x => x.GetAsync(It.IsAny<IEnumerable<string>>())).Returns(Task.FromResult(requirements));

                // Mock get of all terms
                var terms = await new TestTermRepository().GetAsync();
                termRepoMock.Setup(x => x.GetAsync()).ReturnsAsync(terms);

                // Mock get of all sections
                var regTerms = await new TestTermRepository().GetRegistrationTermsAsync();
                termRepoMock.Setup(x => x.GetRegistrationTermsAsync()).Returns(Task.FromResult(regTerms));
                var allSections = await new TestSectionRepository().GetRegistrationSectionsAsync(regTerms);
                sectionRepoMock.Setup(x => x.GetRegistrationSectionsAsync(It.IsAny<IEnumerable<Domain.Student.Entities.Term>>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(allSections));

                // Mock grade repo get
                var grades = await new TestGradeRepository().GetAsync();
                gradeRepoMock.Setup(x => x.GetAsync()).Returns(Task.FromResult(grades));

                // Mock the get of academic credits
                studentId = "0000894";
                var studentAcademicCredits = await new TestAcademicCreditRepository().GetAsync();
                Domain.Student.Entities.GradeRestriction gradeRestriction = new Domain.Student.Entities.GradeRestriction(false);
                var historyEntity = new Domain.Student.Entities.AcademicHistory(studentAcademicCredits, gradeRestriction, null);
                historyEntity.StudentId = studentId;

                // Mock academic credit repository get using student id returning dict with student's academic credits
                var academicCreditDict = new Dictionary<string, List<Domain.Student.Entities.AcademicCredit>>();
                academicCreditDict[studentId] = studentAcademicCredits.ToList();
                academicCreditRepoMock.Setup(x => x.GetAcademicCreditByStudentIdsAsync(It.IsAny<List<string>>(), false, true, It.IsAny<bool>())).Returns(Task.FromResult<Dictionary<string, List<Domain.Student.Entities.AcademicCredit>>>(academicCreditDict));
                history = new AcademicHistoryEntityToAcademicHistory2DtoAdapter(adapterRegistry, logger).MapToType(historyEntity);
                academicHistoryServiceMock.Setup(svc => svc.ConvertAcademicCreditsToAcademicHistoryDtoAsync(history.StudentId, It.IsAny<IEnumerable<Domain.Student.Entities.AcademicCredit>>(), It.IsAny<Domain.Student.Entities.Student>()))
                    .Returns(Task.FromResult(history));

                // Mock Adapter 
                var degreePlanEntityToDegreePlan4DtoAdapter = new DegreePlanEntity4Adapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<DegreePlan, Dtos.Student.DegreePlans.DegreePlan4>()).Returns(degreePlanEntityToDegreePlan4DtoAdapter);

                var degreePlanApproval2DtoAdapter = new AutoMapperAdapter<DegreePlanApproval, Dtos.Student.DegreePlans.DegreePlanApproval2>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<DegreePlanApproval, Dtos.Student.DegreePlans.DegreePlanApproval2>()).Returns(degreePlanApproval2DtoAdapter);

                var plannedCourseWarningEntityAdapter = new PlannedCourseWarningEntity2Adaptor(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<PlannedCourseWarning, Dtos.Student.DegreePlans.PlannedCourseWarning2>()).Returns(plannedCourseWarningEntityAdapter);

                studentDegreePlanService = new StudentDegreePlanService(
                    adapterRegistry, studentDegreePlanRepo, termRepo, studentRepo, studentProgramRepo, courseRepo, sectionRepo, programRepo,
                    academicCreditRepo, requirementRepo, ruleRepo, progReqRepo, catalogRepo, gradeRepo, academicHistoryService,
                    currentUserFactory, roleRepo, logger, baseConfigurationRepository, studentConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                SetupCleanup();
            }

            [TestMethod]
            public async Task StudentDegreePlanService_GetDegreePlan4Async_Success()
            {
                var result = await studentDegreePlanService.GetDegreePlan4Async(2);
                Assert.AreEqual("0000894", result.DegreePlan.PersonId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentDegreePlanService_GetDegreePlan4Async_NoDegreePlan3Exception()
            {
                // Mock unsuccessful Get of a plan from repository
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(99)).Throws(new ArgumentException());

                var result = await studentDegreePlanService.GetDegreePlan4Async(99);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task StudentDegreePlanService_GetDegreePlan4Async_ExceptionIfCurrentUserNotSelf()
            {
                // Degree plan for student 0000896
                DegreePlan degreePlan2 = (await new TestStudentDegreePlanRepository().GetAsync()).Where(dp => dp.Id == 3).First();
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(3)).Returns(Task.FromResult(degreePlan2));

                // Mock get of a student
                studentRepoMock.Setup(x => x.GetAsync(It.IsAny<string>())).Returns(new TestStudentRepository().GetAsync("00004002"));

                var result = await studentDegreePlanService.GetDegreePlan4Async(3);
            }

            [TestMethod]
            public async Task StudentDegreePlanService_GetDegreePlan4Async_AllowedForAdvisorWithViewPermissions()
            {
                // Set up advisor as the current user.
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();

                // Set up view permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAdviseeDegreePlan));
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock student repository get
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));

                // Attempt to get degree plan
                await studentDegreePlanService.GetDegreePlan4Async(2);
            }

            [TestMethod]
            public async Task StudentDegreePlanService_GetDegreePlan4Async_ContainsAcademicHistory()
            {
                var result = await studentDegreePlanService.GetDegreePlan4Async(2);
                Assert.AreEqual(studentId, result.DegreePlan.PersonId);
                Assert.AreEqual(studentId, result.AcademicHistory.StudentId);
                Assert.IsTrue(history.AcademicTerms.Count() > 0);
                Assert.AreEqual(history.AcademicTerms.Count(), result.AcademicHistory.AcademicTerms.Count());
            }

            [TestMethod]
            public async Task StudentDegreePlanService_GetDegreePlan4Async_Validate()
            {
                // Act- Call in "validate" mode (the default)
                var result = await studentDegreePlanService.GetDegreePlan4Async(2, true);

                // Assert - planned courses have warnings
                var plannedCoursesWithWarnings = result.DegreePlan.Terms.Where(trm => trm.TermId == "2017/SP").SelectMany(t => t.PlannedCourses).Where(pc => pc.Warnings.Count() > 0);
                Assert.AreEqual(2, plannedCoursesWithWarnings.Count());
            }

            [TestMethod]
            public async Task StudentDegreePlanService_GetDegreePlan4Async_NoValidate()
            {
                // Act - Exact same test as above but call WITHOUT "validate" mode
                var result = await studentDegreePlanService.GetDegreePlan4Async(2, false);

                // Assert - planned courses have NO warnings
                var plannedCoursesWithWarnings = result.DegreePlan.Terms.Where(trm => trm.TermId == "2017/SP").SelectMany(t => t.PlannedCourses).Where(pc => pc.Warnings.Count() > 0);
                Assert.IsTrue(plannedCoursesWithWarnings.Count() == 0);
            }
        }

        [TestClass]
        public class StudentDegreePlanService_GetDegreePlan5Async : StudentDegreePlanServiceTestsSetup
        {
            private Dtos.Student.AcademicHistory3 history;
            private string studentId;

            [TestInitialize]
            public async void Initialize()
            {
                SetupInitialize();

                // Degree plan for student 0000894
                DegreePlan degreePlan = (await new TestStudentDegreePlanRepository().GetAsync()).Where(dp => dp.Id == 2).First();

                // Adding these planned courses will generate warnings for 2017/SP when verified degree plan is validated
                degreePlan.AddCourse(new PlannedCourse("333"), "2017/SP");
                degreePlan.AddCourse(new PlannedCourse("87"), "2017/SP");
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(2)).Returns(Task.FromResult(degreePlan));

                // Mock get of all courses
                var courses = await new TestCourseRepository().GetAsync();
                courseRepoMock.Setup(x => x.GetAsync()).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Course>>(courses));

                // Mock get requirements for requisite validation 
                var requirementCodes = new List<string>() { };
                var requirements = (await new TestRequirementRepository().GetAsync(requirementCodes)).Where(r => r.RequirementType.Code == "PRE");
                requirementRepoMock.Setup(x => x.GetAsync(It.IsAny<IEnumerable<string>>())).Returns(Task.FromResult(requirements));

                // Mock get of all terms
                var terms = await new TestTermRepository().GetAsync();
                termRepoMock.Setup(x => x.GetAsync()).ReturnsAsync(terms);

                // Mock get of all sections
                var regTerms = await new TestTermRepository().GetRegistrationTermsAsync();
                termRepoMock.Setup(x => x.GetRegistrationTermsAsync()).Returns(Task.FromResult(regTerms));
                var allSections = await new TestSectionRepository().GetRegistrationSectionsAsync(regTerms);
                sectionRepoMock.Setup(x => x.GetRegistrationSectionsAsync(It.IsAny<IEnumerable<Domain.Student.Entities.Term>>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(allSections));

                // Mock grade repo get
                var grades = await new TestGradeRepository().GetAsync();
                gradeRepoMock.Setup(x => x.GetAsync()).Returns(Task.FromResult(grades));

                // Mock the get of academic credits
                studentId = "0000894";
                var studentAcademicCredits = await new TestAcademicCreditRepository().GetAsync();
                Domain.Student.Entities.GradeRestriction gradeRestriction = new Domain.Student.Entities.GradeRestriction(false);
                var historyEntity = new Domain.Student.Entities.AcademicHistory(studentAcademicCredits, gradeRestriction, null);
                historyEntity.StudentId = studentId;

                // Mock academic credit repository get using student id returning dict with student's academic credits
                var academicCreditDict = new Dictionary<string, List<Domain.Student.Entities.AcademicCredit>>();
                academicCreditDict[studentId] = studentAcademicCredits.ToList();
                academicCreditRepoMock.Setup(x => x.GetAcademicCreditByStudentIdsAsync(It.IsAny<List<string>>(), false, true, It.IsAny<bool>())).Returns(Task.FromResult<Dictionary<string, List<Domain.Student.Entities.AcademicCredit>>>(academicCreditDict));
                history = new AcademicHistoryEntityToAcademicHistory3DtoAdapter(adapterRegistry, logger).MapToType(historyEntity);
                academicHistoryServiceMock.Setup(svc => svc.ConvertAcademicCreditsToAcademicHistoryDto2Async(history.StudentId, It.IsAny<IEnumerable<Domain.Student.Entities.AcademicCredit>>(), It.IsAny<Domain.Student.Entities.Student>()))
                    .Returns(Task.FromResult(history));

                // Mock Adapter 
                var degreePlanEntityToDegreePlan4DtoAdapter = new DegreePlanEntity4Adapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<DegreePlan, Dtos.Student.DegreePlans.DegreePlan4>()).Returns(degreePlanEntityToDegreePlan4DtoAdapter);

                var degreePlanApproval2DtoAdapter = new AutoMapperAdapter<DegreePlanApproval, Dtos.Student.DegreePlans.DegreePlanApproval2>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<DegreePlanApproval, Dtos.Student.DegreePlans.DegreePlanApproval2>()).Returns(degreePlanApproval2DtoAdapter);

                var plannedCourseWarningEntityAdapter = new PlannedCourseWarningEntity2Adaptor(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<PlannedCourseWarning, Dtos.Student.DegreePlans.PlannedCourseWarning2>()).Returns(plannedCourseWarningEntityAdapter);

                studentDegreePlanService = new StudentDegreePlanService(
                    adapterRegistry, studentDegreePlanRepo, termRepo, studentRepo, studentProgramRepo, courseRepo, sectionRepo, programRepo,
                    academicCreditRepo, requirementRepo, ruleRepo, progReqRepo, catalogRepo, gradeRepo, academicHistoryService,
                    currentUserFactory, roleRepo, logger, baseConfigurationRepository, studentConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                SetupCleanup();
            }

            [TestMethod]
            public async Task StudentDegreePlanService_GetDegreePlan5Async_Success()
            {
                var result = await studentDegreePlanService.GetDegreePlan5Async(2);
                Assert.AreEqual("0000894", result.DegreePlan.PersonId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentDegreePlanService_GetDegreePlan5Async_NoDegreePlan3Exception()
            {
                // Mock unsuccessful Get of a plan from repository
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(99)).Throws(new ArgumentException());

                var result = await studentDegreePlanService.GetDegreePlan5Async(99);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task StudentDegreePlanService_GetDegreePlan5Async_ExceptionIfCurrentUserNotSelf()
            {
                // Degree plan for student 0000896
                DegreePlan degreePlan2 = (await new TestStudentDegreePlanRepository().GetAsync()).Where(dp => dp.Id == 3).First();
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(3)).Returns(Task.FromResult(degreePlan2));

                // Mock get of a student
                studentRepoMock.Setup(x => x.GetAsync(It.IsAny<string>())).Returns(new TestStudentRepository().GetAsync("00004002"));

                var result = await studentDegreePlanService.GetDegreePlan5Async(3);
            }

            [TestMethod]
            public async Task StudentDegreePlanService_GetDegreePlan5Async_AllowedForAdvisorWithViewPermissions()
            {
                // Set up advisor as the current user.
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();

                // Set up view permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAdviseeDegreePlan));
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock student repository get
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));

                // Attempt to get degree plan
                await studentDegreePlanService.GetDegreePlan5Async(2);
            }

            [TestMethod]
            public async Task StudentDegreePlanService_GetDegreePlan5Async_ContainsAcademicHistory()
            {
                var result = await studentDegreePlanService.GetDegreePlan5Async(2);
                Assert.AreEqual(studentId, result.DegreePlan.PersonId);
                Assert.AreEqual(studentId, result.AcademicHistory.StudentId);
                Assert.IsTrue(history.AcademicTerms.Count() > 0);
                Assert.AreEqual(history.AcademicTerms.Count(), result.AcademicHistory.AcademicTerms.Count());
            }

            [TestMethod]
            public async Task StudentDegreePlanService_GetDegreePlan5Async_Validate()
            {
                // Act- Call in "validate" mode (the default)
                var result = await studentDegreePlanService.GetDegreePlan5Async(2, true);

                // Assert - planned courses have warnings
                var plannedCoursesWithWarnings = result.DegreePlan.Terms.Where(trm => trm.TermId == "2017/SP").SelectMany(t => t.PlannedCourses).Where(pc => pc.Warnings.Count() > 0);
                Assert.AreEqual(2, plannedCoursesWithWarnings.Count());
            }

            [TestMethod]
            public async Task StudentDegreePlanService_GetDegreePlan5Async_NoValidate()
            {
                // Act - Exact same test as above but call WITHOUT "validate" mode
                var result = await studentDegreePlanService.GetDegreePlan5Async(2, false);

                // Assert - planned courses have NO warnings
                var plannedCoursesWithWarnings = result.DegreePlan.Terms.Where(trm => trm.TermId == "2017/SP").SelectMany(t => t.PlannedCourses).Where(pc => pc.Warnings.Count() > 0);
                Assert.IsTrue(plannedCoursesWithWarnings.Count() == 0);
            }
        }

        [TestClass]
        public class StudentDegreePlanService_GetDegreePlan6Async : StudentDegreePlanServiceTestsSetup
        {
            private Dtos.Student.AcademicHistory4 history;
            private string studentId;

            [TestInitialize]
            public async void Initialize()
            {
                SetupInitialize();

                // Degree plan for student 0000894
                DegreePlan degreePlan = (await new TestStudentDegreePlanRepository().GetAsync()).Where(dp => dp.Id == 2).First();

                // Adding these planned courses will generate warnings for 2017/SP when verified degree plan is validated
                degreePlan.AddCourse(new PlannedCourse("333"), "2017/SP");
                degreePlan.AddCourse(new PlannedCourse("87"), "2017/SP");
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(2)).Returns(Task.FromResult(degreePlan));

                // Mock get of all courses
                var courses = await new TestCourseRepository().GetAsync();
                courseRepoMock.Setup(x => x.GetAsync()).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Course>>(courses));

                // Mock get requirements for requisite validation 
                var requirementCodes = new List<string>() { };
                var requirements = (await new TestRequirementRepository().GetAsync(requirementCodes)).Where(r => r.RequirementType.Code == "PRE");
                requirementRepoMock.Setup(x => x.GetAsync(It.IsAny<IEnumerable<string>>())).Returns(Task.FromResult(requirements));

                // Mock get of all terms
                var terms = await new TestTermRepository().GetAsync();
                termRepoMock.Setup(x => x.GetAsync()).ReturnsAsync(terms);

                // Mock get of all sections
                var regTerms = await new TestTermRepository().GetRegistrationTermsAsync();
                termRepoMock.Setup(x => x.GetRegistrationTermsAsync()).Returns(Task.FromResult(regTerms));
                var allSections = await new TestSectionRepository().GetRegistrationSectionsAsync(regTerms);
                sectionRepoMock.Setup(x => x.GetRegistrationSectionsAsync(It.IsAny<IEnumerable<Domain.Student.Entities.Term>>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(allSections));

                // Mock grade repo get
                var grades = await new TestGradeRepository().GetAsync();
                gradeRepoMock.Setup(x => x.GetAsync()).Returns(Task.FromResult(grades));

                // Mock the get of academic credits
                studentId = "0000894";
                var studentAcademicCredits = await new TestAcademicCreditRepository().GetAsync();
                Domain.Student.Entities.GradeRestriction gradeRestriction = new Domain.Student.Entities.GradeRestriction(false);
                var historyEntity = new Domain.Student.Entities.AcademicHistory(studentAcademicCredits, gradeRestriction, null);
                historyEntity.StudentId = studentId;

                // Mock academic credit repository get using student id returning dict with student's academic credits
                var academicCreditDict = new Dictionary<string, List<Domain.Student.Entities.AcademicCredit>>();
                academicCreditDict[studentId] = studentAcademicCredits.ToList();
                academicCreditRepoMock.Setup(x => x.GetAcademicCreditByStudentIdsAsync(It.IsAny<List<string>>(), false, true, It.IsAny<bool>())).Returns(Task.FromResult<Dictionary<string, List<Domain.Student.Entities.AcademicCredit>>>(academicCreditDict));
                history = new AcademicHistoryEntityToAcademicHistory4DtoAdapter(adapterRegistry, logger).MapToType(historyEntity);
                academicHistoryServiceMock.Setup(svc => svc.ConvertAcademicCreditsToAcademicHistoryDto4Async(history.StudentId, It.IsAny<IEnumerable<Domain.Student.Entities.AcademicCredit>>(), It.IsAny<Domain.Student.Entities.Student>()))
                    .Returns(Task.FromResult(history));

                // Mock Adapter 
                var degreePlanEntityToDegreePlan4DtoAdapter = new DegreePlanEntity4Adapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<DegreePlan, Dtos.Student.DegreePlans.DegreePlan4>()).Returns(degreePlanEntityToDegreePlan4DtoAdapter);

                var degreePlanApproval2DtoAdapter = new AutoMapperAdapter<DegreePlanApproval, Dtos.Student.DegreePlans.DegreePlanApproval2>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<DegreePlanApproval, Dtos.Student.DegreePlans.DegreePlanApproval2>()).Returns(degreePlanApproval2DtoAdapter);

                var plannedCourseWarningEntityAdapter = new PlannedCourseWarningEntity2Adaptor(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<PlannedCourseWarning, Dtos.Student.DegreePlans.PlannedCourseWarning2>()).Returns(plannedCourseWarningEntityAdapter);

                studentDegreePlanService = new StudentDegreePlanService(
                    adapterRegistry, studentDegreePlanRepo, termRepo, studentRepo, studentProgramRepo, courseRepo, sectionRepo, programRepo,
                    academicCreditRepo, requirementRepo, ruleRepo, progReqRepo, catalogRepo, gradeRepo, academicHistoryService,
                    currentUserFactory, roleRepo, logger, baseConfigurationRepository, studentConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                SetupCleanup();
            }

            [TestMethod]
            public async Task StudentDegreePlanService_GetDegreePlan6Async_Success()
            {
                var result = await studentDegreePlanService.GetDegreePlan6Async(2);
                Assert.AreEqual("0000894", result.DegreePlan.PersonId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentDegreePlanService_GetDegreePlan6Async_NoDegreePlan3Exception()
            {
                // Mock unsuccessful Get of a plan from repository
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(99)).Throws(new ArgumentException());

                var result = await studentDegreePlanService.GetDegreePlan6Async(99);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task StudentDegreePlanService_GetDegreePlan6Async_ExceptionIfCurrentUserNotSelf()
            {
                // Degree plan for student 0000896
                DegreePlan degreePlan2 = (await new TestStudentDegreePlanRepository().GetAsync()).Where(dp => dp.Id == 3).First();
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(3)).Returns(Task.FromResult(degreePlan2));

                // Mock get of a student
                studentRepoMock.Setup(x => x.GetAsync(It.IsAny<string>())).Returns(new TestStudentRepository().GetAsync("00004002"));

                var result = await studentDegreePlanService.GetDegreePlan6Async(3);
            }

            [TestMethod]
            public async Task StudentDegreePlanService_GetDegreePlan6Async_AllowedForAdvisorWithViewPermissions()
            {
                // Set up advisor as the current user.
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();

                // Set up view permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAdviseeDegreePlan));
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock student repository get
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));

                // Attempt to get degree plan
                await studentDegreePlanService.GetDegreePlan6Async(2);
            }

            [TestMethod]
            public async Task StudentDegreePlanService_GetDegreePlan6Async_ContainsAcademicHistory()
            {
                var result = await studentDegreePlanService.GetDegreePlan6Async(2);
                Assert.AreEqual(studentId, result.DegreePlan.PersonId);
                Assert.AreEqual(studentId, result.AcademicHistory.StudentId);
                Assert.IsTrue(history.AcademicTerms.Count() > 0);
                Assert.AreEqual(history.AcademicTerms.Count(), result.AcademicHistory.AcademicTerms.Count());
            }

            [TestMethod]
            public async Task StudentDegreePlanService_GetDegreePlan6Async_Validate()
            {
                // Act- Call in "validate" mode (the default)
                var result = await studentDegreePlanService.GetDegreePlan6Async(2, true);

                // Assert - planned courses have warnings
                var plannedCoursesWithWarnings = result.DegreePlan.Terms.Where(trm => trm.TermId == "2017/SP").SelectMany(t => t.PlannedCourses).Where(pc => pc.Warnings.Count() > 0);
                Assert.AreEqual(2, plannedCoursesWithWarnings.Count());
            }

            [TestMethod]
            public async Task StudentDegreePlanService_GetDegreePlan6Async_NoValidate()
            {
                // Act - Exact same test as above but call WITHOUT "validate" mode
                var result = await studentDegreePlanService.GetDegreePlan6Async(2, false);

                // Assert - planned courses have NO warnings
                var plannedCoursesWithWarnings = result.DegreePlan.Terms.Where(trm => trm.TermId == "2017/SP").SelectMany(t => t.PlannedCourses).Where(pc => pc.Warnings.Count() > 0);
                Assert.IsTrue(plannedCoursesWithWarnings.Count() == 0);
            }
        }

        [TestClass]
        public class StudentDegreePlanService_CreateDegreePlan : StudentDegreePlanServiceTestsSetup
        {

            [TestInitialize]
            public async void Initialize()
            {
                SetupInitialize();

                // Mock the get from the student repository
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));
                var student2 = new Domain.Student.Entities.Student("0000896", "Jones", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000896")).Returns(Task.FromResult(student2));

                // Mock the get of the planning terms
                var allTerms = await new TestTermRepository().GetAsync();
                termRepoMock.Setup(x => x.GetAsync()).ReturnsAsync(allTerms);

                // Mock the get of the student programs
                var studentPrograms = await new TestStudentProgramRepository().GetAsync("0000896");
                studentProgramRepoMock.Setup(x => x.GetAsync("0000896")).ReturnsAsync(studentPrograms);
                studentProgramRepoMock.Setup(x => x.GetAsync("0000894")).ReturnsAsync(studentPrograms);

                // Mock an empty program 
                IEnumerable<string> depts = new List<string>() { "Dept1" };
                IEnumerable<Domain.Student.Entities.Requirements.Program> program =
                    new List<Domain.Student.Entities.Requirements.Program>() { new Domain.Student.Entities.Requirements.Program("EmptyRequirements", "Empty Requirements", depts, true, "UG", new CreditFilter(), false) };
                programRepoMock.Setup(pr => pr.GetAsync()).Returns(Task.FromResult(program));

                // Mock successful add of a plan from repository
                var degreePlan = new DegreePlan("0000894");
                degreePlan.Id = 0;
                degreePlan.AddTerm("2012/FA");
                degreePlan.AddTerm("2012/SP");
                degreePlan.AddTerm("2013/SP");
                studentDegreePlanRepoMock.Setup(x => x.AddAsync(It.IsAny<DegreePlan>())).Returns(Task.FromResult(degreePlan));

                // Mock grade repo 
                var grades = await new TestGradeRepository().GetAsync();
                gradeRepoMock.Setup(x => x.GetAsync()).Returns(Task.FromResult(grades));

                // Mock DateTimeOffset->DateTime adapter to verify backward compatibility
                var datetimeOffsetAdapter = new DateTimeOffsetToDateTimeAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<System.DateTimeOffset, System.DateTime>()).Returns(datetimeOffsetAdapter);

                // Mock Adapter 
                var degreePlanDtoAdapter = new AutoMapperAdapter<DegreePlan, Dtos.Student.DegreePlans.DegreePlan>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<DegreePlan, Dtos.Student.DegreePlans.DegreePlan>()).Returns(degreePlanDtoAdapter);

                studentDegreePlanService = new StudentDegreePlanService(
                    adapterRegistry, studentDegreePlanRepo, termRepo, studentRepo, studentProgramRepo, courseRepo, sectionRepo, programRepo,
                    academicCreditRepo, requirementRepo, ruleRepo, progReqRepo, catalogRepo, gradeRepo, academicHistoryService,
                    currentUserFactory, roleRepo, logger, baseConfigurationRepository, studentConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                SetupCleanup();
            }

            [TestMethod]
            public async Task StudentDegreePlanService_CreateDegreePlanAsync_Success()
            {
                var dp = await studentDegreePlanService.CreateDegreePlanAsync("0000894");
                Assert.AreEqual("0000894", dp.PersonId);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task StudentDegreePlanService_CreateDegreePlanAsync_ExceptionIfCurrentUserNotSelf()
            {
                var dp = await studentDegreePlanService.CreateDegreePlanAsync("0000896");
            }
        }

        [TestClass]
        public class StudentDegreePlanService_CreateDegreePlan2 : StudentDegreePlanServiceTestsSetup
        {

            [TestInitialize]
            public async void Initialize()
            {
                SetupInitialize();

                // Mock the get from the student repository
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));
                var student2 = new Domain.Student.Entities.Student("0000896", "Jones", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000896")).Returns(Task.FromResult(student2));


                // Mock the get of the planning terms
                var allTerms = await new TestTermRepository().GetAsync();
                termRepoMock.Setup(x => x.GetAsync()).ReturnsAsync(allTerms);

                // Mock the get of the student programs
                var studentPrograms = await new TestStudentProgramRepository().GetAsync("0000896");
                studentProgramRepoMock.Setup(x => x.GetAsync("0000896")).ReturnsAsync(studentPrograms);
                studentProgramRepoMock.Setup(x => x.GetAsync("0000894")).ReturnsAsync(studentPrograms);

                // Mock an empty program 
                IEnumerable<string> depts = new List<string>() { "Dept1" };
                IEnumerable<Domain.Student.Entities.Requirements.Program> program =
                    new List<Domain.Student.Entities.Requirements.Program>() { new Domain.Student.Entities.Requirements.Program("EmptyRequirements", "Empty Requirements", depts, true, "UG", new CreditFilter(), false) };
                programRepoMock.Setup(pr => pr.GetAsync()).Returns(Task.FromResult(program));

                // Mock successful add of a plan from repository
                var degreePlan = new DegreePlan("0000894");
                degreePlan.Id = 0;
                degreePlan.AddTerm("2012/FA");
                degreePlan.AddTerm("2012/SP");
                degreePlan.AddTerm("2013/SP");
                studentDegreePlanRepoMock.Setup(x => x.AddAsync(It.IsAny<DegreePlan>())).Returns(Task.FromResult(degreePlan));

                // Mock grade repository get
                var grades = await new TestGradeRepository().GetAsync();
                gradeRepoMock.Setup(x => x.GetAsync()).Returns(Task.FromResult(grades));

                // Mock DateTimeOffset->DateTime adapter to verify backward compatibility
                var datetimeOffsetAdapter = new DateTimeOffsetToDateTimeAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<System.DateTimeOffset, System.DateTime>()).Returns(datetimeOffsetAdapter);

                // Mock Adapter 
                var degreePlanEntityToDegreePlan2DtoAdapter = new AutoMapperAdapter<DegreePlan, Dtos.Student.DegreePlans.DegreePlan2>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<DegreePlan, Dtos.Student.DegreePlans.DegreePlan2>()).Returns(degreePlanEntityToDegreePlan2DtoAdapter);

                studentDegreePlanService = new StudentDegreePlanService(
                    adapterRegistry, studentDegreePlanRepo, termRepo, studentRepo, studentProgramRepo, courseRepo, sectionRepo, programRepo,
                    academicCreditRepo, requirementRepo, ruleRepo, progReqRepo, catalogRepo, gradeRepo, academicHistoryService,
                    currentUserFactory, roleRepo, logger, baseConfigurationRepository, studentConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                SetupCleanup();
            }

            [TestMethod]
            public async Task StudentDegreePlanService_CreateDegreePlan2Async_Success()
            {
#pragma warning disable 618
                var dp = await studentDegreePlanService.CreateDegreePlan2Async("0000894");
#pragma warning restore 618
                Assert.AreEqual("0000894", dp.PersonId);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task StudentDegreePlanService_CreateDegreePlan2Async_ExceptionIfCurrentUserNotSelf()
            {
#pragma warning disable 618
                var dp = await studentDegreePlanService.CreateDegreePlan2Async("0000896");
#pragma warning restore 618
            }
        }

        [TestClass]
        public class StudentDegreePlanService_CreateDegreePlan3 : StudentDegreePlanServiceTestsSetup
        {
            private Domain.Student.Entities.AcademicHistory historyEntity;
            private Dtos.Student.AcademicHistory2 historyDto;

            [TestInitialize]
            public async void Initialize()
            {
                SetupInitialize();

                // Mock the get from the student repository
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));
                var student2 = new Domain.Student.Entities.Student("0000896", "Jones", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000896")).Returns(Task.FromResult(student2));

                // Mock the get of the planning terms
                var allTerms = await new TestTermRepository().GetAsync();
                termRepoMock.Setup(x => x.GetAsync()).ReturnsAsync(allTerms);

                // Mock the get of the student programs
                var studentPrograms = await new TestStudentProgramRepository().GetAsync("0000896");
                studentProgramRepoMock.Setup(x => x.GetAsync("0000896")).ReturnsAsync(studentPrograms);
                studentProgramRepoMock.Setup(x => x.GetAsync("0000894")).ReturnsAsync(studentPrograms);

                // Mock an empty program 
                IEnumerable<string> depts = new List<string>() { "Dept1" };
                IEnumerable<Domain.Student.Entities.Requirements.Program> program =
                    new List<Domain.Student.Entities.Requirements.Program>() { new Domain.Student.Entities.Requirements.Program("EmptyRequirements", "Empty Requirements", depts, true, "UG", new CreditFilter(), false) };
                programRepoMock.Setup(pr => pr.GetAsync()).Returns(Task.FromResult(program));

                // Mock successful add of a plan from repository
                var degreePlan = new DegreePlan("0000894");
                degreePlan.Id = 0;
                degreePlan.AddTerm("2012/FA");
                degreePlan.AddTerm("2012/SP");
                degreePlan.AddTerm("2013/SP");
                studentDegreePlanRepoMock.Setup(x => x.AddAsync(It.IsAny<DegreePlan>())).Returns(Task.FromResult(degreePlan));

                // Mock grade repository get
                var grades = await new TestGradeRepository().GetAsync();
                gradeRepoMock.Setup(x => x.GetAsync()).Returns(Task.FromResult(grades));

                // Mock Adapter 
                var degreePlanEntityToDegreePlan3DtoAdapter = new AutoMapperAdapter<DegreePlan, Dtos.Student.DegreePlans.DegreePlan3>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<DegreePlan, Dtos.Student.DegreePlans.DegreePlan3>()).Returns(degreePlanEntityToDegreePlan3DtoAdapter);

                var studentAcademicCredits = await new TestAcademicCreditRepository().GetAsync();
                Domain.Student.Entities.GradeRestriction gradeRestriction = new Domain.Student.Entities.GradeRestriction(false);
                historyEntity = new Domain.Student.Entities.AcademicHistory(studentAcademicCredits, gradeRestriction, null);
                historyEntity.StudentId = "0000894";
                historyDto = new AcademicHistoryEntityToAcademicHistory2DtoAdapter(adapterRegistry, logger).MapToType(historyEntity);
                academicHistoryServiceMock.Setup(svc => svc.ConvertAcademicCreditsToAcademicHistoryDtoAsync(historyEntity.StudentId, It.IsAny<IEnumerable<Domain.Student.Entities.AcademicCredit>>(), It.IsAny<Domain.Student.Entities.Student>()))
                    .Returns(Task.FromResult(historyDto));

                studentDegreePlanService = new StudentDegreePlanService(
                    adapterRegistry, studentDegreePlanRepo, termRepo, studentRepo, studentProgramRepo, courseRepo, sectionRepo, programRepo,
                    academicCreditRepo, requirementRepo, ruleRepo, progReqRepo, catalogRepo, gradeRepo, academicHistoryService,
                    currentUserFactory, roleRepo, logger, baseConfigurationRepository, studentConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                SetupCleanup();
            }

            [TestMethod]
            public async Task StudentDegreePlanService_CreateDegreePlan3Async_Success()
            {
                var dp = await studentDegreePlanService.CreateDegreePlan3Async("0000894");
                Assert.AreEqual("0000894", dp.PersonId);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task StudentDegreePlanService_CreateDegreePlan3Async_ExceptionIfCurrentUserNotSelf()
            {
                var dp = await studentDegreePlanService.CreateDegreePlan3Async("0000896");
            }
        }

        [TestClass]
        public class StudentDegreePlanService_CreateDegreePlan4 : StudentDegreePlanServiceTestsSetup
        {
            private Domain.Student.Entities.AcademicHistory historyEntity;
            private Dtos.Student.AcademicHistory2 historyDto;


            [TestInitialize]
            public async void Initialize()
            {
                SetupInitialize();

                // Mock the get from the student repository
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));
                var student2 = new Domain.Student.Entities.Student("0000896", "Jones", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000896")).Returns(Task.FromResult(student2));

                // Mock the get of the planning terms
                var allTerms = await new TestTermRepository().GetAsync();
                termRepoMock.Setup(x => x.GetAsync()).ReturnsAsync(allTerms);

                // Mock the get of the student programs
                var studentPrograms = await new TestStudentProgramRepository().GetAsync("0000896");
                studentProgramRepoMock.Setup(x => x.GetAsync("0000896")).ReturnsAsync(studentPrograms);
                studentProgramRepoMock.Setup(x => x.GetAsync("0000894")).ReturnsAsync(studentPrograms);

                // Mock an empty program 
                IEnumerable<string> depts = new List<string>() { "Dept1" };
                IEnumerable<Domain.Student.Entities.Requirements.Program> program =
                    new List<Domain.Student.Entities.Requirements.Program>() { new Domain.Student.Entities.Requirements.Program("EmptyRequirements", "Empty Requirements", depts, true, "UG", new CreditFilter(), false) };
                programRepoMock.Setup(pr => pr.GetAsync()).Returns(Task.FromResult(program));

                // Mock successful add of a plan from repository
                var degreePlan = new DegreePlan("0000894");
                degreePlan.Id = 0;
                degreePlan.AddTerm("2012/FA");
                degreePlan.AddTerm("2012/SP");
                degreePlan.AddTerm("2013/SP");
                studentDegreePlanRepoMock.Setup(x => x.AddAsync(It.IsAny<DegreePlan>())).Returns(Task.FromResult(degreePlan));

                // Mock grade repository get
                var grades = await new TestGradeRepository().GetAsync();
                gradeRepoMock.Setup(x => x.GetAsync()).Returns(Task.FromResult(grades));

                var studentAcademicCredits = await new TestAcademicCreditRepository().GetAsync();
                Domain.Student.Entities.GradeRestriction gradeRestriction = new Domain.Student.Entities.GradeRestriction(false);
                historyEntity = new Domain.Student.Entities.AcademicHistory(studentAcademicCredits, gradeRestriction, null);
                historyEntity.StudentId = "0000894";
                historyDto = new AcademicHistoryEntityToAcademicHistory2DtoAdapter(adapterRegistry, logger).MapToType(historyEntity);
                academicHistoryServiceMock.Setup(svc => svc.ConvertAcademicCreditsToAcademicHistoryDtoAsync(historyEntity.StudentId, It.IsAny<IEnumerable<Domain.Student.Entities.AcademicCredit>>(), It.IsAny<Domain.Student.Entities.Student>()))
                    .Returns(Task.FromResult(historyDto));

                // Mock Adapter 
                var degreePlanEntityToDegreePlan4DtoAdapter = new AutoMapperAdapter<DegreePlan, Dtos.Student.DegreePlans.DegreePlan4>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<DegreePlan, Dtos.Student.DegreePlans.DegreePlan4>()).Returns(degreePlanEntityToDegreePlan4DtoAdapter);

                studentDegreePlanService = new StudentDegreePlanService(
                    adapterRegistry, studentDegreePlanRepo, termRepo, studentRepo, studentProgramRepo, courseRepo, sectionRepo, programRepo,
                    academicCreditRepo, requirementRepo, ruleRepo, progReqRepo, catalogRepo, gradeRepo, academicHistoryService,
                    currentUserFactory, roleRepo, logger, baseConfigurationRepository, studentConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                SetupCleanup();
            }

            [TestMethod]
            public async Task StudentDegreePlanService_CreateDegreePlan4Async_Success()
            {
                var result = await studentDegreePlanService.CreateDegreePlan4Async("0000894");
                Assert.AreEqual("0000894", result.DegreePlan.PersonId);
                Assert.AreEqual("0000894", result.AcademicHistory.StudentId);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task StudentDegreePlanService_CreateDegreePlan4Async_ExceptionIfCurrentUserNotSelf()
            {
                var result = await studentDegreePlanService.CreateDegreePlan4Async("0000896");
            }
        }

        [TestClass]
        public class StudentDegreePlanService_CreateDegreePlan5Async : StudentDegreePlanServiceTestsSetup
        {
            private Domain.Student.Entities.AcademicHistory historyEntity;
            private Dtos.Student.AcademicHistory3 historyDto;


            [TestInitialize]
            public async void Initialize()
            {
                SetupInitialize();

                // Mock the get from the student repository
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));
                var student2 = new Domain.Student.Entities.Student("0000896", "Jones", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000896")).Returns(Task.FromResult(student2));
                studentRepoMock.Setup(x => x.GetAsync("StudentWillReturnNull")).Returns(Task.FromResult(null as Domain.Student.Entities.Student));

                // Mock the get of the planning terms
                var allTerms = await new TestTermRepository().GetAsync();
                termRepoMock.Setup(x => x.GetAsync()).ReturnsAsync(allTerms);

                // Mock the get of the student programs
                var studentPrograms = await new TestStudentProgramRepository().GetAsync("0000896");
                studentProgramRepoMock.Setup(x => x.GetAsync("0000896")).ReturnsAsync(studentPrograms);
                studentProgramRepoMock.Setup(x => x.GetAsync("0000894")).ReturnsAsync(studentPrograms);

                // Mock an empty program 
                IEnumerable<string> depts = new List<string>() { "Dept1" };
                IEnumerable<Domain.Student.Entities.Requirements.Program> program =
                    new List<Domain.Student.Entities.Requirements.Program>() { new Domain.Student.Entities.Requirements.Program("EmptyRequirements", "Empty Requirements", depts, true, "UG", new CreditFilter(), false) };
                programRepoMock.Setup(pr => pr.GetAsync()).Returns(Task.FromResult(program));

                // Mock successful add of a plan from repository
                var degreePlan = new DegreePlan("0000894");
                degreePlan.Id = 0;
                degreePlan.AddTerm("2012/FA");
                degreePlan.AddTerm("2012/SP");
                degreePlan.AddTerm("2013/SP");
                studentDegreePlanRepoMock.Setup(x => x.AddAsync(It.IsAny<DegreePlan>())).Returns(Task.FromResult(degreePlan));

                // Mock grade repository get
                var grades = await new TestGradeRepository().GetAsync();
                gradeRepoMock.Setup(x => x.GetAsync()).Returns(Task.FromResult(grades));

                var studentAcademicCredits = await new TestAcademicCreditRepository().GetAsync();
                Domain.Student.Entities.GradeRestriction gradeRestriction = new Domain.Student.Entities.GradeRestriction(false);
                historyEntity = new Domain.Student.Entities.AcademicHistory(studentAcademicCredits, gradeRestriction, null);
                historyEntity.StudentId = "0000894";
                historyDto = new AcademicHistoryEntityToAcademicHistory3DtoAdapter(adapterRegistry, logger).MapToType(historyEntity);
                academicHistoryServiceMock.Setup(svc => svc.ConvertAcademicCreditsToAcademicHistoryDto2Async(historyEntity.StudentId, It.IsAny<IEnumerable<Domain.Student.Entities.AcademicCredit>>(), It.IsAny<Domain.Student.Entities.Student>()))
                    .Returns(Task.FromResult(historyDto));

                // Mock Adapter 
                var degreePlanEntityToDegreePlan4DtoAdapter = new AutoMapperAdapter<DegreePlan, Dtos.Student.DegreePlans.DegreePlan4>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<DegreePlan, Dtos.Student.DegreePlans.DegreePlan4>()).Returns(degreePlanEntityToDegreePlan4DtoAdapter);

                studentDegreePlanService = new StudentDegreePlanService(
                    adapterRegistry, studentDegreePlanRepo, termRepo, studentRepo, studentProgramRepo, courseRepo, sectionRepo, programRepo,
                    academicCreditRepo, requirementRepo, ruleRepo, progReqRepo, catalogRepo, gradeRepo, academicHistoryService,
                    currentUserFactory, roleRepo, logger, baseConfigurationRepository, studentConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                SetupCleanup();
            }

            [TestMethod]
            public async Task StudentDegreePlanService_CreateDegreePlan5Async_Success()
            {
                var result = await studentDegreePlanService.CreateDegreePlan5Async("0000894");
                Assert.AreEqual("0000894", result.DegreePlan.PersonId);
                Assert.AreEqual("0000894", result.AcademicHistory.StudentId);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task StudentDegreePlanService_CreateDegreePlan5Async_ExceptionIfCurrentUserNotSelf()
            {
                var result = await studentDegreePlanService.CreateDegreePlan5Async("0000896");
            }
            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task StudentDegreePlanService_CreateDegreePlan5Async_ExceptionIfStudentRepositoryReturnsNull()
            {

                var result = await studentDegreePlanService.CreateDegreePlan5Async("StudentWillReturnNull");
            }
        }

        [TestClass]
        public class StudentDegreePlanService_CreateDegreePlan6Async : StudentDegreePlanServiceTestsSetup
        {
            private Domain.Student.Entities.AcademicHistory historyEntity;
            private Dtos.Student.AcademicHistory4 historyDto;


            [TestInitialize]
            public async void Initialize()
            {
                SetupInitialize();

                // Mock the get from the student repository
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));
                var student2 = new Domain.Student.Entities.Student("0000896", "Jones", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000896")).Returns(Task.FromResult(student2));
                studentRepoMock.Setup(x => x.GetAsync("StudentWillReturnNull")).Returns(Task.FromResult(null as Domain.Student.Entities.Student));

                // Mock the get of the planning terms
                var allTerms = await new TestTermRepository().GetAsync();
                termRepoMock.Setup(x => x.GetAsync()).ReturnsAsync(allTerms);

                // Mock the get of the student programs
                var studentPrograms = await new TestStudentProgramRepository().GetAsync("0000896");
                studentProgramRepoMock.Setup(x => x.GetAsync("0000896")).ReturnsAsync(studentPrograms);
                studentProgramRepoMock.Setup(x => x.GetAsync("0000894")).ReturnsAsync(studentPrograms);

                // Mock an empty program 
                IEnumerable<string> depts = new List<string>() { "Dept1" };
                IEnumerable<Domain.Student.Entities.Requirements.Program> program =
                    new List<Domain.Student.Entities.Requirements.Program>() { new Domain.Student.Entities.Requirements.Program("EmptyRequirements", "Empty Requirements", depts, true, "UG", new CreditFilter(), false) };
                programRepoMock.Setup(pr => pr.GetAsync()).Returns(Task.FromResult(program));

                // Mock successful add of a plan from repository
                var degreePlan = new DegreePlan("0000894");
                degreePlan.Id = 0;
                degreePlan.AddTerm("2012/FA");
                degreePlan.AddTerm("2012/SP");
                degreePlan.AddTerm("2013/SP");
                studentDegreePlanRepoMock.Setup(x => x.AddAsync(It.IsAny<DegreePlan>())).Returns(Task.FromResult(degreePlan));

                // Mock grade repository get
                var grades = await new TestGradeRepository().GetAsync();
                gradeRepoMock.Setup(x => x.GetAsync()).Returns(Task.FromResult(grades));

                var studentAcademicCredits = await new TestAcademicCreditRepository().GetAsync();
                Domain.Student.Entities.GradeRestriction gradeRestriction = new Domain.Student.Entities.GradeRestriction(false);
                historyEntity = new Domain.Student.Entities.AcademicHistory(studentAcademicCredits, gradeRestriction, null);
                historyEntity.StudentId = "0000894";
                historyDto = new AcademicHistoryEntityToAcademicHistory4DtoAdapter(adapterRegistry, logger).MapToType(historyEntity);
                academicHistoryServiceMock.Setup(svc => svc.ConvertAcademicCreditsToAcademicHistoryDto4Async(historyEntity.StudentId, It.IsAny<IEnumerable<Domain.Student.Entities.AcademicCredit>>(), It.IsAny<Domain.Student.Entities.Student>()))
                    .Returns(Task.FromResult(historyDto));

                // Mock Adapter 
                var degreePlanEntityToDegreePlan4DtoAdapter = new DegreePlanEntity4Adapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<DegreePlan, Dtos.Student.DegreePlans.DegreePlan4>()).Returns(degreePlanEntityToDegreePlan4DtoAdapter);

                studentDegreePlanService = new StudentDegreePlanService(
                    adapterRegistry, studentDegreePlanRepo, termRepo, studentRepo, studentProgramRepo, courseRepo, sectionRepo, programRepo,
                    academicCreditRepo, requirementRepo, ruleRepo, progReqRepo, catalogRepo, gradeRepo, academicHistoryService,
                    currentUserFactory, roleRepo, logger, baseConfigurationRepository, studentConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                SetupCleanup();
            }

            [TestMethod]
            public async Task StudentDegreePlanService_CreateDegreePlan6Async_Success()
            {
                var result = await studentDegreePlanService.CreateDegreePlan6Async("0000894");
                Assert.AreEqual("0000894", result.DegreePlan.PersonId);
                Assert.AreEqual("0000894", result.AcademicHistory.StudentId);
                Assert.IsTrue(result.DegreePlan.Terms.Any());
            }

            [TestMethod]
            public async Task StudentDegreePlanService_CreateDegreePlan6Async_Success_RegistrationConfiguration_Null()
            {
                RegistrationConfiguration configuration = null;
                studentConfigurationRepositoryMock.Setup(repo => repo.GetRegistrationConfigurationAsync()).ReturnsAsync(configuration);

                studentDegreePlanService = new StudentDegreePlanService(
                    adapterRegistry, studentDegreePlanRepo, termRepo, studentRepo, studentProgramRepo, courseRepo, sectionRepo, programRepo,
                    academicCreditRepo, requirementRepo, ruleRepo, progReqRepo, catalogRepo, gradeRepo, academicHistoryService,
                    currentUserFactory, roleRepo, logger, baseConfigurationRepository, studentConfigurationRepository);


                var result = await studentDegreePlanService.CreateDegreePlan6Async("0000894");
                Assert.AreEqual("0000894", result.DegreePlan.PersonId);
                Assert.AreEqual("0000894", result.AcademicHistory.StudentId);
                Assert.IsTrue(result.DegreePlan.Terms.Any());
            }

            [TestMethod]
            public async Task StudentDegreePlanService_CreateDegreePlan6Async_Success_RegistrationConfiguration_AddDefaultTermsToDegreePlan_False()
            {
                var studentId = "0000894";
                DegreePlan degreePlanEntity = new DegreePlan(studentId) { Id = 1 };
                studentDegreePlanRepoMock.Setup(x => x.AddAsync(It.IsAny<DegreePlan>())).Returns(Task.FromResult(degreePlanEntity));

                RegistrationConfiguration configuration = new RegistrationConfiguration(false, 0) { AddDefaultTermsToDegreePlan = false };
                studentConfigurationRepositoryMock.Setup(repo => repo.GetRegistrationConfigurationAsync()).ReturnsAsync(configuration);

                studentDegreePlanService = new StudentDegreePlanService(
                    adapterRegistry, studentDegreePlanRepo, termRepo, studentRepo, studentProgramRepo, courseRepo, sectionRepo, programRepo,
                    academicCreditRepo, requirementRepo, ruleRepo, progReqRepo, catalogRepo, gradeRepo, academicHistoryService,
                    currentUserFactory, roleRepo, logger, baseConfigurationRepository, studentConfigurationRepository);


                var result = await studentDegreePlanService.CreateDegreePlan6Async(studentId);
                Assert.AreEqual(studentId, result.DegreePlan.PersonId);
                Assert.AreEqual(studentId, result.AcademicHistory.StudentId);
                Assert.IsFalse(result.DegreePlan.Terms.Any());
            }

            [TestMethod]
            public async Task StudentDegreePlanService_CreateDegreePlan6Async_Success_RegistrationConfiguration_AddDefaultTermsToDegreePlan_True()
            {
                RegistrationConfiguration configuration = new RegistrationConfiguration(false, 0);
                studentConfigurationRepositoryMock.Setup(repo => repo.GetRegistrationConfigurationAsync()).ReturnsAsync(configuration);

                studentDegreePlanService = new StudentDegreePlanService(
                    adapterRegistry, studentDegreePlanRepo, termRepo, studentRepo, studentProgramRepo, courseRepo, sectionRepo, programRepo,
                    academicCreditRepo, requirementRepo, ruleRepo, progReqRepo, catalogRepo, gradeRepo, academicHistoryService,
                    currentUserFactory, roleRepo, logger, baseConfigurationRepository, studentConfigurationRepository);


                var result = await studentDegreePlanService.CreateDegreePlan6Async("0000894");
                Assert.AreEqual("0000894", result.DegreePlan.PersonId);
                Assert.AreEqual("0000894", result.AcademicHistory.StudentId);
                Assert.IsTrue(result.DegreePlan.Terms.Any());
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task StudentDegreePlanService_CreateDegreePlan6Async_ExceptionIfCurrentUserNotSelf()
            {
                var result = await studentDegreePlanService.CreateDegreePlan6Async("0000896");
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task StudentDegreePlanService_CreateDegreePlan6Async_ExceptionIfStudentRepositoryReturnsNull()
            {

                var result = await studentDegreePlanService.CreateDegreePlan6Async("StudentWillReturnNull");
            }
        }

        [TestClass]
        public class StudentDegreePlanService_UpdateDegreePlan : StudentDegreePlanServiceTestsSetup
        {
            private Dtos.Student.DegreePlans.DegreePlan degreePlanDto;

            [TestInitialize]
            public async void Initialize()
            {
                SetupInitialize();

                // Mock the get from the student repository
                Domain.Student.Entities.Student student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));

                Domain.Student.Entities.Student student2 = new Domain.Student.Entities.Student("0000896", "Jones", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000896")).Returns(Task.FromResult(student2));

                // Mock the get of the planning terms
                IEnumerable<Domain.Student.Entities.Term> allTerms = await new TestTermRepository().GetAsync();
                termRepoMock.Setup(x => x.GetAsync()).ReturnsAsync(allTerms);

                // Mock successful add of a plan from repository
                DegreePlan degreePlan = new DegreePlan(1, "0000894", 0);
                degreePlan.AddTerm("2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Domain.Student.Entities.DegreePlans.DegreePlan>())).Returns(Task.FromResult(degreePlan));

                // Mock grade repo get
                var grades = await new TestGradeRepository().GetAsync();
                gradeRepoMock.Setup(x => x.GetAsync()).Returns(Task.FromResult(grades));

                // Mock DateTimeOffset->DateTime adapter to verify backward compatibility
                var datetimeOffsetAdapter = new DateTimeOffsetToDateTimeAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<System.DateTimeOffset, System.DateTime>()).Returns(datetimeOffsetAdapter);

                // Mock dto Adapter 
                var degreePlanEntityToDtoAdapter = new DegreePlanEntityAdapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan>()).Returns(degreePlanEntityToDtoAdapter);
                //var degreePlanWarningDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.DegreePlans.DegreePlanWarning, Dtos.Student.DegreePlans.DegreePlanWarning>(adapterRegistry, logger);
                //adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlanWarning, Dtos.Student.DegreePlans.DegreePlanWarning>()).Returns(degreePlanWarningDtoAdapter);
                // Mock entity adapter
                var degreePlanDtoToEntityAdapter = new DegreePlanDtoAdapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Dtos.Student.DegreePlans.DegreePlan, Domain.Student.Entities.DegreePlans.DegreePlan>()).Returns(degreePlanDtoToEntityAdapter);
                //var degreePlanWarningEntityAdapter = new AutoMapperAdapter<Dtos.Student.DegreePlans.DegreePlanWarning, Domain.Student.Entities.DegreePlans.DegreePlanWarning>(adapterRegistry, logger);
                //adapterRegistryMock.Setup(reg => reg.GetAdapter<Dtos.Student.DegreePlans.DegreePlanWarning, Domain.Student.Entities.DegreePlans.DegreePlanWarning>()).Returns(degreePlanWarningEntityAdapter);

                // Map degree plan to dto
                degreePlanDto = degreePlanEntityToDtoAdapter.MapToType(degreePlan);

                studentDegreePlanService = new StudentDegreePlanService(
                    adapterRegistry, studentDegreePlanRepo, termRepo, studentRepo, studentProgramRepo, courseRepo, sectionRepo, programRepo,
                    academicCreditRepo, requirementRepo, ruleRepo, progReqRepo, catalogRepo, gradeRepo, academicHistoryService,
                    currentUserFactory, roleRepo, logger, baseConfigurationRepository, studentConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                SetupCleanup();
            }

            [TestMethod]
            public async Task StudentDegreePlanService_UpdateDegreePlanAsync_Success()
            {
                // Only Verifies plan is converted to an entity, updated, and returned in dto form.
                // This doesn't error-out only because there are no courses on the degree plan
#pragma warning disable 618
                var dp = await studentDegreePlanService.UpdateDegreePlanAsync(degreePlanDto);
#pragma warning restore 618
                Assert.AreEqual("0000894", dp.PersonId);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task StudentDegreePlanService_UpdateDegreePlanAsync_ExceptionIfCurrentUserNotSelf()
            {
                // Only Verifies plan is converted to an entity, updated, and returned in dto form.
                // This doesn't error-out only because there are no courses on the degree plan
                degreePlanDto.PersonId = "0000896";
#pragma warning disable 618
                var dp = await studentDegreePlanService.UpdateDegreePlanAsync(degreePlanDto);
#pragma warning restore 618
            }
        }

        [TestClass]
        public class StudentDegreePlanService_UpdateDegreePlan2 : StudentDegreePlanServiceTestsSetup
        {
            private Dtos.Student.DegreePlans.DegreePlan2 degreePlanDto;

            [TestInitialize]
            public async void Initialize()
            {
                SetupInitialize();

                // Mock the get from the student repository
                Domain.Student.Entities.Student student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));

                Domain.Student.Entities.Student student2 = new Domain.Student.Entities.Student("0000896", "Jones", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000896")).Returns(Task.FromResult(student2));

                // Mock the get of the planning terms
                IEnumerable<Domain.Student.Entities.Term> allTerms = await new TestTermRepository().GetAsync();
                termRepoMock.Setup(x => x.GetAsync()).ReturnsAsync(allTerms);

                // Mock successful add of a plan from repository
                DegreePlan degreePlan = new DegreePlan(1, "0000894", 0);
                degreePlan.AddTerm("2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Domain.Student.Entities.DegreePlans.DegreePlan>())).Returns(Task.FromResult(degreePlan));

                // Mock successful get of a degree plan so it can do the UpdateMissingProtections process.
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<int>())).Returns(Task.FromResult(degreePlan));

                // Mock grades get
                var grades = await new TestGradeRepository().GetAsync();
                gradeRepoMock.Setup(x => x.GetAsync()).Returns(Task.FromResult(grades));

                // Mock DateTimeOffset->DateTime adapter to verify backward compatibility
                var datetimeOffsetAdapter = new DateTimeOffsetToDateTimeAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<System.DateTimeOffset, System.DateTime>()).Returns(datetimeOffsetAdapter);

                // Mock dto Adapter 
                var degreePlanEntityToDtoAdapter = new DegreePlanEntity2Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan2>()).Returns(degreePlanEntityToDtoAdapter);

                // Mock entity adapter
                var degreePlanDtoToEntityAdapter = new DegreePlanDto2Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Dtos.Student.DegreePlans.DegreePlan2, Domain.Student.Entities.DegreePlans.DegreePlan>()).Returns(degreePlanDtoToEntityAdapter);


                // Map degree plan to dto
                degreePlanDto = degreePlanEntityToDtoAdapter.MapToType(degreePlan);

                studentDegreePlanService = new StudentDegreePlanService(
                    adapterRegistry, studentDegreePlanRepo, termRepo, studentRepo, studentProgramRepo, courseRepo, sectionRepo, programRepo,
                    academicCreditRepo, requirementRepo, ruleRepo, progReqRepo, catalogRepo, gradeRepo, academicHistoryService,
                    currentUserFactory, roleRepo, logger, baseConfigurationRepository, studentConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                SetupCleanup();
            }

            [TestMethod]
            public async Task StudentDegreePlanService_UpdateDegreePlan2Async_Success()
            {
                // Only Verifies plan is converted to an entity, updated, and returned in dto form.
                // This doesn't error-out only because there are no courses on the degree plan
#pragma warning disable 618
                var dp = await studentDegreePlanService.UpdateDegreePlan2Async(degreePlanDto);
#pragma warning restore 618
                Assert.AreEqual("0000894", dp.PersonId);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task StudentDegreePlanService_UpdateDegreePlan2Async_ExceptionIfCurrentUserNotSelf()
            {
                // Only Verifies plan is converted to an entity, updated, and returned in dto form.
                // This doesn't error-out only because there are no courses on the degree plan
                degreePlanDto.PersonId = "0000896";
#pragma warning disable 618
                var dp = await studentDegreePlanService.UpdateDegreePlan2Async(degreePlanDto);
#pragma warning restore 618
            }
        }

        [TestClass]
        public class StudentDegreePlanService_UpdateDegreePlan3 : StudentDegreePlanServiceTestsSetup
        {

            private Dtos.Student.DegreePlans.DegreePlan3 degreePlanDto;
            private Domain.Student.Entities.AcademicHistory historyEntity;
            private Dtos.Student.AcademicHistory2 historyDto;

            [TestInitialize]
            public async void Initialize()
            {
                SetupInitialize();

                // Mock the get from the student repository
                Domain.Student.Entities.Student student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));

                Domain.Student.Entities.Student student2 = new Domain.Student.Entities.Student("0000896", "Jones", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000896")).Returns(Task.FromResult(student2));

                // Mock the get of the planning terms
                IEnumerable<Domain.Student.Entities.Term> allTerms = await new TestTermRepository().GetAsync();
                termRepoMock.Setup(x => x.GetAsync()).ReturnsAsync(allTerms);

                // Mock successful add of a plan from repository
                DegreePlan degreePlan = new DegreePlan(1, "0000894", 0);
                degreePlan.AddTerm("2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Domain.Student.Entities.DegreePlans.DegreePlan>())).Returns(Task.FromResult(degreePlan));

                // Mock get for comparison against cached degree plan
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<int>())).Returns(Task.FromResult(degreePlan));

                // Mock grades get
                var grades = await new TestGradeRepository().GetAsync();
                gradeRepoMock.Setup(x => x.GetAsync()).Returns(Task.FromResult(grades));

                // Mock dto Adapter 
                var degreePlan3EntityToDtoAdapter = new DegreePlanEntity3Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan3>()).Returns(degreePlan3EntityToDtoAdapter);

                // Mock entity adapter
                var degreePlan3DtoToEntityAdapter = new DegreePlanDto3Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Dtos.Student.DegreePlans.DegreePlan3, Domain.Student.Entities.DegreePlans.DegreePlan>()).Returns(degreePlan3DtoToEntityAdapter);

                // Map degree plan to dto
                degreePlanDto = degreePlan3EntityToDtoAdapter.MapToType(degreePlan);

                // Mock conversion from academic credits to academic history
                var studentAcademicCredits = await new TestAcademicCreditRepository().GetAsync();
                Domain.Student.Entities.GradeRestriction gradeRestriction = new Domain.Student.Entities.GradeRestriction(false);
                historyEntity = new Domain.Student.Entities.AcademicHistory(studentAcademicCredits, gradeRestriction, null);
                historyEntity.StudentId = "0000894";
                historyDto = new AcademicHistoryEntityToAcademicHistory2DtoAdapter(adapterRegistry, logger).MapToType(historyEntity);
                academicHistoryServiceMock.Setup(svc => svc.ConvertAcademicCreditsToAcademicHistoryDtoAsync(historyEntity.StudentId, It.IsAny<IEnumerable<Domain.Student.Entities.AcademicCredit>>(), It.IsAny<Domain.Student.Entities.Student>()))
                    .Returns(Task.FromResult(historyDto));

                studentDegreePlanService = new StudentDegreePlanService(
                    adapterRegistry, studentDegreePlanRepo, termRepo, studentRepo, studentProgramRepo, courseRepo, sectionRepo, programRepo,
                    academicCreditRepo, requirementRepo, ruleRepo, progReqRepo, catalogRepo, gradeRepo, academicHistoryService,
                    currentUserFactory, roleRepo, logger, baseConfigurationRepository, studentConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                SetupCleanup();
            }

            [TestMethod]
            public async Task StudentDegreePlanService_UpdateDegreePlan3Async_Success()
            {
                // Only Verifies plan is converted to an entity, updated, and returned in dto form.
                // This doesn't error-out only because there are no courses on the degree plan
                var dp = await studentDegreePlanService.UpdateDegreePlan3Async(degreePlanDto);
                Assert.AreEqual("0000894", dp.PersonId);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task StudentDegreePlanService_UpdateDegreePlan3Async_ExceptionIfCurrentUserNotSelf()
            {
                // Only Verifies plan is converted to an entity, updated, and returned in dto form.
                // This doesn't error-out only because there are no courses on the degree plan
                degreePlanDto.PersonId = "0000896";
                var dp = await studentDegreePlanService.UpdateDegreePlan3Async(degreePlanDto);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task StudentDegreePlanService_UpdateDegreePlan3Async_ExceptionForProtectedChangeByStudent()
            {
                // Arrange - Set up cached degree plan that has a protection update. Student is now allowed to make changes to protected items.
                DegreePlan originalDegreePlan = new DegreePlan(1, "0000894", 0);
                originalDegreePlan.AddTerm("2012/FA");
                originalDegreePlan.NonTermPlannedCourses.Add(new PlannedCourse("01", "123", Domain.Student.Entities.GradingType.Graded, Domain.Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, null) { IsProtected = true });
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<int>())).Returns(Task.FromResult(originalDegreePlan));
                // Act
                var dp = await studentDegreePlanService.UpdateDegreePlan3Async(degreePlanDto);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task StudentDegreePlanService_UpdateDegreePlan3Async_ExceptionForApprovalChangeByStudent()
            {
                // Arrange - Set up cached degree plan that has an approval update. Student is now allowed to make approval changes.
                DegreePlan originalDegreePlan = new DegreePlan(1, "0000894", 0);
                originalDegreePlan.AddTerm("2012/FA");
                originalDegreePlan.LastReviewedDate = DateTime.Now;
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<int>())).Returns(Task.FromResult(originalDegreePlan));
                // Act
                var dp = await studentDegreePlanService.UpdateDegreePlan3Async(degreePlanDto);
            }
        }

        [TestClass]
        public class StudentDegreePlanService_UpdateDegreePlan4 : StudentDegreePlanServiceTestsSetup
        {

            private Dtos.Student.DegreePlans.DegreePlan4 degreePlanDto;
            private Domain.Student.Entities.AcademicHistory historyEntity;
            private Dtos.Student.AcademicHistory2 historyDto;

            [TestInitialize]
            public async void Initialize()
            {
                SetupInitialize();

                // Mock the get from the student repository
                Domain.Student.Entities.Student student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));

                Domain.Student.Entities.Student student2 = new Domain.Student.Entities.Student("0000896", "Jones", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000896")).Returns(Task.FromResult(student2));

                // Mock the get of the planning terms
                IEnumerable<Domain.Student.Entities.Term> allTerms = await new TestTermRepository().GetAsync();
                termRepoMock.Setup(x => x.GetAsync()).ReturnsAsync(allTerms);

                // Mock successful add of a plan from repository
                DegreePlan degreePlan = new DegreePlan(1, "0000894", 0);
                degreePlan.AddTerm("2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Domain.Student.Entities.DegreePlans.DegreePlan>())).Returns(Task.FromResult(degreePlan));

                // Mock get for comparison against cached degree plan
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<int>())).Returns(Task.FromResult(degreePlan));

                // Mock grades get
                var grades = await new TestGradeRepository().GetAsync();
                gradeRepoMock.Setup(x => x.GetAsync()).Returns(Task.FromResult(grades));

                // Mock dto Adapter 
                var degreePlan4EntityToDtoAdapter = new DegreePlanEntity4Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan4>()).Returns(degreePlan4EntityToDtoAdapter);

                // Mock entity adapter
                var degreePlan4DtoToEntityAdapter = new DegreePlanDto4Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Dtos.Student.DegreePlans.DegreePlan4, Domain.Student.Entities.DegreePlans.DegreePlan>()).Returns(degreePlan4DtoToEntityAdapter);

                // Map degree plan to dto
                degreePlanDto = degreePlan4EntityToDtoAdapter.MapToType(degreePlan);

                // Mock conversion from academic credits to academic history
                var studentAcademicCredits = await new TestAcademicCreditRepository().GetAsync();

                // Mock academic credit repository get using student ids returning dict with student's academic credits
                var academicCreditDict = new Dictionary<string, List<Domain.Student.Entities.AcademicCredit>>();
                academicCreditDict["0000894"] = studentAcademicCredits.ToList();
                academicCreditRepoMock.Setup(x => x.GetAcademicCreditByStudentIdsAsync(It.IsAny<List<string>>(), false, true, It.IsAny<bool>())).Returns(Task.FromResult<Dictionary<string, List<Domain.Student.Entities.AcademicCredit>>>(academicCreditDict));

                Domain.Student.Entities.GradeRestriction gradeRestriction = new Domain.Student.Entities.GradeRestriction(false);
                historyEntity = new Domain.Student.Entities.AcademicHistory(studentAcademicCredits, gradeRestriction, null);
                historyEntity.StudentId = "0000894";
                historyDto = new AcademicHistoryEntityToAcademicHistory2DtoAdapter(adapterRegistry, logger).MapToType(historyEntity);
                academicHistoryServiceMock.Setup(svc => svc.ConvertAcademicCreditsToAcademicHistoryDtoAsync(historyEntity.StudentId, It.IsAny<IEnumerable<Domain.Student.Entities.AcademicCredit>>(), It.IsAny<Domain.Student.Entities.Student>()))
                    .Returns(Task.FromResult(historyDto));

                studentDegreePlanService = new StudentDegreePlanService(
                    adapterRegistry, studentDegreePlanRepo, termRepo, studentRepo, studentProgramRepo, courseRepo, sectionRepo, programRepo,
                    academicCreditRepo, requirementRepo, ruleRepo, progReqRepo, catalogRepo, gradeRepo, academicHistoryService,
                    currentUserFactory, roleRepo, logger, baseConfigurationRepository, studentConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                SetupCleanup();
            }

            [TestMethod]
            public async Task StudentDegreePlanService_UpdateDegreePlan4Async_Success()
            {
                // Only Verifies plan is converted to an entity, updated, and returned in dto form.
                // This doesn't error-out only because there are no courses on the degree plan
                var result = await studentDegreePlanService.UpdateDegreePlan4Async(degreePlanDto);
                Assert.AreEqual("0000894", result.DegreePlan.PersonId);
            }

            [TestMethod]
            public async Task StudentDegreePlanService_UpdateDegreePlan4Async_IncludesAcademicHistory()
            {
                // Only Verifies plan is converted to an entity, updated, and returned in dto form.
                // This doesn't error-out only because there are no courses on the degree plan
                var result = await studentDegreePlanService.UpdateDegreePlan4Async(degreePlanDto);
                Assert.AreEqual("0000894", result.DegreePlan.PersonId);

                // Verify that academic history is also tacked on
                Assert.AreEqual("0000894", result.AcademicHistory.StudentId);
                Assert.IsTrue(result.AcademicHistory.AcademicTerms.Count() > 0);
                Assert.AreEqual(historyEntity.AcademicTerms.Count(), result.AcademicHistory.AcademicTerms.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task StudentDegreePlanService_UpdateDegreePlan4Async_ExceptionIfCurrentUserNotSelf()
            {
                // Only Verifies plan is converted to an entity, updated, and returned in dto form.
                // This doesn't error-out only because there are no courses on the degree plan
                degreePlanDto.PersonId = "0000896";
                var dp = await studentDegreePlanService.UpdateDegreePlan4Async(degreePlanDto);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task StudentDegreePlanService_UpdateDegreePlan4Async_ExceptionForProtectedChangeByStudent()
            {
                // Arrange - Set up cached degree plan that has a protection update. Student is now allowed to make changes to protected items.
                DegreePlan originalDegreePlan = new DegreePlan(1, "0000894", 0);
                originalDegreePlan.AddTerm("2012/FA");
                originalDegreePlan.NonTermPlannedCourses.Add(new PlannedCourse("01", "123", Domain.Student.Entities.GradingType.Graded, Domain.Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, null) { IsProtected = true });
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<int>())).Returns(Task.FromResult(originalDegreePlan));
                // Act
                var dp = await studentDegreePlanService.UpdateDegreePlan4Async(degreePlanDto);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task StudentDegreePlanService_UpdateDegreePlan4Async_ExceptionForApprovalChangeByStudent()
            {
                // Arrange - Set up cached degree plan that has an approval update. Student is now allowed to make approval changes.
                DegreePlan originalDegreePlan = new DegreePlan(1, "0000894", 0);
                originalDegreePlan.AddTerm("2012/FA");
                originalDegreePlan.LastReviewedDate = DateTime.Now;
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<int>())).Returns(Task.FromResult(originalDegreePlan));
                // Act
                var dp = await studentDegreePlanService.UpdateDegreePlan4Async(degreePlanDto);
            }
        }

        [TestClass]
        public class StudentDegreePlanService_UpdateDegreePlan5Async : StudentDegreePlanServiceTestsSetup
        {

            private Dtos.Student.DegreePlans.DegreePlan4 degreePlanDto;
            private Domain.Student.Entities.AcademicHistory historyEntity;
            private Dtos.Student.AcademicHistory3 historyDto;

            [TestInitialize]
            public async void Initialize()
            {
                SetupInitialize();

                // Mock the get from the student repository
                Domain.Student.Entities.Student student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));

                Domain.Student.Entities.Student student2 = new Domain.Student.Entities.Student("0000896", "Jones", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000896")).Returns(Task.FromResult(student2));

                // Mock the get of the planning terms
                IEnumerable<Domain.Student.Entities.Term> allTerms = await new TestTermRepository().GetAsync();
                termRepoMock.Setup(x => x.GetAsync()).ReturnsAsync(allTerms);

                // Mock successful add of a plan from repository
                DegreePlan degreePlan = new DegreePlan(1, "0000894", 0);
                degreePlan.AddTerm("2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Domain.Student.Entities.DegreePlans.DegreePlan>())).Returns(Task.FromResult(degreePlan));

                // Mock get for comparison against cached degree plan
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<int>())).Returns(Task.FromResult(degreePlan));

                // Mock grades get
                var grades = await new TestGradeRepository().GetAsync();
                gradeRepoMock.Setup(x => x.GetAsync()).Returns(Task.FromResult(grades));

                // Mock dto Adapter 
                var degreePlan4EntityToDtoAdapter = new DegreePlanEntity4Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan4>()).Returns(degreePlan4EntityToDtoAdapter);

                // Mock entity adapter
                var degreePlan4DtoToEntityAdapter = new DegreePlanDto4Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Dtos.Student.DegreePlans.DegreePlan4, Domain.Student.Entities.DegreePlans.DegreePlan>()).Returns(degreePlan4DtoToEntityAdapter);

                // Map degree plan to dto
                degreePlanDto = degreePlan4EntityToDtoAdapter.MapToType(degreePlan);

                // Mock conversion from academic credits to academic history
                var studentAcademicCredits = await new TestAcademicCreditRepository().GetAsync();

                // Mock academic credit repository get using student ids returning dict with student's academic credits
                var academicCreditDict = new Dictionary<string, List<Domain.Student.Entities.AcademicCredit>>();
                academicCreditDict["0000894"] = studentAcademicCredits.ToList();
                academicCreditRepoMock.Setup(x => x.GetAcademicCreditByStudentIdsAsync(It.IsAny<List<string>>(), false, true, It.IsAny<bool>())).Returns(Task.FromResult<Dictionary<string, List<Domain.Student.Entities.AcademicCredit>>>(academicCreditDict));

                Domain.Student.Entities.GradeRestriction gradeRestriction = new Domain.Student.Entities.GradeRestriction(false);
                historyEntity = new Domain.Student.Entities.AcademicHistory(studentAcademicCredits, gradeRestriction, null);
                historyEntity.StudentId = "0000894";
                historyDto = new AcademicHistoryEntityToAcademicHistory3DtoAdapter(adapterRegistry, logger).MapToType(historyEntity);
                academicHistoryServiceMock.Setup(svc => svc.ConvertAcademicCreditsToAcademicHistoryDto2Async(historyEntity.StudentId, It.IsAny<IEnumerable<Domain.Student.Entities.AcademicCredit>>(), It.IsAny<Domain.Student.Entities.Student>()))
                    .Returns(Task.FromResult(historyDto));

                studentDegreePlanService = new StudentDegreePlanService(
                    adapterRegistry, studentDegreePlanRepo, termRepo, studentRepo, studentProgramRepo, courseRepo, sectionRepo, programRepo,
                    academicCreditRepo, requirementRepo, ruleRepo, progReqRepo, catalogRepo, gradeRepo, academicHistoryService,
                    currentUserFactory, roleRepo, logger, baseConfigurationRepository, studentConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                SetupCleanup();
            }

            [TestMethod]
            public async Task StudentDegreePlanService_UpdateDegreePlan5Async_Success()
            {
                // Only Verifies plan is converted to an entity, updated, and returned in dto form.
                // This doesn't error-out only because there are no courses on the degree plan
                var result = await studentDegreePlanService.UpdateDegreePlan5Async(degreePlanDto);
                Assert.AreEqual("0000894", result.DegreePlan.PersonId);
            }

            [TestMethod]
            public async Task StudentDegreePlanService_UpdateDegreePlan5Async_IncludesAcademicHistory()
            {
                // Only Verifies plan is converted to an entity, updated, and returned in dto form.
                // This doesn't error-out only because there are no courses on the degree plan
                var result = await studentDegreePlanService.UpdateDegreePlan5Async(degreePlanDto);
                Assert.AreEqual("0000894", result.DegreePlan.PersonId);

                // Verify that academic history is also tacked on
                Assert.AreEqual("0000894", result.AcademicHistory.StudentId);
                Assert.IsTrue(result.AcademicHistory.AcademicTerms.Count() > 0);
                Assert.AreEqual(historyEntity.AcademicTerms.Count(), result.AcademicHistory.AcademicTerms.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task StudentDegreePlanService_UpdateDegreePlan5Async_ExceptionIfCurrentUserNotSelf()
            {
                // Only Verifies plan is converted to an entity, updated, and returned in dto form.
                // This doesn't error-out only because there are no courses on the degree plan
                degreePlanDto.PersonId = "0000896";
                var dp = await studentDegreePlanService.UpdateDegreePlan5Async(degreePlanDto);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task StudentDegreePlanService_UpdateDegreePlan5Async_ExceptionForProtectedChangeByStudent()
            {
                // Arrange - Set up cached degree plan that has a protection update. Student is now allowed to make changes to protected items.
                DegreePlan originalDegreePlan = new DegreePlan(1, "0000894", 0);
                originalDegreePlan.AddTerm("2012/FA");
                originalDegreePlan.NonTermPlannedCourses.Add(new PlannedCourse("01", "123", Domain.Student.Entities.GradingType.Graded, Domain.Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, null) { IsProtected = true });
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<int>())).Returns(Task.FromResult(originalDegreePlan));
                // Act
                var dp = await studentDegreePlanService.UpdateDegreePlan5Async(degreePlanDto);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task DegreePlanService_UpdateDegreePlan5Async_ExceptionForApprovalChangeByStudent()
            {
                // Arrange - Set up cached degree plan that has an approval update. Student is now allowed to make approval changes.
                DegreePlan originalDegreePlan = new DegreePlan(1, "0000894", 0);
                originalDegreePlan.AddTerm("2012/FA");
                originalDegreePlan.LastReviewedDate = DateTime.Now;
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<int>())).Returns(Task.FromResult(originalDegreePlan));
                // Act
                var dp = await studentDegreePlanService.UpdateDegreePlan5Async(degreePlanDto);
            }
        }

        [TestClass]
        public class StudentDegreePlanService_UpdateDegreePlan6Async : StudentDegreePlanServiceTestsSetup
        {

            private Dtos.Student.DegreePlans.DegreePlan4 degreePlanDto;
            private Domain.Student.Entities.AcademicHistory historyEntity;
            private Dtos.Student.AcademicHistory4 historyDto;

            [TestInitialize]
            public async void Initialize()
            {
                SetupInitialize();

                // Mock the get from the student repository
                Domain.Student.Entities.Student student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));

                Domain.Student.Entities.Student student2 = new Domain.Student.Entities.Student("0000896", "Jones", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000896")).Returns(Task.FromResult(student2));

                // Mock the get of the planning terms
                IEnumerable<Domain.Student.Entities.Term> allTerms = await new TestTermRepository().GetAsync();
                termRepoMock.Setup(x => x.GetAsync()).ReturnsAsync(allTerms);

                // Mock successful add of a plan from repository
                DegreePlan degreePlan = new DegreePlan(1, "0000894", 0);
                degreePlan.AddTerm("2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Domain.Student.Entities.DegreePlans.DegreePlan>())).Returns(Task.FromResult(degreePlan));

                // Mock get for comparison against cached degree plan
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<int>())).Returns(Task.FromResult(degreePlan));

                // Mock grades get
                var grades = await new TestGradeRepository().GetAsync();
                gradeRepoMock.Setup(x => x.GetAsync()).Returns(Task.FromResult(grades));

                // Mock dto Adapter 
                var degreePlan4EntityToDtoAdapter = new DegreePlanEntity4Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan4>()).Returns(degreePlan4EntityToDtoAdapter);

                // Mock entity adapter
                var degreePlan4DtoToEntityAdapter = new DegreePlanDto4Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Dtos.Student.DegreePlans.DegreePlan4, Domain.Student.Entities.DegreePlans.DegreePlan>()).Returns(degreePlan4DtoToEntityAdapter);

                // Map degree plan to dto
                degreePlanDto = degreePlan4EntityToDtoAdapter.MapToType(degreePlan);

                // Mock conversion from academic credits to academic history
                var studentAcademicCredits = await new TestAcademicCreditRepository().GetAsync();

                // Mock academic credit repository get using student ids returning dict with student's academic credits
                var academicCreditDict = new Dictionary<string, List<Domain.Student.Entities.AcademicCredit>>();
                academicCreditDict["0000894"] = studentAcademicCredits.ToList();
                academicCreditRepoMock.Setup(x => x.GetAcademicCreditByStudentIdsAsync(It.IsAny<List<string>>(), false, true, It.IsAny<bool>())).Returns(Task.FromResult<Dictionary<string, List<Domain.Student.Entities.AcademicCredit>>>(academicCreditDict));

                Domain.Student.Entities.GradeRestriction gradeRestriction = new Domain.Student.Entities.GradeRestriction(false);
                historyEntity = new Domain.Student.Entities.AcademicHistory(studentAcademicCredits, gradeRestriction, null);
                historyEntity.StudentId = "0000894";
                historyDto = new AcademicHistoryEntityToAcademicHistory4DtoAdapter(adapterRegistry, logger).MapToType(historyEntity);
                academicHistoryServiceMock.Setup(svc => svc.ConvertAcademicCreditsToAcademicHistoryDto4Async(historyEntity.StudentId, It.IsAny<IEnumerable<Domain.Student.Entities.AcademicCredit>>(), It.IsAny<Domain.Student.Entities.Student>()))
                    .Returns(Task.FromResult(historyDto));

                studentDegreePlanService = new StudentDegreePlanService(
                    adapterRegistry, studentDegreePlanRepo, termRepo, studentRepo, studentProgramRepo, courseRepo, sectionRepo, programRepo,
                    academicCreditRepo, requirementRepo, ruleRepo, progReqRepo, catalogRepo, gradeRepo, academicHistoryService,
                    currentUserFactory, roleRepo, logger, baseConfigurationRepository, studentConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                SetupCleanup();
            }

            [TestMethod]
            public async Task StudentDegreePlanService_UpdateDegreePlan6Async_Success()
            {
                // Only Verifies plan is converted to an entity, updated, and returned in dto form.
                // This doesn't error-out only because there are no courses on the degree plan
                var result = await studentDegreePlanService.UpdateDegreePlan6Async(degreePlanDto);
                Assert.AreEqual("0000894", result.DegreePlan.PersonId);
            }

            [TestMethod]
            public async Task StudentegreePlanService_UpdateDegreePlan6Async_IncludesAcademicHistory()
            {
                // Only Verifies plan is converted to an entity, updated, and returned in dto form.
                // This doesn't error-out only because there are no courses on the degree plan
                var result = await studentDegreePlanService.UpdateDegreePlan6Async(degreePlanDto);
                Assert.AreEqual("0000894", result.DegreePlan.PersonId);

                // Verify that academic history is also tacked on
                Assert.AreEqual("0000894", result.AcademicHistory.StudentId);
                Assert.IsTrue(result.AcademicHistory.AcademicTerms.Count() > 0);
                Assert.AreEqual(historyEntity.AcademicTerms.Count(), result.AcademicHistory.AcademicTerms.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task StudentDegreePlanService_UpdateDegreePlan6Async_ExceptionIfCurrentUserNotSelf()
            {
                // Only Verifies plan is converted to an entity, updated, and returned in dto form.
                // This doesn't error-out only because there are no courses on the degree plan
                degreePlanDto.PersonId = "0000896";
                var dp = await studentDegreePlanService.UpdateDegreePlan6Async(degreePlanDto);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task StudentDegreePlanService_UpdateDegreePlan6Async_ExceptionForProtectedChangeByStudent()
            {
                // Arrange - Set up cached degree plan that has a protection update. Student is now allowed to make changes to protected items.
                DegreePlan originalDegreePlan = new DegreePlan(1, "0000894", 0);
                originalDegreePlan.AddTerm("2012/FA");
                originalDegreePlan.NonTermPlannedCourses.Add(new PlannedCourse("01", "123", Domain.Student.Entities.GradingType.Graded, Domain.Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, null) { IsProtected = true });
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<int>())).Returns(Task.FromResult(originalDegreePlan));
                // Act
                var dp = await studentDegreePlanService.UpdateDegreePlan6Async(degreePlanDto);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task StudentDegreePlanService_UpdateDegreePlan6Async_ExceptionForApprovalChangeByStudent()
            {
                // Arrange - Set up cached degree plan that has an approval update. Student is now allowed to make approval changes.
                DegreePlan originalDegreePlan = new DegreePlan(1, "0000894", 0);
                originalDegreePlan.AddTerm("2012/FA");
                originalDegreePlan.LastReviewedDate = DateTime.Now;
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<int>())).Returns(Task.FromResult(originalDegreePlan));
                // Act
                var dp = await studentDegreePlanService.UpdateDegreePlan6Async(degreePlanDto);
            }
        }

        [TestClass]
        public class StudentDegreePlanService_Register : StudentDegreePlanServiceTestsSetup
        {
            [TestInitialize]
            public async void Initialize()
            {
                SetupInitialize();

                // Mock the get from the student repository
                var testStudentRepository = new TestStudentRepository();
                studentRepoMock.Setup(x => x.GetAsync(It.IsAny<string>())).Returns((string s) => testStudentRepository.GetAsync(s));

                // Mock successful add of a plan from repository
                DegreePlan degreePlan = new DegreePlan("0000894");
                degreePlan.Id = 1;
                degreePlan.AddTerm("2012/FA");
                degreePlan.AddTerm("2012/SP");
                degreePlan.AddTerm("2013/SP");
                var plannedCourse = new PlannedCourse("111", "111");
                degreePlan.AddCourse(plannedCourse, "2012/FA");
                var nonTermPlannedCourse = new PlannedCourse("222", "222");
                degreePlan.AddCourse(nonTermPlannedCourse, null);
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(degreePlan.Id)).Returns(Task.FromResult(degreePlan));

                // Mock the get from the term repository
                var testTermRepository = new TestTermRepository();
                termRepoMock.Setup(trepo => trepo.GetAsync(It.IsAny<string>())).Returns((string t) => testTermRepository.GetAsync(t));

                // Mock the get from the section repository for the non-term section
                var nonTermSection = new Domain.Student.Entities.Section("222", "222", "100", new DateTime(2012, 10, 1), 3.0m, null, "Test", "IN", new List<OfferingDepartment>() { new OfferingDepartment("AAA", 100m) }, new List<string>() { "AAA" }, "UG", new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddYears(-6)) });
                var nonTermSections = new List<Domain.Student.Entities.Section>() { nonTermSection };
                sectionRepoMock.Setup(sr => sr.GetCachedSectionsAsync(new List<string>() { "222" }, false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(nonTermSections));

                // Mock grade repository get
                var grades = await new TestGradeRepository().GetAsync();
                gradeRepoMock.Setup(x => x.GetAsync()).Returns(Task.FromResult(grades));

                // Mock Adapter 
                var degreePlanDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan>()).Returns(degreePlanDtoAdapter);

                var messages = new List<Domain.Student.Entities.RegistrationMessage>() { new Domain.Student.Entities.RegistrationMessage() { Message = "Success", SectionId = "" } };
                var response = new Domain.Student.Entities.RegistrationResponse(messages, null, null);
                studentRepoMock.Setup(x => x.RegisterAsync(It.IsAny<Domain.Student.Entities.RegistrationRequest>())).Returns(Task.FromResult(response));

                studentDegreePlanService = new StudentDegreePlanService(
                    adapterRegistry, studentDegreePlanRepo, termRepo, studentRepo, studentProgramRepo, courseRepo, sectionRepo, programRepo,
                    academicCreditRepo, requirementRepo, ruleRepo, progReqRepo, catalogRepo, gradeRepo, academicHistoryService,
                    currentUserFactory, roleRepo, logger, baseConfigurationRepository, studentConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                SetupCleanup();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentDegreePlanService_RegisterAsync_InvalidDegreePlanId()
            {
#pragma warning disable 618
                await studentDegreePlanService.RegisterAsync(0, "test");
#pragma warning restore 618
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentDegreePlanService_RegisterAsync_InvalidTermId()
            {
#pragma warning disable 618
                await studentDegreePlanService.RegisterAsync(1, "");
#pragma warning restore 618
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentDegreePlanService_RegisterAsync_NullTermId()
            {
#pragma warning disable 618
                await studentDegreePlanService.RegisterAsync(1, null);
#pragma warning restore 618
            }

            [TestMethod]
            public async Task StudentDegreePlanService_RegisterAsync_NoPlannedSections()
            {
#pragma warning disable 618
                var messages = (await studentDegreePlanService.RegisterAsync(1, "2012/SP")).Messages;
#pragma warning restore 618
                Assert.AreEqual(1, messages.Count());
                Assert.AreEqual("No sections selected within this term", messages.ElementAt(0).Message);
            }

            [TestMethod]
            public async Task StudentDegreePlanService_RegisterAsync_Success()
            {
#pragma warning disable 618
                var messages = (await studentDegreePlanService.RegisterAsync(1, "2012/FA")).Messages;
#pragma warning restore 618
                Assert.AreEqual(1, messages.Count());
                Assert.AreEqual("Success", messages.ElementAt(0).Message);
            }
        }

        [TestClass]
        public class StudentDegreePlanService_TestAdvisorPermissions : StudentDegreePlanServiceTestsSetup
        {
            private IEnumerable<Domain.Student.Entities.AcademicCredit> activeAcademicCredits;

            [TestInitialize]
            public async void Initialize()
            {
                SetupInitialize();

                // Set up advisor as the current user.
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();

                // Mock successful Get of a plan from repository
                IEnumerable<DegreePlan> degreePlans = await new TestStudentDegreePlanRepository().GetAsync();

                // Mock get of the catalogs
                var catalogs = await new TestCatalogRepository().GetAsync();
                catalogRepoMock.Setup(x => x.GetAsync()).Returns(Task.FromResult(catalogs));

                // Degree plan for student 0000894
                DegreePlan degreePlan2 = degreePlans.Where(dp => dp.Id == 2).First();
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(2)).Returns(Task.FromResult(degreePlan2));

                // Mock successful add of a plan from repository
                studentDegreePlanRepoMock.Setup(x => x.AddAsync(It.IsAny<DegreePlan>())).Returns(Task.FromResult(degreePlan2));

                // Mock student repository get
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                student.AddAdvisor("0000111");
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));

                activeAcademicCredits = new List<Domain.Student.Entities.AcademicCredit>();

                // Mock academic credit repository get using student ids returning dict with student's academic credits
                var academicCreditDict = new Dictionary<string, List<Domain.Student.Entities.AcademicCredit>>();
                academicCreditRepoMock.Setup(x => x.GetAcademicCreditByStudentIdsAsync(It.IsAny<List<string>>(), false, true, It.IsAny<bool>())).Returns(Task.FromResult<Dictionary<string, List<Domain.Student.Entities.AcademicCredit>>>(academicCreditDict));

                // Mock get of all courses
                var courses = await new TestCourseRepository().GetAsync();
                courseRepoMock.Setup(x => x.GetAsync()).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Course>>(courses));

                // Mock get of all sections for the registration terms
                var regTerms = await new TestTermRepository().GetRegistrationTermsAsync();
                termRepoMock.Setup(x => x.GetRegistrationTermsAsync()).Returns(Task.FromResult(regTerms));
                var allSections = await new TestSectionRepository().GetRegistrationSectionsAsync(regTerms);
                sectionRepoMock.Setup(x => x.GetRegistrationSectionsAsync(It.IsAny<IEnumerable<Domain.Student.Entities.Term>>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(allSections));

                // Mock get of grade repo data
                var grades = await new TestGradeRepository().GetAsync();
                gradeRepoMock.Setup(x => x.GetAsync()).Returns(Task.FromResult(grades));

                // Mock Adapters 
                var datetimeOffsetAdapter = new DateTimeOffsetToDateTimeAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<System.DateTimeOffset, System.DateTime>()).Returns(datetimeOffsetAdapter);

                var degreePlanDtoAdapter = new AutoMapperAdapter<DegreePlan, Dtos.Student.DegreePlans.DegreePlan>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<DegreePlan, Dtos.Student.DegreePlans.DegreePlan>()).Returns(degreePlanDtoAdapter);

                var degreePlanApprovalDtoAdapter = new AutoMapperAdapter<DegreePlanApproval, Dtos.Student.DegreePlans.DegreePlanApproval>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<DegreePlanApproval, Dtos.Student.DegreePlans.DegreePlanApproval>()).Returns(degreePlanApprovalDtoAdapter);

                // Mock get requirements for requisite validation 
                var requirementCodes = new List<string>() { };
                var requirements = (await new TestRequirementRepository().GetAsync(requirementCodes)).Where(r => r.RequirementType.Code == "PRE");
                requirementRepoMock.Setup(x => x.GetAsync(It.IsAny<IEnumerable<string>>())).Returns(Task.FromResult(requirements));

                // Mock student degree plan service
                studentDegreePlanService = new StudentDegreePlanService(
                    adapterRegistry, studentDegreePlanRepo, termRepo, studentRepo, studentProgramRepo, courseRepo, sectionRepo, programRepo,
                    academicCreditRepo, requirementRepo, ruleRepo, progReqRepo, catalogRepo, gradeRepo, academicHistoryService,
                    currentUserFactory, roleRepo, logger, baseConfigurationRepository, studentConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                SetupCleanup();
            }

            [TestMethod]
            public async Task RegisterAsync_AllowedForAdvisorWithAllAccessPermissions()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock degree plan get repository
                DegreePlan degreePlan = new DegreePlan("0000894");
                degreePlan.Id = 1;
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(degreePlan.Id)).Returns(Task.FromResult(degreePlan));

#pragma warning disable 618
                await studentDegreePlanService.RegisterAsync(1, "2012/FA");
#pragma warning restore 618
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task RegisterAsync_ThrowsErrorForAdvisorWithoutAllAccessPermissions()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.UpdateAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock degree plan get repository
                DegreePlan degreePlan = new DegreePlan("0000894");
                degreePlan.Id = 1;
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(degreePlan.Id)).Returns(Task.FromResult(degreePlan));
#pragma warning disable 618
                await studentDegreePlanService.RegisterAsync(1, "2012/FA");
#pragma warning restore 618
            }

            [TestMethod]
            public async Task RegisterSectionsAsync_AllowedForAdvisorWithUpdatePermissions()
            {
                // Mock student repository register
                var message = new Domain.Student.Entities.RegistrationMessage() { SectionId = "1234", Message = "" };
                var messages = new List<Domain.Student.Entities.RegistrationMessage>() { message };
                var response = new Domain.Student.Entities.RegistrationResponse(messages, "123", null);
                studentRepoMock.Setup(x => x.RegisterAsync(It.IsAny<Domain.Student.Entities.RegistrationRequest>())).Returns(Task.FromResult(response));

                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock degree plan get repository
                DegreePlan degreePlan = new DegreePlan("0000894");
                degreePlan.Id = 1;
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(degreePlan.Id)).Returns(Task.FromResult(degreePlan));

#pragma warning disable 618
                await studentDegreePlanService.RegisterSectionsAsync(1, new List<Dtos.Student.SectionRegistration>());
#pragma warning restore 618
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task RegisterSections_ThrowsErrorForAdvisorWithoutUpdatePermissions()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.UpdateAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock degree plan get repository
                DegreePlan degreePlan = new DegreePlan("0000894");
                degreePlan.Id = 1;
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(degreePlan.Id)).Returns(Task.FromResult(degreePlan));
#pragma warning disable 618
                await studentDegreePlanService.RegisterSectionsAsync(1, new List<Dtos.Student.SectionRegistration>());
#pragma warning restore 618
            }

        }


    }

}

