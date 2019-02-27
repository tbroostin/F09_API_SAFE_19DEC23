﻿// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Enumeration of funds available statuses.
    /// Used in CheckAvailableFunds for the Higher Education Data Model.
    /// </summary>
    [Serializable]
    public enum FundsAvailableStatus
    {
        /// <summary>
        /// Available
        /// </summary>
        Available,

        /// <summary>
        /// NotAvailable
        /// </summary>
        NotAvailable,

        /// <summary>
        /// Override
        /// </summary>
        Override,

        /// <summary>
        /// Invalid accounting string
        /// </summary>
        Invalid,

        /// <summary>
        /// notApplicable
        /// </summary>
        NotApplicable
    }
}