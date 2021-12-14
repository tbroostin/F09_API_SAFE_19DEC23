/* Copyright 2017-2021 Ellucian Company L.P. and its affiliates */
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;


namespace Ellucian.Colleague.Domain.HumanResources.Tests.Entities
{
    [TestClass]
    public class PayrollRegisterEarningsEntryTests
    {
        public string earnTypeId;
        public string stipendId;

        public decimal totalPeriodAmount;
        public decimal basePeriodAmount;
        public decimal earningsFactorPeriodAmount;
        public decimal? units;
        public decimal rate;       


        public string earnDiffId;
        
        public decimal diffPeriodAmount;
        public decimal diffRate;
        public decimal? diffUnits;
        public bool isAdj;
        
        public PayrollRegisterEarningsEntry standardEarnings
        {
            get
            {
                return new PayrollRegisterEarningsEntry(earnTypeId, totalPeriodAmount, basePeriodAmount, 
                    earningsFactorPeriodAmount, units, rate, HourlySalaryIndicator.Hourly, isAdj);
            }
        }

        public PayrollRegisterEarningsEntry stipendEarnings
        {
            get
            {
                return new PayrollRegisterEarningsEntry(earnTypeId, stipendId, totalPeriodAmount, basePeriodAmount, 
                    earningsFactorPeriodAmount, units, rate, HourlySalaryIndicator.Salary, isAdj);
            }
            
        }

        [TestInitialize]
        public void Initialize()
        {
            earnTypeId = "yob";
            stipendId = "3353";
            totalPeriodAmount = 12.1m;
            basePeriodAmount = 10;
            earningsFactorPeriodAmount = 2.1m;
            units = 7m;
            rate = 1.73m;

            earnDiffId = "aito";
            diffPeriodAmount = 12.4m;
            diffRate = 5m;
            diffUnits = 2.48m;

        }

        [TestMethod]
        public void PropertiesAreSetInConstructor()
        {            
            Assert.AreEqual(earnTypeId, standardEarnings.EarningsTypeId);
            Assert.AreEqual(totalPeriodAmount, standardEarnings.TotalPeriodEarningsAmount);
            Assert.AreEqual(basePeriodAmount, standardEarnings.BasePeriodEarningsAmount);
            Assert.AreEqual(earningsFactorPeriodAmount, standardEarnings.EarningsFactorPeriodAmount);
            Assert.AreEqual(units, standardEarnings.StandardUnitsWorked);
            Assert.AreEqual(rate, standardEarnings.StandardRate);
            Assert.AreEqual(0, standardEarnings.EarningsAdjustmentAmount);
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void NullEarnTypeExceptionStandard()
        {
            earnTypeId = null;
            var error = standardEarnings;
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void NullEarnTypeExceptionStipend()
        {
            earnTypeId = null;
            var error = stipendEarnings;
        }

        [TestMethod]
        public void EarningsDifferentialPropertiesAreSet()
        {
            var earn = standardEarnings;
            earn.SetEarningsDifferential(earnDiffId, diffPeriodAmount, diffUnits, diffRate);
            Assert.AreEqual(earnDiffId, earn.EarningsDifferentialId);
            Assert.AreEqual(diffPeriodAmount, earn.DifferentialPeriodEarningsAmount);
            Assert.AreEqual(diffUnits, earn.DifferentialUnitsWorked);
            Assert.AreEqual(diffRate, earn.DifferentialRate);
        }

        [TestMethod]
        public void HasEarningsDifferentialTest()
        {
            var earn = standardEarnings;
            Assert.IsFalse(earn.HasDifferentialEarnings);
            earn.SetEarningsDifferential(earnDiffId, diffPeriodAmount, diffUnits, diffRate);
            Assert.IsTrue(earn.HasDifferentialEarnings);
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void EarningsDifferentialNullId()
        {
            //standardEarnings = new PayrollRegisterEarningsEntry(earnTypeId, periodEarnAmount, standUnitsWorked, standRate, HourlySalaryIndicator.Hourly);
            standardEarnings.SetEarningsDifferential(null, diffPeriodAmount, diffUnits, diffRate);
        }

        [TestMethod]
        public void StipendConstructorTests()
        {            
            Assert.AreEqual(earnTypeId, stipendEarnings.EarningsTypeId);
            Assert.AreEqual(stipendId, stipendEarnings.StipendId);
            Assert.AreEqual(totalPeriodAmount, stipendEarnings.TotalPeriodEarningsAmount);
            Assert.AreEqual(basePeriodAmount, stipendEarnings.BasePeriodEarningsAmount);
            Assert.AreEqual(earningsFactorPeriodAmount, stipendEarnings.EarningsFactorPeriodAmount);
            Assert.AreEqual(units, stipendEarnings.StandardUnitsWorked);
            Assert.AreEqual(rate, stipendEarnings.StandardRate);
            Assert.AreEqual(HourlySalaryIndicator.Salary, stipendEarnings.HourlySalaryIndication);
            Assert.AreEqual(0, stipendEarnings.EarningsAdjustmentAmount);
        }

        [TestMethod]
        public void IsStipendEarningsTest()
        {
            Assert.IsTrue(stipendEarnings.IsStipendEarnings);
            Assert.IsFalse(standardEarnings.IsStipendEarnings);
        }

        [TestMethod]
        public void AdjustmentRecord_CorrectAmountIsBeingSetTest()
        {
            isAdj = true;
            Assert.AreEqual(totalPeriodAmount, standardEarnings.EarningsAdjustmentAmount);
            Assert.IsTrue(standardEarnings.TotalPeriodEarningsAmount == 0);
            Assert.AreEqual(totalPeriodAmount, stipendEarnings.EarningsAdjustmentAmount);
            Assert.IsTrue(stipendEarnings.TotalPeriodEarningsAmount == 0);
        }
    }
}
