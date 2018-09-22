/*Copyright 2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests.Entities
{
    [TestClass]
    public class StudentBudgetComponentTests
    {
        public string studentId;
        public string awardYear;
        public string budgetComponentCode;
        public int campusBasedOriginalAmount;
        public int? campusBaesdOverrideAmount;

        public StudentBudgetComponent studentBudgetComponent;

        public void StudentBudgetComponentTestsInitialize()
        {
            studentId = "0003914";
            awardYear = "2015";
            budgetComponentCode = "TUITION";
            campusBasedOriginalAmount = 12345;
            campusBaesdOverrideAmount = 54321;

            studentBudgetComponent = new StudentBudgetComponent(awardYear, studentId, budgetComponentCode, campusBasedOriginalAmount) { CampusBasedOverrideAmount = campusBaesdOverrideAmount };
        }

        [TestClass]
        public class StudentBudgetComponentConstructorTests : StudentBudgetComponentTests
        {
            [TestInitialize]
            public void Initialize()
            {
                StudentBudgetComponentTestsInitialize();
            }

            [TestMethod]
            public void StudentIdEqualTest()
            {
                Assert.AreEqual(studentId, studentBudgetComponent.StudentId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentIdRequiredTest()
            {
                new StudentBudgetComponent(awardYear, null, budgetComponentCode, campusBasedOriginalAmount);
            }

            [TestMethod]
            public void AwardYearEqualTest()
            {
                Assert.AreEqual(awardYear, studentBudgetComponent.AwardYear);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AwardYearRequiredTest()
            {
                new StudentBudgetComponent(null, studentId, budgetComponentCode, campusBasedOriginalAmount);
            }

            [TestMethod]
            public void BudgetComponentCodeEqualTest()
            {
                Assert.AreEqual(budgetComponentCode, studentBudgetComponent.BudgetComponentCode);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void BudgetComponentCodeRequiredTest()
            {
                new StudentBudgetComponent(awardYear, studentId, string.Empty, campusBasedOriginalAmount);
            }

            [TestMethod]
            public void CampusBasedOriginalAmountEqualTest()
            {
                Assert.AreEqual(campusBasedOriginalAmount, studentBudgetComponent.CampusBasedOriginalAmount);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void CampusBasedOriginalAmountLessThanZeroTest()
            {
                new StudentBudgetComponent(awardYear, studentId, budgetComponentCode, -1);
            }
        }

        [TestClass]
        public class CampusBasedOverrideAmountTests : StudentBudgetComponentTests
        {
            [TestInitialize]
            public void Initialize()
            {
                StudentBudgetComponentTestsInitialize();
            }

            [TestMethod]
            public void SetCampusBasedOverrideAmountToZeroTest()
            {
                int? amount = 0;
                studentBudgetComponent.CampusBasedOverrideAmount = amount;

                Assert.AreEqual(amount, studentBudgetComponent.CampusBasedOverrideAmount);
            }

            [TestMethod]
            public void IgnoreNegativeNumbersTest()
            {
                studentBudgetComponent.CampusBasedOverrideAmount = -1;

                Assert.AreEqual(campusBaesdOverrideAmount, studentBudgetComponent.CampusBasedOverrideAmount);
            }

            [TestMethod]
            public void SetToNullTest()
            {
                studentBudgetComponent.CampusBasedOverrideAmount = null;
                Assert.IsNull(studentBudgetComponent.CampusBasedOverrideAmount);
            }
        }

        [TestClass]
        public class StudentBudgetComponentEqualsTests : StudentBudgetComponentTests
        {
            public StudentBudgetComponent testStudentBudgetComponent;

            [TestInitialize]
            public void Initialize()
            {
                StudentBudgetComponentTestsInitialize();
            }

            [TestMethod]
            public void NullObject_ReturnFalseTest()
            {
                Assert.IsFalse(studentBudgetComponent.Equals(null));
            }

            [TestMethod]
            public void NonStudentBudgetComponentType_ReturnFalseTest()
            {
                Assert.IsFalse(studentBudgetComponent.Equals(new BudgetComponent("foo", "bar", "desc")));
            }

            [TestMethod]
            public void SameStudentIdAwardYearAndBudgetComponentCode_ReturnTrueTest()
            {
                testStudentBudgetComponent = new StudentBudgetComponent(studentBudgetComponent.AwardYear, studentBudgetComponent.StudentId, studentBudgetComponent.BudgetComponentCode, 5);

                Assert.AreEqual(testStudentBudgetComponent, studentBudgetComponent);
            }

            [TestMethod]
            public void SameStudentIdAwardYearAndBudgetComponentCode_SameHashCodeTest()
            {
                testStudentBudgetComponent = new StudentBudgetComponent(studentBudgetComponent.AwardYear, studentBudgetComponent.StudentId, studentBudgetComponent.BudgetComponentCode, 5);

                Assert.AreEqual(testStudentBudgetComponent.GetHashCode(), studentBudgetComponent.GetHashCode());
            }

            [TestMethod]
            public void DifferentAwardYear_ReturnFalseTest()
            {
                testStudentBudgetComponent = new StudentBudgetComponent("foobar", studentBudgetComponent.StudentId, studentBudgetComponent.BudgetComponentCode, 5);

                Assert.AreNotEqual(testStudentBudgetComponent, studentBudgetComponent);
            }

            [TestMethod]
            public void DifferentAwardYear_DifferentHashCodeTest()
            {
                testStudentBudgetComponent = new StudentBudgetComponent("foobar", studentBudgetComponent.StudentId, studentBudgetComponent.BudgetComponentCode, 5);

                Assert.AreNotEqual(testStudentBudgetComponent.GetHashCode(), studentBudgetComponent.GetHashCode());
            }

            [TestMethod]
            public void DifferentStudentId_ReturnFalseTest()
            {
                testStudentBudgetComponent = new StudentBudgetComponent(awardYear, "foobar", studentBudgetComponent.BudgetComponentCode, 5);

                Assert.AreNotEqual(testStudentBudgetComponent, studentBudgetComponent);
            }

            [TestMethod]
            public void DifferentStudentId_DifferentHashCodeTest()
            {
                testStudentBudgetComponent = new StudentBudgetComponent(awardYear, "foobar", studentBudgetComponent.BudgetComponentCode, 5);

                Assert.AreNotEqual(testStudentBudgetComponent.GetHashCode(), studentBudgetComponent.GetHashCode());
            }

            [TestMethod]
            public void DifferentBudgetComponentCode_ReturnFalseTest()
            {
                testStudentBudgetComponent = new StudentBudgetComponent(awardYear, studentId, "foobar", 5);

                Assert.AreNotEqual(testStudentBudgetComponent, studentBudgetComponent);
            }

            [TestMethod]
            public void DifferentBudgetComponentCode_DifferentHashCodeTest()
            {
                testStudentBudgetComponent = new StudentBudgetComponent(awardYear, studentId, "foobar", 5);

                Assert.AreNotEqual(testStudentBudgetComponent.GetHashCode(), studentBudgetComponent.GetHashCode());
            }
        }


    }
}
