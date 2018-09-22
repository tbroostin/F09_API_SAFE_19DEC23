// Copyright 2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class StudentSectionsAttendancesTests
    {
        [TestClass]
        public class StudentSectionsAttendances_Constructor
        {

            [TestInitialize]
            public void Initialize()
            {
            }

            [TestCleanup]
            public void CleanUp()
            {
            }


            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentSectionAttendance_ThrowArgumentNullException_StudenIdNull()
            {
                StudentSectionsAttendances studentSectionAttendances = new StudentSectionsAttendances(null);
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentSectionAttendance_ThrowArgumentNullException_StudenIdIsEmpty()
            {
                StudentSectionsAttendances studentSectionAttendances = new StudentSectionsAttendances(string.Empty);
            }
            [TestMethod]
            public void StudentSectionAttendance_ThrowArgumentNullException_StudentId_IsNotNull()
            {
                StudentSectionsAttendances studentSectionAttendances = new StudentSectionsAttendances("2345");
                Assert.AreEqual("2345", studentSectionAttendances.StudentId);
                Assert.IsNotNull(studentSectionAttendances.SectionWiseAttendances);
                Assert.AreEqual(0, studentSectionAttendances.SectionWiseAttendances.Count);
            }



        }

        [TestClass]
        public class StudentSectionsAttendances_AddStudentAttendances
        {
            List<StudentAttendance> attendancesMultiplePerSection = new List<StudentAttendance>();
            List<StudentAttendance> attendancesOnePerSection = new List<StudentAttendance>();
            [TestInitialize]
            public void Initialize()
            {
                //attendance collection with more than 1 attendance for each section
                attendancesMultiplePerSection.Add(new StudentAttendance("0001234", "1111", new DateTime(2018, 01, 01), "P", null, "good job")
                {
                    EndTime = DateTime.Now.AddHours(-1),
                    StartTime = DateTime.Now.AddHours(-3),
                    InstructionalMethod = "LAB",
                    StudentCourseSectionId = "67890"
                });

                attendancesMultiplePerSection.Add(new StudentAttendance("0001234", "1111", new DateTime(2018, 01, 02), "A", null, "")
                {
                    EndTime = DateTime.Now.AddHours(-1),
                    StartTime = DateTime.Now.AddHours(-3),
                    InstructionalMethod = "LEC",
                    StudentCourseSectionId = "67890"
                });
                attendancesMultiplePerSection.Add(new StudentAttendance("0001234", "1112", new DateTime(2018, 01, 01), null, 23, "good job again")
                {
                    EndTime = DateTime.Now.AddHours(-1),
                    StartTime = DateTime.Now.AddHours(-3),
                    InstructionalMethod = "LAB",
                    StudentCourseSectionId = "67891"
                });
                attendancesMultiplePerSection.Add(new StudentAttendance("0001234", "1112", new DateTime(2018, 01, 02), "L", null, "come regularly")
                {
                    EndTime = DateTime.Now.AddHours(-1),
                    StartTime = DateTime.Now.AddHours(-3),
                    InstructionalMethod = "LEC",
                    StudentCourseSectionId = "67891"
                });


                //attendance collection with only one entry for each section

                attendancesOnePerSection.Add(new StudentAttendance("0001234", "1111", new DateTime(2018, 01, 01), "P", null, "good job")
                {
                    EndTime = DateTime.Now.AddHours(-1),
                    StartTime = DateTime.Now.AddHours(-3),
                    InstructionalMethod = "LAB",
                    StudentCourseSectionId = "67890"
                });


                attendancesOnePerSection.Add(new StudentAttendance("0001234", "1112", new DateTime(2018, 01, 01), null, 23, "good job again")
                {
                    EndTime = DateTime.Now.AddHours(-1),
                    StartTime = DateTime.Now.AddHours(-3),
                    InstructionalMethod = "LAB",
                    StudentCourseSectionId = "67891"
                });

            }
            [TestCleanup]
            public void Cleanup()
            {

            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentSectionsAttendances_AddStudentAttendances_PassNull()
            {
                StudentSectionsAttendances att = new StudentSectionsAttendances("s001");
                att.AddStudentAttendances(null);
            }

            [TestMethod]
            public void StudentSectionsAttendances_AddStudentAttendances_PassEmptyList()
            {
                StudentSectionsAttendances att = new StudentSectionsAttendances("s001");
                att.AddStudentAttendances(new List<StudentAttendance>());
                Assert.AreEqual("s001", att.StudentId);
                Assert.AreEqual(0, att.SectionWiseAttendances.Count);
            }
            [TestMethod]
            public void StudentSectionsAttendances_AddStudentAttendances_MulitpleForASection()
            {
                StudentSectionsAttendances att = new StudentSectionsAttendances("s001");
                att.AddStudentAttendances(attendancesMultiplePerSection);
                Assert.AreEqual("s001", att.StudentId);
                Assert.AreEqual(2, att.SectionWiseAttendances.Count);
                Assert.AreEqual(2, att.SectionWiseAttendances["1111"].Count);
                Assert.AreEqual(2, att.SectionWiseAttendances["1112"].Count);
                //first element for  first key - 1111
                Assert.AreEqual("67890", att.SectionWiseAttendances["1111"][0].StudentCourseSectionId);
                Assert.AreEqual("P", att.SectionWiseAttendances["1111"][0].AttendanceCategoryCode);
                Assert.AreEqual("good job", att.SectionWiseAttendances["1111"][0].Comment);
                Assert.AreEqual("LAB", att.SectionWiseAttendances["1111"][0].InstructionalMethod);
                Assert.AreEqual("0001234", att.SectionWiseAttendances["1111"][0].StudentId);
                Assert.AreEqual("1111", att.SectionWiseAttendances["1111"][0].SectionId);
                //2nd element for  first key - 1111
                Assert.AreEqual("67890", att.SectionWiseAttendances["1111"][1].StudentCourseSectionId);
                Assert.AreEqual("A", att.SectionWiseAttendances["1111"][1].AttendanceCategoryCode);
                Assert.AreEqual(string.Empty, att.SectionWiseAttendances["1111"][1].Comment);
                Assert.AreEqual("LEC", att.SectionWiseAttendances["1111"][1].InstructionalMethod);
                Assert.AreEqual("0001234", att.SectionWiseAttendances["1111"][1].StudentId);
                Assert.AreEqual("1111", att.SectionWiseAttendances["1111"][1].SectionId);
                //first element for  2nd key - 1112
                Assert.AreEqual("67891", att.SectionWiseAttendances["1112"][0].StudentCourseSectionId);
                Assert.AreEqual(null, att.SectionWiseAttendances["1112"][0].AttendanceCategoryCode);
                Assert.AreEqual("good job again", att.SectionWiseAttendances["1112"][0].Comment);
                Assert.AreEqual("LAB", att.SectionWiseAttendances["1112"][0].InstructionalMethod);
                Assert.AreEqual("0001234", att.SectionWiseAttendances["1112"][0].StudentId);
                Assert.AreEqual("1112", att.SectionWiseAttendances["1112"][0].SectionId);
                //2nd element for  2nd key - 1112
                Assert.AreEqual("67891", att.SectionWiseAttendances["1112"][1].StudentCourseSectionId);
                Assert.AreEqual("L", att.SectionWiseAttendances["1112"][1].AttendanceCategoryCode);
                Assert.AreEqual("come regularly", att.SectionWiseAttendances["1112"][1].Comment);
                Assert.AreEqual("LEC", att.SectionWiseAttendances["1112"][1].InstructionalMethod);
                Assert.AreEqual("0001234", att.SectionWiseAttendances["1112"][1].StudentId);
                Assert.AreEqual("1112", att.SectionWiseAttendances["1112"][1].SectionId);

            }

            [TestMethod]
            public void StudentSectionsAttendances_AddStudentAttendances_OneForASection()
            {
                StudentSectionsAttendances att = new StudentSectionsAttendances("s001");
                att.AddStudentAttendances(attendancesOnePerSection);
                Assert.AreEqual("s001", att.StudentId);
                Assert.AreEqual(2, att.SectionWiseAttendances.Count);
                Assert.AreEqual(1, att.SectionWiseAttendances["1111"].Count);
                Assert.AreEqual(1, att.SectionWiseAttendances["1112"].Count);
                //first element for  first key - 1111
                Assert.AreEqual("67890", att.SectionWiseAttendances["1111"][0].StudentCourseSectionId);
                Assert.AreEqual("P", att.SectionWiseAttendances["1111"][0].AttendanceCategoryCode);
                Assert.AreEqual("good job", att.SectionWiseAttendances["1111"][0].Comment);
                Assert.AreEqual("LAB", att.SectionWiseAttendances["1111"][0].InstructionalMethod);
                Assert.AreEqual("0001234", att.SectionWiseAttendances["1111"][0].StudentId);
                Assert.AreEqual("1111", att.SectionWiseAttendances["1111"][0].SectionId);
              
                //first element for  2nd key - 1112
                Assert.AreEqual("67891", att.SectionWiseAttendances["1112"][0].StudentCourseSectionId);
                Assert.AreEqual(null, att.SectionWiseAttendances["1112"][0].AttendanceCategoryCode);
                Assert.AreEqual("good job again", att.SectionWiseAttendances["1112"][0].Comment);
                Assert.AreEqual("LAB", att.SectionWiseAttendances["1112"][0].InstructionalMethod);
                Assert.AreEqual("0001234", att.SectionWiseAttendances["1112"][0].StudentId);
                Assert.AreEqual("1112", att.SectionWiseAttendances["1112"][0].SectionId);
               
            }

        }
    }
}