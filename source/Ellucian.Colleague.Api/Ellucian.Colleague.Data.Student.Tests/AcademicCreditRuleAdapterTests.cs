// Copyright 2012-2017 Ellucian Company L.P. and its affiliates.
using System.Linq;
using Ellucian.Colleague.Data.Base;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Tests
{
    [TestClass]
    public class AcademicCreditRuleAdapterTests
    {
        private TestAcademicCreditRepository creditRepo = new TestAcademicCreditRepository();

        [TestMethod]
        public void GreaterThanOrEqualTo()
        {
            var rd = new RuleDescriptor() { Id = "WHATEVS", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.COURSE.LEVEL", "GE", "\"200\""));
            var rule = (Rule<AcademicCredit>) new AcademicCreditRuleAdapter().Create(rd);
            Assert.IsTrue(rule.HasExpression);
            Assert.IsFalse(rule.Passes(creditRepo.Biol100));
            Assert.IsTrue(rule.Passes(creditRepo.Biol200));
            Assert.IsTrue(rule.Passes(creditRepo.Biol400));
        }

        [TestMethod]
        public void GreaterThan()
        {
            var rd = new RuleDescriptor() { Id = "WHATEVS", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.COURSE.LEVEL", "GT", "\"200\""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            Assert.IsTrue(rule.HasExpression);
            Assert.IsFalse(rule.Passes(creditRepo.Biol100));
            Assert.IsFalse(rule.Passes(creditRepo.Biol200));
            Assert.IsTrue(rule.Passes(creditRepo.Biol400));
        }

        [TestMethod]
        public void Equal()
        {
            var rd = new RuleDescriptor() { Id = "WHATEVS", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.COURSE.LEVEL", "EQ", "\"200\""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            Assert.IsTrue(rule.HasExpression);
            Assert.IsTrue(rule.Passes(creditRepo.Biol200));
            Assert.IsFalse(rule.Passes(creditRepo.Biol100));
        }

        [TestMethod]
        public void EqualMultipleCommaDelimited()
        {
            var rd = new RuleDescriptor() { Id = "WHATEVS", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.COURSE.LEVEL", "EQ", "\"200\",\"100\",\"400\""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            Assert.IsTrue(rule.HasExpression);
            Assert.IsTrue(rule.Passes(creditRepo.Biol100));
            Assert.IsTrue(rule.Passes(creditRepo.Biol200));
            Assert.IsTrue(rule.Passes(creditRepo.Biol400));
            Assert.IsFalse(rule.Passes(creditRepo.Span300));
        }

        [TestMethod]
        public void EqualMultipleCommaDelimitedAndSpaces()
        {
            var rd = new RuleDescriptor() { Id = "WHATEVS", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.COURSE.LEVEL", "EQ", "\" 200\" ,  \"100 \"   ,\"400\" "));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            Assert.IsTrue(rule.HasExpression);
            Assert.IsTrue(rule.Passes(creditRepo.Biol100));
            Assert.IsTrue(rule.Passes(creditRepo.Biol200));
            Assert.IsTrue(rule.Passes(creditRepo.Biol400));
            Assert.IsFalse(rule.Passes(creditRepo.Span300));
        }

        [TestMethod]
        public void EqualMultiple()
        {
            var rd = new RuleDescriptor() { Id = "WHATEVS", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.COURSE.LEVEL", "EQ", "\"200\"\"100\"\"400\""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            Assert.IsTrue(rule.HasExpression);
            Assert.IsTrue(rule.Passes(creditRepo.Biol100));
            Assert.IsTrue(rule.Passes(creditRepo.Biol200));
            Assert.IsTrue(rule.Passes(creditRepo.Biol400));
            Assert.IsFalse(rule.Passes(creditRepo.Span300));
        }

        [TestMethod]
        public void EqualMultipleSingleQuote()
        {
            var rd = new RuleDescriptor() { Id = "WHATEVS", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.COURSE.LEVEL", "EQ", "\'200\'\'100\'\'400\'"));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            Assert.IsTrue(rule.HasExpression);
            Assert.IsTrue(rule.Passes(creditRepo.Biol100));
            Assert.IsTrue(rule.Passes(creditRepo.Biol200));
            Assert.IsTrue(rule.Passes(creditRepo.Biol400));
            Assert.IsFalse(rule.Passes(creditRepo.Span300));
        }

        [TestMethod]
        public void LessThan()
        {
            var rd = new RuleDescriptor() { Id = "WHATEVS", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.COURSE.LEVEL", "LT", "\"200\""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            Assert.IsTrue(rule.HasExpression);
            Assert.IsTrue(rule.Passes(creditRepo.Biol100));
            Assert.IsFalse(rule.Passes(creditRepo.Biol200));
            Assert.IsFalse(rule.Passes(creditRepo.Biol400));
        }

        [TestMethod]
        public void LessThanOrEqualTo()
        {
            var rd = new RuleDescriptor() { Id = "WHATEVS", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.COURSE.LEVEL", "LE", "\"200\""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            Assert.IsTrue(rule.HasExpression);
            Assert.IsTrue(rule.Passes(creditRepo.Biol100));
            Assert.IsTrue(rule.Passes(creditRepo.Biol200));
            Assert.IsFalse(rule.Passes(creditRepo.Biol400));
        }

        [TestMethod]
        public void LikeStart()
        {
            var rd = new RuleDescriptor() { Id = "WHATEVS", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.COURSE.NAME", "LIKE", "\"...OL*100\""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            Assert.IsTrue(rule.HasExpression);
            Assert.IsTrue(rule.Passes(creditRepo.Biol100));
            Assert.IsFalse(rule.Passes(creditRepo.Hist100));
        }

        [TestMethod]
        public void LikeEnd()
        {
            var rd = new RuleDescriptor() { Id = "WHATEVS", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.COURSE.NAME", "LIKE", "\"BIOL*10...\""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            Assert.IsTrue(rule.HasExpression);
            Assert.IsTrue(rule.Passes(creditRepo.Biol100));
            Assert.IsFalse(rule.Passes(creditRepo.Biol200));
            Assert.IsFalse(rule.Passes(creditRepo.Biol400));
            Assert.IsFalse(rule.Passes(creditRepo.Hist100));
        }

        [TestMethod]
        public void LikeStartAndEnd()
        {
            var rd = new RuleDescriptor() { Id = "WHATEVS", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.COURSE.NAME", "LIKE", "\"...IOL*10...\""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            Assert.IsTrue(rule.HasExpression);
            Assert.IsTrue(rule.Passes(creditRepo.Biol100));
            Assert.IsFalse(rule.Passes(creditRepo.Biol200));
            Assert.IsFalse(rule.Passes(creditRepo.Biol400));
            Assert.IsFalse(rule.Passes(creditRepo.Hist100));
        }

        [TestMethod]
        public void LikeNoWildcard()
        {
            var rd = new RuleDescriptor() { Id = "WHATEVS", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.COURSE.NAME", "LIKE", "\"BIOL*100\""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            Assert.IsTrue(rule.HasExpression);
            Assert.IsTrue(rule.Passes(creditRepo.Biol100));
            Assert.IsFalse(rule.Passes(creditRepo.Biol200));
            Assert.IsFalse(rule.Passes(creditRepo.Biol400));
            Assert.IsFalse(rule.Passes(creditRepo.Hist100));
        }

        [TestMethod]
        public void UnlikeStart()
        {
            var rd = new RuleDescriptor() { Id = "WHATEVS", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.COURSE.NAME", "UNLIKE", "\"...OL*100\""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            Assert.IsTrue(rule.HasExpression);
            Assert.IsFalse(rule.Passes(creditRepo.Biol100));
            Assert.IsTrue(rule.Passes(creditRepo.Hist100));
        }

        [TestMethod]
        public void UnlikeEnd()
        {
            var rd = new RuleDescriptor() { Id = "WHATEVS", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.COURSE.NAME", "UNLIKE", "\"BIOL*10...\""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            Assert.IsTrue(rule.HasExpression);
            Assert.IsFalse(rule.Passes(creditRepo.Biol100));
            Assert.IsTrue(rule.Passes(creditRepo.Biol200));
            Assert.IsTrue(rule.Passes(creditRepo.Biol400));
            Assert.IsTrue(rule.Passes(creditRepo.Hist100));
        }

        [TestMethod]
        public void UnlikeStartAndEnd()
        {
            var rd = new RuleDescriptor() { Id = "WHATEVS", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.COURSE.NAME", "UNLIKE", "\"...IOL*10...\""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            Assert.IsTrue(rule.HasExpression);
            Assert.IsFalse(rule.Passes(creditRepo.Biol100));
            Assert.IsTrue(rule.Passes(creditRepo.Biol200));
            Assert.IsTrue(rule.Passes(creditRepo.Biol400));
            Assert.IsTrue(rule.Passes(creditRepo.Hist100));
        }

        [TestMethod]
        public void UnlikeNoWildcard()
        {
            var rd = new RuleDescriptor() { Id = "WHATEVS", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.COURSE.NAME", "UNLIKE", "\"BIOL*100\""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            Assert.IsTrue(rule.HasExpression);
            Assert.IsFalse(rule.Passes(creditRepo.Biol100));
            Assert.IsTrue(rule.Passes(creditRepo.Biol200));
            Assert.IsTrue(rule.Passes(creditRepo.Biol400));
            Assert.IsTrue(rule.Passes(creditRepo.Hist100));
        }

        [TestMethod]
        public void And()
        {
            var rd = new RuleDescriptor() { Id = "WHATEVS", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.COURSE.LEVEL", "EQ", "\"100\""));
            rd.Expressions.Add(new RuleExpressionDescriptor("AND", "STC.SUBJECT", "EQ", "\"BIOL\""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            Assert.IsTrue(rule.HasExpression);
            Assert.IsTrue(rule.Passes(creditRepo.Biol100));
            Assert.IsFalse(rule.Passes(creditRepo.Hist100));
            Assert.IsFalse(rule.Passes(creditRepo.Biol200));
        }

        [TestMethod]
        public void Or()
        {
            var rd = new RuleDescriptor() { Id = "WHATEVS", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.COURSE.LEVEL", "EQ", "\"100\""));
            rd.Expressions.Add(new RuleExpressionDescriptor("OR", "STC.SUBJECT", "EQ", "\"BIOL\""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            Assert.IsTrue(rule.HasExpression);
            Assert.IsTrue(rule.Passes(creditRepo.Biol100));
            Assert.IsTrue(rule.Passes(creditRepo.Hist100));
            Assert.IsTrue(rule.Passes(creditRepo.Biol200));
            Assert.IsFalse(rule.Passes(creditRepo.Hist400));
        }

        [TestMethod]
        public void OrWith()
        {
            var rd = new RuleDescriptor() { Id = "WHATEVS", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.COURSE.LEVEL", "EQ", "\"100\""));
            rd.Expressions.Add(new RuleExpressionDescriptor("ORWITH", "STC.SUBJECT", "EQ", "\"BIOL\""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            Assert.IsTrue(rule.HasExpression);
            Assert.IsTrue(rule.Passes(creditRepo.Biol100));
            Assert.IsTrue(rule.Passes(creditRepo.Hist100));
            Assert.IsTrue(rule.Passes(creditRepo.Biol200));
            Assert.IsFalse(rule.Passes(creditRepo.Hist400));
        }

        [TestMethod]
        public void OrWith2()
        {
            var rd = new RuleDescriptor() { Id = "WHATEVS", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.COURSE.LEVEL", "EQ", "\"100\""));
            rd.Expressions.Add(new RuleExpressionDescriptor("ORWITH", "STC.COURSE.LEVEL", "EQ", "\"400\""));
            rd.Expressions.Add(new RuleExpressionDescriptor("AND", "STC.SUBJECT", "EQ", "\"MATH\""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            Assert.IsTrue(rule.HasExpression);
            Assert.IsTrue(rule.Passes(creditRepo.Biol100));
            Assert.IsTrue(rule.Passes(creditRepo.Hist100));
            Assert.IsFalse(rule.Passes(creditRepo.Biol200));
            Assert.IsTrue(rule.Passes(creditRepo.Math400));
        }

        [TestMethod]
        public void OrWith3()
        {
            var rd = new RuleDescriptor() { Id = "WHATEVS", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.SUBJECT", "EQ", "\"BIOL\""));
            rd.Expressions.Add(new RuleExpressionDescriptor("AND", "STC.COURSE.LEVEL", "EQ", "\"100\""));
            rd.Expressions.Add(new RuleExpressionDescriptor("AND", "STC.TERM", "EQ", "\"2009/SP\""));
            rd.Expressions.Add(new RuleExpressionDescriptor("ORWITH", "STC.SUBJECT", "EQ", "\"MATH\""));
            rd.Expressions.Add(new RuleExpressionDescriptor("AND", "STC.COURSE.LEVEL", "EQ", "\"400\""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            Assert.IsTrue(rule.HasExpression);
            Assert.IsTrue(rule.Passes(creditRepo.Biol100));
            Assert.IsFalse(rule.Passes(creditRepo.Hist100));
            Assert.IsFalse(rule.Passes(creditRepo.Biol200));
            Assert.IsTrue(rule.Passes(creditRepo.Math400));

            var credit = creditRepo.Biol100;
            var passes = (credit.SubjectCode == "BIOL" && credit.CourseLevelCode == "100" && credit.TermCode == "2009/SP")
                || (credit.SubjectCode == "HIST" && credit.CourseLevelCode == "400");
        }

        [TestMethod]
        public void CourseNumberViaCredit()
        {
            var rd = new RuleDescriptor() { Id = "WHATEVS", PrimaryView = "STUDENT.ACAD.CRED" };
            var stcCrsNo = new RuleDataElement()
            {
                Id = "STC.CRS.NUMBER",
                ComputedFieldDefinition = "TRANS(\"COURSES\",STC.COURSE,\"CRS.NO\",\"X\")"
            };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", stcCrsNo, "EQ", "\"100\""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            Assert.IsTrue(rule.HasExpression);
            Assert.IsTrue(rule.Passes(creditRepo.Biol100));
            Assert.IsFalse(rule.Passes(creditRepo.Biol200));
        }

        [TestMethod]
        public void CourseNumberViaCreditWithNullCourse()
        {
            var rd = new RuleDescriptor() { Id = "WHATEVS", PrimaryView = "STUDENT.ACAD.CRED" };
            var stcCrsNo = new RuleDataElement()
            {
                Id = "STC.CRS.NUMBER",
                ComputedFieldDefinition = "TRANS(\"COURSES\",STC.COURSE,\"CRS.NO\",\"X\")"
            };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", stcCrsNo, "EQ", "\"100\""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            Assert.IsTrue(rule.HasExpression);
            var credit = new AcademicCredit("12345");
            Assert.IsFalse(rule.Passes(credit));
        }

        [TestMethod]
        public void CourseNameViaCredit()
        {
            var rd = new RuleDescriptor() { Id = "WHATEVS", PrimaryView = "STUDENT.ACAD.CRED" };
            var stcCrsName = new RuleDataElement()
            {
                Id = "STC.CRS.NAME",
                ComputedFieldDefinition = "TRANS(\"COURSES\",STC.COURSE,\"CRS.NAME\",\"X\")"
            };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", stcCrsName, "EQ", "\"BIOL-100\""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            Assert.IsTrue(rule.HasExpression);
            Assert.IsTrue(rule.Passes(creditRepo.Biol100));
            Assert.IsFalse(rule.Passes(creditRepo.Biol200));
        }

        [TestMethod]
        public void CourseNameViaCreditWithNullCourse()
        {
            var rd = new RuleDescriptor() { Id = "WHATEVS", PrimaryView = "STUDENT.ACAD.CRED" };
            var stcCrsName = new RuleDataElement()
            {
                Id = "STC.CRS.NAME",
                ComputedFieldDefinition = "TRANS(\"COURSES\",STC.COURSE,\"CRS.NAME\",\"X\")"
            };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", stcCrsName, "EQ", "\"BIOL-100\""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            Assert.IsTrue(rule.HasExpression);
            var credit = new AcademicCredit("12345");
            Assert.IsFalse(rule.Passes(credit));
        }

        [TestMethod]
        public void CreditType()
        {
            var rd = new RuleDescriptor() { Id = "WHATEVS", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.CRED.TYPE", "EQ", "\"IN\""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            var credit1 = new AcademicCredit("1");
            credit1.Type = Domain.Student.Entities.CreditType.Institutional;
            credit1.LocalType = "IN";
            var credit2 = new AcademicCredit("2");
            credit2.Type = Domain.Student.Entities.CreditType.Transfer;
            credit2.LocalType = "TR";
            Assert.IsTrue(rule.HasExpression);
            Assert.IsTrue(rule.Passes(credit1));
            Assert.IsFalse(rule.Passes(credit2));
        }

        [TestMethod]
        public void CreditType2()
        {
            var rd = new RuleDescriptor() { Id = "WHATEVS", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.CRED.TYPE", "EQ", "\"TR\""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            var credit1 = new AcademicCredit("1");
            credit1.Type = Domain.Student.Entities.CreditType.Institutional;
            credit1.LocalType = "IN";
            var credit2 = new AcademicCredit("2");
            credit2.Type = Domain.Student.Entities.CreditType.Transfer;
            credit2.LocalType = "TR";
            Assert.IsTrue(rule.HasExpression);
            Assert.IsFalse(rule.Passes(credit1));
            Assert.IsTrue(rule.Passes(credit2));
        }

        [TestMethod]
        public void CreditType3()
        {
            var rd = new RuleDescriptor() { Id = "WHATEVS", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.CRED.TYPE", "EQ", "\"CE\""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            var credit1 = new AcademicCredit("1");
            credit1.Type = Domain.Student.Entities.CreditType.Other;
            Assert.IsTrue(rule.HasExpression);
            Assert.IsFalse(rule.Passes(credit1));
        }

        [TestMethod]
        public void StcCourseWithNoCourseOnCredit()
        {
            var rd = new RuleDescriptor() { Id = "WHATEVS", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.COURSE", "EQ", "\"110\""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            Assert.IsTrue(rule.HasExpression);
            var credit = new AcademicCredit("1", new TestCourseRepository().Biol100, "101");
            Assert.IsTrue(rule.Passes(credit));
            credit = new AcademicCredit("1", new TestCourseRepository().Hist400, "101");
            Assert.IsFalse(rule.Passes(credit));
            credit = new AcademicCredit("1");
            Assert.IsFalse(rule.Passes(credit));
        }


        [TestMethod]
        public async Task StcGrade()
        {
            var rd = new RuleDescriptor() { Id = "STCGRDRL", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.GRADE", "EQ", "\"A\""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            Assert.IsTrue(rule.HasExpression);
            var credit = new AcademicCredit("1", new TestCourseRepository().Biol100, "101");
            credit.VerifiedGrade =(await new TestGradeRepository().GetAsync()).Where(g => g.Id == "A").First();
            Assert.IsTrue(rule.HasExpression);
            Assert.IsTrue(rule.Passes(credit));
            credit.VerifiedGrade =(await new TestGradeRepository().GetAsync()).Where(g => g.Id == "B").First();
            Assert.IsFalse(rule.Passes(credit));
        }

        [TestMethod]
        public void StcGrade_NoVerifiedGrade()
        {
            var rd = new RuleDescriptor() { Id = "STCGRDRL", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.GRADE", "EQ", "\"A\""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            var credit = new AcademicCredit("1", new TestCourseRepository().Biol100, "101");
            Assert.IsTrue(rule.HasExpression);
            Assert.IsFalse(rule.Passes(credit));
        }

        [TestMethod]
        public void StcCourseSection()
        {
            var rd = new RuleDescriptor() { Id = "STCCSRL", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.COURSE.SECTION", "EQ", "\"12345\""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            Assert.IsTrue(rule.HasExpression);
            // Passes rule for Credit with matching course section Id
            var credit = new AcademicCredit("1", new TestCourseRepository().Biol100, "101");
            credit.SectionId = "12345";
            Assert.IsTrue(rule.Passes(credit));
            // Fails rule for credit with nonmatching course section id
            var credit2 = new AcademicCredit("1", new TestCourseRepository().Hist400, "401");
            credit2.SectionId = "1234";
            Assert.IsFalse(rule.Passes(credit2));
        }

        [TestMethod]
        public void StcCourseSection_ValidLiteralAndNullCourseSectionId_FailsGracefully()
        {
            var rd = new RuleDescriptor() { Id = "STCCSRL", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.COURSE.SECTION", "EQ", "\"12345\""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            Assert.IsTrue(rule.HasExpression);
            // Passes rule for Credit with matching course section Id
            var credit = new AcademicCredit("1", new TestCourseRepository().Biol100, "101");
            credit.SectionId = null;
            Assert.IsFalse(rule.Passes(credit));
        }

        [TestMethod]
        public void StcCourseSection_EmptyCourseSectionIdLiteralAndNullSectionId_Passes()
        {
            var rd = new RuleDescriptor() { Id = "STCCSRL", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.COURSE.SECTION", "EQ", "\"\""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            Assert.IsTrue(rule.HasExpression);
            // Passes rule for Credit with matching course section Id
            var credit = new AcademicCredit("1", new TestCourseRepository().Biol100, "101");
            credit.SectionId = null;
            Assert.IsFalse(rule.Passes(credit));
        }

        [TestMethod]
        public void StcCred_Evaluations()
        {
            var rd = new RuleDescriptor() { Id = "STCCRL", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.CRED", "LT", "\"1.5\""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            var credit = new AcademicCredit("1", new TestCourseRepository().Biol100, "101");
            credit.Credit = 2m;

            Assert.IsTrue(rule.HasExpression);
            Assert.IsFalse(rule.Passes(credit));
        }

        [TestMethod]
        public void LessThanDecimal()
        {
            var rd = new RuleDescriptor() { Id = "STCCRL", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.CMPL.CRED", "LT", "\"1.5\""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            var credit = new AcademicCredit("1", new TestCourseRepository().Biol100, "101");
            credit.CompletedCredit = 2m;
            Assert.IsTrue(rule.HasExpression);
            Assert.IsFalse(rule.Passes(credit));
        }

        [TestMethod]
        public void GreaterThanOrEqualDecimal()
        {
            var rd = new RuleDescriptor() { Id = "STCCRL", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.CMPL.CRED", "GE", "\"2\""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            var credit = new AcademicCredit("1", new TestCourseRepository().Biol100, "101");
            credit.CompletedCredit = 2m;
            Assert.IsTrue(rule.HasExpression);
            Assert.IsTrue(rule.Passes(credit));
        }

        [TestMethod]
        public void GreaterThanOrEqualDecimalWithSpaces()
        {
            var rd = new RuleDescriptor() { Id = "STCCRL", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.CMPL.CRED", "GE", "\" 2 \""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            var credit = new AcademicCredit("1", new TestCourseRepository().Biol100, "101");
            credit.CompletedCredit = 2m;
            Assert.IsTrue(rule.HasExpression);
            Assert.IsTrue(rule.Passes(credit));
        }

        [TestMethod]
        public void RuleToCompareCompletedCreditsWith_NE0()
        {
            var rd = new RuleDescriptor() { Id = "STCCRL", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.CMPL.CRED", "NE", "\" 0 \""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            var credit = new AcademicCredit("1", new TestCourseRepository().Biol100, "101");
            credit.CompletedCredit = 0m;
            Assert.IsTrue(rule.HasExpression);
            Assert.IsFalse(rule.Passes(credit));
        }

        [TestMethod]
        public void RuleToCompareCompletedCreditsWith_NE0_withNot0Value()
        {
            var rd = new RuleDescriptor() { Id = "STCCRL", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.CMPL.CRED", "NE", "\" 0 \""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            var credit = new AcademicCredit("1", new TestCourseRepository().Biol100, "101");
            credit.CompletedCredit = 2m;
            Assert.IsTrue(rule.HasExpression);
            Assert.IsTrue(rule.Passes(credit));
        }

        [TestMethod]
        public void RuleToCompareCompletedCreditsWith_NE0_withNullValue()
        {
            var rd = new RuleDescriptor() { Id = "STCCRL", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.CMPL.CRED", "NE", "\" 0 \""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            var credit = new AcademicCredit("1", new TestCourseRepository().Biol100, "101");
            credit.CompletedCredit = null;
            Assert.IsTrue(rule.HasExpression);
            Assert.IsTrue(rule.Passes(credit));
        }


        [TestMethod]
        public void RuleToCompareCompletedCreditsWith_LT0_withNullValue()
        {
            var rd = new RuleDescriptor() { Id = "STCCRL", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.CMPL.CRED", "LT", "\" 0 \""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            var credit = new AcademicCredit("1", new TestCourseRepository().Biol100, "101");
            credit.CompletedCredit = null;
            Assert.IsTrue(rule.HasExpression);
            Assert.IsTrue(rule.Passes(credit));
        }

        [TestMethod]
        public void RuleToCompareCompletedCreditsWith_LE0_withNullValue()
        {
            var rd = new RuleDescriptor() { Id = "STCCRL", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.CMPL.CRED", "LE", "\" 0 \""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            var credit = new AcademicCredit("1", new TestCourseRepository().Biol100, "101");
            credit.CompletedCredit = null;
            Assert.IsTrue(rule.HasExpression);
            Assert.IsTrue(rule.Passes(credit));
        }

        [TestMethod]
        public void RuleToCompareCompletedCreditsWith_GE0_withNullValue()
        {
            var rd = new RuleDescriptor() { Id = "STCCRL", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.CMPL.CRED", "GE", "\" 0 \""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            var credit = new AcademicCredit("1", new TestCourseRepository().Biol100, "101");
            credit.CompletedCredit = null;
            Assert.IsTrue(rule.HasExpression);
            Assert.IsFalse(rule.Passes(credit));
        }

        [TestMethod]
        public void RuleToCompareCompletedCreditsWith_GT0_withNullValue()
        {
            var rd = new RuleDescriptor() { Id = "STCCRL", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.CMPL.CRED", "GT", "\" 0 \""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            var credit = new AcademicCredit("1", new TestCourseRepository().Biol100, "101");
            credit.CompletedCredit = null;
            Assert.IsTrue(rule.HasExpression);
            Assert.IsFalse(rule.Passes(credit));
        }

        [TestMethod]
        public void NotEqualTo_With0Credits_andEmptyRule()
        {
            var rd = new RuleDescriptor() { Id = "STCCRL", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.CMPL.CRED", "NE", "\'\'"));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            var credit = new AcademicCredit("1", new TestCourseRepository().Biol100, "101");
            credit.CompletedCredit = 0;
            Assert.IsTrue(rule.HasExpression);
            Assert.IsTrue(rule.Passes(credit));
        }

        [TestMethod]
        public void GreaterThanDecimal()
        {
            var rd = new RuleDescriptor() { Id = "STCCRL", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.GPA.CRED", "GT", " \" 1.1234 \" "));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            var credit = new AcademicCredit("1", new TestCourseRepository().Biol100, "101");
            credit.GpaCredit = 1.4321m;
            Assert.IsTrue(rule.HasExpression);
            Assert.IsTrue(rule.Passes(credit));
        }

        [TestMethod]
        public void LessThanOrEqualDecimal()
        {
            var rd = new RuleDescriptor() { Id = "STCCRL", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.GPA.CRED", "LE", "\"1.1234\""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            var credit = new AcademicCredit("1", new TestCourseRepository().Biol100, "101");
            credit.GpaCredit = .982m;
            Assert.IsTrue(rule.HasExpression);
            Assert.IsTrue(rule.Passes(credit));
        }


        [TestMethod]
        public void LessThanOrEqualDecimal_EqualPart()
        {
            var rd = new RuleDescriptor() { Id = "STCCRL", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.GPA.CRED", "LE", "\"1.1234\""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            var credit = new AcademicCredit("1", new TestCourseRepository().Biol100, "101");
            credit.GpaCredit = 1.1234m;
            Assert.IsTrue(rule.HasExpression);
            Assert.IsTrue(rule.Passes(credit));
        }
        [TestMethod]
        public void NotEqualDecimal()
        {
            var rd = new RuleDescriptor() { Id = "STCCRL", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.GPA.CRED", "NE", "\"0.001\""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            var credit = new AcademicCredit("1", new TestCourseRepository().Biol100, "101");
            credit.GpaCredit = .001m;
            Assert.IsTrue(rule.HasExpression);
            Assert.IsFalse(rule.Passes(credit));
        }
        [TestMethod]
        public void NotEqualDecimal_withNotSameValue()
        {
            var rd = new RuleDescriptor() { Id = "STCCRL", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.GPA.CRED", "NE", "\"0.001\""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            var credit = new AcademicCredit("1", new TestCourseRepository().Biol100, "101");
            credit.GpaCredit = .5m;
            Assert.IsTrue(rule.HasExpression);
            Assert.IsTrue(rule.Passes(credit));
        }

        [TestMethod]
        public void EqualDecimal()
        {
            var rd = new RuleDescriptor() { Id = "STCCRL", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.GPA.CRED", "EQ", "\"0.001\""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            var credit = new AcademicCredit("1", new TestCourseRepository().Biol100, "101");
            credit.GpaCredit = .001m;
            Assert.IsTrue(rule.HasExpression);
            Assert.IsTrue(rule.Passes(credit));
        }

        [TestMethod]
        public void EqualDecimal_withNotSameValue()
        {
            var rd = new RuleDescriptor() { Id = "STCCRL", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.GPA.CRED", "EQ", "\"0.001\""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            var credit = new AcademicCredit("1", new TestCourseRepository().Biol100, "101");
            credit.GpaCredit = .5m;
            Assert.IsTrue(rule.HasExpression);
            Assert.IsFalse(rule.Passes(credit));
        }

        [TestMethod]
        public void EqualDecimal_NE0()
        {
            var rd = new RuleDescriptor() { Id = "STCCRL", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.GPA.CRED", "NE", "\"0\""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            var credit = new AcademicCredit("1", new TestCourseRepository().Biol100, "101");
            credit.GpaCredit = .001m;
            Assert.IsTrue(rule.HasExpression);
            Assert.IsTrue(rule.Passes(credit));
        }

        [TestMethod]
        public void EqualDecimal_NE0_Compare0Value()
        {
            var rd = new RuleDescriptor() { Id = "STCCRL", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.GPA.CRED", "NE", "\"0\""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            var credit = new AcademicCredit("1", new TestCourseRepository().Biol100, "101");
            credit.GpaCredit = 0m;
            Assert.IsTrue(rule.HasExpression);
            Assert.IsFalse(rule.Passes(credit));
        }

        [TestMethod]
        public void EqualDecimal_NE0_CompareNULLValue()
        {
            var rd = new RuleDescriptor() { Id = "STCCRL", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.GPA.CRED", "NE", "\"0\""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            var credit = new AcademicCredit("1", new TestCourseRepository().Biol100, "101");
            credit.GpaCredit = null;
            Assert.IsTrue(rule.HasExpression);
            Assert.IsTrue(rule.Passes(credit));
        }

        [TestMethod]
        public void EqualDecimal_EqualToNULL()
        {
            var rd = new RuleDescriptor() { Id = "STCCRL", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.GPA.CRED", "EQ", "\"null\""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            var credit = new AcademicCredit("1", new TestCourseRepository().Biol100, "101");
            credit.GpaCredit = null;
            Assert.IsTrue(rule.HasExpression);
            Assert.IsTrue(rule.Passes(credit));
        }

        [TestMethod]
        public void EqualDecimal_NotEqualToNULL()
        {
            var rd = new RuleDescriptor() { Id = "STCCRL", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.GPA.CRED", "EQ", "\"null\""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            var credit = new AcademicCredit("1", new TestCourseRepository().Biol100, "101");
            credit.GpaCredit = 1.00m;
            Assert.IsTrue(rule.HasExpression);
            Assert.IsFalse(rule.Passes(credit));
        }

        [TestMethod]
        public void NEDecimal_NotEqualToNULL()
        {
            var rd = new RuleDescriptor() { Id = "STCCRL", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.GPA.CRED", "NE", "\"null\""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            var credit = new AcademicCredit("1", new TestCourseRepository().Biol100, "101");
            credit.GpaCredit = 1.00m;
            Assert.IsTrue(rule.HasExpression);
            Assert.IsTrue(rule.Passes(credit));
        }

        [TestMethod]
        public void NEDecimal_EqualToNULL()
        {
            var rd = new RuleDescriptor() { Id = "STCCRL", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.GPA.CRED", "NE", "\"null\""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            var credit = new AcademicCredit("1", new TestCourseRepository().Biol100, "101");
            credit.GpaCredit = null;
            Assert.IsTrue(rule.HasExpression);
            Assert.IsFalse(rule.Passes(credit));
        }

        [TestMethod]
        public async Task EqualSectionId()
        {
            var rd = new RuleDescriptor() { Id = "ABC", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.COURSE.SECTION", "EQ", "\"8002\""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            Assert.IsTrue(rule.HasExpression);
            var credit = await new TestAcademicCreditRepository().GetAsync("1"); // HIST-100 section id 8001
            Assert.IsFalse(rule.Passes(credit));
            credit = await new TestAcademicCreditRepository().GetAsync("2");     // HIST-200 section id 8002
            Assert.IsTrue(rule.Passes(credit));
        }

        [TestMethod]
        public async Task NotEqualSectionId()
        {
            var rd = new RuleDescriptor() { Id = "ABC", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.COURSE.SECTION", "NE", "\'8002\'"));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            Assert.IsTrue(rule.HasExpression);
            var credit = await new TestAcademicCreditRepository().GetAsync("1"); // HIST-100 section id 8001
            Assert.IsTrue(rule.Passes(credit));
            credit = await new TestAcademicCreditRepository().GetAsync("2");     // HIST-200 section id 8002
            Assert.IsFalse(rule.Passes(credit));
        }


        [TestMethod]
        public async Task DateCompare_Adds20thCenturyToTwoDigitYearLessThanThreshhold()
        {
            var rd = new RuleDescriptor() { Id = "SDRULE", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.RuleConversionOptions = new RuleConversionOptions() { DateFormat = "MDY", DateDelimiter = "/", CenturyThreshhold = 68 };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.START.DATE", "EQ", "\'01/15/01\'"));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            Assert.IsTrue(rule.HasExpression);
            var credit = await new TestAcademicCreditRepository().GetAsync("1");
            credit.StartDate = new System.DateTime(2001, 01, 15);
            Assert.IsTrue(rule.Passes(credit));
            credit.StartDate = new System.DateTime(1901, 01, 15);
            Assert.IsFalse(rule.Passes(credit));
        }

        [TestMethod]
        public async Task DateCompare_Adds19thCenturyToTwoDigitYearGreaterThanThreshhold()
        {
            var rd = new RuleDescriptor() { Id = "SDRULE", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.RuleConversionOptions = new RuleConversionOptions() { DateFormat = "MDY", DateDelimiter = "/", CenturyThreshhold = 68 };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.START.DATE", "EQ", "\'01/15/68\'"));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            Assert.IsTrue(rule.HasExpression);
            var credit = await new TestAcademicCreditRepository().GetAsync("1");
            credit.StartDate = new System.DateTime(2068, 01, 15);
            Assert.IsFalse(rule.Passes(credit));
            credit.StartDate = new System.DateTime(1968, 01, 15);
            Assert.IsTrue(rule.Passes(credit));
        }

        [TestMethod]
        public async Task DateCompare_DifferentThreshhold()
        {
            var rd = new RuleDescriptor() { Id = "SDRULE", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.RuleConversionOptions = new RuleConversionOptions() { DateFormat = "MDY", DateDelimiter = "/", CenturyThreshhold = 50 };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.START.DATE", "EQ", "\'01/15/50\'"));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            Assert.IsTrue(rule.HasExpression);
            var credit = await new TestAcademicCreditRepository().GetAsync("1");
            credit.StartDate = new System.DateTime(2050, 01, 15);
            Assert.IsFalse(rule.Passes(credit));
            credit.StartDate = new System.DateTime(1950, 01, 15);
            Assert.IsTrue(rule.Passes(credit));
        }


        [TestMethod]
        public async Task DateCompare_DifferentDelimiter()
        {
            var rd = new RuleDescriptor() { Id = "SDRULE", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.RuleConversionOptions = new RuleConversionOptions() { DateFormat = "MDY", DateDelimiter = ".", CenturyThreshhold = 68 };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.START.DATE", "EQ", "\'01.15.68\'"));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            Assert.IsTrue(rule.HasExpression);
            var credit = await new TestAcademicCreditRepository().GetAsync("1");
            credit.StartDate = new System.DateTime(2068, 01, 15);
            Assert.IsFalse(rule.Passes(credit));
            credit.StartDate = new System.DateTime(1968, 01, 15);
            Assert.IsTrue(rule.Passes(credit));
        }

        [TestMethod]
        public void DateCompare_MisMatchedDelimiter_NotSupported()
        {
            var rd = new RuleDescriptor() { Id = "SDRULE", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.RuleConversionOptions = new RuleConversionOptions() { DateFormat = "MDY", DateDelimiter = "/", CenturyThreshhold = 68 };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.START.DATE", "EQ", "\'01_15_68\'"));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            Assert.IsFalse(rule.HasExpression);
            Assert.IsTrue(!string.IsNullOrEmpty(rule.NotSupportedMessage));
        }

        [TestMethod]
        public async Task DateCompare_MDY_Format()
        {
            var rd = new RuleDescriptor() { Id = "SDRULE", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.RuleConversionOptions = new RuleConversionOptions() { DateFormat = "MDY", DateDelimiter = ".", CenturyThreshhold = 68 };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.START.DATE", "EQ", "\'01.15.68\'"));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            Assert.IsTrue(rule.HasExpression);
            var credit = await new TestAcademicCreditRepository().GetAsync("1");
            credit.StartDate = new System.DateTime(2068, 01, 15);
            Assert.IsFalse(rule.Passes(credit));
            credit.StartDate = new System.DateTime(1968, 01, 15);
            Assert.IsTrue(rule.Passes(credit));
        }

        [TestMethod]
        public async Task DateCompare_YMD_Format()
        {
            var rd = new RuleDescriptor() { Id = "SDRULE", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.RuleConversionOptions = new RuleConversionOptions() { DateFormat = "YMD", DateDelimiter = "/", CenturyThreshhold = 68 };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.START.DATE", "EQ", "\'68/01/15\'"));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            Assert.IsTrue(rule.HasExpression);
            var credit = await new TestAcademicCreditRepository().GetAsync("1");
            credit.StartDate = new System.DateTime(2068, 01, 15);
            Assert.IsFalse(rule.Passes(credit));
            credit.StartDate = new System.DateTime(1968, 01, 15);
            Assert.IsTrue(rule.Passes(credit));
        }

        [TestMethod]
        public async Task DateCompare_DMY_Format()
        {
            var rd = new RuleDescriptor() { Id = "SDRULE", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.RuleConversionOptions = new RuleConversionOptions() { DateFormat = "DMY", DateDelimiter = "/", CenturyThreshhold = 68 };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.START.DATE", "EQ", "\'15/01/68\'"));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            Assert.IsTrue(rule.HasExpression);
            var credit = await new TestAcademicCreditRepository().GetAsync("1");
            credit.StartDate = new System.DateTime(2068, 01, 15);
            Assert.IsFalse(rule.Passes(credit));
            credit.StartDate = new System.DateTime(1968, 01, 15);
            Assert.IsTrue(rule.Passes(credit));
        }

        [TestMethod]
        public void DateCompare_InvalidFormat_NotSupported()
        {
            var rd = new RuleDescriptor() { Id = "SDRULE", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.RuleConversionOptions = new RuleConversionOptions() { DateFormat = "XYZ", DateDelimiter = "/", CenturyThreshhold = 68 };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.START.DATE", "EQ", "\'01_15_68\'"));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            Assert.IsFalse(rule.HasExpression);
            Assert.IsTrue(!string.IsNullOrEmpty(rule.NotSupportedMessage));
        }

        [TestMethod]
        public async Task DateCompare_SpaceDelimiter()
        {
            var rd = new RuleDescriptor() { Id = "SDRULE", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.RuleConversionOptions = new RuleConversionOptions() { DateFormat = "MDY", DateDelimiter = " ", CenturyThreshhold = 68 };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.START.DATE", "EQ", "\'01 15 68\'"));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            Assert.IsTrue(rule.HasExpression);
            var credit = await new TestAcademicCreditRepository().GetAsync("1");
            credit.StartDate = new System.DateTime(2068, 01, 15);
            Assert.IsFalse(rule.Passes(credit));
            credit.StartDate = new System.DateTime(1968, 01, 15);
            Assert.IsTrue(rule.Passes(credit));
        }

        [TestMethod]
        public async Task DateCompare_Equal_StartDate()
        {
            var rd = new RuleDescriptor() { Id = "SDRULE", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.RuleConversionOptions = new RuleConversionOptions() { DateFormat = "MDY", DateDelimiter = "/", CenturyThreshhold = 68 };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.START.DATE", "EQ", "\"01/15/2001\""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            Assert.IsTrue(rule.HasExpression);
            var credit = await new TestAcademicCreditRepository().GetAsync("1");
            credit.StartDate = new System.DateTime(2001, 01, 15);
            Assert.IsTrue(rule.Passes(credit));
            credit.StartDate = new System.DateTime(2014, 01, 15);
            Assert.IsFalse(rule.Passes(credit));
        }

        [TestMethod]
        public async Task DateCompare_NotEqual_StartDate()
        {
            var rd = new RuleDescriptor() { Id = "SDRULE", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.RuleConversionOptions = new RuleConversionOptions() { DateFormat = "MDY", DateDelimiter = "/", CenturyThreshhold = 68 };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.START.DATE", "NE", "\'01/15/01\'"));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            Assert.IsTrue(rule.HasExpression);
            var credit = await new TestAcademicCreditRepository().GetAsync("1");
            credit.StartDate = new System.DateTime(2001, 01, 15);
            Assert.IsFalse(rule.Passes(credit));
            credit.StartDate = new System.DateTime(2014, 01, 15);
            Assert.IsTrue(rule.Passes(credit));
        }

        [TestMethod]
        public async Task DateCompare_GreaterThan_StartDate()
        {
            var rd = new RuleDescriptor() { Id = "SDRULE", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.RuleConversionOptions = new RuleConversionOptions() { DateFormat = "MDY", DateDelimiter = "/", CenturyThreshhold = 68 };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.START.DATE", "GT", "\'01/15/2001\'"));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            Assert.IsTrue(rule.HasExpression);
            var credit = await new TestAcademicCreditRepository().GetAsync("1");
            credit.StartDate = new System.DateTime(2001, 01, 15);
            Assert.IsFalse(rule.Passes(credit));
            credit.StartDate = new System.DateTime(2001, 02, 15);
            Assert.IsTrue(rule.Passes(credit));
        }

        [TestMethod]
        public async Task DateCompare_GreaterThanOrEqualTo_StartDate()
        {
            var rd = new RuleDescriptor() { Id = "SDRULE", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.RuleConversionOptions = new RuleConversionOptions() { DateFormat = "MDY", DateDelimiter = "/", CenturyThreshhold = 68 };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.START.DATE", "GE", "\'01/15/2001\'"));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            Assert.IsTrue(rule.HasExpression);
            var credit = await new TestAcademicCreditRepository().GetAsync("1");
            credit.StartDate = new System.DateTime(2001, 01, 14);
            Assert.IsFalse(rule.Passes(credit));
            credit.StartDate = new System.DateTime(2001, 01, 15);
            Assert.IsTrue(rule.Passes(credit));
            credit.StartDate = new System.DateTime(2001, 02, 15);
            Assert.IsTrue(rule.Passes(credit));
        }


        [TestMethod]
        public async Task DateCompare_LessThan_StartDate()
        {
            var rd = new RuleDescriptor() { Id = "SDRULE", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.RuleConversionOptions = new RuleConversionOptions() { DateFormat = "MDY", DateDelimiter = "/", CenturyThreshhold = 68 };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.START.DATE", "LT", "\'03/31/2015\'"));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            Assert.IsTrue(rule.HasExpression);
            var credit = await new TestAcademicCreditRepository().GetAsync("1");
            credit.StartDate = new System.DateTime(2015, 3, 31);
            Assert.IsFalse(rule.Passes(credit));
            credit.StartDate = new System.DateTime(2015, 03, 30);
            Assert.IsTrue(rule.Passes(credit));
        }

        [TestMethod]
        public async Task DateCompare_LessThanOrEqualTo_StartDate()
        {
            var rd = new RuleDescriptor() { Id = "SDRULE", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.RuleConversionOptions = new RuleConversionOptions() { DateFormat = "MDY", DateDelimiter = "/", CenturyThreshhold = 68 };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.START.DATE", "LE", "\'05/12/14\'"));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            Assert.IsTrue(rule.HasExpression);
            var credit = await new TestAcademicCreditRepository().GetAsync("1");
            credit.StartDate = new System.DateTime(2014, 1, 9);
            Assert.IsTrue(rule.Passes(credit));
            credit.StartDate = new System.DateTime(2014, 5, 12);
            Assert.IsTrue(rule.Passes(credit));
            credit.StartDate = new System.DateTime(2014, 8, 1);
            Assert.IsFalse(rule.Passes(credit));
        }

        [TestMethod]
        public async Task EqualGradeSchemeCode()
        {
            var rd = new RuleDescriptor() { Id = "ABC", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.GRADE.SCHEME", "EQ", "\"UG\""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            Assert.IsTrue(rule.HasExpression);
            var credit = await new TestAcademicCreditRepository().GetAsync("99");
            Assert.IsTrue(rule.Passes(credit));
        }

        [TestMethod]
        public async Task EqualAcademicLevelCode()
        {
            var rd = new RuleDescriptor() { Id = "ABC", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.ACAD.LEVEL", "EQ", "\"UG\""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            Assert.IsTrue(rule.HasExpression);
            var credit = await new TestAcademicCreditRepository().GetAsync("99");
            Assert.IsTrue(rule.Passes(credit));
        }

        [TestMethod]
        public async Task EqualSectionNumber()
        {
            var rd = new RuleDescriptor() { Id = "ABC", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.SECTION.NO", "EQ", "\"001\""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            Assert.IsTrue(rule.HasExpression);
            var credit = await new TestAcademicCreditRepository().GetAsync("99");
            Assert.IsTrue(rule.Passes(credit));
        }

        [TestMethod]
        public async Task EqualMark()
        {
            var rd = new RuleDescriptor() { Id = "ABC", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.MARK", "EQ", "\"ABCD\""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            Assert.IsTrue(rule.HasExpression);
            var credit = await new TestAcademicCreditRepository().GetAsync("99");
            Assert.IsTrue(rule.Passes(credit));
        }

        [TestMethod]
        public async Task EqualStudentId()
        {
            var rd = new RuleDescriptor() { Id = "ABC", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.PERSON.ID", "EQ", "\"0001234\""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            Assert.IsTrue(rule.HasExpression);
            var credit = await new TestAcademicCreditRepository().GetAsync("99");
            Assert.IsTrue(rule.Passes(credit));
        }

        [TestMethod]
        public async Task EqualFinalGrade()
        {
            var rd = new RuleDescriptor() { Id = "ABC", PrimaryView = "STUDENT.ACAD.CRED" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "STC.FINAL.GRADE", "EQ", "\"A\""));
            var rule = (Rule<AcademicCredit>)new AcademicCreditRuleAdapter().Create(rd);
            Assert.IsTrue(rule.HasExpression);
            var credit = await new TestAcademicCreditRepository().GetAsync("99");
            Assert.IsTrue(rule.Passes(credit));
        }
    }
}