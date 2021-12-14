// Copyright 2019-2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Planning.Entities;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Entities.DegreePlans;
using slf4net;
using System.Linq;
using Moq;
using Ellucian.Colleague.Coordination.Planning.Services;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Planning.Repositories;
using Ellucian.Web.Security;
using Ellucian.Web.Http.TestUtil;
using Ellucian.Colleague.Domain.Repositories;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Planning.Tests.Entities
{

    public abstract class CurrentUserSetup
    {

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
                        PersonId = "0016285",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "Student",
                        Roles = new List<string>() { },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }

    }
    [TestClass]
    public class DegreePlanPreviewTests
    {
        [TestClass]
        public class DegreePlanPreviewConstructor
        {
            private string personId;
            private int degreePlanId;
            private DegreePlan degreePlan;
            private DegreePlanPreview degreePlanPreview;
            private SampleDegreePlan sampleDegreePlan;
            private IEnumerable<Term> planningTerms;

            [TestInitialize]
            public async void Initialize()
            {
                personId = "0000693";
                degreePlanId = 1;
                degreePlan = new DegreePlan(degreePlanId, personId, 1);
                degreePlan.AddTerm("2012/FA");
                degreePlan.AddTerm("2013/SP");
                // Add a planned course on the sample degree plan.
                degreePlan.AddCourse(new PlannedCourse("110"), "2013/FA");
                sampleDegreePlan = await new TestSampleDegreePlanRepository().GetAsync("TRACK3");
                planningTerms = new TestTermRepository().Get();
                //var studentAcademicCredits = new TestAcademicCreditRepository().Get();
                // Asserts are based off this constructor statement, unless another constructor is used in the test method
                degreePlanPreview = new DegreePlanPreview(degreePlan, sampleDegreePlan, new List<AcademicCredit>(), planningTerms, string.Empty);
            }

            [TestCleanup]
            public void CleanUp()
            {

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void DegreePlanPreview_NullPlanningTerms()
            {
                degreePlanPreview = new DegreePlanPreview(degreePlan, sampleDegreePlan, new List<AcademicCredit>(), null, string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void DegreePlanPreview_NoPlanningTerms()
            {
                degreePlanPreview = new DegreePlanPreview(degreePlan, sampleDegreePlan, new List<AcademicCredit>(), new List<Term>(), string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void DegreePlanPreview_NullDegreePlan()
            {
                degreePlanPreview = new DegreePlanPreview(null, sampleDegreePlan, new List<AcademicCredit>(), planningTerms, string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void DegreePlanPreview_NullSamplePlan()
            {
                degreePlanPreview = new DegreePlanPreview(degreePlan, null, new List<AcademicCredit>(), planningTerms, string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void DegreePlanPreview_NullCredits()
            {
                degreePlanPreview = new DegreePlanPreview(degreePlan, sampleDegreePlan, null, planningTerms, string.Empty);
            }

            [TestMethod]
            public void DegreePlanPreview_NumberOfTerms()
            {
                // Verify each term on preview is also found on the merged degree plan
                foreach (var termId in degreePlanPreview.Preview.TermIds)
                {
                    Assert.IsTrue(degreePlanPreview.MergedDegreePlan.TermIds.Contains(termId));
                }
            }
        }

        [TestClass]
        public class LoadDegreePlanPreviewWithAllTerms
        {
            private string personId;
            private int degreePlanId;
            private DegreePlan degreePlan;
            private DegreePlanPreview degreePlanPreview;
            private SampleDegreePlan sampleDegreePlan;
            private SampleDegreePlan placeholderSampleDegreePlan;
            private List<Term> allTerms;
            List<AcademicCredit> studentAcademicCredits;
            private ILogger logger;
            TestTermRepository termRepo = new TestTermRepository();
            TestAcademicCreditRepository academicCreditRepo = new TestAcademicCreditRepository();
            private Mock<ILogger> _loggerMock;
            List<Term> filteredAllTerms = new List<Term>();

            [TestInitialize]
            public async void Initialize()
            {
                _loggerMock = new Mock<ILogger>();
                logger = _loggerMock.Object;
                personId = "0000693";
                degreePlanId = 1;
                degreePlan = new DegreePlan(degreePlanId, personId, 1);
                degreePlan.AddTerm("2010/FA");
                degreePlan.AddTerm("2012/FA");
                degreePlan.AddTerm("2013/SP");
                // Add a planned course on the sample degree plan.
                degreePlan.AddCourse(new PlannedCourse("84"), "2010/FA");//addign SPAN-300
                studentAcademicCredits = new List<AcademicCredit>();
                studentAcademicCredits.Add(academicCreditRepo.Span300);//this is in 2010/FA 
                sampleDegreePlan = await new TestSampleDegreePlanRepository().GetAsync("TRACK3"); //have 3 blocks with courses 
                placeholderSampleDegreePlan = await new TestSampleDegreePlanRepository().GetAsync("TRACK4");
                //{ "1","block 1","139,142"}, hist-100, hist-200
                // { "2","block 2","110,21"}, biol-100, biol-200
                // { "3","block 3","91,333,64"},math-400, math-152, biol-400
                allTerms = (await termRepo.GetAsync()).ToList<Term>();

                //lets play with only 3 terms. These terms are already default planned terms on DP 
                filteredAllTerms.Add(allTerms.Where(a => a.Code == "2010/FA").First());
                filteredAllTerms.Add(allTerms.Where(a => a.Code == "2012/FA").First());
                filteredAllTerms.Add(allTerms.Where(a => a.Code == "2013/SP").First());

                // Asserts are based off this constructor statement, unless another constructor is used in the test method
                degreePlanPreview = new DegreePlanPreview();
            }

            [TestCleanup]
            public void CleanUp()
            {

            }
            [TestMethod]
            public void DegreePlanPreview_WhenCourseBlocksAreMoreThanPreviewTerms()
            {

                //student is loading the course blocks in preview terms that are not enough
                //there are 3 blocks and 3 terms on DP and one term as future term
                filteredAllTerms.Add(new Term("2021/SP", "future term", DateTime.Today.AddMonths(-2), DateTime.Today.AddMonths(2), 2021, 1, true, true, "2021RSP", false));

                //load sample plan from 2013/SP term
                degreePlanPreview.LoadDegreePlanPreviewWithAllTerms(degreePlan, sampleDegreePlan, studentAcademicCredits, filteredAllTerms, "2013/SP", logger);
                //should load 2 course blocks to 2 terms 2013/SP and 2021/SP
                Assert.IsNotNull(degreePlanPreview.Preview);
                Assert.AreEqual(4, degreePlanPreview.Preview.PlannedCourses.Count());
                Assert.AreEqual(2, degreePlanPreview.Preview.TermIds.Count());
                Assert.AreEqual("2013/SP", degreePlanPreview.Preview.TermsCoursePlanned("139").First());
                Assert.AreEqual("2021/SP", degreePlanPreview.Preview.TermsCoursePlanned("110").First());
                Assert.IsNull(degreePlanPreview.Preview.TermsCoursePlanned("333").FirstOrDefault()); //course from 3rd block is not loaded

                Assert.IsNotNull(degreePlanPreview.MergedDegreePlan);
                Assert.AreEqual(5, degreePlanPreview.MergedDegreePlan.PlannedCourses.Count());
                Assert.AreEqual(4, degreePlanPreview.MergedDegreePlan.TermIds.Count());//3 from DP and 1 from sample plan (2021/SP)
                Assert.AreEqual("2013/SP", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("139").First());
                Assert.AreEqual("2021/SP", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("110").First());
                Assert.IsNull(degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("333").FirstOrDefault()); //course from 3rd block is not loaded
                Assert.IsNull(degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("91").FirstOrDefault());
                Assert.IsNull(degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("64").FirstOrDefault());
            }

            [TestMethod]
            public void DegreePlanPreview_WhenCourseBlocksAreLessThanPreviewTerms()
            {

                //student is loading the course blocks in preview terms that are not enough
                //there are 3 blocks and 3 terms on DP and one term as future term
                filteredAllTerms.Add(new Term("2021/SP", "future term", DateTime.Today.AddMonths(-2), DateTime.Today.AddMonths(2), 2021, 1, true, true, "2021RSP", false));

                //load sample plan from 2010/FA term
                degreePlanPreview.LoadDegreePlanPreviewWithAllTerms(degreePlan, sampleDegreePlan, studentAcademicCredits, filteredAllTerms, "2010/FA", logger);
                //should load 3 course blocks to 3 terms 2010/FA, 2012/FA, 2013/SP
                Assert.IsNotNull(degreePlanPreview.Preview);
                Assert.AreEqual(7, degreePlanPreview.Preview.PlannedCourses.Count());
                Assert.AreEqual(3, degreePlanPreview.Preview.TermIds.Count());
                Assert.AreEqual("2010/FA", degreePlanPreview.Preview.TermsCoursePlanned("139").First());
                Assert.AreEqual("2012/FA", degreePlanPreview.Preview.TermsCoursePlanned("110").First());
                Assert.AreEqual("2013/SP", degreePlanPreview.Preview.TermsCoursePlanned("333").First());
                Assert.AreEqual("2013/SP", degreePlanPreview.Preview.TermsCoursePlanned("91").First());
                Assert.AreEqual("2013/SP", degreePlanPreview.Preview.TermsCoursePlanned("64").First());

                Assert.IsNotNull(degreePlanPreview.MergedDegreePlan);
                Assert.AreEqual(8, degreePlanPreview.MergedDegreePlan.PlannedCourses.Count());
                Assert.AreEqual(3, degreePlanPreview.MergedDegreePlan.TermIds.Count());//2021/Sp is not loaded
                Assert.AreEqual("2010/FA", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("139").First());
                Assert.AreEqual("2012/FA", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("110").First());
                Assert.AreEqual("2013/SP", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("333").First());
                Assert.AreEqual("2013/SP", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("91").First());
                Assert.AreEqual("2013/SP", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("64").First());
            }

            [TestMethod]
            public void DegreePlanPreview_WhenCourseBlocksAreEqualToPreviewTerms()
            {

                //student is loading the course blocks in preview terms that are not enough
                //there are 3 blocks and 3 terms on DP and one term as future term
                filteredAllTerms.Add(new Term("2021/SP", "future term", DateTime.Today.AddMonths(-2), DateTime.Today.AddMonths(2), 2021, 1, true, true, "2021RSP", false));

                //load sample plan from 2010/FA term
                degreePlanPreview.LoadDegreePlanPreviewWithAllTerms(degreePlan, sampleDegreePlan, studentAcademicCredits, filteredAllTerms, "2012/FA", logger);
                //should load 3 course blocks to 3 terms 2012/FA, 2013/SP, 2021/SP
                Assert.IsNotNull(degreePlanPreview.Preview);
                Assert.AreEqual(7, degreePlanPreview.Preview.PlannedCourses.Count());
                Assert.AreEqual(3, degreePlanPreview.Preview.TermIds.Count());
                Assert.AreEqual("2012/FA", degreePlanPreview.Preview.TermsCoursePlanned("139").First());
                Assert.AreEqual("2013/SP", degreePlanPreview.Preview.TermsCoursePlanned("110").First());
                Assert.AreEqual("2021/SP", degreePlanPreview.Preview.TermsCoursePlanned("333").First());
                Assert.AreEqual("2021/SP", degreePlanPreview.Preview.TermsCoursePlanned("91").First());
                Assert.AreEqual("2021/SP", degreePlanPreview.Preview.TermsCoursePlanned("64").First());

                Assert.IsNotNull(degreePlanPreview.MergedDegreePlan);
                Assert.AreEqual(8, degreePlanPreview.MergedDegreePlan.PlannedCourses.Count());
                Assert.AreEqual(4, degreePlanPreview.MergedDegreePlan.TermIds.Count());//2021/Sp is  loaded
                Assert.AreEqual("2010/FA", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("84").First());
                Assert.AreEqual("2012/FA", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("139").First());
                Assert.AreEqual("2013/SP", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("110").First());
                Assert.AreEqual("2021/SP", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("333").First());
                Assert.AreEqual("2021/SP", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("91").First());
                Assert.AreEqual("2021/SP", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("64").First());
            }

            [TestMethod]
            public void DegreePlanPreview_WhenCourseIsAlreadyTakenFromTheCourseBlock()
            {
                //student is loading the course blocks in preview terms that are not enough
                //there are 3 blocks and 3 terms on DP and one term as future term
                List<Term> filteredAllTerms = new List<Term>();
                filteredAllTerms.Add(allTerms.Where(a => a.Code == "2009/SP").First());
                filteredAllTerms.Add(allTerms.Where(a => a.Code == "2010/FA").First());
                filteredAllTerms.Add(allTerms.Where(a => a.Code == "2012/FA").First());
                filteredAllTerms.Add(allTerms.Where(a => a.Code == "2013/SP").First());
                filteredAllTerms.Add(new Term("2021/SP", "future term", DateTime.Today.AddMonths(-2), DateTime.Today.AddMonths(2), 2021, 1, true, true, "2021RSP", false));
                studentAcademicCredits.Add(academicCreditRepo.Biol100);
                studentAcademicCredits.Add(academicCreditRepo.Hist100);
                degreePlan.AddTerm("2009/SP");
                degreePlan.AddCourse(new PlannedCourse("139"), "2009/SP");//addign biol-100
                degreePlan.AddCourse(new PlannedCourse("110"), "2009/SP");//addign his-100
                //load sample plan from 2013/SP term
                degreePlanPreview.LoadDegreePlanPreviewWithAllTerms(degreePlan, sampleDegreePlan, studentAcademicCredits, filteredAllTerms, "2010/FA", logger);
                //should load 3 course blocks to 3 terms 2010/FA, 2012/FA, 2013/SP but should not load hist-100 (139) from course block 1
                //should not load biol-100 (110) from course block 2 because those credits are already taken
                Assert.IsNotNull(degreePlanPreview.Preview);
                Assert.AreEqual(7, degreePlanPreview.Preview.PlannedCourses.Count());
                Assert.AreEqual(3, degreePlanPreview.Preview.TermIds.Count());
                Assert.AreEqual("2010/FA", degreePlanPreview.Preview.TermsCoursePlanned("142").First());
                Assert.AreEqual("2010/FA", degreePlanPreview.Preview.TermsCoursePlanned("139").First());
                Assert.AreEqual("2012/FA", degreePlanPreview.Preview.TermsCoursePlanned("21").First());
                Assert.AreEqual("2012/FA", degreePlanPreview.Preview.TermsCoursePlanned("110").First());
                Assert.AreEqual("2013/SP", degreePlanPreview.Preview.TermsCoursePlanned("333").First());
                Assert.AreEqual("2013/SP", degreePlanPreview.Preview.TermsCoursePlanned("91").First());

                Assert.IsNotNull(degreePlanPreview.MergedDegreePlan);
                Assert.AreEqual(8, degreePlanPreview.MergedDegreePlan.PlannedCourses.Count());
                Assert.AreEqual(4, degreePlanPreview.MergedDegreePlan.TermIds.Count());
                //courses from course block 1 and 2 not merged to new 2010/FA term because credits were already taken in 2009/SP
                Assert.AreEqual("2009/SP", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("139").Last());
                Assert.AreEqual("2009/SP", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("110").Last());
                //but other courses from course blocks are merged
                Assert.AreEqual("2010/FA", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("142").First());
                Assert.AreEqual("2012/FA", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("21").First());
                Assert.AreEqual("2013/SP", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("333").First());
                Assert.AreEqual("2013/SP", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("91").First());
                Assert.AreEqual("2013/SP", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("64").First());
            }

            [TestMethod]
            public void DegreePlanPreview_WhenCoursePlaceholderInCourseBlockAlreadyExists()
            {
                // course block for track4
                //{ "0","block 0","87", ""}, hist-100
                //{ "5","block 5","139,142", "MUSC-100,MUSC-101"},
                //{ "6","block 6","", "MUSC-200,MUSC-201,MUSC-202"},

                List<Term> filteredAllTerms = new List<Term>();
                filteredAllTerms.Add(allTerms.Where(a => a.Code == "2010/FA").First());
                filteredAllTerms.Add(allTerms.Where(a => a.Code == "2012/FA").First());
                filteredAllTerms.Add(allTerms.Where(a => a.Code == "2013/SP").First());
                filteredAllTerms.Add(new Term("2021/SP", "future term", DateTime.Today.AddMonths(-2), DateTime.Today.AddMonths(2), 2021, 1, true, true, "2021RSP", false));

                degreePlan.AddCourse(new PlannedCourse(course: null, section: null, coursePlaceholder: "MUSC-100"), "2012/FA");//adding MUSC-100

                //load sample plan from 2010/FA term
                degreePlanPreview.LoadDegreePlanPreviewWithAllTerms(degreePlan, placeholderSampleDegreePlan, studentAcademicCredits, filteredAllTerms, "2010/FA", logger);

                //should load 3 course blocks to 3 terms 2010/FA, 2012/FA, 2013/SP but should not load musc-100 2012/FA from course block 2
                //should not load musc-100 2012/FA from course block 2 because already exists for the term
                Assert.IsNotNull(degreePlanPreview.Preview);
                Assert.AreEqual(3, degreePlanPreview.Preview.TermIds.Count());
                Assert.AreEqual(8, degreePlanPreview.Preview.PlannedCourses.Count());
                Assert.AreEqual(5, degreePlanPreview.Preview.PlannedCourses.Where(c => !string.IsNullOrEmpty(c.CoursePlaceholderId)).Count());

                Assert.AreEqual("2010/FA", degreePlanPreview.Preview.TermsCoursePlanned("87").First());
                Assert.AreEqual("2012/FA", degreePlanPreview.Preview.TermsCoursePlanned("139").First());
                Assert.AreEqual("2012/FA", degreePlanPreview.Preview.TermsCoursePlanned("142").First());
                Assert.AreEqual("2012/FA", degreePlanPreview.Preview.TermsCoursePlaceholderPlanned("MUSC-100").First());
                Assert.AreEqual("2012/FA", degreePlanPreview.Preview.TermsCoursePlaceholderPlanned("MUSC-101").First());
                Assert.AreEqual("2013/SP", degreePlanPreview.Preview.TermsCoursePlaceholderPlanned("MUSC-200").First());
                Assert.AreEqual("2013/SP", degreePlanPreview.Preview.TermsCoursePlaceholderPlanned("MUSC-201").First());
                Assert.AreEqual("2013/SP", degreePlanPreview.Preview.TermsCoursePlaceholderPlanned("MUSC-202").First());

                Assert.IsNotNull(degreePlanPreview.MergedDegreePlan);
                Assert.AreEqual(9, degreePlanPreview.MergedDegreePlan.PlannedCourses.Count());
                Assert.AreEqual(3, degreePlanPreview.MergedDegreePlan.TermIds.Count());

                // MUSC-100 only exists once on term 2012/FA
                Assert.AreEqual(1, degreePlanPreview.MergedDegreePlan.TermsCoursePlaceholderPlanned("MUSC-100").Count());

                //but other courses from course blocks are merged
                Assert.AreEqual("2010/FA", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("87").First());
                Assert.AreEqual("2012/FA", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("139").First());
                Assert.AreEqual("2012/FA", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("142").First());
                Assert.AreEqual("2012/FA", degreePlanPreview.MergedDegreePlan.TermsCoursePlaceholderPlanned("MUSC-100").First());
                Assert.AreEqual("2012/FA", degreePlanPreview.MergedDegreePlan.TermsCoursePlaceholderPlanned("MUSC-101").First());
                Assert.AreEqual("2013/SP", degreePlanPreview.MergedDegreePlan.TermsCoursePlaceholderPlanned("MUSC-200").First());
                Assert.AreEqual("2013/SP", degreePlanPreview.MergedDegreePlan.TermsCoursePlaceholderPlanned("MUSC-201").First());
                Assert.AreEqual("2013/SP", degreePlanPreview.MergedDegreePlan.TermsCoursePlaceholderPlanned("MUSC-202").First());
            }

            [TestMethod]
            public void DegreePlanPreview_WhenNoTermIdIsPassed_LoadsInCurrentTerm()
            {
                List<Term> filteredAllTerms = new List<Term>();
                filteredAllTerms.Add(new Term("2021/SP", "future term", DateTime.Today.AddMonths(-2), DateTime.Today.AddMonths(2), 2021, 1, true, true, "2021RSP", false));

                //load sample plan from 2013/SP term
                degreePlanPreview.LoadDegreePlanPreviewWithAllTerms(degreePlan, sampleDegreePlan, studentAcademicCredits, filteredAllTerms, null, logger);
                //should load 1 course block to 2021/SP term because this term is future term and not yet ended
                Assert.IsNotNull(degreePlanPreview.Preview);
                Assert.AreEqual(2, degreePlanPreview.Preview.PlannedCourses.Count());
                Assert.AreEqual(1, degreePlanPreview.Preview.TermIds.Count());
                Assert.AreEqual("2021/SP", degreePlanPreview.Preview.TermsCoursePlanned("139").First());
                Assert.AreEqual("2021/SP", degreePlanPreview.Preview.TermsCoursePlanned("142").First());

                Assert.IsNotNull(degreePlanPreview.MergedDegreePlan);
                Assert.AreEqual(3, degreePlanPreview.MergedDegreePlan.PlannedCourses.Count());
                Assert.AreEqual(4, degreePlanPreview.MergedDegreePlan.TermIds.Count());//3 from DP and 1 from sample plan (2021/SP)
                Assert.AreEqual("2021/SP", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("139").First());
                Assert.AreEqual("2021/SP", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("142").First());
                Assert.IsNull(degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("110").FirstOrDefault());//course from 2nd block not merged
                Assert.IsNull(degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("42").FirstOrDefault());
                Assert.IsNull(degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("333").FirstOrDefault()); //course from 3rd block is not loaded
                Assert.IsNull(degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("91").FirstOrDefault());
                Assert.IsNull(degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("64").FirstOrDefault());
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public void DegreePlanPreview_WhenDegreePlanHaveNoCurrentActiveTerms_AndThereAreNoFutureTerms_AndNoTermIsSelectedToLoadSamplePlanFrom()
            {
                degreePlanPreview.LoadDegreePlanPreviewWithAllTerms(degreePlan, sampleDegreePlan, studentAcademicCredits, filteredAllTerms, null, logger);
            }

            [TestMethod]
            public void DegreePlanPreview_WhenDegreePlanHaveNoCurrentActiveTerms_AndThereAreNoFutureTerms_AndTermIsSelectedToLoadSamplePlanFrom()
            {

                degreePlan = new DegreePlan(degreePlanId, personId, 1);
                degreePlan.AddTerm("2010/FA");
                degreePlan.AddCourse(new PlannedCourse("84"), "2010/FA");//addign SPAN-300

                //load sample plan from 2010/FA term. It will be only one course block
                degreePlanPreview.LoadDegreePlanPreviewWithAllTerms(degreePlan, sampleDegreePlan, studentAcademicCredits, filteredAllTerms, "2010/FA", logger);
                Assert.IsNotNull(degreePlanPreview.Preview);
                Assert.AreEqual(2, degreePlanPreview.Preview.PlannedCourses.Count());
                Assert.AreEqual(1, degreePlanPreview.Preview.TermIds.Count());

                Assert.IsNotNull(degreePlanPreview.MergedDegreePlan);
                Assert.AreEqual(3, degreePlanPreview.MergedDegreePlan.PlannedCourses.Count());//courses from course block first got merged
                Assert.AreEqual(1, degreePlanPreview.MergedDegreePlan.TermIds.Count());//no term got merged
                Assert.AreEqual("2010/FA", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("84").First());
                Assert.AreEqual("2010/FA", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("142").FirstOrDefault());//course from 1st block not merged
                Assert.AreEqual("2010/FA", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("139").FirstOrDefault());//course from 1st block not merged
                Assert.IsNull(degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("110").FirstOrDefault());//course from 2nd block not merged
                Assert.IsNull(degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("42").FirstOrDefault());
                Assert.IsNull(degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("333").FirstOrDefault()); //course from 3rd block is not loaded
                Assert.IsNull(degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("91").FirstOrDefault());
                Assert.IsNull(degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("64").FirstOrDefault());
            }

            [TestMethod]
            public void DegreePlanPreview_WhenDegreePlanHaveNoTermsAtAll_AndThereAreFutureTerms()
            {
                filteredAllTerms.Add(new Term("2021/SP", "future term", DateTime.Today.AddMonths(-2), DateTime.Today.AddMonths(2), 2021, 1, true, true, "2021RSP", false));

                degreePlan = new DegreePlan(degreePlanId, personId, 1);
                //load sample plan from 2021/SP term
                degreePlanPreview.LoadDegreePlanPreviewWithAllTerms(degreePlan, sampleDegreePlan, studentAcademicCredits, filteredAllTerms, null, logger);
                //should load 1 course block to 2021/SP term because this term is future term and not yet ended
                Assert.IsNotNull(degreePlanPreview.Preview);
                Assert.AreEqual(2, degreePlanPreview.Preview.PlannedCourses.Count());
                Assert.AreEqual(1, degreePlanPreview.Preview.TermIds.Count());
                Assert.AreEqual("2021/SP", degreePlanPreview.Preview.TermsCoursePlanned("139").First());
                Assert.AreEqual("2021/SP", degreePlanPreview.Preview.TermsCoursePlanned("142").First());

                Assert.IsNotNull(degreePlanPreview.MergedDegreePlan);
                Assert.AreEqual(2, degreePlanPreview.MergedDegreePlan.PlannedCourses.Count());
                Assert.AreEqual(1, degreePlanPreview.MergedDegreePlan.TermIds.Count());//3 from DP and 1 from sample plan (2021/SP)
                Assert.AreEqual("2021/SP", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("139").First());
                Assert.AreEqual("2021/SP", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("142").First());
                Assert.IsNull(degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("110").FirstOrDefault());//course from 2nd block not merged
                Assert.IsNull(degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("42").FirstOrDefault());
                Assert.IsNull(degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("333").FirstOrDefault()); //course from 3rd block is not loaded
                Assert.IsNull(degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("91").FirstOrDefault());
                Assert.IsNull(degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("64").FirstOrDefault());
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public void DegreePlanPreview_WhenDegreePlanHaveNoTermsAtAll_AndThereAreNoFutureTerms()
            {

                degreePlan = new DegreePlan(degreePlanId, personId, 1);
                degreePlanPreview.LoadDegreePlanPreviewWithAllTerms(degreePlan, sampleDegreePlan, studentAcademicCredits, filteredAllTerms, null, logger);
            }
            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public void DegreePlanPreview_WhenDegreePlanHaveCurrentActiveTerm_AndThereAreNoFutureTerms()
            {
                //This test scenario is rare but can happen if a term was removed after student had already planned for the term
                degreePlan.AddTerm("2021/SP");
                degreePlanPreview.LoadDegreePlanPreviewWithAllTerms(degreePlan, sampleDegreePlan, studentAcademicCredits, filteredAllTerms, null, logger);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public void DegreePlanPreview_WhenDegreePlanHaveNoCurrentActiveTerm_AndThereAreNoFutureTerms_AndSelectedTermIsNotOneOfThePlannedTerm()
            {
                //This test scenario is rare but can only happen when API is called directly rather than through Self-service
                //degree plan have 2010/FA, 2012/fa, 2013/SP terms only
                degreePlanPreview.LoadDegreePlanPreviewWithAllTerms(degreePlan, sampleDegreePlan, studentAcademicCredits, filteredAllTerms, "2010/SP", logger);

            }



        }

        [TestClass]
        public class LoadDegreePlanPreviewWithAllTermsAndAppliedCreditsFromEvaluation: CurrentUserSetup
        {
          
            private string personId;
            private int degreePlanId;
            private DegreePlan degreePlan;
            private DegreePlanPreview degreePlanPreview;
            private SampleDegreePlan sampleDegreePlan;
            private SampleDegreePlan placeholderSampleDegreePlan;
            private List<Term> allTerms;
            List<AcademicCredit> studentAcademicCredits;
            List<Course> courses;
            private ILogger logger;
            TestTermRepository termRepo = new TestTermRepository();
            TestAcademicCreditRepository academicCreditTestRepo = new TestAcademicCreditRepository();
            private Mock<ILogger> _loggerMock;
            List<Term> filteredAllTerms = new List<Term>();
            ProgramEvaluation emptyProgramEvalResult;
            TestCourseRepository courseRepo = new TestCourseRepository();

            //setup for program evaluation result. We are going to setup the program for min grade and then call program eval service-> evaluation method
            //rather than populating entity with the data, we will let evaluation service returns as evaluated data

            private ProgramEvaluationService programEvaluationService;

            private IAdapterRegistry adapterRegistry;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IProgramRequirementsRepository programRequirementsRepo;
            private IStudentRepository studentRepo;
            private Mock<IPlanningStudentRepository> planningStudentRepoMock;
            private IPlanningStudentRepository planningStudentRepo;
            private Mock<IStudentProgramRepository> studentProgramRepoMock;
            private IStudentProgramRepository studentProgramRepo;
            private Mock<IAcademicCreditRepository> academicCreditRepoMock;
            private IAcademicCreditRepository academicCreditRepo;
            private IAcademicCreditRepository academicCreditRepoInstance = new TestAcademicCreditRepository();
            private Mock<IStudentDegreePlanRepository> studentDegreePlanRepoMock;
            private IStudentDegreePlanRepository studentDegreePlanRepo;
            private IRequirementRepository requirementRepo;
            private Mock<IRequirementRepository> requirementRepoMock;
            private IRequirementRepository requirementRepoInstance = new TestRequirementRepository();
            private IGradeRepository gradeRepo = new TestGradeRepository();
            private IRuleRepository ruleRepo;
            private IProgramRepository programRepo;
            private ICatalogRepository catalogRepo;
            private IPlanningConfigurationRepository planningConfigRepo;
            private IReferenceDataRepository referenceDataRepo;
            private Domain.Repositories.IRoleRepository roleRepository;
            private Mock<Domain.Repositories.IRoleRepository> roleRepoMock;
            private ICurrentUserFactory currentUserFactory;
            private Mock<IConfigurationRepository> configRepoMock;
            private IConfigurationRepository configRepo;
            private Dumper dumper;
            private IEnumerable<Grade> grades ;



            [TestInitialize]
            public async void Initialize()
            {
                _loggerMock = new Mock<ILogger>();
                logger = _loggerMock.Object;
                personId = "0000693";
                degreePlanId = 1;
                degreePlan = new DegreePlan(degreePlanId, personId, 1);
                degreePlan.AddTerm("2010/FA");
                degreePlan.AddTerm("2012/FA");
                degreePlan.AddTerm("2013/SP");
                // Add a planned course on the sample degree plan.
                degreePlan.AddCourse(new PlannedCourse("84"), "2010/FA");//adding SPAN-300
                studentAcademicCredits = new List<AcademicCredit>();
                studentAcademicCredits.Add(academicCreditTestRepo.Span300);//this is in 2010/FA 
                sampleDegreePlan = await new TestSampleDegreePlanRepository().GetAsync("TRACK3"); //have 3 blocks with courses 
                placeholderSampleDegreePlan = await new TestSampleDegreePlanRepository().GetAsync("TRACK4");
                //{ "1","block 1","139,142"}, hist-100, hist-200
                // { "2","block 2","110,21"}, biol-100, biol-200
                // { "3","block 3","91,333,64"},math-400, math-152, biol-400
                allTerms = (await termRepo.GetAsync()).ToList<Term>();

                //lets play with only 3 terms. These terms are already default planned terms on DP 
                filteredAllTerms.Add(allTerms.Where(a => a.Code == "2010/FA").First());
                filteredAllTerms.Add(allTerms.Where(a => a.Code == "2012/FA").First());
                filteredAllTerms.Add(allTerms.Where(a => a.Code == "2013/SP").First());

                //courses
                courses = (await courseRepo.GetAsync()).ToList();

                // Asserts are based off this constructor statement, unless another constructor is used in the test method
                degreePlanPreview = new DegreePlanPreview();
                emptyProgramEvalResult = new ProgramEvaluation(studentAcademicCredits);

                programRequirementsRepo = new TestProgramRequirementsRepository();
                studentRepo = new TestStudentRepository();
                planningStudentRepoMock = new Mock<IPlanningStudentRepository>();
                planningStudentRepo = planningStudentRepoMock.Object;
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
                currentUserFactory = new StudentUserFactory();

                grades = await gradeRepo.GetAsync();

            }

            [TestCleanup]
            public void CleanUp()
            {

            }
            [TestMethod]
            public void DegreePlanPreview_WhenCourseBlocksAreMoreThanPreviewTerms()
            {

                //student is loading the course blocks in preview terms that are not enough
                //there are 3 blocks and 3 terms on DP and one term as future term
                filteredAllTerms.Add(new Term("2021/SP", "future term", DateTime.Today.AddMonths(-2), DateTime.Today.AddMonths(2), 2021, 1, true, true, "2021RSP", false));

                //load sample plan from 2013/SP term
                degreePlanPreview.LoadDegreePlanPreviewWithAllTermsAndAppliedCreditsFromEvaluation(degreePlan, sampleDegreePlan, studentAcademicCredits, emptyProgramEvalResult,courses, filteredAllTerms, "2013/SP", logger);
                //should load 2 course blocks to 2 terms 2013/SP and 2021/SP
                Assert.IsNotNull(degreePlanPreview.Preview);
                Assert.AreEqual(4, degreePlanPreview.Preview.PlannedCourses.Count());
                Assert.AreEqual(2, degreePlanPreview.Preview.TermIds.Count());

                //check courses from course blocks are properly added to preview plan in appropriate terms

                //courses from block 1 to first term
                //first course
                Assert.AreEqual("2013/SP", degreePlanPreview.Preview.TermsCoursePlanned("139").First());
                //validate evaluation status- should not be applied since hist-100 or 139 course is not taken by the student (not in list of student acad creds)
                Assert.AreEqual(DegreePlanPreviewCourseEvaluationStatusType.NonApplied, degreePlanPreview.DegreePlanPreviewCoursesEvaluation.Where(s => s.CourseId == "139" && s.TermCode == "2013/SP").Select(s => s.EvaluationStatus).First());
                //second course
                Assert.AreEqual("2013/SP", degreePlanPreview.Preview.TermsCoursePlanned("142").First());
                //validate evaluation status- should not be applied since hist-200 or 142 course is not taken by the student (not in list of student acad creds)
                Assert.AreEqual(false, degreePlanPreview.DegreePlanPreviewCoursesEvaluation.Where(s => s.CourseId == "142" && s.TermCode == "2013/SP").First().IsEnrolled);
                Assert.AreEqual(DegreePlanPreviewCourseEvaluationStatusType.NonApplied, degreePlanPreview.DegreePlanPreviewCoursesEvaluation.Where(s => s.CourseId == "142" && s.TermCode == "2013/SP").Select(s => s.EvaluationStatus).First());

                //courses from block 2 to 2nd term
                //first course
                Assert.AreEqual("2021/SP", degreePlanPreview.Preview.TermsCoursePlanned("110").First());
                Assert.AreEqual(DegreePlanPreviewCourseEvaluationStatusType.NonApplied, degreePlanPreview.DegreePlanPreviewCoursesEvaluation.Where(s => s.CourseId == "110" && s.TermCode == "2021/SP").Select(s => s.EvaluationStatus).First());
                //second course
                Assert.AreEqual("2021/SP", degreePlanPreview.Preview.TermsCoursePlanned("21").First());
                Assert.AreEqual(DegreePlanPreviewCourseEvaluationStatusType.NonApplied, degreePlanPreview.DegreePlanPreviewCoursesEvaluation.Where(s => s.CourseId == "21" && s.TermCode == "2021/SP").Select(s => s.EvaluationStatus).First());

                //course from 3rd block is not loaded and should also not be in evaluation result collection
                Assert.IsNull(degreePlanPreview.Preview.TermsCoursePlanned("333").FirstOrDefault());
                Assert.IsNull(degreePlanPreview.DegreePlanPreviewCoursesEvaluation.Where(s => s.CourseId == "333").FirstOrDefault());

                //validate merged plans
                Assert.IsNotNull(degreePlanPreview.MergedDegreePlan);
                Assert.AreEqual(5, degreePlanPreview.MergedDegreePlan.PlannedCourses.Count());
                Assert.AreEqual(4, degreePlanPreview.MergedDegreePlan.TermIds.Count());//3 from DP and 1 from sample plan (2021/SP)
                Assert.AreEqual("2013/SP", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("139").First());
                Assert.AreEqual("2021/SP", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("110").First());
                Assert.IsNull(degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("333").FirstOrDefault()); //course from 3rd block is not loaded
                Assert.IsNull(degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("91").FirstOrDefault());
                Assert.IsNull(degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("64").FirstOrDefault());
            }

            [TestMethod]
            public void DegreePlanPreview_WhenCourseBlocksAreLessThanPreviewTerms()
            {

                //student is loading the course blocks in preview terms that are not enough
                //there are 3 blocks and 3 terms on DP and one term as future term
                filteredAllTerms.Add(new Term("2021/SP", "future term", DateTime.Today.AddMonths(-2), DateTime.Today.AddMonths(2), 2021, 1, true, true, "2021RSP", false));

                //load sample plan from 2010/FA term
                degreePlanPreview.LoadDegreePlanPreviewWithAllTermsAndAppliedCreditsFromEvaluation(degreePlan, sampleDegreePlan, studentAcademicCredits, emptyProgramEvalResult, courses, filteredAllTerms, "2010/FA", logger);
                //should load 3 course blocks to 3 terms 2010/FA, 2012/FA, 2013/SP
                Assert.IsNotNull(degreePlanPreview.Preview);
                Assert.AreEqual(7, degreePlanPreview.Preview.PlannedCourses.Count());
                Assert.AreEqual(3, degreePlanPreview.Preview.TermIds.Count());
                //first course block loaded in first term 2010/fa
                //first course
                Assert.AreEqual("2010/FA", degreePlanPreview.Preview.TermsCoursePlanned("139").First());
                Assert.AreEqual(DegreePlanPreviewCourseEvaluationStatusType.NonApplied, degreePlanPreview.DegreePlanPreviewCoursesEvaluation.Where(s => s.CourseId == "139" && s.TermCode == "2010/FA").Select(s => s.EvaluationStatus).First());
                //2nd course
                Assert.AreEqual("2010/FA", degreePlanPreview.Preview.TermsCoursePlanned("142").First());
                Assert.AreEqual(DegreePlanPreviewCourseEvaluationStatusType.NonApplied, degreePlanPreview.DegreePlanPreviewCoursesEvaluation.Where(s => s.CourseId == "142" && s.TermCode == "2010/FA").Select(s => s.EvaluationStatus).First());
                //2nd block loaded in 2nd term
                Assert.AreEqual("2012/FA", degreePlanPreview.Preview.TermsCoursePlanned("110").First());
                Assert.AreEqual(DegreePlanPreviewCourseEvaluationStatusType.NonApplied, degreePlanPreview.DegreePlanPreviewCoursesEvaluation.Where(s => s.CourseId == "110" && s.TermCode == "2012/FA").Select(s => s.EvaluationStatus).First());

                Assert.AreEqual("2012/FA", degreePlanPreview.Preview.TermsCoursePlanned("21").First());
                Assert.AreEqual(DegreePlanPreviewCourseEvaluationStatusType.NonApplied, degreePlanPreview.DegreePlanPreviewCoursesEvaluation.Where(s => s.CourseId == "21" && s.TermCode == "2012/FA").Select(s => s.EvaluationStatus).First());
                //3rd block loaded in 3rd term
                Assert.AreEqual("2013/SP", degreePlanPreview.Preview.TermsCoursePlanned("333").First());
                Assert.AreEqual(DegreePlanPreviewCourseEvaluationStatusType.NonApplied, degreePlanPreview.DegreePlanPreviewCoursesEvaluation.Where(s => s.CourseId == "333" && s.TermCode == "2013/SP").Select(s => s.EvaluationStatus).First());

                Assert.AreEqual("2013/SP", degreePlanPreview.Preview.TermsCoursePlanned("91").First());
                Assert.AreEqual(DegreePlanPreviewCourseEvaluationStatusType.NonApplied, degreePlanPreview.DegreePlanPreviewCoursesEvaluation.Where(s => s.CourseId == "91" && s.TermCode == "2013/SP").Select(s => s.EvaluationStatus).First());

                Assert.AreEqual("2013/SP", degreePlanPreview.Preview.TermsCoursePlanned("64").First());
                Assert.AreEqual(DegreePlanPreviewCourseEvaluationStatusType.NonApplied, degreePlanPreview.DegreePlanPreviewCoursesEvaluation.Where(s => s.CourseId == "64" && s.TermCode == "2013/SP").Select(s => s.EvaluationStatus).First());


                Assert.IsNotNull(degreePlanPreview.MergedDegreePlan);
                Assert.AreEqual(8, degreePlanPreview.MergedDegreePlan.PlannedCourses.Count());
                Assert.AreEqual(3, degreePlanPreview.MergedDegreePlan.TermIds.Count());//2021/Sp is not loaded
                Assert.AreEqual("2010/FA", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("139").First());
                Assert.AreEqual("2012/FA", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("110").First());

                Assert.AreEqual("2013/SP", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("333").First());
                Assert.AreEqual("2013/SP", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("91").First());
                Assert.AreEqual("2013/SP", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("64").First());

            }

            [TestMethod]
            public void DegreePlanPreview_WhenCourseBlocksAreEqualToPreviewTerms()
            {

                //student is loading the course blocks in preview terms that are not enough
                //there are 3 blocks and 3 terms on DP and one term as future term
                filteredAllTerms.Add(new Term("2021/SP", "future term", DateTime.Today.AddMonths(-2), DateTime.Today.AddMonths(2), 2021, 1, true, true, "2021RSP", false));

                //load sample plan from 2010/FA term
                degreePlanPreview.LoadDegreePlanPreviewWithAllTermsAndAppliedCreditsFromEvaluation(degreePlan, sampleDegreePlan, studentAcademicCredits, emptyProgramEvalResult, courses, filteredAllTerms, "2012/FA", logger);
                //should load 3 course blocks to 3 terms 2012/FA, 2013/SP, 2021/SP
                Assert.IsNotNull(degreePlanPreview.Preview);
                Assert.AreEqual(7, degreePlanPreview.Preview.PlannedCourses.Count());
                Assert.AreEqual(3, degreePlanPreview.Preview.TermIds.Count());
                Assert.AreEqual("2012/FA", degreePlanPreview.Preview.TermsCoursePlanned("139").First());
                Assert.AreEqual(DegreePlanPreviewCourseEvaluationStatusType.NonApplied, degreePlanPreview.DegreePlanPreviewCoursesEvaluation.Where(s => s.CourseId == "139" && s.TermCode == "2012/FA").Select(s => s.EvaluationStatus).First());

                Assert.AreEqual("2012/FA", degreePlanPreview.Preview.TermsCoursePlanned("142").First());
                Assert.AreEqual(DegreePlanPreviewCourseEvaluationStatusType.NonApplied, degreePlanPreview.DegreePlanPreviewCoursesEvaluation.Where(s => s.CourseId == "142" && s.TermCode == "2012/FA").Select(s => s.EvaluationStatus).First());

                Assert.AreEqual("2013/SP", degreePlanPreview.Preview.TermsCoursePlanned("110").First());
                Assert.AreEqual(DegreePlanPreviewCourseEvaluationStatusType.NonApplied, degreePlanPreview.DegreePlanPreviewCoursesEvaluation.Where(s => s.CourseId == "110" && s.TermCode == "2013/SP").Select(s => s.EvaluationStatus).First());

                Assert.AreEqual("2013/SP", degreePlanPreview.Preview.TermsCoursePlanned("21").First());
                Assert.AreEqual(DegreePlanPreviewCourseEvaluationStatusType.NonApplied, degreePlanPreview.DegreePlanPreviewCoursesEvaluation.Where(s => s.CourseId == "21" && s.TermCode == "2013/SP").Select(s => s.EvaluationStatus).First());

                Assert.AreEqual("2021/SP", degreePlanPreview.Preview.TermsCoursePlanned("333").First());
                Assert.AreEqual("2021/SP", degreePlanPreview.Preview.TermsCoursePlanned("91").First());
                Assert.AreEqual("2021/SP", degreePlanPreview.Preview.TermsCoursePlanned("64").First());

                Assert.AreEqual(DegreePlanPreviewCourseEvaluationStatusType.NonApplied, degreePlanPreview.DegreePlanPreviewCoursesEvaluation.Where(s => s.CourseId == "333" && s.TermCode == "2021/SP").Select(s => s.EvaluationStatus).First());
                Assert.AreEqual(DegreePlanPreviewCourseEvaluationStatusType.NonApplied, degreePlanPreview.DegreePlanPreviewCoursesEvaluation.Where(s => s.CourseId == "91" && s.TermCode == "2021/SP").Select(s => s.EvaluationStatus).First());
                Assert.AreEqual(DegreePlanPreviewCourseEvaluationStatusType.NonApplied, degreePlanPreview.DegreePlanPreviewCoursesEvaluation.Where(s => s.CourseId == "64" && s.TermCode == "2021/SP").Select(s => s.EvaluationStatus).First());

                Assert.IsNotNull(degreePlanPreview.MergedDegreePlan);
                Assert.AreEqual(8, degreePlanPreview.MergedDegreePlan.PlannedCourses.Count());
                Assert.AreEqual(4, degreePlanPreview.MergedDegreePlan.TermIds.Count());//2021/Sp is  loaded
                Assert.AreEqual("2010/FA", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("84").First());
                Assert.AreEqual("2012/FA", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("139").First());
                Assert.AreEqual("2013/SP", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("110").First());
                Assert.AreEqual("2021/SP", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("333").First());
                Assert.AreEqual("2021/SP", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("91").First());
                Assert.AreEqual("2021/SP", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("64").First());
            }

            [TestMethod]
            public void DegreePlanPreview_WhenCourseIsAlreadyTakenFromTheCourseBlock()
            {
                //student is loading the course blocks in preview terms that are not enough
                //there are 3 blocks and 3 terms on DP and one term as future term
                List<Term> filteredAllTerms = new List<Term>();
                filteredAllTerms.Add(allTerms.Where(a => a.Code == "2009/SP").First());
                filteredAllTerms.Add(allTerms.Where(a => a.Code == "2010/FA").First());
                filteredAllTerms.Add(allTerms.Where(a => a.Code == "2012/FA").First());
                filteredAllTerms.Add(allTerms.Where(a => a.Code == "2013/SP").First());
                filteredAllTerms.Add(new Term("2021/SP", "future term", DateTime.Today.AddMonths(-2), DateTime.Today.AddMonths(2), 2021, 1, true, true, "2021RSP", false));
                studentAcademicCredits.Add(academicCreditTestRepo.Biol100);
                studentAcademicCredits.Add(academicCreditTestRepo.Hist100);
                degreePlan.AddTerm("2009/SP");
                degreePlan.AddCourse(new PlannedCourse("139"), "2009/SP");//addign biol-100
                degreePlan.AddCourse(new PlannedCourse("110"), "2009/SP");//addign his-100
                //load sample plan from 2010/FA term
                degreePlanPreview.LoadDegreePlanPreviewWithAllTermsAndAppliedCreditsFromEvaluation(degreePlan, sampleDegreePlan, studentAcademicCredits, emptyProgramEvalResult, courses, filteredAllTerms, "2010/FA", logger);
                //should load 3 course blocks to 3 terms 2010/FA, 2012/FA, 2013/SP but should not load hist-100 (139) from course block 1
                //should not load biol-100 (110) from course block 2 because those credits are already taken
                Assert.IsNotNull(degreePlanPreview.Preview);
                Assert.AreEqual(7, degreePlanPreview.Preview.PlannedCourses.Count());
                Assert.AreEqual(3, degreePlanPreview.Preview.TermIds.Count());
                Assert.AreEqual("2010/FA", degreePlanPreview.Preview.TermsCoursePlanned("142").First());
                Assert.AreEqual(DegreePlanPreviewCourseEvaluationStatusType.NonApplied, degreePlanPreview.DegreePlanPreviewCoursesEvaluation.Where(s => s.CourseId == "142" && s.TermCode == "2010/FA").Select(s => s.EvaluationStatus).First());

                Assert.AreEqual("2010/FA", degreePlanPreview.Preview.TermsCoursePlanned("139").First());
                Assert.AreEqual(true, degreePlanPreview.DegreePlanPreviewCoursesEvaluation.Where(s => s.CourseId == "139" && s.TermCode == "2010/FA").First().IsEnrolled);
                Assert.AreEqual(DegreePlanPreviewCourseEvaluationStatusType.NonApplied, degreePlanPreview.DegreePlanPreviewCoursesEvaluation.Where(s => s.CourseId == "139" && s.TermCode == "2010/FA").Select(s => s.EvaluationStatus).First());

                Assert.AreEqual("2012/FA", degreePlanPreview.Preview.TermsCoursePlanned("21").First());
                Assert.AreEqual("2012/FA", degreePlanPreview.Preview.TermsCoursePlanned("110").First());
                Assert.AreEqual(false, degreePlanPreview.DegreePlanPreviewCoursesEvaluation.Where(s => s.CourseId == "21" && s.TermCode == "2012/FA").First().IsEnrolled);
                Assert.AreEqual(DegreePlanPreviewCourseEvaluationStatusType.NonApplied, degreePlanPreview.DegreePlanPreviewCoursesEvaluation.Where(s => s.CourseId == "21" && s.TermCode == "2012/FA").Select(s => s.EvaluationStatus).First());
               
                Assert.AreEqual(true, degreePlanPreview.DegreePlanPreviewCoursesEvaluation.Where(s => s.CourseId == "110" && s.TermCode == "2012/FA").First().IsEnrolled);
                Assert.AreEqual(DegreePlanPreviewCourseEvaluationStatusType.NonApplied, degreePlanPreview.DegreePlanPreviewCoursesEvaluation.Where(s => s.CourseId == "110" && s.TermCode == "2012/FA").Select(s => s.EvaluationStatus).First());


                Assert.AreEqual("2013/SP", degreePlanPreview.Preview.TermsCoursePlanned("333").First());
                Assert.AreEqual("2013/SP", degreePlanPreview.Preview.TermsCoursePlanned("91").First());
                Assert.AreEqual(DegreePlanPreviewCourseEvaluationStatusType.NonApplied, degreePlanPreview.DegreePlanPreviewCoursesEvaluation.Where(s => s.CourseId == "333" && s.TermCode == "2013/SP").Select(s => s.EvaluationStatus).First());
                Assert.AreEqual(DegreePlanPreviewCourseEvaluationStatusType.NonApplied, degreePlanPreview.DegreePlanPreviewCoursesEvaluation.Where(s => s.CourseId == "91" && s.TermCode == "2013/SP").Select(s => s.EvaluationStatus).First());


                Assert.IsNotNull(degreePlanPreview.MergedDegreePlan);
                Assert.AreEqual(8, degreePlanPreview.MergedDegreePlan.PlannedCourses.Count());
                Assert.AreEqual(4, degreePlanPreview.MergedDegreePlan.TermIds.Count());
                //courses from course block 1 and 2 not merged to new 2010/FA term because credits were already taken in 2009/SP
                Assert.AreEqual("2009/SP", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("139").Last());
                Assert.AreEqual("2009/SP", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("110").Last());
                //but other courses from course blocks are merged
                Assert.AreEqual("2010/FA", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("142").First());
                Assert.AreEqual("2012/FA", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("21").First());
                Assert.AreEqual("2013/SP", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("333").First());
                Assert.AreEqual("2013/SP", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("91").First());
                Assert.AreEqual("2013/SP", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("64").First());
            }

            [TestMethod]
            public void DegreePlanPreview_WhenCoursePlaceholderInCourseBlockAlreadyExists()
            {
                // course block for track4
                //{ "0","block 0","87", ""}, hist-100
                //{ "5","block 5","139,142", "MUSC-100,MUSC-101"},
                //{ "6","block 6","", "MUSC-200,MUSC-201,MUSC-202"},

                List<Term> filteredAllTerms = new List<Term>();
                filteredAllTerms.Add(allTerms.Where(a => a.Code == "2010/FA").First());
                filteredAllTerms.Add(allTerms.Where(a => a.Code == "2012/FA").First());
                filteredAllTerms.Add(allTerms.Where(a => a.Code == "2013/SP").First());
                filteredAllTerms.Add(new Term("2021/SP", "future term", DateTime.Today.AddMonths(-2), DateTime.Today.AddMonths(2), 2021, 1, true, true, "2021RSP", false));

                degreePlan.AddCourse(new PlannedCourse(course: null, section: null, coursePlaceholder: "MUSC-100"), "2012/FA");//adding MUSC-100

                //load sample plan from 2010/FA term
                degreePlanPreview.LoadDegreePlanPreviewWithAllTermsAndAppliedCreditsFromEvaluation(degreePlan, placeholderSampleDegreePlan, studentAcademicCredits, emptyProgramEvalResult, courses, filteredAllTerms, "2010/FA", logger);

                //should load 3 course blocks to 3 terms 2010/FA, 2012/FA, 2013/SP but should not load musc-100 2012/FA from course block 2
                //should not load musc-100 2012/FA from course block 2 because already exists for the term
                Assert.IsNotNull(degreePlanPreview.Preview);
                Assert.AreEqual(3, degreePlanPreview.Preview.TermIds.Count());
                Assert.AreEqual(8, degreePlanPreview.Preview.PlannedCourses.Count());
                Assert.AreEqual(5, degreePlanPreview.Preview.PlannedCourses.Where(c => !string.IsNullOrEmpty(c.CoursePlaceholderId)).Count());

                Assert.AreEqual("2010/FA", degreePlanPreview.Preview.TermsCoursePlanned("87").First());
                Assert.AreEqual("2012/FA", degreePlanPreview.Preview.TermsCoursePlanned("139").First());
                Assert.AreEqual("2012/FA", degreePlanPreview.Preview.TermsCoursePlanned("142").First());
                Assert.AreEqual("2012/FA", degreePlanPreview.Preview.TermsCoursePlaceholderPlanned("MUSC-100").First());
                Assert.AreEqual("2012/FA", degreePlanPreview.Preview.TermsCoursePlaceholderPlanned("MUSC-101").First());
                Assert.AreEqual("2013/SP", degreePlanPreview.Preview.TermsCoursePlaceholderPlanned("MUSC-200").First());
                Assert.AreEqual("2013/SP", degreePlanPreview.Preview.TermsCoursePlaceholderPlanned("MUSC-201").First());
                Assert.AreEqual("2013/SP", degreePlanPreview.Preview.TermsCoursePlaceholderPlanned("MUSC-202").First());

                Assert.IsNotNull(degreePlanPreview.MergedDegreePlan);
                Assert.AreEqual(9, degreePlanPreview.MergedDegreePlan.PlannedCourses.Count());
                Assert.AreEqual(3, degreePlanPreview.MergedDegreePlan.TermIds.Count());

                // MUSC-100 only exists once on term 2012/FA
                Assert.AreEqual(1, degreePlanPreview.MergedDegreePlan.TermsCoursePlaceholderPlanned("MUSC-100").Count());

                //but other courses from course blocks are merged
                Assert.AreEqual("2010/FA", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("87").First());
                Assert.AreEqual("2012/FA", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("139").First());
                Assert.AreEqual("2012/FA", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("142").First());
                Assert.AreEqual("2012/FA", degreePlanPreview.MergedDegreePlan.TermsCoursePlaceholderPlanned("MUSC-100").First());
                Assert.AreEqual("2012/FA", degreePlanPreview.MergedDegreePlan.TermsCoursePlaceholderPlanned("MUSC-101").First());
                Assert.AreEqual("2013/SP", degreePlanPreview.MergedDegreePlan.TermsCoursePlaceholderPlanned("MUSC-200").First());
                Assert.AreEqual("2013/SP", degreePlanPreview.MergedDegreePlan.TermsCoursePlaceholderPlanned("MUSC-201").First());
                Assert.AreEqual("2013/SP", degreePlanPreview.MergedDegreePlan.TermsCoursePlaceholderPlanned("MUSC-202").First());
            }

            [TestMethod]
            public void DegreePlanPreview_WhenNoTermIdIsPassed_LoadsInCurrentTerm()
            {
                List<Term> filteredAllTerms = new List<Term>();
                filteredAllTerms.Add(new Term("2021/SP", "future term", DateTime.Today.AddMonths(-2), DateTime.Today.AddMonths(2), 2021, 1, true, true, "2021RSP", false));

                //load sample plan from 2013/SP term
                degreePlanPreview.LoadDegreePlanPreviewWithAllTermsAndAppliedCreditsFromEvaluation(degreePlan, sampleDegreePlan, studentAcademicCredits, emptyProgramEvalResult, courses, filteredAllTerms, null, logger);
                //should load 1 course block to 2021/SP term because this term is future term and not yet ended
                Assert.IsNotNull(degreePlanPreview.Preview);
                Assert.AreEqual(2, degreePlanPreview.Preview.PlannedCourses.Count());
                Assert.AreEqual(1, degreePlanPreview.Preview.TermIds.Count());
                Assert.AreEqual("2021/SP", degreePlanPreview.Preview.TermsCoursePlanned("139").First());
                Assert.AreEqual("2021/SP", degreePlanPreview.Preview.TermsCoursePlanned("142").First());
                Assert.AreEqual(DegreePlanPreviewCourseEvaluationStatusType.NonApplied, degreePlanPreview.DegreePlanPreviewCoursesEvaluation.Where(s => s.CourseId == "139" && s.TermCode == "2021/SP").Select(s => s.EvaluationStatus).First());
                Assert.AreEqual(DegreePlanPreviewCourseEvaluationStatusType.NonApplied, degreePlanPreview.DegreePlanPreviewCoursesEvaluation.Where(s => s.CourseId == "142" && s.TermCode == "2021/SP").Select(s => s.EvaluationStatus).First());

                Assert.IsNotNull(degreePlanPreview.MergedDegreePlan);
                Assert.AreEqual(3, degreePlanPreview.MergedDegreePlan.PlannedCourses.Count());
                Assert.AreEqual(4, degreePlanPreview.MergedDegreePlan.TermIds.Count());//3 from DP and 1 from sample plan (2021/SP)
                Assert.AreEqual("2021/SP", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("139").First());
                Assert.AreEqual("2021/SP", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("142").First());
                Assert.IsNull(degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("110").FirstOrDefault());//course from 2nd block not merged
                Assert.IsNull(degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("42").FirstOrDefault());
                Assert.IsNull(degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("333").FirstOrDefault()); //course from 3rd block is not loaded
                Assert.IsNull(degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("91").FirstOrDefault());
                Assert.IsNull(degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("64").FirstOrDefault());
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public void DegreePlanPreview_WhenDegreePlanHaveNoCurrentActiveTerms_AndThereAreNoFutureTerms_AndNoTermIsSelectedToLoadSamplePlanFrom()
            {
                degreePlanPreview.LoadDegreePlanPreviewWithAllTermsAndAppliedCreditsFromEvaluation(degreePlan, sampleDegreePlan, studentAcademicCredits, emptyProgramEvalResult, courses, filteredAllTerms, null, logger);
            }

            [TestMethod]
            public void DegreePlanPreview_WhenDegreePlanHaveNoCurrentActiveTerms_AndThereAreNoFutureTerms_AndTermIsSelectedToLoadSamplePlanFrom()
            {

                degreePlan = new DegreePlan(degreePlanId, personId, 1);
                degreePlan.AddTerm("2010/FA");
                degreePlan.AddCourse(new PlannedCourse("84"), "2010/FA");//addign SPAN-300

                //load sample plan from 2010/FA term. It will be only one course block
                degreePlanPreview.LoadDegreePlanPreviewWithAllTermsAndAppliedCreditsFromEvaluation(degreePlan, sampleDegreePlan, studentAcademicCredits, emptyProgramEvalResult, courses, filteredAllTerms, "2010/FA", logger);
                Assert.IsNotNull(degreePlanPreview.Preview);
                Assert.AreEqual(2, degreePlanPreview.Preview.PlannedCourses.Count());
                Assert.AreEqual(1, degreePlanPreview.Preview.TermIds.Count());

                Assert.IsNotNull(degreePlanPreview.MergedDegreePlan);
                Assert.AreEqual(3, degreePlanPreview.MergedDegreePlan.PlannedCourses.Count());//courses from course block first got merged
                Assert.AreEqual(1, degreePlanPreview.MergedDegreePlan.TermIds.Count());//no term got merged
                Assert.AreEqual("2010/FA", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("84").First());
                Assert.AreEqual("2010/FA", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("142").FirstOrDefault());//course from 1st block not merged
                Assert.AreEqual("2010/FA", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("139").FirstOrDefault());//course from 1st block not merged
                Assert.IsNull(degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("110").FirstOrDefault());//course from 2nd block not merged
                Assert.IsNull(degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("42").FirstOrDefault());
                Assert.IsNull(degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("333").FirstOrDefault()); //course from 3rd block is not loaded
                Assert.IsNull(degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("91").FirstOrDefault());
                Assert.IsNull(degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("64").FirstOrDefault());
            }

            [TestMethod]
            public void DegreePlanPreview_WhenDegreePlanHaveNoTermsAtAll_AndThereAreFutureTerms()
            {
                filteredAllTerms.Add(new Term("2021/SP", "future term", DateTime.Today.AddMonths(-2), DateTime.Today.AddMonths(2), 2021, 1, true, true, "2021RSP", false));

                degreePlan = new DegreePlan(degreePlanId, personId, 1);
                //load sample plan from 2021/SP term
                degreePlanPreview.LoadDegreePlanPreviewWithAllTermsAndAppliedCreditsFromEvaluation(degreePlan, sampleDegreePlan, studentAcademicCredits, emptyProgramEvalResult, courses, filteredAllTerms, null, logger);
                //should load 1 course block to 2021/SP term because this term is future term and not yet ended
                Assert.IsNotNull(degreePlanPreview.Preview);
                Assert.AreEqual(2, degreePlanPreview.Preview.PlannedCourses.Count());
                Assert.AreEqual(1, degreePlanPreview.Preview.TermIds.Count());
                Assert.AreEqual("2021/SP", degreePlanPreview.Preview.TermsCoursePlanned("139").First());
                Assert.AreEqual("2021/SP", degreePlanPreview.Preview.TermsCoursePlanned("142").First());

                Assert.IsNotNull(degreePlanPreview.MergedDegreePlan);
                Assert.AreEqual(2, degreePlanPreview.MergedDegreePlan.PlannedCourses.Count());
                Assert.AreEqual(1, degreePlanPreview.MergedDegreePlan.TermIds.Count());//3 from DP and 1 from sample plan (2021/SP)
                Assert.AreEqual("2021/SP", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("139").First());
                Assert.AreEqual("2021/SP", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("142").First());
                Assert.IsNull(degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("110").FirstOrDefault());//course from 2nd block not merged
                Assert.IsNull(degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("42").FirstOrDefault());
                Assert.IsNull(degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("333").FirstOrDefault()); //course from 3rd block is not loaded
                Assert.IsNull(degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("91").FirstOrDefault());
                Assert.IsNull(degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("64").FirstOrDefault());
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public void DegreePlanPreview_WhenDegreePlanHaveNoTermsAtAll_AndThereAreNoFutureTerms()
            {

                degreePlan = new DegreePlan(degreePlanId, personId, 1);
                degreePlanPreview.LoadDegreePlanPreviewWithAllTermsAndAppliedCreditsFromEvaluation(degreePlan, sampleDegreePlan, studentAcademicCredits, emptyProgramEvalResult, courses, filteredAllTerms, null, logger);
            }
            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public void DegreePlanPreview_WhenDegreePlanHaveCurrentActiveTerm_AndThereAreNoFutureTerms()
            {
                //This test scenario is rare but can happen if a term was removed after student had already planned for the term
                degreePlan.AddTerm("2021/SP");
                degreePlanPreview.LoadDegreePlanPreviewWithAllTermsAndAppliedCreditsFromEvaluation(degreePlan, sampleDegreePlan, studentAcademicCredits, emptyProgramEvalResult, courses, filteredAllTerms, null, logger);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public void DegreePlanPreview_WhenDegreePlanHaveNoCurrentActiveTerm_AndThereAreNoFutureTerms_AndSelectedTermIsNotOneOfThePlannedTerm()
            {
                //This test scenario is rare but can only happen when API is called directly rather than through Self-service
                //degree plan have 2010/FA, 2012/fa, 2013/SP terms only
                degreePlanPreview.LoadDegreePlanPreviewWithAllTermsAndAppliedCreditsFromEvaluation(degreePlan, sampleDegreePlan, studentAcademicCredits, emptyProgramEvalResult, courses, filteredAllTerms, "2010/SP", logger);

            }

            [TestMethod]
          public async Task DegreePlanPreview_CourseFailsMinGrade_IsMergedAgain_AndDisplayedAsEnrolledButWithStatusAsMinGrade()
            {
                //Description-  load sample plan such as course fails min grade at group level but is applied to another group(will be related in group with min grade; will not endup in other courses)

                //STUDENT took 3 courses - BIOL-110 with D grade, HIST-100 , SPAN-300 with A grade
                //student have following in degreeplan
                //DP terms- 2009/sp,  2010/FA,  2012/FA,  2013/SP
                //2009/sp term hav BIOL-110, HIST-100

                //EVALUATING A PROGRAM EVALUATION- SUCH AS GROUP HAVE MINGRADE OF C-  so BIOL-110 have MinGrade ,  HIST-100 is not applied , SPAN-300 is applied

                string studentid = "0016285";
                var student = studentRepo.Get(studentid);
                var planningStudent = new Domain.Student.Entities.PlanningStudent(student.Id, student.LastName, student.DegreePlanId, student.ProgramIds);
                planningStudent.Advisements = student.Advisements;
                planningStudentRepoMock.Setup(psr => psr.GetAsync(studentid, It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));

                //student is loading the course blocks in preview terms that are not enough
                //there are 3 blocks and 3 terms on DP and one term as future term
                List<Term> filteredAllTerms = new List<Term>();
                filteredAllTerms.Add(allTerms.Where(a => a.Code == "2009/SP").First());
                filteredAllTerms.Add(allTerms.Where(a => a.Code == "2010/FA").First());
                filteredAllTerms.Add(allTerms.Where(a => a.Code == "2012/FA").First());
                filteredAllTerms.Add(allTerms.Where(a => a.Code == "2013/SP").First());
                filteredAllTerms.Add(new Term("2021/SP", "future term", DateTime.Today.AddMonths(-2), DateTime.Today.AddMonths(2), 2021, 1, true, true, "2021RSP", false));
                           
                degreePlan.AddTerm("2009/SP");
                degreePlan.AddCourse(new PlannedCourse("139"), "2009/SP");//addign biol-100
                degreePlan.AddCourse(new PlannedCourse("110"), "2009/SP");//addign his-100

                studentAcademicCredits = new List<AcademicCredit>();
                academicCreditTestRepo.Span300.VerifiedGrade = (await gradeRepo.GetAsync()).Where(g => g.Id == "A").First();
                studentAcademicCredits.Add(academicCreditTestRepo.Span300);//this is in 2010/FA 
                academicCreditTestRepo.Biol100.VerifiedGrade = (await gradeRepo.GetAsync()).Where(g => g.Id == "D").First();
                studentAcademicCredits.Add(academicCreditTestRepo.Biol100);
                studentAcademicCredits.Add(academicCreditTestRepo.Hist100);
                Dictionary<string, List<AcademicCredit>> creditsDict = new Dictionary<string, List<AcademicCredit>>();
                creditsDict.Add(studentid, studentAcademicCredits);

                academicCreditRepoMock.Setup(repo => repo.GetAcademicCreditByStudentIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns((IEnumerable<string> s, bool b1, bool b2, bool b3) => Task.FromResult(creditsDict));
                programEvaluationService = new ProgramEvaluationService(
                    adapterRegistry, studentDegreePlanRepo, programRequirementsRepo, studentRepo, planningStudentRepo, studentProgramRepo,
                    requirementRepo, academicCreditRepo, null, courseRepo, termRepo, ruleRepo, programRepo, catalogRepo, planningConfigRepo,
                    referenceDataRepo, currentUserFactory, roleRepository, logger, configRepo);

                var programResult = (await programEvaluationService.EvaluateAsync(studentid, new List<string>() { "DEGREE.PLAN.REVIEW.MIN.GRADE" }, null)).First();

                
                //load sample plan from 2010/FA term
                degreePlanPreview.LoadDegreePlanPreviewWithAllTermsAndAppliedCreditsFromEvaluation(degreePlan, sampleDegreePlan, studentAcademicCredits, programResult, courses, filteredAllTerms, "2010/FA", logger);
                //should load 3 course blocks to 3 terms 2010/FA, 2012/FA, 2013/SP but should not load hist-100 (139) from course block 1
                //should  load biol-100 (110) from course block 2 because those credits are already taken but failed MinGrade
                Assert.IsNotNull(degreePlanPreview.Preview);
                Assert.AreEqual(7, degreePlanPreview.Preview.PlannedCourses.Count());
                Assert.AreEqual(3, degreePlanPreview.Preview.TermIds.Count());
                Assert.AreEqual("2010/FA", degreePlanPreview.Preview.TermsCoursePlanned("142").First()); //hist-200
                Assert.AreEqual(false, degreePlanPreview.DegreePlanPreviewCoursesEvaluation.Where(s => s.CourseId == "142" && s.TermCode == "2010/FA").First().IsEnrolled);
                Assert.AreEqual(DegreePlanPreviewCourseEvaluationStatusType.NonApplied, degreePlanPreview.DegreePlanPreviewCoursesEvaluation.Where(s => s.CourseId == "142" && s.TermCode == "2010/FA").Select(s => s.EvaluationStatus).First());

                Assert.AreEqual("2010/FA", degreePlanPreview.Preview.TermsCoursePlanned("139").First()); //hist-100
                Assert.AreEqual(true, degreePlanPreview.DegreePlanPreviewCoursesEvaluation.Where(s => s.CourseId == "139" && s.TermCode == "2010/FA").First().IsEnrolled);
                Assert.AreEqual(DegreePlanPreviewCourseEvaluationStatusType.NonApplied, degreePlanPreview.DegreePlanPreviewCoursesEvaluation.Where(s => s.CourseId == "139" && s.TermCode == "2010/FA").Select(s => s.EvaluationStatus).First());

                Assert.AreEqual("2012/FA", degreePlanPreview.Preview.TermsCoursePlanned("21").First());//biol-200
                Assert.AreEqual("2012/FA", degreePlanPreview.Preview.TermsCoursePlanned("110").First());//biol-100
                Assert.AreEqual(false, degreePlanPreview.DegreePlanPreviewCoursesEvaluation.Where(s => s.CourseId == "21" && s.TermCode == "2012/FA").First().IsEnrolled);
                Assert.AreEqual(DegreePlanPreviewCourseEvaluationStatusType.NonApplied, degreePlanPreview.DegreePlanPreviewCoursesEvaluation.Where(s => s.CourseId == "21" && s.TermCode == "2012/FA").Select(s => s.EvaluationStatus).First());

                Assert.AreEqual(true, degreePlanPreview.DegreePlanPreviewCoursesEvaluation.Where(s => s.CourseId == "110" && s.TermCode == "2012/FA").First().IsEnrolled);
                Assert.AreEqual(DegreePlanPreviewCourseEvaluationStatusType.MinGrade, degreePlanPreview.DegreePlanPreviewCoursesEvaluation.Where(s => s.CourseId == "110" && s.TermCode == "2012/FA").Select(s => s.EvaluationStatus).First());


                Assert.AreEqual("2013/SP", degreePlanPreview.Preview.TermsCoursePlanned("333").First());
                Assert.AreEqual("2013/SP", degreePlanPreview.Preview.TermsCoursePlanned("91").First());
                Assert.AreEqual(false, degreePlanPreview.DegreePlanPreviewCoursesEvaluation.Where(s => s.CourseId == "333" && s.TermCode == "2013/SP").First().IsEnrolled);
                Assert.AreEqual(false, degreePlanPreview.DegreePlanPreviewCoursesEvaluation.Where(s => s.CourseId == "91" && s.TermCode == "2013/SP").First().IsEnrolled);
                Assert.AreEqual(DegreePlanPreviewCourseEvaluationStatusType.NonApplied, degreePlanPreview.DegreePlanPreviewCoursesEvaluation.Where(s => s.CourseId == "333" && s.TermCode == "2013/SP").Select(s => s.EvaluationStatus).First());
                Assert.AreEqual(DegreePlanPreviewCourseEvaluationStatusType.NonApplied, degreePlanPreview.DegreePlanPreviewCoursesEvaluation.Where(s => s.CourseId == "91" && s.TermCode == "2013/SP").Select(s => s.EvaluationStatus).First());


                Assert.IsNotNull(degreePlanPreview.MergedDegreePlan);
                Assert.AreEqual(8, degreePlanPreview.MergedDegreePlan.PlannedCourses.Count());
                Assert.AreEqual(4, degreePlanPreview.MergedDegreePlan.TermIds.Count());
                //courses from course block 1 and 2 not merged to new 2010/FA term because credits were already taken in 2009/SP
                Assert.AreEqual("2009/SP", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("139").Last());
                Assert.AreEqual("2009/SP", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("110").Last());
                //but other courses from course blocks are merged
                Assert.AreEqual("2010/FA", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("142").First());
                Assert.AreEqual("2012/FA", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("21").First());
                Assert.AreEqual("2012/FA", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("110").First()); //biol-100 should be added because it failed min grade
                Assert.AreEqual("2013/SP", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("333").First());
                Assert.AreEqual("2013/SP", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("91").First());
                Assert.AreEqual("2013/SP", degreePlanPreview.MergedDegreePlan.TermsCoursePlanned("64").First());

            }
            //load sample plan such as course is simply applied to groups//not in other courses
            //load sample plan such as course is not applied to any group but not due to mingrade failure. will endup in other courses
            //load sample plan such as course fails one group because of wrong subject, another because of wrong course (basically is not related at all) and  is also not applied to another group because of min grade failure (basically is related in that group) will endup in other courses

            private List<AcademicCredit> GetCredits(string studentId, bool includeDrops = false)
            {
                var creditsList = new List<AcademicCredit>();
               
                    var student = studentRepo.Get(studentId);
                    var acadCreditIds = student.AcademicCreditIds;

                     creditsList = academicCreditRepoInstance.GetAsync(acadCreditIds, includeDrops: includeDrops).Result.ToList();

                return creditsList;
            }



        }



    }
}
