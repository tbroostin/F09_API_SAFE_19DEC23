// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Finance.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.Finance.Tests.Entities
{
    [TestClass]
    public class PotentialD7FinancialAidCriteriaTests
    {
        private string _student = "STUDENT";
        private string _term = "TERM";
        private string _award = "AWD";
        private bool _xmit = true;
        private List<AwardPeriodAwardTransmitExcessStatus> _emptyList;
        private List<AwardPeriodAwardTransmitExcessStatus> _goodList;

        [TestInitialize]
        public void Initialize()
        {
            _emptyList = new List<AwardPeriodAwardTransmitExcessStatus>();
            _goodList = new List<AwardPeriodAwardTransmitExcessStatus>()
            {
                new AwardPeriodAwardTransmitExcessStatus(_award, _xmit),
            };
        }

        [TestCleanup]
        public void Cleanup()
        {
            _emptyList = null;
            _goodList = null;
        }

        /// <summary>
        /// Contructs a valid object using valid values.  List is non-null but empty.
        /// </summary>
        [TestMethod]
        public void PotentialD7FinancialAidCriteria_Constructor_Valid()
        {
            var criteria = new PotentialD7FinancialAidCriteria(_student, _term, _emptyList);
            Assert.AreEqual(_student, criteria.StudentId);
            Assert.AreEqual(_term, criteria.TermId);
            Assert.IsNotNull(criteria.AwardsToEvaluate);
            Assert.AreEqual(0, criteria.AwardsToEvaluate.Count());
        }

        /// <summary>
        /// Null student id throws an ArgumentNullException
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PotentialD7FinancialAidCriteria_Constructor_NullStudentId()
        {
            var criteria = new PotentialD7FinancialAidCriteria(null, _term, _emptyList);
        }

        /// <summary>
        /// Empty student id throws ArgumentNullException
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PotentialD7FinancialAidCriteria_Constructor_EmptyStudentId()
        {
            var criteria = new PotentialD7FinancialAidCriteria(string.Empty, _term, _emptyList);
        }

        /// <summary>
        /// Null term id throws ArgumentNullException
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PotentialD7FinancialAidCriteria_Constructor_NullTermId()
        {
            var criteria = new PotentialD7FinancialAidCriteria(_student, null, _emptyList);
        }
        /// <summary>
        /// Null term id throws ArgumentNullException
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PotentialD7FinancialAidCriteria_Constructor_EmptyTermId()
        {
            var criteria = new PotentialD7FinancialAidCriteria(_student, string.Empty, _emptyList);
        }
        /// <summary>
        /// Null awardsToEvaluate throws ArgumentNullException
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PotentialD7FinancialAidCriteria_Constructor_NullAwardsToEvaluate()
        {
            var criteria = new PotentialD7FinancialAidCriteria(_student, _term, null);
        }

        /// <summary>
        /// Null awardsToEvaluate throws ArgumentNullException
        /// </summary>
        [TestMethod]
        public void PotentialD7FinancialAidCriteria_Constructor_ValidEmptyAwardsToEvaluate()
        {
            var criteria = new PotentialD7FinancialAidCriteria(_student, _term, _goodList);
            Assert.AreEqual(_student, criteria.StudentId);
            Assert.AreEqual(_term, criteria.TermId);
            Assert.IsNotNull(criteria.AwardsToEvaluate);
            Assert.AreEqual(1, criteria.AwardsToEvaluate.Count());
            Assert.AreEqual(_award, criteria.AwardsToEvaluate.ElementAt(0).AwardPeriodAward);
            Assert.AreEqual(_xmit, criteria.AwardsToEvaluate.ElementAt(0).TransmitExcessIndicator);
        }
    }
}
