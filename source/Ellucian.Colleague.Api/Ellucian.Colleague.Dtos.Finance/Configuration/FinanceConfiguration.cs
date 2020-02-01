// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Finance.Configuration
{
    /// <summary>
    /// Configuration for Student Finance
    /// </summary>
    public class FinanceConfiguration
    {
        /// <summary>
        /// FinanceConfiguration constructor
        /// </summary>
        public FinanceConfiguration()
        {
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
            DisplayPotentialD7Amounts = false;
        }

        /// <summary>
        /// Indicates whether payments from self-service are allowed
        /// </summary>
        public bool SelfServicePaymentsAllowed { get; set; }

        /// <summary>
        /// Indicates whether e-commerce payments are allowed
        /// </summary>
        public bool ECommercePaymentsAllowed { get; set; }

        /// <summary>
        /// Indicates whether partial payments are allowed on accounts
        /// </summary>
        public bool PartialAccountPaymentsAllowed { get; set; }

        /// <summary>
        /// Indicates what <see cref="PartialPlanPayments">PartialPlanPayments</see> option is chosen for plan payments
        /// </summary>
        public PartialPlanPayments PartialPlanPaymentsAllowed { get; set; }

        /// <summary>
        /// Indicates whether partial payments are allowed on deposits due
        /// </summary>
        public bool PartialDepositPaymentsAllowed { get; set; }

        /// <summary>
        /// Indicates whether the institution is using guaranteed checks
        /// </summary>
        public bool UseGuaranteedChecks { get; set; }

        /// <summary>
        /// List of <see cref="AvailablePaymentMethod">available payment methods</see> for Make a Payment
        /// </summary>
        public List<AvailablePaymentMethod> PaymentMethods { get; set; }

        /// <summary>
        /// Indicates the display option for Account Activity
        /// </summary>
        public ActivityDisplay ActivityDisplay { get; set; }

        /// <summary>
        /// Indicates the display option for Make a Payment
        /// </summary>
        public PaymentDisplay PaymentDisplay { get; set; }

        /// <summary>
        /// Email address to get support for Student Finance
        /// </summary>
        public string SupportEmailAddress { get; set; }

        /// <summary>
        /// Indicates whether credit amounts are shown on Make a Payment
        /// </summary>
        public bool ShowCreditAmounts { get; set; }

        /// <summary>
        /// Notification text to display on Make a Payment
        /// </summary>
        public string NotificationText { get; set; }

        /// <summary>
        /// Indicates whether the student's schedule is displayed on the statement
        /// </summary>
        public bool IncludeSchedule { get; set; }

        /// <summary>
        /// Indicates whether the detail section is displayed on the statement
        /// </summary>
        public bool IncludeDetail { get; set; }

        /// <summary>
        /// Indicates whether the history section is displayed on the statement
        /// </summary>
        public bool IncludeHistory { get; set; }

        /// <summary>
        /// The institution's name as displayed on the statement
        /// </summary>
        public string InstitutionName { get; set; }

        /// <summary>
        /// The remittance address to include on the statement
        /// </summary>
        public List<string> RemittanceAddress { get; set; }

        /// <summary>
        /// The title of the student statement
        /// </summary>
        public string StatementTitle { get; set; }

        /// <summary>
        /// The message to display when the student is reviewing payment information
        /// </summary>
        public string PaymentReviewMessage { get; set; }

        /// <summary>
        /// The list of current <see cref="FinancialPeriod">financial periods</see> for Student Finance
        /// </summary>
        public List<FinancialPeriod> Periods { get; set; }

        /// <summary>
        /// A message to display on all student statements
        /// </summary>
        public List<string> StatementMessage { get; set; }

        /// <summary>
        /// The list of <see cref="StudentFinanceLink">student finance links</see>
        /// </summary>
        public List<StudentFinanceLink> Links { get; set; }

        /// <summary>
        /// Collection of <see cref="PayableReceivableType">receivable types with payment permissions</see>
        /// </summary>
        public IEnumerable<PayableReceivableType> DisplayedReceivableTypes { get; set; }

        /// <summary>
        /// Flag indicating whether or not users may sign up for payment plans when eligible
        /// </summary>
        public bool UserPaymentPlanCreationEnabled { get; set; }

        /// <summary>
        /// Collection of eligibility rule IDs for determining whether a user is eligible to sign up for a payment plan
        /// </summary>
        public IEnumerable<string> PaymentPlanEligibilityRuleIds { get; set; }

        /// <summary>
        /// Collection of <see cref="PaymentRequirement">term payment plan requirements</see>
        /// </summary>
        public IEnumerable<PaymentRequirement> TermPaymentPlanRequirements { get; set; }

        /// <summary>
        /// Text for communicating institution-defined messages regarding invoice and charge eligibility for payment plans
        /// </summary>
        public string PaymentPlanEligibilityText { get; set; }
        /// <summary>
        /// Ecommerce provider url
        /// </summary>
        public string EcommerceProviderLink { get; set; }
        /// <summary>
        /// Flag indicating whether or not to display potential D7 award amounts
        /// </summary>
        public bool DisplayPotentialD7Amounts { get; set; }
    }
}
