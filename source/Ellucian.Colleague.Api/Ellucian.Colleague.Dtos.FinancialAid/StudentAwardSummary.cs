/*Copyright 2014-2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Define the StudentAwardSummary data set
    /// </summary>
    public class StudentAwardSummary
    {
        /// <summary>
        /// Student Id
        /// </summary>
        public string StudentId { get; set; }
        /// <summary>
        /// Financial Aid year
        /// </summary>
        public string AidYear { get; set; }
        /// <summary>
        /// AwardType which is derived from the Award's AwardCategory resource
        /// </summary>
        public string AwardType { get; set; }
        /// <summary>
        /// FundSource which is derived from the AwardType resource
        /// </summary>
        public string FundSource { get; set; }
        /// <summary>
        /// FundType which is derived from the Award resource
        /// </summary>
        public string FundType { get; set; }
        /// <summary>
        /// Amount of award which has been offered to the student across all award periods
        /// </summary>
        public decimal? AmountOffered { get; set; }
        /// <summary>
        /// Amount of award which has been accepted by the student across all award periods
        /// </summary>
        public decimal? AmountAccepted { get; set; }
    }
}
