﻿// Copyright 2016-2019 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// The General Ledger Configuration DTO.
    /// </summary>
    public class GeneralLedgerConfiguration
    {
        /// <summary>
        /// List of the major components in the GL account number.
        /// </summary>
        public List<GeneralLedgerComponent> MajorComponents { get; set; }

        /// <summary>
        /// List of the sub componentsin the GL account number.
        /// </summary>
        public List<GeneralLedgerComponent> SubComponents { get; set; }
    }
}
