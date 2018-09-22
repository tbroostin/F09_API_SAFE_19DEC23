// Copyright 2014-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Finance.Entities;
using Ellucian.Colleague.Domain.Finance.Repositories;
using slf4net;

namespace Ellucian.Colleague.Domain.Finance.Services
{
    public class PaymentPlanProcessor : IPaymentPlanProcessor
    {
        private List<ChargeCode> _chargeCodes;
        private ILogger _logger;
        private Func<List<Invoice>, IEnumerable<string>, List<Invoice>> _processInvoiceRulesFunction;

        /// <summary>
        /// Constructor for the PaymentPlanProcessor
        /// </summary>
        /// <param name="processInvoiceRulesFunction">Function needed to process invoice rules</param>
        /// <param name="logger">The Logger</param>
        public PaymentPlanProcessor(Func<List<Invoice>, IEnumerable<string>, List<Invoice>> processInvoiceRulesFunction, ILogger logger)
        {
            _processInvoiceRulesFunction = processInvoiceRulesFunction;
            _logger = logger;
        }

        /// <summary>
        /// Filters a collection of invoices based on payment plan template parameters
        /// </summary>
        /// <param name="payPlanTemplate">Payment Plan Template</param>
        /// <param name="invoices">Collection of invoices</param>
        /// <returns>A collection of invoices and charges that pass all payment plan template limiting criteria</returns>
        public IEnumerable<Invoice> FilterInvoices(PaymentPlanTemplate payPlanTemplate, IEnumerable<Invoice> invoices)
        {
            if (payPlanTemplate == null)
            {
                throw new ArgumentNullException("payPlanTemplate", "A payment plan template must be provided.");
            }

            if (invoices == null || invoices.Count() == 0)
            {
                throw new ArgumentNullException("invoices", "There are no invoices to filter.");
            }

            // Identify the Receivable Type(s) on the invoices
            var newInvoiceList = new List<Invoice>();

            // Filter the invoice Receivable Type(s) to just the ones on the list of allowed receivable types on the template
            if (payPlanTemplate.AllowedReceivableTypeCodes != null && payPlanTemplate.AllowedReceivableTypeCodes.Count() > 0)
            {
                var invoiceReceivableTypes = invoices.Select(x => x.ReceivableTypeCode).Distinct();
                foreach (var receivableType in invoiceReceivableTypes)
                {
                    if (payPlanTemplate.AllowedReceivableTypeCodes.Contains(receivableType))
                    {
                        newInvoiceList.AddRange(invoices.Where(invoice => invoice.ReceivableTypeCode == receivableType));
                    }
                }
            }
            // There are no Allowed Receivable Types - all invoices are still plan-eligible
            else
            {
                newInvoiceList.AddRange(invoices);
            }


            // Check if any invoices remain after filtering based on allowed receivable types for the template
            if (newInvoiceList.Count == 0)
            {
                _logger.Info("Registration invoice Receivable Type(s) are not allowed for template " + payPlanTemplate.Id + " - payment plan cannot be created.");
                return newInvoiceList;
            }

            // Filter remaining invoices based on the invoice exclusion rules
            if (payPlanTemplate.InvoiceExclusionRuleIds != null && payPlanTemplate.InvoiceExclusionRuleIds.Count > 0)
            {
                newInvoiceList = _processInvoiceRulesFunction.Invoke(newInvoiceList, payPlanTemplate.InvoiceExclusionRuleIds);
            }

            // Check if any invoices remain after filtering based on invoice exclusion rules for the template
            if (newInvoiceList.Count == 0)
            {
                _logger.Info("Registration invoices passed invoice exclusion rules for template " + payPlanTemplate.Id + " - payment plan cannot be created.");
            }

            return newInvoiceList;
        }

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
        public decimal GetPlanAmount(decimal registrationBalance, decimal termBalance, PaymentPlanTemplate payPlanTemplate, IEnumerable<Invoice> invoices, 
            IEnumerable<ReceivablePayment> payments, IEnumerable<ChargeCode> chargeCodes, bool loggingEnabled, out string planReceivableTypeCode)
        {
            if (chargeCodes == null || chargeCodes.Count() == 0)
            {
                throw new ArgumentNullException("chargeCodes", "There are no charge codes available.");
            }
            // Store the charge codes globally so we don't have to pass them around
            _chargeCodes = chargeCodes.ToList();

            planReceivableTypeCode = null;

            var eligibleCharges = GetEligibleCharges(payPlanTemplate, invoices);
            if (eligibleCharges == null || eligibleCharges.Count == 0)
            {
                if (loggingEnabled)
                {
                    _logger.Info("Payment Plan is not an option - there are no plan-eligible charges for template " + payPlanTemplate.Id + ".");
                }
                return 0m;
            }
            List<Charge> ineligibleCharges = invoices.SelectMany(inv => inv.Charges).Except(eligibleCharges).ToList();

            // Build a dictionary of all of the Receivable Types, and their corresponding totals, from the collection of payments.
            // The payments are currently positive numbers; we're going to make them negative to simplify later processing.
            var totalPaymentsByReceivableType = payments.GroupBy(x => x.ReceivableType, x => 0 - x.Amount).ToDictionary(x => x.Key, x => x.Sum());

            // Build other dictionaries that contain all the ineligible and eligible charges by receivable type
            var invoiceLookupTable = BuildInvoiceLookupTable(invoices);
            var ineligibleChargesByType = ineligibleCharges.GroupBy(x => invoiceLookupTable[x.InvoiceId], y => y.Amount).ToDictionary(x => x.Key, x => x.Sum());
            var eligibleChargesByType = eligibleCharges.GroupBy(x => invoiceLookupTable[x.InvoiceId], y => y.Amount).ToDictionary(x => x.Key, x => x.Sum());

            // Now apply payments against the ineligible charges by receivable type, then the eligible charges.
            // Note that the charges are being added to the payments, and the results are in the payments table.
            // After the ineligible charges are done, check the table for any positive amounts.  Those would be
            // ineligible charges that should not be included.
            ApplyChargesToPayments(totalPaymentsByReceivableType, ineligibleChargesByType);
            for (int i = 0; i < totalPaymentsByReceivableType.Count; i++ )
            {
                string key = totalPaymentsByReceivableType.Keys.ElementAt(i);
                if (totalPaymentsByReceivableType[key] > 0)
                {
                    totalPaymentsByReceivableType[key] = 0;
                }
            }
            ApplyChargesToPayments(totalPaymentsByReceivableType, eligibleChargesByType);

            // The only thing that matters at this point is whether the receivable type has a positive number (more charges
            // than payments); if not, it can be ignored. First, we have to determine whether there is more than one
            // receivables type with unpaid charges.  If so, then the student can't use a payment plan.

            var validReceivableTypes = totalPaymentsByReceivableType.Where(x => x.Value > 0);
            if (validReceivableTypes.Count() != 1)
            {
                if (loggingEnabled)
                {
                    if (validReceivableTypes.Count() > 1)
                    {
                        _logger.Info("Plan-eligible invoices exist for multiple receivable Types after invoice filtering and payments have been applied - payment plan is not an option.");
                    }
                    else
                    {
                        _logger.Info("No outstanding balance to pay on a payment plan.");
                    }
                }
                return 0m;
            }

            // Get the one receivable type that has a positive balance and its balance.
            planReceivableTypeCode = validReceivableTypes.First().Key;
            decimal planReceivableTypeAmount = validReceivableTypes.First().Value;

            // If there is no receivable code, then there's nothing to go on the plan, and we're done
            if (string.IsNullOrEmpty(planReceivableTypeCode))
            {
                if (loggingEnabled)
                {
                    _logger.Info("No plan can be created after payments are applied to charges.");
                }
                return 0m;
            }

            // Calculate the total unpaid charges, which is the amount for the one receivable type with a positive balance.
            // The plan amount is the lesser of that amount and the registration balance.
            var netPlanAmount = Math.Min(planReceivableTypeAmount, registrationBalance);

            // Take the lesser of the current net plan amount and the term balance
            netPlanAmount = Math.Min(netPlanAmount, termBalance);

            // Compare Net Plan Amount to template minimum - plan creation not allowed if plan amount is less than template minimum
            if (netPlanAmount < payPlanTemplate.MinimumPlanAmount)
            {
                if (loggingEnabled)
                {
                    _logger.Info("Calculated Plan Amount of " + netPlanAmount.ToString() + " is less than template " + payPlanTemplate.Id + "'s Minimum Plan Amount of " + payPlanTemplate.MinimumPlanAmount.ToString());
                }
                return 0m;
            }

            // If the template has a maximum amount, enforce that now, too.
            if (payPlanTemplate.MaximumPlanAmount.HasValue)
            {
                netPlanAmount = Math.Min(netPlanAmount, payPlanTemplate.MaximumPlanAmount.Value);
                if (loggingEnabled)
                {
                    _logger.Info("Calculated Plan Amount of " + netPlanAmount.ToString() + " is greater than template " + payPlanTemplate.Id + "'s Maximum Plan Amount of " + payPlanTemplate.MaximumPlanAmount.ToString() + " and has been reduced to that amount.");
                }
            }

            return netPlanAmount;
        }

