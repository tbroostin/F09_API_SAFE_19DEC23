/*Copyright 2018 Ellucian Company L.P. and its affiliates*/
using System;

namespace Ellucian.Colleague.Domain.Finance.Entities.AccountActivity
{
    /// <summary>
    /// Holds disbursement information such as amount, 
    /// last transmit date, anticipated date
    /// </summary>
    [Serializable]
    public class StudentAwardDisbursement
    {
        /// <summary>
        /// Award period code
        /// </summary>
        public string AwardPeriodCode { get { return awardPeriodCode; } }
        private readonly string awardPeriodCode;
        /// <summary>
        /// The date of anticipated disbursement
        /// </summary>
        public DateTime? AnticipatedDisbursementDate { get { return anticipatedDisbursementDate; } }
        private readonly DateTime? anticipatedDisbursementDate;
        /// <summary>
        /// Last transmitted amount
        /// </summary>
        public decimal? LastTransmitAmount { get { return lastTransmitAmount; } }
        private readonly decimal? lastTransmitAmount;
        /// <summary>
        /// Last trasmittal date
        /// </summary>
        public DateTime? LastTransmitDate { get { return lastTransmitDate; } }
        private readonly DateTime? lastTransmitDate;

        /// <summary>
        /// StudentAwardDisbursement Constructor
        /// </summary>
        /// <param name="anticipatedDate"></param>
        /// <param name="lastTransmitAmount"></param>
        /// <param name="lastTransmitDate"></param>
        public StudentAwardDisbursement(string awardPeriodCode, DateTime? anticipatedDate, decimal? lastTransmitAmount, DateTime? lastTransmitDate)
        {
            if (string.IsNullOrEmpty(awardPeriodCode))
            {
                throw new ArgumentNullException("awardPeriodCode");
            }
            
            this.awardPeriodCode = awardPeriodCode;
            this.anticipatedDisbursementDate = anticipatedDate;
            this.lastTransmitAmount = lastTransmitAmount;
            this.lastTransmitDate = lastTransmitDate;
        }

    }
}
