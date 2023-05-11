// Copyright 2012-2022 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Web.Http.TestUtil;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Planning.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Entities.DegreePlans;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Domain.Planning.Tests;

using Ellucian.Colleague.Coordination.Planning.Services;

namespace Ellucian.Colleague.Coordination.Planning.Tests.Services
{
    [TestClass]
    public class ProgramEvaluationServiceTests
    {
        [TestClass]
        public class ProgramEvaluationServiceTests_Evaluate : CurrentUserSetup
        {
            private IAdapterRegistry adapterRegistry;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IProgramRequirementsRepository programRequirementsRepo;
            private IStudentRepository studentRepo;
            private Mock<IPlanningStudentRepository> planningStudentRepoMock;
            private IPlanningStudentRepository planningStudentRepo;
            private Mock<IApplicantRepository> applicantRepoMock;
            private IApplicantRepository applicantRepo;
            private Mock<IStudentProgramRepository> studentProgramRepoMock;
            private IStudentProgramRepository studentProgramRepo;
            private Mock<IAcademicCreditRepository> academicCreditRepoMock;
            private IAcademicCreditRepository academicCreditRepo;
            private IAcademicCreditRepository academicCreditRepoInstance = new TestAcademicCreditRepository();
            private Mock<IStudentDegreePlanRepository> studentDegreePlanRepoMock;
            private IStudentDegreePlanRepository studentDegreePlanRepo;
            private ICourseRepository courseRepo;
            private IRequirementRepository requirementRepo;
            private ITermRepository termRepo;
            private IRuleRepository ruleRepo;
            private IProgramRepository programRepo;
            private ICatalogRepository catalogRepo;
            private IPlanningConfigurationRepository planningConfigRepo;
            private IReferenceDataRepository referenceDataRepo;
            private IRoleRepository roleRepository;
            private Mock<IRoleRepository> roleRepoMock;
            private ProgramEvaluationService programEvaluationService;
            private Dumper dumper;
            private ILogger logger;
            private ICurrentUserFactory currentUserFactory;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

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

            [TestInitialize]
            public void Initialize()
            {
                planningStudentRepoMock = new Mock<IPlanningStudentRepository>();
                planningStudentRepo = planningStudentRepoMock.Object;

                applicantRepoMock = new Mock<IApplicantRepository>();
                applicantRepo = applicantRepoMock.Object;

                applicantRepoMock = new Mock<IApplicantRepository>();
                applicantRepo = applicantRepoMock.Object;

                academicCreditRepoMock = new Mock<IAcademicCreditRepository>();
                academicCreditRepo = academicCreditRepoMock.Object;

                referenceDataRepo = new Mock<IReferenceDataRepository>().Object;
                academicCreditRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string[]>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns((string[] s, bool b1, bool b2, bool b3) => Task.FromResult(academicCreditRepoInstance.GetAsync(s, b1, b2, b3).Result));
                academicCreditRepoMock.Setup(repo => repo.GetAcademicCreditByStudentIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns((IEnumerable<string> s, bool b1, bool b2, bool b3) => Task.FromResult(GetCredits(s)));

                studentDegreePlanRepoMock = new Mock<IStudentDegreePlanRepository>();
                studentProgramRepoMock = new Mock<IStudentProgramRepository>();

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;

                roleRepoMock = new Mock<IRoleRepository>();
                roleRepository = roleRepoMock.Object;

                logger = new Mock<ILogger>().Object;
                currentUserFactory = new StudentUserFactory();

                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                termRepo = new TestTermRepository();
                ruleRepo = new TestRuleRepository2();
                courseRepo = new TestCourseRepository();
                catalogRepo = new TestCatalogRepository();
                programRepo = new TestProgramRepository();
                requirementRepo = new TestRequirementRepository();

                studentRepo = new TestStudentRepository();
                studentProgramRepo = new TestStudentProgramRepository();
                planningConfigRepo = new TestPlanningConfigurationRepository();
                programRequirementsRepo = new TestProgramRequirementsRepository();

                studentDegreePlanRepo = new TestStudentDegreePlanRepository();

                dumper = new Dumper();
            }

            [TestMethod]
            public async Task UserIsSelf_ReturnsEvaluations()
            {
                // Arrange
                string studentid = "0000894";
                var programCodes = new List<string>() { "MATH.BS.BLANKTAKEONE", "MATH.BS.TOBEOPTIMIZED2" };

                var student = studentRepo.Get("0000894");
                var planningStudent = new Domain.Student.Entities.PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
                planningStudent.Advisements = student.Advisements;
                planningStudentRepoMock.Setup(psr => psr.GetAsync(studentid, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));

                // reinitialize the service to effect the advisor as the current user
                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo, catalogRepo, planningConfigRepo,
                    referenceDataRepo, currentUserFactory, roleRepository, logger, baseConfigurationRepository);

                // Act
                var evaluation = await programEvaluationService.EvaluateAsync(studentid, programCodes, null);

                // Assert
                Assert.IsTrue(evaluation is IEnumerable<ProgramEvaluation>);
                Assert.AreEqual(programCodes.ElementAt(0), evaluation.ElementAt(0).ProgramCode);
                Assert.IsTrue(evaluation.ElementAt(0).RequirementResults.Count() > 0);
                Assert.AreEqual(programCodes.ElementAt(1), evaluation.ElementAt(1).ProgramCode);
                Assert.IsTrue(evaluation.ElementAt(0).RequirementResults.Count() > 0);
            }

            [TestMethod]
            public async Task UserIsSelf_ReturnsEvaluationsWithCatalog()
            {
                // Arrange
                string studentid = "0000894";
                var programCodes = new List<string>() { "MATH.BS.BLANKTAKEONE", "MATH.BS.TOBEOPTIMIZED2" };

                var student = studentRepo.Get("0000894");
                var planningStudent = new Domain.Student.Entities.PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
                planningStudent.Advisements = student.Advisements;
                planningStudentRepoMock.Setup(psr => psr.GetAsync(studentid, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));

                // reinitialize the service to effect the advisor as the current user
                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo, catalogRepo, planningConfigRepo,
                    referenceDataRepo, currentUserFactory, roleRepository, logger, baseConfigurationRepository);

                // Act
                var evaluation = await programEvaluationService.EvaluateAsync(studentid, programCodes, "2012");

                // Assert
                Assert.IsTrue(evaluation is IEnumerable<ProgramEvaluation>);
                Assert.AreEqual(programCodes.ElementAt(0), evaluation.ElementAt(0).ProgramCode);
                Assert.IsTrue(evaluation.ElementAt(0).RequirementResults.Count() > 0);
                Assert.AreEqual(programCodes.ElementAt(1), evaluation.ElementAt(1).ProgramCode);
                Assert.IsTrue(evaluation.ElementAt(0).RequirementResults.Count() > 0);
            }


            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task NonExistent_Student_ThrowsException()
            {
                // Arrange
                string studentid = "0000894";
                var programCodes = new List<string>() { "MATH.BS.BLANKTAKEONE", "MATH.BS.TOBEOPTIMIZED2" };

                var student = studentRepo.Get("0000894");
                Domain.Student.Entities.PlanningStudent planningStudent = null;
                planningStudentRepoMock.Setup(psr => psr.GetAsync(studentid, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));

                // reinitialize the service to effect the advisor as the current user
                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo, catalogRepo, planningConfigRepo,
                    referenceDataRepo, currentUserFactory, roleRepository, logger, baseConfigurationRepository);

                // Act
                var evaluation = await programEvaluationService.EvaluateAsync(studentid, programCodes, null);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task NonExistent_Student_ThrowsExceptionWithCatalog()
            {
                // Arrange
                string studentid = "0000894";
                var programCodes = new List<string>() { "MATH.BS.BLANKTAKEONE", "MATH.BS.TOBEOPTIMIZED2" };

                var student = studentRepo.Get("0000894");
                Domain.Student.Entities.PlanningStudent planningStudent = null;
                planningStudentRepoMock.Setup(psr => psr.GetAsync(studentid, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));

                // reinitialize the service to effect the advisor as the current user
                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo, catalogRepo, planningConfigRepo,
                    referenceDataRepo, currentUserFactory, roleRepository, logger, baseConfigurationRepository);

                // Act
                var evaluation = await programEvaluationService.EvaluateAsync(studentid, programCodes, "2012");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task Student_Invalid_DegreePlan_ThrowsException()
            {
                // Arrange
                string studentid = "0000894";
                var programCodes = new List<string>() { "MATH.BS.BLANKTAKEONE", "MATH.BS.TOBEOPTIMIZED2" };

                var student = studentRepo.Get("0000894");
                var planningStudent = new Domain.Student.Entities.PlanningStudent(student.Id, student.LastName, 123, student.ProgramIds);
                planningStudent.Advisements = student.Advisements;
                planningStudentRepoMock.Setup(psr => psr.GetAsync(student.Id, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));
                DegreePlan dp = null;
                studentDegreePlanRepoMock.Setup(dpr => dpr.GetAsync(It.IsAny<int>())).ReturnsAsync(dp);
                studentDegreePlanRepo = studentDegreePlanRepoMock.Object;

                // reinitialize the service to effect the advisor as the current user
                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo, catalogRepo, planningConfigRepo,
                    referenceDataRepo, currentUserFactory, roleRepository, logger, baseConfigurationRepository);

                // Act
                var evaluation = await programEvaluationService.EvaluateAsync(studentid, programCodes, null);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task Student_Invalid_DegreePlan_ThrowsExceptionWithCatalog()
            {
                // Arrange
                string studentid = "0000894";
                var programCodes = new List<string>() { "MATH.BS.BLANKTAKEONE", "MATH.BS.TOBEOPTIMIZED2" };

                var student = studentRepo.Get("0000894");
                var planningStudent = new Domain.Student.Entities.PlanningStudent(student.Id, student.LastName, 123, student.ProgramIds);
                planningStudent.Advisements = student.Advisements;
                planningStudentRepoMock.Setup(psr => psr.GetAsync(student.Id, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));
                DegreePlan dp = null;
                studentDegreePlanRepoMock.Setup(dpr => dpr.GetAsync(It.IsAny<int>())).ReturnsAsync(dp);
                studentDegreePlanRepo = studentDegreePlanRepoMock.Object;

                // reinitialize the service to effect the advisor as the current user
                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo, catalogRepo, planningConfigRepo,
                    referenceDataRepo, currentUserFactory, roleRepository, logger, baseConfigurationRepository);

                // Act
                var evaluation = await programEvaluationService.EvaluateAsync(studentid, programCodes, "2012");
            }


            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task Student_Null_StudentProgram_ThrowsException()
            {
                // Arrange
                string studentid = "0000894";
                var programCodes = new List<string>() { "MATH.BS.BLANKTAKEONE", "MATH.BS.TOBEOPTIMIZED2" };

                var student = studentRepo.Get("0000894");
                var planningStudent = new Domain.Student.Entities.PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
                planningStudent.Advisements = student.Advisements;
                planningStudentRepoMock.Setup(psr => psr.GetAsync(student.Id, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));
                List<StudentProgram> programs = null;
                studentProgramRepoMock.Setup(spr => spr.GetStudentProgramsByIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>(), It.IsAny<Term>(), It.IsAny<bool>())).ReturnsAsync(programs);
                studentProgramRepo = studentProgramRepoMock.Object;

