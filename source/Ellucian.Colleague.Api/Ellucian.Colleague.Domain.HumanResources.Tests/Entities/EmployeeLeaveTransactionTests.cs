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
    public class EmployeeLeaveTransactionTests
    {
        public int id;
        public string leavePlanDefinitionId;
        public string employeeLeavePlanId;
        public decimal transactionHours;
        public DateTimeOffset date;
        public LeaveTransactionType type;
        public decimal forwardingBalance;

        public void BaseInitialize()
        {
            id = 5;
            leavePlanDefinitionId = "LPN";
            employeeLeavePlanId = "234";
            transactionHours = 8;
            date = new DateTimeOffset(2018, 8, 14, 0, 0, 0, new TimeSpan(-5, 0, 0));
            type = LeaveTransactionType.Earned;
            forwardingBalance = 15;
        }

        public EmployeeLeaveTransaction employeeLeaveTransaction
        {
            get
            {
                return new EmployeeLeaveTransaction(id, leavePlanDefinitionId, employeeLeavePlanId, transactionHours, date, type, forwardingBalance);
            }
        }

        [TestClass]
        public class ConstructorTests : EmployeeLeaveTransactionTests
        {
            

            [TestInitialize]
            public void Initialize()
            {
                BaseInitialize();
            }

            [TestMethod]
            public void IdTest()
            {
                Assert.AreEqual(id, employeeLeaveTransaction.Id);
            }

            [TestMethod]
            public void LeavePlanDefinitionIdTest()
            {
                Assert.AreEqual(leavePlanDefinitionId, employeeLeaveTransaction.LeavePlanDefinitionId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void LeavePlanDefinitionIdRequredTest()
            {
                leavePlanDefinitionId = null;
                var fail = employeeLeaveTransaction;
            }

            [TestMethod]
            public void EmployeeLeavePlanIdTest()
            {
                Assert.AreEqual(employeeLeavePlanId, employeeLeaveTransaction.EmployeeLeavePlanId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EmployeeLeavePlanIdRequredTest()
            {
                employeeLeavePlanId = null;
                var fail = employeeLeaveTransaction;
            }

            [TestMethod]
            public void TransactionHoursTest()
            {
                Assert.AreEqual(transactionHours, employeeLeaveTransaction.TransactionHours);
            }


            [TestMethod]
            public void TransactionHoursNotRangeBoundTest()
            {
                transactionHours = decimal.MaxValue;
                Assert.AreEqual(transactionHours, employeeLeaveTransaction.TransactionHours);

                transactionHours = decimal.MinValue;
                Assert.AreEqual(transactionHours, employeeLeaveTransaction.TransactionHours);
            }

            [TestMethod]
            public void DateTest()
            {
                Assert.AreEqual(date, employeeLeaveTransaction.Date);
            }

            [TestMethod]
            public void TypeTest()
            {
                Assert.AreEqual(type, employeeLeaveTransaction.Type);
            }

            [TestMethod]
            public void ForwardingBalanceTest()
            {
                Assert.AreEqual(forwardingBalance, employeeLeaveTransaction.ForwardingBalance);
            }

            [TestMethod]
            public void ForwardingBalanceNotRangeBoundTest()
            {
                forwardingBalance = decimal.MaxValue;
                Assert.AreEqual(forwardingBalance, employeeLeaveTransaction.ForwardingBalance);

                forwardingBalance = decimal.MinValue;
                Assert.AreEqual(forwardingBalance, employeeLeaveTransaction.ForwardingBalance);
            }


    
        }

        [TestClass]
        public class EqualsAndComparisonTests : EmployeeLeaveTransactionTests
        {
            [TestInitialize]
            public void Initialize()
            {
                BaseInitialize();
            }

            [TestMethod]
            public void EqualsTest()
            {
                var transaction1 = employeeLeaveTransaction;
                var transaction2 = employeeLeaveTransaction;
                Assert.IsTrue(transaction1.Equals(transaction2));
                Assert.IsTrue(transaction2.Equals(transaction1));
            }

            [TestMethod]
            public void NotEqualTest()
            {
                var trans1 = employeeLeaveTransaction;
                id = 1234;
                var trans2 = employeeLeaveTransaction;
                Assert.IsFalse(trans1.Equals(trans2));
                Assert.IsFalse(trans2.Equals(trans1));
            }

            [TestMethod]
            public void NotEqualNullTest()
            {
                Assert.IsFalse(employeeLeaveTransaction.Equals(null));
            }

            [TestMethod]
            public void NotEqualTypeTest()
            {
                Assert.IsFalse(employeeLeaveTransaction.Equals(LeaveTransactionType.Adjusted));
            }

            [TestMethod]
            public void HashCodeEqualTest()
            {
                Assert.AreEqual(employeeLeaveTransaction.GetHashCode(), employeeLeaveTransaction.GetHashCode());
            }

            [TestMethod]
            public void HashCodeNotEqualTest()
            {
                var trans1 = employeeLeaveTransaction;
                id = 1234;
                var trans2 = employeeLeaveTransaction;
                Assert.AreNotEqual(trans1.GetHashCode(), trans2.GetHashCode());
            }

            [TestMethod]
            public void CompareLessThanTest()
            {
                var trans1 = employeeLeaveTransaction;
                id = 15;
                var trans2 = employeeLeaveTransaction;

                Assert.AreEqual(-1, trans1.CompareTo(trans2));
                Assert.AreEqual(1, trans2.CompareTo(trans1));
                Assert.AreEqual(-1, trans1.Compare(trans1, trans2));
                Assert.AreEqual(1, trans1.Compare(trans2, trans1));
            }

            [TestMethod]
            public void CompareEqualsTest()
            {
                var trans1 = employeeLeaveTransaction;
                var trans2 = employeeLeaveTransaction;
                Assert.AreEqual(0, trans1.CompareTo(trans2));
                Assert.AreEqual(0, trans2.CompareTo(trans1));
                Assert.AreEqual(0, trans1.Compare(trans1, trans2));
                Assert.AreEqual(0, trans2.Compare(trans2, trans1));
            }

            [TestMethod]
            public void CompareGreaterThanTest()
            {
                var trans1 = employeeLeaveTransaction;
                id = -5;
                var trans2 = employeeLeaveTransaction;
                Assert.AreEqual(1, trans1.CompareTo(trans2));
                Assert.AreEqual(-1, trans2.CompareTo(trans1));
                Assert.AreEqual(1, trans1.Compare(trans1, trans2));
                Assert.AreEqual(-1, trans2.Compare(trans2, trans1));
            }
        }

    }
}
