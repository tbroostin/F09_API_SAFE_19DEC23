using Ellucian.Colleague.Domain.HumanResources.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Tests.Entities
{
    [TestClass]
    public class EmployeeLeavePlanTests
    {
        public string id;
        public string employeeId;
        public DateTime startDate;
        public DateTime? endDate;
        public string leavePlanId;
        public string leavePlanDescription;
        public DateTime leavePlanStartDate;
        public DateTime? leavePlanEndDate;
        public DateTime leaveAllowedDate;
        public decimal priorPayPeriodBalance;
        public bool allowNegativeBalance;
        public LeaveTypeCategory leavePlanTypeCategory;
        public string earningsTypeId;
        public string earningsTypeDescription;
        public int planYearStartMonth;
        public int planYearStartDay;

        public List<EmployeeLeaveTransaction> employeeLeaveTransactions;

        public EmployeeLeavePlan employeeLeavePlan
        {
            get
            {
                return new EmployeeLeavePlan(id,
                    employeeId,
                    startDate,
                    endDate,
                    leavePlanId,
                    leavePlanDescription,
                    leavePlanStartDate,
                    leavePlanEndDate,
                    leavePlanTypeCategory,
                    earningsTypeId,
                    earningsTypeDescription,
                    leaveAllowedDate,
                    priorPayPeriodBalance,
                    planYearStartMonth,
                    planYearStartDay,
                    allowNegativeBalance);


            }
        }


        public void BaseInitialize()
        {
            id = "938723";
            employeeId = "0003914";
            startDate = new DateTime(2017, 1, 1);
            endDate = null;
            leavePlanId = "VACH";
            leavePlanDescription = "Vacation Hourly LeavePlan";
            leavePlanStartDate = new DateTime(1960, 1, 1);
            leavePlanEndDate = null;
            leaveAllowedDate = startDate;
            priorPayPeriodBalance = 20;
            allowNegativeBalance = true;
            leavePlanTypeCategory = LeaveTypeCategory.Vacation;
            earningsTypeId = "VAC";
            earningsTypeDescription = "Vacation Leave EarningsType";
            planYearStartMonth = 1;
            planYearStartDay = 1;

            var lastYear = DateTime.Today.AddYears(-1).Year;
            employeeLeaveTransactions = new List<EmployeeLeaveTransaction>()
            {
                new EmployeeLeaveTransaction(1, leavePlanId, id, 5, new DateTimeOffset(new DateTime(lastYear, 5, 1)), LeaveTransactionType.Earned, 5),
                new EmployeeLeaveTransaction(2, leavePlanId, id, -5, new DateTimeOffset(new DateTime(lastYear, 6, 1)), LeaveTransactionType.Used, 0),
                new EmployeeLeaveTransaction(3, leavePlanId, id, 20, new DateTimeOffset(new DateTime(DateTime.Today.Year, 1, 1)), LeaveTransactionType.Adjusted, 20), //PriorPlanYearLastTransaction, starting balance should be 20     
                new EmployeeLeaveTransaction(4, leavePlanId, id, -5, new DateTimeOffset(new DateTime(DateTime.Today.Year, 1, 15)), LeaveTransactionType.Used, 15), //used
                new EmployeeLeaveTransaction(5, leavePlanId, id, 10, new DateTimeOffset(new DateTime(DateTime.Today.Year, 2, 1)), LeaveTransactionType.Earned, 25), //earned
                new EmployeeLeaveTransaction(6, leavePlanId, id, -2, new DateTimeOffset(new DateTime(DateTime.Today.Year, 2, 1)), LeaveTransactionType.Adjusted, 23) //adjusted
            };

        }


        [TestClass]
        public class ConstructorTests : EmployeeLeavePlanTests
        {
            [TestInitialize]
            public void Initialize()
            {
                BaseInitialize();
            }

            [TestMethod]
            public void IdTest()
            {
                Assert.AreEqual(id, employeeLeavePlan.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void IdRequiredTest()
            {
                id = null;
                var fail = employeeLeavePlan;
            }

            [TestMethod]
            public void EmployeeIdTest()
            {
                Assert.AreEqual(employeeId, employeeLeavePlan.EmployeeId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EmployeeIdRequiredTest()
            {
                employeeId = null;
                var fail = employeeLeavePlan;
            }

            [TestMethod]
            public void LeavePlanIdTest()
            {
                Assert.AreEqual(leavePlanId, employeeLeavePlan.LeavePlanId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void LeavePlanIdRequiredTest()
            {
                leavePlanId = null;
                var fail = employeeLeavePlan;
            }

            [TestMethod]
            public void EarningsTypeIdTest()
            {
                Assert.AreEqual(earningsTypeId, employeeLeavePlan.EarningsTypeId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EarningsTypeIdRequiredTest()
            {
                earningsTypeId = null;
                var fail = employeeLeavePlan;
            }

            [TestMethod]
            public void StartDateTest()
            {
                Assert.AreEqual(startDate, employeeLeavePlan.StartDate);
            }

            [TestMethod]
            public void EndDateTest()
            {
                endDate = null;
                Assert.AreEqual(endDate, employeeLeavePlan.EndDate);

                endDate = startDate;
                Assert.AreEqual(endDate, employeeLeavePlan.EndDate);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void EndDateBeforeStartDateTest()
            {
                endDate = startDate.AddDays(-1);
                var fail = employeeLeavePlan;
            }

            [TestMethod]
            public void LeavePlanStartDateTest()
            {
                Assert.AreEqual(leavePlanStartDate, employeeLeavePlan.LeavePlanStartDate);
            }

            [TestMethod]
            public void LeavePlanEndDateTest()
            {
                leavePlanEndDate = null;
                Assert.AreEqual(leavePlanEndDate, employeeLeavePlan.LeavePlanEndDate);

                leavePlanEndDate = leavePlanStartDate;
                Assert.AreEqual(leavePlanEndDate, employeeLeavePlan.LeavePlanEndDate);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void LeavePlanEndDateBeforeLeavePlanStartDateTest()
            {
                leavePlanEndDate = leavePlanStartDate.AddDays(-1);
                var fail = employeeLeavePlan;
            }

            [TestMethod]
            public void PlanYearStartMonthTest()
            {
                Assert.AreEqual(planYearStartMonth, employeeLeavePlan.PlanYearStartMonth);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void PlanYearStartMonthLessThan1Test()
            {
                planYearStartMonth = 0;
                var fail = employeeLeavePlan;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void PlanYearStartMonthGreaterThan12Test()
            {
                planYearStartMonth = 13;
                var fail = employeeLeavePlan;
            }

            [TestMethod]
            public void PlanYearStartDayTest()
            {
                Assert.AreEqual(planYearStartDay, employeeLeavePlan.PlanYearStartDay);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void PlanYearStartDayLessThan1Test()
            {
                planYearStartDay = 0;
                var fail = employeeLeavePlan;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void PlanYearStartDayGreaterThan31Test()
            {
                planYearStartDay = 32;
                var fail = employeeLeavePlan;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void PlanYearStartMonthAndDayInvalidCombinationTest()
            {

                planYearStartMonth = 2;
                planYearStartDay = 30; //Feb 30 is not a date
                var fail = employeeLeavePlan;
            }

            [TestMethod]
            public void LeavePlanDescriptionTest()
            {
                Assert.AreEqual(leavePlanDescription, employeeLeavePlan.LeavePlanDescription);
            }

            [TestMethod]
            public void LeavePlanTypeCategoryTest()
            {
                Assert.AreEqual(leavePlanTypeCategory, employeeLeavePlan.LeavePlanTypeCategory);
            }

            [TestMethod]
            public void EarningsTypeDescriptionTest()
            {
                Assert.AreEqual(earningsTypeDescription, employeeLeavePlan.EarningsTypeDescription);
            }

            [TestMethod]
            public void LeaveAllowedDateTest()
            {
                Assert.AreEqual(leaveAllowedDate, employeeLeavePlan.LeaveAllowedDate);
            }

            [TestMethod]
            public void PriorPeriodLeaveBalanceTest()
            {
                Assert.AreEqual(priorPayPeriodBalance, employeeLeavePlan.PriorPeriodLeaveBalance);
            }

            [TestMethod]
            public void AllowNegativeBalanceTest()
            {
                Assert.AreEqual(allowNegativeBalance, employeeLeavePlan.AllowNegativeBalance);
            }

            [TestMethod]
            public void LeaveTransactionsInitializedTest()
            {
                Assert.IsFalse(employeeLeavePlan.SortedLeaveTransactions.Any());
            }
        }

        [TestClass]
        public class EqualsTests : EmployeeLeavePlanTests
        {
            [TestInitialize]
            public void Initialize()
            {
                BaseInitialize();
            }

            [TestMethod]
            public void EqualsTest()
            {
                var plan1 = employeeLeavePlan;
                var plan2 = employeeLeavePlan;
                Assert.AreNotSame(plan1, plan2);
                Assert.IsTrue(plan1.Equals(plan2));
                Assert.IsTrue(plan2.Equals(plan1));
            }

            [TestMethod]
            public void HashCodeEqualsTest()
            {
                var plan1 = employeeLeavePlan;
                var plan2 = employeeLeavePlan;
                Assert.AreEqual(plan1.GetHashCode(), plan2.GetHashCode());
            }


            [TestMethod]
            public void NotEqualTest()
            {
                var plan1 = employeeLeavePlan;
                id = "foo";
                var plan2 = employeeLeavePlan;

                Assert.IsFalse(plan1.Equals(plan2));
                Assert.IsFalse(plan2.Equals(plan1));
            }

            [TestMethod]
            public void HashCodeNotEqualsTest()
            {
                var plan1 = employeeLeavePlan;
                id = "bar";
                var plan2 = employeeLeavePlan;
                Assert.AreNotEqual(plan1.GetHashCode(), plan2.GetHashCode());
            }

            [TestMethod]
            public void NotEqualNullTest()
            {
                Assert.IsFalse(employeeLeavePlan.Equals(null));
            }

            [TestMethod]
            public void NotEqualTypeTest()
            {
                Assert.IsFalse(employeeLeavePlan.Equals(employeeLeaveTransactions));
            }
        }

        [TestClass]
        public class CurrentPlanYearStartDateTests : EmployeeLeavePlanTests
        {
            [TestInitialize]
            public void Initialize()
            {
                BaseInitialize();
            }

            //When the plan year starts on Jan 1, the default plan year start date
            //will be Jan 1 of the current year.
            [TestMethod]
            public void DefaultPlanYearStartDateTest()
            {
                planYearStartMonth = 1;
                planYearStartDay = 1;

                var expectedStartDate = new DateTime(DateTime.Today.Year, 1, 1);
                var actualStartDate = employeeLeavePlan.CurrentPlanYearStartDate;

                Assert.AreEqual(expectedStartDate, actualStartDate);
            }

            //when planYearStartMonth and Day are not Jan1, then we compare the default plan year start date to today
            //if today is on or after the default start date, then use the default
            [TestMethod]
            public void TodayIsOnTheDefaultPlanYearStartDateTest()
            {
                planYearStartMonth = DateTime.Today.Month;
                planYearStartDay = DateTime.Today.Day;

                var expectedStartDate = DateTime.Today;
                var actualStartDate = employeeLeavePlan.CurrentPlanYearStartDate;

                Assert.AreEqual(expectedStartDate, actualStartDate);
            }

            [TestMethod]
            public void TodayIsAfterTheDefaultPlanYearStartDateTest()
            {
                planYearStartMonth = DateTime.Today.Month;
                planYearStartDay = 1; //first of the month

                var expectedStartDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                var actualStartDate = employeeLeavePlan.CurrentPlanYearStartDate;

                Assert.AreEqual(expectedStartDate, actualStartDate);
            }

            //when planYearStartMonth and Day are not Jan1, then we compare the default plan year start date to today
            //if today is before the default start date, then the plan year start date is year - 1, month, day
            [TestMethod]
            public void TodayIsBeforeTheDefaultPlanYearStartDateTest()
            {
                planYearStartMonth = DateTime.Today.AddMonths(1).Month;
                planYearStartDay = 1; //first of next month

                var expectedStartDate = new DateTime(DateTime.Today.AddYears(-1).Year, planYearStartMonth, 1);
                var actualStartDate = employeeLeavePlan.CurrentPlanYearStartDate;

                Assert.AreEqual(expectedStartDate, actualStartDate);
            }
        }

        [TestClass]
        public class PriorPlanYearEndTransactionTests : EmployeeLeavePlanTests
        {
            public EmployeeLeavePlan planWithTransactions;

            [TestInitialize]
            public void Initialize()
            {
                BaseInitialize();
                planWithTransactions = employeeLeavePlan;
                planWithTransactions.AddLeaveTransactionRange(employeeLeaveTransactions.ToArray());
            }

            [TestMethod]
            public void AdjustmentTransactionOnPlanYearStartDateTest()
            {
                var expected = employeeLeaveTransactions[2];
                var actual = planWithTransactions.PriorPlanYearEndTransaction;

                Assert.AreEqual(expected, actual);
            }

            [TestMethod]
            public void SkipOverTransactionsAfterPlanYearStartDateTest()
            {
                var plan = planWithTransactions;
                var skipOver = new EmployeeLeaveTransaction(7, leavePlanId, id, -5, new DateTimeOffset(new DateTime(DateTime.Today.Year, 1, 2)), LeaveTransactionType.Used, 15);
                var skipOver2 = new EmployeeLeaveTransaction(8, leavePlanId, id, 10, new DateTimeOffset(new DateTime(DateTime.Today.Year, 2, 1)), LeaveTransactionType.Earned, 25);

                planWithTransactions.AddLeaveTransactionRange(skipOver, skipOver2);

                var expected = employeeLeaveTransactions[2];
                var actual = planWithTransactions.PriorPlanYearEndTransaction;

                Assert.AreEqual(expected, actual);

            }

            [TestMethod]
            public void NonAdjustmentTransactionOnPlanYearStartDateTest()
            {
                var plan = planWithTransactions;

                var expected = employeeLeaveTransactions[2];
                var skipOver = new EmployeeLeaveTransaction(7, leavePlanId, id, -5, expected.Date, LeaveTransactionType.Used, 15);
                plan.AddLeaveTransaction(skipOver);


                var actual = plan.PriorPlanYearEndTransaction;

                Assert.AreEqual(expected, actual);
            }

            [TestMethod]
            public void LastTransactionBeforePlanYearStartDateTest()
            {
                planWithTransactions = employeeLeavePlan;
                var skipOver = new EmployeeLeaveTransaction(7, leavePlanId, id, -5, planWithTransactions.CurrentPlanYearStartDate, LeaveTransactionType.Used, 15);
                planWithTransactions.AddLeaveTransactionRange(employeeLeaveTransactions[0], employeeLeaveTransactions[1], skipOver); //not adding the adjustment transaction

                var expected = employeeLeaveTransactions[1];
                var actual = planWithTransactions.PriorPlanYearEndTransaction;
                Assert.AreEqual(expected, actual);
            }

            [TestMethod]
            public void NoTransactionsBeforePlanYearStartDateTest()
            {
                planWithTransactions = employeeLeavePlan;
                var skipOver = new EmployeeLeaveTransaction(7, leavePlanId, id, -5, planWithTransactions.CurrentPlanYearStartDate, LeaveTransactionType.Used, 15);
                planWithTransactions.AddLeaveTransaction(skipOver);

                Assert.IsNull(planWithTransactions.PriorPlanYearEndTransaction);
            }

            [TestMethod]
            public void NoTransactionsTest()
            {
                planWithTransactions = employeeLeavePlan;
                Assert.IsNull(planWithTransactions.PriorPlanYearEndTransaction);
            }

        }

        [TestClass]
        public class CurrentPlanYearStartTransactionIndexTests : EmployeeLeavePlanTests
        {
            public EmployeeLeavePlan planWithTransactions
            {
                get
                {
                    var plan = employeeLeavePlan;
                    plan.AddLeaveTransactionRange(employeeLeaveTransactions.ToArray());
                    return plan;
                }
            }



            [TestInitialize]
            public void Initialize()
            {
                BaseInitialize();

            }

            [TestMethod]
            public void PriorPlanYearEndTransactionIndexTest()
            {
                Assert.AreEqual(employeeLeaveTransactions[2], planWithTransactions.PriorPlanYearEndTransaction);

                Assert.AreEqual(2, planWithTransactions.PriorPlanYearEndTransactionIndex);
            }

            [TestMethod]
            public void NoPriorPlanYearEndTransactionTest()
            {
                employeeLeaveTransactions = new List<EmployeeLeaveTransaction>();

                Assert.AreEqual(-1, planWithTransactions.PriorPlanYearEndTransactionIndex);
            }
        }

        [TestClass]
        public class CurrentPlanYearStartingBalanceTests : EmployeeLeavePlanTests
        {
            public EmployeeLeavePlan planWithTransactions
            {
                get
                {
                    var plan = employeeLeavePlan;
                    plan.AddLeaveTransactionRange(employeeLeaveTransactions.ToArray());
                    return plan;
                }
            }



            [TestInitialize]
            public void Initialize()
            {
                BaseInitialize();
            }

            [TestMethod]
            public void StartingBalanceIsForwardBalanceOfPriorPlanYearEndTransactionTest()
            {
                Assert.AreEqual(employeeLeaveTransactions[2], planWithTransactions.PriorPlanYearEndTransaction);

                Assert.AreEqual(employeeLeaveTransactions[2].ForwardingBalance, planWithTransactions.CurrentPlanYearStartingBalance);
            }

            [TestMethod]
            public void StartingBalanceIsZeroTest()
            {
                employeeLeaveTransactions = new List<EmployeeLeaveTransaction>();

                Assert.AreEqual(0, planWithTransactions.CurrentPlanYearStartingBalance);
            }
        }

        [TestClass]
        public class CurrentPlanYearBalanceTests : EmployeeLeavePlanTests
        {
            public EmployeeLeavePlan planWithTransactions
            {
                get
                {
                    var plan = employeeLeavePlan;
                    plan.AddLeaveTransactionRange(employeeLeaveTransactions.ToArray());
                    return plan;
                }
            }

            [TestInitialize]
            public void Initialize()
            {
                BaseInitialize();
            }

            [TestMethod]
            public void CurrentPlanYearEarnedHoursTest()
            {
                var plan = planWithTransactions;
                decimal expectedAmount = 0;
                for (int i = plan.PriorPlanYearEndTransactionIndex + 1; i < plan.SortedLeaveTransactions.Count; i++)
                {
                    if (plan.SortedLeaveTransactions[i].Type == LeaveTransactionType.Earned)
                    {
                        expectedAmount += plan.SortedLeaveTransactions[i].TransactionHours;
                    }
                }

                Assert.AreEqual(expectedAmount, plan.CurrentPlanYearEarnedHours);
            }

            [TestMethod]
            public void DifferentIndexEarnedHoursTest()
            {
                employeeLeaveTransactions = new List<EmployeeLeaveTransaction>();
                var plan = planWithTransactions;
                decimal expectedAmount = 0;
                for (int i = plan.PriorPlanYearEndTransactionIndex + 1; i < plan.SortedLeaveTransactions.Count; i++)
                {
                    if (plan.SortedLeaveTransactions[i].Type == LeaveTransactionType.Earned)
                    {
                        expectedAmount += plan.SortedLeaveTransactions[i].TransactionHours;
                    }
                }
                Assert.AreEqual(expectedAmount, plan.CurrentPlanYearEarnedHours);
            }

            [TestMethod]
            public void CurrentPlanYearUsedHoursTest()
            {
                var plan = planWithTransactions;
                decimal expectedAmount = 0;
                for (int i = plan.PriorPlanYearEndTransactionIndex + 1; i < plan.SortedLeaveTransactions.Count; i++)
                {
                    if (plan.SortedLeaveTransactions[i].Type == LeaveTransactionType.Used)
                    {
                        expectedAmount += plan.SortedLeaveTransactions[i].TransactionHours;
                    }
                }
                Assert.AreEqual(expectedAmount, plan.CurrentPlanYearUsedHours);
            }

            [TestMethod]
            public void DifferentIndexUsedHoursTest()
            {
                employeeLeaveTransactions = new List<EmployeeLeaveTransaction>();
                var plan = planWithTransactions;
                decimal expectedAmount = 0;
                for (int i = plan.PriorPlanYearEndTransactionIndex + 1; i < plan.SortedLeaveTransactions.Count; i++)
                {
                    if (plan.SortedLeaveTransactions[i].Type == LeaveTransactionType.Used)
                    {
                        expectedAmount += plan.SortedLeaveTransactions[i].TransactionHours;
                    }
                }
                Assert.AreEqual(expectedAmount, plan.CurrentPlanYearUsedHours);
            }

            [TestMethod]
            public void CurrentPlanYearAdjustedHoursTest()
            {
                var plan = planWithTransactions;
                decimal expectedAmount = 0;
                for (int i = plan.PriorPlanYearEndTransactionIndex + 1; i < plan.SortedLeaveTransactions.Count; i++)
                {
                    if (plan.SortedLeaveTransactions[i].Type == LeaveTransactionType.Adjusted)
                    {
                        expectedAmount += plan.SortedLeaveTransactions[i].TransactionHours;
                    }
                }
                Assert.AreEqual(expectedAmount, plan.CurrentPlanYearAdjustedHours);
            }

            [TestMethod]
            public void DifferentIndexAdjustedHoursTest()
            {
                employeeLeaveTransactions = new List<EmployeeLeaveTransaction>();
                var plan = planWithTransactions;
                decimal expectedAmount = 0;
                for (int i = plan.PriorPlanYearEndTransactionIndex + 1; i < plan.SortedLeaveTransactions.Count; i++)
                {
                    if (plan.SortedLeaveTransactions[i].Type == LeaveTransactionType.Adjusted)
                    {
                        expectedAmount += plan.SortedLeaveTransactions[i].TransactionHours;
                    }
                }
                Assert.AreEqual(expectedAmount, plan.CurrentPlanYearAdjustedHours);
            }

            [TestMethod]
            public void CurrentPlanYearBalanceTest()
            {
                var plan = planWithTransactions;
                decimal expectedAmount = plan.CurrentPlanYearStartingBalance;
                for (int i = plan.PriorPlanYearEndTransactionIndex + 1; i < plan.SortedLeaveTransactions.Count; i++)
                {
                    expectedAmount += plan.SortedLeaveTransactions[i].TransactionHours;     
                }

                Assert.AreEqual(expectedAmount, plan.CurrentPlanYearBalance);
            }

            [TestMethod]
            public void DifferentIndexCurrentPlanYearBalanceTest()
            {
                employeeLeaveTransactions = new List<EmployeeLeaveTransaction>();
                var plan = planWithTransactions;
                decimal expectedAmount = plan.CurrentPlanYearStartingBalance;
                for (int i = plan.PriorPlanYearEndTransactionIndex + 1; i < plan.SortedLeaveTransactions.Count; i++)
                {
                    expectedAmount += plan.SortedLeaveTransactions[i].TransactionHours;
                }

                Assert.AreEqual(expectedAmount, plan.CurrentPlanYearBalance);
            }

        }

        [TestClass]
        public class AddTransactionsTests : EmployeeLeavePlanTests
        {
            [TestInitialize]
            public void Initialize()
            {
                BaseInitialize();
            }

            //add transactions in reverse order, verify sorted after each add
            [TestMethod]
            public void AddTransactionMaintainsOrderTest()
            {
                var expected = employeeLeaveTransactions.ToList();
                expected.Sort();

                var plan = employeeLeavePlan;

                foreach (var trans in employeeLeaveTransactions.AsEnumerable().Reverse())
                {
                    plan.AddLeaveTransaction(trans);
                }

                CollectionAssert.AreEqual(expected, plan.SortedLeaveTransactions);
            }

            //add transaction range in revser order, verify sorted.
            [TestMethod]
            public void AddTransactionRangeMaintainsOrderTest()
            {
                var expected = employeeLeaveTransactions.ToList();
                expected.Sort();

                var plan = employeeLeavePlan;

                plan.AddLeaveTransactionRange(employeeLeaveTransactions.AsEnumerable().Reverse().ToArray());

                CollectionAssert.AreEqual(expected, plan.SortedLeaveTransactions);
            }

            //does not add duplicate transaction
            [TestMethod]
            public void AddTransactionDoesNotAddDuplicateTest()
            {
                var plan = employeeLeavePlan;
                plan.AddLeaveTransactionRange(employeeLeaveTransactions.ToArray());
                var expectedCount = plan.SortedLeaveTransactions.Count;

                var duplicate = new EmployeeLeaveTransaction(employeeLeaveTransactions.Last().Id, leavePlanId, id, 100, DateTime.Today, LeaveTransactionType.Used, 105);
                plan.AddLeaveTransaction(duplicate);

                Assert.AreEqual(expectedCount, plan.SortedLeaveTransactions.Count);
            }

            [TestMethod]
            public void AddTransactionRangeDoesNotAddDuplicatesTest()
            {
                var plan = employeeLeavePlan;
                var duplicate = new EmployeeLeaveTransaction(employeeLeaveTransactions.Last().Id, leavePlanId, id, 100, DateTime.Today, LeaveTransactionType.Used, 105); //duplicate transaction to be filtered out
                employeeLeaveTransactions.Add(duplicate);

                plan.AddLeaveTransactionRange(employeeLeaveTransactions.ToArray());

                Assert.AreEqual(employeeLeaveTransactions.Count - 1, plan.SortedLeaveTransactions.Count);

                //try adding it directly of of the others
                var expected = plan.SortedLeaveTransactions.Count;
                plan.AddLeaveTransactionRange(duplicate);
                Assert.AreEqual(expected, plan.SortedLeaveTransactions.Count);
            }


            //does not add nonemployeeplan transaction
            [TestMethod]
            public void AddTransactionDoesNotAddTransactionBelongingToDifferentEmployeePlanTest()
            {
                var plan = employeeLeavePlan;
                plan.AddLeaveTransactionRange(employeeLeaveTransactions.ToArray());
                var expectedCount = plan.SortedLeaveTransactions.Count;

                var notAdded = new EmployeeLeaveTransaction(100, leavePlanId, "foobar", 100, DateTime.Today, LeaveTransactionType.Used, 105);
                plan.AddLeaveTransaction(notAdded);
                Assert.AreEqual(expectedCount, plan.SortedLeaveTransactions.Count);
            }

            [TestMethod]
            public void AddTransactionRangeDoesNotAddTransactionBelongingToDifferentEmployeePlanTest()
            {
                var plan = employeeLeavePlan;
                var notAdded = new EmployeeLeaveTransaction(100, leavePlanId, "foobar", 100, DateTime.Today, LeaveTransactionType.Used, 105);
                employeeLeaveTransactions.Add(notAdded);

                plan.AddLeaveTransactionRange(employeeLeaveTransactions.ToArray());

                Assert.AreEqual(employeeLeaveTransactions.Count - 1, plan.SortedLeaveTransactions.Count);

                var expectedCount = plan.SortedLeaveTransactions.Count;

                plan.AddLeaveTransactionRange(notAdded);
                Assert.AreEqual(expectedCount, plan.SortedLeaveTransactions.Count);
            }

            //does not add nonleaveplandefinition transaction
            [TestMethod]
            public void AddTransactionDoesNotAddTransactionBelongingToDifferentLeavePlanTest()
            {
                var plan = employeeLeavePlan;
                plan.AddLeaveTransactionRange(employeeLeaveTransactions.ToArray());
                var expectedCount = plan.SortedLeaveTransactions.Count;

                var notAdded = new EmployeeLeaveTransaction(100, "foobar", id, 100, DateTime.Today, LeaveTransactionType.Used, 105);
                plan.AddLeaveTransaction(notAdded);
                Assert.AreEqual(expectedCount, plan.SortedLeaveTransactions.Count);
            }

            [TestMethod]
            public void AddTransactionRangeDoesNotAddTransactionBelongingToDifferentLeavePlanTest()
            {
                var plan = employeeLeavePlan;
                var notAdded = new EmployeeLeaveTransaction(100, "foobar", id, 100, DateTime.Today, LeaveTransactionType.Used, 105);
                employeeLeaveTransactions.Add(notAdded);

                plan.AddLeaveTransactionRange(employeeLeaveTransactions.ToArray());

                Assert.AreEqual(employeeLeaveTransactions.Count - 1, plan.SortedLeaveTransactions.Count);

                var expectedCount = plan.SortedLeaveTransactions.Count;

                plan.AddLeaveTransactionRange(notAdded);
                Assert.AreEqual(expectedCount, plan.SortedLeaveTransactions.Count);
            }
        }
    }
}
