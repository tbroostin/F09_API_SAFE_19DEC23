﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Represents a request to create/update new purchase Order
    /// </summary>
    [Serializable]
    public class PurchaseOrderCreateUpdateRequest
    {
        /// <summary>
        /// Person ID
        /// </summary>
        public string PersonId { get; set; }

        /// <summary>
        /// The Purchase Order initiator initials
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
        /// The Purchase Order object
        /// </summary>
        public PurchaseOrder PurchaseOrder { get; set; }
    }
}
