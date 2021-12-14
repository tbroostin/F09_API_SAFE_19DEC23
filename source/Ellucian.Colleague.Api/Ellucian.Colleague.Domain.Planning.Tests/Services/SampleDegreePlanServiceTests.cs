// Copyright 2019-2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Planning.Services;
using Ellucian.Colleague.Domain.Planning.Entities;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Entities.DegreePlans;
using Ellucian.Colleague.Domain.Student.Tests;

namespace Ellucian.Colleague.Domain.Planning.Tests.Services
{
    [TestClass]
    public class SampleDegreePlanServiceTests
    {
        [TestClass]
        public class ApplySample
        {
            string personId;
            DegreePlan degreePlan;
            SampleDegreePlan curriculumTrack;
            CourseBlocks block1;
            CourseBlocks block2;
            string term1;
            string term2;
            IEnumerable<Term> terms;
            IEnumerable<AcademicCredit> emptyAcademicCredits;
            IEnumerable<AcademicCredit> academicCredits;

            [TestInitialize]
            public async void Initialize()
            {
                personId = "123";
                degreePlan = new DegreePlan(personId);

                term1 = "2012/FA";
                term2 = "2013/SP";

                block1 = new CourseBlocks("block1", new List<string>() { "1", "2" }, new List<string>());
                block2 = new CourseBlocks("block2", new List<string>() { "3", "4" }, new List<string>());
                curriculumTrack = new SampleDegreePlan("CT1", "track1", new List<CourseBlocks>() { block1, block2 });
                terms = new TestTermRepository().Get();
                emptyAcademicCredits = new List<AcademicCredit>();
                academicCredits = await new TestAcademicCreditRepository().GetAsync();
            }

