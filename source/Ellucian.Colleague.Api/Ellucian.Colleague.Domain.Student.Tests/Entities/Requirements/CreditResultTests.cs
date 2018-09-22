// Copyright 2015-2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities.Requirements
{
    [TestClass]
    public class CreditResultTests
    {
        CreditResult creditResult;
        AcademicCredit acadCredit;
        Course course;

        [TestInitialize]
        public void Initialize()
        {
            course = new TestCourseRepository().Biol100;
            acadCredit = new AcademicCredit("3", course, "123");
            acadCredit.TermCode = "testTerm";
            acadCredit.SubjectCode = "testSubject";
            acadCredit.AddDepartment("DEPT1");
            acadCredit.AddDepartment("DEPT2");
            acadCredit.CourseLevelCode = "100";
            acadCredit.VerifiedGrade = new Grade("B", "Very Good", "UG");
            acadCredit.Type = CreditType.Institutional;
            acadCredit.CompletedCredit = 3m;
            acadCredit.AdjustedGradePoints = 6m;
            acadCredit.AdjustedGpaCredit = 5m;
            acadCredit.AdjustedCredit = 4m;
            acadCredit.Credit = 3m;
            acadCredit.CourseName = "CRS_100";
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowsExceptionForNullCredit()
        {
            creditResult = new CreditResult(null);
        }

        [TestMethod]
        public void GetAcadCred_ReturnsAcademicCredit()
        {
            creditResult = new CreditResult(acadCredit);
            Assert.AreEqual(acadCredit, creditResult.GetAcadCred());
        }

        [TestMethod]
        public void GetSubject_ReturnsAcadCreditSubject()
        {
            creditResult = new CreditResult(acadCredit);
            Assert.IsTrue(!string.IsNullOrEmpty(acadCredit.SubjectCode));
            Assert.AreEqual(acadCredit.SubjectCode, creditResult.GetSubject());
        }

        [TestMethod]
        public void GetDepartmentCodes_ReturnsAcadCreditDepartmentCodes()
        {
            creditResult = new CreditResult(acadCredit);
            Assert.IsNotNull(acadCredit.DepartmentCodes);
            Assert.AreEqual(acadCredit.DepartmentCodes, creditResult.GetDepartments());            
        }

        [TestMethod]
        public void GetCourseLevels_ReturnsAcadCreditCourseLevels()
        {
            creditResult = new CreditResult(acadCredit);
            Assert.IsTrue(!string.IsNullOrEmpty(acadCredit.CourseLevelCode));
            Assert.AreEqual(1, creditResult.GetCourseLevels().Count());
            Assert.AreEqual(acadCredit.CourseLevelCode, creditResult.GetCourseLevels().ElementAt(0));
        }

        [TestMethod]
        public void GetGrade_ReturnsAcadCreditVerifiedGrade()
        {
            creditResult = new CreditResult(acadCredit);
            Assert.IsNotNull(acadCredit.VerifiedGrade);
            Assert.AreEqual(acadCredit.VerifiedGrade, creditResult.GetGrade());
        }

        [TestMethod]
        public void GetCourse_ReturnsAcadCreditCourse()
        {
            creditResult = new CreditResult(acadCredit);
            Assert.IsNotNull(acadCredit.Course);
            Assert.AreEqual(acadCredit.Course, creditResult.GetCourse());
        }

        [TestMethod]
        public void IsInstitutional_ReturnsAcadCreditIsInstitutional()
        {
            creditResult = new CreditResult(acadCredit);
            Assert.IsTrue(acadCredit.IsInstitutional());
            Assert.AreEqual(acadCredit.IsInstitutional(), creditResult.IsInstitutional());
        }

        [TestMethod]
        public void GetCredits_ReturnsAcadCreditCredit()
        {
            creditResult = new CreditResult(acadCredit);
            Assert.IsNotNull(acadCredit);
            Assert.AreEqual(acadCredit, creditResult.GetAcadCred());
        }

        [TestMethod]
        public void GetAdjustedCredits_Returns_zero_when_AcadCredit_is_complete_and_AdjustedCredit_is_null()
        {
            acadCredit.AdjustedCredit = null;
            creditResult = new CreditResult(acadCredit);
            Assert.IsNull(acadCredit.AdjustedCredit);
            Assert.AreEqual(0m, creditResult.GetAdjustedCredits());
        }

        [TestMethod]
        public void GetAdjustedCredits_Returns_AcadCredit_AdjustedCredit_when_AcadCredit_is_complete_and_AdjustedCredit_is_not_null()
        {
            creditResult = new CreditResult(acadCredit);
            Assert.IsNotNull(acadCredit.AdjustedCredit);
            Assert.AreEqual(acadCredit.AdjustedCredit, creditResult.GetAdjustedCredits());
        }

        [TestMethod]
        public void GetAdjustedCredits_Returns_AcadCredit_Credit_when_AcadCredit_is_incomplete()
        {
            acadCredit.VerifiedGrade = null;
            creditResult = new CreditResult(acadCredit);
            Assert.AreEqual(acadCredit.Credit, creditResult.GetAdjustedCredits());
        }

        [TestMethod]
        public void GetCompletedCredits_ReturnsAcadCreditCompletedCredit()
        {
            creditResult = new CreditResult(acadCredit);
            Assert.IsNotNull(acadCredit.CompletedCredit);
            Assert.AreEqual(acadCredit.CompletedCredit, creditResult.GetCompletedCredits());
        }

        [TestMethod]
        public void GetGradePoints_ReturnsAcadCreditAdjustedGradePoints()
        {
            creditResult = new CreditResult(acadCredit);
            Assert.IsNotNull(acadCredit.AdjustedGradePoints);
            Assert.AreEqual(acadCredit.AdjustedGradePoints, creditResult.GetGradePoints());
        }

        [TestMethod]
        public void GetGpaCredit_ReturnsAcadCreditAdjustedGpaCredit()
        {
            creditResult = new CreditResult(acadCredit);
            Assert.IsNotNull(acadCredit.AdjustedGpaCredit);
            Assert.AreEqual(acadCredit.AdjustedGpaCredit, creditResult.GetGpaCredit());
        }

        [TestMethod]
        public void ToString_ReturnsCourseName()
        {
            creditResult = new CreditResult(acadCredit);
            Assert.IsTrue(!string.IsNullOrEmpty(acadCredit.CourseName));
            Assert.AreEqual("AcadCred " + acadCredit.CourseName, creditResult.ToString());
        }

        [TestMethod]
        public void GetAcadCredId_ReturnsAcadCreditId()
        {
            creditResult = new CreditResult(acadCredit);
            Assert.AreEqual(acadCredit.Id, creditResult.GetAcadCredId());
        }

        [TestMethod]
        public void GetTermCode_ReturnsAcadCreditTermCode()
        {
            creditResult = new CreditResult(acadCredit);
            Assert.IsTrue(!string.IsNullOrEmpty(acadCredit.TermCode));
            Assert.AreEqual(acadCredit.TermCode, creditResult.GetTermCode());
        }

        [TestMethod]
        public void GetSectionId_ReturnsSectionId()
        {
            creditResult = new CreditResult(acadCredit);
            Assert.IsTrue(!string.IsNullOrEmpty(acadCredit.SectionId));
            Assert.AreEqual(acadCredit.SectionId, creditResult.GetSectionId());
        }

        [TestMethod]
        public void Equals_FalseIfCourseResultComparedAgainstAcadCred()
        {
            creditResult = new CreditResult(acadCredit);
            CourseResult crsResult = new CourseResult(new PlannedCredit(course, "2015/FA", "123"));
            Assert.IsFalse(creditResult.Equals(crsResult));
        }

        [TestMethod]
        public void Equals_FalseIfAcadCredIdsNotEqual()
        {
            creditResult = new CreditResult(acadCredit);
            var acadCredit2 = new AcademicCredit("4", course, "123");
            var creditResult2 = new CreditResult(acadCredit2);
            Assert.IsFalse(creditResult.Equals(creditResult2));
        }

        [TestMethod]
        public void Equals_TrueIfAcadCredIdsEqual()
        {
            creditResult = new CreditResult(acadCredit);
            var acadCredit2 = new AcademicCredit("3", course, "456");
            var creditResult2 = new CreditResult(acadCredit2);
            Assert.IsTrue(creditResult.Equals(creditResult2));            
        }
    }
}
