// Copyright 2015-2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Entities
{
    /// <summary>
    /// This class tests the valid and invalid conditions of the LineItem domain
    /// entity. The LineItem domain entity requires an ID, description, quantity,
    /// price, and extended price.
    /// </summary>
    [TestClass]
    public class LineItemTests
    {
        #region Initialize and Cleanup
        private TestVoucherRepository voucherRepository;
        private string lineItemId = "1",
                description = "Line item description";
        private decimal quantity = 15m,
            price = 550.02m,
            extendedPrice = 600.00m;
        private string personId = "1";
        private int versionNumber;

        [TestInitialize]
        public void Initialize()
        {
            voucherRepository = new TestVoucherRepository();
            versionNumber = 2;
        }

        [TestCleanup]
        public void Cleanup()
        {
            voucherRepository = null;
        }
        #endregion

        #region Constructor tests
        [TestMethod]
        public void LineItem_Base()
        {
            var lineItem = new LineItem(lineItemId, description, quantity, price, extendedPrice);

            Assert.AreEqual(lineItemId, lineItem.Id);
            Assert.AreEqual(description, lineItem.Description);
            Assert.AreEqual(quantity, lineItem.Quantity);
            Assert.AreEqual(price, lineItem.Price);
            Assert.AreEqual(extendedPrice, lineItem.ExtendedPrice);
            Assert.IsTrue(lineItem.GlDistributions.Count() == 0);
            Assert.IsTrue(lineItem.LineItemTaxes.Count() == 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LineItem_NullId()
        {
            var lineItem = new LineItem(null, description, quantity, price, extendedPrice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LineItem_NullDescription()
        {
            var lineItem = new LineItem(lineItemId, null, quantity, price, extendedPrice);
        }
        #endregion

        #region AddGlDistribution tests
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AddGlDistribution_NullDistribution()
        {
            var voucher = await voucherRepository.GetVoucherAsync("1", personId, GlAccessLevel.Full_Access, null, versionNumber);
            var lineItem = voucher.LineItems.FirstOrDefault();
            lineItem.AddGlDistribution(null);
        }

        [TestMethod]
        public async Task AddGlDistribution_DuplicateDistribution()
        {
            var voucher = await voucherRepository.GetVoucherAsync("1", personId, GlAccessLevel.Full_Access, null, versionNumber);
            var lineItem = voucher.LineItems.FirstOrDefault();
            var glDistribution = lineItem.GlDistributions.FirstOrDefault();
            var glDistributionCount = lineItem.GlDistributions.Count();
            lineItem.AddGlDistribution(glDistribution);

            Assert.AreEqual(glDistributionCount, lineItem.GlDistributions.Count());
        }

        [TestMethod]
        public async Task AddGlDistribution_Success()
        {
            var voucher = await voucherRepository.GetVoucherAsync("1", personId, GlAccessLevel.Full_Access, null, versionNumber);
            var lineItem = voucher.LineItems.FirstOrDefault();
            var glDistributionCount = lineItem.GlDistributions.Count();
            string glAccount = "99_88_01_02_33445_75999";
            decimal quantity = 15m,
                amount = 550.02m;

            lineItem.AddGlDistribution(new LineItemGlDistribution(glAccount, quantity, amount));
            var glDistribution = lineItem.GlDistributions.Where(x => x.GlAccountNumber == glAccount).FirstOrDefault();

            Assert.AreEqual(glDistributionCount + 1, lineItem.GlDistributions.Count());
            Assert.AreEqual(glAccount, glDistribution.GlAccountNumber);
            Assert.AreEqual(quantity, glDistribution.Quantity);
            Assert.AreEqual(amount, glDistribution.Amount);
        }
        #endregion

        #region AddTax tests
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AddTax_NullTax()
        {
            var voucher = await voucherRepository.GetVoucherAsync("1", personId, GlAccessLevel.Full_Access, null, versionNumber);
            var lineItem = voucher.LineItems.FirstOrDefault();
            lineItem.AddTax(null);
        }

        [TestMethod]
        public async Task AddTax_DuplicateTax()
        {
            var voucher = await voucherRepository.GetVoucherAsync("1", personId, GlAccessLevel.Full_Access, null, versionNumber);
            var lineItem = voucher.LineItems.FirstOrDefault();
            var tax = lineItem.LineItemTaxes.FirstOrDefault();
            var taxCount = lineItem.LineItemTaxes.Count();
            lineItem.AddTax(tax);

            Assert.AreEqual(taxCount, lineItem.LineItemTaxes.Count());
        }

        [TestMethod]
        public async Task AddTax_Success()
        {
            var voucher = await voucherRepository.GetVoucherAsync("1", personId, GlAccessLevel.Full_Access, null, versionNumber);
            var lineItem = voucher.LineItems.FirstOrDefault();
            var taxCount = lineItem.LineItemTaxes.Count();
            string taxCode = "CA";
            decimal taxAmount = 15m;

            lineItem.AddTax(new LineItemTax(taxCode, taxAmount));
            var tax = lineItem.LineItemTaxes.Where(x => x.TaxCode == taxCode).FirstOrDefault();

            Assert.AreEqual(taxCount + 1, lineItem.LineItemTaxes.Count());
            Assert.AreEqual(taxCode, tax.TaxCode);
            Assert.AreEqual(taxAmount, tax.TaxAmount);
        }
        #endregion
    }
}
