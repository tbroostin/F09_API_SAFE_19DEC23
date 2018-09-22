/*Copyright 2014-2017 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests.Entities
{
    [TestClass]
    public class ShoppingSheetConfigurationTests
    {
        [TestClass]
        public class ShoppingSheetConfigurationConstructorTests
        {
            private string customMessageRuleTableId;
            private ShoppingSheetOfficeType OfficeType;
            private decimal? GraduationRate;
            private decimal? LoanDefaultRate;
            private decimal? NationalLoanDefaultRate;
            private int? MedianBorrowingAmount;
            private decimal? MedianMonthlyPaymentAmount;
            private decimal? LowToMediumBoundary;
            private decimal? MediumToHighBoundary;
            private ShoppingSheetEfcOption EfcOption;
            private decimal? InstitutionRepaymentRate;
            private decimal? NationalRepaymentRateAverage;


            public ShoppingSheetConfiguration configuration;

            [TestInitialize]
            public void Initialize()
            {
                customMessageRuleTableId = "CUSTOM";
                OfficeType = ShoppingSheetOfficeType.AssociateDegreeGranting;
                GraduationRate = (decimal)23.2;
                LoanDefaultRate = 8;
                NationalLoanDefaultRate = (decimal)12.2;
                MedianBorrowingAmount = 96000;
                MedianMonthlyPaymentAmount = (decimal)1200.3;
                LowToMediumBoundary = (decimal)42.1;
                MediumToHighBoundary = (decimal)65.7;
                EfcOption = ShoppingSheetEfcOption.ProfileEfcUntilIsirExists;
                InstitutionRepaymentRate = 45.5m;
                NationalRepaymentRateAverage = 56.8m;

                configuration = new ShoppingSheetConfiguration();
            }

            [TestMethod]
            public void CustomMessageRuleTableIdGetSetTest()
            {
                configuration.CustomMessageRuleTableId = customMessageRuleTableId;
                Assert.AreEqual(customMessageRuleTableId, configuration.CustomMessageRuleTableId);
            }

            [TestMethod]
            public void OfficeTypeGetSetTest()
            {
                //default is bachelor degree granting
                Assert.AreEqual(ShoppingSheetOfficeType.BachelorDegreeGranting, configuration.OfficeType);

                configuration.OfficeType = OfficeType;
                Assert.AreEqual(OfficeType, configuration.OfficeType);
            }

            [TestMethod]
            public void GraduationRateGetSetTest()
            {
                configuration.GraduationRate = GraduationRate;
                Assert.AreEqual(GraduationRate, configuration.GraduationRate);

                configuration.GraduationRate = null;
                Assert.IsNull(configuration.GraduationRate);
            }

            [TestMethod]
            public void LoanDefaultRateGetSetTest()
            {
                configuration.LoanDefaultRate = LoanDefaultRate;
                Assert.AreEqual(LoanDefaultRate, configuration.LoanDefaultRate);

                configuration.LoanDefaultRate = null;
                Assert.IsNull(configuration.LoanDefaultRate);
            }

            [TestMethod]
            public void NationalLoanDefaultRateGetSetTest()
            {
                configuration.NationalLoanDefaultRate = NationalLoanDefaultRate;
                Assert.AreEqual(NationalLoanDefaultRate, configuration.NationalLoanDefaultRate);

                configuration.NationalLoanDefaultRate = null;
                Assert.IsNull(configuration.NationalLoanDefaultRate);
            }

            [TestMethod]
            public void MedianBorrowingAmountGetSetTest()
            {
                configuration.MedianBorrowingAmount = MedianBorrowingAmount;
                Assert.AreEqual(MedianBorrowingAmount, configuration.MedianBorrowingAmount);

                configuration.MedianBorrowingAmount = null;
                Assert.IsNull(configuration.MedianBorrowingAmount);
            }

            [TestMethod]
            public void MedianMonthlyPaymentAmountGetSetTest()
            {
                configuration.MedianMonthlyPaymentAmount = MedianMonthlyPaymentAmount;
                Assert.AreEqual(MedianMonthlyPaymentAmount, configuration.MedianMonthlyPaymentAmount);

                configuration.MedianMonthlyPaymentAmount = null;
                Assert.IsNull(configuration.MedianMonthlyPaymentAmount);
            }

            [TestMethod]
            public void EfcOptionGetSetTest()
            {
                //default is IsirEfc option
                Assert.AreEqual(ShoppingSheetEfcOption.IsirEfc, configuration.EfcOption);

                configuration.EfcOption = EfcOption;
                Assert.AreEqual(EfcOption, configuration.EfcOption);
            }

            [TestMethod]
            public void LowToMediumSetTest()
            {
                configuration.LowToMediumBoundary = LowToMediumBoundary;
                Assert.AreEqual(LowToMediumBoundary, configuration.LowToMediumBoundary);

                configuration.LowToMediumBoundary = null;
                Assert.IsNull(configuration.LowToMediumBoundary);
            }

            [TestMethod]
            public void MediumToHighGetSetTest()
            {
                configuration.MediumToHighBoundary = MediumToHighBoundary;
                Assert.AreEqual(MediumToHighBoundary, configuration.MediumToHighBoundary);

                configuration.MediumToHighBoundary = null;
                Assert.IsNull(configuration.MediumToHighBoundary);
            }

            [TestMethod]
            public void InstitutionRepaymentRateGetSetTest()
            {
                configuration.InstitutionRepaymentRate = InstitutionRepaymentRate;
                Assert.AreEqual(InstitutionRepaymentRate, configuration.InstitutionRepaymentRate);

                configuration.InstitutionRepaymentRate = null;
                Assert.IsNull(configuration.InstitutionRepaymentRate);
            }

            [TestMethod]
            public void NationalRepaymentRateGetSetTest()
            {
                configuration.NationalRepaymentRateAverage = NationalRepaymentRateAverage;
                Assert.AreEqual(NationalRepaymentRateAverage, configuration.NationalRepaymentRateAverage);

                configuration.NationalRepaymentRateAverage = null;
                Assert.IsNull(configuration.NationalRepaymentRateAverage);
            }
        }
    }
}
