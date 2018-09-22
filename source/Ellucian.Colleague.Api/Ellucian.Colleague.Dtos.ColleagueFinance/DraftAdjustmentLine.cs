// Copyright 2018 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// A single line in a draft budget adjustment.
    /// </summary>
    public class DraftAdjustmentLine
    {
        /// <summary>
        /// GL number from or to which money is being moved.
        /// </summary>
        public string GlNumber { get; set; }

        /// <summary>
        /// Amount of money being moved out of the account.
        /// </summary>
        public decimal FromAmount { get; set; }

        /// <summary>
        /// Amount of money being moved into the account.
        /// </summary>
        public decimal ToAmount { get; set; }
    }
}