// Copyright 2013-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Domain.Student.Tests.Entities.Requirements
{
    [TestClass]
    public class GroupTests
    {
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        /// <summary>
        /// GroupTests
        /// These tests use GroupTestsData.xml. This test data file is best modified by using Visual Studio's built-in XML editor. 
        /// Steps to modify the XML file:
        ///     	* In api, right click XML file in visual studio and "check out for edit"
        ///     	* Double-click on the XML file to open it within Visual Studio
        ///     	* Save and close the XML file when complete
        ///         (check in as normal when all work complete)
        /// The test data consists of the following data:
        ///     TestNumber: Sequence number simply enables quick determination in debugger which test your are on
        ///     Phrase: The (approximate) group degree audit language to describe the test scenario--for reference only
        ///     Taken: Academic credits to be used as input to eval, using course name. The TestAcademicCredit repo provides logic
        ///         in the method that will take course names and select academic credits from the static test repository. (Use asterisk
        ///         delimiter between subject and number). Note that each course named here represents an academic credit, so this tool cannot
        ///         be used to test scenarios that specifically involve planned courses or duplicate takes of the same course.
        ///     Result: The expected group evaluation result, must be a value from GroupExplanation enum (found at the bottom of GroupResult.cs)
        ///     TestFile: The ID of the group to use for the test, set up as a case option in the BuildGroup method in the TestProgramRequirementsRepository.
        ///         This is the spec-version of the DA language, should reflect the Phrase information and whatever other specifications are needed to complete
        ///         the group entity to accurately reflect the scenario.
        ///     ResultDetails: Each course name (representing AcadResult) and the associated Result enum value expected for each item.
        /// </summary>
        [TestMethod]
        [DeploymentItem("Entities\\Requirements\\TestData\\GroupTestsData.xml")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\GroupTestsData.xml", "TestData", DataAccessMethod.Sequential)]
        public async Task GroupTest()
        {
            int testNumber = System.Convert.ToInt32(TestContext.DataRow["TestNumber"]);
            // if you need to test a specific test, set up an if statement + breakpoint combo to stop when testNumber == <desiredTestNumber>
            string phrase = System.Convert.ToString(TestContext.DataRow["Phrase"]);
            string strTaken = System.Convert.ToString(TestContext.DataRow["Taken"]);
            string strPlanned = System.Convert.ToString(TestContext.DataRow["Planned"]);
            string result = System.Convert.ToString(TestContext.DataRow["Result"]);//.ToUpper();
            string testFile = System.Convert.ToString(TestContext.DataRow["TestFile"]); // Case sensitive
            string resultDetails = System.Convert.ToString(TestContext.DataRow["ResultDetails"]);//.ToUpper();

            if (string.IsNullOrEmpty(testFile))
            {
                // no data yet
                return;
            }

            Console.WriteLine("TestNumber: " + testNumber);
            Console.WriteLine("TestFile: " + testFile);
            Console.WriteLine("Phrase: " + phrase);
            Console.WriteLine("Taken: " + strTaken);
            Console.WriteLine("Expected Group Result: " + result);


            ////for breakpoint on particular test
            //if (testNumber == 284)
            //{
            //    Console.WriteLine("Breaking on test " + testNumber);
            //}


            Group group;
            // Student student = TestUtil.FromJson<Student>(TestUtil.GetAssemblyName(), "Entities.Requirements.TestData.0000896.js");

            var allCredits = new TestAcademicCreditRepository().GetAsync().Result;
            var equatedCourses = new TestCourseRepository().GetAsync().Result;
            var allPlannedCourses = GetPlannedCourses(allCredits, equatedCourses);

            // This replaces the JSON group data from the TestXX.js files   
            // This must now be mocked up properly even though we are just testing the group.

            List<string> reqnames = new List<string>();
            List<string> subnames = new List<string>();
            List<string> grpnames = new List<string>() { testFile };
            Dictionary<string, List<string>> Subreq = new Dictionary<string, List<string>>();
            Dictionary<string, List<string>> groups = new Dictionary<string, List<string>>();

            // This is confusing but it makes sense in its own way
            reqnames.Add("GroupTestRequirement");           // list of requirement names to handle
            subnames.Add("GroupTestSubrequirement");        // list of Subrequirement names
            Subreq.Add(reqnames[0], subnames);              // dictionary as pointer from requirement name to list of Subrequirement names under it
            grpnames.Add(testFile);                         // list of group names
            groups.Add(subnames[0], grpnames);              // dictionary as pointer from Subrequirement name to list of group names under it

            ProgramRequirements pr = await new TestProgramRequirementsRepository().BuildTestProgramRequirementsAsync(testFile, reqnames, Subreq, groups);


            group = pr.Requirements.First().SubRequirements.First().Groups.First();

            List<AcademicCredit> taken = GetCredits(strTaken, allCredits.ToList());
            List<PlannedCredit> plannedCourses = GetPlannedCredits(strPlanned, allPlannedCourses.ToList());

            //  ****** this is where the magic happens
            // First add the credits taken from the specified test
            List<AcadResult> results = new List<AcadResult>();
            foreach (var cred in taken)
            {
                CreditResult credResult = new CreditResult(cred);
                // Test #285 looks at a STUDENT.ACAD.CRED where STC.ALTCUM.CONTRIB.CMPL.CRED != STC.CRED
                // The TestAcademicCreditRepository carries different numbers for this test but 
                // when the course data is used to determine the taken courses, the repo data is overridden;
                // Overriding here to ensure mismatch between the two credit properties.
                if (testNumber == 285)
                {
                    credResult.Credit.AdjustedCredit = credResult.Credit.Credit - 1m;
                }
                results.Add(credResult);
            }
            // Next add any planned courses from the specified test
            List<AcadResult> plannedResults = new List<AcadResult>();
            foreach (var pc in plannedCourses)
            {
                results.Add(new CourseResult(pc));
            }

            var allRules = group.GetRules();

            var requests = new List<RuleRequest<AcademicCredit>>();
            foreach (var rule in allRules)
            {
                foreach (var credit in taken)
                {
                    requests.Add(new RuleRequest<AcademicCredit>(rule.CreditRule, credit));
                }
            }
            var ruleResults = new TestRuleRepository2().Execute(requests);
            foreach (var rule in allRules)
            {
                var matchingResult = ruleResults.FirstOrDefault(rr => rr.RuleId == rule.CreditRule.Id);
                if (matchingResult != null)
                {
                    rule.SetAnswer(matchingResult.Passed, matchingResult.Context);
                }
            }

            // **********
            GroupResult gr = group.Evaluate(results, new List<Override> { new Override("10003", new List<string>() { "73" }, null) }, equatedCourses);

            // **********

            Assert.IsNotNull(gr);


            List<Tuple<AcademicCredit, PlannedCredit, Result>> expectedResults = ParseResultDetails(resultDetails, taken, plannedCourses);
            if (testNumber != 183)
            {
                Assert.AreEqual(taken.Count + plannedCourses.Count, expectedResults.Count, "Sanity check the ResultDetails column, should have all credits or planned courses listed");
            }
            foreach (var expectedResultTest in expectedResults)
            {
                Result expectedResultForTest = expectedResultTest.Item3;
                if (expectedResultTest.Item1 != null)
                {
                    var credit = expectedResultTest.Item1;
                    var creditResults = gr.Results.OfType<CreditResult>().ToList();
                    AcadResult ar = creditResults.FirstOrDefault(crr => ((CreditResult)crr).Credit.Equals(credit));
                    Assert.IsNotNull(ar);
                    try
                    {
                        Assert.AreEqual(expectedResultForTest, ar.Result); //, "Result for " + cr.Credit.Course + " should match");
                    }
                    catch (AssertFailedException)
                    {
                        Visit(gr);
                        throw new AssertFailedException("Expected academic credit result " + expectedResultForTest.ToString() + " does not match actual result " + ar.Result.ToString() +
                                                        " for course " + ar.GetCourse().ToString() + ".");
                    }
                }
                else
                {
                    // Since there is no academic credit this must be a planned course.
                    var pc = expectedResultTest.Item2;
                    var courseResults = gr.Results.OfType<CourseResult>().ToList();
                    AcadResult ar = courseResults.FirstOrDefault(prr => ((CourseResult)prr).PlannedCourse.Equals(pc));
                    try
                    {
                        Assert.AreEqual(expectedResultForTest, ar.Result); //, "Result for " + plannedCourse + " should match");
                    }
                    catch (AssertFailedException)
                    {
                        Visit(gr);
                        throw new AssertFailedException("Expected planned credit result " + expectedResultForTest.ToString() + " does not match actual result " + ar.Result.ToString() +
                                                        " for course " + ar.GetCourse().ToString() + ".");
                    }
                }
            }


            string actualresults = "";
            foreach (GroupExplanation ex in gr.Explanations)
            {
                if (!(actualresults == ""))
                {
                    actualresults += ",";
                }
                actualresults += ex.ToString();
            }


            Console.WriteLine("Actual Group Results " + actualresults);

            // Assert all expected group results are present
            string[] expectedGroupResults = result.Split(',');
            foreach (string expectedGroupResult in expectedGroupResults)
            {
                GroupExplanation e = (GroupExplanation)Enum.Parse(typeof(GroupExplanation), expectedGroupResult);
                try
                {
                    Assert.IsTrue(gr.Explanations.Contains(e));
                }
                catch (AssertFailedException)
                {
                    Visit(gr);
                    throw new AssertFailedException("Expected result " + expectedGroupResult + " not in actual results " + actualresults + ".");
                }

                // free(e);    haha just kidding
            }

            // Assert all group results present are expected
            foreach (GroupExplanation ee in gr.Explanations)
            {
                //Assert.IsTrue(expectedGroupResults.Contains(ee.ToString()));
                try
                {
                    Assert.IsTrue(expectedGroupResults.Contains(ee.ToString()));
                }
                catch (AssertFailedException)
                {
                    Visit(gr);
                    throw new AssertFailedException("Actual result " + ee.ToString() + " not in expected results " + result + ".");
                }
            }

            // Visit results for details
            Console.WriteLine("Individial Academic Goal Contributor (AGC) results:");
            Visit(gr);
        }

        private static List<Tuple<AcademicCredit, PlannedCredit, Result>> ParseResultDetails(string resultDetails, ICollection<AcademicCredit> credits, ICollection<PlannedCredit> plannedCourses)
        {
            List<Tuple<AcademicCredit, PlannedCredit, Result>> results = new List<Tuple<AcademicCredit, PlannedCredit, Result>>();
            if (!string.IsNullOrEmpty(resultDetails))
            {
                string[] assignments = resultDetails.Split(',');
                foreach (var assignment in assignments)
                {
                    var expectedResult = assignment;
                    bool isPlanned = false;

                    if (expectedResult.IndexOf("P:") >= 0)
                    {
                        isPlanned = true;
                        expectedResult = expectedResult.Substring(2);
                    }
                    string courseId = expectedResult.Split('=')[0];
                    string result = expectedResult.Split('=')[1];
                    PlannedCredit pc = null;
                    AcademicCredit credit = null;
                    if (isPlanned)
                    {
                        pc = GetPlannedCredit(courseId, plannedCourses);
                        Assert.IsNotNull(pc);
                    }
                    else
                    {
                        credit = GetCredit(courseId, credits);
                        Assert.IsNotNull(credit);
                    }
                    Result r = (Result)Enum.Parse(typeof(Result), result);
                    Assert.IsNotNull(r);
                    results.Add(Tuple.Create<AcademicCredit, PlannedCredit, Result>(credit, pc, r));
                }
            }
            return results;
        }

        private static List<string> GetCourseIds(string strCourseIds)
        {
            List<string> ids = new List<string>();
            if (!string.IsNullOrEmpty(strCourseIds))
            {
                ids.AddRange(strCourseIds.Split(','));
            }
            return ids;
        }

        private List<AcademicCredit> GetCredits(string strTaken, ICollection<AcademicCredit> credits)
        {
            List<AcademicCredit> taken = new List<AcademicCredit>();
            foreach (var courseId in GetCourseIds(strTaken))
            {
                taken.Add(GetCredit(courseId, credits));
            }
            return taken;
        }

        private List<PlannedCredit> GetPlannedCredits(string strTaken, ICollection<PlannedCredit> plannedCredits)
        {
            List<PlannedCredit> planned = new List<PlannedCredit>();
            foreach (var courseId in GetCourseIds(strTaken))
            {
                planned.Add(GetPlannedCredit(courseId, plannedCredits));
            }
            return planned;
        }

        private static AcademicCredit GetCredit(string courseId, ICollection<AcademicCredit> credits)
        {
            // Handle MATH-101 and MATH 101 as well as MATH*101

            courseId.Replace("-", "*");
            courseId.Replace(" ", "*");

            string subj = courseId.Trim().Split('*')[0];
            string num = courseId.Trim().Split('*')[1];

            return credits.First(ac => ac.Course.SubjectCode == subj && ac.Course.Number == num);
        }

        private static PlannedCredit GetPlannedCredit(string courseId, ICollection<PlannedCredit> plannedCredits)
        {
            // Handle MATH-101 and MATH 101 as well as MATH*101

            courseId.Replace("-", "*");
            courseId.Replace(" ", "*");

            string subj = courseId.Trim().Split('*')[0];
            string num = courseId.Trim().Split('*')[1];
            var plannedCourse = plannedCredits.First(pc => pc.Course.SubjectCode == subj && pc.Course.Number == num);
            return plannedCourse;
        }

        private static Group GetFirstGroup(ProgramRequirements pr)
        {
            return pr.Requirements.First(r => r.Id == "BSV.REQ1").SubRequirements.First(sr => sr.Id == "938").Groups.First();
        }

        // RTM FIX
        //public void Visit(ProgramEvaluation programResult)
        //{
        //    throw new NotImplementedException();
        //}

        public void Visit(RequirementResult requirementResult)
        {
            throw new NotImplementedException();
        }

        public void Visit(SubrequirementResult SubrequirementResult)
        {
            throw new NotImplementedException();
        }

        public void Visit(GroupResult gr)
        {

            foreach (var cr in gr.Results)
            {
                string type = cr.GetType() == typeof(CourseResult) ? "Planned Course: " : "Academic Credit: ";
                Console.WriteLine(type + cr.GetCourse().ToString() + "  " + cr.Result.ToString());
            }
        }
        // Convert the academic credits into viable planned courses.
        private List<PlannedCredit> GetPlannedCourses(IEnumerable<AcademicCredit> allCredits, IEnumerable<Course> allCourses)
        {
            var plannedCourses = new List<PlannedCredit>();
            foreach (var credit in allCredits)
            {
                if (credit.Course != null)
                {
                    string termCode = "Future";
                    var course = allCourses.Where(c => c.Id == credit.Course.Id).FirstOrDefault();
                    if (course != null)
                    {
                        var plannedCourse = new PlannedCredit(course, termCode);
                        plannedCourse.Credits = credit.Credit;
                        plannedCourses.Add(plannedCourse);
                    }

                }

            }

            return plannedCourses;
        }

        [TestMethod]
        public void Group_ToString()
        {
            Subrequirement subrequirement = new Subrequirement("S", "S");
            Group group = new Group("G", "G", subrequirement);
            Assert.AreEqual("G G", group.ToString());
        }
}

    [TestClass]
    public class GetRules
    {
        private Requirement requirement;
        private Subrequirement subrequirement;
        private Group group;

        [TestInitialize]
        public void Initialize()
        {
            requirement = new Requirement("1", "R1", "Req1", "UG", null);
            subrequirement = new Subrequirement("2", "subreq2");
            subrequirement.Requirement = requirement;
            group = new Group("1", "group1", subrequirement);
        }

        [TestMethod]
        public void GetsAllRules()
        {
            group.AcademicCreditRules = new List<RequirementRule>() { new RequirementRule(new Rule<AcademicCredit>("rule1")), new RequirementRule(new Rule<AcademicCredit>("rule2")) };
            group.MaxCoursesRule = new RequirementRule(new Rule<AcademicCredit>("rule3"));
            group.MaxCreditsRule = new RequirementRule(new Rule<AcademicCredit>("rule4"));
            var rules = group.GetRules();
            Assert.AreEqual(4, rules.Count());
            Assert.IsTrue(rules.Contains(group.AcademicCreditRules.ElementAt(0)));
            Assert.IsTrue(rules.Contains(group.AcademicCreditRules.ElementAt(1)));
            Assert.IsTrue(rules.Contains(group.MaxCoursesRule));
            Assert.IsTrue(rules.Contains(group.MaxCreditsRule));
        }

        [TestMethod]
        public void ReturnsEmptyListIfNoRules()
        {
            var rules = group.GetRules();
            Assert.AreEqual(0, rules.Count());
        }

        [TestMethod]
        public void ReturnsEmptyListExclusions()
        {
            Assert.AreEqual(0, group.Exclusions.Count());
        }
    }

    [TestClass]
    public class Group_GroupTypeEvalSequence
    {
        private Requirement requirement;
        private Subrequirement subreq;

        [TestInitialize]
        public void Initialize()
        {
            requirement = new Requirement("1", "R1", "Req1", "UG", null);
            subreq = new Subrequirement("11", "SUBREQ1");
            subreq.Requirement = requirement;
        }

        [TestMethod]
        public void SortsTakeAllFirst()
        {
            var g = new Group("1", "GROUP1", subreq);
            g.GroupType = GroupType.TakeAll;
            Assert.AreEqual(1, g.GroupTypeEvalSequence);
        }

        [TestMethod]
        public void SortsTakeSelectedSecond()
        {
            var g = new Group("1", "GROUP1", subreq);
            g.GroupType = GroupType.TakeSelected;
            Assert.AreEqual(2, g.GroupTypeEvalSequence);
        }

        [TestMethod]
        public void SortsTakeCoursesThird()
        {
            var g = new Group("1", "GROUP1", subreq);
            g.GroupType = GroupType.TakeCourses;
            Assert.AreEqual(3, g.GroupTypeEvalSequence);
        }

        [TestMethod]
        public void SortsTakeCreditsThird()
        {
            var g = new Group("1", "GROUP1", subreq);
            g.GroupType = GroupType.TakeCredits;
            Assert.AreEqual(3, g.GroupTypeEvalSequence);
        }

    }

    [TestClass]
    public class HasAcademicCreditBasedRules
    {
        private Subrequirement subreq;
        private Requirement requirement;

        [TestInitialize]
        public void Initialize()
        {
            subreq = new Subrequirement("11", "SUBREQ1");
            requirement = new Requirement("1", "R1", "Req1", "UG", null);
            subreq.Requirement = requirement;
        }

        [TestMethod]
        public void HasAcademicCreditBasedRules_TrueIfCombination()
        {
            var group = new Group("1", "group1", subreq);
            group.AcademicCreditRules = new List<RequirementRule>() { new RequirementRule(new Rule<Course>("rule1")), new RequirementRule(new Rule<AcademicCredit>("rule2")) };
            group.MaxCoursesRule = new RequirementRule(new Rule<AcademicCredit>("rule3"));
            group.MaxCreditsRule = new RequirementRule(new Rule<AcademicCredit>("rule4"));
            var rules = group.GetRules();
            Assert.AreEqual(4, rules.Count());
            Assert.IsTrue(group.HasAcademicCreditBasedRules);
        }

        [TestMethod]
        public void HasAcademicCreditBasedRules_FalseIfOnlyCourseRules()
        {
            var group = new Group("1", "group1", subreq);
            group.AcademicCreditRules = new List<RequirementRule>() { new RequirementRule(new Rule<Course>("rule1")), new RequirementRule(new Rule<Course>("rule2")) };
            group.MaxCoursesRule = new RequirementRule(new Rule<AcademicCredit>("rule3"));
            group.MaxCreditsRule = new RequirementRule(new Rule<AcademicCredit>("rule4"));
            var rules = group.GetRules();
            Assert.AreEqual(4, rules.Count());
            Assert.IsFalse(group.HasAcademicCreditBasedRules);
        }

        [TestMethod]
        public void HasAcademicCreditBasedRules_FalseIfNoRules()
        {
            var group = new Group("1", "group1", subreq);
            group.MaxCoursesRule = new RequirementRule(new Rule<AcademicCredit>("rule3"));
            group.MaxCreditsRule = new RequirementRule(new Rule<AcademicCredit>("rule4"));
            var rules = group.GetRules();
            Assert.AreEqual(2, rules.Count());
            Assert.IsFalse(group.HasAcademicCreditBasedRules);
        }

        [TestMethod]
        public void HasAcademicCreditBasedRules_FalseIfNullRules()
        {
            var group = new Group("1", "group1", subreq);
            group.AcademicCreditRules = null;
            group.MaxCoursesRule = new RequirementRule(new Rule<AcademicCredit>("rule3"));
            group.MaxCreditsRule = new RequirementRule(new Rule<AcademicCredit>("rule4"));
            var rules = group.GetRules();
            Assert.AreEqual(2, rules.Count());
            Assert.IsFalse(group.HasAcademicCreditBasedRules);
        }
    }

    [TestClass]
    public class OnlyConveysPrintText
    {
        private string id;
        private string code;
        private Subrequirement subrequirement;
        private Group group;

        [TestInitialize]
        public void Initialize_OnlyConveysPrintText()
        {
            id = "1";
            code = "code";
            subrequirement = new Subrequirement(id + "S", code + "S");
            group = new Group(id, code, subrequirement);
        }

        [TestMethod]
        public void Group_OnlyConveysPrintText_True_Take_0_Credits()
        {
            group.MinCredits = 0m;
            group.GroupType = GroupType.TakeCredits;
            group.ButNotDepartments = null;
            group.ButNotCourseLevels = null;
            group.ButNotSubjects = null;
            group.ButNotCourses = null;
            group.Courses = null;
            group.FromCourses = null;
            group.FromDepartments = null;
            group.FromSubjects = null;
            group.FromLevels = null;
            group.FromCoursesException = null;
            group.Exclusions = null;
            Assert.IsTrue(group.OnlyConveysPrintText);
        }

        [TestMethod]
        public void Group_OnlyConveysPrintText_False_Take_0_Courses()
        {
            group.MinCourses = 0;
            group.GroupType = GroupType.TakeCourses;
            Assert.IsFalse(group.OnlyConveysPrintText);
        }

        [TestMethod]
        public void Group_OnlyConveysPrintText_False_AcademicCreditRules_has_value()
        {
            group.MinCredits = 0;
            group.AcademicCreditRules = new List<RequirementRule>() { new RequirementRule(new Rule<AcademicCredit>("Code")) };
            Assert.IsFalse(group.OnlyConveysPrintText);
        }

        [TestMethod]
        public void Group_OnlyConveysPrintText_False_CourseBasedRules_has_value()
        {
            group.MinCredits = 0;
            group.AcademicCreditRules = new List<RequirementRule>() { new RequirementRule(new Rule<Course>("Code")) };
            Assert.IsFalse(group.OnlyConveysPrintText);
        }

        [TestMethod]
        public void Group_OnlyConveysPrintText_False_MaxCourses_has_value()
        {
            group.MinCredits = 0;
            group.GroupType = GroupType.TakeCredits;
            group.MaxCourses = 1;
            Assert.IsFalse(group.OnlyConveysPrintText);
        }

        [TestMethod]
        public void Group_OnlyConveysPrintText_False_MaxCoursesAtLevels_has_value()
        {
            group.MinCredits = 0;
            group.GroupType = GroupType.TakeCredits;
            group.MaxCoursesAtLevels = new MaxCoursesAtLevels(5, new List<string>());
            Assert.IsFalse(group.OnlyConveysPrintText);
        }

        [TestMethod]
        public void Group_OnlyConveysPrintText_False_MaxCoursesPerDepartment_has_value()
        {
            group.MinCredits = 0;
            group.GroupType = GroupType.TakeCredits;
            group.MaxCoursesPerDepartment = 1;
            Assert.IsFalse(group.OnlyConveysPrintText);
        }

        [TestMethod]
        public void Group_OnlyConveysPrintText_False_MaxCoursesPerRule_has_value()
        {
            group.MinCredits = 0;
            group.GroupType = GroupType.TakeCredits;
            group.MaxCoursesPerRule = 1;
            Assert.IsFalse(group.OnlyConveysPrintText);
        }

        [TestMethod]
        public void Group_OnlyConveysPrintText_False_MaxCoursesPerSubject_has_value()
        {
            group.MinCredits = 0;
            group.GroupType = GroupType.TakeCredits;
            group.MaxCoursesPerSubject = 1;
            Assert.IsFalse(group.OnlyConveysPrintText);
        }

        [TestMethod]
        public void Group_OnlyConveysPrintText_False_MaxCoursesRule_has_value()
        {
            group.MinCredits = 0;
            group.GroupType = GroupType.TakeCredits;
            group.MaxCoursesRule = new RequirementRule(new Rule<AcademicCredit>("RULE"));
            Assert.IsFalse(group.OnlyConveysPrintText);
        }

        [TestMethod]
        public void Group_OnlyConveysPrintText_False_MaxCredits_has_value()
        {
            group.MinCredits = 0;
            group.GroupType = GroupType.TakeCredits;
            group.MaxCredits = 1;
            Assert.IsFalse(group.OnlyConveysPrintText);
        }

        [TestMethod]
        public void Group_OnlyConveysPrintText_False_MaxCreditsAtLevels_has_value()
        {
            group.MinCredits = 0;
            group.GroupType = GroupType.TakeCredits;
            group.MaxCreditsAtLevels = new MaxCreditAtLevels(5, new List<string>());
            Assert.IsFalse(group.OnlyConveysPrintText);
        }

        [TestMethod]
        public void Group_OnlyConveysPrintText_False_MaxCreditsPerCourse_has_value()
        {
            group.MinCredits = 0;
            group.GroupType = GroupType.TakeCredits;
            group.MaxCreditsPerCourse = 1;
            Assert.IsFalse(group.OnlyConveysPrintText);
        }

        [TestMethod]
        public void Group_OnlyConveysPrintText_False_MaxCreditsPerDepartment_has_value()
        {
            group.MinCredits = 0;
            group.GroupType = GroupType.TakeCredits;
            group.MaxCreditsPerDepartment = 1;
            Assert.IsFalse(group.OnlyConveysPrintText);
        }

        [TestMethod]
        public void Group_OnlyConveysPrintText_False_MaxCreditsPerRule_has_value()
        {
            group.MinCredits = 0;
            group.GroupType = GroupType.TakeCredits;
            group.MaxCreditsPerRule = 1;
            Assert.IsFalse(group.OnlyConveysPrintText);
        }

        [TestMethod]
        public void Group_OnlyConveysPrintText_False_MaxCreditsPerSubject_has_value()
        {
            group.MinCredits = 0;
            group.GroupType = GroupType.TakeCredits;
            group.MaxCreditsPerSubject = 1;
            Assert.IsFalse(group.OnlyConveysPrintText);
        }

        [TestMethod]
        public void Group_OnlyConveysPrintText_False_MaxCreditsRule_has_value()
        {
            group.MinCredits = 0;
            group.GroupType = GroupType.TakeCredits;
            group.MaxCreditsRule = new RequirementRule(new Rule<AcademicCredit>("RULE"));
            Assert.IsFalse(group.OnlyConveysPrintText);
        }

        [TestMethod]
        public void Group_OnlyConveysPrintText_False_MaxDepartments_has_value()
        {
            group.MinCredits = 0;
            group.GroupType = GroupType.TakeCredits;
            group.MaxDepartments = 1;
            Assert.IsFalse(group.OnlyConveysPrintText);
        }

        [TestMethod]
        public void Group_OnlyConveysPrintText_False_MaxSubjects_has_value()
        {
            group.MinCredits = 0;
            group.GroupType = GroupType.TakeCredits;
            group.MaxSubjects = 1;
            Assert.IsFalse(group.OnlyConveysPrintText);
        }

        [TestMethod]
        public void Group_OnlyConveysPrintText_False_MinCoursesPerDepartment_has_value()
        {
            group.MinCredits = 0;
            group.GroupType = GroupType.TakeCredits;
            group.MinCoursesPerDepartment = 1;
            Assert.IsFalse(group.OnlyConveysPrintText);
        }

        [TestMethod]
        public void Group_OnlyConveysPrintText_False_MinCoursesPerSubject_has_value()
        {
            group.MinCredits = 0;
            group.GroupType = GroupType.TakeCredits;
            group.MinCoursesPerSubject = 1;
            Assert.IsFalse(group.OnlyConveysPrintText);
        }

        [TestMethod]
        public void Group_OnlyConveysPrintText_False_MinCreditsPerCourse_has_value()
        {
            group.MinCredits = 0;
            group.GroupType = GroupType.TakeCredits;
            group.MinCreditsPerCourse = 1;
            Assert.IsFalse(group.OnlyConveysPrintText);
        }

        [TestMethod]
        public void Group_OnlyConveysPrintText_False_MinCreditsPerDepartment_has_value()
        {
            group.MinCredits = 0;
            group.GroupType = GroupType.TakeCredits;
            group.MinCreditsPerDepartment = 1;
            Assert.IsFalse(group.OnlyConveysPrintText);
        }

        [TestMethod]
        public void Group_OnlyConveysPrintText_False_MinCreditsPerSubject_has_value()
        {
            group.MinCredits = 0;
            group.GroupType = GroupType.TakeCredits;
            group.MinCreditsPerSubject = 1;
            Assert.IsFalse(group.OnlyConveysPrintText);
        }

        [TestMethod]
        public void Group_OnlyConveysPrintText_False_MinDepartments_has_value()
        {
            group.MinCredits = 0;
            group.GroupType = GroupType.TakeCredits;
            group.MinDepartments = 1;
            Assert.IsFalse(group.OnlyConveysPrintText);
        }

        [TestMethod]
        public void Group_OnlyConveysPrintText_False_MinGpa_has_value()
        {
            group.MinCredits = 0;
            group.GroupType = GroupType.TakeCredits;
            group.MinGpa = 1;
            Assert.IsFalse(group.OnlyConveysPrintText);
        }

        [TestMethod]
        public void Group_OnlyConveysPrintText_True_MinGrade_has_value()
        {
            group.MinCredits = 0;
            group.GroupType = GroupType.TakeCredits;
            group.MinGrade = new Grade("A", "A", "UG");
            Assert.IsTrue(group.OnlyConveysPrintText);
        }

        [TestMethod]
        public void Group_OnlyConveysPrintText_True_MinGrade_no_value()
        {
            group.MinCredits = 0;
            group.GroupType = GroupType.TakeCredits;
            group.MinGrade = null;
            Assert.IsTrue(group.OnlyConveysPrintText);
        }

        [TestMethod]
        public void Group_OnlyConveysPrintText_False_MinInstitutionalCredits_has_value()
        {
            group.MinCredits = 0;
            group.GroupType = GroupType.TakeCredits;
            group.MinInstitutionalCredits = 1;
            Assert.IsFalse(group.OnlyConveysPrintText);
        }

        [TestMethod]
        public void Group_OnlyConveysPrintText_False_MinSubjects_has_value()
        {
            group.MinCredits = 0;
            group.GroupType = GroupType.TakeCredits;
            group.MinSubjects = 1;
            Assert.IsFalse(group.OnlyConveysPrintText);
        }

        [TestMethod]
        public void Group_OnlyConveysPrintText_False_ButNotCourseLevels_has_value()
        {
            group.MinCredits = 0;
            group.GroupType = GroupType.TakeCredits;
            group.ButNotCourseLevels = new List<string>() { "ABC" };
            Assert.IsFalse(group.OnlyConveysPrintText);
        }

        [TestMethod]
        public void Group_OnlyConveysPrintText_False_ButNotCourses_has_value()
        {
            group.MinCredits = 0;
            group.GroupType = GroupType.TakeCredits;
            group.ButNotCourses = new List<string>() { "ABC" };
            Assert.IsFalse(group.OnlyConveysPrintText);
        }

        [TestMethod]
        public void Group_OnlyConveysPrintText_False_ButNotDepartments_has_value()
        {
            group.MinCredits = 0;
            group.GroupType = GroupType.TakeCredits;
            group.ButNotDepartments = new List<string>() { "ABC" };
            Assert.IsFalse(group.OnlyConveysPrintText);
        }

        [TestMethod]
        public void Group_OnlyConveysPrintText_False_ButNotSubjects_has_value()
        {
            group.MinCredits = 0;
            group.GroupType = GroupType.TakeCredits;
            group.ButNotSubjects = new List<string>() { "ABC" };
            Assert.IsFalse(group.OnlyConveysPrintText);
        }

        [TestMethod]
        public void Group_OnlyConveysPrintText_False_Exclusions_has_value()
        {
            group.MinCredits = 0;
            group.GroupType = GroupType.TakeCredits;
            group.Exclusions = new List<string>() { "ABC" };
            Assert.IsFalse(group.OnlyConveysPrintText);
        }

        [TestMethod]
        public void Group_OnlyConveysPrintText_False_FromCourses_has_value()
        {
            group.MinCredits = 0;
            group.GroupType = GroupType.TakeCredits;
            group.FromCourses = new List<string>() { "ABC" };
            Assert.IsFalse(group.OnlyConveysPrintText);
        }

        [TestMethod]
        public void Group_OnlyConveysPrintText_False_FromCoursesException_has_value()
        {
            group.MinCredits = 0;
            group.GroupType = GroupType.TakeCredits;
            group.FromCoursesException = new List<string>() { "ABC" };
            Assert.IsFalse(group.OnlyConveysPrintText);
        }

        [TestMethod]
        public void Group_OnlyConveysPrintText_False_FromDepartments_has_value()
        {
            group.MinCredits = 0;
            group.GroupType = GroupType.TakeCredits;
            group.FromDepartments = new List<string>() { "ABC" };
            Assert.IsFalse(group.OnlyConveysPrintText);
        }

        [TestMethod]
        public void Group_OnlyConveysPrintText_False_FromLevels_has_value()
        {
            group.MinCredits = 0;
            group.GroupType = GroupType.TakeCredits;
            group.FromLevels = new List<string>() { "ABC" };
            Assert.IsFalse(group.OnlyConveysPrintText);
        }

        [TestMethod]
        public void Group_OnlyConveysPrintText_False_FromSubjects_has_value()
        {
            group.MinCredits = 0;
            group.GroupType = GroupType.TakeCredits;
            group.FromSubjects = new List<string>() { "ABC" };
            Assert.IsFalse(group.OnlyConveysPrintText);
        }

        [TestMethod]
        public void Group_OnlyConveysPrintText_False_Courses_has_value()
        {
            group.MinCredits = 0;
            group.GroupType = GroupType.TakeCredits;
            group.Courses = new List<string>() { "ABC" };
            Assert.IsFalse(group.OnlyConveysPrintText);
        }
    }

    [TestClass]
    public class Evaluate
    {
        private string id;
        private string code;
        private Subrequirement subrequirement;
        private Group group;
        private Course course;
        private List<AcadResult> acadResults;
        private GroupResult gr;
        private List<Course> equatedCourses;

        [TestMethod]
        public void Evaluate_Results_when_AcadResult_CheckCredit_does_not_exclude_course_based_on_equated_course_level()
        {
            // Set up an acad result whose course level is not in the group's ButNotCourseLevels,
            // with an equated course whose course level *is* in the group's ButNotCourseLevels.
            // This acad result should *NOT* have a result of LevelExcluded because equated course level codes
            // are not considered when determining if an acad result should be excluded
            id = "1";
            code = "code";
            subrequirement = new Subrequirement(id + "S", code + "S")
            {
                IsWaived = false,
                Requirement = new Requirement(id + "R", code + "R", "requirement", "UG", new RequirementType(code + "rtype", "requirementtype", "1")) { IsWaived = false }
            };
            group = new Group(id, code, subrequirement)
            {
                ButNotCourseLevels = new List<string>() { "200" },
                IsWaived = false
            };
            course = new Course("2", "short", "long", new List<OfferingDepartment>()
            {
                new OfferingDepartment("MATH", 100m)
            }, "MATH", "200", "2", new List<string>() { "100" }, 3m, null, new List<CourseApproval>()
            {
                new CourseApproval("A", DateTime.Today.AddDays(-7), "0000043", "0003315", DateTime.Today.AddDays(-7))
            });
            course.AddEquatedCourseId(new TestCourseRepository().GetAsync().Result.ToList()[4].Id);
            acadResults = new List<AcadResult>()
            {
                new CreditResult(new AcademicCredit("3", course, "4")
                {
                    CourseLevelCode = "100",
                })
                {
                    Result = Result.LevelExcluded
                }
            };
            equatedCourses = new TestCourseRepository().GetAsync().Result.ToList();

            gr = group.Evaluate(acadResults, null, equatedCourses);
            // Assert that the group's ButNotCourseLevels matches those found on the equated course for the acad credit result
            CollectionAssert.AreEqual(group.ButNotCourseLevels, equatedCourses.Where(ec => course.EquatedCourseIds.Contains(ec.Id)).FirstOrDefault().CourseLevelCodes);
            // Assert that the group's ButNotCourseLevels does not match the acad result's course level
            Assert.AreNotEqual(group.ButNotCourseLevels[0], acadResults[0].GetCourseLevels().ToList()[0]);
            Assert.AreEqual(1, gr.Results.Count);
            Assert.AreNotEqual(Result.LevelExcluded, gr.Results[0].Result);
        }

        [TestMethod]
        public void Evaluate_Results_when_AcadResult_CheckCredit_excludes_course_based_on_course_level()
        {
            // Set up an acad result whose course level *is* in the group's ButNotCourseLevels.
            // This acad result should have a result of LevelExcluded
            id = "1";
            code = "code";
            subrequirement = new Subrequirement(id + "S", code + "S")
            {
                IsWaived = false,
                Requirement = new Requirement(id + "R", code + "R", "requirement", "UG", new RequirementType(code + "rtype", "requirementtype", "1")) { IsWaived = false }
            };
            group = new Group(id, code, subrequirement)
            {
                ButNotCourseLevels = new List<string>() { "200" },
                IsWaived = false
            };
            course = new Course("2", "short", "long", new List<OfferingDepartment>()
            {
                new OfferingDepartment("MATH", 100m)
            }, "MATH", "200", "2", group.ButNotCourseLevels, 3m, null, new List<CourseApproval>()
            {
                new CourseApproval("A", DateTime.Today.AddDays(-7), "0000043", "0003315", DateTime.Today.AddDays(-7))
            });
            course.AddEquatedCourseId(new TestCourseRepository().GetAsync().Result.ToList()[4].Id);
            acadResults = new List<AcadResult>()
            {
                new CreditResult(new AcademicCredit("3", course, "4")
                {
                    CourseLevelCode = group.ButNotCourseLevels[0],
                })
                {
                    Result = Result.LevelExcluded
                }
            };
            equatedCourses = new TestCourseRepository().GetAsync().Result.ToList();

            gr = group.Evaluate(acadResults, null, equatedCourses);
            // Assert that the group's ButNotCourseLevels matches the acad result's course level
            Assert.AreEqual(group.ButNotCourseLevels[0], acadResults[0].GetCourseLevels().ToList()[0]);
            Assert.AreEqual(1, gr.Results.Count);
            Assert.AreEqual(Result.LevelExcluded, gr.Results[0].Result);
        }

    }
    [TestClass]
    public class Evaluate_Courses_ExcludedFromTranscriptGrouping
    {
        [TestMethod]
        public void Credits_filtered_from_transcriptgrouping_With_MinCourses_Dept_Syntax()
        {
            Requirement requirement = new Requirement("1", "R1", "Req1", "UG", null);
             Subrequirement sreq = new Subrequirement("sreq-1", "subReq1");
            sreq.Requirement = requirement;

            //Take 2 courses from MATH
            Group gr = new Group("group-1", "group1", sreq);
            gr.MinCourses = 2;
            gr.FromSubjects = new List<string>() { "MATH" };

          

            //other acad credits
            AcademicCredit acadcredit1 = new AcademicCredit("acadcred-1");
            acadcredit1.AcademicLevelCode = "UG";
            acadcredit1.AddDepartment("MATH");
            acadcredit1.CompletedCredit = 3m;
            acadcredit1.CourseName = "MATH-100";
            acadcredit1.CourseLevelCode = "300";
            acadcredit1.SubjectCode = "MATH";
            acadcredit1.SectionId = "sec1";

            AcademicCredit acadcredit2 = new AcademicCredit("acadcred-2");
            acadcredit2.AcademicLevelCode = "GR";
            acadcredit2.AddDepartment("MATH");
            acadcredit2.CompletedCredit = 5m;
            acadcredit2.CourseName = "MATH-500";
            acadcredit2.CourseLevelCode = "500";
            acadcredit2.SubjectCode = "MATH";
            acadcredit2.SectionId = "sec2";

          
            List<Override> overrides = new List<Override>();
            overrides.Add(new Override("group-1", new List<string>() { "acadcred-2" }, new List<string>()));


            //Acad results

            List<AcadResult> acadresults = new List<AcadResult>();
            acadresults.Add(new CreditResult(acadcredit1));

           GroupResult grResult= gr.Evaluate(acadresults, overrides, null, new List<AcademicCredit>() { acadcredit2 });

            Assert.AreEqual(2, grResult.Results.Count());
            Assert.AreEqual(1, grResult.ForceAppliedAcademicCreditIds.Count());
            Assert.AreEqual("acadcred-2", grResult.ForceAppliedAcademicCreditIds[0]);
            Assert.AreEqual("acadcred-2",grResult.Results[0].GetAcadCredId());
            Assert.AreEqual("acadcred-1", grResult.Results[1].GetAcadCredId());
            Assert.IsTrue(grResult.Explanations.Contains(GroupExplanation.Satisfied));
        }

        [TestMethod]
        public void Credits_filtered_from_transcriptgrouping_With_MinCourses_CourseLevel_Syntax()
        {
            Requirement requirement = new Requirement("1", "R1", "Req1", "UG", null);
            Subrequirement sreq = new Subrequirement("sreq-1", "subReq1");
            sreq.Requirement = requirement;

            //Take 2 courses from Level 300
            Group gr = new Group("group-1", "group1", sreq);
            gr.MinCourses = 2;
            gr.FromLevels = new List<string>() { "300" };



            //other acad credits
            AcademicCredit acadcredit1 = new AcademicCredit("acadcred-1");
            acadcredit1.AcademicLevelCode = "UG";
            acadcredit1.AddDepartment("MATH");
            acadcredit1.CompletedCredit = 3m;
            acadcredit1.CourseName = "MATH-100";
            acadcredit1.CourseLevelCode = "300";
            acadcredit1.SubjectCode = "MATH";
            acadcredit1.SectionId = "sec1";

            AcademicCredit acadcredit2 = new AcademicCredit("acadcred-2");
            acadcredit2.AcademicLevelCode = "GR";
            acadcredit2.AddDepartment("MATH");
            acadcredit2.CompletedCredit = 5m;
            acadcredit2.CourseName = "MATH-500";
            acadcredit2.CourseLevelCode = "500";
            acadcredit2.SubjectCode = "MATH";
            acadcredit2.SectionId = "sec2";


            List<Override> overrides = new List<Override>();
            overrides.Add(new Override("group-1", new List<string>() { "acadcred-2" }, new List<string>()));


            //Acad results

            List<AcadResult> acadresults = new List<AcadResult>();
            acadresults.Add(new CreditResult(acadcredit1));

            GroupResult grResult = gr.Evaluate(acadresults, overrides, null, new List<AcademicCredit>() { acadcredit2 });

            Assert.AreEqual(2, grResult.Results.Count());
            Assert.AreEqual(1, grResult.ForceAppliedAcademicCreditIds.Count());
            Assert.AreEqual("acadcred-2", grResult.ForceAppliedAcademicCreditIds[0]);
            Assert.AreEqual("acadcred-2", grResult.Results[0].GetAcadCredId());
            Assert.AreEqual("acadcred-1", grResult.Results[1].GetAcadCredId());
            Assert.IsTrue(grResult.Explanations.Contains(GroupExplanation.Satisfied));
        }
    }

    [TestClass]
    public class Evaluate_Related_Courses
    {
        [TestMethod]
        public void Related_Courses_With_ShowRelatedCourses()
        {
            Requirement requirement = new Requirement("1", "R1", "Req1", "UG", null);
            Subrequirement sreq = new Subrequirement("sreq-1", "subReq1");
            sreq.Requirement = requirement;

            //Take 2 courses from MATH
            Group gr = new Group("group-1", "group1", sreq);
            gr.MinCourses = 2;
            gr.FromSubjects = new List<string>() { "MATH" };



            //other acad credits
            AcademicCredit acadcredit1 = new AcademicCredit("acadcred-1");
            acadcredit1.AcademicLevelCode = "UG";
            acadcredit1.AddDepartment("MATH");
            acadcredit1.CompletedCredit = 3m;
            acadcredit1.CourseName = "MATH-100";
            acadcredit1.CourseLevelCode = "300";
            acadcredit1.SubjectCode = "MATH";
            acadcredit1.SectionId = "sec1";

            AcademicCredit acadcredit2 = new AcademicCredit("acadcred-2");
            acadcredit2.AcademicLevelCode = "UG";
            acadcredit2.AddDepartment("MATH");
            acadcredit2.CompletedCredit = 5m;
            acadcredit2.CourseName = "MATH-500";
            acadcredit2.CourseLevelCode = "500";
            acadcredit2.SubjectCode = "MATH";
            acadcredit2.SectionId = "sec2";

            AcademicCredit acadcredit3 = new AcademicCredit("acadcred-3");
            acadcredit3.AcademicLevelCode = "UG";
            acadcredit3.AddDepartment("MATH");
            acadcredit3.CompletedCredit = 3m;
            acadcredit3.CourseName = "MATH-200";
            acadcredit3.CourseLevelCode = "200";
            acadcredit3.SubjectCode = "MATH";
            acadcredit3.SectionId = "sec1";

            AcademicCredit acadcredit4 = new AcademicCredit("acadcred-4");
            acadcredit4.AcademicLevelCode = "UG";
            acadcredit4.AddDepartment("MATH");
            acadcredit4.CompletedCredit = 5m;
            acadcredit4.CourseName = "MATH-300";
            acadcredit4.CourseLevelCode = "300";
            acadcredit4.SubjectCode = "MATH";
            acadcredit4.SectionId = "sec2";

            AcademicCredit acadcredit5 = new AcademicCredit("acadcred-5");
            acadcredit5.AcademicLevelCode = "UG";
            acadcredit5.AddDepartment("ENGL");
            acadcredit5.CompletedCredit = 5m;
            acadcredit5.CourseName = "ENGL-300";
            acadcredit5.CourseLevelCode = "300";
            acadcredit5.SubjectCode = "ENGL";
            acadcredit5.SectionId = "sec2";

            //Acad results

            List<AcadResult> acadresults = new List<AcadResult>();
            acadresults.Add(new CreditResult(acadcredit1));
            acadresults.Add(new CreditResult(acadcredit2));
            acadresults.Add(new CreditResult(acadcredit3));
            acadresults.Add(new CreditResult(acadcredit4));
            acadresults.Add(new CreditResult(acadcredit5));

            GroupResult grResult = gr.Evaluate(acadresults, null, null, null,true,false);

            Assert.AreEqual(5, grResult.Results.Count());
            Assert.AreEqual(2, grResult.GetApplied().Count());
            Assert.AreEqual(2, grResult.GetRelated().Count());
            Assert.AreEqual("acadcred-1", grResult.GetApplied().ToList()[0].GetAcadCredId());
            Assert.AreEqual("acadcred-2", grResult.GetApplied().ToList()[1].GetAcadCredId());
            Assert.IsTrue(grResult.Explanations.Contains(GroupExplanation.Satisfied));
        }

        [TestMethod]
        public void Related_Courses_With_ShowRelatedCourses_false()
        {
            Requirement requirement = new Requirement("1", "R1", "Req1", "UG", null);
            Subrequirement sreq = new Subrequirement("sreq-1", "subReq1");
            sreq.Requirement = requirement;

            //Take 2 courses from MATH
            Group gr = new Group("group-1", "group1", sreq);
            gr.MinCourses = 2;
            gr.FromSubjects = new List<string>() { "MATH" };



            //other acad credits
            AcademicCredit acadcredit1 = new AcademicCredit("acadcred-1");
            acadcredit1.AcademicLevelCode = "UG";
            acadcredit1.AddDepartment("MATH");
            acadcredit1.CompletedCredit = 3m;
            acadcredit1.CourseName = "MATH-100";
            acadcredit1.CourseLevelCode = "300";
            acadcredit1.SubjectCode = "MATH";
            acadcredit1.SectionId = "sec1";

            AcademicCredit acadcredit2 = new AcademicCredit("acadcred-2");
            acadcredit2.AcademicLevelCode = "UG";
            acadcredit2.AddDepartment("MATH");
            acadcredit2.CompletedCredit = 5m;
            acadcredit2.CourseName = "MATH-500";
            acadcredit2.CourseLevelCode = "500";
            acadcredit2.SubjectCode = "MATH";
            acadcredit2.SectionId = "sec2";

            AcademicCredit acadcredit3 = new AcademicCredit("acadcred-3");
            acadcredit3.AcademicLevelCode = "UG";
            acadcredit3.AddDepartment("MATH");
            acadcredit3.CompletedCredit = 3m;
            acadcredit3.CourseName = "MATH-200";
            acadcredit3.CourseLevelCode = "200";
            acadcredit3.SubjectCode = "MATH";
            acadcredit3.SectionId = "sec1";

            AcademicCredit acadcredit4 = new AcademicCredit("acadcred-4");
            acadcredit4.AcademicLevelCode = "UG";
            acadcredit4.AddDepartment("MATH");
            acadcredit4.CompletedCredit = 5m;
            acadcredit4.CourseName = "MATH-300";
            acadcredit4.CourseLevelCode = "300";
            acadcredit4.SubjectCode = "MATH";
            acadcredit4.SectionId = "sec2";

            AcademicCredit acadcredit5 = new AcademicCredit("acadcred-5");
            acadcredit5.AcademicLevelCode = "UG";
            acadcredit5.AddDepartment("ENGL");
            acadcredit5.CompletedCredit = 5m;
            acadcredit5.CourseName = "ENGL-300";
            acadcredit5.CourseLevelCode = "300";
            acadcredit5.SubjectCode = "ENGL";
            acadcredit5.SectionId = "sec2";

            //Acad results

            List<AcadResult> acadresults = new List<AcadResult>();
            acadresults.Add(new CreditResult(acadcredit1));
            acadresults.Add(new CreditResult(acadcredit2));
            acadresults.Add(new CreditResult(acadcredit3));
            acadresults.Add(new CreditResult(acadcredit4));
            acadresults.Add(new CreditResult(acadcredit5));

            GroupResult grResult = gr.Evaluate(acadresults, null, null, null, false, false);

            Assert.AreEqual(5, grResult.Results.Count());
            Assert.AreEqual(2, grResult.GetApplied().Count());
            Assert.IsNull(grResult.GetRelated());
            Assert.AreEqual("acadcred-1", grResult.GetApplied().ToList()[0].GetAcadCredId());
            Assert.AreEqual("acadcred-2", grResult.GetApplied().ToList()[1].GetAcadCredId());
            Assert.IsTrue(grResult.Explanations.Contains(GroupExplanation.Satisfied));
        }

        [TestMethod]
        public void Related_Courses_With_ShowRelatedCourses_IncludePlannedCourses_WithPossibleReplaceInProgress_Status()
        {
            //Any planned course with Possible replace in progress status will also be included as Related course
            //Before group is evaluated list of academic credits and planned course are already marked with status of possible replace in progress or possible replacement
            //Group evaluation skips those courses which are possible replace in prgress and mark the Result status as ReplaceInProgress which is included to display as Related. 
            Requirement requirement = new Requirement("1", "R1", "Req1", "UG", null);
            Subrequirement sreq = new Subrequirement("sreq-1", "subReq1");
            sreq.Requirement = requirement;

            //Take 2 courses from MATH
            Group gr = new Group("group-1", "group1", sreq);
            gr.MinCourses = 2;
            gr.FromSubjects = new List<string>() { "MATH" };

            Course course = new Course("2", "short", "long", new List<OfferingDepartment>()
            {
                new OfferingDepartment("MATH", 100m)
            }, "MATH", "100", "2", new List<string>() {"100" }, 3m, null, new List<CourseApproval>()
            {
                new CourseApproval("A", DateTime.Today.AddDays(-7), "0000043", "0003315", DateTime.Today.AddDays(-7))
            });

            //MATH-100 is taken 1 time and planned twice in 2020/fa and 2021/fa terms. Since we are testing group evaluation then the academic credits and planned courses passed to 
            //evaluate method are already updated properly with Replaced and Replacement status. Therefore in data setup:
            //MATH-100 have one academic credit and two planned courses - planed courses are always picked over completed courses and IP courses. 
            //Therefore academic credit is possible be replaced by planned course in 2020/fa which will further possible be replace in progress with planned course in 2021/fa. Hence will be Related courses
            //Pnly planned course in 2021/fa will be planned applied in group. 

            //other acad credits
            AcademicCredit acadcredit1 = new AcademicCredit("acadcred-1", course, "sec1");
            acadcredit1.AcademicLevelCode = "UG";
            acadcredit1.AddDepartment("MATH");
            acadcredit1.CompletedCredit = 3m;
            acadcredit1.CourseName = "MATH-100";
            acadcredit1.CourseLevelCode = "300";
            acadcredit1.SubjectCode = "MATH";
            acadcredit1.ReplacedStatus = ReplacedStatus.ReplaceInProgress;
            acadcredit1.ReplacementStatus = ReplacementStatus.NotReplacement;

            AcademicCredit acadcredit2 = new AcademicCredit("acadcred-2");
            acadcredit2.AcademicLevelCode = "UG";
            acadcredit2.AddDepartment("MATH");
            acadcredit2.CompletedCredit = 5m;
            acadcredit2.CourseName = "MATH-500";
            acadcredit2.CourseLevelCode = "500";
            acadcredit2.SubjectCode = "MATH";
            acadcredit2.SectionId = "sec2";

            AcademicCredit acadcredit3 = new AcademicCredit("acadcred-3");
            acadcredit3.AcademicLevelCode = "UG";
            acadcredit3.AddDepartment("MATH");
            acadcredit3.CompletedCredit = 3m;
            acadcredit3.CourseName = "MATH-200";
            acadcredit3.CourseLevelCode = "200";
            acadcredit3.SubjectCode = "MATH";
            acadcredit3.SectionId = "sec1";

            AcademicCredit acadcredit4 = new AcademicCredit("acadcred-4");
            acadcredit4.AcademicLevelCode = "UG";
            acadcredit4.AddDepartment("MATH");
            acadcredit4.CompletedCredit = 5m;
            acadcredit4.CourseName = "MATH-300";
            acadcredit4.CourseLevelCode = "300";
            acadcredit4.SubjectCode = "MATH";
            acadcredit4.SectionId = "sec2";

            AcademicCredit acadcredit5 = new AcademicCredit("acadcred-5");
            acadcredit5.AcademicLevelCode = "UG";
            acadcredit5.AddDepartment("ENGL");
            acadcredit5.CompletedCredit = 5m;
            acadcredit5.CourseName = "ENGL-300";
            acadcredit5.CourseLevelCode = "300";
            acadcredit5.SubjectCode = "ENGL";
            acadcredit5.SectionId = "sec2";

            PlannedCredit plannedCourse1 = new PlannedCredit(course, "2020/FA");
            plannedCourse1.ReplacedStatus = ReplacedStatus.ReplaceInProgress;
            plannedCourse1.ReplacementStatus = ReplacementStatus.NotReplacement;

            PlannedCredit plannedCourse2 = new PlannedCredit(course, "2021/FA");
            plannedCourse2.ReplacedStatus = ReplacedStatus.NotReplaced;
            plannedCourse2.ReplacementStatus = ReplacementStatus.PossibleReplacement;


            //Acad results

            List<AcadResult> acadresults = new List<AcadResult>();
            acadresults.Add(new CreditResult(acadcredit1));
            acadresults.Add(new CreditResult(acadcredit2));
            acadresults.Add(new CreditResult(acadcredit3));
            acadresults.Add(new CreditResult(acadcredit4));
            acadresults.Add(new CreditResult(acadcredit5));
            acadresults.Add(new CourseResult(plannedCourse1));
            acadresults.Add(new CourseResult(plannedCourse2));

            GroupResult grResult = gr.Evaluate(acadresults, null, new List<Course>() { course }, null, true, false);

            Assert.AreEqual(7, grResult.Results.Count());
            Assert.AreEqual(2, grResult.GetApplied().Count());
            Assert.AreEqual(0, grResult.GetPlannedApplied().Count());
            Assert.AreEqual(4, grResult.GetRelated().Count());
            Assert.AreEqual("acadcred-2", grResult.GetApplied().ToList()[0].GetAcadCredId());
            Assert.AreEqual("acadcred-3", grResult.GetApplied().ToList()[1].GetAcadCredId());

            Assert.AreEqual("acadcred-1", grResult.GetRelated().ToList()[0].GetAcadCredId());
            Assert.AreEqual("acadcred-4", grResult.GetRelated().ToList()[1].GetAcadCredId());
            Assert.AreEqual("2020/FA",( grResult.GetRelated().ToList()[2] as CourseResult).PlannedCourse.TermCode);
            Assert.AreEqual("2021/FA", (grResult.GetRelated().ToList()[3] as CourseResult).PlannedCourse.TermCode);
            Assert.IsTrue(grResult.Explanations.Contains(GroupExplanation.Satisfied));
        }
    }

    [TestClass]
    public class Evaluate_Verify_IfOverrideApplied_With_Courses
    {
        [TestMethod]
        public void NonOverrideApplied_Equals_ListOfCourses_Satisfied()
        {
            Requirement requirement = new Requirement("1", "R1", "Req1", "UG", null);
            Subrequirement sreq = new Subrequirement("sreq-1", "subReq1");
            sreq.Requirement = requirement;

            //Take ENGL-100 ENGL-102
            Group gr = new Group("group-1", "group1", sreq);
            gr.Courses.Add("ENGL-101");
            gr.Courses.Add("ENGL-102");
            gr.GroupType = GroupType.TakeAll;
            gr.SubRequirement = sreq;

            //other acad credits
            AcademicCredit acadcredit1 = new AcademicCredit("acadcred-1",new Course("ENGL-101","SHORT TITLE","LONG TILTE", new List<OfferingDepartment>() { new OfferingDepartment("eng")}, 
                "ENGL","101","UG",new List<string>() {"100" },2,0, new List<CourseApproval>() ),"sec1");
            acadcredit1.AcademicLevelCode = "UG";
            acadcredit1.AddDepartment("ENGL");
            acadcredit1.CompletedCredit = 3m;
            acadcredit1.CourseName = "ENGL-101";
            acadcredit1.CourseLevelCode = "101";
            acadcredit1.SubjectCode = "ENGL";
            acadcredit1.SectionId = "sec1";

            AcademicCredit acadcredit2 = new AcademicCredit("acadcred-2", new Course("ENGL-102", "SHORT TITLE", "LONG TILTE", new List<OfferingDepartment>() { new OfferingDepartment("eng") },
                "ENGL", "102", "UG", new List<string>() { "100" }, 2, 0, new List<CourseApproval>()), "sec2");
            acadcredit2.AddDepartment("ENGL");
            acadcredit2.CompletedCredit = 5m;
            acadcredit2.CourseName = "ENGL-102";
            acadcredit2.CourseLevelCode = "102";
            acadcredit2.SubjectCode = "ENGL";
            acadcredit2.SectionId = "sec2";

            List<AcadResult> acadresults = new List<AcadResult>();
            acadresults.Add(new CreditResult(acadcredit1));
            acadresults.Add(new CreditResult(acadcredit2));

            List<Override> overrides = new List<Override>();
            overrides.Add(new Override("group-1", new List<string>() { "acadcred-2" }, new List<string>()));
            GroupResult grResult = gr.Evaluate(acadresults, overrides, null);

            Assert.AreEqual(2, grResult.Results.Count());
            Assert.AreEqual(2, grResult.GetApplied().Count());
            Assert.AreEqual("acadcred-1", grResult.GetApplied().ToList()[0].GetAcadCredId());
            Assert.AreEqual("acadcred-2", grResult.GetApplied().ToList()[1].GetAcadCredId());
            Assert.IsTrue(grResult.Explanations.Contains(GroupExplanation.Satisfied));
        }
       [TestMethod]
        public void NonOverrideApplied_NotEquals_ListOfCourses_NotSatisfied()
        {
            Requirement requirement = new Requirement("1", "R1", "Req1", "UG", null);
            Subrequirement sreq = new Subrequirement("sreq-1", "subReq1");
            sreq.Requirement = requirement;

            //Take ENGL-101 ENGL-102 MATH-100
            Group gr = new Group("group-1", "group1", sreq);
            gr.Courses.Add("ENGL-101");
            gr.Courses.Add("ENGL-102");
            gr.Courses.Add("MATH-100");
            gr.GroupType = GroupType.TakeAll;
            gr.SubRequirement = sreq;

            //other acad credits
            AcademicCredit acadcredit1 = new AcademicCredit("acadcred-1", new Course("ENGL-101", "SHORT TITLE", "LONG TILTE", new List<OfferingDepartment>() { new OfferingDepartment("eng") },
                "ENGL", "101", "UG", new List<string>() { "100" }, 2, 0, new List<CourseApproval>()), "sec1");
            acadcredit1.AcademicLevelCode = "UG";
            acadcredit1.AddDepartment("ENGL");
            acadcredit1.CompletedCredit = 3m;
            acadcredit1.CourseName = "ENGL-101";
            acadcredit1.CourseLevelCode = "101";
            acadcredit1.SubjectCode = "ENGL";
            acadcredit1.SectionId = "sec1";

            AcademicCredit acadcredit2 = new AcademicCredit("acadcred-2", new Course("ENGL-102", "SHORT TITLE", "LONG TILTE", new List<OfferingDepartment>() { new OfferingDepartment("eng") },
                "ENGL", "102", "UG", new List<string>() { "100" }, 2, 0, new List<CourseApproval>()), "sec2");
            acadcredit2.AddDepartment("ENGL");
            acadcredit2.CompletedCredit = 5m;
            acadcredit2.CourseName = "ENGL-102";
            acadcredit2.CourseLevelCode = "102";
            acadcredit2.SubjectCode = "ENGL";
            acadcredit2.SectionId = "sec2";

            List<AcadResult> acadresults = new List<AcadResult>();
            acadresults.Add(new CreditResult(acadcredit1));
            acadresults.Add(new CreditResult(acadcredit2));

            List<Override> overrides = new List<Override>();
            overrides.Add(new Override("group-1", new List<string>() { "acadcred-2" }, new List<string>()));
            GroupResult grResult = gr.Evaluate(acadresults, overrides, null);

            Assert.AreEqual(2, grResult.Results.Count());
            Assert.AreEqual(2, grResult.GetApplied().Count());
            Assert.AreEqual(2, grResult.GetNonOverrideApplied().Count());
            Assert.AreEqual("acadcred-1", grResult.GetApplied().ToList()[0].GetAcadCredId());
            Assert.AreEqual("acadcred-2", grResult.GetApplied().ToList()[1].GetAcadCredId());
            Assert.IsTrue(grResult.Explanations.Contains(GroupExplanation.Courses));
        }

        [TestMethod]
        public void NonOverrideApplied_NotIn_ListOfCourses_NotSatisfied()
        {
            Requirement requirement = new Requirement("1", "R1", "Req1", "UG", null);
            Subrequirement sreq = new Subrequirement("sreq-1", "subReq1");
            sreq.Requirement = requirement;

            //Take ENGL-101  MATH-100
            Group gr = new Group("group-1", "group1", sreq);
            gr.Courses.Add("ENGL-101");
            gr.Courses.Add("MATH-100");
            gr.GroupType = GroupType.TakeAll;
            gr.SubRequirement = sreq;

            //other acad credits
            AcademicCredit acadcredit1 = new AcademicCredit("acadcred-1", new Course("ENGL-101", "SHORT TITLE", "LONG TILTE", new List<OfferingDepartment>() { new OfferingDepartment("eng") },
                "ENGL", "101", "UG", new List<string>() { "100" }, 2, 0, new List<CourseApproval>()), "sec1");
            acadcredit1.AcademicLevelCode = "UG";
            acadcredit1.AddDepartment("ENGL");
            acadcredit1.CompletedCredit = 3m;
            acadcredit1.CourseName = "ENGL-101";
            acadcredit1.CourseLevelCode = "101";
            acadcredit1.SubjectCode = "ENGL";
            acadcredit1.SectionId = "sec1";

            AcademicCredit acadcredit2 = new AcademicCredit("acadcred-2", new Course("ENGL-102", "SHORT TITLE", "LONG TILTE", new List<OfferingDepartment>() { new OfferingDepartment("eng") },
                "ENGL", "102", "UG", new List<string>() { "100" }, 2, 0, new List<CourseApproval>()), "sec2");
            acadcredit2.AddDepartment("ENGL");
            acadcredit2.CompletedCredit = 5m;
            acadcredit2.CourseName = "ENGL-102";
            acadcredit2.CourseLevelCode = "102";
            acadcredit2.SubjectCode = "ENGL";
            acadcredit2.SectionId = "sec2";

            List<AcadResult> acadresults = new List<AcadResult>();
            acadresults.Add(new CreditResult(acadcredit1));
            acadresults.Add(new CreditResult(acadcredit2));

            List<Override> overrides = new List<Override>();
            overrides.Add(new Override("group-1", new List<string>() { "acadcred-2" }, new List<string>()));
            GroupResult grResult = gr.Evaluate(acadresults, overrides, null);

            Assert.AreEqual(2, grResult.Results.Count());
            Assert.AreEqual(2, grResult.GetApplied().Count());
            Assert.AreEqual(1, grResult.GetNonOverrideApplied().Count());
            Assert.AreEqual("acadcred-1", grResult.GetApplied().ToList()[0].GetAcadCredId());
            Assert.AreEqual("acadcred-2", grResult.GetApplied().ToList()[1].GetAcadCredId());
            Assert.IsTrue(grResult.Explanations.Contains(GroupExplanation.Courses));
        }

    }
}

