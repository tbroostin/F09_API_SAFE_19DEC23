// Copyright 2012-2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Data.Student.Tests
{
    [TestClass]
    public class CourseRuleAdapterTests
    {
        [TestClass]
        public class Create
        {
            private TestCourseRepository courseRepo = new TestCourseRepository();

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ThrowsExceptionIfDescriptorNull()
            {
                new CourseRuleAdapter().Create(null);
            }

            [TestMethod]
            public void SetsId()
            {
                var rd = new RuleDescriptor();
                rd.Id = "id";
                rd.PrimaryView = "COURSES";
                var rule = new CourseRuleAdapter().Create(rd);
                Assert.AreEqual(rd.Id, rule.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void ThrowsExceptionIfPrimaryViewNotCourses()
            {
                var rd = new RuleDescriptor();
                rd.Id = "id";
                rd.PrimaryView = "JUNK";
                new CourseRuleAdapter().Create(rd);
            }

            [TestMethod]
            public void Equal()
            {
                var rd = new RuleDescriptor();
                rd.Id = "SIMPLE.EQUALS";
                rd.PrimaryView = "COURSES";
                rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "COURSES.ID", "EQ", "'139'"));
                var rule = new CourseRuleAdapter().Create(rd);
                Assert.IsTrue(rule.GetType() == typeof(Rule<Course>));
                var theRule = (Rule<Course>)rule;

                var repo = new TestCourseRepository();
                Assert.IsTrue(theRule.Passes(repo.GetAsync("139").Result));
                Assert.IsFalse(theRule.Passes(repo.GetAsync("42").Result));
            }

            [TestMethod]
            public void EqualAndEqual()
            {
                var rd = new RuleDescriptor();
                rd.Id = "SIMPLE.EQUALS";
                rd.PrimaryView = "COURSES";
                rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "CRS.SUBJECT", "EQ", "'BIOL'"));
                rd.Expressions.Add(new RuleExpressionDescriptor("AND", "CRS.NO", "EQ", "'100'"));
                var rule = new CourseRuleAdapter().Create(rd);
                Assert.IsTrue(rule.GetType() == typeof(Rule<Course>));
                var theRule = (Rule<Course>)rule;

                var repo = new TestCourseRepository();
                Assert.IsTrue(theRule.Passes(repo.GetAsync("110").Result)); // BIOL-100
                Assert.IsFalse(theRule.Passes(repo.GetAsync("21").Result)); // BIOL-200
                
            }

            [TestMethod]
            public void OrWith2()
            {
                var rd = new RuleDescriptor() { Id = "WHATEVS", PrimaryView = "COURSES" };
                rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "CRS.SUBJECT", "EQ", "\"BIOL\""));
                rd.Expressions.Add(new RuleExpressionDescriptor("ORWITH", "CRS.SUBJECT", "EQ", "\"MATH\""));
                rd.Expressions.Add(new RuleExpressionDescriptor("AND", "CRS.NO", "EQ", "\"100\""));
                var rule = (Rule<Course>)new CourseRuleAdapter().Create(rd);
                Assert.IsTrue(rule.HasExpression);
                Assert.IsTrue(rule.Passes(courseRepo.Biol100));
                Assert.IsTrue(rule.Passes(courseRepo.Biol200));
                Assert.IsTrue(rule.Passes(courseRepo.Math100));
                Assert.IsFalse(rule.Passes(courseRepo.Hist400));
            }

            [TestMethod]
            public void CrsName()
            {
                var rd = new RuleDescriptor() { Id = "WHATEVS", PrimaryView = "COURSES" };
                rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "CRS.NAME", "EQ", "\"BIOL-100\""));
                var rule = (Rule<Course>)new CourseRuleAdapter().Create(rd);
                Assert.IsTrue(rule.HasExpression);
                Assert.IsTrue(rule.Passes(courseRepo.Biol100));
                Assert.IsFalse(rule.Passes(courseRepo.Biol200));
                Assert.IsFalse(rule.Passes(courseRepo.Math100));
                Assert.IsFalse(rule.Passes(courseRepo.Hist400));
            }

            [TestMethod]
            public void CrsDepts()
            {
                var rd = new RuleDescriptor() { Id = "HISTDEPT", PrimaryView = "COURSES" };
                rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "CRS.DEPTS", "EQ", "\"HIST\""));
                var rule = (Rule<Course>)new CourseRuleAdapter().Create(rd);
                Assert.IsTrue(rule.HasExpression);
                Assert.IsTrue(rule.Passes(courseRepo.Hist400));
                Assert.IsFalse(rule.Passes(courseRepo.Biol200));

            }

            [TestMethod]
            public void CrsLevels()
            {
                var rd = new RuleDescriptor() { Id = "LVL200", PrimaryView = "COURSES" };
                rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "CRS.LEVELS", "EQ", "\"200\""));
                var rule = (Rule<Course>)new CourseRuleAdapter().Create(rd);
                Assert.IsTrue(rule.HasExpression);
                Assert.IsFalse(rule.Passes(courseRepo.Hist400));
                Assert.IsTrue(rule.Passes(courseRepo.Biol200));

            }

            [TestMethod]
            public void CrsCourseTypes()
            {
                var rd = new RuleDescriptor() { Id = "CRSTYPE", PrimaryView = "COURSES" };
                rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "CRS.COURSE.TYPES", "EQ", "\"TYPEA\""));
                var rule = (Rule<Course>)new CourseRuleAdapter().Create(rd);
                Assert.IsTrue(rule.HasExpression);
                Assert.IsTrue(rule.Passes(courseRepo.Hist400));
            }

            [TestMethod]
            public void CrsCredType()
            {
                var rd = new RuleDescriptor() { Id = "CRSCREDTYPE", PrimaryView = "COURSES" };
                rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "CRS.CRED.TYPE", "EQ", "\"IN\""));
                var rule = (Rule<Course>)new CourseRuleAdapter().Create(rd);
                Assert.IsTrue(rule.HasExpression);
                Assert.IsTrue(rule.Passes(courseRepo.Hist400));
            }

            [TestMethod]
            public void CrsSubject()
            {
                var rd = new RuleDescriptor() { Id = "CRSSUBJECT", PrimaryView = "COURSES" };
                rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "CRS.SUBJECT", "EQ", "\"HIST\""));
                var rule = (Rule<Course>)new CourseRuleAdapter().Create(rd);
                Assert.IsTrue(rule.HasExpression);
                Assert.IsTrue(rule.Passes(courseRepo.Hist400));
            }


            [TestMethod]
            public void CrsNo()
            {
                var rd = new RuleDescriptor() { Id = "CRSNO", PrimaryView = "COURSES" };
                rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "CRS.NO", "EQ", "\"400\""));
                var rule = (Rule<Course>)new CourseRuleAdapter().Create(rd);
                Assert.IsTrue(rule.HasExpression);
                Assert.IsTrue(rule.Passes(courseRepo.Hist400));
            }

            [TestMethod]
            public void CrsShortTitle()
            {
                var rd = new RuleDescriptor() { Id = "CRSSHORTTITLE", PrimaryView = "COURSES" };
                rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "CRS.SHORT.TITLE", "EQ", "\"American History From WWI\""));
                var rule = (Rule<Course>)new CourseRuleAdapter().Create(rd);
                Assert.IsTrue(rule.HasExpression);
                Assert.IsTrue(rule.Passes(courseRepo.Hist400));
            }

            [TestMethod]
            public void CrsLongTitle()
            {
                var rd = new RuleDescriptor() { Id = "CRSLONGTITLE", PrimaryView = "COURSES" };
                rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "CRS.LONG.TITLE", "EQ", "\"American History From WWI and some extra text\""));
                var rule = (Rule<Course>)new CourseRuleAdapter().Create(rd);
                Assert.IsTrue(rule.HasExpression);
                Assert.IsTrue(rule.Passes(courseRepo.Hist400));
            }

            [TestMethod]
            public void CrsAcadLevel()
            {
                var rd = new RuleDescriptor() { Id = "CRSACADLEVEL", PrimaryView = "COURSES" };
                rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "CRS.ACAD.LEVEL", "EQ", "\"UG\""));
                var rule = (Rule<Course>)new CourseRuleAdapter().Create(rd);
                Assert.IsTrue(rule.HasExpression);
                Assert.IsTrue(rule.Passes(courseRepo.Hist400));
            }

            [TestMethod]
            public void CrsSessionCycle()
            {
                var rd = new RuleDescriptor() { Id = "CRSSESSIONCYCLE", PrimaryView = "COURSES" };
                rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "CRS.SESSION.CYCLE", "EQ", "\"A\""));
                var rule = (Rule<Course>)new CourseRuleAdapter().Create(rd);
                Assert.IsTrue(rule.HasExpression);
                Assert.IsTrue(rule.Passes(courseRepo.Hist400));
            }

            [TestMethod]
            public void CrsYearlyCycle()
            {
                var rd = new RuleDescriptor() { Id = "CRSYEARLYCYCLE", PrimaryView = "COURSES" };
                rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "CRS.YEARLY.CYCLE", "EQ", "\"B\""));
                var rule = (Rule<Course>)new CourseRuleAdapter().Create(rd);
                Assert.IsTrue(rule.HasExpression);
                Assert.IsTrue(rule.Passes(courseRepo.Hist400));
            }

            [TestMethod]
            public void CrsTopicCode()
            {
                var rd = new RuleDescriptor() { Id = "CRSTOPICCODE", PrimaryView = "COURSES" };
                rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "CRS.TOPIC.CODE", "EQ", "\"\""));
                var rule = (Rule<Course>)new CourseRuleAdapter().Create(rd);
                Assert.IsTrue(rule.HasExpression);
                Assert.IsTrue(rule.Passes(courseRepo.Hist400));
            }

            [TestMethod]
            public void CrsCip()
            {
                var rd = new RuleDescriptor() { Id = "CRSCIP", PrimaryView = "COURSES" };
                rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "CRS.CIP", "EQ", "\"ABCD\""));
                var rule = (Rule<Course>)new CourseRuleAdapter().Create(rd);
                Assert.IsTrue(rule.HasExpression);
                Assert.IsTrue(rule.Passes(courseRepo.Hist400));
            }

            [TestMethod]
            public void CrsGradeScheme()
            {
                var rd = new RuleDescriptor() { Id = "CRSGRADESCHEME", PrimaryView = "COURSES" };
                rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "CRS.GRADE.SCHEME", "EQ", "\"UG\""));
                var rule = (Rule<Course>)new CourseRuleAdapter().Create(rd);
                Assert.IsTrue(rule.HasExpression);
                Assert.IsTrue(rule.Passes(courseRepo.Hist400));
            }

            [TestMethod]
            public void CrsExternalSource()
            {
                var rd = new RuleDescriptor() { Id = "CRSEXTERNALSOURCE", PrimaryView = "COURSES" };
                rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "CRS.EXTERNAL.SOURCE", "EQ", "\"EXT\""));
                var rule = (Rule<Course>)new CourseRuleAdapter().Create(rd);
                Assert.IsTrue(rule.HasExpression);
                Assert.IsTrue(rule.Passes(courseRepo.Hist400));
            }
        }
    }
}