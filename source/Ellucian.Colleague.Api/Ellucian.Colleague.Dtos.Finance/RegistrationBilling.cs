// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Finance
{
    /// <summary>
    /// Billing information tied to a student's registration
    /// </summary>
    public class RegistrationBilling
    {
        /// <summary>
        /// The type of AR account for which the invoice was created - roughly equivalent to AR Type
        /// </summary>
        public string AccountTypeCode { get; set; }

        /// <summary>
        /// Billing Period End Date for the registration billing
        /// </summary>
        public DateTime BillingEnd { get; set; }

        /// <summary>
        /// Billing Period Start Date for the registration billing
        /// </summary>
        public DateTime BillingStart { get; set; }

        /// <summary>
        /// ID of the registration billing
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// ID of the AR invoice associated with the registration billing
        /// </summary>
        public string InvoiceId { get; set; }

        /// <summary>
        /// List of registration billing items associated with the registration billing
        /// </summary>
        public List<RegistrationBillingItem> Items { get; set; }

        /// <summary>
        /// ID of the person for whom the registration billing was created
        /// </summary>
        public string PersonId { get; set; }

        /// <summary>
        /// ID of an adjustment to the registration billing
        /// </summary>
        public string AdjustmentId { get; set; }

        /// <summary>
        /// ID of the term in which the registration billing resides
        /// </summary>
        public string TermId { get; set; }
    }
}
