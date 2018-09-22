// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class SectionPermissionTests
    {
        [TestClass]
        public class SectionPermission_Constructor
        {
            private string sectionId;
            private SectionPermission sectionPermission;

            [TestInitialize]
            public void Initialize()
            {
                sectionId = "5";

            }

            [TestCleanup]
            public void CleanUp()
            {
                sectionPermission = null;
            }

            [TestMethod]
            public void SectionPermission_Id()
            {
                sectionPermission = new SectionPermission(sectionId);
                Assert.AreEqual(sectionId, sectionPermission.SectionId);
                Assert.AreEqual(0, sectionPermission.FacultyConsents.Count());
                Assert.AreEqual(0, sectionPermission.StudentPetitions.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SectionPermission_NullIdThrowsException()
            {
                sectionPermission = new SectionPermission(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SectionPermission_EmptyIdThrowsException()
            {
                sectionPermission = new SectionPermission(string.Empty);
            }
        }

        [TestClass]
        public class sectionPermission_AddStudentPetition
        {
            private string sectionId;
            private string sectionId2;
            private string studentId;
            private string statusCode;
            private StudentPetition studentPetition;
            private SectionPermission sectionPermission;
            private StudentPetitionType type;

            [TestInitialize]
            public void Initialize()
            {
                sectionId = "5";
                sectionId2 = "6";
                studentId = "studentId";
                statusCode = "ACC";
                type = StudentPetitionType.StudentPetition;
                studentPetition = new StudentPetition("petitionId", null, sectionId, studentId, type, statusCode);
                sectionPermission = new SectionPermission(sectionId);
            }

            [TestCleanup]
            public void CleanUp()
            {
                sectionPermission = null;
                studentPetition = null;
            }

            [TestMethod]
            public void AddStudentPetition_AddsUniqueStudentPetitions()
            {
                sectionPermission.AddStudentPetition(studentPetition);
                Assert.AreEqual(1, sectionPermission.StudentPetitions.Count());
                Assert.AreEqual(sectionId, sectionPermission.StudentPetitions.ElementAt(0).SectionId);
                Assert.AreEqual(studentId, sectionPermission.StudentPetitions.ElementAt(0).StudentId);
                Assert.AreEqual(statusCode, sectionPermission.StudentPetitions.ElementAt(0).StatusCode);

                StudentPetition sectionPetition2 = new StudentPetition("petitionId", null, sectionId, "studentId2", type, statusCode);
                sectionPermission.AddStudentPetition(sectionPetition2);
                Assert.AreEqual(2, sectionPermission.StudentPetitions.Count());
                Assert.AreEqual(sectionId, sectionPermission.StudentPetitions.ElementAt(1).SectionId);
                Assert.AreEqual("studentId2", sectionPermission.StudentPetitions.ElementAt(1).StudentId);
                Assert.AreEqual(statusCode, sectionPermission.StudentPetitions.ElementAt(1).StatusCode);
                Assert.AreEqual(type, sectionPermission.StudentPetitions.ElementAt(1).Type);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void AddStudentPetition_AddExistingStudentPetition_ThrowsException()
            {
                sectionPermission.AddStudentPetition(studentPetition);
                sectionPermission.AddStudentPetition(studentPetition);
            }

            [TestMethod]
            public void AddStudentPetition_DoesNotAddNullStudentPetition()
            {
                sectionPermission.AddStudentPetition(studentPetition);
                Assert.AreEqual(1, sectionPermission.StudentPetitions.Count());
                Assert.AreEqual(sectionId, sectionPermission.StudentPetitions.ElementAt(0).SectionId);
                Assert.AreEqual(statusCode, sectionPermission.StudentPetitions.ElementAt(0).StatusCode);

                sectionPermission.AddStudentPetition(null);
                Assert.AreEqual(1, sectionPermission.StudentPetitions.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void AddStudentPetition_AddingPetitionWithWrongSection_ThrowsException()
            {
                StudentPetition sectionPetition3 = new StudentPetition("petitionId", null, sectionId2, "studentId2", type, statusCode) { Comment = "Petition comment", ReasonCode = "RCODE" };
                sectionPermission.AddStudentPetition(sectionPetition3);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void AddStudentPetition_AddingPetitionWithWrongType_ThrowsException()
            {
                StudentPetition sectionPetition3 = new StudentPetition("petitionId", null, sectionId2, "studentId2", StudentPetitionType.FacultyConsent, statusCode) { Comment = "Petition comment", ReasonCode = "RCODE" };
                sectionPermission.AddStudentPetition(sectionPetition3);
            }
        }

        [TestClass]
        public class sectionPermission_AddFacultyConsent
        {
            private string sectionId;
            private string sectionId2;
            private string studentId;
            private string statusCode;
            private StudentPetition studentPetition;
            private SectionPermission sectionPermission;
            private StudentPetitionType type;

            [TestInitialize]
            public void Initialize()
            {
                sectionId = "5";
                sectionId2 = "6";
                studentId = "studentId";
                statusCode = "ACC";
                type = StudentPetitionType.FacultyConsent;
                studentPetition = new StudentPetition("petitionId", null, sectionId, studentId, type, statusCode);
                sectionPermission = new SectionPermission(sectionId);
            }

            [TestCleanup]
            public void CleanUp()
            {
                sectionPermission = null;
                studentPetition = null;
            }

            [TestMethod]
            public void AddFacultyConsent_AddsUniqueStudentPetitions()
            {
                sectionPermission.AddFacultyConsent(studentPetition);
                Assert.AreEqual(1, sectionPermission.FacultyConsents.Count());
                Assert.AreEqual(sectionId, sectionPermission.FacultyConsents.ElementAt(0).SectionId);
                Assert.AreEqual(studentId, sectionPermission.FacultyConsents.ElementAt(0).StudentId);
                Assert.AreEqual(statusCode, sectionPermission.FacultyConsents.ElementAt(0).StatusCode);

                StudentPetition sectionPetition2 = new StudentPetition("petitionId", null, sectionId, "studentId2", type, statusCode);
                sectionPermission.AddFacultyConsent(sectionPetition2);
                Assert.AreEqual(0, sectionPermission.StudentPetitions.Count());
                Assert.AreEqual(2, sectionPermission.FacultyConsents.Count());
                Assert.AreEqual(sectionId, sectionPermission.FacultyConsents.ElementAt(1).SectionId);
                Assert.AreEqual("studentId2", sectionPermission.FacultyConsents.ElementAt(1).StudentId);
                Assert.AreEqual(statusCode, sectionPermission.FacultyConsents.ElementAt(1).StatusCode);
                Assert.AreEqual(type, sectionPermission.FacultyConsents.ElementAt(1).Type);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void AddFacultyConsent_AddExistingStudentPetition_ThrowsException()
            {
                sectionPermission.AddFacultyConsent(studentPetition);
                sectionPermission.AddFacultyConsent(studentPetition);
            }

            [TestMethod]
            public void AddFacultyConsent_DoesNotAddNullStudentPetition()
            {
                sectionPermission.AddFacultyConsent(studentPetition);
                Assert.AreEqual(0, sectionPermission.StudentPetitions.Count());
                Assert.AreEqual(1, sectionPermission.FacultyConsents.Count());
                Assert.AreEqual(sectionId, sectionPermission.FacultyConsents.ElementAt(0).SectionId);
                Assert.AreEqual(statusCode, sectionPermission.FacultyConsents.ElementAt(0).StatusCode);

                sectionPermission.AddFacultyConsent(null);
                Assert.AreEqual(1, sectionPermission.FacultyConsents.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void AddFacultyConsent_AddingPetitionWithWrongSection_ThrowsException()
            {
                StudentPetition sectionPetition3 = new StudentPetition("petitionId", null, sectionId2, "studentId2", type, statusCode) { Comment = "Petition comment", ReasonCode = "RCODE" };
                sectionPermission.AddFacultyConsent(sectionPetition3);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void AddFacultyConsent_AddingPetitionWithWrongType_ThrowsException()
            {
                StudentPetition sectionPetition3 = new StudentPetition("petitionId", null, sectionId2, "studentId2", StudentPetitionType.StudentPetition, statusCode) { Comment = "Petition comment", ReasonCode = "RCODE" };
                sectionPermission.AddFacultyConsent(sectionPetition3);
            }
        }
    }
}
