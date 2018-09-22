/*Copyright 2018 Ellucian Company L.P. and its affiliates*/
using Ellucian.Colleague.Domain.Finance.Entities.AccountActivity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.Finance.Tests.Entities
{
    [TestClass]
    public class StudentAwardDisbursementTests
    {
        private string awardPeriodCode;
        private DateTime? anticipatedDisbursementDate;
        private decimal? lastTransmitAmount;
        private DateTime? lastTransmitDate;

        private StudentAwardDisbursement disbursement;

        [TestInitialize]
        public void Initialize()
        {
            awardPeriodCode = "15/FA";
            anticipatedDisbursementDate = DateTime.Today;
            lastTransmitAmount = 2345;
            lastTransmitDate = DateTime.Today;
            disbursement = new StudentAwardDisbursement(awardPeriodCode, anticipatedDisbursementDate, lastTransmitAmount, lastTransmitDate);
        }

        [TestMethod]
        public void ObjectCreatedTest()
        {
            Assert.IsNotNull(disbursement);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullPeriodCode_ArgumentNullExceptionThrownTest()
        {
            new StudentAwardDisbursement(null, anticipatedDisbursementDate, lastTransmitAmount, lastTransmitDate);
        }

        [TestMethod]
        public void AwardPeriodCodeProperty_EqualsExpectedTest()
        {
            Assert.AreEqual(awardPeriodCode, disbursement.AwardPeriodCode);
        }

        [TestMethod]
        public void AnticipatedisbursementDateProperty_EqualsExpectedTest()
        {
            Assert.AreEqual(anticipatedDisbursementDate, disbursement.AnticipatedDisbursementDate);
        }

        [TestMethod]
        public void LastTransmitAmountProperty_EqualsExpectedTest()
        {
            Assert.AreEqual(lastTransmitAmount, disbursement.LastTransmitAmount);
        }

        [TestMethod]
        public void LastTransmitDateProperty_EqualsExpectedTest()
        {
            Assert.AreEqual(lastTransmitDate, disbursement.LastTransmitDate);
        }
    }
}
