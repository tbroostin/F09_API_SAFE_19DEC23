//Copyright 2014-2016 Ellucian Company L.P. and its affiliates.
using System.Linq;
using Ellucian.Colleague.Coordination.FinancialAid.Adapters;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Coordination.FinancialAid.Tests.Adapters
{
    /// <summary>
    /// Tests for the StudentAwardDtoAdapter, which maps a StudentAward Entity to a StudentAward DTO
    /// </summary>
    [TestClass]
    public class StudentAwardDtoAdapterTests
    {
        private Domain.FinancialAid.Entities.StudentAwardYear studentAwardYear;
        private string studentId;
        private Domain.FinancialAid.Entities.Award award;
        private bool isEligible;
        private Domain.FinancialAid.Entities.StudentAward studentAwardEntity;

        private string awardPeriodId;
        private Domain.FinancialAid.Entities.AwardStatus awardStatus;
        private bool isFrozen;
        private bool isTransmitted;
        private decimal? awardAmount;
        private string pendingChangeRequestId;
        private Domain.FinancialAid.Entities.StudentAwardPeriod studentAwardPeriodEntity;

        private Dtos.FinancialAid.StudentAward studentAwardDtoTest;
        private Dtos.FinancialAid.StudentAwardPeriod studentAwardPeriodDtoTest;


        [TestInitialize]
        public void Initialize()
        {
            var loggerMock = new Mock<ILogger>();
            var adapterRegistryMock = new Mock<IAdapterRegistry>();
            var studentAwardDtoAdapter = new StudentAwardEntityToDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);

            studentId = "0003914";
            studentAwardYear = new Domain.FinancialAid.Entities.StudentAwardYear(studentId, "2014", new Domain.FinancialAid.Entities.FinancialAidOffice("office"));
            studentAwardYear.CurrentOffice.AddConfiguration(new Domain.FinancialAid.Entities.FinancialAidConfiguration("office", "2014"));
            award = new Domain.FinancialAid.Entities.Award("WOOFY", "Woofy Award", new Domain.FinancialAid.Entities.AwardCategory("foo", "bar", null));
            isEligible = true;
            pendingChangeRequestId = "5";
            studentAwardEntity = new Domain.FinancialAid.Entities.StudentAward(studentAwardYear, studentId, award, isEligible)
                {
                    PendingChangeRequestId = pendingChangeRequestId
                };

            awardPeriodId = "14/FA";
            awardStatus = new Domain.FinancialAid.Entities.AwardStatus("A", "Accepted", Domain.FinancialAid.Entities.AwardStatusCategory.Accepted);
            isFrozen = false;
            isTransmitted = false;
            awardAmount = 3982;
            studentAwardPeriodEntity = new Domain.FinancialAid.Entities.StudentAwardPeriod(studentAwardEntity, awardPeriodId, awardStatus, isFrozen, isTransmitted)
            {
                AwardAmount = awardAmount
            };

            studentAwardDtoTest = studentAwardDtoAdapter.MapToType(studentAwardEntity);
            studentAwardPeriodDtoTest = studentAwardDtoTest.StudentAwardPeriods.First();
        }

        [TestMethod]
        public void AwardYear_EqualsTest()
        {
            Assert.AreEqual(studentAwardEntity.StudentAwardYear.Code, studentAwardDtoTest.AwardYearId);
        }

        [TestMethod]
        public void StudentId_EqualsTest()
        {
            Assert.AreEqual(studentAwardEntity.StudentId, studentAwardDtoTest.StudentId);
        }

        [TestMethod]
        public void AwardId_EqualsTest()
        {
            Assert.AreEqual(studentAwardEntity.Award.Code, studentAwardDtoTest.AwardId);
        }

        [TestMethod]
        public void IsEligible_EqualsTest()
        {
            Assert.AreEqual(studentAwardEntity.IsEligible, studentAwardDtoTest.IsEligible);
        }

        [TestMethod]
        public void IsAmountModifiable_EqualsTest()
        {
            Assert.AreEqual(studentAwardEntity.IsAmountModifiable, studentAwardDtoTest.IsAmountModifiable);
        }

        [TestMethod]
        public void PendingChangeRequestId_EqualsTest()
        {
            Assert.AreEqual(studentAwardEntity.PendingChangeRequestId, studentAwardDtoTest.PendingChangeRequestId);
        }

        [TestMethod]
        public void StudentAwardPeriods_HasValueTest()
        {
            Assert.AreEqual(studentAwardEntity.StudentAwardPeriods.Count(), studentAwardDtoTest.StudentAwardPeriods.Count());
        }

        [TestMethod]
        public void StudentAwardPeriod_AwardYear_EqualsTest()
        {
            Assert.AreEqual(studentAwardPeriodEntity.StudentAwardYear.Code, studentAwardPeriodDtoTest.AwardYearId);
        }

        [TestMethod]
        public void StudentAwardPeriod_StudentId_EqualsTest()
        {
            Assert.AreEqual(studentAwardPeriodEntity.StudentId, studentAwardPeriodDtoTest.StudentId);
        }

        [TestMethod]
        public void StudentAwardPeriod_AwardId_EqualsTest()
        {
            Assert.AreEqual(studentAwardPeriodEntity.Award.Code, studentAwardPeriodDtoTest.AwardId);
        }

        [TestMethod]
        public void StudentAwardPeriod_AwardPeriodId_EqualsTest()
        {
            Assert.AreEqual(studentAwardPeriodEntity.AwardPeriodId, studentAwardPeriodDtoTest.AwardPeriodId);
        }

        [TestMethod]
        public void StudentAwardPeriod_AwardAmount_EqualsTest()
        {
            Assert.AreEqual(studentAwardPeriodEntity.AwardAmount, studentAwardPeriodDtoTest.AwardAmount);
        }

        [TestMethod]
        public void StudentAwardPeriod_AwardStatus_EqualsTest()
        {
            Assert.AreEqual(studentAwardPeriodEntity.AwardStatus.Code, studentAwardPeriodDtoTest.AwardStatusId);
        }

        [TestMethod]
        public void StudentAwardPeriod_IsFrozen_EqualsTest()
        {
            Assert.AreEqual(studentAwardPeriodEntity.IsFrozen, studentAwardPeriodDtoTest.IsFrozen);
        }

        [TestMethod]
        public void StudentAwardPeriod_IsTransmitted_EqualsTest()
        {
            Assert.AreEqual(studentAwardPeriodEntity.IsTransmitted, studentAwardPeriodDtoTest.IsTransmitted);
        }

        [TestMethod]
        public void StudentAwardPeriod_IsAmountModifiable_EqualsTest()
        {
            Assert.AreEqual(studentAwardPeriodEntity.IsAmountModifiable, studentAwardPeriodDtoTest.IsAmountModifiable);
        }

        [TestMethod]
        public void StudentAwardPeriod_IsStatusModifiable_EqualsTest()
        {
            Assert.AreEqual(studentAwardPeriodEntity.IsStatusModifiable, studentAwardPeriodDtoTest.IsStatusModifiable);
        }

        [TestMethod]
        public void StudentAwardPeriod_UpdateActionTaken_NullTest()
        {
            Assert.IsNull(studentAwardPeriodDtoTest.UpdateActionTaken);
        }

        [TestMethod]
        public void StudentAwardPeriod_IsIgnoreOnAwardLetter_NullTest()
        {
            Assert.AreEqual(studentAwardPeriodEntity.IsIgnoredOnAwardLetter, studentAwardPeriodDtoTest.IsIgnoredOnAwardLetter);
        }
    }
}