                // reinitialize the service to effect the advisor as the current user
                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo, catalogRepo, planningConfigRepo,
                    referenceDataRepo, currentUserFactory, roleRepository, logger, baseConfigurationRepository);

                // Act
                var evaluation = await programEvaluationService.EvaluateAsync(studentid, programCodes, null);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task Student_Empty_StudentProgram_ThrowsException()
            {
                // Arrange
                string studentid = "0000894";
                var programCodes = new List<string>() { "MATH.BS.BLANKTAKEONE", "MATH.BS.TOBEOPTIMIZED2" };

                var student = studentRepo.Get("0000894");
                var planningStudent = new Domain.Student.Entities.PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
                planningStudent.Advisements = student.Advisements;
                planningStudentRepoMock.Setup(psr => psr.GetAsync(student.Id, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));
                List<StudentProgram> programs = new List<StudentProgram>();
                studentProgramRepoMock.Setup(spr => spr.GetStudentProgramsByIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>(), It.IsAny<Term>(), It.IsAny<bool>())).ReturnsAsync(programs);
                studentProgramRepo = studentProgramRepoMock.Object;

                // reinitialize the service to effect the advisor as the current user
                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo, catalogRepo, planningConfigRepo,
                    referenceDataRepo, currentUserFactory, roleRepository, logger, baseConfigurationRepository);

                // Act
                var evaluation = await programEvaluationService.EvaluateAsync(studentid, programCodes, null);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task AdvisorWithViewAssignedAccess_NotAssigned_ThrowsException()
            {
                // Arrange
                var studentId = "00004013";
                var student = studentRepo.Get(studentId);
                var planningStudent = new Domain.Student.Entities.PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
                planningStudent.Advisements = student.Advisements;
                planningStudentRepoMock.Setup(psr => psr.GetAsync(student.Id, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));

                currentUserFactory = new AdvisorUserFactory();
                // Set up view assigned advisee permissions on advisor's role--so that advisor cannot access this student
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { advisorRole });
                // reinitialize the service to effect the advisor as the current user
                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo, catalogRepo, planningConfigRepo,
                    referenceDataRepo, currentUserFactory, roleRepository, logger, baseConfigurationRepository);

                // Act
                var evaluation = (await programEvaluationService.EvaluateAsync("00004013", new List<string>() { "ENGL.BA", "MATH.BS.BLANKTAKEONE" }, null));
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task AdvisorWithViewAssignedAccess_NotAssigned_WithCatalog_ThrowsException()
            {
                // Arrange
                var studentId = "00004013";
                var student = studentRepo.Get(studentId);
                var planningStudent = new Domain.Student.Entities.PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
                planningStudent.Advisements = student.Advisements;
                planningStudentRepoMock.Setup(psr => psr.GetAsync(student.Id, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));

                currentUserFactory = new AdvisorUserFactory();
                // Set up view assigned advisee permissions on advisor's role--so that advisor cannot access this student
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { advisorRole });
                // reinitialize the service to effect the advisor as the current user
                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo, catalogRepo, planningConfigRepo,
                    referenceDataRepo, currentUserFactory, roleRepository, logger, baseConfigurationRepository);

                // Act
                var evaluation = (await programEvaluationService.EvaluateAsync("00004013", new List<string>() { "ENGL.BA", "MATH.BS.BLANKTAKEONE" }, "2012"));
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task StudentNotAbleToGetAnotherStudentsEvaluation_ThrowsException()
            {
                // Arrange
                string studentid = "0004002";
                var programCodes = new List<string>() { "MATH.BS.BLANKTAKEONE", "MATH.BS.TOBEOPTIMIZED2" };

                var student = studentRepo.Get("0000894");
                var planningStudent = new Domain.Student.Entities.PlanningStudent("0004002", student.LastName, student.DegreePlanId, student.ProgramIds);
                //planningStudent.Advisements = student.Advisements;
                planningStudentRepoMock.Setup(psr => psr.GetAsync(studentid, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));

                // reinitialize the service to effect the advisor as the current user
                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo, catalogRepo, planningConfigRepo,
                    referenceDataRepo, currentUserFactory, roleRepository, logger, baseConfigurationRepository);

                // Act
                var evaluation = await programEvaluationService.EvaluateAsync("0004002", programCodes, null);
            }

            [TestMethod]
            public void AdvisorWithViewAssignedAccess_AssignedToStudent_ReturnsEvaluation()
            {
                // Arrange
                // In TestStudentRepository, Student 0004012 has advisor 0000111 (Id for advisor from AdvisorUserFactory) set as current advisor
                string studentid = "00004012";
                var programCodes = new List<string>() { "MATH.BS.BLANKTAKEONE", "MATH.BS.TOBEOPTIMIZED2" };
                var student = studentRepo.Get(studentid);

                var planningStudent = new Domain.Student.Entities.PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
                planningStudent.Advisements = student.Advisements;
                planningStudentRepoMock.Setup(psr => psr.GetAsync(studentid, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));

                currentUserFactory = new AdvisorUserFactory();
                // View Assigned advisees permissions
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { advisorRole });
                // reinitialize the service to effect the advisor as the current user
                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo, catalogRepo, planningConfigRepo,
                    referenceDataRepo, currentUserFactory, roleRepository, logger, baseConfigurationRepository);

                // Act
                var evaluation = programEvaluationService.EvaluateAsync(studentid, programCodes, null).Result;

                // Assert
                Assert.IsTrue(evaluation is IEnumerable<ProgramEvaluation>);
                Assert.AreEqual(programCodes.ElementAt(0), evaluation.ElementAt(0).ProgramCode);
                Assert.IsTrue(evaluation.ElementAt(0).RequirementResults.Count() > 0);
                Assert.AreEqual(programCodes.ElementAt(1), evaluation.ElementAt(1).ProgramCode);
                Assert.IsTrue(evaluation.ElementAt(0).RequirementResults.Count() > 0);
            }

            [TestMethod]
            public void AdvisorWithViewAssignedAccess_AssignedToStudent_WithCatalog_ReturnsEvaluation()
            {
                // Arrange
                // In TestStudentRepository, Student 0004012 has advisor 0000111 (Id for advisor from AdvisorUserFactory) set as current advisor
                string studentid = "00004012";
                var programCodes = new List<string>() { "MATH.BS.BLANKTAKEONE", "MATH.BS.TOBEOPTIMIZED2" };
                var student = studentRepo.Get(studentid);

                var planningStudent = new Domain.Student.Entities.PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
                planningStudent.Advisements = student.Advisements;
                planningStudentRepoMock.Setup(psr => psr.GetAsync(studentid, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));

                currentUserFactory = new AdvisorUserFactory();
                // View Assigned advisees permissions
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { advisorRole });
                // reinitialize the service to effect the advisor as the current user
                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo, catalogRepo, planningConfigRepo,
                    referenceDataRepo, currentUserFactory, roleRepository, logger, baseConfigurationRepository);

                // Act
                var evaluation = programEvaluationService.EvaluateAsync(studentid, programCodes, "2012").Result;

                // Assert
                Assert.IsTrue(evaluation is IEnumerable<ProgramEvaluation>);
                Assert.AreEqual(programCodes.ElementAt(0), evaluation.ElementAt(0).ProgramCode);
                Assert.IsTrue(evaluation.ElementAt(0).RequirementResults.Count() > 0);
                Assert.AreEqual(programCodes.ElementAt(1), evaluation.ElementAt(1).ProgramCode);
                Assert.IsTrue(evaluation.ElementAt(0).RequirementResults.Count() > 0);
            }

            [TestMethod]
            public async Task Evaluate_Credits()
            {
                // Set up user as "AllAccessAnyAdvisee" advisor so that evaluation is permitted
                currentUserFactory = new AdvisorUserFactory();
                // Set up view any advisee permissions on advisor's role--so that advisor has access to this student
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { advisorRole });

                string studentid = "00004001"; // has acad cred ids 1,2,and 3 - three credits each, #3 is transfer
                var student = studentRepo.Get(studentid);
                var planningStudent = new Domain.Student.Entities.PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
                planningStudent.Advisements = student.Advisements;
                planningStudentRepoMock.Setup(psr => psr.GetAsync(studentid, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));

                string programid = "MATH.BS";

                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo, catalogRepo, planningConfigRepo,
                    referenceDataRepo, currentUserFactory, roleRepository, logger, baseConfigurationRepository);

                var progresult = (await programEvaluationService.EvaluateAsync(studentid, new List<string>() { programid }, null)).First();
                Assert.AreEqual(9, progresult.Credits);
                //dumper.Dump(progresult);
            }


            [TestMethod]
            public async Task Evaluate_InstitutionalCredits()
            {
                // Set up user as "AllAccessAnyAdvisee" advisor so that evaluation is permitted
                currentUserFactory = new AdvisorUserFactory();
                // Set up view any advisee permissions on advisor's role--so that advisor has access to this student
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { advisorRole });

                string studentid = "00004001"; // has acad cred ids 1,2,and 3 - three credits each, #3 is transfer
                var student = studentRepo.Get(studentid);
                var planningStudent = new Domain.Student.Entities.PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
                planningStudent.Advisements = student.Advisements;
                planningStudentRepoMock.Setup(psr => psr.GetAsync(studentid, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));

                string programid = "MATH.BS";

                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo, catalogRepo, planningConfigRepo,
                    referenceDataRepo, currentUserFactory, roleRepository, logger, baseConfigurationRepository);

                var progresult = (await programEvaluationService.EvaluateAsync(studentid, new List<string>() { programid }, null)).First();
                Assert.AreEqual(6, progresult.InstitutionalCredits);
            }

            [TestMethod]
            public void HawkingComparison()
            {
                // Set up user as "AllAccessAnyAdvisee" advisor so that evaluation is permitted
                currentUserFactory = new AdvisorUserFactory();
                // Set up view any advisee permissions on advisor's role--so that advisor has access to this student
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { advisorRole });

                string studentid = "1143352"; // Stephen Hawking
                var student = studentRepo.Get(studentid);
                var planningStudent = new Domain.Student.Entities.PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
                planningStudent.Advisements = student.Advisements;
                planningStudentRepoMock.Setup(psr => psr.GetAsync(studentid, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));

                string programid = "STSS.MATH.BS*2010";

                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo, catalogRepo, planningConfigRepo,
                    referenceDataRepo, currentUserFactory, roleRepository, logger, baseConfigurationRepository);

                var pe = programEvaluationService.EvaluateAsync(studentid, new List<string>() { programid }, null).Result.First();

                dumper.Dump(pe, "Verbosely, he spaketh");

                Assert.AreEqual(15m, pe.InstitutionalCredits);
                Assert.AreEqual(15m, pe.Credits);
                Assert.AreEqual(3, pe.RequirementResults.Count);
            }

            [TestMethod]
            public async Task Evaluate_WithNonCourseCreditsAndPlannedCourses()
            {
                // Set up user as "AllAccessAnyAdvisee" advisor so that evaluation is permitted
                currentUserFactory = new AdvisorUserFactory();
                // Set up view any advisee permissions on advisor's role--so that advisor has access to this student
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { advisorRole });

                string studentid = "00004010"; // has a plan, pluse has a few acad creds including one noncourse
                var student = studentRepo.Get(studentid);
                var planningStudent = new Domain.Student.Entities.PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
                planningStudent.Advisements = student.Advisements;
                planningStudentRepoMock.Setup(psr => psr.GetAsync(studentid, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));

                string programid = "MATH.BS";

                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo, catalogRepo, planningConfigRepo,
                    referenceDataRepo, currentUserFactory, roleRepository, logger, baseConfigurationRepository);

                var progresult = (await programEvaluationService.EvaluateAsync(studentid, new List<string>() { programid }, null)).First();

                dumper.Dump(progresult, "verbose");
            }

            [TestMethod]
            public async Task Evaluate_WithGroupLevelCourseRule()
            {
                // Set up user as "AllAccessAnyAdvisee" advisor so that evaluation is permitted
                currentUserFactory = new AdvisorUserFactory();
                // Set up view any advisee permissions on advisor's role--so that advisor has access to this student
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { advisorRole });

                string studentid = "00004002"; //planned MATH-100, MATH-200       ENGL-102, ENGL-101
                var student = studentRepo.Get(studentid);
                var planningStudent = new Domain.Student.Entities.PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
                planningStudent.Advisements = student.Advisements;
                planningStudentRepoMock.Setup(psr => psr.GetAsync(studentid, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));

                string programid = "MATH.BS.WITHGROUPRULE";

                // the test repo will return for this programid a program with a group that requires one course, from rule "MATH100"
                // if the rule evaluates correctly, only the planned course for math 100 will satisfy it.  If it fails to do so,
                // the english 100 will because credit > plannedcourse.

                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo, catalogRepo, planningConfigRepo,
                    referenceDataRepo, currentUserFactory, roleRepository, logger, baseConfigurationRepository);

                var progresult = (await programEvaluationService.EvaluateAsync(studentid, new List<string>() { programid }, null)).First();

                dumper.Dump(progresult, "verbose");

                var gr = progresult.RequirementResults.First().SubRequirementResults.First().GroupResults.First(gg => gg.Group.Id == "10056");
                var c = gr.GetPlannedApplied().First().GetCourse();
                Assert.AreEqual("MATH", c.SubjectCode);
                Assert.AreEqual("100", c.Number);
            }

            [TestMethod]
            public async Task Evaluate_WithSubrequirementLevelCourseRule()
            {
                // Set up user as "AllAccessAnyAdvisee" advisor so that evaluation is permitted
                currentUserFactory = new AdvisorUserFactory();
                // Set up view any advisee permissions on advisor's role--so that advisor has access to this student
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { advisorRole });

                string studentid = "00004002"; //planned MATH-100, MATH-200       ENGL-102, ENGL-101
                var student = studentRepo.Get(studentid);
                var planningStudent = new Domain.Student.Entities.PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
                planningStudent.Advisements = student.Advisements;
                planningStudentRepoMock.Setup(psr => psr.GetAsync(studentid, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));

                string programid = "MATH.BS.WITHSUBRULE";

                // the test repo will return for this programid a program with a group that requires one course, from rule "MATH100"
                // if the rule evaluates correctly, only the planned course for math 100 will satisfy it.  If it fails to do so,
                // the english 100 will because credit > plannedcourse.

                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo, catalogRepo, planningConfigRepo,
                    referenceDataRepo, currentUserFactory, roleRepository, logger, baseConfigurationRepository);

                var progresult = (await programEvaluationService.EvaluateAsync(studentid, new List<string>() { programid }, null)).First();

                dumper.Dump(progresult, "verbose");

                var gr = progresult.RequirementResults.First().SubRequirementResults.First().GroupResults.First(gg => gg.Group.Id == "10056");
                var c = gr.GetPlannedApplied().First().GetCourse();
                Assert.AreEqual("MATH", c.SubjectCode);
                Assert.AreEqual("100", c.Number);
            }

            [TestMethod]
            public async Task Evaluate_WithRequirementLevelCourseRule()
            {
                // Set up user as "AllAccessAnyAdvisee" advisor so that evaluation is permitted
                currentUserFactory = new AdvisorUserFactory();
                // Set up view any advisee permissions on advisor's role--so that advisor has access to this student
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { advisorRole });

                string studentid = "00004002"; //planned MATH-100, MATH-200       ENGL-102, ENGL-101
                var student = studentRepo.Get(studentid);
                var planningStudent = new Domain.Student.Entities.PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
                planningStudent.Advisements = student.Advisements;
                planningStudentRepoMock.Setup(psr => psr.GetAsync(studentid, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));

                string programid = "MATH.BS.WITHREQRULE";

                // the test repo will return for this programid a program with a group that requires one course, from rule "MATH100"
                // if the rule evaluates correctly, only the planned course for math 100 will satisfy it.  If it fails to do so,
                // the english 100 will because credit > plannedcourse.

                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo, catalogRepo, planningConfigRepo,
                    referenceDataRepo, currentUserFactory, roleRepository, logger, baseConfigurationRepository);

                var progresult = (await programEvaluationService.EvaluateAsync(studentid, new List<string>() { programid }, null)).First();

                dumper.Dump(progresult, "verbose");

                var gr = progresult.RequirementResults.First().SubRequirementResults.First().GroupResults.First(gg => gg.Group.Id == "10056");
                var c = gr.GetPlannedApplied().First().GetCourse();
                Assert.AreEqual("MATH", c.SubjectCode);
                Assert.AreEqual("100", c.Number);
            }

            [TestMethod]
            public async Task Evaluate_WithProgramLevelCourseRule()
            {
                // Set up user as "AllAccessAnyAdvisee" advisor so that evaluation is permitted
                currentUserFactory = new AdvisorUserFactory();
                // Set up view any advisee permissions on advisor's role--so that advisor has access to this student
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { advisorRole });

                string studentid = "00004002"; //planned MATH-100, MATH-200       ENGL-102, ENGL-101
                var student = studentRepo.Get(studentid);
                var planningStudent = new Domain.Student.Entities.PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
                planningStudent.Advisements = student.Advisements;
                planningStudentRepoMock.Setup(psr => psr.GetAsync(studentid, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));

                string programid = "MATH.BS.WITHPROGRULE";

                // the test repo will return for this programid a program with a group that requires one course, from rule "MATH100"
                // if the rule evaluates correctly, only the planned course for math 100 will satisfy it.  If it fails to do so,
                // the english 100 will because credit > plannedcourse.

                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo, catalogRepo, planningConfigRepo,
                    referenceDataRepo, currentUserFactory, roleRepository, logger, baseConfigurationRepository);

                var progresult = (await programEvaluationService.EvaluateAsync(studentid, new List<string>() { programid }, null)).First();

                dumper.Dump(progresult, "verbose");

                var gr = progresult.RequirementResults.First().SubRequirementResults.First().GroupResults.First(gg => gg.Group.Id == "10056");
                var c = gr.GetPlannedApplied().First().GetCourse();
                Assert.AreEqual("MATH", c.SubjectCode);
                Assert.AreEqual("100", c.Number);
            }

            [TestMethod]
            public async Task Evaluate_WithGroupLevelCreditRule()
            {
                // Set up user as "AllAccessAnyAdvisee" advisor so that evaluation is permitted
                currentUserFactory = new AdvisorUserFactory();
                // Set up view any advisee permissions on advisor's role--so that advisor has access to this student
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { advisorRole });

                string studentid = "00004001"; // none                     HIST-100,HIST-200,BIOL-100 (bio 100 has "A" status, which will fail rule NEWSTAT)
                var student = studentRepo.Get(studentid);
                var planningStudent = new Domain.Student.Entities.PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
                planningStudent.Advisements = student.Advisements;
                planningStudentRepoMock.Setup(psr => psr.GetAsync(studentid, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));

                string programid = "MATH.BS.WITHGROUPCREDRULE";

                // the test repo will return for this programid a program with a group that requires one course, from rule "NEWSTAT"
                // and from subjects "BIOL".  The BIOL-100 credit does not have a new status, so it should not satisfy the requirement

                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo, catalogRepo, planningConfigRepo,
                    referenceDataRepo, currentUserFactory, roleRepository, logger, baseConfigurationRepository);

                var progresult = (await programEvaluationService.EvaluateAsync(studentid, new List<string>() { programid }, null)).First();

                dumper.Dump(progresult, "verbose");

                var gr = progresult.RequirementResults.First().SubRequirementResults.First().GroupResults.First(gg => gg.Group.Id == "10056");
                Assert.IsFalse(gr.IsSatisfied());
                Assert.AreEqual(Domain.Student.Entities.Requirements.Result.RuleFailed, gr.Results.First(cr => cr.GetAcadCredId() == "3").Result);
            }

            [TestMethod]
            public async Task Evaluate_WithSubrequirementLevelCreditRule()
            {
                // Set up user as "AllAccessAnyAdvisee" advisor so that evaluation is permitted
                currentUserFactory = new AdvisorUserFactory();
                // Set up view any advisee permissions on advisor's role--so that advisor has access to this student
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { advisorRole });

                string studentid = "00004001"; // none                     HIST-100,HIST-200,BIOL-100 (bio 100 has "A" status, which will fail rule NEWSTAT)
                var student = studentRepo.Get(studentid);
                var planningStudent = new Domain.Student.Entities.PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
                planningStudent.Advisements = student.Advisements;
                planningStudentRepoMock.Setup(psr => psr.GetAsync(studentid, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));

                string programid = "MATH.BS.WITHSUBCREDRULE";

                // the test repo will return for this programid a program with a group that requires one course, from rule "NEWSTAT"
                // and from subjects "BIOL".  The BIOL-100 credit does not have a new status, so it should not satisfy the requirement

                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo, catalogRepo, planningConfigRepo,
                    referenceDataRepo, currentUserFactory, roleRepository, logger, baseConfigurationRepository);

                var progresult = (await programEvaluationService.EvaluateAsync(studentid, new List<string>() { programid }, null)).First();

                dumper.Dump(progresult, "verbose");

                var gr = progresult.RequirementResults.First().SubRequirementResults.First().GroupResults.First(gg => gg.Group.Id == "10056");
                Assert.IsFalse(gr.IsSatisfied());
                Assert.AreEqual(Domain.Student.Entities.Requirements.Result.RuleFailed, gr.Results.First(cr => cr.GetAcadCredId() == "3").Result);
            }

            [TestMethod]
            public async Task Evaluate_WithRequirementLevelCreditRule()
            {
                // Set up user as "AllAccessAnyAdvisee" advisor so that evaluation is permitted
                currentUserFactory = new AdvisorUserFactory();
                // Set up view any advisee permissions on advisor's role--so that advisor has access to this student
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { advisorRole });

                string studentid = "00004001"; // none                     HIST-100,HIST-200,BIOL-100 (bio 100 has "A" status, which will fail rule NEWSTAT)
                var student = studentRepo.Get(studentid);
                var planningStudent = new Domain.Student.Entities.PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
                planningStudent.Advisements = student.Advisements;
                planningStudentRepoMock.Setup(psr => psr.GetAsync(studentid, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));

                string programid = "MATH.BS.WITHREQCREDRULE";

                // the test repo will return for this programid a program with a group that requires one course, from rule "NEWSTAT"
                // and from subjects "BIOL".  The BIOL-100 credit does not have a new status, so it should not satisfy the requirement

                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo, catalogRepo, planningConfigRepo,
                    referenceDataRepo, currentUserFactory, roleRepository, logger, baseConfigurationRepository);

                var progresult = (await programEvaluationService.EvaluateAsync(studentid, new List<string>() { programid }, null)).First();

                dumper.Dump(progresult, "verbose");

                var gr = progresult.RequirementResults.First().SubRequirementResults.First().GroupResults.First(gg => gg.Group.Id == "10056");
                Assert.IsFalse(gr.IsSatisfied());
                Assert.AreEqual(Domain.Student.Entities.Requirements.Result.RuleFailed, gr.Results.First(cr => cr.GetAcadCredId() == "3").Result);
            }

            [TestMethod]
            public async Task Evaluate_WithProgramLevelCreditRule()
            {
                // Set up user as "AllAccessAnyAdvisee" advisor so that evaluation is permitted
                currentUserFactory = new AdvisorUserFactory();
                // Set up view any advisee permissions on advisor's role--so that advisor has access to this student
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { advisorRole });

                string studentid = "00004001"; // none                     HIST-100,HIST-200,BIOL-100 (bio 100 has "A" status, which will fail rule NEWSTAT)
                var student = studentRepo.Get(studentid);
                var planningStudent = new Domain.Student.Entities.PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
                planningStudent.Advisements = student.Advisements;
                planningStudentRepoMock.Setup(psr => psr.GetAsync(studentid, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));

                string programid = "MATH.BS.WITHPROGCREDRULE";

                // the test repo will return for this programid a program with a group that requires one course, from rule "NEWSTAT"
                // and from subjects "BIOL".  The BIOL-100 credit does not have a new status, so it should not satisfy the requirement

                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo, catalogRepo, planningConfigRepo,
                    referenceDataRepo, currentUserFactory, roleRepository, logger, baseConfigurationRepository);

                var progresult = (await programEvaluationService.EvaluateAsync(studentid, new List<string>() { programid }, null)).First();

                dumper.Dump(progresult, "verbose");

                var gr = progresult.RequirementResults.First().SubRequirementResults.First().GroupResults.First(gg => gg.Group.Id == "10056");
                Assert.IsFalse(gr.IsSatisfied());
                Assert.AreEqual(Domain.Student.Entities.Requirements.Result.RuleFailed, gr.Results.First(cr => cr.GetAcadCredId() == "3").Result);
            }

            [TestMethod]
            public async Task Evaluate_IgnoresWithdrawnCredits()
            {
                // Set up user as "AllAccessAnyAdvisee" advisor so that evaluation is permitted
                currentUserFactory = new AdvisorUserFactory();
                // Set up view any advisee permissions on advisor's role--so that advisor has access to this student
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { advisorRole });

                string studentid = "00004011"; // none                     MATH*4003 & MATH*151 - both withdrawn (one with a grade and one without)
                var student = studentRepo.Get(studentid);
                var planningStudent = new Domain.Student.Entities.PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
                planningStudent.Advisements = student.Advisements;
                planningStudentRepoMock.Setup(psr => psr.GetAsync(studentid, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));

                string programid = "MATH.BS.BLANKTAKEONE";

                // the test repo will return for this programid a program with a group that requires one course, no other requirement modifiers.
                // The student has only one acad cred, for a withdrawn course.  It should not apply.

                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo, catalogRepo, planningConfigRepo,
                    referenceDataRepo, currentUserFactory, roleRepository, logger, baseConfigurationRepository);

                var progresult = (await programEvaluationService.EvaluateAsync(studentid, new List<string>() { programid }, null)).First();

                dumper.Dump(progresult, "verbose");

                var gr = progresult.RequirementResults.First().SubRequirementResults.First().GroupResults.First(gg => gg.Group.Id == "10056");
                Assert.IsFalse(gr.IsSatisfied());
                Assert.AreEqual(0, gr.Results.Count);  // no results because the evaluator doesn't even pass the withdrawn credit to the group
            }

            [TestMethod]
            public async Task Evaluate_IgnoresCreditsInWrongTranscriptGrouping()
            {
                // Set up user as "AllAccessAnyAdvisee" advisor so that evaluation is permitted
                currentUserFactory = new AdvisorUserFactory();
                // Set up view any advisee permissions on advisor's role--so that advisor has access to this student
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { advisorRole });

                string studentid = "00004012"; // none                     MATH-502 - graduate course
                var student = studentRepo.Get(studentid);
                var planningStudent = new Domain.Student.Entities.PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
                planningStudent.Advisements = student.Advisements;
                planningStudentRepoMock.Setup(psr => psr.GetAsync(studentid, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));

                string programid = "MATH.BS.BLANKTAKEONE";

                // the test repo will return for this programid a program with a group that requires one course, no other requirement modifiers.
                // The student has only one acad cred, for a withdrawn course.  It should not apply.

                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo, catalogRepo, planningConfigRepo,
                    referenceDataRepo, currentUserFactory, roleRepository, logger, baseConfigurationRepository);

                var progresult = (await programEvaluationService.EvaluateAsync(studentid, new List<string>() { programid }, null)).First();

                dumper.Dump(progresult, "verbose");

                var gr = progresult.RequirementResults.First().SubRequirementResults.First().GroupResults.First(gg => gg.Group.Id == "10056");
                Assert.IsFalse(gr.IsSatisfied());
                Assert.AreEqual(0, gr.Results.Count);   // no results because the Evaluation service applies the program's credit filter 
                // and doesn't pass the credit to the evaluator
                Assert.AreEqual(0, progresult.AllCredit.Count);
            }


            [TestMethod]
            public void SatisfiedSubrequirementTriggersOptimizerForPartialGroups()
            {
                // Set up user as "AllAccessAnyAdvisee" advisor so that evaluation is permitted
                currentUserFactory = new AdvisorUserFactory();
                // Set up view any advisee permissions on advisor's role--so that advisor has access to this student
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { advisorRole });

                string studentid = "00004013";  //no plan, credits ENGL-102
                var student = studentRepo.Get(studentid);
                var planningStudent = new Domain.Student.Entities.PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
                planningStudent.Advisements = student.Advisements;
                planningStudentRepoMock.Setup(psr => psr.GetAsync(studentid, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));

                string programid = "MATH.BS.TOBEOPTIMIZED";


                // Test3 is a "Take all" which should evaluate before Test32.  The Evaluator should see that 
                // test3 is NOT satisfied and that therefore will evaluate Test32 should eval and be satisfied.  
                // The optimizer should see that Test3 is partially satisfied and hanging out to dry in a satisfied
                // subrequriement and rerun the eval without evaluating Test3.

                // It would have been extra cool to 

                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo, catalogRepo, planningConfigRepo,
                    referenceDataRepo, currentUserFactory, roleRepository, logger, baseConfigurationRepository);

                var eval = programEvaluationService.EvaluateAsync(studentid, new List<string>() { programid }, null).Result.First();

                dumper.Dump(eval, "verbose");

                // get the group and Subrequirement results
                GroupResult gr1 = eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First(gr => gr.Group.Code == "Test3");
                GroupResult gr2 = eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First(gr => gr.Group.Code == "Test32");
                SubrequirementResult sr1 = eval.RequirementResults.First().SubRequirementResults.First();

                // check group
                Assert.IsTrue(gr1.CompletionStatus == CompletionStatus.NotStarted);
                Assert.IsTrue(gr2.CompletionStatus == CompletionStatus.Completed);


                // Check Acad Cred results
                Assert.IsTrue(gr1.Results.Where(x => x.Result == Result.Applied).Count() == 0);
                Assert.IsTrue(gr2.Results.Where(x => x.Result == Result.Applied).Count() == 1);

                // Check Subrequirement
                Assert.IsTrue(eval.RequirementResults.First().SubRequirementResults.First().CompletionStatus == CompletionStatus.Completed);
            }

            [TestMethod]
            public void SatisfiedRequirementTriggersOptimizerForPartialSubrequirements()
            {
                // Set up user as "AllAccessAnyAdvisee" advisor so that evaluation is permitted
                currentUserFactory = new AdvisorUserFactory();
                // Set up view any advisee permissions on advisor's role--so that advisor has access to this student
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { advisorRole });

                string studentid = "00004013";  //no plan, credits ENGL-102
                var student = studentRepo.Get(studentid);
                var planningStudent = new Domain.Student.Entities.PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
                planningStudent.Advisements = student.Advisements;
                planningStudentRepoMock.Setup(psr => psr.GetAsync(studentid, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));

                string programid = "MATH.BS.TOBEOPTIMIZED2";


                // Test3 is a "Take all" which should evaluate before Test32.  The Evaluator should see that 
                // test3 is NOT satisfied and that therefore will evaluate Test32 should eval and be satisfied.  
                // The optimizer should see that Test3 is partially satisfied and hanging out to dry in a satisfied
                // subrequriement and rerun the eval without evaluating Test3.

                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo, catalogRepo, planningConfigRepo,
                    referenceDataRepo, currentUserFactory, roleRepository, logger, baseConfigurationRepository);

                var eval = programEvaluationService.EvaluateAsync(studentid, new List<string>() { programid }, null).Result.First();

                dumper.Dump(eval, "verbose");

                // get the group and Subrequirement results
                GroupResult gr1 = eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First(gr => gr.Group.Code == "Test3");
                GroupResult gr2 = eval.RequirementResults.First().SubRequirementResults.Last().GroupResults.First(gr => gr.Group.Code == "Test32");
                SubrequirementResult sr1 = eval.RequirementResults.First().SubRequirementResults.First();

                // check group
                Assert.IsTrue(gr1.CompletionStatus == CompletionStatus.NotStarted);
                Assert.IsTrue(gr2.CompletionStatus == CompletionStatus.Completed);


                // Check Acad Cred results
                Assert.IsTrue(gr1.Results.Where(x => x.Result == Result.Applied).Count() == 0);
                Assert.IsTrue(gr2.Results.Where(x => x.Result == Result.Applied).Count() == 1);

                // Check Subrequirement
                Assert.IsTrue(eval.RequirementResults.First().SubRequirementResults.Last().CompletionStatus == CompletionStatus.Completed);

                // Check Requirement
                Assert.IsTrue(eval.RequirementResults.First().CompletionStatus == CompletionStatus.Completed);
            }

            [TestMethod]
            public async Task Evaluate_AppliesCreditsByTypeThenByDateThenByReverseAcademicCreditId()
            {
                // Set up user as "AllAccessAnyAdvisee" advisor so that evaluation is permitted
                currentUserFactory = new AdvisorUserFactory();
                // Set up view any advisee permissions on advisor's role--so that advisor has access to this student
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { advisorRole });

                string studentid = "00004014";
                var student = studentRepo.Get(studentid);
                var planningStudent = new Domain.Student.Entities.PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
                planningStudent.Advisements = student.Advisements;
                planningStudentRepoMock.Setup(psr => psr.GetAsync(studentid, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));
                // Student has academic credits in the following sequence on student record with the following id and course name, type (all graded, but some institutional and some transfer) 
                // and start date (same as term start) and expected applied position
                // 13 MUSC-100 TR 2010SP expected sort pos: 5
                // 16 MATH-362 IN 2010SP expected sort pos: 3
                // 14 MUSC-207 TR 2010SP expected sort pos: 4
                //  8 MATH-100 IN 2009FA expected sort pos: 1
                // 63 MUSC-209 IN 2010SP expected sort pos: 2
                // 11 MATH-150 IN 2009FA expected sort pos: 0
                string programid = "SIMPLE";
                // The SIMPLE program will allow up to 10 courses of any course level to be applied.

                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo, catalogRepo, planningConfigRepo,
                    referenceDataRepo, currentUserFactory, roleRepository, logger, baseConfigurationRepository);

                var progresult = (await programEvaluationService.EvaluateAsync(studentid, new List<string>() { programid }, null)).First();

                var gr = progresult.RequirementResults.First().SubRequirementResults.First().GroupResults.First();
                Assert.AreEqual(6, gr.Results.Count());
                Assert.AreEqual("11", gr.Results.ElementAt(0).GetAcadCredId()); // MATH-150
                Assert.AreEqual("8", gr.Results.ElementAt(1).GetAcadCredId());  // MATH-100
                Assert.AreEqual("63", gr.Results.ElementAt(2).GetAcadCredId()); // MUSC-209
                Assert.AreEqual("16", gr.Results.ElementAt(3).GetAcadCredId()); // MATH-362
                Assert.AreEqual("14", gr.Results.ElementAt(4).GetAcadCredId()); // MUSC-207
                Assert.AreEqual("13", gr.Results.ElementAt(5).GetAcadCredId()); // MUSC-100
            }

            [TestMethod]
            public async Task Evaluate_ReturnsEvaluationIfStudentHasNoAcademicCredits()
            {
                // Set up user as "AllAccessAnyAdvisee" advisor so that evaluation is permitted
                currentUserFactory = new AdvisorUserFactory();
                // Set up view any advisee permissions on advisor's role--so that advisor has access to this student
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { advisorRole });

                string studentid = "00004014";
                var student = studentRepo.Get(studentid);
                var planningStudent = new Domain.Student.Entities.PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
                planningStudent.Advisements = student.Advisements;
                planningStudentRepoMock.Setup(psr => psr.GetAsync(studentid, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));
                // set up academic credit repo response with no academic credits.
                academicCreditRepoMock.Setup(repo => repo.GetAcademicCreditByStudentIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync((new Dictionary<string, List<AcademicCredit>>()));
                string programid = "SIMPLE";
                // The SIMPLE program will allow up to 10 courses of any course level to be applied.

                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo, catalogRepo, planningConfigRepo,
                    referenceDataRepo, currentUserFactory, roleRepository, logger, baseConfigurationRepository);

                var progresult = (await programEvaluationService.EvaluateAsync(studentid, new List<string>() { programid }, null)).First();
                Assert.IsNotNull(progresult);
            }

            [TestMethod]
            public async Task Evaluate_ReturnsEvaluationIfStudentHasNoAcademicCredits_WithCatalog()
            {
                // Set up user as "AllAccessAnyAdvisee" advisor so that evaluation is permitted
                currentUserFactory = new AdvisorUserFactory();
                // Set up view any advisee permissions on advisor's role--so that advisor has access to this student
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { advisorRole });

                string studentid = "00004014";
                var student = studentRepo.Get(studentid);
                var planningStudent = new Domain.Student.Entities.PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
                planningStudent.Advisements = student.Advisements;
                planningStudentRepoMock.Setup(psr => psr.GetAsync(studentid, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));
                // set up academic credit repo response with no academic credits.
                academicCreditRepoMock.Setup(repo => repo.GetAcademicCreditByStudentIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync((new Dictionary<string, List<AcademicCredit>>()));
                string programid = "SIMPLE";
                // The SIMPLE program will allow up to 10 courses of any course level to be applied.

                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo, catalogRepo, planningConfigRepo,
                    referenceDataRepo, currentUserFactory, roleRepository, logger, baseConfigurationRepository);

                var progresult = (await programEvaluationService.EvaluateAsync(studentid, new List<string>() { programid }, "2012")).First();
                Assert.IsNotNull(progresult);
            }

            [TestMethod]
            public async Task Evaluate_ExcludesWithdrawnCreditsWithNoGrade()
            {
                // Set up user as "AllAccessAnyAdvisee" advisor so that evaluation is permitted
                currentUserFactory = new AdvisorUserFactory();
                // Set up view any advisee permissions on advisor's role--so that advisor has access to this student
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { advisorRole });

                string studentid = "00004011"; // none                     2 withdrawn classes MATH*4003 id 56 with grade, MATH-151 id 74 without grade
                var student = studentRepo.Get(studentid);
                var planningStudent = new Domain.Student.Entities.PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
                planningStudent.Advisements = student.Advisements;
                planningStudentRepoMock.Setup(psr => psr.GetAsync(studentid, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));

                string programid = "SIMPLE";

                // the test repo will return for this programid a program with a group that is take any 10 courses.
                // The student has 2 acad cred, one for a withdrawn course with a grade and one for a course with no grade.  Only 1 should be used.

                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo, catalogRepo, planningConfigRepo,
                    referenceDataRepo, currentUserFactory, roleRepository, logger, baseConfigurationRepository);

                var progresult = (await programEvaluationService.EvaluateAsync(studentid, new List<string>() { programid }, null)).First();

                dumper.Dump(progresult, "verbose");

                var gr = progresult.RequirementResults.First().SubRequirementResults.First().GroupResults.First(gg => gg.Group.Id == "SIMPLE.ANY");
                Assert.IsFalse(gr.IsSatisfied());       // Neither of the withdrawns are applied.
                Assert.AreEqual(0, gr.Results.Count);
                Assert.AreEqual(1, progresult.OtherAcademicCredits.Count);    // Only the one withdrawn class is listed in the other count and it is 56.
                var otherCreditId = progresult.OtherAcademicCredits.Where(c => c == "56").FirstOrDefault();
                Assert.AreEqual("56", otherCreditId);
            }
        }

        [TestClass]
        public class ProgramEvaluationServiceTests_GetEvaluationNotices : CurrentUserSetup
        {
            private IAdapterRegistry adapterRegistry;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IProgramRequirementsRepository programRequirementsRepo;
            private IStudentRepository studentRepo;
            private Mock<IPlanningStudentRepository> planningStudentRepoMock;
            private IPlanningStudentRepository planningStudentRepo;
            private Mock<IApplicantRepository> applicantRepoMock;
            private IApplicantRepository applicantRepo;
            private IStudentProgramRepository studentProgramRepo;
            private Mock<IStudentProgramRepository> studentProgramRepoMock;
            private IAcademicCreditRepository academicCreditRepo;
            private IStudentDegreePlanRepository studentDegreePlanRepo;
            private ICourseRepository courseRepo;
            private IRequirementRepository requirementRepo;
            private ITermRepository termRepo;
            private IRuleRepository ruleRepo;
            private IProgramRepository programRepo;
            private ICatalogRepository catalogRepo;
            private IPlanningConfigurationRepository planningConfigRepo;
            private ProgramEvaluationService programEvaluationService;
            private Dumper dumper;
            private ILogger logger;
            private ICurrentUserFactory currentUserFactory;
            private IRoleRepository roleRepository;
            private Mock<IRoleRepository> roleRepoMock;
            private IReferenceDataRepository referenceDataRepo;
            private string studentId;
            private string programCode;
            private IEnumerable<Domain.Student.Entities.EvaluationNotice> evaluationNoticeEntities;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;


            [TestInitialize]
            public void Initialize()
            {
                programRequirementsRepo = new TestProgramRequirementsRepository();
                planningStudentRepoMock = new Mock<IPlanningStudentRepository>();
                planningStudentRepo= planningStudentRepoMock.Object;
                applicantRepoMock = new Mock<IApplicantRepository>();
                applicantRepo = applicantRepoMock.Object;
                studentProgramRepoMock = new Mock<IStudentProgramRepository>();
                studentProgramRepo = studentProgramRepoMock.Object;
                studentRepo = new TestStudentRepository();
                academicCreditRepo = new TestAcademicCreditRepository();
                studentDegreePlanRepo = new TestStudentDegreePlanRepository();
                courseRepo = new TestCourseRepository();
                requirementRepo = new TestRequirementRepository();
                ruleRepo = new TestRuleRepository2();
                programRepo = new TestProgramRepository();
                catalogRepo = new TestCatalogRepository();
                planningConfigRepo = null;
                termRepo = new TestTermRepository();
                dumper = new Dumper();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                logger = new Mock<ILogger>().Object;
                currentUserFactory = new StudentUserFactory();
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepository = roleRepoMock.Object;
                referenceDataRepo = new Mock<IReferenceDataRepository>().Object;
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                studentId = "0000894";
                programCode = "ENGL.BA";

                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo, catalogRepo, planningConfigRepo,
                    referenceDataRepo, currentUserFactory, roleRepository, logger, baseConfigurationRepository);

                // Mock adapter
                var evaluationNoticeAdapter = new AutoMapperAdapter<EvaluationNotice, Dtos.Student.EvaluationNotice>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<EvaluationNotice, Dtos.Student.EvaluationNotice>()).Returns(evaluationNoticeAdapter);

                // Mock object returned from call to GetStudentProgramEvaluationNotices
                evaluationNoticeEntities = new List<EvaluationNotice>()
                {
                    new EvaluationNotice(studentId, programCode, new List<string> {"line1","line2"}, EvaluationNoticeType.Program),
                    new EvaluationNotice(studentId, programCode, new List<string> {"line3","line4"}, EvaluationNoticeType.StudentProgram)
                };
                studentProgramRepoMock.Setup(repo => repo.GetStudentProgramEvaluationNoticesAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(evaluationNoticeEntities));
            }

            [TestMethod]
            public async Task Returns_EvaluationNoticeDtos()
            {
                // Act
                var notices = await programEvaluationService.GetEvaluationNoticesAsync("0000894", "ENGL.BA");

                // Assert
                Assert.IsTrue(notices is List<Dtos.Student.EvaluationNotice>);
                Assert.AreEqual(2, notices.Count());
                for (int i = 0; i < evaluationNoticeEntities.Count(); i++)
                {
                    Assert.AreEqual(evaluationNoticeEntities.ElementAt(i).ProgramCode, notices.ElementAt(i).ProgramCode);
                    Assert.AreEqual(evaluationNoticeEntities.ElementAt(i).StudentId, notices.ElementAt(i).StudentId);
                    Assert.AreEqual(evaluationNoticeEntities.ElementAt(i).Type.ToString(), notices.ElementAt(i).Type.ToString());
                    for (int j = 0; j < evaluationNoticeEntities.ElementAt(i).Text.Count(); j++)
                    {
                        Assert.AreEqual(evaluationNoticeEntities.ElementAt(i).Text.ElementAt(j), notices.ElementAt(i).Text.ElementAt(j));
                    }
                }
            }

            [TestMethod]
            public async Task NullNoticesReturned_ReturnsEmptyDto()
            {
                IEnumerable<EvaluationNotice> nullNotices = null;
                studentProgramRepoMock.Setup(spr => spr.GetStudentProgramEvaluationNoticesAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(nullNotices));
                var notices = await programEvaluationService.GetEvaluationNoticesAsync("0000894", "ENGL.BA");
            }

            [TestMethod]
            public async Task AdvisorWithAccess_ReturnsEvaluationNoticeDtos()
            {
                // Arrange
                currentUserFactory = new AdvisorUserFactory();
                // Set up view any advisee permissions on advisor's role--so that advisor has access to this student
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { advisorRole });
                // reinitialize the service to effect the advisor as the current user
                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo, catalogRepo, planningConfigRepo,
                    referenceDataRepo, currentUserFactory, roleRepository, logger, baseConfigurationRepository);

                // Act
                var notices = await programEvaluationService.GetEvaluationNoticesAsync("00004013", "ENGL.BA");

                // Assert
                Assert.IsTrue(notices is List<Dtos.Student.EvaluationNotice>);
                Assert.AreEqual(2, notices.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task AdvisorWithoutAccess_ThrowsException()
            {
                // Arrange
                currentUserFactory = new AdvisorUserFactory();
                // Set up view assigned advisee permissions on advisor's role--so that advisor cannot access this student
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { advisorRole });
                // reinitialize the service to effect the advisor as the current user
                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo, catalogRepo, planningConfigRepo,
                    referenceDataRepo, currentUserFactory, roleRepository, logger, baseConfigurationRepository);

                // Act
                var notices = await programEvaluationService.GetEvaluationNoticesAsync("00004013", "ENGL.BA");
            }
        }

        [TestClass]
        public class ProgramEvaluationServiceTests_Evaluate_SortOrder : CurrentUserSetup
        {
            private IAdapterRegistry adapterRegistry;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IProgramRequirementsRepository programRequirementsRepo;
            private IStudentRepository studentRepo;
            private Mock<IPlanningStudentRepository> planningStudentRepoMock;
            private IPlanningStudentRepository planningStudentRepo;
            private Mock<IApplicantRepository> applicantRepoMock;
            private IApplicantRepository applicantRepo;
            private Mock<IStudentProgramRepository> studentProgramRepoMock;
            private IStudentProgramRepository studentProgramRepo;
            private Mock<IAcademicCreditRepository> academicCreditRepoMock;
            private IAcademicCreditRepository academicCreditRepo;
            private IAcademicCreditRepository academicCreditRepoInstance = new TestAcademicCreditRepository();
            private Mock<IStudentDegreePlanRepository> studentDegreePlanRepoMock;
            private IStudentDegreePlanRepository studentDegreePlanRepo;
            private ICourseRepository courseRepo;
            private IRequirementRepository requirementRepo;
            private Mock<IRequirementRepository> requirementRepoMock;
            private IRequirementRepository requirementRepoInstance = new TestRequirementRepository();
            private ITermRepository termRepo;
            private IRuleRepository ruleRepo;
            private IProgramRepository programRepo;
            private ICatalogRepository catalogRepo;
            private IPlanningConfigurationRepository planningConfigRepo;
            private IReferenceDataRepository referenceDataRepo;
            private IRoleRepository roleRepository;
            private Mock<IRoleRepository> roleRepoMock;
            private ProgramEvaluationService programEvaluationService;
            private Dumper dumper;
            private ILogger logger;
            private ICurrentUserFactory currentUserFactory;
            private Mock<IConfigurationRepository> configRepoMock;
            private IConfigurationRepository configRepo;

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

            [TestInitialize]
            public void Initialize()
            {
                programRequirementsRepo = new TestProgramRequirementsRepository();
                studentRepo = new TestStudentRepository();
                planningStudentRepoMock = new Mock<IPlanningStudentRepository>();
                planningStudentRepo = planningStudentRepoMock.Object;
                applicantRepoMock = new Mock<IApplicantRepository>();
                applicantRepo = applicantRepoMock.Object;
                studentProgramRepo = new TestStudentProgramRepository();

                academicCreditRepoMock = new Mock<IAcademicCreditRepository>();
                academicCreditRepo = academicCreditRepoMock.Object;
                referenceDataRepo = new Mock<IReferenceDataRepository>().Object;
                academicCreditRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string[]>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns((string[] s, bool b1, bool b2, bool b3) => Task.FromResult(academicCreditRepoInstance.GetAsync(s, b1, b2, b3).Result));
                academicCreditRepoMock.Setup(repo => repo.GetAcademicCreditByStudentIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns((IEnumerable<string> s, bool b1, bool b2, bool b3) => Task.FromResult(GetCredits(s)));

                studentDegreePlanRepoMock = new Mock<IStudentDegreePlanRepository>();
                studentProgramRepoMock = new Mock<IStudentProgramRepository>();

                studentDegreePlanRepo = new TestStudentDegreePlanRepository();
                courseRepo = new TestCourseRepository();
                //requirementRepo = new TestRequirementRepository();
                requirementRepoMock = new Mock<IRequirementRepository>();
                requirementRepo = requirementRepoMock.Object;
                ruleRepo = new TestRuleRepository2();
                programRepo = new TestProgramRepository();
                catalogRepo = new TestCatalogRepository();
                planningConfigRepo = new TestPlanningConfigurationRepository();
                termRepo = new TestTermRepository();
                dumper = new Dumper();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                logger = new Mock<ILogger>().Object;
                currentUserFactory = new StudentUserFactory();
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepository = roleRepoMock.Object;
                configRepoMock = new Mock<IConfigurationRepository>();
                configRepo = configRepoMock.Object;
            }

            [TestMethod]
            public async Task Evaluate_AppliesCreditsByDefaultSortSpec()
            {
                // Set up user as "AllAccessAnyAdvisee" advisor so that evaluation is permitted
                currentUserFactory = new AdvisorUserFactory();
                // Set up view any advisee permissions on advisor's role--so that advisor has access to this student
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Ellucian.Colleague.Domain.Entities.Role>() { advisorRole });
                // Mock the parameters where the modified sort is true.
                requirementRepoMock.Setup(repo => repo.GetDegreeAuditParametersAsync()).Returns(Task.FromResult(new DegreeAuditParameters(ExtraCourses.Apply,  false,true, true)));
                // Build a filterCreditDictionary
                Dictionary<string, List<AcademicCredit>> filteredCreditsDict = new Dictionary<string, List<AcademicCredit>>();
                IEnumerable<string> students = new List<string>() { "00004014" };
                List<AcademicCredit> sortedCredits = GetCredits(students)["00004014"].OrderBy(ac => ac.CourseName).ToList();
                filteredCreditsDict.Add("DEFAULT", sortedCredits);
                academicCreditRepoMock.Setup(repo => repo.GetSortedAcademicCreditsBySortSpecificationIdAsync(It.IsAny<IEnumerable<AcademicCredit>>(), It.IsAny<List<string>>())).Returns(Task.FromResult(filteredCreditsDict));
                string studentid = "00004014";
                var student = studentRepo.Get(studentid);
                var planningStudent = new Domain.Student.Entities.PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
                planningStudent.Advisements = student.Advisements;
                planningStudentRepoMock.Setup(psr => psr.GetAsync(studentid, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));



                // Student has academic credits in the following sequence on student record with the following id and course name, type (all graded, but some institutional and some transfer) 
                // and start date (same as term start) and expected applied position
                // 13 MUSC-100 TR 2010SP expected sort pos: 5
                // 16 MATH-362 IN 2010SP expected sort pos: 3
                // 14 MUSC-207 TR 2010SP expected sort pos: 4
                //  8 MATH-100 IN 2009FA expected sort pos: 1
                // 63 MUSC-209 IN 2010SP expected sort pos: 2
                // 11 MATH-150 IN 2009FA expected sort pos: 0
                string programid = "SIMPLE";
                // The SIMPLE program will allow up to 10 courses of any course level to be applied.

                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo, catalogRepo, planningConfigRepo,
                    referenceDataRepo, currentUserFactory, roleRepository, logger, configRepo);

                var progresult = (await programEvaluationService.EvaluateAsync(studentid, new List<string>() { programid }, null)).First();

                var gr = progresult.RequirementResults.First().SubRequirementResults.First().GroupResults.First();
                Assert.AreEqual(6, gr.Results.Count());
                // Item will be sorted in title order because that is what was returned by the sort spec DEFAULT.
                Assert.AreEqual("8", gr.Results.ElementAt(0).GetAcadCredId());  // MATH-100
                Assert.AreEqual("11", gr.Results.ElementAt(1).GetAcadCredId()); // MATH-150
                Assert.AreEqual("16", gr.Results.ElementAt(2).GetAcadCredId()); // MATH-362
                Assert.AreEqual("13", gr.Results.ElementAt(3).GetAcadCredId()); // MUSC-100
                Assert.AreEqual("14", gr.Results.ElementAt(4).GetAcadCredId()); // MUSC-207
                Assert.AreEqual("63", gr.Results.ElementAt(5).GetAcadCredId()); // MUSC-209
            }
        }

        [TestClass]
        public class ProgramEvaluationServiceTests_Evaluate_ReplaceAndReplacementStatuses : CurrentUserSetup
        {
            private IAdapterRegistry adapterRegistry;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IProgramRequirementsRepository programRequirementsRepo;
            private IStudentRepository studentRepo;
            private Mock<IPlanningStudentRepository> planningStudentRepoMock;
            private IPlanningStudentRepository planningStudentRepo;
            private Mock<IApplicantRepository> applicantRepoMock;
            private IApplicantRepository applicantRepo;
            private Mock<IStudentProgramRepository> studentProgramRepoMock;
            private IStudentProgramRepository studentProgramRepo;
            private Mock<IAcademicCreditRepository> academicCreditRepoMock;
            private IAcademicCreditRepository academicCreditRepo;
            private IAcademicCreditRepository academicCreditRepoInstance = new TestAcademicCreditRepository();
            private Mock<IStudentDegreePlanRepository> studentDegreePlanRepoMock;
            private IStudentDegreePlanRepository studentDegreePlanRepo;
            private ICourseRepository courseRepo;
            private IRequirementRepository requirementRepo;
            private Mock<IRequirementRepository> requirementRepoMock;
            private IRequirementRepository requirementRepoInstance = new TestRequirementRepository();
            private ITermRepository termRepo;
            private IRuleRepository ruleRepo;
            private IProgramRepository programRepo;
            private ICatalogRepository catalogRepo;
            private IPlanningConfigurationRepository planningConfigRepo;
            private IReferenceDataRepository referenceDataRepo;
            private IRoleRepository roleRepository;
            private Mock<IRoleRepository> roleRepoMock;
            private ProgramEvaluationService programEvaluationService;
            private Dumper dumper;
            private ILogger logger;
            private ICurrentUserFactory currentUserFactory;
            private Mock<IConfigurationRepository> configRepoMock;
            private IConfigurationRepository configRepo;
            private Dictionary<string, List<AcademicCredit>> creditDictionary = new Dictionary<string, List<AcademicCredit>>();

            private Dictionary<string, List<AcademicCredit>> GetCredits(IEnumerable<string> studentIds, bool includeDrops = false)
            {
                creditDictionary = new Dictionary<string, List<AcademicCredit>>();
                foreach (var id in studentIds)
                {
                    var student = studentRepo.Get(id);
                    var acadCreditIds = student.AcademicCreditIds;

                    var credits = academicCreditRepoInstance.GetAsync(acadCreditIds, includeDrops: includeDrops).Result.ToList();

                    creditDictionary.Add(id, credits);
                }

                return creditDictionary;
            }

            [TestInitialize]
            public void Initialize()
            {
                programRequirementsRepo = new TestProgramRequirementsRepository();
                studentRepo = new TestStudentRepository();
                planningStudentRepoMock = new Mock<IPlanningStudentRepository>();
                planningStudentRepo = planningStudentRepoMock.Object;
                applicantRepoMock = new Mock<IApplicantRepository>();
                applicantRepo = applicantRepoMock.Object;
                studentProgramRepo = new TestStudentProgramRepository();


                academicCreditRepoMock = new Mock<IAcademicCreditRepository>();
                academicCreditRepo = academicCreditRepoMock.Object;
                referenceDataRepo = new Mock<IReferenceDataRepository>().Object;

                studentDegreePlanRepoMock = new Mock<IStudentDegreePlanRepository>();
                studentProgramRepoMock = new Mock<IStudentProgramRepository>();

                studentDegreePlanRepo = new TestStudentDegreePlanRepository();
                courseRepo = new TestCourseRepository();
                requirementRepo = new TestRequirementRepository();
                requirementRepoMock = new Mock<IRequirementRepository>();
                requirementRepo = requirementRepoMock.Object;
                ruleRepo = new TestRuleRepository2();
                programRepo = new TestProgramRepository();
                catalogRepo = new TestCatalogRepository();
                planningConfigRepo = new TestPlanningConfigurationRepository();
                termRepo = new TestTermRepository();
                dumper = new Dumper();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                logger = new Mock<ILogger>().Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepository = roleRepoMock.Object;
                configRepoMock = new Mock<IConfigurationRepository>();
                configRepo = configRepoMock.Object;

                // Set up user as "AllAccessAnyAdvisee" advisor so that evaluation is permitted
                currentUserFactory = new AdvisorUserFactory();
                // Set up view any advisee permissions on advisor's role--so that advisor has access to this student
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { advisorRole });
            }

            [TestMethod]
            public async Task Evaluate_ReplaceStatuses_AllCoursesAreInProgress()
            {
                string studentid = "0016285";
                GetCredits(new List<string>() { studentid });

                //for this particular student all these acadcredits are inprogress
                //these student acadCredits are retaken but will replace and count only one because this course have retakeforcredits flag as N
                //110 is 2009/sp MATH-3300BB credit course at offset 0
                //111 is 2010/sp math-300bb credit course at offset 1
                //112 is 2017/sp math-300bb credit course at offfset 2
                creditDictionary["0016285"][0].RepeatAcademicCreditIds = new List<string>() { "110", "111", "112" };
                creditDictionary["0016285"][1].RepeatAcademicCreditIds = new List<string>() { "110", "111", "112" };
                creditDictionary["0016285"][2].RepeatAcademicCreditIds = new List<string>() { "110", "111", "112" };
                creditDictionary["0016285"][0].CanBeReplaced = true;
                creditDictionary["0016285"][1].CanBeReplaced = true;
                creditDictionary["0016285"][2].CanBeReplaced = true;
                creditDictionary["0016285"][0].EndDate = null;
                creditDictionary["0016285"][1].EndDate = null;
                creditDictionary["0016285"][2].EndDate = null;
                creditDictionary["0016285"][2].StartDate = new DateTime(2017, 03, 01);
                //Since they are inprogress courses adjustec credits will be null
                creditDictionary["0016285"][0].AdjustedCredit = null;
                creditDictionary["0016285"][1].AdjustedCredit = null;
                creditDictionary["0016285"][2].AdjustedCredit = null;

                var student = studentRepo.Get(studentid);
                var planningStudent = new Domain.Student.Entities.PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
                planningStudent.Advisements = student.Advisements;
                planningStudentRepoMock.Setup(psr => psr.GetAsync(studentid, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));

                academicCreditRepoMock.Setup(repo => repo.GetAcademicCreditByStudentIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns((IEnumerable<string> s, bool b1, bool b2, bool b3) => Task.FromResult(creditDictionary));
                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo, catalogRepo, planningConfigRepo,
                    referenceDataRepo, currentUserFactory, roleRepository, logger, configRepo);

                var programResult = (await programEvaluationService.EvaluateAsync(studentid, new List<string>() { "REPEAT.BB" }, null)).First();
                Assert.IsNotNull(programResult);
                //since these course are replacing each other and a flag on course math-300bb says do not retake for credits
                //total inprogress credits will be 3
                //110 and 110 will not be applied to any group but will fall in other group category

                Assert.AreEqual(3, programResult.InProgressCredits);
                Assert.AreEqual(0, programResult.Credits);
                Assert.AreEqual(2, programResult.OtherAcademicCredits.Count);
                //other courses
                Assert.AreEqual("110", programResult.OtherAcademicCredits[0]);
                Assert.AreEqual("111", programResult.OtherAcademicCredits[1]);
                //first group result
                Assert.AreEqual("110", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].GetAcadCredId());
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].Result);
                Assert.AreEqual("111", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1].GetAcadCredId());
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1].Result);
                Assert.AreEqual("112", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[2].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[2].Result);
                //second group result
                Assert.AreEqual("110", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[0].GetAcadCredId());
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[0].Result);
                Assert.AreEqual("111", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[1].GetAcadCredId());
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[1].Result);
                Assert.AreEqual("112", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[2].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[2].Result);
                //third group result
                Assert.AreEqual("110", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[0].GetAcadCredId());
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[0].Result);
                Assert.AreEqual("111", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[1].GetAcadCredId());
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[1].Result);
                Assert.AreEqual("112", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[2].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[2].Result);
            }

            [TestMethod]
            public async Task Evaluate_ReplaceStatuses_AllCoursesAreCompeleted()
            {
                string studentid = "0016286";
                GetCredits(new List<string>() { studentid });

                //for this particular student all these acadcredits are completed
                //Colleague does a work for us by changing the status of acadcredits to replaced when  courses are completed.
                //we will assume in this test case that those statuses are already set by colleague
                //these student acadCredits are retaken but will replace and count only one because this course have retakeforcredits flag as N
                //113 is 2009/sp MATH-3300BB credit course- This will have Replaced flad
                //114 is 2010/sp math-300bb credit course- //This will have replaced flag
                //115 is 2017/sp math-300bb credit course
                creditDictionary["0016286"][0].RepeatAcademicCreditIds = new List<string>() { "113", "114", "115" };
                creditDictionary["0016286"][1].RepeatAcademicCreditIds = new List<string>() { "113", "114", "115" };
                creditDictionary["0016286"][2].RepeatAcademicCreditIds = new List<string>() { "113", "114", "115" };
                creditDictionary["0016286"][0].CanBeReplaced = true;
                creditDictionary["0016286"][1].CanBeReplaced = true;
                creditDictionary["0016286"][2].CanBeReplaced = true;
                creditDictionary["0016286"][0].ReplacedStatus = ReplacedStatus.Replaced;
                creditDictionary["0016286"][1].ReplacedStatus = ReplacedStatus.Replaced;
                creditDictionary["0016286"][2].StartDate = new DateTime(2017, 03, 01);

                var student = studentRepo.Get(studentid);
                var planningStudent = new Domain.Student.Entities.PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
                planningStudent.Advisements = student.Advisements;
                planningStudentRepoMock.Setup(psr => psr.GetAsync(studentid, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));

                academicCreditRepoMock.Setup(repo => repo.GetAcademicCreditByStudentIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns((IEnumerable<string> s, bool b1, bool b2, bool b3) => Task.FromResult(creditDictionary));
                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo, catalogRepo, planningConfigRepo,
                    referenceDataRepo, currentUserFactory, roleRepository, logger, configRepo);

                var programResult = (await programEvaluationService.EvaluateAsync(studentid, new List<string>() { "REPEAT.BB" }, null)).First();
                Assert.IsNotNull(programResult);
                //since these course are replacing each other and a flag on course math-300bb says do not retake for credits
                //total completed credits will be 3
                //113 and 113 will not be applied to any group but will fall in other group category
                //115 course will have status of replacement

                Assert.AreEqual(0, programResult.InProgressCredits);
                Assert.AreEqual(9, programResult.Credits);
                Assert.AreEqual(2, programResult.OtherAcademicCredits.Count);
                //other courses
                Assert.AreEqual("113", programResult.OtherAcademicCredits[0]);
                Assert.AreEqual("114", programResult.OtherAcademicCredits[1]);
                //first group result
                Assert.AreEqual("113", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].GetAcadCredId());
                Assert.AreEqual(Result.ReplacedWithGPAValues, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].Result);
                Assert.AreEqual("114", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1].GetAcadCredId());
                Assert.AreEqual(Result.ReplacedWithGPAValues, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1].Result);
                Assert.AreEqual("115", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[2].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[2].Result);
                //second group result
                Assert.AreEqual("113", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[0].GetAcadCredId());
                Assert.AreEqual(Result.ReplacedWithGPAValues, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[0].Result);
                Assert.AreEqual("114", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[1].GetAcadCredId());
                Assert.AreEqual(Result.ReplacedWithGPAValues, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[1].Result);
                Assert.AreEqual("115", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[2].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[2].Result);
                //third group result
                Assert.AreEqual("113", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[0].GetAcadCredId());
                Assert.AreEqual(Result.ReplacedWithGPAValues, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[0].Result);
                Assert.AreEqual("114", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[1].GetAcadCredId());
                Assert.AreEqual(Result.ReplacedWithGPAValues, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[1].Result);
                Assert.AreEqual("115", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[2].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[2].Result);
            }

            [TestMethod]
            public async Task Evaluate_ReplaceStatuses_AllCoursesAreCompeleted_WithCatalog()
            {
                string studentid = "0016286";
                GetCredits(new List<string>() { studentid });

                //for this particular student all these acadcredits are completed
                //Colleague does a work for us by changing the status of acadcredits to replaced when  courses are completed.
                //we will assume in this test case that those statuses are already set by colleague
                //these student acadCredits are retaken but will replace and count only one because this course have retakeforcredits flag as N
                //113 is 2009/sp MATH-3300BB credit course- This will have Replaced flad
                //114 is 2010/sp math-300bb credit course- //This will have replaced flag
                //115 is 2017/sp math-300bb credit course
                creditDictionary["0016286"][0].RepeatAcademicCreditIds = new List<string>() { "113", "114", "115" };
                creditDictionary["0016286"][1].RepeatAcademicCreditIds = new List<string>() { "113", "114", "115" };
                creditDictionary["0016286"][2].RepeatAcademicCreditIds = new List<string>() { "113", "114", "115" };
                creditDictionary["0016286"][0].CanBeReplaced = true;
                creditDictionary["0016286"][1].CanBeReplaced = true;
                creditDictionary["0016286"][2].CanBeReplaced = true;
                creditDictionary["0016286"][0].ReplacedStatus = ReplacedStatus.Replaced;
                creditDictionary["0016286"][1].ReplacedStatus = ReplacedStatus.Replaced;
                creditDictionary["0016286"][2].StartDate = new DateTime(2017, 03, 01);

                var student = studentRepo.Get(studentid);
                var planningStudent = new Domain.Student.Entities.PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
                planningStudent.Advisements = student.Advisements;
                planningStudentRepoMock.Setup(psr => psr.GetAsync(studentid, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));

                academicCreditRepoMock.Setup(repo => repo.GetAcademicCreditByStudentIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns((IEnumerable<string> s, bool b1, bool b2, bool b3) => Task.FromResult(creditDictionary));
                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo, catalogRepo, planningConfigRepo,
                    referenceDataRepo, currentUserFactory, roleRepository, logger, configRepo);

                var programResult = (await programEvaluationService.EvaluateAsync(studentid, new List<string>() { "REPEAT.BB" }, "2012")).First();
                Assert.IsNotNull(programResult);
                //since these course are replacing each other and a flag on course math-300bb says do not retake for credits
                //total completed credits will be 3
                //113 and 113 will not be applied to any group but will fall in other group category
                //115 course will have status of replacement

                Assert.AreEqual(0, programResult.InProgressCredits);
                Assert.AreEqual(9, programResult.Credits);
                Assert.AreEqual(2, programResult.OtherAcademicCredits.Count);
                //other courses
                Assert.AreEqual("113", programResult.OtherAcademicCredits[0]);
                Assert.AreEqual("114", programResult.OtherAcademicCredits[1]);
                //first group result
                Assert.AreEqual("113", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].GetAcadCredId());
                Assert.AreEqual(Result.ReplacedWithGPAValues, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].Result);
                Assert.AreEqual("114", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1].GetAcadCredId());
                Assert.AreEqual(Result.ReplacedWithGPAValues, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1].Result);
                Assert.AreEqual("115", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[2].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[2].Result);
                //second group result
                Assert.AreEqual("113", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[0].GetAcadCredId());
                Assert.AreEqual(Result.ReplacedWithGPAValues, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[0].Result);
                Assert.AreEqual("114", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[1].GetAcadCredId());
                Assert.AreEqual(Result.ReplacedWithGPAValues, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[1].Result);
                Assert.AreEqual("115", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[2].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[2].Result);
                //third group result
                Assert.AreEqual("113", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[0].GetAcadCredId());
                Assert.AreEqual(Result.ReplacedWithGPAValues, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[0].Result);
                Assert.AreEqual("114", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[1].GetAcadCredId());
                Assert.AreEqual(Result.ReplacedWithGPAValues, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[1].Result);
                Assert.AreEqual("115", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[2].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[2].Result);
            }

            [TestMethod]
            public async Task Evaluate_ReplaceStatuses_TwoCoursesAreCompeletedOneInProgress()
            {
                string studentid = "0016287";
                GetCredits(new List<string>() { studentid });

                //these student acadCredits are retaken but will replace and count only one because this course have retakeforcredits flag as N
                //111 is 2009/sp MATH-3300BB credit course- This is inprogress
                //113 is 2010/sp math-300bb credit course- //This is complete graded this will be replaced by collegue
                //115 is 2017/sp math-300bb credit course // this is complete- graded
                creditDictionary[studentid][0].RepeatAcademicCreditIds = new List<string>() { "111", "113", "115" };
                creditDictionary[studentid][1].RepeatAcademicCreditIds = new List<string>() { "111", "113", "115" };
                creditDictionary[studentid][2].RepeatAcademicCreditIds = new List<string>() { "111", "113", "115" };
                creditDictionary[studentid][0].CanBeReplaced = true;
                creditDictionary[studentid][1].CanBeReplaced = true;
                creditDictionary[studentid][2].CanBeReplaced = true;
                creditDictionary[studentid][1].ReplacedStatus = ReplacedStatus.Replaced;
                creditDictionary[studentid][1].AdjustedCredit = 0m;
                creditDictionary[studentid][0].EndDate = null;
                creditDictionary[studentid][2].StartDate = new DateTime(2017, 03, 01);

                var student = studentRepo.Get(studentid);
                var planningStudent = new Domain.Student.Entities.PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
                planningStudent.Advisements = student.Advisements;
                planningStudentRepoMock.Setup(psr => psr.GetAsync(studentid, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));

                academicCreditRepoMock.Setup(repo => repo.GetAcademicCreditByStudentIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns((IEnumerable<string> s, bool b1, bool b2, bool b3) => Task.FromResult(creditDictionary));
                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo, catalogRepo, planningConfigRepo,
                    referenceDataRepo, currentUserFactory, roleRepository, logger, configRepo);

                var programResult = (await programEvaluationService.EvaluateAsync(studentid, new List<string>() { "REPEAT.BB" }, null)).First();
                Assert.IsNotNull(programResult);
                //since these course are replacing each other and a flag on course math-300bb says do not retake for credits
                //total completed credits will be 3
                //113 and 113 will not be applied to any group but will fall in other group category
                //115 course will have status of replacement

                Assert.AreEqual(3, programResult.InProgressCredits);
                Assert.AreEqual(3, programResult.Credits);
                Assert.AreEqual(2, programResult.OtherAcademicCredits.Count);
                //other courses
                Assert.AreEqual("113", programResult.OtherAcademicCredits[0]);
                Assert.AreEqual("115", programResult.OtherAcademicCredits[1]);
                //first group result
                Assert.AreEqual("113", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].GetAcadCredId());
                Assert.AreEqual(Result.ReplacedWithGPAValues, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].Result);
                Assert.AreEqual("115", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1].GetAcadCredId());
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1].Result);
                Assert.AreEqual("111", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[2].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[2].Result);
                //second group result
                Assert.AreEqual("113", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[0].GetAcadCredId());
                Assert.AreEqual(Result.ReplacedWithGPAValues, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[0].Result);
                Assert.AreEqual("115", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[1].GetAcadCredId());
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[1].Result);
                Assert.AreEqual("111", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[2].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[2].Result);
                //third group result
                Assert.AreEqual("113", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[0].GetAcadCredId());
                Assert.AreEqual(Result.ReplacedWithGPAValues, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[0].Result);
                Assert.AreEqual("115", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[1].GetAcadCredId());
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[1].Result);
                Assert.AreEqual("111", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[2].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[2].Result);
            }

            [TestMethod]
            public async Task Evaluate_ReplaceStatuses_TwoCoursesAreCompletedOneInProgress_WithCatalog()
            {
                string studentid = "0016287";
                GetCredits(new List<string>() { studentid });

                //these student acadCredits are retaken but will replace and count only one because this course have retakeforcredits flag as N
                //111 is 2009/sp MATH-3300BB credit course- This is inprogress
                //113 is 2010/sp math-300bb credit course- //This is complete graded this will be replaced by collegue
                //115 is 2017/sp math-300bb credit course // this is complete- graded
                creditDictionary[studentid][0].RepeatAcademicCreditIds = new List<string>() { "111", "113", "115" };
                creditDictionary[studentid][1].RepeatAcademicCreditIds = new List<string>() { "111", "113", "115" };
                creditDictionary[studentid][2].RepeatAcademicCreditIds = new List<string>() { "111", "113", "115" };
                creditDictionary[studentid][0].CanBeReplaced = true;
                creditDictionary[studentid][1].CanBeReplaced = true;
                creditDictionary[studentid][2].CanBeReplaced = true;
                creditDictionary[studentid][1].ReplacedStatus = ReplacedStatus.Replaced;
                creditDictionary[studentid][1].AdjustedCredit = 0m;
                //adjusted credit for IP is null
                creditDictionary[studentid][0].AdjustedCredit = null;
                creditDictionary[studentid][0].EndDate = null;
                creditDictionary[studentid][2].StartDate = new DateTime(2017, 03, 01);

                var student = studentRepo.Get(studentid);
                var planningStudent = new Domain.Student.Entities.PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
                planningStudent.Advisements = student.Advisements;
                planningStudentRepoMock.Setup(psr => psr.GetAsync(studentid, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));

                academicCreditRepoMock.Setup(repo => repo.GetAcademicCreditByStudentIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns((IEnumerable<string> s, bool b1, bool b2, bool b3) => Task.FromResult(creditDictionary));
                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo, catalogRepo, planningConfigRepo,
                    referenceDataRepo, currentUserFactory, roleRepository, logger, configRepo);

                var programResult = (await programEvaluationService.EvaluateAsync(studentid, new List<string>() { "REPEAT.BB" }, "2012")).First();
                Assert.IsNotNull(programResult);
                //since these course are replacing each other and a flag on course math-300bb says do not retake for credits
                //total completed credits will be 3
                //113 and 113 will not be applied to any group but will fall in other group category
                //115 course will have status of replacement

                Assert.AreEqual(3, programResult.InProgressCredits);
                Assert.AreEqual(0, programResult.Credits);
                Assert.AreEqual(2, programResult.OtherAcademicCredits.Count);
                //other courses
                Assert.AreEqual("113", programResult.OtherAcademicCredits[0]);
                Assert.AreEqual("115", programResult.OtherAcademicCredits[1]);
                //first group result
                Assert.AreEqual("113", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].GetAcadCredId());
                Assert.AreEqual(Result.ReplacedWithGPAValues, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].Result);
                Assert.AreEqual("115", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1].GetAcadCredId());
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1].Result);
                Assert.AreEqual("111", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[2].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[2].Result);
                //second group result
                Assert.AreEqual("113", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[0].GetAcadCredId());
                Assert.AreEqual(Result.ReplacedWithGPAValues, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[0].Result);
                Assert.AreEqual("115", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[1].GetAcadCredId());
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[1].Result);
                Assert.AreEqual("111", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[2].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[2].Result);
                //third group result
                Assert.AreEqual("113", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[0].GetAcadCredId());
                Assert.AreEqual(Result.ReplacedWithGPAValues, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[0].Result);
                Assert.AreEqual("115", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[1].GetAcadCredId());
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[1].Result);
                Assert.AreEqual("111", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[2].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[2].Result);
            }

            [TestMethod]
            public async Task Evaluate_ReplaceStatuses_OneCourseIsCompletedTwoInProgress()
            {
                string studentid = "0016288";
                GetCredits(new List<string>() { studentid });
                creditDictionary[studentid][0].RepeatAcademicCreditIds = new List<string>() { "110", "112", "114" };
                creditDictionary[studentid][1].RepeatAcademicCreditIds = new List<string>() { "110", "112", "114" };
                creditDictionary[studentid][2].RepeatAcademicCreditIds = new List<string>() { "110", "112", "114" };
                creditDictionary[studentid][0].CanBeReplaced = true;
                creditDictionary[studentid][1].CanBeReplaced = true;
                creditDictionary[studentid][2].CanBeReplaced = true;
                creditDictionary[studentid][0].EndDate = null;
                creditDictionary[studentid][1].EndDate = null;
                creditDictionary[studentid][2].EndDate = null;
                creditDictionary[studentid][1].StartDate = new DateTime(2017, 03, 01);
                //make adjusted credit to null for IP
                creditDictionary[studentid][0].AdjustedCredit = null;
                creditDictionary[studentid][1].AdjustedCredit = null;

                var student = studentRepo.Get(studentid);
                var planningStudent = new Domain.Student.Entities.PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
                planningStudent.Advisements = student.Advisements;
                planningStudentRepoMock.Setup(psr => psr.GetAsync(studentid, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));

                academicCreditRepoMock.Setup(repo => repo.GetAcademicCreditByStudentIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns((IEnumerable<string> s, bool b1, bool b2, bool b3) => Task.FromResult(creditDictionary));
                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo, catalogRepo, planningConfigRepo,
                    referenceDataRepo, currentUserFactory, roleRepository, logger, configRepo);

                var programResult = (await programEvaluationService.EvaluateAsync(studentid, new List<string>() { "REPEAT.BB" }, null)).First();
                Assert.IsNotNull(programResult);
                Assert.AreEqual(3, programResult.InProgressCredits);
                Assert.AreEqual(0, programResult.Credits);
                Assert.AreEqual(2, programResult.OtherAcademicCredits.Count);
                //other courses
                Assert.AreEqual("110", programResult.OtherAcademicCredits[0]);
                Assert.AreEqual("114", programResult.OtherAcademicCredits[1]);
                //first group result
                Assert.AreEqual("114", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].GetAcadCredId());
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].Result);
                Assert.AreEqual("110", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1].GetAcadCredId());
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1].Result);
                Assert.AreEqual("112", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[2].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[2].Result);
                //second group result
                Assert.AreEqual("114", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[0].GetAcadCredId());
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[0].Result);
                Assert.AreEqual("110", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[1].GetAcadCredId());
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[1].Result);
                Assert.AreEqual("112", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[2].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[2].Result);
                //third group result
                Assert.AreEqual("114", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[0].GetAcadCredId());
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[0].Result);
                Assert.AreEqual("110", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[1].GetAcadCredId());
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[1].Result);
                Assert.AreEqual("112", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[2].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[2].Result);
            }

            [TestMethod]
            public async Task Evaluate_ReplaceStatuses_OneCourseIsCompletedTwoInProgress_WithCatalog()
            {
                string studentid = "0016288";
                GetCredits(new List<string>() { studentid });
                creditDictionary[studentid][0].RepeatAcademicCreditIds = new List<string>() { "110", "112", "114" };
                creditDictionary[studentid][1].RepeatAcademicCreditIds = new List<string>() { "110", "112", "114" };
                creditDictionary[studentid][2].RepeatAcademicCreditIds = new List<string>() { "110", "112", "114" };
                creditDictionary[studentid][0].CanBeReplaced = true;
                creditDictionary[studentid][1].CanBeReplaced = true;
                creditDictionary[studentid][2].CanBeReplaced = true;
                creditDictionary[studentid][0].EndDate = null;
                creditDictionary[studentid][1].EndDate = null;
                creditDictionary[studentid][2].EndDate = null;
                creditDictionary[studentid][1].StartDate = new DateTime(2017, 03, 01);
                creditDictionary[studentid][0].AdjustedCredit = null;
                creditDictionary[studentid][1].AdjustedCredit = null;

                var student = studentRepo.Get(studentid);
                var planningStudent = new Domain.Student.Entities.PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
                planningStudent.Advisements = student.Advisements;
                planningStudentRepoMock.Setup(psr => psr.GetAsync(studentid, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));

                academicCreditRepoMock.Setup(repo => repo.GetAcademicCreditByStudentIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns((IEnumerable<string> s, bool b1, bool b2, bool b3) => Task.FromResult(creditDictionary));
                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo, catalogRepo, planningConfigRepo,
                    referenceDataRepo, currentUserFactory, roleRepository, logger, configRepo);

                var programResult = (await programEvaluationService.EvaluateAsync(studentid, new List<string>() { "REPEAT.BB" }, "2012")).First();
                Assert.IsNotNull(programResult);
                Assert.AreEqual(3, programResult.InProgressCredits);
                Assert.AreEqual(0, programResult.Credits);
                Assert.AreEqual(2, programResult.OtherAcademicCredits.Count);
                //other courses
                Assert.AreEqual("110", programResult.OtherAcademicCredits[0]);
                Assert.AreEqual("114", programResult.OtherAcademicCredits[1]);
                //first group result
                Assert.AreEqual("114", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].GetAcadCredId());
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].Result);
                Assert.AreEqual("110", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1].GetAcadCredId());
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1].Result);
                Assert.AreEqual("112", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[2].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[2].Result);
                //second group result
                Assert.AreEqual("114", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[0].GetAcadCredId());
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[0].Result);
                Assert.AreEqual("110", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[1].GetAcadCredId());
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[1].Result);
                Assert.AreEqual("112", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[2].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[2].Result);
                //third group result
                Assert.AreEqual("114", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[0].GetAcadCredId());
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[0].Result);
                Assert.AreEqual("110", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[1].GetAcadCredId());
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[1].Result);
                Assert.AreEqual("112", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[2].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[2].Result);
            }

            [TestMethod]
            public async Task Evaluate_ReplaceStatuses_OneDroppedCourseOneInProgress()
            {
                string studentid = "0016291";
                GetCredits(studentIds: new List<string>() { studentid }, includeDrops: true);

                //{"0016291","ReplaceAndReplacement-student-7","","REPEAT.BB","122,124" }, //1 dropped no grade and 1 acad credit inprogress
                creditDictionary[studentid][0].RepeatAcademicCreditIds = new List<string>() { "122", "124" };
                creditDictionary[studentid][1].RepeatAcademicCreditIds = new List<string>() { "122", "124" };
                creditDictionary[studentid][0].CanBeReplaced = true;
                creditDictionary[studentid][1].CanBeReplaced = true;
                creditDictionary[studentid][0].EndDate = null;
                creditDictionary[studentid][1].EndDate = null;
                creditDictionary[studentid][1].StartDate = new DateTime(2019, 01, 20);

                var student = studentRepo.Get(studentid);
                var planningStudent = new Domain.Student.Entities.PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
                planningStudent.Advisements = student.Advisements;
                planningStudentRepoMock.Setup(psr => psr.GetAsync(studentid, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));

                academicCreditRepoMock.Setup(repo => repo.GetAcademicCreditByStudentIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns((IEnumerable<string> s, bool b1, bool b2, bool b3) => Task.FromResult(creditDictionary));
                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo, catalogRepo, planningConfigRepo,
                    referenceDataRepo, currentUserFactory, roleRepository, logger, configRepo);

                var programResult = (await programEvaluationService.EvaluateAsync(studentid, new List<string>() { "REPEAT.BB" }, null)).First();

                //since these course are replacing each other and a flag on course math-300bb says do not retake for credits
                //122 is 2018/SP MATH-3300BB credit course - Dropped with no grade: will not be applied to any group
                //124 is 2019/SP MATH-3300BB credit course - Inprogress: will be applied and will not have a replacement status

                Assert.IsNotNull(programResult);
                Assert.AreEqual(3, programResult.InProgressCredits);
                Assert.AreEqual(0, programResult.Credits);
                Assert.AreEqual(0, programResult.OtherAcademicCredits.Count);
                Assert.AreEqual(0, programResult.CumGpa.HasValue ? decimal.Round(programResult.CumGpa.Value, 3) : 0);

                //first group result
                Assert.AreEqual("124", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].Result);
                //second group result
                Assert.AreEqual("124", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[0].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[0].Result);
                //third group result
                Assert.AreEqual("124", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[0].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[0].Result);
            }

            [TestMethod]
            public async Task Evaluate_ReplaceStatuses_OneDroppedGradedCourseOneInProgress()
            {
                //{"0016292","ReplaceAndReplacement-student-8", "","REPEAT.BB","120,124" }, //1 drop graded, 1 inprogress
                string studentid = "0016292";
                GetCredits(studentIds: new List<string>() { studentid }, includeDrops: true);
                creditDictionary[studentid][0].RepeatAcademicCreditIds = new List<string>() { "121", "124" };
                creditDictionary[studentid][1].RepeatAcademicCreditIds = new List<string>() { "121", "124" };
                creditDictionary[studentid][0].CanBeReplaced = true;
                creditDictionary[studentid][1].CanBeReplaced = true;
                creditDictionary[studentid][0].EndDate = null;
                creditDictionary[studentid][1].EndDate = null;
                creditDictionary[studentid][1].StartDate = new DateTime(2019, 01, 20);

                var student = studentRepo.Get(studentid);
                var planningStudent = new Domain.Student.Entities.PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
                planningStudent.Advisements = student.Advisements;
                planningStudentRepoMock.Setup(psr => psr.GetAsync(studentid, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));

                academicCreditRepoMock.Setup(repo => repo.GetAcademicCreditByStudentIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns((IEnumerable<string> s, bool b1, bool b2, bool b3) => Task.FromResult(creditDictionary));
                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo, catalogRepo, planningConfigRepo,
                    referenceDataRepo, currentUserFactory, roleRepository, logger, configRepo);

                var programResult = (await programEvaluationService.EvaluateAsync(studentid, new List<string>() { "REPEAT.BB" }, null)).First();

                //{"0016292","ReplaceAndReplacement-student-8", "","REPEAT.BB","121,124" }, //1 drop graded, 1 inprogress
                //dropped will be in gpa, other credit but not applied
                //inprogress will be applied
                Assert.IsNotNull(programResult);
                Assert.AreEqual(3, programResult.InProgressCredits);
                Assert.AreEqual(3, programResult.Credits);
                Assert.AreEqual(1, programResult.OtherAcademicCredits.Count); //graded drop and inprogress
                Assert.AreEqual(3.333m, programResult.CumGpa.HasValue ? decimal.Round(programResult.CumGpa.Value, 3) : 0); //gpa will include drop courses

                //other courses
                Assert.AreEqual("121", programResult.OtherAcademicCredits[0]); //dropped

                //first group result
                Assert.AreEqual("124", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].Result);

                //second group result
                Assert.AreEqual("124", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[0].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[0].Result);

                //third group result
                Assert.AreEqual("124", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[0].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[0].Result);
            }

            [TestMethod]
            public async Task Evaluate_ReplaceStatuses_OneDroppedGradedCourseOneComplete()
            {
                //{"0016293","ReplaceAndReplacement-student-9", "","REPEAT.BB","120,114" }, //1 drop graded, 1 completed
                string studentid = "0016293";
                GetCredits(studentIds: new List<string>() { studentid }, includeDrops: true);
                creditDictionary[studentid][0].RepeatAcademicCreditIds = new List<string>() { "120", "114" };
                creditDictionary[studentid][1].RepeatAcademicCreditIds = new List<string>() { "120", "114" };
                creditDictionary[studentid][0].CanBeReplaced = true;
                creditDictionary[studentid][1].CanBeReplaced = true;
                creditDictionary[studentid][0].EndDate = null;
                creditDictionary[studentid][1].EndDate = null;
                creditDictionary[studentid][1].StartDate = new DateTime(2010, 01, 20);

                creditDictionary[studentid][0].ReplacedStatus = ReplacedStatus.Replaced; //need to set this for test data (normally done by colleague)
                creditDictionary[studentid][0].AdjustedCredit = 0m; //need to set this for test data (normally done by colleague)

                var student = studentRepo.Get(studentid);
                var planningStudent = new Domain.Student.Entities.PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
                planningStudent.Advisements = student.Advisements;
                planningStudentRepoMock.Setup(psr => psr.GetAsync(studentid, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));

                academicCreditRepoMock.Setup(repo => repo.GetAcademicCreditByStudentIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns((IEnumerable<string> s, bool b1, bool b2, bool b3) => Task.FromResult(creditDictionary));
                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo, catalogRepo, planningConfigRepo,
                    referenceDataRepo, currentUserFactory, roleRepository, logger, configRepo);

                var programResult = (await programEvaluationService.EvaluateAsync(studentid, new List<string>() { "REPEAT.BB" }, null)).First();

                //{"0016293","ReplaceAndReplacement-student-9", "","REPEAT.BB","120,114" }, //1 drop graded, 1 completed
                //dropped will be in gpa, other credit but not applied
                //inprogress will be applied
                Assert.IsNotNull(programResult);
                Assert.AreEqual(0, programResult.InProgressCredits);
                Assert.AreEqual(3, programResult.Credits); //dropped included
                Assert.AreEqual(1, programResult.OtherAcademicCredits.Count); //graded drop
                Assert.AreEqual(3.667m, programResult.CumGpa.HasValue ? decimal.Round(programResult.CumGpa.Value, 3) : 0); //gpa will include drop courses

                //other courses
                Assert.AreEqual("120", programResult.OtherAcademicCredits[0]); //dropped

                //first group result
                Assert.AreEqual("114", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].Result);
                //second group result
                Assert.AreEqual("114", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[0].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[0].Result);
                //third group result
                Assert.AreEqual("114", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[0].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[0].Result);
            }

            [TestMethod]
            public async Task Evaluate_ReplaceStatuses_TwoDroppedGradedCoursesOneDroppedUngradedOneInProgress()
            {
                //{"0016294","ReplaceAndReplacement-student-10","","REPEAT.BB","120,121,122,124" }, //2 drop graded, 1 drop ungraded, 1 inprogress
                string studentid = "0016294";
                GetCredits(studentIds: new List<string>() { studentid }, includeDrops: true);
                creditDictionary[studentid][0].RepeatAcademicCreditIds = new List<string>() { "120", "121", "122", "124" };
                creditDictionary[studentid][1].RepeatAcademicCreditIds = new List<string>() { "120", "121", "122", "124" };
                creditDictionary[studentid][2].RepeatAcademicCreditIds = new List<string>() { "120", "121", "122", "124" };
                creditDictionary[studentid][3].RepeatAcademicCreditIds = new List<string>() { "120", "121", "122", "124" };
                creditDictionary[studentid][0].CanBeReplaced = true;
                creditDictionary[studentid][1].CanBeReplaced = true;
                creditDictionary[studentid][2].CanBeReplaced = true;
                creditDictionary[studentid][3].CanBeReplaced = true;
                creditDictionary[studentid][0].EndDate = null;
                creditDictionary[studentid][1].EndDate = null;
                creditDictionary[studentid][2].EndDate = null;
                creditDictionary[studentid][3].EndDate = null;
                creditDictionary[studentid][3].StartDate = new DateTime(2019, 01, 20);

                var student = studentRepo.Get(studentid);
                var planningStudent = new Domain.Student.Entities.PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
                planningStudent.Advisements = student.Advisements;
                planningStudentRepoMock.Setup(psr => psr.GetAsync(studentid, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));

                academicCreditRepoMock.Setup(repo => repo.GetAcademicCreditByStudentIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns((IEnumerable<string> s, bool b1, bool b2, bool b3) => Task.FromResult(creditDictionary));
                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo, catalogRepo, planningConfigRepo,
                    referenceDataRepo, currentUserFactory, roleRepository, logger, configRepo);

                var programResult = (await programEvaluationService.EvaluateAsync(studentid, new List<string>() { "REPEAT.BB" }, null)).First();

                //{"0016294","ReplaceAndReplacement-student-10","","REPEAT.BB","120,121,122,124" }, //2 drop graded, 1 drop ungraded, 1 inprogress
                //120,121 dropped will be in gpa abd other credit but not applied
                //122 ignored
                //124 inprogress will be applied
                Assert.IsNotNull(programResult);
                Assert.AreEqual(3, programResult.InProgressCredits);
                Assert.AreEqual(6, programResult.Credits); //graded dropped included
                Assert.AreEqual(2, programResult.OtherAcademicCredits.Count); //graded dropped
                Assert.AreEqual(3.667m, programResult.CumGpa.HasValue ? decimal.Round(programResult.CumGpa.Value, 3) : 0); //gpa will include drop courses

                //other courses
                Assert.AreEqual("120", programResult.OtherAcademicCredits[0]); //graded dropped
                Assert.AreEqual("121", programResult.OtherAcademicCredits[1]); //graded dropped

                //first group result
                Assert.AreEqual("124", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].Result);
                //second group result
                Assert.AreEqual("124", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[0].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[0].Result);
                //third group result
                Assert.AreEqual("124", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[0].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[0].Result);
            }

            [TestMethod]
            public async Task Evaluate_ReplaceStatuses_TwoDroppedGradedCoursesOneDroppedUngradedOneCompleted()
            {
                //{"0016295","ReplaceAndReplacement-student-11","","REPEAT.BB","120,121,122,123" }, //2 drop graded, 1 drop ungraded, 1 completed
                string studentid = "0016295";
                GetCredits(studentIds: new List<string>() { studentid }, includeDrops: true);
                creditDictionary[studentid][0].RepeatAcademicCreditIds = new List<string>() { "120", "121", "122", "123" };
                creditDictionary[studentid][1].RepeatAcademicCreditIds = new List<string>() { "120", "121", "122", "123" };
                creditDictionary[studentid][2].RepeatAcademicCreditIds = new List<string>() { "120", "121", "122", "123" };
                creditDictionary[studentid][3].RepeatAcademicCreditIds = new List<string>() { "120", "121", "122", "123" };
                creditDictionary[studentid][0].CanBeReplaced = true;
                creditDictionary[studentid][1].CanBeReplaced = true;
                creditDictionary[studentid][2].CanBeReplaced = true;
                creditDictionary[studentid][3].CanBeReplaced = true;
                creditDictionary[studentid][0].EndDate = null;
                creditDictionary[studentid][1].EndDate = null;
                creditDictionary[studentid][2].EndDate = null;
                creditDictionary[studentid][3].EndDate = null;
                creditDictionary[studentid][3].StartDate = new DateTime(2018, 08, 01);

                var student = studentRepo.Get(studentid);
                var planningStudent = new Domain.Student.Entities.PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
                planningStudent.Advisements = student.Advisements;
                planningStudentRepoMock.Setup(psr => psr.GetAsync(studentid, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));

                academicCreditRepoMock.Setup(repo => repo.GetAcademicCreditByStudentIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns((IEnumerable<string> s, bool b1, bool b2, bool b3) => Task.FromResult(creditDictionary));
                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo, catalogRepo, planningConfigRepo,
                    referenceDataRepo, currentUserFactory, roleRepository, logger, configRepo);

                var programResult = (await programEvaluationService.EvaluateAsync(studentid, new List<string>() { "REPEAT.BB" }, null)).First();

                //{"0016295","ReplaceAndReplacement-student-11","","REPEAT.BB","120,121,122,123" }, //2 drop graded, 1 drop ungraded, 1 completed
                //120 drop graded (A), 121 drop graded (B), 122 drop ungraded, 123 completed (A)

                //since these course are replacing each other and a flag on course math-300bb says do not retake for credits
                //120, 121, 122 will not be applied to any group
                //123 course will be applied to requirement
                //120,121,123 will be used for gpa 

                Assert.IsNotNull(programResult);
                Assert.AreEqual(0, programResult.InProgressCredits);
                Assert.AreEqual(9, programResult.Credits); //graded drop and completed
                Assert.AreEqual(2, programResult.OtherAcademicCredits.Count); //graded drop 
                Assert.AreEqual(3.778m, programResult.CumGpa.HasValue ? decimal.Round(programResult.CumGpa.Value, 3) : 0); //gpa will include drop class

                //other courses
                Assert.AreEqual("120", programResult.OtherAcademicCredits[0]); //drop graded
                Assert.AreEqual("121", programResult.OtherAcademicCredits[1]); //drop graded

                //first group result
                Assert.AreEqual("123", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].Result);
                //second group result
                Assert.AreEqual("123", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[0].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[0].Result);
                //third group result
                Assert.AreEqual("123", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[0].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[0].Result);
            }

            [TestMethod]
            public async Task Evaluate_ReplaceStatuses_OneDroppedGradedCourseOneDroppedUngradedOneCompletedOneInProgress()
            {
                string studentid = "0016296";
                GetCredits(studentIds: new List<string>() { studentid }, includeDrops: true);
                creditDictionary[studentid][0].RepeatAcademicCreditIds = new List<string>() { "120", "122", "114", "124" };
                creditDictionary[studentid][1].RepeatAcademicCreditIds = new List<string>() { "120", "122", "114", "124" };
                creditDictionary[studentid][2].RepeatAcademicCreditIds = new List<string>() { "120", "122", "114", "124" };
                creditDictionary[studentid][3].RepeatAcademicCreditIds = new List<string>() { "120", "122", "114", "124" };
                creditDictionary[studentid][0].CanBeReplaced = true;
                creditDictionary[studentid][1].CanBeReplaced = true;
                creditDictionary[studentid][2].CanBeReplaced = true;
                creditDictionary[studentid][3].CanBeReplaced = true;
                creditDictionary[studentid][0].EndDate = null;
                creditDictionary[studentid][1].EndDate = null;
                creditDictionary[studentid][2].EndDate = null;
                creditDictionary[studentid][3].EndDate = null;
                creditDictionary[studentid][3].StartDate = new DateTime(2019, 01, 20);

                var student = studentRepo.Get(studentid);
                var planningStudent = new Domain.Student.Entities.PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
                planningStudent.Advisements = student.Advisements;
                planningStudentRepoMock.Setup(psr => psr.GetAsync(studentid, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));

                academicCreditRepoMock.Setup(repo => repo.GetAcademicCreditByStudentIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns((IEnumerable<string> s, bool b1, bool b2, bool b3) => Task.FromResult(creditDictionary));
                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo, catalogRepo, planningConfigRepo,
                    referenceDataRepo, currentUserFactory, roleRepository, logger, configRepo);

                var programResult = (await programEvaluationService.EvaluateAsync(studentid, new List<string>() { "REPEAT.BB" }, null)).First();


                //{"0016296","ReplaceAndReplacement-student-11","","REPEAT.BB","120,122,114,124"}, 
                //120 drop graded (A), 122 drop ungraded, 114 completed (B to be replaced), 124 inprogress

                //since these course are replacing each other and a flag on course math-300bb says do not retake for credits
                //total completed credits will be 3
                //total inprogress credits will be 3

                //120 and 122 will not be applied to any group
                //114 course will be applied to requirement
                //124 course will not be applied and will have a replacement inprogress status

                //TODO: why is this not true, where is the third grade coming from?
                //120,114 will be used for gpa

                Assert.IsNotNull(programResult);
                Assert.AreEqual(3, programResult.InProgressCredits);
                Assert.AreEqual(3, programResult.Credits); //graded drop and to be replaced course
                Assert.AreEqual(2, programResult.OtherAcademicCredits.Count); //graded drop and replaced course
                Assert.AreEqual(3.667m, programResult.CumGpa.HasValue ? decimal.Round(programResult.CumGpa.Value, 3) : 0); //gpa will include drop class

                //other courses
                Assert.AreEqual("120", programResult.OtherAcademicCredits[0]); //drop graded
                Assert.AreEqual("114", programResult.OtherAcademicCredits[1]); //replaced course

                //first group result
                Assert.AreEqual("114", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].GetAcadCredId());
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].Result);
                Assert.AreEqual("124", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1].Result);
                //second group result
                Assert.AreEqual("114", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[0].GetAcadCredId());
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[0].Result);
                Assert.AreEqual("124", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[1].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[1].Result);
                //third group result
                Assert.AreEqual("114", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[0].GetAcadCredId());
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[0].Result);
                Assert.AreEqual("124", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[1].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[1].Result);
            }

            [TestMethod]
            public async Task Evaluate_ReplaceStatuses_OneDroppedGradedCourseOneDroppedUngradedTwoCompleted()
            {
                string studentid = "0016297";
                GetCredits(studentIds: new List<string>() { studentid }, includeDrops: true);
                creditDictionary[studentid][0].RepeatAcademicCreditIds = new List<string>() { "121", "122", "114", "123" };
                creditDictionary[studentid][1].RepeatAcademicCreditIds = new List<string>() { "121", "122", "114", "123" };
                creditDictionary[studentid][2].RepeatAcademicCreditIds = new List<string>() { "121", "122", "114", "123" };
                creditDictionary[studentid][3].RepeatAcademicCreditIds = new List<string>() { "121", "122", "114", "123" };
                creditDictionary[studentid][0].CanBeReplaced = true;
                creditDictionary[studentid][1].CanBeReplaced = true;
                creditDictionary[studentid][2].CanBeReplaced = true;
                creditDictionary[studentid][3].CanBeReplaced = true;
                creditDictionary[studentid][0].EndDate = null;
                creditDictionary[studentid][1].EndDate = null;
                creditDictionary[studentid][2].EndDate = null;
                creditDictionary[studentid][3].EndDate = null;
                creditDictionary[studentid][3].StartDate = new DateTime(2018, 08, 01);

                creditDictionary[studentid][2].ReplacedStatus = ReplacedStatus.Replaced; //need to set this for test data (normally done by colleague)
                creditDictionary[studentid][2].AdjustedCredit = 0m; //need to set this for test data (normally done by colleague)

                var student = studentRepo.Get(studentid);
                var planningStudent = new Domain.Student.Entities.PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
                planningStudent.Advisements = student.Advisements;
                planningStudentRepoMock.Setup(psr => psr.GetAsync(studentid, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));

                academicCreditRepoMock.Setup(repo => repo.GetAcademicCreditByStudentIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns((IEnumerable<string> s, bool b1, bool b2, bool b3) => Task.FromResult(creditDictionary));
                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo, catalogRepo, planningConfigRepo,
                    referenceDataRepo, currentUserFactory, roleRepository, logger, configRepo);

                var programResult = (await programEvaluationService.EvaluateAsync(studentid, new List<string>() { "REPEAT.BB" }, null)).First();

                //{"0016296","ReplaceAndReplacement-student-11","","REPEAT.BB","120,122,114,123"}, 
                //120 drop graded (A), 122 drop ungraded, 114 completed (B replaced), 123 completed (A)

                //since these course are replacing each other and a flag on course math-300bb says do not retake for credits
                //120 and 122 will not be applied to any group
                //123 course will be applied to requirement - replacement status
                //114 course will not be applied and will have a replaced status
                //120,114,123 will be used for gpa 

                Assert.IsNotNull(programResult);
                Assert.AreEqual(0, programResult.InProgressCredits);
                Assert.AreEqual(6, programResult.Credits); //graded drop and replacement course
                Assert.AreEqual(2, programResult.OtherAcademicCredits.Count); //graded drop and replaced course
                Assert.AreEqual(3.778m, programResult.CumGpa.HasValue ? decimal.Round(programResult.CumGpa.Value, 3) : 0); //gpa will include drop class

                //other courses
                Assert.AreEqual("120", programResult.OtherAcademicCredits[0]); //drop graded
                Assert.AreEqual("114", programResult.OtherAcademicCredits[1]); //replaced course

                //first group result
                Assert.AreEqual("114", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].GetAcadCredId());
                Assert.AreEqual(Result.ReplacedWithGPAValues, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].Result);
                Assert.AreEqual("123", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1].Result);
                //second group result
                Assert.AreEqual("114", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[0].GetAcadCredId());
                Assert.AreEqual(Result.ReplacedWithGPAValues, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[0].Result);
                Assert.AreEqual("123", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[1].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[1].Result);
                //third group result
                Assert.AreEqual("114", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[0].GetAcadCredId());
                Assert.AreEqual(Result.ReplacedWithGPAValues, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[0].Result);
                Assert.AreEqual("123", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[1].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[1].Result);
            }

            //All are completed without Replaced flag - like with AVG grade policy

            [TestMethod]
            public async Task Evaluate_ReplaceStatuses_AllCoursesAreCompleted_NoneMarkedAsReplaced()
            {
                string studentid = "0016286";
                GetCredits(new List<string>() { studentid });

                //for this particular student all these acadcredits are completed
                //We willa ssume Colleague has grade policy set to AVG, hence
                //none of the completed course has replaced status set to R
                //these student acadCredits are retaken and all will  be counted because these courses are not replaced and retake for credits on CRSE is N
                //113 is 2009/sp MATH-3300BB credit course- 
                //114 is 2010/sp math-300bb credit course- 
                //115 is 2017/sp math-300bb credit course
                creditDictionary["0016286"][0].RepeatAcademicCreditIds = new List<string>() { "113", "114", "115" };
                creditDictionary["0016286"][1].RepeatAcademicCreditIds = new List<string>() { "113", "114", "115" };
                creditDictionary["0016286"][2].RepeatAcademicCreditIds = new List<string>() { "113", "114", "115" };
                creditDictionary["0016286"][0].CanBeReplaced = true;
                creditDictionary["0016286"][1].CanBeReplaced = true;
                creditDictionary["0016286"][2].CanBeReplaced = true;
                creditDictionary["0016286"][0].ReplacedStatus = ReplacedStatus.NotReplaced;
                creditDictionary["0016286"][1].ReplacedStatus = ReplacedStatus.NotReplaced;
                creditDictionary["0016286"][2].StartDate = new DateTime(2017, 03, 01);

                var student = studentRepo.Get(studentid);
                var planningStudent = new Domain.Student.Entities.PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
                planningStudent.Advisements = student.Advisements;
                planningStudentRepoMock.Setup(psr => psr.GetAsync(studentid, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));

                academicCreditRepoMock.Setup(repo => repo.GetAcademicCreditByStudentIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns((IEnumerable<string> s, bool b1, bool b2, bool b3) => Task.FromResult(creditDictionary));
                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo,
                    catalogRepo, planningConfigRepo, referenceDataRepo, currentUserFactory, roleRepository, logger, configRepo);
                var programResult = (await programEvaluationService.EvaluateAsync(studentid, new List<string>() { "REPEAT.BB" }, null)).First();
                Assert.IsNotNull(programResult);
                //since these courses are not replacing each other even though a flag on course math-300bb says do not retake for credits, credits will still be counted for these repeated ones
                //total completed credits will be 9
                //113 and 113 will  be applied to any group 
                //none of the course will have any repalced or replacement flag set

                Assert.AreEqual(0, programResult.InProgressCredits);
                Assert.AreEqual(9, programResult.Credits);
                Assert.AreEqual(0, programResult.OtherAcademicCredits.Count);
                //first group result
                Assert.AreEqual("113", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].Result);
                Assert.AreEqual("114", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1].Result);
                Assert.AreEqual("115", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[2].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[2].Result);
                //second group result
                Assert.AreEqual("113", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[0].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[0].Result);
                Assert.AreEqual("114", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[1].GetAcadCredId());
                Assert.AreEqual(Result.InCoursesListButAlreadyApplied, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[1].Result);
                Assert.AreEqual("115", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[2].GetAcadCredId());
                Assert.AreEqual(Result.InCoursesListButAlreadyApplied, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[2].Result);
                //third group result
                Assert.AreEqual("113", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[0].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[0].Result);
                Assert.AreEqual("114", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[1].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[1].Result);
                Assert.AreEqual("115", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[2].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[2].Result);

            }
            //All are completed with F grades and one good grade - like F in grade scheme has no repeat value

            [TestMethod]
            public async Task Evaluate_ReplaceStatuses_AllCoursesAreCompleted_WithFAndGoodGrades_FewMarkedAsReplaced()
            {
                string studentid = "0016286";
                GetCredits(new List<string>() { studentid });

                //for this particular student all these acadcredits are completed
                //We willa ssume Colleague has grade policy set to BEST,and F grade has no repeat value
                //these student acadCredits are retaken 
                //113 is 2009/sp MATH-3300BB credit course- F grade
                //114 is 2010/sp math-300bb credit course- A grade
                //115 is 2017/sp math-300bb credit course- B grade
                creditDictionary["0016286"][0].RepeatAcademicCreditIds = new List<string>() { "113", "114", "115" };
                creditDictionary["0016286"][1].RepeatAcademicCreditIds = new List<string>() { "113", "114", "115" };
                creditDictionary["0016286"][2].RepeatAcademicCreditIds = new List<string>() { "113", "114", "115" };
                creditDictionary["0016286"][0].CanBeReplaced = true;
                creditDictionary["0016286"][1].CanBeReplaced = true;
                creditDictionary["0016286"][2].CanBeReplaced = true;
                creditDictionary["0016286"][0].ReplacedStatus = ReplacedStatus.NotReplaced;
                creditDictionary["0016286"][1].ReplacedStatus = ReplacedStatus.NotReplaced;
                creditDictionary["0016286"][2].ReplacedStatus = ReplacedStatus.Replaced;
                creditDictionary["0016286"][2].StartDate = new DateTime(2017, 03, 01);

                var student = studentRepo.Get(studentid);
                var planningStudent = new Domain.Student.Entities.PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
                planningStudent.Advisements = student.Advisements;
                planningStudentRepoMock.Setup(psr => psr.GetAsync(studentid, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));

                academicCreditRepoMock.Setup(repo => repo.GetAcademicCreditByStudentIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns((IEnumerable<string> s, bool b1, bool b2, bool b3) => Task.FromResult(creditDictionary));
                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo,
                    catalogRepo, planningConfigRepo, referenceDataRepo, currentUserFactory, roleRepository, logger, configRepo);

                var programResult = (await programEvaluationService.EvaluateAsync(studentid, new List<string>() { "REPEAT.BB" }, null)).First();
                Assert.IsNotNull(programResult);
                //since these courses are not replacing each other even though a flag on course math-300bb says do not retake for credits, credits will still be counted for these repeated ones
                //total completed credits will be 6
                //113 and 113 will  be applied to any group 

                Assert.AreEqual(0, programResult.InProgressCredits);
                Assert.AreEqual(9, programResult.Credits);
                Assert.AreEqual(1, programResult.OtherAcademicCredits.Count);
                //other courses
                Assert.AreEqual("115", programResult.OtherAcademicCredits[0]);

                //first group result
                Assert.AreEqual("113", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].Result);
                Assert.AreEqual("114", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1].Result);
                Assert.AreEqual("115", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[2].GetAcadCredId());
                Assert.AreEqual(Result.ReplacedWithGPAValues, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[2].Result);
                //second group result
                Assert.AreEqual("113", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[0].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[0].Result);
                Assert.AreEqual("114", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[1].GetAcadCredId());
                Assert.AreEqual(Result.InCoursesListButAlreadyApplied, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[1].Result);
                Assert.AreEqual("115", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[2].GetAcadCredId());
                Assert.AreEqual(Result.ReplacedWithGPAValues, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[2].Result);
                //third group result
                Assert.AreEqual("113", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[0].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[0].Result);
                Assert.AreEqual("114", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[1].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[1].Result);
                Assert.AreEqual("115", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[2].GetAcadCredId());
                Assert.AreEqual(Result.ReplacedWithGPAValues, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[2].Result);

            }

            //repeats with planned courses such as planned course takes precedence over completed and inporgress courses
            //all planned with coure retake for credits is N
            [TestMethod]
            public async Task Evaluate_ReplaceStatuses_AllPlannedCourses()
            {
                string studentid = "0016302";
                //no academic credits
                creditDictionary = new Dictionary<string, List<AcademicCredit>>();

               

                var student = studentRepo.Get(studentid);
                var planningStudent = new Domain.Student.Entities.PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
                planningStudent.Advisements = student.Advisements;
                planningStudentRepoMock.Setup(psr => psr.GetAsync(studentid, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));
                
               academicCreditRepoMock.Setup(repo => repo.GetAcademicCreditByStudentIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns((IEnumerable<string> s, bool b1, bool b2, bool b3) => Task.FromResult(creditDictionary));
                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo,
                    catalogRepo, planningConfigRepo, referenceDataRepo, currentUserFactory, roleRepository, logger, configRepo);
                var programResult = (await programEvaluationService.EvaluateAsync(studentid, new List<string>() { "REPEAT.BB" }, null)).First();

                //7435 course is planned 3 times - 2012/FA, 2013/FA, 2014/FA  
                //2014/fa course will be applied and all others will end in other planned courses with notation of possible replace in progress
                Assert.IsNotNull(programResult);
                Assert.AreEqual(0, programResult.InProgressCredits);
                Assert.AreEqual(0, programResult.Credits);
                Assert.AreEqual(3m, programResult.PlannedCredits);
                Assert.AreEqual(0, programResult.OtherAcademicCredits.Count);
                Assert.AreEqual(2, programResult.OtherPlannedCredits.Count);
                //first group result
                Assert.AreEqual(3, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results.Count);
                Assert.AreEqual(1, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].GetAppliedAndPlannedApplied().Count());
                Assert.AreEqual("2014/FA", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].GetTermCode());
                Assert.AreEqual("7435", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].GetCourse().Id);
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].Result);
                Assert.AreEqual(ReplacedStatus.ReplaceInProgress, (programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0] as CourseResult).PlannedCourse.ReplacedStatus);
                Assert.AreEqual(null, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].GetAcadCredId());
                
                Assert.AreEqual("2016/FA", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1].GetTermCode());
                Assert.AreEqual("7435", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1].GetCourse().Id);
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1].Result);
                Assert.AreEqual(ReplacedStatus.ReplaceInProgress, (programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1] as CourseResult).PlannedCourse.ReplacedStatus);
                Assert.AreEqual(null, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1].GetAcadCredId());
               
                Assert.AreEqual("2029/FA", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[2].GetTermCode());
                Assert.AreEqual("7435", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[2].GetCourse().Id);
                Assert.AreEqual(Result.PlannedApplied, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[2].Result);
                Assert.AreEqual(Result.PlannedApplied, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[2].Result);
                Assert.AreEqual(ReplacementStatus.PossibleReplacement, (programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[2] as CourseResult).PlannedCourse.ReplacementStatus);

                //second group result
                Assert.AreEqual(3, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results.Count);
                Assert.AreEqual(1, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].GetAppliedAndPlannedApplied().Count());
                Assert.AreEqual("2014/FA", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[0].GetTermCode());
                Assert.AreEqual("7435", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[0].GetCourse().Id);
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[0].Result);
                Assert.AreEqual(ReplacedStatus.ReplaceInProgress, (programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[0] as CourseResult).PlannedCourse.ReplacedStatus);
                Assert.AreEqual(null, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[0].GetAcadCredId());

                Assert.AreEqual("2016/FA", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[1].GetTermCode());
                Assert.AreEqual("7435", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[1].GetCourse().Id);
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[1].Result);
                Assert.AreEqual(ReplacedStatus.ReplaceInProgress, (programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[1] as CourseResult).PlannedCourse.ReplacedStatus);
                Assert.AreEqual(null, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[1].GetAcadCredId());

                Assert.AreEqual("2029/FA", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[2].GetTermCode());
                Assert.AreEqual("7435", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[2].GetCourse().Id);
                Assert.AreEqual(Result.PlannedApplied, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[2].Result);
                Assert.AreEqual(Result.PlannedApplied, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[2].Result);
                Assert.AreEqual(ReplacementStatus.PossibleReplacement, (programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[2] as CourseResult).PlannedCourse.ReplacementStatus);
                //third group result
                Assert.AreEqual(3, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results.Count);
                Assert.AreEqual(1, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].GetAppliedAndPlannedApplied().Count());
                Assert.AreEqual("2014/FA", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[0].GetTermCode());
                Assert.AreEqual("7435", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[0].GetCourse().Id);
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[0].Result);
                Assert.AreEqual(ReplacedStatus.ReplaceInProgress, (programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[0] as CourseResult).PlannedCourse.ReplacedStatus);
                Assert.AreEqual(null, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[0].GetAcadCredId());

                Assert.AreEqual("2016/FA", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[1].GetTermCode());
                Assert.AreEqual("7435", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[1].GetCourse().Id);
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[1].Result);
                Assert.AreEqual(ReplacedStatus.ReplaceInProgress, (programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[1] as CourseResult).PlannedCourse.ReplacedStatus);
                Assert.AreEqual(null, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[1].GetAcadCredId());

                Assert.AreEqual("2029/FA", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[2].GetTermCode());
                Assert.AreEqual("7435", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[2].GetCourse().Id);
                Assert.AreEqual(Result.PlannedApplied, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[2].Result);
                Assert.AreEqual(Result.PlannedApplied, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[2].Result);
                Assert.AreEqual(ReplacementStatus.PossibleReplacement, (programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[2] as CourseResult).PlannedCourse.ReplacementStatus);

            }
            //planned with inprogress

            [TestMethod]
            public async Task Evaluate_ReplaceStatuses_PlannedWithInProgressCompletedCourses()
            {
                string studentid = "0016302";
                GetCredits(new List<string>() { studentid });

                //for this particular student all these acadcredits are completed
                //We willa ssume Colleague has grade policy set to BEST,and F grade has no repeat value
                //these student acadCredits are retaken 
                //123 is 2018/fa MATH-3300BB credit course-  completed
                //124 is 2019/sp math-300bb credit course- inprogress
                //119 is 2018/sp  engl-201 completed course
                creditDictionary["0016302"][0].RepeatAcademicCreditIds = new List<string>() { "123", "124" };
                creditDictionary["0016302"][1].RepeatAcademicCreditIds = new List<string>() { "123", "124", };
                creditDictionary["0016302"][0].CanBeReplaced = true;
                creditDictionary["0016302"][1].CanBeReplaced = true;
                creditDictionary["0016302"][2].CanBeReplaced = true;
                creditDictionary["0016302"][0].ReplacedStatus = ReplacedStatus.NotReplaced;
                creditDictionary["0016302"][1].ReplacedStatus = ReplacedStatus.NotReplaced;
                creditDictionary["0016302"][2].ReplacedStatus = ReplacedStatus.NotReplaced;



                var student = studentRepo.Get(studentid);
                var planningStudent = new Domain.Student.Entities.PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
                planningStudent.Advisements = student.Advisements;
                planningStudentRepoMock.Setup(psr => psr.GetAsync(studentid, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));

                academicCreditRepoMock.Setup(repo => repo.GetAcademicCreditByStudentIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns((IEnumerable<string> s, bool b1, bool b2, bool b3) => Task.FromResult(creditDictionary));
                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo,
                    catalogRepo, planningConfigRepo, referenceDataRepo, currentUserFactory, roleRepository, logger, configRepo);
                var programResult = (await programEvaluationService.EvaluateAsync(studentid, new List<string>() { "REPEAT.BB" }, null)).First();

                //7435 course is planned 3 times - 2012/FA, 2013/FA, 2014/FA  
                //2014/fa course will be applied and all others will end in other planned courses with notation of possible replace in progress
                Assert.IsNotNull(programResult);
                Assert.AreEqual(0, programResult.InProgressCredits);
                Assert.AreEqual(0, programResult.Credits);
                Assert.AreEqual(3m, programResult.PlannedCredits);
                Assert.AreEqual(2, programResult.OtherAcademicCredits.Count);
                Assert.AreEqual(2, programResult.OtherPlannedCredits.Count);
                //first group result
                Assert.AreEqual(5, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results.Count);
                Assert.AreEqual(1, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].GetAppliedAndPlannedApplied().Count());
                Assert.AreEqual("2018/FA", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].GetTermCode());
                Assert.AreEqual("7435", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].GetCourse().Id);
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].Result);
                Assert.AreEqual(ReplacedStatus.ReplaceInProgress, (programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0] as CreditResult).Credit.ReplacedStatus);
                Assert.AreEqual("123", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].GetAcadCredId());

                Assert.AreEqual("2019/SP", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1].GetTermCode());
                Assert.AreEqual("7435", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1].GetCourse().Id);
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1].Result);
                Assert.AreEqual(ReplacedStatus.ReplaceInProgress, (programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1] as CreditResult).Credit.ReplacedStatus);
                Assert.AreEqual("124", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1].GetAcadCredId());

                Assert.AreEqual("2014/FA", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[2].GetTermCode());
                Assert.AreEqual("7435", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[2].GetCourse().Id);
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[2].Result);
                Assert.AreEqual(ReplacedStatus.ReplaceInProgress, (programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[2] as CourseResult).PlannedCourse.ReplacedStatus);
                Assert.AreEqual(null, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[2].GetAcadCredId());

                Assert.AreEqual("2016/FA", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[3].GetTermCode());
                Assert.AreEqual("7435", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[3].GetCourse().Id);
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[3].Result);
                Assert.AreEqual(ReplacedStatus.ReplaceInProgress, (programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[3] as CourseResult).PlannedCourse.ReplacedStatus);
                Assert.AreEqual(null, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[3].GetAcadCredId());

                Assert.AreEqual("2029/FA", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[4].GetTermCode());
                Assert.AreEqual("7435", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[4].GetCourse().Id);
                Assert.AreEqual(Result.PlannedApplied, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[4].Result);
                Assert.AreEqual(Result.PlannedApplied, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[4].Result);
                Assert.AreEqual(ReplacementStatus.PossibleReplacement, (programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[4] as CourseResult).PlannedCourse.ReplacementStatus);

                //second group result
                Assert.AreEqual(5, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results.Count);
                Assert.AreEqual(1, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].GetAppliedAndPlannedApplied().Count());
                Assert.AreEqual("2018/FA", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[0].GetTermCode());
                Assert.AreEqual("7435", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[0].GetCourse().Id);
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[0].Result);
                Assert.AreEqual(ReplacedStatus.ReplaceInProgress, (programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[0] as CreditResult).Credit.ReplacedStatus);
                Assert.AreEqual("123", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[0].GetAcadCredId());

                Assert.AreEqual("2019/SP", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[1].GetTermCode());
                Assert.AreEqual("7435", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[1].GetCourse().Id);
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[1].Result);
                Assert.AreEqual(ReplacedStatus.ReplaceInProgress, (programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[1] as CreditResult).Credit.ReplacedStatus);
                Assert.AreEqual("124", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[1].GetAcadCredId());

                Assert.AreEqual("2014/FA", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[2].GetTermCode());
                Assert.AreEqual("7435", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[2].GetCourse().Id);
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[2].Result);
                Assert.AreEqual(ReplacedStatus.ReplaceInProgress, (programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[2] as CourseResult).PlannedCourse.ReplacedStatus);
                Assert.AreEqual(null, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[2].GetAcadCredId());

                Assert.AreEqual("2016/FA", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[3].GetTermCode());
                Assert.AreEqual("7435", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[3].GetCourse().Id);
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[3].Result);
                Assert.AreEqual(ReplacedStatus.ReplaceInProgress, (programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[3] as CourseResult).PlannedCourse.ReplacedStatus);
                Assert.AreEqual(null, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[3].GetAcadCredId());

                Assert.AreEqual("2029/FA", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[4].GetTermCode());
                Assert.AreEqual("7435", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[4].GetCourse().Id);
                Assert.AreEqual(Result.PlannedApplied, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[4].Result);
                Assert.AreEqual(Result.PlannedApplied, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[4].Result);
                Assert.AreEqual(ReplacementStatus.PossibleReplacement, (programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[4] as CourseResult).PlannedCourse.ReplacementStatus);

                //third group result
                Assert.AreEqual(5, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results.Count);
                Assert.AreEqual(1, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].GetAppliedAndPlannedApplied().Count());
                Assert.AreEqual("2018/FA", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[0].GetTermCode());
                Assert.AreEqual("7435", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[0].GetCourse().Id);
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[0].Result);
                Assert.AreEqual(ReplacedStatus.ReplaceInProgress, (programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[0] as CreditResult).Credit.ReplacedStatus);
                Assert.AreEqual("123", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[0].GetAcadCredId());

                Assert.AreEqual("2019/SP", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[1].GetTermCode());
                Assert.AreEqual("7435", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[1].GetCourse().Id);
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[1].Result);
                Assert.AreEqual(ReplacedStatus.ReplaceInProgress, (programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[1] as CreditResult).Credit.ReplacedStatus);
                Assert.AreEqual("124", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[1].GetAcadCredId());

                Assert.AreEqual("2014/FA", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[2].GetTermCode());
                Assert.AreEqual("7435", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[2].GetCourse().Id);
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[2].Result);
                Assert.AreEqual(ReplacedStatus.ReplaceInProgress, (programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[2] as CourseResult).PlannedCourse.ReplacedStatus);
                Assert.AreEqual(null, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[2].GetAcadCredId());

                Assert.AreEqual("2016/FA", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[3].GetTermCode());
                Assert.AreEqual("7435", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[3].GetCourse().Id);
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[3].Result);
                Assert.AreEqual(ReplacedStatus.ReplaceInProgress, (programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[3] as CourseResult).PlannedCourse.ReplacedStatus);
                Assert.AreEqual(null, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[3].GetAcadCredId());

                Assert.AreEqual("2029/FA", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[4].GetTermCode());
                Assert.AreEqual("7435", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[4].GetCourse().Id);
                Assert.AreEqual(Result.PlannedApplied, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[4].Result);
                Assert.AreEqual(Result.PlannedApplied, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[4].Result);
                Assert.AreEqual(ReplacementStatus.PossibleReplacement, (programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[4] as CourseResult).PlannedCourse.ReplacementStatus);
            }

            [TestMethod]
            public async Task Evaluate_CumulativeGPA_DoNotCountReplaceInProgressCompletedCourses()
            {
                string studentid = "0016287";
                GetCredits(new List<string>() { studentid });

                //these student acadCredits are retaken but will replace and count only one because this course have retakeforcredits flag as N; 
                //degree parameters says exclude repeated completed courses for cum gpa
                //111 is 2009/sp MATH-3300BB credit course- This is inprogress
                //113 is 2010/sp math-300bb credit course- //This is complete graded this will be replaced by collegue
                //115 is 2017/sp math-300bb credit course // this is complete- graded
                creditDictionary[studentid][0].RepeatAcademicCreditIds = new List<string>() { "111", "113", "115" };
                creditDictionary[studentid][1].RepeatAcademicCreditIds = new List<string>() { "111", "113", "115" };
                creditDictionary[studentid][2].RepeatAcademicCreditIds = new List<string>() { "111", "113", "115" };
                creditDictionary[studentid][0].CanBeReplaced = true;
                creditDictionary[studentid][1].CanBeReplaced = true;
                creditDictionary[studentid][2].CanBeReplaced = true;
                creditDictionary[studentid][1].ReplacedStatus = ReplacedStatus.Replaced;
                creditDictionary[studentid][1].AdjustedCredit = 0m;
                //adjusted credit for IP is null
                creditDictionary[studentid][0].AdjustedCredit = null;
                creditDictionary[studentid][0].EndDate = null;
                //For 115 completed acad credit GPA points and credits will be adjusted to 0 since there is another inprogress course that would take the precedence 
                creditDictionary[studentid][2].StartDate = new DateTime(2017, 03, 01);

                var student = studentRepo.Get(studentid);
                var planningStudent = new Domain.Student.Entities.PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
                planningStudent.Advisements = student.Advisements;
                planningStudentRepoMock.Setup(psr => psr.GetAsync(studentid, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));

                academicCreditRepoMock.Setup(repo => repo.GetAcademicCreditByStudentIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns((IEnumerable<string> s, bool b1, bool b2, bool b3) => Task.FromResult(creditDictionary));
                requirementRepoMock.Setup(repo => repo.GetDegreeAuditParametersAsync()).Returns(Task.FromResult(new DegreeAuditParameters(ExtraCourses.Apply, false, false, false, true)));
                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo, catalogRepo, planningConfigRepo,
                    referenceDataRepo, currentUserFactory, roleRepository, logger, configRepo);

                var programResult = (await programEvaluationService.EvaluateAsync(studentid, new List<string>() { "REPEAT.BB" }, "2012")).First();
                Assert.IsNotNull(programResult);
                //since these course are replacing each other and a flag on course math-300bb says do not retake for credits
                //total inprogress credits will be 3 ; completed credits are 0; 

                Assert.AreEqual(3, programResult.InProgressCredits);
                Assert.AreEqual(0, programResult.Credits);
                Assert.AreEqual(2, programResult.OtherAcademicCredits.Count);
                Assert.AreEqual(4m, programResult.CumGpa);
                Assert.AreEqual(4m, programResult.InstGpa);
                //other courses
                Assert.AreEqual("113", programResult.OtherAcademicCredits[0]);
                Assert.AreEqual("115", programResult.OtherAcademicCredits[1]);
                //first group result
                Assert.AreEqual("113", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].GetAcadCredId());
                Assert.AreEqual(Result.ReplacedWithGPAValues, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].Result);
                Assert.AreEqual("115", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1].GetAcadCredId());
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1].Result);
                Assert.AreEqual("111", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[2].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[2].Result);
                //second group result
                Assert.AreEqual("113", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[0].GetAcadCredId());
                Assert.AreEqual(Result.ReplacedWithGPAValues, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[0].Result);
                Assert.AreEqual("115", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[1].GetAcadCredId());
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[1].Result);
                Assert.AreEqual("111", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[2].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[2].Result);
                //third group result
                Assert.AreEqual("113", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[0].GetAcadCredId());
                Assert.AreEqual(Result.ReplacedWithGPAValues, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[0].Result);
                Assert.AreEqual("115", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[1].GetAcadCredId());
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[1].Result);
                Assert.AreEqual("111", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[2].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[2].Result);
            }

            [TestMethod]
            public async Task Evaluate_CumulativeGPA_CountReplaceInProgressCompletedCourses()
            {
                string studentid = "0016287";
                GetCredits(new List<string>() { studentid });

                //these student acadCredits are retaken but will replace and count only one because this course have retakeforcredits flag as N
                //111 is 2009/sp MATH-3300BB credit course- This is inprogress
                //113 is 2010/sp math-300bb credit course- //This is complete graded this will be replaced by collegue
                //115 is 2017/sp math-300bb credit course // this is complete- graded
                creditDictionary[studentid][0].RepeatAcademicCreditIds = new List<string>() { "111", "113", "115" };
                creditDictionary[studentid][1].RepeatAcademicCreditIds = new List<string>() { "111", "113", "115" };
                creditDictionary[studentid][2].RepeatAcademicCreditIds = new List<string>() { "111", "113", "115" };
                creditDictionary[studentid][0].CanBeReplaced = true;
                creditDictionary[studentid][1].CanBeReplaced = true;
                creditDictionary[studentid][2].CanBeReplaced = true;
                creditDictionary[studentid][1].ReplacedStatus = ReplacedStatus.Replaced;
                creditDictionary[studentid][1].AdjustedCredit = 0m;
                //adjusted credit for IP is null
                creditDictionary[studentid][0].AdjustedCredit = null;
                creditDictionary[studentid][0].EndDate = null;
                creditDictionary[studentid][2].StartDate = new DateTime(2017, 03, 01);

                var student = studentRepo.Get(studentid);
                var planningStudent = new Domain.Student.Entities.PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
                planningStudent.Advisements = student.Advisements;
                planningStudentRepoMock.Setup(psr => psr.GetAsync(studentid, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));

                academicCreditRepoMock.Setup(repo => repo.GetAcademicCreditByStudentIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns((IEnumerable<string> s, bool b1, bool b2, bool b3) => Task.FromResult(creditDictionary));
                requirementRepoMock.Setup(repo => repo.GetDegreeAuditParametersAsync()).Returns(Task.FromResult(new DegreeAuditParameters(ExtraCourses.Apply, false, false, false, false)));
                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo, catalogRepo, planningConfigRepo,
                    referenceDataRepo, currentUserFactory, roleRepository, logger, configRepo);

                var programResult = (await programEvaluationService.EvaluateAsync(studentid, new List<string>() { "REPEAT.BB" }, "2012")).First();
                Assert.IsNotNull(programResult);

                Assert.AreEqual(3, programResult.InProgressCredits);
                Assert.AreEqual(0, programResult.Credits);
                Assert.AreEqual(2, programResult.OtherAcademicCredits.Count);
                Assert.AreEqual(3.33m, Decimal.Round(programResult.CumGpa.Value,2));
                Assert.AreEqual(3.33m, Decimal.Round(programResult.InstGpa.Value, 2));
                //other courses
                Assert.AreEqual("113", programResult.OtherAcademicCredits[0]);
                Assert.AreEqual("115", programResult.OtherAcademicCredits[1]);
                //first group result
                Assert.AreEqual("113", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].GetAcadCredId());
                Assert.AreEqual(Result.ReplacedWithGPAValues, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].Result);
                Assert.AreEqual("115", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1].GetAcadCredId());
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1].Result);
                Assert.AreEqual("111", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[2].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[2].Result);
                //second group result
                Assert.AreEqual("113", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[0].GetAcadCredId());
                Assert.AreEqual(Result.ReplacedWithGPAValues, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[0].Result);
                Assert.AreEqual("115", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[1].GetAcadCredId());
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[1].Result);
                Assert.AreEqual("111", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[2].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[2].Result);
                //third group result
                Assert.AreEqual("113", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[0].GetAcadCredId());
                Assert.AreEqual(Result.ReplacedWithGPAValues, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[0].Result);
                Assert.AreEqual("115", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[1].GetAcadCredId());
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[1].Result);
                Assert.AreEqual("111", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[2].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[2].Result);
            }

            //repeats with planned courses such as completed and inporgress courses takes precedence over planned courses
            //all planned with coure retake for credits is N
            [TestMethod]
            public async Task Evaluate_ReplaceStatuses_AllPlannedCourses_ApplyRepeatedFlagAsY()
            {
                string studentid = "0016302";
                //no academic credits
                creditDictionary = new Dictionary<string, List<AcademicCredit>>();



                var student = studentRepo.Get(studentid);
                var planningStudent = new Domain.Student.Entities.PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
                planningStudent.Advisements = student.Advisements;
                planningStudentRepoMock.Setup(psr => psr.GetAsync(studentid, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));
                requirementRepoMock.Setup(repo => repo.GetDegreeAuditParametersAsync()).Returns(Task.FromResult(new DegreeAuditParameters(ExtraCourses.Apply, false, false, false, false, true)));

                academicCreditRepoMock.Setup(repo => repo.GetAcademicCreditByStudentIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns((IEnumerable<string> s, bool b1, bool b2, bool b3) => Task.FromResult(creditDictionary));
                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo,
                    catalogRepo, planningConfigRepo, referenceDataRepo, currentUserFactory, roleRepository, logger, configRepo);
                var programResult = (await programEvaluationService.EvaluateAsync(studentid, new List<string>() { "REPEAT.BB" }, null)).First();

                //7435 course is planned 3 times - 2012/FA, 2013/FA, 2014/FA  
                //2014/fa course will be applied and all others will end in other planned courses with notation of possible replace in progress
                Assert.IsNotNull(programResult);
                Assert.AreEqual(0, programResult.InProgressCredits);
                Assert.AreEqual(0, programResult.Credits);
                Assert.AreEqual(3m, programResult.PlannedCredits);
                Assert.AreEqual(0, programResult.OtherAcademicCredits.Count);
                Assert.AreEqual(2, programResult.OtherPlannedCredits.Count);
                //first group result
                Assert.AreEqual(3, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results.Count);
                Assert.AreEqual(1, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].GetAppliedAndPlannedApplied().Count());
                Assert.AreEqual("2014/FA", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].GetTermCode());
                Assert.AreEqual("7435", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].GetCourse().Id);
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].Result);
                Assert.AreEqual(ReplacedStatus.ReplaceInProgress, (programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0] as CourseResult).PlannedCourse.ReplacedStatus);
                Assert.AreEqual(null, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].GetAcadCredId());

                Assert.AreEqual("2016/FA", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1].GetTermCode());
                Assert.AreEqual("7435", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1].GetCourse().Id);
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1].Result);
                Assert.AreEqual(ReplacedStatus.ReplaceInProgress, (programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1] as CourseResult).PlannedCourse.ReplacedStatus);
                Assert.AreEqual(null, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1].GetAcadCredId());

                Assert.AreEqual("2029/FA", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[2].GetTermCode());
                Assert.AreEqual("7435", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[2].GetCourse().Id);
                Assert.AreEqual(Result.PlannedApplied, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[2].Result);
                Assert.AreEqual(Result.PlannedApplied, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[2].Result);
                Assert.AreEqual(ReplacementStatus.PossibleReplacement, (programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[2] as CourseResult).PlannedCourse.ReplacementStatus);

                //second group result
                Assert.AreEqual(3, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results.Count);
                Assert.AreEqual(1, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].GetAppliedAndPlannedApplied().Count());
                Assert.AreEqual("2014/FA", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[0].GetTermCode());
                Assert.AreEqual("7435", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[0].GetCourse().Id);
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[0].Result);
                Assert.AreEqual(ReplacedStatus.ReplaceInProgress, (programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[0] as CourseResult).PlannedCourse.ReplacedStatus);
                Assert.AreEqual(null, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[0].GetAcadCredId());

                Assert.AreEqual("2016/FA", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[1].GetTermCode());
                Assert.AreEqual("7435", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[1].GetCourse().Id);
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[1].Result);
                Assert.AreEqual(ReplacedStatus.ReplaceInProgress, (programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[1] as CourseResult).PlannedCourse.ReplacedStatus);
                Assert.AreEqual(null, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[1].GetAcadCredId());

                Assert.AreEqual("2029/FA", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[2].GetTermCode());
                Assert.AreEqual("7435", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[2].GetCourse().Id);
                Assert.AreEqual(Result.PlannedApplied, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[2].Result);
                Assert.AreEqual(Result.PlannedApplied, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[2].Result);
                Assert.AreEqual(ReplacementStatus.PossibleReplacement, (programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[2] as CourseResult).PlannedCourse.ReplacementStatus);
                //third group result
                Assert.AreEqual(3, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results.Count);
                Assert.AreEqual(1, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].GetAppliedAndPlannedApplied().Count());
                Assert.AreEqual("2014/FA", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[0].GetTermCode());
                Assert.AreEqual("7435", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[0].GetCourse().Id);
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[0].Result);
                Assert.AreEqual(ReplacedStatus.ReplaceInProgress, (programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[0] as CourseResult).PlannedCourse.ReplacedStatus);
                Assert.AreEqual(null, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[0].GetAcadCredId());

                Assert.AreEqual("2016/FA", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[1].GetTermCode());
                Assert.AreEqual("7435", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[1].GetCourse().Id);
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[1].Result);
                Assert.AreEqual(ReplacedStatus.ReplaceInProgress, (programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[1] as CourseResult).PlannedCourse.ReplacedStatus);
                Assert.AreEqual(null, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[1].GetAcadCredId());

                Assert.AreEqual("2029/FA", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[2].GetTermCode());
                Assert.AreEqual("7435", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[2].GetCourse().Id);
                Assert.AreEqual(Result.PlannedApplied, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[2].Result);
                Assert.AreEqual(Result.PlannedApplied, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[2].Result);
                Assert.AreEqual(ReplacementStatus.PossibleReplacement, (programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[2] as CourseResult).PlannedCourse.ReplacementStatus);

            }
            //planned with inprogress

            [TestMethod]
            public async Task Evaluate_ReplaceStatuses_PlannedWithInProgressCompletedCourses_ApplyRepeatedFlagAsY()
            {
                string studentid = "0016302";
                GetCredits(new List<string>() { studentid });

                //for this particular student all these acadcredits are completed
                //We willa ssume Colleague has grade policy set to BEST,and F grade has no repeat value
                //these student acadCredits are retaken 
                //123 is 2018/fa MATH-3300BB credit course-  completed
                //124 is 2019/sp math-300bb credit course- inprogress
                //119 is 2018/sp  engl-201 completed course
                creditDictionary["0016302"][0].RepeatAcademicCreditIds = new List<string>() { "123", "124" };
                creditDictionary["0016302"][1].RepeatAcademicCreditIds = new List<string>() { "123", "124", };
                creditDictionary["0016302"][0].CanBeReplaced = true;
                creditDictionary["0016302"][1].CanBeReplaced = true;
                creditDictionary["0016302"][2].CanBeReplaced = true;
                creditDictionary["0016302"][0].ReplacedStatus = ReplacedStatus.NotReplaced;
                creditDictionary["0016302"][1].ReplacedStatus = ReplacedStatus.NotReplaced;
                creditDictionary["0016302"][2].ReplacedStatus = ReplacedStatus.NotReplaced;



                var student = studentRepo.Get(studentid);
                var planningStudent = new Domain.Student.Entities.PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
                planningStudent.Advisements = student.Advisements;
                planningStudentRepoMock.Setup(psr => psr.GetAsync(studentid, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));
                requirementRepoMock.Setup(repo => repo.GetDegreeAuditParametersAsync()).Returns(Task.FromResult(new DegreeAuditParameters(ExtraCourses.Apply, false, false, false, false, true)));

                academicCreditRepoMock.Setup(repo => repo.GetAcademicCreditByStudentIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns((IEnumerable<string> s, bool b1, bool b2, bool b3) => Task.FromResult(creditDictionary));
                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo,
                    catalogRepo, planningConfigRepo, referenceDataRepo, currentUserFactory, roleRepository, logger, configRepo);
                var programResult = (await programEvaluationService.EvaluateAsync(studentid, new List<string>() { "REPEAT.BB" }, null)).First();

                //7435 course is planned 3 times - 2012/FA, 2013/FA, 2014/FA  
                //2014/fa course will be applied and all others will end in other planned courses with notation of possible replace in progress
                Assert.IsNotNull(programResult);
                Assert.AreEqual(3m, programResult.InProgressCredits);
                Assert.AreEqual(0, programResult.Credits);
                Assert.AreEqual(0, programResult.PlannedCredits);
                Assert.AreEqual(1, programResult.OtherAcademicCredits.Count);
                Assert.AreEqual(3, programResult.OtherPlannedCredits.Count);
                //first group result
                Assert.AreEqual(5, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results.Count);
                Assert.AreEqual(1, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].GetAppliedAndPlannedApplied().Count());
                Assert.AreEqual("2018/FA", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].GetTermCode());
                Assert.AreEqual("7435", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].GetCourse().Id);
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].Result);
                Assert.AreEqual(ReplacedStatus.ReplaceInProgress, (programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0] as CreditResult).Credit.ReplacedStatus);
                Assert.AreEqual("123", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].GetAcadCredId());

                Assert.AreEqual("2019/SP", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1].GetTermCode());
                Assert.AreEqual("7435", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1].GetCourse().Id);
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1].Result);
                Assert.AreEqual(ReplacementStatus.PossibleReplacement, (programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1] as CreditResult).Credit.ReplacementStatus);
                Assert.AreEqual("124", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1].GetAcadCredId());

                Assert.AreEqual("2014/FA", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[2].GetTermCode());
                Assert.AreEqual("7435", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[2].GetCourse().Id);
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[2].Result);
                Assert.AreEqual(ReplacedStatus.ReplaceInProgress, (programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[2] as CourseResult).PlannedCourse.ReplacedStatus);
                Assert.AreEqual(null, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[2].GetAcadCredId());

                Assert.AreEqual("2016/FA", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[3].GetTermCode());
                Assert.AreEqual("7435", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[3].GetCourse().Id);
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[3].Result);
                Assert.AreEqual(ReplacedStatus.ReplaceInProgress, (programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[3] as CourseResult).PlannedCourse.ReplacedStatus);
                Assert.AreEqual(null, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[3].GetAcadCredId());

                Assert.AreEqual("2029/FA", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[4].GetTermCode());
                Assert.AreEqual("7435", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[4].GetCourse().Id);
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[4].Result);
                Assert.AreEqual(ReplacedStatus.ReplaceInProgress, (programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[4] as CourseResult).PlannedCourse.ReplacedStatus);

                //second group result
                Assert.AreEqual(5, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results.Count);
                Assert.AreEqual(1, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].GetAppliedAndPlannedApplied().Count());
                Assert.AreEqual("2018/FA", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[0].GetTermCode());
                Assert.AreEqual("7435", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[0].GetCourse().Id);
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[0].Result);
                Assert.AreEqual(ReplacedStatus.ReplaceInProgress, (programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[0] as CreditResult).Credit.ReplacedStatus);
                Assert.AreEqual("123", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[0].GetAcadCredId());

                Assert.AreEqual("2019/SP", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[1].GetTermCode());
                Assert.AreEqual("7435", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[1].GetCourse().Id);
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[1].Result);
                Assert.AreEqual(ReplacementStatus.PossibleReplacement, (programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[1] as CreditResult).Credit.ReplacementStatus);
                Assert.AreEqual("124", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[1].GetAcadCredId());

                Assert.AreEqual("2014/FA", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[2].GetTermCode());
                Assert.AreEqual("7435", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[2].GetCourse().Id);
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[2].Result);
                Assert.AreEqual(ReplacedStatus.ReplaceInProgress, (programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[2] as CourseResult).PlannedCourse.ReplacedStatus);
                Assert.AreEqual(null, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[2].GetAcadCredId());

                Assert.AreEqual("2016/FA", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[3].GetTermCode());
                Assert.AreEqual("7435", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[3].GetCourse().Id);
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[3].Result);
                Assert.AreEqual(ReplacedStatus.ReplaceInProgress, (programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[3] as CourseResult).PlannedCourse.ReplacedStatus);
                Assert.AreEqual(null, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[3].GetAcadCredId());

                Assert.AreEqual("2029/FA", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[4].GetTermCode());
                Assert.AreEqual("7435", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[4].GetCourse().Id);
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[4].Result);
                Assert.AreEqual(ReplacedStatus.ReplaceInProgress, (programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[4] as CourseResult).PlannedCourse.ReplacedStatus);

                //third group result
                Assert.AreEqual(5, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results.Count);
                Assert.AreEqual(1, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].GetAppliedAndPlannedApplied().Count());
                Assert.AreEqual("2018/FA", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[0].GetTermCode());
                Assert.AreEqual("7435", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[0].GetCourse().Id);
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[0].Result);
                Assert.AreEqual(ReplacedStatus.ReplaceInProgress, (programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[0] as CreditResult).Credit.ReplacedStatus);
                Assert.AreEqual("123", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[0].GetAcadCredId());

                Assert.AreEqual("2019/SP", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[1].GetTermCode());
                Assert.AreEqual("7435", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[1].GetCourse().Id);
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[1].Result);
                Assert.AreEqual(ReplacementStatus.PossibleReplacement, (programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[1] as CreditResult).Credit.ReplacementStatus);
                Assert.AreEqual("124", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[1].GetAcadCredId());

                Assert.AreEqual("2014/FA", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[2].GetTermCode());
                Assert.AreEqual("7435", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[2].GetCourse().Id);
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[2].Result);
                Assert.AreEqual(ReplacedStatus.ReplaceInProgress, (programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[2] as CourseResult).PlannedCourse.ReplacedStatus);
                Assert.AreEqual(null, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[2].GetAcadCredId());

                Assert.AreEqual("2016/FA", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[3].GetTermCode());
                Assert.AreEqual("7435", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[3].GetCourse().Id);
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[3].Result);
                Assert.AreEqual(ReplacedStatus.ReplaceInProgress, (programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[3] as CourseResult).PlannedCourse.ReplacedStatus);
                Assert.AreEqual(null, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[3].GetAcadCredId());

                Assert.AreEqual("2029/FA", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[4].GetTermCode());
                Assert.AreEqual("7435", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[4].GetCourse().Id);
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[4].Result);
                Assert.AreEqual(ReplacedStatus.ReplaceInProgress, (programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[4] as CourseResult).PlannedCourse.ReplacedStatus);
            }

            [TestMethod]
            public async Task Evaluate_CumulativeGPA_DoNotCountReplaceInProgressCompletedCourses_ApplyRepeatedFlagAsY()
            {
                string studentid = "0016287";
                GetCredits(new List<string>() { studentid });

                //these student acadCredits are retaken but will replace and count only one because this course have retakeforcredits flag as N; 
                //degree parameters says exclude repeated completed courses for cum gpa
                //111 is 2009/sp MATH-3300BB credit course- This is inprogress
                //113 is 2010/sp math-300bb credit course- //This is complete graded this will be replaced by collegue
                //115 is 2017/sp math-300bb credit course // this is complete- graded
                creditDictionary[studentid][0].RepeatAcademicCreditIds = new List<string>() { "111", "113", "115" };
                creditDictionary[studentid][1].RepeatAcademicCreditIds = new List<string>() { "111", "113", "115" };
                creditDictionary[studentid][2].RepeatAcademicCreditIds = new List<string>() { "111", "113", "115" };
                creditDictionary[studentid][0].CanBeReplaced = true;
                creditDictionary[studentid][1].CanBeReplaced = true;
                creditDictionary[studentid][2].CanBeReplaced = true;
                creditDictionary[studentid][1].ReplacedStatus = ReplacedStatus.Replaced;
                creditDictionary[studentid][1].AdjustedCredit = 0m;
                //adjusted credit for IP is null
                creditDictionary[studentid][0].AdjustedCredit = null;
                creditDictionary[studentid][0].EndDate = null;
                //For 115 completed acad credit GPA points and credits will be adjusted to 0 since there is another inprogress course that would take the precedence 
                creditDictionary[studentid][2].StartDate = new DateTime(2017, 03, 01);

                var student = studentRepo.Get(studentid);
                var planningStudent = new Domain.Student.Entities.PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
                planningStudent.Advisements = student.Advisements;
                planningStudentRepoMock.Setup(psr => psr.GetAsync(studentid, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));

                academicCreditRepoMock.Setup(repo => repo.GetAcademicCreditByStudentIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns((IEnumerable<string> s, bool b1, bool b2, bool b3) => Task.FromResult(creditDictionary));
                requirementRepoMock.Setup(repo => repo.GetDegreeAuditParametersAsync()).Returns(Task.FromResult(new DegreeAuditParameters(ExtraCourses.Apply, false, false, false, true, true)));
                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo, catalogRepo, planningConfigRepo,
                    referenceDataRepo, currentUserFactory, roleRepository, logger, configRepo);

                var programResult = (await programEvaluationService.EvaluateAsync(studentid, new List<string>() { "REPEAT.BB" }, "2012")).First();
                Assert.IsNotNull(programResult);
                //since these course are replacing each other and a flag on course math-300bb says do not retake for credits
                //total inprogress credits will be 3 ; completed credits are 0; 

                Assert.AreEqual(3, programResult.InProgressCredits);
                Assert.AreEqual(0, programResult.Credits);
                Assert.AreEqual(2, programResult.OtherAcademicCredits.Count);
                Assert.AreEqual(4m, programResult.CumGpa);
                Assert.AreEqual(4m, programResult.InstGpa);
                //other courses
                Assert.AreEqual("113", programResult.OtherAcademicCredits[0]);
                Assert.AreEqual("115", programResult.OtherAcademicCredits[1]);
                //first group result
                Assert.AreEqual("113", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].GetAcadCredId());
                Assert.AreEqual(Result.ReplacedWithGPAValues, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].Result);
                Assert.AreEqual("115", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1].GetAcadCredId());
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1].Result);
                Assert.AreEqual("111", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[2].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[2].Result);
                //second group result
                Assert.AreEqual("113", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[0].GetAcadCredId());
                Assert.AreEqual(Result.ReplacedWithGPAValues, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[0].Result);
                Assert.AreEqual("115", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[1].GetAcadCredId());
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[1].Result);
                Assert.AreEqual("111", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[2].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[2].Result);
                //third group result
                Assert.AreEqual("113", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[0].GetAcadCredId());
                Assert.AreEqual(Result.ReplacedWithGPAValues, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[0].Result);
                Assert.AreEqual("115", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[1].GetAcadCredId());
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[1].Result);
                Assert.AreEqual("111", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[2].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[2].Result);
            }

            [TestMethod]
            public async Task Evaluate_CumulativeGPA_CountReplaceInProgressCompletedCourses_ApplyRepeatedFlagAsY()
            {
                string studentid = "0016287";
                GetCredits(new List<string>() { studentid });

                //these student acadCredits are retaken but will replace and count only one because this course have retakeforcredits flag as N
                //111 is 2009/sp MATH-3300BB credit course- This is inprogress
                //113 is 2010/sp math-300bb credit course- //This is complete graded this will be replaced by collegue
                //115 is 2017/sp math-300bb credit course // this is complete- graded
                creditDictionary[studentid][0].RepeatAcademicCreditIds = new List<string>() { "111", "113", "115" };
                creditDictionary[studentid][1].RepeatAcademicCreditIds = new List<string>() { "111", "113", "115" };
                creditDictionary[studentid][2].RepeatAcademicCreditIds = new List<string>() { "111", "113", "115" };
                creditDictionary[studentid][0].CanBeReplaced = true;
                creditDictionary[studentid][1].CanBeReplaced = true;
                creditDictionary[studentid][2].CanBeReplaced = true;
                creditDictionary[studentid][1].ReplacedStatus = ReplacedStatus.Replaced;
                creditDictionary[studentid][1].AdjustedCredit = 0m;
                //adjusted credit for IP is null
                creditDictionary[studentid][0].AdjustedCredit = null;
                creditDictionary[studentid][0].EndDate = null;
                creditDictionary[studentid][2].StartDate = new DateTime(2017, 03, 01);

                var student = studentRepo.Get(studentid);
                var planningStudent = new Domain.Student.Entities.PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
                planningStudent.Advisements = student.Advisements;
                planningStudentRepoMock.Setup(psr => psr.GetAsync(studentid, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));

                academicCreditRepoMock.Setup(repo => repo.GetAcademicCreditByStudentIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns((IEnumerable<string> s, bool b1, bool b2, bool b3) => Task.FromResult(creditDictionary));
                requirementRepoMock.Setup(repo => repo.GetDegreeAuditParametersAsync()).Returns(Task.FromResult(new DegreeAuditParameters(ExtraCourses.Apply, false, false, false, false, true)));
                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo, catalogRepo, planningConfigRepo,
                    referenceDataRepo, currentUserFactory, roleRepository, logger, configRepo);

                var programResult = (await programEvaluationService.EvaluateAsync(studentid, new List<string>() { "REPEAT.BB" }, "2012")).First();
                Assert.IsNotNull(programResult);

                Assert.AreEqual(3, programResult.InProgressCredits);
                Assert.AreEqual(0, programResult.Credits);
                Assert.AreEqual(2, programResult.OtherAcademicCredits.Count);
                Assert.AreEqual(3.33m, Decimal.Round(programResult.CumGpa.Value, 2));
                Assert.AreEqual(3.33m, Decimal.Round(programResult.InstGpa.Value, 2));
                //other courses
                Assert.AreEqual("113", programResult.OtherAcademicCredits[0]);
                Assert.AreEqual("115", programResult.OtherAcademicCredits[1]);
                //first group result
                Assert.AreEqual("113", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].GetAcadCredId());
                Assert.AreEqual(Result.ReplacedWithGPAValues, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].Result);
                Assert.AreEqual("115", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1].GetAcadCredId());
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1].Result);
                Assert.AreEqual("111", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[2].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[2].Result);
                //second group result
                Assert.AreEqual("113", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[0].GetAcadCredId());
                Assert.AreEqual(Result.ReplacedWithGPAValues, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[0].Result);
                Assert.AreEqual("115", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[1].GetAcadCredId());
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[1].Result);
                Assert.AreEqual("111", programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[2].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results[2].Result);
                //third group result
                Assert.AreEqual("113", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[0].GetAcadCredId());
                Assert.AreEqual(Result.ReplacedWithGPAValues, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[0].Result);
                Assert.AreEqual("115", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[1].GetAcadCredId());
                Assert.AreEqual(Result.ReplaceInProgress, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[1].Result);
                Assert.AreEqual("111", programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[2].GetAcadCredId());
                Assert.AreEqual(Result.Applied, programResult.RequirementResults[0].SubRequirementResults[2].GroupResults[0].Results[2].Result);
            }

        }

        //Evaluate for transcript grouping

        //Evaluate credits that filter for transcript grouping like- subject, dept, schools, divisions
        //create a program that have creditFilters with list like subject, dept, all
        //create list of credits that are all

        [TestClass]
        public class ProgramEvaluationServiceTests_Evaluate_TranscriptGroupingFiltering : CurrentUserSetup
        {

            private IAdapterRegistry adapterRegistry;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IProgramRequirementsRepository programRequirementsRepo;
            private IStudentRepository studentRepo;
            private Mock<IPlanningStudentRepository> planningStudentRepoMock;
            private IPlanningStudentRepository planningStudentRepo;
            private Mock<IApplicantRepository> applicantRepoMock;
            private IApplicantRepository applicantRepo;
            private Mock<IStudentProgramRepository> studentProgramRepoMock;
            private IStudentProgramRepository studentProgramRepo;
            private Mock<IAcademicCreditRepository> academicCreditRepoMock;
            private IAcademicCreditRepository academicCreditRepo;
            private IAcademicCreditRepository academicCreditRepoInstance = new TestAcademicCreditRepository();
            private Mock<IStudentDegreePlanRepository> studentDegreePlanRepoMock;
            private IStudentDegreePlanRepository studentDegreePlanRepo;
            private ICourseRepository courseRepo;
            private IRequirementRepository requirementRepo;
            private Mock<IRequirementRepository> requirementRepoMock;
            private IRequirementRepository requirementRepoInstance = new TestRequirementRepository();
            private ITermRepository termRepo;
            private IRuleRepository ruleRepo;
            private IProgramRepository programRepo;
            private ICatalogRepository catalogRepo;
            private IPlanningConfigurationRepository planningConfigRepo;
            private IReferenceDataRepository referenceDataRepo;
            private IRoleRepository roleRepository;
            private Mock<IRoleRepository> roleRepoMock;
            private ProgramEvaluationService programEvaluationService;
            private Dumper dumper;
            private ILogger logger;
            private ICurrentUserFactory currentUserFactory;
            private Mock<IConfigurationRepository> configRepoMock;
            private IConfigurationRepository configRepo;
            private Dictionary<string, List<AcademicCredit>> creditDictionary = new Dictionary<string, List<AcademicCredit>>();
            private Dictionary<string, List<AcademicCredit>> GetCredits(IEnumerable<string> studentIds, bool includeDrops = false)
            {
                creditDictionary = new Dictionary<string, List<AcademicCredit>>();
                foreach (var id in studentIds)
                {
                    var student = studentRepo.Get(id);
                    var acadCreditIds = student.AcademicCreditIds;

                    var credits = academicCreditRepoInstance.GetAsync(acadCreditIds, includeDrops: includeDrops).Result.ToList();

                    creditDictionary.Add(id, credits);
                }

                return creditDictionary;
            }

            [TestInitialize]
            public void Initialize()
            {
                programRequirementsRepo = new TestProgramRequirementsRepository();
                studentRepo = new TestStudentRepository();
                planningStudentRepoMock = new Mock<IPlanningStudentRepository>();
                planningStudentRepo = planningStudentRepoMock.Object;
                applicantRepoMock = new Mock<IApplicantRepository>();
                applicantRepo = applicantRepoMock.Object;
                studentProgramRepo = new TestStudentProgramRepository();


                academicCreditRepoMock = new Mock<IAcademicCreditRepository>();
                academicCreditRepo = academicCreditRepoMock.Object;
                referenceDataRepo = new Mock<IReferenceDataRepository>().Object;

                studentDegreePlanRepoMock = new Mock<IStudentDegreePlanRepository>();
                studentProgramRepoMock = new Mock<IStudentProgramRepository>();

                studentDegreePlanRepo = new TestStudentDegreePlanRepository();
                courseRepo = new TestCourseRepository();
                requirementRepo = new TestRequirementRepository();
                requirementRepoMock = new Mock<IRequirementRepository>();
                requirementRepo = requirementRepoMock.Object;
                ruleRepo = new TestRuleRepository2();
                programRepo = new TestProgramRepository();
                catalogRepo = new TestCatalogRepository();
                planningConfigRepo = new TestPlanningConfigurationRepository();
                termRepo = new TestTermRepository();
                dumper = new Dumper();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                logger = new Mock<ILogger>().Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepository = roleRepoMock.Object;
                configRepoMock = new Mock<IConfigurationRepository>();
                configRepo = configRepoMock.Object;

                // Set up user as "AllAccessAnyAdvisee" advisor so that evaluation is permitted
                currentUserFactory = new AdvisorUserFactory();
                // Set up view any advisee permissions on advisor's role--so that advisor has access to this student
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { advisorRole });
            }

            [TestMethod]
            public async Task Transcript_Grouping_Filter_On_AcadLevel_GR()
            {
                //This student took 4 credits - 116,117,120,121
                //116, 117 are of UG level
                //only those credits should be part of program result
                string studentid = "0016298";
                GetCredits(new List<string>() { studentid }, true);
                var student = studentRepo.Get(studentid);
                var planningStudent = new Domain.Student.Entities.PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
                planningStudent.Advisements = student.Advisements;
                planningStudentRepoMock.Setup(psr => psr.GetAsync(studentid, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));

                academicCreditRepoMock.Setup(repo => repo.GetAcademicCreditByStudentIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns((IEnumerable<string> s, bool b1, bool b2, bool b3) => Task.FromResult(creditDictionary));
                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo, catalogRepo, planningConfigRepo,
                    referenceDataRepo, currentUserFactory, roleRepository, logger, configRepo);

                var programResult = (await programEvaluationService.EvaluateAsync(studentid, new List<string>() { "TRANSCRIPT.GROUPING.FILTER" }, null)).First();
                Assert.IsNotNull(programResult);
                Assert.AreEqual(2, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results.Count());
                Assert.AreEqual(0, programResult.OtherAcademicCredits.Count());
                Assert.AreEqual(0, programResult.OtherPlannedCredits.Count());
                Assert.AreEqual("117", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].GetAcadCredId());
                Assert.AreEqual("116", programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1].GetAcadCredId());

            }

            [TestMethod]
            public async Task Transcript_Grouping_Filter_On_Subject()
            {
                //This student took 4 credits - 116,117,120,121
                //120, 121 are of MATH level
                //only those credits should be part of program result
                string studentid = "0016299";
                GetCredits(new List<string>() { studentid }, true);
                var student = studentRepo.Get(studentid);
                var planningStudent = new Domain.Student.Entities.PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
                planningStudent.Advisements = student.Advisements;
                planningStudentRepoMock.Setup(psr => psr.GetAsync(studentid, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));

                academicCreditRepoMock.Setup(repo => repo.GetAcademicCreditByStudentIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns((IEnumerable<string> s, bool b1, bool b2, bool b3) => Task.FromResult(creditDictionary));
                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, applicantRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo, catalogRepo, planningConfigRepo,
                    referenceDataRepo, currentUserFactory, roleRepository, logger, configRepo);

                var programResult = (await programEvaluationService.EvaluateAsync(studentid, new List<string>() { "TRANSCRIPT.GROUPING.FILTER.SUBJECTS" }, null)).First();
                Assert.IsNotNull(programResult);
                Assert.AreEqual(0, programResult.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results.Count());
                Assert.AreEqual(2, programResult.OtherAcademicCredits.Count());
                Assert.AreEqual(0, programResult.OtherPlannedCredits.Count());
                Assert.AreEqual("120", programResult.OtherAcademicCredits[0]);
                Assert.AreEqual("121", programResult.OtherAcademicCredits[1]);
            }
        }

    }
}
