// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ellucian.Colleague.Domain.Finance.Entities.Configuration
{
    /// <summary>
    /// Contains configuration information for Student Finance
    /// </summary>
    [Serializable]
    public class FinanceConfiguration
    {
        private readonly List<PayableReceivableType> _displayedReceivableTypes = new List<PayableReceivableType>();
        private readonly List<string> _paymentPlanEligibilityRuleIds = new List<string>();
        private readonly List<PaymentRequirement> _termPaymentPlanRequirements = new List<PaymentRequirement>();

        /// <summary>
        /// Base constructor for a <see cref="FinanceConfiguration"/> object
        /// </summary>
        public FinanceConfiguration()
        {
            DisplayedReceivableTypes = _displayedReceivableTypes.AsReadOnly();
            PaymentPlanEligibilityRuleIds = _paymentPlanEligibilityRuleIds.AsReadOnly();
            TermPaymentPlanRequirements = _termPaymentPlanRequirements.AsReadOnly();
            PartialAccountPaymentsAllowed = true;
            PartialPlanPaymentsAllowed = PartialPlanPayments.Allowed;
            PartialDepositPaymentsAllowed = true;
            UseGuaranteedChecks = false;
            PaymentMethods = new List<AvailablePaymentMethod>();
            IncludeSchedule = true;
            IncludeDetail = true;
            IncludeHistory = true;
            RemittanceAddress = new List<string>();
            Periods = new List<FinancialPeriod>();
            StatementMessage = new List<string>();
            Links = new List<StudentFinanceLink>();
            DisplayPotentialD7Amounts = false;
        }

        /// <summary>
        /// Flag indicating whether or not the institution accepts self service payments
        /// </summary>
        public bool SelfServicePaymentsAllowed { get; set; }

        /// <summary>
        /// Flag indicating whether or not the institution accepts e-commerce enabled payments
        /// </summary>
        public bool ECommercePaymentsAllowed { get; set; }

        /// <summary>
        /// Flag indicating whether or not the institution accepts partial account payments
        /// </summary>
        public bool PartialAccountPaymentsAllowed { get; set; }

        /// <summary>
        /// Indicator of institution accepting partial payment plan payments
        /// </summary>
        public PartialPlanPayments PartialPlanPaymentsAllowed { get; set; }

        /// <summary>
        /// Flag indicating whether or not the institution accepts partial deposit payments
        /// </summary>
        public bool PartialDepositPaymentsAllowed { get; set; }

        /// <summary>
        /// Flag indicating whether or not the institution uses guaranteed check processing
        /// </summary>
        public bool UseGuaranteedChecks { get; set; }

        /// <summary>
        /// Collection of <see cref="AvailablePaymentMethod"/> objects
        /// </summary>
        public List<AvailablePaymentMethod> PaymentMethods { get; set; }

        /// <summary>
        /// Indicator of account activity display by term or by period
        /// </summary>
        public ActivityDisplay ActivityDisplay { get; set; }

        /// <summary>
        /// Indicator of payment display by term or by financial peroid
        /// </summary>
        public PaymentDisplay PaymentDisplay { get; set; }

        /// <summary>
        /// A contact address for questions pertaining to Student Finance
        /// </summary>
        public string SupportEmailAddress { get; set; }

        /// <summary>
        /// Flag indicating whether or not credit amounts are displayed in payment workflows
        /// </summary>
        public bool ShowCreditAmounts { get; set; }

        /// <summary>
        /// Text for communicating institution-defined messages to account holders
        /// </summary>
        public string NotificationText { get; set; }

        /// <summary>
        /// Flag indicating whether or not the user's course schedule is displayed on the student statement
        /// </summary>
        public bool IncludeSchedule { get; set; }

        /// <summary>
        /// Flag indicating whether or not detailed transactions are displayed on the student statement
        /// </summary>
        public bool IncludeDetail { get; set; }

        /// <summary>
        /// Flag indicating whether or not a history of transactions is displayed on the student statement
        /// </summary>
        public bool IncludeHistory { get; set; }

        /// <summary>
        /// Institution name displayed on the student statement
        /// </summary>
        public string InstitutionName { get; set; }

        /// <summary>
        /// The remittance address displayed on the student statement
        /// </summary>
        public List<string> RemittanceAddress { get; set; }

        /// <summary>
        /// Title for the student statement
        /// </summary>
        public string StatementTitle { get; set; }

        /// <summary>
        /// Message displayed in payment workflows to users when reviewing charges to be paid
        /// </summary>
        public string PaymentReviewMessage { get; set; }

        /// <summary>
        /// Collection of <see cref="FinancialPeriod"/> objects
        /// </summary>
        public List<FinancialPeriod> Periods { get; set; }

        /// <summary>
        /// Message displayed at the top of the student statement
        /// </summary>
        public List<string> StatementMessage { get; set; }

        /// <summary>
        /// Collection of <see cref="StudentFinanceLink"/> objects
        /// </summary>
        public List<StudentFinanceLink> Links { get; set; }

        /// <summary>
        /// Collection of <see cref="PayableReceivableType"/> objects
        /// </summary>
        public ReadOnlyCollection<PayableReceivableType> DisplayedReceivableTypes { get; private set; }

        /// <summary>
        /// Flag indicating whether or not users may sign up for payment plans when eligible
        /// </summary>
        public bool UserPaymentPlanCreationEnabled { get; set; }

        /// <summary>
        /// Collection of eligibility rule IDs for determining whether a user is eligible to sign up for a payment plan
        /// </summary>
        public ReadOnlyCollection<string> PaymentPlanEligibilityRuleIds { get; private set; }

        /// <summary>
        /// Collection of <see cref="PaymentRequirement"/> objects
        /// </summary>
        public ReadOnlyCollection<PaymentRequirement> TermPaymentPlanRequirements { get; private set; }

        /// <summary>
        /// Text for communicating institution-defined messages regarding invoice and charge eligibility for payment plans
        /// </summary>
        public string PaymentPlanEligibilityText { get; set; }
        /// <summary>
        /// Ecommerce provider url 
        /// </summary>
        public string EcommerceProviderLink { get; set; }

        /// <summary>
        /// Flag that controls whether to display due dates on Make A Payment
        /// </summary>
        public bool DisplayDueDates { get; set; }

        /// <summary>
        /// Colleague timezone data
        /// </summary>
        public string ColleagueTimezone { get; set; }

        /// <summary>
        /// Add a <see cref="PayableReceivableType"/> to the <see cref="FinanceConfiguration"/> object.
        /// </summary>
        /// <param name="prt">The <see cref="PayableReceivableType"/> to be added.</param>
        public void AddDisplayedReceivableType(PayableReceivableType prt)
        {
            if (prt == null)
            {
                throw new ArgumentNullException("prt", "Payable receivable type must have a value to be added.");
            }
            if (_displayedReceivableTypes.Any(drt => drt.Code.ToUpperInvariant() == prt.Code.ToUpperInvariant()))
            {
                throw new ApplicationException("Duplicate receivable types are not permitted; Receivable type " + prt.Code + " is already listed as a receivable type.");
            }
            _displayedReceivableTypes.Add(prt);
        }

        /// <summary>
        /// Add a payment plan eligibility rule to the <see cref="FinanceConfiguration"/> object.
        /// </summary>
        /// <param name="ruleId">The eligibility rule to be added.</param>
        public void AddPaymentPlanEligibilityRuleId(string ruleId)
        {
            if (string.IsNullOrEmpty(ruleId))
            {
                throw new ArgumentNullException("ruleId", "Payment Plan Eligibility Rule ID must have a value.");
            }
            if (_paymentPlanEligibilityRuleIds.Contains(ruleId))
            {
                throw new ApplicationException("Rule " + ruleId + " is already listed as a payment plan eligibility rule.");
            }
            _paymentPlanEligibilityRuleIds.Add(ruleId);
        }

        /// <summary>
        /// Add a <see cref="PaymentRequirement"/> to the <see cref="FinanceConfiguration"/> object.
        /// </summary>
        /// <param name="termPayPlanOption">The <see cref="PaymentRequirement"/> to be added.</param>
        public void AddTermPaymentPlanRequirement(PaymentRequirement termPayPlanOption)
        {
            if (termPayPlanOption == null)
            {
                throw new ArgumentNullException("termPayPlanOption", "Term payment plan option cannot be null.");
            }
            if (_termPaymentPlanRequirements.Any(tppo => tppo.TermId == termPayPlanOption.TermId && tppo.EligibilityRuleId == termPayPlanOption.EligibilityRuleId))
            {
                throw new ApplicationException("A term payment plan option for term " + termPayPlanOption.TermId + " and eligibility rule " + termPayPlanOption.EligibilityRuleId + " is already listed as a term payment plan option.");
            }
            _termPaymentPlanRequirements.Add(termPayPlanOption);
        }

        /// <summary>
        /// Determines if a receivable type is payable
        /// </summary>
        /// <param name="receivableTypeCode">Receivable type code</param>
        /// <returns>Flag indicating whether or not the receivable type is payable</returns>
        public bool IsReceivableTypePayable(string receivableTypeCode)
        {
            if (string.IsNullOrEmpty(receivableTypeCode))
            {
                throw new ArgumentNullException("receivableTypeCode", "Receivable type must have a value to be evaluated.");
            }
            // Determine if any displayed receivable types are defined in configuration
            if (DisplayedReceivableTypes.Any())
            {
                // Identify the list of payable receivable types 
                var payableReceivableTypes = DisplayedReceivableTypes.Where(drt => drt.IsPayable).Select(drt => drt.Code).Distinct().ToList();

                // If there are any payable receivable types, check the supplied receivable type
                if (!payableReceivableTypes.Contains(receivableTypeCode))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Flag indicating whether or not to display potential D7 award amounts
        /// </summary>
        public bool DisplayPotentialD7Amounts { get; set; }
    }
}
 