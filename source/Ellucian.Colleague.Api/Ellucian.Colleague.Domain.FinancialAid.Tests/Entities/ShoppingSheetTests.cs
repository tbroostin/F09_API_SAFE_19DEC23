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
    public class ShoppingSheetTests
    {
        [TestClass]
        public class ShoppingSheetConstructorTests
        {
            public string AwardYear;
            public string StudentId;
            public List<ShoppingSheetCostItem> Costs;
            public List<ShoppingSheetAwardItem> GrantsAndScholarships;
            public List<ShoppingSheetAwardItem> WorkOptions;
            public List<ShoppingSheetAwardItem> LoanOptions;
            public int? FamilyContribution;
            public List<string> CustomMessages;

            public ShoppingSheet ShoppingSheet;

            [TestInitialize]
            public void Initialize()
            {
                AwardYear = "2014";
                StudentId = "0003914";
                Costs = new List<ShoppingSheetCostItem>() { new ShoppingSheetCostItem(ShoppingSheetBudgetGroup.BooksAndSupplies, 50) };
                GrantsAndScholarships = new List<ShoppingSheetAwardItem>() { new ShoppingSheetAwardItem(ShoppingSheetAwardGroup.SchoolGrants, 40) };
                WorkOptions = new List<ShoppingSheetAwardItem>() { new ShoppingSheetAwardItem(ShoppingSheetAwardGroup.WorkStudy, 30) };
                LoanOptions = new List<ShoppingSheetAwardItem>() { new ShoppingSheetAwardItem(ShoppingSheetAwardGroup.UnsubsidizedLoans, 20) };

                FamilyContribution = 10;
                CustomMessages = new List<string>() { "CustomMessag1", "CustomMessage2" };

                ShoppingSheet = new ShoppingSheet(AwardYear, StudentId);
            }

            [TestMethod]
            public void AwardYearTest()
            {
                Assert.AreEqual(AwardYear, ShoppingSheet.AwardYear);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AwardYearRequired()
            {
                new ShoppingSheet("", StudentId);
            }

            [TestMethod]
            public void StudentIdTest()
            {
                Assert.AreEqual(StudentId, ShoppingSheet.StudentId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentIdRequired()
            {
                new ShoppingSheet(AwardYear, null);
            }

            [TestMethod]
            public void FamilyContributionGetSetTest()
            {
                //Default value is null
                Assert.IsNull(ShoppingSheet.FamilyContribution);

                ShoppingSheet.FamilyContribution = FamilyContribution;
                Assert.AreEqual(FamilyContribution, ShoppingSheet.FamilyContribution);
            }

            [TestMethod]
            public void CustomMessagesGetSetTest()
            {
                //initialized to empty list
                Assert.AreEqual(0, ShoppingSheet.CustomMessages.Count());

                ShoppingSheet.CustomMessages = CustomMessages;
                CollectionAssert.AreEqual(CustomMessages, ShoppingSheet.CustomMessages);
            }

            [TestMethod]
            public void CostsGetSetTest()
            {
                //initialized to empty
                Assert.AreEqual(0, ShoppingSheet.Costs.Count());

                ShoppingSheet.Costs = Costs;
                CollectionAssert.AreEqual(Costs, ShoppingSheet.Costs);
            }

            [TestMethod]
            public void GrantsAndScholarshipsGetSetTest()
            {
                //initialized to empty
                Assert.AreEqual(0, ShoppingSheet.GrantsAndScholarships.Count());

                ShoppingSheet.GrantsAndScholarships = GrantsAndScholarships;
                CollectionAssert.AreEqual(GrantsAndScholarships, ShoppingSheet.GrantsAndScholarships);
            }

            [TestMethod]
            public void WorkOptionsGetSetTest()
            {
                //initialized to empty
                Assert.AreEqual(0, ShoppingSheet.WorkOptions.Count());

                ShoppingSheet.WorkOptions = WorkOptions;
                CollectionAssert.AreEqual(WorkOptions, ShoppingSheet.WorkOptions);
            }

            [TestMethod]
            public void LoanOptionsGetSetTest()
            {
                //initialized to empty
                Assert.AreEqual(0, ShoppingSheet.LoanOptions.Count());

                ShoppingSheet.LoanOptions = LoanOptions;
                CollectionAssert.AreEqual(LoanOptions, ShoppingSheet.LoanOptions);
            }

            [TestMethod]
            public void TotalEstimatedCostTest()
            {

                var expectedCost = Costs.Sum(c => c.Cost);
                ShoppingSheet.Costs = Costs;
                Assert.AreEqual(expectedCost, ShoppingSheet.TotalEstimatedCost);
            }

            [TestMethod]
            public void TotalEstimatedCostsIsNullTest()
            {
                ShoppingSheet.Costs = new List<ShoppingSheetCostItem>();
                Assert.AreEqual(null, ShoppingSheet.TotalEstimatedCost);
            }

            [TestMethod]
            public void TotalGrantsAndScholarshipsTest()
            {
                var expectedTotal = GrantsAndScholarships.Sum(a => a.Amount);
                ShoppingSheet.GrantsAndScholarships = GrantsAndScholarships;
                Assert.AreEqual(expectedTotal, ShoppingSheet.TotalGrantsAndScholarships);
            }

            [TestMethod]
            public void TotalGrantsAndScholarshipsIsNull()
            {
                ShoppingSheet.GrantsAndScholarships = new List<ShoppingSheetAwardItem>();
                Assert.AreEqual(null, ShoppingSheet.TotalGrantsAndScholarships);
            }

            [TestMethod]
            public void NetCostsTest()
            {
                var expectedCost = Costs.Sum(c => c.Cost) - GrantsAndScholarships.Sum(a => a.Amount);
                ShoppingSheet.Costs = Costs;
                ShoppingSheet.GrantsAndScholarships = GrantsAndScholarships;
                Assert.AreEqual(expectedCost, ShoppingSheet.NetCosts);
            }

            [TestMethod]
            public void NetCosts_CostsAreEmptyTest()
            {
                var expectedCost = 0 - GrantsAndScholarships.Sum(a => a.Amount);
                ShoppingSheet.Costs = new List<ShoppingSheetCostItem>();
                ShoppingSheet.GrantsAndScholarships = GrantsAndScholarships;
                Assert.AreEqual(expectedCost, ShoppingSheet.NetCosts);
            }

            [TestMethod]
            public void NetCosts_GrantsAndScholarshipsAreEmptyTest()
            {
                var expectedCost = Costs.Sum(c => c.Cost);
                ShoppingSheet.Costs = Costs;
                ShoppingSheet.GrantsAndScholarships = new List<ShoppingSheetAwardItem>();
                Assert.AreEqual(expectedCost, ShoppingSheet.NetCosts);
            }

            [TestMethod]
            public void NetCosts_CostsAndGrantsAndScholarshipsAreEmptyTest()
            {
                var expectedCost = 0;
                ShoppingSheet.Costs = new List<ShoppingSheetCostItem>();
                ShoppingSheet.GrantsAndScholarships = new List<ShoppingSheetAwardItem>();
                Assert.AreEqual(expectedCost, ShoppingSheet.NetCosts);
            }
        }

        [TestClass]
        public class ShoppingSheetEqualsTests
        {
            public string AwardYear;
            public string StudentId;

            public ShoppingSheet shoppingSheet;

            [TestInitialize]
            public void Initialize()
            {
                AwardYear = "2014";
                StudentId = "0003914";
                shoppingSheet = new ShoppingSheet(AwardYear, StudentId);
            }

            [TestMethod]
            public void ShoppingSheetEqualTest()
            {
                var testSheet = new ShoppingSheet(AwardYear, StudentId);
                Assert.AreEqual(testSheet, shoppingSheet);
            }

            [TestMethod]
            public void ShoppingSheetHashCodesEqualTest()
            {
                var testSheet = new ShoppingSheet(AwardYear, StudentId);
                Assert.AreEqual(testSheet.GetHashCode(), shoppingSheet.GetHashCode());
            }

            [TestMethod]
            public void DifferentAwardYearNotEqualTest()
            {
                var testSheet = new ShoppingSheet("foobar", StudentId);
                Assert.AreNotEqual(testSheet, shoppingSheet);
            }

            [TestMethod]
            public void DifferentAwardYearHashCodesNotEqualTest()
            {
                var testSheet = new ShoppingSheet("foobar", StudentId);
                Assert.AreNotEqual(testSheet.GetHashCode(), shoppingSheet.GetHashCode());
            }

            [TestMethod]
            public void DifferentStudentIdNotEqualTest()
            {
                var testSheet = new ShoppingSheet(AwardYear, "foobar");
                Assert.AreNotEqual(testSheet, shoppingSheet);
            }

            [TestMethod]
            public void DifferentStudentIdHashCodesNotEqualTest()
            {
                var testSheet = new ShoppingSheet(AwardYear, "foobar");
                Assert.AreNotEqual(testSheet.GetHashCode(), shoppingSheet.GetHashCode());
            }
        }
    }
}
