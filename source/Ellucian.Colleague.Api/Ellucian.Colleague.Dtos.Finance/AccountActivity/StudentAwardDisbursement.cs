/*Copyright 2018 Ellucian Company L.P. and its affiliates*/
using System;

namespace Ellucian.Colleague.Dtos.Finance.AccountActivity
{
    /// <summary>
    /// Object holding information about a disbursement such as
    /// disbursement date, last transmit amount, last transmit date
    /// </summary>
    public class StudentAwardDisbursement
    {
        /// <summary>
        /// Award period code
        /// </summary>
        public string AwardPeriodCode { get; set; }
        /// <summary>
        /// The date of anticipated disbursement
        /// </summary>
        public DateTime AnticipatedDisbursementDate { get; set; }
        /// <summary>
        /// Last transmitted amount
        /// </summary>
        public decimal? LastTransmitAmount { get; set; }
        /// <summary>
        /// Last trasmittal date
        /// </summary>
        public DateTime? LastTransmitDate { get; set; }

    }
}