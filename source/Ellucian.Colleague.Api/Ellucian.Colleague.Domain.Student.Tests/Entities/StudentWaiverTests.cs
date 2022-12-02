// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class StudentWaiverTests
    {
        [TestClass]
        public class StudentWaiver_Constructor
        {
            private string id;
            private string studentId;
            private string courseId;
            private string sectionId;
            private string reasonCode;
            private string comment;
            private string termCode;
            private DateTime? startDate;
            private StudentWaiver waiver;

            [TestInitialize]
            public void Initialize()
            {
                id = "5";
                studentId = "0001234";
                courseId = "01";
                sectionId = "011";
                reasonCode = "LIFE";
                comment = "waiver comment";
                termCode = "2015/SP";
                startDate = DateTime.Today;
            }

            [TestCleanup]
            public void CleanUp()
            {
                waiver = null;
            }

            [TestMethod]
            public void StudentWaiver_Id()
            {
                waiver = new StudentWaiver(id, studentId, null, sectionId, reasonCode, comment, termCode);
                Assert.AreEqual(id, waiver.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void StudentWaiver_ThrowExceptionForAttemptToChangeId()
            {
                waiver = new StudentWaiver(id, studentId, null, sectionId, reasonCode, comment, termCode);
                waiver.Id = "23";
            }

            [TestMethod]
            public void StudentWaiver_UpdatesIdIfNull()
            {
                waiver = new StudentWaiver(null, studentId, null, sectionId, reasonCode, comment, termCode);
                waiver.Id = "23";
                Assert.AreEqual("23", waiver.Id);
            }

            [TestMethod]
            public void StudentWaiver_UpdatesIdIfEmpty()
            {
                waiver = new StudentWaiver(string.Empty, studentId, null, sectionId, reasonCode, comment, termCode);
                waiver.Id = "23";
                Assert.AreEqual("23", waiver.Id);
            }

            [TestMethod]
            public void StudentWaiver_IncomingIdNull_CreatesWaiverWithNullId()
            {
                waiver = new StudentWaiver(null, studentId, null, sectionId, reasonCode, comment, termCode);
                Assert.AreEqual(null, waiver.Id);
            }

            [TestMethod]
            public void StudentWaiver_IncomingIdEmptyString_CreatesWaiverWithEmptyStringId()
            {
                waiver = new StudentWaiver(string.Empty, studentId, null, sectionId, reasonCode, comment, termCode);
                Assert.AreEqual(string.Empty, waiver.Id);
            }

            [TestMethod]
            public void StudentWaiver_StudentId()
            {
                waiver = new StudentWaiver(id, studentId, null, sectionId, reasonCode, comment, termCode);
                Assert.AreEqual(studentId, waiver.StudentId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentWaiver_ThrowsExceptionIfStudentIdNull()
            {
                waiver = new StudentWaiver(id, null, null, sectionId, reasonCode, comment, termCode);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentWaiver_ThrowsExceptionIfStudentIdEmpty()
            {
                waiver = new StudentWaiver(id, string.Empty, null, sectionId, reasonCode, comment, termCode);
            }

            [TestMethod]
            public void StudentWaiver_CourseId()
            {
                waiver = new StudentWaiver(id, studentId, courseId, null, reasonCode, comment, termCode);
                Assert.AreEqual(courseId, waiver.CourseId);
            }

            [TestMethod]
            public void StudentWaiver_SectionId()
            {
                waiver = new StudentWaiver(id, studentId, null, sectionId, reasonCode, comment, termCode);
                Assert.AreEqual(sectionId, waiver.SectionId);
            }

            [TestMethod]
            public void StudentWaiver_TermAndStartDateNotRequiredIfSectionSpecified()
            {
                waiver = new StudentWaiver(id, studentId, null, sectionId, reasonCode, comment, null);
                Assert.AreEqual(sectionId, waiver.SectionId);
                Assert.AreEqual(null, waiver.TermCode);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void StudentWaiver_ThrowsExceptionIfBothCourseIdAndSectionIdPresent()
            {
                waiver = new StudentWaiver(id, studentId, courseId, sectionId, reasonCode, comment, termCode);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void StudentWaiver_ThrowsExceptionIfBothCourseIdAndSectionIdMissing()
            {
                waiver = new StudentWaiver(id, studentId, string.Empty, string.Empty, reasonCode, comment, termCode);
            }

            [TestMethod]
            public void StudentWaiver_ReasonCode()
            {
                waiver = new StudentWaiver(id, studentId, null, sectionId, reasonCode, string.Empty, termCode);
                Assert.AreEqual(reasonCode, waiver.ReasonCode);
            }

            [TestMethod]
            public void StudentWaiver_Comment()
            {
                waiver = new StudentWaiver(id, studentId, null, sectionId, string.Empty, comment, termCode);
                Assert.AreEqual(comment, waiver.Comment);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void StudentWaiver_ThrowsExceptionIfBothReasonCodeAndCommentNull()
            {
                waiver = new StudentWaiver(id, studentId, null, sectionId, null, null, termCode);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void StudentWaiver_ThrowsExceptionIfReasonCodeAndCommentEmpty()
            {
                waiver = new StudentWaiver(id, studentId, null, sectionId, string.Empty, string.Empty, termCode);
            }

            [TestMethod]
            public void StudentWaiver_StartDate()
            {
                waiver = new StudentWaiver(id, studentId, courseId, null, reasonCode, comment, null, startDate);
                Assert.AreEqual(startDate, waiver.StartDate);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void StudentWaiver_ThrowsExceptionIfNoSectionIdAndTermCodeAndStartDateBothUnspecified()
            {
                waiver = new StudentWaiver(id, studentId, courseId, null, reasonCode, comment, null, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void StudentWaiver_ThrowsExceptionIfNoCourseidNoSectionIdAndTermCodeAndStartDateBothUnspecified()
            {
                waiver = new StudentWaiver(id, studentId, null, null, reasonCode, comment, null, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void StudentWaiver_ThrowsExcpetionIfTermCodeAndStartDateBothSpecified()
            {
                waiver = new StudentWaiver(id, studentId, null, sectionId, reasonCode, comment, termCode, startDate);
            }

            [TestMethod]
            public void StudentWaiver_RequisiteWaivers()
            {
                waiver = new StudentWaiver(id, studentId, null, sectionId, reasonCode, comment, termCode);
                Assert.AreEqual(0, waiver.RequisiteWaivers.Count());
            }

        }

        [TestClass]
        public class StudentWaiver_AddRequisiteWaiver
        {
            private string id;
            private string studentId;
            private string courseId;
            private string sectionId;
            private string reasonCode;
            private string comment;
            private string termCode;
            private DateTime? startDate;
            private StudentWaiver waiver;

            [TestInitialize]
            public void Initialize()
            {
                id = "5";
                studentId = "0001234";
                courseId = "01";
                sectionId = "011";
                reasonCode = "LIFE";
                comment = "waiver comment";
                termCode = "2015/SP";
                startDate = DateTime.Today;
                waiver = new StudentWaiver(id, studentId, null, sectionId, reasonCode, comment, termCode, null);
            }

            [TestCleanup]
            public void CleanUp()
            {
                waiver = null;
            }

            [TestMethod]
            public void AddRequisiteWaiver_AddsUniqueRequisiteWaivers()
            {
                var reqId = "987";
                var status = WaiverStatus.Denied;
                RequisiteWaiver reqWaiver = new RequisiteWaiver(reqId, status);
                waiver.AddRequisiteWaiver(reqWaiver);
                Assert.AreEqual(1, waiver.RequisiteWaivers.Count());
                Assert.AreEqual(reqId, waiver.RequisiteWaivers.ElementAt(0).RequisiteId);
                Assert.AreEqual(status, waiver.RequisiteWaivers.ElementAt(0).Status);

                var reqId1 = "654";
                var status1 = WaiverStatus.Waived;
                RequisiteWaiver reqWaiver1 = new RequisiteWaiver(reqId1, status1);
                waiver.AddRequisiteWaiver(reqWaiver1);
                Assert.AreEqual(2, waiver.RequisiteWaivers.Count());
                Assert.AreEqual(reqId1, waiver.RequisiteWaivers.ElementAt(1).RequisiteId);
                Assert.AreEqual(status1, waiver.RequisiteWaivers.ElementAt(1).Status);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void AddRequisiteWaiver_AddExistingRequisiteWaiver_ThrowsException()
            {
                var status = WaiverStatus.Denied;
                var reqId = "987";
                RequisiteWaiver reqWaiver = new RequisiteWaiver(reqId, status);
                waiver.AddRequisiteWaiver(reqWaiver);
                var status1 = WaiverStatus.Waived;
                RequisiteWaiver reqWaiver1 = new RequisiteWaiver(reqId, status1);
                waiver.AddRequisiteWaiver(reqWaiver1);
            }
        }

        [TestClass]
        public class StudentWaiver_Equals
        {
            private string id;
            private string studentId;
            private string courseId;
            private string sectionId;
            private string reasonCode;
            private string comment;
            private string termCode;
            private DateTime? startDate;
            private StudentWaiver waiver;

            [TestInitialize]
            public void Initialize()
            {

            }

            [TestCleanup]
            public void CleanUp()
            {
                waiver = null;
            }

            [TestMethod]
            public void StudentWaiver_Equals_NotEqualIfTargetNull()
            {
                id = "5";
                studentId = "0001234";
                courseId = "01";
                sectionId = "011";
                reasonCode = "LIFE";
                comment = "waiver comment";
                termCode = "2015/SP";
                startDate = DateTime.Today;
                waiver = new StudentWaiver(id, studentId, null, sectionId, reasonCode, comment, termCode);

                StudentWaiver newWaiver = null;
                Assert.IsFalse(waiver.Equals(newWaiver));
            }

            [TestMethod]
            public void StudentWaiver_Equals_NotEqualIfStudentDifferent()
            {
                id = "5";
                studentId = "0001234";
                courseId = "01";
                sectionId = "011";
                reasonCode = "LIFE";
                termCode = "2015/SP";
                startDate = DateTime.Today;
                waiver = new StudentWaiver(id, studentId, null, sectionId, reasonCode, comment, termCode);

                id = "5";
                studentId = "0001255";
                sectionId = "011";
                reasonCode = "LIFE";
                termCode = "2015/SP";
                startDate = DateTime.Today;
                var newWaiver = new StudentWaiver(id, studentId, string.Empty, sectionId, reasonCode, comment, termCode);
                Assert.IsFalse(waiver.Equals(newWaiver));
            }

            [TestMethod]
            public void StudentWaiver_Equals_NotEqualIfSectionDifferent()
            {
                id = "5";
                studentId = "0001234";
                courseId = "01";
                sectionId = "011";
                reasonCode = "LIFE";
                termCode = "2015/SP";
                startDate = DateTime.Today;
                waiver = new StudentWaiver(id, studentId, null, sectionId, reasonCode, comment, termCode);

                id = "5";
                studentId = "0001234";
                sectionId = "022";
                reasonCode = "LIFE";
                termCode = "2015/SP";
                startDate = DateTime.Today;
                var newWaiver = new StudentWaiver(id, studentId, string.Empty, sectionId, reasonCode, comment, termCode);
                Assert.IsFalse(waiver.Equals(newWaiver));
            }

            [TestMethod]
            public void StudentWaiver_Equals_EqualIfSectionSame()
            {
                id = "5";
                studentId = "0001234";
                courseId = "01";
                sectionId = "011";
                reasonCode = "LIFE";
                termCode = "2015/SP";
                startDate = DateTime.Today;
                waiver = new StudentWaiver(id, studentId, null, sectionId, reasonCode, comment, termCode);

                id = "5";
                studentId = "0001234";
                sectionId = "011";
                reasonCode = "OTHER";
                termCode = "2016/SP";
                startDate = DateTime.Today;
                var newWaiver = new StudentWaiver(id, studentId, string.Empty, sectionId, reasonCode, comment, termCode);
                Assert.IsTrue(waiver.Equals(newWaiver));
            }

            [TestMethod]
            public void StudentWaiver_Equals_NotEqualIfCourseDifferent()
            {
                id = "5";
                studentId = "0001234";
                courseId = "01";
                reasonCode = "LIFE";
                termCode = "2015/SP";
                waiver = new StudentWaiver(id, studentId, courseId, string.Empty, reasonCode, comment, termCode);
                id = "5";
                studentId = "0001234";
                courseId = "02";
                reasonCode = "OTHER";
                termCode = "2015/SP";
                startDate = DateTime.Today;
                var newWaiver = new StudentWaiver(id, studentId, courseId, string.Empty, reasonCode, comment, termCode);
                Assert.IsFalse(waiver.Equals(newWaiver));
            }

            [TestMethod]
            public void StudentWaiver_Equals_EqualIfCourseAndTermSame()
            {
                id = "5";
                studentId = "0001234";
                courseId = "01";
                reasonCode = "LIFE";
                termCode = "2015/SP";
                waiver = new StudentWaiver(id, studentId, courseId, string.Empty, reasonCode, comment, termCode);
                id = "5";
                studentId = "0001234";
                courseId = "01";
                reasonCode = "OTHER";
                termCode = "2015/SP";
                startDate = DateTime.Today;
                var newWaiver = new StudentWaiver(id, studentId, courseId, string.Empty, reasonCode, comment, termCode);
                Assert.IsTrue(waiver.Equals(newWaiver));
            }

            [TestMethod]
            public void StudentWaiver_Equals_EqualIfCourseSameAndDatesOverlap()
            {
                id = "5";
                studentId = "0001234";
                courseId = "01";
                reasonCode = "LIFE";
                startDate = new DateTime(2015, 01, 15);
                waiver = new StudentWaiver(id, studentId, courseId, string.Empty, reasonCode, comment, string.Empty, startDate);
                waiver.EndDate = new DateTime(2015, 08, 01);
                id = "5";
                studentId = "0001234";
                courseId = "01";
                reasonCode = "OTHER";
                termCode = "2015/SP";
                startDate = new DateTime(2015, 07, 31);
                var newWaiver = new StudentWaiver(id, studentId, courseId, string.Empty, reasonCode, comment, string.Empty, startDate);
                newWaiver.EndDate = new DateTime(2015, 08, 15);
                Assert.IsTrue(waiver.Equals(newWaiver));
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public void StudentWaiver_Equals_ThrowsExceptionWhenTermVsDates()
            {
                id = "5";
                studentId = "0001234";
                courseId = "01";
                reasonCode = "LIFE";
                termCode = "2015/SP";
                startDate = new DateTime(2015, 01, 15);
                waiver = new StudentWaiver(id, studentId, courseId, string.Empty, reasonCode, comment, termCode);
                waiver.EndDate = new DateTime(2015, 08, 01);
                id = "5";
                studentId = "0001234";
                courseId = "01";
                reasonCode = "OTHER";
                termCode = "2015/SP";
                startDate = new DateTime(2015, 07, 31);
                var newWaiver = new StudentWaiver(id, studentId, courseId, string.Empty, reasonCode, comment, null, startDate);
                newWaiver.EndDate = new DateTime(2015, 08, 15);
                Assert.IsTrue(waiver.Equals(newWaiver));
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public void StudentWaiver_Equals_ThrowsExceptionWhenDatesVsTerm()
            {
                id = "5";
                studentId = "0001234";
                courseId = "01";
                reasonCode = "LIFE";
                termCode = "2015/SP";
                startDate = new DateTime(2015, 01, 15);
                waiver = new StudentWaiver(id, studentId, courseId, string.Empty, reasonCode, comment, null, startDate);
                waiver.EndDate = new DateTime(2015, 08, 01);
                id = "5";
                studentId = "0001234";
                courseId = "01";
                reasonCode = "OTHER";
                termCode = "2015/SP";
                startDate = new DateTime(2015, 07, 31);
                var newWaiver = new StudentWaiver(id, studentId, courseId, string.Empty, reasonCode, comment, termCode);
                newWaiver.EndDate = new DateTime(2015, 08, 15);
                Assert.IsTrue(waiver.Equals(newWaiver));
            }
        }

        [TestClass]
        public class StudentWaiver_ValidateRequisteWaivers
        {
            StudentWaiver waiver;
            Section section;
            Course course;

            [TestInitialize]
            public void Initialize()
            {
                waiver = new StudentWaiver("1", "Student1", "", "SEC1", "OTHER", "This is a waiver comment", "2014/FA");
                waiver.AddRequisiteWaiver(new RequisiteWaiver("R1", WaiverStatus.Denied));
                waiver.AddRequisiteWaiver(new RequisiteWaiver("R5", WaiverStatus.NotSelected));
                waiver.AddRequisiteWaiver(new RequisiteWaiver("R7", WaiverStatus.Waived));

                var depts = new List<OfferingDepartment>() { new OfferingDepartment("HIST") };
                course = new Course("1", "Course title", "Long course title", depts, "HIST", "100", "UG", new List<string>() { "100" }, 3, 0, new List<CourseApproval>());
                course.Requisites = new List<Requisite>()
                    {
                        new Requisite("R1", true, RequisiteCompletionOrder.Previous, true),
                        new Requisite("R2", false, RequisiteCompletionOrder.PreviousOrConcurrent, false),
                        new Requisite("R3", true, RequisiteCompletionOrder.Concurrent, false),
                        new Requisite("R4", true, RequisiteCompletionOrder.PreviousOrConcurrent, true)
                    };

                section = new Section("s1", "c1", "01", DateTime.Today, 3, 0, "section 1", "IN", depts, new List<string> { "100" }, "ug", new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "X", DateTime.Today) });
                section.OverridesCourseRequisites = true;
                section.Requisites = new List<Requisite>()
                {
                        new Requisite("R4", true, RequisiteCompletionOrder.Previous, false),
                        new Requisite("R5", false, RequisiteCompletionOrder.PreviousOrConcurrent, false),
                        new Requisite("R6", true, RequisiteCompletionOrder.Concurrent, false),
                        new Requisite("R7", true, RequisiteCompletionOrder.PreviousOrConcurrent, false)
                };
            }

            [TestMethod]
            public void AllRequisiteWaiversAreOk()
            {
                waiver.ValidateRequisiteWaivers(section, course);        
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public void ThrowsExceptionIfNoRequisitesInWaiver()
            {
                var waiver1 = new StudentWaiver("1", "Student1", "", "SEC1", "OTHER", "This is a waiver comment", "2014/FA");
                waiver1.ValidateRequisiteWaivers(section, course);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public void ThrowsExceptionIfNoWaiverableRequisitesInSection()
            {
                course.Requisites = new List<Requisite>();
                section.Requisites = new List<Requisite>()
                {
                        new Requisite("R4", false, RequisiteCompletionOrder.Previous, false),
                        new Requisite("R6", true, RequisiteCompletionOrder.Concurrent, false),
                };
                waiver.ValidateRequisiteWaivers(section, course);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public void ThrowsExceptionIfUnwaiverableRequisitesInWaiver()
            {
                waiver.AddRequisiteWaiver(new RequisiteWaiver("R2", WaiverStatus.Waived));
                waiver.ValidateRequisiteWaivers(section, course);
            }

            [TestMethod]
            public void DoesNotThrowExceptionForUnwaiverableRequisitesWithNoAction()
            {
                waiver.AddRequisiteWaiver(new RequisiteWaiver("R2", WaiverStatus.NotSelected));
                waiver.ValidateRequisiteWaivers(section, course);                
            }
        }
    }
}
