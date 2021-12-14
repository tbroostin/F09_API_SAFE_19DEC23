// Copyright 2013-2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Web.Http.TestUtil;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Services;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Entities.DegreePlans;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using Ellucian.Colleague.Domain.Student.Entities.Requirements.Modifications;

namespace Ellucian.Colleague.Domain.Student.Tests.Services
{
    [TestClass]
    public class ProgramEvaluationTests
    {
        private TestContext testContextInstance;

        public TestContext TestContext
        { get { return testContextInstance; } set { testContextInstance = value; } }

        private Mock<IProgramRequirementsRepository> programRequirementsRepoMock;
        private IProgramRequirementsRepository programRequirementsRepoMk;           // for when we want to mock up specific requirements
        private IProgramRequirementsRepository programRequirementsRepo;             // for when we don't
        private IStudentRepository studentRepo;
        private IStudentProgramRepository studentProgramRepo;
        private IAcademicCreditRepository academicCreditRepo;
        private IStudentDegreePlanRepository studentDegreePlanRepo;
        private ICourseRepository courseRepo;
        private IRequirementRepository requirementRepo;
        private TestGradeRepository graderepo;
        private Dumper dumper;

        public List<PlannedCredit> plannedcourses;
        public ProgramRequirements programrequirements;
        public StudentProgram studentprogram;
        public List<Requirement> additionalrequirements;
        public List<AcademicCredit> credits;
        public List<Override> overrides;
        public List<AcadResult> results;
        public IEnumerable<RuleResult> ruleResults;
        public IEnumerable<Course> courses;
        public List<RequirementModification> requirementModifications;
        public string studentid;
        public const string programid = "MATH.BS";  // the test repo is ignoring this

        // For building specific requirement trees
        public TestProgramRequirementsRepository tprr;
        public IRuleRepository trr;

        public List<string> reqnames;
        public List<string> subnames;
        public List<string> grpnames;
        public Dictionary<string, List<string>> Subreq;
        public Dictionary<string, List<string>> groups;

        public List<Override> overs;
        public List<AcadResult> acadresults;
        private ILogger logger;

        [TestInitialize]
        public async void Initialize()
        {
            programRequirementsRepoMock = new Mock<IProgramRequirementsRepository>();
            programRequirementsRepoMk = programRequirementsRepoMock.Object;

            studentRepo = new TestStudentRepository();
            studentProgramRepo = new TestStudentProgramRepository();
            academicCreditRepo = new TestAcademicCreditRepository();
            studentDegreePlanRepo = new TestStudentDegreePlanRepository();
            courseRepo = new TestCourseRepository();
            requirementRepo = new TestRequirementRepository();
            programRequirementsRepo = new TestProgramRequirementsRepository();
            graderepo = new TestGradeRepository();
            tprr = new TestProgramRequirementsRepository(); //need non-interface method for building particular groups
            trr = new TestRuleRepository2();

            reqnames = new List<string>();
            subnames = new List<string>();
            grpnames = new List<string>();
            Subreq = new Dictionary<string, List<string>>();
            groups = new Dictionary<string, List<string>>();

            ruleResults = new List<RuleResult>();
            courses = await new TestCourseRepository().GetAsync();
            dumper = new Dumper();
            logger = new Mock<ILogger>().Object;
        }

