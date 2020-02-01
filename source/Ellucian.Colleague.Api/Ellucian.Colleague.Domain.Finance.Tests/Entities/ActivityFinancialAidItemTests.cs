// Copyright 2014-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Finance.Entities.AccountActivity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Ellucian.Colleague.Domain.Finance.Tests.Entities
{
    [TestClass]
    public class ActivityFinancialAidItemTests
    {
        ActivityFinancialAidItem ap = new ActivityFinancialAidItem();

        [TestInitialize]
        public void Initialize()
        {
            ap.AwardTerms = new List<ActivityFinancialAidTerm>()
            {
                new ActivityFinancialAidTerm()
                {
                    AnticipatedAmount = 1000m,
                    AwardTerm = "14/FA",
                    DisbursedAmount = 2000m
                },
                new ActivityFinancialAidTerm()
                {
                    AnticipatedAmount = null,
                    AwardTerm = "15/SP",
                    DisbursedAmount = 1000m
                },
                new ActivityFinancialAidTerm()
                {
                    AnticipatedAmount = 200M,
                    AwardTerm = "15/FA",
                    DisbursedAmount = null
                }
            };
        }

        [TestMethod]
        public void ActivityFinancialAidItem_AssociatedPeriods()
        {
            var ap = new ActivityFinancialAidItem();
            CollectionAssert.AreEqual(new List<ActivityFinancialAidTerm>(), ap.AwardTerms);
        }

        [TestMethod]
        public void ActivityFinancialAidItem_StudentStatementAwardTerms_Default()
        {
            var ap = new ActivityFinancialAidItem();
            Assert.AreEqual(null, ap.StudentStatementAwardTerms);
        }

        [TestMethod]
        public void ActivityFinancialAidItem_StudentStatementAwardTerms_MultipleAwardTerms()
        {
            var expected = ap.AwardTerms[0].AwardTerm + Environment.NewLine + ap.AwardTerms[1].AwardTerm + Environment.NewLine + ap.AwardTerms[2].AwardTerm;
            Assert.AreEqual(expected, ap.StudentStatementAwardTerms);
        }

        [TestMethod]
        public void ActivityFinancialAidItem_StudentStatementDisbursedAmounts_Default()
        {
            var ap = new ActivityFinancialAidItem();
            Assert.AreEqual(null, ap.StudentStatementDisbursedAmounts);
        }

        [TestMethod]
        public void ActivityFinancialAidItem_StudentStatementDisbursedAmounts_MultipleAwardTerms()
        {
            var expected = string.Format(null, "{0:C}", ap.AwardTerms[0].DisbursedAmount) + Environment.NewLine + string.Format(null, "{0:C}", ap.AwardTerms[1].DisbursedAmount) + Environment.NewLine + string.Format(null, "{0:C}", ap.AwardTerms[2].DisbursedAmount);
            Assert.AreEqual(expected, ap.StudentStatementDisbursedAmounts);
        }

        [TestMethod]
        public void ActivityFinancialAidItem_StudentStatementDisbursedAmounts_MultipleAwardTerms_NullAndZeroDisbursedAmounts()
        {
            var ap2 = ap;
            ap2.AwardTerms[0].DisbursedAmount = null;
            ap2.AwardTerms[1].DisbursedAmount = 0;
            var expected = null + Environment.NewLine + string.Format(null, "{0:C}", ap.AwardTerms[1].DisbursedAmount) + Environment.NewLine + string.Format(null, "{0:C}", ap.AwardTerms[2].DisbursedAmount);
            Assert.AreEqual(expected, ap2.StudentStatementDisbursedAmounts);
        }

        [TestMethod]
        public void ActivityFinancialAidItem_StudentStatementAnticipatedAmounts_Default()
        {
            var ap = new ActivityFinancialAidItem();
            Assert.AreEqual(null, ap.StudentStatementAnticipatedAmounts);
        }

        [TestMethod]
        public void ActivityFinancialAidItem_StudentStatementAnticipatedAmounts_MultipleAwardTerms()
        {
            var expected = string.Format(null, "{0:C}", ap.AwardTerms[0].AnticipatedAmount) + Environment.NewLine + string.Format(null, "{0:C}", ap.AwardTerms[1].AnticipatedAmount) + Environment.NewLine + string.Format(null, "{0:C}", ap.AwardTerms[2].AnticipatedAmount); 
            Assert.AreEqual(expected, ap.StudentStatementAnticipatedAmounts);
        }

        [TestMethod]
        public void ActivityFinancialAidItem_StudentStatementAnticipatedAmounts_MultipleAwardTerms_NullAndZeroAnticipatedAmounts()
        {
            var ap2 = ap;
            ap2.AwardTerms[0].AnticipatedAmount = null;
            ap2.AwardTerms[1].AnticipatedAmount = 0;
            var expected = null + Environment.NewLine + string.Format(null, "{0:C}", ap.AwardTerms[1].AnticipatedAmount) + Environment.NewLine + string.Format(null, "{0:C}", ap.AwardTerms[2].AnticipatedAmount);
            Assert.AreEqual(expected, ap2.StudentStatementAnticipatedAmounts);
        }

        [TestMethod]
        public void ActivityFinancialAidItem_IneligibilityReasons_DefaultTest()
        {
            Assert.IsFalse(ap.IneligibilityReasons.Any());
        }

        [TestMethod]
        public void ActivityFinancialAidItem_IneligibilityReasons_GetSetTest()
        {
            var expectedReasons = new List<string>() {"Reason 1", "Reason 2" };
            ap.IneligibilityReasons.AddRange(expectedReasons);
            Assert.IsTrue(ap.IneligibilityReasons.Any());
            Assert.AreEqual(expectedReasons.Count, ap.IneligibilityReasons.Count);
        }

        /// <summary>
        /// Validate TransmitAwardExcess initializes to false
        /// </summary>
        [TestMethod]
        public void ActivityFinancialAidItem_TransmitAwardExcess_DefaultTest()
        {
            Assert.IsFalse(ap.TransmitAwardExcess);
        }

        /// <summary>
        /// Validate TransmitAwardExcess setter
        /// </summary>
        [TestMethod]
        public void ActivityFinancialAidItem_TransmitAwardExcess_GetSetTest()
        {
            ap.TransmitAwardExcess = true;
            Assert.IsTrue(ap.TransmitAwardExcess);
        }
    }
}
