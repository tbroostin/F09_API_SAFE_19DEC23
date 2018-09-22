// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Finance
{
    /// <summary>
    /// A charge, roughly equivalent to an AR invoice item
    /// </summary>
    public class Charge
    {
        /// <summary>
        /// ID of the charge
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// ID of the invoice to which the charge belongs
        /// </summary>
        public string InvoiceId { get; set; }

        /// <summary>
        /// AR Code associated with the charge
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Description of the charge
        /// </summary>
        public List<string> Description { get; set; }

        /// <summary>
        /// List of payment plans this charge is on
        /// </summary>
        public List<string> PaymentPlanIds { get; set; }

        /// <summary>
        /// Base amount of the charge, without taxes
        /// </summary>
        public decimal BaseAmount { get; set; }

        /// <summary>
        /// Amount of taxes associated with the charge
        /// </summary>
        public decimal TaxAmount { get; set; }

        /// <summary>
        /// Total amount of the charge
        /// </summary>
        public decimal Amount { get; set; }
    }
}