        /// <summary>
        /// Get the list of plan charges to go on a payment plan.
        /// </summary>
        /// <param name="receivableType">The receivable type of the payment plan</param>
        /// <param name="planAmount">The amount of the payment plan</param>
        /// <param name="eligibleCharges">The list of charges that are eligible for the payment plan</param>
        /// <param name="invoiceLookupTable">Lookup table of invoices and their receivable types</param>
        /// <param name="chargeCodes">Collection of all receivable charge codes</param>
        /// <returns>Collection of plan charges for the payment plan</returns>
        public List<PlanCharge> GetPlanCharges(string receivableType, decimal planAmount, PaymentPlanTemplate template, IEnumerable<Invoice> invoices, IEnumerable<ChargeCode> chargeCodes = null)
        {
            if ((_chargeCodes == null || !_chargeCodes.Any()) && chargeCodes != null)
            {
                _chargeCodes = chargeCodes.ToList();
            }

            // Build the invoice lookup table
            var invoiceLookupTable = BuildInvoiceLookupTable(invoices);
            var eligibleCharges = GetEligibleCharges(template, invoices);

            // We'll go through the eligible charges and apply as much is available - until we run out of money.
            List<PlanCharge> eligiblePlanCharges = new List<PlanCharge>();
            foreach (var item in eligibleCharges)
            {
                // Get the charge's receivable type
                string type;
                if (invoiceLookupTable.TryGetValue(item.InvoiceId, out type))
                {
                    // The receivable type has to match what's going on the plan.
                    if (type == receivableType)
                    {
                        if (planAmount >= item.Amount)
                        {
                            // The entire amount can be paid
                            eligiblePlanCharges.Add(BuildPlanCharge(item));
                            planAmount -= item.Amount;
                        }
                        else
                        {
                            // Take what's available
                            eligiblePlanCharges.Add(BuildPlanCharge(item, planAmount));
                            planAmount = 0m;
                            break;
                        }
                    }
                }
            }
            return eligiblePlanCharges;
        }

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
        public PaymentPlan GetProposedPlan(PaymentPlanTemplate template, string personId, string receivableType, string termId, decimal planAmount,
            DateTime firstPaymentDate, IEnumerable<PlanCharge> planCharges)
        {
            if (template == null)
            {
                throw new ArgumentNullException("template", "Payment Plan Template must be provided to retrieve proposed payment plan.");
            }
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "Person ID must be provided to retrieve proposed payment plan.");
            }
            if (string.IsNullOrEmpty(receivableType))
            {
                throw new ArgumentNullException("receivableType", "Receivable Type must be provided to retrieve proposed payment plan.");
            }
            if (string.IsNullOrEmpty(termId))
            {
                throw new ArgumentNullException("termId", "Term ID must be provided to retrieve proposed payment plan.");
            }
            if (planAmount <= 0)
            {
                throw new ArgumentOutOfRangeException("planAmount", "Plan amount must be greater than zero for proposed payment plan.");
            }

