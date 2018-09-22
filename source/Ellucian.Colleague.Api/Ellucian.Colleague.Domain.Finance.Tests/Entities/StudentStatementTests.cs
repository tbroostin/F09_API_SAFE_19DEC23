// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Finance.Entities;
using Ellucian.Colleague.Domain.Finance.Entities.AccountActivity;
using Ellucian.Colleague.Domain.Finance.Entities.Configuration;
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Finance.Tests.Entities
{
    [TestClass]
    public class StudentStatementTests
    {
        const string StatementMessageLine1 = "Statement Message Line 1";
        const string StatementMessageLine2 = "Statement message Line 2";

        [TestClass]
        public class StudentStatementTests_Constructor : StudentStatementTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentStatementTests_Constructor_NullAccountHolder()
            {
                var statement = new StudentStatement(null, null, DateTime.Today, null, null, 0, 0, 0, null, null, null, 0);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentStatementTests_Constructor_NullTimeframe()
            {
                var statement = new StudentStatement(TestAccountHolderRepository.AccountHolders[0], null, DateTime.Today, null, null, 0, 0, 0, null, null, null, 0);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentStatementTests_Constructor_EmptyTimeframe()
            {
                var statement = new StudentStatement(TestAccountHolderRepository.AccountHolders[0], string.Empty, DateTime.Today, null, null, 0, 0, 0, null, null, null, 0);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentStatementTests_Constructor_NullFinanceConfiguration()
            {
                var statement = new StudentStatement(TestAccountHolderRepository.AccountHolders[0], FinanceTimeframeCodes.PastPeriod, DateTime.Today, null, null, 0, 0, 0, null, null, null, 0);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentStatementTests_Constructor_NullStudentStatementSummary()
            {
                var statement = new StudentStatement(TestAccountHolderRepository.AccountHolders[0], FinanceTimeframeCodes.PastPeriod, DateTime.Today,
                    DateTime.Today.AddDays(7), TestFinanceConfigurationRepository.PeriodFinanceConfiguration, 0, 0, 0, null, null, null, 0);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentStatementTests_Constructor_NullDetailedAccountPeriod()
            {
                var statement = new StudentStatement(TestAccountHolderRepository.AccountHolders[0], FinanceTimeframeCodes.PastPeriod, DateTime.Today,
                    DateTime.Today.AddDays(7), TestFinanceConfigurationRepository.PeriodFinanceConfiguration, 0, 0, 0,
                    TestStudentStatementSummaryRepository.StudentStatementSummary(), null, null, 0);
            }

            [TestMethod]
            public void StudentStatementTests_Constructor_NullSchedule()
            {
                var statement = new StudentStatement(TestAccountHolderRepository.AccountHolders[0], FinanceTimeframeCodes.PastPeriod, DateTime.Today,
                    DateTime.Today.AddDays(7), TestFinanceConfigurationRepository.PeriodFinanceConfiguration, 1600, 0, 1600,
                    TestStudentStatementSummaryRepository.StudentStatementSummary(), TestDetailedAccountPeriodRepository.FullDetailedAccountPeriod("0003315"), null, 0);
            }

            [TestMethod]
            public void StudentStatementTests_Constructor_ZeroTotalAmountDue()
            {
                var statement = new StudentStatement(TestAccountHolderRepository.AccountHolders[0], FinanceTimeframeCodes.PastPeriod, DateTime.Today,
                    DateTime.Today.AddDays(3), TestFinanceConfigurationRepository.PeriodFinanceConfiguration, 1600, 0, 1600,
                    TestStudentStatementSummaryRepository.StudentStatementSummary(), TestDetailedAccountPeriodRepository.FullDetailedAccountPeriod(TestAccountHolderRepository.AccountHolders[0].Id),
                    TestStudentStatementScheduleItemRepository.StudentStatementSchedule(TestAccountHolderRepository.AccountHolders[0].Id), 0);
                Assert.IsNull(statement.DueDate);
            }

            [TestMethod]
            public void StudentStatementTests_Constructor_NullDueDate()
            {
                var statement = new StudentStatement(TestAccountHolderRepository.AccountHolders[0], FinanceTimeframeCodes.PastPeriod, DateTime.Today,
                    null, TestFinanceConfigurationRepository.PeriodFinanceConfiguration, 1600, 1600, 1600,
                    TestStudentStatementSummaryRepository.StudentStatementSummary(), TestDetailedAccountPeriodRepository.FullDetailedAccountPeriod(TestAccountHolderRepository.AccountHolders[0].Id),
                    TestStudentStatementScheduleItemRepository.StudentStatementSchedule(TestAccountHolderRepository.AccountHolders[0].Id), 0);
                Assert.IsNull(statement.DueDate);
                Assert.IsFalse(statement.Overdue);
            }

            [TestMethod]
            public void StudentStatementTests_Constructor_FutureDueDate_NotOverdue()
            {
                var statement = new StudentStatement(TestAccountHolderRepository.AccountHolders[0], FinanceTimeframeCodes.PastPeriod, DateTime.Today,
                    DateTime.Today.AddDays(3), TestFinanceConfigurationRepository.PeriodFinanceConfiguration, 1600, 1600, 1600,
                    TestStudentStatementSummaryRepository.StudentStatementSummary(), TestDetailedAccountPeriodRepository.FullDetailedAccountPeriod(TestAccountHolderRepository.AccountHolders[0].Id),
                    TestStudentStatementScheduleItemRepository.StudentStatementSchedule(TestAccountHolderRepository.AccountHolders[0].Id), 0);
                Assert.AreEqual(DateTime.Today.AddDays(3).ToShortDateString(), statement.DueDate);
                Assert.IsFalse(statement.Overdue);
            }

            [TestMethod]
            public void StudentStatementTests_Constructor_OverdueAmount()
            {
                var statement = new StudentStatement(TestAccountHolderRepository.AccountHolders[0], FinanceTimeframeCodes.PastPeriod, DateTime.Today,
                    DateTime.Today.AddDays(3), TestFinanceConfigurationRepository.PeriodFinanceConfiguration, 1600, 1600, 1600,
                    TestStudentStatementSummaryRepository.StudentStatementSummary(), TestDetailedAccountPeriodRepository.FullDetailedAccountPeriod(TestAccountHolderRepository.AccountHolders[0].Id),
                    TestStudentStatementScheduleItemRepository.StudentStatementSchedule(TestAccountHolderRepository.AccountHolders[0].Id), 500m);
                Assert.AreEqual(500m, statement.OverdueAmount);
            }

            [TestMethod]
            public void StudentStatementTests_Constructor_CurrentAmountDue()
            {
                var statement = new StudentStatement(TestAccountHolderRepository.AccountHolders[0], FinanceTimeframeCodes.PastPeriod, DateTime.Today,
                    DateTime.Today.AddDays(3), TestFinanceConfigurationRepository.PeriodFinanceConfiguration, 1600, 1600, 1600,
                    TestStudentStatementSummaryRepository.StudentStatementSummary(), TestDetailedAccountPeriodRepository.FullDetailedAccountPeriod(TestAccountHolderRepository.AccountHolders[0].Id),
                    TestStudentStatementScheduleItemRepository.StudentStatementSchedule(TestAccountHolderRepository.AccountHolders[0].Id), 500m);
                Assert.AreEqual(1100m, statement.CurrentAmountDue);
            }
        }

        [TestMethod]
        public void StudentStatementTests_StatementMessage_Valid()
        {
            var myConfig = new FinanceConfiguration() { StatementMessage = new List<string>() };
            var statement = new StudentStatement(TestAccountHolderRepository.AccountHolders[0], FinanceTimeframeCodes.PastPeriod, DateTime.Today,
                DateTime.Today.AddDays(3), myConfig, 1600, 1600, 1600,
                TestStudentStatementSummaryRepository.StudentStatementSummary(), TestDetailedAccountPeriodRepository.FullDetailedAccountPeriod(TestAccountHolderRepository.AccountHolders[0].Id),
                TestStudentStatementScheduleItemRepository.StudentStatementSchedule(TestAccountHolderRepository.AccountHolders[0].Id), 0);
            Assert.AreEqual(string.Empty, statement.StatementMessage);
        }

        [TestMethod]
        public void StudentStatementTests_StatementMessage_Valid1()
        {
            var myConfig = new FinanceConfiguration() { StatementMessage = new List<string>() { StatementMessageLine1 } };
            var statement = new StudentStatement(TestAccountHolderRepository.AccountHolders[0], FinanceTimeframeCodes.PastPeriod, DateTime.Today,
                DateTime.Today.AddDays(3),
                myConfig,
                1600, 1600, 1600,
                TestStudentStatementSummaryRepository.StudentStatementSummary(), TestDetailedAccountPeriodRepository.FullDetailedAccountPeriod(TestAccountHolderRepository.AccountHolders[0].Id),
                TestStudentStatementScheduleItemRepository.StudentStatementSchedule(TestAccountHolderRepository.AccountHolders[0].Id), 0);
            var message = myConfig.StatementMessage.ToArray();
            var line1 = message[0];
            Assert.AreEqual(line1, statement.StatementMessage);
        }

        [TestMethod]
        public void StudentStatementTests_StatementMessage_Valid2()
        {
            var myConfig = new FinanceConfiguration() { StatementMessage = new List<string>() { string.Empty, StatementMessageLine2 } };
            var statement = new StudentStatement(TestAccountHolderRepository.AccountHolders[0], FinanceTimeframeCodes.PastPeriod, DateTime.Today,
                DateTime.Today.AddDays(3), myConfig, 1600, 1600, 1600,
                TestStudentStatementSummaryRepository.StudentStatementSummary(), TestDetailedAccountPeriodRepository.FullDetailedAccountPeriod(TestAccountHolderRepository.AccountHolders[0].Id),
                TestStudentStatementScheduleItemRepository.StudentStatementSchedule(TestAccountHolderRepository.AccountHolders[0].Id), 0);
            var message = myConfig.StatementMessage.ToArray();
            var line1 = message[0];
            var line2 = message[1];
            Assert.AreEqual(line2, statement.StatementMessage);
        }

        [TestMethod]
        public void StudentStatementTests_StatementMessage_Valid3()
        {
            var myConfig = new FinanceConfiguration() { StatementMessage = new List<string>() { StatementMessageLine1, StatementMessageLine2 } };
            var statement = new StudentStatement(TestAccountHolderRepository.AccountHolders[0], FinanceTimeframeCodes.PastPeriod, DateTime.Today,
                DateTime.Today.AddDays(3), myConfig, 1600, 1600, 1600,
                TestStudentStatementSummaryRepository.StudentStatementSummary(), TestDetailedAccountPeriodRepository.FullDetailedAccountPeriod(TestAccountHolderRepository.AccountHolders[0].Id),
                TestStudentStatementScheduleItemRepository.StudentStatementSchedule(TestAccountHolderRepository.AccountHolders[0].Id), 0);
            var message = myConfig.StatementMessage.ToArray();
            var line1 = message[0];
            var line2 = message[1];
            Assert.AreEqual(line1 + line2, statement.StatementMessage);
        }
    }
}
