/*Copyright 2016 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Outside award - awards that students self-report
    /// </summary>
    public class OutsideAward
    {
        /// <summary>
        /// Outside award record assigned id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Student id the outside award is associated with
        /// </summary>
        public string StudentId { get; set; }
        /// <summary>
        /// Award year code award is associated with
        /// </summary>
        public string AwardYearCode { get; set; }
        /// <summary>
        /// Outside award name
        /// </summary>
        public string AwardName { get; set; }
        /// <summary>
        /// Outside award type: scholarship, grant, or loan
        /// </summary>
        public string AwardType { get; set; }
        /// <summary>
        /// Outside award amount
        /// </summary>
        public decimal AwardAmount { get; set; }
        /// <summary>
        /// Outside award funding source
        /// </summary>
        public string AwardFundingSource { get; set; }
    }
}
