// Copyright 2014-2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using Ellucian.Colleague.Dtos.Finance;
using Ellucian.Colleague.Dtos.Finance.Payments;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Finance
{
    /// <summary>
    /// Interface to the Payment Plan service
    /// </summary>
    public interface IPaymentPlanService
    {
        /// <summary>
        /// Get all payment plan templates
        /// </summary>
        /// <returns>A list of PaymentPlanTemplate DTOs</returns>
        IEnumerable<PaymentPlanTemplate> GetPaymentPlanTemplates();

        /// <summary>
        /// Gets the specified payment plan template
        /// </summary>
        /// <param name="templateId">ID of the payment plan template</param>
        /// <returns>A PaymentPlanTemplate DTO</returns>
        PaymentPlanTemplate GetPaymentPlanTemplate(string templateId);

        /// <summary>
        /// Gets the specified payment plan
        /// </summary>
        /// <param name="planId">ID of the payment plan</param>
        /// <returns>A PaymentPlan DTO</returns>
        PaymentPlan GetPaymentPlan(string planId);

        /// <summary>
        /// Accept the terms and conditions associated with a payment plan
        /// </summary>
        /// <param name="acceptance">The information for the acceptance</param>
        /// <returns>The updated approval information</returns>
        PaymentPlanApproval ApprovePaymentPlanTerms(PaymentPlanTermsAcceptance acceptance);

        /// <summary>
        /// Get a payment plan approval
        /// </summary>
        /// <param name="approvalId">ID of plan approval</param>
        /// <returns>The recorded approval information</returns>
        PaymentPlanApproval GetPaymentPlanApproval(string approvalId);

        /// <summary>
        /// Gets the down payment information for a payment control, payment plan, pay method and amount
        /// </summary>
        /// <param name="planId">Payment plan ID</param>
        /// <param name="payMethod">Payment method code</param>
        /// <param name="amount">Total payment amount</param>
        /// <param name="payControlId">Optional registration payment control ID</param>
        /// <returns>List of payments to be made</returns>
        Payment GetPlanPaymentSummary(string planId, string payMethod, decimal amount, string payControlId);

        /// <summary>
        /// Gets valid billing term payment plan information from a proposed billing term payment plan information collection
        /// </summary>
        /// <param name="billingTerms">Collection of payment items</param>
        /// <returns>Valid billing term payment plan information from a proposed billing term payment plan information collection</returns>
        [Obsolete("Obsolete as of API version 1.16, use GetBillingTermPaymentPlanInformation2Async instead.")]
        Task<PaymentPlanEligibility> GetBillingTermPaymentPlanInformationAsync(IEnumerable<BillingTermPaymentPlanInformation> billingTerms);

        /// <summary>
        /// Gets valid billing term payment plan information from a proposed billing term payment plan information collection
        /// </summary>
        /// <param name="criteria">payment plan query criteria</param>
        /// <returns>Valid billing term payment plan information from a proposed billing term payment plan information collection</returns>
        Task<PaymentPlanEligibility> GetBillingTermPaymentPlanInformation2Async(PaymentPlanQueryCriteria criteria);

        /// <summary>
        /// Gets a proposed payment plan for a given person for a given term and receivable type with total charges
        /// no greater than the stated amount
        /// </summary>
        /// <param name="personId">Proposed plan owner ID</param>
        /// <param name="termId">Billing term ID</param>
        /// <param name="receivableTypeCode">Receivable Type Code</param>
        /// <param name="planAmount">Maximum total payment plan charges</param>
        /// <returns>Proposed payment plan</returns>
        Task<PaymentPlan> GetProposedPaymentPlanAsync(string personId, string termId,
            string receivableTypeCode, decimal planAmount);
    }
}
