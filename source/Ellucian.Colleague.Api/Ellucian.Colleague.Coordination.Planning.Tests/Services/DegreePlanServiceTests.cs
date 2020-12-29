// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Web.Security;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Coordination.Base.Adapters;
using Ellucian.Colleague.Coordination.Student.Adapters;
using Ellucian.Colleague.Coordination.Planning.Adapters;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Planning.Repositories;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Entities.DegreePlans;
using Ellucian.Colleague.Domain.Planning.Entities;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Coordination.Planning.Services;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Domain.Planning.Tests;
using Ellucian.Colleague.Domain.Planning.Services;

namespace Ellucian.Colleague.Coordination.Planning.Tests.Services
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
    public abstract class DegreePlanServiceTestsSetup : CurrentUserSetup
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
        protected IDegreePlanRepository degreePlanRepo;
        protected Mock<IDegreePlanRepository> degreePlanRepoMock;
        protected IStudentDegreePlanRepository studentDegreePlanRepo;
        protected Mock<IStudentDegreePlanRepository> studentDegreePlanRepoMock;
        protected IPlanningStudentRepository planningStudentRepo;
        protected Mock<IPlanningStudentRepository> planningStudentRepoMock;
        protected IStudentProgramRepository studentProgramRepo;
        protected Mock<IStudentProgramRepository> studentProgramRepoMock;
        protected ISampleDegreePlanRepository currTrackRepo;
        protected Mock<ISampleDegreePlanRepository> currTrackRepoMock;
        protected IPlanningConfigurationRepository configRepo;
        protected Mock<IPlanningConfigurationRepository> configRepoMock;
        protected IAdapterRegistry adapterRegistry;
        protected Mock<IAdapterRegistry> adapterRegistryMock;
        protected IRoleRepository roleRepo;
        protected Mock<IRoleRepository> roleRepoMock;
        protected IGradeRepository gradeRepo;
        protected Mock<IGradeRepository> gradeRepoMock;
        protected IDegreePlanArchiveRepository degreePlanArchiveRepo;
        protected Mock<IDegreePlanArchiveRepository> degreePlanArchiveRepoMock;
        protected IAdvisorRepository advisorRepo;
        protected Mock<IAdvisorRepository> advisorRepoMock;
        protected IConfigurationRepository baseConfigurationRepository;
        protected Mock<IConfigurationRepository> baseConfigurationRepositoryMock;
        protected IStudentConfigurationRepository studentConfigurationRepository;
        protected Mock<IStudentConfigurationRepository> studentConfigurationRepositoryMock;

        protected IAcademicHistoryService academicHistoryService;
        protected Mock<IAcademicHistoryService> academicHistoryServiceMock;

        protected DegreePlanService degreePlanService;
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
            degreePlanRepoMock = new Mock<IDegreePlanRepository>();
            degreePlanRepo = degreePlanRepoMock.Object;

            studentDegreePlanRepoMock = new Mock<IStudentDegreePlanRepository>();
            studentDegreePlanRepo = studentDegreePlanRepoMock.Object;

            studentRepoMock = new Mock<IStudentRepository>();
            studentRepo = studentRepoMock.Object;
            planningStudentRepoMock = new Mock<IPlanningStudentRepository>();
            planningStudentRepo = planningStudentRepoMock.Object;
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
            currTrackRepoMock = new Mock<ISampleDegreePlanRepository>();
            currTrackRepo = currTrackRepoMock.Object;
            configRepoMock = new Mock<IPlanningConfigurationRepository>();
            configRepo = configRepoMock.Object;
            catalogRepoMock = new Mock<ICatalogRepository>();
            catalogRepo = catalogRepoMock.Object;
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            logger = new Mock<ILogger>().Object;
            gradeRepoMock = new Mock<IGradeRepository>();
            gradeRepo = gradeRepoMock.Object;
            academicHistoryServiceMock = new Mock<IAcademicHistoryService>();
            academicHistoryService = academicHistoryServiceMock.Object;
            degreePlanArchiveRepoMock = new Mock<IDegreePlanArchiveRepository>();
            degreePlanArchiveRepo = degreePlanArchiveRepoMock.Object;
            advisorRepoMock = new Mock<IAdvisorRepository>();
            advisorRepo = advisorRepoMock.Object;

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
            degreePlanRepo = null;
            studentDegreePlanRepo = null;
            adapterRegistry = null;
            studentRepo = null;
            planningStudentRepo = null;
            studentProgramRepo = null;
            termRepo = null;
            courseRepo = null;
            sectionRepo = null;
            academicCreditRepo = null;
            requirementRepo = null;
            currentUserFactory = null;
            catalogRepo = null;
            gradeRepo = null;
            degreePlanService = null;
            studentDegreePlanService = null;
            currentUserFactory = null;
            catalogRepo = null;
            gradeRepo = null;
        }
    }

    [TestClass]
    public class DegreePlanServiceTests
    {
        [TestClass]
        public class DegreePlanService_ApplySampleDegreePlan : DegreePlanServiceTestsSetup
        {

            private Dtos.Student.DegreePlans.DegreePlan degreePlanDto;
            private DegreePlanEntityAdapter degreePlanEntityToDtoAdapter;
            private IEnumerable<Domain.Student.Entities.Course> courses;
            private IEnumerable<Domain.Student.Entities.AcademicCredit> emptyAcademicCredits;

            [TestInitialize]
            public async void Initialize()
            {
                SetupInitialize();

                // Mock the get from the student repository
                Domain.Student.Entities.Student student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));

                Domain.Student.Entities.Student student2 = new Domain.Student.Entities.Student("0000896", "Jones", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000896")).Returns(Task.FromResult(student2));

                // Mock getting the student's active academic credits.
                emptyAcademicCredits = new List<Domain.Student.Entities.AcademicCredit>();

                // Mock the get of the planning terms
                IEnumerable<Domain.Student.Entities.Term> allTerms = await new TestTermRepository().GetAsync();
                termRepoMock.Setup(x => x.GetAsync()).ReturnsAsync(allTerms);

                // Mock the get of courses
                courses = await new TestCourseRepository().GetAsync();
                courseRepoMock.Setup(x => x.GetAsync()).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Course>>(courses));

                // Mock the get for the catalogs
                ICollection<Domain.Student.Entities.Requirements.Catalog> allCatalogs = await new TestCatalogRepository().GetAsync();
                catalogRepoMock.Setup(cr => cr.GetAsync()).Returns(Task.FromResult(allCatalogs));

                // Mock the planningConfiguration
                var planningConfig = new PlanningConfiguration() { DefaultCatalogPolicy = CatalogPolicy.StudentCatalogYear, DefaultCurriculumTrack = "TRACK3" };
                configRepoMock.Setup(x => x.GetPlanningConfigurationAsync()).Returns(Task.FromResult(planningConfig));

                // Mock the get of the student programs
                var studentPrograms = await new TestStudentProgramRepository().GetAsync("0000894");
                studentProgramRepoMock.Setup(x => x.GetAsync("0000894")).ReturnsAsync(studentPrograms);

                // Mock successful add of a plan from repository
                DegreePlan degreePlan = new DegreePlan(1, "0000894", 0);
                degreePlan.AddTerm("2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Domain.Student.Entities.DegreePlans.DegreePlan>())).Returns(Task.FromResult(degreePlan));
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<List<string>>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.DegreePlans.DegreePlan>>(new List<Domain.Student.Entities.DegreePlans.DegreePlan>() { degreePlan }));

                // Mock grade repo get
                var grades = await new TestGradeRepository().GetAsync();
                gradeRepoMock.Setup(x => x.GetAsync()).Returns(Task.FromResult(grades));

                // Mock academic credit repository get using student ids returning dict with student's academic credits
                var academicCreditDict = new Dictionary<string, List<Domain.Student.Entities.AcademicCredit>>();
                academicCreditRepoMock.Setup(x => x.GetAcademicCreditByStudentIdsAsync(It.IsAny<List<string>>(), false, true, It.IsAny<bool>())).Returns(Task.FromResult<Dictionary<string, List<Domain.Student.Entities.AcademicCredit>>>(academicCreditDict));

                // Mock dto Adapter 
                var degreePlanPreviewEntityToDtoAdapter = new DegreePlanPreviewEntityAdapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Planning.Entities.DegreePlanPreview, Dtos.Planning.DegreePlanPreview>()).Returns(degreePlanPreviewEntityToDtoAdapter);
                degreePlanEntityToDtoAdapter = new DegreePlanEntityAdapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan>()).Returns(degreePlanEntityToDtoAdapter);

                // Mock entity adapter
                var degreePlanDtoToEntityAdapter = new DegreePlanDtoAdapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Dtos.Student.DegreePlans.DegreePlan, Domain.Student.Entities.DegreePlans.DegreePlan>()).Returns(degreePlanDtoToEntityAdapter);

                // Mock get requirements for requisite validation 
                var requirementCodes = new List<string>() { };
                var requirements = (await new TestRequirementRepository().GetAsync(requirementCodes)).Where(r => r.RequirementType.Code == "PRE");
                requirementRepoMock.Setup(x => x.GetAsync(It.IsAny<IEnumerable<string>>())).Returns(Task.FromResult(requirements));


                // Map degree plan to dto
                degreePlanDto = degreePlanEntityToDtoAdapter.MapToType(degreePlan);

                studentDegreePlanService = new StudentDegreePlanService(
                    adapterRegistry, studentDegreePlanRepo, termRepo, studentRepo, studentProgramRepo, courseRepo, sectionRepo, programRepo,
                    academicCreditRepo, requirementRepo, ruleRepo, progReqRepo, catalogRepo, gradeRepo, academicHistoryService,
                    currentUserFactory, roleRepo, logger, baseConfigurationRepository, studentConfigurationRepository);

                degreePlanService = new DegreePlanService(
                    adapterRegistry, degreePlanRepo, termRepo, studentRepo, planningStudentRepo, studentProgramRepo,
                    courseRepo, sectionRepo, programRepo, academicCreditRepo, requirementRepo, ruleRepo, progReqRepo,
                    currTrackRepo, configRepo, catalogRepo, null, null, gradeRepo, academicHistoryService, studentDegreePlanRepo, studentDegreePlanService,
                    currentUserFactory, roleRepo, logger, baseConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                SetupCleanup();
            }

            [TestMethod]
            public async Task DegreePlanService_ApplySamplePlanAsync_StudentWithNoDegreePlan_ExistingProgram()
            {
                // Arrange-Mock program requirement with curriculum track
                var programCode = "EmptyRequirements";
                var catalogCode = "2012";
                var programReq = await new TestProgramRequirementsRepository().GetAsync(programCode, catalogCode);
                progReqRepoMock.Setup(x => x.GetAsync(programCode, catalogCode)).Returns(Task.FromResult(programReq));
                var currTrack = await new TestSampleDegreePlanRepository().GetAsync("TRACK1");
                currTrackRepoMock.Setup(x => x.GetAsync("TRACK1")).Returns(Task.FromResult(currTrack));

                // Mock a program 
                IEnumerable<string> depts = new List<string>() { "Dept1" };
                IEnumerable<Domain.Student.Entities.Requirements.Program> program =
                    new List<Domain.Student.Entities.Requirements.Program>() { new Domain.Student.Entities.Requirements.Program(programCode, programCode, depts, true, "UG", new CreditFilter(), true) };
                programRepoMock.Setup(pr => pr.GetAsync()).Returns(Task.FromResult(program));



                // Mock finding the sample plan's program as one of the student's current "programs". 
                Domain.Student.Entities.StudentProgram emptyRequirementsStudentProgram = new Domain.Student.Entities.StudentProgram("0000894", "EmptyRequirements", "2012");
                studentProgramRepoMock.Setup(spr => spr.GetAsync(It.IsAny<string>(), "EmptyRequirements")).Returns(Task.FromResult(emptyRequirementsStudentProgram));

                // Apply the curriculum track and set resulting plan as the update response
                var terms = await new TestTermRepository().GetAsync();
                DegreePlan dpApplied = new DegreePlan(1, "0000894", 0);
                SampleDegreePlanService.ApplySample(ref dpApplied, currTrack, terms, emptyAcademicCredits, null, null);

                // Return null since student has no degree plan
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<List<string>>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.DegreePlans.DegreePlan>>(new List<Domain.Student.Entities.DegreePlans.DegreePlan>() { null }));

                // Mock the get from the student repository - student has no plan.
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));

                // Mock returned result of degree plan Add
                studentDegreePlanRepoMock.Setup(x => x.AddAsync(It.IsAny<DegreePlan>())).Returns(Task.FromResult(dpApplied));

                // return applied degree plan
                studentDegreePlanRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Domain.Student.Entities.DegreePlans.DegreePlan>())).Returns(Task.FromResult(dpApplied));

                // Act--Apply sample degree plan
#pragma warning disable 618
                var dp = await degreePlanService.ApplySampleDegreePlanAsync(dpApplied.PersonId, programCode);
#pragma warning restore 618

                // Assert--Do a cursory check of the degree plan returned. 
                // If errors don't occur, also verifies the correct track was used.
                Assert.AreEqual(dpApplied.PersonId, dp.PersonId);
                Assert.AreEqual(dpApplied.TermIds.Count(), dp.Terms.Count());
            }

            [TestMethod]
            public async Task DegreePlanService_ApplySamplePlanAsync_ExistingProgram()
            {
                // Arrange-Mock program requirement with curriculum track
                var programCode = "EmptyRequirements";
                var catalogCode = "2012";
                var programReq = await new TestProgramRequirementsRepository().GetAsync(programCode, catalogCode);
                progReqRepoMock.Setup(x => x.GetAsync(programCode, catalogCode)).Returns(Task.FromResult(programReq));
                var currTrack = await new TestSampleDegreePlanRepository().GetAsync("TRACK1");
                currTrackRepoMock.Setup(x => x.GetAsync("TRACK1")).Returns(Task.FromResult(currTrack));

                // Mock finding the sample plan's program as one of the student's current "programs". 
                Domain.Student.Entities.StudentProgram emptyRequirementsStudentProgram = new Domain.Student.Entities.StudentProgram("0000894", "EmptyRequirements", "2012");
                studentProgramRepoMock.Setup(spr => spr.GetAsync(It.IsAny<string>(), "EmptyRequirements")).Returns(Task.FromResult(emptyRequirementsStudentProgram));

                // Apply the curriculum track and set resulting plan as the update response
                var terms = await new TestTermRepository().GetAsync();
                DegreePlan dpApplied = new DegreePlan(1, "0000894", 0);
                SampleDegreePlanService.ApplySample(ref dpApplied, currTrack, terms, emptyAcademicCredits, null, null);
                studentDegreePlanRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Domain.Student.Entities.DegreePlans.DegreePlan>())).Returns(Task.FromResult(dpApplied));

                // Act--Apply sample degree plan
#pragma warning disable 618
                var dp = await degreePlanService.ApplySampleDegreePlanAsync(dpApplied.PersonId, programCode);
#pragma warning restore 618

                // Assert--Do a cursory check of the degree plan returned. 
                // If errors don't occur, also verifies the correct track was used.
                Assert.AreEqual(dpApplied.PersonId, dp.PersonId);
                Assert.AreEqual(dpApplied.TermIds.Count(), dp.Terms.Count());
            }

            [TestMethod]
            public async Task DegreePlanService_ApplySamplePlanAsync_ApplyDefaultSamplePlan_ExistingProgram()
            {
                // Arrange-Mock program requirement with no curriculum track, so default will be used
                var programCode = "MATH.BS.BLANKTAKEONE";
                var catalogCode = "2012";
                var programReq = await new TestProgramRequirementsRepository().GetAsync(programCode, catalogCode);
                progReqRepoMock.Setup(x => x.GetAsync(programCode, catalogCode)).Returns(Task.FromResult(programReq));

                var currTrack = await new TestSampleDegreePlanRepository().GetAsync("TRACK3");
                currTrackRepoMock.Setup(x => x.GetAsync("TRACK3")).Returns(Task.FromResult(currTrack));

                // Mock finding the sample plan's program as one of the student's current "programs". 
                Domain.Student.Entities.StudentProgram emptyRequirementsStudentProgram = new Domain.Student.Entities.StudentProgram("0000894", "MATH.BS.BLANKTAKEONE", "2012");
                studentProgramRepoMock.Setup(spr => spr.GetAsync(It.IsAny<string>(), "MATH.BS.BLANKTAKEONE")).Returns(Task.FromResult(emptyRequirementsStudentProgram));


                // Apply the curriculum track and set resulting plan as the update response
                var terms = await new TestTermRepository().GetAsync();
                DegreePlan dpApplied = new DegreePlan(1, "0000894", 0);
                SampleDegreePlanService.ApplySample(ref dpApplied, currTrack, terms, emptyAcademicCredits, null, null);
                studentDegreePlanRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Domain.Student.Entities.DegreePlans.DegreePlan>())).Returns(Task.FromResult(dpApplied));

                // Act--Apply sample degree plan
#pragma warning disable 618
                var dp = await degreePlanService.ApplySampleDegreePlanAsync(dpApplied.PersonId, programCode);
#pragma warning restore 618

                // Assert--Do a cursory check of the degree plan returned. 
                // If errors don't occur, also verifies the correct track was used.
                Assert.AreEqual(dpApplied.PersonId, dp.PersonId);
                Assert.AreEqual(dpApplied.TermIds.Count(), dp.Terms.Count());
            }

            [TestMethod]
            public async Task DegreePlanService_ApplySamplePlanAsync_DefaultSampleNoPlan_ExistingProgram()
            {
                // Arrange-Apply sample degree plan for student with no degree plan
                var programCode = "MATH.BS.BLANKTAKEONE";
                var catalogCode = "2012";
                var programReq = await new TestProgramRequirementsRepository().GetAsync(programCode, catalogCode);
                progReqRepoMock.Setup(x => x.GetAsync(programCode, catalogCode)).Returns(Task.FromResult(programReq));

                // Mock a program 
                IEnumerable<string> depts = new List<string>() { "Dept1" };
                IEnumerable<Domain.Student.Entities.Requirements.Program> program =
                    new List<Domain.Student.Entities.Requirements.Program>() { new Domain.Student.Entities.Requirements.Program(programCode, programCode, depts, true, "UG", new CreditFilter(), true) };
                programRepoMock.Setup(pr => pr.GetAsync()).Returns(Task.FromResult(program));

                var currTrack = await new TestSampleDegreePlanRepository().GetAsync("TRACK3");
                currTrackRepoMock.Setup(x => x.GetAsync("TRACK3")).Returns(Task.FromResult(currTrack));

                // Mock finding the sample plan's program as one of the student's current "programs". 
                Domain.Student.Entities.StudentProgram emptyRequirementsStudentProgram = new Domain.Student.Entities.StudentProgram("0000894", "MATH.BS.BLANKTAKEONE", "2012");
                studentProgramRepoMock.Setup(spr => spr.GetAsync(It.IsAny<string>(), "MATH.BS.BLANKTAKEONE")).Returns(Task.FromResult(emptyRequirementsStudentProgram));

                var terms = await new TestTermRepository().GetAsync();
                var dp1 = new DegreePlan(1, "0000894", 0);
                studentDegreePlanRepoMock.Setup(x => x.AddAsync(It.IsAny<Domain.Student.Entities.DegreePlans.DegreePlan>())).ReturnsAsync(dp1);

                // Apply the curriculum track and set resulting plan as the update response
                DegreePlan dpApplied = new DegreePlan(1, "0000894", 0);
                SampleDegreePlanService.ApplySample(ref dpApplied, currTrack, terms, emptyAcademicCredits, null, null);

                studentDegreePlanRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Domain.Student.Entities.DegreePlans.DegreePlan>())).Returns(Task.FromResult(dpApplied));
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<List<string>>())).Throws(new Exception());

                // Mock student degree plan service CreateDegreePlan response
                var studentDegreePlanServiceMock = new Mock<IStudentDegreePlanService>();
                var studentDegreePlanService2 = studentDegreePlanServiceMock.Object;
                studentDegreePlanServiceMock.Setup(svc => svc.CreateDegreePlanAsync(It.IsAny<string>())).Returns(Task.FromResult(degreePlanDto));

                // Act--Apply sample degree plan
#pragma warning disable 618
                var dp = await degreePlanService.ApplySampleDegreePlanAsync(dpApplied.PersonId, programCode);
#pragma warning restore 618

                // Assert--Do a cursory check of the degree plan returned. 
                // If errors don't occur, also verifies the correct track was used.
                Assert.AreEqual(dpApplied.PersonId, dp.PersonId);
                Assert.AreEqual(dpApplied.TermIds.Count(), dp.Terms.Count());
            }

            [TestMethod]
            public async Task DegreePlanService_ApplySamplePlanAsync_StudentWithNoDegreePlan_DifferentProgram()
            {
                // Arrange-Mock program requirement with curriculum track
                var programCode = "EmptyRequirements";
                var catalogCode = "2013";
                var programReq = await new TestProgramRequirementsRepository().GetAsync(programCode, catalogCode);
                progReqRepoMock.Setup(x => x.GetAsync(programCode, catalogCode)).Returns(Task.FromResult(programReq));

                var currTrack = await new TestSampleDegreePlanRepository().GetAsync("TRACK1");
                currTrackRepoMock.Setup(x => x.GetAsync("TRACK1")).Returns(Task.FromResult(currTrack));

                // Mock NOT finding the sample plan's program as one of the student's current "programs". 
                // Mock the get of the sample plan program (with catalog year set to 2013)
                IEnumerable<string> depts = new List<string>() { "Dept1" };
                List<string> catalogYears = new List<string>() { "2012", "2013" };
                Domain.Student.Entities.Requirements.Program emptyReqProgram = new Domain.Student.Entities.Requirements.Program(programCode, "Empty Requirements", depts, true, "UG", new CreditFilter(), false);
                emptyReqProgram.Catalogs = catalogYears;
                programRepoMock.Setup(pr => pr.GetAsync(programCode)).Returns(Task.FromResult(emptyReqProgram));

                IEnumerable<Domain.Student.Entities.Requirements.Program> programs = new List<Domain.Student.Entities.Requirements.Program>();
                programRepoMock.Setup(pr => pr.GetAsync()).Returns(Task.FromResult(programs));

                // Apply the curriculum track and set resulting plan as the update response
                var terms = await new TestTermRepository().GetAsync();
                DegreePlan dpApplied = new DegreePlan(1, "0000894", 0);
                SampleDegreePlanService.ApplySample(ref dpApplied, currTrack, terms, emptyAcademicCredits, null, null);

                // Return null since student has no degree plan
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<List<string>>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.DegreePlans.DegreePlan>>(new List<Domain.Student.Entities.DegreePlans.DegreePlan>() { null }));

                // Mock the get from the student repository - student has no plan.
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));

                // Mock returned result of degree plan Add
                studentDegreePlanRepoMock.Setup(x => x.AddAsync(It.IsAny<DegreePlan>())).Returns(Task.FromResult(dpApplied));

                // return applied degree plan
                studentDegreePlanRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Domain.Student.Entities.DegreePlans.DegreePlan>())).Returns(Task.FromResult(dpApplied));

                // Act--Apply sample degree plan
#pragma warning disable 618
                var dp = await degreePlanService.ApplySampleDegreePlanAsync(dpApplied.PersonId, programCode);
