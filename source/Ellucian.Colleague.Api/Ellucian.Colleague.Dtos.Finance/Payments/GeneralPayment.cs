// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Dtos.Finance.Payments
{
    /// <summary>
    /// A general (non-AR) payment
    /// </summary>
    public class GeneralPayment
    {
        /// <summary>
        /// Payment code
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Payment code description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Payment location code
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Payment location description
        /// </summary>
        public string LocationDescription { get; set; }

        /// <summary>
        /// Payment amount
        /// </summary>
        public Nullable<Decimal> NetAmount { get; set; }
    }
}