        [TestMethod]
        public async Task TestEvaluate()
        {

            studentid = "00004005";  //no plan, credits include MATH-100 and ENGL-101
            programrequirements = await GetRequirementsAsync();
            studentprogram = await GetStudentProgram(studentid, programid);
            additionalrequirements = new List<Requirement>();
            plannedcourses = await GetCourses(studentid);
            credits = await GetCredits(studentid);

            overrides = await GetOverrides(studentid, programid);


            ProgramEvaluation eval = new ProgramEvaluator(studentprogram, programrequirements, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();


            dumper.Dump(eval, "verbose");
            Assert.IsNotNull(eval);
            Assert.IsTrue(eval.IsSatisfied);
            //Assert.IsTrue(progresult.RequirementResults.First().SubRequirementResults.First().GroupResults.First().GetApplied()
            //                            .Select(g => g.GetCourse().ToString()).Contains("MATH*100"));
            //Assert.IsTrue(progresult.RequirementResults.First().SubRequirementResults.First().GroupResults.First().GetApplied()
            //                            .Select(g => g.GetCourse().ToString()).Contains("ENGL*101"));

            Assert.IsTrue(eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First().GetApplied().Count() == 2);



        }


        [TestMethod]
        public async Task TestEvaluateWithPlan()
        {                                   //       planned                credit
            string studentid = "00004002";  // MATH-100, MATH-200       ENGL-101, ENGL-102
            programrequirements = await GetRequirementsAsync();
            studentprogram = await GetStudentProgram(studentid, programid);
            additionalrequirements = new List<Requirement>();
            plannedcourses = await GetCourses(studentid);
            credits = await GetCredits(studentid);
            overrides = await GetOverrides(studentid, programid);

            ProgramEvaluation eval = new ProgramEvaluator(studentprogram, programrequirements, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();
            dumper.Dump(eval, "verbose");

            Assert.IsTrue(eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First().GetPlannedApplied()
                                                    .Select(g => g.GetCourse().ToString()).Contains("MATH*100"));

        }

        [TestMethod]
        public async Task TestEvaluateWithPlan_AllowedGrades_not_null()
        {                                   //       planned                credit
            string studentid = "00004002";  // MATH-100, MATH-200       ENGL-101, ENGL-102
            programrequirements = await GetRequirementsAsync();
            programrequirements.AllowedGrades = new List<Grade>()
                {
                    (await graderepo.GetAsync()).Where(g => g.Id == "A").First()
                };
            studentprogram = await GetStudentProgram(studentid, programid);
            additionalrequirements = new List<Requirement>();
            plannedcourses = await GetCourses(studentid);
            credits = await GetCredits(studentid);
            overrides = await GetOverrides(studentid, programid);

            ProgramEvaluation eval = new ProgramEvaluator(studentprogram, programrequirements, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();
            dumper.Dump(eval, "verbose");

            Assert.IsTrue(eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First().GetPlannedApplied()
                                                    .Select(g => g.GetCourse().ToString()).Contains("MATH*100"));
        }

        [TestMethod]
        public async Task TestEvaluate_PlannedCredits()
        {
            //       planned                credit
            // MATH-100, MATH-200       ENGL-101, ENGL-102
            string studentid = "00004002";
            programrequirements = await GetRequirementsAsync();
            studentprogram = await GetStudentProgram(studentid, programid);
            additionalrequirements = new List<Requirement>();
            plannedcourses = new List<PlannedCredit>();

            // Set up a planned course with variable credit
            var crs1 = new Course("MATH-200", "Short Title", "Long Title", new List<OfferingDepartment>() { new OfferingDepartment("MATH") }, "MATH", "200", "UG", new List<string>() { "Level1" }, 2.0m, null, new List<CourseApproval>() { new CourseApproval("A", DateTime.Today, "1111", "1111", DateTime.Today) });
            crs1.MaximumCredits = 9.0m;
            crs1.VariableCreditIncrement = 1.0m;
            var evalPlannedCourse1 = new PlannedCredit(crs1, "2017/SP");
            evalPlannedCourse1.Credits = 4.0m;
            plannedcourses.Add(evalPlannedCourse1);

            // Set up a planned course with CEU's only
            var crs2 = new Course("MATH-200", "Short Title", "Long Title", new List<OfferingDepartment>() { new OfferingDepartment("MATH") }, "MATH", "200", "UG", new List<string>() { "Level1" }, null, 3.0m, new List<CourseApproval>() { new CourseApproval("A", DateTime.Today, "1111", "1111", DateTime.Today) });
            var evalPlannedCourse2 = new PlannedCredit(crs2, "2017/SP");
            plannedcourses.Add(evalPlannedCourse2);

            // Set up a planned course with min credits only
            var crs3 = new Course("MATH-300", "Short Title", "Long Title", new List<OfferingDepartment>() { new OfferingDepartment("MATH") }, "MATH", "300", "UG", new List<string>() { "Level1" }, 3.0m, null, new List<CourseApproval>() { new CourseApproval("A", DateTime.Today, "1111", "1111", DateTime.Today) });
            var evalPlannedCourse3 = new PlannedCredit(crs3, "2017/SP");
            plannedcourses.Add(evalPlannedCourse3);

            credits = await GetCredits(studentid);
            overrides = await GetOverrides(studentid, programid);

            ProgramEvaluation eval = new ProgramEvaluator(studentprogram, programrequirements, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();
            dumper.Dump(eval, "verbose");

            Assert.AreEqual(7.0m, eval.PlannedCredits);
        }

        [TestMethod]
        public async Task TestEvaluateWithInclusionOverride()
        {
            //       planned                credit
            string studentid = "00004006";  //        (none)         ENGL-102 (ungraded), MATH-460 (A)
            programrequirements = await GetRequirementsAsync();
            studentprogram = await GetStudentProgram(studentid, programid);
            additionalrequirements = new List<Requirement>();
            plannedcourses = await GetCourses(studentid);
            credits = await GetCredits(studentid);
            overrides = new List<Override>();
            overrides.Add(new Override("10055", new List<string>() { "36" }, new List<string>()));  // allow ENGL-101 to apply to group 55, which would
            // normally not allow that credit because the grade is
            // not A, AU, or P.
            ProgramEvaluation eval = new ProgramEvaluator(studentprogram, programrequirements, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();
            dumper.Dump(eval, "verbose");

            Assert.IsTrue(eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First().GetApplied()
                                                    .Select(g => g.GetCourse().ToString()).Contains("ENGL*101"));


            Assert.IsTrue(eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First().GetApplied()
                                                    .Select(g => g.GetCourse().ToString()).Contains("MATH*460"));
        }


        [TestMethod]
        public async Task TestEvaluateWithInclusionOverrideForCourseExcludedFromTranscriptGrouping_Constructor_withNoTransGrouping()
        {
            //Program have academic level of UG and Transcript grouping of UG
            //one of the acad credit is from acad level GR
            //This course will be filtered out from tanscript grouping but Override takes the same course of type GR
            //Result: Override course is applied even though it is initially filtered out from transcript grouing acad level mismatch
            //Example - take 2 courses from level 100, 200; - this should not apply MATH-502 since constructor is not taking a list of courses
            //excluded from transcript grouping

            //       planned                credit
            string studentid = "0000112";  //        (none)         ENGL-102 (ungraded), MATH-460 (A), MATH-502
            ProgramRequirements pr = await programRequirementsRepo.GetAsync("OVERRIDE.WITH.TRANSCRIPT.GROUPING", "2013");
            studentprogram = await GetStudentProgram(studentid, "OVERRIDE.WITH.TRANSCRIPT.GROUPING");
            additionalrequirements = new List<Requirement>();
            plannedcourses = await GetCourses(studentid);
            credits = await GetCredits(studentid);
            overrides = new List<Override>();
            overrides.Add(new Override("10013", new List<string>() { "57" }, new List<string>()));  // allow MATH-502 to apply to group 55, which would
            // normally not allow that credit because the grade is
            // not A, AU, or P.
            ProgramEvaluation eval = new ProgramEvaluator(studentprogram, pr, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();
            dumper.Dump(eval, "verbose");

            Assert.IsFalse(eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First().GetApplied()
                                                    .Select(g => g.GetCourse().ToString()).Contains("MATH*502"));


            Assert.IsTrue(eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First().GetApplied()
                                                    .Select(g => g.GetCourse().ToString()).Contains("MATH*460"));
        }

        [TestMethod]
        public async Task TestEvaluateWithInclusionOverrideForCourseExcludedFromTranscriptGrouping()
        {
            //Program have academic level of UG and Transcript grouping of UG
            //one of the acad credit is from acad level GR
            //This course will be filtered out from tanscript grouping but Override takes the same course of type GR
            //Result: Override course is applied even though it is initially filtered out from transcript grouing acad level mismatch
            //Example - take 2 courses from level 100, 200; - this should apply MATH-502 even though the course is grom GR level and complete the group

            //       planned                credit
            string studentid = "0000112";  //        (none)         ENGL-102 (ungraded), MATH-460 (A), MATH-502
            var crse = await courseRepo.GetAsync("9878");
            ProgramRequirements pr = await programRequirementsRepo.GetAsync("OVERRIDE.WITH.TRANSCRIPT.GROUPING", "2013");
            studentprogram = await GetStudentProgram(studentid, "OVERRIDE.WITH.TRANSCRIPT.GROUPING");
            additionalrequirements = new List<Requirement>();
            plannedcourses = await GetCourses(studentid);
            credits = await GetCredits(studentid);
            overrides = new List<Override>();
            overrides.Add(new Override("10013", new List<string>() { "57" }, new List<string>()));  // allow MATH-502 to apply to group 55, which would
            var coursesExcludedFromTranscriptGrouping = new List<AcademicCredit>();
            List<AcademicCredit> acadcreds = (await academicCreditRepo.GetAsync(new List<string>() { "57" })).ToList();
            coursesExcludedFromTranscriptGrouping.Add(acadcreds[0]);
            // normally not allow that credit because the grade is
            // not A, AU, or P.
            ProgramEvaluation eval = new ProgramEvaluator(studentprogram, pr, additionalrequirements, credits, plannedcourses, ruleResults, overrides, coursesExcludedFromTranscriptGrouping, courses, new DegreeAuditParameters(ExtraCourses.Display), logger).Evaluate();
            dumper.Dump(eval, "verbose");

            Assert.IsTrue(eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First().GetApplied()
                                                    .Select(g => g.GetCourse().ToString()).Contains("MATH*502"));


            Assert.IsFalse(eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First().GetApplied()
                                                    .Select(g => g.GetCourse().ToString()).Contains("MATH*460"));
        }

        [TestMethod]
        public async Task TestEvaluateWithExclusionOverride()
        {
            //       planned                credit
            string studentid = "00004007";  //        (none)         MATH-201, MATH-152 (two "A"s))
            programrequirements = await GetRequirementsAsync();
            studentprogram = await GetStudentProgram(studentid, programid);
            additionalrequirements = new List<Requirement>();
            plannedcourses = await GetCourses(studentid);
            credits = await GetCredits(studentid);
            overrides = new List<Override>();
            overrides.Add(new Override("10055", new List<string>(), new List<string>() { "6" }));  // disallow MATH-152 from group 55, which would
            // normally  allow that credit because the grade is
            // an A
            ProgramEvaluation eval = new ProgramEvaluator(studentprogram, programrequirements, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();
            dumper.Dump(eval, "verbose");
            Assert.IsFalse(eval.IsSatisfied);

            Assert.IsTrue(eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First().GetApplied()
                                                    .Select(g => g.GetCourse().ToString()).Contains("MATH*201"));


            Assert.IsFalse(eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First().GetApplied()
                                                    .Select(g => g.GetCourse().ToString()).Contains("MATH*152"));
        }

        [TestMethod]
        public async Task TestEvaluateWithInstitutionalCreditModification()
        {
            //       planned                credit
            string studentid = "00004007";  //        (none)         MATH-201, MATH-152 (two "A"s))
            programrequirements = await GetRequirementsAsync();
            studentprogram = await GetStudentProgram(studentid, programid);
            additionalrequirements = new List<Requirement>();
            plannedcourses = await GetCourses(studentid);
            credits = await GetCredits(studentid);
            overrides = new List<Override>();


            Assert.AreNotEqual(90m, programrequirements.MinimumInstitutionalCredits);

            InstitutionalCreditModification icm = new InstitutionalCreditModification(null, 90m, "");
            studentprogram.AddRequirementModification(icm);

            ProgramEvaluation eval = new ProgramEvaluator(studentprogram, programrequirements, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();


            Assert.AreEqual(90m, eval.ProgramRequirements.MinimumInstitutionalCredits);

        }
        [TestMethod]
        public async Task TestEvaluateWithCourseWaiver()
        {
            //       planned                credit
            string studentid = "00004002";    // MATH-100, MATH-200       ENGL-101, ENGL-102
            programrequirements = await GetRequirementsAsync("Test3");  //requires ENGL-101 and ENGL-102
            studentprogram = await GetStudentProgram(studentid, programid);
            additionalrequirements = new List<Requirement>();
            plannedcourses = await GetCourses(studentid);
            credits = await GetCredits(studentid);
            overrides = new List<Override>();

            CourseWaiver cw = new CourseWaiver("10003", new List<string>() { "130" }, "");  //130, 187 are the two courses 
            studentprogram.AddRequirementModification(cw);

            ProgramEvaluation eval = new ProgramEvaluator(studentprogram, programrequirements, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();
            dumper.Dump(eval, "Verbose");

            Assert.IsFalse(eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First().GetApplied()
                                                    .Select(g => g.GetCourse().ToString()).Contains("ENGL*101"));


            Assert.IsTrue(eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First().GetApplied()
                                                    .Select(g => g.GetCourse().ToString()).Contains("ENGL*102"));

        }

        [TestMethod]
        public async Task TestEvaluateWithRequirementAcademicCreditRule()
        {
            //       planned                credit
            string studentid = "00004002";    // MATH-100, MATH-200       ENGL-101, ENGL-102
            programrequirements = await GetRequirementsAsync("Test3");  //requires ENGL-101 and ENGL-102
            studentprogram = await GetStudentProgram(studentid, programid);
            additionalrequirements = new List<Requirement>();
            plannedcourses = await GetCourses(studentid);
            credits = await GetCredits(studentid);
            overrides = new List<Override>();

            // Add the rule
            var rule = new RequirementRule(new Rule<AcademicCredit>("MATH100"));
            programrequirements.Requirements.First().AcademicCreditRules.Add(rule);  // the rule requires math100, the other parts
            // of the requirement require english.  nothing will ever
            // satisfy this, but it's a good test.
            // get a EvaluationRuleResult that evaluates these results against the test rules 
            // "MATH100" is one of the test rules.
            var ruleRequests = new List<RuleRequest<AcademicCredit>>();
            foreach (var credit in credits)
            {
                ruleRequests.Add(new RuleRequest<AcademicCredit>(rule.CreditRule, credit));
            }
            ruleResults = new TestRuleRepository2().Execute(ruleRequests);

            ProgramEvaluation eval = new ProgramEvaluator(studentprogram, programrequirements, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();
            dumper.Dump(eval, "Verbose");

            Assert.IsFalse(eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First().GetApplied()
                                                    .Select(g => g.GetCourse().ToString()).Contains("ENGL*101"));


            Assert.IsFalse(eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First().GetApplied()
                                                    .Select(g => g.GetCourse().ToString()).Contains("ENGL*102"));

        }

        [TestMethod]
        public async Task TestEvaluateWithPlanSettingStatus()
        {                                   //       planned                credit
            string studentid = "00004002";  // MATH-100, MATH-200       ENGL-101, ENGL-102
            programrequirements = await GetRequirementsAsync();
            studentprogram = await GetStudentProgram(studentid, programid);
            additionalrequirements = new List<Requirement>();
            plannedcourses = await GetCourses(studentid);
            credits = await GetCredits(studentid);

            overrides = await GetOverrides(studentid, programid);

            ProgramEvaluation eval = new ProgramEvaluator(studentprogram, programrequirements, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();
            dumper.Dump(eval, "verbose");

            // because this was satisfied, regardless of the fact that the planned courses are not all needed, this 
            // is "completely planned."
            var gr = eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First();
            Assert.IsTrue(gr.PlanningStatus == PlanningStatus.CompletelyPlanned);

            // partially satisfied by a planned course, so the eval is satisfied, but the actual completion status is not complete.
            // Since one of the academic credits has a grade below minimum and the other is in progress but not graded (and therefore there are no completed credits)
            // This really is Not Started. 
            Assert.IsTrue(gr.CompletionStatus == CompletionStatus.NotStarted);

        }
        [TestMethod]
        public async Task SettingCompletion()
        {
            // The two acad creds complete the group and set the group to 'completely planned' even though it's really not planned.

            //       planned                credit
            string studentid = "00004002";    // MATH-100, MATH-200       ENGL-101, ENGL-102
            programrequirements = await GetRequirementsAsync("Test3");  //requires ENGL-101 and ENGL-102
            studentprogram = await GetStudentProgram(studentid, programid);
            additionalrequirements = new List<Requirement>();
            plannedcourses = await GetCourses(studentid);
            credits = await GetCredits(studentid);
            //36 take grade from 26
            // Since this test was written the code was changed to change the completeness of in-progrss courses. 
            // We'll give the in-progress course a grade so it evaluates as it did before for this test.
            var credit1 = credits.First(crd => crd.Id == "36");
            credit1.VerifiedGrade = credits.First(crd => crd.Id == "26").VerifiedGrade;


            overrides = new List<Override>();
            ProgramEvaluation eval = new ProgramEvaluator(studentprogram, programrequirements, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();
            dumper.Dump(eval, "Verbose");

            Assert.IsTrue(eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First().CompletionStatus == CompletionStatus.Completed);
            Assert.IsTrue(eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First().PlanningStatus == PlanningStatus.CompletelyPlanned);

            Assert.IsTrue(eval.RequirementResults.First().SubRequirementResults.First().CompletionStatus == CompletionStatus.Completed);
            Assert.IsTrue(eval.RequirementResults.First().SubRequirementResults.First().PlanningStatus == PlanningStatus.CompletelyPlanned);

            Assert.IsTrue(eval.RequirementResults.First().CompletionStatus == CompletionStatus.Completed);
            Assert.IsTrue(eval.RequirementResults.First().PlanningStatus == PlanningStatus.CompletelyPlanned);
        }

        [TestMethod]
        public async Task SettingCompletionForPlan()
        {
            // Credit doesn't apply, plan covers this group

            //       planned                credit
            string studentid = "00004002";     // MATH-100, MATH-200       ENGL-101, ENGL-102
            programrequirements = await GetRequirementsAsync("Test6");  //requires one MATH course
            studentprogram = await GetStudentProgram(studentid, programid);
            additionalrequirements = new List<Requirement>();
            plannedcourses = await GetCourses(studentid);
            credits = await GetCredits(studentid);
            overrides = new List<Override>();
            ProgramEvaluation eval = new ProgramEvaluator(studentprogram, programrequirements, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();
            dumper.Dump(eval, "Verbose");

            var req = eval.RequirementResults.First();
            var sub = req.SubRequirementResults.First();
            var gr = sub.GroupResults.First();

            Assert.IsTrue(gr.CompletionStatus == CompletionStatus.NotStarted);
            Assert.IsTrue(gr.PlanningStatus == PlanningStatus.CompletelyPlanned);

            Assert.IsTrue(sub.CompletionStatus == CompletionStatus.NotStarted);
            Assert.IsTrue(sub.PlanningStatus == PlanningStatus.CompletelyPlanned);

            Assert.IsTrue(req.CompletionStatus == CompletionStatus.NotStarted);
            Assert.IsTrue(req.PlanningStatus == PlanningStatus.CompletelyPlanned);
        }

        [TestMethod]
        public async Task SettingCompletionWithOverrideCourses_ApplyOverrideWhenListedInCourses()
        {
            //       planned                credit
            string studentid = "00004002";    // MATH-100, MATH-200       ENGL-101, ENGL-102
            programrequirements = await GetRequirementsAsync("Test3");  //requires ENGL-101 and ENGL-102
            studentprogram = await GetStudentProgram(studentid, programid);
            additionalrequirements = new List<Requirement>();
            plannedcourses = await GetCourses(studentid);
            //just apply ENGL-102(26) take ENGL-101(36) is override
            credits = (await GetCredits(studentid));
            var credit1 = credits.First(crd => crd.Id == "36");
            credit1.VerifiedGrade = credits.First(crd => crd.Id == "26").VerifiedGrade;

            studentprogram.AddOverride(new Override("10003", new List<string>() { "36" }, null));
            overrides = new List<Override>() { new Override("10003", new List<string>() { "36" }, null) };
            ProgramEvaluation eval = new ProgramEvaluator(studentprogram, programrequirements, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();
            dumper.Dump(eval, "Verbose");

            Assert.IsTrue(eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First().CompletionStatus == CompletionStatus.Completed);
            Assert.IsTrue(eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First().PlanningStatus == PlanningStatus.CompletelyPlanned);

            Assert.IsTrue(eval.RequirementResults.First().SubRequirementResults.First().CompletionStatus == CompletionStatus.Completed);
            Assert.IsTrue(eval.RequirementResults.First().SubRequirementResults.First().PlanningStatus == PlanningStatus.CompletelyPlanned);

            Assert.IsTrue(eval.RequirementResults.First().CompletionStatus == CompletionStatus.Completed);
            Assert.IsTrue(eval.RequirementResults.First().PlanningStatus == PlanningStatus.CompletelyPlanned);
        }

        [TestMethod]
        public async Task PlanNotStarted()
        {
            // Credit doesn't apply, no plan

            //       planned                credit
            string studentid = "00004001";     // no plan, credit doesn't apply to this group
            programrequirements = await GetRequirementsAsync("Test6");  //requires one MATH course
            studentprogram = await GetStudentProgram(studentid, programid);
            additionalrequirements = new List<Requirement>();
            plannedcourses = await GetCourses(studentid);
            credits = await GetCredits(studentid);
            overrides = new List<Override>();

            ProgramEvaluation eval = new ProgramEvaluator(studentprogram, programrequirements, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();
            dumper.Dump(eval, "Verbose");

            Assert.IsTrue(eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First().CompletionStatus == CompletionStatus.NotStarted);
            Assert.IsTrue(eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First().PlanningStatus == PlanningStatus.NotPlanned);
            Assert.IsTrue(eval.RequirementResults.First().SubRequirementResults.First().CompletionStatus == CompletionStatus.NotStarted);
            Assert.IsTrue(eval.RequirementResults.First().SubRequirementResults.First().PlanningStatus == PlanningStatus.NotPlanned);
            Assert.IsTrue(eval.RequirementResults.First().CompletionStatus == CompletionStatus.NotStarted);
            Assert.IsTrue(eval.RequirementResults.First().PlanningStatus == PlanningStatus.NotPlanned);

        }

        [TestMethod]
        public async Task DuplicatePlannedCoursesArePickedWhenRetakesAreNotAllowed_AppliedTwiceToRequirement()
        {
            string studentid = "00004008";      //   HIST-100 (twice), MATH-100
            programrequirements = await GetRequirementsAsync("Test20");  //two courses, no MATH,PERF course allowed
            studentprogram = await GetStudentProgram(studentid, programid);
            additionalrequirements = new List<Requirement>();
            plannedcourses = await GetCourses(studentid);
            credits = await GetCredits(studentid);
            overrides = new List<Override>();
            ProgramEvaluation eval = new ProgramEvaluator(studentprogram, programrequirements, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();

            Assert.IsTrue(eval.IsPlannedSatisfied);
        }

        [TestMethod]
        public async Task DuplicatePlannedCoursesArePickedWhenRetakeIsAllowed_AppliedTwiceToRequirement()
        {
            string studentid = "00004008";      //   HIST-100 (twice), MATH-100
            programrequirements = await GetRequirementsAsync("Test20");  //two courses, no MATH,PERF course allowed
            studentprogram = await GetStudentProgram(studentid, programid);
            additionalrequirements = new List<Requirement>();
            plannedcourses = await GetCourses(studentid);
            foreach (var plannedCourse in plannedcourses)
            {
                if (plannedCourse.Course.Id == "139") //hist-100
                {
                    plannedCourse.Course.AllowToCountCourseRetakeCredits = true;
                }
            }
            credits = await GetCredits(studentid);
            overrides = new List<Override>();
            ProgramEvaluation eval = new ProgramEvaluator(studentprogram, programrequirements, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();

            Assert.IsTrue(eval.IsPlannedSatisfied);
        }


        [TestMethod]
        public async Task FurtherGroupsNotEvaluatedForSatisfiedSubrequirement()
        {
            studentid = "00004006";  //no plan, credits ENGL-101, MATH-460
            studentprogram = await GetStudentProgram(studentid, programid);
            additionalrequirements = new List<Requirement>();
            plannedcourses = await GetCourses(studentid);
            credits = await GetCredits(studentid);
            overrides = new List<Override>();


            // Since this test was written the code was changed to change the completeness of in-progrss courses. 
            // We'll give the in-progress course a grade so it evaluates as it did before for this test.
            var credit1 = credits.First(crd => crd.Id == "36");
            credit1.VerifiedGrade = credits.First(crd => crd.Id == "33").VerifiedGrade;


            reqnames.Add("GroupTestRequirement");
            subnames.Add("GroupTestSubrequirement");
            Subreq.Add(reqnames[0], subnames);
            grpnames.Add("Test1"); // TAKE ENGL*101
            grpnames.Add("Test6"); // TAKE 1 COURSE; FROM DEPARTMENT MATH
            groups.Add(subnames[0], grpnames);
            programrequirements = await tprr.BuildTestProgramRequirementsAsync("TestProgramRequirementsId", reqnames, Subreq, groups);

            programrequirements.Requirements.First().SubRequirements.First().MinGroups = 1;  // One satisfied group satisfies Subrequirement

            // Test1 is a "Take all" which should evaluate before Test6.  The Evaluator should see that 
            // test1 is satisfied and that therefore the Subrequirement is as well, and not evaluate
            // test6 at all.

            ProgramEvaluation eval = new ProgramEvaluator(studentprogram, programrequirements, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();
            dumper.Dump(eval, "Verbose");

            // get the group and Subrequirement results
            GroupResult gr1 = eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First(gr => gr.Group.Code == "Test1");
            GroupResult gr2 = eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First(gr => gr.Group.Code == "Test6");
            SubrequirementResult sr1 = eval.RequirementResults.First().SubRequirementResults.First();

            // check group
            Assert.IsTrue(gr1.CompletionStatus == CompletionStatus.Completed);
            //Assert.IsTrue(gr1.PlanningStatus == PlanningStatus.Unplanned);   // this is in flux and unimportant anyway.

            Assert.IsTrue(gr2.CompletionStatus == CompletionStatus.NotStarted);
            Assert.IsTrue(gr2.PlanningStatus == PlanningStatus.NotPlanned);

            // Check Acad Cred results

            Assert.IsTrue(gr1.Results.First(ar => ar.GetCourse().SubjectCode == "ENGL").Result == Result.Applied);
            Assert.IsTrue(gr2.Results.Where(x => x.Result == Result.Applied).Count() == 0);

            // Check Subrequirement
            Assert.IsTrue(eval.RequirementResults.First().SubRequirementResults.First().CompletionStatus == CompletionStatus.Completed);


        }



        [TestMethod]
        public async Task TestSubroutineEvaluatesCompleteifOnlyGroupWaived()
        {
            //       planned                credit
            string studentid = "00004002";    // MATH-100, MATH-200       ENGL-101, ENGL-102
            programrequirements = await GetRequirementsAsync("Test3");  //requires ENGL-101 and ENGL-102
            studentprogram = await GetStudentProgram(studentid, programid);
            additionalrequirements = new List<Requirement>();
            plannedcourses = await GetCourses(studentid);
            credits = await GetCredits(studentid);
            overrides = new List<Override>();

            ProgramEvaluation eval = new ProgramEvaluator(studentprogram, programrequirements, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();
            dumper.Dump(eval, "Verbose");


            Assert.AreEqual(CompletionStatus.PartiallyCompleted, eval.RequirementResults.First().SubRequirementResults.First().CompletionStatus);

            Assert.IsTrue(eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First().GetApplied()
                                                    .Select(g => g.GetCourse().ToString()).Contains("ENGL*101"));


            //Assert.IsTrue(eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First().GetApplied()
            //                                        .Select(g => g.GetCourse().ToString()).Contains("ENGL*102"));

        }

        [TestMethod]
        public async Task GroupPartiallyCompleteIfCreditHasNoGrade()
        {
            //       planned                credit
            string studentid = "00004002";    // MATH-100, MATH-200       ENGL-101, ENGL-102
            programrequirements = await GetRequirementsAsync("Test3");  //requires ENGL-101 and ENGL-102
            studentprogram = await GetStudentProgram(studentid, programid);
            additionalrequirements = new List<Requirement>();
            plannedcourses = await GetCourses(studentid);
            credits = await GetCredits(studentid);
            overrides = new List<Override>();
            BlockReplacement br = new BlockReplacement("10003", null, "");

            studentprogram.AddRequirementModification(br);

            ProgramEvaluation eval = new ProgramEvaluator(studentprogram, programrequirements, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();
            dumper.Dump(eval, "Verbose");


            Assert.AreEqual(CompletionStatus.Completed, eval.RequirementResults.First().SubRequirementResults.First().CompletionStatus);

            //Assert.IsFalse(eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First().GetApplied()
            //                                        .Select(g => g.GetCourse().ToString()).Contains("ENGL*101"));


            //Assert.IsTrue(eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First().GetApplied()
            //                                        .Select(g => g.GetCourse().ToString()).Contains("ENGL*102"));

        }

        [TestMethod]
        public async Task GroupHandlesCourseEquates()
        {
            //// Course equate codes for degree plan domain tests
            // cid 42 HIST 200  has cid 155 POLI 100 on its equate list.  // POLI 100 is an equated course of HIST 200
            // requirement for POLI 100 (cid 155).  acad cred ID 2 for hist 200

            //       planned                credit

            string studentid = "00004002";    // MATH-100, MATH-200       ENGL-101, ENGL-102
            programrequirements = await GetRequirementsAsync("Test26");  // take 3 credits (we'll change this)
            studentprogram = await GetStudentProgram(studentid, programid);
            additionalrequirements = new List<Requirement>();
            plannedcourses = await GetCourses(studentid);

            ICollection<string> ids = new List<string>() { "2" };
            credits = (await academicCreditRepo.GetAsync(ids)).ToList();

            overrides = new List<Override>();

            // add the required course, remove the min credits.  This is kind of unnecessary to do it
            // this way but why not

            CoursesAddition ca = new CoursesAddition("10026", new List<string>() { "155" }, "");
            studentprogram.AddRequirementModification(ca);
            CreditModification cm = new CreditModification("10026", null, "");
            studentprogram.AddRequirementModification(cm);

            ProgramEvaluation eval = new ProgramEvaluator(studentprogram, programrequirements, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();
            dumper.Dump(eval, "Verbose");


            Assert.AreEqual(CompletionStatus.Completed, eval.RequirementResults.First().SubRequirementResults.First().CompletionStatus);



        }

        [TestMethod]
        public async Task ReqExcludingOwnTypeSharesWithSelf()
        {
            //       planned                credit
            string studentid = "00004002";    // MATH-100, MATH-200       ENGL-101, ENGL-102
            programrequirements = await programRequirementsRepo.GetAsync("STSS.MATH.BS*2010", "2010");

            studentprogram = await GetStudentProgram(studentid, programid);
            additionalrequirements = new List<Requirement>();
            plannedcourses = await GetCourses(studentid);
            credits = await GetCredits(studentid);
            overrides = new List<Override>();

            ProgramEvaluation eval = new ProgramEvaluator(studentprogram, programrequirements, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();
            dumper.Dump(eval, "Verbose");

            GroupResult Res20895 = eval.RequirementResults.First(rr => rr.Requirement.Code == "STSS.MATH.MAJ")
                                       .SubRequirementResults.First(sr => sr.SubRequirement.Code == "STSS.MATH.APPL.SUB")
                                       .GroupResults.First(gr => gr.Group.Id == "20895");

            Assert.AreEqual(PlanningStatus.PartiallyPlanned, Res20895.PlanningStatus);
        }

        [TestMethod]
        public async Task ReqExcludingTypeDoesNotShareWithOtherReqs()
        {
            string studentId = "0004002";
            // Create academic program that allows courses of any level to be applied
            // This program contains two requirements, both of GEN requirement type.
            string programCode = "EXCLUDED_REQ";
            programrequirements = await programRequirementsRepo.GetAsync(programCode, "2013");
            // Exclude GEN requirement type
            programrequirements.Requirements.ElementAt(0).RequirementExclusions = new List<RequirementBlockExclusion>() { new RequirementBlockExclusion("GEN") };

            studentprogram = await GetStudentProgram(studentId, programCode);
            // two academic credits for MUSC-209, repeated for credit 
            credits = (await academicCreditRepo.GetAsync(new Collection<string>() { "63", "64" })).ToList();

            // Evaluate
            additionalrequirements = new List<Requirement>();
            overrides = new List<Override>();
            plannedcourses = new List<PlannedCredit>();
            ProgramEvaluation eval = new ProgramEvaluator(studentprogram, programrequirements, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();

            // Verify that both courses were applied to the first requirement, and none applied to the second one (not shared because the first requirement excludes the second via GEN exclusion type)
            Assert.AreEqual(6, eval.RequirementResults.ElementAt(0).GetAppliedCredits());
            Assert.AreEqual(0, eval.RequirementResults.ElementAt(1).GetAppliedCredits());
        }

        [TestMethod]
        public async Task ReqExcludingTypeDoesNotShareWithOtherReqs_WithRetakeAllowed()
        {
            string studentId = "0004002";
            // Create academic program that allows courses of any level to be applied
            // This program contains two requirements, both of GEN requirement type.
            string programCode = "EXCLUDED_REQ";
            programrequirements = await programRequirementsRepo.GetAsync(programCode, "2013");
            // Exclude GEN requirement type
            programrequirements.Requirements.ElementAt(0).RequirementExclusions = new List<RequirementBlockExclusion>() { new RequirementBlockExclusion("GEN") };

            studentprogram = await GetStudentProgram(studentId, programCode);
            // two academic credits for MUSC-209, repeated for credit 
            credits = (await academicCreditRepo.GetAsync(new Collection<string>() { "63", "64" })).ToList();
            foreach (var credit in credits)
            {
                if (credit.Course.Id == "7718")
                {
                    credit.Course.AllowToCountCourseRetakeCredits = true;
                }
            }
            // Evaluate
            additionalrequirements = new List<Requirement>();
            overrides = new List<Override>();
            plannedcourses = new List<PlannedCredit>();
            ProgramEvaluation eval = new ProgramEvaluator(studentprogram, programrequirements, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();

            // Verify that both courses were applied to the first requirement, and none applied to the second one (not shared because the first requirement excludes the second via GEN exclusion type)
            Assert.AreEqual(6, eval.RequirementResults.ElementAt(0).GetAppliedCredits());
            Assert.AreEqual(0, eval.RequirementResults.ElementAt(1).GetAppliedCredits());
        }


        [TestMethod]
        public async Task GroupExcludingTypeDoesNotShareWithOtherReqs()
        {
            string studentId = "0004002";
            // Create academic program that allows courses of any level to be applied
            // This program contains two requirements, one MAJ and one GEN.
            string programCode = "GROUP_EXCLUDED_REQ";
            programrequirements = await programRequirementsRepo.GetAsync(programCode, "2013");
            // Exclude GEN requirement type
            //programrequirements.Requirements.ElementAt(0).Exclusions = new List<string>() { "GEN" };
            studentprogram = await GetStudentProgram(studentId, programCode);
            // two academic credits for MUSC-209, repeated for credit 
            credits = (await academicCreditRepo.GetAsync(new Collection<string>() { "13", "17" })).ToList();

            // Evaluate
            additionalrequirements = new List<Requirement>();
            overrides = new List<Override>();
            plannedcourses = new List<PlannedCredit>();
            ProgramEvaluation eval = new ProgramEvaluator(studentprogram, programrequirements, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();

            // Verify that both courses were applied to the first requirement, and none applied to the second one (not shared because the first requirement excludes the second via GEN exclusion type)
            Assert.AreEqual(4, eval.RequirementResults.ElementAt(0).GetAppliedCredits());
            Assert.AreEqual(0, eval.RequirementResults.ElementAt(1).GetAppliedCredits());
        }

        [TestMethod]
        public async Task ReqExcludingTypeDoesNotShareWithOtherReqs_WithOverride()
        {
            string studentId = "0004002";
            // Create academic program that allows courses of any level to be applied
            // This program contains two requirements, both of GEN requirement type.
            string programCode = "EXCLUDED_REQ2";
            programrequirements = await programRequirementsRepo.GetAsync(programCode, "2013");
            // Exclude GEN requirement type
            programrequirements.Requirements.ElementAt(0).RequirementExclusions = new List<RequirementBlockExclusion>() { new RequirementBlockExclusion("GEN") };

            studentprogram = await GetStudentProgram(studentId, programCode);
            // academic credits for MUSC-209 level 200 - would fulfill a group in both requirements but exclusions prevent it.
            credits = (await academicCreditRepo.GetAsync(new Collection<string>() { "63" })).ToList();
            overrides = new List<Override>();
            overrides.Add(new Override("100-200ONLY", new List<string>() { "63" }, new List<string>()));  // allow MUSC-209 on group 100-200ONLY - which normally would have excluded it based on requirement type exclusion.

            // Evaluate
            additionalrequirements = new List<Requirement>();

            plannedcourses = new List<PlannedCredit>();
            ProgramEvaluation eval = new ProgramEvaluator(studentprogram, programrequirements, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();

            // Verify that MUSC-209 course (3 credits) was applied to the first & second requirement because of override
            // (even though requirements do not share courses based on a GEN exclusion type)
            Assert.AreEqual(3, eval.RequirementResults.ElementAt(0).GetAppliedCredits());
            Assert.AreEqual(3, eval.RequirementResults.ElementAt(1).GetAppliedCredits());
        }

        // Verifies that when a requirement type is excluded by a requirement, that a given credit applied to that
        // requirement will not be applied to any additional requirements of the excluded requirement type.
        [TestMethod]
        public async Task ReqWithExcludingTypeDoesNotShareWithAdditionalReqs_WithRetakeAllowed()
        {
            string studentId = "0004002";
            // Create academic program that allows courses of any level to be applied
            // Creates two requirements each of GEN requirement type, Exclude GEN requirement type
            string programCode = "EXCLUDED_REQ";
            programrequirements = await programRequirementsRepo.GetAsync(programCode, "2013");
            // Exclude GEN requirement type
            programrequirements.Requirements.ElementAt(0).RequirementExclusions = new List<RequirementBlockExclusion>() { new RequirementBlockExclusion("GEN") };

            studentprogram = await GetStudentProgram(studentId, programCode);
            // two academic credits for MUSC-209, repeated for credit 
            credits = (await academicCreditRepo.GetAsync(new Collection<string>() { "63", "64" })).ToList();
            foreach (var credit in credits)
            {
                if (credit.Course.Id == "7718")
                {
                    credit.Course.AllowToCountCourseRetakeCredits = true;
                }
            }
            // Additional requirement 1 of type GEN. This will be excluded due to exclusion type above
            var addlReq1 = new Requirement("AR1", "AR1", "Additional Requirement 1", "UG", new RequirementType("GEN", "General", "98"));
            var addlSubreq1 = new Subrequirement("ASR1", "ASR1");
            var addlGroup1 = new Group("AG1", "AG1", addlSubreq1)
            {
                MinCourses = 10,
                // accept courses from any level
                FromLevels = new List<string>() { "100", "200", "300", "400", "500" },
                GroupType = GroupType.TakeCourses
            };
            addlSubreq1.Requirement = addlReq1;
            addlSubreq1.Groups = new List<Group>() { addlGroup1 };
            addlReq1.SubRequirements = new List<Subrequirement>() { addlSubreq1 };
            additionalrequirements = new List<Requirement>() { addlReq1 };

            // Evaluate
            overrides = new List<Override>();
            plannedcourses = new List<PlannedCredit>();
            ProgramEvaluation eval = new ProgramEvaluator(studentprogram, programrequirements, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();

            // Verify that two courses were applied two the first requirement
            Assert.AreEqual(6, eval.RequirementResults.ElementAt(0).GetAppliedCredits());
            // Credits not applied to second requirement due to exclusion
            Assert.AreEqual(0, eval.RequirementResults.ElementAt(1).GetAppliedCredits());
            // Credits not applied to additional requirement because it is of GEN type and it is explicitly excluded
            Assert.AreEqual(0, eval.RequirementResults.ElementAt(2).GetAppliedCredits());
        }



        // Verifies that when a student fails a course AND plans another course to replace the failed one, the plannedcredits
        // aren't applied to any other excluding group.
        // Bug Example: A student fails Math 100. When the student plans Math-100 again, the planned course is applied to
        //              every requirement that needs Math-100 even if they exclude from eachother.
        [TestMethod]
        public async Task ReqWithExcludingTypeDoesNotSharePlannedCourseIfCourseHasBeenFailedPreviously()
        {
            string studentId = "0004002";
            // Create academic program that allows courses of any level to be applied
            // Creates two requirements each of GEN requirement type, both exclude GEN requirement type
            string programCode = "EXCLUDED_REQ3";
            programrequirements = await programRequirementsRepo.GetAsync(programCode, "2013");
            // Exclude GEN requirement type
            programrequirements.Requirements.ElementAt(0).RequirementExclusions = new List<RequirementBlockExclusion>() { new RequirementBlockExclusion("GEN") };
            programrequirements.Requirements.ElementAt(1).RequirementExclusions = new List<RequirementBlockExclusion>() { new RequirementBlockExclusion("GEN") };


            // Set MinGrade for the group (makes it so that a credit with a grade of 'F' isn't applied to a requirement)
            programrequirements.Requirements.ElementAt(0).SubRequirements.ElementAt(0).Groups.ElementAt(0).MinGrade = (await graderepo.GetAsync()).Where(g => g.Id == "D").First();
            programrequirements.Requirements.ElementAt(1).SubRequirements.ElementAt(0).Groups.ElementAt(0).MinGrade = (await graderepo.GetAsync()).Where(g => g.Id == "D").First();

            studentprogram = await GetStudentProgram(studentId, programCode);
            // One academic credit for LAW-368. Failed no credit is received.
            credits = (await academicCreditRepo.GetAsync(new Collection<string>() { "72" })).ToList();
            additionalrequirements = new List<Requirement>();

            // One planned course for Law-368. Not started
            plannedcourses = new List<PlannedCredit>();
            var crse = await courseRepo.GetAsync("7724");
            plannedcourses.Add(new PlannedCredit(crse, "2010/SP"));

            overrides = new List<Override>();

            // Evaluate
            ProgramEvaluation eval = new ProgramEvaluator(studentprogram, programrequirements, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();

            //Verify that the planned credit was only applied to one requirement
            Assert.AreNotEqual(eval.RequirementResults.ElementAt(0).GetPlannedAppliedCredits(), eval.RequirementResults.ElementAt(1).GetPlannedAppliedCredits());
        }

        // Verifies that when an academic credit is applied to a requirement, and that requirement's type is excluded by another requirement, then the
        // academic credit will not be applied to the requirement that excludes that type since it has already been applied to a requirement of the 
        // excluded type. However it will be applied to a requirement that does not exclude the type that the academic credit has been applied to.
        [TestMethod]
        public async Task ReqWithExcludedTypeDoesNotShareWithReqThatExcludesIt()
        {
            string studentId = "0004002";
            // Create academic program with two requirements that allow courses of any level to be applied
            string programCode = "EXCLUDED_REQ";
            programrequirements = await programRequirementsRepo.GetAsync(programCode, "2013");
            // This is a GEN requirement type, exclude MAJ requirement type. This will also imply that MAJ requirement types exclude GEN requirement types.
            programrequirements.Requirements.ElementAt(0).RequirementType = new RequirementType("GEN", "General", "98");
            programrequirements.Requirements.ElementAt(0).RequirementExclusions = new List<RequirementBlockExclusion>(){new RequirementBlockExclusion("MAJ") };
            // Second requirement is also a GEN requirement type. 
            programrequirements.Requirements.ElementAt(1).RequirementType = new RequirementType("GEN", "General", "98");
            studentprogram = await GetStudentProgram(studentId, programCode);
            // one academic credit for MUSC-209
            credits = (await academicCreditRepo.GetAsync(new Collection<string>() { "63" })).ToList();
            // Additional requirement 1 - this one will be evaluated first. GEN exclude is implied; because GEN excludes MAJ above, MAJ will in turn exclude GEN.
            var addlReq1 = new Requirement("AR1", "AR1", "Additional Requirement 1", "UG", new RequirementType("MAJ", "Major", "98"));
            var addlSubreq1 = new Subrequirement("ASR1", "ASR1");
            var addlGroup1 = new Group("AG1", "AG1", addlSubreq1)
            {
                MinCourses = 10,
                // take all: take course MUSC-209
                Courses = new List<string>() { "7718" },
                GroupType = GroupType.TakeAll
            };
            addlSubreq1.Requirement = addlReq1;
            addlSubreq1.Groups = new List<Group>() { addlGroup1 };
            addlReq1.SubRequirements = new List<Subrequirement>() { addlSubreq1 };
            additionalrequirements = new List<Requirement>() { addlReq1 };

            // Evaluate
            overrides = new List<Override>();
            plannedcourses = new List<PlannedCredit>();
            ProgramEvaluation eval = new ProgramEvaluator(studentprogram, programrequirements, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();

            // Verify that the two courses were not applied to the first requirement because of implied exclusion of GEN type by MAJ (requirement 0)
            Assert.AreEqual(0, eval.RequirementResults.ElementAt(0).GetAppliedCredits());
            // Applied to second requirement type because MAJ not excluded (requirement 1)
            Assert.AreEqual(3, eval.RequirementResults.ElementAt(1).GetAppliedCredits());
            // Credits applied to additional requirement of type MAJ first because it is "take all". Since applied to this one first, the credit will not be applied 
            // to the requirement that excludes requirements of type MAJ.
            Assert.AreEqual(3, eval.RequirementResults.ElementAt(2).GetAppliedCredits());
        }

        [TestMethod]
        public async Task ReqExcludingTypeSharesWithExcludedReqTakeAllGroup()
        {
            string studentId = "0004002";
            // Create academic program that allows courses of any level to be applied
            string programCode = "EXCLUDED_REQ_WITH_TAKEALL";
            programrequirements = await programRequirementsRepo.GetAsync(programCode, "2013");
            studentprogram = await GetStudentProgram(studentId, programCode);
            // two academic credits for MUSC-209, repeated for credit 
            credits = (await academicCreditRepo.GetAsync(new Collection<string>() { "63", "64" })).ToList();

            // Evaluate
            additionalrequirements = new List<Requirement>();
            overrides = new List<Override>();
            plannedcourses = new List<PlannedCredit>();
            ProgramEvaluation eval = new ProgramEvaluator(studentprogram, programrequirements, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();

            // Verify that MATH-309 was applied to both requirements since they both had the TAKE ALL group.
            Assert.AreEqual(3, eval.RequirementResults.ElementAt(0).GetAppliedCredits());
            Assert.AreEqual(3, eval.RequirementResults.ElementAt(1).GetAppliedCredits());
        }

        [TestMethod]
        public async Task ReqExcludingTypeSharesWithExcludedReqTakeAllGroupNotWithTakeCreditGroup()
        {
            string studentId = "0004002";
            // Create academic program that allows courses of any level to be applied
            string programCode = "EXCLUDED_REQ_WITH_TAKEALL_AND_TAKECREDITS";
            programrequirements = await programRequirementsRepo.GetAsync(programCode, "2013");
            studentprogram = await GetStudentProgram(studentId, programCode);
            // two academic credits for MUSC-209, repeated for credit 
            credits = (await academicCreditRepo.GetAsync(new Collection<string>() { "63", "64" })).ToList();

            // Evaluate
            additionalrequirements = new List<Requirement>();
            overrides = new List<Override>();
            plannedcourses = new List<PlannedCredit>();
            ProgramEvaluation eval = new ProgramEvaluator(studentprogram, programrequirements, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();

            // Verify that the course was applied twice to one requirement, but only once to the other (because it shared with the "takeall" group but will not share with the "take credits" group.
            Assert.AreEqual(6, eval.RequirementResults.ElementAt(0).GetAppliedCredits());
            Assert.AreEqual(3, eval.RequirementResults.ElementAt(1).GetAppliedCredits());
        }

        [TestMethod]
        public async Task ReqExclusion_WithExclusionFlag()
        {
            // Simple program with three requirements,
            //R1 type MAJ   with S1 -> G1 -> Take 10 courses from level 100,200,300,400,500 (SIMPLY.ANY)
            //R2 type MAJ  with S2 -> G1 -> ake 10 courses from level 100,200,300,400,500 (SIMPLY.ANY) and S1 -> G2 (TAKE MUSC-209)
            //R3 type MIN this excludes MAJ with flag=Y with S3 ->G2 -> take MUSC-209
            string studentId = "0004002";
            string programCode = "EXCLUDED_REQ_WITH_EXCLUSION_FLAG";
            programrequirements = await programRequirementsRepo.GetAsync(programCode, "2013");
            studentprogram = await GetStudentProgram(studentId, programCode);

            // 64 - MUSC-209,  58 is TR,  61 is IP and others are completed
            credits = (await academicCreditRepo.GetAsync(new Collection<string>() { "58", "64" , "61","43", "115"})).ToList();
            plannedcourses = new List<PlannedCredit>();
            // Evaluate
            additionalrequirements = new List<Requirement>();
            overrides = new List<Override>();
            plannedcourses = new List<PlannedCredit>();
            ProgramEvaluation eval = new ProgramEvaluator(studentprogram, programrequirements, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();
            //R1 with first MAJ should not have MUSC-209 applied because it should only be applied to MIN
            Assert.AreEqual(4, eval.RequirementResults.ElementAt(0).SubRequirementResults[0].GroupResults[0].GetApplied().Count());
            Assert.IsFalse(eval.RequirementResults.ElementAt(0).SubRequirementResults[0].GroupResults[0].GetApplied().Select(s=>s.GetAcadCredId()).Contains("64"));
            //R2 of type MAJ should apply all the courses because it is 2nd major
            Assert.AreEqual(1, eval.RequirementResults.ElementAt(1).SubRequirementResults[0].GroupResults[0].GetApplied().Count());
            Assert.AreEqual("64", eval.RequirementResults.ElementAt(1).SubRequirementResults[0].GroupResults[0].GetApplied().ToList()[0].GetAcadCredId());

            //R2 should have MUSC-209 applied because MIN is only excluding first major
            Assert.AreEqual(5, eval.RequirementResults.ElementAt(1).SubRequirementResults[0].GroupResults[1].GetApplied().Count());
            //apply MSUC-209
            Assert.AreEqual(1, eval.RequirementResults.ElementAt(2).SubRequirementResults[0].GroupResults[0].GetApplied().Count());
            Assert.AreEqual("64", eval.RequirementResults.ElementAt(2).SubRequirementResults[0].GroupResults[0].GetApplied().ToList()[0].GetAcadCredId());
            
        }

        // Verifies that when a requirement is set up to not allow course reuse between subrequirements, and there
        // are two instances of a course taken for credit, that one academic credit for the course will be applied
        // to a group under one subrequirement, and the other academic credit for the same course will be applied
        // to a group under the other subrequirement.
        [TestMethod]
        public async Task TwoSectionsOfSameCourseAppliedToNonSharingSubrequirements()
        {
            string studentId = "0004002";
            // This program contains one requirement with two subrequirements that cannot share
            string programCode = "TWO_SUBREQS_NOSHARE";
            programrequirements = await programRequirementsRepo.GetAsync(programCode, "2013");
            studentprogram = await GetStudentProgram(studentId, programCode);
            // two academic credits for MUSC-209, repeated for credit 
            credits = (await academicCreditRepo.GetAsync(new Collection<string>() { "63", "64" })).ToList();

            // Evaluate
            additionalrequirements = new List<Requirement>();
            overrides = new List<Override>();
            plannedcourses = new List<PlannedCredit>();
            ProgramEvaluation eval = new ProgramEvaluator(studentprogram, programrequirements, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();

            // Verify that one course was applied to one group and the other course applied to the other group.
            Assert.AreEqual("64", eval.RequirementResults.ElementAt(0).SubRequirementResults.ElementAt(0).GroupResults.ElementAt(0).GetApplied().ElementAt(0).GetAcadCredId());
            Assert.AreEqual("63", eval.RequirementResults.ElementAt(0).SubRequirementResults.ElementAt(1).GroupResults.ElementAt(0).GetApplied().ElementAt(0).GetAcadCredId());
        }

        // Verifies that when a requirement is set up to not allow course reuse between subrequirements, but that the student has an override to 
        // allow the course on the second subrequirement that it is allowed. 
        [TestMethod]
        public async Task SameCourseAppliedToNonSharingSubrequirements_WithOverride()
        {
            string studentId = "0004002";
            // This program contains one requirement with two subrequirements that cannot share
            string programCode = "TWO_SUBREQS_NOSHARE";
            programrequirements = await programRequirementsRepo.GetAsync(programCode, "2013");
            studentprogram = await GetStudentProgram(studentId, programCode);
            // two academic credits for MUSC-209, repeated for credit 
            credits = (await academicCreditRepo.GetAsync(new Collection<string>() { "63" })).ToList();
            overrides = new List<Override>();
            overrides.Add(new Override("MUSC-209-TWO", new List<string>() { "63" }, new List<string>()));  // allow MUSC-209 on group xx - which normally would have excluded it based on reuse.

            // Evaluate
            additionalrequirements = new List<Requirement>();
            plannedcourses = new List<PlannedCredit>();
            ProgramEvaluation eval = new ProgramEvaluator(studentprogram, programrequirements, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();

            // Verify that one course was applied to one group and the other course applied to the other group.
            Assert.AreEqual("63", eval.RequirementResults.ElementAt(0).SubRequirementResults.ElementAt(0).GroupResults.ElementAt(0).GetApplied().ElementAt(0).GetAcadCredId());
            Assert.AreEqual("63", eval.RequirementResults.ElementAt(0).SubRequirementResults.ElementAt(1).GroupResults.ElementAt(0).GetApplied().ElementAt(0).GetAcadCredId());
        }

        // Verifies that when a subrequirement is set up to now allow course reuse between groups, and there are two 
        // instances of a course taken for credit, that one academic credit for the course will be applied to one
        // group and the other academic credit (not the original) will be applied to the other group.
        [TestMethod]
        public async Task TwoSectionsOfSameCourseAppliedToNonSharingGroups()
        {
            string studentId = "0004002";
            // This program contains one requirement with one subrequirement with two groups that cannot share
            string programCode = "TWO_GROUPS_NOSHARE";
            programrequirements = await programRequirementsRepo.GetAsync(programCode, "2013");
            studentprogram = await GetStudentProgram(studentId, programCode);
            // two academic credits for MUSC-209, repeated for credit 
            credits = (await academicCreditRepo.GetAsync(new Collection<string>() { "63", "64" })).ToList();

            // Evaluate
            additionalrequirements = new List<Requirement>();
            overrides = new List<Override>();
            plannedcourses = new List<PlannedCredit>();
            ProgramEvaluation eval = new ProgramEvaluator(studentprogram, programrequirements, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();

            // Verify that one course was applied to one group and the other course applied to the other group.
            Assert.AreEqual("64", eval.RequirementResults.ElementAt(0).SubRequirementResults.ElementAt(0).GroupResults.ElementAt(0).GetApplied().ElementAt(0).GetAcadCredId());
            Assert.AreEqual("63", eval.RequirementResults.ElementAt(0).SubRequirementResults.ElementAt(0).GroupResults.ElementAt(1).GetApplied().ElementAt(0).GetAcadCredId());
        }

        // Verifies that when a subrequirement is set up to now allow course reuse between groups, but there is an override for the second group
        // that the credit will be applied to both.
        [TestMethod]
        public async Task SameCourseAppliedToNonSharingGroups_WithOverride()
        {
            string studentId = "0004002";
            // This program contains one requirement with one subrequirement with two groups that cannot share
            string programCode = "TWO_GROUPS_NOSHARE";
            programrequirements = await programRequirementsRepo.GetAsync(programCode, "2013");
            studentprogram = await GetStudentProgram(studentId, programCode);
            // two academic credits for MUSC-209, repeated for credit 
            credits = (await academicCreditRepo.GetAsync(new Collection<string>() { "63" })).ToList();
            overrides = new List<Override>();
            overrides.Add(new Override("MUSC-209-TWO", new List<string>() { "63" }, new List<string>()));  // allow MUSC-209 on group xx - which normally would have excluded it based on reuse.

            // Evaluate
            additionalrequirements = new List<Requirement>();

            plannedcourses = new List<PlannedCredit>();
            ProgramEvaluation eval = new ProgramEvaluator(studentprogram, programrequirements, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();

            // Verify that one course was applied to one group and the other course applied to the other group.
            Assert.AreEqual("63", eval.RequirementResults.ElementAt(0).SubRequirementResults.ElementAt(0).GroupResults.ElementAt(0).GetApplied().ElementAt(0).GetAcadCredId());
            Assert.AreEqual("63", eval.RequirementResults.ElementAt(0).SubRequirementResults.ElementAt(0).GroupResults.ElementAt(1).GetApplied().ElementAt(0).GetAcadCredId());
        }

        [TestMethod]
        public async Task AcadCreditsSortedByDefault()
        {
            string studentId = "0004002";
            string programCode = "SIMPLE";
            programrequirements = await programRequirementsRepo.GetAsync(programCode, "2013");
            studentprogram = await GetStudentProgram(studentId, programCode);
            additionalrequirements = new List<Requirement>();
            overrides = new List<Override>();

            // two planned courses, BIOL 200, MATH 201. These will be applied last
            var crses = (await courseRepo.GetAsync(new List<string>() { "21", "47" })).ToList();
            plannedcourses = new List<PlannedCredit>();
            foreach (var crs in crses)
            {
                plannedcourses.Add(new PlannedCredit(crs, "2009/SP"));
            }
            // Seven academic credits, to be applied in the sequence indicated with #
            // Institutional graded credits
            //    #0 {"6","MATH-152","Calculus II",				    "N","3.00", "I","2009/SP", "A", "4", "12",   "333", "8006","0.00","3.00","C","" ,"" , "","" ,"", "G",   "001"},
            //    #1 {"58","MATH-201","Linear Algebra",			    "N","3.00", "I","2009/SP", "A", "4", "12",    "47", "8005","0.00","3.00","B","C","" ," "," ","", "G",   "001"},
            //    #2 {"7","BIOL-400","Advanced Topics in Biology",	"N","4.00", "I","2009/FA", "B", "3", "12",    "64", "8007","0.00","4.00","A","", "A","A","A","A","G",   "002"}, 
            // Institutional, ungraded but complete
            //    #3 {"62","MUSC-208","Instrumental Ensemble",		"N","1.00","I","2009/SP", "", "3",  "3",   "7117", "8113","0.00","1.00", "", "", "", "", "", "","G",   "003"}, // Institutional, never graded, complete
            // Transfer, Graded
            //    #4 {"59","SPAN-100","Beginning Spanish",			"T","3.00","TR","2009/SP", "B", "3",  "9",   "160", "8112","0.00","3.00", "", "", "", "", "", "","G",   "003"},
            //    #5 {"13","MUSC-100","History of Music",			"T","4.00","TR","2010/SP", "B", "3", "12",   "148", "8013","0.00","4.00", "", "", "", "", "", "","G",   "003"},
            // Institutional, in progress
            //    #6 {"61","HU-1000","Caring about your fellow man","N","1.00", "I",       "",  "",  "",  "0",  "2420", "8114","0.00","1.00", "", "", "", "", "", "","G",   "001"}, // Institutional, in progress
            // Planned courses
            //    #7 BIOL 200
            //    #8 MATH 201
            credits = (await academicCreditRepo.GetAsync(new Collection<string>() { "61", "13", "59", "62", "7", "58", "6" })).ToList(); // the seven academic credits (in reverse sequence, to ensure not applied in sequence received

            ProgramEvaluation eval = new ProgramEvaluator(studentprogram, programrequirements, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();
            dumper.Dump(eval, "Verbose");

            var groupResultApplied = eval.RequirementResults.ElementAt(0).SubRequirementResults.ElementAt(0).GroupResults.ElementAt(0).Results;
            Assert.AreEqual(9, groupResultApplied.Count());
            Assert.AreEqual("58", groupResultApplied.ElementAt(0).GetAcadCredId()); // Institutional/graded
            Assert.AreEqual("6", groupResultApplied.ElementAt(1).GetAcadCredId()); // Institutional/graded
            Assert.AreEqual("7", groupResultApplied.ElementAt(2).GetAcadCredId()); // Institutional/graded--later start date
            Assert.AreEqual("62", groupResultApplied.ElementAt(3).GetAcadCredId()); // Institutional/ungraded/complete
            Assert.AreEqual("59", groupResultApplied.ElementAt(4).GetAcadCredId()); // Transfer/graded
            Assert.AreEqual("13", groupResultApplied.ElementAt(5).GetAcadCredId()); // Transfer/graded
            Assert.AreEqual("61", groupResultApplied.ElementAt(6).GetAcadCredId()); // Institutional/in progress
            Assert.AreEqual("BIOL-200", groupResultApplied.ElementAt(7).GetCourse().Name); // planned course
            Assert.AreEqual("MATH-201", groupResultApplied.ElementAt(8).GetCourse().Name); // planned course
        }

        [TestMethod]
        public async Task HasPlannedCourses()
        {
            string studentId = "0004002";
            string programCode = "SIMPLE1";
            programrequirements = await programRequirementsRepo.GetAsync(programCode, "2013");
            studentprogram = await GetStudentProgram(studentId, programCode);
            additionalrequirements = new List<Requirement>();
            overrides = new List<Override>();

            // two planned courses, BIOL 200, MATH 201 and PHYS 500. Only phys-500 will be applied because the "NONE" program accepts 500 level courses only
            var crses = (await courseRepo.GetAsync(new List<string>() { "21", "47", "9877" })).ToList();
            plannedcourses = new List<PlannedCredit>();
            foreach (var crs in crses)
            {
                plannedcourses.Add(new PlannedCredit(crs, "2017/SP"));
            }
            credits = (await academicCreditRepo.GetAsync(new Collection<string>() { "60", "59" })).ToList();

            ProgramEvaluation eval = new ProgramEvaluator(studentprogram, programrequirements, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();

            Assert.AreEqual(2, eval.OtherPlannedCredits.Count());
        }

        [TestMethod]
        public async Task SetsMinInstGPAExplanation()
        {
            string studentId = "0004002";
            string programCode = "SIMPLE1";
            programrequirements = await programRequirementsRepo.GetAsync(programCode, "2013");
            studentprogram = await GetStudentProgram(studentId, programCode);
            additionalrequirements = new List<Requirement>();
            overrides = new List<Override>();

            // two planned courses, BIOL 200, MATH 201 and PHYS 500. Only phys-500 will be applied because the "NONE" program accepts 500 level courses only
            var crses = (await courseRepo.GetAsync(new List<string>() { "21", "47", "9877" })).ToList();
            plannedcourses = new List<PlannedCredit>();
            foreach (var crs in crses)
            {
                plannedcourses.Add(new PlannedCredit(crs, "2017/SP"));
            }
            //The AcademicCredit of id "50" has a grade of F resulting in an institutional GPA of 0.000
            credits = (await academicCreditRepo.GetAsync(new Collection<string>() { "60", "59", "50" })).ToList();

            ProgramEvaluation eval = new ProgramEvaluator(studentprogram, programrequirements, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();

            dumper.Dump(eval, "Verbose");

            // The resulting institutional gpa is less than the required inst gpa (first assert verifies that), and the appropriate explanation should be there.
            Assert.IsTrue(eval.InstGpa < eval.ProgramRequirements.MinInstGpa);
            Assert.IsTrue(eval.Explanations.Contains(ProgramRequirementsExplanation.MinInstGpa));
        }

        [TestMethod]
        public async Task SetsMinInstGPAExplanationWithNullInstGpa()
        {
            string studentId = "0004002";
            string programCode = "SIMPLE1";
            programrequirements = await programRequirementsRepo.GetAsync(programCode, "2013");
            studentprogram = await GetStudentProgram(studentId, programCode);
            additionalrequirements = new List<Requirement>();
            overrides = new List<Override>();

            // two planned courses, BIOL 200, MATH 201 and PHYS 500. Only phys-500 will be applied because the "NONE" program accepts 500 level courses only
            var crses = (await courseRepo.GetAsync(new List<string>() { "21", "47", "9877" })).ToList();
            plannedcourses = new List<PlannedCredit>();
            foreach (var crs in crses)
            {
                plannedcourses.Add(new PlannedCredit(crs, "2017/SP"));
            }
            //Neither of these credits are set to "Institutional" so the eval.InstGpa should result in null and still have the MinInstGpa explanation
            credits = (await academicCreditRepo.GetAsync(new Collection<string>() { "60", "59" })).ToList();

            ProgramEvaluation eval = new ProgramEvaluator(studentprogram, programrequirements, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();

            dumper.Dump(eval, "Verbose");

            // The resulting institutional gpa is null and therefore less than the minimum institutional gpa
            Assert.IsTrue(eval.InstGpa == null);
            Assert.IsTrue(eval.Explanations.Contains(ProgramRequirementsExplanation.MinInstGpa));
        }
        [TestMethod]
        public async Task NullInstitutionGPA()
        {
            string studentId = "0004002";
            string programCode = "SIMPLE1";
            programrequirements = await programRequirementsRepo.GetAsync(programCode, "2013");
            studentprogram = await GetStudentProgram(studentId, programCode);
            additionalrequirements = new List<Requirement>();
            overrides = new List<Override>();

            // two planned courses, BIOL 200, MATH 201 and PHYS 500. Only phys-500 will be applied because the "NONE" program accepts 500 level courses only
            var crses = (await courseRepo.GetAsync(new List<string>() { "21", "47", "9877" })).ToList();
            plannedcourses = new List<PlannedCredit>();
            foreach (var crs in crses)
            {
                plannedcourses.Add(new PlannedCredit(crs, "2017/SP"));
            }
            credits = (await academicCreditRepo.GetAsync(new Collection<string>() { "60", "59" })).ToList();

            ProgramEvaluation eval = new ProgramEvaluator(studentprogram, programrequirements, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();

            dumper.Dump(eval, "Verbose");

            // The resulting Gpa should be null since neither of the AcademicCredits are institutional credits.
            Assert.IsTrue(eval.InstGpa == null);
            Assert.IsTrue(eval.Explanations.Contains(ProgramRequirementsExplanation.MinInstGpa));
        }

        [TestMethod]
        public async Task RepeatsOfCourseRepeatableForCreditBothApplied_WhenRetakeIsAllowed()
        {
            string studentId = "0004002";
            // Create academic program that allows courses of any level to be applied
            string programCode = "SIMPLE";
            programrequirements = await programRequirementsRepo.GetAsync(programCode, "2013");
            studentprogram = await GetStudentProgram(studentId, programCode);
            // two academic credits for MUSC-209, repeated for credit 
            credits = (await academicCreditRepo.GetAsync(new Collection<string>() { "63", "64" })).ToList();
            foreach (var credit in credits)
            {
                if (credit.Course.Id == "7718") //MUSC-209
                {
                    credit.Course.AllowToCountCourseRetakeCredits = true;
                }

            }
            // Evaluate
            additionalrequirements = new List<Requirement>();
            overrides = new List<Override>();
            plannedcourses = new List<PlannedCredit>();
            ProgramEvaluation eval = new ProgramEvaluator(studentprogram, programrequirements, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();

            // Verify that two courses were applied
            Assert.AreEqual(2, eval.RequirementResults.ElementAt(0).SubRequirementResults.ElementAt(0).GroupResults.ElementAt(0).GetApplied().Count());
        }

        [TestMethod]
        public async Task RepeatsOfCourseRepeatableForCreditOnlyBothApplied_WhenRetakeIsNotAllowed()
        {
            string studentId = "0004002";
            // Create academic program that allows courses of any level to be applied
            string programCode = "SIMPLE";
            programrequirements = await programRequirementsRepo.GetAsync(programCode, "2013");
            studentprogram = await GetStudentProgram(studentId, programCode);
            // two academic credits for MUSC-209, repeated for credit 
            credits = (await academicCreditRepo.GetAsync(new Collection<string>() { "63", "64" })).ToList();
            foreach (var credit in credits)
            {
                if (credit.Course.Id == "7718") //MUSC-209
                {
                    credit.Course.AllowToCountCourseRetakeCredits = false;
                }

            }
            // Evaluate
            additionalrequirements = new List<Requirement>();
            overrides = new List<Override>();
            plannedcourses = new List<PlannedCredit>();
            ProgramEvaluation eval = new ProgramEvaluator(studentprogram, programrequirements, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();

            // Verify that two courses were applied
            Assert.AreEqual(2, eval.RequirementResults.ElementAt(0).SubRequirementResults.ElementAt(0).GroupResults.ElementAt(0).GetApplied().Count());
        }

        [TestMethod]
        public async Task RepeatsOfCourseRepeatableForCreditOnlyBothApplied_WhenRetakeValueIsSet()
        {
            string studentId = "0004002";
            // Create academic program that allows courses of any level to be applied
            string programCode = "SIMPLE";
            programrequirements = await programRequirementsRepo.GetAsync(programCode, "2013");
            studentprogram = await GetStudentProgram(studentId, programCode);
            // two academic credits for MUSC-209, repeated for credit 
            credits = (await academicCreditRepo.GetAsync(new Collection<string>() { "63", "64" })).ToList();
            foreach (var credit in credits)
            {
                if (credit.Course.Id == "7718") //MUSC-209
                {
                    credit.Course.AllowToCountCourseRetakeCredits = true;
                }

            }
            // Evaluate
            additionalrequirements = new List<Requirement>();
            overrides = new List<Override>();
            plannedcourses = new List<PlannedCredit>();
            ProgramEvaluation eval = new ProgramEvaluator(studentprogram, programrequirements, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();

            // Verify that two courses were applied
            Assert.AreEqual(2, eval.RequirementResults.ElementAt(0).SubRequirementResults.ElementAt(0).GroupResults.ElementAt(0).GetApplied().Count());
        }
        [TestMethod]
        public async Task ReplacedCourseNotApplied()
        {
            string studentId = "0004002";
            // Create academic program that allows courses of any level to be applied
            string programCode = "SIMPLE";
            programrequirements = await programRequirementsRepo.GetAsync(programCode, "2013");
            studentprogram = await GetStudentProgram(studentId, programCode);
            // two academic credits for MUSC-210, 66 replaces 65
            credits = (await academicCreditRepo.GetAsync(new Collection<string>() { "65", "66" })).ToList();

            // Evaluate
            additionalrequirements = new List<Requirement>();
            overrides = new List<Override>();
            plannedcourses = new List<PlannedCredit>();
            ProgramEvaluation eval = new ProgramEvaluator(studentprogram, programrequirements, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();

            // Verify that the replacement course was applied
            var creditApplied = eval.RequirementResults.ElementAt(0).SubRequirementResults.ElementAt(0).GroupResults.ElementAt(0).GetApplied();
            Assert.AreEqual(1, creditApplied.Count());
            Assert.AreEqual("66", creditApplied.ElementAt(0).GetAcadCredId());
            // Replaced course is in other courses
            Assert.IsTrue(eval.OtherAcademicCredits.Contains("65"));
            Assert.IsTrue(eval.NotAppliedOtherAcademicCredits.Contains(credits[0]));
        }

        /// <summary>
        /// A Replaced course and replacement course are both included in the GPA because
        /// IncludeLowGradesInGpa is set to True at all levels
        /// </summary>
        [TestMethod]
        public async Task ReplacedCourse_IncludedInAllGPAs()
        {
            string studentId = "0004002";
            // Create academic program that allows courses of any level to be applied with a grade of B or higher
            string programCode = "MIN_GRADE_B";
            programrequirements = await programRequirementsRepo.GetAsync(programCode, "2013");
            studentprogram = await GetStudentProgram(studentId, programCode);
            // two academic credits for MUSC-210, 66 (grade A) replaces 65 (grade C)
            // But the replaced item is included in GPA because it has GPA values
            credits = (await academicCreditRepo.GetAsync(new Collection<string>() { "65", "66" })).ToList();

            // Evaluate
            additionalrequirements = new List<Requirement>();
            overrides = new List<Override>();
            plannedcourses = new List<PlannedCredit>();
            ProgramEvaluation eval = new ProgramEvaluator(studentprogram, programrequirements, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();

            // Verify that the replacement course was applied
            var creditApplied = eval.RequirementResults.ElementAt(0).SubRequirementResults.ElementAt(0).GroupResults.ElementAt(0).GetApplied();
            Assert.AreEqual(1, creditApplied.Count());
            Assert.AreEqual("66", creditApplied.ElementAt(0).GetAcadCredId());
            // Verify replaced course is to be included in the gpa
            var groupResult = eval.RequirementResults.ElementAt(0).SubRequirementResults.ElementAt(0).GroupResults.ElementAt(0);
            var creditsToInclude = groupResult.GetCreditsToIncludeInGpa();
            Assert.AreEqual(2, creditsToInclude.Count());
            Assert.IsTrue(creditsToInclude.Select(cr => cr.GetAcadCredId()).Contains("65"));
            Assert.IsTrue(creditsToInclude.Select(cr => cr.GetAcadCredId()).Contains("66"));
            Assert.AreEqual(3m, eval.CumGpa);
            Assert.AreEqual(3m, eval.InstGpa);
            Assert.AreEqual(3m, eval.RequirementResults.First().Gpa);
            Assert.AreEqual(3m, eval.RequirementResults.First().SubRequirementResults.First().Gpa);
            Assert.AreEqual(3m, eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First().Gpa);
        }

        /// <summary>
        /// Replaced course is not included in the GPA, even though it has GPA numbers,
        /// because IncludeLowGradesInGpa is set to False at all levels.
        /// </summary>
        [TestMethod]
        public async Task ReplacedCourse_ExcludedFromGroupGPA_IncludedInOverallGPA()
        {
            string studentId = "0004002";
            // Create academic program that allows courses of any level to be applied with a grade of B or higher
            string programCode = "MIN_GRADE_B";
            programrequirements = await programRequirementsRepo.GetAsync(programCode, "2013");
            // Modify program requirements to NOT include grades below the minimum
            foreach (var req1 in programrequirements.Requirements)
            {
                programrequirements.MinGrade = req1.MinGrade;
                req1.IncludeLowGradesInGpa = false;
                foreach (var subreq1 in req1.SubRequirements)
                {
                    subreq1.IncludeLowGradesInGpa = false;
                    foreach (var group1 in subreq1.Groups)
                    {
                        group1.IncludeLowGradesInGpa = false;
                    }
                }
            }
            studentprogram = await GetStudentProgram(studentId, programCode);
            // two academic credits for MUSC-210, 66 replaces 65
            credits = (await academicCreditRepo.GetAsync(new Collection<string>() { "65", "66" })).ToList();

            // Evaluate
            additionalrequirements = new List<Requirement>();
            overrides = new List<Override>();
            plannedcourses = new List<PlannedCredit>();
            ProgramEvaluation eval = new ProgramEvaluator(studentprogram, programrequirements, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();

            // Verify that the replacement course was applied
            var creditApplied = eval.RequirementResults.ElementAt(0).SubRequirementResults.ElementAt(0).GroupResults.ElementAt(0).GetApplied();
            Assert.AreEqual(1, creditApplied.Count());
            Assert.AreEqual("66", creditApplied.ElementAt(0).GetAcadCredId());
            // Verify replaced course is to be included in the gpa
            var groupResult = eval.RequirementResults.ElementAt(0).SubRequirementResults.ElementAt(0).GroupResults.ElementAt(0);
            var creditsToInclude = groupResult.GetCreditsToIncludeInGpa();
            Assert.AreEqual(1, creditsToInclude.Count());
            Assert.IsTrue(creditsToInclude.Select(cr => cr.GetAcadCredId()).Contains("66"));
            Assert.AreEqual(3m, eval.CumGpa);
            Assert.AreEqual(3m, eval.InstGpa);
            Assert.AreEqual(4m, eval.RequirementResults.First().Gpa);
            Assert.AreEqual(4m, eval.RequirementResults.First().SubRequirementResults.First().Gpa);
            Assert.AreEqual(4m, eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First().Gpa);
        }

        // Withdrawn courses should not be included in GPA even though low grades are included, because they have zero gpa credits
        [TestMethod]
        public async Task WithdrawnCourse_NotIncludedInGPA()
        {
            string studentId = "0004002";
            // Create academic program that allows courses of any level to be applied with a grade of B or higher
            string programCode = "MIN_GRADE_B";
            programrequirements = await programRequirementsRepo.GetAsync(programCode, "2013");
            studentprogram = await GetStudentProgram(studentId, programCode);
            // Withdrawn credit (zero GPA credits), one graded credit
            credits = (await academicCreditRepo.GetAsync(new Collection<string>() { "44", "66" })).ToList();

            // Evaluate
            additionalrequirements = new List<Requirement>();
            overrides = new List<Override>();
            plannedcourses = new List<PlannedCredit>();
            ProgramEvaluation eval = new ProgramEvaluator(studentprogram, programrequirements, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();

            // Verify that the graded credit with gpa values was applied
            var creditApplied = eval.RequirementResults.ElementAt(0).SubRequirementResults.ElementAt(0).GroupResults.ElementAt(0).GetApplied();
            Assert.AreEqual(1, creditApplied.Count());
            Assert.AreEqual("66", creditApplied.ElementAt(0).GetAcadCredId());
            // Verify only that item included in the gpa, not the withdrawn course
            var groupResult = eval.RequirementResults.ElementAt(0).SubRequirementResults.ElementAt(0).GroupResults.ElementAt(0);
            var creditsToInclude = groupResult.GetCreditsToIncludeInGpa();
            Assert.AreEqual(1, creditsToInclude.Count());
            Assert.IsTrue(creditsToInclude.Select(cr => cr.GetAcadCredId()).Contains("66"));
        }

        [TestMethod]
        public async Task GroupsEvaluatedInPrioritySequence()
        {
            studentid = "00004005";
            string programCode = "MULTIPLE_REQTYPE";
            studentprogram = await GetStudentProgram(studentid, programCode);
            programrequirements = await programRequirementsRepo.GetAsync(programCode, "2013");
            additionalrequirements = new List<Requirement>();
            plannedcourses = new List<PlannedCredit>();
            credits = (await academicCreditRepo.GetAsync(new Collection<string>() { "35", "41" })).ToList();
            overrides = new List<Override>();

            ProgramEvaluation eval = new ProgramEvaluator(studentprogram, programrequirements, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();
            var minGroupResults = eval.RequirementResults.Where(rr => rr.Requirement.RequirementType.Code == "MIN").Select(rmin => rmin.SubRequirementResults).First().Select(srr => srr.GroupResults).First();
            Assert.AreEqual(2, minGroupResults.First().Results.Count());
            var genGroupResults = eval.RequirementResults.Where(rr => rr.Requirement.RequirementType.Code == "GEN").Select(rmin => rmin.SubRequirementResults).First().Select(srr => srr.GroupResults).First();
            Assert.AreEqual(0, genGroupResults.First().Results.Count());
        }

        [TestMethod]
        public async Task GroupsEvaluatedInAppearanceSequence()
        {
            studentid = "00004005";
            string programCode = "SAME_REQTYPE_PRIORITY";
            studentprogram = await GetStudentProgram(studentid, programCode);
            programrequirements = await programRequirementsRepo.GetAsync(programCode, "2013");
            additionalrequirements = new List<Requirement>();
            plannedcourses = new List<PlannedCredit>();
            credits = (await academicCreditRepo.GetAsync(new Collection<string>() { "35", "41" })).ToList();
            overrides = new List<Override>();

            ProgramEvaluation eval = new ProgramEvaluator(studentprogram, programrequirements, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();
            var minGroupResults = eval.RequirementResults.Where(rr => rr.Requirement.RequirementType.Code == "CCD").Select(rmin => rmin.SubRequirementResults).First().Select(srr => srr.GroupResults).First();
            Assert.AreEqual(2, minGroupResults.First().Results.Count());
            var genGroupResults = eval.RequirementResults.Where(rr => rr.Requirement.RequirementType.Code == "MIN").Select(rmin => rmin.SubRequirementResults).First().Select(srr => srr.GroupResults).First();
            Assert.AreEqual(0, genGroupResults.First().Results.Count());
        }


        /// <summary>
        /// This is to test how many maximum  courses can be taken in a particular course levels to fulfil the subrequirement.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task MaxCoursesPerCourseLevels()
        {
            //       planned                credit
            string studentid = "00004002";    // MATH-100, MATH-200       ENGL-101, ENGL-102
            programrequirements = await GetRequirementsAsync("Test77");  //requires ENGL-101 and ENGL-102
            studentprogram = await GetStudentProgram(studentid, programid);
            additionalrequirements = new List<Requirement>();
            plannedcourses = await GetCourses(studentid);
            credits = await GetCredits(studentid);
            overrides = new List<Override>();

            ProgramEvaluation eval = new ProgramEvaluator(studentprogram, programrequirements, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();
            dumper.Dump(eval, "Verbose");

            Assert.IsTrue(eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First().GetApplied()
                                                    .Select(g => g.GetCourse().ToString()).Contains("ENGL*101"));


            Assert.IsTrue(eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First().GetApplied()
                                                    .Select(g => g.GetCourse().ToString()).Contains("ENGL*102"));

            Assert.IsFalse(eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First().GetApplied()
                                                  .Select(g => g.GetCourse().ToString()).Contains("MATH*100"));
            Assert.IsFalse(eval.IsSatisfied);

        }

        /// <summary>
        /// This is to test how many maximum  credits can be taken in a particular course levels to fulfil the subrequirement.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task MaxCreditsPerCourseLevels()
        {
            //               credit
            string studentid = "00004015";    // MATH-100, MATH-200       ENGL-101, ENGL-102,MATH-100,MATH-200
            programrequirements = await GetRequirementsAsync("Test78");  //requires ENGL-101 and ENGL-102,MATH-100,MATH-200
            studentprogram = await GetStudentProgram(studentid, programid);
            additionalrequirements = new List<Requirement>();
            plannedcourses = await GetCourses(studentid);
            credits = await GetCredits(studentid);
            overrides = new List<Override>();

            ProgramEvaluation eval = new ProgramEvaluator(studentprogram, programrequirements, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();
            dumper.Dump(eval, "Verbose");

            Assert.IsFalse(eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First().GetApplied()
                                                    .Select(g => g.GetCourse().ToString()).Contains("ENGL*101"));


            Assert.IsTrue(eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First().GetApplied()
                                                    .Select(g => g.GetCourse().ToString()).Contains("ENGL*102"));

            Assert.IsTrue(eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First().GetApplied()
                                                  .Select(g => g.GetCourse().ToString()).Contains("MATH*100"));
            Assert.IsTrue(eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First().GetApplied()
                                                 .Select(g => g.GetCourse().ToString()).Contains("MATH*200"));
            Assert.IsFalse(eval.IsSatisfied);

        }


        [TestMethod]
        public async Task ExtraCoursesForApplyOption()
        {
            string studentid = "00004015";    // MATH-100, MATH-200       ENGL-101, ENGL-102,MATH-100,MATH-200
            programrequirements = await GetRequirementsAsync("Test79");  //requires ENGL-101 and ENGL-102,MATH-100,MATH-200
            studentprogram = await GetStudentProgram(studentid, programid);
            additionalrequirements = new List<Requirement>();
            plannedcourses = await GetCourses(studentid);
            credits = await GetCredits(studentid);
            overrides = new List<Override>();
            var subReqmts = programrequirements.Requirements.SelectMany(a => a.SubRequirements);
            var groups = subReqmts.SelectMany(s => s.Groups);
            foreach (var group in groups)
            {
                group.ExtraCourseDirective = ExtraCourses.Apply;
            }
            ProgramEvaluation eval = new ProgramEvaluator(studentprogram, programrequirements, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();
            dumper.Dump(eval, "Verbose");

            Assert.IsTrue(eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First().GetApplied()
                                                    .Select(g => g.GetCourse().ToString()).Contains("ENGL*101"));


            Assert.IsTrue(eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First().GetApplied()
                                                    .Select(g => g.GetCourse().ToString()).Contains("ENGL*102"));

            Assert.IsTrue(eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First().GetApplied()
                                                  .Select(g => g.GetCourse().ToString()).Contains("MATH*100"));
            Assert.IsTrue(eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First().GetApplied()
                                                 .Select(g => g.GetCourse().ToString()).Contains("MATH*200"));

            Assert.IsTrue(eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First().GetApplied()
                                                   .First(g => g.GetCourse().ToString() == "ENGL*101").Explanation == AcadResultExplanation.Extra);
            Assert.IsTrue(eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First().GetApplied()
                                                  .First(g => g.GetCourse().ToString() == "ENGL*102").Explanation == AcadResultExplanation.None);
            Assert.IsTrue(eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First().GetApplied()
                                                   .First(g => g.GetCourse().ToString() == "MATH*100").Explanation == AcadResultExplanation.Extra);
            Assert.IsTrue(eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First().GetApplied()
                                                   .First(g => g.GetCourse().ToString() == "MATH*200").Explanation == AcadResultExplanation.Extra);

        }

        [TestMethod]
        public async Task ExtraCoursesForSemiApplyOption()
        {
            string studentid = "00004015";    // MATH-100, MATH-200       ENGL-101, ENGL-102,MATH-100,MATH-200
            programrequirements = await GetRequirementsAsync("Test79");  //requires ENGL-101 and ENGL-102,MATH-100,MATH-200
            studentprogram = await GetStudentProgram(studentid, programid);
            additionalrequirements = new List<Requirement>();
            plannedcourses = await GetCourses(studentid);
            credits = await GetCredits(studentid);
            overrides = new List<Override>();
            var subReqmts = programrequirements.Requirements.SelectMany(a => a.SubRequirements);
            var groups = subReqmts.SelectMany(s => s.Groups);
            foreach (var group in groups)
            {
                group.ExtraCourseDirective = ExtraCourses.SemiApply;
            }
            ProgramEvaluation eval = new ProgramEvaluator(studentprogram, programrequirements, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();
            dumper.Dump(eval, "Verbose");

            Assert.IsTrue(eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First().GetApplied()
                                                    .Select(g => g.GetCourse().ToString()).Contains("ENGL*101"));


            Assert.IsTrue(eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First().GetApplied()
                                                    .Select(g => g.GetCourse().ToString()).Contains("ENGL*102"));

            Assert.IsTrue(eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First().GetApplied()
                                                  .Select(g => g.GetCourse().ToString()).Contains("MATH*100"));
            Assert.IsTrue(eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First().GetApplied()
                                                 .Select(g => g.GetCourse().ToString()).Contains("MATH*200"));

            Assert.IsTrue(eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First().GetApplied()
                                                   .First(g => g.GetCourse().ToString() == "ENGL*101").Explanation == AcadResultExplanation.Extra);
            Assert.IsTrue(eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First().GetApplied()
                                                  .First(g => g.GetCourse().ToString() == "ENGL*102").Explanation == AcadResultExplanation.None);
            Assert.IsTrue(eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First().GetApplied()
                                                   .First(g => g.GetCourse().ToString() == "MATH*100").Explanation == AcadResultExplanation.Extra);
            Assert.IsTrue(eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First().GetApplied()
                                                   .First(g => g.GetCourse().ToString() == "MATH*200").Explanation == AcadResultExplanation.Extra);

        }

        //Min groups takes tests
        [TestMethod]
        public async Task MinGroupDefinedInSubRequirement_Take1()
        {
            studentid = "0000999";  //no plan, credits ENGL-101, MATH-460
            studentprogram = await GetStudentProgram(studentid, programid);
            additionalrequirements = new List<Requirement>();
            plannedcourses = await GetCourses(studentid);
            credits = await GetCredits(studentid);
            overrides = new List<Override>();

            reqnames.Add("MinGroupTestRequirement");
            subnames.Add("MinGroupTestSubrequirement");
            Subreq.Add(reqnames[0], subnames);
            grpnames.Add("Test30"); //all completed
            grpnames.Add("Test29");// all in-progress
            grpnames.Add("Test27");//not strated
            grpnames.Add("Test35");//planned, completed. Not started
            grpnames.Add("Test36");//planned. completed, in-progress
            grpnames.Add("Test51");//planned, completed,in-progress,not-started
            grpnames.Add("Test22");//completed
            grpnames.Add("Test32");//planned
            grpnames.Add("Test3");//completed, planned
            grpnames.Add("Test4");//completed, in-progress

            groups.Add(subnames[0], grpnames);
            programrequirements = await tprr.BuildTestProgramRequirementsAsync("TestProgramRequirementsId", reqnames, Subreq, groups);

            programrequirements.Requirements.First().SubRequirements.First().MinGroups = 1;  // One satisfied group satisfies Subrequirement

            ProgramEvaluation eval = new ProgramEvaluator(studentprogram, programrequirements, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();
            dumper.Dump(eval, "Verbose");

            // get the group and Subrequirement results
            GroupResult gr1 = eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First(gr => gr.Group.Code == "Test30");
            GroupResult gr2 = eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First(gr => gr.Group.Code == "Test29");
            GroupResult gr3 = eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First(gr => gr.Group.Code == "Test27");
            GroupResult gr4 = eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First(gr => gr.Group.Code == "Test35");
            GroupResult gr5 = eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First(gr => gr.Group.Code == "Test36");
            GroupResult gr6 = eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First(gr => gr.Group.Code == "Test51");
            GroupResult gr7 = eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First(gr => gr.Group.Code == "Test22");
            GroupResult gr8 = eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First(gr => gr.Group.Code == "Test32");
            GroupResult gr9 = eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First(gr => gr.Group.Code == "Test3");
            GroupResult gr10 = eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First(gr => gr.Group.Code == "Test4");

            SubrequirementResult sr1 = eval.RequirementResults.First().SubRequirementResults.First();

            // check group
            Assert.IsTrue(gr1.CompletionStatus == CompletionStatus.Completed);
            Assert.IsTrue(gr2.CompletionStatus == CompletionStatus.NotStarted);
            Assert.IsTrue(gr2.PlanningStatus == PlanningStatus.NotPlanned);
            Assert.IsTrue(gr3.CompletionStatus == CompletionStatus.NotStarted);
            Assert.IsTrue(gr3.PlanningStatus == PlanningStatus.NotPlanned);
            Assert.IsTrue(gr4.CompletionStatus == CompletionStatus.NotStarted);
            Assert.IsTrue(gr4.PlanningStatus == PlanningStatus.NotPlanned);
            Assert.IsTrue(gr5.CompletionStatus == CompletionStatus.NotStarted);
            Assert.IsTrue(gr5.PlanningStatus == PlanningStatus.NotPlanned);
            Assert.IsTrue(gr6.CompletionStatus == CompletionStatus.NotStarted);
            Assert.IsTrue(gr6.PlanningStatus == PlanningStatus.NotPlanned);
            Assert.IsTrue(gr7.CompletionStatus == CompletionStatus.NotStarted);
            Assert.IsTrue(gr7.PlanningStatus == PlanningStatus.NotPlanned);
            Assert.IsTrue(gr8.CompletionStatus == CompletionStatus.NotStarted);
            Assert.IsTrue(gr8.PlanningStatus == PlanningStatus.NotPlanned);
            Assert.IsTrue(gr9.CompletionStatus == CompletionStatus.NotStarted);
            Assert.IsTrue(gr9.PlanningStatus == PlanningStatus.NotPlanned);
            Assert.IsTrue(gr10.CompletionStatus == CompletionStatus.NotStarted);
            Assert.IsTrue(gr10.PlanningStatus == PlanningStatus.NotPlanned);

            // Check Acad Cred results

            Assert.IsTrue(gr1.Results.Where(x => x.Result == Result.Applied).Count() > 0);
            Assert.IsTrue(gr2.Results.Where(x => x.Result == Result.Applied).Count() == 0);
            Assert.IsTrue(gr3.Results.Where(x => x.Result == Result.Applied).Count() == 0);
            Assert.IsTrue(gr4.Results.Where(x => x.Result == Result.Applied).Count() == 0);
            Assert.IsTrue(gr5.Results.Where(x => x.Result == Result.Applied).Count() == 0);
            Assert.IsTrue(gr6.Results.Where(x => x.Result == Result.Applied).Count() == 0);
            Assert.IsTrue(gr7.Results.Where(x => x.Result == Result.Applied).Count() == 0);
            Assert.IsTrue(gr8.Results.Where(x => x.Result == Result.Applied).Count() == 0);
            Assert.IsTrue(gr9.Results.Where(x => x.Result == Result.Applied).Count() == 0);
            Assert.IsTrue(gr10.Results.Where(x => x.Result == Result.Applied).Count() == 0);

            // Check Subrequirement
            Assert.IsTrue(eval.RequirementResults.First().SubRequirementResults.First().CompletionStatus == CompletionStatus.Completed);
        }
        [TestMethod]
        public async Task MinGroupDefinedInSubRequirement_Take2()
        {
            studentid = "0000999";  //no plan, credits ENGL-101, MATH-460
            studentprogram = await GetStudentProgram(studentid, programid);
            additionalrequirements = new List<Requirement>();
            plannedcourses = await GetCourses(studentid);
            credits = await GetCredits(studentid);
            overrides = new List<Override>();

            reqnames.Add("MinGroupTestRequirement");
            subnames.Add("MinGroupTestSubrequirement");
            Subreq.Add(reqnames[0], subnames);
            grpnames.Add("Test30");
            grpnames.Add("Test29");
            grpnames.Add("Test27");
            grpnames.Add("Test35");
            grpnames.Add("Test36");
            grpnames.Add("Test51");
            grpnames.Add("Test22");
            grpnames.Add("Test32");
            grpnames.Add("Test3");
            grpnames.Add("Test4");

            groups.Add(subnames[0], grpnames);
            programrequirements = await tprr.BuildTestProgramRequirementsAsync("TestProgramRequirementsId", reqnames, Subreq, groups);

            programrequirements.Requirements.First().SubRequirements.First().MinGroups = 2;  // One satisfied group satisfies Subrequirement

            ProgramEvaluation eval = new ProgramEvaluator(studentprogram, programrequirements, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();
            dumper.Dump(eval, "Verbose");

            // get the group and Subrequirement results
            GroupResult gr1 = eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First(gr => gr.Group.Code == "Test30");
            GroupResult gr2 = eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First(gr => gr.Group.Code == "Test29");
            GroupResult gr3 = eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First(gr => gr.Group.Code == "Test27");
            GroupResult gr4 = eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First(gr => gr.Group.Code == "Test35");
            GroupResult gr5 = eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First(gr => gr.Group.Code == "Test36");
            GroupResult gr6 = eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First(gr => gr.Group.Code == "Test51");
            GroupResult gr7 = eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First(gr => gr.Group.Code == "Test22");
            GroupResult gr8 = eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First(gr => gr.Group.Code == "Test32");
            GroupResult gr9 = eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First(gr => gr.Group.Code == "Test3");
            GroupResult gr10 = eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First(gr => gr.Group.Code == "Test4");

            SubrequirementResult sr1 = eval.RequirementResults.First().SubRequirementResults.First();

            // check group
            Assert.IsTrue(gr1.CompletionStatus == CompletionStatus.Completed);
            Assert.IsTrue(gr2.CompletionStatus == CompletionStatus.Completed);

            Assert.IsTrue(gr3.CompletionStatus == CompletionStatus.NotStarted);
            Assert.IsTrue(gr3.PlanningStatus == PlanningStatus.NotPlanned);
            Assert.IsTrue(gr4.CompletionStatus == CompletionStatus.NotStarted);
            Assert.IsTrue(gr4.PlanningStatus == PlanningStatus.NotPlanned);
            Assert.IsTrue(gr5.CompletionStatus == CompletionStatus.NotStarted);
            Assert.IsTrue(gr5.PlanningStatus == PlanningStatus.NotPlanned);
            Assert.IsTrue(gr6.CompletionStatus == CompletionStatus.NotStarted);
            Assert.IsTrue(gr6.PlanningStatus == PlanningStatus.NotPlanned);
            Assert.IsTrue(gr7.CompletionStatus == CompletionStatus.NotStarted);
            Assert.IsTrue(gr7.PlanningStatus == PlanningStatus.NotPlanned);
            Assert.IsTrue(gr8.CompletionStatus == CompletionStatus.NotStarted);
            Assert.IsTrue(gr8.PlanningStatus == PlanningStatus.NotPlanned);
            Assert.IsTrue(gr9.CompletionStatus == CompletionStatus.NotStarted);
            Assert.IsTrue(gr9.PlanningStatus == PlanningStatus.NotPlanned);
            Assert.IsTrue(gr10.CompletionStatus == CompletionStatus.NotStarted);
            Assert.IsTrue(gr10.PlanningStatus == PlanningStatus.NotPlanned);

            // Check Acad Cred results

            Assert.IsTrue(gr1.Results.Where(x => x.Result == Result.Applied).Count() > 0);
            Assert.IsTrue(gr2.Results.Where(x => x.Result == Result.Applied).Count() > 0);

            Assert.IsTrue(gr3.Results.Where(x => x.Result == Result.Applied).Count() == 0);
            Assert.IsTrue(gr4.Results.Where(x => x.Result == Result.Applied).Count() == 0);
            Assert.IsTrue(gr5.Results.Where(x => x.Result == Result.Applied).Count() == 0);
            Assert.IsTrue(gr6.Results.Where(x => x.Result == Result.Applied).Count() == 0);
            Assert.IsTrue(gr7.Results.Where(x => x.Result == Result.Applied).Count() == 0);
            Assert.IsTrue(gr8.Results.Where(x => x.Result == Result.Applied).Count() == 0);
            Assert.IsTrue(gr9.Results.Where(x => x.Result == Result.Applied).Count() == 0);
            Assert.IsTrue(gr10.Results.Where(x => x.Result == Result.Applied).Count() == 0);

            // Check Subrequirement
            Assert.IsTrue(eval.RequirementResults.First().SubRequirementResults.First().CompletionStatus == CompletionStatus.Completed);
        }

        [TestMethod]
        public async Task MinGroupDefinedInSubRequirement_Take8()
        {
            studentid = "0000999";  //no plan, credits ENGL-101, MATH-460
            studentprogram = await GetStudentProgram(studentid, programid);
            additionalrequirements = new List<Requirement>();
            plannedcourses = await GetCourses(studentid);
            credits = await GetCredits(studentid);
            overrides = new List<Override>();

            reqnames.Add("MinGroupTestRequirement");
            subnames.Add("MinGroupTestSubrequirement");
            Subreq.Add(reqnames[0], subnames);
            grpnames.Add("Test30");
            grpnames.Add("Test29");
            grpnames.Add("Test27");
            grpnames.Add("Test35");
            grpnames.Add("Test36");
            grpnames.Add("Test51");
            grpnames.Add("Test22");
            grpnames.Add("Test32");
            grpnames.Add("Test3");
            grpnames.Add("Test4");

            groups.Add(subnames[0], grpnames);
            programrequirements = await tprr.BuildTestProgramRequirementsAsync("TestProgramRequirementsId", reqnames, Subreq, groups);

            programrequirements.Requirements.First().SubRequirements.First().MinGroups = 8;  // One satisfied group satisfies Subrequirement

            ProgramEvaluation eval = new ProgramEvaluator(studentprogram, programrequirements, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();
            dumper.Dump(eval, "Verbose");

            // get the group and Subrequirement results
            GroupResult gr1 = eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First(gr => gr.Group.Code == "Test30");
            GroupResult gr2 = eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First(gr => gr.Group.Code == "Test29");
            GroupResult gr3 = eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First(gr => gr.Group.Code == "Test27");
            GroupResult gr4 = eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First(gr => gr.Group.Code == "Test35");
            GroupResult gr5 = eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First(gr => gr.Group.Code == "Test36");
            GroupResult gr6 = eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First(gr => gr.Group.Code == "Test51");
            GroupResult gr7 = eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First(gr => gr.Group.Code == "Test22");
            GroupResult gr8 = eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First(gr => gr.Group.Code == "Test32");
            GroupResult gr9 = eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First(gr => gr.Group.Code == "Test3");
            GroupResult gr10 = eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First(gr => gr.Group.Code == "Test4");

            SubrequirementResult sr1 = eval.RequirementResults.First().SubRequirementResults.First();

            // check group
            Assert.IsTrue(gr1.CompletionStatus == CompletionStatus.Completed);
            Assert.IsTrue(gr2.CompletionStatus == CompletionStatus.Completed);
            Assert.IsTrue(gr3.CompletionStatus == CompletionStatus.Completed);
            Assert.IsTrue(gr4.CompletionStatus == CompletionStatus.Completed);
            Assert.IsTrue(gr5.CompletionStatus == CompletionStatus.Completed);
            Assert.IsTrue(gr6.CompletionStatus == CompletionStatus.Completed);
            Assert.IsTrue(gr7.CompletionStatus == CompletionStatus.Completed);
            Assert.IsTrue(gr8.CompletionStatus == CompletionStatus.Completed);

            Assert.IsTrue(gr9.CompletionStatus == CompletionStatus.NotStarted);
            Assert.IsTrue(gr9.PlanningStatus == PlanningStatus.NotPlanned);
            Assert.IsTrue(gr10.CompletionStatus == CompletionStatus.NotStarted);
            Assert.IsTrue(gr10.PlanningStatus == PlanningStatus.NotPlanned);

            // Check Acad Cred results

            Assert.IsTrue(gr1.Results.Where(x => x.Result == Result.Applied).Count() > 0);
            Assert.IsTrue(gr2.Results.Where(x => x.Result == Result.Applied).Count() > 0);
            Assert.IsTrue(gr3.Results.Where(x => x.Result == Result.Applied).Count() > 0);
            Assert.IsTrue(gr4.Results.Where(x => x.Result == Result.Applied).Count() > 0);
            Assert.IsTrue(gr5.Results.Where(x => x.Result == Result.Applied).Count() > 0);
            Assert.IsTrue(gr6.Results.Where(x => x.Result == Result.Applied).Count() > 0);
            Assert.IsTrue(gr7.Results.Where(x => x.Result == Result.Applied).Count() > 0);
            Assert.IsTrue(gr8.Results.Where(x => x.Result == Result.Applied).Count() > 0);

            Assert.IsTrue(gr9.Results.Where(x => x.Result == Result.Applied).Count() == 0);
            Assert.IsTrue(gr10.Results.Where(x => x.Result == Result.Applied).Count() == 0);

            // Check Subrequirement
            Assert.IsTrue(eval.RequirementResults.First().SubRequirementResults.First().CompletionStatus == CompletionStatus.Completed);
        }

        [TestMethod]
        public async Task MinGroupDefinedInSubRequirement_Take10()
        {
            studentid = "0000999";  //no plan, credits ENGL-101, MATH-460
            studentprogram = await GetStudentProgram(studentid, programid);
            additionalrequirements = new List<Requirement>();
            plannedcourses = await GetCourses(studentid);
            credits = await GetCredits(studentid);
            overrides = new List<Override>();

            reqnames.Add("MinGroupTestRequirement");
            subnames.Add("MinGroupTestSubrequirement");
            Subreq.Add(reqnames[0], subnames);
            grpnames.Add("Test30");
            grpnames.Add("Test29");
            grpnames.Add("Test27");
            grpnames.Add("Test35");
            grpnames.Add("Test36");
            grpnames.Add("Test51");
            grpnames.Add("Test22");
            grpnames.Add("Test32");
            grpnames.Add("Test3");
            grpnames.Add("Test4");

            groups.Add(subnames[0], grpnames);
            programrequirements = await tprr.BuildTestProgramRequirementsAsync("TestProgramRequirementsId", reqnames, Subreq, groups);

            programrequirements.Requirements.First().SubRequirements.First().MinGroups = 10;  // One satisfied group satisfies Subrequirement

            ProgramEvaluation eval = new ProgramEvaluator(studentprogram, programrequirements, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();
            dumper.Dump(eval, "Verbose");

            // get the group and Subrequirement results
            GroupResult gr1 = eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First(gr => gr.Group.Code == "Test30");
            GroupResult gr2 = eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First(gr => gr.Group.Code == "Test29");
            GroupResult gr3 = eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First(gr => gr.Group.Code == "Test27");
            GroupResult gr4 = eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First(gr => gr.Group.Code == "Test35");
            GroupResult gr5 = eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First(gr => gr.Group.Code == "Test36");
            GroupResult gr6 = eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First(gr => gr.Group.Code == "Test51");
            GroupResult gr7 = eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First(gr => gr.Group.Code == "Test22");
            GroupResult gr8 = eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First(gr => gr.Group.Code == "Test32");
            GroupResult gr9 = eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First(gr => gr.Group.Code == "Test3");
            GroupResult gr10 = eval.RequirementResults.First().SubRequirementResults.First().GroupResults.First(gr => gr.Group.Code == "Test4");

            SubrequirementResult sr1 = eval.RequirementResults.First().SubRequirementResults.First();

            // check group
            Assert.IsTrue(gr1.CompletionStatus == CompletionStatus.Completed);
            Assert.IsTrue(gr2.CompletionStatus == CompletionStatus.Completed);
            Assert.IsTrue(gr3.CompletionStatus == CompletionStatus.Completed);
            Assert.IsTrue(gr4.CompletionStatus == CompletionStatus.Completed);
            Assert.IsTrue(gr5.CompletionStatus == CompletionStatus.Completed);
            Assert.IsTrue(gr6.CompletionStatus == CompletionStatus.Completed);
            Assert.IsTrue(gr7.CompletionStatus == CompletionStatus.Completed);
            Assert.IsTrue(gr8.CompletionStatus == CompletionStatus.Completed);

            Assert.IsTrue(gr9.CompletionStatus == CompletionStatus.PartiallyCompleted);
            Assert.IsTrue(gr10.CompletionStatus == CompletionStatus.PartiallyCompleted);

            // Check Acad Cred results

            Assert.IsTrue(gr1.Results.Where(x => x.Result == Result.Applied).Count() > 0);
            Assert.IsTrue(gr2.Results.Where(x => x.Result == Result.Applied).Count() > 0);
            Assert.IsTrue(gr3.Results.Where(x => x.Result == Result.Applied).Count() > 0);
            Assert.IsTrue(gr4.Results.Where(x => x.Result == Result.Applied).Count() > 0);
            Assert.IsTrue(gr5.Results.Where(x => x.Result == Result.Applied).Count() > 0);
            Assert.IsTrue(gr6.Results.Where(x => x.Result == Result.Applied).Count() > 0);
            Assert.IsTrue(gr7.Results.Where(x => x.Result == Result.Applied).Count() > 0);
            Assert.IsTrue(gr8.Results.Where(x => x.Result == Result.Applied).Count() > 0);

            Assert.IsTrue(gr9.Results.Where(x => x.Result == Result.Applied).Count() > 0);
            Assert.IsTrue(gr10.Results.Where(x => x.Result == Result.Applied).Count() > 0);

            // Check Subrequirement
            Assert.IsTrue(eval.RequirementResults.First().SubRequirementResults.First().CompletionStatus == CompletionStatus.PartiallyCompleted);
        }

        
        /// <summary>
        /// ProgramTests
        /// These tests use ProgramTestsData.xml. This test data file is best modified by using Visual Studio's built-in XML editor. 
        /// Steps to modify the XML file:
        ///     	* In api, right click XML file in visual studio and "check out for edit"
        ///     	* Double-click on the XML file to open it within Visual Studio
        ///     	* Save and close the XML file when complete
        ///         (check in as normal when all work complete)
        /// The test data consists of the following data:
        ///     TestNumber: Sequence number simply enables quick determination in debugger which test your are on
        ///     Description: Brief description of test for debugging reference
        ///     Taken: Academic credits to be used as input to eval, using course name. The TestAcademicCredit repo provides logic
        ///         in the method that will take course names and select academic credits from the static test repository. (Use asterisk
        ///         delimiter between subject and number)
        ///     Planned: Planned courses to be used as input to eval, using course Id. You can lookup the course in the TestCourseRepository.cs
        ///     AuditResult: The expected overall evaluation result, must be a value from ProgramRequirementsExplanation enum (but not currently checked)
        ///     REQUIREMENTS - The next set of columns repeats 3 times, for up to 3 requirements in a test. The N at the end of each column name
        ///         is the iteration set (1,2,3)
        ///         RequirementN: The name of the requirement. If the requirement does not allow course reuse by its subrequirements, 
        ///             append :N to the requirement name, default is Y. Append a third value for min inst credits, such as :10.5. 
        ///             All requirement names in a row must be unique.
        ///         SubrequirementN: The name of the subrequirements, separated by commas. If a subrequirement does not allow course 
        ///             reuse by its groups, append ":N" to the subrequirement name, default is Y. Append a third value for min inst credits, such as :10.5. 
        ///             All subrequirement names in a row must be unique.
        ///         GroupsN: The Id of the groups within each subrequirement. A pipe (|) indicates where the groups of one subreq
        ///            end and the groups of the next subreq begin. There MUST be at least one group for each subreq. The group
        ///            number corresponds to the groups named Test... in the TestProgramRequirementsRepository BuildGroup method
        ///            switch statement. All group names must be unique. Therefore, if you want to use a given group multiple times 
        ///            in a single row row, append Xn to the group number. ie, 57X1, 57X2, 57X3 all use the group defined as Test57.
        ///         RequirementResultN: The expected requirement result Explanation. Must exactly match a value in RequirementExplanation enum
        ///         SubrequirementResultsN: The expected Explanation for each subrequirement result, designated by subrequirement name. Must
        ///            exactly match a value in the SubrequirementExplanation enum
        ///         GroupResultsN: The expected Explanation for each group result, designated by group name. Must exactly match a value
        ///            in the GroupExplanation enum. Separate multiple explanations for a group with a forward slash (/)
        ///         GroupAppliedN: The expected course names applied to each group (use dash delimiter between subject and number). Separate multiple
        ///            applied courses for a group with a forward slash (/)
        /// </summary>
        [TestMethod]
        [DeploymentItem("Services\\TestData\\ProgramTestsData.xml")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\ProgramTestsData.xml", "TestData", DataAccessMethod.Sequential)]
        public async Task ProgramTests()
        {
            TestContext tc = this.TestContext;

            int testNumber = System.Convert.ToInt32(TestContext.DataRow["TestNumber"]);
            string description = System.Convert.ToString(TestContext.DataRow["Description"]);
            string strTaken = System.Convert.ToString(TestContext.DataRow["Taken"]); // List of academic credits using course name
            string strPlanned = System.Convert.ToString(TestContext.DataRow["Planned"]); // List of planned courses using course Id
            string expectedAuditResult = System.Convert.ToString(TestContext.DataRow["AuditResult"]);//.ToUpper();
            string studentid = "ignored for these tests";

            Console.WriteLine("TestNumber: " + testNumber);
            Console.WriteLine("Description: " + description);
            Console.WriteLine("Taken: " + strTaken);
            Console.WriteLine("Expected Audit  Result: " + expectedAuditResult);

            // Here is a tree of strings for holding all the names of stuff so we can examine them later.  Can I just get this later
            // from what comes out of the repository?  Probably.  But I'm testing that, no?  

            List<string> requirementNames = new List<string>();
            Dictionary<string, string> expectedRequirementResults = new Dictionary<string, string>();
            // REQUIREMENT1 =>  [ SubreqUIREMENT1,SubreqUIREMENT2 ]
            Dictionary<string, List<string>> subrequirementNames = new Dictionary<string, List<string>>();
            Dictionary<string, string> expectedSubrequirementResults = new Dictionary<string, string>(); // subreq names must be unique across all requirements
                                                                                                         // SubreqUIREMENT1 =>  [ Group1,Group2 ]  ("Group" is implied in the XML file, but will be added here for clarity)
            Dictionary<string, List<string>> groupNames = new Dictionary<string, List<string>>();
            Dictionary<string, string> expectedGroupResults = new Dictionary<string, string>(); // group names must be unique across all requirements. Use Xn notation to use group multiple times in spreadsheet
            Dictionary<string, string> expectedGroupApplied = new Dictionary<string, string>(); // courses expected, listed by course name
            var plannedCredits = new List<PlannedCredit>();

            // up to 3 possible requirements for now.   This could be done more neatly with multiple tables and child rows
            // but who wants to deal with that?

            for (int i = 1; i <= 3; i++)
            {
                string reqno = i.ToString();

                if (TestContext.DataRow["Requirement" + reqno].ToString() != "")
                {
                    string reqName = System.Convert.ToString(TestContext.DataRow["Requirement" + reqno]);//.ToUpper();
                    requirementNames.Add(reqName);
                    reqName = reqName.Split(':')[0];
                    expectedRequirementResults.Add(reqName, System.Convert.ToString(TestContext.DataRow["RequirementResult" + reqno]));

                    string Subreqs = System.Convert.ToString(TestContext.DataRow["Subrequirement" + reqno]);//.ToUpper();
                    if (Subreqs != null)
                    {
                        List<string> subreqNames = Subreqs.Split(',').ToList();
                        subrequirementNames.Add(reqName, subreqNames);

                        // Load the subreq expected results into a dictionary
                        string subreqExpectedResultsString = System.Convert.ToString(TestContext.DataRow["SubrequirementResults" + reqno]);
                        List<string> subreqExpectedResultsList = subreqExpectedResultsString.Split(',').ToList();
                        foreach (var subreqResultString in subreqExpectedResultsList)
                        {
                            if (!string.IsNullOrEmpty(subreqResultString))
                            {
                                List<string> subreqNameValuePair = subreqResultString.Split('=').ToList();
                                if (expectedSubrequirementResults.ContainsKey(subreqNameValuePair.ElementAt(0)))
                                {
                                    throw new ArgumentOutOfRangeException("Cannot duplicate subrequirement name in test data");
                                }
                                expectedSubrequirementResults.Add(subreqNameValuePair.ElementAt(0), subreqNameValuePair.ElementAt(1));
                            }
                        }

                        string groupsetstr = System.Convert.ToString(TestContext.DataRow["Groups" + reqno]);//.ToUpper();

                        for (int j = 0; j < subreqNames.Count(); j++)
                        {
                            string Subreqname = subreqNames.ElementAt(j).Split(':')[0]; // get only the name, not the allow course reuse indicator
                            string groupstr = groupsetstr.Split('|').ElementAt(j);
                            Console.WriteLine("  Subrequirement Name: " + Subreqname + " has groups: " + groupstr);
                            List<string> grouplist = new List<string>();
                            grouplist = groupstr.Split(',').ToList<string>();
                            // Convert the numeric group into concatenated code 'Group'+n
                            for (int k = 0; k < grouplist.Count(); k++)
                            {
                                grouplist[k] = "Group" + grouplist[k];
                            }
                            groupNames.Add(Subreqname, grouplist);
                        }

                        // Load the group expected results into a dictionary
                        string groupExpectedResultsString = System.Convert.ToString(TestContext.DataRow["GroupResults" + reqno]);
                        List<string> groupExpectedResultList = groupExpectedResultsString.Split('|').ToList();
                        foreach (var groupResultString in groupExpectedResultList)
                        {
                            List<string> groupResultList = groupResultString.Split(',').ToList();
                            foreach (var resultString in groupResultList)
                            {
                                if (!string.IsNullOrEmpty(resultString))
                                {
                                    List<string> groupNameValuePair = resultString.Split('=').ToList();
                                    if (expectedGroupResults.ContainsKey(groupNameValuePair.ElementAt(0)))
                                    {
                                        throw new ArgumentOutOfRangeException("Cannot duplicate group name in expected group results test data");
                                    }
                                    expectedGroupResults.Add(groupNameValuePair.ElementAt(0), groupNameValuePair.ElementAt(1));
                                }
                            }
                        }

                        // Load the group expected applied into a dictionary
                        string groupExpectedAppliedString = System.Convert.ToString(TestContext.DataRow["GroupApplied" + reqno]);
                        List<string> groupExpectedAppliedList = groupExpectedAppliedString.Split('|').ToList();
                        foreach (var groupAppliedString in groupExpectedAppliedList)
                        {
                            List<string> groupAppliedList = groupAppliedString.Split(',').ToList();
                            foreach (var appliedString in groupAppliedList)
                            {
                                if (!string.IsNullOrEmpty(appliedString))
                                {
                                    List<string> groupNameValuePair = appliedString.Split('=').ToList();
                                    if (expectedGroupApplied.ContainsKey(groupNameValuePair.ElementAt(0)))
                                    {
                                        throw new ArgumentOutOfRangeException("Cannot duplicate group name in expected group applied test data");
                                    }
                                    expectedGroupApplied.Add(groupNameValuePair.ElementAt(0), groupNameValuePair.ElementAt(1));
                                }
                            }
                        }
                    }
                }
            }

            var creditIds = strTaken.Split(',').ToList<string>();

            programrequirements = await new TestProgramRequirementsRepository().BuildTestProgramRequirementsAsync("MATH.BS*2013", requirementNames, subrequirementNames, groupNames);

            studentprogram = await GetStudentProgram(studentid, programid);
            additionalrequirements = new List<Requirement>();
            credits = (await academicCreditRepo.GetAsync(creditIds)).ToList();
            overrides = new List<Override>();

            foreach (var courseId in strPlanned.Split(','))
            {
                if (!string.IsNullOrEmpty(courseId))
                {
                    plannedCredits.Add(new PlannedCredit((await courseRepo.GetAsync(courseId)), "2017/SP"));
                }
            }

            ProgramEvaluation eval = new ProgramEvaluator(studentprogram, programrequirements, additionalrequirements, credits, plannedCredits, ruleResults, overrides, courses, logger).Evaluate();
            // Dump some of the structure to test results console so you can see what
            // happened to each course/credit/group/requirement etc.
            if (testNumber == 6)
            {
                var xxx = 1;
            }

            dumper.Dump(eval, "verbose");

            foreach (RequirementResult rres in eval.RequirementResults)
            {
                // Verify each expected requirement explanation
                string reqName = rres.Requirement.ToString();
                string expectedResult = expectedRequirementResults[reqName];
                try
                {
                    var expected = (RequirementExplanation)Enum.Parse(typeof(RequirementExplanation), expectedResult);
                    Assert.IsTrue(rres.Explanations.Contains(expected));
                }
                catch (ArgumentException)
                {
                    throw new ArgumentException("Test data for requirement " + reqName + " is missing an expected result.");
                }

                // Verify each expected subrequirement explanation
                foreach (var subreqRes in rres.SubRequirementResults)
                {
                    string subreqName = subreqRes.SubRequirement.ToString();
                    if (expectedSubrequirementResults.ContainsKey(subreqName))
                    {
                        string subreqExpectedResult = expectedSubrequirementResults[subreqName];
                        try
                        {
                            var expected = (SubrequirementExplanation)Enum.Parse(typeof(SubrequirementExplanation), subreqExpectedResult);
                            Assert.IsTrue(subreqRes.Explanations.Contains(expected));
                        }
                        catch (ArgumentException)
                        {
                            throw new ArgumentException("Test data for subrequirement " + subreqName + " is invalid.");
                        }
                    }

                    // Verify each expected group explanation
                    foreach (var groupRes in subreqRes.GroupResults)
                    {
                        string groupName = groupRes.Group.Code;
                        if (expectedGroupResults.ContainsKey(groupName))
                        {
                            string groupExpectedResult = expectedGroupResults[groupName];
                            // If there are multiple possible group results, they will be separated by a forward slash
                            var groupExpectedResultList = groupExpectedResult.Split('/');
                            for (int g = 0; g < groupExpectedResultList.Count(); g++)
                            {
                                try
                                {
                                    var expected = (GroupExplanation)Enum.Parse(typeof(GroupExplanation), groupExpectedResultList.ElementAt(g));
                                    Assert.IsTrue(groupRes.Explanations.Contains(expected));
                                }
                                catch (ArgumentException)
                                {
                                    throw new ArgumentException("Test data for group " + groupName + " is invalid.");
                                }
                            }
                        }

                        // Verify each expected group applied acad credit/planned course
                        if (expectedGroupApplied.ContainsKey(groupName))
                        {
                            string groupExpectedApplied = expectedGroupApplied[groupName];
                            // If there are multiple possible group applied, they will be separated by a forward slash
                            var groupExpectedAppliedList = groupExpectedApplied.Split('/');
                            for (int g = 0; g < groupExpectedAppliedList.Count(); g++)
                            {
                                try
                                {
                                    var expected = groupExpectedAppliedList.ElementAt(g);
                                    var appliedCourseNames = groupRes.GetApplied().Select(ac => ac.GetCourse()).Select(c => c.Name).ToList();
                                    appliedCourseNames.AddRange(groupRes.GetPlannedApplied().Select(ac => ac.GetCourse()).Select(c => c.Name));
                                    if (expected == "NONE")
                                    {
                                        Assert.IsTrue(appliedCourseNames.Count() == 0);
                                    }
                                    else
                                    {
                                        Assert.IsTrue(appliedCourseNames.Contains(expected));
                                    }
                                }
                                catch (ArgumentException)
                                {
                                    throw new ArgumentException("Test data for group " + groupName + " is invalid.");
                                }
                            }
                        }
                    }
                }
            }
        }

        private async Task<ProgramRequirements> BuildProgramRequirementsAsync(string id, string grpname = "Test55")
        {
            // This creates a barebones PR object with one req, one Subreq, one group
            // The group requirement spec is , "TAKE 2 COURSES, MIN_GRADE A, AU, P"

            List<string> reqnames = new List<string>();
            List<string> subnames = new List<string>();
            List<string> grpnames = new List<string>();
            Dictionary<string, List<string>> Subreq = new Dictionary<string, List<string>>();
            Dictionary<string, List<string>> groups = new Dictionary<string, List<string>>();

            // This is confusing but it makes sense in its own way
            reqnames.Add("GroupTestRequirement");           // list of requirement names to handle
            subnames.Add("GroupTestSubrequirement");        // list of Subrequirement names
            Subreq.Add(reqnames[0], subnames);              // dictionary as pointer from requirement name to list of Subrequirement names under it
            grpnames.Add(grpname);                          // list of group names
            groups.Add(subnames[0], grpnames);              // dictionary as pointer from Subrequirement name to list of group names under it

            ProgramRequirements pr = await new TestProgramRequirementsRepository().BuildTestProgramRequirementsAsync(id, reqnames, Subreq, groups);
            return pr;
        }

        private async Task<List<AcademicCredit>> GetCredits(string id)
        {
            var s = studentRepo.Get(id);
            List<AcademicCredit> acadcreds = (await academicCreditRepo.GetAsync(s.AcademicCreditIds)).ToList();
            return acadcreds;
        }

        private async Task<ProgramRequirements> GetRequirementsAsync()
        {
            ProgramRequirements pr = await BuildProgramRequirementsAsync("MATH.BS*2013");
            return pr;
        }

        private async Task<ProgramRequirements> GetRequirementsAsync(string grpname)
        {
            ProgramRequirements pr = await BuildProgramRequirementsAsync("MATH.BS*2013", grpname);
            return pr;
        }
        private async Task<List<PlannedCredit>> GetCourses(string id)
        {
            var plannedCourses = new List<PlannedCredit>();

            var s = studentRepo.Get(id);
            if (s.DegreePlanId != null)
            {
                // Get the degree plan
                List<string> courseids = new List<string>();
                var dp = await studentDegreePlanRepo.GetAsync((int)s.DegreePlanId);

                // Check each term for planned courses
                foreach (var term in dp.TermIds)
                {
                    IEnumerable<PlannedCourse> plannedtermcourses = dp.GetPlannedCourses(term).Where(c => !string.IsNullOrEmpty(c.CourseId));

                    foreach (var plannedCourse in plannedtermcourses)
                    {
                        var crs = await courseRepo.GetAsync(plannedCourse.CourseId);
                        var evalPlannedCourse = new PlannedCredit(crs, term);
                        plannedCourses.Add(evalPlannedCourse);
                    }
                }

            }
            return plannedCourses;
        }

        private async Task<List<Override>> GetOverrides(string id, string pid)
        {
            StudentProgram sp = await studentProgramRepo.GetAsync(id, pid);
            if (sp.Overrides != null && sp.Overrides.Count() > 0)
            {
                return sp.Overrides.ToList();
            }
            return new List<Override>();
        }

        private async Task<StudentProgram> GetStudentProgram(string id, string pid)
        {
            return await studentProgramRepo.GetAsync(id, pid);
        }
        private async Task<StudentProgram> GetStudentProgram(string id, string pid, List<Override> overrides, List<RequirementModification> requirementmodifications)
        {
            StudentProgram sp = await studentProgramRepo.GetAsync(id, pid);
            if (overrides != null)
            {
                foreach (var over in overrides) { sp.AddOverride(over); }
            }
            if (requirementmodifications != null)
            {
                foreach (var mod in requirementmodifications) { sp.AddRequirementModification(mod); }
            }
            return sp;
        }


        [TestClass]
        public class ProgramEvaluatorConstructor_Prerequisite
        {
            private IStudentRepository studentRepo;
            private IAcademicCreditRepository academicCreditRepo;
            private ICourseRepository courseRepo;
            private IRequirementRepository requirementRepo;
            private Ellucian.Colleague.Domain.Student.Entities.Student student;
            private List<Ellucian.Colleague.Domain.Student.Entities.AcademicCredit> credits;
            private List<PlannedCredit> plannedCourses;
            private List<Ellucian.Colleague.Domain.Student.Entities.Requirements.Requirement> requirements;
            private IEnumerable<RuleResult> ruleResults;
            private IEnumerable<Course> courses;

            [TestInitialize]
            public async void Initialize()
            {
                studentRepo = new TestStudentRepository();
                academicCreditRepo = new TestAcademicCreditRepository();
                courseRepo = new TestCourseRepository();
                requirementRepo = new TestRequirementRepository();
                string studentId = "00004002";
                student = studentRepo.Get(studentId);
                requirements = new List<Ellucian.Colleague.Domain.Student.Entities.Requirements.Requirement>();
                requirements.Add(await requirementRepo.GetAsync("PREREQ1"));
                var courseList = new List<string>() { "139", "42", "110" };
                var repoCourses = (await courseRepo.GetAsync(courseList)).ToList();
                plannedCourses = new List<PlannedCredit>();
                foreach (var crs in repoCourses)
                {
                    plannedCourses.Add(new PlannedCredit(crs, "2009/FA"));
                }
                credits = (await academicCreditRepo.GetAsync(student.AcademicCreditIds)).ToList();
                ruleResults = new List<RuleResult>();
                courses = await new TestCourseRepository().GetAsync();
            }

            [TestMethod]
            public void ProgramEvaluatorConstructor()
            {
                var evaluator = new ProgramEvaluator(requirements, credits, plannedCourses, ruleResults, courses);
                Assert.IsTrue(evaluator is ProgramEvaluator);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ProgramEvaluatorConstructor_ThrowsExceptionIfRequirementsNull()
            {
                var evaluator = new ProgramEvaluator(null, credits, plannedCourses, ruleResults, courses);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ProgramEvaluatorConstructor_ThrowsExceptionIfRequirementsEmpty()
            {
                var evaluator = new ProgramEvaluator(new List<Requirement>(), credits, plannedCourses, ruleResults, courses);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ProgramEvaluatorConstructor_ThrowsExceptionIfCreditsNull()
            {
                var evaluator = new ProgramEvaluator(requirements, null, plannedCourses, ruleResults, courses);
            }

               [TestMethod]
               [ExpectedException(typeof(ArgumentNullException))]
               public void ProgramEvaluatorConstructor_ThrowsExceptionIfPlannedCoursesNull()
               {
                    var evaluator = new ProgramEvaluator(requirements, credits, null, ruleResults, courses);
               }
          }

        [TestClass]
        public class ProgramEvaluatorRequirmentExclusionsWithMinGroupsAndCourseReuse : ProgramEvaluationTests
        {
            //PROG.COURSE.REUSE
            //Scenario 1: Req1->SubReq1 - type GEN - excludes MAJ
            //          Take 1 group;
            //  Take ENGL-200 ENGL-300 COMM-100; (type 30)
            //  Take 2 courses;
            //  From Subject COMM;  (type 32)
            //Course Reuse = "Y" at subrequirement and group level (REQU, AEVP)

            //Req 2-> Sub Req1 - type MAJ - excludes GEN
            //Take 2 courses (type 32)

            //Req1 excludes Req2 and vice-versa

            //Acad credits
            //ENGL-200  COMPLETED
            //ENGL-300  COMPLETED
            //COMM-1321  COMPLETED
            //COMM-100  COMPLETED
            //MATH-1004  IN-PROGRESS

            //when first Req1 will be evaluated then COMM-1321 SHOULD be freed and be available to Req 2 but ENGL-200, ENGL-300 COMM-100 should not be available to Req2
            //With the bug in 1.19 ENGL-200 and ENGL-300 were available for req 2

            [TestMethod]
            public async Task ExclusionWithMinGroupsCoursReuseY()
            {
                
                studentid = "0000111";  
                studentprogram = await GetStudentProgram(studentid, "PROG.COURSE.REUSE");
                additionalrequirements = new List<Requirement>();
                plannedcourses = new List<PlannedCredit>();
                credits = await GetCredits(studentid);

                overrides = new List<Override>();

                ProgramRequirements pr = await programRequirementsRepo.GetAsync("PROG.COURSE.REUSE", "2013");

                ProgramEvaluation eval = new ProgramEvaluator(studentprogram, pr, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();

                Assert.IsNotNull(eval);
                Assert.AreEqual(eval.RequirementResults[0].PlanningStatus , PlanningStatus.CompletelyPlanned);
                Assert.AreEqual(eval.RequirementResults[0].CompletionStatus, CompletionStatus.Completed);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].PlanningStatus, PlanningStatus.CompletelyPlanned);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].CompletionStatus, CompletionStatus.Completed);

                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].PlanningStatus, PlanningStatus.CompletelyPlanned);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].CompletionStatus, CompletionStatus.Completed);

                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results.First(r => r.GetCourse().Name == "ENGL-200").Result, Result.Applied);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results.First(r => r.GetCourse().Name == "ENGL-300").Result, Result.Applied);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results.First(r => r.GetCourse().Name == "COMM-100").Result, Result.Applied);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results.First(r => r.GetCourse().Name == "COMM-1321").Result, Result.NotInCoursesList);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results.First(r => r.GetCourse().Name == "MATH-1004").Result, Result.NotInCoursesList);

                //This group gets cleaned
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[1].PlanningStatus, PlanningStatus.NotPlanned);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[1].CompletionStatus, CompletionStatus.NotStarted);

                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[1].Results.First(r => r.GetCourse().Name == "ENGL-200").Result, Result.FromWrongDepartment);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[1].Results.First(r => r.GetCourse().Name == "ENGL-300").Result, Result.FromWrongDepartment);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[1].Results.First(r => r.GetCourse().Name == "COMM-100").Result, Result.Related);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[1].Results.First(r => r.GetCourse().Name == "COMM-1321").Result, Result.Related);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[1].Results.First(r => r.GetCourse().Name == "MATH-1004").Result, Result.FromWrongDepartment);


                //2nd requirment - subrequirment - COMM*1321 and Math are applied here

                Assert.AreEqual(eval.RequirementResults[1].SubRequirementResults[0].GroupResults[0].PlanningStatus, PlanningStatus.CompletelyPlanned);
                Assert.AreEqual(eval.RequirementResults[1].SubRequirementResults[0].GroupResults[0].CompletionStatus, CompletionStatus.PartiallyCompleted);

                Assert.AreEqual(eval.RequirementResults[1].SubRequirementResults[0].GroupResults[0].Results.First(r => r.GetCourse().Name == "COMM-1321").Result, Result.Applied);
                Assert.AreEqual(eval.RequirementResults[1].SubRequirementResults[0].GroupResults[0].Results.First(r => r.GetCourse().Name == "MATH-1004").Result, Result.Applied);


            }


            //Another Scenario found with client in 1.19

            //req-1 (MAJ)  priority 1 with course reuse=N
            //excludes ELE, GEN, MIN, MAJ
            //has 2 subrequirments with course reuse = N
            //Subrequirment 1
            //Take 1 group;
            //  Take 1 course from ENGL-200 ENGL-300;  (block type 31)
            //  TAKE 3 COURSES FROM COMM;  (33)

            //Subrequirment 2
            //Take 1 group;
            //#take 1 course from COMM-1315 COMM-100 COMM-1321;  (31)
            //#TAKE 1 COURSE FROM department ART;  (33)


            //Requirment 2 type ELE 
            //excludes MAJ, MIN, GEN, ELE
            //subrequirment -1 
            //Take 3 courses

            //Acad credits
            //ENGL-200  COMPLETED
            //ENGL-300  COMPLETED
            //COMM-1321  COMPLETED
            //COMM-100  COMPLETED
            //MATH-1004  IN-PROGRESS
            [TestMethod]
            public async Task ExclusionWithMinGroupsAcrossSubrequirmentsWithCourseReuseN()
            {

                studentid = "0000111";
                studentprogram = await GetStudentProgram(studentid, "PROG.COURSE.REUSE.2");
                additionalrequirements = new List<Requirement>();
                plannedcourses = new List<PlannedCredit>();
                credits = await GetCredits(studentid);

                overrides = new List<Override>();

                ProgramRequirements pr = await programRequirementsRepo.GetAsync("PROG.COURSE.REUSE.2", "2013");

                ProgramEvaluation eval = new ProgramEvaluator(studentprogram, pr, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();
                Assert.IsNotNull(eval);
                Assert.AreEqual(eval.RequirementResults[0].PlanningStatus, PlanningStatus.PartiallyPlanned);
                Assert.AreEqual(eval.RequirementResults[0].CompletionStatus, CompletionStatus.PartiallyCompleted);

                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].PlanningStatus, PlanningStatus.CompletelyPlanned);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].CompletionStatus, CompletionStatus.Completed);

                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].PlanningStatus, PlanningStatus.CompletelyPlanned);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].CompletionStatus, CompletionStatus.Completed);

                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results.First(r => r.GetCourse().Name == "ENGL-200").Result, Result.Related);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results.First(r => r.GetCourse().Name == "ENGL-300").Result, Result.Applied);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results.First(r => r.GetCourse().Name == "COMM-100").Result, Result.NotInCoursesList);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results.First(r => r.GetCourse().Name == "COMM-1321").Result, Result.NotInCoursesList);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results.First(r => r.GetCourse().Name == "MATH-1004").Result, Result.NotInCoursesList);
                //this one is cleaned GROUP-2-COURSE-REUSE-BB.2
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[1].PlanningStatus, PlanningStatus.NotPlanned);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[1].CompletionStatus, CompletionStatus.NotStarted);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[1].Results.First(r => r.GetCourse().Name == "ENGL-200").Result, Result.FromWrongDepartment);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[1].Results.First(r => r.GetCourse().Name == "MATH-1004").Result, Result.FromWrongDepartment);

                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[1].PlanningStatus, PlanningStatus.PartiallyPlanned);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[1].CompletionStatus, CompletionStatus.PartiallyCompleted);

                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[1].GroupResults[0].CompletionStatus, CompletionStatus.PartiallyCompleted);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[1].GroupResults[0].PlanningStatus, PlanningStatus.PartiallyPlanned);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results.First(r => r.GetCourse().Name == "ENGL-200").Result, Result.NotInCoursesList);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results.First(r => r.GetCourse().Name == "COMM-100").Result, Result.Applied);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results.First(r => r.GetCourse().Name == "COMM-1321").Result, Result.Applied);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[1].GroupResults[0].Results.First(r => r.GetCourse().Name == "MATH-1004").Result, Result.NotInCoursesList);

                //this is cleaned up
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[1].GroupResults[1].CompletionStatus, CompletionStatus.NotStarted);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[1].GroupResults[1].PlanningStatus, PlanningStatus.NotPlanned);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[1].GroupResults[1].Results.First(r => r.GetCourse().Name == "ENGL-200").Result, Result.FromWrongDepartment);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[1].GroupResults[1].Results.First(r => r.GetCourse().Name == "MATH-1004").Result, Result.FromWrongDepartment);

                //requirement 2 - will not use applied courses from req 1 because of requirement type exclusions
                Assert.AreEqual(eval.RequirementResults[1].PlanningStatus, PlanningStatus.PartiallyPlanned);
                Assert.AreEqual(eval.RequirementResults[1].CompletionStatus, CompletionStatus.PartiallyCompleted);
                Assert.AreEqual(eval.RequirementResults[1].SubRequirementResults[0].PlanningStatus, PlanningStatus.PartiallyPlanned);
                Assert.AreEqual(eval.RequirementResults[1].SubRequirementResults[0].CompletionStatus, CompletionStatus.PartiallyCompleted);

                Assert.AreEqual(eval.RequirementResults[1].SubRequirementResults[0].GroupResults[0].PlanningStatus, PlanningStatus.PartiallyPlanned);
                Assert.AreEqual(eval.RequirementResults[1].SubRequirementResults[0].GroupResults[0].CompletionStatus, CompletionStatus.PartiallyCompleted);
                Assert.AreEqual(eval.RequirementResults[1].SubRequirementResults[0].GroupResults[0].Results.First(r => r.GetCourse().Name == "ENGL-200").Result, Result.Applied);
                Assert.AreEqual(eval.RequirementResults[1].SubRequirementResults[0].GroupResults[0].Results.First(r => r.GetCourse().Name == "MATH-1004").Result, Result.Applied);





            }
        }


        [TestClass]
        public class ProgramEvaluatorRequirmentExclusionsWithExtraCourses : ProgramEvaluationTests
        {
            //PROG.RELEASE.2.BB - DATASETUP is in devcoll - releaseuser/Testing123!
            //Scenario 1: Req1->SubReq1 - type MIN - excludes SPC
            //  Take 2 credits from dept perf; (type 32)
            //Course Reuse = "N" at subrequirement and group level (REQU, AEVP)
            //AEDF setting is Semi_Apply

            //Req 2-> Sub Req1 - type SPC - excludes MIN
            //Take 2 CREDITS from DANC-100 DANC-101 (type 32)

            //Req1 excludes Req2 and vice-versa

            //Acad credits
            //DANC-101  IN-PROGRESS
            //DANC-102  IN-PROGRESS
            //DANC-100  IN-PROGRESS
            //ENGL-1BMA  REGISTERED
            //ENGL-201  IN-PROGRESS

            //when option is semi-apply, course resuse is N -  when first Req1 will be evaluated then DANC-100 will be marked as extra and SHOULD be  available to Req 2 
            //when option is apply, course resuse is N -  when first Req1 will be evaluated then DANC-100 will be marked as extra and SHOULD not be  available to Req 2 
            //when option is ignore, course resuse is N -  when first Req1 will be evaluated then DANC-100 should not be in req 1 and SHOULD  be  available to Req 2 
            //when option is apply, course resuse is N -  when first Req1 will be evaluated then DANC-100 should not be in req 1 and SHOULD  be  available to Req 2 


            [TestMethod]
            public async Task ExclusionWithSemiApplyCourseReuseN()
            {

                studentid = "0017053";
                studentprogram = await GetStudentProgram(studentid, "PROG.RELEASE.2.BB");
                additionalrequirements = new List<Requirement>();
                plannedcourses = new List<PlannedCredit>();
                credits = await GetCredits(studentid);
                credits[0].AddDepartment("PERF");
                credits[1].AddDepartment("PERF");
                credits[2].AddDepartment("PERF");

                overrides = new List<Override>();

                ProgramRequirements pr = await programRequirementsRepo.GetAsync("PROG.RELEASE.2.BB", "2013");

                //setup extra course handling to semi-apply and course reuse flag=N
                pr.Requirements[0].AllowsCourseReuse = false;
                pr.Requirements[1].AllowsCourseReuse = false;
                pr.Requirements[0].SubRequirements[0].AllowsCourseReuse = false;
                pr.Requirements[1].SubRequirements[0].AllowsCourseReuse = false;
                pr.Requirements[0].ExtraCourseDirective = ExtraCourses.SemiApply;
                pr.Requirements[0].SubRequirements[0].ExtraCourseDirective = ExtraCourses.SemiApply;
                pr.Requirements[0].SubRequirements[0].Groups[0].ExtraCourseDirective = ExtraCourses.SemiApply;

                pr.Requirements[1].ExtraCourseDirective = ExtraCourses.SemiApply;
                pr.Requirements[1].SubRequirements[0].ExtraCourseDirective = ExtraCourses.SemiApply;
                pr.Requirements[1].SubRequirements[0].Groups[0].ExtraCourseDirective = ExtraCourses.SemiApply;


                ProgramEvaluation eval = new ProgramEvaluator(studentprogram, pr, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();

                Assert.IsNotNull(eval);
                Assert.AreEqual(eval.RequirementResults[0].PlanningStatus, PlanningStatus.CompletelyPlanned);
                Assert.AreEqual(eval.RequirementResults[0].CompletionStatus, CompletionStatus.PartiallyCompleted);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].PlanningStatus, PlanningStatus.CompletelyPlanned);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].CompletionStatus, CompletionStatus.PartiallyCompleted);

                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].PlanningStatus, PlanningStatus.CompletelyPlanned);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].CompletionStatus, CompletionStatus.PartiallyCompleted);

                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results.Count(), 4);
                //DANC-100 is marked as extra
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1].GetCourse().Name, "DANC-102");
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1].Result, Result.Applied);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[2].GetCourse().Name, "DANC-101");
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[2].Result, Result.Applied);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[3].GetCourse().Name, "DANC-100");
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[3].Result, Result.Applied);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[3].Explanation, AcadResultExplanation.Extra);


