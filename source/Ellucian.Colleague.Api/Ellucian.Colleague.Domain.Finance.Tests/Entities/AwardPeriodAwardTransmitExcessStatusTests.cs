// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Finance.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.Finance.Tests.Entities
{
    [TestClass]
    public class AwardPeriodAwardTransmitExcessStatusTests
    {
        private string _awardPeriodAward;
        private bool _xmitInd;

        [TestInitialize]
        public void Initialize()
        {
            _awardPeriodAward = "FOO";
            _xmitInd = true;
        }

        /// <summary>
        /// Validate that a null award throws an exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AwardPeriodAwardTransmitExcessStatus_Constructor_NullAward()
        {
            var result = new AwardPeriodAwardTransmitExcessStatus(null, false);
        }

        /// <summary>
        /// Validate that an empty award throws an exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AwardPeriodAwardTransmitExcessStatus_Constructor_EmptyAward()
        {
            var result = new AwardPeriodAwardTransmitExcessStatus(string.Empty, false);
        }

        /// <summary>
        /// Validate that valid input results in a valid object
        /// </summary>
        [TestMethod]
        public void AwardPeriodAwardTransmitExcessStatus_Constructor_Valid()
        {
            var result = new AwardPeriodAwardTransmitExcessStatus(_awardPeriodAward, _xmitInd);
            Assert.AreEqual(_awardPeriodAward, result.AwardPeriodAward);
            Assert.AreEqual(_xmitInd, result.TransmitExcessIndicator);
        }

        [TestCleanup]
        public void AwardPeriodAwardTransmitExcessStatus_Cleanup()
        {
            _awardPeriodAward = null;
        }
    }
}
