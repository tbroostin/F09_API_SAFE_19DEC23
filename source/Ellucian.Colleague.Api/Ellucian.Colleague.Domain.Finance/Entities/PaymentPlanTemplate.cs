// Copyright 2014-2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    /// <summary>
    /// A template for creating a payment plan
    /// </summary>
    [Serializable]
    public class PaymentPlanTemplate
    {
        private readonly string _id;
        /// <summary>
        /// ID of the payment plan template
        /// </summary>
        public string Id { get { return _id; } }
        
        private readonly string _description;
        /// <summary>
        /// Description of the payment plan template
        /// </summary>
        public string Description { get { return _description; } }

        private readonly bool _isActive;
        /// <summary>
        /// Determines whether the payment plan template may be used to create a payment plan
        /// </summary>
        public bool IsActive { get { return _isActive; } }

        private readonly PlanFrequency _frequency;
        /// <summary>
        /// Frequency of scheduled payments for payment plans created from the template
        /// </summary>
        public PlanFrequency Frequency { get { return _frequency; } }

        private readonly int _numberOfPayments;
        /// <summary>
        /// Number of scheduled payments for payment plans created from the template
        /// </summary>
        public int NumberOfPayments { get { return _numberOfPayments; } }

        private readonly decimal _minimumPlanAmount;
        /// <summary>
        /// Minimum amount for which a payment plan may be created from the template
        /// </summary>
        public decimal MinimumPlanAmount { get { return _minimumPlanAmount; } }

        private readonly decimal? _maximumPlanAmount;
        /// <summary>
        /// Maximum amount for which a payment plan may be created from the template
        /// </summary>
        public decimal? MaximumPlanAmount { get { return _maximumPlanAmount; } }

        private readonly string _customFrequencySubroutine;
        /// <summary>
        /// Subroutine used in calculating custom payment plan frequency
        /// </summary>
        public string CustomFrequencySubroutine { get { return _customFrequencySubroutine; } }

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

        private readonly List<string> _allowedReceivableTypeCodes = new List<string>();
        /// <summary>
        /// Collection of receivable type codes for which charges may be assigned to a payment plan created from the template
        /// </summary>
        public ReadOnlyCollection<string> AllowedReceivableTypeCodes { get; private set; }

        private readonly List<string> _invoiceExclusionRuleIds = new List<string>();
        /// <summary>
        /// Collection of IDs of invoice exclusion rules that an invoice must fail in order to be assigned to a payment plan created from the template
        /// </summary>
        public ReadOnlyCollection<string> InvoiceExclusionRuleIds { get; private set; }

        private readonly List<string> _includedChargeCodes = new List<string>();
        /// <summary>
        /// Collection of IDs of charge codes that may be assigned to a payment plan created from the template
        /// </summary>
        public ReadOnlyCollection<string> IncludedChargeCodes { get; private set; }

        private readonly List<string> _excludedChargeCodes = new List<string>();
        /// <summary>
        /// Collection of IDs of charge codes that may not be assigned to a payment plan created from the template
        /// </summary>
        public ReadOnlyCollection<string> ExcludedChargeCodes { get; private set; }

        /// <summary>
        /// Gets the down payment date for a plan
        /// </summary>
        public DateTime? DownPaymentDate
        {
            get
            {
                DateTime? downPaymentDate = null;
                if (DownPaymentPercentage > 0)
                {
                    downPaymentDate = DateTime.Today.AddDays(DaysUntilDownPaymentIsDue);
                }
                return downPaymentDate;
            }
        }

        /// <summary>
        /// Constructor for the payment plan template
        /// </summary>
        /// <param name="id">ID of the payment plan template</param>        
        /// <param name="description">Description of the payment plan template</param>
        /// <param name="isActive">Determines whether the payment plan template may be used to create payment plans</param>
        /// <param name="frequency">Frequency by which a payment plan's scheduled payments are calculated</param>
        /// <param name="numberOfPayments">The number of scheduled payments for a payment plan</param>
        /// <param name="minimumAmount">Minimum amount for which a payment plan may be created</param>
        /// <param name="maximumAmount">Maximum amount for which a payment plan may be created</param>
        /// <param name="customFrequencySubroutine">Subroutine used to calculate custom plan schedule frequency</param>
        public PaymentPlanTemplate(string id, string description, bool isActive, PlanFrequency frequency, int numberOfPayments,
            decimal minimumAmount, decimal? maximumAmount, string customFrequencySubroutine)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Plan ID cannot be null/empty");
            }
            if (string.IsNullOrEmpty(description))
            {
                throw new ArgumentNullException("description", "Description cannot be null/empty");
            }
            if (numberOfPayments < 0 || numberOfPayments > 999)
            {
                throw new ArgumentOutOfRangeException("numberOfPayments", "Number of Payments must be between 0 and 999");
            }
            if (minimumAmount < 0)
            {
                throw new ArgumentOutOfRangeException("minimumAmount", "Minimum Plan Amount must be greater than or equal to zero");
            }
            if (maximumAmount.HasValue && maximumAmount.Value <= 0)
            {
                throw new ArgumentOutOfRangeException("maximumAmount", "Maximum Plan Amount must be greater than zero if specified");
            }
            if (maximumAmount.HasValue && minimumAmount > maximumAmount.Value)
            {
                throw new ArgumentOutOfRangeException("maximumAmount", "Maximum Plan Amount must be greater than minimum plan amount if specified");
            }
            if (frequency == PlanFrequency.Custom && string.IsNullOrEmpty(customFrequencySubroutine))
            {
                throw new ArgumentNullException("customFrequencySubroutine", "Custom Frequency Subroutine must be specified when Frequency is Custom");
            }
            if (frequency != PlanFrequency.Custom && !string.IsNullOrEmpty(customFrequencySubroutine))
            {
                throw new ArgumentException("Custom Frequency Subroutine may not be specified when Frequency is not Custom", "customFrequencySubroutine");
            }

            _id = id;
            _isActive = isActive;
            _description = description;
            _frequency = frequency;
            _numberOfPayments = numberOfPayments;
            _minimumPlanAmount = minimumAmount;
            _maximumPlanAmount = maximumAmount;
            _customFrequencySubroutine = customFrequencySubroutine;

            // Default the Boolean values to false
            IncludeSetupChargeInFirstPayment = false;
            SubtractAnticipatedFinancialAid = false;
            CalculatePlanAmountAutomatically = false;
            ModifyPlanAutomatically = false;

            AllowedReceivableTypeCodes = _allowedReceivableTypeCodes.AsReadOnly();
            InvoiceExclusionRuleIds = _invoiceExclusionRuleIds.AsReadOnly();
            IncludedChargeCodes = _includedChargeCodes.AsReadOnly();
            ExcludedChargeCodes = _excludedChargeCodes.AsReadOnly();
        }

        /// <summary>
        /// Add a receivable type code to the list of allowed receivable type codes
        /// </summary>
        /// <param name="receivableTypeCode">Receivable type code</param>
        public void AddAllowedReceivableTypeCode(string receivableTypeCode)
        {
            if (string.IsNullOrEmpty(receivableTypeCode))
            {
                throw new ArgumentNullException("receivableTypeCode", "Receivable Type Code cannot be null/empty");
            }

            // Prevent duplicates
            if (!_allowedReceivableTypeCodes.Contains(receivableTypeCode))
            {
                _allowedReceivableTypeCodes.Add(receivableTypeCode);
            }
        }

        /// <summary>
        /// Add a rule ID to the list of invoice exclusion rule IDs
        /// </summary>
        /// <param name="ruleId">Rule ID</param>
        public void AddInvoiceExclusionRuleId(string ruleId)
        {
            if (string.IsNullOrEmpty(ruleId))
            {
                throw new ArgumentNullException("ruleId", "Rule ID cannot be null/empty");
            }

            // Prevent duplicates
            if (!_invoiceExclusionRuleIds.Contains(ruleId))
            {
                _invoiceExclusionRuleIds.Add(ruleId);
            }
        }

        /// <summary>
        /// Add a charge code to the list of included charge codes
        /// </summary>
        /// <param name="chargeCode">Charge Code</param>
        public void AddIncludedChargeCode(string chargeCode)
        {
            if (string.IsNullOrEmpty(chargeCode))
            {
                throw new ArgumentNullException("chargeCode", "Charge Code cannot be null/empty");
            }

            // Check for excluded AR Codes
            if (_excludedChargeCodes.Count > 0)
            {
                throw new ArgumentException("Payment plan template cannot have Included AR Codes and Excluded AR Codes", "chargeCode");
            }

            // Prevent duplicates
            if (!_includedChargeCodes.Contains(chargeCode))
            {
                _includedChargeCodes.Add(chargeCode);
            }
        }

        /// <summary>
        /// Add a charge code to the list of excluded charge codes
        /// </summary>
        /// <param name="chargeCode">Charge Code</param>
        public void AddExcludedChargeCode(string chargeCode)
        {
            if (string.IsNullOrEmpty(chargeCode))
            {
                throw new ArgumentNullException("chargeCode", "Charge Code cannot be null/empty");
            }

            // Check for included AR Codes
            if (_includedChargeCodes.Count > 0)
            {
                throw new ArgumentException("Payment plan template cannot have Included AR Codes and Excluded AR Codes", "chargeCode");
            }

            // Prevent duplicates
            if (!_excludedChargeCodes.Contains(chargeCode))
            {
                _excludedChargeCodes.Add(chargeCode);
            }
        }

        /// <summary>
        /// Calculates the down payment for a given payment plan amount
        /// </summary>
        /// <param name="planAmount">Amount of the payment plan</param>
        /// <returns>Down Payment amount</returns>
        public decimal CalculateDownPaymentAmount(decimal planAmount)
        {
            if (planAmount <= 0)
            {
                throw new ArgumentOutOfRangeException("planAmount", "Payment Plan Amount must be greater than zero.");
            }

            // Down Payment is only calculated if there is a down payment percentage greater than zero
            if (DownPaymentPercentage <= 0)
            {
                return 0m;
            }

            // If the setup charge is amortized, then it is included in the amount for the down payment calculation
            decimal setupChargeAmount = CalculateSetupChargeAmount(planAmount);
            decimal amortizedAmount = planAmount;
            if (!IncludeSetupChargeInFirstPayment)
            {
                amortizedAmount += setupChargeAmount;
            }
            // Now calculate the down payment based on the amortized amount, then decrease the amortized amount by the down payment amount
            decimal downPaymentAmount = Math.Round((amortizedAmount * DownPaymentPercentage / 100), 2, MidpointRounding.AwayFromZero);
            amortizedAmount -= downPaymentAmount;

            // If the setup charge gets included in the first payment, add it in now
            if (IncludeSetupChargeInFirstPayment)
            {
                downPaymentAmount += setupChargeAmount;
            }
            // Add in any leftover amount
            decimal scheduledAmount = decimal.Floor(amortizedAmount / NumberOfPayments);
            decimal leftoverAmount = amortizedAmount - (scheduledAmount * NumberOfPayments);
            downPaymentAmount += leftoverAmount;

            return downPaymentAmount;
        }

        /// <summary>
        /// Calculate the amount of the first scheduled payment based on the plan amount
        /// </summary>
        /// <param name="planAmount">Payment plan amount</param>
        /// <param name="numberOfPayments">Number of scheduled payments for the plan</param>
        /// <returns>Amount of first scheduled payment</returns>
        public decimal CalculateFirstPaymentAmount(decimal planAmount, int numberOfPayments)
        {
            if (numberOfPayments == 0)
            {
                throw new ArgumentOutOfRangeException("numberOfPayments", "numberOfPayments cannot be 0.");
            }
            decimal firstPaymentAmount = CalculateScheduledPaymentAmount(planAmount, numberOfPayments);
            // If there's no down payment, any leftover amount is included in the first scheduled payment
            if (DownPaymentPercentage <= 0)
            {
                firstPaymentAmount += (CalculateAmortizedAmount(planAmount) -
                                       (CalculateScheduledPaymentAmount(planAmount, numberOfPayments) * numberOfPayments));
                if (IncludeSetupChargeInFirstPayment)
                {
                    firstPaymentAmount += CalculateSetupChargeAmount(planAmount);
                }
            }
            return firstPaymentAmount;
        }

        /// <summary>
        /// Calculates the setup charge for a given payment plan amount
        /// </summary>
        /// <param name="planAmount">Amount of the payment plan</param>
        /// <returns>Setup Charge amount</returns>
        public decimal CalculateSetupChargeAmount(decimal planAmount)
        {
            if (planAmount <= 0)
            {
                throw new ArgumentOutOfRangeException("planAmount", "Payment Plan Amount must be greater than zero.");
            }

            return SetupChargeAmount + Math.Round((planAmount * SetupChargePercentage / 100), 2, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Calculates the amount of a payment plan scheduled payment
        /// </summary>
        /// <param name="planAmount">Payment plan amount</param>
        /// <param name="numberOfPayments">Number of scheduled payments for the plan</param>
        /// <returns>Scheduled Payment Amount</returns>
        public decimal CalculateScheduledPaymentAmount(decimal planAmount, int numberOfPayments)
        {
            if (numberOfPayments == 0)
            {
                throw new ArgumentOutOfRangeException("numberOfPayments", "numberOfPayments cannot be 0.");
            }
            return decimal.Floor(CalculateAmortizedAmount(planAmount) / numberOfPayments);
        }

        /// <summary>
        /// Calculate the dates for the plans scheduled payments.
        /// Note that this does not include the down payment.
        /// </summary>
        /// <param name="firstPaymentDate">Due date for the first scheduled payment</param>
        /// <returns>List of scheduled payment dates</returns>
        public List<DateTime> GetPaymentScheduleDates(DateTime firstPaymentDate)
        {
            if (_frequency == PlanFrequency.Custom)
            {
                throw new InvalidOperationException("Cannot use this method with a custom frequency.");
            }
            var schedule = new List<DateTime>();
            for (int i = 0; i < _numberOfPayments; i++)
            {
                DateTime nextDate;
                switch (_frequency)
                {
                    case PlanFrequency.Weekly:
                        nextDate = firstPaymentDate.AddDays(7 * i);
                        break;
                    case PlanFrequency.Biweekly:
                        nextDate = firstPaymentDate.AddDays(14 * i);
                        break;
                    case PlanFrequency.Monthly:
                        nextDate = firstPaymentDate.AddMonths(i);
                        break;
                    case PlanFrequency.Yearly:
                        nextDate = firstPaymentDate.AddYears(i);
                        break;
                    default:
                        throw new InvalidOperationException("Unknown frequency defined on template " + _id);
                }
                // The schedule date cannot be before the down payment date, if there is one
                schedule.Add(DownPaymentDate.HasValue && nextDate < DownPaymentDate.Value ? DownPaymentDate.Value : nextDate);
            }
            return schedule;
        }

        /// <summary>
        /// Indicates if a receivable type code is permitted by the payment plan template
        /// </summary>
        /// <param name="receivableTypeCode">Receivable Type code</param>
        /// <param name="paymentPlanAmount">Payment plan amount</param>
        /// <param name="messages">Messages related to payment plan template validity</param>
        /// <param name="reason">Ineligibility reason</param>
        /// <returns>Flag indicating if a receivable type code is permitted by the payment plan template</returns>
        public bool IsValidForUserPaymentPlanCreation(string receivableTypeCode, decimal paymentPlanAmount, out List<string> messages, out PaymentPlanIneligibilityReason? reason)
        {
            if (string.IsNullOrEmpty(receivableTypeCode))
            {
                throw new ArgumentNullException("receivableTypeCode", "A receivable type code must be specified.");
            }
            messages = new List<string>();
            reason = null;
            string formatString = "Payment plan template " + Id + " {0}";

            if (!IsActive)
            {
                messages.Add(String.Format(formatString, "is not active and cannot be used for user payment plan creation."));
                reason = PaymentPlanIneligibilityReason.PreventedBySystemConfiguration;
            }
            if (!CalculatePlanAmountAutomatically)
            {
                messages.Add(String.Format(formatString, "does not automatically calculate payment plan amounts and cannot be used for user payment plan creation."));
                reason = PaymentPlanIneligibilityReason.PreventedBySystemConfiguration;
            }
            if (!ModifyPlanAutomatically)
            {
                messages.Add(String.Format(formatString, "does not automatically modify payment plans when billing changes occur and cannot be used for user payment plan creation."));
                reason = PaymentPlanIneligibilityReason.PreventedBySystemConfiguration;
            }
            if (string.IsNullOrEmpty(TermsAndConditionsDocumentId))
            {
                messages.Add(String.Format(formatString, "does not have any terms and conditions specified and cannot be used for user payment plan creation."));
                reason = PaymentPlanIneligibilityReason.PreventedBySystemConfiguration;
            }
            if (paymentPlanAmount < MinimumPlanAmount)
            {
                messages.Add(String.Format(formatString, "cannot be used for user payment plan creation for this amount. " + paymentPlanAmount.ToString("C2") + " is less than the template's minimum plan amount of " + MinimumPlanAmount.ToString("C2")));
                reason = PaymentPlanIneligibilityReason.ChargesAreNotEligible;
            }
            if (AllowedReceivableTypeCodes.Any() && !AllowedReceivableTypeCodes.Contains(receivableTypeCode))
            {
                messages.Add(String.Format(formatString, "cannot be used for user payment plan creation for receivable type " + receivableTypeCode));
                reason = PaymentPlanIneligibilityReason.ChargesAreNotEligible;
            }
            return !messages.Any();
        }

        /// <summary>
        /// Calculates the "oddball" amount that goes on the first payment when the distributed amount does not distribute evenly across all payments.
        /// </summary>
        /// <param name="planAmount">Payment plan amount</param>
        /// <param name="numberOfPayments">Number of scheduled payments for the plan</param>
        /// <returns>"Oddball" amount to be added to first payment</returns>
        private decimal CalculateLeftoverAmount(decimal planAmount, int numberOfPayments)
        {
            return CalculateAmortizedAmount(planAmount) - (CalculateScheduledPaymentAmount(planAmount, numberOfPayments) * numberOfPayments);
        }

        /// <summary>
        /// Calculate amortized amount of payment plan
        /// </summary>
        /// <param name="planAmount">Payment plan amount</param>
        /// <returns>Amount being amortized</returns>
        private decimal CalculateAmortizedAmount(decimal planAmount)
        {
            decimal setupChargeAmount = CalculateSetupChargeAmount(planAmount);
            decimal downPaymentAmount = CalculateDownPaymentAmount(planAmount);
            decimal amortizedAmount = planAmount - downPaymentAmount;
            if (downPaymentAmount > 0 || !IncludeSetupChargeInFirstPayment)
            {
                amortizedAmount += setupChargeAmount;
            }
            return amortizedAmount;
        }
    }
}
