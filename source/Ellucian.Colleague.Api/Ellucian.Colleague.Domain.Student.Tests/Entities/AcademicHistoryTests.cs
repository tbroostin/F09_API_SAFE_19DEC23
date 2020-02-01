// Copyright 2014-2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Web.Http.TestUtil;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class AcademicHistoryConstructorTests_Unrestricted
    {
        private IEnumerable<AcademicCredit> studentAcademicCredits;
        private AcademicHistory history;
        TestAcademicCreditRepository testRepo;
        private AcademicTerm acTerm;
        private AcademicCredit ac;

        [TestInitialize]
        public void Initialize()
        {
            testRepo = new TestAcademicCreditRepository();
            var allCredits = testRepo.GetAsync();
            studentAcademicCredits = testRepo.GetAsync().Result.Where(acred => acred.Status != CreditStatus.Unknown);
            GradeRestriction gradeRestriction = new GradeRestriction(false);
            history = new AcademicHistory(studentAcademicCredits, gradeRestriction, null);
            acTerm = history.AcademicTerms.ElementAt(0);
            ac = history.NonTermAcademicCredits.ElementAt(0);
        }

        [TestCleanup]
        public void Cleanup()
        {
            testRepo = null;
        }

        [TestMethod]
        public void ReturnsAcademicHistory_NonTermAcadCreds()
        {
            var nontermAcadCredit = studentAcademicCredits.Where(ac => string.IsNullOrEmpty(ac.TermCode) && (ac.Status == CreditStatus.New || ac.Status == CreditStatus.Add || ac.Status == CreditStatus.Preliminary || ac.Status == CreditStatus.Withdrawn || ac.Status == CreditStatus.TransferOrNonCourse));
            Assert.AreEqual(nontermAcadCredit.Count(), history.NonTermAcademicCredits.Count());
        }

        [TestMethod]
        public void ReturnsAcademicHistory_NumberAcademicTerms()
        {
            Assert.AreEqual(12, history.AcademicTerms.Count());
        }

        [TestMethod]
        public void ReturnsHistory_TermGradePointAverage()
        {
            var gpaCredits = acTerm.AcademicCredits.Sum(ac=>ac.AdjustedGpaCredit);
            var gpa = acTerm.AcademicCredits.Sum(ac => ac.AdjustedGradePoints) / gpaCredits; 
            // at last check, this should be about 3.48. Not checking for zero divisor; shouldn't be zero, if it is there is a problem with the data
            Assert.AreEqual(gpa, acTerm.GradePointAverage);
        }

        [TestMethod]
        public void ReturnsHistory_TermGradePointAverage_For_GPACredit_Null()
        {
            var acTerm = history.AcademicTerms.Where(a => a.TermId == "2017/SP").First();
            var gpaCredits = acTerm.AcademicCredits.Sum(ac => ac.GpaCredit ?? 0m);
            var gpa = acTerm.AcademicCredits.Sum(agp => agp.GradePoints)/gpaCredits;
            Assert.AreEqual(gpa, acTerm.GradePointAverage);
            Assert.AreEqual(3.619m, Math.Round( Convert.ToDecimal(acTerm.GradePointAverage), 3));
        }

        [TestMethod]
        public void ReturnsHistory_TermCredits_For_CreditsCompleted_Null()
        {
            var acTerm = history.AcademicTerms.Where(a => a.TermId == "2017/SP").First();
            var termCredits = acTerm.AcademicCredits.Sum(tc => tc.CompletedCredit ?? 0m);
            Assert.AreEqual(termCredits, acTerm.Credits);
            Assert.AreEqual(24.00m, acTerm.Credits);
        }

        [TestMethod]
        public void ReturnsHistory_TermCredits()
        {
            var acadCr = studentAcademicCredits.Where(ac => ac.TermCode == acTerm.TermId && (ac.Status == CreditStatus.New || ac.Status == CreditStatus.Add || ac.Status == CreditStatus.Withdrawn || ac.Status == CreditStatus.TransferOrNonCourse));
            Assert.AreEqual(acadCr.Sum(cr=>cr.CompletedCredit), acTerm.Credits);
        }

        [TestMethod]
        public void ReturnsHistory_AcademicCreditsCount()
        {
            var acadCr = studentAcademicCredits.Where(ac => ac.TermCode == acTerm.TermId && (ac.Status == CreditStatus.New || ac.Status == CreditStatus.Add || ac.Status == CreditStatus.Withdrawn || ac.Status == CreditStatus.TransferOrNonCourse));
            Assert.AreEqual(acadCr.Count(), acTerm.AcademicCredits.Count());
        }

        [TestMethod]
        public void AcademicHistory_AcademicCreditCourseId()
        {
            Assert.AreEqual("46", ac.Course.Id);
        }

        [TestMethod]
        public void AcademicHistory_AcademicCreditSectionId()
        {
            Assert.AreEqual("8039", ac.SectionId);
        }

        [TestMethod]
        public void AcademicHistory_AcademicCreditCredits()
        {
            Assert.AreEqual(3m, ac.Credit);
        }

        [TestMethod]
        public void AcademicHistory_AcademicCreditGradePoints()
        {
            Assert.AreEqual(9m, ac.GradePoints);
        }

        [TestMethod]
        public void AcademicHistory_AcademicCreditGpaCredit()
        {
            Assert.AreEqual(3m, ac.GpaCredit);
        }

        [TestMethod]
        public void AcademicHistory_AcademicCreditVerifiedGrade()
        {
            Assert.AreEqual("B", ac.VerifiedGrade.LetterGrade);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullCreditsAcademicHistory_ThrowsException()
        {
            AcademicHistory noHistory = new AcademicHistory(null, new GradeRestriction(false), null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullGradeRestrictionAcademicHistory_ThrowsException()
        {
            AcademicHistory noHistory = new AcademicHistory(studentAcademicCredits, null, null);
        }

        [TestMethod]
        public void NoAcademicHistory_NumberAcademicTerms()
        {
            IEnumerable<AcademicCredit> emptyList = new List<AcademicCredit>();
            AcademicHistory noHistory = new AcademicHistory(emptyList, new GradeRestriction(false), null);
            Assert.AreEqual(0, noHistory.AcademicTerms.Count());
        }

        [TestMethod]
        public void NoAcademicHistory_NumberNonTermAcadCreds()
        {
            IEnumerable<AcademicCredit> emptyList = new List<AcademicCredit>();
            AcademicHistory noHistory = new AcademicHistory(emptyList, new GradeRestriction(false), null);
            Assert.AreEqual(0, noHistory.NonTermAcademicCredits.Count());
        }


        [TestMethod]
        public void ReturnsHistory_GradeRestrictions()
        {
            Assert.IsFalse(history.GradeRestriction.IsRestricted);
            Assert.AreEqual(0, history.GradeRestriction.Reasons.Count());
        }

        [TestMethod]
        public void ReturnsHistory_ExcludeUnknownStatusAcademicCredits()
        {
            List<AcademicCredit> credits = new List<AcademicCredit>();
            foreach (var term in history.AcademicTerms)
            {
                credits.AddRange(term.AcademicCredits);
            }
            Assert.IsFalse(credits.Any(c => c.Status == CreditStatus.Unknown));
        }


        [TestMethod]
        public void ReturnsHistory_WithReplacementsFlagged()
        {
            var replacements = new List<AcademicCredit>();
            foreach (var historyTerm in history.AcademicTerms)
            {
                replacements.AddRange(historyTerm.AcademicCredits.Where(ac => ac.ReplacementStatus == ReplacementStatus.Replacement));
            }
            // There is one replacement in the academic credit repository.. MUSC*210
            Assert.AreEqual(1, replacements.Count());
            Assert.AreEqual("66", replacements.ElementAt(0).Id);
        }

        [TestMethod]
        public void ReturnsHistory_WithPossibleReplacementsFlagged()
        {
            var possibleReplacements = new List<AcademicCredit>();
            var possiblyReplaced = new List<AcademicCredit>();
            foreach (var historyTerm in history.AcademicTerms)
            {
                possibleReplacements.AddRange(historyTerm.AcademicCredits.Where(ac => ac.ReplacementStatus == ReplacementStatus.PossibleReplacement));
                possiblyReplaced.AddRange(historyTerm.AcademicCredits.Where(ac => ac.ReplacedStatus == ReplacedStatus.ReplaceInProgress));
            }
            // There is one possible replacement/possibly replaced pair in the academic credit repository.. MUSC*211
            // MATH-460 is also replaced but it is with a non-graded drop so it does not qualify. 
            Assert.AreEqual(1, possibleReplacements.Count());
            Assert.AreEqual("68", possibleReplacements.ElementAt(0).Id);
            Assert.AreEqual(1, possiblyReplaced.Count());
            Assert.AreEqual("67", possiblyReplaced.ElementAt(0).Id);
        }


    }

    [TestClass]
    public class AcademicHistoryConstructorTests_Restricted
    {
        private IEnumerable<AcademicCredit> studentAcademicCredits;
        private AcademicHistory history;
        TestAcademicCreditRepository testRepo;
        private AcademicTerm acTerm;
        private AcademicCredit ac;

        [TestInitialize]
        public void Initialize()
        {
            testRepo = new TestAcademicCreditRepository();
            studentAcademicCredits = testRepo.GetAsync().Result;
            GradeRestriction gradeRestriction = new GradeRestriction(true);
            gradeRestriction.AddReason("Unpaid library fines.");
            gradeRestriction.AddReason("Room damage.");
            history = new AcademicHistory(studentAcademicCredits, gradeRestriction,null);
            acTerm = history.AcademicTerms.ElementAt(0);
            ac = history.NonTermAcademicCredits.ElementAt(0);
        }

        [TestCleanup]
        public void Cleanup()
        {
            testRepo = null;
        }

        [TestMethod]
        public void ReturnsAcademicHistory_NonTermAcadCreds()
        {
            var nontermAcadCredit = studentAcademicCredits.Where(ac => string.IsNullOrEmpty(ac.TermCode) && (ac.Status == CreditStatus.New || ac.Status == CreditStatus.Preliminary || ac.Status == CreditStatus.Add || ac.Status == CreditStatus.Withdrawn || ac.Status == CreditStatus.TransferOrNonCourse));
            Assert.AreEqual(nontermAcadCredit.Count(), history.NonTermAcademicCredits.Count());
        }

        [TestMethod]
        public void ReturnsAcademicHistory_NumberAcademicTerms()
        {
            Assert.AreEqual(12, history.AcademicTerms.Count());
        }

        [TestMethod]
        public void ReturnsHistory_TermGradePointAverage()
        {

            Assert.AreEqual(null, acTerm.GradePointAverage);
        }

        [TestMethod]
        public void ReturnsHistory_TermCredits()
        {
            var acadCr = studentAcademicCredits.Where(ac => ac.TermCode == acTerm.TermId && (ac.Status == CreditStatus.New || ac.Status == CreditStatus.Add || ac.Status == CreditStatus.Withdrawn || ac.Status == CreditStatus.TransferOrNonCourse));
            Assert.AreEqual(acadCr.Sum(cr=>cr.CompletedCredit), acTerm.Credits);
        }

        [TestMethod]
        public void ReturnsHistory_AcademicCreditsCount()
        {
            var acadCr = studentAcademicCredits.Where(ac => ac.TermCode == acTerm.TermId && (ac.Status == CreditStatus.New || ac.Status == CreditStatus.Add || ac.Status == CreditStatus.Withdrawn || ac.Status == CreditStatus.TransferOrNonCourse));
            Assert.AreEqual(acadCr.Count(), acTerm.AcademicCredits.Count());
        }

        [TestMethod]
        public void AcademicHistory_AcademicCreditCourseId()
        {
            Assert.AreEqual("46", ac.Course.Id);
        }

        [TestMethod]
        public void AcademicHistory_AcademicCreditSectionId()
        {
            Assert.AreEqual("8039", ac.SectionId);
        }

        [TestMethod]
        public void AcademicHistory_AcademicCreditCredits()
        {
            Assert.AreEqual(3m, ac.Credit);
        }

        [TestMethod]
        public void AcademicHistory_AcademicCreditGradePoints()
        {
            Assert.AreEqual(0, ac.GradePoints);
        }
        [TestMethod]
        public void AcademicHistory_AcademicCreditGpaCredit()
        {
            Assert.AreEqual(0, ac.GpaCredit);
        }

        [TestMethod]
        public void AcademicHistory_AcademicCreditVerifiedGrade()
        {
            Assert.AreEqual(null, ac.VerifiedGrade);
        }
        [TestMethod]
        public void ReturnsHistory_GradeRestrictions()
        {
            Assert.IsTrue(history.GradeRestriction.IsRestricted);
            Assert.AreEqual(2, history.GradeRestriction.Reasons.Count());
        }
    }


    /// <summary>
    /// Scenarios covered
    ///academic credits that are not repeated
    ///academic credits of same course that have repeatedCreditsIds collection and course does not allow retakes for credits (all 3 are in-progress)
    ///academic credits of same course that have repeatedCreditsIds collection and course does not allow retakes for credits (all 3 are completed)
    ///when one of the acad credit is replaced (3) - 2 are completed
    ///multiple courses with repetitions
    ///Equated Courses
    ///
    ///Please not we are playing with data as if Colleague has already done its homework by putting Replaced flag
    ///based upon parameters like grade policy and grade scheme.
    ///Our purpose of code is that we should put approrpiate replaced and replacement flag as needed and should not throw
    ///completed courses over dropped/withdrawn or failed courses
    /// We will be adding replacement flag to all the completed courses if any of the course has repeated flag
    /// We will only be adding replaceInProgress or Possiblereplacement when processing completed courses with in-progress courses or
    /// all in-progress courses or registered courses.
    /// 
    /// Equated Courses are also passed through repeats workflow as if those are the same course as of primary course.
    /// </summary>
    [TestClass]
    public class AcademicHistoryConstructorTests_ReplaceStatusOnAcademicCredits
    {
        private AcademicHistory history;
        private Course crs1;
        private Course crs2;
        private Course crs3;
        GradeRestriction gradeRestriction = new GradeRestriction(true);

        private List<OfferingDepartment> cDepts = new List<OfferingDepartment>();
        private List<CourseApproval> approvals = new List<CourseApproval>();




        [TestInitialize]
        public void Initialize()
        {
            cDepts.Add(new OfferingDepartment("math", 100));
            approvals.Add(new CourseApproval("approved", DateTime.Today, "ellucian", "ellucina-person", DateTime.Today));
            crs1 = new Course("CRSE1234", "MATH-300BB", "MATH-300BB", cDepts, "MATH", "300BB", "GR", new List<string>() { "300" }, 3, null, approvals);
            crs2 = new Course("CRSE4567", "MATH-400BB", "MATH-400BB", cDepts, "MATH", "400BB", "GR", new List<string>() { "400" }, 4, null, approvals);
            //this is equated course to MATH-300BB
            crs3 = new Course("CRSE2345", "MATH-300BB-EQ", "MATH-300BB-Equated", cDepts, "MATH", "300BB", "GR", new List<string>() { "300" }, 3, null, approvals);

            gradeRestriction.AddReason("Unpaid library fines.");
            gradeRestriction.AddReason("Room damage.");
        }

        [TestCleanup]
        public void Cleanup()
        {
        }

        [TestMethod]
        public void NoCoursesAreRetaken_VerifyReplaceStatus()
        {
            List<AcademicCredit> credits = GetNonRepeatedInProgressAcademicCredits();
            history = new AcademicHistory(credits, gradeRestriction, null);
            AcademicTerm term = history.AcademicTerms.Where(a => a.TermId == "2018/SP").First();
            Assert.IsNotNull(history);
            Assert.AreEqual(ReplacedStatus.NotReplaced, term.AcademicCredits[0].ReplacedStatus);
            Assert.AreEqual(ReplacementStatus.NotReplacement, term.AcademicCredits[0].ReplacementStatus);
            Assert.AreEqual(ReplacedStatus.NotReplaced, term.AcademicCredits[1].ReplacedStatus);
            Assert.AreEqual(ReplacementStatus.NotReplacement, term.AcademicCredits[1].ReplacementStatus);

        }

        [TestMethod]
        public void CoursesAreRetaken_AllAreInProgress()
        {
            List<AcademicCredit> credits = GetInProgressRepeatedAcademicCredits();
            history = new AcademicHistory(credits, gradeRestriction, null);
            AcademicTerm term = history.AcademicTerms.Where(a => a.TermId == "2018/SP").First();

            Assert.AreEqual("1234-1", term.AcademicCredits[0].Id);//VALIDATE FOR MATH-300BB
            Assert.AreEqual(ReplacedStatus.ReplaceInProgress, term.AcademicCredits[0].ReplacedStatus);
            Assert.AreEqual(ReplacementStatus.NotReplacement, term.AcademicCredits[0].ReplacementStatus);

            Assert.AreEqual("4567", term.AcademicCredits[1].Id);//validate for MATH-400BB
            Assert.AreEqual(ReplacedStatus.NotReplaced, term.AcademicCredits[1].ReplacedStatus);
            Assert.AreEqual(ReplacementStatus.NotReplacement, term.AcademicCredits[1].ReplacementStatus);

            AcademicTerm term2 = history.AcademicTerms.Where(a => a.TermId == "2018/S1").First();

            Assert.AreEqual("1234-2", term2.AcademicCredits[0].Id);//VALIDATE FOR MATH-300BB
            Assert.AreEqual(ReplacedStatus.ReplaceInProgress, term2.AcademicCredits[0].ReplacedStatus);
            Assert.AreEqual(ReplacementStatus.NotReplacement, term2.AcademicCredits[0].ReplacementStatus);

            AcademicTerm term3 = history.AcademicTerms.Where(a => a.TermId == "2018/FA").First();

            Assert.AreEqual("1234-3", term3.AcademicCredits[0].Id);//VALIDATE FOR MATH-300BB
            Assert.AreEqual(ReplacedStatus.NotReplaced, term3.AcademicCredits[0].ReplacedStatus);
            Assert.AreEqual(ReplacementStatus.PossibleReplacement, term3.AcademicCredits[0].ReplacementStatus);

        }

        [TestMethod]
        public void CoursesAreRetaken_AllAreCompleted()
        {
            List<AcademicCredit> credits = GetCompletedRepeatedAcademicCredits();
            history = new AcademicHistory(credits, gradeRestriction, null);
            AcademicTerm term = history.AcademicTerms.Where(a => a.TermId == "2018/SP").First();

            Assert.AreEqual("1234-1", term.AcademicCredits[0].Id);//VALIDATE FOR MATH-300BB
            Assert.AreEqual(ReplacedStatus.Replaced, term.AcademicCredits[0].ReplacedStatus);
            Assert.AreEqual(ReplacementStatus.NotReplacement, term.AcademicCredits[0].ReplacementStatus);

            Assert.AreEqual("4567", term.AcademicCredits[1].Id);//validate for MATH-400BB
            Assert.AreEqual(ReplacedStatus.NotReplaced, term.AcademicCredits[1].ReplacedStatus);
            Assert.AreEqual(ReplacementStatus.NotReplacement, term.AcademicCredits[1].ReplacementStatus);

            AcademicTerm term2 = history.AcademicTerms.Where(a => a.TermId == "2018/S1").First();

            Assert.AreEqual("1234-2", term2.AcademicCredits[0].Id);//VALIDATE FOR MATH-300BB
            Assert.AreEqual(ReplacedStatus.Replaced, term2.AcademicCredits[0].ReplacedStatus);
            Assert.AreEqual(ReplacementStatus.NotReplacement, term2.AcademicCredits[0].ReplacementStatus);

            AcademicTerm term3 = history.AcademicTerms.Where(a => a.TermId == "2018/FA").First();

            Assert.AreEqual("1234-3", term3.AcademicCredits[0].Id);//VALIDATE FOR MATH-300BB
            Assert.AreEqual(ReplacedStatus.NotReplaced, term3.AcademicCredits[0].ReplacedStatus);
            Assert.AreEqual(ReplacementStatus.Replacement, term3.AcademicCredits[0].ReplacementStatus);

        }

        [TestMethod]
        public void CoursesAreRetaken_FewAreCompleted()
        {
            List<AcademicCredit> credits = GetFewCompletedRepeatedAcademicCredits();
            history = new AcademicHistory(credits, gradeRestriction, null);
            AcademicTerm term = history.AcademicTerms.Where(a => a.TermId == "2018/SP").First();

            Assert.AreEqual("1234-1", term.AcademicCredits[0].Id);//VALIDATE FOR MATH-300BB
            Assert.AreEqual(ReplacedStatus.Replaced, term.AcademicCredits[0].ReplacedStatus);
            Assert.AreEqual(ReplacementStatus.NotReplacement, term.AcademicCredits[0].ReplacementStatus);

            Assert.AreEqual("4567", term.AcademicCredits[1].Id);//validate for MATH-400BB
            Assert.AreEqual(ReplacedStatus.NotReplaced, term.AcademicCredits[1].ReplacedStatus);
            Assert.AreEqual(ReplacementStatus.NotReplacement, term.AcademicCredits[1].ReplacementStatus);

            AcademicTerm term2 = history.AcademicTerms.Where(a => a.TermId == "2018/S1").First();

            Assert.AreEqual("1234-2", term2.AcademicCredits[0].Id);//VALIDATE FOR MATH-300BB
            Assert.AreEqual(ReplacedStatus.ReplaceInProgress, term2.AcademicCredits[0].ReplacedStatus);
            Assert.AreEqual(ReplacementStatus.NotReplacement, term2.AcademicCredits[0].ReplacementStatus);

            AcademicTerm term3 = history.AcademicTerms.Where(a => a.TermId == "2018/FA").First();

            Assert.AreEqual("1234-3", term3.AcademicCredits[0].Id);//VALIDATE FOR MATH-300BB
            Assert.AreEqual(ReplacedStatus.NotReplaced, term3.AcademicCredits[0].ReplacedStatus);
            Assert.AreEqual(ReplacementStatus.PossibleReplacement, term3.AcademicCredits[0].ReplacementStatus);

        }

        [TestMethod]
        public void MultipleCoursesAreRetaken_FewAreCompleted()
        {
            List<AcademicCredit> credits = GetMultipleFewCompletedRepeatedAcademicCredits();
            history = new AcademicHistory(credits, gradeRestriction, null);
            AcademicTerm term = history.AcademicTerms.Where(a => a.TermId == "2018/SP").First();

            Assert.AreEqual("1234-1", term.AcademicCredits[0].Id);//VALIDATE FOR MATH-300BB
            Assert.AreEqual(ReplacedStatus.Replaced, term.AcademicCredits[0].ReplacedStatus);
            Assert.AreEqual(ReplacementStatus.NotReplacement, term.AcademicCredits[0].ReplacementStatus);

            Assert.AreEqual("4567-1", term.AcademicCredits[1].Id);//validate for MATH-400BB
            Assert.AreEqual(ReplacedStatus.ReplaceInProgress, term.AcademicCredits[1].ReplacedStatus);
            Assert.AreEqual(ReplacementStatus.NotReplacement, term.AcademicCredits[1].ReplacementStatus);

            AcademicTerm term2 = history.AcademicTerms.Where(a => a.TermId == "2018/S1").First();

            Assert.AreEqual("1234-2", term2.AcademicCredits[0].Id);//VALIDATE FOR MATH-300BB
            Assert.AreEqual(ReplacedStatus.ReplaceInProgress, term2.AcademicCredits[0].ReplacedStatus);
            Assert.AreEqual(ReplacementStatus.NotReplacement, term2.AcademicCredits[0].ReplacementStatus);

            Assert.AreEqual("4567-2", term2.AcademicCredits[1].Id);//validate for MATH-400BB
            Assert.AreEqual(ReplacedStatus.NotReplaced, term2.AcademicCredits[1].ReplacedStatus);
            Assert.AreEqual(ReplacementStatus.PossibleReplacement, term2.AcademicCredits[1].ReplacementStatus);

            AcademicTerm term3 = history.AcademicTerms.Where(a => a.TermId == "2018/FA").First();

            Assert.AreEqual("1234-3", term3.AcademicCredits[0].Id);//VALIDATE FOR MATH-300BB
            Assert.AreEqual(ReplacedStatus.NotReplaced, term3.AcademicCredits[0].ReplacedStatus);
            Assert.AreEqual(ReplacementStatus.PossibleReplacement, term3.AcademicCredits[0].ReplacementStatus);

        }

        [TestMethod]
        public void CoursesAreRetaken_EquatedCourses_AllAreInProgress()
        {
            List<AcademicCredit> credits = GetInProgressRepeatedEquatedAcademicCredits();
            history = new AcademicHistory(credits, gradeRestriction, null);
            AcademicTerm term = history.AcademicTerms.Where(a => a.TermId == "2018/SP").First();

            Assert.AreEqual("1234-1", term.AcademicCredits[0].Id);//VALIDATE FOR MATH-300BB
            Assert.AreEqual(ReplacedStatus.ReplaceInProgress, term.AcademicCredits[0].ReplacedStatus);
            Assert.AreEqual(ReplacementStatus.NotReplacement, term.AcademicCredits[0].ReplacementStatus);

            Assert.AreEqual("2345-1", term.AcademicCredits[1].Id);//validate for MATH-400BB
            Assert.AreEqual(ReplacedStatus.ReplaceInProgress, term.AcademicCredits[1].ReplacedStatus);
            Assert.AreEqual(ReplacementStatus.NotReplacement, term.AcademicCredits[1].ReplacementStatus);

            AcademicTerm term2 = history.AcademicTerms.Where(a => a.TermId == "2018/S1").First();

            Assert.AreEqual("1234-2", term2.AcademicCredits[0].Id);//VALIDATE FOR MATH-300BB
            Assert.AreEqual(ReplacedStatus.ReplaceInProgress, term2.AcademicCredits[0].ReplacedStatus);
            Assert.AreEqual(ReplacementStatus.NotReplacement, term2.AcademicCredits[0].ReplacementStatus);

            AcademicTerm term3 = history.AcademicTerms.Where(a => a.TermId == "2018/FA").First();

            Assert.AreEqual("1234-3", term3.AcademicCredits[0].Id);//VALIDATE FOR MATH-300BB
            Assert.AreEqual(ReplacedStatus.NotReplaced, term3.AcademicCredits[0].ReplacedStatus);
            Assert.AreEqual(ReplacementStatus.PossibleReplacement, term3.AcademicCredits[0].ReplacementStatus);

        }

        [TestMethod]
        public void CoursesAreRetaken_AllAreCompleted_WithFewTimesFGrade_NoneMarkedAsReplacedByColleague()
        {
            List<AcademicCredit> credits = GetCompletedCreditsWithFGradesNOnMarkedAsReplaced();
            history = new AcademicHistory(credits, gradeRestriction, null);
            AcademicTerm term = history.AcademicTerms.Where(a => a.TermId == "2018/SP").First();

            Assert.AreEqual("1234-1", term.AcademicCredits[0].Id);//VALIDATE FOR MATH-300BB
            Assert.AreEqual(ReplacedStatus.NotReplaced, term.AcademicCredits[0].ReplacedStatus);
            Assert.AreEqual(ReplacementStatus.NotReplacement, term.AcademicCredits[0].ReplacementStatus);

            Assert.AreEqual("4567", term.AcademicCredits[1].Id);//validate for MATH-400BB
            Assert.AreEqual(ReplacedStatus.NotReplaced, term.AcademicCredits[1].ReplacedStatus);
            Assert.AreEqual(ReplacementStatus.NotReplacement, term.AcademicCredits[1].ReplacementStatus);

            AcademicTerm term2 = history.AcademicTerms.Where(a => a.TermId == "2018/S1").First();

            Assert.AreEqual("1234-2", term2.AcademicCredits[0].Id);//VALIDATE FOR MATH-300BB
            Assert.AreEqual(ReplacedStatus.NotReplaced, term2.AcademicCredits[0].ReplacedStatus);
            Assert.AreEqual(ReplacementStatus.NotReplacement, term2.AcademicCredits[0].ReplacementStatus);

            AcademicTerm term3 = history.AcademicTerms.Where(a => a.TermId == "2018/FA").First();

            Assert.AreEqual("1234-3", term3.AcademicCredits[0].Id);//VALIDATE FOR MATH-300BB
            Assert.AreEqual(ReplacedStatus.NotReplaced, term3.AcademicCredits[0].ReplacedStatus);
            Assert.AreEqual(ReplacementStatus.NotReplacement, term3.AcademicCredits[0].ReplacementStatus);
        }

        [TestMethod]
        public void CoursesAreRetaken_AllAreCompleted_WithGoodGrades_AllCounted_NoneMarkedAsReplacedByColleague()
        {
            List<AcademicCredit> credits = GetCompletedCreditsWithGoodGradesNOnMarkedAsReplaced();
            history = new AcademicHistory(credits, gradeRestriction, null);
            AcademicTerm term = history.AcademicTerms.Where(a => a.TermId == "2018/SP").First();

            Assert.AreEqual("1234-1", term.AcademicCredits[0].Id);//VALIDATE FOR MATH-300BB
            Assert.AreEqual(ReplacedStatus.NotReplaced, term.AcademicCredits[0].ReplacedStatus);
            Assert.AreEqual(ReplacementStatus.NotReplacement, term.AcademicCredits[0].ReplacementStatus);

            Assert.AreEqual("4567", term.AcademicCredits[1].Id);//validate for MATH-400BB
            Assert.AreEqual(ReplacedStatus.NotReplaced, term.AcademicCredits[1].ReplacedStatus);
            Assert.AreEqual(ReplacementStatus.NotReplacement, term.AcademicCredits[1].ReplacementStatus);

            AcademicTerm term2 = history.AcademicTerms.Where(a => a.TermId == "2018/S1").First();

            Assert.AreEqual("1234-2", term2.AcademicCredits[0].Id);//VALIDATE FOR MATH-300BB
            Assert.AreEqual(ReplacedStatus.NotReplaced, term2.AcademicCredits[0].ReplacedStatus);
            Assert.AreEqual(ReplacementStatus.NotReplacement, term2.AcademicCredits[0].ReplacementStatus);

            AcademicTerm term3 = history.AcademicTerms.Where(a => a.TermId == "2018/FA").First();

            Assert.AreEqual("1234-3", term3.AcademicCredits[0].Id);//VALIDATE FOR MATH-300BB
            Assert.AreEqual(ReplacedStatus.NotReplaced, term3.AcademicCredits[0].ReplacedStatus);
            Assert.AreEqual(ReplacementStatus.NotReplacement, term3.AcademicCredits[0].ReplacementStatus);
        }
        [TestMethod]
        public void CoursesAreRetaken_AllAreCompleted_WithGoodAndFGrades_FewMarkedAsReplacedByColleague()
        {
            List<AcademicCredit> credits = GetCompletedCreditsWithGoodAndFGradesFewMarkedAsReplaced();
            history = new AcademicHistory(credits, gradeRestriction, null);
            AcademicTerm term = history.AcademicTerms.Where(a => a.TermId == "2018/SP").First();

            Assert.AreEqual("1234-1", term.AcademicCredits[0].Id);//VALIDATE FOR MATH-300BB
            Assert.AreEqual(ReplacedStatus.NotReplaced, term.AcademicCredits[0].ReplacedStatus);
            Assert.AreEqual(ReplacementStatus.Replacement, term.AcademicCredits[0].ReplacementStatus);

            AcademicTerm term2 = history.AcademicTerms.Where(a => a.TermId == "2018/S1").First();

            Assert.AreEqual("1234-2", term2.AcademicCredits[0].Id);//VALIDATE FOR MATH-300BB
            Assert.AreEqual(ReplacedStatus.Replaced, term2.AcademicCredits[0].ReplacedStatus);
            Assert.AreEqual(ReplacementStatus.NotReplacement, term2.AcademicCredits[0].ReplacementStatus);

            AcademicTerm term3 = history.AcademicTerms.Where(a => a.TermId == "2018/FA").First();

            Assert.AreEqual("1234-3", term3.AcademicCredits[0].Id);//VALIDATE FOR MATH-300BB
            Assert.AreEqual(ReplacedStatus.NotReplaced, term3.AcademicCredits[0].ReplacedStatus);
            Assert.AreEqual(ReplacementStatus.Replacement, term3.AcademicCredits[0].ReplacementStatus);

        }

        [TestMethod]
        public void CoursesAreRetaken_FewCompleted_FewInProgress_WithGoodAndFGrades_FewMarkedAsReplacedByColleague()
        {
            List<AcademicCredit> credits = GetCompletedInprogressCreditsWithGoodAndFGradesFewMarkedAsReplaced();
            history = new AcademicHistory(credits, gradeRestriction, null);
            AcademicTerm term = history.AcademicTerms.Where(a => a.TermId == "2018/SP").First();

            Assert.AreEqual("1234-1", term.AcademicCredits[0].Id);//VALIDATE FOR MATH-300BB
            Assert.AreEqual(ReplacedStatus.ReplaceInProgress, term.AcademicCredits[0].ReplacedStatus);
            Assert.AreEqual(ReplacementStatus.NotReplacement, term.AcademicCredits[0].ReplacementStatus);

            AcademicTerm term2 = history.AcademicTerms.Where(a => a.TermId == "2018/S1").First();

            Assert.AreEqual("1234-2", term2.AcademicCredits[0].Id);//VALIDATE FOR MATH-300BB
            Assert.AreEqual(ReplacedStatus.ReplaceInProgress, term2.AcademicCredits[0].ReplacedStatus);
            Assert.AreEqual(ReplacementStatus.NotReplacement, term2.AcademicCredits[0].ReplacementStatus);

            AcademicTerm term3 = history.AcademicTerms.Where(a => a.TermId == "2018/FA").First();

            Assert.AreEqual("1234-3", term3.AcademicCredits[0].Id);//VALIDATE FOR MATH-300BB
            Assert.AreEqual(ReplacedStatus.Replaced, term3.AcademicCredits[0].ReplacedStatus);
            Assert.AreEqual(ReplacementStatus.NotReplacement, term3.AcademicCredits[0].ReplacementStatus);

                 AcademicTerm term4 = history.AcademicTerms.Where(a => a.TermId == "2018/WI").First();

            Assert.AreEqual("1234-4", term4.AcademicCredits[0].Id);//VALIDATE FOR MATH-300BB
            Assert.AreEqual(ReplacedStatus.NotReplaced, term4.AcademicCredits[0].ReplacedStatus);
            Assert.AreEqual(ReplacementStatus.PossibleReplacement, term4.AcademicCredits[0].ReplacementStatus);
        }

        [TestMethod]
        public void CoursesAreRetaken_EquatedCourses_FewGradedFewInProgress_GradedAreReplaced()
        {
            List<AcademicCredit> credits = new List<AcademicCredit>();

            crs1.AddEquatedCourseId("CRSE2345");
            crs3.AddEquatedCourseId("CRSE1234");
            //MATH-300BB is repeated thrice
            AcademicCredit ac1 = new AcademicCredit("1234-1", crs1, "01"); //FOR MATH-300BB 2018/SP
            ac1.TermCode = "2018/SP";
            ac1.RepeatAcademicCreditIds = new List<string>() { "1234-1", "1234-2", "1234-3" , "2345-1" };
            ac1.CanBeReplaced = true;
            ac1.StartDate = new DateTime(2018, 02, 01);
            ac1.CompletedCredit = 0m;
            ac1.VerifiedGrade = new Grade("F", "F grade", "GR");
            ac1.ReplacedStatus = ReplacedStatus.Replaced;

            AcademicCredit ac2 = new AcademicCredit("1234-2", crs1, "01"); //FOR MATH-300BB 2018/S1
            ac2.TermCode = "2018/S1";
            ac2.CanBeReplaced = true;
            ac2.RepeatAcademicCreditIds = new List<string>() { "1234-1", "1234-2", "1234-3", "2345-1" };
            ac2.StartDate = new DateTime(2018, 05, 01);
            ac2.CompletedCredit = 2m;
            ac2.VerifiedGrade = new Grade("B", "B grade", "GR");
            ac2.ReplacedStatus = ReplacedStatus.NotReplaced;

            AcademicCredit ac3 = new AcademicCredit("1234-3", crs1, "01"); //FOR MATH-300BB 2018/FA
            ac3.TermCode = "2018/FA";
            ac3.CanBeReplaced = true;
            ac3.RepeatAcademicCreditIds = new List<string>() { "1234-1", "1234-2", "1234-3", "2345-1" };
            ac3.StartDate = new DateTime(2018, 09, 01);

            //credit from equated course
            AcademicCredit ac1Equated = new AcademicCredit("2345-1", crs3, "01"); //FOR MATH-300BB-EQ 2018/SP
            ac1Equated.TermCode = "2018/SP";
            ac1Equated.CanBeReplaced = true;
            ac1Equated.StartDate = new DateTime(2018, 02, 01);
            ac1Equated.CompletedCredit = 2m;
            ac1Equated.VerifiedGrade = new Grade("C", "C grade", "GR");
            ac1Equated.ReplacedStatus = ReplacedStatus.Replaced;

            credits.Add(ac1);
            credits.Add(ac2);
            credits.Add(ac3);
            credits.Add(ac1Equated);

            history = new AcademicHistory(credits, gradeRestriction, null);
            AcademicTerm term = history.AcademicTerms.Where(a => a.TermId == "2018/SP").First();

            Assert.AreEqual("1234-1", term.AcademicCredits[0].Id);//VALIDATE FOR MATH-300BB F graded that is replaced
            Assert.AreEqual(ReplacedStatus.Replaced, term.AcademicCredits[0].ReplacedStatus);
            Assert.AreEqual(ReplacementStatus.NotReplacement, term.AcademicCredits[0].ReplacementStatus);


            Assert.AreEqual("2345-1", term.AcademicCredits[1].Id);//VALIDATE FOR MATH-300BB -equated course with C graded that is replaced by next B grade
            Assert.AreEqual(ReplacedStatus.Replaced, term.AcademicCredits[1].ReplacedStatus);
            Assert.AreEqual(ReplacementStatus.NotReplacement, term.AcademicCredits[1].ReplacementStatus);

            AcademicTerm term2 = history.AcademicTerms.Where(a => a.TermId == "2018/S1").First();

            Assert.AreEqual("1234-2", term2.AcademicCredits[0].Id);//VALIDATE FOR MATH-300BB- B grade here 
            Assert.AreEqual(ReplacedStatus.ReplaceInProgress, term2.AcademicCredits[0].ReplacedStatus);
            Assert.AreEqual(ReplacementStatus.NotReplacement, term2.AcademicCredits[0].ReplacementStatus);

            AcademicTerm term3 = history.AcademicTerms.Where(a => a.TermId == "2018/FA").First();

            Assert.AreEqual("1234-3", term3.AcademicCredits[0].Id);//VALIDATE FOR MATH-300BB- inprogress cours
            Assert.AreEqual(ReplacedStatus.NotReplaced, term3.AcademicCredits[0].ReplacedStatus);
            Assert.AreEqual(ReplacementStatus.PossibleReplacement, term3.AcademicCredits[0].ReplacementStatus);

           

        }

        //This is to test heavy load courses that are repeated. Just to be sure all courses are processed for repeats
        [TestMethod]
        public void ProcessLotsOfInProgressCourses()
        {
            List<AcademicCredit> credits = new List<AcademicCredit>();


            List<Course> courses = new List<Course>() { crs1, crs2, crs3 };
            for (int i = 0; i < 30; i++)
            {
                string id = "CRSE1234" + i;
                Course crs = new Course(id, "MATH-300BB", "MATH-300BB", cDepts, "MATH", "300BB", "GR", new List<string>() { "300" }, 3, null, approvals);
                courses.Add(crs);

            }
            //took 30 courses and each course was repeated 30 times (not a reallife scenario but just testing with heavy load)

            for (int i = 0; i < 30; i++)
            {
                Course crs = courses[i];
                List<string> previousRepeatedCredits = new List<string>();
                for (int k = 0; k < 30; k++)
                {
                    string id = "12345" + i + k;
                    AcademicCredit ac = new AcademicCredit(id, crs, "01"); //FOR MATH-300BB 2018/WI
                    ac.TermCode = "2018/WI";
                    ac.CanBeReplaced = true;
                    ac.RepeatAcademicCreditIds = new List<string>();
                    ac.RepeatAcademicCreditIds.AddRange(previousRepeatedCredits);
                    ac.RepeatAcademicCreditIds.Add(id);
                    previousRepeatedCredits.Add(id);
                    credits.Add(ac);

                }
            }

            history = new AcademicHistory(credits, gradeRestriction, null);
            Assert.IsNotNull(history);
            Assert.AreEqual(1, history.AcademicTerms.Count);
            Assert.AreEqual(900, history.AcademicTerms[0].AcademicCredits.Count);
        }

        [TestMethod]
        public void CoursesAreRetaken_Combination_Of_droppedcourses_droppedWithGradesAndReplaced_FewFs_FewCompleted_FewInprogress()
        {


        }



        private List<AcademicCredit> GetNonRepeatedInProgressAcademicCredits()
        {
            List<AcademicCredit> credits = new List<AcademicCredit>();
            AcademicCredit ac1 = new AcademicCredit("1234", crs1, "01"); //FOR MATH-300BB 2018/SP
            ac1.TermCode = "2018/SP";
            AcademicCredit ac2 = new AcademicCredit("4567", crs2, "01");//FOR MATH-400BB 2018/SP
            ac2.TermCode = "2018/SP";

            credits.Add(ac1);
            credits.Add(ac2);
            return credits;

        }

        private List<AcademicCredit> GetInProgressRepeatedAcademicCredits()
        {
            List<AcademicCredit> credits = new List<AcademicCredit>();

            //MATH-300BB is repeated thrice
            AcademicCredit ac1 = new AcademicCredit("1234-1", crs1, "01"); //FOR MATH-300BB 2018/SP
            ac1.TermCode = "2018/SP";
            ac1.RepeatAcademicCreditIds = new List<string>() { "1234-1", "1234-2", "1234-3" };
            ac1.CanBeReplaced = true;

            AcademicCredit ac2 = new AcademicCredit("1234-2", crs1, "01"); //FOR MATH-300BB 2018/S1
            ac2.TermCode = "2018/S1";
            ac2.CanBeReplaced = true;
            ac2.RepeatAcademicCreditIds = new List<string>() { "1234-1", "1234-2", "1234-3" };

            AcademicCredit ac3 = new AcademicCredit("1234-3", crs1, "01"); //FOR MATH-300BB 2018/FA
            ac3.TermCode = "2018/FA";
            ac3.CanBeReplaced = true;
            ac3.RepeatAcademicCreditIds = new List<string>() { "1234-1", "1234-2", "1234-3" };

            AcademicCredit ac4 = new AcademicCredit("4567", crs2, "01");//FOR MATH-400BB 2018/SP
            ac4.TermCode = "2018/SP";

            credits.Add(ac1);
            credits.Add(ac2);
            credits.Add(ac3);
            credits.Add(ac4);
            return credits;

        }

        private List<AcademicCredit> GetCompletedRepeatedAcademicCredits()
        {
            List<AcademicCredit> credits = new List<AcademicCredit>();

            //MATH-300BB is repeated thrice
            AcademicCredit ac1 = new AcademicCredit("1234-1", crs1, "01"); //FOR MATH-300BB 2018/SP
            ac1.TermCode = "2018/SP";
            ac1.RepeatAcademicCreditIds = new List<string>() { "1234-1", "1234-2", "1234-3" };
            ac1.CanBeReplaced = true;
            ac1.CompletedCredit = 3m;
            ac1.VerifiedGrade = new Grade("B", "B grade", "GR");
            ac1.ReplacedStatus = ReplacedStatus.Replaced;

            AcademicCredit ac2 = new AcademicCredit("1234-2", crs1, "01"); //FOR MATH-300BB 2018/S1
            ac2.TermCode = "2018/S1";
            ac2.CanBeReplaced = true;
            ac2.RepeatAcademicCreditIds = new List<string>() { "1234-1", "1234-2", "1234-3" };
            ac2.CompletedCredit = 3m;
            ac2.VerifiedGrade = new Grade("A", "A grade", "GR");
            ac2.ReplacedStatus = ReplacedStatus.Replaced;

            AcademicCredit ac3 = new AcademicCredit("1234-3", crs1, "01"); //FOR MATH-300BB 2018/FA
            ac3.TermCode = "2018/FA";
            ac3.CanBeReplaced = true;
            ac3.RepeatAcademicCreditIds = new List<string>() { "1234-1", "1234-2", "1234-3" };
            ac3.CompletedCredit = 3m;
            ac3.VerifiedGrade = new Grade("C", "C grade", "GR");

            AcademicCredit ac4 = new AcademicCredit("4567", crs2, "01");//FOR MATH-400BB 2018/SP
            ac4.TermCode = "2018/SP";

            credits.Add(ac1);
            credits.Add(ac2);
            credits.Add(ac3);
            credits.Add(ac4);
            return credits;

        }

        private List<AcademicCredit> GetFewCompletedRepeatedAcademicCredits()
        {
            List<AcademicCredit> credits = new List<AcademicCredit>();

            //MATH-300BB is repeated thrice
            AcademicCredit ac1 = new AcademicCredit("1234-1", crs1, "01"); //FOR MATH-300BB 2018/SP
            ac1.TermCode = "2018/SP";
            ac1.RepeatAcademicCreditIds = new List<string>() { "1234-1", "1234-2", "1234-3" };
            ac1.CanBeReplaced = true;
            ac1.CompletedCredit = 3m;
            ac1.VerifiedGrade = new Grade("B", "B grade", "GR");
            ac1.ReplacedStatus = ReplacedStatus.Replaced;

            AcademicCredit ac2 = new AcademicCredit("1234-2", crs1, "01"); //FOR MATH-300BB 2018/S1
            ac2.TermCode = "2018/S1";
            ac2.CanBeReplaced = true;
            ac2.RepeatAcademicCreditIds = new List<string>() { "1234-1", "1234-2", "1234-3" };
            ac2.CompletedCredit = 3m;
            ac2.VerifiedGrade = new Grade("A", "A grade", "GR");


            AcademicCredit ac3 = new AcademicCredit("1234-3", crs1, "01"); //FOR MATH-300BB 2018/FA
            ac3.TermCode = "2018/FA";
            ac3.CanBeReplaced = true;
            ac3.RepeatAcademicCreditIds = new List<string>() { "1234-1", "1234-2", "1234-3" };


            AcademicCredit ac4 = new AcademicCredit("4567", crs2, "01");//FOR MATH-400BB 2018/SP
            ac4.TermCode = "2018/SP";

            credits.Add(ac1);
            credits.Add(ac2);
            credits.Add(ac3);
            credits.Add(ac4);
            return credits;

        }

        private List<AcademicCredit> GetMultipleFewCompletedRepeatedAcademicCredits()
        {
            List<AcademicCredit> credits = new List<AcademicCredit>();

            //MATH-300BB is repeated thrice
            AcademicCredit ac1 = new AcademicCredit("1234-1", crs1, "01"); //FOR MATH-300BB 2018/SP
            ac1.TermCode = "2018/SP";
            ac1.RepeatAcademicCreditIds = new List<string>() { "1234-1", "1234-2", "1234-3" };
            ac1.CanBeReplaced = true;
            ac1.CompletedCredit = 3m;
            ac1.VerifiedGrade = new Grade("B", "B grade", "GR");
            ac1.ReplacedStatus = ReplacedStatus.Replaced;

            AcademicCredit ac2 = new AcademicCredit("1234-2", crs1, "01"); //FOR MATH-300BB 2018/S1
            ac2.TermCode = "2018/S1";
            ac2.CanBeReplaced = true;
            ac2.RepeatAcademicCreditIds = new List<string>() { "1234-1", "1234-2", "1234-3" };
            ac2.CompletedCredit = 3m;
            ac2.VerifiedGrade = new Grade("A", "A grade", "GR");


            AcademicCredit ac3 = new AcademicCredit("1234-3", crs1, "01"); //FOR MATH-300BB 2018/FA
            ac3.TermCode = "2018/FA";
            ac3.CanBeReplaced = true;
            ac3.RepeatAcademicCreditIds = new List<string>() { "1234-1", "1234-2", "1234-3" };


            AcademicCredit ac4 = new AcademicCredit("4567-1", crs2, "01");//FOR MATH-400BB 2018/SP
            ac4.TermCode = "2018/SP";
            ac4.RepeatAcademicCreditIds = new List<string>() { "4567-1", "4567-2" };
            ac4.CanBeReplaced = true;

            AcademicCredit ac5 = new AcademicCredit("4567-2", crs2, "01");//FOR MATH-400BB 2018/S1
            ac5.TermCode = "2018/S1";
            ac5.RepeatAcademicCreditIds = new List<string>() { "4567-1", "4567-2" };
            ac5.CanBeReplaced = true;

            credits.Add(ac1);
            credits.Add(ac2);
            credits.Add(ac3);
            credits.Add(ac4);
            credits.Add(ac5);
            return credits;

        }
        private List<AcademicCredit> GetInProgressRepeatedEquatedAcademicCredits()
        {
            List<AcademicCredit> credits = new List<AcademicCredit>();

            crs1.AddEquatedCourseId("CRSE2345");
            crs3.AddEquatedCourseId("CRSE1234");
            //MATH-300BB is repeated thrice
            AcademicCredit ac1 = new AcademicCredit("1234-1", crs1, "01"); //FOR MATH-300BB 2018/SP
            ac1.TermCode = "2018/SP";
            ac1.RepeatAcademicCreditIds = new List<string>() { "1234-1", "1234-2", "1234-3" };
            ac1.CanBeReplaced = true;
            ac1.StartDate = new DateTime(2018, 02, 01);

            AcademicCredit ac2 = new AcademicCredit("1234-2", crs1, "01"); //FOR MATH-300BB 2018/S1
            ac2.TermCode = "2018/S1";
            ac2.CanBeReplaced = true;
            ac2.RepeatAcademicCreditIds = new List<string>() { "1234-1", "1234-2", "1234-3" };
            ac2.StartDate = new DateTime(2018, 05, 01);

            AcademicCredit ac3 = new AcademicCredit("1234-3", crs1, "01"); //FOR MATH-300BB 2018/FA
            ac3.TermCode = "2018/FA";
            ac3.CanBeReplaced = true;
            ac3.RepeatAcademicCreditIds = new List<string>() { "1234-1", "1234-2", "1234-3" };
            ac3.StartDate = new DateTime(2018, 09, 01);

            //credit from equated course
            AcademicCredit ac1Equated = new AcademicCredit("2345-1", crs3, "01"); //FOR MATH-300BB-EQ 2018/SP
            ac1Equated.TermCode = "2018/SP";
            ac1Equated.CanBeReplaced = true;
            ac1Equated.StartDate = new DateTime(2018, 02, 01);

            credits.Add(ac1);
            credits.Add(ac2);
            credits.Add(ac3);
            credits.Add(ac1Equated);
            return credits;

        }

        private List<AcademicCredit> GetCompletedCreditsWithFGradesNOnMarkedAsReplaced()
        {
            List<AcademicCredit> credits = new List<AcademicCredit>();

            //MATH-300BB is repeated thrice and student failed it twice, 3rd time got C grade.
            //When failed with F grade it wasn't marked as Replaced by Colleague because it didn't have Repeat Value in grade scheme
            AcademicCredit ac1 = new AcademicCredit("1234-1", crs1, "01"); //FOR MATH-300BB 2018/SP
            ac1.TermCode = "2018/SP";
            ac1.RepeatAcademicCreditIds = new List<string>() { "1234-1", "1234-2", "1234-3" };
            ac1.CanBeReplaced = true;
            ac1.CompletedCredit = 0m;
            ac1.VerifiedGrade = new Grade("F", "F grade", "GR");
            ac1.ReplacedStatus = ReplacedStatus.NotReplaced;

            AcademicCredit ac2 = new AcademicCredit("1234-2", crs1, "01"); //FOR MATH-300BB 2018/S1
            ac2.TermCode = "2018/S1";
            ac2.CanBeReplaced = true;
            ac2.RepeatAcademicCreditIds = new List<string>() { "1234-1", "1234-2", "1234-3" };
            ac2.CompletedCredit = 0m;
            ac2.VerifiedGrade = new Grade("F", "F grade", "GR");
            ac2.ReplacedStatus = ReplacedStatus.NotReplaced;

            AcademicCredit ac3 = new AcademicCredit("1234-3", crs1, "01"); //FOR MATH-300BB 2018/FA
            ac3.TermCode = "2018/FA";
            ac3.CanBeReplaced = true;
            ac3.RepeatAcademicCreditIds = new List<string>() { "1234-1", "1234-2", "1234-3" };
            ac3.CompletedCredit = 3m;
            ac3.VerifiedGrade = new Grade("C", "C grade", "GR");

            AcademicCredit ac4 = new AcademicCredit("4567", crs2, "01");//FOR MATH-400BB 2018/SP
            ac4.TermCode = "2018/SP";

            credits.Add(ac1);
            credits.Add(ac2);
            credits.Add(ac3);
            credits.Add(ac4);
            return credits;

        }

        private List<AcademicCredit> GetCompletedCreditsWithGoodGradesNOnMarkedAsReplaced()
        {
            List<AcademicCredit> credits = new List<AcademicCredit>();

            //MATH-300BB is repeated thrice and student hasn't failed even once. Colleague doesn't put Replaced flag on these completed courses because grade policy
            //is of AVG. Hence, all will be counted.
            AcademicCredit ac1 = new AcademicCredit("1234-1", crs1, "01"); //FOR MATH-300BB 2018/SP
            ac1.TermCode = "2018/SP";
            ac1.RepeatAcademicCreditIds = new List<string>() { "1234-1", "1234-2", "1234-3" };
            ac1.CanBeReplaced = true;
            ac1.CompletedCredit = 3m;
            ac1.VerifiedGrade = new Grade("A", "A grade", "GR");
            ac1.ReplacedStatus = ReplacedStatus.NotReplaced;

            AcademicCredit ac2 = new AcademicCredit("1234-2", crs1, "01"); //FOR MATH-300BB 2018/S1
            ac2.TermCode = "2018/S1";
            ac2.CanBeReplaced = true;
            ac2.RepeatAcademicCreditIds = new List<string>() { "1234-1", "1234-2", "1234-3" };
            ac2.CompletedCredit = 3m;
            ac2.VerifiedGrade = new Grade("B", "B grade", "GR");
            ac2.ReplacedStatus = ReplacedStatus.NotReplaced;

            AcademicCredit ac3 = new AcademicCredit("1234-3", crs1, "01"); //FOR MATH-300BB 2018/FA
            ac3.TermCode = "2018/FA";
            ac3.CanBeReplaced = true;
            ac3.RepeatAcademicCreditIds = new List<string>() { "1234-1", "1234-2", "1234-3" };
            ac3.CompletedCredit = 3m;
            ac3.VerifiedGrade = new Grade("C", "C grade", "GR");

            AcademicCredit ac4 = new AcademicCredit("4567", crs2, "01");//FOR MATH-400BB 2018/SP
            ac4.TermCode = "2018/SP";

            credits.Add(ac1);
            credits.Add(ac2);
            credits.Add(ac3);
            credits.Add(ac4);
            return credits;

        }

        private List<AcademicCredit> GetCompletedCreditsWithGoodAndFGradesFewMarkedAsReplaced()
        {
            List<AcademicCredit> credits = new List<AcademicCredit>();

            //MATH-300BB is repeated thrice and student failed once and 
            //then took the course again, got B grade
            //again took and got A grade
            //Colleague doesn't put Replaced flag on F graded course because it has empty 
            //repeat value but replaces B with A by putting Replaced flag on B because of BEST Grade policy.
          
            AcademicCredit ac1 = new AcademicCredit("1234-1", crs1, "01"); //FOR MATH-300BB 2018/SP
            ac1.TermCode = "2018/SP";
            ac1.RepeatAcademicCreditIds = new List<string>() { "1234-1", "1234-2", "1234-3" };
            ac1.CanBeReplaced = true;
            ac1.CompletedCredit = 0m;
            ac1.VerifiedGrade = new Grade("F", "F grade", "GR");
            ac1.ReplacedStatus = ReplacedStatus.NotReplaced;

            AcademicCredit ac2 = new AcademicCredit("1234-2", crs1, "01"); //FOR MATH-300BB 2018/S1
            ac2.TermCode = "2018/S1";
            ac2.CanBeReplaced = true;
            ac2.RepeatAcademicCreditIds = new List<string>() { "1234-1", "1234-2", "1234-3" };
            ac2.CompletedCredit = 3m;
            ac2.VerifiedGrade = new Grade("B", "B grade", "GR");
            ac2.ReplacedStatus = ReplacedStatus.Replaced;

            AcademicCredit ac3 = new AcademicCredit("1234-3", crs1, "01"); //FOR MATH-300BB 2018/FA
            ac3.TermCode = "2018/FA";
            ac3.CanBeReplaced = true;
            ac3.RepeatAcademicCreditIds = new List<string>() { "1234-1", "1234-2", "1234-3" };
            ac3.CompletedCredit = 3m;
            ac3.VerifiedGrade = new Grade("A", "A grade", "GR");

            

            credits.Add(ac1);
            credits.Add(ac2);
            credits.Add(ac3);
           
            return credits;

        }

        private List<AcademicCredit> GetCompletedInprogressCreditsWithGoodAndFGradesFewMarkedAsReplaced()
        {
            List<AcademicCredit> credits = new List<AcademicCredit>();

            //MATH-300BB is repeated four times and student failed once and 
            //then took the course again, got B grade
            //then took again, got C grade
            //again took again which is in-progress
            //Colleague doesn't put Replaced flag on F graded course because it has empty 
            //repeat value but replaces C with B by putting Replaced flag on C because of BEST Grade policy.
            //B grade has CanBeReplaced flag to allow it to replace with inprogress one

            AcademicCredit ac1 = new AcademicCredit("1234-1", crs1, "01"); //FOR MATH-300BB 2018/SP
            ac1.TermCode = "2018/SP";
            ac1.RepeatAcademicCreditIds = new List<string>() { "1234-1", "1234-2", "1234-3", "1234-4" };
            ac1.CanBeReplaced = true;
            ac1.CompletedCredit = 0m;
            ac1.VerifiedGrade = new Grade("F", "F grade", "GR");
            ac1.ReplacedStatus = ReplacedStatus.NotReplaced;

            AcademicCredit ac2 = new AcademicCredit("1234-2", crs1, "01"); //FOR MATH-300BB 2018/S1
            ac2.TermCode = "2018/S1";
            ac2.CanBeReplaced = true;
            ac2.RepeatAcademicCreditIds = new List<string>() { "1234-1", "1234-2", "1234-3", "1234-4" };
            ac2.CompletedCredit = 3m;
            ac2.VerifiedGrade = new Grade("B", "B grade", "GR");
            ac2.ReplacedStatus = ReplacedStatus.NotReplaced;

            AcademicCredit ac3 = new AcademicCredit("1234-3", crs1, "01"); //FOR MATH-300BB 2018/FA
            ac3.TermCode = "2018/FA";
            ac3.CanBeReplaced = true;
            ac3.RepeatAcademicCreditIds = new List<string>() { "1234-1", "1234-2", "1234-3", "1234-4" };
            ac3.CompletedCredit = 3m;
            ac3.VerifiedGrade = new Grade("C", "C grade", "GR");
            ac3.ReplacedStatus = ReplacedStatus.Replaced;

            AcademicCredit ac4 = new AcademicCredit("1234-4", crs1, "01"); //FOR MATH-300BB 2018/WI
            ac4.TermCode = "2018/WI";
            ac4.CanBeReplaced = true;
            ac4.RepeatAcademicCreditIds = new List<string>() { "1234-1", "1234-2", "1234-3", "1234-4" };
            ac4.CompletedCredit = 3m;
           


            credits.Add(ac1);
            credits.Add(ac2);
            credits.Add(ac3);
            credits.Add(ac4);
            return credits;

        }
    }

}
