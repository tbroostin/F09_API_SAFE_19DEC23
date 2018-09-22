//Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Coordination.FinancialAid.Adapters;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Coordination.FinancialAid.Tests.Adapters
{
    [TestClass]
    public class StudentAwardEntityAdapterTests
    {

        private Dtos.FinancialAid.StudentAward StudentAwardDto;
        private Dtos.FinancialAid.StudentAwardPeriod StudentAwardPeriodDto;

        private StudentAwardDtoToEntityAdapter studentAwardEntityAdapter;

        private string studentId;
        private string awardYearCode;
        private string awardCode;

        private Domain.FinancialAid.Entities.StudentAward StudentAwardEntityTest;
        private Domain.FinancialAid.Entities.StudentAwardPeriod StudentAwardPeriodEntityTest;

        private Domain.FinancialAid.Entities.StudentAwardYear StudentAwardYearEntity;
        private Domain.FinancialAid.Entities.Award AwardEntity;
        private IEnumerable<Domain.FinancialAid.Entities.AwardStatus> AwardStatusEntities;

        [TestInitialize]
        public void Initialize()
        {
            var adapterRegistryMock = new Mock<IAdapterRegistry>();
            var loggerMock = new Mock<ILogger>();
            studentAwardEntityAdapter = new StudentAwardDtoToEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);

            studentId = "0003914";
            awardYearCode = "2014";
            awardCode = "WOOFY";
            StudentAwardPeriodDto = new Dtos.FinancialAid.StudentAwardPeriod()
            {
                AwardYearId = awardYearCode,
                StudentId = studentId,
                AwardId = awardCode,
                AwardPeriodId = "14/FA",
                AwardStatusId = "P",
                AwardAmount = 3000,
                IsFrozen = false,
                IsTransmitted = false,
                IsAmountModifiable = false,
                UpdateActionTaken = Dtos.FinancialAid.AwardStatusCategory.Accepted
            };

            StudentAwardDto = new Dtos.FinancialAid.StudentAward()
            {
                AwardYearId = awardYearCode,
                StudentId = studentId,
                AwardId = awardCode,
                IsEligible = true,
                IsAmountModifiable = false,
                StudentAwardPeriods = new List<Dtos.FinancialAid.StudentAwardPeriod>() { StudentAwardPeriodDto }
            };

            StudentAwardYearEntity = new Domain.FinancialAid.Entities.StudentAwardYear(studentId, awardYearCode, new Domain.FinancialAid.Entities.FinancialAidOffice("office"));
            AwardEntity = new Domain.FinancialAid.Entities.Award(awardCode, "desc", new Domain.FinancialAid.Entities.AwardCategory("foo", "bar", null));

            AwardStatusEntities = new List<Domain.FinancialAid.Entities.AwardStatus>()
            {
                new Domain.FinancialAid.Entities.AwardStatus("P", "Pending", Domain.FinancialAid.Entities.AwardStatusCategory.Pending)
            };

            StudentAwardEntityTest = studentAwardEntityAdapter.MapToType(StudentAwardDto, StudentAwardYearEntity, AwardEntity, AwardStatusEntities);
            StudentAwardPeriodEntityTest = StudentAwardEntityTest.StudentAwardPeriods.First();

        }

        [TestMethod]
        public void StudentAward_AwardYear_EqualsTest()
        {
            Assert.AreEqual(StudentAwardDto.AwardYearId, StudentAwardEntityTest.StudentAwardYear.Code);
        }

        [TestMethod]
        public void StudentAward_StudentId_EqualsTest()
        {
            Assert.AreEqual(StudentAwardDto.StudentId, StudentAwardEntityTest.StudentId);
        }

        [TestMethod]
        public void StudentAward_AwardId_EqualsTest()
        {
            Assert.AreEqual(StudentAwardDto.AwardId, StudentAwardEntityTest.Award.Code);
        }

        [TestMethod]
        public void StudentAward_IsEligible_EqualsTest()
        {
            Assert.AreEqual(StudentAwardDto.IsEligible, StudentAwardEntityTest.IsEligible);
        }

        [TestMethod]
        public void StudentAward_IsModifiable_EqualsTest()
        {
            Assert.AreEqual(StudentAwardDto.IsAmountModifiable, StudentAwardEntityTest.IsAmountModifiable);
        }

        [TestMethod]
        public void StudentAward_StudentAwardPeriods_CountTest()
        {
            Assert.AreEqual(StudentAwardDto.StudentAwardPeriods.Count(), StudentAwardEntityTest.StudentAwardPeriods.Count());
        }

        [TestMethod]
        public void StudentAwardPeriod_StudentId_EqualsTest()
        {
            Assert.AreEqual(StudentAwardPeriodDto.StudentId, StudentAwardPeriodEntityTest.StudentId);
        }

        [TestMethod]
        public void StudentAwardPeriod_AwardYear_EqualsTest()
        {
            Assert.AreEqual(StudentAwardPeriodDto.AwardYearId, StudentAwardPeriodEntityTest.StudentAwardYear.Code);
        }

        [TestMethod]
        public void StudentAwardPeriod_AwardId_EqualsTest()
        {
            Assert.AreEqual(StudentAwardPeriodDto.AwardId, StudentAwardPeriodEntityTest.Award.Code);
        }

        [TestMethod]
        public void StudentAwardPeriod_AwardPeriodId_EqualsTest()
        {
            Assert.AreEqual(StudentAwardPeriodDto.AwardPeriodId, StudentAwardPeriodEntityTest.AwardPeriodId);
        }

        [TestMethod]
        public void StudentAwardPeriod_AwardAmount_EqualsTest()
        {
            Assert.AreEqual(StudentAwardPeriodDto.AwardAmount, StudentAwardPeriodEntityTest.AwardAmount);
        }

        [TestMethod]
        public void StudentAwardPeriod_AwardStatusId_EqualsTest()
        {
            Assert.AreEqual(StudentAwardPeriodDto.AwardStatusId, StudentAwardPeriodEntityTest.AwardStatus.Code);
        }

        [TestMethod]
        public void StudentAwardPeriod_IsFrozen_EqualsTest()
        {
            Assert.AreEqual(StudentAwardPeriodDto.IsFrozen, StudentAwardPeriodEntityTest.IsFrozen);
        }

        [TestMethod]
        public void StudentAwardPeriod_IsTransmitted_EqualsTest()
        {
            Assert.AreEqual(StudentAwardPeriodDto.IsTransmitted, StudentAwardPeriodEntityTest.IsTransmitted);
        }

        [TestMethod]
        public void StudentAwardPeriod_IsModifiable_EqualsTest()
        {
            Assert.AreEqual(StudentAwardPeriodDto.IsAmountModifiable, StudentAwardPeriodEntityTest.IsAmountModifiable);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StudentAwardDto_RequiredArgumentTest()
        {
            studentAwardEntityAdapter.MapToType(null, StudentAwardYearEntity, AwardEntity, AwardStatusEntities);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StudentAwardYearEntity_RequiredTest()
        {
            studentAwardEntityAdapter.MapToType(StudentAwardDto, null, AwardEntity, AwardStatusEntities);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void StudentAwardYearEntityMustMatchYearInStudentAwardDtoTest()
        {
            StudentAwardDto.AwardYearId = "foobar";
            studentAwardEntityAdapter.MapToType(StudentAwardDto, StudentAwardYearEntity, AwardEntity, AwardStatusEntities);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void StudentAwardYearEntityMustMatchStudentIdInStudentAwardDtoTest()
        {
            StudentAwardDto.StudentId = "foobar";
            studentAwardEntityAdapter.MapToType(StudentAwardDto, StudentAwardYearEntity, AwardEntity, AwardStatusEntities);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AwardEntity_RequiredArgumentTest()
        {
            studentAwardEntityAdapter.MapToType(StudentAwardDto, StudentAwardYearEntity, null, AwardStatusEntities);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void AwardEntityMustMatchAwardIdInStudentAwardDtoTest()
        {
            StudentAwardDto.AwardId = "foobar";
            studentAwardEntityAdapter.MapToType(StudentAwardDto, StudentAwardYearEntity, AwardEntity, AwardStatusEntities);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AwardStatusEntities_RequiredArgumenTest()
        {
            studentAwardEntityAdapter.MapToType(StudentAwardDto, StudentAwardYearEntity, AwardEntity, null);
        }
    }
}
