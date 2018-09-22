/*Copyright 2014-2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// This class indicates whether a student has completed Financial Aid Applications for a particular award year
    /// </summary>
    [Obsolete("Obsolete as of API version 1.7. Deprecated. FinancialAidApplication objects are now represented by Fafsa and ProfileApplication objects")]
    public class FinancialAidApplication
    {
        /// <summary>
        /// The year for the applications
        /// </summary>
        public string AwardYear { get; set; }

        /// <summary>
        /// The student's Id
        /// </summary>
        public string StudentId { get; set; }

        /// <summary>
        /// Has the student completed the Profile application. 
        /// To determine the value of this attribute using the ProfileApplication endpoints, simply query the list of ProfileApplication objects to 
        /// find an object with the awardYear and studentId you're looking for. The existence of this object is equivalent to a true value for IsProfileComplete
        /// </summary>
        public bool IsProfileComplete { get; set; }

        /// <summary>
        /// Has the student complete the FAFSA application
        /// To determine the value of this attribute using the Fafsa endpoints, simply query the list of FAFSA objects to 
        /// find an object with the awardYear and studentId you're looking for. The existence of this object is equivalent to a true value for IsFafsaComplete
        /// </summary>
        public bool IsFafsaComplete { get; set; }

    }
}
