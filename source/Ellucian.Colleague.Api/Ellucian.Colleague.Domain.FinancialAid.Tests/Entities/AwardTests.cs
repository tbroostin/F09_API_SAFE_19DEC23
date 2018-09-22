/*Copyright 2014-2018 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using System.Reflection;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests.Entities
{
    /// <summary>
    /// Tests the Award domain object - extending CodeItem
    /// </summary>
    [TestClass]
    public class AwardTests
    {
        /// <summary>
        /// Tests the constructor of the Award class
        /// </summary>
        [TestClass]
        public class AwardConstructor
        {

            private string code;
            private string description;
            private string explanation;
            private AwardCategory category;
            private bool isFederalDirectLoan;
            private ShoppingSheetAwardGroup? shoppingSheetGroup;

            private Award award;

            [TestInitialize]
            public void Initialize()
            {
                code = "WOOFY";
                description = "Woofy Veternarian Loan";
                explanation = "Explanation of Woofy";
                category = new AwardCategory("GSL", "description", AwardCategoryType.Loan);
                isFederalDirectLoan = true;
                shoppingSheetGroup = ShoppingSheetAwardGroup.OtherGrants;
                award = new Award(code, description, category, explanation);
            }

            [TestMethod]
            public void NumberOfPropertiesTest()
            {
                var awardProperties = typeof(Award).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                Assert.AreEqual(10, awardProperties.Count());
            }


            [TestMethod]
            public void AttributesAreEqualTest()
            {
                Assert.AreEqual(code, award.Code);
                Assert.AreEqual(description, award.Description);
                Assert.AreEqual(explanation, award.Explanation);
                Assert.AreEqual(category, award.AwardCategory);
                Assert.IsFalse(award.IsFederalDirectLoan);
                Assert.IsNull(award.ShoppingSheetGroup);
            }


            /// <summary>
            /// Tests that the explanation argument of the constructor is not required
            /// and that the resulting explanation is empty.
            /// </summary>
            [TestMethod]
            public void AwardExplanationNotRequired()
            {
                award = new Award(code, description, category);
                Assert.AreEqual(string.Empty, award.Explanation);
            }

            /// <summary>
            /// Test that a NullException is thrown by the constructor when a null code is passed in
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AwardCodeNullException()
            {
                new Award(null, description, category);
            }

            /// <summary>
            /// Test that a NullException is thrown when a null description is passed in
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AwardDescNullException()
            {
                new Award(code, null, category);
            }

            /// <summary>
            /// Test that the IsFederalDirectLoan attribute is intitialized to false;
            /// </summary>
            [TestMethod]
            public void IsFederalDirectLoan_InitTest()
            {
                Assert.IsFalse(award.IsFederalDirectLoan);
            }

            [TestMethod]
            public void IsFederalDirectLoan_GetSetTest()
            {
                award.IsFederalDirectLoan = isFederalDirectLoan;
                Assert.AreEqual(isFederalDirectLoan, award.IsFederalDirectLoan);
            }

            [TestMethod]
            public void LoanTypeDerivedFromAwardCategoryTest()
            {
                Assert.AreEqual(category.CategoryLoanType.Value, award.LoanType.Value);
            }

            [TestMethod]
            public void ShoppingSheetGroup_GetSetTest()
            {
                award.ShoppingSheetGroup = shoppingSheetGroup;
                Assert.IsNotNull(award.ShoppingSheetGroup);
                Assert.AreEqual(shoppingSheetGroup.Value, award.ShoppingSheetGroup.Value);
            }

            [TestMethod]
            public void IsTitleIV_ReturnsTrueTest()
            {
                category = new AwardCategory("GSL", "description", AwardCategoryType.Loan);
                award = new Award(code, description, category);
                Assert.IsTrue(award.IsTitleIV);
            }

            [TestMethod]
            public void IsTitleIV_NonTIVCategory_ReturnsFalseTest()
            {
                category = new AwardCategory("Foo", "description", AwardCategoryType.Loan);
                award = new Award(code, description, category);
                Assert.IsFalse(award.IsTitleIV);
            }

            [TestMethod]
            public void IsTitleIV_NoCategory_ReturnsFalseTest()
            {
                award = new Award(code, description, null);
                Assert.IsFalse(award.IsTitleIV);
            }
            
        }

        /// <summary>
        /// Class to test the equals method of Award (inherited from CodeItem)
        /// </summary>
        [TestClass]
        public class AwardEquals
        {
            /// <summary>
            /// Award Code 1
            /// </summary>
            private string code1;

            /// <summary>
            /// Award Description 1
            /// </summary>
            private string desc1;

            /// <summary>
            /// Award Code 2
            /// </summary>
            private string code2;

            /// <summary>
            /// Award Description 2
            /// </summary>
            private string desc2;

            /// <summary>
            /// Test Award 1 
            /// </summary>
            private Award award1;

            /// <summary>
            /// Test Award 2 has the same code as but a different description than award 1
            /// </summary>
            private Award award2;

            /// <summary>
            /// Test Award 3 has a different code than but same description as award 1
            /// </summary>
            private Award award3;

            private AwardCategory category;

            /// <summary>
            /// Initialize the the default code and description, and instantiate the three
            /// test awards
            /// </summary>
            [TestInitialize]
            public void Initialize()
            {
                code1 = "WOOFY";
                desc1 = "Woofy Veternarian Scholarship";

                code2 = "PELL";
                desc2 = "Pell Grant";

                category = new AwardCategory("foo", "bar", null);

                award1 = new Award(code1, desc1, category);
                award2 = new Award(code1, desc2, category);
                award3 = new Award(code2, desc1, category);
            }

            /// <summary>
            /// Verifies the Equals method returns true when comparing two awards with the same code
            /// </summary>
            [TestMethod]
            public void AwardSameCodesEqual()
            {
                Assert.IsTrue(award1.Equals(award2));
            }

            /// <summary>
            /// Verifies the Equals method returns false when comparing two awards with different codes
            /// </summary>
            [TestMethod]
            public void AwardDifferentCodeNotEqual()
            {
                Assert.IsFalse(award1.Equals(award3));
            }
        }
    }
}