#pragma warning restore 618

                // Assert--Do a cursory check of the degree plan returned. 
                // If errors don't occur, also verifies the correct track was used.
                Assert.AreEqual(dpApplied.PersonId, dp.PersonId);
                Assert.AreEqual(dpApplied.TermIds.Count(), dp.Terms.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task DegreePlanService_ApplySamplePlan_ThrowsExceptionIfCurrentUserNotSelf()
            {
#pragma warning disable 618
                var dp = await degreePlanService.ApplySampleDegreePlanAsync("0000896", "BA.ENGL");
#pragma warning restore 618
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public async Task DegreePlanService_ApplySamplePlanAsync_ThrowsExceptionIfCreatePlanFails()
            {
                // Mock returned result of degree plan Get and Add
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<List<string>>())).Throws(new ArgumentOutOfRangeException());
                studentDegreePlanRepoMock.Setup(x => x.AddAsync(It.IsAny<Domain.Student.Entities.DegreePlans.DegreePlan>())).Throws(new Exception());
#pragma warning disable 618
                var dp = await degreePlanService.ApplySampleDegreePlanAsync("0000894", "BA.ENGL");
#pragma warning restore 618
            }
        }

        [TestClass]
        public class DegreePlanService_PreviewSampleDegreePlan : DegreePlanServiceTestsSetup
        {

            private Dtos.Student.DegreePlans.DegreePlan degreePlanDto;
            private DegreePlanEntityAdapter degreePlanEntityToDtoAdapter;
            private IEnumerable<Domain.Student.Entities.Course> courses;

            [TestInitialize]
            public async void Initialize()
            {
                SetupInitialize();

                // Mock the get from the student repository
                Domain.Student.Entities.Student student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));

                // Mock the get of the planning terms
                IEnumerable<Domain.Student.Entities.Term> allTerms = await new TestTermRepository().GetAsync();
                termRepoMock.Setup(x => x.GetAsync()).ReturnsAsync(allTerms);

                // Mock the get of courses
                courses = await new TestCourseRepository().GetAsync();
                courseRepoMock.Setup(x => x.GetAsync()).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Course>>(courses));

                // Mock the get for the catalogs
                ICollection<Domain.Student.Entities.Requirements.Catalog> allCatalogs = await new TestCatalogRepository().GetAsync();
                catalogRepoMock.Setup(cr => cr.GetAsync()).Returns(Task.FromResult(allCatalogs));

                // Mock the planningConfiguration
                var planningConfig = new PlanningConfiguration() { DefaultCatalogPolicy = CatalogPolicy.StudentCatalogYear, DefaultCurriculumTrack = "TRACK3" };
                configRepoMock.Setup(x => x.GetPlanningConfigurationAsync()).Returns(Task.FromResult(planningConfig));

                // Mock the get of the student programs
                var studentPrograms = await new TestStudentProgramRepository().GetAsync("0000894");
                studentProgramRepoMock.Setup(x => x.GetAsync("0000894")).ReturnsAsync(studentPrograms);

                // Mock successful get of a plan from repository
                DegreePlan degreePlan = new DegreePlan(1, "0000894", 0);
                degreePlan.AddTerm("2012/FA");
                degreePlan.AddTerm("2013/SP");
                degreePlan.AddTerm("2013/FA");
                degreePlan.AddTerm("2014/SP");
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(1)).Returns(Task.FromResult(degreePlan));

                // Mock get of grade data
                var grades = await new TestGradeRepository().GetAsync();
                gradeRepoMock.Setup(x => x.GetAsync()).Returns(Task.FromResult(grades));

                // Mock DateTimeOffset->DateTime adapter to verify backward compatibility
                var datetimeOffsetAdapter = new DateTimeOffsetToDateTimeAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<System.DateTimeOffset, System.DateTime>()).Returns(datetimeOffsetAdapter);

                // Mock dto Adapters
                var degreePlanPreviewEntityToDtoAdapter = new DegreePlanPreviewEntityAdapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Planning.Entities.DegreePlanPreview, Dtos.Planning.DegreePlanPreview>()).Returns(degreePlanPreviewEntityToDtoAdapter);

                degreePlanEntityToDtoAdapter = new DegreePlanEntityAdapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan>()).Returns(degreePlanEntityToDtoAdapter);

                // Mock entity adapter
                var degreePlanDtoToEntityAdapter = new DegreePlanDtoAdapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Dtos.Student.DegreePlans.DegreePlan, Domain.Student.Entities.DegreePlans.DegreePlan>()).Returns(degreePlanDtoToEntityAdapter);

                // Map degree plan to dto
                degreePlanDto = degreePlanEntityToDtoAdapter.MapToType(degreePlan);

                // Mock degree plan service
                degreePlanService = new DegreePlanService(
                    adapterRegistry, degreePlanRepo, termRepo, studentRepo, planningStudentRepo, studentProgramRepo,
                    courseRepo, sectionRepo, programRepo, academicCreditRepo, requirementRepo, ruleRepo, progReqRepo,
                    currTrackRepo, configRepo, catalogRepo, null, null, gradeRepo, academicHistoryService, studentDegreePlanRepo, studentDegreePlanService,
                    currentUserFactory, roleRepo, logger, baseConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                SetupCleanup();
            }

            [TestMethod]
            public async Task PreviewSamplePlanASync_UsingDefaultSamplePlan()
            {
                Domain.Student.Entities.StudentProgram studentProgram = await new TestStudentProgramRepository().GetAsync("0000894", "MATH.BA");
                studentProgramRepoMock.Setup(x => x.GetAsync("0000894", "MATH.BA")).Returns(Task.FromResult(studentProgram));

                var programCode = "MATH.BA";
                var catalogCode = "2013";
                var programReq = await new TestProgramRequirementsRepository().GetAsync(programCode, catalogCode);
                progReqRepoMock.Setup(x => x.GetAsync(programCode, catalogCode)).Returns(Task.FromResult(programReq));

                var currTrack = await new TestSampleDegreePlanRepository().GetAsync("TRACK3");
                currTrackRepoMock.Setup(x => x.GetAsync("TRACK3")).Returns(Task.FromResult(currTrack));

                var dpp = await degreePlanService.PreviewSampleDegreePlanAsync(1, programCode);
                // Verify that every term and course in the preview is also in the merged degree plan
                foreach (var term in dpp.Preview.Terms)
                {
                    var mergedTerm = dpp.MergedDegreePlan.Terms.Where(t => t.TermId == term.TermId).FirstOrDefault();
                    Assert.IsNotNull(mergedTerm);
                    foreach (var courseId in term.GetPlannedCourseIds())
                    {
                        Assert.IsTrue(mergedTerm.GetPlannedCourseIds().Contains(courseId));
                    }
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PreviewSamplePlanAsync_ZeroDegreePlanId()
            {
                var programCode = "EmptyRequirements";
                var dp = await degreePlanService.PreviewSampleDegreePlanAsync(0, programCode);
            }

            [ExpectedException(typeof(ArgumentNullException))]
            [TestMethod]
            public async Task PreviewSamplePlanAsync_NullProgramCode()
            {
                var dp = await degreePlanService.PreviewSampleDegreePlanAsync(1, null);
            }

            [ExpectedException(typeof(ArgumentNullException))]
            [TestMethod]
            public async Task PreviewSamplePlanAsync_EmptyProgramCode()
            {
                var dp = await degreePlanService.PreviewSampleDegreePlanAsync(1, "");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task PreviewSamplePlanAsync_UnableToGetPlan()
            {
                var programCode = "EmptyRequirements";
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(2)).Throws(new ArgumentException("Degree plan Not Found"));

                var dp = await degreePlanService.PreviewSampleDegreePlanAsync(2, programCode);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task PreviewSamplePlanAsync_NotAuthorized()
            {
                // Mock the get from the student repository
                Domain.Student.Entities.Student student = new Domain.Student.Entities.Student("0000899", "Jones", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000899")).Returns(Task.FromResult(student));

                var programCode = "MATH.BS.BLANKTAKEONE";

                DegreePlan dpApplied3 = new DegreePlan(3, "0000899", 0);
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(3)).Returns(Task.FromResult(dpApplied3));

                var dp = await degreePlanService.PreviewSampleDegreePlanAsync(3, programCode);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task PreviewSamplePlan_StudentHasNoPrograms()
            {
                var programCode = "MATH.BS.BLANKTAKEONE";

                IEnumerable<Domain.Student.Entities.StudentProgram> noPrograms = new List<Domain.Student.Entities.StudentProgram>();
                studentProgramRepoMock.Setup(x => x.GetAsync("0000894")).ReturnsAsync(noPrograms);

                DegreePlan dpApplied1 = new DegreePlan(1, "0000894", 0);
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(1)).Returns(Task.FromResult(dpApplied1));

                var dp = await degreePlanService.PreviewSampleDegreePlanAsync(1, programCode);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task PreviewSamplePlan_NoSamplePlanForProgramCode()
            {
                var programCode = "MATH.BA";
                var catalogCode = "2013";
                var programReq = await new TestProgramRequirementsRepository().GetAsync(programCode, catalogCode);
                programReq.CurriculumTrackCode = null;
                progReqRepoMock.Setup(x => x.GetAsync(programCode, catalogCode)).Returns(Task.FromResult(programReq));
                // Mock the planningConfiguration to return no default.
                var planningConfig = new PlanningConfiguration() { DefaultCatalogPolicy = CatalogPolicy.StudentCatalogYear, DefaultCurriculumTrack = "" };
                configRepoMock.Setup(x => x.GetPlanningConfigurationAsync()).Returns(Task.FromResult(planningConfig));

                DegreePlan dpApplied = new DegreePlan(1, "0000894", 0);
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(1)).Returns(Task.FromResult(dpApplied));

                var dp = await degreePlanService.PreviewSampleDegreePlanAsync(1, programCode);
            }
        }

        [TestClass]
        public class DegreePlanService_PreviewSampleDegreePlan2 : DegreePlanServiceTestsSetup
        {
            private Dtos.Student.DegreePlans.DegreePlan2 degreePlanDto;
            private IEnumerable<Domain.Student.Entities.Course> courses;

            [TestInitialize]
            public async void Initialize()
            {
                SetupInitialize();
                // Mock the get from the student repository
                Domain.Student.Entities.Student student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));

                // Mock the get of the planning terms
                IEnumerable<Domain.Student.Entities.Term> allTerms = await new TestTermRepository().GetAsync();
                termRepoMock.Setup(x => x.GetAsync()).ReturnsAsync(allTerms);

                // Mock the get of courses
                courses = await new TestCourseRepository().GetAsync();
                courseRepoMock.Setup(x => x.GetAsync()).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Course>>(courses));

                // Mock the get for the catalogs
                ICollection<Domain.Student.Entities.Requirements.Catalog> allCatalogs = await new TestCatalogRepository().GetAsync();
                catalogRepoMock.Setup(cr => cr.GetAsync()).Returns(Task.FromResult(allCatalogs));

                // Mock the planningConfiguration
                var planningConfig = new PlanningConfiguration() { DefaultCatalogPolicy = CatalogPolicy.StudentCatalogYear, DefaultCurriculumTrack = "TRACK3" };
                configRepoMock.Setup(x => x.GetPlanningConfigurationAsync()).Returns(Task.FromResult(planningConfig));

                // Mock the get of the student programs
                var studentPrograms = await new TestStudentProgramRepository().GetAsync("0000894");
                studentProgramRepoMock.Setup(x => x.GetAsync("0000894")).ReturnsAsync(studentPrograms);

                // Mock successful get of a plan from repository
                DegreePlan degreePlan = new DegreePlan(1, "0000894", 0);
                degreePlan.AddTerm("2012/FA");
                degreePlan.AddTerm("2013/SP");
                degreePlan.AddTerm("2013/FA");
                degreePlan.AddTerm("2014/SP");
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(1)).Returns(Task.FromResult(degreePlan));

                // Mock grade data get
                var grades = await new TestGradeRepository().GetAsync();
                gradeRepoMock.Setup(x => x.GetAsync()).Returns(Task.FromResult(grades));

                // Mock DateTimeOffset->DateTime adapter to verify backward compatibility
                var datetimeOffsetAdapter = new DateTimeOffsetToDateTimeAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<System.DateTimeOffset, System.DateTime>()).Returns(datetimeOffsetAdapter);

                // Mock dto Adapter 
                var degreePlanPreviewEntityToDtoAdapter = new DegreePlanPreviewEntity2Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Planning.Entities.DegreePlanPreview, Dtos.Planning.DegreePlanPreview2>()).Returns(degreePlanPreviewEntityToDtoAdapter);
                var degreePlanEntityToDtoAdapter = new DegreePlanEntity2Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan2>()).Returns(degreePlanEntityToDtoAdapter);

                // Mock entity adapter
                var degreePlanDtoToEntityAdapter = new DegreePlanDto2Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Dtos.Student.DegreePlans.DegreePlan2, Domain.Student.Entities.DegreePlans.DegreePlan>()).Returns(degreePlanDtoToEntityAdapter);

                // Map degree plan to dto
                degreePlanDto = degreePlanEntityToDtoAdapter.MapToType(degreePlan);

                // Mock degree plan service
                degreePlanService = new DegreePlanService(
                    adapterRegistry, degreePlanRepo, termRepo, studentRepo, planningStudentRepo, studentProgramRepo,
                    courseRepo, sectionRepo, programRepo, academicCreditRepo, requirementRepo, ruleRepo, progReqRepo,
                    currTrackRepo, configRepo, catalogRepo, null, null, gradeRepo, academicHistoryService, studentDegreePlanRepo, studentDegreePlanService,
                    currentUserFactory, roleRepo, logger, baseConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                SetupCleanup();
            }

            [TestMethod]
            public async Task PreviewSamplePlanAsync_UsingDefaultSamplePlan2()
            {
                Domain.Student.Entities.StudentProgram studentProgram = await new TestStudentProgramRepository().GetAsync("0000894", "MATH.BA");
                studentProgramRepoMock.Setup(x => x.GetAsync("0000894", "MATH.BA")).Returns(Task.FromResult(studentProgram));

                var programCode = "MATH.BA";
                var catalogCode = "2013";
                var programReq = await new TestProgramRequirementsRepository().GetAsync(programCode, catalogCode);
                progReqRepoMock.Setup(x => x.GetAsync(programCode, catalogCode)).Returns(Task.FromResult(programReq));

                var currTrack = await new TestSampleDegreePlanRepository().GetAsync("TRACK3");
                currTrackRepoMock.Setup(x => x.GetAsync("TRACK3")).Returns(Task.FromResult(currTrack));

#pragma warning disable 618
                var dpp = await degreePlanService.PreviewSampleDegreePlan2Async(1, programCode, string.Empty);
#pragma warning restore 618

                // Verify that every term and course on the preview is also on the merged plan in the same term
                foreach (var term in dpp.Preview.Terms)
                {
                    var mergedTerm = dpp.MergedDegreePlan.Terms.Where(t => t.TermId == term.TermId).FirstOrDefault();
                    Assert.IsNotNull(mergedTerm);
                    foreach (var courseId in term.GetPlannedCourseIds())
                    {
                        Assert.IsTrue(mergedTerm.GetPlannedCourseIds().Contains(courseId));
                    }
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PreviewSamplePlanAsync_ZeroDegreePlanId()
            {
                var programCode = "EmptyRequirements";
#pragma warning disable 618
                var dp = await degreePlanService.PreviewSampleDegreePlan2Async(0, programCode, string.Empty);
#pragma warning restore 618
            }

            [ExpectedException(typeof(ArgumentNullException))]
            [TestMethod]
            public async Task PreviewSamplePlanAsync_NullProgramCode()
            {
#pragma warning disable 618
                var dp = await degreePlanService.PreviewSampleDegreePlan2Async(1, null, string.Empty);
#pragma warning restore 618
            }

            [ExpectedException(typeof(ArgumentNullException))]
            [TestMethod]
            public async Task PreviewSamplePlanAsync_EmptyProgramCode()
            {
#pragma warning disable 618
                var dp = await degreePlanService.PreviewSampleDegreePlan2Async(1, "", string.Empty);
#pragma warning restore 618
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task PreviewSamplePlanAsync_UnableToGetPlan()
            {
                var programCode = "EmptyRequirements";
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(2)).Throws(new ArgumentException("Degree plan Not Found"));

#pragma warning disable 618
                var dp = await degreePlanService.PreviewSampleDegreePlan2Async(2, programCode, string.Empty);
#pragma warning restore 618
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task PreviewSamplePlanAsync_NotAuthorized()
            {
                var programCode = "MATH.BS.BLANKTAKEONE";

                DegreePlan dpApplied3 = new DegreePlan(3, "0000899", 0);
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(3)).Returns(Task.FromResult(dpApplied3));

#pragma warning disable 618
                var dp = await degreePlanService.PreviewSampleDegreePlan2Async(3, programCode, string.Empty);
#pragma warning restore 618
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task PreviewSamplePlanAsync_StudentHasNoPrograms()
            {
                var programCode = "MATH.BS.BLANKTAKEONE";

                IEnumerable<Domain.Student.Entities.StudentProgram> noPrograms = new List<Domain.Student.Entities.StudentProgram>();
                studentProgramRepoMock.Setup(x => x.GetAsync("0000894")).ReturnsAsync(noPrograms);

                DegreePlan dpApplied1 = new DegreePlan(1, "0000894", 0);
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(1)).Returns(Task.FromResult(dpApplied1));

#pragma warning disable 618
                var dp = await degreePlanService.PreviewSampleDegreePlan2Async(1, programCode, string.Empty);
#pragma warning restore 618
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task PreviewSamplePlanAsync_NoSamplePlanForProgramCode()
            {
                var programCode = "MATH.BA";
                var catalogCode = "2013";
                var programReq = await new TestProgramRequirementsRepository().GetAsync(programCode, catalogCode);
                programReq.CurriculumTrackCode = null;
                progReqRepoMock.Setup(x => x.GetAsync(programCode, catalogCode)).Returns(Task.FromResult(programReq));
                // Mock the planningConfiguration to return no default.
                var planningConfig = new PlanningConfiguration() { DefaultCatalogPolicy = CatalogPolicy.StudentCatalogYear, DefaultCurriculumTrack = "" };
                configRepoMock.Setup(x => x.GetPlanningConfigurationAsync()).Returns(Task.FromResult(planningConfig));

                DegreePlan dpApplied = new DegreePlan(1, "0000894", 0);
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(1)).Returns(Task.FromResult(dpApplied));

#pragma warning disable 618
                var dp = await degreePlanService.PreviewSampleDegreePlan2Async(1, programCode, string.Empty);
#pragma warning restore 618
            }
        }

        [TestClass]
        public class DegreePlanService_PreviewSampleDegreePlan3 : DegreePlanServiceTestsSetup
        {
            private Dtos.Student.DegreePlans.DegreePlan3 degreePlanDto;
            private IEnumerable<Domain.Student.Entities.Course> courses;
            private Domain.Student.Entities.AcademicHistory historyEntity;
            private Dtos.Student.AcademicHistory2 historyDto;

            [TestInitialize]
            public async void Initialize()
            {
                SetupInitialize();

                // Mock the get from the student repository
                Domain.Student.Entities.Student student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));

                // Mock the get of the planning terms
                IEnumerable<Domain.Student.Entities.Term> allTerms = await new TestTermRepository().GetAsync();
                termRepoMock.Setup(x => x.GetAsync()).ReturnsAsync(allTerms);

                // Mock the get of courses
                courses = await new TestCourseRepository().GetAsync();
                courseRepoMock.Setup(x => x.GetAsync()).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Course>>(courses));

                // Mock the get for the catalogs
                ICollection<Domain.Student.Entities.Requirements.Catalog> allCatalogs = await new TestCatalogRepository().GetAsync();
                catalogRepoMock.Setup(cr => cr.GetAsync()).Returns(Task.FromResult(allCatalogs));

                // Mock the planningConfiguration
                var planningConfig = new PlanningConfiguration() { DefaultCatalogPolicy = CatalogPolicy.StudentCatalogYear, DefaultCurriculumTrack = "TRACK3" };
                configRepoMock.Setup(x => x.GetPlanningConfigurationAsync()).Returns(Task.FromResult(planningConfig));

                // Mock the get of the student programs
                var studentPrograms = await new TestStudentProgramRepository().GetAsync("0000894");
                studentProgramRepoMock.Setup(x => x.GetAsync("0000894")).ReturnsAsync(studentPrograms);

                // Mock successful get of a plan from repository
                DegreePlan degreePlan = new DegreePlan(1, "0000894", 0);
                degreePlan.AddTerm("2012/FA");
                degreePlan.AddTerm("2013/SP");
                degreePlan.AddTerm("2013/FA");
                degreePlan.AddTerm("2014/SP");
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(1)).Returns(Task.FromResult(degreePlan));

                // Mock grade data get
                var grades = await new TestGradeRepository().GetAsync();
                gradeRepoMock.Setup(x => x.GetAsync()).Returns(Task.FromResult(grades));

                // Mock dto Adapter 
                var datetimeOffsetAdapter = new DateTimeOffsetToDateTimeAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<System.DateTimeOffset, System.DateTime>()).Returns(datetimeOffsetAdapter);

                var degreePlanPreview3EntityToDtoAdapter = new DegreePlanPreviewEntity3Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Planning.Entities.DegreePlanPreview, Dtos.Planning.DegreePlanPreview3>()).Returns(degreePlanPreview3EntityToDtoAdapter);
                var degreePlan3EntityToDtoAdapter = new DegreePlanEntity3Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan3>()).Returns(degreePlan3EntityToDtoAdapter);

                // Mock entity adapter
                var degreePlan3DtoToEntityAdapter = new DegreePlanDto3Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Dtos.Student.DegreePlans.DegreePlan3, Domain.Student.Entities.DegreePlans.DegreePlan>()).Returns(degreePlan3DtoToEntityAdapter);


                // Map degree plan to dto
                degreePlanDto = degreePlan3EntityToDtoAdapter.MapToType(degreePlan);

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

                degreePlanService = new DegreePlanService(
                    adapterRegistry, degreePlanRepo, termRepo, studentRepo, planningStudentRepo, studentProgramRepo,
                    courseRepo, sectionRepo, programRepo, academicCreditRepo, requirementRepo, ruleRepo, progReqRepo,
                    currTrackRepo, configRepo, catalogRepo, null, null, gradeRepo, academicHistoryService, studentDegreePlanRepo, studentDegreePlanService,
                    currentUserFactory, roleRepo, logger, baseConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                SetupCleanup();
            }

            [TestMethod]
            public async Task PreviewSamplePlan3Async_UsingDefaultSamplePlan()
            {
                Domain.Student.Entities.StudentProgram studentProgram = await new TestStudentProgramRepository().GetAsync("0000894", "MATH.BA");
                studentProgramRepoMock.Setup(x => x.GetAsync("0000894", "MATH.BA")).Returns(Task.FromResult(studentProgram));

                // Mock academic credit repository get using student ids returning empty dict for this test
                var academicCreditDict = new Dictionary<string, List<Domain.Student.Entities.AcademicCredit>>();
                academicCreditRepoMock.Setup(x => x.GetAcademicCreditByStudentIdsAsync(It.IsAny<List<string>>(), false, true, It.IsAny<bool>())).Returns(Task.FromResult<Dictionary<string, List<Domain.Student.Entities.AcademicCredit>>>(academicCreditDict));

                var programCode = "MATH.BA";
                var catalogCode = "2013";
                var programReq = await new TestProgramRequirementsRepository().GetAsync(programCode, catalogCode);
                progReqRepoMock.Setup(x => x.GetAsync(programCode, catalogCode)).Returns(Task.FromResult(programReq));

                var currTrack = await new TestSampleDegreePlanRepository().GetAsync("TRACK3");
                currTrackRepoMock.Setup(x => x.GetAsync("TRACK3")).Returns(Task.FromResult(currTrack));

                var dpp = await degreePlanService.PreviewSampleDegreePlan3Async(1, programCode, string.Empty);
                // Verify that every term and course on the preview is also on the merged plan in the same term
                foreach (var term in dpp.Preview.Terms)
                {
                    var mergedTerm = dpp.MergedDegreePlan.Terms.Where(t => t.TermId == term.TermId).FirstOrDefault();
                    Assert.IsNotNull(mergedTerm);
                    foreach (var courseId in term.GetPlannedCourseIds())
                    {
                        Assert.IsTrue(mergedTerm.GetPlannedCourseIds().Contains(courseId));
                    }
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PreviewSamplePlan3Async_ZeroDegreePlanId()
            {
                var programCode = "EmptyRequirements";
                var dp = await degreePlanService.PreviewSampleDegreePlan3Async(0, programCode, string.Empty);
            }

            [ExpectedException(typeof(ArgumentNullException))]
            [TestMethod]
            public async Task PreviewSamplePlan3Async_NullProgramCode()
            {
                var dp = await degreePlanService.PreviewSampleDegreePlan3Async(1, null, string.Empty);
            }

            [ExpectedException(typeof(ArgumentNullException))]
            [TestMethod]
            public async Task PreviewSamplePlan3Async_EmptyProgramCode()
            {
                var dp = await degreePlanService.PreviewSampleDegreePlan3Async(1, "", string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task PreviewSamplePlan3Async_UnableToGetPlan()
            {
                var programCode = "EmptyRequirements";
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(2)).Throws(new ArgumentException("Degree plan Not Found"));

                var dp = await degreePlanService.PreviewSampleDegreePlan3Async(2, programCode, string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task PreviewSamplePlan3Async_NotAuthorized()
            {
                var programCode = "MATH.BS.BLANKTAKEONE";

                DegreePlan dpApplied3 = new DegreePlan(3, "0000899", 0);
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(3)).Returns(Task.FromResult(dpApplied3));

                var dp = await degreePlanService.PreviewSampleDegreePlan3Async(3, programCode, string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task PreviewSamplePlan3Async_StudentHasNoPrograms()
            {
                var programCode = "MATH.BS.BLANKTAKEONE";

                IEnumerable<Domain.Student.Entities.StudentProgram> noPrograms = new List<Domain.Student.Entities.StudentProgram>();
                studentProgramRepoMock.Setup(x => x.GetAsync("0000894")).ReturnsAsync(noPrograms);

                DegreePlan dpApplied1 = new DegreePlan(1, "0000894", 0);
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(1)).Returns(Task.FromResult(dpApplied1));

                var dp = await degreePlanService.PreviewSampleDegreePlan3Async(1, programCode, string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task PreviewSamplePlan3Async_NoSamplePlanForProgramCode()
            {
                var programCode = "MATH.BA";
                var catalogCode = "2013";
                var programReq = await new TestProgramRequirementsRepository().GetAsync(programCode, catalogCode);
                programReq.CurriculumTrackCode = null;
                progReqRepoMock.Setup(x => x.GetAsync(programCode, catalogCode)).Returns(Task.FromResult(programReq));
                // Mock the planningConfiguration to return no default.
                var planningConfig = new PlanningConfiguration() { DefaultCatalogPolicy = CatalogPolicy.StudentCatalogYear, DefaultCurriculumTrack = "" };
                configRepoMock.Setup(x => x.GetPlanningConfigurationAsync()).Returns(Task.FromResult(planningConfig));

                DegreePlan dpApplied = new DegreePlan(1, "0000894", 0);
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(1)).Returns(Task.FromResult(dpApplied));

                var dp = await degreePlanService.PreviewSampleDegreePlan3Async(1, programCode, string.Empty);
            }
        }

        [TestClass]
        public class DegreePlanService_ArchiveDegreePlan : DegreePlanServiceTestsSetup
        {
            private DegreePlanEntity2Adapter degreePlanEntityToDtoAdapter;

            IEnumerable<DegreePlan> degreePlans;

            [TestInitialize]
            public async void Initialize()
            {
                SetupInitialize();

                // Mock successful Get of a plan from repository
                degreePlans = await new TestStudentDegreePlanRepository().GetAsync();

                // Map degree plan to dto
                degreePlanEntityToDtoAdapter = new DegreePlanEntity2Adapter(adapterRegistryMock.Object, logger);

                // Mock Adapter 
                var degreePlanDtoAdapter = new AutoMapperAdapter<DegreePlan, Dtos.Student.DegreePlans.DegreePlan2>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<DegreePlan, Dtos.Student.DegreePlans.DegreePlan2>()).Returns(degreePlanDtoAdapter);

                var datetimeOffsetAdapter = new DateTimeOffsetToDateTimeAdapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<System.DateTimeOffset, System.DateTime>()).Returns(datetimeOffsetAdapter);

                var degreePlanApprovalDtoAdapter = new AutoMapperAdapter<DegreePlanApproval, Dtos.Student.DegreePlans.DegreePlanApproval>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<DegreePlanApproval, Dtos.Student.DegreePlans.DegreePlanApproval>()).Returns(degreePlanApprovalDtoAdapter);
                var plannedCourseWarningEntityAdapter = new AutoMapperAdapter<PlannedCourseWarning, Dtos.Student.DegreePlans.PlannedCourseWarning>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<PlannedCourseWarning, Dtos.Student.DegreePlans.PlannedCourseWarning>()).Returns(plannedCourseWarningEntityAdapter);
                var requisiteEntityAdapter = new AutoMapperAdapter<Domain.Student.Entities.Requisite, Dtos.Student.Requisite>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.Requisite, Dtos.Student.Requisite>()).Returns(requisiteEntityAdapter);
                var sectionRequisiteEntityAdapter = new AutoMapperAdapter<Domain.Student.Entities.SectionRequisite, Dtos.Student.SectionRequisite>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.SectionRequisite, Dtos.Student.SectionRequisite>()).Returns(sectionRequisiteEntityAdapter);
                var degreePlanDtoToEntityAdapter = new DegreePlanDto2Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Dtos.Student.DegreePlans.DegreePlan2, Domain.Student.Entities.DegreePlans.DegreePlan>()).Returns(degreePlanDtoToEntityAdapter);

                var degreePlanArchiveAdapter = new AutoMapperAdapter<Domain.Planning.Entities.DegreePlanArchive, Dtos.Planning.DegreePlanArchive>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Planning.Entities.DegreePlanArchive, Dtos.Planning.DegreePlanArchive>()).Returns(degreePlanArchiveAdapter);

                var studentPrograms = await new TestStudentProgramRepository().GetAsync("0000896");
                studentProgramRepoMock.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(studentPrograms);

                // Mock the get of courses
                var courses = await new TestCourseRepository().GetAsync();
                courseRepoMock.Setup(x => x.GetAsync()).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Course>>(courses));

                // Mock the get of registration Terms
                var regTerms = await new TestTermRepository().GetRegistrationTermsAsync();
                termRepoMock.Setup(x => x.GetRegistrationTermsAsync()).Returns(Task.FromResult(regTerms));

                var allSections = await new TestSectionRepository().GetRegistrationSectionsAsync(regTerms);
                sectionRepoMock.Setup(x => x.GetCachedSectionsAsync(It.IsAny<IEnumerable<string>>(), false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(allSections));

                DegreePlanArchive degreePlanArchive = new DegreePlanArchive(5, 2, "0000894", 1);
                degreePlanArchiveRepoMock.Setup(x => x.AddAsync(It.IsAny<DegreePlanArchive>())).Returns(Task.FromResult(degreePlanArchive));

                // Mock the get of academic credits 
                var academicCredits = await new TestAcademicCreditRepository().GetAsync();
                academicCreditRepoMock.Setup(x => x.GetAsync(It.IsAny<ICollection<string>>(), It.IsAny<bool>(), true, It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.AcademicCredit>>(academicCredits));

                // Mock the get of student, because we need to have a student object to access the list of academic credits
                var student = new TestStudentRepository().GetAsync("00004001");
                studentRepoMock.Setup(x => x.GetAsync(It.IsAny<string>())).Returns(student);

                // Mock grade data get
                var grades = await new TestGradeRepository().GetAsync();
                gradeRepoMock.Setup(x => x.GetAsync()).Returns(Task.FromResult(grades));

                // Mock student degree plan service
                studentDegreePlanService = new StudentDegreePlanService(
                    adapterRegistry, studentDegreePlanRepo, termRepo, studentRepo, studentProgramRepo, courseRepo, sectionRepo, programRepo,
                    academicCreditRepo, requirementRepo, ruleRepo, progReqRepo, catalogRepo, gradeRepo, academicHistoryService,
                    currentUserFactory, roleRepo, logger, baseConfigurationRepository, studentConfigurationRepository);

                degreePlanService = new DegreePlanService(
                    adapterRegistry, degreePlanRepo, termRepo, studentRepo, planningStudentRepo, studentProgramRepo,
                    courseRepo, sectionRepo, programRepo, academicCreditRepo, requirementRepo, ruleRepo, progReqRepo,
                    currTrackRepo, configRepo, catalogRepo, degreePlanArchiveRepo, null, gradeRepo,
                    academicHistoryService, studentDegreePlanRepo, studentDegreePlanService,
                    currentUserFactory, roleRepo, logger, baseConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                SetupCleanup();
            }

            [TestMethod]
            public async Task TestArchiveSuccess_Student()
            {
                var degreePlan = degreePlans.Where(d => d.Id == 2).FirstOrDefault();
                var degreePlanDto = degreePlanEntityToDtoAdapter.MapToType(degreePlan);

#pragma warning disable 618
                var archive = await degreePlanService.ArchiveDegreePlanAsync(degreePlanDto);
#pragma warning restore 618

                var expectedArchive = new DegreePlanArchive(archive.Id, degreePlan.Id, currentUserFactory.CurrentUser.PersonId, degreePlan.Version);

                // Can't assert the expectArchive and archive equality directly because they are timestamped at creation
                // So, assert the attributes we know
                Assert.AreEqual(expectedArchive.DegreePlanId, archive.DegreePlanId);
                Assert.AreEqual(expectedArchive.StudentId, archive.StudentId);
                Assert.AreEqual(expectedArchive.ReviewedBy, archive.ReviewedBy);
            }

            [TestMethod]
            public async Task TestArchiveSuccess_Advisor()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.UpdateAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();

                studentDegreePlanService = new StudentDegreePlanService(
                    adapterRegistry, studentDegreePlanRepo, termRepo, studentRepo, studentProgramRepo, courseRepo, sectionRepo, programRepo,
                    academicCreditRepo, requirementRepo, ruleRepo, progReqRepo, catalogRepo, gradeRepo, academicHistoryService,
                    currentUserFactory, roleRepo, logger, baseConfigurationRepository, studentConfigurationRepository);

                degreePlanService = new DegreePlanService(
                    adapterRegistry, degreePlanRepo, termRepo, studentRepo, planningStudentRepo, studentProgramRepo,
                    courseRepo, sectionRepo, programRepo, academicCreditRepo, requirementRepo, ruleRepo, progReqRepo,
                    currTrackRepo, configRepo, catalogRepo, degreePlanArchiveRepo, null, gradeRepo,
                    academicHistoryService, studentDegreePlanRepo, studentDegreePlanService,
                    currentUserFactory, roleRepo, logger, baseConfigurationRepository);

                var degreePlan = degreePlans.Where(d => d.Id == 2).FirstOrDefault();
                var degreePlanDto = degreePlanEntityToDtoAdapter.MapToType(degreePlan);

#pragma warning disable 618
                var archive = await degreePlanService.ArchiveDegreePlanAsync(degreePlanDto);
#pragma warning restore 618

                var expectedArchive = new DegreePlanArchive(archive.Id, degreePlan.Id, degreePlan.PersonId, degreePlan.Version);

                // Can't assert the expectArchive and archive equality directly because they are timestamped at creation
                // So, assert the attributes we know
                Assert.AreEqual(expectedArchive.DegreePlanId, archive.DegreePlanId);
                Assert.AreEqual(expectedArchive.StudentId, archive.StudentId);
                Assert.AreEqual(expectedArchive.ReviewedBy, archive.ReviewedBy);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task ArchiveFail_AdvisorPermissions()
            {
                // Set up advisor as the current user.
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();

                degreePlanService = new DegreePlanService(
                    adapterRegistry, degreePlanRepo, termRepo, studentRepo, planningStudentRepo, studentProgramRepo,
                    courseRepo, sectionRepo, programRepo, academicCreditRepo, requirementRepo, ruleRepo, progReqRepo,
                    currTrackRepo, configRepo, catalogRepo, null, null, gradeRepo, academicHistoryService,
                    studentDegreePlanRepo, studentDegreePlanService,
                    currentUserFactory, roleRepo, logger, baseConfigurationRepository);

                var degreePlanDto = degreePlanEntityToDtoAdapter.MapToType(degreePlans.First());

#pragma warning disable 618
                var archive = await degreePlanService.ArchiveDegreePlanAsync(degreePlanDto);
#pragma warning restore 618
            }
        }

        [TestClass]
        public class DegreePlanService_ArchiveDegreePlan2 : DegreePlanServiceTestsSetup
        {
            private DegreePlanEntity3Adapter degreePlanEntityToDtoAdapter;
            IEnumerable<DegreePlan> degreePlans;

            [TestInitialize]
            public async void Initialize()
            {
                SetupInitialize();

                // Mock successful Get of a plan from repository
                degreePlans = await new TestStudentDegreePlanRepository().GetAsync();

                // Map degree plan to dto
                degreePlanEntityToDtoAdapter = new DegreePlanEntity3Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<DegreePlan, Dtos.Student.DegreePlans.DegreePlan3>()).Returns(degreePlanEntityToDtoAdapter);

                // Mock Adapter 
                var degreePlanDtoAdapter = new AutoMapperAdapter<DegreePlan, Dtos.Student.DegreePlans.DegreePlan3>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<DegreePlan, Dtos.Student.DegreePlans.DegreePlan3>()).Returns(degreePlanDtoAdapter);
                var degreePlanApprovalDtoAdapter = new AutoMapperAdapter<DegreePlanApproval, Dtos.Student.DegreePlans.DegreePlanApproval2>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<DegreePlanApproval, Dtos.Student.DegreePlans.DegreePlanApproval2>()).Returns(degreePlanApprovalDtoAdapter);
                var plannedCourseWarningEntityAdapter = new AutoMapperAdapter<PlannedCourseWarning, Dtos.Student.DegreePlans.PlannedCourseWarning2>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<PlannedCourseWarning, Dtos.Student.DegreePlans.PlannedCourseWarning2>()).Returns(plannedCourseWarningEntityAdapter);
                var requisiteEntityAdapter = new AutoMapperAdapter<Domain.Student.Entities.Requisite, Dtos.Student.Requisite>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.Requisite, Dtos.Student.Requisite>()).Returns(requisiteEntityAdapter);
                var sectionRequisiteEntityAdapter = new AutoMapperAdapter<Domain.Student.Entities.SectionRequisite, Dtos.Student.SectionRequisite>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.SectionRequisite, Dtos.Student.SectionRequisite>()).Returns(sectionRequisiteEntityAdapter);
                var degreePlanDtoToEntityAdapter = new DegreePlanDto3Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Dtos.Student.DegreePlans.DegreePlan3, Domain.Student.Entities.DegreePlans.DegreePlan>()).Returns(degreePlanDtoToEntityAdapter);

                var degreePlanArchiveAdapter = new AutoMapperAdapter<Domain.Planning.Entities.DegreePlanArchive, Dtos.Planning.DegreePlanArchive2>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Planning.Entities.DegreePlanArchive, Dtos.Planning.DegreePlanArchive2>()).Returns(degreePlanArchiveAdapter);

                var studentPrograms = await new TestStudentProgramRepository().GetAsync("0000896");
                studentProgramRepoMock.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(studentPrograms);

                // Mock the get of courses
                var courses = await new TestCourseRepository().GetAsync();
                courseRepoMock.Setup(x => x.GetAsync()).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Course>>(courses));

                // Mock the get of registration Terms
                var regTerms = await new TestTermRepository().GetRegistrationTermsAsync();
                termRepoMock.Setup(x => x.GetRegistrationTermsAsync()).Returns(Task.FromResult(regTerms));

                var allSections = await new TestSectionRepository().GetRegistrationSectionsAsync(regTerms);
                sectionRepoMock.Setup(x => x.GetCachedSectionsAsync(It.IsAny<IEnumerable<string>>(), false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(allSections));

                DegreePlanArchive degreePlanArchive = new DegreePlanArchive(5, 2, "0000894", 1);
                //studentDegreePlanRepoMock.Setup(x => x.Archive(It.IsAny<DegreePlanArchive>())).Returns((int id, int degreePlanId, string studentId, int version) => new DegreePlanArchive(id, degreePlanId, studentId, version));
                degreePlanArchiveRepoMock.Setup(x => x.AddAsync(It.IsAny<DegreePlanArchive>())).Returns(Task.FromResult(degreePlanArchive));

                // Mock the get of academic credits
                var academicCredits = await new TestAcademicCreditRepository().GetAsync();
                academicCreditRepoMock.Setup(x => x.GetAsync(It.IsAny<Collection<string>>(), It.IsAny<bool>(), true, It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.AcademicCredit>>(academicCredits));

                // Mock the get of student, because we need to have a student object to access the list of academic credits
                var student = await new TestStudentRepository().GetAsync("0000894");
                studentRepoMock.Setup(x => x.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(student));
                var planningStudent = new PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
                planningStudentRepoMock.Setup(repo => repo.GetAsync(student.Id, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent)); studentRepoMock.Setup(x => x.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(student));

                // Mock grade data get
                var grades = await new TestGradeRepository().GetAsync();
                gradeRepoMock.Setup(x => x.GetAsync()).Returns(Task.FromResult(grades));

                studentDegreePlanService = new StudentDegreePlanService(
                    adapterRegistry, studentDegreePlanRepo, termRepo, studentRepo, studentProgramRepo, courseRepo, sectionRepo, programRepo,
                    academicCreditRepo, requirementRepo, ruleRepo, progReqRepo, catalogRepo, gradeRepo, academicHistoryService,
                    currentUserFactory, roleRepo, logger, baseConfigurationRepository, studentConfigurationRepository);

                degreePlanService = new DegreePlanService(
                    adapterRegistry, degreePlanRepo, termRepo, studentRepo, planningStudentRepo, studentProgramRepo,
                    courseRepo, sectionRepo, programRepo, academicCreditRepo, requirementRepo, ruleRepo, progReqRepo,
                    currTrackRepo, configRepo, catalogRepo, degreePlanArchiveRepo, null, gradeRepo, academicHistoryService, 
                    studentDegreePlanRepo, studentDegreePlanService,
                    currentUserFactory, roleRepo, logger, baseConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                degreePlanRepo = null;
                adapterRegistry = null;
                studentRepo = null;
                planningStudentRepo = null;
                studentProgramRepo = null;
                termRepo = null;
                degreePlanService = null;
                studentDegreePlanService = null;
                courseRepo = null;
                sectionRepo = null;
                currentUserFactory = null;
                catalogRepo = null;
                gradeRepo = null;
            }

            [TestMethod]
            public async Task DegreePlanService_ArchiveDegreePlan2Async_SuccessIfStudent()
            {
                // Degree plan to archive
                var degreePlan = degreePlans.Where(d => d.Id == 2).FirstOrDefault();

                // Need to mock get cache for data verification while checking permissions
                studentDegreePlanRepoMock.Setup(repo => repo.GetAsync(It.IsAny<int>())).Returns(Task.FromResult(degreePlan));

                var degreePlanDto = degreePlanEntityToDtoAdapter.MapToType(degreePlan);

                var archive = await degreePlanService.ArchiveDegreePlan2Async(degreePlanDto);

                var expectedArchive = new DegreePlanArchive(archive.Id, degreePlan.Id, currentUserFactory.CurrentUser.PersonId, degreePlan.Version);

                // Can't assert the expectArchive and archive equality directly because they are timestamped at creation
                // So, assert the attributes we know
                Assert.AreEqual(expectedArchive.DegreePlanId, archive.DegreePlanId);
                Assert.AreEqual(expectedArchive.StudentId, archive.StudentId);
                Assert.AreEqual(expectedArchive.ReviewedBy, archive.ReviewedBy);
            }

            [TestMethod]
            public async Task DegreePlanService_ArchiveDegreePlan2Async_Success_AdvisorWithPermissions()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.UpdateAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();

                studentDegreePlanService = new StudentDegreePlanService(
                    adapterRegistry, studentDegreePlanRepo, termRepo, studentRepo, studentProgramRepo, courseRepo, sectionRepo, programRepo,
                    academicCreditRepo, requirementRepo, ruleRepo, progReqRepo, catalogRepo, gradeRepo, academicHistoryService,
                    currentUserFactory, roleRepo, logger, baseConfigurationRepository, studentConfigurationRepository);

                degreePlanService = new DegreePlanService(
                    adapterRegistry, degreePlanRepo, termRepo, studentRepo, planningStudentRepo, studentProgramRepo,
                    courseRepo, sectionRepo, programRepo, academicCreditRepo, requirementRepo, ruleRepo, progReqRepo,
                    currTrackRepo, configRepo, catalogRepo, degreePlanArchiveRepo, null, gradeRepo,
                    academicHistoryService, studentDegreePlanRepo, studentDegreePlanService,
                    currentUserFactory, roleRepo, logger, baseConfigurationRepository);

                var degreePlan = degreePlans.Where(d => d.Id == 2).FirstOrDefault();

                // Need to mock get cache for data verification while checking permissions
                studentDegreePlanRepoMock.Setup(repo => repo.GetAsync(It.IsAny<int>())).Returns(Task.FromResult(degreePlan));

                var degreePlanDto = degreePlanEntityToDtoAdapter.MapToType(degreePlan);

                var archive = await degreePlanService.ArchiveDegreePlan2Async(degreePlanDto);

                var expectedArchive = new DegreePlanArchive(archive.Id, degreePlan.Id, degreePlan.PersonId, degreePlan.Version);

                // Can't assert the expectArchive and archive equality directly because they are timestamped at creation
                // So, assert the attributes we know
                Assert.AreEqual(expectedArchive.DegreePlanId, archive.DegreePlanId);
                Assert.AreEqual(expectedArchive.StudentId, archive.StudentId);
                Assert.AreEqual(expectedArchive.ReviewedBy, archive.ReviewedBy);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task DegreePlanService_ArchiveDegreePlan2Async_Fail_AdvisorWithoutPermissions()
            {
                // Set up advisor as the current user.
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();

                studentDegreePlanService = new StudentDegreePlanService(
                    adapterRegistry, studentDegreePlanRepo, termRepo, studentRepo, studentProgramRepo, courseRepo, sectionRepo, programRepo,
                    academicCreditRepo, requirementRepo, ruleRepo, progReqRepo, catalogRepo, gradeRepo, academicHistoryService,
                    currentUserFactory, roleRepo, logger, baseConfigurationRepository, studentConfigurationRepository);

                degreePlanService = new DegreePlanService(
                    adapterRegistry, degreePlanRepo, termRepo, studentRepo, planningStudentRepo, studentProgramRepo,
                    courseRepo, sectionRepo, programRepo, academicCreditRepo, requirementRepo, ruleRepo, progReqRepo,
                    currTrackRepo, configRepo, catalogRepo, null, null, gradeRepo, academicHistoryService, studentDegreePlanRepo, studentDegreePlanService,
                    currentUserFactory, roleRepo, logger, baseConfigurationRepository);

                var degreePlanEntity = degreePlans.First();
                var degreePlanDto = degreePlanEntityToDtoAdapter.MapToType(degreePlanEntity);

                // Mock cached degree plan get for verification methods (not being tested here)
                studentDegreePlanRepoMock.Setup(repo => repo.GetAsync(It.IsAny<int>())).Returns(Task.FromResult(degreePlanEntity));

                var archive = await degreePlanService.ArchiveDegreePlan2Async(degreePlanDto);
            }
        }

        [TestClass]
        public class DegreePlanService_ArchiveDegreePlan3 : DegreePlanServiceTestsSetup
        {
            private DegreePlanEntity4Adapter degreePlanEntityToDtoAdapter;

            IEnumerable<DegreePlan> degreePlans;

            [TestInitialize]
            public async void Initialize()
            {
                SetupInitialize();

                // Mock successful Get of a plan from repository
                degreePlans = await new TestStudentDegreePlanRepository().GetAsync();

                // Map degree plan to dto
                degreePlanEntityToDtoAdapter = new DegreePlanEntity4Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<DegreePlan, Dtos.Student.DegreePlans.DegreePlan4>()).Returns(degreePlanEntityToDtoAdapter);

                // Mock Adapter 
                var degreePlanDtoAdapter = new AutoMapperAdapter<DegreePlan, Dtos.Student.DegreePlans.DegreePlan4>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<DegreePlan, Dtos.Student.DegreePlans.DegreePlan4>()).Returns(degreePlanDtoAdapter);
                var degreePlanApprovalDtoAdapter = new AutoMapperAdapter<DegreePlanApproval, Dtos.Student.DegreePlans.DegreePlanApproval2>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<DegreePlanApproval, Dtos.Student.DegreePlans.DegreePlanApproval2>()).Returns(degreePlanApprovalDtoAdapter);
                var plannedCourseWarningEntityAdapter = new AutoMapperAdapter<PlannedCourseWarning, Dtos.Student.DegreePlans.PlannedCourseWarning2>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<PlannedCourseWarning, Dtos.Student.DegreePlans.PlannedCourseWarning2>()).Returns(plannedCourseWarningEntityAdapter);
                var requisiteEntityAdapter = new AutoMapperAdapter<Domain.Student.Entities.Requisite, Dtos.Student.Requisite>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.Requisite, Dtos.Student.Requisite>()).Returns(requisiteEntityAdapter);
                var sectionRequisiteEntityAdapter = new AutoMapperAdapter<Domain.Student.Entities.SectionRequisite, Dtos.Student.SectionRequisite>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.SectionRequisite, Dtos.Student.SectionRequisite>()).Returns(sectionRequisiteEntityAdapter);
                var degreePlanDtoToEntityAdapter = new DegreePlanDto4Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Dtos.Student.DegreePlans.DegreePlan4, Domain.Student.Entities.DegreePlans.DegreePlan>()).Returns(degreePlanDtoToEntityAdapter);

                var degreePlanArchiveAdapter = new AutoMapperAdapter<Domain.Planning.Entities.DegreePlanArchive, Dtos.Planning.DegreePlanArchive2>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Planning.Entities.DegreePlanArchive, Dtos.Planning.DegreePlanArchive2>()).Returns(degreePlanArchiveAdapter);

                var studentPrograms = await new TestStudentProgramRepository().GetAsync("0000896");
                studentProgramRepoMock.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(studentPrograms);

                // Mock the get of courses
                var courses = await new TestCourseRepository().GetAsync();
                courseRepoMock.Setup(x => x.GetAsync()).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Course>>(courses));

                // Mock the get of registration Terms
                var regTerms = await new TestTermRepository().GetRegistrationTermsAsync();
                termRepoMock.Setup(x => x.GetRegistrationTermsAsync()).Returns(Task.FromResult(regTerms));

                var allSections = await new TestSectionRepository().GetRegistrationSectionsAsync(regTerms);
                sectionRepoMock.Setup(x => x.GetCachedSectionsAsync(It.IsAny<IEnumerable<string>>(), false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(allSections));

                DegreePlanArchive degreePlanArchive = new DegreePlanArchive(5, 2, "0000894", 1);
                //studentDegreePlanRepoMock.Setup(x => x.Archive(It.IsAny<DegreePlanArchive>())).Returns((int id, int degreePlanId, string studentId, int version) => new DegreePlanArchive(id, degreePlanId, studentId, version));
                degreePlanArchiveRepoMock.Setup(x => x.AddAsync(It.IsAny<DegreePlanArchive>())).Returns(Task.FromResult(degreePlanArchive));

                // Mock the get of academic credits
                var academicCredits = await new TestAcademicCreditRepository().GetAsync();
                academicCreditRepoMock.Setup(x => x.GetAsync(It.IsAny<Collection<string>>(), It.IsAny<bool>(), true, It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.AcademicCredit>>(academicCredits));

                // Mock the get of student, because we need to have a student object to access the list of academic credits
                var student = await new TestStudentRepository().GetAsync("00004001");
                //studentRepoMock.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(student);
                var planningStudent = new PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
                planningStudentRepoMock.Setup(repo => repo.GetAsync(student.Id, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));

                // Mock grade data get
                var grades = await new TestGradeRepository().GetAsync();
                gradeRepoMock.Setup(x => x.GetAsync()).Returns(Task.FromResult(grades));

                studentDegreePlanService = new StudentDegreePlanService(
                    adapterRegistry, studentDegreePlanRepo, termRepo, studentRepo, studentProgramRepo, courseRepo, sectionRepo, programRepo,
                    academicCreditRepo, requirementRepo, ruleRepo, progReqRepo, catalogRepo, gradeRepo, academicHistoryService,
                    currentUserFactory, roleRepo, logger, baseConfigurationRepository, studentConfigurationRepository);

                degreePlanService = new DegreePlanService(
                    adapterRegistry, degreePlanRepo, termRepo, studentRepo, planningStudentRepo, studentProgramRepo,
                    courseRepo, sectionRepo, programRepo, academicCreditRepo, requirementRepo, ruleRepo, progReqRepo,
                    currTrackRepo, configRepo, catalogRepo, degreePlanArchiveRepo, null, gradeRepo, academicHistoryService,
                    studentDegreePlanRepo, studentDegreePlanService,
                    currentUserFactory, roleRepo, logger, baseConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                SetupCleanup();
            }

            [TestMethod]
            public async Task DegreePlanService_ArchiveDegreePlan3Async_SuccessIfStudent()
            {
                // Mock the get of student, because we need to have a student object to access the list of academic credits
                var student = await new TestStudentRepository().GetAsync("0000894");
                studentRepoMock.Setup(x => x.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(student));

                var planningStudent = new PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
                planningStudentRepoMock.Setup(repo => repo.GetAsync(student.Id, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));

                // Degree plan to archive
                var degreePlan = degreePlans.Where(d => d.Id == 2).FirstOrDefault();
                // Need to mock get cache for data verification while checking permissions
                studentDegreePlanRepoMock.Setup(repo => repo.GetAsync(It.IsAny<int>())).Returns(Task.FromResult(degreePlan));

                var degreePlanDto = degreePlanEntityToDtoAdapter.MapToType(degreePlan);

                var archive = await degreePlanService.ArchiveDegreePlan3Async(degreePlanDto);

                var expectedArchive = new DegreePlanArchive(archive.Id, degreePlan.Id, currentUserFactory.CurrentUser.PersonId, degreePlan.Version);

                // Can't assert the expectArchive and archive equality directly because they are timestamped at creation
                // So, assert the attributes we know
                Assert.AreEqual(expectedArchive.DegreePlanId, archive.DegreePlanId);
                Assert.AreEqual(expectedArchive.StudentId, archive.StudentId);
                Assert.AreEqual(expectedArchive.ReviewedBy, archive.ReviewedBy);
            }

            [TestMethod]
            public async Task DegreePlanService_ArchiveDegreePlan3Async_Success_AdvisorWithPermissions()
            {
                // Mock the get of student, because we need to have a student object to access the list of academic credits
                var student = await new TestStudentRepository().GetAsync("0000894");
                studentRepoMock.Setup(x => x.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(student));

                var planningStudent = new PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
                foreach (var advisement in student.Advisements)
                {
                    planningStudent.AddAdvisement(advisement.AdvisorId, advisement.StartDate, advisement.EndDate, advisement.AdvisorType);
                }
                planningStudentRepoMock.Setup(repo => repo.GetAsync(student.Id, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));

                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.UpdateAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();

                studentDegreePlanService = new StudentDegreePlanService(
                    adapterRegistry, studentDegreePlanRepo, termRepo, studentRepo, studentProgramRepo, courseRepo, sectionRepo, programRepo,
                    academicCreditRepo, requirementRepo, ruleRepo, progReqRepo, catalogRepo, gradeRepo, academicHistoryService,
                    currentUserFactory, roleRepo, logger, baseConfigurationRepository, studentConfigurationRepository);

                degreePlanService = new DegreePlanService(
                    adapterRegistry, degreePlanRepo, termRepo, studentRepo, planningStudentRepo, studentProgramRepo,
                    courseRepo, sectionRepo, programRepo, academicCreditRepo, requirementRepo, ruleRepo, progReqRepo,
                    currTrackRepo, configRepo, catalogRepo, degreePlanArchiveRepo, null, gradeRepo, academicHistoryService,
                    studentDegreePlanRepo, studentDegreePlanService,
                    currentUserFactory, roleRepo, logger, baseConfigurationRepository);

                var degreePlan = degreePlans.Where(d => d.Id == 2).FirstOrDefault();
                // Need to mock get cache for data verification while checking permissions
                studentDegreePlanRepoMock.Setup(repo => repo.GetAsync(It.IsAny<int>())).Returns(Task.FromResult(degreePlan));
                var degreePlanDto = degreePlanEntityToDtoAdapter.MapToType(degreePlan);

                var archive = await degreePlanService.ArchiveDegreePlan3Async(degreePlanDto);

                var expectedArchive = new DegreePlanArchive(archive.Id, degreePlan.Id, degreePlan.PersonId, degreePlan.Version);

                // Can't assert the expectArchive and archive equality directly because they are timestamped at creation
                // So, assert the attributes we know
                Assert.AreEqual(expectedArchive.DegreePlanId, archive.DegreePlanId);
                Assert.AreEqual(expectedArchive.StudentId, archive.StudentId);
                Assert.AreEqual(expectedArchive.ReviewedBy, archive.ReviewedBy);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task DegreePlanService_ArchiveDegreePlan3Async_Fail_AdvisorWithoutPermissions()
            {
                // Mock the get of student, because we need to have a student object to access the list of academic credits
                var student = await new TestStudentRepository().GetAsync("0000894");
                var planningStudent = new PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
                planningStudentRepoMock.Setup(repo => repo.GetAsync(student.Id, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));

                // Set up advisor as the current user.
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();

                studentDegreePlanService = new StudentDegreePlanService(
                    adapterRegistry, studentDegreePlanRepo, termRepo, studentRepo, studentProgramRepo, courseRepo, sectionRepo, programRepo,
                    academicCreditRepo, requirementRepo, ruleRepo, progReqRepo, catalogRepo, gradeRepo, academicHistoryService,
                    currentUserFactory, roleRepo, logger, baseConfigurationRepository, studentConfigurationRepository);

                degreePlanService = new DegreePlanService(
                    adapterRegistry, degreePlanRepo, termRepo, studentRepo, planningStudentRepo, studentProgramRepo,
                    courseRepo, sectionRepo, programRepo, academicCreditRepo, requirementRepo, ruleRepo, progReqRepo,
                    currTrackRepo, configRepo, catalogRepo, null, null, gradeRepo, academicHistoryService, studentDegreePlanRepo, studentDegreePlanService,
                    currentUserFactory, roleRepo, logger, baseConfigurationRepository);

                var degreePlanEntity = degreePlans.First();
                var degreePlanDto = degreePlanEntityToDtoAdapter.MapToType(degreePlanEntity);
                // Mock cached degree plan get for verification methods (not being tested here)
                studentDegreePlanRepoMock.Setup(repo => repo.GetAsync(It.IsAny<int>())).Returns(Task.FromResult(degreePlanEntity));

                var archive = await degreePlanService.ArchiveDegreePlan3Async(degreePlanDto);
            }
        }

        [TestClass]
        public class DegreePlanService_GetDegreePlanArchives : DegreePlanServiceTestsSetup
        {
            private DegreePlan degreePlan;
            private List<DegreePlanArchive> degreePlanArchives = new List<DegreePlanArchive>();

            [TestInitialize]
            public async void Initialize()
            {
                SetupInitialize();

                var degreePlanArchiveAdapter = new AutoMapperAdapter<Domain.Planning.Entities.DegreePlanArchive, Dtos.Planning.DegreePlanArchive>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Planning.Entities.DegreePlanArchive, Dtos.Planning.DegreePlanArchive>()).Returns(degreePlanArchiveAdapter);

                var studentPrograms = await new TestStudentProgramRepository().GetAsync("0000896");
                studentProgramRepoMock.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(studentPrograms);

                // Mock the get of courses
                var courses = await new TestCourseRepository().GetAsync();
                courseRepoMock.Setup(x => x.GetAsync()).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Course>>(courses));

                // Mock the get of registration Terms
                var regTerms = await new TestTermRepository().GetRegistrationTermsAsync();
                termRepoMock.Setup(x => x.GetRegistrationTermsAsync()).Returns(Task.FromResult(regTerms));

                var allSections = await new TestSectionRepository().GetRegistrationSectionsAsync(regTerms);
                sectionRepoMock.Setup(x => x.GetCachedSectionsAsync(It.IsAny<IEnumerable<string>>(), false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(allSections));

                degreePlan = new DegreePlan("0000894");
                degreePlan.Id = 2;
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(2)).Returns(Task.FromResult(degreePlan));

                DegreePlanArchive degreePlanArchive1 = new DegreePlanArchive(5, 2, "0000894", 1);
                DegreePlanArchive degreePlanArchive2 = new DegreePlanArchive(6, 2, "0000894", 1);

                degreePlanArchives.Add(degreePlanArchive1);
                degreePlanArchives.Add(degreePlanArchive2);
                degreePlanArchiveRepoMock.Setup(x => x.GetDegreePlanArchivesAsync(It.IsAny<int>())).Returns(Task.FromResult<IEnumerable<DegreePlanArchive>>(degreePlanArchives));

                // Mock the get of academic credits
                var academicCredits = await new TestAcademicCreditRepository().GetAsync();
                academicCreditRepoMock.Setup(x => x.GetAsync(It.IsAny<ICollection<string>>(), false, true, It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.AcademicCredit>>(academicCredits));

                // Mock the get of student, because we need to have a student object to access the list of academic credits
                var student = new TestStudentRepository().GetAsync("00004001");
                studentRepoMock.Setup(x => x.GetAsync(It.IsAny<string>())).Returns(student);

                // Mock grade data get
                var grades = await new TestGradeRepository().GetAsync();
                gradeRepoMock.Setup(x => x.GetAsync()).Returns(Task.FromResult(grades));

                degreePlanService = new DegreePlanService(
                    adapterRegistry, degreePlanRepo, termRepo, studentRepo, planningStudentRepo, studentProgramRepo,
                    courseRepo, sectionRepo, programRepo, academicCreditRepo, requirementRepo, ruleRepo, progReqRepo,
                    currTrackRepo, configRepo, catalogRepo, degreePlanArchiveRepo, null, gradeRepo, academicHistoryService, studentDegreePlanRepo, studentDegreePlanService,
                    currentUserFactory, roleRepo, logger, baseConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                SetupCleanup();
            }

            [TestMethod]
            public async Task DegreePlanService_GetDegreePlanArchivesAsync_Success()
            {
#pragma warning disable 618
                var archives = await degreePlanService.GetDegreePlanArchivesAsync(degreePlan.Id);
#pragma warning restore 618

                Assert.AreEqual(degreePlanArchives.Count, archives.Count());
                foreach (var archive in degreePlanArchives)
                {
                    // If there is an archive missing, Single will throw an exception
                    archives.Single(x => x.Id == archive.Id);
                }
            }

            [TestMethod]
            public async Task DegreePlanService_GetDegreePlanArchivesAsync_Success_Advisor()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.UpdateAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();

                degreePlanService = new DegreePlanService(
                    adapterRegistry, degreePlanRepo, termRepo, studentRepo, planningStudentRepo, studentProgramRepo,
                    courseRepo, sectionRepo, programRepo, academicCreditRepo, requirementRepo, ruleRepo, progReqRepo,
                    currTrackRepo, configRepo, catalogRepo, degreePlanArchiveRepo, null, gradeRepo, academicHistoryService, studentDegreePlanRepo, studentDegreePlanService,
                    currentUserFactory, roleRepo, logger, baseConfigurationRepository);

#pragma warning disable 618
                var archives = await degreePlanService.GetDegreePlanArchivesAsync(degreePlan.Id);
#pragma warning restore 618

                Assert.AreEqual(degreePlanArchives.Count, archives.Count());
                foreach (var archive in degreePlanArchives)
                {
                    // If there is an archive missing, Single will throw an exception
                    archives.Single(x => x.Id == archive.Id);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task DegreePlanService_GetDegreePlanArchivesAsync_ExceptionAdvisorPermissions()
            {
                // Set up advisor as the current user.
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();

                degreePlanService = new DegreePlanService(
                    adapterRegistry, degreePlanRepo, termRepo, studentRepo, planningStudentRepo, studentProgramRepo,
                    courseRepo, sectionRepo, programRepo, academicCreditRepo, requirementRepo, ruleRepo, progReqRepo,
                    currTrackRepo, configRepo, catalogRepo, degreePlanArchiveRepo, null, gradeRepo, academicHistoryService, studentDegreePlanRepo, studentDegreePlanService,
                    currentUserFactory, roleRepo, logger, baseConfigurationRepository);

#pragma warning disable 618
                var archives = await degreePlanService.GetDegreePlanArchivesAsync(degreePlan.Id);
#pragma warning restore 618
            }
        }

        [TestClass]
        public class DegreePlanService_GetDegreePlanArchives2 : DegreePlanServiceTestsSetup
        {
            private DegreePlan degreePlan;
            private List<DegreePlanArchive> degreePlanArchives = new List<DegreePlanArchive>();

            [TestInitialize]
            public async void Initialize()
            {
                SetupInitialize();

                var degreePlanArchiveAdapter = new AutoMapperAdapter<Domain.Planning.Entities.DegreePlanArchive, Dtos.Planning.DegreePlanArchive2>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Planning.Entities.DegreePlanArchive, Dtos.Planning.DegreePlanArchive2>()).Returns(degreePlanArchiveAdapter);

                var studentPrograms = await new TestStudentProgramRepository().GetAsync("0000896");
                studentProgramRepoMock.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(studentPrograms);

                // Mock the get of courses
                var courses = await new TestCourseRepository().GetAsync();
                courseRepoMock.Setup(x => x.GetAsync()).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Course>>(courses));

                // Mock the get of registration Terms
                var regTerms = await new TestTermRepository().GetRegistrationTermsAsync();
                termRepoMock.Setup(x => x.GetRegistrationTermsAsync()).Returns(Task.FromResult(regTerms));

                var allSections = await new TestSectionRepository().GetRegistrationSectionsAsync(regTerms);
                sectionRepoMock.Setup(x => x.GetCachedSectionsAsync(It.IsAny<IEnumerable<string>>(), false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(allSections));

                degreePlan = new DegreePlan("0000894");
                degreePlan.Id = 2;
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(2)).Returns(Task.FromResult(degreePlan));

                DegreePlanArchive degreePlanArchive1 = new DegreePlanArchive(5, 2, "0000894", 1);
                DegreePlanArchive degreePlanArchive2 = new DegreePlanArchive(6, 2, "0000894", 1);

                degreePlanArchives.Add(degreePlanArchive1);
                degreePlanArchives.Add(degreePlanArchive2);
                degreePlanArchiveRepoMock.Setup(x => x.GetDegreePlanArchivesAsync(It.IsAny<int>())).Returns(Task.FromResult<IEnumerable<DegreePlanArchive>>(degreePlanArchives));

                // Mock the get of academic credits
                var academicCredits = await new TestAcademicCreditRepository().GetAsync();
                academicCreditRepoMock.Setup(x => x.GetAsync(It.IsAny<ICollection<string>>(), false, true, It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.AcademicCredit>>(academicCredits));

                // Mock the get of student, because we need to have a student object to access the list of academic credits
                var student = new TestStudentRepository().GetAsync("00004001");
                studentRepoMock.Setup(x => x.GetAsync(It.IsAny<string>())).Returns(student);

                // Mock grade data get
                var grades = await new TestGradeRepository().GetAsync();
                gradeRepoMock.Setup(x => x.GetAsync()).Returns(Task.FromResult(grades));

                degreePlanService = new DegreePlanService(
                    adapterRegistry, degreePlanRepo, termRepo, studentRepo, planningStudentRepo, studentProgramRepo,
                    courseRepo, sectionRepo, programRepo, academicCreditRepo, requirementRepo, ruleRepo, progReqRepo,
                    currTrackRepo, configRepo, catalogRepo, degreePlanArchiveRepo, null, gradeRepo, academicHistoryService, studentDegreePlanRepo, studentDegreePlanService,
                    currentUserFactory, roleRepo, logger, baseConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                SetupCleanup();
            }

            [TestMethod]
            public async Task DegreePlanService_GetDegreePlanArchives2Async_Success()
            {
                var archives = await degreePlanService.GetDegreePlanArchives2Async(degreePlan.Id);

                Assert.AreEqual(degreePlanArchives.Count, archives.Count());
                foreach (var archive in degreePlanArchives)
                {
                    // If there is an archive missing, Single will throw an exception
                    archives.Single(x => x.Id == archive.Id);
                }
            }

            [TestMethod]
            public async Task DegreePlanService_GetDegreePlanArchives2Async_Success_Advisor()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.UpdateAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();

                degreePlanService = new DegreePlanService(
                    adapterRegistry, degreePlanRepo, termRepo, studentRepo, planningStudentRepo, studentProgramRepo,
                    courseRepo, sectionRepo, programRepo, academicCreditRepo, requirementRepo, ruleRepo, progReqRepo,
                    currTrackRepo, configRepo, catalogRepo, degreePlanArchiveRepo, null, gradeRepo, academicHistoryService,
                    studentDegreePlanRepo, studentDegreePlanService,
                    currentUserFactory, roleRepo, logger, baseConfigurationRepository);

                var archives = await degreePlanService.GetDegreePlanArchives2Async(degreePlan.Id);

                Assert.AreEqual(degreePlanArchives.Count, archives.Count());
                foreach (var archive in degreePlanArchives)
                {
                    // If there is an archive missing, Single will throw an exception
                    archives.Single(x => x.Id == archive.Id);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task DegreePlanService_GetDegreePlanArchives2Async_ExceptionAdvisorPermissions()
            {
                // Set up advisor as the current user.
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();

                degreePlanService = new DegreePlanService(
                    adapterRegistry, degreePlanRepo, termRepo, studentRepo, planningStudentRepo, studentProgramRepo,
                    courseRepo, sectionRepo, programRepo, academicCreditRepo, requirementRepo, ruleRepo, progReqRepo,
                    currTrackRepo, configRepo, catalogRepo, degreePlanArchiveRepo, null, gradeRepo, academicHistoryService,
                    studentDegreePlanRepo, studentDegreePlanService,
                    currentUserFactory, roleRepo, logger, baseConfigurationRepository);

                var archives = await degreePlanService.GetDegreePlanArchives2Async(degreePlan.Id);
            }
        }

        [TestClass]
        public class DegreePlanService_GetDegreePlanArchiveReport : DegreePlanServiceTestsSetup
        {
            private DegreePlanArchive degreePlanArchive1;
            private DegreePlanArchive degreePlanArchive2;
            private string knownFacultyAdvisor;
            private string knownStaffAdvisor;
            private string unknownAdvisor;
            private string knownStudent;
            private DateTime archivedDate;
            private DateTime reviewedDate;

            [TestInitialize]
            public async void Initialize()
            {
                SetupInitialize();

                knownStudent = "0000894";
                knownFacultyAdvisor = "0000222";
                knownStaffAdvisor = "0000111";
                unknownAdvisor = "9999999";
                archivedDate = new DateTime(2014, 1, 14, 10, 0, 0);
                reviewedDate = new DateTime(2014, 1, 13, 9, 0, 0);

                // Set up student 0000894 as the current user.
                currentUserFactory = new CurrentUserSetup.StudentUserFactory();

                // Mock successful Gets of a plan archives from repository
                // 1: Valid student and faculty advisor for created by, staff advisor for reviewed by.  One good program, and one bad program.
                var studentPrograms = new List<Domain.Student.Entities.StudentProgram>()
                {
                    new Domain.Student.Entities.StudentProgram("0000894", "MATH.BS", "2012"),
                    new Domain.Student.Entities.StudentProgram("0000894", "ECON.BA", "2012")
                };
                var notes = new List<Domain.Student.Entities.DegreePlans.DegreePlanNote>()
                {
                    new DegreePlanNote(34, "0000222", reviewedDate, "Text Note 2"),
                    new Domain.Student.Entities.DegreePlans.DegreePlanNote(33, "0000111", archivedDate, "Text Note 1"),
                };
                degreePlanArchive1 = new DegreePlanArchive(1, 2, "0000894", 10) { CreatedBy = "0000222", CreatedDate = archivedDate, ReviewedBy = "0000111", ReviewedDate = reviewedDate, StudentPrograms = studentPrograms, Notes = notes };
                degreePlanArchiveRepoMock.Setup(repo => repo.GetDegreePlanArchiveAsync(degreePlanArchive1.Id)).Returns(Task.FromResult(degreePlanArchive1));

                // 2: Invalid advisor
                degreePlanArchive2 = new DegreePlanArchive(2, 3, "0000894", 5) { CreatedBy = unknownAdvisor, CreatedDate = archivedDate };
                degreePlanArchiveRepoMock.Setup(repo => repo.GetDegreePlanArchiveAsync(degreePlanArchive2.Id)).Returns(Task.FromResult(degreePlanArchive2));

                // 3: Permission failure - no access to this archive
                var degreePlanArchive3 = new DegreePlanArchive(3, 4, "0000896", 5) { CreatedBy = unknownAdvisor, CreatedDate = archivedDate };
                degreePlanArchiveRepoMock.Setup(repo => repo.GetDegreePlanArchiveAsync(degreePlanArchive3.Id)).Returns(Task.FromResult(degreePlanArchive3));

                // Mock successful Get of student 0000894 from repository
                var student = new Domain.Student.Entities.Student(knownStudent, "StudentLast", 2, new List<String>() { "MATH.BS", "ENGL.BA" }, new List<string>()) { FirstName = "StudentFirst" };
                studentRepoMock.Setup(repo => repo.GetAsync(knownStudent)).Returns(Task.FromResult(student));

                // Mock successful Get of an advisor.
                var advisor = new Advisor(knownFacultyAdvisor, "FacultyLast") { FirstName = "FacultyFirst", Name = "FacultyPreferred" };
                advisorRepoMock.Setup(repo => repo.GetAsync(knownFacultyAdvisor, AdviseeInclusionType.NoAdvisees)).ReturnsAsync(advisor);

                // Mock successful Get of a staff.
                var staff = new Advisor(knownStaffAdvisor, "StaffLast") { FirstName = "StaffFirst", Name = "StaffPreferred" };
                advisorRepoMock.Setup(repo => repo.GetAsync(knownStaffAdvisor, AdviseeInclusionType.NoAdvisees)).ReturnsAsync(staff);

                // Mock getting of program codes from repository 
                var programs = await new TestProgramRepository().GetAsync();
                IEnumerable<Domain.Student.Entities.Requirements.Program> program =
                    new List<Domain.Student.Entities.Requirements.Program>() { programs.Where(p => p.Code == "MATH.BS").First() };
                programRepoMock.Setup(repo => repo.GetAsync()).Returns(Task.FromResult(program));

                // Mock the get of the student programs
                studentProgramRepoMock.Setup(x => x.GetAsync("0000894")).ReturnsAsync(studentPrograms);

                // There will be one unknown program

                // Mock getting of terms from repository 
                var terms = await new TestTermRepository().GetAsync();
                termRepoMock.Setup(repo => repo.GetAsync()).ReturnsAsync(terms);

                // Mock grade repo data
                var grades = await new TestGradeRepository().GetAsync();
                gradeRepoMock.Setup(x => x.GetAsync()).Returns(Task.FromResult(grades));

                // Mock the necessary adapters
                var archivedCourseAdapter = new AutoMapperAdapter<Domain.Planning.Entities.ArchivedCourse, Coordination.Planning.ArchivedCourse>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Planning.Entities.ArchivedCourse, Coordination.Planning.ArchivedCourse>()).Returns(archivedCourseAdapter);

                degreePlanService = new DegreePlanService(
                    adapterRegistry, degreePlanRepo, termRepo, studentRepo, planningStudentRepo, studentProgramRepo,
                    courseRepo, sectionRepo, programRepo, academicCreditRepo, requirementRepo, ruleRepo, progReqRepo,
                    currTrackRepo, configRepo, catalogRepo, degreePlanArchiveRepo, advisorRepo, gradeRepo, 
                    academicHistoryService, studentDegreePlanRepo, studentDegreePlanService,
                    currentUserFactory, roleRepo, logger, baseConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                SetupCleanup();
            }

            [TestMethod]
            public async Task GetDegreePlanArchiveReportAsync_StudentInfo()
            {
                var report = await degreePlanService.GetDegreePlanArchiveReportAsync(1);

                Assert.AreEqual(knownStudent, report.StudentId);
                Assert.AreEqual("StudentLast", report.StudentLastName);
                Assert.AreEqual("StudentFirst", report.StudentFirstName);
            }

            [TestMethod]
            public async Task GetDegreePlanArchiveReportAsync_AdvisorName()
            {
                var report = await degreePlanService.GetDegreePlanArchiveReportAsync(1);

                Assert.AreEqual("FacultyPreferred", report.ArchivedBy);
                Assert.AreEqual(archivedDate, report.ArchivedOn);
                Assert.AreEqual("StaffPreferred", report.ReviewedBy);
                Assert.AreEqual(reviewedDate, report.ReviewedOn);
            }

            [TestMethod]
            public async Task GetDegreePlanArchiveReportAsync_Notes()
            {
                var report = await degreePlanService.GetDegreePlanArchiveReportAsync(1);

                Assert.AreEqual(2, report.ArchivedNotes.Count());

                // Notes are sorted by descending date
                var note = report.ArchivedNotes.ElementAt(0);
                Assert.AreEqual("StaffLast, S.", note.PersonName);
                Assert.AreEqual("on " + archivedDate.ToShortDateString() + " at " + archivedDate.ToShortTimeString(), note.Date);
                Assert.AreEqual("Text Note 1", note.Text);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task GetDegreePlanArchiveReportAsync_Fail_NoPermission()
            {
                // have the repository return a student that is not the current user
                var otherStudent = new Domain.Student.Entities.Student("0000896", "Smith", 2, new List<String>() { "MATH.BS", "ENGL.BA" }, new List<string>()) { FirstName = "Joe" };
                studentRepoMock.Setup(repo => repo.GetAsync("0000896")).Returns(Task.FromResult(otherStudent));

                var report = await degreePlanService.GetDegreePlanArchiveReportAsync(3);
            }
        }

        [TestClass]
        public class DegreePlanService_PreviewSampleDegreePlan4 : DegreePlanServiceTestsSetup
        {
            private Dtos.Student.DegreePlans.DegreePlan4 degreePlanDto;
            private IEnumerable<Domain.Student.Entities.Course> courses;
            private Domain.Student.Entities.AcademicHistory historyEntity;
            private Dtos.Student.AcademicHistory2 historyDto;
            private Dtos.Student.AcademicHistory4 history4Dto;

            [TestInitialize]
            public async void Initialize()
            {
                SetupInitialize();

                // Mock the get from the student repository
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));

                // Mock the get of the planning terms
                IEnumerable<Domain.Student.Entities.Term> allTerms = await new TestTermRepository().GetAsync();
                termRepoMock.Setup(x => x.GetAsync()).ReturnsAsync(allTerms);

                // Mock the get of courses
                courses = await new TestCourseRepository().GetAsync();
                courseRepoMock.Setup(x => x.GetAsync()).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Course>>(courses));

                // Mock the get for the catalogs
                ICollection<Domain.Student.Entities.Requirements.Catalog> allCatalogs = await new TestCatalogRepository().GetAsync();
                catalogRepoMock.Setup(cr => cr.GetAsync()).Returns(Task.FromResult(allCatalogs));

                // Mock the planningConfiguration
                var planningConfig = new PlanningConfiguration() { DefaultCatalogPolicy = CatalogPolicy.StudentCatalogYear, DefaultCurriculumTrack = "TRACK3" };
                configRepoMock.Setup(x => x.GetPlanningConfigurationAsync()).Returns(Task.FromResult(planningConfig));

                // Mock the get of the student programs
                var studentPrograms = await new TestStudentProgramRepository().GetAsync("0000894");
                studentProgramRepoMock.Setup(x => x.GetAsync("0000894")).ReturnsAsync(studentPrograms);

                // Mock successful get of a plan from repository
                DegreePlan degreePlan = new DegreePlan(1, "0000894", 0);
                degreePlan.AddTerm("2012/FA");
                degreePlan.AddTerm("2013/SP");
                degreePlan.AddTerm("2013/FA");
                degreePlan.AddTerm("2014/SP");
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(1)).Returns(Task.FromResult(degreePlan));

                // Mock grade data get
                var grades = await new TestGradeRepository().GetAsync();
                gradeRepoMock.Setup(x => x.GetAsync()).Returns(Task.FromResult(grades));

                // Mock dto Adapter 
                var datetimeOffsetAdapter = new DateTimeOffsetToDateTimeAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<System.DateTimeOffset, System.DateTime>()).Returns(datetimeOffsetAdapter);

                var degreePlanPreview4EntityToDtoAdapter = new DegreePlanPreviewEntity4Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Planning.Entities.DegreePlanPreview, Dtos.Planning.DegreePlanPreview4>()).Returns(degreePlanPreview4EntityToDtoAdapter);
                var degreePlanPreview6EntityToDtoAdapter = new DegreePlanPreviewEntity6Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Planning.Entities.DegreePlanPreview, Dtos.Planning.DegreePlanPreview6>()).Returns(degreePlanPreview6EntityToDtoAdapter);
                var degreePlan4EntityToDtoAdapter = new DegreePlanEntity4Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan4>()).Returns(degreePlan4EntityToDtoAdapter);

                // Mock entity adapters
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

                var gradeRestriction = new Domain.Student.Entities.GradeRestriction(false);
                historyEntity = new Domain.Student.Entities.AcademicHistory(studentAcademicCredits, gradeRestriction, null);
                historyEntity.StudentId = "0000894";
                historyDto = new AcademicHistoryEntityToAcademicHistory2DtoAdapter(adapterRegistry, logger).MapToType(historyEntity);
                history4Dto = new AcademicHistoryEntityToAcademicHistory4DtoAdapter(adapterRegistry, logger).MapToType(historyEntity);
                academicHistoryServiceMock.Setup(svc => svc.ConvertAcademicCreditsToAcademicHistoryDtoAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Domain.Student.Entities.AcademicCredit>>(), It.IsAny<Domain.Student.Entities.Student>()))
                    .Returns(Task.FromResult(historyDto));
                academicHistoryServiceMock.Setup(svc => svc.ConvertAcademicCreditsToAcademicHistoryDto4Async(It.IsAny<string>(), It.IsAny<IEnumerable<Domain.Student.Entities.AcademicCredit>>(), It.IsAny<Domain.Student.Entities.Student>()))
                    .Returns(Task.FromResult(history4Dto));

                degreePlanService = new DegreePlanService(
                    adapterRegistry, degreePlanRepo, termRepo, studentRepo, planningStudentRepo, studentProgramRepo,
                    courseRepo, sectionRepo, programRepo, academicCreditRepo, requirementRepo, ruleRepo, progReqRepo,
                    currTrackRepo, configRepo, catalogRepo, null, null, gradeRepo, academicHistoryService, 
                    studentDegreePlanRepo, studentDegreePlanService,
                    currentUserFactory, roleRepo, logger, baseConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                SetupCleanup();
            }

            [TestMethod]
            public async Task PreviewSamplePlan4Async_UsingDefaultSamplePlan()
            {
                Domain.Student.Entities.StudentProgram studentProgram = await new TestStudentProgramRepository().GetAsync("0000894", "MATH.BA");
                studentProgramRepoMock.Setup(x => x.GetAsync("0000894", "MATH.BA")).Returns(Task.FromResult(studentProgram));

                var programCode = "MATH.BA";
                var catalogCode = "2013";
                var programReq = await new TestProgramRequirementsRepository().GetAsync(programCode, catalogCode);
                progReqRepoMock.Setup(x => x.GetAsync(programCode, catalogCode)).Returns(Task.FromResult(programReq));

                var currTrack = await new TestSampleDegreePlanRepository().GetAsync("TRACK3");
                currTrackRepoMock.Setup(x => x.GetAsync("TRACK3")).Returns(Task.FromResult(currTrack));

                // Mock no academic credits because we don't want anything in the academic history to cause something to not be applied.
                var academicCreditDict = new Dictionary<string, List<Domain.Student.Entities.AcademicCredit>>();
                academicCreditRepoMock.Setup(x => x.GetAcademicCreditByStudentIdsAsync(It.IsAny<List<string>>(), false, true, It.IsAny<bool>())).Returns(Task.FromResult<Dictionary<string, List<Domain.Student.Entities.AcademicCredit>>>(academicCreditDict));

                var dpp = await degreePlanService.PreviewSampleDegreePlan4Async(1, programCode, string.Empty);
                // Verify that every term and course on the preview is also on the merged plan in the same term
                foreach (var term in dpp.Preview.Terms)
                {
                    var mergedTerm = dpp.MergedDegreePlan.Terms.Where(t => t.TermId == term.TermId).FirstOrDefault();
                    Assert.IsNotNull(mergedTerm);
                    foreach (var courseId in term.GetPlannedCourseIds())
                    {
                        Assert.IsTrue(mergedTerm.GetPlannedCourseIds().Contains(courseId));
                    }
                }
            }

            [TestMethod]
            public async Task PreviewSamplePlan4Async_IncludesAcademicHistory()
            {
                Domain.Student.Entities.StudentProgram studentProgram = await new TestStudentProgramRepository().GetAsync("0000894", "MATH.BA");
                studentProgramRepoMock.Setup(x => x.GetAsync("0000894", "MATH.BA")).Returns(Task.FromResult(studentProgram));

                var programCode = "MATH.BA";
                var catalogCode = "2013";
                var programReq = await new TestProgramRequirementsRepository().GetAsync(programCode, catalogCode);
                progReqRepoMock.Setup(x => x.GetAsync(programCode, catalogCode)).Returns(Task.FromResult(programReq));

                var currTrack = await new TestSampleDegreePlanRepository().GetAsync("TRACK3");
                currTrackRepoMock.Setup(x => x.GetAsync("TRACK3")).Returns(Task.FromResult(currTrack));

                var dpp = await degreePlanService.PreviewSampleDegreePlan4Async(1, programCode, string.Empty);

                // Verify that academic history is also tacked onto the merged degree plan
                Assert.AreEqual("0000894", dpp.AcademicHistory.StudentId);
                Assert.IsTrue(dpp.AcademicHistory.AcademicTerms.Count() > 0);
                Assert.AreEqual(historyEntity.AcademicTerms.Count(), dpp.AcademicHistory.AcademicTerms.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PreviewSamplePlan4Async_ZeroDegreePlanId()
            {
                var programCode = "EmptyRequirements";
                var dp = await degreePlanService.PreviewSampleDegreePlan4Async(0, programCode, string.Empty);
            }

            [ExpectedException(typeof(ArgumentNullException))]
            [TestMethod]
            public async Task PreviewSamplePlan4Async_NullProgramCode()
            {
                var dp = await degreePlanService.PreviewSampleDegreePlan4Async(1, null, string.Empty);
            }

            [ExpectedException(typeof(ArgumentNullException))]
            [TestMethod]
            public async Task PreviewSamplePlan4Async_EmptyProgramCode()
            {
                var dp = await degreePlanService.PreviewSampleDegreePlan4Async(1, "", string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task PreviewSamplePlan4Async_UnableToGetPlan()
            {
                var programCode = "EmptyRequirements";
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(2)).Throws(new ArgumentException("Degree plan Not Found"));

                var dp = await degreePlanService.PreviewSampleDegreePlan4Async(2, programCode, string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task PreviewSamplePlan4Async_NotAuthorized()
            {
                var programCode = "MATH.BS.BLANKTAKEONE";

                DegreePlan dpApplied3 = new DegreePlan(3, "0000899", 0);
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(3)).Returns(Task.FromResult(dpApplied3));

                var dp = await degreePlanService.PreviewSampleDegreePlan4Async(3, programCode, string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task PreviewSamplePlan4Async_StudentHasNoPrograms()
            {
                var programCode = "MATH.BS.BLANKTAKEONE";

                IEnumerable<Domain.Student.Entities.StudentProgram> noPrograms = new List<Domain.Student.Entities.StudentProgram>();
                studentProgramRepoMock.Setup(x => x.GetAsync("0000894")).ReturnsAsync(noPrograms);

                DegreePlan dpApplied1 = new DegreePlan(1, "0000894", 0);
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(1)).Returns(Task.FromResult(dpApplied1));
                var dp = await degreePlanService.PreviewSampleDegreePlan4Async(1, programCode, string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task PreviewSamplePlan4Async_NoSamplePlanForProgramCode()
            {
                var programCode = "MATH.BA";
                var catalogCode = "2013";
                var programReq = await new TestProgramRequirementsRepository().GetAsync(programCode, catalogCode);
                programReq.CurriculumTrackCode = null;
                progReqRepoMock.Setup(x => x.GetAsync(programCode, catalogCode)).Returns(Task.FromResult(programReq));
                // Mock the planningConfiguration to return no default.
                var planningConfig = new PlanningConfiguration() { DefaultCatalogPolicy = CatalogPolicy.StudentCatalogYear, DefaultCurriculumTrack = "" };
                configRepoMock.Setup(x => x.GetPlanningConfigurationAsync()).Returns(Task.FromResult(planningConfig));

                DegreePlan dpApplied = new DegreePlan(1, "0000894", 0);
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(1)).Returns(Task.FromResult(dpApplied));

                var dp = await degreePlanService.PreviewSampleDegreePlan4Async(1, programCode, string.Empty);
            }

            [TestMethod]
            public async Task PreviewSamplePlan6Async_UsingDefaultSamplePlan()
            {
                Domain.Student.Entities.StudentProgram studentProgram = await new TestStudentProgramRepository().GetAsync("0000894", "MATH.BA");
                studentProgramRepoMock.Setup(x => x.GetAsync("0000894", "MATH.BA")).Returns(Task.FromResult(studentProgram));

                var programCode = "MATH.BA";
                var catalogCode = "2013";
                var programReq = await new TestProgramRequirementsRepository().GetAsync(programCode, catalogCode);
                progReqRepoMock.Setup(x => x.GetAsync(programCode, catalogCode)).Returns(Task.FromResult(programReq));

                var currTrack = await new TestSampleDegreePlanRepository().GetAsync("TRACK3");
                currTrackRepoMock.Setup(x => x.GetAsync("TRACK3")).Returns(Task.FromResult(currTrack));

                // Mock no academic credits because we don't want anything in the academic history to cause something to not be applied.
                var academicCreditDict = new Dictionary<string, List<Domain.Student.Entities.AcademicCredit>>();
                academicCreditRepoMock.Setup(x => x.GetAcademicCreditByStudentIdsAsync(It.IsAny<List<string>>(), false, true, It.IsAny<bool>())).Returns(Task.FromResult<Dictionary<string, List<Domain.Student.Entities.AcademicCredit>>>(academicCreditDict));

                var dpp = await degreePlanService.PreviewSampleDegreePlan6Async(1, programCode, string.Empty);
                Assert.IsInstanceOfType(dpp, typeof(Dtos.Planning.DegreePlanPreview6));
                // Verify that every term and course on the preview is also on the merged plan in the same term
                foreach (var term in dpp.Preview.Terms)
                {
                    var mergedTerm = dpp.MergedDegreePlan.Terms.Where(t => t.TermId == term.TermId).FirstOrDefault();
                    Assert.IsNotNull(mergedTerm);
                    foreach (var courseId in term.GetPlannedCourseIds())
                    {
                        Assert.IsTrue(mergedTerm.GetPlannedCourseIds().Contains(courseId));
                    }
                }
            }
            [TestMethod]
            public async Task PreviewSamplePlan6Async_IncludesAcademicHistory()
            {
                Domain.Student.Entities.StudentProgram studentProgram = await new TestStudentProgramRepository().GetAsync("0000894", "MATH.BA");
                studentProgramRepoMock.Setup(x => x.GetAsync("0000894", "MATH.BA")).Returns(Task.FromResult(studentProgram));

                var programCode = "MATH.BA";
                var catalogCode = "2013";
                var programReq = await new TestProgramRequirementsRepository().GetAsync(programCode, catalogCode);
                progReqRepoMock.Setup(x => x.GetAsync(programCode, catalogCode)).Returns(Task.FromResult(programReq));

                var currTrack = await new TestSampleDegreePlanRepository().GetAsync("TRACK3");
                currTrackRepoMock.Setup(x => x.GetAsync("TRACK3")).Returns(Task.FromResult(currTrack));

                var dpp = await degreePlanService.PreviewSampleDegreePlan6Async(1, programCode, string.Empty);

                // Verify that academic history is also tacked onto the merged degree plan
                Assert.AreEqual("0000894", dpp.AcademicHistory.StudentId);
                Assert.IsTrue(dpp.AcademicHistory.AcademicTerms.Count() > 0);
                Assert.AreEqual(historyEntity.AcademicTerms.Count(), dpp.AcademicHistory.AcademicTerms.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PreviewSamplePlan6Async_ZeroDegreePlanId()
            {
                var programCode = "EmptyRequirements";
                var dp = await degreePlanService.PreviewSampleDegreePlan6Async(0, programCode, string.Empty);
            }

            [ExpectedException(typeof(ArgumentNullException))]
            [TestMethod]
            public async Task PreviewSamplePlan6Async_NullProgramCode()
            {
                var dp = await degreePlanService.PreviewSampleDegreePlan6Async(1, null, string.Empty);
            }

            [ExpectedException(typeof(ArgumentNullException))]
            [TestMethod]
            public async Task PreviewSamplePlan6Async_EmptyProgramCode()
            {
                var dp = await degreePlanService.PreviewSampleDegreePlan6Async(1, "", string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task PreviewSamplePlan6Async_UnableToGetPlan()
            {
                var programCode = "EmptyRequirements";
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(2)).Throws(new ArgumentException("Degree plan Not Found"));

                var dp = await degreePlanService.PreviewSampleDegreePlan6Async(2, programCode, string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task PreviewSamplePlan6Async_NotAuthorized()
            {
                var programCode = "MATH.BS.BLANKTAKEONE";

                DegreePlan dpApplied3 = new DegreePlan(3, "0000899", 0);
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(3)).Returns(Task.FromResult(dpApplied3));

                var dp = await degreePlanService.PreviewSampleDegreePlan6Async(3, programCode, string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task PreviewSamplePlan6Async_StudentHasNoPrograms()
            {
                var programCode = "MATH.BS.BLANKTAKEONE";

                IEnumerable<Domain.Student.Entities.StudentProgram> noPrograms = new List<Domain.Student.Entities.StudentProgram>();
                studentProgramRepoMock.Setup(x => x.GetAsync("0000894")).ReturnsAsync(noPrograms);

                DegreePlan dpApplied1 = new DegreePlan(1, "0000894", 0);
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(1)).Returns(Task.FromResult(dpApplied1));
                var dp = await degreePlanService.PreviewSampleDegreePlan6Async(1, programCode, string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task PreviewSamplePlan6Async_NoSamplePlanForProgramCode()
            {
                var programCode = "MATH.BA";
                var catalogCode = "2013";
                var programReq = await new TestProgramRequirementsRepository().GetAsync(programCode, catalogCode);
                programReq.CurriculumTrackCode = null;
                progReqRepoMock.Setup(x => x.GetAsync(programCode, catalogCode)).Returns(Task.FromResult(programReq));
                // Mock the planningConfiguration to return no default.
                var planningConfig = new PlanningConfiguration() { DefaultCatalogPolicy = CatalogPolicy.StudentCatalogYear, DefaultCurriculumTrack = "" };
                configRepoMock.Setup(x => x.GetPlanningConfigurationAsync()).Returns(Task.FromResult(planningConfig));

                DegreePlan dpApplied = new DegreePlan(1, "0000894", 0);
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(1)).Returns(Task.FromResult(dpApplied));

                var dp = await degreePlanService.PreviewSampleDegreePlan6Async(1, programCode, string.Empty);
            }
        }

        [TestClass]
        public class DegreePlanService_TestAdvisorPermissions : DegreePlanServiceTestsSetup
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

                var degreePlanPreviewDtoAdapter = new AutoMapperAdapter<DegreePlanPreview, Dtos.Planning.DegreePlanPreview>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<DegreePlanPreview, Dtos.Planning.DegreePlanPreview>()).Returns(degreePlanPreviewDtoAdapter);

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

                // Mock degree plan service
                degreePlanService = new DegreePlanService(
                    adapterRegistry, degreePlanRepo, termRepo, studentRepo, planningStudentRepo, studentProgramRepo,
                    courseRepo, sectionRepo, programRepo, academicCreditRepo, requirementRepo, ruleRepo, progReqRepo,
                    currTrackRepo, configRepo, catalogRepo, null, null, gradeRepo, academicHistoryService, studentDegreePlanRepo, studentDegreePlanService,
                    currentUserFactory, roleRepo, logger, baseConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                SetupCleanup();
            }

            [TestMethod]
            public async Task GetDegreePlanAsync_AllowedForAdvisorWithViewPermissions()
            {
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

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task GetDegreePlanAsync_ThrowsErrorForAdvisorWithoutViewPermissions()
            {
#pragma warning disable 618
                await studentDegreePlanService.GetDegreePlanAsync(2);
#pragma warning restore 618
            }

            [TestMethod]
            public async Task CreateDegreePlanAsync_AllowedForAdvisorWithUpdatePermissions()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.UpdateAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock the get from the student repository (Create tests)
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));
                studentRepoMock.Setup(x => x.GetAsync("0000896")).Returns(Task.FromResult(student));


                // Mock the get of the planning terms (Create tests)
                var allTerms = await new TestTermRepository().GetAsync();
                termRepoMock.Setup(x => x.GetAsync()).ReturnsAsync(allTerms);

                // Mock the get of the student programs (Create tests)
                var studentPrograms = await new TestStudentProgramRepository().GetAsync("0000896");
                studentProgramRepoMock.Setup(x => x.GetAsync("0000896")).ReturnsAsync(studentPrograms);
                studentProgramRepoMock.Setup(x => x.GetAsync("0000894")).ReturnsAsync(studentPrograms);

                // Mock an empty program 
                IEnumerable<string> depts = new List<string>() { "Dept1" };
                IEnumerable<Domain.Student.Entities.Requirements.Program> program =
                    new List<Domain.Student.Entities.Requirements.Program>() { new Domain.Student.Entities.Requirements.Program("EmptyRequirements", "Empty Requirements", depts, true, "UG", new CreditFilter(), false) };
                programRepoMock.Setup(pr => pr.GetAsync()).Returns(Task.FromResult(program));

                // Attempt to get degree plan
                await studentDegreePlanService.CreateDegreePlanAsync("0000894");
            }

            [TestMethod]
            public async Task CreateDegreePlanAsync_AllowedForAdvisorWithReviewPermissions()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ReviewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock the get from the student repository (Create tests)
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));
                studentRepoMock.Setup(x => x.GetAsync("0000896")).Returns(Task.FromResult(student));

                // Mock the get of the planning terms (Create tests)
                var allTerms = await new TestTermRepository().GetAsync();
                termRepoMock.Setup(x => x.GetAsync()).ReturnsAsync(allTerms);

                // Mock the get of the student programs (Create tests)
                var studentPrograms = await new TestStudentProgramRepository().GetAsync("0000896");
                studentProgramRepoMock.Setup(x => x.GetAsync("0000896")).ReturnsAsync(studentPrograms);
                studentProgramRepoMock.Setup(x => x.GetAsync("0000894")).ReturnsAsync(studentPrograms);

                // Mock an empty program 
                IEnumerable<string> depts = new List<string>() { "Dept1" };
                IEnumerable<Domain.Student.Entities.Requirements.Program> program =
                    new List<Domain.Student.Entities.Requirements.Program>() { new Domain.Student.Entities.Requirements.Program("EmptyRequirements", "Empty Requirements", depts, true, "UG", new CreditFilter(), false) };
                programRepoMock.Setup(pr => pr.GetAsync()).Returns(Task.FromResult(program));

                // Attempt to get degree plan
                await studentDegreePlanService.CreateDegreePlanAsync("0000894");
            }

            [TestMethod]
            public async Task CreateDegreePlanAsync_AllowedForAdvisorWithViewPermissions()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock the get from the student repository (Create tests)
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));
                studentRepoMock.Setup(x => x.GetAsync("0000896")).Returns(Task.FromResult(student));

                // Mock the get of the planning terms (Create tests)
                var allTerms = await new TestTermRepository().GetAsync();
                termRepoMock.Setup(x => x.GetAsync()).ReturnsAsync(allTerms);

                // Mock the get of the student programs (Create tests)
                var studentPrograms = await new TestStudentProgramRepository().GetAsync("0000896");
                studentProgramRepoMock.Setup(x => x.GetAsync("0000896")).ReturnsAsync(studentPrograms);
                studentProgramRepoMock.Setup(x => x.GetAsync("0000894")).ReturnsAsync(studentPrograms);

                // Mock an empty program 
                IEnumerable<string> depts = new List<string>() { "Dept1" };
                IEnumerable<Domain.Student.Entities.Requirements.Program> program =
                    new List<Domain.Student.Entities.Requirements.Program>() { new Domain.Student.Entities.Requirements.Program("EmptyRequirements", "Empty Requirements", depts, true, "UG", new CreditFilter(), false) };
                programRepoMock.Setup(pr => pr.GetAsync()).Returns(Task.FromResult(program));

                // Attempt to get degree plan
                await studentDegreePlanService.CreateDegreePlanAsync("0000894");
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CreateDegreePlanAsync_ThrowsErrorForAdvisorWithoutCorrectPermissions()
            {
                // Attempt to get degree plan
                await studentDegreePlanService.CreateDegreePlanAsync("0000894");
            }

            [TestMethod]
            public async Task ApplySampleAsync_AllowedForAdvisorWithUpdatePermissions()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.UpdateAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Arrange-Mock program requirement with curriculum track
                var programCode = "EmptyRequirements";
                var catalogCode = "2012";
                var programReq = await new TestProgramRequirementsRepository().GetAsync(programCode, catalogCode);
                progReqRepoMock.Setup(x => x.GetAsync(programCode, catalogCode)).Returns(Task.FromResult(programReq));
                var currTrack = await new TestSampleDegreePlanRepository().GetAsync("TRACK1");
                currTrackRepoMock.Setup(x => x.GetAsync("TRACK1")).Returns(Task.FromResult(currTrack));

                var terms = await new TestTermRepository().GetAsync();
                termRepoMock.Setup(x => x.GetAsync()).ReturnsAsync(terms);
                var courses = await new TestCourseRepository().GetAsync();
                courseRepoMock.Setup(x => x.GetAsync()).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Course>>(courses));

                // Mock student repository get
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));

                // Mock finding the sample plan's program as one of the student's current "programs". 
                var emptyRequirementsStudentProgram = new Domain.Student.Entities.StudentProgram("0000894", "EmptyRequirements", "2012");
                studentProgramRepoMock.Setup(spr => spr.GetAsync(It.IsAny<string>(), "EmptyRequirements")).Returns(Task.FromResult(emptyRequirementsStudentProgram));

                // Mock successful add of a plan from repository
                DegreePlan degreePlan = new DegreePlan(1, "0000894", 0);
                degreePlan.AddTerm("2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Domain.Student.Entities.DegreePlans.DegreePlan>())).Returns(Task.FromResult(degreePlan));
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<List<string>>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.DegreePlans.DegreePlan>>(new List<Domain.Student.Entities.DegreePlans.DegreePlan>() { degreePlan }));

                // Mock entity adapter
                var degreePlanDtoToEntityAdapter = new DegreePlanDtoAdapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Dtos.Student.DegreePlans.DegreePlan, Domain.Student.Entities.DegreePlans.DegreePlan>()).Returns(degreePlanDtoToEntityAdapter);

                // Apply the curriculum track and set resulting plan as the update response
                DegreePlan dpApplied = new DegreePlan(1, "0000894", 0);
                SampleDegreePlanService.ApplySample(ref dpApplied, currTrack, terms, activeAcademicCredits, null, null);
                studentDegreePlanRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Domain.Student.Entities.DegreePlans.DegreePlan>())).Returns(Task.FromResult(dpApplied));

                // Mock student degree plan service
                studentDegreePlanService = new StudentDegreePlanService(
                    adapterRegistry, studentDegreePlanRepo, termRepo, studentRepo, studentProgramRepo, courseRepo, sectionRepo, programRepo,
                    academicCreditRepo, requirementRepo, ruleRepo, progReqRepo, catalogRepo, gradeRepo, academicHistoryService,
                    currentUserFactory, roleRepo, logger, baseConfigurationRepository, studentConfigurationRepository);

                // Mock degree plan service
                degreePlanService = new DegreePlanService(
                    adapterRegistry, degreePlanRepo, termRepo, studentRepo, planningStudentRepo, studentProgramRepo,
                    courseRepo, sectionRepo, programRepo, academicCreditRepo, requirementRepo, ruleRepo, progReqRepo,
                    currTrackRepo, configRepo, catalogRepo, null, null, gradeRepo, academicHistoryService, studentDegreePlanRepo, studentDegreePlanService,
                    currentUserFactory, roleRepo, logger, baseConfigurationRepository);

                // Act--Apply sample degree plan
