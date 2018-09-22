// Copyright 2014-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Finance.Entities;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Finance.Entities.Configuration;

namespace Ellucian.Colleague.Domain.Finance.Services
{
    /// <summary>
    /// Helper class that assists in servicing requests regarding ReceivableTransactions
    /// </summary>
    public static class ReceivableService 
    {
        /// <summary>
        /// Validates that the invoice conforms to business rules
        /// </summary>
        /// <param name="invoice">invoice to validate</param>
        /// <param name="receivableTypes">list of valid receivable types</param>
        /// <param name="chargeCodes">list of valid charge codes</param>
        /// <param name="invoiceTypes">list of valid invoice types</param>
        /// <param name="terms">list of valid terms</param>
        public static void ValidateInvoice(ReceivableInvoice invoice, IEnumerable<ReceivableType> receivableTypes, IEnumerable<ChargeCode> chargeCodes,
            IEnumerable<InvoiceType> invoiceTypes, IEnumerable<Term> terms)
        {
            if (invoice == null)
            {
                throw new ArgumentNullException("invoice", "Invoice Id must be specified.");
            }

            if (receivableTypes == null || !receivableTypes.Any())
            {
                throw new ArgumentNullException("receivableTypes", "Receivable types must be specified.");
            }

            if (chargeCodes == null || !chargeCodes.Any())
            {
                throw new ArgumentNullException("chargeCodes", "Charge codes must be specified.");
            }

            if (invoiceTypes == null || !invoiceTypes.Any())
            {
                throw new ArgumentNullException("invoiceTypes", "Invoice types must be specified.");
            }

            if (terms == null || !terms.Any())
            {
                throw new ArgumentNullException("terms", "Terms must be specified.");
            }

            //validate that the invoice's AR Code, AR Type, and Invoice Type are in chargeCodes, receivableTypes, and invoiceTypes respectively
            var receivableType = receivableTypes.FirstOrDefault(x => x.Code == invoice.ReceivableType);
            if (receivableType == null)
            {
                throw new ApplicationException("Invalid receivable type: " + invoice.ReceivableType);
            }

            // Invoice type is not required
            if (!string.IsNullOrEmpty(invoice.InvoiceType))
            {
                var invoiceType = invoiceTypes.FirstOrDefault(x => x.Code == invoice.InvoiceType);
                if (invoiceType == null)
                {
                    throw new ApplicationException("Invalid invoice type: " + invoice.InvoiceType);
                }
            }

            foreach (var charge in invoice.Charges)
            {
                var chargeCode = chargeCodes.FirstOrDefault(x => x.Code == charge.Code);
                if (chargeCode == null)
                {
                    throw new ApplicationException("Invalid charge code: " + charge.Code);
                }
            }

            // term is not required
            if (!string.IsNullOrEmpty(invoice.TermId))
            {
                var term = terms.FirstOrDefault(x => x.Code == invoice.TermId);
                if (term == null)
                {
                    throw new ApplicationException("Invalid term: " + invoice.TermId);
                }
            }
        }

        /// <summary>
        /// Validate that the deposit conforms to business rules
        /// </summary>
        /// <param name="deposit">The deposit to validate</param>
        /// <param name="types">List of valid Deposit Types</param>
        /// <param name="systems">List of valid external systems</param>
        /// <param name="terms">List of valid terms</param>
        public static void ValidateDeposit(Deposit deposit, IEnumerable<DepositType> types, IEnumerable<ExternalSystem> systems, IEnumerable<Term> terms)
        {
            if (deposit == null)
            {
                throw new ArgumentNullException("deposit", "Deposit must be specified.");
            }

            if (types == null || types.Count() == 0)
            {
                throw new ArgumentNullException("types", "Deposit types must be specified.");
            }
            // validate deposit type
            var deptype = types.FirstOrDefault(x => (x.Code == deposit.DepositType));
            if (deptype == null)
            {
                throw new ApplicationException("Invalid deposit type " + deposit.DepositType);
            }

            if (!string.IsNullOrEmpty(deposit.ExternalSystem))
            {
                var extSys = systems.FirstOrDefault(x => (x.Code == deposit.ExternalSystem));
                if (extSys == null)
                {
                    throw new ApplicationException("Invalid external system " + deposit.ExternalSystem);
                }
            }

            if (!string.IsNullOrEmpty(deposit.TermId))
            {
                var term = terms.FirstOrDefault(x => (x.Code == deposit.TermId));
                if (term == null)
                {
                    throw new ApplicationException("Invalid term " + deposit.TermId);
                }
            }
        }

