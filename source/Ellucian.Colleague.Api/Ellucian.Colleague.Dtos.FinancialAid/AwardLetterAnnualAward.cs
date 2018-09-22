//Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// List of annual Award information
    /// </summary>
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
        public decimal? AnnualAwardAmount { get; set; }

        /// <summary>
        /// The group number (1-3) of this award defining which group this award belongs to.
        /// </summary>
        public int GroupNumber { get; set; }

        /// <summary>
        /// The award category group name the award belongs to
        /// </summary>
        public string GroupName { get; set; }
 
        /// <summary>
        /// List of award period (column) associated data for the display of an award letter.
        /// </summary>
        public List<AwardLetterAwardPeriod> AwardLetterAwardPeriods { get; set; }

    }
}
