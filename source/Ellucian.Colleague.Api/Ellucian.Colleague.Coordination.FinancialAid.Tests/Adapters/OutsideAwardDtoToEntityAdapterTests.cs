/*Copyright 2016 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Coordination.FinancialAid.Adapters;
using Moq;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.FinancialAid.Tests.Adapters
{
    [TestClass]
    public class OutsideAwardDtoToEntityAdapterTests
    {
        private Domain.FinancialAid.Entities.OutsideAward expectedOtsideAwardEntity;
        private Domain.FinancialAid.Entities.OutsideAward actualOtsideAwardEntity;

        private Dtos.FinancialAid.OutsideAward inputOutsideAwardDto;

        private OutsideAwardDtoToEntityAdapter outsideAwardDtoToEntityAdapter;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private Mock<ILogger> loggerMock;

        [TestInitialize]
        public void Initialize()
        {
            inputOutsideAwardDto = new Dtos.FinancialAid.OutsideAward()
            {
                Id = "23",
                StudentId = "67898765",
                AwardYearCode = "2016",
                AwardName = "OutsideAward",
                AwardType = "Grant",
                AwardAmount = 345.67m,
                AwardFundingSource = "Vanguard"
            };

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            loggerMock = new Mock<ILogger>();

            outsideAwardDtoToEntityAdapter = new OutsideAwardDtoToEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            inputOutsideAwardDto = null;
            expectedOtsideAwardEntity = null;
            actualOtsideAwardEntity = null;
            outsideAwardDtoToEntityAdapter = null;
            adapterRegistryMock = null;
            loggerMock = null;
        }

        [TestMethod]
        public void ActualOutsideAwardDto_IsNotNullTest()
        {
            actualOtsideAwardEntity = outsideAwardDtoToEntityAdapter.MapToType(inputOutsideAwardDto);
            Assert.IsNotNull(actualOtsideAwardEntity);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullInputOutsideAward_ArgumentNullExceptionThrownTest()
        {
            outsideAwardDtoToEntityAdapter.MapToType(null);
        }

        [TestMethod]
        public void OutsideRecordId_ActualOutsideAward_EqualsExpectedTest()
        {
            expectedOtsideAwardEntity = new Domain.FinancialAid.Entities.OutsideAward(inputOutsideAwardDto.Id, inputOutsideAwardDto.StudentId,
                inputOutsideAwardDto.AwardYearCode, inputOutsideAwardDto.AwardName, inputOutsideAwardDto.AwardType, inputOutsideAwardDto.AwardAmount,
                inputOutsideAwardDto.AwardFundingSource);
            actualOtsideAwardEntity = outsideAwardDtoToEntityAdapter.MapToType(inputOutsideAwardDto);
            Assert.AreEqual(expectedOtsideAwardEntity.Id, actualOtsideAwardEntity.Id);
            Assert.AreEqual(expectedOtsideAwardEntity.StudentId, actualOtsideAwardEntity.StudentId);
            Assert.AreEqual(expectedOtsideAwardEntity.AwardYearCode, actualOtsideAwardEntity.AwardYearCode);
            Assert.AreEqual(expectedOtsideAwardEntity.AwardName, actualOtsideAwardEntity.AwardName);
            Assert.AreEqual(expectedOtsideAwardEntity.AwardType, actualOtsideAwardEntity.AwardType);
            Assert.AreEqual(expectedOtsideAwardEntity.AwardAmount, actualOtsideAwardEntity.AwardAmount);
            Assert.AreEqual(expectedOtsideAwardEntity.AwardFundingSource, actualOtsideAwardEntity.AwardFundingSource);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ErrorCreatingOutsideAwardEntity_InvalidOperationExceptionThrownTest()
        {
            inputOutsideAwardDto.StudentId = string.Empty;
            outsideAwardDtoToEntityAdapter.MapToType(inputOutsideAwardDto);
        }

        [TestMethod]
        public void InputOutsideAwardDtoIdIsNull_DtoIsNotNullTest()
        {
            inputOutsideAwardDto.Id = null;
            actualOtsideAwardEntity = outsideAwardDtoToEntityAdapter.MapToType(inputOutsideAwardDto);
            Assert.IsNotNull(actualOtsideAwardEntity);
        }

        [TestMethod]
        public void NullOutsideRecordId_ActualOutsideAward_EqualsExpectedTest()
        {
            expectedOtsideAwardEntity = new Domain.FinancialAid.Entities.OutsideAward(inputOutsideAwardDto.StudentId,
                inputOutsideAwardDto.AwardYearCode, inputOutsideAwardDto.AwardName, inputOutsideAwardDto.AwardType, inputOutsideAwardDto.AwardAmount,
                inputOutsideAwardDto.AwardFundingSource);

            inputOutsideAwardDto.Id = null;
            actualOtsideAwardEntity = outsideAwardDtoToEntityAdapter.MapToType(inputOutsideAwardDto);
            Assert.AreEqual(expectedOtsideAwardEntity.Id, actualOtsideAwardEntity.Id);
            Assert.AreEqual(expectedOtsideAwardEntity.StudentId, actualOtsideAwardEntity.StudentId);
            Assert.AreEqual(expectedOtsideAwardEntity.AwardYearCode, actualOtsideAwardEntity.AwardYearCode);
            Assert.AreEqual(expectedOtsideAwardEntity.AwardName, actualOtsideAwardEntity.AwardName);
            Assert.AreEqual(expectedOtsideAwardEntity.AwardType, actualOtsideAwardEntity.AwardType);
            Assert.AreEqual(expectedOtsideAwardEntity.AwardAmount, actualOtsideAwardEntity.AwardAmount);
            Assert.AreEqual(expectedOtsideAwardEntity.AwardFundingSource, actualOtsideAwardEntity.AwardFundingSource);
        }
    }
}
