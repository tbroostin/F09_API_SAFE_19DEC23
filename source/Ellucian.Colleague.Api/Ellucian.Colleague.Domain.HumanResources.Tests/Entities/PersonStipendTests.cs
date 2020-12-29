/* Copyright 2019 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Tests.Entities
{
    [TestClass]
    public class PersonStipendTests
    {
        public string id;
        public string personId;
        public string positionId;
        public DateTime startDate;
        public DateTime? endDate;
        public string description;
        public string baseAmount;
        public string payrollDesignation;
        public int? numberOfPayments;
        public int? numberOfPaymentsTaken;
        public List<string> courseSectionAssignments;
        public List<string> advisorAssignments;
        public List<string> membershipAssignment;

        public PersonStipend personStipend;

        public void PersonStipendTestInitialize()
        {
            id = "4931";
            personId = "0016390";
            positionId = "MANAGER";
            startDate = new DateTime(2010, 1, 1);
            endDate = null;
            description = "Restricted Stipend";
            baseAmount = "10000";
            payrollDesignation = "R";
            numberOfPayments = 3;
            numberOfPaymentsTaken = 2;
            courseSectionAssignments = new List<string>()
            {
                "23560",
                "18905"
            };
            advisorAssignments = new List<string>()
            {
                "FTBPL",
                "BIOS"
            };
            membershipAssignment = new List<string>()
            {
                "CYC"
            };
        }

        [TestClass]
        public class PersonStipendConstructorTests : PersonStipendTests
        {
            public new PersonStipend personStipend
            {
                get
                {
                    return new PersonStipend(id, personId, positionId, startDate, endDate, description, baseAmount, payrollDesignation, numberOfPayments, numberOfPaymentsTaken, courseSectionAssignments, advisorAssignments, membershipAssignment);
                }
            }

            [TestInitialize]
            public void Initialize()
            {
                PersonStipendTestInitialize();
            }

            [TestMethod]
            public void IdTest()
            {
                Assert.AreEqual(id, personStipend.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void IdRequiredTest()
            {
                id = "";
                var error = personStipend;
            }

            [TestMethod]
            public void PersonIdTest()
            {
                Assert.AreEqual(personId, personStipend.PersonId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PersonIdRequiredTest()
            {
                personId = "";
                var error = personStipend;
            }

            [TestMethod]
            public void PositionIdTest()
            {
                Assert.AreEqual(positionId, personStipend.PositionId);
            }

            [TestMethod]
            public void StartDateTest()
            {
                Assert.AreEqual(startDate, personStipend.StartDate);
            }

            [TestMethod]
            public void DescriptionTest()
            {
                Assert.AreEqual(description, personStipend.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void DescriptionRequiredTest()
            {
                description = "";
                var error = personStipend;
            }

            [TestMethod]
            public void BaseAmountTest()
            {
                Assert.AreEqual(baseAmount, personStipend.BaseAmount);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void BaseAmountRequiredTest()
            {
                baseAmount = "";
                var error = personStipend;
            }

            [TestMethod]
            public void PayrollDesignationTest()
            {
                Assert.AreEqual(payrollDesignation, personStipend.PayrollDesignation);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PayrollDesignationRequiredTest()
            {
                payrollDesignation = "";
                var error = personStipend;
            }
        }

        [TestClass]
        public class AttributeTests : PersonStipendTests
        {

            [TestInitialize]
            public void Initialize()
            {
                PersonStipendTestInitialize();
                personStipend = new PersonStipend(id, personId, positionId, startDate, endDate, description, baseAmount, payrollDesignation, numberOfPayments, numberOfPaymentsTaken, courseSectionAssignments, advisorAssignments, membershipAssignment);
            }

            [TestMethod]
            public void EndDateTest()
            {
                endDate = null;
                Assert.AreEqual(endDate, personStipend.EndDate);

                personStipend.EndDate = endDate;
                Assert.AreEqual(endDate, personStipend.EndDate);
            }

            [TestMethod]
            public void PayrollDesignationTest()
            {
                var payrollDesignation = "R";
                Assert.AreEqual(payrollDesignation, personStipend.PayrollDesignation);
            }

            [TestMethod]
            public void NumberOfPaymentsTest()
            {
                var numberOfPayments = 3;
                Assert.AreEqual(numberOfPayments, personStipend.NumberOfPayments);
            }

            [TestMethod]
            public void NumberOfpaymentsTaken()
            {
                var numberOfpaymentsTaken = 2;
                Assert.AreEqual(numberOfpaymentsTaken, personStipend.NumberOfPaymentsTaken);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void EndDateBeforeStartDateTest()
            {
                personStipend.EndDate = startDate.AddDays(-1);
                var error = personStipend;
            }

            [TestMethod]
            public void CourseSectionAssignments_ContainsGivenItemTest()
            {
                string courseSectionId = "18905";
                var result = personStipend.CourseSectionAssignments.Contains(courseSectionId);
                Assert.IsTrue(result);
            }

            [TestMethod]
            public void AdvisorAssignments_ContainsGivenItemTest()
            {
                string campusOrgsId = "BIOS";
                var result = personStipend.AdvisorAssignments.Contains(campusOrgsId);
                Assert.IsTrue(result);
            }

            [TestMethod]
            public void MembershipAssignments_ContainsGivenItemTest()
            {
                string campusOrgsId = "CYC";
                var result = personStipend.MembershipAssignments.Contains(campusOrgsId);
                Assert.IsTrue(result);
            }
        }

        [TestClass]
        public class EqualsTests : PersonStipendTests
        {
            public PersonStipend buildPersonStipend()
            {
                return new PersonStipend(id, personId, positionId, startDate, endDate, description, baseAmount, payrollDesignation, numberOfPayments, numberOfPaymentsTaken, courseSectionAssignments, advisorAssignments, membershipAssignment);
            }

            [TestInitialize]
            public void Initialize()
            {
                PersonStipendTestInitialize();
            }

            [TestMethod]
            public void ObjectsEqualWhenIdsEqualTest()
            {
                var ps1 = buildPersonStipend();
                var ps2 = buildPersonStipend();
                Assert.AreEqual(ps1.Id, ps2.Id);           
            }          

            [TestMethod]
            public void ObjectsNotEqualWhenIdsNotEqualTest()
            {
                var ps1 = buildPersonStipend();
                id = "dummyId";
                var ps2 = buildPersonStipend();
                Assert.AreNotEqual(ps1.Id, ps2.Id);
            }

            [TestMethod]
            public void ObjectsNotEqualWhenInputIsNullTest()
            {
                Assert.IsFalse(buildPersonStipend().Equals(null));
            }
        }
    }
}
