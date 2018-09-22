// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// The possible values of the status applied to an individual waiver item
    /// </summary>
    [Serializable]
    public enum WaiverStatus
    {
        /// <summary>
        /// No action has been taken on the item--it is neither waived nor denied
        /// </summary>
        NotSelected,
        /// <summary>
        /// Item has been waived
        /// </summary>
        Waived,
        /// <summary>
        /// Waiver has been denied for the item
        /// </summary>
        Denied
    }
}
