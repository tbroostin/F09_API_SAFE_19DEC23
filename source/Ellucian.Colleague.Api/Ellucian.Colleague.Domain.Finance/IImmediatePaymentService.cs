using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Finance.Entities;
using Ellucian.Colleague.Domain.Finance.Entities.AccountDue;
using Ellucian.Colleague.Domain.Finance.Entities.Payments;

namespace Ellucian.Colleague.Domain.Finance
{
    public interface IImmediatePaymentService
    {
        /// <summary>
        /// Gets the payment options for a student
        /// </summary>
        /// <param name="paymentControl">Registration payment control for the student/term</param>
        /// <param name="invoices">List of registration invoices</param>
        /// <param name="payments">List of registration payments</param>
        /// <param name="accountTerms">List of account terms</param>
        /// <param name="distributionMap">Mapping of receivable types to distributions</param>
        /// <param name="payReq">Payment requirement for student/term</param>
        /// <param name="payPlanTemplate">Payment Plan Template</param>
        /// <param name="firstPaymentDate">Date on which first payment plan scheduled payment is due</param>
        /// <param name="chargeCodes">List of charge codes</param>
        /// <param name="receivableTypes">List of limiting receivable types</param>
        /// <returns>Immediate Payment Options outlining the payment options available to the student</returns>
        ImmediatePaymentOptions GetPaymentOptions(RegistrationPaymentControl paymentControl, IEnumerable<Invoice> invoices, IEnumerable<ReceivablePayment> payments,
            List<AccountTerm> accountTerms, Dictionary<string, string> distributionMap, PaymentRequirement payReq, IEnumerable<ChargeCode> chargeCodes,
            IEnumerable<string> receivableTypes = null);

        /// <summary>
        /// Add payment plan information to the payment options
        /// </summary>
        /// <param name="paymentOptions">The payment options to update</param>
        /// <param name="payPlanTemplate">The payment plan template to use for a new plan</param>
        /// <param name="firstPaymentDate">Date of the first scheduled payment on a new plan</param>
        /// <param name="payPlanAmount">Amount of the new payment plan</param>
        /// <param name="payPlanReceivableTypeCode">Receivable type of the new plan</param>
        void AddPaymentPlanOption(ImmediatePaymentOptions paymentOptions, PaymentPlanTemplate payPlanTemplate, DateTime? firstPaymentDate, decimal payPlanAmount,
            string payPlanReceivableTypeCode);

        /// <summary>
        /// Gets the payment plan option for the payment requirement for the student, if applicable, for the current date
        /// </summary>
        /// <param name="applicablePaymentRequirement">The payment requirement that is applicable for the student</param>
        PaymentPlanOption GetPaymentPlanOption(PaymentRequirement applicablePaymentRequirement);

        /// <summary>
        /// Get the payment summary for a payment control, pay method, and payment amount
        /// </summary>
        /// <param name="paymentControl">Registration payment control for the student/term</param>
        /// <param name="payMethod">Payment method code</param>
        /// <param name="amount">Total payment amount</param>
        /// <param name="invoices">List of registration invoices</param>
        /// <param name="regPayments">List of registration payments</param>
        /// <param name="accountTerms">List of account terms</param>
        /// <param name="distributionMap">Mapping of receivable types to distributions</param>
        /// <param name="payReq">Payment requirement for student/term</param>
        /// <param name="confMap">Mapping of distributions to e-commerce configurations</param>
        /// <param name="receivableTypes">List of all receivable types</param>
        /// <returns>List of payments to be made</returns>
        IEnumerable<Payment> GetPaymentSummary(RegistrationPaymentControl paymentControl, string payMethod, decimal amount, IEnumerable<Invoice> invoices,
            IEnumerable<ReceivablePayment> regPayments, List<AccountTerm> accountTerms, Dictionary<string, string> distributionMap, PaymentRequirement payReq,
            Dictionary<string, PaymentConfirmation> confMap, IEnumerable<ReceivableType> receivableTypes);

        /// <summary>
        /// Determines if payment plan template can be used to create payment plans through the Immediate Payment Control workflow
        /// </summary>
        /// <param name="payPlanTemplate">Payment Plan Template</param>
        /// <returns>Whether or not the payment plan template can be used to create payment plans through the Immediate Payment Control workflow</returns>
        bool IsValidTemplate(PaymentPlanTemplate payPlanTemplate);
    }
}
