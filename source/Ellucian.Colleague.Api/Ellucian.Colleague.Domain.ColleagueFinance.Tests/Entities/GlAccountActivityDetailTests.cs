// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Entities
{
    /// <summary>
    /// Test the General Ledger account number activity detail.
    /// </summary>
    [TestClass]
    public class GlAccountActivityDetailTests
    {
        #region Initialize and Cleanup

        private TestGlAccountActivityDetailRepository glAccountActivityDetailRepository;
        private string glAccount = "10_00_01_01_33333_51001";
        private string fiscalYear = DateTime.Now.ToString("yyyy");

        [TestInitialize]
        public void Initialize()
        {
            glAccountActivityDetailRepository = new TestGlAccountActivityDetailRepository();
        }

        [TestCleanup]
        public void Cleanup()
        {
            glAccountActivityDetailRepository = null;
        }
        #endregion

        [TestMethod]
        public async Task GlAccountActivityDetail_Constructor_Success()
        {
            string glClassName = "GL.CLASS";
            var glExpenseValues = new List<string>() { "5", "7" };
            var glRevenueValues = new List<string>() { "4", "6" };
            var glAssetValues = new List<string>() { "1" };
            var glLiabilityValues = new List<string>() { "2" };
            var glFundBalValues = new List<string>() { "3" };

            var activityRepository = await glAccountActivityDetailRepository.QueryGlActivityDetailAsync(glAccount, fiscalYear,
                new CostCenterStructure(), new GeneralLedgerClassConfiguration(glClassName, glExpenseValues, glRevenueValues, glAssetValues, glLiabilityValues, glFundBalValues));
            var glAccountActivityDetail = new GlAccountActivityDetail(activityRepository.GlAccountNumber);
            Assert.AreEqual(activityRepository.GlAccountNumber, glAccountActivityDetail.GlAccountNumber);
            Assert.IsTrue(glAccountActivityDetail.ActualAmount == 0);
            Assert.IsTrue(glAccountActivityDetail.BudgetAmount == 0);
            Assert.IsTrue(glAccountActivityDetail.EncumbranceAmount == 0);
            Assert.IsTrue(glAccountActivityDetail.MemoActualsAmount == 0);
            Assert.IsTrue(glAccountActivityDetail.MemoBudgetAmount == 0);

            Assert.IsTrue(glAccountActivityDetail.Transactions is ReadOnlyCollection<GlTransaction>, "The GL transaction list should be the correct type.");
        }
    }
}