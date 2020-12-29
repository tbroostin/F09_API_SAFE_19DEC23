// Copyright 2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Represents a request to create/update new requisition
    /// </summary>
    [Serializable]
    public class RequisitionCreateUpdateRequest
    {
        /// <summary>
        /// Person ID
        /// </summary>
        public string PersonId { get; set; }

        /// <summary>
        /// The requisition initiator initials
        /// </summary>
        public string InitiatorInitials { get; set; }

        /// <summary>
        /// List of email address for which confirmation mail will be sent.
        /// </summary>
        public List<string> ConfEmailAddresses { get; set; }

        /// <summary>
        /// Flag to determine if vendor is person
        /// </summary>
        public bool IsPersonVendor { get; set; }

        /// <summary>
        /// The requisition object
        /// </summary>
        public Requisition Requisition { get; set; }
    }
}
