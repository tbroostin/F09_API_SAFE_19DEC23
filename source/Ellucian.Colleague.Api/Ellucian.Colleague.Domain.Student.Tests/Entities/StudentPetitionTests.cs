// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class StudentPetitionTests
    {
        [TestClass]
        public class StudentPetition_Constructor
        {
            private string id;
            private string studentId;
            private string courseId;
            private string sectionId;
            private string statusCode;
            private string reasonCode;
            private string comment;
            private string termCode;
            private DateTime? startDate;
            private StudentPetition petition;

            [TestInitialize]
            public void Initialize()
            {
                id = "5";
                studentId = "0001234";
                statusCode = "ACC";
                courseId = "01";
                sectionId = "011";
                reasonCode = "ACC";
                comment = "petition comment";
                termCode = "2015/SP";
                startDate = DateTime.Today;
            }

            [TestCleanup]
            public void CleanUp()
            {
                petition = null;
            }

            [TestMethod]
            public void StudentPetition_Id()
            {
                petition = new StudentPetition(id, null, sectionId, studentId, StudentPetitionType.StudentPetition, statusCode) { ReasonCode = reasonCode, Comment = comment};
                Assert.AreEqual(id, petition.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void StudentPetition_ThrowExceptionForAttemptToChangeId()
            {
                petition = new StudentPetition(id, null, sectionId, studentId, StudentPetitionType.StudentPetition, statusCode);
                petition.Id = "23";
            }

            [TestMethod]
            public void StudentPetition_UpdatesIdIfNull()
            {
                petition = new StudentPetition(null, null, sectionId, studentId, StudentPetitionType.StudentPetition, statusCode) { ReasonCode = reasonCode, Comment = comment };
                petition.Id = "23";
                Assert.AreEqual("23", petition.Id);
            }

            [TestMethod]
            public void StudentPetition_UpdatesIdIfEmpty()
            {
                petition = new StudentPetition(string.Empty, null, sectionId, studentId, StudentPetitionType.StudentPetition, statusCode) { ReasonCode = reasonCode, Comment = comment };
                petition.Id = "23";
                Assert.AreEqual("23", petition.Id);
            }

            [TestMethod]
            public void StudentPetition_IncomingIdNull_CreatesPetitionWithNullId()
            {
                petition = new StudentPetition(null, null, sectionId, studentId, StudentPetitionType.StudentPetition, statusCode) { ReasonCode = reasonCode, Comment = comment };
                Assert.AreEqual(null, petition.Id);
            }

            [TestMethod]
            public void StudentPetition_IncomingIdEmptyString_CreatesPetitionWithEmptyStringId()
            {
                petition = new StudentPetition(string.Empty, null, sectionId, studentId, StudentPetitionType.StudentPetition, statusCode) { ReasonCode = reasonCode, Comment = comment };
                Assert.AreEqual(string.Empty, petition.Id);
            }

            [TestMethod]
            public void StudentPetition_StudentId()
            {
                petition = new StudentPetition(id, null, sectionId, studentId, StudentPetitionType.StudentPetition, statusCode) { ReasonCode = reasonCode, Comment = comment };
                Assert.AreEqual(studentId, petition.StudentId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentPetition_ThrowsExceptionIfStudentIdNull()
            {
                petition = new StudentPetition(id, null, sectionId, null, StudentPetitionType.StudentPetition, statusCode);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentPetition_ThrowsExceptionIfStudentIdEmpty()
            {
                petition = new StudentPetition(id, null, sectionId, string.Empty, StudentPetitionType.StudentPetition, statusCode);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentPetition_ThrowsExceptionIfStatusCodeEmpty()
            {
                petition = new StudentPetition(id, null, sectionId, studentId, StudentPetitionType.StudentPetition, string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentPetition_ThrowsExceptionIfStatusCodeNull()
            {
                petition = new StudentPetition(id, null, sectionId, studentId, StudentPetitionType.StudentPetition, null);
            }

            [TestMethod]
            public void StudentPetition_CourseId()
            {
                petition = new StudentPetition(id, courseId, null, studentId, StudentPetitionType.StudentPetition, statusCode);
                Assert.AreEqual(courseId, petition.CourseId);
            }

            [TestMethod]
            public void StudentPetition_SectionId()
            {
                petition = new StudentPetition(id, null, sectionId, studentId, StudentPetitionType.StudentPetition, statusCode);
                Assert.AreEqual(sectionId, petition.SectionId);
            }


            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void StudentPetition_ThrowsExceptionIfBothCourseIdAndSectionIdMissing()
            {
                petition = new StudentPetition(id, string.Empty, string.Empty, studentId, StudentPetitionType.StudentPetition, statusCode);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void StudentPetition_ThrowsExceptionIfBothCourseIdAndSectionNull()
            {
                petition = new StudentPetition(id, null, null, studentId, StudentPetitionType.StudentPetition, statusCode);
            }

            [TestMethod]
            public void StudentPetition_ReasonCode()
            {
                petition = new StudentPetition(id, null, sectionId, studentId, StudentPetitionType.StudentPetition, statusCode) { ReasonCode = reasonCode, Comment = comment };
                Assert.AreEqual(reasonCode, petition.ReasonCode);
            }

            [TestMethod]
            public void StudentPetition_Comment()
            {
                petition = new StudentPetition(id, null, sectionId, studentId, StudentPetitionType.StudentPetition, statusCode) { ReasonCode = reasonCode, Comment = comment };
                Assert.AreEqual(comment, petition.Comment);
            }

            [TestMethod]
            public void StudentPetition_TermCode()
            {
                petition = new StudentPetition(id, courseId, null, studentId, StudentPetitionType.StudentPetition, statusCode) { ReasonCode = reasonCode, Comment = comment, TermCode = termCode };
                Assert.AreEqual(termCode, petition.TermCode);
            }

            [TestMethod]
            public void StudentPetition_StatusCode()
            {
                petition = new StudentPetition(id, courseId, null, studentId, StudentPetitionType.StudentPetition, statusCode);
                Assert.AreEqual(statusCode, petition.StatusCode);
            }

            [TestMethod]
            public void StudentPetition_Type_StudentPetition()
            {
                petition = new StudentPetition(id, courseId, null, studentId, StudentPetitionType.StudentPetition, statusCode);
                Assert.AreEqual(StudentPetitionType.StudentPetition, petition.Type);
            }

            [TestMethod]
            public void StudentPetition_Type_FacultyConsent()
            {
                petition = new StudentPetition(id, courseId, null, studentId, StudentPetitionType.FacultyConsent, statusCode);
                Assert.AreEqual(StudentPetitionType.FacultyConsent, petition.Type);
            }
        }

        [TestClass]
        public class StudentPetition_Equals
        {
            private string id;
            private string studentId;
            private string courseId;
            private string sectionId;
            private string reasonCode;
            private string comment;
            private string termCode;
            private string statusCode;
            private DateTime? startDate;
            private StudentPetition petition;

            [TestInitialize]
            public void Initialize()
            {

            }

            [TestCleanup]
            public void CleanUp()
            {
                petition = null;
            }

            [TestMethod]
            public void StudentPetition_Equals_NotEqualIfTargetNull()
            {
                id = "5";
                studentId = "0001234";
                sectionId = "011";
                reasonCode = "LIFE";
                statusCode = "status";
                startDate = DateTime.Today;
                petition = new StudentPetition(id, null, sectionId, studentId, StudentPetitionType.StudentPetition, statusCode);

                StudentPetition newPetition = null;
                Assert.IsFalse(petition.Equals(newPetition));
            }

            [TestMethod]
            public void StudentPetition_Equals_NotEqualIfStudentDifferent()
            {
                id = "5";
                studentId = "0001234";
                sectionId = "011";
                statusCode = "status";
                petition = new StudentPetition(id, null, sectionId, studentId, StudentPetitionType.StudentPetition, statusCode);

                id = "5";
                studentId = "0001255";
                sectionId = "011";
                statusCode = "status";
                var newPetition = new StudentPetition(id, null, sectionId, studentId, StudentPetitionType.StudentPetition, statusCode);
                Assert.IsFalse(petition.Equals(newPetition));
            }

            [TestMethod]
            public void StudentPetition_Equals_NotEqualIfSectionDifferent()
            {
                id = "5";
                studentId = "0001234";
                sectionId = "011";
                statusCode = "status";
                petition = new StudentPetition(id, null, sectionId, studentId, StudentPetitionType.StudentPetition, statusCode);

                id = "5";
                studentId = "0001234";
                sectionId = "022";
                var newPetition = new StudentPetition(id, null, sectionId, studentId, StudentPetitionType.StudentPetition, statusCode);
                Assert.IsFalse(petition.Equals(newPetition));
            }

            [TestMethod]
            public void StudentPetition_Equals_EqualIfSectionSame()
            {
                id = "5";
                studentId = "0001234";
                sectionId = "011";
                statusCode = "status";
                petition = new StudentPetition(id, null, sectionId, studentId, StudentPetitionType.StudentPetition, statusCode);

                id = "5";
                studentId = "0001234";
                sectionId = "011";
                var newPetition = new StudentPetition(id, null, sectionId, studentId, StudentPetitionType.StudentPetition, statusCode);
                Assert.IsTrue(petition.Equals(newPetition));
            }

            [TestMethod]
            public void StudentPetition_Equals_NotEqualIfCourseDifferent()
            {
                id = "5";
                studentId = "0001234";
                courseId = "01";
                statusCode = "status";
                petition = new StudentPetition(id, courseId, null, studentId, StudentPetitionType.StudentPetition, statusCode);
                id = "5";
                studentId = "0001234";
                courseId = "02";
                var newPetition = new StudentPetition(id, courseId, null, studentId, StudentPetitionType.StudentPetition, statusCode);
                Assert.IsFalse(petition.Equals(newPetition));
            }

            [TestMethod]
            public void StudentPetition_Equals_EqualIfCourseAndTermSame()
            {
                id = "5";
                studentId = "0001234";
                courseId = "01";
                statusCode = "status";
                termCode = "term1";
                petition = new StudentPetition(id, courseId, null, studentId, StudentPetitionType.StudentPetition, statusCode);
                petition.TermCode = termCode;
                id = "5";
                studentId = "0001234";
                courseId = "01";
                termCode = "term1";
                startDate = DateTime.Today;
                var newPetition = new StudentPetition(id, courseId, null, studentId, StudentPetitionType.StudentPetition, statusCode);
                newPetition.TermCode = termCode;
                Assert.IsTrue(petition.Equals(newPetition));
            }

            
            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public void StudentPetition_Equals_ThrowsExceptionWhenTermVsDates()
            {
                id = "5";
                studentId = "0001234";
                courseId = "01";
                reasonCode = "LIFE";
                termCode = "2015/SP";
                startDate = new DateTime(2015, 01, 15);
                statusCode = "status";
                petition = new StudentPetition(id, courseId, null, studentId, StudentPetitionType.StudentPetition, statusCode) { ReasonCode = reasonCode, Comment = comment };
                petition.EndDate = new DateTime(2015, 08, 01);
                id = "5";
                studentId = "0001234";
                courseId = "01";
                reasonCode = "OTHER";
                termCode = "2015/SP";
                startDate = new DateTime(2015, 07, 31);
                var newPetition = new StudentPetition(id, courseId, null, studentId, StudentPetitionType.StudentPetition, statusCode) { ReasonCode = reasonCode, Comment = comment };
                newPetition.EndDate = new DateTime(2015, 08, 15);
                Assert.IsTrue(petition.Equals(newPetition));
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public void StudentPetition_Equals_ThrowsExceptionWhenDatesVsTerm()
            {
                id = "5";
                studentId = "0001234";
                courseId = "01";
                reasonCode = "LIFE";
                termCode = "2015/SP";
                startDate = new DateTime(2015, 01, 15);
                statusCode = "status";
                petition = new StudentPetition(id, courseId, null, studentId, StudentPetitionType.StudentPetition, statusCode) { ReasonCode = reasonCode, Comment = comment };
                petition.EndDate = new DateTime(2015, 08, 01);
                id = "5";
                studentId = "0001234";
                courseId = "01";
                reasonCode = "OTHER";
                termCode = "2015/SP";
                startDate = new DateTime(2015, 07, 31);
                var newPetition = new StudentPetition(id, courseId, null, studentId, StudentPetitionType.StudentPetition, statusCode) { ReasonCode = reasonCode, Comment = comment };
                newPetition.EndDate = new DateTime(2015, 08, 15);
                Assert.IsTrue(petition.Equals(newPetition));
            }
        }
    }
}