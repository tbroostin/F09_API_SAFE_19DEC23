// Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class SectionAttendanceResponseTests
    {
        [TestClass]
        public class SectionAttendanceResponse_Constructor
        {
            private string sectionId;
            private DateTime meetingDate;
            private DateTimeOffset? startTime;
            private DateTimeOffset? endTime;
            private TimeSpan offset;
            private SectionAttendanceResponse sectionAttendance;
            private SectionMeetingInstance meetingInstance;

            [TestInitialize]
            public void Initialize()
            {
                sectionId = "111";
                offset = new TimeSpan(1, 0, 0);
                meetingDate = DateTime.Today.AddDays(-10);
                startTime = DateTimeOffset.Now.AddDays(-10).AddHours(-1);
                endTime = DateTimeOffset.Now.AddDays(-10);
                meetingInstance = new SectionMeetingInstance("4444", sectionId, meetingDate, startTime, endTime) { InstructionalMethod = "IN" };

            }

            [TestCleanup]
            public void CleanUp()
            {
                sectionAttendance = null;
                meetingInstance = null;
            }


            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SectionAttendanceResponse_ThrowArgumentNullException_NullSectionId()
            {
                sectionAttendance = new SectionAttendanceResponse(null, meetingInstance);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SectionAttendanceResponse_ThrowArgumentNullException_EmptySectionId()
            {
                sectionAttendance = new SectionAttendanceResponse(string.Empty, meetingInstance);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SectionAttendanceResponse_ThrowArgumentNullException_NullMeetingInstance()
            {
                sectionAttendance = new SectionAttendanceResponse(sectionId, null);
            }


            [TestMethod]
            public void SectionAttendanceResponse_ValidProperties()
            {
                sectionAttendance = new SectionAttendanceResponse(sectionId, meetingInstance);

                Assert.AreEqual(sectionId, sectionAttendance.SectionId);
                Assert.AreEqual(meetingDate, sectionAttendance.MeetingInstance.MeetingDate);
                Assert.AreEqual(startTime, sectionAttendance.MeetingInstance.StartTime);
                Assert.AreEqual(endTime, sectionAttendance.MeetingInstance.EndTime);
                Assert.AreEqual(sectionId, sectionAttendance.MeetingInstance.SectionId);
                Assert.AreEqual("IN", sectionAttendance.MeetingInstance.InstructionalMethod);
                CollectionAssert.AreEqual(new List<StudentAttendance>(), sectionAttendance.UpdatedStudentAttendances);
                CollectionAssert.AreEqual(new List<StudentSectionAttendanceError>(), sectionAttendance.StudentAttendanceErrors);
                CollectionAssert.AreEqual(new List<string>(), sectionAttendance.StudentCourseSectionsWithDeletedAttendances);
            }
        }

        [TestClass]
        public class SectionAttendanceResponse_AddUpdatedStudentAttendance
        {
            private string sectionId;
            private string studentId;
            private DateTime meetingDate;
            private DateTimeOffset? startTime;
            private DateTimeOffset? endTime;
            private TimeSpan offset;
            private string studentCourseSecId;
            private string attendanceCode;
            private string comment;
            private SectionAttendanceResponse sectionAttendance;
            private SectionMeetingInstance meetingInstance;
            private StudentAttendance sa;

            [TestInitialize]
            public void Initialize()
            {
                sectionId = "111";
                studentId = "studentId";
                studentCourseSecId = "1234";
                offset = new TimeSpan(1, 0, 0);
                meetingDate = DateTime.Today.AddDays(-10);
                startTime = DateTimeOffset.Now.AddDays(-10).AddHours(-1);
                endTime = DateTimeOffset.Now.AddDays(-10);
                attendanceCode = "L";
                comment = "Whatever";
                meetingInstance = new SectionMeetingInstance("4444", sectionId, meetingDate, startTime, endTime) { InstructionalMethod = "IN" };
                sectionAttendance = new SectionAttendanceResponse(sectionId, meetingInstance);
                sa = new StudentAttendance(studentId, sectionId, meetingDate, attendanceCode, null);
            }

            [TestCleanup]
            public void CleanUp()
            {
                sa = null;
                sectionAttendance = null;
                meetingInstance = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AddUpdatedStudentAttendance_ThrowArgumentNullException_NullStudentAttendance()
            {
                sectionAttendance.AddUpdatedStudentAttendance(null);
            }

            [TestMethod]
            public void AddUpdatedStudentAttendance_AddNewStudentAttendance()
            {
                Assert.AreEqual(0, sectionAttendance.UpdatedStudentAttendances.Count);
                sectionAttendance.AddUpdatedStudentAttendance(sa);
                Assert.AreEqual(1, sectionAttendance.UpdatedStudentAttendances.Count);
            }
        }

        [TestClass]
        public class SectionAttendanceResponse_AddStudentAttendanceError
        {
            private string sectionId;
            private string studentId;
            private DateTime meetingDate;
            private DateTimeOffset? startTime;
            private DateTimeOffset? endTime;
            private TimeSpan offset;
            private string studentCourseSecId;
            private string attendanceCode;
            private string comment;
            private SectionAttendanceResponse sectionAttendance;
            private SectionMeetingInstance meetingInstance;
            private StudentSectionAttendance ssa;
            private StudentSectionAttendanceError ssae;

            [TestInitialize]
            public void Initialize()
            {
                sectionId = "111";
                studentCourseSecId = "1234";
                offset = new TimeSpan(1, 0, 0);
                meetingDate = DateTime.Today.AddDays(-10);
                startTime = DateTimeOffset.Now.AddDays(-10).AddHours(-1);
                endTime = DateTimeOffset.Now.AddDays(-10);
                meetingInstance = new SectionMeetingInstance("4444", sectionId, meetingDate, startTime, endTime) { InstructionalMethod = "IN" };
                sectionAttendance = new SectionAttendanceResponse(sectionId, meetingInstance);
                ssae = new StudentSectionAttendanceError(studentCourseSecId);
            }

            [TestCleanup]
            public void CleanUp()
            {
                ssa = null;
                sectionAttendance = null;
                meetingInstance = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AddStudentAttendanceError_ThrowArgumentNullException_NullStudentSectionAttendanceError()
            {
                sectionAttendance.AddStudentAttendanceError(null);
            }

            [TestMethod]
            public void AddStudentAttendanceError_AddNewStudentSectionAttendanceError()
            {
                Assert.AreEqual(0, sectionAttendance.StudentAttendanceErrors.Count);
                sectionAttendance.AddStudentAttendanceError(ssae);
                Assert.AreEqual(1, sectionAttendance.StudentAttendanceErrors.Count);
            }
        }

        [TestClass]
        public class SectionAttendanceResponse_AddStudentCourseSectionsWithDeletedAttendance
        {
            private string sectionId;
            private string studentId;
            private DateTime meetingDate;
            private DateTimeOffset? startTime;
            private DateTimeOffset? endTime;
            private TimeSpan offset;
            private string studentCourseSecId;
            private string attendanceCode;
            private string comment;
            private SectionAttendanceResponse sectionAttendance;
            private SectionMeetingInstance meetingInstance;
            private StudentSectionAttendance ssa;
            private string scswda;

            [TestInitialize]
            public void Initialize()
            {
                sectionId = "111";
                studentCourseSecId = "1234";
                offset = new TimeSpan(1, 0, 0);
                meetingDate = DateTime.Today.AddDays(-10);
                startTime = DateTimeOffset.Now.AddDays(-10).AddHours(-1);
                endTime = DateTimeOffset.Now.AddDays(-10);
                meetingInstance = new SectionMeetingInstance("4444", sectionId, meetingDate, startTime, endTime) { InstructionalMethod = "IN" };
                sectionAttendance = new SectionAttendanceResponse(sectionId, meetingInstance);
                scswda = studentCourseSecId;
            }

            [TestCleanup]
            public void CleanUp()
            {
                ssa = null;
                sectionAttendance = null;
                meetingInstance = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AddStudentCourseSectionsWithDeletedAttendance_ThrowArgumentNullException_NullStudentSectionAttendanceError()
            {
                sectionAttendance.AddStudentCourseSectionsWithDeletedAttendance(null);
            }

            [TestMethod]
            public void AddStudentCourseSectionsWithDeletedAttendance_AddNewStudentSectionAttendanceError()
            {
                Assert.AreEqual(0, sectionAttendance.StudentCourseSectionsWithDeletedAttendances.Count);
                sectionAttendance.AddStudentCourseSectionsWithDeletedAttendance(scswda);
                Assert.AreEqual(1, sectionAttendance.StudentCourseSectionsWithDeletedAttendances.Count);
            }
        }

    }
}