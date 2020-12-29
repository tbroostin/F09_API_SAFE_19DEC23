// Copyright 2015-2019 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Additional information to convey for a tax form.
    /// </summary>
    [Serializable]
    public enum TaxFormNotations
    {
        /// <summary>
        /// No additional information to convey.
        /// </summary>
        None,

        /// <summary>
        /// A correction form has been issued.
        /// </summary>
        Correction,

        /// <summary>
        /// Multiple forms have been issued.
        /// </summary>
        MultipleForms,

        /// <summary>
        /// A second form is required to present all data.
        /// </summary>
        HasOverflowData,

        /// <summary>
        /// Represents the second form when overflow data is present
        /// </summary>
        IsOverflow,

        /// <summary>
        /// The tax form statement is not available.
        /// </summary>
        NotAvailable,

        /// <summary>
        /// The tax form statement is marked as cancelled.
        /// </summary>
        Cancelled
    }
}
