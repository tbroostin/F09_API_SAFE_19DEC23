// Copyright 2015-2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Planning.Tests.Entities
{
    [TestClass]
    // Tests for the PlanningStudent Entity
    public class PlanningStudentTests
    {
        private string id;
        private string lastName;
        private int degreePlanId;
        private List<string> programIds;
        Student.Entities.PlanningStudent planningStudentEntity;
        Student.Entities.PlanningStudent planningStudentEntityNullableParams;
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
            planningStudentEntity = new Student.Entities.PlanningStudent(id, lastName, degreePlanId, programIds);
            email = new EmailAddress("other@yahoo.com", "PER");
            preferredEmail = new EmailAddress("dsmith@yahoo.com", "HOME");
            preferredEmail.IsPreferred = true;
            financialAidCounselorId = "0007575";
            planningStudentEntityNullableParams = new Student.Entities.PlanningStudent(id, lastName, null, null);
        }

        [TestCleanup]
        public void Cleanup()
        {
            planningStudentEntity = null;
            planningStudentEntityNullableParams = null;
        }

        [TestClass]
        public class PlanningStudentConstructor : PlanningStudentTests
        {
            [TestMethod]
            public void Ctor_VerifyStudentIdProp_Set()
            {
                Assert.AreEqual(id, planningStudentEntity.Id);
            }

            [TestMethod]
            public void Ctor_VerifyDegreePlanIdProp_Set()
            {
                Assert.AreEqual(degreePlanId, planningStudentEntity.DegreePlanId);
            }

            [TestMethod]
            public void Ctor_DegreePlanId_CanBeNull()
            {
                Assert.AreEqual(null, planningStudentEntityNullableParams.DegreePlanId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException), "degreePlanId cannot be zero")]
            public void Ctor_DegreePlanId_CannotBeZero()
            {
                new Student.Entities.PlanningStudent(id, lastName, 0, programIds);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException), "degreePlanId cannot be negative")]
            public void Ctor_DegreePlanId_CannotBeNegative()
            {
                new Student.Entities.PlanningStudent(id, lastName, -1, programIds);
            }

            [TestMethod]
            public void Ctor_VerifyProgramIdsProp_Set()
            {
                Assert.AreEqual(programIds, planningStudentEntity.ProgramIds);
            }

            [TestMethod]
            public void Ctor_VerifyProgramIdsProp_CanBeNull()
            {
                Assert.AreEqual(null, planningStudentEntityNullableParams.ProgramIds);
            }

            [TestMethod]
            public void PlanningStudent_PreferredEmailAddress()
            {
                planningStudentEntity.AddEmailAddress(email);
                planningStudentEntity.AddEmailAddress(preferredEmail);
                Assert.AreEqual("dsmith@yahoo.com", planningStudentEntity.PreferredEmailAddress.Value);
            }
        }

        [TestClass]
        public class PlanningStudentAddAdvisement : PlanningStudentTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PlanningStudentAddAdvisement_AdvisorId_Null()
            {
                planningStudentEntity.AddAdvisement(null, new DateTime(), null, "AdvisorType");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PlanningStudentAddAdvisor_AdvisorId_Empty()
            {
                planningStudentEntity.AddAdvisement(string.Empty, new DateTime(), null, "AdvisorType");
            }

            [TestMethod]
            public void PlanningStudentAddAdvisemenr_Valid()
            {
                planningStudentEntity.AddAdvisement("AdvisorId", null, null, null);
                Assert.AreEqual(1, planningStudentEntity.Advisements.Count);
                Assert.AreEqual("AdvisorId", planningStudentEntity.Advisements[0].AdvisorId);
            }

            [TestMethod]
            public void PlanningStudentAddAdvisement_Duplicate()
            {
                planningStudentEntity.AddAdvisement("Advisor1", null, null, null);
                planningStudentEntity.AddAdvisement("Advisor2", new DateTime(), null, "AdvisorType");
                planningStudentEntity.AddAdvisement("Advisor1", new DateTime(), null, "AdvisorType");
                Assert.AreEqual(2, planningStudentEntity.Advisements.Count);
                Assert.AreEqual("Advisor1", planningStudentEntity.Advisements[0].AdvisorId);
                Assert.AreEqual("Advisor2", planningStudentEntity.Advisements[1].AdvisorId);
            }
        }

        [TestClass]
        public class PlanningStudentAddCompletedAdvisement : PlanningStudentTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PlanningStudent_AddCompletedAdvisement_AdvisorId_Null()
            {
                planningStudentEntity.AddCompletedAdvisement(null, new DateTime(), new DateTimeOffset());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PlanningStudent_AddCompletedAdvisement_AdvisorId_Empty()
            {
                planningStudentEntity.AddCompletedAdvisement(string.Empty, new DateTime(), new DateTimeOffset());
            }

            [TestMethod]
            public void PlanningStudent_AddCompletedAdvisement_Valid()
            {
                var today = DateTime.Today;
                var now = DateTimeOffset.Now;
                planningStudentEntity.AddCompletedAdvisement("AdvisorId", today, now);
                Assert.AreEqual(1, planningStudentEntity.CompletedAdvisements.Count);
                Assert.AreEqual("AdvisorId", planningStudentEntity.CompletedAdvisements[0].AdvisorId);
                Assert.AreEqual(today, planningStudentEntity.CompletedAdvisements[0].CompletionDate);
                Assert.AreEqual(now, planningStudentEntity.CompletedAdvisements[0].CompletionTime);
            }
        }

        [TestClass]
        public class PlanningStudentEquals
        {
            private Student.Entities.PlanningStudent planningStudentEntityOne;
            private Student.Entities.PlanningStudent planningStudentEntityTwo;
            private Student.Entities.PlanningStudent planningStudentEntityThree;

            [TestInitialize]
            public void Initialize()
            {
                planningStudentEntityOne = new Student.Entities.PlanningStudent("0000001", "Smith", null, null);
                planningStudentEntityTwo = new Student.Entities.PlanningStudent("0000001", "Smith", null, null);
                planningStudentEntityThree = new Student.Entities.PlanningStudent("0000002", "Jones", null, null);
            }

            [TestCleanup]
            public void Cleanup()
            {
                planningStudentEntityOne = null;
                planningStudentEntityTwo = null;
                planningStudentEntityThree = null;
            }

            [TestMethod]
            public void Equals_PlanningStudentEntity_Equal()
            {
                Assert.IsTrue(planningStudentEntityOne.Equals(planningStudentEntityTwo));
            }

            [TestMethod]
            public void Equals_PlanningStudentEntity_NotEqual()
            {
                Assert.IsFalse(planningStudentEntityOne.Equals(planningStudentEntityThree));
            }

            [TestMethod]
            public void Equals_PlanningStudentEntity_NotEqualIfNull()
            {
                Assert.IsFalse(planningStudentEntityOne.Equals(null));
            }
        }

        [TestClass]
        public class PlanningStudentGetHashCode
        {
            private Student.Entities.PlanningStudent studentEntityOne;
            private Student.Entities.PlanningStudent studentEntityTwo;
            private Student.Entities.PlanningStudent studentEntityThree;

            [TestInitialize]
            public void Initialize()
            {
                studentEntityOne = new Student.Entities.PlanningStudent("0000001", "Smith", null, null);
                studentEntityTwo = new Student.Entities.PlanningStudent("0000001", "Smith", null, null);
                studentEntityThree = new Student.Entities.PlanningStudent("0000002", "Jones", null, null);
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
            public void GetHashCode_StudentEntity_NotEqual()
            {
                Assert.AreNotEqual(studentEntityOne.GetHashCode(), studentEntityThree.GetHashCode());
            }
        }

        [TestClass]
        public class PlanningStudentAddRegistrationPriorityId
        {
            private string id;
            private string lastName;
            private int degreePlanId;
            private List<string> programIds;
            Student.Entities.PlanningStudent planningStudentEntity;

            [TestInitialize]
            public void Initialize()
            {
                id = "0000001";
                lastName = "Smith";
                degreePlanId = 1;
                programIds = new List<string>() { };
                planningStudentEntity = new Student.Entities.PlanningStudent(id, lastName, degreePlanId, programIds);
            }

            [TestCleanup]
            public void Cleanup()
            {
                planningStudentEntity = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AddRegistrationPriorityId_NullParameter()
            {
                planningStudentEntity.AddRegistrationPriority(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AddRegistrationPriorityId_EmptyString()
            {
                planningStudentEntity.AddRegistrationPriority("");
            }

            [TestMethod]
            public void AddRegistrationPriority_Success()
            {
                planningStudentEntity.AddRegistrationPriority("4");
                Assert.AreEqual(1, planningStudentEntity.RegistrationPriorityIds.Count());
                Assert.IsTrue(planningStudentEntity.RegistrationPriorityIds.Contains("4"));
            }

            [TestMethod]
            public void AddRegistrationPriorityIds_NoDuplicate()
            {
                planningStudentEntity.AddRegistrationPriority("4");
                planningStudentEntity.AddRegistrationPriority("4");
                Assert.AreEqual(1, planningStudentEntity.RegistrationPriorityIds.Count());
            }
        }

        [TestClass]
        public class PlanningStudentHasAdvisor
        {
            Student.Entities.PlanningStudent planningStudentEntity;

            [TestMethod]
            public void HasAdvisor_FalseIfNoAdvisements()
            {
                planningStudentEntity = new Student.Entities.PlanningStudent("1", "smith", 2, new List<string>());
                Assert.IsFalse(planningStudentEntity.HasAdvisor);
            }

            [TestMethod]
            public void HasAdvisor_TrueIfAnyAdvisements()
            {
                planningStudentEntity = new Student.Entities.PlanningStudent("1", "smith", 2, new List<string>());
                planningStudentEntity.AddAdvisement("01234", DateTime.Now, null, "MAJOR");
                Assert.IsTrue(planningStudentEntity.HasAdvisor);
            }
        }

        [TestClass]
        public class PlanningStudent_ConvertToStudentAccessEntity
        {
            [TestMethod]
            public void ConvertToStudentAccessEntity()
            {
                var planningStudentEntity = new Student.Entities.PlanningStudent("1", "smith", 2, new List<string>());
                planningStudentEntity.AddAdvisement("01234", new DateTime(2012,01,02), new DateTime(2020,12,05), "MAJOR");
                planningStudentEntity.AddAdvisement("2345", DateTime.Today, null, "MINOR");
                var studentAccessEntity = planningStudentEntity.ConvertToStudentAccess();
                Assert.AreEqual(planningStudentEntity.Id, studentAccessEntity.Id);
                Assert.AreEqual(planningStudentEntity.Advisements.Count(), studentAccessEntity.Advisements.Count());
                foreach (var item in planningStudentEntity.Advisements)
                {
                    var advisement = studentAccessEntity.Advisements.Where(a => a.AdvisorId == item.AdvisorId).First();
                    Assert.AreEqual(item.StartDate, advisement.StartDate);
                    Assert.AreEqual(item.EndDate, advisement.EndDate);
                    Assert.AreEqual(item.AdvisorType, advisement.AdvisorType);
                }
            }
        }
    }
}