                //2nd requirment - subrequirment - DANC-100 is applied here because it is extra is requirment 1 but DANC-101 is not applied here becuase course is already applied to req 1 and we are excluding first requirement courses

                Assert.AreEqual(eval.RequirementResults[1].PlanningStatus, PlanningStatus.PartiallyPlanned);
                Assert.AreEqual(eval.RequirementResults[1].CompletionStatus, CompletionStatus.NotStarted);
                Assert.AreEqual(eval.RequirementResults[1].SubRequirementResults[0].PlanningStatus, PlanningStatus.PartiallyPlanned);
                Assert.AreEqual(eval.RequirementResults[1].SubRequirementResults[0].CompletionStatus, CompletionStatus.NotStarted);

                Assert.AreEqual(eval.RequirementResults[1].SubRequirementResults[0].GroupResults[0].PlanningStatus, PlanningStatus.PartiallyPlanned);
                Assert.AreEqual(eval.RequirementResults[1].SubRequirementResults[0].GroupResults[0].CompletionStatus, CompletionStatus.NotStarted);

                Assert.AreEqual(eval.RequirementResults[1].SubRequirementResults[0].GroupResults[0].Results.Count(), 2);
                Assert.AreEqual(eval.RequirementResults[1].SubRequirementResults[0].GroupResults[0].Results[1].GetCourse().Name, "DANC-100");
                Assert.AreEqual(eval.RequirementResults[1].SubRequirementResults[0].GroupResults[0].Results[1].Result, Result.Applied);
                Assert.AreEqual(eval.RequirementResults[1].SubRequirementResults[0].GroupResults[0].Results[1].Explanation, AcadResultExplanation.None);



            }

