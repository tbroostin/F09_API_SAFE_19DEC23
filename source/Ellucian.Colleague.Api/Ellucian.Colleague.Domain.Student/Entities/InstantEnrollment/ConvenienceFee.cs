// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment
{
    /// <summary>
    /// A convenience fee
    /// </summary>
    [Serializable]
    public class ConvenienceFee
    {
        /// <summary>
        /// Convenience fee code
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Convenience fee description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Convenience fee amount
        /// </summary>
        public Nullable<Decimal> Amount { get; set; }
    }
}
