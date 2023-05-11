using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using Ellucian.Web.Http.TestUtil;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Base.Entities;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities.Requirements
{
    [TestClass]
    public class IndividualGroupTests
    {
        private ProgramRequirements pr;
        private TestAcademicCreditRepository tacr;
        private TestProgramRequirementsRepository tprr;
        private TestCourseRepository tcr;
        private TestGradeRepository tgr;

        private List<string> reqnames;
        private List<string> subnames;
        private List<string> grpnames;
        private Dictionary<string, List<string>> Subreq;
        private Dictionary<string, List<string>> groups;

        private List<Override> overs;
        private List<AcadResult> acadresults;
        private string termId;
        private IEnumerable<Course> equatedCourses;
 
        [TestInitialize]
        public async void Initialize()
        {
            tacr = new TestAcademicCreditRepository();
            tprr = new TestProgramRequirementsRepository();
            tcr = new TestCourseRepository();
            tgr = new TestGradeRepository();

            // This must be mocked up properly even though we are just testing the group.  Group evaluation
            // checks up the tree for upper-level minimum grade requirements, for example

            reqnames = new List<string>();
            subnames = new List<string>();
            grpnames = new List<string>();
            Subreq = new Dictionary<string, List<string>>();
            groups = new Dictionary<string, List<string>>();

            // This is how to describe a requirement tree to the test repository
            // We will create one here for tests that aren't too picky.  Otherwise
            // individual tests can create their own.

            reqnames.Add("GroupTestRequirement");           // list of requirement names to handle
            subnames.Add("GroupTestSubrequirement");        // list of Subrequirement names
            Subreq.Add(reqnames[0], subnames);              // dictionary as pointer from requirement name to list of Subrequirement names under it
            grpnames.Add("Test55");                         // list of group names (Check TestProgramRequirementsRepository to see what groups are 
            // available.  Test55, for example, is "TAKE 2 COURSES; MIN GRADE A,AU,P"
            groups.Add(subnames[0], grpnames);              // dictionary as pointer from Subrequirement name to list of group names under it
            pr = await tprr.BuildTestProgramRequirementsAsync("Test55", reqnames, Subreq, groups);

            overs = new List<Override>();
            equatedCourses = new TestCourseRepository().GetAsync().Result;

            termId = "2014/FA";
        }

        [TestMethod]
        public void RulesFail()
        {
            // This acad cred will fail to apply because of a rule

            Subrequirement s = pr.Requirements.First().SubRequirements.First();
            s.Groups.Clear();
            Group g = new Group("GroupId", "GroupCode", s);
            s.Groups.Add(g);

            g.MinCourses = 1;
            var rule = new Rule<AcademicCredit>("anyrule");
            var rr = new RequirementRule(rule);
            rr.SetAnswer(false, new AcademicCredit("1"));
            g.AcademicCreditRules.Add(rr);
            string[] credids = { "1" };
            acadresults = PrepResults(credids);

            GroupResult gr = g.Evaluate(acadresults, overs, equatedCourses);

            Assert.AreEqual(Result.RuleFailed, gr.Results.First().Result);
        }
        [TestMethod]
        public void RulesPass()
        {
            // This acad cred will apply 

            Subrequirement s = pr.Requirements.First().SubRequirements.First();
            s.Groups.Clear();
            Group g = new Group("GroupId", "GroupCode", s);
            s.Groups.Add(g);

            g.MinCourses = 1;
            var rule = new Rule<AcademicCredit>("anyrule");
            var rrule = new RequirementRule(rule);
            rrule.SetAnswer(true, new AcademicCredit("1"));
            g.AcademicCreditRules.Add(rrule);
            string[] credids = { "1" };
            acadresults = PrepResults(credids);
            GroupResult gr = g.Evaluate(acadresults, overs, equatedCourses);

            Assert.AreNotEqual(Result.RuleFailed, gr.Results.First().Result);
        }

        [TestMethod]
        public void FromCoursesEvalAgainstNonCourseNoThrow()
        {
            Subrequirement s = pr.Requirements.First().SubRequirements.First();
            s.Groups.Clear();
            Group g = new Group("GroupId", "GroupCode", s);
            s.Groups.Add(g);

            g.MinCourses = 1;
            g.FromCourses = new List<string>() { "x", "y", "niner" };

            string[] credids = { "1001" };  //noncourse
            acadresults = PrepResults(credids);
            GroupResult gr = g.Evaluate(acadresults, overs, equatedCourses);

        }

        [TestMethod]
        public async Task OtherAllowedGradesCascadesDownFromProgram()
        {
            // clear the generic stuff
            reqnames.Clear(); subnames.Clear(); grpnames.Clear(); Subreq.Clear(); groups.Clear(); pr = null;

            reqnames.Add("GroupTestRequirement");           // list of requirement names to handle
            subnames.Add("GroupTestSubrequirement");        // list of Subrequirement names
            Subreq.Add(reqnames[0], subnames);              // dictionary as pointer from requirement name to list of Subrequirement names under it
            grpnames.Add("Test60");                         // list of group names (Check TestProgramRequirementsRepository to see what groups are 
            // test60 is "TAKE 2 COURSES; FROM DEPT MATH,DANC
            groups.Add(subnames[0], grpnames);              // dictionary as pointer from Subrequirement name to list of group names under it
            pr = await tprr.BuildTestProgramRequirementsAsync("Test60", reqnames, Subreq, groups);
            Group g = pr.Requirements.First().SubRequirements.First().Groups.First();

            // Define min grade of D, allowed grade P at program level
            pr.MinGrade =(await tgr.GetAsync()).Where(grd => grd.LetterGrade == "D").First();
            pr.AllowedGrades = new List<Grade>() {(await tgr.GetAsync()).First(x => x.Id == "P") };

            string[] credids = { "16", "22" };  //one "A" and one "P"
            acadresults = PrepResults(credids);
            GroupResult gr = g.Evaluate(acadresults, overs, equatedCourses);

            Assert.IsTrue(gr.IsSatisfied());
        }

        [TestMethod]
        public async Task OtherAllowedGradesCascadesDownFromRequirement()
        {
            // clear the generic stuff
            reqnames.Clear(); subnames.Clear(); grpnames.Clear(); Subreq.Clear(); groups.Clear(); pr = null;

            reqnames.Add("GroupTestRequirement");           // list of requirement names to handle
            subnames.Add("GroupTestSubrequirement");        // list of Subrequirement names
            Subreq.Add(reqnames[0], subnames);              // dictionary as pointer from requirement name to list of Subrequirement names under it
            grpnames.Add("Test60");                         // list of group names (Check TestProgramRequirementsRepository to see what groups are 
            // test60 is "TAKE 2 COURSES; FROM DEPT MATH,DANC
            groups.Add(subnames[0], grpnames);              // dictionary as pointer from Subrequirement name to list of group names under it
            pr = await tprr.BuildTestProgramRequirementsAsync("Test60", reqnames, Subreq, groups);
            Group g = pr.Requirements.First().SubRequirements.First().Groups.First();


            // Add "P" to other allowed grades in the requirement
            pr.Requirements.First().MinGrade = (await tgr.GetAsync()).Where(grd => grd.LetterGrade == "D").First();
            pr.Requirements.First().AllowedGrades = new List<Grade>() { (await tgr.GetAsync()).First(x => x.Id == "P") };

            string[] credids = { "16", "22" };  //one "A" and one "P"
            acadresults = PrepResults(credids);
            GroupResult gr = g.Evaluate(acadresults, overs, equatedCourses);

            Assert.IsTrue(gr.IsSatisfied());
        }

        [TestMethod]
        public async Task OtherAllowedGradesCascadesDownFromSubrequirement()
        {
            // clear the generic stuff
            reqnames.Clear(); subnames.Clear(); grpnames.Clear(); Subreq.Clear(); groups.Clear(); pr = null;

            reqnames.Add("GroupTestRequirement");           // list of requirement names to handle
            subnames.Add("GroupTestSubrequirement");        // list of Subrequirement names
            Subreq.Add(reqnames[0], subnames);              // dictionary as pointer from requirement name to list of Subrequirement names under it
            grpnames.Add("Test60");                         // list of group names (Check TestProgramRequirementsRepository to see what groups are 
            // test60 is "TAKE 2 COURSES; FROM DEPT MATH,DANC"
            groups.Add(subnames[0], grpnames);              // dictionary as pointer from Subrequirement name to list of group names under it
            pr = await tprr.BuildTestProgramRequirementsAsync("Test60", reqnames, Subreq, groups);
            Group g = pr.Requirements.First().SubRequirements.First().Groups.First();


            // Add "P" to other allowed grades in the sub
            pr.Requirements.First().SubRequirements.First().MinGrade = (await tgr.GetAsync()).Where(grd => grd.LetterGrade == "D").First();
            pr.Requirements.First().SubRequirements.First().AllowedGrades = new List<Grade>() { (await tgr.GetAsync()).First(x => x.Id == "P") };

            string[] credids = { "16", "22" };  //one "A" and one "P"
            acadresults = PrepResults(credids);
            GroupResult gr = g.Evaluate(acadresults, overs, equatedCourses);

            Assert.IsTrue(gr.IsSatisfied());
        }
        [TestMethod]
        public void MinCoursesPerDepartmentWithFromList()
        {
            // Min x courses/credits per department works against
            // departments that are in applied credits, but doesn't check that
            // all of the departments in the from-depts list are covered.
            // The language can make the specifications here confusing:

            // TAKE 2 COURSES FROM DEPARTMENTS MATH,ENGL
            // MIN 2 COURSES PER DEPARTMENT

            // This specification actually requires *4* courses.  The first
            // requirement is overridden by the second.


            Subrequirement s = pr.Requirements.First().SubRequirements.First();
            s.Groups.Clear();
            Group g = new Group("GroupId", "GroupCode", s);
            s.Groups.Add(g);

            g.MinCourses = 2;
            g.FromDepartments = new List<string>() { "MATH", "ENGL" };
            g.MinCoursesPerDepartment = 2;

            string[] credids = { "5", "6", "26", "36" };  // 2 math, 2 engl - should satisfy
            acadresults = PrepResults(credids);
            GroupResult gr = g.Evaluate(acadresults, overs, equatedCourses);

            Assert.AreEqual(GroupExplanation.Satisfied, gr.Explanations.First());
            Assert.IsTrue(gr.IsSatisfied());

        }

        [TestMethod]
        public void MinCreditsPerDepartmentWithFromList()
        {
            // Same as above with credits

            Subrequirement s = pr.Requirements.First().SubRequirements.First();
            s.Groups.Clear();
            Group g = new Group("GroupId", "GroupCode", s);
            s.Groups.Add(g);

            g.MinCourses = 2;
            g.FromDepartments = new List<string>() { "MATH", "ENGL" };
            g.MinCreditsPerDepartment = 5;

            string[] credids = { "5", "6", "26", "36" };  // 2 math, 2 engl - should satisfy
            acadresults = PrepResults(credids);
            GroupResult gr = g.Evaluate(acadresults, overs, equatedCourses);

            Assert.AreEqual(GroupExplanation.Satisfied, gr.Explanations.First());
            Assert.IsTrue(gr.IsSatisfied());

        }
        [TestMethod]
        public void MinCoursesPerSubjectWithFromList()
        {
            // Same as above but for Courses per Subject
            Subrequirement s = pr.Requirements.First().SubRequirements.First();
            s.Groups.Clear();
            Group g = new Group("GroupId", "GroupCode", s);
            s.Groups.Add(g);

            g.MinCourses = 2;
            g.FromSubjects = new List<string>() { "MATH", "ENGL" };
            g.MinCoursesPerSubject = 2;

            string[] credids = { "5", "6", "26", "36" };  // 2 math, 2 engl - should satisfy
            acadresults = PrepResults(credids);
            GroupResult gr = g.Evaluate(acadresults, overs, equatedCourses);

            Assert.AreEqual(GroupExplanation.Satisfied, gr.Explanations.First());
            Assert.IsTrue(gr.IsSatisfied());

        }
        [TestMethod]
        public void MinCreditsPerSubjectWithFromList()
        {
            // Same as above but for creduts per Subject
            Subrequirement s = pr.Requirements.First().SubRequirements.First();
            s.Groups.Clear();
            Group g = new Group("GroupId", "GroupCode", s);
            s.Groups.Add(g);

            g.MinCourses = 2;
            g.FromSubjects = new List<string>() { "MATH", "ENGL" };
            g.MinCreditsPerSubject = 5;


            string[] credids = { "5", "6", "26", "36" };  // 2 math, 2 engl - should satisfy

            acadresults = PrepResults(credids);
            GroupResult gr = g.Evaluate(acadresults, overs, equatedCourses);

            Assert.AreEqual(GroupExplanation.Satisfied, gr.Explanations.First());
            Assert.IsTrue(gr.IsSatisfied());

        }

        [TestMethod]
        public void MinCreditsWithPlannedCourses()
        {

            Subrequirement s = pr.Requirements.First().SubRequirements.First();
            s.Groups.Clear();
            Group g = new Group("GroupId", "GroupCode", s);
            s.Groups.Add(g);

            g.MinCredits = 9;

            string[] courseids = { "139", "42", "110", "21" };  // all courses in test repo are 3 cred min currently
            acadresults = PrepCourseResults(courseids);
            GroupResult gr = g.Evaluate(acadresults, overs, equatedCourses);

            Assert.AreEqual(GroupExplanation.MinCredits, gr.Explanations.First());

        }

        [TestMethod]
        public void MinGpaLookAheadOptimization()
        {

            Subrequirement s = pr.Requirements.First().SubRequirements.First();
            s.Groups.Clear();
            Group g = new Group("GroupId", "GroupCode", s);
            s.Groups.Add(g);
            g.MinCredits = 9m;
            g.MinGpa = 3m;


            string[] credids = { "7", "8", "24", "30" };  // B, B, D, A

            acadresults = PrepResults(credids);

            GroupResult gr = g.Evaluate(acadresults, overs, equatedCourses);

            //  Assert.AreEqual(GroupExplanation.Satisfied, gr.Explanations.First());
            
            foreach (string res in gr.EvalDebug) { Console.WriteLine(res); }
            Assert.AreEqual(Result.MinGPA, gr.Results.First(re => re.GetAcadCredId() == "24").Result);
            Assert.AreEqual(Result.Applied, gr.Results.First(re => re.GetAcadCredId() == "30").Result);
            Assert.AreEqual(GroupExplanation.Satisfied, gr.Explanations.First());
        }

        [TestMethod]
        public void MinInstCredLookAheadOptimization()
        {

            Subrequirement s = pr.Requirements.First().SubRequirements.First();
            s.Groups.Clear();
            Group g = new Group("GroupId", "GroupCode", s);
            s.Groups.Add(g);
            g.MinCredits = 5m;
            g.MinInstitutionalCredits = 3m;


            string[] credids = { "13", "14", "24", "30" };  // TR,TR,I,I

            acadresults = PrepResults(credids);

            GroupResult gr = g.Evaluate(acadresults, overs, equatedCourses);

            //  Assert.AreEqual(GroupExplanation.Satisfied, gr.Explanations.First());

            foreach (string res in gr.EvalDebug) { Console.WriteLine(res); }
            Assert.AreEqual(Result.MinInstitutionalCredit, gr.Results.First(re => re.GetAcadCredId() == "14").Result);
            Assert.AreEqual(Result.Applied, gr.Results.First(re => re.GetAcadCredId() == "24").Result);
            Assert.AreEqual(GroupExplanation.Satisfied, gr.Explanations.First());
        }

        [TestMethod]
        public void MinGpaLookAheadOptimization_When_disabled()
        {

            Subrequirement s = pr.Requirements.First().SubRequirements.First();
            s.Groups.Clear();
            Group g = new Group("GroupId", "GroupCode", s);
            s.Groups.Add(g);
            g.MinCredits = 9m;
            g.MinGpa = 3m;


            string[] credids = { "7", "8", "24", "30" };  // B, B, D, A

            acadresults = PrepResults(credids);

            GroupResult gr = g.Evaluate(acadresults, overs, equatedCourses, null, false, disableLookaheadOptimization:true);

            foreach (string res in gr.EvalDebug) { Console.WriteLine(res); }
            Assert.AreEqual(Result.Applied, gr.Results.First(re => re.GetAcadCredId() == "7").Result);
            Assert.AreEqual(Result.Applied, gr.Results.First(re => re.GetAcadCredId() == "8").Result);
            Assert.AreEqual(Result.Applied, gr.Results.First(re => re.GetAcadCredId() == "24").Result);
            Assert.AreEqual(Result.Related, gr.Results.First(re => re.GetAcadCredId() == "30").Result);
            Assert.AreEqual(GroupExplanation.MinGpa, gr.Explanations.First());
        }

       
        private List<AcadResult> PrepResults(string[] creditids)
        {
            List<AcadResult> results = new List<AcadResult>();

            IEnumerable<AcademicCredit> credits = tacr.GetAsync(creditids.ToList()).Result;
            foreach (var cred in credits)
            {
                results.Add(new CreditResult(cred));
            }
            return results;
        }

        private List<AcadResult> PrepCourseResults(string[] courseids)
        {
            List<AcadResult> results = new List<AcadResult>();

            IEnumerable<Course> courses = tcr.GetAsync(courseids.ToList()).Result;
            foreach (var course in courses)
            {
                results.Add(new CourseResult(new PlannedCredit(course,termId)));
            }
            return results;
        }

    }
}