        /// <summary>
        /// Given a collection of billing terms with payment plan information and the student finance configuration data,
        /// identify just the billing terms that have payable receivable types
        /// </summary>
        /// <param name="billingTerms">Collection of <see cref="BillingTermPaymentPlanInformation"/> objects.</param>
        /// <param name="configuration"><see cref="FinanceConfiguration"/> object.</param>
        /// <returns>Collection of billing terms with receivable types that are payable</returns>
        public static IEnumerable<BillingTermPaymentPlanInformation> RemoveBillingTermPaymentPlanInformationWithNonPayableReceivableType
            (IEnumerable<BillingTermPaymentPlanInformation> billingTerms, FinanceConfiguration configuration)
        {
            if (billingTerms == null || !billingTerms.Any())
            {
                throw new ArgumentNullException("billingTerms", "Billing term payment plan information for at least one term must be specified.");
            }
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration", "Finance configuration cannot be null.");
            }

            // Determine if any displayed receivable types are defined in finance configuration
            if (configuration.DisplayedReceivableTypes != null && configuration.DisplayedReceivableTypes.Any())
            {
                // Identify the list of payable receivable types 
                var payableReceivableTypes = configuration.DisplayedReceivableTypes.Where(drt => drt.IsPayable).Select(drt => drt.Code).Distinct().ToList();
                
                // Initialize list for subset of billing terms with payable receivable types
                var billingTermsWithPayableReceivableTypes = new List<BillingTermPaymentPlanInformation>();
                
                // If there are any payable receivable types, evaluate each billing term to determine its payability
                if (payableReceivableTypes.Any())
                {
                    foreach(var billingTerm in billingTerms)
                    {
                        // Determine if billing term receivable type is a payable receivable type
                        if (payableReceivableTypes.Contains(billingTerm.ReceivableTypeCode))
                        {
                            // Add billing term to subset of billing terms with payable receivable types 
                            billingTermsWithPayableReceivableTypes.Add(billingTerm);
                        }
                    }
                }

                // Return the list of billing terms with payable receivable types
                // This is either a subset of the original list of billing terms OR an empty list if there are no payable receivable types
                return billingTermsWithPayableReceivableTypes;
            } 
            else
            {
                // No displayed/payable receivable types explicitly stated in finance configuration; 
                // all receivable types are payable, so return the original list of billing terms; 
                return billingTerms;
            }
        }

        /// <summary>
        /// Gets the payment plan option for the payment requirement for the student, if applicable, for the current date
        /// </summary>
        /// <param name="applicablePaymentRequirement">The payment requirement that is applicable for the account holder</param>
        public static PaymentPlanOption GetApplicablePaymentPlanOption(PaymentRequirement applicablePaymentRequirement)
        {
            // Find the payment plan option whose date range contains the current date and return its associated payment plan template ID
            if (applicablePaymentRequirement != null && applicablePaymentRequirement.PaymentPlanOptions != null && applicablePaymentRequirement.PaymentPlanOptions.Count() > 0)
            {
                foreach (var payPlanOption in applicablePaymentRequirement.PaymentPlanOptions)
                {
                    // Check if today is between the start and end dates
                    if (DateTime.Today.Date >= payPlanOption.EffectiveStart.Date && DateTime.Today.Date <= payPlanOption.EffectiveEnd.Date)
                    {
                        return payPlanOption;
                    }
                }
            }
            return null;
        }
    }
}
