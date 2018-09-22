using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Encapsulates a single row in the award table of the award letter. This DTO supports up to six award period columns.
    /// </summary>
    public class AwardLetterAward
    {
        /// <summary>
        /// The name of the group that this award is grouped under
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// The sequence number of the group that this award is grouped under
        /// </summary>
        public int GroupNumber { get; set; }

        /// <summary>
        /// The identifier of the award information contained in this object
        /// </summary>
        public string AwardId { get; set; }

        /// <summary>
        /// The description of the award contained in this object
        /// </summary>
        public string AwardDescription { get; set; }

        /// <summary>
        /// The total amount of all the award period columns.
        /// </summary>
        public decimal Total { get; set; }

        /// <summary>
        /// The amount of the first award period
        /// </summary>
        public decimal Period1Amount { get; set; }

        /// <summary>
        /// The amount of the second award period
        /// </summary>
        public decimal Period2Amount { get; set; }

        /// <summary>
        /// The amount of the third award period
        /// </summary>
        public decimal Period3Amount { get; set; }

        /// <summary>
        /// The amount of the fourth award period
        /// </summary>
        public decimal Period4Amount { get; set; }

        /// <summary>
        /// The amount of the fifth award period
        /// </summary>
        public decimal Period5Amount { get; set; }

        /// <summary>
        /// The amount of the sixth award period
        /// </summary>
        public decimal Period6Amount { get; set; }
    }


}
