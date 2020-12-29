// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Finance.Entities.AccountActivity
{
    /// <summary>
    /// An estimated D7 Financial Aid award and corresponding amount.
    /// The award and amount have not been transmitted.  This is an estimate only, based
    /// on current charges and payments. 
    /// </summary>
    [Serializable]
    public class PotentialD7FinancialAid
    {
        private readonly string _awardPeriodAward;
        private readonly string _awardDescription;
        private readonly decimal _awardAmount;
        /// <summary>
        /// The award period and award code of the D7 award, represented as a single token having an asterisk as a seperator
        /// </summary>
        public string AwardPeriodAward { get { return _awardPeriodAward; } }

        /// <summary>
        /// The description of the award code comprising the AwardPeriodAward
        /// </summary>
        public string AwardDescription { get { return _awardDescription; } }
        /// <summary>
        /// The potential amount of the award
        /// </summary>
        public decimal AwardAmount { get { return _awardAmount; } }

        /// <summary>
        /// Constructor for a PotentialD7FinancialAid
        /// </summary>
        /// <param name="awardPeriodAward">The code of the D7 award.  Must contain a non-empty value.</param>
        /// <param name="awardDescription">The description of the award code comprising the AwardPeriodAward</param>
        /// <param name="awardAmount">The potential amount of the award comprising the AwardPeriodAward.  Can be zero.</param>
        public PotentialD7FinancialAid(string awardPeriodAward, string awardDescription, decimal awardAmount)
        {
            if (string.IsNullOrEmpty(awardPeriodAward))
            {
                throw new ArgumentNullException("awardPeriodAward");
            }

            if (string.IsNullOrEmpty(awardDescription))
            {
                throw new ArgumentNullException("awardDescription");
            }

            _awardPeriodAward = awardPeriodAward;
            _awardDescription = awardDescription;
            _awardAmount = awardAmount;
        }
    }
}
