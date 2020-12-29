// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    [Serializable]
    /// <summary>
    /// Contains the criteria necessary to query Colleague about potentially untransmitted
    /// D7 financial aid award amounts
    /// </summary>
    public class PotentialD7FinancialAidCriteria
    {
        private readonly string _studentId;
        private readonly string _termId;
        private readonly List<AwardPeriodAwardTransmitExcessStatus> _awardsToEvaluate;

        /// <summary>
        /// Contruct a <see cref="PotentialD7FinancialAidCriteria"/>
        /// </summary>
        /// <param name="studentId">The identifier of the student of interest.  Required.</param>
        /// <param name="termId">The identifier of the academic term of interest.  Required.</param>
        /// <param name="awardsToEvaluate">The list of <see cref="AwardPeriodAwardTransmitExcessStatus"/> to evaluate.
        /// Required.  Can be empty, but so too will be the results.</param>
        public PotentialD7FinancialAidCriteria(string studentId, string termId,
            List<AwardPeriodAwardTransmitExcessStatus> awardsToEvaluate)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (string.IsNullOrEmpty(termId))
            {
                throw new ArgumentNullException("termId");
            }
            if (awardsToEvaluate == null)
            {
                throw new ArgumentNullException("awardsToEvaluate");
            }

            _studentId = studentId;
            _termId = termId;
            _awardsToEvaluate = awardsToEvaluate;
        }

        /// <summary>
        /// The identifier of the student of interest
        /// </summary>
        public string StudentId { get { return _studentId; } }

        /// <summary>
        /// The identifier of the term of interest
        /// </summary>
        public string TermId { get { return _termId; } }

        /// <summary>
        /// An enumeration of <see cref="AwardPeriodAwardTransmitExcessStatus"/> to consider for evaluation.
        /// </summary>
        public IList<AwardPeriodAwardTransmitExcessStatus> AwardsToEvaluate { get { return _awardsToEvaluate; } }
    }
}
