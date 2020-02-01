// Copyright 2015-2019 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Additional information to convey for a tax form.
    /// </summary>
    public enum TaxFormNotations2
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
        /// Represents the second form when overflow data is present.
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
