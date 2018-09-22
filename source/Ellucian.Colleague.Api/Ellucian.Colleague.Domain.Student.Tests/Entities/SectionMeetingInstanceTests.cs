// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class SectionMeetingInstanceTests
    {
        [TestClass]
        public class SectionMeetingInstance_Constructor
        {
            string id;
            string sectionId;
            SectionMeetingInstance sm;
            string instMethod;
            DateTime meetingDate = new System.DateTime(2017, 9, 1);
            DateTimeOffset? startTime = new DateTimeOffset(2018, 8, 15, 13, 15, 00, new TimeSpan(4, 0, 0));
            DateTimeOffset? endTime = new DateTimeOffset(2018, 8, 15, 15, 15, 00, new TimeSpan(4, 0, 0));
            DateTimeOffset? defaultDate = default(DateTimeOffset);

            [TestInitialize]
            public void SectionMeeting_Initialize()
            {
                id = "1111";
                sectionId = "987";
                instMethod = "LEC";
                sm = new SectionMeetingInstance(id, sectionId, meetingDate, startTime, endTime) { InstructionalMethod = instMethod };
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SectionMeetingInstance_NoSectionId_NullException()
            {
                new SectionMeetingInstance(id, null, meetingDate, startTime, endTime);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SectionMeetingInstance_EmptySectionId_NullException()
            {
                new SectionMeetingInstance(id, string.Empty, meetingDate, startTime, endTime);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SectionMeetingInstance_MinDateMeetingDate_NullException()
            {
                new SectionMeetingInstance(id, sectionId, DateTime.MinValue, startTime, endTime);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SectionMeetingInstance_DefaultStartTime_NullException()
            {
                new SectionMeetingInstance(id, sectionId, meetingDate, defaultDate, endTime);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SectionMeetingInstance_DefaultEndTime_NullException()
            {
                new SectionMeetingInstance(id, sectionId, meetingDate, startTime, defaultDate);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void SectionMeetingInstance_EndBeforeStart_Exception()
            {
                new SectionMeetingInstance(id, sectionId, meetingDate, endTime, startTime);
            }

            [TestMethod]
            public void SectionMeetingInstance_Id()
            {
                Assert.AreEqual(id, sm.Id);
            }

            [TestMethod]
            public void SectionMeetingInstance_SectionId()
            {
                Assert.AreEqual(sectionId, sm.SectionId);
            }

            [TestMethod]
            public void SectionMeetingInstance_InstrMethod()
            {
                Assert.AreEqual(instMethod, sm.InstructionalMethod);
            }

            [TestMethod]
            public void SectionMeetingInstance_MeetingDate()
            {
                Assert.AreEqual(meetingDate, sm.MeetingDate);
            }

            [TestMethod]
            public void SectionMeetingInstance_StartTime()
            {
                Assert.AreEqual(startTime.Value.TimeOfDay, sm.StartTime.Value.TimeOfDay);
            }

            [TestMethod]
            public void SectionMeetingInstance_EndTime()
            {
                Assert.AreEqual(endTime.Value.TimeOfDay, sm.EndTime.Value.TimeOfDay);
            }

            [TestMethod]
            public void SectionMeetingInstance_NullTimes()
            {

                var testSm = new SectionMeetingInstance(id, sectionId, meetingDate, null, null);
                Assert.IsFalse(testSm.StartTime.HasValue);
                Assert.IsFalse(testSm.EndTime.HasValue);

            }
        }

        [TestClass]
        public class SectionMeetingInstance_EqualsOverride
        {
            string id;
            string sectionId;
            SectionMeetingInstance sm;
            string instMethod;
            DateTime meetingDate = new DateTime(2017, 9, 1);
            DateTimeOffset? startTime = new DateTimeOffset(2018, 8, 15, 13, 15, 00, new TimeSpan(4, 0, 0));
            DateTimeOffset? endTime = new DateTimeOffset(2018, 8, 15, 15, 15, 00, new TimeSpan(4, 0, 0));

            [TestInitialize]
            public void SectionMeetingInstance_Initialize()
            {
                id = "1111";
                sectionId = "987";
                instMethod = "LEC";
                sm = new SectionMeetingInstance(id, sectionId, meetingDate, startTime, endTime) { InstructionalMethod = instMethod };
            }
            [TestCleanup]
            public void SectionMeetingInstance_Cleanup()
            {
                sm = null;

            }

            [TestMethod]
            public void SectionMeeting_Equals_InvalidObject_ReturnsFalse()
            {
                SectionMeeting testSm = new SectionMeeting(id, sectionId, instMethod, new DateTime(2018, 8, 15, 13, 15, 00), new DateTime(2018, 8, 15, 15, 15, 00), string.Empty);
                Assert.IsFalse(sm.Equals(testSm));
            }

            [TestMethod]
            public void SectionMeeting_Equals_SectionMeetingInstanceDifferentID_ReturnsFalse()
            {
                var testSm = new SectionMeetingInstance(id = "111", sectionId, meetingDate, startTime, endTime) { InstructionalMethod = instMethod };
                Assert.IsFalse(sm.Equals(testSm));
            }

            [TestMethod]
            public void SectionMeeting_Equals_SectionMeetingInstanceSameID_ReturnsTrue()
            {
                var testSm = new SectionMeetingInstance(id = "1111", sectionId, meetingDate, startTime, endTime) { InstructionalMethod = instMethod };
                Assert.IsTrue(sm.Equals(testSm));
            }
        }

        [TestClass]
        public class SectionMeetingInstance_BelongsToStudentAttendance
        {
            string id;
            string sectionId;
            SectionMeetingInstance sm;
            string instMethod;
            DateTime meetingDate = new DateTime(2017, 9, 1);
            DateTimeOffset? startTime = new DateTimeOffset(2018, 8, 15, 13, 15, 00, new TimeSpan(4, 0, 0));
            DateTimeOffset? endTime = new DateTimeOffset(2018, 8, 15, 15, 15, 00, new TimeSpan(4, 0, 0));

            [TestInitialize]
            public void SectionMeetingInstance_Initialize()
            {
                id = "1111";
                sectionId = "987";
                instMethod = "LEC";
                sm = new SectionMeetingInstance(id, sectionId, meetingDate, startTime, endTime) { InstructionalMethod = instMethod };
            }
            [TestCleanup]
            public void SectionMeetingInstance_Cleanup()
            {
                sm = null;

            }

            [TestMethod]
            public void SectionMeeting_BelongsToStudentAttendance_DifferentMeetingDate_ReturnsFalse()
            {
                var testSm = new StudentAttendance("studentId", sectionId, new DateTime(2017, 9, 2), "A", null) { InstructionalMethod = instMethod, StartTime = startTime, EndTime = endTime };
                Assert.IsFalse(sm.BelongsToStudentAttendance(testSm));
            }

            [TestMethod]
            public void SectionMeeting_BelongsToStudentAttendance_NullStartTime_ReturnsFalse()
            {
                var testSm = new StudentAttendance("studentId", sectionId, meetingDate, "A", null) { InstructionalMethod = instMethod, EndTime = endTime };
                Assert.IsFalse(sm.BelongsToStudentAttendance(testSm));
            }
            [TestMethod]
            public void SectionMeeting_BelongsToStudentAttendance_DifferentStartTime_ReturnsFalse()
            {
                var testSm = new StudentAttendance("studentId", sectionId, meetingDate, "A", null) { InstructionalMethod = instMethod, StartTime = new DateTimeOffset(2018, 8, 16, 13, 15, 00, new TimeSpan(4, 0, 0)), EndTime = endTime };
                Assert.IsFalse(sm.BelongsToStudentAttendance(testSm));
            }

            [TestMethod]
            public void SectionMeeting_BelongsToStudentAttendance_DifferentEndTime_ReturnsFalse()
            {
                var testSm = new StudentAttendance("studentId", sectionId, meetingDate, "A", null) { InstructionalMethod = instMethod, StartTime = startTime, EndTime = new DateTimeOffset(2018, 8, 15, 14, 15, 00, new TimeSpan(4, 0, 0)) };
                Assert.IsFalse(sm.BelongsToStudentAttendance(testSm));
            }

            [TestMethod]
            public void SectionMeeting_BelongsToStudentAttendance_NullEndTime_ReturnsFalse()
            {
                var testSm = new StudentAttendance("studentId", sectionId, meetingDate, "A", null) { InstructionalMethod = instMethod, StartTime = startTime };
                Assert.IsFalse(sm.BelongsToStudentAttendance(testSm));
            }

            [TestMethod]
            public void SectionMeeting_BelongsToStudentAttendance_InstructionMethod_Different_False()
            {
                var sm1 = new SectionMeetingInstance(id, sectionId, meetingDate, startTime, endTime) { InstructionalMethod = instMethod };
                var sm2 = new StudentAttendance("studentId", sectionId, meetingDate, "A", null) { InstructionalMethod = "XYZ", StartTime = startTime, EndTime = endTime };
                Assert.IsFalse(sm1.BelongsToStudentAttendance(sm2));
            }

            [TestMethod]
            public void SectionMeeting_BelongsToStudentAttendance_InstructionMethod_EmptyToNull_True()
            {
                var sm1 = new SectionMeetingInstance(id, sectionId, meetingDate, startTime, endTime);
                var sm2 = new StudentAttendance("studentId", sectionId, meetingDate, "A", null) { InstructionalMethod = string.Empty, StartTime = startTime, EndTime = endTime };
                Assert.IsTrue(sm1.BelongsToStudentAttendance(sm2));
            }

            [TestMethod]
            public void SectionMeeting_BelongsToStudentAttendance_InstructionMethod_NullToEmpty_True()
            {
                var sm1 = new SectionMeetingInstance(id, sectionId, meetingDate, startTime, endTime) { InstructionalMethod = string.Empty };
                var sm2 = new StudentAttendance("studentId", sectionId, meetingDate, "A", null) { StartTime = startTime, EndTime = endTime };
                Assert.IsTrue(sm1.BelongsToStudentAttendance(sm2));
            }

            [TestMethod]
            public void SectionMeeting_BelongsToStudentAttendance_InstructionMethod_BothNull_True()
            {
                var sm1 = new SectionMeetingInstance(id, sectionId, meetingDate, startTime, endTime);
                var sm2 = new StudentAttendance("studentId", sectionId, meetingDate, "A", null) { StartTime = startTime, EndTime = endTime };
                Assert.IsTrue(sm1.BelongsToStudentAttendance(sm2));
            }

            [TestMethod]
            public void SectionMeeting_BelongsToStudentAttendance_Same_True()
            {
                var sm1 = new SectionMeetingInstance(id, sectionId, meetingDate, startTime, endTime) { InstructionalMethod = instMethod };
                var sm2 = new StudentAttendance("studentId", sectionId, meetingDate, "A", null) { InstructionalMethod = instMethod, StartTime = startTime, EndTime = endTime };
                Assert.IsTrue(sm1.BelongsToStudentAttendance(sm2));
            }
        }
    }

}
