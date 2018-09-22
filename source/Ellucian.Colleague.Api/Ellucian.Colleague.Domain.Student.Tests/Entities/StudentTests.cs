// Copyright 2012-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Base.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    // Tests for the Student Entity
    public class StudentTests
    {
        private string id;
        private string lastName;
        private int degreePlanId;
        private List<string> programIds;
        Student.Entities.Student studentEntity;
        Student.Entities.Student studentEntityNullableParams;
        EmailAddress email;
        private string financialAidCounselorId;
        EmailAddress preferredEmail;

        [TestInitialize]
        public void Initialize()
        {
            id = "0000001";
            lastName = "Smith";
            degreePlanId = 1;
            programIds = new List<string>() { };
            studentEntity = new Student.Entities.Student(id, lastName, degreePlanId, programIds, null);
            email = new EmailAddress("other@yahoo.com", "PER");
            preferredEmail = new EmailAddress("dsmith@yahoo.com", "HOME");
            preferredEmail.IsPreferred = true;
            financialAidCounselorId = "0007575";
            studentEntityNullableParams = new Student.Entities.Student(id, lastName, null, null, null, null);
        }

        [TestCleanup]
        public void Cleanup()
        {
            studentEntity = null;
            studentEntityNullableParams = null;
        }

        [TestClass]
        public class StudentConstructor : StudentTests
        {
            [TestMethod]
            public void Ctor_VerifyStudentIdProp_Set()
            {
                Assert.AreEqual(id, studentEntity.Id);
            }

            [TestMethod]
            public void Ctor_VerifyDegreePlanIdProp_Set()
            {
                Assert.AreEqual(degreePlanId, studentEntity.DegreePlanId);
            }

            [TestMethod]
            public void Ctor_DegreePlanId_CanBeNull()
            {
                Assert.AreEqual(null, studentEntityNullableParams.DegreePlanId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException), "degreePlanId cannot be zero")]
            public void Ctor_DegreePlanId_CannotBeZero()
            {
                new Student.Entities.Student(id, lastName, 0, programIds, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException), "degreePlanId cannot be negative")]
            public void Ctor_DegreePlanId_CannotBeNegative()
            {
                new Student.Entities.Student(id, lastName, -1, programIds, null);
            }

            [TestMethod]
            public void Ctor_VerifyProgramIdsProp_Set()
            {
                Assert.AreEqual(programIds, studentEntity.ProgramIds);
            }

            [TestMethod]
            public void Ctor_VerifyProgramIdsProp_CanBeNull()
            {
                Assert.AreEqual(null, studentEntityNullableParams.ProgramIds);
            }

            [TestMethod]
            public void Student_PreferredEmailAddress()
            {
                studentEntity.AddEmailAddress(email);
                studentEntity.AddEmailAddress(preferredEmail);
                Assert.AreEqual("dsmith@yahoo.com", studentEntity.PreferredEmailAddress.Value);
            }

            [TestMethod]
            public void FinancialAidCounselorId_InitTest()
            {
                Assert.IsTrue(string.IsNullOrEmpty(studentEntity.FinancialAidCounselorId));
            }

            [TestMethod]
            public void FinancialAidCounselor_GetSetTest()
            {
                studentEntity.FinancialAidCounselorId = financialAidCounselorId;
                Assert.AreEqual(financialAidCounselorId, studentEntity.FinancialAidCounselorId);
            }
        }

        [TestClass]
        public class StudentAddStudentRestriction : StudentTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentAddStudentRestriction_Null()
            {
                studentEntity.AddStudentRestriction(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentAddStudentRestriction_Empty()
            {
                studentEntity.AddStudentRestriction(string.Empty);
            }

            [TestMethod]
            public void StudentAddStudentRestriction_Valid()
            {
                studentEntity.AddStudentRestriction("BH");
                Assert.AreEqual(1, studentEntity.StudentRestrictionIds.Count);
                Assert.AreEqual("BH", studentEntity.StudentRestrictionIds[0]);
            }

            [TestMethod]
            public void StudentAddStudentRestriction_Duplicate()
            {
                studentEntity.AddStudentRestriction("BH");
                studentEntity.AddStudentRestriction("LF");
                studentEntity.AddStudentRestriction("BH");
                Assert.AreEqual(2, studentEntity.StudentRestrictionIds.Count);
                Assert.AreEqual("BH", studentEntity.StudentRestrictionIds[0]);
                Assert.AreEqual("LF", studentEntity.StudentRestrictionIds[1]);
            }
        }

        [TestClass][Ignore]
        public class StudentAddAdvisor : StudentTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentAddAdvisor_Null()
            {
                studentEntity.AddAdvisor(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentAddAdvisor_Empty()
            {
                studentEntity.AddAdvisor(string.Empty);
            }

            [TestMethod]
            public void StudentAddAdvisor_Valid()
            {
                studentEntity.AddAdvisor("BH");
                Assert.AreEqual(1, studentEntity.AdvisorIds.Count);
                Assert.AreEqual("BH", studentEntity.AdvisorIds[0]);
            }

            [TestMethod]
            public void StudentAddAdvisor_Duplicate()
            {
                studentEntity.AddAdvisor("BH");
                studentEntity.AddAdvisor("LF");
                studentEntity.AddAdvisor("BH");
                Assert.AreEqual(2, studentEntity.AdvisorIds.Count);
                Assert.AreEqual("BH", studentEntity.AdvisorIds[0]);
                Assert.AreEqual("LF", studentEntity.AdvisorIds[1]);
            }

            [TestMethod]
            public void StudentAddAdvisor_HasAdvisor_False()
            {
                studentEntity = new Student.Entities.Student(id, lastName, degreePlanId, programIds, null);
                Assert.IsFalse(studentEntity.HasAdvisor);
            }
        }

        [TestClass]
        public class StudentAddAdvisement : StudentTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentAddAdvisement_AdvisorId_Null()
            {
                studentEntity.AddAdvisement(null, new DateTime(), null, "AdvisorType");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentAddAdvisor_AdvisorId_Empty()
            {
                studentEntity.AddAdvisement(string.Empty, new DateTime(), null, "AdvisorType");
            }

            [TestMethod]
            public void StudentAddAdvisemenr_Valid()
            {
                studentEntity.AddAdvisement("AdvisorId", null, null, null);
                Assert.AreEqual(1, studentEntity.Advisements.Count);
                Assert.AreEqual("AdvisorId", studentEntity.Advisements[0].AdvisorId);
            }

            [TestMethod]
            public void StudentAddAdvisement_Duplicate()
            {
                studentEntity.AddAdvisement("Advisor1", null, null, null);
                studentEntity.AddAdvisement("Advisor2", new DateTime(), null, "AdvisorType1");
                studentEntity.AddAdvisement("Advisor1", new DateTime(), null, "AdvisorType1");
                studentEntity.AddAdvisement("Advisor1", new DateTime(), null, "AdvisorType2");
                // This should not be added.  Already have Advisor1 for AdvisorType1.
                studentEntity.AddAdvisement("Advisor1", new DateTime(), null, "AdvisorType1");
                // This should not be added.  Already have Advisor1 for null advisor type.
                studentEntity.AddAdvisement("Advisor1", new DateTime(), null, null);

                Assert.AreEqual(4, studentEntity.Advisements.Count);
                Assert.AreEqual("Advisor1", studentEntity.Advisements[0].AdvisorId);
                Assert.AreEqual(null, studentEntity.Advisements[0].AdvisorType);

                Assert.AreEqual("Advisor2", studentEntity.Advisements[1].AdvisorId);
                Assert.AreEqual("AdvisorType1", studentEntity.Advisements[1].AdvisorType);

                Assert.AreEqual("Advisor1", studentEntity.Advisements[2].AdvisorId);
                Assert.AreEqual("AdvisorType1", studentEntity.Advisements[2].AdvisorType);

                Assert.AreEqual("Advisor1", studentEntity.Advisements[3].AdvisorId);
                Assert.AreEqual("AdvisorType2", studentEntity.Advisements[3].AdvisorType);
            }
        }

        [TestClass]
        public class StudentEquals
        {
            private string financialAidCounselorId;
            private Student.Entities.Student studentEntityOne;
            private Student.Entities.Student studentEntityTwo;
            private Student.Entities.Student studentEntityThree;

            [TestInitialize]
            public void Initialize()
            {
                financialAidCounselorId = "0007575";
                studentEntityOne = new Student.Entities.Student("0000001", "Smith", null, null, null, null);
                studentEntityTwo = new Student.Entities.Student("0000001", "Smith", null, null, null, null);
                studentEntityThree = new Student.Entities.Student("0000002", "Jones", null, null, null, null);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentEntityOne = null;
                studentEntityTwo = null;
                studentEntityThree = null;
            }

            [TestMethod]
            public void Equals_StudentEntity_Equal()
            {
                Assert.IsTrue(studentEntityOne.Equals(studentEntityTwo));
            }

            [TestMethod]
            public void DiffFinancialAidCounselorId_EqualTest()
            {
                studentEntityOne.FinancialAidCounselorId = financialAidCounselorId;
                Assert.AreEqual(studentEntityOne, studentEntityTwo);
            }

            [TestMethod]
            public void Equals_StudentEntity_NotEqual()
            {
                Assert.IsFalse(studentEntityOne.Equals(studentEntityThree));
            }
        }

        [TestClass]
        public class StudentGetHashCode
        {
            private string financialAidCounselorId;
            private Student.Entities.Student studentEntityOne;
            private Student.Entities.Student studentEntityTwo;
            private Student.Entities.Student studentEntityThree;

            [TestInitialize]
            public void Initialize()
            {
                financialAidCounselorId = "0007475";
                studentEntityOne = new Student.Entities.Student("0000001", "Smith", null, null, null, null);
                studentEntityTwo = new Student.Entities.Student("0000001", "Smith", null, null, null, null);
                studentEntityThree = new Student.Entities.Student("0000002", "Jones", null, null, null, null);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentEntityOne = null;
                studentEntityTwo = null;
                studentEntityThree = null;
            }

            [TestMethod]
            public void GetHashCode_StudentEntity_Equal()
            {
                Assert.AreEqual(studentEntityOne.GetHashCode(), studentEntityTwo.GetHashCode());
            }

            [TestMethod]
            public void DiffFinancialAidCounselor_EqualHashCodeTest()
            {
                studentEntityOne.FinancialAidCounselorId = financialAidCounselorId;
                Assert.AreEqual(studentEntityOne.GetHashCode(), studentEntityTwo.GetHashCode());
            }

            [TestMethod]
            public void GetHashCode_StudentEntity_NotEqual()
            {
                Assert.AreNotEqual(studentEntityOne.GetHashCode(), studentEntityThree.GetHashCode());
            }
        }

        [TestClass]
        public class StudentAddRegistrationPriorityId
        {
            private string id;
            private string lastName;
            private int degreePlanId;
            private List<string> programIds;
            Student.Entities.Student studentEntity;

            [TestInitialize]
            public void Initialize()
            {
                id = "0000001";
                lastName = "Smith";
                degreePlanId = 1;
                programIds = new List<string>() { };
                studentEntity = new Student.Entities.Student(id, lastName, degreePlanId, programIds, null);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentEntity = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AddRegistrationPriorityId_NullParameter()
            {
                studentEntity.AddRegistrationPriority(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AddRegistrationPriorityId_EmptyString()
            {
                studentEntity.AddRegistrationPriority("");
            }

            [TestMethod]
            public void AddRegistrationPriority_Success()
            {
                studentEntity.AddRegistrationPriority("4");
                Assert.AreEqual(1, studentEntity.RegistrationPriorityIds.Count());
                Assert.IsTrue(studentEntity.RegistrationPriorityIds.Contains("4"));
            }

            [TestMethod]
            public void AddRegistrationPriorityIds_NoDuplicate()
            {
                studentEntity.AddRegistrationPriority("4");
                studentEntity.AddRegistrationPriority("4");
                Assert.AreEqual(1, studentEntity.RegistrationPriorityIds.Count());
            }
        }

        [TestClass]
        public class Student_GetPrimaryLocation
        {
            private string id;
            private string lastName;
            private int degreePlanId;
            private List<string> programIds;
            Student.Entities.Student studentEntity;
            IEnumerable<Student.Entities.StudentProgram> studentPrograms;


            [TestInitialize]
            public void Initialize()
            {
                id = "0000001";
                lastName = "Smith";
                degreePlanId = 1;
                programIds = new List<string>() { "Prog1", "Prog2" };
                studentEntity = new Student.Entities.Student(id, lastName, degreePlanId, programIds, null);
                var sp1 = new Student.Entities.StudentProgram("0000001", "Prog1", "Cat1");
                // First one has start and end dates in the past
                sp1.StartDate = DateTime.Today.AddDays(-3);
                sp1.EndDate = DateTime.Today.AddDays(-1);
                sp1.Location = "PLoc1";
                //Second one has no start date but an end date in the past
                var sp2 = new Student.Entities.StudentProgram("0000001", "Prog2", "Cat2");
                sp2.EndDate = DateTime.Today.AddDays(-1);
                sp2.Location = "PLoc2";
                //Third one has a start date in the future and no end date
                var sp3 = new Student.Entities.StudentProgram("0000001", "Prog3", "Cat3");
                sp3.StartDate = DateTime.Today.AddDays(1);
                sp3.Location = "PLoc3";
                //Fourth one has start date in the past and an end date of today - still means inactive
                var sp4 = new Student.Entities.StudentProgram("0000001", "Prog4", "Cat4");
                sp4.StartDate = DateTime.Today.AddDays(1);
                sp4.Location = "PLoc4";
                //Fifth one has a start date in the past and no end date - considered the first active program
                var sp5 = new Student.Entities.StudentProgram("0000001", "Prog5", "Cat5");
                sp5.StartDate = DateTime.Today.AddDays(-1);
                sp5.Location = "PLoc5";
                //Sixth one has no start date and no end date - considered the second active program
                var sp6 = new Student.Entities.StudentProgram("0000001", "Prog6", "Cat6");
                sp6.Location = "PLoc6";
                studentPrograms = new List<Student.Entities.StudentProgram>() { sp1, sp2, sp3, sp4, sp5, sp6 };

            }

            [TestCleanup]
            public void Cleanup()
            {
                studentEntity = null;
            }

            [TestMethod]
            public void GetPrimaryLocation_FromHomeLocations_NoProgramsProvided()
            {
                // Note: Home locations can't have a null start date.
                studentEntity.AddLocation("Loc1", DateTime.Today.AddDays(-2), DateTime.Now.AddDays(-1), true);
                studentEntity.AddLocation("Loc2", DateTime.Today, DateTime.Now.AddDays(-1), false);
                studentEntity.AddLocation("Loc3", DateTime.Today.AddDays(1), DateTime.Now.AddDays(2), false);
                studentEntity.AddLocation("Loc4", DateTime.Today.AddDays(1), null, false);
                studentEntity.AddLocation("Loc5", DateTime.Today.AddDays(-1), DateTime.Today, false);
                // First real active one is the 6.
                studentEntity.AddLocation("Loc6", DateTime.Today, null, false);
                studentEntity.AddLocation("Loc7", DateTime.Today.AddDays(-1), null, false);
                var location = studentEntity.GetPrimaryLocation(new List<Student.Entities.StudentProgram>());
                Assert.AreEqual("Loc6", location);
            }

            [TestMethod]
            public void GetPrimaryLocation_FromHomeLocations_WhenActiveProgramsProvided()
            {
                // Note: Home locations can't have a null start date.
                studentEntity.AddLocation("Loc1", DateTime.Today.AddDays(-2), DateTime.Now.AddDays(-1), true);
                studentEntity.AddLocation("Loc2", DateTime.Today, DateTime.Now.AddDays(-1), false);
                studentEntity.AddLocation("Loc3", DateTime.Today.AddDays(1), DateTime.Now.AddDays(2), false);
                studentEntity.AddLocation("Loc4", DateTime.Today.AddDays(1), null, false);
                studentEntity.AddLocation("Loc5", DateTime.Today.AddDays(-1), DateTime.Today, false);
                // First real active one is the 6.
                studentEntity.AddLocation("Loc6", DateTime.Today, null, false);
                studentEntity.AddLocation("Loc7", DateTime.Today.AddDays(-1), null, false);
                var location = studentEntity.GetPrimaryLocation(studentPrograms);
                Assert.AreEqual("Loc6", location);
            }

            [TestMethod]
            public void GetPrimaryLocation_FromProgramLocations_NoHomeLocations()
            {
                // Takes the program of the first active one.
                var location = studentEntity.GetPrimaryLocation(studentPrograms);
                Assert.AreEqual("PLoc5", location);
            }

            [TestMethod]
            public void GetPrimaryLocation_FromProgramLocations_NoHomeLocations_WhenProgramLocationIsNull()
            {
                // Takes the program of the first active one.
                var sp1 = new Student.Entities.StudentProgram("0000001", "Prog1", "Cat1");
                // First one has start and end dates in the past
                sp1.StartDate = DateTime.Today.AddDays(-3);
                sp1.EndDate = DateTime.Today.AddDays(-1);
                sp1.Location = "PLoc1";
                //Second one has no start date but an end date in the past
                var sp2 = new Student.Entities.StudentProgram("0000001", "Prog2", "Cat2");
                sp2.EndDate = DateTime.Today.AddDays(-1);
                sp2.Location = "PLoc2";
                //Third one has a start date in the future and no end date
                var sp3 = new Student.Entities.StudentProgram("0000001", "Prog3", "Cat3");
                sp3.StartDate = DateTime.Today.AddDays(1);
                sp3.Location = "PLoc3";
                //Fourth one has start date in the past and an end date of today - still means inactive
                var sp4 = new Student.Entities.StudentProgram("0000001", "Prog4", "Cat4");
                sp4.StartDate = DateTime.Today.AddDays(1);
                sp4.Location = "PLoc4";
                //Fifth one has a start date in the past and no end date - considered the first active program - and location on this is null
                var sp5 = new Student.Entities.StudentProgram("0000001", "Prog5", "Cat5");
                sp5.StartDate = DateTime.Today.AddDays(-1);
                //Sixth one has no start date and no end date - considered the second active program
                var sp6 = new Student.Entities.StudentProgram("0000001", "Prog6", "Cat6");
                sp6.Location = "PLoc6";
                studentPrograms = new List<Student.Entities.StudentProgram>() { sp1, sp2, sp3, sp4, sp5, sp6 };
                var location = studentEntity.GetPrimaryLocation(studentPrograms);
                Assert.IsNull(location);
            }

            [TestMethod]
            public void GetPrimaryLocation_FromProgramLocations_WhenNoApplicableHomeLocations()
            {
                studentEntity.AddLocation("Loc1", DateTime.Today.AddDays(-2), DateTime.Now.AddDays(-1), true);
                studentEntity.AddLocation("Loc2", DateTime.Today, DateTime.Now.AddDays(-1), false);
                studentEntity.AddLocation("Loc3", DateTime.Today.AddDays(1), DateTime.Now.AddDays(2), false);
                studentEntity.AddLocation("Loc4", DateTime.Today.AddDays(1), null, false);
                studentEntity.AddLocation("Loc5", DateTime.Today.AddDays(-1), DateTime.Today, false);
                var location = studentEntity.GetPrimaryLocation(studentPrograms);
                Assert.AreEqual("PLoc5", location);
            }

            [TestMethod]
            public void GetPrimaryLocation_NoHomeLocations_ZeroProgramsProvided()
            {
                // When student doesn't have any home locations and when no active programs are provided the result is primary location is null
                var location = studentEntity.GetPrimaryLocation(new List<Student.Entities.StudentProgram>());
                Assert.IsNull(location);
            }

            [TestMethod]
            public void GetPrimaryLocation_NoHomeLocations_NullProgramsProvided()
            {
                // When student doesn't have any home locations and when null active programs are provided the result is primary location is null
                var location = studentEntity.GetPrimaryLocation(null);
                Assert.IsNull(location);
            }
        }
    }
}
