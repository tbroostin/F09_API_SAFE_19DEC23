// Copyright 2018 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class SectionAttendanceTests
    {
        [TestClass]
        public class SectionAttendance_Constructor
        {
            private string sectionId;
            private DateTime meetingDate;
            private DateTimeOffset? startTime;
            private DateTimeOffset? endTime;
            private TimeSpan offset;
            private SectionAttendance sectionAttendance;
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
            public void SectionAttendance_ThrowArgumentNullException_NullSectionId()
            {
                sectionAttendance = new SectionAttendance(null, meetingInstance);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SectionAttendance_ThrowArgumentNullException_EmptySectionId()
            {
                sectionAttendance = new SectionAttendance(string.Empty, meetingInstance);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SectionAttendance_ThrowArgumentNullException_NullMeetingInstance()
            {
                sectionAttendance = new SectionAttendance(sectionId, null);
            }


            [TestMethod]
            public void SectionAttendance_ValidProperties()
            {
                sectionAttendance = new SectionAttendance(sectionId, meetingInstance);

                Assert.AreEqual(sectionId, sectionAttendance.SectionId);
                Assert.AreEqual(meetingDate, sectionAttendance.MeetingInstance.MeetingDate);
                Assert.AreEqual(startTime, sectionAttendance.MeetingInstance.StartTime);
                Assert.AreEqual(endTime, sectionAttendance.MeetingInstance.EndTime);
                Assert.AreEqual(sectionId, sectionAttendance.MeetingInstance.SectionId);
                Assert.AreEqual("IN", sectionAttendance.MeetingInstance.InstructionalMethod);

            }
        }

        [TestClass]
        public class SectionAttendance_AddStudentSectionAttendance
        {
            private string sectionId;
            private DateTime meetingDate;
            private DateTimeOffset? startTime;
            private DateTimeOffset? endTime;
            private TimeSpan offset;
            private string studentCourseSecId;
            private string attendanceCode;
            private string comment;
            private SectionAttendance sectionAttendance;
            private SectionMeetingInstance meetingInstance;
            private StudentSectionAttendance ssa;

            [TestInitialize]
            public void Initialize()
            {
                sectionId = "111";
                studentCourseSecId = "1234";
                offset = new TimeSpan(1, 0, 0);
                meetingDate = DateTime.Today.AddDays(-10);
                startTime = DateTimeOffset.Now.AddDays(-10).AddHours(-1);
                endTime = DateTimeOffset.Now.AddDays(-10);
                attendanceCode = "L";
                comment = "Whatever";
                meetingInstance = new SectionMeetingInstance("4444", sectionId, meetingDate, startTime, endTime) { InstructionalMethod = "IN" };
                sectionAttendance = new SectionAttendance(sectionId, meetingInstance);
                ssa = new StudentSectionAttendance(studentCourseSecId, attendanceCode, null, comment);
            }

            [TestCleanup]
            public void CleanUp()
            {
                ssa = null;
                sectionAttendance = null;
                meetingInstance = null;
            }

            [ExpectedException(typeof(ArgumentNullException))]
            public void AddStudentSectionAttendance_ThrowArgumentNullException_NullStudentSectionAttendance()
            {
                sectionAttendance.AddStudentSectionAttendance(null);
            }

            public void AddStudentSectionAttendance_AddNewStudentSectionAttendance()
            {
                sectionAttendance.AddStudentSectionAttendance(ssa);
                Assert.AreEqual(1, sectionAttendance.StudentAttendances.Count);
            }
        }
    }
}