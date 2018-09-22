// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Dtos.Finance.Payments
{
    /// <summary>
    /// A payment on an AR account
    /// </summary>
    public class AccountsReceivablePayment
    {
        /// <summary>
        /// Account holder ID
        /// </summary>
        public string PersonId { get; set; }

        /// <summary>
        /// Account holder name
        /// </summary>
        public string PersonName { get; set; }

        /// <summary>
        /// AR type code
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// AR type description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Term code
        /// </summary>
        public string Term { get; set; }

        /// <summary>
        /// Term description
        /// </summary>
        public string TermDescription { get; set; }

        /// <summary>
        /// Location code
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Location description
        /// </summary>
        public string LocationDescription { get; set; }

        /// <summary>
        /// Payment description
        /// </summary>
        public string PaymentDescription { get; set; }

        /// <summary>
        /// Payment amount
        /// </summary>
        public Nullable<Decimal> NetAmount { get; set; }

        /// <summary>
        /// Registration payment control ID
        /// </summary>
        public string PaymentControlId { get; set; }
    }
}
