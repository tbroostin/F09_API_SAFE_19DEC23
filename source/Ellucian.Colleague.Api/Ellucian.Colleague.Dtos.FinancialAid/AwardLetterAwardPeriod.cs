//Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Award period information for an award letter
    /// </summary>
    public class AwardLetterAwardPeriod
    {
        /// <summary>
        /// The identifier of the award information contained in this object
        /// </summary>
        public string AwardId { get; set; }

        /// <summary>
        /// The description of this award to use when displaying this award.
        /// </summary>
        public string AwardDescription { get; set; }

        /// <summary>
        /// The amount of the award for the award period
        /// </summary>
        public decimal? AwardPeriodAmount { get; set; }

        /// <summary>
        /// The number describing which of the 6 columns this award period amount belongs to
        /// </summary>
        public int ColumnNumber { get; set; }

        /// <summary>
        /// The name of the column
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        /// The group number (1-3) of this award defining which group this award belongs to.
        /// </summary>
        public int GroupNumber { get; set; }

        /// <summary>
        /// The group name of this award
        /// </summary>
        public string GroupName { get; set; }
    }
}
