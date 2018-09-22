// RTM FIX
//using System.Linq;
//using Ellucian.Utility.Logging;
//using Ellucian.Utility.Adapters;
//using Ellucian.Web.Http.TestUtil;
//using Ellucian.Web.Student.Coordination.Adapters;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Moq;
//using Ellucian.Colleague.Domain.Student.Repositories;
//using Ellucian.Colleague.Coordination.Planning.Services;
//using Ellucian.Colleague.Domain.Planning.Repositories;
//using Ellucian.Colleague.Domain.Base.Repositories;

//namespace Ellucian.Colleague.Coordination.Student.Tests.Adapters
//{
//    [TestClass]
//    public class SubrequirementResultEntityToSubrequirementResultDtoAdapterTests
//    {

//        private IProgramRequirementsRepository programRequirementsRepo;
//        private IProgramEvaluationService programEvaluationService;
//        private IStudentRepository studentRepo;
//        private IStudentProgramRepository studentProgramRepo;
//        private IRequirementRepository requirementRepo;
//        private IAcademicCreditRepository acadCredRepo;
//        private IDegreePlanRepository degreePlanRepo;
//        private ICourseRepository courseRepo;
//        private ITermRepository termRepo;
//        private IEvaluationRuleRepository ruleRepo;
//        private IAdapterRegistry adapterRegistry;
        
//        private ILogger logger;
//        private string studentId = "0000001";

//        [TestInitialize]
//        // Test repositories for test data
//        public void Initialize(){
//                degreePlanRepo = new TestDegreePlanRepository();
//                studentRepo = new TestStudentRepository();
//                studentProgramRepo = new TestStudentProgramRepository();
//                programRequirementsRepo = new TestProgramRequirementsRepository();
//                acadCredRepo = new TestAcademicCreditRepository();
//                courseRepo = new TestCourseRepository();
//                requirementRepo = new TestRequirementRepository();
//                termRepo = new TestTermRepository();
//                ruleRepo = new TestRuleRepository();

//                // This won't work for additional requirements right now - TestRequirementRepo data doesn't match other data
//                programEvaluationService = new ProgramEvaluationService(adapterRegistry, programRequirementsRepo, studentRepo, studentProgramRepo,
//                                                                        requirementRepo, acadCredRepo, degreePlanRepo, courseRepo, termRepo, ruleRepo);

//                Mock<IAdapterRegistry> adapterRegistryMock = new Mock<IAdapterRegistry>();
//                adapterRegistry = adapterRegistryMock.Object;
//                logger = new Mock<ILogger>().Object;

//            }


//            [TestMethod]
//            public void SubrequirementResultDtoAdapter()
//            {

//                // default behavior of test prr is to create whatever programrequirements you specify with these requirements:
//                // "MATH-100"  satisfied by acad cred id 8
//                // "MATH-200"                           39
//                // "ENGL-101"                           36
//                // "ENGL-102"                           26      

//                studentId = "00004002";
//                Student.Domain.Entities.Student student = studentRepo.Get(studentId);

//                Student.Domain.Entities.Requirements.ProgramEvaluation progEval = programEvaluationService.Evaluate(studentId, student.ProgramIds.First());

//                SubrequirementResultDtoAdapter adapter = new SubrequirementResultDtoAdapter(adapterRegistry, logger);

//                Student.Domain.Entities.Requirements.SubrequirementResult source = progEval.RequirementResults.First().SubRequirementResults.First();
//                Requirements.SubrequirementResult target = adapter.MapToType(source);
                
//                Assert.IsNotNull(target);

//               // Assert.AreEqual(source.IsSatisfied(), target.IsSatisfied);
//                Assert.AreEqual(source.SubRequirement.Id, target.SubrequirementId);
//                Assert.AreEqual(source.GroupResults.Count, target.GroupResults.Count);
//            }
//    }
//}
