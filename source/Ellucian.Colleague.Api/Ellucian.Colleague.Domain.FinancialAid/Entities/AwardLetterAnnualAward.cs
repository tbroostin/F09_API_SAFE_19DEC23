//Copyright 2015-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// List of annual Award information
    /// </summary>
    [Serializable]
    public class AwardLetterAnnualAward
    {
        /// <summary>
        /// The identifier of the award information contained in this object.
        /// </summary>
        public string AwardId { get; set; }

        /// <summary>
        /// The description of this award to use when displaying this award.
        /// </summary>
        public string AwardDescription { get; set; }

        /// <summary>
        /// The annual amount for this award.
        /// </summary>
        public decimal? AnnualAnnualAmount { get; set; }

        /// <summary>
        /// The group number (1-3) of this award defining which group this award belongs to.
        /// </summary>
        public int GroupNumber { get; set; }

        /// <summary>
        /// The group name of this award
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// List of award period (column) associated data for the display of an award letter.
        /// </summary>
        public List<AwardLetterAwardPeriod> AwardLetterAwardPeriods { get; set; }

        /// <summary>
        /// Flag that determines if the award is renewable
        /// </summary>
        public string AwRenewableFlag { get; set; }

        /// <summary>
        /// Renewable text for the selected award
        /// </summary>
        public string AwRenewableText { get; set; }

        /// <summary>
        /// Constructor for Annual Award information group
        /// </summary>
        public AwardLetterAnnualAward()
        {
            AwardLetterAwardPeriods = new List<AwardLetterAwardPeriod>();
        }
    }
}
