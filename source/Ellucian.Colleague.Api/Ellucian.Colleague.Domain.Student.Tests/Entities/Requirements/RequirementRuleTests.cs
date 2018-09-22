// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities.Requirements
{
    [TestClass]
    public class RequirementRuleTests
    {
        private AcademicCredit biol100 = new TestAcademicCreditRepository().Biol100;

        private Group group;
        private string termId;
        private IEnumerable<Course> equatedCourses;
        private List<RequirementType> requirementTypes = new TestRequirementRepository().RequirementTypes;

        [TestInitialize]
        public void Initialize()
        {
            var subreq = new Subrequirement("2", "2");
            var req = new Requirement("ID", "CODE", "DESC", "UG", requirementTypes.First(rt=>rt.Code == "MAJ"));
            subreq.Requirement = req;
            req.SubRequirements.Add(subreq);
            group = new Group("1", "1", subreq);
            group.MinCourses = 1;
            subreq.Groups.Add(group);
            termId = "2014/FA";
            equatedCourses = new TestCourseRepository().GetAsync().Result;
        }

        [TestMethod]
        public void CreditPassesCreditRuleIfSatisfactory()
        {
            // Build the following lambda expression:  c => c.Id == biol100.Id
            var param = Expression.Parameter(typeof(AcademicCredit), "c");
            var leftExp = Expression.Property(param, "Id");
            var rightExp = Expression.Constant(biol100.Id, typeof(string));
            var equalityExp = Expression.Equal(leftExp, rightExp);
            var lambdaExp = Expression.Lambda<Func<AcademicCredit, bool>>(equalityExp, param);

            var rule = new Rule<AcademicCredit>("ID", lambdaExp);
            group.AcademicCreditRules.Add(new RequirementRule(rule));
            var acadResult = new CreditResult(biol100);
            var groupResult = group.Evaluate(new List<AcadResult>() { acadResult }, new List<Override>(), equatedCourses);
            Assert.AreEqual(1, groupResult.CountApplied());
            Assert.AreEqual(Result.Applied, acadResult.Result);
        }

        [TestMethod]
        public void CreditFailsCreditRuleIfNotSatisfactory()
        {
            // Build the following lambda expression:  c => c.Id == "asdjfiaoewjr"
            var param = Expression.Parameter(typeof(AcademicCredit), "c");
            var leftExp = Expression.Property(param, "Id");
            var rightExp = Expression.Constant("asdjfiaoewjr", typeof(string));
            var equalityExp = Expression.Equal(leftExp, rightExp);
            var lambdaExp = Expression.Lambda<Func<AcademicCredit, bool>>(equalityExp, param);

            var rule = new Rule<AcademicCredit>("ID", lambdaExp);
            group.AcademicCreditRules.Add(new RequirementRule(rule));
            var acadResult = new CreditResult(biol100);
            var groupResult = group.Evaluate(new List<AcadResult>() { acadResult }, new List<Override>(), equatedCourses);
            Assert.AreEqual(0, groupResult.CountApplied());
            Assert.AreEqual(Result.RuleFailed, acadResult.Result);
        }

        [TestMethod]
        public void CreditPassesCreditRuleAtHigherLevelIfSatisfactory()
        {
            // Build the following lambda expression:  c => c.Id == biol100.Id
            var param = Expression.Parameter(typeof(AcademicCredit), "c");
            var leftExp = Expression.Property(param, "Id");
            var rightExp = Expression.Constant(biol100.Id, typeof(string));
            var equalityExp = Expression.Equal(leftExp, rightExp);
            var lambdaExp = Expression.Lambda<Func<AcademicCredit, bool>>(equalityExp, param);

            var rule = new Rule<AcademicCredit>("ID", lambdaExp);
            group.SubRequirement.AcademicCreditRules.Add(new RequirementRule(rule));
            var acadResult = new CreditResult(biol100);
            var groupResult = group.Evaluate(new List<AcadResult>() { acadResult }, new List<Override>(), equatedCourses);
            Assert.AreEqual(1, groupResult.CountApplied());
            Assert.AreEqual(Result.Applied, acadResult.Result);
        }

        [TestMethod]
        public void CreditFailsCreditRuleAtHigherLevelIfNotSatisfactory()
        {
            // Build the following lambda expression:  c => c.Id == "asjdfkljslkf"
            var param = Expression.Parameter(typeof(AcademicCredit), "c");
            var leftExp = Expression.Property(param, "Id");
            var rightExp = Expression.Constant("asjdfkljslkf", typeof(string));
            var equalityExp = Expression.Equal(leftExp, rightExp);
            var lambdaExp = Expression.Lambda<Func<AcademicCredit, bool>>(equalityExp, param);

            var rule = new Rule<AcademicCredit>("ID", lambdaExp);
            group.SubRequirement.AcademicCreditRules.Add(new RequirementRule(rule));
            var acadResult = new CreditResult(biol100);
            var groupResult = group.Evaluate(new List<AcadResult>() { acadResult }, new List<Override>(), equatedCourses);
            Assert.AreEqual(0, groupResult.CountApplied());
            Assert.AreEqual(Result.RuleFailed, acadResult.Result);
        }

        [TestMethod]
        public void CreditPassesCourseRuleIfAssociatedCourseSatisfactory()
        {
            // Build the following lambda expression:  c.Id == biol100.Course.Id
            var param = Expression.Parameter(typeof(Course), "c");
            var leftExp = Expression.Property(param, "Id");
            var rightExp = Expression.Constant(biol100.Course.Id, typeof(string));
            var equalityExp = Expression.Equal(leftExp, rightExp);
            var lambdaExp = Expression.Lambda<Func<Course, bool>>(equalityExp, param);

            var rule = new Rule<Course>("ID", lambdaExp);
            group.AcademicCreditRules.Add(new RequirementRule(rule));
            var acadResult = new CreditResult(biol100);
            var groupResult = group.Evaluate(new List<AcadResult>() { acadResult }, new List<Override>(), equatedCourses);
            Assert.AreEqual(1, groupResult.CountApplied());
            Assert.AreEqual(Result.Applied, acadResult.Result);
        }

        [TestMethod]
        public void CreditFailsCourseRuleIfAssociatedCourseNotSatisfactory()
        {
            // Build the following lambda expression:  c => c.Id == "sdjfksdfjklds"
            var param = Expression.Parameter(typeof(Course), "c");
            var leftExp = Expression.Property(param, "Id");
            var rightExp = Expression.Constant("sdjfksdfjklds", typeof(string));
            var equalityExp = Expression.Equal(leftExp, rightExp);
            var lambdaExp = Expression.Lambda<Func<Course, bool>>(equalityExp, param);

            var rule = new Rule<Course>("ID", lambdaExp);
            group.AcademicCreditRules.Add(new RequirementRule(rule));
            var acadResult = new CreditResult(biol100);
            var groupResult = group.Evaluate(new List<AcadResult>() { acadResult }, new List<Override>(), equatedCourses);
            Assert.AreEqual(0, groupResult.CountApplied());
            Assert.AreEqual(Result.RuleFailed, acadResult.Result);
        }

        [TestMethod]
        public void CreditPassesCourseRuleAtHigherLevelIfAssociatedCourseSatisfactory()
        {
            // Build the following lambda expression:  c => c.Id == biol100.Course.Id
            var param = Expression.Parameter(typeof(Course), "c");
            var leftExp = Expression.Property(param, "Id");
            var rightExp = Expression.Constant(biol100.Course.Id, typeof(string));
            var equalityExp = Expression.Equal(leftExp, rightExp);
            var lambdaExp = Expression.Lambda<Func<Course, bool>>(equalityExp, param);

            var rule = new Rule<Course>("ID", lambdaExp);
            group.SubRequirement.AcademicCreditRules.Add(new RequirementRule(rule));
            var acadResult = new CreditResult(biol100);
            var groupResult = group.Evaluate(new List<AcadResult>() { acadResult }, new List<Override>(), equatedCourses);
            Assert.AreEqual(1, groupResult.CountApplied());
            Assert.AreEqual(Result.Applied, acadResult.Result);
        }

        [TestMethod]
        public void CreditFailsCourseRuleAtHigherLevelIfAssociatedCourseNotSatisfactory()
        {
            // Build the following lambda expression:  c => c.Id == "ssdjkflsajfklds"
            var param = Expression.Parameter(typeof(Course), "c");
            var leftExp = Expression.Property(param, "Id");
            var rightExp = Expression.Constant("ssdjkflsajfklds", typeof(string));
            var equalityExp = Expression.Equal(leftExp, rightExp);
            var lambdaExp = Expression.Lambda<Func<Course, bool>>(equalityExp, param);

            var rule = new Rule<Course>("ID", lambdaExp);
            group.SubRequirement.AcademicCreditRules.Add(new RequirementRule(rule));
            var acadResult = new CreditResult(biol100);
            var groupResult = group.Evaluate(new List<AcadResult>() { acadResult }, new List<Override>(), equatedCourses);
            Assert.AreEqual(0, groupResult.CountApplied());
            Assert.AreEqual(Result.RuleFailed, acadResult.Result);
        }

        // BEGIN CREDIT WITHOUT COURSE SECTION

        [TestMethod]
        public void CreditWithoutCourseFailsCourseRule()
        {
            // Build the following lambda expression:  c => c.Id == "nomatter"
            var param = Expression.Parameter(typeof(Course), "c");
            var leftExp = Expression.Property(param, "Id");
            var rightExp = Expression.Constant("nomatter", typeof(string));
            var equalityExp = Expression.Equal(leftExp, rightExp);
            var lambdaExp = Expression.Lambda<Func<Course, bool>>(equalityExp, param);

            var credit = new AcademicCredit("999");
            var rule = new Rule<Course>("ID", lambdaExp);
            group.AcademicCreditRules.Add(new RequirementRule(rule));
            var acadResult = new CreditResult(credit);
            var groupResult = group.Evaluate(new List<AcadResult>() { acadResult }, new List<Override>(), equatedCourses);
            Assert.AreEqual(0, groupResult.CountApplied());
            Assert.AreEqual(Result.RuleFailed, acadResult.Result);
        }

        [TestMethod]
        public void CreditWithoutCourseFailsCourseRuleAtHigherLevel()
        {
            // Build the following lambda expression:  c => c.Id == "nomatter"
            var param = Expression.Parameter(typeof(Course), "c");
            var leftExp = Expression.Property(param, "Id");
            var rightExp = Expression.Constant("nomatter", typeof(string));
            var equalityExp = Expression.Equal(leftExp, rightExp);
            var lambdaExp = Expression.Lambda<Func<Course, bool>>(equalityExp, param);

            var credit = new AcademicCredit("999");
            var rule = new Rule<Course>("ID", lambdaExp);
            group.SubRequirement.AcademicCreditRules.Add(new RequirementRule(rule));
            var acadResult = new CreditResult(credit);
            var groupResult = group.Evaluate(new List<AcadResult>() { acadResult }, new List<Override>(), equatedCourses);
            Assert.AreEqual(0, groupResult.CountApplied());
            Assert.AreEqual(Result.RuleFailed, acadResult.Result);
        }

        // BEGIN PLANNED COURSES SECTION

        [TestMethod]
        public void CourseAlwaysFailsCreditRule()
        {
            // Build the following lambda expression:  c => c.Id == "nomatter"
            var param = Expression.Parameter(typeof(AcademicCredit), "c");
            var leftExp = Expression.Property(param, "Id");
            var rightExp = Expression.Constant("nomatter", typeof(string));
            var equalityExp = Expression.Equal(leftExp, rightExp);
            var lambdaExp = Expression.Lambda<Func<AcademicCredit, bool>>(equalityExp, param);

            var rule = new Rule<AcademicCredit>("ID", lambdaExp);
            group.AcademicCreditRules.Add(new RequirementRule(rule));
            var acadResult = new CourseResult(new PlannedCredit(biol100.Course, termId));
            var groupResult = group.Evaluate(new List<AcadResult>() { acadResult }, new List<Override>(), equatedCourses);
            Assert.AreEqual(0, groupResult.CountApplied());
            Assert.AreEqual(Result.RuleFailed, acadResult.Result);
        }

        [TestMethod]
        public void CourseAlwaysPassesCreditRuleAtHigherLevel()
        {
            // Build the following lambda expression:  c => c.Id == "nomatter"
            var param = Expression.Parameter(typeof(AcademicCredit), "c");
            var leftExp = Expression.Property(param, "Id");
            var rightExp = Expression.Constant("nomatter", typeof(string));
            var equalityExp = Expression.Equal(leftExp, rightExp);
            var lambdaExp = Expression.Lambda<Func<AcademicCredit, bool>>(equalityExp, param);

            var rule = new Rule<AcademicCredit>("ID", lambdaExp);
            group.SubRequirement.AcademicCreditRules.Add(new RequirementRule(rule));
            var acadResult = new CourseResult(new PlannedCredit(biol100.Course, termId));
            var groupResult = group.Evaluate(new List<AcadResult>() { acadResult }, new List<Override>(), equatedCourses);
            Assert.AreEqual(1, groupResult.CountPlannedApplied());
            Assert.AreEqual(Result.PlannedApplied, acadResult.Result);
        }

        [TestMethod]
        public void CoursePassesCourseRuleIfCourseSatisfactory()
        {
            // Build the following lambda expression:  c => c.Id == biol100.Course.Id
            var param = Expression.Parameter(typeof(Course), "c");
            var leftExp = Expression.Property(param, "Id");
            var rightExp = Expression.Constant(biol100.Course.Id, typeof(string));
            var equalityExp = Expression.Equal(leftExp, rightExp);
            var lambdaExp = Expression.Lambda<Func<Course, bool>>(equalityExp, param);

            var rule = new Rule<Course>("ID", lambdaExp);
            group.AcademicCreditRules.Add(new RequirementRule(rule));
            var acadResult = new CourseResult(new PlannedCredit(biol100.Course, termId));
            var groupResult = group.Evaluate(new List<AcadResult>() { acadResult }, new List<Override>(), equatedCourses);
            Assert.AreEqual(1, groupResult.CountPlannedApplied());
            Assert.AreEqual(Result.PlannedApplied, acadResult.Result);
        }

        [TestMethod]
        public void CourseFailsCourseRuleIfCourseNotSatisfactory()
        {
            // Build the following lambda expression:  c => c.Id == "whatevs"
            var param = Expression.Parameter(typeof(Course), "c");
            var leftExp = Expression.Property(param, "Id");
            var rightExp = Expression.Constant("whatevs", typeof(string));
            var equalityExp = Expression.Equal(leftExp, rightExp);
            var lambdaExp = Expression.Lambda<Func<Course, bool>>(equalityExp, param);

            var rule = new Rule<Course>("ID", lambdaExp);
            group.AcademicCreditRules.Add(new RequirementRule(rule));
            var acadResult = new CourseResult(new PlannedCredit(biol100.Course, termId));
            var groupResult = group.Evaluate(new List<AcadResult>() { acadResult }, new List<Override>(), equatedCourses);
            Assert.AreEqual(0, groupResult.CountApplied());
            Assert.AreEqual(Result.RuleFailed, acadResult.Result);
        }

        [TestMethod]
        public void CoursePassesCourseRuleAtHigherLevelIfCourseSatisfactory()
        {
            // Build the following lambda expression:  c => c.Id == biol100.Course.Id
            var param = Expression.Parameter(typeof(Course), "c");
            var leftExp = Expression.Property(param, "Id");
            var rightExp = Expression.Constant(biol100.Course.Id, typeof(string));
            var equalityExp = Expression.Equal(leftExp, rightExp);
            var lambdaExp = Expression.Lambda<Func<Course, bool>>(equalityExp, param);

            var rule = new Rule<Course>("ID", lambdaExp);
            group.SubRequirement.AcademicCreditRules.Add(new RequirementRule(rule));
            var acadResult = new CourseResult(new PlannedCredit(biol100.Course, termId));
            var groupResult = group.Evaluate(new List<AcadResult>() { acadResult }, new List<Override>(), equatedCourses);
            Assert.AreEqual(1, groupResult.CountPlannedApplied());
            Assert.AreEqual(Result.PlannedApplied, acadResult.Result);
        }

        [TestMethod]
        public void CourseFailsCourseRuleAtHigherLevelIfCourseNotSatisfactory()
        {
            // Build the following lambda expression:  c => c.Id == "whatevs"
            var param = Expression.Parameter(typeof(Course), "c");
            var leftExp = Expression.Property(param, "Id");
            var rightExp = Expression.Constant("whatevs", typeof(string));
            var equalityExp = Expression.Equal(leftExp, rightExp);
            var lambdaExp = Expression.Lambda<Func<Course, bool>>(equalityExp, param);

            var rule = new Rule<Course>("ID", lambdaExp);
            group.SubRequirement.AcademicCreditRules.Add(new RequirementRule(rule));
            var acadResult = new CourseResult(new PlannedCredit(biol100.Course, termId));
            var groupResult = group.Evaluate(new List<AcadResult>() { acadResult }, new List<Override>(), equatedCourses);
            Assert.AreEqual(0, groupResult.CountApplied());
            Assert.AreEqual(Result.RuleFailed, acadResult.Result);
        }
    }
}
