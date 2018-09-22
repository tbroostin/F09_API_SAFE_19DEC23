// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Finance
{
    /// <summary>
    /// A charge against a receivable account
    /// </summary>
    public class ReceivableCharge
    {
        /// <summary>
        /// ID of charge
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Invoice to which this charge belongs
        /// </summary>
        public string InvoiceId { get; set; }

        /// <summary>
        /// Charge description
        /// </summary>
        public List<string> Description { get; set; }

        /// <summary>
        /// Charge code
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Collection of IDs to the Allocation objects that pay on this charge
        /// </summary>
        public List<string> AllocationIds { get; set; }

        /// <summary>
        /// Collection of IDs to the payment plans containing this charge
        /// </summary>
        public List<string> PaymentPlanIds { get; set; }

        /// <summary>
        /// Base amount of charge (pre-tax amount)
        /// </summary>
        public decimal BaseAmount { get; set; }

        /// <summary>
        /// Amount of tax on this charge
        /// </summary>
        public decimal TaxAmount { get; set; }
    }
}