#pragma warning disable 618
                var dp = await degreePlanService.ApplySampleDegreePlanAsync(dpApplied.PersonId, programCode);
#pragma warning restore 618
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task ApplySampleAsync_ThrowsErrorForAdvisorWithoutUpdatePermissions()
            {
#pragma warning disable 618
                await degreePlanService.ApplySampleDegreePlanAsync("0000894", "BA.ENGL");
#pragma warning restore 618
            }

            [TestMethod]
            public async Task UpdateDegreePlanAsync_AllowedForAdvisorWithUpdateAnyPermissions()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.UpdateAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock successful update of a plan from repository
                DegreePlan degreePlan = new DegreePlan(1, "0000894", 0);
                degreePlan.AddTerm("2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Domain.Student.Entities.DegreePlans.DegreePlan>())).Returns(Task.FromResult(degreePlan));
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<List<string>>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.DegreePlans.DegreePlan>>(new List<Domain.Student.Entities.DegreePlans.DegreePlan>() { degreePlan }));

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
                var degreePlanDto = degreePlanEntityToDtoAdapter.MapToType(degreePlan);
#pragma warning disable 618
                await studentDegreePlanService.UpdateDegreePlanAsync(degreePlanDto);
#pragma warning restore 618
            }

            public async Task UpdateDegreePlanAsync_AllowedForAdvisorWithUpdateAssignedPermissions()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.UpdateAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock successful update of a plan from repository
                DegreePlan degreePlan = new DegreePlan(1, "0000894", 0);
                degreePlan.AddTerm("2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Domain.Student.Entities.DegreePlans.DegreePlan>())).Returns(Task.FromResult(degreePlan));
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<List<string>>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.DegreePlans.DegreePlan>>(new List<Domain.Student.Entities.DegreePlans.DegreePlan>() { degreePlan }));

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
                var degreePlanDto = degreePlanEntityToDtoAdapter.MapToType(degreePlan);
