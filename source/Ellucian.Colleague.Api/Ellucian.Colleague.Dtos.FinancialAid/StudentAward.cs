/*Copyright 2014-2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// StudentAward DTO Object encapsulates a student's award data for a single award in a particular award year.
    /// The DTO also contains a list of StudentAwardPeriod DTOs, which break down the award into Colleague
    /// defined segments
    /// </summary>
    public class StudentAward
    {
        /// <summary>
        /// The Award Year to which this object applies
        /// Required on PUT
        /// </summary>
        public string AwardYearId { get; set; }
        /// <summary>
        /// The StudentId of the award
        /// Required on PUT
        /// </summary>
        public string StudentId { get; set; }
        /// <summary>
        /// The AwardId
        /// Required on PUT
        /// </summary>
        public string AwardId { get; set; }
        /// <summary>
        /// The list of StudentAwardPeriod DTOs - A segmented breakdown of the award
        /// Required on PUT
        /// </summary>
        public List<StudentAwardPeriod> StudentAwardPeriods { get; set; }

        /// <summary>
        /// Indicates whether the student is eligible to receive the award. This is currently unused.
        /// </summary>
        public bool IsEligible { get; set; }

        /// <summary>
        /// Indicates whether the amount of this award and award periods can be modified.
        /// If this is false, a PUT StudentAward or PUT StudentAwardPackage request will throw an exception
        /// </summary>
        public bool IsAmountModifiable { get; set; }

        /// <summary>
        /// Foreign Key: AwardPackageChangeRequest Id that is pending for this StudentAward.
        /// If a value exists in this field, a PUT StudentAward or PUT StudentAwardPackage will throw an exception
        /// </summary>
        public string PendingChangeRequestId { get; set; }
    }
}
