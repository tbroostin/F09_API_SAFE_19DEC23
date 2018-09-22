/*Copyright 2018 Ellucian Company L.P. and its affiliates*/
using Ellucian.Colleague.Domain.Finance.Entities.AccountActivity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.Finance.Tests.Entities
{
    [TestClass]
    public class StudentAwardDisbursementInfoTests
    {
        private string studentId;
        private string awardCode;
        private string awardDescription;
        private string awardYearCode;
        private List<StudentAwardDisbursement> disbursements;

        private StudentAwardDisbursementInfo disbInfo;

        [TestInitialize]
        public void TestInitialize()
        {
            studentId = "0004791";
            awardCode = "TEACH";
            awardDescription = "Teach award";
            awardYearCode = "2018";

            disbursements = new List<StudentAwardDisbursement>()
            {
                new StudentAwardDisbursement("19/SP", new DateTime(2018, 01, 30), 765, new DateTime(2018, 01, 29)),
                new StudentAwardDisbursement("18/FA", new DateTime(2017, 09, 30), 765, new DateTime(2017, 09, 30))
            };

            disbInfo = new StudentAwardDisbursementInfo(studentId, awardCode, awardYearCode);
        }

        [TestMethod]
        public void ObjectCreatedTest()
        {
            Assert.IsNotNull(disbInfo);
        }

        [TestMethod]
        public void StudentIdProperty_EqualsExpectedTest()
        {
            Assert.AreEqual(studentId, disbInfo.StudentId);
        }

        [TestMethod]
        public void AwardCodeProperty_EqualsExpectedTest()
        {
            Assert.AreEqual(awardCode, disbInfo.AwardCode);
        }

        [TestMethod]
        public void AwardYearCodeProperty_EqualsExpectedTest()
        {
            Assert.AreEqual(awardYearCode, disbInfo.AwardYearCode);
        }

        [TestMethod]
        public void DisbursementsList_InitializesToEmptyListTest()
        {
            Assert.IsTrue(disbInfo.AwardDisbursements != null && !disbInfo.AwardDisbursements.Any());
        }

        [TestMethod]
        public void AwardDescriptionProperty_GetSetTest()
        {
            disbInfo.AwardDescription = awardDescription;
            Assert.AreEqual(awardDescription, disbInfo.AwardDescription);
        }

        [TestMethod]
        public void DisbursementList_EqualsExpectedTest()
        {
            foreach(var disb in disbursements)
            {
                disbInfo.AwardDisbursements.Add(new StudentAwardDisbursement(disb.AwardPeriodCode, disb.AnticipatedDisbursementDate, disb.LastTransmitAmount, disb.LastTransmitDate));
            }
            for(int i = 0; i < disbInfo.AwardDisbursements.Count; i++)
            {
                Assert.AreEqual(disbursements[i].AnticipatedDisbursementDate, disbInfo.AwardDisbursements[i].AnticipatedDisbursementDate);
                Assert.AreEqual(disbursements[i].AwardPeriodCode, disbInfo.AwardDisbursements[i].AwardPeriodCode);
                Assert.AreEqual(disbursements[i].LastTransmitAmount, disbInfo.AwardDisbursements[i].LastTransmitAmount);
                Assert.AreEqual(disbursements[i].LastTransmitDate, disbInfo.AwardDisbursements[i].LastTransmitDate);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullStudentId_ArgumentNullExceptionThrownTest()
        {
            disbInfo = new StudentAwardDisbursementInfo(null, awardCode, awardYearCode);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullAwardCode_ArgumentNullExceptionThrownTest()
        {
            disbInfo = new StudentAwardDisbursementInfo(studentId, null, awardYearCode);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullAwardYearCode_ArgumentNullExceptionThrownTest()
        {
            disbInfo = new StudentAwardDisbursementInfo(studentId, awardCode, null);
        }
    }
}
