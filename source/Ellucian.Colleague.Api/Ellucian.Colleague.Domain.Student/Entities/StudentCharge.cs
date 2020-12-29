// Copyright 2016-2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// This is a transaction posted to Misc. Charges through the Data Model.
    /// </summary>
    [Serializable]
    public class StudentCharge
    {
        /// <summary>
        /// Unique identifier (GUID) for Misc. Charges transaction.
        /// </summary>
        public string Guid { get; set; }

        /// <summary>
        /// Person ID for this charge.
        /// </summary>
        public string PersonId { get; private set; }

        /// <summary>
        /// AR Type Code used in this charge.
        /// </summary>
        public string AccountsReceivableTypeCode { get; set; }

        /// <summary>
        /// The AR Code to use for this charge.
        /// </summary>
        public string AccountsReceivableCode { get; set; }

        /// <summary>
        /// The Academic Term to associate the charge to.
        /// </summary>
        public string Term { get; set; }

        /// <summary>
        /// The charge type.  One of, "tuition", "fee", "housing", or "meal".
        /// </summary>
        public string ChargeType { get; set; }

        /// <summary>
        /// The Due Date or the date that this is charged on.
        /// </summary>
        public DateTime ChargeDate { get; private set; }

        /// <summary>
        /// Comments to be recorded with the charge.
        /// </summary>
        public List<string> Comments { get; set; }

        /// <summary>
        /// Charged Amount (exclusive of Quantity).
        /// </summary>
        public decimal? ChargeAmount { get; set; }

        /// <summary>
        /// Charged Currency (only USD and CAD are supported).
        /// </summary>
        public string ChargeCurrency { get; set; }

        /// <summary>
        /// Charged quantity if by units.
        /// </summary>
        public long? UnitQuantity { get; set; }

        /// <summary>
        /// Charged amount if by units.
        /// </summary>
        public decimal? UnitCost { get; set; }

        /// <summary>
        /// Charged Currency if by units (only USD and CAD are supported).
        /// </summary>
        public string UnitCurrency { get; set; }

        /// <summary>
        /// Once posted to Misc. Charges, this is the key to AR.INVOICES.
        /// </summary>
        public string InvoiceItemID { get; set; }

        /// <summary>
        /// Send to CTX if the payment was from elvate or not. A true value is
        /// from Elevate
        /// </summary>
        public bool ChargeFromElevate { get; set; }

        /// <summary>
        /// The usage associated with the charge (i.e. tax reporting only)
        /// </summary>
        public string Usage { get; set; }

        /// <summary>
        /// The date the charge orginated for sonsideration in tax report generation.
        /// </summary>
        public DateTime? OriginatedOn { get; set; }

        /// <summary>
        /// The override description associated with the charge.
        /// </summary>
        public string OverrideDescription { get; set; }

        /// <summary>
        /// The start date of the activity associated with the charge.
        /// </summary>
        public DateTime? BillingStartDate { get; set; }

        /// <summary>
        /// he end date of the activity associated with the charge.
        /// </summary>
        public DateTime? BillingEndDate { get; set; }

        /// <summary>
        /// Constructor initializes the StudentCharges transaction object.
        /// </summary>
        
        public StudentCharge(string personId, string chargeType, DateTime? chargeDate)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "Person ID is required when creating a StudentCharge.");
            }
            if (string.IsNullOrEmpty(chargeType))
            {
                throw new ArgumentNullException("chargeType", "Charge Type is required when creating a StudentCharge.");
            }
            if (!chargeDate.HasValue)
            {
                chargeDate = DateTime.Today;
            }
            PersonId = personId;
            ChargeType = chargeType;
            ChargeDate = chargeDate.Value;
            Comments = new List<string>();
        }

        /// <summary>
        /// Constructor initializes the StudentCharges transaction object.
        /// </summary>

        public StudentCharge(string personId, DateTime? chargeDate)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "Person ID is required when creating a StudentCharge.");
            }
            if (!chargeDate.HasValue)
            {
                chargeDate = DateTime.Today;
            }
            PersonId = personId;
            ChargeDate = chargeDate.Value;
            Comments = new List<string>();
        }
    }
}
