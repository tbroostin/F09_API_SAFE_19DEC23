// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Finance
{
    /// <summary>
    /// Contains the necessary criteria to determine if a student potentially
    /// has untransmitted D7 financial aid.
    /// </summary>
    public class PotentialD7FinancialAidCriteria
    {
        /// <summary>
        /// The identifier of the student of interest.
        /// </summary>
        public string StudentId { get; set; }

        /// <summary>
        /// The identifier of the term of interest.
        /// </summary>
        public string TermId { get; set; }

        /// <summary>
        /// An enumeration of <see cref="AwardPeriodAwardTransmitExcessStatus"/> financial aid items 
        /// to evaluate for potentially untransmitted financial aid amounts.  Results will be a subset
        /// of this enumeration.  The enumeration can be empty, but so to will be the results of this query.
        /// </summary>
        public IEnumerable<AwardPeriodAwardTransmitExcessStatus> AwardPeriodAwardsToEvaluate { get; set; }
    }
}
