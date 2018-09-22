// Copyright 2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class StudentSectionAttendanceTests
    {
        [TestClass]
        public class StudentSectionAttendance_Constructor
        {
            private string studentCourseSecId;
            private string attendanceCategory;
            private int? minutesAttended;
            private string comment;
            private StudentSectionAttendance studentSectionAttendance;

            [TestInitialize]
            public void Initialize()
            {
                studentCourseSecId = "1234";
                comment = "Always late.";
                attendanceCategory = "L";
                minutesAttended = 10;
            }

            [TestCleanup]
            public void CleanUp()
            {
                studentSectionAttendance = null;
            }


            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentSectionAttendance_ThrowArgumentNullException_StudentCourseSecIdNull()
            {
                studentSectionAttendance = new StudentSectionAttendance(null, attendanceCategory, null, comment);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentSectionAttendance_ThrowArgumentNullException_StudentCourseSecIdEmpty()
            {
                studentSectionAttendance = new StudentSectionAttendance(string.Empty, attendanceCategory, null, comment);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void StudentSectionAttendance_ThrowArgumentException_AttendanceCategoryAndMinutes()
            {
                studentSectionAttendance = new StudentSectionAttendance(studentCourseSecId, attendanceCategory, minutesAttended, null);
            }

            [TestMethod]
            public void StudentSectionAttendance_ValidProperties_Category()
            {
                studentSectionAttendance = new StudentSectionAttendance(studentCourseSecId, attendanceCategory, null, comment);

                Assert.AreEqual(studentCourseSecId, studentSectionAttendance.StudentCourseSectionId);
                Assert.AreEqual(attendanceCategory, studentSectionAttendance.AttendanceCategoryCode);
                Assert.AreEqual(comment, studentSectionAttendance.Comment);
            }

            [TestMethod]
            public void StudentSectionAttendance_ValidProperties_Minutes()
            {
                studentSectionAttendance = new StudentSectionAttendance(studentCourseSecId, null, minutesAttended, comment);

                Assert.AreEqual(studentCourseSecId, studentSectionAttendance.StudentCourseSectionId);
                Assert.IsNull(studentSectionAttendance.AttendanceCategoryCode);
                Assert.AreEqual(minutesAttended, studentSectionAttendance.MinutesAttended);
                Assert.AreEqual(comment, studentSectionAttendance.Comment);
            }

            [TestMethod]
            public void StudentSectionAttendance_ValidPropertiesOnlyCategory()
            {
                studentSectionAttendance = new StudentSectionAttendance(studentCourseSecId, attendanceCategory, null, null);

                Assert.AreEqual(studentCourseSecId, studentSectionAttendance.StudentCourseSectionId);
                Assert.AreEqual(attendanceCategory, studentSectionAttendance.AttendanceCategoryCode);
                Assert.IsNull(studentSectionAttendance.Comment);
                Assert.IsNull(studentSectionAttendance.MinutesAttended);
            }

            [TestMethod]
            public void StudentSectionAttendance_ValidPropertiesOnlyComment()
            {
                studentSectionAttendance = new StudentSectionAttendance(studentCourseSecId, null, null, comment);

                Assert.AreEqual(studentCourseSecId, studentSectionAttendance.StudentCourseSectionId);
                Assert.AreEqual(comment, studentSectionAttendance.Comment);
                Assert.IsNull(studentSectionAttendance.AttendanceCategoryCode);
                Assert.IsNull(studentSectionAttendance.MinutesAttended);
            }
        }

        
    }
}