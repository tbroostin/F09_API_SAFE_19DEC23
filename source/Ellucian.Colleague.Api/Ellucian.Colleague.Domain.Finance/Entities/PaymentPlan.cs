// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    [Serializable]
    public class PaymentPlan
    {
        // Required fields
        private string _id;
        private readonly string _templateId;
        private readonly string _personId;
        private readonly string _receivableTypeCode;
        private readonly string _termId;    // Term not required, but cannot be changed after plan is created
        private readonly decimal _originalAmount;
        private readonly DateTime _firstDueDate;

        // Protected fields - not required, but updates are limited
        private readonly List<PlanStatus> _statuses = new List<PlanStatus>();
        private readonly List<ScheduledPayment> _scheduledPayments = new List<ScheduledPayment>();
        private readonly List<PlanCharge> _planCharges = new List<PlanCharge>();

        // Properties for required and protected fields

        /// <summary>
        /// ID of the payment plan
        /// </summary>
        public string Id 
        { 
            get { return _id; }
            set
            {
                if (!string.IsNullOrEmpty(_id))
                {
                    throw new InvalidOperationException("ID already defined for payment plan.");
                }
                _id = value;
                // Update the plan ID on plan charges and scheduled payments
                for (int i = 0; i < _planCharges.Count; i++)
                {
                    _planCharges[i].PlanId = _id;
                }
                for (int i = 0; i < _scheduledPayments.Count; i++)
                {
                    _scheduledPayments[i].PlanId = _id;
                }
            }
        }

        /// <summary>
        /// ID of the template used to create the payment plan
        /// </summary>
        public string TemplateId { get { return _templateId; } }

        /// <summary>
        /// ID of the person for whom the payment plan was created
        /// </summary>
        public string PersonId { get { return _personId; } }

        /// <summary>
        /// ID of the receivable type for which the plan was created
        /// </summary>
        public string ReceivableTypeCode { get { return _receivableTypeCode; } }

        /// <summary>
        /// ID of the term for which the plan was created
        /// </summary>
        public string TermId { get { return _termId; } }

        /// <summary>
        /// Amount for which the plan was originally created
        /// </summary>
        public decimal OriginalAmount { get { return _originalAmount; } }

        /// <summary>
        /// Date on which first scheduled payment is due
        /// </summary>
        public DateTime FirstDueDate { get { return _firstDueDate; } }

        /// <summary>
        /// Current plan amount
        /// </summary>
        public decimal CurrentAmount { get; set; }

        /// <summary>
        /// Frequency of scheduled payments
        /// </summary>
        public PlanFrequency Frequency { get; set; }

        /// <summary>
        /// Number of scheduled payments
        /// </summary>
        public int NumberOfPayments { get; set; }

        /// <summary>
        /// Flat amount of setup fee
        /// </summary>
        public decimal SetupAmount { get; set; }

        /// <summary>
        /// Percentage used in calculating variable amount of setup fee
        /// </summary>
        public decimal SetupPercentage { get; set; }

        /// <summary>
        /// Percentage of plan amount in down payment
        /// </summary>
        public decimal DownPaymentPercentage { get; set; }

        /// <summary>
        /// Number of days that a payment may be overdue before a late fee is assessed
        /// </summary>
        public int GraceDays { get; set; }

        /// <summary>
        /// Flat amount of late fee assessed on overdue payments
        /// </summary>
        public decimal LateChargeAmount { get; set; }

        /// <summary>
        /// Percentage used in calculating amount of variable late fee assessed on overdue payments
        /// </summary>
        public decimal LateChargePercentage { get; set; }

        /// <summary>
        /// Plan statuses and their dates
        /// </summary>
        public ReadOnlyCollection<PlanStatus> Statuses { get; private set; }

        /// <summary>
        /// IDs of scheduled payments on the plan
        /// </summary>
        public ReadOnlyCollection<ScheduledPayment> ScheduledPayments { get; private set; }

        /// <summary>
        /// IDs of charges included on the plan
        /// </summary>
        public ReadOnlyCollection<PlanCharge> PlanCharges { get; private set; }

        //------ Computed properties ------//

        /// <summary>
        /// Current status of the plan
        /// </summary>
        public PlanStatusType? CurrentStatus { get { return _statuses == null || _statuses.Count == 0 ? (PlanStatusType?)null : _statuses[0].Status; } }

        /// <summary>
        /// Date on which the plan was given its current status
        /// </summary>
        public DateTime? CurrentStatusDate { get { return _statuses[0].Date; } }

        /// <summary>
        /// The total amount of the setup charge for the plan
        /// </summary>
        public decimal TotalSetupChargeAmount { get { return Math.Round(SetupAmount + (SetupPercentage * CurrentAmount / 100), 2, MidpointRounding.AwayFromZero); } }

        /// <summary>
        /// Amount of the down payment for the plan
        /// </summary>
        public decimal DownPaymentAmount { get { return (DownPaymentPercentage == 0) ? 0 : ScheduledPayments[0].Amount; } }

        /// <summary>
        /// Amount of the payment made towards the down payment for the plan
        /// </summary>
        public decimal DownPaymentAmountPaid { get { return (DownPaymentPercentage == 0) ? 0 : ScheduledPayments[0].AmountPaid; } }

        /// <summary>
        /// Gets the down payment date for the plan
        /// </summary>
        public DateTime? DownPaymentDate { get { return (DownPaymentPercentage == 0) ? (DateTime?)null : ScheduledPayments[0].DueDate; } }

        /// <summary>
        /// Constructor for the payment plan entity
        /// </summary>
        /// <param name="id">ID of the plan</param>
        /// <param name="templateId">ID of the template used to create the plan</param>
        /// <param name="personId">ID of the person for whom the plan was created</param>
        /// <param name="receivableTypeCode">Receivable type code for charges on the plan</param>
        /// <param name="termId">ID of the term for which the plan was created</param>
        /// <param name="originalAmount">Original amount of the plan</param>
        /// <param name="firstDueDate">Date on which first scheduled payment is due</param>
        /// <param name="planStatuses">List of plan statuses</param>
        public PaymentPlan(string id, string templateId, string personId, string receivableTypeCode, string termId, decimal originalAmount, 
            DateTime firstDueDate, IEnumerable<PlanStatus> planStatuses, IEnumerable<ScheduledPayment> schedules, IEnumerable<PlanCharge> charges)
        {
            if (string.IsNullOrEmpty(templateId))
            {
                throw new ArgumentNullException("templateId", "Template ID cannot be null/empty");
            }
            if (originalAmount <= 0)
            {
                throw new ArgumentOutOfRangeException("originalAmount", "Payment Plan original amount must be greater than zero.");
            }
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "Person ID cannot be null/empty");
            }
            if (string.IsNullOrEmpty(receivableTypeCode))
            {
                throw new ArgumentNullException("receivableTypeCode", "AR Type cannot be null/empty");
            }

            // If an ID is provided, then the plan should already have scheduled payments and plan charges to go on it.
            if (!string.IsNullOrEmpty(id) && (schedules == null || schedules.Count() == 0))
            {
                throw new ArgumentNullException("schedules", "A plan with an ID must have scheduled payments.");
            }
            if (!string.IsNullOrEmpty(id) && (charges == null || charges.Count() == 0))
            {
                throw new ArgumentNullException("charges", "A plan with an ID must have plan charges.");
            }

            // If an ID is not provided, then no scheduled payments or plan charges should be included
            //if (string.IsNullOrEmpty(id) && schedules != null && schedules.Count() > 0)
            //{
            //    throw new ArgumentException("Scheduled payments cannot be included if the plan ID is not assigned.", "schedules");
            //}
            //if (string.IsNullOrEmpty(id) && charges != null && charges.Count() > 0)
            //{
            //    throw new ArgumentException("Plan charges cannot be included if the plan ID is not assigned.", "charges");
            //}

            _id = id;
            _templateId = templateId;
            _personId = personId;
            _receivableTypeCode = receivableTypeCode;
            _termId = termId;
            _originalAmount = originalAmount;
            _firstDueDate = firstDueDate;

            if (planStatuses == null || planStatuses.Count() == 0)
            {
                // Initial status is Open with a status date of the current date
                AddStatus(new PlanStatus(PlanStatusType.Open, DateTime.Today));
            }
            else
            {
                _statuses.InsertRange(0, planStatuses);
            }

            // If scheduled payments and/or plan charges were provided, add them to the plan
            if (schedules != null && schedules.Count() > 0)
            {
                foreach (var schedule in schedules)
                {
                    AddScheduledPayment(schedule);
                }
            }
            if (charges != null && charges.Count() > 0)
            {
                foreach (var charge in charges)
                {
                    AddPlanCharge(charge);
                }
            }

            // The plan's current amount is the original amount of the plan at the time it is created
            CurrentAmount = _originalAmount;

            Statuses = _statuses.AsReadOnly();
            PlanCharges = _planCharges.AsReadOnly();
            ScheduledPayments = _scheduledPayments.AsReadOnly();
        }

        /// <summary>
        /// Adds a status to a plan
        /// </summary>
        /// <param name="status">Plan status</param>
        /// <param name="date">Date on which plan status became effective</param>
        public void AddStatus(PlanStatus planStatus)
        {
            if (planStatus == null)
            {
                throw new ArgumentNullException("planStatus", "Plan Status cannot be null.");
            }
            // If the current status is cancelled, don't touch the plan - it should NOT be reopend
            if (CurrentStatus == PlanStatusType.Cancelled)
            {
                throw new InvalidOperationException("You cannot change the status of a cancelled plan.");
            }
            // Statuses can have duplicates so don't look for unique values
            _statuses.Insert(0, planStatus);
        }

        /// <summary>
        /// Adds a plan charge to the plan
        /// </summary>
        /// <param name="planCharge">Plan Charge</param>
        public void AddPlanCharge(PlanCharge planCharge)
        {
            if (planCharge == null)
            {
                throw new ArgumentNullException("planCharge", "Plan charge must be specified.");
            }

            // Do not allow a plan charge to be added if its plan ID does not match the ID of the plan
            if (string.IsNullOrEmpty(planCharge.PlanId) && !string.IsNullOrEmpty(_id))
            {
                throw new ArgumentOutOfRangeException("planCharge", "Plan ID is null/empty on plan charge being added, but existing plan ID is not null/empty.");
            }

            if (!string.IsNullOrEmpty(planCharge.PlanId) && string.IsNullOrEmpty(_id))
            {
                throw new ArgumentOutOfRangeException("planCharge", "Plan charge has ID " + planCharge.PlanId + " but plan ID has not been assigned.");
            }
                
            if (!string.IsNullOrEmpty(planCharge.PlanId) && !string.IsNullOrEmpty(_id) && planCharge.PlanId != _id)
            {
                throw new ArgumentOutOfRangeException("planCharge", "Plan ID " + planCharge.PlanId + " does not match Plan ID " + _id);
            }

            // No duplicates allowed
            if (_planCharges.Contains(planCharge))
            {
                return;
            }

            _planCharges.Add(planCharge);
        }

        /// <summary>
        /// Adds a scheduled payment ID to the plan
        /// </summary>
        /// <param name="scheduledPayment">ID of the scheduled payment</param>
        public void AddScheduledPayment(ScheduledPayment scheduledPayment)
        {
            if (scheduledPayment == null)
            {
                throw new ArgumentNullException("scheduledPayment", "Scheduled payment must be specified.");
            }

            // Do not allow a plan charge to be added if its plan ID does not match the ID of the plan
            if (string.IsNullOrEmpty(scheduledPayment.PlanId) && !string.IsNullOrEmpty(_id))
            {
                throw new ArgumentOutOfRangeException("scheduledPayment", "Plan ID is null/empty on scheduled payment being added, but ID of plan is " + _id + ", not null/empty.");
            }

            if (!string.IsNullOrEmpty(scheduledPayment.PlanId) && string.IsNullOrEmpty(_id))
            {
                throw new ArgumentOutOfRangeException("scheduledPayment", "Scheduled payment has plan ID " + scheduledPayment.PlanId + " but plan ID has not been assigned.");
            }

            if (!string.IsNullOrEmpty(scheduledPayment.PlanId) && !string.IsNullOrEmpty(_id) && scheduledPayment.PlanId != _id)
            {
                throw new ArgumentOutOfRangeException("scheduledPayment", "Plan ID " + scheduledPayment.PlanId + " on the scheduled payment does not match Plan ID " + _id);
            }

            // No duplicates allowed - just ignore them
            if (_scheduledPayments.Contains(scheduledPayment))
            {
                return;
            }

            _scheduledPayments.Add(scheduledPayment);
        }

        /// <summary>
        /// Replaces a payment plan schedule
        /// </summary>
        /// <param name="planSchedule">Collection of Scheduled Payments</param>
        public void ReplaceSchedule(IEnumerable<ScheduledPayment> planSchedule)
        {
            if (planSchedule == null || planSchedule.Count() == 0)
            {
                throw new ArgumentNullException("planSchedule", "Plan Schedule must be specified to replace plan schedule.");
            }

            _scheduledPayments.Clear();
            foreach (var item in planSchedule)
            {
                AddScheduledPayment(item);
            }
        }
    }
}
