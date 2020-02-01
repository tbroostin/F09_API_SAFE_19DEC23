// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class SectionTests
    {
        [TestClass]
        public class Section_Constructor
        {
            private string guid;
            private string id;
            private string course1;
            private string term1;
            private string number;
            private string credType;
            private DateTime startDate;
            private decimal credits;
            private decimal ceus;
            private List<OfferingDepartment> depts;
            private List<string> courseLevels;
            private string acadLevel;
            private string title;
            private List<SectionStatusItem> statuses;
            private Section sec;

            [TestInitialize]
            public void Section_Constructor_Initialize()
            {
                guid = Guid.NewGuid().ToString();
                id = "1";
                acadLevel = "UG";
                courseLevels = new List<string>() { "100" };
                depts = new List<OfferingDepartment>() { new OfferingDepartment("CS", 100m) };
                course1 = "1";
                term1 = "2011FA";
                number = "01";
                startDate = DateTime.Now;
                credits = 4m;
                ceus = 1m;
                title = "Section Title";
                credType = "IN";
                statuses = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-199)) };
                // constructor with no optional parameters
                sec = new Section(id, course1, number, startDate, credits, ceus, title, credType, depts, courseLevels, acadLevel, statuses);
                sec.TermId = term1;
            }

            [TestCleanup]
            public void Section_Constructor_CleanUp()
            {
            }

            [TestMethod]
            public void Section_Constructor_Id()
            {
                Assert.AreEqual(id, sec.Id);
            }

            [TestMethod]
            public void Section_Constructor_Course()
            {
                Assert.AreEqual(course1, sec.CourseId);
            }

            [TestMethod]
            public void Section_Constructor_Term()
            {
                Assert.AreEqual(term1, sec.TermId);
            }

            [TestMethod]
            public void Section_Constructor_Number()
            {
                Assert.AreEqual(number, sec.Number);
            }

            [TestMethod]
            public void Section_Constructor_StartDate()
            {
                Assert.AreEqual(startDate, sec.StartDate);
            }

            [TestMethod]
            public void Section_Constructor_MinimumCredits()
            {
                Assert.AreEqual(4m, sec.MinimumCredits);
            }

            [TestMethod]
            public void Section_Constructor_CEUs()
            {
                Assert.AreEqual(1m, sec.Ceus);
            }

            [TestMethod]
            public void Section_Constructor_Title()
            {
                Assert.AreEqual(title, sec.Title);
            }

            [TestMethod]
            public void Section_Constructor_CreditTypeCode()
            {
                Assert.AreEqual(credType, sec.CreditTypeCode);
            }

            [TestMethod]
            public void Section_Constructor_Department()
            {
                Assert.AreEqual(depts.Count, sec.Departments.Count);
                CollectionAssert.AreEqual(depts, sec.Departments.ToList());
            }

            [TestMethod]
            public void Section_Constructor_CourseLevels()
            {
                Assert.AreEqual(courseLevels.Count, sec.CourseLevelCodes.Count);
                CollectionAssert.AreEqual(courseLevels, sec.CourseLevelCodes.ToList());
            }

            [TestMethod]
            public void Section_Constructor_AcademicLevel()
            {
                Assert.AreEqual(acadLevel, sec.AcademicLevelCode);
            }

            [TestMethod]
            public void Section_Section_Constructor_Statuses()
            {
                Assert.AreEqual(statuses.Count, sec.Statuses.Count);
                CollectionAssert.AreEqual(statuses, sec.Statuses);
            }

            [TestMethod]
            public void Section_IsActive()
            {
                Assert.IsTrue(sec.IsActive);
            }

            [TestMethod]
            public void Section_IsActive_Override()
            {
                statuses[0].Status = SectionStatus.Inactive;
                Section sec1 = new Section(id, course1, number, startDate, credits, ceus, title, credType, depts, courseLevels, acadLevel, statuses);
                Assert.IsFalse(sec1.IsActive);
            }

            [TestMethod]
            public void Section_AllowPassNoPass_Default()
            {
                Assert.IsTrue(sec.AllowPassNoPass);
            }

            [TestMethod]
            public void Section_AllowPassNoPass_Override()
            {
                Section sec1 = new Section(id, course1, number, startDate, credits, ceus, title, credType, depts, courseLevels, acadLevel, statuses, allowPassNoPass: false);
                Assert.IsFalse(sec1.AllowPassNoPass);
            }

            [TestMethod]
            public void Section_AllowAudit_Default()
            {
                Assert.IsTrue(sec.AllowAudit);
            }

            [TestMethod]
            public void Section_AllowAudit_Override()
            {
                Section sec1 = new Section(id, course1, number, startDate, credits, ceus, title, credType, depts, courseLevels, acadLevel, statuses, allowAudit: false);
                Assert.IsFalse(sec1.AllowAudit);
            }

            [TestMethod]
            public void Section_OnlyPassNoPass_Override()
            {
                Section sec1 = new Section(id, course1, number, startDate, credits, ceus, title, credType, depts, courseLevels, acadLevel, statuses, allowPassNoPass: false, allowAudit: true, onlyPassNoPass: true);
                Assert.IsTrue(sec1.OnlyPassNoPass);
                Assert.IsTrue(sec1.AllowPassNoPass);
                Assert.IsFalse(sec1.AllowAudit);
            }

            [TestMethod]
            public void Section_HideInCatalog_DefaultFalse()
            {
                Section sec1 = new Section(id, course1, number, startDate, credits, ceus, title, credType, depts, courseLevels, acadLevel, statuses, allowPassNoPass: false, allowAudit: true, onlyPassNoPass: true);
                // Should be false by default
                Assert.IsFalse(sec1.HideInCatalog);
            }

            [TestMethod]
            public void Section_HideInCatalog_True()
            {
                Section sec1 = new Section(id, course1, number, startDate, credits, ceus, title, credType, depts, courseLevels, acadLevel, statuses, allowPassNoPass: false, allowAudit: true, onlyPassNoPass: true, hideInCatalog: true);
                Assert.IsTrue(sec1.HideInCatalog);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Section_ThrowsExceptionIfCourseIdEmpty()
            {
                new Section(id, "", number, startDate, credits, ceus, title, credType, depts, courseLevels, acadLevel, statuses);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Section_ThrowsExceptionIfNumberNull()
            {
                new Section(id, course1, null, startDate, credits, ceus, title, credType, depts, courseLevels, acadLevel, statuses);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Section_ThrowsExceptionIfStartDateMinValue()
            {
                new Section(id, course1, number, DateTime.MinValue, credits, ceus, title, credType, depts, courseLevels, acadLevel, statuses);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void Section_ThrowsExceptionIfMinCreditsNegative()
            {
                new Section(id, course1, number, startDate, -1m, ceus, title, credType, depts, courseLevels, acadLevel, statuses);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void Section_ThrowsExceptionIfCEUsNegative()
            {
                new Section(id, course1, number, startDate, credits, -2m, title, credType, depts, courseLevels, acadLevel, statuses);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void Section_ThrowsExceptionIfBothMinCreditsAndCEUsNull()
            {
                new Section(id, course1, number, startDate, null, null, title, credType, depts, courseLevels, acadLevel, statuses);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Section_ThrowsExceptionIfTitleNull()
            {
                new Section(id, course1, number, startDate, credits, ceus, null, credType, depts, courseLevels, acadLevel, statuses);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Section_ThrowsExceptionIfTitleEmpty()
            {
                new Section(id, course1, number, startDate, credits, ceus, "", credType, depts, courseLevels, acadLevel, statuses);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Section_ThrowsExceptionIfCreditTypeNull()
            {
                new Section(id, course1, number, startDate, credits, ceus, title, null, depts, courseLevels, acadLevel, statuses);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Section_ThrowsExceptionIfCreditTypeEmpty()
            {
                new Section(id, course1, number, startDate, credits, ceus, title, "", depts, courseLevels, acadLevel, statuses);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Section_ThrowsExceptionIfDeptsNull()
            {
                new Section(id, course1, number, startDate, credits, ceus, title, credType, null, courseLevels, acadLevel, statuses);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void Section_ThrowsExceptionIfDeptsCountZero()
            {
                var departments = new List<OfferingDepartment>();
                new Section(id, course1, number, startDate, credits, ceus, title, credType, departments, courseLevels, acadLevel, statuses);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Section_ThrowsExceptionIfCourseLevelsNull()
            {
                new Section(id, course1, number, startDate, credits, ceus, title, credType, depts, null, acadLevel, statuses);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void Section_ThrowsExceptionIfCourseLevelsCountZero()
            {
                var cLevels = new List<string>();
                new Section(id, course1, number, startDate, credits, ceus, title, credType, depts, cLevels, acadLevel, statuses);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Section_ThrowsExceptionIfAcademicLevelNull()
            {
                new Section(id, course1, number, startDate, credits, ceus, title, credType, depts, courseLevels, null, statuses);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Section_ThrowsExceptionIfStatusesNull()
            {
                new Section(id, course1, number, startDate, credits, ceus, title, credType, depts, courseLevels, acadLevel, null);
            }

            [TestMethod]
            public void Section_AttendanceTrackingType_Defaults_to_PresentAbsent()
            {
                Assert.AreEqual(AttendanceTrackingType.PresentAbsent, sec.AttendanceTrackingType);
            }
        }

        [TestClass]
        public class Section_Id
        {
            private string course1;
            private string number;
            private DateTime startDate;
            private decimal credits;
            private decimal ceus;
            private string creditType;
            private List<OfferingDepartment> depts;
            private List<SectionStatusItem> statuses;
            private string term1;
            private Section sec;
            private string title;
            private string dept1;
            private string dept2;
            private string acadLevel;
            private List<string> courseLevels;

            [TestInitialize]
            public void Section_Id_Initialize()
            {
                number = "01";
                startDate = DateTime.Today;
                credits = 3m;
                ceus = 2m;
                dept1 = "CS";
                dept2 = "MATH";
                acadLevel = "UG";
                creditType = "IN";
                courseLevels = new List<string>() { "100" };
                depts = new List<OfferingDepartment>() { new OfferingDepartment(dept1, 50m), new OfferingDepartment(dept2, 50m) };
                statuses = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddYears(-3)) };
                title = "Course 101";
                course1 = "1";
                term1 = "2011FA";
                sec = new Section("", course1, number, startDate, credits, ceus, title, creditType, depts, courseLevels, acadLevel, statuses);
                sec.TermId = term1;
            }

            [TestMethod]
            public void Section_Id_MayBeUpdatedIfEmpty()
            {
                sec.Id = "2";
                Assert.AreEqual("2", sec.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void Section_Id_ThrowsExceptionIfUpdatingNonzeroId()
            {
                sec = new Section("1", course1, number, startDate, credits, ceus, title, creditType, depts, courseLevels, acadLevel, statuses);
                sec.Id = "3";
            }
        }

        [TestClass]
        public class Section_NonRequiredProperties
        {
            private string course1;
            private string number;
            private DateTime startDate;
            private decimal credits;
            private decimal ceus;
            private string creditType;
            private List<OfferingDepartment> depts;
            private List<SectionStatusItem> statuses;
            private Section sec;
            private string title;
            private string dept1;
            private string dept2;
            private string acadLevel;
            private ICollection<string> courseLevels;
            private decimal billingCredit;

            [TestInitialize]
            public void Section_NonRequiredProperties_Initialize()
            {
                number = "01";
                startDate = DateTime.Today;
                credits = 3m;
                ceus = 2m;
                creditType = "IN";
                dept1 = "CS";
                dept2 = "MATH";
                depts = new List<OfferingDepartment>() { new OfferingDepartment(dept1, 50m), new OfferingDepartment(dept2, 50m) };
                statuses = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddYears(-3)) };
                acadLevel = "UG";
                courseLevels = new List<string>() { "100" };
                title = "Course 101";
                course1 = "1";
                billingCredit = 2;
                sec = new Section("", course1, number, startDate, credits, ceus, title, creditType, depts, courseLevels, acadLevel, statuses);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void Section_MaxCredits_ThrowErrorIfNegative()
            {
                sec.MaximumCredits = -2.0m;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void Section_MaxCredits_ThrowErrorIfLessThanMinCredits()
            {
                sec.MaximumCredits = 1.0m;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void Section_VariableCreditIncrement_ThrowsExceptionIfNegative()
            {
                sec.VariableCreditIncrement = -1.0m;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void Section_VariableCreditIncrement_ThrowsExceptionIfLargerThanMaximum()
            {
                sec.MaximumCredits = 3.0m;
                sec.VariableCreditIncrement = 4.0m;
            }

            [TestMethod]
            public void Section_Location_EmptyAllowed()
            {
                sec.Location = "";
                Assert.IsTrue(string.IsNullOrEmpty(sec.Location));
            }

            [TestMethod]
            public void Section_Location_NullAllowed()
            {
                sec.Location = null;
                Assert.IsTrue(string.IsNullOrEmpty(sec.Location));
            }

            [TestMethod]
            public void Section_Location()
            {
                sec.Location = "something";
                Assert.AreEqual("something", sec.Location);
            }

            public void Section_Term_EmptyAllowed()
            {
                sec.TermId = "";
                Assert.IsTrue(string.IsNullOrEmpty(sec.TermId));
            }

            [TestMethod]
            public void Section_Term_NullAllowed()
            {
                sec.TermId = null;
                Assert.IsTrue(string.IsNullOrEmpty(sec.TermId));
            }

            [TestMethod]
            public void Section_TermId()
            {
                sec.TermId = "something";
                Assert.AreEqual("something", sec.TermId);
            }

            [TestMethod]
            public void Section_EndDate_NullAllowed()
            {
                sec.EndDate = null;
                Assert.AreEqual(null, sec.EndDate);
            }

            [TestMethod]
            public void Section_EndDate()
            {
                var newDate = new DateTime(2012, 12, 1);
                sec.EndDate = newDate;
                Assert.AreEqual(newDate, sec.EndDate);
            }

            [TestMethod]
            public void Section_CourseTypes()
            {
                var courseTypes = new List<string>() { "type1", "type2" };
                foreach (var item in courseTypes)
                {
                    sec.AddCourseType(item);
                }
                Assert.AreEqual(courseTypes.Count, sec.CourseTypeCodes.Count());
                CollectionAssert.AreEqual(courseTypes, sec.CourseTypeCodes.ToList());
            }

            [TestMethod]
            public void Section_CourseTypes_IsEmptyListWhenNulled()
            {
                Assert.AreEqual(0, sec.CourseTypeCodes.Count());
            }


            [TestMethod]
            public void Section_BillingCredits()
            {
                sec.BillingCred = billingCredit;
                Assert.AreEqual(billingCredit, sec.BillingCred);
            }

            [TestMethod]
            public void Section_AttendanceTrackingType()
            {
                sec.AttendanceTrackingType = AttendanceTrackingType.CumulativeHours;
                Assert.AreEqual(AttendanceTrackingType.CumulativeHours, sec.AttendanceTrackingType);
            }

            [TestMethod]
            public void Section_PrimarySectionMeetings()
            {
                Assert.IsNotNull(sec.PrimarySectionMeetings);
                Assert.AreEqual(0, sec.PrimarySectionMeetings.Count);
            }

            [TestMethod]
            public void Section_GradeSchemeCode()
            {
                sec.GradeSchemeCode = "UG";
                Assert.AreEqual("UG", sec.GradeSchemeCode);
            }

            [TestMethod]
            public void Section_GradeSubschemeCode()
            {
                sec.GradeSubschemeCode = "UGS";
                Assert.AreEqual("UGS", sec.GradeSubschemeCode);
            }

            [TestMethod]
            public void Section_Synonym()
            {
                sec.Synonym = "0003444";
                Assert.AreEqual("0003444", sec.Synonym);
            }
        }

        [TestClass]
        public class Section_AddSectionMeeting
        {
            private Section sec;
            private string id;
            private string course1;
            private string title;
            private string number;
            private DateTime startDate;
            private decimal credits;
            private decimal ceus;
            private string creditType;
            private string acadLevel;
            private List<OfferingDepartment> depts;
            private List<string> courseLevels;
            private List<SectionStatusItem> statuses;
            private string smId1, smId2, smGuid1, smGuid2;
            private List<DayOfWeek> days1;
            private DateTimeOffset start1;
            private DateTimeOffset end1;
            private string room1;
            private List<DayOfWeek> days2;
            private string room2;
            private string instrMethod;
            private SectionMeeting mt1;
            private SectionMeeting mt2;

            [TestInitialize]
            public void Section_AddSectionMeeting_Initialize()
            {
                id = "1";
                course1 = "1";
                number = "01";
                startDate = DateTime.Today;
                credits = 4m;
                ceus = 1m;
                title = "Section Title";
                creditType = "IN";
                acadLevel = "UG";
                courseLevels = new List<string>() { "100" };
                var dept = new OfferingDepartment("CS", 100m);
                depts = new List<OfferingDepartment>() { dept };
                statuses = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddYears(-3)) };
                sec = new Section(id, course1, number, startDate, credits, ceus, title, creditType, depts, courseLevels, acadLevel, statuses);
                sec.EndDate = startDate.AddDays(60);

                smGuid1 = Guid.NewGuid().ToString();
                smGuid2 = Guid.NewGuid().ToString();
                smId1 = "123";
                smId2 = "124";
                days1 = new List<DayOfWeek>();
                days1.Add(DayOfWeek.Monday);
                days1.Add(DayOfWeek.Wednesday);
                days2 = new List<DayOfWeek>();
                days2.Add(DayOfWeek.Saturday);

                var now = DateTime.Now;
                var offset = now.ToLocalTime() - now.ToUniversalTime();
                start1 = new DateTimeOffset(new DateTime(1, 1, 1, 10, 0, 0), offset);
                end1 = new DateTimeOffset(new DateTime(1, 1, 1, 11, 30, 0), offset);
                room1 = "BOCK*101";
                room2 = "BOCK*201";
                instrMethod = "LEC";
                var frequency = "W";
                mt1 = new SectionMeeting(smId1, id, instrMethod, sec.StartDate, sec.EndDate.GetValueOrDefault(), frequency) { Guid = smGuid1, Room = room1, StartTime = start1, EndTime = end1, Days = days1 };
                mt2 = new SectionMeeting(smId2, id, instrMethod, sec.StartDate, sec.EndDate.GetValueOrDefault(), frequency) { Guid = smGuid2, Room = room2 };
                sec.AddSectionMeeting(mt1);
            }

            [TestMethod]
            public void Section_AddSectionMeeting_AddsSectionMeetingSuccess()
            {
                Assert.AreEqual(1, sec.Meetings.Count());
            }

            [TestMethod]
            public void Section_AddSectionMeeting_AddsAdditionalMeetingTime()
            {
                sec.AddSectionMeeting(mt2);
                Assert.AreEqual(2, sec.Meetings.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Section_AddSectionMeeting_ThrowsExceptionIfMeetingPatternNull()
            {
                sec.AddSectionMeeting(null);
            }
        }

        [TestClass]
        public class Section_AddFaculty
        {
            private Section sec;
            private string title;

            [TestInitialize]
            public void Section_AddFaculty_Initialize()
            {
                var dept = "CS";
                var acadLevel = "UG";
                var courseLevels = new List<string>() { "100" };
                var depts = new List<OfferingDepartment>() { new OfferingDepartment(dept, 100m) };
                var statuses = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddYears(-4)) };
                var course1 = "1";
                //var term1 = "2011FA";
                var number = "01";
                var startDate = DateTime.Now;
                title = "Section Title";
                sec = new Section("1", course1, number, startDate, 4m, 1m, title, "IN", depts, courseLevels, acadLevel, statuses);
                sec.AddFaculty("1010101");
            }

            [TestMethod]
            public void Section_AddFaculty_AddsFacultySuccess()
            {
                Assert.AreEqual(1, sec.FacultyIds.Count);
            }

            [TestMethod]
            public void Section_AddFaculty_AddsAdditionalFacultySuccess()
            {
                sec.AddFaculty("2020202");
                Assert.AreEqual(2, sec.FacultyIds.Count);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Section_AddFaculty_ThrowsExceptionIfFacultyNull()
            {
                sec.AddFaculty(null);
            }

            [TestMethod]
            public void Section_AddFaculty_DoesNotAddDuplicateFaculty()
            {
                Assert.AreEqual(1, sec.FacultyIds.Count);
                Assert.AreEqual("1010101", sec.FacultyIds[0]);
                sec.AddFaculty("1010101");
                Assert.AreEqual(1, sec.FacultyIds.Count);
                Assert.AreEqual("1010101", sec.FacultyIds[0]);
            }
        }

        [TestClass]
        public class Section_AddActiveStudent 
        {
            private Section sec;
            private string title;

            [TestInitialize]
            public void Section_AddActiveStudent_Initialize()
            {
                var dept = "CS";
                var acadLevel = "UG";
                var courseLevels = new List<string>() { "100" };
                var depts = new List<OfferingDepartment>() { new OfferingDepartment(dept, 100m) };
                var course1 = "1";
                //var term1 = "2011FA";
                var number = "01";
                var startDate = DateTime.Now;
                title = "Section Title";
                var statuses = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddYears(-4)) };
                sec = new Section("1", course1, number, startDate, 4m, 1m, title, "IN", depts, courseLevels, acadLevel, statuses);
                sec.AddActiveStudent("11111111");
            }

            [TestMethod]
            public void Section_AddActiveStudent_AddsActiveStudentSuccess()
            {
                Assert.AreEqual(1, sec.ActiveStudentIds.Count);
            }

            [TestMethod]
            public void Section_AddActiveStudent_AddsAdditionalStudentSuccess()
            {
                sec.AddActiveStudent("22222222");
                Assert.AreEqual(2, sec.ActiveStudentIds.Count);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Section_AddActiveStudent_ThrowsExceptionIfActiveStudentNull()
            {
                sec.AddActiveStudent(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void Section_AddActiveStudent_ThrowsExceptionIfDuplicateActiveStudent()
            {
                sec.AddActiveStudent("11111111");
            }
        }

        [TestClass]
        public class Section_AddBook
        {
            private Section sec;

            [TestInitialize]
            public void Section_AddBook_Initialize()
            {
                var dept = "CS";
                var acadLevel = "UG";
                var courseLevels = new List<string>() { "100" };
                var depts = new List<OfferingDepartment>() { new OfferingDepartment(dept, 100m) };
                var statuses = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddYears(-4)) };
                var course1 = "1";
                var number = "01";
                var startDate = DateTime.Now;
                string title = "Title";
                sec = new Section("1", course1, number, startDate, 4m, 1m, title, "IN", depts, courseLevels, acadLevel, statuses);
                sec.AddBook("111", "R", true);
                sec.AddBook("222", "O", false);
            }

            [TestMethod]
            public void Section_AddBook_Books_Count()
            {
                Assert.AreEqual(2, sec.Books.Count);
            }
                

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Section_AddBook_ThrowsExceptionIfBookIdNull()
            {
                sec.AddBook(null, "R", true);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Section_AddBook_ThrowsExceptionIfBookIdEmpty()
            {
                sec.AddBook(string.Empty, "R", true);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void Section_AddBook_ThrowsExceptionIfDuplicateBook()
            {
                sec.AddBook("111", "O", false);
            }

            [TestMethod]
            public void Section_AddBook_Properties()
            {
                Assert.AreEqual("111", sec.Books[0].BookId);
                Assert.IsFalse(sec.Books[1].IsRequired);
            }

        }

        [TestClass]
        public class Section_AddCrossListedSection
        {
            private Section sec;
            private Section sec2;

            [TestInitialize]
            public void Section_AddCrossListedSection_Initialize()
            {
                var dept = "CS";
                var acadLevel = "UG";
                var courseLevels = new List<string>() { "100" };
                var depts = new List<OfferingDepartment>() { new OfferingDepartment(dept, 100m) };
                var statuses = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddYears(-4)) };
                var course1 = "1";
                var number = "01";
                var startDate = DateTime.Now;
                string title = "Title";
                sec = new Section("1", course1, number, startDate, 4m, 1m, title, "IN", depts, courseLevels, acadLevel, statuses);
                sec2 = new Section("2", course1, "02", startDate, 4m, 1m, "New Title", "IN", depts, courseLevels, acadLevel, statuses);
                sec.AddCrossListedSection(sec2);
            }

            [TestMethod]
            public void Section_AddCrossListedSection_CrossListedSection_Count()
            {
                Assert.AreEqual(1, sec.CrossListedSections.Count);
            }


            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Section_AddCrossListedSection_ThrowsExceptionIfCrossListNull()
            {
                sec.AddCrossListedSection(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void Section_AddCrossListedSection_ThrowsExceptionIfDuplicateCrossListedSection()
            {
                sec.AddCrossListedSection(sec2);
            }

            [TestMethod]
            public void Section_AddCrossListedSection_SectionCrossListedSection_Properties()
            {
                Assert.AreEqual("2", sec.CrossListedSections[0].Id);
                Assert.AreEqual("02", sec.CrossListedSections[0].Number);
                Assert.AreEqual("New Title", sec.CrossListedSections[0].Title);
            }

        }

        [TestClass]
        public class Section_CalculatedSectionProperties
        {
            private Section sec;
            private string dept;
            private string acadLevel;
            private List<string> courseLevels = new List<string>();
            private string course1;
            private string number;
            private string title;
            private DateTime startDate;
            private List<OfferingDepartment> depts;
            private List<SectionStatusItem> statuses;

            [TestInitialize]
            public void Section_CalculatedSectionProperties_Initialize()
            {
                dept = "CS";
                acadLevel = "UG";
                courseLevels = new List<string>() { "100" };
                depts = new List<OfferingDepartment>() { new OfferingDepartment(dept, 100m) };
                statuses = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddYears(-4)) };
                course1 = "1";
                number = "01";
                startDate = DateTime.Now;
                title = "Title";

                // SectionCapacity but no students and no reserved seats.
                sec = new Section("1", course1, number, startDate, 4m, 1m, title, "IN", depts, courseLevels, acadLevel, statuses);
                sec.CombineCrosslistWaitlists = false;
                //sec.CombineCrosslistWaitlists = true;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void Section_CalculatedSectionProperties_SectionCapacityNegative()
            {
                sec.SectionCapacity = -10;
            }

            [TestMethod]
            public void Section_CalculatedSectionProperties_SecCapacityOnly()
            {
                sec.SectionCapacity = 10;
                Assert.AreEqual(10, sec.Capacity);
                Assert.AreEqual(10, sec.Available);
            }

            [TestMethod]
            public void Section_CalculatedSectionProperties_NullCapacity_EnrolledStudents()
            {
                // No Section Capacity but with enrolled students and no reserved seats.
                sec.AddActiveStudent("1111");
                sec.AddActiveStudent("2222");
                Assert.IsNull(sec.Capacity);
                Assert.IsNull(sec.Available);
            }

            [TestMethod]
            public void Section_CalculatedSectionProperties_SectionEnrolledGreaterThanSectionCapacity()
            {
                // No Section Capacity but with enrolled students and no reserved seats.
                sec.SectionCapacity = 20;
                sec.AddActiveStudent("1111");
                sec.AddActiveStudent("2222");
                sec.ReservedSeats = 20;
                Assert.AreEqual(20, sec.Capacity);
                Assert.AreEqual(0, sec.Available);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void Section_CalculatedSectionProperties_NegativeGlobalCapacity()
            {
                sec.GlobalCapacity = -1;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void Section_CalculatedSectionProperties_NegativeReservedSeats()
            {
                sec.ReservedSeats = -1;
            }

            [TestMethod]
            public void Section_CalculatedSectionProperties_GlobalCapacity()
            {
                sec.GlobalCapacity = 100;
                sec.SectionCapacity = 5;
                Assert.AreEqual(100, sec.Capacity);
            }

            [TestMethod]
            public void Section_CalculatedSectionProperties_GlobalAvailableLessThanLocalAvailable()
            {
                Section crossList1 = new Section("1", course1, number, DateTime.Now, 4m, 1m, "Tech Writing", "IN", depts, courseLevels, acadLevel, statuses);
                crossList1.AddActiveStudent("1111");
                crossList1.AddActiveStudent("2222");
                crossList1.ReservedSeats = 1;
                crossList1.SectionCapacity = 5;
                Section crossList2 = new Section("2", course1, number, DateTime.Now, 4m, 1m, "Tech Writing", "IN", depts, courseLevels, acadLevel, statuses);
                crossList2.AddActiveStudent("3333");
                crossList2.AddActiveStudent("4444");
                crossList2.SectionCapacity = 5;
                Section section = new Section("3", course1, number, DateTime.Now, 4m, 1m, "Tech Writing", "IN", depts, courseLevels, acadLevel, statuses);
                section.AddActiveStudent("5555");
                section.SectionCapacity = 5;
                section.GlobalCapacity = 7;
                section.AddCrossListedSection(crossList1);
                section.AddCrossListedSection(crossList2);
                Assert.AreEqual(7, section.Capacity);
                Assert.AreEqual(1, section.Available);
            }

            [TestMethod]
            public void Section_CalculatedSectionProperties_LocalAvailableLessThanGlobalAvailable()
            {
                Section crossList1 = new Section("1", course1, number, DateTime.Now, 4m, 1m, "Tech Writing", "IN", depts, courseLevels, acadLevel, statuses);
                crossList1.AddActiveStudent("1111");
                crossList1.AddActiveStudent("2222");
                crossList1.ReservedSeats = 1;
                crossList1.SectionCapacity = 5;
                Section crossList2 = new Section("2", course1, number, DateTime.Now, 4m, 1m, "Tech Writing", "IN", depts, courseLevels, acadLevel, statuses);
                crossList2.AddActiveStudent("3333");
                crossList2.AddActiveStudent("4444");
                crossList2.SectionCapacity = 5;
                Section section = new Section("3", course1, number, DateTime.Now, 4m, 1m, "Tech Writing", "IN", depts, courseLevels, acadLevel, statuses);
                section.AddActiveStudent("5555");
                section.SectionCapacity = 5;
                section.GlobalCapacity = 12;
                section.AddCrossListedSection(crossList1);
                section.AddCrossListedSection(crossList2);
                Assert.AreEqual(12, section.Capacity);
                Assert.AreEqual(4, section.Available);
            }

            [TestMethod]
            public void Section_CalculatedSectionProperties_NullCapacityAndAvailable()
            {
                Section section = new Section("1", course1, number, DateTime.Now, 4m, 1m, "Tech Writing", "IN", depts, courseLevels, acadLevel, statuses);
                section.AddActiveStudent("5555");
                // Set neither capacity.
                Assert.IsNull(section.Capacity);
                Assert.IsNull(section.Available);
            }

            [TestMethod]
            public void Section_CalculatedSectionProperties_GlobalCapacityNullLocalCapacity()
            {
                Section section = new Section("1", course1, number, DateTime.Now, 4m, 1m, "Tech Writing", "IN", depts, courseLevels, acadLevel, statuses);
                section.AddActiveStudent("5555");
                section.GlobalCapacity = 5;
                Assert.AreEqual(5, section.Capacity);
                // However the section is still unlimited. As weird as it seems.
                Assert.IsNull(section.Available);
            }

            [TestMethod]
            public void Section_CalculatedSectionProperties_CombinedCrosslistWaitlist_WaitlistOnCrosslist()
            {
                sec.CombineCrosslistWaitlists = true;
                sec.SectionCapacity = 5;
                sec.AddActiveStudent("1111");
                sec.GlobalCapacity = 10;
                Section xlist1 = new Section("44", course1, number, DateTime.Now, 4m, 1m, "Tech Writing 2", "IN", depts, courseLevels, acadLevel, statuses);
                xlist1.NumberOnWaitlist = 1;
                sec.AddCrossListedSection(xlist1);
                Assert.AreEqual(0, sec.Available);
            }

            [TestMethod]
            public void Section_CalculatedSectionProperties_CombinedCrosslistWaitlistFalse_WaitlistOnCrosslist()
            {
                sec.CombineCrosslistWaitlists = false;
                sec.SectionCapacity = 5;
                sec.AddActiveStudent("1111");
                sec.GlobalCapacity = 10;
                Section xlist1 = new Section("44", course1, number, DateTime.Now, 4m, 1m, "Tech Writing 2", "IN", depts, courseLevels, acadLevel, statuses);
                xlist1.NumberOnWaitlist = 1;
                sec.AddCrossListedSection(xlist1);
                Assert.AreEqual(4, sec.Available);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void Section_CalculatedSectionProperties_NegativeWaitlistMaximum()
            {
                sec.WaitlistMaximum = -1;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void Section_CalculatedSectionProperties_NegativeGlobalWaitlistMaximum()
            {
                sec.GlobalWaitlistMaximum = -1;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void Section_CalculatedSectionProperties_NegativePermittedToRegister()
            {
                sec.PermittedToRegisterOnWaitlist = -1;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void Section_CalculatedSectionProperties_NegativeNumberOnWaitlist()
            {
                sec.NumberOnWaitlist = -1;
            }

            [TestMethod]
            public void Section_CalculatedSectionProperties_WaitlistAvailable_CancelledSection()
            {
                Section sec2 = new Section("1", course1, number, startDate, 4m, 1m, title, "IN", depts, courseLevels, acadLevel, statuses);
                sec2.AddStatus(SectionStatus.Inactive, "I");
                Assert.IsFalse(sec2.WaitlistAvailable);
            }

            [TestMethod]
            public void Section_CalculatedSectionProperties_WaitlistAvailable_SectionWaitlistNotAllowed()
            {
                Section sec2 = new Section("1", course1, number, startDate, 4m, 1m, title, "IN", depts, courseLevels, acadLevel, statuses, allowWaitlist: false);
                Assert.IsFalse(sec2.WaitlistAvailable);
            }

            [TestMethod]
            public void Section_CalculatedSectionProperties_WaitlistAvailable_SectionWaitlistClosed()
            {
                Section sec2 = new Section("1", course1, number, startDate, 4m, 1m, title, "IN", depts, courseLevels, acadLevel, statuses, allowWaitlist: true, waitlistClosed: true);
                Assert.IsFalse(sec2.WaitlistAvailable);
            }

            [TestMethod]
            public void Section_CalculatedSectionProperties_WaitlistAvailable_NotCrossListed_NullWaitlistMax()
            {
                Section sec2 = new Section("1", course1, number, startDate, 4m, 1m, title, "IN", depts, courseLevels, acadLevel, statuses, allowWaitlist: true, waitlistClosed: false);
                sec2.NumberOnWaitlist = 10;
                Assert.IsTrue(sec2.WaitlistAvailable);
            }

            [TestMethod]
            public void Section_CalculatedSectionProperties_WaitlistAvailable_NotCrossListed_WaitlistMaxReached()
            {
                Section sec2 = new Section("1", course1, number, startDate, 4m, 1m, title, "IN", depts, courseLevels, acadLevel, statuses, allowWaitlist: true, waitlistClosed: false);
                sec2.WaitlistMaximum = 9;
                sec2.NumberOnWaitlist = 10;
                Assert.IsFalse(sec2.WaitlistAvailable);
            }

            [TestMethod]
            public void Section_CalculatedSectionProperties_WaitlistAvailable_NotCrossListed_WaitlistMaxNotReached()
            {
                Section sec2 = new Section("1", course1, number, startDate, 4m, 1m, title, "IN", depts, courseLevels, acadLevel, statuses, allowWaitlist: true, waitlistClosed: false);
                sec2.WaitlistMaximum = 9;
                sec2.NumberOnWaitlist = 8;
                Assert.IsTrue(sec2.WaitlistAvailable);
            }

            [TestMethod]
            public void Section_CalculatedSectionProperties_WaitlistAvailable_NotCrossListed_WaitlistMaxNotReached2()
            {
                Section sec2 = new Section("1", course1, number, startDate, 4m, 1m, title, "IN", depts, courseLevels, acadLevel, statuses, allowWaitlist: true, waitlistClosed: false);
                sec2.WaitlistMaximum = 9;
                sec2.NumberOnWaitlist = null;
                Assert.IsTrue(sec2.WaitlistAvailable);
            }

            [TestMethod]
            public void Section_CalculatedSectionProperties_WaitlistAvailable_NullWaitlistMaximums()
            {
                Section sec2 = new Section("1", course1, number, startDate, 4m, 1m, title, "IN", depts, courseLevels, acadLevel, statuses, allowWaitlist: true, waitlistClosed: false);
                sec2.NumberOnWaitlist = 100;
                Assert.IsTrue(sec2.WaitlistAvailable);
            }

            [TestMethod]
            public void Section_CalculatedSectionProperties_WaitlistAvailable_NullGlobalMax_WaitlistMaxNotReached()
            {
                Section sec2 = new Section("1", course1, number, startDate, 4m, 1m, title, "IN", depts, courseLevels, acadLevel, statuses, allowWaitlist: true, waitlistClosed: false);
                sec2.WaitlistMaximum = 3;
                sec2.NumberOnWaitlist = 2;
                Assert.IsTrue(sec2.WaitlistAvailable);
            }

            [TestMethod]
            public void Section_CalculatedSectionProperties_WaitlistAvailable_NullGlobalMax_WaitlistMaxReached()
            {
                Section sec2 = new Section("1", course1, number, startDate, 4m, 1m, title, "IN", depts, courseLevels, acadLevel, statuses, allowWaitlist: true, waitlistClosed: false);
                sec2.WaitlistMaximum = 3;
                sec2.NumberOnWaitlist = 3;
                Assert.IsFalse(sec2.WaitlistAvailable);
            }

            [TestMethod]
            public void Section_CalculatedSectionProperties_WaitlistAvailable_GlobalMaxReached_NullSectionMax()
            {
                Section sec2 = new Section("1", course1, number, startDate, 4m, 1m, title, "IN", depts, courseLevels, acadLevel, statuses, allowWaitlist: true, waitlistClosed: false);
                sec2.NumberOnWaitlist = 4;
                Section sec3 = new Section("2", "333", "01", startDate, 3m, 1m, "New Title", "IN", depts, courseLevels, acadLevel, statuses, allowWaitlist: true, waitlistClosed: false);
                sec3.NumberOnWaitlist = 6;
                sec3.GlobalWaitlistMaximum = 10;
                sec2.AddCrossListedSection(sec3);
                sec2.GlobalWaitlistMaximum = 10;
                Assert.IsFalse(sec2.WaitlistAvailable);
            }

            [TestMethod]
            public void Section_CalculatedSectionProperties_WaitlistAvailable_GlobalMaxNotReached_NullSectionMax()
            {
                Section sec2 = new Section("1", course1, number, startDate, 4m, 1m, title, "IN", depts, courseLevels, acadLevel, statuses, allowWaitlist: true, waitlistClosed: false);
                sec2.NumberOnWaitlist = null;
                Section sec3 = new Section("2", "333", "01", startDate, 3m, 1m, "New Title", "IN", depts, courseLevels, acadLevel, statuses, allowWaitlist: true, waitlistClosed: false);
                sec3.NumberOnWaitlist = 6;
                sec3.GlobalWaitlistMaximum = 10;
                sec2.AddCrossListedSection(sec3);
                sec2.GlobalWaitlistMaximum = 10;
                Assert.IsTrue(sec2.WaitlistAvailable);
            }

            [TestMethod]
            public void Section_CalculatedSectionProperties_WaitlistAvailable_BothMaximums_NeitherReached()
            {
                Section sec2 = new Section("1", course1, number, startDate, 4m, 1m, title, "IN", depts, courseLevels, acadLevel, statuses, allowWaitlist: true, waitlistClosed: false);
                sec2.NumberOnWaitlist = 2;
                sec2.WaitlistMaximum = 5;
                sec2.GlobalWaitlistMaximum = 10;
                Section sec3 = new Section("2", "333", "01", startDate, 3m, 1m, "New Title", "IN", depts, courseLevels, acadLevel, statuses, allowWaitlist: true, waitlistClosed: false);
                sec3.NumberOnWaitlist = 6;
                sec3.GlobalWaitlistMaximum = 10;
                sec2.AddCrossListedSection(sec3); 
                Assert.IsTrue(sec2.WaitlistAvailable);
            }

            [TestMethod]
            public void Section_CalculatedSectionProperties_WaitlistAvailable_BothMaximums_GlobalReached()
            {
                Section sec2 = new Section("1", course1, number, startDate, 4m, 1m, title, "IN", depts, courseLevels, acadLevel, statuses, allowWaitlist: true, waitlistClosed: false);
                sec2.NumberOnWaitlist = 2;
                sec2.WaitlistMaximum = 5;
                sec2.GlobalWaitlistMaximum = 8;
                Section sec3 = new Section("2", "333", "01", startDate, 3m, 1m, "New Title", "IN", depts, courseLevels, acadLevel, statuses, allowWaitlist: true, waitlistClosed: false);
                sec3.NumberOnWaitlist = 6;
                sec3.GlobalWaitlistMaximum = 8;
                sec2.AddCrossListedSection(sec3);
                bool x = sec2.WaitlistAvailable;
                Assert.IsFalse(sec2.WaitlistAvailable);
            }

            [TestMethod]
            public void Section_CalculatedSectionProperties_WaitlistAvailable_BothMaximums_SectionReached()
            {
                Section sec2 = new Section("1", course1, number, startDate, 4m, 1m, title, "IN", depts, courseLevels, acadLevel, statuses, allowWaitlist: true, waitlistClosed: false);
                sec2.NumberOnWaitlist = 5;
                sec2.WaitlistMaximum = 5;
                sec2.GlobalWaitlistMaximum = 10;
                Section sec3 = new Section("2", "333", "01", startDate, 3m, 1m, "New Title", "IN", depts, courseLevels, acadLevel, statuses, allowWaitlist: true, waitlistClosed: false);
                sec3.NumberOnWaitlist = 4;
                sec3.GlobalWaitlistMaximum = 10;
                sec3.WaitlistMaximum = 5;
                sec2.AddCrossListedSection(sec3);
                bool x = sec2.WaitlistAvailable;
                Assert.IsFalse(sec2.WaitlistAvailable);
            }

            [TestMethod]
            public void Section_CalculatedSectionProperties_OnlineCategory_Online()
            {
                Section onlineSection = new Section("1", course1, number, startDate, 4m, 1m, title, "IN", depts, courseLevels, acadLevel, statuses, allowWaitlist: true, waitlistClosed: false);
                var meeting1 = new SectionMeeting("11", "1", "ONL", new DateTime(2012, 9, 1), new DateTime(2012, 12, 12), "W") { IsOnline = true };
                onlineSection.AddSectionMeeting(meeting1);
                var meeting2 = new SectionMeeting("12", "1", "ONL", new DateTime(2012, 9, 1), new DateTime(2012, 12, 12), "W") { IsOnline = true };
                onlineSection.AddSectionMeeting(meeting2);
                Assert.AreEqual(OnlineCategory.Online, onlineSection.OnlineCategory);
            }

            [TestMethod]
            public void Section_CalculatedSectionProperties_OnlineCategory_NoMeetings_NotOnline()
            {
                Section onlineSection = new Section("1", course1, number, startDate, 4m, 1m, title, "IN", depts, courseLevels, acadLevel, statuses, allowWaitlist: true, waitlistClosed: false);
                Assert.AreEqual(OnlineCategory.NotOnline, onlineSection.OnlineCategory);
            }

            [TestMethod]
            public void Section_CalculatedSectionProperties_OnlineCategory_NotOnline()
            {
                Section onlineSection = new Section("1", course1, number, startDate, 4m, 1m, title, "IN", depts, courseLevels, acadLevel, statuses, allowWaitlist: true, waitlistClosed: false);
                var meeting1 = new SectionMeeting("11", "1", "LEC", new DateTime(2012, 9, 1), new DateTime(2012, 12, 12), "W") { IsOnline = false };
                onlineSection.AddSectionMeeting(meeting1);
                Assert.AreEqual(OnlineCategory.NotOnline, onlineSection.OnlineCategory);
            }

            [TestMethod]
            public void Section_CalculatedSectionProperties_OnlineCategory_Hybrid()
            {
                Section onlineSection = new Section("1", course1, number, startDate, 4m, 1m, title, "IN", depts, courseLevels, acadLevel, statuses, allowWaitlist: true, waitlistClosed: false);
                var meeting1 = new SectionMeeting("11", "1", "LEC", new DateTime(2012, 9, 1), new DateTime(2012, 12, 12), "W") { IsOnline = false };
                onlineSection.AddSectionMeeting(meeting1);
                var meeting2 = new SectionMeeting("12", "1", "ONL", new DateTime(2012, 9, 1), new DateTime(2012, 12, 12), "W") { IsOnline = true };
                onlineSection.AddSectionMeeting(meeting2);
                var meeting3 = new SectionMeeting("13", "1", "LEC", new DateTime(2012, 9, 1), new DateTime(2012, 12, 12), "W") { IsOnline = false };
                onlineSection.AddSectionMeeting(meeting3);
                Assert.AreEqual(OnlineCategory.Hybrid, onlineSection.OnlineCategory);
            }
        }

        [TestClass]
        public class Section_AddSectionCharge
        {
            private SectionCharge sectionCharge;
            private Section sec;

            [TestInitialize]
            public void Section_AddSectionCharge_Initialize()
            {
                var dept = "CS";
                var acadLevel = "UG";
                var courseLevels = new List<string>() { "100" };
                var depts = new List<OfferingDepartment>() { new OfferingDepartment(dept, 100m) };
                var statuses = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddYears(-4)) };
                var course1 = "1";
                var number = "01";
                var startDate = DateTime.Now;
                string title = "Title";
                sec = new Section("1", course1, number, startDate, 4m, 1m, title, "IN", depts, courseLevels, acadLevel, statuses);
                sectionCharge = new SectionCharge("123", "ABC", 50m, true, true);
            }

            [TestMethod]
            public void Section_AddSectionCharge_Books_Count()
            {
                sec.AddSectionCharge(sectionCharge);
                Assert.AreEqual(1, sec.SectionCharges.Count);
                Assert.AreEqual(sectionCharge, sec.SectionCharges[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Section_AddSectionCharge_Throws_Exception_If_SectionCharge_Null()
            {
                sec.AddSectionCharge(null);
            }

            [TestMethod]
            public void Section_AddSectionCharge_Ignores_Duplicate_SectionCharge()
            {
                sec.AddSectionCharge(sectionCharge);
                sec.AddSectionCharge(sectionCharge);
                Assert.AreEqual(1, sec.SectionCharges.Count);
                Assert.AreEqual(sectionCharge, sec.SectionCharges[0]);
            }
        }

        [TestClass]
        public class Section_UpdateMeetingsFromPrimarySection
        {
            List<SectionMeeting> sectionMeetings = new List<SectionMeeting>();
            private Section sec;
            [TestInitialize]
            public void Initialize()
            {
                var dept = "CS";
                var acadLevel = "UG";
                var courseLevels = new List<string>() { "100" };
                var depts = new List<OfferingDepartment>() { new OfferingDepartment(dept, 100m) };
                var statuses = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddYears(-4)) };
                var course1 = "1";
                var number = "01";
                var startDate = DateTime.Now;
                string title = "Title";
                sec = new Section("S001", course1, number, startDate, 4m, 1m, title, "IN", depts, courseLevels, acadLevel, statuses);

                sectionMeetings.Add(new SectionMeeting("1", "S001", "LEC", DateTime.Today, DateTime.Today.AddMonths(2), "2") {Room="R1" });
                sectionMeetings.Add(new SectionMeeting("1", "S001", "LAB", DateTime.Today.AddDays(3), DateTime.Today.AddMonths(5), "3"));
            }

            [TestMethod]
            public void Section_UpdateMeetingsFromPrimarySection_SectionMeetings()
            {
                sec.UpdatePrimarySectionMeetings(sectionMeetings);
                Assert.IsNotNull(sec.PrimarySectionMeetings);
                Assert.AreEqual(2, sec.PrimarySectionMeetings.Count());

                Assert.AreEqual("S001",sec.PrimarySectionMeetings[0].SectionId);
                Assert.AreEqual("LEC",sec.PrimarySectionMeetings[0].InstructionalMethodCode);
                Assert.AreEqual(sectionMeetings[0].StartDate,sec.PrimarySectionMeetings[0].StartDate);
                Assert.AreEqual(sectionMeetings[0].EndDate,sec.PrimarySectionMeetings[0].EndDate);
                Assert.AreEqual("R1",sec.PrimarySectionMeetings[0].Room);

                Assert.AreEqual("S001", sec.PrimarySectionMeetings[1].SectionId);
                Assert.AreEqual("LAB", sec.PrimarySectionMeetings[1].InstructionalMethodCode);
                Assert.AreEqual(sectionMeetings[1].StartDate, sec.PrimarySectionMeetings[1].StartDate);
                Assert.AreEqual(sectionMeetings[1].EndDate, sec.PrimarySectionMeetings[1].EndDate);
                Assert.IsNull(sec.PrimarySectionMeetings[1].Room);
            }

            [TestMethod]
            public void Section_UpdateMeetingsFromPrimarySection_Overwrite_when_SectionMeetings_Already_Exists()
            {

                sec.UpdatePrimarySectionMeetings(new List<SectionMeeting>() { new SectionMeeting("1", "S009", "something", DateTime.Today.AddMonths(-1), DateTime.Today.AddMonths(2), "everyday") { Room = "my room" } });
                sec.UpdatePrimarySectionMeetings(sectionMeetings);
                Assert.IsNotNull(sec.PrimarySectionMeetings);
                Assert.AreEqual(2, sec.PrimarySectionMeetings.Count());

                Assert.AreEqual("S001", sec.PrimarySectionMeetings[0].SectionId);
                Assert.AreEqual("LEC", sec.PrimarySectionMeetings[0].InstructionalMethodCode);
                Assert.AreEqual(sectionMeetings[0].StartDate, sec.PrimarySectionMeetings[0].StartDate);
                Assert.AreEqual(sectionMeetings[0].EndDate, sec.PrimarySectionMeetings[0].EndDate);
                Assert.AreEqual("R1", sec.PrimarySectionMeetings[0].Room);

                Assert.AreEqual("S001", sec.PrimarySectionMeetings[1].SectionId);
                Assert.AreEqual("LAB", sec.PrimarySectionMeetings[1].InstructionalMethodCode);
                Assert.AreEqual(sectionMeetings[1].StartDate, sec.PrimarySectionMeetings[1].StartDate);
                Assert.AreEqual(sectionMeetings[1].EndDate, sec.PrimarySectionMeetings[1].EndDate);
                Assert.IsNull(sec.PrimarySectionMeetings[1].Room);
            }

            [TestMethod]
            public void Section_UpdateMeetingsFromPrimarySection_SectionMeetings_is_Null()
            {

                sec.UpdatePrimarySectionMeetings(new List<SectionMeeting>() { new SectionMeeting("1", "S009", "something", DateTime.Today.AddMonths(-1), DateTime.Today.AddMonths(2), "everyday") { Room = "my room" } });
                sec.UpdatePrimarySectionMeetings(null);
                Assert.IsNotNull(sec.PrimarySectionMeetings);
                Assert.AreEqual(1, sec.PrimarySectionMeetings.Count());
                Assert.AreEqual("S009", sec.PrimarySectionMeetings[0].SectionId);
                Assert.AreEqual("something", sec.PrimarySectionMeetings[0].InstructionalMethodCode);
                Assert.AreEqual(DateTime.Today.AddMonths(-1), sec.PrimarySectionMeetings[0].StartDate);
                Assert.AreEqual(DateTime.Today.AddMonths(2), sec.PrimarySectionMeetings[0].EndDate);
                Assert.AreEqual("my room", sec.PrimarySectionMeetings[0].Room);

            }
            [TestMethod]
            public void Section_UpdateMeetingsFromPrimarySection_SectionMeetings_is_Null_PrimaryIsEmpty()
            {

                sec.UpdatePrimarySectionMeetings(null);
                Assert.IsNotNull(sec.PrimarySectionMeetings);
                Assert.AreEqual(0, sec.PrimarySectionMeetings.Count());
            }

        }
    }
}
