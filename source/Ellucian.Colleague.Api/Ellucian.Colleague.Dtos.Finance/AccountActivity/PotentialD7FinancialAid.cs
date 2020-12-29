// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.Finance.AccountActivity
{
    /// <summary>
    /// A token representing an award period and a D7 Financial Aid award concatenated together with an asterisk,
    /// a description of the award, and a potentially transmittable amount for the award.
    /// The award and amount have not been transmitted.  This is an estimate only, based
    /// on current charges and payments. 
    /// </summary>
    /// 
    public class PotentialD7FinancialAid
    {
        /// <summary>
        /// The award period and award code of the D7 award, concatenated together with an asterisk
        /// </summary>
        public string AwardPeriodAward { get; set; }

        /// <summary>
        /// The description of the award code comprising the AwardPeriodAward
        /// </summary>
        public string AwardDescription { get; set; }

        /// <summary>
        /// The potential amount of the award
        /// </summary>
        /// 
        public decimal AwardAmount { get; set; }
    }
}
