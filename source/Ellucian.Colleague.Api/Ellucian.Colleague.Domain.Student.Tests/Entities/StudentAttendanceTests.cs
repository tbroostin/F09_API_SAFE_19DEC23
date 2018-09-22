// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class StudentAttendanceTests
    {
        [TestClass]
        public class StudentAttendance_Constructor
        {
            private string studentId;
            private string sectionId;
            private string studentCourseSecId;
            private DateTime meetingDate;
            private DateTimeOffset? startTime;
            private DateTimeOffset? endTime;
            private TimeSpan offset;
            private string attendanceCategory;
            private int? minutesAttended;
            private StudentAttendance studentAttendance;

            [TestInitialize]
            public void Initialize()
            {
                studentId = "0001234";
                sectionId = "111";
                studentCourseSecId = "1234";
                offset = new TimeSpan(1, 0, 0);
                meetingDate = DateTime.Today.AddDays(-10);
                startTime = DateTimeOffset.Now.AddDays(-10).AddHours(-1);
                endTime = DateTimeOffset.Now.AddDays(-10);
                attendanceCategory = "L";
                minutesAttended = 60;
            }

            [TestCleanup]
            public void CleanUp()
            {
                studentAttendance = null;
            }


            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentAttendance_ThrowExceptionNullStudentId()
            {
                studentAttendance = new StudentAttendance(null, sectionId, meetingDate, attendanceCategory, minutesAttended);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentAttendance_ThrowExceptionEmptyStudentId()
            {
                studentAttendance = new StudentAttendance(string.Empty, sectionId, meetingDate, attendanceCategory, minutesAttended);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentAttendance_ThrowExceptionNullSectionId()
            {
                studentAttendance = new StudentAttendance(studentId, null, meetingDate, attendanceCategory, minutesAttended);
            }

            [TestMethod]
            public void StudentAttendance_Allows_blank_AttendanceCategoryCode_with_Comment()
            {
                studentAttendance = new StudentAttendance(studentId, sectionId, meetingDate, null, minutesAttended, "Comment");
                Assert.AreEqual("Comment", studentAttendance.Comment);
            }

            [TestMethod]
            public void StudentAttendance_Allows_null_MinutesAttended_with_Comment()
            {
                studentAttendance = new StudentAttendance(studentId, sectionId, meetingDate, null, null, "Comment");
                Assert.AreEqual("Comment", studentAttendance.Comment);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void StudentAttendance_ThrowExceptionMinValueDateCategory()
            {
                studentAttendance = new StudentAttendance(studentId, sectionId, DateTime.MinValue, attendanceCategory, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void StudentAttendance_ThrowExceptionAttendanceCategoryCodeAndMinutesAttended()
            {
                studentAttendance = new StudentAttendance(studentId, sectionId, meetingDate, attendanceCategory, minutesAttended);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void StudentAttendance_ThrowExceptionMinutesAttended_Less_than_Zero()
            {
                studentAttendance = new StudentAttendance(studentId, sectionId, meetingDate, null, -10);
            }

            [TestMethod]
            public void StudentAttendance_ValidProperties_AttendanceCategoryCode()
            {
                studentAttendance = new StudentAttendance(studentId, sectionId, meetingDate, attendanceCategory, null, "Comment");
                studentAttendance.InstructionalMethod = "LEC";
                studentAttendance.StudentCourseSectionId = studentCourseSecId;
                studentAttendance.StartTime = startTime;
                studentAttendance.EndTime = endTime;

                Assert.AreEqual(studentId, studentAttendance.StudentId);
                Assert.AreEqual(studentCourseSecId, studentAttendance.StudentCourseSectionId);
                Assert.AreEqual(attendanceCategory, studentAttendance.AttendanceCategoryCode);
                Assert.AreEqual(meetingDate, studentAttendance.MeetingDate);
                Assert.AreEqual(startTime, studentAttendance.StartTime);
                Assert.AreEqual(endTime, studentAttendance.EndTime);
                Assert.AreEqual(sectionId, studentAttendance.SectionId);
                Assert.AreEqual("LEC", studentAttendance.InstructionalMethod);
                Assert.AreEqual("Comment", studentAttendance.Comment);
            }

            [TestMethod]
            public void StudentAttendance_ValidProperties_MinutesAttended()
            {
                studentAttendance = new StudentAttendance(studentId, sectionId, meetingDate, null, minutesAttended, "Comment");
                studentAttendance.InstructionalMethod = "LEC";
                studentAttendance.StudentCourseSectionId = studentCourseSecId;
                studentAttendance.StartTime = startTime;
                studentAttendance.EndTime = endTime;

                Assert.AreEqual(studentId, studentAttendance.StudentId);
                Assert.AreEqual(studentCourseSecId, studentAttendance.StudentCourseSectionId);
                Assert.AreEqual(minutesAttended, studentAttendance.MinutesAttended);
                Assert.AreEqual(meetingDate, studentAttendance.MeetingDate);
                Assert.AreEqual(startTime, studentAttendance.StartTime);
                Assert.AreEqual(endTime, studentAttendance.EndTime);
                Assert.AreEqual(sectionId, studentAttendance.SectionId);
                Assert.AreEqual("LEC", studentAttendance.InstructionalMethod);
                Assert.AreEqual("Comment", studentAttendance.Comment);
            }
        }

        [TestClass]
        public class StudentAttendance_Equals
        {
            private string studentId;
            private string sectionId;
            private string studentCourseSecId;
            private DateTime meetingDate;
            private DateTimeOffset? startTime;
            private DateTimeOffset? endTime;
            private TimeSpan offset;
            private string attendanceCategory;
            private int? minutesAttended;
            private string instMethod;
            private StudentAttendance sa;

            [TestInitialize]
            public void Initialize()
            {
                studentId = "0001234";
                sectionId = "111";
                studentCourseSecId = "1234";
                offset = new TimeSpan(1, 0, 0);
                meetingDate = DateTime.Today.AddDays(-10);
                startTime = DateTimeOffset.Now.AddDays(-10).AddHours(-1);
                endTime = DateTimeOffset.Now.AddDays(-10);
                attendanceCategory = "L";
                instMethod = "LEC";
                minutesAttended = 60;
                sa = new StudentAttendance(studentId, sectionId, meetingDate, attendanceCategory, null) { StartTime = startTime, EndTime = endTime, InstructionalMethod = instMethod};
            }

            [TestCleanup]
            public void CleanUp()
            {
                sa = null;
            }

            [TestMethod]
            public void StudentAttendance_Equals_InvalidObject_ReturnsFalse()
            {
                SectionMeetingInstance testSm = new SectionMeetingInstance("1111", sectionId, meetingDate, startTime, endTime) { InstructionalMethod = instMethod };
                Assert.IsFalse(sa.Equals(testSm));
            }

            [TestMethod]
            public void StudentAttendance_Equals_StudentAttendanceDifferentStudentID_ReturnsFalse()
            {
                var testSa = new StudentAttendance("different", sectionId, meetingDate, attendanceCategory, null) { StartTime = startTime, EndTime = endTime, InstructionalMethod = instMethod };
                Assert.IsFalse(sa.Equals(testSa));
            }

            [TestMethod]
            public void StudentAttendance_Equals_StudentAttendanceDifferentSectionId_ReturnsFalse()
            {
                var testSa = new StudentAttendance(studentId, "different", meetingDate, attendanceCategory, null) { StartTime = startTime, EndTime = endTime, InstructionalMethod = instMethod };
                Assert.IsFalse(sa.Equals(testSa));
            }

            [TestMethod]
            public void StudentAttendance_Equals_StudentAttendanceDifferentMeetingDate_ReturnsFalse()
            {
                var testSa = new StudentAttendance(studentId, sectionId, DateTime.Today, attendanceCategory, null) { StartTime = startTime, EndTime = endTime, InstructionalMethod = instMethod };
                Assert.IsFalse(sa.Equals(testSa));
            }

            [TestMethod]
            public void StudentAttendance_Equals_StudentAttendanceNullStartTime_ReturnsFalse()
            {
                var testSa = new StudentAttendance(studentId, sectionId, meetingDate, attendanceCategory, null) { EndTime = endTime, InstructionalMethod = instMethod };
                Assert.IsFalse(sa.Equals(testSa));
            }
            [TestMethod]
            public void StudentAttendance_Equals_StudentAttendanceDifferentStartTime_ReturnsFalse()
            {
                var testSa = new StudentAttendance(studentId, sectionId, meetingDate, attendanceCategory, null) { InstructionalMethod = instMethod, StartTime = new DateTimeOffset(2018, 8, 16, 13, 15, 00, new TimeSpan(4, 0, 0)), EndTime = endTime };
                Assert.IsFalse(sa.Equals(testSa));
            }

            [TestMethod]
            public void StudentAttendance_Equals_StudentAttendanceDifferentEndTime_ReturnsFalse()
            {
                var testSa = new StudentAttendance(studentId, sectionId, meetingDate, attendanceCategory, null) { InstructionalMethod = instMethod, StartTime = startTime, EndTime = new DateTimeOffset(2018, 8, 15, 14, 15, 00, new TimeSpan(4, 0, 0)) };
                Assert.IsFalse(sa.Equals(testSa));
            }

            [TestMethod]
            public void StudentAttendance_Equals_StudentAttendanceNullEndTime_ReturnsFalse()
            {
                var testSa = new StudentAttendance(studentId, sectionId, meetingDate, attendanceCategory, null) { InstructionalMethod = instMethod, StartTime = startTime };
                Assert.IsFalse(sa.Equals(testSa));
            }

            [TestMethod]
            public void StudentAttendance_Equals_StudentAttendanceInstructionMethod_Different_False()
            {
                var sm1 = new StudentAttendance(studentId, sectionId, meetingDate, attendanceCategory, null) { StartTime = startTime, EndTime = endTime, InstructionalMethod = instMethod };
                var sm2 = new StudentAttendance(studentId, sectionId, meetingDate, attendanceCategory, null) { StartTime = startTime, EndTime = endTime, InstructionalMethod = "XXX" };
                Assert.IsFalse(sm1.Equals(sm2));
            }

            [TestMethod]
            public void StudentAttendance_Equals_StudentAttendanceInstructionMethod_EmptyToNull_True()
            {
                var sm1 = new StudentAttendance(studentId, sectionId, meetingDate, attendanceCategory, null) { StartTime = startTime, EndTime = endTime, InstructionalMethod = string.Empty };
                var sm2 = new StudentAttendance(studentId, sectionId, meetingDate, attendanceCategory, null) { StartTime = startTime, EndTime = endTime };
                Assert.IsTrue(sm1.Equals(sm2));
            }

            [TestMethod]
            public void StudentAttendance_Equals_StudentAttendanceInstructionMethod_NullToEmpty_True()
            {
                var sm1 = new StudentAttendance(studentId, sectionId, meetingDate, attendanceCategory, null) { StartTime = startTime, EndTime = endTime };
                var sm2 = new StudentAttendance(studentId, sectionId, meetingDate, attendanceCategory, null) { StartTime = startTime, EndTime = endTime, InstructionalMethod = string.Empty };
                Assert.IsTrue(sm1.Equals(sm2));
            }

            [TestMethod]
            public void StudentAttendance_Equals_StudentAttendanceInstructionMethod_BothNull_True()
            {
                var sm1 = new StudentAttendance(studentId, sectionId, meetingDate, attendanceCategory, null) { StartTime = startTime, EndTime = endTime };
                var sm2 = new StudentAttendance(studentId, sectionId, meetingDate, attendanceCategory, null) { StartTime = startTime, EndTime = endTime };
                Assert.IsTrue(sm1.Equals(sm2));
            }

            [TestMethod]
            public void StudentAttendance_Equals_StudentAttendanceSame_True()
            {
                var sm1 = new StudentAttendance(studentId, sectionId, meetingDate, attendanceCategory, null) { StartTime = startTime, EndTime = endTime, InstructionalMethod = instMethod };
                var sm2 = new StudentAttendance(studentId, sectionId, meetingDate, attendanceCategory, null) { StartTime = startTime, EndTime = endTime, InstructionalMethod = instMethod };
                Assert.IsTrue(sm1.Equals(sm2));
            }

            [TestMethod]
            public void StudentAttendance_Equals_MinutesAttended_different_False()
            {
                var sm1 = new StudentAttendance(studentId, sectionId, meetingDate, null, 10) { StartTime = startTime, EndTime = endTime, InstructionalMethod = instMethod };
                var sm2 = new StudentAttendance(studentId, sectionId, meetingDate, null, 20) { StartTime = startTime, EndTime = endTime, InstructionalMethod = instMethod };
                Assert.IsFalse(sm1.Equals(sm2));
            }

            [TestMethod]
            public void StudentAttendance_Equals_MinutesAttended_different_False_one_is_null()
            {
                var sm1 = new StudentAttendance(studentId, sectionId, meetingDate, attendanceCategory, null) { StartTime = startTime, EndTime = endTime, InstructionalMethod = instMethod };
                var sm2 = new StudentAttendance(studentId, sectionId, meetingDate, null, 20) { StartTime = startTime, EndTime = endTime, InstructionalMethod = instMethod };
                Assert.IsFalse(sm1.Equals(sm2));
            }

            [TestMethod]
            public void StudentAttendance_Equals_Null_AttendanceCategoryCodes_True()
            {
                var sm1 = new StudentAttendance(studentId, sectionId, meetingDate, null, null, "A") { StartTime = startTime, EndTime = endTime, InstructionalMethod = instMethod };
                var sm2 = new StudentAttendance(studentId, sectionId, meetingDate, null, null, "A") { StartTime = startTime, EndTime = endTime, InstructionalMethod = instMethod };
                Assert.IsTrue(sm1.Equals(sm2));
            }

            [TestMethod]
            public void StudentAttendance_Equals_Empty_AttendanceCategoryCodes_True()
            {
                var sm1 = new StudentAttendance(studentId, sectionId, meetingDate, string.Empty, null, "A") { StartTime = startTime, EndTime = endTime, InstructionalMethod = instMethod };
                var sm2 = new StudentAttendance(studentId, sectionId, meetingDate, string.Empty, null, "A") { StartTime = startTime, EndTime = endTime, InstructionalMethod = instMethod };
                Assert.IsTrue(sm1.Equals(sm2));
            }

            [TestMethod]
            public void StudentAttendance_Equals_Null_and_not_null_AttendanceCategoryCodes_False()
            {
                var sm1 = new StudentAttendance(studentId, sectionId, meetingDate, null, null, "A") { StartTime = startTime, EndTime = endTime, InstructionalMethod = instMethod };
                var sm2 = new StudentAttendance(studentId, sectionId, meetingDate, "P", null, "A") { StartTime = startTime, EndTime = endTime, InstructionalMethod = instMethod };
                Assert.IsFalse(sm1.Equals(sm2));
            }

            [TestMethod]
            public void StudentAttendance_Equals_not_empty_and_empty_AttendanceCategoryCodes_False()
            {
                var sm1 = new StudentAttendance(studentId, sectionId, meetingDate, "P", null, "A") { StartTime = startTime, EndTime = endTime, InstructionalMethod = instMethod };
                var sm2 = new StudentAttendance(studentId, sectionId, meetingDate, string.Empty, null, "A") { StartTime = startTime, EndTime = endTime, InstructionalMethod = instMethod };
                Assert.IsFalse(sm1.Equals(sm2));
            }
        }

        [TestClass]
        public class StudentAttendance_MinutesAttendedToDate
        {
            private string studentId;
            private string sectionId;
            private string studentCourseSecId;
            private DateTime meetingDate;
            private DateTimeOffset? startTime;
            private DateTimeOffset? endTime;
            private TimeSpan offset;
            private string attendanceCategory;
            private int? minutesAttended;
            private string instMethod;
            private StudentAttendance sa;

            [TestInitialize]
            public void Initialize()
            {
                studentId = "0001234";
                sectionId = "111";
                studentCourseSecId = "1234";
                offset = new TimeSpan(1, 0, 0);
                meetingDate = DateTime.Today.AddDays(-10);
                startTime = DateTimeOffset.Now.AddDays(-10).AddHours(-1);
                endTime = DateTimeOffset.Now.AddDays(-10);
                attendanceCategory = "L";
                instMethod = "LEC";
                minutesAttended = 60;
                sa = new StudentAttendance(studentId, sectionId, meetingDate, attendanceCategory, null) { StartTime = startTime, EndTime = endTime, InstructionalMethod = instMethod };
            }

            [TestMethod]
            public void StudentAttendance_Set_MinutesAttendedToDate_allows_null_when_AttendanceCategoryCode_set()
            {
                sa.MinutesAttendedToDate = null;
                Assert.IsNull(sa.MinutesAttendedToDate);
            }

            [ExpectedException(typeof(ApplicationException))]
            [TestMethod]
            public void StudentAttendance_Set_MinutesAttendedToDate_throws_ApplicationException_when_MinutesAttended_greater_than_value()
            {
                sa = new StudentAttendance(studentId, sectionId, meetingDate, null, minutesAttended) { StartTime = startTime, EndTime = endTime, InstructionalMethod = instMethod };
                sa.MinutesAttendedToDate = minutesAttended - 1;
            }

            [ExpectedException(typeof(ApplicationException))]
            [TestMethod]
            public void StudentAttendance_Set_MinutesAttendedToDate_throws_ApplicationException_when_value_greater_than_CumulativeMinutesAttended()
            {
                sa = new StudentAttendance(studentId, sectionId, meetingDate, null, minutesAttended) { StartTime = startTime, EndTime = endTime, InstructionalMethod = instMethod };
                sa.CumulativeMinutesAttended = minutesAttended + 1;
                sa.MinutesAttendedToDate = minutesAttended + 10;
            }

            [TestMethod]
            public void StudentAttendance_Set_MinutesAttendedToDate_valid()
            {
                sa = new StudentAttendance(studentId, sectionId, meetingDate, null, minutesAttended) { StartTime = startTime, EndTime = endTime, InstructionalMethod = instMethod };
                sa.CumulativeMinutesAttended = minutesAttended + 10;
                sa.MinutesAttendedToDate = minutesAttended + 1;
                Assert.AreEqual(minutesAttended + 1, sa.MinutesAttendedToDate);
            }
        }

        [TestClass]
        public class StudentAttendance_CumulativeMinutesAttended
        {
            private string studentId;
            private string sectionId;
            private string studentCourseSecId;
            private DateTime meetingDate;
            private DateTimeOffset? startTime;
            private DateTimeOffset? endTime;
            private TimeSpan offset;
            private string attendanceCategory;
            private int? minutesAttended;
            private string instMethod;
            private StudentAttendance sa;

            [TestInitialize]
            public void Initialize()
            {
                studentId = "0001234";
                sectionId = "111";
                studentCourseSecId = "1234";
                offset = new TimeSpan(1, 0, 0);
                meetingDate = DateTime.Today.AddDays(-10);
                startTime = DateTimeOffset.Now.AddDays(-10).AddHours(-1);
                endTime = DateTimeOffset.Now.AddDays(-10);
                attendanceCategory = "L";
                instMethod = "LEC";
                minutesAttended = 60;
                sa = new StudentAttendance(studentId, sectionId, meetingDate, attendanceCategory, null) { StartTime = startTime, EndTime = endTime, InstructionalMethod = instMethod };
            }


            [TestMethod]
            public void StudentAttendance_Set_CumulativeMinutesAttended_allows_null_when_AttendanceCategoryCode_set()
            {
                sa.CumulativeMinutesAttended = null;
                Assert.IsNull(sa.CumulativeMinutesAttended);
            }

            [ExpectedException(typeof(ApplicationException))]
            [TestMethod]
            public void StudentAttendance_Set_CumulativeMinutesAttended_throws_ApplicationException_when_value_less_than_MinutesAttended()
            {
                sa = new StudentAttendance(studentId, sectionId, meetingDate, null, minutesAttended) { StartTime = startTime, EndTime = endTime, InstructionalMethod = instMethod };
                sa.CumulativeMinutesAttended = minutesAttended - 1;
            }

            [ExpectedException(typeof(ApplicationException))]
            [TestMethod]
            public void StudentAttendance_Set_CumulativeMinutesAttended_throws_ApplicationException_when_value_less_than_MinutesAttendedToDate()
            {
                sa = new StudentAttendance(studentId, sectionId, meetingDate, null, minutesAttended) { StartTime = startTime, EndTime = endTime, InstructionalMethod = instMethod };
                sa.MinutesAttendedToDate = minutesAttended + 10;
                sa.CumulativeMinutesAttended = minutesAttended + 1;
            }

            [TestMethod]
            public void StudentAttendance_Set_CumulativeMinutesAttended_valid()
            {
                sa = new StudentAttendance(studentId, sectionId, meetingDate, null, minutesAttended) { StartTime = startTime, EndTime = endTime, InstructionalMethod = instMethod };
                sa.MinutesAttendedToDate = minutesAttended + 1;
                sa.CumulativeMinutesAttended = minutesAttended + 10;
                Assert.AreEqual(minutesAttended + 1, sa.MinutesAttendedToDate);
            }

        }
    }
}