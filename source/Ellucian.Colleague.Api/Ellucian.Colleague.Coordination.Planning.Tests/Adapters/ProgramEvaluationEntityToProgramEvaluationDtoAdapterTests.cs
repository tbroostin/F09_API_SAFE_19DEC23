// Copyright 2012-2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Planning.Adapters;
using Ellucian.Colleague.Coordination.Planning.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Planning.Entities;
using Ellucian.Colleague.Domain.Planning.Repositories;
using Ellucian.Colleague.Domain.Planning.Tests;
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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Planning.Tests.Adapters
{
    [TestClass]
    public class ProgramEvaluationEntityToProgramEvaluationDtoAdapterTests : CurrentUserSetup
    {
        private IProgramRequirementsRepository programRequirementsRepo;
        private IProgramEvaluationService programEvaluationService;
        private IStudentRepository studentRepo;
        private IPlanningStudentRepository planningStudentRepo;
        private Mock<IPlanningStudentRepository> planningStudentRepoMock;
        private IApplicantRepository applicantRepo;
        private Mock<IApplicantRepository> applicantRepoMock;
        private IStudentProgramRepository studentProgramRepo;
        private IRequirementRepository requirementRepo;
        private Mock<IAcademicCreditRepository> academicCreditRepoMock;
        private IAcademicCreditRepository academicCreditRepo;
        private IAcademicCreditRepository academicCreditRepoInstance = new TestAcademicCreditRepository();
        private TestStudentDegreePlanRepository studentDegreePlanRepo;
        private Mock<IDegreePlanRepository> degreePlanRepoMock;
        private IDegreePlanRepository degreePlanRepo;
        private ICourseRepository courseRepo;
        private ITermRepository termRepo;
        private IRuleRepository ruleRepo;
        private IProgramRepository programRepo;
        private ICatalogRepository catalogRepo;
        private IPlanningConfigurationRepository planningConfigRepo;
        private IReferenceDataRepository referenceDataRepo;
        private IAdapterRegistry adapterRegistry;
        private ICurrentUserFactory currentUserFactory;
        private IRoleRepository roleRepository;
        private Mock<IRoleRepository> roleRepoMock;
        private IConfigurationRepository baseConfigurationRepository;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

        private ILogger logger;
        private string studentId = "0000001";

        [TestInitialize]
        // Test repositories for test data
        public  void Initialize()
        {
            studentDegreePlanRepo = new TestStudentDegreePlanRepository();
            degreePlanRepoMock = new Mock<IDegreePlanRepository>();
            degreePlanRepo = degreePlanRepoMock.Object;

            studentRepo = new TestStudentRepository();
            planningStudentRepoMock = new Mock<IPlanningStudentRepository>();
            planningStudentRepo = planningStudentRepoMock.Object;
            applicantRepoMock = new Mock<IApplicantRepository>();
            applicantRepo = applicantRepoMock.Object;
            studentProgramRepo = new TestStudentProgramRepository();
            programRequirementsRepo = new TestProgramRequirementsRepository();
            referenceDataRepo = new Mock<IReferenceDataRepository>().Object;
            academicCreditRepoMock = new Mock<IAcademicCreditRepository>();
            academicCreditRepo = academicCreditRepoMock.Object;
            academicCreditRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string[]>(), false, true, It.IsAny<bool>())).Returns((string[] s, bool b1, bool b2, bool b3) => Task.FromResult(academicCreditRepoInstance.GetAsync(s, b1, b2, b3).Result));
            academicCreditRepoMock.Setup(repo => repo.GetAcademicCreditByStudentIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns((IEnumerable<string> s, bool b1, bool b2, bool b3) => Task.FromResult(GetCredits(s)));

            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

            courseRepo = new TestCourseRepository();
            requirementRepo = new TestRequirementRepository();
            termRepo = new TestTermRepository();
            ruleRepo = new TestRuleRepository2();
            programRepo = new TestProgramRepository();
            catalogRepo = new TestCatalogRepository();
            planningConfigRepo = new TestPlanningConfigurationRepository();
            currentUserFactory = new StudentUserFactory();
            roleRepoMock = new Mock<IRoleRepository>();
            roleRepository = roleRepoMock.Object;

            // Set up user as "AllAccessAnyAdvisee" advisor so that evaluation is permitted
            currentUserFactory = new AdvisorUserFactory();
            // Set up view any advisee permissions on advisor's role--so that advisor has access to this student
            advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAnyAdvisee));
            roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Ellucian.Colleague.Domain.Entities.Role>() { advisorRole });

            Mock<IAdapterRegistry> adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            logger = new Mock<ILogger>().Object;

            var evalPlannedCourseDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Requirements.PlannedCredit, Ellucian.Colleague.Dtos.Student.Requirements.PlannedCredit>(adapterRegistryMock.Object, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Requirements.PlannedCredit, Ellucian.Colleague.Dtos.Student.Requirements.PlannedCredit>()).Returns(evalPlannedCourseDtoAdapter);

            // This won't work for additional requirements right now - TestRequirementRepo data doesn't match other data
            programEvaluationService = new ProgramEvaluationService(
                adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                requirementRepo, academicCreditRepo, degreePlanRepo, courseRepo, termRepo, ruleRepo, programRepo, catalogRepo, 
                planningConfigRepo,referenceDataRepo, currentUserFactory, roleRepository, logger, baseConfigurationRepository);
        }

        private Dictionary<string, List<AcademicCredit>> GetCredits(IEnumerable<string> studentIds)
        {
            var creditDictionary = new Dictionary<string, List<AcademicCredit>>();
            foreach (var id in studentIds)
            {
                var student = studentRepo.Get(id);
                var acadCreditIds = student.AcademicCreditIds;

                var credits = academicCreditRepoInstance.GetAsync(acadCreditIds).Result.ToList();

                creditDictionary.Add(id, credits);
            }

            return creditDictionary;
        }

        [TestMethod]
        public async Task  ProgramEvaluationDtoAdapter()
        {

            // default behavior of test prr is to create whatever programrequirements you specify with these requirements:
            // "MATH-100"  satisfied by acad cred id 8
            // "MATH-200"                           39
            // "ENGL-101"                           36
            // "ENGL-102"                           26      

            studentId = "00004002";
            var student = studentRepo.Get(studentId);
            var planningStudent = new PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
            planningStudentRepoMock.Setup(repo => repo.GetAsync(student.Id, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));

            var progEval = (await programEvaluationService.EvaluateAsync(studentId, new List<string>() { student.ProgramIds.First() }, null)).First();

            ProgramEvaluationDtoAdapter adapter = new ProgramEvaluationDtoAdapter(adapterRegistry, logger);

            var progEvalDto = adapter.MapToType(progEval);

            Assert.IsNotNull(progEvalDto);

            Assert.AreEqual(progEval.CatalogCode, progEvalDto.CatalogCode);
            Assert.AreEqual(progEval.ProgramCode, progEvalDto.ProgramCode);
            Assert.AreEqual(progEval.InstitutionalCredits, progEvalDto.InstitutionalCredits);
            //Assert.AreEqual(progEval.AllCredit.Count, progresult.AllCredit.Count);
            Assert.AreEqual(progEval.Credits, progEvalDto.Credits);
            Assert.AreEqual(progEval.RequirementResults.Count, progEvalDto.RequirementResults.Count);
            Assert.AreEqual(progEval.ProgramRequirements.MinimumCredits, progEvalDto.ProgramRequirements.MinimumCredits);
        }

        [TestMethod]
        public async Task ProgramEvaluationDtoAdapter_OtherPlannedCourses()
        {
            // This student and program will have OtherPlannedCourses in the evaluation result
            studentId = "00004002";
            var student = studentRepo.Get(studentId);
            var planningStudent = new PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
            planningStudentRepoMock.Setup(repo => repo.GetAsync(student.Id, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));
            var progEval = (await programEvaluationService.EvaluateAsync(studentId, new List<string>() { "SIMPLE1" }, null)).First();

            ProgramEvaluationDtoAdapter adapter = new ProgramEvaluationDtoAdapter(adapterRegistry, logger);

            var progEvalDto = adapter.MapToType(progEval);

            Assert.IsNotNull(progEvalDto);

            Assert.AreEqual(progEval.OtherPlannedCredits.Count(), progEvalDto.OtherPlannedCredits.Count());
            Assert.AreEqual(progEval.OtherPlannedCredits.ElementAt(0).Course.Id, progEvalDto.OtherPlannedCredits.ElementAt(0).CourseId);
            Assert.AreEqual(progEval.OtherPlannedCredits.ElementAt(0).TermCode, progEvalDto.OtherPlannedCredits.ElementAt(0).TermCode);
        }

    }
}
