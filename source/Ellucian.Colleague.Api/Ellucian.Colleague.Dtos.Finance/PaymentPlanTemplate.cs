// Copyright 2014-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Finance
{
    /// <summary>
    /// Template for creating a payment plan for an accountholder
    /// </summary>
    public class PaymentPlanTemplate
    {
        /// <summary>
        /// ID of the payment plan template
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Description of the payment plan template
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Determines whether the payment plan template may be used to create a payment plan
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Frequency of scheduled payments for payment plans created from the template
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public PlanFrequency Frequency { get; set; }

        /// <summary>
        /// Number of scheduled payments for payment plans created from the template
        /// </summary>
        public int NumberOfPayments { get; set; }

        /// <summary>
        /// Minimum amount for which a payment plan may be created from the template
        /// </summary>
        public decimal MinimumPlanAmount { get; set; }

        /// <summary>
        /// Maximum amount for which a payment plan may be created from the template
        /// </summary>
        public decimal? MaximumPlanAmount { get; set; }

        /// <summary>
        /// Subroutine used in calculating custom payment plan frequency
        /// </summary>
        public string CustomFrequencySubroutine { get; set; }

        /// <summary>
        /// ID of document specifying the terms and conditions for a payment plan
        /// </summary>
        public string TermsAndConditionsDocumentId { get; set; }

        /// <summary>
        /// Flat amount of setup fee for payment plan
        /// </summary>
        public decimal SetupChargeAmount { get; set; }

        /// <summary>
        /// Percentage used in calculating variable amount of setup fee for payment plan
        /// </summary>
        public decimal SetupChargePercentage { get; set; }

        /// <summary>
        /// Percentage used in calculating down payment for payment plan
        /// </summary>
        public decimal DownPaymentPercentage { get; set; }

        /// <summary>
        /// Number of days until payment plan down payment is due
        /// </summary>
        public int DaysUntilDownPaymentIsDue { get; set; }

        /// <summary>
        /// Number of days that a payment plan payment may be made past its due date before a late fee is assessed
        /// </summary>
        public int GraceDays { get; set; }

        /// <summary>
        /// Flat amount of late fee for overdue scheduled payments
        /// </summary>
        public decimal LateChargeAmount { get; set; }

        /// <summary>
        /// Percentage used in calculating amount of variable late fee for overdue scheduled payments
        /// </summary>
        public decimal LateChargePercentage { get; set; }

        /// <summary>
        /// Determines whether the payment plan setup fee is included in the first scheduled payment amount
        /// </summary>
        public bool IncludeSetupChargeInFirstPayment { get; set; }

        /// <summary>
        /// Determines whether anticipated financial aid is considered in calculating the amount of a payment plan
        /// </summary>
        public bool SubtractAnticipatedFinancialAid { get; set; }

        /// <summary>
        /// Determines if payment plan amounts are calculated automatically
        /// </summary>
        public bool CalculatePlanAmountAutomatically { get; set; }

        /// <summary>
        /// Determines if payment plan is modified automatically when plan charges are adjusted
        /// </summary>
        public bool ModifyPlanAutomatically { get; set; }

        /// <summary>
        /// Collection of receivable type codes for which charges may be assigned to a payment plan created from the template
        /// </summary>
        public List<string> AllowedReceivableTypeCodes { get; set; }

        /// <summary>
        /// Collection of IDs of invoice exclusion rules that an invoice must fail in order to be assigned to a payment plan created from the template
        /// </summary>
        public List<string> InvoiceExclusionRuleIds { get; set; }

        /// <summary>
        /// Collection of IDs of charge codes that may be assigned to a payment plan created from the template
        /// </summary>
        public List<string> IncludedChargeCodes { get; set; }

        /// <summary>
        /// Collection of IDs of charge codes that may not be assigned to a payment plan created from the template
        /// </summary>
        public List<string> ExcludedChargeCodes { get; set; }

        /// <summary>
        /// Gets the down payment date for a plan
        /// </summary>
        public DateTime? DownPaymentDate { get; set; }
    }
}