#pragma warning disable 618
                await studentDegreePlanService.UpdateDegreePlanAsync(degreePlanDto);
#pragma warning restore 618
            }

            [ExpectedException(typeof(PermissionsException))]
            public async Task UpdateDegreePlanAsync_UserNotAssignedAdvisor()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.UpdateAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock student repository get
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                student.AddAdvisor("0000222");
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));

                // Mock successful update of a plan from repository
                DegreePlan degreePlan = new DegreePlan(1, "0000894", 0);
                degreePlan.AddTerm("2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Domain.Student.Entities.DegreePlans.DegreePlan>())).Returns(Task.FromResult(degreePlan));
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<List<string>>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.DegreePlans.DegreePlan>>(new List<Domain.Student.Entities.DegreePlans.DegreePlan>() { degreePlan }));

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
                var degreePlanDto = degreePlanEntityToDtoAdapter.MapToType(degreePlan);
#pragma warning disable 618
                await studentDegreePlanService.UpdateDegreePlanAsync(degreePlanDto);
#pragma warning restore 618
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task UpdateDegreePlanAsync_ThrowsErrorForAdvisorOnlyViewPermissions()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock successful update of a plan from repository
                DegreePlan degreePlan = new DegreePlan(1, "0000894", 0);
                degreePlan.AddTerm("2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Domain.Student.Entities.DegreePlans.DegreePlan>())).Returns(Task.FromResult(degreePlan));
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<List<string>>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.DegreePlans.DegreePlan>>(new List<Domain.Student.Entities.DegreePlans.DegreePlan>() { degreePlan }));

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
                var degreePlanDto = degreePlanEntityToDtoAdapter.MapToType(degreePlan);
