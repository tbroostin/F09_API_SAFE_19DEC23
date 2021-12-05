// Copyright 2012-2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Entities.DegreePlans;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using Ellucian.Colleague.Domain.Student.Exceptions;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities.DegreePlans
{
    [TestClass]
    public class DegreePlanTests
    {
        [TestClass]
        public class DegreePlanConstructorWithId
        {
            private string personId;
            private int degreePlanId;
            private DegreePlan degreePlan;

            [TestInitialize]
            public void Initialize()
            {
                // Asserts are based off this constructor statement, unless another constructor is used in the test method
                personId = "0000693";
                degreePlanId = 1;
                degreePlan = new DegreePlan(degreePlanId, personId, 1);
                degreePlan.AddTerm("2012/FA");
                degreePlan.AddTerm("2013/SP");
                degreePlan.AddCourse(new PlannedCourse(course: "111", coursePlaceholder: null), "2012/FA");
            }

            [TestCleanup]
            public void CleanUp()
            {
            }
            [TestMethod]
            public void DegreeId()
            {
                Assert.AreEqual(degreePlan.Id, degreePlanId);
            }
            [TestMethod]
            public void DegreePersonId()
            {
                Assert.AreEqual(degreePlan.PersonId, personId);
            }

            [TestMethod]
            public void Degree_ReviewRequested()
            {
                Assert.IsFalse(degreePlan.ReviewRequested);
            }

            [TestMethod]
            public void Degree_LastReviewed()
            {
                Assert.IsNull(degreePlan.LastReviewedAdvisorId);
                Assert.IsNull(degreePlan.LastReviewedDate);
            }

            [TestMethod]
            public void Degree_ArchiveNotificationDate()
            {
                Assert.IsNull(degreePlan.ArchiveNotificationDate);
            }

            [TestMethod]
            public void DegreeTerms()
            {
                Assert.AreEqual(2, degreePlan.TermIds.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void DegreePersonId_ExceptionIfNull()
            {
                new DegreePlan(degreePlanId, null, 1);
            }

            [TestMethod]
            public void Degree_WithNoTerms()
            {
                // Using a different constructor.
                personId = "0000693";
                degreePlanId = 1;
                degreePlan = new DegreePlan(degreePlanId, personId, 1);
                Assert.AreEqual(0, degreePlan.TermIds.Count());
            }

            [TestMethod]
            public void Notes()
            {
                Assert.AreEqual(0, degreePlan.Notes.Count());
            }

            [TestMethod]
            public void RestrictedNotes()
            {
                Assert.AreEqual(0, degreePlan.RestrictedNotes.Count());
            }

            [TestMethod]
            public void DegreePlan_Properties()
            {
                DegreePlan degreePlan2 = new DegreePlan(degreePlanId, personId, 12, true);
                degreePlan2.LastReviewedAdvisorId = "1111111";
                degreePlan2.LastReviewedDate = new DateTime(2012, 9, 1);
                degreePlan2.ArchiveNotificationDate = new DateTime(2021, 8, 27);
                Assert.IsTrue(degreePlan2.ReviewRequested);
                Assert.AreEqual(new DateTime(2012, 9, 1), degreePlan2.LastReviewedDate);
                Assert.AreEqual("1111111", degreePlan2.LastReviewedAdvisorId);
                Assert.AreEqual(new DateTime(2021, 8, 27), degreePlan2.ArchiveNotificationDate);
            }
        }

        [TestClass]
        public class DegreePlanConstructorWithNoId
        {
            private string personId;
            private DegreePlan degreePlan;

            [TestInitialize]
            public void Initialize()
            {
                // Asserts in this class are based off this constructor statement, unless another constructor is used in the test method
                personId = "0000693";
                degreePlan = new DegreePlan(personId);
                degreePlan.AddTerm("2012/FA");
                degreePlan.AddTerm("2013/SP");
            }

            [TestCleanup]
            public void CleanUp()
            {
            }

            [TestMethod]
            public void DegreePersonId()
            {
                Assert.AreEqual(degreePlan.PersonId, personId);
            }

            [TestMethod]
            public void DegreeTerms()
            {
                Assert.AreEqual(2, degreePlan.TermIds.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void DegreePersonId_ExceptionIfNull()
            {
                new DegreePlan(null);
            }

            [TestMethod]
            public void Degree_WithNoTerms()
            {
                // Using a different constructor.
                personId = "0000693";
                degreePlan = new DegreePlan(personId);
                Assert.AreEqual(0, degreePlan.TermIds.Count());
            }

            [TestMethod]
            public void Notes()
            {
                Assert.AreEqual(0, degreePlan.Notes.Count());
            }

            [TestMethod]
            public void RestrictedNotes()
            {
                Assert.AreEqual(0, degreePlan.RestrictedNotes.Count());
            }
        }

        [TestClass]
        public class DegreePlan_CreateDegreePlan
        {
            private TestStudentProgramRepository testStudentProgramRepo = new TestStudentProgramRepository();
            private TestTermRepository testTermRepo = new TestTermRepository();
            private TestProgramRepository testProgramRepo = new TestProgramRepository();
            private IEnumerable<StudentProgram> studentPrograms;
            private IEnumerable<Term> availableTerms;
            private IEnumerable<Program> allPrograms;
            private Ellucian.Colleague.Domain.Student.Entities.Student student;

            [TestInitialize]
            public void Initialize()
            {
                studentPrograms = testStudentProgramRepo.Get("0000894");
                foreach (var prm in studentPrograms)
                {
                    prm.AnticipatedCompletionDate = new DateTime(2014, 6, 1);
                }
                // Really dont' need to put the student program Ids into the student entity for this purpose...
                student = new Student.Entities.Student("0000894", "Smith", null, null, null, null);
                availableTerms = testTermRepo.Get().Where(t => t.DefaultOnPlan == true);
                allPrograms = testProgramRepo.Get();
            }

            [TestCleanup]
            public void CleanUp()
            {
                testStudentProgramRepo = null;
                testTermRepo = null;
            }
            [TestMethod]
            public void CreateDegreePlan_StudentWithPrograms()
            {
                DegreePlan degreePlan = DegreePlan.CreateDegreePlan(student, studentPrograms, availableTerms, allPrograms);
                Assert.AreEqual(degreePlan.PersonId, "0000894");
                Assert.IsTrue(degreePlan.TermIds.Count() > 0);
            }

            [TestMethod]
            public async Task StudentWithoutAnticipatedCompletionDate()
            {
                List<Program> selectPrograms = new List<Program>();
                Program prog = allPrograms.Where(p => p.Code == "MATH.BS").First();
                prog.MonthsToComplete = 52;
                selectPrograms.Add(prog);
                StudentProgram sp = await testStudentProgramRepo.GetAsync("0000894", "MATH.BS");
                sp.StartDate = new DateTime(2011, 11, 1);
                List<StudentProgram> specificStudentPrograms = new List<StudentProgram>();
                specificStudentPrograms.Add(sp);
                DegreePlan degreePlan = DegreePlan.CreateDegreePlan(student, specificStudentPrograms, availableTerms, selectPrograms);
                Assert.AreEqual(degreePlan.PersonId, "0000894");
                DateTime CalculatedEnd = CalculateEndDate(specificStudentPrograms, selectPrograms);
                int termsToAddCount = availableTerms.Count(t => t.StartDate < CalculatedEnd && t.EndDate > DateTime.Today && t.DefaultOnPlan == true);
                Assert.AreEqual(termsToAddCount, degreePlan.TermIds.Count());
            }

            private DateTime CalculateEndDate(IEnumerable<StudentProgram> studentPrograms, IEnumerable<Program> programs)
            {
                // Loop thru the student's programs and determine the latest program anticpated completion date.  
                // For planning purposes we will always assume this is at least a year out.
                DateTime planEndDate = DateTime.Today.AddYears(1);
                if (studentPrograms != null)
                {
                    foreach (var studentProgram in studentPrograms)
                    {
                        DateTime studentCompletionDate = studentProgram.AnticipatedCompletionDate.GetValueOrDefault();
                        DateTime studentStartDate = studentProgram.StartDate.GetValueOrDefault();
                        if (studentProgram.AnticipatedCompletionDate == null && programs.Count() > 0)
                        {
                            // If the student program doesn't have an anticipated completion date but they do have a start date, see if we can
                            // calculate their anticipated completion based on the program's months to completion.
                            Program program = null;
                            try
                            {
                                program = programs.Where(p => p.Code == studentProgram.ProgramCode).FirstOrDefault();
                            }
                            catch
                            {

                            }
                            if (program != null)
                            {
                                if (program.MonthsToComplete > 0 && studentStartDate != DateTime.MinValue)
                                {
                                    var addYears = (decimal)(program.MonthsToComplete / 12);
                                    addYears = Math.Truncate(addYears);
                                    var addMonths = (decimal)(program.MonthsToComplete - (addYears * 12));

                                    var newMonth = studentStartDate.Month + addMonths;
                                    if (newMonth > 12)
                                    {
                                        newMonth -= 12;
                                        addYears += 1;
                                    }
                                    var newYear = studentStartDate.Year + addYears;
                                    studentCompletionDate = new DateTime(Convert.ToInt32(newYear), Convert.ToInt32(newMonth), 1);
                                }
                            }
                        }
                        if (studentCompletionDate > planEndDate)
                        {
                            planEndDate = studentCompletionDate;
                        }
                    }
                }
                return planEndDate;
            }

            [TestMethod]
            [ExpectedException(typeof(ExistingDegreePlanException))]
            public void CreateDegreePlanWhenStudentHasPlan_Exception()
            {
                Ellucian.Colleague.Domain.Student.Entities.Student student2 = new Student.Entities.Student("0000894", "Smith", 3, null, null);
                DegreePlan degreePlan = DegreePlan.CreateDegreePlan(student2, studentPrograms, availableTerms, allPrograms);
            }

            [TestMethod]
            public void TestDefaultTermsOnPlan_WithNoAvailableTerms()
            {
                DegreePlan degreePlan = DegreePlan.CreateDegreePlan(student, studentPrograms, null, allPrograms);
                Assert.AreEqual(degreePlan.TermIds.Count(), 0);
            }

            [TestMethod]
            public void TestDefaultTermsOnPlan_StudentWithNoPrograms()
            {
                // Should get terms for 1 year.
                DegreePlan degreePlan = DegreePlan.CreateDegreePlan(student, null, availableTerms, allPrograms);
                Assert.IsTrue(degreePlan.TermIds.Count() > 0);
            }

            [TestMethod]
            public void TestDefaultTermsOnPlan_CreateWithDefaultTerms_false()
            {
                // Should get terms for 1 year.
                DegreePlan degreePlan = DegreePlan.CreateDegreePlan(student, null, availableTerms, allPrograms, false);
                Assert.IsFalse(degreePlan.TermIds.Any());
            }
        }

        [TestClass]
        public class DegreePlanGetCourses
        {
            private string personId;
            private DegreePlan degreePlan;

            [TestInitialize]
            public void Initialize()
            {
                // Asserts in this class are based off this constructor statement, unless another constructor is used in the test method
                personId = "0000693";
                degreePlan = new DegreePlan(personId);
                degreePlan.AddTerm("2012/FA");
                degreePlan.AddTerm("2013/SP");
                degreePlan.AddTerm("2013/FA");
                degreePlan.AddCourse(new PlannedCourse(course: "111", coursePlaceholder: null), "2012/FA");
                degreePlan.AddCourse(new PlannedCourse(course: "222", coursePlaceholder: null), "2012/FA");
                degreePlan.AddCourse(new PlannedCourse(course: "333", section: "444", gradingType: GradingType.Audit, coursePlaceholder: null), "2013/SP");
            }

            [TestCleanup]
            public void CleanUp()
            {
            }

            [TestMethod]
            public void ReturnsEmptyIfNoCoursesInTerm()
            {
                Assert.AreEqual(0, degreePlan.GetPlannedCourses("2013/FA").Count());
            }

            [TestMethod]
            public void ReturnsEmptyIfTermNotOnPlan()
            {
                Assert.AreEqual(Enumerable.Empty<PlannedCourse>(), degreePlan.GetPlannedCourses("xxx"));
            }

            [TestMethod]
            public void ReturnsCoursesForTerm()
            {
                var courses = degreePlan.GetPlannedCourses("2012/FA");
                Assert.AreEqual(2, courses.Count());
                Assert.AreEqual(new PlannedCourse(course: "111", coursePlaceholder: null), courses.ElementAt(0));
            }

            [TestMethod]
            public void ReturnsCoursesIncludingCoursePlaceholdersForTerm()
            {
                //add course placeholder that should be included
                var plannedCourse = new PlannedCourse(course: null, section: null, coursePlaceholder: "abc");
                degreePlan.AddCourse(plannedCourse, "2012/FA");

                var courses = degreePlan.GetPlannedCourses("2012/FA");
                Assert.AreEqual(3, courses.Count());
                Assert.AreEqual(new PlannedCourse(course: "111", coursePlaceholder: null), courses.ElementAt(0));
                Assert.AreEqual(plannedCourse, courses.ElementAt(2));
            }

            [TestMethod]
            public void ReturnsCoursesForTerm_Audited()
            {
                var pcs = degreePlan.GetPlannedCourses("2013/SP");
                Assert.AreEqual(GradingType.Audit, pcs.ElementAt(0).GradingType);
            }
        }

        [TestClass]
        public class DegreePlanAddTerm
        {
            private string personId;
            private DegreePlan degreePlan;

            [TestInitialize]
            public void Initialize()
            {
                // Asserts in this class are based off this constructor statement, unless another constructor is used in the test method
                personId = "0000693";
                degreePlan = new DegreePlan(personId);
            }

            [TestCleanup]
            public void CleanUp()
            {
            }

            [TestMethod]
            public void AddingTermSuccessful()
            {
                degreePlan.AddTerm("2012/FA");
                Assert.AreEqual(1, degreePlan.TermIds.Count());
                Assert.AreEqual("2012/FA", degreePlan.TermIds.ElementAt(0));
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AddingTerm_NoTermCodeProvided()
            {
                degreePlan.AddTerm(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AddingTerm_EmptyTerm_ThrowsException()
            {
                degreePlan.AddTerm(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void AddingDuplicateTerm_ThrowsException()
            {
                degreePlan.AddTerm("2012/FA");
                degreePlan.AddTerm("2012/FA");
            }
        }

        [TestClass]
        public class DegreePlanAddCourse
        {
            private string personId;
            private DegreePlan degreePlan;

            [TestInitialize]
            public void Initialize()
            {
                // Asserts in this class are based off this constructor statement, unless another constructor is used in the test method
                personId = "0000693";
                degreePlan = new DegreePlan(personId);
                degreePlan.AddTerm("2012/FA");
                degreePlan.AddTerm("2013/SP");
                degreePlan.AddTerm("2013/FA");
                degreePlan.AddCourse(new PlannedCourse(course: "333", coursePlaceholder: null), "2013/SP");
            }

            [TestCleanup]
            public void CleanUp()
            {
            }

            [TestMethod]
            public void AddsTermAndCourseIfTermNotOnPlan()
            {
                degreePlan.AddCourse(new PlannedCourse(course: "111", coursePlaceholder: null), "2014/SP");
                Assert.AreEqual(new PlannedCourse(course: "111", coursePlaceholder: null), degreePlan.GetPlannedCourses("2014/SP").ElementAt(0));
            }

            [TestMethod]
            public void AddsCourseIfNotInTerm()
            {
                degreePlan.AddCourse(new PlannedCourse("222"), "2012/FA");
                var coursesInTerm = degreePlan.GetPlannedCourses("2012/FA");
                Assert.AreEqual(1, coursesInTerm.Count());
                Assert.AreEqual(new PlannedCourse("222"), coursesInTerm.ElementAt(0));
            }

            [TestMethod]
            public void DoesNotThrowExceptionIfCourseAlreadyInTerm()
            {
                degreePlan.AddCourse(new PlannedCourse("333"), "2013/SP");
                var coursesInTerm = degreePlan.GetPlannedCourses("2013/SP");
                Assert.AreEqual(2, coursesInTerm.Count());
            }

            [TestMethod]
            public void NonTermCourseNotAddedWithoutSection()
            {
                var count = degreePlan.NonTermPlannedCourses.Count();
                degreePlan.AddCourse(new PlannedCourse("111"), null);
                Assert.AreEqual(count, degreePlan.NonTermPlannedCourses.Count());
            }

            [TestMethod]
            public void PlannedCourseNotAddedIfObjectNull()
            {
                var count = degreePlan.NonTermPlannedCourses.Count();
                PlannedCourse plannedCourse = null;
                degreePlan.AddCourse(plannedCourse, null);
                Assert.AreEqual(count, degreePlan.NonTermPlannedCourses.Count());
            }

            [TestMethod]
            public void DoesNotThrowsExceptionIfCourseEmptyStringAndCoursePlaceholderHasValue()
            {
                degreePlan.AddCourse(new PlannedCourse(course: "", coursePlaceholder: "abc"), "2012/FA");
            }

            [TestMethod]
            public void DoesNotThrowsExceptionIfCourseHasValueAndCoursePlaceholderEmptyString()
            {
                degreePlan.AddCourse(new PlannedCourse(course: "abc", coursePlaceholder: ""), "2012/FA");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ThrowsExceptionIfCourseEmptyStringAndCoursePlaceholderEmptyString()
            {
                degreePlan.AddCourse(new PlannedCourse(course: "", coursePlaceholder: ""), "2012/FA");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ThrowsExceptionIfCourseEmptyStringAndCoursePlaceholderNull()
            {
                degreePlan.AddCourse(new PlannedCourse(course: "", coursePlaceholder: null), "2012/FA"); ; ;
            }

            //Course Placeholders
            [TestMethod]
            public void AddsTermAndCoursePlaceholderIfTermNotOnPlan()
            {
                degreePlan.AddCourse(new PlannedCourse(course: "", coursePlaceholder: "abc"), "2014/SP");
                Assert.AreEqual(new PlannedCourse(course: "", coursePlaceholder: "abc"), degreePlan.GetPlannedCourses("2014/SP").ElementAt(0));
            }

            [TestMethod]
            public void AddsCoursePlaceholderIfNotInTerm()
            {
                degreePlan.AddCourse(new PlannedCourse(course: "", coursePlaceholder: "abc"), "2012/FA");
                var coursesInTerm = degreePlan.GetPlannedCourses("2012/FA");
                Assert.AreEqual(1, coursesInTerm.Count());
                Assert.AreEqual(new PlannedCourse(course: "", coursePlaceholder: "abc"), coursesInTerm.ElementAt(0));
            }

            [TestMethod]
            public void DoesNotThrowExceptionOrAddDuplicateWhenCoursePlaceholderAlreadyInTerm()
            {
                // try to add the courseplaceholder to the same term twice
                degreePlan.AddCourse(new PlannedCourse(course: "", coursePlaceholder: "xyz"), "2013/SP");
                degreePlan.AddCourse(new PlannedCourse(course: "", coursePlaceholder: "xyz"), "2013/SP");
                var coursesInTerm = degreePlan.GetPlannedCourses("2013/SP");
                Assert.AreEqual(2, coursesInTerm.Count());
            }
        }

        [TestClass]
        public class DegreePlanCourseInPlan
        {
            private string personId;
            private DegreePlan degreePlan;

            [TestInitialize]
            public void Initialize()
            {
                personId = "0000693";
                degreePlan = new DegreePlan(personId);
                degreePlan.AddTerm("2012/FA");
                degreePlan.AddTerm("2013/SP");
                degreePlan.AddTerm("2013/FA");
                degreePlan.AddCourse(new PlannedCourse("111", "1112"), "2012/FA");
                degreePlan.AddCourse(new PlannedCourse("222", "2223"), "2012/FA");
                degreePlan.AddCourse(new PlannedCourse("333", "3334"), "2013/SP");
                degreePlan.AddCourse(new PlannedCourse("333", "3335"), "2013/FA");
            }

            [TestCleanup]
            public void CleanUp()
            {
                degreePlan = null;
            }

            [TestMethod]
            public void ReturnsTermIfCourseFound()
            {
                var termIds = degreePlan.TermsCoursePlanned("222");
                Assert.AreEqual(1, termIds.Count());
                Assert.AreEqual("2012/FA", termIds.ElementAt(0));
            }

            [TestMethod]
            public void ReturnsMultipleTerms()
            {
                var termIds = degreePlan.TermsCoursePlanned("333");
                Assert.AreEqual(2, termIds.Count());
                Assert.AreEqual("2013/SP", termIds.ElementAt(0));
                Assert.AreEqual("2013/FA", termIds.ElementAt(1));
            }

            [TestMethod]
            public void ReturnsFalseIfCourseNotFound()
            {
                var termIds = degreePlan.TermsCoursePlanned("2223");
                Assert.AreEqual(0, termIds.Count());
            }

            [TestMethod]
            public void ReturnsTermIfCoursePlaceholderFound()
            {
                degreePlan.AddCourse(new PlannedCourse(course: null, coursePlaceholder: "abc"), "2012/FA");
                degreePlan.AddCourse(new PlannedCourse("222", "2223"), "2012/FA");
                degreePlan.AddCourse(new PlannedCourse("333", "3334"), "2013/SP");
                degreePlan.AddCourse(new PlannedCourse("333", "3335"), "2013/FA");

                var termIds = degreePlan.TermsCoursePlaceholderPlanned(coursePlaceholderId: "abc");
                Assert.AreEqual(1, termIds.Count());
                Assert.AreEqual("2012/FA", termIds.ElementAt(0));
            }

            [TestMethod]
            public void ReturnsMultipleTermsIfCoursePlaceholderFound()
            {
                degreePlan.AddCourse(new PlannedCourse(course: null, coursePlaceholder: "abc"), "2012/FA");
                degreePlan.AddCourse(new PlannedCourse("222", "2223"), "2012/FA");
                degreePlan.AddCourse(new PlannedCourse(course: null, coursePlaceholder: "abc"), "2013/SP");
                degreePlan.AddCourse(new PlannedCourse("333", "3335"), "2013/FA");
                var termIds = degreePlan.TermsCoursePlaceholderPlanned(coursePlaceholderId: "abc");
                Assert.AreEqual(2, termIds.Count());
                Assert.AreEqual("2012/FA", termIds.ElementAt(0));
                Assert.AreEqual("2013/SP", termIds.ElementAt(1));
            }

            [TestMethod]
            public void ReturnsFalseIfCoursePlaceholderNotFound()
            {
                var termIds = degreePlan.TermsCoursePlaceholderPlanned(coursePlaceholderId: "abc");
                Assert.AreEqual(0, termIds.Count());
            }
        }

        [TestClass]
        public class DegreePlanSectionsInPlan
        {
            private string personId;
            private DegreePlan degreePlan;

            [TestInitialize]
            public void Initialize()
            {
                personId = "0000693";
                degreePlan = new DegreePlan(personId);
                degreePlan.AddTerm("2012/FA");
                degreePlan.AddTerm("2013/SP");
                degreePlan.AddTerm("2013/FA");
                degreePlan.AddCourse(new PlannedCourse(course: "111", section: "1112", coursePlaceholder: null), "2012/FA");
                degreePlan.AddCourse(new PlannedCourse(course: "222", section: "2223", coursePlaceholder: null), "2012/FA");
                degreePlan.AddCourse(new PlannedCourse(course: "333", section: "3334", coursePlaceholder: null), "2013/SP");
                degreePlan.AddCourse(new PlannedCourse(course: "444", section: "4445", coursePlaceholder: null), "");
            }

            [TestCleanup]
            public void CleanUp()
            {
                degreePlan = null;
            }

            [TestMethod]
            public void ContainsListOfAllSections()
            {
                Assert.IsTrue(degreePlan.SectionsInPlan.Count() == 4);
                Assert.IsTrue(degreePlan.SectionsInPlan.Contains("3334"));
                Assert.IsTrue(degreePlan.SectionsInPlan.Contains("4445"));
            }
        }

        [TestClass]
        public class DegreePlanCheckReq_Coreqs
        {
            private string personId;
            private DegreePlan degreePlan;
            private PlannedCourse plannedCourse1;
            private PlannedCourse plannedCourse2;
            private PlannedCourse plannedCourse3;
            List<Course> allCourses;
            List<Section> allSections;
            List<Term> allTerms;
            List<Term> regTerms;
            IEnumerable<RuleResult> RuleResults;
            IEnumerable<AcademicCredit> credits;
            IEnumerable<Requirement> requirements = new List<Requirement>();

            [TestInitialize]
            public async void Initialize()
            {
                personId = "00004010";
                degreePlan = new DegreePlan(personId);
                degreePlan.AddTerm("2012/SP");
                degreePlan.AddTerm("2012/FA");
                degreePlan.AddTerm("2013/SP");
                degreePlan.AddTerm("2014/FA");

                allCourses = (await new TestCourseRepository().GetAsync()).ToList();
                var x = allCourses.Where(c => c.Id == "7702");
                allTerms = new TestTermRepository().Get().ToList();
                regTerms = (await new TestTermRepository().GetRegistrationTermsAsync()).ToList();
                allSections = (await new TestSectionRepository().GetRegistrationSectionsAsync(regTerms)).ToList();
                var student = new TestStudentRepository().Get(personId);
                credits = (await new TestAcademicCreditRepository().GetAsync(student.AcademicCreditIds));
                RuleResults = new List<RuleResult>();
            }

            [TestMethod]
            public void HandlesPlannedCourseMissingFromDomain()
            {
                // Arrange--Set up degree plan with no coreqs planned
                plannedCourse1 = new PlannedCourse("9999999999", "");
                degreePlan.AddCourse(plannedCourse1, "2012/FA");
                degreePlan.AddCourse(new PlannedCourse(course: null, coursePlaceholder: "abc"), "2012/FA");

                // Act--Check for conflicts
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, credits, requirements, RuleResults);
            }

            [TestMethod]
            public void HandlesCoreqCourseMissingFromDomain()
            {
                // Arrange--Set up degree plan with course that has required coreqs
                plannedCourse1 = new PlannedCourse("7702", "");
                degreePlan.AddCourse(plannedCourse1, "2012/FA");
                degreePlan.AddCourse(new PlannedCourse(course: null, coursePlaceholder: "abc"), "2012/FA");

                // Act--Check for conflicts. Course List contains only planned course
                var courses = allCourses.Where(c => c.Id == "7702");
                degreePlan.CheckForConflicts(allTerms, regTerms, courses, allSections, credits, requirements, RuleResults);

                // Assert--Conflict messages appear for both coreq courses
                var plannedCourses = degreePlan.GetPlannedCourses("2012/FA");
                var pc1 = plannedCourses.Where(pc => pc.CourseId == "7702").First();
                Assert.AreEqual(2, pc1.Warnings.Count());
            }

            [TestMethod]
            public void NoMessageWhenPlannedCourseHasNoCoreqs()
            {
                // Course 7705 has no coreq courses

                // Arrange--Set up degree plan, add a course with no coreqs
                plannedCourse1 = new PlannedCourse("7705", "");
                degreePlan.AddCourse(plannedCourse1, "2012/FA");
                degreePlan.AddCourse(new PlannedCourse(course: null, coursePlaceholder: "abc"), "2012/FA");

                // Act--Check for conflicts
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, credits, requirements, RuleResults);

                // Assert--No messages appear, no coreqs are needed
                var plannedCourses = degreePlan.GetPlannedCourses("2012/FA");
                var pc1 = plannedCourses.Where(pc => pc.CourseId == "7705").First();
                var warnings = pc1.Warnings.Select(w => w.Type);
                // Verify that there are no corequisite warnings
                Assert.IsFalse(warnings.Contains(PlannedCourseWarningType.UnmetRequisite));
            }

            [TestMethod]
            public void MessageForMissingCourseCoreqs()
            {
                // Course 7702 has two coreq courses, neither planned: 
                //  110 (BIOL-100) Required
                //  143 (MATH-100) Not Required

                // Arrange--Set up degree plan with no coreqs planned
                plannedCourse1 = new PlannedCourse("7702", "");
                degreePlan.AddCourse(plannedCourse1, "2012/FA");
                degreePlan.AddCourse(new PlannedCourse(course: null, coursePlaceholder: "abc"), "2012/FA");

                // Act--Check for conflicts
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, credits, requirements, RuleResults);

                // Assert--Two messages appear, one for each missing coreq
                var plannedCourses = degreePlan.GetPlannedCourses("2012/FA");
                var pc1 = plannedCourses.Where(pc => pc.CourseId == "7702").First();
                var warnings = pc1.Warnings.Where(w => w.Type == PlannedCourseWarningType.UnmetRequisite);
                Assert.AreEqual(2, pc1.Warnings.Count());
                Assert.AreEqual("110", warnings.ElementAt(0).Requisite.CorequisiteCourseId);
                Assert.IsTrue(warnings.ElementAt(0).Requisite.IsRequired);
                Assert.AreEqual("143", warnings.ElementAt(1).Requisite.CorequisiteCourseId);
                Assert.IsFalse(warnings.ElementAt(1).Requisite.IsRequired);
            }

            [TestMethod]
            public void MessageWhenCourseCoreqsPlannedTooLate()
            {
                // Course 7702 has two coreq courses, planned in 2013/SP--too late to qualify as coreqs
                //  110 (BIOL-100) Required
                //  143 (MATH-100) Not Required

                // Arrange--Set up degree plan with coreq courses planned later than requiring course
                plannedCourse1 = new PlannedCourse("7702", "");
                degreePlan.AddCourse(plannedCourse1, "2012/FA");
                plannedCourse2 = new PlannedCourse("110", "");
                degreePlan.AddCourse(plannedCourse2, "2013/SP");
                plannedCourse3 = new PlannedCourse("143", "");
                degreePlan.AddCourse(plannedCourse3, "2013/SP");
                degreePlan.AddCourse(new PlannedCourse(course: null, coursePlaceholder: "abc"), "2012/FA");

                // Act--Check for conflicts
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, credits, requirements, RuleResults);

                // Assert--Conflict messages appear for both coreq courses
                var plannedCourses = degreePlan.GetPlannedCourses("2012/FA");
                var pc1 = plannedCourses.Where(pc => pc.CourseId == "7702").First();
                Assert.AreEqual(2, pc1.Warnings.Count());
                Assert.AreEqual("110", pc1.Warnings.ElementAt(0).Requisite.CorequisiteCourseId);
                Assert.AreEqual(PlannedCourseWarningType.UnmetRequisite, pc1.Warnings.ElementAt(0).Type);
                Assert.IsTrue(pc1.Warnings.ElementAt(0).Requisite.IsRequired);
                Assert.AreEqual("143", pc1.Warnings.ElementAt(1).Requisite.CorequisiteCourseId);
                Assert.AreEqual(PlannedCourseWarningType.UnmetRequisite, pc1.Warnings.ElementAt(1).Type);
                Assert.IsFalse(pc1.Warnings.ElementAt(1).Requisite.IsRequired);
            }

            [TestMethod]
            public void NoMessagesWhenCourseCoreqsPlannedInSameTerm()
            {
                // Course 7702 has two coreq courses, planned in concurrent term
                //  110 (BIOL-100) Required
                //  143 (MATH-100) Not Required

                // Arrange--Set up degree plan, set up coreqs for the same term as the requiring course
                plannedCourse1 = new PlannedCourse("7702", "");
                degreePlan.AddCourse(plannedCourse1, "2012/FA");
                plannedCourse2 = new PlannedCourse("110", "");
                degreePlan.AddCourse(plannedCourse2, "2012/FA");
                plannedCourse3 = new PlannedCourse("143", "");
                degreePlan.AddCourse(plannedCourse3, "2012/FA");
                degreePlan.AddCourse(new PlannedCourse(course: null, coursePlaceholder: "abc"), "2012/FA");

                // Act--Check for conflicts
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, credits, requirements, RuleResults);

                // Assert--No messages appear, coreqs have been planned
                var plannedCourses = degreePlan.GetPlannedCourses("2012/FA");
                var pc1 = plannedCourses.Where(pc => pc.CourseId == "7702").First();
                Assert.AreEqual(0, pc1.Warnings.Count());
            }

            [TestMethod]
            public void NoMessagesWhenCourseCoreqsPlannedInPriorTerm()
            {
                // Course 7702 has two coreq courses, planned in 2011/FA, 2012/SP--prior terms
                //  110 (BIOL-100) Required
                //  143 (MATH-100) Not Required

                // Arrange--Set up degree plan and plan coreqs for prior terms
                plannedCourse1 = new PlannedCourse("7702", "");
                degreePlan.AddCourse(plannedCourse1, "2012/FA");
                plannedCourse2 = new PlannedCourse("110", "");
                degreePlan.AddCourse(plannedCourse2, "2011/FA");
                plannedCourse3 = new PlannedCourse("143", "");
                degreePlan.AddCourse(plannedCourse3, "2012/SP");
                degreePlan.AddCourse(new PlannedCourse(course: null, coursePlaceholder: "abc"), "2012/FA");

                // Act--check for conflicts
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, credits, requirements, RuleResults);

                // Assert--No messages appear since both coreqs have been planned
                var plannedCourses = degreePlan.GetPlannedCourses("2012/FA");
                var pc1 = plannedCourses.Where(pc => pc.CourseId == "7702").First();
                Assert.AreEqual(0, pc1.Warnings.Count());
            }

            [TestMethod]
            public void NoMessagesForCourseCoreqsPlannedInCurrentAndPriorTerm()
            {
                // Course 7702 has two coreq courses
                //  110 (BIOL-100) Required
                //  143 (MATH-100) Not Required

                // Arrange--Set up plan and add required coreqs, one planned concurrently, one planned in prior term
                plannedCourse1 = new PlannedCourse("7702", "");
                degreePlan.AddCourse(plannedCourse1, "2012/FA");
                plannedCourse2 = new PlannedCourse("110", "");
                degreePlan.AddCourse(plannedCourse2, "2012/FA");
                plannedCourse3 = new PlannedCourse("143", "");
                degreePlan.AddCourse(plannedCourse3, "2011/SP");
                degreePlan.AddCourse(new PlannedCourse(course: null, coursePlaceholder: "abc"), "2012/FA");

                // Act--Check for conflicts
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, credits, requirements, RuleResults);

                // Assert--No messages appear because coreqs may be satisfied in current or prior terms
                var plannedCourses = degreePlan.GetPlannedCourses("2012/FA");
                var pc1 = plannedCourses.Where(pc => pc.CourseId == "7702").First();
                Assert.AreEqual(0, pc1.Warnings.Count());
            }

            [TestMethod]
            public void MessageForSectionCourseCoreqNotPlanned()
            {
                // Section 7702 2012/FA section has two course coreqs:
                //  42 (HIST-200) Required
                //  21 (BIOL-200) Not Required

                // Arrange--Set up degree plan, do not add either corequisite course
                var sec1 = allSections.Where(s => s.CourseId == "7702" && s.TermId == "2012/FA").First();
                plannedCourse1 = new PlannedCourse("7702", sec1.Id);
                degreePlan.AddCourse(plannedCourse1, "2012/FA");
                degreePlan.AddCourse(new PlannedCourse(course: null, coursePlaceholder: "abc"), "2012/FA");

                // Act--Check for conflicts
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, credits, requirements, RuleResults);

                // Assert--Messages found for both missing corequisite courses
                var plannedCourses = degreePlan.GetPlannedCourses("2012/FA");
                var pc1 = plannedCourses.Where(pc => pc.CourseId == "7702").First();
                Assert.AreEqual(2, pc1.Warnings.Count());
                Assert.AreEqual("42", pc1.Warnings.ElementAt(0).Requisite.CorequisiteCourseId);
                Assert.AreEqual(PlannedCourseWarningType.UnmetRequisite, pc1.Warnings.ElementAt(0).Type);
                Assert.IsTrue(pc1.Warnings.ElementAt(0).Requisite.IsRequired);
                Assert.AreEqual("21", pc1.Warnings.ElementAt(1).Requisite.CorequisiteCourseId);
                Assert.AreEqual(PlannedCourseWarningType.UnmetRequisite, pc1.Warnings.ElementAt(1).Type);
                Assert.IsFalse(pc1.Warnings.ElementAt(1).Requisite.IsRequired);
            }

            [TestMethod]
            public void MessageWhenSectionCourseCoreqPlannedTooLate()
            {
                // Course 7702 2012/FA section has two course coreqs, both planned but in a later term
                //  42 (HIST-200) Required
                //  21 (BIOL-200) Not Required

                // Arrange--Set up degree plan with section course corequisites planned for a later term
                var sec1 = allSections.Where(s => s.CourseId == "7702" && s.TermId == "2012/FA").First();
                plannedCourse1 = new PlannedCourse("7702", sec1.Id);
                degreePlan.AddCourse(plannedCourse1, "2012/FA");
                plannedCourse2 = new PlannedCourse("42", "");
                degreePlan.AddCourse(plannedCourse2, "2014/FA");
                plannedCourse3 = new PlannedCourse("21", "");
                degreePlan.AddCourse(plannedCourse3, "2014/FA");
                degreePlan.AddCourse(new PlannedCourse(course: null, coursePlaceholder: "abc"), "2012/FA");

                // Act--Check for conflicts
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, credits, requirements, RuleResults);

                // Assert--Messages appear for both courses, since coreqs must be taken in the same or prior term
                var plannedCourses = degreePlan.GetPlannedCourses("2012/FA");
                var pc1 = plannedCourses.Where(pc => pc.CourseId == "7702").First();
                Assert.AreEqual(2, pc1.Warnings.Count());
                Assert.AreEqual("42", pc1.Warnings.ElementAt(0).Requisite.CorequisiteCourseId);
                Assert.AreEqual(PlannedCourseWarningType.UnmetRequisite, pc1.Warnings.ElementAt(0).Type);
                Assert.IsTrue(pc1.Warnings.ElementAt(0).Requisite.IsRequired);
                Assert.AreEqual("21", pc1.Warnings.ElementAt(1).Requisite.CorequisiteCourseId);
                Assert.AreEqual(PlannedCourseWarningType.UnmetRequisite, pc1.Warnings.ElementAt(1).Type);
                Assert.IsFalse(pc1.Warnings.ElementAt(1).Requisite.IsRequired);
            }

            [TestMethod]
            public void NoMessageWhenSectionCourseCoreqPlannedInSameTerm()
            {
                // Course 7702 2012/FA section has two course coreqs, planned in the same term (concurrently)
                //  42 (HIST-200) Required
                //  21 (BIOL-200) Not Required

                // Arrange--Set up degree plan with both coreq courses in the same term
                var sec1 = allSections.Where(s => s.CourseId == "7702" && s.TermId == "2012/FA").First();
                plannedCourse1 = new PlannedCourse("7702", sec1.Id);
                degreePlan.AddCourse(plannedCourse1, "2012/FA");
                plannedCourse2 = new PlannedCourse("42", "");
                degreePlan.AddCourse(plannedCourse2, "2012/FA");
                plannedCourse3 = new PlannedCourse("21", "");
                degreePlan.AddCourse(plannedCourse3, "2012/FA");
                degreePlan.AddCourse(new PlannedCourse(course: null, coursePlaceholder: "abc"), "2012/FA");

                // Act--Check for conflicts
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, credits, requirements, RuleResults);

                // Assert--No messages appear because coreqs taken in the same term satisfy the corequisite
                var plannedCourses = degreePlan.GetPlannedCourses("2012/FA");
                var pc1 = plannedCourses.Where(pc => pc.CourseId == "7702").First();
                Assert.AreEqual(0, pc1.Warnings.Count());
            }

            [TestMethod]
            public void NoMessageWhenSectionCourseCoreqPlannedInPriorTerm()
            {
                // Course 7702 2012/FA section has two course coreqs, both are planned in a prior term (system considers a coreq
                // taken in a prior term to be "met")
                //  42 (HIST-200) Required
                //  21 (BIOL-200) Not Required                

                // Arrange--Set up degree plan, adding coreq courses to a prior term
                var sec1 = allSections.Where(s => s.CourseId == "7702" && s.TermId == "2012/FA").First();
                plannedCourse1 = new PlannedCourse("7702", sec1.Id);
                degreePlan.AddCourse(plannedCourse1, "2012/FA");
                plannedCourse2 = new PlannedCourse("42", "");
                degreePlan.AddCourse(plannedCourse2, "2011/FA");
                plannedCourse3 = new PlannedCourse("21", "");
                degreePlan.AddCourse(plannedCourse3, "2011/FA");
                degreePlan.AddCourse(new PlannedCourse(course: null, coursePlaceholder: "abc"), "2012/FA");

                //Act--check for conflicts
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, credits, requirements, RuleResults);

                // Assert--Since both course coreqs are satisfied, no messages appear
                var plannedCourses = degreePlan.GetPlannedCourses("2012/FA");
                var pc1 = plannedCourses.Where(pc => pc.CourseId == "7702").First();
                Assert.AreEqual(0, pc1.Warnings.Count());
            }

            [TestMethod]
            public void NoMessageWhenSectionCourseCoreqInAcadCreditsPriorTerm()
            {
                // Course 7702 2012/FA section has two course coreqs, both are planned/acadCredit in a prior term (system considers a coreq
                // taken in a prior term to be "met")
                //  42 (HIST-200) Required
                //  21 (BIOL-200) Not Required                

                // Arrange--Set up degree plan, adding coreq courses to a prior term
                var sec1 = allSections.Where(s => s.CourseId == "7702" && s.TermId == "2012/FA").First();
                plannedCourse1 = new PlannedCourse("7702", sec1.Id);
                degreePlan.AddCourse(plannedCourse1, "2012/FA");
                // Add BIOL-200 as a planned course
                plannedCourse3 = new PlannedCourse("21", "");
                degreePlan.AddCourse(plannedCourse3, "2011/FA");
                // Add HIST-200 as an academic credit
                var course1 = allCourses.Where(c => c.Id == "42").FirstOrDefault();
                var section1 = allSections.Where(s => s.CourseId == "42").FirstOrDefault();
                var acadCredit1 = new AcademicCredit("01", course1, section1.Id);
                acadCredit1.TermCode = "2011/FA";
                var acadCredits = credits.Union(new List<AcademicCredit>() { acadCredit1 });

                degreePlan.AddCourse(new PlannedCourse(course: null, coursePlaceholder: "abc"), "2012/FA");

                //Act--check for conflicts
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, acadCredits, requirements, RuleResults);


                // Assert--Since both course coreqs are satisfied, no messages appear
                var plannedCourses = degreePlan.GetPlannedCourses("2012/FA");
                var pc1 = plannedCourses.Where(pc => pc.CourseId == "7702").First();
                Assert.AreEqual(0, pc1.Warnings.Count());
            }

            [TestMethod]
            public void MessageWhenSectionCourseCoreqInAcadCreditInFutureTerm()
            {
                // Course 7702 2012/FA section has two course coreqs, acad credit item found in future term 
                //  42 (HIST-200) Required
                //  21 (BIOL-200) Not Required                

                // Arrange--Set up degree plan, adding coreq courses to a prior term
                var sec1 = allSections.Where(s => s.CourseId == "7702" && s.TermId == "2012/FA").First();
                plannedCourse1 = new PlannedCourse("7702", sec1.Id);
                degreePlan.AddCourse(plannedCourse1, "2012/FA");
                // Add BIOL-200 as a planned course
                plannedCourse3 = new PlannedCourse("21", "");
                degreePlan.AddCourse(plannedCourse3, "2011/FA");
                // Add HIST-200 as an academic credit in future term
                var course1 = allCourses.Where(c => c.Id == "42").FirstOrDefault();
                var section1 = allSections.Where(s => s.CourseId == "42").FirstOrDefault();
                var acadCredit1 = new AcademicCredit("01", course1, section1.Id);
                acadCredit1.TermCode = "2013/FA";
                var acadCredits = credits.Union(new List<AcademicCredit>() { acadCredit1 });

                degreePlan.AddCourse(new PlannedCourse(course: null, coursePlaceholder: "abc"), "2012/FA");

                //Act--check for conflicts
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, acadCredits, requirements, RuleResults);

                // Assert--One message appears for the acad credit found in a later term
                var plannedCourses = degreePlan.GetPlannedCourses("2012/FA");
                var pc1 = plannedCourses.Where(pc => pc.CourseId == "7702").First();
                Assert.AreEqual(1, pc1.Warnings.Count());
                Assert.AreEqual("42", pc1.Warnings.ElementAt(0).Requisite.CorequisiteCourseId);
                Assert.AreEqual(PlannedCourseWarningType.UnmetRequisite, pc1.Warnings.ElementAt(0).Type);
                Assert.IsTrue(pc1.Warnings.ElementAt(0).Requisite.IsRequired);
            }

            [TestMethod]
            public void MessagesWhenAllSectionCoreqMissing()
            {
                // Course 7703 2012/FA section has two Course coreqs, none planned
                //  42 (HIST-200) Required
                //  21 (BIOL-200) Not Required
                // And the following section coreqs, none planned
                //  (HIST 400 02(course Id 87), PHYS-100-01 (courseId 154), MATH-152-03 (courseId 333) 2 out of 3 Required
                //  (MATH 400 02, SOCI-100-01) Recommended

                // Arrange--Set up degree plan, don't add any coreq courses or sections
                var sec1 = allSections.Where(s => s.CourseId == "7703" && s.TermId == "2012/FA").First();
                plannedCourse1 = new PlannedCourse("7703", sec1.Id);
                degreePlan.AddCourse(plannedCourse1, "2012/FA");
                degreePlan.AddCourse(new PlannedCourse(course: null, coursePlaceholder: "abc"), "2012/FA");

                // Act--Check for conflicts
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, credits, requirements, RuleResults);

                // Assert--Four messages should appear, two for the missing courses, two for the missing sections
                var plannedCourses = degreePlan.GetPlannedCourses("2012/FA");
                var pc1 = plannedCourses.Where(pc => pc.CourseId == "7703").First();
                Assert.AreEqual(5, pc1.Warnings.Count());

                // Loop through the Requisites, verify there is a matching warning for each one (assuming all are unmet)
                foreach (var req in sec1.Requisites)
                {
                    var reqWarnings = pc1.Warnings.Where(sw => sw.Requisite != null);
                    if (!string.IsNullOrEmpty(req.CorequisiteCourseId))
                    {
                        // and a warning for the unmet requisites specified by coreq course Id 
                        var warning = reqWarnings.Where(w => w.Requisite.CorequisiteCourseId == req.CorequisiteCourseId).FirstOrDefault();
                        Assert.IsNotNull(warning);
                        Assert.AreEqual(PlannedCourseWarningType.UnmetRequisite, warning.Type);
                        Assert.AreEqual(req.IsRequired, warning.Requisite.IsRequired);
                        Assert.AreEqual(req.CompletionOrder, warning.Requisite.CompletionOrder);
                    }
                    else
                    {
                        // and a warning for the unmet requisites specified by requisite code
                        var warning = reqWarnings.Where(w => w.Requisite.RequirementCode == req.RequirementCode).FirstOrDefault();
                        Assert.IsNotNull(warning);
                        Assert.AreEqual(PlannedCourseWarningType.UnmetRequisite, warning.Type);
                        Assert.AreEqual(req.IsRequired, warning.Requisite.IsRequired);
                        Assert.AreEqual(req.CompletionOrder, warning.Requisite.CompletionOrder);
                    }
                }

                foreach (var req in sec1.SectionRequisites)
                {
                    if (req.CorequisiteSectionIds != null)
                    {
                        var secWarnings = pc1.Warnings.Where(sw => sw.SectionRequisite != null);
                        if (req.IsRequired && req.NumberNeeded > 0)
                        {
                            // find the warning for this requisite, there will be only one
                            var warning = secWarnings.Where(w => w.SectionRequisite.IsRequired == req.IsRequired && w.SectionRequisite.NumberNeeded == req.NumberNeeded).First();
                            Assert.IsNotNull(warning);
                            Assert.AreEqual(PlannedCourseWarningType.UnmetRequisite, warning.Type);
                            Assert.AreEqual(req, warning.SectionRequisite);
                        }
                        else
                        {
                            // find a warning for each recommended sectoin
                            foreach (var recommendedSectionId in req.CorequisiteSectionIds)
                            {
                                var warning = secWarnings.Where(w => w.SectionId == recommendedSectionId).First();
                                Assert.IsNotNull(warning);
                                Assert.AreEqual(PlannedCourseWarningType.UnmetRequisite, warning.Type);
                                Assert.AreEqual(req, warning.SectionRequisite);
                            }
                        }
                    }
                }
            }


            [TestMethod]
            public void NoMessageWhenSectionSectionCoreqPlanned()
            {
                // Course 7703 2012/FA section has two Course coreqs, not planned, messages will result:
                //  42 (HIST-200) Required
                //  21 (BIOL-200) Not Required
                // And section coreqs, all planned, no messages result
                //  (HIST 400 02(course Id 87), PHYS-100-01 (courseId 154), MATH-152-03 (courseId 333) 2 out of 3 Required
                //  (MATH 400 02, SOCI-100-01) Recommended

                // Arrange--Set up degree plan with the required sections planned, but not the required courses planned
                var sec1 = allSections.Where(s => s.CourseId == "7703" && s.TermId == "2012/FA").First();
                var plannedCourse1 = new PlannedCourse("7703", sec1.Id);
                degreePlan.AddCourse(plannedCourse1, "2012/FA");
                degreePlan.AddCourse(new PlannedCourse(course: null, coursePlaceholder: "abc"), "2012/FA");

                // Add a planned section for the sections cited in the section requisites--first the multi- requisite sections
                var coreqSectionRequisite = sec1.SectionRequisites.Where(r => r.CorequisiteSectionIds.Count() > 1).FirstOrDefault();
                // For the required 2 out of 3 requisites, add two out of the 3 as planned courses; should be enough to satisfy requisite
                var coreqSec = allSections.Where(s => s.Id == coreqSectionRequisite.CorequisiteSectionIds.ElementAt(0)).FirstOrDefault();
                var plannedCourse2 = new PlannedCourse(coreqSec.CourseId, coreqSec.Id);
                degreePlan.AddCourse(plannedCourse2, "2012/FA");
                coreqSec = allSections.Where(s => s.Id == coreqSectionRequisite.CorequisiteSectionIds.ElementAt(2)).FirstOrDefault();
                var plannedCourse4 = new PlannedCourse(coreqSec.CourseId, coreqSec.Id);
                degreePlan.AddCourse(plannedCourse4, "2012/FA");

                // now add planned courses for both of the single requisite sections
                var coreqSectionRequisites = sec1.SectionRequisites.Where(r => r.CorequisiteSectionIds.Count() == 1);

                coreqSec = allSections.Where(s => s.Id == coreqSectionRequisites.ElementAt(0).CorequisiteSectionIds.ElementAt(0)).FirstOrDefault();
                var plannedCourse5 = new PlannedCourse(coreqSec.CourseId, coreqSec.Id);
                degreePlan.AddCourse(plannedCourse5, "2012/FA");

                coreqSec = allSections.Where(s => s.Id == coreqSectionRequisites.ElementAt(1).CorequisiteSectionIds.ElementAt(0)).FirstOrDefault();
                var plannedCourse6 = new PlannedCourse(coreqSec.CourseId, coreqSec.Id);
                degreePlan.AddCourse(plannedCourse6, "2012/FA");

                // Act--Check for conflicts
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, credits, requirements, RuleResults);

                // Assert--Two messages result, both reporting missing planned courses, no messages since all requisite sections were planned
                var plannedCourses = degreePlan.GetPlannedCourses("2012/FA");
                var pc1 = plannedCourses.Where(pc => pc.CourseId == "7703").First();
                Assert.AreEqual(2, pc1.Warnings.Count());
                // Check on section course coreq messages
                Assert.AreEqual("42", pc1.Warnings.ElementAt(0).Requisite.CorequisiteCourseId);
                Assert.AreEqual(PlannedCourseWarningType.UnmetRequisite, pc1.Warnings.ElementAt(0).Type);
                Assert.IsTrue(pc1.Warnings.ElementAt(0).Requisite.IsRequired);
                Assert.AreEqual(RequisiteCompletionOrder.PreviousOrConcurrent, pc1.Warnings.ElementAt(0).Requisite.CompletionOrder);
                Assert.AreEqual("21", pc1.Warnings.ElementAt(1).Requisite.CorequisiteCourseId);
                Assert.AreEqual(PlannedCourseWarningType.UnmetRequisite, pc1.Warnings.ElementAt(1).Type);
                Assert.IsFalse(pc1.Warnings.ElementAt(1).Requisite.IsRequired);
                Assert.AreEqual(RequisiteCompletionOrder.PreviousOrConcurrent, pc1.Warnings.ElementAt(1).Requisite.CompletionOrder);
                // No section section coreq messages
            }

            [TestMethod]
            public void NoMessageWhenSectionSectionCoreqInCredits()
            {
                // Course 7703 2012/FA section has two Course coreqs and three section requisites.
                // Add section requisites as academic credits.
                // Verify messages for only the missing course requisites

                // Arrange--Set up degree plan, don't add any coreq courses or sections
                var sec1 = allSections.Where(s => s.CourseId == "7703" && s.TermId == "2012/FA").First();
                plannedCourse1 = new PlannedCourse("7703", sec1.Id);
                degreePlan.AddCourse(plannedCourse1, "2012/FA");
                // Get the section and the course for the two single section-section corequisites  (MATH-400-02, SOCI-100-01) and build acad credit for each of them
                var coreqSectionRequisites = sec1.SectionRequisites.Where(r => r.CorequisiteSectionIds.Count() == 1);
                var coreqSec = allSections.Where(s => s.Id == coreqSectionRequisites.ElementAt(0).CorequisiteSectionIds.ElementAt(0)).First();
                var coreqSecCourse = allCourses.Where(c => c.Id == coreqSec.CourseId).First();
                var credit1 = new AcademicCredit("01", coreqSecCourse, coreqSec.Id);
                coreqSec = allSections.Where(s => s.Id == coreqSectionRequisites.ElementAt(1).CorequisiteSectionIds.ElementAt(0)).First();
                coreqSecCourse = allCourses.Where(c => c.Id == coreqSec.CourseId).First();
                var credit2 = new AcademicCredit("02", coreqSecCourse, coreqSec.Id);
                // Get the section and the course for the section-multisection corequisite  (HIST-400-02, PHYS-100-01) and build acad credit for them
                var multiRequisite = sec1.SectionRequisites.Where(r => r.CorequisiteSectionIds.Count() > 1).First();
                coreqSec = allSections.Where(s => s.Id == multiRequisite.CorequisiteSectionIds.ElementAt(0)).First();
                coreqSecCourse = allCourses.Where(c => c.Id == coreqSec.CourseId).FirstOrDefault();
                var credit3 = new AcademicCredit("03", coreqSecCourse, coreqSec.Id);
                coreqSec = allSections.Where(s => s.Id == multiRequisite.CorequisiteSectionIds.ElementAt(1)).First();
                coreqSecCourse = allCourses.Where(c => c.Id == coreqSec.CourseId).FirstOrDefault();
                var credit4 = new AcademicCredit("04", coreqSecCourse, coreqSec.Id);
                credits = new List<AcademicCredit>() { credit1, credit2, credit3, credit4 };
                degreePlan.AddCourse(new PlannedCourse(course: null, coursePlaceholder: "abc"), "2012/FA");


                // Act--Check for conflicts
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, credits, requirements, RuleResults);

                // Assert--Two messages result, both reporting missing course requisites. No messages section requisites since sections are in credits
                var plannedCourses = degreePlan.GetPlannedCourses("2012/FA");
                var pc1 = plannedCourses.Where(pc => pc.CourseId == "7703").First();
                Assert.AreEqual(2, pc1.Warnings.Count());
                // Check on section course coreq messages
                Assert.AreEqual("42", pc1.Warnings.ElementAt(0).Requisite.CorequisiteCourseId);
                Assert.AreEqual(PlannedCourseWarningType.UnmetRequisite, pc1.Warnings.ElementAt(0).Type);
                Assert.IsTrue(pc1.Warnings.ElementAt(0).Requisite.IsRequired);
                Assert.AreEqual(RequisiteCompletionOrder.PreviousOrConcurrent, pc1.Warnings.ElementAt(0).Requisite.CompletionOrder);
                Assert.AreEqual("21", pc1.Warnings.ElementAt(1).Requisite.CorequisiteCourseId);
                Assert.AreEqual(PlannedCourseWarningType.UnmetRequisite, pc1.Warnings.ElementAt(1).Type);
                Assert.IsFalse(pc1.Warnings.ElementAt(1).Requisite.IsRequired);
                Assert.AreEqual(RequisiteCompletionOrder.PreviousOrConcurrent, pc1.Warnings.ElementAt(0).Requisite.CompletionOrder);
            }

            [TestMethod]
            public void NoMessageWhenAllSectionCoreqsPlanned()
            {
                // Course 7703 section has two Course coreqs, both planned
                //  42 (HIST-200) Required
                //  21 (BIOL-200) Not Required
                // And section coreqs, all planned, no messages result
                //  (HIST 400 02(course Id 87), PHYS-100-01 (courseId 154), MATH-152-03 (courseId 333) 2 out of 3 Required
                //  (MATH 400 02) recommended
                //  (SOCI-100-01) Required

                // Arrange--Set up degree plan, adding all required courses and sections
                var sec1 = allSections.Where(s => s.CourseId == "7703" && s.TermId == "2012/FA").First();
                plannedCourse1 = new PlannedCourse("7703", sec1.Id);
                degreePlan.AddCourse(plannedCourse1, "2012/FA");
                degreePlan.AddCourse(new PlannedCourse(course: null, coursePlaceholder: "abc"), "2012/FA");

                // Add a planned section for the sections cited in the section requisites--first the multi-section requisite
                var coreqSectionRequisite = sec1.SectionRequisites.Where(r => r.CorequisiteSectionIds.Count() > 1 && r.IsRequired).First();

                // For the required 2 out of 3 requisites, add two out of the 3 as planned courses; should be enough to satisfy requisite
                var coreqSec = allSections.Where(s => s.Id == coreqSectionRequisite.CorequisiteSectionIds.ElementAt(0)).FirstOrDefault();
                var plannedCourse2 = new PlannedCourse(coreqSec.CourseId, coreqSec.Id);
                degreePlan.AddCourse(plannedCourse2, "2012/FA");
                coreqSec = allSections.Where(s => s.Id == coreqSectionRequisite.CorequisiteSectionIds.ElementAt(2)).FirstOrDefault();
                var plannedCourse4 = new PlannedCourse(coreqSec.CourseId, coreqSec.Id);
                degreePlan.AddCourse(plannedCourse4, "2012/FA");

                // Add planned courses for the recommended requisite sections
                var coreqSectionRequisites = sec1.SectionRequisites.Where(r => r.CorequisiteSectionIds.Count() == 1);
                coreqSec = allSections.Where(s => s.Id == coreqSectionRequisites.ElementAt(0).CorequisiteSectionIds.ElementAt(0)).FirstOrDefault();
                var plannedCourse5 = new PlannedCourse(coreqSec.CourseId, coreqSec.Id);
                degreePlan.AddCourse(plannedCourse5, "2012/FA");
                coreqSec = allSections.Where(s => s.Id == coreqSectionRequisites.ElementAt(1).CorequisiteSectionIds.ElementAt(0)).FirstOrDefault();
                var plannedCourse6 = new PlannedCourse(coreqSec.CourseId, coreqSec.Id);
                degreePlan.AddCourse(plannedCourse6, "2012/FA");

                // Add planned courses for the coreq courses
                var plannedCourse7 = new PlannedCourse("42", "");
                degreePlan.AddCourse(plannedCourse7, "2011/FA");
                var plannedCourse8 = new PlannedCourse("21", "");
                degreePlan.AddCourse(plannedCourse8, "2011/FA");

                // Act--Check plan for conflicts
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, credits, requirements, RuleResults);

                // Assert--No coreq conflicts are reported
                var plannedCourses = degreePlan.GetPlannedCourses("2012/FA");
                var pc1 = plannedCourses.Where(pc => pc.CourseId == "7703").First();
                Assert.AreEqual(0, pc1.Warnings.Count());
            }

            [TestMethod]
            public void MessagesWhenHalfOfASectionsCoreqsArePlanned()
            {
                // Course 7703 section has the following coreqs
                //  42 (HIST-200) Required, planned
                //  21 (BIOL-200) Not Required, not planned--WARNING EXPECTED
                // And section coreq
                //  (HIST 400 02(course Id 87), PHYS-100-01 (courseId 154), MATH-152-03 (courseId 333), 2 out of 3 required, 1 out of 3 planned -- WARNING EXPECTED
                //  SOCI-100-01 (course id 159, recommended) not planned--WARNING EXPECTED
                //  MATH-400(courseId 91 recommended) planned

                // Arrange--Set up degree plan, adding the first coreq course and the first coreq section
                // Arrange--Set up degree plan, adding all required courses and sections
                var sec1 = allSections.Where(s => s.CourseId == "7703" && s.TermId == "2012/FA").First();
                plannedCourse1 = new PlannedCourse("7703", sec1.Id);
                degreePlan.AddCourse(plannedCourse1, "2012/FA");

                // Single section requisite
                var coreqSectionRequisite = sec1.SectionRequisites.Where(r => r.CorequisiteSectionIds.Count() == 1).First();
                var coreqSec = allSections.Where(s => s.Id == coreqSectionRequisite.CorequisiteSectionIds.ElementAt(0)).FirstOrDefault();
                var plannedCourse2 = new PlannedCourse(coreqSec.CourseId, coreqSec.Id);
                degreePlan.AddCourse(plannedCourse2, "2012/FA");

                // Add one planned course for the recommended requisite sections (doesn't matter which, just pick the first section in the multisection requisite)
                coreqSectionRequisite = sec1.SectionRequisites.Where(r => r.CorequisiteSectionIds.Count() > 1).First();
                coreqSec = allSections.Where(s => s.Id == coreqSectionRequisite.CorequisiteSectionIds.ElementAt(0)).FirstOrDefault();
                var plannedCourse5 = new PlannedCourse(coreqSec.CourseId, coreqSec.Id);
                degreePlan.AddCourse(plannedCourse5, "2012/FA");

                // Add one of the two coreq courses
                var plannedCourse7 = new PlannedCourse("42", "");
                degreePlan.AddCourse(plannedCourse7, "2011/FA");

                degreePlan.AddCourse(new PlannedCourse(course: null, coursePlaceholder: "abc"), "2012/FA");

                // Act--Check plan for conflicts
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, credits, requirements, RuleResults);

                // Assert--Coreq conflicts are reported for the requisites NOT completely planned
                var plannedCourses = degreePlan.GetPlannedCourses("2012/FA");
                var pc1 = plannedCourses.Where(pc => pc.CourseId == "7703").First();
                Assert.AreEqual(3, pc1.Warnings.Count());

                // verify requisite course warning for missing coreq course BIOL 200 (course Id 21)
                var warning = pc1.Warnings.Where(w => (w.Requisite != null) && (w.Requisite.CorequisiteCourseId == "21")).First();
                Assert.AreEqual(PlannedCourseWarningType.UnmetRequisite, warning.Type);

                // verify requisite section warning for missing requisite section
                warning = pc1.Warnings.Where(w => w.SectionRequisite != null && w.SectionRequisite.CorequisiteSectionIds.Count() == 1).First();
                Assert.AreEqual(PlannedCourseWarningType.UnmetRequisite, warning.Type);

                // verify requisite section warning for required multi-section requisite
                warning = pc1.Warnings.Where(w => w.SectionRequisite != null && w.SectionRequisite.CorequisiteSectionIds.Count() > 1).First();
                Assert.AreEqual(PlannedCourseWarningType.UnmetRequisite, warning.Type);
            }

            [TestMethod]
            public void NoMessageWhenCoreqsOnCourseAreNotOnPlannedSection()
            {
                // Course 7704 has two coreq courses, but planned section has no Coreqs so no coreq enforcement is done.
                //  110 (BIOL-100) Required
                //  143 (MATH-100) Not Required

                // Arrange--Add course 7704 to this degree plan with a section from 2012/FA
                var sec1 = allSections.Where(s => s.CourseId == "7704" && s.TermId == "2012/FA" && s.Number == "02").First();
                plannedCourse1 = new PlannedCourse("7704", sec1.Id);
                degreePlan.AddCourse(plannedCourse1, "2012/FA");

                degreePlan.AddCourse(new PlannedCourse(course: null, coursePlaceholder: "abc"), "2012/FA");

                // Act--Check for conflicts
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, credits, requirements, RuleResults);

                // Assert--There are no conflict messages associated with this course
                var plannedCourses = degreePlan.GetPlannedCourses("2012/FA");
                var pc1 = plannedCourses.Where(pc => pc.CourseId == "7704").First();
                Assert.AreEqual(0, pc1.Warnings.Count());
            }

            [TestMethod]
            public void MessageWhenNonTermSectionCoreqsNotPlanned()
            {
                // Arrange--Add course 7704, non-term section 04 to this degree plan
                var sec1 = allSections.Where(s => s.CourseId == "7704" && s.Number == "04").First();
                plannedCourse1 = new PlannedCourse("7704", sec1.Id);
                degreePlan.AddCourse(plannedCourse1, "");   // Since this section has no term, added to non-term planned courses

                degreePlan.AddCourse(new PlannedCourse(course: null, coursePlaceholder: "abc"), "2012/FA");

                // Act--Check for conflicts
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, credits, requirements, RuleResults);

                // Assert--There are four requisites, therefore 4 warnings for this section
                var plannedCourses = degreePlan.NonTermPlannedCourses;
                var pc1 = plannedCourses.Where(pc => pc.CourseId == "7704").First();
                Assert.AreEqual(4, pc1.Warnings.Count());
                // Check on each specific expected warning. We expect a warning for every requisite
                foreach (var req in sec1.Requisites)
                {
                    if (!string.IsNullOrEmpty(req.CorequisiteCourseId))
                    {
                        // have a warning for each of the two requisite courses (pre-conversion only). Always has an UnmetRequisite type
                        var warning = pc1.Warnings.Where(w => w.Requisite != null && w.Requisite.CorequisiteCourseId == req.CorequisiteCourseId).First();
                        Assert.IsTrue(warning.Type == PlannedCourseWarningType.UnmetRequisite);
                    }
                }
                foreach (var req in sec1.SectionRequisites)
                {
                    // may be one of two requisite sections containing req.corequisiteSectionIds
                    var warning = pc1.Warnings.Where(w => w.SectionRequisite != null && w.SectionRequisite.CorequisiteSectionIds.Contains(req.CorequisiteSectionIds.First())).First();
                    if (req.IsRequired || req.NumberNeeded > 0)
                    {
                        // Verify warning type and with section Id present single required req (because number required == number of required sections)
                        Assert.AreEqual(PlannedCourseWarningType.UnmetRequisite, warning.Type);
                        Assert.AreEqual(warning.SectionRequisite.CorequisiteSectionIds.ElementAt(0), warning.SectionId);
                    }
                    else
                    {
                        // Verify warning type with section Id present in warning for recommended reqs.
                        Assert.AreEqual(PlannedCourseWarningType.UnmetRequisite, warning.Type);
                        Assert.IsTrue(!string.IsNullOrEmpty(warning.SectionId));
                        Assert.IsTrue(req.CorequisiteSectionIds.Contains(warning.SectionId));
                    }

                }
            }

            [TestMethod]
            public void NoMessageWhenNonTermSectionCoreqsPlannedInTerms()
            {
                // Arrange--Add course 7706, non-term section 04 to this degree plan
                var sec1 = allSections.Where(s => s.CourseId == "7704" && s.Number == "04").First();
                // For a non-term section it MUST have start and end date for the requiste checking to work. 
                sec1.EndDate = new DateTime(2012, 12, 10);
                plannedCourse1 = new PlannedCourse("7704", sec1.Id);
                degreePlan.AddCourse(plannedCourse1, "");   // Since this section has no term, added to non-term planned courses
                // Add required course coreq to planned term courses
                var coreqSectionRequisites = sec1.SectionRequisites.Where(r => r.CorequisiteSectionIds != null);
                var coreqSec = allSections.Where(s => s.Id == coreqSectionRequisites.ElementAt(0).CorequisiteSectionIds.ElementAt(0)).FirstOrDefault();
                plannedCourse2 = new PlannedCourse(coreqSec.CourseId, coreqSec.Id);
                degreePlan.AddCourse(plannedCourse2, "2012/FA");
                coreqSec = allSections.Where(s => s.Id == coreqSectionRequisites.ElementAt(1).CorequisiteSectionIds.ElementAt(0)).FirstOrDefault();
                plannedCourse3 = new PlannedCourse(coreqSec.CourseId, coreqSec.Id);
                degreePlan.AddCourse(plannedCourse3, "2012/FA");
                var plannedCourse4 = new PlannedCourse("42", "");
                degreePlan.AddCourse(plannedCourse4, "2011/FA");
                var plannedCourse5 = new PlannedCourse("21", "");
                degreePlan.AddCourse(plannedCourse5, "2011/FA");

                degreePlan.AddCourse(new PlannedCourse(course: null, coursePlaceholder: "abc"), "2012/FA");

                // Act--Check for conflicts
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, credits, requirements, RuleResults);

                // Assert--There are no conflicts found because all coreqs are planned
                var plannedCourses = degreePlan.NonTermPlannedCourses;
                var pc1 = plannedCourses.Where(pc => pc.CourseId == "7704").First();
                Assert.AreEqual(0, pc1.Warnings.Count());
            }

            [TestMethod]
            public void NoMessageWhenNonTermSectionCoreqsPlanned_NoEndDate()
            {
                // Arrange--Add course 7706, non-term section 04 to this degree plan
                var sec1 = allSections.Where(s => s.CourseId == "7704" && s.Number == "04").First();

                // NonTerm SECTION DOESN'T HAVE AN END DATE - SINCE THERE ARE PLANNED ITEMS - DATES DON'T MATTER - CONSIDER IT MET. 

                plannedCourse1 = new PlannedCourse("7704", sec1.Id);
                degreePlan.AddCourse(plannedCourse1, "");   // Since this section has no term, added to non-term planned courses
                // Add required course coreq to planned term courses
                var coreqSectionRequisites = sec1.SectionRequisites.Where(r => r.CorequisiteSectionIds != null);
                var coreqSec = allSections.Where(s => s.Id == coreqSectionRequisites.ElementAt(0).CorequisiteSectionIds.ElementAt(0)).FirstOrDefault();
                plannedCourse2 = new PlannedCourse(coreqSec.CourseId, coreqSec.Id);
                degreePlan.AddCourse(plannedCourse2, "2012/FA");
                coreqSec = allSections.Where(s => s.Id == coreqSectionRequisites.ElementAt(1).CorequisiteSectionIds.ElementAt(0)).FirstOrDefault();
                plannedCourse3 = new PlannedCourse(coreqSec.CourseId, coreqSec.Id);
                degreePlan.AddCourse(plannedCourse3, "2012/FA");
                var plannedCourse4 = new PlannedCourse("42", "");
                degreePlan.AddCourse(plannedCourse4, "2011/FA");
                var plannedCourse5 = new PlannedCourse("21", "");
                degreePlan.AddCourse(plannedCourse5, "2011/FA");

                degreePlan.AddCourse(new PlannedCourse(course: null, coursePlaceholder: "abc"), "2012/FA");

                // Act--Check for conflicts
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, credits, requirements, RuleResults);

                // Assert--There are no conflicts found because all coreqs are planned
                var plannedCourses = degreePlan.NonTermPlannedCourses;
                var pc1 = plannedCourses.Where(pc => pc.CourseId == "7704").First();
                Assert.AreEqual(0, pc1.Warnings.Count());
            }

            [TestMethod]
            public void MessageWhenNonTermSectionNonTermCoreqIsNotPlanned()
            {
                // Arrange--Add course 7706, non-term section 04 to this degree plan. This section requires course 7701, section 04 (nonterm)
                var sec1 = allSections.Where(s => s.CourseId == "7706" && s.Number == "04").First();
                plannedCourse1 = new PlannedCourse("7706", sec1.Id);
                degreePlan.AddCourse(plannedCourse1, "");   // Since this section has no term, added to non-term planned courses

                degreePlan.AddCourse(new PlannedCourse(course: null, coursePlaceholder: "abc"), "2012/FA");

                // Act--Check for conflicts
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, credits, requirements, RuleResults);

                // Assert--There are no conflicts found because required nonterm section is not planned
                var plannedCourses = degreePlan.NonTermPlannedCourses;
                var pc1 = plannedCourses.Where(pc => pc.CourseId == "7706").First();
                Assert.AreEqual(1, pc1.Warnings.Count());
                var sectionRequisite = sec1.SectionRequisites.Where(r => r.CorequisiteSectionIds != null).First();
                var coreqSection = allSections.Where(s => s.Id == sectionRequisite.CorequisiteSectionIds.ElementAt(0)).FirstOrDefault();
                Assert.AreEqual(PlannedCourseWarningType.UnmetRequisite, pc1.Warnings.ElementAt(0).Type);
            }

            [TestMethod]
            public void NoMessageWhenNonTermSectionNonTermCoreqIsPlanned()
            {
                // Arrange--Add course 7706, non-term section 04 to this degree plan
                var sec1 = allSections.Where(s => s.CourseId == "7706" && s.Number == "04").First();
                plannedCourse1 = new PlannedCourse("7706", sec1.Id);
                degreePlan.AddCourse(plannedCourse1, "");   // No term == added as nonterm courseSection
                var sec2 = allSections.Where(s => s.CourseId == "7701" && s.Number == "04").First();
                plannedCourse2 = new PlannedCourse("7701", sec2.Id);
                degreePlan.AddCourse(plannedCourse2, "");   // No term == added as nonterm courseSection

                degreePlan.AddCourse(new PlannedCourse(course: null, coursePlaceholder: "abc"), "2012/FA");

                // Act--Check for conflicts
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, credits, requirements, RuleResults);

                // Assert--There are no conflicts found because required nonterm section is not planned
                var plannedCourses = degreePlan.NonTermPlannedCourses;
                var pc1 = plannedCourses.Where(pc => pc.CourseId == "7706").First();
                Assert.AreEqual(0, pc1.Warnings.Count());
            }

            [TestMethod]
            public void NoMessageWhenNonTermSectionNonTermCoreqIsInAcadCredits()
            {
                // Arrange--Add course 7706, non-term section 04 to this degree plan
                var sec1 = allSections.Where(s => s.CourseId == "7706" && s.Number == "04").First();
                plannedCourse1 = new PlannedCourse("7706", sec1.Id);
                degreePlan.AddCourse(plannedCourse1, "");   // No term == added as nonterm courseSection
                var sec2 = allSections.Where(s => s.CourseId == "7701" && s.Number == "04").First();
                var crs2 = allCourses.Where(c => c.Id == sec2.CourseId).First();
                var acadCredit1 = new AcademicCredit("01", crs2, sec2.Id);
                var acadCredits = new List<AcademicCredit>() { acadCredit1 };
                degreePlan.AddCourse(plannedCourse2, "");   // No term == added as nonterm courseSection

                degreePlan.AddCourse(new PlannedCourse(course: null, coursePlaceholder: "abc"), "2012/FA");

                // Act--Check for conflicts
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, acadCredits, requirements, RuleResults);

                // Assert--There are no conflicts found because required nonterm section is not planned
                var plannedCourses = degreePlan.NonTermPlannedCourses;
                var pc1 = plannedCourses.Where(pc => pc.CourseId == "7706").First();
                Assert.AreEqual(0, pc1.Warnings.Count());
            }

            [TestMethod]
            public void NoMessageWhenNonTermSectionNonTermCoreqStartsLater()
            {
                // Arrange--Add course 7706, non-term section 05 to this degree plan. Coreq Section DENT 101-05
                // set up specifically with a start date later, but message will still not display because 
                // we need to assume a section set up with a coreq section was done so intentionally and dates don't matter.
                var sec1 = allSections.Where(s => s.CourseId == "7706" && s.Number == "05").First();
                plannedCourse1 = new PlannedCourse("7706", sec1.Id);
                degreePlan.AddCourse(plannedCourse1, "");   // No term == added as nonterm courseSection
                var sec2 = allSections.Where(s => s.CourseId == "7701" && s.Number == "05").First();
                plannedCourse2 = new PlannedCourse("7701", sec2.Id);
                degreePlan.AddCourse(plannedCourse2, "");   // No term == added as nonterm courseSection

                degreePlan.AddCourse(new PlannedCourse(course: null, coursePlaceholder: "abc"), "2012/FA");

                // Act--Check for conflicts
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, credits, requirements, RuleResults);

                // Assert--There are no messages even though nonterm section 7701 dates are later than dates on requiring section
                var plannedCourses = degreePlan.NonTermPlannedCourses;
                var pc1 = plannedCourses.Where(pc => pc.CourseId == "7706").First();
                Assert.AreEqual(0, pc1.Warnings.Count());
            }

            [TestMethod]
            public void MessageWhenTermSectionNonTermCoreqIsNotPlanned()
            {
                // Arrange--Add term-based section DENT 105 to term 2013/SP. It calls for optional nonterm coreq DENT 104 04 and course coreq DENT 101
                var sec1 = allSections.Where(s => s.CourseId == "7705" && s.TermId == "2013/SP").First();
                plannedCourse1 = new PlannedCourse("7705", sec1.Id);
                degreePlan.AddCourse(plannedCourse1, "2013/SP"); // This is a term-based section

                degreePlan.AddCourse(new PlannedCourse(course: null, coursePlaceholder: "abc"), "2012/FA");

                // Act--Check for conflicts
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, credits, requirements, RuleResults);

                // Assert--Two Messages expected
                var plannedCourses = degreePlan.GetPlannedCourses("2013/SP");
                var pc1 = plannedCourses.Where(pc => pc.CourseId == "7705").First();
                Assert.AreEqual(2, pc1.Warnings.Count());
                // Find the expected warning for the missing required requisite section
                var warning = pc1.Warnings.Where(w => w.SectionRequisite != null && w.SectionRequisite.CorequisiteSectionIds != null).First();
                Assert.AreEqual(PlannedCourseWarningType.UnmetRequisite, warning.Type);
                // Find the expected warning for the missing course requisite (pre-conversion only)
                warning = pc1.Warnings.Where(w => w.Requisite != null && !(string.IsNullOrEmpty(w.Requisite.CorequisiteCourseId))).First();
                Assert.AreEqual(PlannedCourseWarningType.UnmetRequisite, warning.Type);
            }

            [TestMethod]
            public void NoMessageWhenTermSectionNonTermCoreqIsPlanned()
            {
                // Arrange--Add term-based section DENT 105 to term 2013/SP. It calls for optional nonterm coreq DENT 104 04
                var sec1 = allSections.Where(s => s.CourseId == "7705" && s.TermId == "2013/SP").First();
                plannedCourse1 = new PlannedCourse("7705", sec1.Id);
                degreePlan.AddCourse(plannedCourse1, "2013/SP"); // This is a term-based section
                // Get the nonterm section for course DENT 104 to add to plan, non-term
                var sec2 = allSections.Where(s => s.CourseId == "7704" && s.Number == "04").First();
                degreePlan.AddCourse(new PlannedCourse("7704", sec2.Id), "");
                // Get the nonterm section for course DENT 101 and add it to the plan, non-term
                var sec3 = allSections.Where(s => s.CourseId == "7701" && s.Number == "04").First();
                degreePlan.AddCourse(new PlannedCourse("7701", sec3.Id), "");

                degreePlan.AddCourse(new PlannedCourse(course: null, coursePlaceholder: "abc"), "2012/FA");

                // Act--Check for conflicts
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, credits, requirements, RuleResults);

                // Assert--Message found because required nonterm coreq has not been planned
                var plannedCourses = degreePlan.GetPlannedCourses("2013/SP");
                var pc1 = plannedCourses.Where(pc => pc.CourseId == "7705").First();
                Assert.AreEqual(0, pc1.Warnings.Count());
            }


            [TestMethod]
            public void NoMessageWhenTermSectionNonTermCoreqStartsLater()
            {
                // Arrange--Add term-based section DENT 105 to term 2012/FA. It calls for optional nonterm coreq DENT 104 04
                // DENT 104 04 has been set up specifically with a date that starts later than the beginning of 2012/FA, but
                // still no warning will appear because we will not question the timing of section coreqs set up for sections.
                var sec1 = allSections.Where(s => s.CourseId == "7705" && s.TermId == "2012/FA").First();
                plannedCourse1 = new PlannedCourse("7705", sec1.Id);
                degreePlan.AddCourse(plannedCourse1, "2012/FA"); // This is a term-based section, added for 2012/fa term
                var sec2 = allSections.Where(s => s.CourseId == "7704" && s.Number == "04").First();
                plannedCourse2 = new PlannedCourse("7704", sec2.Id);
                degreePlan.AddCourse(plannedCourse2, ""); // This is a non term-based section with start date later than 2012/fa term start date
                // Get the nonterm section for course DENT 101 and add it to the plan, non-term
                var sec3 = allSections.Where(s => s.CourseId == "7701" && s.Number == "04").First();
                degreePlan.AddCourse(new PlannedCourse("7701", sec3.Id), ""); // Nonterm, but start date will compare favorably with 2012/fa

                degreePlan.AddCourse(new PlannedCourse(course: null, coursePlaceholder: "abc"), "2012/FA");

                // Act--Check for conflicts
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, credits, requirements, RuleResults);

                // Assert--No Message even though required nonterm coreq has a start date later than the planned course.
                var plannedCourses = degreePlan.GetPlannedCourses("2012/FA");
                var pc1 = plannedCourses.Where(pc => pc.CourseId == "7705").First();
                Assert.AreEqual(0, pc1.Warnings.Count());
            }

            [TestMethod]
            public void NoMessageWhenEquateIsPlanned()
            {
                // Course 7702 2012/FA section has two course coreqs:
                //  42 (HIST-200) Required
                //  21 (BIOL-200) Not Required

                // Arrange--Set up degree plan, do not add either corequisite course
                var sec1 = allSections.Where(s => s.CourseId == "7702" && s.TermId == "2012/FA").First();
                plannedCourse1 = new PlannedCourse("7702", sec1.Id);
                degreePlan.AddCourse(plannedCourse1, "2012/FA");
                // Add equate course POLI 100 (id 155) to plan
                plannedCourse2 = new PlannedCourse("155");
                degreePlan.AddCourse(plannedCourse2, "2012/FA");
                degreePlan.AddCourse(new PlannedCourse(course: null, coursePlaceholder: "abc"), "2012/FA");

                // Act--Check for conflicts
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, credits, requirements, RuleResults);

                // Assert--Messages found for only the one missing unplanned corequisite course
                var plannedCourses = degreePlan.GetPlannedCourses("2012/FA");
                var pc1 = plannedCourses.Where(pc => pc.CourseId == "7702").First();
                Assert.AreEqual(1, pc1.Warnings.Count());
                Assert.AreEqual("21", pc1.Warnings.ElementAt(0).Requisite.CorequisiteCourseId);
                Assert.AreEqual(PlannedCourseWarningType.UnmetRequisite, pc1.Warnings.ElementAt(0).Type);
                Assert.IsFalse(pc1.Warnings.ElementAt(0).Requisite.IsRequired);
                Assert.AreEqual(RequisiteCompletionOrder.PreviousOrConcurrent, pc1.Warnings.ElementAt(0).Requisite.CompletionOrder);
            }

            public void NoMessageWhenEquateInAcademicCredits()
            {
                // Course 7702 2012/FA section has two course coreqs:
                //  42 (HIST-200) Required
                //  21 (BIOL-200) Not Required

                // Arrange--Set up degree plan, do not add either corequisite course
                var sec1 = allSections.Where(s => s.CourseId == "7702" && s.TermId == "2012/FA").First();
                plannedCourse1 = new PlannedCourse("7702", sec1.Id);
                degreePlan.AddCourse(plannedCourse1, "2012/FA");
                // Add equate course POLI 100 (id 155) to academic credits
                var acadCourse = allCourses.First(c => c.Id == "155");
                var acadSectionId = allSections.Where(s => s.CourseId == "155" && s.TermId == "2012/FA").Select(s => s.Id).First();
                var academicCredit = new AcademicCredit("01", acadCourse, acadSectionId);
                var acadCredits = credits.ToList();
                acadCredits.Add(academicCredit);

                degreePlan.AddCourse(new PlannedCourse(course: null, coursePlaceholder: "abc"), "2012/FA");

                // Act--Check for conflicts
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, acadCredits, requirements, RuleResults);

                // Assert--Message found for only the one unplanned missing corequisite course
                var plannedCourses = degreePlan.GetPlannedCourses("2012/FA");
                var pc1 = plannedCourses.Where(pc => pc.CourseId == "7702").First();
                Assert.AreEqual(1, pc1.Warnings.Count());
                Assert.AreEqual("21", pc1.Warnings.ElementAt(0).Requisite.CorequisiteCourseId);
                Assert.AreEqual(PlannedCourseWarningType.UnmetRequisite, pc1.Warnings.ElementAt(0).Type);
                Assert.IsFalse(pc1.Warnings.ElementAt(0).Requisite.IsRequired);
                Assert.AreEqual(RequisiteCompletionOrder.PreviousOrConcurrent, pc1.Warnings.ElementAt(0).Requisite.CompletionOrder);
            }

            [TestMethod]
            public void MessageWhenTermSectionNonTermCoreqEquateIsPlanned()
            {
                // Arrange--Add term-based section DENT 105 to term 2013/SP. It calls for optional nonterm coreq DENT 104 04
                var sec1 = allSections.Where(s => s.CourseId == "7705" && s.TermId == "2013/SP").First();
                plannedCourse1 = new PlannedCourse("7705", sec1.Id);
                degreePlan.AddCourse(plannedCourse1, "2013/SP"); // This is a term-based section
                // Add non-term section to the plan that is an equate of the coreq DENT 104
                var sec3 = allSections.Where(s => s.CourseId == "7703" && s.TermId == "2013/SP").First();
                plannedCourse2 = new PlannedCourse("7703", sec3.Id);
                degreePlan.AddCourse(plannedCourse2, "");

                degreePlan.AddCourse(new PlannedCourse(course: null, coursePlaceholder: "abc"), "2012/FA");

                // Act--Check for conflicts
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, credits, requirements, RuleResults);

                // Assert--Message found because required nonterm section has not been planned, equated planned course will not suffice
                var plannedCourses = degreePlan.GetPlannedCourses("2013/SP");
                var pc1 = plannedCourses.Where(pc => pc.CourseId == "7705").First();
                Assert.AreEqual(1, pc1.Warnings.Count());
                Assert.AreEqual(PlannedCourseWarningType.UnmetRequisite, pc1.Warnings.ElementAt(0).Type);
            }
        }

        [TestClass]
        public class DegreePlanValidateCredits
        {
            string personId;
            int degreePlanId;
            DegreePlan degreePlan;
            List<Section> sections = new List<Section>();
            IEnumerable<Term> regTerms = new List<Term>();
            IEnumerable<Term> allTerms = new List<Term>();
            IEnumerable<Course> allCourses = new List<Course>();
            IEnumerable<AcademicCredit> credits;
            IEnumerable<Requirement> requirements = new List<Requirement>();
            IEnumerable<RuleResult> RuleResults;
            ILogger logger;

            [TestInitialize]
            public async void Initialize()
            {
                personId = "00004002";
                degreePlanId = 1;
                degreePlan = new DegreePlan(degreePlanId, personId, 1);
                degreePlan.AddTerm("2012/FA");
                degreePlan.AddTerm("2013/SP");

                regTerms = new TestTermRepository().Get().Where(t => t.Code == "2012/FA");
                var student = new TestStudentRepository().Get(personId);
                credits = await new TestAcademicCreditRepository().GetAsync(student.AcademicCreditIds);
                // Create some test sections
                var statuses = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddYears(-6)) };
                // Sections reference real courses from course repo
                // Section with min 3.0 only (formerly course Id 111)
                Section sec1 = new Section("11111", "130", "01", new DateTime(2012, 09, 01), 3.0m, null, "Introduction to Art", "IN", new List<OfferingDepartment>() { new OfferingDepartment("ART", 100m) }, new List<string>() { "UG" }, "UG", statuses);
                sections.Add(sec1);
                // Section with min 3.0 and max 6.0 (former course Id 222)
                Section sec2 = new Section("22222", "93", "02", new DateTime(2012, 09, 01), 3.0m, null, "Introduction to Art", "IN", new List<OfferingDepartment>() { new OfferingDepartment("ART", 100m) }, new List<string>() { "UG" }, "UG", statuses);
                sec2.MaximumCredits = 6.0m;
                sections.Add(sec2);
                // Section with no min or max
                Section sec3 = new Section("33333", "78", "03", new DateTime(2012, 09, 01), null, 2.0m, "Introduction to Art", "IN", new List<OfferingDepartment>() { new OfferingDepartment("ART", 100m) }, new List<string>() { "UG" }, "UG", statuses);
                sections.Add(sec3);
                // Section with min 2 and max 9 and increment 2 (2, 4, 6, 8)
                Section sec4 = new Section("44444", "46", "04", new DateTime(2012, 09, 01), 2.0m, null, "Introduction to Art", "IN", new List<OfferingDepartment>() { new OfferingDepartment("ART", 100m) }, new List<string>() { "UG" }, "UG", statuses);
                sec4.MaximumCredits = 9.0m;
                sec4.VariableCreditIncrement = 2;
                sections.Add(sec4);
                // Section with no min but a max
                Section sec5 = new Section("55555", "20", "05", new DateTime(2012, 09, 01), null, 2.0m, "Introduction to Art", "IN", new List<OfferingDepartment>() { new OfferingDepartment("ART", 100m) }, new List<string>() { "UG" }, "UG", statuses);
                sec5.MaximumCredits = 6.0m;
                sections.Add(sec5);
                RuleResults = new List<RuleResult>();
                logger = new Mock<ILogger>().Object;
            }

            [TestCleanup]
            public void CleanUp()
            {

            }

            [TestMethod]
            public void NullPlannedCredits()
            {
                PlannedCourse pc = new PlannedCourse("130", "11111");
                degreePlan.AddCourse(pc, "2012/FA");
                degreePlan.AddCourse(new PlannedCourse(course: null, coursePlaceholder: "abc"), "2012/FA");

                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, sections, credits, requirements, RuleResults);
                PlannedCourse updatedpc = degreePlan.GetPlannedCourses("2012/FA").ElementAt(0);
                Assert.AreEqual(0, updatedpc.Warnings.Count());
            }

            [TestMethod]
            public void NegativePlannedCredits()
            {
                PlannedCourse pc = new PlannedCourse("93", "11111");
                pc.Credits = -1.0m;
                degreePlan.AddCourse(pc, "2012/FA");
                degreePlan.AddCourse(new PlannedCourse(course: null, coursePlaceholder: "abc"), "2012/FA");

                //IEnumerable<PlannedCourseWarning> warnings = degreePlan.Validate(sections);
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, sections, credits, requirements, RuleResults);
                PlannedCourse updatedpc = degreePlan.GetPlannedCourses("2012/FA").ElementAt(0);
                Assert.AreEqual(1, updatedpc.Warnings.Count());
                Assert.AreEqual(PlannedCourseWarningType.InvalidPlannedCredits, updatedpc.Warnings.ElementAt(0).Type);
            }

            [TestMethod]
            public void PlannedCourse_NoSectionId()
            {
                PlannedCourse pc = new PlannedCourse("130", null);
                pc.Credits = 44.0m;
                degreePlan.AddCourse(pc, "2012/FA");
                degreePlan.AddCourse(new PlannedCourse(course: null, coursePlaceholder: "abc"), "2012/FA");

                //IEnumerable<PlannedCourseWarning> warnings = degreePlan.Validate(sections);
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, sections, credits, requirements, RuleResults);
                PlannedCourse updatedpc = degreePlan.GetPlannedCourses("2012/FA").ElementAt(0);
                Assert.AreEqual(0, updatedpc.Warnings.Count());
            }

            [TestMethod]
            public void PlannedCourse_SectionWithNoMinOrMax()
            {
                PlannedCourse pc = new PlannedCourse("78", "33333");
                pc.Credits = 44.0m;
                degreePlan.AddCourse(pc, "2012/FA");
                degreePlan.AddCourse(new PlannedCourse(course: null, coursePlaceholder: "abc"), "2012/FA");

                //IEnumerable<PlannedCourseWarning> warnings = degreePlan.Validate(sections);
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, sections, credits, requirements, RuleResults);
                PlannedCourse updatedpc = degreePlan.GetPlannedCourses("2012/FA").ElementAt(0);
                Assert.AreEqual(0, updatedpc.Warnings.Count());
            }

            [TestMethod]
            public void FixedSectionCredits_PlannedCreditsMatch()
            {
                PlannedCourse pc = new PlannedCourse("130", "11111");
                pc.Credits = 3.0m;
                degreePlan.AddCourse(pc, "2012/FA");
                degreePlan.AddCourse(new PlannedCourse(course: null, coursePlaceholder: "abc"), "2012/FA");

                //IEnumerable<PlannedCourseWarning> warningss = degreePlan.Validate(sections);
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, sections, credits, requirements, RuleResults);
                PlannedCourse updatedpc = degreePlan.GetPlannedCourses("2012/FA").ElementAt(0);
                Assert.AreEqual(0, updatedpc.Warnings.Count());
            }

            [TestMethod]
            public void FixedSectionCredits_PlannedCreditsDifferent()
            {
                PlannedCourse pc = new PlannedCourse("130", "11111");
                pc.Credits = 4.0m;
                degreePlan.AddCourse(pc, "2012/FA");
                degreePlan.AddCourse(new PlannedCourse(course: null, coursePlaceholder: "abc"), "2012/FA");

                //IEnumerable<PlannedCourseWarning> warnings = degreePlan.Validate(sections);
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, sections, credits, requirements, RuleResults);
                PlannedCourse updatedpc = degreePlan.GetPlannedCourses("2012/FA").ElementAt(0);
                Assert.AreEqual(1, updatedpc.Warnings.Count());
                Assert.AreEqual(PlannedCourseWarningType.InvalidPlannedCredits, updatedpc.Warnings.ElementAt(0).Type);
            }

            [TestMethod]
            public void SectionMaxCreditsOnly_PlannedCreditsLess()
            {
                PlannedCourse pc = new PlannedCourse("20", "55555");
                pc.Credits = 4.0m;
                degreePlan.AddCourse(pc, "2012/FA");
                degreePlan.AddCourse(new PlannedCourse(course: null, coursePlaceholder: "abc"), "2012/FA");

                //IEnumerable<PlannedCourseWarning> warnings = degreePlan.Validate(sections);
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, sections, credits, requirements, RuleResults);
                PlannedCourse updatedpc = degreePlan.GetPlannedCourses("2012/FA").ElementAt(0);
                Assert.AreEqual(0, updatedpc.Warnings.Count());
            }

            [TestMethod]
            public void SectionMaxCreditsOnly_PlannedCreditsMore()
            {
                PlannedCourse pc = new PlannedCourse("20", "55555");
                pc.Credits = 7.0m;
                degreePlan.AddCourse(pc, "2012/FA");
                degreePlan.AddCourse(new PlannedCourse(course: null, coursePlaceholder: "abc"), "2012/FA");

                //IEnumerable<PlannedCourseWarning> warnings = degreePlan.Validate(sections);
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, sections, credits, requirements, RuleResults);
                PlannedCourse updatedpc = degreePlan.GetPlannedCourses("2012/FA").ElementAt(0);
                Assert.AreEqual(1, updatedpc.Warnings.Count());
                Assert.AreEqual(PlannedCourseWarningType.InvalidPlannedCredits, updatedpc.Warnings.ElementAt(0).Type);
            }

            [TestMethod]
            public void SectionVariableCredits_InRange()
            {
                PlannedCourse pc = new PlannedCourse("93", "22222");
                pc.Credits = 5.0m;
                degreePlan.AddCourse(pc, "2012/FA");
                degreePlan.AddCourse(new PlannedCourse(course: null, coursePlaceholder: "abc"), "2012/FA");

                //IEnumerable<PlannedCourseWarning> warnings = degreePlan.Validate(sections);
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, sections, credits, requirements, RuleResults);
                PlannedCourse updatedpc = degreePlan.GetPlannedCourses("2012/FA").ElementAt(0);
                Assert.AreEqual(0, updatedpc.Warnings.Count());
            }

            [TestMethod]
            public void SectionVariableCredits_OutsideRange()
            {
                PlannedCourse pc = new PlannedCourse("93", "22222");
                pc.Credits = 7.0m;
                degreePlan.AddCourse(pc, "2012/FA");
                degreePlan.AddCourse(new PlannedCourse(course: null, coursePlaceholder: "abc"), "2012/FA");

                //IEnumerable<PlannedCourseWarning> warnings = degreePlan.Validate(sections);
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, sections, credits, requirements, RuleResults);
                PlannedCourse updatedpc = degreePlan.GetPlannedCourses("2012/FA").ElementAt(0);
                Assert.AreEqual(1, updatedpc.Warnings.Count());
                Assert.AreEqual(PlannedCourseWarningType.InvalidPlannedCredits, updatedpc.Warnings.ElementAt(0).Type);
            }
            [TestMethod]
            public void SectionVariableCredits_OutsideRangeLow()
            {
                PlannedCourse pc = new PlannedCourse("93", "22222");
                pc.Credits = 2.0m;
                degreePlan.AddCourse(pc, "2012/FA");
                degreePlan.AddCourse(new PlannedCourse(course: null, coursePlaceholder: "abc"), "2012/FA");

                //IEnumerable<DegreePlanMessage> messages = degreePlan.Validate(sections);
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, sections, credits, requirements, RuleResults);
                PlannedCourse updatedpc = degreePlan.GetPlannedCourses("2012/FA").ElementAt(0);
                Assert.AreEqual(1, updatedpc.Warnings.Count());
                Assert.AreEqual(PlannedCourseWarningType.InvalidPlannedCredits, updatedpc.Warnings.ElementAt(0).Type);
            }

            [TestMethod]
            public void SectionVariableCreditsWithInc_Matches()
            {
                PlannedCourse pc = new PlannedCourse("46", "44444");
                pc.Credits = 4.0m;
                degreePlan.AddCourse(pc, "2012/FA");
                degreePlan.AddCourse(new PlannedCourse(course: null, coursePlaceholder: "abc"), "2012/FA");

                //IEnumerable<PlannedCourseWarning> warnings = degreePlan.Validate(sections);
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, sections, credits, requirements, RuleResults);
                PlannedCourse updatedpc = degreePlan.GetPlannedCourses("2012/FA").ElementAt(0);
                Assert.AreEqual(0, updatedpc.Warnings.Count());
            }

            [TestMethod]
            public void SectionVariableCreditsWithInc_NoMatch()
            {
                PlannedCourse pc = new PlannedCourse("46", "44444");
                pc.Credits = 4.5m;
                degreePlan.AddCourse(pc, "2012/FA");
                degreePlan.AddCourse(new PlannedCourse(course: null, coursePlaceholder: "abc"), "2012/FA");

                //IEnumerable<PlannedCourseWarning> warnings = degreePlan.Validate(sections);
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, sections, credits, requirements, RuleResults);
                PlannedCourse updatedpc = degreePlan.GetPlannedCourses("2012/FA").ElementAt(0);
                Assert.AreEqual(1, updatedpc.Warnings.Count());
                Assert.AreEqual(PlannedCourseWarningType.InvalidPlannedCredits, updatedpc.Warnings.ElementAt(0).Type);
            }
        }

        [TestClass]
        public class DegreePlanCheckTimeConflicts
        {

            private string personId;
            private DegreePlan degreePlan;
            private PlannedCourse plannedCourse1;
            private PlannedCourse plannedCourse2;
            private PlannedCourse plannedCourse3;
            List<Course> allCourses;
            List<Section> allSections;
            List<Term> allTerms;
            List<Term> regTerms;
            IEnumerable<AcademicCredit> credits;
            List<Requirement> requirements = new List<Requirement>();
            IEnumerable<RuleResult> RuleResults;
            ILogger logger;

            [TestInitialize]
            public async void Initialize()
            {
                personId = "00004002";
                degreePlan = new DegreePlan(personId);

                allCourses = (await new TestCourseRepository().GetAsync()).ToList();
                allTerms = new TestTermRepository().Get().ToList();
                regTerms = (await new TestTermRepository().GetRegistrationTermsAsync()).ToList();
                allSections = (await new TestSectionRepository().GetRegistrationSectionsAsync(regTerms)).ToList();
                var student = new TestStudentRepository().Get(personId);
                credits = await new TestAcademicCreditRepository().GetAsync(student.AcademicCreditIds);
                RuleResults = new List<RuleResult>();
                logger = new Mock<ILogger>().Object;
            }

            [TestCleanup]
            public async void Cleanup()
            {
                degreePlan = null;
            }

            [TestMethod]
            public void LogicGracefullyHandlesNoCoursesPlanned()
            {
                // Act -- Check conflicts
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, credits, requirements, RuleResults);

                // Assert -- No time conflict messages reported
                foreach (var termId in degreePlan.TermIds)
                {
                    var plannedCourses = degreePlan.GetPlannedCourses(termId);
                    var messages = plannedCourses.SelectMany(pc => pc.Warnings).Where(m => m.Type == PlannedCourseWarningType.TimeConflict);
                    Assert.IsTrue(messages.Count() == 0);
                }
            }

            [TestMethod]
            public void NoMessageWhenNoMeetingsOnEitherSection()
            {
                // Arrange--Add two sections to a degree plan, neither one has meeting information.
                var termId = regTerms.ElementAt(0).Code;
                var courseId1 = "7701";
                var section1 = allSections.Where(s => s.CourseId == courseId1 && s.TermId == termId && s.Number == "01").First();
                // Make sure this section has no meetings
                Assert.IsTrue(section1.Meetings.Count() == 0);
                plannedCourse1 = new PlannedCourse(course: courseId1, section: section1.Id, coursePlaceholder: null);
                degreePlan.AddCourse(plannedCourse1, termId);
                var courseId2 = "7704";
                var section2 = allSections.Where(s => s.CourseId == courseId2 && s.Number == "04").First();
                // Make sure this section has no meetings
                Assert.IsTrue(section2.Meetings.Count() == 0);
                plannedCourse2 = new PlannedCourse(course: courseId2, section: section2.Id, coursePlaceholder: null);
                degreePlan.AddCourse(plannedCourse2, ""); // add as non-term to test that sections are retrieved from both term and nonterm

                // Act -- Check conflicts
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, credits, requirements, RuleResults);

                // Assert -- No time conflict messages reported
                foreach (var termCode in degreePlan.TermIds)
                {
                    var plannedCourses = degreePlan.GetPlannedCourses(termCode);
                    var messages = plannedCourses.SelectMany(pc => pc.Warnings).Where(m => m.Type == PlannedCourseWarningType.TimeConflict);
                    Assert.IsTrue(messages.Count() == 0);
                }
            }

            [TestMethod]
            public void NoMessageWhenMeetingsNotOnBothSections()
            {
                // Arrange--Add two sections to a degree plan, only the second one has meeting information.
                var termId = regTerms.ElementAt(0).Code;
                var courseId1 = "7701";
                var section1 = allSections.Where(s => s.CourseId == courseId1 && s.TermId == termId && s.Number == "01").First();
                Assert.IsTrue(section1.Meetings.Count() == 0);
                plannedCourse1 = new PlannedCourse(courseId1, section1.Id);
                degreePlan.AddCourse(plannedCourse1, termId);
                var courseId2 = "7704";
                var section2 = allSections.Where(s => s.CourseId == courseId2 && s.Number == "01").First();
                Assert.IsTrue(section2.Meetings.Count() > 0);
                Assert.IsTrue(section2.Meetings.ElementAt(0).StartDate != null);
                plannedCourse2 = new PlannedCourse(courseId2, section2.Id);
                degreePlan.AddCourse(plannedCourse2, ""); // add as non-term to test that sections are retrieved from both term and nonterm

                // Act -- Check conflicts
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, credits, requirements, RuleResults);

                // Assert -- No time conflict messages reported
                foreach (var termCode in degreePlan.TermIds)
                {
                    var plannedCourses = degreePlan.GetPlannedCourses(termCode);
                    var messages = plannedCourses.SelectMany(pc => pc.Warnings).Where(m => m.Type == PlannedCourseWarningType.TimeConflict);
                    Assert.IsTrue(messages.Count() == 0);
                }
            }

            [TestMethod]
            public void MessageWhenTermsAreSameAndMeetingTimesOverlap()
            {
                // Arrange--Add two sections to a degree plan, both with same term and same meeting times.
                var termId = regTerms.ElementAt(0).Code;
                var courseId1 = "7702";
                var section1 = allSections.Where(s => s.CourseId == courseId1 && s.TermId == termId && s.Number == "01").First();
                Assert.IsTrue(section1.Meetings.Count() > 0);
                Assert.IsTrue(section1.Meetings.ElementAt(0).StartDate != null);
                plannedCourse1 = new PlannedCourse(courseId1, section1.Id);
                degreePlan.AddCourse(plannedCourse1, termId);
                var courseId2 = "7704";
                var section2 = allSections.Where(s => s.CourseId == courseId2 && s.Number == "01").First();
                Assert.IsTrue(section2.Meetings.Count() > 0);
                Assert.IsTrue(section2.Meetings.ElementAt(0).StartDate != null);
                plannedCourse2 = new PlannedCourse(courseId2, section2.Id);
                degreePlan.AddCourse(plannedCourse2, termId);

                // Act -- Check conflicts
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, credits, requirements, RuleResults);

                // Assert -- Two time conflict messages reporting other course
                foreach (var termCode in degreePlan.TermIds)
                {
                    var plannedCourses = degreePlan.GetPlannedCourses(termCode);
                    foreach (var item in plannedCourses)
                    {
                        // Verify that each section has a message, warning of a time conflict with the other section.
                        var message = item.Warnings.Where(m => m.Type == PlannedCourseWarningType.TimeConflict);
                        Assert.IsTrue(message.Count() == 1);
                        if (item.SectionId == section1.Id)
                        {
                            Assert.AreEqual(section2.Id, message.ElementAt(0).SectionId);
                        }
                        if (item.SectionId == section2.Id)
                        {
                            Assert.AreEqual(section1.Id, message.ElementAt(0).SectionId);
                        }
                    }
                }
            }

            [TestMethod]
            public void MessageWhenTermsAreSameAndMeetingTimesOverlapWithAcadCredits()
            {
                // Arrange--Add planned course with section to a degree plan, add academic credit, 
                // both with same term and same meeting times.
                var termId = regTerms.ElementAt(0).Code;
                var courseId1 = "7702";
                var section1 = allSections.Where(s => s.CourseId == courseId1 && s.TermId == termId && s.Number == "01").First();
                Assert.IsTrue(section1.Meetings.Count() > 0);
                Assert.IsTrue(section1.Meetings.ElementAt(0).StartDate != null);
                plannedCourse1 = new PlannedCourse(courseId1, section1.Id);
                degreePlan.AddCourse(plannedCourse1, termId);
                // Create academic credit
                var courseId2 = "7704";
                var section2 = allSections.Where(s => s.CourseId == courseId2 && s.Number == "01").First();
                var course2 = allCourses.Where(c => c.Id == section2.CourseId).First();
                Assert.IsTrue(section2.Meetings.Count() > 0);
                Assert.IsTrue(section2.Meetings.ElementAt(0).StartDate != null);
                var acadCredit = new AcademicCredit("01", course2, section2.Id);
                acadCredit.TermCode = termId;
                var acadCredits = new List<AcademicCredit>() { acadCredit };

                // Act -- Check conflicts
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, acadCredits, requirements, RuleResults);

                // Assert -- One time conflict messages reporting acad credit
                foreach (var termCode in degreePlan.TermIds)
                {
                    var plannedCourses = degreePlan.GetPlannedCourses(termCode);
                    foreach (var item in plannedCourses)
                    {
                        // Verify that planned section has a message, warning of a time conflict with the acad credit section.
                        var message = item.Warnings.Where(m => m.Type == PlannedCourseWarningType.TimeConflict);
                        Assert.IsTrue(message.Count() == 1);
                        if (item.SectionId == section1.Id)
                        {
                            Assert.AreEqual(section2.Id, message.ElementAt(0).SectionId);
                        }
                    }
                }
            }

            [TestMethod]
            public void NoMessageWhenSectionDatesDontOverlap()
            {
                // Arrange--Add two sections to a degree plan, same meeting time, different terms and dates
                var termId = regTerms.ElementAt(0).Code;
                var courseId1 = "7702";
                var section1 = allSections.Where(s => s.CourseId == courseId1 && s.TermId == termId && s.Number == "01").First();
                Assert.IsTrue(section1.Meetings.Count() > 0);
                Assert.IsTrue(section1.Meetings.ElementAt(0).StartDate != null);
                plannedCourse1 = new PlannedCourse(courseId1, section1.Id);
                degreePlan.AddCourse(plannedCourse1, termId);
                var courseId2 = "7704";
                var section2 = allSections.Where(s => s.CourseId == courseId2 && s.TermId == termId && s.Number == "01").First();
                Assert.IsTrue(section2.Meetings.Count() > 0);
                Assert.IsTrue(section2.Meetings.ElementAt(0).StartDate != null);
                plannedCourse2 = new PlannedCourse(courseId2, section2.Id);
                degreePlan.AddCourse(plannedCourse2, termId);

                // Act -- Check conflicts
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, credits, requirements, RuleResults);

                // Assert -- Two time conflict messages reporting other course
                foreach (var termCode in degreePlan.TermIds)
                {
                    var plannedCourses = degreePlan.GetPlannedCourses(termCode);
                    foreach (var item in plannedCourses)
                    {
                        // Verify that each section has a message, warning of a time conflict with the other section.
                        var message = item.Warnings.Where(m => m.Type == PlannedCourseWarningType.TimeConflict);
                        Assert.IsTrue(message.Count() == 1);
                        if (item.SectionId == section1.Id)
                        {
                            Assert.AreEqual(section2.Id, message.ElementAt(0).SectionId);
                        }
                        if (item.SectionId == section2.Id)
                        {
                            Assert.AreEqual(section1.Id, message.ElementAt(0).SectionId);
                        }
                    }
                }
            }

            [TestMethod]
            public void MessagesWhenSectionDatesOverlapAndTimesConflict()
            {
                // Arrange--Add two sections to a degree plan, overlapping start/end dates and times
                var termId = regTerms.ElementAt(0).Code;
                var courseId1 = "7710";
                var section1 = allSections.Where(s => s.CourseId == courseId1 && s.Number == "04").First();
                Assert.IsTrue(section1.Meetings.Count() > 0);
                Assert.IsTrue(section1.Meetings.ElementAt(0).StartDate != null);
                plannedCourse1 = new PlannedCourse(courseId1, section1.Id);
                degreePlan.AddCourse(plannedCourse1, termId);
                var courseId2 = "7711";
                var section2 = allSections.Where(s => s.CourseId == courseId2 && s.Number == "04").First();
                Assert.IsTrue(section2.Meetings.Count() > 0);
                Assert.IsTrue(section2.Meetings.ElementAt(0).StartDate != null);
                plannedCourse2 = new PlannedCourse(courseId2, section2.Id);
                degreePlan.AddCourse(plannedCourse2, termId);

                // Act -- Check conflicts
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, credits, requirements, RuleResults);

                // Assert -- Two time conflict messages reporting other course
                foreach (var termCode in degreePlan.TermIds)
                {
                    var plannedCourses = degreePlan.GetPlannedCourses(termCode);
                    foreach (var item in plannedCourses)
                    {
                        // Find the time conflict message and verify warning of a time conflict with the other section.
                        var message = item.Warnings.Where(m => m.Type == PlannedCourseWarningType.TimeConflict);
                        Assert.IsTrue(message.Count() == 1);
                        if (item.SectionId == section1.Id)
                        {
                            Assert.AreEqual(section2.Id, message.ElementAt(0).SectionId);
                        }
                        if (item.SectionId == section2.Id)
                        {
                            Assert.AreEqual(section1.Id, message.ElementAt(0).SectionId);
                        }
                    }
                }
            }


            [TestMethod]
            public void MessagesWhenDatesOverlapAndTimesConflictWithAcadCredits()
            {
                // Arrange--Add a section to a degree plan
                var termId = regTerms.ElementAt(0).Code;
                var courseId1 = "7710";
                var section1 = allSections.Where(s => s.CourseId == courseId1 && s.Number == "04").First();
                Assert.IsTrue(section1.Meetings.Count() > 0);
                Assert.IsTrue(section1.Meetings.ElementAt(0).StartDate != null);
                plannedCourse1 = new PlannedCourse(courseId1, section1.Id);
                degreePlan.AddCourse(plannedCourse1, termId);
                // Add acad credit with overlapping start/end dates and times
                var courseId2 = "7711";
                var section2 = allSections.Where(s => s.CourseId == courseId2 && s.Number == "04").First();
                var course2 = allCourses.Where(c => c.Id == section2.CourseId).First();
                Assert.IsTrue(section2.Meetings.Count() > 0);
                Assert.IsTrue(section2.Meetings.ElementAt(0).StartDate != null);
                var acadCredit = new AcademicCredit("01", course2, section2.Id);
                acadCredit.TermCode = termId;
                var acadCredits = new List<AcademicCredit>() { acadCredit };

                // Act -- Check conflicts
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, acadCredits, requirements, RuleResults);

                // Assert -- One time conflict messages reporting acad credit
                foreach (var termCode in degreePlan.TermIds)
                {
                    var plannedCourses = degreePlan.GetPlannedCourses(termCode);
                    foreach (var item in plannedCourses)
                    {
                        // Find the time conflict message and verify warning of a time conflict with the acad credit section.
                        var message = item.Warnings.Where(m => m.Type == PlannedCourseWarningType.TimeConflict);
                        Assert.IsTrue(message.Count() == 1);
                        if (item.SectionId == section1.Id)
                        {
                            Assert.AreEqual(section2.Id, message.ElementAt(0).SectionId);
                        }
                    }
                }
            }

            [TestMethod]
            public void MessageWhenTermsAreSameAndMeetingTimesOverlapWithDroppedCompleteAcadCredits()
            {
                // Arrange--Add a section to a degree plan
                var termId = regTerms.ElementAt(0).Code;
                var courseId1 = "7710";
                var section1 = allSections.Where(s => s.CourseId == courseId1 && s.Number == "04").First();
                Assert.IsTrue(section1.Meetings.Count() > 0);
                Assert.IsTrue(section1.Meetings.ElementAt(0).StartDate != null);
                plannedCourse1 = new PlannedCourse(courseId1, section1.Id);
                degreePlan.AddCourse(plannedCourse1, termId);
                // Add acad credit with overlapping start/end dates and times with status of dropped and is completed true
                var courseId2 = "7711";
                var section2 = allSections.Where(s => s.CourseId == courseId2 && s.Number == "04").First();
                var course2 = allCourses.Where(c => c.Id == section2.CourseId).First();
                Assert.IsTrue(section2.Meetings.Count() > 0);
                Assert.IsTrue(section2.Meetings.ElementAt(0).StartDate != null);
                var acadCredit = new AcademicCredit("01", course2, section2.Id);
                acadCredit.TermCode = termId;
                acadCredit.Status = CreditStatus.Dropped;
                acadCredit.VerifiedGrade = new Grade("A", "A", "UG");
                acadCredit.EndDate = DateTime.Now.AddDays(-1);

                var acadCredits = new List<AcademicCredit>() { acadCredit };

                // Act -- Check conflicts
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, acadCredits, requirements, RuleResults);

                // Assert -- One time conflict messages reporting acad credit
                foreach (var termCode in degreePlan.TermIds)
                {
                    var plannedCourses = degreePlan.GetPlannedCourses(termCode);
                    foreach (var item in plannedCourses)
                    {
                        // Find the time conflict message and verify warning of a time conflict with the acad credit section.
                        var message = item.Warnings.Where(m => m.Type == PlannedCourseWarningType.TimeConflict);
                        Assert.IsTrue(message.Count() == 1);
                        if (item.SectionId == section1.Id)
                        {
                            Assert.AreEqual(section2.Id, message.ElementAt(0).SectionId);
                        }
                    }
                }
            }

            public void NoMessageWhenTermsAreSameAndMeetingTimesOverlapWithDroppedIncompleteAcadCredits()
            {
                // Arrange--Add a section to a degree plan
                var termId = regTerms.ElementAt(0).Code;
                var courseId1 = "7710";
                var section1 = allSections.Where(s => s.CourseId == courseId1 && s.Number == "04").First();
                Assert.IsTrue(section1.Meetings.Count() > 0);
                Assert.IsTrue(section1.Meetings.ElementAt(0).StartDate != null);
                plannedCourse1 = new PlannedCourse(courseId1, section1.Id);
                degreePlan.AddCourse(plannedCourse1, termId);
                // Add acad credit with overlapping start/end dates and times with status of dropped and is completed false
                var courseId2 = "7711";
                var section2 = allSections.Where(s => s.CourseId == courseId2 && s.Number == "04").First();
                var course2 = allCourses.Where(c => c.Id == section2.CourseId).First();
                Assert.IsTrue(section2.Meetings.Count() > 0);
                Assert.IsTrue(section2.Meetings.ElementAt(0).StartDate != null);
                var acadCredit = new AcademicCredit("01", course2, section2.Id);
                acadCredit.TermCode = termId;
                acadCredit.Status = CreditStatus.Dropped;

                var acadCredits = new List<AcademicCredit>() { acadCredit };

                // Act -- Check conflicts
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, acadCredits, requirements, RuleResults);

                // Assert -- No time conflict messages reported
                foreach (var termCode in degreePlan.TermIds)
                {
                    var plannedCourses = degreePlan.GetPlannedCourses(termCode);
                    var messages = plannedCourses.SelectMany(pc => pc.Warnings).Where(m => m.Type == PlannedCourseWarningType.TimeConflict);
                    Assert.IsTrue(messages.Count() == 0);
                }
            }

            [TestMethod]
            public void MessageWhenTermsAreSameAndMeetingTimesOverlapWith2PlannedCourseWithDroppedCompleteAcadCredits()
            {
                var termId = regTerms.ElementAt(0).Code;

                // Arrange--Add a section to a degree plan
                var courseId1 = "7710";
                var section1 = allSections.Where(s => s.CourseId == courseId1 && s.Number == "04").First();
                Assert.IsTrue(section1.Meetings.Count() > 0);
                Assert.IsTrue(section1.Meetings.ElementAt(0).StartDate != null);
                plannedCourse1 = new PlannedCourse(courseId1, section1.Id);
                degreePlan.AddCourse(plannedCourse1, termId);

                // Arrange--Add a section to a degree plan
                var courseId2 = "7711";
                var section2 = allSections.Where(s => s.CourseId == courseId2 && s.Number == "04").First();
                Assert.IsTrue(section2.Meetings.Count() > 0);
                Assert.IsTrue(section2.Meetings.ElementAt(0).StartDate != null);
                plannedCourse2 = new PlannedCourse(courseId2, section2.Id);
                degreePlan.AddCourse(plannedCourse2, termId);

                // Add above planned section 2 to acad credit with status of dropped and is completed true
                var course2 = allCourses.Where(c => c.Id == section2.CourseId).First();
                var acadCredit = new AcademicCredit("01", course2, section2.Id);
                acadCredit.TermCode = termId;
                acadCredit.Status = CreditStatus.Dropped;
                acadCredit.VerifiedGrade = new Grade("A", "A", "UG");
                acadCredit.EndDate = DateTime.Now.AddDays(-1);

                var acadCredits = new List<AcademicCredit>() { acadCredit };

                // Act -- Check conflicts
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, acadCredits, requirements, RuleResults);

                // Assert -- One time conflict messages reporting acad credit
                foreach (var termCode in degreePlan.TermIds)
                {
                    var plannedCourses = degreePlan.GetPlannedCourses(termCode);
                    foreach (var item in plannedCourses)
                    {
                        //Planned course will have a time conflict warning with the acad credit section
                        if (item.SectionId == section1.Id)
                        {
                            // Find the time conflict message and verify warning of a time conflict with the acad credit section.
                            var message = item.Warnings.Where(m => m.Type == PlannedCourseWarningType.TimeConflict);
                            Assert.IsTrue(message.Count() == 1);
                            Assert.AreEqual(section2.Id, message.ElementAt(0).SectionId);
                        }

                        //Acad Credit section will not have a time conflict warning with the planned section
                        if (item.SectionId == section2.Id)
                        {
                            // Verify there is no time conflict message and verify warning of a time conflict with the planned section.
                            var message = item.Warnings.Where(m => m.Type == PlannedCourseWarningType.TimeConflict);
                            Assert.IsTrue(message.Count() == 0);
                        }
                    }
                }
            }

            [TestMethod]
            public void MessageWhenTermsAreSameAndMeetingTimesOverlapWith2PlannedCourseWithDroppedIncompleteAcadCredits()
            {
                var termId = regTerms.ElementAt(0).Code;

                // Arrange--Add a section to a degree plan
                var courseId1 = "7710";
                var section1 = allSections.Where(s => s.CourseId == courseId1 && s.Number == "04").First();
                Assert.IsTrue(section1.Meetings.Count() > 0);
                Assert.IsTrue(section1.Meetings.ElementAt(0).StartDate != null);
                plannedCourse1 = new PlannedCourse(courseId1, section1.Id);
                degreePlan.AddCourse(plannedCourse1, termId);

                // Arrange--Add a section to a degree plan
                var courseId2 = "7711";
                var section2 = allSections.Where(s => s.CourseId == courseId2 && s.Number == "04").First();
                Assert.IsTrue(section2.Meetings.Count() > 0);
                Assert.IsTrue(section2.Meetings.ElementAt(0).StartDate != null);
                plannedCourse2 = new PlannedCourse(courseId2, section2.Id);
                degreePlan.AddCourse(plannedCourse2, termId);

                // Add above planned section 2 to acad credit with status of dropped and is completed false
                var course2 = allCourses.Where(c => c.Id == section2.CourseId).First();
                Assert.IsTrue(section2.Meetings.Count() > 0);
                Assert.IsTrue(section2.Meetings.ElementAt(0).StartDate != null);
                var acadCredit = new AcademicCredit("01", course2, section2.Id);
                acadCredit.TermCode = termId;
                acadCredit.Status = CreditStatus.Dropped;

                var acadCredits = new List<AcademicCredit>() { acadCredit };

                // Act -- Check conflicts
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, acadCredits, requirements, RuleResults);

                // Assert -- One time conflict messages reporting acad credit
                foreach (var termCode in degreePlan.TermIds)
                {
                    var plannedCourses = degreePlan.GetPlannedCourses(termCode);
                    foreach (var item in plannedCourses)
                    {
                        // Find the time conflict message and verify acad credit is ignored but warning of a time conflict with planned courses and acad credit
                        var message = item.Warnings.Where(m => m.Type == PlannedCourseWarningType.TimeConflict);
                        Assert.IsTrue(message.Count() == 1);
                        if (item.SectionId == section1.Id)
                        {
                            Assert.AreEqual(section2.Id, message.ElementAt(0).SectionId);
                        }
                        if (item.SectionId == section2.Id)
                        {
                            Assert.AreEqual(section1.Id, message.ElementAt(0).SectionId);
                        }
                    }
                }
            }

            [TestMethod]
            public void MessagesWhenDatesOverlapAndTimesConflictWithNoEndDateOrTime()
            {
                // Arrange--Add two sections to a degree plan, overlapping start/end dates and times BUT without end dates/times
                var termId = regTerms.ElementAt(0).Code;
                var courseId1 = "7710";
                var section1 = allSections.Where(s => s.CourseId == courseId1 && s.Number == "05").First();
                Assert.IsTrue(section1.Meetings.Count() > 0);
                Assert.IsTrue(section1.Meetings.ElementAt(0).StartDate != null);
                plannedCourse1 = new PlannedCourse(courseId1, section1.Id);
                degreePlan.AddCourse(plannedCourse1, termId);
                var courseId2 = "7711";
                var section2 = allSections.Where(s => s.CourseId == courseId2 && s.Number == "05").First();
                Assert.IsTrue(section2.Meetings.Count() > 0);
                Assert.IsTrue(section2.Meetings.ElementAt(0).StartDate != null);
                plannedCourse2 = new PlannedCourse(courseId2, section2.Id);
                degreePlan.AddCourse(plannedCourse2, termId);

                // Act -- Check conflicts
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, credits, requirements, RuleResults);

                // Assert -- Two time conflict messages reporting other course
                foreach (var termCode in degreePlan.TermIds)
                {
                    var plannedCourses = degreePlan.GetPlannedCourses(termCode);
                    foreach (var item in plannedCourses)
                    {
                        // Find the time conflict message and verify warning of a time conflict with the other section.
                        var message = item.Warnings.Where(m => m.Type == PlannedCourseWarningType.TimeConflict);
                        Assert.IsTrue(message.Count() == 1);
                        if (item.SectionId == section1.Id)
                        {
                            Assert.AreEqual(section2.Id, message.ElementAt(0).SectionId);
                        }
                        if (item.SectionId == section2.Id)
                        {
                            Assert.AreEqual(section1.Id, message.ElementAt(0).SectionId);
                        }
                    }
                }
            }

            [TestMethod]
            public void MessagesWhenDatesOverlapAndTimesConflictWithNoEndDateOrTimeOnAcadCredits()
            {
                // Arrange--Add sections to a degree plan and create acad credit with overlapping start/end dates and times 
                // BUT without end dates/times
                var termId = regTerms.ElementAt(0).Code;
                var courseId1 = "7710";
                var section1 = allSections.Where(s => s.CourseId == courseId1 && s.Number == "05").First();
                Assert.IsTrue(section1.Meetings.Count() > 0);
                Assert.IsTrue(section1.Meetings.ElementAt(0).StartDate != null);
                plannedCourse1 = new PlannedCourse(courseId1, section1.Id);
                degreePlan.AddCourse(plannedCourse1, termId);
                // Add academic credit
                var courseId2 = "7711";
                var section2 = allSections.Where(s => s.CourseId == courseId2 && s.Number == "05").First();
                var course2 = allCourses.Where(c => c.Id == section2.CourseId).First();
                Assert.IsTrue(section2.Meetings.Count() > 0);
                Assert.IsTrue(section2.Meetings.ElementAt(0).StartDate != null);
                var acadCredit = new AcademicCredit("01", course2, section2.Id);
                acadCredit.TermCode = termId;
                var acadCredits = new List<AcademicCredit>() { acadCredit };

                // Act -- Check conflicts
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, acadCredits, requirements, RuleResults);

                // Assert -- One time conflict messages reporting conflict with acad credit
                foreach (var termCode in degreePlan.TermIds)
                {
                    var plannedCourses = degreePlan.GetPlannedCourses(termCode);
                    foreach (var item in plannedCourses)
                    {
                        // Find the time conflict message and verify warning of a time conflict with the acad credit section.
                        var message = item.Warnings.Where(m => m.Type == PlannedCourseWarningType.TimeConflict);
                        Assert.IsTrue(message.Count() == 1);
                        if (item.SectionId == section1.Id)
                        {
                            Assert.AreEqual(section2.Id, message.ElementAt(0).SectionId);
                        }
                    }
                }
            }

            [TestMethod]
            public void MessagesWhenDatesOverlapAndTimesConflictWithMultipleConflicts()
            {
                // Arrange--Add three sections to a degree plan, all with overlapping start/end dates and times
                var termId = regTerms.ElementAt(0).Code;
                var courseId1 = "7710";
                var section1 = allSections.Where(s => s.CourseId == courseId1 && s.Number == "04").First();
                Assert.IsTrue(section1.Meetings.Count() > 0);
                Assert.IsTrue(section1.Meetings.ElementAt(0).StartDate != null);
                plannedCourse1 = new PlannedCourse(courseId1, section1.Id);
                degreePlan.AddCourse(plannedCourse1, "");
                var courseId2 = "7711";
                var section2 = allSections.Where(s => s.CourseId == courseId2 && s.Number == "04").First();
                Assert.IsTrue(section2.Meetings.Count() > 0);
                Assert.IsTrue(section2.Meetings.ElementAt(0).StartDate != null);
                plannedCourse2 = new PlannedCourse(courseId2, section2.Id);
                degreePlan.AddCourse(plannedCourse2, "");
                var courseId3 = "7704";
                var section3 = allSections.Where(s => s.CourseId == courseId3 && s.Number == "01" && s.TermId == termId).First();
                Assert.IsTrue(section3.Meetings.Count() > 0);
                Assert.IsTrue(section3.Meetings.ElementAt(0).StartDate != null);
                plannedCourse3 = new PlannedCourse(courseId3, section3.Id);
                degreePlan.AddCourse(plannedCourse3, termId);

                // Act -- Check conflicts
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, credits, requirements, RuleResults);

                // Assert -- Two time conflict messages reporting other course
                foreach (var termCode in degreePlan.TermIds)
                {
                    var plannedCourses = degreePlan.GetPlannedCourses(termCode);
                    foreach (var item in plannedCourses)
                    {
                        // Find the time conflict message and verify warning of a time conflict with the other section.
                        var message = item.Warnings.Where(m => m.Type == PlannedCourseWarningType.TimeConflict);
                        Assert.IsTrue(message.Count() == 2);
                    }
                }
            }


            [TestMethod]
            public void MessagesWhenDatesOverlapAndTimesConflictWithMultipleConflictsWithAcadCredit()
            {
                // Arrange--Add one section to a degree plan, two acad credits, all overlapping times
                var termId = regTerms.ElementAt(0).Code;
                var courseId1 = "7710";
                var section1 = allSections.Where(s => s.CourseId == courseId1 && s.Number == "04").First();
                Assert.IsTrue(section1.Meetings.Count() > 0);
                Assert.IsTrue(section1.Meetings.ElementAt(0).StartDate != null);
                plannedCourse1 = new PlannedCourse(courseId1, section1.Id);
                degreePlan.AddCourse(plannedCourse1, "");
                // Add overlapping acad credit
                var courseId2 = "7711";
                var course2 = allCourses.Where(c => c.Id == courseId2).First();
                var section2 = allSections.Where(s => s.CourseId == courseId2 && s.Number == "04").First();
                Assert.IsTrue(section2.Meetings.Count() > 0);
                Assert.IsTrue(section2.Meetings.ElementAt(0).StartDate != null);
                var acadCredit1 = new AcademicCredit("01", course2, section2.Id) { TermCode = termId };
                var courseId3 = "7704";
                var course3 = allCourses.Where(c => c.Id == courseId3).First();
                var section3 = allSections.Where(s => s.CourseId == courseId3 && s.Number == "01" && s.TermId == termId).First();
                Assert.IsTrue(section3.Meetings.Count() > 0);
                Assert.IsTrue(section3.Meetings.ElementAt(0).StartDate != null);
                var acadCredit2 = new AcademicCredit("02", course3, section3.Id) { TermCode = termId };
                var acadCredits = new List<AcademicCredit>() { acadCredit1, acadCredit2 };
                // Act -- Check conflicts
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, acadCredits, requirements, RuleResults);

                // Assert -- Two time conflict messages reporting acad credits
                foreach (var termCode in degreePlan.TermIds)
                {
                    var plannedCourses = degreePlan.GetPlannedCourses(termCode);
                    foreach (var item in plannedCourses)
                    {
                        // Find the time conflict message and verify warning of a time conflict with the two acadCredits
                        var message = item.Warnings.Where(m => m.Type == PlannedCourseWarningType.TimeConflict);
                        Assert.IsTrue(message.Count() == 2);
                    }
                }
            }

            [TestMethod]
            public void NoMessagesWhenDatesOverlapAndTimesConflictWithMismatchedDays()
            {
                // Arrange--Add two sections to a degree plan, overlapping start/end dates and times BUT without end dates/times
                var termId = regTerms.ElementAt(0).Code;
                var courseId1 = "7710";
                var section1 = allSections.Where(s => s.CourseId == courseId1 && s.Number == "05").First();
                Assert.IsTrue(section1.Meetings.Count() > 0);
                Assert.IsTrue(section1.Meetings.ElementAt(0).StartDate != null);
                plannedCourse1 = new PlannedCourse(courseId1, section1.Id);
                degreePlan.AddCourse(plannedCourse1, termId);
                var courseId2 = "7711";
                var section2 = allSections.Where(s => s.CourseId == courseId2 && s.Number == "06").First();
                Assert.IsTrue(section2.Meetings.Count() > 0);
                Assert.IsTrue(section2.Meetings.ElementAt(0).StartDate != null);
                plannedCourse2 = new PlannedCourse(courseId2, section2.Id);
                degreePlan.AddCourse(plannedCourse2, termId);

                // Act -- Check conflicts
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, credits, requirements, RuleResults);

                // Assert -- No time conflict messages
                foreach (var termCode in degreePlan.TermIds)
                {
                    var plannedCourses = degreePlan.GetPlannedCourses(termCode);
                    foreach (var item in plannedCourses)
                    {
                        var message = item.Warnings.Where(m => m.Type == PlannedCourseWarningType.TimeConflict);
                        Assert.IsTrue(message.Count() == 0);
                    }
                }
            }

            [TestMethod]
            public void NoMessageWhenSectionStartsAtSameTimeAsOtherSectionEnds()
            {
                // Arrange--Add two sections to a degree plan, 7711/07 starts at same time 7710/04 ends. No message
                var termId = regTerms.ElementAt(0).Code;
                var courseId1 = "7710";
                var section1 = allSections.Where(s => s.CourseId == courseId1 && s.Number == "04").First();
                Assert.IsTrue(section1.Meetings.Count() > 0);
                Assert.IsTrue(section1.Meetings.ElementAt(0).StartDate != null);
                plannedCourse1 = new PlannedCourse(courseId1, section1.Id);
                degreePlan.AddCourse(plannedCourse1, termId);
                var courseId2 = "7711";
                var section2 = allSections.Where(s => s.CourseId == courseId2 && s.Number == "07").First();
                Assert.IsTrue(section2.Meetings.Count() > 0);
                Assert.IsTrue(section2.Meetings.ElementAt(0).StartDate != null);
                plannedCourse2 = new PlannedCourse(courseId2, section2.Id);
                degreePlan.AddCourse(plannedCourse2, termId);

                // Act -- Check conflicts
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, credits, requirements, RuleResults);

                // Assert -- No time conflict messages
                foreach (var termCode in degreePlan.TermIds)
                {
                    var plannedCourses = degreePlan.GetPlannedCourses(termCode);
                    foreach (var item in plannedCourses)
                    {
                        var message = item.Warnings.Where(m => m.Type == PlannedCourseWarningType.TimeConflict);
                        Assert.IsTrue(message.Count() == 0);
                    }
                }
            }

            [TestMethod]
            public void NoMessageWhenSectionEndAtSameTimeAsOtherSectionStarts()
            {
                // Arrange--Add two sections to a degree plan, 7711/08 ends at same time 7710/04 starts. No message
                var termId = regTerms.ElementAt(0).Code;
                var courseId1 = "7710";
                var section1 = allSections.Where(s => s.CourseId == courseId1 && s.Number == "04").First();
                Assert.IsTrue(section1.Meetings.Count() > 0);
                Assert.IsTrue(section1.Meetings.ElementAt(0).StartDate != null);
                plannedCourse1 = new PlannedCourse(courseId1, section1.Id);
                degreePlan.AddCourse(plannedCourse1, termId);
                var courseId2 = "7711";
                var section2 = allSections.Where(s => s.CourseId == courseId2 && s.Number == "08").First();
                Assert.IsTrue(section2.Meetings.Count() > 0);
                Assert.IsTrue(section2.Meetings.ElementAt(0).StartDate != null);
                plannedCourse2 = new PlannedCourse(courseId2, section2.Id);
                degreePlan.AddCourse(plannedCourse2, termId);

                // Act -- Check conflicts
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, credits, requirements, RuleResults);

                // Assert -- No time conflict messages
                foreach (var termCode in degreePlan.TermIds)
                {
                    var plannedCourses = degreePlan.GetPlannedCourses(termCode);
                    foreach (var item in plannedCourses)
                    {
                        var message = item.Warnings.Where(m => m.Type == PlannedCourseWarningType.TimeConflict);
                        Assert.IsTrue(message.Count() == 0);
                    }
                }
            }

            [TestMethod]
            public void NoMessageWhenSectionMeetingDatesDoNotOverlap()
            {
                // Arrange--Add two sections to a degree plan, 7714/15 with same section meeting days and times, but dates do not overlap
                var termId = regTerms.ElementAt(0).Code;
                var courseId1 = "7714";
                var section1 = allSections.Where(s => s.CourseId == courseId1 && s.Number == "04").First();
                Assert.IsTrue(section1.Meetings.Count() > 0);
                Assert.IsTrue(section1.Meetings.ElementAt(0).StartDate != null);
                plannedCourse1 = new PlannedCourse(courseId1, section1.Id);
                degreePlan.AddCourse(plannedCourse1, termId);
                var courseId2 = "7715";
                var section2 = allSections.Where(s => s.CourseId == courseId2 && s.Number == "04").First();
                Assert.IsTrue(section2.Meetings.Count() > 0);
                Assert.IsTrue(section2.Meetings.ElementAt(0).StartDate != null);
                plannedCourse2 = new PlannedCourse(courseId2, section2.Id);
                degreePlan.AddCourse(plannedCourse2, termId);

                // Act -- Check conflicts
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, credits, requirements, RuleResults);

                // Assert -- No time conflict messages
                foreach (var termCode in degreePlan.TermIds)
                {
                    var plannedCourses = degreePlan.GetPlannedCourses(termCode);
                    foreach (var item in plannedCourses)
                    {
                        var message = item.Warnings.Where(m => m.Type == PlannedCourseWarningType.TimeConflict);
                        Assert.IsTrue(message.Count() == 0);
                    }
                }
            }

            [TestMethod]
            public void MessageWhenSectionMeetingDatesDoOverlap()
            {
                // Arrange--Add two sections to a degree plan, 7714/15 with same section meeting days and times, but dates do not overlap
                var termId = regTerms.ElementAt(0).Code;
                var courseId1 = "7714";
                var section1 = allSections.Where(s => s.CourseId == courseId1 && s.Number == "05").First();
                Assert.IsTrue(section1.Meetings.Count() > 0);
                Assert.IsTrue(section1.Meetings.ElementAt(0).StartDate != null);
                plannedCourse1 = new PlannedCourse(courseId1, section1.Id);
                degreePlan.AddCourse(plannedCourse1, termId);
                var courseId2 = "7715";
                var section2 = allSections.Where(s => s.CourseId == courseId2 && s.Number == "05").First();
                Assert.IsTrue(section2.Meetings.Count() > 0);
                Assert.IsTrue(section2.Meetings.ElementAt(0).StartDate != null);
                plannedCourse2 = new PlannedCourse(courseId2, section2.Id);
                degreePlan.AddCourse(plannedCourse2, termId);

                // Act -- Check conflicts
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, credits, requirements, RuleResults);

                // Assert -- Each planned course has a time conflict message reporting other course
                foreach (var termCode in degreePlan.TermIds)
                {
                    var plannedCourses = degreePlan.GetPlannedCourses(termCode);
                    foreach (var item in plannedCourses)
                    {
                        // Find the time conflict message and verify warning of a time conflict with the other section.
                        var message = item.Warnings.Where(m => m.Type == PlannedCourseWarningType.TimeConflict);
                        Assert.IsTrue(message.Count() == 1);
                    }
                }
            }

            /// <summary>
            /// Section 1 meets Tuesdays TBD
            /// Section 2 meets Tuesdays TBD
            /// These sections should not show conflicts
            /// </summary>
            [TestMethod]
            public void DegreePlan_CheckForConflicts_CheckTimeConflicts_CheckIfSectionsOverlap_planned_courses_have_same_day_but_no_times()
            {
                var section1 = new Section("123",
                    "7714",
                    "01",
                    DateTime.Today.AddDays(30),
                    3m,
                    null,
                    "Section 1",
                    "IN",
                    new List<OfferingDepartment>() { new OfferingDepartment("MATH") },
                    new List<string>() { "100" },
                    "UG",
                    new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-60)) })
                {
                    TermId = regTerms.ElementAt(0).Code
                };
                section1.AddSectionMeeting(new SectionMeeting("234",
                    section1.Id,
                    "LEC",
                    section1.StartDate,
                    DateTime.Today.AddDays(60),
                    "W")
                {
                    Days = new List<DayOfWeek>() { DayOfWeek.Tuesday },
                    StartTime = null,
                    EndTime = null
                });

                var section2 = new Section("124",
                    "7715",
                    "01",
                    DateTime.Today.AddDays(30),
                    3m,
                    null,
                    "Section 2",
                    "IN",
                    new List<OfferingDepartment>() { new OfferingDepartment("HIST") },
                    new List<string>() { "100" },
                    "UG",
                    new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-60)) })
                {
                    TermId = regTerms.ElementAt(0).Code
                };

                section2.AddSectionMeeting(new SectionMeeting("235",
                    section2.Id,
                    "LEC",
                    section2.StartDate,
                    DateTime.Today.AddDays(60),
                    "W")
                {
                    Days = new List<DayOfWeek>() { DayOfWeek.Tuesday },
                    StartTime = null,
                    EndTime = null
                });
                var dpSections = new List<Section>() { section1, section2 };

                plannedCourse1 = new PlannedCourse(section1.CourseId, section1.Id);
                degreePlan.AddCourse(plannedCourse1, section1.TermId);
                plannedCourse2 = new PlannedCourse(section2.CourseId, section2.Id);
                degreePlan.AddCourse(plannedCourse2, section2.TermId);

                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, dpSections, credits, requirements, RuleResults);

                // Assert -- Each planned course has a time conflict message reporting other course
                foreach (var termCode in degreePlan.TermIds)
                {
                    var plannedCourses = degreePlan.GetPlannedCourses(termCode);
                    foreach (var item in plannedCourses)
                    {
                        var message = item.Warnings.Where(m => m.Type == PlannedCourseWarningType.TimeConflict);
                        Assert.IsTrue(message.Count() == 0);
                    }
                }
            }

            /// <summary>
            /// Section 1 meets Tuesdays TBD and Thursdays 6am-7am
            /// Section 2 meets Tuesdays TBD and Thursdays 8am-9am
            /// These sections should not show conflicts
            /// </summary>
            [TestMethod]
            public void DegreePlan_CheckForConflicts_CheckTimeConflicts_CheckIfSectionsOverlap_planned_courses_have_same_days_but_no_conflicts_on_days_with_times()
            {
                var section1 = new Section("123",
                    "7714",
                    "01",
                    DateTime.Today.AddDays(30),
                    3m,
                    null,
                    "Section 1",
                    "IN",
                    new List<OfferingDepartment>() { new OfferingDepartment("MATH") },
                    new List<string>() { "100" },
                    "UG",
                    new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-60)) })
                {
                    TermId = regTerms.ElementAt(0).Code
                };
                section1.AddSectionMeeting(new SectionMeeting("234",
                    section1.Id,
                    "LEC",
                    section1.StartDate,
                    DateTime.Today.AddDays(60),
                    "W")
                {
                    Days = new List<DayOfWeek>() { DayOfWeek.Tuesday },
                    StartTime = null,
                    EndTime = null
                });
                section1.AddSectionMeeting(new SectionMeeting("234",
                    section1.Id,
                    "LEC",
                    section1.StartDate,
                    DateTime.Today.AddDays(60),
                    "W")
                {
                    Days = new List<DayOfWeek>() { DayOfWeek.Thursday },
                    StartTime = DateTime.Today.AddHours(6),
                    EndTime = DateTime.Today.AddHours(7)
                });

                var section2 = new Section("124",
                    "7715",
                    "01",
                    DateTime.Today.AddDays(30),
                    3m,
                    null,
                    "Section 2",
                    "IN",
                    new List<OfferingDepartment>() { new OfferingDepartment("HIST") },
                    new List<string>() { "100" },
                    "UG",
                    new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-60)) })
                {
                    TermId = regTerms.ElementAt(0).Code
                };

                section2.AddSectionMeeting(new SectionMeeting("235",
                    section2.Id,
                    "LEC",
                    section2.StartDate,
                    DateTime.Today.AddDays(60),
                    "W")
                {
                    Days = new List<DayOfWeek>() { DayOfWeek.Tuesday },
                    StartTime = null,
                    EndTime = null
                });
                section2.AddSectionMeeting(new SectionMeeting("235",
                    section2.Id,
                    "LEC",
                    section2.StartDate,
                    DateTime.Today.AddDays(60),
                    "W")
                {
                    Days = new List<DayOfWeek>() { DayOfWeek.Thursday },
                    StartTime = DateTime.Today.AddHours(8),
                    EndTime = DateTime.Today.AddHours(9)
                });
                var dpSections = new List<Section>() { section1, section2 };

                plannedCourse1 = new PlannedCourse(section1.CourseId, section1.Id);
                degreePlan.AddCourse(plannedCourse1, section1.TermId);
                plannedCourse2 = new PlannedCourse(section2.CourseId, section2.Id);
                degreePlan.AddCourse(plannedCourse2, section2.TermId);

                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, dpSections, credits, requirements, RuleResults);

                // Assert -- Each planned course has a time conflict message reporting other course
                foreach (var termCode in degreePlan.TermIds)
                {
                    var plannedCourses = degreePlan.GetPlannedCourses(termCode);
                    foreach (var item in plannedCourses)
                    {
                        var message = item.Warnings.Where(m => m.Type == PlannedCourseWarningType.TimeConflict);
                        Assert.IsTrue(message.Count() == 0);
                    }
                }
            }

            /// <summary>
            /// Section 1 meets Tuesdays TBD and Thursdays 8am-9am
            /// Section 2 meets Tuesdays TBD and Thursdays 8am-9am
            /// These sections should show conflicts
            /// </summary>
            [TestMethod]
            public void DegreePlan_CheckForConflicts_CheckTimeConflicts_CheckIfSectionsOverlap_planned_courses_have_same_days_and_conflicts_on_days_with_times()
            {
                var section1 = new Section("123",
                    "7714",
                    "01",
                    DateTime.Today.AddDays(30),
                    3m,
                    null,
                    "Section 1",
                    "IN",
                    new List<OfferingDepartment>() { new OfferingDepartment("MATH") },
                    new List<string>() { "100" },
                    "UG",
                    new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-60)) })
                {
                    TermId = regTerms.ElementAt(0).Code
                };
                section1.AddSectionMeeting(new SectionMeeting("234",
                    section1.Id,
                    "LEC",
                    section1.StartDate,
                    DateTime.Today.AddDays(60),
                    "W")
                {
                    Days = new List<DayOfWeek>() { DayOfWeek.Tuesday },
                    StartTime = null,
                    EndTime = null
                });
                section1.AddSectionMeeting(new SectionMeeting("234",
                    section1.Id,
                    "LEC",
                    section1.StartDate,
                    DateTime.Today.AddDays(60),
                    "W")
                {
                    Days = new List<DayOfWeek>() { DayOfWeek.Thursday },
                    StartTime = DateTime.Today.AddHours(8),
                    EndTime = DateTime.Today.AddHours(9)
                });

                var section2 = new Section("124",
                    "7715",
                    "01",
                    DateTime.Today.AddDays(30),
                    3m,
                    null,
                    "Section 2",
                    "IN",
                    new List<OfferingDepartment>() { new OfferingDepartment("HIST") },
                    new List<string>() { "100" },
                    "UG",
                    new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-60)) })
                {
                    TermId = regTerms.ElementAt(0).Code
                };

                section2.AddSectionMeeting(new SectionMeeting("235",
                    section2.Id,
                    "LEC",
                    section2.StartDate,
                    DateTime.Today.AddDays(60),
                    "W")
                {
                    Days = new List<DayOfWeek>() { DayOfWeek.Tuesday },
                    StartTime = null,
                    EndTime = null
                });
                section2.AddSectionMeeting(new SectionMeeting("235",
                    section2.Id,
                    "LEC",
                    section2.StartDate,
                    DateTime.Today.AddDays(60),
                    "W")
                {
                    Days = new List<DayOfWeek>() { DayOfWeek.Thursday },
                    StartTime = DateTime.Today.AddHours(8),
                    EndTime = DateTime.Today.AddHours(9)
                });
                var dpSections = new List<Section>() { section1, section2 };

                plannedCourse1 = new PlannedCourse(section1.CourseId, section1.Id);
                degreePlan.AddCourse(plannedCourse1, section1.TermId);
                plannedCourse2 = new PlannedCourse(section2.CourseId, section2.Id);
                degreePlan.AddCourse(plannedCourse2, section2.TermId);

                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, dpSections, credits, requirements, RuleResults);

                // Assert -- Each planned course has a time conflict message reporting other course
                foreach (var termCode in degreePlan.TermIds)
                {
                    var plannedCourses = degreePlan.GetPlannedCourses(termCode);
                    foreach (var item in plannedCourses)
                    {
                        var message = item.Warnings.Where(m => m.Type == PlannedCourseWarningType.TimeConflict);
                        Assert.IsTrue(message.Count() == 1);
                    }
                }
            }

            /// <summary>
            /// Section 1 meets Tuesdays TBD
            /// Section 2 meets Tuesdays TBD
            /// These sections should not show conflicts
            /// </summary>
            [TestMethod]
            public void DegreePlan_CheckForConflicts_CheckTimeConflicts_CheckIfSectionsOverlap_academic_credits_have_same_day_but_no_times()
            {
                var credit1 = new AcademicCredit("123")
                {
                    TermCode = regTerms.ElementAt(0).Code,
                    SectionId = "123"
                };
                var credit2 = new AcademicCredit("124")
                {
                    TermCode = regTerms.ElementAt(0).Code,
                    SectionId = "124"
                };
                var dpCredits = new List<AcademicCredit>() { credit1, credit2 };

                var section1 = new Section("123",
                    "7714",
                    "01",
                    DateTime.Today.AddDays(30),
                    3m,
                    null,
                    "Section 1",
                    "IN",
                    new List<OfferingDepartment>() { new OfferingDepartment("MATH") },
                    new List<string>() { "100" },
                    "UG",
                    new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-60)) })
                {
                    TermId = regTerms.ElementAt(0).Code
                };
                section1.AddSectionMeeting(new SectionMeeting("234",
                    section1.Id,
                    "LEC",
                    section1.StartDate,
                    DateTime.Today.AddDays(60),
                    "W")
                {
                    Days = new List<DayOfWeek>() { DayOfWeek.Tuesday },
                    StartTime = null,
                    EndTime = null
                });

                var section2 = new Section("124",
                    "7715",
                    "01",
                    DateTime.Today.AddDays(30),
                    3m,
                    null,
                    "Section 2",
                    "IN",
                    new List<OfferingDepartment>() { new OfferingDepartment("HIST") },
                    new List<string>() { "100" },
                    "UG",
                    new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-60)) })
                {
                    TermId = regTerms.ElementAt(0).Code
                };

                section2.AddSectionMeeting(new SectionMeeting("235",
                    section2.Id,
                    "LEC",
                    section2.StartDate,
                    DateTime.Today.AddDays(60),
                    "W")
                {
                    Days = new List<DayOfWeek>() { DayOfWeek.Tuesday },
                    StartTime = null,
                    EndTime = null
                });
                var dpSections = new List<Section>() { section1, section2 };

                degreePlan.AddTerm(section1.TermId);
                degreePlan.AddCourse(new PlannedCourse(section1.CourseId, section1.Id), section1.TermId);
                degreePlan.AddCourse(new PlannedCourse(section2.CourseId, section2.Id), section2.TermId);
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, dpSections, dpCredits, requirements, RuleResults);

                // Assert -- Each planned course has a time conflict message reporting other course
                foreach (var termCode in degreePlan.TermIds)
                {
                    var plannedCourses = degreePlan.GetPlannedCourses(termCode);
                    foreach (var item in plannedCourses)
                    {
                        var message = item.Warnings.Where(m => m.Type == PlannedCourseWarningType.TimeConflict);
                        Assert.IsTrue(message.Count() == 0);
                    }
                }
            }

            /// <summary>
            /// Section 1 meets Tuesdays TBD and Thursdays 6am-7am
            /// Section 2 meets Tuesdays TBD and Thursdays 8am-9am
            /// These sections should not show conflicts
            /// </summary>
            [TestMethod]
            public void DegreePlan_CheckForConflicts_CheckTimeConflicts_CheckIfSectionsOverlap_academic_credits_have_same_days_but_no_conflicts_on_days_with_times()
            {
                var credit1 = new AcademicCredit("123")
                {
                    TermCode = regTerms.ElementAt(0).Code,
                    SectionId = "123"
                };
                var credit2 = new AcademicCredit("124")
                {
                    TermCode = regTerms.ElementAt(0).Code,
                    SectionId = "124"
                };
                var dpCredits = new List<AcademicCredit>() { credit1, credit2 };


                var section1 = new Section("123",
                    "7714",
                    "01",
                    DateTime.Today.AddDays(30),
                    3m,
                    null,
                    "Section 1",
                    "IN",
                    new List<OfferingDepartment>() { new OfferingDepartment("MATH") },
                    new List<string>() { "100" },
                    "UG",
                    new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-60)) })
                {
                    TermId = regTerms.ElementAt(0).Code
                };
                section1.AddSectionMeeting(new SectionMeeting("234",
                    section1.Id,
                    "LEC",
                    section1.StartDate,
                    DateTime.Today.AddDays(60),
                    "W")
                {
                    Days = new List<DayOfWeek>() { DayOfWeek.Tuesday },
                    StartTime = null,
                    EndTime = null
                });
                section1.AddSectionMeeting(new SectionMeeting("234",
                    section1.Id,
                    "LEC",
                    section1.StartDate,
                    DateTime.Today.AddDays(60),
                    "W")
                {
                    Days = new List<DayOfWeek>() { DayOfWeek.Thursday },
                    StartTime = DateTime.Today.AddHours(6),
                    EndTime = DateTime.Today.AddHours(7)
                });

                var section2 = new Section("124",
                    "7715",
                    "01",
                    DateTime.Today.AddDays(30),
                    3m,
                    null,
                    "Section 2",
                    "IN",
                    new List<OfferingDepartment>() { new OfferingDepartment("HIST") },
                    new List<string>() { "100" },
                    "UG",
                    new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-60)) })
                {
                    TermId = regTerms.ElementAt(0).Code
                };

                section2.AddSectionMeeting(new SectionMeeting("235",
                    section2.Id,
                    "LEC",
                    section2.StartDate,
                    DateTime.Today.AddDays(60),
                    "W")
                {
                    Days = new List<DayOfWeek>() { DayOfWeek.Tuesday },
                    StartTime = null,
                    EndTime = null
                });
                section2.AddSectionMeeting(new SectionMeeting("235",
                    section2.Id,
                    "LEC",
                    section2.StartDate,
                    DateTime.Today.AddDays(60),
                    "W")
                {
                    Days = new List<DayOfWeek>() { DayOfWeek.Thursday },
                    StartTime = DateTime.Today.AddHours(8),
                    EndTime = DateTime.Today.AddHours(9)
                });
                var dpSections = new List<Section>() { section1, section2 };

                degreePlan.AddTerm(section1.TermId);
                degreePlan.AddCourse(new PlannedCourse(section1.CourseId, section1.Id), section1.TermId);
                degreePlan.AddCourse(new PlannedCourse(section2.CourseId, section2.Id), section2.TermId);
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, dpSections, new List<AcademicCredit>(), requirements, RuleResults);

                // Assert -- Each planned course has a time conflict message reporting other course
                foreach (var termCode in degreePlan.TermIds)
                {
                    var plannedCourses = degreePlan.GetPlannedCourses(termCode);
                    foreach (var item in plannedCourses)
                    {
                        var message = item.Warnings.Where(m => m.Type == PlannedCourseWarningType.TimeConflict);
                        Assert.IsTrue(message.Count() == 0);
                    }
                }
            }

            /// <summary>
            /// Section 1 meets Tuesdays TBD and Thursdays 8am-9am
            /// Section 2 meets Tuesdays TBD and Thursdays 8am-9am
            /// These sections should not show conflicts
            /// </summary>
            [TestMethod]
            public void DegreePlan_CheckForConflicts_CheckTimeConflicts_CheckIfSectionsOverlap_academic_credits_have_same_days_and_conflicts_on_days_with_times()
            {
                var credit1 = new AcademicCredit("123")
                {
                    TermCode = regTerms.ElementAt(0).Code,
                    SectionId = "123"
                };
                var credit2 = new AcademicCredit("124")
                {
                    TermCode = regTerms.ElementAt(0).Code,
                    SectionId = "124"
                };
                var dpCredits = new List<AcademicCredit>() { credit1, credit2 };


                var section1 = new Section("123",
                    "7714",
                    "01",
                    DateTime.Today.AddDays(30),
                    3m,
                    null,
                    "Section 1",
                    "IN",
                    new List<OfferingDepartment>() { new OfferingDepartment("MATH") },
                    new List<string>() { "100" },
                    "UG",
                    new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-60)) })
                {
                    TermId = regTerms.ElementAt(0).Code
                };
                section1.AddSectionMeeting(new SectionMeeting("234",
                    section1.Id,
                    "LEC",
                    section1.StartDate,
                    DateTime.Today.AddDays(60),
                    "W")
                {
                    Days = new List<DayOfWeek>() { DayOfWeek.Tuesday },
                    StartTime = null,
                    EndTime = null
                });
                section1.AddSectionMeeting(new SectionMeeting("234",
                    section1.Id,
                    "LEC",
                    section1.StartDate,
                    DateTime.Today.AddDays(60),
                    "W")
                {
                    Days = new List<DayOfWeek>() { DayOfWeek.Thursday },
                    StartTime = DateTime.Today.AddHours(8),
                    EndTime = DateTime.Today.AddHours(9)
                });

                var section2 = new Section("124",
                    "7715",
                    "01",
                    DateTime.Today.AddDays(30),
                    3m,
                    null,
                    "Section 2",
                    "IN",
                    new List<OfferingDepartment>() { new OfferingDepartment("HIST") },
                    new List<string>() { "100" },
                    "UG",
                    new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-60)) })
                {
                    TermId = regTerms.ElementAt(0).Code
                };

                section2.AddSectionMeeting(new SectionMeeting("235",
                    section2.Id,
                    "LEC",
                    section2.StartDate,
                    DateTime.Today.AddDays(60),
                    "W")
                {
                    Days = new List<DayOfWeek>() { DayOfWeek.Tuesday },
                    StartTime = null,
                    EndTime = null
                });
                section2.AddSectionMeeting(new SectionMeeting("235",
                    section2.Id,
                    "LEC",
                    section2.StartDate,
                    DateTime.Today.AddDays(60),
                    "W")
                {
                    Days = new List<DayOfWeek>() { DayOfWeek.Thursday },
                    StartTime = DateTime.Today.AddHours(8),
                    EndTime = DateTime.Today.AddHours(9)
                });
                var dpSections = new List<Section>() { section1, section2 };

                degreePlan.AddTerm(section1.TermId);
                degreePlan.AddCourse(new PlannedCourse(section1.CourseId, section1.Id), section1.TermId);
                degreePlan.AddCourse(new PlannedCourse(section2.CourseId, section2.Id), section2.TermId);
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, dpSections, new List<AcademicCredit>(), requirements, RuleResults);

                // Assert -- Each planned course has a time conflict message reporting other course
                foreach (var termCode in degreePlan.TermIds)
                {
                    var plannedCourses = degreePlan.GetPlannedCourses(termCode);
                    foreach (var item in plannedCourses)
                    {
                        var message = item.Warnings.Where(m => m.Type == PlannedCourseWarningType.TimeConflict);
                        Assert.IsTrue(message.Count() > 0);
                    }
                }
            }

            /// <summary>
            /// Section 1 meets Tuesdays TBD, Thursdays 4am-5am
            /// Section 2 meets Tuesdays TBD
            /// These sections should not show conflicts
            /// </summary>
            [TestMethod]
            public void DegreePlan_CheckForConflicts_CheckTimeConflicts_CheckIfSectionsOverlap_planned_courses_have_same_day_but_no_times_and_first_section_has_another_meeting()
            {
                var credit1 = new AcademicCredit("123")
                {
                    TermCode = regTerms.ElementAt(0).Code,
                    SectionId = "123"
                };
                var credit2 = new AcademicCredit("124")
                {
                    TermCode = regTerms.ElementAt(0).Code,
                    SectionId = "124"
                };
                var dpCredits = new List<AcademicCredit>() { credit1, credit2 };

                var section1 = new Section("123",
                    "7714",
                    "01",
                    DateTime.Today.AddDays(30),
                    3m,
                    null,
                    "Section 1",
                    "IN",
                    new List<OfferingDepartment>() { new OfferingDepartment("MATH") },
                    new List<string>() { "100" },
                    "UG",
                    new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-60)) })
                {
                    TermId = regTerms.ElementAt(0).Code
                };
                section1.AddSectionMeeting(new SectionMeeting("234",
                    section1.Id,
                    "LEC",
                    section1.StartDate,
                    DateTime.Today.AddDays(60),
                    "W")
                {
                    Days = new List<DayOfWeek>() { DayOfWeek.Thursday },
                    StartTime = DateTimeOffset.Now.AddHours(-3),
                    EndTime = DateTimeOffset.Now.AddHours(-2)
                });
                section1.AddSectionMeeting(new SectionMeeting("235",
                    section1.Id,
                    "LEC",
                    section1.StartDate,
                    DateTime.Today.AddDays(60),
                    "W")
                {
                    Days = new List<DayOfWeek>() { DayOfWeek.Tuesday },
                    StartTime = null,
                    EndTime = null
                });

                var section2 = new Section("124",
                    "7715",
                    "01",
                    DateTime.Today.AddDays(30),
                    3m,
                    null,
                    "Section 2",
                    "IN",
                    new List<OfferingDepartment>() { new OfferingDepartment("HIST") },
                    new List<string>() { "100" },
                    "UG",
                    new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-60)) })
                {
                    TermId = regTerms.ElementAt(0).Code
                };

                section2.AddSectionMeeting(new SectionMeeting("236",
                    section2.Id,
                    "LEC",
                    section2.StartDate,
                    DateTime.Today.AddDays(60),
                    "W")
                {
                    Days = new List<DayOfWeek>() { DayOfWeek.Tuesday },
                    StartTime = null,
                    EndTime = null
                });
                var dpSections = new List<Section>() { section1, section2 };

                degreePlan.AddTerm(section1.TermId);
                degreePlan.AddCourse(new PlannedCourse(section1.CourseId, section1.Id), section1.TermId);
                degreePlan.AddCourse(new PlannedCourse(section2.CourseId, section2.Id), section2.TermId);
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, dpSections, new List<AcademicCredit>(), requirements, RuleResults);

                // Assert -- Each planned course has a time conflict message reporting other course
                foreach (var termCode in degreePlan.TermIds)
                {
                    var plannedCourses = degreePlan.GetPlannedCourses(termCode);
                    foreach (var item in plannedCourses)
                    {
                        var message = item.Warnings.Where(m => m.Type == PlannedCourseWarningType.TimeConflict);
                        Assert.IsTrue(message.Count() == 0);
                    }
                }
            }

            /// <summary>
            /// Section 1 meets Tuesdays TBD, Thursdays 4am-5am
            /// Section 2 meets Tuesdays TBD
            /// These sections should not show conflicts
            /// </summary>
            [TestMethod]
            public void DegreePlan_CheckForConflicts_CheckTimeConflicts_CheckIfSectionsOverlap_academic_credits_have_same_day_but_no_times_and_first_section_has_another_meeting()
            {
                var credit1 = new AcademicCredit("123")
                {
                    TermCode = regTerms.ElementAt(0).Code,
                    SectionId = "123"
                };
                var credit2 = new AcademicCredit("124")
                {
                    TermCode = regTerms.ElementAt(0).Code,
                    SectionId = "124"
                };
                var dpCredits = new List<AcademicCredit>() { credit1, credit2 };

                var section1 = new Section("123",
                    "7714",
                    "01",
                    DateTime.Today.AddDays(30),
                    3m,
                    null,
                    "Section 1",
                    "IN",
                    new List<OfferingDepartment>() { new OfferingDepartment("MATH") },
                    new List<string>() { "100" },
                    "UG",
                    new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-60)) })
                {
                    TermId = regTerms.ElementAt(0).Code
                };
                section1.AddSectionMeeting(new SectionMeeting("234",
                    section1.Id,
                    "LEC",
                    section1.StartDate,
                    DateTime.Today.AddDays(60),
                    "W")
                {
                    Days = new List<DayOfWeek>() { DayOfWeek.Thursday },
                    StartTime = DateTimeOffset.Now.AddHours(-3),
                    EndTime = DateTimeOffset.Now.AddHours(-2)
                });
                section1.AddSectionMeeting(new SectionMeeting("235",
                    section1.Id,
                    "LEC",
                    section1.StartDate,
                    DateTime.Today.AddDays(60),
                    "W")
                {
                    Days = new List<DayOfWeek>() { DayOfWeek.Tuesday },
                    StartTime = null,
                    EndTime = null
                });

                var section2 = new Section("124",
                    "7715",
                    "01",
                    DateTime.Today.AddDays(30),
                    3m,
                    null,
                    "Section 2",
                    "IN",
                    new List<OfferingDepartment>() { new OfferingDepartment("HIST") },
                    new List<string>() { "100" },
                    "UG",
                    new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-60)) })
                {
                    TermId = regTerms.ElementAt(0).Code
                };

                section2.AddSectionMeeting(new SectionMeeting("236",
                    section2.Id,
                    "LEC",
                    section2.StartDate,
                    DateTime.Today.AddDays(60),
                    "W")
                {
                    Days = new List<DayOfWeek>() { DayOfWeek.Tuesday },
                    StartTime = null,
                    EndTime = null
                });
                var dpSections = new List<Section>() { section1, section2 };

                degreePlan.AddTerm(section1.TermId);
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, dpSections, new List<AcademicCredit>(), requirements, RuleResults);

                // Assert -- Each planned course has a time conflict message reporting other course
                foreach (var termCode in degreePlan.TermIds)
                {
                    var plannedCourses = degreePlan.GetPlannedCourses(termCode);
                    foreach (var item in plannedCourses)
                    {
                        var message = item.Warnings.Where(m => m.Type == PlannedCourseWarningType.TimeConflict);
                        Assert.IsTrue(message.Count() == 0);
                    }
                }
            }

            /// <summary>
            /// Section 1 meets Tuesdays TBD, Thursdays 4am-5am
            /// Section 2 meets Tuesdays TBD
            /// These sections should not show conflicts
            /// </summary>
            [TestMethod]
            public void DegreePlan_CheckForConflicts_CheckTimeConflicts_CheckIfSectionsOverlap_academic_credits_and_planned_courses_have_same_day_but_no_times_and_first_section_has_another_meeting()
            {
                var credit1 = new AcademicCredit("123")
                {
                    TermCode = regTerms.ElementAt(0).Code,
                    SectionId = "123"
                };
                var dpCredits = new List<AcademicCredit>() { credit1 };

                var section1 = new Section("123",
                    "7714",
                    "01",
                    DateTime.Today.AddDays(30),
                    3m,
                    null,
                    "Section 1",
                    "IN",
                    new List<OfferingDepartment>() { new OfferingDepartment("MATH") },
                    new List<string>() { "100" },
                    "UG",
                    new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-60)) })
                {
                    TermId = regTerms.ElementAt(0).Code
                };
                section1.AddSectionMeeting(new SectionMeeting("234",
                    section1.Id,
                    "LEC",
                    section1.StartDate,
                    DateTime.Today.AddDays(60),
                    "W")
                {
                    Days = new List<DayOfWeek>() { DayOfWeek.Thursday },
                    StartTime = DateTimeOffset.Now.AddHours(-3),
                    EndTime = DateTimeOffset.Now.AddHours(-2)
                });
                section1.AddSectionMeeting(new SectionMeeting("235",
                    section1.Id,
                    "LEC",
                    section1.StartDate,
                    DateTime.Today.AddDays(60),
                    "W")
                {
                    Days = new List<DayOfWeek>() { DayOfWeek.Tuesday },
                    StartTime = null,
                    EndTime = null
                });

                var section2 = new Section("124",
                    "7715",
                    "01",
                    DateTime.Today.AddDays(30),
                    3m,
                    null,
                    "Section 2",
                    "IN",
                    new List<OfferingDepartment>() { new OfferingDepartment("HIST") },
                    new List<string>() { "100" },
                    "UG",
                    new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-60)) })
                {
                    TermId = regTerms.ElementAt(0).Code
                };

                section2.AddSectionMeeting(new SectionMeeting("236",
                    section2.Id,
                    "LEC",
                    section2.StartDate,
                    DateTime.Today.AddDays(60),
                    "W")
                {
                    Days = new List<DayOfWeek>() { DayOfWeek.Tuesday },
                    StartTime = null,
                    EndTime = null
                });
                var dpSections = new List<Section>() { section1, section2 };

                degreePlan.AddTerm(section1.TermId);
                degreePlan.AddCourse(new PlannedCourse(section2.CourseId, section2.Id), section2.TermId);
                degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, dpSections, new List<AcademicCredit>(), requirements, RuleResults);

                // Assert -- Each planned course has a time conflict message reporting other course
                foreach (var termCode in degreePlan.TermIds)
                {
                    var plannedCourses = degreePlan.GetPlannedCourses(termCode);
                    foreach (var item in plannedCourses)
                    {
                        var message = item.Warnings.Where(m => m.Type == PlannedCourseWarningType.TimeConflict);
                        Assert.IsTrue(message.Count() == 0);
                    }
                }
            }


        }
    }

    [TestClass]
    public class DegreePlanCheckTimeConflicts_WithPrimarySectionMeetings
    {

        private string personId;
        private DegreePlan degreePlan;
        private PlannedCourse plannedCourse1;
        private PlannedCourse plannedCourse2;
        private PlannedCourse plannedCourse3;
        List<Course> allCourses;
        List<Section> allSections;
        List<Term> allTerms;
        List<Term> regTerms;
        IEnumerable<AcademicCredit> credits;
        List<Requirement> requirements = new List<Requirement>();
        IEnumerable<RuleResult> RuleResults;
        ILogger logger;

        [TestInitialize]
        public async void Initialize()
        {
            personId = "00004002";
            degreePlan = new DegreePlan(personId);

            allCourses = (await new TestCourseRepository().GetAsync()).ToList();
            allTerms = new TestTermRepository().Get().ToList();
            regTerms = (await new TestTermRepository().GetRegistrationTermsAsync()).ToList();
            allSections = (await new TestSectionRepository().GetRegistrationSectionsAsync(regTerms)).ToList();
            var student = new TestStudentRepository().Get(personId);
            credits = await new TestAcademicCreditRepository().GetAsync(student.AcademicCreditIds);
            RuleResults = new List<RuleResult>();
            logger = new Mock<ILogger>().Object;
        }

        [TestMethod]
        public void LogicGracefullyHandlesNoCoursesPlanned()
        {
            // Act -- Check conflicts
            degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, credits, requirements, RuleResults);

            // Assert -- No time conflict messages reported
            foreach (var termId in degreePlan.TermIds)
            {
                var plannedCourses = degreePlan.GetPlannedCourses(termId);
                var messages = plannedCourses.SelectMany(pc => pc.Warnings).Where(m => m.Type == PlannedCourseWarningType.TimeConflict);
                Assert.IsTrue(messages.Count() == 0);
            }
        }

        [TestMethod]
        public void NoMessageWhenNoPrimarySectionMeetingsOnEitherSection()
        {
            // Arrange--Add two sections to a degree plan, neither one has meeting information.
            var termId = regTerms.ElementAt(0).Code;
            var courseId1 = "7701";
            var section1 = allSections.Where(s => s.CourseId == courseId1 && s.TermId == termId && s.Number == "01").First();
            // Make sure this section has no meetings
            Assert.IsTrue(section1.PrimarySectionMeetings.Count() == 0);
            plannedCourse1 = new PlannedCourse(course: courseId1, section: section1.Id, coursePlaceholder: null);
            degreePlan.AddCourse(plannedCourse1, termId);
            var courseId2 = "7704";
            var section2 = allSections.Where(s => s.CourseId == courseId2 && s.Number == "04").First();
            // Make sure this section has no meetings
            Assert.IsTrue(section2.PrimarySectionMeetings.Count() == 0);
            plannedCourse2 = new PlannedCourse(course: courseId2, section: section2.Id, coursePlaceholder: null);
            degreePlan.AddCourse(plannedCourse2, ""); // add as non-term to test that sections are retrieved from both term and nonterm

            // Act -- Check conflicts
            degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, credits, requirements, RuleResults);

            // Assert -- No time conflict messages reported
            foreach (var termCode in degreePlan.TermIds)
            {
                var plannedCourses = degreePlan.GetPlannedCourses(termCode);
                var messages = plannedCourses.SelectMany(pc => pc.Warnings).Where(m => m.Type == PlannedCourseWarningType.TimeConflict);
                Assert.IsTrue(messages.Count() == 0);
            }
        }

        [TestMethod]
        public void NoMessageWhenPrimarySectionMeetingsNotOnBothSections()
        {
            // Arrange--Add two sections to a degree plan, only the second one has primary section meeting information.
            var termId = regTerms.ElementAt(0).Code;
            var courseId1 = "7701";
            var section1 = allSections.Where(s => s.CourseId == courseId1 && s.TermId == termId && s.Number == "01").First();
            Assert.IsTrue(section1.PrimarySectionMeetings.Count() == 0);
            plannedCourse1 = new PlannedCourse(course: courseId1, section: section1.Id, coursePlaceholder: null);
            degreePlan.AddCourse(plannedCourse1, termId);
            var courseId2 = "7704";
            var section2 = allSections.Where(s => s.CourseId == courseId2 && s.Number == "01").First();
            //manipulate section2 to have primary section meeting info by putting what was retrieved from testRepo and then removing Meetings from 
            section2.UpdatePrimarySectionMeetings(section2.Meetings.ToList());
            //remove section meetings 
            foreach (var sectionMeeting in section2.PrimarySectionMeetings)
            {
                section2.RemoveSectionMeeting(sectionMeeting);
            }
            Assert.IsTrue(section2.Meetings.Count() == 0);
            Assert.IsTrue(section2.PrimarySectionMeetings.Count() > 0);
            Assert.IsTrue(section2.PrimarySectionMeetings.ElementAt(0).StartDate != null);
            plannedCourse2 = new PlannedCourse(course: courseId2, section: section2.Id, coursePlaceholder: null);
            degreePlan.AddCourse(plannedCourse2, ""); // add as non-term to test that sections are retrieved from both term and nonterm

            // Act -- Check conflicts
            degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, credits, requirements, RuleResults);

            // Assert -- No time conflict messages reported
            foreach (var termCode in degreePlan.TermIds)
            {
                var plannedCourses = degreePlan.GetPlannedCourses(termCode);
                var messages = plannedCourses.SelectMany(pc => pc.Warnings).Where(m => m.Type == PlannedCourseWarningType.TimeConflict);
                Assert.IsTrue(messages.Count() == 0);
            }
        }

        [TestMethod]
        public void MessageWhenTermsAreSameAndMeetingTimesOverlapForSectionMeetingAndSectionPrimaryMeeting()
        {
            // Arrange--Add two sections to a degree plan, both with same term and same meeting times. 
            //section1 have Meetings and section2 have PrimarySectionMeetings
            var termId = regTerms.ElementAt(0).Code;
            var courseId1 = "7702";
            var section1 = allSections.Where(s => s.CourseId == courseId1 && s.TermId == termId && s.Number == "01").First();
            Assert.IsTrue(section1.Meetings.Count() > 0);
            Assert.IsTrue(section1.Meetings.ElementAt(0).StartDate != null);
            plannedCourse1 = new PlannedCourse(course: courseId1, section: section1.Id, coursePlaceholder: null);
            degreePlan.AddCourse(plannedCourse1, termId);
            var courseId2 = "7704";
            var section2 = allSections.Where(s => s.CourseId == courseId2 && s.Number == "01").First();
            //manipulate section2 to have primary section meeting info by putting what was retrieved from testRepo and then removing Meetings from 
            section2.UpdatePrimarySectionMeetings(section2.Meetings.ToList());
            //remove section meetings 
            foreach (var sectionMeeting in section2.PrimarySectionMeetings)
            {
                section2.RemoveSectionMeeting(sectionMeeting);
            }
            Assert.IsTrue(section2.PrimarySectionMeetings.Count() > 0);
            Assert.IsTrue(section2.PrimarySectionMeetings.ElementAt(0).StartDate != null);
            plannedCourse2 = new PlannedCourse(course: courseId2, section: section2.Id, coursePlaceholder: null);
            degreePlan.AddCourse(plannedCourse2, termId);

            // Act -- Check conflicts
            degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, credits, requirements, RuleResults);

            // Assert -- Two time conflict messages reporting other course
            foreach (var termCode in degreePlan.TermIds)
            {
                var plannedCourses = degreePlan.GetPlannedCourses(termCode);
                foreach (var item in plannedCourses)
                {
                    // Verify that each section has a message, warning of a time conflict with the other section.
                    var message = item.Warnings.Where(m => m.Type == PlannedCourseWarningType.TimeConflict);
                    Assert.IsTrue(message.Count() == 1);
                    if (item.SectionId == section1.Id)
                    {
                        Assert.AreEqual(section2.Id, message.ElementAt(0).SectionId);
                    }
                    if (item.SectionId == section2.Id)
                    {
                        Assert.AreEqual(section1.Id, message.ElementAt(0).SectionId);
                    }
                }
            }
        }

        [TestMethod]
        public void MessageWhenTermsAreSameAndMeetingTimesOverlapWithAcadCreditsWithMeetingsAndPrimSecMtngs()
        {
            // Arrange--Add planned course with section to a degree plan, add academic credit, 
            // both with same term and same meeting times.section1 have meetings and section2 have PrimarySectionMeetings
            var termId = regTerms.ElementAt(0).Code;
            var courseId1 = "7702";
            var section1 = allSections.Where(s => s.CourseId == courseId1 && s.TermId == termId && s.Number == "01").First();
            Assert.IsTrue(section1.Meetings.Count() > 0);
            Assert.IsTrue(section1.Meetings.ElementAt(0).StartDate != null);
            plannedCourse1 = new PlannedCourse(course: courseId1, section: section1.Id, coursePlaceholder: null);
            degreePlan.AddCourse(plannedCourse1, termId);
            // Create academic credit
            var courseId2 = "7704";
            var section2 = allSections.Where(s => s.CourseId == courseId2 && s.Number == "01").First();
            //manipulate section2 to have primary section meeting info by putting what was retrieved from testRepo and then removing Meetings from 
            section2.UpdatePrimarySectionMeetings(section2.Meetings.ToList());
            //remove section meetings 
            foreach (var sectionMeeting in section2.PrimarySectionMeetings)
            {
                section2.RemoveSectionMeeting(sectionMeeting);
            }
            var course2 = allCourses.Where(c => c.Id == section2.CourseId).First();
            Assert.IsTrue(section2.PrimarySectionMeetings.Count() > 0);
            Assert.IsTrue(section2.PrimarySectionMeetings.ElementAt(0).StartDate != null);
            var acadCredit = new AcademicCredit("01", course2, section2.Id);
            acadCredit.TermCode = termId;
            var acadCredits = new List<AcademicCredit>() { acadCredit };

            // Act -- Check conflicts
            degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, acadCredits, requirements, RuleResults);

            // Assert -- One time conflict messages reporting acad credit
            foreach (var termCode in degreePlan.TermIds)
            {
                var plannedCourses = degreePlan.GetPlannedCourses(termCode);
                foreach (var item in plannedCourses)
                {
                    // Verify that planned section has a message, warning of a time conflict with the acad credit section.
                    var message = item.Warnings.Where(m => m.Type == PlannedCourseWarningType.TimeConflict);
                    Assert.IsTrue(message.Count() == 1);
                    if (item.SectionId == section1.Id)
                    {
                        Assert.AreEqual(section2.Id, message.ElementAt(0).SectionId);
                    }
                }
            }
        }

        [TestMethod]
        public void NoMessageWhenSectionDatesDontOverlapWithMtngsAndPrimSecMtngs()
        {
            // Arrange--Add two sections to a degree plan, same meeting time on Meetings and PrimarySectionMeetings properties, different terms and dates
            var termId = regTerms.ElementAt(0).Code;
            var courseId1 = "7702";
            var section1 = allSections.Where(s => s.CourseId == courseId1 && s.TermId == termId && s.Number == "01").First();
            Assert.IsTrue(section1.Meetings.Count() > 0);
            Assert.IsTrue(section1.Meetings.ElementAt(0).StartDate != null);
            plannedCourse1 = new PlannedCourse(courseId1, section1.Id);
            degreePlan.AddCourse(plannedCourse1, termId);
            var courseId2 = "7704";
            var section2 = allSections.Where(s => s.CourseId == courseId2 && s.TermId == termId && s.Number == "01").First();
            //manipulate section2 to have primary section meeting info by putting what was retrieved from testRepo and then removing Meetings from 
            section2.UpdatePrimarySectionMeetings(section2.Meetings.ToList());
            //remove section meetings 
            foreach (var sectionMeeting in section2.PrimarySectionMeetings)
            {
                section2.RemoveSectionMeeting(sectionMeeting);
            }
            Assert.IsTrue(section2.PrimarySectionMeetings.Count() > 0);
            Assert.IsTrue(section2.PrimarySectionMeetings.ElementAt(0).StartDate != null);
            plannedCourse2 = new PlannedCourse(courseId2, section2.Id);
            degreePlan.AddCourse(plannedCourse2, termId);

            // Act -- Check conflicts
            degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, credits, requirements, RuleResults);

            // Assert -- Two time conflict messages reporting other course
            foreach (var termCode in degreePlan.TermIds)
            {
                var plannedCourses = degreePlan.GetPlannedCourses(termCode);
                foreach (var item in plannedCourses)
                {
                    // Verify that each section has a message, warning of a time conflict with the other section.
                    var message = item.Warnings.Where(m => m.Type == PlannedCourseWarningType.TimeConflict);
                    Assert.IsTrue(message.Count() == 1);
                    if (item.SectionId == section1.Id)
                    {
                        Assert.AreEqual(section2.Id, message.ElementAt(0).SectionId);
                    }
                    if (item.SectionId == section2.Id)
                    {
                        Assert.AreEqual(section1.Id, message.ElementAt(0).SectionId);
                    }
                }
            }
        }

        [TestMethod]
        public void MessagesWhenSectionDatesOverlapAndTimesConflictWithMtngsAndPrimSecMtngs()
        {
            // Arrange--Add two sections to a degree plan, overlapping start/end dates and times
            //with section1 Meetings collection and section2 have PrimarySectionMeetings collection
            var termId = regTerms.ElementAt(0).Code;
            var courseId1 = "7710";
            var section1 = allSections.Where(s => s.CourseId == courseId1 && s.Number == "04").First();
            Assert.IsTrue(section1.Meetings.Count() > 0);
            Assert.IsTrue(section1.Meetings.ElementAt(0).StartDate != null);
            plannedCourse1 = new PlannedCourse(courseId1, section1.Id);
            degreePlan.AddCourse(plannedCourse1, termId);
            var courseId2 = "7711";
            var section2 = allSections.Where(s => s.CourseId == courseId2 && s.Number == "04").First();
            //manipulate section2 to have primary section meeting info by putting what was retrieved from testRepo and then removing Meetings from 
            section2.UpdatePrimarySectionMeetings(section2.Meetings.ToList());
            //remove section meetings 
            foreach (var sectionMeeting in section2.PrimarySectionMeetings)
            {
                section2.RemoveSectionMeeting(sectionMeeting);
            }
            Assert.IsTrue(section2.PrimarySectionMeetings.Count() > 0);
            Assert.IsTrue(section2.PrimarySectionMeetings.ElementAt(0).StartDate != null);
            plannedCourse2 = new PlannedCourse(courseId2, section2.Id);
            degreePlan.AddCourse(plannedCourse2, termId);

            // Act -- Check conflicts
            degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, credits, requirements, RuleResults);

            // Assert -- Two time conflict messages reporting other course
            foreach (var termCode in degreePlan.TermIds)
            {
                var plannedCourses = degreePlan.GetPlannedCourses(termCode);
                foreach (var item in plannedCourses)
                {
                    // Find the time conflict message and verify warning of a time conflict with the other section.
                    var message = item.Warnings.Where(m => m.Type == PlannedCourseWarningType.TimeConflict);
                    Assert.IsTrue(message.Count() == 1);
                    if (item.SectionId == section1.Id)
                    {
                        Assert.AreEqual(section2.Id, message.ElementAt(0).SectionId);
                    }
                    if (item.SectionId == section2.Id)
                    {
                        Assert.AreEqual(section1.Id, message.ElementAt(0).SectionId);
                    }
                }
            }
        }


        [TestMethod]
        public void MessagesWhenDatesOverlapAndTimesConflictWithAcadCreditsWithBothPrimsecMtngs()
        {
            // Arrange--Add a section to a degree plan
            var termId = regTerms.ElementAt(0).Code;
            var courseId1 = "7710";
            var section1 = allSections.Where(s => s.CourseId == courseId1 && s.Number == "04").First();
            //manipulate section1 to have primary section meeting info by putting what was retrieved from testRepo and then removing Meetings from 
            section1.UpdatePrimarySectionMeetings(section1.Meetings.ToList());
            //remove section meetings 
            foreach (var sectionMeeting in section1.PrimarySectionMeetings)
            {
                section1.RemoveSectionMeeting(sectionMeeting);
            }
            Assert.IsTrue(section1.PrimarySectionMeetings.Count() > 0);
            Assert.IsTrue(section1.PrimarySectionMeetings.ElementAt(0).StartDate != null);
            plannedCourse1 = new PlannedCourse(courseId1, section1.Id);
            degreePlan.AddCourse(plannedCourse1, termId);
            // Add acad credit with overlapping start/end dates and times
            var courseId2 = "7711";
            var section2 = allSections.Where(s => s.CourseId == courseId2 && s.Number == "04").First();
            //manipulate section2 to have primary section meeting info by putting what was retrieved from testRepo and then removing Meetings from 
            section2.UpdatePrimarySectionMeetings(section2.Meetings.ToList());
            //remove section meetings 
            foreach (var sectionMeeting in section2.PrimarySectionMeetings)
            {
                section2.RemoveSectionMeeting(sectionMeeting);
            }
            var course2 = allCourses.Where(c => c.Id == section2.CourseId).First();

            Assert.IsTrue(section2.PrimarySectionMeetings.Count() > 0);
            Assert.IsTrue(section2.PrimarySectionMeetings.ElementAt(0).StartDate != null);
            var acadCredit = new AcademicCredit("01", course2, section2.Id);
            acadCredit.TermCode = termId;
            var acadCredits = new List<AcademicCredit>() { acadCredit };

            // Act -- Check conflicts
            degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, acadCredits, requirements, RuleResults);

            // Assert -- One time conflict messages reporting acad credit
            foreach (var termCode in degreePlan.TermIds)
            {
                var plannedCourses = degreePlan.GetPlannedCourses(termCode);
                foreach (var item in plannedCourses)
                {
                    // Find the time conflict message and verify warning of a time conflict with the acad credit section.
                    var message = item.Warnings.Where(m => m.Type == PlannedCourseWarningType.TimeConflict);
                    Assert.IsTrue(message.Count() == 1);
                    if (item.SectionId == section1.Id)
                    {
                        Assert.AreEqual(section2.Id, message.ElementAt(0).SectionId);
                    }
                }
            }
        }
    }

    [TestClass]
    public class DegreePlanGetPlannedCoursesForValidation
    {
        string personId;
        int degreePlanId;
        DegreePlan degreePlan;
        IEnumerable<Term> regTerms = new List<Term>();
        IEnumerable<Term> allTerms = new List<Term>();
        IEnumerable<Course> allCourses = new List<Course>();
        IEnumerable<AcademicCredit> academicCredits = new List<AcademicCredit>();
        IEnumerable<Section> allSections = new List<Section>();


        [TestInitialize]
        public async void Initialize()
        {
            personId = "0000693";
            degreePlanId = 1;
            degreePlan = new DegreePlan(degreePlanId, personId, 1);
            regTerms = await new TestTermRepository().GetRegistrationTermsAsync();
            allSections = await new TestSectionRepository().GetRegistrationSectionsAsync(regTerms);
        }

        [TestCleanup]
        public void CleanUp()
        {
            degreePlan = null;
        }

        [TestMethod]
        public async Task ReturnsEmptyListWhenNoCoursesOnPlan()
        {
            // Action -- call method to get all planned courses against an empty degree plan
            var plannedTerms = degreePlan.GetPlannedCoursesForValidation(null, new TestTermRepository().Get(), await new TestTermRepository().GetRegistrationTermsAsync(), new List<AcademicCredit>(), new List<Section>());

            // Assert -- Returns empty list
            Assert.AreEqual(0, plannedTerms.Count());
        }

        [TestMethod]
        public async Task ReturnsAllPlannedCoursesForFutureTerm()
        {
            // Add two courses for 2017/SP term. This term is manipulated in the TestTermRepository
            // so that it will never expire, and this will be true regardless of the today's date.
            string foreverTermId = "2017/SP";
            degreePlan.AddTerm(foreverTermId);
            var course1Id = "139";
            var plannedCourse1 = new PlannedCourse(course1Id);
            degreePlan.AddCourse(plannedCourse1, foreverTermId);
            var course2Id = "42";
            var plannedCourse2 = new PlannedCourse(course2Id);
            degreePlan.AddCourse(plannedCourse2, foreverTermId);

            // Action -- call method to get all planned courses. No date or credits to filter results.
            // current date used as earliest term start date because no reg term provided.
            var plannedTerms = degreePlan.GetPlannedCoursesForValidation(null, new TestTermRepository().Get(), await new TestTermRepository().GetRegistrationTermsAsync(), new List<AcademicCredit>(), new List<Section>());
            var plannedCourses = plannedTerms.SelectMany(p => p.Value);
            // Assert -- two courses for the "forever" term will always be returned because that term initialized in test repo with future dates
            // With no term and no date in the call method, the returned courses must be in a term later than the current date.
            Assert.AreEqual(2, plannedCourses.Count());
            Assert.AreEqual(course1Id, plannedCourses.Where(pc => pc.CourseId == course1Id).FirstOrDefault().CourseId);
            Assert.AreEqual(course2Id, plannedCourses.Where(pc => pc.CourseId == course2Id).FirstOrDefault().CourseId);
        }

        [TestMethod]
        public async Task ReturnsAllPlannedCoursesForFutureTermExcludesCoursePlaceholders()
        {
            // Add two courses for 2017/SP term. This term is manipulated in the TestTermRepository
            // so that it will never expire, and this will be true regardless of the today's date.
            string foreverTermId = "2017/SP";
            degreePlan.AddTerm(foreverTermId);
            var course1Id = "139";
            var plannedCourse1 = new PlannedCourse(course1Id, coursePlaceholder: null);
            degreePlan.AddCourse(plannedCourse1, foreverTermId);
            var course2Id = "42";
            var plannedCourse2 = new PlannedCourse(course2Id, coursePlaceholder: null);
            degreePlan.AddCourse(plannedCourse2, foreverTermId);

            //add course placeholder that should be excluded
            var plannedCourse3 = new PlannedCourse(course: null, section: null, coursePlaceholder: "abc");
            degreePlan.AddCourse(plannedCourse3, foreverTermId);

            // Action -- call method to get all planned courses. No date or credits to filter results.
            // current date used as earliest term start date because no reg term provided.
            var plannedTerms = degreePlan.GetPlannedCoursesForValidation(null, new TestTermRepository().Get(), await new TestTermRepository().GetRegistrationTermsAsync(), new List<AcademicCredit>(), new List<Section>());
            var plannedCourses = plannedTerms.SelectMany(p => p.Value);
            // Assert -- two courses for the "forever" term will always be returned because that term initialized in test repo with future dates
            // With no term and no date in the call method, the returned courses must be in a term later than the current date.
            Assert.AreEqual(2, plannedCourses.Count());
            Assert.AreEqual(course1Id, plannedCourses.Where(pc => pc.CourseId == course1Id).FirstOrDefault().CourseId);
            Assert.AreEqual(course2Id, plannedCourses.Where(pc => pc.CourseId == course2Id).FirstOrDefault().CourseId);
        }

        [TestMethod]
        public void ReturnsPlannedCoursesIncludingThoseInRegistrationTerm()
        {
            // Arrange--add courses two two terms in degree plan
            // 2017/SP is manipulated in the TestTermRepository so that it will never expire.
            string foreverTermId = "2017/SP";
            degreePlan.AddTerm(foreverTermId);
            var course1Id = "139";
            var plannedCourse1 = new PlannedCourse(course1Id);
            degreePlan.AddCourse(plannedCourse1, foreverTermId);
            var course2Id = "42";
            var plannedCourse2 = new PlannedCourse(course2Id);
            degreePlan.AddCourse(plannedCourse2, foreverTermId);
            // 2012/FA is registration term, add courses for that term
            var termId = "2012/FA";
            var course3Id = "110";
            var plannedCourse3 = new PlannedCourse(course3Id);
            degreePlan.AddCourse(plannedCourse3, termId);
            var course4Id = "21";
            var plannedCourse4 = new PlannedCourse(course4Id);
            degreePlan.AddCourse(plannedCourse4, termId);

            // This will be the registration term for this test
            var regTermId = "2012/FA";
            regTerms = new TestTermRepository().Get().Where(t => t.Code == regTermId);

            // Action -- call method to get all planned courses. Reg term provides minimum start date of terms to include.
            var plannedTerms = degreePlan.GetPlannedCoursesForValidation(null, new TestTermRepository().Get(), regTerms, new List<AcademicCredit>(), new List<Section>());

            // Assert -- two courses for the "forever" term and the two from the registration term found
            var plannedCourses = plannedTerms.Where(p => p.Key == foreverTermId).SelectMany(p => p.Value);
            Assert.AreEqual(2, plannedCourses.Count());
            Assert.AreEqual(course1Id, plannedCourses.Where(pc => pc.CourseId == course1Id).FirstOrDefault().CourseId);
            Assert.AreEqual(course2Id, plannedCourses.Where(pc => pc.CourseId == course2Id).FirstOrDefault().CourseId);
            plannedCourses = plannedTerms.Where(p => p.Key == regTermId).SelectMany(p => p.Value);
            Assert.AreEqual(2, plannedCourses.Count());
            Assert.AreEqual(course3Id, plannedCourses.Where(pc => pc.CourseId == course3Id).FirstOrDefault().CourseId);
            Assert.AreEqual(course4Id, plannedCourses.Where(pc => pc.CourseId == course4Id).FirstOrDefault().CourseId);
        }

        [TestMethod]
        public void OmitsPlannedCoursesPriorToRegistrationTerm()
        {
            // Arrange
            // 2017/SP is manipulated in the TestTermRepository so that it will never expire.
            string foreverTermId = "2017/SP";
            degreePlan.AddTerm(foreverTermId);
            var course1Id = "139";
            var plannedCourse1 = new PlannedCourse(course1Id);
            degreePlan.AddCourse(plannedCourse1, foreverTermId);
            var course2Id = "42";
            var plannedCourse2 = new PlannedCourse(course2Id);
            degreePlan.AddCourse(plannedCourse2, foreverTermId);
            // 2012/FA is registration term, add courses for that term
            var termId = "2012/FA";
            var course3Id = "110";
            var plannedCourse3 = new PlannedCourse(course3Id);
            degreePlan.AddCourse(plannedCourse3, termId);
            var course4Id = "21";
            var plannedCourse4 = new PlannedCourse(course4Id);
            degreePlan.AddCourse(plannedCourse4, termId);

            // This will be the registration term for this test
            var regTermId = "2013/SP";
            regTerms = new TestTermRepository().Get().Where(t => t.Code == regTermId);

            // Action -- call method to get all planned courses. 
            var plannedTerms = degreePlan.GetPlannedCoursesForValidation(null, new TestTermRepository().Get(), regTerms, new List<AcademicCredit>(), new List<Section>());

            // Assert -- two courses for the "forever" term, but two in 2012/FA omitted because they are earlier than min start date taken from 2013/SP reg term
            var plannedCourses = plannedTerms.Where(p => p.Key == termId).SelectMany(p => p.Value);
            Assert.AreEqual(0, plannedCourses.Count());
            plannedCourses = plannedTerms.Where(p => p.Key == foreverTermId).SelectMany(p => p.Value);
            Assert.AreEqual(2, plannedCourses.Count());
            Assert.AreEqual(course1Id, plannedCourses.Where(pc => pc.CourseId == course1Id).FirstOrDefault().CourseId);
            Assert.AreEqual(course2Id, plannedCourses.Where(pc => pc.CourseId == course2Id).FirstOrDefault().CourseId);
        }

        [TestMethod]
        public void OmitsPlannedCoursesEndingAfterCompletedByDate()
        {
            // Arrange
            // 2017/SP is manipulated in the TestTermRepository so that it will never expire.
            string foreverTermId = "2017/SP";
            degreePlan.AddTerm(foreverTermId);
            var course1Id = "139";
            var plannedCourse1 = new PlannedCourse(course1Id);
            degreePlan.AddCourse(plannedCourse1, foreverTermId);
            var course2Id = "42";
            var plannedCourse2 = new PlannedCourse(course2Id);
            degreePlan.AddCourse(plannedCourse2, foreverTermId);
            // 2012/FA is registration term, add courses for that term
            var termId = "2012/FA";
            var course3Id = "110";
            var plannedCourse3 = new PlannedCourse(course3Id);
            degreePlan.AddCourse(plannedCourse3, termId);
            var course4Id = "21";
            var plannedCourse4 = new PlannedCourse(course4Id);
            degreePlan.AddCourse(plannedCourse4, termId);

            // Including this registration term sets min date to the start date of that term
            var regTermId = "2012/FA";
            regTerms = new TestTermRepository().Get().Where(t => t.Code == regTermId);
            // BUT setting completed by date to 20 days before the end of the FOREVER makes that term's courses
            // ineligible because they will not be completed by the complete by date.
            var foreverTerm = new TestTermRepository().Get().Where(t => t.Code == foreverTermId).First();
            var completedByDate = foreverTerm.StartDate.AddDays(-20);

            // Action -- call method to get all planned courses. 
            var plannedTerms = degreePlan.GetPlannedCoursesForValidation(completedByDate, new TestTermRepository().Get(), regTerms, new List<AcademicCredit>(), new List<Section>());

            // Assert -- only two in 2012/FA term included because the forever term not complete in time
            Assert.AreEqual(1, plannedTerms.Count());
            var plannedCourses = plannedTerms[termId];
            Assert.AreEqual(2, plannedCourses.Count());
            Assert.AreEqual(course3Id, plannedCourses.Where(pc => pc.CourseId == course3Id).FirstOrDefault().CourseId);
            Assert.AreEqual(course4Id, plannedCourses.Where(pc => pc.CourseId == course4Id).FirstOrDefault().CourseId);
        }

        [TestMethod]
        public async Task OmitsPlannedItemsInCredits()
        {
            // Add two courses for this term
            var termId = "2012/FA";
            var course3Id = "110";
            var plannedCourse3 = new PlannedCourse(course3Id);
            degreePlan.AddCourse(plannedCourse3, termId);
            var course4Id = "21";
            var plannedCourse4 = new PlannedCourse(course4Id);
            degreePlan.AddCourse(plannedCourse4, termId);
            // This will be the registration term for this test so that credits will be checked
            var regTermId = "2012/FA";
            regTerms = new TestTermRepository().Get().Where(t => t.Code == regTermId);
            // Build academic credit that contains an item in the plan for the same term
            var course1 = await new TestCourseRepository().GetAsync(course3Id);
            var section1Id = (await new TestSectionRepository().GetRegistrationSectionsAsync(regTerms)).Where(s => s.CourseId == course3Id).Select(s => s.Id).First();
            var academicCredit1 = new AcademicCredit("01", course1, section1Id);
            academicCredit1.TermCode = regTermId;

            // Action -- call method to get all planned courses. 
            var plannedTerms = degreePlan.GetPlannedCoursesForValidation(null, new TestTermRepository().Get(), regTerms, new List<AcademicCredit>() { academicCredit1 }, new List<Section>());
            var plannedCourses = plannedTerms.Where(p => p.Key == termId).SelectMany(p => p.Value);
            // Assert -- only the one course not found in the acad credit included
            Assert.AreEqual(1, plannedCourses.Count());
            Assert.AreEqual(course4Id, plannedCourses.Where(pc => pc.CourseId == course4Id).FirstOrDefault().CourseId);
        }

        [TestMethod]
        public async Task CompletesGracefullyWhenNoAcademicCredits()
        {
            // Add two courses for this term
            var termId = "2012/FA";
            var course3Id = "110";
            var plannedCourse3 = new PlannedCourse(course3Id);
            degreePlan.AddCourse(plannedCourse3, termId);
            var course4Id = "21";
            var plannedCourse4 = new PlannedCourse(course4Id);
            degreePlan.AddCourse(plannedCourse4, termId);
            // This will be the registration term for this test so that credits will be checked
            var regTermId = "2012/FA";
            regTerms = new TestTermRepository().Get().Where(t => t.Code == regTermId);
            // Build academic credit that contains an item in the plan for the same term
            var course1 = await new TestCourseRepository().GetAsync(course3Id);
            var section1Id = (await new TestSectionRepository().GetRegistrationSectionsAsync(regTerms)).Where(s => s.CourseId == course3Id).Select(s => s.Id).First();
            var academicCredit1 = new AcademicCredit("01", course1, section1Id);
            academicCredit1.TermCode = regTermId;

            // Action -- call method to get all planned courses. 
            var plannedTerms = degreePlan.GetPlannedCoursesForValidation(null, new TestTermRepository().Get(), regTerms, null, new List<Section>());
            var plannedCourses = plannedTerms.Where(p => p.Key == termId).SelectMany(p => p.Value);
            // Assert -- both planned courses returned
            Assert.AreEqual(2, plannedCourses.Count());
        }

        [TestMethod]
        public void CompletesGracefullyWhenNoncourseAcademicCredits()
        {
            // Add two courses for this term
            var termId = "2012/FA";
            var course3Id = "110";
            var plannedCourse3 = new PlannedCourse(course3Id);
            degreePlan.AddCourse(plannedCourse3, termId);
            var course4Id = "21";
            var plannedCourse4 = new PlannedCourse(course4Id);
            degreePlan.AddCourse(plannedCourse4, termId);
            // This will be the registration term for this test so that credits will be checked
            var regTermId = "2012/FA";
            regTerms = new TestTermRepository().Get().Where(t => t.Code == regTermId);
            // Build academic credit that contains a noncourse item in the plan for the same term
            // var course1 = new TestCourseRepository().Get("186");
            //var section1Id = new TestSectionRepository().GetRegistrationSections(regTerms).Where(s => s.CourseId == "186").Select(s => s.Id).First();
            var academicCredit1 = new AcademicCredit("01");
            academicCredit1.TermCode = regTermId;

            // Action -- call method to get all planned courses. 
            var plannedTerms = degreePlan.GetPlannedCoursesForValidation(null, new TestTermRepository().Get(), regTerms, new List<AcademicCredit>() { academicCredit1 }, new List<Section>());
            var plannedCourses = plannedTerms.Where(p => p.Key == termId).SelectMany(p => p.Value);
            // Assert -- both planned courses returned
            Assert.AreEqual(2, plannedCourses.Count());
        }

        [TestMethod]
        public async Task OmitsPlannedItemsInCreditsOnlyOnce()
        {
            // Plan three entries for the same course
            var termId = "2012/FA";
            var courseId = "110";
            var sectionIds = (await new TestSectionRepository().GetRegistrationSectionsAsync(regTerms)).Where(s => s.CourseId == courseId).Select(s => s.Id);
            var plannedCourse2 = new PlannedCourse(courseId);
            degreePlan.AddCourse(plannedCourse2, termId);
            var plannedCourse3 = new PlannedCourse(courseId, sectionIds.ElementAt(1));
            degreePlan.AddCourse(plannedCourse3, termId);
            var plannedCourse4 = new PlannedCourse(courseId, sectionIds.ElementAt(0));
            degreePlan.AddCourse(plannedCourse4, termId);
            // This will be the registration term for this test so that credits will be checked
            regTerms = new TestTermRepository().Get().Where(t => t.Code == termId);
            // Build two academic credits that contains the same course
            // This one will be matched up with the first planned course
            var course1 = await new TestCourseRepository().GetAsync(courseId);
            var academicCredit1 = new AcademicCredit("01", course1, sectionIds.ElementAt(0));
            academicCredit1.TermCode = termId;
            // This one will not be matched up with the third planned course because section doesn't match with the second
            var course2 = await new TestCourseRepository().GetAsync(courseId);
            var academicCredit2 = new AcademicCredit("02", course2, sectionIds.ElementAt(2));
            academicCredit2.TermCode = termId;

            // Action -- call method to get all planned courses. 
            var plannedTerms = degreePlan.GetPlannedCoursesForValidation(null, new TestTermRepository().Get(), regTerms, new List<AcademicCredit>() { academicCredit1, academicCredit2 }, new List<Section>());
            var plannedCourses = plannedTerms.Where(p => p.Key == termId).SelectMany(p => p.Value);

            // Assert -- one is included... two were omitted because of the acad credit
            Assert.AreEqual(1, plannedCourses.Count());
            Assert.AreEqual(plannedCourse3, plannedCourses.ElementAt(0));
        }

        [TestMethod]
        public async Task IncludesNonTermCourses()
        {
            // add nonterm course
            var courseId = "7704";
            var sectionId = (await new TestSectionRepository().GetRegistrationSectionsAsync(regTerms)).Where(s => s.CourseId == courseId && s.Number == "04").Select(s => s.Id).First();
            var plannedCourse1 = new PlannedCourse(courseId, sectionId);
            degreePlan.AddCourse(plannedCourse1, "");

            // Action -- call method to get all planned courses. 
            var plannedTerms = degreePlan.GetPlannedCoursesForValidation(null, new TestTermRepository().Get(), regTerms, new List<AcademicCredit>(), allSections);
            var plannedCourses = plannedTerms.Where(p => p.Key == "NONTERM").SelectMany(p => p.Value);

            // Assert -- nonterm planned course included
            Assert.AreEqual(1, plannedCourses.Count());
        }

        [TestMethod]
        public async Task ExcludesNonTermCoursesNotDoneByCompletedByDate()
        {
            // add nonterm course
            var regTerms = await new TestTermRepository().GetRegistrationTermsAsync();
            var courseId = "7704";
            var sectionId = (await new TestSectionRepository().GetRegistrationSectionsAsync(regTerms)).Where(s => s.CourseId == courseId && s.Number == "04").Select(s => s.Id).First();
            var plannedCourse1 = new PlannedCourse(courseId, sectionId);
            degreePlan.AddCourse(plannedCourse1, "");

            // Action -- call method to get all planned courses. Give early start date so nonterm course won't be selected
            var plannedTerms = degreePlan.GetPlannedCoursesForValidation(DateTime.Today.AddYears(-10), new TestTermRepository().Get(), regTerms, new List<AcademicCredit>(), new List<Section>());
            var plannedCourses = plannedTerms.Where(p => p.Key == "NONTERM").SelectMany(p => p.Value);

            // Assert -- nonterm planned course included
            Assert.AreEqual(0, plannedCourses.Count());
        }

        [TestMethod]
        public async Task OmitsNonTermCoursesFoundInAcademicCredits()
        {
            // Add nonterm course
            var courseId = "7704";
            var termId = "2012/FA";
            var sectionId = (await new TestSectionRepository().GetRegistrationSectionsAsync(regTerms)).Where(s => s.CourseId == courseId && s.Number == "04").Select(s => s.Id).First();
            var plannedCourse1 = new PlannedCourse(courseId, sectionId);
            degreePlan.AddCourse(plannedCourse1, "");
            // This academic credit will be matched up with the first planned course
            var course1 = await new TestCourseRepository().GetAsync(courseId);
            var academicCredit1 = new AcademicCredit("01", course1, sectionId);
            academicCredit1.TermCode = termId;

            // Action -- call method to get all planned courses. 
            var plannedTerms = degreePlan.GetPlannedCoursesForValidation(null, new TestTermRepository().Get(), regTerms, new List<AcademicCredit>() { academicCredit1 }, new List<Section>());
            var plannedCourses = plannedTerms.Where(p => p.Key == "NONTERM").SelectMany(p => p.Value);

            // Assert -- one is included... two were omitted because of the acad credit
            Assert.AreEqual(0, plannedCourses.Count());
        }
    }

    [TestClass]
    public class DegreePlanGetCoursesForValidation
    {
        string personId;
        int degreePlanId;
        DegreePlan degreePlan;
        IEnumerable<Term> regTerms = new List<Term>();
        IEnumerable<Term> allTerms = new List<Term>();
        IEnumerable<Course> allCourses = new List<Course>();
        IEnumerable<AcademicCredit> academicCredits = new List<AcademicCredit>();


        [TestInitialize]
        public async void Initialize()
        {
            personId = "0000693";
            degreePlanId = 1;
            degreePlan = new DegreePlan(degreePlanId, personId, 1);
            allCourses = await new TestCourseRepository().GetAsync();
        }

        [TestCleanup]
        public void CleanUp()
        {
            degreePlan = null;
        }

        [TestMethod]
        public async Task ReturnsEmptyListWhenNoItemsFound()
        {
            // Action -- call method to get all planned courses. No date or credits to filter results.
            // current date used as earliest term start date because no reg term provided.
            var plannedCourses = degreePlan.GetCoursesForValidation(new TestTermRepository().Get(), await new TestTermRepository().GetRegistrationTermsAsync(), new List<AcademicCredit>(), allCourses);

            // Assert -- Empty list returned
            Assert.AreEqual(0, plannedCourses.Count());
        }

        [TestMethod]
        public void ReturnsIEnumerableListOfCoursesExcludingPastPlanned()
        {
            // Arrange--add courses to two terms in degree plan
            string foreverTermId = "2017/SP";
            degreePlan.AddTerm(foreverTermId);
            var course1Id = "139";
            var plannedCourse1 = new PlannedCourse(course1Id);
            degreePlan.AddCourse(plannedCourse1, foreverTermId);
            var course2Id = "42";
            var plannedCourse2 = new PlannedCourse(course2Id);
            degreePlan.AddCourse(plannedCourse2, foreverTermId);
            // 2012/FA is registration term, add courses for that term
            var termId = "2012/FA";
            var course3Id = "110";
            var plannedCourse3 = new PlannedCourse(course3Id);
            degreePlan.AddCourse(plannedCourse3, termId);
            var course4Id = "21";
            var plannedCourse4 = new PlannedCourse(course4Id);
            degreePlan.AddCourse(plannedCourse4, termId);

            // This will be the registration term for this test
            var regTermId = "2012/FA";
            regTerms = new TestTermRepository().Get().Where(t => t.Code == regTermId);

            // Action -- call method to get all planned courses. 
            var plannedCourses = degreePlan.GetCoursesForValidation(new TestTermRepository().Get(), regTerms, new List<AcademicCredit>(), allCourses);
            // Assert -- The two items returned are flattened out to a simple list of courses
            Assert.AreEqual(2, plannedCourses.Count());
            Assert.IsTrue(plannedCourses is IEnumerable<PlannedCredit>);
        }

        [TestMethod]
        public void ReturnsIEnumerableListOfEvaluationPlannedCourses()
        {
            // Arrange--add courses two two terms in degree plan
            string foreverTermId = "2017/SP";
            degreePlan.AddTerm(foreverTermId);
            var course1Id = "139";
            var plannedCourse1 = new PlannedCourse(course1Id);
            degreePlan.AddCourse(plannedCourse1, foreverTermId);
            var course2Id = "42";
            var plannedCourse2 = new PlannedCourse(course2Id);
            degreePlan.AddCourse(plannedCourse2, foreverTermId);
            // 2012/FA is registration term, add courses for that term
            var termId = "2016/FA";
            var course3Id = "110";
            var plannedCourse3 = new PlannedCourse(course3Id);
            degreePlan.AddCourse(plannedCourse3, termId);
            var course4Id = "21";
            var plannedCourse4 = new PlannedCourse(course4Id);
            degreePlan.AddCourse(plannedCourse4, termId);

            // This will be the registration term for this test
            var regTermId = "2016/FA";
            regTerms = new TestTermRepository().Get().Where(t => t.Code == regTermId);

            // Action -- call method to get all planned courses. 
            var plannedCourses = degreePlan.GetCoursesForValidation(new TestTermRepository().Get(), regTerms, new List<AcademicCredit>(), allCourses);
            // Assert -- The four items returned are flattened out to a simple list of courses
            Assert.AreEqual(4, plannedCourses.Count());
            Assert.IsTrue(plannedCourses is IEnumerable<PlannedCredit>);
        }

        [TestMethod]
        public void ReturnsIEnumerableListOfEvaluationPlannedCoursesExcludingCoursePlaceholders()
        {
            // Arrange--add courses two two terms in degree plan
            string foreverTermId = "2017/SP";
            degreePlan.AddTerm(foreverTermId);
            var course1Id = "139";
            var plannedCourse1 = new PlannedCourse(course1Id);
            degreePlan.AddCourse(plannedCourse1, foreverTermId);
            var course2Id = "42";
            var plannedCourse2 = new PlannedCourse(course2Id);
            degreePlan.AddCourse(plannedCourse2, foreverTermId);
            var coursePlaceholder1 = new PlannedCourse(course: null, coursePlaceholder: "abc");
            degreePlan.AddCourse(coursePlaceholder1, foreverTermId);

            // 2012/FA is registration term, add courses for that term
            var termId = "2016/FA";
            var course3Id = "110";
            var plannedCourse3 = new PlannedCourse(course3Id);
            degreePlan.AddCourse(plannedCourse3, termId);
            var course4Id = "21";
            var plannedCourse4 = new PlannedCourse(course4Id);
            degreePlan.AddCourse(plannedCourse4, termId);
            var coursePlaceholder2 = new PlannedCourse(course: null, coursePlaceholder: "xyz");
            degreePlan.AddCourse(coursePlaceholder2, termId);

            // This will be the registration term for this test
            var regTermId = "2016/FA";
            regTerms = new TestTermRepository().Get().Where(t => t.Code == regTermId);

            // Action -- call method to get all planned courses. 
            var plannedCourses = degreePlan.GetCoursesForValidation(new TestTermRepository().Get(), regTerms, new List<AcademicCredit>(), allCourses);
            // Assert -- The four items returned are flattened out to a simple list of courses
            Assert.AreEqual(4, plannedCourses.Count());
            Assert.IsTrue(plannedCourses is IEnumerable<PlannedCredit>);
        }
    }

    [TestClass]
    public class DegreePlanGetPrerequisites
    {
        string personId;
        int degreePlanId;
        DegreePlan degreePlan;
        IEnumerable<Term> regTerms = new List<Term>();
        IEnumerable<Term> allTerms = new List<Term>();
        IEnumerable<Course> allCourses = new List<Course>();
        IEnumerable<Section> allSections = new List<Section>();
        IEnumerable<AcademicCredit> academicCredits = new List<AcademicCredit>();

        [TestInitialize]
        public async void Initialize()
        {
            personId = "00004002";
            degreePlanId = 1;
            degreePlan = new DegreePlan(degreePlanId, personId, 1);
            allCourses = await new TestCourseRepository().GetAsync();
            allTerms = new TestTermRepository().Get();
            regTerms = await new TestTermRepository().GetRegistrationTermsAsync();
            allSections = await new TestSectionRepository().GetRegistrationSectionsAsync(regTerms);
            var student = new TestStudentRepository().Get(personId);
            academicCredits = await new TestAcademicCreditRepository().GetAsync(student.AcademicCreditIds);
        }

        [TestCleanup]
        public void CleanUp()
        {
            degreePlan = null;
        }

        [TestMethod]
        public void ReturnsMultipleRequisites()
        {
            // Arrange-- Add two courses with prereqs to 2012/FA 
            var termId1 = "2012/FA";
            var courseId1 = "186";
            var plannedCourse1 = new PlannedCourse(courseId1);
            degreePlan.AddCourse(plannedCourse1, termId1);
            var courseId2 = "87";
            var plannedCourse2 = new PlannedCourse(courseId2);
            degreePlan.AddCourse(plannedCourse2, termId1);
            var courseId3 = "139";
            var plannedCourse3 = new PlannedCourse(courseId3);
            degreePlan.AddCourse(plannedCourse3, termId1);

            // Act - Call degree plan method
            var requirementCodes = degreePlan.GetRequirementCodes(allTerms, regTerms, allCourses, academicCredits, allSections);

            // Assert - Expect two requirement codes returned
            Assert.AreEqual(2, requirementCodes.Count());
            Assert.IsTrue(requirementCodes.Contains("PREREQ1"));
            Assert.IsTrue(requirementCodes.Contains("PREREQ2"));
        }


        [TestMethod]
        public void EmptyListReturnedIfNoRequisites()
        {
            // Arrange-- Add same course twice with no prereq to 2013/FA
            var termId1 = "2013/FA";
            var plannedCourse1 = new PlannedCourse("139");
            degreePlan.AddCourse(plannedCourse1, termId1);
            // Act - Call degree plan method
            var requirementCodes = degreePlan.GetRequirementCodes(allTerms, regTerms, allCourses, academicCredits, allSections);

            // Assert - Expect empty list
            Assert.AreEqual(0, requirementCodes.Count());
        }

        [TestMethod]
        public async Task ReturnsNontermCourseRequisite()
        {
            // add nonterm course
            var courseId = "7704";
            var sectionId = (await new TestSectionRepository().GetRegistrationSectionsAsync(regTerms)).Where(s => s.CourseId == courseId && s.Number == "04").Select(s => s.Id).First();
            var plannedCourse1 = new PlannedCourse(courseId, sectionId);
            degreePlan.AddCourse(plannedCourse1, "");

            // Act - Call degree plan method
            var requirementCodes = degreePlan.GetRequirementCodes(allTerms, regTerms, allCourses, academicCredits, allSections);

            // Assert - Expect return of NONTERM term
            Assert.AreEqual(1, requirementCodes.Count());
            Assert.AreEqual("PREREQ1", requirementCodes.ElementAt(0));
        }

        [TestMethod]
        public void ReturnsListOfDistinctItems()
        {
            // Arrange-- Add same course with prereq in two separate terms
            var termId1 = "2012/FA";
            var plannedCourse1 = new PlannedCourse("186");
            degreePlan.AddCourse(plannedCourse1, termId1);
            var termId2 = "2013/SP";
            var plannedCourse2 = new PlannedCourse("186");
            degreePlan.AddCourse(plannedCourse2, termId2);

            // Act - Call degree plan method
            var requirementCodes = degreePlan.GetRequirementCodes(allTerms, regTerms, allCourses, academicCredits, allSections);

            // Assert - Expect one prerequisite code
            Assert.AreEqual(1, requirementCodes.Count());
            Assert.AreEqual("PREREQ1", requirementCodes.ElementAt(0));
        }
    }

    [TestClass]
    public class DegreePlanCheckReq_Concurrent
    {
        string personId;
        DegreePlan degreePlan;
        IEnumerable<Term> regTerms = new List<Term>();
        IEnumerable<Term> allTerms = new List<Term>();
        IEnumerable<Course> allCourses = new List<Course>();
        IEnumerable<Section> allSections = new List<Section>();
        IEnumerable<AcademicCredit> academicCredits = new List<AcademicCredit>();
        IEnumerable<Requirement> requirements = new List<Requirement>();
        IEnumerable<RuleResult> RuleResults;
        ILogger logger;

        [TestInitialize]
        public async void Initialize()
        {
            allCourses = await new TestCourseRepository().GetAsync();
            allTerms = new TestTermRepository().Get();
            regTerms = await new TestTermRepository().GetRegistrationTermsAsync();
            allSections = await new TestSectionRepository().GetRegistrationSectionsAsync(regTerms);
            personId = "00004002";
            degreePlan = new DegreePlan(personId);
            requirements = await new TestRequirementRepository().GetAsync(new List<string>() { "PREREQ1", "PREREQ2", "COREQ1", "COREQ2", "REQ1" });
            RuleResults = new List<RuleResult>();
            logger = new Mock<ILogger>().Object;
        }

        [TestCleanup]
        public void CleanUp()
        {
            degreePlan = null;
        }

        [TestMethod]
        public void WarningAddedforUnsatisfiedConcurrentRequisite()
        {
            // Arrange-- Add two courses 2012/FA - course 333 has a required concurrent coreq of course 47
            var termId1 = "2012/FA";
            var courseId1 = "333";
            var plannedCourse1 = new PlannedCourse(courseId1);
            degreePlan.AddCourse(plannedCourse1, termId1);
            var courseId2 = "139";
            var plannedCourse2 = new PlannedCourse(courseId2);
            degreePlan.AddCourse(plannedCourse2, termId1);

            // Act-- Check degree plan for conflicts, specifically prerequisites
            degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, academicCredits, requirements, RuleResults);

            // Assert
            var plannedCourse = degreePlan.GetPlannedCourses(termId1).Where(c => c.CourseId == "333").First();
            var warnings = plannedCourse.Warnings.Where(w => w.Type == PlannedCourseWarningType.UnmetRequisite);
            Assert.AreEqual(1, warnings.Count());
            Assert.AreEqual("COREQ1", warnings.ElementAt(0).Requisite.RequirementCode);
            Assert.IsTrue(warnings.ElementAt(0).Requisite.IsRequired);
            Assert.AreEqual(RequisiteCompletionOrder.Concurrent, warnings.ElementAt(0).Requisite.CompletionOrder);

        }



        [TestMethod]
        public void NoWarning_SatisfiedConcurrent_MatchingTerm()
        {
            // Arrange-- Add with reqs to 2014/FA. This course has COREQ which requires classId 47
            // A. SAME TERM
            var termId1 = "2014/FA";
            var courseId1 = "333";
            var plannedCourse1 = new PlannedCourse(courseId1);
            degreePlan.AddCourse(plannedCourse1, termId1);
            // Add planned course that satisfies coreq to 2014/FA
            var termId2 = "2014/FA";
            var courseId2 = "47";
            var plannedCourse2 = new PlannedCourse(courseId2);
            degreePlan.AddCourse(plannedCourse2, termId2);

            // Act-- Check degree plan for conflicts, specifically prerequisites
            degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, academicCredits, requirements, RuleResults);

            // Assert - no warning appears
            var plannedCourse = degreePlan.GetPlannedCourses(termId1).Where(c => c.CourseId == "333").First();
            Assert.AreEqual(0, plannedCourse.Warnings.Where(w => w.Type == PlannedCourseWarningType.UnmetRequisite).Count());
        }

        [TestMethod]
        public void NoWarning_SatisfiedConcurrent_Dates()
        {
            // Arrange-- Add with reqs to 2014/FA. This course has COREQ which requires classId 47
            // A. OVERLAPPING TERMS - USING DATES.
            var termId1 = "2014/FA";
            var courseId1 = "333";
            var plannedCourse1 = new PlannedCourse(courseId1);
            degreePlan.AddCourse(plannedCourse1, termId1);
            // Add planned course that satisfies coreq to 2014/GR - Term within 2014/FA
            var termId2 = "2014/GR";
            var courseId2 = "47";
            var plannedCourse2 = new PlannedCourse(courseId2);
            degreePlan.AddCourse(plannedCourse2, termId2);

            // Act-- Check degree plan for conflicts, specifically prerequisites
            degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, academicCredits, requirements, RuleResults);

            // Assert - no warning appears
            var plannedCourse = degreePlan.GetPlannedCourses(termId1).Where(c => c.CourseId == "333").First();
            Assert.AreEqual(0, plannedCourse.Warnings.Where(w => w.Type == PlannedCourseWarningType.UnmetRequisite).Count());
        }

        [TestMethod]
        public void Warning_UnSatisfiedConcurrent_TooLate()
        {
            // Arrange-- Add with reqs to 2013/SP. This course has COREQ which requires classId 47
            var termId1 = "2013/SP";
            var courseId1 = "333";
            var plannedCourse1 = new PlannedCourse(courseId1);
            degreePlan.AddCourse(plannedCourse1, termId1);
            // Add planned course that satisfies coreq to 2013/FA Too late
            var termId2 = "2013/FA";
            var courseId2 = "47";
            var plannedCourse2 = new PlannedCourse(courseId2);
            degreePlan.AddCourse(plannedCourse2, termId2);

            // Act-- Check degree plan for conflicts, specifically prerequisites
            degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, academicCredits, requirements, RuleResults);

            // Assert - no warning appears
            var plannedCourse = degreePlan.GetPlannedCourses(termId1).Where(c => c.CourseId == "333").First();
            var warnings = plannedCourse.Warnings.Where(w => w.Type == PlannedCourseWarningType.UnmetRequisite);
            Assert.AreEqual(1, warnings.Count());
            Assert.AreEqual("COREQ1", warnings.ElementAt(0).Requisite.RequirementCode);
            Assert.IsTrue(warnings.ElementAt(0).Requisite.IsRequired);
            Assert.AreEqual(RequisiteCompletionOrder.Concurrent, warnings.ElementAt(0).Requisite.CompletionOrder);
        }

        [TestMethod]
        public void Warning_UnSatisfiedConcurrent_TooSoon()
        {
            // Arrange-- Add with reqs to 2014/FA. This course has COREQ which requires classId 47
            var termId1 = "2014/FA";
            var courseId1 = "333";
            var plannedCourse1 = new PlannedCourse(courseId1);
            degreePlan.AddCourse(plannedCourse1, termId1);
            // Add planned course that satisfies coreq to 2015/SP - Too late
            var termId2 = "2014/SP";
            var courseId2 = "47";
            var plannedCourse2 = new PlannedCourse(courseId2);
            degreePlan.AddCourse(plannedCourse2, termId2);

            // Act-- Check degree plan for conflicts, specifically prerequisites
            degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, academicCredits, requirements, RuleResults);

            // Assert - no warning appears
            var plannedCourse = degreePlan.GetPlannedCourses(termId1).Where(c => c.CourseId == "333").First();
            var warnings = plannedCourse.Warnings.Where(w => w.Type == PlannedCourseWarningType.UnmetRequisite);
            Assert.AreEqual(1, warnings.Count());
            Assert.AreEqual("COREQ1", warnings.ElementAt(0).Requisite.RequirementCode);
            Assert.IsTrue(warnings.ElementAt(0).Requisite.IsRequired);
            Assert.AreEqual(RequisiteCompletionOrder.Concurrent, warnings.ElementAt(0).Requisite.CompletionOrder);
        }

        [TestMethod]
        public void NoWarning_SatisfiedConcurrent_AcadCredit1()
        {
            // Arrange-- Add course with req to 2014/FA. This course has COREQ1 which requires classId 47
            var termId1 = "2014/FA";
            var courseId1 = "333";
            var plannedCourse1 = new PlannedCourse(courseId1);
            degreePlan.AddCourse(plannedCourse1, termId1);

            // Arrange--Add 47 to academic credits, taken in current term
            var course2 = allCourses.Where(c => c.Id == "47").First();
            var credit = new AcademicCredit("47", course2, "");
            credit.StartDate = new DateTime(2014, 08, 22);
            credit.EndDate = new DateTime(2014, 08, 25);
            credit.Status = CreditStatus.Add;
            academicCredits = new List<AcademicCredit>() { credit };

            // Act-- Check degree plan for conflicts
            degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, academicCredits, requirements, RuleResults);

            // Assert 
            var plannedCourse = degreePlan.GetPlannedCourses(termId1).Where(c => c.CourseId == "333").First();

            Assert.AreEqual(0, plannedCourse.Warnings.Where(w => w.Type == PlannedCourseWarningType.UnmetRequisite).Count());
        }

        [TestMethod]
        public void NoWarning_SatisfiedConcurrent_AcadCredit2()
        {
            // Arrange-- Add course with req to 2014/FA. This course has COREQ1 which requires classId 47
            var termId1 = "2014/FA";
            var courseId1 = "333";
            var plannedCourse1 = new PlannedCourse(courseId1);
            degreePlan.AddCourse(plannedCourse1, termId1);

            // Arrange--Add 47 to academic credits, taken in current term
            var course2 = allCourses.Where(c => c.Id == "47").First();
            var credit = new AcademicCredit("47", course2, "");
            credit.StartDate = new DateTime(2014, 12, 12);
            credit.EndDate = new DateTime(2014, 12, 25);
            credit.Status = CreditStatus.Add;
            academicCredits = new List<AcademicCredit>() { credit };

            // Act-- Check degree plan for conflicts
            degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, academicCredits, requirements, RuleResults);

            // Assert 
            var plannedCourse = degreePlan.GetPlannedCourses(termId1).Where(c => c.CourseId == "333").First();

            Assert.AreEqual(0, plannedCourse.Warnings.Where(w => w.Type == PlannedCourseWarningType.UnmetRequisite).Count());
        }

        [TestMethod]
        public void NoWarning_SatisfiedConcurrent_AcadCredit3()
        {
            // Arrange-- Add course with req to 2014/FA. This course has COREQ1 which requires classId 47
            var termId1 = "2014/FA";
            var courseId1 = "333";
            var plannedCourse1 = new PlannedCourse(courseId1);
            degreePlan.AddCourse(plannedCourse1, termId1);

            // Arrange--Add 47 to academic credits, taken in current term
            var course2 = allCourses.Where(c => c.Id == "47").First();
            var credit = new AcademicCredit("47", course2, "");
            credit.StartDate = new DateTime(2014, 09, 12);
            credit.EndDate = new DateTime(2014, 10, 25);
            credit.Status = CreditStatus.Add;
            academicCredits = new List<AcademicCredit>() { credit };

            // Act-- Check degree plan for conflicts
            degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, academicCredits, requirements, RuleResults);

            // Assert 
            var plannedCourse = degreePlan.GetPlannedCourses(termId1).Where(c => c.CourseId == "333").First();

            Assert.AreEqual(0, plannedCourse.Warnings.Where(w => w.Type == PlannedCourseWarningType.UnmetRequisite).Count());
        }

        [TestMethod]
        public void NoWarning_SatisfiedConcurrent_AcadCredit_MissingDates()
        {
            // Arrange-- Add course with req to 2014/FA. This course has COREQ1 which requires classId 47
            var termId1 = "2014/FA";
            var courseId1 = "333";
            var plannedCourse1 = new PlannedCourse(courseId1);
            degreePlan.AddCourse(plannedCourse1, termId1);

            // Arrange--Add 47 to academic credits, taken in an overlapping term
            var course2 = allCourses.Where(c => c.Id == "47").First();
            //var section2Id = allSections.Where(s => s.CourseId == "47" && s.TermId == "2014/FA").First().Id;
            var credit = new AcademicCredit("47", course2, "");
            credit.StartDate = new DateTime(2014, 09, 12);
            credit.TermCode = "2014/GR";
            credit.Status = CreditStatus.Add;
            academicCredits = new List<AcademicCredit>() { credit };

            // Act-- Check degree plan for conflicts
            degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, academicCredits, requirements, RuleResults);

            // Assert 
            var plannedCourse = degreePlan.GetPlannedCourses(termId1).Where(c => c.CourseId == "333").First();

            Assert.AreEqual(0, plannedCourse.Warnings.Where(w => w.Type == PlannedCourseWarningType.UnmetRequisite).Count());
        }

        [TestMethod]
        public void NoWarningAdded_NonTermCourseConcurrentReq()
        {
            // Arrange-- Add nonterm course with a required concurrent req (course ID 333). 
            // This nonterm Section This nonterm Section has dates of 2/22/13 - 3/24/13
            var courseId1 = "333";
            var course1 = allCourses.First(c => c.Id == courseId1);
            var section1 = allSections.First(s => s.CourseId == courseId1 && s.Number == "NT");
            var plannedCourse1 = new PlannedCourse(courseId1, section1.Id);
            degreePlan.AddCourse(plannedCourse1, "");

            var courseId2 = "47";
            var course2 = allCourses.First(c => c.Id == courseId2);
            var acadCredit1 = new AcademicCredit("01", course2, "");
            acadCredit1.TermCode = "2013/SP";
            acadCredit1.Status = CreditStatus.New;
            academicCredits = new List<AcademicCredit>() { acadCredit1 };

            // Act-- Check degree plan for conflicts
            degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, academicCredits, requirements, RuleResults);

            // Assert-- Get the planned course and verify the message is not there
            var plannedCourse = degreePlan.GetPlannedCourses("").Where(c => c.CourseId == "333").First();
            Assert.AreEqual(0, plannedCourse.Warnings.Where(w => w.Type == PlannedCourseWarningType.UnmetRequisite).Count());
        }

        [TestMethod]
        public void NoWarningAdded_NonTermCourseConcurrentReq_AcadCred()
        {
            // Arrange-- Add nonterm course with a required concurrent req (course ID 333). 
            // This nonterm Section has dates of 2/22/13 - 3/24/13
            var courseId1 = "333";
            var course1 = allCourses.First(c => c.Id == courseId1);
            var section1 = allSections.First(s => s.CourseId == courseId1 && s.Number == "NT");
            var plannedCourse1 = new PlannedCourse(courseId1, section1.Id);
            degreePlan.AddCourse(plannedCourse1, "");

            var courseId2 = "47";
            var course2 = allCourses.First(c => c.Id == courseId2);
            var acadCredit1 = new AcademicCredit("01", course2, "");
            acadCredit1.StartDate = new DateTime(2013, 02, 01);
            acadCredit1.EndDate = new DateTime(2013, 02, 22);
            acadCredit1.Status = CreditStatus.New;
            academicCredits = new List<AcademicCredit>() { acadCredit1 };

            // Act-- Check degree plan for conflicts
            degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, academicCredits, requirements, RuleResults);

            // Assert-- Get the planned course and verify the message is not there
            var plannedCourse = degreePlan.GetPlannedCourses("").Where(c => c.CourseId == "333").First();
            Assert.AreEqual(0, plannedCourse.Warnings.Where(w => w.Type == PlannedCourseWarningType.UnmetRequisite).Count());
        }

        [TestMethod]
        public void WarningAdded_NonTermCourseConcurrentReq_TooLateAsAcadCredit()
        {
            // Arrange-- Add nonterm course with a required concurrent req (course ID 333). 
            // This nonterm Section has dates of 2/22/13 - 3/24/13
            var courseId1 = "333";
            var course1 = allCourses.First(c => c.Id == courseId1);
            var section1 = allSections.First(s => s.CourseId == courseId1 && s.Number == "NT");
            var plannedCourse1 = new PlannedCourse(courseId1, section1.Id);
            degreePlan.AddCourse(plannedCourse1, "");

            // set up acad credit to satisfy but make dates too early.
            var courseId2 = "47";
            var course2 = allCourses.First(c => c.Id == courseId2);
            var acadCredit1 = new AcademicCredit("01", course2, "");
            acadCredit1.StartDate = new DateTime(2013, 04, 01);
            acadCredit1.EndDate = new DateTime(2013, 04, 30);
            acadCredit1.Status = CreditStatus.New;
            academicCredits = new List<AcademicCredit>() { acadCredit1 };

            // Act-- Check degree plan for conflicts
            degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, academicCredits, requirements, RuleResults);

            // Assert-- Get the planned course and verify the message is there
            var plannedCourse = degreePlan.GetPlannedCourses("").Where(c => c.CourseId == "333").First();
            var warnings = plannedCourse.Warnings.Where(w => w.Type == PlannedCourseWarningType.UnmetRequisite);
            Assert.AreEqual(1, warnings.Count());
            Assert.AreEqual("COREQ1", warnings.ElementAt(0).Requisite.RequirementCode);
            Assert.IsTrue(warnings.ElementAt(0).Requisite.IsRequired);
            Assert.AreEqual(RequisiteCompletionOrder.Concurrent, warnings.ElementAt(0).Requisite.CompletionOrder);
        }

        [TestMethod]
        public void WarningAdded_NonTermCourseConcurrentReq_TooSoonAsAcadCredit()
        {
            // Arrange-- Add nonterm course with a required concurrent req (course ID 333). 
            // This nonterm Section has dates of 2/22/13 - 3/24/13
            var courseId1 = "333";
            var course1 = allCourses.First(c => c.Id == courseId1);
            var section1 = allSections.First(s => s.CourseId == courseId1 && s.Number == "NT");
            var plannedCourse1 = new PlannedCourse(courseId1, section1.Id);
            degreePlan.AddCourse(plannedCourse1, "");

            // set up acad credit to satisfy but make dates too early.
            var courseId2 = "47";
            var course2 = allCourses.First(c => c.Id == courseId2);
            var acadCredit1 = new AcademicCredit("01", course2, "");
            acadCredit1.StartDate = new DateTime(2013, 02, 01);
            acadCredit1.EndDate = new DateTime(2013, 02, 21);
            acadCredit1.Status = CreditStatus.New;
            academicCredits = new List<AcademicCredit>() { acadCredit1 };

            // Act-- Check degree plan for conflicts
            degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, academicCredits, requirements, RuleResults);

            // Assert-- Get the planned course and verify the message is there
            var plannedCourse = degreePlan.GetPlannedCourses("").Where(c => c.CourseId == "333").First();
            var warnings = plannedCourse.Warnings.Where(w => w.Type == PlannedCourseWarningType.UnmetRequisite);
            Assert.AreEqual(1, warnings.Count());
            Assert.AreEqual("COREQ1", warnings.ElementAt(0).Requisite.RequirementCode);
            Assert.IsTrue(warnings.ElementAt(0).Requisite.IsRequired);
            Assert.AreEqual(RequisiteCompletionOrder.Concurrent, warnings.ElementAt(0).Requisite.CompletionOrder);
        }

        [TestMethod]
        public void NoWarning_Multiple_ConcurrentReq_InAcadCredit4()
        {
            // Arrange-- Course 21 has COREQ2 (course 110 concurrent recommended) and REQ1 (course 47 either required)
            var courseId1 = "21";
            var plannedCourse1 = new PlannedCourse(courseId1);
            var termId1 = "2013/SP";
            degreePlan.AddCourse(plannedCourse1, termId1);

            // Add planned course that satisfies either 
            var courseId2 = "47";
            var plannedCourse2 = new PlannedCourse(courseId2);
            degreePlan.AddCourse(plannedCourse2, termId1);

            // Act-- Check degree plan for conflicts
            degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, academicCredits, requirements, RuleResults);

            // Assert-- Get the planned course and verify the message is there
            var plannedCourse = degreePlan.GetPlannedCourses("2013/SP").Where(c => c.CourseId == "21").First();
            var warnings = plannedCourse.Warnings.Where(w => w.Type == PlannedCourseWarningType.UnmetRequisite);
            Assert.AreEqual(1, warnings.Count());
            Assert.AreEqual("COREQ2", warnings.ElementAt(0).Requisite.RequirementCode);
            Assert.IsFalse(warnings.ElementAt(0).Requisite.IsRequired);
            Assert.AreEqual(RequisiteCompletionOrder.Concurrent, warnings.ElementAt(0).Requisite.CompletionOrder);
        }

        [TestMethod]
        public void NoWarning_ConcurrentReq_AcadCredit_MiniTerm()
        {
            // Arrange-- Course 21 has COREQ2 (course 110 concurrent recommended) and REQ1 (course 47 either required)
            var courseId1 = "21";
            var plannedCourse1 = new PlannedCourse(courseId1);
            var termId1 = "2014/FA";
            degreePlan.AddCourse(plannedCourse1, termId1);

            // set up acad credit to satisfy req and put it in an overlapping miniterm
            var courseId2 = "47";
            var course2 = allCourses.First(c => c.Id == courseId2);
            var acadCredit1 = new AcademicCredit("01", course2, "");
            acadCredit1.TermCode = "2014/GR";
            acadCredit1.Status = CreditStatus.New;
            academicCredits = new List<AcademicCredit>() { acadCredit1 };

            //// Add planned course that satisfies req 
            //var courseId2 = "47";
            //var plannedCourse2 = new PlannedCourse(courseId2);
            //degreePlan.AddCourse(plannedCourse2, termId1);

            // Act-- Check degree plan for conflicts
            degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, academicCredits, requirements, RuleResults);

            // Assert-- Get the planned course and verify the message is there
            var plannedCourse = degreePlan.GetPlannedCourses("2014/FA").Where(c => c.CourseId == "21").First();
            var warnings = plannedCourse.Warnings.Where(w => w.Type == PlannedCourseWarningType.UnmetRequisite);
            Assert.AreEqual(1, warnings.Count());
            Assert.AreEqual("COREQ2", warnings.ElementAt(0).Requisite.RequirementCode);
            Assert.IsFalse(warnings.ElementAt(0).Requisite.IsRequired);
            Assert.AreEqual(RequisiteCompletionOrder.Concurrent, warnings.ElementAt(0).Requisite.CompletionOrder);
        }
    }

    [TestClass]
    public class DegreePlanCheckReq_Prereqs
    {
        string personId;
        DegreePlan degreePlan;
        IEnumerable<Term> regTerms = new List<Term>();
        IEnumerable<Term> allTerms = new List<Term>();
        IEnumerable<Course> allCourses = new List<Course>();
        IEnumerable<Section> allSections = new List<Section>();
        IEnumerable<AcademicCredit> academicCredits = new List<AcademicCredit>();
        IEnumerable<Requirement> requirements = new List<Requirement>();
        IEnumerable<RuleResult> RuleResults;
        ILogger logger;

        [TestInitialize]
        public async void Initialize()
        {
            allCourses = await new TestCourseRepository().GetAsync();
            allTerms = new TestTermRepository().Get();
            regTerms = await new TestTermRepository().GetRegistrationTermsAsync();
            allSections = await new TestSectionRepository().GetRegistrationSectionsAsync(regTerms);
            personId = "00004002";
            degreePlan = new DegreePlan(personId);
            var academicCredits = new List<AcademicCredit>();
            requirements = await new TestRequirementRepository().GetAsync(new List<string>() { "PREREQ1", "PREREQ2", "COREQ1", "COREQ2", "REQ1" });
            RuleResults = new List<RuleResult>();
            logger = new Mock<ILogger>().Object;
        }

        [TestCleanup]
        public void CleanUp()
        {
            degreePlan = null;
        }

        [TestMethod]
        public void WarningAddedforUnsatisfiedPrerequisite()
        {
            // Arrange-- Add two courses with prereqs to 2012/FA
            var termId1 = "2012/FA";
            var courseId1 = "186";
            var plannedCourse1 = new PlannedCourse(courseId1);
            degreePlan.AddCourse(plannedCourse1, termId1);
            var courseId2 = "87";
            var plannedCourse2 = new PlannedCourse(courseId2);
            degreePlan.AddCourse(plannedCourse2, termId1);
            var courseId3 = "139";
            var plannedCourse3 = new PlannedCourse(courseId3);
            degreePlan.AddCourse(plannedCourse3, termId1);

            // Act-- Check degree plan for conflicts, specifically prerequisites
            degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, academicCredits, requirements, RuleResults);

            // Assert
            var plannedCourse = degreePlan.GetPlannedCourses(termId1).Where(c => c.CourseId == "186").First();
            var warnings = plannedCourse.Warnings.Where(w => w.Type == PlannedCourseWarningType.UnmetRequisite);
            Assert.AreEqual(1, warnings.Count());
            Assert.AreEqual("PREREQ1", warnings.ElementAt(0).Requisite.RequirementCode);
            Assert.IsTrue(warnings.ElementAt(0).Requisite.IsRequired);
            Assert.AreEqual(RequisiteCompletionOrder.Previous, warnings.ElementAt(0).Requisite.CompletionOrder);

            var plannedcourse = degreePlan.GetPlannedCourses(termId1).Where(c => c.CourseId == "87").First();
            var pwarnings = plannedcourse.Warnings.Where(w => w.Type == PlannedCourseWarningType.UnmetRequisite);
            Assert.AreEqual(1, pwarnings.Count());
            Assert.AreEqual("PREREQ2", pwarnings.ElementAt(0).Requisite.RequirementCode);
            Assert.IsFalse(pwarnings.ElementAt(0).Requisite.IsRequired);
            Assert.AreEqual(RequisiteCompletionOrder.Previous, pwarnings.ElementAt(0).Requisite.CompletionOrder);
        }

        // Same setup as previous test but pass in empty list of requirements
        [TestMethod]
        public void MissingRequirementsIgnored()
        {
            // Arrange-- Add two courses with prereqs to 2012/FA
            var termId1 = "2012/FA";
            var courseId1 = "186";
            var plannedCourse1 = new PlannedCourse(courseId1);
            degreePlan.AddCourse(plannedCourse1, termId1);
            var courseId2 = "87";
            var plannedCourse2 = new PlannedCourse(courseId2);
            degreePlan.AddCourse(plannedCourse2, termId1);
            var courseId3 = "139";
            var plannedCourse3 = new PlannedCourse(courseId3);
            degreePlan.AddCourse(plannedCourse3, termId1);

            // Act-- Check degree plan for conflicts, specifically prerequisites
            degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, academicCredits, new List<Requirement>(), RuleResults);

            // Assert
            var plannedCourse = degreePlan.GetPlannedCourses(termId1).Where(c => c.CourseId == "186").First();
            Assert.AreEqual(0, plannedCourse.Warnings.Where(w => w.Type == PlannedCourseWarningType.UnmetRequisite).Count());

            var plannedcourse = degreePlan.GetPlannedCourses(termId1).Where(c => c.CourseId == "87").First();
            Assert.AreEqual(0, plannedCourse.Warnings.Where(w => w.Type == PlannedCourseWarningType.UnmetRequisite).Count());
        }

        [TestMethod]
        public void WarningAddedforUnsatisfiedNonTermCoursePrerequisite()
        {
            // Arrange-- Add nonterm course with prereq
            var courseId1 = "7704";
            var section1 = allSections.First(s => s.CourseId == courseId1 && s.Number == "04");
            var plannedCourse1 = new PlannedCourse(courseId1, section1.Id);
            degreePlan.AddCourse(plannedCourse1, "");

            // Act-- Check degree plan for conflicts, specifically prerequisites
            degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, academicCredits, requirements, RuleResults);

            // Assert-- Get the planned course and verify the message is there
            var plannedCourse = degreePlan.GetPlannedCourses("").Where(c => c.CourseId == "7704").First();
            var requisiteWarnings = plannedCourse.Warnings.Where(w => w.Type == PlannedCourseWarningType.UnmetRequisite && w.Requisite != null);
            var warnings = requisiteWarnings.Where(w => w.Requisite.RequirementCode == "PREREQ1");
            Assert.AreEqual(1, warnings.Count());
            Assert.AreEqual("PREREQ1", warnings.ElementAt(0).Requisite.RequirementCode);
            Assert.IsTrue(warnings.ElementAt(0).Requisite.IsRequired);
            Assert.AreEqual(RequisiteCompletionOrder.Previous, warnings.ElementAt(0).Requisite.CompletionOrder);

        }


        [TestMethod]
        public void NoWarningAddedforSatisfiedPrerequisite()
        {
            // Arrange-- Add with prereqs to 2014/FA. This course has PREREQ which requires classId 143
            var termId1 = "2014/FA";
            var courseId1 = "186";
            var plannedCourse1 = new PlannedCourse(courseId1);
            degreePlan.AddCourse(plannedCourse1, termId1);
            // Add planned course that satisfies prereq to 2013/SP
            var termId2 = "2013/SP";
            var courseId2 = "143";
            var plannedCourse2 = new PlannedCourse(courseId2);
            degreePlan.AddCourse(plannedCourse2, termId2);
            // Add the same course again...degree plan calls program evaluation to evaluation prereq and it should
            // be able to handle duplicate planned courses.
            var plannedCourse3 = new PlannedCourse(courseId2);
            degreePlan.AddCourse(plannedCourse3, termId2);

            // Act-- Check degree plan for conflicts, specifically prerequisites
            degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, academicCredits, requirements, RuleResults);

            // Assert - no warning appears
            var plannedCourse = degreePlan.GetPlannedCourses(termId1).Where(c => c.CourseId == "186").First();
            Assert.AreEqual(0, plannedCourse.Warnings.Where(w => w.Type == PlannedCourseWarningType.UnmetRequisite).Count());
        }

        [TestMethod]
        public void NoWarningAddedforSatisfiedNonTermCoursePrerequisite()
        {
            // Arrange-- Add nonterm course with prereq of MATH-100 (course ID 143). 
            // This nonterm Section starts 30 days after 2013/FA term start date, math101 must be complete before section start date
            var courseId1 = "7701";
            var course1 = allCourses.First(c => c.Id == courseId1);
            var section1 = allSections.First(s => s.CourseId == courseId1 && s.Number == "05");
            var plannedCourse1 = new PlannedCourse(courseId1, section1.Id);
            degreePlan.AddCourse(plannedCourse1, "");
            var acadCredit1 = new AcademicCredit("01", course1, section1.Id);
            acadCredit1.TermCode = "2012/FA";
            academicCredits = new List<AcademicCredit>() { acadCredit1 };

            // Act-- Check degree plan for conflicts
            degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, academicCredits, requirements, RuleResults);

            // Assert-- Get the planned course and verify the message is not there
            var plannedCourse = degreePlan.GetPlannedCourses("").Where(c => c.CourseId == "7701").First();
            Assert.AreEqual(0, plannedCourse.Warnings.Where(w => w.Type == PlannedCourseWarningType.UnmetRequisite).Count());
        }

        [TestMethod]
        public void WarningAddedforNonTermCoursePrerequisiteTakenTooLateAsAcadCredit()
        {
            // Arrange-- Add nonterm course with prereq of MATH-100 (course ID 143). 
            // This nonterm Section starts 30 days after 2013/FA term start date, math101 must be complete before section start date
            var courseId1 = "7701";
            var section1 = allSections.First(s => s.CourseId == courseId1 && s.Number == "05");
            var plannedCourse1 = new PlannedCourse(courseId1, section1.Id);
            degreePlan.AddCourse(plannedCourse1, "");
            // Create academic credit for math 101, but taken in 2013/FA, too late to be considered to satisfy this prereq
            var courseId2 = "143";
            var course2 = allCourses.First(c => c.Id == courseId2);
            var section2 = allSections.First(s => s.CourseId == courseId2 && s.TermId == "2013/SP");
            var acadCredit1 = new AcademicCredit("01", course2, section2.Id);
            acadCredit1.TermCode = "2013/FA";
            academicCredits = new List<AcademicCredit>() { acadCredit1 };

            // Act-- Check degree plan for conflicts
            degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, academicCredits, requirements, RuleResults);

            // Assert-- Get the planned course and verify the message is not there
            var plannedCourse = degreePlan.GetPlannedCourses("").Where(c => c.CourseId == "7701").First();
            var warnings = plannedCourse.Warnings.Where(w => w.Type == PlannedCourseWarningType.UnmetRequisite);
            Assert.AreEqual(1, warnings.Count());
            Assert.AreEqual("PREREQ1", warnings.ElementAt(0).Requisite.RequirementCode);
            Assert.IsTrue(warnings.ElementAt(0).Requisite.IsRequired);
            Assert.AreEqual(RequisiteCompletionOrder.Previous, warnings.ElementAt(0).Requisite.CompletionOrder);
        }

        [TestMethod]
        public void WarningAddedforNonTermCoursePrerequisiteTakenTooLateAsPlannedCourse()
        {
            // Arrange-- Add nonterm course with prereq1 of MATH-100 (course ID 143). 
            // This nonterm Section starts 30 days after 2013/FA term start date, math101 must be complete before section start date
            var courseId1 = "7701";
            var section1 = allSections.First(s => s.CourseId == courseId1 && s.Number == "05");
            var plannedCourse1 = new PlannedCourse(courseId1, section1.Id);
            degreePlan.AddCourse(plannedCourse1, "");
            // Create academic credit for math 101, but taken in 2013/FA, too late to be considered to satisfy this prereq
            var courseId2 = "143";
            var course2 = allCourses.First(c => c.Id == courseId2);
            var section2 = allSections.First(s => s.CourseId == courseId2 && s.TermId == "2013/SP");
            var plannedCourse2 = new PlannedCourse(courseId2, section2.Id);
            degreePlan.AddCourse(plannedCourse2, "2013/SP");

            // Act-- Check degree plan for conflicts
            degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, academicCredits, requirements, RuleResults);

            // Assert-- Get the planned course and verify the message is not there
            var plannedCourse = degreePlan.GetPlannedCourses("").Where(c => c.CourseId == "7701").First();
            var warnings = plannedCourse.Warnings.Where(w => w.Type == PlannedCourseWarningType.UnmetRequisite);
            Assert.AreEqual(1, warnings.Count());
            Assert.AreEqual("PREREQ1", warnings.ElementAt(0).Requisite.RequirementCode);
            Assert.IsTrue(warnings.ElementAt(0).Requisite.IsRequired);
            Assert.AreEqual(RequisiteCompletionOrder.Previous, warnings.ElementAt(0).Requisite.CompletionOrder);
        }

        [TestMethod]
        public void WarningAddedforPrerequisiteSatisfiedTooLate()
        {
            // Arrange-- Add with prereqs to 2014/FA
            var termId1 = "2014/FA";
            var courseId1 = "186";
            var plannedCourse1 = new PlannedCourse(courseId1);
            degreePlan.AddCourse(plannedCourse1, termId1);
            // Add planned course that satisfies prereq but taken too late
            var termId2 = "2014/FA";
            var courseId2 = "143";
            var plannedCourse2 = new PlannedCourse(courseId2);
            degreePlan.AddCourse(plannedCourse2, termId2);

            // Act-- Check degree plan for conflicts, specifically prerequisites
            degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, academicCredits, requirements, RuleResults);

            // Assert - no warning appears
            var plannedCourse = degreePlan.GetPlannedCourses(termId1).Where(c => c.CourseId == "186").First();
            var warnings = plannedCourse.Warnings.Where(w => w.Type == PlannedCourseWarningType.UnmetRequisite);
            Assert.AreEqual(1, warnings.Count());
            Assert.AreEqual("PREREQ1", warnings.ElementAt(0).Requisite.RequirementCode);
            Assert.IsTrue(warnings.ElementAt(0).Requisite.IsRequired);
            Assert.AreEqual(RequisiteCompletionOrder.Previous, warnings.ElementAt(0).Requisite.CompletionOrder);
        }

        [TestMethod]
        public void WarningForCourseRequiringUnsatisfiedNontermPrerequisite()
        {
            // Arrange-- Course 87 (HIST-400) has a prerequisite of PREREQ2 which recommends course 7701 (DENT-101)
            var courseId1 = "87";
            var plannedCourse1 = new PlannedCourse(courseId1);
            var termId1 = "2013/SP";
            degreePlan.AddCourse(plannedCourse1, termId1);

            // Act-- Check degree plan for conflicts
            degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, academicCredits, requirements, RuleResults);

            // Assert - One warning appears, citing the correct prerequisite code
            var plannedCourse = degreePlan.GetPlannedCourses(termId1).Where(c => c.CourseId == "87").First();
            var warnings = plannedCourse.Warnings.Where(w => w.Type == PlannedCourseWarningType.UnmetRequisite);
            Assert.AreEqual(1, warnings.Count());
            Assert.AreEqual("PREREQ2", warnings.ElementAt(0).Requisite.RequirementCode);
            Assert.IsFalse(warnings.ElementAt(0).Requisite.IsRequired);
            Assert.AreEqual(RequisiteCompletionOrder.Previous, warnings.ElementAt(0).Requisite.CompletionOrder);
        }

        [TestMethod]
        public void WarningForNontermPrerequisitePlannedTooLate()
        {
            // Arrange-- Course 87 (HIST-400) has a prerequisite of PREREQ2 which recommends course 7701 (DENT-101)
            var courseId1 = "87";
            var plannedCourse1 = new PlannedCourse(courseId1);
            var termId1 = "2013/SP";
            degreePlan.AddCourse(plannedCourse1, termId1);

            // Arrange-- Add course 7701 as a nonterm course that does not finish before HIST-400 is started (this section starts in 2013/sp)
            var courseId2 = "7701";
            var section2 = allSections.Where(s => s.CourseId == "7701" && s.Number == "05").First();
            var plannedCourse2 = new PlannedCourse(courseId2, section2.Id);
            degreePlan.AddCourse(plannedCourse2, "");

            // Act-- Check degree plan for conflicts
            degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, academicCredits, requirements, RuleResults);

            // Assert - One warning appears, citing the correct prerequisite code
            var plannedCourse = degreePlan.GetPlannedCourses(termId1).Where(c => c.CourseId == "87").First();
            var warnings = plannedCourse.Warnings.Where(w => w.Type == PlannedCourseWarningType.UnmetRequisite);
            Assert.AreEqual(1, warnings.Count());
            Assert.AreEqual("PREREQ2", warnings.ElementAt(0).Requisite.RequirementCode);
            Assert.IsFalse(warnings.ElementAt(0).Requisite.IsRequired);
            Assert.AreEqual(RequisiteCompletionOrder.Previous, warnings.ElementAt(0).Requisite.CompletionOrder);
        }

        [TestMethod]
        public void NoWarningForTermPrerequisiteInAcadCredit()
        {
            // Arrange-- Course 87 (HIST-400) has a prerequisite of PREREQ2 which recommends course 7701 (DENT-101)
            var courseId1 = "87";
            var plannedCourse1 = new PlannedCourse(courseId1);
            var termId1 = "2013/SP";
            degreePlan.AddCourse(plannedCourse1, termId1);
            // Arrange--Add 7701 to academic credits, taken in prior term
            var course2 = allCourses.Where(c => c.Id == "7701").First();
            var section2Id = allSections.Where(s => s.CourseId == "7701" && s.TermId == "2012/FA").First().Id;
            var credit = new AcademicCredit("7701", course2, section2Id);
            credit.Status = CreditStatus.Add;
            academicCredits = new List<AcademicCredit>() { credit };

            // Act-- Check degree plan for conflicts
            degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, academicCredits, requirements, RuleResults);

            // Assert - One warning appears, citing the correct prerequisite code
            var plannedCourse = degreePlan.GetPlannedCourses(termId1).Where(c => c.CourseId == "87").First();

            Assert.AreEqual(0, plannedCourse.Warnings.Where(w => w.Type == PlannedCourseWarningType.UnmetRequisite).Count());
        }

        [TestMethod]
        public void NoWarningForTermPrerequisiteInAcadCreditWithNoSection()
        {
            // Arrange-- Course 87 (HIST-400) has a prerequisite of PREREQ2 which recommends course 7701 (DENT-101)
            var courseId1 = "87";
            var plannedCourse1 = new PlannedCourse(courseId1);
            var termId1 = "2013/SP";
            degreePlan.AddCourse(plannedCourse1, termId1);
            // Arrange--Add 7701 to academic credits, taken in prior term
            var course2 = allCourses.Where(c => c.Id == "7701").First();
            var section2Id = allSections.Where(s => s.CourseId == "7701" && s.TermId == "2012/FA").First().Id;
            var credit = new AcademicCredit("7700", course2, "");
            credit.Status = CreditStatus.Add;
            credit.EndDate = new DateTime(2012, 12, 10);
            academicCredits = new List<AcademicCredit>() { credit };

            // Act-- Check degree plan for conflicts
            degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, academicCredits, requirements, RuleResults);

            // Assert - One warning appears, citing the correct prerequisite code
            var plannedCourse = degreePlan.GetPlannedCourses(termId1).Where(c => c.CourseId == "87").First();
            Assert.AreEqual(0, plannedCourse.Warnings.Where(w => w.Type == PlannedCourseWarningType.UnmetRequisite).Count());
        }

        [TestMethod]
        public void NoWarningForTermPrerequisiteInAcadCreditWithNoEndDateOrSection()
        {
            // Arrange-- Course 87 (HIST-400) has a prerequisite of PREREQ2 which recommends course 7701 (DENT-101)
            var courseId1 = "87";
            var plannedCourse1 = new PlannedCourse(courseId1);
            var termId1 = "2013/SP";
            degreePlan.AddCourse(plannedCourse1, termId1);
            // Arrange--Add 7701 to academic credits, taken in prior term
            var course2 = allCourses.Where(c => c.Id == "7701").First();
            var credit = new AcademicCredit("7700", course2, "");
            credit.Status = CreditStatus.Add;
            credit.TermCode = "2012/FA";
            academicCredits = new List<AcademicCredit>() { credit };

            // Act-- Check degree plan for conflicts
            degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, academicCredits, requirements, RuleResults);

            // Assert - One warning appears, citing the correct prerequisite code
            var plannedCourse = degreePlan.GetPlannedCourses(termId1).Where(c => c.CourseId == "87").First();
            Assert.AreEqual(0, plannedCourse.Warnings.Where(w => w.Type == PlannedCourseWarningType.UnmetRequisite).Count());
        }

        [TestMethod]
        public void WarningForTermPrerequisitePlannedButNeverTaken()
        {
            // Arrange-- Course 87 (HIST-400) has a prerequisite of PREREQ2 which recommends course 7701 (DENT-101) taken first
            var courseId1 = "87";
            var plannedCourse1 = new PlannedCourse(courseId1);
            var termId1 = "2013/SP";
            degreePlan.AddCourse(plannedCourse1, termId1);
            // Arrange--Add 7701 to planned courses in prior term, never taken
            var courseId2 = "7701";
            var plannedCourse2 = new PlannedCourse(courseId2);
            var termId2 = "2011/FA";
            degreePlan.AddCourse(plannedCourse2, termId2);

            // Act-- Check degree plan for conflicts
            degreePlan.CheckForConflicts(allTerms, regTerms, allCourses, allSections, academicCredits, requirements, RuleResults);

            // Assert - One warning appears, citing the correct prerequisite code
            var plannedCourse = degreePlan.GetPlannedCourses(termId1).Where(c => c.CourseId == "87").First();
            var warnings = plannedCourse.Warnings.Where(w => w.Type == PlannedCourseWarningType.UnmetRequisite);
            Assert.AreEqual(1, warnings.Count());
            Assert.AreEqual("PREREQ2", warnings.ElementAt(0).Requisite.RequirementCode);
            Assert.IsFalse(warnings.ElementAt(0).Requisite.IsRequired);
            Assert.AreEqual(RequisiteCompletionOrder.Previous, warnings.ElementAt(0).Requisite.CompletionOrder);
        }
    }

    [TestClass]
    public class DegreePlanAddApproval
    {
        private string personId;
        private DegreePlanApprovalStatus status;
        private DateTime timeStamp;
        private string termCode;
        private string courseId;
        private DegreePlan degreePlan;
        private PlannedCourse plannedCourse;

        [TestInitialize]
        public void Initialize()
        {
            personId = "1111111";
            degreePlan = new DegreePlan(personId);
            status = DegreePlanApprovalStatus.Approved;
            timeStamp = DateTime.Now;
            termCode = "2012/FA";
            courseId = "123";
            plannedCourse = new PlannedCourse(courseId, null, GradingType.Graded);
            degreePlan.AddApproval(personId, status, timeStamp, courseId, termCode);
        }

        [TestMethod]
        public void PersonId()
        {
            Assert.AreEqual(personId, degreePlan.Approvals.ElementAt(0).PersonId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PersonIdThrowsExceptionIfNull()
        {
            personId = null;
            degreePlan.AddApproval(personId, status, timeStamp, courseId, termCode);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PersonIdThrowsExceptionIfEmpty()
        {
            personId = "";
            degreePlan.AddApproval(personId, status, timeStamp, courseId, termCode);
        }

        [TestMethod]
        public void Status()
        {
            Assert.AreEqual(status, degreePlan.Approvals.ElementAt(0).Status);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CourseIdThrowsExceptionIfNull()
        {
            degreePlan.AddApproval(personId, status, timeStamp, null, termCode);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CourseIdThrowsExceptionIfEmpty()
        {
            degreePlan.AddApproval(personId, status, timeStamp, "", termCode);
        }

        [TestMethod]
        public void TimeStamp()
        {
            // Can't exactly check the time stamp, but verify that it was set
            // to the current date/time when the approval item was constructed.
            Assert.IsTrue(timeStamp <= degreePlan.Approvals.ElementAt(0).Date);
            Assert.IsTrue(DateTime.Now >= degreePlan.Approvals.ElementAt(0).Date);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TermCodeThrowsExceptionIfNull()
        {
            termCode = null;
            degreePlan.AddApproval(personId, status, timeStamp, courseId, termCode);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TermCodeThrowsExceptionIfEmpty()
        {
            termCode = "";
            degreePlan.AddApproval(personId, status, timeStamp, courseId, termCode);
        }

        [TestMethod]
        public void DegreePlanAddApprovalRetainsPreviousApprovals()
        {
            status = DegreePlanApprovalStatus.Denied;
            termCode = "2012/FA";
            Thread.Sleep(10);
            degreePlan.AddApproval(personId, status, timeStamp, "123", termCode);
            Assert.AreEqual(2, degreePlan.Approvals.Count());
        }

    }

    [TestClass]
    public class ReviewOnlyChange
    {
        private string personId;
        private int degreePlanId;
        private DegreePlan degreePlanSource;
        private DegreePlan degreePlanTarget;

        [TestInitialize]
        public void Initialize()
        {
            personId = "0000693";
            degreePlanId = 1;
            degreePlanSource = new DegreePlan(degreePlanId, personId, 1);
            degreePlanSource.AddTerm("2012/FA");
            degreePlanSource.AddTerm("2013/SP");
            degreePlanSource.AddCourse(new PlannedCourse(course: "111", coursePlaceholder: null), "2012/FA");

            degreePlanId = 1;
            degreePlanTarget = new DegreePlan(degreePlanId, personId, 1);
            degreePlanTarget.AddTerm("2013/SP");
            degreePlanTarget.AddTerm("2012/FA");
            degreePlanTarget.AddCourse(new PlannedCourse(course: "111", coursePlaceholder: null), "2012/FA");
            degreePlanTarget.AddApproval(personId, DegreePlanApprovalStatus.Approved, DateTime.Now, "111", "2012/FA");

            // Asserts are based off this constructor statement, unless another constructor is used in the test method
        }

        [TestCleanup]
        public void CleanUp()
        {
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReviewOnlyChange_InvalidArgument()
        {
            degreePlanSource.ReviewOnlyChange(null);
        }

        [TestMethod]
        public void ReviewOnlyChange_DifferentIds()
        {
            DegreePlan degreePlanTarget2 = new DegreePlan(2, personId, 1);
            degreePlanTarget2.AddTerm("2013/SP");
            degreePlanTarget2.AddTerm("2012/FA");
            degreePlanTarget2.AddCourse(new PlannedCourse(course: "111", coursePlaceholder: null), "2012/FA");
            bool result = degreePlanSource.ReviewOnlyChange(degreePlanTarget2);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ReviewOnlyChange_DifferentPersonIds()
        {
            DegreePlan degreePlanTarget3 = new DegreePlan(1, "1111111", 1);
            degreePlanTarget3.AddTerm("2013/SP");
            degreePlanTarget3.AddTerm("2012/FA");
            degreePlanTarget3.AddCourse(new PlannedCourse(course: "111", coursePlaceholder: null), "2012/FA");
            bool result = degreePlanSource.ReviewOnlyChange(degreePlanTarget3);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ReviewOnlyChange_DifferentVersions()
        {
            DegreePlan degreePlanTarget4 = new DegreePlan(1, personId, 2);
            degreePlanTarget4.AddTerm("2013/SP");
            degreePlanTarget4.AddTerm("2012/FA");
            degreePlanTarget4.AddCourse(new PlannedCourse(course: "111", coursePlaceholder: null), "2012/FA");
            bool result = degreePlanSource.ReviewOnlyChange(degreePlanTarget4);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ReviewOnlyChange_TermsOutOfOrder()
        {
            // Term order doesn't matter.
            bool result = degreePlanSource.ReviewOnlyChange(degreePlanTarget);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ReviewOnlyChange_TargetExtraTerm()
        {
            // Target has an extra term.
            degreePlanTarget.AddTerm("2014/SP");
            bool result = degreePlanSource.ReviewOnlyChange(degreePlanTarget);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ReviewOnlyChange_SourceExtraTerm()
        {
            // Target has an extra term.
            degreePlanSource.AddTerm("2014/SP");
            bool result = degreePlanSource.ReviewOnlyChange(degreePlanTarget);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ReviewOnlyChange_DifferentTerms()
        {
            DegreePlan degreePlanTarget6 = new DegreePlan(1, personId, 1);
            degreePlanTarget6.AddTerm("2013/SP");
            degreePlanTarget6.AddTerm("2013/FA");
            degreePlanTarget6.AddCourse(new PlannedCourse(course: "111", coursePlaceholder: null), "2012/FA");
            bool result = degreePlanSource.ReviewOnlyChange(degreePlanTarget6);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ReviewOnlyChange_SourceExtraCourse()
        {
            // Target has an extra course in term 2012/FA.
            degreePlanSource.AddCourse(new PlannedCourse(course: "112", coursePlaceholder: null), "2012/FA");
            bool result = degreePlanSource.ReviewOnlyChange(degreePlanTarget);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ReviewOnlyChange_SourceDifferentCourseGradingType()
        {
            // Target has an extra course in term 2012/FA.
            PlannedCourse pc1 = new PlannedCourse(course: "112", section: null, gradingType: GradingType.Audit, coursePlaceholder: null);
            degreePlanSource.AddCourse(pc1, "2012/FA");
            degreePlanSource.AddCourse(new PlannedCourse(course: "112", coursePlaceholder: null), "2012/FA");
            bool result = degreePlanSource.ReviewOnlyChange(degreePlanTarget);
            Assert.IsFalse(result);
        }
    }

    [TestClass]
    public class DegreePlan_HasProtectionChange
    {
        // Arrange -- all tests
        DegreePlan currentDP;
        DegreePlan cachedDP;
        string termCode;

        [TestInitialize]
        public void Initialize()
        {
            currentDP = new DegreePlan(1, "0000001", 2, false);
            cachedDP = new DegreePlan(1, "0000001", 2, false);
            termCode = "2015SP";
        }

        [TestCleanup]
        public void CleanUp()
        {
            currentDP = null;
            cachedDP = null;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ThrowsExceptionIfVersionMisMatch()
        {
            // Arrange
            DegreePlan currentDP = new DegreePlan(1, "0000001", 2, false);
            DegreePlan cachedDP = new DegreePlan(1, "0000001", 3, false);

            // Act
            var result = currentDP.HasProtectedChange(cachedDP);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ThrowsExceptionIfPersonMisMatch()
        {
            // Arrange
            DegreePlan currentDP = new DegreePlan(1, "0000001", 2, false);
            DegreePlan cachedDP = new DegreePlan(1, "0000002", 2, false);

            // Act
            var result = currentDP.HasProtectedChange(cachedDP);
        }

        [TestMethod]
        public void NoProtectionChangeOnEmptyPlan()
        {
            // Act
            var result = currentDP.HasProtectedChange(cachedDP);
            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void VerifiesProtectededNonTermCourse()
        {
            // Arrange
            // cached degree plan
            cachedDP.NonTermPlannedCourses.Add(new PlannedCourse(course: "13", section: "22", gradingType: GradingType.Graded, status: Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, addedBy: null, addedOn: null, coursePlaceholder: null));
            cachedDP.NonTermPlannedCourses.Add(new PlannedCourse(course: "12", section: "21", gradingType: GradingType.Graded, status: Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, addedBy: null, addedOn: null, coursePlaceholder: null) { IsProtected = true });
            // current degree plan - unprotected course removed, protected course may not be removed
            currentDP.NonTermPlannedCourses.Add(new PlannedCourse(course: "12", section: "21", gradingType: GradingType.Graded, status: Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, addedBy: null, addedOn: null, coursePlaceholder: null) { IsProtected = true });
            var result = currentDP.HasProtectedChange(cachedDP);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void AllowsAddOfUnprotectededNonTermCourse()
        {
            // Arrange
            // cached degree plan
            cachedDP.NonTermPlannedCourses.Add(new PlannedCourse(course: "12", section: "21", gradingType: GradingType.Graded, status: Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, addedBy: null, addedOn: null, coursePlaceholder: null) { IsProtected = true });
            // current degree plan - unprotected course add is ok
            currentDP.NonTermPlannedCourses.Add(new PlannedCourse(course: "12", section: "21", gradingType: GradingType.Graded, status: Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, addedBy: null, addedOn: null, coursePlaceholder: null) { IsProtected = true });
            currentDP.NonTermPlannedCourses.Add(new PlannedCourse(course: "13", section: "22", gradingType: GradingType.Graded, status: Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, addedBy: null, addedOn: null, coursePlaceholder: null));

            // Act
            var result = currentDP.HasProtectedChange(cachedDP);
            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void VerifiesProtectedTermBasedCourse()
        {
            // Arrange
            // Cached degree plan
            cachedDP.AddCourse(new PlannedCourse(course: "13", section: null, gradingType: GradingType.Graded, status: Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, addedBy: null, addedOn: null, coursePlaceholder: null), termCode);
            cachedDP.AddCourse(new PlannedCourse(course: "12", section: "21", gradingType: GradingType.Graded, status: Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, addedBy: null, addedOn: null, coursePlaceholder: null) { IsProtected = true }, termCode);
            // Current degree plan - unprotected course removed, protected course must remain
            currentDP.AddCourse(new PlannedCourse(course: "12", section: "21", gradingType: GradingType.Graded, status: Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, addedBy: null, addedOn: null, coursePlaceholder: null) { IsProtected = true }, termCode);
            // Act
            var result = currentDP.HasProtectedChange(cachedDP);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void AllowsAddOfUnprotectedTermBasedCourse()
        {
            // Arrange
            // Cached degree plan
            cachedDP.AddCourse(new PlannedCourse(course: "12", section: "21", gradingType: GradingType.Graded, status: Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, addedBy: null, addedOn: null, coursePlaceholder: null) { IsProtected = true }, termCode);
            // Current degree plan - unprotected course add is ok
            currentDP.AddCourse(new PlannedCourse(course: "12", section: "21", gradingType: GradingType.Graded, status: Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, addedBy: null, addedOn: null, coursePlaceholder: null) { IsProtected = true }, termCode);
            currentDP.AddCourse(new PlannedCourse(course: "13", section: null, gradingType: GradingType.Graded, status: Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, addedBy: null, addedOn: null, coursePlaceholder: null), termCode);

            // Act
            var result = currentDP.HasProtectedChange(cachedDP);
            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void CannotRemoveProtectedTermBasedCourse()
        {
            // Arrange
            // Cached degree plan
            cachedDP.AddCourse(new PlannedCourse(course: "13", section: null, gradingType: GradingType.Graded, status: Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, addedBy: null, addedOn: null, coursePlaceholder: null), termCode);
            cachedDP.AddCourse(new PlannedCourse(course: "12", section: "21", gradingType: GradingType.Graded, status: Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, addedBy: null, addedOn: null, coursePlaceholder: null) { IsProtected = true }, termCode);
            // Current degree plan - attempt to remove protected course
            currentDP.AddCourse(new PlannedCourse(course: "13", section: null, gradingType: GradingType.Graded, status: Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, addedBy: null, addedOn: null, coursePlaceholder: null), termCode);

            // Act
            var result = currentDP.HasProtectedChange(cachedDP);
            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CannotAddProtectedTermBasedCourse()
        {
            // Arrange
            // Cached degree plan
            cachedDP.AddCourse(new PlannedCourse(course: "13", section: null, gradingType: GradingType.Graded, status: Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, addedBy: null, addedOn: null, coursePlaceholder: null), termCode);
            // Current degree plan - attempt to add protected course
            currentDP.AddCourse(new PlannedCourse(course: "13", section: null, gradingType: GradingType.Graded, status: Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, addedBy: null, addedOn: null, coursePlaceholder: null), termCode);
            currentDP.AddCourse(new PlannedCourse(course: "12", section: "21", gradingType: GradingType.Graded, status: Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, addedBy: null, addedOn: null, coursePlaceholder: null) { IsProtected = true }, termCode);

            // Act
            var result = currentDP.HasProtectedChange(cachedDP);
            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CannotRemoveProtectedNonTermCourse()
        {
            // Arrange
            // Cached degree plan
            cachedDP.NonTermPlannedCourses.Add(new PlannedCourse(course: "13", section: null, gradingType: GradingType.Graded, status: Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, addedBy: null, addedOn: null, coursePlaceholder: null));
            cachedDP.NonTermPlannedCourses.Add(new PlannedCourse(course: "12", section: "21", gradingType: GradingType.Graded, status: Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, addedBy: null, addedOn: null, coursePlaceholder: null) { IsProtected = true });
            // Current degree plan - attempt to remove protected course
            currentDP.AddCourse(new PlannedCourse(course: "13", section: null, gradingType: GradingType.Graded, status: Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, addedBy: null, addedOn: null, coursePlaceholder: null), termCode);

            // Act
            var result = currentDP.HasProtectedChange(cachedDP);
            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CannotAddProtectedNonTermCourse()
        {
            // Arrange
            // Cached degree plan
            cachedDP.NonTermPlannedCourses.Add(new PlannedCourse(course: "13", section: null, gradingType: GradingType.Graded, status: Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, addedBy: null, addedOn: null, coursePlaceholder: null));
            // Current degree plan - attempt to add protected course
            currentDP.NonTermPlannedCourses.Add(new PlannedCourse(course: "13", section: null, gradingType: GradingType.Graded, status: Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, addedBy: null, addedOn: null, coursePlaceholder: null));
            currentDP.NonTermPlannedCourses.Add(new PlannedCourse(course: "12", section: "21", gradingType: GradingType.Graded, status: Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, addedBy: null, addedOn: null, coursePlaceholder: null) { IsProtected = true });

            // Act
            var result = currentDP.HasProtectedChange(cachedDP);
            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowsExceptionIfCachedDegreePlanNull()
        {
            // Arrange
            cachedDP = null;
            // Act
            var result = currentDP.HasProtectedChange(cachedDP);
        }
    }

    [TestClass]
    public class DegreePlan_HasReviewChange
    {
        // Arrange -- all tests
        DegreePlan currentDP;
        DegreePlan cachedDP;

        [TestInitialize]
        public void Initialize()
        {
            currentDP = new DegreePlan(1, "0000001", 2, false);
            cachedDP = new DegreePlan(1, "0000001", 2, true);
        }

        [TestCleanup]
        public void CleanUp()
        {
            currentDP = null;
            cachedDP = null;
        }

        [TestMethod]
        public void ReturnsFalseIfNoReviewChanges()
        {
            // Act
            var result = currentDP.HasReviewChange(cachedDP);
            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void NoReviewChangeIfCurrentLastAdvisorEmptyAndCachedLastAdvisorNull()
        {
            // arrange
            currentDP.LastReviewedAdvisorId = string.Empty;
            // Act
            var result = currentDP.HasReviewChange(cachedDP);
            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void NoReviewChangeIfCachedLastAdvisorEmptyAndCurrentLastAdvisorNull()
        {
            // arrange
            cachedDP.LastReviewedAdvisorId = string.Empty;
            // Act
            var result = currentDP.HasReviewChange(cachedDP);
            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowsExceptionIfCachedDegreePlanNull()
        {
            // Arrange
            cachedDP = null;

            // Act
            currentDP.HasReviewChange(cachedDP);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ThrowsExceptionIfVersionMisMatch()
        {
            // Arrange
            DegreePlan currentDP = new DegreePlan(1, "0000001", 2, false);
            DegreePlan cachedDP = new DegreePlan(1, "0000001", 3, false);

            // Act
            var result = currentDP.HasReviewChange(cachedDP);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ThrowsExceptionIfPersonMisMatch()
        {
            // Arrange
            DegreePlan currentDP = new DegreePlan(1, "0000001", 2, false);
            DegreePlan cachedDP = new DegreePlan(1, "0000002", 2, false);

            // Act
            var result = currentDP.HasReviewChange(cachedDP);
        }

        [TestMethod]
        public void ReturnsTrueIfLastReviewDatesDiffer()
        {
            // Arrange
            currentDP.LastReviewedDate = DateTime.Now;
            cachedDP.LastReviewedDate = DateTime.Now - new TimeSpan(2, 0, 0, 0);

            // Act
            var result = currentDP.HasReviewChange(cachedDP);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ReturnsTrueIfReviewAdvisorDiffers()
        {
            // Arrange
            currentDP.LastReviewedAdvisorId = "0000004";
            cachedDP.LastReviewedAdvisorId = "0000005";

            // Act
            var result = currentDP.HasReviewChange(cachedDP);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ReturnsTrueIfApproverDiffers()
        {
            // Arrange
            var timestamp = DateTimeOffset.Now;
            currentDP.Approvals.Add(new DegreePlanApproval("0000001", DegreePlanApprovalStatus.Approved, timestamp, "01", "2014/FA"));
            currentDP.Approvals.Add(new DegreePlanApproval("0000001", DegreePlanApprovalStatus.Approved, timestamp, "02", "2014/FA"));
            cachedDP.Approvals.Add(new DegreePlanApproval("0000001", DegreePlanApprovalStatus.Approved, timestamp, "01", "2014/FA"));
            cachedDP.Approvals.Add(new DegreePlanApproval("0000003", DegreePlanApprovalStatus.Approved, timestamp, "02", "2014/FA"));

            // Act
            var result = currentDP.HasReviewChange(cachedDP);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ReturnsTrueIfApprovalStatusDiffers()
        {
            // Arrange
            var timestamp = DateTimeOffset.Now;
            currentDP.Approvals.Add(new DegreePlanApproval("0000001", DegreePlanApprovalStatus.Approved, timestamp, "01", "2014/FA"));
            cachedDP.Approvals.Add(new DegreePlanApproval("0000001", DegreePlanApprovalStatus.Denied, timestamp, "01", "2014/FA"));

            // Act
            var result = currentDP.HasReviewChange(cachedDP);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ReturnsTrueIfApprovalCountDiffers()
        {
            // Arrange
            var timestamp = DateTimeOffset.Now;
            currentDP.Approvals.Add(new DegreePlanApproval("0000001", DegreePlanApprovalStatus.Approved, timestamp, "01", "2014/FA"));
            currentDP.Approvals.Add(new DegreePlanApproval("0000001", DegreePlanApprovalStatus.Approved, timestamp, "02", "2014/FA"));
            cachedDP.Approvals.Add(new DegreePlanApproval("0000001", DegreePlanApprovalStatus.Approved, timestamp, "01", "2014/FA"));

            // Act
            var result = currentDP.HasReviewChange(cachedDP);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ReturnsTrueIfApprovalTimeDiffers()
        {
            // Arrange
            var timestamp = DateTimeOffset.Now;
            currentDP.Approvals.Add(new DegreePlanApproval("0000001", DegreePlanApprovalStatus.Approved, timestamp, "01", "2014/FA"));
            cachedDP.Approvals.Add(new DegreePlanApproval("0000001", DegreePlanApprovalStatus.Approved, timestamp + new TimeSpan(3, 0, 0), "01", "2014/FA"));

            // Act
            var result = currentDP.HasReviewChange(cachedDP);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ReturnsTrueIfApprovalCourseDiffers()
        {
            // Arrange
            var timestamp = DateTimeOffset.Now;
            currentDP.Approvals.Add(new DegreePlanApproval("0000001", DegreePlanApprovalStatus.Approved, timestamp, "01", "2014/FA"));
            cachedDP.Approvals.Add(new DegreePlanApproval("0000001", DegreePlanApprovalStatus.Approved, timestamp, "02", "2014/FA"));

            // Act
            var result = currentDP.HasReviewChange(cachedDP);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ReturnsTrueIfApprovalTermDiffers()
        {
            // Arrange
            var timestamp = DateTimeOffset.Now;
            currentDP.Approvals.Add(new DegreePlanApproval("0000001", DegreePlanApprovalStatus.Approved, timestamp, "01", "2014/FA"));
            cachedDP.Approvals.Add(new DegreePlanApproval("0000001", DegreePlanApprovalStatus.Approved, timestamp, "02", "2015/FA"));

            // Act
            var result = currentDP.HasReviewChange(cachedDP);

            // Assert
            Assert.IsTrue(result);
        }
    }

    [TestClass]
    public class IsPlanProtected
    {
        [TestMethod]
        public void IsPlanProtected_SetToTrueIfAnyTermCourseProtected()
        {
            // Arrange
            // create degree plan with protected term course
            var degreePlan = new DegreePlan("0000001");
            var term = "2014/FA";
            degreePlan.AddTerm(term);
            var pc1 = new PlannedCourse("12", "21", GradingType.Graded, Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, null, coursePlaceholder: null) { IsProtected = true };
            degreePlan.AddCourse(pc1, term);
            var pc2 = new PlannedCourse("13", null, GradingType.Graded, Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, null, coursePlaceholder: null);
            degreePlan.AddCourse(pc2, term);

            // Assert
            Assert.IsTrue(degreePlan.IsPlanProtected);
        }

        [TestMethod]
        public void IsPlanProtected_SetToTrueIfAnyNontermCourseProtected()
        {
            // Arrange
            // create degree plan with protected term course
            var degreePlan = new DegreePlan("0000001");
            var term = "2014/FA";
            degreePlan.AddTerm(term);
            var pc1 = new PlannedCourse("12", "21", GradingType.Graded, Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, null, coursePlaceholder: null) { IsProtected = true };
            degreePlan.NonTermPlannedCourses.Add(pc1);
            var pc2 = new PlannedCourse("13", "31", GradingType.Graded, Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, null, coursePlaceholder: null);
            degreePlan.NonTermPlannedCourses.Add(pc2);

            // Assert
            Assert.IsTrue(degreePlan.IsPlanProtected);
        }

        [TestMethod]
        public void IsPlanProtected_SetToFalseIfNoPlannedCourseProtected()
        {
            // Arrange
            // create degree plan with protected term course
            var degreePlan = new DegreePlan("0000001");
            var term = "2014/FA";
            degreePlan.AddTerm(term);
            var pc1 = new PlannedCourse("12", "21", GradingType.Graded, Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, null, coursePlaceholder: null) { IsProtected = false };
            degreePlan.AddCourse(pc1, term);
            var pc2 = new PlannedCourse("13", "31", GradingType.Graded, Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, null, coursePlaceholder: null);
            degreePlan.NonTermPlannedCourses.Add(pc2);
            var pc3 = new PlannedCourse("14", "41", GradingType.Graded, Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, null, coursePlaceholder: null);
            degreePlan.NonTermPlannedCourses.Add(pc3);

            // Assert
            Assert.IsFalse(degreePlan.IsPlanProtected);
        }
    }

    [TestClass]
    public class DegreePlan_UpdateMissingProtectionFlags
    {
        // Arrange -- all tests
        DegreePlan currentDP;
        DegreePlan storedDP;
        string termCode;

        [TestInitialize]
        public void Initialize()
        {
            currentDP = new DegreePlan(1, "0000001", 2, false);
            storedDP = new DegreePlan(1, "0000001", 2, false);


            termCode = "2015SP";
            // Arrange current degree plan with planned courses without protection
            // Nonterm courses
            currentDP.AddCourse(new PlannedCourse("11", "20", GradingType.Graded, Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, null, coursePlaceholder: null), null);
            currentDP.AddCourse(new PlannedCourse("12", "21", GradingType.Graded, Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, null, coursePlaceholder: null), null);
            currentDP.AddCourse(new PlannedCourse("14", "24", GradingType.Graded, Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, null, coursePlaceholder: null), null);
            currentDP.AddCourse(new PlannedCourse("14", "25", GradingType.Graded, Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, null, coursePlaceholder: null), null);
            currentDP.AddCourse(new PlannedCourse("14", "26", GradingType.Graded, Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, null, coursePlaceholder: null), null);

            // Term courses
            currentDP.AddCourse(new PlannedCourse("21", "30", GradingType.Graded, Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, null, coursePlaceholder: null), termCode);
            currentDP.AddCourse(new PlannedCourse("22", "31", GradingType.Graded, Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, null, coursePlaceholder: null), termCode);
            currentDP.AddCourse(new PlannedCourse("23", "32", GradingType.Graded, Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, null, coursePlaceholder: null), termCode);
            currentDP.AddCourse(new PlannedCourse("23", "", GradingType.Graded, Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, null, coursePlaceholder: null), termCode);
            currentDP.AddCourse(new PlannedCourse("24", "33", GradingType.Graded, Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, null, coursePlaceholder: null), termCode);
            currentDP.AddCourse(new PlannedCourse("24", "34", GradingType.Graded, Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, null, coursePlaceholder: null), termCode);
            currentDP.AddCourse(new PlannedCourse("24", "35", GradingType.Graded, Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, null, coursePlaceholder: null), termCode);
            currentDP.AddCourse(new PlannedCourse("26", "", GradingType.Graded, Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, null, coursePlaceholder: null), termCode);
            currentDP.AddCourse(new PlannedCourse("26", "", GradingType.Graded, Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, null, coursePlaceholder: null), termCode);

            // Arranged stored degree plan
            // NONTERM COURSES

            // Single exact match
            storedDP.AddCourse(new PlannedCourse("11", "20", GradingType.Graded, Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, null, coursePlaceholder: null) { IsProtected = true }, null);

            // Unprotected
            storedDP.AddCourse(new PlannedCourse("12", "21", GradingType.Graded, Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, null, coursePlaceholder: null), null);

            // Course/Section not found - go to first one (Section is 24)
            storedDP.AddCourse(new PlannedCourse("14", "42", GradingType.Graded, Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, null, coursePlaceholder: null) { IsProtected = true }, null);

            // plan is missing this protected item - none applied.
            storedDP.AddCourse(new PlannedCourse("15", "43", GradingType.Graded, Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, null, coursePlaceholder: null) { IsProtected = true }, null);

            // course without a section - finds 2 on the plan so just take first one.
            storedDP.AddCourse(new PlannedCourse("16", "", GradingType.Graded, Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, null, coursePlaceholder: null) { IsProtected = true }, null);

            // NOW SOME TERM COURSES

            // Single exact match
            storedDP.AddCourse(new PlannedCourse("21", "30", GradingType.Graded, Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, null, coursePlaceholder: null) { IsProtected = true }, termCode);
            // Unprotected
            storedDP.AddCourse(new PlannedCourse("22", "31", GradingType.Graded, Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, null, coursePlaceholder: null), termCode);
            // Course/Section not found but should go to the one without a section
            storedDP.AddCourse(new PlannedCourse("23", "52", GradingType.Graded, Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, null, coursePlaceholder: null) { IsProtected = true }, termCode);
            // Course/Section not found choose first in the list (
            storedDP.AddCourse(new PlannedCourse("24", "53", GradingType.Graded, Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, null, coursePlaceholder: null) { IsProtected = true }, termCode);
            // Course 25 not on the current plan - can't protect it.
            storedDP.AddCourse(new PlannedCourse("25", "54", GradingType.Graded, Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, null, coursePlaceholder: null) { IsProtected = true }, termCode);
            // Course 26 doesn't match either of the nonsection ones - just take first one.
            storedDP.AddCourse(new PlannedCourse("26", "55", GradingType.Graded, Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, null, null, coursePlaceholder: null) { IsProtected = true }, termCode);
            currentDP.UpdateMissingProtectionFlags(storedDP);
        }

        [TestCleanup]
        public void CleanUp()
        {
            currentDP = null;
            storedDP = null;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UpdateMissingProtectionFlags_NoStoredPlan()
        {
            var anotherDP = new DegreePlan(1, "0000001", 2, false);
            anotherDP.UpdateMissingProtectionFlags(null);
        }

        [TestMethod]
        public void UpdateMissingProtectionFlags_NonTerm_SingleMatch()
        {
            var pcs = currentDP.NonTermPlannedCourses.Where(c => c.CourseId == "11");
            Assert.AreEqual(1, pcs.Count());
            Assert.AreEqual(true, pcs.First().IsProtected);
        }

        [TestMethod]
        public void UpdateMissingProtectionFlags_NonTerm_Unprotected()
        {
            var pcs = currentDP.NonTermPlannedCourses.Where(c => c.CourseId == "12");
            Assert.AreEqual(1, pcs.Count());
            Assert.AreEqual(null, pcs.First().IsProtected);
        }


        [TestMethod]
        public void UpdateMissingProtectionFlags_NonTerm_Multiple_UseFirst()
        {
            // Expected that Course 14, Section 24 is protected but course 14 sections 25 and 26 are not.
            var protectedcourse = currentDP.NonTermPlannedCourses.Where(c => c.CourseId == "14" && c.SectionId == "24").FirstOrDefault();
            Assert.AreEqual(true, protectedcourse.IsProtected);
            var unprotected1 = currentDP.NonTermPlannedCourses.Where(c => c.CourseId == "14" && c.SectionId == "25").FirstOrDefault();
            Assert.IsNull(unprotected1.IsProtected);
            var unprotected2 = currentDP.NonTermPlannedCourses.Where(c => c.CourseId == "14" && c.SectionId == "26").FirstOrDefault();
            Assert.IsNull(unprotected2.IsProtected);
        }

        [TestMethod]
        public void UpdateMissingProtectionFlags_NonTerm_MissingOne()
        {
            // Expected that Course 15 is still  missing.  
            var pcs = currentDP.NonTermPlannedCourses.Where(c => c.CourseId == "15");
            Assert.AreEqual(0, pcs.Count());
        }

        [TestMethod]
        public void UpdateMissingProtectionFlags_Term_SingleMatch()
        {
            var termCourses = currentDP.GetPlannedCourses(termCode);
            var pcs = termCourses.Where(c => c.CourseId == "21");
            Assert.AreEqual(1, pcs.Count());
            Assert.AreEqual(true, pcs.First().IsProtected);
        }

        [TestMethod]
        public void UpdateMissingProtectionFlags_Term_Unprotected()
        {
            var termCourses = currentDP.GetPlannedCourses(termCode);
            var pcs = termCourses.Where(c => c.CourseId == "22");
            Assert.AreEqual(1, pcs.Count());
            Assert.AreEqual(null, pcs.First().IsProtected);
        }

        [TestMethod]
        public void UpdateMissingProtectionFlags_Term_Multiple_UseNoSection()
        {
            // Expected that Course 23, Section 32 is unprotected
            // And Course 23 with no section is protected
            var termCourses = currentDP.GetPlannedCourses(termCode);
            var unprotected = termCourses.Where(c => c.CourseId == "23" && c.SectionId == "32").FirstOrDefault();
            Assert.IsNull(unprotected.IsProtected);
            var protectedcourse = termCourses.Where(c => c.CourseId == "23" && string.IsNullOrEmpty(c.SectionId)).FirstOrDefault();
            Assert.AreEqual(true, protectedcourse.IsProtected);
        }

        [TestMethod]
        public void UpdateMissingProtectionFlags_Term_Multiple_UseFirst()
        {
            // Expected that Course 24, Section 33 is protected but course 24 sections 34 and 35 are not.
            var termCourses = currentDP.GetPlannedCourses(termCode);
            var protectedcourse = termCourses.Where(c => c.CourseId == "24" && c.SectionId == "33").FirstOrDefault();
            Assert.AreEqual(true, protectedcourse.IsProtected);
            var unprotected1 = termCourses.Where(c => c.CourseId == "24" && c.SectionId == "34").FirstOrDefault();
            Assert.IsNull(unprotected1.IsProtected);
            var unprotected2 = termCourses.Where(c => c.CourseId == "24" && c.SectionId == "35").FirstOrDefault();
            Assert.IsNull(unprotected2.IsProtected);
        }

        [TestMethod]
        public void UpdateMissingProtectionFlags_Term_MissingOne()
        {
            // Expected that Course 25 is still  missing.  
            var termCourses = currentDP.GetPlannedCourses(termCode);
            var pcs = termCourses.Where(c => c.CourseId == "25");
            Assert.AreEqual(0, pcs.Count());
        }

        [TestMethod]
        public void UpdateMissingProtectionFlags_Term_Multiple()
        {
            // Expected that Course 26 first one is protected and second is not
            var termCourses = currentDP.GetPlannedCourses(termCode);
            var pcs = termCourses.Where(c => c.CourseId == "26").ToList();
            Assert.AreEqual(2, pcs.Count());
            Assert.AreEqual(true, pcs[0].IsProtected);
            Assert.IsNull(pcs[1].IsProtected);
        }
    }

    [TestClass]
    public class DegreePlan_ValidateCourseOfferingInTerm
    {
        string personId;
        int degreePlanId;
        DegreePlan degreePlan;
        IEnumerable<Term> allTerms = new List<Term>();
        List<Course> allCourses = new List<Course>();
        PlannedCourse pc1;
        PlannedCourse pc2;
        Term FA2014;
        Term FA2015;
        Term FA2016;
        Term SP2015;
        Course course1;
        Course course2;
        List<RuleResult> RuleResults;
        ILogger logger;

        [TestInitialize]
        public async void Initialize()
        {
            personId = "00004002";
            degreePlanId = 1;
            degreePlan = new DegreePlan(degreePlanId, personId, 1);
            degreePlan.AddTerm("2014/FA");
            degreePlan.AddTerm("2015/SP");
            degreePlan.AddTerm("2015/FA");
            degreePlan.AddTerm("2016/SP");
            degreePlan.AddTerm("2016/FA");

            allTerms = await new TestTermRepository().GetAsync();
            FA2014 = allTerms.Where(t => t.Code == "2014/FA").FirstOrDefault();
            FA2015 = allTerms.Where(t => t.Code == "2015/FA").FirstOrDefault();
            FA2016 = allTerms.Where(t => t.Code == "2016/FA").FirstOrDefault();
            SP2015 = allTerms.Where(t => t.Code == "2015/SP").FirstOrDefault();

            var student = new TestStudentRepository().Get(personId);


            // Create test courses
            course1 = new Course("Course1", "Mathmatics", "General Math", new List<OfferingDepartment>() { new OfferingDepartment("MATH") }, "MATH", "400", "UG", new List<string>() { "UG" }, 3.0m, null, new List<CourseApproval>());
            course2 = new Course("Course2", "English", "General English", new List<OfferingDepartment>() { new OfferingDepartment("ENGL") }, "ENGL", "400", "UG", new List<string>() { "UG" }, 3.0m, null, new List<CourseApproval>());

            allCourses = new List<Course>();

            // Create planned courses 
            pc1 = new PlannedCourse(course: "Course1", section: null, gradingType: GradingType.Graded, coursePlaceholder: null);
            pc2 = new PlannedCourse(course: "Course2", section: null, gradingType: GradingType.Graded, coursePlaceholder: null);
            RuleResults = new List<RuleResult>();
            logger = new Mock<ILogger>().Object;
        }

        [TestCleanup]
        public void CleanUp()
        {
            degreePlan = null;
            course1 = null;
            course2 = null;
            allCourses = null;
        }

        [TestMethod]
        public void DegreePlan_ValidateCourseOfferingInTerm_NoCoursesOnPlan()
        {
            // Act
            degreePlan.CheckForConflicts(allTerms, allTerms, allCourses, new List<Section>(), new List<AcademicCredit>(), new List<Requirement>(), RuleResults, null);
            Assert.AreEqual(0, degreePlan.PlannedCourses.Count());
        }

        [TestMethod]
        public void DegreePlan_ValidateCourseOfferingInTerm_CourseAndTermNotRestricted()
        {
            // Term 2015/SP has no session or yearly cycle restrictions
            // Course 1 has no session or yearly cycle restrictions
            // Student location empty
            // Expecting no warnng

            // Arrange
            degreePlan.AddCourse(pc1, "2015/SP");
            course1.TermSessionCycle = "";
            course1.TermYearlyCycle = "";
            allCourses.Add(course1);
            allCourses.Add(course2);
            // Act
            degreePlan.CheckForConflicts(allTerms, allTerms, allCourses, new List<Section>(), new List<AcademicCredit>(), new List<Requirement>(), RuleResults, null);
            PlannedCourse updatedpc = degreePlan.GetPlannedCourses("2015/SP").ElementAt(0);
            // Assert
            Assert.AreEqual(0, updatedpc.Warnings.Count());
        }

        [TestMethod]
        public void DegreePlan_ValidateCourseOfferingInTerm_CourseRestrictedTermNotRestricted()
        {
            // Term 2015/SP has no session or yearly cycle restrictions
            // Course 1 has no session or yearly cycle restrictions
            // Student location empty
            // Expecting no warnng

            // Arrange
            degreePlan.AddCourse(pc1, "2015/SP");
            course1.TermSessionCycle = "F";
            course1.TermYearlyCycle = "E";
            allCourses.Add(course1);
            allCourses.Add(course2);
            // Act
            degreePlan.CheckForConflicts(allTerms, allTerms, allCourses, new List<Section>(), new List<AcademicCredit>(), new List<Requirement>(), RuleResults, null);
            PlannedCourse updatedpc = degreePlan.GetPlannedCourses("2015/SP").ElementAt(0);
            // Assert
            Assert.AreEqual(0, updatedpc.Warnings.Count());
        }

        [TestMethod]
        public void DegreePlan_ValidateCourseOfferingInTerm_CourseNotRestrictedTermRestricted()
        {
            // Term 2014/FA has session cycles F,FS and yearly cycle E.
            // Course 1 has no session or yearly cycle restrictions
            // Student location empty
            // Expecting no warnng

            // Arrange
            degreePlan.AddCourse(pc1, "2014/FA");
            course1.TermSessionCycle = null;
            course1.TermYearlyCycle = null;
            allCourses.Add(course1);
            allCourses.Add(course2);
            // Act
            degreePlan.CheckForConflicts(allTerms, allTerms, allCourses, new List<Section>(), new List<AcademicCredit>(), new List<Requirement>(), RuleResults, null);
            PlannedCourse updatedpc = degreePlan.GetPlannedCourses("2014/FA").ElementAt(0);
            // Assert
            Assert.AreEqual(0, updatedpc.Warnings.Count());
        }

        [TestMethod]
        public void DegreePlan_ValidateCourseOfferingInTerm_SpringCourseTermMultipleCycles_Warning()
        {
            // Term 2014/FA has session cycles F,FS and yearly cycle E.
            // Course 1 has session S
            // Student location empty
            // Expecting a warning of type CourseOfferingConflict
            // Arrange
            degreePlan.AddCourse(pc1, "2014/FA");
            course1.TermSessionCycle = "S";
            course1.TermYearlyCycle = "";
            allCourses.Add(course1);
            allCourses.Add(course2);
            // Act
            degreePlan.CheckForConflicts(allTerms, allTerms, allCourses, new List<Section>(), new List<AcademicCredit>(), new List<Requirement>(), RuleResults, null);
            PlannedCourse updatedpc = degreePlan.GetPlannedCourses("2014/FA").ElementAt(0);
            // Assert
            Assert.AreEqual(1, updatedpc.Warnings.Count());
            var warning = updatedpc.Warnings.Where(w => w.Type == PlannedCourseWarningType.CourseOfferingConflict).FirstOrDefault();
            Assert.IsNotNull(warning);
        }
        [TestMethod]
        public void DegreePlan_ValidateCourseOfferingInTerm_SpringCourseOnFallTerm_Warning()
        {
            // Term 2015/FA has session cycles F and no yearly cycle.
            // Course 1 has session S
            // Student location empty
            // Expecting a warning of type CourseOfferingConflict
            // Arrange
            degreePlan.AddCourse(pc1, "2015/FA");
            course1.TermSessionCycle = "S";
            course1.TermYearlyCycle = "";
            allCourses.Add(course1);
            allCourses.Add(course2);
            // Act
            degreePlan.CheckForConflicts(allTerms, allTerms, allCourses, new List<Section>(), new List<AcademicCredit>(), new List<Requirement>(), RuleResults);
            PlannedCourse updatedpc = degreePlan.GetPlannedCourses("2015/FA").ElementAt(0);
            // Assert
            Assert.AreEqual(1, updatedpc.Warnings.Count());
            var warning = updatedpc.Warnings.Where(w => w.Type == PlannedCourseWarningType.CourseOfferingConflict).FirstOrDefault();
            Assert.IsNotNull(warning);
        }

        [TestMethod]
        public void DegreePlan_ValidateCourseOfferingInTerm_OddYearCourseOnEvenYearTerm_Warning()
        {
            // Term 2014/FA has session cycles F,FS and yearly cycle E.
            // Course 1 has YearlyCycle of O
            // Expecting a warning of type CourseOfferingConflict
            // Arrange
            degreePlan.AddCourse(pc1, "2014/FA");
            course1.TermSessionCycle = "";
            course1.TermYearlyCycle = "O";
            allCourses.Add(course1);
            allCourses.Add(course2);
            // Act
            degreePlan.CheckForConflicts(allTerms, allTerms, allCourses, new List<Section>(), new List<AcademicCredit>(), new List<Requirement>(), RuleResults);
            PlannedCourse updatedpc = degreePlan.GetPlannedCourses("2014/FA").ElementAt(0);
            // Assert
            Assert.AreEqual(1, updatedpc.Warnings.Count());
            var warning = updatedpc.Warnings.Where(w => w.Type == PlannedCourseWarningType.CourseOfferingConflict).FirstOrDefault();
            Assert.IsNotNull(warning);
        }

        [TestMethod]
        public void DegreePlan_ValidateCourseOfferingInTerm_OddYearCourseOnMultiYearTerm_Warning()
        {
            // Term 2016/SP has no session cycles and yearly cycle E.
            // Course 1 has yearly cycle of O
            // Expecting a warning of type CourseOfferingConflict
            // Arrange
            degreePlan.AddCourse(pc1, "2016/SP");
            course1.TermSessionCycle = "";
            course1.TermYearlyCycle = "O";
            allCourses.Add(course1);
            allCourses.Add(course2);
            // Act
            degreePlan.CheckForConflicts(allTerms, allTerms, allCourses, new List<Section>(), new List<AcademicCredit>(), new List<Requirement>(), RuleResults);
            PlannedCourse updatedpc = degreePlan.GetPlannedCourses("2016/SP").ElementAt(0);
            // Assert
            Assert.AreEqual(1, updatedpc.Warnings.Count());
            var warning = updatedpc.Warnings.Where(w => w.Type == PlannedCourseWarningType.CourseOfferingConflict).FirstOrDefault();
            Assert.IsNotNull(warning);
        }

        [TestMethod]
        public void DegreePlan_ValidateCourseOfferingInTerm_NoCourses()
        {
            // No courses
            // Arrange
            degreePlan.AddCourse(pc1, "2016/SP");

            // Act
            degreePlan.CheckForConflicts(allTerms, allTerms, new List<Course>(), new List<Section>(), new List<AcademicCredit>(), new List<Requirement>(), RuleResults);
            PlannedCourse updatedpc = degreePlan.GetPlannedCourses("2016/SP").ElementAt(0);
            // Assert
            Assert.AreEqual(0, updatedpc.Warnings.Count());

        }

        [TestMethod]
        public void DegreePlan_ValidateCourseOfferingInTerm_NoTerms()
        {
            // No terms
            // Arrange
            degreePlan.AddCourse(pc1, "2016/SP");
            course1.TermSessionCycle = "";
            course1.TermYearlyCycle = "O";
            allCourses.Add(course1);
            allCourses.Add(course2);

            // Act
            degreePlan.CheckForConflicts(new List<Term>(), new List<Term>(), allCourses, new List<Section>(), new List<AcademicCredit>(), new List<Requirement>(), RuleResults);
            PlannedCourse updatedpc = degreePlan.GetPlannedCourses("2016/SP").ElementAt(0);
            // Assert
            Assert.AreEqual(0, updatedpc.Warnings.Count());

        }

        [TestMethod]
        public void DegreePlan_ValidateCourseOfferingInTerm_OddYearCourseWithSection_OnEvenYearTerm_NoWarning()
        {
            // Term 2016/SP has no session cycles and yearly cycle E.
            // Course 1 has yearly cycle of O - which should represent a conflict but
            // since the planned course has a section then expecting no warning.
            // Arrange
            var pc3 = new PlannedCourse("Course1", "Section1", GradingType.Graded);
            degreePlan.AddCourse(pc3, "2016/SP");
            course1.TermSessionCycle = "";
            course1.TermYearlyCycle = "O";
            allCourses.Add(course1);
            allCourses.Add(course2);
            // Act
            degreePlan.CheckForConflicts(allTerms, allTerms, allCourses, new List<Section>(), new List<AcademicCredit>(), new List<Requirement>(), RuleResults);
            PlannedCourse updatedpc = degreePlan.GetPlannedCourses("2016/SP").ElementAt(0);
            // Assert
            Assert.AreEqual(0, updatedpc.Warnings.Count());
        }

        [TestMethod]
        public void DegreePlan_ValidateCourseOfferingInTerm_CourseLocationRestrictions_NoStudentLocation()
        {
            // Term 2015/SP has no session or yearly cycle restrictions
            // Course 1 has no session or yearly cycle restrictions
            // Student location empty
            // Expecting no warnng

            // Arrange
            degreePlan.AddCourse(pc1, "2015/SP");
            course1.TermSessionCycle = "";
            course1.TermYearlyCycle = "";
            course1.AddLocationCycleRestriction(new LocationCycleRestriction("Main", "F", "O"));
            allCourses.Add(course1);
            allCourses.Add(course2);
            // Act
            degreePlan.CheckForConflicts(allTerms, allTerms, allCourses, new List<Section>(), new List<AcademicCredit>(), new List<Requirement>(), RuleResults, null);
            PlannedCourse updatedpc = degreePlan.GetPlannedCourses("2015/SP").ElementAt(0);
            // Assert
            Assert.AreEqual(0, updatedpc.Warnings.Count());
        }

        [TestMethod]
        public void DegreePlan_ValidateCourseOfferingInTerm_CourseLocationRestrictions_StudentLocationDoesNotMatch()
        {
            // Term 2014/FA has session cycles F,FS and yearly cycle E.
            // Course 1 has general session or yearly cycle restrictions of F and E which should match
            //     but it also has an override for location "Main" and "North" that does not match.
            // Student location is "South" so location restrictions are not used.
            // Expecting no warnng

            // Arrange
            degreePlan.AddCourse(pc1, "2014/FA");
            course1.TermSessionCycle = "F";
            course1.TermYearlyCycle = "E";
            course1.AddLocationCycleRestriction(new LocationCycleRestriction("Main", "S", "O"));
            course1.AddLocationCycleRestriction(new LocationCycleRestriction("North", "", "O"));
            allCourses.Add(course1);
            allCourses.Add(course2);
            // Act
            degreePlan.CheckForConflicts(allTerms, allTerms, allCourses, new List<Section>(), new List<AcademicCredit>(), new List<Requirement>(), RuleResults, "South");
            PlannedCourse updatedpc = degreePlan.GetPlannedCourses("2014/FA").ElementAt(0);
            // Assert
            Assert.AreEqual(0, updatedpc.Warnings.Count());
        }

        [TestMethod]
        public void DegreePlan_ValidateCourseOfferingInTerm_CourseLocationRestrictions_StudentLocationMatches()
        {
            // Term 2014/FA has session cycles F,FS and yearly cycle E.
            // Course 1 has general session or yearly cycle restrictions of F and E which should match
            //     but it also has an override for locations "Main" & "North" but "North"yearly cycle will not match the term. 
            // Student location is "North"
            // Expecting a warning

            // Arrange
            degreePlan.AddCourse(pc1, "2014/FA");
            course1.TermSessionCycle = "F";
            course1.TermYearlyCycle = "E";
            course1.AddLocationCycleRestriction(new LocationCycleRestriction("Main", "F", ""));
            course1.AddLocationCycleRestriction(new LocationCycleRestriction("North", "", "O"));
            allCourses.Add(course1);
            allCourses.Add(course2);
            // Act
            degreePlan.CheckForConflicts(allTerms, allTerms, allCourses, new List<Section>(), new List<AcademicCredit>(), new List<Requirement>(), RuleResults, "North");
            PlannedCourse updatedpc = degreePlan.GetPlannedCourses("2014/FA").ElementAt(0);
            // Assert
            Assert.AreEqual(1, updatedpc.Warnings.Count());
            var warning = updatedpc.Warnings.Where(w => w.Type == PlannedCourseWarningType.CourseOfferingConflict).FirstOrDefault();
            Assert.IsNotNull(warning);
        }

        [TestMethod]
        public void DegreePlan_ValidateCourseOfferingInTerm_CourseLocationRestrictions_FailsSession()
        {
            // Term 2014/FA has session cycles F,FS and yearly cycle E.
            // Course 1 has no general session or yearly cycle restrictions 
            //     but it does have an override by locations "Main" & "North". "Main" does not match the term in session cycle.  
            // Student location is "Main"
            // Expecting a warning
            // Expecting a warning of type CourseOfferingConflict
            // Arrange
            degreePlan.AddCourse(pc1, "2014/FA");
            course1.TermSessionCycle = "";
            course1.TermYearlyCycle = "";
            course1.AddLocationCycleRestriction(new LocationCycleRestriction("Main", "S", ""));
            course1.AddLocationCycleRestriction(new LocationCycleRestriction("North", "", "E"));
            allCourses.Add(course1);
            allCourses.Add(course2);
            // Act
            degreePlan.CheckForConflicts(allTerms, allTerms, allCourses, new List<Section>(), new List<AcademicCredit>(), new List<Requirement>(), RuleResults, "Main");
            PlannedCourse updatedpc = degreePlan.GetPlannedCourses("2014/FA").ElementAt(0);
            // Assert
            Assert.AreEqual(1, updatedpc.Warnings.Count());
            var warning = updatedpc.Warnings.Where(w => w.Type == PlannedCourseWarningType.CourseOfferingConflict).FirstOrDefault();
            Assert.IsNotNull(warning);
        }

        [TestMethod]
        public void DegreePlan_ValidateCourseOfferingInTerm_CourseMultipleLocationRestrictions_StudentLocationMatches()
        {
            // Term 2014/FA has session cycles F,FS and yearly cycle E.
            // Course 1 has general session or yearly cycle restrictions of F and E which should match
            //     but it also has an override for locations "Main" & "North". "North" has location restrictions and 3 match student's location but none match the term's info. 
            // Student location is "North"
            // Expecting a warning

            // Arrange
            degreePlan.AddCourse(pc1, "2014/FA");
            course1.TermSessionCycle = "F";
            course1.TermYearlyCycle = "E";
            course1.AddLocationCycleRestriction(new LocationCycleRestriction("Main", "F", ""));
            course1.AddLocationCycleRestriction(new LocationCycleRestriction("North", "", "A"));
            course1.AddLocationCycleRestriction(new LocationCycleRestriction("North", "F", "O"));
            course1.AddLocationCycleRestriction(new LocationCycleRestriction("North", "S", ""));
            allCourses.Add(course1);
            allCourses.Add(course2);
            // Act
            degreePlan.CheckForConflicts(allTerms, allTerms, allCourses, new List<Section>(), new List<AcademicCredit>(), new List<Requirement>(), RuleResults, "North");
            PlannedCourse updatedpc = degreePlan.GetPlannedCourses("2014/FA").ElementAt(0);
            // Assert
            Assert.AreEqual(1, updatedpc.Warnings.Count());
            var warning = updatedpc.Warnings.Where(w => w.Type == PlannedCourseWarningType.CourseOfferingConflict).FirstOrDefault();
            Assert.IsNotNull(warning);
        }

        [TestMethod]
        public void DegreePlan_ValidateCourseOfferingInTerm_CourseMultipleLocationRestrictions_StudentLocationMatches_NoWarning()
        {
            // Term 2014/FA has session cycles F,FS and yearly cycle E.
            // Course 1 has general session or yearly cycle restrictions of F and E which should match
            //     but it also has an override for locations "Main" & "North". "North" had 3 location restrictions and one does match - Fall every year. 
            // Student location is "North"
            // Expecting no warning

            // Arrange
            degreePlan.AddCourse(pc1, "2014/FA");
            course1.TermSessionCycle = "F";
            course1.TermYearlyCycle = "E";
            course1.AddLocationCycleRestriction(new LocationCycleRestriction("Main", "F", ""));
            course1.AddLocationCycleRestriction(new LocationCycleRestriction("North", "", "A"));
            course1.AddLocationCycleRestriction(new LocationCycleRestriction("North", "F", ""));
            course1.AddLocationCycleRestriction(new LocationCycleRestriction("North", "S", "O"));
            allCourses.Add(course1);
            allCourses.Add(course2);
            // Act
            degreePlan.CheckForConflicts(allTerms, allTerms, allCourses, new List<Section>(), new List<AcademicCredit>(), new List<Requirement>(), RuleResults, "North");
            PlannedCourse updatedpc = degreePlan.GetPlannedCourses("2014/FA").ElementAt(0);
            // Assert
            Assert.AreEqual(0, updatedpc.Warnings.Count());

        }

        [TestMethod]
        public void DegreePlan_ValidateCourseOfferingInTerm_CourseMultipleLocationRestrictions_StudentLocationMatches_NoWarning2()
        {
            // Term 2014/FA has session cycles F,FS and yearly cycle E.
            // Course 1 has general session or yearly cycle restrictions of F and E which should match
            //     but it also has an override for locations "Main" & "North". "North" had 3 location restrictions and one does match - Fall Even. 
            // Student location is "North"
            // Expecting no warning

            // Arrange
            degreePlan.AddCourse(pc1, "2014/FA");
            course1.TermSessionCycle = "F";
            course1.TermYearlyCycle = "E";
            course1.AddLocationCycleRestriction(new LocationCycleRestriction("Main", "F", ""));
            course1.AddLocationCycleRestriction(new LocationCycleRestriction("North", "", "A"));
            course1.AddLocationCycleRestriction(new LocationCycleRestriction("North", "F", "E"));
            course1.AddLocationCycleRestriction(new LocationCycleRestriction("North", "S", ""));
            allCourses.Add(course1);
            allCourses.Add(course2);
            // Act
            degreePlan.CheckForConflicts(allTerms, allTerms, allCourses, new List<Section>(), new List<AcademicCredit>(), new List<Requirement>(), RuleResults, "North");
            PlannedCourse updatedpc = degreePlan.GetPlannedCourses("2014/FA").ElementAt(0);
            // Assert
            Assert.AreEqual(0, updatedpc.Warnings.Count());

        }

        [TestMethod]
        public void DegreePlan_ValidateCourseOfferingInTerm_CourseMultipleLocationRestrictions_StudentLocationMatches_NoWarning3()
        {
            // Term 2014/FA has session cycles F,FS and yearly cycle E.
            // Course 1 has general session or yearly cycle restrictions of F and E which should match
            //     but it also has an override for locations "Main" & "North". "North" had 3 location restrictions and one does match - Any session Even. 
            // Student location is "North"
            // Expecting no warning

            // Arrange
            degreePlan.AddCourse(pc1, "2014/FA");
            course1.TermSessionCycle = "F";
            course1.TermYearlyCycle = "E";
            course1.AddLocationCycleRestriction(new LocationCycleRestriction("Main", "F", ""));
            course1.AddLocationCycleRestriction(new LocationCycleRestriction("North", "", "A"));
            course1.AddLocationCycleRestriction(new LocationCycleRestriction("North", "", "E"));
            course1.AddLocationCycleRestriction(new LocationCycleRestriction("North", "S", ""));
            allCourses.Add(course1);
            allCourses.Add(course2);
            // Act
            degreePlan.CheckForConflicts(allTerms, allTerms, allCourses, new List<Section>(), new List<AcademicCredit>(), new List<Requirement>(), RuleResults, "North");
            PlannedCourse updatedpc = degreePlan.GetPlannedCourses("2014/FA").ElementAt(0);
            // Assert
            Assert.AreEqual(0, updatedpc.Warnings.Count());

        }

        [TestMethod]
        public void DegreePlan_ValidateCourseOfferingInTerm_CourseMultipleLocationRestrictions_StudentLocationMatches_NoWarning4()
        {
            // Term 2015/SP has no session cycles or yearly cycles.
            // Course 1 has general session or yearly cycle restrictions of F and E which should match
            //     but it also has an override for locations "Main" & "North". "North" has 3 location restrictions. 
            // Student location is "North"
            // Expecting no warning cause term has nothing.

            // Arrange
            degreePlan.AddCourse(pc1, "2014/FA");
            course1.TermSessionCycle = "F";
            course1.TermYearlyCycle = "E";
            course1.AddLocationCycleRestriction(new LocationCycleRestriction("Main", "F", ""));
            course1.AddLocationCycleRestriction(new LocationCycleRestriction("North", "", "A"));
            course1.AddLocationCycleRestriction(new LocationCycleRestriction("North", "F", "E"));
            course1.AddLocationCycleRestriction(new LocationCycleRestriction("North", "S", ""));
            allCourses.Add(course1);
            allCourses.Add(course2);
            // Act
            degreePlan.CheckForConflicts(allTerms, allTerms, allCourses, new List<Section>(), new List<AcademicCredit>(), new List<Requirement>(), RuleResults, "North");
            PlannedCourse updatedpc = degreePlan.GetPlannedCourses("2014/FA").ElementAt(0);
            // Assert
            Assert.AreEqual(0, updatedpc.Warnings.Count());

        }
    }
}