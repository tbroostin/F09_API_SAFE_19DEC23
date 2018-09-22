// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Finance.Entities;
using Ellucian.Colleague.Domain.Finance.Entities.AccountDue;
using Ellucian.Colleague.Domain.Finance.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Finance.Tests.Services
{
    [TestClass]
    public class DueDateOverrideProcessorTests
    {
        DueDateOverrides dueDateOverrides;
        AccountDue accountDue;
        AccountDuePeriod accountDuePeriod;

        [TestInitialize]
        public void Initialize()
        {
            dueDateOverrides = new DueDateOverrides()
            {
                CurrentPeriodOverride = DateTime.Today.AddDays(30),
                FuturePeriodOverride = DateTime.Today.AddDays(90),
                NonTermOverride = DateTime.Today.AddDays(10),
                PastPeriodOverride = DateTime.Today.AddDays(-90),
                TermOverrides = new Dictionary<string, DateTime>()
                {
                    { "Term", DateTime.Today.AddDays(3) },
                }
            };
            accountDuePeriod = TestAccountDuePeriodRepository.AccountDuePeriod("0003315");
            accountDue = accountDuePeriod.Current;
        }

        [TestCleanup]
        public void Cleanup()
        {
            dueDateOverrides = null;
            accountDuePeriod = null;
            accountDue = null;
        }

        [TestClass]
        public class DueDateOverrideProcessor_OverrideTermDueDates : DueDateOverrideProcessorTests
        {
            [TestInitialize]
            public void DueDateOverrideProcessor_OverrideTermDueDates_Initialize()
            {
                base.Initialize();
            }

            [TestCleanup]
            public void DueDateOverrideProcessor_OverrideTermDueDates_Cleanup()
            {
                base.Cleanup();
            }

            [TestMethod]
            public void DueDateOverrideProcessor_OverrideTermDueDates_NullDueDateOverrides()
            {
                DueDateOverrideProcessor.OverrideTermDueDates(null, accountDue);
            }

            [TestMethod]
            public void DueDateOverrideProcessor_OverrideTermDueDates_NullAccountDueAccountTermAccountDetails()
            {
                var ad = accountDuePeriod.Current;
                ad.AccountTerms[0].AccountDetails = null;
                DueDateOverrideProcessor.OverrideTermDueDates(dueDateOverrides, ad);
            }

            [TestMethod]
            public void DueDateOverrideProcessor_OverrideTermDueDates_NullTermOverridesAndNonTermOverride()
            {
                AccountDue preOverride = accountDue;
                DueDateOverrideProcessor.OverrideTermDueDates(new DueDateOverrides() { CurrentPeriodOverride = DateTime.Today.AddDays(30) }, accountDue);
                for(int i = 0; i < accountDue.AccountTerms.Count; i++)
                {
                    for(int j = 0; j < accountDue.AccountTerms[i].AccountDetails.Count; j++)
                    {
                        Assert.AreEqual(preOverride.AccountTerms[i].AccountDetails[j].DueDate, accountDue.AccountTerms[i].AccountDetails[j].DueDate);
                    }
                }
            }

            [TestMethod]
            public void DueDateOverrideProcessor_OverrideTermDueDates_NonTermOverride()
            {
                AccountDue preOverride = accountDue;
                DueDateOverrides tempOverride = new DueDateOverrides() { NonTermOverride = DateTime.Today.AddDays(10) };
                DueDateOverrideProcessor.OverrideTermDueDates(tempOverride, accountDue);

                Assert.AreEqual(preOverride.AccountTerms[0].AccountDetails[0].DueDate, accountDue.AccountTerms[0].AccountDetails[0].DueDate);
                Assert.AreEqual(tempOverride.NonTermOverride, accountDue.AccountTerms[0].AccountDetails[1].DueDate);
                Assert.IsFalse(accountDue.AccountTerms[0].AccountDetails[1].Overdue);
                Assert.AreEqual(preOverride.AccountTerms[0].AccountDetails[2].DueDate, accountDue.AccountTerms[0].AccountDetails[2].DueDate);
                Assert.AreEqual(preOverride.AccountTerms[0].AccountDetails[3].DueDate, accountDue.AccountTerms[0].AccountDetails[3].DueDate);
                Assert.AreEqual(preOverride.AccountTerms[0].AccountDetails[4].DueDate, accountDue.AccountTerms[0].AccountDetails[4].DueDate);
                Assert.AreEqual(preOverride.AccountTerms[0].AccountDetails[5].DueDate, accountDue.AccountTerms[0].AccountDetails[5].DueDate);
                Assert.IsFalse(accountDue.AccountTerms[0].AccountDetails[5].Overdue);
                Assert.AreEqual(tempOverride.NonTermOverride, accountDue.AccountTerms[0].AccountDetails[6].DueDate);
                Assert.IsFalse(accountDue.AccountTerms[0].AccountDetails[6].Overdue);
            }

            [TestMethod]
            public void DueDateOverrideProcessor_OverrideTermDueDates_TermOverrides_MatchingTermOverride()
            {
                AccountDue preOverride = accountDue;
                DueDateOverrides tempOverride = new DueDateOverrides() { TermOverrides = new Dictionary<string,DateTime>() { { "Term", DateTime.Today.AddDays(3) } } };
                DueDateOverrideProcessor.OverrideTermDueDates(tempOverride, accountDue);

                Assert.AreEqual(tempOverride.TermOverrides["Term"], accountDue.AccountTerms[0].AccountDetails[0].DueDate);
                Assert.AreEqual(preOverride.AccountTerms[0].AccountDetails[1].DueDate, accountDue.AccountTerms[0].AccountDetails[1].DueDate);
                Assert.AreEqual(preOverride.AccountTerms[0].AccountDetails[2].DueDate, accountDue.AccountTerms[0].AccountDetails[2].DueDate);
                Assert.AreEqual(preOverride.AccountTerms[0].AccountDetails[3].DueDate, accountDue.AccountTerms[0].AccountDetails[3].DueDate);
                Assert.AreEqual(preOverride.AccountTerms[0].AccountDetails[4].DueDate, accountDue.AccountTerms[0].AccountDetails[4].DueDate);
                Assert.AreEqual(preOverride.AccountTerms[0].AccountDetails[5].DueDate, accountDue.AccountTerms[0].AccountDetails[5].DueDate);
                Assert.IsFalse(accountDue.AccountTerms[0].AccountDetails[5].Overdue);
                Assert.AreEqual(preOverride.AccountTerms[0].AccountDetails[6].DueDate, accountDue.AccountTerms[0].AccountDetails[6].DueDate);
                Assert.IsTrue(accountDue.AccountTerms[0].AccountDetails[6].Overdue);
            }

            [TestMethod]
            public void DueDateOverrideProcessor_OverrideTermDueDates_TermOverrides_NoMatchingTermOverride()
            {
                AccountDue preOverride = accountDue;
                DueDateOverrides tempOverride = new DueDateOverrides() { TermOverrides = new Dictionary<string, DateTime>() { { "Term2", DateTime.Today.AddDays(3) } } };
                DueDateOverrideProcessor.OverrideTermDueDates(tempOverride, accountDue);

                Assert.AreEqual(preOverride.AccountTerms[0].AccountDetails[0].DueDate, accountDue.AccountTerms[0].AccountDetails[0].DueDate);
                Assert.AreEqual(preOverride.AccountTerms[0].AccountDetails[1].DueDate, accountDue.AccountTerms[0].AccountDetails[1].DueDate);
                Assert.AreEqual(preOverride.AccountTerms[0].AccountDetails[2].DueDate, accountDue.AccountTerms[0].AccountDetails[2].DueDate);
                Assert.AreEqual(preOverride.AccountTerms[0].AccountDetails[3].DueDate, accountDue.AccountTerms[0].AccountDetails[3].DueDate);
                Assert.AreEqual(preOverride.AccountTerms[0].AccountDetails[4].DueDate, accountDue.AccountTerms[0].AccountDetails[4].DueDate);
                Assert.AreEqual(preOverride.AccountTerms[0].AccountDetails[5].DueDate, accountDue.AccountTerms[0].AccountDetails[5].DueDate);
                Assert.AreEqual(preOverride.AccountTerms[0].AccountDetails[6].DueDate, accountDue.AccountTerms[0].AccountDetails[6].DueDate);
            }
        }

        [TestClass]
        public class DueDateOverrideProcessor_OverridePeriodDueDates : DueDateOverrideProcessorTests
        {
            [TestInitialize]
            public void DueDateOverrideProcessor_OverridePeriodDueDates_Initialize()
            {
                base.Initialize();
            }

            [TestCleanup]
            public void DueDateOverrideProcessor_OverridePeriodDueDates_Cleanup()
            {
                base.Cleanup();
            }

            [TestMethod]
            public void DueDateOverrideProcessor_OverridePeriodDueDates_NullDueDateOverrides()
            {
                DueDateOverrideProcessor.OverridePeriodDueDates(null, accountDuePeriod);
            }

            [TestMethod]
            public void DueDateOverrideProcessor_OverridePeriodDueDates_NullPastAccountDue()
            {
                accountDuePeriod.Past = null;
                DueDateOverrideProcessor.OverridePeriodDueDates(dueDateOverrides, accountDuePeriod);
            }

            [TestMethod]
            public void DueDateOverrideProcessor_OverridePeriodDueDates_NullPastAccountDueAccountTerms()
            {
                accountDuePeriod.Past.AccountTerms = null;
                DueDateOverrideProcessor.OverridePeriodDueDates(dueDateOverrides, accountDuePeriod);
            }

            [TestMethod]
            public void DueDateOverrideProcessor_OverridePeriodDueDates_PastPeriodOverride()
            {
                AccountDuePeriod preOverride = accountDuePeriod;
                dueDateOverrides.CurrentPeriodOverride = null;
                dueDateOverrides.FuturePeriodOverride = null;
                DueDateOverrideProcessor.OverridePeriodDueDates(dueDateOverrides, accountDuePeriod);

                Assert.AreEqual(dueDateOverrides.PastPeriodOverride, accountDuePeriod.Past.AccountTerms[0].AccountDetails[0].DueDate);
                Assert.IsTrue(accountDuePeriod.Past.AccountTerms[0].AccountDetails[0].Overdue);
                Assert.AreEqual(dueDateOverrides.PastPeriodOverride, accountDuePeriod.Past.AccountTerms[0].AccountDetails[1].DueDate);
                Assert.IsTrue(accountDuePeriod.Past.AccountTerms[0].AccountDetails[1].Overdue);
                Assert.AreEqual(preOverride.Past.AccountTerms[0].AccountDetails[2].DueDate, accountDuePeriod.Past.AccountTerms[0].AccountDetails[2].DueDate);
                Assert.IsFalse(accountDuePeriod.Past.AccountTerms[0].AccountDetails[2].Overdue);
                Assert.AreEqual(preOverride.Past.AccountTerms[0].AccountDetails[3].DueDate, accountDuePeriod.Past.AccountTerms[0].AccountDetails[3].DueDate);
                Assert.IsFalse(accountDuePeriod.Past.AccountTerms[0].AccountDetails[3].Overdue);
                Assert.AreEqual(preOverride.Past.AccountTerms[0].AccountDetails[4].DueDate, accountDuePeriod.Past.AccountTerms[0].AccountDetails[4].DueDate);
                Assert.IsFalse(accountDuePeriod.Past.AccountTerms[0].AccountDetails[4].Overdue);
            }

            [TestMethod]
            public void DueDateOverrideProcessor_OverridePeriodDueDates_CurrentPeriodOverride()
            {
                AccountDuePeriod preOverride = accountDuePeriod;
                dueDateOverrides.PastPeriodOverride = null;
                dueDateOverrides.FuturePeriodOverride = null;
                DueDateOverrideProcessor.OverridePeriodDueDates(dueDateOverrides, accountDuePeriod);

                Assert.AreEqual(dueDateOverrides.CurrentPeriodOverride, accountDuePeriod.Current.AccountTerms[0].AccountDetails[0].DueDate);
                Assert.IsFalse(accountDuePeriod.Current.AccountTerms[0].AccountDetails[0].Overdue);
                Assert.AreEqual(dueDateOverrides.CurrentPeriodOverride, accountDuePeriod.Current.AccountTerms[0].AccountDetails[1].DueDate);
                Assert.IsFalse(accountDuePeriod.Current.AccountTerms[0].AccountDetails[1].Overdue);
                Assert.AreEqual(preOverride.Current.AccountTerms[0].AccountDetails[2].DueDate, accountDuePeriod.Current.AccountTerms[0].AccountDetails[2].DueDate);
                Assert.IsFalse(accountDuePeriod.Current.AccountTerms[0].AccountDetails[2].Overdue);
                Assert.AreEqual(preOverride.Current.AccountTerms[0].AccountDetails[3].DueDate, accountDuePeriod.Current.AccountTerms[0].AccountDetails[3].DueDate);
                Assert.IsTrue(accountDuePeriod.Current.AccountTerms[0].AccountDetails[3].Overdue);
                Assert.AreEqual(preOverride.Current.AccountTerms[0].AccountDetails[4].DueDate, accountDuePeriod.Current.AccountTerms[0].AccountDetails[4].DueDate);
                Assert.IsTrue(accountDuePeriod.Current.AccountTerms[0].AccountDetails[4].Overdue);
                Assert.AreEqual(preOverride.Current.AccountTerms[0].AccountDetails[5].DueDate, accountDuePeriod.Current.AccountTerms[0].AccountDetails[5].DueDate);
                Assert.IsFalse(accountDuePeriod.Current.AccountTerms[0].AccountDetails[5].Overdue);
                Assert.AreEqual(dueDateOverrides.CurrentPeriodOverride, accountDuePeriod.Current.AccountTerms[0].AccountDetails[6].DueDate);
                Assert.IsFalse(accountDuePeriod.Current.AccountTerms[0].AccountDetails[6].Overdue);
            }

            [TestMethod]
            public void DueDateOverrideProcessor_OverridePeriodDueDates_FuturePeriodOverride()
            {
                AccountDuePeriod preOverride = accountDuePeriod;
                dueDateOverrides.PastPeriodOverride = null;
                dueDateOverrides.CurrentPeriodOverride = null;
                DueDateOverrideProcessor.OverridePeriodDueDates(dueDateOverrides, accountDuePeriod);

                Assert.AreEqual(dueDateOverrides.FuturePeriodOverride, accountDuePeriod.Future.AccountTerms[0].AccountDetails[0].DueDate);
                Assert.IsFalse(accountDuePeriod.Future.AccountTerms[0].AccountDetails[0].Overdue);
                Assert.AreEqual(dueDateOverrides.FuturePeriodOverride, accountDuePeriod.Future.AccountTerms[0].AccountDetails[1].DueDate);
                Assert.IsFalse(accountDuePeriod.Future.AccountTerms[0].AccountDetails[1].Overdue);
                Assert.AreEqual(preOverride.Future.AccountTerms[0].AccountDetails[2].DueDate, accountDuePeriod.Future.AccountTerms[0].AccountDetails[2].DueDate);
                Assert.IsFalse(accountDuePeriod.Future.AccountTerms[0].AccountDetails[2].Overdue);
                Assert.AreEqual(preOverride.Future.AccountTerms[0].AccountDetails[3].DueDate, accountDuePeriod.Future.AccountTerms[0].AccountDetails[3].DueDate);
                Assert.IsFalse(accountDuePeriod.Future.AccountTerms[0].AccountDetails[3].Overdue);
                Assert.AreEqual(preOverride.Future.AccountTerms[0].AccountDetails[4].DueDate, accountDuePeriod.Future.AccountTerms[0].AccountDetails[4].DueDate);
                Assert.IsFalse(accountDuePeriod.Future.AccountTerms[0].AccountDetails[4].Overdue);
            }
        }
    }
}
