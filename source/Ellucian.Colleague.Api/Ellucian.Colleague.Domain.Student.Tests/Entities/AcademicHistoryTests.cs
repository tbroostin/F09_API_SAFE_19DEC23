// Copyright 2014-2018 Ellucian Company L.P. and its affiliates.
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

            Assert.AreEqual(9, history.AcademicTerms.Count());
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
            Assert.AreEqual(9, history.AcademicTerms.Count());
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
    
}
