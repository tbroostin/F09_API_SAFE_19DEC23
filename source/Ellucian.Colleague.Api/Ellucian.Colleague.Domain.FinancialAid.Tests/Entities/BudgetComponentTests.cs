using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests.Entities
{
    [TestClass]
    public class BudgetComponentTests
    {
        public string AwardYear;
        public string Code;
        public string Description;
        public ShoppingSheetBudgetGroup? ShoppingSheetGroup;

        public BudgetComponent budgetComponent;

        [TestClass]
        public class BudgetComponentConstructorTests : BudgetComponentTests
        {
            [TestInitialize]
            public void Initialize()
            {
                AwardYear = "2014";
                Code = "BUDGET";
                Description = "Budget Description";
                ShoppingSheetGroup = ShoppingSheetBudgetGroup.OtherCosts;
            }

            [TestMethod]
            public void AwardYearEqualsTest()
            {
                budgetComponent = new BudgetComponent(AwardYear, Code, Description);
                Assert.AreEqual(AwardYear, budgetComponent.AwardYear);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AwardYearRequiredTest()
            {
                new BudgetComponent("", Code, Description);
            }

            [TestMethod]
            public void CodeEqualsTest()
            {
                budgetComponent = new BudgetComponent(AwardYear, Code, Description);
                Assert.AreEqual(Code, budgetComponent.Code);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CodeRequiredTest()
            {
                new BudgetComponent(AwardYear, "", Description);
            }

            [TestMethod]
            public void DescriptionEqualsTest()
            {
                budgetComponent = new BudgetComponent(AwardYear, Code, Description);
                Assert.AreEqual(Description, budgetComponent.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void DescriptionRequiredTest()
            {
                new BudgetComponent(AwardYear, Code, "");
            }

            [TestMethod]
            public void ShoppingSheetGroupGetSetTest()
            {
                budgetComponent = new BudgetComponent(AwardYear, Code, Description)
                    {
                        ShoppingSheetGroup = ShoppingSheetGroup
                    };
                Assert.AreEqual(ShoppingSheetGroup, budgetComponent.ShoppingSheetGroup);
            }

            [TestMethod]
            public void ShoppingSheetGroupNullableTest()
            {
                budgetComponent = new BudgetComponent(AwardYear, Code, Description);
                Assert.AreEqual(null, budgetComponent.ShoppingSheetGroup);
            }
        }

        [TestClass]
        public class BudgetComponentEqualsAndHashCodeTests : BudgetComponentTests
        {
            public BudgetComponent testBudgetComponent;

            [TestInitialize]
            public void Initialize()
            {
                AwardYear = "2014";
                Code = "BUDGET";
                Description = "Budget Description";
                ShoppingSheetGroup = ShoppingSheetBudgetGroup.OtherCosts;

                testBudgetComponent = new BudgetComponent(AwardYear, Code, Description)
                {
                    ShoppingSheetGroup = ShoppingSheetGroup
                };
            }

            [TestMethod]
            public void BudgetsAreEqualTest()
            {
                budgetComponent = new BudgetComponent(AwardYear, Code, Description)
                    {
                        ShoppingSheetGroup = ShoppingSheetGroup
                    };

                Assert.AreEqual(testBudgetComponent, budgetComponent);
            }

            [TestMethod]
            public void BudgetsHaveSameHashCodeTest()
            {
                budgetComponent = new BudgetComponent(AwardYear, Code, Description)
                {
                    ShoppingSheetGroup = ShoppingSheetGroup
                };

                Assert.AreEqual(testBudgetComponent.GetHashCode(), budgetComponent.GetHashCode());
            }

            [TestMethod]
            public void BudgetsAreEqualWithDifferentDescriptionsTest()
            {
                budgetComponent = new BudgetComponent(AwardYear, Code, "foobar")
                {
                    ShoppingSheetGroup = ShoppingSheetGroup
                };

                Assert.AreEqual(testBudgetComponent, budgetComponent);
            }

            [TestMethod]
            public void BudGetHashCodesAreEqualWithDifferentDescriptionsTest()
            {
                budgetComponent = new BudgetComponent(AwardYear, Code, "foobar")
                {
                    ShoppingSheetGroup = ShoppingSheetGroup
                };

                Assert.AreEqual(testBudgetComponent.GetHashCode(), budgetComponent.GetHashCode());
            }

            [TestMethod]
            public void BudgetsAreEqualWithDifferentShoppingSheetGroupsTest()
            {
                budgetComponent = new BudgetComponent(AwardYear, Code, Description)
                {
                    ShoppingSheetGroup = null
                };

                Assert.AreEqual(testBudgetComponent, budgetComponent);
            }

            [TestMethod]
            public void BudGetHashCodesAreEqualWithDifferentShoppingSheetGroupsTest()
            {
                budgetComponent = new BudgetComponent(AwardYear, Code, Description)
                {
                    ShoppingSheetGroup = null
                };

                Assert.AreEqual(testBudgetComponent.GetHashCode(), budgetComponent.GetHashCode());
            }

            [TestMethod]
            public void BudgetsAreNotEqualWithDifferentAwardYearsTest()
            {
                budgetComponent = new BudgetComponent("foobar", Code, Description)
                {
                    ShoppingSheetGroup = ShoppingSheetGroup
                };

                Assert.AreNotEqual(testBudgetComponent, budgetComponent);
            }

            [TestMethod]
            public void BudGetHashCodesAreNotEqualWithDifferentAwardYearsTest()
            {
                budgetComponent = new BudgetComponent("foobar", Code, Description)
                {
                    ShoppingSheetGroup = ShoppingSheetGroup
                };

                Assert.AreNotEqual(testBudgetComponent.GetHashCode(), budgetComponent.GetHashCode());
            }

            [TestMethod]
            public void BudgetsAreNotEqualWithDifferentCodesTest()
            {
                budgetComponent = new BudgetComponent(AwardYear, "foobar", Description)
                {
                    ShoppingSheetGroup = ShoppingSheetGroup
                };

                Assert.AreNotEqual(testBudgetComponent, budgetComponent);
            }

            [TestMethod]
            public void BudGetHashCodesAreNotEqualWithDifferentCodesTest()
            {
                budgetComponent = new BudgetComponent(AwardYear, "foobar", Description)
                {
                    ShoppingSheetGroup = ShoppingSheetGroup
                };

                Assert.AreNotEqual(testBudgetComponent.GetHashCode(), budgetComponent.GetHashCode());
            }
        }


    }
}
