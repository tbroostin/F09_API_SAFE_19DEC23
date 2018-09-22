// Copyright 2014-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Finance.Entities;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Finance.Repositories
{
    /// <summary>
    /// Interface to the Payment Plan repository
    /// </summary>
    public interface IPaymentPlanRepository
    {
        /// <summary>
        /// Get all of the payment plan templates
        /// </summary>
        /// <returns>List of payment plan templates</returns>
        IEnumerable<PaymentPlanTemplate> PaymentPlanTemplates { get; }

        /// <summary>
        /// Get a specified payment plan template
        /// </summary>
        /// <param name="templateId">ID of the payment plan template</param>
        /// <returns>Payment plan template</returns>
        PaymentPlanTemplate GetTemplate(string templateId);

        /// <summary>
        /// Get a specified payment plan
        /// </summary>
        /// <param name="planId">ID of the payment plan</param>
        /// <returns>A PaymentPlan entity</returns>
        PaymentPlan GetPaymentPlan(string planId);

        /// <summary>
        /// Get the payment plan schedule dates using a custom frequency subroutine
        /// </summary>
        /// <param name="personId">ID of the person for whom the plan dates are being calculated</param>
        /// <param name="receivableType">Receivable Type of the plan for which dates are being calculated</param>
        /// <param name="termId">ID of the term for which plan dates are being calculated</param>
        /// <param name="templateId">ID of the template used in creating the payment plan</param>
        /// <param name="downPaymentDate">Date on which down payment is due</param>
        /// <param name="firstPaymentDate">Date on which first payment (after down payment) is due</param>
        /// <param name="planId">Optional ID of the payment plan</param>
        /// <returns>A list of dates</returns>
        IEnumerable<DateTime?> GetPlanCustomScheduleDates(string personId, string receivableType, string termId, string templateId, DateTime? downPaymentDate, DateTime firstPaymentDate, string planId);
    
        /// <summary>
        /// Creates a payment plan terms approval
        /// </summary>
        /// <param name="acceptance">Payment Plan approval information</param>
        /// <returns>Updated Payment Plan approval information</returns>
        PaymentPlanApproval ApprovePaymentPlanTerms(PaymentPlanTermsAcceptance acceptance); 
   
        /// <summary>
        /// Get a payment plan approval
        /// </summary>
        /// <param name="approvalId">ID of plan approval</param>
        /// <returns>The payment plan approval information</returns>
        PaymentPlanApproval GetPaymentPlanApproval(string approvalId);

        /// <summary>
        /// Gets a proposed payment plan for a given person, term, receivable type, amount, and first payment date
        /// </summary>
        /// <param name="personId">Proposed plan owner ID</param>
        /// <param name="termId">Billing term ID</param>
        /// <param name="receivableTypeCode">Receivable Type Code</param>
        /// <param name="paymentPlanTemplateId">Payment Plan template ID</param>
        /// <param name="firstPaymentDate">Date on which first scheduled payment will be due</param>
        /// <param name="planAmount">Maximum total payment plan charges</param>       
        /// <returns>Proposed payment plan</returns>
        Task<PaymentPlan> GetProposedPaymentPlanAsync(string personId, string termId,
            string receivableTypeCode, string paymentPlanTemplateId, DateTime firstPaymentDate, decimal planAmount);
    }
}
