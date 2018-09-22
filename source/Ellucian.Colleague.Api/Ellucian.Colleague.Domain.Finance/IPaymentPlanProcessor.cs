// Copyright 2014-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Finance.Entities;

namespace Ellucian.Colleague.Domain.Finance
{
    public interface IPaymentPlanProcessor
    {
        /// <summary>
        /// Filters a collection of invoices based on payment plan template parameters
        /// </summary>
        /// <param name="payPlanTemplate">Payment Plan Template</param>
        /// <param name="invoices">Collection of invoices</param>
        /// <returns>A collection of invoices that pass all payment plan template limiting criteria</returns>
        IEnumerable<Invoice> FilterInvoices(PaymentPlanTemplate payPlanTemplate, IEnumerable<Invoice> invoices);

        /// <summary>
        /// Calculates the payment plan amount for a given registration balance, payment plan template, and collection of invoices
        /// </summary>
        /// <param name="registrationBalance">Registration Balance</param>
        /// <param name="termBalance">Term Balance</param>
        /// <param name="payPlanTemplate">Payment Plan Template</param>
        /// <param name="invoices">Collection of invoices</param>
        /// <param name="payments">Collection of payments</param>
        /// <param name="chargeCodes">Collection of all receivable charge codes</param>
        /// <param name="loggingEnabled">Determines whether or not to log information</param>
        /// <param name="planReceivableTypeCode">Receivable Type Code of the payment plan</param>
        /// <returns>Payment Plan Amount</returns>
        decimal GetPlanAmount(decimal registrationBalance, decimal termBalance, PaymentPlanTemplate payPlanTemplate, IEnumerable<Invoice> invoices,
            IEnumerable<ReceivablePayment> payments, IEnumerable<ChargeCode> chargeCodes, bool loggingEnabled, out string planReceivableTypeCode);

        /// <summary>
        /// Get the list of plan charges to go on a payment plan.
        /// </summary>
        /// <param name="receivableType">The receivable type of the payment plan</param>
        /// <param name="planAmount">The amount of the payment plan</param>
        /// <param name="eligibleCharges">The list of charges that are eligible for the payment plan</param>
        /// <param name="invoiceLookupTable">Lookup table of invoices and their receivable types</param>
        /// <param name="chargeCodes">Collection of all receivable charge codes</param>
        /// <returns>Collection of plan charges for the payment plan</returns>
        List<PlanCharge> GetPlanCharges(string receivableType, decimal planAmount, PaymentPlanTemplate template, IEnumerable<Invoice> invoices,
            IEnumerable<ChargeCode> chargeCodes = null);

        /// <summary>
        /// Get a proposed payment plan
        /// </summary>
        /// <param name="template">Payment Plan Template</param>
        /// <param name="personId">Person ID of the account holder for whom the proposed plan is being retrieved</param>
        /// <param name="receivableType">Receivable Type Code</param>
        /// <param name="termId">Term ID</param>
        /// <param name="planAmount">Amount of the proposed plan</param>
        /// <param name="firstPaymentDate">Date on which first scheduled payment is due for proposed plan</param>
        /// <param name="planCharges">Collection of plan charges on the payment plan</param>
        /// <returns>Proposed Payment Plan</returns>
        PaymentPlan GetProposedPlan(PaymentPlanTemplate template, string personId, string receivableType, string termId, decimal planAmount,
            DateTime firstPaymentDate, IEnumerable<PlanCharge> planCharges);

        /// <summary>
        /// Add the list of scheduled payments for the plan.
        /// </summary>
        /// <param name="template">Template to create the plan</param>
        /// <param name="proposedPlan">Plan under construction</param>
        /// <param name="downPaymentDate">Down Payment date</param>
        /// <param name="planDates">List of scheduled payment dates</param>
        void AddPlanSchedules(PaymentPlanTemplate template, PaymentPlan proposedPlan, List<DateTime?> planDates);
    }
}
