// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities.Requirements
{
    [TestClass]
    public class CourseResultTests
    {

        Course course;
        PlannedCredit plannedCourse;
        CourseResult courseResult;

        [TestInitialize]
        public void Initialize()
        {
            course = new TestCourseRepository().Biol100;
            plannedCourse = new PlannedCredit(course, "2015/FA", "123");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowsExceptionForNullPlannedCourse()
        {
            courseResult = new CourseResult(null);
        }

        [TestMethod]
        public void GetAcadCred_ReturnsNull()
        {
            courseResult = new CourseResult(plannedCourse);
            Assert.IsNull(courseResult.GetAcadCred());
        }

        [TestMethod]
        public void GetSubject_ReturnsCourseSubjectCode()
        {
            courseResult = new CourseResult(plannedCourse);
            Assert.IsTrue(!string.IsNullOrEmpty(course.SubjectCode));
            Assert.AreEqual(course.SubjectCode, courseResult.GetSubject());
        }

        [TestMethod]
        public void GetDepartmentCodes_ReturnsCourseDepartmentCodes()
        {
            courseResult = new CourseResult(plannedCourse);
            Assert.IsNotNull(course.DepartmentCodes);
            for (int i = 0; i < course.DepartmentCodes.Count(); i++)
            {
                Assert.IsTrue(courseResult.GetDepartments().Contains(course.DepartmentCodes.ElementAt(i)));                            
            }
        }

        [TestMethod]
        public void GetCourseLevels_ReturnsCourseCourseLevels()
        {
            courseResult = new CourseResult(plannedCourse);
            Assert.IsTrue(course.CourseLevelCodes.Count() > 0);
            for (int i = 0; i < course.CourseLevelCodes.Count(); i++)
            {
                Assert.IsTrue(courseResult.GetCourseLevels().Contains(course.CourseLevelCodes.ElementAt(i)));                
            }
        }

        [TestMethod]
        public void GetGrade_ReturnsNull()
        {
            courseResult = new CourseResult(plannedCourse);
            Assert.IsNull(courseResult.GetGrade());
        }

        [TestMethod]
        public void GetCourse_ReturnsCourse()
        {
            courseResult = new CourseResult(plannedCourse);
            Assert.IsNotNull(course);
            Assert.AreEqual(course, courseResult.GetCourse());
        }

        [TestMethod]
        public void IsInstitutional_Returnsfalse()
        {
            courseResult = new CourseResult(plannedCourse);
            Assert.IsFalse(courseResult.IsInstitutional());
        }

        [TestMethod]
        public void GetCredits_ReturnsPlannedCourseCredits()
        {
            plannedCourse.Credits = 7m;
            courseResult = new CourseResult(plannedCourse);
            Assert.AreEqual(plannedCourse.Credits, courseResult.GetCredits());
        }

        [TestMethod]
        public void GetCredits_ReturnsMinCreditsIfNoPlannedCourse()
        {
            courseResult = new CourseResult(plannedCourse);
            Assert.IsNull(plannedCourse.Credits);
            Assert.IsNotNull(course.MinimumCredits);
            Assert.AreEqual(course.MinimumCredits, courseResult.GetCredits());
        }

        [TestMethod]
        public void GetCredits_ReturnsCeusIfNoMinCredits()
        {
            var course2 = new Course(course.Id, course.Title, course.LongTitle, course.Departments, course.SubjectCode, course.Number, course.AcademicLevelCode, course.CourseLevelCodes, null, 4m, course.CourseApprovals);
            var plannedCourse2 = new PlannedCredit(course2, "2015/FA", "321");
            var courseResult2 = new CourseResult(plannedCourse2);
            Assert.IsNull(course2.MinimumCredits);
            Assert.IsNotNull(course2.Ceus);
            Assert.AreEqual(course2.Ceus, courseResult2.GetCredits());
        }

        [TestMethod]
        public void GetAdjustedCredit_ReturnsZero()
        {
            courseResult = new CourseResult(plannedCourse);
            Assert.AreEqual(0, courseResult.GetAdjustedCredits());
        }


        [TestMethod]
        public void GetCompletedCredits_ReturnsZero()
        {
            courseResult = new CourseResult(plannedCourse);
            Assert.AreEqual(0, courseResult.GetCompletedCredits());
        }

        [TestMethod]
        public void GetGradePoints_ReturnsZero()
        {
            courseResult = new CourseResult(plannedCourse);
            Assert.AreEqual(0, courseResult.GetGradePoints());
        }

        [TestMethod]
        public void GetGpaCredit_ReturnsAcadCreditAdjustedGpaCredit()
        {
            courseResult = new CourseResult(plannedCourse);
            Assert.AreEqual(0, courseResult.GetGpaCredit());
        }

        [TestMethod]
        public void ToString_ReturnsCourseName()
        {
            courseResult = new CourseResult(plannedCourse);
            Assert.AreEqual("PlnCrs " + course.SubjectCode + "*" + course.Number, courseResult.ToString());
        }

        [TestMethod]
        public void GetTermCode_ReturnsPlannedCourseTermCode()
        {
            courseResult = new CourseResult(plannedCourse);
            Assert.AreEqual(plannedCourse.TermCode, courseResult.GetTermCode());
        }

        [TestMethod]
        public void GetSectionId_ReturnsPlannedCourseSectionId()
        {
            courseResult = new CourseResult(plannedCourse);
            Assert.AreEqual(plannedCourse.SectionId, courseResult.GetSectionId());
        }

        [TestMethod]
        public void Equals_FalseIfCourseResultComparedAgainstCreditResult()
        {
            courseResult = new CourseResult(plannedCourse);
            var creditResult = new CreditResult(new AcademicCredit("12", course, "123"));
            Assert.IsFalse(courseResult.Equals(creditResult));
        }

        [TestMethod]
        public void Equals_FalseIfCourseIdNotEqual()
        {
            courseResult = new CourseResult(plannedCourse);
            var course2 = new TestCourseRepository().Biol200;
            var plannedCourse2 = new PlannedCredit(course2, "2015/FA", "123");
            var courseResult2 = new CourseResult(plannedCourse2);
            Assert.IsFalse(courseResult.Equals(courseResult2));
        }

        [TestMethod]
        public void Equals_FalseIfTermCodeNotEqual()
        {
            courseResult = new CourseResult(plannedCourse);
            var plannedCourse2 = new PlannedCredit(course, "2014/FA", "123");
            var courseResult2 = new CourseResult(plannedCourse2);
            Assert.IsFalse(courseResult.Equals(courseResult2));
        }

        [TestMethod]
        public void Equals_FalseIfSectionNotEqual()
        {
            courseResult = new CourseResult(plannedCourse);
            var plannedCourse2 = new PlannedCredit(course, "2014/FA", "321");
            var courseResult2 = new CourseResult(plannedCourse2);
            Assert.IsFalse(courseResult.Equals(courseResult2));
        }

        [TestMethod]
        public void Equals_FalseIfOneSectionIdNull()
        {
            courseResult = new CourseResult(plannedCourse);
            var plannedCourse2 = new PlannedCredit(course, "2014/FA");
            var courseResult2 = new CourseResult(plannedCourse2);
            Assert.IsFalse(courseResult.Equals(courseResult2));
        }

        [TestMethod]
        public void Equals_FalseIfAllPartsEqual()
        {
            courseResult = new CourseResult(plannedCourse);
            var plannedCourse2 = new PlannedCredit(course, "2015/FA", "123");
            var courseResult2 = new CourseResult(plannedCourse2);
            Assert.IsFalse(courseResult.Equals(courseResult2));
        }

        [TestMethod]
        public void Equals_FalseIfAllPartsEqualWithNullSectionIds()
        {
            plannedCourse.SectionId = null;
            courseResult = new CourseResult(plannedCourse);
            var plannedCourse2 = new PlannedCredit(course, "2015/FA");
            var courseResult2 = new CourseResult(plannedCourse2);
            Assert.IsFalse(courseResult.Equals(courseResult2));
        }
    }
}
