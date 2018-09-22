// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Enumeration of amount value expressed as either a invoice or refund.
    /// </summary>
    [Serializable]
    public enum InvoiceOrRefund
    {
        NotSet,

        /// <summary>
        /// Invoice
        /// </summary>
        Invoice,

        /// <summary>
        /// Refund
        /// </summary>
       Refund
    }
}