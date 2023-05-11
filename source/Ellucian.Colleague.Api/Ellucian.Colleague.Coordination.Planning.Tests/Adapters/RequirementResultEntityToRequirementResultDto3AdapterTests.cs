// Copyright 2016-2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Planning.Services;
using Ellucian.Colleague.Coordination.Student.Adapters;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Planning.Entities;
using Ellucian.Colleague.Domain.Planning.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
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
    public class RequirementResultEntityToRequirementResultDto3AdapterTests : CurrentUserSetup
    {
        private IProgramRequirementsRepository programRequirementsRepo;
        private IProgramEvaluationService programEvaluationService;
        private IStudentRepository studentRepo;
        private Mock<IPlanningStudentRepository> planningStudentRepoMock;
        private IPlanningStudentRepository planningStudentRepo;
        private Mock<IApplicantRepository> applicantRepoMock;
        private IApplicantRepository applicantRepo;
        private IStudentProgramRepository studentProgramRepo;
        private IRequirementRepository requirementRepo;
        private Mock<IAcademicCreditRepository> academicCreditRepoMock;
        private IAcademicCreditRepository academicCreditRepo;
        private IAcademicCreditRepository academicCreditRepoInstance = new TestAcademicCreditRepository();
        private IStudentDegreePlanRepository studentDegreePlanRepo;
        private IDegreePlanRepository degreePlanRepo;
        private Mock<IDegreePlanRepository> degreePlanRepoMock;
        private ICourseRepository courseRepo;
        private ITermRepository termRepo;
        private IRuleRepository ruleRepo;
        private IAdapterRegistry adapterRegistry;
        private IProgramRepository programRepo;
        private ICatalogRepository catalogRepo;
        private IPlanningConfigurationRepository planningConfigRepo;
        private IReferenceDataRepository referenceDataRepo;
        private ILogger logger;
        private string studentId = "0000001";
        private ICurrentUserFactory currentUserFactory;
        private IRoleRepository roleRepository;
        private Mock<IRoleRepository> roleRepoMock;
        private IConfigurationRepository baseConfigurationRepository;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

        private Dictionary<string, List<Ellucian.Colleague.Domain.Student.Entities.AcademicCredit>> GetCredits(IEnumerable<string> studentIds)
        {
            var creditDictionary = new Dictionary<string, List<Ellucian.Colleague.Domain.Student.Entities.AcademicCredit>>();
            foreach (var id in studentIds)
            {
                var student = studentRepo.Get(id);
                var acadCreditIds = student.AcademicCreditIds;

                var credits = academicCreditRepoInstance.GetAsync(acadCreditIds).Result.ToList();

                creditDictionary.Add(id, credits);
            }

            return creditDictionary;
        }

        [TestInitialize]
        // Test repositories for test data
        public void Initialize()
        {
            studentDegreePlanRepo = new TestStudentDegreePlanRepository();
            degreePlanRepoMock = new Mock<IDegreePlanRepository>();
            degreePlanRepo = degreePlanRepoMock.Object;

            studentRepo = new TestStudentRepository();
            planningStudentRepoMock = new Mock<IPlanningStudentRepository>();
            planningStudentRepo = planningStudentRepoMock.Object;
            studentProgramRepo = new TestStudentProgramRepository();
            programRequirementsRepo = new TestProgramRequirementsRepository();
            
            academicCreditRepoMock = new Mock<IAcademicCreditRepository>();
            academicCreditRepo = academicCreditRepoMock.Object;
            referenceDataRepo = new Mock<IReferenceDataRepository>().Object;

            academicCreditRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string[]>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns((string[] s, bool b1, bool b2, bool b3) => Task.FromResult(academicCreditRepoInstance.GetAsync(s, b1, b2, b3).Result));
            academicCreditRepoMock.Setup(repo => repo.GetAcademicCreditByStudentIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns((IEnumerable<string> s, bool b1, bool b2, bool b3) => Task.FromResult(GetCredits(s)));

            courseRepo = new TestCourseRepository();
            requirementRepo = new TestRequirementRepository();
            termRepo = new TestTermRepository();
            ruleRepo = new TestRuleRepository2();
            programRepo = new TestProgramRepository();
            catalogRepo = new TestCatalogRepository();
            planningConfigRepo = null;
            currentUserFactory = new StudentUserFactory();
            roleRepoMock = new Mock<IRoleRepository>();
            roleRepository = roleRepoMock.Object;

            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

            // Set up user as "AllAccessAnyAdvisee" advisor so that evaluation is permitted
            currentUserFactory = new AdvisorUserFactory();
            // Set up view any advisee permissions on advisor's role--so that advisor has access to this student
            advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAnyAdvisee));
            roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Ellucian.Colleague.Domain.Entities.Role>() { advisorRole });
            Mock<IAdapterRegistry> adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;

            logger = new Mock<ILogger>().Object;

            // This won't work for additional requirements right now - TestRequirementRepo data doesn't match other data
            programEvaluationService = new ProgramEvaluationService(
                adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo,
                studentProgramRepo, requirementRepo, academicCreditRepo, degreePlanRepo, courseRepo, termRepo, 
                ruleRepo, programRepo, catalogRepo, planningConfigRepo, referenceDataRepo,  
                currentUserFactory, roleRepository, logger, baseConfigurationRepository);
        }

        [TestMethod]
        public void RequirementResultEntityToRequirementResult3DtoAdapter()
        {

            // default behavior of test prr is to create whatever programrequirements you specify with these requirements:
            // "MATH-100"  satisfied by acad cred id 8
            // "MATH-200"                           39
            // "ENGL-101"                           36
            // "ENGL-102"                           26      

            studentId = "00004002";
            var student = studentRepo.Get(studentId);
            var planningStudent = new Domain.Student.Entities.PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
            planningStudentRepoMock.Setup(repo => repo.GetAsync(student.Id, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));

            var progEval = programEvaluationService.EvaluateAsync(studentId, new List<string>() {student.ProgramIds.First()}, null).Result;
            progEval.First().RequirementResults.First().Explanations.Add(Ellucian.Colleague.Domain.Student.Entities.Requirements.RequirementExplanation.MinGpa);
            var adapter = new RequirementResultEntityToRequirementResult3DtoAdapter(adapterRegistry, logger);

            var source = progEval.First().RequirementResults.First();
            var target = adapter.MapToType(source);

            Assert.IsNotNull(target);

            //Assert.AreEqual(source.IsSatisfied(), target.IsSatisfied);

            Assert.AreEqual(source.Requirement.Id, target.RequirementId);
            Assert.AreEqual(source.SubRequirementResults.Count, target.SubrequirementResults.Count);
            Assert.IsTrue(target.MinGpaIsNotMet);
        }
    }
}