            [TestMethod]
            public async Task ExclusionWithApplyCourseReuseN()
            {

                studentid = "0017053";
                studentprogram = await GetStudentProgram(studentid, "PROG.RELEASE.2.BB");
                additionalrequirements = new List<Requirement>();
                plannedcourses = new List<PlannedCredit>();
                credits = await GetCredits(studentid);
                credits[0].AddDepartment("PERF");
                credits[1].AddDepartment("PERF");
                credits[2].AddDepartment("PERF");

                overrides = new List<Override>();

                ProgramRequirements pr = await programRequirementsRepo.GetAsync("PROG.RELEASE.2.BB", "2013");

                //setup extra course handling to apply and course reuse = N
                pr.Requirements[0].AllowsCourseReuse = false;
                pr.Requirements[1].AllowsCourseReuse = false;
                pr.Requirements[0].SubRequirements[0].AllowsCourseReuse = false;
                pr.Requirements[1].SubRequirements[0].AllowsCourseReuse = false;
                pr.Requirements[0].ExtraCourseDirective = ExtraCourses.Apply;
                pr.Requirements[0].SubRequirements[0].ExtraCourseDirective = ExtraCourses.Apply;
                pr.Requirements[0].SubRequirements[0].Groups[0].ExtraCourseDirective = ExtraCourses.Apply;

                pr.Requirements[1].ExtraCourseDirective = ExtraCourses.Apply;
                pr.Requirements[1].SubRequirements[0].ExtraCourseDirective = ExtraCourses.Apply;
                pr.Requirements[1].SubRequirements[0].Groups[0].ExtraCourseDirective = ExtraCourses.Apply;


                ProgramEvaluation eval = new ProgramEvaluator(studentprogram, pr, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();

                Assert.IsNotNull(eval);
                Assert.AreEqual(eval.RequirementResults[0].PlanningStatus, PlanningStatus.CompletelyPlanned);
                Assert.AreEqual(eval.RequirementResults[0].CompletionStatus, CompletionStatus.PartiallyCompleted);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].PlanningStatus, PlanningStatus.CompletelyPlanned);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].CompletionStatus, CompletionStatus.PartiallyCompleted);

                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].PlanningStatus, PlanningStatus.CompletelyPlanned);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].CompletionStatus, CompletionStatus.PartiallyCompleted);

                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results.Count(), 4);
                //DANC-100 is marked as extra
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].GetCourse().Name, "ENGL-201");
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].Result, Result.FromWrongDepartment);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1].GetCourse().Name, "DANC-102");
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1].Result, Result.Applied);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[2].GetCourse().Name, "DANC-101");
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[2].Result, Result.Applied);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[3].GetCourse().Name, "DANC-100");
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[3].Result, Result.Applied);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[3].Explanation, AcadResultExplanation.Extra);


                //2nd requirment - subrequirment - DANC-100 will not be applied here even  it is extra because of apply option 

                Assert.AreEqual(eval.RequirementResults[1].PlanningStatus, PlanningStatus.NotPlanned);
                Assert.AreEqual(eval.RequirementResults[1].CompletionStatus, CompletionStatus.NotStarted);
                Assert.AreEqual(eval.RequirementResults[1].SubRequirementResults[0].PlanningStatus, PlanningStatus.NotPlanned);
                Assert.AreEqual(eval.RequirementResults[1].SubRequirementResults[0].CompletionStatus, CompletionStatus.NotStarted);

                Assert.AreEqual(eval.RequirementResults[1].SubRequirementResults[0].GroupResults[0].PlanningStatus, PlanningStatus.NotPlanned);
                Assert.AreEqual(eval.RequirementResults[1].SubRequirementResults[0].GroupResults[0].CompletionStatus, CompletionStatus.NotStarted);

                Assert.AreEqual(eval.RequirementResults[1].SubRequirementResults[0].GroupResults[0].Results.Count(), 1);



            }

            [TestMethod]
            public async Task ExclusionWithIgnoreCourseReuseN()
            {

                studentid = "0017053";
                studentprogram = await GetStudentProgram(studentid, "PROG.RELEASE.2.BB");
                additionalrequirements = new List<Requirement>();
                plannedcourses = new List<PlannedCredit>();
                credits = await GetCredits(studentid);
                credits[0].AddDepartment("PERF");
                credits[1].AddDepartment("PERF");
                credits[2].AddDepartment("PERF");

                overrides = new List<Override>();

                ProgramRequirements pr = await programRequirementsRepo.GetAsync("PROG.RELEASE.2.BB", "2013");

                //setup extra course handling to apply and course reuse = N
                pr.Requirements[0].AllowsCourseReuse = false;
                pr.Requirements[1].AllowsCourseReuse = false;
                pr.Requirements[0].SubRequirements[0].AllowsCourseReuse = false;
                pr.Requirements[1].SubRequirements[0].AllowsCourseReuse = false;
                pr.Requirements[0].ExtraCourseDirective = ExtraCourses.Ignore;
                pr.Requirements[0].SubRequirements[0].ExtraCourseDirective = ExtraCourses.Ignore;
                pr.Requirements[0].SubRequirements[0].Groups[0].ExtraCourseDirective = ExtraCourses.Ignore;

                pr.Requirements[1].ExtraCourseDirective = ExtraCourses.Ignore;
                pr.Requirements[1].SubRequirements[0].ExtraCourseDirective = ExtraCourses.Ignore;
                pr.Requirements[1].SubRequirements[0].Groups[0].ExtraCourseDirective = ExtraCourses.Ignore;


                ProgramEvaluation eval = new ProgramEvaluator(studentprogram, pr, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();

                Assert.IsNotNull(eval);
                Assert.AreEqual(eval.RequirementResults[0].PlanningStatus, PlanningStatus.CompletelyPlanned);
                Assert.AreEqual(eval.RequirementResults[0].CompletionStatus, CompletionStatus.PartiallyCompleted);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].PlanningStatus, PlanningStatus.CompletelyPlanned);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].CompletionStatus, CompletionStatus.PartiallyCompleted);

                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].PlanningStatus, PlanningStatus.CompletelyPlanned);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].CompletionStatus, CompletionStatus.PartiallyCompleted);

                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results.Count(), 4);
                //DANC-100 will not be picked up and won't be applied here because there is no extra course handling
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1].GetCourse().Name, "DANC-102");
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1].Result, Result.Applied);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[2].GetCourse().Name, "DANC-101");
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[2].Result, Result.Applied);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[3].GetCourse().Name, "DANC-100");
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[3].Result, Result.Related);



                //2nd requirment - subrequirment - DANC-100 will  be applied here since it is not applied in first requirment 

                Assert.AreEqual(eval.RequirementResults[1].PlanningStatus, PlanningStatus.PartiallyPlanned);
                Assert.AreEqual(eval.RequirementResults[1].CompletionStatus, CompletionStatus.NotStarted);
                Assert.AreEqual(eval.RequirementResults[1].SubRequirementResults[0].PlanningStatus, PlanningStatus.PartiallyPlanned);
                Assert.AreEqual(eval.RequirementResults[1].SubRequirementResults[0].CompletionStatus, CompletionStatus.NotStarted);

                Assert.AreEqual(eval.RequirementResults[1].SubRequirementResults[0].GroupResults[0].PlanningStatus, PlanningStatus.PartiallyPlanned);
                Assert.AreEqual(eval.RequirementResults[1].SubRequirementResults[0].GroupResults[0].CompletionStatus, CompletionStatus.NotStarted);

                Assert.AreEqual(eval.RequirementResults[1].SubRequirementResults[0].GroupResults[0].Results.Count(), 2);
                Assert.AreEqual(eval.RequirementResults[1].SubRequirementResults[0].GroupResults[0].Results[1].GetCourse().Name, "DANC-100");
                Assert.AreEqual(eval.RequirementResults[1].SubRequirementResults[0].GroupResults[0].Results[1].Result, Result.Applied);
                Assert.AreEqual(eval.RequirementResults[1].SubRequirementResults[0].GroupResults[0].Results[1].Explanation, AcadResultExplanation.None);



            }

            [TestMethod]
            public async Task ExclusionWithIgnoreCourseReuseY()
            {
                //course reuse doesn't come in picture across requirments. In this scenario even though course reuse is allowed but still course applied to first requirment is not
                //available to other requirment because of exclusion syntax between those requirements and not due to the flag
                studentid = "0017053";
                studentprogram = await GetStudentProgram(studentid, "PROG.RELEASE.2.BB");
                additionalrequirements = new List<Requirement>();
                plannedcourses = new List<PlannedCredit>();
                credits = await GetCredits(studentid);
                credits[0].AddDepartment("PERF");
                credits[1].AddDepartment("PERF");
                credits[2].AddDepartment("PERF");

                overrides = new List<Override>();

                ProgramRequirements pr = await programRequirementsRepo.GetAsync("PROG.RELEASE.2.BB", "2013");

                //setup extra course handling to apply and course reuse = N
                pr.Requirements[0].AllowsCourseReuse = true;
                pr.Requirements[1].AllowsCourseReuse = true;
                pr.Requirements[0].SubRequirements[0].AllowsCourseReuse = true;
                pr.Requirements[1].SubRequirements[0].AllowsCourseReuse = true;
                pr.Requirements[0].ExtraCourseDirective = ExtraCourses.Ignore;
                pr.Requirements[0].SubRequirements[0].ExtraCourseDirective = ExtraCourses.Ignore;
                pr.Requirements[0].SubRequirements[0].Groups[0].ExtraCourseDirective = ExtraCourses.Ignore;

                pr.Requirements[1].ExtraCourseDirective = ExtraCourses.Ignore;
                pr.Requirements[1].SubRequirements[0].ExtraCourseDirective = ExtraCourses.Ignore;
                pr.Requirements[1].SubRequirements[0].Groups[0].ExtraCourseDirective = ExtraCourses.Ignore;


                ProgramEvaluation eval = new ProgramEvaluator(studentprogram, pr, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();

                Assert.IsNotNull(eval);
                Assert.AreEqual(eval.RequirementResults[0].PlanningStatus, PlanningStatus.CompletelyPlanned);
                Assert.AreEqual(eval.RequirementResults[0].CompletionStatus, CompletionStatus.PartiallyCompleted);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].PlanningStatus, PlanningStatus.CompletelyPlanned);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].CompletionStatus, CompletionStatus.PartiallyCompleted);

                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].PlanningStatus, PlanningStatus.CompletelyPlanned);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].CompletionStatus, CompletionStatus.PartiallyCompleted);

                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results.Count(), 4);
                //DANC-100 will not be picked up and won't be applied here because there is no extra course handling
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1].GetCourse().Name, "DANC-102");
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1].Result, Result.Applied);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[2].GetCourse().Name, "DANC-101");
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[2].Result, Result.Applied);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[3].GetCourse().Name, "DANC-100");
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[3].Result, Result.Related);



                //2nd requirment - subrequirment - DANC-100 will  be applied here since it is not applied in first requirment 

                Assert.AreEqual(eval.RequirementResults[1].PlanningStatus, PlanningStatus.PartiallyPlanned);
                Assert.AreEqual(eval.RequirementResults[1].CompletionStatus, CompletionStatus.NotStarted);
                Assert.AreEqual(eval.RequirementResults[1].SubRequirementResults[0].PlanningStatus, PlanningStatus.PartiallyPlanned);
                Assert.AreEqual(eval.RequirementResults[1].SubRequirementResults[0].CompletionStatus, CompletionStatus.NotStarted);

                Assert.AreEqual(eval.RequirementResults[1].SubRequirementResults[0].GroupResults[0].PlanningStatus, PlanningStatus.PartiallyPlanned);
                Assert.AreEqual(eval.RequirementResults[1].SubRequirementResults[0].GroupResults[0].CompletionStatus, CompletionStatus.NotStarted);

                Assert.AreEqual(eval.RequirementResults[1].SubRequirementResults[0].GroupResults[0].Results.Count(), 2);
                Assert.AreEqual(eval.RequirementResults[1].SubRequirementResults[0].GroupResults[0].Results[1].GetCourse().Name, "DANC-100");
                Assert.AreEqual(eval.RequirementResults[1].SubRequirementResults[0].GroupResults[0].Results[1].Result, Result.Applied);
                Assert.AreEqual(eval.RequirementResults[1].SubRequirementResults[0].GroupResults[0].Results[1].Explanation, AcadResultExplanation.None);



            }


            //Another Scenario 
            //PROG.RELEASE.BB - DATASETUP is in devcoll - releaseuser/Testing123!
            //req-1 (MAJ)  priority 1 with course reuse=N
            //excludes  MIN
            //has 1 subrequirments with course reuse = N
            //Subrequirment 1
            //Take 1 group;
            //  Take 2 credits from dept PERF;  (block type 33)
            //  TAKE 2 credits from DANC-102 DANC-101 ENGL-201; (31)


            //Requirment 2 type MIN 
            //excludes MAJ
            //subrequirment -1 
            //Take 4 CREDITS FROM DANC-100 DANC-101 DANC-102 ENGL-201

            //Acad credits
            //DANC-100  IN-PROGRESS
            //DANC-101  IN-PROGRESS
            //DANC-102  IN-PROGRESS
            //ENGL-201  IN-PROGRESS

            //Here first subrequirment asks for to take only one group then all the courses applied to the group, that will be marked extra, should be freed for 2nd requirment- when extra course handling is semi-apply
            //so from sub-req 1 of req 1 - "take 2 credits from perf" will be taken and danc-100 will be marked as extra
            //engl-201 in group 2 will be marked ExtraInGroup.
            //so danc-100 and engl-201 should be available to 2nd requirment

            //when apply option then enlg-201 should be available but not danc-100 for 2nd requirement

            [TestMethod]
            public async Task ExclusionWithTakeGroupSemiApplyCourseReuseN()
            {

                studentid = "0017053";
                studentprogram = await GetStudentProgram(studentid, "PROG.RELEASE.BB");
                additionalrequirements = new List<Requirement>();
                plannedcourses = new List<PlannedCredit>();
                credits = await GetCredits(studentid);
                credits[0].AddDepartment("PERF");
                credits[1].AddDepartment("PERF");
                credits[2].AddDepartment("PERF");
                

                overrides = new List<Override>();

                ProgramRequirements pr = await programRequirementsRepo.GetAsync("PROG.RELEASE.BB", "2013");

                //setup extra course handling to semi-apply and course reuse flag=N
                pr.Requirements[0].AllowsCourseReuse = false;
                pr.Requirements[1].AllowsCourseReuse = false;
                pr.Requirements[0].SubRequirements[0].AllowsCourseReuse = false;
                pr.Requirements[1].SubRequirements[0].AllowsCourseReuse = false;
                pr.Requirements[0].ExtraCourseDirective = ExtraCourses.SemiApply;
                pr.Requirements[0].SubRequirements[0].ExtraCourseDirective = ExtraCourses.SemiApply;
                pr.Requirements[0].SubRequirements[0].Groups[0].ExtraCourseDirective = ExtraCourses.SemiApply;
                pr.Requirements[0].SubRequirements[0].Groups[1].ExtraCourseDirective = ExtraCourses.SemiApply;

                pr.Requirements[1].ExtraCourseDirective = ExtraCourses.SemiApply;
                pr.Requirements[1].SubRequirements[0].ExtraCourseDirective = ExtraCourses.SemiApply;
                pr.Requirements[1].SubRequirements[0].Groups[0].ExtraCourseDirective = ExtraCourses.SemiApply;


                ProgramEvaluation eval = new ProgramEvaluator(studentprogram, pr, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();

                Assert.IsNotNull(eval);
                Assert.AreEqual(eval.RequirementResults[0].PlanningStatus, PlanningStatus.CompletelyPlanned);
                Assert.AreEqual(eval.RequirementResults[0].CompletionStatus, CompletionStatus.PartiallyCompleted);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].PlanningStatus, PlanningStatus.CompletelyPlanned);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].CompletionStatus, CompletionStatus.PartiallyCompleted);

                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].PlanningStatus, PlanningStatus.CompletelyPlanned);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].CompletionStatus, CompletionStatus.PartiallyCompleted);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].MinGroupStatus, GroupResultMinGroupStatus.None);
                //2nd group is marked as extra
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[1].PlanningStatus, PlanningStatus.PartiallyPlanned);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[1].CompletionStatus, CompletionStatus.NotStarted);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[1].MinGroupStatus, GroupResultMinGroupStatus.Extra);

                //1st group of sub-req 1 for req 1
                //DANC-100 is marked as extra
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results.Count(), 4);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].GetCourse().Name, "ENGL-201");
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].Result, Result.FromWrongDepartment);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].Explanation, AcadResultExplanation.None);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1].GetCourse().Name, "DANC-102");
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1].Result, Result.Applied);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[2].GetCourse().Name, "DANC-101");
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[2].Result, Result.Applied);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[3].GetCourse().Name, "DANC-100");
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[3].Result, Result.Applied);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[3].Explanation, AcadResultExplanation.Extra);
                

                //2nd group of sub-req 1 for req 1
                //engl-201 will be applied here, danc-101 and danc-102 will not be picked because course reuse is false
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[1].Results.Count(), 2);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[1].Results[0].GetCourse().Name, "ENGL-201");
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[1].Results[0].Result, Result.Applied);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[1].Results[0].Explanation, AcadResultExplanation.ExtraInGroup);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[1].Results[1].GetCourse().Name, "DANC-100");
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[1].Results[1].Result, Result.NotInCoursesList);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[1].Results[1].Explanation, AcadResultExplanation.ExtraInGroup);


                //2nd requirment - subrequirment - DANC-100 is applied AND ENGL-201 is applied here because it is extra is requirment 1 but DANC-101 is not applied here becuase course is already applied to req 1 and we are excluding first requirement courses

               

                Assert.AreEqual(eval.RequirementResults[1].SubRequirementResults[0].GroupResults[0].Results.Count(), 2);
                Assert.AreEqual(eval.RequirementResults[1].SubRequirementResults[0].GroupResults[0].Results[0].GetCourse().Name, "ENGL-201");
                Assert.AreEqual(eval.RequirementResults[1].SubRequirementResults[0].GroupResults[0].Results[0].Result, Result.Applied);
                Assert.AreEqual(eval.RequirementResults[1].SubRequirementResults[0].GroupResults[0].Results[0].Explanation, AcadResultExplanation.None);
                Assert.AreEqual(eval.RequirementResults[1].SubRequirementResults[0].GroupResults[0].Results[1].GetCourse().Name, "DANC-100");
                Assert.AreEqual(eval.RequirementResults[1].SubRequirementResults[0].GroupResults[0].Results[1].Result, Result.Applied);
                Assert.AreEqual(eval.RequirementResults[1].SubRequirementResults[0].GroupResults[0].Results[1].Explanation, AcadResultExplanation.None);
              



            }

        }

        [TestClass]
        public class ProgramEvaluatorRequirmentGroupsWithInListOrder : ProgramEvaluationTests
        {


            //PROG.SORT.ORDER.BB - DATASETUP is in devcoll - student_sort/Testing123!
            /* Req9( IN.LIST.ORDER.2)->S-5
             * TAKE 7 COURSES FROM FREN-100 GERM-100 HIND-100 ARTH-100 HUMT-100 CRIM-100 POLI-100; IN.LIST.ORDER;
             * using default sorting only and no sort specifications
             * such as default sorting will occur by putting completed courses and then IP courses and then planned courses at end.
             * completed courses and ip courses are sorted on STC.START.DATE ASC followed by STC.ACAD.CRED.ID in desc
             * planned courses/sections by default are sorted on term id followed by desc section ids. If planned course is there which does not have sectionId then it will fall at the end of sorted planned courses/sections list
             * After default soring is done, the result again gets re-sorted based upon the list of courses on From courses syntax
             * IN.LIST.ORDER rearrange Completed courses on top, sorted based upon sequence
             * followed by Inprogress courses, sorted based upon sequence
             * followed by planned courses (planned courses are not sorted based upon the sequence in from courses list, these are always pushed at the bottom)
             */

            /* STUDENT ID- 0016301
             Academic Credits and planned courses in order of STC.START.DATE
             Courses   Id		 Status 	 	Term 		STC.START.DATE  
            FRN-100      126		IP		    2019/FA	    08/10/20	
            GERM-100                 PL		    2019/FA	    08/06/20			COURSE ID-7442
            HIND-100     125	    PR		    2020/FA 	08/01/20		
            ARTH-100       	        PL		    2020/WI	    08/07/20		COURSE ID-7444
            HUMT-100     128	    C		    2021/SP	    08/09/20		
            CRIM-100     129	    IP		    2020/FA    	08/11/20		
            POLI-100     127		C		    2020BT	   	08/05/20		
         
                without in list order
                POLI-100 HUMT-100 HIND-100 FREN-100 CRIM-100  ARTH-100 GERM-100

                WITH IN LIST ORDER
                HUMT-100 POLI-100 FREN-100 HIND-100 CRIM-100 GERM-100 ARTH-100*/


            [TestMethod]
            public async Task ValidateNonInListOrderAppliedCourses()
            {
                 studentid = "0016301";
                    studentprogram = await GetStudentProgram(studentid, "PROG.IN.LIST.SORT.ORDER.BB");
                    additionalrequirements = new List<Requirement>();
                    credits = await GetCredits(studentid);
                    plannedcourses = await GetCourses(studentid);
                    overrides = new List<Override>();
                    ProgramRequirements pr = await programRequirementsRepo.GetAsync("PROG.IN.LIST.SORT.ORDER.BB", "2013");
                    ProgramEvaluation eval = new ProgramEvaluator(studentprogram, pr, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();
                    Assert.IsNotNull(eval);
                Assert.IsNotNull(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results);

                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].Result,Result.Applied);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].GetCourse().Name,"POLI-100");
                Assert.IsNotNull(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].GetAcadCred());
                Assert.IsTrue(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[0].GetAcadCred().IsCompletedCredit);

                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1].Result, Result.Applied);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1].GetCourse().Name, "HUMT-100");
                Assert.IsNotNull(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1].GetAcadCred());
                Assert.IsTrue(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[1].GetAcadCred().IsCompletedCredit);

                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[2].Result, Result.Applied);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[2].GetCourse().Name, "HIND-100");
                Assert.IsNotNull(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[2].GetAcadCred());
                Assert.IsFalse(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[2].GetAcadCred().IsCompletedCredit);

                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[3].Result, Result.Applied);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[3].GetCourse().Name, "FREN-100");
                Assert.IsNotNull(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[3].GetAcadCred());
                Assert.IsFalse(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[3].GetAcadCred().IsCompletedCredit);

                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[4].Result, Result.Applied);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[4].GetCourse().Name, "CRIM-100");
                Assert.IsNotNull(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[4].GetAcadCred());
                Assert.IsFalse(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[4].GetAcadCred().IsCompletedCredit);

                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[5].Result, Result.PlannedApplied);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[5].GetCourse().Name, "ARTH-100");
                Assert.IsNull(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[5].GetAcadCred());

                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[6].Result, Result.PlannedApplied);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[6].GetCourse().Name, "GERM-100");
                Assert.IsNull(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results[6].GetAcadCred());
            }

            [TestMethod]
            public async Task ValidateInListOrderInFromClause()
            {

                studentid = "0016301";
                studentprogram = await GetStudentProgram(studentid, "PROG.IN.LIST.SORT.ORDER.BB");
                additionalrequirements = new List<Requirement>();
                credits = await GetCredits(studentid);
                plannedcourses = await GetCourses(studentid);
                overrides = new List<Override>();
                ProgramRequirements pr = await programRequirementsRepo.GetAsync("PROG.IN.LIST.SORT.ORDER.BB", "2013");
                ProgramEvaluation eval = new ProgramEvaluator(studentprogram, pr, additionalrequirements, credits, plannedcourses, ruleResults, overrides, courses, logger).Evaluate();
                Assert.IsNotNull(eval);
                Assert.IsNotNull(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[1].Results);

                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[1].Results[0].Result, Result.Applied);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[1].Results[0].GetCourse().Name, "HUMT-100");
                Assert.IsNotNull(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[1].Results[0].GetAcadCred());
                Assert.IsTrue(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[1].Results[0].GetAcadCred().IsCompletedCredit);

                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[1].Results[1].Result, Result.Applied);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[1].Results[1].GetCourse().Name, "POLI-100");
                Assert.IsNotNull(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[1].Results[1].GetAcadCred());
                Assert.IsTrue(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[1].Results[1].GetAcadCred().IsCompletedCredit);

                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[1].Results[2].Result, Result.Applied);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[1].Results[2].GetCourse().Name, "FREN-100");
                Assert.IsNotNull(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[1].Results[2].GetAcadCred());
                Assert.IsFalse(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[1].Results[2].GetAcadCred().IsCompletedCredit);

                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[1].Results[3].Result, Result.Applied);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[1].Results[3].GetCourse().Name, "HIND-100");
                Assert.IsNotNull(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[1].Results[3].GetAcadCred());
                Assert.IsFalse(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[1].Results[3].GetAcadCred().IsCompletedCredit);

                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[1].Results[4].Result, Result.Applied);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[1].Results[4].GetCourse().Name, "CRIM-100");
                Assert.IsNotNull(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[1].Results[4].GetAcadCred());
                Assert.IsFalse(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[1].Results[4].GetAcadCred().IsCompletedCredit);

                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[1].Results[5].Result, Result.PlannedApplied);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[1].Results[5].GetCourse().Name, "GERM-100");
                Assert.IsNull(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[1].Results[5].GetAcadCred());

                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[1].Results[6].Result, Result.PlannedApplied);
                Assert.AreEqual(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[1].Results[6].GetCourse().Name, "ARTH-100");
                Assert.IsNull(eval.RequirementResults[0].SubRequirementResults[0].GroupResults[1].Results[6].GetAcadCred());
            }

        }

    }
}
