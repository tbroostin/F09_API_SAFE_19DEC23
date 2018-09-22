/*Copyright 2015-2017 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests.Services
{
    [TestClass]
    public class ShoppingSheetDomainServiceTests
    {
        public string studentId;
        public List<StudentAwardYear> studentAwardYears;
        public List<StudentAward> studentAwards;
        public List<BudgetComponent> budgetComponents;
        public List<StudentBudgetComponent> studentBudgetComponents;
        public List<FinancialAidApplication2> financialAidApplications;
        public List<ShoppingSheetRuleTable> shoppingSheetRuleTables;

        public void MainInitialize()
        {
            studentId = "0003914";
            studentAwardYears = new List<StudentAwardYear>() 
            {
                new StudentAwardYear(studentId, "2013", new FinancialAidOffice("MAIN")),
                new StudentAwardYear(studentId, "2014", new FinancialAidOffice("MAIN"))
            };
            studentAwardYears.ForEach(y =>
                y.CurrentOffice.AddConfiguration(
                    new FinancialAidConfiguration(y.CurrentOffice.Id, y.Code)
                    {
                        ShoppingSheetConfiguration = new ShoppingSheetConfiguration(),
                        IsShoppingSheetActive = true
                    }));
            studentAwards = new List<StudentAward>();
            budgetComponents = new List<BudgetComponent>();
            studentBudgetComponents = new List<StudentBudgetComponent>();
            financialAidApplications = new List<FinancialAidApplication2>();
            shoppingSheetRuleTables = new List<ShoppingSheetRuleTable>();
        }

        public async Task<ShoppingSheet> BuildShoppingSheetAsync(StudentAwardYear studentAwardYear)
        {
            return await ShoppingSheetDomainService.BuildShoppingSheetAsync(studentAwardYear, studentAwards, budgetComponents, studentBudgetComponents, financialAidApplications, shoppingSheetRuleTables);
        }

        [TestClass]
        public class BuildSingleShoppingSheetTest : ShoppingSheetDomainServiceTests
        {
            public StudentAwardYear studentAwardYear;
            public ShoppingSheet shoppingSheet;

            [TestInitialize]
            public void Initialize()
            {
                MainInitialize();
                studentAwardYear = studentAwardYears[0];
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentAwardYearRequiredTest()
            {
                await BuildShoppingSheetAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task NullConfigurationTest()
            {
                studentAwardYear = new StudentAwardYear(studentAwardYears[0].StudentId, studentAwardYears[0].Code, new FinancialAidOffice(studentAwardYears[0].CurrentOffice.Id));
                await BuildShoppingSheetAsync(studentAwardYear);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task NullShoppingSheetConfigurationTest()
            {
                studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration = null;
                await BuildShoppingSheetAsync(studentAwardYear);
            }

            [TestMethod]
            public void NullBudgetsAllowedIfAwardsExistTest()
            {
                var studentAward =
                    new StudentAward(studentAwardYear, studentId,
                        new Award("AWARD", "desc",
                            new AwardCategory("CATEGORY", "desc", null)) { ShoppingSheetGroup = ShoppingSheetAwardGroup.SchoolGrants }, true);
                new StudentAwardPeriod(studentAward, "P", new AwardStatus("ACTION", "desc", AwardStatusCategory.Accepted), false, false)
                {
                    AwardAmount = 500
                };
                studentAwards.Add(studentAward);

                budgetComponents = null;
                studentBudgetComponents = null;

                shoppingSheet = BuildShoppingSheetAsync(studentAwardYear).Result;
                Assert.IsNotNull(shoppingSheet);
                Assert.AreEqual(0, shoppingSheet.Costs.Count());
            }

            [TestMethod]
            public void NullStudentAwardsAllowedIfBudgetsExistTest()
            {
                budgetComponents.Add(new BudgetComponent(studentAwardYear.Code, "foobar", "description"));
                studentBudgetComponents.Add(new StudentBudgetComponent(studentAwardYear.Code, studentAwardYear.StudentId, "foobar", 5));

                studentAwards = null;
                shoppingSheet = BuildShoppingSheetAsync(studentAwardYear).Result;
                Assert.IsNotNull(shoppingSheet);
                Assert.AreEqual(0, shoppingSheet.GrantsAndScholarships.Count());
                Assert.AreEqual(0, shoppingSheet.WorkOptions.Count());
                Assert.AreEqual(3, shoppingSheet.LoanOptions.Count());
                Assert.AreEqual(0, shoppingSheet.LoanOptions.Sum(l => l.Amount));
            }

            [TestMethod]
            public async Task NullApplicationAllowedIfBudgetOrAwardExistTest()
            {
                budgetComponents.Add(new BudgetComponent(studentAwardYear.Code, "foobar", "description"));
                studentBudgetComponents.Add(new StudentBudgetComponent(studentAwardYear.Code, studentAwardYear.StudentId, "foobar", 5));

                financialAidApplications = null;
                shoppingSheet = await BuildShoppingSheetAsync(studentAwardYear);
                Assert.IsNotNull(shoppingSheet);
                Assert.IsNull(shoppingSheet.FamilyContribution);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task StudentBudgetComponentsExistButBudgetComponentsDoNotExistTest()
            {
                studentBudgetComponents.Add(new StudentBudgetComponent(studentAwardYear.Code, studentAwardYear.StudentId, "foobar", 5));
                await BuildShoppingSheetAsync(studentAwardYear);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task NoAwardsNoBudgetsTest()
            {
                studentAwards = new List<StudentAward>();
                studentBudgetComponents = new List<StudentBudgetComponent>();
                await BuildShoppingSheetAsync(studentAwardYear);
            }

            [TestMethod]
            public async Task ShoppingSheetCreatedForAwardYearAndStudentIdTest()
            {
                budgetComponents.Add(new BudgetComponent(studentAwardYear.Code, "foobar", "description"));
                studentBudgetComponents.Add(new StudentBudgetComponent(studentAwardYear.Code, studentAwardYear.StudentId, "foobar", 5));

                var studentAward =
                    new StudentAward(studentAwardYear, studentId,
                        new Award("AWARD", "desc",
                            new AwardCategory("CATEGORY", "desc", null)) { ShoppingSheetGroup = ShoppingSheetAwardGroup.SchoolGrants }, true);
                new StudentAwardPeriod(studentAward, "P", new AwardStatus("ACTION", "desc", AwardStatusCategory.Accepted), false, false)
                {
                    AwardAmount = 500
                };
                studentAwards.Add(studentAward);

                shoppingSheet = await BuildShoppingSheetAsync(studentAwardYear);
                Assert.AreEqual(studentAwardYear.Code, shoppingSheet.AwardYear);
                Assert.AreEqual(studentAwardYear.StudentId, shoppingSheet.StudentId);
            }

            [TestMethod]
            public async Task BudgetWithNullShoppingSheetGroupNotAddedToShoppingSheetTest()
            {
                budgetComponents.Add(new BudgetComponent(studentAwardYear.Code, "foobar", "description"));
                studentBudgetComponents.Add(new StudentBudgetComponent(studentAwardYear.Code, studentAwardYear.StudentId, "foobar", 5000));

                shoppingSheet = await BuildShoppingSheetAsync(studentAwardYear);
                Assert.AreEqual(0, shoppingSheet.Costs.Count());
            }

            [TestMethod]
            public async Task StudentDoesNotHaveTuitionAndFeesBudgetTest()
            {
                budgetComponents.Add(new BudgetComponent(studentAwardYear.Code, "foo", "description") { ShoppingSheetGroup = ShoppingSheetBudgetGroup.TuitionAndFees });
                studentBudgetComponents.Add(new StudentBudgetComponent(studentAwardYear.Code, studentAwardYear.StudentId, "bar", 5000));

                shoppingSheet = await BuildShoppingSheetAsync(studentAwardYear);

                var cost = shoppingSheet.Costs.FirstOrDefault(c => c.BudgetGroup == ShoppingSheetBudgetGroup.TuitionAndFees);
                Assert.IsNull(cost);
            }

            [TestMethod]
            public async Task BudgetAddedToTuitionAndFeesTest()
            {
                int testAmount = 1943;
                budgetComponents.Add(new BudgetComponent(studentAwardYear.Code, "foobar", "description") { ShoppingSheetGroup = ShoppingSheetBudgetGroup.TuitionAndFees });
                studentBudgetComponents.Add(new StudentBudgetComponent(studentAwardYear.Code, studentAwardYear.StudentId, "foobar", testAmount));

                shoppingSheet = await BuildShoppingSheetAsync(studentAwardYear);

                var cost = shoppingSheet.Costs.FirstOrDefault(c => c.BudgetGroup == ShoppingSheetBudgetGroup.TuitionAndFees);
                Assert.AreEqual(testAmount, cost.Cost);
            }

            [TestMethod]
            public async Task BudgetOverrideAddedToTuitionAndFeesTest()
            {
                int testAmount = 1943;
                budgetComponents.Add(new BudgetComponent(studentAwardYear.Code, "foobar", "description") { ShoppingSheetGroup = ShoppingSheetBudgetGroup.TuitionAndFees });
                studentBudgetComponents.Add(new StudentBudgetComponent(studentAwardYear.Code, studentAwardYear.StudentId, "foobar", 99999) { CampusBasedOverrideAmount = testAmount });

                shoppingSheet = await BuildShoppingSheetAsync(studentAwardYear);

                var cost = shoppingSheet.Costs.FirstOrDefault(c => c.BudgetGroup == ShoppingSheetBudgetGroup.TuitionAndFees);
                Assert.AreEqual(testAmount, cost.Cost);
            }

            [TestMethod]
            public async Task StudentDoesNotHaveHousingAndMealsBudgetTest()
            {
                budgetComponents.Add(new BudgetComponent(studentAwardYear.Code, "foo", "description") { ShoppingSheetGroup = ShoppingSheetBudgetGroup.HousingAndMeals });
                studentBudgetComponents.Add(new StudentBudgetComponent(studentAwardYear.Code, studentAwardYear.StudentId, "bar", 5000));

                shoppingSheet = await BuildShoppingSheetAsync(studentAwardYear);

                var cost = shoppingSheet.Costs.FirstOrDefault(c => c.BudgetGroup == ShoppingSheetBudgetGroup.HousingAndMeals);
                Assert.IsNull(cost);
            }

            [TestMethod]
            public async Task BudgetAddedToHousingAndMealsTest()
            {
                int testAmount = 1943;
                budgetComponents.Add(new BudgetComponent(studentAwardYear.Code, "foobar", "description") { ShoppingSheetGroup = ShoppingSheetBudgetGroup.HousingAndMeals });
                studentBudgetComponents.Add(new StudentBudgetComponent(studentAwardYear.Code, studentAwardYear.StudentId, "foobar", testAmount));

                shoppingSheet = await BuildShoppingSheetAsync(studentAwardYear);

                var cost = shoppingSheet.Costs.FirstOrDefault(c => c.BudgetGroup == ShoppingSheetBudgetGroup.HousingAndMeals);
                Assert.AreEqual(testAmount, cost.Cost);
            }

            [TestMethod]
            public async Task BudgetOverrideAddedToHousingAndMealsTest()
            {
                int testAmount = 1943;
                budgetComponents.Add(new BudgetComponent(studentAwardYear.Code, "foobar", "description") { ShoppingSheetGroup = ShoppingSheetBudgetGroup.HousingAndMeals });
                studentBudgetComponents.Add(new StudentBudgetComponent(studentAwardYear.Code, studentAwardYear.StudentId, "foobar", 99999) { CampusBasedOverrideAmount = testAmount });

                shoppingSheet = await BuildShoppingSheetAsync(studentAwardYear);

                var cost = shoppingSheet.Costs.FirstOrDefault(c => c.BudgetGroup == ShoppingSheetBudgetGroup.HousingAndMeals);
                Assert.AreEqual(testAmount, cost.Cost);
            }

            [TestMethod]
            public async Task StudentDoesNotHaveBooksAndSuppliesBudgetTest()
            {
                budgetComponents.Add(new BudgetComponent(studentAwardYear.Code, "foo", "description") { ShoppingSheetGroup = ShoppingSheetBudgetGroup.BooksAndSupplies });
                studentBudgetComponents.Add(new StudentBudgetComponent(studentAwardYear.Code, studentAwardYear.StudentId, "bar", 5000));

                shoppingSheet = await BuildShoppingSheetAsync(studentAwardYear);

                var cost = shoppingSheet.Costs.FirstOrDefault(c => c.BudgetGroup == ShoppingSheetBudgetGroup.BooksAndSupplies);
                Assert.IsNull(cost);
            }

            [TestMethod]
            public async Task BudgetAddedToBooksAndSuppliesTest()
            {
                int testAmount = 1943;
                budgetComponents.Add(new BudgetComponent(studentAwardYear.Code, "foobar", "description") { ShoppingSheetGroup = ShoppingSheetBudgetGroup.BooksAndSupplies });
                studentBudgetComponents.Add(new StudentBudgetComponent(studentAwardYear.Code, studentAwardYear.StudentId, "foobar", testAmount));

                shoppingSheet = await BuildShoppingSheetAsync(studentAwardYear);

                var cost = shoppingSheet.Costs.FirstOrDefault(c => c.BudgetGroup == ShoppingSheetBudgetGroup.BooksAndSupplies);
                Assert.AreEqual(testAmount, cost.Cost);
            }

            [TestMethod]
            public async Task BudgetOverrideAddedToBooksAndSuppliesTest()
            {
                int testAmount = 1943;
                budgetComponents.Add(new BudgetComponent(studentAwardYear.Code, "foobar", "description") { ShoppingSheetGroup = ShoppingSheetBudgetGroup.BooksAndSupplies });
                studentBudgetComponents.Add(new StudentBudgetComponent(studentAwardYear.Code, studentAwardYear.StudentId, "foobar", 99999) { CampusBasedOverrideAmount = testAmount });

                shoppingSheet = await BuildShoppingSheetAsync(studentAwardYear);

                var cost = shoppingSheet.Costs.FirstOrDefault(c => c.BudgetGroup == ShoppingSheetBudgetGroup.BooksAndSupplies);
                Assert.AreEqual(testAmount, cost.Cost);
            }

            [TestMethod]
            public async Task StudentDoesNotHaveTransportationBudgetTest()
            {
                budgetComponents.Add(new BudgetComponent(studentAwardYear.Code, "foo", "description") { ShoppingSheetGroup = ShoppingSheetBudgetGroup.Transportation });
                studentBudgetComponents.Add(new StudentBudgetComponent(studentAwardYear.Code, studentAwardYear.StudentId, "bar", 5000));

                shoppingSheet = await BuildShoppingSheetAsync(studentAwardYear);

                var cost = shoppingSheet.Costs.FirstOrDefault(c => c.BudgetGroup == ShoppingSheetBudgetGroup.Transportation);
                Assert.IsNull(cost);
            }

            [TestMethod]
            public async Task BudgetAddedToTransportationTest()
            {
                int testAmount = 1943;
                budgetComponents.Add(new BudgetComponent(studentAwardYear.Code, "foobar", "description") { ShoppingSheetGroup = ShoppingSheetBudgetGroup.Transportation });
                studentBudgetComponents.Add(new StudentBudgetComponent(studentAwardYear.Code, studentAwardYear.StudentId, "foobar", testAmount));

                shoppingSheet = await BuildShoppingSheetAsync(studentAwardYear);

                var cost = shoppingSheet.Costs.FirstOrDefault(c => c.BudgetGroup == ShoppingSheetBudgetGroup.Transportation);
                Assert.AreEqual(testAmount, cost.Cost);
            }

            [TestMethod]
            public async Task BudgetOverrideAddedToTransportationTest()
            {
                int testAmount = 1943;
                budgetComponents.Add(new BudgetComponent(studentAwardYear.Code, "foobar", "description") { ShoppingSheetGroup = ShoppingSheetBudgetGroup.Transportation });
                studentBudgetComponents.Add(new StudentBudgetComponent(studentAwardYear.Code, studentAwardYear.StudentId, "foobar", 99999) { CampusBasedOverrideAmount = testAmount });

                shoppingSheet = await BuildShoppingSheetAsync(studentAwardYear);

                var cost = shoppingSheet.Costs.FirstOrDefault(c => c.BudgetGroup == ShoppingSheetBudgetGroup.Transportation);
                Assert.AreEqual(testAmount, cost.Cost);
            }

            [TestMethod]
            public async Task StudentDoesNotHaveOtherCostsBudgetTest()
            {
                budgetComponents.Add(new BudgetComponent(studentAwardYear.Code, "foo", "description") { ShoppingSheetGroup = ShoppingSheetBudgetGroup.OtherCosts });
                studentBudgetComponents.Add(new StudentBudgetComponent(studentAwardYear.Code, studentAwardYear.StudentId, "bar", 5000));

                shoppingSheet = await BuildShoppingSheetAsync(studentAwardYear);

                var cost = shoppingSheet.Costs.FirstOrDefault(c => c.BudgetGroup == ShoppingSheetBudgetGroup.OtherCosts);
                Assert.IsNull(cost);
            }

            [TestMethod]
            public async Task BudgetAddedToOtherCostsTest()
            {
                int testAmount = 1943;
                budgetComponents.Add(new BudgetComponent(studentAwardYear.Code, "foobar", "description") { ShoppingSheetGroup = ShoppingSheetBudgetGroup.OtherCosts });
                studentBudgetComponents.Add(new StudentBudgetComponent(studentAwardYear.Code, studentAwardYear.StudentId, "foobar", testAmount));

                shoppingSheet = await BuildShoppingSheetAsync(studentAwardYear);

                var cost = shoppingSheet.Costs.FirstOrDefault(c => c.BudgetGroup == ShoppingSheetBudgetGroup.OtherCosts);
                Assert.AreEqual(testAmount, cost.Cost);
            }

            [TestMethod]
            public async Task BudgetOverrideAddedToOtherCostsTest()
            {
                int testAmount = 1943;
                budgetComponents.Add(new BudgetComponent(studentAwardYear.Code, "foobar", "description") { ShoppingSheetGroup = ShoppingSheetBudgetGroup.OtherCosts });
                studentBudgetComponents.Add(new StudentBudgetComponent(studentAwardYear.Code, studentAwardYear.StudentId, "foobar", 99999) { CampusBasedOverrideAmount = testAmount });

                shoppingSheet = await BuildShoppingSheetAsync(studentAwardYear);

                var cost = shoppingSheet.Costs.FirstOrDefault(c => c.BudgetGroup == ShoppingSheetBudgetGroup.OtherCosts);
                Assert.AreEqual(testAmount, cost.Cost);
            }

            [TestMethod]
            public async Task SchoolGrantNotAddedToGrantsAndScholarships()
            {
                budgetComponents.Add(new BudgetComponent(studentAwardYear.Code, "foobar", "description"));
                studentBudgetComponents.Add(new StudentBudgetComponent(studentAwardYear.Code, studentAwardYear.StudentId, "foobar", 99999));
                studentAwards = new List<StudentAward>();

                shoppingSheet = await BuildShoppingSheetAsync(studentAwardYear);

                var amount = shoppingSheet.GrantsAndScholarships.FirstOrDefault(c => c.AwardGroup == ShoppingSheetAwardGroup.SchoolGrants);
                Assert.IsNull(amount);
            }

            [TestMethod]
            public async Task SchoolGrantAddedToGrantsAndScholarships()
            {
                var studentAward =
                    new StudentAward(studentAwardYear, studentId,
                        new Award("AWARD", "desc",
                            new AwardCategory("CATEGORY", "desc", null)) { ShoppingSheetGroup = ShoppingSheetAwardGroup.SchoolGrants }, true);

                new StudentAwardPeriod(studentAward, "P", new AwardStatus("ACTION", "desc", AwardStatusCategory.Accepted), false, false)
                {
                    AwardAmount = 500
                };

                studentAwards.Add(studentAward);

                shoppingSheet = await BuildShoppingSheetAsync(studentAwardYear);

                var amount = shoppingSheet.GrantsAndScholarships.FirstOrDefault(c => c.AwardGroup == ShoppingSheetAwardGroup.SchoolGrants);
                Assert.AreEqual(studentAward.AwardAmount.Value, amount.Amount);
            }

            [TestMethod]
            public async Task PellGrantNotAddedToGrantsAndScholarships()
            {
                budgetComponents.Add(new BudgetComponent(studentAwardYear.Code, "foobar", "description"));
                studentBudgetComponents.Add(new StudentBudgetComponent(studentAwardYear.Code, studentAwardYear.StudentId, "foobar", 99999));
                studentAwards = new List<StudentAward>();

                shoppingSheet = await BuildShoppingSheetAsync(studentAwardYear);

                var amount = shoppingSheet.GrantsAndScholarships.FirstOrDefault(c => c.AwardGroup == ShoppingSheetAwardGroup.PellGrants);
                Assert.IsNull(amount);
            }

            [TestMethod]
            public async Task PellGrantAddedToGrantsAndScholarships()
            {
                var studentAward =
                    new StudentAward(studentAwardYear, studentId,
                        new Award("AWARD", "desc",
                            new AwardCategory("CATEGORY", "desc", null)) { ShoppingSheetGroup = ShoppingSheetAwardGroup.PellGrants }, true);

                new StudentAwardPeriod(studentAward, "P", new AwardStatus("ACTION", "desc", AwardStatusCategory.Accepted), false, false)
                {
                    AwardAmount = 500
                };

                studentAwards.Add(studentAward);

                shoppingSheet = await BuildShoppingSheetAsync(studentAwardYear);

                var amount = shoppingSheet.GrantsAndScholarships.FirstOrDefault(c => c.AwardGroup == ShoppingSheetAwardGroup.PellGrants);
                Assert.AreEqual(studentAward.AwardAmount.Value, amount.Amount);
            }

            [TestMethod]
            public async Task StateGrantNotAddedToGrantsAndScholarships()
            {
                budgetComponents.Add(new BudgetComponent(studentAwardYear.Code, "foobar", "description"));
                studentBudgetComponents.Add(new StudentBudgetComponent(studentAwardYear.Code, studentAwardYear.StudentId, "foobar", 99999));
                studentAwards = new List<StudentAward>();

                shoppingSheet = await BuildShoppingSheetAsync(studentAwardYear);

                var amount = shoppingSheet.GrantsAndScholarships.FirstOrDefault(c => c.AwardGroup == ShoppingSheetAwardGroup.StateGrants);
                Assert.IsNull(amount);
            }

            [TestMethod]
            public async Task StateGrantAddedToGrantsAndScholarships()
            {
                var studentAward =
                    new StudentAward(studentAwardYear, studentId,
                        new Award("AWARD", "desc",
                            new AwardCategory("CATEGORY", "desc", null)) { ShoppingSheetGroup = ShoppingSheetAwardGroup.StateGrants }, true);

                new StudentAwardPeriod(studentAward, "P", new AwardStatus("ACTION", "desc", AwardStatusCategory.Accepted), false, false)
                {
                    AwardAmount = 500
                };

                studentAwards.Add(studentAward);

                shoppingSheet = await BuildShoppingSheetAsync(studentAwardYear);

                var amount = shoppingSheet.GrantsAndScholarships.FirstOrDefault(c => c.AwardGroup == ShoppingSheetAwardGroup.StateGrants);
                Assert.AreEqual(studentAward.AwardAmount.Value, amount.Amount);
            }

            [TestMethod]
            public async Task OtherGrantNotAddedToGrantsAndScholarships()
            {
                budgetComponents.Add(new BudgetComponent(studentAwardYear.Code, "foobar", "description"));
                studentBudgetComponents.Add(new StudentBudgetComponent(studentAwardYear.Code, studentAwardYear.StudentId, "foobar", 99999));
                studentAwards = new List<StudentAward>();

                shoppingSheet = await BuildShoppingSheetAsync(studentAwardYear);

                var amount = shoppingSheet.GrantsAndScholarships.FirstOrDefault(c => c.AwardGroup == ShoppingSheetAwardGroup.OtherGrants);
                Assert.IsNull(amount);
            }

            [TestMethod]
            public async Task OtherGrantAddedToGrantsAndScholarships()
            {
                var studentAward =
                    new StudentAward(studentAwardYear, studentId,
                        new Award("AWARD", "desc",
                            new AwardCategory("CATEGORY", "desc", null)) { ShoppingSheetGroup = ShoppingSheetAwardGroup.OtherGrants }, true);

                new StudentAwardPeriod(studentAward, "P", new AwardStatus("ACTION", "desc", AwardStatusCategory.Accepted), false, false)
                {
                    AwardAmount = 500
                };

                studentAwards.Add(studentAward);

                shoppingSheet = await BuildShoppingSheetAsync(studentAwardYear);

                var amount = shoppingSheet.GrantsAndScholarships.FirstOrDefault(c => c.AwardGroup == ShoppingSheetAwardGroup.OtherGrants);
                Assert.AreEqual(studentAward.AwardAmount.Value, amount.Amount);
            }

            [TestMethod]
            public async Task WorkStudyNotAddedToWorkOptions()
            {
                budgetComponents.Add(new BudgetComponent(studentAwardYear.Code, "foobar", "description"));
                studentBudgetComponents.Add(new StudentBudgetComponent(studentAwardYear.Code, studentAwardYear.StudentId, "foobar", 99999));
                studentAwards = new List<StudentAward>();

                shoppingSheet = await BuildShoppingSheetAsync(studentAwardYear);

                var amount = shoppingSheet.WorkOptions.FirstOrDefault(c => c.AwardGroup == ShoppingSheetAwardGroup.WorkStudy);
                Assert.IsNull(amount);
            }

            [TestMethod]
            public async Task WorkStudyAddedToWorkOptions()
            {
                var studentAward =
                    new StudentAward(studentAwardYear, studentId,
                        new Award("AWARD", "desc",
                            new AwardCategory("CATEGORY", "desc", null)) { ShoppingSheetGroup = ShoppingSheetAwardGroup.WorkStudy }, true);

                new StudentAwardPeriod(studentAward, "P", new AwardStatus("ACTION", "desc", AwardStatusCategory.Accepted), false, false)
                {
                    AwardAmount = 500
                };

                studentAwards.Add(studentAward);

                shoppingSheet = await BuildShoppingSheetAsync(studentAwardYear);

                var amount = shoppingSheet.WorkOptions.FirstOrDefault(c => c.AwardGroup == ShoppingSheetAwardGroup.WorkStudy);
                Assert.AreEqual(studentAward.AwardAmount.Value, amount.Amount);
            }

            [TestMethod]
            public async Task PerkinsLoanZeroAmountAddedToLoanOptions()
            {
                budgetComponents.Add(new BudgetComponent(studentAwardYear.Code, "foobar", "description"));
                studentBudgetComponents.Add(new StudentBudgetComponent(studentAwardYear.Code, studentAwardYear.StudentId, "foobar", 99999));
                studentAwards = new List<StudentAward>();

                shoppingSheet = await BuildShoppingSheetAsync(studentAwardYear);

                var amount = shoppingSheet.LoanOptions.FirstOrDefault(c => c.AwardGroup == ShoppingSheetAwardGroup.PerkinsLoans);
                Assert.AreEqual(0, amount.Amount);
            }

            [TestMethod]
            public async Task PerkinsLoanAddedToLoanOptions()
            {
                var studentAward =
                    new StudentAward(studentAwardYear, studentId,
                        new Award("AWARD", "desc",
                            new AwardCategory("CATEGORY", "desc", null)) { ShoppingSheetGroup = ShoppingSheetAwardGroup.PerkinsLoans }, true);

                new StudentAwardPeriod(studentAward, "P", new AwardStatus("ACTION", "desc", AwardStatusCategory.Accepted), false, false)
                {
                    AwardAmount = 500
                };

                studentAwards.Add(studentAward);

                shoppingSheet = await BuildShoppingSheetAsync(studentAwardYear);

                var amount = shoppingSheet.LoanOptions.FirstOrDefault(c => c.AwardGroup == ShoppingSheetAwardGroup.PerkinsLoans);
                Assert.AreEqual(studentAward.AwardAmount.Value, amount.Amount);
            }

            [TestMethod]
            public async Task SubsidizedLoansZeroAmountAddedToLoanOptions()
            {
                budgetComponents.Add(new BudgetComponent(studentAwardYear.Code, "foobar", "description"));
                studentBudgetComponents.Add(new StudentBudgetComponent(studentAwardYear.Code, studentAwardYear.StudentId, "foobar", 99999));
                studentAwards = new List<StudentAward>();

                shoppingSheet = await BuildShoppingSheetAsync(studentAwardYear);

                var amount = shoppingSheet.LoanOptions.FirstOrDefault(c => c.AwardGroup == ShoppingSheetAwardGroup.SubsidizedLoans);
                Assert.AreEqual(0, amount.Amount);
            }

            [TestMethod]
            public async Task SubsidizedLoansAddedToLoanOptions()
            {
                var studentAward =
                    new StudentAward(studentAwardYear, studentId,
                        new Award("AWARD", "desc",
                            new AwardCategory("CATEGORY", "desc", null)) { ShoppingSheetGroup = ShoppingSheetAwardGroup.SubsidizedLoans }, true);

                new StudentAwardPeriod(studentAward, "P", new AwardStatus("ACTION", "desc", AwardStatusCategory.Accepted), false, false)
                {
                    AwardAmount = 500
                };

                studentAwards.Add(studentAward);

                shoppingSheet = await BuildShoppingSheetAsync(studentAwardYear);

                var amount = shoppingSheet.LoanOptions.FirstOrDefault(c => c.AwardGroup == ShoppingSheetAwardGroup.SubsidizedLoans);
                Assert.AreEqual(studentAward.AwardAmount.Value, amount.Amount);
            }

            [TestMethod]
            public async Task UnsubsidizedLoansZeroAmountAddedToLoanOptions()
            {
                budgetComponents.Add(new BudgetComponent(studentAwardYear.Code, "foobar", "description"));
                studentBudgetComponents.Add(new StudentBudgetComponent(studentAwardYear.Code, studentAwardYear.StudentId, "foobar", 99999));
                studentAwards = new List<StudentAward>();

                shoppingSheet = await BuildShoppingSheetAsync(studentAwardYear);

                var amount = shoppingSheet.LoanOptions.FirstOrDefault(c => c.AwardGroup == ShoppingSheetAwardGroup.UnsubsidizedLoans);
                Assert.AreEqual(0, amount.Amount);
            }

            [TestMethod]
            public async Task UnsubsidizedLoansAddedToWorkOptions()
            {
                var studentAward =
                    new StudentAward(studentAwardYear, studentId,
                        new Award("AWARD", "desc",
                            new AwardCategory("CATEGORY", "desc", null)) { ShoppingSheetGroup = ShoppingSheetAwardGroup.UnsubsidizedLoans }, true);

                new StudentAwardPeriod(studentAward, "P", new AwardStatus("ACTION", "desc", AwardStatusCategory.Accepted), false, false)
                {
                    AwardAmount = 500
                };

                studentAwards.Add(studentAward);

                shoppingSheet = await BuildShoppingSheetAsync(studentAwardYear);

                var amount = shoppingSheet.LoanOptions.FirstOrDefault(c => c.AwardGroup == ShoppingSheetAwardGroup.UnsubsidizedLoans);
                Assert.AreEqual(studentAward.AwardAmount.Value, amount.Amount);
            }

            /// <summary>
            /// Test if a student award with award status category to be excluded from the
            /// shopping sheet does not get added to one of the GrantsAndScholarships
            /// </summary>
            [TestMethod]
            public async Task AwardNotAddedtoGrantsAndScholarshipsTest()
            {
                var studentAward = new StudentAward(studentAwardYear, studentId,
                    new Award("Award", "desc",
                        new AwardCategory("Category", "desc", null)) { ShoppingSheetGroup = ShoppingSheetAwardGroup.OtherGrants }, true);
                new StudentAwardPeriod(studentAward, "D", new AwardStatus("ACTION", "desc", AwardStatusCategory.Denied), false, false)
                {
                    AwardAmount = 1000
                };
                studentAwards.Add(studentAward);

                studentAwardYear.CurrentConfiguration.ExcludeAwardStatusCategoriesFromAwardLetterAndShoppingSheet.Add(AwardStatusCategory.Denied);
                shoppingSheet = await BuildShoppingSheetAsync(studentAwardYear);

                var amount = shoppingSheet.GrantsAndScholarships.FirstOrDefault(c => c.AwardGroup == ShoppingSheetAwardGroup.OtherGrants);
                Assert.IsNull(amount);
            }

            /// <summary>
            /// Tests if one of the award periods for the same award gets added and the other one
            /// with the status category to be excluded does not 
            /// </summary>
            [TestMethod]
            public async Task AwardPeriodGetsAddedToLoanOptionsTest()
            {
                var expectedAmount = 500;
                var studentAward = new StudentAward(studentAwardYear, studentId,
                    new Award("Award", "desc",
                        new AwardCategory("Category", "desc", null)) { ShoppingSheetGroup = ShoppingSheetAwardGroup.PerkinsLoans }, true);
                new StudentAwardPeriod(studentAward, "D", new AwardStatus("ACTION", "desc", AwardStatusCategory.Denied), false, false)
                {
                    AwardAmount = 300
                };
                new StudentAwardPeriod(studentAward, "A", new AwardStatus("ACTION1", "desc1", AwardStatusCategory.Accepted), false, false)
                {
                    AwardAmount = expectedAmount
                };
                studentAwards.Add(studentAward);

                studentAwardYear.CurrentConfiguration.ExcludeAwardStatusCategoriesFromAwardLetterAndShoppingSheet.Add(AwardStatusCategory.Denied);
                shoppingSheet = await BuildShoppingSheetAsync(studentAwardYear);

                var amount = shoppingSheet.LoanOptions.FirstOrDefault(c => c.AwardGroup == ShoppingSheetAwardGroup.PerkinsLoans);
                Assert.AreEqual(expectedAmount, amount.Amount);
            }

            [TestMethod]
            public async Task NoApplications_SetFamilyContributionToNullTest()
            {
                studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.EfcOption = ShoppingSheetEfcOption.IsirEfc;
                financialAidApplications = new List<FinancialAidApplication2>();
                budgetComponents.Add(new BudgetComponent(studentAwardYear.Code, "foobar", "description"));
                studentBudgetComponents.Add(new StudentBudgetComponent(studentAwardYear.Code, studentAwardYear.StudentId, "foobar", 99999));
                shoppingSheet = await BuildShoppingSheetAsync(studentAwardYear);

                Assert.IsNull(shoppingSheet.FamilyContribution);
            }

            [TestMethod]
            public async Task IsirEfcOption_WithFederallyFlaggedIsir_SetFamilyContributionTest()
            {
                int efc = 55555;
                studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.EfcOption = ShoppingSheetEfcOption.IsirEfc;
                financialAidApplications.Add(new Fafsa("foobar", studentAwardYear.Code, studentAwardYear.StudentId) { IsFederallyFlagged = true, FamilyContribution = efc });

                budgetComponents.Add(new BudgetComponent(studentAwardYear.Code, "foobar", "description"));
                studentBudgetComponents.Add(new StudentBudgetComponent(studentAwardYear.Code, studentAwardYear.StudentId, "foobar", 99999));

                shoppingSheet = await BuildShoppingSheetAsync(studentAwardYear);

                Assert.AreEqual(efc, shoppingSheet.FamilyContribution.Value);
            }

            [TestMethod]
            public async Task IsirEfcOption_WithFederallyFlaggedProfile_SetFamilyContributionTest()
            {
                int efc = 55555;
                studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.EfcOption = ShoppingSheetEfcOption.IsirEfc;
                financialAidApplications.Add(new ProfileApplication("foobar", studentAwardYear.Code, studentAwardYear.StudentId) { IsFederallyFlagged = true, FamilyContribution = efc });

                budgetComponents.Add(new BudgetComponent(studentAwardYear.Code, "foobar", "description"));
                studentBudgetComponents.Add(new StudentBudgetComponent(studentAwardYear.Code, studentAwardYear.StudentId, "foobar", 99999));

                shoppingSheet = await BuildShoppingSheetAsync(studentAwardYear);

                Assert.AreEqual(efc, shoppingSheet.FamilyContribution.Value);
            }

            [TestMethod]
            public async Task IsirEfcOption_WithNoFederallyFlaggedIsir_SetFamilyContributionToNullTest()
            {
                int efc = 55555;
                studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.EfcOption = ShoppingSheetEfcOption.IsirEfc;
                financialAidApplications.Add(new Fafsa("foobar", studentAwardYear.Code, studentAwardYear.StudentId) { IsFederallyFlagged = false, FamilyContribution = efc });

                budgetComponents.Add(new BudgetComponent(studentAwardYear.Code, "foobar", "description"));
                studentBudgetComponents.Add(new StudentBudgetComponent(studentAwardYear.Code, studentAwardYear.StudentId, "foobar", 99999));

                shoppingSheet = await BuildShoppingSheetAsync(studentAwardYear);

                Assert.IsNull(shoppingSheet.FamilyContribution);
            }

            [TestMethod]
            public async Task ProfileEfcOption_WithInsitutionallyFlaggedProfile_SetFamilyContributionTest()
            {
                int efc = 55555;
                studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.EfcOption = ShoppingSheetEfcOption.ProfileEfc;
                financialAidApplications.Add(new ProfileApplication("foobar", studentAwardYear.Code, studentAwardYear.StudentId) { IsInstitutionallyFlagged = true, InstitutionalFamilyContribution = efc });

                budgetComponents.Add(new BudgetComponent(studentAwardYear.Code, "foobar", "description"));
                studentBudgetComponents.Add(new StudentBudgetComponent(studentAwardYear.Code, studentAwardYear.StudentId, "foobar", 99999));

                shoppingSheet = await BuildShoppingSheetAsync(studentAwardYear);

                Assert.AreEqual(efc, shoppingSheet.FamilyContribution.Value);
            }

            [TestMethod]
            public async Task ProfileEfcOption_WithInsitutionallyFlaggedFafsa_SetFamilyContributionToNullTest()
            {
                int efc = 55555;
                studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.EfcOption = ShoppingSheetEfcOption.ProfileEfc;
                financialAidApplications.Add(new Fafsa("foobar", studentAwardYear.Code, studentAwardYear.StudentId) { IsInstitutionallyFlagged = true, InstitutionalFamilyContribution = efc });

                budgetComponents.Add(new BudgetComponent(studentAwardYear.Code, "foobar", "description"));
                studentBudgetComponents.Add(new StudentBudgetComponent(studentAwardYear.Code, studentAwardYear.StudentId, "foobar", 99999));

                shoppingSheet = await BuildShoppingSheetAsync(studentAwardYear);

                Assert.IsNull(shoppingSheet.FamilyContribution);
            }

            [TestMethod]
            public async Task ProfileEfcOption_WithNoInsitutionallyFlaggedProfile_SetFamilyContributionToNullTest()
            {
                int efc = 55555;
                studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.EfcOption = ShoppingSheetEfcOption.ProfileEfc;
                financialAidApplications.Add(new ProfileApplication("foobar", studentAwardYear.Code, studentAwardYear.StudentId) { IsInstitutionallyFlagged = false, InstitutionalFamilyContribution = efc });

                budgetComponents.Add(new BudgetComponent(studentAwardYear.Code, "foobar", "description"));
                studentBudgetComponents.Add(new StudentBudgetComponent(studentAwardYear.Code, studentAwardYear.StudentId, "foobar", 99999));

                shoppingSheet = await BuildShoppingSheetAsync(studentAwardYear);

                Assert.IsNull(shoppingSheet.FamilyContribution);
            }

            [TestMethod]
            public async Task ProfileEfcUntilIsirExistsOption_WithFederallyFlaggedFafsa_SetFamilyContributionTest()
            {
                int efc = 55555;
                studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.EfcOption = ShoppingSheetEfcOption.ProfileEfcUntilIsirExists;
                financialAidApplications.Add(new Fafsa("foobar", studentAwardYear.Code, studentAwardYear.StudentId) { IsFederallyFlagged = true, FamilyContribution = efc });

                budgetComponents.Add(new BudgetComponent(studentAwardYear.Code, "foobar", "description"));
                studentBudgetComponents.Add(new StudentBudgetComponent(studentAwardYear.Code, studentAwardYear.StudentId, "foobar", 99999));

                shoppingSheet = await BuildShoppingSheetAsync(studentAwardYear);

                Assert.AreEqual(efc, shoppingSheet.FamilyContribution.Value);
            }

            [TestMethod]
            public async Task ProfileEfcUntilIsirExistsOption_WithNoFederallyFlaggedFafsa_SetFamilyContributionToNullTest()
            {
                int efc = 55555;
                studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.EfcOption = ShoppingSheetEfcOption.ProfileEfcUntilIsirExists;
                financialAidApplications.Add(new Fafsa("foobar", studentAwardYear.Code, studentAwardYear.StudentId) { IsFederallyFlagged = false, FamilyContribution = efc });

                budgetComponents.Add(new BudgetComponent(studentAwardYear.Code, "foobar", "description"));
                studentBudgetComponents.Add(new StudentBudgetComponent(studentAwardYear.Code, studentAwardYear.StudentId, "foobar", 99999));

                shoppingSheet = await BuildShoppingSheetAsync(studentAwardYear);

                Assert.IsNull(shoppingSheet.FamilyContribution);
            }

            [TestMethod]
            public async Task ProfileEfcUntilIsirExistsOption_WithFederallyFlaggedProfile_SetFamilyContributionToNullTest()
            {
                int efc = 55555;
                studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.EfcOption = ShoppingSheetEfcOption.ProfileEfcUntilIsirExists;
                financialAidApplications.Add(new ProfileApplication("foobar", studentAwardYear.Code, studentAwardYear.StudentId) { IsFederallyFlagged = true, FamilyContribution = efc });

                budgetComponents.Add(new BudgetComponent(studentAwardYear.Code, "foobar", "description"));
                studentBudgetComponents.Add(new StudentBudgetComponent(studentAwardYear.Code, studentAwardYear.StudentId, "foobar", 99999));

                shoppingSheet = await BuildShoppingSheetAsync(studentAwardYear);

                Assert.IsNull(shoppingSheet.FamilyContribution);
            }

            [TestMethod]
            public async Task ProfileEfcUntilIsirExistsOption_WithInstitutionalalProfile_SetFamilyContributionTest()
            {
                int efc = 55555;
                studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.EfcOption = ShoppingSheetEfcOption.ProfileEfcUntilIsirExists;
                financialAidApplications.Add(new ProfileApplication("foobar", studentAwardYear.Code, studentAwardYear.StudentId) { IsInstitutionallyFlagged = true, InstitutionalFamilyContribution = efc });

                budgetComponents.Add(new BudgetComponent(studentAwardYear.Code, "foobar", "description"));
                studentBudgetComponents.Add(new StudentBudgetComponent(studentAwardYear.Code, studentAwardYear.StudentId, "foobar", 99999));

                shoppingSheet = await BuildShoppingSheetAsync(studentAwardYear);

                Assert.AreEqual(efc, shoppingSheet.FamilyContribution.Value);
            }

            [TestMethod]
            public async Task ProfileEfcUntilIsirExistsOption_WithNoInstitutionalProfile_SetFamilyContributionToNullTest()
            {
                int efc = 55555;
                studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.EfcOption = ShoppingSheetEfcOption.ProfileEfcUntilIsirExists;
                financialAidApplications.Add(new ProfileApplication("foobar", studentAwardYear.Code, studentAwardYear.StudentId) { IsInstitutionallyFlagged = false, InstitutionalFamilyContribution = efc });

                budgetComponents.Add(new BudgetComponent(studentAwardYear.Code, "foobar", "description"));
                studentBudgetComponents.Add(new StudentBudgetComponent(studentAwardYear.Code, studentAwardYear.StudentId, "foobar", 99999));

                shoppingSheet = await BuildShoppingSheetAsync(studentAwardYear);

                Assert.IsNull(shoppingSheet.FamilyContribution);
            }

            [TestMethod]
            public async Task ProfileEfcUntilIsirExistsOption_WithInstitutionalFafsa_SetFamilyContributionToNullTest()
            {
                int efc = 55555;
                studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.EfcOption = ShoppingSheetEfcOption.ProfileEfcUntilIsirExists;
                financialAidApplications.Add(new Fafsa("foobar", studentAwardYear.Code, studentAwardYear.StudentId) { IsInstitutionallyFlagged = true, InstitutionalFamilyContribution = efc });

                budgetComponents.Add(new BudgetComponent(studentAwardYear.Code, "foobar", "description"));
                studentBudgetComponents.Add(new StudentBudgetComponent(studentAwardYear.Code, studentAwardYear.StudentId, "foobar", 99999));

                shoppingSheet = await BuildShoppingSheetAsync(studentAwardYear);

                Assert.IsNull(shoppingSheet.FamilyContribution);
            }

            [TestMethod]
            public async Task ProfileEfcUntilIsirExistsOption_WithFederalFafsaInstitutionalalProfile_SetFamilyContributionToFafsaEfcTest()
            {
                int profileEfc = 99999;
                int fafsaEfc = 55555;
                studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.EfcOption = ShoppingSheetEfcOption.ProfileEfcUntilIsirExists;
                financialAidApplications.Add(new Fafsa("fafsa", studentAwardYear.Code, studentAwardYear.StudentId) { IsFederallyFlagged = true, FamilyContribution = fafsaEfc });
                financialAidApplications.Add(new ProfileApplication("profile", studentAwardYear.Code, studentAwardYear.StudentId) { IsInstitutionallyFlagged = true, InstitutionalFamilyContribution = profileEfc });

                budgetComponents.Add(new BudgetComponent(studentAwardYear.Code, "foobar", "description"));
                studentBudgetComponents.Add(new StudentBudgetComponent(studentAwardYear.Code, studentAwardYear.StudentId, "foobar", 99999));

                shoppingSheet = await BuildShoppingSheetAsync(studentAwardYear);

                Assert.AreEqual(fafsaEfc, shoppingSheet.FamilyContribution.Value);
            }

            [TestMethod]
            public async Task NoShoppingSheetRuleTableDoesNotSetCustomMessagesTest()
            {
                budgetComponents.Add(new BudgetComponent(studentAwardYear.Code, "foobar", "description"));
                studentBudgetComponents.Add(new StudentBudgetComponent(studentAwardYear.Code, studentAwardYear.StudentId, "foobar", 99999));

                shoppingSheetRuleTables = new List<ShoppingSheetRuleTable>();
                shoppingSheet = await BuildShoppingSheetAsync(studentAwardYear);

                Assert.AreEqual(0, shoppingSheet.CustomMessages.Count());
            }

            [TestMethod]
            public async Task ShoppingSheetRuleTableSetsCustomMessagesTest()
            {
                budgetComponents.Add(new BudgetComponent(studentAwardYear.Code, "foobar", "description"));
                studentBudgetComponents.Add(new StudentBudgetComponent(studentAwardYear.Code, studentAwardYear.StudentId, "foobar", 99999));

                var shoppingSheetRuleTableCode = "FOO";
                var defaultMessage = "This is the defaultResult";
                var message1 = "Current Office Is MAIN";
                var message2 = "You have no ISIR";
                studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.CustomMessageRuleTableId = shoppingSheetRuleTableCode;
                var ruleTable = new ShoppingSheetRuleTable(shoppingSheetRuleTableCode, studentAwardYear.Code, defaultMessage)
                    {
                        AlwaysUseDefault = true,
                        RuleProcessor = new Func<IEnumerable<RuleRequest<StudentAwardYear>>, Task<IEnumerable<RuleResult>>>(
                            (ruleRequests) =>
                                Task.FromResult(ruleRequests.Select(req => new RuleResult() { RuleId = req.Rule.Id, Context = req.Context, Passed = req.Rule.Passes(req.Context) }))
                        ),
                    };
                ruleTable.AddRuleResultPair("rule1", message1);
                ruleTable.AddRuleResultPair("rule2", message2);
                ruleTable.LinkRuleObjects(new Rule<StudentAwardYear>("rule1", (year) => year.CurrentOffice.Id == "MAIN"));
                ruleTable.LinkRuleObjects(new Rule<StudentAwardYear>("rule2", (year) => string.IsNullOrEmpty(year.FederallyFlaggedIsirId)));

                shoppingSheetRuleTables.Add(ruleTable);

                shoppingSheet = await BuildShoppingSheetAsync(studentAwardYear);

                Assert.AreEqual(3, shoppingSheet.CustomMessages.Count());
                Assert.AreEqual(defaultMessage, shoppingSheet.CustomMessages[0]);
                Assert.AreEqual(message1, shoppingSheet.CustomMessages[1]);
                Assert.AreEqual(message2, shoppingSheet.CustomMessages[2]);
            }

            [TestMethod]
            public async Task ShoppingSheetRuleTableGetsAllCustomMessagesTest()
            {
                budgetComponents.Add(new BudgetComponent(studentAwardYear.Code, "foobar", "description"));
                studentBudgetComponents.Add(new StudentBudgetComponent(studentAwardYear.Code, studentAwardYear.StudentId, "foobar", 99999));

                var shoppingSheetRuleTableCode = "FOO";
                var defaultMessage = "This is the defaultResult";
                var message1 = "Current Office Is MAIN";
                studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.CustomMessageRuleTableId = shoppingSheetRuleTableCode;
                var ruleTable = new ShoppingSheetRuleTable(shoppingSheetRuleTableCode, studentAwardYear.Code, defaultMessage)
                {
                    AlwaysUseDefault = false,
                    RuleProcessor = new Func<IEnumerable<RuleRequest<StudentAwardYear>>, Task<IEnumerable<RuleResult>>>(
                        (ruleRequests) =>
                            Task.FromResult(ruleRequests.Select(req => new RuleResult() { RuleId = req.Rule.Id, Context = req.Context, Passed = req.Rule.Passes(req.Context) }))
                    ),
                };
                
                //Added 8 rule-result pairs. all will pass
                ruleTable.AddRuleResultPair("rule1", message1);
                ruleTable.AddRuleResultPair("rule1", message1);
                ruleTable.AddRuleResultPair("rule1", message1);
                ruleTable.AddRuleResultPair("rule1", message1);
                ruleTable.AddRuleResultPair("rule1", message1);
                ruleTable.AddRuleResultPair("rule1", message1);
                ruleTable.AddRuleResultPair("rule1", message1);
                ruleTable.AddRuleResultPair("rule1", message1);
                ruleTable.LinkRuleObjects(new Rule<StudentAwardYear>("rule1", (year) => year.CurrentOffice.Id == "MAIN"));

                shoppingSheetRuleTables.Add(ruleTable);

                shoppingSheet = await BuildShoppingSheetAsync(studentAwardYear);

                Assert.AreEqual(8, shoppingSheet.CustomMessages.Count());
                Assert.IsTrue(shoppingSheet.CustomMessages.All(msg => msg == message1));
            }
        }
    }
}
