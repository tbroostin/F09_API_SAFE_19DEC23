// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment
{
    /// <summary>
    /// Processing status of an instant enrollment registration e-commerce transaction
    /// </summary>
    [Serializable]

    public enum EcommerceProcessStatus
    {
        /// <summary>
        /// No status for transactions that are not e-commerce transactions
        /// </summary>
        None,
        /// <summary>
        /// The transaction was successfully processed
        /// </summary>
        Success,
        /// <summary>
        /// The transaction was not successfully processed
        /// </summary>
        Failure,
        /// <summary>
        /// The transaction was canceled by the user
        /// </summary>
        Canceled,
    }
}