            [TestCleanup]
            public void Cleanup()
            {
                degreePlan = null;
                curriculumTrack = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void ExceptionThrownForNullTermList()
            {
                terms = null;
                SampleDegreePlanService.ApplySample(ref degreePlan, curriculumTrack, terms, emptyAcademicCredits, null, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void ExceptionThrownForEmptyTermList()
            {
                terms = new List<Term>();
                SampleDegreePlanService.ApplySample(ref degreePlan, curriculumTrack, terms, emptyAcademicCredits, null, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void ExceptionThrownIfDefaultPlanningTermsNotSetUp()
            {
                terms = terms.Where(t => t.DefaultOnPlan == false);
                SampleDegreePlanService.ApplySample(ref degreePlan, curriculumTrack, terms, emptyAcademicCredits, null, null);
            }

            [TestMethod]
            public void ApplyToDegreePlanWithTerms_NoCredits()
            {
                // Arrange--Add two terms to degree plan
                degreePlan.AddTerm(term1);
                degreePlan.AddTerm(term2);

                // Act--Apply curriculum track to degree plan
                SampleDegreePlanService.ApplySample(ref degreePlan, curriculumTrack, terms, emptyAcademicCredits, null, null);

                // Assert--Courses in curriculum track ar now on the degree plan
                var courses = degreePlan.GetPlannedCourses("2012/FA");
                Assert.AreEqual(0, courses.Count());
                courses = degreePlan.GetPlannedCourses("2013/SP");
                Assert.AreEqual(0, courses.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void ReturnsExceptionWhenDegreePlanHasNoTerms()
            {
                // Act--Apply curriculum track to degree plan
                SampleDegreePlanService.ApplySample(ref degreePlan, curriculumTrack, null, emptyAcademicCredits, null, null);
            }

            [TestMethod]
            public void AppliesPartialTrackOnlyToExistingTerms()
            {
                // Act--Apply curriculum track. Limited terms used to simulate
                // the situation where there are no more default plan terms configured.
                var limitedTerms = new List<Term>();
                limitedTerms.Add(new Term("TESTTRM", "test term", DateTime.Today.AddDays(30), DateTime.Today.AddDays(120), 2020, 1, true, true, "TESTTRM", true));
                SampleDegreePlanService.ApplySample(ref degreePlan, curriculumTrack, limitedTerms, emptyAcademicCredits, null, null);

                // Assert--The one available term added to the plan, course block applied
                Assert.AreEqual(1, degreePlan.TermIds.Count());
                var courses = degreePlan.GetPlannedCourses("TESTTRM");
                Assert.AreEqual(2, courses.Count());
                Assert.AreEqual(block1.CourseIds.ElementAt(0), courses.ElementAt(0).CourseId);
                Assert.AreEqual(block1.CourseIds.ElementAt(1), courses.ElementAt(1).CourseId);
            }

            [TestMethod]
            public void AddsAllPlanTermsNeededForAllBlocks()
            {
                // Act--Apply curriculum track
                var terms = new List<Term>();
                var testTerm1 = new Term("TERM1", "test term 1", DateTime.Today.AddDays(30), DateTime.Today.AddDays(120), 2020, 1, true, true, "TERM1", true);
                terms.Add(testTerm1);
                var testTerm2 = new Term("TERM2", "test term 2", DateTime.Today.AddDays(60), DateTime.Today.AddDays(160), 2020, 2, true, true, "TERM2", true);
                terms.Add(testTerm2);
                SampleDegreePlanService.ApplySample(ref degreePlan, curriculumTrack, terms, emptyAcademicCredits, null, null);

                // Assert--needed terms added to the plan, no more, no less
                Assert.AreEqual(2, degreePlan.TermIds.Count());

                // Assert--Courses in curriculum track are now on the degree plan
                var courses = degreePlan.GetPlannedCourses(testTerm1.Code);
                Assert.AreEqual(2, courses.Count());
                Assert.AreEqual(block1.CourseIds.ElementAt(0), courses.ElementAt(0).CourseId);
                Assert.AreEqual(block1.CourseIds.ElementAt(1), courses.ElementAt(1).CourseId);
                courses = degreePlan.GetPlannedCourses(testTerm2.Code);
                Assert.AreEqual(2, courses.Count());
                Assert.AreEqual(block2.CourseIds.ElementAt(0), courses.ElementAt(0).CourseId);
                Assert.AreEqual(block2.CourseIds.ElementAt(1), courses.ElementAt(1).CourseId);
            }

            [TestMethod]
            public void AddsPartialPlanTermsNeededForAllBlocks()
            {
                // Arrange--Add one term, another term still needed to accommodate all blocks in curr track
                var testTerms = new List<Term>();
                var testTerm1 = new Term("TERM1", "test term 1", DateTime.Today.AddDays(30), DateTime.Today.AddDays(120), 2020, 2, true, true, "TERM1", true);
                testTerms.Add(testTerm1);
                var testTerm2 = new Term("TERM2", "test term 2", DateTime.Today.AddDays(60), DateTime.Today.AddDays(160), 2020, 3, true, true, "TERM2", true);
                testTerms.Add(testTerm2);
                var testTerm3 = new Term("TERM3", "test term 3", DateTime.Today, DateTime.Today.AddDays(30), 2020, 1, true, true, "TERM3", true);
                testTerms.Add(testTerm3);
                degreePlan.AddTerm("TERM3");

                // Act--Apply curriculum track
                SampleDegreePlanService.ApplySample(ref degreePlan, curriculumTrack, testTerms, emptyAcademicCredits, null, null);

                // Assert--needed terms added to the plan
                Assert.AreEqual(2, degreePlan.TermIds.Count());
                // Assert--Courses in curriculum track are all on the degree plan
                var sortedTerms = testTerms.OrderBy(t => t.ReportingYear).ThenBy(t => t.Sequence);
                var courses = degreePlan.GetPlannedCourses(sortedTerms.ElementAt(0).Code);
                Assert.AreEqual(2, courses.Count());
                Assert.AreEqual(block1.CourseIds.ElementAt(0), courses.ElementAt(0).CourseId);
                Assert.AreEqual(block1.CourseIds.ElementAt(1), courses.ElementAt(1).CourseId);
                courses = degreePlan.GetPlannedCourses(sortedTerms.ElementAt(1).Code);
                Assert.AreEqual(2, courses.Count());
                Assert.AreEqual(block2.CourseIds.ElementAt(0), courses.ElementAt(0).CourseId);
                Assert.AreEqual(block2.CourseIds.ElementAt(1), courses.ElementAt(1).CourseId);
            }

            [TestMethod]
            public void AppliesOnlyDefaultAndCurrentTerms()
            {
                // Arrange -- set up terms
                var terms = new List<Term>();
                var testTerm1 = new Term("TERM1", "test term 1", DateTime.Today.AddDays(30), DateTime.Today.AddDays(120), 2020, 1, true, true, "TERM1", true);
                terms.Add(testTerm1); // this one is good
                var testTerm2 = new Term("TERM2", "test term 2", DateTime.Today.AddDays(60), DateTime.Today.AddDays(160), 2020, 2, false, true, "TERM2", true);
                terms.Add(testTerm2); // this one is bad--default==false
                var testTerm3 = new Term("TERM3", "test term 3", DateTime.Today.AddDays(-200), DateTime.Today.AddDays(-30), 2020, 3, true, true, "TERM3", true);
                terms.Add(testTerm3); // this one is bad--end date < today
                var testTerm4 = new Term("TERM4", "test term 4", DateTime.Today.AddDays(60), DateTime.Today.AddDays(120), 2020, 1, true, true, "TERM4", true);
                terms.Add(testTerm4); // this one is good

                // Act -- apply sample degree plan
                SampleDegreePlanService.ApplySample(ref degreePlan, curriculumTrack, terms, emptyAcademicCredits, null, null);

                // Assert-- two terms were added to the plan (two are required by the sample/track)
                Assert.AreEqual(2, degreePlan.TermIds.Count());

                // Assert that only the future, default terms were added.
                foreach (var term in degreePlan.TermIds)
                {
                    Assert.IsTrue(terms.First(t => t.Code == term).DefaultOnPlan);
                }
            }

            [TestMethod]
            public void DoesNotApplyCourseAlreadyInPlan()
            {
                // Arrange--place one of the courses from the curriculum track in each term
                degreePlan.AddCourse(new PlannedCourse("1"), term1);
                degreePlan.AddCourse(new PlannedCourse("4"), term2);

                // Act -- apply sample degree plan
                SampleDegreePlanService.ApplySample(ref degreePlan, curriculumTrack, terms, emptyAcademicCredits, null, null);

                // Assert -- only two courses found on each degree plan term
                var courses = degreePlan.GetPlannedCourses(term1).Select(p => p.CourseId);
                Assert.AreEqual(1, courses.Count());
                Assert.IsTrue(courses.Contains(block1.CourseIds.ElementAt(0)));
                Assert.IsFalse(courses.Contains(block1.CourseIds.ElementAt(1)));
                courses = degreePlan.GetPlannedCourses(term2).Select(p => p.CourseId);
                Assert.AreEqual(1, courses.Count());
                Assert.IsFalse(courses.Contains(block2.CourseIds.ElementAt(0)));
                Assert.IsTrue(courses.Contains(block2.CourseIds.ElementAt(1)));
            }

            [TestMethod]
            public async Task DoesNotApply_IfAcadCreditWithNewStatus()
            {
                // Set up degree plan, sample plan, and credits
                degreePlan.AddTerm(term1);
                degreePlan.AddTerm(term2);
                block1 = new CourseBlocks("block1", new List<string>() { "139", "2" }, new List<string>());
                block2 = new CourseBlocks("block2", new List<string>() { "3", "4" }, new List<string>());
                SampleDegreePlan curriculumTrack = new SampleDegreePlan("CT1", "track1", new List<CourseBlocks>() { block1, block2 });

                // Get specific academic credits for the student
                ICollection<string> creditlist = new List<string>() { "1", "3", "4" };
                var specificCredits = await new TestAcademicCreditRepository().GetAsync(creditlist);
                // Act -- apply sample degree plan
                SampleDegreePlanService.ApplySample(ref degreePlan, curriculumTrack, terms, specificCredits, null, null);
                Assert.AreEqual(degreePlan.PlannedCourses.Count(), 3);
                var xcourse = degreePlan.PlannedCourses.Where(c => c.CourseId == "139").FirstOrDefault();
                Assert.IsNull(xcourse);
            }

            [TestMethod]
            public async Task DoesNotApply_IfAcadCredWithAddStatus()
            {
                // Set up degree plan, sample plan, and credits
                degreePlan.AddTerm(term1);
                degreePlan.AddTerm(term2);
                block1 = new CourseBlocks("block1", new List<string>() { "110", "2" }, new List<string>());
                block2 = new CourseBlocks("block2", new List<string>() { "3", "4" }, new List<string>());
                SampleDegreePlan curriculumTrack = new SampleDegreePlan("CT1", "track1", new List<CourseBlocks>() { block1, block2 });

                // Get specific academic credits for the student
                ICollection<string> creditlist = new List<string>() { "1", "3", "4" };
                var specificCredits = await new TestAcademicCreditRepository().GetAsync(creditlist);
                // Act -- apply sample degree plan
                SampleDegreePlanService.ApplySample(ref degreePlan, curriculumTrack, terms, specificCredits, null, null);
                Assert.AreEqual(degreePlan.PlannedCourses.Count(), 3);
                var xcourse = degreePlan.PlannedCourses.Where(c => c.CourseId == "110").FirstOrDefault();
                Assert.IsNull(xcourse);
            }

            [TestMethod]
            public async Task ApplyCourse_IfAcadCredWithdrawnStatus()
            {
                // Set up degree plan, sample plan, and credits
                degreePlan.AddTerm(term1);
                degreePlan.AddTerm(term2);
                block1 = new CourseBlocks("block1", new List<string>() { "2450", "2" }, new List<string>());
                block2 = new CourseBlocks("block2", new List<string>() { "3", "4" }, new List<string>());
                SampleDegreePlan curriculumTrack = new SampleDegreePlan("CT1", "track1", new List<CourseBlocks>() { block1, block2 });

                // Get specific academic credits for the student
                ICollection<string> creditlist = new List<string>() { "56" };
                var specificCredits = await new TestAcademicCreditRepository().GetAsync(creditlist);
                // Act -- apply sample degree plan
                SampleDegreePlanService.ApplySample(ref degreePlan, curriculumTrack, terms, specificCredits, null, null);
                Assert.AreEqual(degreePlan.PlannedCourses.Count(), 4);
                var xcourse = degreePlan.PlannedCourses.Where(c => c.CourseId == "2450").FirstOrDefault();
                Assert.IsNotNull(xcourse);
            }

            [TestMethod]
            public async Task ApplyCourse_IfAcadCredDeletedStatus()
            {
                // Set up degree plan, sample plan, and credits
                degreePlan.AddTerm(term1);
                degreePlan.AddTerm(term2);
                block1 = new CourseBlocks("block1", new List<string>() { "2450", "2" }, new List<string>());
                block2 = new CourseBlocks("block2", new List<string>() { "3", "4" }, new List<string>());
                SampleDegreePlan curriculumTrack = new SampleDegreePlan("CT1", "track1", new List<CourseBlocks>() { block1, block2 });

                // Get specific academic credits for the student
                ICollection<string> creditlist = new List<string>() { "55" };
                var specificCredits = await new TestAcademicCreditRepository().GetAsync(creditlist);
                // Act -- apply sample degree plan
                SampleDegreePlanService.ApplySample(ref degreePlan, curriculumTrack, terms, specificCredits, null, null);
                Assert.AreEqual(degreePlan.PlannedCourses.Count(), 4);
            }

            [TestMethod]
            public void ApplyCourse_IfAcadCreditHasNoCourse()
            {
                // Set up degree plan, sample plan, and credits
                degreePlan.AddTerm(term1);
                degreePlan.AddTerm(term2);
                block1 = new CourseBlocks("block1", new List<string>() { "2450", "2" }, new List<string>());
                block2 = new CourseBlocks("block2", new List<string>() { "3", "4" }, new List<string>());
                SampleDegreePlan curriculumTrack = new SampleDegreePlan("CT1", "track1", new List<CourseBlocks>() { block1, block2 });

                // Get specific academic credits for the student
                List<AcademicCredit> specificCredits = new List<AcademicCredit>();
                var nonCourseAcademicCredit = new AcademicCredit("7777");
                specificCredits.Add(nonCourseAcademicCredit);
                // Act -- apply sample degree plan
                SampleDegreePlanService.ApplySample(ref degreePlan, curriculumTrack, terms, specificCredits, null, null);
                Assert.AreEqual(degreePlan.PlannedCourses.Count(), 4);
            }

            /// <summary>
            /// Ensure that planned courses that are added are not protected, even if other courses on the plan are protected.
            /// </summary>
            [TestMethod]
            public void NotStudentAndPlanProtected_AppliedCoursesAreNotProtected()
            {
                // Arrange
                // Set up an existing planned course that is protected
                degreePlan.AddTerm(term1);
                degreePlan.AddCourse(new PlannedCourse("01", null, GradingType.Graded, Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, null) { IsProtected = true }, term1);
                // Curriculum track to add courses to two terms
                block1 = new CourseBlocks("block1", new List<string>() { "2450", "2" }, new List<string>());
                block2 = new CourseBlocks("block2", new List<string>() { "3", "4" }, new List<string>());
                SampleDegreePlan curriculumTrack = new SampleDegreePlan("CT1", "track1", new List<CourseBlocks>() { block1, block2 });

                // Act -- apply sample degree plan - user is not equal to degree plan person id to imply advisor
                SampleDegreePlanService.ApplySample(ref degreePlan, curriculumTrack, terms, new List<AcademicCredit>(), null, "0000099");

                // Assert - all courses are protected
                Assert.AreEqual(5, degreePlan.PlannedCourses.Count());
                foreach (var termCode in degreePlan.TermIds)
                {
                    // term1 has original planned course, skip that one. Check that the courses added from the
                    // course blocks have the protected flag set properly.
                    if (termCode != term1)
                    {
                        var plannedCourses = degreePlan.GetPlannedCourses(termCode);
                        Assert.AreEqual(2, plannedCourses.Count());
                        foreach (var plannedCourse in plannedCourses)
                        {
                            Assert.AreEqual(null, plannedCourse.IsProtected);
                        }
                    }
                }
            }

            [TestMethod]
            public void IsStudentAndPlanProtected_AppliedCoursesAreNotProtected()
            {
                // Arrange
                // Arrange
                // Set up an existing planned course that is not protected
                degreePlan.AddTerm(term1);
                degreePlan.AddCourse(new PlannedCourse("01", null, GradingType.Graded, Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, null), term1);
                // Curriculum track to add courses to two terms
                block1 = new CourseBlocks("block1", new List<string>() { "2450", "2" }, new List<string>());
                block2 = new CourseBlocks("block2", new List<string>() { "3", "4" }, new List<string>());
                SampleDegreePlan curriculumTrack = new SampleDegreePlan("CT1", "track1", new List<CourseBlocks>() { block1, block2 });

                // Act -- apply sample degree plan - user is not equal to degree plan person id to imply advisor
                SampleDegreePlanService.ApplySample(ref degreePlan, curriculumTrack, terms, new List<AcademicCredit>(), null, degreePlan.PersonId);

                // Assert - all courses are protected
                Assert.AreEqual(5, degreePlan.PlannedCourses.Count());
                foreach (var termCode in degreePlan.TermIds)
                {
                    // term1 has original planned course, skip that one. Check that the courses added from the
                    // course blocks have the protected flag set properly.
                    if (termCode != term1)
                    {
                        var plannedCourses = degreePlan.GetPlannedCourses(termCode);
                        Assert.AreEqual(2, plannedCourses.Count());
                        foreach (var plannedCourse in plannedCourses)
                        {
                            Assert.AreEqual(null, plannedCourse.IsProtected);
                        }
                    }
                }
            }

            [TestMethod]
            public void UserNotStudentAndPlanNotProtected_AppliedCoursesAreNotProtected()
            {
                // Arrange
                // Set up an existing planned course that is protected
                degreePlan.AddTerm(term1);
                degreePlan.AddCourse(new PlannedCourse("01", null, GradingType.Graded, Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, null), term1);
                // Curriculum track to add courses to two terms
                block1 = new CourseBlocks("block1", new List<string>() { "2450", "2" }, new List<string>());
                block2 = new CourseBlocks("block2", new List<string>() { "3", "4" }, new List<string>());
                SampleDegreePlan curriculumTrack = new SampleDegreePlan("CT1", "track1", new List<CourseBlocks>() { block1, block2 });

                // Act -- apply sample degree plan - user is not equal to degree plan person id to imply advisor
                SampleDegreePlanService.ApplySample(ref degreePlan, curriculumTrack, terms, new List<AcademicCredit>(), null, "0000099");

                // Assert - all courses are protected
                Assert.AreEqual(5, degreePlan.PlannedCourses.Count());
                foreach (var termCode in degreePlan.TermIds)
                {
                    // term1 has original planned course, skip that one. Check that the courses added from the
                    // course blocks have the protected flag set properly.
                    if (termCode != term1)
                    {
                        var plannedCourses = degreePlan.GetPlannedCourses(termCode);
                        Assert.AreEqual(2, plannedCourses.Count());
                        foreach (var plannedCourse in plannedCourses)
                        {
                            Assert.AreEqual(null, plannedCourse.IsProtected);
                        }
                    }
                }
            }

            [TestMethod]
            public void UserNotStudentAndNoPreviousCoursesExistOnPlan_AppliedCoursesAreNotProtected()
            {
                // Arrange
                // Set up an existing planned course that is protected
                degreePlan.AddTerm(term1);
                // Curriculum track to add courses to two terms
                block1 = new CourseBlocks("block1", new List<string>() { "2450", "2" }, new List<string>());
                block2 = new CourseBlocks("block2", new List<string>() { "3", "4" }, new List<string>());
                SampleDegreePlan curriculumTrack = new SampleDegreePlan("CT1", "track1", new List<CourseBlocks>() { block1, block2 });

                // Act -- apply sample degree plan - user is not equal to degree plan person id to imply advisor
                SampleDegreePlanService.ApplySample(ref degreePlan, curriculumTrack, terms, new List<AcademicCredit>(), null, "0000099");

                // Assert - all courses are protected
                Assert.AreEqual(4, degreePlan.PlannedCourses.Count());
                foreach (var termCode in degreePlan.TermIds)
                {
                    // term1 has original planned course, skip that one. Check that the courses added from the
                    // course blocks have the protected flag set properly.
                    if (termCode != term1)
                    {
                        var plannedCourses = degreePlan.GetPlannedCourses(termCode);
                        Assert.AreEqual(2, plannedCourses.Count());
                        foreach (var plannedCourse in plannedCourses)
                        {
                            Assert.AreEqual(null, plannedCourse.IsProtected);
                        }
                    }
                }
            }
        }


    }
}
