// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// A General Ledger Component DTO.
    /// </summary>
    public class GeneralLedgerComponent
    {
        /// <summary>
        /// The name of the GL component.
        /// </summary>
        public string ComponentName { get; set; }

        /// <summary>
        /// The length of the GL component.
        /// </summary>
        public int ComponentLength { get; set; }

        /// <summary>
        /// The start position of the GL component in the GL account number.
        /// </summary>
        public int StartPosition { get; set; }
    }
}