#pragma warning disable 618
                await studentDegreePlanService.UpdateDegreePlanAsync(degreePlanDto);
#pragma warning restore 618
            }
        }

        [TestClass]
        public class DegreePlanService_TestAdvisorPermissions2 : DegreePlanServiceTestsSetup
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

                // Mock academic credit repository get using student ids returning dict with student's academic credits
                activeAcademicCredits = new List<Domain.Student.Entities.AcademicCredit>();
                var academicCreditDict = new Dictionary<string, List<Domain.Student.Entities.AcademicCredit>>();
                academicCreditDict["0000894"] = activeAcademicCredits.ToList();
                academicCreditRepoMock.Setup(x => x.GetAcademicCreditByStudentIdsAsync(It.IsAny<List<string>>(), false, true, It.IsAny<bool>())).Returns(Task.FromResult<Dictionary<string, List<Domain.Student.Entities.AcademicCredit>>>(academicCreditDict));

                // Mock get of all sections for the registration terms
                var regTerms = await new TestTermRepository().GetRegistrationTermsAsync();
                termRepoMock.Setup(x => x.GetRegistrationTermsAsync()).Returns(Task.FromResult(regTerms));
                var allSections = await new TestSectionRepository().GetRegistrationSectionsAsync(regTerms);
                sectionRepoMock.Setup(x => x.GetRegistrationSectionsAsync(It.IsAny<IEnumerable<Domain.Student.Entities.Term>>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(allSections));

                // Mock get of grade repo data
                var grades = await new TestGradeRepository().GetAsync();
                gradeRepoMock.Setup(x => x.GetAsync()).Returns(Task.FromResult(grades));
                // Mock get of all courses
                var courses = await new TestCourseRepository().GetAsync();
                courseRepoMock.Setup(x => x.GetAsync()).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Course>>(courses));

                // Mock get requirements for requisite validation 
                var requirementCodes = new List<string>() { };
                var requirements = (await new TestRequirementRepository().GetAsync(requirementCodes)).Where(r => r.RequirementType.Code == "PRE");
                requirementRepoMock.Setup(x => x.GetAsync(It.IsAny<IEnumerable<string>>())).Returns(Task.FromResult(requirements));

                // Mock DateTimeOffset->DateTime adapter to verify backward compatibility
                var datetimeOffsetAdapter = new DateTimeOffsetToDateTimeAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<System.DateTimeOffset, System.DateTime>()).Returns(datetimeOffsetAdapter);

                // Mock Adapter 
                var degreePlanPreviewDtoAdapter = new AutoMapperAdapter<DegreePlanPreview, Dtos.Planning.DegreePlanPreview2>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<DegreePlanPreview, Dtos.Planning.DegreePlanPreview2>()).Returns(degreePlanPreviewDtoAdapter);

                var degreePlanDtoAdapter = new AutoMapperAdapter<DegreePlan, Dtos.Student.DegreePlans.DegreePlan2>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<DegreePlan, Dtos.Student.DegreePlans.DegreePlan2>()).Returns(degreePlanDtoAdapter);

                var degreePlanApprovalDtoAdapter = new AutoMapperAdapter<DegreePlanApproval, Dtos.Student.DegreePlans.DegreePlanApproval2>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<DegreePlanApproval, Dtos.Student.DegreePlans.DegreePlanApproval2>()).Returns(degreePlanApprovalDtoAdapter);

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
            public async Task GetDegreePlanAsync_AllowedForAdvisorWithViewPermissions()
            {
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

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task GetDegreePlanAsync_ThrowsErrorForAdvisorWithoutViewPermissions()
            {
#pragma warning disable 618
                await studentDegreePlanService.GetDegreePlan2Async(2);
#pragma warning restore 618
            }

            [TestMethod]
            public async Task CreateDegreePlan_AllowedForAdvisorWithUpdatePermissions()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.UpdateAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock the get from the student repository (Create tests)
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));
                studentRepoMock.Setup(x => x.GetAsync("0000896")).Returns(Task.FromResult(student));

                // Mock the get of the planning terms (Create tests)
                var allTerms = await new TestTermRepository().GetAsync();
                termRepoMock.Setup(x => x.GetAsync()).ReturnsAsync(allTerms);

                // Mock the get of the student programs (Create tests)
                var studentPrograms = await new TestStudentProgramRepository().GetAsync("0000896");
                studentProgramRepoMock.Setup(x => x.GetAsync("0000896")).ReturnsAsync(studentPrograms);
                studentProgramRepoMock.Setup(x => x.GetAsync("0000894")).ReturnsAsync(studentPrograms);

                // Mock an empty program 
                IEnumerable<string> depts = new List<string>() { "Dept1" };
                IEnumerable<Domain.Student.Entities.Requirements.Program> program =
                    new List<Domain.Student.Entities.Requirements.Program>() { new Domain.Student.Entities.Requirements.Program("EmptyRequirements", "Empty Requirements", depts, true, "UG", new CreditFilter(), false) };
                programRepoMock.Setup(pr => pr.GetAsync()).Returns(Task.FromResult(program));

                // Attempt to get degree plan
#pragma warning disable 618
                await studentDegreePlanService.CreateDegreePlan2Async("0000894");
#pragma warning restore 618
            }

            [TestMethod]
            public async Task CreateDegreePlanAsync_AllowedForAdvisorWithReviewPermissions()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ReviewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock the get from the student repository (Create tests)
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));
                studentRepoMock.Setup(x => x.GetAsync("0000896")).Returns(Task.FromResult(student));

                // Mock the get of the planning terms (Create tests)
                var allTerms = await new TestTermRepository().GetAsync();
                termRepoMock.Setup(x => x.GetAsync()).ReturnsAsync(allTerms);

                // Mock the get of the student programs (Create tests)
                var studentPrograms = await new TestStudentProgramRepository().GetAsync("0000896");
                studentProgramRepoMock.Setup(x => x.GetAsync("0000896")).ReturnsAsync(studentPrograms);
                studentProgramRepoMock.Setup(x => x.GetAsync("0000894")).ReturnsAsync(studentPrograms);

                // Mock an empty program 
                IEnumerable<string> depts = new List<string>() { "Dept1" };
                IEnumerable<Domain.Student.Entities.Requirements.Program> program =
                    new List<Domain.Student.Entities.Requirements.Program>() { new Domain.Student.Entities.Requirements.Program("EmptyRequirements", "Empty Requirements", depts, true, "UG", new CreditFilter(), false) };
                programRepoMock.Setup(pr => pr.GetAsync()).Returns(Task.FromResult(program));

                // Attempt to get degree plan
#pragma warning disable 618
                await studentDegreePlanService.CreateDegreePlan2Async("0000894");
#pragma warning restore 618
            }

            [TestMethod]
            public async Task CreateDegreePlanAsync_AllowedForAdvisorWithViewPermissions()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock the get from the student repository (Create tests)
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));
                studentRepoMock.Setup(x => x.GetAsync("0000896")).Returns(Task.FromResult(student));

                // Mock the get of the planning terms (Create tests)
                var allTerms = await new TestTermRepository().GetAsync();
                termRepoMock.Setup(x => x.GetAsync()).ReturnsAsync(allTerms);

                // Mock the get of the student programs (Create tests)
                var studentPrograms = await new TestStudentProgramRepository().GetAsync("0000896");
                studentProgramRepoMock.Setup(x => x.GetAsync("0000896")).ReturnsAsync(studentPrograms);
                studentProgramRepoMock.Setup(x => x.GetAsync("0000894")).ReturnsAsync(studentPrograms);

                // Mock an empty program 
                IEnumerable<string> depts = new List<string>() { "Dept1" };
                IEnumerable<Domain.Student.Entities.Requirements.Program> program =
                    new List<Domain.Student.Entities.Requirements.Program>() { new Domain.Student.Entities.Requirements.Program("EmptyRequirements", "Empty Requirements", depts, true, "UG", new CreditFilter(), false) };
                programRepoMock.Setup(pr => pr.GetAsync()).Returns(Task.FromResult(program));

                // Attempt to get degree plan
#pragma warning disable 618
                await studentDegreePlanService.CreateDegreePlan2Async("0000894");
#pragma warning restore 618
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CreateDegreePlanAsync_ThrowsErrorForAdvisorWithoutCorrectPermissions()
            {
                // Attempt to get degree plan
#pragma warning disable 618
                await studentDegreePlanService.CreateDegreePlan2Async("0000894");
#pragma warning restore 618
            }

            [TestMethod]
            public async Task UpdateDegreePlan2Async_AllowedForAdvisorWithUpdateAnyPermissions()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.UpdateAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock successful update of a plan from repository
                DegreePlan degreePlan = new DegreePlan(1, "0000894", 0);
                degreePlan.AddTerm("2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Domain.Student.Entities.DegreePlans.DegreePlan>())).Returns(Task.FromResult(degreePlan));
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<List<string>>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.DegreePlans.DegreePlan>>(new List<Domain.Student.Entities.DegreePlans.DegreePlan>() { degreePlan }));

                // Mock successful get of a plan from repository so the Missing Protection checks can be done.
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<int>())).Returns(Task.FromResult(degreePlan));

                // Mock dto Adapter 
                var degreePlanEntityToDtoAdapter = new DegreePlanEntity2Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan2>()).Returns(degreePlanEntityToDtoAdapter);

                // Mock entity adapter
                var degreePlanDtoToEntityAdapter = new DegreePlanDto2Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Dtos.Student.DegreePlans.DegreePlan2, Domain.Student.Entities.DegreePlans.DegreePlan>()).Returns(degreePlanDtoToEntityAdapter);

                // Map degree plan to dto
                var degreePlanDto = degreePlanEntityToDtoAdapter.MapToType(degreePlan);
#pragma warning disable 618
                await studentDegreePlanService.UpdateDegreePlan2Async(degreePlanDto);
#pragma warning restore 618
            }

            public async Task UpdateDegreePlanAsync_AllowedForAdvisorWithUpdateAssignedPermissions()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.UpdateAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock successful update of a plan from repository
                DegreePlan degreePlan = new DegreePlan(1, "0000894", 0);
                degreePlan.AddTerm("2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Domain.Student.Entities.DegreePlans.DegreePlan>())).Returns(Task.FromResult(degreePlan));
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<List<string>>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.DegreePlans.DegreePlan>>(new List<Domain.Student.Entities.DegreePlans.DegreePlan>() { degreePlan }));

                // Mock dto Adapter 
                var degreePlanEntityToDtoAdapter = new DegreePlanEntity2Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan2>()).Returns(degreePlanEntityToDtoAdapter);

                // Mock entity adapter
                var degreePlanDtoToEntityAdapter = new DegreePlanDto2Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Dtos.Student.DegreePlans.DegreePlan2, Domain.Student.Entities.DegreePlans.DegreePlan>()).Returns(degreePlanDtoToEntityAdapter);

                // Map degree plan to dto
                var degreePlanDto = degreePlanEntityToDtoAdapter.MapToType(degreePlan);

#pragma warning disable 618
                await studentDegreePlanService.UpdateDegreePlan2Async(degreePlanDto);
#pragma warning restore 618
            }

            [ExpectedException(typeof(PermissionsException))]
            public async Task UpdateDegreePlan_UserNotAssignedAdvisor()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.UpdateAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock student repository get
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                student.AddAdvisor("0000222");
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));

                // Mock successful update of a plan from repository
                DegreePlan degreePlan = new DegreePlan(1, "0000894", 0);
                degreePlan.AddTerm("2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Domain.Student.Entities.DegreePlans.DegreePlan>())).Returns(Task.FromResult(degreePlan));
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<List<string>>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.DegreePlans.DegreePlan>>(new List<Domain.Student.Entities.DegreePlans.DegreePlan>() { degreePlan }));

                // Mock dto Adapter 
                var degreePlanEntityToDtoAdapter = new DegreePlanEntity2Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan2>()).Returns(degreePlanEntityToDtoAdapter);

                // Mock entity adapter
                var degreePlanDtoToEntityAdapter = new DegreePlanDto2Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Dtos.Student.DegreePlans.DegreePlan2, Domain.Student.Entities.DegreePlans.DegreePlan>()).Returns(degreePlanDtoToEntityAdapter);

                // Map degree plan to dto
                var degreePlanDto = degreePlanEntityToDtoAdapter.MapToType(degreePlan);
#pragma warning disable 618
                await studentDegreePlanService.UpdateDegreePlan2Async(degreePlanDto);
#pragma warning restore 618
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task UpdateDegreePlanAsync_ThrowsErrorForAdvisorOnlyViewPermissions()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock successful update of a plan from repository
                DegreePlan degreePlan = new DegreePlan(1, "0000894", 0);
                degreePlan.AddTerm("2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Domain.Student.Entities.DegreePlans.DegreePlan>())).Returns(Task.FromResult(degreePlan));
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<List<string>>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.DegreePlans.DegreePlan>>(new List<Domain.Student.Entities.DegreePlans.DegreePlan>() { degreePlan }));

                // Mock successful get of a plan from repository so the Missing Protection checks can be done.
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<int>())).Returns(Task.FromResult(degreePlan));

                // Mock dto Adapter 
                var degreePlanEntityToDtoAdapter = new DegreePlanEntity2Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan2>()).Returns(degreePlanEntityToDtoAdapter);

                // Mock entity adapter
                var degreePlanDtoToEntityAdapter = new DegreePlanDto2Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Dtos.Student.DegreePlans.DegreePlan2, Domain.Student.Entities.DegreePlans.DegreePlan>()).Returns(degreePlanDtoToEntityAdapter);

                // Map degree plan to dto
                var degreePlanDto = degreePlanEntityToDtoAdapter.MapToType(degreePlan);
#pragma warning disable 618
                await studentDegreePlanService.UpdateDegreePlan2Async(degreePlanDto);
