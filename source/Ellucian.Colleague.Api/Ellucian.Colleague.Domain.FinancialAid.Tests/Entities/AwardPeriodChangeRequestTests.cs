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
    public class AwardPeriodChangeRequestTests
    {
        public string awardPeriodId;
        public string newAwardStatusId;
        public decimal? newAmount;
        public AwardPackageChangeRequestStatus status;
        public string statusReason;

        public AwardPeriodChangeRequest awardPeriodChangeRequest;

        public void AwardPeriodChangeRequestTestInitialize()
        {
            awardPeriodId = "15/FA";
            newAwardStatusId = "A";
            newAmount = (decimal)5555.55;
            status = AwardPackageChangeRequestStatus.Pending;
            statusReason = "Pending Status Reason";

            awardPeriodChangeRequest = new AwardPeriodChangeRequest(awardPeriodId);
        }

        [TestClass]
        public class AwardPeriodChangeRequestConstructorTests : AwardPeriodChangeRequestTests
        {
            [TestInitialize]
            public void Initialize()
            {
                AwardPeriodChangeRequestTestInitialize();
            }

            [TestMethod]
            public void AwardPeriodIdTest()
            {
                Assert.AreEqual(awardPeriodId, awardPeriodChangeRequest.AwardPeriodId);
            }
            
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AwardPeriodIdRequiredTest()
            {
                new AwardPeriodChangeRequest(string.Empty);
            }

            [TestMethod]
            public void NewAwardStatusIdGetSetTest()
            {
                awardPeriodChangeRequest.NewAwardStatusId = newAwardStatusId;
                Assert.AreEqual(newAwardStatusId, awardPeriodChangeRequest.NewAwardStatusId);
            }

            [TestMethod]
            public void NewAmountGetSetTest()
            {
                awardPeriodChangeRequest.NewAmount = newAmount;
                Assert.AreEqual(newAmount, awardPeriodChangeRequest.NewAmount);
            }

            [TestMethod]
            public void StatusGetSetTest()
            {
                awardPeriodChangeRequest.Status = status;
                Assert.AreEqual(status, awardPeriodChangeRequest.Status);
            }

            [TestMethod]
            public void StatusReasonGetSetTest()
            {
                awardPeriodChangeRequest.StatusReason = statusReason;
                Assert.AreEqual(statusReason, awardPeriodChangeRequest.StatusReason);
            }
        }
    }
}
