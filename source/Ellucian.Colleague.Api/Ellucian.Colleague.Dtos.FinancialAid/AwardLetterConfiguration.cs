//Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Award letter configuration that contains award letter parameters data
    /// </summary>
    public class AwardLetterConfiguration
    {
        /// <summary>
        /// AwardLetterConfiguration id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Flag to indicate whether contact block should be included on the award letter
        /// </summary>
        public bool IsContactBlockActive { get; set; }

        /// <summary>
        /// Flag to indicate whether to display the housing code on the award letter
        /// </summary>
        public bool IsHousingBlockActive { get; set; }

        /// <summary>
        /// Flag to indicate whether EFC, Need, and Budget should be included on the
        /// award letter
        /// </summary>
        public bool IsNeedBlockActive { get; set; }

        /// <summary>
        /// Paragraph spacing for the opening and closing paragraphs of an award letter:
        /// single or double; single by default
        /// </summary>
        public string ParagraphSpacing { get; set; }

        /// <summary>
        /// Award letter awards table title;
        /// defaults to "Awards"
        /// </summary>
        public string AwardTableTitle { get; set; }

        /// <summary>
        /// Award total title for the award letter awards table;
        /// defaults to "Total"
        /// </summary>
        public string AwardTotalTitle { get; set; }

        /// <summary>
        /// Collection of award categories row groups
        /// </summary>
        public IEnumerable<AwardLetterGroup> AwardCategoriesGroups { get; set; }

        /// <summary>
        /// Collection of award period column groups
        /// </summary>
        public IEnumerable<AwardLetterGroup> AwardPeriodsGroups { get; set; }
    }
}
