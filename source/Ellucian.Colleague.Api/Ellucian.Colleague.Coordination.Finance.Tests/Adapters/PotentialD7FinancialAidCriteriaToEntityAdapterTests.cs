// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Finance.Adapters;
using Ellucian.Colleague.Dtos.Finance;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Coordination.Finance.Tests.Adapters
{
    [TestClass]
    public class PotentialD7FinancialAidCriteriaToEntityAdapterTests
    {
        static Mock<ILogger> loggerMock = new Mock<ILogger>();
        static Mock<IAdapterRegistry> adapterRegistryMock = new Mock<IAdapterRegistry>();
        static PotentialD7FinancialAidCriteriaToEntityAdapter adapter = new PotentialD7FinancialAidCriteriaToEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);
        private string _award1 = "Award1";
        private string _award2 = "Award2";
        private bool _xmit1 = false;
        private bool _xmit2 = true;

        /// <summary>
        /// Verify a valid result with valid inputs: with awards
        /// </summary>
        [TestMethod]
        public void PotentialD7FinancialAidCriteriaToEntityAdapter_ValidWithAwards()
        {
            var dto = new PotentialD7FinancialAidCriteria()
            {
                StudentId = "Valid",
                TermId = "Term",
                AwardPeriodAwardsToEvaluate = new List<AwardPeriodAwardTransmitExcessStatus>(){
                    new AwardPeriodAwardTransmitExcessStatus(){AwardPeriodAward = _award1, TransmitExcessIndicator = _xmit1 },
                    new AwardPeriodAwardTransmitExcessStatus(){AwardPeriodAward = _award2, TransmitExcessIndicator = _xmit2 },
                },
            };
            Domain.Finance.Entities.PotentialD7FinancialAidCriteria entity = (Domain.Finance.Entities.PotentialD7FinancialAidCriteria) adapter.MapToType(dto);
            Assert.AreEqual("Valid", entity.StudentId);
            Assert.AreEqual("Term", entity.TermId);
            var awards = entity.AwardsToEvaluate;
            Assert.AreEqual(_award1, awards[0].AwardPeriodAward);
            Assert.AreEqual(_xmit1, awards[0].TransmitExcessIndicator);
            Assert.AreEqual(_award2, awards[1].AwardPeriodAward);
            Assert.AreEqual(_xmit2, awards[1].TransmitExcessIndicator);
        }

        /// <summary>
        /// Verify a valid result with valid inputs: empty awards
        /// </summary>
        [TestMethod]
        public void PotentialD7FinancialAidCriteriaToEntityAdapter_ValidEmptyAwards()
        {
            var dto = new PotentialD7FinancialAidCriteria()
            {
                StudentId = "Valid",
                TermId = "Term",
                AwardPeriodAwardsToEvaluate = new List<AwardPeriodAwardTransmitExcessStatus>(),
            };
            var entity = adapter.MapToType(dto);
            Assert.AreEqual("Valid", entity.StudentId);
            Assert.AreEqual("Term", entity.TermId);
            Assert.IsNotNull(entity.AwardsToEvaluate);
            Assert.AreEqual(0, entity.AwardsToEvaluate.Count);
        }

        /// <summary>
        /// Validate exception when student id is null
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PotentialD7FinancialAidCriteriaToEntityAdapter_NullStudentId()
        {
            var dto = new PotentialD7FinancialAidCriteria()
            {
                StudentId = null,
                TermId = "Term",
                AwardPeriodAwardsToEvaluate = new List<AwardPeriodAwardTransmitExcessStatus>(),
            };
            var entity = adapter.MapToType(dto);
        }

        /// <summary>
        /// Validate an exception when student id is empty
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PotentialD7FinancialAidCriteriaToEntityAdapter_EmptyStudentId()
        {
            var dto = new PotentialD7FinancialAidCriteria()
            {
                StudentId = string.Empty,
                TermId = "Term",
                AwardPeriodAwardsToEvaluate = new List<AwardPeriodAwardTransmitExcessStatus>(),
            };
            var entity = adapter.MapToType(dto);
        }

        /// <summary>
        /// Validate an exception when term is null
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PotentialD7FinancialAidCriteriaToEntityAdapter_NullTermId()
        {
            var dto = new PotentialD7FinancialAidCriteria()
            {
                StudentId = "Valid",
                TermId = null,
                AwardPeriodAwardsToEvaluate = new List<AwardPeriodAwardTransmitExcessStatus>(),
            };
            var entity = adapter.MapToType(dto);
        }

        /// <summary>
        /// Validate an exception when term is empty
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PotentialD7FinancialAidCriteriaToEntityAdapter_EmptyTermId()
        {
            var dto = new PotentialD7FinancialAidCriteria()
            {
                StudentId = "Valid",
                TermId = string.Empty,
                AwardPeriodAwardsToEvaluate = new List<AwardPeriodAwardTransmitExcessStatus>(),
            };
            var entity = adapter.MapToType(dto);
        }

        /// <summary>
        /// Validate an exception when awards are null
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PotentialD7FinancialAidCriteriaToEntityAdapter_NullAwardsToEvaluate()
        {
            var dto = new PotentialD7FinancialAidCriteria()
            {
                StudentId = "Valid",
                TermId = "Term",
                AwardPeriodAwardsToEvaluate = null,
            };
            var entity = adapter.MapToType(dto);
        }

        /// <summary>
        /// Valdiate an exception when a provided award name is null
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PotentialD7FinancialAidCriteriaToEntityAdapter_NullAward()
        {
            var dto = new PotentialD7FinancialAidCriteria()
            {
                StudentId = "Valid",
                TermId = "Term",
                AwardPeriodAwardsToEvaluate = new List<AwardPeriodAwardTransmitExcessStatus>(){
                    new AwardPeriodAwardTransmitExcessStatus(){AwardPeriodAward = null, TransmitExcessIndicator = _xmit1 },
                    new AwardPeriodAwardTransmitExcessStatus(){AwardPeriodAward = _award2, TransmitExcessIndicator = _xmit2 },
                },
            };
            var entity = adapter.MapToType(dto);
        }

        /// <summary>
        /// Validate an exception when a provided award name is empty
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PotentialD7FinancialAidCriteriaToEntityAdapter_EmptyAward()
        {
            var dto = new PotentialD7FinancialAidCriteria()
            {
                StudentId = "Valid",
                TermId = "Term",
                AwardPeriodAwardsToEvaluate = new List<AwardPeriodAwardTransmitExcessStatus>(){
                    new AwardPeriodAwardTransmitExcessStatus(){AwardPeriodAward = string.Empty, TransmitExcessIndicator = _xmit1 },
                    new AwardPeriodAwardTransmitExcessStatus(){AwardPeriodAward = _award2, TransmitExcessIndicator = _xmit2 },
                },
            };
            var entity = adapter.MapToType(dto);
        }
    }
}
