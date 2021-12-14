// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Finance.Entities;
using Ellucian.Colleague.Domain.Finance.Entities.AccountActivity;
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Finance.Tests.Entities
{
    [TestClass]
    public class StudentStatementScheduleItemTests
    {
         Course course;
         AcademicCredit academicCredit;
         Section section;
         Section section2;
         Section section3;
         Section section4;
         SectionMeeting meeting1;
         SectionMeeting meeting2;
         SectionMeeting meeting3;
         SectionMeeting meeting4;
         SectionMeeting meeting5;

        StudentStatementScheduleItem scheduleItem;

        [TestInitialize]
        public void Initialize()
        {
            course = new Course("122", "Introduction to Art", null, new List<OfferingDepartment>() { new OfferingDepartment("ART", 100m) }, "ART", "100", "UG", new List<string>() { "Type" }, 3.0m, null, new List<CourseApproval>() { new CourseApproval("A", DateTime.Parse("12/10/2014"), "0000043", "0003315", DateTime.Parse("12/10/2014")) { Status = CourseStatus.Active } });
            academicCredit = new AcademicCredit("123", course, "124") 
            { 
                ContinuingEducationUnits = 1.5m,
                CourseName = "ART-100", 
                Credit = 3m,
                SectionNumber = "122",
                Title = "Introduction to Art"
            };
            section = new Section("124", "122", "01", DateTime.Parse("12/07/2014"), 3m, null, "Introduction to Art",
            "IN", new List<OfferingDepartment>() { new OfferingDepartment("ART", 100m) }, new List<string>() { "Type" }, "UG",
            new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Parse("12/07/2014")) })
            {
                EndDate = null,
                Location = "MC"
            };
            section2 = new Section("124", "122", "01", DateTime.Parse("12/07/2014"), 3m, null, "Introduction to Art",
            "IN", new List<OfferingDepartment>() { new OfferingDepartment("ART", 100m) }, new List<string>() { "Type" }, "UG",
            new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Parse("12/07/2014")) })
                {
                    EndDate = DateTime.Parse("1/13/2015"),
                    Location = null
                };
            section3 = new Section("124", "122", "01", DateTime.Parse("12/07/2014"), 3m, null, "Introduction to Art",
            "IN", new List<OfferingDepartment>() { new OfferingDepartment("ART", 100m) }, new List<string>() { "Type" }, "UG",
            new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Parse("12/07/2014")) })
                {
                    Location = string.Empty
                };
            section4 = new Section("124", "122", "01", DateTime.Parse("12/07/2014"), 3m, null, "Introduction to Art",
            "IN", new List<OfferingDepartment>() { new OfferingDepartment("ART", 100m) }, new List<string>() { "Type" }, "UG",
            new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Parse("12/07/2014")) });
            meeting1 = new SectionMeeting("125", "124", "LEC", DateTime.Parse("10/10/2014"), DateTime.Parse("02/10/2015"),
            "W")
            {
                Days = new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday },
                EndTime = DateTimeOffset.Parse("12:00 PM"),
                Room = "LBP*102",
                StartTime = DateTimeOffset.Parse("10:00 AM")
            };
            meeting2 = new SectionMeeting("126", "124", "LEC", null, DateTime.Parse("02/10/2015"),
            "W")
                {
                    Days = new List<DayOfWeek>() { DayOfWeek.Tuesday, DayOfWeek.Thursday },
                    EndTime = null,
                    Room = null,
                    StartTime = DateTimeOffset.Parse("8:00 AM")
                };
            meeting3 = new SectionMeeting("127", "124", "LEC", DateTime.Parse("10/10/2014"), null,
            "W")
            {
                Days = new List<DayOfWeek>() { DayOfWeek.Sunday, DayOfWeek.Saturday },
                EndTime = DateTimeOffset.Parse("3:00 PM"),
                Room = string.Empty,
                StartTime = null
            };
            meeting4 = new SectionMeeting("128", "124", "LEC", null, null,
            "W")
                {
                    Days = new List<DayOfWeek>(),
                    EndTime = null,
                    Room = string.Empty,
                    StartTime = null
                };
            meeting5 = new SectionMeeting("129", "124", "LEC", DateTime.Parse("10/10/2014"), DateTime.Parse("02/10/2015"),
            "W")
                {
                    Days = new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday },
                    EndTime = DateTimeOffset.Parse("2:00 PM"),
                    Room = "OKF*203",
                    StartTime = DateTimeOffset.Parse("12:00 PM")
                };

            scheduleItem = new StudentStatementScheduleItem(academicCredit, section);
        }

        [TestCleanup]
        public void Cleanup()
        {
            course = null;
            academicCredit = null;
            section = null;
            section2 = null;
            section3 = null;
            section4 = null;
            meeting1 = null;
            meeting2 = null;
            meeting3 = null;
            meeting4 = null;
            meeting5 = null;

            scheduleItem = null;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StudentStatementScheduleItem_Constructor_NullAcademicCredit()
        {
            scheduleItem = new StudentStatementScheduleItem(null, section);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StudentStatementScheduleItem_Constructor_NullSection()
        {
            scheduleItem = new StudentStatementScheduleItem(academicCredit, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void StudentStatementScheduleItem_Constructor_SectionIdMismatch()
        {
            section = new Section("ABC", "122", "01", DateTime.Parse("12/07/2014"), 3m, null, "Introduction to Art",
            "IN", new List<OfferingDepartment>() { new OfferingDepartment("ART", 100m) }, new List<string>() { "Type" }, "UG",
            new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Parse("12/07/2014")) });
            scheduleItem = new StudentStatementScheduleItem(academicCredit, section);
        }

        [TestMethod]
        [Ignore]
        public void StudentStatementScheduleItem_Constructor_Valid()
        {
            var sectionA = section;
            sectionA.AddSectionMeeting(meeting1);
            sectionA.AddSectionMeeting(meeting2);
            sectionA.AddSectionMeeting(meeting3);
            sectionA.AddSectionMeeting(meeting4);
            sectionA.AddSectionMeeting(meeting5);

            scheduleItem = new StudentStatementScheduleItem(academicCredit, sectionA);
            Assert.AreEqual(academicCredit.CourseName + "-" + academicCredit.SectionNumber, scheduleItem.SectionId);
            Assert.AreEqual(academicCredit.Title, scheduleItem.SectionTitle);
            Assert.AreEqual(academicCredit.Credit, scheduleItem.Credits);
            Assert.AreEqual(academicCredit.ContinuingEducationUnits, scheduleItem.ContinuingEducationUnits);
            Assert.AreEqual("MWF\r\nTuTh\r\nSaSu\r\nTBD\r\nMWF", scheduleItem.MeetingDays);
            Assert.AreEqual("MC LBP 102\r\nMC TBD\r\nMC TBD\r\nMC TBD\r\nMC OKF 203", scheduleItem.MeetingLocations);
            Assert.AreEqual("10:00 AM-12:00 PM\r\n8:00 AM-TBD\r\nTBD- 3:00 PM\r\nTBD\r\n12:00-2:00 PM", scheduleItem.MeetingTimes);
            var date = new DateTime(2014, 12, 7);
            Assert.AreEqual(date.ToShortDateString() + "-TBD", scheduleItem.SectionDates);
        }

        [TestMethod]
        [Ignore]
        public void StudentStatementScheduleItem_Constructor_Valid_SectionWithNullLocationAndEndDate()
        {
            section2.AddSectionMeeting(meeting1);
            section2.AddSectionMeeting(meeting2);
            section2.AddSectionMeeting(meeting3);
            section2.AddSectionMeeting(meeting4);
            section2.AddSectionMeeting(meeting5);

            scheduleItem = new StudentStatementScheduleItem(academicCredit, section2);
            Assert.AreEqual(academicCredit.CourseName + "-" + academicCredit.SectionNumber, scheduleItem.SectionId);
            Assert.AreEqual(academicCredit.Title, scheduleItem.SectionTitle);
            Assert.AreEqual(academicCredit.Credit, scheduleItem.Credits);
            Assert.AreEqual(academicCredit.ContinuingEducationUnits, scheduleItem.ContinuingEducationUnits);
            Assert.AreEqual("MWF\r\nTuTh\r\nSaSu\r\nTBD\r\nMWF", scheduleItem.MeetingDays);
            Assert.AreEqual("LBP 102\r\nTBD\r\nTBD\r\nTBD\r\nOKF 203", scheduleItem.MeetingLocations);
            Assert.AreEqual("10:00 AM-12:00 PM\r\n8:00 AM-TBD\r\nTBD- 3:00 PM\r\nTBD\r\n12:00-2:00 PM", scheduleItem.MeetingTimes);
            var date1 = new DateTime(2014, 12, 7);
            var date2 = new DateTime(2015, 1, 13);
            Assert.AreEqual(date1.ToShortDateString() + "-" + date2.ToShortDateString(), scheduleItem.SectionDates);
        }

        [TestMethod]
        [Ignore]
        public void StudentStatementScheduleItem_Constructor_Valid_SectionWithEmptyLocation()
        {
            section3.AddSectionMeeting(meeting1);
            section3.AddSectionMeeting(meeting2);
            section3.AddSectionMeeting(meeting3);
            section3.AddSectionMeeting(meeting4);
            section3.AddSectionMeeting(meeting5);

            scheduleItem = new StudentStatementScheduleItem(academicCredit, section3);
            Assert.AreEqual(academicCredit.CourseName + "-" + academicCredit.SectionNumber, scheduleItem.SectionId);
            Assert.AreEqual(academicCredit.Title, scheduleItem.SectionTitle);
            Assert.AreEqual(academicCredit.Credit, scheduleItem.Credits);
            Assert.AreEqual(academicCredit.ContinuingEducationUnits, scheduleItem.ContinuingEducationUnits);
            Assert.AreEqual("MWF\r\nTuTh\r\nSaSu\r\nTBD\r\nMWF", scheduleItem.MeetingDays);
            Assert.AreEqual("LBP 102\r\nTBD\r\nTBD\r\nTBD\r\nOKF 203", scheduleItem.MeetingLocations);
            Assert.AreEqual("10:00 AM-12:00 PM\r\n8:00 AM-TBD\r\nTBD- 3:00 PM\r\nTBD\r\n12:00-2:00 PM", scheduleItem.MeetingTimes);
            var date = new DateTime(2014, 12, 7);
            Assert.AreEqual(date.ToShortDateString() + "-TBD", scheduleItem.SectionDates);
        }

        [TestMethod]
        [Ignore]
        public void StudentStatementScheduleItem_Constructor_Valid_SectionWithNoMeetings()
        {
            scheduleItem = new StudentStatementScheduleItem(academicCredit, section4);
            Assert.AreEqual(academicCredit.CourseName + "-" + academicCredit.SectionNumber, scheduleItem.SectionId);
            Assert.AreEqual(academicCredit.Title, scheduleItem.SectionTitle);
            Assert.AreEqual(academicCredit.Credit, scheduleItem.Credits);
            Assert.AreEqual(academicCredit.ContinuingEducationUnits, scheduleItem.ContinuingEducationUnits);
            Assert.AreEqual("TBD", scheduleItem.MeetingDays);
            Assert.AreEqual("TBD", scheduleItem.MeetingLocations);
            Assert.AreEqual("TBD", scheduleItem.MeetingTimes);
            var date = new DateTime(2014, 12, 7);
            Assert.AreEqual(date.ToShortDateString() + "-TBD", scheduleItem.SectionDates);
        }

        [TestMethod]
        [Ignore]
        public void StudentStatementScheduleItem_Constructor_ValidSection_NullMeetings()
        {
            var sectionA = section;
            sectionA.AddSectionMeeting(meeting1);
            sectionA.AddSectionMeeting(meeting2);
            sectionA.AddSectionMeeting(meeting3);
            sectionA.AddSectionMeeting(meeting4);
            sectionA.AddSectionMeeting(meeting5);

            scheduleItem = new StudentStatementScheduleItem(academicCredit, sectionA);
            Assert.AreEqual(academicCredit.CourseName + "-" + academicCredit.SectionNumber, scheduleItem.SectionId);
            Assert.AreEqual(academicCredit.Title, scheduleItem.SectionTitle);
            Assert.AreEqual(academicCredit.Credit, scheduleItem.Credits);
            Assert.AreEqual(academicCredit.ContinuingEducationUnits, scheduleItem.ContinuingEducationUnits);
            Assert.AreEqual("MWF\r\nTuTh\r\nSaSu\r\nTBD\r\nMWF", scheduleItem.MeetingDays);
            Assert.AreEqual("MC LBP 102\r\nMC TBD\r\nMC TBD\r\nMC TBD\r\nMC OKF 203", scheduleItem.MeetingLocations);
            Assert.AreEqual("10:00 AM-12:00 PM\r\n8:00 AM-TBD\r\nTBD- 3:00 PM\r\nTBD\r\n12:00-2:00 PM", scheduleItem.MeetingTimes);
            var date = new DateTime(2014, 12, 7);
            Assert.AreEqual(date.ToShortDateString() + "-TBD", scheduleItem.SectionDates);
        }

    }
}