#pragma warning restore 618
            }
        }

        [TestClass]
        public class DegreePlanService_TestAdvisorPermissions3 : DegreePlanServiceTestsSetup
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
                academicCreditDict["0000894"] = activeAcademicCredits.ToList();
                academicCreditRepoMock.Setup(x => x.GetAcademicCreditByStudentIdsAsync(It.IsAny<List<string>>(), false, true, It.IsAny<bool>())).Returns(Task.FromResult<Dictionary<string, List<Domain.Student.Entities.AcademicCredit>>>(academicCreditDict));

                // Mock get of all sections for the registration terms
                var regTerms = await new TestTermRepository().GetRegistrationTermsAsync();
                termRepoMock.Setup(x => x.GetRegistrationTermsAsync()).Returns(Task.FromResult(regTerms));
                var allSections = await new TestSectionRepository().GetRegistrationSectionsAsync(regTerms);
                sectionRepoMock.Setup(x => x.GetRegistrationSectionsAsync(It.IsAny<IEnumerable<Domain.Student.Entities.Term>>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(allSections));

                // Mock get of grade repo data
                var grades = await new TestGradeRepository().GetAsync();
                gradeRepoMock.Setup(x => x.GetAsync()).Returns(Task.FromResult(grades));

                // Mock get of all courses
                var courses = await new TestCourseRepository().GetAsync();
                courseRepoMock.Setup(x => x.GetAsync()).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Course>>(courses));

                // Mock get requirements for requisite validation 
                var requirementCodes = new List<string>() { };
                var requirements = (await new TestRequirementRepository().GetAsync(requirementCodes)).Where(r => r.RequirementType.Code == "PRE");
                requirementRepoMock.Setup(x => x.GetAsync(It.IsAny<IEnumerable<string>>())).Returns(Task.FromResult(requirements));

                // Mock Adapter 
                var degreePlanPreviewDtoAdapter = new AutoMapperAdapter<DegreePlanPreview, Dtos.Planning.DegreePlanPreview3>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<DegreePlanPreview, Dtos.Planning.DegreePlanPreview3>()).Returns(degreePlanPreviewDtoAdapter);
                var degreePlanDtoAdapter = new AutoMapperAdapter<DegreePlan, Dtos.Student.DegreePlans.DegreePlan3>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<DegreePlan, Dtos.Student.DegreePlans.DegreePlan3>()).Returns(degreePlanDtoAdapter);
                var degreePlanApprovalDtoAdapter = new AutoMapperAdapter<DegreePlanApproval, Dtos.Student.DegreePlans.DegreePlanApproval2>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<DegreePlanApproval, Dtos.Student.DegreePlans.DegreePlanApproval2>()).Returns(degreePlanApprovalDtoAdapter);

                // Mock degree plan3 dto Adapter 
                var degreePlanEntityToDegreePlan3DtoAdapter = new AutoMapperAdapter<DegreePlan, Dtos.Student.DegreePlans.DegreePlan3>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<DegreePlan, Dtos.Student.DegreePlans.DegreePlan3>()).Returns(degreePlanEntityToDegreePlan3DtoAdapter);

                // Mock conversion from academic credits to academic history
                var studentAcademicCredits = await new TestAcademicCreditRepository().GetAsync();
                Domain.Student.Entities.GradeRestriction gradeRestriction = new Domain.Student.Entities.GradeRestriction(false);
                var historyEntity = new Domain.Student.Entities.AcademicHistory(studentAcademicCredits, gradeRestriction, null);
                historyEntity.StudentId = "0000894";
                var historyDto = new AcademicHistoryEntityToAcademicHistory2DtoAdapter(adapterRegistry, logger).MapToType(historyEntity);
                academicHistoryServiceMock.Setup(svc => svc.ConvertAcademicCreditsToAcademicHistoryDtoAsync(historyEntity.StudentId, It.IsAny<IEnumerable<Domain.Student.Entities.AcademicCredit>>(), It.IsAny<Domain.Student.Entities.Student>()))
                    .Returns(Task.FromResult(historyDto));

                // Mock student degree plan service
                studentDegreePlanService = new StudentDegreePlanService(
                    adapterRegistry, studentDegreePlanRepo, termRepo, studentRepo, studentProgramRepo, courseRepo, sectionRepo, programRepo,
                    academicCreditRepo, requirementRepo, ruleRepo, progReqRepo, catalogRepo, gradeRepo, academicHistoryService,
                    currentUserFactory, roleRepo, logger, baseConfigurationRepository, studentConfigurationRepository);

                // Mock degree plan service
                degreePlanService = new DegreePlanService(
                    adapterRegistry, degreePlanRepo, termRepo, studentRepo, planningStudentRepo, studentProgramRepo,
                    courseRepo, sectionRepo, programRepo, academicCreditRepo, requirementRepo, ruleRepo, progReqRepo,
                    currTrackRepo, configRepo, catalogRepo, null, null, gradeRepo, academicHistoryService, studentDegreePlanRepo, studentDegreePlanService,
                    currentUserFactory, roleRepo, logger, baseConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                SetupCleanup();
            }

            [TestMethod]
            public async Task GetDegreePlanAsync_AllowedForAdvisorWithViewPermissions()
            {
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

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task GetDegreePlanAsync_ThrowsErrorForAdvisorWithoutViewPermissions()
            {
                await studentDegreePlanService.GetDegreePlan3Async(2);
            }

            [TestMethod]
            public async Task CreateDegreePlanAsync_AllowedForAdvisorWithUpdatePermissions()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.UpdateAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock the get from the student repository (Create tests)
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));
                studentRepoMock.Setup(x => x.GetAsync("0000896")).Returns(Task.FromResult(student));

                // Mock the get of the planning terms (Create tests)
                var allTerms = await new TestTermRepository().GetAsync();
                termRepoMock.Setup(x => x.GetAsync()).ReturnsAsync(allTerms);

                // Mock the get of the student programs (Create tests)
                var studentPrograms = await new TestStudentProgramRepository().GetAsync("0000896");
                studentProgramRepoMock.Setup(x => x.GetAsync("0000896")).ReturnsAsync(studentPrograms);
                studentProgramRepoMock.Setup(x => x.GetAsync("0000894")).ReturnsAsync(studentPrograms);

                // Mock an empty program 
                IEnumerable<string> depts = new List<string>() { "Dept1" };
                IEnumerable<Domain.Student.Entities.Requirements.Program> program =
                    new List<Domain.Student.Entities.Requirements.Program>() { new Domain.Student.Entities.Requirements.Program("EmptyRequirements", "Empty Requirements", depts, true, "UG", new CreditFilter(), false) };
                programRepoMock.Setup(pr => pr.GetAsync()).Returns(Task.FromResult(program));

                // Attempt to get degree plan
                await studentDegreePlanService.CreateDegreePlan3Async("0000894");
            }

            [TestMethod]
            public async Task CreateDegreePlanAsync_AllowedForAdvisorWithReviewPermissions()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ReviewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock the get from the student repository (Create tests)
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));
                studentRepoMock.Setup(x => x.GetAsync("0000896")).Returns(Task.FromResult(student));

                // Mock the get of the planning terms (Create tests)
                var allTerms = await new TestTermRepository().GetAsync();
                termRepoMock.Setup(x => x.GetAsync()).ReturnsAsync(allTerms);

                // Mock the get of the student programs (Create tests)
                var studentPrograms = await new TestStudentProgramRepository().GetAsync("0000896");
                studentProgramRepoMock.Setup(x => x.GetAsync("0000896")).ReturnsAsync(studentPrograms);
                studentProgramRepoMock.Setup(x => x.GetAsync("0000894")).ReturnsAsync(studentPrograms);

                // Mock an empty program 
                IEnumerable<string> depts = new List<string>() { "Dept1" };
                IEnumerable<Domain.Student.Entities.Requirements.Program> program =
                    new List<Domain.Student.Entities.Requirements.Program>() { new Domain.Student.Entities.Requirements.Program("EmptyRequirements", "Empty Requirements", depts, true, "UG", new CreditFilter(), false) };
                programRepoMock.Setup(pr => pr.GetAsync()).Returns(Task.FromResult(program));

                // Attempt to get degree plan
                await studentDegreePlanService.CreateDegreePlan3Async("0000894");
            }

            [TestMethod]
            public async Task CreateDegreePlanAsync_AllowedForAdvisorWithViewPermissions()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock the get from the student repository (Create tests)
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));
                studentRepoMock.Setup(x => x.GetAsync("0000896")).Returns(Task.FromResult(student));

                // Mock the get of the planning terms (Create tests)
                var allTerms = await new TestTermRepository().GetAsync();
                termRepoMock.Setup(x => x.GetAsync()).ReturnsAsync(allTerms);

                // Mock the get of the student programs (Create tests)
                var studentPrograms = await new TestStudentProgramRepository().GetAsync("0000896");
                studentProgramRepoMock.Setup(x => x.GetAsync("0000896")).ReturnsAsync(studentPrograms);
                studentProgramRepoMock.Setup(x => x.GetAsync("0000894")).ReturnsAsync(studentPrograms);

                // Mock an empty program 
                IEnumerable<string> depts = new List<string>() { "Dept1" };
                IEnumerable<Domain.Student.Entities.Requirements.Program> program =
                    new List<Domain.Student.Entities.Requirements.Program>() { new Domain.Student.Entities.Requirements.Program("EmptyRequirements", "Empty Requirements", depts, true, "UG", new CreditFilter(), false) };
                programRepoMock.Setup(pr => pr.GetAsync()).Returns(Task.FromResult(program));

                // Attempt to get degree plan
                await studentDegreePlanService.CreateDegreePlan3Async("0000894");
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CreateDegreePlanAsync_ThrowsErrorForAdvisorWithoutCorrectPermissions()
            {
                // Attempt to get degree plan
                await studentDegreePlanService.CreateDegreePlan3Async("0000894");
            }

            [TestMethod]
            public async Task UpdateDegreePlanAsync_AllowedForAdvisorWithUpdateAnyPermissions()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.UpdateAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock successful update of a plan from repository
                DegreePlan degreePlan = new DegreePlan(1, "0000894", 0);
                degreePlan.AddTerm("2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Domain.Student.Entities.DegreePlans.DegreePlan>())).Returns(Task.FromResult(degreePlan));
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<List<string>>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.DegreePlans.DegreePlan>>(new List<Domain.Student.Entities.DegreePlans.DegreePlan>() { degreePlan }));

                // Mock dto Adapter 
                var degreePlanEntityToDtoAdapter = new DegreePlanEntity3Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan3>()).Returns(degreePlanEntityToDtoAdapter);
                // Mock entity adapter
                var degreePlanDtoToEntityAdapter = new DegreePlanDto3Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Dtos.Student.DegreePlans.DegreePlan3, Domain.Student.Entities.DegreePlans.DegreePlan>()).Returns(degreePlanDtoToEntityAdapter);

                // Map degree plan to dto
                var degreePlanDto = degreePlanEntityToDtoAdapter.MapToType(degreePlan);

                // Add review/protected changes to degree plan. Permission exception would be thrown if advisor permissions did not allow this action.
                DegreePlan originalDegreePlan = new DegreePlan(1, "0000894", 0);
                originalDegreePlan.AddTerm("2012/FA");
                originalDegreePlan.LastReviewedAdvisorId = "0101010";
                originalDegreePlan.AddCourse(new PlannedCourse("01", "12", Domain.Student.Entities.GradingType.Graded, Domain.Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, null) { IsProtected = true }, "2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<int>())).Returns(Task.FromResult(originalDegreePlan));

                // Act
                await studentDegreePlanService.UpdateDegreePlan3Async(degreePlanDto);
            }

            [TestMethod]
            public async Task UpdateDegreePlanAsync_AllowedForAdvisorWithUpdateAssignedPermissions()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.UpdateAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock student repository get
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                student.AddAdvisor("0000111");
                student.AddAdvisement("0000111", null, null, null);
                var studentAccess = student.ConvertToStudentAccess();
                studentRepoMock.Setup(x => x.GetStudentAccessAsync(new List<string>() { "0000894" })).Returns(Task.FromResult(new List<StudentAccess>() { studentAccess }.AsEnumerable()));

                // Mock successful update of a plan from repository
                DegreePlan degreePlan = new DegreePlan(1, "0000894", 0);
                degreePlan.AddTerm("2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Domain.Student.Entities.DegreePlans.DegreePlan>())).Returns(Task.FromResult(degreePlan));
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<List<string>>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.DegreePlans.DegreePlan>>(new List<Domain.Student.Entities.DegreePlans.DegreePlan>() { degreePlan }));

                // Mock dto Adapter 
                var degreePlanEntityToDtoAdapter = new DegreePlanEntity3Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan3>()).Returns(degreePlanEntityToDtoAdapter);

                // Mock entity adapter
                var degreePlanDtoToEntityAdapter = new DegreePlanDto3Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Dtos.Student.DegreePlans.DegreePlan3, Domain.Student.Entities.DegreePlans.DegreePlan>()).Returns(degreePlanDtoToEntityAdapter);

                // Map degree plan to dto
                var degreePlanDto = degreePlanEntityToDtoAdapter.MapToType(degreePlan);

                // Add review/protected changes to degree plan. Permission exception would be thrown if advisor permissions did not allow this action.
                DegreePlan originalDegreePlan = new DegreePlan(1, "0000894", 0);
                originalDegreePlan.AddTerm("2012/FA");
                originalDegreePlan.LastReviewedAdvisorId = "0101010";
                originalDegreePlan.AddCourse(new PlannedCourse("01", "12", Domain.Student.Entities.GradingType.Graded, Domain.Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, null) { IsProtected = true }, "2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<int>())).Returns(Task.FromResult(originalDegreePlan));

                // Act
                await studentDegreePlanService.UpdateDegreePlan3Async(degreePlanDto);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task UpdateDegreePlanAsync_UserNotAssignedAdvisor()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.UpdateAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock student repository get
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                student.AddAdvisor("0000222");
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));

                // Mock successful update of a plan from repository
                DegreePlan degreePlan = new DegreePlan(1, "0000894", 0);
                degreePlan.AddTerm("2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Domain.Student.Entities.DegreePlans.DegreePlan>())).Returns(Task.FromResult(degreePlan));
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<List<string>>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.DegreePlans.DegreePlan>>(new List<Domain.Student.Entities.DegreePlans.DegreePlan>() { degreePlan }));

                // Mock dto Adapter 
                var degreePlanEntityToDtoAdapter = new DegreePlanEntity3Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan3>()).Returns(degreePlanEntityToDtoAdapter);

                // Mock entity adapter
                var degreePlanDtoToEntityAdapter = new DegreePlanDto3Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Dtos.Student.DegreePlans.DegreePlan3, Domain.Student.Entities.DegreePlans.DegreePlan>()).Returns(degreePlanDtoToEntityAdapter);

                // Map degree plan to dto
                var degreePlanDto = degreePlanEntityToDtoAdapter.MapToType(degreePlan);

                // Add review/protected changes to degree plan. Permission exception thrown because advisor permissions do not allow this action.
                DegreePlan originalDegreePlan = new DegreePlan(1, "0000894", 0);
                originalDegreePlan.AddTerm("2012/FA");
                originalDegreePlan.LastReviewedAdvisorId = "0101010";
                originalDegreePlan.AddCourse(new PlannedCourse("01", "12", Domain.Student.Entities.GradingType.Graded, Domain.Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, null) { IsProtected = true }, "2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<int>())).Returns(Task.FromResult(originalDegreePlan));

                // Act
                await studentDegreePlanService.UpdateDegreePlan3Async(degreePlanDto);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task UpdateDegreePlanAsync_ThrowsErrorForAdvisorWithViewAnyPermissions()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock successful update of a plan from repository
                DegreePlan degreePlan = new DegreePlan(1, "0000894", 0);
                degreePlan.AddTerm("2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Domain.Student.Entities.DegreePlans.DegreePlan>())).Returns(Task.FromResult(degreePlan));
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<List<string>>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.DegreePlans.DegreePlan>>(new List<Domain.Student.Entities.DegreePlans.DegreePlan>() { degreePlan }));
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<int>())).Returns(Task.FromResult(degreePlan));

                // Mock dto Adapter 
                var degreePlanEntityToDtoAdapter = new DegreePlanEntity3Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan3>()).Returns(degreePlanEntityToDtoAdapter);

                // Mock entity adapter
                var degreePlanDtoToEntityAdapter = new DegreePlanDto3Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Dtos.Student.DegreePlans.DegreePlan3, Domain.Student.Entities.DegreePlans.DegreePlan>()).Returns(degreePlanDtoToEntityAdapter);

                // Map degree plan to dto
                var degreePlanDto = degreePlanEntityToDtoAdapter.MapToType(degreePlan);

                // Act
                await studentDegreePlanService.UpdateDegreePlan3Async(degreePlanDto);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task UpdateDegreePlanAsync_ThrowsErrorForAdvisorWithReviewAnyPermissionsAndProtectedUpdates()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ReviewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock successful update of a plan from repository
                DegreePlan degreePlan = new DegreePlan(1, "0000894", 0);
                degreePlan.AddTerm("2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Domain.Student.Entities.DegreePlans.DegreePlan>())).Returns(Task.FromResult(degreePlan));
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<List<string>>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.DegreePlans.DegreePlan>>(new List<Domain.Student.Entities.DegreePlans.DegreePlan>() { degreePlan }));
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<int>())).Returns(Task.FromResult(degreePlan));

                // Mock dto Adapter 
                var degreePlanEntityToDtoAdapter = new DegreePlanEntity3Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan3>()).Returns(degreePlanEntityToDtoAdapter);

                // Mock entity adapter
                var degreePlanDtoToEntityAdapter = new DegreePlanDto3Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Dtos.Student.DegreePlans.DegreePlan3, Domain.Student.Entities.DegreePlans.DegreePlan>()).Returns(degreePlanDtoToEntityAdapter);

                // Map degree plan to dto
                var degreePlanDto = degreePlanEntityToDtoAdapter.MapToType(degreePlan);

                // Add review/protected changes to degree plan. Permission exception will be thrown because advisor permissions do not allow this action.
                DegreePlan originalDegreePlan = new DegreePlan(1, "0000894", 0);
                originalDegreePlan.AddTerm("2012/FA");
                originalDegreePlan.LastReviewedAdvisorId = "0101010";
                originalDegreePlan.AddCourse(new PlannedCourse("01", "12", Domain.Student.Entities.GradingType.Graded, Domain.Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, null) { IsProtected = true }, "2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<int>())).Returns(Task.FromResult(originalDegreePlan));

                // Act
                await studentDegreePlanService.UpdateDegreePlan3Async(degreePlanDto);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task UpdateDegreePlanAsync_ThrowsErrorForAdvisorWithReviewAssignedPermissionsAndProtectedUpdates()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ReviewAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock student repository get
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                student.AddAdvisor("0000111");
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));

                // Mock successful update of a plan from repository
                DegreePlan degreePlan = new DegreePlan(1, "0000894", 0);
                degreePlan.AddTerm("2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Domain.Student.Entities.DegreePlans.DegreePlan>())).Returns(Task.FromResult(degreePlan));
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<List<string>>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.DegreePlans.DegreePlan>>(new List<Domain.Student.Entities.DegreePlans.DegreePlan>() { degreePlan }));
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<int>())).Returns(Task.FromResult(degreePlan));

                // Mock dto Adapter 
                var degreePlanEntityToDtoAdapter = new DegreePlanEntity3Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan3>()).Returns(degreePlanEntityToDtoAdapter);

                // Mock entity adapter
                var degreePlanDtoToEntityAdapter = new DegreePlanDto3Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Dtos.Student.DegreePlans.DegreePlan3, Domain.Student.Entities.DegreePlans.DegreePlan>()).Returns(degreePlanDtoToEntityAdapter);

                // Map degree plan to dto
                var degreePlanDto = degreePlanEntityToDtoAdapter.MapToType(degreePlan);

                // Add review/protected changes to degree plan. Permission exception will be thrown because advisor permissions do not allow this action.
                DegreePlan originalDegreePlan = new DegreePlan(1, "0000894", 0);
                originalDegreePlan.AddTerm("2012/FA");
                originalDegreePlan.LastReviewedAdvisorId = "0101010";
                originalDegreePlan.AddCourse(new PlannedCourse("01", "12", Domain.Student.Entities.GradingType.Graded, Domain.Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, null) { IsProtected = true }, "2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<int>())).Returns(Task.FromResult(originalDegreePlan));

                // Act
                await studentDegreePlanService.UpdateDegreePlan3Async(degreePlanDto);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task UpdateDegreePlanAsync_ThrowsErrorForAdvisorWithViewAssignedPermissions()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock student repository get
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                student.AddAdvisor("0000111");
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));

                // Mock successful update of a plan from repository
                DegreePlan degreePlan = new DegreePlan(1, "0000894", 0);
                degreePlan.AddTerm("2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Domain.Student.Entities.DegreePlans.DegreePlan>())).Returns(Task.FromResult(degreePlan));
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<List<string>>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.DegreePlans.DegreePlan>>(new List<Domain.Student.Entities.DegreePlans.DegreePlan>() { degreePlan }));
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<int>())).Returns(Task.FromResult(degreePlan));

                // Mock dto Adapter 
                var degreePlanEntityToDtoAdapter = new DegreePlanEntity3Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan3>()).Returns(degreePlanEntityToDtoAdapter);

                // Mock entity adapter
                var degreePlanDtoToEntityAdapter = new DegreePlanDto3Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Dtos.Student.DegreePlans.DegreePlan3, Domain.Student.Entities.DegreePlans.DegreePlan>()).Returns(degreePlanDtoToEntityAdapter);

                // Map degree plan to dto
                var degreePlanDto = degreePlanEntityToDtoAdapter.MapToType(degreePlan);

                // Add review/protected changes to degree plan. Permission exception will be thrown if advisor permissions do not allow this action.
                DegreePlan originalDegreePlan = new DegreePlan(1, "0000894", 0);
                originalDegreePlan.AddTerm("2012/FA");
                originalDegreePlan.LastReviewedAdvisorId = "0101010";
                originalDegreePlan.AddCourse(new PlannedCourse("01", "12", Domain.Student.Entities.GradingType.Graded, Domain.Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, null) { IsProtected = true }, "2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<int>())).Returns(Task.FromResult(originalDegreePlan));

                // Act
                await studentDegreePlanService.UpdateDegreePlan3Async(degreePlanDto);
            }
        }

        [TestClass]
        public class DegreePlanService_TestAdvisorPermissions4 : DegreePlanServiceTestsSetup
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
                student.AddAdvisement("0000111", null, null, null);
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));

                activeAcademicCredits = new List<Domain.Student.Entities.AcademicCredit>();
                // Mock academic credit repository get using student ids returning dict with student's academic credits
                var academicCreditDict = new Dictionary<string, List<Domain.Student.Entities.AcademicCredit>>();
                academicCreditDict["0000894"] = activeAcademicCredits.ToList();
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

                // Mock get requirements for requisite validation 
                var requirementCodes = new List<string>() { };
                var requirements = (await new TestRequirementRepository().GetAsync(requirementCodes)).Where(r => r.RequirementType.Code == "PRE");
                requirementRepoMock.Setup(x => x.GetAsync(It.IsAny<IEnumerable<string>>())).Returns(Task.FromResult(requirements));

                // Mock Adapter 
                var degreePlanPreviewDtoAdapter = new AutoMapperAdapter<DegreePlanPreview, Dtos.Planning.DegreePlanPreview4>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<DegreePlanPreview, Dtos.Planning.DegreePlanPreview4>()).Returns(degreePlanPreviewDtoAdapter);
                var degreePlanDtoAdapter = new AutoMapperAdapter<DegreePlan, Dtos.Student.DegreePlans.DegreePlan4>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<DegreePlan, Dtos.Student.DegreePlans.DegreePlan4>()).Returns(degreePlanDtoAdapter);
                var degreePlanApprovalDtoAdapter = new AutoMapperAdapter<DegreePlanApproval, Dtos.Student.DegreePlans.DegreePlanApproval2>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<DegreePlanApproval, Dtos.Student.DegreePlans.DegreePlanApproval2>()).Returns(degreePlanApprovalDtoAdapter);


                // Mock degree plan4 dto Adapter 
                var degreePlanEntityToDegreePlan3DtoAdapter = new AutoMapperAdapter<DegreePlan, Dtos.Student.DegreePlans.DegreePlan4>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<DegreePlan, Dtos.Student.DegreePlans.DegreePlan4>()).Returns(degreePlanEntityToDegreePlan3DtoAdapter);

                // Mock conversion from academic credits to academic history
                var studentAcademicCredits = await new TestAcademicCreditRepository().GetAsync();
                Domain.Student.Entities.GradeRestriction gradeRestriction = new Domain.Student.Entities.GradeRestriction(false);
                var historyEntity = new Domain.Student.Entities.AcademicHistory(studentAcademicCredits, gradeRestriction, null);
                historyEntity.StudentId = "0000894";
                var historyDto = new AcademicHistoryEntityToAcademicHistory2DtoAdapter(adapterRegistry, logger).MapToType(historyEntity);
                academicHistoryServiceMock.Setup(svc => svc.ConvertAcademicCreditsToAcademicHistoryDtoAsync(historyEntity.StudentId, It.IsAny<IEnumerable<Domain.Student.Entities.AcademicCredit>>(), It.IsAny<Domain.Student.Entities.Student>()))
                    .Returns(Task.FromResult(historyDto));

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
            public async Task GetDegreePlanAsync_AllowedForAdvisorWithViewAnyAdviseePermissions()
            {
                // Set up view permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock student repository get
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));

                // Attempt to get degree plan
                await studentDegreePlanService.GetDegreePlan4Async(2);
            }

            [TestMethod]
            public async Task GetDegreePlanAsync_AllowedForAdvisorWithReviewAnyAdviseePermissions()
            {
                // Set up view permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ReviewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock student repository get
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));

                // Attempt to get degree plan
                await studentDegreePlanService.GetDegreePlan4Async(2);
            }

            [TestMethod]
            public async Task GetDegreePlanAsync_AllowedForAdvisorWithUpdateAnyAdviseePermissions()
            {
                // Set up view permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.UpdateAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock student repository get
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));

                // Attempt to get degree plan
                await studentDegreePlanService.GetDegreePlan4Async(2);
            }

            [TestMethod]
            public async Task GetDegreePlanAsync_AllowedForAdvisorWithAllAccessAnyAdviseePermissions()
            {
                // Set up view permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock student repository get
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));

                // Attempt to get degree plan
                await studentDegreePlanService.GetDegreePlan4Async(2);
            }

            [TestMethod]
            public async Task GetDegreePlanAsync_AllowedForAdvisorWithViewAssignedAdviseesPermissions()
            {
                // Set up view permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock student repository get--student with this advisor
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                student.AddAdvisor("0000111");
                student.AddAdvisement("0000111", null, null, null);
                //studentRepoMock.Setup(x => x.GetAsync("0000894")).ReturnsAsync(student);
                StudentAccess studentAccess = student.ConvertToStudentAccess();
                studentRepoMock.Setup(repo => repo.GetStudentAccessAsync(It.IsAny<List<string>>())).Returns(Task.FromResult(new List<StudentAccess>() { studentAccess }.AsEnumerable()));

                // Attempt to get degree plan
                await studentDegreePlanService.GetDegreePlan4Async(2);
            }

            [TestMethod]
            public async Task GetDegreePlanAsync_AllowedForAdvisorWithReviewAssignedAdviseesPermissions()
            {
                // Set up view permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ReviewAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock student repository get--student with this advisor
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                student.AddAdvisement("0000111", null, null, null);
                //studentRepoMock.Setup(x => x.GetAsync("0000894")).ReturnsAsync(student);
                StudentAccess studentAccess = student.ConvertToStudentAccess();
                studentRepoMock.Setup(repo => repo.GetStudentAccessAsync(It.IsAny<List<string>>())).Returns(Task.FromResult(new List<StudentAccess>() { studentAccess }.AsEnumerable()));

                // Attempt to get degree plan
                await studentDegreePlanService.GetDegreePlan4Async(2);
            }

            [TestMethod]
            public async Task GetDegreePlanAsync_AllowedForAdvisorWithUpdateAssignedAdviseesPermissions()
            {
                // Set up view permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.UpdateAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock student repository get--student with this advisor
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                student.AddAdvisement("0000111", null, null, null);
                //studentRepoMock.Setup(x => x.GetAsync("0000894")).ReturnsAsync(student);
                StudentAccess studentAccess = student.ConvertToStudentAccess();
                studentRepoMock.Setup(repo => repo.GetStudentAccessAsync(It.IsAny<List<string>>())).Returns(Task.FromResult(new List<StudentAccess>() { studentAccess }.AsEnumerable()));

                // Attempt to get degree plan
                await studentDegreePlanService.GetDegreePlan4Async(2);
            }

            [TestMethod]
            public async Task GetDegreePlanAsync_AllowedForAdvisorWithAllAccessAssignedAdviseesPermissions()
            {
                // Set up view permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock student repository get--student with this advisor
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                student.AddAdvisement("0000111", null, null, null);
                //studentRepoMock.Setup(x => x.GetAsync("0000894")).ReturnsAsync(student);
                StudentAccess studentAccess = student.ConvertToStudentAccess();
                studentRepoMock.Setup(repo => repo.GetStudentAccessAsync(It.IsAny<List<string>>())).Returns(Task.FromResult(new List<StudentAccess>() { studentAccess }.AsEnumerable()));

                // Attempt to get degree plan
                await studentDegreePlanService.GetDegreePlan4Async(2);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task GetDegreePlan_ThrowsErrorForAdvisorWithoutAnyPermissions()
            {
                await studentDegreePlanService.GetDegreePlan4Async(2);
            }

            [TestMethod]
            public async Task CreateDegreePlanAsync_AllowedForAdvisorWithAllAccessAnyAdviseePermissions()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock the get from the student repository (Create tests)
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                student.AddAdvisor("0000111");
                student.AddAdvisement("0000111", null, null, null);
                // Still need to mock student repo, since CreateDegreePlan needs to get the student
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));

                // Mock the get of the planning terms (Create tests)
                var allTerms = await new TestTermRepository().GetAsync();
                termRepoMock.Setup(x => x.GetAsync()).ReturnsAsync(allTerms);

                // Mock the get of the student programs (Create tests)
                var studentPrograms = await new TestStudentProgramRepository().GetAsync("0000896");
                studentProgramRepoMock.Setup(x => x.GetAsync("0000896")).ReturnsAsync(studentPrograms);

                // Mock an empty program 
                IEnumerable<string> depts = new List<string>() { "Dept1" };
                IEnumerable<Domain.Student.Entities.Requirements.Program> program =
                    new List<Domain.Student.Entities.Requirements.Program>() { new Domain.Student.Entities.Requirements.Program("EmptyRequirements", "Empty Requirements", depts, true, "UG", new CreditFilter(), false) };
                programRepoMock.Setup(pr => pr.GetAsync()).Returns(Task.FromResult(program));

                // Attempt to get degree plan
                await studentDegreePlanService.CreateDegreePlan4Async("0000894");
            }

            [TestMethod]
            public async Task CreateDegreePlanAsync_AllowedForAdvisorWithAllAccessAssignedAdviseesPermissions()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock the get from the student repository (Create tests)
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                student.AddAdvisement("0000111", null, null, null);
                // Still need to mock student repo, since CreateDegreePlan needs to get the student
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));

                // Mock the get of the planning terms (Create tests)
                var allTerms = await new TestTermRepository().GetAsync();
                termRepoMock.Setup(x => x.GetAsync()).ReturnsAsync(allTerms);

                // Mock the get of the student programs (Create tests)
                var studentPrograms = await new TestStudentProgramRepository().GetAsync("0000896");
                studentProgramRepoMock.Setup(x => x.GetAsync("0000896")).ReturnsAsync(studentPrograms);

                // Mock an empty program 
                IEnumerable<string> depts = new List<string>() { "Dept1" };
                IEnumerable<Domain.Student.Entities.Requirements.Program> program =
                    new List<Domain.Student.Entities.Requirements.Program>() { new Domain.Student.Entities.Requirements.Program("EmptyRequirements", "Empty Requirements", depts, true, "UG", new CreditFilter(), false) };
                programRepoMock.Setup(pr => pr.GetAsync()).Returns(Task.FromResult(program));

                // Attempt to get degree plan
                await studentDegreePlanService.CreateDegreePlan4Async("0000894");
            }

            [TestMethod]
            public async Task CreateDegreePlanAsync_AllowedForAdvisorWithUpdateAnyPermissions()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.UpdateAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock the get from the student repository (Create tests)
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));

                // Mock the get of the planning terms (Create tests)
                var allTerms = await new TestTermRepository().GetAsync();
                termRepoMock.Setup(x => x.GetAsync()).ReturnsAsync(allTerms);

                // Mock the get of the student programs (Create tests)
                var studentPrograms = await new TestStudentProgramRepository().GetAsync("0000896");
                studentProgramRepoMock.Setup(x => x.GetAsync("0000896")).ReturnsAsync(studentPrograms);

                // Mock an empty program 
                IEnumerable<string> depts = new List<string>() { "Dept1" };
                IEnumerable<Domain.Student.Entities.Requirements.Program> program =
                    new List<Domain.Student.Entities.Requirements.Program>() { new Domain.Student.Entities.Requirements.Program("EmptyRequirements", "Empty Requirements", depts, true, "UG", new CreditFilter(), true) };
                programRepoMock.Setup(pr => pr.GetAsync()).Returns(Task.FromResult(program));

                // Attempt to get degree plan
                await studentDegreePlanService.CreateDegreePlan4Async("0000894");
            }

            [TestMethod]
            public async Task CreateDegreePlan_AllowedForAdvisorWithUpdateAssignedPermissions()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.UpdateAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock the get from the student repository (Create tests)
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                student.AddAdvisement("0000111", null, null, null);
                // Still need to mock student repo, since CreateDegreePlan needs to get the student
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));

                // Mock the get of the planning terms (Create tests)
                var allTerms = await new TestTermRepository().GetAsync();
                termRepoMock.Setup(x => x.GetAsync()).ReturnsAsync(allTerms);

                // Mock the get of the student programs (Create tests)
                var studentPrograms = await new TestStudentProgramRepository().GetAsync("0000896");
                studentProgramRepoMock.Setup(x => x.GetAsync("0000896")).ReturnsAsync(studentPrograms);

                // Mock an empty program 
                IEnumerable<string> depts = new List<string>() { "Dept1" };
                IEnumerable<Domain.Student.Entities.Requirements.Program> program =
                    new List<Domain.Student.Entities.Requirements.Program>() { new Domain.Student.Entities.Requirements.Program("EmptyRequirements", "Empty Requirements", depts, true, "UG", new CreditFilter(), true) };
                programRepoMock.Setup(pr => pr.GetAsync()).Returns(Task.FromResult(program));

                // Attempt to get degree plan
                await studentDegreePlanService.CreateDegreePlan4Async("0000894");
            }

            [TestMethod]
            public async Task CreateDegreePlanAsync_AllowedForAdvisorWithReviewAnyPermissions()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ReviewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock the get from the student repository (Create tests)
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));

                // Mock the get of the planning terms (Create tests)
                var allTerms = await new TestTermRepository().GetAsync();
                termRepoMock.Setup(x => x.GetAsync()).ReturnsAsync(allTerms);

                // Mock the get of the student programs (Create tests)
                var studentPrograms = await new TestStudentProgramRepository().GetAsync("0000896");
                studentProgramRepoMock.Setup(x => x.GetAsync("0000896")).ReturnsAsync(studentPrograms);

                // Mock an empty program 
                IEnumerable<string> depts = new List<string>() { "Dept1" };
                IEnumerable<Domain.Student.Entities.Requirements.Program> program =
                    new List<Domain.Student.Entities.Requirements.Program>() { new Domain.Student.Entities.Requirements.Program("EmptyRequirements", "Empty Requirements", depts, true, "UG", new CreditFilter(), true) };
                programRepoMock.Setup(pr => pr.GetAsync()).Returns(Task.FromResult(program));

                // Attempt to get degree plan
                await studentDegreePlanService.CreateDegreePlan4Async("0000894");
            }

            [TestMethod]
            public async Task CreateDegreePlan_AllowedForAdvisorWithReviewAssignedPermissions()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ReviewAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock the get from the student repository (Create tests)
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                student.AddAdvisement("0000111", null, null, null);
                // Still need to mock student repo, since CreateDegreePlan needs to get the student
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));

                // Mock the get of the planning terms (Create tests)
                var allTerms = await new TestTermRepository().GetAsync();
                termRepoMock.Setup(x => x.GetAsync()).ReturnsAsync(allTerms);

                // Mock the get of the student programs (Create tests)
                var studentPrograms = await new TestStudentProgramRepository().GetAsync("0000896");
                studentProgramRepoMock.Setup(x => x.GetAsync("0000896")).ReturnsAsync(studentPrograms);

                // Mock an empty program 
                IEnumerable<string> depts = new List<string>() { "Dept1" };
                IEnumerable<Domain.Student.Entities.Requirements.Program> program =
                    new List<Domain.Student.Entities.Requirements.Program>() { new Domain.Student.Entities.Requirements.Program("EmptyRequirements", "Empty Requirements", depts, true, "UG", new CreditFilter(), false) };
                programRepoMock.Setup(pr => pr.GetAsync()).Returns(Task.FromResult(program));

                // Attempt to get degree plan
                await studentDegreePlanService.CreateDegreePlan4Async("0000894");
            }

            [TestMethod]
            public async Task CreateDegreePlan_AllowedForAdvisorWithViewAnyAdviseePermissions()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock the get from the student repository (Create tests)
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));
                studentRepoMock.Setup(x => x.GetAsync("0000896")).Returns(Task.FromResult(student));

                // Mock the get of the planning terms (Create tests)
                var allTerms = await new TestTermRepository().GetAsync();
                termRepoMock.Setup(x => x.GetAsync()).ReturnsAsync(allTerms);

                // Mock the get of the student programs (Create tests)
                var studentPrograms = await new TestStudentProgramRepository().GetAsync("0000896");
                studentProgramRepoMock.Setup(x => x.GetAsync("0000896")).ReturnsAsync(studentPrograms);
                studentProgramRepoMock.Setup(x => x.GetAsync("0000894")).ReturnsAsync(studentPrograms);

                // Mock an empty program 
                IEnumerable<string> depts = new List<string>() { "Dept1" };
                IEnumerable<Domain.Student.Entities.Requirements.Program> program =
                    new List<Domain.Student.Entities.Requirements.Program>() { new Domain.Student.Entities.Requirements.Program("EmptyRequirements", "Empty Requirements", depts, true, "UG", new CreditFilter(), false) };
                programRepoMock.Setup(pr => pr.GetAsync()).Returns(Task.FromResult(program));

                // Attempt to get degree plan
                await studentDegreePlanService.CreateDegreePlan4Async("0000894");
            }

            [TestMethod]
            public async Task CreateDegreePlanAsync_AllowedForAdvisorWithViewAssignedAdviseesPermissions()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock the get from the student repository (Create tests)
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                student.AddAdvisement("0000111", null, null, null);
                // Still need to mock student repo, since CreateDegreePlan needs to get the student
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));

                // Mock the get of the planning terms (Create tests)
                var allTerms = await new TestTermRepository().GetAsync();
                termRepoMock.Setup(x => x.GetAsync()).ReturnsAsync(allTerms);

                // Mock the get of the student programs (Create tests)
                var studentPrograms = await new TestStudentProgramRepository().GetAsync("0000896");
                studentProgramRepoMock.Setup(x => x.GetAsync("0000896")).ReturnsAsync(studentPrograms);

                // Mock an empty program 
                IEnumerable<string> depts = new List<string>() { "Dept1" };
                IEnumerable<Domain.Student.Entities.Requirements.Program> program =
                    new List<Domain.Student.Entities.Requirements.Program>() { new Domain.Student.Entities.Requirements.Program("EmptyRequirements", "Empty Requirements", depts, true, "UG", new CreditFilter(), false) };
                programRepoMock.Setup(pr => pr.GetAsync()).Returns(Task.FromResult(program));

                // Attempt to get degree plan
                await studentDegreePlanService.CreateDegreePlan4Async("0000894");
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CreateDegreePlanAsync_ThrowsErrorForAdvisorWithoutPermissions()
            {
                // Attempt to get degree plan
                await studentDegreePlanService.CreateDegreePlan4Async("0000894");
            }


            [TestMethod]
            public async Task UpdateDegreePlanAsync_AllowedForAdvisorWithUpdateAnyPermissions()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.UpdateAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock successful update of a plan from repository
                DegreePlan degreePlan = new DegreePlan(1, "0000894", 0);
                degreePlan.AddTerm("2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Domain.Student.Entities.DegreePlans.DegreePlan>())).Returns(Task.FromResult(degreePlan));
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<List<string>>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.DegreePlans.DegreePlan>>(new List<Domain.Student.Entities.DegreePlans.DegreePlan>() { degreePlan }));

                // Mock dto Adapter 
                var degreePlanEntityToDtoAdapter = new DegreePlanEntity4Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan4>()).Returns(degreePlanEntityToDtoAdapter);

                // Mock entity adapter
                var degreePlanDtoToEntityAdapter = new DegreePlanDto4Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Dtos.Student.DegreePlans.DegreePlan4, Domain.Student.Entities.DegreePlans.DegreePlan>()).Returns(degreePlanDtoToEntityAdapter);

                // Map degree plan to dto
                var degreePlanDto = degreePlanEntityToDtoAdapter.MapToType(degreePlan);

                // Add review/protected changes to degree plan. Permission exception would be thrown if advisor permissions did not allow this action.
                DegreePlan originalDegreePlan = new DegreePlan(1, "0000894", 0);
                originalDegreePlan.AddTerm("2012/FA");
                originalDegreePlan.LastReviewedAdvisorId = "0101010";
                originalDegreePlan.AddCourse(new PlannedCourse("01", "12", Domain.Student.Entities.GradingType.Graded, Domain.Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, null) { IsProtected = true }, "2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<int>())).Returns(Task.FromResult(originalDegreePlan));

                // Act
                await studentDegreePlanService.UpdateDegreePlan4Async(degreePlanDto);
            }

            [TestMethod]
            public async Task UpdateDegreePlanAsync_AllowedForAdvisorWithUpdateAssignedPermissions()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.UpdateAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock student repository get
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                student.AddAdvisor("0000111");
                student.AddAdvisement("0000111", null, null, null);
                var studentAccess = student.ConvertToStudentAccess();
                studentRepoMock.Setup(x => x.GetStudentAccessAsync(new List<string>() { "0000894" })).Returns(Task.FromResult(new List<StudentAccess>() { studentAccess }.AsEnumerable()));

                // Mock successful update of a plan from repository
                DegreePlan degreePlan = new DegreePlan(1, "0000894", 0);
                degreePlan.AddTerm("2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Domain.Student.Entities.DegreePlans.DegreePlan>())).Returns(Task.FromResult(degreePlan));
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<List<string>>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.DegreePlans.DegreePlan>>(new List<Domain.Student.Entities.DegreePlans.DegreePlan>() { degreePlan }));

                // Mock dto Adapter 
                var degreePlanEntityToDtoAdapter = new DegreePlanEntity4Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan4>()).Returns(degreePlanEntityToDtoAdapter);

                // Mock entity adapter
                var degreePlanDtoToEntityAdapter = new DegreePlanDto4Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Dtos.Student.DegreePlans.DegreePlan4, Domain.Student.Entities.DegreePlans.DegreePlan>()).Returns(degreePlanDtoToEntityAdapter);

                // Map degree plan to dto
                var degreePlanDto = degreePlanEntityToDtoAdapter.MapToType(degreePlan);

                // Add review/protected changes to degree plan. Permission exception would be thrown if advisor permissions did not allow this action.
                DegreePlan originalDegreePlan = new DegreePlan(1, "0000894", 0);
                originalDegreePlan.AddTerm("2012/FA");
                originalDegreePlan.LastReviewedAdvisorId = "0101010";
                originalDegreePlan.AddCourse(new PlannedCourse("01", "12", Domain.Student.Entities.GradingType.Graded, Domain.Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, null) { IsProtected = true }, "2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<int>())).Returns(Task.FromResult(originalDegreePlan));

                // Act
                await studentDegreePlanService.UpdateDegreePlan4Async(degreePlanDto);
            }

            [TestMethod]
            public async Task UpdateDegreePlan_AllowedForAdvisorWithAllAccessAnyPermissions()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock successful update of a plan from repository
                DegreePlan degreePlan = new DegreePlan(1, "0000894", 0);
                degreePlan.AddTerm("2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Domain.Student.Entities.DegreePlans.DegreePlan>())).Returns(Task.FromResult(degreePlan));
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<List<string>>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.DegreePlans.DegreePlan>>(new List<Domain.Student.Entities.DegreePlans.DegreePlan>() { degreePlan }));

                // Mock dto Adapter 
                var degreePlanEntityToDtoAdapter = new DegreePlanEntity4Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan4>()).Returns(degreePlanEntityToDtoAdapter);
                // Mock entity adapter
                var degreePlanDtoToEntityAdapter = new DegreePlanDto4Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Dtos.Student.DegreePlans.DegreePlan4, Domain.Student.Entities.DegreePlans.DegreePlan>()).Returns(degreePlanDtoToEntityAdapter);

                // Map degree plan to dto
                var degreePlanDto = degreePlanEntityToDtoAdapter.MapToType(degreePlan);

                // Add review/protected changes to degree plan. Permission exception would be thrown if advisor permissions did not allow this action.
                DegreePlan originalDegreePlan = new DegreePlan(1, "0000894", 0);
                originalDegreePlan.AddTerm("2012/FA");
                originalDegreePlan.LastReviewedAdvisorId = "0101010";
                originalDegreePlan.AddCourse(new PlannedCourse("01", "12", Domain.Student.Entities.GradingType.Graded, Domain.Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, null) { IsProtected = true }, "2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<int>())).Returns(Task.FromResult(originalDegreePlan));

                // Act
                await studentDegreePlanService.UpdateDegreePlan4Async(degreePlanDto);
            }

            [TestMethod]
            public async Task UpdateDegreePlanAsync_AllowedForAdvisorWithAllAccessAssignedPermissions()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock student repository get
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                student.AddAdvisor("0000111");
                student.AddAdvisement("0000111", null, null, null);
                var studentAccess = student.ConvertToStudentAccess();
                studentRepoMock.Setup(x => x.GetStudentAccessAsync(new List<string>() { "0000894" })).Returns(Task.FromResult(new List<StudentAccess>() { studentAccess }.AsEnumerable()));

                // Mock successful update of a plan from repository
                DegreePlan degreePlan = new DegreePlan(1, "0000894", 0);
                degreePlan.AddTerm("2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Domain.Student.Entities.DegreePlans.DegreePlan>())).Returns(Task.FromResult(degreePlan));
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<List<string>>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.DegreePlans.DegreePlan>>(new List<Domain.Student.Entities.DegreePlans.DegreePlan>() { degreePlan }));

                // Mock dto Adapter 
                var degreePlanEntityToDtoAdapter = new DegreePlanEntity4Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan4>()).Returns(degreePlanEntityToDtoAdapter);

                // Mock entity adapter
                var degreePlanDtoToEntityAdapter = new DegreePlanDto4Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Dtos.Student.DegreePlans.DegreePlan4, Domain.Student.Entities.DegreePlans.DegreePlan>()).Returns(degreePlanDtoToEntityAdapter);

                // Map degree plan to dto
                var degreePlanDto = degreePlanEntityToDtoAdapter.MapToType(degreePlan);

                // Add review/protected changes to degree plan. Permission exception would be thrown if advisor permissions did not allow this action.
                DegreePlan originalDegreePlan = new DegreePlan(1, "0000894", 0);
                originalDegreePlan.AddTerm("2012/FA");
                originalDegreePlan.LastReviewedAdvisorId = "0101010";
                originalDegreePlan.AddCourse(new PlannedCourse("01", "12", Domain.Student.Entities.GradingType.Graded, Domain.Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, null) { IsProtected = true }, "2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<int>())).Returns(Task.FromResult(originalDegreePlan));

                // Act
                await studentDegreePlanService.UpdateDegreePlan4Async(degreePlanDto);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task UpdateDegreePlanAsync_UserNotAssignedAdvisor()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.UpdateAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock student repository get
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                student.AddAdvisor("0000222");
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));

                // Mock successful update of a plan from repository
                DegreePlan degreePlan = new DegreePlan(1, "0000894", 0);
                degreePlan.AddTerm("2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Domain.Student.Entities.DegreePlans.DegreePlan>())).Returns(Task.FromResult(degreePlan));
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<List<string>>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.DegreePlans.DegreePlan>>(new List<Domain.Student.Entities.DegreePlans.DegreePlan>() { degreePlan }));

                // Mock dto Adapter 
                var degreePlanEntityToDtoAdapter = new DegreePlanEntity4Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan4>()).Returns(degreePlanEntityToDtoAdapter);

                // Mock entity adapter
                var degreePlanDtoToEntityAdapter = new DegreePlanDto4Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Dtos.Student.DegreePlans.DegreePlan4, Domain.Student.Entities.DegreePlans.DegreePlan>()).Returns(degreePlanDtoToEntityAdapter);

                // Map degree plan to dto
                var degreePlanDto = degreePlanEntityToDtoAdapter.MapToType(degreePlan);

                // Add review/protected changes to degree plan. Permission exception thrown because advisor permissions do not allow this action.
                DegreePlan originalDegreePlan = new DegreePlan(1, "0000894", 0);
                originalDegreePlan.AddTerm("2012/FA");
                originalDegreePlan.LastReviewedAdvisorId = "0101010";
                originalDegreePlan.AddCourse(new PlannedCourse("01", "12", Domain.Student.Entities.GradingType.Graded, Domain.Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, null) { IsProtected = true }, "2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<int>())).Returns(Task.FromResult(originalDegreePlan));

                // Act
                await studentDegreePlanService.UpdateDegreePlan4Async(degreePlanDto);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task UpdateDegreePlanAsync_ThrowsErrorForAdvisorWithViewAnyPermissions()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock successful update of a plan from repository
                DegreePlan degreePlan = new DegreePlan(1, "0000894", 0);
                degreePlan.AddTerm("2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Domain.Student.Entities.DegreePlans.DegreePlan>())).Returns(Task.FromResult(degreePlan));
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<List<string>>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.DegreePlans.DegreePlan>>(new List<Domain.Student.Entities.DegreePlans.DegreePlan>() { degreePlan }));
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<int>())).Returns(Task.FromResult(degreePlan));

                // Mock dto Adapter 
                var degreePlanEntityToDtoAdapter = new DegreePlanEntity4Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan4>()).Returns(degreePlanEntityToDtoAdapter);

                // Mock entity adapter
                var degreePlanDtoToEntityAdapter = new DegreePlanDto4Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Dtos.Student.DegreePlans.DegreePlan4, Domain.Student.Entities.DegreePlans.DegreePlan>()).Returns(degreePlanDtoToEntityAdapter);

                // Map degree plan to dto
                var degreePlanDto = degreePlanEntityToDtoAdapter.MapToType(degreePlan);

                // Act
                await studentDegreePlanService.UpdateDegreePlan4Async(degreePlanDto);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task UpdateDegreePlanAsync_ThrowsErrorForAdvisorWithReviewAnyPermissionsAndProtectedUpdates()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ReviewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock successful update of a plan from repository
                DegreePlan degreePlan = new DegreePlan(1, "0000894", 0);
                degreePlan.AddTerm("2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Domain.Student.Entities.DegreePlans.DegreePlan>())).Returns(Task.FromResult(degreePlan));
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<List<string>>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.DegreePlans.DegreePlan>>(new List<Domain.Student.Entities.DegreePlans.DegreePlan>() { degreePlan }));
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<int>())).Returns(Task.FromResult(degreePlan));

                // Mock dto Adapter 
                var degreePlanEntityToDtoAdapter = new DegreePlanEntity4Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan4>()).Returns(degreePlanEntityToDtoAdapter);

                // Mock entity adapter
                var degreePlanDtoToEntityAdapter = new DegreePlanDto4Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Dtos.Student.DegreePlans.DegreePlan4, Domain.Student.Entities.DegreePlans.DegreePlan>()).Returns(degreePlanDtoToEntityAdapter);

                // Map degree plan to dto
                var degreePlanDto = degreePlanEntityToDtoAdapter.MapToType(degreePlan);

                // Add review/protected changes to degree plan. Permission exception will be thrown because advisor permissions do not allow this action.
                DegreePlan originalDegreePlan = new DegreePlan(1, "0000894", 0);
                originalDegreePlan.AddTerm("2012/FA");
                originalDegreePlan.LastReviewedAdvisorId = "0101010";
                originalDegreePlan.AddCourse(new PlannedCourse("01", "12", Domain.Student.Entities.GradingType.Graded, Domain.Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, null) { IsProtected = true }, "2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<int>())).Returns(Task.FromResult(originalDegreePlan));

                // Act
                await studentDegreePlanService.UpdateDegreePlan4Async(degreePlanDto);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task UpdateDegreePlanAsync_ThrowsErrorForAdvisorWithReviewAssignedPermissionsAndProtectedUpdates()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ReviewAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock student repository get
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                student.AddAdvisor("0000111");
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));

                // Mock successful update of a plan from repository
                DegreePlan degreePlan = new DegreePlan(1, "0000894", 0);
                degreePlan.AddTerm("2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Domain.Student.Entities.DegreePlans.DegreePlan>())).Returns(Task.FromResult(degreePlan));
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<List<string>>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.DegreePlans.DegreePlan>>(new List<Domain.Student.Entities.DegreePlans.DegreePlan>() { degreePlan }));
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<int>())).Returns(Task.FromResult(degreePlan));

                // Mock dto Adapter 
                var degreePlanEntityToDtoAdapter = new DegreePlanEntity4Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan4>()).Returns(degreePlanEntityToDtoAdapter);

                // Mock entity adapter
                var degreePlanDtoToEntityAdapter = new DegreePlanDto4Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Dtos.Student.DegreePlans.DegreePlan4, Domain.Student.Entities.DegreePlans.DegreePlan>()).Returns(degreePlanDtoToEntityAdapter);

                // Map degree plan to dto
                var degreePlanDto = degreePlanEntityToDtoAdapter.MapToType(degreePlan);

                // Add review/protected changes to degree plan. Permission exception will be thrown because advisor permissions do not allow this action.
                DegreePlan originalDegreePlan = new DegreePlan(1, "0000894", 0);
                originalDegreePlan.AddTerm("2012/FA");
                originalDegreePlan.LastReviewedAdvisorId = "0101010";
                originalDegreePlan.AddCourse(new PlannedCourse("01", "12", Domain.Student.Entities.GradingType.Graded, Domain.Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, null) { IsProtected = true }, "2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<int>())).Returns(Task.FromResult(originalDegreePlan));

                // Act
                await studentDegreePlanService.UpdateDegreePlan4Async(degreePlanDto);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task UpdateDegreePlanAsync_ThrowsErrorForAdvisorWithViewAssignedPermissions()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock student repository get
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                student.AddAdvisor("0000111");
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));

                // Mock successful update of a plan from repository
                DegreePlan degreePlan = new DegreePlan(1, "0000894", 0);
                degreePlan.AddTerm("2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Domain.Student.Entities.DegreePlans.DegreePlan>())).Returns(Task.FromResult(degreePlan));
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<List<string>>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.DegreePlans.DegreePlan>>(new List<Domain.Student.Entities.DegreePlans.DegreePlan>() { degreePlan }));
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<int>())).Returns(Task.FromResult(degreePlan));

                // Mock dto Adapter 
                var degreePlanEntityToDtoAdapter = new DegreePlanEntity4Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan4>()).Returns(degreePlanEntityToDtoAdapter);

                // Mock entity adapter
                var degreePlanDtoToEntityAdapter = new DegreePlanDto4Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Dtos.Student.DegreePlans.DegreePlan4, Domain.Student.Entities.DegreePlans.DegreePlan>()).Returns(degreePlanDtoToEntityAdapter);

                // Map degree plan to dto
                var degreePlanDto = degreePlanEntityToDtoAdapter.MapToType(degreePlan);

                // Add review/protected changes to degree plan. Permission exception will be thrown if advisor permissions do not allow this action.
                DegreePlan originalDegreePlan = new DegreePlan(1, "0000894", 0);
                originalDegreePlan.AddTerm("2012/FA");
                originalDegreePlan.LastReviewedAdvisorId = "0101010";
                originalDegreePlan.AddCourse(new PlannedCourse("01", "12", Domain.Student.Entities.GradingType.Graded, Domain.Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, null) { IsProtected = true }, "2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<int>())).Returns(Task.FromResult(originalDegreePlan));

                // Act
                await studentDegreePlanService.UpdateDegreePlan4Async(degreePlanDto);
            }
        }

        [TestClass]
        public class DegreePlanService_TestAdvisorPermissions5 : DegreePlanServiceTestsSetup
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
                student.AddAdvisement("0000111", null, null, null);
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));

                activeAcademicCredits = new List<Domain.Student.Entities.AcademicCredit>();
                // Mock academic credit repository get using student ids returning dict with student's academic credits
                var academicCreditDict = new Dictionary<string, List<Domain.Student.Entities.AcademicCredit>>();
                academicCreditDict["0000894"] = activeAcademicCredits.ToList();
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

                // Mock get requirements for requisite validation 
                var requirementCodes = new List<string>() { };
                var requirements = (await new TestRequirementRepository().GetAsync(requirementCodes)).Where(r => r.RequirementType.Code == "PRE");
                requirementRepoMock.Setup(x => x.GetAsync(It.IsAny<IEnumerable<string>>())).Returns(Task.FromResult(requirements));

                // Mock Adapter 
                var degreePlanPreviewDtoAdapter = new AutoMapperAdapter<DegreePlanPreview, Dtos.Planning.DegreePlanPreview4>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<DegreePlanPreview, Dtos.Planning.DegreePlanPreview4>()).Returns(degreePlanPreviewDtoAdapter);
                var degreePlanDtoAdapter = new AutoMapperAdapter<DegreePlan, Dtos.Student.DegreePlans.DegreePlan4>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<DegreePlan, Dtos.Student.DegreePlans.DegreePlan4>()).Returns(degreePlanDtoAdapter);
                var degreePlanApprovalDtoAdapter = new AutoMapperAdapter<DegreePlanApproval, Dtos.Student.DegreePlans.DegreePlanApproval2>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<DegreePlanApproval, Dtos.Student.DegreePlans.DegreePlanApproval2>()).Returns(degreePlanApprovalDtoAdapter);


                // Mock degree plan4 dto Adapter 
                var degreePlanEntityToDegreePlan3DtoAdapter = new AutoMapperAdapter<DegreePlan, Dtos.Student.DegreePlans.DegreePlan4>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<DegreePlan, Dtos.Student.DegreePlans.DegreePlan4>()).Returns(degreePlanEntityToDegreePlan3DtoAdapter);

                // Mock conversion from academic credits to academic history
                var studentAcademicCredits = await new TestAcademicCreditRepository().GetAsync();
                Domain.Student.Entities.GradeRestriction gradeRestriction = new Domain.Student.Entities.GradeRestriction(false);
                var historyEntity = new Domain.Student.Entities.AcademicHistory(studentAcademicCredits, gradeRestriction, null);
                historyEntity.StudentId = "0000894";
                var historyDto = new AcademicHistoryEntityToAcademicHistory2DtoAdapter(adapterRegistry, logger).MapToType(historyEntity);
                academicHistoryServiceMock.Setup(svc => svc.ConvertAcademicCreditsToAcademicHistoryDtoAsync(historyEntity.StudentId, It.IsAny<IEnumerable<Domain.Student.Entities.AcademicCredit>>(), It.IsAny<Domain.Student.Entities.Student>()))
                    .Returns(Task.FromResult(historyDto));

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
            public async Task GetDegreePlan5Async_AllowedForAdvisorWithViewAnyAdviseePermissions()
            {
                // Set up view permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock student repository get
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));

                // Attempt to get degree plan
                await studentDegreePlanService.GetDegreePlan5Async(2);
            }

            [TestMethod]
            public async Task GetDegreePlan5Async_AllowedForAdvisorWithReviewAnyAdviseePermissions()
            {
                // Set up view permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ReviewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock student repository get
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));

                // Attempt to get degree plan
                await studentDegreePlanService.GetDegreePlan5Async(2);
            }

            [TestMethod]
            public async Task GetDegreePlan5Async_AllowedForAdvisorWithUpdateAnyAdviseePermissions()
            {
                // Set up view permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.UpdateAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock student repository get
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));

                // Attempt to get degree plan
                await studentDegreePlanService.GetDegreePlan5Async(2);
            }

            [TestMethod]
            public async Task GetDegreePlan5Async_AllowedForAdvisorWithAllAccessAnyAdviseePermissions()
            {
                // Set up view permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock student repository get
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));

                // Attempt to get degree plan
                await studentDegreePlanService.GetDegreePlan5Async(2);
            }

            [TestMethod]
            public async Task GetDegreePlan5Async_AllowedForAdvisorWithViewAssignedAdviseesPermissions()
            {
                // Set up view permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock student repository get--student with this advisor
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                student.AddAdvisor("0000111");
                student.AddAdvisement("0000111", null, null, null);
                //studentRepoMock.Setup(x => x.GetAsync("0000894")).ReturnsAsync(student);
                StudentAccess studentAccess = student.ConvertToStudentAccess();
                studentRepoMock.Setup(repo => repo.GetStudentAccessAsync(It.IsAny<List<string>>())).Returns(Task.FromResult(new List<StudentAccess>() { studentAccess }.AsEnumerable()));

                // Attempt to get degree plan
                await studentDegreePlanService.GetDegreePlan5Async(2);
            }

            [TestMethod]
            public async Task GetDegreePlan5Async_AllowedForAdvisorWithReviewAssignedAdviseesPermissions()
            {
                // Set up view permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ReviewAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock student repository get--student with this advisor
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                student.AddAdvisement("0000111", null, null, null);
                //studentRepoMock.Setup(x => x.GetAsync("0000894")).ReturnsAsync(student);
                StudentAccess studentAccess = student.ConvertToStudentAccess();
                studentRepoMock.Setup(repo => repo.GetStudentAccessAsync(It.IsAny<List<string>>())).Returns(Task.FromResult(new List<StudentAccess>() { studentAccess }.AsEnumerable()));

                // Attempt to get degree plan
                await studentDegreePlanService.GetDegreePlan5Async(2);
            }

            [TestMethod]
            public async Task GetDegreePlan5Async_AllowedForAdvisorWithUpdateAssignedAdviseesPermissions()
            {
                // Set up view permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.UpdateAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock student repository get--student with this advisor
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                student.AddAdvisement("0000111", null, null, null);
                //studentRepoMock.Setup(x => x.GetAsync("0000894")).ReturnsAsync(student);
                StudentAccess studentAccess = student.ConvertToStudentAccess();
                studentRepoMock.Setup(repo => repo.GetStudentAccessAsync(It.IsAny<List<string>>())).Returns(Task.FromResult(new List<StudentAccess>() { studentAccess }.AsEnumerable()));

                // Attempt to get degree plan
                await studentDegreePlanService.GetDegreePlan5Async(2);
            }

            [TestMethod]
            public async Task GetDegreePlan5Async_AllowedForAdvisorWithAllAccessAssignedAdviseesPermissions()
            {
                // Set up view permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock student repository get--student with this advisor
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                student.AddAdvisement("0000111", null, null, null);
                //studentRepoMock.Setup(x => x.GetAsync("0000894")).ReturnsAsync(student);
                StudentAccess studentAccess = student.ConvertToStudentAccess();
                studentRepoMock.Setup(repo => repo.GetStudentAccessAsync(It.IsAny<List<string>>())).Returns(Task.FromResult(new List<StudentAccess>() { studentAccess }.AsEnumerable()));

                // Attempt to get degree plan
                await studentDegreePlanService.GetDegreePlan5Async(2);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task GetDegreePlan5Async_ThrowsErrorForAdvisorWithoutAnyPermissions()
            {
                await studentDegreePlanService.GetDegreePlan5Async(2);
            }


            [TestMethod]
            public async Task UpdateDegreePlan5Async_AllowedForAdvisorWithUpdateAnyPermissions()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.UpdateAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock successful update of a plan from repository
                DegreePlan degreePlan = new DegreePlan(1, "0000894", 0);
                degreePlan.AddTerm("2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Domain.Student.Entities.DegreePlans.DegreePlan>())).Returns(Task.FromResult(degreePlan));
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<List<string>>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.DegreePlans.DegreePlan>>(new List<Domain.Student.Entities.DegreePlans.DegreePlan>() { degreePlan }));

                // Mock dto Adapter 
                var degreePlanEntityToDtoAdapter = new DegreePlanEntity4Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan4>()).Returns(degreePlanEntityToDtoAdapter);

                // Mock entity adapter
                var degreePlanDtoToEntityAdapter = new DegreePlanDto4Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Dtos.Student.DegreePlans.DegreePlan4, Domain.Student.Entities.DegreePlans.DegreePlan>()).Returns(degreePlanDtoToEntityAdapter);

                // Map degree plan to dto
                var degreePlanDto = degreePlanEntityToDtoAdapter.MapToType(degreePlan);

                // Add review/protected changes to degree plan. Permission exception would be thrown if advisor permissions did not allow this action.
                DegreePlan originalDegreePlan = new DegreePlan(1, "0000894", 0);
                originalDegreePlan.AddTerm("2012/FA");
                originalDegreePlan.LastReviewedAdvisorId = "0101010";
                originalDegreePlan.AddCourse(new PlannedCourse("01", "12", Domain.Student.Entities.GradingType.Graded, Domain.Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, null) { IsProtected = true }, "2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<int>())).Returns(Task.FromResult(originalDegreePlan));

                // Act
                await studentDegreePlanService.UpdateDegreePlan5Async(degreePlanDto);
            }

            [TestMethod]
            public async Task UpdateDegreePlan5Async_AllowedForAdvisorWithUpdateAssignedPermissions()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.UpdateAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock student repository get
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                student.AddAdvisor("0000111");
                student.AddAdvisement("0000111", null, null, null);
                var studentAccess = student.ConvertToStudentAccess();
                studentRepoMock.Setup(x => x.GetStudentAccessAsync(new List<string>() { "0000894" })).Returns(Task.FromResult(new List<StudentAccess>() { studentAccess }.AsEnumerable()));

                // Mock successful update of a plan from repository
                DegreePlan degreePlan = new DegreePlan(1, "0000894", 0);
                degreePlan.AddTerm("2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Domain.Student.Entities.DegreePlans.DegreePlan>())).Returns(Task.FromResult(degreePlan));
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<List<string>>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.DegreePlans.DegreePlan>>(new List<Domain.Student.Entities.DegreePlans.DegreePlan>() { degreePlan }));

                // Mock dto Adapter 
                var degreePlanEntityToDtoAdapter = new DegreePlanEntity4Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan4>()).Returns(degreePlanEntityToDtoAdapter);

                // Mock entity adapter
                var degreePlanDtoToEntityAdapter = new DegreePlanDto4Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Dtos.Student.DegreePlans.DegreePlan4, Domain.Student.Entities.DegreePlans.DegreePlan>()).Returns(degreePlanDtoToEntityAdapter);

                // Map degree plan to dto
                var degreePlanDto = degreePlanEntityToDtoAdapter.MapToType(degreePlan);

                // Add review/protected changes to degree plan. Permission exception would be thrown if advisor permissions did not allow this action.
                DegreePlan originalDegreePlan = new DegreePlan(1, "0000894", 0);
                originalDegreePlan.AddTerm("2012/FA");
                originalDegreePlan.LastReviewedAdvisorId = "0101010";
                originalDegreePlan.AddCourse(new PlannedCourse("01", "12", Domain.Student.Entities.GradingType.Graded, Domain.Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, null) { IsProtected = true }, "2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<int>())).Returns(Task.FromResult(originalDegreePlan));

                // Act
                await studentDegreePlanService.UpdateDegreePlan5Async(degreePlanDto);
            }

            [TestMethod]
            public async Task UpdateDegreePlan5Async_AllowedForAdvisorWithAllAccessAnyPermissions()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock successful update of a plan from repository
                DegreePlan degreePlan = new DegreePlan(1, "0000894", 0);
                degreePlan.AddTerm("2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Domain.Student.Entities.DegreePlans.DegreePlan>())).Returns(Task.FromResult(degreePlan));
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<List<string>>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.DegreePlans.DegreePlan>>(new List<Domain.Student.Entities.DegreePlans.DegreePlan>() { degreePlan }));

                // Mock dto Adapter 
                var degreePlanEntityToDtoAdapter = new DegreePlanEntity4Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan4>()).Returns(degreePlanEntityToDtoAdapter);
                // Mock entity adapter
                var degreePlanDtoToEntityAdapter = new DegreePlanDto4Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Dtos.Student.DegreePlans.DegreePlan4, Domain.Student.Entities.DegreePlans.DegreePlan>()).Returns(degreePlanDtoToEntityAdapter);

                // Map degree plan to dto
                var degreePlanDto = degreePlanEntityToDtoAdapter.MapToType(degreePlan);

                // Add review/protected changes to degree plan. Permission exception would be thrown if advisor permissions did not allow this action.
                DegreePlan originalDegreePlan = new DegreePlan(1, "0000894", 0);
                originalDegreePlan.AddTerm("2012/FA");
                originalDegreePlan.LastReviewedAdvisorId = "0101010";
                originalDegreePlan.AddCourse(new PlannedCourse("01", "12", Domain.Student.Entities.GradingType.Graded, Domain.Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, null) { IsProtected = true }, "2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<int>())).Returns(Task.FromResult(originalDegreePlan));

                // Act
                await studentDegreePlanService.UpdateDegreePlan5Async(degreePlanDto);
            }

            [TestMethod]
            public async Task UpdateDegreePlan5Async_AllowedForAdvisorWithAllAccessAssignedPermissions()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock student repository get
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                student.AddAdvisor("0000111");
                student.AddAdvisement("0000111", null, null, null);
                var studentAccess = student.ConvertToStudentAccess();
                studentRepoMock.Setup(x => x.GetStudentAccessAsync(new List<string>() { "0000894" })).Returns(Task.FromResult(new List<StudentAccess>() { studentAccess }.AsEnumerable()));

                // Mock successful update of a plan from repository
                DegreePlan degreePlan = new DegreePlan(1, "0000894", 0);
                degreePlan.AddTerm("2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Domain.Student.Entities.DegreePlans.DegreePlan>())).Returns(Task.FromResult(degreePlan));
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<List<string>>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.DegreePlans.DegreePlan>>(new List<Domain.Student.Entities.DegreePlans.DegreePlan>() { degreePlan }));

                // Mock dto Adapter 
                var degreePlanEntityToDtoAdapter = new DegreePlanEntity4Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan4>()).Returns(degreePlanEntityToDtoAdapter);

                // Mock entity adapter
                var degreePlanDtoToEntityAdapter = new DegreePlanDto4Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Dtos.Student.DegreePlans.DegreePlan4, Domain.Student.Entities.DegreePlans.DegreePlan>()).Returns(degreePlanDtoToEntityAdapter);

                // Map degree plan to dto
                var degreePlanDto = degreePlanEntityToDtoAdapter.MapToType(degreePlan);

                // Add review/protected changes to degree plan. Permission exception would be thrown if advisor permissions did not allow this action.
                DegreePlan originalDegreePlan = new DegreePlan(1, "0000894", 0);
                originalDegreePlan.AddTerm("2012/FA");
                originalDegreePlan.LastReviewedAdvisorId = "0101010";
                originalDegreePlan.AddCourse(new PlannedCourse("01", "12", Domain.Student.Entities.GradingType.Graded, Domain.Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, null) { IsProtected = true }, "2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<int>())).Returns(Task.FromResult(originalDegreePlan));

                // Act
                await studentDegreePlanService.UpdateDegreePlan5Async(degreePlanDto);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task UpdateDegreePlan5Async_UserNotAssignedAdvisor()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.UpdateAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock student repository get
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                student.AddAdvisor("0000222");
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));

                // Mock successful update of a plan from repository
                DegreePlan degreePlan = new DegreePlan(1, "0000894", 0);
                degreePlan.AddTerm("2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Domain.Student.Entities.DegreePlans.DegreePlan>())).Returns(Task.FromResult(degreePlan));
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<List<string>>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.DegreePlans.DegreePlan>>(new List<Domain.Student.Entities.DegreePlans.DegreePlan>() { degreePlan }));

                // Mock dto Adapter 
                var degreePlanEntityToDtoAdapter = new DegreePlanEntity4Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan4>()).Returns(degreePlanEntityToDtoAdapter);

                // Mock entity adapter
                var degreePlanDtoToEntityAdapter = new DegreePlanDto4Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Dtos.Student.DegreePlans.DegreePlan4, Domain.Student.Entities.DegreePlans.DegreePlan>()).Returns(degreePlanDtoToEntityAdapter);

                // Map degree plan to dto
                var degreePlanDto = degreePlanEntityToDtoAdapter.MapToType(degreePlan);

                // Add review/protected changes to degree plan. Permission exception thrown because advisor permissions do not allow this action.
                DegreePlan originalDegreePlan = new DegreePlan(1, "0000894", 0);
                originalDegreePlan.AddTerm("2012/FA");
                originalDegreePlan.LastReviewedAdvisorId = "0101010";
                originalDegreePlan.AddCourse(new PlannedCourse("01", "12", Domain.Student.Entities.GradingType.Graded, Domain.Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, null) { IsProtected = true }, "2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<int>())).Returns(Task.FromResult(originalDegreePlan));

                // Act
                await studentDegreePlanService.UpdateDegreePlan5Async(degreePlanDto);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task UpdateDegreePlan5Async_ThrowsErrorForAdvisorWithViewAnyPermissions()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock successful update of a plan from repository
                DegreePlan degreePlan = new DegreePlan(1, "0000894", 0);
                degreePlan.AddTerm("2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Domain.Student.Entities.DegreePlans.DegreePlan>())).Returns(Task.FromResult(degreePlan));
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<List<string>>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.DegreePlans.DegreePlan>>(new List<Domain.Student.Entities.DegreePlans.DegreePlan>() { degreePlan }));
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<int>())).Returns(Task.FromResult(degreePlan));

                // Mock dto Adapter 
                var degreePlanEntityToDtoAdapter = new DegreePlanEntity4Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan4>()).Returns(degreePlanEntityToDtoAdapter);

                // Mock entity adapter
                var degreePlanDtoToEntityAdapter = new DegreePlanDto4Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Dtos.Student.DegreePlans.DegreePlan4, Domain.Student.Entities.DegreePlans.DegreePlan>()).Returns(degreePlanDtoToEntityAdapter);

                // Map degree plan to dto
                var degreePlanDto = degreePlanEntityToDtoAdapter.MapToType(degreePlan);

                // Act
                await studentDegreePlanService.UpdateDegreePlan5Async(degreePlanDto);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task UpdateDegreePlan5Async_ThrowsErrorForAdvisorWithReviewAnyPermissionsAndProtectedUpdates()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ReviewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock successful update of a plan from repository
                DegreePlan degreePlan = new DegreePlan(1, "0000894", 0);
                degreePlan.AddTerm("2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Domain.Student.Entities.DegreePlans.DegreePlan>())).Returns(Task.FromResult(degreePlan));
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<List<string>>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.DegreePlans.DegreePlan>>(new List<Domain.Student.Entities.DegreePlans.DegreePlan>() { degreePlan }));
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<int>())).Returns(Task.FromResult(degreePlan));

                // Mock dto Adapter 
                var degreePlanEntityToDtoAdapter = new DegreePlanEntity4Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan4>()).Returns(degreePlanEntityToDtoAdapter);

                // Mock entity adapter
                var degreePlanDtoToEntityAdapter = new DegreePlanDto4Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Dtos.Student.DegreePlans.DegreePlan4, Domain.Student.Entities.DegreePlans.DegreePlan>()).Returns(degreePlanDtoToEntityAdapter);

                // Map degree plan to dto
                var degreePlanDto = degreePlanEntityToDtoAdapter.MapToType(degreePlan);

                // Add review/protected changes to degree plan. Permission exception will be thrown because advisor permissions do not allow this action.
                DegreePlan originalDegreePlan = new DegreePlan(1, "0000894", 0);
                originalDegreePlan.AddTerm("2012/FA");
                originalDegreePlan.LastReviewedAdvisorId = "0101010";
                originalDegreePlan.AddCourse(new PlannedCourse("01", "12", Domain.Student.Entities.GradingType.Graded, Domain.Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, null) { IsProtected = true }, "2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<int>())).Returns(Task.FromResult(originalDegreePlan));

                // Act
                await studentDegreePlanService.UpdateDegreePlan5Async(degreePlanDto);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task UpdateDegreePlan5Async_ThrowsErrorForAdvisorWithReviewAssignedPermissionsAndProtectedUpdates()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ReviewAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock student repository get
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                student.AddAdvisor("0000111");
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));

                // Mock successful update of a plan from repository
                DegreePlan degreePlan = new DegreePlan(1, "0000894", 0);
                degreePlan.AddTerm("2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Domain.Student.Entities.DegreePlans.DegreePlan>())).Returns(Task.FromResult(degreePlan));
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<List<string>>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.DegreePlans.DegreePlan>>(new List<Domain.Student.Entities.DegreePlans.DegreePlan>() { degreePlan }));
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<int>())).Returns(Task.FromResult(degreePlan));

                // Mock dto Adapter 
                var degreePlanEntityToDtoAdapter = new DegreePlanEntity4Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan4>()).Returns(degreePlanEntityToDtoAdapter);

                // Mock entity adapter
                var degreePlanDtoToEntityAdapter = new DegreePlanDto4Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Dtos.Student.DegreePlans.DegreePlan4, Domain.Student.Entities.DegreePlans.DegreePlan>()).Returns(degreePlanDtoToEntityAdapter);

                // Map degree plan to dto
                var degreePlanDto = degreePlanEntityToDtoAdapter.MapToType(degreePlan);

                // Add review/protected changes to degree plan. Permission exception will be thrown because advisor permissions do not allow this action.
                DegreePlan originalDegreePlan = new DegreePlan(1, "0000894", 0);
                originalDegreePlan.AddTerm("2012/FA");
                originalDegreePlan.LastReviewedAdvisorId = "0101010";
                originalDegreePlan.AddCourse(new PlannedCourse("01", "12", Domain.Student.Entities.GradingType.Graded, Domain.Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, null) { IsProtected = true }, "2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<int>())).Returns(Task.FromResult(originalDegreePlan));

                // Act
                await studentDegreePlanService.UpdateDegreePlan5Async(degreePlanDto);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task UpdateDegreePlan5Async_ThrowsErrorForAdvisorWithViewAssignedPermissions()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock student repository get
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                student.AddAdvisor("0000111");
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));

                // Mock successful update of a plan from repository
                DegreePlan degreePlan = new DegreePlan(1, "0000894", 0);
                degreePlan.AddTerm("2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Domain.Student.Entities.DegreePlans.DegreePlan>())).Returns(Task.FromResult(degreePlan));
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<List<string>>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.DegreePlans.DegreePlan>>(new List<Domain.Student.Entities.DegreePlans.DegreePlan>() { degreePlan }));
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<int>())).Returns(Task.FromResult(degreePlan));

                // Mock dto Adapter 
                var degreePlanEntityToDtoAdapter = new DegreePlanEntity4Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlan, Dtos.Student.DegreePlans.DegreePlan4>()).Returns(degreePlanEntityToDtoAdapter);

                // Mock entity adapter
                var degreePlanDtoToEntityAdapter = new DegreePlanDto4Adapter(adapterRegistryMock.Object, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Dtos.Student.DegreePlans.DegreePlan4, Domain.Student.Entities.DegreePlans.DegreePlan>()).Returns(degreePlanDtoToEntityAdapter);

                // Map degree plan to dto
                var degreePlanDto = degreePlanEntityToDtoAdapter.MapToType(degreePlan);

                // Add review/protected changes to degree plan. Permission exception will be thrown if advisor permissions do not allow this action.
                DegreePlan originalDegreePlan = new DegreePlan(1, "0000894", 0);
                originalDegreePlan.AddTerm("2012/FA");
                originalDegreePlan.LastReviewedAdvisorId = "0101010";
                originalDegreePlan.AddCourse(new PlannedCourse("01", "12", Domain.Student.Entities.GradingType.Graded, Domain.Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, null) { IsProtected = true }, "2012/FA");
                studentDegreePlanRepoMock.Setup(x => x.GetAsync(It.IsAny<int>())).Returns(Task.FromResult(originalDegreePlan));

                // Act
                await studentDegreePlanService.UpdateDegreePlan5Async(degreePlanDto);
            }

            [TestMethod]
            public async Task CreateDegreePlan5Async_AllowedForAdvisorWithAllAccessAnyAdviseePermissions()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock the get from the student repository (Create tests)
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                student.AddAdvisor("0000111");
                student.AddAdvisement("0000111", null, null, null);
                // Still need to mock student repo, since CreateDegreePlan needs to get the student
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));

                // Mock the get of the planning terms (Create tests)
                var allTerms = await new TestTermRepository().GetAsync();
                termRepoMock.Setup(x => x.GetAsync()).ReturnsAsync(allTerms);

                // Mock the get of the student programs (Create tests)
                var studentPrograms = await new TestStudentProgramRepository().GetAsync("0000896");
                studentProgramRepoMock.Setup(x => x.GetAsync("0000896")).ReturnsAsync(studentPrograms);

                // Mock an empty program 
                IEnumerable<string> depts = new List<string>() { "Dept1" };
                IEnumerable<Domain.Student.Entities.Requirements.Program> program =
                    new List<Domain.Student.Entities.Requirements.Program>() { new Domain.Student.Entities.Requirements.Program("EmptyRequirements", "Empty Requirements", depts, true, "UG", new CreditFilter(), false) };
                programRepoMock.Setup(pr => pr.GetAsync()).Returns(Task.FromResult(program));

                // Attempt to get degree plan
                await studentDegreePlanService.CreateDegreePlan5Async("0000894");
            }

            [TestMethod]
            public async Task CreateDegreePlan5Async_AllowedForAdvisorWithAllAccessAssignedAdviseesPermissions()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock the get from the student repository (Create tests)
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                student.AddAdvisement("0000111", null, null, null);
                // Still need to mock student repo, since CreateDegreePlan needs to get the student
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));

                // Mock the get of the planning terms (Create tests)
                var allTerms = await new TestTermRepository().GetAsync();
                termRepoMock.Setup(x => x.GetAsync()).ReturnsAsync(allTerms);

                // Mock the get of the student programs (Create tests)
                var studentPrograms = await new TestStudentProgramRepository().GetAsync("0000896");
                studentProgramRepoMock.Setup(x => x.GetAsync("0000896")).ReturnsAsync(studentPrograms);

                // Mock an empty program 
                IEnumerable<string> depts = new List<string>() { "Dept1" };
                IEnumerable<Domain.Student.Entities.Requirements.Program> program =
                    new List<Domain.Student.Entities.Requirements.Program>() { new Domain.Student.Entities.Requirements.Program("EmptyRequirements", "Empty Requirements", depts, true, "UG", new CreditFilter(), false) };
                programRepoMock.Setup(pr => pr.GetAsync()).Returns(Task.FromResult(program));

                // Attempt to get degree plan
                await studentDegreePlanService.CreateDegreePlan5Async("0000894");
            }

            [TestMethod]
            public async Task CreateDegreePlan5Async_AllowedForAdvisorWithUpdateAnyPermissions()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.UpdateAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock the get from the student repository (Create tests)
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));

                // Mock the get of the planning terms (Create tests)
                var allTerms = await new TestTermRepository().GetAsync();
                termRepoMock.Setup(x => x.GetAsync()).ReturnsAsync(allTerms);

                // Mock the get of the student programs (Create tests)
                var studentPrograms = await new TestStudentProgramRepository().GetAsync("0000896");
                studentProgramRepoMock.Setup(x => x.GetAsync("0000896")).ReturnsAsync(studentPrograms);

                // Mock an empty program 
                IEnumerable<string> depts = new List<string>() { "Dept1" };
                IEnumerable<Domain.Student.Entities.Requirements.Program> program =
                    new List<Domain.Student.Entities.Requirements.Program>() { new Domain.Student.Entities.Requirements.Program("EmptyRequirements", "Empty Requirements", depts, true, "UG", new CreditFilter(), true) };
                programRepoMock.Setup(pr => pr.GetAsync()).Returns(Task.FromResult(program));

                // Attempt to get degree plan
                await studentDegreePlanService.CreateDegreePlan5Async("0000894");
            }

            [TestMethod]
            public async Task CreateDegreePlan5_AllowedForAdvisorWithUpdateAssignedPermissions()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.UpdateAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock the get from the student repository (Create tests)
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                student.AddAdvisement("0000111", null, null, null);
                // Still need to mock student repo, since CreateDegreePlan needs to get the student
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));

                // Mock the get of the planning terms (Create tests)
                var allTerms = await new TestTermRepository().GetAsync();
                termRepoMock.Setup(x => x.GetAsync()).ReturnsAsync(allTerms);

                // Mock the get of the student programs (Create tests)
                var studentPrograms = await new TestStudentProgramRepository().GetAsync("0000896");
                studentProgramRepoMock.Setup(x => x.GetAsync("0000896")).ReturnsAsync(studentPrograms);

                // Mock an empty program 
                IEnumerable<string> depts = new List<string>() { "Dept1" };
                IEnumerable<Domain.Student.Entities.Requirements.Program> program =
                    new List<Domain.Student.Entities.Requirements.Program>() { new Domain.Student.Entities.Requirements.Program("EmptyRequirements", "Empty Requirements", depts, true, "UG", new CreditFilter(), true) };
                programRepoMock.Setup(pr => pr.GetAsync()).Returns(Task.FromResult(program));

                // Attempt to get degree plan
                await studentDegreePlanService.CreateDegreePlan5Async("0000894");
            }

            [TestMethod]
            public async Task CreateDegreePlan5Async_AllowedForAdvisorWithReviewAnyPermissions()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ReviewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock the get from the student repository (Create tests)
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));

                // Mock the get of the planning terms (Create tests)
                var allTerms = await new TestTermRepository().GetAsync();
                termRepoMock.Setup(x => x.GetAsync()).ReturnsAsync(allTerms);

                // Mock the get of the student programs (Create tests)
                var studentPrograms = await new TestStudentProgramRepository().GetAsync("0000896");
                studentProgramRepoMock.Setup(x => x.GetAsync("0000896")).ReturnsAsync(studentPrograms);

                // Mock an empty program 
                IEnumerable<string> depts = new List<string>() { "Dept1" };
                IEnumerable<Domain.Student.Entities.Requirements.Program> program =
                    new List<Domain.Student.Entities.Requirements.Program>() { new Domain.Student.Entities.Requirements.Program("EmptyRequirements", "Empty Requirements", depts, true, "UG", new CreditFilter(), true) };
                programRepoMock.Setup(pr => pr.GetAsync()).Returns(Task.FromResult(program));

                // Attempt to get degree plan
                await studentDegreePlanService.CreateDegreePlan5Async("0000894");
            }

            [TestMethod]
            public async Task CreateDegreePlan5Async_AllowedForAdvisorWithReviewAssignedPermissions()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ReviewAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock the get from the student repository (Create tests)
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                student.AddAdvisement("0000111", null, null, null);
                // Still need to mock student repo, since CreateDegreePlan needs to get the student
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));

                // Mock the get of the planning terms (Create tests)
                var allTerms = await new TestTermRepository().GetAsync();
                termRepoMock.Setup(x => x.GetAsync()).ReturnsAsync(allTerms);

                // Mock the get of the student programs (Create tests)
                var studentPrograms = await new TestStudentProgramRepository().GetAsync("0000896");
                studentProgramRepoMock.Setup(x => x.GetAsync("0000896")).ReturnsAsync(studentPrograms);

                // Mock an empty program 
                IEnumerable<string> depts = new List<string>() { "Dept1" };
                IEnumerable<Domain.Student.Entities.Requirements.Program> program =
                    new List<Domain.Student.Entities.Requirements.Program>() { new Domain.Student.Entities.Requirements.Program("EmptyRequirements", "Empty Requirements", depts, true, "UG", new CreditFilter(), false) };
                programRepoMock.Setup(pr => pr.GetAsync()).Returns(Task.FromResult(program));

                // Attempt to get degree plan
                await studentDegreePlanService.CreateDegreePlan5Async("0000894");
            }

            [TestMethod]
            public async Task CreateDegreePlan5Async_AllowedForAdvisorWithViewAnyAdviseePermissions()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock the get from the student repository (Create tests)
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));
                studentRepoMock.Setup(x => x.GetAsync("0000896")).Returns(Task.FromResult(student));

                // Mock the get of the planning terms (Create tests)
                var allTerms = await new TestTermRepository().GetAsync();
                termRepoMock.Setup(x => x.GetAsync()).ReturnsAsync(allTerms);

                // Mock the get of the student programs (Create tests)
                var studentPrograms = await new TestStudentProgramRepository().GetAsync("0000896");
                studentProgramRepoMock.Setup(x => x.GetAsync("0000896")).ReturnsAsync(studentPrograms);
                studentProgramRepoMock.Setup(x => x.GetAsync("0000894")).ReturnsAsync(studentPrograms);

                // Mock an empty program 
                IEnumerable<string> depts = new List<string>() { "Dept1" };
                IEnumerable<Domain.Student.Entities.Requirements.Program> program =
                    new List<Domain.Student.Entities.Requirements.Program>() { new Domain.Student.Entities.Requirements.Program("EmptyRequirements", "Empty Requirements", depts, true, "UG", new CreditFilter(), false) };
                programRepoMock.Setup(pr => pr.GetAsync()).Returns(Task.FromResult(program));

                // Attempt to get degree plan
                await studentDegreePlanService.CreateDegreePlan5Async("0000894");
            }

            [TestMethod]
            public async Task CreateDegreePlan5Async_AllowedForAdvisorWithViewAssignedAdviseesPermissions()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Mock the get from the student repository (Create tests)
                var student = new Domain.Student.Entities.Student("0000894", "Smith", null, new List<string>(), new List<string>());
                student.AddAdvisement("0000111", null, null, null);
                // Still need to mock student repo, since CreateDegreePlan needs to get the student
                studentRepoMock.Setup(x => x.GetAsync("0000894")).Returns(Task.FromResult(student));

                // Mock the get of the planning terms (Create tests)
                var allTerms = await new TestTermRepository().GetAsync();
                termRepoMock.Setup(x => x.GetAsync()).ReturnsAsync(allTerms);

                // Mock the get of the student programs (Create tests)
                var studentPrograms = await new TestStudentProgramRepository().GetAsync("0000896");
                studentProgramRepoMock.Setup(x => x.GetAsync("0000896")).ReturnsAsync(studentPrograms);

                // Mock an empty program 
                IEnumerable<string> depts = new List<string>() { "Dept1" };
                IEnumerable<Domain.Student.Entities.Requirements.Program> program =
                    new List<Domain.Student.Entities.Requirements.Program>() { new Domain.Student.Entities.Requirements.Program("EmptyRequirements", "Empty Requirements", depts, true, "UG", new CreditFilter(), false) };
                programRepoMock.Setup(pr => pr.GetAsync()).Returns(Task.FromResult(program));

                // Attempt to get degree plan
                await studentDegreePlanService.CreateDegreePlan5Async("0000894");
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CreateDegreePlan5Async_ThrowsErrorForAdvisorWithoutPermissions()
            {
                // Attempt to get degree plan
                await studentDegreePlanService.CreateDegreePlan5Async("0000894");
            }
        }
    }
}
