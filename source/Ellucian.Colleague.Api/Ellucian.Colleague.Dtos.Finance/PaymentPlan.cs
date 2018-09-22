// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Finance
{
    /// <summary>
    /// Plan established for a student to pay for a set of charges on their account in installments
    /// </summary>
    public class PaymentPlan
    {
        /// <summary>
        /// ID of the payment plan
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// ID of the payment plan template used to create the payment plan
        /// </summary>
        public string TemplateId { get; set; }

        /// <summary>
        /// ID of the person for whom the payment plan was created
        /// </summary>
        public string PersonId { get; set; }

        /// <summary>
        /// Code of the AR Type for the charges on the payment plan
        /// </summary>
        public string ReceivableTypeCode { get; set; }
        
        /// <summary>
        /// ID of the term for which the payment plan was created
        /// </summary>
        public string TermId { get; set; }

        /// <summary>
        /// Amount of the payment plan at the time it was created
        /// </summary>
        public decimal OriginalAmount { get; set; }

        /// <summary>
        /// Due Date of the first scheduled payment (installment) for the payment plan
        /// </summary>
        public DateTime FirstDueDate { get; set; }

        /// <summary>
        /// Current amount of the payment plan
        /// </summary>
        public decimal CurrentAmount { get; set; }

        /// <summary>
        /// Interval at which the payment plan's scheduled payments are spaced
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public PlanFrequency Frequency { get; set; }

        /// <summary>
        /// The number of scheduled payments for the payment plan
        /// </summary>
        public int NumberOfPayments { get; set; }

        /// <summary>
        /// Flat amount of setup fee for the payment plan
        /// </summary>
        public decimal SetupAmount { get; set; }

        /// <summary>
        /// Percentage used to calculate variable amount of setup fee for the payment plan
        /// </summary>
        public decimal SetupPercentage { get; set; }

        /// <summary>
        /// Percentage of the plan amount required to be paid at the time the plan was created
        /// </summary>
        public decimal DownPaymentPercentage { get; set; }

        /// <summary>
        /// Number of days past a scheduled payment's due date that payments can be made before late fees are generated
        /// </summary>
        public int GraceDays { get; set; }

        /// <summary>
        /// Flat amount of late fee assessed to past due scheduled payments
        /// </summary>
        public decimal LateChargeAmount { get; set; }

        /// <summary>
        /// Percentage used to calculate variable amount of late fee assessed to past due scheduled payments
        /// </summary>
        public decimal LateChargePercentage { get; set; }

        /// <summary>
        /// List of a payment plan's statuses from the time it was created
        /// </summary>
        public IEnumerable<PlanStatus> Statuses { get; set; }

        /// <summary>
        /// List of IDs of scheduled payments associated with the payment plan
        /// </summary>
        public IEnumerable<ScheduledPayment> ScheduledPayments { get; set; }

        /// <summary>
        /// List of IDs of charges included on the payment plan
        /// </summary>
        public IEnumerable<PlanCharge> PlanCharges { get; set; }

        /// <summary>
        /// The current status of the payment plan
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public PlanStatusType CurrentStatus { get; set; }

        /// <summary>
        /// Date on which the payment plan was given it's current status
        /// </summary>
        public DateTime CurrentStatusDate { get; set; }

        /// <summary>
        /// The total amount of the setup charge for the plan
        /// </summary>
        public decimal TotalSetupChargeAmount { get; set; }

        /// <summary>
        /// Amount of the down payment for the plan
        /// </summary>
        public decimal DownPaymentAmount { get; set; }

        /// <summary>
        /// Amount of the payment made towards the down payment for the plan
        /// </summary>
        public decimal DownPaymentAmountPaid { get; set; }

        /// <summary>
        /// Gets the down payment date for the plan
        /// </summary>
        public DateTime? DownPaymentDate { get; set; }
    }
}