            var proposedPlan = new PaymentPlan(null, template.Id, personId, receivableType, termId, planAmount, firstPaymentDate, null, null, null)
            {
                CurrentAmount = planAmount,
                DownPaymentPercentage = template.DownPaymentPercentage,
                Frequency = template.Frequency,
                GraceDays = template.GraceDays,
                LateChargeAmount = template.LateChargeAmount,
                LateChargePercentage = template.LateChargePercentage,
                NumberOfPayments = template.NumberOfPayments,
                SetupAmount = template.SetupChargeAmount,
                SetupPercentage = template.SetupChargePercentage,
            };

            if (planCharges != null && planCharges.Count() > 0)
            {
                foreach (var planCharge in planCharges)
                {
                    proposedPlan.AddPlanCharge(planCharge);
                }
            }

            return proposedPlan;
        }

        /// <summary>
        /// Build the list of scheduled payments for the plan.
        /// </summary>
        /// <param name="template">Template to create the plan</param>
        /// <param name="proposedPlan">Plan under construction</param>
        /// <param name="downPaymentDate">Down Payment date</param>
        /// <param name="planDates">List of scheduled payment dates</param>
        /// <returns></returns>
        public void AddPlanSchedules(PaymentPlanTemplate template, PaymentPlan proposedPlan, List<DateTime?> planDates)
        {
            if (template == null)
            {
                throw new ArgumentNullException("template", "template cannot be null when building a plan schedule.");
            }
            if (proposedPlan == null)
            {
                throw new ArgumentNullException("proposedPlan", "proposed plan cannot be null when building a plan schedule.");
            }
            if (planDates == null || planDates.Count == 0)
            {
                throw new ArgumentNullException("planDates", "A proposed payment plan must have at least 1 plan date.");
            }
            // Verify that we have the correct number of payments defined
            if (proposedPlan.NumberOfPayments + 1 != planDates.Count)
            {
                throw new ArgumentOutOfRangeException("planDates", "Number of plan dates provided must match the number of payments defined on the plan.");
            }

            // Get the plan dates
            DateTime? downPaymentDate = planDates[0];
            List<DateTime> scheduleDates = new List<DateTime>();
            if (planDates.Count > 0)
            {
                scheduleDates.AddRange(planDates.GetRange(1, planDates.Count - 1).Select(x => x.Value).ToList());
            }

            // Determine the down payment amount, normal schedule amounts, and the amount of the first non-down payment payment
            decimal downPaymentAmount = template.CalculateDownPaymentAmount(proposedPlan.OriginalAmount);
            decimal firstPaymentAmount = template.CalculateFirstPaymentAmount(proposedPlan.OriginalAmount, proposedPlan.NumberOfPayments);
            decimal scheduleAmount = template.CalculateScheduledPaymentAmount(proposedPlan.OriginalAmount, proposedPlan.NumberOfPayments);

            // Now add the scheduled payments to the proposed plan
            // Add the down payment if one exists.  
            if (downPaymentDate.HasValue)
            {
                proposedPlan.AddScheduledPayment(new ScheduledPayment(null, null, downPaymentAmount, downPaymentDate.Value, 0, null));
            }

            // Add the first payment, since it may be different
            proposedPlan.AddScheduledPayment(new ScheduledPayment(null, null, firstPaymentAmount, scheduleDates[0], 0, null));

            // Add the other scheduled payments to the proposed plan
            for (int i = 1; i < proposedPlan.NumberOfPayments; i++)
            {
                proposedPlan.AddScheduledPayment(new ScheduledPayment(null, null, scheduleAmount, scheduleDates[i], 0, null));
            }
        }

        /// <summary>
        /// Get the list of charges that are eligible for a plan using a specified template
        /// </summary>
        /// <param name="template"></param>
        /// <param name="invoices"></param>
        /// <param name="loggingEnabled"></param>
        /// <returns></returns>
        private List<Charge> GetEligibleCharges(PaymentPlanTemplate template, IEnumerable<Invoice> invoices)
        {
            // Filter out registration invoices that are not plan-eligible
            var filteredInvoices = FilterInvoices(template, invoices);

            // Check that there are plan-eligible invoices after filtering; if not, then the student does not have the payment plan option
            if (filteredInvoices == null || filteredInvoices.Count() == 0)
            {
                return new List<Charge>();
            }

            // Build a collection of eligible charges
            List<Charge> eligibleCharges = filteredInvoices.SelectMany(inv => inv.Charges).Where(x => ValidateChargeCode(x.Code, template))
                .OrderBy(x => GetChargePriority(x.Code)).ToList();

            return eligibleCharges;
        }

        /// <summary>
        /// Build a lookup table of invoice receivable types by invoice ID
        /// </summary>
        /// <param name="invoices">List of invoices</param>
        /// <returns>Invoice lookup table</returns>
        private Dictionary<string, string> BuildInvoiceLookupTable(IEnumerable<Invoice> invoices)
        {
            return invoices.ToDictionary(x => x.Id, y => y.ReceivableTypeCode);
        }

        /// <summary>
        /// Apply charge amounts to payment amounts
        /// </summary>
        /// <param name="paymentsByReceivableType">Table of payment receivables by receivable type</param>
        /// <param name="chargesByReceivableType">Table of charged receivables by receivable type</param>
        private void ApplyChargesToPayments(Dictionary<string, decimal> paymentsByReceivableType, Dictionary<string, decimal> chargesByReceivableType)
        {
            // If there are no payments, initialize the payments table
            if (paymentsByReceivableType == null)
            {
                paymentsByReceivableType = new Dictionary<string, decimal>();
            }
            // If there are no charges, then there's nothing more to do
            if (chargesByReceivableType == null || chargesByReceivableType.Count == 0)
            {
                return;
            }
            // Process each receivable type separately
            foreach (string receivableType in chargesByReceivableType.Keys)
            {
                // If this receivables type isn't in the payments table, add it
                if (!paymentsByReceivableType.Keys.Contains(receivableType))
                {
                    paymentsByReceivableType.Add(receivableType, 0m);
                }
                // Add the charges to the payments to accumulate a net amount
                paymentsByReceivableType[receivableType] += chargesByReceivableType[receivableType];
            }
        }

        /// <summary>
        /// Determine whether a charge code is eligible for the specified pay plan template
        /// </summary>
        /// <param name="chargeCode">Charge code</param>
        /// <param name="template">Payment plan template</param>
        /// <returns>True/false value indicating whether the charge is eligible</returns>
        private bool ValidateChargeCode(string chargeCode, PaymentPlanTemplate template)
        {
            if (template.IncludedChargeCodes != null && template.IncludedChargeCodes.Count > 0)
            {
                return template.IncludedChargeCodes.Contains(chargeCode);
            }
            if (template.ExcludedChargeCodes != null && template.ExcludedChargeCodes.Count > 0)
            {
                return !template.ExcludedChargeCodes.Contains(chargeCode);
            }
            return true;
        }

        /// <summary>
        /// Get the payment priority for a given charge
        /// </summary>
        /// <param name="chargeCode">The charge code on the charge</param>
        /// <returns>The priority of the charge</returns>
        private int GetChargePriority(string chargeCode)
        {
            if (string.IsNullOrEmpty(chargeCode))
            {
                throw new ArgumentNullException("chargeCode", "Charge code must be specified.");
            }

            var charge = _chargeCodes.FirstOrDefault(x => x.Code == chargeCode);

            return charge == null ? 999 : charge.Priority;
        }

        /// <summary>
        /// Builds a PlanCharge entity from a Charge entity for a specific amount
        /// </summary>
        /// <param name="charge">Charge item</param>
        /// <param name="amount">Amount of charge to put on the payment plan</param>
        /// <returns>A PlanCharge item</returns>
        private PlanCharge BuildPlanCharge(Charge charge, decimal amount)
        {
            if (charge == null)
            {
                throw new ArgumentNullException("charge", "Charge must be provided.");
            }
            // If the entire charge amount is going on the plan, it can be modified automatically
            return new PlanCharge(null, charge, amount, false, (amount == charge.Amount));
        }

        /// <summary>
        /// Builds a PlanCharge entity from a Charge entity for the entire amount of the charge
        /// </summary>
        /// <param name="charge">Charge item</param>
        /// <returns>A PlanCharge item</returns>
        private PlanCharge BuildPlanCharge(Charge charge)
        {
            return BuildPlanCharge(charge, charge.Amount);
        }
    }
}
