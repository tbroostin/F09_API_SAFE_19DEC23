// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// These are the available codes for a Vendor Hold Reason
    /// </summary>
    [Serializable]
    public enum VendorHoldReasonCodes
    {
        /// <summary>
        /// Out of Business
        /// </summary>
        Ob,
        /// <summary>
        /// Vendor Discontinued
        /// </summary>
        Disc,
        /// <summary>
        /// Quality Hold
        /// </summary>
        Qual,
        /// <summary>
        /// Disputed Transactions 
        /// </summary>
        Disp
    }
}
