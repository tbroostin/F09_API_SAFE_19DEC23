/*Copyright 2018 Ellucian Company L.P. and its affiliates*/
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Finance.AccountActivity
{
    /// <summary>
    /// Object holding information about student award disbursements
    /// </summary>
    public class StudentAwardDisbursementInfo
    {
        /// <summary>
        /// Student id
        /// </summary>
        public string StudentId { get; set; }
        /// <summary>
        /// Award Code
        /// </summary>
        public string AwardCode { get; set; }        
        /// <summary>
        /// Award Description
        /// </summary>
        public string AwardDescription { get; set; }
        /// <summary>
        /// Award Year Code
        /// </summary>
        public string AwardYearCode { get; set; }
        /// <summary>
        /// List of Award Disbursements
        /// </summary>
        public IEnumerable<StudentAwardDisbursement> AwardDisbursements { get; set; }

    }
}
