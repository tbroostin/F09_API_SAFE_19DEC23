// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Finance.Entities;
using Ellucian.Colleague.Domain.Finance.Entities.AccountActivity;
using Ellucian.Colleague.Domain.Finance.Entities.AccountDue;
using Ellucian.Colleague.Domain.Finance.Entities.Configuration;
using Ellucian.Colleague.Domain.Finance.Services;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Finance.Tests.Services
{
    [TestClass]
    public class StudentStatementProcessorTests
    {
        List<DepositDue> depositsDue;
        List<DepositType> depositTypes;
        List<Term> terms;
        ActivityDisplay activityDisplay;
        PaymentDisplay paymentDisplay;
        List<FinancialPeriod> financialPeriods;
        List<AccountPeriod> pcfAccountPeriods;
        List<AccountPeriod> termAccountPeriods;
        AccountPeriod nonTermAccountPeriod;
        List<string> termIds;
        List<AccountTerm> accountTerms;
        DetailedAccountPeriod detailedAccountPeriod;
        DetailedAccountPeriod detailedAccountPeriod2;
        DetailedAccountPeriod detailedAccountPeriod3;

        StudentStatementProcessor processor;

        [TestInitialize]
        public void Initialize()
        {
            depositsDue = TestDepositsDueRepository.DepositsDue;
            depositTypes = TestDepositTypesRepository.DepositTypes;
            terms = new TestTermRepository().Get().ToList();
            activityDisplay = ActivityDisplay.DisplayByTerm;
            paymentDisplay = PaymentDisplay.DisplayByTerm;
            financialPeriods = TestFinancialPeriodRepository.FinancialPeriods;
            pcfAccountPeriods = new List<Domain.Finance.Entities.AccountActivity.AccountPeriod>()
            {
                new Domain.Finance.Entities.AccountActivity.AccountPeriod()
                {
                    AssociatedPeriods = new List<string>() { "2013/FA", "2014/SP", "2014/S1"},
                    Balance = 10000m,
                    Description = "Past",
                    Id = FinanceTimeframeCodes.PastPeriod,
                    EndDate = DateTime.Today.AddDays(-30),
                    StartDate = null
                },
                new Domain.Finance.Entities.AccountActivity.AccountPeriod()
                {
                    AssociatedPeriods = new List<string>() { "2014/FA", "2015/SP", "2015/S1"},
                    Balance = 5000m,
                    Description = "Current",
                    Id = FinanceTimeframeCodes.CurrentPeriod,
                    EndDate = DateTime.Today.AddDays(30),
                    StartDate = DateTime.Today.AddDays(-29)
                },
                new Domain.Finance.Entities.AccountActivity.AccountPeriod()
                {
                    AssociatedPeriods = new List<string>() { "2015/FA", "2016/SP", "2016/S1"},
                    Balance = 1000m,
                    Description = "Future",
                    Id = FinanceTimeframeCodes.FuturePeriod,
                    EndDate = null,
                    StartDate = DateTime.Today.AddDays(31)
                }
            };
            termAccountPeriods = new List<Domain.Finance.Entities.AccountActivity.AccountPeriod>()
            {
                new Domain.Finance.Entities.AccountActivity.AccountPeriod()
                {
                    AssociatedPeriods = null,
                    Balance = 10000m,
                    Description = "2014 Spring Term",
                    Id = "2014/SP",
                    EndDate = DateTime.Today.AddDays(-30),
                    StartDate = null
                },
                new Domain.Finance.Entities.AccountActivity.AccountPeriod()
                {
                    AssociatedPeriods = null,
                    Balance = 5000m,
                    Description = "2015 Spring Term",
                    Id = "2015/SP",
                    EndDate = DateTime.Today.AddDays(30),
                    StartDate = DateTime.Today.AddDays(-29)
                },
                new Domain.Finance.Entities.AccountActivity.AccountPeriod()
                {
                    AssociatedPeriods = null,
                    Balance = 1000m,
                    Description = "2015 Fall Term",
                    Id = "2015/FA",
                    EndDate = null,
                    StartDate = DateTime.Today.AddDays(31)
                }
            };
            nonTermAccountPeriod = new Domain.Finance.Entities.AccountActivity.AccountPeriod()
            {
                AssociatedPeriods = null,
                Balance = 10000m,
                Description = "Other",
                Id = FinanceTimeframeCodes.NonTerm,
                EndDate = null,
                StartDate = null
            };
            termIds = new List<string>(pcfAccountPeriods.SelectMany(ap => ap.AssociatedPeriods));
            processor = new StudentStatementProcessor("2015/SP", activityDisplay, paymentDisplay, depositTypes, terms,
                termAccountPeriods, financialPeriods);
            accountTerms = new List<Domain.Finance.Entities.AccountDue.AccountTerm>()
            {
                new Domain.Finance.Entities.AccountDue.AccountTerm()
                {
                    AccountDetails = new List<Domain.Finance.Entities.AccountDue.AccountsReceivableDueItem>()
                    {
                        new Domain.Finance.Entities.AccountDue.AccountsReceivableDueItem()
                        {
                            AccountDescription = "Account Description",
                            AccountType = "Account Type",
                            AmountDue = 5000m,
                            Description = "Description",
                            Distribution = "Distribution",
                            DueDate = DateTime.Today.AddMonths(1),
                            Overdue = false,
                            Period = FinanceTimeframeCodes.CurrentPeriod,
                            PeriodDescription = "Current",
                            Term = "2015/SP",
                            TermDescription = "2015 Spring Term"
                        },
                        new Domain.Finance.Entities.AccountDue.InvoiceDueItem()
                        {
                            AccountDescription = "Account Description",
                            AccountType = "Account Type",
                            AmountDue = 4000m,
                            Description = "Description",
                            Distribution = "Distribution",
                            DueDate = DateTime.Today.AddDays(10),
                            InvoiceId = "Invoice ID",
                            Overdue = false,
                            Period = FinanceTimeframeCodes.CurrentPeriod,
                            PeriodDescription = "Current",
                            Term = "2015/SP",
                            TermDescription = "2015 Spring Term"
                        },
                        new Domain.Finance.Entities.AccountDue.PaymentPlanDueItem()
                        {
                            AccountDescription = "Account Description",
                            AccountType = "Account Type",
                            AmountDue = 250m,
                            Description = "Description",
                            Distribution = "Distribution",
                            DueDate = DateTime.Today.AddMonths(1),
                            Overdue = false,
                            PaymentPlanCurrent = true,
                            PaymentPlanId = "Payment Plan ID1",
                            UnpaidAmount = 250m,
                            Period = FinanceTimeframeCodes.CurrentPeriod,
                            PeriodDescription = "Current",
                            Term = "2015/SP",
                            TermDescription = "2015 Spring Term"
                        },
                        new Domain.Finance.Entities.AccountDue.PaymentPlanDueItem()
                        {
                            AccountDescription = "Account Description",
                            AccountType = "Account Type",
                            AmountDue = 150m,
                            Description = "Description",
                            Distribution = "Distribution",
                            DueDate = DateTime.Today.AddMonths(-1),
                            Overdue = true,
                            PaymentPlanCurrent = false,
                            PaymentPlanId = "Payment Plan ID1",
                            UnpaidAmount = 100m,
                            Period = FinanceTimeframeCodes.CurrentPeriod,
                            PeriodDescription = "Current",
                            Term = "2015/SP",
                            TermDescription = "2015 Spring Term"
                        },
                        new Domain.Finance.Entities.AccountDue.PaymentPlanDueItem()
                        {
                            AccountDescription = "Account Description",
                            AccountType = "Account Type",
                            AmountDue = 100m,
                            Description = "Description",
                            Distribution = "Distribution",
                            DueDate = DateTime.Today.AddMonths(-2),
                            Overdue = false,
                            PaymentPlanCurrent = false,
                            PaymentPlanId = "Payment Plan ID1",
                            UnpaidAmount = 0m,
                            Period = FinanceTimeframeCodes.CurrentPeriod,
                            PeriodDescription = "Current",
                            Term = "2015/SP",
                            TermDescription = "2015 Spring Term",
                        }
                    },
                    Amount = 10000m,
                    Description = "2015 Spring Term",
                    TermId = "2015/SP"
                },
                new Domain.Finance.Entities.AccountDue.AccountTerm()
                {
                    AccountDetails = new List<Domain.Finance.Entities.AccountDue.AccountsReceivableDueItem>()
                    {
                        new Domain.Finance.Entities.AccountDue.AccountsReceivableDueItem()
                        {
                            AccountDescription = "Account Description",
                            AccountType = "Account Type",
                            AmountDue = 5000m,
                            Description = "Description",
                            Distribution = "Distribution",
                            DueDate = DateTime.Today.AddMonths(11),
                            Overdue = false,
                            Period = FinanceTimeframeCodes.PastPeriod,
                            PeriodDescription = "Past",
                            Term = "2014/FA",
                            TermDescription = "2014 Fall Term"
                        },
                        new Domain.Finance.Entities.AccountDue.InvoiceDueItem()
                        {
                            AccountDescription = "Account Description",
                            AccountType = "Account Type",
                            AmountDue = 4000m,
                            Description = "Description",
                            Distribution = "Distribution",
                            DueDate = DateTime.Today.AddDays(110),
                            InvoiceId = "Invoice ID",
                            Overdue = false,
                            Period = FinanceTimeframeCodes.PastPeriod,
                            PeriodDescription = "Past",
                            Term = "2014/FA",
                            TermDescription = "2014 Fall Term"
                        },
                        new Domain.Finance.Entities.AccountDue.PaymentPlanDueItem()
                        {
                            AccountDescription = "Account Description",
                            AccountType = "Account Type",
                            AmountDue = -250m,
                            Description = "Description",
                            Distribution = "Distribution",
                            DueDate = DateTime.Today.AddMonths(11),
                            Overdue = false,
                            PaymentPlanCurrent = true,
                            PaymentPlanId = "Payment Plan ID1",
                            UnpaidAmount = 250m,
                            Period = FinanceTimeframeCodes.PastPeriod,
                            PeriodDescription = "Past",
                            Term = "2014/FA",
                            TermDescription = "2014 Fall Term"
                        },
                        new Domain.Finance.Entities.AccountDue.PaymentPlanDueItem()
                        {
                            AccountDescription = "Account Description",
                            AccountType = "Account Type",
                            AmountDue = null,
                            Description = "Description",
                            Distribution = "Distribution",
                            DueDate = DateTime.Today.AddMonths(-11),
                            Overdue = true,
                            PaymentPlanCurrent = false,
                            PaymentPlanId = "Payment Plan ID1",
                            UnpaidAmount = 100m,
                            Period = FinanceTimeframeCodes.PastPeriod,
                            PeriodDescription = "Past",
                            Term = "2014/FA",
                            TermDescription = "2014 Fall Term"
                        },
                        new Domain.Finance.Entities.AccountDue.PaymentPlanDueItem()
                        {
                            AccountDescription = "Account Description",
                            AccountType = "Account Type",
                            AmountDue = 100m,
                            Description = "Description",
                            Distribution = "Distribution",
                            DueDate = DateTime.Today.AddMonths(-12),
                            Overdue = false,
                            PaymentPlanCurrent = false,
                            PaymentPlanId = "Payment Plan ID1",
                            UnpaidAmount = 0m,
                            Period = FinanceTimeframeCodes.PastPeriod,
                            PeriodDescription = "Past",
                            Term = "2014/FA",
                            TermDescription = "2014 Fall Term",
                        }
                    },
                    Amount = 10000m,
                    Description = "2014 Fall Term",
                    TermId = "2014/FA"
                },
                new Domain.Finance.Entities.AccountDue.AccountTerm()
                {
                    AccountDetails = new List<Domain.Finance.Entities.AccountDue.AccountsReceivableDueItem>()
                    {
                        new Domain.Finance.Entities.AccountDue.AccountsReceivableDueItem()
                        {
                            AccountDescription = "Account Description",
                            AccountType = "Account Type",
                            AmountDue = 5000m,
                            Description = "Description",
                            Distribution = "Distribution",
                            DueDate = DateTime.Today.AddMonths(11),
                            Overdue = false,
                            Period = FinanceTimeframeCodes.PastPeriod,
                            PeriodDescription = "Past",
                            Term = "2012/FA",
                            TermDescription = "2012 Fall Term"
                        },
                        new Domain.Finance.Entities.AccountDue.InvoiceDueItem()
                        {
                            AccountDescription = "Account Description",
                            AccountType = "Account Type",
                            AmountDue = 4000m,
                            Description = "Description",
                            Distribution = "Distribution",
                            DueDate = DateTime.Today.AddDays(110),
                            InvoiceId = "Invoice ID",
                            Overdue = false,
                            Period = FinanceTimeframeCodes.PastPeriod,
                            PeriodDescription = "Past",
                            Term = "2012/FA",
                            TermDescription = "2012 Fall Term"
                        },
                        new Domain.Finance.Entities.AccountDue.PaymentPlanDueItem()
                        {
                            AccountDescription = "Account Description",
                            AccountType = "Account Type",
                            AmountDue = -250m,
                            Description = "Description",
                            Distribution = "Distribution",
                            DueDate = DateTime.Today.AddMonths(11),
                            Overdue = false,
                            PaymentPlanCurrent = true,
                            PaymentPlanId = "Payment Plan ID1",
                            UnpaidAmount = 250m,
                            Period = FinanceTimeframeCodes.PastPeriod,
                            PeriodDescription = "Past",
                            Term = "2012/FA",
                            TermDescription = "2012 Fall Term"
                        },
                        new Domain.Finance.Entities.AccountDue.PaymentPlanDueItem()
                        {
                            AccountDescription = "Account Description",
                            AccountType = "Account Type",
                            AmountDue = null,
                            Description = "Description",
                            Distribution = "Distribution",
                            DueDate = DateTime.Today.AddMonths(-11),
                            Overdue = true,
                            PaymentPlanCurrent = false,
                            PaymentPlanId = "Payment Plan ID1",
                            UnpaidAmount = 100m,
                            Period = FinanceTimeframeCodes.PastPeriod,
                            PeriodDescription = "Past",
                            Term = "2012/FA",
                            TermDescription = "2012 Fall Term"
                        },
                        new Domain.Finance.Entities.AccountDue.PaymentPlanDueItem()
                        {
                            AccountDescription = "Account Description",
                            AccountType = "Account Type",
                            AmountDue = 100m,
                            Description = "Description",
                            Distribution = "Distribution",
                            DueDate = DateTime.Today.AddMonths(-12),
                            Overdue = false,
                            PaymentPlanCurrent = false,
                            PaymentPlanId = "Payment Plan ID1",
                            UnpaidAmount = 0m,
                            Period = FinanceTimeframeCodes.PastPeriod,
                            PeriodDescription = "Past",
                            Term = "2012/FA",
                            TermDescription = "2012 Fall Term",
                        }
                    },
                    Amount = 10000m,
                    Description = "2012 Fall Term",
                    TermId = "2012/FA"
                },
                new AccountTerm()
                {
                    AccountDetails = new List<AccountsReceivableDueItem>(),
                    Amount = 0m,
                    Description = "2014 Spring Term",
                    TermId = "2014/SP"
                },
                new AccountTerm()
                {
                    AccountDetails = null,
                    Amount = 0m,
                    Description = "2013 Fall Term",
                    TermId = "2013/FA"
                },

            };
            detailedAccountPeriod = TestDetailedAccountPeriodRepository.FullDetailedAccountPeriod("0000895");
            detailedAccountPeriod2 = TestDetailedAccountPeriodRepository.FullDetailedAccountPeriod("0000895");
            detailedAccountPeriod2.Charges = null;
            detailedAccountPeriod2.StudentPayments = null;
            detailedAccountPeriod2.Deposits = null;
            detailedAccountPeriod3 = TestDetailedAccountPeriodRepository.FullDetailedAccountPeriod("0000895");
            detailedAccountPeriod3.FinancialAid = null;
            detailedAccountPeriod3.Sponsorships = null;
            detailedAccountPeriod3.Refunds = null;
        }

        [TestCleanup]
        public void Cleanup()
        {
            depositsDue = null;
            depositTypes = null;
            terms = null;
            activityDisplay = ActivityDisplay.DisplayByTerm;
            paymentDisplay = PaymentDisplay.DisplayByTerm;
            financialPeriods = null;
            pcfAccountPeriods = null;
            termAccountPeriods = null;
            nonTermAccountPeriod = null;
            detailedAccountPeriod = null;
            termIds = null;
            accountTerms = null;
        }

        [TestClass]
        public class StudentStatementProcessor_Constructor : StudentStatementProcessorTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentStatementProcessor_Constructor_NullTimeframeId()
            {
                var processor = new StudentStatementProcessor(null, activityDisplay, paymentDisplay, depositTypes, terms,
                    pcfAccountPeriods, financialPeriods);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentStatementProcessor_Constructor_EmptyTimeframeId()
            {
                var processor = new StudentStatementProcessor(string.Empty, activityDisplay, paymentDisplay, depositTypes, terms,
                    pcfAccountPeriods, financialPeriods);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentStatementProcessor_Constructor_NullTerms()
            {
                var processor = new StudentStatementProcessor("2015/SP", activityDisplay, paymentDisplay, depositTypes, null,
                    pcfAccountPeriods, financialPeriods);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentStatementProcessor_Constructor_NoTerms()
            {
                var processor = new StudentStatementProcessor("2015/SP", activityDisplay, paymentDisplay, depositTypes, new List<Term>(),
                    pcfAccountPeriods, financialPeriods);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentStatementProcessor_Constructor_NullAccountPeriods()
            {
                var processor = new StudentStatementProcessor("2015/SP", activityDisplay, paymentDisplay, depositTypes, terms,
                    null, financialPeriods);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentStatementProcessor_Constructor_NullFinancialPeriods()
            {
                var processor = new StudentStatementProcessor("2015/SP", activityDisplay, paymentDisplay, depositTypes, terms,
                    pcfAccountPeriods, null);
            }

            [TestMethod]
            public void StudentStatementProcessor_Constructor_Valid()
            {
                var processor = new StudentStatementProcessor("2015/SP", activityDisplay, paymentDisplay, depositTypes, terms,
                    new List<AccountPeriod>(), financialPeriods);
            }
        }

        [TestClass]
        public class StudentStatementProcessor_FilterAndUpdateDepositsDue : StudentStatementProcessorTests
        {
            [TestMethod]
            public void StudentStatementProcessor_FilterAndUpdateDepositsDue_NullDepositsDue()
            {
                var filteredDepositsDue = processor.FilterAndUpdateDepositsDue(null, termIds, DateTime.Today.AddDays(-30),
                    DateTime.Today.AddDays(30));
                Assert.IsNull(filteredDepositsDue);
            }

            [TestMethod]
            public void StudentStatementProcessor_FilterAndUpdateDepositsDue_NoDepositsDue()
            {
                var filteredDepositsDue = processor.FilterAndUpdateDepositsDue(new List<DepositDue>(), termIds, DateTime.Today.AddDays(-30),
                    DateTime.Today.AddDays(30));
                Assert.AreEqual(0, filteredDepositsDue.Count());

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentStatementProcessor_FilterAndUpdateDepositsDue_NullTermIds()
            {
                var filteredDepositsDue = processor.FilterAndUpdateDepositsDue(depositsDue, null, DateTime.Today.AddDays(-30),
                    DateTime.Today.AddDays(30));
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentStatementProcessor_FilterAndUpdateDepositsDue_NoTermIds()
            {
                var filteredDepositsDue = processor.FilterAndUpdateDepositsDue(depositsDue, new List<string>(), DateTime.Today.AddDays(-30),
                    DateTime.Today.AddDays(30));
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void StudentStatementProcessor_FilterAndUpdateDepositsDue_MultipleTermIdsAndNoDates()
            {
                var filteredDepositsDue = processor.FilterAndUpdateDepositsDue(depositsDue, termIds, null, null);
            }

            [TestMethod]
            public void StudentStatementProcessor_FilterAndUpdateDepositsDue_ActivityDisplayByTerm()
            {
                var depositsDueToFilter = depositsDue.Where(dd => dd.PersonId == "0000004");
                var termId = new List<string>() { "2015/SP" };
                var filteredDepositsDue = processor.FilterAndUpdateDepositsDue(depositsDueToFilter, termId, null, null).ToList();
                Assert.AreEqual(2, filteredDepositsDue.Count);
                Assert.AreEqual("Hr Prepaid Benefit Deposits", filteredDepositsDue[0].DepositTypeDescription);
                Assert.AreEqual("Meal Plan Deposit", filteredDepositsDue[1].DepositTypeDescription);
                Assert.AreEqual("2015 Spring Term", filteredDepositsDue[0].TermDescription);
                Assert.AreEqual("2015 Spring Term", filteredDepositsDue[1].TermDescription);
            }

            [TestMethod]
            public void StudentStatementProcessor_FilterAndUpdateDepositsDue_ActivityDisplayByPeriod()
            {
                processor = new StudentStatementProcessor("2015/SP", ActivityDisplay.DisplayByPeriod, paymentDisplay, depositTypes, terms,
                    pcfAccountPeriods, financialPeriods);
                var depositsDueToFilter = depositsDue.Where(dd => dd.PersonId == "0000004");
                var termId = new List<string>() { "2015/SP" };
                var filteredDepositsDue = processor.FilterAndUpdateDepositsDue(depositsDueToFilter, termId, DateTime.Parse("01/01/2015"),
                    DateTime.Parse("06/01/2015")).ToList();
                Assert.AreEqual(2, filteredDepositsDue.Count);
                Assert.AreEqual("Hr Prepaid Benefit Deposits", filteredDepositsDue[0].DepositTypeDescription);
                Assert.AreEqual("Meal Plan Deposit", filteredDepositsDue[1].DepositTypeDescription);
                Assert.AreEqual("2015 Spring Term", filteredDepositsDue[0].TermDescription);
                Assert.AreEqual("2015 Spring Term", filteredDepositsDue[1].TermDescription);
            }

            [TestMethod]
            public void StudentStatementProcessor_FilterAndUpdateDepositsDue_NoDepositsDueRemainAfterFiltering()
            {
                var depositsDueToFilter = depositsDue.Where(dd => dd.PersonId == "0000004");
                var termId = new List<string>() { "2014/SP" };
                var filteredDepositsDue = processor.FilterAndUpdateDepositsDue(depositsDueToFilter, termId, null, null).ToList();
                Assert.AreEqual(0, filteredDepositsDue.Count);
            }
        }

        [TestClass]
        public class StudentStatementProcessor_GetTermIdsForTimeframe : StudentStatementProcessorTests
        {
            [TestMethod]
            public void StudentStatementProcessor_GetTermIdsForTimeframe_ActivityDisplayByTerm()
            {
                var termIds = processor.GetTermIdsForTimeframe();
                Assert.AreEqual(1, termIds.Count());
                Assert.AreEqual("2015/SP", termIds.First());
            }

            [TestMethod]
            public void StudentStatementProcessor_GetTermIdsForTimeframe_PastPeriod()
            {
                processor = new StudentStatementProcessor(FinanceTimeframeCodes.PastPeriod, ActivityDisplay.DisplayByPeriod, paymentDisplay,
                    depositTypes, terms, pcfAccountPeriods, financialPeriods);
                var termIds = processor.GetTermIdsForTimeframe();
                Assert.AreEqual(terms.Where(t => t.FinancialPeriod == PeriodType.Past).Count(), termIds.Count());
            }

            [TestMethod]
            public void StudentStatementProcessor_GetTermIdsForTimeframe_CurrentPeriod()
            {
                processor = new StudentStatementProcessor(FinanceTimeframeCodes.CurrentPeriod, ActivityDisplay.DisplayByPeriod, paymentDisplay,
                    depositTypes, terms, pcfAccountPeriods, financialPeriods);
                var termIds = processor.GetTermIdsForTimeframe();
                Assert.AreEqual(terms.Where(t => t.FinancialPeriod == PeriodType.Current).Count(), termIds.Count());
            }

            [TestMethod]
            public void StudentStatementProcessor_GetTermIdsForTimeframe_FuturePeriod()
            {
                processor = new StudentStatementProcessor(FinanceTimeframeCodes.FuturePeriod, ActivityDisplay.DisplayByPeriod, paymentDisplay,
                    depositTypes, terms, pcfAccountPeriods, financialPeriods);
                var termIds = processor.GetTermIdsForTimeframe();
                Assert.AreEqual(terms.Where(t => t.FinancialPeriod == PeriodType.Future).Count(), termIds.Count());
            }

            [TestMethod]
            public void StudentStatementProcessor_GetTermIdsForTimeframe_InvalidPeriod()
            {
                processor = new StudentStatementProcessor("2015/SP", ActivityDisplay.DisplayByPeriod, paymentDisplay,
                    depositTypes, terms, pcfAccountPeriods, financialPeriods);
                var termIds = processor.GetTermIdsForTimeframe();
                Assert.AreEqual(1, termIds.Count());
                Assert.AreEqual("2015/SP", termIds.First());
            }
        }

        [TestClass]
        public class StudentStatementProcessor_CalculateDueDate : StudentStatementProcessorTests
        {
            [TestMethod]
            public void StudentStatementProcessor_CalculateDueDate_NullAccountTerms_NoDepositsDue()
            {
                var dueDate = processor.CalculateDueDate(null, new List<DepositDue>());
                Assert.IsNull(dueDate);
            }

            [TestMethod]
            public void StudentStatementProcessor_CalculateDueDate_NoAccountTerms_NullDepositsDue()
            {
                var dueDate = processor.CalculateDueDate(new List<AccountTerm>(), null);
                Assert.IsNull(dueDate);
            }

            [TestMethod]
            public void StudentStatementProcessor_CalculateDueDate_FutureDueDate()
            {
                var dueDate = processor.CalculateDueDate(accountTerms, depositsDue.Where(d => d.DueDate < DateTime.Today));
                Assert.AreEqual(DateTime.Today.AddDays(10), dueDate);
            }

            [TestMethod]
            public void StudentStatementProcessor_CalculateDueDate_TodayDueDate()
            {
                accountTerms = new List<Domain.Finance.Entities.AccountDue.AccountTerm>()
                {
                    new Domain.Finance.Entities.AccountDue.AccountTerm()
                    {
                        AccountDetails = new List<Domain.Finance.Entities.AccountDue.AccountsReceivableDueItem>()
                        {
                            new Domain.Finance.Entities.AccountDue.AccountsReceivableDueItem()
                            {
                                AccountDescription = "Account Description",
                                AccountType = "Account Type",
                                AmountDue = 5000m,
                                Description = "Description",
                                Distribution = "Distribution",
                                DueDate = DateTime.Today.AddMonths(1),
                                Overdue = false,
                                Period = FinanceTimeframeCodes.CurrentPeriod,
                                PeriodDescription = "Current",
                                Term = "2015/SP",
                                TermDescription = "2015 Spring Term"
                            },
                            new Domain.Finance.Entities.AccountDue.InvoiceDueItem()
                            {
                                AccountDescription = "Account Description",
                                AccountType = "Account Type",
                                AmountDue = 4000m,
                                Description = "Description",
                                Distribution = "Distribution",
                                DueDate = DateTime.Today,
                                InvoiceId = "Invoice ID",
                                Overdue = false,
                                Period = FinanceTimeframeCodes.CurrentPeriod,
                                PeriodDescription = "Current",
                                Term = "2015/SP",
                                TermDescription = "2015 Spring Term"
                            }
                        },
                        Amount = 10000m,
                        Description = "2015 Spring Term",
                        TermId = "2015/SP"
                    }
                };
                var dueDate = processor.CalculateDueDate(accountTerms, depositsDue);
                Assert.AreEqual(DateTime.Today, dueDate);
            }
        }

        [TestClass]
        public class StudentStatementProcessor_CalculatePreviousBalance : StudentStatementProcessorTests
        {
            [TestMethod]
            public void StudentStatementProcessor_CalculatePreviousBalance_ActivityDisplayByTerm()
            {
                var previousBalance = processor.CalculatePreviousBalance();
                Assert.AreEqual(10000m, previousBalance);
            }

            [TestMethod]
            public void StudentStatementProcessor_CalculatePreviousBalance_PastPeriod()
            {
                processor = new StudentStatementProcessor(FinanceTimeframeCodes.PastPeriod, ActivityDisplay.DisplayByPeriod,
                    paymentDisplay, depositTypes, terms, pcfAccountPeriods, financialPeriods);
                var previousBalance = processor.CalculatePreviousBalance();
                Assert.AreEqual(0, previousBalance);
            }

            [TestMethod]
            public void StudentStatementProcessor_CalculatePreviousBalance_CurrentPeriod()
            {
                processor = new StudentStatementProcessor(FinanceTimeframeCodes.CurrentPeriod, ActivityDisplay.DisplayByPeriod,
                    paymentDisplay, depositTypes, terms, pcfAccountPeriods, financialPeriods);
                var previousBalance = processor.CalculatePreviousBalance();
                Assert.AreEqual(10000m, previousBalance);
            }

            [TestMethod]
            public void StudentStatementProcessor_CalculatePreviousBalance_CurrentPeriod_NullPastPeriodBalance()
            {
                var periods = pcfAccountPeriods;
                foreach (var p in periods)
                {
                    p.Balance = null;
                }
                processor = new StudentStatementProcessor(FinanceTimeframeCodes.CurrentPeriod, ActivityDisplay.DisplayByPeriod,
                    paymentDisplay, depositTypes, terms, periods, financialPeriods);
                var previousBalance = processor.CalculatePreviousBalance();
                Assert.AreEqual(0m, previousBalance);
            }

            [TestMethod]
            public void StudentStatementProcessor_CalculatePreviousBalance_FuturePeriod()
            {
                processor = new StudentStatementProcessor(FinanceTimeframeCodes.FuturePeriod, ActivityDisplay.DisplayByPeriod,
                    paymentDisplay, depositTypes, terms, pcfAccountPeriods, financialPeriods);
                var previousBalance = processor.CalculatePreviousBalance();
                Assert.AreEqual(15000m, previousBalance);
            }
        }

        [TestClass]
        public class StudentStatementProcessor_CalculateFutureBalance : StudentStatementProcessorTests
        {
            [TestMethod]
            public void StudentStatementProcessor_CalculateFutureBalance_ActivityDisplayByTerm()
            {
                var futureBalance = processor.CalculateFutureBalance();
                Assert.AreEqual(1000m, futureBalance);
            }

            [TestMethod]
            public void StudentStatementProcessor_CalculateFutureBalance_PastPeriod()
            {
                processor = new StudentStatementProcessor(FinanceTimeframeCodes.PastPeriod, ActivityDisplay.DisplayByPeriod,
                    paymentDisplay, depositTypes, terms, pcfAccountPeriods, financialPeriods);
                var futureBalance = processor.CalculateFutureBalance();
                Assert.AreEqual(6000m, futureBalance);
            }

            [TestMethod]
            public void StudentStatementProcessor_CalculateFutureBalance_PastPeriod_NullCurrentAndFuturePeriodBalance()
            {
                var periods = pcfAccountPeriods;
                foreach (var p in periods)
                {
                    p.Balance = null;
                }
                processor = new StudentStatementProcessor(FinanceTimeframeCodes.PastPeriod, ActivityDisplay.DisplayByPeriod,
                    paymentDisplay, depositTypes, terms, periods, financialPeriods);
                var futureBalance = processor.CalculateFutureBalance();
                Assert.AreEqual(0m, futureBalance);
            }

            [TestMethod]
            public void StudentStatementProcessor_CalculateFutureBalance_CurrentPeriod()
            {
                processor = new StudentStatementProcessor(FinanceTimeframeCodes.CurrentPeriod, ActivityDisplay.DisplayByPeriod,
                    paymentDisplay, depositTypes, terms, pcfAccountPeriods, financialPeriods);
                var futureBalance = processor.CalculateFutureBalance();
                Assert.AreEqual(1000m, futureBalance);
            }

            [TestMethod]
            public void StudentStatementProcessor_CalculateFutureBalance_CurrentPeriod_NullFuturePeriodBalance()
            {
                var periods = pcfAccountPeriods;
                foreach (var p in periods)
                {
                    p.Balance = null;
                }
                processor = new StudentStatementProcessor(FinanceTimeframeCodes.CurrentPeriod, ActivityDisplay.DisplayByPeriod,
                    paymentDisplay, depositTypes, terms, periods, financialPeriods);
                var futureBalance = processor.CalculateFutureBalance();
                Assert.AreEqual(0m, futureBalance);
            }

            [TestMethod]
            public void StudentStatementProcessor_CalculateFutureBalance_FuturePeriod()
            {
                processor = new StudentStatementProcessor(FinanceTimeframeCodes.FuturePeriod, ActivityDisplay.DisplayByPeriod,
                    paymentDisplay, depositTypes, terms, pcfAccountPeriods, financialPeriods);
                var futureBalance = processor.CalculateFutureBalance();
                Assert.AreEqual(0m, futureBalance);
            }
        }

        [TestClass]
        public class StudentStatementProcessor_CalculateOtherBalance : StudentStatementProcessorTests
        {
            [TestMethod]
            public void StudentStatementProcessor_CalculateOtherBalance_ActivityDisplayByPeriod()
            {
                processor = new StudentStatementProcessor(FinanceTimeframeCodes.PastPeriod, ActivityDisplay.DisplayByPeriod,
                    paymentDisplay, depositTypes, terms, pcfAccountPeriods, financialPeriods);
                var otherBalance = processor.CalculateOtherBalance(nonTermAccountPeriod);
                Assert.AreEqual(0m, otherBalance);
            }

            [TestMethod]
            public void StudentStatementProcessor_CalculateOtherBalance_NonTerm()
            {
                processor = new StudentStatementProcessor(FinanceTimeframeCodes.NonTerm, activityDisplay,
                    paymentDisplay, depositTypes, terms, termAccountPeriods, financialPeriods);
                var otherBalance = processor.CalculateOtherBalance(nonTermAccountPeriod);
                Assert.AreEqual(16000m, otherBalance);
            }

            [TestMethod]
            public void StudentStatementProcessor_CalculateOtherBalance_Term()
            {
                processor = new StudentStatementProcessor("2014/SP", activityDisplay,
                    paymentDisplay, depositTypes, terms, termAccountPeriods, financialPeriods);
                var otherBalance = processor.CalculateOtherBalance(nonTermAccountPeriod);
                Assert.AreEqual(nonTermAccountPeriod.Balance, otherBalance);
            }
        }

        [TestClass]
        public class StudentStatementProcessor_CalculateCurrentBalance : StudentStatementProcessorTests
        {
            [TestMethod]
            public void StudentStatementProcessor_CalculateCurrentBalance_NullAccountDetails()
            {
                var currentBalance = processor.CalculateCurrentBalance(null);
                Assert.AreEqual(0m, currentBalance);
            }

            [TestMethod]
            public void StudentStatementProcessor_CalculateCurrentBalance_NullChargesPaymentsDeposits()
            {
                var currentBalance = processor.CalculateCurrentBalance(detailedAccountPeriod2);
                Assert.AreEqual(-8553m, currentBalance);
            }

            [TestMethod]
            public void StudentStatementProcessor_CalculateCurrentBalance_NullFinancialAidSponsorshipsRefunds()
            {
                var currentBalance = processor.CalculateCurrentBalance(detailedAccountPeriod3);
                Assert.AreEqual(12500m, currentBalance);
            }
        }

        [TestClass]
        public class StudentStatementProcessor_CalculateTotalAmountDue : StudentStatementProcessorTests
        {
            [TestMethod]
            public void StudentStatementProcessor_CalculateTotalAmountDue_Valid()
            {
                var totalAmountDue = processor.CalculateTotalAmountDue(100, 200, 300, 400, 500);
                Assert.AreEqual(900m, totalAmountDue);
            }
        }

        [TestClass]
        public class StudentStatementProcessor_BuildStatementSummary : StudentStatementProcessorTests
        {
            [TestMethod]
            public void StudentStatementProcessor_BuildStatementSummary_NullAccountDetails()
            {
                var summary = processor.BuildStatementSummary(null, accountTerms, 0m, DateTime.Today.AddDays(-30), DateTime.Today.AddDays(30));
                Assert.IsNull(summary);
            }
        }

        [TestClass]
        public class StudentStatementProcessor_BuildPreviousBalanceDescription : StudentStatementProcessorTests
        {
            [TestMethod]
            public void StudentStatementProcessor_BuildPreviousBalanceDescription_ActivityDisplayByTerm_MatchingTerm()
            {
                var desc = processor.BuildPreviousBalanceDescription(null);
                Assert.AreEqual("Charges before 2015 Spring Term", desc);
            }

            [TestMethod]
            public void StudentStatementProcessor_BuildPreviousBalanceDescription_ActivityDisplayByTerm_NoMatchingTerm()
            {
                processor = new StudentStatementProcessor("ABC123", activityDisplay, paymentDisplay, depositTypes,
                    terms, termAccountPeriods, financialPeriods);
                var desc = processor.BuildPreviousBalanceDescription(null);
                Assert.AreEqual("Charges before ", desc);
            }

            [TestMethod]
            public void StudentStatementProcessor_BuildPreviousBalanceDescription_ActivityDisplayByPeriod()
            {
                processor = new StudentStatementProcessor(FinanceTimeframeCodes.CurrentPeriod, ActivityDisplay.DisplayByPeriod,
                    paymentDisplay, depositTypes, terms, pcfAccountPeriods, financialPeriods);
                var desc = processor.BuildPreviousBalanceDescription(financialPeriods[1].Start);
                Assert.AreEqual(string.Format("Charges before {0}", financialPeriods[1].Start.ToShortDateString()), desc);
            }
        }

        [TestClass]
        public class StudentStatementProcessor_BuildFutureBalanceDescription : StudentStatementProcessorTests
        {
            [TestMethod]
            public void StudentStatementProcessor_BuildFutureBalanceDescription_ActivityDisplayByTerm_MatchingTerm()
            {
                var desc = processor.BuildFutureBalanceDescription(null);
                Assert.AreEqual("Charges after 2015 Spring Term", desc);
            }

            [TestMethod]
            public void StudentStatementProcessor_BuildFutureBalanceDescription_ActivityDisplayByTerm_NoMatchingTerm()
            {
                processor = new StudentStatementProcessor("ABC123", activityDisplay, paymentDisplay, depositTypes,
                    terms, termAccountPeriods, financialPeriods);
                var desc = processor.BuildFutureBalanceDescription(null);
                Assert.AreEqual("Charges after ", desc);
            }

            [TestMethod]
            public void StudentStatementProcessor_BuildFutureBalanceDescription_ActivityDisplayByPeriod()
            {
                processor = new StudentStatementProcessor(FinanceTimeframeCodes.CurrentPeriod, ActivityDisplay.DisplayByPeriod,
                    paymentDisplay, depositTypes, terms, pcfAccountPeriods, financialPeriods);
                var desc = processor.BuildFutureBalanceDescription(financialPeriods[1].End);
                Assert.AreEqual(string.Format("Charges after {0}", financialPeriods[1].End.ToShortDateString()), desc);
            }
        }

        [TestClass]
        public class StudentStatementProcessor_GetSummaryHeaderDateRange : StudentStatementProcessorTests
        {
            [TestMethod]
            public void StudentStatementProcessor_GetSummaryHeaderDateRange_NullDates()
            {
                var range = processor.GetSummaryHeaderDateRange(null, null);
                Assert.AreEqual(string.Empty, range);
            }

            [TestMethod]
            public void StudentStatementProcessor_GetSummaryHeaderDateRange_NullStartDate()
            {
                processor = new StudentStatementProcessor(FinanceTimeframeCodes.PastPeriod, ActivityDisplay.DisplayByPeriod, paymentDisplay, depositTypes, terms,
                    termAccountPeriods, financialPeriods);
                var range = processor.GetSummaryHeaderDateRange(null, DateTime.Parse("12/22/2014"));
                Assert.AreEqual(" (before 12/23/2014)", range);
            }

            [TestMethod]
            public void StudentStatementProcessor_GetSummaryHeaderDateRange_NullEndDate()
            {
                processor = new StudentStatementProcessor(FinanceTimeframeCodes.FuturePeriod, ActivityDisplay.DisplayByPeriod, paymentDisplay, depositTypes, terms,
                    termAccountPeriods, financialPeriods);
                var range = processor.GetSummaryHeaderDateRange(DateTime.Parse("12/22/2014"), null);
                Assert.AreEqual(" (after 12/21/2014)", range);
            }

            [TestMethod]
            public void StudentStatementProcessor_GetSummaryHeaderDateRange_StartAndEndDates()
            {
                processor = new StudentStatementProcessor(FinanceTimeframeCodes.CurrentPeriod, ActivityDisplay.DisplayByPeriod, paymentDisplay, depositTypes, terms,
                    termAccountPeriods, financialPeriods);
                var range = processor.GetSummaryHeaderDateRange(DateTime.Parse("12/22/2014"), DateTime.Parse("12/25/2014"));
                Assert.AreEqual(" (12/22/2014 to 12/25/2014)", range);
            }

            [TestMethod]
            public void StudentStatementProcessor_GetSummaryHeaderDateRange_EmptyStringInTermMode()
            {
                var range = processor.GetSummaryHeaderDateRange(DateTime.Parse("12/22/2014"), DateTime.Parse("12/25/2014"));
                Assert.AreEqual(string.Empty, range);
            }
        }

        [TestClass]
        public class StudentStatementProcessor_SortAndConsolidateAccountDetails : StudentStatementProcessorTests
        {
            [TestMethod]
            public void StudentStatementProcessor_SortAndConsolidateAccountDetails_NullDetails()
            {
                processor.SortAndConsolidateAccountDetails(null);
            }

            [TestMethod]
            public void StudentStatementProcessor_SortAndConsolidateAccountDetails_NullCharges()
            {
                processor.SortAndConsolidateAccountDetails(new DetailedAccountPeriod() { Charges = null });
            }

            [TestMethod]
            public void StudentStatementProcessor_SortAndConsolidateAccountDetails_NullTuitionBySectionCharges()
            {
                processor.SortAndConsolidateAccountDetails(new DetailedAccountPeriod() { Charges = new ChargesCategory() { TuitionBySectionGroups = null } });
            }

            [TestMethod]
            public void StudentStatementProcessor_SortAndConsolidateAccountDetails_NoTuitionBySectionCharges()
            {
                processor.SortAndConsolidateAccountDetails(new DetailedAccountPeriod() { Charges = new ChargesCategory() { TuitionBySectionGroups = new List<TuitionBySectionType>() } });
            }

            [TestMethod]
            public void StudentStatementProcessor_SortAndConsolidateAccountDetails_TuitionBySectionCharges_OneGroup()
            {
                DetailedAccountPeriod details = new DetailedAccountPeriod()
                {
                    Charges = new ChargesCategory()
                    {
                        TuitionBySectionGroups = new List<TuitionBySectionType>()
                        {
                            new TuitionBySectionType()
                            {
                                DisplayOrder = 1,
                                Name = "Tuition by Section",
                                SectionCharges = new List<ActivityTuitionItem>()
                                {
                                    new ActivityTuitionItem()
                                    {
                                        Amount = 500m,
                                        BillingCredits = 3m,
                                        Ceus = null,
                                        Classroom = "Room",
                                        Credits = 3m,
                                        Days = new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday },
                                        Description = "Desc",
                                        EndTime = "3:00 PM",
                                        Id = "123",
                                        Instructor = "Professor",
                                        StartTime = "1:00 PM",
                                        Status = "A",
                                        TermId = "2014/FA"
                                    },
                                    new ActivityTuitionItem()
                                    {
                                        Amount = 500m,
                                        BillingCredits = null,
                                        Ceus = 1.5m,
                                        Classroom = "Room2",
                                        Credits = null,
                                        Days = new List<DayOfWeek>() { DayOfWeek.Tuesday, DayOfWeek.Thursday },
                                        Description = "Desc2",
                                        EndTime = "5:00 PM",
                                        Id = "124",
                                        Instructor = "Professor2",
                                        StartTime = "3:00 PM",
                                        Status = "A",
                                        TermId = "2014/FA"
                                    },
                                    new ActivityTuitionItem()
                                    {
                                        Id = "Other Tuition Activity",
                                        Amount = 500m,
                                        TermId = "2014/FA"
                                    }
                                }
                            }
                        }
                    }
                };
                processor.SortAndConsolidateAccountDetails(details);
                Assert.AreEqual(1, details.Charges.TuitionBySectionGroups.Count);
            }

            [TestMethod]
            public void StudentStatementProcessor_SortAndConsolidateAccountDetails_TuitionBySectionCharges_MultipleGroups()
            {
                DetailedAccountPeriod details = new DetailedAccountPeriod()
                {
                    Charges = new ChargesCategory()
                    {
                        TuitionBySectionGroups = new List<TuitionBySectionType>()
                        {
                            new TuitionBySectionType()
                            {
                                DisplayOrder = 1,
                                Name = "Tuition by Section",
                                SectionCharges = new List<ActivityTuitionItem>()
                                {
                                    new ActivityTuitionItem()
                                    {
                                        Amount = 500m,
                                        BillingCredits = 3m,
                                        Ceus = null,
                                        Classroom = "Room",
                                        Credits = 3m,
                                        Days = new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday },
                                        Description = "Desc",
                                        EndTime = "3:00 PM",
                                        Id = "123",
                                        Instructor = "Professor",
                                        StartTime = "1:00 PM",
                                        Status = "A",
                                        TermId = "2014/FA"
                                    },
                                    new ActivityTuitionItem()
                                    {
                                        Amount = 500m,
                                        BillingCredits = null,
                                        Ceus = 1.5m,
                                        Classroom = "Room2",
                                        Credits = null,
                                        Days = new List<DayOfWeek>() { DayOfWeek.Tuesday, DayOfWeek.Thursday },
                                        Description = "Desc2",
                                        EndTime = "5:00 PM",
                                        Id = "124",
                                        Instructor = "Professor2",
                                        StartTime = "3:00 PM",
                                        Status = "A",
                                        TermId = "2014/FA"
                                    }
                                }
                            },
                            new TuitionBySectionType()
                            {
                                DisplayOrder = 2,
                                Name = "Tuition by Section3",
                                SectionCharges = new List<ActivityTuitionItem>()
                                {
                                    new ActivityTuitionItem()
                                    {
                                        Amount = 500m,
                                        BillingCredits = 3m,
                                        Ceus = null,
                                        Classroom = "Room3",
                                        Credits = 3m,
                                        Days = new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday },
                                        Description = "Desc3",
                                        EndTime = "3:00 PM",
                                        Id = "125",
                                        Instructor = "Professor3",
                                        StartTime = "1:00 PM",
                                        Status = "A",
                                        TermId = "2014/SP"
                                    },
                                    new ActivityTuitionItem()
                                    {
                                        Amount = 500m,
                                        BillingCredits = null,
                                        Ceus = 1.5m,
                                        Classroom = "Room4",
                                        Credits = null,
                                        Days = new List<DayOfWeek>() { DayOfWeek.Tuesday, DayOfWeek.Thursday },
                                        Description = "Desc4",
                                        EndTime = "5:00 PM",
                                        Id = "126",
                                        Instructor = "Professor4",
                                        StartTime = "3:00 PM",
                                        Status = "A",
                                        TermId = "2014/SP"
                                    }
                                }
                            }
                        }
                    }
                };
                processor.SortAndConsolidateAccountDetails(details);
                Assert.AreEqual(1, details.Charges.TuitionBySectionGroups.Count);
                Assert.AreEqual("125", details.Charges.TuitionBySectionGroups[0].SectionCharges[0].Id);
                Assert.AreEqual("126", details.Charges.TuitionBySectionGroups[0].SectionCharges[1].Id);
                Assert.AreEqual("123", details.Charges.TuitionBySectionGroups[0].SectionCharges[2].Id);
                Assert.AreEqual("124", details.Charges.TuitionBySectionGroups[0].SectionCharges[3].Id);
            }

            [TestMethod]
            public void StudentStatementProcessor_SortAndConsolidateAccountDetails_NullTuitionByTotalCharges()
            {
                processor.SortAndConsolidateAccountDetails(new DetailedAccountPeriod() { Charges = new ChargesCategory() { TuitionByTotalGroups = null } });
            }

            [TestMethod]
            public void StudentStatementProcessor_SortAndConsolidateAccountDetails_NoTuitionByTotalCharges()
            {
                processor.SortAndConsolidateAccountDetails(new DetailedAccountPeriod() { Charges = new ChargesCategory() { TuitionByTotalGroups = new List<TuitionByTotalType>() } });
            }

            [TestMethod]
            public void StudentStatementProcessor_SortAndConsolidateAccountDetails_TuitionByTotalCharges_OneGroup()
            {
                DetailedAccountPeriod details = new DetailedAccountPeriod()
                {
                    Charges = new ChargesCategory()
                    {
                        TuitionByTotalGroups = new List<TuitionByTotalType>()
                        {
                            new TuitionByTotalType()
                            {
                                DisplayOrder = 1,
                                Name = "Tuition by Total",
                                TotalCharges = new List<ActivityTuitionItem>()
                                {
                                    new ActivityTuitionItem()
                                    {
                                        Amount = 500m,
                                        BillingCredits = 3m,
                                        Ceus = null,
                                        Classroom = "Room",
                                        Credits = 3m,
                                        Days = new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday },
                                        Description = "Desc",
                                        EndTime = "3:00 PM",
                                        Id = "123",
                                        Instructor = "Professor",
                                        StartTime = "1:00 PM",
                                        Status = "A",
                                        TermId = "2014/FA"
                                    },
                                    new ActivityTuitionItem()
                                    {
                                        Amount = 500m,
                                        BillingCredits = null,
                                        Ceus = 1.5m,
                                        Classroom = "Room2",
                                        Credits = null,
                                        Days = new List<DayOfWeek>() { DayOfWeek.Tuesday, DayOfWeek.Thursday },
                                        Description = "Desc2",
                                        EndTime = "5:00 PM",
                                        Id = "124",
                                        Instructor = "Professor2",
                                        StartTime = "3:00 PM",
                                        Status = "A",
                                        TermId = "2014/FA"
                                    },
                                    new ActivityTuitionItem()
                                    {
                                        Id = "Other Tuition Activity",
                                        Amount = 500m,
                                        TermId = "2014/FA"
                                    }
                                }
                            }
                        }
                    }
                };
                processor.SortAndConsolidateAccountDetails(details);
                Assert.AreEqual(1, details.Charges.TuitionByTotalGroups.Count);
            }

            [TestMethod]
            public void StudentStatementProcessor_SortAndConsolidateAccountDetails_TuitionByTotalCharges_MultipleGroups()
            {
                DetailedAccountPeriod details = new DetailedAccountPeriod()
                {
                    Charges = new ChargesCategory()
                    {
                        TuitionByTotalGroups = new List<TuitionByTotalType>()
                        {
                            new TuitionByTotalType()
                            {
                                DisplayOrder = 1,
                                Name = "Tuition by Section",
                                TotalCharges = new List<ActivityTuitionItem>()
                                {
                                    new ActivityTuitionItem()
                                    {
                                        Amount = 500m,
                                        BillingCredits = 3m,
                                        Ceus = null,
                                        Classroom = "Room",
                                        Credits = 3m,
                                        Days = new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday },
                                        Description = "Desc",
                                        EndTime = "3:00 PM",
                                        Id = "123",
                                        Instructor = "Professor",
                                        StartTime = "1:00 PM",
                                        Status = "A",
                                        TermId = "2014/FA"
                                    },
                                    new ActivityTuitionItem()
                                    {
                                        Amount = 500m,
                                        BillingCredits = null,
                                        Ceus = 1.5m,
                                        Classroom = "Room2",
                                        Credits = null,
                                        Days = new List<DayOfWeek>() { DayOfWeek.Tuesday, DayOfWeek.Thursday },
                                        Description = "Desc2",
                                        EndTime = "5:00 PM",
                                        Id = "124",
                                        Instructor = "Professor2",
                                        StartTime = "3:00 PM",
                                        Status = "A",
                                        TermId = "2014/FA"
                                    }
                                }
                            },
                            new TuitionByTotalType()
                            {
                                DisplayOrder = 2,
                                Name = "Tuition by Section3",
                                TotalCharges = new List<ActivityTuitionItem>()
                                {
                                    new ActivityTuitionItem()
                                    {
                                        Amount = 500m,
                                        BillingCredits = 3m,
                                        Ceus = null,
                                        Classroom = "Room3",
                                        Credits = 3m,
                                        Days = new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday },
                                        Description = "Desc3",
                                        EndTime = "3:00 PM",
                                        Id = "125",
                                        Instructor = "Professor3",
                                        StartTime = "1:00 PM",
                                        Status = "A",
                                        TermId = "2014/SP"
                                    },
                                    new ActivityTuitionItem()
                                    {
                                        Amount = 500m,
                                        BillingCredits = null,
                                        Ceus = 1.5m,
                                        Classroom = "Room4",
                                        Credits = null,
                                        Days = new List<DayOfWeek>() { DayOfWeek.Tuesday, DayOfWeek.Thursday },
                                        Description = "Desc4",
                                        EndTime = "5:00 PM",
                                        Id = "126",
                                        Instructor = "Professor4",
                                        StartTime = "3:00 PM",
                                        Status = "A",
                                        TermId = "2014/SP"
                                    }
                                }
                            }
                        }
                    }
                };
                processor.SortAndConsolidateAccountDetails(details);
                Assert.AreEqual(1, details.Charges.TuitionByTotalGroups.Count);
                Assert.AreEqual("125", details.Charges.TuitionByTotalGroups[0].TotalCharges[0].Id);
                Assert.AreEqual("126", details.Charges.TuitionByTotalGroups[0].TotalCharges[1].Id);
                Assert.AreEqual("123", details.Charges.TuitionByTotalGroups[0].TotalCharges[2].Id);
                Assert.AreEqual("124", details.Charges.TuitionByTotalGroups[0].TotalCharges[3].Id);
            }
        }

        [TestClass]
        public class StudentStatementProcessor_CalculatePaymentPlanAdjustments : StudentStatementProcessorTests
        {
            [TestMethod]
            public void StudentStatementProcessor_CalculatePaymentPlanAdjustments_NullAccountTerms()
            {
                var adj = processor.CalculatePaymentPlanAdjustments(null);
                Assert.AreEqual(0, adj);
            }

            [TestMethod]
            public void StudentStatementProcessor_CalculatePaymentPlanAdjustments_NoAccountTerms()
            {
                var adj = processor.CalculatePaymentPlanAdjustments(new List<AccountTerm>());
                Assert.AreEqual(0, adj);
            }

            [TestMethod]
            public void StudentStatementProcessor_CalculatePaymentPlanAdjustments_Term_Valid()
            {
                var adj = processor.CalculatePaymentPlanAdjustments(new List<AccountTerm>()
                    {
                        new AccountTerm() { Amount = 1500m, Description = "2015 Spring Term", TermId = "2015/SP", AccountDetails = new List<AccountsReceivableDueItem>()
                        {
                            new PaymentPlanDueItem() { AccountDescription = "Plan", AccountType = "01", AmountDue = 500m, Description = "Plan Pay #1",
                                Distribution = "BANK", DueDate = DateTime.Today.AddDays(-14), Overdue = false, PaymentPlanCurrent = false, PaymentPlanId = "123",
                                Term = "2015/SP", TermDescription = "2015 Spring Term", UnpaidAmount = 0m },
                            new PaymentPlanDueItem() { AccountDescription = "Plan", AccountType = "01", AmountDue = 500m, Description = "Plan Pay #1",
                                Distribution = "BANK", DueDate = DateTime.Today.AddDays(-7), Overdue = true, PaymentPlanCurrent = false, PaymentPlanId = "123",
                                Term = "2015/SP", TermDescription = "2015 Spring Term", UnpaidAmount = 250m },
                            new PaymentPlanDueItem() { AccountDescription = "Plan", AccountType = "01", AmountDue = 500m, Description = "Plan Pay #1",
                                Distribution = "BANK", DueDate = DateTime.Today, Overdue = false, PaymentPlanCurrent = false, PaymentPlanId = "123",
                                Term = "2015/SP", TermDescription = "2015 Spring Term", UnpaidAmount = 500m },
                            new PaymentPlanDueItem() { AccountDescription = "Plan", AccountType = "01", AmountDue = 500m, Description = "Plan Pay #1",
                                Distribution = "BANK", DueDate = DateTime.Today.AddDays(7), Overdue = false, PaymentPlanCurrent = false, PaymentPlanId = "123",
                                Term = "2015/SP", TermDescription = "2015 Spring Term", UnpaidAmount = 500m },

                        }}
                    });
                Assert.AreEqual(1000m, adj);
            }

            [TestMethod]
            public void StudentStatementProcessor_CalculatePaymentPlanAdjustments_Term_Valid_NoAccountDetails()
            {
                var adj = processor.CalculatePaymentPlanAdjustments(new List<AccountTerm>()
                    {
                        new AccountTerm() 
                        { 
                            Amount = 1500m, 
                            Description = "2015 Spring Term", 
                            TermId = "2015/SP", 
                            AccountDetails = new List<AccountsReceivableDueItem>()
                        }
                    });
                Assert.AreEqual(0, adj);
            }

            [TestMethod]
            public void StudentStatementProcessor_CalculatePaymentPlanAdjustments_Term_Valid_NullAccountDetails()
            {
                var adj = processor.CalculatePaymentPlanAdjustments(new List<AccountTerm>()
                    {
                        new AccountTerm() 
                        { 
                            Amount = 1500m, 
                            Description = "2015 Spring Term", 
                            TermId = "2015/SP", 
                            AccountDetails = null
                        }
                    });
                Assert.AreEqual(0, adj);
            }
        }

        [TestClass]
        public class StudentStatementProcessor_CalculateCurrentDepositsDue : StudentStatementProcessorTests
        {
            [TestMethod]
            public void StudentStatementProcessor_CalculateCurrentDepositsDue_NullAccountTerms()
            {
                var cdd = processor.CalculateCurrentDepositsDue(null);
                Assert.AreEqual(0, cdd);
            }

            [TestMethod]
            public void StudentStatementProcessor_CalculateCurrentDepositsDue_NoAccountTerms()
            {
                var cdd = processor.CalculateCurrentDepositsDue(new List<DepositDue>());
                Assert.AreEqual(0, cdd);
            }
        }

        [TestClass]
        public class StudentStatementProcessor_UpdateRefundDatesAndReferenceNumbers : StudentStatementProcessorTests
        {
            [TestMethod]
            public void StudentStatementProcessor_UpdateRefundDatesAndReferenceNumbers_NullRefunds()
            {
                IEnumerable<ActivityPaymentMethodItem> refunds = null;
                processor.UpdateRefundDatesAndReferenceNumbers(refunds);
                Assert.IsNull(refunds);
            }

            [TestMethod]
            public void StudentStatementProcessor_UpdateRefundDatesAndReferenceNumbers_Outstanding()
            {
                List<ActivityPaymentMethodItem> refunds = new List<ActivityPaymentMethodItem>()
                    {
                        new ActivityPaymentMethodItem()
                        {
                            Status = RefundVoucherStatus.Outstanding
                        }
                    };
                processor.UpdateRefundDatesAndReferenceNumbers(refunds);
                Assert.AreEqual("In Progress", refunds[0].CheckNumber);
            }

            [TestMethod]
            public void StudentStatementProcessor_UpdateRefundDatesAndReferenceNumbers_NotApproved()
            {
                List<ActivityPaymentMethodItem> refunds = new List<ActivityPaymentMethodItem>()
                    {
                        new ActivityPaymentMethodItem()
                        {
                            Status = RefundVoucherStatus.NotApproved
                        }
                    };
                processor.UpdateRefundDatesAndReferenceNumbers(refunds);
                Assert.AreEqual("In Progress", refunds[0].CheckNumber);
            }

            [TestMethod]
            public void StudentStatementProcessor_UpdateRefundDatesAndReferenceNumbers_Paid()
            {
                List<ActivityPaymentMethodItem> refunds = new List<ActivityPaymentMethodItem>()
                    {
                        new ActivityPaymentMethodItem()
                        {
                            Status = RefundVoucherStatus.Paid,
                            CreditCardLastFourDigits = "1234"
                        }
                    };
                processor.UpdateRefundDatesAndReferenceNumbers(refunds);
                Assert.AreEqual("1234", refunds[0].CheckNumber);
            }

            [TestMethod]
            public void StudentStatementProcessor_UpdateRefundDatesAndReferenceNumbers_Reconciled()
            {
                List<ActivityPaymentMethodItem> refunds = new List<ActivityPaymentMethodItem>()
                    {
                        new ActivityPaymentMethodItem()
                        {
                            Status = RefundVoucherStatus.Reconciled,
                            CheckNumber = "4567",
                            CheckDate = DateTime.Today.AddDays(-3)
                        }
                    };
                processor.UpdateRefundDatesAndReferenceNumbers(refunds);
                Assert.AreEqual("4567", refunds[0].CheckNumber);
                Assert.AreEqual(DateTime.Today.AddDays(-3), refunds[0].StatusDate);
            }

            [TestMethod]
            public void StudentStatementProcessor_UpdateRefundDatesAndReferenceNumbers_Other()
            {
                List<ActivityPaymentMethodItem> refunds = new List<ActivityPaymentMethodItem>()
                    {
                        new ActivityPaymentMethodItem()
                        {
                            Status = RefundVoucherStatus.Cancelled
                        }
                    };
                processor.UpdateRefundDatesAndReferenceNumbers(refunds);
                Assert.AreEqual(string.Empty, refunds[0].CheckNumber);
            }
        }

        [TestClass]
        public class StudentStatementProcessor_CalculateOverdueAmount : StudentStatementProcessorTests
        {
            [TestMethod]
            public void StudentStatementProcessor_CalculateOverdueAmount_NullAccountTermsAndDepositsDue()
            {
                var oa = processor.CalculateOverdueAmount(null, null);
                Assert.AreEqual(0, oa);
            }

            [TestMethod]
            public void StudentStatementProcessor_CalculateOverdueAmount_NullAccountTermsAndNoDepositsDue()
            {
                var oa = processor.CalculateOverdueAmount(null, new List<DepositDue>());
                Assert.AreEqual(0, oa);
            }

            [TestMethod]
            public void StudentStatementProcessor_CalculateOverdueAmount_NoAccountTermsOrDepositsDue()
            {
                var oa = processor.CalculateOverdueAmount(new List<AccountTerm>(), new List<DepositDue>());
                Assert.AreEqual(0, oa);
            }

            [TestMethod]
            public void StudentStatementProcessor_CalculateOverdueAmount_NoAccountTermsAndNullDepositsDue()
            {
                var oa = processor.CalculateOverdueAmount(new List<AccountTerm>(), null);
                Assert.AreEqual(0, oa);
            }

            [TestMethod]
            public void StudentStatementProcessor_CalculateOverdueAmount_Valid_AccountTerms_NoDepositsDue()
            {
                var oa = processor.CalculateOverdueAmount(new List<AccountTerm>() 
                { 
                    new AccountTerm() 
                    { 
                        AccountDetails = new List<AccountsReceivableDueItem>()
                        {
                            // This item is overdue and should be included
                            new AccountsReceivableDueItem() { DueDate = DateTime.Today.AddDays(-3), AmountDue = 500m },
                            // This item is overdue and should be included, but has no amount due
                            new AccountsReceivableDueItem() { DueDate = DateTime.Today.AddDays(-3), AmountDue = null },
                            // This item is has no due date and should not be included
                            new AccountsReceivableDueItem() { DueDate = null, AmountDue = -500m },
                            // This item is due in the future and should not be included
                            new AccountsReceivableDueItem() { DueDate = DateTime.Today.AddDays(3), AmountDue = 500m },
                        }
                    }
                }, null);
                Assert.AreEqual(500m, oa);
            }

            [TestMethod]
            public void StudentStatementProcessor_CalculateOverdueAmount_Valid_DepositsDue_NoAccountTerms()
            {
                var oa = processor.CalculateOverdueAmount(null, depositsDue);
                Assert.AreEqual(24000m, oa);
            }
        }

        [TestClass]
        public class StudentStatementProcessor_CalculateFutureOverdueAmounts : StudentStatementProcessorTests
        {
            [TestMethod]
            public void StudentStatementProcessor_CalculateFutureOverdueAmounts_NullAccountTerms()
            {
                var foa = processor.CalculateFutureOverdueAmounts(null);
                Assert.AreEqual(0, foa);
            }

            [TestMethod]
            public void StudentStatementProcessor_CalculateFutureOverdueAmounts_NoAccountTerms()
            {
                var foa = processor.CalculateFutureOverdueAmounts(new List<AccountTerm>());
                Assert.AreEqual(0, foa);
            }

            [TestMethod]
            public void StudentStatementProcessor_CalculateFutureOverdueAmounts_Term_FutureTerms()
            {
                var foa = processor.CalculateFutureOverdueAmounts(new List<AccountTerm>() 
                { 
                    new AccountTerm() 
                    { 
                        AccountDetails = new List<AccountsReceivableDueItem>()
                        {
                            // This item is overdue and should be included
                            new AccountsReceivableDueItem() { DueDate = DateTime.Today.AddDays(-3), AmountDue = 500m },
                            // This item is overdue and should be included, but has no amount due
                            new AccountsReceivableDueItem() { DueDate = DateTime.Today.AddDays(-3), AmountDue = null },
                            // This item is has no due date and should not be included
                            new AccountsReceivableDueItem() { DueDate = null, AmountDue = -500m },
                            // This item is due in the future and should not be included
                            new AccountsReceivableDueItem() { DueDate = DateTime.Today.AddDays(3), AmountDue = 500m },
                        },
                        TermId = "2015/FA"
                    }
                });
                Assert.AreEqual(500m, foa);
            }

            [TestMethod]
            public void StudentStatementProcessor_CalculateFutureOverdueAmounts_Term_NoFutureTerms()
            {
                var foa = processor.CalculateFutureOverdueAmounts(new List<AccountTerm>() 
                { 
                    new AccountTerm() 
                    { 
                        AccountDetails = new List<AccountsReceivableDueItem>()
                        {
                            // This item is overdue and should be included
                            new AccountsReceivableDueItem() { DueDate = DateTime.Today.AddDays(-3), AmountDue = 500m },
                            // This item is overdue and should be included, but has no amount due
                            new AccountsReceivableDueItem() { DueDate = DateTime.Today.AddDays(-3), AmountDue = null },
                            // This item is has no due date and should not be included
                            new AccountsReceivableDueItem() { DueDate = null, AmountDue = -500m },
                            // This item is due in the future and should not be included
                            new AccountsReceivableDueItem() { DueDate = DateTime.Today.AddDays(3), AmountDue = 500m },
                        },
                        TermId = "2014/FA"
                    }
                });
                Assert.AreEqual(0m, foa);
            }

            [Ignore]
            [TestMethod]
            public void StudentStatementProcessor_CalculateFutureOverdueAmounts_Pcf_Past()
            {
                processor = new StudentStatementProcessor("PAST", ActivityDisplay.DisplayByPeriod, PaymentDisplay.DisplayByPeriod, depositTypes, terms,
                    termAccountPeriods, financialPeriods);

                var foa = processor.CalculateFutureOverdueAmounts(new List<AccountTerm>() 
                { 
                    new AccountTerm() 
                    { 
                        AccountDetails = new List<AccountsReceivableDueItem>()
                        {
                            // This item is overdue and should be included
                            new AccountsReceivableDueItem() { DueDate = DateTime.Today.AddDays(-3), AmountDue = 500m },
                            // This item is overdue and should be included, but has no amount due
                            new AccountsReceivableDueItem() { DueDate = DateTime.Today.AddDays(-3), AmountDue = null },
                            // This item is has no due date and should not be included
                            new AccountsReceivableDueItem() { DueDate = null, AmountDue = -500m },
                            // This item is due in the future and should not be included
                            new AccountsReceivableDueItem() { DueDate = DateTime.Today.AddDays(3), AmountDue = 500m },
                        },
                        TermId = "2015/FA"
                    }
                });
                Assert.AreEqual(0m, foa);
            }

            [TestMethod]
            public void StudentStatementProcessor_CalculateFutureOverdueAmounts_Pcf_Current()
            {
                processor = new StudentStatementProcessor("CUR", ActivityDisplay.DisplayByPeriod, PaymentDisplay.DisplayByPeriod, depositTypes, terms,
                    termAccountPeriods, financialPeriods);

                var foa = processor.CalculateFutureOverdueAmounts(new List<AccountTerm>() 
                { 
                    new AccountTerm() 
                    { 
                        AccountDetails = new List<AccountsReceivableDueItem>()
                        {
                            // This item is overdue and should be included
                            new AccountsReceivableDueItem() { DueDate = DateTime.Today.AddDays(-3), AmountDue = 500m },
                            // This item is overdue and should be included, but has no amount due
                            new AccountsReceivableDueItem() { DueDate = DateTime.Today.AddDays(-3), AmountDue = null },
                            // This item is has no due date and should not be included
                            new AccountsReceivableDueItem() { DueDate = null, AmountDue = -500m },
                            // This item is due in the future and should not be included
                            new AccountsReceivableDueItem() { DueDate = DateTime.Today.AddDays(3), AmountDue = 500m },
                        },
                        TermId = "2014/FA"
                    }
                });
                Assert.AreEqual(0m, foa);
            }

            [TestMethod]
            public void StudentStatementProcessor_CalculateFutureOverdueAmounts_Pcf_Future()
            {
                processor = new StudentStatementProcessor("FTR", ActivityDisplay.DisplayByPeriod, PaymentDisplay.DisplayByPeriod, depositTypes, terms,
                    termAccountPeriods, financialPeriods);

                var foa = processor.CalculateFutureOverdueAmounts(new List<AccountTerm>() 
                { 
                    new AccountTerm() 
                    { 
                        AccountDetails = new List<AccountsReceivableDueItem>()
                        {
                            // This item is overdue and should be included
                            new AccountsReceivableDueItem() { DueDate = DateTime.Today.AddDays(-3), AmountDue = 500m },
                            // This item is overdue and should be included, but has no amount due
                            new AccountsReceivableDueItem() { DueDate = DateTime.Today.AddDays(-3), AmountDue = null },
                            // This item is has no due date and should not be included
                            new AccountsReceivableDueItem() { DueDate = null, AmountDue = -500m },
                            // This item is due in the future and should not be included
                            new AccountsReceivableDueItem() { DueDate = DateTime.Today.AddDays(3), AmountDue = 500m },
                        },
                        TermId = "2014/FA"
                    }
                });
                Assert.AreEqual(0m, foa);
            }

        }

    }
}
