using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests.Entities
{
    [TestClass]
    public class AwardCategoryTests
    {
        public string code;
        public string description;
        public AwardCategoryType? awardCategoryType;

        public AwardCategory awardCategory;

        public void BaseInitialize()
        {
            code = "FOO";
            description = "bar";
            awardCategoryType = AwardCategoryType.Scholarship;

            awardCategory = new AwardCategory(code, description, awardCategoryType);
        }

        [TestClass]
        public class AwardCategoryConstructorTests : AwardCategoryTests
        {

            [TestInitialize]
            public void Initialize()
            {
                BaseInitialize();
            }

            [TestMethod]
            public void AttributesAreEqual()
            {
                Assert.AreEqual(code, awardCategory.Code);
                Assert.AreEqual(description, awardCategory.Description);
                Assert.AreEqual(awardCategoryType.Value, awardCategory.AwardCategoryType.Value);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CodeRequiredTest()
            {
                new AwardCategory(null, description, awardCategoryType);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void DescriptionRequiredTest()
            {
                new AwardCategory(code, null, awardCategoryType);
            }
        }

        [TestClass]
        public class CategoryLoanTypeTests : AwardCategoryTests
        {
            [TestInitialize]
            public void Initialize()
            {
                BaseInitialize();
            }

            [TestMethod]
            public void NonLoanCategoryType_NullLoanTypeTest()
            {
                Assert.IsNull(awardCategory.CategoryLoanType);
            }

            [TestMethod]
            public void NullCategoryType_NullLoanTypeTest()
            {
                awardCategory = new AwardCategory(code, description, null);
                Assert.IsNull(awardCategory.AwardCategoryType);
                Assert.IsNull(awardCategory.CategoryLoanType);
            }

            [TestMethod]
            public void LoanAwardCategoryType_NonNullLoanTypeTest()
            {
                awardCategory = new AwardCategory(code, description, AwardCategoryType.Loan);
                Assert.IsNotNull(awardCategory.CategoryLoanType);
            }

            [TestMethod]
            public void SubsidizedAwardCategory_SubLoanTypeTest()
            {
                awardCategory = new AwardCategory("GSL", description, AwardCategoryType.Loan);
                Assert.AreEqual(LoanType.SubsidizedLoan, awardCategory.CategoryLoanType.Value);
            }

            [TestMethod]
            public void UnsubsidizedAwardCategory_UnsubLoanTypeTest()
            {
                awardCategory = new AwardCategory("USTF", description, AwardCategoryType.Loan);
                Assert.AreEqual(LoanType.UnsubsidizedLoan, awardCategory.CategoryLoanType.Value);
            }

            [TestMethod]
            public void GradPlusAwardCategory_GplusLoanTypeTest()
            {
                awardCategory = new AwardCategory("GPLUS", description, AwardCategoryType.Loan);
                Assert.AreEqual(LoanType.GraduatePlusLoan, awardCategory.CategoryLoanType.Value);
            }

            [TestMethod]
            public void OtherLoanAwardCategory_OtherLoanTypeTest()
            {
                awardCategory = new AwardCategory("OTHER", description, AwardCategoryType.Loan);
                Assert.AreEqual(LoanType.OtherLoan, awardCategory.CategoryLoanType.Value);
            }

            [TestMethod]
            public void NonUpperCaseCodeIsValidTest()
            {
                awardCategory = new AwardCategory("gsl", description, AwardCategoryType.Loan);
                Assert.AreEqual(LoanType.SubsidizedLoan, awardCategory.CategoryLoanType.Value);
            }

        }

        [TestClass]
        public class EqualsTests : AwardCategoryTests
        {
            [TestInitialize]
            public void Initialize()
            {
                BaseInitialize();
            }

            [TestMethod]
            public void SameCode_EqualTest()
            {
                var testCategory = new AwardCategory(code, description, awardCategoryType);
                Assert.AreEqual(testCategory, awardCategory);
            }

            [TestMethod]
            public void DiffCode_NotEqualTest()
            {
                var testCategory = new AwardCategory("OTHER", description, awardCategoryType);
                Assert.AreNotEqual(testCategory, awardCategory);
            }

            [TestMethod]
            public void SameCode_EqualIgnoreCaseTest()
            {
                var testCategory = new AwardCategory(code.ToLower(), description, awardCategoryType);
                Assert.AreEqual(testCategory, awardCategory);
            }

            [TestMethod]
            public void DiffDescription_EqualTest()
            {
                var testCategory = new AwardCategory(code, "whatever", awardCategoryType);
                Assert.AreEqual(testCategory, awardCategory);
            }

            [TestMethod]
            public void DiffCategoryType_EqualTest()
            {
                var testCategory = new AwardCategory(code, description, null);
                Assert.AreEqual(testCategory, awardCategory);
            }

        }
    }
}
